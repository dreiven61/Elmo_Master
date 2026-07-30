# 캡쳐파일 분석 및 검증

> 니가 테스트 요청한 캡쳐파일 완료했으니 내용 분석해서 문제없는지 확인해봐
>
> User context
>
> Attachments:
> - 작은 Move Linear Absolute.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/작은 Move Linear Absolute.pcapng`
> - 작은 Move Linear Absolute.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/작은 Move Linear Absolute.txt`
> - Stop_InPosition 확인.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Stop_InPosition 확인.pcapng`
> - Disable.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Disable.pcapng`
> - Disable.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Disable.txt`
> - Power Off_Group.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Power Off_Group.pcapng`
> - Power Off_Group.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Power Off_Group.txt`
> - Refresh Admin Capabilities.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Refresh Admin Capabilities.pcapng`
> - Refresh Admin Capabilities.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Refresh Admin Capabilities.txt`
> - Refresh Capabilities.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Refresh Capabilities.pcapng`
> - Refresh Capabilities.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Refresh Capabilities.txt`
> - Load Axis.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Load Axis.pcapng`
> - Load Axis.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Load Axis.txt`
> - Connect.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Connect.pcapng`
> - Connect.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Connect.txt`
> - Read Status.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Read Status.pcapng`
> - Read Status.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Read Status.txt`
> - Read Position.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Read Position.pcapng`
> - Read Position.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Read Position.txt`
> - Load Group.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Load Group.pcapng`
> - Load Group.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Load Group.txt`
> - Get Members.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Get Members.pcapng`
> - Get Members.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Get Members.txt`
> - Read Position_Group.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Read Position_Group.pcapng`
> - Read Position_Group.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Read Position_Group.txt`
> - Read EtherCAT Health.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Read EtherCAT Health.pcapng`
> - Read EtherCAT Health.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Read EtherCAT Health.txt`
> - Load PI Catalog.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Load PI Catalog.pcapng`
> - Load PI Catalog.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Load PI Catalog.txt`
> - Read Selected PI.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Read Selected PI.pcapng`
> - Read Selected PI.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Read Selected PI.txt`
> - Power On.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Power On.pcapng`
> - Power On.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Power On.txt`
> - Status 확인.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Status 확인.pcapng`
> - Status 확인.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Status 확인.txt`
> - 작은 Absolute Move.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/작은 Absolute Move.pcapng`
> - 작은 Absolute Move.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/작은 Absolute Move.txt`
> - Stop.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Stop.pcapng`
> - Stop.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Stop.txt`
> - Power Off.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Power Off.pcapng`
> - Power On_Group.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Power On_Group.pcapng`
> - Power On_Group.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Power On_Group.txt`
> - Read Status_Group.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Read Status_Group.pcapng`
> - Read Status_Group.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Read Status_Group.txt`
> - Power Ready 확인_Group.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Power Ready 확인_Group.pcapng`
> - Power Ready 확인_Group.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Power Ready 확인_Group.txt`
> - Home Check.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Home Check.pcapng`
> - Home Check.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Home Check.txt`
> - Set Identity.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Set Identity.pcapng`
> - Set Identity.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Set Identity.txt`
> - Enable.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Enable.pcapng`
> - Enable.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Enable.txt`
> - Locked Standby 확인.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Locked Standby 확인.pcapng`
> - Locked Standby 확인.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test/Locked Standby 확인.txt`

<details><summary>4 previous messages</summary>

> 완료된 캡처를 실제 TCP payload, 요청/응답 순서, 반환 코드, 축/그룹 상태 변화 기준으로 교차 검증하겠습니다. 먼저 이전 분석 기준과 현재 코드 상태를 확인한 뒤, 캡처를 기능군별로 병렬 분석하고 마지막에 “정상 / 의심 / 추가 확인 필요”로 분리해 결론 내리겠습니다.
>
> 초기 확인 결과, 캡처 27개와 텍스트 로그가 모두 존재합니다. 다만 `Power Off.txt`와 `Stop_InPosition 확인.txt`는 0바이트라서 해당 두 기능은 텍스트 PASS가 아니라 패킷 자체로만 판정해야 합니다. 또한 저장소에는 이미 사용자 작업 변경이 많이 있어, 이번 분석에서는 파일을 수정하지 않고 현재 소스를 판독 기준으로만 사용하겠습니다.
>
> 패킷 레벨에서는 지금까지 모든 port 4000 요청이 정확히 한 개의 응답과 짝을 이루고, 재전송·TCP payload 누락·비정상 RST는 없습니다. 핵심 예외는 축 `작은 Absolute Move`: 명령 수락과 실제 이동 상태는 잡혔지만 캡처가 텍스트의 완료 시각보다 1.765초 먼저 끝나 최종 Standstill 3회가 패킷에 없습니다. 이 항목은 “완료까지 패킷 검증됨”으로 판정할 수 없습니다.
>
> 현재 라이브러리 계약 테스트는 202/202 통과했고 LASAL SourceOnly/full static 계약도 통과했습니다. WPF 자체 빌드는 `dotnet msbuild`로 시도했으나 이 구형 .NET Framework WPF 프로젝트의 XAML 생성 단계를 처리하지 못해 실패했습니다. 실제 Visual Studio MSBuild가 설치되어 있는지 확인한 뒤 동일 프로젝트를 맞는 빌더로 재검증하겠습니다.
</details>

결론부터 말하면, TCP/RPC 구현에는 문제가 없습니다. 다만 28개 중 `작은 Absolute Move` 1건은 캡처를 너무 일찍 종료해 “이동 완료”까지는 패킷으로 입증되지 않았습니다.

## 판정 요약

| 범위 | 확인 결과 | 판정 |
|---|---|---|
| 전체 전송 | 512 request / 512 response, 27개 command ID | 정상 |
| 프레임 | 선언 길이 불일치·잘림·응답 누락 0건 | 정상 |
| 네트워크 | payload 재전송·IP fragment·active RPC RST 0건 | 정상 |
| 응답 | HeaderStatus 0, ErrorId 0, command-error 0 | 정상 |
| 응답시간 | 평균 1.339 ms, P95 2.377 ms, 최대 3.810 ms | 정상 |
| 축/그룹/진단 happy path | 현재 C#/LASAL wire 계약과 일치 | 정상 |
| 축 Absolute Move 완료 | 최종 Standstill이 캡처에 없음 | 재캡처 필요 |

`Connect.pcapng`에 RST가 1개 있지만, 기존 포트 4436 세션이 Close 응답과 FIN/FIN/ACK로 끝난 뒤 발생한 후행 RST입니다. 새 포트 5598 세션의 Init/Callback 등록에는 영향이 없습니다.

### 축 기능

- Load/Status/Position/Power On/Home/Set Identity/Enable/Disable 모두 정상입니다.
- Power On 후 `0x2290400F`: PowerOn/Referenced/Standstill 모두 확인됐습니다.
- Stop은 ACK 후 `0x2290410F`가 3회 연속 나와 안정 정지를 확인했습니다.
- Power Off는 첫 응답부터 PowerOn이 해제됐고, 이후 `0x2290030E` Standstill이 3회 확인됐습니다. [Power Off.txt](<./test/packet_capture/SIGMATEK_API_Analyze/Test/Power Off.txt>)는 0바이트지만 pcap 자체는 PASS입니다.
- Set Identity의 1,320바이트 payload는 X/Y/Z/U reference 1/2/3/4, ratio 1:1, shift 0, Cartesian, Buffered, Execute=1로 현재 [LmcProtocol.cs](./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs:499)와 정확히 일치합니다.
- Enable ACK 성공 후 별도 상태 캡처에서 `0x40060000` Locked Standby가 확인됐습니다. 따라서 Group Enable happy path는 이번 캡처로 실기 확인됐습니다. 단, 상태 확인을 수동으로 35초 뒤 실행했으므로 실제 lock 완료시간을 측정한 것은 아닙니다.

### 반드시 다시 캡처할 항목

[작은 Absolute Move.pcapng](<./test/packet_capture/SIGMATEK_API_Analyze/Test/작은 Absolute Move.pcapng>)에는 다음까지만 있습니다.

- `0x209F` ACK 성공
- 실제 비정지 상태 `0x2090410B` 110회
- 모든 110회에서 `Standstill=False`
- pcap 종료: `17:11:05.010`
- [완료 로그](<./test/packet_capture/SIGMATEK_API_Analyze/Test/작은 Absolute Move.txt>): `17:11:06.776`

즉 캡처가 완료 로그보다 1.766초 먼저 끝났습니다. 이번 파일은 “명령 수락과 실제 이동”은 PASS지만 “최종 완료”는 미증명입니다.

재캡처할 때는 `Move Absolute completion PASS`가 나온 뒤 1~2초 더 유지하고, 가능하면 마지막에 Read Position까지 실행하면 됩니다.

### 그룹 기능

그룹 흐름은 정상적으로 이어졌습니다.

- Power Ready: `0x40050000`
- Move 진행 317회: `0x40040000`
- Move 완료 3회: `0x40060000`
- Stop 후 3회: `0x40060000`
- Power Off 후 3회: `0x40010000`

Move Linear 요청도 정확합니다.

- Target: `[100000,100000,100000,100000,0×12]`
- Velocity/Acceleration/Deceleration: `100000`
- Jerk: `0`
- None / ExactStop / Aborting / Execute
- 첫 InPosition 약 20.072초, 세 번째 안정 InPosition 약 20.20초

다만 최종 `0x2051` 위치 재조회는 없습니다. 따라서 profile 완료는 증명하지만 최종 XYZU 값까지 독립 검증한 것은 아닙니다.

[Stop_InPosition 확인.pcapng](<./test/packet_capture/SIGMATEK_API_Analyze/Test/Stop_InPosition 확인.pcapng>)은 정상이나 이미 InPosition인 상태에서 Stop을 실행했습니다. 이동 중 감속 정지 성능 시험은 아닙니다. 동반 [txt](<./test/packet_capture/SIGMATEK_API_Analyze/Test/Stop_InPosition 확인.txt>)도 0바이트입니다.

Get Members 응답은 AxisCount=9, reference 1~9입니다. 현재 설계대로 Cartesian SetKin/Move는 처음 네 축 X/Y/Z/U만 사용합니다. 오류는 아니지만 “그룹이 물리 4축만 가진다”는 의미는 아닙니다.

### 진단 기능

- Admin capability: FeatureBits `0x7`, physical axes 4, group ref `0x0100`
- Diagnostics capability: `0x213F`, MapRevision `0x957F101E`, Catalog 24개, BootId 10
- Catalog 24개 전체 CRC32가 `0x957F101E`로 정확히 일치
- EtherCAT master와 slave 1~4 모두 Online/OP, AL=0, AxisError=0, invalid cycle=0
- PI 네 값 모두 Int32/Valid/detail 0

PI 네 축은 서로 다른 cycle에서 순차 읽혔습니다. 동일 cycle 비교가 필요하면 Bulk Snapshot을 사용해야 합니다. 또한 PI `actual_position`은 PDO raw counts이고 일반 Read Position은 LASAL application unit이므로 숫자를 직접 비교하면 안 됩니다.

Health의 DS402 raw는 네 축 모두 `0x02B3`입니다. 이 값은 OperationEnabled bit가 꺼져 있고 Warning bit가 켜져 있습니다. 캡처 시점이 Power On 전이므로 OperationEnabled=false는 자연스럽지만, “drive warning도 전혀 없다”까지 증명하려면 Power On 후 Health/StatusWord를 다시 읽어야 합니다.

### 이동량 주의

저장소 설정인 `10000 raw/mm`가 다운로드된 PLC에도 동일하다면 파일명과 달리 이동량은 작지 않습니다.

- 축: `87.1065 → 10.0000 mm`, 약 `77.1065 mm`
- 그룹 Y/Z/U: 각각 약 `-100 → +10 mm`, 축당 약 `110 mm`
- 그룹 합성 경로: 약 `190.5 mm`

의도한 거리라면 문제없지만, “작은 이동”을 의도했다면 입력값 해석을 다시 확인해야 합니다.

현재 소스 검증도 통과했습니다.

- PC 계약 테스트: `202/202 PASS`
- LASAL SourceOnly/full static contract: PASS
- WPF Debug Visual Studio MSBuild: PASS
- 분석 과정에서 파일은 수정하지 않았습니다.

필수 보완은 축 Absolute Move 재캡처 1건입니다. 안전 범위를 더 강하게 닫으려면 이동 중 Stop, 축/그룹 최종 Position readback, Power On 후 DS402 Health도 추가하면 됩니다.

> 다시 테스트 할 내용이랑 수정된 내용 테스트할꺼 정리해서 알려줘

<details><summary>4 previous messages</summary>

> 현재 캡처에서 빠진 재시험과, 작업 트리에 수정된 기능 때문에 새로 필요한 시험을 분리해 정리하겠습니다. 우선 현재 diff와 테스트 설계 문서를 다시 대조해서 버튼 순서·캡처 종료 조건·합격 기준까지 실행 가능한 체크리스트로 만들겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 변경분에는 기존 Group/Bulk/Recorder 외에 응답 길이 방어, Negative Wire 도구, D5 SDO 시험도 추가되어 있습니다. 캡처 재시험과 신규 qualification을 섞지 않고, 위험도가 낮은 순서로 배열하겠습니다.
>
> 현재 변경분 기준 PC 자동시험은 기존 문서의 202개가 아니라 219개이며, 방금 Debug에서 219/219 통과했습니다. 이제 실기에서만 증명 가능한 항목만 남겨 목록에 넣고 있습니다.
</details>

결론부터 말하면, 기존 캡처의 기본 API 동작은 정상입니다. 기능상 반드시 다시 캡처할 것은 1건이고, 7월 27일 이후 추가·수정된 Qualification 기능은 별도로 실기 시험해야 합니다.

현재 코드 상태는 방금 재검증했습니다.

- PC 자동시험 Debug/Release: 각각 `219/219 PASS`
- WPF Debug/Release 빌드: PASS
- LASAL SourceOnly/full 정적 계약: PASS
- 문서의 `202/202` 표기는 현재 코드보다 뒤처진 상태입니다.
- PLC 실기·패킷 시험은 아래 항목이 남았습니다.

## 1. 기존 캡처 중 다시 할 것

| 우선순위 | 시험 | 이유와 합격 기준 |
|---|---|---|
| 필수 | Axis `작은 Absolute Move` | 기존 pcap이 완료 로그보다 1.766초 먼저 끝났고, 패킷 안에서는 끝까지 `Standstill=False`였습니다. 현재 위치 `P0`를 읽고 안전거리 확인 후 `P0 ± 10000 raw` 정도로 이동하십시오. `0x209F` 1회, 실제 비정지 관측, `Standstill=True` 3회, 마지막 `0x202E Read Position`이 목표 허용오차 내인 것까지 캡처해야 합니다. PASS 후 2초 더 캡처하십시오. |
| 로그만 보완 | `Power Off` | pcap은 정상이나 TXT가 0바이트입니다. 제출물에 로그가 필요할 때만 재실행합니다. |
| 로그만 보완 | `Stop_InPosition 확인` | pcap은 정상이나 TXT가 0바이트입니다. 기능 재검증은 불필요하고 로그가 필요할 때만 재실행합니다. |
| 범위에 따라 | Axis 이동 중 Stop | 기존 Axis Stop은 이동 완료 후 실행됐습니다. “실제 이동 중 축 Stop”이 승인 범위라면 긴 저속 안전 이동 중 `0x2022`를 보내고 최종 Standstill 3회와 위치를 확인해야 합니다. |
| 권장 | Group Absolute 최종 위치 | 기존 `작은 Move Linear Absolute`는 InPosition 완료까지는 정상이나 마지막 `0x2051` 위치 재조회가 없습니다. 다시 한다면 한 축만 작은 거리로 움직이고 최종 XYZU를 읽으십시오. 기존 시험은 Y/Z/U가 각각 약 110 mm 이동해 이름과 달리 작은 이동이 아니었습니다. |

Group 이동 중 Stop은 7월 23일의 `07_Group_Relative_StopRace_7D22_2085`에서 이미 확인됐습니다. 같은 PLC/WPF 빌드라면 반복할 필요가 없지만, 최신 빌드를 새로 배포했다면 회귀시험으로 다시 해야 합니다.

그 외 Connect, Load Axis/Group, Power, Home, Identity, Enable/Disable, 기본 Group Move, Admin/Diagnostics Capabilities, Health, Catalog, PI Read는 기존 캡처 범위에서 정상입니다.

## 2. 수정된 기능의 신규 실기시험 순서

각 항목은 pcap과 `Save QTEST Log` TXT를 같은 이름으로 저장하십시오.

| 순서 | 실행 버튼/시험 | 핵심 합격 조건 |
|---:|---|---|
| 1 | `Run Enable ACK -> Locked` | PowerOn+Identity, Disabled/Unlocked에서 시작. `0x2047` 정확히 1회 성공 후 `0x2045`로 5초 안에 Locked Standby 3회. 파일 `13_GroupEnable_AcceptedThenLocked_2047_2045.*` |
| 2 | `Run Read-only 0x2045 RPC` | 단독 PLC 세션, 정지·InPosition 상태. Warm-up 100회 + 측정 10,000회 전부 성공, 정상 20-byte frame/12-byte payload, 응답 byte 안정성과 SHA-256 확인. CSV도 저장합니다. |
| 3 | `Run Buffered A -> B` | 기본 A `+10000`, B `+20000 raw`. A 완료 전에 B가 전송되어야 합니다. `0x7D22 Buffer=2` 2회, 최종 `Start+A+B`, 복귀용 `Buffer=1` 1회, 성공 경로의 `0x2085`는 0건. 파일 `14_Group_TrueBuffered_Chaining_7D22.*` |
| 4 | `Run Deterministic Stop-First` | Move가 wire 전에 취소돼 `0x7D22/0x20A4` 0건, `0x2085` 정확히 1건, 최종 InPosition 3회. 파일 `15_Group_StopFirst_Preemption_2085_NoMove.*` |
| 5 | `Run 24-entry Snapshot Soak` | 100회/10 ms, 100/100 성공, 매회 24 Valid, `Partial=False`, SameCycle/InputMapped, invalid/error 0, Release 성공. 파일 `16_Bulk_24Entry_100Snapshot_Soak_7E30_33.*` |
| 6 | `Run Configure/Read/Release Soak` | 100 lifecycle 모두 성공, 종료 후 새 Configure 재사용 성공, 중복 Release는 PC에서 차단되어 추가 wire 0건. 파일 `17_Bulk_100Lifecycle_ReleaseReuse_7E30_33.*` |
| 7 | `Run Single Manual` | 4채널×1000 samples, 16,000 bytes, 두 Download의 byte/SHA-256 동일, Release 성공. 파일 `19_Recorder_SingleManual_Lifecycle_7E40_48.*` |
| 8 | `Run Ring Forced Trigger` | Pre 100/Post 899, `TriggerComplete`, TriggerIndex 100, 총 1000 samples, chunk 누락·중복 0. 파일 `20_Recorder_Ring_ForcedTrigger_7E42.*` |
| 9 | `Run Trigger Lifecycle Soak` | 기본 100회, capacity 32, 100/100 성공, ResourceBusy/Dropped/Overflow 0, 매회 Release. 파일 `22_Recorder_100Cycle_TriggerSoak.*` |
| 10 | `Run Reconnect Exact Adopt` | 실제 RPC close/reopen, 동일 BootId/MapRevision/RecordId/BufferId, 새 OwnerSessionEpoch, Download·Release 성공. `21a_Recorder_Reconnect_Exact_7E49.*` 권장 |
| 11 | `Run Reconnect 0/0 Discovery` | 별도 캡처로 실행. Single-bank에서 0/0 discovery가 기존 Recorder를 찾아 Status/Download/Release까지 성공. `21b_Recorder_Reconnect_00_Discovery_7E49.*` 권장 |
| 12 | `Run D5 Abort -> Recovery` | 축 PowerOff/Standstill 3회 확인. 승인된 존재하지 않는 read-only object 사용. Baseline `0x6061:0` 성공 → Abort `Failed`, `ErrorId=-32000`, raw abort code nonzero, result 없음 → 같은 BootId에서 `0x6061:0` 동일 값 복구. 파일 `23a_SDO_Abort_Recovery_7E50_03.*` |
| 13 | `Run One-Slave-Offline Partial` | 가장 마지막에 실행. Group PowerOff/Disabled 후 정확히 한 slave를 `Online=False`로 변경. 해당 축 6개만 Detail 18/SlaveOffline, 나머지 18개 Valid. 같은 slave를 OP 복구한 뒤 24 Valid와 Release 확인. 파일 `18_Bulk_Partial_OneSlaveOffline_7E32.*` |
| 14 | Negative Wire 5종 | `malformed-payload`, `stale-map`, `stale-boot`, `stale-config`, `duplicate-bulk-release`. 모든 정상 시험 종료 후 단독 세션에서 마지막으로 실행합니다. Motion/Write는 보내지 않습니다. |

Negative Wire의 정확한 실행 조건은 [NEGATIVE_WIRE_TOOL_2026-07-27.md](./LMC_Library/LMC_API_Delivery/docs/NEGATIVE_WIRE_TOOL_2026-07-27.md:19)에 정리되어 있습니다.

## 3. Cleanup 수정 검증

Happy-path 완료 후 아래 취소 시험도 별도 캡처하는 것이 좋습니다.

- Bulk soak 중 `Cancel Test`: 결과 `ABORTED`, Release 성공, 직후 짧은 재실행 성공.
- Recorder soak 중 `Cancel Test`: 진행 중 RPC 완료 후 다음 RPC 전에 취소, 필요 시 Stop, Release 후 새 Configure 성공.
- Buffered 시험 중 Cancel: `ABORTED`와 fallback Group Stop, 최종 InPosition 3회 확인.
- Transport 10,000회 중 Cancel: `ABORTED` CSV가 생성되고 연결이 계속 사용 가능해야 합니다.

Recorder Fault/quarantine, 강제 Release 실패, D5 timeout/queued-cancel/orphan/contention은 재현용 PLC hook이나 승인된 절차 없이 임의로 시험하면 안 됩니다.

공통으로 캡처는 버튼 클릭 전에 시작하고, `END|verdict=PASS/ABORTED`, cleanup 및 재사용 확인 후 2초 뒤 종료하십시오. 전체 실행 기준은 [WPF README](./LMC_Library/LasalApiWpfTestApp/README.md:72)와 [Qualification 설계](./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md:239)에 있습니다.

> 이제 테스트 어떤거 진행하면되냐?

<details><summary>4 previous messages</summary>

> 지금 바로 진행할 순서를 최신 작업 트리 기준으로 다시 확인해서, 필수 시험만 짧게 번호로 정리하겠습니다.
>
> 어제보다 작업 트리가 크게 바뀌었습니다. Topology/IO, SDO Write 검증, D5 timeout·contention, Recorder Double-bank까지 추가돼 이전 목록만 그대로 실행하면 누락됩니다. 최신 추가분을 포함해 실제 실행 순서를 다시 좁히겠습니다.
