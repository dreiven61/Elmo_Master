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

$expectedControlBlob = '3f4ef46b2a584410781e933743b34469b745ebc3'
$expectedTcpBlob = '7c90a2b3cf46abb67b550ab1f8deefa453803b63'
$expectedDiagnosticsBlob = '498c37a768d39f62d999abe7dc4eec3e4ec42bac'

function Read-Lf([string]$Path) {
    return [System.IO.File]::ReadAllText($Path).Replace("`r`n", "`n").Replace("`r", "`n")
}

function Write-AsciiLf([string]$Path, [string]$Text) {
    $Text = $Text.Replace("`r`n", "`n").Replace("`r", "`n")
    foreach ($ch in $Text.ToCharArray()) {
        if ([int]$ch -gt 0x7F) { throw "HOMEEX-07 transform produced non-ASCII source: $Path" }
    }
    [System.IO.File]::WriteAllText($Path, $Text, [System.Text.Encoding]::ASCII)
}

function Replace-Once([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $count = ([regex]::Matches($Text, [regex]::Escape($Old))).Count
    if ($count -ne 1) { throw "HOMEEX-07 transform refused: '$Label' expected 1 match, found $count" }
    Write-Host "PASS exact transform anchor: $Label"
    return $Text.Replace($Old, $New)
}

function Replace-RegexCount([string]$Text, [string]$Pattern, [int]$Expected, [scriptblock]$Evaluator, [string]$Label) {
    $options = [System.Text.RegularExpressions.RegexOptions]::Multiline -bor [System.Text.RegularExpressions.RegexOptions]::Singleline
    $regex = [regex]::new($Pattern, $options)
    $count = $regex.Matches($Text).Count
    if ($count -ne $Expected) { throw "HOMEEX-07 transform refused: '$Label' expected $Expected matches, found $count" }
    Write-Host "PASS regex transform anchors: $Label ($count)"
    return $regex.Replace($Text, [System.Text.RegularExpressions.MatchEvaluator]$Evaluator)
}

foreach ($pair in @(
    @($controlPath, $expectedControlBlob, 'control'),
    @($tcpPath, $expectedTcpBlob, 'tcp'),
    @($diagnosticsPath, $expectedDiagnosticsBlob, 'diagnostics'))) {
    $actual = (& git -C $RepositoryRoot hash-object -- $pair[0]).Trim()
    if ($actual -ne $pair[1]) { throw "HOMEEX-07 transform refused: $($pair[2]) baseline blob $actual != $($pair[1])" }
    Write-Host "PASS pinned source blob: $($pair[2])=$actual"
}

$control = Read-Lf $controlPath
$tcp = Read-Lf $tcpPath
$diagnostics = Read-Lf $diagnosticsPath

# ---------------------------------------------------------------------------
# LMCControlCommandService: full non-group identity tail + OwnerKind 7.
# ---------------------------------------------------------------------------
$control = Replace-Once $control @'
#define LMC_OWNER_STATE_AXIS_OPERATION_MODE_ACTIVE 12
#define LMC_OWNER_KIND_DIRECT 1
'@ @'
#define LMC_OWNER_STATE_AXIS_OPERATION_MODE_ACTIVE 12
#define LMC_OWNER_STATE_DS402_HOME_EX_ACTIVE 13
#define LMC_OWNER_KIND_DIRECT 1
'@ 'reserve active state 13'

$control = Replace-Once $control @'
#define LMC_OWNER_KIND_AXIS_OPERATION_MODE        6
#define LMC_OWNER_RESOURCE_AXIS 1
'@ @'
#define LMC_OWNER_KIND_AXIS_OPERATION_MODE        6
#define LMC_OWNER_KIND_DS402_HOME_EX 7
#define LMC_OWNER_RESOURCE_AXIS 1
'@ 'define OwnerKind 7'

$control = Replace-Once $control @'
#define LMC_OWNER_IDENTITY_PREFIX_BYTES 0x00000040
#define LMC_OWNER_IDENTITY_SUFFIX_DINTS 314
'@ @'
#define LMC_OWNER_IDENTITY_PREFIX_BYTES 0x00000040
#define LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES 52
#define LMC_OWNER_IDENTITY_SUFFIX_DINTS 314
'@ 'define 52-byte per-axis tail slot'

$control = Replace-Once $control `
    'OwnerKind > LMC_OWNER_KIND_AXIS_OPERATION_MODE' `
    'OwnerKind > LMC_OWNER_KIND_DS402_HOME_EX' `
    'extend owner-kind range guard'

$control = Replace-Once $control @'
	elsif CommandId = 0x7D23 then
		identityShapeValid := IdentitySize = 56;
	elsif (CommandId = 0x7D15) | (CommandId = 0x7E53) then
'@ @'
	elsif CommandId = 0x7D1B then
		identityShapeValid := IdentitySize = 116;
	elsif CommandId = 0x7D23 then
		identityShapeValid := IdentitySize = 56;
	elsif (CommandId = 0x7D15) | (CommandId = 0x7E53) then
'@ 'add 116-byte Start identity shape'

$control = Replace-RegexCount $control 'identityTailSize\s*>\s*8' 6 {
    param($m)
    'identityTailSize > LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES'
} 'replace all 8-byte tail limits'

$control = Replace-RegexCount $control 'identityTailOffset\s*:=\s*TO_UDINT\((?<expr>[^;]+?)\)\s*\*\s*8\s*;' 12 {
    param($m)
    'identityTailOffset := TO_UDINT(' + $m.Groups['expr'].Value + ') * LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES;'
} 'replace all per-axis 8-byte tail offsets'

$control = Replace-Once $control @'
	elsif ResourceKind = LMC_OWNER_RESOURCE_DS402_HOME_ENGINE then
		if (OwnerKind <> LMC_OWNER_KIND_DS402_HOME) |
		   (AdmissionMode <> LMC_OWNER_ADMISSION_LIFECYCLE) |
		   (CommandId <> 0x7D15) | (referenceAxisMask = 0) |
		   (referenceAxisMask > 0x00000008) |
		   (RequestedAxisMask <> referenceAxisMask) then
			RETURN;
		end_if;
'@ @'
	elsif ResourceKind = LMC_OWNER_RESOURCE_DS402_HOME_ENGINE then
		if (((OwnerKind <> LMC_OWNER_KIND_DS402_HOME) |
		     (CommandId <> 0x7D15)) &
		    ((OwnerKind <> LMC_OWNER_KIND_DS402_HOME_EX) |
		     (CommandId <> 0x7D1B))) |
		   (AdmissionMode <> LMC_OWNER_ADMISSION_LIFECYCLE) |
		   (referenceAxisMask = 0) |
		   (referenceAxisMask > 0x00000008) |
		   (RequestedAxisMask <> referenceAxisMask) then
			RETURN;
		end_if;
'@ 'pair ResourceKind 3 legacy and HomeDS402Ex tuples'

# Every current OperationMode owner-kind branch is the final specific owner-kind
# branch before CASE ELSE. Duplicate the branch and substitute only the frozen
# HomeDS402Ex tuple/state/identity values.
$control = Replace-RegexCount $control '^(?<indent>[ \t]*)LMC_OWNER_KIND_AXIS_OPERATION_MODE:[ \t]*\n(?<body>.*?)(?=^\k<indent>else\b)' 10 {
    param($m)
    $indent = $m.Groups['indent'].Value
    $body = $m.Groups['body'].Value
    $copy = $body.Replace('LMC_OWNER_KIND_AXIS_OPERATION_MODE', 'LMC_OWNER_KIND_DS402_HOME_EX')
    $copy = $copy.Replace('LMC_OWNER_STATE_AXIS_OPERATION_MODE_ACTIVE', 'LMC_OWNER_STATE_DS402_HOME_EX_ACTIVE')
    $copy = $copy.Replace('LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE', 'LMC_OWNER_RESOURCE_DS402_HOME_ENGINE')
    $copy = $copy.Replace('0x7D23', '0x7D1B')
    $copy = [regex]::Replace($copy, '(?<![0-9])56(?![0-9])', '116')
    $m.Value + $indent + 'LMC_OWNER_KIND_DS402_HOME_EX:' + "`n" + $copy
} 'duplicate all lifecycle owner-kind switch branches'

# Three command switch sites enumerate 0x7D15 immediately before 0x7D23.
# Duplicate the exact legacy DS402 Home command branch, changing semantic owner
# identity but retaining shared ResourceKind 3.
$control = Replace-RegexCount $control '^(?<indent>[ \t]*)0x7D15:[ \t]*\n(?<body>.*?)(?=^\k<indent>0x7D23:)' 3 {
    param($m)
    $indent = $m.Groups['indent'].Value
    $body = $m.Groups['body'].Value
    $copy = $body.Replace('LMC_OWNER_KIND_DS402_HOME', 'LMC_OWNER_KIND_DS402_HOME_EX')
    $copy = $copy.Replace('LMC_OWNER_STATE_DS402_HOME_ACTIVE', 'LMC_OWNER_STATE_DS402_HOME_EX_ACTIVE')
    $copy = $copy.Replace('0x7D15', '0x7D1B')
    $copy = [regex]::Replace($copy, '(?<![0-9])72(?![0-9])', '116')
    $m.Value + $indent + '0x7D1B:' + "`n" + $copy
} 'duplicate all exact DS402 Home command-tuple branches'

# ---------------------------------------------------------------------------
# TCPMotionInterface: reserve exact HomeDS402Ex Start identity before dispatch.
# ---------------------------------------------------------------------------
$tcp = Replace-Once $tcp @'
		diagnosticsReserved : BOOL;
		diagnosticsDs402StartValid : BOOL;
		diagnosticsOperationModeStartValid : BOOL;
		diagnosticsDs402PreflightAttempted : BOOL;
'@ @'
		diagnosticsReserved : BOOL;
		diagnosticsDs402StartValid : BOOL;
		diagnosticsHomeExStartValid : BOOL;
		diagnosticsOperationModeStartValid : BOOL;
		diagnosticsHomeExSpareZero : BOOL;
		diagnosticsHomeExSpareIndex : DINT;
		diagnosticsHomeExMethod : DINT;
		diagnosticsDs402PreflightAttempted : BOOL;
'@ 'declare TCP HomeDS402Ex classifier state'

$tcp = Replace-Once $tcp @'
	  diagnosticsReserved := FALSE;
	  diagnosticsDs402StartValid := FALSE;
	  diagnosticsOperationModeStartValid := FALSE;
	  diagnosticsDs402PreflightAttempted := FALSE;
'@ @'
	  diagnosticsReserved := FALSE;
	  diagnosticsDs402StartValid := FALSE;
	  diagnosticsHomeExStartValid := FALSE;
	  diagnosticsOperationModeStartValid := FALSE;
	  diagnosticsHomeExSpareZero := TRUE;
	  diagnosticsHomeExSpareIndex := 0;
	  diagnosticsHomeExMethod := 0;
	  diagnosticsDs402PreflightAttempted := FALSE;
'@ 'initialize TCP HomeDS402Ex classifier state'

$tcp = Replace-Once $tcp @'
	  elsif (CommandID = 0x7D23) & (Payload = 56) then
		diagnosticsOwnerReference := AxisRef$UINT;
		diagnosticsOwnerKind := 6;
		diagnosticsResourceKind := 4;
	  elsif (CommandID = 0x7D15) & (Payload = 72) then
'@ @'
	  elsif (CommandID = 0x7D1B) & (Payload = 116) then
		diagnosticsOwnerReference := AxisRef$UINT;
		diagnosticsOwnerKind := 7;
		diagnosticsResourceKind := 3;
	  elsif (CommandID = 0x7D23) & (Payload = 56) then
		diagnosticsOwnerReference := AxisRef$UINT;
		diagnosticsOwnerKind := 6;
		diagnosticsResourceKind := 4;
	  elsif (CommandID = 0x7D15) & (Payload = 72) then
'@ 'map TCP HomeDS402Ex Start to OwnerKind 7 / ResourceKind 3'

$tcp = Replace-Once $tcp @'
	  if (CommandID = 0x7D23) & (Payload = 56) &
		 (diagnosticsAxisMask <> 0) then
'@ @'
	  if (CommandID = 0x7D1B) & (Payload = 116) &
		 (diagnosticsAxisMask <> 0) then
		diagnosticsHomeExSpareZero := TRUE;
		diagnosticsHomeExSpareIndex := 88;
		while diagnosticsHomeExSpareIndex <= 119 do
		  if RequestBuf[diagnosticsHomeExSpareIndex] <> 0 then
			diagnosticsHomeExSpareZero := FALSE;
		  end_if;
		  diagnosticsHomeExSpareIndex += 1;
		end_while;
		diagnosticsHomeExMethod := RequestBuf[44]$DINT;
		diagnosticsHomeExStartValid :=
		  (RequestBuf[8]$UINT = 1) &
		  (RequestBuf[10]$UINT = 0) &
		  (RequestBuf[12]$UDINT <> 0) &
		  (RequestBuf[16]$UDINT = 1) &
		  (RequestBuf[20]$UDINT <> 0) &
		  (RequestBuf[24]$UDINT <> 0) &
		  ((RequestBuf[28]$UDINT <> 0) |
		   (RequestBuf[32]$UDINT <> 0) |
		   (RequestBuf[36]$UDINT <> 0) |
		   (RequestBuf[40]$UDINT <> 0)) &
		  (((diagnosticsHomeExMethod >= 1) &
		    (diagnosticsHomeExMethod <= 14)) |
		   ((diagnosticsHomeExMethod >= 17) &
		    (diagnosticsHomeExMethod <= 30)) |
		   ((diagnosticsHomeExMethod >= 33) &
		    (diagnosticsHomeExMethod <= 34))) &
		  (RequestBuf[48]$UDINT <> 0x80000000) &
		  (RequestBuf[76]$UINT = 1) &
		  (RequestBuf[78]$UINT = 0) &
		  (RequestBuf[80]$UDINT <> 0) &
		  (RequestBuf[84]$UDINT <> 0) &
		  diagnosticsHomeExSpareZero &
		  (RequestBuf[120]$UDINT = 0x58453448);
	  end_if;
	  if (CommandID = 0x7D23) & (Payload = 56) &
		 (diagnosticsAxisMask <> 0) then
'@ 'add strict TCP HomeDS402Ex pre-admission classifier'

$tcp = Replace-Once $tcp @'
		 ((CommandID = 0x7E53) |
		  diagnosticsOperationModeStartValid |
		  (diagnosticsDs402StartValid &
'@ @'
		 ((CommandID = 0x7E53) |
		  diagnosticsHomeExStartValid |
		  diagnosticsOperationModeStartValid |
		  (diagnosticsDs402StartValid &
'@ 'include HomeDS402Ex in ownership admission'

$tcp = Replace-Once $tcp `
    'elsif diagnosticsDs402StartValid | diagnosticsOperationModeStartValid then' `
    'elsif diagnosticsDs402StartValid | diagnosticsHomeExStartValid | diagnosticsOperationModeStartValid then' `
    'fail closed if HomeDS402Ex ownership services are unavailable'

$tcp = Replace-Once $tcp @'
    if (diagnosticsDs402StartValid | diagnosticsOperationModeStartValid) &
'@ @'
    if (diagnosticsDs402StartValid | diagnosticsHomeExStartValid |
        diagnosticsOperationModeStartValid) &
'@ 'produce deterministic owner-admission failure for HomeDS402Ex'

$tcp = Replace-Once $tcp @'
    if (((diagnosticsDs402StartValid = FALSE) &
		  (diagnosticsOperationModeStartValid = FALSE)) |
		  (diagnosticsDs402StartValid & diagnosticsDs402PreflightAccepted &
		   (diagnosticsAdmissionResult = 0)) |
		  (diagnosticsOperationModeStartValid &
		   (diagnosticsAdmissionResult = 0))) &
'@ @'
    if (((diagnosticsDs402StartValid = FALSE) &
		  (diagnosticsHomeExStartValid = FALSE) &
		  (diagnosticsOperationModeStartValid = FALSE)) |
		  (diagnosticsDs402StartValid & diagnosticsDs402PreflightAccepted &
		   (diagnosticsAdmissionResult = 0)) |
		  (diagnosticsHomeExStartValid &
		   (diagnosticsAdmissionResult = 0)) |
		  (diagnosticsOperationModeStartValid &
		   (diagnosticsAdmissionResult = 0))) &
'@ 'require HomeDS402Ex reservation before diagnostics dispatch'

$tcp = Replace-Once $tcp @'
		elsif CommandID = 0x7D15 then
		  diagnosticsExactAccepted :=
'@ @'
		elsif CommandID = 0x7D1B then
		  // HOMEEX-07 remains gate-OFF. Any apparent success is not accepted.
		  diagnosticsExactAccepted := FALSE;
		  diagnosticsExactFailure :=
			(diagnosticsResponseSize = 24) &
			(Sendbuf[8]$UINT = 1) &
			(Sendbuf[12]$UINT = 1) &
			(Sendbuf[14]$INT = -31000) &
			(Sendbuf[16]$UDINT = RequestBuf[12]$UDINT) &
			(Sendbuf[20]$UDINT <> 0) &
			(Sendbuf[24]$DINT = RequestBuf[44]$DINT) &
			(Sendbuf[28]$UDINT = 0);
		elsif CommandID = 0x7D15 then
		  diagnosticsExactAccepted :=
'@ 'recognize exact HomeDS402Ex deterministic failure'

# ---------------------------------------------------------------------------
# LMCDiagnosticsService: validate exact reservation, then stay gate-OFF.
# ---------------------------------------------------------------------------
$diagnostics = Replace-Once $diagnostics @'
#define LMC_DIAG_OWNER_KIND_DS402_HOME 4
#define LMC_DIAG_RESOURCE_DIAGNOSTICS_SDO 4
'@ @'
#define LMC_DIAG_OWNER_KIND_DS402_HOME 4
#define LMC_DIAG_OWNER_KIND_DS402_HOME_EX 7
#define LMC_DIAG_RESOURCE_DIAGNOSTICS_SDO 4
'@ 'define diagnostics HomeDS402Ex OwnerKind 7'

$diagnostics = Replace-Once $diagnostics @'
		positionRaw : UDINT;
		spareIndex, recordIndex, recordBase : DINT;
		spareZero, methodCandidate, recordDirty : BOOL;
'@ @'
		positionRaw : UDINT;
		axisMask : UDINT;
		spareIndex, recordIndex, recordBase : DINT;
		ownerResult, rollbackResult : DINT;
		spareZero, methodCandidate, recordDirty : BOOL;
'@ 'add HomeDS402Ex ownership validation locals'

$diagnostics = Replace-Once $diagnostics @'
		elsif (methodCandidate = FALSE) | (positionRaw = 0x80000000) then
			detailCode := LMC_DIAG_HOMEEX_DETAIL_INVALID_PROFILE;
		elsif (AdmissionToken <> 0) | (OwnerGeneration <> 0) then
			// HOMEEX-06 has no ownership reservation. HOMEEX-07 adds the full
			// 116-byte owner identity before any Start crosses that boundary.
			detailCode := 42;
		else
			recordBase := TO_DINT(Reference - 1) * LMC_DIAG_HOMEEX_RECORD_STRIDE;
			recordDirty := FALSE;
			recordIndex := 0;
			while recordIndex < LMC_DIAG_HOMEEX_RECORD_STRIDE do
				if Ds402HomeExState[recordBase + recordIndex] <> 0 then
					recordDirty := TRUE;
				end_if;
				recordIndex += 1;
			end_while;
			if recordDirty then
				detailCode := LMC_DIAG_HOMEEX_DETAIL_STORE_CORRUPT;
			elsif LMC_DIAG_DS402_HOME_EX_ENABLED = FALSE then
				detailCode := LMC_DIAG_HOMEEX_DETAIL_INVALID_PROFILE;
			else
				// HOMEEX-06 remains non-executable even if the private gate is
				// edited. HOMEEX-07/08 must replace this deterministic failure.
				detailCode := LMC_DIAG_HOMEEX_DETAIL_STORAGE;
			end_if;
		end_if;
'@ @'
		elsif (methodCandidate = FALSE) | (positionRaw = 0x80000000) then
			detailCode := LMC_DIAG_HOMEEX_DETAIL_INVALID_PROFILE;
		elsif (AdmissionToken = 0) | (OwnerGeneration = 0) |
		      (IsClientConnected(#AxisOwnership) = FALSE) then
			detailCode := 42;
		else
			axisMask := TO_UDINT(1) shl TO_UDINT(Reference - 1);
			ownerResult := AxisOwnership.ValidateAxisOwnershipIdentity(
				CommandId:=0x7D1B,
				Reference:=Reference,
				ExpectedAxisMask:=axisMask,
				OwnerKind:=LMC_DIAG_OWNER_KIND_DS402_HOME_EX,
				ResourceKind:=LMC_DIAG_RESOURCE_DS402_HOME,
				AdmissionMode:=LMC_DIAG_ADMISSION_LIFECYCLE,
				CallerSessionEpoch:=CallerSessionEpoch,
				RequestSequence:=RequestSequence,
				AdmissionToken:=AdmissionToken,
				OwnerGeneration:=OwnerGeneration,
				RequiredPhase:=LMC_DIAG_OWNER_PHASE_RESERVED,
				pIdentity:=pRequest$^void,
				IdentitySize:=RequestSize);
			if ownerResult <> 0 then
				detailCode := 42;
			else
				recordBase := TO_DINT(Reference - 1) * LMC_DIAG_HOMEEX_RECORD_STRIDE;
				recordDirty := FALSE;
				recordIndex := 0;
				while recordIndex < LMC_DIAG_HOMEEX_RECORD_STRIDE do
					if Ds402HomeExState[recordBase + recordIndex] <> 0 then
						recordDirty := TRUE;
					end_if;
					recordIndex += 1;
				end_while;
				if recordDirty then
					detailCode := LMC_DIAG_HOMEEX_DETAIL_STORE_CORRUPT;
				elsif LMC_DIAG_DS402_HOME_EX_ENABLED = FALSE then
					detailCode := LMC_DIAG_HOMEEX_DETAIL_INVALID_PROFILE;
				else
					// HOMEEX-07 proves ownership only. HOMEEX-08 adds execution.
					detailCode := LMC_DIAG_HOMEEX_DETAIL_STORAGE;
				end_if;
			end_if;
		end_if;
'@ 'validate exact HomeDS402Ex reservation before deterministic gate-OFF failure'

$diagnostics = Replace-Once $diagnostics @'
	(pResponse + 12)^$UDINT := detailCode;
	if detailCode >= 16 then
'@ @'
	rollbackResult := 0;
	if (detailCode <> 0) & (AdmissionToken <> 0) &
	   (OwnerGeneration <> 0) & (CallerSessionEpoch <> 0) &
	   (RequestSequence <> 0) & IsClientConnected(#AxisOwnership) then
		rollbackResult := AxisOwnership.RollbackAxisOwnership(
			AdmissionToken:=AdmissionToken,
			OwnerGeneration:=OwnerGeneration,
			CallerSessionEpoch:=CallerSessionEpoch,
			RequestSequence:=RequestSequence,
			Reason:=0);
	end_if;

	(pResponse + 12)^$UDINT := detailCode;
	if detailCode >= 16 then
'@ 'rollback HomeDS402Ex reservation on deterministic gate-OFF rejection'

$diagnostics = Replace-Once $diagnostics @'
FUNCTION LMCDiagnosticsService::ProcessAxisDs402HomeEx
	// HOMEEX-06 is a parser/outcome scaffold only. No ownership, SDO, RT
	// mailbox, controlword, mode, setpoint or motion transition is permitted.
	RETURN;
'@ @'
FUNCTION LMCDiagnosticsService::ProcessAxisDs402HomeEx
	// HOMEEX-07 adds exact ownership only. SDO, RT mailbox, controlword,
	// mode, setpoint and motion execution remain forbidden until HOMEEX-08.
	RETURN;
'@ 'advance no-op processor comment to HOMEEX-07 boundary'

Write-AsciiLf $controlPath $control
Write-AsciiLf $tcpPath $tcp
Write-AsciiLf $diagnosticsPath $diagnostics

$changed = @(& git -C $RepositoryRoot diff --name-only)
$expected = @(
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st')
if ($changed.Count -ne 3) { throw "HOMEEX-07 transform changed $($changed.Count) files instead of 3: $($changed -join ', ')" }
foreach ($path in $expected) {
    if (-not ($changed -contains $path)) { throw "HOMEEX-07 transform missing expected source change: $path" }
}

& git -C $RepositoryRoot diff --check
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host 'HOMEEX-07 exact ownership source transform PASS.'
