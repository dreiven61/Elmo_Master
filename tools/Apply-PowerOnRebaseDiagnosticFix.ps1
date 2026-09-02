param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'

function Read-Text([string]$Path) {
    return [IO.File]::ReadAllText($Path)
}

function Write-Text([string]$Path, [string]$Text) {
    $utf8NoBom = New-Object Text.UTF8Encoding($false)
    [IO.File]::WriteAllText($Path, $Text, $utf8NoBom)
}

function Replace-ExactOne {
    param(
        [string]$Path,
        [string]$Old,
        [string]$New,
        [string]$Label
    )
    $text = Read-Text $Path
    $count = ([regex]::Matches($text, [regex]::Escape($Old))).Count
    if ($count -ne 1) {
        throw "$Label: expected exactly one target, found $count in $Path"
    }
    $text = $text.Replace($Old, $New)
    Write-Text $Path $text
}

function Replace-RegexOne {
    param(
        [string]$Path,
        [string]$Pattern,
        [string]$Replacement,
        [string]$Label
    )
    $text = Read-Text $Path
    $rx = [regex]::new($Pattern, [Text.RegularExpressions.RegexOptions]::Singleline)
    $matches = $rx.Matches($text)
    if ($matches.Count -ne 1) {
        throw "$Label: expected exactly one target, found $($matches.Count) in $Path"
    }
    $text = $rx.Replace($text, $Replacement, 1)
    Write-Text $Path $text
}

$control = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'
$tcp = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
$errorCatalog = Join-Path $RepositoryRoot 'LMC_Library\LMC_API_Delivery\src\LmcErrorCatalog.cs'
$adminModels = Join-Path $RepositoryRoot 'LMC_Library\LMC_API_Delivery\src\LmcAdminModels.cs'
$errorTests = Join-Path $RepositoryRoot 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\ErrorCatalogTests.cs'
$rebaseFixture = Join-Path $RepositoryRoot 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalAxisRebaseBarrier.Fixture.ps1'
$architecture = Join-Path $RepositoryRoot 'docs\architecture\LMC_ENCODER_MAINTENANCE_TW19_TW20_FIXED_ONE_ACTIVATION_2026-08-04.md'
$homeOperator = Join-Path $RepositoryRoot 'docs\api\design\HOME_DS402_H37_OPERATOR_ACTIVATION_IMPLEMENTATION_20260902.md'

# LMCControlCommandService: preserve the retained barrier, but return a distinct
# admission reason instead of overloading ordinary resource-busy (-2).
Replace-ExactOne $control `
    '#define LMC_OWNER_REBASE_PERSIST_RETRY -4' `
    "#define LMC_OWNER_REBASE_PERSIST_RETRY -4`r`n#define LMC_OWNER_REBASE_REQUIRED -15" `
    'Control rebase-required define'

Replace-RegexOne $control `
    '(if \(\(effectiveAxisMask and rebaseAxisMask\) <> 0\) &\s*\(rebaseAdmissionAllowed = FALSE\) then\s*Result := )-2(;\s*RETURN;\s*end_if;)' `
    '${1}LMC_OWNER_REBASE_REQUIRED${2}' `
    'Control reserve rebase result'

# TCP adapter: propagate the dedicated reason for ordinary Axis/Group commands
# and expose the same semantic as Admin detail 65 for HomeDS402/HomeEx.
Replace-ExactOne $tcp `
    '#define LMC_OWNER_ADAPTER_ERROR_CONFLICT -9' `
    "#define LMC_OWNER_ADAPTER_ERROR_CONFLICT -9`r`n#define LMC_OWNER_ADAPTER_ERROR_REBASE_REQUIRED -15" `
    'TCP adapter rebase-required define'

Replace-RegexOne $tcp `
    '(if controlAdmissionResult < 0 then.*?elsif CommandID = 0x20E7 then.*?else\s+_memset\(dest:=#Sendbuf, usByte:=0, cntr:=16\);\s+Sendbuf\[0\]\$UINT := 0;\s+Sendbuf\[2\]\$UINT := 8;\s+Sendbuf\[8\]\$UDINT := TO_UDINT\(AxisRef\);\s+Sendbuf\[12\]\$UINT := 1;\s+)Sendbuf\[14\]\$INT := LMC_OWNER_ADAPTER_ERROR_CONFLICT;(\s+controlResponseSize := 16;)' `
    '${1}if controlAdmissionResult = LMC_OWNER_ADAPTER_ERROR_REBASE_REQUIRED then`r`n              Sendbuf[14]$INT := LMC_OWNER_ADAPTER_ERROR_REBASE_REQUIRED;`r`n            else`r`n              Sendbuf[14]$INT := LMC_OWNER_ADAPTER_ERROR_CONFLICT;`r`n            end_if;${2}' `
    'TCP ordinary rebase propagation'

Replace-RegexOne $tcp `
    '(if \(diagnosticsDs402StartValid \| diagnosticsHomeExStartValid \|\s*diagnosticsOperationModeStartValid\) &\s*\(diagnosticsAdmissionResult <> 0\).*?Sendbuf\[16\]\$UDINT := RequestBuf\[12\]\$UDINT;\s*)if diagnosticsAdmissionResult = -2 then\s*Sendbuf\[20\]\$UDINT := 41;\s*else\s*Sendbuf\[20\]\$UDINT := 42;\s*end_if;' `
    '${1}if diagnosticsAdmissionResult = LMC_OWNER_ADAPTER_ERROR_REBASE_REQUIRED then`r`n        Sendbuf[20]$UDINT := 65;`r`n      elsif diagnosticsAdmissionResult = -2 then`r`n        Sendbuf[20]$UDINT := 41;`r`n      else`r`n        Sendbuf[20]$UDINT := 42;`r`n      end_if;' `
    'TCP Admin rebase detail propagation'

# Public error catalog: make the UI explain exactly what the operator must do.
Replace-ExactOne $errorCatalog `
    'public const uint CurrentCatalogVersion = 2;' `
    'public const uint CurrentCatalogVersion = 3;' `
    'Error catalog version'
Replace-ExactOne $errorCatalog `
    '"Elmo_Master TCPMotionInterface local errors v2";' `
    '"Elmo_Master TCPMotionInterface local errors v3";' `
    'Adapter source version'

$catalogNeedle = @'
            Add(
                entries,
                LMCErrorDomain.AdapterCommand,
                -9,
                "AxisOwnershipConflict",
                "The requested axes are reserved by another active or retained operation.",
                "Read the current operation outcome, wait for its ownership to retire, then retry once.",
                AdapterSourceVersion);
'@
$catalogInsert = $catalogNeedle + @'
            Add(
                entries,
                LMCErrorDomain.AdapterCommand,
                -15,
                "AxisRebaseRequired",
                "The selected axis has a retained current-position rebase barrier, so Power On or motion admission is blocked.",
                "Keep the axis Power Off and Standstill, execute exact LMC Home (current-position-zero) to terminal success and retire it, then retry Power On once.",
                AdapterSourceVersion);
'@
Replace-ExactOne $errorCatalog $catalogNeedle $catalogInsert 'Adapter -15 catalog entry'

Replace-ExactOne $adminModels `
    '        SetOperationModeFeatureDisabled = 64' `
    "        SetOperationModeFeatureDisabled = 64,`r`n        AxisRebaseRequired = 65" `
    'Admin detail 65 enum'

$adminCatalogNeedle = @'
            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeFeatureDisabled,
'@
# Insert the new AddAdmin before the existing final SetOperationMode entry without
# depending on its description text.
$text = Read-Text $errorCatalog
$idx = $text.IndexOf($adminCatalogNeedle, [StringComparison]::Ordinal)
if ($idx -lt 0) { throw 'Admin catalog insertion anchor not found.' }
# Find the end of that AddAdmin call, then append our entry after it.
$endIdx = $text.IndexOf(');', $idx, [StringComparison]::Ordinal)
if ($endIdx -lt 0) { throw 'Admin catalog anchor terminator not found.' }
$endIdx += 2
$nl = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }
$adminAdd = $nl + '            AddAdmin(entries, LMCAdminDetailCode.AxisRebaseRequired,' + $nl +
    '                "The selected axis has a retained current-position rebase barrier.",' + $nl +
    '                "Execute exact LMC Home while Power Off/Standstill, prove terminal success and retire it, then retry the blocked mutation.");'
$text = $text.Insert($endIdx, $adminAdd)
Write-Text $errorCatalog $text

# Tests: keep -1..-9 coverage and explicitly cover the sparse -15 code.
$testNeedle = @'
            AssertEx.False(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.AdapterCommand,
                    -10,
                    out description));
'@
$testInsert = @'
            AssertEx.True(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.AdapterCommand,
                    -15,
                    out description));
            AssertDescription(
                description,
                LMCErrorDomain.AdapterCommand,
                -15,
                "AxisRebaseRequired",
                LMCErrorCatalog.AdapterSourceVersion);

'@ + $testNeedle
Replace-ExactOne $errorTests $testNeedle $testInsert 'Adapter -15 test'
Replace-ExactOne $errorTests `
    '                    65,' `
    '                    66,' `
    'Admin unknown boundary test'

# Static negative fixture follows the symbolic contract instead of requiring -2.
Replace-ExactOne $rebaseFixture `
    "'rebaseAdmissionAllowed\\s*=\\s*FALSE\\)\\s+then\\s*Result\\s*:=\\s*)-2') '4{1}-3'" `
    "'rebaseAdmissionAllowed\\s*=\\s*FALSE\\)\\s+then\\s*Result\\s*:=\\s*)LMC_OWNER_REBASE_REQUIRED') '4{1}-3'" `
    'Rebase fixture dedicated result'

# Documentation: preserve the barrier and clarify the actual test order.
$archNeedle = 'adapter ABI가 적용되는 경로는 symbolic `-9 AxisOwnershipConflict`를 사용하고'
$archReplacement = 'adapter ABI가 적용되는 일반 ownership 충돌은 symbolic `-9 AxisOwnershipConflict`를 사용한다. retained current-position rebase barrier 차단은 별도 `-15 AxisRebaseRequired`를 반환하며,'
Replace-ExactOne $architecture $archNeedle $archReplacement 'Architecture error semantics'

$operatorNeedle = @'
## Operator procedure after this change

1. In LASAL IDE, rebuild/link the tracked project and confirm 0 errors.
'@
$operatorReplacement = @'
## Operator procedure after this change

> `LMC Home (0x7D13)` is not Servo On. It is the current-position-zero command that clears the retained rebase barrier for the selected axis. A fresh/retained `AxisRebaseRequiredState` may therefore reject WPF `Power On (0x2023)` even when direct LASAL PowerOn works. The adapter now reports this as `ErrorId=-15 (AxisRebaseRequired)` instead of generic ownership conflict.

Required test order when the selected physical axis still has the rebase bit set:

```text
PowerOff + Standstill
-> exact LMC Home 0x7D13
-> terminal success + exact retire
-> Power On 0x2023
-> stable PowerOn proof
-> HomeDS402 Method 37 test
```

Do not clear the retained word manually and do not bypass the barrier in PowerOn.

1. In LASAL IDE, rebuild/link the tracked project and confirm 0 errors.
'@
Replace-ExactOne $homeOperator $operatorNeedle $operatorReplacement 'HomeDS402 operator order'

# Final invariant checks.
$controlText = Read-Text $control
$tcpText = Read-Text $tcp
if ($controlText -notmatch '#define\s+LMC_OWNER_REBASE_REQUIRED\s+-15') { throw 'Missing control rebase define.' }
if ($controlText -notmatch 'Result\s*:=\s*LMC_OWNER_REBASE_REQUIRED') { throw 'Reserve does not return dedicated rebase result.' }
if ($tcpText -notmatch 'LMC_OWNER_ADAPTER_ERROR_REBASE_REQUIRED\s+-15') { throw 'Missing TCP rebase adapter define.' }
if ($tcpText -notmatch 'Sendbuf\[14\]\$INT\s*:=\s*LMC_OWNER_ADAPTER_ERROR_REBASE_REQUIRED') { throw 'PowerOn path does not expose rebase error.' }
if ($tcpText -notmatch 'Sendbuf\[20\]\$UDINT\s*:=\s*65') { throw 'Admin rebase detail 65 missing.' }

Write-Host 'PowerOn/rebase diagnostic fix applied successfully.'
