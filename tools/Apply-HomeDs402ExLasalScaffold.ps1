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
$fragmentRelative = 'tools/HomeDs402ExLasalScaffold.fragment.txt'
$tcpPath = Join-Path $RepositoryRoot ($tcpRelative -replace '/', '\')
$diagnosticsPath = Join-Path $RepositoryRoot ($diagnosticsRelative -replace '/', '\')
$fragmentPath = Join-Path $RepositoryRoot ($fragmentRelative -replace '/', '\')

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
    $Text = $Text.Replace("`r`n", "`n").Replace("`r", "`n")
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
Require-True (Test-Path -LiteralPath $fragmentPath) 'HomeDS402Ex scaffold fragment exists'
Require-True ((Get-TrackedBlobSha $tcpRelative) -eq $expectedTcpBlob) 'TCPMotionInterface baseline blob is exact'
Require-True ((Get-TrackedBlobSha $diagnosticsRelative) -eq $expectedDiagnosticsBlob) 'LMCDiagnosticsService baseline blob is exact'

$tcp = Read-AsciiLf $tcpPath
$diagnostics = Read-AsciiLf $diagnosticsPath
$fragment = Read-AsciiLf $fragmentPath
Require-True ($fragment.StartsWith('FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeExStart')) 'fragment begins at HomeDS402Ex Start handler'
Require-True ($fragment.Contains('FUNCTION LMCDiagnosticsService::ProcessAxisDs402HomeEx')) 'fragment contains dormant processor'

$oldTcpRoute = '  0x7D15, 0x7D16, 0x7D17,'
$newTcpRoute = $oldTcpRoute + "`n  0x7D1B, 0x7D1C, 0x7D1D,"
$tcp = Replace-Once $tcp $oldTcpRoute $newTcpRoute 'route HomeDS402Ex lifecycle through diagnostics without ownership admission'

$oldState = "`t`tDs402HomeState : ARRAY [0..127] OF DINT;"
$newState = $oldState + "`n`n`t`tDs402HomeExState : ARRAY [0..255] OF DINT;"
$diagnostics = Replace-Once $diagnostics $oldState $newState 'declare dedicated HomeDS402Ex scaffold state'

$declarations = @'
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
$functionDeclarationAnchor = "`tFUNCTION HandleAxisDs402HomeStart"
$diagnostics = Replace-Once $diagnostics $functionDeclarationAnchor ($declarations + $functionDeclarationAnchor) 'declare HomeDS402Ex handlers and dormant processor'

$oldGate = '#define LMC_DIAG_DS402_HOME_ENABLED FALSE'
$newGate = $oldGate + @'

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
'@
$diagnostics = Replace-Once $diagnostics $oldGate $newGate 'add frozen HomeDS402Ex gate and scaffold constants'

$oldPump = "`tProcessAxisDs402Home();"
$newPump = $oldPump + "`n`tProcessAxisDs402HomeEx();"
$diagnostics = Replace-Once $diagnostics $oldPump $newPump 'pump dormant HomeDS402Ex processor before generic diagnostics work'

$oldInit = "`t_memset(dest:=#EncoderMaintenanceState[0], usByte:=0,"
$newInit = "`t_memset(dest:=#Ds402HomeExState[0], usByte:=0,`n`t`tcntr:=sizeof(Ds402HomeExState));`n" + $oldInit
$diagnostics = Replace-Once $diagnostics $oldInit $newInit 'zero dedicated HomeDS402Ex scaffold state at construction'

$oldRequestRoute = "`t// Home uses physical axis references 1..4, unlike diagnostics commands below.`n`tif CommandId = 0x7D15 then"
$newRequestRoute = @'
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
'@
$diagnostics = Replace-Once $diagnostics $oldRequestRoute $newRequestRoute 'route HomeDS402Ex commands inside diagnostics service'

$implementationAnchor = 'FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeStart'
$diagnostics = Replace-Once $diagnostics $implementationAnchor ($fragment + $implementationAnchor) 'insert fail-closed HomeDS402Ex parser/outcome scaffold'

Write-AsciiLf $tcpPath $tcp
Write-AsciiLf $diagnosticsPath $diagnostics

$changed = @(& git -C $RepositoryRoot diff --name-only -- $tcpRelative $diagnosticsRelative)
Require-True ($changed.Count -eq 2) 'exactly two LASAL tracked sources changed'
Require-True ($changed -contains $tcpRelative) 'TCPMotionInterface is in transformed diff'
Require-True ($changed -contains $diagnosticsRelative) 'LMCDiagnosticsService is in transformed diff'

Write-Host 'HOMEEX-06 exact scaffold transform completed; activation remains OFF.'
