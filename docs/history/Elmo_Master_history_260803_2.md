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
>
> User context
>
> Attachments:
> - 20260731_141635_SingleAxisPowerMoveStopPowerOff.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/20260731_141635_SingleAxisPowerMoveStopPowerOff.txt`
> - 20260731_141635_SingleAxisPowerMoveStopPowerOff.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/20260731_141635_SingleAxisPowerMoveStopPowerOff.pcapng`

<details><summary>2 previous messages</summary>

> 개별축 Test4는 이미 정상 종료까지 확인됐습니다. 지금은 바로 그룹 모션으로 넘어갈 단계인지, 먼저 막아야 할 선결조건이 남았는지 현재 테스트 계획과 실제 배포 상태를 다시 맞춰서 다음 클릭 순서를 정리하겠습니다.
>
> 현재 Test4는 `Power On → 상대이동 → 정지 확인 → Stop → Power Off`까지는 확실히 PASS입니다. 다만 다음 시험을 그룹으로 바로 넘기기 전에, Test4가 검사하지 않은 드라이브 Warning과 PLC 소스 불일치를 한 번만 해소해야 합니다. 그 뒤의 정확한 그룹 버튼 순서도 함께 확인 중입니다.
</details>

Test4의 Axis1 결과는 좋습니다. 위치 `29996 → 39996`, Move 재전송 0회, 최종 `PowerOff+Standstill`, 캡처 유지 7.396초까지 PASS입니다. 다만 첨부 증거는 `_LMCAxis1` 하나뿐입니다.

다음 정식 순서는 `Axis2 → Axis3 → Axis4 → Group`입니다. 단, 바로 Axis2를 움직이면 안 됩니다.

### 먼저 할 것

1. 현재 LASAL 소스를 `Rebuild → Link → PLC download → Reconnect`합니다.
2. `Refresh Capabilities`에서 확인합니다.

```text
CapabilityBits = 0x0000633F
BootId         = 새 nonzero 값
MapRevision    = 0x957F101E
```

현재 PLC는 계속 `0x613F / BootId 0x0C`라서 [현재 소스](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1337)가 배포되지 않은 상태입니다. `0x613F`이면 추가 모션을 중단하세요.

3. Axis1에서 이동 없는 Power On 진단을 한 번 합니다.

   - `_LMCAxis1` Load
   - 일반 `Power On`
   - Diagnostics → Axis1
   - `Get Drive Operation Mode`
   - `Read Drive Status`
   - `Get Drive Error Code`
   - `Read EtherCAT Health`
   - 일반 `Power Off`

합격 조건:

```text
0x6061 = 8
0x6041 OperationEnabled bit 2 = 1
0x6041 Fault bit 3           = 0
0x6041 Warning bit 7         = 0
0x6041 InternalLimit bit 11  = 0
0x603F                        = 0
AxisError                     = 0
EtherCAT                      = OP, AL=0
```

Test4에는 `0x6041/0x603F/0x6061` 읽기가 전혀 없어서, 이동 중 Warning이 해제됐는지는 아직 모릅니다. 로그의 `statusWord=0`은 DS402 값이 아닙니다.

### 위 조건 통과 후

현재 빌드 기준으로 Axis1 qualification을 다시 한 번 실행한 다음, 각 축마다 별도 캡처로 반복하세요.

1. `_LMCAxis2` Load → 안전한 delta 확인 → `Run LIVE Axis Qualification`
2. `_LMCAxis3` 동일
3. `_LMCAxis4` 동일
4. 각 PASS 후 캡처 2초 유지 및 QTEST 저장

각 축 합격 조건은 Test4와 같습니다.

- Power On 1회
- Move 1회, replay 0회
- 실제 non-Standstill 관측
- Standstill 3회
- 최종 위치 3회 tolerance 이내
- Stop 1회
- Power Off 1회
- 최종 PowerOff+Standstill 3회

그다음이 Group 시험입니다. 첫 Group 시험은 `_LMCRobotBase1`의 `Run Enable ACK → Locked`이며, Buffered와 Stop-First는 그 결과를 분석한 뒤 진행하면 됩니다.

즉, 지금 당장 할 작업은 **최신 PLC 다운로드 후 Axis1 Power On 상태의 Drive Status 재확인**입니다.

> 테스트 진행했어
>
> User context
>
> Attachments:
> - Axis1_Test.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis1_Test.pcapng`
> - Axis1_Test.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis1_Test.txt`

<details><summary>6 previous messages</summary>

> 새 Axis1 캡처를 실제 payload 기준으로 확인하겠습니다. 이번 판정의 핵심은 최신 PLC 반영 여부(`0x633F`/새 BootId), Power On 상태의 `0x6041 Warning`, 그리고 마지막 Power Off 안전 상태입니다.
>
> 이번 시험은 안전하게 Power Off로 끝났지만 합격은 아닙니다. 새 BootId `0x0D`는 확인됐는데 capability가 기대한 `0x633F`가 아니라 `0x00000001`로 축소되어 SDO 읽기 3개가 모두 wire 전에 차단됐습니다. Health에서는 Power On 중 Axis1 `0x6041=0x12B7`로 Warning bit가 여전히 1입니다. 원인을 더 좁히고 있습니다.
>
> 두 문제가 분리됐습니다. 통신·EtherCAT은 정상이고 Power Off도 안전하게 끝났지만, PLC에는 현재 작업 소스가 아니라 `Health-only capability=0x1`인 다른/과거 빌드가 올라갔습니다. 동시에 Axis1은 Operation Enabled 상태에서도 Warning이 남았습니다. 이제 모션이 아니라 배포 경로와 드라이브 경고 원인 진단으로 전환해야 합니다.
>
> 이것도 진행했어
>
> User context
>
> Attachments:
> - 20260731_152953_SingleAxisPowerMoveStopPowerOff_Axis2.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/20260731_152953_SingleAxisPowerMoveStopPowerOff_Axis2.txt`
> - 20260731_152953_SingleAxisPowerMoveStopPowerOff_Axis2.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/20260731_152953_SingleAxisPowerMoveStopPowerOff_Axis2.pcapng`
> - 20260731_153048_SingleAxisPowerMoveStopPowerOff_Axis3.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/20260731_153048_SingleAxisPowerMoveStopPowerOff_Axis3.txt`
> - 20260731_153048_SingleAxisPowerMoveStopPowerOff_Axis3.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/20260731_153048_SingleAxisPowerMoveStopPowerOff_Axis3.pcapng`
> - 20260731_153126_SingleAxisPowerMoveStopPowerOff_Axis4.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/20260731_153126_SingleAxisPowerMoveStopPowerOff_Axis4.txt`
> - 20260731_153126_SingleAxisPowerMoveStopPowerOff_Axis4.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/20260731_153126_SingleAxisPowerMoveStopPowerOff_Axis4.pcapng`
>
> Axis2·3·4 결과도 같이 받았습니다. 지금 진행 중인 Axis1 진단과 합쳐서 네 축의 실제 이동·최종 안전 상태·runtime identity를 한 표로 판정하겠습니다. 이미 확인된 `capability=0x1`과 Warning 문제 때문에, “움직였다”와 “정식 합격”은 분리해서 보겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
</details>

결론: 캡처는 정상적으로 됐지만, Axis2~4의 실제 이동 시험은 실행되지 않았습니다. 모두 `Referenced=FALSE`에서 안전 차단됐습니다.

| 축 | 결과 | 이동 명령 | 최종 상태 |
|---|---|---:|---|
| Axis1 | FAIL — SDO 기능 미광고, Warning 유지 | 미실행 | PowerOff 안전 확인 |
| Axis2 | FAIL — Referenced=FALSE | 0회 | PowerOff 안전 확인 |
| Axis3 | FAIL — Referenced=FALSE | 0회 | PowerOff 안전 확인 |
| Axis4 | FAIL — Referenced=FALSE | 0회 | PowerOff 안전 확인 |

확인된 핵심 사항:

- Axis2~4 모두 PowerOn 후 `PowerOn + Standstill`까지 들어갔지만 `Referenced=FALSE`여서 중단됐습니다.
- 따라서 Move Relative/Absolute, 위치 읽기, Stop 명령은 모두 0회입니다.
- 네 축 모두 최종 PowerOff와 Standstill은 정상입니다.
- TCP 재전송, 패킷 누락, RST는 없습니다.
- 캡처 유지 시간도 Axis2 `16.899초`, Axis3 `6.369초`, Axis4 `2.204초`로 통과했습니다.
- 모든 캡처가 `CapabilityBits=0x00000001`, `BootId=0x0D`입니다. 현재 소스가 계산하는 값은 `0x633F`이므로 실행 PLC가 현재 소스와 맞지 않습니다. [LMCDiagnosticsService.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1337)
- 이 때문에 Axis1의 `0x6061`, `0x6041`, `0x603F` 읽기도 실행되지 않았습니다. [Axis1 결과](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis1_Test.txt:20)
- Axis1 Health의 DS402 상태는 `0x12B7`: Fault는 없지만 Warning은 여전히 켜져 있습니다.

지금은 추가 이동이나 그룹 테스트를 하지 마십시오. 다음 순서입니다.

1. LASAL 배포부터 수정
   정확히 `C:\work\Elmo\Elmo_Master\Lasal_PRG\Elmo_EtherCAT_Test_4Axis` 프로젝트를 열고 `LMCDiagnosticsService`를 Rebuild → Link → Download → Restart합니다. `_Edit` 또는 이전 복사본은 사용하지 않습니다.

2. 재접속 후 첫 합격 조건
   `CapabilityBits=0x633F`, `Map=0x957F101E`, `BootId`가 `0x0D`와 다른 새 값이어야 합니다. 다시 `0x00000001`이면 테스트하지 말고 LASAL Build/Link 결과를 확인해야 합니다.

3. 네 축 Reference 복구
   현재 네 축 전부 `Referenced=FALSE`입니다. 승인된 장비 Homing/절대엔코더 복구 절차로 Reference를 먼저 살려야 합니다. 현재 API의 Home Check는 상태 확인만 하며 실제 Homing을 수행하지 않습니다. [Reference 구현 상태](/C:/work/Elmo/Elmo_Master/docs/architecture/AXIS_REFERENCE_LASAL_NATIVE_DORMANT_CONTRACT_2026-07-31.md:11)

4. 그다음 Axis1부터 재시험
   `6061=8`, `6041 Warning=0/Fault=0/OperationEnabled=1`, `603F=0`, `Referenced=TRUE`를 확인한 뒤 Axis1→2→3→4 이동 시험을 다시 수행합니다.

즉, 지금 당장 이어서 할 것은 이동 시험이 아니라 **올바른 PLC 프로젝트 재배포와 네 축 Reference 복구**입니다.

> Reference하고 테스트했어.
>
> User context
>
> Attachments:
> - Axis2_20260731_165407_SingleAxisPowerMoveStopPowerOff.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis2_20260731_165407_SingleAxisPowerMoveStopPowerOff.txt`
> - Axis2__20260731_165407_SingleAxisPowerMoveStopPowerOff.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis2__20260731_165407_SingleAxisPowerMoveStopPowerOff.pcapng`
> - Axis3_20260731_165505_SingleAxisPowerMoveStopPowerOff.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis3_20260731_165505_SingleAxisPowerMoveStopPowerOff.txt`
> - Axis3_20260731_165505_SingleAxisPowerMoveStopPowerOff.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis3_20260731_165505_SingleAxisPowerMoveStopPowerOff.pcapng`
> - Axis4_20260731_165651_SingleAxisPowerMoveStopPowerOff.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis4_20260731_165651_SingleAxisPowerMoveStopPowerOff.txt`
> - Axis4_20260731_165651_SingleAxisPowerMoveStopPowerOff.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis4_20260731_165651_SingleAxisPowerMoveStopPowerOff.pcapng`

<details><summary>3 previous messages</summary>

> Reference 이후 캡처 3개를 축별로 다시 판정하겠습니다. 이번에는 `Referenced=TRUE`만 보지 않고, 실제 Move 명령·위치 변화·Stop/PowerOff 완료·최종 안전 상태·TCP 이상·PASS 후 캡처 유지시간까지 확인하겠습니다.
>
> 텍스트 로그상 Axis2~4는 모두 이번에는 `Referenced=TRUE`로 preflight를 통과했고, 상대이동 `+10000` 및 최종 위치 3회 확인까지 PASS입니다. 지금 PCAP에서 이 기록이 실제 wire 명령/응답과 일치하는지 교차 확인 중입니다.
>
> 세 축의 결과는 로그와 PCAP이 일치합니다. Axis2~4 기본 상대이동 시험은 실제로 모두 PASS했고, 각각 최종 `PowerOff + Standstill`까지 확인됐습니다. 다만 세 캡처 모두 실행 PLC가 계속 `CapabilityBits=0x00000001`을 반환하므로, 현재 소스 배포 일치 문제와 DS402 Warning 확인은 별도 미해결 상태입니다.
</details>

결론: **Reference는 정상 반영됐고 Axis2~4 기본 단일축 상대이동 시험은 모두 PASS**입니다.

| 축 | 위치 변화 | 이동 관측 | 최종 안전 상태 | PASS 후 캡처 |
|---|---:|---:|---|---:|
| [Axis2](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis2_20260731_165407_SingleAxisPowerMoveStopPowerOff.txt:16) | `29997 → 39997` | 18회 | PowerOff+Standstill | 27.027초 |
| [Axis3](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis3_20260731_165505_SingleAxisPowerMoveStopPowerOff.txt:16) | `29998 → 39998` | 17회 | PowerOff+Standstill | 31.738초 |
| [Axis4](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis4_20260731_165651_SingleAxisPowerMoveStopPowerOff.txt:16) | `29998 → 39998` | 18회 | PowerOff+Standstill | 2.816초 |

세 축 공통으로:

- PowerOn 상태 `0x22D0000F`: `Referenced=TRUE`, `Standstill=TRUE`, 오류 0
- `0x20A0` 상대이동 정확히 1회, 자동 replay 0
- non-Standstill 관측 후 Standstill 3회
- 최종 위치 3회가 목표 ±100 안
- Stop `0x2022` 1회, PowerOff `0x2023(false)` 1회
- 최종 `0x22D0420E` 3회
- 요청/응답 누락, 재전송, TCP gap, RST, 캡처 drop 없음

단, Stop은 이동 완료 후 실행됐으므로 **이동 중 Stop 성능은 아직 미검증**입니다.

아직 그룹 테스트는 하지 마십시오. 남은 문제는 다음입니다.

1. 세 캡처 모두 `CapabilityBits=0x00000001`입니다.
2. 현재 디스크 소스는 SDO Write와 Test Reset bit 18까지 켜져 있어 예상값이 **`0x0004633F`**입니다. 앞서 말한 `0x633F`는 현재 소스에는 더 이상 맞지 않습니다. [현재 capability 플래그](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:245)
3. 이번 캡처에는 `0x7E10` Health와 D5 SDO 읽기가 없어 DS402 Warning 해제 여부가 확인되지 않았습니다.
4. 기존 Axis1 이동 PASS는 `BootId=0x0C`이고, 이번 결과는 `BootId=0x0D`입니다. 따라서 같은 PLC 세대의 4축 완료 증거는 아직 `3/4`입니다. [이전 Axis1 결과](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test4/Axis1_20260731_141635_SingleAxisPowerMoveStopPowerOff.txt:2)

지금 바로 이어서 할 수 있는 안전한 테스트는 이것입니다.

1. 새 캡처 시작
2. `Home Check (X/Y/Z/U)` 실행 → `4/4 Referenced`
3. `Read Legacy Elmo Health` 실행 → 전 축 Online, EtherCAT OP, AL=0, Fault=0, Warning=0
4. 마지막 결과 후 3초 이상 캡처 유지
5. TXT와 PCAP 저장

그 다음 현재 LASAL 소스를 Rebuild/Link/Download/Restart하여 `CapabilityBits=0x0004633F`와 새로운 BootId를 확인해야 합니다. 다운로드 후에는 Reference가 유지되는지 다시 확인하고 Axis1~4를 같은 새 BootId에서 재시험해야 합니다.

그것까지 통과하면 그룹 시험 순서는:

`Group Power On → Set Identity → Run Enable ACK → Locked → Disable → Power Off`

이후에 `Buffered A→B`, 마지막으로 `Deterministic Stop-First`를 진행하면 됩니다.

> User context
>
> Attachments:
> - Home Check_Read Legacy Elmo Health.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/Home Check_Read Legacy Elmo Health.pcapng`
> - Home Check_Read Legacy Elmo Health.txt: `./test/packet_capture/SIGMATEK_API_Analyze/Test4/Home Check_Read Legacy Elmo Health.txt`

<details><summary>2 previous messages</summary>

> Home Check와 Legacy Health 결과로 이해하고 바로 판정하겠습니다. `4/4 Referenced`, EtherCAT OP/AL 상태, 각 축 DS402 Fault·Warning, Capability/BootId, TCP 이상과 캡처 유지시간을 PCAP까지 대조하겠습니다.
>
> 결과가 명확합니다. Home Check는 payload 기준 `4/4 Referenced`로 통과했지만, Health의 네 축 DS402 상태가 모두 `0x02B3`이라 **Warning bit 7이 여전히 켜져 있습니다**. 따라서 로그의 “Health PASS”는 RPC 성공이고, 그룹 운전 준비 PASS는 아닙니다.
</details>

판정: **Home Check PASS / EtherCAT 통신 PASS / 드라이브 운전 준비 FAIL**입니다.

| 항목 | 결과 |
|---|---|
| Home Check | PASS — 4/4 Referenced |
| 최종 축 상태 | 전 축 PowerOff + Referenced + Standstill, 오류 0 |
| EtherCAT | Master/4축 OP, AL Status 0, invalid cycle 0 |
| DS402 Fault | 전 축 0 |
| DS402 Warning | **전 축 1 — FAIL** |
| Capability | **`0x00000001` — 현재 소스와 불일치** |
| 캡처 품질 | 요청/응답 15/15, drop·재전송·RST 없음 |

상세 상태:

- Axis1: `0x2290020E`
- Axis2~4: `0x22D0420E`
- 전 축 DS402: `0x02B3`
  - OperationEnabled=0
  - Fault=0
  - **Warning=1**
  - InternalLimit=0
  - Remote=1

[Home Check 로그](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test4/Home%20Check_Read%20Legacy%20Elmo%20Health.txt:4)와 PCAP이 동일하게 4축 Referenced를 증명합니다. 하지만 [Health PASS](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test4/Home%20Check_Read%20Legacy%20Elmo%20Health.txt:7)는 RPC 성공이라는 뜻이며, Warning 해제를 뜻하지 않습니다.

또한 이번에는 Home Check만 수행됐습니다.

- Set Identity `0x20E7`: 0회
- Group Power/Enable `0x2047`: 0회
- Group Move: 0회

따라서 아직 그룹 테스트로 넘어가면 안 됩니다.

현재 소스를 다시 확인한 결과 TW20 Test Reset은 `FALSE`로 변경돼 있으므로, **현재 예상 Capability는 `0x0000633F`**입니다. PLC는 계속 `0x00000001`, `BootId=0x0D`를 반환합니다.

다음 작업은 테스트가 아니라 PLC 배포 정합성 수정입니다.

1. 현재 소스 상태를 고정합니다.
2. 정확한 `Elmo_EtherCAT_Test_4Axis` 프로젝트를 Rebuild → Link → Download → Restart합니다.
3. 재접속 후 다음을 확인합니다.
   - Capability=`0x0000633F`
   - Map=`0x957F101E`
   - BootId가 `0x0D`와 다른 새 값
4. Axis1~4에서 Move 없이 `0x6061`, `0x6041`, `0x603F`를 읽습니다.
   - Warning=0
   - Fault=0
   - InternalLimit=0
   - `0x603F=0`
5. Elmo EAS에서도 각 드라이브의 활성 Warning 원인을 확인합니다. 원인이 확인되기 전 Reset, TW20, SDO Write는 하지 마십시오.

이 조건을 통과한 다음에만 `Home Check → Group Power On → Set Identity → Enable ACK→Locked → Disable → Power Off` 순서로 진행하면 됩니다.
