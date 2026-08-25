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

$tcpRelative = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'
$diagnosticsRelative = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'
$tcpPath = Join-Path $RepositoryRoot ($tcpRelative -replace '/', '\')
$diagnosticsPath = Join-Path $RepositoryRoot ($diagnosticsRelative -replace '/', '\')

$expectedTcpBlob = 'a46c570e45e210266dde7d02255c4807867e5316'
$expectedDiagnosticsBlob = '1a63ed6b0b7bfe94ed978ba3666f4c2d1e4af6da'

function Require-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        throw "HOMEEX-06 transform refused: $Message"
    }
    Write-Host "PASS $Message"
}

function Get-TrackedBlobSha {
    param([string]$RelativePath)
    $sha = (& git -C $RepositoryRoot rev-parse ("HEAD:" + $RelativePath) 2>$null)
    if ($LASTEXITCODE -ne 0) {
        throw "HOMEEX-06 transform refused: cannot resolve tracked blob for $RelativePath"
    }
    return ($sha | Select-Object -First 1).Trim()
}

function Read-AsciiLf {
    param([string]$Path)
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    foreach ($value in $bytes) {
        if ($value -gt 0x7F) {
            throw "HOMEEX-06 transform refused: non-ASCII byte in $Path"
        }
    }
    $text = [System.Text.Encoding]::ASCII.GetString($bytes)
    return $text.Replace("`r`n", "`n").Replace("`r", "`n")
}

function Replace-Once {
    param(
        [string]$Text,
        [string]$Old,
        [string]$New,
        [string]$Label
    )
    $count = ([regex]::Matches($Text, [regex]::Escape($Old))).Count
    if ($count -ne 1) {
        throw "HOMEEX-06 transform refused: '$Label' expected one match, found $count"
    }
    Write-Host "PASS exact anchor: $Label"
    return $Text.Replace($Old, $New)
}

function Write-AsciiLf {
    param([string]$Path, [string]$Text)
    Require-True (-not $Text.Contains("`r")) ("LF-only transformed text: " + $Path)
    foreach ($character in $Text.ToCharArray()) {
        if ([int]$character -gt 0x7F) {
            throw "HOMEEX-06 transform refused: generated non-ASCII character in $Path"
        }
    }
    $encoding = New-Object System.Text.ASCIIEncoding
    [System.IO.File]::WriteAllText($Path, $Text, $encoding)
}

Require-True (Test-Path -LiteralPath $tcpPath) 'TCPMotionInterface tracked source exists'
Require-True (Test-Path -LiteralPath $diagnosticsPath) 'LMCDiagnosticsService tracked source exists'
Require-True ((Get-TrackedBlobSha $tcpRelative) -eq $expectedTcpBlob) 'TCPMotionInterface baseline blob is exact'
Require-True ((Get-TrackedBlobSha $diagnosticsRelative) -eq $expectedDiagnosticsBlob) 'LMCDiagnosticsService baseline blob is exact'

$tcp = Read-AsciiLf $tcpPath
$diagnostics = Read-AsciiLf $diagnosticsPath

$tcp = Replace-Once $tcp @'
  0x7D15, 0x7D16, 0x7D17,
  0x7D23, 0x7D24, 0x7D25,
'@ @'
  0x7D15, 0x7D16, 0x7D17,
  0x7D1B, 0x7D1C, 0x7D1D,
  0x7D23, 0x7D24, 0x7D25,
'@ 'route HomeDS402Ex lifecycle through diagnostics without ownership admission'

$diagnostics = Replace-Once $diagnostics @'
		Ds402HomeState : ARRAY [0..127] OF DINT;

		EncoderMaintenanceState : ARRAY [0..191] OF DINT;
'@ @'
		Ds402HomeState : ARRAY [0..127] OF DINT;

		Ds402HomeExState : ARRAY [0..255] OF DINT;

		EncoderMaintenanceState : ARRAY [0..191] OF DINT;
'@ 'declare dedicated HomeDS402Ex scaffold state'

$homeExDeclarations = @'
	FUNCTION HandleAxisDs402HomeExStart
		VAR_INPUT
			Reference 	: UINT;
			pRequest 	: ^USINT;
			pResponse 	: ^USINT;
			ResponseCapacity 	: UDINT;
			CallerSessionEpoch 	: UDINT;
			RequestSequence 	: UDINT;
			AdmissionToken 	: UDINT;
			OwnerGeneration 	: UDINT;
			RequestSize 	: UDINT;
		END_VAR
		VAR_OUTPUT
			ResponseSize 	: DINT;
		END_VAR;

	FUNCTION HandleAxisDs402HomeExOutcome
		VAR_INPUT
			Reference 	: UINT;
			pRequest 	: ^USINT;
			pResponse 	: ^USINT;
			ResponseCapacity 	: UDINT;
			CallerSessionEpoch 	: UDINT;
			RequestSize 	: UDINT;
		END_VAR
		VAR_OUTPUT
			ResponseSize 	: DINT;
		END_VAR;

	FUNCTION HandleAxisDs402HomeExRetire
		VAR_INPUT
			Reference 	: UINT;
			pRequest 	: ^USINT;
			pResponse 	: ^USINT;
			ResponseCapacity 	: UDINT;
			CallerSessionEpoch 	: UDINT;
			RequestSize 	: UDINT;
		END_VAR
		VAR_OUTPUT
			ResponseSize 	: DINT;
		END_VAR;

	FUNCTION ProcessAxisDs402HomeEx;

'@
$diagnostics = Replace-Once $diagnostics "`tFUNCTION HandleAxisDs402HomeStart" ($homeExDeclarations + "`tFUNCTION HandleAxisDs402HomeStart") 'declare HomeDS402Ex handlers and dormant processor'

$diagnostics = Replace-Once $diagnostics @'
#define LMC_DIAG_DS402_HOME_ENABLED FALSE
// SetOperationMode runtime remains private until MODE-14 paired activation.
'@ @'
#define LMC_DIAG_DS402_HOME_ENABLED FALSE
// HomeDS402Ex parser/outcome scaffold is private until HOMEEX-07/08 runtime work.
#define LMC_DIAG_DS402_HOME_EX_ENABLED FALSE
#define LMC_DIAG_HOMEEX_RECORD_STRIDE 40
#define LMC_DIAG_HOMEEX_EXECUTE_TOKEN 0x58453448
#define LMC_DIAG_HOMEEX_DETAIL_NOT_FOUND 53
#define LMC_DIAG_HOMEEX_DETAIL_INDETERMINATE 54
#define LMC_DIAG_HOMEEX_DETAIL_STORE_CORRUPT 55
#define LMC_DIAG_HOMEEX_DETAIL_KEY_MISMATCH 56
#define LMC_DIAG_HOMEEX_DETAIL_STORAGE 57
#define LMC_DIAG_HOMEEX_DETAIL_EXECUTION 58
#define LMC_DIAG_HOMEEX_DETAIL_ABORTED 59
#define LMC_DIAG_HOMEEX_DETAIL_SLOT_OCCUPIED 60
#define LMC_DIAG_HOMEEX_DETAIL_INVALID_PROFILE 61
#define LMC_DIAG_HOMEEX_DETAIL_CLEANUP_INCOMPLETE 62
// SetOperationMode runtime remains private until MODE-14 paired activation.
'@ 'add frozen HomeDS402Ex gate and scaffold constants'

$diagnostics = Replace-Once $diagnostics @'
	ProcessEncoderMaintenance();
	ProcessAxisDs402Home();
	ProcessAxisSetOperationMode();
'@ @'
	ProcessEncoderMaintenance();
	ProcessAxisDs402Home();
	ProcessAxisDs402HomeEx();
	ProcessAxisSetOperationMode();
'@ 'pump dormant HomeDS402Ex processor before generic diagnostics work'

$diagnostics = Replace-Once $diagnostics @'
	_memset(dest:=#EncoderMaintenanceState[0], usByte:=0,
		cntr:=sizeof(EncoderMaintenanceState));
'@ @'
	_memset(dest:=#Ds402HomeExState[0], usByte:=0,
		cntr:=sizeof(Ds402HomeExState));
	_memset(dest:=#EncoderMaintenanceState[0], usByte:=0,
		cntr:=sizeof(EncoderMaintenanceState));
'@ 'zero dedicated HomeDS402Ex scaffold state at construction'

$diagnostics = Replace-Once $diagnostics @'
	// Home uses physical axis references 1..4, unlike diagnostics commands below.
	if CommandId = 0x7D15 then
'@ @'
	// Home lifecycles use physical axis references 1..4.
	if CommandId = 0x7D1B then
		ResponseSize := HandleAxisDs402HomeExStart(
			Reference:=Reference, pRequest:=pRequest, pResponse:=pResponse,
			ResponseCapacity:=ResponseCapacity,
			CallerSessionEpoch:=CallerSessionEpoch,
			RequestSequence:=RequestSequence,
			AdmissionToken:=AdmissionToken,
			OwnerGeneration:=OwnerGeneration,
			RequestSize:=RequestSize);
		RETURN;
	elsif CommandId = 0x7D1C then
		ResponseSize := HandleAxisDs402HomeExOutcome(
			Reference:=Reference, pRequest:=pRequest, pResponse:=pResponse,
			ResponseCapacity:=ResponseCapacity,
			CallerSessionEpoch:=CallerSessionEpoch, RequestSize:=RequestSize);
		RETURN;
	elsif CommandId = 0x7D1D then
		ResponseSize := HandleAxisDs402HomeExRetire(
			Reference:=Reference, pRequest:=pRequest, pResponse:=pResponse,
			ResponseCapacity:=ResponseCapacity,
			CallerSessionEpoch:=CallerSessionEpoch, RequestSize:=RequestSize);
		RETURN;
	elsif CommandId = 0x7D15 then
'@ 'route HomeDS402Ex commands inside diagnostics service'

$homeExImplementation = @'
FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeExStart
	VAR_INPUT
		Reference 	: UINT;
		pRequest 	: ^USINT;
		pResponse 	: ^USINT;
		ResponseCapacity 	: UDINT;
		CallerSessionEpoch 	: UDINT;
		RequestSequence 	: UDINT;
		AdmissionToken 	: UDINT;
		OwnerGeneration 	: UDINT;
		RequestSize 	: UDINT;
	END_VAR
	VAR_OUTPUT
		ResponseSize 	: DINT;
	END_VAR
	VAR
		schemaVersion, requestFlags, bufferMode, reservedValue : UINT;
		requestId, diagnosticsBuild, bootId, mapRevision : UDINT;
		intent0, intent1, intent2, intent3 : UDINT;
		overallTimeout, detectionTimeout, currentBootId : UDINT;
		detailCode : UDINT;
		homingMethod : DINT;
		positionRaw : UDINT;
		spareIndex, recordIndex, recordBase : DINT;
		spareZero, methodCandidate, recordDirty : BOOL;
	END_VAR

	ResponseSize := -1;
	if (pResponse = NIL) | (ResponseCapacity < 16) then
		RETURN;
	end_if;
	_memset(dest:=pResponse, usByte:=0, cntr:=ResponseCapacity);
	pResponse^$UINT := LMC_DIAG_SCHEMA_VERSION;
	(pResponse + 4)^$UINT := 1;
	(pResponse + 6)^$INT := LMC_DIAG_ADMIN_ERROR_ID;
	requestId := 0;
	homingMethod := 0;
	if (pRequest <> NIL) & (RequestSize >= 8) then
		requestId := (pRequest + 4)^$UDINT;
	end_if;
	if (pRequest <> NIL) & (RequestSize >= 40) then
		homingMethod := (pRequest + 36)^$DINT;
	end_if;
	(pResponse + 8)^$UDINT := requestId;
	ResponseSize := 16;

	if pRequest = NIL then
		detailCode := 5;
	elsif (Reference < 1) | (Reference > 4) then
		detailCode := 4;
	elsif RequestSize <> 116 then
		detailCode := 5;
	else
		schemaVersion := pRequest^$UINT;
		requestFlags := (pRequest + 2)^$UINT;
		diagnosticsBuild := (pRequest + 8)^$UDINT;
		bootId := (pRequest + 12)^$UDINT;
		mapRevision := (pRequest + 16)^$UDINT;
		intent0 := (pRequest + 20)^$UDINT;
		intent1 := (pRequest + 24)^$UDINT;
		intent2 := (pRequest + 28)^$UDINT;
		intent3 := (pRequest + 32)^$UDINT;
		homingMethod := (pRequest + 36)^$DINT;
		positionRaw := (pRequest + 40)^$UDINT;
		bufferMode := (pRequest + 68)^$UINT;
		reservedValue := (pRequest + 70)^$UINT;
		overallTimeout := (pRequest + 72)^$UDINT;
		detectionTimeout := (pRequest + 76)^$UDINT;
		spareZero := TRUE;
		spareIndex := 80;
		while spareIndex <= 111 do
			if (pRequest + TO_UDINT(spareIndex))^$USINT <> 0 then
				spareZero := FALSE;
			end_if;
			spareIndex += 1;
		end_while;
		methodCandidate :=
			((homingMethod >= 1) & (homingMethod <= 14)) |
			((homingMethod >= 17) & (homingMethod <= 30)) |
			((homingMethod >= 33) & (homingMethod <= 34));
		currentBootId := GetDiagnosticsBootId();

		if schemaVersion <> LMC_DIAG_SCHEMA_VERSION then
			detailCode := 1;
		elsif requestFlags <> 0 then
			detailCode := 2;
		elsif requestId = 0 then
			detailCode := 3;
		elsif diagnosticsBuild <> 1 then
			detailCode := 16;
		elsif (currentBootId = 0) | (bootId <> currentBootId) then
			detailCode := 17;
		elsif mapRevision <> LMC_DIAG_MAP_REVISION then
			detailCode := 18;
		elsif (CallerSessionEpoch = 0) | (RequestSequence = 0) then
			detailCode := 5;
		elsif ((intent0 = 0) & (intent1 = 0) &
		       (intent2 = 0) & (intent3 = 0)) |
		      (bufferMode <> 1) | (reservedValue <> 0) |
		      (overallTimeout = 0) | (detectionTimeout = 0) |
		      (spareZero = FALSE) |
		      ((pRequest + 112)^$UDINT <> LMC_DIAG_HOMEEX_EXECUTE_TOKEN) then
			detailCode := 5;
		elsif (methodCandidate = FALSE) | (positionRaw = 0x80000000) then
			detailCode := LMC_DIAG_HOMEEX_DETAIL_INVALID_PROFILE;
		elsif (AdmissionToken <> 0) | (OwnerGeneration <> 0) then
			// HOMEEX-06 has no ownership reservation. HOMEEX-07 adds the full
			// 116-byte owner identity before any Start can cross that boundary.
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
				// The scaffold remains non-executable even if this private gate is
				// edited accidentally. HOMEEX-07/08 must replace this failure path.
				detailCode := LMC_DIAG_HOMEEX_DETAIL_STORAGE;
			end_if;
		end_if;
	end_if;

	(pResponse + 12)^$UDINT := detailCode;
	if detailCode >= 16 then
		if ResponseCapacity < 24 then
			ResponseSize := -1;
			RETURN;
		end_if;
		(pResponse + 16)^$DINT := homingMethod;
		(pResponse + 20)^$UDINT := 0;
		ResponseSize := 24;
	end_if;

END_FUNCTION


FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeExOutcome
	VAR_INPUT
		Reference 	: UINT;
		pRequest 	: ^USINT;
		pResponse 	: ^USINT;
		ResponseCapacity 	: UDINT;
		CallerSessionEpoch 	: UDINT;
		RequestSize 	: UDINT;
	END_VAR
	VAR_OUTPUT
		ResponseSize 	: DINT;
	END_VAR
	VAR
		schemaVersion, requestFlags, bufferMode, reservedValue : UINT;
		requestId, diagnosticsBuild, bootId, mapRevision : UDINT;
		originalRequestId, intent0, intent1, intent2, intent3 : UDINT;
		overallTimeout, detectionTimeout, currentBootId : UDINT;
		detailCode : UDINT;
		spareIndex, recordIndex, recordBase : DINT;
		spareZero, recordDirty : BOOL;
	END_VAR

	ResponseSize := -1;
	if (pResponse = NIL) | (ResponseCapacity < 16) then
		RETURN;
	end_if;
	_memset(dest:=pResponse, usByte:=0, cntr:=ResponseCapacity);
	pResponse^$UINT := LMC_DIAG_SCHEMA_VERSION;
	(pResponse + 4)^$UINT := 1;
	(pResponse + 6)^$INT := LMC_DIAG_ADMIN_ERROR_ID;
	requestId := 0;
	if (pRequest <> NIL) & (RequestSize >= 8) then
		requestId := (pRequest + 4)^$UDINT;
	end_if;
	(pResponse + 8)^$UDINT := requestId;
	ResponseSize := 16;

	if pRequest = NIL then
		detailCode := 5;
	elsif (Reference < 1) | (Reference > 4) then
		detailCode := 4;
	elsif RequestSize <> 116 then
		detailCode := 5;
	else
		schemaVersion := pRequest^$UINT;
		requestFlags := (pRequest + 2)^$UINT;
		diagnosticsBuild := (pRequest + 8)^$UDINT;
		bootId := (pRequest + 12)^$UDINT;
		mapRevision := (pRequest + 16)^$UDINT;
		originalRequestId := (pRequest + 20)^$UDINT;
		intent0 := (pRequest + 24)^$UDINT;
		intent1 := (pRequest + 28)^$UDINT;
		intent2 := (pRequest + 32)^$UDINT;
		intent3 := (pRequest + 36)^$UDINT;
		bufferMode := (pRequest + 72)^$UINT;
		reservedValue := (pRequest + 74)^$UINT;
		overallTimeout := (pRequest + 76)^$UDINT;
		detectionTimeout := (pRequest + 80)^$UDINT;
		spareZero := TRUE;
		spareIndex := 84;
		while spareIndex <= 115 do
			if (pRequest + TO_UDINT(spareIndex))^$USINT <> 0 then
				spareZero := FALSE;
			end_if;
			spareIndex += 1;
		end_while;
		currentBootId := GetDiagnosticsBootId();

		if schemaVersion <> LMC_DIAG_SCHEMA_VERSION then
			detailCode := 1;
		elsif requestFlags <> 0 then
			detailCode := 2;
		elsif requestId = 0 then
			detailCode := 3;
		elsif diagnosticsBuild <> 1 then
			detailCode := 16;
		elsif (currentBootId = 0) | (bootId <> currentBootId) then
			detailCode := 17;
		elsif mapRevision <> LMC_DIAG_MAP_REVISION then
			detailCode := 18;
		elsif (CallerSessionEpoch = 0) | (originalRequestId = 0) |
		      ((intent0 = 0) & (intent1 = 0) &
		       (intent2 = 0) & (intent3 = 0)) |
		      (bufferMode <> 1) | (reservedValue <> 0) |
		      (overallTimeout = 0) | (detectionTimeout = 0) |
		      (spareZero = FALSE) then
			detailCode := 5;
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
			else
				detailCode := LMC_DIAG_HOMEEX_DETAIL_NOT_FOUND;
			end_if;
		end_if;
	end_if;
	(pResponse + 12)^$UDINT := detailCode;

END_FUNCTION


FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeExRetire
	VAR_INPUT
		Reference 	: UINT;
		pRequest 	: ^USINT;
		pResponse 	: ^USINT;
		ResponseCapacity 	: UDINT;
		CallerSessionEpoch 	: UDINT;
		RequestSize 	: UDINT;
	END_VAR
	VAR_OUTPUT
		ResponseSize 	: DINT;
	END_VAR
	VAR
		schemaVersion, requestFlags, bufferMode, reservedValue : UINT;
		requestId, diagnosticsBuild, bootId, mapRevision : UDINT;
		originalRequestId, intent0, intent1, intent2, intent3 : UDINT;
		overallTimeout, detectionTimeout, expectedGeneration, currentBootId : UDINT;
		detailCode : UDINT;
		spareIndex, recordIndex, recordBase : DINT;
		spareZero, recordDirty : BOOL;
	END_VAR

	ResponseSize := -1;
	if (pResponse = NIL) | (ResponseCapacity < 16) then
		RETURN;
	end_if;
	_memset(dest:=pResponse, usByte:=0, cntr:=ResponseCapacity);
	pResponse^$UINT := LMC_DIAG_SCHEMA_VERSION;
	(pResponse + 4)^$UINT := 1;
	(pResponse + 6)^$INT := LMC_DIAG_ADMIN_ERROR_ID;
	requestId := 0;
	if (pRequest <> NIL) & (RequestSize >= 8) then
		requestId := (pRequest + 4)^$UDINT;
	end_if;
	(pResponse + 8)^$UDINT := requestId;
	ResponseSize := 16;

	if pRequest = NIL then
		detailCode := 5;
	elsif (Reference < 1) | (Reference > 4) then
		detailCode := 4;
	elsif RequestSize <> 120 then
		detailCode := 5;
	else
		schemaVersion := pRequest^$UINT;
		requestFlags := (pRequest + 2)^$UINT;
		diagnosticsBuild := (pRequest + 8)^$UDINT;
		bootId := (pRequest + 12)^$UDINT;
		mapRevision := (pRequest + 16)^$UDINT;
		originalRequestId := (pRequest + 20)^$UDINT;
		intent0 := (pRequest + 24)^$UDINT;
		intent1 := (pRequest + 28)^$UDINT;
		intent2 := (pRequest + 32)^$UDINT;
		intent3 := (pRequest + 36)^$UDINT;
		bufferMode := (pRequest + 72)^$UINT;
		reservedValue := (pRequest + 74)^$UINT;
		overallTimeout := (pRequest + 76)^$UDINT;
		detectionTimeout := (pRequest + 80)^$UDINT;
		expectedGeneration := (pRequest + 116)^$UDINT;
		spareZero := TRUE;
		spareIndex := 84;
		while spareIndex <= 115 do
			if (pRequest + TO_UDINT(spareIndex))^$USINT <> 0 then
				spareZero := FALSE;
			end_if;
			spareIndex += 1;
		end_while;
		currentBootId := GetDiagnosticsBootId();

		if schemaVersion <> LMC_DIAG_SCHEMA_VERSION then
			detailCode := 1;
		elsif requestFlags <> 0 then
			detailCode := 2;
		elsif requestId = 0 then
			detailCode := 3;
		elsif diagnosticsBuild <> 1 then
			detailCode := 16;
		elsif (currentBootId = 0) | (bootId <> currentBootId) then
			detailCode := 17;
		elsif mapRevision <> LMC_DIAG_MAP_REVISION then
			detailCode := 18;
		elsif (CallerSessionEpoch = 0) | (originalRequestId = 0) |
		      ((intent0 = 0) & (intent1 = 0) &
		       (intent2 = 0) & (intent3 = 0)) |
		      (bufferMode <> 1) | (reservedValue <> 0) |
		      (overallTimeout = 0) | (detectionTimeout = 0) |
		      (spareZero = FALSE) | (expectedGeneration = 0) then
			detailCode := 5;
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
			else
				detailCode := LMC_DIAG_HOMEEX_DETAIL_NOT_FOUND;
			end_if;
		end_if;
	end_if;
	(pResponse + 12)^$UDINT := detailCode;

END_FUNCTION


FUNCTION LMCDiagnosticsService::ProcessAxisDs402HomeEx
	// HOMEEX-06 is a parser/outcome scaffold only. No ownership, SDO, RT
	// mailbox, controlword, mode, setpoint or motion transition is permitted.
	RETURN;

END_FUNCTION


'@
$diagnostics = Replace-Once $diagnostics "FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeStart" ($homeExImplementation + "FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeStart") 'insert fail-closed HomeDS402Ex parser/outcome scaffold'

Write-AsciiLf $tcpPath $tcp
Write-AsciiLf $diagnosticsPath $diagnostics

$changed = @(& git -C $RepositoryRoot diff --name-only -- $tcpRelative $diagnosticsRelative)
Require-True ($changed.Count -eq 2) 'exactly two LASAL tracked sources changed'
Require-True ($changed -contains $tcpRelative) 'TCPMotionInterface is in transformed diff'
Require-True ($changed -contains $diagnosticsRelative) 'LMCDiagnosticsService is in transformed diff'

Write-Host 'HOMEEX-06 exact scaffold transform completed; activation remains OFF.'
