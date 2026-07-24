
- 상대이동이 먼저 송신됨: `0x7D22 → 0x2085`
- Stop이 먼저 처리됨: `0x2085` 이후 기존 클릭에서 발생한 `0x7D22`가 없어야 함

아래 순서가 나오면 결함입니다.

- `0x2085 → 나중에 기존 클릭의 0x7D22`

Stop 이후에는 `0x2045`에서 최종 `InPosition=True`도 확인해야 합니다.

## 추가 권장 회귀 캡처

필수 8개가 정상인 뒤 다음도 권장합니다.

- `09_Group_ReadPosition_None_ACS_2051.pcapng`
  - Coordinate `None`, `ACS` 각각 읽기
  - 현재 두 좌표계가 static identity 계약대로 같은 축 순서인지 확인

- `10_DriveRead_Axis1to4.pcapng`
  - Axis 1~4 각각 `Get Drive Operation Mode`, `Read Drive Status`
  - 기존 `0x2028`, SDO submit `0x7E50`, ticket poll `0x7E03` 회귀 확인

- `11_PI_Bulk_Regression.pcapng`
  - `EtherCAT / PI` 탭의 `Refresh Capabilities`
  - `Load PI Catalog`
  - `Read Selected PI`
  - `Bulk Snapshot`
  - 여기의 `Refresh Capabilities`는 Admin `0x7D00`과 다른 진단 capability 명령입니다.

## 캡처 방법

Wireshark 표시 필터:

```text
ip.addr == 10.10.150.1 && tcp.port == 4000 && tcp.len > 0
```

콜백까지 볼 때:

```text
udp.port == 5000
```

각 시험은 다음 방식으로 진행하세요.

1. Connect와 Group 준비를 먼저 완료합니다.
2. Execution Log를 지웁니다.
3. Wireshark 캡처를 시작합니다.
4. 표에 적힌 시험만 실행합니다.
5. 상태 poll이 최종 상태에 도달할 때까지 기다립니다.
6. 캡처를 종료하고 지정한 파일명으로 저장합니다.
7. `Copy Log` 결과도 같은 이름의 `.txt`로 저장합니다.

프레임 길이 참고:

- `0x7D00`: 요청/응답 application frame `16/48 bytes`
- `0x7D10`: `20/36 bytes`
- `0x7D20`: `20/40 bytes`
- `0x7D22`: `112/24 bytes`

TCP 분할·병합 때문에 이것을 Wireshark의 개별 `tcp.len`과 직접 동일시하면 안 됩니다. 실제 분석은 TCP stream을 재조립해서 해야 합니다.

Invalid axis/key/body 같은 입력 오류는 현재 PC SDK에서 송신 전에 차단하므로 Wireshark에는 아무 패킷도 나오지 않습니다. 그 오류 분기는 별도 raw test harness가 필요하므로 지금은 캡처하지 않아도 됩니다.

우선 `01`부터 `08`까지 가져오면 Admin Phase 1과 Group Relative Phase 2의 실물 동작 완료 여부를 판정할 수 있습니다. 시험 순서는 [테스트 앱 README](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/README.md:150)와 [구현 설계서](/C:/work/Elmo/Elmo_Master/docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md:429)에 맞춘 것입니다.

> 니가 요청한 캡쳐 진행했다. Log캡쳐는 나중에 확인해서 뭉뚱그려서 캡쳐했어.
>
> 그리고
>
> 필수 8개가 정상인 뒤 다음도 권장합니다.
> 09_Group_ReadPosition_None_ACS_2051.pcapng
> Coordinate None, ACS 각각 읽기
> 현재 두 좌표계가 static identity 계약대로 같은 축 순서인지 확인
>
> 10_DriveRead_Axis1to4.pcapng
> Axis 1~4 각각 Get Drive Operation Mode, Read Drive Status
> 기존 0x2028, SDO submit 0x7E50, ticket poll 0x7E03 회귀 확인
>
> 11_PI_Bulk_Regression.pcapng
> EtherCAT / PI 탭의 Refresh Capabilities
> Load PI Catalog
> Read Selected PI
> Bulk Snapshot
> 여기의 Refresh Capabilities는 Admin 0x7D00과 다른 진단 capability 명령입니다.
>
> 요 내용은 어디서 캡쳐하는지 모르겠다. 내용 확인해줘.
>
> User context
>
> Attachments:
> - 01_Admin_Capabilities_7D00.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/01_Admin_Capabilities_7D00.pcapng`
> - 02_Admin_AxisParameters_1to4_7D10.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/02_Admin_AxisParameters_1to4_7D10.pcapng`
> - 03_Admin_GroupParameters_7D20.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/03_Admin_GroupParameters_7D20.pcapng`
> - 04_Group_Absolute_Regression_20A4.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/04_Group_Absolute_Regression_20A4.pcapng`
> - 05_Group_Relative_Aborting_XYZU_7D22.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/05_Group_Relative_Aborting_XYZU_7D22.pcapng`
> - 06_Group_Relative_Buffered_7D22.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/06_Group_Relative_Buffered_7D22.pcapng`
> - 07_Group_Relative_StopRace_7D22_2085.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/07_Group_Relative_StopRace_7D22_2085.pcapng`
> - 08_Group_Recovery_2085_2048_204B.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/08_Group_Recovery_2085_2048_204B.pcapng`
> - API_TEST.txt: `./test/packet_capture/SIGMATEK_API_Analyze/API_TEST.txt`

<details><summary>2 previous messages</summary>

> 8개 패킷과 묶음 로그를 먼저 실제 TCP stream 기준으로 대조하겠습니다. 동시에 `09~11`이 현재 UI에서 가능한지, 정확히 어느 탭·버튼에서 실행해야 하는지도 소스와 화면 구조로 확인하겠습니다.
>
> 현재까지 확인된 핵심은 Admin `0x7D00/10/20`은 wire 기준 전부 정상입니다. 다만 모션 쪽에서는 두 가지가 이미 보입니다. Absolute는 실제로 움직였지만 큰 이동량 때문에 15초 모니터가 먼저 timeout 되었고, Recovery 캡처는 Power Off 뒤 `Read Status`가 아니라 `Read Position`을 눌러 최종 전원 OFF 검증이 빠졌습니다. 나머지 패킷 필드와 Stop 경합까지 계속 대조 중입니다.
</details>

결론부터 말하면 `01~03`은 완전 정상이고, 상대이동 `0x7D22` 자체도 정상입니다. 다만 `04`, `05`, `06`, `08`은 시험 범위가 일부 부족하며, 별도로 `0x2047 Group Enable`의 실제 결함이 하나 발견됐습니다.

## 캡처 판정

| 파일 | 판정 | 확인 결과 |
|---|---|---|
| `01_Admin_Capabilities` | PASS | `Feature=0x7`, AxisMask `0x3F`, GroupMask `0x7`, 축 4개, Group `0x0100` 정상 |
| `02_Admin_AxisParameters` | PASS | 축 1~4 × key 1~6, 총 24회 모두 성공. RequestId와 응답 24/24 일치 |
| `03_Admin_GroupParameters` | 통신 PASS | mask `1/2/4/7` 정상. 세 파라미터 값은 PLC가 모두 `0`을 반환 |
| `04_Group_Absolute` | 조건부 PASS | `0x20A4`와 실제 이동은 정상. 이동 완료 전에 WPF 15초 monitor가 먼저 timeout |
| `05_Relative_Aborting` | 기능 PASS, 축 매핑 미완료 | `0x7D22` 4회 성공. 하지만 X/Y/Z/U를 한 축씩 시험한 것이 아니라 네 축 모두 동일 delta로 움직임 |
| `06_Relative_Buffered` | 수락 PASS | Buffer=`2`로 두 번 성공. 실제 두 명령 동시 queue 동작은 검증하지 못함 |
| `07_StopRace` | PASS, move-first 분기 | 3회 모두 `0x7D22 → 0x2085`, Stop 뒤 추가 이동 없이 terminal 상태 확인 |
| `08_Recovery` | 불완전 | Stop, Disable, PowerOff ACK는 성공. PowerOff 뒤 최종 `0x2045 Read Status`가 없음 |

8개 파일 모두 TCP retransmission, lost segment, request/response 누락이 없습니다.

### Admin 결과값

축 1~4 모두 다음 값이었습니다.

- SoftwareMin: `-117440512`
- SoftwareMax: `117440512`
- EndPositionToleranceWindow: `0`
- MaxVelocity: `750000`
- MaxAcceleration: `75000000`
- ReferencePosition: `0`

그룹의 PathVelocityLimit, PathAccelerationLimit, JerkTime은 모두 `0`입니다. 프로토콜 오류는 아니며 PLC의 `ReadGroupParameter`가 실제로 0을 반환했습니다. 제한값이 설정돼 있어야 하는 상황이라면 LASAL group profile 설정을 별도로 확인해야 합니다.

## 발견된 실제 결함

`04`, `05` 준비 과정에서 `0x2047 Group Enable`이 다음 실패를 반환했습니다.

```text
CommandStatus=1, ErrorId=-6
```

그런데 약 2초 뒤 `0x2045`는 `0x40060000`, 즉 실제로는 Locked Standby 상태였고 이후 Absolute/Relative 이동도 성공했습니다.

현재 PLC 코드는 `LockProfile()` 호출 직후 같은 cycle에서 `LockState`가 즉시 반영됐는지 확인합니다. 비동기 상태 반영이 늦으면 실제 lock은 진행 중인데 ACK만 `-6`으로 실패하는 구조입니다.

[해당 LASAL 코드](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:2390)

수정 방향은 `LockProfile()`이 명령을 정상 접수하면 ACK 성공을 반환하고, 최종 Locked Standby는 이미 구현된 `0x2045` polling으로 확인하는 것입니다.

Absolute의 15초 timeout도 고정값 문제입니다. 이번 이동은 최장 축 기준으로도 가감속을 제외한 최소 이동시간이 약 19.48초라서 15초 내 완료가 불가능했습니다. 거리·속도 기반 동적 timeout으로 바꾸거나 사용자 설정값으로 만들어야 합니다.

## 09는 현재 UI에서 캡처할 수 없음

이건 사용자가 못 찾은 게 맞습니다. 제가 이전에 현재 UI에서 가능하다고 안내한 것이 잘못됐습니다.

`Group Motion` 탭에는 `Read Position` 버튼이 있지만 Coordinate 콤보에는 `None`만 들어갑니다.

- [Coordinate 초기화](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:69)
- [None 이외 차단 코드](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:2647)

`API_TEST.txt`의 마지막 두 번 `Read Group Position PASS`도 모두 `None`입니다. ACS를 시험하려면 UI에 ACS를 추가하되 다음처럼 분리해야 합니다.

- `Read Position`: None, ACS 허용
- Absolute/Relative Motion: 계속 None만 허용

따라서 `09`는 UI 수정 후 캡처해야 합니다.

## 10 캡처 위치

`Read-only API` 탭을 아래로 스크롤하면 `Physical drive reads` 영역이 있습니다.

[Physical drive reads 화면](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:1204)

실행 순서:

1. Axis `1` 선택
2. `Get Drive Operation Mode`
3. `Read Drive Status`
4. Axis `2`, `3`, `4`도 반복
5. 한 파일로 `10_DriveRead_Axis1to4.pcapng` 저장

`Read Drive Status` 하나만으로도 아래 항목이 모두 실행됩니다.

```text
0x2028 Axis Status
0x7E50 SDO 0x6041:0
0x7E03 Ticket poll
0x7E50 SDO 0x6061:0
0x7E03 Ticket poll
```

축을 처음 선택할 때 `0x103C`, `0x202B` lookup 패킷이 추가되는 것은 정상입니다.

## 11 캡처 위치

두 탭을 연속으로 사용해야 합니다. 이전 안내에서 `Configure Selected`를 빠뜨렸습니다.

1. `EtherCAT / PI` 탭

   - `Refresh Capabilities`
   - `Load PI Catalog`
   - 축 1~4의 `actual_position`이 `Use` 체크됐는지 확인
   - `Read Selected PI`

2. `Bulk Snapshot` 탭

   - `Configure Selected`
   - `Refresh Status`
   - `Read Snapshot`
   - `Release`

3. 전체를 `11_PI_Bulk_Regression.pcapng` 한 파일로 저장

[EtherCAT / PI 버튼](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:600)
[Bulk Snapshot 버튼](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:696)

## 추가로 필요한 캡처

현재 결과를 보완하려면 다음만 더 필요합니다.

- `05b_Group_Relative_Aborting_PerAxis_7D22.pcapng`
  - X만 `+1/-1`, Y만 `+1/-1`, Z만 `+1/-1`, U만 `+1/-1`
  - 나머지 축은 반드시 `0`
  - 현재 캡처는 네 축 delta가 같아서 축 순서가 뒤바뀌어도 검출할 수 없습니다.

- `08b_PowerOff_FinalStatus_2045.pcapng`
  - Power Off 뒤 `Read Position`이 아니라 `2 / 5 Read Status` 실행
  - `PowerOn=False` 확인

- `10_DriveRead_Axis1to4.pcapng`
- `11_PI_Bulk_Regression.pcapng`

`09`는 UI 수정 전에는 진행하지 마세요. 이번에는 분석만 수행했으며 소스나 문서는 수정하지 않았습니다.

> 그럼 내가 테스트를 진행하고 있을테니 너는 TEST UI를 개선하고 있어봐

<details><summary>31 previous messages</summary>

> 확인된 실기 불편을 기준으로 TEST UI를 바로 손보겠습니다. 우선 ACS 위치 읽기 경로를 실제로 노출하고, 모션은 계속 `None`만 허용하며, 고정 15초 모니터 timeout과 Power-Off 최종 상태 확인이 헷갈리는 부분을 개선하겠습니다. 기존 탭 흐름 안에서 수정하고 실제 모션 자동 실행은 추가하지 않겠습니다.
>
> 설계는 안전 경계를 유지하는 쪽으로 확정했습니다. 위치 읽기는 `None/ACS`, 모션은 계속 `None` 전용으로 분리하고, ACS 선택 시 Move 버튼을 비활성화합니다. 모션 timeout은 거리·속도·가감속으로 자동 산정하며, Power Off 후에는 `Read Position`을 잠그고 `Verify Power Off (Read Status)`로 다음 동작을 명확히 유도하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+23 -11)
