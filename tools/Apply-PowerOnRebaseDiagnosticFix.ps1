param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
$ErrorActionPreference = 'Stop'

function ReadText([string]$p) { [IO.File]::ReadAllText($p) }
function WriteText([string]$p,[string]$s) {
    [IO.File]::WriteAllText($p,$s,(New-Object Text.UTF8Encoding($false)))
}
function ReplaceOne([string]$p,[string]$old,[string]$new,[string]$label) {
    $s=ReadText $p
    $n=([regex]::Matches($s,[regex]::Escape($old))).Count
    if($n -ne 1){ throw "$label expected 1 target, found $n" }
    WriteText $p ($s.Replace($old,$new))
}
function RegexOne([string]$p,[string]$pattern,[string]$replacement,[string]$label) {
    $s=ReadText $p
    $rx=[regex]::new($pattern,[Text.RegularExpressions.RegexOptions]::Singleline)
    $m=$rx.Matches($s)
    if($m.Count -ne 1){ throw "$label expected 1 target, found $($m.Count)" }
    WriteText $p ($rx.Replace($s,$replacement,1))
}
function NL([string]$s) { if($s.Contains("`r`n")){"`r`n"}else{"`n"} }

$control=Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'
$tcp=Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
$catalog=Join-Path $RepositoryRoot 'LMC_Library\LMC_API_Delivery\src\LmcErrorCatalog.cs'
$models=Join-Path $RepositoryRoot 'LMC_Library\LMC_API_Delivery\src\LmcAdminModels.cs'
$tests=Join-Path $RepositoryRoot 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\ErrorCatalogTests.cs'
$fixture=Join-Path $RepositoryRoot 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalAxisRebaseBarrier.Fixture.ps1'
$arch=Join-Path $RepositoryRoot 'docs\architecture\LMC_ENCODER_MAINTENANCE_TW19_TW20_FIXED_ONE_ACTIVATION_2026-08-04.md'
$op=Join-Path $RepositoryRoot 'docs\api\design\HOME_DS402_H37_OPERATOR_ACTIVATION_IMPLEMENTATION_20260902.md'

# Distinguish retained rebase barrier from an ordinary ownership-busy result.
$s=ReadText $control; $nl=NL $s
ReplaceOne $control '#define LMC_OWNER_REBASE_PERSIST_RETRY -4' ('#define LMC_OWNER_REBASE_PERSIST_RETRY -4'+$nl+'#define LMC_OWNER_REBASE_REQUIRED -15') 'control define'
RegexOne $control '(if \(\(effectiveAxisMask and rebaseAxisMask\) <> 0\) &\s*\(rebaseAdmissionAllowed = FALSE\) then\s*Result := )-2(;\s*RETURN;\s*end_if;)' '${1}LMC_OWNER_REBASE_REQUIRED${2}' 'reserve rebase result'

$s=ReadText $tcp; $nl=NL $s
ReplaceOne $tcp '#define LMC_OWNER_ADAPTER_ERROR_CONFLICT -9' ('#define LMC_OWNER_ADAPTER_ERROR_CONFLICT -9'+$nl+'#define LMC_OWNER_ADAPTER_ERROR_REBASE_REQUIRED -15') 'tcp define'
RegexOne $tcp '(if controlAdmissionResult < 0 then.*?elsif CommandID = 0x20E7 then.*?else\s+_memset\(dest:=#Sendbuf, usByte:=0, cntr:=16\);\s+Sendbuf\[0\]\$UINT := 0;\s+Sendbuf\[2\]\$UINT := 8;\s+Sendbuf\[8\]\$UDINT := TO_UDINT\(AxisRef\);\s+Sendbuf\[12\]\$UINT := 1;\s+)Sendbuf\[14\]\$INT := LMC_OWNER_ADAPTER_ERROR_CONFLICT;(\s+controlResponseSize := 16;)' ('${1}if controlAdmissionResult = LMC_OWNER_ADAPTER_ERROR_REBASE_REQUIRED then'+$nl+'              Sendbuf[14]$INT := LMC_OWNER_ADAPTER_ERROR_REBASE_REQUIRED;'+$nl+'            else'+$nl+'              Sendbuf[14]$INT := LMC_OWNER_ADAPTER_ERROR_CONFLICT;'+$nl+'            end_if;${2}') 'tcp poweron propagation'
RegexOne $tcp '(if \(diagnosticsDs402StartValid \| diagnosticsHomeExStartValid \|\s*diagnosticsOperationModeStartValid\) &\s*\(diagnosticsAdmissionResult <> 0\).*?Sendbuf\[16\]\$UDINT := RequestBuf\[12\]\$UDINT;\s*)if diagnosticsAdmissionResult = -2 then\s*Sendbuf\[20\]\$UDINT := 41;\s*else\s*Sendbuf\[20\]\$UDINT := 42;\s*end_if;' ('${1}if diagnosticsAdmissionResult = LMC_OWNER_ADAPTER_ERROR_REBASE_REQUIRED then'+$nl+'        Sendbuf[20]$UDINT := 65;'+$nl+'      elsif diagnosticsAdmissionResult = -2 then'+$nl+'        Sendbuf[20]$UDINT := 41;'+$nl+'      else'+$nl+'        Sendbuf[20]$UDINT := 42;'+$nl+'      end_if;') 'tcp admin rebase detail'

ReplaceOne $catalog 'public const uint CurrentCatalogVersion = 2;' 'public const uint CurrentCatalogVersion = 3;' 'catalog version'
ReplaceOne $catalog '"Elmo_Master TCPMotionInterface local errors v2";' '"Elmo_Master TCPMotionInterface local errors v3";' 'adapter source version'
$s=ReadText $catalog; $nl=NL $s
$anchor='            Add('+ $nl +'                entries,'+$nl+'                LMCErrorDomain.AdapterCommand,'+$nl+'                -9,'+$nl+'                "AxisOwnershipConflict",'+$nl+'                "The requested axes are reserved by another active or retained operation.",'+$nl+'                "Read the current operation outcome, wait for its ownership to retire, then retry once.",'+$nl+'                AdapterSourceVersion);'
$insert=$anchor+$nl+'            Add('+$nl+'                entries,'+$nl+'                LMCErrorDomain.AdapterCommand,'+$nl+'                -15,'+$nl+'                "AxisRebaseRequired",'+$nl+'                "The selected axis has a retained current-position rebase barrier, so Power On or motion admission is blocked.",'+$nl+'                "Keep the axis Power Off and Standstill, execute exact LMC Home (current-position-zero) to terminal success and retire it, then retry Power On once.",'+$nl+'                AdapterSourceVersion);'
ReplaceOne $catalog $anchor $insert 'adapter -15 catalog'

$s=ReadText $models; $nl=NL $s
ReplaceOne $models '        SetOperationModeFeatureDisabled = 64' ('        SetOperationModeFeatureDisabled = 64,'+$nl+'        AxisRebaseRequired = 65') 'admin detail enum'

$s=ReadText $catalog; $nl=NL $s
$anchor='            AddAdmin(entries, LMCAdminDetailCode.SetOperationModeFeatureDisabled,'+$nl+'                "The loaded PLC runtime has SetOperationMode disabled at its feature gate.",'+$nl+'                "Verify the exact generated PLC artifact and loaded image feature activation before submitting a new Start request.");'
$insert=$anchor+$nl+'            AddAdmin(entries, LMCAdminDetailCode.AxisRebaseRequired,'+$nl+'                "The selected axis has a retained current-position rebase barrier.",'+$nl+'                "Execute exact LMC Home while Power Off/Standstill, prove terminal success and retire it, then retry the blocked mutation.");'
ReplaceOne $catalog $anchor $insert 'admin detail catalog'

$s=ReadText $tests; $nl=NL $s
$anchor='            AssertEx.False('+$nl+'                LMCErrorCatalog.TryDescribe('+$nl+'                    LMCErrorDomain.AdapterCommand,'+$nl+'                    -10,'+$nl+'                    out description));'
$insert='            AssertEx.True('+$nl+'                LMCErrorCatalog.TryDescribe('+$nl+'                    LMCErrorDomain.AdapterCommand,'+$nl+'                    -15,'+$nl+'                    out description));'+$nl+'            AssertDescription('+$nl+'                description,'+$nl+'                LMCErrorDomain.AdapterCommand,'+$nl+'                -15,'+$nl+'                "AxisRebaseRequired",'+$nl+'                LMCErrorCatalog.AdapterSourceVersion);'+$nl+$nl+$anchor
ReplaceOne $tests $anchor $insert 'adapter -15 test'
ReplaceOne $tests '                    65,' '                    66,' 'admin unknown boundary'

# Keep negative mutation tests aligned with the new symbolic result.
$old="'rebaseAdmissionAllowed\\s*=\\s*FALSE\\)\\s+then\\s*Result\\s*:=\\s*)-2') '${1}-3'"
$new="'rebaseAdmissionAllowed\\s*=\\s*FALSE\\)\\s+then\\s*Result\\s*:=\\s*)LMC_OWNER_REBASE_REQUIRED') '${1}-3'"
ReplaceOne $fixture $old $new 'rebase fixture result'

ReplaceOne $arch 'adapter ABI가 적용되는 경로는 symbolic `-9 AxisOwnershipConflict`를 사용하고' 'adapter ABI가 적용되는 일반 ownership 충돌은 symbolic `-9 AxisOwnershipConflict`를 사용한다. retained current-position rebase barrier 차단은 별도 `-15 AxisRebaseRequired`를 반환하며,' 'architecture rebase error'

$s=ReadText $op; $nl=NL $s
$anchor='## Operator procedure after this change'+$nl+$nl+'1. In LASAL IDE, rebuild/link the tracked project and confirm 0 errors.'
$replacement='## Operator procedure after this change'+$nl+$nl+'> `LMC Home (0x7D13)` is not Servo On. It is the current-position-zero command that clears the retained rebase barrier for the selected axis. A fresh/retained `AxisRebaseRequiredState` may therefore reject WPF `Power On (0x2023)` even when direct LASAL PowerOn works. The adapter reports this as `ErrorId=-15 (AxisRebaseRequired)` instead of generic ownership conflict.'+$nl+$nl+'Required test order when the selected physical axis still has the rebase bit set:'+$nl+$nl+'```text'+$nl+'PowerOff + Standstill'+$nl+'-> exact LMC Home 0x7D13'+$nl+'-> terminal success + exact retire'+$nl+'-> Power On 0x2023'+$nl+'-> stable PowerOn proof'+$nl+'-> HomeDS402 Method 37 test'+$nl+'```'+$nl+$nl+'Do not clear the retained word manually and do not bypass the barrier in PowerOn.'+$nl+$nl+'1. In LASAL IDE, rebuild/link the tracked project and confirm 0 errors.'
ReplaceOne $op $anchor $replacement 'operator test order'

$ct=ReadText $control; $tt=ReadText $tcp
if($ct -notmatch 'LMC_OWNER_REBASE_REQUIRED\s+-15'){throw 'control rebase symbol missing'}
if($ct -notmatch 'Result\s*:=\s*LMC_OWNER_REBASE_REQUIRED'){throw 'reserve result not dedicated'}
if($tt -notmatch 'Sendbuf\[14\]\$INT\s*:=\s*LMC_OWNER_ADAPTER_ERROR_REBASE_REQUIRED'){throw 'adapter -15 propagation missing'}
if($tt -notmatch 'Sendbuf\[20\]\$UDINT\s*:=\s*65'){throw 'Admin detail 65 propagation missing'}
Write-Host 'PowerOn retained-rebase diagnostic fix applied.'
