# SetOperationMode Fresh C78 Evidence Capture Guide

- 대상 gate: MODE-10 IDE/artifact continuation before MODE-11/12
- 목적: 실제 LASAL IDE C78/ARM Rebuild/Link 결과와 generated artifact identity를 같은 build window로 고정
- 자동 승인 여부: **없음**
- activation: **OFF 유지**

이 절차는 LASAL IDE build를 대체하지 않는다. 실제 IDE에서 fresh Rebuild/Link를 수행한 직후
`tools/Capture-SetOperationModeC78Evidence.ps1`로 build log와 두 `.lcb` artifact를 한 evidence
checkpoint에 묶는다.

## 1. build 전

1. `dev`를 checkout하고 작업 tree의 의도하지 않은 변경을 확인한다.
2. `LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE`와 Admin capability bits 8/9/10 OFF를 유지한다.
3. build 시작 직전에 UTC 시간을 기록한다.

PowerShell 예:

```powershell
$buildStartedUtc = [DateTime]::UtcNow
$buildStartedUtc.ToString('o')
```

이 값을 build가 끝날 때까지 보존한다. artifact freshness 검증의 하한으로 사용한다.

## 2. LASAL IDE fresh build

LASAL IDE에서 current `dev` source를 대상으로 **C78 / ARM Rebuild/Link**한다.

build-specific log에는 최소 다음 증거가 있어야 한다.

- target `C78`
- architecture `ARM`
- explicit `0 errors`
- successful linker evidence(`Linker ... Done` 또는 `Linking ... successful`)

누적 IDE log 전체를 그대로 사용하지 말고 가능하면 이번 rebuild 범위만 별도 파일로 저장한다.
과거 nonzero error가 같은 파일에 섞여 있으면 collector는 의도적으로 fail-closed 한다.

## 3. evidence capture

IDE가 생성/갱신한 다음 파일을 그대로 둔다.

- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb`
- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcb`

그 다음 repository root에서 실행한다.

```powershell
.\tools\Capture-SetOperationModeC78Evidence.ps1 `
  -BuildLogPath 'C:\path\to\fresh-c78-build.log' `
  -BuildStartedUtc $buildStartedUtc
```

원하는 evidence 파일명을 고정하려면 `-OutputPath`를 추가한다.

```powershell
.\tools\Capture-SetOperationModeC78Evidence.ps1 `
  -BuildLogPath 'C:\path\to\fresh-c78-build.log' `
  -BuildStartedUtc $buildStartedUtc `
  -OutputPath '.\docs\api\design\evidence\SET_OPERATION_MODE_C78_CAPTURE_YYYYMMDD.md'
```

collector는 다음을 검증/기록한다.

- artifact/log `LastWriteTimeUtc >= BuildStartedUtc`
- C78/ARM/zero-error/link evidence
- repository HEAD와 working-tree status
- tracked HEAD blob과 current working blob identity
- artifact/log SHA-256, size, timestamp
- Control/Diagnostics/TCP/static-verifier/SetOperationMode design source SHA-256

## 4. 정상 결과의 의미

정상 capture의 문서에는 반드시 다음이 남는다.

- `GateResult: CAPTURED_FOR_REVIEW`
- `ArtifactRatchetDecision: REVIEW_REQUIRED`
- `CapabilityActivation: KEEP_OFF`

즉 collector PASS는 **IDE/artifact 최종 PASS가 아니다.** hash가 바뀌었다는 이유만으로
`Classes.lcb` physical identity ratchet을 갱신하지 않는다.

## 5. capture 후 manual review

다음을 사람이 검토한 뒤에만 artifact identity 승인 여부를 결정한다.

1. generated declaration/ABI가 tracked source 의도와 일치하는가.
2. `Classes.lcb`와 project `.lcb`가 같은 fresh build에서 생성됐는가.
3. compiler/linker log와 artifact timestamp가 같은 build window인가.
4. source SHA가 build 대상으로 의도한 `dev` checkpoint와 일치하는가.
5. 의도하지 않은 generated/source 변경이 working tree에 없는가.

## 6. 다음 hardware gate

artifact review가 끝나도 capability는 아직 켜지 않는다.

1. 같은 image를 PLC에 load한다.
2. 같은 image의 `DiagnosticsBuild + DiagnosticsBootId + MapRevision` tuple을 기록한다.
3. MODE-11에서 axis 1부터
   - same-mode `6061=8` no-write
   - non-8 -> 8 exact one-byte `6060:0=8`
   - final `6061=8`
   의 packet causal evidence를 검증한다.
4. MODE-12 timeout/disconnect/mismatch/quarantine/retire matrix를 axis 1에서 닫은 뒤 2~4로 확대한다.
5. 모든 증거가 닫힌 뒤 MODE-14 paired activation을 별도 commit으로 검토한다.

## 7. collector 자체 검증

repository CI에서는 실제 C78 build를 위조하지 않고 collector logic만 synthetic fixture로 검증한다.

```powershell
.\tools\Capture-SetOperationModeC78Evidence.ps1 -SelfTest
```

이 self-test가 PASS해도 실제 C78/PLC/hardware PASS를 뜻하지 않는다.
