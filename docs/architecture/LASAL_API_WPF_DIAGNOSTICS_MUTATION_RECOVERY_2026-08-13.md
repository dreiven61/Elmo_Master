# LASAL API WPF Diagnostics Mutation 기능 복구 (2026-08-13)

이 문서는 diagnostics mutation, 연결 종료/X, Axis/Group Servo admission 복구만 다룬다.
별도 검토가 필요한 Recorder 기능의 구현 상태와 검증 결과는 이 문서 범위에 포함하지 않는다.

## 문제

`DiagnosticsMutationJournal`에 `SdoWrite / OutcomeUnverified`가 남으면 새 live 명령뿐 아니라 연결 종료와 창 닫기도 fail-closed로 차단된다. 저장된 BootId/MapRevision이 현재 PLC와 달라도 이 저널은 기존 reconnect identity 검사 대상이 아니어서 read-only quarantine으로 전환되지 않았다.

확인된 영향은 다음과 같다.

- Axis Power On, Group Power On, Group Enable: SDK/PLC 송신 전 차단
- Close Connection, Window X: UI admission 단계에서 차단
- PLC 또는 LMC API의 Power/Close 구현 실패가 아니라 WPF durable recovery admission 문제

## 변경

1. Connect 후 active `SdoWrite / OutcomeUnverified`의 저장 BootId/MapRevision을 현재 session-bound capabilities와 비교한다.
2. 불일치하면 기존 `RECOVERY IDENTITY READ-ONLY QUARANTINE`으로 전환한다.
   - Close Connection과 Window X는 허용한다.
   - Axis/Group Power, Group Enable, Motion, SDO Write 등 live mutation은 계속 zero-wire로 차단한다.
   - 원본 diagnostics journal은 연결/종료만으로 수정하지 않는다.
3. typed SDO v2 legacy record는 operator-confirmed stale retirement에 포함한다.
   - v2 원본에는 PLC endpoint가 없으므로 현재 quarantine endpoint를 원본 endpoint로 위장하지 않는다.
   - `OperatorClassifiedLegacyEndpoint`라는 별도 evidence kind로 기록한다.
   - 사용자 확인 전후에 동일 TCP session과 현재 BootId/MapRevision을 두 번 읽는다.
   - 원본 journal bytes/SHA-256와 operator endpoint classification을 immutable retirement ledger에 먼저 기록한다.
   - exact-byte CAS가 일치할 때만 diagnostics journal에 `Resolved` tombstone을 쓴다.
   - 성공 후 연결을 종료하고 애플리케이션 재시작을 요구한다.
4. ledger commit 후 journal resolve 전에 프로세스가 종료된 경우, 다음 시작에서 동일 원본 bytes와 ledger decision이 일치할 때만 crash-finalization한다.
5. 테스트 전용 Encoder 유지보수 operation 목록은 `Tw19MultiturnPositionReset`,
   `Tw20ErrorWarningReset` 순서이며 기본 선택은 TW19다.
   - 이는 UI 초기값일 뿐 Arm/Execute 또는 PLC RPC를 자동 수행하지 않는다.
6. Encoder admission이 recovery-identity read-only quarantine을 표시할 때
   `Open Safety / Recovery Details` 버튼으로 상단의 기본 접힘
   `Safety / recovery details` panel을 직접 펼칠 수 있다.
   - Recovery Identity retirement panel이 표시 대상이면 그 위치로 이동한다.
   - 이 동작은 PLC 명령을 전송하거나 journal/ledger/record를 변경 또는 폐기하지 않는다.

## 운영 절차

1. 새 빌드로 실행하고 기존 PLC에 Connect한다.
2. `RECOVERY IDENTITY READ-ONLY QUARANTINE`과 저장/current BootId 불일치를 확인한다.
3. 이 단계에서는 Close Connection과 Window X를 사용할 수 있다. Power/Motion/Write는 사용할 수 없다.
4. 실제 기계와 드라이브 상태를 독립적으로 확인한다.
5. Encoder 유지보수 영역에서 `Open Safety / Recovery Details`
   (`안전 / 복구 상세 정보 열기`)를 누른다. 기본 접힘 panel이 펼쳐지고
   Recovery Identity retirement panel로 이동한다. 이 버튼 자체는 zero-wire이며 stale record retirement를 실행하지 않는다.
6. 펼쳐진 Recovery Identity 패널에서 legacy endpoint 경고와 대상 SDO를 확인한다.
7. 확인 체크 후 `Archive and Retire Stale Recovery`를 실행한다.
8. 앱이 연결을 종료하면 재시작하고 다시 Connect/Load Axis 또는 Load Group을 수행한다.
9. Axis/Group 자체 recovery journal이 별도로 active가 아니면 Power On/Enable admission이 다시 열린다.

이 절차는 이전 SDO Write의 성공/실패를 증명하지 않는다. 이전 결과는 계속 `UNKNOWN`이며, Power/SDO/Motion 명령을 자동 재생하지 않는다.

## 현장 시험 합격 기준

1. retirement 전 연결
   - `10.10.150.1:4000` 연결 후 read-only quarantine과 stored/current identity 불일치가 표시된다.
   - `Close Connection`은 활성이고 Axis/Group Power On은 비활성이다.
   - `Close Connection` 클릭 후 `Disconnected`, 재연결 후 Window X 클릭 시 프로세스 종료가 확인된다.
   - 이 단계에서 diagnostics journal은 byte 수, 수정 시각, SHA-256이 바뀌면 안 된다.
   - `Safety / recovery details`는 기본 접힘 상태여야 한다. `Open Safety / Recovery Details`
     클릭 후 Expander와 Recovery Identity retirement panel이 표시되어야 하며, 클릭 전후 RPC 수와
     diagnostics journal bytes는 동일해야 한다.
2. operator-confirmed retirement
   - 기계와 드라이브의 안전 상태를 독립 확인한 사용자만 물리 확인 체크를 수행한다.
   - 확인 창에는 legacy endpoint 경고, 이전 SDO 결과 `UNKNOWN`, retire/keep 대상이 구분되어야 한다.
   - 성공 후 연결이 종료되고 restart-required 상태가 표시되어야 한다. Power/Motion/SDO 명령은 전송되면 안 된다.
3. 재시작 후 Axis Servo On/Off
   - `_LMCAxis1` lookup 후 Axis Power On이 활성화된다.
   - Axis Power On 실행 후 `Stable=3/3`이 확인되고 실패 로그가 없어야 한다.
   - 즉시 Axis Power Off를 실행해 실제 드라이브가 안전 상태로 복귀하는지 확인한다.
4. 재시작 후 Group Servo On/Off
   - `_LMCRobotBase1` lookup 후 Group Power On이 활성화된다.
   - Group Power On 실행 후 `Stable=3/3`이 확인되고 실패 로그가 없어야 한다.
   - 즉시 Group Power Off를 실행해 모든 멤버 축이 안전 상태로 복귀하는지 확인한다.
5. 재시작 후 테스트 전용 Encoder 유지보수 admission
   - Encoder 유지보수 화면의 초기 operation은 `Tw19MultiturnPositionReset`이어야 한다.
     초기 선택만으로 Arm/Execute나 `0x7E53`/`0x7E54`/`0x7E55`가 실행되면 안 된다.
   - capability refresh에서 `EncoderTw20ErrorWarningReset=True`와
     `EncoderTw19MultiturnPositionReset=True`를 확인한다.
   - TW20/TW19 각각에 대해 선택 축 Power Off, stable standstill, 실제 위치와 정확한
     `0x20FC` 대상 지원을 독립 확인한 뒤 Step 1 네 항목을 체크한다.
   - 이 시점에는 `Arm`만 활성화되고 `Execute`는 비활성 상태여야 한다.
   - `Arm`은 PC 메모리에 exact request만 준비하며 `0x7E53`을 송신하지 않는다.
     실제 `Execute`는 최종 Step 2 확인 뒤 별도 현장 시험으로 수행한다.

현장 결과에는 실행 EXE SHA-256, current BootId/MapRevision, 각 단계의 UI 결과 문자열과 PLC/드라이브 실제 상태를 같이 기록한다. UI 성공만으로 실제 Servo 상태를 확정하지 않는다.

## 검증 범위

- .NET Framework 4.8 Debug/Release Rebuild: 경고 0, 오류 0
- Debug/Release full smoke: 각각 `354/354 PASS` (전체 회귀 수치이며 Recorder 구현 상태나 검증 판정에는 사용하지 않음)
- Debug/Release `Wpf.RecoveryRetirement`: 각각 `20/20 PASS`
- 최종 Release EXE SHA-256:
  `AAC6A0CB53C32ADF8CA7BA3CE0DDEBC14CF5A59AAA924D77A6A480BF965FA05E`
- 최종 Release actual-EXE relaunch gate는 사용자가 실행 중인 Debug 예제 앱이 기본 named mutex를
  보유해 사전조건에서 중단됐다. 실행 중인 앱을 강제 종료하지 않았다. 이전 Release build에서는
  `1/1 PASS`였지만, 위 최종 SHA에 대한 relaunch 결과로 간주하지 않는다.
- fake RPC 기반 BootId mismatch -> read-only quarantine
- Close Connection 버튼 실제 handler와 connected Window X 허용, 각각 exact `0x405D` 1회
- 두 종료 경로에서 `0x2023`/`0x204A`와 다른 mutation RPC 0건, diagnostics 원본 bytes 보존
- legacy endpoint warning, immutable archive, exact-byte CAS, `Resolved` tombstone, restart-required
- restart/reconnect 후 `_LMCAxis1`과 `_LMCRobotBase1` lookup, Axis/Group Power On 실제 handler 실행,
  exact `0x2023`/`0x204A` 각 1회, 상태 조회 3회 연속 안정, 관련 recovery journal `Resolved`
- 같은 restart 세션에서 TW20/TW19 capability를 각각 선택하고 Step 1 네 항목을 만족한 뒤
  `Arm=True`, `Execute=False`와 내부 maintenance admission 통과를 검증했다. 실제 encoder command
  handler는 실행하지 않았고 `0x7E53`/`0x7E54`/`0x7E55`는 모두 0건이다.
- disconnected 초기 UI에서 TW19 기본 선택과 `LMCTw19MultiturnPositionResetRequest` mapping,
  영문/한글 표시를 검증
- read-only quarantine에서 `Open Safety / Recovery Details`가 기본 접힘 Expander와 retirement panel을
  표시하고 RPC 요청 수를 변경하지 않음을 검증
- diagnostics mismatch와 exact-current Group Reset이 같이 있어도 Close/X를 허용하고 Group Reset 원본은 보존
- ledger-commit crash-finalization
- ledger format v1/v2 read 호환 및 format v3 endpoint evidence kind 보존
- 관련 recovery retirement 및 process-termination 회귀

2026-08-13 실제 예제 앱에서 `10.10.150.1:4000`에 연결해 stored BootId `0x0000003B`,
current BootId `0x0000003D`, 동일 MapRevision `0x957F101E`의 read-only quarantine 진입을 확인했다.
수정된 Close 버튼으로 실제 연결 종료했고 connected X 종료도 확인했다. 이 과정에서 Servo On이나
SDO Write는 실행하지 않았으며, 실제 diagnostics journal SHA-256은
`3530B397832F6F576CFBA138ADCBF7A6460161A05022FA39A3E321E8AE0FD721`로 유지됐다.

2026-08-18 TW19 기본값/UI 동선 변경 전 Release EXE로 같은 endpoint를 다시 확인했다. current BootId는 `0x0000003E`,
MapRevision은 `0x957F101E`였고 read-only quarantine, 표시된 Close 버튼, 실제 연결 종료와 후속 X 종료가
정상 동작했다. 연결 종료 뒤 snapshot은 diagnostics record를 `active-endpoint-unbound/reconnect-required`로
표시했다. Servo On, SDO Write와 retirement 확인 UI는 실행하지 않았고 journal 시각·214 bytes·위 SHA-256은
그대로 유지됐다. 동일 연결에서 maintenance capability refresh 결과는
`EncoderTw20ErrorWarningReset=True`, `EncoderTw19MultiturnPositionReset=True`였다. 이는 명령 지원
광고의 현장 관찰이며 실제 encoder reset 결과 증거는 아니다.

2026-08-18 01:43:40Z에 default retirement ledger에 기존 diagnostics identity와 원본 SHA-256
`3530B397832F6F576CFBA138ADCBF7A6460161A05022FA39A3E321E8AE0FD721`의 immutable entry가 생성됐고,
default diagnostics journal은 동일 identity의 `State=Resolved` tombstone으로 전환됐다. 새 journal은
214 bytes, SHA-256 `A0EB23E95A46B6170CA2F9C2AF1595D9C914C0B2C521A9E83A2D26E6FB228E07`이며 checksum이
유효하다. 5초 뒤 최신 Debug 앱이 재시작된 것도 확인했다. 이는 해당 stale diagnostics interlock의
로컬 archive/resolve 증거이며 TW19 명령 실행이나 encoder 물리 결과 증거는 아니다.

같은 최신 Debug 앱의 live UI를 후속 확인한 결과, TW19가 기본 operation으로 표시됐고 admission은
`READY`였다. 사용자가 실행한 결과는 `RequestId=36`, `Kind=Tw19MultiturnPositionReset`,
`State=Succeeded`, `OriginalStatus/ErrorId/Detail=0`, `SdoAbort=0x00000000`,
`VerificationFlags=0x000003FF`, `StatusWord=0x02D0`, `AxisError=0`, `DriveError=0x00000000`,
`ActualPosition=6028554`였다. 정확한 terminal 결과와 일치하는 retirement 뒤 durable record도
resolve된 상태였다. 이는 실제 PLC의 TW19 SDO write terminal/cleanup 경로가 성공했다는 실기 증거다.
absolute multi-turn position의 물리적 reset 효과와 이후 LMC Home 현재 위치 0 설정 성공은 별도로
확인해야 하며, 이 UI 결과만으로 해당 물리 효과를 확정하지 않는다.

후속 live Axis4 화면에서는 `PowerOn=False`, `Home/Referenced=True`, `Standstill=True`와 Axis Power Off
status poll `3/3`이 확인됐고 Power On 버튼 admission이 다시 활성화돼 있었다. TW19 뒤 실행한 LMC Home도
`HomeSucceeded=True`, `OriginalStatus/ErrorId=0`, `RawDriveBefore=6028554`,
`RawDriveAfter=6028554`, `ActualApplicationAfter=0`, `SetApplicationAfter=0`으로 terminal/retirement를
완료했다. 따라서 stale diagnostics blocker 해제, Axis Power admission 복구, Power Off 안정 확인과
필수 application-position Home 0 설정까지는 실기 UI evidence가 있다. 이번 확인에서는 Power On 또는
다른 motion 명령을 새로 전송하지 않았으며, drive의 absolute multi-turn 물리 reset 효과는 여전히 사용자의
독립 확인 대상이다.

같은 session의 최신 Group Status UI는 `_LMCRobotBase1` reference `256`에 대해 `PowerOn=True`,
`Enabled/LockedStandby=True`, `FunctionStatus=0`, `ErrorId=0`, `GroupErrorId=0`을 표시했고,
준비 상태도 Power Ready/ACTIVE, identity reference/configuration, profile locked/standby가 모두 확인된
상태였다. 이미 Power On/Enable 상태이므로 해당 두 버튼이 비활성이고 Disable, Group Power Off, Stop이
활성인 것은 정상 admission이다. 이 확인에서도 새 Group 명령은 전송하지 않았다.

실제 PLC/드라이브 Servo On은 자동 테스트하지 않는다. 실제 하드웨어 검증은 사용자가 기계 안전 상태를 확인한 뒤 별도로 수행해야 한다.
