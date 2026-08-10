[CmdletBinding(DefaultParameterSetName = 'Current')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Current')]
    [switch]$VerifyCurrent,

    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$RunSelfTest,

    [Parameter(ParameterSetName = 'Current')]
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($VerifyCurrent -and [string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Join-Path $PSScriptRoot '..\..\..\..'
}

$Owner = 'LASAL.UdpCallbackGateDDeclaration'
$GateCCommit = '17cdd13dd6876af388e661401e1b7423f96df9f1'
$LasalRoot = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis'
$DiagnosticsRelativePath =
    "$LasalRoot/Class/LMCDiagnosticsService/LMCDiagnosticsService.st"
$TcpRelativePath =
    "$LasalRoot/Class/TCPMotionInterface/TCPMotionInterface.st"
$ClassesRelativePath = "$LasalRoot/Class/Classes.lcb"
$ProjectRelativePath = "$LasalRoot/Elmo_EtherCAT_Test_4Axis.lcb"
$ProjectExpectedBytes = 634514
$ProjectBaselineSha256 =
    'C0975BA573245BBDBA78F7586F65C08107706091C88278D089EEA5BFB556DB39'
$ProjectDeclarationSha256 =
    'FBBBA940F04E558D73AE3935CFAD167AEB87BE52E705442987AF9E1795949169'
$ProjectDeclarationDeltaOffset = 39
$AllowedTrackedDrift = @(
    $DiagnosticsRelativePath,
    $TcpRelativePath,
    $ClassesRelativePath,
    $ProjectRelativePath
)

$ProtectedFiles = @(
    [ordered]@{
        Path = "$LasalRoot/Elmo_EtherCAT_Test_4Axis.lcp"
        Bytes = 25188
        Sha256 = 'C84DE0051F579AEDEEB203AB1491EB989DC8C14BEB9D722F0E8002865C957648'
    },
    [ordered]@{
        Path = "$LasalRoot/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st"
        Bytes = 23452
        Sha256 = '168346F705618E876E466A0762210C64EC7BA8EDAC32C0BBD8EC7DC4B881FCBF'
    },
    [ordered]@{
        Path = "$LasalRoot/Network/Comm_Network/Comm_Network.lcn"
        Bytes = 16387
        Sha256 = '4EFA35899443D8DFE10D3F9974493056CAE6E103751AF6B9A408338077A8C0DA'
    },
    [ordered]@{
        Path = "$LasalRoot/Network/Comm_Network/ONE_Comm_Network_Table.st"
        Bytes = 11828
        Sha256 = '752C2873FBE8D1470A82E4E4A651DEC298567B42625EA69EE8F2F2C85514E373'
    },
    [ordered]@{
        Path = "$LasalRoot/Network/Networks.lcb"
        Bytes = 242267
        Sha256 = '755F59127516637DE9568A6352846AF10892465808A322FBF0B3DDEE09A2B6AA'
    },
    [ordered]@{
        Path = "$LasalRoot/Network/ConfigObjects.st"
        Bytes = 8791
        Sha256 = '96AD3639F7984D5F0E7E69344B39C8CACEE407DBA06E6876EB5DA888C476F919'
    },
    [ordered]@{
        Path = "$LasalRoot/Include/C_channels.h"
        Bytes = 25286
        Sha256 = 'D4184CAFF23D15DF68DC2B4B9F44FDCC96BD6DB50B71A6D40C02A711ABDF4EA3'
    },
    [ordered]@{
        Path = "$LasalRoot/Include/C_global.h"
        Bytes = 241
        Sha256 = '7B1F785348A2CF59FA2BD6496CE800D4BFF89B8CE239C7EF447213133DDC4D81'
    },
    [ordered]@{
        Path = "$LasalRoot/Include/C_types.h"
        Bytes = 96914
        Sha256 = '1F774C3C8DBBA3191A6C2ECBF3D727AD47CA452F90C8EE6412C5B6D68F7D2754'
    },
    [ordered]@{
        Path = "$LasalRoot/Include/UserDef.h"
        Bytes = 55
        Sha256 = 'A08B90B0F07D27509585CD28DCD10A5F94CC8F09CBD91256A883122C42B18A2B'
    },
    [ordered]@{
        Path = "$LasalRoot/Include/channels.h"
        Bytes = 21385
        Sha256 = '60B754390CD0078A5B3FBFFE24955A99EB7C003361F0937B26D53214E20EFB7F'
    },
    [ordered]@{
        Path = "$LasalRoot/Include/global.h"
        Bytes = 1586
        Sha256 = '69693216B85B44E3853B61922B7947BD65D8A4384F364B381531E67E611E1B02'
    },
    [ordered]@{
        Path = "$LasalRoot/Include/lslpublictypes.h"
        Bytes = 64423
        Sha256 = 'EDD3F17794126577E17A841AD1D562F1CB6D41D03D7C7A7E3690146918E029AD'
    },
    [ordered]@{
        Path = "$LasalRoot/Include/types.h"
        Bytes = 192021
        Sha256 = '95E7979138912505E01CDC454F64D7575E16F257EAD48034060E5850D048F5C2'
    },
    [ordered]@{
        Path = "$LasalRoot/Include/unit.h"
        Bytes = 763
        Sha256 = '2298895E6F5189165DC6E411AF62DC31A3B2597B431054268FB0E3DE3C251FA2'
    }
)

$DiagnosticsVariableNames = @(
    'D5TerminalWakeLastAttemptTicketId',
    'D5TerminalWakeLastAttemptTicketBootId',
    'D5TerminalWakeLastAttemptOwnerSessionEpoch'
)
$TcpVariableNames = @(
    'D5TerminalWakeAttemptCount',
    'D5TerminalWakeEnqueuedCount',
    'D5TerminalWakeRejectedCount'
)
$DiagnosticsMethodSpec = [ordered]@{
    Name = 'TryTakeD5TerminalWake'
    IsGlobal = $true
    Inputs = @(
        'pTicketId:^UDINT',
        'pTicketBootId:^UDINT',
        'pOwnerSessionEpoch:^UDINT'
    )
    Outputs = @('Result:DINT')
}
$TcpMethodSpec = [ordered]@{
    Name = 'PublishD5TerminalWake'
    IsGlobal = $false
    Inputs = @()
    Outputs = @()
}
$ExpectedDiagnosticsMethodInventory = @(
    'HandleRequest',
    'NotifySessionClosed',
    'ProcessOperations',
    'TryTakeD5TerminalWake',
    'IsSdoReadReady',
    'GetSdoWritePolicyDetail',
    'GetDiagnosticsBootId',
    'BuildCatalogEntry',
    'HandleEtherCATTopologyIoRequest',
    'HandleAxisDs402HomeStart',
    'HandleAxisDs402HomeOutcome',
    'HandleAxisDs402HomeRetire',
    'ProcessAxisDs402Home',
    'HandleDiagnosticsCapabilities',
    'HandleEncoderMaintenanceStart',
    'HandleEncoderMaintenanceOutcome',
    'HandleEncoderMaintenanceRetire',
    'ProcessEncoderMaintenance',
    'HandleDiagnosticsBulkRequest',
    'ProcessAxisOwnershipStartup',
    'HandleEncoderMaintenancePreemption',
    'HandleAxisDs402HomeReceiptStages',
    'HandleAxisDs402HomeCleanupStages'
)
$ExpectedTcpMethodInventory = @(
    'CyWork',
    'DataHandling',
    'Response',
    'SendData',
    'ConnSocketInfo',
    'MsgPaser',
    'HandleControlSafetyDrainPending',
    'HandleRpcLifecycleCommands',
    'DisarmRpcCallbackEndpoint',
    'PublishD5TerminalWake'
)
function Throw-GateDBlocker {
    param([Parameter(Mandatory = $true)][string]$Message)

    throw "$Owner blocker: $Message"
}

function Get-Sha256Hex {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes
    )

    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString(
                $sha256.ComputeHash($Bytes))).Replace('-', '')
    }
    finally {
        $sha256.Dispose()
    }
}

function ConvertTo-CanonicalLf {
    param([Parameter(Mandatory = $true)][string]$Text)

    return $Text.Replace("`r`n", "`n").Replace("`r", "`n")
}

function Get-StrictAsciiText {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$FileOwner,
        [switch]$RequireCrLf
    )

    if (($Bytes.Count -ge 3) -and
        ($Bytes[0] -eq 0xEF) -and
        ($Bytes[1] -eq 0xBB) -and
        ($Bytes[2] -eq 0xBF)) {
        Throw-GateDBlocker "$FileOwner has a UTF-8 BOM."
    }
    foreach ($value in $Bytes) {
        if ($value -gt 0x7F) {
            Throw-GateDBlocker "$FileOwner contains a non-ASCII byte."
        }
    }
    $text = [Text.Encoding]::ASCII.GetString($Bytes)
    if ($RequireCrLf) {
        $withoutCrLf = $text.Replace("`r`n", '')
        if (($withoutCrLf.IndexOf("`r", [StringComparison]::Ordinal) -ge 0) -or
            ($withoutCrLf.IndexOf("`n", [StringComparison]::Ordinal) -ge 0)) {
            Throw-GateDBlocker "$FileOwner has mixed or non-CRLF line endings."
        }
    }
    return $text
}

function Assert-ExactInventory {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Actual,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Expected,
        [Parameter(Mandatory = $true)][string]$InventoryOwner
    )

    if ([string]::Join('|', $Actual) -cne [string]::Join('|', $Expected)) {
        Throw-GateDBlocker (
            "$InventoryOwner is '$([string]::Join('|', $Actual))', expected " +
            "'$([string]::Join('|', $Expected))'.")
    }
}

function Get-OrdinalCount {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Needle
    )

    $count = 0
    $cursor = 0
    while ($cursor -lt $Text.Length) {
        $found = $Text.IndexOf($Needle, $cursor, [StringComparison]::Ordinal)
        if ($found -lt 0) {
            break
        }
        $count++
        $cursor = $found + $Needle.Length
    }
    return $count
}

function Get-DeclarationRegion {
    param([Parameter(Mandatory = $true)][string]$Text)

    $startMarker = '//{{LSL_DECLARATION'
    $endMarker = '//}}LSL_DECLARATION'
    $start = $Text.IndexOf($startMarker, [StringComparison]::Ordinal)
    $end = $Text.IndexOf($endMarker, [StringComparison]::Ordinal)
    if (($start -lt 0) -or ($end -le $start) -or
        ((Get-OrdinalCount -Text $Text -Needle $startMarker) -ne 1) -or
        ((Get-OrdinalCount -Text $Text -Needle $endMarker) -ne 1)) {
        Throw-GateDBlocker 'source declaration markers are not exact one pair.'
    }
    return $Text.Substring($start, $end + $endMarker.Length - $start)
}

function Get-DeclarationFunctionRecords {
    param([Parameter(Mandatory = $true)][string]$DeclarationText)

    $pattern = [regex]::new(
        '(?im)^[ \t]*FUNCTION[ \t]+' +
        '(?<Modifiers>(?:(?:VIRTUAL|GLOBAL)[ \t]+)*)' +
        '(?<Name>[A-Za-z_][A-Za-z0-9_]*)\b')
    $matches = @($pattern.Matches($DeclarationText))
    $records = [Collections.Generic.List[object]]::new()
    for ($index = 0; $index -lt $matches.Count; $index++) {
        $match = $matches[$index]
        $name = $match.Groups['Name'].Value
        if ($name -ceq 'TAB') {
            continue
        }
        $boundary = [regex]::new(
            '(?im)^[ \t]*(?:FUNCTION\b|END_CLASS[ \t]*;)').Match(
            $DeclarationText,
            $match.Index + $match.Length)
        if (-not $boundary.Success) {
            Throw-GateDBlocker "$name declaration has no bounded successor."
        }
        $records.Add([pscustomobject]@{
                Name = $name
                Modifiers = $match.Groups['Modifiers'].Value.Trim()
                Start = $match.Index
                End = $boundary.Index
                Block = $DeclarationText.Substring(
                    $match.Index,
                    $boundary.Index - $match.Index)
            })
    }
    return $records.ToArray()
}

function Get-ImplementationFunctionRecords {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$ClassName
    )

    $pattern = [regex]::new(
        '(?im)^[ \t]*FUNCTION[ \t]+' +
        '(?<Modifiers>(?:(?:VIRTUAL|GLOBAL)[ \t]+)*)' +
        [regex]::Escape($ClassName) +
        '::(?<Name>[A-Za-z_][A-Za-z0-9_]*)\b')
    $matches = @($pattern.Matches($Text))
    $records = [Collections.Generic.List[object]]::new()
    foreach ($match in $matches) {
        $name = $match.Groups['Name'].Value
        if ($name -ceq 'TAB') {
            continue
        }
        $end = [regex]::new(
            '(?im)^[ \t]*END_FUNCTION[ \t]*$').Match(
            $Text,
            $match.Index + $match.Length)
        if (-not $end.Success) {
            Throw-GateDBlocker "$ClassName::$name has no END_FUNCTION."
        }
        $records.Add([pscustomobject]@{
                Name = $name
                Modifiers = $match.Groups['Modifiers'].Value.Trim()
                Start = $match.Index
                End = $end.Index + $end.Length
                Block = $Text.Substring(
                    $match.Index,
                    $end.Index + $end.Length - $match.Index)
            })
    }
    return $records.ToArray()
}

function Get-FunctionVariableInventory {
    param(
        [Parameter(Mandatory = $true)][string]$FunctionBlock,
        [Parameter(Mandatory = $true)]
        [ValidateSet('VAR_INPUT', 'VAR_OUTPUT')]
        [string]$Section,
        [Parameter(Mandatory = $true)][string]$FunctionOwner
    )

    $matches = @([regex]::Matches(
            $FunctionBlock,
            "(?ims)^[ \t]*$Section[ \t]*`$" +
                '(?<Body>.*?)^[ \t]*END_VAR[ \t]*;?[ \t]*$'))
    if ($matches.Count -ne 1) {
        Throw-GateDBlocker (
            "$FunctionOwner $Section count is $($matches.Count), expected 1.")
    }
    $inventory = [Collections.Generic.List[string]]::new()
    foreach ($line in $matches[0].Groups['Body'].Value -split '\r?\n') {
        $trimmed = $line.Trim()
        if ($trimmed.Length -eq 0) {
            continue
        }
        $variable = [regex]::Match(
            $trimmed,
            '^(?<Name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*:[ \t]*' +
                '(?<Type>[^;]+?)[ \t]*;[ \t]*$')
        if (-not $variable.Success) {
            Throw-GateDBlocker (
                "$FunctionOwner $Section has an invalid line: $trimmed")
        }
        $type = [regex]::Replace(
            $variable.Groups['Type'].Value.Trim(),
            '[ \t]+',
            '')
        $inventory.Add($variable.Groups['Name'].Value + ':' + $type)
    }
    return $inventory.ToArray()
}

function Get-FunctionExecutableText {
    param([Parameter(Mandatory = $true)][string]$FunctionBlock)

    $body = ConvertTo-CanonicalLf -Text $FunctionBlock
    $lineEnd = $body.IndexOf("`n", [StringComparison]::Ordinal)
    if ($lineEnd -lt 0) {
        Throw-GateDBlocker 'function block has no implementation body.'
    }
    $body = $body.Substring($lineEnd + 1)
    while ($true) {
        $section = [regex]::Match(
            $body,
            '(?ims)\A[ \t]*VAR(?:_INPUT|_OUTPUT)?[ \t]*\n' +
                '.*?^[ \t]*END_VAR[ \t]*;?[ \t]*(?:\n|\z)')
        if (-not $section.Success) {
            break
        }
        $body = $body.Substring($section.Length)
    }
    $body = [regex]::Replace(
        $body,
        '(?im)^[ \t]*END_FUNCTION[ \t]*\z',
        '')
    return $body.Trim()
}

function Assert-SourceMethodAbi {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Record,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Spec,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$ExpectedModifiers,
        [switch]$Implementation
    )

    if ($Record.Modifiers -cne $ExpectedModifiers) {
        Throw-GateDBlocker (
            "$($Spec.Name) modifiers are '$($Record.Modifiers)', expected " +
            "'$ExpectedModifiers'.")
    }
    foreach ($section in @(
            @{ Name = 'VAR_INPUT'; Values = @($Spec.Inputs) },
            @{ Name = 'VAR_OUTPUT'; Values = @($Spec.Outputs) })) {
        $hasSection = $Record.Block -match
            "(?im)^[ \t]*$($section.Name)[ \t]*$"
        if ($section.Values.Count -eq 0) {
            if ($hasSection) {
                Throw-GateDBlocker (
                    "$($Spec.Name) has unexpected $($section.Name).")
            }
        }
        else {
            if (-not $hasSection) {
                Throw-GateDBlocker (
                    "$($Spec.Name) is missing $($section.Name).")
            }
            Assert-ExactInventory `
                -Actual @(Get-FunctionVariableInventory `
                    -FunctionBlock $Record.Block `
                    -Section $section.Name `
                    -FunctionOwner $Spec.Name) `
                -Expected @($section.Values) `
                -InventoryOwner "$($Spec.Name) $($section.Name) ABI"
        }
    }
    if ($Implementation -and
        ((Get-FunctionExecutableText -FunctionBlock $Record.Block).Length -ne 0)) {
        Throw-GateDBlocker "$($Spec.Name) implementation body is not empty."
    }
}

function Assert-ExactInsertedVariableSpan {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$BeforeName,
        [Parameter(Mandatory = $true)][string]$AfterName,
        [Parameter(Mandatory = $true)][string[]]$ExpectedNames,
        [Parameter(Mandatory = $true)][string]$SpanOwner
    )

    $before = [regex]::Match(
        $Text,
        '(?m)^[ \t]*' + [regex]::Escape($BeforeName) +
            '[ \t]*:[^\r\n;]+;[ \t]*$')
    $after = [regex]::Match(
        $Text,
        '(?m)^[ \t]*' + [regex]::Escape($AfterName) +
            '[ \t]*:[^\r\n;]+;[ \t]*$')
    if ((-not $before.Success) -or (-not $after.Success) -or
        ($after.Index -le $before.Index)) {
        Throw-GateDBlocker "$SpanOwner variable anchors are not ordered."
    }
    $beforeEnd = $before.Index + $before.Length
    $middle = $Text.Substring($beforeEnd, $after.Index - $beforeEnd)
    $lines = @($middle -split '\r?\n' | Where-Object {
            -not [string]::IsNullOrWhiteSpace($_)
        })
    $actual = [Collections.Generic.List[string]]::new()
    foreach ($line in $lines) {
        $match = [regex]::Match(
            $line,
            '^[ \t]*(?<Name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*' +
                ':[ \t]*(?<Type>[A-Za-z_][A-Za-z0-9_]*)[ \t]*;[ \t]*$')
        if (-not $match.Success) {
            Throw-GateDBlocker (
                "$SpanOwner contains a comment, initializer, or invalid declaration.")
        }
        if ($match.Groups['Type'].Value -cne 'UDINT') {
            Throw-GateDBlocker (
                "$SpanOwner $($match.Groups['Name'].Value) is not UDINT.")
        }
        $actual.Add($match.Groups['Name'].Value)
    }
    Assert-ExactInventory `
        -Actual $actual.ToArray() `
        -Expected $ExpectedNames `
        -InventoryOwner "$SpanOwner variable order"
}

function Get-UniqueRecord {
    param(
        [Parameter(Mandatory = $true)][object[]]$Records,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $matches = @($Records | Where-Object { $_.Name -ceq $Name })
    if ($matches.Count -ne 1) {
        Throw-GateDBlocker (
            "$RecordOwner count is $($matches.Count), expected 1.")
    }
    return $matches[0]
}

function Assert-TableBaseline {
    param(
        [Parameter(Mandatory = $true)][string]$Current,
        [Parameter(Mandatory = $true)][string]$Baseline,
        [Parameter(Mandatory = $true)][string]$ClassName,
        [Parameter(Mandatory = $true)][int]$UserCount
    )

    $definePattern = '(?m)^#define[ \t]+USER_CNT_' +
        [regex]::Escape($ClassName) + '[ \t]+' + $UserCount + '[ \t]*$'
    if (([regex]::Matches($Current, $definePattern).Count -ne 1) -or
        ([regex]::Matches(
            $Current,
            '(?m)^FUNCTION GLOBAL TAB ' + [regex]::Escape($ClassName) +
                '::@CT_[ \t]*$').Count -ne 1)) {
        Throw-GateDBlocker "$ClassName generated @CT_/USER_CNT contract drifted."
    }
    $currentStart = $Current.IndexOf(
        "FUNCTION GLOBAL TAB $ClassName::@CT_",
        [StringComparison]::Ordinal)
    $baselineStart = $Baseline.IndexOf(
        "FUNCTION GLOBAL TAB $ClassName::@CT_",
        [StringComparison]::Ordinal)
    $currentEnd = $Current.IndexOf(
        "//{{LSL_IMPLEMENTATION",
        $currentStart,
        [StringComparison]::Ordinal)
    $baselineEnd = $Baseline.IndexOf(
        "//{{LSL_IMPLEMENTATION",
        $baselineStart,
        [StringComparison]::Ordinal)
    if (($currentStart -lt 0) -or ($baselineStart -lt 0) -or
        ($currentEnd -le $currentStart) -or
        ($baselineEnd -le $baselineStart)) {
        Throw-GateDBlocker "$ClassName generated table span is not bounded."
    }
    $currentTable = $Current.Substring($currentStart, $currentEnd - $currentStart)
    $baselineTable = $Baseline.Substring(
        $baselineStart,
        $baselineEnd - $baselineStart)
    if ($currentTable -cne $baselineTable) {
        Throw-GateDBlocker "$ClassName generated @CT_ or @STD bytes drifted."
    }
}

function Replace-Span {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][int]$Start,
        [Parameter(Mandatory = $true)][int]$End,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Replacement,
        [Parameter(Mandatory = $true)][string]$SpanOwner
    )

    if (($Start -lt 0) -or ($End -lt $Start) -or ($End -gt $Text.Length)) {
        Throw-GateDBlocker "$SpanOwner reverse-delta bounds are invalid."
    }
    return $Text.Substring(0, $Start) + $Replacement + $Text.Substring($End)
}

function Get-LineEnd {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][int]$LineStart
    )

    $end = $Text.IndexOf("`n", $LineStart, [StringComparison]::Ordinal)
    if ($end -lt 0) {
        return $Text.Length
    }
    return $end
}

function Get-VariableLineSpan {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$BeforeName,
        [Parameter(Mandatory = $true)][string]$AfterName
    )

    $before = [regex]::Match(
        $Text,
        '(?m)^[ \t]*' + [regex]::Escape($BeforeName) +
            '[ \t]*:[^\n;]+;[ \t]*$')
    $after = [regex]::Match(
        $Text,
        '(?m)^[ \t]*' + [regex]::Escape($AfterName) +
            '[ \t]*:[^\n;]+;[ \t]*$')
    if ((-not $before.Success) -or (-not $after.Success) -or
        ($after.Index -le $before.Index)) {
        Throw-GateDBlocker 'variable reverse-delta anchors drifted.'
    }
    return [pscustomobject]@{
        Start = Get-LineEnd -Text $Text -LineStart $before.Index
        End = $after.Index
    }
}

function Get-BaselineMiddle {
    param(
        [Parameter(Mandatory = $true)][string]$Baseline,
        [Parameter(Mandatory = $true)][string]$BeforeNeedle,
        [Parameter(Mandatory = $true)][string]$AfterNeedle
    )

    $before = $Baseline.IndexOf($BeforeNeedle, [StringComparison]::Ordinal)
    $after = $Baseline.IndexOf(
        $AfterNeedle,
        $before + $BeforeNeedle.Length,
        [StringComparison]::Ordinal)
    if (($before -lt 0) -or ($after -lt 0)) {
        Throw-GateDBlocker 'baseline reverse-delta anchors drifted.'
    }
    return $Baseline.Substring(
        $before + $BeforeNeedle.Length,
        $after - ($before + $BeforeNeedle.Length))
}

function Apply-ReverseSpans {
    param(
        [Parameter(Mandatory = $true)][string]$Current,
        [Parameter(Mandatory = $true)][string]$Baseline,
        [Parameter(Mandatory = $true)][object[]]$Spans,
        [Parameter(Mandatory = $true)][string]$SourceOwner
    )

    $result = $Current
    foreach ($span in @($Spans | Sort-Object CurrentStart -Descending)) {
        $replacement = $Baseline.Substring(
            $span.BaselineStart,
            $span.BaselineEnd - $span.BaselineStart)
        $result = Replace-Span `
            -Text $result `
            -Start $span.CurrentStart `
            -End $span.CurrentEnd `
            -Replacement $replacement `
            -SpanOwner "$SourceOwner $($span.Name)"
    }
    return $result
}

function Assert-GateDSourceContract {
    param(
        [Parameter(Mandatory = $true)][string]$CurrentText,
        [Parameter(Mandatory = $true)][string]$BaselineText,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Diagnostics', 'Tcp')]
        [string]$Kind
    )

    $current = ConvertTo-CanonicalLf -Text $CurrentText
    $baseline = ConvertTo-CanonicalLf -Text $BaselineText
    $className = if ($Kind -ceq 'Diagnostics') {
        'LMCDiagnosticsService'
    }
    else {
        'TCPMotionInterface'
    }
    $variables = if ($Kind -ceq 'Diagnostics') {
        $DiagnosticsVariableNames
    }
    else {
        $TcpVariableNames
    }
    $methodSpec = if ($Kind -ceq 'Diagnostics') {
        $DiagnosticsMethodSpec
    }
    else {
        $TcpMethodSpec
    }
    $expectedModifiers = if ($methodSpec.IsGlobal) { 'GLOBAL' } else { '' }
    $beforeVariable = if ($Kind -ceq 'Diagnostics') {
        'BootIdFault'
    }
    else {
        'RpcCallbackLastDisarmResult'
    }
    $afterVariable = if ($Kind -ceq 'Diagnostics') {
        'Ds402HomeState'
    }
    else {
        'lsl_tcp_user'
    }

    foreach ($name in $variables) {
        if ((Get-OrdinalCount -Text $current -Needle $name) -ne 1) {
            Throw-GateDBlocker "$className source $name count is not 1."
        }
    }
    if ((Get-OrdinalCount -Text $current -Needle $methodSpec.Name) -ne 2) {
        Throw-GateDBlocker "$className source $($methodSpec.Name) count is not 2."
    }
    Assert-ExactInsertedVariableSpan `
        -Text (Get-DeclarationRegion -Text $current) `
        -BeforeName $beforeVariable `
        -AfterName $afterVariable `
        -ExpectedNames $variables `
        -SpanOwner "$className declaration"

    $currentDeclaration = Get-DeclarationRegion -Text $current
    $baselineDeclaration = Get-DeclarationRegion -Text $baseline
    $currentDeclarations = @(Get-DeclarationFunctionRecords `
            -DeclarationText $currentDeclaration)
    $baselineDeclarations = @(Get-DeclarationFunctionRecords `
            -DeclarationText $baselineDeclaration)
    $declaration = Get-UniqueRecord `
        -Records $currentDeclarations `
        -Name $methodSpec.Name `
        -RecordOwner "$className $($methodSpec.Name) declaration"
    Assert-SourceMethodAbi `
        -Record $declaration `
        -Spec $methodSpec `
        -ExpectedModifiers $expectedModifiers

    $currentImplementations = @(Get-ImplementationFunctionRecords `
            -Text $current -ClassName $className)
    $baselineImplementations = @(Get-ImplementationFunctionRecords `
            -Text $baseline -ClassName $className)
    $implementation = Get-UniqueRecord `
        -Records $currentImplementations `
        -Name $methodSpec.Name `
        -RecordOwner "$className $($methodSpec.Name) implementation"
    Assert-SourceMethodAbi `
        -Record $implementation `
        -Spec $methodSpec `
        -ExpectedModifiers $expectedModifiers `
        -Implementation

    $currentVariableSpan = Get-VariableLineSpan `
        -Text $current `
        -BeforeName $beforeVariable `
        -AfterName $afterVariable
    $baselineVariableSpan = Get-VariableLineSpan `
        -Text $baseline `
        -BeforeName $beforeVariable `
        -AfterName $afterVariable
    $spans = [Collections.Generic.List[object]]::new()
    $spans.Add([pscustomobject]@{
            Name = 'variable declarations'
            CurrentStart = $currentVariableSpan.Start
            CurrentEnd = $currentVariableSpan.End
            BaselineStart = $baselineVariableSpan.Start
            BaselineEnd = $baselineVariableSpan.End
        })

    $currentDeclarationStart = $current.IndexOf(
        $currentDeclaration,
        [StringComparison]::Ordinal)
    $baselineDeclarationStart = $baseline.IndexOf(
        $baselineDeclaration,
        [StringComparison]::Ordinal)
    if ($Kind -ceq 'Diagnostics') {
        $currentBefore = Get-UniqueRecord `
            -Records $currentDeclarations `
            -Name 'ProcessOperations' `
            -RecordOwner 'Diagnostics ProcessOperations declaration'
        $currentAfter = Get-UniqueRecord `
            -Records $currentDeclarations `
            -Name 'IsSdoReadReady' `
            -RecordOwner 'Diagnostics IsSdoReadReady declaration'
        $baselineBefore = Get-UniqueRecord `
            -Records $baselineDeclarations `
            -Name 'ProcessOperations' `
            -RecordOwner 'baseline Diagnostics ProcessOperations declaration'
        $baselineAfter = Get-UniqueRecord `
            -Records $baselineDeclarations `
            -Name 'IsSdoReadReady' `
            -RecordOwner 'baseline Diagnostics IsSdoReadReady declaration'
        $spans.Add([pscustomobject]@{
                Name = 'method declaration'
                CurrentStart = $currentDeclarationStart + $currentBefore.End
                CurrentEnd = $currentDeclarationStart + $currentAfter.Start
                BaselineStart = $baselineDeclarationStart + $baselineBefore.End
                BaselineEnd = $baselineDeclarationStart + $baselineAfter.Start
            })

        $baselineBeforeImpl = @(
            $baselineImplementations | Sort-Object Start)[-1]
        $currentBeforeImpl = Get-UniqueRecord `
            -Records $currentImplementations `
            -Name $baselineBeforeImpl.Name `
            -RecordOwner 'Diagnostics final baseline implementation'
        $spans.Add([pscustomobject]@{
                Name = 'method implementation'
                CurrentStart = $currentBeforeImpl.End
                CurrentEnd = $current.Length
                BaselineStart = $baselineBeforeImpl.End
                BaselineEnd = $baseline.Length
            })
        Assert-TableBaseline `
            -Current $current `
            -Baseline $baseline `
            -ClassName $className `
            -UserCount 0
    }
    else {
        $currentBefore = Get-UniqueRecord `
            -Records $currentDeclarations `
            -Name 'DisarmRpcCallbackEndpoint' `
            -RecordOwner 'TCP Disarm declaration'
        $baselineBefore = Get-UniqueRecord `
            -Records $baselineDeclarations `
            -Name 'DisarmRpcCallbackEndpoint' `
            -RecordOwner 'baseline TCP Disarm declaration'
        $currentDisarmEnd = $currentBefore.Block.LastIndexOf(
            'END_VAR;',
            [StringComparison]::Ordinal)
        $baselineDisarmEnd = $baselineBefore.Block.LastIndexOf(
            'END_VAR;',
            [StringComparison]::Ordinal)
        if (($currentDisarmEnd -lt 0) -or ($baselineDisarmEnd -lt 0)) {
            Throw-GateDBlocker 'TCP Disarm declaration output boundary drifted.'
        }
        $currentDisarmEnd += $currentBefore.Start + 'END_VAR;'.Length
        $baselineDisarmEnd += $baselineBefore.Start + 'END_VAR;'.Length
        $currentTables = $currentDeclaration.IndexOf(
            '  //Tables:',
            $currentDisarmEnd,
            [StringComparison]::Ordinal)
        $baselineTables = $baselineDeclaration.IndexOf(
            '  //Tables:',
            $baselineDisarmEnd,
            [StringComparison]::Ordinal)
        if (($currentTables -lt 0) -or ($baselineTables -lt 0)) {
            Throw-GateDBlocker 'TCP declaration table boundary drifted.'
        }
        $spans.Add([pscustomobject]@{
                Name = 'method declaration'
                CurrentStart = $currentDeclarationStart + $currentDisarmEnd
                CurrentEnd = $currentDeclarationStart + $currentTables
                BaselineStart = $baselineDeclarationStart + $baselineDisarmEnd
                BaselineEnd = $baselineDeclarationStart + $baselineTables
            })

        $currentBeforeImpl = Get-UniqueRecord `
            -Records $currentImplementations `
            -Name 'DisarmRpcCallbackEndpoint' `
            -RecordOwner 'TCP Disarm implementation'
        $baselineBeforeImpl = Get-UniqueRecord `
            -Records $baselineImplementations `
            -Name 'DisarmRpcCallbackEndpoint' `
            -RecordOwner 'baseline TCP Disarm implementation'
        $spans.Add([pscustomobject]@{
                Name = 'method implementation'
                CurrentStart = $currentBeforeImpl.End
                CurrentEnd = $current.Length
                BaselineStart = $baselineBeforeImpl.End
                BaselineEnd = $baseline.Length
            })
        Assert-TableBaseline `
            -Current $current `
            -Baseline $baseline `
            -ClassName $className `
            -UserCount 10
    }

    $reversed = Apply-ReverseSpans `
        -Current $current `
        -Baseline $baseline `
        -Spans $spans.ToArray() `
        -SourceOwner $className
    if ($reversed -cne $baseline) {
        Throw-GateDBlocker (
            "$className source has drift outside the exact Gate D declaration spans.")
    }
}

function Test-ClassDatabaseByteSequence {
    param(
        [Parameter(Mandatory = $true)][byte[]]$DatabaseBytes,
        [Parameter(Mandatory = $true)][int]$Start,
        [Parameter(Mandatory = $true)][byte[]]$ExpectedBytes
    )

    if (($Start -lt 0) -or
        (($Start + $ExpectedBytes.Count) -gt $DatabaseBytes.Count)) {
        return $false
    }
    for ($index = 0; $index -lt $ExpectedBytes.Count; $index++) {
        if ($DatabaseBytes[$Start + $index] -ne $ExpectedBytes[$index]) {
            return $false
        }
    }
    return $true
}

function Get-ClassDatabaseRecord {
    param(
        [Parameter(Mandatory = $true)][byte[]]$DatabaseBytes,
        [Parameter(Mandatory = $true)][string]$DatabaseText,
        [Parameter(Mandatory = $true)][string]$SourcePath,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    if ($DatabaseBytes.Count -ne $DatabaseText.Length) {
        Throw-GateDBlocker "$RecordOwner byte/text offsets diverged."
    }
    if ((Get-OrdinalCount -Text $DatabaseText -Needle $SourcePath) -ne 1) {
        Throw-GateDBlocker "$RecordOwner source path count is not 1."
    }
    $start = $DatabaseText.IndexOf($SourcePath, [StringComparison]::Ordinal)
    $end = $DatabaseText.IndexOf(
        '.\Class\',
        $start + $SourcePath.Length,
        [StringComparison]::Ordinal)
    if ($end -lt 0) {
        $end = $DatabaseText.Length
    }
    $length = $end - $start
    $bytes = [byte[]]::new($length)
    [Array]::Copy($DatabaseBytes, $start, $bytes, 0, $length)
    return [pscustomobject]@{
        Bytes = $bytes
        Text = $DatabaseText.Substring($start, $length)
    }
}

function Get-ClassDatabaseFunctionHeaderBytes {
    param(
        [Parameter(Mandatory = $true)][byte]$MethodKind,
        [Parameter(Mandatory = $true)][bool]$IsVirtual,
        [Parameter(Mandatory = $true)][bool]$IsGlobal,
        [Parameter(Mandatory = $true)][uint32]$InputCount
    )

    return ,([byte[]]@(
            $MethodKind, 0x00, 0x00, 0x00,
            [byte]$(if ($IsVirtual) { 1 } else { 0 }),
            [byte]$(if ($IsGlobal) { 1 } else { 0 }), 0x00, 0x00,
            [byte]($InputCount -band 0xFF),
            [byte](($InputCount -shr 8) -band 0xFF),
            [byte](($InputCount -shr 16) -band 0xFF),
            [byte](($InputCount -shr 24) -band 0xFF)))
}

function Get-ClassDatabaseMethodAbiInventory {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $inventory = [Collections.Generic.List[object]]::new()
    for ($prefixStart = 0;
         $prefixStart -le ($RecordBytes.Count - 18);
         $prefixStart++) {
        if (($RecordBytes[$prefixStart] -ne 0) -or
            ($RecordBytes[$prefixStart + 1] -ne 1) -or
            ($RecordBytes[$prefixStart + 5] -ne 0xAA)) {
            continue
        }
        $nameLength = [int]$RecordBytes[$prefixStart + 2] -bor
            ([int]$RecordBytes[$prefixStart + 3] -shl 8) -bor
            ([int]$RecordBytes[$prefixStart + 4] -shl 16)
        if (($nameLength -lt 1) -or ($nameLength -gt 128)) {
            continue
        }
        $nameStart = $prefixStart + 6
        $headerStart = $nameStart + $nameLength
        if (($headerStart + 12) -gt $RecordBytes.Count) {
            continue
        }
        $name = [Text.Encoding]::ASCII.GetString(
            $RecordBytes,
            $nameStart,
            $nameLength)
        if ($name -cnotmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
            continue
        }
        $methodKind = $RecordBytes[$headerStart]
        if (($methodKind -notin @([byte]0x05, [byte]0x0B)) -or
            ($RecordBytes[$headerStart + 1] -ne 0) -or
            ($RecordBytes[$headerStart + 2] -ne 0) -or
            ($RecordBytes[$headerStart + 3] -ne 0) -or
            ($RecordBytes[$headerStart + 4] -gt 1) -or
            ($RecordBytes[$headerStart + 5] -gt 1) -or
            ($RecordBytes[$headerStart + 6] -ne 0) -or
            ($RecordBytes[$headerStart + 7] -ne 0)) {
            continue
        }
        $inputCount = [BitConverter]::ToUInt32(
            $RecordBytes,
            $headerStart + 8)
        if ($inputCount -gt 64) {
            continue
        }
        $inventory.Add([pscustomobject]@{
                Name = $name
                PrefixStart = $prefixStart
                NameStart = $nameStart
                MethodKind = [byte]$methodKind
                IsVirtual = $RecordBytes[$headerStart + 4] -eq 1
                IsGlobal = $RecordBytes[$headerStart + 5] -eq 1
                InputCount = [uint32]$inputCount
            })
    }
    if ($inventory.Count -eq 0) {
        Throw-GateDBlocker "$RecordOwner has no bounded method ABI record."
    }
    return $inventory.ToArray()
}

function Read-ClassDatabaseAaString {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][int]$Cursor,
        [Parameter(Mandatory = $true)][int]$RecordEnd,
        [Parameter(Mandatory = $true)][int]$MaximumLength,
        [Parameter(Mandatory = $true)][string]$FieldOwner
    )

    if (($Cursor -lt 0) -or (($Cursor + 4) -gt $RecordEnd) -or
        ($RecordEnd -gt $RecordBytes.Count)) {
        Throw-GateDBlocker "$FieldOwner length prefix crosses its bounded record."
    }
    if ($RecordBytes[$Cursor + 3] -ne 0xAA) {
        Throw-GateDBlocker "$FieldOwner length prefix sentinel drifted."
    }
    $length = [int]$RecordBytes[$Cursor] -bor
        ([int]$RecordBytes[$Cursor + 1] -shl 8) -bor
        ([int]$RecordBytes[$Cursor + 2] -shl 16)
    if (($length -lt 0) -or ($length -gt $MaximumLength) -or
        (($Cursor + 4 + $length) -gt $RecordEnd)) {
        Throw-GateDBlocker "$FieldOwner length is outside its bounded record."
    }
    return [pscustomobject]@{
        Text = [Text.Encoding]::ASCII.GetString(
            $RecordBytes,
            $Cursor + 4,
            $length)
        Next = $Cursor + 4 + $length
    }
}

function Get-ClassDatabaseParameterTypeContract {
    param(
        [Parameter(Mandatory = $true)][string]$Type,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $pointer = $Type.StartsWith('^', [StringComparison]::Ordinal)
    $value = if ($pointer) { $Type.Substring(1) } else { $Type }
    if (($value.Length -lt 1) -or
        $value.StartsWith('^', [StringComparison]::Ordinal)) {
        Throw-GateDBlocker "$RecordOwner has an unsupported verifier type."
    }
    $separator = $value.LastIndexOf('::', [StringComparison]::Ordinal)
    $ownerName = ''
    $base = $value
    if ($separator -ge 0) {
        $ownerName = $value.Substring(0, $separator)
        $base = $value.Substring($separator + 2)
    }
    return [pscustomobject]@{
        Pointer = $pointer
        Base = $base
        Owner = $ownerName
    }
}

function Assert-ClassDatabaseParameterRecord {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][int]$Cursor,
        [Parameter(Mandatory = $true)][int]$RecordEnd,
        [Parameter(Mandatory = $true)][string]$Entry,
        [Parameter(Mandatory = $true)][bool]$IsOutput,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $separator = $Entry.IndexOf(':', [StringComparison]::Ordinal)
    if ($separator -lt 1) {
        Throw-GateDBlocker "$RecordOwner has an invalid parameter spec."
    }
    $name = $Entry.Substring(0, $separator)
    $type = Get-ClassDatabaseParameterTypeContract `
        -Type $Entry.Substring($separator + 1) `
        -RecordOwner "$RecordOwner $name"
    $nameLength = [uint32]$name.Length
    $namePrefix = [byte[]]@(
        0x00, 0x01,
        [byte]($nameLength -band 0xFF),
        [byte](($nameLength -shr 8) -band 0xFF),
        [byte](($nameLength -shr 16) -band 0xFF),
        0xAA)
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $Cursor `
                -ExpectedBytes $namePrefix)) {
        Throw-GateDBlocker "$RecordOwner parameter $name prefix drifted."
    }
    $cursorAfterName = $Cursor + $namePrefix.Count
    if (($cursorAfterName + $name.Length) -gt $RecordEnd) {
        Throw-GateDBlocker "$RecordOwner parameter $name crosses its record."
    }
    if ([Text.Encoding]::ASCII.GetString(
            $RecordBytes,
            $cursorAfterName,
            $name.Length) -cne $name) {
        Throw-GateDBlocker "$RecordOwner parameter $name name drifted."
    }
    $cursorAfterName += $name.Length
    $comment = Read-ClassDatabaseAaString `
        -RecordBytes $RecordBytes `
        -Cursor $cursorAfterName `
        -RecordEnd $RecordEnd `
        -MaximumLength 4096 `
        -FieldOwner "$RecordOwner parameter $name comment"
    if ($comment.Text.Length -ne 0) {
        Throw-GateDBlocker "$RecordOwner parameter $name comment is not blank."
    }
    $descriptor = [byte[]]@(
        0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA,
        0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF,
        0x01, 0x00, 0x00, 0x00, 0x00, 0xAA,
        0x00, 0xFF, 0xFF, 0xFF, 0xFF,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA,
        0x00, 0x00, 0x00, 0xAA,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00)
    if ($type.Pointer) {
        $descriptor[54] = 1
    }
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $comment.Next `
                -ExpectedBytes $descriptor)) {
        Throw-GateDBlocker "$RecordOwner parameter $name descriptor drifted."
    }
    $actualBase = Read-ClassDatabaseAaString `
        -RecordBytes $RecordBytes `
        -Cursor ($comment.Next + $descriptor.Count) `
        -RecordEnd $RecordEnd `
        -MaximumLength 255 `
        -FieldOwner "$RecordOwner parameter $name base type"
    if ($actualBase.Text -cne $type.Base) {
        Throw-GateDBlocker "$RecordOwner parameter $name base type drifted."
    }
    $actualOwner = Read-ClassDatabaseAaString `
        -RecordBytes $RecordBytes `
        -Cursor $actualBase.Next `
        -RecordEnd $RecordEnd `
        -MaximumLength 255 `
        -FieldOwner "$RecordOwner parameter $name type owner"
    if ($actualOwner.Text -cne $type.Owner) {
        Throw-GateDBlocker "$RecordOwner parameter $name type owner drifted."
    }
    $tail = [Collections.Generic.List[byte]]::new()
    foreach ($unused in 1..5) {
        foreach ($value in [byte[]]@(0, 0, 0, 0xAA)) {
            $tail.Add($value)
        }
    }
    foreach ($value in [byte[]]@(1, 0, 0, 0)) {
        $tail.Add($value)
    }
    foreach ($unused in 1..18) {
        $tail.Add(0)
    }
    $tail.Add(0xAA)
    foreach ($value in [byte[]]@(0xFF, 0xFF, 0xFF, 0xFF,
            [byte]$(if ($IsOutput) { 0 } else { 1 }))) {
        $tail.Add($value)
    }
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $actualOwner.Next `
                -ExpectedBytes $tail.ToArray())) {
        Throw-GateDBlocker "$RecordOwner parameter $name tail drifted."
    }
    $next = $actualOwner.Next + $tail.Count
    if ($next -gt $RecordEnd) {
        Throw-GateDBlocker "$RecordOwner parameter $name exceeds its method."
    }
    return $next
}

function Assert-ClassDatabaseFunctionAbiRecord {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][pscustomobject]$InventoryRecord,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Spec,
        [Parameter(Mandatory = $true)][int]$RecordEnd,
        [switch]$RequireExactEnd,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    if (($InventoryRecord.MethodKind -ne 0x0B) -or
        $InventoryRecord.IsVirtual -or
        ($InventoryRecord.IsGlobal -ne $Spec.IsGlobal) -or
        ($InventoryRecord.InputCount -ne [uint32]$Spec.Inputs.Count)) {
        Throw-GateDBlocker "$RecordOwner method-kind/header drifted."
    }
    $header = Get-ClassDatabaseFunctionHeaderBytes `
        -MethodKind 0x0B `
        -IsVirtual $false `
        -IsGlobal $Spec.IsGlobal `
        -InputCount ([uint32]$Spec.Inputs.Count)
    $cursor = $InventoryRecord.NameStart + $Spec.Name.Length + $header.Count
    foreach ($entry in $Spec.Inputs) {
        $cursor = Assert-ClassDatabaseParameterRecord `
            -RecordBytes $RecordBytes `
            -Cursor $cursor `
            -RecordEnd $RecordEnd `
            -Entry $entry `
            -IsOutput $false `
            -RecordOwner $RecordOwner
    }
    $outputCount = [uint32]$Spec.Outputs.Count
    $outputCountBytes = [byte[]]@(
        [byte]($outputCount -band 0xFF),
        [byte](($outputCount -shr 8) -band 0xFF),
        [byte](($outputCount -shr 16) -band 0xFF),
        [byte](($outputCount -shr 24) -band 0xFF))
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $cursor `
                -ExpectedBytes $outputCountBytes)) {
        Throw-GateDBlocker "$RecordOwner generated output count drifted."
    }
    $cursor += 4
    foreach ($entry in $Spec.Outputs) {
        $cursor = Assert-ClassDatabaseParameterRecord `
            -RecordBytes $RecordBytes `
            -Cursor $cursor `
            -RecordEnd $RecordEnd `
            -Entry $entry `
            -IsOutput $true `
            -RecordOwner $RecordOwner
    }
    $methodComment = Read-ClassDatabaseAaString `
        -RecordBytes $RecordBytes `
        -Cursor $cursor `
        -RecordEnd $RecordEnd `
        -MaximumLength 4096 `
        -FieldOwner "$RecordOwner method comment"
    if ($methodComment.Text.Length -ne 0) {
        Throw-GateDBlocker "$RecordOwner method comment is not blank."
    }
    $trailer = [byte[]]@(0, 0, 0, 0, 0, 0)
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $methodComment.Next `
                -ExpectedBytes $trailer)) {
        Throw-GateDBlocker "$RecordOwner method trailer drifted."
    }
    $cursor = $methodComment.Next + $trailer.Count
    if ($RequireExactEnd -and ($cursor -ne $RecordEnd)) {
        Throw-GateDBlocker "$RecordOwner parser did not consume its bounded chunk."
    }
    if ($cursor -gt $RecordEnd) {
        Throw-GateDBlocker "$RecordOwner parser crossed its bounded chunk."
    }
}

function Get-VariablePrefixStart {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][string]$RecordText,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $candidates = [Collections.Generic.List[int]]::new()
    $cursor = 0
    while ($cursor -lt $RecordText.Length) {
        $nameStart = $RecordText.IndexOf(
            $Name,
            $cursor,
            [StringComparison]::Ordinal)
        if ($nameStart -lt 0) {
            break
        }
        $prefixStart = $nameStart - 5
        if (($prefixStart -ge 0) -and
            ($RecordBytes[$prefixStart] -eq 1) -and
            ($RecordBytes[$prefixStart + 4] -eq 0xAA)) {
            $length = [int]$RecordBytes[$prefixStart + 1] -bor
                ([int]$RecordBytes[$prefixStart + 2] -shl 8) -bor
                ([int]$RecordBytes[$prefixStart + 3] -shl 16)
            if ($length -eq $Name.Length) {
                $candidates.Add($prefixStart)
            }
        }
        $cursor = $nameStart + $Name.Length
    }
    if ($candidates.Count -ne 1) {
        Throw-GateDBlocker (
            "$RecordOwner exact variable-name record count is " +
            "$($candidates.Count), expected 1.")
    }
    return $candidates[0]
}

function Get-PrivateUdintVariableMetadata {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][string]$RecordText,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][int]$NextVariableStart,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $start = Get-VariablePrefixStart `
        -RecordBytes $RecordBytes `
        -RecordText $RecordText `
        -Name $Name `
        -RecordOwner $RecordOwner
    $afterName = $start + 5 + $Name.Length
    $comment = Read-ClassDatabaseAaString `
        -RecordBytes $RecordBytes `
        -Cursor $afterName `
        -RecordEnd $NextVariableStart `
        -MaximumLength 4096 `
        -FieldOwner "$RecordOwner comment"
    if ($comment.Text.Length -ne 0) {
        Throw-GateDBlocker "$RecordOwner comment is not blank."
    }
    $privatePrefix = [byte[]]@(1, 0, 0, 0, 0, 0, 0)
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $comment.Next `
                -ExpectedBytes $privatePrefix)) {
        Throw-GateDBlocker "$RecordOwner private-variable prefix drifted."
    }
    $alternateName = Read-ClassDatabaseAaString `
        -RecordBytes $RecordBytes `
        -Cursor ($comment.Next + $privatePrefix.Count) `
        -RecordEnd $NextVariableStart `
        -MaximumLength 4096 `
        -FieldOwner "$RecordOwner alternate name"
    $trailerLength = 13
    $semanticStart = $alternateName.Next + 4
    $trailerStart = $NextVariableStart - $trailerLength
    if (($NextVariableStart -le $start) -or
        ($semanticStart -gt $trailerStart) -or
        (($trailerStart - $semanticStart) -ne 93)) {
        Throw-GateDBlocker "$RecordOwner metadata crosses its class record."
    }
    $typeRecord = [byte[]]@(
        5, 0, 0, 0xAA,
        [byte][char]'U', [byte][char]'D', [byte][char]'I',
        [byte][char]'N', [byte][char]'T')
    $typeCount = 0
    for ($cursor = $semanticStart;
         $cursor -le ($trailerStart - $typeRecord.Count);
         $cursor++) {
        if (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $cursor `
                -ExpectedBytes $typeRecord) {
            $typeCount++
        }
    }
    if ($typeCount -ne 1) {
        Throw-GateDBlocker "$RecordOwner UDINT type record count is $typeCount."
    }
    $trailerTail = [byte[]]@(
        0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0xAA)
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start ($trailerStart + 4) `
                -ExpectedBytes $trailerTail)) {
        Throw-GateDBlocker "$RecordOwner storage trailer drifted."
    }
    return [pscustomobject]@{
        Name = $Name
        Start = $start
        End = $NextVariableStart
        AlternateName = $alternateName.Text
    }
}

function Assert-ByteRangeEqual {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Left,
        [Parameter(Mandatory = $true)][int]$LeftStart,
        [Parameter(Mandatory = $true)][int]$LeftEnd,
        [Parameter(Mandatory = $true)][byte[]]$Right,
        [Parameter(Mandatory = $true)][int]$RightStart,
        [Parameter(Mandatory = $true)][int]$RightEnd,
        [Parameter(Mandatory = $true)][string]$RangeOwner
    )

    $leftLength = $LeftEnd - $LeftStart
    $rightLength = $RightEnd - $RightStart
    if (($leftLength -lt 0) -or ($rightLength -lt 0) -or
        ($leftLength -ne $rightLength)) {
        Throw-GateDBlocker "$RangeOwner byte range length drifted."
    }
    for ($index = 0; $index -lt $leftLength; $index++) {
        if ($Left[$LeftStart + $index] -ne $Right[$RightStart + $index]) {
            Throw-GateDBlocker "$RangeOwner bytes drifted."
        }
    }
}

function Assert-ClassesClassContract {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$CurrentRecord,
        [Parameter(Mandatory = $true)][pscustomobject]$BaselineRecord,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Diagnostics', 'Tcp')]
        [string]$Kind
    )

    $methodSpec = if ($Kind -ceq 'Diagnostics') {
        $DiagnosticsMethodSpec
    }
    else {
        $TcpMethodSpec
    }
    $variables = if ($Kind -ceq 'Diagnostics') {
        $DiagnosticsVariableNames
    }
    else {
        $TcpVariableNames
    }
    $expectedMethods = if ($Kind -ceq 'Diagnostics') {
        $ExpectedDiagnosticsMethodInventory
    }
    else {
        $ExpectedTcpMethodInventory
    }
    $anchorName = if ($Kind -ceq 'Diagnostics') {
        'BootIdFault'
    }
    else {
        'RpcCallbackLastDisarmResult'
    }
    $nextName = if ($Kind -ceq 'Diagnostics') {
        'Ds402HomeState'
    }
    else {
        'lsl_tcp_user'
    }
    $recordOwner = if ($Kind -ceq 'Diagnostics') {
        'LMCDiagnosticsService Classes.lcb'
    }
    else {
        'TCPMotionInterface Classes.lcb'
    }

    $currentMethods = @(Get-ClassDatabaseMethodAbiInventory `
            -RecordBytes $CurrentRecord.Bytes `
            -RecordOwner $recordOwner)
    $baselineMethods = @(Get-ClassDatabaseMethodAbiInventory `
            -RecordBytes $BaselineRecord.Bytes `
            -RecordOwner "baseline $recordOwner")
    Assert-ExactInventory `
        -Actual @($currentMethods.Name) `
        -Expected $expectedMethods `
        -InventoryOwner "$recordOwner bounded method inventory"
    Assert-ExactInventory `
        -Actual @($baselineMethods.Name) `
        -Expected @($expectedMethods | Where-Object { $_ -cne $methodSpec.Name }) `
        -InventoryOwner "baseline $recordOwner bounded method inventory"
    $methodIndex = [Array]::IndexOf([string[]]$currentMethods.Name, $methodSpec.Name)
    if ($methodIndex -lt 0) {
        Throw-GateDBlocker "$recordOwner new method inventory entry is missing."
    }
    $methodEnd = if (($methodIndex + 1) -lt $currentMethods.Count) {
        $currentMethods[$methodIndex + 1].PrefixStart
    }
    else {
        $CurrentRecord.Bytes.Count
    }
    Assert-ClassDatabaseFunctionAbiRecord `
        -RecordBytes $CurrentRecord.Bytes `
        -InventoryRecord $currentMethods[$methodIndex] `
        -Spec $methodSpec `
        -RecordEnd $methodEnd `
        -RequireExactEnd:(($methodIndex + 1) -lt $currentMethods.Count) `
        -RecordOwner "$recordOwner $($methodSpec.Name)"

    $anchorCurrent = Get-VariablePrefixStart `
        -RecordBytes $CurrentRecord.Bytes `
        -RecordText $CurrentRecord.Text `
        -Name $anchorName `
        -RecordOwner "$recordOwner $anchorName"
    $nextCurrent = Get-VariablePrefixStart `
        -RecordBytes $CurrentRecord.Bytes `
        -RecordText $CurrentRecord.Text `
        -Name $nextName `
        -RecordOwner "$recordOwner $nextName"
    $anchorBaseline = Get-VariablePrefixStart `
        -RecordBytes $BaselineRecord.Bytes `
        -RecordText $BaselineRecord.Text `
        -Name $anchorName `
        -RecordOwner "baseline $recordOwner $anchorName"
    $nextBaseline = Get-VariablePrefixStart `
        -RecordBytes $BaselineRecord.Bytes `
        -RecordText $BaselineRecord.Text `
        -Name $nextName `
        -RecordOwner "baseline $recordOwner $nextName"

    $variableStarts = [Collections.Generic.List[int]]::new()
    foreach ($name in $variables) {
        $variableStarts.Add((Get-VariablePrefixStart `
                -RecordBytes $CurrentRecord.Bytes `
                -RecordText $CurrentRecord.Text `
                -Name $name `
                -RecordOwner "$recordOwner $name"))
    }
    $metadata = [Collections.Generic.List[object]]::new()
    for ($index = 0; $index -lt $variables.Count; $index++) {
        $recordEnd = if (($index + 1) -lt $variables.Count) {
            $variableStarts[$index + 1]
        }
        else {
            $nextCurrent
        }
        $metadata.Add((Get-PrivateUdintVariableMetadata `
                -RecordBytes $CurrentRecord.Bytes `
                -RecordText $CurrentRecord.Text `
                -Name $variables[$index] `
                -NextVariableStart $recordEnd `
                -RecordOwner "$recordOwner $($variables[$index])"))
    }
    Assert-ByteRangeEqual `
        -Left $CurrentRecord.Bytes `
        -LeftStart $anchorCurrent `
        -LeftEnd $metadata[0].Start `
        -Right $BaselineRecord.Bytes `
        -RightStart $anchorBaseline `
        -RightEnd $nextBaseline `
        -RangeOwner "$recordOwner anchor metadata"
    for ($index = 1; $index -lt $metadata.Count; $index++) {
        if ($metadata[$index - 1].End -ne $metadata[$index].Start) {
            Throw-GateDBlocker "$recordOwner Gate D variable records are not contiguous."
        }
    }
    if ($metadata[$metadata.Count - 1].End -ne $nextCurrent) {
        Throw-GateDBlocker "$recordOwner Gate D variables are not immediately before $nextName."
    }
}

function Assert-ClassesDatabaseContract {
    param(
        [Parameter(Mandatory = $true)][byte[]]$CurrentBytes,
        [Parameter(Mandatory = $true)][byte[]]$BaselineBytes
    )

    if ((Get-Sha256Hex -Bytes $CurrentBytes) -ceq
        (Get-Sha256Hex -Bytes $BaselineBytes)) {
        Throw-GateDBlocker 'Classes.lcb did not change from Gate C.'
    }
    $latin1 = [Text.Encoding]::GetEncoding(28591)
    $currentText = $latin1.GetString($CurrentBytes)
    $baselineText = $latin1.GetString($BaselineBytes)
    foreach ($entry in @(
            [pscustomobject]@{
                Kind = 'Diagnostics'
                SourcePath = '.\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
            },
            [pscustomobject]@{
                Kind = 'Tcp'
                SourcePath = '.\Class\TCPMotionInterface\TCPMotionInterface.st'
            })) {
        $currentRecord = Get-ClassDatabaseRecord `
            -DatabaseBytes $CurrentBytes `
            -DatabaseText $currentText `
            -SourcePath $entry.SourcePath `
            -RecordOwner "$($entry.Kind) current Classes.lcb"
        $baselineRecord = Get-ClassDatabaseRecord `
            -DatabaseBytes $BaselineBytes `
            -DatabaseText $baselineText `
            -SourcePath $entry.SourcePath `
            -RecordOwner "$($entry.Kind) baseline Classes.lcb"
        Assert-ClassesClassContract `
            -CurrentRecord $currentRecord `
            -BaselineRecord $baselineRecord `
            -Kind $entry.Kind
    }
}

function Add-BytesToList {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)][byte[]]$Bytes
    )

    foreach ($value in $Bytes) {
        $List.Add($value)
    }
}

function Add-AsciiToList {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    if ($Text.Length -gt 0) {
        Add-BytesToList `
            -List $List `
            -Bytes ([Text.Encoding]::ASCII.GetBytes($Text))
    }
}

function Add-AaStringToList {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    $length = [uint32]$Text.Length
    Add-BytesToList -List $List -Bytes ([byte[]]@(
            [byte]($length -band 0xFF),
            [byte](($length -shr 8) -band 0xFF),
            [byte](($length -shr 16) -band 0xFF),
            0xAA))
    Add-AsciiToList -List $List -Text $Text
}

function Convert-HexStringToBytes {
    param([Parameter(Mandatory = $true)][string]$Hex)

    if (($Hex.Length % 2) -ne 0) {
        Throw-GateDBlocker 'synthetic hex fixture length is odd.'
    }
    $bytes = [byte[]]::new($Hex.Length / 2)
    for ($index = 0; $index -lt $bytes.Count; $index++) {
        $bytes[$index] = [Convert]::ToByte(
            $Hex.Substring($index * 2, 2),
            16)
    }
    return ,$bytes
}

function Add-SyntheticParameterMetadata {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)][string]$Entry,
        [Parameter(Mandatory = $true)][bool]$IsOutput
    )

    $separator = $Entry.IndexOf(':', [StringComparison]::Ordinal)
    $name = $Entry.Substring(0, $separator)
    $type = Get-ClassDatabaseParameterTypeContract `
        -Type $Entry.Substring($separator + 1) `
        -RecordOwner 'synthetic parameter'
    $length = [uint32]$name.Length
    Add-BytesToList -List $List -Bytes ([byte[]]@(
            0x00, 0x01,
            [byte]($length -band 0xFF),
            [byte](($length -shr 8) -band 0xFF),
            [byte](($length -shr 16) -band 0xFF),
            0xAA))
    Add-AsciiToList -List $List -Text $name
    Add-AaStringToList -List $List -Text ''
    $descriptor = [byte[]]@(
        0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA,
        0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF,
        0x01, 0x00, 0x00, 0x00, 0x00, 0xAA,
        0x00, 0xFF, 0xFF, 0xFF, 0xFF,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA,
        0x00, 0x00, 0x00, 0xAA,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00)
    if ($type.Pointer) {
        $descriptor[54] = 1
    }
    Add-BytesToList -List $List -Bytes $descriptor
    Add-AaStringToList -List $List -Text $type.Base
    Add-AaStringToList -List $List -Text $type.Owner
    foreach ($unused in 1..5) {
        Add-AaStringToList -List $List -Text ''
    }
    Add-BytesToList -List $List -Bytes ([byte[]]@(1, 0, 0, 0))
    Add-BytesToList -List $List -Bytes ([byte[]]::new(18))
    $List.Add(0xAA)
    Add-BytesToList -List $List -Bytes ([byte[]]@(
            0xFF, 0xFF, 0xFF, 0xFF,
            [byte]$(if ($IsOutput) { 0 } else { 1 })))
}

function New-SyntheticMethodMetadata {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [byte]$MethodKind = 0x0B,
        [bool]$IsVirtual = $false,
        [Parameter(Mandatory = $true)][bool]$IsGlobal,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Inputs,
        [AllowEmptyCollection()]
        [string[]]$Outputs = @()
    )

    $bytes = [Collections.Generic.List[byte]]::new()
    $length = [uint32]$Name.Length
    Add-BytesToList -List $bytes -Bytes ([byte[]]@(
            0x00, 0x01,
            [byte]($length -band 0xFF),
            [byte](($length -shr 8) -band 0xFF),
            [byte](($length -shr 16) -band 0xFF),
            0xAA))
    Add-AsciiToList -List $bytes -Text $Name
    Add-BytesToList -List $bytes -Bytes (
        Get-ClassDatabaseFunctionHeaderBytes `
            -MethodKind $MethodKind `
            -IsVirtual $IsVirtual `
            -IsGlobal $IsGlobal `
            -InputCount ([uint32]$Inputs.Count))
    foreach ($entry in $Inputs) {
        Add-SyntheticParameterMetadata `
            -List $bytes `
            -Entry $entry `
            -IsOutput $false
    }
    $outputCount = [uint32]$Outputs.Count
    Add-BytesToList -List $bytes -Bytes ([byte[]]@(
            [byte]($outputCount -band 0xFF),
            [byte](($outputCount -shr 8) -band 0xFF),
            [byte](($outputCount -shr 16) -band 0xFF),
            [byte](($outputCount -shr 24) -band 0xFF)))
    foreach ($entry in $Outputs) {
        Add-SyntheticParameterMetadata `
            -List $bytes `
            -Entry $entry `
            -IsOutput $true
    }
    Add-AaStringToList -List $bytes -Text ''
    Add-BytesToList -List $bytes -Bytes ([byte[]]@(0, 0, 0, 0, 0, 0))
    return ,$bytes.ToArray()
}

function Add-SyntheticVariablePrefix {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $length = [uint32]$Name.Length
    Add-BytesToList -List $List -Bytes ([byte[]]@(
            0x01,
            [byte]($length -band 0xFF),
            [byte](($length -shr 8) -band 0xFF),
            [byte](($length -shr 16) -band 0xFF),
            0xAA))
    Add-AsciiToList -List $List -Text $Name
}

function Add-SyntheticVariableTrailer {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)][uint32]$StorageOffset
    )

    Add-BytesToList -List $List -Bytes ([byte[]]@(
            [byte]($StorageOffset -band 0xFF),
            [byte](($StorageOffset -shr 8) -band 0xFF),
            [byte](($StorageOffset -shr 16) -band 0xFF),
            [byte](($StorageOffset -shr 24) -band 0xFF),
            0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0xAA))
}

function Add-SyntheticAnchorVariable {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][uint32]$StorageOffset
    )

    Add-SyntheticVariablePrefix -List $List -Name $Name
    Add-BytesToList -List $List -Bytes ([Text.Encoding]::ASCII.GetBytes(
            'synthetic-anchor-metadata'))
    Add-SyntheticVariableTrailer -List $List -StorageOffset $StorageOffset
}

function Add-SyntheticPrivateUdintVariable {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][uint32]$StorageOffset,
        [AllowEmptyString()][string]$AlternateName = ''
    )

    Add-SyntheticVariablePrefix -List $List -Name $Name
    Add-AaStringToList -List $List -Text ''
    Add-BytesToList -List $List -Bytes ([byte[]]@(1, 0, 0, 0, 0, 0, 0))
    Add-AaStringToList -List $List -Text $AlternateName
    Add-BytesToList -List $List -Bytes (Convert-HexStringToBytes -Hex (
            '04000000FFFFFFFF0100000000AA' +
            '00FFFFFFFF000000000000000000000000000000AA000000AA00000000' +
            '0000050000AA5544494E54000000AA000000AA000000AA000000AA000000' +
            'AA000000AA01000000000000000000000000000000000000'))
    Add-SyntheticVariableTrailer -List $List -StorageOffset $StorageOffset
}

function Add-SyntheticMethodInventory {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)][string[]]$Names,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$NewMethodSpec,
        [Parameter(Mandatory = $true)][string]$Kind
    )

    foreach ($name in $Names) {
        if ($name -ceq $NewMethodSpec.Name) {
            Add-BytesToList -List $List -Bytes (
                New-SyntheticMethodMetadata `
                    -Name $name `
                    -IsGlobal $NewMethodSpec.IsGlobal `
                    -Inputs @($NewMethodSpec.Inputs) `
                    -Outputs @($NewMethodSpec.Outputs))
        }
        else {
            $methodKind = if ($name -ceq 'CyWork') { [byte]0x05 } else { [byte]0x0B }
            Add-BytesToList -List $List -Bytes (
                New-SyntheticMethodMetadata `
                    -Name $name `
                    -MethodKind $methodKind `
                    -IsVirtual:($name -ceq 'CyWork') `
                    -IsGlobal $false `
                    -Inputs @())
        }
    }
}

function Add-SyntheticClassRecord {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Diagnostics', 'Tcp')]
        [string]$Kind,
        [Parameter(Mandatory = $true)][bool]$WithGateD,
        [bool]$WithAlternateNames = $false
    )

    $sourcePath = if ($Kind -ceq 'Diagnostics') {
        '.\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
    }
    else {
        '.\Class\TCPMotionInterface\TCPMotionInterface.st'
    }
    $anchorName = if ($Kind -ceq 'Diagnostics') {
        'BootIdFault'
    }
    else {
        'RpcCallbackLastDisarmResult'
    }
    $nextName = if ($Kind -ceq 'Diagnostics') {
        'Ds402HomeState'
    }
    else {
        'lsl_tcp_user'
    }
    $anchorOffset = if ($Kind -ceq 'Diagnostics') { [uint32]0x145 } else { [uint32]0x458C }
    $firstOffset = if ($Kind -ceq 'Diagnostics') { [uint32]0x148 } else { [uint32]0x4590 }
    $variables = if ($Kind -ceq 'Diagnostics') {
        $DiagnosticsVariableNames
    }
    else {
        $TcpVariableNames
    }
    $methods = if ($Kind -ceq 'Diagnostics') {
        $ExpectedDiagnosticsMethodInventory
    }
    else {
        $ExpectedTcpMethodInventory
    }
    $spec = if ($Kind -ceq 'Diagnostics') {
        $DiagnosticsMethodSpec
    }
    else {
        $TcpMethodSpec
    }

    Add-AsciiToList -List $List -Text $sourcePath
    Add-BytesToList -List $List -Bytes ([byte[]]@(0, 0, 0, 0xAA))
    Add-SyntheticAnchorVariable `
        -List $List `
        -Name $anchorName `
        -StorageOffset $anchorOffset
    if ($WithGateD) {
        for ($index = 0; $index -lt $variables.Count; $index++) {
            Add-SyntheticPrivateUdintVariable `
                -List $List `
                -Name $variables[$index] `
                -StorageOffset ($firstOffset + [uint32](4 * $index)) `
                -AlternateName $(if ($WithAlternateNames) {
                        $variables[$index].Substring(2)
                    }
                    else { '' })
        }
    }
    Add-SyntheticVariablePrefix -List $List -Name $nextName
    Add-AsciiToList -List $List -Text 'synthetic-next-variable'
    Add-SyntheticVariableTrailer `
        -List $List `
        -StorageOffset $(if ($WithGateD) {
                $firstOffset + 12
            }
            else {
                $firstOffset
            })
    Add-AsciiToList -List $List -Text 'synthetic-method-boundary'
    $methodNames = if ($WithGateD) {
        $methods
    }
    else {
        @($methods | Where-Object { $_ -cne $spec.Name })
    }
    Add-SyntheticMethodInventory `
        -List $List `
        -Names $methodNames `
        -NewMethodSpec $spec `
        -Kind $Kind
}

function New-SyntheticClassesDatabase {
    param(
        [Parameter(Mandatory = $true)][bool]$WithGateD,
        [bool]$WithAlternateNames = $false
    )

    $bytes = [Collections.Generic.List[byte]]::new()
    Add-SyntheticClassRecord `
        -List $bytes `
        -Kind Diagnostics `
        -WithGateD $WithGateD `
        -WithAlternateNames $WithAlternateNames
    Add-SyntheticClassRecord `
        -List $bytes `
        -Kind Tcp `
        -WithGateD $WithGateD `
        -WithAlternateNames $WithAlternateNames
    Add-AsciiToList -List $bytes -Text '.\Class\Sentinel\Sentinel.st'
    return ,$bytes.ToArray()
}

function New-SyntheticGateDSourcePair {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Diagnostics', 'Tcp')]
        [string]$Kind
    )

    if ($Kind -ceq 'Diagnostics') {
        $baseline = @'
// generated fixture
//{{LSL_DECLARATION
LMCDiagnosticsService : CLASS
  //Variables:
        BootIdFault : BOOL;
        Ds402HomeState : ARRAY [0..127] OF DINT;
  //Functions:
        FUNCTION GLOBAL ProcessOperations;

        FUNCTION IsSdoReadReady
                VAR_INPUT
                        SlaveReference : UINT;
                END_VAR
                VAR_OUTPUT
                        Ready : BOOL;
                END_VAR;
  //Tables:
        FUNCTION @STD;
        FUNCTION GLOBAL TAB @CT_;
END_CLASS;
//}}LSL_DECLARATION

FUNCTION GLOBAL TAB LMCDiagnosticsService::@CT_
0$UINT,
END_FUNCTION

#define USER_CNT_LMCDiagnosticsService 0

FUNCTION LMCDiagnosticsService::@STD
END_FUNCTION

//{{LSL_IMPLEMENTATION
FUNCTION GLOBAL LMCDiagnosticsService::ProcessOperations

END_FUNCTION


FUNCTION LMCDiagnosticsService::IsSdoReadReady
        VAR_INPUT
                SlaveReference : UINT;
        END_VAR
        VAR_OUTPUT
                Ready : BOOL;
        END_VAR

END_FUNCTION
'@
        $current = $baseline.Replace(
            "        BootIdFault : BOOL;`n        Ds402HomeState",
            ("        BootIdFault : BOOL;`n" +
             "        D5TerminalWakeLastAttemptTicketId : UDINT;`n" +
             "        D5TerminalWakeLastAttemptTicketBootId : UDINT;`n" +
             "        D5TerminalWakeLastAttemptOwnerSessionEpoch : UDINT;`n" +
             '        Ds402HomeState'))
        $current = $current.Replace(
            "        FUNCTION GLOBAL ProcessOperations;`n`n" +
            '        FUNCTION IsSdoReadReady',
            ("        FUNCTION GLOBAL ProcessOperations;`n`n" +
             "        FUNCTION GLOBAL TryTakeD5TerminalWake`n" +
             "                VAR_INPUT`n" +
             "                        pTicketId : ^UDINT;`n" +
             "                        pTicketBootId : ^UDINT;`n" +
             "                        pOwnerSessionEpoch : ^UDINT;`n" +
             "                END_VAR`n" +
             "                VAR_OUTPUT`n" +
             "                        Result : DINT;`n" +
             "                END_VAR;`n`n" +
             '        FUNCTION IsSdoReadReady'))
        $current += @'


FUNCTION GLOBAL LMCDiagnosticsService::TryTakeD5TerminalWake
        VAR_INPUT
                pTicketId : ^UDINT;
                pTicketBootId : ^UDINT;
                pOwnerSessionEpoch : ^UDINT;
        END_VAR
        VAR_OUTPUT
                Result : DINT;
        END_VAR

END_FUNCTION
'@
    }
    else {
        $baseline = @'
// generated fixture
//{{LSL_DECLARATION
TCPMotionInterface : CLASS
  //Variables:
        RpcCallbackLastDisarmResult : DINT;
        lsl_tcp_user : ^LSL_TCP_USER;
  //Functions:
        FUNCTION HandleRpcLifecycleCommands;

        FUNCTION DisarmRpcCallbackEndpoint
                VAR_OUTPUT
                        Result : DINT;
                END_VAR;
  //Tables:
        FUNCTION @STD;
        FUNCTION GLOBAL TAB @CT_;
END_CLASS;
//}}LSL_DECLARATION

FUNCTION GLOBAL TAB TCPMotionInterface::@CT_
0$UINT,
END_FUNCTION

#define USER_CNT_TCPMotionInterface 10

FUNCTION TCPMotionInterface::@STD
END_FUNCTION

//{{LSL_IMPLEMENTATION
FUNCTION TCPMotionInterface::HandleRpcLifecycleCommands

END_FUNCTION


FUNCTION TCPMotionInterface::DisarmRpcCallbackEndpoint
        VAR_OUTPUT
                Result : DINT;
        END_VAR

END_FUNCTION
'@
        $current = $baseline.Replace(
            "        RpcCallbackLastDisarmResult : DINT;`n" +
            '        lsl_tcp_user',
            ("        RpcCallbackLastDisarmResult : DINT;`n" +
             "        D5TerminalWakeAttemptCount : UDINT;`n" +
             "        D5TerminalWakeEnqueuedCount : UDINT;`n" +
             "        D5TerminalWakeRejectedCount : UDINT;`n" +
             '        lsl_tcp_user'))
        $current = $current.Replace(
            "                END_VAR;`n  //Tables:",
            ("                END_VAR;`n`n" +
             "        FUNCTION PublishD5TerminalWake;`n" +
             '  //Tables:'))
        $current += @'


FUNCTION TCPMotionInterface::PublishD5TerminalWake

END_FUNCTION
'@
    }
    return [pscustomobject]@{
        Baseline = ConvertTo-CanonicalLf -Text $baseline
        Current = ConvertTo-CanonicalLf -Text $current
    }
}

function Assert-NegativeFixture {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][scriptblock]$Action
    )

    $rejected = $false
    try {
        & $Action
    }
    catch {
        if ($_.Exception.Message -notlike "$Owner blocker:*") {
            throw
        }
        $rejected = $true
    }
    if (-not $rejected) {
        throw "$Owner self-test negative fixture '$Name' was accepted."
    }
    return 1
}

function Set-FirstAsciiOccurrenceByte {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$Needle,
        [Parameter(Mandatory = $true)][int]$RelativeOffset,
        [Parameter(Mandatory = $true)][byte]$Value
    )

    $text = [Text.Encoding]::GetEncoding(28591).GetString($Bytes)
    $index = $text.IndexOf($Needle, [StringComparison]::Ordinal)
    if ($index -lt 0) {
        throw "synthetic mutation target is missing: $Needle"
    }
    $copy = [byte[]]$Bytes.Clone()
    $copy[$index + $RelativeOffset] = $Value
    return ,$copy
}

function Invoke-GitText {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [int[]]$AllowedExitCodes = @(0)
    )

    Push-Location -LiteralPath $Root
    try {
        $output = @(& git @Arguments 2>&1)
        $exitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }
    if ($AllowedExitCodes -notcontains $exitCode) {
        Throw-GateDBlocker (
            "git $([string]::Join(' ', $Arguments)) failed with $exitCode`: " +
            [string]::Join("`n", @($output)))
    }
    return [pscustomobject]@{
        ExitCode = $exitCode
        Lines = @($output | ForEach-Object { $_.ToString() })
        Text = [string]::Join("`n", @($output | ForEach-Object { $_.ToString() }))
    }
}

function Get-GitBlobBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$ObjectSpec
    )

    if ($ObjectSpec -notmatch '^[0-9a-f]{40}:[A-Za-z0-9_./-]+$') {
        Throw-GateDBlocker "unsafe git blob spec: $ObjectSpec"
    }
    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = 'git'
    $startInfo.WorkingDirectory = $Root
    $startInfo.Arguments = 'cat-file blob ' + $ObjectSpec
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $memory = [IO.MemoryStream]::new()
    try {
        if (-not $process.Start()) {
            Throw-GateDBlocker "git cat-file did not start for $ObjectSpec."
        }
        $errorTask = $process.StandardError.ReadToEndAsync()
        $process.StandardOutput.BaseStream.CopyTo($memory)
        $process.WaitForExit()
        $errorText = $errorTask.GetAwaiter().GetResult()
        if ($process.ExitCode -ne 0) {
            Throw-GateDBlocker (
                "git cat-file failed for $ObjectSpec`: $errorText")
        }
        return ,$memory.ToArray()
    }
    finally {
        $memory.Dispose()
        $process.Dispose()
    }
}

function Assert-TrackedDriftLines {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Lines
    )

    $actual = [Collections.Generic.List[string]]::new()
    foreach ($line in $Lines) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }
        $parts = $line.Split("`t")
        if ($parts.Count -ne 2) {
            Throw-GateDBlocker "tracked diff line is not status/path: $line"
        }
        if ($parts[0] -cne 'M') {
            Throw-GateDBlocker (
                "tracked diff status for $($parts[1]) is $($parts[0]), expected M.")
        }
        $actual.Add($parts[1].Replace('\', '/'))
    }
    Assert-ExactInventory `
        -Actual @($actual.ToArray() | Sort-Object) `
        -Expected @($AllowedTrackedDrift | Sort-Object) `
        -InventoryOwner 'Gate D tracked worktree drift'
}

function Assert-LasalIdeClosed {
    $processes = @(Get-Process -Name Lasal2 -ErrorAction SilentlyContinue)
    if ($processes.Count -ne 0) {
        Throw-GateDBlocker (
            "LASAL IDE is running (PID $([string]::Join(',', @($processes.Id)))).")
    }
}

function Assert-RepositoryEnvelope {
    param([Parameter(Mandatory = $true)][string]$Root)

    $top = (Invoke-GitText `
            -Root $Root `
            -Arguments @('rev-parse', '--show-toplevel')).Text.Trim()
    $resolved = (Resolve-Path -LiteralPath $Root).Path.TrimEnd('\', '/')
    if (-not [string]::Equals(
            ([IO.Path]::GetFullPath($top).TrimEnd('\', '/')),
            ([IO.Path]::GetFullPath($resolved).TrimEnd('\', '/')),
            [StringComparison]::OrdinalIgnoreCase)) {
        Throw-GateDBlocker 'RepositoryRoot is not the Git top-level directory.'
    }
    $null = Invoke-GitText `
        -Root $Root `
        -Arguments @('cat-file', '-e', "$GateCCommit`^{commit}")
    $ancestor = Invoke-GitText `
        -Root $Root `
        -Arguments @('merge-base', '--is-ancestor', $GateCCommit, 'HEAD') `
        -AllowedExitCodes @(0, 1)
    if ($ancestor.ExitCode -ne 0) {
        Throw-GateDBlocker "HEAD is not descended from Gate C $GateCCommit."
    }
    $committedLasal = Invoke-GitText `
        -Root $Root `
        -Arguments @('diff', '--quiet', '--exit-code',
            $GateCCommit, 'HEAD', '--', $LasalRoot) `
        -AllowedExitCodes @(0, 1)
    if ($committedLasal.ExitCode -ne 0) {
        Throw-GateDBlocker (
            'committed LASAL tree no longer equals the Gate C baseline.')
    }
    $index = Invoke-GitText `
        -Root $Root `
        -Arguments @('diff', '--cached', '--quiet', '--exit-code') `
        -AllowedExitCodes @(0, 1)
    if ($index.ExitCode -ne 0) {
        Throw-GateDBlocker 'Git index is not empty.'
    }
    $drift = Invoke-GitText `
        -Root $Root `
        -Arguments @('-c', 'core.autocrlf=false',
            '-c', 'core.safecrlf=false',
            '-c', 'core.quotepath=false', 'diff',
            '--name-status', '--no-renames', 'HEAD', '--')
    Assert-TrackedDriftLines -Lines $drift.Lines
}

function Assert-ProtectedFiles {
    param([Parameter(Mandatory = $true)][string]$Root)

    foreach ($contract in $ProtectedFiles) {
        $path = Join-Path $Root $contract.Path
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            Throw-GateDBlocker "protected file is missing: $($contract.Path)"
        }
        $bytes = [IO.File]::ReadAllBytes($path)
        $sha256 = Get-Sha256Hex -Bytes $bytes
        if (($bytes.Count -ne $contract.Bytes) -or
            ($sha256 -cne $contract.Sha256)) {
            Throw-GateDBlocker (
                "protected file drifted: $($contract.Path) " +
                "($($bytes.Count)/$sha256).")
        }
    }
}

function Assert-ProjectDatabaseContract {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$CurrentBytes,
        [Parameter(Mandatory = $true)]
        [byte[]]$BaselineBytes,
        [switch]$SyntheticFixture
    )

    if ($CurrentBytes.Count -ne $BaselineBytes.Count) {
        Throw-GateDBlocker 'project .lcb byte length drifted during Save All.'
    }
    if ($CurrentBytes.Count -le $ProjectDeclarationDeltaOffset) {
        Throw-GateDBlocker 'project .lcb is too short for the declaration delta.'
    }
    if (-not $SyntheticFixture) {
        if ($CurrentBytes.Count -ne $ProjectExpectedBytes) {
            Throw-GateDBlocker (
                "project .lcb byte length is $($CurrentBytes.Count), expected " +
                "$ProjectExpectedBytes.")
        }
        $baselineSha256 = Get-Sha256Hex -Bytes $BaselineBytes
        if ($baselineSha256 -cne $ProjectBaselineSha256) {
            Throw-GateDBlocker (
                "Gate C project .lcb baseline drifted: $baselineSha256")
        }
        $currentSha256 = Get-Sha256Hex -Bytes $CurrentBytes
        if ($currentSha256 -cne $ProjectDeclarationSha256) {
            Throw-GateDBlocker (
                "declaration project .lcb identity drifted: $currentSha256")
        }
    }

    $differenceCount = 0
    $differenceOffset = -1
    for ($index = 0; $index -lt $CurrentBytes.Count; $index++) {
        if ($CurrentBytes[$index] -ne $BaselineBytes[$index]) {
            $differenceCount++
            if ($differenceCount -eq 1) {
                $differenceOffset = $index
            }
            else {
                break
            }
        }
    }
    if (($differenceCount -ne 1) -or
        ($differenceOffset -ne $ProjectDeclarationDeltaOffset) -or
        ($BaselineBytes[$ProjectDeclarationDeltaOffset] -ne 0) -or
        ($CurrentBytes[$ProjectDeclarationDeltaOffset] -ne 1)) {
        Throw-GateDBlocker (
            'project .lcb declaration delta is not exactly offset 39, 0 -> 1.')
    }
}

function Invoke-CurrentVerification {
    param([Parameter(Mandatory = $true)][string]$Root)

    Assert-LasalIdeClosed
    Assert-RepositoryEnvelope -Root $Root
    Assert-ProtectedFiles -Root $Root

    $diagnosticsBytes = [IO.File]::ReadAllBytes(
        (Join-Path $Root $DiagnosticsRelativePath))
    $tcpBytes = [IO.File]::ReadAllBytes(
        (Join-Path $Root $TcpRelativePath))
    $classesBytes = [IO.File]::ReadAllBytes(
        (Join-Path $Root $ClassesRelativePath))
    $projectBytes = [IO.File]::ReadAllBytes(
        (Join-Path $Root $ProjectRelativePath))
    $baselineDiagnosticsBytes = Get-GitBlobBytes `
        -Root $Root `
        -ObjectSpec "$GateCCommit`:$DiagnosticsRelativePath"
    $baselineTcpBytes = Get-GitBlobBytes `
        -Root $Root `
        -ObjectSpec "$GateCCommit`:$TcpRelativePath"
    $baselineClassesBytes = Get-GitBlobBytes `
        -Root $Root `
        -ObjectSpec "$GateCCommit`:$ClassesRelativePath"
    $baselineProjectBytes = Get-GitBlobBytes `
        -Root $Root `
        -ObjectSpec "$GateCCommit`:$ProjectRelativePath"

    $diagnostics = Get-StrictAsciiText `
        -Bytes $diagnosticsBytes `
        -FileOwner 'LMCDiagnosticsService.st' `
        -RequireCrLf
    $tcp = Get-StrictAsciiText `
        -Bytes $tcpBytes `
        -FileOwner 'TCPMotionInterface.st' `
        -RequireCrLf
    $baselineDiagnostics = Get-StrictAsciiText `
        -Bytes $baselineDiagnosticsBytes `
        -FileOwner 'Gate C LMCDiagnosticsService.st'
    $baselineTcp = Get-StrictAsciiText `
        -Bytes $baselineTcpBytes `
        -FileOwner 'Gate C TCPMotionInterface.st'
    Assert-GateDSourceContract `
        -CurrentText $diagnostics `
        -BaselineText $baselineDiagnostics `
        -Kind Diagnostics
    Assert-GateDSourceContract `
        -CurrentText $tcp `
        -BaselineText $baselineTcp `
        -Kind Tcp
    Assert-ClassesDatabaseContract `
        -CurrentBytes $classesBytes `
        -BaselineBytes $baselineClassesBytes

    Assert-ProjectDatabaseContract `
        -CurrentBytes $projectBytes `
        -BaselineBytes $baselineProjectBytes

    Assert-RepositoryEnvelope -Root $Root
    Assert-LasalIdeClosed
    return [pscustomobject]@{
        DiagnosticsBytes = $diagnosticsBytes.Count
        DiagnosticsSha256 = Get-Sha256Hex -Bytes $diagnosticsBytes
        TcpBytes = $tcpBytes.Count
        TcpSha256 = Get-Sha256Hex -Bytes $tcpBytes
        ClassesBytes = $classesBytes.Count
        ClassesSha256 = Get-Sha256Hex -Bytes $classesBytes
        ProjectBytes = $projectBytes.Count
        ProjectSha256 = Get-Sha256Hex -Bytes $projectBytes
        ProtectedCount = $ProtectedFiles.Count
    }
}

function Invoke-GateDSelfTest {
    $diagnostics = New-SyntheticGateDSourcePair -Kind Diagnostics
    $tcp = New-SyntheticGateDSourcePair -Kind Tcp
    $baselineClasses = New-SyntheticClassesDatabase -WithGateD $false
    $currentClasses = New-SyntheticClassesDatabase -WithGateD $true
    $alternateNameClasses = New-SyntheticClassesDatabase `
        -WithGateD $true `
        -WithAlternateNames $true
    $baselineProject = [byte[]]::new(64)
    $currentProject = [byte[]]$baselineProject.Clone()
    $currentProject[$ProjectDeclarationDeltaOffset] = 1
    Assert-GateDSourceContract `
        -CurrentText $diagnostics.Current `
        -BaselineText $diagnostics.Baseline `
        -Kind Diagnostics
    Assert-GateDSourceContract `
        -CurrentText $tcp.Current `
        -BaselineText $tcp.Baseline `
        -Kind Tcp
    Assert-ClassesDatabaseContract `
        -CurrentBytes $currentClasses `
        -BaselineBytes $baselineClasses
    Assert-ClassesDatabaseContract `
        -CurrentBytes $alternateNameClasses `
        -BaselineBytes $baselineClasses
    Assert-ProjectDatabaseContract `
        -CurrentBytes $currentProject `
        -BaselineBytes $baselineProject `
        -SyntheticFixture

    $negativeCount = 0
    $negativeCount += Assert-NegativeFixture `
        -Name 'diagnostics variable type' `
        -Action {
            Assert-GateDSourceContract `
                -CurrentText $diagnostics.Current.Replace(
                    'D5TerminalWakeLastAttemptTicketId : UDINT;',
                    'D5TerminalWakeLastAttemptTicketId : UINT;') `
                -BaselineText $diagnostics.Baseline `
                -Kind Diagnostics
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'diagnostics variable initializer' `
        -Action {
            Assert-GateDSourceContract `
                -CurrentText $diagnostics.Current.Replace(
                    'D5TerminalWakeLastAttemptTicketId : UDINT;',
                    'D5TerminalWakeLastAttemptTicketId : UDINT := 0;') `
                -BaselineText $diagnostics.Baseline `
                -Kind Diagnostics
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'diagnostics variable order' `
        -Action {
            $mutated = $diagnostics.Current.Replace(
                "D5TerminalWakeLastAttemptTicketId : UDINT;`n" +
                '        D5TerminalWakeLastAttemptTicketBootId : UDINT;',
                "D5TerminalWakeLastAttemptTicketBootId : UDINT;`n" +
                '        D5TerminalWakeLastAttemptTicketId : UDINT;')
            Assert-GateDSourceContract `
                -CurrentText $mutated `
                -BaselineText $diagnostics.Baseline `
                -Kind Diagnostics
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'diagnostics pointer ABI' `
        -Action {
            Assert-GateDSourceContract `
                -CurrentText $diagnostics.Current.Replace(
                    'pTicketId : ^UDINT;',
                    'pTicketId : UDINT;') `
                -BaselineText $diagnostics.Baseline `
                -Kind Diagnostics
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'diagnostics input order' `
        -Action {
            $mutated = $diagnostics.Current.Replace(
                "pTicketId : ^UDINT;`n" +
                '                        pTicketBootId : ^UDINT;',
                "pTicketBootId : ^UDINT;`n" +
                '                        pTicketId : ^UDINT;')
            Assert-GateDSourceContract `
                -CurrentText $mutated `
                -BaselineText $diagnostics.Baseline `
                -Kind Diagnostics
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'diagnostics nonempty stub' `
        -Action {
            $needle = "        END_VAR`n`nEND_FUNCTION"
            $methodStart = $diagnostics.Current.IndexOf(
                'FUNCTION GLOBAL LMCDiagnosticsService::TryTakeD5TerminalWake',
                [StringComparison]::Ordinal)
            $needleStart = $diagnostics.Current.IndexOf(
                $needle,
                $methodStart,
                [StringComparison]::Ordinal)
            if (($methodStart -lt 0) -or ($needleStart -lt 0)) {
                throw 'synthetic diagnostics implementation fixture drifted.'
            }
            $mutated = $diagnostics.Current.Insert(
                $needleStart + "        END_VAR`n`n".Length,
                "Result := 0;`n")
            Assert-GateDSourceContract `
                -CurrentText $mutated `
                -BaselineText $diagnostics.Baseline `
                -Kind Diagnostics
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'TCP promoted global helper' `
        -Action {
            Assert-GateDSourceContract `
                -CurrentText $tcp.Current.Replace(
                    'FUNCTION TCPMotionInterface::PublishD5TerminalWake',
                    'FUNCTION GLOBAL TCPMotionInterface::PublishD5TerminalWake') `
                -BaselineText $tcp.Baseline `
                -Kind Tcp
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'TCP helper unexpected output' `
        -Action {
            Assert-GateDSourceContract `
                -CurrentText $tcp.Current.Replace(
                    '        FUNCTION PublishD5TerminalWake;',
                    "        FUNCTION PublishD5TerminalWake`n" +
                    "                VAR_OUTPUT`n" +
                    "                        Result : DINT;`n" +
                    '                END_VAR;') `
                -BaselineText $tcp.Baseline `
                -Kind Tcp
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'generated USER count' `
        -Action {
            Assert-GateDSourceContract `
                -CurrentText $tcp.Current.Replace(
                    '#define USER_CNT_TCPMotionInterface 10',
                    '#define USER_CNT_TCPMotionInterface 11') `
                -BaselineText $tcp.Baseline `
                -Kind Tcp
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'source drift outside insertion' `
        -Action {
            Assert-GateDSourceContract `
                -CurrentText $diagnostics.Current.Replace('0$UINT,', '1$UINT,') `
                -BaselineText $diagnostics.Baseline `
                -Kind Diagnostics
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'Classes variable descriptor' `
        -Action {
            $mutated = Set-FirstAsciiOccurrenceByte `
                -Bytes $currentClasses `
                -Needle $DiagnosticsVariableNames[0] `
                -RelativeOffset ($DiagnosticsVariableNames[0].Length + 10) `
                -Value 1
            Assert-ClassesDatabaseContract `
                -CurrentBytes $mutated `
                -BaselineBytes $baselineClasses
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'Classes method global flag' `
        -Action {
            $mutated = Set-FirstAsciiOccurrenceByte `
                -Bytes $currentClasses `
                -Needle $DiagnosticsMethodSpec.Name `
                -RelativeOffset ($DiagnosticsMethodSpec.Name.Length + 5) `
                -Value 0
            Assert-ClassesDatabaseContract `
                -CurrentBytes $mutated `
                -BaselineBytes $baselineClasses
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'Classes method input count' `
        -Action {
            $mutated = Set-FirstAsciiOccurrenceByte `
                -Bytes $currentClasses `
                -Needle $DiagnosticsMethodSpec.Name `
                -RelativeOffset ($DiagnosticsMethodSpec.Name.Length + 8) `
                -Value 2
            Assert-ClassesDatabaseContract `
                -CurrentBytes $mutated `
                -BaselineBytes $baselineClasses
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'Classes anchor metadata' `
        -Action {
            $mutated = Set-FirstAsciiOccurrenceByte `
                -Bytes $currentClasses `
                -Needle 'synthetic-anchor-metadata' `
                -RelativeOffset 0 `
                -Value ([byte][char]'S')
            Assert-ClassesDatabaseContract `
                -CurrentBytes $mutated `
                -BaselineBytes $baselineClasses
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'project declaration delta wrong offset' `
        -Action {
            $mutated = [byte[]]$baselineProject.Clone()
            $mutated[$ProjectDeclarationDeltaOffset + 1] = 1
            Assert-ProjectDatabaseContract `
                -CurrentBytes $mutated `
                -BaselineBytes $baselineProject `
                -SyntheticFixture
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'project declaration delta extra mutation' `
        -Action {
            $mutated = [byte[]]$currentProject.Clone()
            $mutated[20] = 1
            Assert-ProjectDatabaseContract `
                -CurrentBytes $mutated `
                -BaselineBytes $baselineProject `
                -SyntheticFixture
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'tracked drift missing path' `
        -Action {
            Assert-TrackedDriftLines -Lines @(
                "M`t$DiagnosticsRelativePath",
                "M`t$TcpRelativePath",
                "M`t$ClassesRelativePath")
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'tracked drift extra path' `
        -Action {
            Assert-TrackedDriftLines -Lines @(
                @($AllowedTrackedDrift | ForEach-Object { "M`t$_" }) +
                "M`tdocs/unapproved.md")
        }
    $negativeCount += Assert-NegativeFixture `
        -Name 'tracked drift deletion' `
        -Action {
            $lines = @($AllowedTrackedDrift | ForEach-Object { "M`t$_" })
            $lines[0] = "D`t$DiagnosticsRelativePath"
            Assert-TrackedDriftLines -Lines $lines
        }
    return $negativeCount
}

if ($RunSelfTest) {
    $negativeCount = Invoke-GateDSelfTest
    Write-Output (
        'PASS LASAL.UdpCallbackGateDDeclaration.SelfTest ' +
        "($negativeCount/$negativeCount focused negatives rejected; " +
        'source reverse-delta, bounded Classes ABI, and exact project delta ' +
        'positives accepted)')
    return
}

if ($VerifyCurrent) {
    $root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
    $result = Invoke-CurrentVerification -Root $root
    Write-Output (
        'PASS LASAL.UdpCallbackGateDDeclaration.Current ' +
        "(baseline=$GateCCommit; trackedDrift=4; indexEmpty=true; " +
        "IDEClosed=true; Diagnostics=$($result.DiagnosticsBytes)/" +
        "$($result.DiagnosticsSha256); TCP=$($result.TcpBytes)/" +
        "$($result.TcpSha256); Classes=$($result.ClassesBytes)/" +
        "$($result.ClassesSha256); project=$($result.ProjectBytes)/" +
        "$($result.ProjectSha256); protected=$($result.ProtectedCount))")
    return
}

throw "$Owner blocker: no operation was selected."
