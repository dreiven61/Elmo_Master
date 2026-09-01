# Generic SDO Write 직접 수동 실행 전환 설계 — 2026-09-01

- 기준 branch: `dev`
- 기준 source: `dev@eeebda2b36a52a442f4919cbe70011536103b7be`
- 목적: WPF의 ordinary/manual Generic SDO Write에서 **Same-Value Qualification 선행 의무를 제거**하고, 연결 후 유효한 generic request를 `Arm -> Confirm -> Write -> Exact Readback`으로 직접 실행할 수 있게 한다.
- 상태: **DESIGN APPROVED / CODE CHANGE PENDING**
- production posture: **NO-GO**
- 선행 상세 설계: `SDO_WRITE_DETAILED_DESIGN_20260901.md`

이 문서는 `SDO_WRITE_DETAILED_DESIGN_20260901.md`의 transport qualification 관련 ordinary-manual admission 규칙을 override한다. 특히 기존 문서의 `current transport qualification proof`, `transport proof stale -> Write 0`, `Run Same-Value Qualification First` 계열 요구는 **ordinary manual Write의 필수조건으로 사용하지 않는다.** Same-Value runner는 선택적 진단/qualification 도구로만 유지한다.

---

# 1. 변경 배경

current WPF는 Generic SDO 주소 제한을 제거한 뒤에도 manual Write submit을 current-session `SdoWriteActivationQualificationProof`에 묶고 있다. 따라서 editor에는 Write request를 입력할 수 있어도 submit button이 `Run Same-Value Qualification First` 상태로 비활성화된다.

현재 source의 실질적 차단점은 두 층이다.

1. `MainWindow.Diagnostics.cs` UI state
   - `hasCurrentSdoWriteTransportProof`
   - `ButtonSubmitSdo.IsEnabled`의 proof 조건
   - proof가 없을 때 `Run Same-Value Qualification First` 문구
2. `ButtonSubmitSdo_Click()` runtime admission
   - Write 시작 직후 proof 검사
   - baseline Read 뒤 proof 재검사
   - final preflight 뒤 proof 재검사

즉 ObjectIndex policy와 별개로 qualification proof가 ordinary Write의 독립적인 admission gate로 남아 있다.

---

# 2. 최종 사용자 동작

변경 후 ordinary manual Write의 정상 흐름은 다음으로 고정한다.

```text
Connect
-> Refresh Diagnostics Capabilities
-> Operation = Write
-> Slave/Object/SubIndex/Type/Length/Value 입력
-> Arm SDO Write
-> exact baseline Read + immutable confirmation arm
-> Confirm & Submit SDO Write
-> final safe-axis/capability preflight
-> exact pre-Write guard Read
-> baseline == pre-Write guard
-> durable mutation journal arm
-> one-shot Write
-> terminal polling
-> mandatory exact Readback
-> VERIFIED / unresolved classification
```

**Same-Value Qualification은 위 흐름의 선행 단계가 아니다.** 사용자가 transport canary를 별도로 점검하고 싶을 때만 실행한다.

---

# 3. 유지하는 안전/무결성 계약

qualification proof만 제거한다. 아래 계약은 그대로 유지한다.

## 3.1 request shape

- `SlaveReference = 1..4`
- `ObjectIndex != 0`
- `SubIndex = 0..255`
- canonical scalar only
  - 1 byte: Bool / Int8 / UInt8 / BitField8
  - 2 bytes: Int16 / UInt16 / BitField16
  - 4 bytes: Int32 / UInt32 / Real32 / BitField32
- Bool raw value는 `0` 또는 `1`
- exact DataLength / WriteData length
- `TimeoutCycles = 1..60000`

## 3.2 ObjectIndex policy

Generic SDO Write 계층에는 **hard-coded ObjectIndex denylist를 두지 않는다.**

```text
valid: 0x0001 .. 0xFFFF
invalid: 0x0000
```

따라서 `0x6040`, `0x6060`, `0x607A`, `0x60FF`, `0x6071`, `0x3204`, `0x20FC`도 주소 자체만으로 Generic SDO Write에서 거부하지 않는다.

이 low-level Generic SDO surface는 semantic owner API를 대신하지 않는다. 예를 들어 operation mode를 의미적으로 변경할 때는 `SetOperationMode`가 더 높은 수준의 lifecycle/no-replay contract를 제공한다. 그러나 Generic SDO editor/API는 사용자가 명시적으로 raw object를 선택한 경우 ObjectIndex만으로 이를 차단하지 않는다.

## 3.3 current-session/fresh capability

Same-Value proof는 제거하지만 current request마다 다음 identity/capability 검사는 유지한다.

- active `LMCConnection`
- current `SessionGeneration`
- nonzero `DiagnosticsBootId`
- nonzero `MapRevision`
- `SDORead`
- `SDOWrite`
- required general-inline read capability for baseline/readback
- request에 필요한 `MaxSdoDataBytes`

Write 직전 fresh capability observation과 identity-pinned submit 계약은 유지한다. 과거 qualification proof를 캐시해 admission을 대신하지 않는다.

## 3.4 safe-axis preflight

ordinary Write의 current safe-axis 검사를 유지한다.

- `Standstill = True`
- fault/unsafe state reject
- stable position samples
- current source가 추가로 확인하는 safety ownership/interlock 조건 유지

Same-Value qualification의 존재 여부가 safe-axis 판정을 대체하지 않는다.

## 3.5 two-click confirmation

첫 click은 Write를 보내지 않는다.

```text
first click
-> baseline Read
-> exact immutable request snapshot
-> Arm only
-> Write count = 0
```

두 번째 click에서 exact same request/session/identity만 submit 후보가 된다. editor field 변경 시 armed state를 폐기한다.

## 3.6 baseline / pre-Write guard

- Arm 전에 exact baseline Read
- 실제 Write 직전에 exact pre-Write guard Read
- bytes가 다르면 Write 0회

이 비교는 외부 actor와의 atomic CAS를 의미하지 않는다. stale operator intent 방어로 사용한다.

## 3.7 durable no-replay journal

journal은 wire attempt 전에 반드시 arm한다.

보존 항목:

- endpoint/session identity
- Build/BootId/MapRevision
- Slave/Object/SubIndex/Type/Length/Timeout
- baseline bytes
- pre-Write guard bytes
- expected Write bytes

wire attempt 이후 original Write automatic replay는 0회다.

## 3.8 mandatory exact readback

Write ticket `Completed/Success`만으로 VERIFIED 처리하지 않는다.

```text
terminal success
-> exact target Readback
-> exact bytes == requested bytes
-> journal Resolved
```

readback mismatch/identity drift/readback failure는 unresolved/recovery 상태로 남긴다.

---

# 4. 제거하는 admission gate

## 4.1 UI state gate 제거

파일:

- `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs`
- distribution mirror의 동일 파일

현재 형태의 다음 의미를 제거한다.

```text
hasCurrentSdoWriteTransportProof
ButtonSubmitSdo.IsEnabled ... && hasCurrentSdoWriteTransportProof
Run Same-Value Qualification First
```

변경 후 button enablement는 최소 다음에만 의존한다.

```text
connected
&& idle
&& canSubmitSdoOperation
&& supportsSdoWrite
&& DiagnosticsMutationJournalCanArm
&& !pending incompatible mutation/readback state
```

Write mode button text:

```text
not armed -> Arm SDO Write
armed     -> Confirm & Submit SDO Write
```

proof 없음은 disabled reason이나 tooltip reason이 아니다.

## 4.2 runtime handler proof 검사 제거

`ButtonSubmitSdo_Click()`의 ordinary Write path에서 아래 세 종류 검사를 제거한다.

```text
HasCurrentSdoWriteActivationQualificationProof(...) before baseline
HasCurrentSdoWriteActivationQualificationProof(...) after baseline
HasCurrentSdoWriteActivationQualificationProof(...) at final preflight
```

대신 각 지점에서 이미 존재하는 current connection/capability/session/journal/safe-axis 검사를 사용한다.

주의: proof 검사 제거 과정에서 baseline Read, confirmation arm, second-click consume, final safe-axis verification, fresh capability refresh, pre-write guard, durable journal arm을 같이 삭제하면 안 된다.

---

# 5. Same-Value Qualification의 새 역할

다음 파일/기능은 제거 대상이 아니다.

- `MainWindow.Qualification.SdoWrite.cs`
- `SdoWriteActivationQualificationProof.cs`
- D5 Same-Value qualification runner

다만 역할을 다음으로 축소한다.

```text
optional engineering diagnostic / transport canary
```

허용:

- UI24 same-value baseline/prewrite/write/readback 검증
- ticket/executor/readback transport evidence 수집
- qualification 결과 표시

금지:

- ordinary manual Write button unlock key로 사용
- arbitrary ObjectIndex authorization으로 사용
- proof가 없다는 이유로 manual Write 차단

`SdoWriteActivationQualificationProof` class는 qualification evidence 표시/회귀시험 때문에 남겨도 된다. ordinary manual execution path에서 참조하지 않으면 된다. 이후 dead-code 정리는 별도 변경으로 한다.

---

# 6. UI 문구/정체성 수정

current title의 다음 문구는 실제 current-session qualification 상태처럼 보이므로 제거 또는 일반화한다.

```text
[LIVE Axis qualification / qualified Axis1 UI24 SDO Write]
```

권장:

```text
[Generic SDO Write / LIVE Diagnostics]
```

또한 다음 stale UI 문구를 검색해 정리한다.

- `Run Same-Value Qualification First`
- `Manual SDO Write requires ... qualification proof`
- `transport proof is no longer current`
- `transport proof changed during baseline acquisition`

qualification tab 자체의 안내문은 유지하되 `optional diagnostic`임을 명시한다.

한국어 localization catalog에도 대응 문자열을 추가/수정하고 localization coverage test를 맞춘다.

---

# 7. 구현 대상 파일

## canonical WPF

- `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs`
  - proof-based button enablement 제거
  - ordinary handler proof checks 제거
  - Arm/Confirm text와 tooltip 수정
- `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs`
  - stale qualified-UI24 window title 수정
- `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.SdoWrite.cs`
  - readiness matrix에서 manual-write admission status를 optional diagnostic status로 재표현
- `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/SdoWriteActivationQualificationProof.cs`
  - ordinary path dependency가 없어졌는지 확인; 필요 시 class는 그대로 유지
- localization catalog/test

## distribution mirror

`LMC_Library/LMC_API_Distribution/02_Example_Program/LasalApiWpfTestApp/`의 대응 WPF source도 canonical source와 동기화한다.

## SDK / LASAL

이번 변경의 핵심은 WPF admission gate 제거다. 이미 적용된 **all-valid-nonzero ObjectIndex policy를 되돌리지 않는다.**

SDK/LASAL은 다음 회귀만 확인한다.

- ObjectIndex 0만 invalid
- former seven-object denylist가 다시 생기지 않음
- 1/2/4 canonical request policy 유지
- identity-pinned one-shot submit 유지
- shared executor/no-replay 유지

---

# 8. 회귀시험 요구사항

## 8.1 UI enablement

- connected + idle + SDO Write capability + journal arm 가능 + proof 없음 => `ButtonSubmitSdo.IsEnabled == true`
- proof 없음 => button text가 `Arm SDO Write`
- proof 없음 => `Run Same-Value Qualification First`가 나타나지 않음
- pending required readback/interlock 상태는 기존대로 우선 차단

## 8.2 no-proof direct Write workflow

fake transport에서 qualification runner를 한 번도 실행하지 않은 fresh session으로 검사한다.

```text
first click
-> baseline Read 1회
-> Write 0회
-> confirmation armed

second click
-> safe/fresh preflight
-> pre-write guard Read 1회
-> journal arm
-> Write 1회
-> terminal success
-> exact readback
-> VERIFIED
```

assertion:

- qualification Write count = 0
- ordinary Write count = 1
- original Write replay count = 0

## 8.3 negative regression

proof 유무와 무관하게 다음은 계속 Write 0회여야 한다.

- disconnected
- SDO Write capability 없음
- invalid ObjectIndex 0
- malformed type/length
- safe-axis failure
- baseline failure
- editor change after arm
- baseline/prewrite mismatch
- journal arm failure
- pending incompatible mutation/readback
- identity drift before submit

## 8.4 former denylist positive regression

ObjectIndex 자체로 막히지 않는지 최소 다음 tuple을 request-policy/UI path에서 positive로 검사한다.

```text
0x6040
0x6060
0x607A
0x60FF
0x6071
0x3204
0x20FC
```

단, fake test에서는 실제 drive side effect를 만들지 않는다. hardware에서 변경값을 쓸지는 별도 시험계획과 operator 승인으로 결정한다.

---

# 9. 완료 조건

다음이 모두 만족되면 `DIRECT_MANUAL_SDO_WRITE_ENABLEMENT`를 software complete로 판정한다.

- [ ] WPF button이 qualification proof 없이 Write mode에서 활성화됨
- [ ] first click baseline/arm, Write 0회
- [ ] second click exactly one Write
- [ ] runtime handler에 qualification proof mandatory check 0개
- [ ] Same-Value runner는 optional diagnostic으로 동작
- [ ] safe-axis / baseline / prewrite / journal / no-replay / exact readback 유지
- [ ] ObjectIndex denylist 0개, ObjectIndex 0만 invalid
- [ ] canonical WPF와 distribution mirror 동기화
- [ ] Debug/Release WPF build PASS
- [ ] relevant smoke/regression PASS
- [ ] stale qualification-required UI/localization 문구 제거

이 gate는 PC/software 판정이다. 실제 drive ObjectIndex/값의 안전성과 physical side effect는 각 hardware qualification의 별도 evidence다.

---

# 10. 구현 순서

1. `MainWindow.Diagnostics.cs`의 UI proof enablement 제거
2. `ButtonSubmitSdo_Click()`의 세 proof mandatory check 제거
3. button text/tooltip/localization 수정
4. window title과 qualification readiness 문구를 optional diagnostic으로 수정
5. distribution mirror 동기화
6. no-proof direct Write smoke 추가/수정
7. former denylist positive regression 확인
8. WPF Debug/Release build + focused smoke
9. 결과를 `API_DEVELOPMENT_PROGRESS.md`와 SDO implementation result 문서에 반영

코드 변경 시 이 문서의 핵심 불변식은 다음 한 문장이다.

> **Same-Value Qualification은 선택적 진단이고, ordinary Generic SDO Write의 admission은 current request의 capability/session/safety/confirmation/journal/readback 계약으로 결정한다.**
