
> move linear 한번 실행하고 stop한담에 다시 실행하니까 실행이 안돼.
>
> 그리고 위에 순서가 해깔리니까 read Status에 2 5 이렇게 하고 Move를 6으로 놔야될꺼같다.
>
> User context
>
> Attachments:
> - [14:23:12.618] Example ready. Connect, load _LMCAxis1, and start with Read Stat…: `C:/Users/dreiv/.codex/attachments/2a815607-ece1-4d74-826b-f9121b671652/pasted-text.txt`

<details><summary>10 previous messages</summary>

> 로그를 기준으로 Stop 이후 재실행이 막히는 상태 전이부터 확인하고, 예제 화면의 순서 표기도 `Read Status = 2·5`, `Move = 6`으로 맞추겠습니다. LASAL 명령 처리와 WPF 버튼 가드를 함께 대조하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 로그에서 재실행 거절 코드는 `7 = _LMCPROF_SWE_ERROR`로 확인됐습니다. Stop 자체가 해제되지 않은 문제가 아니라 두 번째 목표점이 PLC의 실시간 소프트웨어 끝단 제한에 걸린 것입니다. 이 제한은 우회하지 않고, 화면 순서 상태를 정확히 고치면서 거절된 목표 원시값도 로그에 남기도록 수정하겠습니다.
>
> 화면 로직에도 실제 문제가 하나 있습니다. 현재는 `Enable` 응답만 성공하면 바로 “Lock 완료”로 간주해 Move를 켜고 있습니다. 이를 `Enable 수락 → 5번 Read Status에서 Locked/Standby 확인 → 6번 Move 활성화`로 바꾸겠습니다. Stop은 현재 의미가 맞는 `Mode 3(감속 정지 + 기존 큐 폐기)`를 유지합니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+3 -3)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+106 -5)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/API_MAPPING.md` (+3 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+12 -3)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+15 -6)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+9 -6)
>
> </details>
>
> 수정은 반영했습니다. 이제 WPF를 빌드해 상태 변수 누락이나 XAML 오류를 잡고, 문서·화면 표기와 실제 버튼 활성 조건이 같은지 정적 검증하겠습니다.
>
> Debug 빌드는 통과했습니다. Release 실행 파일은 현재 사용자가 실행 중이고 원격 디버거가 DLL을 잠가 일반 Rebuild만 실패했습니다. 실행 중인 장비 연결 프로그램은 종료하지 않고, 별도 임시 출력 폴더로 Release 컴파일을 검증하겠습니다.
>
> 검토에서 두 가지 복구 경로를 더 확인했습니다. `Enable` 뒤 Status가 계속 Disabled면 버튼이 영구 잠길 수 있고, Status 조회 실패 뒤 이전 Lock 상태가 남을 수 있습니다. 둘 다 fail-closed 방식으로 수정해 재시도 가능하게 만들겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+2 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+24 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+3 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+3 -2)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+55 -14)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+5 -4)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+4 -1)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+3 -2)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+6 -5)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+3 -2)
>
> </details>
</details>

수정 완료했습니다.

- UI 순서: `1 Power On → 2 Read Status → 3 Set Identity → 4 Enable → 5 Read Status → 6 Move`
- Enable 응답만으로 Move를 활성화하지 않고, 5번 Status에서 `Enabled/LockedStandby=True`가 확인돼야 활성화됩니다.
- Status 실패 시 이전 Lock 상태를 재사용하지 않습니다.
- `Disabled/Unlocked`가 3회 연속 확인되면 Enable 재시도가 가능합니다.
- Move 전송 시 `StartRaw`, `TargetRaw`, 속도·가속도·감속도·Jerk를 로그에 기록합니다.
- `ErrorId=7`이면 `_LMCPROF_SWE_ERROR`와 진단 방법을 명확하게 표시합니다.

첨부 로그의 재실행 실패는 Stop 해제 문제가 아닙니다. `ErrorId=7`은 실행 당시 목표점이 런타임 Software End Position 검사에 걸렸다는 뜻입니다. `StopMove(Mode:=3)`은 기존 궤적을 폐기하지만 새 Move를 금지하지 않으므로 그대로 유지했습니다.

수정 파일은 [MainWindow.xaml](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:394), [MainWindow.xaml.cs](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:714), [README.md](./LMC_Library/Codex_LASAL_WPF/README.md:31), [DESIGN.md](./LMC_Library/Codex_LASAL_WPF/DESIGN.md:64)입니다.

Debug와 별도 출력 Release 빌드 모두 통과했습니다. 현재 실행 중인 Release 프로그램이 기존 DLL을 잠그고 있으므로, 실제 Release 출력 갱신 전에는 실행 중인 예제를 닫고 다시 빌드해야 합니다.

다시 `Move → Stop → 같은 목표 Move`로 시험해 주세요. 또 `ErrorId=7`이면 새 로그의 `Move Linear Absolute input:` 줄과 실패 줄을 보내주면 어느 입력이 문제인지 좁힐 수 있습니다. 정확한 위반 축까지 확정하려면 LASAL의 축별 `AxReadSWEndPos`와 `ReadProfileError().SubErrorNo`도 필요합니다.

> 이거 실행이 안되던데 리미트에 걸리는건데 리미트에 왜 걸리는거지? 리미트에 걸릴게 없는데?
>
> User context
>
> Attachments:
> - [14:56:12.020] Example ready. Connect, load _LMCAxis1, and start with Read Stat…: `C:/Users/dreiv/.codex/attachments/5d68ebb0-1e48-4a8c-a1f6-b88f5ba02af5/pasted-text.txt`
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-5c3c8e96-0d6c-4f0a-9eb2-ad1e61bc89f6.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-116c609b-2a3e-4014-ab58-e3203bb3c22f.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-69f3b7ab-3987-4fa7-a585-ef72984d735c.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-f5915408-5737-4bfc-b4da-4fa19590eabb.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-bddb7e81-a993-40ea-934b-00e80b8438d0.png`

<details><summary>3 previous messages</summary>

> 새 로그의 원시 목표값과 네 축의 실제 `SWMin/SWMax`, `ExUnits/IntUnits`, 로봇 profile 설정을 수치로 대조하겠습니다. 화면상 리미트 값이 커 보여도, 그룹에서 사용하는 application-unit 변환이나 Stop 뒤 profile 기준점이 다르면 검사값이 달라질 수 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 로그 경계가 명확합니다. `1,200,000`은 양·음 방향 모두 통과하고 `1,300,000`부터 즉시 Error 7이므로, 센서 리미트가 아니라 단위 변환 뒤 내부 위치 범위가 끊기는 패턴입니다. 현재 `ExUnits=8,388,608`, `IntUnits=10,000` 조합에서 그 경계를 계산·소스와 대조 중입니다.
</details>

결론부터 말하면, 물리 리미트가 아니라 `ExUnits/IntUnits` 변환 후 MotionLib 내부 위치 범위에 걸린 것입니다.

현재 설정은 4축 모두 다음과 같습니다.

```text
ExUnits = 8,388,608
IntUnits = 10,000 (1 mm)
외부 엔코더 위치 = API Raw 위치 × ExUnits / IntUnits
```

| API TargetRaw | 화면 입력값 | 변환된 위치 | 결과 |
|---:|---:|---:|---|
| 1,200,000 | 120 | `0x3C000000` | 정상 |
| 1,280,000 | 128 | `0x40000000` | 내부 경계 |
| 1,300,000 | 130 | `0x41000000` | Error 7 |
| 2,000,000 | 200 | `0x64000000` | Error 7 |

MotionLib에는 유효 위치 경계가 다음처럼 정의돼 있습니다.

```st
#define _LMCAXIS_INVALID_POS 16#3FFFFFFF
```

따라서 현재 스케일에서 이론적인 마지막 허용 TargetRaw는 `1,279,999`입니다. 로그의 `1,200,000 성공 / 1,300,000 실패`와 정확히 맞습니다. [MotionLib 상수](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_LMCAxisBase/_LMCAxisBase.st:16), [현재 축 단위 설정](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/ONE_Motion_Network_Table.st:1255)

화면의 `SWMinPos=-117,440,512`, `SWMaxPos=117,440,512`는 사용자가 지정한 리미트가 아닙니다. 프로젝트가 `SWMinPos/SWMaxPos=0`으로 초기화돼 MotionLib 기본값이 표시된 것입니다. 실제 그룹 프로파일은 별도 SW limit이 없으면 자동 생성된 `±MaxModulo`를 최종 목표 허용 범위로 사용합니다. 그래서 화면 SWMax보다 작은 값이어도 Error 7이 발생합니다.

추가로 2번 축은 별도 문제가 보입니다.

- 1·3·4축 `OutputSignal`: `1,006,632,960 = 0x3C000000`
- 2번 축 `OutputSignal`: `2,080,374,784 = 0x7C000000`
- 차이: 정확히 `0x40000000`

2번 축은 absolute encoder/reference 원점이 다른 축보다 `0x40000000`만큼 밀려 있습니다. 130을 명령하면 예상 출력이 `0x81000000`이 되어 signed DINT 범위도 넘습니다. 다만 이 오프셋이 drive absolute position인지, 저장된 reference offset인지는 현재 화면만으로 확정할 수 없습니다.

조치는 다음 순서가 맞습니다.

1. `IntUnits=1 mm/rev`가 실제 기구의 1회전 이동량과 맞는지 확인합니다.
2. 2번 축의 DS402 `Actual Position(0x6064)`, `Target Position(0x607A)`, reference/encoder offset을 확인합니다.
3. 축별 `LMCAXIS_PAR_RD_MAX_MODULO`와 `ReadProfileError().SubErrorNo`를 읽어 실제 제한값과 최초 검출 축을 확인합니다.
4. SW limit 검사 비활성화는 하지 않아야 합니다. 숫자 overflow까지 숨기게 됩니다.

즉 API나 Stop 문제는 아닙니다. 핵심 원인은 현재 `8,388,608 counts / 1 mm` 축 스케일이며, 2번 축에는 추가 원점 오프셋 문제도 있습니다. 이번에는 진단만 했고 코드는 변경하지 않았습니다.

> 실제 저렇게 리미트 숫자까지 받을수 있어야되는데? 어짜피 PLC에서 쓰는 유닛은 8,388,608이 아니라 1mm = 10000 이거야. 그리고 GUI에서 10000 이렇게 넣지말고 우리가 작성한 PLC Unit을 콤보박스로 선택할 수 있게 만들어. 그리고 거기에도 원하는 유닛이 없으면 선택 안함으로 해서 쓰게 해

<details><summary>9 previous messages</summary>

> 요청대로 두 부분을 분리해 반영하겠습니다. GUI는 숫자 입력 대신 LASAL의 `PLC Unit` 목록을 선택하는 콤보박스로 바꾸고, `선택 안 함`은 배율 1로 원시값을 보내게 하겠습니다. PLC 쪽은 API 단위 `1 mm = 10000`과 하드웨어 엔코더 비율 `8,388,608 count/rev`를 혼동하지 않도록, 큰 좌표가 프로파일 한계에서 잘리는 지점을 먼저 확인한 뒤 수정하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> GUI 쪽은 현재 API에 이미 정의된 `LMC_Units` 상수와 정확히 맞춰 콤보 항목을 만들고 있습니다. PLC 범위는 `PLC 표시 단위(1 mm = 10000)`와 DS402 하드웨어 카운트 변환을 분리해서 보고 있으며, 축 스케일을 10000으로 덮어써 실제 모터 이동량을 망가뜨리는 변경은 하지 않겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+6 -8)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+96 -39)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+5 -5)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+42 -0)
>
> </details>
>
> GUI 변경은 기본값을 `mm (×10000)`으로 유지하고, `None / raw DINT`에서는 `117440512` 같은 값을 그대로 전송하도록 구현했습니다. Raw 모드에서는 소수 입력을 반올림하지 않고 거부하게 해 단위 착오를 막았습니다. 이제 실제 빌드와 PLC 범위 경로를 각각 검증하겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/API_MAPPING.md` (+4 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+15 -7)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+10 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md` (+27 -4)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+4 -1)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md` (+25 -0)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+6 -0)
>
> </details>
>
> PC 쪽 검증은 끝까지 통과시키는 중입니다. 현재 Debug WPF 빌드는 경고·오류 0이고, `mm 1 → 10000`, `raw 117440512 → 117440512`, Raw 소수 거부를 실제 컴파일된 코드로 확인했습니다. PLC의 `117440512` 표시는 별도로 분석됐는데, 현재 encoder 변환비에서는 그 값 자체가 실제 구동 가능한 DS402 범위를 뜻하지 않습니다. 이 부분은 최종 결과에서 숫자로 분명히 구분하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/API_MAPPING.md` (+2 -2)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+1 -1)
>
> </details>
</details>

반영 완료했습니다.

- Axis/Group의 숫자 UNIT 입력을 콤보박스로 교체했습니다.
- 선택 항목:
  - `mm (×10000)` — 기본값
  - `m (×10000000)`
  - `deg (×10000)`
  - `None / raw DINT` — 변환 없이 정수 그대로 전송
- Raw 모드에서는 `117440512`가 그대로 전송되며, 소수 입력은 거부합니다.
- Read Position도 선택한 UNIT으로 역변환하고, Raw 모드에서는 DINT만 표시합니다.
- `8,388,608`은 encoder 측 `ExUnits`이며 PC API UNIT으로 사용하지 않습니다.

구현은 [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:261), [MainWindow.xaml.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:16)에 반영했고, [UNIT 변환 매뉴얼](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md:50)도 업데이트했습니다.

중요한 한계가 있습니다. GUI/API는 `117440512`를 받을 수 있지만, 현재 `ExUnits=8388608`, `IntUnits=10000` 설정에서 이 값을 실제 모션으로 실행할 수는 없습니다. 약 985억 encoder count가 필요해 DS402 signed DINT 범위를 초과합니다. offset 제외 이론상 양의 범위는 약 `2559999 DINT = 255.9999 mm`입니다. 따라서 LASAL 축 스케일은 임의로 바꾸지 않았습니다. 실제 11,744 mm 범위가 필요하면 축별 실제 리드(mm/rev), 감속비, absolute/multi-turn encoder 방식을 기준으로 `IntUnits`, MaxModulo, BinOffset을 다시 설계해야 합니다.

검증 결과:

- Debug 빌드: 경고 0, 오류 0
- Release 빌드: 별도 출력 경로에서 성공
- PC API 테스트: 46/46 통과
- LASAL source-only contract: 통과
- 변환 확인: `1 mm → 10000`, Raw `117440512 → 117440512`
- `git diff --check`, `git diff --cached --check`: 통과

커밋은 아직 하지 않았습니다.

> 요거 화면 짤리는거 보완해주고 위에 20 ui control paths/25 wire commands 이거 왜해놓은거임? 쓸데없으면 없애버려ㅕ
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-c4a350b2-897c-452a-b0fb-c19cc1991e8a.png`

<details><summary>9 previous messages</summary>

> 화면 상단의 `20 UI control paths / 25 wire commands`는 기능 상태가 아니라 구현 범위를 표시하던 개발용 문구라서 실행에 필요 없습니다. 제거하겠습니다. 화면 잘림은 탭 내부 스크롤 위치와 고정 높이/여백 구조를 확인해서, 작은 창에서도 첫 행 제목과 하단 결과 영역이 정상적으로 보이도록 레이아웃을 수정하겠습니다.
>
> 맞습니다. 이번 로그 기준으로 PLC 쪽 리미트는 아직 해소되지 않았습니다. 앞 변경은 GUI의 UNIT 변환과 Raw DINT 입력만 고친 것이고, LASAL의 `MaxModulo/축 변환 한계`는 그대로였습니다. 로그에서도 `1,000,000`은 통과하고 `2,000,000`부터 `_LMCPROF_SWE_ERROR(7)`로 거부돼 그 경계가 그대로 확인됩니다. 화면 수정과 함께, 2,000,000을 안전하게 허용할 수 있는 LASAL 초기화 위치와 축 offset 조건을 현재 소스 기준으로 다시 확인하겠습니다. `20/25` 배너는 기능과 무관하므로 제거합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+29 -19)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+44 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
