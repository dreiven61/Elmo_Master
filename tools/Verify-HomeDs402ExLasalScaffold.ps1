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

$script:PassCount = 0
$tcpPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
$diagnosticsPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
$controlPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'

function Require-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        throw "HOMEEX-06 scaffold verification failed: $Message"
    }
    Write-Host "PASS $Message"
    $script:PassCount++
}

function Require-Regex {
    param([string]$Text, [string]$Pattern, [string]$Message)
    $options = [System.Text.RegularExpressions.RegexOptions]::Multiline
    Require-True ([regex]::IsMatch($Text, $Pattern, $options)) $Message
}

function Require-AbsentRegex {
    param([string]$Text, [string]$Pattern, [string]$Message)
    $options = [System.Text.RegularExpressions.RegexOptions]::Multiline
    Require-True (-not [regex]::IsMatch($Text, $Pattern, $options)) $Message
}

function Get-FunctionBody {
    param([string]$Text, [string]$Name)
    $pattern = 'FUNCTION\s+(?:GLOBAL\s+)?LMCDiagnosticsService::' +
        [regex]::Escape($Name) + '(?s).*?END_FUNCTION'
    $matches = [regex]::Matches($Text, $pattern)
    Require-True ($matches.Count -eq 1) ("exact function definition: " + $Name)
    return $matches[0].Value
}

function Require-AsciiFile {
    param([string]$Path, [string]$Label)
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $ascii = $true
    foreach ($value in $bytes) {
        if ($value -gt 0x7F) {
            $ascii = $false
            break
        }
    }
    Require-True $ascii ($Label + ' remains 7-bit ASCII')
}

foreach ($path in @($tcpPath, $diagnosticsPath, $controlPath)) {
    Require-True (Test-Path -LiteralPath $path) ("required source exists: " + $path)
}

$tcp = Get-Content -LiteralPath $tcpPath -Raw
$diagnostics = Get-Content -LiteralPath $diagnosticsPath -Raw
$control = Get-Content -LiteralPath $controlPath -Raw

Require-Regex $tcp '0x7D15\s*,\s*0x7D16\s*,\s*0x7D17' 'legacy HomeDS402 lifecycle route remains present'
Require-Regex $tcp '0x7D1B\s*,\s*0x7D1C\s*,\s*0x7D1D' 'HomeDS402Ex lifecycle route is present'
Require-Regex $tcp '0x7D23\s*,\s*0x7D24\s*,\s*0x7D25' 'SetOperationMode lifecycle route remains present'
Require-True (([regex]::Matches($tcp, '0x7D1B')).Count -eq 1) 'TCP HomeDS402Ex Start id appears only in the diagnostics route, not ownership admission'
Require-True (([regex]::Matches($tcp, '0x7D1C')).Count -eq 1) 'TCP HomeDS402Ex Outcome id appears only in the diagnostics route'
Require-True (([regex]::Matches($tcp, '0x7D1D')).Count -eq 1) 'TCP HomeDS402Ex Retire id appears only in the diagnostics route'

Require-Regex $diagnostics 'Ds402HomeExState\s*:\s*ARRAY\s*\[0\.\.255\]\s*OF\s*DINT\s*;' 'dedicated HomeDS402Ex state store is declared'
Require-Regex $diagnostics '^#define LMC_DIAG_DS402_HOME_EX_ENABLED FALSE$' 'HomeDS402Ex runtime gate is exactly OFF'
Require-Regex $diagnostics '^#define LMC_DIAG_HOMEEX_RECORD_STRIDE 40$' 'HomeDS402Ex record stride is frozen at 40 DINTs'
Require-Regex $diagnostics '^#define LMC_DIAG_HOMEEX_EXECUTE_TOKEN 0x58453448$' 'HomeDS402Ex execute token is frozen'
foreach ($detail in 53..62) {
    Require-Regex $diagnostics ("^#define LMC_DIAG_HOMEEX_DETAIL_[A-Z_]+ " + $detail + '$') ("HomeDS402Ex detail " + $detail + ' is declared')
}

foreach ($name in @(
    'HandleAxisDs402HomeExStart',
    'HandleAxisDs402HomeExOutcome',
    'HandleAxisDs402HomeExRetire',
    'ProcessAxisDs402HomeEx')) {
    Require-Regex $diagnostics ("FUNCTION " + $name) ("class declaration exists: " + $name)
}

Require-Regex $diagnostics 'CommandId\s*=\s*0x7D1B[\s\S]*?HandleAxisDs402HomeExStart' 'HandleRequest routes HomeDS402Ex Start'
Require-Regex $diagnostics 'CommandId\s*=\s*0x7D1C[\s\S]*?HandleAxisDs402HomeExOutcome' 'HandleRequest routes HomeDS402Ex Outcome'
Require-Regex $diagnostics 'CommandId\s*=\s*0x7D1D[\s\S]*?HandleAxisDs402HomeExRetire' 'HandleRequest routes HomeDS402Ex Retire'
Require-Regex $diagnostics 'ProcessAxisDs402Home\(\);\s*ProcessAxisDs402HomeEx\(\);\s*ProcessAxisSetOperationMode\(\);' 'ProcessOperations pumps dormant HomeDS402Ex between Home and SetOperationMode'
Require-Regex $diagnostics '_memset\(dest:=#Ds402HomeExState\[0\],\s*usByte:=0,\s*cntr:=sizeof\(Ds402HomeExState\)\);' 'constructor clears HomeDS402Ex scaffold state'

$startBody = Get-FunctionBody $diagnostics 'HandleAxisDs402HomeExStart'
$outcomeBody = Get-FunctionBody $diagnostics 'HandleAxisDs402HomeExOutcome'
$retireBody = Get-FunctionBody $diagnostics 'HandleAxisDs402HomeExRetire'
$processBody = Get-FunctionBody $diagnostics 'ProcessAxisDs402HomeEx'
$homeExBodies = $startBody + "`n" + $outcomeBody + "`n" + $retireBody + "`n" + $processBody

Require-Regex $startBody 'RequestSize\s*<>\s*116' 'Start requires exact 116-byte payload'
Require-Regex $startBody 'spareIndex\s*:=\s*80[\s\S]*?spareIndex\s*<=\s*111' 'Start validates all 32 spare bytes'
Require-Regex $startBody 'LMC_DIAG_HOMEEX_EXECUTE_TOKEN' 'Start validates H4EX execute token'
Require-Regex $startBody 'bufferMode\s*<>\s*1' 'Start remains Aborting-only'
Require-Regex $startBody 'positionRaw\s*=\s*0x80000000' 'Start rejects unrepresentable final-position negation'
Require-Regex $startBody 'LMC_DIAG_HOMEEX_DETAIL_INVALID_PROFILE' 'well-shaped dormant Start fails with HomeDS402Ex profile/activation detail'
Require-Regex $startBody 'AdmissionToken\s*<>\s*0' 'HOMEEX-06 rejects any unexpected ownership admission token'
Require-Regex $startBody 'OwnerGeneration\s*<>\s*0' 'HOMEEX-06 rejects any unexpected ownership generation'
Require-Regex $startBody '\(pResponse \+ 20\)\^\$UDINT\s*:=\s*0' 'Start domain failure keeps NativeCommandState zero'

Require-Regex $outcomeBody 'RequestSize\s*<>\s*116' 'Outcome query requires exact 116-byte payload'
Require-Regex $outcomeBody 'spareIndex\s*:=\s*84[\s\S]*?spareIndex\s*<=\s*115' 'Outcome query validates all 32 spare bytes'
Require-Regex $outcomeBody 'LMC_DIAG_HOMEEX_DETAIL_NOT_FOUND' 'empty outcome slot is reported as not found'
Require-Regex $outcomeBody 'LMC_DIAG_HOMEEX_DETAIL_STORE_CORRUPT' 'unexpected nonzero scaffold slot is reported corrupt'

Require-Regex $retireBody 'RequestSize\s*<>\s*120' 'Retire requires exact 120-byte payload'
Require-Regex $retireBody 'expectedGeneration\s*=\s*0' 'Retire rejects zero expected generation'
Require-Regex $retireBody 'spareIndex\s*:=\s*84[\s\S]*?spareIndex\s*<=\s*115' 'Retire validates all 32 spare bytes'
Require-Regex $retireBody 'LMC_DIAG_HOMEEX_DETAIL_NOT_FOUND' 'Retire cannot invent a missing terminal record'

Require-AbsentRegex $homeExBodies 'AxisOwnership\.' 'HOMEEX-06 handlers do not call ownership service'
Require-AbsentRegex $homeExBodies 'SdoAxis[1-4]\.' 'HOMEEX-06 handlers do not call SDO executors'
Require-AbsentRegex $homeExBodies 'InputLatch\.' 'HOMEEX-06 handlers do not consume RT latch state'
Require-AbsentRegex $homeExBodies '0x6060|0x6040|0x607A|0x60FF|0x6071' 'HOMEEX-06 handlers contain no motion/mode SDO object mutation sites'
Require-AbsentRegex $homeExBodies 'Ds402HomeExState\s*\[[^\]]+\]\s*:=' 'HOMEEX-06 handlers never write HomeDS402Ex runtime/outcome records'
Require-Regex $processBody 'RETURN;' 'HomeDS402Ex cyclic processor is a no-op in HOMEEX-06'

Require-Regex $diagnostics 'if \(ObjectIndex = 0x6040\) \| \(ObjectIndex = 0x6060\)' 'generic D5 continues to deny 0x6060'

$featureMatches = [regex]::Matches(
    $control,
    '\(pResponseFrame\s*\+\s*24\)\^\$UDINT\s*:=\s*0x([0-9A-Fa-f]{8})\s*;')
Require-True ($featureMatches.Count -eq 1) 'Admin feature mask has exactly one canonical assignment'
$featureMask = [Convert]::ToUInt32($featureMatches[0].Groups[1].Value, 16)
Require-True ($featureMask -eq 0x00000017) 'production Admin feature mask remains 0x00000017'
Require-True (($featureMask -band 0x00000800) -eq 0) 'HomeDS402Ex capability bit 11 remains OFF'
Require-AbsentRegex $control 'LMC_OWNER_KIND_DS402_HOME_EX|LMC_OWNER_KIND_AXIS_DS402_HOME_EX' 'HOMEEX-06 does not prematurely add OwnerKind 7 source support'
Require-Regex $control 'OwnerKind\s*>\s*LMC_OWNER_KIND_AXIS_OPERATION_MODE' 'ownership service still caps source owner kinds at SetOperationMode for HOMEEX-06'

Require-AsciiFile $tcpPath 'TCPMotionInterface.st'
Require-AsciiFile $diagnosticsPath 'LMCDiagnosticsService.st'

Write-Host ("HOMEEX-06 LASAL scaffold PASS: {0} checks; state=SCAFFOLD_OFF" -f $script:PassCount)
