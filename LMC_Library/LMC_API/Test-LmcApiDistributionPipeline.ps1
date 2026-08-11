[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$implementation = Join-Path $PSScriptRoot 'DistributionPipeline.ps1'
if (-not (Test-Path -LiteralPath $implementation -PathType Leaf)) {
    throw "Distribution pipeline implementation not found: $implementation"
}
. $implementation

$script:Passed = 0
$script:TrackedReparsePaths = New-Object `
    'System.Collections.Generic.List[string]'

function Assert-True {
    param(
        [Parameter(Mandatory = $true)]
        [bool]$Condition,
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
    $script:Passed += 1
}

function Assert-Equal {
    param(
        [AllowNull()]
        [object]$Expected,
        [AllowNull()]
        [object]$Actual,
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if (-not [object]::Equals($Expected, $Actual)) {
        throw "$Message expected='$Expected' actual='$Actual'"
    }
    $script:Passed += 1
}

function Assert-Throws {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action,
        [Parameter(Mandatory = $true)]
        [string]$ExpectedMessage
    )

    $caught = $null
    try {
        & $Action | Out-Null
    }
    catch {
        $caught = $_.Exception.Message
    }
    if ($null -eq $caught) {
        throw "Expected an exception containing: $ExpectedMessage"
    }
    if ($caught.IndexOf(
        $ExpectedMessage,
        [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Unexpected exception. expected='$ExpectedMessage' actual='$caught'"
    }
    $script:Passed += 1
    return $caught
}

function Write-TestFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Content
    )

    $parent = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $parent -PathType Container)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    [System.IO.File]::WriteAllBytes(
        $Path,
        [System.Text.Encoding]::ASCII.GetBytes($Content))
}

function New-TestFixture {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $fixtureRoot = Join-Path $script:TestRoot $Name
    $canonical = Join-Path $fixtureRoot 'LMC_API_Distribution'
    New-Item -ItemType Directory -Path (
        Join-Path $canonical '01_API') -Force | Out-Null
    New-Item -ItemType Directory -Path (
        Join-Path $canonical '02_Example_Program/EmptyDirectory') `
        -Force | Out-Null
    Write-TestFile `
        -Path (Join-Path $canonical '01_API/LasalMotionControlLib.dll') `
        -Content 'canonical-dll-v1'
    Write-TestFile `
        -Path (Join-Path $canonical 'README.md') `
        -Content 'canonical-readme-v1'
    Write-TestFile `
        -Path (Join-Path $canonical '.hidden-fixture') `
        -Content 'hidden-canonical-input'

    return [pscustomobject]@{
        Root = $fixtureRoot
        Canonical = $canonical
        Parent = $fixtureRoot
        Candidate = Join-Path $fixtureRoot (
            'LMC_API_Distribution_candidate_' + $Name)
    }
}

function Populate-TestCandidate {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Stage,
        [string]$Marker = 'candidate-v1'
    )

    New-Item -ItemType Directory -Path (
        Join-Path $Stage '01_API') -Force | Out-Null
    New-Item -ItemType Directory -Path (
        Join-Path $Stage '02_Example_Program/EmptyDirectory') `
        -Force | Out-Null
    Write-TestFile `
        -Path (Join-Path $Stage '01_API/LasalMotionControlLib.dll') `
        -Content $Marker
    Write-TestFile `
        -Path (Join-Path $Stage 'README.md') `
        -Content 'candidate-readme'
}

function Get-StagingDirectories {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Parent
    )

    return @(
        Get-ChildItem -LiteralPath $Parent -Directory -Force |
            Where-Object {
                $_.Name -like '.LMC_API_Distribution.stage.*'
            }
    )
}

function Assert-NoTransactionResidue {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Fixture,
        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    Assert-Equal `
        -Expected 0 `
        -Actual (@(Get-StagingDirectories -Parent $Fixture.Parent).Count) `
        -Message "$Context left a staging directory."
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath (
            Join-Path $Fixture.Parent `
                '.LMC_API_Distribution.transaction.lock'))) `
        -Message "$Context left the transaction lock file behind."
}

function Assert-CanonicalUnchanged {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Before,
        [Parameter(Mandatory = $true)]
        [string]$Canonical,
        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    $after = Get-LmcDistributionTreeSnapshot -Root $Canonical
    Assert-Equal `
        -Expected $Before.Sha256 `
        -Actual $after.Sha256 `
        -Message "$Context changed the canonical package."
}

function Remove-TestRootSafely {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    foreach ($reparsePath in @($script:TrackedReparsePaths)) {
        if (-not (Test-Path -LiteralPath $reparsePath)) {
            continue
        }
        $item = Get-Item -LiteralPath $reparsePath -Force
        $fullReparsePath = [System.IO.Path]::GetFullPath($item.FullName)
        $fullTestRoot = [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
        if (-not $fullReparsePath.StartsWith(
            $fullTestRoot + '\',
            [System.StringComparison]::OrdinalIgnoreCase) -or
            -not (Test-LmcDistributionReparsePoint -Item $item)) {
            throw "Refusing test cleanup for an unexpected path: $fullReparsePath"
        }
        [System.IO.Directory]::Delete($fullReparsePath)
    }

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }
    $fullPath = [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
    $tempRoot = [System.IO.Path]::GetFullPath(
        [System.IO.Path]::GetTempPath()).TrimEnd('\')
    if (-not $fullPath.StartsWith(
        $tempRoot + '\',
        [System.StringComparison]::OrdinalIgnoreCase) -or
        [System.IO.Path]::GetFileName($fullPath) -notmatch
            '^LmcDistributionPipelineTest-[0-9a-f]{32}$') {
        throw "Refusing to remove an unsafe test root: $fullPath"
    }
    Assert-LmcDistributionTreeHasNoReparsePoints `
        -Root $fullPath `
        -Context 'Test cleanup tree'
    [System.IO.Directory]::Delete($fullPath, $true)
}

$systemTemp = [System.IO.Path]::GetFullPath(
    [System.IO.Path]::GetTempPath()).TrimEnd('\')
$script:TestRoot = Join-Path $systemTemp (
    'LmcDistributionPipelineTest-' + [System.Guid]::NewGuid().ToString('N'))

try {
    New-Item -ItemType Directory -Path $script:TestRoot | Out-Null

    # Manual inputs default to canonical files or accept one explicit pair.
    $manualFixtureRoot = Join-Path $script:TestRoot 'manual-inputs'
    $manualCanonicalRoot = Join-Path $manualFixtureRoot `
        'LMC_API_Distribution/03_API_User_Manual'
    $manualAuthoringRoot = Join-Path $manualFixtureRoot 'output'
    $canonicalPdf = Join-Path $manualCanonicalRoot 'manual.pdf'
    $canonicalDocx = Join-Path $manualCanonicalRoot 'manual.docx'
    $candidatePdf = Join-Path $manualAuthoringRoot 'candidate.pdf'
    $candidateDocx = Join-Path $manualAuthoringRoot 'candidate.docx'
    Write-TestFile -Path $canonicalPdf -Content 'canonical-pdf'
    Write-TestFile -Path $canonicalDocx -Content 'canonical-docx'
    Write-TestFile -Path $candidatePdf -Content 'candidate-pdf'
    Write-TestFile -Path $candidateDocx -Content 'candidate-docx'

    $manualInputs = Resolve-LmcDistributionManualInputs `
        -RepositoryRoot $manualFixtureRoot `
        -CanonicalPdfPath $canonicalPdf `
        -CanonicalDocxPath $canonicalDocx
    Assert-Equal `
        -Expected ([System.IO.Path]::GetFullPath($canonicalPdf)) `
        -Actual $manualInputs.PdfPath `
        -Message 'Default manual PDF input was not canonical.'
    Assert-Equal `
        -Expected ([System.IO.Path]::GetFullPath($canonicalDocx)) `
        -Actual $manualInputs.DocxPath `
        -Message 'Default manual DOCX input was not canonical.'
    Assert-True `
        -Condition $manualInputs.UsesCanonicalInputs `
        -Message 'Default manual inputs were not marked canonical.'

    $manualInputs = Resolve-LmcDistributionManualInputs `
        -RepositoryRoot $manualFixtureRoot `
        -CanonicalPdfPath $canonicalPdf `
        -CanonicalDocxPath $canonicalDocx `
        -ManualPdfPath $candidatePdf `
        -ManualDocxPath $candidateDocx
    Assert-Equal `
        -Expected ([System.IO.Path]::GetFullPath($candidatePdf)) `
        -Actual $manualInputs.PdfPath `
        -Message 'Explicit manual PDF input was not selected.'
    Assert-Equal `
        -Expected ([System.IO.Path]::GetFullPath($candidateDocx)) `
        -Actual $manualInputs.DocxPath `
        -Message 'Explicit manual DOCX input was not selected.'
    Assert-True `
        -Condition (-not $manualInputs.UsesCanonicalInputs) `
        -Message 'Explicit manual inputs were marked canonical.'

    Assert-Throws `
        -Action {
            Resolve-LmcDistributionManualInputs `
                -RepositoryRoot $manualFixtureRoot `
                -CanonicalPdfPath $canonicalPdf `
                -CanonicalDocxPath $canonicalDocx `
                -ManualPdfPath $candidatePdf
        } `
        -ExpectedMessage 'must be supplied together' | Out-Null
    Assert-Throws `
        -Action {
            Resolve-LmcDistributionManualInputs `
                -RepositoryRoot $manualFixtureRoot `
                -CanonicalPdfPath $canonicalPdf `
                -CanonicalDocxPath $canonicalDocx `
                -ManualPdfPath (Join-Path $manualAuthoringRoot 'missing.pdf') `
                -ManualDocxPath $candidateDocx
        } `
        -ExpectedMessage 'Manual PDF input was not found' | Out-Null
    Assert-Throws `
        -Action {
            Resolve-LmcDistributionManualInputs `
                -RepositoryRoot $manualFixtureRoot `
                -CanonicalPdfPath $canonicalPdf `
                -CanonicalDocxPath $canonicalDocx `
                -ManualPdfPath $candidateDocx `
                -ManualDocxPath $candidateDocx
        } `
        -ExpectedMessage 'Manual PDF input must use the .pdf extension' |
        Out-Null

    $outsidePdf = Join-Path $script:TestRoot 'outside-manual.pdf'
    Write-TestFile -Path $outsidePdf -Content 'outside-pdf'
    Assert-Throws `
        -Action {
            Resolve-LmcDistributionManualInputs `
                -RepositoryRoot $manualFixtureRoot `
                -CanonicalPdfPath $canonicalPdf `
                -CanonicalDocxPath $canonicalDocx `
                -ManualPdfPath $outsidePdf `
                -ManualDocxPath $candidateDocx
        } `
        -ExpectedMessage 'Manual PDF input escaped the repository' | Out-Null

    $manualTarget = Join-Path $manualFixtureRoot 'manual-target'
    $manualLink = Join-Path $manualFixtureRoot 'manual-link'
    $linkedPdf = Join-Path $manualLink 'linked.pdf'
    $linkedDocx = Join-Path $manualLink 'linked.docx'
    Write-TestFile `
        -Path (Join-Path $manualTarget 'linked.pdf') `
        -Content 'linked-pdf'
    Write-TestFile `
        -Path (Join-Path $manualTarget 'linked.docx') `
        -Content 'linked-docx'
    New-Item -ItemType Junction `
        -Path $manualLink `
        -Target $manualTarget | Out-Null
    $script:TrackedReparsePaths.Add($manualLink)
    Assert-Throws `
        -Action {
            Resolve-LmcDistributionManualInputs `
                -RepositoryRoot $manualFixtureRoot `
                -CanonicalPdfPath $canonicalPdf `
                -CanonicalDocxPath $canonicalDocx `
                -ManualPdfPath $linkedPdf `
                -ManualDocxPath $linkedDocx
        } `
        -ExpectedMessage 'traverses a reparse point' | Out-Null
    [System.IO.Directory]::Delete($manualLink)

    Assert-Equal `
        -Expected 'clean' `
        -Actual (Get-LmcDistributionManualWorktreeState `
            -UsesCanonicalInputs $true `
            -WorktreeState 'clean') `
        -Message 'Canonical manual inputs changed a clean worktree state.'
    Assert-Throws `
        -Action {
            Get-LmcDistributionManualWorktreeState `
                -UsesCanonicalInputs $false `
                -WorktreeState 'clean'
        } `
        -ExpectedMessage 'Noncanonical manual inputs require -AllowDirty' |
        Out-Null
    Assert-Equal `
        -Expected 'dirty-preview' `
        -Actual (Get-LmcDistributionManualWorktreeState `
            -UsesCanonicalInputs $false `
            -WorktreeState 'clean' `
            -AllowDirty) `
        -Message 'Noncanonical manual inputs were not forced dirty-preview.'

    $candidatePdfHash = (Get-FileHash `
        -LiteralPath $candidatePdf -Algorithm SHA256).Hash
    $candidateDocxHash = (Get-FileHash `
        -LiteralPath $candidateDocx -Algorithm SHA256).Hash
    $manualSnapshot = New-LmcDistributionManualInputSnapshot `
        -RepositoryRoot $manualFixtureRoot `
        -PdfPath $candidatePdf `
        -DocxPath $candidateDocx
    Assert-True `
        -Condition ($manualSnapshot.PdfBytes -is [byte[]] -and
            $manualSnapshot.DocxBytes -is [byte[]]) `
        -Message 'Manual input snapshot did not retain byte arrays.'
    Assert-Equal `
        -Expected $candidatePdfHash `
        -Actual $manualSnapshot.PdfSha256 `
        -Message 'Manual PDF snapshot hash does not match the source.'
    Assert-Equal `
        -Expected $candidateDocxHash `
        -Actual $manualSnapshot.DocxSha256 `
        -Message 'Manual DOCX snapshot hash does not match the source.'
    Assert-Equal `
        -Expected ([long](Get-Item -LiteralPath $candidatePdf).Length) `
        -Actual $manualSnapshot.PdfLength `
        -Message 'Manual PDF snapshot length does not match the source.'
    Assert-Equal `
        -Expected ([long](Get-Item -LiteralPath $candidateDocx).Length) `
        -Actual $manualSnapshot.DocxLength `
        -Message 'Manual DOCX snapshot length does not match the source.'
    Write-TestFile -Path $candidatePdf -Content 'candidate-pdf-mutated'
    Assert-Equal `
        -Expected $candidatePdfHash `
        -Actual (Get-LmcDistributionBytesSha256 `
            -Bytes $manualSnapshot.PdfBytes) `
        -Message 'Manual PDF snapshot changed with the original file.'

    # The release builder must keep the exact Run copy/gate/manifest/final
    # identity order inside the candidate transaction callback.
    $builderPath = Join-Path $PSScriptRoot 'Build-LmcApiDistribution.ps1'
    $builderTokens = $null
    $builderParseErrors = $null
    $builderAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $builderPath,
        [ref]$builderTokens,
        [ref]$builderParseErrors)
    Assert-Equal `
        -Expected 0 `
        -Actual @($builderParseErrors).Count `
        -Message 'Release builder has PowerShell parser errors.'
    $builderCommands = @($builderAst.FindAll(
        {
            param($node)
            $node -is [System.Management.Automation.Language.CommandAst]
        },
        $true))
    $transactionCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq
                'Invoke-LmcDistributionCandidateTransaction'
        })
    $runCopyCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq 'Copy-Item' -and
            $_.Extent.Text -match
                '(?is)-LiteralPath\s+\$exampleExe\b.*?-Destination\s+\$runDirectory\b'
        })
    $gateCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq
                'Invoke-LmcDistributionExecutableRelaunchGate'
        })
    $semanticCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq 'Test-LmcDistributionSemanticPolicy'
        })
    $manifestCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq 'Write-LmcReleaseManifestAtomic'
        })
    $finalIdentityCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq
                'Assert-LmcDistributionExecutableRelaunchIdentity'
        })
    foreach ($commandInventory in @(
        [pscustomobject]@{
            Name = 'candidate transaction'
            Commands = $transactionCommands
        },
        [pscustomobject]@{
            Name = 'Run EXE copy'
            Commands = $runCopyCommands
        },
        [pscustomobject]@{
            Name = 'executable relaunch gate helper'
            Commands = $gateCommands
        },
        [pscustomobject]@{
            Name = 'semantic policy'
            Commands = $semanticCommands
        },
        [pscustomobject]@{
            Name = 'manifest writer'
            Commands = $manifestCommands
        },
        [pscustomobject]@{
            Name = 'final executable identity helper'
            Commands = $finalIdentityCommands
        })) {
        Assert-Equal `
            -Expected 1 `
            -Actual @($commandInventory.Commands).Count `
            -Message (
                "Release builder $($commandInventory.Name) command count drifted.")
    }
    $transactionCommand = $transactionCommands[0]
    $runCopyCommand = $runCopyCommands[0]
    $gateCommand = $gateCommands[0]
    $semanticCommand = $semanticCommands[0]
    $manifestCommand = $manifestCommands[0]
    $finalIdentityCommand = $finalIdentityCommands[0]
    Assert-True `
        -Condition (
            $transactionCommand.Extent.StartOffset -lt
                $runCopyCommand.Extent.StartOffset -and
            $runCopyCommand.Extent.EndOffset -lt
                $gateCommand.Extent.StartOffset -and
            $gateCommand.Extent.EndOffset -lt
                $semanticCommand.Extent.StartOffset -and
            $semanticCommand.Extent.EndOffset -lt
                $manifestCommand.Extent.StartOffset -and
            $manifestCommand.Extent.EndOffset -lt
                $finalIdentityCommand.Extent.StartOffset -and
            $finalIdentityCommand.Extent.EndOffset -lt
                $transactionCommand.Extent.EndOffset) `
        -Message (
            'Release builder order must remain transaction -> Run copy -> ' +
            'EXE gate -> semantic policy -> manifest -> final EXE identity ' +
            '-> transaction completion.')
    Assert-True `
        -Condition ($gateCommand.Extent.Text -match (
            '(?is)-StagingRoot\s+\$stagingRoot\b.*?' +
            '-ExecutablePath\s+\$runExampleExe\b.*?' +
            '-GateAction\s*\{.*?' +
            "-Target\s+'RunWpfExecutableRelaunchTest'.*?" +
            'WpfExecutableRelaunchExe\s*=\s*\$testedExecutable\b')) `
        -Message (
            'Release builder gate helper no longer invokes the actual-EXE ' +
            'relaunch target with the exact staged Run path.')
    Assert-True `
        -Condition ($finalIdentityCommand.Extent.Text -match (
            '(?is)-StagingRoot\s+\$stagingRoot\b.*?' +
            '-ExecutablePath\s+\$runExampleExe\b.*?' +
            '-TestedSha256\s*\(\s*' +
            '\$buildSummary\.ExecutableRelaunchTestedExeSha256\s*\)')) `
        -Message (
            'Release builder final identity helper is not bound to the exact ' +
            'staged Run path and tested SHA.')

    # The helper executes only the exact staged Run EXE and binds its bytes.
    $gateRoot = Join-Path $script:TestRoot 'executable-relaunch-helper'
    $gateExecutable = Join-Path $gateRoot (
        '02_Example_Program/Run/LasalMotionControlApiExample.exe')
    Write-TestFile -Path $gateExecutable -Content 'tested-executable-v1'
    $expectedGateHash = (Get-FileHash `
        -LiteralPath $gateExecutable `
        -Algorithm SHA256).Hash.ToUpperInvariant()
    $gateObservation = [pscustomobject]@{
        Calls = 0
        Path = $null
    }
    $testedGateHash = Invoke-LmcDistributionExecutableRelaunchGate `
        -StagingRoot $gateRoot `
        -ExecutablePath $gateExecutable `
        -GateAction {
            param($testedExecutable)
            $gateObservation.Calls += 1
            $gateObservation.Path = $testedExecutable
            'gate-output-must-not-escape-the-helper'
        }
    Assert-Equal `
        -Expected $expectedGateHash `
        -Actual $testedGateHash `
        -Message 'Executable relaunch gate did not return the exact tested SHA.'
    Assert-Equal `
        -Expected 1 `
        -Actual $gateObservation.Calls `
        -Message 'Executable relaunch gate action did not run exactly once.'
    Assert-Equal `
        -Expected ([System.IO.Path]::GetFullPath($gateExecutable)) `
        -Actual $gateObservation.Path `
        -Message 'Executable relaunch gate action received the wrong EXE path.'
    Assert-Equal `
        -Expected $expectedGateHash `
        -Actual (Assert-LmcDistributionExecutableRelaunchIdentity `
            -StagingRoot $gateRoot `
            -ExecutablePath $gateExecutable `
            -TestedSha256 $testedGateHash) `
        -Message 'Final executable identity did not return the exact SHA.'

    Assert-Throws `
        -Action {
            Invoke-LmcDistributionExecutableRelaunchGate `
                -StagingRoot $gateRoot `
                -ExecutablePath $gateExecutable `
                -GateAction {
                    throw 'fixture executable relaunch gate failed'
                }
        } `
        -ExpectedMessage 'fixture executable relaunch gate failed' | Out-Null

    $missingGateRoot = Join-Path $script:TestRoot `
        'executable-relaunch-missing'
    New-Item -ItemType Directory -Path $missingGateRoot -Force | Out-Null
    $missingGateExecutable = Join-Path $missingGateRoot (
        '02_Example_Program/Run/LasalMotionControlApiExample.exe')
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionExecutableRelaunchGate `
                -StagingRoot $missingGateRoot `
                -ExecutablePath $missingGateExecutable `
                -GateAction { }
        } `
        -ExpectedMessage 'Executable relaunch gate input was not found' |
        Out-Null

    $wrongGateExecutable = Join-Path $gateRoot (
        '02_Example_Program/Run/WrongExample.exe')
    Write-TestFile -Path $wrongGateExecutable -Content 'wrong-executable'
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionExecutableRelaunchGate `
                -StagingRoot $gateRoot `
                -ExecutablePath $wrongGateExecutable `
                -GateAction { }
        } `
        -ExpectedMessage 'path must be the exact staged Run EXE' | Out-Null

    Write-TestFile -Path $gateExecutable -Content 'mutated-after-gate'
    Assert-Throws `
        -Action {
            Assert-LmcDistributionExecutableRelaunchIdentity `
                -StagingRoot $gateRoot `
                -ExecutablePath $gateExecutable `
                -TestedSha256 $testedGateHash
        } `
        -ExpectedMessage (
            'final example EXE bytes do not match the executable relaunch gate input') |
        Out-Null

    Write-TestFile -Path $gateExecutable -Content 'tested-executable-v1'
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionExecutableRelaunchGate `
                -StagingRoot $gateRoot `
                -ExecutablePath $gateExecutable `
                -GateAction {
                    param($testedExecutable)
                    Write-TestFile `
                        -Path $testedExecutable `
                        -Content 'mutated-during-gate'
                }
        } `
        -ExpectedMessage (
            'staged example EXE changed while the executable relaunch gate ran') |
        Out-Null

    # Snapshot coverage includes file content, hidden files, and empty directories.
    $fixture = New-TestFixture -Name 'snapshot'
    $snapshotBaseline = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $extraEmptyDirectory = Join-Path $fixture.Canonical 'new-empty-directory'
    New-Item -ItemType Directory -Path $extraEmptyDirectory | Out-Null
    $snapshotWithDirectory = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-True `
        -Condition ($snapshotBaseline.Sha256 -ne $snapshotWithDirectory.Sha256) `
        -Message 'An empty directory did not change the tree snapshot.'
    [System.IO.Directory]::Delete($extraEmptyDirectory)
    Assert-CanonicalUnchanged `
        -Before $snapshotBaseline `
        -Canonical $fixture.Canonical `
        -Context 'Snapshot directory restoration'

    $hiddenPath = Join-Path $fixture.Canonical '.hidden-fixture'
    Write-TestFile -Path $hiddenPath -Content 'hidden-canonical-input-mutated'
    $snapshotWithHiddenChange = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-True `
        -Condition ($snapshotBaseline.Sha256 -ne $snapshotWithHiddenChange.Sha256) `
        -Message 'A hidden file change did not change the tree snapshot.'
    Write-TestFile -Path $hiddenPath -Content 'hidden-canonical-input'
    Assert-CanonicalUnchanged `
        -Before $snapshotBaseline `
        -Canonical $fixture.Canonical `
        -Context 'Snapshot hidden-file restoration'

    # Successful promotion seals a staging tree, renames once, and preserves canonical.
    $fixture = New-TestFixture -Name 'success'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $fingerprintState = [pscustomobject]@{
        Value = 'input-v1'
        Calls = 0
    }
    $result = Invoke-LmcDistributionCandidateTransaction `
        -CanonicalRoot $fixture.Canonical `
        -CandidatePath $fixture.Candidate `
        -PopulateAndValidate {
            param($stage)
            Populate-TestCandidate -Stage $stage
        } `
        -GetInputFingerprint {
            $fingerprintState.Calls += 1
            $fingerprintState.Value
        }
    Assert-True `
        -Condition $result.Committed `
        -Message 'Successful transaction did not report Committed=True.'
    Assert-True `
        -Condition (Test-Path -LiteralPath $fixture.Candidate -PathType Container) `
        -Message 'Successful transaction did not publish the candidate directory.'
    Assert-Equal `
        -Expected 2 `
        -Actual $fingerprintState.Calls `
        -Message 'Successful transaction did not check the input fingerprint twice.'
    $publishedSnapshot = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Candidate
    Assert-Equal `
        -Expected $result.CandidateSnapshotSha256 `
        -Actual $publishedSnapshot.Sha256 `
        -Message 'Published candidate does not match its sealed snapshot.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Successful transaction'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Successful transaction'

    # Prepared inputs bind the transaction baseline and populate callback.
    $fixture = New-TestFixture -Name 'prepared_inputs'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $preparedFixtureInput = [pscustomobject]@{ Value = 'prepared-input-v1' }
    $preparedObservation = [pscustomobject]@{
        Baseline = $null
        PreparedValue = $null
        ProviderCalls = 0
        ValidationCalls = 0
        ValidationStage = $null
    }
    $result = Invoke-LmcDistributionCandidateTransaction `
        -CanonicalRoot $fixture.Canonical `
        -CandidatePath $fixture.Candidate `
        -PrepareInputs { $preparedFixtureInput } `
        -PopulateAndValidate {
            param($stage, $inputBaseline, $preparedInput)
            $preparedObservation.Baseline = $inputBaseline
            $preparedObservation.PreparedValue = $preparedInput.Value
            Populate-TestCandidate -Stage $stage
        } `
        -GetInputFingerprint {
            param($preparedInput)
            $preparedObservation.ProviderCalls += 1
            if ($null -eq $preparedInput) {
                return 'prepared-input-v1'
            }
            return $preparedInput.Value
        } `
        -ValidatePreparedInputs {
            param($preparedInput, $stage)
            $preparedObservation.ValidationCalls += 1
            $preparedObservation.ValidationStage = $stage
            if ($preparedInput.Value -ne 'prepared-input-v1') {
                throw 'prepared input changed before validation'
            }
        }
    Assert-Equal `
        -Expected 'prepared-input-v1' `
        -Actual $preparedObservation.Baseline `
        -Message 'Populate callback did not receive the transaction baseline.'
    Assert-Equal `
        -Expected 'prepared-input-v1' `
        -Actual $preparedObservation.PreparedValue `
        -Message 'Populate callback did not receive the prepared input.'
    Assert-Equal `
        -Expected 'prepared-input-v1' `
        -Actual $result.InputFingerprint `
        -Message 'Transaction result did not preserve the prepared baseline.'
    Assert-Equal `
        -Expected 2 `
        -Actual $preparedObservation.ProviderCalls `
        -Message 'Prepared transaction did not fingerprint baseline and live inputs.'
    Assert-Equal `
        -Expected 1 `
        -Actual $preparedObservation.ValidationCalls `
        -Message 'Prepared transaction did not validate metadata once.'
    Assert-True `
        -Condition (-not [string]::IsNullOrWhiteSpace(
            $preparedObservation.ValidationStage)) `
        -Message 'Prepared metadata validation did not receive the stage.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Prepared-input transaction'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Prepared-input transaction'

    $fixture = New-TestFixture -Name 'prepared_validation_failure'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PrepareInputs {
                    [pscustomobject]@{ Value = 'prepared-input-v1' }
                } `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                } `
                -GetInputFingerprint { 'prepared-input-v1' } `
                -ValidatePreparedInputs {
                    throw 'prepared metadata changed'
                }
        } `
        -ExpectedMessage 'prepared metadata changed' | Out-Null
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Prepared metadata failure published a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Prepared metadata failure'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Prepared metadata failure'

    # A validation callback failure removes only staging and keeps canonical exact.
    $fixture = New-TestFixture -Name 'callback_failure'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                    throw 'fixture populate failure'
                } `
                -GetInputFingerprint { 'input-v1' }
        } `
        -ExpectedMessage 'fixture populate failure' | Out-Null
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Callback failure published a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Callback failure'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Callback failure'

    # An actual-EXE gate failure remains inside PopulateAndValidate, so the
    # transaction removes staging without publishing or touching canonical.
    $fixture = New-TestFixture -Name 'executable_gate_failure'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                    $stageExecutable = Join-Path $stage (
                        '02_Example_Program/Run/' +
                        'LasalMotionControlApiExample.exe')
                    Write-TestFile `
                        -Path $stageExecutable `
                        -Content 'transaction-gate-executable'
                    Invoke-LmcDistributionExecutableRelaunchGate `
                        -StagingRoot $stage `
                        -ExecutablePath $stageExecutable `
                        -GateAction {
                            throw 'transaction executable gate failed'
                        } | Out-Null
                } `
                -GetInputFingerprint { 'input-v1' }
        } `
        -ExpectedMessage 'transaction executable gate failed' | Out-Null
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Executable gate failure published a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Executable gate failure'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Executable gate failure'

    # A mutation after a successful gate must fail the final identity check
    # before the candidate transaction can seal or promote the stage.
    $fixture = New-TestFixture -Name 'executable_identity_mutation'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                    $stageExecutable = Join-Path $stage (
                        '02_Example_Program/Run/' +
                        'LasalMotionControlApiExample.exe')
                    Write-TestFile `
                        -Path $stageExecutable `
                        -Content 'transaction-tested-executable'
                    $transactionTestedSha =
                        Invoke-LmcDistributionExecutableRelaunchGate `
                            -StagingRoot $stage `
                            -ExecutablePath $stageExecutable `
                            -GateAction { }
                    Write-TestFile `
                        -Path $stageExecutable `
                        -Content 'transaction-mutated-executable'
                    Assert-LmcDistributionExecutableRelaunchIdentity `
                        -StagingRoot $stage `
                        -ExecutablePath $stageExecutable `
                        -TestedSha256 $transactionTestedSha | Out-Null
                } `
                -GetInputFingerprint { 'input-v1' }
        } `
        -ExpectedMessage (
            'final example EXE bytes do not match the executable relaunch gate input') |
        Out-Null
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Executable identity mutation published a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Executable identity mutation'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Executable identity mutation'

    # Candidate bytes changed after validation must fail before Directory.Move.
    $fixture = New-TestFixture -Name 'tamper'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                } `
                -GetInputFingerprint { 'input-v1' } `
                -BeforePromotion {
                    param($stage, $candidate)
                    Write-TestFile `
                        -Path (Join-Path $stage 'tampered-after-seal.txt') `
                        -Content 'tamper'
                }
        } `
        -ExpectedMessage 'Candidate staging tree after validation changed' |
        Out-Null
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Tampered staging tree was promoted.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Candidate tamper failure'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Candidate tamper failure'

    # Inputs are fingerprinted before population and again before promotion.
    $fixture = New-TestFixture -Name 'input_drift'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $fingerprintState = [pscustomobject]@{
        Value = 'input-v1'
        Calls = 0
    }
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                } `
                -GetInputFingerprint {
                    $fingerprintState.Calls += 1
                    $fingerprintState.Value
                } `
                -BeforePromotion {
                    param($stage, $candidate)
                    $fingerprintState.Value = 'input-v2'
                }
        } `
        -ExpectedMessage 'Distribution input fingerprint changed before promotion' |
        Out-Null
    Assert-Equal `
        -Expected 2 `
        -Actual $fingerprintState.Calls `
        -Message 'Input drift test did not execute both fingerprint checks.'
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Input drift promoted a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Input drift failure'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Input drift failure'

    # A target that appears during the transaction must never be replaced.
    $fixture = New-TestFixture -Name 'occupied_target'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                } `
                -GetInputFingerprint { 'input-v1' } `
                -BeforePromotion {
                    param($stage, $candidate)
                    New-Item -ItemType Directory -Path $candidate | Out-Null
                    Write-TestFile `
                        -Path (Join-Path $candidate 'sentinel.txt') `
                        -Content 'external-owner'
                }
        } `
        -ExpectedMessage 'CandidatePath must not already exist before promotion' |
        Out-Null
    Assert-Equal `
        -Expected 'external-owner' `
        -Actual ([System.Text.Encoding]::ASCII.GetString(
            [System.IO.File]::ReadAllBytes(
                (Join-Path $fixture.Candidate 'sentinel.txt')))) `
        -Message 'Occupied candidate target was overwritten.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Occupied target failure'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Occupied target failure'

    # Canonical mutation is detected before promotion. The pipeline does not
    # attempt a risky automatic rollback of external canonical writes.
    $fixture = New-TestFixture -Name 'canonical_mutation'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $canonicalReadme = Join-Path $fixture.Canonical 'README.md'
    $canonicalReadmeBytes = [System.IO.File]::ReadAllBytes($canonicalReadme)
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                } `
                -GetInputFingerprint { 'input-v1' } `
                -BeforePromotion {
                    param($stage, $candidate)
                    Write-TestFile `
                        -Path $canonicalReadme `
                        -Content 'external-canonical-mutation'
                }
        } `
        -ExpectedMessage 'Canonical distribution before promotion changed' |
        Out-Null
    $canonicalAfterMutation = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-True `
        -Condition ($canonicalAfterMutation.Sha256 -ne $canonicalBefore.Sha256) `
        -Message 'Canonical mutation fixture did not actually change canonical.'
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Canonical mutation promoted a candidate.'
    [System.IO.File]::WriteAllBytes($canonicalReadme, $canonicalReadmeBytes)
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Canonical mutation fixture restoration'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Canonical mutation failure'

    # A nested contender proves FileShare.None exclusion without a child
    # process, polling, or an unbounded wait.
    $fixture = New-TestFixture -Name 'concurrency_lock'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $nestedCandidate = Join-Path $fixture.Parent `
        'LMC_API_Distribution_candidate_concurrency_nested'
    $lockObservation = [pscustomobject]@{
        Message = $null
    }
    $result = Invoke-LmcDistributionCandidateTransaction `
        -CanonicalRoot $fixture.Canonical `
        -CandidatePath $fixture.Candidate `
        -PopulateAndValidate {
            param($stage)
            try {
                Invoke-LmcDistributionCandidateTransaction `
                    -CanonicalRoot $fixture.Canonical `
                    -CandidatePath $nestedCandidate `
                    -PopulateAndValidate {
                        param($nestedStage)
                        Populate-TestCandidate -Stage $nestedStage
                    } `
                    -GetInputFingerprint { 'nested-input-v1' } |
                    Out-Null
            }
            catch {
                $lockObservation.Message = $_.Exception.Message
            }
            Populate-TestCandidate -Stage $stage
        } `
        -GetInputFingerprint { 'outer-input-v1' }
    Assert-True `
        -Condition $result.Committed `
        -Message 'Outer concurrency transaction did not complete.'
    Assert-True `
        -Condition ($null -ne $lockObservation.Message -and
            $lockObservation.Message.IndexOf(
                'exclusive distribution transaction lock',
                [System.StringComparison]::OrdinalIgnoreCase) -ge 0) `
        -Message 'Nested transaction was not rejected by the exclusive lock.'
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $nestedCandidate)) `
        -Message 'Nested lock contender published a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Concurrency transaction'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Concurrency transaction'

    # Cleanup accepts only a direct child with the exact generated stage name.
    $fixture = New-TestFixture -Name 'cleanup_safety'
    $unexpectedName = Join-Path $fixture.Parent 'not-a-stage'
    New-Item -ItemType Directory -Path $unexpectedName | Out-Null
    Write-TestFile `
        -Path (Join-Path $unexpectedName 'sentinel.txt') `
        -Content 'must-survive'
    Assert-Throws `
        -Action {
            Remove-LmcDistributionStagingDirectory `
                -StagingPath $unexpectedName `
                -ExpectedParent $fixture.Parent
        } `
        -ExpectedMessage 'unexpected directory name' | Out-Null
    Assert-True `
        -Condition (Test-Path -LiteralPath (
            Join-Path $unexpectedName 'sentinel.txt') -PathType Leaf) `
        -Message 'Cleanup removed an unexpected-name directory.'

    $outsideParent = Join-Path $script:TestRoot 'outside-cleanup-parent'
    New-Item -ItemType Directory -Path $outsideParent | Out-Null
    $outsideStage = Join-Path $outsideParent (
        '.LMC_API_Distribution.stage.' +
        [System.Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $outsideStage | Out-Null
    Assert-Throws `
        -Action {
            Remove-LmcDistributionStagingDirectory `
                -StagingPath $outsideStage `
                -ExpectedParent $fixture.Parent
        } `
        -ExpectedMessage 'outside the expected parent' | Out-Null
    Assert-True `
        -Condition (Test-Path -LiteralPath $outsideStage -PathType Container) `
        -Message 'Cleanup removed a stage outside the expected parent.'

    $reparseStage = Join-Path $fixture.Parent (
        '.LMC_API_Distribution.stage.' +
        [System.Guid]::NewGuid().ToString('N'))
    $reparseTarget = Join-Path $fixture.Parent 'reparse-target'
    $reparseLink = Join-Path $reparseStage 'linked-directory'
    New-Item -ItemType Directory -Path $reparseStage | Out-Null
    New-Item -ItemType Directory -Path $reparseTarget | Out-Null
    Write-TestFile `
        -Path (Join-Path $reparseTarget 'sentinel.txt') `
        -Content 'reparse-target-must-survive'
    New-Item -ItemType Junction `
        -Path $reparseLink `
        -Target $reparseTarget | Out-Null
    $script:TrackedReparsePaths.Add($reparseLink)
    Assert-Throws `
        -Action {
            Remove-LmcDistributionStagingDirectory `
                -StagingPath $reparseStage `
                -ExpectedParent $fixture.Parent
        } `
        -ExpectedMessage 'contains a reparse point' | Out-Null
    Assert-True `
        -Condition (Test-Path -LiteralPath (
            Join-Path $reparseTarget 'sentinel.txt') -PathType Leaf) `
        -Message 'Cleanup followed a reparse point into its target.'
    [System.IO.Directory]::Delete($reparseLink)
    Remove-LmcDistributionStagingDirectory `
        -StagingPath $reparseStage `
        -ExpectedParent $fixture.Parent
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $reparseStage)) `
        -Message 'Verified safe staging cleanup did not remove the stage.'
    Assert-CanonicalUnchanged `
        -Before (Get-LmcDistributionTreeSnapshot -Root $fixture.Canonical) `
        -Canonical $fixture.Canonical `
        -Context 'Cleanup safety checks'

    Write-Host "PASS: $script:Passed distribution pipeline assertions"
}
finally {
    Remove-TestRootSafely -Path $script:TestRoot
}
