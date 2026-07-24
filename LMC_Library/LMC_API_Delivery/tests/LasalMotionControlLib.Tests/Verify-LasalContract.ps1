param(
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot,

    [switch]$SourceOnly,

    [ValidateSet(
        'Phase2Skeleton',
        'Phase3GroupDormant',
        'Phase3GroupRouted')]
    [string]$ControlServiceCheckpoint = 'Phase3GroupDormant'
)

$ErrorActionPreference = 'Stop'

function Assert-Match {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Message
    )

    if ($Text -notmatch $Pattern) {
        throw $Message
    }
}

function Get-LasalCommandCaseIds {
    param(
        [string]$FunctionBlock
    )

    $commandIds = @()
    $caseLabelPattern = (
        '(?m)^[ \t]*(?<Labels>0x[0-9A-Fa-f]{4}' +
        '(?:[ \t]*,[ \t]*(?:\r?\n[ \t]*)?' +
        '0x[0-9A-Fa-f]{4})*)[ \t]*:')
    foreach ($caseLabel in [regex]::Matches(
            $FunctionBlock,
            $caseLabelPattern)) {
        foreach ($commandId in [regex]::Matches(
                $caseLabel.Groups['Labels'].Value,
                '0x(?<Id>[0-9A-Fa-f]{4})')) {
            $commandIds += $commandId.Groups['Id'].Value.ToUpperInvariant()
        }
    }

    return @($commandIds)
}

function Assert-ExactLasalCommandCaseIds {
    param(
        [string]$FunctionBlock,
        [string]$Owner,
        [string[]]$ExpectedCommandIds
    )

    Assert-Match $FunctionBlock '(?i)\bcase\s+CommandId\s+of\b' (
        "$Owner CommandId case was not found.")

    $actualCommandIds = @(Get-LasalCommandCaseIds $FunctionBlock)
    $duplicateCommandIds = @(
        $actualCommandIds |
            Group-Object |
            Where-Object { $_.Count -ne 1 } |
            ForEach-Object { $_.Name })
    if ($duplicateCommandIds.Count -ne 0) {
        throw (
            "$Owner contains duplicate command IDs: " +
            ($duplicateCommandIds -join ', ') + '.')
    }

    $expected = @($ExpectedCommandIds | ForEach-Object {
            $_.ToUpperInvariant()
        })
    $difference = @(Compare-Object `
        -ReferenceObject $expected `
        -DifferenceObject $actualCommandIds)
    if ($difference.Count -ne 0 -or
        $actualCommandIds.Count -ne $expected.Count) {
        throw (
            "$Owner command IDs are [$($actualCommandIds -join ', ')], " +
            "expected exactly [$($expected -join ', ')].")
    }
}

function Assert-ExactLasalCommandRouteIds {
    param(
        [string]$RouterBlock,
        [string]$Owner,
        [string]$CallPattern,
        [string[]]$ExpectedCommandIds
    )

    $routePattern = (
        '(?ms)^[ \t]*(?<Labels>0x[0-9A-Fa-f]{4}' +
        '(?:[ \t]*,[ \t]*(?:\r?\n[ \t]*)?' +
        '0x[0-9A-Fa-f]{4})*)[ \t]*:' +
        '(?<Body>.*?)(?=^[ \t]*(?:0x[0-9A-Fa-f]{4}|else\b|end_case\b))')
    $matchingRoutes = @(
        [regex]::Matches($RouterBlock, $routePattern) |
            Where-Object { $_.Groups['Body'].Value -match $CallPattern })
    if ($matchingRoutes.Count -ne 1) {
        throw (
            "$Owner matching route count is $($matchingRoutes.Count), " +
            'expected one.')
    }

    $actualCommandIds = @(
        [regex]::Matches(
            $matchingRoutes[0].Groups['Labels'].Value,
            '0x(?<Id>[0-9A-Fa-f]{4})') |
            ForEach-Object { $_.Groups['Id'].Value.ToUpperInvariant() })
    $expected = @($ExpectedCommandIds | ForEach-Object {
            $_.ToUpperInvariant()
        })
    $difference = @(Compare-Object `
        -ReferenceObject $expected `
        -DifferenceObject $actualCommandIds)
    if ($difference.Count -ne 0 -or
        $actualCommandIds.Count -ne $expected.Count) {
        throw (
            "$Owner command IDs are [$($actualCommandIds -join ', ')], " +
            "expected exactly [$($expected -join ', ')].")
    }
}

function Assert-ExactRegexValueSet {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Owner,
        [string[]]$ExpectedValues
    )

    $actualValues = @(
        [regex]::Matches($Text, $Pattern) |
            ForEach-Object { $_.Groups['Value'].Value } |
            Sort-Object -Unique)
    $expected = @($ExpectedValues | Sort-Object -Unique)
    $difference = @(Compare-Object `
        -ReferenceObject $expected `
        -DifferenceObject $actualValues)
    if ($difference.Count -ne 0 -or
        $actualValues.Count -ne $expected.Count) {
        throw (
            "$Owner values are [$($actualValues -join ', ')], " +
            "expected exactly [$($expected -join ', ')].")
    }
}

function Assert-ExactLasalConnectedClientSet {
    param(
        [string]$Text,
        [string]$Owner,
        [string[]]$ExpectedClients
    )

    $actualClients = @(
        [regex]::Matches(
            $Text,
            'IsClientConnected\(#(?<Name>[A-Za-z_][A-Za-z0-9_]*)\)') |
            ForEach-Object { $_.Groups['Name'].Value })
    $duplicateClients = @(
        $actualClients |
            Group-Object |
            Where-Object { $_.Count -ne 1 } |
            ForEach-Object { $_.Name })
    $expected = @($ExpectedClients | Sort-Object)
    $actualDistinct = @($actualClients | Sort-Object -Unique)
    $difference = @(Compare-Object `
        -ReferenceObject $expected `
        -DifferenceObject $actualDistinct)
    if ($duplicateClients.Count -ne 0 -or
        $difference.Count -ne 0 -or
        $actualClients.Count -ne $expected.Count) {
        throw (
            "$Owner connected clients are [$($actualClients -join ', ')], " +
            "expected each exactly once: [$($ExpectedClients -join ', ')].")
    }
}

function Test-LasalFailClosedBody {
    param(
        [string]$FunctionBlock
    )

    return [regex]::IsMatch(
        $FunctionBlock,
        ('(?s)VAR_OUTPUT\s*ResponseSize\s*:\s*DINT\s*;\s*END_VAR\s*' +
         'ResponseSize\s*:=\s*-1\s*;\s*END_FUNCTION\s*\z'))
}

function Assert-LasalFailClosedBody {
    param(
        [string]$FunctionBlock,
        [string]$Owner,
        [string]$Checkpoint
    )

    if (-not (Test-LasalFailClosedBody $FunctionBlock)) {
        throw "$Checkpoint $Owner must contain only ResponseSize := -1."
    }
}

function Assert-LasalImplementedBody {
    param(
        [string]$FunctionBlock,
        [string]$Owner,
        [string]$Checkpoint
    )

    if (Test-LasalFailClosedBody $FunctionBlock) {
        throw "$Checkpoint $Owner must be implemented, not fail-closed."
    }
}

function Get-LasalClassDatabaseRecord {
    param(
        [string]$DatabaseText,
        [string]$SourcePath,
        [string]$ClassName
    )

    $recordStart = $DatabaseText.IndexOf(
        $SourcePath,
        [StringComparison]::OrdinalIgnoreCase)
    if ($recordStart -lt 0) {
        throw "LASAL Classes.lcb record for $ClassName was not found."
    }

    $recordEnd = $DatabaseText.IndexOf(
        '.\Class\',
        $recordStart + $SourcePath.Length,
        [StringComparison]::OrdinalIgnoreCase)
    if ($recordEnd -lt 0) {
        $recordEnd = $DatabaseText.Length
    }

    return $DatabaseText.Substring($recordStart, $recordEnd - $recordStart)
}

function Assert-ExactLasalFunctionAbi {
    param(
        [string]$ClassBlock,
        [string]$FunctionName,
        [bool]$IsGlobal,
        [object[]]$Inputs,
        [object[]]$Outputs
    )

    $scopeToken = if ($IsGlobal) { ' GLOBAL' } else { '' }
    $escapedHeader = 'FUNCTION' + $scopeToken + ' ' +
        [regex]::Escape($FunctionName)
    $headerPattern = '(?m)^[ \t]*' + $escapedHeader + '[ \t]*\r?$'
    $declarationCount = [regex]::Matches(
        $ClassBlock,
        $headerPattern).Count
    if ($declarationCount -ne 1) {
        $scopeDescription = if ($IsGlobal) { 'global' } else { 'private' }
        throw ("LMCControlCommandService.$FunctionName $scopeDescription " +
            "declaration count is $declarationCount, expected one.")
    }

    $declaration = [regex]::Match(
        $ClassBlock,
        ('(?ms)^[ \t]*' + $escapedHeader + '[ \t]*\r?\n' +
         '.*?(?=^[ \t]*FUNCTION\b|^[ \t]*//Tables:)')).Value
    if ([string]::IsNullOrWhiteSpace($declaration)) {
        throw "LMCControlCommandService.$FunctionName declaration was not found."
    }

    $canonicalPattern = '\A\s*' + $escapedHeader + '\s*'
    if ($Inputs.Count -gt 0) {
        $canonicalPattern += 'VAR_INPUT\s*'
        foreach ($inputVariable in $Inputs) {
            $canonicalPattern += (
                [regex]::Escape($inputVariable.Name) + '\s*:\s*' +
                [regex]::Escape($inputVariable.Type) + '\s*;\s*')
        }
        $canonicalPattern += 'END_VAR\s*'
    }
    if ($Outputs.Count -gt 0) {
        $canonicalPattern += 'VAR_OUTPUT\s*'
        foreach ($outputVariable in $Outputs) {
            $canonicalPattern += (
                [regex]::Escape($outputVariable.Name) + '\s*:\s*' +
                [regex]::Escape($outputVariable.Type) + '\s*;\s*')
        }
        $canonicalPattern += 'END_VAR;\s*'
    }
    else {
        $canonicalPattern += ';\s*'
    }
    $canonicalPattern += '\z'

    if (-not [regex]::IsMatch($declaration, $canonicalPattern)) {
        throw ("LMCControlCommandService.$FunctionName declaration does not " +
            'match the exact ordered input/output ABI.')
    }
}

function Assert-NoCaseInsensitiveMemberShadowing {
    param(
        [string]$ClassSource,
        [string]$ClassName
    )

    $classDeclaration = [regex]::Match(
        $ClassSource,
        ('(?s)' + [regex]::Escape($ClassName) +
            '\s*:\s*CLASS(?<Members>.*?)//Functions:'))
    if (-not $classDeclaration.Success) {
        throw "$ClassName generated class member declaration was not found."
    }

    $implementationMarker = '//{{LSL_IMPLEMENTATION'
    $implementationIndex = $ClassSource.IndexOf(
        $implementationMarker,
        [StringComparison]::Ordinal)
    if ($implementationIndex -lt 0) {
        throw "$ClassName implementation marker was not found."
    }

    $declarationPattern = (
        '(?m)^[ \t]*' +
        '(?<Names>[A-Za-z_][A-Za-z0-9_]*' +
        '(?:[ \t]*,[ \t]*[A-Za-z_][A-Za-z0-9_]*)*)[ \t]*:')
    $memberNames = @{}
    foreach ($member in [regex]::Matches(
            $classDeclaration.Groups['Members'].Value,
            $declarationPattern)) {
        foreach ($memberNameValue in ($member.Groups['Names'].Value -split ',')) {
            $memberName = $memberNameValue.Trim()
            $memberNames[$memberName.ToLowerInvariant()] = $memberName
        }
    }

    $implementation = $ClassSource.Substring($implementationIndex)
    $functionHeaderPattern = (
        '(?m)^[ \t]*FUNCTION[^\r\n]*\b' +
        [regex]::Escape($ClassName) +
        '(?:::|\b)')
    $functionPattern = (
        '(?ms)^[ \t]*FUNCTION[^\r\n]*\b' +
        [regex]::Escape($ClassName) +
        '(?:::|\b)[^\r\n]*\r?\n.*?^[ \t]*END_FUNCTION[ \t]*;?[ \t]*$')
    $variableBlockHeaderPattern =
        '(?m)^[ \t]*VAR(?:_[A-Z_]+)?[ \t]*$'
    $variableBlockPattern =
        '(?ms)^[ \t]*VAR(?:_[A-Z_]+)?[ \t]*\r?$' +
        '\s*(?<Variables>.*?)^[ \t]*END_VAR[ \t]*;?[ \t]*$'
    $collisions = @()
    $functionHeaders = [regex]::Matches(
        $implementation,
        $functionHeaderPattern)
    $functions = [regex]::Matches(
        $implementation,
        $functionPattern)

    if ($functions.Count -eq 0 -or
        $functions.Count -ne $functionHeaders.Count) {
        throw (
            "$ClassName implementation function parsing is incomplete: " +
            "headers=$($functionHeaders.Count), blocks=$($functions.Count).")
    }

    $variableBlockCount = 0

    foreach ($function in $functions) {
        $functionName = [regex]::Match(
            $function.Value,
            '^[^\r\n]+').Value.Trim()
        $variableBlockHeaders = [regex]::Matches(
            $function.Value,
            $variableBlockHeaderPattern)
        $variableBlocks = [regex]::Matches(
            $function.Value,
            $variableBlockPattern)
        if ($variableBlocks.Count -ne $variableBlockHeaders.Count) {
            throw (
                "$functionName variable block parsing is incomplete: " +
                "headers=$($variableBlockHeaders.Count), " +
                "blocks=$($variableBlocks.Count).")
        }
        $variableBlockCount += $variableBlocks.Count

        foreach ($variableBlock in $variableBlocks) {
            foreach ($local in [regex]::Matches(
                    $variableBlock.Groups['Variables'].Value,
                    $declarationPattern)) {
                foreach ($localNameValue in ($local.Groups['Names'].Value -split ',')) {
                    $localName = $localNameValue.Trim()
                    $lookupName = $localName.ToLowerInvariant()
                    if ($memberNames.ContainsKey($lookupName)) {
                        $collisions += (
                            "$functionName local '$localName' shadows member " +
                            "'$($memberNames[$lookupName])'")
                    }
                }
            }
        }
    }

    if ($variableBlockCount -eq 0) {
        throw "$ClassName implementation variable blocks were not found."
    }

    if ($collisions.Count -ne 0) {
        throw (
            "$ClassName contains LASAL case-insensitive member shadowing: " +
            ($collisions -join '; '))
    }
}

$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$stPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
$commNetworkPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network\Comm_Network.lcn'
$commNetworkTablePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network\ONE_Comm_Network_Table.st'
$etherCatNetworkPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\EtherCAT_Network\EtherCAT_Network.lcn'
$motionNetworkPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Motion_Network\Motion_Network.lcn'
$motionNetworkTablePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Motion_Network\ONE_Motion_Network_Table.st'
$tcpServerRtPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\_TCPIPServer_RT\_TCPIPServer_RT.st'
$classDbPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb'
$protocolPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcProtocol.cs'
$adminProtocolPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcAdminProtocol.cs'
$diagnosticsProtocolPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsProtocol.cs'
$diagnosticsLatchPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCEcatInputLatch\LMCEcatInputLatch.st'
$diagnosticsServicePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
$recorderStorePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCRecorderStore\LMCRecorderStore.st'
$sdoExecutorPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCSdoExecutor\LMCSdoExecutor.st'
$controlCommandServicePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'
$projectPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcp'

$st = Get-Content -Raw -LiteralPath $stPath
$commNetwork = Get-Content -Raw -LiteralPath $commNetworkPath
$etherCatNetwork = Get-Content -Raw -LiteralPath $etherCatNetworkPath
$motionNetwork = Get-Content -Raw -LiteralPath $motionNetworkPath
$commNetworkTable = ''
$motionNetworkTable = ''
if (-not $SourceOnly) {
    foreach ($generatedNetworkTable in @(
            @{ Path = $commNetworkTablePath; Name = 'Comm_Network' },
            @{ Path = $motionNetworkTablePath; Name = 'Motion_Network' })) {
        if (-not (Test-Path -LiteralPath $generatedNetworkTable.Path -PathType Leaf)) {
            throw (
                "LASAL generated table for $($generatedNetworkTable.Name) is missing: " +
                "$($generatedNetworkTable.Path). Save the Object Network and complete a " +
                'successful LASAL Rebuild before running the full static contract; do not ' +
                'restore a stale table from Git.')
        }
    }
    $commNetworkTable = Get-Content -Raw -LiteralPath $commNetworkTablePath
    $motionNetworkTable = Get-Content -Raw -LiteralPath $motionNetworkTablePath
}
$tcpServerRt = Get-Content -Raw -LiteralPath $tcpServerRtPath
$classDbText = [Text.Encoding]::ASCII.GetString(
    [IO.File]::ReadAllBytes($classDbPath))
$protocol = Get-Content -Raw -LiteralPath $protocolPath
$adminProtocol = Get-Content -Raw -LiteralPath $adminProtocolPath
$diagnosticsProtocol = Get-Content -Raw -LiteralPath $diagnosticsProtocolPath
$diagnosticsLatch = Get-Content -Raw -LiteralPath $diagnosticsLatchPath
$diagnosticsService = Get-Content -Raw -LiteralPath $diagnosticsServicePath
$recorderStore = Get-Content -Raw -LiteralPath $recorderStorePath
$sdoExecutor = Get-Content -Raw -LiteralPath $sdoExecutorPath
$controlCommandService = Get-Content -Raw -LiteralPath $controlCommandServicePath
$project = Get-Content -Raw -LiteralPath $projectPath

[xml]$commNetworkXml = $commNetwork
[xml]$etherCatNetworkXml = $etherCatNetwork
[xml]$motionNetworkXml = $motionNetwork

$controlServiceClassBlock = [regex]::Match(
    $controlCommandService,
    '(?s)LMCControlCommandService\s*:\s*CLASS.*?END_CLASS;').Value
if ([string]::IsNullOrWhiteSpace($controlServiceClassBlock)) {
    throw 'LMCControlCommandService generated class declaration was not found.'
}
$controlServiceMetadataBlock = [regex]::Match(
    $controlCommandService,
    '(?s)<Class\s+.*?Name\s*=\s*"LMCControlCommandService".*?</Class>').Value
if ([string]::IsNullOrWhiteSpace($controlServiceMetadataBlock)) {
    throw 'LMCControlCommandService generated class metadata was not found.'
}

foreach ($classProperty in @(
    @{ Name = 'RealtimeTask'; Value = 'false' },
    @{ Name = 'CyclicTask'; Value = 'false' },
    @{ Name = 'BackgroundTask'; Value = 'false' },
    @{ Name = 'Automatic'; Value = 'false' },
    @{ Name = 'SharedCommandTable'; Value = 'true' })) {
    Assert-Match $controlCommandService (
        [regex]::Escape($classProperty.Name) +
        '\s*=\s*"' + [regex]::Escape($classProperty.Value) + '"') (
        "LMCControlCommandService.$($classProperty.Name) must be $($classProperty.Value).")
}

Assert-Match $controlServiceClassBlock '(?m)^\s*ClassSvr\s*:\s*SvrChCmd_DINT\s*;\s*$' 'LMCControlCommandService.ClassSvr command server declaration is missing.'
foreach ($axisNumber in 1..9) {
    $axisClientName = "LMCAxis$axisNumber"
    Assert-Match $controlServiceClassBlock (
        '(?m)^\s*' + [regex]::Escape($axisClientName) +
        '\s*:\s*CltChCmd__LMCAxis\s*;\s*$') (
        "LMCControlCommandService.$axisClientName must be an _LMCAxis object command client.")
    Assert-Match $controlServiceMetadataBlock (
        '<Client\s+Name="' + [regex]::Escape($axisClientName) +
        '"\s+Required="true"\s+Internal="false"\s*/>') (
        "LMCControlCommandService.$axisClientName must be generated as a required external client.")
}
Assert-Match $controlServiceClassBlock '(?m)^\s*LMCRobot\s*:\s*CltChCmd__LMCRobotBase\s*;\s*$' 'LMCControlCommandService.LMCRobot must be an _LMCRobotBase object command client.'
Assert-Match $controlServiceMetadataBlock '<Client\s+Name="LMCRobot"\s+Required="true"\s+Internal="false"\s*/>' 'LMCControlCommandService.LMCRobot must be generated as a required external client.'
$controlServiceMetadataClients = [regex]::Matches(
    $controlServiceMetadataBlock,
    '<Client\s+Name="[^"]+"[^>]*/>')
if ($controlServiceMetadataClients.Count -ne 10) {
    throw "LMCControlCommandService metadata client count is $($controlServiceMetadataClients.Count), expected ten."
}

$controlServiceTableBlock = [regex]::Match(
    $controlCommandService,
    '(?s)FUNCTION GLOBAL TAB LMCControlCommandService::@CT_.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($controlServiceTableBlock)) {
    throw 'LMCControlCommandService generated command table was not found.'
}
Assert-Match $controlServiceTableBlock '(?m)^\s*1\$UINT,\s*10\$UINT,\s*0\$UINT,\s*$' 'LMCControlCommandService generated server/client/data counts are not exactly 1/10/0.'

$controlServiceServerEntries = [regex]::Matches(
    $controlServiceTableBlock,
    '\(::LMCControlCommandService\.[A-Za-z_][A-Za-z0-9_]*\.pMeth\)\$UINT')
if ($controlServiceServerEntries.Count -ne 1) {
    throw "LMCControlCommandService generated server entry count is $($controlServiceServerEntries.Count), expected one."
}
Assert-Match $controlServiceTableBlock '(?m)^\s*\(::LMCControlCommandService\.ClassSvr\.pMeth\)\$UINT,\s*_CH_CMD\$UINT,.*"ClassSvr"' 'LMCControlCommandService.ClassSvr generated metadata is missing.'

$controlServiceClientLines = [regex]::Matches(
    $controlServiceTableBlock,
    '(?m)^\s*\(::LMCControlCommandService\.(?<Name>[A-Za-z_][A-Za-z0-9_]*)\.pCh\)\$UINT.*$')
if ($controlServiceClientLines.Count -ne 10) {
    throw "LMCControlCommandService generated client entry count is $($controlServiceClientLines.Count), expected ten."
}
foreach ($clientLine in $controlServiceClientLines) {
    if ($clientLine.Value -notmatch
        '_CH_CLT_OBJ\$UINT,\s*2#0000000000000010\$UINT') {
        throw ("LMCControlCommandService.$($clientLine.Groups['Name'].Value) " +
            'is not generated as a required object client.')
    }
}
if ($controlServiceTableBlock -match '_CH_CLT\$UINT') {
    throw 'LMCControlCommandService contains a generated scalar client entry.'
}
foreach ($axisNumber in 1..9) {
    $axisClientName = "LMCAxis$axisNumber"
    Assert-Match $controlServiceTableBlock (
        '(?m)^\s*\(::LMCControlCommandService\.' +
        [regex]::Escape($axisClientName) +
        '\.pCh\)\$UINT,\s*_CH_CLT_OBJ\$UINT,\s*' +
        '2#0000000000000010\$UINT,.*"' +
        [regex]::Escape($axisClientName) + '".*"_LMCAxis"') (
        "LMCControlCommandService.$axisClientName generated object-client metadata is missing.")
}
Assert-Match $controlServiceTableBlock '(?m)^\s*\(::LMCControlCommandService\.LMCRobot\.pCh\)\$UINT,\s*_CH_CLT_OBJ\$UINT,\s*2#0000000000000010\$UINT,.*"LMCRobot".*"_LMCRobotBase"' 'LMCControlCommandService.LMCRobot required object-client metadata is missing.'

$controlServicePragmas = [regex]::Matches(
    $controlCommandService,
    '(?m)^\s*#pragma usingLtd\s+(?<Class>[A-Za-z_][A-Za-z0-9_]*)\s*$')
if ($controlServicePragmas.Count -ne 2 -or
    @($controlServicePragmas | Where-Object {
            $_.Groups['Class'].Value -eq '_LMCAxis' }).Count -ne 1 -or
    @($controlServicePragmas | Where-Object {
            $_.Groups['Class'].Value -eq '_LMCRobotBase' }).Count -ne 1) {
    throw 'LMCControlCommandService must have exactly the _LMCAxis and _LMCRobotBase limited-using pragmas.'
}
if ($controlCommandService -match '(?:#pragma usingLtd\s+_StdLib|\b_StdLib\b)') {
    throw 'LMCControlCommandService must not depend on an _StdLib client.'
}

$controlServiceRequestInputs = @(
    @{ Name = 'CommandId'; Type = 'UINT' },
    @{ Name = 'Reference'; Type = 'UINT' },
    @{ Name = 'pRequestFrame'; Type = '^USINT' },
    @{ Name = 'RequestFrameSize'; Type = 'UDINT' },
    @{ Name = 'pResponseFrame'; Type = '^USINT' },
    @{ Name = 'ResponseCapacity'; Type = 'UDINT' })
$controlServiceResponseOutput = @(
    @{ Name = 'ResponseSize'; Type = 'DINT' })

Assert-ExactLasalFunctionAbi `
    -ClassBlock $controlServiceClassBlock `
    -FunctionName 'HandleRequest' `
    -IsGlobal $true `
    -Inputs $controlServiceRequestInputs `
    -Outputs $controlServiceResponseOutput

$controlServicePrivateMethods = @(
    'HandleAdminCommands',
    'HandleRegistryCommands',
    'HandleAxisCommands',
    'HandleGroupCommands',
    'MoveLinearAbsEx',
    'GroupReadStatus')
foreach ($methodName in $controlServicePrivateMethods[0..3]) {
    Assert-ExactLasalFunctionAbi `
        -ClassBlock $controlServiceClassBlock `
        -FunctionName $methodName `
        -IsGlobal $false `
        -Inputs $controlServiceRequestInputs `
        -Outputs $controlServiceResponseOutput
}

$moveLinearAbsExInputs = @(
    @{ Name = 'Reference'; Type = 'UINT' },
    @{ Name = 'pResponseFrame'; Type = '^USINT' },
    @{ Name = 'ResponseCapacity'; Type = 'UDINT' },
    @{ Name = 'pRequestFrame'; Type = '^USINT' },
    @{ Name = 'RequestFrameSize'; Type = 'UDINT' })
Assert-ExactLasalFunctionAbi `
    -ClassBlock $controlServiceClassBlock `
    -FunctionName 'MoveLinearAbsEx' `
    -IsGlobal $false `
    -Inputs $moveLinearAbsExInputs `
    -Outputs $controlServiceResponseOutput

$groupReadStatusInputs = @(
    @{ Name = 'pResponseFrame'; Type = '^USINT' },
    @{ Name = 'ResponseCapacity'; Type = 'UDINT' })
Assert-ExactLasalFunctionAbi `
    -ClassBlock $controlServiceClassBlock `
    -FunctionName 'GroupReadStatus' `
    -IsGlobal $false `
    -Inputs $groupReadStatusInputs `
    -Outputs $controlServiceResponseOutput

$controlServiceClassDbRecord = Get-LasalClassDatabaseRecord `
    -DatabaseText $classDbText `
    -SourcePath '.\Class\LMCControlCommandService\LMCControlCommandService.st' `
    -ClassName 'LMCControlCommandService'
foreach ($generatedMemberName in @(
        'HandleRequest',
        'HandleAdminCommands',
        'HandleRegistryCommands',
        'HandleAxisCommands',
        'HandleGroupCommands',
        'MoveLinearAbsEx',
        'GroupReadStatus')) {
    Assert-Match $controlServiceClassDbRecord (
        '(?<![A-Za-z0-9_])' + [regex]::Escape($generatedMemberName) +
        '(?![A-Za-z0-9_])') (
        "LASAL Classes.lcb LMCControlCommandService record is missing $generatedMemberName.")
}
$tcpClassDbRecord = Get-LasalClassDatabaseRecord `
    -DatabaseText $classDbText `
    -SourcePath '.\Class\TCPMotionInterface\TCPMotionInterface.st' `
    -ClassName 'TCPMotionInterface'
Assert-Match $tcpClassDbRecord '(?<![A-Za-z0-9_])ControlCommands(?![A-Za-z0-9_])' 'LASAL Classes.lcb TCPMotionInterface record is missing ControlCommands.'

$controlServiceHandleRequestBlock = [regex]::Match(
    $controlCommandService,
    '(?s)FUNCTION GLOBAL LMCControlCommandService::HandleRequest.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($controlServiceHandleRequestBlock)) {
    throw 'LMCControlCommandService.HandleRequest implementation was not found.'
}
$controlServicePrivateBlocks = [ordered]@{}
foreach ($methodName in $controlServicePrivateMethods) {
    $privateMethodBlock = [regex]::Match(
        $controlCommandService,
        ('(?s)FUNCTION LMCControlCommandService::' +
         [regex]::Escape($methodName) + '.*?END_FUNCTION')).Value
    if ([string]::IsNullOrWhiteSpace($privateMethodBlock)) {
        throw "LMCControlCommandService.$methodName implementation was not found."
    }
    $controlServicePrivateBlocks[$methodName] = $privateMethodBlock
}
$controlServiceMethodBlocks = [ordered]@{
    HandleRequest = $controlServiceHandleRequestBlock
}
foreach ($methodName in $controlServicePrivateMethods) {
    $controlServiceMethodBlocks[$methodName] =
        $controlServicePrivateBlocks[$methodName]
}
foreach ($methodEntry in $controlServiceMethodBlocks.GetEnumerator()) {
    $methodByteCount = [Text.Encoding]::UTF8.GetByteCount($methodEntry.Value)
    if ($methodByteCount -gt 32768) {
        throw ("LMCControlCommandService.$($methodEntry.Key) is " +
            "$methodByteCount bytes, expected at most 32768.")
    }
}

$phase3GroupCommandIds = @(
    '20D2',
    '2047',
    '2048',
    '2049',
    '204A',
    '204B',
    '2085',
    '20A4',
    '2045',
    '2051',
    '20E7')
$phase3AdminCommandIds = @('7D20', '7D22')

switch ($ControlServiceCheckpoint) {
    'Phase2Skeleton' {
        Assert-LasalFailClosedBody `
            -FunctionBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest' `
            -Checkpoint $ControlServiceCheckpoint
        foreach ($methodName in $controlServicePrivateMethods) {
            Assert-LasalFailClosedBody `
                -FunctionBlock $controlServicePrivateBlocks[$methodName] `
                -Owner "LMCControlCommandService.$methodName" `
                -Checkpoint $ControlServiceCheckpoint
        }
    }

    'Phase3GroupDormant' {
        Assert-LasalFailClosedBody `
            -FunctionBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest' `
            -Checkpoint $ControlServiceCheckpoint
        foreach ($methodName in @(
                'HandleRegistryCommands',
                'HandleAxisCommands')) {
            Assert-LasalFailClosedBody `
                -FunctionBlock $controlServicePrivateBlocks[$methodName] `
                -Owner "LMCControlCommandService.$methodName" `
                -Checkpoint $ControlServiceCheckpoint
        }
        foreach ($methodName in @(
                'HandleGroupCommands',
                'HandleAdminCommands',
                'MoveLinearAbsEx',
                'GroupReadStatus')) {
            Assert-LasalImplementedBody `
                -FunctionBlock $controlServicePrivateBlocks[$methodName] `
                -Owner "LMCControlCommandService.$methodName" `
                -Checkpoint $ControlServiceCheckpoint
        }
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServicePrivateBlocks['HandleGroupCommands'] `
            -Owner 'LMCControlCommandService.HandleGroupCommands' `
            -ExpectedCommandIds $phase3GroupCommandIds
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServicePrivateBlocks['HandleAdminCommands'] `
            -Owner 'LMCControlCommandService.HandleAdminCommands' `
            -ExpectedCommandIds $phase3AdminCommandIds
    }

    'Phase3GroupRouted' {
        Assert-LasalImplementedBody `
            -FunctionBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest' `
            -Checkpoint $ControlServiceCheckpoint
        foreach ($methodName in @(
                'HandleRegistryCommands',
                'HandleAxisCommands')) {
            Assert-LasalFailClosedBody `
                -FunctionBlock $controlServicePrivateBlocks[$methodName] `
                -Owner "LMCControlCommandService.$methodName" `
                -Checkpoint $ControlServiceCheckpoint
        }
        foreach ($methodName in @(
                'HandleGroupCommands',
                'HandleAdminCommands',
                'MoveLinearAbsEx',
                'GroupReadStatus')) {
            Assert-LasalImplementedBody `
                -FunctionBlock $controlServicePrivateBlocks[$methodName] `
                -Owner "LMCControlCommandService.$methodName" `
                -Checkpoint $ControlServiceCheckpoint
        }
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest' `
            -ExpectedCommandIds ($phase3GroupCommandIds + $phase3AdminCommandIds)
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServicePrivateBlocks['HandleGroupCommands'] `
            -Owner 'LMCControlCommandService.HandleGroupCommands' `
            -ExpectedCommandIds $phase3GroupCommandIds
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServicePrivateBlocks['HandleAdminCommands'] `
            -Owner 'LMCControlCommandService.HandleAdminCommands' `
            -ExpectedCommandIds $phase3AdminCommandIds
        Assert-ExactLasalCommandRouteIds `
            -RouterBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest group ownership' `
            -CallPattern 'ResponseSize\s*:=\s*HandleGroupCommands\s*\(' `
            -ExpectedCommandIds $phase3GroupCommandIds
        Assert-ExactLasalCommandRouteIds `
            -RouterBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest Admin ownership' `
            -CallPattern 'ResponseSize\s*:=\s*HandleAdminCommands\s*\(' `
            -ExpectedCommandIds $phase3AdminCommandIds
        foreach ($handlerName in @(
                'HandleGroupCommands',
                'HandleAdminCommands')) {
            $handlerCallCount = [regex]::Matches(
                $controlServiceHandleRequestBlock,
                ('(?<![A-Za-z0-9_.])' +
                 [regex]::Escape($handlerName) + '\s*\(')).Count
            if ($handlerCallCount -ne 1) {
                throw (
                    "$ControlServiceCheckpoint LMCControlCommandService." +
                    "HandleRequest $handlerName call count is " +
                    "$handlerCallCount, expected one.")
            }
            Assert-Match $controlServiceHandleRequestBlock (
                '(?s)ResponseSize\s*:=\s*' +
                [regex]::Escape($handlerName) + '\(\s*' +
                'CommandId:=CommandId\s*,\s*' +
                'Reference:=Reference\s*,\s*' +
                'pRequestFrame:=pRequestFrame\s*,\s*' +
                'RequestFrameSize:=RequestFrameSize\s*,\s*' +
                'pResponseFrame:=pResponseFrame\s*,\s*' +
                'ResponseCapacity:=ResponseCapacity\s*\)') (
                "$ControlServiceCheckpoint HandleRequest does not pass the " +
                "complete zero-copy ABI to $handlerName.")
        }
        foreach ($handlerName in @(
                'HandleRegistryCommands',
                'HandleAxisCommands')) {
            if ($controlServiceHandleRequestBlock -match (
                    '(?<![A-Za-z0-9_.])' +
                    [regex]::Escape($handlerName) + '\s*\(')) {
                throw (
                    "$ControlServiceCheckpoint LMCControlCommandService." +
                    "HandleRequest already routes to $handlerName.")
            }
        }
        Assert-Match $controlServiceHandleRequestBlock (
            '(?s)ResponseSize\s*:=\s*-1\s*;.*?' +
            'if\s+\(pRequestFrame\s*=\s*NIL\)\s*\|\s*' +
            '\(pResponseFrame\s*=\s*NIL\)\s*\|\s*' +
            '\(RequestFrameSize\s*<\s*8\)\s+then\s*RETURN;\s*end_if;.*?' +
            'case\s+CommandId\s+of.*?' +
            'else\s+ResponseSize\s*:=\s*-1\s*;\s*end_case') (
            'Phase3GroupRouted HandleRequest unsupported-command fail-closed path is missing.')
    }
}

if ($ControlServiceCheckpoint -ne 'Phase3GroupRouted') {
    foreach ($methodName in $controlServicePrivateMethods) {
        if ($controlServiceHandleRequestBlock -match (
                '(?<![A-Za-z0-9_.])' +
                [regex]::Escape($methodName) + '\s*\(')) {
            throw (
                "$ControlServiceCheckpoint LMCControlCommandService." +
                "HandleRequest already routes to $methodName.")
        }
    }
    if ($controlServiceHandleRequestBlock -match '(?i)\bcase\s+CommandId\b') {
        throw (
            "$ControlServiceCheckpoint LMCControlCommandService." +
            'HandleRequest must remain dormant without command routing.')
    }
}

$controlServiceOwnedSource = $controlServiceClassBlock + "`n" +
    $controlCommandService.Substring(
        $controlCommandService.IndexOf('//{{LSL_IMPLEMENTATION', [StringComparison]::Ordinal))
$forbiddenControlServiceStatePattern = (
    '(?i)(?:_TCPIPServer|_TCPMI_|sigclib_atomic_|' +
    '\b(?:SendData|CurrentSock|ClientFd|Socket|RequestQueue|RequestBuf|' +
    'ReceiveBuf|Sendbuf|SessionEpoch|Ingress|NotifySessionClosed|CyWork|' +
    'RtWork|BackgroundWork|CyclicCall)\b)')
if ($controlServiceOwnedSource -match $forbiddenControlServiceStatePattern) {
    throw "LMCControlCommandService owns forbidden transport/task state '$($Matches[0])'."
}

$controlServiceRegistrationPattern = '<File\s+Path="\.\\Class\\LMCControlCommandService\\LMCControlCommandService\.st"\s*/>'
$controlServiceRegistrationCount = [regex]::Matches(
    $project,
    $controlServiceRegistrationPattern,
    [Text.RegularExpressions.RegexOptions]::IgnoreCase).Count
if ($controlServiceRegistrationCount -ne 1) {
    throw "Elmo_EtherCAT_Test_4Axis.lcp LMCControlCommandService registration count is $controlServiceRegistrationCount, expected one."
}

$commRecorderStoreObjects = @(
    $commNetworkXml.SelectNodes("//Object[@Name='LMCRecorderStore1']"))
$motionRecorderStoreObjects = @(
    $motionNetworkXml.SelectNodes("//Object[@Name='LMCRecorderStore1']"))
if ($commRecorderStoreObjects.Count -ne 0 -or
    $motionRecorderStoreObjects.Count -ne 1 -or
    $motionRecorderStoreObjects[0].Class -ne 'LMCRecorderStore') {
    throw ('LMCRecorderStore1 must exist exactly once as LMCRecorderStore in ' +
        "Motion_Network: motion=$($motionRecorderStoreObjects.Count), " +
        "comm=$($commRecorderStoreObjects.Count).")
}

$recorderStoreConnections = @(
    $commNetworkXml.SelectNodes("//Connection[@Destination='LMCRecorderStore1.ClassSvr']")
    $motionNetworkXml.SelectNodes("//Connection[@Destination='LMCRecorderStore1.ClassSvr']"))
if ($recorderStoreConnections.Count -ne 2) {
    throw "LMCRecorderStore1 client connection count is $($recorderStoreConnections.Count), expected exactly two."
}
$recorderConnectionSources = @(
    $recorderStoreConnections | ForEach-Object { $_.Source })
foreach ($expectedRecorderSource in @(
    'LMCEcatInputLatch1.RecorderStore',
    'LMCDiagnosticsService1.RecorderStore')) {
    if (@($recorderConnectionSources | Where-Object {
                $_ -eq $expectedRecorderSource }).Count -ne 1) {
        throw "Missing or duplicate $expectedRecorderSource -> LMCRecorderStore1.ClassSvr connection."
    }
}
if (@($motionNetworkXml.SelectNodes(
            "//Connection[@Source='LMCEcatInputLatch1.RecorderStore' and " +
            "@Destination='LMCRecorderStore1.ClassSvr']")).Count -ne 1 -or
    @($commNetworkXml.SelectNodes(
            "//Connection[@Source='LMCDiagnosticsService1.RecorderStore' and " +
            "@Destination='LMCRecorderStore1.ClassSvr']")).Count -ne 1) {
    throw 'RecorderStore client connections are not in their required Motion/Comm networks.'
}

$tcpCommandTableBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION GLOBAL TAB TCPMotionInterface::@CT_.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($tcpCommandTableBlock)) {
    throw 'TCPMotionInterface generated command table was not found.'
}
Assert-Match $tcpCommandTableBlock '(?m)^\s*20\$UINT,\s*13\$UINT,\s*0\$UINT,\s*$' 'TCPMotionInterface generated client count is not 13.'

$clientEntries = [regex]::Matches(
    $tcpCommandTableBlock,
    '\(::TCPMotionInterface\.(LMCAxis[1-9]|LMCRobot|_StdLib|Diagnostics|ControlCommands)\.pCh\)\$UINT').Count
if ($clientEntries -ne 13) {
    throw "TCPMotionInterface generated client entry count is $clientEntries, expected 13."
}

Assert-Match $tcpCommandTableBlock '\(::TCPMotionInterface\.Diagnostics\.pCh\)\$UINT.*"Diagnostics".*"LMCDiagnosticsService"' 'TCPMotionInterface Diagnostics client metadata is missing.'
Assert-Match $st '(?m)^\s*ControlCommands\s*:\s*CltChCmd_LMCControlCommandService\s*;\s*$' 'TCPMotionInterface.ControlCommands object command client declaration is missing.'
Assert-Match $st '<Client\s+Name="ControlCommands"\s+Required="true"\s+Internal="false"\s*/>' 'TCPMotionInterface.ControlCommands must be generated as a required external client.'
Assert-Match $tcpCommandTableBlock '\(::TCPMotionInterface\.ControlCommands\.pCh\)\$UINT,\s*_CH_CLT_OBJ\$UINT,\s*2#0000000000000010\$UINT,.*"ControlCommands".*"LMCControlCommandService"' 'TCPMotionInterface.ControlCommands required object-client metadata is missing.'
Assert-Match $st '(?m)^\s*#pragma usingLtd LMCControlCommandService\s*$' 'TCPMotionInterface LMCControlCommandService limited-using pragma is missing.'
$controlServiceCallCount = [regex]::Matches(
    $st,
    'ControlCommands\s*\.\s*HandleRequest\s*\(').Count
$expectedControlServiceCallCount = if (
    $ControlServiceCheckpoint -eq 'Phase3GroupRouted') { 1 } else { 0 }
if ($controlServiceCallCount -ne $expectedControlServiceCallCount) {
    throw (
        "$ControlServiceCheckpoint TCPMotionInterface " +
        "ControlCommands.HandleRequest call count is $controlServiceCallCount, " +
        "expected $expectedControlServiceCallCount.")
}

foreach ($axisNumber in 1..9) {
    $clientName = "LMCAxis$axisNumber"
    $linkPattern = [regex]::Escape("TCPMotionInterface1.$clientName") +
        '.*' +
        [regex]::Escape("_LMCAxis$axisNumber.Control")
    Assert-Match $commNetwork $linkPattern "Missing $clientName -> _LMCAxis$axisNumber.Control link in Comm_Network."
}

if (-not $SourceOnly) {
    $interfaceObject = $commNetworkXml.SelectSingleNode("//Object[@Name='TCPMotionInterface1']")
    $serverObject = $commNetworkXml.SelectSingleNode("//Object[@Name='_TCPIPServer1']")
    if ($null -eq $interfaceObject -or $null -eq $serverObject) {
        throw 'TCPMotionInterface1 or _TCPIPServer1 network object is missing.'
    }
    if ($interfaceObject.HasAttribute('RealTime')) {
        throw 'TCPMotionInterface1 must not have a RealTime task assignment.'
    }
    if ($interfaceObject.CyclicTime -ne '1 ms') {
        throw 'TCPMotionInterface1.CyclicTime must be 1 ms.'
    }
    $configClient = $serverObject.SelectSingleNode("./Channels/Client[@Name='Config']")
    $maxConnectionsClient = $serverObject.SelectSingleNode("./Channels/Client[@Name='MaxConnections']")
    if ($null -eq $configClient -or $configClient.Value -ne '0') {
        throw '_TCPIPServer1.Config must be explicitly set to 0.'
    }
    if ($null -eq $maxConnectionsClient -or $maxConnectionsClient.Value -ne '1') {
        throw '_TCPIPServer1.MaxConnections must be explicitly set to 1.'
    }
    Assert-Match $commNetwork 'TCPMotionInterface1\._TCPIPServer.*_TCPIPServer1\.Control' 'TCPMotionInterface1 is not connected to the ordinary TCP server in Comm_Network.'

    $commControlServiceObjects = @(
        $commNetworkXml.SelectNodes(
            "/Network/Components/Object[@Name='LMCControlCommandService1' and " +
            "@Class='LMCControlCommandService']"))
    $allControlServiceObjects = @(
        $commNetworkXml.SelectNodes(
            "/Network/Components/Object[@Name='LMCControlCommandService1' or " +
            "@Class='LMCControlCommandService']")
        $motionNetworkXml.SelectNodes(
            "/Network/Components/Object[@Name='LMCControlCommandService1' or " +
            "@Class='LMCControlCommandService']")
        $etherCatNetworkXml.SelectNodes(
            "/Network/Components/Object[@Name='LMCControlCommandService1' or " +
            "@Class='LMCControlCommandService']"))
    if ($commControlServiceObjects.Count -ne 1 -or
        $allControlServiceObjects.Count -ne 1) {
        throw ('LMCControlCommandService1 must exist exactly once as ' +
            'LMCControlCommandService in Comm_Network and nowhere else.')
    }
    $controlServiceObject = $commControlServiceObjects[0]
    foreach ($taskAttribute in @('RealTime', 'CyclicTime', 'BackgroundTime')) {
        if ($controlServiceObject.HasAttribute($taskAttribute)) {
            throw ("LMCControlCommandService1 must not own a scheduled task; " +
                "$taskAttribute is present.")
        }
    }

    $expectedControlServiceConnections = @(
        @{ Source = 'TCPMotionInterface1.ControlCommands'; Destination = 'LMCControlCommandService1.ClassSvr' })
    foreach ($axisNumber in 1..9) {
        $expectedControlServiceConnections += @{
            Source = "LMCControlCommandService1.LMCAxis$axisNumber"
            Destination = "_LMCAxis$axisNumber.Control"
        }
    }
    $expectedControlServiceConnections += @{
        Source = 'LMCControlCommandService1.LMCRobot'
        Destination = '_LMCRobotBase1.Control'
    }
    foreach ($expectedConnection in $expectedControlServiceConnections) {
        $source = $expectedConnection.Source
        $destination = $expectedConnection.Destination
        $connections = @(
            $commNetworkXml.SelectNodes(
                "/Network/Connections/Connection[@Source='$source' and " +
                "@Destination='$destination']"))
        if ($connections.Count -ne 1) {
            throw "Missing or duplicate $source -> $destination connection in Comm_Network."
        }
    }
    $controlServiceOutgoingConnections = @(
        $commNetworkXml.SelectNodes(
            "/Network/Connections/Connection[starts-with(@Source," +
            "'LMCControlCommandService1.') ]"))
    if ($controlServiceOutgoingConnections.Count -ne 10) {
        throw ("LMCControlCommandService1 outgoing connection count is " +
            "$($controlServiceOutgoingConnections.Count), expected exactly ten.")
    }
    $controlServiceServerConnections = @(
        $commNetworkXml.SelectNodes(
            "/Network/Connections/Connection[" +
            "@Destination='LMCControlCommandService1.ClassSvr']"))
    if ($controlServiceServerConnections.Count -ne 1) {
        throw ("LMCControlCommandService1.ClassSvr connection count is " +
            "$($controlServiceServerConnections.Count), expected exactly one.")
    }

    Assert-Match $commNetworkTable '(?m)^\s*TO_UDINT\(\d+\),\s*"LMCControlCommandService",.*$' 'Comm_Network generated table is stale: LMCControlCommandService class metadata is missing.'
    Assert-Match $commNetworkTable '(?m)^\s*_NO_ATTR,\s*TO_UDINT\(\d+\),\s*"LMCCONTROLCOMMANDSERVICE1",\s*$' 'Comm_Network generated table is stale: LMCControlCommandService1 object metadata is missing.'
    Assert-Match $commNetworkTable '(?m)^\s*TO_UDINT\(\d+\),\s*"ControlCommands",\s*TO_UDINT\(\d+\),\s*"ClassSvr",\s*$' 'Comm_Network generated table is stale: ControlCommands internal connection is missing.'
    foreach ($axisNumber in 1..9) {
        $generatedAxisConnectionPattern = (
            '(?m)^\s*TO_UDINT\(\d+\),\s*"LMCAxis' + $axisNumber +
            '",\s*C_DIR,\s*TO_UDINT\(\d+\),\s*"_LMCAxis' + $axisNumber +
            '",\s*"Control",\s*$')
        $generatedAxisConnectionCount = [regex]::Matches(
            $commNetworkTable,
            $generatedAxisConnectionPattern).Count
        if ($generatedAxisConnectionCount -ne 2) {
            throw ("Comm_Network generated LMCAxis$axisNumber connection count is " +
                "$generatedAxisConnectionCount, expected two retained TCP/service links.")
        }
    }
    $generatedRobotConnectionCount = [regex]::Matches(
        $commNetworkTable,
        '(?m)^\s*TO_UDINT\(\d+\),\s*"LMCRobot",\s*C_DIR,\s*TO_UDINT\(\d+\),\s*"_LMCRobotBase1",\s*"Control",\s*$').Count
    if ($generatedRobotConnectionCount -ne 2) {
        throw ("Comm_Network generated LMCRobot connection count is " +
            "$generatedRobotConnectionCount, expected two retained TCP/service links.")
    }
    $generatedTaskBlock = [regex]::Match(
        $commNetworkTable,
        '(?s)//Configuration of tasks \(RealTime, Cyclic, Background\).*?(?=//External connections)').Value
    if ([string]::IsNullOrWhiteSpace($generatedTaskBlock)) {
        throw 'Comm_Network generated task configuration block was not found.'
    }
    if ($generatedTaskBlock -match 'LMCCONTROLCOMMANDSERVICE1') {
        throw 'Comm_Network generated table assigns a task to LMCControlCommandService1.'
    }

    $diagnosticsServiceObject = $commNetworkXml.SelectSingleNode("//Object[@Name='LMCDiagnosticsService1']")
    $diagnosticsLatchObject = $motionNetworkXml.SelectSingleNode("/Network/Components/Object[@Name='LMCEcatInputLatch1']")
    if ($null -eq $diagnosticsServiceObject -or $diagnosticsServiceObject.Class -ne 'LMCDiagnosticsService') {
        throw 'LMCDiagnosticsService1 network object is missing from Comm_Network.'
    }
    Assert-Match $classDbText 'DiagnosticsBootCounter' 'Classes.lcb metadata is missing DiagnosticsBootCounter. Reload and save LMCDiagnosticsService through LASAL IDE.'
    Assert-Match $classDbText 'GetDiagnosticsBootId' 'Classes.lcb metadata is missing GetDiagnosticsBootId. Reload and save LMCDiagnosticsService through LASAL IDE.'
    $diagnosticsBootCounterServer = $diagnosticsServiceObject.SelectSingleNode(
        "./Channels/Server[@Name='DiagnosticsBootCounter']")
    if ($null -eq $diagnosticsBootCounterServer -or
        $diagnosticsBootCounterServer.Value -ne '0') {
        throw 'LMCDiagnosticsService1.DiagnosticsBootCounter network initialization is missing.'
    }
    Assert-Match $commNetworkTable '"DiagnosticsBootCounter",\s*TO_UDINT\(0\),//\|Comm_Network\.LMCDiagnosticsService1\.DiagnosticsBootCounter;' 'LMCDiagnosticsService1 generated DiagnosticsBootCounter initialization is stale in Comm_Network.'
    if ($null -eq $diagnosticsLatchObject -or $diagnosticsLatchObject.Class -ne 'LMCEcatInputLatch') {
        throw 'LMCEcatInputLatch1 network object is missing from Motion_Network.'
    }
    if ($diagnosticsLatchObject.HasAttribute('RealTime') -or
        $diagnosticsLatchObject.HasAttribute('CyclicTime') -or
        $diagnosticsLatchObject.HasAttribute('BackgroundTime')) {
        throw 'LMCEcatInputLatch1 must not own an independent scheduled task.'
    }
    $diagnosticsLatchTriggerConnections = @(
        $motionNetworkXml.SelectNodes(
            "//Connection[@Source='_LMCAxis1.LMCPreRtWorkTrigger' and " +
            "@Destination='LMCEcatInputLatch1.ClassSvr']"))
    if ($diagnosticsLatchTriggerConnections.Count -ne 1) {
        throw ('LMCEcatInputLatch1 must have exactly one ' +
            '_LMCAxis1.LMCPreRtWorkTrigger connection for same-cycle ordering.')
    }
    $diagnosticsNetworkText = $commNetwork + "`n" + $motionNetwork
    foreach ($link in @(
        'TCPMotionInterface1.Diagnostics.*LMCDiagnosticsService1.ClassSvr',
        'LMCDiagnosticsService1.InputLatch.*LMCEcatInputLatch1.ClassSvr',
        'LMCEcatInputLatch1.EcatMaster.*EtherCAT_PLC1.ClassState',
        'LMCEcatInputLatch1.Drive1.*Elmo_11.ClassState',
        'LMCEcatInputLatch1.Drive2.*Elmo_21.ClassState',
        'LMCEcatInputLatch1.Drive3.*Elmo_31.ClassState',
        'LMCEcatInputLatch1.Drive4.*Elmo_41.ClassState')) {
        Assert-Match $diagnosticsNetworkText $link "Missing diagnostics network link matching $link."
    }

    $sdoExecutorObjects = @(
        $etherCatNetworkXml.SelectNodes(
            "/Network/Components/Object[@Class='LMCSdoExecutor']"))
    if ($sdoExecutorObjects.Count -ne 4) {
        throw "EtherCAT_Network LMCSdoExecutor object count is $($sdoExecutorObjects.Count), expected exactly four."
    }
    $rawSdoBaseObjects = @(
        $etherCatNetworkXml.SelectNodes(
            "/Network/Components/Object[@Class='EtherCAT_SDOBase']"))
    if ($rawSdoBaseObjects.Count -ne 0) {
        throw ('EtherCAT_Network still contains production EtherCAT_SDOBase ' +
            "objects=$($rawSdoBaseObjects.Count); replace them with LMCSdoExecutor instances.")
    }
    foreach ($sdoAxis in 1..4) {
        $executorName = "LMCSdoExecutor$sdoAxis"
        $driveName = "Elmo_$($sdoAxis)1"
        $executorObjectsForAxis = @(
            $etherCatNetworkXml.SelectNodes(
                "/Network/Components/Object[@Name='$executorName' and " +
                "@Class='LMCSdoExecutor']"))
        if ($executorObjectsForAxis.Count -ne 1) {
            throw "$executorName must exist exactly once as LMCSdoExecutor in EtherCAT_Network."
        }
        $executorObject = $executorObjectsForAxis[0]
        $executorRemotely = $executorObject.GetAttribute('Remotely')
        if ($executorObject.GetAttribute('Visualized') -ne 'false' -or
            ($executorRemotely -ne '' -and $executorRemotely -ne 'false')) {
            throw "$executorName must set Visualized=false and Remotely=false."
        }

        $slaveConnections = @(
            $etherCatNetworkXml.SelectNodes(
                "/Network/Connections/Connection[" +
                "@Source='$executorName.toSlave' and " +
                "@Destination='$driveName.ClassState']"))
        if ($slaveConnections.Count -ne 1) {
            throw "Missing or duplicate $executorName.toSlave -> $driveName.ClassState connection in EtherCAT_Network."
        }

        $sdoClientName = "SdoAxis$sdoAxis"
        $sdoClient = $diagnosticsServiceObject.SelectSingleNode(
            "./Channels/Client[@Name='$sdoClientName']")
        if ($null -eq $sdoClient) {
            throw "LMCDiagnosticsService1.$sdoClientName client is missing from Comm_Network."
        }
        $serviceConnections = @(
            $commNetworkXml.SelectNodes(
                "/Network/Connections/Connection[" +
                "@Source='LMCDiagnosticsService1.$sdoClientName' and " +
                "@Destination='$executorName.ClassState']"))
        if ($serviceConnections.Count -ne 1) {
            throw ("Missing or duplicate LMCDiagnosticsService1.$sdoClientName " +
                "-> $executorName.ClassState cross-network connection in Comm_Network.")
        }
    }
    if (@($etherCatNetworkXml.SelectNodes(
                "/Network/Connections/Connection[starts-with(@Source,'LMCSdoExecutor') " +
                "and substring-after(@Source,'.')='toSlave']")).Count -ne 4) {
        throw 'EtherCAT_Network must contain exactly four LMCSdoExecutor.toSlave connections.'
    }
    if (@($etherCatNetworkXml.SelectNodes(
                "/Network/Connections/Connection[starts-with(@Source,'EtherCAT_SDOBase')]")).Count -ne 0) {
        throw 'EtherCAT_Network still contains legacy EtherCAT_SDOBase connections.'
    }
    if (@($commNetworkXml.SelectNodes(
                "/Network/Connections/Connection[starts-with(@Source,'LMCDiagnosticsService1.SdoAxis')]")).Count -ne 4) {
        throw 'Comm_Network must contain exactly four LMCDiagnosticsService1.SdoAxis cross-network connections.'
    }

    foreach ($classDbEntry in @(
        'LMCSdoExecutor',
        'TryStartRead',
        'CopyCompletion',
        'MarkOrphan',
        'IsReusable',
        'LMCSdoExecutorResult',
        'SdoAxis1',
        'SdoAxis2',
        'SdoAxis3',
        'SdoAxis4',
        'ProcessOperations')) {
        Assert-Match $classDbText ([regex]::Escape($classDbEntry)) (
            "Classes.lcb metadata is missing $classDbEntry. Reload and save the SDO classes through LASAL IDE.")
    }
    Assert-Match $classDbText 'TryStartRead(?!4)' 'Classes.lcb still lacks the exact TryStartRead method name. Update the LMCSdoExecutor declaration and save it through LASAL IDE.'
    if ($classDbText -match 'TryStartRead4') {
        throw 'Classes.lcb still contains the stale TryStartRead4 declaration. Replace it with TryStartRead and save through LASAL IDE.'
    }
    Assert-Match $commNetworkTable '"MaxConnections",\s*TO_UDINT\(1\),//\|Comm_Network\._TCPIPServer1\.MaxConnections;' '_TCPIPServer1 generated MaxConnections value is stale in Comm_Network.'
    foreach ($axisNumber in 1..9) {
        $axisObject = $motionNetworkXml.SelectSingleNode(
            "/Network/Components/Object[@Name='_LMCAxis$axisNumber']")
        if ($null -eq $axisObject) {
            throw "_LMCAxis$axisNumber network object is missing."
        }

        $moveTypeServer = $axisObject.SelectSingleNode(
            "./Channels/Server[@Name='MoveType']")
        if ($null -eq $moveTypeServer -or
            $moveTypeServer.Value -ne '_JERK_PROFILE') {
            throw "_LMCAxis$axisNumber.MoveType must be _JERK_PROFILE for nonzero Jerk commands."
        }

        $jMaxServer = $axisObject.SelectSingleNode(
            "./Channels/Server[@Name='JMax']")
        if ($null -eq $jMaxServer -or
            [string]::IsNullOrWhiteSpace($jMaxServer.Value) -or
            $jMaxServer.Value -match '^\s*0(?:\s|$)') {
            throw "_LMCAxis$axisNumber.JMax must be configured to a nonzero value."
        }

        $generatedMoveTypePattern =
            '"MoveType",\s*TO_UDINT\(_JERK_PROFILE\),//\|Motion_Network\._LMCAxis' +
            $axisNumber +
            '\.MoveType;'
        Assert-Match $motionNetworkTable $generatedMoveTypePattern "_LMCAxis$axisNumber generated MoveType value is stale."

        $posControllerName = "PosController$axisNumber"
        $posControllerObjects = @(
            $motionNetworkXml.SelectNodes(
                "/Network/Components/Object[@Name='$posControllerName' and " +
                "@Class='PosController']"))
        if ($posControllerObjects.Count -ne 1) {
            throw "$posControllerName must exist exactly once in Motion_Network."
        }
        $posControllerConnections = @(
            $motionNetworkXml.SelectNodes(
                "/Network/Connections/Connection[" +
                "@Source='_LMCAxis$axisNumber.LMCController' and " +
                "@Destination='$posControllerName.Signal_Input']"))
        if ($posControllerConnections.Count -ne 1) {
            throw ("_LMCAxis$axisNumber.LMCController must have exactly one " +
                "connection to $posControllerName.Signal_Input.")
        }
        Assert-Match $motionNetworkTable (
            '"POSCONTROLLER' + $axisNumber + '"') (
            "$posControllerName generated object metadata is missing.")
    }

    $robotObject = $motionNetworkXml.SelectSingleNode(
        "/Network/Components/Object[@Name='_LMCRobotBase1']")
    if ($null -eq $robotObject) {
        throw '_LMCRobotBase1 network object is missing.'
    }

    $robotMoveTypeServer = $robotObject.SelectSingleNode(
        "./Channels/Server[@Name='MoveType']")
    if ($null -eq $robotMoveTypeServer -or
        $robotMoveTypeServer.Value -ne '_JERK_PROFILE') {
        throw '_LMCRobotBase1.MoveType must be _JERK_PROFILE for nonzero group Jerk commands.'
    }

    $robotJMaxServer = $robotObject.SelectSingleNode(
        "./Channels/Server[@Name='JMax']")
    if ($null -eq $robotJMaxServer -or
        [string]::IsNullOrWhiteSpace($robotJMaxServer.Value) -or
        $robotJMaxServer.Value -match '^\s*0(?:\s|$)') {
        throw '_LMCRobotBase1.JMax must be configured to a nonzero value.'
    }

    Assert-Match $motionNetworkTable '"MoveType",\s*TO_UDINT\(_JERK_PROFILE\),//\|Motion_Network\._LMCRobotBase1\.MoveType;' '_LMCRobotBase1 generated MoveType value is stale.'
    Assert-Match $motionNetworkTable '"JMax",\s*TO_UDINT\((?!0(?:\s|\)))[^)]+\),//\|Motion_Network\._LMCRobotBase1\.JMax;' '_LMCRobotBase1 generated JMax value is zero or stale.'
    $generatedTaskRefs = [regex]::Matches($commNetworkTable, '//TCPMOTIONINTERFACE1').Count
    if ($generatedTaskRefs -ne 2) {
        throw "TCPMotionInterface1 generated task references=$generatedTaskRefs, expected two cyclic-only entries. Regenerate the LASAL network table."
    }
}

if ($st -match '(?<![A-Za-z0-9_])LMCAxis(?![A-Za-z0-9_])') {
    throw 'Legacy standalone LMCAxis name is still present in TCPMotionInterface.'
}

Assert-Match $st 'RequestQueue\s*:\s*ARRAY \[0\.\.7\] OF _TCPMI_REQUEST_ENTRY' 'Depth-8 LASAL request queue is missing.'
Assert-Match $st 'TO_UDINT\(1663666918\),\s*"LMCAxis1".*TO_UDINT\(1422175863\),\s*"_LMCAxis"' 'LMCAxis1 client-name/type hashes are incorrect.'
Assert-Match $st 'RealtimeTask\s*=\s*"false"' 'TCPMotionInterface still enables a RealTime task.'
Assert-Match $st 'CyclicTask\s*=\s*"true"' 'TCPMotionInterface Cyclic task is disabled.'
Assert-Match $st 'DefCyclictime\s*=\s*"1 ms"' 'TCPMotionInterface default cyclic time is not 1 ms.'
Assert-Match $st 'PayloadData\s*:\s*ARRAY \[0\.\.1319\] OF BYTE' 'LASAL queue does not hold the 1320-byte kinematic payload.'
Assert-Match $st 'ReceiveBuf\s*:\s*ARRAY \[0\.\.2047\] OF BYTE' 'LASAL receive accumulator does not hold a 1328-byte kinematic frame.'
Assert-Match $st 'RequestBuf\s*:\s*ARRAY \[0\.\.1327\] OF BYTE' 'LASAL active request buffer does not hold a 1328-byte kinematic frame.'
Assert-Match $st 'if usPayloadLength > 1320 then' 'LASAL queue payload bound is not 1320 bytes.'
Assert-Match $st 'IngressDiscardRemaining\s*:=\s*udFrameSize - ReceiveFill' 'Oversize frame bounded discard is missing.'
Assert-Match $st 'GroupMoveRetCode\s*:=\s*_LMCPROF_MOVECMD_ERROR' 'Group move false-success guard is missing.'
$classDeclarationBlock = [regex]::Match(
    $st,
    '(?s)TCPMotionInterface\s*:\s*CLASS.*?END_CLASS;').Value
foreach ($persistentName in @(
    'GroupCommandConfig',
    'GroupCommandInputValid',
    'GroupStopCommandNo',
    'GroupReadPos',
    'GroupReadRetCode',
    'GroupKinematicReady',
    'GroupReadInPosition',
    'GroupReadState',
    'GroupReadErrorId')) {
    Assert-Match $classDeclarationBlock ([regex]::Escape($persistentName)) "LASAL class declaration is missing $persistentName."
    Assert-Match $classDbText ([regex]::Escape($persistentName)) "Classes.lcb metadata is missing $persistentName. Save the variable through LASAL IDE."
}
foreach ($localOnlyName in @(
    'GroupKinematicConfigured',
    'GroupPowerIsOn',
    'GroupProfileLocked',
    'GroupProfileLockState')) {
    if ($classDeclarationBlock -match [regex]::Escape($localOnlyName)) {
        throw "$localOnlyName was added to the generated class declaration without matching LASAL class metadata."
    }
}
$localFamilyHandlerNames = @(
    'HandleAdminCommands',
    'HandleDiagnosticsCommands',
    'HandleRegistryCommands',
    'HandleAxisCommands',
    'HandleGroupCommands')
foreach ($handlerName in $localFamilyHandlerNames) {
    Assert-Match $classDeclarationBlock (
        'FUNCTION\s+' + [regex]::Escape($handlerName) + '\s*;') (
        "TCPMotionInterface.$handlerName declaration is missing.")
    Assert-Match $classDbText ([regex]::Escape($handlerName)) (
        "Classes.lcb metadata is missing $handlerName. Save the method through LASAL IDE.")
}
if ($st -match '(?:_TCPMI_RT_|RtRequest|RtResult|ActiveAwaitingRt|TCPMotionInterface::RtWork|CmdTable\.RtWork)') {
    throw 'TCPMotionInterface still contains an RT mailbox or RtWork dependency.'
}
if ($st -match 'sigclib_atomic_') {
    throw 'TCPMotionInterface still contains cross-task atomic operations.'
}
if ($st -match 'bDirect\s*:=\s*FALSE') {
    throw 'TCPMotionInterface mixes buffered and direct TX ordering.'
}

$msgParserBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION TCPMotionInterface::MsgPaser.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($msgParserBlock)) {
    throw 'TCPMotionInterface.MsgPaser implementation was not found.'
}
$localFamilyHandlerBlocks = [ordered]@{}
foreach ($handlerName in $localFamilyHandlerNames) {
    $handlerBlock = [regex]::Match(
        $st,
        ('(?s)FUNCTION TCPMotionInterface::' +
         [regex]::Escape($handlerName) +
         '.*?END_FUNCTION')).Value
    if ([string]::IsNullOrWhiteSpace($handlerBlock)) {
        throw "TCPMotionInterface.$handlerName implementation was not found."
    }
    $handlerByteCount = [Text.Encoding]::UTF8.GetByteCount($handlerBlock)
    if ($handlerByteCount -gt 32768) {
        throw "TCPMotionInterface.$handlerName is $handlerByteCount bytes, expected at most 32768."
    }
    $localFamilyHandlerBlocks[$handlerName] = $handlerBlock
}
$adminHandlerBlock = $localFamilyHandlerBlocks['HandleAdminCommands']
$diagnosticsHandlerBlock = $localFamilyHandlerBlocks['HandleDiagnosticsCommands']
$registryHandlerBlock = $localFamilyHandlerBlocks['HandleRegistryCommands']
$axisHandlerBlock = $localFamilyHandlerBlocks['HandleAxisCommands']
$groupHandlerBlock = $localFamilyHandlerBlocks['HandleGroupCommands']
if ($ControlServiceCheckpoint -eq 'Phase3GroupRouted') {
    foreach ($adminLocal in @(
            @{ Name = 'adminSchemaVersion'; Type = 'UINT' },
            @{ Name = 'adminRequestFlags'; Type = 'UINT' },
            @{ Name = 'adminRequestId'; Type = 'UDINT' },
            @{ Name = 'adminParameterKey'; Type = 'UINT' },
            @{ Name = 'adminDetailCode'; Type = 'UDINT' },
            @{ Name = 'adminAxisValue'; Type = 'DINT' },
            @{ Name = 'adminUnitCode'; Type = 'UINT' },
            @{ Name = 'adminAxisReadKind'; Type = 'UINT' },
            @{ Name = 'adminAxisParameter'; Type = '_LMCAXIS_READPARAMETER' },
            @{ Name = 'adminSwEndMode'; Type = '_LMCAXIS_READSWENDPOS' },
            @{ Name = 'adminAxisClientConnected'; Type = 'BOOL' },
            @{ Name = 'adminErrorId'; Type = 'INT' })) {
        Assert-Match $adminHandlerBlock (
            '(?m)^\s*' + [regex]::Escape($adminLocal.Name) +
            '\s*:\s*' + [regex]::Escape($adminLocal.Type) + '\s*;\s*$') (
            "HandleAdminCommands remaining local $($adminLocal.Name) is missing.")
    }
}
else {
    Assert-Match $adminHandlerBlock '(?s)VAR\s+kinIndex\s*:\s*DINT;.*?adminErrorId\s*:\s*INT;\s*END_VAR' 'HandleAdminCommands local declaration contract is incomplete.'
}
Assert-Match $diagnosticsHandlerBlock '(?s)VAR\s+diagnosticsSchemaVersion\s*:\s*UINT;.*?diagnosticsBootId\s*:\s*UDINT;\s*END_VAR' 'HandleDiagnosticsCommands local declaration contract is incomplete.'
Assert-Match $registryHandlerBlock '(?s)VAR\s+objectNameLength\s*:\s*UDINT;\s*END_VAR' 'HandleRegistryCommands local declaration contract is incomplete.'
if ($ControlServiceCheckpoint -ne 'Phase3GroupRouted') {
    Assert-Match $groupHandlerBlock '(?s)VAR\s+objectNameLength\s*:\s*UDINT;\s*kinIndex\s*:\s*DINT;\s*kinValid\s*:\s*BOOL;\s*powerIsOn\s*:\s*DINT;\s*profileLockState\s*:\s*DINT;\s*END_VAR' 'HandleGroupCommands local declaration contract is incomplete or reordered.'
}
$msgParserByteCount = [Text.Encoding]::UTF8.GetByteCount($msgParserBlock)
if ($msgParserByteCount -gt 32768) {
    throw "TCPMotionInterface.MsgPaser is $msgParserByteCount bytes, expected at most 32768."
}
$caseIndex = $msgParserBlock.IndexOf('case CommandID of')
if ($caseIndex -lt 0) {
    throw 'TCPMotionInterface.MsgPaser command case was not found.'
}
$preCaseBlock = $msgParserBlock.Substring(0, $caseIndex)
foreach ($commandId in @('2023', '2024', '2022', '2028', '202E', '209F', '20A0', '20A2', '20D2', '2047', '2048', '2049', '204A', '204B', '2045', '2051', '2085', '20A4', '20E7', '7D00', '7D10', '7D20', '7D22')) {
    if ($preCaseBlock -match "CommandID = 0x$commandId") {
        throw "Active command 0x$commandId is blocked before its CyWork handler."
    }
}
Assert-Match $msgParserBlock '(?s)0x7E00,\s*0x7E01,\s*0x7E02,\s*0x7E03,\s*0x7E04,\s*0x7E10,\s*0x7E20,\s*0x7E21,\s*0x7E30,\s*0x7E31,\s*0x7E32,\s*0x7E33,\s*0x7E40,\s*0x7E41,\s*0x7E42,\s*0x7E43,\s*0x7E44,\s*0x7E45,\s*0x7E46,\s*0x7E47,\s*0x7E48,\s*0x7E49,\s*0x7E50,\s*0x7E51:\s*HandleDiagnosticsCommands\(\);' 'MsgPaser diagnostics-family aggregate route is missing or reordered.'
Assert-Match $msgParserBlock '(?s)0x103C,\s*0x1042,\s*0x202B:\s*HandleRegistryCommands\(\);' 'MsgPaser registry-family aggregate route is missing or reordered.'
Assert-Match $msgParserBlock '(?s)0x2023,\s*0x2024,\s*0x2022,\s*0x2028,\s*0x202E,\s*0x209F,\s*0x20A0,\s*0x20A2:\s*HandleAxisCommands\(\);' 'MsgPaser axis-family aggregate route is missing or reordered.'
$localHandlerExpectedCallCounts = [ordered]@{
    HandleAdminCommands = 1
    HandleDiagnosticsCommands = 1
    HandleRegistryCommands = 1
    HandleAxisCommands = 1
    HandleGroupCommands = 1
}
if ($ControlServiceCheckpoint -eq 'Phase3GroupRouted') {
    $localHandlerExpectedCallCounts['HandleGroupCommands'] = 0
    Assert-Match $msgParserBlock (
        '(?s)0x7D00,\s*0x7D10:\s*HandleAdminCommands\(\);') (
        'Phase3GroupRouted MsgPaser remaining Admin route is missing or reordered.')
    Assert-ExactLasalCommandRouteIds `
        -RouterBlock $msgParserBlock `
        -Owner 'Phase3GroupRouted TCPMotionInterface control-service route' `
        -CallPattern 'ControlCommands\s*\.\s*HandleRequest\s*\(' `
        -ExpectedCommandIds ($phase3GroupCommandIds + $phase3AdminCommandIds)

    $controlServiceRoutePattern = (
        '(?ms)^[ \t]*(?<Labels>0x[0-9A-Fa-f]{4}' +
        '(?:[ \t]*,[ \t]*(?:\r?\n[ \t]*)?' +
        '0x[0-9A-Fa-f]{4})*)[ \t]*:' +
        '(?<Body>.*?)(?=^[ \t]*(?:0x[0-9A-Fa-f]{4}|else\b|end_case\b))')
    $controlServiceRouteMatches = @(
        [regex]::Matches($msgParserBlock, $controlServiceRoutePattern) |
            Where-Object {
                $_.Groups['Body'].Value -match
                    'ControlCommands\s*\.\s*HandleRequest\s*\('
            })
    if ($controlServiceRouteMatches.Count -ne 1) {
        throw ('Phase3GroupRouted control-service route could not be ' +
            'isolated for transport-contract validation.')
    }
    $controlServiceRouteBlock = $controlServiceRouteMatches[0].Groups['Body'].Value
    $controlServiceCallMatch = [regex]::Match(
        $controlServiceRouteBlock,
        ('(?s)(?<Result>[A-Za-z_][A-Za-z0-9_]*)\s*:=\s*' +
         'ControlCommands\s*\.\s*HandleRequest\s*\(\s*' +
         'CommandId\s*:=\s*CommandID\$UINT\s*,\s*' +
         'Reference\s*:=\s*AxisRef\$UINT\s*,\s*' +
         'pRequestFrame\s*:=\s*\(?\s*#RequestBuf\[0\]\s*\)?' +
         '(?:\$\^USINT)?\s*,\s*' +
         'RequestFrameSize\s*:=\s*\(?\s*Payload\s*\+\s*8\s*\)?' +
         '(?:\$UDINT)?\s*,\s*' +
         'pResponseFrame\s*:=\s*\(?\s*#Sendbuf\[0\]\s*\)?' +
         '(?:\$\^USINT)?\s*,\s*' +
         'ResponseCapacity\s*:=\s*sizeof\(Sendbuf\)\s*\)'))
    if (-not $controlServiceCallMatch.Success) {
        throw ('Phase3GroupRouted must pass CommandID, AxisRef, the complete ' +
            'request frame and size, and the complete response buffer and ' +
            'capacity to ControlCommands.HandleRequest in ABI order.')
    }
    $controlResponseName = $controlServiceCallMatch.Groups['Result'].Value
    $escapedControlResponseName = [regex]::Escape($controlResponseName)
    $controlResponseDeclarations = [regex]::Matches(
        $msgParserBlock,
        ('(?m)^\s*' + $escapedControlResponseName +
         '\s*:\s*DINT\s*;\s*$'))
    $msgParserVarBlock = [regex]::Match(
        $msgParserBlock,
        '(?s)\AFUNCTION\s+TCPMotionInterface::MsgPaser\s*' +
        'VAR\s*(?<Body>.*?)\s*END_VAR').Groups['Body'].Value
    if ($controlResponseDeclarations.Count -ne 1 -or
        [string]::IsNullOrWhiteSpace($msgParserVarBlock) -or
        $msgParserVarBlock -notmatch (
            '(?m)^\s*' + $escapedControlResponseName +
            '\s*:\s*DINT\s*;\s*$')) {
        throw ("Phase3GroupRouted response scratch $controlResponseName " +
            'must be declared exactly once as a MsgPaser-local DINT.')
    }
    $controlResponseInitMatch = [regex]::Match(
        $controlServiceRouteBlock,
        $escapedControlResponseName + '\s*:=\s*-1\s*;')
    $controlClientCallBlockMatch = [regex]::Match(
        $controlServiceRouteBlock,
        ('(?s)if\s+IsClientConnected\(#ControlCommands\)\s+then.*?' +
         [regex]::Escape($controlServiceCallMatch.Value) +
         '\s*;\s*end_if;'))
    $controlFallbackBlockMatch = [regex]::Match(
        $controlServiceRouteBlock,
        ('(?s)if\s+\(' + $escapedControlResponseName +
         '\s*<=\s*0\)\s*\|\s*\(' + $escapedControlResponseName +
         '\s*>\s*sizeof\(Sendbuf\)\)\s+then.*?' +
         $escapedControlResponseName + '\s*:=\s*12;\s*end_if;'))
    $controlSharedSendMatch = [regex]::Match(
        $controlServiceRouteBlock,
        ('(?s)SendData\(\s*pData:=#Sendbuf\[0\],\s*' +
         'udSize:=' + $escapedControlResponseName + '\$UDINT,\s*' +
         'dSocket:=CurrentSock,\s*bDirect:=TRUE\s*\);'))
    if (-not $controlResponseInitMatch.Success -or
        -not $controlClientCallBlockMatch.Success -or
        -not $controlFallbackBlockMatch.Success -or
        -not $controlSharedSendMatch.Success -or
        $controlResponseInitMatch.Index -ge $controlClientCallBlockMatch.Index -or
        $controlServiceCallMatch.Index -lt $controlClientCallBlockMatch.Index -or
        ($controlServiceCallMatch.Index + $controlServiceCallMatch.Length) -gt
            ($controlClientCallBlockMatch.Index + $controlClientCallBlockMatch.Length) -or
        ($controlClientCallBlockMatch.Index + $controlClientCallBlockMatch.Length) -gt
            $controlFallbackBlockMatch.Index -or
        ($controlFallbackBlockMatch.Index + $controlFallbackBlockMatch.Length) -gt
            $controlSharedSendMatch.Index) {
        throw ('Phase3GroupRouted order must be result init, connected ' +
            'HandleRequest call, invalid-response normalization, then one ' +
            'shared SendData.')
    }
    Assert-Match $controlServiceRouteBlock (
        '(?s)if\s+\(' + $escapedControlResponseName + '\s*<=\s*0\)\s*\|\s*' +
        '\(' + $escapedControlResponseName +
        '\s*>\s*sizeof\(Sendbuf\)\)\s+then\s*' +
        '_memset\(dest:=#Sendbuf,\s*usByte:=0,\s*cntr:=sizeof\(Sendbuf\)\);.*?' +
        'Sendbuf\[0\]\$UINT\s*:=\s*1;.*?' +
        'Sendbuf\[2\]\$UINT\s*:=\s*4;.*?' +
        'Sendbuf\[4\]\$UDINT\s*:=\s*0;.*?' +
        'Sendbuf\[8\]\$UINT\s*:=\s*1;.*?' +
        'Sendbuf\[10\]\$INT\s*:=\s*-1;.*?' +
        $escapedControlResponseName + '\s*:=\s*12;.*?end_if;.*?' +
        'SendData\(\s*pData:=#Sendbuf\[0\],\s*' +
        'udSize:=' + $escapedControlResponseName + '\$UDINT,\s*' +
        'dSocket:=CurrentSock,\s*bDirect:=TRUE\s*\);') (
        'Phase3GroupRouted invalid-response bound, common fail-closed frame, or single-send path is incomplete.')
    $controlRouteSendCount = [regex]::Matches(
        $controlServiceRouteBlock,
        '(?m)^\s*SendData\s*\(').Count
    if ($controlRouteSendCount -ne 1) {
        throw ('Phase3GroupRouted control-service route SendData call count is ' +
            "$controlRouteSendCount, expected exactly one shared send.")
    }
}
else {
    Assert-Match $msgParserBlock '(?s)0x20D2,\s*0x2047,\s*0x2048,\s*0x2049,\s*0x204A,\s*0x204B,\s*0x2085,\s*0x20A4,\s*0x2045,\s*0x2051,\s*0x20E7:\s*HandleGroupCommands\(\);' 'MsgPaser group-family aggregate route is missing or reordered.'
    Assert-Match $msgParserBlock '(?s)0x7D00,\s*0x7D10,\s*0x7D20,\s*0x7D22:\s*HandleAdminCommands\(\);' 'MsgPaser admin-family aggregate route is missing or reordered.'
    Assert-ExactLasalCommandCaseIds `
        -FunctionBlock $groupHandlerBlock `
        -Owner 'TCPMotionInterface.HandleGroupCommands' `
        -ExpectedCommandIds $phase3GroupCommandIds
    Assert-ExactLasalCommandCaseIds `
        -FunctionBlock $adminHandlerBlock `
        -Owner 'TCPMotionInterface.HandleAdminCommands' `
        -ExpectedCommandIds @('7D00', '7D10', '7D20', '7D22')
}
foreach ($handlerName in $localHandlerExpectedCallCounts.Keys) {
    $handlerCallCount = [regex]::Matches(
        $st,
        ('(?m)^\s*' + [regex]::Escape($handlerName) + '\(\);\s*$')).Count
    $expectedCallCount = $localHandlerExpectedCallCounts[$handlerName]
    if ($handlerCallCount -ne $expectedCallCount) {
        throw (
            "$ControlServiceCheckpoint $handlerName call count is " +
            "$handlerCallCount, expected $expectedCallCount MsgPaser caller(s).")
    }
}

$adminCapabilitiesCaseBlock = [regex]::Match(
    $adminHandlerBlock,
    '(?s)0x7D00:.*?0x7D10:').Value
$adminAxisParameterCasePattern = if (
    $ControlServiceCheckpoint -eq 'Phase3GroupRouted') {
    '(?s)0x7D10:.*'
}
else {
    '(?s)0x7D10:.*?0x7D20:'
}
$adminAxisParameterCaseBlock = [regex]::Match(
    $adminHandlerBlock,
    $adminAxisParameterCasePattern).Value
if ([string]::IsNullOrWhiteSpace($adminCapabilitiesCaseBlock) -or
    [string]::IsNullOrWhiteSpace($adminAxisParameterCaseBlock)) {
    throw 'The local 0x7D00/0x7D10 admin cases were not found.'
}

Assert-Match $adminCapabilitiesCaseBlock '(?s)if Payload >= 8 then.*?RequestBuf\[8\]\$UINT.*?RequestBuf\[10\]\$UINT.*?RequestBuf\[12\]\$UDINT' '0x7D00 common request offsets are incomplete.'
Assert-Match $adminCapabilitiesCaseBlock '(?s)Payload <> 8.*?AxisRef <> 0.*?adminSchemaVersion <> 1.*?adminRequestFlags <> 0.*?adminRequestId = 0' '0x7D00 request validation is incomplete.'
Assert-Match $adminCapabilitiesCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*40.*?Sendbuf\[24\]\$UDINT\s*:=\s*0x00000007.*?Sendbuf\[28\]\$UDINT\s*:=\s*0x0000003F.*?Sendbuf\[32\]\$UDINT\s*:=\s*0x00000007.*?Sendbuf\[36\]\$UINT\s*:=\s*4.*?Sendbuf\[40\]\$UINT\s*:=\s*0x0100.*?Sendbuf\[42\]\$UINT\s*:=\s*3.*?udSize:=48' '0x7D00 capability bits, masks, limits, or response framing are incomplete.'

Assert-Match $adminAxisParameterCaseBlock '(?s)Payload <> 12.*?\(AxisRef < 1\) \| \(AxisRef > 4\).*?adminSchemaVersion <> 1.*?RequestBuf\[18\]\$UINT <> 0' '0x7D10 payload/reference/common/reserved validation is incomplete.'
Assert-Match $adminAxisParameterCaseBlock '(?s)case adminParameterKey of.*?LMCAXIS_RD_SWMIN_APPUNIT.*?LMCAXIS_RD_SWMAX_APPUNIT.*?LMCAXIS_PAR_RD_SWLIMWINDOW.*?LMCAXIS_PAR_RD_V_MAX.*?LMCAXIS_PAR_RD_A_MAX.*?LMCAXIS_PAR_RD_REFPOS.*?adminDetailCode := 6' '0x7D10 semantic-to-native allowlist mapping is incomplete.'
if ([regex]::Matches($adminAxisParameterCaseBlock, '\bLMCAxis[1-4]\.ReadSWEndPos\s*\(').Count -ne 4 -or
    [regex]::Matches($adminAxisParameterCaseBlock, '\bLMCAxis[1-4]\.ReadParameter\s*\(').Count -ne 4) {
    throw '0x7D10 must expose both safe native read paths for each physical axis exactly once.'
}
Assert-Match $adminAxisParameterCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*28.*?Sendbuf\[24\]\$UINT\s*:=\s*adminParameterKey.*?Sendbuf\[26\]\$UINT\s*:=\s*1.*?Sendbuf\[28\]\$UINT\s*:=\s*adminUnitCode.*?Sendbuf\[32\]\$DINT\s*:=\s*adminAxisValue.*?udSize:=36.*?Sendbuf\[2\]\$UINT\s*:=\s*16.*?Sendbuf\[14\]\$INT\s*:=\s*-31000.*?udSize:=24' '0x7D10 success/error response framing is incomplete.'

if ($ControlServiceCheckpoint -ne 'Phase3GroupRouted') {
    $adminGroupParametersCaseBlock = [regex]::Match(
        $adminHandlerBlock,
        '(?s)0x7D20:.*?0x7D22:').Value
    $adminGroupMoveRelativeCaseBlock = [regex]::Match(
        $adminHandlerBlock,
        '(?s)0x7D22:.*').Value
    if ([string]::IsNullOrWhiteSpace($adminGroupParametersCaseBlock) -or
        [string]::IsNullOrWhiteSpace($adminGroupMoveRelativeCaseBlock)) {
        throw 'The legacy local 0x7D20/0x7D22 admin cases were not found.'
    }

    Assert-Match $adminGroupParametersCaseBlock '(?s)Payload <> 12.*?AxisRef <> 0x0100.*?adminSelectionMask = 0.*?adminSelectionMask and 0xFFFFFFF8.*?IsClientConnected\(#LMCRobot\)' '0x7D20 group reference, mask, or client validation is incomplete.'
    foreach ($groupParameter in @('_LMCPROF_GRP_VEL_LIMIT', '_LMCPROF_GRP_ACCEL_LIMIT', '_LMCPROF_GRP_TJERK')) {
        Assert-Match $adminGroupParametersCaseBlock (
            'LMCRobot\.ReadGroupParameter\(\s*GrpNo:=1,\s*ParNo:=' +
            [regex]::Escape($groupParameter) + '\)') "0x7D20 is missing $groupParameter semantic mapping."
    }
    if ([regex]::Matches($adminGroupParametersCaseBlock, '\bLMCRobot\.ReadGroupParameter\s*\(').Count -ne 3) {
        throw '0x7D20 must issue at most the three selected native parameter reads.'
    }
    Assert-Match $adminGroupParametersCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*32.*?Sendbuf\[24\]\$UDINT\s*:=\s*adminSelectionMask.*?Sendbuf\[28\]\$DINT\s*:=\s*adminGroupVelocityLimit.*?Sendbuf\[32\]\$DINT\s*:=\s*adminGroupAccelerationLimit.*?Sendbuf\[36\]\$DINT\s*:=\s*adminGroupJerkTime.*?udSize:=40' '0x7D20 fixed success response framing is incomplete.'

    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)Payload <> 104.*?AxisRef <> 0x0100.*?adminSchemaVersion <> 1.*?adminRequestFlags <> 0.*?adminRequestId = 0' '0x7D22 payload/reference/common request validation is incomplete.'
    Assert-Match $adminGroupMoveRelativeCaseBlock 'adminErrorId := -31000' '0x7D22 local validation and state errors do not use the Admin error ID.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)source:=#RequestBuf\[16\].*?source:=#RequestBuf\[80\].*?source:=#RequestBuf\[84\].*?source:=#RequestBuf\[88\].*?source:=#RequestBuf\[92\].*?source:=#RequestBuf\[96\].*?source:=#RequestBuf\[100\].*?source:=#RequestBuf\[104\].*?source:=#RequestBuf\[108\]' '0x7D22 DINT field offsets are incomplete.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)\(GroupVelocity > 0\).*?\(GroupAccel > 0\).*?\(GroupDecel > 0\).*?\(GroupJerk >= 0\).*?\(GroupCoordSystem = 0\).*?\(GroupTransitionModeInput = 0\).*?\(GroupTransitionModeInput = 2\).*?\(bufMode = 1\).*?\(bufMode = 2\).*?\(GroupExecute = 1\).*?adminDetailCode := 9' '0x7D22 approved motion-parameter validation is incomplete.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)for kinIndex := 4 to 15 do.*?RequestBuf\[\(16 \+ \(kinIndex \* 4\)\)\$DINT\]\$DINT <> 0.*?GroupCommandInputValid := FALSE' '0x7D22 does not reject nonzero distances outside the four-axis topology.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)case GroupTransitionModeInput of.*?_LMCPROF_EXACT_STOP.*?_LMCPROF_CONT_DIRECT.*?if bufMode = 1 then.*?GroupCommandConfig := 16' '0x7D22 transition and buffer-mode mapping is incomplete.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)IsClientConnected\(#LMCRobot\).*?IsClientConnected\(#LMCAxis1\).*?IsClientConnected\(#LMCAxis2\).*?IsClientConnected\(#LMCAxis3\).*?IsClientConnected\(#LMCAxis4\).*?LMCRobot\.RobotIsOn\(\).*?LMCRobot\.ReadProfileParameter\(\s*ParNo:=_LMCPROF_LockState\).*?GroupKinematicReady = TRUE.*?powerIsOn <> 0.*?profileLockState <> 0.*?LMCRobot\.MoveRelativeCoord\(.*?pDistances:=#GroupMovePos.*?CmdConfig:=GroupCommandConfig.*?Velocity:=GroupVelocity.*?Accel:=GroupAccel.*?Decel:=GroupDecel.*?TransMode:=GroupTransitionMode.*?TransRadius:=GroupTransitionRadius.*?CoordSystem:=0.*?Jerk:=GroupJerk' '0x7D22 does not gate and dispatch the relative move through the configured, powered, locked four-axis profile.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)GroupMoveRetCode = _LMCPROF_NoError.*?adminErrorId := 0.*?adminDetailCode := 11.*?GroupMoveRetCode\$UDINT <= 32767.*?adminErrorId := GroupMoveRetCode\$INT.*?adminErrorId := -6' '0x7D22 does not preserve a representable native rejection code.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)adminDetailCode := 10.*?_memset\(dest:=#Sendbuf.*?Sendbuf\[2\]\$UINT\s*:=\s*16.*?Sendbuf\[8\]\$UINT\s*:=\s*1.*?Sendbuf\[10\]\$UINT\s*:=\s*0.*?Sendbuf\[12\]\$UINT\s*:=\s*0.*?Sendbuf\[14\]\$INT\s*:=\s*0.*?Sendbuf\[16\]\$UDINT\s*:=\s*adminRequestId.*?Sendbuf\[20\]\$UDINT\s*:=\s*adminDetailCode.*?adminDetailCode <> 0.*?Sendbuf\[12\]\$UINT\s*:=\s*1.*?Sendbuf\[14\]\$INT\s*:=\s*adminErrorId.*?udSize:=24' '0x7D22 state error or Admin response framing is incomplete.'
}

$diagnosticsCapabilitiesCaseBlock = [regex]::Match(
    $diagnosticsHandlerBlock,
    '(?s)0x7E00:.*?0x7E01,').Value
if ([string]::IsNullOrWhiteSpace($diagnosticsCapabilitiesCaseBlock)) {
    throw '0x7E00 diagnostics capability case was not found.'
}
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)if Payload >= 8 then.*?RequestBuf\[8\]\$UINT.*?RequestBuf\[10\]\$UINT.*?RequestBuf\[12\]\$UDINT' '0x7E00 common request fields are not decoded for exact and overlength envelopes at the specified offsets.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)\(Payload <> 8\) \| \(AxisRef <> 0\).*?diagnosticsSchemaVersion <> 1.*?diagnosticsRequestFlags <> 0' '0x7E00 payload/reference/schema/flags validation is missing.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)elsif diagnosticsRequestId = 0 then.*?Sendbuf\[20\]\$UDINT\s*:=\s*12' '0x7E00 does not reject the reserved RequestId zero value with BoundsInvalid.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[2\]\$UINT\s*:=\s*68' '0x7E00 response payload length is not 68 bytes.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[24\]\$UDINT\s*:=\s*1' '0x7E00 DiagnosticsBuild is not 1.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[28\]\$UDINT\s*:=\s*0' '0x7E00 disconnected CapabilityBits default is not fail-closed.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[32\]\$UDINT\s*:=\s*0' '0x7E00 disconnected MapRevision default is not fail-closed.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[36\]\$UINT\s*:=\s*0' '0x7E00 disconnected CatalogEntryCount default is not fail-closed.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)diagnosticsBootId := 0;.*?IsClientConnected\(#Diagnostics\).*?diagnosticsBootId := Diagnostics\.GetDiagnosticsBootId\(\)' '0x7E00 does not obtain the runtime retained DiagnosticsBootId.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)if IsClientConnected\(#Diagnostics\) then\s*Sendbuf\[28\]\$UDINT\s*:=\s*0x00000007;\s*Sendbuf\[32\]\$UDINT\s*:=\s*0x957F101E;\s*Sendbuf\[36\]\$UINT\s*:=\s*24' '0x7E00 does not advertise active D1 Health/Catalog/PI with the canonical map.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)if diagnosticsBootId <> 0 then\s*Sendbuf\[28\]\$UDINT\s*:=\s*0x0000213F;\s*Sendbuf\[38\]\$UINT\s*:=\s*24;\s*Sendbuf\[40\]\$UINT\s*:=\s*24;\s*Sendbuf\[42\]\$UINT\s*:=\s*1;\s*Sendbuf\[44\]\$UDINT\s*:=\s*320000;\s*Sendbuf\[64\]\$UDINT\s*:=\s*1280000;\s*Sendbuf\[68\]\$UINT\s*:=\s*4' '0x7E00 does not advertise the bounded D2-D4 envelope and general inline D5 SDO Read only for a stable BootId.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[52\]\$UINT\s*:=\s*1320' '0x7E00 MaxRequestPayloadBytes is not 1320.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[54\]\$UINT\s*:=\s*2040' '0x7E00 MaxResponsePayloadBytes is not 2040.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[56\]\$UINT\s*:=\s*1280' '0x7E00 MaxChunkDataBytes is not 1280.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[58\]\$UINT\s*:=\s*80' '0x7E00 CatalogEntryStride is not 80.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[60\]\$UINT\s*:=\s*16' '0x7E00 SignalValueEntryStride is not 16.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)Sendbuf\[68\]\$UINT\s*:=\s*0.*?if IsClientConnected\(#Diagnostics\).*?if diagnosticsBootId <> 0 then.*?Sendbuf\[68\]\$UINT\s*:=\s*4' '0x7E00 MaxSdoDataBytes must remain zero unless the diagnostics service is connected with a stable BootId, then advertise the general inline 4-byte limit.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[72\]\$UDINT\s*:=\s*diagnosticsBootId' '0x7E00 does not return the runtime DiagnosticsBootId.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)SendData\(.*?udSize:=76' '0x7E00 does not send the complete 76-byte frame.'

$diagnosticsDispatchBlock = [regex]::Match(
    $diagnosticsHandlerBlock,
    '(?s)0x7E01,\s*0x7E02.*?0x7E50,\s*0x7E51:.*?end_case;').Value
if ([string]::IsNullOrWhiteSpace($diagnosticsDispatchBlock)) {
    throw 'The reserved diagnostics command family is not delegated to LMCDiagnosticsService.'
}
Assert-Match $diagnosticsDispatchBlock '(?s)IsClientConnected\(#Diagnostics\).*?Diagnostics\.HandleRequest\(.*?ResponseCapacity:=2040.*?diagnosticsResponseSize <= 2040.*?SendData' 'Diagnostics service delegation or response bound is incomplete.'

Assert-Match $diagnosticsLatch 'RealtimeTask\s*=\s*"true"' 'LMCEcatInputLatch is not declared as an RT class.'
Assert-Match $diagnosticsLatch 'SnapshotBytes\s*:\s*ARRAY \[0\.\.511\] OF USINT' 'LMCEcatInputLatch fixed snapshot storage is not 512 bytes.'
Assert-Match $diagnosticsLatch '(?s)FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork.*?OS_READMICROSEC\(\).*?Drive1\.ActPos\.Read\(\).*?Drive4\.StateWord\.Read\(\).*?state := READY' 'LMCEcatInputLatch does not latch all four PDO images and timestamp in RtWork.'
Assert-Match $diagnosticsLatch 'sigclib_atomic_setU32\(pValue:=#PublishSequence' 'LMCEcatInputLatch publish sequence is not stored atomically.'
Assert-Match $diagnosticsLatch 'sigclib_atomic_getU32\(pValue:=#PublishSequence' 'LMCEcatInputLatch publish sequence is not loaded atomically.'
Assert-Match $diagnosticsLatch '(?s)FUNCTION GLOBAL LMCEcatInputLatch::CopySnapshot.*?DestSize < 304.*?retryCount < 3.*?_memcpy.*?sequenceBefore = sequenceAfter' 'LMCEcatInputLatch bounded seqlock copy is incomplete.'
Assert-Match $diagnosticsLatch '(?s)FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork.*?sigclib_atomic_setU32\(pValue:=#PublishSequence,\s*value:=finalSequence\).*?IsClientConnected\(#RecorderStore\).*?RecorderStore\.AppendSnapshot\(\s*pSnapshot:=#SnapshotBytes\[0\],\s*SnapshotSize:=304\).*?state := READY' 'LMCEcatInputLatch does not append the final immutable 304-byte RT snapshot to RecorderStore.'

Assert-Match $sdoExecutor '(?s)LMCSdoExecutor\s*:\s*CLASS\s*:\s*EtherCAT_SDOBase' 'LMCSdoExecutor no longer derives from EtherCAT_SDOBase.'
if ([regex]::Matches(
        $sdoExecutor,
        '<Connection\s+Source="_base\.toSlave"\s+Destination="this\.toSlave"').Count -ne 1) {
    throw 'LMCSdoExecutor internal network must forward exactly one _base.toSlave client to this.toSlave.'
}
$sdoResultTypeBlock = [regex]::Match(
    $sdoExecutor,
    '(?s)LMCSdoExecutorResult\s*:\s*STRUCT.*?END_STRUCT').Value
if ([string]::IsNullOrWhiteSpace($sdoResultTypeBlock)) {
    throw 'LMCSdoExecutorResult declaration was not found.'
}
Assert-Match $sdoResultTypeBlock 'Type Public="true" Name="LMCSdoExecutorResult"' 'LMCSdoExecutorResult is not a public LASAL type.'
Assert-Match $sdoResultTypeBlock '(?s)Token\s*:\s*UDINT;.*?OsResult\s*:\s*DINT;.*?AbortCode\s*:\s*UDINT;.*?ActualLength\s*:\s*UDINT;.*?ObjectIndex\s*:\s*UINT;.*?SubIndex\s*:\s*USINT;.*?IsWrite\s*:\s*USINT;.*?ValidationCode\s*:\s*UDINT;.*?Data\s*:\s*UDINT;.*?Reserved\s*:\s*UDINT;' 'LMCSdoExecutorResult 32-byte public field layout is incomplete or reordered.'
Assert-Match $sdoExecutor 'sizeof\(LMCSdoExecutorResult\)\s*<>\s*32' 'LMCSdoExecutor does not fail closed if its public result ABI is not 32 bytes.'
Assert-Match $sdoExecutor '(?s)#define LMC_SDO_EXEC_IDLE\s+0.*?#define LMC_SDO_EXEC_ARMING\s+1.*?#define LMC_SDO_EXEC_RUNNING\s+2.*?#define LMC_SDO_EXEC_RESULT_READY\s+3.*?#define LMC_SDO_EXEC_ORPHANED\s+4.*?#define LMC_SDO_EXEC_QUARANTINED\s+5.*?#define LMC_SDO_EXEC_RELEASING\s+6' 'LMCSdoExecutor atomic state constants are incomplete.'
Assert-Match $sdoExecutor 'Function Name="ClassState\.NewInst" UseBaseCmd="true"' 'LMCSdoExecutor callback override does not preserve the EtherCAT_SDOBase command table.'
Assert-Match $sdoExecutor '(?s)ParaReadWrite\.pMeth\s*:=\s*StoreMethod\(\s*#M_RD_DIRECT\(\),\s*#ParaReadWrite::Write\(\)\s*\).*?ParaType\.pMeth\s*:=\s*StoreMethod\(\s*#M_RD_DIRECT\(\),\s*#ParaType::Write\(\)\s*\).*?_memcpy\(\(#vmt\.CmdTable\)\$\^USINT,\s*ParaString\.pMeth.*?vmt\.CmdTable\.Write\s*:=\s*#ParaString::Write\(\).*?ParaString\.pMeth\s*:=\s*StoreCmd' 'LMCSdoExecutor manual-channel write overrides are not registered in the IDE-generated unqualified VMT entries.'

foreach ($manualWrite in @(
    @{ Name = 'ParaReadWrite'; Expected = 'ParaReadWrite' },
    @{ Name = 'ParaType'; Expected = 'ParaType' },
    @{ Name = 'ParaString'; Expected = 'ParaString' })) {
    $manualWriteBlock = [regex]::Match(
        $sdoExecutor,
        ('(?s)FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::' +
            $manualWrite.Name + '::Write.*?END_FUNCTION')).Value
    if ([string]::IsNullOrWhiteSpace($manualWriteBlock)) {
        throw "LMCSdoExecutor.$($manualWrite.Name).Write implementation was not found."
    }
    Assert-Match $manualWriteBlock (
        'result\s*:=\s*' + $manualWrite.Expected + '\s*;') (
        "LMCSdoExecutor.$($manualWrite.Name).Write does not ignore manual writes fail-closed.")
    if ($manualWriteBlock -match 'result\s*:=\s*input') {
        throw "LMCSdoExecutor.$($manualWrite.Name).Write accepts the manual input."
    }
}

$sdoTryStartBlock = [regex]::Match(
    $sdoExecutor,
    '(?s)FUNCTION GLOBAL LMCSdoExecutor::TryStartRead.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($sdoTryStartBlock)) {
    throw 'LMCSdoExecutor.TryStartRead implementation was not found.'
}
Assert-Match $sdoTryStartBlock '(?s)ObjectIndex\s*:\s*UINT;.*?SubIndex\s*:\s*USINT;.*?ReadLength\s*:\s*UINT;.*?ReadLength <> 1.*?ReadLength <> 2.*?ReadLength <> 4.*?sigclib_atomic_cmpxchgU32\(\s*pValue:=#AdapterState,\s*cmpVal:=LMC_SDO_EXEC_IDLE,\s*newVal:=LMC_SDO_EXEC_ARMING\).*?ActiveIndex := ObjectIndex;.*?ActiveSubIndex := SubIndex;.*?ActiveLength := ReadLength;.*?cmpVal:=LMC_SDO_EXEC_ARMING,\s*newVal:=LMC_SDO_EXEC_RUNNING.*?toSlave\.StartReadSDO\(\s*ObjectIndex\$HINT,\s*SubIndex\$HSINT,\s*0,\s*\(#ReadBuffer\[0\]\)\$\^USINT,\s*TO_UDINT\(ReadLength\),\s*TimeoutMs,\s*THIS\)' 'LMCSdoExecutor must publish Running before exposing its exact 1/2/4-byte vendor request and callback buffer.'
if ([regex]::Matches($sdoTryStartBlock, 'toSlave\.StartReadSDO\(').Count -ne 1) {
    throw 'LMCSdoExecutor.TryStartRead must expose exactly one vendor SDO request.'
}
Assert-Match $sdoTryStartBlock '(?s)IsClientConnected\(#toSlave\) = FALSE.*?cmpVal:=LMC_SDO_EXEC_ARMING,\s*newVal:=LMC_SDO_EXEC_RELEASING.*?if previousState = LMC_SDO_EXEC_ARMING then.*?ActiveToken := 0.*?_memset\(dest:=#ReadBuffer\[0\].*?cmpVal:=LMC_SDO_EXEC_RELEASING,\s*newVal:=LMC_SDO_EXEC_IDLE.*?if previousState <> LMC_SDO_EXEC_RELEASING then.*?value:=LMC_SDO_EXEC_QUARANTINED.*?else\s*sigclib_atomic_setU32\(\s*pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED\)' 'LMCSdoExecutor disconnected rollback can overwrite an unsolicited callback or expose Idle before cleanup.'
Assert-Match $sdoTryStartBlock '(?s)startResult <> READY.*?cmpVal:=LMC_SDO_EXEC_RUNNING,\s*newVal:=LMC_SDO_EXEC_RELEASING.*?if previousState = LMC_SDO_EXEC_RUNNING then.*?ActiveToken := 0.*?cmpVal:=LMC_SDO_EXEC_RELEASING,\s*newVal:=LMC_SDO_EXEC_IDLE' 'LMCSdoExecutor does not exclusively clear and release an unaccepted vendor request.'
Assert-Match $sdoTryStartBlock '(?s)if startResult <> READY then.*?value:=LMC_SDO_EXEC_QUARANTINED\);\s*ret_code := ERROR;.*?end_if;\s*end_if;\s*END_FUNCTION' 'LMCSdoExecutor does not preserve an unaccepted-request invariant failure as hard quarantine.'
if ($sdoTryStartBlock -match '(?s)if startResult <> READY then.*?ret_code := READY') {
    throw 'LMCSdoExecutor incorrectly promotes an unaccepted vendor request to Ready.'
}

$sdoCopyCompletionBlock = [regex]::Match(
    $sdoExecutor,
    '(?s)FUNCTION GLOBAL LMCSdoExecutor::CopyCompletion.*?END_FUNCTION').Value
Assert-Match $sdoCopyCompletionBlock '(?s)stateValue := sigclib_atomic_getU32\(pValue:=#AdapterState\);\s*if stateValue <> LMC_SDO_EXEC_RESULT_READY then\s*Result := -2;\s*RETURN;\s*end_if;.*?retryCount < 3.*?sequenceBefore := sigclib_atomic_getU32.*?_memcpy.*?sequenceAfter := sigclib_atomic_getU32.*?sequenceBefore = sequenceAfter.*?localResult\.Token <> ExpectedToken.*?value:=LMC_SDO_EXEC_QUARANTINED' 'LMCSdoExecutor completion copy lacks ResultReady-only admission, bounded seqlock, or token validation.'
if ($sdoCopyCompletionBlock -match 'stateValue <> LMC_SDO_EXEC_QUARANTINED') {
    throw 'LMCSdoExecutor.CopyCompletion must not recover a hard-quarantined adapter.'
}
Assert-Match $sdoCopyCompletionBlock '(?s)cmpVal:=stateValue,\s*newVal:=LMC_SDO_EXEC_RELEASING.*?if previousState <> stateValue then.*?RETURN;\s*end_if;\s*ActiveToken := 0.*?_memset\(dest:=#ReadBuffer\[0\].*?_memset\(dest:=#PublishedResult.*?cmpVal:=LMC_SDO_EXEC_RELEASING,\s*newVal:=LMC_SDO_EXEC_IDLE.*?if previousState <> LMC_SDO_EXEC_RELEASING then.*?RETURN;\s*end_if;.*?_memcpy\(ptr1:=pDest, ptr2:=#localResult' 'LMCSdoExecutor does not exclusively clear and release a consumed owned completion before exposing Idle.'

$sdoMarkOrphanBlock = [regex]::Match(
    $sdoExecutor,
    '(?s)FUNCTION GLOBAL LMCSdoExecutor::MarkOrphan.*?END_FUNCTION').Value
Assert-Match $sdoMarkOrphanBlock '(?s)ExpectedToken = 0.*?ActiveToken <> ExpectedToken.*?sigclib_atomic_cmpxchgU32\(\s*pValue:=#AdapterState,\s*cmpVal:=LMC_SDO_EXEC_RUNNING,\s*newVal:=LMC_SDO_EXEC_ORPHANED\)' 'LMCSdoExecutor does not atomically orphan only the expected running token.'
Assert-Match $sdoExecutor '(?s)FUNCTION GLOBAL LMCSdoExecutor::IsReusable.*?sigclib_atomic_getU32\(\s*pValue:=#AdapterState\)\s*=\s*LMC_SDO_EXEC_IDLE.*?END_FUNCTION' 'LMCSdoExecutor reusable state is not an atomic Idle-only check.'

$sdoCallbackBlock = [regex]::Match(
    $sdoExecutor,
    '(?s)FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ClassState::NewInst.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($sdoCallbackBlock)) {
    throw 'LMCSdoExecutor.ClassState.NewInst callback implementation was not found.'
}
Assert-Match $sdoCallbackBlock '(?s)pPara\^\.uiCmd <> ECAT_M_SDO_CALLBACK.*?ret_code := EtherCAT_SDOBase::NewInst\(pPara, pResult\).*?RETURN' 'LMCSdoExecutor does not forward unknown commands to EtherCAT_SDOBase.'
Assert-Match $sdoCallbackBlock '(?s)callbackIsWrite := pPara\^\.aPara\[2\]\$USINT.*?callbackIndex := pPara\^\.aPara\[3\]\$UINT.*?callbackSubIndex := pPara\^\.aPara\[4\]\$USINT.*?osResult := pPara\^\.aPara\[1\]\$DINT.*?actualLength := pPara\^\.aPara\[5\]\$UDINT.*?abortCode := pPara\^\.aPara\[6\]\$UDINT' 'LMCSdoExecutor callback metadata extraction is incomplete.'
Assert-Match $sdoCallbackBlock '(?s)aPara\[0\]\$DINT <> 1.*?stateValue <> LMC_SDO_EXEC_RUNNING.*?stateValue <> LMC_SDO_EXEC_ORPHANED.*?callbackIsWrite <> 0.*?callbackIndex <> ActiveIndex.*?callbackSubIndex <> ActiveSubIndex.*?ActiveToken = 0.*?actualLength <> TO_UDINT\(ActiveLength\)' 'LMCSdoExecutor callback version/state/direction/index/subindex/token/length validation is incomplete.'
Assert-Match $sdoCallbackBlock '(?s)stateValue <> LMC_SDO_EXEC_RUNNING.*?stateValue <> LMC_SDO_EXEC_ORPHANED.*?ActiveToken = 0.*?value:=LMC_SDO_EXEC_QUARANTINED.*?RETURN' 'LMCSdoExecutor does not keep unsolicited or duplicate callbacks quarantined.'
Assert-Match $sdoCallbackBlock '(?s)if stateValue = LMC_SDO_EXEC_ORPHANED then.*?cmpVal:=LMC_SDO_EXEC_ORPHANED,\s*newVal:=LMC_SDO_EXEC_RELEASING.*?if previousState <> LMC_SDO_EXEC_ORPHANED then.*?RETURN;\s*end_if;\s*ActiveToken := 0.*?_memset\(dest:=#ReadBuffer\[0\].*?cmpVal:=LMC_SDO_EXEC_RELEASING,\s*newVal:=LMC_SDO_EXEC_IDLE.*?if previousState <> LMC_SDO_EXEC_RELEASING then.*?value:=LMC_SDO_EXEC_QUARANTINED.*?end_if;\s*RETURN' 'LMCSdoExecutor does not drain every owned late orphan callback back to Idle.'
Assert-Match $sdoCallbackBlock '(?s)writeSequence := sigclib_atomic_getU32.*?writeSequence and 1.*?sigclib_atomic_setU32\(\s*pValue:=#PublishSequence, value:=writeSequence\).*?PublishedResult\.Token := ActiveToken.*?PublishedResult\.ValidationCode := validationCode.*?PublishedResult\.Data := ReadBuffer\[0\]\$UDINT.*?finalSequence := writeSequence \+ 1.*?value:=finalSequence.*?cmpVal:=LMC_SDO_EXEC_RUNNING,\s*newVal:=LMC_SDO_EXEC_RESULT_READY' 'LMCSdoExecutor owned callback publication is not an atomic seqlock result that remains consumable after validation failure.'
Assert-Match $sdoCallbackBlock '(?s)previousState = LMC_SDO_EXEC_ORPHANED.*?cmpVal:=LMC_SDO_EXEC_ORPHANED,\s*newVal:=LMC_SDO_EXEC_RELEASING.*?ActiveToken := 0.*?_memset\(dest:=#PublishedResult.*?cmpVal:=LMC_SDO_EXEC_RELEASING,\s*newVal:=LMC_SDO_EXEC_IDLE.*?previousState <> LMC_SDO_EXEC_RUNNING.*?value:=LMC_SDO_EXEC_QUARANTINED' 'LMCSdoExecutor does not resolve the callback-publication versus orphan race without overwriting the orphan state.'

Assert-Match $diagnosticsService '#define LMC_DIAG_D1_ENABLED\s+TRUE' 'D1 Health/Catalog/PI Read is not enabled.'
Assert-Match $diagnosticsService '#define LMC_DIAG_D2_ENABLED\s+TRUE' 'D2 Bulk Snapshot is not enabled.'
Assert-Match $diagnosticsService '#define LMC_DIAG_D3_ENABLED\s+TRUE' 'D3 single-bank Recorder is not enabled.'
Assert-Match $diagnosticsService '#define LMC_DIAG_D5_SDO_READ_ENABLED\s+TRUE' 'D5 general inline SDO Read gate must remain TRUE while the test project advertises bits 8 and 13 with MaxSdoDataBytes=4.'
Assert-NoCaseInsensitiveMemberShadowing $diagnosticsService 'LMCDiagnosticsService'
if ([regex]::Matches($diagnosticsService, '<Client Name="SdoAxis[1-4]" Required="true" Internal="false"/>').Count -ne 4 -or
    [regex]::Matches($diagnosticsService, 'SdoAxis[1-4]\s*:\s*CltChCmd_LMCSdoExecutor;').Count -ne 4) {
    throw 'LMCDiagnosticsService does not declare exactly four required LMCSdoExecutor clients.'
}
Assert-Match $diagnosticsService '#define LMC_DIAG_MAP_REVISION\s+0x957F101E' 'LMCDiagnosticsService MapRevision is not the canonical D1 catalog CRC.'
Assert-Match $diagnosticsService 'Server Name="DiagnosticsBootCounter".*Initialize="true".*DefValue="0".*Retentive="File"' 'LMCDiagnosticsService retained DiagnosticsBootCounter metadata is missing.'
Assert-Match $diagnosticsService '(?s)FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId.*?DiagnosticsBootCounter\.Read\(\).*?nextBootId = 0xFFFFFFFF.*?DiagnosticsBootCounter\.Write\(input:=nextBootId\).*?DiagnosticsBootCounter\.Read\(\) = nextBootId.*?BootIdFault := TRUE.*?END_FUNCTION' 'LMCDiagnosticsService retained BootId generation or write verification is incomplete.'
Assert-Match $diagnosticsService '(?s)FUNCTION LMCDiagnosticsService::BuildCatalogEntry.*?CatalogIndex >= 24.*?pEntry \+ 76.*?:= 0' 'LMCDiagnosticsService fixed 80-byte catalog entry builder is incomplete.'
Assert-Match $diagnosticsService '(?s)FUNCTION GLOBAL LMCDiagnosticsService::HandleRequest.*?0x7E01:.*?0x7E02:.*?0x7E10:.*?0x7E20:' 'LMCDiagnosticsService D1 command handlers are missing.'
Assert-Match $diagnosticsService '(?s)InputLatch\.CopySnapshot\(.*?DestSize:=sizeof\(snapshot\).*?ResponseSize := 200' 'EtherCAT Health does not use the immutable latch snapshot.'
Assert-Match $diagnosticsService '(?s)entryStatus := 0.*?entryStatus := entryStatus or 4.*?entryStatus := entryStatus or 2.*?entryStatus := 1' 'PI Read entry validity/staleness status construction is incomplete.'

$diagnosticsServiceHandleBlock = [regex]::Match(
    $diagnosticsService,
    '(?s)FUNCTION GLOBAL LMCDiagnosticsService::HandleRequest.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($diagnosticsServiceHandleBlock)) {
    throw 'LMCDiagnosticsService.HandleRequest implementation was not found.'
}
Assert-Match $diagnosticsServiceHandleBlock '(?s)currentBootId := GetDiagnosticsBootId\(\).*?\(CommandId >= 0x7E30\).*?\(CommandId <= 0x7E33\).*?\(CommandId >= 0x7E40\).*?\(CommandId <= 0x7E49\).*?currentBootId = 0.*?detailCode := 11' 'LMCDiagnosticsService does not fail closed for raw stateful D2/D3 calls when BootId is unavailable.'
Assert-Match $diagnosticsServiceHandleBlock '(?s)if \(CommandId >= 0x7E40\)\s*&\s*\(CommandId <= 0x7E49\).*?IsClientConnected\(#RecorderStore\) = FALSE.*?\(pResponse \+ 4\)\^\$UINT := 1.*?\(pResponse \+ 12\)\^\$UDINT := 11.*?ResponseSize := 16.*?RETURN' 'LMCDiagnosticsService RecorderStore disconnected path is not fail-closed.'
Assert-Match $diagnosticsServiceHandleBlock '(?s)RecorderStore\.HandleRequest\(.*?CommandId:=CommandId.*?CallerSessionEpoch:=CallerSessionEpoch.*?CurrentDiagnosticsBootId:=currentBootId.*?ResponseCapacity:=ResponseCapacity\)' 'LMCDiagnosticsService does not delegate D3 requests with the retained runtime BootId.'
Assert-Match $diagnosticsServiceHandleBlock '(?s)0x7E21:\s*.*?if RequestSize <> 28 then\s*detailCode := 12;\s*else\s*detailCode := 2;\s*end_if' 'SubmitPIWrite 0x7E21 must validate its 28-byte reserved wire and remain UnsupportedFeature.'

$sdoStatusBlock = [regex]::Match(
    $diagnosticsServiceHandleBlock,
    '(?s)0x7E03:.*?0x7E04:').Value
$sdoCancelBlock = [regex]::Match(
    $diagnosticsServiceHandleBlock,
    '(?s)0x7E04:.*?0x7E21:').Value
$sdoSubmitBlock = [regex]::Match(
    $diagnosticsServiceHandleBlock,
    '(?s)0x7E50:.*?0x7E51:').Value
foreach ($sdoHandler in @(
    @{ Name = 'GetOperationStatus 0x7E03'; Block = $sdoStatusBlock },
    @{ Name = 'CancelOperation 0x7E04'; Block = $sdoCancelBlock },
    @{ Name = 'SubmitSDO 0x7E50'; Block = $sdoSubmitBlock })) {
    if ([string]::IsNullOrWhiteSpace($sdoHandler.Block)) {
        throw "$($sdoHandler.Name) implementation was not found."
    }
}

Assert-Match $sdoStatusBlock '(?s)RequestSize <> 16.*?LMC_DIAG_D5_SDO_READ_ENABLED = FALSE.*?ResponseCapacity < 64.*?sdoTicketId := \(pRequest \+ 8\)\^\$UDINT.*?sdoBootId := \(pRequest \+ 12\)\^\$UDINT.*?sdoTicketId <> TicketId.*?sdoBootId <> TicketBootId.*?CallerSessionEpoch <> OwnerSessionEpoch.*?\(pResponse \+ 16\)\^\$UDINT := TicketId.*?\(pResponse \+ 22\)\^\$UINT := OperationState.*?\(pResponse \+ 32\)\^\$UINT := OperationOutcome.*?\(pResponse \+ 60\)\^\$UDINT := TicketBootId.*?ResponseSize := 64' 'GetOperationStatus 0x7E03 does not validate ticket/boot/session ownership and return the fixed D5 status envelope.'
Assert-Match $sdoStatusBlock '(?s)OperationState = LMC_DIAG_SDO_STATE_COMPLETED.*?OperationOutcome = LMC_DIAG_SDO_OUTCOME_SUCCESS.*?\(pResponse \+ 40\)\^\$UDINT := SdoResultLength.*?\(pResponse \+ 44\)\^\$USINT := SdoValueType.*?\(pResponse \+ 45\)\^\$USINT := SdoResultLength\$USINT.*?\(pResponse \+ 48\)\^\$UDINT := SdoResultData' 'GetOperationStatus 0x7E03 does not expose exact typed data only for a successful completed operation.'

Assert-Match $sdoCancelBlock '(?s)RequestSize <> 16.*?LMC_DIAG_D5_SDO_READ_ENABLED = FALSE.*?ResponseCapacity < 28.*?sdoTicketId <> TicketId.*?sdoBootId <> TicketBootId.*?CallerSessionEpoch <> OwnerSessionEpoch.*?OperationState <> LMC_DIAG_SDO_STATE_QUEUED.*?detailCode := 19.*?OperationState := LMC_DIAG_SDO_STATE_CANCELLED.*?OperationOutcome := LMC_DIAG_SDO_OUTCOME_CANCELLED.*?ResponseSize := 28' 'CancelOperation 0x7E04 is not restricted to the owning queued ticket.'

if ([regex]::Matches($diagnosticsService, '(?m)^\s*TicketId\s*:\s*UDINT;').Count -ne 1 -or
    $diagnosticsService -match '(?m)^\s*TicketId\s*:\s*ARRAY') {
    throw 'LMCDiagnosticsService must own one global D5 ticket, not a ticket array.'
}
Assert-Match $sdoSubmitBlock '(?s)RequestSize < 32.*?expectedMapRevision := \(pRequest \+ 8\)\^\$UDINT.*?requestSdoSlaveReference := \(pRequest \+ 12\)\^\$UINT.*?sdoOperationFlags := \(pRequest \+ 14\)\^\$UINT.*?requestSdoObjectIndex := \(pRequest \+ 16\)\^\$UINT.*?requestSdoSubIndex := \(pRequest \+ 18\)\^\$USINT.*?requestSdoValueType := \(pRequest \+ 19\)\^\$USINT.*?requestSdoTimeoutCycles := \(pRequest \+ 20\)\^\$UDINT.*?sdoDataLength := \(pRequest \+ 24\)\^\$UINT.*?sdoReserved := \(pRequest \+ 26\)\^\$UINT.*?sdoBootId := \(pRequest \+ 28\)\^\$UDINT.*?expectedRequestSize := 32.*?sdoOperationFlags = 1.*?expectedRequestSize \+= TO_UDINT\(sdoDataLength\).*?RequestSize <> expectedRequestSize' 'SubmitSDO 0x7E50 generic request envelope validation is incomplete.'
Assert-Match $sdoSubmitBlock '(?s)LMC_DIAG_D5_SDO_READ_ENABLED = FALSE.*?requestSdoSlaveReference < 1.*?requestSdoSlaveReference > 4.*?requestSdoTimeoutCycles < 1.*?requestSdoTimeoutCycles > 60000.*?expectedMapRevision <> LMC_DIAG_MAP_REVISION.*?sdoBootId <> currentBootId.*?sdoOperationFlags = 1.*?sdoDataLength > 4.*?requestSdoObjectIndex = 0' 'SubmitSDO 0x7E50 does not enforce the gated read-only axes 1..4, timeout, identity, inline capacity, and nonzero object-index policy.'
Assert-Match $sdoSubmitBlock '(?s)sdoDataLength <> 1.*?sdoDataLength <> 2.*?sdoDataLength <> 4.*?requestSdoValueType < 1.*?requestSdoValueType > 11' 'SubmitSDO 0x7E50 does not bound general Read lengths and SDO ValueType codes.'
Assert-Match $sdoSubmitBlock '(?s)requestSdoValueType = 1.*?requestSdoValueType = 9.*?requestSdoValueType = 10.*?requestSdoValueType = 11.*?sdoDataLength <> 1.*?requestSdoValueType = 2.*?requestSdoValueType = 3.*?requestSdoValueType = 7.*?sdoDataLength <> 2.*?requestSdoValueType = 4.*?requestSdoValueType = 5.*?requestSdoValueType = 6.*?requestSdoValueType = 8.*?sdoDataLength <> 4' 'SubmitSDO 0x7E50 does not enforce exact 8/16/32-bit ValueType-to-length mapping.'
Assert-Match $sdoSubmitBlock '(?s)SdoSlaveReference := requestSdoSlaveReference;.*?SdoObjectIndex := requestSdoObjectIndex;.*?SdoSubIndex := requestSdoSubIndex;.*?SdoValueType := requestSdoValueType;.*?SdoRequestedLength := sdoDataLength;.*?SdoTimeoutCycles := requestSdoTimeoutCycles;' 'SubmitSDO 0x7E50 does not copy parsed request values into the retained ticket state.'
Assert-Match $sdoSubmitBlock '(?s)OperationState = LMC_DIAG_SDO_STATE_QUEUED.*?OperationState = LMC_DIAG_SDO_STATE_RUNNING.*?SdoInternalDrainState <> 0.*?detailCode := 9.*?case requestSdoSlaveReference of.*?SdoAxis1\.IsReusable\(\).*?SdoAxis4\.IsReusable\(\).*?NextTicketId = 0xFFFFFFFF.*?NextOperationToken = 0xFFFFFFFF.*?NextTicketId \+= 1.*?NextOperationToken \+= 1.*?TicketId := NextTicketId.*?OperationToken := NextOperationToken.*?OperationState := LMC_DIAG_SDO_STATE_QUEUED.*?SdoInternalDrainState := 0.*?ResponseSize := 32' 'SubmitSDO 0x7E50 does not allocate exactly one reusable queued ticket with wrap and drain guards.'
Assert-Match $sdoSubmitBlock '(?s)executorConnected = FALSE.*?detailCode := 11.*?executorReusable = FALSE.*?detailCode := 24' 'SubmitSDO 0x7E50 does not distinguish a disconnected executor from an unowned non-Idle invariant fault.'
Assert-Match $diagnosticsServiceHandleBlock '(?s)0x7E51:\s*.*?if RequestSize <> 28 then\s*detailCode := 12;\s*else\s*detailCode := 2;\s*end_if' 'ReadSDOResultChunk 0x7E51 must validate its 28-byte reserved wire and remain UnsupportedFeature.'
Assert-Match $diagnosticsServiceHandleBlock '(?s)if detailCode <> 0 then.*?\(pResponse \+ 4\)\^\$UINT := 1.*?ResponseSize := 16' 'LMCDiagnosticsService reserved and error commands do not return the common 16-byte error envelope.'

$sdoProcessBlock = [regex]::Match(
    $diagnosticsService,
    '(?s)FUNCTION GLOBAL LMCDiagnosticsService::ProcessOperations.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($sdoProcessBlock)) {
    throw 'LMCDiagnosticsService.ProcessOperations implementation was not found.'
}
Assert-Match $sdoProcessBlock 'completion\s*:\s*LMCSdoExecutor::LMCSdoExecutorResult;' 'ProcessOperations does not use the derived executor public result type with its class qualifier.'
if ($sdoProcessBlock -match 'completion\s*:\s*LMCSdoExecutorResult;') {
    throw 'ProcessOperations uses an unqualified LMCSdoExecutorResult type that LASAL C78 cannot resolve.'
}
$typedSdoConnectionChecks = [regex]::Matches(
    $diagnosticsService,
    'executorConnected\s*:=\s*IsClientConnected\(#SdoAxis[1-4]\)\s*<>\s*0;')
if ($typedSdoConnectionChecks.Count -ne 12 -or
    $diagnosticsService -match 'executorConnected\s*:=\s*IsClientConnected\(#SdoAxis[1-4]\)\s*;') {
    throw 'LMCDiagnosticsService must convert all twelve SdoAxis connection checks from DINT to BOOL explicitly.'
}
Assert-Match $sdoProcessBlock '(?s)LMC_DIAG_D5_SDO_READ_ENABLED = FALSE.*?RETURN.*?TicketId = 0.*?SdoInternalDrainState = 0' 'ProcessOperations does not remain inert behind the D5 compile gate and empty-ticket guard.'
Assert-Match $sdoProcessBlock '(?s)SdoInternalDrainState <> 0.*?IsSdoReadReady\(SlaveReference:=SdoSlaveReference\) = FALSE.*?CopyCompletion\(\s*ExpectedToken:=OperationToken.*?IsSdoReadReady\(SlaveReference:=SdoSlaveReference\) then.*?SdoInternalDrainState := 0.*?RETURN' 'ProcessOperations does not drain late timeout/disconnect callbacks before releasing the executor.'
Assert-Match $sdoProcessBlock '(?s)OperationState = LMC_DIAG_SDO_STATE_RUNNING.*?CopyCompletion\(\s*ExpectedToken:=OperationToken.*?elapsedCycles := currentCycle - SdoSubmitCycle.*?if \(completionResult = 0\)\s*&\s*\(elapsedCycles > SdoTimeoutCycles\) then.*?elsif completionResult = 0 then.*?OperationState := LMC_DIAG_SDO_STATE_COMPLETED.*?RETURN.*?elapsedCycles >= SdoTimeoutCycles.*?MarkOrphan\(\s*ExpectedToken:=OperationToken\).*?OperationState := LMC_DIAG_SDO_STATE_EXPIRED.*?SdoInternalDrainState := LMC_DIAG_SDO_DRAIN_EXPIRED' 'ProcessOperations must consume a completion at the deadline before timeout and quarantine an incomplete timed-out adapter for late-callback drain.'
Assert-Match $sdoProcessBlock '(?s)completion\.ValidationCode = 7.*?SdoOperationDetail := 5.*?else\s*SdoOperationDetail := 24.*?completion\.OsResult <> 0.*?completion\.AbortCode = 0x08000000.*?SdoOperationDetail := completion\.OsResult\$UDINT.*?elsif completion\.AbortCode <> 0.*?elsif completion\.ActualLength <> TO_UDINT\(SdoRequestedLength\) then.*?SdoOperationDetail := 5.*?completion\.ObjectIndex <> SdoObjectIndex.*?SdoOperationDetail := 24' 'ProcessOperations does not preserve the general-read validation, OS/abort priority, exact length, and metadata error mapping.'
Assert-Match $sdoProcessBlock '(?s)OperationState <> LMC_DIAG_SDO_STATE_QUEUED.*?OperationState <> LMC_DIAG_SDO_STATE_RUNNING.*?currentCycle = SdoLastProcessedCycle.*?remainingCycles := SdoTimeoutCycles - elapsedCycles.*?case SdoSlaveReference of.*?SdoAxis1\.TryStartRead\(.*?ReadLength:=SdoRequestedLength.*?SdoAxis4\.TryStartRead\(.*?ReadLength:=SdoRequestedLength.*?startResult = READY.*?OperationState := LMC_DIAG_SDO_STATE_RUNNING' 'ProcessOperations does not start one exact-length queued read per published RT cycle through the selected executor.'

$diagnosticsServiceNotifyBlock = [regex]::Match(
    $diagnosticsService,
    '(?s)FUNCTION GLOBAL LMCDiagnosticsService::NotifySessionClosed.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($diagnosticsServiceNotifyBlock)) {
    throw 'LMCDiagnosticsService.NotifySessionClosed implementation was not found.'
}
Assert-Match $diagnosticsServiceNotifyBlock '(?s)SessionEpoch = BulkOwnerSessionEpoch.*?BulkState := 0.*?RecorderStore\.NotifySessionClosed\(SessionEpoch:=SessionEpoch\)' 'LMCDiagnosticsService does not release the matching Bulk owner and notify RecorderStore on session close.'
Assert-Match $diagnosticsService '(?s)FUNCTION LMCDiagnosticsService::@STD.*?ret_code\s*:=\s*LMCDiagnosticsService\(\).*?END_FUNCTION' 'LMCDiagnosticsService @STD does not invoke its constructor.'
Assert-Match $diagnosticsService '(?s)FUNCTION LMCDiagnosticsService::LMCDiagnosticsService.*?NextBulkId := 0.*?BulkState := 0.*?_memset\(dest:=#BulkSignalIds\[0\].*?ret_code := C_OK.*?END_FUNCTION' 'LMCDiagnosticsService constructor does not initialize its complete Bulk state.'

Assert-Match $recorderStore '(?s)VAR_GLOBAL\s+g_LMCRecorderData\s*:\s*ARRAY \[0\.\.1279999\] OF USINT;\s*END_VAR' 'LMCRecorderStore fixed 1,280,000-byte global recorder bank is missing.'
Assert-Match $recorderStore '#define LMC_RECORDER_STORAGE_BYTES\s+1280000' 'LMCRecorderStore storage-size constant does not match the global recorder bank.'
Assert-Match $recorderStore '(?s)stride := TO_UDINT\(requestedChannelCount\) \* 4;.*?acceptedCapacity := LMC_RECORDER_STORAGE_BYTES / stride;.*?if acceptedCapacity > requestedCapacity then\s*acceptedCapacity := requestedCapacity;\s*end_if' 'LMCRecorderStore ConfigureRecorder does not clamp AcceptedCapacity to the fixed bank size and requested sample count.'
Assert-Match $recorderStore '(?s)FUNCTION LMCRecorderStore::@STD.*?ret_code\s*:=\s*LMCRecorderStore\(\).*?END_FUNCTION' 'LMCRecorderStore @STD does not invoke its constructor.'
Assert-Match $recorderStore '(?s)FUNCTION LMCRecorderStore::LMCRecorderStore.*?StateValue := LMC_RECORDER_EMPTY.*?SamplePeriodCycles := 1.*?NextConfigId := 1.*?NextRecordId := 1.*?BufferReleased := TRUE.*?ret_code := C_OK.*?END_FUNCTION' 'LMCRecorderStore constructor does not initialize recorder identity, timing, and ownership state.'
Assert-Match $recorderStore '(?s)elsif \(CurrentDiagnosticsBootId = 0\) then\s*detailCode := 11' 'LMCRecorderStore does not reject the BootId-zero sentinel before stateful D3 processing.'

$recorderHandleRequestBlock = [regex]::Match(
    $recorderStore,
    '(?s)FUNCTION GLOBAL LMCRecorderStore::HandleRequest.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($recorderHandleRequestBlock)) {
    throw 'LMCRecorderStore.HandleRequest implementation was not found.'
}
foreach ($recorderCommandId in @(
    '0x7E40', '0x7E41', '0x7E42', '0x7E43', '0x7E44',
    '0x7E45', '0x7E46', '0x7E47', '0x7E48', '0x7E49')) {
    $recorderCommandCount = [regex]::Matches(
        $recorderHandleRequestBlock,
        "(?m)^\s*$recorderCommandId\s*:").Count
    if ($recorderCommandCount -ne 1) {
        throw "LMCRecorderStore command $recorderCommandId handler count is $recorderCommandCount, expected one."
    }
}
Assert-Match $recorderHandleRequestBlock '(?s)0x7E42:\s*.*?RequestSize <> 28.*?requestRecordId := \(pRequest \+ 8\)\^\$UDINT.*?requestBufferId := \(pRequest \+ 12\)\^\$UDINT.*?expectedMapRevision := \(pRequest \+ 16\)\^\$UDINT.*?requestOwnerEpoch := \(pRequest \+ 20\)\^\$UDINT.*?requestBootId := \(pRequest \+ 24\)\^\$UDINT.*?TriggerType = 0.*?TriggerRequestSequence.*?ResponseSize := 16.*?0x7E43:' 'TriggerRecorder 0x7E42 does not validate identity/ownership and queue an RT trigger request.'
Assert-Match $recorderHandleRequestBlock '(?s)0x7E43:.*?requestRecordId <> RecordId.*?expectedMapRevision <> MapRevision.*?requestBootId <> DiagnosticsBootId.*?requestOwnerEpoch <> OwnerSessionEpoch.*?state = LMC_RECORDER_READY.*?state = LMC_RECORDER_UPLOADING.*?ResponseSize := 16.*?state <> LMC_RECORDER_ARMED.*?state <> LMC_RECORDER_RECORDING.*?detailCode := 19.*?StopRequestSequence.*?ResponseSize := 16.*?0x7E44:' 'StopRecorder must preserve identity/ownership checks, acknowledge Ready/Uploading idempotently, and queue only active-state stops.'
Assert-Match $recorderHandleRequestBlock '(?s)if detailCode <> 0 then.*?\(pResponse \+ 4\)\^\$UINT := 1.*?ResponseSize := 16' 'LMCRecorderStore reserved and error commands do not return the common 16-byte error envelope.'
Assert-Match $recorderStore '(?s)FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot.*?StateValue.*?g_LMCRecorderData.*?SampleCount \+= 1.*?END_FUNCTION' 'LMCRecorderStore RT AppendSnapshot capture path is incomplete.'
Assert-Match $recorderStore '(?s)FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot.*?prehistoryReady := SampleCount >= PreTriggerSamples.*?TriggerType = 1.*?TriggerType = 2.*?case TriggerOperator of.*?LMC_RECORDER_STOP_TRIGGER_COMPLETE.*?END_FUNCTION' 'LMCRecorderStore D4 edge/window/mask RT trigger path is incomplete.'
Assert-Match $recorderStore '(?s)triggerInputValid :=\s*\(\(pSnapshot \+ 12\)\^\$UDINT = 8\) &\s*\(\(pSnapshot \+ 16\)\^\$UDINT = 0\) &\s*\(\(pSnapshot \+ triggerHealthOffset\)\^\$DINT <> 0\) &\s*\(\(pSnapshot \+ triggerHealthOffset \+ 4\)\^\$UDINT = 8\) &\s*\(\(pSnapshot \+ triggerHealthOffset \+ 12\)\^\$UDINT = 0\)' 'LMCRecorderStore trigger validity must require master OP/no missed frame and axis Online/OP/AL=0.'
Assert-Match $recorderStore '(?s)triggerHealthOffset := 64.*?TriggerSignalId.*?triggerInputValid :=.*?triggerHealthOffset.*?prehistoryReady := SampleCount >= PreTriggerSamples.*?if prehistoryReady then.*?triggerRequest <> TriggerAppliedSequence.*?triggerEvent := TRUE.*?elsif triggerInputValid then.*?TriggerType = 1.*?TriggerType = 2.*?case TriggerOperator of' 'LMCRecorderStore automatic edge/window/mask trigger evaluation is not gated by a valid EtherCAT trigger sample.'
Assert-Match $recorderStore '(?s)if triggerInputValid then\s*PreviousTriggerValue := triggerRaw;\s*PreviousTriggerValid := TRUE;\s*else\s*.*?PreviousTriggerValid := FALSE;\s*end_if' 'LMCRecorderStore does not reset edge/window history across an invalid EtherCAT trigger sample.'
Assert-Match $recorderStore '(?s)FrozenFirstSampleIndex :=.*?WriteSampleIndex \+ SampleCapacity - SampleCount.*?FUNCTION GLOBAL LMCRecorderStore::HandleRequest.*?physicalSampleIndex :=.*?FrozenFirstSampleIndex \+ offsetSample.*?_memcpy' 'LMCRecorderStore does not preserve and upload pre-trigger ring data in chronological order.'
Assert-Match $recorderStore '(?s)stopRequest <> StopAppliedSequence.*?TriggerIndex = 0xFFFFFFFF.*?FrozenFirstSampleIndex :=\s*\(WriteSampleIndex \+ SampleCapacity - SampleCount\).*?StopReason := LMC_RECORDER_STOP_USER' 'LMCRecorderStore does not freeze chronological pre-trigger ring order when the user stops before a trigger.'
Assert-Match $recorderStore '(?s)StopReason := LMC_RECORDER_STOP_USER;\s*if SampleCount = 0 then.*?EndCycle := cycleCounter' 'LMCRecorderStore user stop must preserve the End metadata of the last copied sample.'
Assert-Match $recorderHandleRequestBlock '(?s)0x7E42:.*?TriggerType = 0.*?TriggerIndex <> 0xFFFFFFFF then.*?detailCode := 19.*?TriggerRequestSequence' 'TriggerRecorder must reject a second force-trigger after the current record has already triggered.'
Assert-Match $recorderHandleRequestBlock '(?s)0x7E40:.*?bufferMode = 2 then\s*detailCode := 2.*?triggerType <> 0.*?bufferMode <> 1.*?expectedTriggerValueType.*?preTriggerSamples >= requestedCapacity.*?triggerOperator < 5.*?triggerValue <> 0.*?TriggerSignalOffset := triggerSignalOffset' 'ConfigureRecorder does not fail closed for double bank or fully validate and publish a D4 ring trigger configuration.'
Assert-Match $recorderStore '(?s)triggerHealthOffset := 64 \+\s*\(\(\(TriggerSignalId shr 8\) and 0xFF\) - 1\) \* 36' 'AppendSnapshot does not bind trigger validity to the configured physical axis health image.'
Assert-Match $recorderStore '(?s)FUNCTION GLOBAL LMCRecorderStore::NotifySessionClosed.*?SessionEpoch = OwnerSessionEpoch.*?ClosedSessionEpoch := SessionEpoch.*?END_FUNCTION' 'LMCRecorderStore does not retain the closed owner epoch for Recorder adoption.'
Assert-Match $recorderHandleRequestBlock '(?s)0x7E49:.*?requestRecordId := \(pRequest \+ 8\)\^\$UDINT.*?requestBufferId := \(pRequest \+ 12\)\^\$UDINT.*?requestBootId := \(pRequest \+ 16\)\^\$UDINT.*?if requestRecordId = 0 then.*?requestBufferId <> 0.*?detailCode := 22.*?requestBootId <> DiagnosticsBootId.*?requestBootId <> CurrentDiagnosticsBootId.*?detailCode := 25.*?RecordId = 0.*?BufferId <> 0.*?detailCode := 22.*?state < LMC_RECORDER_ARMED.*?state > LMC_RECORDER_UPLOADING.*?ClosedSessionEpoch = 0.*?ClosedSessionEpoch <> OwnerSessionEpoch' 'AdoptRecorder 0x7E49 does not implement the fail-closed 0/0 active single-bank discovery sentinel.'
Assert-Match $recorderHandleRequestBlock '(?s)0x7E49:.*?if requestRecordId = 0 then.*?else\s*.*?requestRecordId <> RecordId.*?requestBufferId <> BufferId.*?detailCode := 22.*?requestBootId <> DiagnosticsBootId.*?requestBootId <> CurrentDiagnosticsBootId.*?state < LMC_RECORDER_ARMED.*?state > LMC_RECORDER_UPLOADING.*?ClosedSessionEpoch = 0.*?ClosedSessionEpoch <> OwnerSessionEpoch.*?end_if;\s*if detailCode = 0 then.*?OwnerSessionEpoch := CallerSessionEpoch.*?ClosedSessionEpoch := 0.*?\(pResponse \+ 20\)\^\$UDINT := RecordId.*?\(pResponse \+ 24\)\^\$UDINT := BufferId.*?\(pResponse \+ 28\)\^\$UDINT := OwnerSessionEpoch.*?\(pResponse \+ 32\)\^\$UINT := TO_UINT\(state\)' 'AdoptRecorder 0x7E49 no longer preserves exact-ID adoption or does not return the adopted active identity and new owner.'

$recorderProtocolCommands = [ordered]@{
    ConfigureRecorder = '0x7E40'
    StartRecorder = '0x7E41'
    TriggerRecorder = '0x7E42'
    StopRecorder = '0x7E43'
    ReadRecorderStatus = '0x7E44'
    ReadRecorderHeader = '0x7E45'
    ReadRecorderChunk = '0x7E46'
    ReleaseRecorderBuffer = '0x7E47'
    ReleaseRecorder = '0x7E48'
    AdoptRecorder = '0x7E49'
}
foreach ($recorderProtocolCommand in $recorderProtocolCommands.GetEnumerator()) {
    Assert-Match $protocol (
        'internal const ushort ' +
        [regex]::Escape($recorderProtocolCommand.Key) +
        ' = ' +
        [regex]::Escape($recorderProtocolCommand.Value) +
        ';') "C# recorder command $($recorderProtocolCommand.Key) has the wrong ID."
}

Assert-Match $protocol 'internal const ushort GetDiagnosticsCapabilities = 0x7E00;' 'C# diagnostics capability command ID is missing.'
Assert-Match $protocol 'internal const ushort GetAdminCapabilities = 0x7D00;' 'C# admin capability command ID is missing.'
Assert-Match $protocol 'internal const ushort ReadAxisParameter = 0x7D10;' 'C# axis parameter command ID is missing.'
Assert-Match $protocol 'internal const ushort ReadGroupParameters = 0x7D20;' 'C# group parameter command ID is missing.'
Assert-Match $protocol 'internal const ushort GroupMoveLinearRelative = 0x7D22;' 'C# group relative-move command ID is missing.'
Assert-Match $adminProtocol '(?s)GetCapabilities\(uint requestId\).*?CreateCommonRequest\(\s*LMC_CommandId\.GetAdminCapabilities,\s*0,\s*CommonRequestPayloadLength,\s*requestId\)' 'C# 0x7D00 request builder is incomplete.'
Assert-Match $adminProtocol '(?s)ReadAxisParameter\(.*?CreateCommonRequest\(\s*LMC_CommandId\.ReadAxisParameter,\s*axisReference,\s*ReadParameterRequestPayloadLength,\s*requestId\).*?CommonRequestPayloadLength,\s*\(ushort\)key' 'C# 0x7D10 request builder is incomplete.'
Assert-Match $adminProtocol '(?s)ReadGroupParameters\(.*?CreateCommonRequest\(\s*LMC_CommandId\.ReadGroupParameters,\s*groupReference,\s*ReadParameterRequestPayloadLength,\s*requestId\).*?CommonRequestPayloadLength,\s*\(uint\)selection' 'C# 0x7D20 request builder is incomplete.'
Assert-Match $adminProtocol 'GroupMoveLinearRelativeRequestPayloadLength = 104;' 'C# 0x7D22 request payload length is not 104 bytes.'
$adminGroupMoveRelativeFrameBlock = [regex]::Match(
    $adminProtocol,
    '(?s)internal static byte\[\] GroupMoveLinearRelative\(.*?internal static void ValidateGroupLinearRelative').Value
if ([string]::IsNullOrWhiteSpace($adminGroupMoveRelativeFrameBlock)) {
    throw 'C# 0x7D22 request builder was not found.'
}
Assert-Match $adminGroupMoveRelativeFrameBlock '(?s)CreateCommonRequest\(\s*LMC_CommandId\.GroupMoveLinearRelative,\s*groupReference,\s*GroupMoveLinearRelativeRequestPayloadLength,\s*requestId\).*?motionOffset = LMC_Frame\.HeaderSize\s*\+ CommonRequestPayloadLength.*?WriteGroupLinearVector\(\s*buffer,\s*motionOffset,\s*distance\)' 'C# 0x7D22 common envelope or 16-slot distance vector is incomplete.'
Assert-Match $adminGroupMoveRelativeFrameBlock '(?s)motionOffset \+ 64, velocity.*?motionOffset \+ 68, acceleration.*?motionOffset \+ 72, deceleration.*?motionOffset \+ 76, jerk.*?motionOffset \+ 80,\s*\(int\)options\.CoordinateSystem.*?motionOffset \+ 84,\s*\(int\)options\.TransitionMode.*?motionOffset \+ 88,\s*\(int\)options\.BufferMode.*?motionOffset \+ 92,\s*options\.Execute \? 1 : 0' 'C# 0x7D22 motion field offsets are incomplete.'
Assert-Match $diagnosticsProtocol '(?s)GetDiagnosticsCapabilities\(uint requestId\).*?CreateRequest\(\s*LMC_CommandId\.GetDiagnosticsCapabilities,\s*0,\s*CommonRequestPayloadLength\).*?WriteUInt16\(buffer, LMC_Frame\.HeaderSize, SchemaVersion\).*?WriteUInt16\(buffer, LMC_Frame\.HeaderSize \+ 2, 0\).*?WriteUInt32\(buffer, LMC_Frame\.HeaderSize \+ 4, requestId\)' 'C# diagnostics capability common request builder is incomplete.'

$axisLookupBlock = [regex]::Match(
    $registryHandlerBlock,
    '(?s)0x103C:.*?0x1042:').Value
if ([string]::IsNullOrWhiteSpace($axisLookupBlock)) {
    throw '0x103C axis lookup case was not found.'
}
if ($axisLookupBlock -match 'ObjectRegistryReady') {
    throw '0x103C axis lookup still depends on the aggregate object registry.'
}
if ([regex]::Matches($axisLookupBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9) {
    throw '0x103C axis lookup does not validate each axis client independently.'
}
if ([regex]::Matches($axisLookupBlock, '_GetObjName\(\s*pThis:=LMCAxis[1-9]\.pCmd').Count -ne 9) {
    throw '0x103C axis lookup does not refresh all nine connected object names on demand.'
}
if ([regex]::Matches($axisLookupBlock, '_memset\(dest:=#AxisObjectName[1-9]\[0\]').Count -ne 9) {
    throw '0x103C axis lookup does not clear every name buffer before discovery.'
}
Assert-Match $axisLookupBlock '(?s)objectNameLength := _GetObjName.*?objectNameLength > 0.*?objectNameLength <= 79.*?_stricmp' '0x103C axis lookup does not validate the discovered name length before a case-insensitive comparison.'
if ($axisLookupBlock -match '_strcmp') {
    throw '0x103C axis lookup still performs a case-sensitive object-name comparison.'
}

$groupLookupBlock = [regex]::Match(
    $registryHandlerBlock,
    '(?s)0x1042:.*?0x202B:').Value
if ([string]::IsNullOrWhiteSpace($groupLookupBlock)) {
    throw '0x1042 group lookup case was not found.'
}
if ($groupLookupBlock -match 'ObjectRegistryReady') {
    throw '0x1042 group lookup still depends on the aggregate object registry.'
}
Assert-Match $groupLookupBlock 'IsClientConnected\(#LMCRobot\)' '0x1042 group lookup does not validate the robot client independently.'
Assert-Match $groupLookupBlock '(?s)_memset\(dest:=#GroupObjectName\[0\].*?_GetObjName\(\s*pThis:=LMCRobot\.pCmd.*?objectNameLength > 0.*?objectNameLength <= 79.*?_stricmp' '0x1042 group lookup does not safely refresh and compare the group name case-insensitively.'
if ($groupLookupBlock -match '_strcmp') {
    throw '0x1042 group lookup still performs a case-sensitive object-name comparison.'
}

$powerCaseBlock = [regex]::Match($axisHandlerBlock, '(?s)0x2023:.*?0x2024:').Value
Assert-Match $powerCaseBlock '(?s)\(Payload = 8\).*?\(RequestBuf\[8\]\$UDINT = 1\).*?\(RequestBuf\[12\] = 0\).*?\(RequestBuf\[12\] = 1\).*?\(RequestBuf\[13\] = 1\).*?\(RequestBuf\[14\] = 0\).*?\(RequestBuf\[15\] = 1\)' '0x2023 exact DINT payload validation is missing.'
Assert-Match $powerCaseBlock '(?s)if RequestBuf\[12\] = 1 then.*?PowerOn\(\);.*?else.*?PowerOff\(\);' '0x2023 PowerOn/PowerOff dispatch is missing.'

$powerOnBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::PowerOn.*?END_FUNCTION').Value
$powerOffBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::PowerOff.*?END_FUNCTION').Value
if ([regex]::Matches($powerOnBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9 -or
    [regex]::Matches($powerOnBlock, '\bLMCAxis[1-9]\.PowerOn\s*\(').Count -ne 9) {
    throw 'PowerOn does not validate and dispatch all nine LASAL axis clients.'
}
if ([regex]::Matches($powerOffBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9 -or
    [regex]::Matches($powerOffBlock, '\bLMCAxis[1-9]\.PowerOff\s*\(').Count -ne 9) {
    throw 'PowerOff does not validate and dispatch all nine LASAL axis clients.'
}

$resetCaseBlock = [regex]::Match($axisHandlerBlock, '(?s)0x2024:.*?0x2022:').Value
Assert-Match $resetCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef >= 1\).*?\(AxisRef <= 9\).*?AxisReset\(\);' '0x2024 exact reset validation/dispatch is missing.'
$axisResetBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::AxisReset.*?END_FUNCTION').Value
if ([regex]::Matches($axisResetBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9 -or
    [regex]::Matches($axisResetBlock, '\bLMCAxis[1-9]\.QuitError\s*\(').Count -ne 9) {
    throw 'AxisReset does not validate and dispatch all nine LASAL axis clients.'
}

$stopCaseBlock = [regex]::Match($axisHandlerBlock, '(?s)0x2022:.*?0x2028:').Value
Assert-Match $stopCaseBlock '(?s)if Payload = 16 then.*?\(bufMode = 1\).*?\(Exec = 1\).*?else\s*AxisRef := 0;.*?MoveStop\(\);' '0x2022 exact payload and semantic validation is missing.'
Assert-Match $stopCaseBlock '_StdLib\.MemCpy\(dest:=#jer,\s*source:=#RequestBuf\[12\],\s*size:=4\);' '0x2022 does not read Jerk from request offset 12.'
$moveStopBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::MoveStop.*?END_FUNCTION').Value
if ([regex]::Matches($moveStopBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9 -or
    [regex]::Matches($moveStopBlock, '\bLMCAxis[1-9]\.StopMove\s*\(').Count -ne 9) {
    throw 'MoveStop does not validate and dispatch all nine LASAL axis clients.'
}
if ([regex]::Matches($moveStopBlock, 'Jerk:=jer').Count -ne 9) {
    throw 'MoveStop does not forward the received Jerk to all nine LASAL axis clients.'
}

$readStatusCaseBlock = [regex]::Match(
    $axisHandlerBlock,
    '(?s)0x2028:.*?0x202E:').Value
if ([string]::IsNullOrWhiteSpace($readStatusCaseBlock)) {
    throw '0x2028 MsgPaser case was not found.'
}
Assert-Match $readStatusCaseBlock '(?s)\(Payload = 8\).*?\(AxisRef >= 1\).*?\(AxisRef <= 9\).*?\(PayloadReference = AxisRef\).*?\(Exec = 1\)' '0x2028 payload/reference/execute validation is missing.'
$readAxisStatusCalls = [regex]::Matches($readStatusCaseBlock, '\bLMCAxis[1-9]\.ReadAxisStatus\s*\(').Count
$readAxisErrorCalls = [regex]::Matches($readStatusCaseBlock, '\bLMCAxis[1-9]\.ReadAxisError\s*\(').Count
if ($readAxisStatusCalls -ne 9 -or $readAxisErrorCalls -ne 9) {
    throw "0x2028 CyWork client calls are incomplete: status=$readAxisStatusCalls error=$readAxisErrorCalls."
}
Assert-Match $readStatusCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*12;.*?Sendbuf\[8\]\$UDINT\s*:=\s*AxisStatusValue\$UDINT;.*?Sendbuf\[12\]\$UINT\s*:=\s*AxisCommandStatus;.*?Sendbuf\[14\]\$INT\s*:=\s*AxisCommandErrorId;.*?Sendbuf\[16\]\$UINT\s*:=\s*AxisErrorValue\$UINT;.*?Sendbuf\[18\]\$UINT\s*:=\s*0;.*?udSize:=20' '0x2028 20-byte typed response framing is missing.'

$readPositionCaseBlock = [regex]::Match(
    $axisHandlerBlock,
    '(?s)0x202E:.*?0x209F:').Value
if ([string]::IsNullOrWhiteSpace($readPositionCaseBlock)) {
    throw '0x202E MsgPaser case was not found.'
}
Assert-Match $readPositionCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 0\).*?\(AxisRef >= 1\).*?\(AxisRef <= 9\)' '0x202E payload/reference validation is missing.'
$readPositionCalls = [regex]::Matches($readPositionCaseBlock, '\bLMCAxis[1-9]\.ReadPosition\s*\(').Count
if ($readPositionCalls -ne 9) {
    throw "0x202E CyWork client calls=$readPositionCalls, expected 9."
}
Assert-Match $readPositionCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*8;.*?Sendbuf\[8\]\$DINT\s*:=\s*ReadPos;.*?Sendbuf\[12\]\$UINT\s*:=\s*AxisCommandStatus;.*?Sendbuf\[14\]\$INT\s*:=\s*AxisCommandErrorId;.*?udSize:=16' '0x202E 16-byte typed response framing is missing.'

$moveShortestCaseBlock = [regex]::Match($axisHandlerBlock, '(?s)0x209F:.*?0x20A0:').Value
$moveRelativeCaseBlock = [regex]::Match($axisHandlerBlock, '(?s)0x20A0:.*?0x20A2:').Value
$moveVelocityCaseBlock = [regex]::Match($axisHandlerBlock, '(?s)0x20A2:.*?end_case;').Value
foreach ($entry in @(
    @{ Name = '0x209F'; Block = $moveShortestCaseBlock },
    @{ Name = '0x20A0'; Block = $moveRelativeCaseBlock })) {
    Assert-Match $entry.Block '(?s)if Payload = 32 then.*?\(dir = 2\).*?\(bufMode = 1\).*?\(Exec = 1\).*?else\s*AxisRef := 0;.*?MoveAbs\(\);' "$($entry.Name) exact payload and shortest-only validation is missing."
    Assert-Match $entry.Block '_StdLib\.MemCpy\(dest:=#jer,\s*source:=#RequestBuf\[24\],\s*size:=4\);' "$($entry.Name) does not read Jerk from request offset 24."
}
Assert-Match $moveVelocityCaseBlock '(?s)if Payload = 24 then.*?\(dec = 0\).*?\(Exec = 1\).*?\(dir = 1\).*?\(velo >= 0\).*?\(dir = 3\).*?\(velo <= 0\).*?else\s*AxisRef := 0;.*?MoveAbs\(\);' '0x20A2 exact payload, direction, and execute validation is missing.'
Assert-Match $moveVelocityCaseBlock '_StdLib\.MemCpy\(dest:=#jer,\s*source:=#RequestBuf\[20\],\s*size:=4\);' '0x20A2 does not read Jerk from request offset 20.'

$moveAbsBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::MoveAbs.*?END_FUNCTION').Value
if ([regex]::Matches($moveAbsBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9 -or
    [regex]::Matches($moveAbsBlock, '\bLMCAxis[1-9]\.MoveShortestWay\s*\(').Count -ne 9 -or
    [regex]::Matches($moveAbsBlock, '\bLMCAxis[1-9]\.MoveRelative\s*\(').Count -ne 9 -or
    [regex]::Matches($moveAbsBlock, '\bLMCAxis[1-9]\.MoveEndless\s*\(').Count -ne 9) {
    throw 'MoveAbs does not dispatch all three approved motion commands to all nine LASAL axis clients.'
}
if ([regex]::Matches($moveAbsBlock, 'Jerk:=jer').Count -ne 27) {
    throw 'MoveAbs does not forward the received Jerk through all 27 axis motion dispatch paths.'
}

if ($ControlServiceCheckpoint -ne 'Phase3GroupRouted') {
$groupMembersCaseBlock = [regex]::Match(
    $groupHandlerBlock,
    '(?s)0x20D2:.*?0x2047:').Value
if ([string]::IsNullOrWhiteSpace($groupMembersCaseBlock)) {
    throw '0x20D2 group-members case was not found.'
}
Assert-Match $groupMembersCaseBlock 'ObjectRegistryReady\s*:=\s*FALSE' '0x20D2 does not invalidate the object registry before refreshing it.'
if ([regex]::Matches($groupMembersCaseBlock, 'IsClientConnected\(#(?:LMCAxis[1-9]|LMCRobot)\)').Count -ne 10) {
    throw '0x20D2 does not validate all ten current LASAL client connections.'
}
if ([regex]::Matches($groupMembersCaseBlock, '_GetObjName\(\s*pThis:=(?:LMCAxis[1-9]|LMCRobot)\.pCmd').Count -ne 10) {
    throw '0x20D2 does not refresh all ten object names on demand.'
}
if ([regex]::Matches($groupMembersCaseBlock, '_memset\(dest:=#(?:AxisObjectName[1-9]|GroupObjectName)\[0\]').Count -ne 10) {
    throw '0x20D2 does not clear all ten object-name buffers before discovery.'
}
Assert-Match $groupMembersCaseBlock '(?s)objectNameLength = 0.*?objectNameLength > 79.*?ObjectRegistryReady := FALSE' '0x20D2 does not reject empty or overlength discovered names.'
foreach ($entry in @(
    @{ Offset = 16; Value = 5 },
    @{ Offset = 18; Value = 6 },
    @{ Offset = 20; Value = 7 },
    @{ Offset = 22; Value = 8 },
    @{ Offset = 24; Value = 9 })) {
    Assert-Match $groupMembersCaseBlock (
        'Sendbuf\[' + $entry.Offset + '\]\$UINT\s*:=\s*' +
        $entry.Value + ';') (
        "0x20D2 axis $($entry.Value) reference slot is missing.")
}
foreach ($entry in @(
    @{ Offset = 48; Value = 4 },
    @{ Offset = 50; Value = 5 },
    @{ Offset = 52; Value = 6 },
    @{ Offset = 54; Value = 7 },
    @{ Offset = 56; Value = 8 })) {
    Assert-Match $groupMembersCaseBlock (
        'Sendbuf\[' + $entry.Offset + '\]\$UINT\s*:=\s*' +
        $entry.Value + ';') (
        "0x20D2 axis $($entry.Value + 1) device-ID slot is missing.")
}
foreach ($entry in @(
    @{ Offset = 396; Axis = 5 },
    @{ Offset = 476; Axis = 6 },
    @{ Offset = 556; Axis = 7 },
    @{ Offset = 636; Axis = 8 },
    @{ Offset = 716; Axis = 9 })) {
    Assert-Match $groupMembersCaseBlock (
        '(?s)pThis:=LMCAxis' + $entry.Axis + '\.pCmd,.*?' +
        'MemCpy\(dest:=#Sendbuf\[' + $entry.Offset +
        '\],\s*source:=#AxisObjectName1' +
        '\[0\],\s*size:=80\)') (
        "0x20D2 axis $($entry.Axis) shared-buffer name slot is missing.")
}
Assert-Match $groupMembersCaseBlock 'Sendbuf\[1356\]\s*:=\s*9;' '0x20D2 AxisCount is not 9.'

$groupEnableCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x2047:.*?0x2048:').Value
$groupDisableCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x2048:.*?0x2049:').Value
Assert-Match $groupEnableCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?IsClientConnected\(#LMCAxis1\).*?IsClientConnected\(#LMCAxis2\).*?IsClientConnected\(#LMCAxis3\).*?IsClientConnected\(#LMCAxis4\).*?GroupReadErrorId := -6;.*?GroupKinematicReady = TRUE.*?powerIsOn <> 0.*?LMCRobot\.LockProfile\(.*?Axis1:=1.*?Axis4:=1.*?Axis5:=0.*?Axis9:=0.*?GroupReadRetCode = _LMCPROF_NoError then.*?GroupReadErrorId := 0;.*?elsif GroupReadRetCode\$UDINT <= 32767 then.*?GroupReadErrorId := GroupReadRetCode\$DINT;.*?udSize:=16' '0x2047 preconditions, four-axis profile-lock dispatch, acceptance mapping, native error preservation, or ACK is missing.'
if ($groupEnableCaseBlock -match 'ReadProfileParameter|_LMCPROF_LockState') {
    throw '0x2047 still treats the same-CyWork LockState read as command completion.'
}
Assert-Match $groupDisableCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?ProfileInPosition\(.*?_LMCPROF_ProfileFinished.*?GroupReadInPosition <> 0.*?LMCRobot\.UnlockProfile\(\).*?ReadProfileParameter\(.*?_LMCPROF_LockState.*?udSize:=16' '0x2048 group profile-unlock standstill validation/dispatch/ACK is missing.'

$groupPowerOnCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x204A:.*?0x204B:').Value
$groupPowerOffCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x204B:.*?0x2085:').Value
Assert-Match $groupPowerOnCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?LMCRobot\.RobotOn\(Mode:=_ACTIVE\).*?udSize:=16' '0x204A group-power-on validation/RobotOn/ACK is missing.'
if ($groupPowerOnCaseBlock -match 'GroupKinematicReady\s*=\s*TRUE') {
    throw '0x204A group-power-on is incorrectly gated by kinematic configuration.'
}
Assert-Match $groupPowerOffCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?LMCRobot\.RobotOff\(\).*?udSize:=16' '0x204B group-power-off validation/RobotOff/ACK is missing.'
foreach ($entry in @(
    @{ Name = '0x2047'; Block = $groupEnableCaseBlock },
    @{ Name = '0x2048'; Block = $groupDisableCaseBlock },
    @{ Name = '0x204A'; Block = $groupPowerOnCaseBlock },
    @{ Name = '0x204B'; Block = $groupPowerOffCaseBlock })) {
    Assert-Match $entry.Block '(?s)\(GroupReadErrorId >= -32768\).*?\(GroupReadErrorId <= 32767\).*?Sendbuf\[14\]\$INT\s*:=\s*GroupReadErrorId\$INT;.*?else.*?Sendbuf\[14\]\$INT\s*:=\s*-6' "$($entry.Name) does not preserve signed 16-bit LASAL/disconnected errors before overflow mapping."
    if ($entry.Block -match 'GroupReadErrorId\$UDINT\s+and\s+0xFFFF0000') {
        throw "$($entry.Name) still sign-extends negative DINT errors into overflow error -6."
    }
}

$groupStatusCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x2045:.*?0x2051:').Value
Assert-Match $groupStatusCaseBlock '(?s)\(Payload = 8\).*?\(AxisRef = 0x0100\).*?\(PayloadReference = AxisRef\).*?\(Exec = 1\).*?IsClientConnected\(#LMCRobot\).*?GroupReadStatus\(\);' '0x2045 group-status validation/dispatch is missing.'
$groupReadStatusBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::GroupReadStatus.*?END_FUNCTION').Value
Assert-Match $groupReadStatusBlock '(?s)LMCRobot\.RobotIsOn\(\).*?powerIsOn <> 0.*?GroupReadState := GroupReadState or 0x00040000' 'GroupReadStatus project-local power-ready mapping is missing.'
Assert-Match $groupReadStatusBlock '(?s)ReadProfileParameter\(.*?_LMCPROF_LockState.*?powerIsOn <> 0.*?profileLocked = TRUE.*?GroupReadInPosition <> 0.*?GroupReadState := GroupReadState or 0x00020000.*?profileLocked = FALSE.*?GroupReadState := GroupReadState or 0x00010000' 'GroupReadStatus locked-standby/unlocked-disabled mapping is missing.'
Assert-Match $groupReadStatusBlock '(?s)LMCRobot\.ReadRobotParameter\(ParNo:=_ROBOT_STATE, Mode:=0\).*?robotState = _ROBOT_ERROR\$DINT.*?LMCRobot\.ReadProfileError\(\).*?GroupReadErrorId := profileErrorInfo\.ErrorNo\$DINT' 'GroupReadStatus robot/profile error propagation or enum-to-DINT typing is missing.'
Assert-Match $groupReadStatusBlock '(?s)if GroupReadErrorId = 0 then.*?GroupReadErrorId := -6;.*?robotState < _ROBOT_PASSIVE\$DINT.*?robotState > _ROBOT_MODE_CHANGE\$DINT.*?GroupReadErrorId := -6' 'GroupReadStatus false-success guards are missing.'
Assert-Match $groupReadStatusBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*0x000C;.*?Sendbuf\[8\]\$UDINT\s*:=\s*GroupReadState;.*?Sendbuf\[16\]\$UINT\s*:=\s*GroupReadErrorId\$UINT;.*?SendData' '0x2045 20-byte typed response framing is missing.'
if ($groupReadStatusBlock -match 'GroupMoveRetCode') {
    throw 'GroupReadStatus still reports stale GroupMoveRetCode state.'
}

$groupResetCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x2049:.*?0x204A:').Value
Assert-Match $groupResetCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?LMCRobot\.AxQuitError\(AxisNo:=0\).*?AxisCommandStatus := 0;.*?AxisCommandErrorId := 0;.*?udSize:=16' '0x2049 axis-error reset validation/AxQuitError dispatch/ACK is missing.'

$groupStopCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x2085:.*?0x20A4:').Value
Assert-Match $groupStopCaseBlock '(?s)if Payload = 16 then.*?RequestBuf\[8\].*?RequestBuf\[12\].*?\(bufMode = 1\).*?\(GroupExecute = 1\).*?\(GroupDecel >= 0\).*?\(GroupJerk >= 0\).*?\(\(GroupJerk = 0\) \| \(GroupDecel > 0\)\).*?GroupStopCommandNo\s*:=\s*LMCRobot\.StopMove\(\s*Mode:=3, Decel:=GroupDecel, Jerk:=GroupJerk\).*?GroupReadErrorId\s*:=\s*0;.*?udSize:=16' '0x2085 group stop validation/StopMove dispatch/ACK is missing.'
$groupStopCommandNoUseCount = [regex]::Matches(
    $groupStopCaseBlock,
    '\bGroupStopCommandNo\b').Count
if ($groupStopCommandNoUseCount -ne 2) {
    throw '0x2085 incorrectly treats StopMove StopCmdNo as an error or acceptance code.'
}

$groupMoveCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x20A4:.*?0x2045:').Value
Assert-Match $groupMoveCaseBlock '(?s)\(Payload = 96\).*?\(AxisRef = 0x0100\).*?source:=#RequestBuf\[72\].*?source:=#RequestBuf\[76\].*?source:=#RequestBuf\[80\].*?source:=#RequestBuf\[84\].*?source:=#RequestBuf\[88\].*?source:=#RequestBuf\[92\].*?source:=#RequestBuf\[96\].*?source:=#RequestBuf\[100\]' '0x20A4 DINT field offsets are incomplete.'
Assert-Match $groupMoveCaseBlock '(?s)for kinIndex := 4 to 15 do.*?GroupCommandInputValid := FALSE.*?end_for' '0x20A4 does not reject nonzero positions outside the four-axis topology.'
Assert-Match $groupMoveCaseBlock '(?s)\(GroupCoordSystem = 0\).*?\(GroupTransitionModeInput = 0\).*?\(GroupTransitionModeInput = 2\).*?\(bufMode = 1\).*?\(bufMode = 2\).*?MoveLinearAbsEx\(\);' '0x20A4 approved coordinate/transition/buffer validation is missing.'
$groupMoveBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::MoveLinearAbsEx.*?END_FUNCTION').Value
Assert-Match $groupMoveBlock '(?s)GroupCommandInputValid = TRUE.*?IsClientConnected\(#LMCRobot\).*?LMCRobot\.RobotIsOn\(\).*?ReadProfileParameter\(.*?_LMCPROF_LockState.*?GroupKinematicReady = TRUE.*?powerIsOn <> 0.*?profileLocked = TRUE.*?LMCRobot\.MoveLinearCoord\(.*?CmdConfig:=GroupCommandConfig.*?CoordSystem:=0.*?Jerk:=GroupJerk.*?udSize:=16' 'MoveLinearAbsEx does not gate and dispatch the validated configured/powered/locked command.'
Assert-Match $groupMoveBlock '(?s)GroupMoveRetCode = _LMCPROF_NoError then.*?GroupReadErrorId := 0;.*?if GroupReadErrorId = 0 then.*?Sendbuf\[12\]\$UINT := 0;.*?else.*?Sendbuf\[12\]\$UINT := 1;' 'MoveLinearAbsEx does not gate success on the MotionLib return code.'

$groupPositionCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x2051:.*?0x20E7:').Value
Assert-Match $groupPositionCaseBlock '(?s)GroupCoordSystem := -1;.*?GroupReadErrorId := -3;.*?\(Payload = 8\).*?\(AxisRef = 0x0100\).*?\(GroupExecute = 1\).*?if \(GroupCoordSystem = 0\) \| \(GroupCoordSystem = 1\) then.*?LMCRobot\.GetRobotPosition\(.*?Mode:=_ACTPOS_APPUNITS.*?CoordSystem:=0.*?pPositions:=#GroupReadPos.*?elsif \(GroupCoordSystem = 2\) \| \(GroupCoordSystem = 3\) then.*?GroupReadErrorId := -7.*?end_if;.*?end_if;' '0x2051 None/ACS member-slot mapping, MCS/PCS rejection, or unknown-enum -3 default is missing.'
Assert-Match $groupPositionCaseBlock '(?s)GroupReadRetCode = _LMCPROF_NoError then.*?GroupReadErrorId := 0;.*?if GroupReadErrorId = 0 then.*?Sendbuf\[2\]\$UINT\s*:=\s*68;.*?else.*?Sendbuf\[2\]\$UINT\s*:=\s*4;' '0x2051 does not gate the typed success payload on the MotionLib return code.'
Assert-Match $groupPositionCaseBlock '(?s)_memset\(dest:=#Sendbuf, usByte:=0, cntr:=sizeof\(Sendbuf\)\);.*?Sendbuf\[2\]\$UINT\s*:=\s*68;.*?MemCpy\(dest:=#Sendbuf\[8\], source:=#GroupReadPos, size:=36\).*?Sendbuf\[72\]\$UINT\s*:=\s*0x4000;.*?udSize:=76' '0x2051 68-byte DINT position response or zero-tail initialization is missing.'

$kinCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x20E7:.*?end_case;').Value
Assert-Match $kinCaseBlock '(?s)kinValid := \(Payload = 1320\).*?for kinIndex := 0 to 3 do.*?0x3FF00000.*?RequestBuf\[648\]\$DINT <> 4.*?RequestBuf\[1316\]\$DINT <> 2.*?RequestBuf\[1320\]\$DINT <> 1' '0x20E7 identity-shift Cartesian4 payload validation is missing.'
Assert-Match $kinCaseBlock '(?s)IsClientConnected\(#LMCRobot\).*?IsClientConnected\(#LMCAxis1\).*?IsClientConnected\(#LMCAxis2\).*?IsClientConnected\(#LMCAxis3\).*?IsClientConnected\(#LMCAxis4\).*?GroupKinematicReady := TRUE;.*?GroupReadErrorId := 0;' '0x20E7 static four-axis mapping registration is missing.'
if ($kinCaseBlock -match 'LockProfile|UnlockProfile|RobotOn|RobotOff') {
    throw '0x20E7 mapping validation still changes profile-lock or group-power state.'
}
Assert-Match $kinCaseBlock '(?s)if GroupReadErrorId = 0 then.*?Sendbuf\[8\]\$UINT := 0;.*?else.*?Sendbuf\[8\]\$UINT := 1;' '0x20E7 does not gate acknowledgement success on mapping validation.'
Assert-Match $kinCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*4;.*?Sendbuf\[8\]\$UINT.*?Sendbuf\[10\]\$INT.*?udSize:=12' '0x20E7 short acknowledgement framing is missing.'
}

if ($ControlServiceCheckpoint -ne 'Phase2Skeleton') {
    $serviceGroupHandlerBlock =
        $controlServicePrivateBlocks['HandleGroupCommands']
    $serviceAdminHandlerBlock =
        $controlServicePrivateBlocks['HandleAdminCommands']
    $serviceMoveLinearBlock =
        $controlServicePrivateBlocks['MoveLinearAbsEx']
    $serviceGroupReadStatusBlock =
        $controlServicePrivateBlocks['GroupReadStatus']

    $serviceGroupMembersCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x20D2:.*?0x2047:').Value
    $serviceGroupEnableCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x2047:.*?0x2048:').Value
    $serviceGroupDisableCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x2048:.*?0x2049:').Value
    $serviceGroupResetCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x2049:.*?0x204A:').Value
    $serviceGroupPowerOnCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x204A:.*?0x204B:').Value
    $serviceGroupPowerOffCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x204B:.*?0x2085:').Value
    $serviceGroupStopCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x2085:.*?0x20A4:').Value
    $serviceGroupMoveCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x20A4:.*?0x2045:').Value
    $serviceGroupStatusCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x2045:.*?0x2051:').Value
    $serviceGroupPositionCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x2051:.*?0x20E7:').Value
    $serviceKinematicCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x20E7:.*?end_case;').Value
    $serviceAdminGroupParametersCaseBlock = [regex]::Match(
        $serviceAdminHandlerBlock,
        '(?s)0x7D20:.*?0x7D22:').Value
    $serviceAdminRelativeMoveCaseBlock = [regex]::Match(
        $serviceAdminHandlerBlock,
        '(?s)0x7D22:.*(?=\s+else\s+ResponseSize\s*:=\s*-1\s*;\s*end_case;)').Value

    $serviceSemanticBlocks = [ordered]@{
        '0x20D2' = $serviceGroupMembersCaseBlock
        '0x2047' = $serviceGroupEnableCaseBlock
        '0x2048' = $serviceGroupDisableCaseBlock
        '0x2049' = $serviceGroupResetCaseBlock
        '0x204A' = $serviceGroupPowerOnCaseBlock
        '0x204B' = $serviceGroupPowerOffCaseBlock
        '0x2085' = $serviceGroupStopCaseBlock
        '0x20A4' = $serviceGroupMoveCaseBlock
        '0x2045' = $serviceGroupStatusCaseBlock
        '0x2051' = $serviceGroupPositionCaseBlock
        '0x20E7' = $serviceKinematicCaseBlock
        '0x7D20' = $serviceAdminGroupParametersCaseBlock
        '0x7D22' = $serviceAdminRelativeMoveCaseBlock
    }
    foreach ($semanticEntry in $serviceSemanticBlocks.GetEnumerator()) {
        if ([string]::IsNullOrWhiteSpace($semanticEntry.Value)) {
            throw (
                'LMCControlCommandService semantic block ' +
                "$($semanticEntry.Key) was not found.")
        }
    }

    $fourAxisServiceClients = @(
        'LMCRobot',
        'LMCAxis1',
        'LMCAxis2',
        'LMCAxis3',
        'LMCAxis4')
    foreach ($clientGate in @(
            @{ Owner = 'Service 0x2047'; Block = $serviceGroupEnableCaseBlock },
            @{ Owner = 'Service 0x204A'; Block = $serviceGroupPowerOnCaseBlock },
            @{ Owner = 'Service MoveLinearAbsEx'; Block = $serviceMoveLinearBlock },
            @{ Owner = 'Service 0x20E7'; Block = $serviceKinematicCaseBlock },
            @{ Owner = 'Service 0x7D22'; Block = $serviceAdminRelativeMoveCaseBlock })) {
        Assert-ExactLasalConnectedClientSet `
            -Text $clientGate.Block `
            -Owner $clientGate.Owner `
            -ExpectedClients $fourAxisServiceClients
        Assert-Match $clientGate.Block (
            '(?s)if\s+\(IsClientConnected\(#LMCRobot\)\s*=\s*1\)\s*&\s*' +
            '\(IsClientConnected\(#LMCAxis1\)\s*=\s*1\)\s*&\s*' +
            '\(IsClientConnected\(#LMCAxis2\)\s*=\s*1\)\s*&\s*' +
            '\(IsClientConnected\(#LMCAxis3\)\s*=\s*1\)\s*&\s*' +
            '\(IsClientConnected\(#LMCAxis4\)\s*=\s*1\)\s+then') (
            "$($clientGate.Owner) must conjunct all five exact client gates.")
    }

    Assert-Match $serviceGroupEnableCaseBlock (
        'if\s+\(GroupKinematicReady\s*=\s*TRUE\)\s*&\s*' +
        '\(powerIsOn\s*<>\s*0\)\s+then') (
        'Service 0x2047 must conjunct kinematic readiness and group power.')
    Assert-Match $serviceMoveLinearBlock (
        '(?s)if\s+\(GroupKinematicReady\s*=\s*TRUE\)\s*&\s*' +
        '\(powerIsOn\s*<>\s*0\)\s*&\s*' +
        '\(profileLocked\s*=\s*TRUE\)\s+then') (
        'Service MoveLinearAbsEx must conjunct kinematic, power, and lock readiness.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)if\s+\(GroupKinematicReady\s*=\s*TRUE\)\s*&\s*' +
        '\(powerIsOn\s*<>\s*0\)\s*&\s*' +
        '\(profileLockState\s*<>\s*0\)\s+then') (
        'Service 0x7D22 must conjunct kinematic, power, and lock readiness.')

    $serviceFrameContracts = @(
        @{ Owner = '0x20D2'; Block = $serviceGroupMembersCaseBlock;
            Sizes = @('12', '1358'); Outer = @('0', '1') },
        @{ Owner = '0x2047'; Block = $serviceGroupEnableCaseBlock;
            Sizes = @('12', '16'); Outer = @('0', '1') },
        @{ Owner = '0x2048'; Block = $serviceGroupDisableCaseBlock;
            Sizes = @('12', '16'); Outer = @('0', '1') },
        @{ Owner = '0x2049'; Block = $serviceGroupResetCaseBlock;
            Sizes = @('16'); Outer = @('0') },
        @{ Owner = '0x204A'; Block = $serviceGroupPowerOnCaseBlock;
            Sizes = @('12', '16'); Outer = @('0', '1') },
        @{ Owner = '0x204B'; Block = $serviceGroupPowerOffCaseBlock;
            Sizes = @('12', '16'); Outer = @('0', '1') },
        @{ Owner = '0x2085'; Block = $serviceGroupStopCaseBlock;
            Sizes = @('16'); Outer = @('0') },
        @{ Owner = '0x2045'; Block = $serviceGroupStatusCaseBlock;
            Sizes = @('12'); Outer = @('1') },
        @{ Owner = '0x2051'; Block = $serviceGroupPositionCaseBlock;
            Sizes = @('12', '76'); Outer = @('0') },
        @{ Owner = '0x20E7'; Block = $serviceKinematicCaseBlock;
            Sizes = @('12'); Outer = @('0') },
        @{ Owner = '0x7D20'; Block = $serviceAdminGroupParametersCaseBlock;
            Sizes = @('24', '40'); Outer = @('0') },
        @{ Owner = '0x7D22'; Block = $serviceAdminRelativeMoveCaseBlock;
            Sizes = @('24'); Outer = @('0') },
        @{ Owner = 'MoveLinearAbsEx'; Block = $serviceMoveLinearBlock;
            Sizes = @('16'); Outer = @('0') },
        @{ Owner = 'GroupReadStatus'; Block = $serviceGroupReadStatusBlock;
            Sizes = @('20'); Outer = @('0') })
    foreach ($frameContract in $serviceFrameContracts) {
        Assert-ExactRegexValueSet `
            -Text $frameContract.Block `
            -Pattern 'ResponseSize\s*:=\s*(?<Value>[1-9][0-9]*)\s*;' `
            -Owner "LMCControlCommandService $($frameContract.Owner) response sizes" `
            -ExpectedValues $frameContract.Sizes
        Assert-ExactRegexValueSet `
            -Text $frameContract.Block `
            -Pattern 'pResponseFrame\^\$UINT\s*:=\s*(?<Value>[0-9]+)\s*;' `
            -Owner "LMCControlCommandService $($frameContract.Owner) outer statuses" `
            -ExpectedValues $frameContract.Outer
    }

    Assert-Match $serviceGroupMembersCaseBlock (
        '(?s)objectRegistryReady\s*:=\s*FALSE.*?' +
        'if\s+RequestFrameSize\s*=\s*9\s+then\s*' +
        'objectRegistryReady\s*:=\s*' +
        '\(\(pRequestFrame\s*\+\s*8\)\^\$USINT\s*=\s*1\).*?' +
        'Reference\s*=\s*0x0100.*?;\s*end_if;\s*' +
        'if\s+objectRegistryReady\s*=\s*TRUE\s+then.*?' +
        'ResponseCapacity\s*<\s*1358') (
        'Service 0x20D2 exact request envelope or response capacity is missing.')
    Assert-ExactLasalConnectedClientSet `
        -Text $serviceGroupMembersCaseBlock `
        -Owner 'Service 0x20D2 registry gate' `
        -ExpectedClients @(
            'LMCRobot',
            'LMCAxis1',
            'LMCAxis2',
            'LMCAxis3',
            'LMCAxis4',
            'LMCAxis5',
            'LMCAxis6',
            'LMCAxis7',
            'LMCAxis8',
            'LMCAxis9')
    if ([regex]::Matches(
            $serviceGroupMembersCaseBlock,
            '_GetObjName\(\s*pThis:=(?:LMCAxis[1-9]|LMCRobot)\.pCmd').Count -ne 10) {
        throw 'Service 0x20D2 must refresh exactly nine axis names and one robot name.'
    }
    if ([regex]::Matches(
            $serviceGroupMembersCaseBlock,
            '_memset\(dest:=#objectName\[0\]').Count -ne 10) {
        throw 'Service 0x20D2 must clear its shared object-name scratch before every lookup.'
    }
    if ([regex]::Matches(
            $serviceGroupMembersCaseBlock,
            '(?s)_memcpy\(ptr1:=pResponseFrame\s*\+\s*\d+,\s*' +
            'ptr2:=#objectName\[0\],\s*cntr:=80\)').Count -ne 9) {
        throw 'Service 0x20D2 must copy exactly the nine axis names into the wire response.'
    }
    Assert-Match $serviceGroupMembersCaseBlock (
        '(?s)objectNameLength\s*=\s*0.*?objectNameLength\s*>\s*79.*?' +
        'objectRegistryReady\s*:=\s*FALSE') (
        'Service 0x20D2 empty/overlength object-name rejection is missing.')
    $serviceRobotNameTail = [regex]::Match(
        $serviceGroupMembersCaseBlock,
        '(?s)_GetObjName\(\s*pThis:=LMCRobot\.pCmd.*?(?=\s*end_if;\s*\r?\n\s*if objectRegistryReady)').Value
    if ([string]::IsNullOrWhiteSpace($serviceRobotNameTail) -or
        $serviceRobotNameTail -match '_memcpy\(') {
        throw ('Service 0x20D2 must validate the robot object name without ' +
            'publishing it as a member-axis name.')
    }
    foreach ($entry in @(
            @{ Axis = 1; Offset = 76 },
            @{ Axis = 2; Offset = 156 },
            @{ Axis = 3; Offset = 236 },
            @{ Axis = 4; Offset = 316 },
            @{ Axis = 5; Offset = 396 },
            @{ Axis = 6; Offset = 476 },
            @{ Axis = 7; Offset = 556 },
            @{ Axis = 8; Offset = 636 },
            @{ Axis = 9; Offset = 716 })) {
        Assert-Match $serviceGroupMembersCaseBlock (
            '(?s)pThis:=LMCAxis' + $entry.Axis + '\.pCmd.*?' +
            '_memcpy\(ptr1:=pResponseFrame\s*\+\s*' + $entry.Offset +
            ',\s*ptr2:=#objectName\[0\],\s*cntr:=80\)') (
            "Service 0x20D2 axis $($entry.Axis) name slot is missing.")
    }
    foreach ($entry in @(
            @{ Offset = 8; Value = 1 },
            @{ Offset = 10; Value = 2 },
            @{ Offset = 12; Value = 3 },
            @{ Offset = 14; Value = 4 },
            @{ Offset = 16; Value = 5 },
            @{ Offset = 18; Value = 6 },
            @{ Offset = 20; Value = 7 },
            @{ Offset = 22; Value = 8 },
            @{ Offset = 24; Value = 9 },
            @{ Offset = 40; Value = 0 },
            @{ Offset = 42; Value = 1 },
            @{ Offset = 44; Value = 2 },
            @{ Offset = 46; Value = 3 },
            @{ Offset = 48; Value = 4 },
            @{ Offset = 50; Value = 5 },
            @{ Offset = 52; Value = 6 },
            @{ Offset = 54; Value = 7 },
            @{ Offset = 56; Value = 8 })) {
        Assert-Match $serviceGroupMembersCaseBlock (
            '\(pResponseFrame\s*\+\s*' + $entry.Offset +
            '\)\^\$UINT\s*:=\s*' + $entry.Value + '\s*;') (
            "Service 0x20D2 slot $($entry.Offset) value is missing.")
    }
    Assert-Match $serviceGroupMembersCaseBlock (
        '(?s)pResponseFrame\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*1350.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*1356\)\^\$USINT\s*:=\s*9') (
        'Service 0x20D2 opaque outer reference, payload length, or AxisCount is missing.')
    if ($serviceGroupMembersCaseBlock -match
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=.*?Reference') {
        throw 'Service 0x20D2 must keep the outer reference opaque zero.'
    }

    foreach ($singleByteCommand in @(
            @{ Name = '0x2047'; Block = $serviceGroupEnableCaseBlock },
            @{ Name = '0x2048'; Block = $serviceGroupDisableCaseBlock },
            @{ Name = '0x2049'; Block = $serviceGroupResetCaseBlock },
            @{ Name = '0x204A'; Block = $serviceGroupPowerOnCaseBlock },
            @{ Name = '0x204B'; Block = $serviceGroupPowerOffCaseBlock })) {
        Assert-Match $singleByteCommand.Block (
            '(?s)groupCommandInputValid\s*:=\s*FALSE.*?' +
            'if\s+RequestFrameSize\s*=\s*9\s+then\s*' +
            'groupCommandInputValid\s*:=\s*' +
            '\(\(pRequestFrame\s*\+\s*8\)\^\$USINT\s*=\s*1\).*?' +
            'Reference\s*=\s*0x0100.*?;\s*end_if;\s*' +
            'if\s+groupCommandInputValid\s*=\s*TRUE\s+then') (
            "Service $($singleByteCommand.Name) exact request envelope is missing.")
        if ($singleByteCommand.Block -match (
            '(?s)if\b(?:(?!\bthen\b).)*' +
            'RequestFrameSize\s*=\s*9(?:(?!\bthen\b).)*' +
            'pRequestFrame(?:(?!\bthen\b).)*\bthen\b')) {
            throw (
                "Service $($singleByteCommand.Name) dereferences byte 8 " +
                'inside the size-test expression instead of after its nested gate.')
        }
    }
    if ($serviceGroupMembersCaseBlock -match (
        '(?s)if\b(?:(?!\bthen\b).)*' +
        'RequestFrameSize\s*=\s*9(?:(?!\bthen\b).)*' +
        'pRequestFrame(?:(?!\bthen\b).)*\bthen\b')) {
        throw ('Service 0x20D2 dereferences byte 8 inside the size-test ' +
            'expression instead of after its nested gate.')
    }
    Assert-Match $serviceGroupEnableCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'IsClientConnected\(#LMCAxis1\).*?' +
        'IsClientConnected\(#LMCAxis4\).*?' +
        'LMCRobot\.RobotIsOn\(\).*?GroupKinematicReady\s*=\s*TRUE.*?' +
        'LMCRobot\.LockProfile\(.*?Axis1:=1.*?Axis4:=1.*?' +
        'Axis5:=0.*?Axis9:=0.*?groupReadRetCode\s*=\s*_LMCPROF_NoError') (
        'Service 0x2047 configured/powered four-axis LockProfile dispatch is missing.')
    Assert-Match $serviceGroupEnableCaseBlock (
        '(?s)LMCRobot\.LockProfile\(\s*' +
        'Axis1:=1\s*,\s*Axis2:=1\s*,\s*Axis3:=1\s*,\s*Axis4:=1\s*,\s*' +
        'Axis5:=0\s*,\s*Axis6:=0\s*,\s*Axis7:=0\s*,\s*Axis8:=0\s*,\s*' +
        'Axis9:=0\s*\)') (
        'Service 0x2047 LockProfile must enable exactly Axis1..4 and ' +
        'disable Axis5..9.')
    if ($serviceGroupEnableCaseBlock -match
        'ReadProfileParameter|_LMCPROF_LockState') {
        throw 'Service 0x2047 must not treat the same-call LockState as completion.'
    }
    Assert-Match $serviceGroupDisableCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'LMCRobot\.ProfileInPosition\(.*?_LMCPROF_ProfileFinished.*?' +
        'groupReadInPosition\s*<>\s*0.*?LMCRobot\.UnlockProfile\(\).*?' +
        'LMCRobot\.ReadProfileParameter\(.*?_LMCPROF_LockState.*?' +
        'profileLockState\s*=\s*0') (
        'Service 0x2048 standstill-gated profile unlock verification is missing.')
    Assert-Match $serviceGroupResetCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'LMCRobot\.AxQuitError\(AxisNo:=0\).*?' +
        'axisCommandStatus\s*:=\s*0.*?axisCommandErrorId\s*:=\s*0') (
        'Service 0x2049 group-axis error reset dispatch is missing.')
    Assert-Match $serviceGroupResetCaseBlock (
        '(?s)ResponseCapacity\s*<\s*16.*?' +
        'axisCommandStatus\s*:=\s*1;.*?' +
        'axisCommandErrorId\s*:=\s*-3;.*?' +
        'if\s+groupCommandInputValid\s*=\s*TRUE\s+then\s*' +
        'axisCommandErrorId\s*:=\s*-2;\s*' +
        'if\s+IsClientConnected\(#LMCRobot\)\s*=\s*1\s+then.*?' +
        'axisCommandStatus\s*:=\s*0;.*?' +
        'axisCommandErrorId\s*:=\s*0;.*?end_if;\s*end_if;\s*' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=16\);.*?' +
        'pResponseFrame\^\$UINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*8;.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UDINT\s*:=\s*' +
        'TO_UDINT\(Reference\);.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*' +
        'axisCommandStatus;.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*' +
        'axisCommandErrorId;.*?ResponseSize\s*:=\s*16') (
        'Service 0x2049 must always return the 16-byte typed ACK with ' +
        'malformed -3, disconnected -2, and accepted zero status semantics.')
    Assert-Match $serviceGroupPowerOnCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'IsClientConnected\(#LMCAxis1\).*?' +
        'IsClientConnected\(#LMCAxis4\).*?' +
        'LMCRobot\.RobotOn\(Mode:=_ACTIVE\)') (
        'Service 0x204A four-axis RobotOn dispatch is missing.')
    if ($serviceGroupPowerOnCaseBlock -match
        'GroupKinematicReady\s*=\s*TRUE') {
        throw 'Service 0x204A must not gate power-on on kinematic readiness.'
    }
    Assert-Match $serviceGroupPowerOffCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'LMCRobot\.RobotOff\(\)') (
        'Service 0x204B RobotOff dispatch is missing.')
    foreach ($signedAck in @(
            @{ Name = '0x2047'; Block = $serviceGroupEnableCaseBlock },
            @{ Name = '0x2048'; Block = $serviceGroupDisableCaseBlock },
            @{ Name = '0x204A'; Block = $serviceGroupPowerOnCaseBlock },
            @{ Name = '0x204B'; Block = $serviceGroupPowerOffCaseBlock })) {
        Assert-Match $signedAck.Block (
            '(?s)groupReadErrorId\s*:=\s*-2;\s*' +
            'if\s+\(?IsClientConnected\(#LMCRobot\).*?' +
            'end_if;\s*' +
            '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=16\);.*?' +
            'pResponseFrame\^\$UINT\s*:=\s*0;.*?' +
            '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*8;.*?' +
            '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
            '\(pResponseFrame\s*\+\s*8\)\^\$UDINT\s*:=\s*' +
            'TO_UDINT\(Reference\);.*?' +
            'if\s+groupReadErrorId\s*=\s*0\s+then.*?' +
            '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*0;.*?' +
            '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*0;.*?' +
            'elsif\s+\(groupReadErrorId\s*>=\s*-32768\)\s*&\s*' +
            '\(groupReadErrorId\s*<=\s*32767\).*?' +
            '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*1;.*?' +
            '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*' +
            'groupReadErrorId\$INT.*?else.*?' +
            '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*1;.*?' +
            '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*-6;.*?' +
            'ResponseSize\s*:=\s*16;\s*else\s*' +
            'if\s+ResponseCapacity\s*<\s*12\s+then.*?' +
            '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=12\);.*?' +
            'pResponseFrame\^\$UINT\s*:=\s*1;.*?' +
            '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*4;.*?' +
            '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
            '\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*1;.*?' +
            '\(pResponseFrame\s*\+\s*10\)\^\$INT\s*:=\s*-3;.*?' +
            'ResponseSize\s*:=\s*12') (
            "Service $($signedAck.Name) typed ACK, disconnected -2 " +
            'mapping, malformed -3 short frame, or signed native-error ' +
            'mapping is incomplete.')
    }

    Assert-Match $serviceGroupStopCaseBlock (
        '(?s)ResponseCapacity\s*<\s*16.*?RequestFrameSize\s*=\s*24.*?' +
        'pRequestFrame\s*\+\s*8.*?pRequestFrame\s*\+\s*12.*?' +
        'pRequestFrame\s*\+\s*16.*?pRequestFrame\s*\+\s*20.*?' +
        'Reference\s*=\s*0x0100.*?bufferMode\s*=\s*1.*?' +
        'groupExecute\s*=\s*1.*?groupDecel\s*>=\s*0.*?' +
        '\(groupJerk\s*>=\s*0\)\s*&\s*' +
        '\(\(groupJerk\s*=\s*0\)\s*\|\s*' +
        '\(groupDecel\s*>\s*0\)\).*?LMCRobot\.StopMove\(.*?' +
        'Mode:=3.*?Decel:=groupDecel.*?Jerk:=groupJerk.*?' +
        'groupReadErrorId\s*:=\s*0') (
        'Service 0x2085 exact offsets, validation, or StopMove dispatch is missing.')
    if ([regex]::Matches(
            $serviceGroupStopCaseBlock,
            '\bgroupStopCommandNo\b').Count -ne 2) {
        throw 'Service 0x2085 must treat StopMove output only as an opaque command number.'
    }
    Assert-Match $serviceGroupStopCaseBlock (
        '(?s)groupReadErrorId\s*:=\s*-3;.*?' +
        'if\s+groupCommandInputValid\s*=\s*TRUE\s+then\s*' +
        'groupReadErrorId\s*:=\s*-2;\s*' +
        'if\s+IsClientConnected\(#LMCRobot\)\s*=\s*1\s+then.*?' +
        'LMCRobot\.StopMove\(.*?groupReadErrorId\s*:=\s*0;.*?' +
        'end_if;\s*else\s*groupReadErrorId\s*:=\s*-7;\s*end_if;\s*' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=16\);.*?' +
        'pResponseFrame\^\$UINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*8;.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UDINT\s*:=\s*' +
        'TO_UDINT\(Reference\);.*?' +
        'if\s+groupReadErrorId\s*=\s*0\s+then.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*0;.*?' +
        'else.*?\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*1;.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*' +
        'groupReadErrorId\$INT;.*?ResponseSize\s*:=\s*16') (
        'Service 0x2085 must always return the 16-byte typed ACK with ' +
        'disconnected -2, invalid-motion -7, and accepted zero semantics.')

    Assert-Match $serviceGroupMoveCaseBlock (
        '(?s)ResponseSize\s*:=\s*MoveLinearAbsEx\(.*?' +
        'Reference:=Reference.*?pResponseFrame:=pResponseFrame.*?' +
        'ResponseCapacity:=ResponseCapacity.*?' +
        'pRequestFrame:=pRequestFrame.*?' +
        'RequestFrameSize:=RequestFrameSize') (
        'Service 0x20A4 does not delegate the unchanged zero-copy frame ABI.')
    Assert-Match $serviceMoveLinearBlock (
        '(?s)pResponseFrame\s*=\s*NIL.*?ResponseCapacity\s*<\s*16.*?' +
        'pRequestFrame\s*<>\s*NIL.*?RequestFrameSize\s*=\s*104.*?' +
        'Reference\s*=\s*0x0100.*?_memcpy\(ptr1:=#GroupMovePos,\s*' +
        'ptr2:=pRequestFrame\s*\+\s*8,\s*cntr:=16\)') (
        'Service MoveLinearAbsEx exact request/capacity/position-vector contract is missing.')
    foreach ($offset in @(72, 76, 80, 84, 88, 92, 96, 100)) {
        Assert-Match $serviceMoveLinearBlock (
            '\(pRequestFrame\s*\+\s*' + $offset + '\)\^\$DINT') (
            "Service 0x20A4 request DINT offset $offset is missing.")
    }
    Assert-Match $serviceMoveLinearBlock (
        '(?s)for kinIndex\s*:=\s*4 to 15 do.*?' +
        'pRequestFrame\s*\+\s*8\s*\+\s*' +
        'TO_UDINT\(kinIndex \* 4\).*?' +
        'groupCommandInputValid\s*:=\s*FALSE') (
        'Service 0x20A4 non-four-axis position rejection is missing.')
    Assert-Match $serviceMoveLinearBlock (
        '(?s)groupVelocity\s*>\s*0.*?groupAccel\s*>\s*0.*?' +
        'groupDecel\s*>\s*0.*?groupJerk\s*>=\s*0.*?' +
        'groupCoordSystem\s*=\s*0.*?' +
        'groupTransitionModeInput\s*=\s*0.*?' +
        'groupTransitionModeInput\s*=\s*2.*?' +
        'bufferMode\s*=\s*1.*?bufferMode\s*=\s*2.*?' +
        'groupExecute\s*=\s*1') (
        'Service 0x20A4 approved motion parameter validation is incomplete.')
    Assert-Match $serviceMoveLinearBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'IsClientConnected\(#LMCAxis1\).*?' +
        'IsClientConnected\(#LMCAxis4\).*?LMCRobot\.RobotIsOn\(\).*?' +
        'LMCRobot\.ReadProfileParameter\(.*?_LMCPROF_LockState.*?' +
        'GroupKinematicReady\s*=\s*TRUE.*?powerIsOn\s*<>\s*0.*?' +
        'profileLocked\s*=\s*TRUE.*?LMCRobot\.MoveLinearCoord\(.*?' +
        'pPositions:=#GroupMovePos.*?CmdConfig:=groupCommandConfig.*?' +
        'Velocity:=groupVelocity.*?Accel:=groupAccel.*?' +
        'Decel:=groupDecel.*?TransMode:=groupTransitionMode.*?' +
        'TransRadius:=groupTransitionRadius.*?CoordSystem:=0.*?' +
        'Jerk:=groupJerk.*?' +
        'groupMoveRetCode\s*=\s*_LMCPROF_NoError.*?' +
        'groupReadErrorId\s*:=\s*0') (
        'Service MoveLinearAbsEx powered/locked dispatch and return-code gate is missing.')
    Assert-Match $serviceMoveLinearBlock (
        '(?s)\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*8.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UDINT\s*:=\s*' +
        'TO_UDINT\(Reference\).*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*0.*?' +
        'else.*?\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*1') (
        'Service MoveLinearAbsEx 16-byte typed acknowledgement is incomplete.')

    Assert-Match $serviceGroupStatusCaseBlock (
        '(?s)RequestFrameSize\s*=\s*16.*?' +
        'payloadReference\s*:=\s*\(pRequestFrame\s*\+\s*8\)\^\$DINT.*?' +
        'executeRequest\s*:=\s*\(pRequestFrame\s*\+\s*12\)\^\$DINT.*?' +
        'Reference\s*=\s*0x0100.*?' +
        'payloadReference\s*=\s*TO_DINT\(Reference\).*?' +
        'executeRequest\s*=\s*1.*?IsClientConnected\(#LMCRobot\).*?' +
        'ResponseSize\s*:=\s*GroupReadStatus\(\s*' +
        'pResponseFrame:=pResponseFrame\s*,\s*' +
        'ResponseCapacity:=ResponseCapacity\s*\)') (
        'Service 0x2045 exact descriptor request or GroupReadStatus dispatch is missing.')
    Assert-Match $serviceGroupStatusCaseBlock (
        '(?s)if\s+\(RequestFrameSize\s*=\s*16\)\s*&\s*' +
        '\(Reference\s*=\s*0x0100\)\s*&\s*' +
        '\(payloadReference\s*=\s*TO_DINT\(Reference\)\)\s*&\s*' +
        '\(executeRequest\s*=\s*1\)\s+then\s*' +
        'if\s+IsClientConnected\(#LMCRobot\)\s*=\s*1\s+then.*?' +
        'ResponseSize\s*:=\s*GroupReadStatus\(.*?' +
        'else\s*if\s+ResponseCapacity\s*<\s*12\s+then.*?' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=12\);.*?' +
        'pResponseFrame\^\$UINT\s*:=\s*1;.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*4;.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*1;.*?' +
        '\(pResponseFrame\s*\+\s*10\)\^\$INT\s*:=\s*-2;.*?' +
        'ResponseSize\s*:=\s*12;\s*end_if;\s*else\s*' +
        'if\s+ResponseCapacity\s*<\s*12\s+then.*?' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=12\);.*?' +
        'pResponseFrame\^\$UINT\s*:=\s*1;.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*4;.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*1;.*?' +
        '\(pResponseFrame\s*\+\s*10\)\^\$INT\s*:=\s*-3;.*?' +
        'ResponseSize\s*:=\s*12') (
        'Service 0x2045 must distinguish disconnected -2 from malformed ' +
        '-3 using the exact 12-byte outer fail-closed frame.')
    Assert-Match $serviceGroupReadStatusBlock (
        '(?s)ResponseCapacity\s*<\s*20.*?' +
        'LMCRobot\.ProfileInPosition\(.*?_LMCPROF_ProfileFinished.*?' +
        'LMCRobot\.RobotIsOn\(\).*?LMCRobot\.ReadProfileParameter\(.*?' +
        '_LMCPROF_LockState.*?LMCRobot\.ReadRobotParameter\(.*?' +
        '_ROBOT_STATE.*?powerIsOn\s*<>\s*0.*?' +
        'groupReadState\s*:=\s*groupReadState or 0x00040000.*?' +
        'profileLocked\s*=\s*TRUE.*?groupReadInPosition\s*<>\s*0.*?' +
        'groupReadState\s*:=\s*groupReadState or 0x00020000.*?' +
        'profileLocked\s*=\s*FALSE.*?' +
        'groupReadState\s*:=\s*groupReadState or 0x00010000') (
        'Service GroupReadStatus power/lock/in-position state mapping is missing.')
    Assert-Match $serviceGroupReadStatusBlock (
        '(?s)robotState\s*=\s*_ROBOT_ERROR\$DINT.*?' +
        'LMCRobot\.ReadProfileError\(\).*?' +
        'groupReadErrorId\s*:=\s*profileErrorInfo\.ErrorNo\$DINT.*?' +
        'groupReadErrorId\s*=\s*0.*?groupReadErrorId\s*:=\s*-6.*?' +
        'robotState\s*<\s*_ROBOT_PASSIVE\$DINT.*?' +
        'robotState\s*>\s*_ROBOT_MODE_CHANGE\$DINT') (
        'Service GroupReadStatus native error and false-success guards are missing.')
    Assert-Match $serviceGroupReadStatusBlock (
        '(?s)_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=20\).*?' +
        'pResponseFrame\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*12.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UDINT\s*:=\s*groupReadState.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*16\)\^\$UINT\s*:=\s*' +
        'groupReadErrorId\$UINT.*?ResponseSize\s*:=\s*20') (
        'Service GroupReadStatus 20-byte typed response is incomplete.')
    if ($serviceGroupReadStatusBlock -match '\bgroupMoveRetCode\b') {
        throw 'Service GroupReadStatus must not report stale move return state.'
    }

    Assert-Match $serviceGroupPositionCaseBlock (
        '(?s)RequestFrameSize\s*=\s*16.*?' +
        'groupCoordSystem\s*:=\s*\(pRequestFrame\s*\+\s*8\)\^\$DINT.*?' +
        'groupExecute\s*:=\s*\(pRequestFrame\s*\+\s*12\)\^\$DINT.*?' +
        'Reference\s*=\s*0x0100.*?groupExecute\s*=\s*1.*?' +
        'groupCoordSystem\s*=\s*0.*?groupCoordSystem\s*=\s*1.*?' +
        'LMCRobot\.GetRobotPosition\(.*?_ACTPOS_APPUNITS.*?' +
        'CoordSystem:=0.*?pPositions:=#groupReadPos.*?' +
        'groupCoordSystem\s*=\s*2.*?groupCoordSystem\s*=\s*3.*?' +
        'groupReadErrorId\s*:=\s*-7') (
        'Service 0x2051 coordinate validation or GetRobotPosition mapping is missing.')
    Assert-Match $serviceGroupPositionCaseBlock (
        '(?s)groupReadRetCode\s*:=\s*LMCRobot\.GetRobotPosition\(.*?' +
        'if\s+groupReadRetCode\s*=\s*_LMCPROF_NoError\s+then\s*' +
        'groupReadErrorId\s*:=\s*0;\s*' +
        'elsif\s+groupReadRetCode\$UDINT\s*<=\s*32767\s+then\s*' +
        'groupReadErrorId\s*:=\s*groupReadRetCode\$DINT;\s*' +
        'else\s*groupReadErrorId\s*:=\s*-6;\s*end_if;') (
        'Service 0x2051 must map only _LMCPROF_NoError to success, ' +
        'preserve representable native errors, and map overflow to -6.')
    Assert-Match $serviceGroupPositionCaseBlock (
        '(?s)ResponseCapacity\s*<\s*76.*?' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=76\).*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*68.*?' +
        '_memcpy\(ptr1:=pResponseFrame\s*\+\s*8,\s*' +
        'ptr2:=#groupReadPos,\s*cntr:=36\).*?' +
        '\(pResponseFrame\s*\+\s*72\)\^\$UINT\s*:=\s*0x4000.*?' +
        'ResponseCapacity\s*<\s*12.*?' +
        'pResponseFrame\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*4') (
        'Service 0x2051 success frame or outer-status-zero error frame is incomplete.')
    Assert-Match $serviceGroupPositionCaseBlock (
        '(?s)ResponseSize\s*:=\s*76;\s*else\s*' +
        'if\s+ResponseCapacity\s*<\s*12\s+then.*?' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=12\);.*?' +
        'pResponseFrame\^\$UINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*4;.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*1;.*?' +
        '\(pResponseFrame\s*\+\s*10\)\^\$INT\s*:=\s*' +
        'groupReadErrorId\$INT;.*?ResponseSize\s*:=\s*12') (
        'Service 0x2051 error path must return the exact outer-success ' +
        '12-byte status/error frame.')

    $kinSizeGuard = [regex]::Match(
        $serviceKinematicCaseBlock,
        'kinValid\s*:=\s*\(RequestFrameSize\s*=\s*1328\)\s*&\s*' +
        '\(Reference\s*=\s*0x0100\)')
    $kinFirstGate = [regex]::Match(
        $serviceKinematicCaseBlock,
        'if\s+kinValid\s*=\s*TRUE\s+then')
    $kinFirstDereference = [regex]::Match(
        $serviceKinematicCaseBlock,
        '\(pRequestFrame\s*\+')
    if (-not $kinSizeGuard.Success -or -not $kinFirstGate.Success -or
        -not $kinFirstDereference.Success -or
        $kinSizeGuard.Index -ge $kinFirstGate.Index -or
        $kinFirstGate.Index -ge $kinFirstDereference.Index) {
        throw ('Service 0x20E7 must establish the exact 1328-byte guard ' +
            'before its first request-pointer dereference.')
    }
    if ([regex]::Matches(
            $serviceKinematicCaseBlock,
            'if\s+kinValid\s*=\s*TRUE\s+then').Count -ne 4) {
        throw ('Service 0x20E7 must retain three bounded validation ' +
            'stages followed by one guarded dispatch stage.')
    }
    Assert-Match $serviceKinematicCaseBlock (
        '(?s)if\s+kinValid\s*=\s*TRUE\s+then\s*' +
        'for kinIndex\s*:=\s*0 to 3 do.*?' +
        'pRequestFrame\s*\+\s*8.*?' +
        '0x3FF00000.*?TO_UDINT\(kinIndex \+ 1\).*?' +
        'pRequestFrame\s*\+\s*44') (
        'Service 0x20E7 four-axis identity-entry validation is incomplete.')
    Assert-Match $serviceKinematicCaseBlock (
        '(?s)for kinIndex\s*:=\s*168 to 647 do.*?' +
        'for kinIndex\s*:=\s*652 to 1311 do.*?' +
        'for kinIndex\s*:=\s*1321 to 1327 do') (
        'Service 0x20E7 reserved zero ranges do not cover the complete frame tail.')
    Assert-Match $serviceKinematicCaseBlock (
        '(?s)\(pRequestFrame\s*\+\s*648\)\^\$DINT\s*<>\s*4.*?' +
        '\(pRequestFrame\s*\+\s*1312\)\^\$DINT\s*<>\s*0.*?' +
        '\(pRequestFrame\s*\+\s*1316\)\^\$DINT\s*<>\s*2.*?' +
        '\(pRequestFrame\s*\+\s*1320\)\^\$DINT\s*<>\s*1') (
        'Service 0x20E7 Cartesian4 topology constants are incomplete.')
    Assert-Match $serviceKinematicCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'IsClientConnected\(#LMCAxis1\).*?' +
        'IsClientConnected\(#LMCAxis4\).*?' +
        'GroupKinematicReady\s*:=\s*TRUE.*?' +
        'groupReadErrorId\s*:=\s*0') (
        'Service 0x20E7 four-axis mapping registration is missing.')
    if ($serviceKinematicCaseBlock -match
        'LockProfile|UnlockProfile|RobotOn|RobotOff') {
        throw 'Service 0x20E7 must not change profile-lock or group-power state.'
    }
    Assert-Match $serviceKinematicCaseBlock (
        '(?s)ResponseCapacity\s*<\s*12.*?' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=12\).*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*4.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*0.*?' +
        'else.*?\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*1') (
        'Service 0x20E7 short acknowledgement framing is incomplete.')

    Assert-Match $serviceAdminGroupParametersCaseBlock (
        '(?s)RequestFrameSize\s*>=\s*16.*?' +
        'pRequestFrame\s*\+\s*8.*?pRequestFrame\s*\+\s*10.*?' +
        'pRequestFrame\s*\+\s*12.*?RequestFrameSize\s*>=\s*20.*?' +
        'pRequestFrame\s*\+\s*16.*?RequestFrameSize\s*<>\s*20.*?' +
        'Reference\s*<>\s*0x0100.*?adminSchemaVersion\s*<>\s*1.*?' +
        'adminRequestFlags\s*<>\s*0.*?adminRequestId\s*=\s*0.*?' +
        'adminSelectionMask\s*=\s*0.*?' +
        'adminSelectionMask and 0xFFFFFFF8.*?' +
        'IsClientConnected\(#LMCRobot\)\s*<>\s*1') (
        'Service 0x7D20 exact request offsets/reference/mask validation is incomplete.')
    Assert-Match $serviceAdminGroupParametersCaseBlock (
        '(?s)if\s+RequestFrameSize\s*<>\s*20\s+then\s*' +
        'adminDetailCode\s*:=\s*5;\s*' +
        'elsif\s+Reference\s*<>\s*0x0100\s+then\s*' +
        'adminDetailCode\s*:=\s*4;\s*' +
        'elsif\s+adminSchemaVersion\s*<>\s*1\s+then\s*' +
        'adminDetailCode\s*:=\s*1;\s*' +
        'elsif\s+adminRequestFlags\s*<>\s*0\s+then\s*' +
        'adminDetailCode\s*:=\s*2;\s*' +
        'elsif\s+adminRequestId\s*=\s*0\s+then\s*' +
        'adminDetailCode\s*:=\s*3;\s*' +
        'elsif\s+\(adminSelectionMask\s*=\s*0\)\s*\|\s*' +
        '\(\(adminSelectionMask\s+and\s+0xFFFFFFF8\)\s*<>\s*0\)\s+then\s*' +
        'adminDetailCode\s*:=\s*8;\s*' +
        'elsif\s+IsClientConnected\(#LMCRobot\)\s*<>\s*1\s+then\s*' +
        'adminDetailCode\s*:=\s*7;\s*end_if;') (
        'Service 0x7D20 Admin detail mapping must remain exactly ' +
        'size=5, reference=4, schema=1, flags=2, request=3, mask=8, client=7.')
    foreach ($groupParameter in @(
            '_LMCPROF_GRP_VEL_LIMIT',
            '_LMCPROF_GRP_ACCEL_LIMIT',
            '_LMCPROF_GRP_TJERK')) {
        Assert-Match $serviceAdminGroupParametersCaseBlock (
            'LMCRobot\.ReadGroupParameter\(\s*GrpNo:=1,\s*ParNo:=' +
            [regex]::Escape($groupParameter) + '\)') (
            "Service 0x7D20 is missing $groupParameter mapping.")
    }
    if ([regex]::Matches(
            $serviceAdminGroupParametersCaseBlock,
            '\bLMCRobot\.ReadGroupParameter\s*\(').Count -ne 3) {
        throw 'Service 0x7D20 must expose exactly three selected native reads.'
    }
    Assert-Match $serviceAdminGroupParametersCaseBlock (
        '(?s)ResponseCapacity\s*<\s*40.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*32.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*24\)\^\$UDINT\s*:=\s*' +
        'adminSelectionMask.*?' +
        '\(pResponseFrame\s*\+\s*28\)\^\$DINT\s*:=\s*' +
        'adminGroupVelocityLimit.*?' +
        '\(pResponseFrame\s*\+\s*36\)\^\$DINT\s*:=\s*' +
        'adminGroupJerkTime.*?ResponseCapacity\s*<\s*24.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*16.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*1.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*-31000') (
        'Service 0x7D20 success/error Admin frames are incomplete.')

    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)ResponseCapacity\s*<\s*24.*?' +
        'RequestFrameSize\s*>=\s*16.*?pRequestFrame\s*\+\s*8.*?' +
        'pRequestFrame\s*\+\s*10.*?pRequestFrame\s*\+\s*12.*?' +
        'RequestFrameSize\s*<>\s*112.*?Reference\s*<>\s*0x0100.*?' +
        'adminSchemaVersion\s*<>\s*1.*?adminRequestFlags\s*<>\s*0.*?' +
        'adminRequestId\s*=\s*0') (
        'Service 0x7D22 exact request envelope validation is incomplete.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)if\s+RequestFrameSize\s*<>\s*112\s+then\s*' +
        'adminDetailCode\s*:=\s*5;\s*' +
        'elsif\s+Reference\s*<>\s*0x0100\s+then\s*' +
        'adminDetailCode\s*:=\s*4;\s*' +
        'elsif\s+adminSchemaVersion\s*<>\s*1\s+then\s*' +
        'adminDetailCode\s*:=\s*1;\s*' +
        'elsif\s+adminRequestFlags\s*<>\s*0\s+then\s*' +
        'adminDetailCode\s*:=\s*2;\s*' +
        'elsif\s+adminRequestId\s*=\s*0\s+then\s*' +
        'adminDetailCode\s*:=\s*3;\s*else.*?' +
        'if\s+groupCommandInputValid\s*=\s*FALSE\s+then\s*' +
        'adminDetailCode\s*:=\s*9;\s*end_if;\s*end_if;') (
        'Service 0x7D22 Admin input detail mapping must remain exactly ' +
        'size=5, reference=4, schema=1, flags=2, request=3, motion=9.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)_memcpy\(ptr1:=#GroupMovePos,\s*' +
        'ptr2:=pRequestFrame\s*\+\s*16,\s*cntr:=16\).*?' +
        'pRequestFrame\s*\+\s*80.*?pRequestFrame\s*\+\s*84.*?' +
        'pRequestFrame\s*\+\s*88.*?pRequestFrame\s*\+\s*92.*?' +
        'pRequestFrame\s*\+\s*96.*?pRequestFrame\s*\+\s*100.*?' +
        'pRequestFrame\s*\+\s*104.*?pRequestFrame\s*\+\s*108') (
        'Service 0x7D22 position and DINT field offsets are incomplete.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)groupVelocity\s*>\s*0.*?groupAccel\s*>\s*0.*?' +
        'groupDecel\s*>\s*0.*?groupJerk\s*>=\s*0.*?' +
        'groupCoordSystem\s*=\s*0.*?' +
        'groupTransitionModeInput\s*=\s*0.*?' +
        'groupTransitionModeInput\s*=\s*2.*?' +
        'bufferMode\s*=\s*1.*?bufferMode\s*=\s*2.*?' +
        'groupExecute\s*=\s*1.*?for kinIndex\s*:=\s*4 to 15 do.*?' +
        'groupCommandInputValid\s*:=\s*FALSE.*?adminDetailCode\s*:=\s*9') (
        'Service 0x7D22 motion and four-axis tail validation is incomplete.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)case groupTransitionModeInput of.*?_LMCPROF_EXACT_STOP.*?' +
        '_LMCPROF_CONT_DIRECT.*?bufferMode\s*=\s*1.*?' +
        'groupCommandConfig\s*:=\s*16') (
        'Service 0x7D22 transition/buffer mapping is incomplete.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'IsClientConnected\(#LMCAxis1\).*?' +
        'IsClientConnected\(#LMCAxis4\).*?LMCRobot\.RobotIsOn\(\).*?' +
        'LMCRobot\.ReadProfileParameter\(.*?_LMCPROF_LockState.*?' +
        'GroupKinematicReady\s*=\s*TRUE.*?powerIsOn\s*<>\s*0.*?' +
        'profileLockState\s*<>\s*0.*?LMCRobot\.MoveRelativeCoord\(.*?' +
        'pDistances:=#GroupMovePos.*?CmdConfig:=groupCommandConfig.*?' +
        'Velocity:=groupVelocity.*?Accel:=groupAccel.*?' +
        'Decel:=groupDecel.*?TransMode:=groupTransitionMode.*?' +
        'TransRadius:=groupTransitionRadius.*?CoordSystem:=0.*?' +
        'Jerk:=groupJerk') (
        'Service 0x7D22 powered/locked MoveRelativeCoord dispatch is missing.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)if\s+adminDetailCode\s*=\s*0\s+then\s*' +
        'if\s+\(IsClientConnected\(#LMCRobot\)\s*=\s*1\).*?then.*?' +
        'if\s+\(GroupKinematicReady\s*=\s*TRUE\)\s*&\s*' +
        '\(powerIsOn\s*<>\s*0\)\s*&\s*' +
        '\(profileLockState\s*<>\s*0\)\s+then.*?' +
        'if\s+groupMoveRetCode\s*=\s*_LMCPROF_NoError\s+then\s*' +
        'adminErrorId\s*:=\s*0;\s*else\s*' +
        'adminDetailCode\s*:=\s*11;.*?end_if;\s*' +
        'else\s*adminDetailCode\s*:=\s*10;\s*end_if;\s*' +
        'else\s*adminDetailCode\s*:=\s*10;\s*end_if;\s*end_if;') (
        'Service 0x7D22 must map readiness/client failure to detail 10 ' +
        'and native rejection to detail 11.')
    if ([regex]::Matches(
            $serviceAdminRelativeMoveCaseBlock,
            'adminDetailCode\s*:=\s*10\s*;').Count -ne 2 -or
        [regex]::Matches(
            $serviceAdminRelativeMoveCaseBlock,
            'adminDetailCode\s*:=\s*11\s*;').Count -ne 1) {
        throw 'Service 0x7D22 state detail 10 and native detail 11 assignments are not exact.'
    }
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)groupMoveRetCode\s*=\s*_LMCPROF_NoError.*?' +
        'adminErrorId\s*:=\s*0.*?adminDetailCode\s*:=\s*11.*?' +
        'groupMoveRetCode\$UDINT\s*<=\s*32767.*?' +
        'adminErrorId\s*:=\s*groupMoveRetCode\$INT.*?' +
        'adminErrorId\s*:=\s*-6.*?adminDetailCode\s*:=\s*10') (
        'Service 0x7D22 native rejection/state detail mapping is incomplete.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=24\).*?' +
        'pResponseFrame\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*16.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*1.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*16\)\^\$UDINT\s*:=\s*' +
        'adminRequestId.*?' +
        '\(pResponseFrame\s*\+\s*20\)\^\$UDINT\s*:=\s*' +
        'adminDetailCode.*?adminDetailCode\s*<>\s*0.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*1.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*adminErrorId') (
        'Service 0x7D22 fixed outer-success Admin response framing is incomplete.')
}

$responseBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION VIRTUAL GLOBAL TCPMotionInterface::Response.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($responseBlock)) {
    throw 'TCPMotionInterface.Response implementation was not found.'
}
if ($responseBlock -match '\bMsgPaser\s*\(') {
    throw 'Response still calls MsgPaser directly.'
}
if ($responseBlock -match '\bSendData\s*\(') {
    throw 'Response still performs TCP send work.'
}
if ($responseBlock -match '\b(?:LMCAxis[1-9]|LMCRobot)\s*\.') {
    throw 'Response still performs a LASAL motion client call.'
}

$motionCyWorkBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($motionCyWorkBlock)) {
    throw 'TCPMotionInterface.CyWork implementation was not found.'
}
if ($motionCyWorkBlock -match '_GetObjName|_strlen|_stricmp|_strcmp') {
    throw 'CyWork still performs periodic object-name discovery or string comparison.'
}
Assert-Match $st 'PendingClosedSessionEpoch\s*:\s*UDINT' 'TCPMotionInterface pending closed-session epoch storage is missing.'
Assert-Match $motionCyWorkBlock '(?s)PendingClosedSessionEpoch <> 0.*?IsClientConnected\(#Diagnostics\).*?Diagnostics\.NotifySessionClosed\(\s*SessionEpoch:=PendingClosedSessionEpoch\).*?PendingClosedSessionEpoch := 0.*?currentEpoch := SessionEpoch' 'TCPMotionInterface.CyWork does not flush the pending closed epoch to LMCDiagnosticsService before processing requests.'
$closedEpochCaptureCount = [regex]::Matches(
    $st,
    '(?s)if \(SessionEpoch <> 0\)\s*&\s*\(PendingClosedSessionEpoch = 0\) then\s*PendingClosedSessionEpoch := SessionEpoch;\s*end_if;\s*SessionEpoch \+= 1').Count
if ($closedEpochCaptureCount -ne 3) {
    throw "TCPMotionInterface first-wins closed-session capture count is $closedEpochCaptureCount, expected three disconnect/send/close paths."
}
Assert-Match $motionCyWorkBlock '(?s)RequestQueue\[QueueReadIndex\$DINT\]\.State\s*=\s*TCPMI_QUEUE_READY.*?State\s*:=\s*TCPMI_QUEUE_ACTIVE.*?MemCpy.*?State\s*:=\s*TCPMI_QUEUE_FREE' 'CyWork queue READY/ACTIVE/FREE transition is missing.'
Assert-Match $motionCyWorkBlock '(?s)CommandID\s*:=\s*TO_DINT\(ActiveRequest\.CommandId\);.*?AxisRef\s*:=\s*TO_DINT\(ActiveRequest\.Reference\);.*?Payload\s*:=\s*TO_DINT\(ActiveRequest\.PayloadLength\);.*?MsgPaser\(\);.*?ActiveRequestValid\s*:=\s*FALSE' 'CyWork does not numerically widen, execute, and release one active request.'
Assert-Match $motionCyWorkBlock '(?s)MsgPaser\(\);.*?ActiveRequestValid\s*:=\s*FALSE.*?if IsClientConnected\(#Diagnostics\) then\s*Diagnostics\.ProcessOperations\(\);\s*end_if' 'TCPMotionInterface.CyWork does not safely advance D5 operations after request processing.'
if ($motionCyWorkBlock -match 'ActiveRequest\.(?:CommandId|Reference|PayloadLength)\$DINT') {
    throw 'CyWork reinterprets a 16-bit request field as a 32-bit DINT instead of using numeric conversion.'
}

$msgParserCallCount = [regex]::Matches($st, '(?m)^\s*MsgPaser\(\);\s*$').Count
if ($msgParserCallCount -ne 1) {
    throw "MsgPaser call count is $msgParserCallCount, expected one CyWork caller."
}

Assert-Match $responseBlock '(?s)State\s*=\s*TCPMI_QUEUE_FREE.*?State\s*:=\s*TCPMI_QUEUE_WRITING.*?State\s*:=\s*TCPMI_QUEUE_READY' 'Response queue FREE/WRITING/READY transition is missing.'

$sendDataBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION VIRTUAL GLOBAL TCPMotionInterface::SendData.*?END_FUNCTION').Value
Assert-Match $sendDataBlock '_TCPIPServerInterface::SendData' 'TCPMotionInterface.SendData base call is missing.'
Assert-Match $sendDataBlock 'if dRetcode <> udSize\$DINT then' 'Partial/failed send check is missing.'
Assert-Match $sendDataBlock 'IngressFaultCloseRequired\s*:=\s*TRUE' 'Partial send quarantine is missing.'
Assert-Match $sendDataBlock 'SessionEpoch\s*\+=\s*1' 'Partial send does not invalidate the session epoch.'
Assert-Match $st 'vmt\.UserFcts\[2\]\s*:=\s*#SendData\(\)' 'TCPMotionInterface.SendData override is not registered.'

$tcpRtWorkBlock = [regex]::Match(
    $tcpServerRt,
    '(?s)FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($tcpRtWorkBlock)) {
    throw '_TCPIPServer_RT.RtWork implementation was not found.'
}
if ($tcpRtWorkBlock -match '\bCyclicCall\s*\(') {
    throw '_TCPIPServer_RT.RtWork still owns TCP transport work.'
}

if ($st -match '(?m)^\s*0x208[1-4]\s*:') {
    throw 'Legacy 0x2081..0x2084 handler is active.'
}

$upperBitMappings = [regex]::Matches($st, 'AxisCommandState\$UDINT\s+and\s+0xFFFF0000').Count
if ($upperBitMappings -ne 4) {
    throw "32-bit axis error truncation guards=$upperBitMappings, expected 4."
}

Assert-Match $st 'AxisObjectName1\s*:\s*ARRAY \[0\.\.255\] OF CHAR' 'LASAL object-name buffer is not 256 bytes.'
if ($st -match 'AxisObjectName[5-9]\s*:\s*ARRAY') {
    throw 'Axes 5..9 must reuse an IDE-registered object-name buffer instead of adding CodeGenerator-only class variables.'
}
Assert-Match $st '(?s)AxisCommandInputValid\s*:=.*?\(dir = 2\).*?\(bufMode = 1\).*?\(Exec = 1\)' 'Shortest-only axis direction validation is missing.'
Assert-Match $st '(?s)\(dec = 0\).*?\(Exec = 1\)' 'MoveVelocity deceleration/execute validation is missing.'
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize, reference\);\s*WriteInt32\(buffer, HeaderSize \+ 4, 1\);' 'C# read-status descriptor payload is missing.'
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize \+ 64, velocity\);' 'C# group velocity offset is not 64 bytes into payload.'
Assert-Match $protocol 'WriteInt32\(\s*buffer,\s*HeaderSize \+ 92,\s*options\.Execute \? 1 : 0\s*\);' 'C# group execute option is not serialized at payload offset 92.'

if ($SourceOnly) {
    Write-Host (
        "PASS LASAL.StaticContract.SourceOnly ($ControlServiceCheckpoint; " +
        'Admin reads and 0x7D22 relative motion, CyWork queue, ' +
        'control-service checkpoint, diagnostics D1-D5, recorder bank, ' +
        'and session-close wiring)')
}
else {
    Write-Host (
        "PASS LASAL.StaticContract ($ControlServiceCheckpoint; " +
        'Admin reads and 0x7D22 relative motion, CyWork queue, ' +
        'control-service checkpoint, diagnostics D1-D5, nine-axis network, ' +
        'recorder wiring, and generated metadata/tables)')
}
