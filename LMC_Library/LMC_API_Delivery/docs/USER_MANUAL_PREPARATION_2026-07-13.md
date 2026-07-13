# LASAL Motion Control Lib 사용자 매뉴얼 제작 준비서

작성일: 2026-07-13

대상: `LasalMotionControlLib`

예정 최종 문서: `LASAL_MOTION_CONTROL_LIB_USER_MANUAL_KO.md`

배포 패키지 복사 위치:
`LMC_Library/LMC_API/LMC_API/docs/USER_MANUAL_KO.md`

현재 개발 등급: `.NET Framework 4.8`, C# 7.3,
assembly `0.9.0.0`, product `0.9.0-pc-api` preview

## 1. 결론

사용자 매뉴얼의 Markdown 원본 작성은 시작할 수 있다. PC API source, LASAL
handler, canonical 프로젝트 Rebuild와 implementation-search smoke까지 완료했다.
실제 PLC 시험은 아직 끝나지 않았으므로 최종 매뉴얼에서 `지원`이라고 표시할
기능은 아래 세
조건을 모두 만족한 항목으로 제한한다.

1. 공개 C# 호출 경로가 있다.
2. LASAL dispatcher가 실제 client method를 호출하거나 유효한 조회 응답을 만든다.
3. 출판 전 LASAL IDE build와 PLC smoke test를 통과한다.

PC에 메서드가 있어도 LASAL이 `-5`를 반환하는 기능은 사용 예제에 넣지 않고
`현재 미지원` 표에만 기록한다.

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

`LMC_Library/LMC_API/LMC_API` 패키지는 이번 변경에서 최신 CyWork-only source와
Release DLL/EXE로 다시 생성하고 `RELEASE_MANIFEST.md`의 size/hash를 갱신한다.
이 패키지는 실기 시험용 preview이며 PLC E2E 승인본은 아니다.

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
| `ReadStatusResult` | 활성 | ready/standstill 확인의 기본 API |
| `GetActualPositionResult` | 활성 | raw DINT를 UNIT으로 나누는 예제 포함 |

동기 API와 `CancellationToken`을 받는 async API를 별도 절로 정리한다. motion
명령의 ACK 성공은 축이 목표 위치에 도착했다는 뜻이 아니라 명령 접수 결과다.

### 그룹 API

| API | LASAL 실행 | 매뉴얼 처리 |
|---|---|---|
| `GetGroupMembersInfoResult` | 응답 path 활성 | 본문에 포함 |
| `GroupEnable`, `GroupDisable` | client call 활성 | 본문에 포함 |
| `GroupReadStatusResult` | client call 활성 | 본문에 포함 |
| `GroupReset` | `-5` | 현재 미지원 |
| `GroupStop` | `-5` | 현재 미지원 |
| `MoveLinearAbsoluteEx` | `-5` | 현재 미지원 |
| `GroupReadActualPosition` | `-5` | PC 구현만 완료, 현재 미지원 |
| `SetKinTransformCartesian4Axis` | `-5` | PC 구현만 완료, 현재 미지원 |

그룹 motion 완료 예제는 `MoveLinearAbsoluteEx`가 실제 PLC에서 승인되기 전에는
작성하지 않는다. `GroupReadStatusResult`는 valid bit `0x40000000`, standby bit
`0x00020000`과 `GroupErrorId`를 함께 확인하도록 안내한다.

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
- nonzero jerk 변환은 실제 motion profile 검증 전까지 예제에서 사용하지 않는다.

최종 매뉴얼에는 `ToDint`, `FromDint` helper와 현재 `_LMCAxis1..4`의 `DEG`
profile 예제를 포함한다.

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
11. 그룹 Enable/Disable/Status/Member 조회
12. 동기 API와 async/cancellation
13. `LMC_Response`와 typed result 판정
14. 오류 코드와 문제 해결
15. 안전 정지, 종료 및 재연결
16. 현재 미지원 API
17. 전체 예제
18. 버전, 변경 이력과 지원 문의 정보

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

- [x] 이번 `_ROBOT_ERROR$DINT` 수정 뒤 LASAL IDE Rebuild/Link 0 error
- [x] `Power`, `pos`, `velo` Find in Implementation smoke 성공
- [x] smoke 시작 뒤 `%TEMP%/Lasal2.log`에 새 `CInvalidArgException` 없음
- [ ] `TCPMotionInterface`와 axis RT thread의 CPU core/priority 조건 확인
- [ ] RPC init/callback/close 실제 PLC 왕복 확인
- [ ] `_LMCAxis1..4`와 group 실제 object name 확정
- [ ] 단일축 8개 API의 성공/실패 ACK 및 read response 재캡처
- [ ] 그룹 member/enable/disable/status 실제 응답 재캡처
- [ ] unsupported 5개 API가 `-5`를 반환하고 motion을 실행하지 않는지 확인
- [ ] PC 자동 테스트와 LASAL strict contract 통과
- [ ] 배포 DLL을 확정 commit에서 Release rebuild
- [ ] DLL version, file size, SHA-256과 source commit 기록
- [ ] 실제 PLC UNIT, software limit, ready/complete mask 확정
- [ ] 매뉴얼 표지의 제품명, 회사명, 버전, 지원 연락처 확정

## 9. 현재 준비 상태

| 항목 | 상태 |
|---|---|
| 공개 API inventory | 준비됨 |
| 지원/미지원 분리 | 준비됨 |
| UNIT 정책 | 준비됨 |
| 기본 C# 예제 | 존재, 최종 PLC 값 반영 필요 |
| 오류 모델 | source 기준 준비됨, PLC 실패 캡처 필요 |
| LASAL build | Rebuild/Link 및 implementation smoke 완료 |
| PLC E2E 근거 | 미완료 |
| 배포 버전/해시 | 최종 source 확정 뒤 재생성 필요 |
| 최종 문서 형식 | Markdown 원본 준비 후 PDF/DOCX 배포 형식 결정 필요 |

현재 바로 할 다음 작업은 최신 Release test app으로 read-only부터 실제 PLC smoke를
진행하고 packet/상태 결과를 기록하는 것이다. 그 결과를 반영해 최종 사용자 매뉴얼
Markdown 본문과 안전한 실행 예제를 작성한다.
