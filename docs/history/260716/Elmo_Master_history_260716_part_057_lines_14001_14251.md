
1. 실제 기계 전송량에 맞춰 `IntUnits`를 수정합니다.

```text
IntUnits = 모터 1회전당 실제 이동량(mm) × 10000
```

예를 들어 볼스크루가 모터 1회전당 10 mm 이동한다면:

```text
ExUnits  = 8388608
IntUnits = 100000     // 10 mm
```

PC는 여전히 `128 mm → 1,280,000`으로 보냅니다. 이 방법이 가장 정상적인 해결입니다.

2. 실제 기구가 정말 `1 mm/rev`라면 LASAL 초기화 단계에서 `MaxModulo`를 명시해야 합니다.

```pascal
LMCAxis1.SetParameter(
    ParNr := LMCAXIS_PAR_SET_MAXMODULO,
    Value := 2100000,
    mode := 0
);
```

`2100000`은 210 mm를 허용하는 예시입니다. 이 호출은 반드시 **마지막 Init 이전**에 실행해야 하며, TCP `CyWork`나 RT Task에서 실행하면 안 됩니다. RT Task를 새로 만들 필요도 없습니다.

현재 비율에서 가능한 이론상 최대치는 다음입니다.

```text
MaxModulo × 8388608 / 10000 <= 2147483647
MaxModulo 최대 = 2,559,999 raw = 255.9999 mm
```

따라서 실제 `1 mm/rev`가 맞으면서 256 mm 이상 이동해야 한다면 `MaxModulo`만으로는 해결할 수 없습니다. 기계 전송비, 드라이브 전자기어 또는 좌표 구조를 다시 설계해야 합니다.

3. `MaxModulo` 설정 후 실제 기계 범위로 SW Limit을 설정합니다.

```text
-MaxModulo < SWMin < SWMax < MaxModulo
```

그리고 Group을 Unlock한 뒤 다시 `LockProfile()`해야 Profile이 변경된 끝점을 반영합니다.

4. 다음 값을 4축 모두 확인합니다.

- `LMCAXIS_PAR_RD_MAX_MODULO`
- `LMCAXIS_PAR_RD_BINOFFSET`
- `AxReadSWEndPos` 최소/최대
- `ReadProfileError().SubErrorNo`

2번 축에는 큰 절대엔코더 `BinOffset`이 있는 것으로 보이지만, 이것을 임의로 0으로 만들면 안 됩니다. 스케일 변경 후 위치가 맞지 않을 때 정식 절대엔코더 참조 절차로 다시 설정해야 합니다. [_LMCABSEncoder.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_LMCABSEncoder/_LMCABSEncoder.st:26)

정확한 수정값을 결정하려면 각 축의 다음 두 값이 필요합니다.

- 모터 1회전당 실제 이동량 `mm/rev`
- 실제 기계 이동 범위 `최소 mm / 최대 mm`

이 값 없이 `IntUnits`나 `MaxModulo`를 확정해서 적용하면 실제 이동량이 달라질 수 있어 위험합니다. 관련 구분과 범위 조건은 [에러 방지 지침서](./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md:380)에도 정리되어 있습니다.

> 실제로 한바퀴에 10 mm 가는 시스템이면 Sigmatek system은 1280 mm 이상 보낼수 없다는 말인가? 아님 맥스 2560 mm 이상 보낼수 없는건가? 한계가 너무 명확한 시스템 아님? 매뉴얼을 보고 수정할 수 있으면 수정해줘
>
> User context
>
> Attachments:
> - SIGMATEK Navigation: Motion > Motion Control > _LMCAxis: Interfaces No contents…: `C:/Users/dreiv/.codex/attachments/112118a8-d5c0-4a41-9ccc-2d153f7d827d/pasted-text.txt`
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-9d71a653-8857-4205-9216-ed74de5464c6.png`

<details><summary>7 previous messages</summary>

> 첨부 매뉴얼의 `Resolution`, `Modulo`, `ExUnits/IntUnits`, `MaxModulo` 설명을 현재 MotionLib 소스와 대조하겠습니다. 핵심은 10 mm/rev라면 1,280 mm 또는 2,560 mm가 절대 시스템 한계인지, 아니면 `Resolution`·overflow 처리로 더 긴 유한축을 정상 지원하는지 확인한 뒤, 안전하게 확정되는 LASAL 설정만 수정하는 것입니다.
>
> <details><summary>Searched the web</summary>
>
> <details><summary>Searched the web</summary>
>
> - Searched the web for `site:elmomc.com 0x6093 position factor Elmo Gold drive EtherCAT ...`
>
> </details>
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 매뉴얼에서 핵심은 확인됐습니다. `MaxModulo`는 “총 이동거리 한계”가 아니라 내부 좌표가 접히는 overflow 위치이고, 최신 `_LMCAxis`는 SW limit가 비활성인 연속축에서 이 경계를 여러 번 넘어 이동하도록 설계돼 있습니다. 지금 발생한 SWE는 이 기능 자체보다 Group `_LMCProfile`의 기본 종점 사전검사가 `±MaxModulo`를 임시 limit로 쓰는 데서 발생한 쪽입니다. 안전하게 고칠 수 있는 범위를 더 좁히고 있습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md` (+57 -25)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+2 -1)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+13 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+4 -4)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+3 -2)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+9 -5)
>
> </details>
>
> 수정은 적용했습니다. 4축의 저장 transmission을 `ExUnits=8,388,608 / IntUnits=10 mm`로 맞췄고, PC의 `mm×10000` 계약은 그대로 유지했습니다. `Resolution`이나 Group limit 검사를 억지로 바꾸지는 않았습니다. 그 둘은 범위 해결책이 아니거나 기계 한계를 제거하는 변경이기 때문입니다. 이제 네트워크 생성값·WPF 문구·배포 문서가 같은 내용을 가리키는지 빌드와 diff로 확인하겠습니다.
>
> <details><summary>Searched the web</summary>
>
> <details><summary>Searched the web</summary>
>
> - Searched the web for `site:elmomc.com 0x6091 gear ratio 0x6092 feed constant user units Elmo EtherCAT ...`
>
> </details>
>
> </details>
</details>

결론부터 말하면, SIGMATEK 시스템이 1,280 mm 또는 2,560 mm까지만 움직이는 것은 아닙니다.

현재 수치의 의미는 다음과 같습니다.

| 조건 | 의미 |
|---|---|
| `10 mm/rev`, offset 0 | 약 `-2560 ~ +2559.9999 mm`의 한 좌표 창 |
| `+0x40000000` offset 존재 | 양의 여유가 약 `1280 mm`로 감소 |
| SW limit 없는 endless 단축 | MaxModulo overflow 후 남은 거리 계속 이동 가능 |
| 현재 Group `_LMCProfile` | 기본적으로 `±MaxModulo`를 종점으로 검사하여 SWE 발생 가능 |

`Resolution`은 범위를 늘리지 않습니다. `Resolution`을 키우면 `IntUnits`도 같은 비율로 키워야 하므로 변환식에서 약분됩니다.

적용한 수정은 다음과 같습니다.

- 4축 모두 `ExUnits=8388608`, `IntUnits=10 mm(100000)`로 변경
  - [Motion_Network.lcn](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/Motion_Network.lcn:27)
  - [ONE_Motion_Network_Table.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/ONE_Motion_Network_Table.st:1256)
- PC API의 `1 mm = 10000 DINT` 계약은 그대로 유지
- 1,280/2,560 mm, MaxModulo, endless axis, Group SWE 차이를 매뉴얼에 반영
  - [UNIT_CONVERSION_MANUAL_2026-07-10.md](./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md:84)
  - [SIGMATEK_LASAL_programming_error_prevention_guide.md](./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md:387)
- WPF 안내 문구와 문서도 새 스케일에 맞춤
  - [MainWindow.xaml](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:302)

강제로 `MaxModulo`를 확대하거나 `_LMCPROF_ChkEndPosForSwLimit=0`으로 끄지는 않았습니다. 현재 저장된 `SWMin/Max=±10000 mm`는 MaxModulo 밖이어서 실제 보호 limit가 비활성인 상태입니다. 실제 기계 최소·최대 위치를 모르는 상태에서 Group 검사를 끄면 위험합니다.

2,560 mm보다 넓은 유한축 절대좌표가 필요하면 Elmo 드라이브의 DS402 scaling을 적용해야 합니다. Elmo는 Gear Ratio와 Feed Constant를 이용한 [DS402 사용자 단위 scaling](https://www.elmomc.com/capabilities/servo-technology/special-functionality/scaling-factors/)을 지원합니다. 예를 들어 드라이브 PDO 자체를 `10000 unit/mm`로 만들면 이론적인 signed-DINT 좌표 범위는 약 ±214.7 m까지 늘어납니다. 정확한 EAS/CoE 설정값은 드라이브 모델과 실제 기계 한계를 확인한 뒤 적용해야 합니다.

검증 결과:

- WPF/API Debug 빌드 성공
- PC 테스트 `46/46` 통과
- LASAL 정적 계약 검사 통과
- LASAL IDE 빌드 및 PLC 다운로드는 아직 필요

다운로드 전제는 네 축 모두 실제로 `10 mm/rev`라는 것입니다. 축별 lead가 다르면 지금 다운로드하지 말고 각 축의 실제 `mm/rev`를 따로 지정해야 합니다. 다운로드 후에는 반드시 절대엔코더를 재참조하고 `MaxModulo`, `BinOffset`, `PosOffset`, `PosOffsetOk`를 확인해야 합니다.

> 지금 실제축 4축외에 가상축 5축을 lasal 프로그램에서 동작할 수 있도록 연결해놨어. 이걸 토대로 9축이 돌아 갈수 있도록 코드를 수정해..

<details><summary>14 previous messages</summary>

> 현재 저장된 네트워크에서 `_LMCAxis5..9` 객체·TCPMotionInterface 연결·Group 연결 상태를 먼저 확인한 뒤, 하드코딩된 `1..4` 범위를 PLC dispatcher, PC API, WPF 예제, 테스트까지 한 번에 `1..9`로 맞추겠습니다. 기존 4축 Group identity와 새 가상축을 임의로 같은 Group에 넣지는 않고, 실제 네트워크 연결을 기준으로 지원 범위를 결정하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 네트워크 확인 결과는 명확합니다. `_LMCAxis1..9`가 모두 `TCPMotionInterface1`과 `_LMCRobotBase1`에 실제로 연결돼 있습니다. 현재 막는 부분은 연결이 아니라 dispatcher가 축 참조를 `1..4`로 제한하고, Group LockProfile도 `Axis5..9:=0`으로 고정한 코드입니다. 이제 이 두 제한과 관련 테스트 계약을 9축 기준으로 맞추겠습니다.
>
> 구현 범위는 안전하게 분리했습니다. 이번 변경은 축 1~9를 각각 이름으로 로드해 Power/상태/위치/절대·상대·속도 이동/Stop/Reset까지 완전 제어하고, Group member 조회도 실제 연결된 9축을 반환하도록 합니다. 기존 X/Y/Z/U 4축 Cartesian Group Move는 별도 계약이므로 그대로 유지합니다. `LockProfile`만 9축으로 바꾸면 기존 4좌표 명령의 빈 슬롯 때문에 가상축이 0으로 움직일 수 있어 이번에는 건드리지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+93 -8)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+14 -14)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+78 -13)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+91 -21)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+60 -10)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+88 -8)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+60 -10)
>
> </details>
>
> 단축 dispatcher와 9축 member 응답까지 반영됐고, 정적 계약 검사는 현재 통과했습니다. TCP 인터페이스는 여전히 `RealtimeTask=false`이며 새 RT 작업은 추가하지 않았습니다. 이제 PC 회귀 테스트와 전체 네트워크/프레임 일치 검사를 돌리고 있습니다.
>
> <details><summary>Created a file, edited 3 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+6 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/NINE_AXIS_DISPATCH_IMPLEMENTATION_2026-07-15.md` (+55 -0)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+15 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+36 -1)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 마지막 정적 리뷰와 저장소 상태만 확인하고 있습니다. 특히 9축 확장 때문에 기존 4축 Cartesian Group 동작이 함께 넓어지지 않았는지, 실제 저장된 축 UNIT 설정이 기존 결정과 충돌하지 않는지를 최종 결과에 분리해서 적겠습니다.
>
> 최종 리뷰에서 LASAL CodeGenerator 관련 배포 위험 1건을 잡았습니다. 새 이름 버퍼 5개를 소스 선언부에만 추가하면 IDE 재생성 때 사라질 수 있습니다. 이 선언을 없애고, 이미 IDE 메타데이터에 등록된 기존 이름 버퍼 하나를 순차 재사용하도록 바꿔서 빌드·재생성에 강한 형태로 마무리하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+74 -84)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+78 -78)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/NINE_AXIS_DISPATCH_IMPLEMENTATION_2026-07-15.md` (+5 -0)
>
> </details>
>
> CodeGenerator 위험은 제거했습니다. 축 5~9 이름은 기존 등록 버퍼를 순차적으로 쓰고 각 응답 슬롯에 즉시 복사하므로, 별도 class variable 없이 9축 registry 응답을 유지합니다. 이제 PC 테스트·LASAL 정적 계약·예제 빌드 전체를 한 번 더 돌립니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -0)
>
> </details>
</details>

9축 개별 운전 코드 확장을 완료했습니다.

- `_LMCAxis1..9` 이름 조회와 descriptor 발급
- Read Status / Read Position
- Power On / Power Off
- Reset / Stop
- Move Absolute / Relative / Velocity
- Group member 응답을 9축으로 확장
  - Axis reference: 1~9
  - Device ID: 0~8
  - `AxisCount=9`
- TCPMotionInterface는 기존처럼 CyWork-only이며 RT Task를 추가하지 않음
- CodeGenerator에 새 변수를 직접 추가하지 않고 기존 등록 버퍼를 재사용하도록 처리

핵심 구현은 [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1076), 5~9축 연결은 [Comm_Network.lcn](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn:260), 구현 범위는 [NINE_AXIS_DISPATCH_IMPLEMENTATION_2026-07-15.md](./LMC_Library/LMC_API_Delivery/docs/NINE_AXIS_DISPATCH_IMPLEMENTATION_2026-07-15.md:1)에 정리했습니다.

검증 결과:

- PC API 테스트: 46/46 PASS
- LASAL 정적 계약 검사: PASS
- 기존 테스트 앱 빌드: PASS
- Codex LASAL WPF 빌드: PASS
- `git diff --check`: PASS

중요한 범위 제한이 있습니다. 현재 Group Cartesian 동작은 기존 X/Y/Z/U 4축만 Lock하고 `MoveLinearCoord`를 실행합니다. 따라서 현재 완료된 것은 “9축을 각각 독립적으로 제어하는 기능”입니다. 9축 동시 보간 운전은 target 9개를 명시적으로 받는 별도 Group API가 필요합니다.
