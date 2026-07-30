
3. Bulk 시험

   - `Run 24-entry Snapshot Soak`: 100회
   - `Run Lifecycle Soak`: 100회
   - 모든 snapshot이 24개, 동일 cycle, Valid, 순서 일치
   - 마지막 Release까지 캡처

4. Recorder 시험

   - `Run Single`
   - `Run Ring`
   - `Run Trigger Soak`: 100회
   - `Run Reconnect Exact`
   - `Run Reconnect Discovery`
   - Header, 두 번의 Download SHA-256, StopReason, TriggerIndex와 Release 확인

5. D5 Read-only fault 시험

   이 순서로 하고 Abrupt Disconnect를 마지막에 실행합니다.

   - `Run D5 Abort → Recovery`
   - `Run D5 Contention → Recovery`
   - `Run D5 Timeout → Recovery`
   - `Run D5 Queued Cancel → Recovery`
   - `Run D5 Abrupt Disconnect → App Recovery`

   각 시험은 QTEST 로그와 pcap을 반드시 같이 저장해야 합니다. 세부 PASS 조건은 [Qualification 자동화 절차](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/README.md:115)에 정리돼 있습니다.

6. Fault injection

   Group PowerOff·Disabled와 4축 정지를 먼저 확인한 뒤 실행합니다.

   - `Bulk One-Slave-Offline Partial`
   - 정확히 Elmo 한 축만 Offline
   - 해당 축의 6개 PI만 `SlaveOffline`
   - 같은 축을 OP로 복구한 뒤 24개 모두 Valid 확인
   - cable loss/reconnect, stale session도 별도 캡처

7. Motion은 마지막

   - `Run Enable ACK → Locked`
   - `Run Deterministic Stop-First`
     - 예상 wire: Move `0x7D22` 0건, Stop `0x2085` 1건
   - `Run Buffered A → B`
     - A가 움직이는 동안 B가 실제 queue되는지 확인
   - 단축 `Move Relative` 작은 왕복
   - 단축 `Move Velocity → Stop → Standstill 3회`
   - Group 종료 후 Position readback과 PowerOff 3회 안정 확인

## 지금 시험하면 안 되는 기능

현재 capability/gate가 꺼져 있으므로 아래 항목은 정상적으로 비활성입니다.

- `0x7E13` CREVIS/node live health
- `0x7E22` Digital Input
- `0x7E23` Digital Output
- SDO Write
- PI Write
- Recorder Double Bank
- 8/12-byte SDO와 extended result

SDO Write는 `UI[24]`가 사용자 드라이브 프로그램에서 미사용인지와 적용할 첫 축이 확정되기 전에는 활성화하면 안 됩니다. 현재 남은 전체 runtime gate는 [release status](/C:/work/Elmo/Elmo_Master/docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md:1191)에서 확인할 수 있습니다.

Wireshark 캡처 필터는 다음으로 사용하면 됩니다.

```text
host 10.10.150.1 and (tcp port 4000 or udp port 5000)
```

표시 필터:

```text
ip.addr == 10.10.150.1 && ((tcp.port == 4000 && tcp.len > 0) || udp.port == 5000)
```

버튼 누르기 전에 캡처를 시작하고, `PASS/FAIL`과 cleanup 최종 상태가 나온 뒤 2초 후 종료하십시오. 이번 파일 중 Connect·Refresh·Catalog·Topology는 이 2초 보존 기준을 충족하지 않습니다. 기능 판정에는 충분하지만 정식 qualification 자료로 쓸 경우 이 네 개만 재캡처하는 것이 맞습니다.

> 다른 작업 하느라 테스트를 진행 못했다. 현 시점 기준으로 테스트 할 내용 공유해줘. 니가 테스트 한건 빼

<details><summary>6 previous messages</summary>

> 현재 소스와 캡처 이력을 다시 대조해서, 제가 실행한 PC 자동시험·빌드·정적검증은 목록에서 제외하겠습니다. 사용자가 장비에서 직접 해야 하는 PLC/실축 시험만 우선순위와 버튼명 기준으로 정리하겠습니다.
>
> 어제 이후 소스가 추가로 바뀌었습니다. 현재 작업 트리는 tracked 122개·untracked 111개 상태이고, Axis Power/Stop/Reset 및 Group Disable 복구 계약이 새로 들어와 있습니다. 따라서 어제 목록을 그대로 반복하지 않고 새로 생긴 실기 검증까지 포함해 다시 추리고 있습니다.
>
> 오늘 오전 LASAL 로그에는 Rebuild/Download 성공 기록이 있지만, 다운로드 경로가 canonical 저장소가 아니라 `C:\work\Elmo\Elmo_Master_test\...`로 찍혀 있습니다. 현재 저장소와 그 다운로드본이 같은지 확인한 뒤, 장비 시험 시작 가능 여부부터 확정하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 목록을 “기존 캡처로 끝난 기능”과 “7월 29~30일 이후 바뀌어 실제 PLC에서 다시 증명해야 하는 기능”으로 분리했습니다. 특히 예전 Power/Stop 캡처는 기본 동작 증거일 뿐, 새 accepted-once·재개 로직의 증거로는 쓰지 않겠습니다.
>
> 최종 목록은 안전 순서로 배열하고 있습니다. 결과 파일은 각 항목마다 `pcapng + QTEST 로그`, 0x2045 성능 시험은 여기에 `RPC CSV`까지 남기는 기준으로 정리하겠습니다.
</details>

현 시점에서 남은 것은 실제 PLC·드라이브·네트워크 시험입니다. 제가 실행한 PC 자동 테스트, 빌드, 정적 검증은 모두 제외했습니다. 기존 실기 캡처로 확인된 기능도 제외하되, 7월 28일 이후 구현이 변경된 Power/Stop/Reset 계열만 다시 포함했습니다.

중요한 기준선:

- 오늘 08:36 Rebuild/Link, 08:37 PLC Download가 성공했습니다.
- 다운로드한 프로젝트는 `C:\work\Elmo\Elmo_Master_test\Elmo_EtherCAT_Test_4Axis`입니다.
- 주요 `.st`와 Network 소스는 현재 저장소와 같지만 생성 파일 `Classes.lcb`는 다릅니다. 시험 결과에는 반드시 `_test` 다운로드 기준이라고 기록해야 합니다.
- 기존 캡처는 7월 28일까지이고, 새 accepted-once/recovery 구현은 그 이후 변경됐습니다.

## P0. 시험 시작 전 안전 확인

아래는 기능시험이 아니라 필수 시작 조건입니다.

1. Connect
2. `Refresh Capabilities`
3. `Load PI Catalog`
4. EtherCAT Master OP, 축 1~4 Online/OP 확인
5. 축 1~4 각각 실행:

   `Get Drive Operation Mode → Read Drive Status → Get Drive Error Code`

지난 Test2에서 모든 축에 DS402 Warning이 있었습니다. 원인이 확인되지 않거나 `0x6041` Fault, `0x603F` 오류가 있으면 모션 시험은 중단해야 합니다.

## P1. 움직임 없는 시험부터

다음 순서로 진행하십시오.

1. `Run 24-entry Snapshot Soak`

   - 100회, 10 ms
   - 24개 항목 순서/유효성/동일 사이클 확인

2. `Run Configure/Read/Release Soak`

   - 100회
   - Configure → Active → Snapshot → Release 반복

3. Recorder

   - `Run Single Manual`
   - `Run Ring Forced Trigger`
   - `Run Trigger Lifecycle Soak` 100회
   - `Run Reconnect Exact Adopt`
   - `Run Reconnect 0/0 Discovery`

Recorder는 기존 캡처에 요청 `0x7E40~0x7E4F`가 한 건도 없어 전부 실기가 필요합니다.

## P2. 새 Power/Enable 완료 판정

기존 기본 Power 캡처를 반복하는 목적이 아닙니다. 새 구현의 “명령 한 번 + 상태 확인”을 검증합니다.

| 시험 | 패킷 합격 기준 |
|---|---|
| Axis Power On | `0x2023(enable=true)` 정확히 1회, 이후 `0x2028`만 polling, 마지막 PowerOn 3회 안정 |
| Axis Power Off | `0x2023(enable=false)` 정확히 1회, 이후 `0x2028`, PowerOff+Standstill 3회 |
| Group Power On | `0x204A` 정확히 1회, 이후 `0x2045`, PowerOn 3회 |
| `Run Enable ACK -> Locked` | `0x2047` 정확히 1회, 이후 `0x2045`, Locked Standby 3회 |
| Group Power Off | `0x204B` 정확히 1회, 이후 `0x2045`, PowerOff 3회 |
| Group Stop | `0x2085` 정확히 1회, 이후 `0x2045`, Standby 3회 |

Group Enable 전제는 `PowerOn + Disabled/Unlocked` 3회 안정과 `Set Identity` 완료입니다.

그다음 `Run Read-only 0x2045 RPC`를 실행하십시오.

- Warm-up 100
- Measured 10,000
- 전 응답 성공
- 20-byte frame / 12-byte payload
- PowerOn + Locked + InPosition 유지
- 원시 응답 바이트 동일
- `QTEST Log + RPC CSV + pcapng` 모두 저장

구현 기준은 [accepted-once 설명](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/README.md:404)에 있습니다.

## P3. 모션·정지 시험

기구 여유, 소프트웨어 리미트, 비상정지 수단을 확인한 뒤 진행하십시오.

1. `Run Buffered A -> B`

   - A가 아직 InPosition이 아닐 때 B가 전송돼야 함
   - 누적 목표 위치 도달
   - 마지막 InPosition 안정
   - 시작 위치로 정상 복귀

2. `Run Deterministic Stop-First`

   패킷 기준은 반드시 다음과 같아야 합니다.

   - Group Move `0x7D22`: 0회
   - Group Stop `0x2085`: 정확히 1회
   - 이후 `0x2045` Standby 3회

3. Axis Stop

   - 작은 저속 이동 중 실행
   - `0x2022` 정확히 1회
   - 이후 `0x2028`만 polling
   - Standstill 3회와 최종 위치 확인

4. Axis Reset

   - 실제로 안전하게 복구 가능한 Fault가 있을 때만 실행
   - `0x2024` 정확히 1회
   - 이후 `0x2028`, `AxisErrorId=0` 3회
   - 전후 `AxisErrorId`, DS402 `0x6041` Fault bit, `0x603F`를 각각 저장

`AxisErrorId=0`만으로 DS402 Fault나 드라이브 오류가 해제됐다고 판정하면 안 됩니다.

## P4. Fault·복구 시험

모든 정상 시험이 통과한 뒤 진행하십시오.

1. D5 SDO Read fault/recovery

   - `Run D5 Abort -> Recovery`
   - `Run D5 Contention -> Recovery`
   - `Run D5 Timeout -> Recovery`
   - `Run D5 Queued Cancel -> Recovery`
   - `Run D5 Abrupt Disconnect -> App Recovery` — 가장 마지막

전제는 대상 축 `PowerOn=False`, `Standstill=True`, Actual Position 3회 동일입니다. Abrupt Disconnect PASS는 애플리케이션 복구만 증명하며 PLC orphan cleanup 증거는 아닙니다.

2. `Run One-Slave-Offline Partial`

   - Group PowerOff + Disabled
   - 4축 위치 3회 동일
   - 정확히 한 slave만 Offline
   - `Resume External Step`
   - 같은 slave를 OP로 복구
   - 다시 `Resume External Step`
   - 최종 24개 항목 전부 Valid 확인

3. 현재 마스터 프로젝트 TCP takeover

   - 기존 연결 A 유지 중 같은 PC/IP에서 B 연결
   - A가 종료되고 B가 정상 RPC 수행
   - 100회 reconnect soak
   - 다른 IP 연결은 기존 owner를 교체하지 않는지 확인
   - 모션 중 takeover는 전체 시험의 마지막에만 진행

## Wireshark 조건

Capture Filter:

```text
host 10.10.150.1 and (tcp port 4000 or udp port 5000)
```

Display Filter:

```text
