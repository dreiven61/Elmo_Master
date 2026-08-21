# LASAL Motion Control Lib 사용자 매뉴얼 제작 준비서

> **Historical preparation record:** current Markdown 정본은
> [API_MANUAL.md](../../../docs/api/API_MANUAL.md), current 진척도는
> [API_DEVELOPMENT_PROGRESS.md](../../../docs/api/API_DEVELOPMENT_PROGRESS.md)다.

작성일: 2026-07-13

최종 상태 갱신: 2026-07-23

> 이 문서는 매뉴얼 제작 준비 기록이다. 현재 제품 구조, 검증 수치와 릴리스
> 차단 항목은
> [ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md](../../../docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)를
> 우선한다. 2026-07-23 실기 캡처로 Admin/대표 motion/recovery, drive read와
> PI/Bulk happy path를 확인했지만 25-command full matrix와 fault/soak는 미완료다.
> 최신 packet 판정은
> [SIGMATEK Phase 1/2 Live Packet Capture Analysis](../../../docs/architecture/SIGMATEK_PHASE1_PHASE2_LIVE_CAPTURE_ANALYSIS_2026-07-23.md)를
> 따른다. `GroupReadActualPosition`의 None/ACS static slot 계약은 구현됐고
> MCS/PCS는 지원하지 않는다.

대상: `LasalMotionControlLib`

편집 원본:
`LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx`

Current 생성용 Markdown: `docs/api/API_MANUAL.md`

최종 PDF:
`LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf`

현재 개발 등급: `.NET Framework 4.8`, C# 7.3,
assembly `0.9.1.0`, product `0.9.1-preview`

## 1. 결론

사용자 매뉴얼의 Markdown 원본 작성은 시작할 수 있다. PC API source와 현재
캡처 기반 23개 command 및 LASAL-local Group Power extension 2개, C# 46 tests,
LASAL static/strict contract까지 완료했다. 다만 현재 group/9-axis source 반영 뒤 canonical 프로젝트 Rebuild와
implementation-search smoke, 실제 PLC 시험은 아직 끝나지 않았다. 최종
매뉴얼에서 `지원`이라고 표시할 기능은 아래 세 조건을 모두 만족한 항목으로
제한한다.

1. 공개 C# 호출 경로가 있다.
2. LASAL dispatcher가 실제 client method를 호출하거나 유효한 조회 응답을 만든다.
3. 출판 전 LASAL IDE build와 PLC smoke test를 통과한다.

PC에 메서드가 있어도 current LASAL source나 PLC 검증이 없는 기능은 배포용
완료 예제로 표시하지 않는다.

## 2. 매뉴얼 기준 자료

아래 순서로 사실을 확정한다.

1. 공개 API source
   - `src/LmcConnection.cs`
   - `src/LmcConnectionModels.cs`
   - `src/LmcAxis.cs`
   - `src/LmcGroup.cs`
   - `src/LmcGroupModels.cs`
   - `src/LmcResults.cs`
   - `src/LmcUnits.cs`
2. 안전한 기본 예제
   - `sample/BasicUsage.cs`
3. LASAL 실행 계약
   - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st`
   - `LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md`
   - `LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md`
4. 단위 계약
   - `UNIT_CONVERSION_MANUAL_2026-07-10.md`
5. 검증 근거
   - `tests/LasalMotionControlLib.Tests/`
   - `AUTOMATED_TESTS_2026-07-10.md`
   - `API_DEVELOPMENT_BACKLOG_2026-07-10.md`

폐기된 과거 task 분리 설계와 PMAS legacy LREAL API는 현재 사용 매뉴얼의 동작 근거로
사용하지 않는다.

`LMC_Library/LMC_API/LMC_API`는 구버전 보관본이다. 신규 패키지는
`LMC_Library/LMC_API_Distribution`의 `01_API`, `02_Example_Program`,
`03_API_User_Manual` 세 항목으로만 구성한다. 상세 build metadata와 개발 검증
기록은 외부 패키지에 포함하지 않고 내부 개발 문서에서 관리한다.

## 3. 매뉴얼에 포함할 현재 API 범위

### 연결과 객체 생성

| 분류 | 사용자에게 안내할 API | 상태 |
|---|---|---|
| 연결 | `LMCConnection`, `LMCConnectionOptions` | 매뉴얼 작성 가능, PLC 재검증 대기 |
| 초기화 | `RpcInitConnection`, `RpcInitConnectionAsync` | 매뉴얼 작성 가능, PLC 재검증 대기 |
| 종료 | `CloseConnection`, `CloseConnectionAsync`, `Dispose` | 매뉴얼 작성 가능, PLC 재검증 대기 |
| 상태 | `State`, `IsConnected`, `IsRpcInitialized`, `ConnectionStateChanged` | 매뉴얼 작성 가능 |
| callback | `CallbackReceived`, `CallbackListenerError` | raw datagram 진단용만 안내 |
| 축 lookup | `LMCSingleAxis` 생성자, `LMCSingleAxis.CreateAsync` | 매뉴얼 작성 가능, 실제 object name 확인 필요 |
| 그룹 lookup | `LMCGroupAxis` 생성자, `LMCGroupAxis.CreateAsync` | 매뉴얼 작성 가능, 실제 object name 확인 필요 |

기본 예제에서는 `LMCSingleAxis`와 `LMCGroupAxis`를 사용한다. 호환용 파생 이름인
`LMCAxis`와 `LMCGroup`은 API 목록에는 기록하되 신규 예제의 기본 형식으로 쓰지
않는다.

연결 예제는 PLC TCP 주소/포트와 별도로 PC NIC의 구체적인 local IPv4를 받는다는
점을 설명한다. `0.0.0.0`은 허용하지 않는다. 기본 callback port는 `5003`, 기본
event mask는 `0xFFFFFFFF`이며 방화벽에서 TCP와 UDP 경로를 모두 확인한다.
axis/group name은 DLL 고정값이 아니라 LASAL 실제 object name과 정확히 일치하는
printable ASCII 1~79자여야 한다. 재접속 뒤에는 이전 axis/group 객체를 버리고
다시 lookup한다.

### 단일축 API

| API | LASAL 실행 | 매뉴얼 처리 |
|---|---|---|
| `PowerOn`, `PowerOff` | 활성 | 본문과 안전 순서에 포함 |
| `Reset` | 활성 | 요청 접수 ACK 의미를 명시 |
| `Stop` | 활성 | 감속도와 jerk UNIT 주의 포함 |
| `MoveAbsoluteEx` | 활성 | 본문 예제 대상 |
| `MoveRelativeEx` | 활성 | 본문 예제 대상 |
| `MoveVelocityEx` | 활성 | direction과 signed velocity 제약 포함 |
| `ReadStatusResult` | 활성 | power/home(`IsReferenced`)/standstill 확인의 기본 API |
| `GetActualPositionResult` | 활성 | raw DINT를 UNIT으로 나누는 예제 포함 |

동기 API와 `CancellationToken`을 받는 async API를 별도 절로 정리한다. motion
명령의 ACK 성공은 축이 목표 위치에 도착했다는 뜻이 아니라 명령 접수 결과다.

### 그룹 API

| API | LASAL 실행 | 매뉴얼 처리 |
|---|---|---|
| `GetGroupMembersInfoResult` | 응답 path 활성 | 본문에 포함 |
| `GroupPowerOn`, `GroupPowerOff` | LASAL-local `RobotOn`/`RobotOff` 시작 요청 활성 | ACK 뒤 `IsPowerOn`으로 실제 상태를 확인한다고 명시 |
| `GroupEnable`, `GroupDisable` | `LockProfile`/`UnlockProfile` 활성 | servo power와 분리된 profile lock API로 설명 |
| `GroupReadStatusResult` | client call 활성 | `IsPowerOn`/`IsStandby`/`IsDisabled` 상태 의미 포함 |
| `GroupReset` | `AxQuitError(AxisNo:=0)` 활성 | axis/hardware error reset이며 profile error 전체 reset이 아님을 명시 |
| `GroupStop` | `StopMove(Mode:=3)` 활성 | deceleration/jerk, Aborting 제약과 ACK 후 상태 확인 포함 |
| `MoveLinearAbsoluteEx` | static 4축 `MoveLinearCoord` 활성 | 승인된 coordinate/transition/buffer 조합만 예제화 |
| `GroupReadActualPosition` | static member-slot 68B DINT 응답 활성 | None/ACS alias, MCS/PCS unsupported와 slot 1..9/10..16 zero를 명시 |
| `SetKinTransformCartesian4Axis` | exact identity 검증과 static mapping 등록 활성 | profile lock이나 dynamic transform 생성 기능이 아님을 명시 |

`MoveLinearAbsoluteEx`는 position 1..4만 사용하고 5..16은 0이어야 한다.
coordinate는 `None(0)`, transition은 `ExactStop(0)` 또는
`ContinuousDirect(2)`, buffer는 `Aborting(1)` 또는 `Buffered(2)`만 허용한다.
그룹 motion 예제 원본은 작성할 수 있지만 실제 PLC에서 승인되기 전에는
배포용 완료 예제로 표시하지 않는다. `GroupReadStatusResult`는 valid bit
`0x40000000`, LASAL-local Power Ready `0x00040000`, Maestro 표준 standby
`0x00020000`/disabled `0x00010000`과 `GroupErrorId`를 함께 확인한다.

`MoveCircle`은 현재 공개 C# API와 승인된 LASAL-DINT command ID/payload 계약에
없다. 최종 매뉴얼의 API 사용법에는 포함하지 않고, 현재 버전 범위 밖이라고
명시한다.

## 4. 반드시 포함할 UNIT 계약

사용자 프로그램이 API 호출 전에 변환한다.

```text
송신 DINT = 물리값 x PLC에 등록된 UNIT
표시 물리값 = 수신 DINT / 같은 UNIT
```

- DLL은 입력 `int`를 자동 변환하지 않는다.
- LASAL도 수신 DINT를 다시 변환하지 않는다.
- 축과 인자마다 PLC 설정 UNIT이 다를 수 있다.
- `8,388,608`은 더미 23-bit encoder 예제이며 공통 배율이 아니다.
- DINT 변환에는 `checked`와 명시적 반올림을 사용한다.
- `_LMCAxis` Jerk의 단위는 `axis application unit/s^3/1000`이다. 따라서
  `Jerk DINT = (물리 jerk / 1000) x 축 UNIT`으로 변환한다.
- nonzero Jerk는 `_JERK_PROFILE`에서만 효과가 있다. 현재 저장된
  `_LMCAxis1..9`는 `_JERK_PROFILE`, `JMax=75000 mm`지만 실제 다운로드된 PLC
  설정과 장비 허용 범위는 시험 전에 다시 확인한다.
- group motion의 nonzero Jerk도 `_LMCRobotBase1.MoveType=_JERK_PROFILE`이어야
  효과가 있다. canonical network는 `_JERK_PROFILE`, `JMax=50000 mm`로
  저장돼 있으며 PLC download 뒤 동일한지 다시 확인한다.

최종 매뉴얼에는 `ToDint`, `ToJerkDint`, `FromDint` helper와 현재 저장된
`_LMCAxis1..9`의 `MM`/`_JERK_PROFILE` 예제를 포함한다. 실제 장비 safety와
PDO 검증 대상은 physical axis 1..4이고, axis 5..9는 `SimulateMode=1` software
axis라는 점을 분리해서 설명한다.

## 5. 최종 매뉴얼 목차

1. 제품 개요와 적용 범위
2. 지원 환경과 설치
3. PLC/LASAL 사전 설정
4. DLL 참조와 namespace
5. RPC 연결, callback, 종료 순서
6. axis/group object name lookup
7. UNIT 변환 규칙
8. 단일축 Power/Reset/Stop
9. 단일축 Absolute/Relative/Velocity motion
10. ReadStatus와 ActualPosition
11. 그룹 Enable/Disable/Reset/Stop
12. 그룹 Status/Member/ActualPosition 조회
13. static 4축 MoveLinear와 identity kinematic profile
14. 동기 API와 async/cancellation
15. `LMC_Response`와 typed result 판정
16. 오류 코드와 문제 해결
17. 안전 정지, 종료 및 재연결
18. 현재 범위 밖 API와 제한
19. 전체 예제
20. 버전, 변경 이력과 지원 문의 정보

## 6. 준비할 코드 예제

| 예제 | 필수 내용 |
|---|---|
| 최소 연결 | timeout 설정, state event, `RpcInitConnection`, `Dispose` |
| 안전한 축 생성 | PLC 실제 object name, lookup 실패 처리, stale handle 금지 |
| UNIT 변환 | `checked`, 반올림, 송신 곱셈과 수신 나눗셈 |
| Power/ready | `PowerOn` ACK, `ReadStatusResult`, project-specific ready 판정 |
| Absolute move | 변환된 DINT 인자, ACK 검사, 완료 polling |
| Relative/Velocity | signed 값과 direction 제약, Stop 경로 |
| 읽기 | typed result의 `IsSuccess`, `AxisErrorId`, raw position 변환 |
| 그룹 상태 | Enable, `GroupReadStatusResult`, standby mask와 `GroupErrorId` |
| 그룹 reset/stop | `GroupReset`, `GroupStop`, ACK와 정지 완료 상태 확인 |
| 그룹 위치 | `GroupReadActualPosition`, tracked source의 slot 1..9와 실제 axis-order를 대조하고 공개 범위 결정 |
| 그룹 선형 이동 | 4축 position, 승인된 coordinate/transition/buffer, 완료 polling |
| kinematic profile | X/Y/Z/U axis lookup, exact identity `SetKinTransformCartesian4Axis`, dynamic transform가 아님 |
| async | `CancellationToken`, in-flight 취소 시 transport fault와 command outcome unknown, 재연결 전 객체 재사용 금지 |
| 종료 | Stop/PowerOff 정책, `CloseConnection` 또는 `Dispose` |

실제 motion 예제는 비상정지, software limit, power-ready 조건이 장비 프로그램에
구현됐다는 전제 없이 자동 실행되는 형태로 배포하지 않는다.
취소 토큰은 안전 정지 수단이 아니다. 송신 뒤 취소되면 PLC 명령 적용 여부를
PC가 확정할 수 없으므로, motion은 별도 `Stop`/`PowerOff`와 상태 확인 절차로
종료하고 안전 관련 workflow 중에는 UI Cancel을 허용하지 않는다.

## 7. 오류 설명 준비

최종 매뉴얼은 최소한 아래 LASAL local error를 구분한다.

| ErrorId | 의미 |
|---:|---|
| `-1` | RPC/session 상태 오류 |
| `-2` | object lookup 실패 또는 LASAL client 미연결 |
| `-3` | 잘못된 descriptor, payload 또는 요청 형식 |
| `-4` | 알 수 없는 command |
| `-5` | 현재 LASAL에서 지원하지 않는 기능 |
| `-6` | 16-bit 응답에 보존할 수 없는 오류 또는 일관되지 않은 robot error 상태 |
| `-7` | 지원하지 않는 motion 인자 조합 |
| `-8` | queue/transport framing 오류 |

이 표와 별개로 `LMC_Response.IsSuccess`, typed result `IsSuccess`,
`AxisErrorId`/`GroupErrorId`, 예외를 모두 확인하는 예제를 제공한다.

## 8. 출판 전 필수 gate

- [x] C# 자동 테스트 46/46 통과
- [x] 현재 LASAL static/strict contract 통과
- [x] WPF example VS2019 MSBuild Debug 통과
- [ ] 현재 group/9-axis source 반영 뒤 LASAL IDE Rebuild/Link
- [ ] group method를 `Edit Method` 또는 `Enter`로 직접 열어 exact Implementation header를
      확인하고, 이후 `%TEMP%/Lasal2.log`의 신규 `CInvalidArgException` 확인
- [ ] `TCPMotionInterface`와 axis RT thread의 CPU core/priority 조건 확인
- [ ] RPC init/callback/close 실제 PLC 왕복 확인
- [ ] `_LMCAxis1..9`와 group 실제 object name, physical 1..4/simulated 5..9 구분 확정
- [ ] 단일축 8개 API의 성공/실패 ACK 및 read response 재캡처
- [ ] 그룹 power-on/off, member/enable/disable/status/reset/stop 실제 응답 재캡처
- [ ] static 4축 MoveLinear 성공/인자 오류와 완료 상태 재캡처
- [ ] GroupReadActualPosition 68B DINT slot 1..9와 static identity axis order 확인 후 4축/9축 공개 계약 확정
- [ ] SetKin exact identity 1320B request/4B ACK와 static mapping 등록 확인
- [ ] GroupEnable/Disable의 `LockProfile`/`UnlockProfile` 결과 확인
- [ ] 배포 DLL을 확정 commit에서 Release rebuild
- [ ] package 내부 `RELEASE_MANIFEST.md`의 source commit, clean 상태, DLL version/3복제 identity와 모든 파일 SHA-256 확인
- [ ] 실제 PLC UNIT, transmission ratio, software limit, ready/complete mask 확정
- [ ] 현재 1 mm/rev Git 설정 다운로드 후 physical 4축의 IntUnits/MaxModulo/BinOffset/absolute offset readback 및 재참조 확인
- [ ] 매뉴얼 표지의 제품명, 회사명, 버전, 지원 연락처 확정

## 9. 현재 준비 상태

| 항목 | 상태 |
|---|---|
| 공개 API inventory | 준비됨 |
| 지원/범위 밖/제한 분리 | source 기준 준비됨, PLC 승인 대기 |
| UNIT 정책 | 준비됨 |
| 기본 C# 예제 | 존재, 최종 PLC 값 반영 필요 |
| WPF example build | VS2019 Debug PASS, PLC 동작 승인 아님 |
| 오류 모델 | source 기준 준비됨, PLC 실패 캡처 필요 |
| LASAL static contract | PASS |
| 현재 LASAL IDE build | group/9-axis source 반영 뒤 미검증 |
| PLC E2E 근거 | 2026-07-23 대표 happy path 캡처 PASS; 25-command full matrix와 fault/soak 미완료 |
| 배포 버전/해시 | `0.9.1-preview` 산출물과 내부 hash 기록 존재, production 승인 아님 |
| 최종 문서 형식 | DOCX/PDF 형식은 확정, 외부 문서 버전 1.0을 내부 Markdown 1.4로 재출판 필요 |

현재 바로 할 다음 작업은 canonical project의 Rebuild/Link와 implementation
smoke를 먼저 수행한 뒤 최신 Release test app으로 read-only부터 실제 PLC smoke를
진행하고 packet/상태 결과를 기록하는 것이다. 그 결과를 반영해 최종 사용자 매뉴얼
Markdown 본문과 안전한 실행 예제를 작성한다.
