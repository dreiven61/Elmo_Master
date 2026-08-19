[CmdletBinding(DefaultParameterSetName = 'Library')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Aggregate')]
    [switch]$VerifyCurrent,

    [Parameter(Mandatory = $true, ParameterSetName = 'Aggregate')]
    [string]$RepositoryRoot,

    [Parameter(Mandatory = $true, ParameterSetName = 'Worker')]
    [ValidateSet(
        'Pipeline',
        'SemanticPolicy',
        'ReleaseManifest',
        'ToolchainProvenance',
        'MethodSize',
        'UdpCallback',
        'ControlHandleRequest')]
    [string]$WorkerSuite,

    [Parameter(Mandatory = $true, ParameterSetName = 'Worker')]
    [string]$WorkerRepositoryRootBase64,

    [Parameter(Mandatory = $true, ParameterSetName = 'Worker')]
    [string]$WorkerPowerShellHomeBase64,

    [Parameter(Mandatory = $true, ParameterSetName = 'Worker')]
    [ValidatePattern('^[0-9a-f]{32}$')]
    [string]$WorkerNonce
)

# A worker is entered through -File from a separately validated PowerShell
# process. Reset the module path before any command that could autoload a module.
$script:LmcDistributionWorkerPowerShellHome = $null
if (-not [string]::IsNullOrWhiteSpace($WorkerSuite)) {
    $script:LmcDistributionWorkerPowerShellHome =
        [System.Text.Encoding]::UTF8.GetString(
            [System.Convert]::FromBase64String(
                $WorkerPowerShellHomeBase64))
    $env:PSModulePath = [System.IO.Path]::Combine(
        $script:LmcDistributionWorkerPowerShellHome,
        'Modules')
    $ProgressPreference = 'SilentlyContinue'
}

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$script:LmcDistributionToolingPreflightPath =
    [System.IO.Path]::GetFullPath($PSCommandPath)

function ConvertTo-LmcDistributionBase64 {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return [System.Convert]::ToBase64String(
        [System.Text.Encoding]::UTF8.GetBytes($Text))
}

function ConvertFrom-LmcDistributionBase64 {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    return [System.Text.Encoding]::UTF8.GetString(
        [System.Convert]::FromBase64String($Text))
}

function Get-LmcDistributionFileSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [string]$LiteralPath
    )

    $fullPath = [System.IO.Path]::GetFullPath($LiteralPath)
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        throw "Distribution tooling hash input was not found: $fullPath"
    }
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $stream = [System.IO.File]::OpenRead($fullPath)
        try {
            return ([System.BitConverter]::ToString(
                $sha.ComputeHash($stream))).Replace('-', '')
        }
        finally {
            $stream.Dispose()
        }
    }
    finally {
        $sha.Dispose()
    }
}

function Get-LmcDistributionOrdinalSortedUniqueStrings {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Values,

        [switch]$IgnoreCaseForUniqueness
    )

    $uniqueComparer = if ($IgnoreCaseForUniqueness) {
        [System.StringComparer]::OrdinalIgnoreCase
    }
    else {
        [System.StringComparer]::Ordinal
    }
    $unique = New-Object `
        'System.Collections.Generic.HashSet[string]' `
        $uniqueComparer
    foreach ($value in @($Values)) {
        if ($null -eq $value) {
            throw 'Ordinal string inventory contains null.'
        }
        $null = $unique.Add([string]$value)
    }
    [string[]]$sorted = @($unique)
    [System.Array]::Sort(
        $sorted,
        [System.StringComparer]::Ordinal)
    return @($sorted)
}

function ConvertTo-LmcDistributionProcessArgument {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Value
    )

    if ($Value.Length -ne 0 -and $Value -notmatch '[\s"]') {
        return $Value
    }
    if ($Value.IndexOf('"', [System.StringComparison]::Ordinal) -ge 0) {
        throw 'A child-process argument contains a quotation mark.'
    }
    return '"' + $Value + '"'
}

function Remove-LmcDistributionAnsiEscape {
    param(
        [AllowNull()]
        [string]$Text
    )

    if ($null -eq $Text) {
        return ''
    }
    $escapePattern = ([string][char]27) + '\[[0-?]*[ -/]*[@-~]'
    return [regex]::Replace($Text, $escapePattern, '')
}

function Get-LmcDistributionTerminalLine {
    param(
        [AllowNull()]
        [string]$Text
    )

    $lines = @(
        (Remove-LmcDistributionAnsiEscape -Text $Text) -split "`r?`n" |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
    if ($lines.Count -eq 0) {
        return ''
    }
    return ([string]$lines[-1]).TrimEnd()
}

function Get-LmcDistributionDiagnosticTail {
    param(
        [AllowNull()]
        [string]$Text,

        [ValidateRange(1, 16384)]
        [int]$MaximumCharacters = 4096
    )

    if ([string]::IsNullOrEmpty($Text)) {
        return ''
    }
    if ($Text.Length -le $MaximumCharacters) {
        return $Text
    }
    return $Text.Substring($Text.Length - $MaximumCharacters)
}

function Stop-LmcDistributionProcessTree {
    param(
        [Parameter(Mandatory = $true)]
        [int]$ProcessId
    )

    $taskKill = Join-Path $env:WINDIR 'System32\taskkill.exe'
    if (Test-Path -LiteralPath $taskKill -PathType Leaf) {
        $startInfo = New-Object System.Diagnostics.ProcessStartInfo
        $startInfo.FileName = $taskKill
        $startInfo.Arguments = "/PID $ProcessId /T /F"
        $startInfo.UseShellExecute = $false
        $startInfo.CreateNoWindow = $true
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true
        $killer = New-Object System.Diagnostics.Process
        $killer.StartInfo = $startInfo
        try {
            if ($killer.Start()) {
                $stdoutTask = $killer.StandardOutput.ReadToEndAsync()
                $stderrTask = $killer.StandardError.ReadToEndAsync()
                if (-not $killer.WaitForExit(10000)) {
                    try {
                        $killer.Kill()
                        $null = $killer.WaitForExit(5000)
                    }
                    catch {
                    }
                    return $false
                }
                else {
                    $null = $stdoutTask.GetAwaiter().GetResult()
                    $null = $stderrTask.GetAwaiter().GetResult()
                    return ($killer.ExitCode -eq 0)
                }
            }
        }
        finally {
            $killer.Dispose()
        }
    }
    else {
        try {
            [System.Diagnostics.Process]::GetProcessById($ProcessId).Kill()
            return $false
        }
        catch {
            return $false
        }
    }
    return $false
}

function Invoke-LmcDistributionRawPowerShellProcess {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$ExecutablePath,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,

        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,

        [ValidateRange(1, 1800)]
        [int]$TimeoutSeconds,

        [hashtable]$EnvironmentOverrides = @{},

        [string[]]$RemoveEnvironmentVariables = @()
    )

    $executable = [System.IO.Path]::GetFullPath($ExecutablePath)
    $working = [System.IO.Path]::GetFullPath($WorkingDirectory)
    if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) {
        throw "PowerShell child executable was not found: $executable"
    }
    if (-not (Test-Path -LiteralPath $working -PathType Container)) {
        throw "PowerShell child working directory was not found: $working"
    }

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $executable
    $startInfo.Arguments = (@(
            $Arguments |
                ForEach-Object {
                    ConvertTo-LmcDistributionProcessArgument `
                        -Value ([string]$_)
                }) -join ' ')
    $startInfo.WorkingDirectory = $working
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true

    foreach ($name in $RemoveEnvironmentVariables) {
        $null = $startInfo.EnvironmentVariables.Remove($name)
    }
    foreach ($name in @(Get-LmcDistributionOrdinalSortedUniqueStrings `
            -Values @($EnvironmentOverrides.Keys))) {
        $startInfo.EnvironmentVariables[[string]$name] =
            [string]$EnvironmentOverrides[$name]
    }

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        if (-not $process.Start()) {
            throw "PowerShell child process did not start: $executable"
        }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            $treeKillSucceeded = Stop-LmcDistributionProcessTree `
                -ProcessId $process.Id
            $terminated = $process.WaitForExit(10000)
            if (-not $terminated) {
                try {
                    $process.Kill()
                    $terminated = $process.WaitForExit(5000)
                }
                catch {
                    $terminated = $false
                }
            }
            $stdout = if ($terminated) {
                $stdoutTask.GetAwaiter().GetResult()
            }
            else {
                '<child did not terminate after exact-PID tree kill>'
            }
            $stderr = if ($terminated) {
                $stderrTask.GetAwaiter().GetResult()
            }
            else {
                '<child did not terminate after exact-PID tree kill>'
            }
            throw (
                "PowerShell child timed out after $TimeoutSeconds seconds. " +
                'stdoutTail=' +
                (Get-LmcDistributionDiagnosticTail -Text $stdout) +
                ' stderrTail=' +
                (Get-LmcDistributionDiagnosticTail -Text $stderr) +
                " treeKillSucceeded=$treeKillSucceeded " +
                "rootTerminated=$terminated")
        }
        $process.WaitForExit()
        $stopwatch.Stop()
        return [pscustomobject]@{
            ExitCode = $process.ExitCode
            StandardOutput = $stdoutTask.GetAwaiter().GetResult()
            StandardError = $stderrTask.GetAwaiter().GetResult()
            ElapsedMilliseconds = $stopwatch.ElapsedMilliseconds
        }
    }
    finally {
        $stopwatch.Stop()
        $process.Dispose()
    }
}

function Assert-LmcDistributionProcessResult {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Result,

        [Parameter(Mandatory = $true)]
        [string]$ExpectedTerminalLine,

        [string[]]$ExpectedEvidencePatterns = @(),

        [switch]$AllowStandardError
    )

    $maximumCapturedCharacters = 16 * 1024 * 1024
    if ($Result.StandardOutput.Length -gt $maximumCapturedCharacters -or
        $Result.StandardError.Length -gt $maximumCapturedCharacters) {
        throw 'PowerShell child output exceeded the 16 MiB validation bound.'
    }
    if ($Result.ExitCode -ne 0) {
        throw (
            "PowerShell child exited abnormally: $($Result.ExitCode) " +
            'stdoutTail=' +
            (Get-LmcDistributionDiagnosticTail `
                -Text $Result.StandardOutput) +
            ' stderrTail=' +
            (Get-LmcDistributionDiagnosticTail `
                -Text $Result.StandardError))
    }
    if (-not $AllowStandardError -and
        -not [string]::IsNullOrWhiteSpace($Result.StandardError)) {
        throw (
            'PowerShell child wrote stderr: ' +
            (Get-LmcDistributionDiagnosticTail `
                -Text $Result.StandardError))
    }
    $plainOutput = (Remove-LmcDistributionAnsiEscape `
        -Text $Result.StandardOutput).
        Replace("`r`n", "`n").Replace("`r", "`n")
    foreach ($pattern in $ExpectedEvidencePatterns) {
        $matches = [regex]::Matches(
            $plainOutput,
            $pattern,
            [System.Text.RegularExpressions.RegexOptions]::Multiline)
        if ($matches.Count -ne 1) {
            throw "PowerShell child required evidence occurrence drifted: pattern=$pattern count=$($matches.Count)"
        }
    }
    $terminal = Get-LmcDistributionTerminalLine `
        -Text $Result.StandardOutput
    if (-not $terminal.Equals(
            $ExpectedTerminalLine,
            [System.StringComparison]::Ordinal)) {
        throw "PowerShell child terminal evidence drifted. expected='$ExpectedTerminalLine' actual='$terminal'"
    }
}

function Invoke-LmcDistributionPowerShellIdentityProbe {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ExecutablePath,

        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,

        [Parameter(Mandatory = $true)]
        [ValidateSet('Desktop', 'Core')]
        [string]$ExpectedEdition,

        [Parameter(Mandatory = $true)]
        [int]$MinimumMajor,

        [Parameter(Mandatory = $true)]
        [int]$MaximumMajor
    )

    $nonce = [System.Guid]::NewGuid().ToString('N')
$probe = @'
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$edition = [string]$PSVersionTable.PSEdition
$major = [int]$PSVersionTable.PSVersion.Major
$version = [string]$PSVersionTable.PSVersion
$home64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes([string]$PSHOME))
[Console]::Out.WriteLine('LMC_HOST_ID|' + $env:LMC_HOST_NONCE + '|' + $edition + '|' + $major + '|' + $version + '|' + $home64)
'@
    $encodedProbe = [System.Convert]::ToBase64String(
        [System.Text.Encoding]::Unicode.GetBytes($probe))
    $result = Invoke-LmcDistributionRawPowerShellProcess `
        -ExecutablePath $ExecutablePath `
        -Arguments @(
            '-NoLogo', '-NoProfile', '-NonInteractive',
            '-ExecutionPolicy', 'Bypass',
            '-EncodedCommand', $encodedProbe) `
        -WorkingDirectory $WorkingDirectory `
        -TimeoutSeconds 30 `
        -RemoveEnvironmentVariables @('PSModulePath') `
        -EnvironmentOverrides @{ LMC_HOST_NONCE = $nonce }
    if ($result.ExitCode -ne 0 -or
        -not [string]::IsNullOrWhiteSpace($result.StandardError)) {
        throw "PowerShell identity probe failed: exit=$($result.ExitCode) stderr=$($result.StandardError)"
    }
    $terminal = Get-LmcDistributionTerminalLine `
        -Text $result.StandardOutput
    $match = [regex]::Match(
        $terminal,
        '^LMC_HOST_ID\|(?<Nonce>[0-9a-f]{32})\|(?<Edition>Desktop|Core)\|(?<Major>[0-9]+)\|(?<Version>[^|]+)\|(?<Home>[A-Za-z0-9+/=]+)$')
    if (-not $match.Success -or $match.Groups['Nonce'].Value -cne $nonce) {
        throw "PowerShell identity probe evidence was invalid: $terminal"
    }
    $edition = $match.Groups['Edition'].Value
    $major = [int]$match.Groups['Major'].Value
    if ($edition -cne $ExpectedEdition -or
        $major -lt $MinimumMajor -or
        $major -gt $MaximumMajor) {
        throw "PowerShell identity mismatch. expected=$ExpectedEdition/$MinimumMajor-$MaximumMajor actual=$edition/$major"
    }
    $powerShellHome = ConvertFrom-LmcDistributionBase64 `
        -Text $match.Groups['Home'].Value
    $modulePath = Join-Path $powerShellHome 'Modules'
    if (-not (Test-Path -LiteralPath $modulePath -PathType Container)) {
        throw "Validated PowerShell module directory was not found: $modulePath"
    }
    return [pscustomobject]@{
        Edition = $edition
        Major = $major
        Version = $match.Groups['Version'].Value
        PowerShellHome = [System.IO.Path]::GetFullPath($powerShellHome)
        ModulePath = [System.IO.Path]::GetFullPath($modulePath)
    }
}

function Resolve-LmcDistributionPowerShellHost {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$CandidatePaths,

        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,

        [Parameter(Mandatory = $true)]
        [ValidateSet('Desktop', 'Core')]
        [string]$ExpectedEdition,

        [Parameter(Mandatory = $true)]
        [int]$MinimumMajor,

        [Parameter(Mandatory = $true)]
        [int]$MaximumMajor,

        [scriptblock]$IdentityProbe
    )

    if ($null -eq $IdentityProbe) {
        $IdentityProbe = {
            param($path, $working, $edition, $minimum, $maximum)
            Invoke-LmcDistributionPowerShellIdentityProbe `
                -ExecutablePath $path `
                -WorkingDirectory $working `
                -ExpectedEdition $edition `
                -MinimumMajor $minimum `
                -MaximumMajor $maximum
        }
    }

    $structuralCandidates = New-Object `
        'System.Collections.Generic.Dictionary[string,string]' `
        ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($candidatePath in @($CandidatePaths)) {
        if ([string]::IsNullOrWhiteSpace($candidatePath) -or
            -not [System.IO.Path]::IsPathRooted($candidatePath)) {
            continue
        }
        $fullPath = [System.IO.Path]::GetFullPath($candidatePath)
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            continue
        }
        $item = Get-Item -LiteralPath $fullPath -Force
        if ($item.Length -le 0 -or
            (($item.Attributes -band
                [System.IO.FileAttributes]::ReparsePoint) -ne 0)) {
            continue
        }
        $fileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo(
            $fullPath).FileVersion
        if ([string]::IsNullOrWhiteSpace($fileVersion)) {
            continue
        }
        $structuralCandidates[$fullPath] = $fullPath
    }
    if ($structuralCandidates.Count -eq 0) {
        throw "$Name PowerShell host was not found as a physical executable."
    }

    $validated = @()
    $rejections = @()
    foreach ($path in @(Get-LmcDistributionOrdinalSortedUniqueStrings `
            -Values @($structuralCandidates.Keys) `
            -IgnoreCaseForUniqueness)) {
        try {
            $identity = & $IdentityProbe `
                $path `
                $WorkingDirectory `
                $ExpectedEdition `
                $MinimumMajor `
                $MaximumMajor
            $validated += [pscustomobject]@{
                Name = $Name
                Label = if ($ExpectedEdition -ceq 'Desktop') {
                    'PS5'
                }
                else {
                    'PS7'
                }
                Path = $path
                Edition = $identity.Edition
                Major = $identity.Major
                Version = $identity.Version
                PowerShellHome = $identity.PowerShellHome
                ModulePath = $identity.ModulePath
                ExecutableSha256 = Get-LmcDistributionFileSha256 `
                    -LiteralPath $path
            }
        }
        catch {
            $rejections += "$path => $($_.Exception.Message)"
        }
    }
    if ($validated.Count -eq 0) {
        throw "$Name PowerShell host identity was not accepted: $($rejections -join '; ')"
    }
    if ($validated.Count -ne 1) {
        throw "$Name PowerShell host resolution is ambiguous: $($validated.Path -join ', ')"
    }
    return $validated[0]
}

function Assert-LmcDistributionPowerShellHostExecutableCurrent {
    param(
        [Parameter(Mandatory = $true)]
        [object]$HostIdentity
    )

    if ($HostIdentity.PSObject.Properties.Name -notcontains 'Path' -or
        $HostIdentity.PSObject.Properties.Name -notcontains
            'ExecutableSha256' -or
        [string]$HostIdentity.ExecutableSha256 -notmatch
            '^[0-9A-F]{64}$') {
        throw 'Distribution PowerShell host executable snapshot is malformed.'
    }
    if (-not (Test-Path -LiteralPath $HostIdentity.Path -PathType Leaf)) {
        throw "Distribution PowerShell host executable disappeared: $($HostIdentity.Label)"
    }
    $currentSha256 = Get-LmcDistributionFileSha256 `
        -LiteralPath $HostIdentity.Path
    if (-not $currentSha256.Equals(
            $HostIdentity.ExecutableSha256,
            [System.StringComparison]::Ordinal)) {
        throw "Distribution PowerShell host executable changed during preflight: $($HostIdentity.Label)"
    }
    return $true
}

function Get-LmcDistributionToolingSuiteSpecifications {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $testRoot = 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests'
    return @(
        [pscustomobject]@{
            Id = 'Pipeline'
            RelativePath = 'LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1'
            TimeoutSeconds = 300
            EvidencePattern = '^PASS: 291 distribution pipeline assertions$'
            EvidenceLine = 'PASS: 291 distribution pipeline assertions'
            WorkerTerminates = $false
        },
        [pscustomobject]@{
            Id = 'SemanticPolicy'
            RelativePath = 'LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1'
            TimeoutSeconds = 120
            EvidencePattern = '^PASS LMC\.DistributionSemanticPolicy\.Tests 70 7B9CDFA6E3C14ED2AA0BA7DA23D87CC15C0A75AE2602BADB733C77F639222DE4 18$'
            EvidenceLine = 'PASS LMC.DistributionSemanticPolicy.Tests 70 7B9CDFA6E3C14ED2AA0BA7DA23D87CC15C0A75AE2602BADB733C77F639222DE4 18'
            WorkerTerminates = $false
        },
        [pscustomobject]@{
            Id = 'ReleaseManifest'
            RelativePath = 'LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1'
            TimeoutSeconds = 120
            EvidencePattern = '^TOTAL 108, PASSED 108, FAILED 0$'
            EvidenceLine = 'TOTAL 108, PASSED 108, FAILED 0'
            WorkerTerminates = $false
        },
        [pscustomobject]@{
            Id = 'ToolchainProvenance'
            RelativePath = 'LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1'
            TimeoutSeconds = 180
            EvidencePattern = '^PASS: 84 distribution toolchain provenance assertions$'
            EvidenceLine = 'PASS: 84 distribution toolchain provenance assertions'
            WorkerTerminates = $false
        },
        [pscustomobject]@{
            Id = 'MethodSize'
            RelativePath = "$testRoot/Verify-LasalCustomMethodSizeBudget.ps1"
            TimeoutSeconds = 180
            EvidencePattern = '^PASS: method-size verifier self-test 16/16\.$'
            EvidenceLine = 'PASS: method-size verifier self-test 16/16.'
            WorkerTerminates = $false
        },
        [pscustomobject]@{
            Id = 'UdpCallback'
            RelativePath = "$testRoot/Verify-LasalUdpCallbackContract.ps1"
            TimeoutSeconds = 900
            EvidencePattern = '^PASS LASAL\.UdpCallbackContract\.SelfTest \(336/336 negative fixtures rejected; Absent explicit, VendorImported, DerivedDeclaration, DerivedWired, corrected DerivedCandidate, and TerminalWakeBrokerCandidate positives accepted\)$'
            EvidenceLine = 'PASS LASAL.UdpCallbackContract.SelfTest (336/336 negative fixtures rejected; Absent explicit, VendorImported, DerivedDeclaration, DerivedWired, corrected DerivedCandidate, and TerminalWakeBrokerCandidate positives accepted)'
            WorkerTerminates = $false
        },
        [pscustomobject]@{
            Id = 'ControlHandleRequest'
            RelativePath = "$testRoot/Verify-LasalContract.ps1"
            TimeoutSeconds = 180
            EvidencePattern = '^PASS LASAL\.ControlHandleRequestVerifier\.SelfTest \(20/20 negative fixtures rejected; comment-only fixture accepted\)$'
            EvidenceLine = 'PASS LASAL.ControlHandleRequestVerifier.SelfTest (20/20 negative fixtures rejected; comment-only fixture accepted)'
            WorkerTerminates = $false
        }
    )
}

function Assert-LmcDistributionToolingSuiteSpecifications {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Specifications
    )

    $expectedIds = @(
        'Pipeline',
        'SemanticPolicy',
        'ReleaseManifest',
        'ToolchainProvenance',
        'MethodSize',
        'UdpCallback',
        'ControlHandleRequest')
    $testRoot =
        'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests'
    $expectedContracts = @{
        Pipeline = @{
            RelativePath = 'LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1'
            TimeoutSeconds = 300
            EvidencePattern = '^PASS: 291 distribution pipeline assertions$'
            EvidenceLine = 'PASS: 291 distribution pipeline assertions'
            WorkerTerminates = $false
        }
        SemanticPolicy = @{
            RelativePath = 'LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1'
            TimeoutSeconds = 120
            EvidencePattern = '^PASS LMC\.DistributionSemanticPolicy\.Tests 70 7B9CDFA6E3C14ED2AA0BA7DA23D87CC15C0A75AE2602BADB733C77F639222DE4 18$'
            EvidenceLine = 'PASS LMC.DistributionSemanticPolicy.Tests 70 7B9CDFA6E3C14ED2AA0BA7DA23D87CC15C0A75AE2602BADB733C77F639222DE4 18'
            WorkerTerminates = $false
        }
        ReleaseManifest = @{
            RelativePath = 'LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1'
            TimeoutSeconds = 120
            EvidencePattern = '^TOTAL 108, PASSED 108, FAILED 0$'
            EvidenceLine = 'TOTAL 108, PASSED 108, FAILED 0'
            WorkerTerminates = $false
        }
        ToolchainProvenance = @{
            RelativePath = 'LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1'
            TimeoutSeconds = 180
            EvidencePattern = '^PASS: 84 distribution toolchain provenance assertions$'
            EvidenceLine = 'PASS: 84 distribution toolchain provenance assertions'
            WorkerTerminates = $false
        }
        MethodSize = @{
            RelativePath = "$testRoot/Verify-LasalCustomMethodSizeBudget.ps1"
            TimeoutSeconds = 180
            EvidencePattern = '^PASS: method-size verifier self-test 16/16\.$'
            EvidenceLine = 'PASS: method-size verifier self-test 16/16.'
            WorkerTerminates = $false
        }
        UdpCallback = @{
            RelativePath = "$testRoot/Verify-LasalUdpCallbackContract.ps1"
            TimeoutSeconds = 900
            EvidencePattern = '^PASS LASAL\.UdpCallbackContract\.SelfTest \(336/336 negative fixtures rejected; Absent explicit, VendorImported, DerivedDeclaration, DerivedWired, corrected DerivedCandidate, and TerminalWakeBrokerCandidate positives accepted\)$'
            EvidenceLine = 'PASS LASAL.UdpCallbackContract.SelfTest (336/336 negative fixtures rejected; Absent explicit, VendorImported, DerivedDeclaration, DerivedWired, corrected DerivedCandidate, and TerminalWakeBrokerCandidate positives accepted)'
            WorkerTerminates = $false
        }
        ControlHandleRequest = @{
            RelativePath = "$testRoot/Verify-LasalContract.ps1"
            TimeoutSeconds = 180
            EvidencePattern = '^PASS LASAL\.ControlHandleRequestVerifier\.SelfTest \(20/20 negative fixtures rejected; comment-only fixture accepted\)$'
            EvidenceLine = 'PASS LASAL.ControlHandleRequestVerifier.SelfTest (20/20 negative fixtures rejected; comment-only fixture accepted)'
            WorkerTerminates = $false
        }
    }
    if ($Specifications.Count -ne $expectedIds.Count) {
        throw "Distribution tooling suite count drifted: $($Specifications.Count)"
    }
    $actualIds = @($Specifications | ForEach-Object { $_.Id })
    if (@($actualIds | Select-Object -Unique).Count -ne $actualIds.Count) {
        throw 'Distribution tooling suite IDs are missing, duplicated, or unexpected.'
    }
    for ($index = 0; $index -lt $expectedIds.Count; $index++) {
        if ($actualIds[$index] -cne $expectedIds[$index]) {
            throw 'Distribution tooling suite order is missing or unexpected.'
        }
    }
    foreach ($specification in $Specifications) {
        if ($specification.RelativePath -match
                '(?i)(Build-LmcApiDistribution|Test-LmcDistributionToolingHostParity)\.ps1$') {
            throw "Distribution tooling suite recursion is forbidden: $($specification.RelativePath)"
        }
        $expected = $expectedContracts[$specification.Id]
        if ($specification.RelativePath -cne $expected.RelativePath -or
            $specification.TimeoutSeconds -ne $expected.TimeoutSeconds -or
            $specification.EvidencePattern -cne $expected.EvidencePattern -or
            $specification.EvidenceLine -cne $expected.EvidenceLine -or
            [bool]$specification.WorkerTerminates -ne
                [bool]$expected.WorkerTerminates) {
            throw "Distribution tooling suite exact contract drifted: $($specification.Id)"
        }
    }
}

function Get-LmcDistributionToolingRelativePaths {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $paths = New-Object `
        'System.Collections.Generic.HashSet[string]' `
        ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($relativePath in @(
        'LMC_Library/LMC_API/Build-LmcApiDistribution.ps1',
        'LMC_Library/LMC_API/DistributionPipeline.ps1',
        'LMC_Library/LMC_API/DistributionSemanticPolicy.ps1',
        'LMC_Library/LMC_API/DistributionToolchainProvenance.ps1',
        'LMC_Library/LMC_API/ReleaseManifest.ps1',
        'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1',
        'LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1',
        'LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1',
        'LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1',
        'LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1',
        'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1',
        'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1',
        'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1',
        'LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs',
        'LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs',
        'LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt',
        'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st',
        'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st',
        'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st')) {
        $null = $paths.Add($relativePath)
    }

    $wpfRootRelative = 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp'
    $projectRelative = "$wpfRootRelative/LasalApiWpfTestApp.csproj"
    $projectPath = Join-Path $RepositoryRoot $projectRelative.Replace('/', '\')
    if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
        throw "Distribution tooling WPF project was not found: $projectPath"
    }
    $null = $paths.Add($projectRelative)
    [xml]$projectXml = Get-Content -LiteralPath $projectPath -Raw
    $namespace = New-Object System.Xml.XmlNamespaceManager(
        $projectXml.NameTable)
    $namespace.AddNamespace('m', $projectXml.Project.NamespaceURI)
    foreach ($node in @($projectXml.SelectNodes(
                '/m:Project/m:ItemGroup/m:ApplicationDefinition | ' +
                '/m:Project/m:ItemGroup/m:Page | ' +
                '/m:Project/m:ItemGroup/m:Compile',
                $namespace))) {
        $include = [string]$node.GetAttribute('Include')
        if ([string]::IsNullOrWhiteSpace($include) -or
            [System.IO.Path]::IsPathRooted($include)) {
            throw "Distribution tooling WPF project item is invalid: $include"
        }
        $normalized = "$wpfRootRelative/$($include.Replace('\', '/'))"
        if ($normalized -match '(^|/)\.\.(/|$)') {
            throw "Distribution tooling WPF project item escapes its root: $include"
        }
        $null = $paths.Add($normalized)
    }
    return @(Get-LmcDistributionOrdinalSortedUniqueStrings `
        -Values @($paths) `
        -IgnoreCaseForUniqueness)
}

function Get-LmcDistributionMonitoredFileSnapshot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,

        [string[]]$RelativePaths
    )

    $root = [System.IO.Path]::GetFullPath($RepositoryRoot).TrimEnd('\')
    if (-not (Test-Path -LiteralPath $root -PathType Container)) {
        throw "Distribution tooling repository was not found: $root"
    }
    if ($null -eq $RelativePaths) {
        $RelativePaths = Get-LmcDistributionToolingRelativePaths `
            -RepositoryRoot $root
    }
    $records = @()
    $sortedRelativePaths = @(
        Get-LmcDistributionOrdinalSortedUniqueStrings `
            -Values @($RelativePaths) `
            -IgnoreCaseForUniqueness)
    foreach ($relativePath in $sortedRelativePaths) {
        $normalized = ([string]$relativePath).Replace('\', '/')
        if ([string]::IsNullOrWhiteSpace($normalized) -or
            [System.IO.Path]::IsPathRooted($normalized) -or
            $normalized -match '(^|/)\.\.(/|$)') {
            throw "Distribution tooling monitored path is invalid: $relativePath"
        }
        $fullPath = [System.IO.Path]::GetFullPath(
            (Join-Path $root $normalized.Replace('/', '\')))
        if (-not $fullPath.StartsWith(
                $root + '\',
                [System.StringComparison]::OrdinalIgnoreCase) -or
            -not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            throw "Distribution tooling monitored file was not found: $fullPath"
        }
        $item = Get-Item -LiteralPath $fullPath -Force
        if (($item.Attributes -band
                [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "Distribution tooling monitored file is a reparse point: $fullPath"
        }
        $sha = [System.Security.Cryptography.SHA256]::Create()
        try {
            $stream = [System.IO.File]::OpenRead($fullPath)
            try {
                $hash = ([System.BitConverter]::ToString(
                    $sha.ComputeHash($stream))).Replace('-', '')
            }
            finally {
                $stream.Dispose()
            }
        }
        finally {
            $sha.Dispose()
        }
        $records += "$normalized|$($item.Length)|$hash"
    }
    $canonical = ($records -join "`n") + "`n"
    $digestAlgorithm = [System.Security.Cryptography.SHA256]::Create()
    try {
        $digest = ([System.BitConverter]::ToString(
            $digestAlgorithm.ComputeHash(
                [System.Text.Encoding]::UTF8.GetBytes($canonical)))).
            Replace('-', '')
    }
    finally {
        $digestAlgorithm.Dispose()
    }
    return [pscustomobject]@{
        Digest = $digest
        Records = @($records)
        FileCount = $records.Count
    }
}

function Assert-LmcDistributionMonitoredFileSnapshot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory = $true)]
        [object]$ExpectedSnapshot,

        [string[]]$RelativePaths
    )

    $actual = Get-LmcDistributionMonitoredFileSnapshot `
        -RepositoryRoot $RepositoryRoot `
        -RelativePaths $RelativePaths
    if (-not [string]::Equals(
            $ExpectedSnapshot.Digest,
            $actual.Digest,
            [System.StringComparison]::Ordinal) -or
        $ExpectedSnapshot.FileCount -ne $actual.FileCount -or
        (@(Compare-Object `
                -ReferenceObject @($ExpectedSnapshot.Records) `
                -DifferenceObject @($actual.Records)).Count -ne 0)) {
        throw "Distribution tooling monitored bytes changed after validation. expected=$($ExpectedSnapshot.Digest) actual=$($actual.Digest)"
    }
    return $actual
}

function Assert-LmcDistributionBuilderPreflightOrder {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BuilderText
    )

    $tokens = $null
    $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseInput(
        $BuilderText,
        [ref]$tokens,
        [ref]$errors)
    if ($errors.Count -ne 0) {
        throw "Distribution builder AST is invalid: $($errors[0].Message)"
    }
    $commands = @($ast.FindAll({
                param($node)
                $node -is [System.Management.Automation.Language.CommandAst]
            }, $true))
    $preflightCalls = @($commands | Where-Object {
            $_.GetCommandName() -ceq
                'Invoke-LmcDistributionToolingHostParityPreflight'
        })
    if ($preflightCalls.Count -ne 1) {
        throw "Distribution builder must invoke tooling preflight exactly once; actual=$($preflightCalls.Count)."
    }
    $preflightOffset = $preflightCalls[0].Extent.StartOffset
    foreach ($commandName in @(
            'Resolve-LmcDistributionManualInputs',
            'Invoke-LmcDistributionCandidateTransaction')) {
        $matches = @($commands | Where-Object {
                $_.GetCommandName() -ceq $commandName
            })
        if ($matches.Count -ne 1 -or
            $matches[0].Extent.StartOffset -le $preflightOffset) {
            throw "Distribution builder tooling preflight must precede $commandName."
        }
    }
    $toolchainCalls = @($commands | Where-Object {
            $_.GetCommandName() -ceq
                'Get-LmcDistributionReleaseToolchainSnapshot'
        })
    if ($toolchainCalls.Count -lt 3 -or
        @($toolchainCalls | Where-Object {
            $_.Extent.StartOffset -le $preflightOffset
        }).Count -ne 0) {
        throw 'Distribution builder tooling preflight must precede every release toolchain resolution.'
    }
    foreach ($marker in @(
            '$canonicalDistribution =',
            'if ([string]::IsNullOrWhiteSpace($CandidatePath))')) {
        $offset = $BuilderText.IndexOf(
            $marker,
            [System.StringComparison]::Ordinal)
        if ($offset -lt 0 -or $offset -le $preflightOffset) {
            throw "Distribution builder tooling preflight must precede marker: $marker"
        }
    }
    return $true
}

function Invoke-LmcDistributionToolingWorker {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SuiteId,

        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory = $true)]
        [string]$PowerShellHome,

        [Parameter(Mandatory = $true)]
        [string]$Nonce
    )

    $root = [System.IO.Path]::GetFullPath($RepositoryRoot).TrimEnd('\')
    $expectedHome = [System.IO.Path]::GetFullPath($PowerShellHome).TrimEnd('\')
    $actualHome = [System.IO.Path]::GetFullPath([string]$PSHOME).TrimEnd('\')
    if (-not $actualHome.Equals(
            $expectedHome,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Distribution tooling worker PSHOME mismatch. expected=$expectedHome actual=$actualHome"
    }
    $expectedModulePath = Join-Path $expectedHome 'Modules'
    if (-not [string]::Equals(
            $env:PSModulePath,
            $expectedModulePath,
            [System.StringComparison]::Ordinal)) {
        throw "Distribution tooling worker module path was not isolated: $env:PSModulePath"
    }
    if ($ExecutionContext.SessionState.PSVariable.Get('PSStyle')) {
        $PSStyle.OutputRendering = 'PlainText'
    }

    $specifications = @(Get-LmcDistributionToolingSuiteSpecifications `
        -RepositoryRoot $root)
    Assert-LmcDistributionToolingSuiteSpecifications `
        -Specifications $specifications
    $specification = @($specifications | Where-Object {
            $_.Id -ceq $SuiteId
        })
    if ($specification.Count -ne 1) {
        throw "Distribution tooling worker suite was not allow-listed: $SuiteId"
    }
    $specification = $specification[0]
    $suitePath = [System.IO.Path]::GetFullPath(
        (Join-Path $root $specification.RelativePath.Replace('/', '\')))
    if (-not $suitePath.StartsWith(
            $root + '\',
            [System.StringComparison]::OrdinalIgnoreCase) -or
        -not (Test-Path -LiteralPath $suitePath -PathType Leaf)) {
        throw "Distribution tooling worker suite path was not found: $suitePath"
    }
    $tokens = $null
    $errors = $null
    $null = [System.Management.Automation.Language.Parser]::ParseFile(
        $suitePath,
        [ref]$tokens,
        [ref]$errors)
    if ($errors.Count -ne 0) {
        throw "Distribution tooling worker suite AST is invalid: $($errors[0].Message)"
    }

    Write-Output (
        'LMC_TOOLING_MODULE_PATH ' + $Nonce + ' ' +
        (ConvertTo-LmcDistributionBase64 -Text $env:PSModulePath))
    $global:LASTEXITCODE = 0
    switch ($SuiteId) {
        'Pipeline' {
            & $suitePath
        }
        'SemanticPolicy' {
            $semanticResult = @(& $suitePath)
            if ($semanticResult.Count -ne 1 -or
                $semanticResult[0].Result -cne 'PASS' -or
                $semanticResult[0].TestCount -ne 70 -or
                $semanticResult[0].PolicyCheckCount -ne 18 -or
                $semanticResult[0].PolicySha256 -cne
                    '7B9CDFA6E3C14ED2AA0BA7DA23D87CC15C0A75AE2602BADB733C77F639222DE4') {
                throw 'Distribution semantic-policy suite result drifted.'
            }
            Write-Output (
                'PASS LMC.DistributionSemanticPolicy.Tests 70 ' +
                $semanticResult[0].PolicySha256 + ' 18')
        }
        'ReleaseManifest' {
            & $suitePath
        }
        'ToolchainProvenance' {
            & $suitePath
        }
        'MethodSize' {
            & $suitePath -RunSelfTest
        }
        'UdpCallback' {
            & $suitePath -RunSelfTest
        }
        'ControlHandleRequest' {
            & $suitePath `
                -RepositoryRoot $root `
                -ControlHandleRequestVerifierSelfTestOnly
        }
        default {
            throw "Distribution tooling worker suite was not handled: $SuiteId"
        }
    }
    if ($LASTEXITCODE -ne 0) {
        throw "Distribution tooling suite left a nonzero native exit code: $LASTEXITCODE"
    }
    Write-Output "PASS LMC.DistributionToolingWorker $SuiteId $Nonce"
}

function Invoke-LmcDistributionToolingHostParityPreflight {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $root = [System.IO.Path]::GetFullPath($RepositoryRoot).TrimEnd('\')
    $builderPath = Join-Path $root `
        'LMC_Library\LMC_API\Build-LmcApiDistribution.ps1'
    Assert-LmcDistributionBuilderPreflightOrder `
        -BuilderText ([System.IO.File]::ReadAllText($builderPath)) |
        Out-Null

    $specifications = @(Get-LmcDistributionToolingSuiteSpecifications `
        -RepositoryRoot $root)
    Assert-LmcDistributionToolingSuiteSpecifications `
        -Specifications $specifications
    foreach ($specification in $specifications) {
        $suitePath = Join-Path $root `
            $specification.RelativePath.Replace('/', '\')
        if (-not (Test-Path -LiteralPath $suitePath -PathType Leaf)) {
            throw "Distribution tooling suite was not found: $suitePath"
        }
    }

    $before = Get-LmcDistributionMonitoredFileSnapshot `
        -RepositoryRoot $root

    $windowsPowerShellPath = Join-Path $env:WINDIR `
        'System32\WindowsPowerShell\v1.0\powershell.exe'
    $windowsPowerShell = Resolve-LmcDistributionPowerShellHost `
        -Name 'WindowsPowerShell' `
        -CandidatePaths @($windowsPowerShellPath) `
        -WorkingDirectory $root `
        -ExpectedEdition 'Desktop' `
        -MinimumMajor 5 `
        -MaximumMajor 5
    $pwshCandidatePaths = @(
        Get-Command pwsh.exe -CommandType Application -All `
            -ErrorAction SilentlyContinue |
            ForEach-Object { [string]$_.Source }
    )
    $powerShell = Resolve-LmcDistributionPowerShellHost `
        -Name 'PowerShell' `
        -CandidatePaths $pwshCandidatePaths `
        -WorkingDirectory $root `
        -ExpectedEdition 'Core' `
        -MinimumMajor 7 `
        -MaximumMajor ([int]::MaxValue)
    if ($windowsPowerShell.Path.Equals(
            $powerShell.Path,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'Windows PowerShell and PowerShell 7 resolved to the same executable.'
    }

    $preflightPath = $script:LmcDistributionToolingPreflightPath
    $runCount = 0
    foreach ($hostSpecification in @($windowsPowerShell, $powerShell)) {
        Assert-LmcDistributionPowerShellHostExecutableCurrent `
            -HostIdentity $hostSpecification | Out-Null
        foreach ($suite in $specifications) {
            Assert-LmcDistributionMonitoredFileSnapshot `
                -RepositoryRoot $root `
                -ExpectedSnapshot $before | Out-Null
            $nonce = [System.Guid]::NewGuid().ToString('N')
            $arguments = @(
                '-NoLogo',
                '-NoProfile',
                '-NonInteractive',
                '-ExecutionPolicy', 'Bypass',
                '-File', $preflightPath,
                '-WorkerSuite', $suite.Id,
                '-WorkerRepositoryRootBase64',
                    (ConvertTo-LmcDistributionBase64 -Text $root),
                '-WorkerPowerShellHomeBase64',
                    (ConvertTo-LmcDistributionBase64 `
                        -Text $hostSpecification.PowerShellHome),
                '-WorkerNonce', $nonce)
            $result = Invoke-LmcDistributionRawPowerShellProcess `
                -ExecutablePath $hostSpecification.Path `
                -Arguments $arguments `
                -WorkingDirectory $root `
                -TimeoutSeconds $suite.TimeoutSeconds `
                -RemoveEnvironmentVariables @('PSModulePath') `
                -EnvironmentOverrides @{
                    PSModulePath = "LMC_POISON_$nonce"
                }
            $expectedModuleEvidence = '^LMC_TOOLING_MODULE_PATH ' +
                [regex]::Escape($nonce) + ' ' +
                [regex]::Escape((ConvertTo-LmcDistributionBase64 `
                    -Text $hostSpecification.ModulePath)) + '$'
            $expectedTerminal = if ($suite.WorkerTerminates) {
                $suite.EvidenceLine
            }
            else {
                "PASS LMC.DistributionToolingWorker $($suite.Id) $nonce"
            }
            Assert-LmcDistributionProcessResult `
                -Result $result `
                -ExpectedTerminalLine $expectedTerminal `
                -ExpectedEvidencePatterns @(
                    $expectedModuleEvidence,
                    $suite.EvidencePattern)
            Assert-LmcDistributionMonitoredFileSnapshot `
                -RepositoryRoot $root `
                -ExpectedSnapshot $before | Out-Null
            Assert-LmcDistributionPowerShellHostExecutableCurrent `
                -HostIdentity $hostSpecification | Out-Null
            $runCount++
            Write-Host (
                'PASS LMC.DistributionToolingHostParity ' +
                "host=$($hostSpecification.Label) " +
                "suite=$($suite.Id) exit=0 " +
                "elapsedMs=$($result.ElapsedMilliseconds)")
        }
    }
    if ($runCount -ne 14) {
        throw "Distribution tooling host parity was vacuous: $runCount/14."
    }
    $after = Assert-LmcDistributionMonitoredFileSnapshot `
        -RepositoryRoot $root `
        -ExpectedSnapshot $before
    foreach ($hostSpecification in @($windowsPowerShell, $powerShell)) {
        Assert-LmcDistributionPowerShellHostExecutableCurrent `
            -HostIdentity $hostSpecification | Out-Null
    }
    Write-Host (
        'PASS LMC.DistributionToolingHostParity 14/14 ' +
        '(PS5=7/7; PS7=7/7) ' +
        "files=$($after.FileCount) SHA256=$($after.Digest)")
    return [pscustomobject]@{
        Result = 'PASS'
        HostCount = 2
        SuiteCount = 7
        RunCount = $runCount
        ToolingDigest = $after.Digest
        ToolingFileCount = $after.FileCount
        ToolingRecords = @($after.Records)
        Digest = $after.Digest
        FileCount = $after.FileCount
        Records = @($after.Records)
        Hosts = @($windowsPowerShell, $powerShell)
    }
}

if (-not [string]::IsNullOrWhiteSpace($WorkerSuite)) {
    $workerRoot = ConvertFrom-LmcDistributionBase64 `
        -Text $WorkerRepositoryRootBase64
    Invoke-LmcDistributionToolingWorker `
        -SuiteId $WorkerSuite `
        -RepositoryRoot $workerRoot `
        -PowerShellHome $script:LmcDistributionWorkerPowerShellHome `
        -Nonce $WorkerNonce
    return
}

if ($VerifyCurrent) {
    Invoke-LmcDistributionToolingHostParityPreflight `
        -RepositoryRoot $RepositoryRoot | Out-Null
    return
}

if ($MyInvocation.InvocationName -ne '.') {
    throw 'Distribution tooling host-parity script selected no operation.'
}
