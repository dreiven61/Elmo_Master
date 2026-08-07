[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Join-Path $PSScriptRoot '..\..\..\..'),
    [switch]$RunSelfTest
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$MethodSizeLimitBytes = 32768
$StrictUtf8 = [System.Text.UTF8Encoding]::new($false, $true)

$ClassSpecifications = @(
    [pscustomobject]@{
        ClassName = 'TCPMotionInterface'
        RelativePath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'
    },
    [pscustomobject]@{
        ClassName = 'LMCControlCommandService'
        RelativePath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
    },
    [pscustomobject]@{
        ClassName = 'LMCDiagnosticsService'
        RelativePath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'
    },
    [pscustomobject]@{
        ClassName = 'LMCEcatInputLatch'
        RelativePath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st'
    },
    [pscustomobject]@{
        ClassName = 'LMCRecorderStore'
        RelativePath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st'
    },
    [pscustomobject]@{
        ClassName = 'LMCSdoExecutor'
        RelativePath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st'
    }
)

$BaselineDebt = @(
    [pscustomobject]@{
        ClassName = 'LMCControlCommandService'
        MethodName = 'ReserveAxisOwnership'
        RawBytes = 79880
        LFBytes = 77732
        CRLFBytes = 79881
    },
    [pscustomobject]@{
        ClassName = 'LMCControlCommandService'
        MethodName = 'PublishAxisOwnership'
        RawBytes = 65118
        LFBytes = 63444
        CRLFBytes = 65119
    },
    [pscustomobject]@{
        ClassName = 'LMCRecorderStore'
        MethodName = 'HandleRequest'
        RawBytes = 75829
        LFBytes = 75249
        CRLFBytes = 77210
    },
    [pscustomobject]@{
        ClassName = 'LMCEcatInputLatch'
        MethodName = 'RtWork'
        RawBytes = 73392
        LFBytes = 71906
        CRLFBytes = 73766
    }
)

function Get-MethodKey {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ClassName,
        [Parameter(Mandatory = $true)]
        [string]$MethodName
    )

    return $ClassName + '::' + $MethodName
}

function Get-ByteDimensions {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Block
    )

    $lfBlock = $Block.Replace("`r`n", "`n").Replace("`r", "`n")
    $crlfBlock = $lfBlock.Replace("`n", "`r`n")
    return [pscustomobject]@{
        RawBytes = [System.Text.Encoding]::UTF8.GetByteCount($Block)
        LFBytes = [System.Text.Encoding]::UTF8.GetByteCount($lfBlock)
        CRLFBytes = [System.Text.Encoding]::UTF8.GetByteCount($crlfBlock)
    }
}

function Get-LasalFunctionInventory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceText,
        [Parameter(Mandatory = $true)]
        [string]$ClassName,
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $escapedClassName = [regex]::Escape($ClassName)
    $methodNamePattern = (
        '(?<MethodName>(?:@[A-Za-z_][A-Za-z0-9_]*|' +
        '[A-Za-z_][A-Za-z0-9_]*)(?:::[A-Za-z_][A-Za-z0-9_]*)*)')
    $modifierPattern = (
        '(?:(?<Modifiers>(?:(?:VIRTUAL[ \t]+)?GLOBAL' +
        '(?:[ \t]+TAB)?|TAB))[ \t]+)?')
    $exactHeaderPattern = (
        '(?im)^[ \t]*FUNCTION[ \t]+' + $modifierPattern +
        '(?<QualifiedName>' + $escapedClassName + '::' +
        $methodNamePattern + ')[ \t]*\r?$')
    $blockPattern = (
        '(?ims)^[ \t]*FUNCTION[ \t]+' + $modifierPattern +
        '(?<QualifiedName>' + $escapedClassName + '::' +
        $methodNamePattern + ')[ \t]*\r?$' +
        '.*?^[ \t]*END_FUNCTION[ \t]*\r?$')
    $broadHeaderPattern = (
        '(?im)^[ \t]*FUNCTION\b[^\r\n]*' +
        $escapedClassName + '::[^\r\n]*')
    $endPattern = '(?im)^[ \t]*END_FUNCTION[ \t]*\r?$'

    $broadHeaders = [regex]::Matches($SourceText, $broadHeaderPattern)
    $exactHeaders = [regex]::Matches($SourceText, $exactHeaderPattern)
    $endMarkers = [regex]::Matches($SourceText, $endPattern)
    $blocks = [regex]::Matches($SourceText, $blockPattern)

    if ($broadHeaders.Count -ne $exactHeaders.Count) {
        throw (
            "$RelativePath method-size blocker: exact FUNCTION header parse is " +
            "$($exactHeaders.Count)/$($broadHeaders.Count).")
    }
    if ($exactHeaders.Count -ne $endMarkers.Count -or
        $exactHeaders.Count -ne $blocks.Count) {
        throw (
            "$RelativePath method-size blocker: FUNCTION/header/block/end count is " +
            "$($broadHeaders.Count)/$($exactHeaders.Count)/$($blocks.Count)/" +
            "$($endMarkers.Count), expected all counts to match.")
    }
    if ($blocks.Count -eq 0) {
        throw "$RelativePath method-size blocker: no FUNCTION blocks were parsed."
    }

    $seenNames = @{}
    $inventory = @()
    for ($index = 0; $index -lt $blocks.Count; $index++) {
        $header = $exactHeaders[$index]
        $block = $blocks[$index]
        $headerName = $header.Groups['QualifiedName'].Value
        $blockName = $block.Groups['QualifiedName'].Value
        if ($headerName -cne $blockName -or $header.Index -ne $block.Index) {
            throw (
                "$RelativePath method-size blocker: header/block identity drifted " +
                "at index $index ($headerName / $blockName).")
        }
        $nestedHeaderCount = [regex]::Matches(
            $block.Value,
            $broadHeaderPattern).Count
        $nestedEndCount = [regex]::Matches($block.Value, $endPattern).Count
        if ($nestedHeaderCount -ne 1 -or $nestedEndCount -ne 1) {
            throw (
                "$RelativePath method-size blocker: $blockName contains " +
                "$nestedHeaderCount FUNCTION headers and $nestedEndCount END_FUNCTION " +
                'markers, expected one each.')
        }

        $methodName = $block.Groups['MethodName'].Value
        $key = Get-MethodKey -ClassName $ClassName -MethodName $methodName
        if ($seenNames.ContainsKey($key)) {
            throw "$RelativePath method-size blocker: duplicate method $key."
        }
        $seenNames[$key] = $true

        $dimensions = Get-ByteDimensions -Block $block.Value
        $inventory += [pscustomobject]@{
            ClassName = $ClassName
            MethodName = $methodName
            QualifiedName = $blockName
            Modifiers = $block.Groups['Modifiers'].Value
            RelativePath = $RelativePath
            RawBytes = $dimensions.RawBytes
            LFBytes = $dimensions.LFBytes
            CRLFBytes = $dimensions.CRLFBytes
            PeakBytes = [Math]::Max(
                $dimensions.RawBytes,
                [Math]::Max($dimensions.LFBytes, $dimensions.CRLFBytes))
        }
    }

    return $inventory
}

function Get-BaselineDebtIndex {
    $index = @{}
    foreach ($entry in $BaselineDebt) {
        $key = Get-MethodKey `
            -ClassName $entry.ClassName `
            -MethodName $entry.MethodName
        if ($index.ContainsKey($key)) {
            throw "Method-size baseline contains duplicate entry $key."
        }
        $index[$key] = $entry
    }
    if ($index.Count -ne 4) {
        throw "Method-size baseline count is $($index.Count), expected exactly 4."
    }
    return $index
}

function Assert-MethodSizeBudget {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Inventory,
        [Parameter(Mandatory = $true)]
        [string]$Owner
    )

    $baselineIndex = Get-BaselineDebtIndex
    $seenMethods = @{}
    $currentDebtCount = 0
    $currentBaselineDebtCount = 0
    foreach ($method in $Inventory) {
        $key = Get-MethodKey `
            -ClassName $method.ClassName `
            -MethodName $method.MethodName
        if ($seenMethods.ContainsKey($key)) {
            throw "$Owner method-size blocker: duplicate inventory entry $key."
        }
        $seenMethods[$key] = $true

        $isOversized = (
            $method.RawBytes -ge $MethodSizeLimitBytes -or
            $method.LFBytes -ge $MethodSizeLimitBytes -or
            $method.CRLFBytes -ge $MethodSizeLimitBytes)
        if ($isOversized) {
            $currentDebtCount++
        }

        if ($baselineIndex.ContainsKey($key)) {
            $baseline = $baselineIndex[$key]
            if ($method.RawBytes -gt $baseline.RawBytes -or
                $method.LFBytes -gt $baseline.LFBytes -or
                $method.CRLFBytes -gt $baseline.CRLFBytes) {
                throw (
                    "$Owner method-size blocker: baseline debt $key grew from " +
                    "raw=$($baseline.RawBytes), LF=$($baseline.LFBytes), " +
                    "CRLF=$($baseline.CRLFBytes) to raw=$($method.RawBytes), " +
                    "LF=$($method.LFBytes), CRLF=$($method.CRLFBytes).")
            }
            if ($isOversized) {
                $currentBaselineDebtCount++
            }
        }
        elseif ($isOversized) {
            throw (
                "$Owner method-size blocker: new debt $key is " +
                "raw=$($method.RawBytes), LF=$($method.LFBytes), " +
                "CRLF=$($method.CRLFBytes); every new method dimension must be " +
                "below $MethodSizeLimitBytes.")
        }
    }

    return [pscustomobject]@{
        MethodCount = $Inventory.Count
        DebtCount = $currentDebtCount
        BaselineDebtCount = $currentBaselineDebtCount
        UnderLimitCount = $Inventory.Count - $currentDebtCount
    }
}

function New-TestInventoryEntry {
    param(
        [string]$ClassName,
        [string]$MethodName,
        [int]$RawBytes,
        [int]$LFBytes,
        [int]$CRLFBytes
    )

    return [pscustomobject]@{
        ClassName = $ClassName
        MethodName = $MethodName
        QualifiedName = $ClassName + '::' + $MethodName
        Modifiers = ''
        RelativePath = 'self-test.st'
        RawBytes = $RawBytes
        LFBytes = $LFBytes
        CRLFBytes = $CRLFBytes
        PeakBytes = [Math]::Max($RawBytes, [Math]::Max($LFBytes, $CRLFBytes))
    }
}

function Assert-SelfTestThrows {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action,
        [Parameter(Mandatory = $true)]
        [string]$ExpectedText,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    try {
        & $Action
    }
    catch {
        if ($_.Exception.Message -notlike "*$ExpectedText*") {
            throw (
                "Self-test $Name failed with unexpected error: " +
                $_.Exception.Message)
        }
        return
    }
    throw "Self-test $Name did not reject the negative mutation."
}

function Invoke-SelfTest {
    $testCount = 0
    $fixtureText = [string]::Join("`r`n", @(
            'FUNCTION GLOBAL TAB FixtureClass::@CT_',
            'END_FUNCTION',
            '',
            'FUNCTION FixtureClass::@STD',
            'END_FUNCTION',
            '',
            'FUNCTION VIRTUAL GLOBAL FixtureClass::Channel::Write',
            'END_FUNCTION',
            '',
            'FUNCTION GLOBAL FixtureClass::Run',
            'END_FUNCTION',
            ''))
    $parsed = @(Get-LasalFunctionInventory `
            -SourceText $fixtureText `
            -ClassName 'FixtureClass' `
            -RelativePath 'self-test.st')
    $expectedNames = @('@CT_', '@STD', 'Channel::Write', 'Run')
    $expectedModifiers = @('GLOBAL TAB', '', 'VIRTUAL GLOBAL', 'GLOBAL')
    if ($parsed.Count -ne 4) {
        throw "Self-test parser inventory is $($parsed.Count), expected 4."
    }
    for ($index = 0; $index -lt $expectedNames.Count; $index++) {
        if ($parsed[$index].MethodName -cne $expectedNames[$index] -or
            $parsed[$index].Modifiers -cne $expectedModifiers[$index]) {
            throw "Self-test parser identity drifted at index $index."
        }
    }
    $testCount++
    Write-Output "SELFTEST $testCount PASS exact header/block/name inventory"

    $reserveBaseline = $BaselineDebt | Where-Object {
        $_.ClassName -ceq 'LMCControlCommandService' -and
        $_.MethodName -ceq 'ReserveAxisOwnership'
    }
    $shrinkInventory = @(
        (New-TestInventoryEntry `
            -ClassName $reserveBaseline.ClassName `
            -MethodName $reserveBaseline.MethodName `
            -RawBytes ($reserveBaseline.RawBytes - 1) `
            -LFBytes ($reserveBaseline.LFBytes - 1) `
            -CRLFBytes ($reserveBaseline.CRLFBytes - 1))
        (New-TestInventoryEntry `
            -ClassName 'FixtureClass' `
            -MethodName 'SmallMethod' `
            -RawBytes 100 `
            -LFBytes 100 `
            -CRLFBytes 101)
    )
    [void](Assert-MethodSizeBudget `
            -Inventory $shrinkInventory `
            -Owner 'self-test shrink')
    $testCount++
    Write-Output "SELFTEST $testCount PASS baseline shrink"

    $removalInventory = @(
        New-TestInventoryEntry `
            -ClassName 'FixtureClass' `
            -MethodName 'SmallMethod' `
            -RawBytes 100 `
            -LFBytes 100 `
            -CRLFBytes 101)
    [void](Assert-MethodSizeBudget `
            -Inventory $removalInventory `
            -Owner 'self-test removal')
    $testCount++
    Write-Output "SELFTEST $testCount PASS baseline removal"

    $newDebtInventory = @(
        New-TestInventoryEntry `
            -ClassName 'FixtureClass' `
            -MethodName 'NewDebt' `
            -RawBytes ($MethodSizeLimitBytes - 1) `
            -LFBytes $MethodSizeLimitBytes `
            -CRLFBytes ($MethodSizeLimitBytes - 1))
    Assert-SelfTestThrows `
        -Action {
            [void](Assert-MethodSizeBudget `
                    -Inventory $newDebtInventory `
                    -Owner 'self-test new debt')
        } `
        -ExpectedText 'new debt FixtureClass::NewDebt' `
        -Name 'new debt'
    $testCount++
    Write-Output "SELFTEST $testCount PASS new debt rejection"

    $retiredReceiptDebtInventory = @(
        New-TestInventoryEntry `
            -ClassName 'LMCControlCommandService' `
            -MethodName 'PublishAxisOwnershipDs402Receipt' `
            -RawBytes ($MethodSizeLimitBytes - 1) `
            -LFBytes ($MethodSizeLimitBytes - 1) `
            -CRLFBytes $MethodSizeLimitBytes)
    Assert-SelfTestThrows `
        -Action {
            [void](Assert-MethodSizeBudget `
                    -Inventory $retiredReceiptDebtInventory `
                    -Owner 'self-test retired receipt debt')
        } `
        -ExpectedText (
            'new debt LMCControlCommandService::' +
            'PublishAxisOwnershipDs402Receipt') `
        -Name 'retired receipt debt recurrence'
    $testCount++
    Write-Output "SELFTEST $testCount PASS retired receipt debt rejection"

    $retiredRollbackDebtInventory = @(
        New-TestInventoryEntry `
            -ClassName 'LMCControlCommandService' `
            -MethodName 'RollbackAxisOwnership' `
            -RawBytes ($MethodSizeLimitBytes - 1) `
            -LFBytes ($MethodSizeLimitBytes - 1) `
            -CRLFBytes $MethodSizeLimitBytes)
    Assert-SelfTestThrows `
        -Action {
            [void](Assert-MethodSizeBudget `
                    -Inventory $retiredRollbackDebtInventory `
                    -Owner 'self-test retired rollback debt')
        } `
        -ExpectedText (
            'new debt LMCControlCommandService::RollbackAxisOwnership') `
        -Name 'retired rollback debt recurrence'
    $testCount++
    Write-Output "SELFTEST $testCount PASS retired rollback debt rejection"

    $growthInventory = @(
        New-TestInventoryEntry `
            -ClassName $reserveBaseline.ClassName `
            -MethodName $reserveBaseline.MethodName `
            -RawBytes ($reserveBaseline.RawBytes + 1) `
            -LFBytes $reserveBaseline.LFBytes `
            -CRLFBytes $reserveBaseline.CRLFBytes)
    Assert-SelfTestThrows `
        -Action {
            [void](Assert-MethodSizeBudget `
                    -Inventory $growthInventory `
                    -Owner 'self-test baseline growth')
        } `
        -ExpectedText 'baseline debt LMCControlCommandService::ReserveAxisOwnership grew' `
        -Name 'baseline growth'
    $testCount++
    Write-Output "SELFTEST $testCount PASS baseline growth rejection"

    Write-Output "PASS: method-size verifier self-test $testCount/$testCount."
}

if ($RunSelfTest) {
    Invoke-SelfTest
    exit 0
}

$resolvedRoot = [System.IO.Path]::GetFullPath($RepositoryRoot)
if (-not (Test-Path -LiteralPath $resolvedRoot -PathType Container)) {
    throw "Repository root does not exist: $resolvedRoot"
}

$allInventory = @()
$sourceEvidence = @()
foreach ($specification in $ClassSpecifications) {
    $sourcePath = Join-Path $resolvedRoot $specification.RelativePath
    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
        throw "Tracked LASAL source does not exist: $sourcePath"
    }
    $sourceText = [System.IO.File]::ReadAllText($sourcePath, $StrictUtf8)
    $classInventory = @(Get-LasalFunctionInventory `
            -SourceText $sourceText `
            -ClassName $specification.ClassName `
            -RelativePath $specification.RelativePath)
    $allInventory += $classInventory
    $sourceEvidence += [pscustomobject]@{
        ClassName = $specification.ClassName
        RelativePath = $specification.RelativePath
        MethodCount = $classInventory.Count
        ByteCount = (Get-Item -LiteralPath $sourcePath).Length
        Sha256 = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash
    }
}

$result = Assert-MethodSizeBudget `
    -Inventory $allInventory `
    -Owner 'LASAL custom service inventory'
$baselineIndex = Get-BaselineDebtIndex

foreach ($source in $sourceEvidence) {
    Write-Output (
        "SOURCE $($source.ClassName) methods=$($source.MethodCount) " +
        "bytes=$($source.ByteCount) SHA256=$($source.Sha256) " +
        "path=$($source.RelativePath)")
}

$rank = 0
$sortedInventory = $allInventory | Sort-Object `
    @{ Expression = 'PeakBytes'; Descending = $true },
    @{ Expression = 'CRLFBytes'; Descending = $true },
    @{ Expression = 'RawBytes'; Descending = $true },
    @{ Expression = 'QualifiedName'; Descending = $false }
foreach ($method in $sortedInventory) {
    if ($method.PeakBytes -lt 30000) {
        continue
    }
    $rank++
    $key = Get-MethodKey `
        -ClassName $method.ClassName `
        -MethodName $method.MethodName
    $debt = if ($baselineIndex.ContainsKey($key) -and
        $method.PeakBytes -ge $MethodSizeLimitBytes) {
        'baseline'
    }
    else {
        'none'
    }
    Write-Output (
        ('SIZE {0:D3} {1} raw={2} LF={3} CRLF={4} debt={5}' -f
            $rank,
            $method.QualifiedName,
            $method.RawBytes,
            $method.LFBytes,
            $method.CRLFBytes,
            $debt))
}

Write-Output (
    "PASS: LASAL custom method size budget classes=$($ClassSpecifications.Count), " +
    "methods=$($result.MethodCount), under-limit=$($result.UnderLimitCount), " +
    "baseline-debt=$($result.BaselineDebtCount), threshold=<$MethodSizeLimitBytes.")
