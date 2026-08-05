# 다음 테스트 항목 확인

> 이제 내가 어떤걸 이어서 테스트해야 되는지 알려줘

<details><summary>3 previous messages</summary>

> 현재 소스·최근 캡처·Git 상태를 다시 확인해서, 이미 끝난 항목과 다음에 실제로 해야 할 테스트를 분리하겠습니다. 우선순위와 실행 방법까지 바로 쓸 수 있게 정리하겠습니다.
>
> 지금 작업 트리는 Test2 당시보다 크게 진행됐습니다. 특히 7월 31일 단일축 실기 검증 절차가 새로 생겨 있어, 과거 결론만 반복하지 않고 그 절차와 현재 구현을 기준으로 다음 테스트를 좁히고 있습니다.
>
> 확인 결과, Test2 이후 새 `pcapng` 증거는 없습니다. 따라서 다음 단계는 “또 다른 기능 버튼”이 아니라 현재 PLC가 최신 빌드인지 고정하고, read-only 상태·경고를 확인한 뒤 새 단일축 통합 시험을 한 번 수행하는 것입니다.
</details>

결론: 지금 바로 할 것은 **축 1~4의 read-only DS402 진단 캡처**입니다. 이것이 정상일 때만 **Axis 1 단일축 통합 qualification**을 실행하세요. `0x209F` Absolute Move 재시험부터 할 단계는 아닙니다.

### 1. 시험 기준부터 고정

현재 상태는 `main@6537bcf`, tracked 변경 97개, untracked 109개입니다. 현재 fingerprint는 다음입니다.

```text
6537bcf1bf0fdb338a934b63891fc9ee110aecad/a543c91704fbe07cf2acc500a7ee6e7192595824/6c1c7df21a5d78658877a08e925ed41664757b63
```

- canonical LASAL 프로젝트 Rebuild/Link
- 현재 PLC에 cold download
- BootId, MapRevision, WPF EXE/DLL SHA-256 기록
- 시험 로그는 저장소 밖 `C:\work\Elmo\evidence\...`에 저장

현재 PLC 다운로드 증거가 없다는 것이 가장 큰 미검증 항목입니다. [현재 미검증 범위](</C:/work/Elmo/Elmo_Master/docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md:181>)

### 2. 지금 실행할 read-only 시험

캡처를 먼저 시작합니다.

```text
Capture:
host 10.10.150.1 and (tcp port 4000 or udp port 5000)

Display:
ip.addr == 10.10.150.1 && tcp.port == 4000 && tcp.len > 0

TCP 이상 확인:
tcp.analysis.retransmission || tcp.analysis.lost_segment ||
tcp.analysis.out_of_order || tcp.flags.reset == 1
```

WPF에서 다음 순서로 실행하세요.

1. Connect
2. Refresh Capabilities
3. Load PI Catalog
4. Topology
5. Read EtherCAT Health
6. Admin 탭 → 축 1~4 각각:
   - `1 Get Drive Operation Mode`
   - `2 Read Drive Status`
   - `3 Get Drive Error Code`

확인값:

- BootId: `0`이 아니며 시험 중 불변
- MapRevision: `0x957F101E`
- Capabilities: 현재 기준 `0x0000613F`
- Topology: 7 entries, revision `0x15867EEC`
- 각 축: Online, EtherCAT OP, AL=0, `AxisError=0`
- `0x6041`: Fault bit 3=0, Warning bit 7 상태와 원인, Internal Limit bit 11
- `0x603F=0`
- `0x6061`: 현재 설정한 운전 모드와 일치

`0x2028` 응답의 `StatusWord`는 현재 reserved `0`이므로 DS402 판정에 사용하면 안 됩니다. `AxisError`, 실제 `0x6041`, `0x603F`를 따로 봐야 합니다. [DS402 판정 경계](</C:/work/Elmo/Elmo_Master/docs/architecture/DRIVE_DS402_FAULT_ERROR_DIAGNOSTICS_2026-07-29.md:50>)

다음 중 하나라도 나오면 모션은 중단하세요.

- `AxisError != 0`
- `0x6041 Fault=1`
- `0x603F != 0`
- Warning 원인이 확인되지 않음
- BootId/MapRevision 변경

### 3. 통과하면 Axis 1 단일축 통합 시험

`Single Axis live qualification`에서 Axis 1만 선택합니다. E-stop/STO, limit, 방향, raw UNIT, reference, 이동 공간, 단일 motion owner를 확인한 값만 사용하세요. UI 기본 `delta=10000`이 실제 장비에서 안전한 거리라고 자동으로 간주하면 안 됩니다.

예상 wire 순서:

1. Power On `0x2023(true)` 정확히 1회
2. `0x2028` PowerOn 안정 상태 3회
3. 시작 위치 `0x202E`
4. Relative Move `0x20A0` 정확히 1회
5. non-Standstill 최소 1회 → Standstill 3회
6. 최종 위치 `0x202E` 3회, target/tolerance 일치
7. Stop `0x2022` 정확히 1회 → Standstill 3회
8. Power Off `0x2023(false)` 정확히 1회 → PowerOff+Standstill 3회
9. 다시 `0x6041`과 `0x603F` 확인
10. PASS 뒤에도 캡처를 최소 2초 유지하고 `pcapng + Axis QTEST + PLC log` 저장

이 Stop은 이동 완료 후 전송되므로 **주행 중 급정지 시험은 아닙니다**. [단일축 실행 계약](</C:/work/Elmo/Elmo_Master/docs/architecture/SINGLE_AXIS_LIVE_QUALIFICATION_2026-07-31.md:18>)과 [필수 증거](</C:/work/Elmo/Elmo_Master/docs/architecture/SINGLE_AXIS_LIVE_QUALIFICATION_2026-07-31.md:97>)를 기준으로 판정하면 됩니다.

Axis 1 PASS 후에는 `Cancel / Safe Cleanup` → 외부 Stop 개입 → 외부 Power Off 개입 → 축 2~4 순으로 진행하세요. SDO Write, PI Write, Recorder Double, `0x7E23`은 아직 실행하지 마세요.

파일이나 PLC에는 아무 변경도 하지 않았습니다.

> 1차 테스트 끝났다. 단일축은 니가 분석하는동안 진행할께
>
> User context
>
> Attachments:
> - Axis1_Read Drive Status.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis1_Read Drive Status.txt`
> - Axis1_Get Drive Error Code.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis1_Get Drive Error Code.pcapng`
> - Axis1_Get Drive Error Code.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis1_Get Drive Error Code.txt`
> - Axis2_Get Drive Operation Mode.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis2_Get Drive Operation Mode.pcapng`
> - Axis2_Get Drive Operation Mode.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis2_Get Drive Operation Mode.txt`
> - Axis2_Read Drive Status.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis2_Read Drive Status.pcapng`
> - Axis2_Read Drive Status.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis2_Read Drive Status.txt`
> - Axis2_Get Drive Error Code.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis2_Get Drive Error Code.pcapng`
> - Axis2_Get Drive Error Code.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis2_Get Drive Error Code.txt`
> - Axis3_Get Drive Operation Mode.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis3_Get Drive Operation Mode.pcapng`
> - Axis3_Get Drive Operation Mode.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis3_Get Drive Operation Mode.txt`
> - Axis3_Read Drive Status.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis3_Read Drive Status.pcapng`
> - Axis3_Read Drive Status.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis3_Read Drive Status.txt`
> - Axis3_Get Drive Error Code.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis3_Get Drive Error Code.pcapng`
> - Axis3_Get Drive Error Code.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis3_Get Drive Error Code.txt`
> - Axis4_Get Drive Operation Mode.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis4_Get Drive Operation Mode.pcapng`
> - Axis4_Get Drive Operation Mode.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis4_Get Drive Operation Mode.txt`
> - Axis4_Read Drive Status.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis4_Read Drive Status.pcapng`
> - Axis4_Read Drive Status.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis4_Read Drive Status.txt`
> - Axis4_Get Drive Error Code.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis4_Get Drive Error Code.pcapng`
> - Axis4_Get Drive Error Code.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis4_Get Drive Error Code.txt`
> - Connect.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Connect.pcapng`
> - Connect.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Connect.txt`
> - Refresh Capabilities.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Refresh Capabilities.pcapng`
> - Refresh Capabilities.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Refresh Capabilities.txt`
> - Load PI Catalog.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Load PI Catalog.pcapng`
> - Load PI Catalog.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Load PI Catalog.txt`
> - Topology.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Topology.pcapng`
> - Topology.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Topology.txt`
> - Read EtherCAT Health.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Read EtherCAT Health.pcapng`
> - Read EtherCAT Health.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Read EtherCAT Health.txt`
> - Axis1_Get Drive Operation Mode.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis1_Get Drive Operation Mode.pcapng`
> - Axis1_Get Drive Operation Mode.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis1_Get Drive Operation Mode.txt`
> - Axis1_Read Drive Status.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test3/Axis1_Read Drive Status.pcapng`

<details><summary>6 previous messages</summary>

> 먼저 `Fault`, `Warning`, `0x603F`, 운전 모드부터 즉시 확인하겠습니다. 모션 중단 조건이 보이면 전체 분석을 기다리지 않고 바로 알려드리고, 그다음 pcap payload·명령 순서·2초 보존까지 전부 대조하겠습니다.
>
> 잠깐, **단일축 모션은 일단 중단하세요.** pcap에서 축 1~4 모두 실제 DS402 `0x6041 = 0x02B3`가 다시 확인됐고, 이는 `Fault=0`이지만 **Warning bit 7=1**입니다. `0x603F`는 현재 0으로 보이지만 Warning 원인은 아직 규명되지 않았습니다. 모터를 Power Off/안전 상태로 두고, 제가 네 축의 mode/error와 패킷 전체를 끝까지 확정하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 지금까지 확인된 통신·축 매핑·읽기 결과는 정상입니다. 다만 정식 합격을 막는 항목은 두 가지입니다: 전 축 공통 DS402 Warning과 일부 캡처의 PASS 후 2초 유지 부족입니다. 정확한 재시험 순서를 정리하고 있습니다.
>
> 추가로 더 큰 선결조건이 확인됐습니다. Test3 PLC는 `CapabilityBits=0x0000613F`를 반환했지만 현재 작업 소스는 Axis1 SDO Write gate가 켜져 있어 `0x0000633F`를 반환해야 합니다. 즉, 지금 연결된 PLC는 현재 LASAL 소스의 빌드/다운로드 상태가 아닙니다. 이 상태에서는 단일축 runner를 진행하지 마세요.
>
> 방금 생성된 Test4 단일축 결과 파일도 확인했습니다. 이미 실행이 끝난 것으로 보여, 현재 안전 상태와 실제 명령 순서를 함께 확인한 뒤 최종 판정을 드리겠습니다.
</details>

결론: **축 2~4 모션 시험은 아직 진행하지 마세요.**
Axis1 단일축 시험은 동작 자체는 PASS했고 마지막 `PowerOff + Standstill`도 확인됐지만, 현재 소스 기준의 정식 qualification으로는 인정할 수 없습니다.

### 이번 캡처 판정

| 구분 | 판정 | 근거 |
|---|---|---|
| Test3 통신/API | PASS | 요청·응답·ticket 완료, 축 매핑, EtherCAT OP 정상 |
| 드라이브 모드 | PASS | 전 축 `0x6061=8` CSP |
| Fault/Error | PASS | 전 축 Fault=0, `0x603F=0`, AxisError=0 |
| DS402 Warning | 미해결 | 전 축 `0x6041=0x02B3`, Warning bit 7=1 |
| 배포 소스 일치 | FAIL | PLC `0x613F`, 현재 소스 기대값 `0x633F` |
| Test3 캡처 유지 | 일부 FAIL | 17개 중 6개가 PASS 후 2초 미달 |

현재 작업 소스는 Axis1 SDO Write bit가 활성화되어 있어 다운로드됐다면 `CapabilityBits=0x0000633F`가 나와야 합니다. 실제 Test3/Test4 PLC는 계속 `0x0000613F`를 반환했습니다. 즉 [현재 LASAL 소스](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1336)가 PLC에 반영된 상태가 아닙니다.

이미 실행된 Test4 Axis1 결과는 다음과 같습니다.

- 시작 `29996` → 목표 `39996` → 최종 `39996`
- Power On 1회, Move Relative 1회, Stop 1회, Power Off 1회
- Move 자동 재전송 0회
- 실제 이동 17회 관측 후 Standstill 3회
- 최종 `PowerOff + Standstill`, AxisError=0
- TCP 재전송/RST 없음, capture drop=0
- PASS 후 캡처 유지 `+7.396초`
- 단, Test4에는 `0x6041/0x603F` 읽기가 없어 이동 중 Warning 해제 여부는 모릅니다.

[Test4 결과 로그](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test4/20260731_141635_SingleAxisPowerMoveStopPowerOff.txt:16)

### 지금부터 테스트할 순서

1. Axis1은 Power Off 상태로 유지하고 축 2~4 모션은 중단합니다.

2. 현재 변경 소스의 기준점을 저장한 뒤 LASAL `Rebuild → Link → PLC download`를 수행합니다.

3. 재접속 후 `Refresh Capabilities`를 실행합니다.

   합격 조건:

   - `DiagnosticsBuild=1`
   - `CapabilityBits=0x0000633F`
   - `BootId != 0`이며 새 다운로드 값을 기록
   - `MapRevision=0x957F101E`
   - Topology revision `0x15867EEC`

   여전히 `0x613F`이면 다운로드가 반영되지 않은 것이므로 중단합니다.

4. Axis1에서 **이동 없는 Power On 진단**을 진행합니다.

   - 캡처 시작
   - `_LMCAxis1` Load
   - Single Axis의 일반 `Power On` 버튼 1회
   - PowerOn 3/3 확인
   - Diagnostics에서 `Read EtherCAT Health`
   - Physical drive reads에서 Axis1 선택
   - `Get Drive Operation Mode`
   - `Read Drive Status`
   - `Get Drive Error Code`
   - 일반 `Power Off` 버튼 1회
   - PowerOff+Standstill 3/3 확인
   - 마지막 결과 후 2초 이상 캡처 유지

   Power On 상태 합격 조건:

   - `0x6041` OperationEnabled bit 2=1
   - Fault bit 3=0
   - **Warning bit 7=0**
   - InternalLimit bit 11=0
   - `0x603F=0`
   - AxisError=0
   - `0x6061=8`
   - EtherCAT OP, AL Status=0

5. Warning bit가 계속 1이면 모션하지 마세요. `0x603F=0`은 Warning이 없다는 증거가 아닙니다. [API 설명](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API/API_USER_MANUAL_KO.md:755)

   이때는 Elmo 진단 화면의 활성 Warning을 확인하고, 필요하면 읽기 전용으로 `0x1002:0 / UInt32 / 4 bytes` 상태 레지스터를 조회합니다. 임의 SDO Write나 Reset으로 없애면 안 됩니다.

6. Warning이 해제되고 새 PLC 소스 일치까지 확인된 후에만 Axis1 qualification을 다시 실행합니다. 그 다음 순서가 Axis2→3→4입니다.

참고로 Test3의 2초 유지 미달 파일은 Axis2 Mode/Status, Axis3 Mode, Axis4 Mode/Status/Error입니다. 하지만 현재 PLC 소스가 다르므로 이 6개만 다시 찍지 말고, 새 다운로드 후 Test3 전체를 다시 캡처하는 게 맞습니다.

Wireshark 기본 필터는 다음이면 충분합니다.

```text
tcp.port == 4000 && tcp.len > 0
```

또한 현재 runner는 이동 전 LASAL `0x2028`만 검사하고 DS402 `0x6041/0x603F`를 자동 안전 게이트로 검사하지 않습니다. 따라서 위 수동 진단을 생략하면 안 됩니다. [runner 검사 코드](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs:989)

> 개별축도 테스트 햇어. 이제 어떤거 테스트하냐?
