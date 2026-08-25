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

$controlPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'
$tcpPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
$diagnosticsPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'

foreach ($path in @($controlPath, $tcpPath, $diagnosticsPath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "HOMEEX-07 audit missing source: $path"
    }
}

$control = Get-Content -LiteralPath $controlPath -Raw
$tcp = Get-Content -LiteralPath $tcpPath -Raw
$diagnostics = Get-Content -LiteralPath $diagnosticsPath -Raw

$controlBlob = (& git -C $RepositoryRoot hash-object -- $controlPath).Trim()
$tcpBlob = (& git -C $RepositoryRoot hash-object -- $tcpPath).Trim()
$diagnosticsBlob = (& git -C $RepositoryRoot hash-object -- $diagnosticsPath).Trim()

Write-Host "ControlBlob=$controlBlob"
Write-Host "TcpBlob=$tcpBlob"
Write-Host "DiagnosticsBlob=$diagnosticsBlob"

function Count-Regex {
    param([string]$Text, [string]$Pattern, [string]$Label)
    $options = [System.Text.RegularExpressions.RegexOptions]::Multiline -bor [System.Text.RegularExpressions.RegexOptions]::Singleline
    $count = ([regex]::Matches($Text, $Pattern, $options)).Count
    Write-Host ("{0}={1}" -f $Label, $count)
    return $count
}

$tailLimit8 = Count-Regex $control 'identityTailSize\s*>\s*8' 'TailLimit8Count'
$tailOffset8 = Count-Regex $control 'identityTailOffset\s*:=\s*TO_UDINT\([^;]+?\)\s*\*\s*8\s*;' 'TailOffset8Count'
$opModeKindLabels = Count-Regex $control 'LMC_OWNER_KIND_AXIS_OPERATION_MODE\s*:' 'OperationModeKindCaseCount'
$opModeCommandLabels = Count-Regex $control '^\s*0x7D23\s*:' 'OperationModeCommandCaseCount'
$ownerMaxGuard = Count-Regex $control 'OwnerKind\s*>\s*LMC_OWNER_KIND_AXIS_OPERATION_MODE' 'OwnerMaxGuardCount'
$homeExKind = Count-Regex $control 'LMC_OWNER_KIND_DS402_HOME_EX' 'HomeExOwnerKindCount'
$homeExCommandControl = Count-Regex $control '0x7D1B' 'HomeExCommandControlCount'
$homeExCommandTcp = Count-Regex $tcp '0x7D1B' 'HomeExCommandTcpCount'
$homeExAdmissionReject = Count-Regex $diagnostics 'AdmissionToken\s*<>\s*0[\s\S]{0,240}OwnerGeneration\s*<>\s*0[\s\S]{0,240}HOMEEX-06 has no ownership reservation' 'HomeExAdmissionRejectCount'

if ($controlBlob -ne '3f4ef46b2a584410781e933743b34469b745ebc3') {
    throw "HOMEEX-07 audit source baseline drifted: control blob $controlBlob"
}
if ($tailLimit8 -lt 5) { throw "HOMEEX-07 audit expected multiple 8-byte tail limits" }
if ($tailOffset8 -lt 5) { throw "HOMEEX-07 audit expected multiple 8-byte tail offsets" }
if ($opModeKindLabels -lt 5) { throw "HOMEEX-07 audit expected multiple owner-kind switch labels" }
if ($opModeCommandLabels -lt 2) { throw "HOMEEX-07 audit expected multiple command tuple cases" }
if ($ownerMaxGuard -ne 1) { throw "HOMEEX-07 audit expected one owner max guard" }
if ($homeExKind -ne 0) { throw "HOMEEX-07 audit expected OwnerKind 7 to remain absent on baseline" }
if ($homeExCommandControl -ne 0) { throw "HOMEEX-07 audit expected 0x7D1B absent from ownership service baseline" }
if ($homeExCommandTcp -ne 1) { throw "HOMEEX-07 audit expected only the HOMEEX-06 route occurrence in TCP baseline" }
if ($homeExAdmissionReject -ne 1) { throw "HOMEEX-07 audit expected the HOMEEX-06 admission-token rejection exactly once" }

Write-Host 'HOMEEX-07 ownership baseline audit PASS.'
