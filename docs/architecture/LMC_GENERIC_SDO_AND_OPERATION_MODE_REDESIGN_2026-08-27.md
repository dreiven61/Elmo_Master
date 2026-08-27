# LMC Generic SDO / Operation Mode 재설계

- 기준일: 2026-08-27
- 기준 branch: `dev@cd89d189a3dd574c1fc1147eba07dff88effc54a`
- 목적: 2026-08-27 실기 테스트에서 확인된 `SetOperationMode` 거부, SDO Write 단일 target 제한, `LMCSdoExecutor` inherited Server Write 비동작을 구조적으로 수정한다.
- 상태: **설계 확정용 문서. 아직 production activation 근거가 아니다.**
- supersedes:
  - `docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md`의 "manual channel을 사용하지 않는다 / no-op override" 결정
  - `LMCDiagnosticsWritePolicy`의 단일 `0x2F00:24` compile-time allowlist를 일반 SDO Write의 최종 정책으로 사용하는 결정
- 유지하는 기존 원칙:
  - wire에 한 번이라도 mutation이 dispatch된 뒤에는 자동 replay 금지
  - exact endpoint / DiagnosticsBuild / BootId / MapRevision identity fence
  - unresolved mutation이 있으면 새 mutation을 막고 read-only recovery 우선
  - semantic motion command와 generic raw SDO가 같은 EtherCAT SDO resource를 동시에 소유하지 않도록 arbitration

---

## 1. 실기 테스트에서 확인된 문제

### 1.1 SetOperationMode UI는 존재하지만 실제 mode 전환이 거부됨

2026-08-27 WPF 실기 로그에서 다음 흐름이 확인됐다.

```text
Set Operation Mode Selected Mode Once started.
SetOperationMode durable journal promoted to RecoveryRequired after definitive Start rejection.
SetOperationMode definitive Start rejection archived durably; no retained PLC outcome exists ...
Set Operation Mode Selected Mode Once FAILED: StartAxisSetOperationMode was rejected.
```

즉 PC/WPF 쪽 selector가 PP/PV/IP/CSP를 보여주는 것과 **현재 PLC runtime이 해당 mode를 실제로 허용하는 것은 별개의 문제**다.

현재 production `dev` 설계는 원래 다음 상태다.

- `LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE`
- Admin capability bits 8/9/10 OFF
- 최초 public activation 범위는 CSP(8)만
- PP(1), PV(3), IP(7)은 별도 physical qualification 전까지 미광고

따라서 qualification branch의 WPF만 갱신했거나, source를 변경했더라도 정확한 generated artifact/C78/PLC image가 내려가지 않은 경우 UI와 runtime이 쉽게 불일치한다.

현재 WPF는 definitive rejection을 보존하는 safety 동작은 올바르게 수행하지만, 작업자가 화면만 보고 다음을 즉시 식별하기 어렵다.

- rejection `DetailCode`
- `ErrorId`
- 실제 PLC가 광고한 mode 지원 범위
- 현재 연결된 PLC가 어느 source/C78에서 생성됐는지
- 현재 mode `0x6061`
- requested mode가 runtime gate에서 거부됐는지, standstill/DS402 precondition에서 거부됐는지

### 1.2 일반 SDO Write가 `0x2F00:24` 한 target으로 고정됨

현재 SDK의 `LMCDiagnosticsWritePolicy`는 다음 구조다.

```text
SdoWriteEnabled = true
SdoWriteUi24Axis1Enabled = true
Axis2/3/4 = false
AllowedSdoWrites = CreateAllowedSdoWriteTargets(...)
```

`CreateAllowedSdoWriteTargets()`는 현재 Axis1의 다음 target 하나만 생성한다.

```text
Reserved diagnostic UI[24]
Slave 1
0x2F00:24
Int32 / 4 bytes
```

그리고 `RequireSdoWriteAllowed()`는 request가 이 compile-time allowlist와 일치하지 않으면 `NotSupportedException`으로 차단한다.

이 정책은 "한 개 안전한 same-value qualification target으로 SDO Write transport를 검증"하기 위한 정책이지, **일반적인 EtherCAT SDO 편집기/API의 운영 요구사항과 맞지 않는다.**

### 1.3 `LMCSdoExecutor`에서 inherited `EtherCAT_SDOBase` Server Write가 의도적으로 죽어 있음

`EtherCAT_SDOBase::ParaReadWrite::Write` 원본은 다음 동작을 한다.

1. `ParaReadWrite`에 0/1 기록
2. `ClassState <> BUSY` 확인
3. Read이면 `toSlave.StartReadSDO(...)`
4. Write이면 `toSlave.StartWriteSDO(...)`
5. READY이면 `ClassState=BUSY`, 아니면 ERROR
6. callback에서 `ClassState=READY/ERROR`, `ParaLength`, `ErrorCode` 갱신

그러나 현재 `LMCSdoExecutor`는 이를 override해 다음처럼 no-op 처리한다.

```text
FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write
    // The production executor cannot be started through the manual channel.
    result := ParaReadWrite;
END_FUNCTION
```

`ParaType::Write`, `ParaString::Write`도 동일하게 base 기능을 사용하지 않는 방향으로 격리돼 있다.

따라서 LASAL Class View에서 inherited Server에 `ParaIndex`, `ParaSubIndex`, `ParaValue`, `ParaLength`를 입력한 뒤 `ParaReadWrite=1`을 Write해도 기존 `EtherCAT_SDOBase`와 같은 SDO가 발동하지 않는 것이 **현재 코드 기준 정상 동작**이다. 이 결정이 사용자 요구와 충돌하므로 폐기한다.

---

## 2. 재설계 목표

새 구조는 다음 세 요구를 동시에 만족해야 한다.

1. `SetOperationMode`는 실제 PLC runtime 지원상태를 PC가 확인한 뒤 PP/PV/IP/CSP를 전환할 수 있어야 한다.
2. WPF/SDK의 generic SDO Write는 단일 `0x2F00:24` allowlist가 아니라 임의의 유효한 ObjectIndex/SubIndex에 대해 사용할 수 있어야 한다.
3. `LMCSdoExecutor`는 programmatic `TryStartRead/TryStartWrite`뿐 아니라 기존 `EtherCAT_SDOBase` Server 조작 방식도 실제로 동작해야 한다.

핵심 구조는 다음과 같다.

```text
                         +-------------------------------+
                         |          LMCSdoExecutor       |
                         |                               |
LASAL Server UI -------->| Manual Server Entry          |
ParaIndex/SubIndex       |  - ParaReadWrite             |
ParaValue/Length         |  - ParaType / ParaString     |
                         |              |                |
LMCDiagnosticsService -->| Tokenized Programmatic Entry | 
TryStartRead/Write       |              |                |
                         |       Shared Arbitration      |
                         |              |                |
                         |    inherited toSlave SDO      |
                         +--------------+----------------+
                                        |
                                        v
                                ECAT_Slave_Base
```

두 entry는 transport를 공유하지만 **동시에 실행할 수 없다.**

---

## 3. `LMCSdoExecutor` Dual-Entry 설계

### 3.1 기존 잘못된 전제 폐기

기존 문서의 다음 문장은 더 이상 적용하지 않는다.

> `EtherCAT_SDOBase`의 수동 Server path는 production executor에서 사용할 수 없게 한다.

새 원칙은 반대다.

> `LMCSdoExecutor`는 `EtherCAT_SDOBase`의 수동 Server 사용성을 유지하면서, 동일 object에서 LMC tokenized request도 안전하게 사용할 수 있는 dual-entry adapter다.

### 3.2 request source arbitration

현재 `AdapterState`만으로는 manual request와 tokenized request를 구분할 수 없다. 다음 source state를 추가한다.

```text
LMC_SDO_SOURCE_NONE          = 0
LMC_SDO_SOURCE_MANUAL_SERVER = 1
LMC_SDO_SOURCE_PROGRAMMATIC  = 2
```

추가 private state 후보:

```text
RequestSource      : UDINT
ActiveToken        : UDINT       // programmatic 전용, manual은 0
ActiveIndex        : UINT
ActiveSubIndex     : USINT
ActiveLength       : UINT
ActiveIsWrite      : BOOL
AdapterState       : existing state machine
```

원칙:

- `RequestSource=NONE`일 때만 새 request를 소유할 수 있다.
- manual Server request가 진행 중이면 `TryStartRead/TryStartWrite`는 `BUSY` 반환.
- programmatic request가 진행 중이면 `ParaReadWrite::Write`는 실제 SDO를 시작하지 않고 manual busy 상태를 명확히 표시한다.
- 어느 경로든 callback이 끝나기 전에 source를 NONE으로 되돌리지 않는다.
- late callback/orphan은 source identity까지 포함해 검증한다.

### 3.3 Manual Server entry

`LMCSdoExecutor::ParaReadWrite::Write`를 no-op으로 두지 않는다.

구현은 `EtherCAT_SDOBase`의 현재 동작을 보존하되 shared arbitration을 추가한다.

개념 순서:

```text
ParaReadWrite::Write(input)
  -> input을 0(Read) / nonzero(Write)로 server에 반영
  -> shared executor가 Idle인지 확인
  -> RequestSource = MANUAL_SERVER 예약
  -> inherited ParaIndex / ParaSubIndex / CompleteAccess /
     ParaType / ParaLength / ParaValue / ParaString / Timeout snapshot
  -> Read 또는 Write StartSDO 호출
  -> READY: ClassState = BUSY, manual callback 대기
  -> BUSY/ERROR: source/adapter reservation 해제, ClassState/ErrorCode 반영
```

`ParaType`과 `ParaString`도 base와 동일한 operator-facing semantics를 복구한다.

- `ParaType=0`: numeric `ParaValue`
- `ParaType=1`: `ParaString`
- numeric write는 `ParaLength`를 사용
- manual string write는 base의 string buffer 규칙을 따른다.

**주의:** generated `.st` header/declaration과 VMT는 LASAL IDE CodeGenerator 산출물이다. Server override의 추가/삭제는 GitHub text만 손으로 맞추지 않고 LASAL IDE에서 declaration/network를 갱신한 뒤 generated source와 `Classes.lcb`를 함께 검증한다.

### 3.4 Programmatic entry

기존 다음 API는 유지한다.

```text
TryStartRead(...)
TryStartWrite(...)
CopyCompletion(...)
MarkOrphan(...)
IsReusable()
```

다만 reservation 시 `RequestSource=PROGRAMMATIC`을 함께 고정한다.

programmatic path는 기존 장점을 유지한다.

- private persistent buffer
- exact OperationToken
- callback index/subindex/direction/length validation
- `READY/BUSY/ERROR` 보존
- ResultReady publication
- orphan drain
- no replay

### 3.5 callback dispatch

`LMCSdoExecutor::ClassState::NewInst`는 `ECAT_M_SDO_CALLBACK`에서 먼저 `RequestSource`를 확인한다.

```text
if RequestSource = MANUAL_SERVER
    -> manual callback handler
       - ClassState READY/ERROR
       - ErrorCode
       - ParaLength
       - numeric/string 결과 반영
       - source release
elsif RequestSource = PROGRAMMATIC
    -> 현재 tokenized callback validator/publication
else
    -> unsolicited callback quarantine
end_if
```

manual callback은 기존 `EtherCAT_SDOBase::ClassState::NewInst`의 사용자 관찰 semantics를 그대로 재현한다. programmatic callback은 기존 strict validator를 유지한다.

### 3.6 `ClassState` 의미

manual Server 사용성을 위해 다음을 보장한다.

| 상태 | manual 의미 |
|---|---|
| `READY` | 마지막 manual SDO가 정상 완료됐거나 새 manual request 가능 |
| `BUSY` | manual request 진행 중 또는 programmatic owner 때문에 manual 시작 불가 |
| `ERROR` | manual request start/callback 실패; `ErrorCode` 확인 |

programmatic path의 내부 `AdapterState`와 operator-facing `ClassState`를 같은 변수로 간주하지 않는다. 즉 service 내부 owner가 존재하더라도 operator에게 상태가 모호하지 않도록 source/busy reason을 별도로 기록한다.

---

## 4. Generic SDO Write 정책 재설계

### 4.1 compile-time 단일 target allowlist 제거

일반 SDO Write에서 다음 구조를 제거한다.

```text
AllowedSdoWrites[]
CreateAllowedSdoWriteTargets()
RequireSdoWriteAllowed() -> exact target match
```

`0x2F00:24`는 앞으로 "유일하게 허용된 target"이 아니라 **qualification preset** 중 하나로만 남긴다.

### 4.2 주소 정책

Phase 1 generic numeric SDO Write는 현재 D5 transport capacity와 executor buffer에 맞춰 다음을 지원한다.

- SlaveReference: physical drive 1..4
- ObjectIndex: `0x0001..0xFFFF`
- SubIndex: `0..255`
- DataLength: 1 / 2 / 4 bytes
- ValueType: Int8 / UInt8 / Int16 / UInt16 / Int32 / UInt32 / BitField 계열 중 wire size가 일치하는 type
- Timeout: 기존 bounded 범위
- CompleteAccess: Phase 1은 0(single access)

이 범위에서는 **index/sub-index를 SDK compile-time 목록에 미리 등록하지 않아도 된다.**

### 4.3 semantic reserved object와 raw access

"임의 object Write"와 "LMC semantic command lifecycle 우회"는 구분한다.

다음과 같이 3단계 policy를 둔다.

1. `GeneralRaw`: 일반 object. generic Write 허용.
2. `SemanticReserved`: LMC가 별도 command/owner로 관리하는 object.
3. `BlockedByActiveOwner`: 현재 motion/Home/SetPosition/SetOperationMode 등의 owner가 사용 중인 object/resource.

대표 semantic reserved 후보:

- `0x6040` Controlword
- `0x6060` Modes of operation
- `0x607A` Target position
- `0x60FF` Target velocity
- `0x6071` Target torque

기본 WPF의 일반 Write 버튼은 `GeneralRaw`를 바로 사용할 수 있다.

semantic reserved object도 SDK request model 자체에서 주소를 표현할 수는 있게 하되, 일반 UI와 semantic API를 분리한다. `0x6060`은 정상 운영에서는 `SetOperationMode`가 우선이며, raw expert access를 허용할 경우에도 active owner가 없고 operator가 명시적으로 Raw/Expert mode를 선택한 경우에만 별도 policy를 통과시킨다.

즉 기존처럼 "주소 자체를 C# compile-time allowlist에서 생성조차 못함"이 아니라 **모든 주소를 표현하고 runtime ownership/policy에서 판단**한다.

### 4.4 Write lifecycle

일반 Write는 same-value qualification에 종속되지 않는다.

새 기본 lifecycle:

```text
Validate request
  -> Refresh current Diagnostics identity
  -> Check SDO capability / owner arbitration
  -> Arm durable mutation journal with exact target + bytes
  -> Submit exactly once
  -> Accepted ticket
  -> Wait terminal
  -> Exact target Readback
  -> Compare against requested bytes when readable
  -> Resolve journal
```

불확실성:

```text
Write may have crossed wire boundary
  -> never auto replay
  -> retain exact target/value/identity
  -> reconnect to same identity
  -> exact target readback
  -> operator decides resolved / still indeterminate
```

readback 불가능한 write-only object는 자동 성공 추론을 하지 않는다. terminal ticket과 physical/device-specific evidence 정책을 별도로 사용한다.

### 4.5 WPF 변경

`SDO / Write 정책` 탭을 "qualification target 선택" 중심에서 **실제 SDO Editor** 중심으로 바꾼다.

필수 입력:

- Operation: Read / Write
- Slave 1..4
- Object index hex
- Sub-index
- Value type
- Data length
- Timeout
- Write value: integer 또는 raw hex
- CompleteAccess (향후)

기존 `SDK 승인 SDO Write target` combo는 삭제하거나 `Known preset`으로 이름을 바꾼다.

버튼 구조:

```text
Read
Write Once
Refresh Ticket
Readback
Cancel Ticket (가능한 상태만)
```

Write 전 화면에는 exact target과 raw bytes를 한 줄로 표시한다.

```text
Slave=1  Object=0x6060:0  Type=Int8  Bytes=03  Timeout=1000
```

`Write Once`는 한번의 request만 보낸다. timeout/response loss 뒤 자동 재전송하지 않는다.

---

## 5. SetOperationMode 재설계

### 5.1 UI mode list를 capability와 분리하지 않는다

현재처럼 WPF가 PP/PV/IP/CSP 목록을 자체적으로 하드코딩하고 Start를 시도하는 구조는 충분하지 않다.

PLC가 "SetOperationMode command 지원"뿐 아니라 **현재 image에서 어떤 requested mode를 허용하는지** 명시적으로 알려야 한다.

신규 capability 정보 개념:

```text
SetOperationModeApiAvailable
SetOperationModeSupportedMask
  bit1  -> PP(1)
  bit3  -> PV(3)
  bit6  -> Homing(6)  // public path에서는 보통 0
  bit7  -> IP(7)
  bit8  -> CSP(8)
```

구현 위치는 기존 Admin capability response의 호환 가능한 확장 field 또는 별도 read-only capability command 중 wire ABI 검토 후 선택한다. **PC에만 hardcoded mask를 두지 않는다.**

WPF selector는 PLC mask와 SDK-known enum의 교집합만 보여준다.

### 5.2 Start enable 조건

Start 버튼은 최소 다음이 모두 참일 때만 활성화한다.

- Admin bits 8/9/10 paired ON
- stable DiagnosticsBuild/BootId/MapRevision
- requested mode가 PLC advertised supported mask에 포함
- exact physical axis selected
- no unresolved mutation/recovery
- SDO resource owner available
- current DS402 state가 mode change 정책을 만족
- explicit one-shot confirmation

### 5.3 rejection 진단 강화

현재 definitive rejection은 durable archive 후 interlock을 정상적으로 해제하지만, operator 진단 정보가 부족하다.

WPF log/status에 다음을 반드시 표시한다.

```text
RequestedMode
AxisReference
CommandStatus
ErrorId
DetailCode numeric + symbolic
RequestId
DiagnosticsBuild
BootId
MapRevision
Admin feature mask
SupportedModeMask
Current 0x6061 mode (read 성공 시)
```

예:

```text
SetOperationMode REJECTED
Axis=1 Requested=PV(3) Current=CSP(8)
Detail=43 UnsupportedMode
Admin=0x00000717 SupportedMask=0x0000018A
Build=... BootId=... MapRevision=...
No 0x6060 write was dispatched.
```

이 정보가 있어야 "PC source는 맞는데 PLC C78이 과거 버전"인지, "mode가 미지원"인지, "DS402 state가 unsafe"인지 구분할 수 있다.

### 5.4 mode별 범위

재설계 1차 bench 지원:

- PP = 1
- PV = 3
- IP = 7
- CSP = 8

Homing = 6은 `HomeDS402/HomeDS402Ex` owner와 충돌하므로 일반 SetOperationMode selector에서 제외한다.

PP/PV/IP로 변경한 뒤 ordinary LMC motion을 자동 허용하지 않는다. mode별 motion support가 별도로 준비되기 전까지 mode change 자체와 `0x6061` verification만 검증한다.

---

## 6. Ownership / resource model

`LMCSdoExecutor`는 물리적으로 slave별 1개 SDO mailbox adapter다. 다음 entry가 동일 slave에서 동시에 실행되면 안 된다.

- generic D5 Read/Write
- SetOperationMode
- HomeDS402/HomeDS402Ex internal SDO
- SetPosition의 SDO dependency가 있다면 해당 flow
- Encoder maintenance
- LASAL manual Server Write

따라서 기존 `LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE` arbitration을 확장해 **ManualServer owner**도 보이도록 한다.

최소 요구:

```text
owner source = None / DiagnosticsTicket / SetOperationMode / Home / Maintenance / ManualServer
slave reference = 1..4
operation token or manual generation
```

LASAL Class View에서 manual `ParaReadWrite`를 시작했을 때 service가 모르는 "숨은 SDO"가 되면 안 된다. 최소한 executor 자체 arbitration으로 programmatic request를 BUSY 처리하고, 가능하면 Diagnostics read-only status에도 manual owner가 노출돼야 한다.

---

## 7. generated source / LASAL IDE 경계

`LMCSdoExecutor.st` 상단은 LASAL2 CodeGenerator 산출물이며 직접 편집 시 다음 generation에서 덮어써질 수 있다.

따라서 Server method 변경은 다음 절차를 고정한다.

1. LASAL IDE에서 `LMCSdoExecutor` declaration/network 수정
2. `ParaReadWrite`, `ParaType`, `ParaString`, 신규 source/status server declaration 생성
3. generated `.st`와 `Classes.lcb` 갱신
4. Rebuild/Link error 0
5. method direct-open 확인
6. Network 연결에서 4개 executor -> 4개 Elmo slave exact connection 확인
7. C78 생성
8. PLC load 후 exact build/BootId/MapRevision 증거 수집

GitHub source/static PASS만으로 physical runtime 반영을 주장하지 않는다.

---

## 8. 필수 실기 시험 matrix

### 8.1 LMCSdoExecutor manual Server

각 Axis 1..4에서 최소:

1. `0x6061:0` Read
2. 안전한 read/write 가능 test object Write
3. 같은 object Readback
4. invalid index abort 확인
5. programmatic D5 active 중 manual start -> BUSY/거부, wire 0회
6. manual active 중 programmatic submit -> BUSY/거부, wire 0회
7. 완료 후 executor reusable

### 8.2 Generic Write

`0x2F00:24` 외 최소 3개 다른 writable object로 검증한다.

- 1-byte object
- 2-byte object
- 4-byte object

각 시험은 exact object dictionary와 장비 안전성을 사전에 확인한다.

필수 evidence:

- submit frame
- PLC ticket
- EtherCAT SDO download
- callback
- terminal status
- readback
- no duplicate write

### 8.3 SetOperationMode

각 supported mode에 대해:

```text
CSP -> PP -> CSP
CSP -> PV -> CSP
CSP -> IP -> CSP
```

각 전환에서:

- before `0x6061`
- exactly one `0x6060` write when change required
- after `0x6061` exact match
- same-mode request는 0x6060 write 0회
- unsafe DS402 state에서는 pre-write rejection
- response loss 후 Start replay 0회, read-only recovery만

---

## 9. 완료 정의

이 재설계는 다음을 모두 만족해야 완료다.

- WPF에서 `0x2F00:24` 외 임의의 valid object를 직접 입력해 Write 가능
- SDK가 compile-time exact target allowlist 때문에 일반 object를 거부하지 않음
- `LMCSdoExecutor` Class View에서 inherited Server 방식으로 Read/Write 실제 발동
- manual/programmatic 동시 접근이 race 없이 BUSY로 직렬화됨
- PP/PV/IP/CSP selector가 PLC advertised mask와 일치
- SetOperationMode rejection이 numeric/symbolic detail과 PLC identity를 화면에 노출
- exact C78/PLC load 후 physical test PASS
- uncertain mutation의 자동 replay는 모든 경로에서 0회

이 조건 전까지 기존 qualification branch의 UI 존재만으로 기능 완료를 선언하지 않는다.
