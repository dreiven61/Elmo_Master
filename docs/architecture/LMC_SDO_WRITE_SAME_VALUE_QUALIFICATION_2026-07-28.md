# LMC SDO Write same-value qualification

- 작성일: 2026-07-28
- 대상: `LasalApiWpfTestApp`의 D5 SDO Write 최초 활성화 시험
- 현재 상태: PC 구현 및 자동 시험 완료, PLC/실장 시험 미수행

## 1. 목적과 현재 제한

최초 SDO Write 시험은 승인된 한 축의 Gold `UI[24] (0x2F00:24)`에서 현재 값을 정확히
읽고, 그 4 bytes를 한 번만 그대로 Write한 뒤, 같은 대상을 다시 읽어 exact byte 일치를
확인한다. 값 변경 시험이 아니다.

현재 gate는 닫혀 있다.

- PLC global gate: `false`
- PLC axis 1..4 gate: 모두 `false`
- SDK approved target allowlist: empty
- 실제 승인 target: 0개

따라서 현재 GUI에서 버튼을 강제로 호출해도 Write request는 송신되지 않는다. 이 zero-wire
동작은 WPF smoke로 고정했다. 현재 상태는 실장 SDO Write 성공을 의미하지 않는다.

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
9. baseline과 byte-identical한 4 bytes를 한 번만 `Write`한다.
10. 반환된 Write ticket을 durable journal/quarantine에 먼저 adopt한 뒤 ticket의 의미, provenance와
    terminal `Completed/Success`를 검증한다.
11. 같은 owner/session, target, type, length의 guarded `Readback`을 수행한다.
12. final fresh capability와 exact readback bytes가 모두 일치할 때만 journal을 `Resolved`로 기록한다.

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

## 4. 활성화 전 필수 조건

아래 조건을 모두 만족하기 전에는 gate를 열지 않는다.

1. 사용자가 대상 드라이브 프로그램에서 `UI[24]`가 미사용임을 확인한다.
2. 최초 시험 축을 Axis 1..4 중 정확히 하나로 지정한다.
3. 그 축/target만 PLC allowlist와 SDK allowlist에 동일하게 등록한다.
4. 변경한 tracked LASAL project를 IDE에서 Rebuild/Link하고 PLC에 download한다.
5. SDO executor mailbox 진단과 packet capture를 시험 시작 전부터 실행한다.
6. 시험 중 다른 도구, PLC 로직 또는 작업자가 같은 object를 쓰지 않는 명시적 단일 writer
   작업창을 확보한다.
7. baseline 값, 두 번의 안전 판정, pre-Write guard 값, Write ticket terminal, exact Readback,
   drive/axis 상태와 물리 동작을 함께 기록한다.

GUI의 네 가지 운영자 확인은 `UI[24]` 미사용, original/baseline 물리 복구 책임,
mailbox+pcap 실행, 명시적 단일 writer 작업창 확보다. 하나라도 확인되지 않으면 Write를 보내지
않는다.

PLC와 SDK gate가 서로 다르거나 승인 target이 0개 또는 둘 이상이면 qualification은 시작하지
않아야 한다. 시험 뒤에는 gate를 다시 닫고, source 설정과 수집한 mailbox/pcap/물리 증거를
같이 보존한다.

## 5. 현재 검증 결과와 증거 경계

- C# PC contract Debug/Release: `636/636 PASS`
- WPF actual-control smoke Debug/Release: `32/32 PASS`; readiness와 마지막 실행 결과가 분리돼
  checkbox reset/UI refresh 뒤 `PASS` 또는 `RECOVERY REQUIRED` evidence를 보존한다.
- same-value runner PC test 9개: four-ticket 정상 순서/byte identity, preflight zero-wire,
  baseline/두 번의 safety/confirmation/journal 실패, changed pre-Write guard의 mutation 0건,
  returned Write ticket adoption-before-semantic-validation, uncertain Write no-replay, readback
  mismatch unresolved, arm 이후 cancellation unresolved를 검사한다.
- WPF smoke: empty allowlist와 gate-off 상태에서 강제 handler 호출이 PLC request 0건임을 검사한다.

위 결과는 PC/fake transport와 정적 계약의 증거다. 아직 아래 증거는 없다.

- gate-on LASAL Rebuild/Link 및 PLC download 결과
- 실제 PLC의 SDO Write mailbox 완료 기록
- 실제 EtherCAT/TCP packet capture의 Write 및 Readback 순서
- 선택 축에서의 drive 상태와 물리 무변화 확인

따라서 현재 분류는 `구현 완료, 활성화/실장 검증 대기`다.
