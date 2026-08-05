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
