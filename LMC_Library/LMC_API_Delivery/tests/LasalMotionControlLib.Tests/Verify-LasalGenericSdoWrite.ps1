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
$verificationPath = Join-Path $RepositoryRoot 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsSdoWriteVerification.cs'
$policyTestsPath = Join-Path $RepositoryRoot 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\DiagnosticsSdoWritePolicyEvaluationTests.cs'
$wpfPath = Join-Path $RepositoryRoot 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.Qualification.SdoWrite.cs'
$wpfDiagnosticsPath = Join-Path $RepositoryRoot 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.Diagnostics.cs'
$proofPath = Join-Path $RepositoryRoot 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\SdoWriteActivationQualificationProof.cs'
$journalPath = Join-Path $RepositoryRoot 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\DiagnosticsMutationJournal.cs'
$sdkSubmitPath = Join-Path $RepositoryRoot 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5.cs'
$plcPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
$executorPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCSdoExecutor\LMCSdoExecutor.st'

$sdk = Get-Content -LiteralPath $sdkPath -Raw
$verification = Get-Content -LiteralPath $verificationPath -Raw
$policyTests = Get-Content -LiteralPath $policyTestsPath -Raw
$wpf = Get-Content -LiteralPath $wpfPath -Raw
$wpfDiagnostics = Get-Content -LiteralPath $wpfDiagnosticsPath -Raw
$proof = Get-Content -LiteralPath $proofPath -Raw
$journal = Get-Content -LiteralPath $journalPath -Raw
$sdkSubmit = Get-Content -LiteralPath $sdkSubmitPath -Raw
$plc = Get-Content -LiteralPath $plcPath -Raw
$executor = Get-Content -LiteralPath $executorPath -Raw

function Require-Match([string]$Text, [string]$Pattern, [string]$Message) {
    if ($Text -notmatch $Pattern) { throw $Message }
}
function Require-NoMatch([string]$Text, [string]$Pattern, [string]$Message) {
    if ($Text -match $Pattern) { throw $Message }
}

Require-NoMatch $sdk 'target is not in the SDK compile-time allowlist' `
    'SDK still rejects generic SDO Write by exact address allowlist.'
Require-Match $sdk 'D5 SDO WriteData must contain exactly 1, 2, 4, 8, or 12 bytes' `
    'SDK request model does not represent narrow scalar Write payloads.'
Require-Match $sdk 'Generic SDO Write supports SlaveReference 1 through 4 only' `
    'SDK generic slave-range policy is missing.'
Require-Match $sdk '(?s)internal static bool IsPermanentlyUnsafeObject\(ushort objectIndex\).*?return false;' `
    'SDK generic SDO Write address policy is not explicitly denylist-free.'
Require-NoMatch $sdk '(?s)RequireSdoWriteAllowed.*?request\.ObjectIndex == 0\s*\|\|' `
    'SDK generic SDO Write admission still chains an ObjectIndex denylist.'
Require-Match $sdk 'ExpectedReadLength\(request\.ValueType\)' `
    'SDK generic Write does not reuse canonical scalar widths.'
Require-Match $sdk '(?s)request\.ObjectIndex == 0x2F00.*?request\.SubIndex == 24' `
    'SDK UI[24] preset range guard is missing.'
Require-Match $verification 'exact canonical 1/2/4-byte scalar Write request' `
    'SDK exact readback verification is still four-byte-only.'
Require-Match $sdk 'NoApprovedTarget = 1u << 0' `
    'SDK legacy NoApprovedTarget enum value was not preserved.'
Require-NoMatch $sdk 'blockers \|= LMCSdoWritePolicyBlockers\.NoApprovedTarget' `
    'SDK generic policy still treats known-preset absence as an admission blocker.'
Require-Match $sdk 'WritePolicyDisabled = 1u << 10' `
    'SDK has no explicit generic Write policy-disabled blocker.'
Require-Match $sdk '(?s)if \(!writePolicyEnabled\).*?WritePolicyDisabled' `
    'SDK global Write policy gate does not emit WritePolicyDisabled.'
Require-Match $sdk '(?s)KnownSdoWritePresets\s*=\s*CreateKnownSdoWritePresets' `
    'SDK still names the UI24 preset collection as address authorization.'
Require-Match $policyTests '(?s)new LMCSdoWriteTarget\[0\].*?CanAttemptSubmission.*?ApprovedTargets\.Count' `
    'SDK regression does not prove empty known presets can pass generic policy.'
Require-NoMatch $wpf 'LMCSdoWritePolicyBlockers\.NoApprovedTarget' `
    'WPF still presents known-preset absence as a generic policy blocker.'
Require-Match $wpf 'UI\[24\] transport-canary qualification requires exactly one known preset' `
    'WPF UI24 workflow is not identified as a transport canary.'
Require-Match $proof 'BaselineTicketId' `
    'Transport qualification proof does not retain the four-ticket canary evidence.'
Require-Match $proof '(?s)MatchesCurrent\(\s*LMCConnection connection,\s*LMCDiagnosticCapabilities capabilities\)' `
    'Runtime transport proof is still target-bound.'
Require-Match $proof 'HasRequiredTransportCapabilities' `
    'Transport proof does not revalidate the current SDO capability image.'
Require-Match $wpfDiagnostics 'manual-sdo-write-baseline' `
    'Ordinary SDO Write baseline Read stage is missing.'
Require-Match $wpfDiagnostics 'manual-sdo-write-prewrite-guard' `
    'Ordinary SDO Write pre-Write guard Read stage is missing.'
if ([regex]::Matches(
        $wpfDiagnostics,
        'HasCurrentSdoWriteActivationQualificationProof').Count -ne 1) {
    throw 'Ordinary WPF SDO Write still references the optional qualification proof.'
}
Require-NoMatch $wpfDiagnostics 'Run Same-Value Qualification First' `
    'Ordinary WPF SDO Write still exposes the old mandatory qualification action.'
Require-Match $wpfDiagnostics '(?s)SdoDataEqual\(\s*baselineData,\s*preWriteGuardData\).*?ArmExternalD5SubmissionOutcomeGuard.*?SubmitSdoWriteIdentityPinnedAsync' `
    'Ordinary SDO Write ordering is not baseline equality -> durable arm -> one identity-pinned submit.'
Require-NoMatch $wpfDiagnostics '(?s)SubmitSdoWriteIdentityPinnedAsync.*?SubmitSdoWriteIdentityPinnedAsync' `
    'Ordinary SDO Write source contains more than one identity-pinned submit call.'
Require-Match $journal 'private const int FormatVersion = 4;' `
    'Durable SDO journal schema was not advanced for baseline guard evidence.'
Require-Match $journal '(?s)BaselineData.*?PreWriteGuardData.*?ExpectedWriteData' `
    'Durable SDO journal does not retain baseline, pre-Write guard, and expected bytes.'
Require-Match $journal 'if \(objectIndex == 0\)' `
    'Durable SDO recovery does not reject ObjectIndex zero.'
Require-NoMatch $journal 'IsPermanentlyUnsafeObject|objectIndex == 0x6040|objectIndex == 0x6060|objectIndex == 0x607A|objectIndex == 0x60FF|objectIndex == 0x6071|objectIndex == 0x3204|objectIndex == 0x20FC' `
    'Durable SDO recovery still contains an ObjectIndex denylist.'
Require-Match $sdkSubmit '(?s)SubmitSdoWriteIdentityPinnedAsync\(\s*LMCSdoRequest request,\s*LMCDiagnosticCapabilities requiredCapabilities,\s*CancellationToken' `
    'SDK generic identity-pinned SDO Write overload is missing.'

Require-NoMatch $plc '\(ObjectIndex <> 0x2F00\) \| \(SubIndex <> 24\)' `
    'PLC still rejects generic SDO Write by the old UI[24] address gate.'
$plcPolicy = [regex]::Match(
    $plc,
    '(?s)FUNCTION LMCDiagnosticsService::GetSdoWritePolicyDetail.*?END_FUNCTION').Value
Require-Match $plcPolicy '\(ObjectIndex = 0\)' `
    'PLC generic SDO Write does not reject ObjectIndex zero.'
Require-NoMatch $plcPolicy '0x6040|0x6060|0x607A|0x60FF|0x6071|0x3204|0x20FC' `
    'PLC generic SDO Write policy still contains the former ObjectIndex denylist.'
Require-Match $plc '(?s)case ValueType of\s*1, 9, 10, 11:.*?DataLength <> 1.*?2, 3, 7:.*?DataLength <> 2.*?4, 5, 6, 8:.*?DataLength <> 4' `
    'PLC canonical 1/2/4-byte scalar type/length admission is missing.'
Require-Match $plc '(?s)case sdoDataLength of\s*1:.*?\(pRequest \+ 32\)\^\$USINT.*?2:.*?\(pRequest \+ 32\)\^\$UINT.*?4:.*?\(pRequest \+ 32\)\^\$UDINT' `
    'PLC 0x7E50 parser does not preserve canonical 1/2/4-byte Write payloads.'
Require-NoMatch $plc '(?s)sdoOperationFlags = 1\) & \(sdoDataLength <> 4\).*?requestSdoValueType <> 4' `
    'PLC 0x7E50 handler still limits Write to Int32/4.'
Require-Match $plc '(?s)\(ValueType = 1\).*?\(WriteData <> 0\).*?\(WriteData <> 1\)' `
    'PLC canonical Bool Write validation is missing.'
Require-Match $plc '(?s)\(ObjectIndex = 0x2F00\) & \(SubIndex = 24\).*?writeValue := WriteData\$DINT' `
    'PLC UI[24] range guard is not scoped to the known preset.'
Require-Match $plc '(?s)if LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = TRUE then\s*\(pResponse \+ 20\)\^\$UDINT :=\s*\(pResponse \+ 20\)\^\$UDINT OR 0x00000200;' `
    'PLC SDO Write capability is still coupled to UI[24] axis preset flags.'
Require-Match $plc 'UI24 axis flags expose qualification presets only' `
    'PLC UI24 per-axis flags are not documented as preset-only exposure.'
Require-Match $executor '(?s)RequestSource <> LMC_SDO_SOURCE_NONE.*?LMC_SDO_SOURCE_MANUAL_SERVER' `
    'Manual SDO server path does not share the executor ownership gate.'
Require-Match $executor '(?s)FUNCTION GLOBAL LMCSdoExecutor::TryStartWrite.*?RequestSource <> LMC_SDO_SOURCE_NONE.*?LMC_SDO_SOURCE_PROGRAMMATIC' `
    'Programmatic SDO Write path does not share the executor ownership gate.'
Require-Match $executor '(?s)FUNCTION GLOBAL LMCSdoExecutor::MarkOrphan.*?LMC_SDO_EXEC_ORPHANED' `
    'SDO timeout/disconnect orphan drain path is missing.'

Write-Host 'PASS SWR-01..04 generic scalar SDO Write source and exact-once ordering contract'
