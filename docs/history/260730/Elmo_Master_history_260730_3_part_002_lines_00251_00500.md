>
> 현재 기준으로는 실기 전에 빌드/다운로드 확인이 먼저입니다. 최신 소스가 마지막 LASAL IDE 빌드 이후 변경됐기 때문에, 바로 패킷 캡처부터 시작하면 어떤 버전을 시험했는지 증명할 수 없습니다.
>
> 중요한 변경점이 확인됐습니다. 현재 최신 WPF는 빌드가 깨져 있어서 실기시험을 시작하면 안 됩니다. `MainWindow.Qualification.Sdo.cs`가 존재하지 않는 `D5SdoTimeoutRecoveryScope` 형식을 참조해 CS0246으로 실패합니다. 먼저 이 빌드 오류를 고친 뒤 시험해야 합니다.
</details>

지금은 실기시험을 시작하면 안 됩니다. 최신 WPF 빌드가 실패합니다.

- PC 자동시험: `423/423 PASS`
- LASAL full 정적 계약: PASS
- WPF 빌드: FAIL
- 원인: [D5SdoTimeoutQualificationOrchestrator.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoTimeoutQualificationOrchestrator.cs:52)는 존재하지만 [LasalApiWpfTestApp.csproj](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj:120)에 Compile 등록이 없습니다.
- 결과: [MainWindow.Qualification.Sdo.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs:2052)에서 `CS0246` 발생

## 빌드 수정 후 시험 순서

1. 배포 준비

   - WPF Debug/Release 재빌드
   - LASAL IDE `Reload Class → Rebuild/Link`
   - 변경 클래스 `Find in Implementation`
   - 이후 `Lasal2.log`의 신규 `CInvalidArgException=0`
   - PLC에 최신 프로그램 다운로드

2. 기본 상태 확인

   - Connect
   - Refresh Capabilities
   - Read EtherCAT Health
   - Load PI Catalog
   - Topology bit 14 ON, BootId nonzero 확인
   - bit 6 Double-bank, bit 9 SDO Write, bit 15~17 NodeHealth/DI/DO는 현재 OFF가 정상

3. 신규 Topology 시험

   - `topology-io-qualify --scope topology-inventory`
   - 예상 wire: `0x7E11` 1회 + `0x7E12` 7회
   - PASS: Revision `0x15867EEC`, 7 entries, 5 slaves+2 slots
   - pcap과 TXT report를 같이 저장

4. 기존 캡처 보완

   - Axis Absolute Move 재시험
   - 이동 완료, Standstill 3회, 최종 Read Position까지 캡처
   - `Power Off.txt`, `Stop_InPosition 확인.txt`는 로그가 필요할 때만 재생성

5. Group 신규 시험

   - `Run Enable ACK -> Locked`
   - `Run Read-only 0x2045 RPC` 10,000회
   - `Run Buffered A -> B`
   - `Run Deterministic Stop-First`
   - 마지막 Group Stop → Disable → Power Off 확인

6. Bulk/Recorder

   - 24-entry Snapshot 100회
   - Configure/Read/Release 100회
   - Recorder Single Manual
   - Ring Forced Trigger
   - Trigger Lifecycle 100회
   - Reconnect Exact
   - Reconnect 0/0 Discovery
   - One-Slave-Offline Partial은 가장 마지막

7. D5 Read-only 신규 시험

   - Abort → Recovery
   - Contention → `ResourceBusy` → Recovery
   - Timeout → late-callback drain → Recovery
   - 모두 축 PowerOff·Standstill 상태에서 실행

8. 마지막 시험

   - Bulk/Recorder 실행 중 Cancel cleanup
   - 진단 작업 중 Group Stop/PowerOff 우선순위
   - Negative Wire 5종

현재 실행하면 안 되는 것은 SDO Write, Digital Output Write `0x7E23`, Node Health `0x7E13`, Digital I/O Read `0x7E22`, Recorder Double-bank입니다. 아직 capability/gate 또는 live 경로가 열리지 않았습니다.

우선 csproj 누락부터 고쳐야 합니다. 원하면 제가 바로 수정하고 Debug/Release 빌드까지 다시 확인하겠습니다.

> wireshark로 캡쳐할때 조건 어떻게 걸어

기존 캡처 기준 PLC는 `10.10.150.1`, PC는 `10.10.150.13`, API TCP 포트는 `4000`입니다.

Wireshark에서 PLC가 연결된 Ethernet 어댑터를 선택한 뒤 아래처럼 설정하세요.

### 권장 Capture Filter

가장 안전한 조건입니다.

```text
host 10.10.150.1
```

API와 UDP callback만 줄여서 받으려면:

```text
host 10.10.150.1 and (tcp port 4000 or udp port 5000)
```

Callback 포트를 WPF에서 5000이 아닌 값으로 설정했다면 바꿔야 합니다.

### 분석할 때 Display Filter

API 요청·응답 전체:

```text
ip.addr == 10.10.150.1 && tcp.port == 4000 && tcp.len > 0
```

PC가 보낸 요청만:

```text
ip.src == 10.10.150.13 && ip.dst == 10.10.150.1 && tcp.dstport == 4000 && tcp.len > 0
```

PLC 응답만:

```text
ip.src == 10.10.150.1 && tcp.srcport == 4000 && tcp.len > 0
```

API와 callback 같이 보기:

```text
tcp.port == 4000 || udp.port == 5000
```

TCP 이상 여부:

```text
tcp.analysis.retransmission ||
tcp.analysis.lost_segment ||
tcp.analysis.out_of_order ||
tcp.flags.reset == 1
```

특정 요청 opcode만 볼 때:

```text
tcp.dstport == 4000 && tcp.payload[0:2] == 45:20
```

대표 opcode의 little-endian 값은 다음과 같습니다.

- Group Status `0x2045`: `45:20`
- Group Enable `0x2047`: `47:20`
- Group Stop `0x2085`: `85:20`
- Group Relative Move `0x7D22`: `22:7d`
- Topology Info `0x7E11`: `11:7e`
- Topology Chunk `0x7E12`: `12:7e`

단, opcode 조건은 Display Filter로만 사용하세요. Capture Filter에 넣으면 TCP 분할 등의 이유로 필요한 응답을 놓칠 수 있습니다.

캡처는 버튼 클릭 전에 시작하고, 화면의 최종 `PASS/FAIL/ABORTED`, cleanup 및 최종 상태 확인 후 2초 더 유지한 다음 종료하세요. 각 시험은 pcap과 QTEST TXT를 같은 이름으로 따로 저장하는 것이 좋습니다.

> 이거 테스트 진행한거 분석해주고 지금 구현된 내용 기준으로 멀 더 테스트 하면 되는지 알려줘
>
> User context
>
> Attachments:
> - Load PI Catalog.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test2/Load PI Catalog.pcapng`
> - Load PI Catalog.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test2/Load PI Catalog.txt`
> - Read EtherCAT Health.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test2/Read EtherCAT Health.pcapng`
> - Read EtherCAT Health.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test2/Read EtherCAT Health.txt`
> - Refresh Capabilities.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test2/Refresh Capabilities.pcapng`
> - Refresh Capabilities.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test2/Refresh Capabilities.txt`
> - Topology.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test2/Topology.pcapng`
> - Topology.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test2/Topology.txt`
> - Axis Absolute Move 재시험.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test2/Axis Absolute Move 재시험.pcapng`
> - Axis Absolute Move 재시험.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test2/Axis Absolute Move 재시험.txt`
> - Connect.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test2/Connect.pcapng`
> - Connect.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test2/Connect.txt`

<details><summary>8 previous messages</summary>

> 이번에는 새 캡처 6세트를 패킷·TXT·현재 소스 계약과 다시 대조하겠습니다. Topology/Diagnostics, Axis Move 완료 증거, Connect를 병렬로 분석하고, 최신 구현 상태까지 확인해 “통과/재시험/추가시험”으로 나누겠습니다. 파일은 수정하지 않겠습니다.
>
> 새 캡처는 이전 결함을 보완한 흔적이 TXT에 있습니다. 이제 pcap에서 실제 완료 상태·최종 위치와 Topology 8개 요청이 그대로 있는지 바이트 단위로 확인 중입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 축 이동 재시험은 패킷상 목표 50,000 raw 도달과 3회 연속 Standstill까지 확인됐습니다. Topology도 7개 엔트리(마스터·4축·DI·DO)를 모두 반환합니다. 남은 진단 필드와 현재 구현의 미실행 항목을 소스 기준으로 좁히겠습니다.
>
> 이번 Test2에서 새로 닫힌 항목은 두 가지입니다. 이전에 부족했던 단축 Absolute Move의 “실제 완료+최종 위치”와, bit 14 기반 정적 EtherCAT Topology의 실선 패킷 증거입니다. 단, CREVIS 실시간 Online/DI/DO는 capability bit 15~17이 꺼져 있어 이번 PASS에 포함되지 않습니다.
>
> 현재 체크아웃도 정적 확인 중입니다. 방금 공용 C# API Release 테스트는 문서의 이전 숫자보다 늘어난 `669/669 PASS`였습니다. 이 결과는 PC 측 계약 검증이고, 지금 보고 있는 PLC 실기 캡처 PASS와는 별도로 구분하겠습니다.
>
> WPF Debug 재빌드는 코드 오류가 아니라 현재 실행 중인 Visual Studio/테스트 앱이 Debug DLL·PDB를 잡고 있어 실패했습니다. 실행 중인 앱은 건드리지 않겠습니다. 잠금과 분리된 Release 출력으로 다시 확인합니다.
>
> 최종 분류는 명확합니다. 이번 6개는 wire 기준 모두 PASS지만, Health에서 4축 모두 DS402 Warning(bit 7)이 보이며 운전 가능 완료 상태는 아닙니다. 또 현재 작업본은 대량 미커밋 상태라, 다음 실기시험 전 빌드/PLC 다운로드 기준점을 먼저 고정해야 결과가 재현됩니다.
</details>

## 결론

Test2의 6개 캡처는 모두 wire 기준 PASS입니다. 특히 이전에 증거가 부족했던 단축 Absolute Move와 정적 EtherCAT Topology가 이번에 확인됐습니다.

| 시험 | 판정 | 확인 내용 |
|---|---|---|
| Connect | PASS | 기존 세션 Close 후 새 세션 12개 요청/응답 정상. 자동 Topology 7개 로드 |
| Refresh Capabilities | PASS | `0x613F`, Map `0x957F101E`, BootId `17` |
| Read EtherCAT Health | 조건부 PASS | Master OP, invalid cycle 0, 4축 Online/OP, AL/AxisError 0 |
| Load PI Catalog | PASS | 24개 엔트리, CRC `0x957F101E` 일치 |
| Topology | PASS | 7개 엔트리, CRC `0x15867EEC` 일치 |
| Axis Absolute Move 재시험 | PASS | `9995 → 50000 raw`, 비정지 확인 후 Standstill 3회, 최종 위치 3회 모두 `50000` |

상세 바이트 분석 결과는 [Test2 캡처 감사 문서](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_ETHERCAT_TEST2_CAPTURE_AUDIT_2026-07-28.md:7)와도 일치합니다.

주의할 점은 다음입니다.

- Health에서 Axis 1은 DS402 `0x02B3`(Switched On), Axis 2~4는 `0x02D0`(Switch On Disabled)입니다.
- 네 축 모두 Statusword bit 7 `Warning`이 설정돼 있습니다. AL Status와 AxisError는 0이고 Fault bit도 없으므로 통신 오류는 아니지만, 다음 motion 시험 전 Drive Status에서 경고 원인은 확인해야 합니다.
- Topology PASS는 설정된 구성만 증명합니다. 실제 CREVIS Online/AL, DI 접점, DO 출력은 확인되지 않았습니다.
- Connect의 기존 세션 종료 뒤 PLC RST 1개가 있지만 정상 FIN/ACK 종료 이후라 새 연결에는 영향이 없습니다.
- Catalog는 목록만 읽었습니다. Test2에는 실제 `Read Selected PI` 값이 없습니다.

현재 소스도 다시 확인했습니다.

- PC API Release: `669/669 PASS`
- WPF Release build: PASS
- WPF Release smoke: `66/66 PASS`
- LASAL SourceOnly/full static contract: PASS
- Debug WPF 재빌드는 실행 중인 Visual Studio/WPF가 DLL·PDB를 잡고 있어 파일 복사만 실패했습니다.
- LASAL IDE의 현재 소스 Rebuild/Link와 PLC download를 이번에 확인한 것은 아닙니다.
- 작업 트리는 tracked 변경 106개, untracked entry 82개입니다. 다음 실기시험 전 현재 버전을 commit 또는 hash 목록으로 고정해야 합니다.

## 이제 진행할 시험 순서

1. 빌드 기준 고정

   - 현재 소스를 commit 또는 별도 hash manifest로 고정
   - LASAL IDE `Reload Class → Rebuild/Link`
   - 변경 클래스 `Find in Implementation`
   - `%TEMP%\Lasal2.log` 신규 `CInvalidArgException=0`
   - PLC cold download
   - 이후 BootId/MapRevision을 다시 기록

2. 안전한 Read-only 시험

   - `Load PI Catalog → Read Selected PI`
   - Axis 1~4 `Read Drive Status`로 DS402 Warning 확인
   - Group이 stable InPosition인 상태에서 `Run Read-only 0x2045 RPC`
     - Warm-up 100
     - Measured 10,000
     - CSV와 pcap 저장
