# Single Axis 실제 Power/Motion/Stop/PowerOff qualification

- 기준일: 2026-07-31
- 개발 UI: `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp`
- 구현: `MainWindow.Qualification.Axis.cs`
- 판정 경계: PC source/build/fake-RPC 계약 구현. current PLC/실축 합격은 아직 아님

## 목적

기존 Single Axis 버튼은 각각 실제 명령을 보냈지만, Power On부터 작은 이동과 최종 Power Off까지
한 번에 검증하는 실기용 순서와 구조화 evidence가 없었다. 새 runner는 기존 SDK와 durable recovery
경로를 재사용해 실제 Power/Motion/Stop/PowerOff를 전송하고, ACK와 완료 proof를 분리한다.

SDO Write는 별도 정책이다. Axis1 `0x2F00:24`, Int32/4-byte만 source-active이며 exact current
session/build/BootId/MapRevision/target의 four-ticket same-value proof가 끝난 뒤 manual Write가 열린다.
이 runner가 SDO gate를 우회하거나 SDO Write를 자동 실행하지 않는다.

## Wire 순서

1. capability를 읽어 connection owner/session, endpoint, `DiagnosticsBuild`, `BootId`, `MapRevision`과
   loaded Axis name/reference/lookup identity를 고정한다.
2. `0x2023(enable=true)`를 정확히 한 번 보내고 accepted evidence를 durable 기록한 뒤
   successful PowerOn 3회를 `0x2028`로 확인한다.
3. capability와 `0x2028`을 다시 읽어 error-free PowerOn + Referenced + Standstill을 요구한다.
4. `0x202E` 시작 위치를 읽고 checked Int32 `target = start + relative delta`를 계산한다.
5. fresh identity 확인과 durable motion arm 뒤 `0x20A0` Move Relative를 정확히 한 번 보낸다.
6. `0x2028`에서 non-Standstill을 한 번 이상 관측하고 뒤의 Standstill 3회를 확인한다.
7. `0x202E`를 3회 읽어 모든 값이 target tolerance와 sample range tolerance 안인지 확인한다.
8. 계획 이동이 끝난 상태에서 `0x2022`를 정확히 한 번 보내고 Standstill 3회를 확인한다.
9. `0x2023(enable=false)`를 정확히 한 번 보내고 PowerOff + Standstill 3회를 확인한다.

8번 Stop은 accepted-once/복구 경로 시험이다. 이미 계획 이동이 완료됐으므로 in-motion halt proof로
표시하지 않는다.

## 안전 및 복구 계약

- E-stop/STO/limit/travel, exact Axis/raw unit/target, exclusive owner/capture의 세 확인이 모두 없으면
  handler 호출에서도 RPC는 0건이다.
- relative delta는 nonzero이고 절대값 최대 1,000,000 raw DINT다. velocity, acceleration,
  deceleration, tolerance는 양수이며 첫 live slice Jerk는 0이다.
- 전체 runner는 `AxisQualificationRecoveryJournal`을 `0x2023(true)`보다 먼저
  `ArmedBeforePowerOn`으로 기록한다. Power On accepted/stable, Move prepared/accepted/stable,
  Stop accepted/stable, Power Off accepted, 최종 `SafeResolved`를 단조 checkpoint로 저장한다.
  Power, Motion, Stop은 기존 command-level command-before durable journal도 함께 사용하며 accepted
  observer는 첫 status 전에 자식 journal과 부모 checkpoint를 갱신한다.
- Move 이후 취소나 실패는 Move를 다시 보내지 않는다. exact durable identity가 유지될 때만
  cancellation-independent Stop과 Power Off를 실행한다.
- 외부 Axis Stop이 qualification에 끼면 runner Stop을 보내지 않고 외부 명령 종료 뒤 Standstill
  3회를 status-only로 재검증한 다음 runner Power Off 한 번만 수행한다. 외부 Axis Power Off이면
  PowerOff+Standstill 3회를 재검증하고 runner Stop/Power Off를 모두 생략한다.
- 현재 실행이 `ArmBeforePowerOn`에서 받은 process-local record GUID와 동일한 부모 record 및 동일
  session만 runner 내부 Power/Move mutation을 허용한다. session generation과 endpoint가 우연히
  같아도 재시작 record에는 이 volatile token이 없으므로 예외를 얻지 못한다.
  재시작 record와 다른 session은 이 예외를 얻지 못하므로 새 live mutation과 정상 Close가
  fail-closed된다. `ArmedBeforePowerOn`과 `MovePrepared` 상태에서 process가 죽으면 각각 Power On과
  Move가 전송됐을 수 있는 상태로 보수 승격하며 어떤 command도 자동 replay하지 않는다.
- DiagnosticsBuild/BootId/MapRevision/Axis identity가 바뀌면 다른 PLC 상태를 이전 명령 완료로
  추정하지 않고 새 cleanup mutation도 보내지 않는다. 정확한 기존 identity로 재접속한 경우에도
  status-only 관찰과 사용자가 명시한 Stop/Power Off만 허용한다. stable Power Off가 증명되고 부모
  `SafeResolved` tombstone이 먼저 저장된 뒤에만 자식 Power/Stop/Motion record를 해제한다.
- Power Off stable proof가 없으면 safe state는 `UNPROVEN`이다. stable Power Off는 확인됐지만 journal
  resolve가 실패한 경우 물리 상태 proof와 durable cleanup failure를 구분한다.
- 입력, Axis name/load 또는 connection session 변경과 run 종료 시 세 확인은 모두 해제된다.
- recovery-identity read-only quarantine이 남아 있으면 runner와 모든 live mutation은 계속 막힌다.
  현재 장비/드라이브 상태를 독립 확인한 뒤 상단 `Archive and Retire Stale Recovery`로 표시된 old-PLC
  record만 보관·폐기하고, 연결을 닫았다가 fresh reconnect/identity 검증을 마쳐야 새 실행이 열린다.
  retirement은 old command 결과를 성공으로 간주하지 않으며 Motion/Power/SDO command를 보내지 않는다.
  startup은 journal의 `ArmedBeforePowerOn`/`MovePrepared` crash 승격보다 먼저 immutable ledger의
  committed retirement를 exact original-byte CAS로 확정한다. pending decision이 없을 때만 volatile
  stage를 보수 승격하므로 retirement commit 직후 process loss도 영구 교착시키지 않는다.

## PC 검증 계약

focused fake-RPC는 다음을 검사한다.

- safety 미확인: RPC/Power/Move/Stop/PowerOff 0건
- 입력 수정: 세 확인 해제, 비활성 Run handler를 직접 호출해도 mutation 0건
- command gate 대기 중 취소: 추가 RPC와 Power/Move/Stop/PowerOff 0건, journal 비활성
- Move 직전 DiagnosticsBuild-only drift: PowerOn 뒤 Move/Stop/PowerOff 0건, safe state 미확정
- happy path: PowerOn 1, `0x20A0` 1, `0x2022` 1, PowerOff 1, `0x209F` 0,
  status 14, position 4와 exact 40-byte relative-move payload
- 첫 non-Standstill 직후 취소: Move 1/replay 0, Stop 1, PowerOff 1, 최종 `ABORTED`, stable
  PowerOff와 Motion/Power/Stop journal 해제
- 첫 non-Standstill 뒤 외부 Axis Stop: 전체 Stop 1회, runner Stop 0회, runner PowerOff 1회
- 첫 non-Standstill 뒤 외부 Axis Power Off: 전체 PowerOff 1회, runner Stop/PowerOff 0회
- process kill/restart: 부모 `PowerOnStable`, 자식 Power record `Resolved`에서 강제 종료한 뒤 exact
  identity로 재접속해도 관찰 barrier까지 mutation 0건이다. 명시적 `POWER_OFF` 뒤에만
  `0x2023(false)` 1회와 `0x2028` 3회가 실행되고 `0x2024` Reset은 0회이며 부모는
  `SafeResolved`가 된다.

현재 focused fake-RPC는 8/8, journal 단위 계약은 6/6, 실제 child-process kill/restart는 1/1
PASS다. 전체 WPF smoke 수치는 언어팩 통합 뒤 다시 고정한다.

이 검증은 실제 PLC download, drive readiness, E-stop/STO, software limit, 실제 이동 거리/방향,
정지 시간, packet capture를 증명하지 않는다.

## PLC 실행 시 필수 evidence

- current source의 LASAL build/download provenance와 BootId/MapRevision
- 실행 전 E-stop/STO/limit/reference/UNIT/axis 방향 확인
- TCP payload 기준 `0x2023(true) -> 0x2028 -> 0x202E -> 0x20A0 -> 0x2028 -> 0x202E x3
  -> 0x2022 -> 0x2028 -> 0x2023(false) -> 0x2028`
- command별 정확히 한 번, Move replay 0, final target/tolerance, 마지막 PowerOff+Standstill 3회
- PASS 표시 뒤 최소 2초 capture 유지
