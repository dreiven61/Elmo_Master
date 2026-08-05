# LMC SDO Write same-value qualification

- 작성일: 2026-07-28
- 대상: `LasalApiWpfTestApp`의 D5 SDO Write 최초 활성화 시험
- 현재 상태: Axis 1-only source/PC 활성, PLC download/실장 시험 미수행

## 1. 목적과 현재 제한

최초 SDO Write 시험은 승인된 한 축의 Gold `UI[24] (0x2F00:24)`에서 현재 값을 정확히
읽고, 그 4 bytes를 한 번만 그대로 Write한 뒤, 같은 대상을 다시 읽어 exact byte 일치를
확인한다. 값 변경 시험이 아니다.

현재 추적 source의 compile-time gate는 Axis 1 한 건에 한해 열려 있다.

- PLC global gate: `true`
- PLC Axis 1 `UI[24]` gate: `true`
- PLC Axis 2..4 gate: `false`
- SDK global gate: `true`
- SDK approved target: Axis/Slave 1 Gold `0x2F00:24`, `Int32`, 4 bytes,
  `-1073741823..1073741823` 정확히 1개

이는 source 승인이며 현재 PLC에 해당 build가 download되었거나 실장 SDO Write가
성공했다는 증거가 아니다. 일반 manual editor의 Write Submit은 동일 process에서
same-value qualification의 네 ticket과 exact readback이 PASS한 뒤에만 열린다. 이 proof는
현재 `LMCConnection` reference/session generation, `DiagnosticsBuild`, `DiagnosticsBootId`,
`MapRevision`과 exact approved target tuple/range에 귀속된 process-local 증거다. 재연결,
PLC identity/build 변경 또는 target 변경 뒤에는 재사용하지 않는다. 한 번이라도 identity
mismatch나 disconnect를 관측하면 proof 자체를 영구 폐기하므로 A -> B -> A로 값이 돌아와도
예전 proof가 다시 열리지 않는다.

## 2. 실행 순서

승인 후 실행기는 아래 순서를 바꾸지 않는다.

1. 현재 connection owner/session과 exact 단일 승인 target을 local preflight에서 확인한다.
2. 대상의 initial baseline을 `Read`하고 terminal `Completed/Success`와 exact Int32/4-byte 결과를
   확인한다.
3. fresh capability를 읽어 owner/session, BootId, MapRevision, capability bits와 payload contract가
   안정적인지 확인한다.
4. 축 안전 조건을 첫 번째로 검사한다.
5. 사용자의 네 가지 확인을 소비한다.
6. 두 번째 exact pre-Write guard `Read`를 수행하고 initial baseline과 bytes가 바뀌지 않았는지
   확인한다. 바뀌었으면 mutation 0건으로 중단한다.
7. guard Read가 끝난 직후 축 안전 조건을 두 번째로 다시 검사한다. 이 최종 검사와 journal arm
   사이에는 추가 async I/O를 수행하지 않는다.
8. durable mutation journal을 Write dispatch 전에 arm한다.
9. manual Write는 proof가 보유한 owner/session/build/BootId/MapRevision/target을 SDK의
   identity-pinned submit에 넘긴다. SDK는 mutation 직렬화 구간 안에서 fresh capability를 다시
   읽고 세 identity와 target을 exact 비교한다. mismatch면 `NotAttempted`, `0x7E50` 0회로
   중단한다.
10. baseline과 byte-identical한 4 bytes를 한 번만 `Write`한다.
11. 반환된 Write ticket을 durable journal/quarantine에 먼저 adopt한 뒤 ticket의 의미, provenance와
    terminal `Completed/Success`를 검증한다.
12. 같은 owner/session, target, type, length의 guarded `Readback`을 수행한다.
13. final fresh capability와 exact readback bytes가 모두 일치할 때만 journal을 `Resolved`로 기록한다.

Initial baseline Read, pre-Write guard Read, Write, Readback은 서로 다른 네 ticket을 사용한다.
두 번째 Read는 다른 writer가 값을 바꾼 경우를 Write 직전까지 좁혀 잡지만 Read와 Write를 하나의
원자적 compare-and-write로 만들지는 않는다. 따라서 명시적인 단일 writer 작업창 확보가 필수다.
Write 응답 유실이나 결과 불명확, terminal 실패, readback 불일치, identity 변경 또는 취소가
발생하면 Write를 재시도하지 않고 journal을 unresolved 상태로 남긴다.

## 3. 의도적으로 하지 않는 동작

- sentinel value를 쓰지 않는다.
- 자동 restore를 하지 않는다.
- accepted 또는 outcome-uncertain Write를 자동 replay하지 않는다.
- ACK만으로 성공을 판정하지 않는다.

same-value 시험은 baseline과 같은 bytes만 송신하므로 별도의 두 번째 restore Write를 추가하지
않는다. 그래도 장비의 물리 상태와 원래 값 확인 책임은 시험자가 가진다. durable journal과
GUI interlock은 증거를 보존하지만 Write 성공이나 장비 안전을 대신 증명하지 않는다.

## 4. 실장 시험 전 필수 조건

아래 조건을 모두 만족하기 전에는 Axis 1 source gate를 runtime/배포 승인으로
해석하지 않는다.

1. 사용자가 대상 드라이브 프로그램에서 `UI[24]`가 미사용임을 확인한다.
2. 최초 시험 대상은 source가 승인한 Axis 1 exact target 한 건으로 고정한다.
3. PLC와 SDK의 global/Axis 1 gate와 exact tuple/range가 동일한지 확인한다.
4. 변경한 tracked LASAL project를 IDE에서 Rebuild/Link하고 PLC에 download한다.
5. SDO executor mailbox 진단과 packet capture를 시험 시작 전부터 실행한다.
6. 시험 중 다른 도구, PLC 로직 또는 작업자가 같은 object를 쓰지 않는 명시적 단일 writer
   작업창을 확보한다.
7. baseline 값, 두 번의 안전 판정, pre-Write guard 값, Write ticket terminal, exact Readback,
   drive/axis 상태와 물리 동작을 함께 기록한다.

GUI의 네 가지 운영자 확인은 `UI[24]` 미사용, original/baseline 물리 복구 책임,
mailbox+pcap 실행, 명시적 단일 writer 작업창 확보다. 하나라도 확인되지 않으면 Write를 보내지
않는다.

PLC와 SDK gate가 서로 다르거나 승인 target이 정확히 1개가 아니면 qualification은
시작하지 않아야 한다. PASS가 만든 manual-editor activation proof는 영속 배포 승인이
아니며 해당 process/session/build/BootId/MapRevision/target에서만 유효하다. 시험 뒤에는
source 설정과 수집한 mailbox/pcap/물리 증거를 같이 보존한다.

## 5. 현재 검증 결과와 증거 경계

- 관련 C# PC contract와 WPF actual-control smoke는 current source에서 PASS했다.
- readiness와 마지막 실행 결과는 분리되어 checkbox reset/UI refresh 뒤에도
  `PASS` 또는 `RECOVERY REQUIRED` evidence를 보존한다.
- same-value runner PC test 9개: four-ticket 정상 순서/byte identity, preflight zero-wire,
  baseline/두 번의 safety/confirmation/journal 실패, changed pre-Write guard의 mutation 0건,
  returned Write ticket adoption-before-semantic-validation, uncertain Write no-replay, readback
  mismatch unresolved, arm 이후 cancellation unresolved를 검사한다.
- WPF smoke: current-session activation proof 전 manual editor Write가 zero-wire로 차단되고,
  proof가 connection/session/build/BootId/MapRevision/target 변경을 건너 재사용되지 않는다.
  identity-pinned SDK preflight의 Build/BootId/MapRevision mismatch는 capability Read까지만 수행하고
  `0x7E50`을 보내지 않으며, identity mismatch/disconnect 뒤 proof는 영구 폐기된다.
- 시작 로그 marker는
  `CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5`다.

위 결과는 PC/fake transport와 정적 계약의 증거다. 아직 아래 증거는 없다.

- current source와 일치하는 PLC cold download/provenance 결과
- 실제 PLC의 SDO Write mailbox 완료 기록
- 실제 EtherCAT/TCP packet capture의 Write 및 Readback 순서
- 선택 축에서의 drive 상태와 물리 무변화 확인

따라서 현재 분류는 `Axis 1 source/PC 활성, PLC/실장 qualification 대기`다.
`LMC_API_Distribution` 복제본은 이 current source/session-proof 계약과 동기화되지 않은
stale artifact이므로 실행 또는 배포 기준으로 사용하지 않는다.
