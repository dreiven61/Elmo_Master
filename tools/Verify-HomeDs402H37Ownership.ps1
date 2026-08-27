param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'
$script:CheckCount = 0

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        throw "FAIL H37 ownership verifier: $Message"
    }
    $script:CheckCount++
    Write-Host "PASS $Message"
}

function Assert-Match {
    param([string]$Text, [string]$Pattern, [string]$Message)
    Assert-True ([regex]::IsMatch($Text, $Pattern)) $Message
}

function Read-SourceText {
    param([string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing source file: $RelativePath"
    }
    return Get-Content -LiteralPath $path -Raw
}

function Get-FunctionBlock {
    param([string]$Text, [string]$FunctionName)
    $pattern = '(?s)FUNCTION(?:\s+GLOBAL)?\s+' + [regex]::Escape($FunctionName) + '\b.*?END_FUNCTION'
    $match = [regex]::Match($Text, $pattern)
    Assert-True $match.Success "$FunctionName source block exists"
    return $match.Value
}

$controlPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
$control = Read-SourceText $controlPath
$reserve = Get-FunctionBlock $control 'LMCControlCommandService::ReserveAxisOwnership'

Assert-Match $control '(?m)^\s*#define\s+LMC_OWNER_STATE_DS402_HOME_ACTIVE\s+6\s*$' 'HomeDS402 active owner state remains 6'
Assert-Match $control '(?m)^\s*#define\s+LMC_OWNER_KIND_DS402_HOME\s+4\s*$' 'HomeDS402 owner kind remains 4'
Assert-Match $control '(?m)^\s*#define\s+LMC_OWNER_RESOURCE_DS402_HOME_ENGINE\s+3\s*$' 'HomeDS402 shared engine resource remains 3'
Assert-Match $control '(?m)^\s*#define\s+LMC_OWNER_KIND_ENCODER\s+5\s*$' 'encoder maintenance owner kind remains 5'
Assert-Match $control '(?m)^\s*#define\s+LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE\s+4\s*$' 'diagnostics SDO resource remains 4'

Assert-Match $reserve '(?s)elsif\s+ResourceKind\s*=\s*LMC_OWNER_RESOURCE_DS402_HOME_ENGINE\s+then.*?OwnerKind\s*<>\s*LMC_OWNER_KIND_DS402_HOME.*?CommandId\s*<>\s*0x7D15.*?AdmissionMode\s*<>\s*LMC_OWNER_ADMISSION_LIFECYCLE' '0x7D15 is admitted only as HomeDS402 lifecycle owner on resource 3'
Assert-Match $reserve '(?s)0x7D12:\s*if\s*\(OwnerKind\s*<>\s*LMC_OWNER_KIND_DIRECT\).*?AdmissionMode\s*<>\s*LMC_OWNER_ADMISSION_ORDINARY' 'SetPosition remains an ordinary direct axis owner'
Assert-Match $reserve '(?s)if\s*\(OwnerKind\s*=\s*LMC_OWNER_KIND_ENCODER\)\s*&\s*\(CommandId\s*=\s*0x7E53\).*?diagnosticsSdoTupleValid\s*:=\s*TRUE.*?AdmissionMode\s*<>\s*LMC_OWNER_ADMISSION_LIFECYCLE' 'encoder maintenance 0x7E53 remains a lifecycle owner on diagnostics SDO resource'

Assert-Match $reserve '(?s)0x2022:\s*if\s*\(OwnerKind\s*<>\s*LMC_OWNER_KIND_DIRECT\)\s*\|.*?AdmissionMode\s*<>\s*LMC_OWNER_ADMISSION_SAFETY' 'Axis Stop 0x2022 is safety admission'
Assert-Match $reserve '(?s)0x2023:\s*if\s*\(OwnerKind\s*<>\s*LMC_OWNER_KIND_DIRECT\)\s*\|.*?AdmissionMode\s*<>\s*LMC_OWNER_ADMISSION_ORDINARY.*?AdmissionMode\s*<>\s*LMC_OWNER_ADMISSION_SAFETY' 'Axis Power 0x2023 supports ordinary or safety admission only'
Assert-Match $reserve '(?s)0x2024,\s*0x209F,\s*0x20A0,\s*0x20A2:\s*if\s*\(OwnerKind\s*<>\s*LMC_OWNER_KIND_DIRECT\)\s*\|.*?AdmissionMode\s*<>\s*LMC_OWNER_ADMISSION_ORDINARY' 'Axis Reset 0x2024 remains ordinary admission'
Assert-Match $reserve '(?s)0x2048,\s*0x2085:\s*if\s*\(OwnerKind\s*<>\s*LMC_OWNER_KIND_GROUP\)\s*\|.*?AdmissionMode\s*<>\s*LMC_OWNER_ADMISSION_SAFETY' 'Group Disable/Stop remain safety admission'
Assert-Match $reserve '(?s)0x2049,\s*0x204A:\s*if\s*\(OwnerKind\s*<>\s*LMC_OWNER_KIND_GROUP\)\s*\|.*?AdmissionMode\s*<>\s*LMC_OWNER_ADMISSION_ORDINARY' 'Group Reset/PowerOn remain ordinary admission'
Assert-Match $reserve '(?s)0x204B:\s*if\s*\(OwnerKind\s*<>\s*LMC_OWNER_KIND_GROUP\)\s*\|.*?AdmissionMode\s*<>\s*LMC_OWNER_ADMISSION_SAFETY' 'Group PowerOff remains safety admission'

Assert-Match $reserve '(?s)if\s*\(OwnerKind\s*=\s*LMC_OWNER_KIND_GROUP\)\s*&\s*\(AdmissionMode\s*<>\s*LMC_OWNER_ADMISSION_SAFETY\)\s*&\s*liveNonGroupFound\s+then\s*Result\s*:=\s*-2\s*;' 'ordinary Group mutation rejects an existing non-group owner'
Assert-Match $reserve '(?s)if\s*\(OwnerKind\s*<>\s*LMC_OWNER_KIND_GROUP\)\s*&\s*\(AdmissionMode\s*<>\s*LMC_OWNER_ADMISSION_SAFETY\)\s*&\s*liveGroupFound\s+then\s*Result\s*:=\s*-2\s*;' 'ordinary/lifecycle non-group mutation rejects an existing Group owner'

Assert-Match $reserve '(?s)if\s+safetyPreemption\s*&.*?existingOwnerKind\s*=\s*LMC_OWNER_KIND_LMC_HOME.*?existingOwnerKind\s*=\s*LMC_OWNER_KIND_DS402_HOME.*?existingOwnerKind\s*=\s*LMC_OWNER_KIND_ENCODER.*?cleanupRequiredMask\s*:=\s*cleanupRequiredMask\s+or\s+axisBit' 'safety preemption requires cleanup for HomeDS402 and encoder maintenance owners'
Assert-Match $reserve '(?s)if\s*\(existingOwnerKind\s*=\s*LMC_OWNER_KIND_LMC_HOME\)\s*\|.*?existingOwnerKind\s*=\s*LMC_OWNER_KIND_DS402_HOME.*?existingOwnerKind\s*=\s*LMC_OWNER_KIND_ENCODER.*?LMC_OWNER_OBSERVER_PREEMPTED_SPECIAL' 'HomeDS402 and encoder maintenance are tagged as special preempted owners'
Assert-Match $reserve '(?s)rebaseAdmissionAllowed\s*:=.*?CommandId\s*=\s*0x7E53.*?CommandId\s*=\s*0x2024\)\s*\|\s*\(CommandId\s*=\s*0x2049\)' 'encoder maintenance and Axis/Group Reset retain explicit rebase admission exceptions'

$managedPattern = '(?s)case\s+CommandId\s+of\s*0x2022,\s*0x2023,\s*0x2024,\s*0x209F,\s*0x20A0,\s*0x20A2,\s*0x2047,\s*0x2048,\s*0x2049,\s*0x204A,\s*0x204B,\s*0x2085,\s*0x20A4,\s*0x20E7,\s*0x7D22:'
Assert-Match $control $managedPattern 'ordinary ownership dispatcher keeps Axis/Group mutation command inventory together'

Write-Host ("H37 ownership verifier PASS: {0} checks" -f $script:CheckCount)
