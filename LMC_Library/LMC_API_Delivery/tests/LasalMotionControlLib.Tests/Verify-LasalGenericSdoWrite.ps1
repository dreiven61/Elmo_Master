param(
    [string]$RepositoryRoot = ""
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $PSScriptRoot '..\..\..\..'))
}

$sdkPath = Join-Path $RepositoryRoot 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5Models.cs'
$plcPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'

$sdk = Get-Content -LiteralPath $sdkPath -Raw
$plc = Get-Content -LiteralPath $plcPath -Raw

function Require-Match([string]$Text, [string]$Pattern, [string]$Message) {
    if ($Text -notmatch $Pattern) { throw $Message }
}
function Require-NoMatch([string]$Text, [string]$Pattern, [string]$Message) {
    if ($Text -match $Pattern) { throw $Message }
}

Require-NoMatch $sdk 'target is not in the SDK compile-time allowlist' `
    'SDK still rejects generic SDO Write by address allowlist.'
Require-Match $sdk 'Generic SDO Write supports SlaveReference 1 through 4 only' `
    'SDK generic slave-range policy is missing.'
Require-Match $sdk '(?s)case 0x6040:\s*case 0x6060:\s*case 0x607A:\s*case 0x60FF:\s*case 0x6071:' `
    'SDK semantic-owner raw-write block list is incomplete.'
Require-Match $sdk 'ExpectedReadLength\(request\.ValueType\)' `
    'SDK generic Write does not reuse canonical scalar type lengths.'
Require-Match $sdk '(?s)request\.ObjectIndex == 0x2F00.*?request\.SubIndex == 24' `
    'SDK UI[24] preset range guard is missing.'

Require-NoMatch $plc '\(ObjectIndex <> 0x2F00\) \| \(SubIndex <> 24\)' `
    'PLC still rejects generic SDO Write by the old UI[24] address gate.'
Require-Match $plc '(?s)\(ObjectIndex = 0x6040\) \| \(ObjectIndex = 0x6060\).*?\(ObjectIndex = 0x607A\).*?\(ObjectIndex = 0x60FF\).*?\(ObjectIndex = 0x6071\)' `
    'PLC semantic-owner raw-write block list is incomplete.'
Require-Match $plc '(?s)case ValueType of\s*1, 9, 10, 11:.*?DataLength <> 1.*?2, 3, 7:.*?DataLength <> 2.*?4, 5, 6, 8:.*?DataLength <> 4' `
    'PLC canonical 1/2/4-byte scalar type/length admission is missing.'
Require-Match $plc '(?s)\(ValueType = 1\).*?\(WriteData <> 0\).*?\(WriteData <> 1\)' `
    'PLC canonical Bool Write validation is missing.'
Require-Match $plc '(?s)\(ObjectIndex = 0x2F00\) & \(SubIndex = 24\).*?writeValue := WriteData\$DINT' `
    'PLC UI[24] range guard is not scoped to the known preset.'
Require-Match $plc '(?s)if LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = TRUE then\s*\(pResponse \+ 20\)\^\$UDINT :=\s*\(pResponse \+ 20\)\^\$UDINT OR 0x00000200;' `
    'PLC SDO Write capability is still coupled to UI[24] axis preset flags.'

Write-Host 'PASS SDO-R03 generic scalar Write source contract'
