[CmdletBinding()]
param(
    [string]$RepositoryRoot
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepositoryRoot = Split-Path -Parent $scriptDirectory
}

function Require-True {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw "HOMEEX baseline verification failed: $Message"
    }

    Write-Host "PASS $Message"
    $script:PassCount++
}

function Require-Regex {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Message
    )

    Require-True ([regex]::IsMatch(
        $Text,
        $Pattern,
        [System.Text.RegularExpressions.RegexOptions]::Multiline)) $Message
}

function Require-AbsentRegex {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Message
    )

    Require-True (-not [regex]::IsMatch(
        $Text,
        $Pattern,
        [System.Text.RegularExpressions.RegexOptions]::Multiline)) $Message
}

$script:PassCount = 0

$tcpPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
$diagnosticsPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
$controlPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'

foreach ($path in @($tcpPath, $diagnosticsPath, $controlPath)) {
    Require-True (Test-Path -LiteralPath $path) ("required source exists: " + $path)
}

$tcp = Get-Content -LiteralPath $tcpPath -Raw
$diagnostics = Get-Content -LiteralPath $diagnosticsPath -Raw
$control = Get-Content -LiteralPath $controlPath -Raw

Require-Regex $tcp '0x7D15\s*,\s*0x7D16\s*,\s*0x7D17' 'existing HomeDS402 lifecycle route remains present'
Require-Regex $tcp '0x7D23\s*,\s*0x7D24\s*,\s*0x7D25' 'existing SetOperationMode lifecycle route remains present'

Require-Regex $tcp '(?s)\(CommandID\s*=\s*0x7D15\).*?diagnosticsOwnerKind\s*:=\s*4\s*;.*?diagnosticsResourceKind\s*:=\s*3\s*;' 'HomeDS402 Start keeps owner kind 4 and shared Home resource kind 3'
Require-Regex $tcp '(?s)\(CommandID\s*=\s*0x7D23\).*?diagnosticsOwnerKind\s*:=\s*6\s*;.*?diagnosticsResourceKind\s*:=\s*4\s*;' 'SetOperationMode Start keeps owner kind 6 and diagnostics SDO resource kind 4'

Require-Regex $diagnostics 'Ds402HomeState\s*:\s*ARRAY\s*\[0\.\.127\]\s*OF\s*DINT\s*;' 'existing HomeDS402 state store remains present'
Require-Regex $diagnostics 'AxisOperationModeState\s*:\s*ARRAY\s*\[0\.\.191\]\s*OF\s*DINT\s*;' 'existing SetOperationMode state store remains present'

Require-AbsentRegex $tcp '0x7D1B|0x7D1C|0x7D1D' 'HomeDS402Ex lifecycle route is still absent from TCPMotionInterface'
Require-AbsentRegex $diagnostics '0x7D1B|0x7D1C|0x7D1D' 'HomeDS402Ex lifecycle command ids are still absent from LMCDiagnosticsService'
Require-AbsentRegex $diagnostics 'Ds402HomeExState|HandleAxisDs402HomeEx|ProcessAxisDs402HomeEx' 'HomeDS402Ex LASAL state/handler symbols are still absent'

$featureMatches = [regex]::Matches(
    $control,
    '\(pResponseFrame\s*\+\s*24\)\^\$UDINT\s*:=\s*0x([0-9A-Fa-f]{8})\s*;')
Require-True ($featureMatches.Count -eq 1) 'Admin capability feature mask has exactly one canonical assignment'

$featureMask = [Convert]::ToUInt32($featureMatches[0].Groups[1].Value, 16)
Require-True ($featureMask -eq 0x00000017) 'production Admin feature mask remains 0x00000017'
Require-True (($featureMask -band 0x00000800) -eq 0) 'HomeDS402Ex Admin feature bit 11 remains OFF'

Require-AbsentRegex $control '0x00000817' 'no HomeDS402Ex activated Admin mask is present'

Write-Host ("HOMEEX LASAL dormant baseline PASS: {0} checks; state=BASELINE_OFF" -f $script:PassCount)
