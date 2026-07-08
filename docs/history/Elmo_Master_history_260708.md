# 히스토리 파일 분할해 분석

> 이 파일을 분석해서 이 쓰레드에서 작업을 이어 진행하도록하자. 히스토리 파일이 너무 크니까 날짜폴더 만들어서 니가 읽어들이기 편한 사이즈로 잘라서 나눠서 저장하고 개별 파일을 읽어 들여서 내용 분석해.
>
> User context
>
> Attachments:
> - Elmo_Master_history_260624.md: `./docs/history/Elmo_Master_history_260624.md`

<details><summary>13 previous messages</summary>

> 히스토리 파일의 구조와 크기부터 확인하겠습니다. 날짜/섹션 경계가 어떻게 잡혀 있는지 본 뒤, 원문을 보존하면서 읽기 쉬운 단위로 분할하겠습니다.
>
> 현재 파일은 약 176KB, `.NET` 기준 2191줄입니다. 내용은 6월 17일 히스토리를 다시 분할·요약한 기록과 이후 커밋/문서/API 분석 작업 기록이 이어진 형태라, `docs/history/260624` 아래에 원본 줄 범위가 붙은 파트 파일과 요약 파일을 만들겠습니다.
>
> 이제 분할 파일을 생성합니다. 원본 `Elmo_Master_history_260624.md`는 그대로 두고, 각 파트에는 원본 파일명과 원본 줄 범위를 붙여 근거 추적이 가능하게 하겠습니다.
>
> 분할은 정상 생성됐습니다. 실제 줄 수는 `ReadAllLines` 기준 2400줄이라 250줄 단위 10개 파트가 됐습니다. 이제 파트별로 요청, 구현, 검증, 남은 판단 포인트를 뽑아 요약 파일로 남기겠습니다.
>
> 검색 정규식 하나가 한글 범위 해석 때문에 실패했습니다. 분할 파일은 이미 만들어졌고, 분석 추출만 더 단순한 패턴으로 다시 돌립니다.
>
> `rg`로 뽑은 키워드만으로는 중간 결론이 빠질 수 있어서, 각 파트에서 blockquote가 아닌 최종 응답 문장과 요청 라인을 따로 추출해 보고 있습니다. 이 방식이 이 히스토리 파일에는 더 정확합니다.
>
> 파트 3~6이 실제 이어받을 작업에 가장 중요합니다. 여기에는 패킷 분석, LASAL TCP 구현, 단위 스케일 정정, 커밋 상태가 들어 있어서 해당 구간은 원문을 더 촘촘히 확인합니다.
>
> 히스토리상 마지막 미완료 지점은 “API 분석 문서 3개 생성 후 아직 커밋 안 됨”과 “Sigmatek GroupReadStatus In-position 검증은 다음 캡처 필요”입니다. 이제 실제 현재 저장소 상태를 확인해서 히스토리와 현재 워크트리가 맞는지 대조하겠습니다.
>
> 요약 파일을 추가하겠습니다. 여기에는 “확정된 사실”, “현재 이어갈 작업”, “주의할 오해”, “현재 워크트리 상태”를 분리해서 적겠습니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./docs/history/260624/analysis_summary.md` (+240 -0)
>
> </details>
>
> 요약 파일까지 추가했습니다. 인덱스에서도 바로 찾을 수 있게 `analysis_summary.md` 링크와 파트별 주제를 덧붙이겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/history/260624/index.md` (+13 -12)
>
> </details>
>
> 분할/요약 산출물은 작성됐습니다. 마지막으로 줄끝 공백, 라인엔딩, 파일 목록을 확인해서 바로 이어 작업할 수 있는 상태인지 보겠습니다.
</details>

완료했습니다. 원본 `docs/history/Elmo_Master_history_260624.md`는 수정하지 않았고, 새 폴더 `docs/history/260624`에 250줄 단위로 10개 파트로 분할했습니다.

생성/갱신 파일:
- [index.md](/C:/work/Elmo/Elmo_Master/docs/history/260624/index.md:1)
- [analysis_summary.md](/C:/work/Elmo/Elmo_Master/docs/history/260624/analysis_summary.md:1)
- `Elmo_Master_history_260624_part_01.md` ~ `part_10.md`

분석 핵심:
- 현재 이어갈 기술 작업은 단위 변환 재수정이 아니라 `MMCMoveLinearAbsoluteExCmd`로 다시 캡처해서 `GroupReadStatus`가 `0x40020000`까지 가는지 확인하는 것입니다.
- 현재 최종 스케일 기준은 `Codex_PMAS_WPF 8,388,608 count == Codex_LASAL_WPF 360 unit`이고, LASAL WPF 전송은 `입력값 * 10000`입니다.
- `GroupReadStatus`는 `0x2045`, `MoveLinearAbsoluteEx`는 `0x20A4`, 진짜 non-Ex `MoveLinearAbsoluteCmd`는 `0x2043`입니다.
- 이전 Sigmatek 캡처는 `GroupReadStatus = 0x40000000`이라 `0x00020000` in-position bit가 안 섰습니다. 계속 그러면 LASAL `AxInPosition(PositionWindow:=0)` 허용창을 현실적인 값으로 바꿔야 합니다.
- `docs/api_analysis/` 문서 3개는 현재 존재하지만 아직 untracked 상태입니다.

검증:
- `git diff --check -- docs/history/260624 docs/history/Elmo_Master_history_260624.md` 통과
- 생성한 md 파일 전부 `CRLF=0`, trailing whitespace `0`
- 현재 `git status --short`는 untracked로 `data_capture/`, `docs/api_analysis/`, `docs/history/260624/`, `docs/history/Elmo_Master_history_260624.md`가 보입니다.

> Hello Seulki,
>
> That is a good suggestion. So I will try to explain our thoughts more detailed.
>
> From the end customers perspective, the performance of the machine is how fast their part is produced. The production time of a part is the sum of all needed processes (movement, activation time of actors for placing, etc.).
>
> The time of the communication between PLC and PC adds to each movement:
> Time for one move = Time of sending cmd + movement of axis + time checking cmd done
>
> Normally the part "movement of axis", should take a lot more time than the other two parts and we assume is the same for both systems (Elmo and Sigmatek).
>
> To check how big the influence of the communication time is on the performance of the whole machine, we suggest making a worst-case assumption for the test. This means we reduce the movement time as far as reasonable possible (depending on the customers typical demanding applications), to increases the meaning of the delay to a maximum weight.
> As an example, if movement costs us 198ms we add for the communication 2ms, so the whole time for the movement would be 200ms. If the movement instead would only take 98ms we get 100ms. For the second example the communication time has more impact.
>
> We assume that due to bus times, CPU cycle time and the movement time in general the communication time does not make a big difference between 0,1 and 1ms. So, for the test we would make many cycles of movement on both systems. At the end we would compare the total time needed to complete the test. This time is the result we would like to discuss with you.
>
> For the test following points are important:
> 1. The dynamics of the systems are the same, just the movement of one axis should take the same time on both systems or the difference must be known.
> 2. The chosen movement cycle is realistic and demanding. Either the customer can provide a test cycle, or you have to make assumptions on how short and fast the placing can be.
> 3. Are there any known delays in placing parts. E.g. Waiting for vacuum to drop or a gripper to open.
>
> I hope this makes our motivation for the performance test clearer and easier to follow.
> I will try to answer any questions , If you find any points unclear while testing.
>
> Beste Grüße,
>
> M.Sc. Jonas Dräger
>
> Applikation
>
> ________________________________
>
> SIGMATEK GmbH & Co KG
>
> Sigmatekstraße 1
>
> 메일내용 해석해서 이 친구가 하고자 하는 모션이 어떤건지 분석해봐

이 메일의 핵심은 “특정 멋진 궤적”이 아니라 **통신 지연이 전체 생산 사이클에 얼마나 영향을 주는지 보기 위한 짧고 빠른 반복 모션 테스트**를 하자는 겁니다.

정확히 말하면 Jonas가 원하는 모션은 이겁니다.

- Elmo와 SIGMATEK에서 **같은 동특성**으로 움직이는 모션
- 가능하면 **1축 기준**으로 먼저 비교
- 이동 시간 자체를 가능한 짧게 잡아서 통신 지연 영향이 크게 보이게 함
- 한 번만 보는 게 아니라 **많은 사이클 반복**
- 마지막에는 개별 통신 latency가 아니라 **전체 테스트 완료 시간**을 비교

메일의 식으로 보면 한 사이클은 이렇게 봅니다.

```text
1 move time =
command 전송 시간
+ 실제 축 이동 시간
+ command done 확인 시간
```

Jonas의 의도는 “통신 시간이 0.1ms냐 1ms냐”를 직접 따지는 게 아닙니다. 그 차이가 실제 생산 시간에 의미 있는지 보려면, 실제 생산에 가까운 짧고 빠른 반복 동작을 돌리고 **전체 cycle time**으로 비교해야 한다는 말입니다.

**그가 상정하는 모션 형태**
가장 단순하게 해석하면:

```text
P1 -> P2 -> P1 -> P2 ... 반복
```

또는 조금 더 실제 placing에 가깝게 하면:

```text
Pick 위치 -> Place 위치 -> Pick 위치 -> Place 위치 ... 반복
```

각 이동은 짧고 빠르게 잡아야 합니다. 예를 들어 이동 자체가 500ms면 통신 1ms 차이는 거의 안 보입니다. 반대로 이동이 50~100ms 수준이면 통신/완료확인 방식 차이가 전체 시간에 더 크게 반영됩니다.

**중요 조건**
Jonas가 요구한 조건은 세 가지입니다.

1. Elmo와 SIGMATEK의 축 동특성이 같아야 함
   속도, 가속도, 감속, jerk, 이동거리 등이 같아야 합니다. 실제 이동 시간이 다르면 그 차이를 별도로 알아야 합니다.

2. 테스트 모션이 현실적이어야 함
   그냥 말도 안 되게 짧은 왕복이 아니라, 고객 장비에서 실제로 요구될 만한 빠른 placing 동작이어야 합니다.

3. 모션 외 대기 시간이 있으면 반영해야 함
   예를 들어 진공 해제, 그리퍼 open, part settling delay 같은 게 실제 생산에 있으면 그것도 cycle에 넣어야 합니다.

**우리 현재 작업과 연결하면**
현재 만든 `Cycle Test Group1`의 P1-P2-P3-P4-P1 반복은 “그룹 모션/블렌딩/InPosition 확인” 테스트에 가깝습니다. Jonas 메일의 취지는 그보다 더 생산성 비교에 가까워서, 우선은 아래 같은 테스트가 더 맞습니다.

```text
N cycles:
  Move P1 -> P2
  Wait done
  optional actuator delay
  Move P2 -> P1
  Wait done
  optional actuator delay

Measure:
  total elapsed time
  average cycle time
  min/max cycle time
  command send + done check latency
```

결론: 이 친구가 하고자 하는 모션은 **고객의 실제 pick/place에 가까운 짧고 빠른 반복 왕복 모션**입니다. 목적은 모션 궤적 자체가 아니라, Elmo와 SIGMATEK에서 같은 동작을 많이 반복했을 때 **전체 생산 시간 차이가 의미 있게 나는지** 확인하는 것입니다.

> 그럼 이 친구가 얘기하는 모션을 측정할 수 있는 테스트 기능을 PMAS랑 SIGMATEK 프로그램에 구현해줘

<details><summary>26 previous messages</summary>

> 요청 범위는 PMAS WPF와 SIGMATEK/LASAL 쪽 테스트 앱에 “짧은 왕복 생산성 테스트”를 넣는 것으로 잡겠습니다. LASAL 관련 규칙 문서와 기존 Cycle Test 구조를 먼저 확인해서, 이미 있는 측정/저장 패턴을 재사용하겠습니다.
>
> 기존 `Cycle Test`가 이미 “단축 왕복 + 완료 확인 + 총 시간 저장”에 가깝습니다. 다만 Jonas 메일 기준에는 actuator 대기 시간, warmup 제외 통계, 전체 생산성 비교용 요약이 명시적으로 필요해서 새 탭/로직으로 분리하는 쪽이 안전합니다.
>
> 구현은 기존 Cycle Test를 덮어쓰지 않고 `Production Test` 탭을 새로 추가하겠습니다. 이유는 기존 탭들은 ReadActualPosition/GroupStatus 실험 목적이 섞여 있고, Jonas 요구는 “전체 생산 사이클 시간”이 1차 결과라서 입력/요약/저장 항목이 다릅니다.
>
> 새 로직은 기존 내부 헬퍼(`ParseDouble`, `NormalizeDirectionForAbsoluteMove`, `HighResolutionTimerScope`, Excel exporter)를 그대로 쓰겠습니다. 이렇게 해야 PMAS와 LASAL 앱 양쪽에서 UI/저장 형식만 맞추고, 모션 명령은 각 앱의 기존 `Context.SingleAxis` 구현 차이를 그대로 타게 됩니다.
>
> PMAS는 상태워드 기반 완료 확인 탭이 이미 있지만, LASAL 기존 Cycle Test는 위치 읽기 기준입니다. 새 테스트는 양쪽에서 동일하게 동작해야 하므로 기본 완료 확인은 `ReadActualPosition` 허용오차 방식으로 구현하고, 결과에는 명령 ACK 시간과 완료 확인 read latency를 따로 저장하겠습니다.
>
> 기존 `Cycle Test`를 생산성 테스트로 확장하는 방식으로 구현하겠습니다. 한 사이클을 `P1->P2 완료확인 -> 배치/그리퍼 대기 -> P2->P1 완료확인 -> 복귀 대기`로 보고, 총 소요시간/평균 part time/parts per minute/명령 ACK/완료확인 read latency를 저장하게 만들겠습니다.
>
> 계획을 조정했습니다. 새 탭을 만들기보다 기존 단축 `Cycle Test`를 확장합니다. 이게 Jonas가 말한 “한 축 짧은 반복 모션 + done 확인 + 부가 actuator delay + 총 생산시간 비교”에 정확히 맞고, PMAS/LASAL 양쪽 기존 테스트 경로도 그대로 유지합니다.
>
> 지금부터 파일을 수정합니다. 변경은 기존 Cycle Test에 actuator delay 입력 2개와 생산성 요약 지표를 추가하고, MoveAbsolute 명령 호출 시간도 별도로 집계하는 범위입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs` (+167 -85)
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs` (+86 -2)
>
> </details>
>
> 코드 로직은 양쪽에 반영했습니다. 이제 UI에 actuator delay 입력칸을 추가하고, 저장 결과와 화면 요약에서 그 값이 보이도록 XAML을 맞춥니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+31 -16)
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+31 -16)
>
> </details>
>
> XAML 패치가 한쪽에서 엉뚱한 첫 번째 3-row Grid에 들어간 흔적이 보입니다. 컨트롤 이름 기준으로 정확히 다시 확인해서, 실제 `Cycle Test`의 `Motion Profile` Grid에만 남도록 정리하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+9 -8)
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+9 -8)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+2 -3)
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+2 -3)
>
> </details>
>
> 불필요하게 다른 Grid에 들어간 row definition도 제거했습니다. Cycle Test의 Motion Profile Grid만 4행 구조가 되도록 맞췄습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+5 -5)
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+5 -5)
>
> </details>
>
> WPF 양쪽 구현은 같은 입력/결과 구조로 맞췄습니다. 이제 테스트 의도와 판정 기준을 문서로 남겨서, 나중에 이 탭이 단순 cycle test인지 Jonas식 생산성 비교인지 혼동되지 않게 하겠습니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./docs/architecture/Production_Cycle_Performance_Test_2026-06-25.md` (+85 -0)
>
> </details>
>
> 문서까지 추가했습니다. 이제 라인엔딩을 저장소 규칙에 맞추고, PMAS/LASAL 두 솔루션을 빌드해서 XAML generated field와 C# 컴파일 오류를 확인하겠습니다.
>
> 변경 파일의 줄바꿈 정책은 `.cs/.xaml = CRLF`, `.md = LF`입니다. 지금 `apply_patch` 때문에 작업본이 LF로 남아 있으니 기계적으로 정규화한 뒤 검사합니다.
>
> 라인엔딩 정규화는 끝났습니다. 이제 `git diff --check`와 Visual Studio MSBuild 빌드를 순서대로 돌립니다.
>
> `diff --check`는 통과했고 줄바꿈도 정책대로 맞았습니다. 고정 후보 경로에서는 MSBuild를 못 찾았으니, 설치 위치를 다시 탐색해서 가능한 빌드 도구로 확인하겠습니다.
>
> MSBuild는 VS2019 Professional 경로에 있습니다. 이제 PMAS와 LASAL WPF 솔루션을 각각 Debug 빌드로 확인합니다.
>
> PMAS WPF 빌드는 경고 0, 오류 0으로 통과했습니다. 이어서 LASAL WPF도 같은 방식으로 확인합니다.
>
> LASAL WPF도 경고 0, 오류 0으로 통과했습니다. 마지막으로 빌드 산출물이 Git에 섞이지 않았는지와 diff 상태를 확인합니다.
>
> 검증은 통과했습니다. 변경된 기능 위치와 문서 위치를 라인 번호 기준으로 한 번 더 뽑아서 최종 보고에 정확히 적겠습니다.
>
> 구현/검증이 끝났습니다. PLC ST는 건드리지 않았습니다. 이유는 이 테스트가 기존 단축 `MoveAbsolute`와 `ReadActualPosition` TCP 경로만 사용하고, SIGMATEK 쪽도 그 경로가 이미 구현되어 있기 때문입니다.
</details>

구현했습니다. 기존 단축 `Cycle Test`를 Jonas가 말한 생산성 비교용으로 확장했습니다.

측정 사이클은 이제 이렇게 동작합니다.

```text
Base -> Forward 위치 이동
-> actual position으로 done 확인
-> Forward Actor Delay
-> Base 복귀 이동
-> actual position으로 done 확인
-> Return Actor Delay
```

변경 파일:
- [PMAS Cycle Test 로직](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:45)
- [PMAS Cycle Test UI](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1262)
- [SIGMATEK/LASAL Cycle Test 로직](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:38)
- [SIGMATEK/LASAL Cycle Test UI](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:1162)
- [테스트 기준 문서](/C:/work/Elmo/Elmo_Master/docs/architecture/Production_Cycle_Performance_Test_2026-06-25.md:1)

추가된 입력:
- `Forward Actor Delay (ms)`
- `Return Actor Delay (ms)`

추가된 결과:
- `TotalElapsed(ms)`
- `AveragePartTime(ms)`
- `Throughput(parts/min)`
- `CommandLatencyAvg/Max(ms)`
- `ResponseLatencyAvg/Max(ms)`

검증:
- `git diff --check` 통과
- PMAS WPF Debug 빌드 성공, 경고 0 / 오류 0
- LASAL WPF Debug 빌드 성공, 경고 0 / 오류 0
- 라인엔딩도 `.cs/.xaml = CRLF`, `.md = LF`로 맞춤

`Elmo_EtherCAT_Test_4Axis` PLC/LASAL ST 코드는 건드리지 않았습니다. 이번 기능은 기존 단축 `MoveAbsolute` + `ReadActualPosition` 경로만 사용하므로 PLC 쪽 새 command는 필요 없습니다.

> api 정리문서 확인해서 api리스트가 어떤식으로 차이나는지 알려줘
>
> User context
>
> Attachments:
> - EtherCAT Controller(Master) 요구 사양.xlsx: `./EtherCAT Controller(Master) 요구 사양.xlsx`
> - Motion_Controller_API_정리.xlsx: `./Motion_Controller_API_정리.xlsx`

<details><summary>12 previous messages</summary>

> 스프레드시트 비교 작업이라 `spreadsheets` 스킬 지침을 먼저 확인하고, 두 xlsx의 시트/컬럼 구조를 본 뒤 API 목록 차이를 정리하겠습니다.
>
> 표시된 스킬 경로가 현재 캐시에 없어서 실제 설치된 스킬 파일 위치를 찾아서 읽겠습니다.
>
> 실제 캐시 버전은 `26.630.12135`입니다. 이 지침을 적용해서 진행합니다.
>
> 이번 요청은 새 xlsx를 만들거나 수정하는 게 아니라 기존 두 파일의 API 목록 비교입니다. 워크북을 읽기만 할 것이고, 먼저 번들 런타임/라이브러리 경로를 확인한 뒤 시트 구조와 컬럼을 추출하겠습니다.
>
> 두 워크북의 시트명, 사용 범위, 헤더 후보를 먼저 확인합니다. 그 다음 API명으로 보이는 컬럼을 잡아서 목록 차이를 비교하겠습니다.
>
> PowerShell heredoc 문법이 bash 방식으로 들어가서 실패했습니다. 같은 내용을 PowerShell here-string으로 다시 실행합니다.
>
> 콘솔 기본 인코딩이 일부 문자를 출력하지 못했습니다. UTF-8 출력으로 다시 구조를 뽑겠습니다.
>
> 구조가 확인됐습니다. `EtherCAT Controller(Master) 요구 사양.xlsx`는 `ELMO Controller API` 시트의 1열에 짧은 요구 API 목록이 있고, `Motion_Controller_API_정리.xlsx`는 `API 목록` 시트의 OPUS(C++) 컬럼에 훨씬 큰 API 목록이 있습니다. 이제 함수명을 정규화해서 공통/누락/추가를 계산합니다.
>
> 1차 비교 결과는 의외로 두 문서의 OPUS API 함수명 집합이 완전히 같습니다. 다만 `Motion_Controller_API_정리.xlsx`는 275행짜리 문서라서 OPERA(C#) 컬럼이나 빈/분류 행이 섞여 있을 수 있습니다. 행 구조를 더 확인해서 실제 API 행이 더 있는지 검증합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 두 엑셀 파일의 API 시트 기준으로 비교는 끝났습니다. 핵심은 “OPUS/MMCLib 고유 함수명” 기준인지, “OPERA C#까지 포함한 정리 행” 기준인지에 따라 차이가 다르게 보인다는 점입니다.
>
> 추가로 행 수와 중복까지 재확인했습니다. `Motion_Controller_API_정리.xlsx`의 C# 열은 반환형이 앞에 붙어 있어서, 메서드명만 따로 뽑아 다시 정리합니다.
</details>

확인 결과, **OPUS/MMCLib API 함수명 기준으로는 두 문서의 리스트가 동일합니다.**

비교 기준:
- [EtherCAT Controller(Master) 요구 사양.xlsx](</C:/work/Elmo/Elmo_Master/EtherCAT Controller(Master) 요구 사양.xlsx>)
  - `ELMO Controller API` 시트
  - API 44개, 고유 함수명 44개
- [Motion_Controller_API_정리.xlsx](</C:/work/Elmo/Elmo_Master/Motion_Controller_API_정리.xlsx>)
  - `API 목록` 시트
  - 전체 정리 행 65개
  - OPUS/C++ 함수가 있는 행 48개
  - OPUS 고유 함수명 44개
  - OPERA/C# 전용 행 17개

즉, **요구사양 문서의 44개 ELMO API는 Motion API 정리 문서의 OPUS 열에 전부 들어있고, 빠진 것도 추가된 OPUS API도 없습니다.**

차이는 문서 구조입니다.

| 구분 | EtherCAT 요구 사양 문서 | Motion API 정리 문서 |
|---|---|---|
| 성격 | 요구 API 목록 | OPUS C++ / OPERA C# 통합 매핑표 |
| API 수 | 44개 | OPUS 고유 44개 + C# 전용 행 17개 |
| 정보량 | 함수명 + 간단 정의 | 함수명, 설명, 사용 목적, 비고, 우선순위 |
| 중복 | 없음 | overload/용도 차이 때문에 일부 OPUS 함수 중복 |

중복으로 나뉜 OPUS API는 3종입니다.

| OPUS API | Motion 문서에서 나뉜 방식 |
|---|---|
| `MMC_PowerCmd` | Power On / Power Off 용도로 분리 |
| `MMC_MoveLinearAbsoluteExCmd` | 파라미터 형태가 다른 C# overload로 분리 |
| `MMC_SendSdoCmd` | SDO download 타입별 overload로 3개 분리 |

그리고 Motion 문서에는 요구사양 문서에 없는 **OPERA/C# 전용 API 행**이 있습니다. 대표적으로:

- `GetEntry`, `AddEntry`, `Upload`, `Init`: PI/BulkRead 보조 API
- `RegisterEmergencyEventCallback`: Emergency Event callback 등록
- `ReadGroupParameters`, `WriteGroupParameters`: Group parameter 일괄 처리
- `HomeDS402`, `GetOpMode`: DS402 / Operation Mode 관련 C# API
- `EnableProfileConditioning`, `SetProfileConditioningMode`: Profile Conditioning
- `MoveLinearRelativeEx`: Group 상대 이동 C# API
- `UploadSDO`, `UploadSdoEx`: SDO upload 계열
- `GroupReadActualPosition`: Group actual position 읽기

정리하면, **기본 이식 대상 OPUS/MMCLib API 목록은 두 문서가 같고**, `Motion_Controller_API_정리.xlsx`는 거기에 **C# OPERA wrapper, overload, helper API, 우선순위/용도 설명을 붙인 확장 정리표**입니다.

> WireShark 분석된 내용을 토대로 API를 만들었어. 이걸 이용해서 lasal 에서 동작될 수 있는 소스코드를 구현해야돼. 이 내용에 대해서 구현 방안에 대해 설계해보자.
>
> User context
>
> Attachments:
> - WireShark: `./LMC_Library/LMC_API/Elmo_API_Packet2/WireShark/`

<details><summary>11 previous messages</summary>

> 먼저 실제 `WireShark` 폴더와 현재 LASAL 쪽 TCP/모션 인터페이스 구조를 같이 확인하겠습니다. 히스토리와 현재 소스를 분리해서 봐야 설계가 헛돌지 않습니다.
>
> 폴더 안에는 분석 문서가 아니라 API별 `.pcapng` 캡처가 직접 들어 있습니다. 그래서 설계는 현재 구현된 C# API/더미 프레임과 LASAL `TCPMotionInterface`를 대조해서, 캡처 기반 프로토콜 계층을 어디에 둘지 정하는 방식으로 잡겠습니다.
>
> 현재 `TCPMotionInterface.st`는 CodeGenerator 파일이고, 실제 수정 가능 영역은 아래 구현부입니다. 여기 구조가 이미 “TCP 수신 -> 파싱 -> 축/로봇 호출 -> 응답”을 한 클래스에 꽤 많이 갖고 있어서, 새 API는 무작정 붙이면 금방 유지보수 불가능해집니다.
>
> `LMC_Library/LMC_API/LMC_API/docs`에 이미 패킷 맵과 API 목록이 있습니다. 다음은 이 문서를 기준으로 “LASAL에서 바로 구현 가능한 API 계층”과 “아직 축/그룹 연결 검증이 필요한 계층”을 나누겠습니다.
>
> 확인된 전달 API는 PMAS/MMCLib을 그대로 포팅한 게 아니라, 캡처된 Maestro TCP 패킷을 `LMC_` API로 감싼 구조입니다. LASAL 구현도 같은 방향, 즉 “Elmo API 흉내”가 아니라 “패킷 command를 SIGMATEK 모션 호출로 변환하는 서버”로 설계하는 게 맞습니다.
>
> 현재 LASAL 구현은 이미 일부 command ID를 처리하지만, 새 LMC 패킷 맵과 안 맞는 부분이 있습니다. 예를 들어 `PowerCmd`는 새 맵에서 `0x2023`인데 현재 구현은 `0x2081/0x2082` 계열도 보고 있어서, 이전 더미 프로토콜과 새 캡처 기반 프로토콜을 분리해야 합니다.
>
> 중요한 설계 판단 하나: 기존 `Codex_LASAL_WPF`의 더미 MMCLib은 과거 테스트용 command id를 일부 쓰고, 새 `LMC_API` 문서는 캡처 기반 command id를 씁니다. 앞으로 기준은 `LMC_PACKET_MAP.md`가 되어야 하고, 기존 더미 프로토콜은 호환 레이어로만 남기는 게 맞습니다.
>
> 설계 관점에서 가장 위험한 부분은 Group입니다. `MoveLinearAbsoluteEx`는 패킷 구조가 비교적 명확하지만, LASAL 쪽 `LMCRobot` 연결/상태 전환/GroupDisable 후 단축 복귀가 실제 장비 상태와 맞아야 하므로 API 이름만 맞춰 성공 응답을 주면 안 됩니다.
>
> 또 하나 확인된 현재 제약입니다. `TCPMotionInterface`에는 `LMCAxis` 클라이언트가 하나뿐이라서, 새 API의 `a01~a04` 멤버 Power/상태/단축 이동을 제대로 처리하려면 4축 라우팅 구조가 필요합니다. 단일 축만 되는 상태에서 그룹 API를 “성공” 처리하면 실제 테스트에서 바로 깨질 가능성이 큽니다.
>
> 네트워크 테이블에는 `_LMCAxis1~4`와 `LMCRobot` 연결 흔적이 있습니다. 다만 `TCPMotionInterface` 클래스 선언은 단일 `LMCAxis` 클라이언트만 받도록 되어 있어서, 구현 설계에는 “축 이름/AxisRef → 실제 축 클라이언트” 매핑 추가가 포함돼야 합니다.
>
> 캡처 자체도 빠르게 훑어서 요청/응답 길이를 확인하겠습니다. 설계에서 중요한 건 ACK가 아니라 실제 TCP payload 길이와 첫 8바이트 header입니다.
</details>

확인한 기준으로 설계 방향은 명확합니다. LASAL에는 “LMC API DLL”을 이식하는 게 아니라, **LMC API가 보내는 캡처 기반 TCP 패킷을 받아서 SIGMATEK `_LMCAxis` / `LMCRobot` 호출로 변환하는 서버 계층**을 구현해야 합니다.

**확인된 사실**
- 패킷 기준 문서는 [LMC_PACKET_MAP.md](</C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API/LMC_API/docs/LMC_PACKET_MAP.md:1>)입니다.
- 실제 캡처는 [WireShark](</C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API/Elmo_API_Packet2/WireShark>)에 있고, TCP payload는 API별로 9, 16, 20, 24, 49, 64, 88, 312, 1328 bytes 등으로 나뉩니다.
- 현재 LASAL 진입점은 [TCPMotionInterface.st](</C:/work/Elmo/Elmo_Master/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:416>)입니다.
- 현재 구현은 일부 과거 더미 command ID를 사용합니다. 예: Power On/Off `0x2081/0x2082`. 새 LMC 캡처 기준은 `LMC_PowerCmd = 0x2023`입니다.

**핵심 구조**
```text
LMC_API.dll / test-app
        |
        | TCP packet
        v
TCPMotionInterface.Response
        |
        v
LMC frame parser
        |
        v
bounded command queue
        |
        v
RtWork dispatch
        |
        +--> Axis adapter: _LMCAxis1..4
        |
        +--> Group adapter: LMCRobot
        |
        v
LMC response builder -> SendData()
```

**먼저 고쳐야 할 구조 문제**
1. Header 파싱을 바꿔야 합니다.
   새 패킷 header는 사실상 `CommandId UINT`, `AxisRef UINT`, `PayloadLength UINT`, `GroupRef/Flags UINT`입니다. 현재 코드는 offset 4에서 4바이트를 `Payload`로 복사하는데, group command에서는 payload length와 group flag가 섞입니다.

2. Motion parameter 타입을 바꿔야 합니다.
   `0x209F`, `0x20A0`, `0x20A2`, `0x20A4`는 8바이트 값이 들어옵니다. 현재 `pos`, `velo`, `acc`, `dec`, `jer`가 `DINT`인데 여기에 8바이트 `MemCpy`를 하는 구조라 위험합니다. `LREAL` 또는 64-bit 별도 버퍼로 받아야 합니다.

3. `Response()`에서 모션을 직접 실행하지 않아야 합니다.
   `Response()`는 복사, 길이 검증, command queue 적재까지만 하고, 실제 `_LMCAxis` / `LMCRobot` 호출과 응답 송신은 `RtWork`에서 처리하는 구조가 맞습니다.

4. 축 라우팅이 필요합니다.
   현재 `TCPMotionInterface`는 `LMCAxis` client 하나만 받습니다. 그런데 LMC API는 `a01~a04`와 group `v01` 흐름을 씁니다. 따라서 `a01/a02/a03/a04 -> _LMCAxis1..4` 매핑이 필요합니다. 네트워크 테이블에는 `_LMCAxis1~4`와 `LMCRobot` 연결 흔적이 이미 있습니다.

5. `0x202B`도 지원해야 합니다.
   문서 API 목록에는 빠져 있지만 캡처상 `LMCAxis` 생성 과정에서 `0x103C` axis name lookup 뒤 `0x202B`가 호출됩니다. 이걸 ACK 처리하지 않으면 C# LMC API 생성자 단계에서 막힐 수 있습니다.

**구현 범위 설계**
1차 구현은 단축 API부터 가는 게 맞습니다.

| 단계 | API |
|---|---|
| 연결/lookup | `0x8080`, `0x405C`, `0x405D`, `0x103C`, `0x1042`, `0x202B` |
| 단축 기본 | `0x2023 Power`, `0x2024 Reset`, `0x2022 Stop`, `0x2028 Status`, `0x202E ActualPosition` |
| 단축 모션 | `0x209F MoveAbsolute`, `0x20A0 MoveRelative`, `0x20A2 MoveVelocity` |
| 그룹 기본 | `0x20D2`, `0x2047`, `0x2048`, `0x2049`, `0x2085`, `0x2045`, `0x2051` |
| 그룹 모션 | `0x20E7 SetKinTransform`, `0x20A4 MoveLinearAbsoluteEx` |

**추천 구현 순서**
1. `TCPMotionInterface` 안에 `LMC_` command constant, header parser, response builder를 먼저 만든다.
2. `Response()`는 `ReceiveBuf` 복사와 frame 검증만 하게 줄인다.
3. `RtWork()`에서 pending command를 하나씩 dispatch한다.
4. `0x103C`, `0x1042`, `0x202B`, `0x202E`, `0x2028`처럼 모션 위험이 낮은 read/lookup 계열부터 맞춘다.
5. 그 다음 `Power/Reset/Stop`.
6. 마지막에 `MoveAbsolute/Relative/Velocity`.
7. 단축이 안정된 뒤 group `v01` / `LMCRobot` 쪽을 붙인다.

**주의할 단위 문제**
LMC 문서는 위치/속도 값을 controller count 기준으로 설명합니다. 현재 LASAL 네트워크 초기값에는 `_LMCAxis`가 `ExUnits=8388608`, `IntUnits=360 deg`로 잡혀 있습니다. 그래서 구현 전 결정이 필요합니다.

- LMC API 입력 count를 그대로 `_LMCAxis`에 넣을지
- `count * 360 / 8388608`로 degree 변환해서 넣을지

이건 실제 `_LMCAxis.MoveAbsolute()`가 기대하는 단위를 확인한 뒤 하나로 고정해야 합니다. 여기서 틀리면 1 rev 명령이 360도인지 8,388,608도인지가 갈립니다.

**결론**
현재 설계는 기존 `TCPMotionInterface`를 완전히 버리는 게 아니라, 그 안을 **LMC protocol server**로 재정리하는 방향이 맞습니다. 다만 기존 command ID, DINT 기반 8바이트 복사, 단일 `LMCAxis` 라우팅은 그대로 두면 안 됩니다. 다음 작업은 이 설계를 문서로 고정하고, 단축 lookup/read/status부터 Phase 1 구현에 들어가는 것입니다.

> 지금 폴더를 기능별로 다 재배치했으니 폴더안에 있는 파일들을 다 재 스캔해서 종류별로 정리해서 Git에 올려

<details><summary>24 previous messages</summary>

> 먼저 현재 Git 상태, ignore 규칙, 새로 재배치된 파일 종류를 다시 스캔하겠습니다. `pcapng` 같은 캡처 파일은 이 저장소 규칙상 기본 무시 대상이라, 실제로 올릴지 여부를 상태로 확인하고 판단하겠습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `rg -n "Elmo_Master|LMC_API|pcapng|Git|ignore|capture|WireShark" C:\Users\dreiv\.codex\memories\MEMORY.md`
> - Ran `git status --short --untracked-files=all`
> - Ran `git branch --show-current`
> - Ran `git remote -v`
>
> </details>
>
> 상태가 매우 큽니다. 기존 파일 삭제와 새 기능별 폴더 추가가 섞여 있고, `Reports`, `test`, `Lasal_PRG`, `LMC_Library`, `docs` 쪽으로 재배치된 것으로 보입니다. 이제 전체 변경을 기계적으로 분류해서 무엇을 Git에 올릴지 확정하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$env:PYTHONIOENCODING='utf-8'
> @'
> import subprocess, os
> from collections import Counter, defaultdict
> raw = subprocess.check_output(['git','status','--porcelain=v1','-z','--untracked-files=all'])
> items = raw.decode('utf-8', errors='replace').split('\0')
> records=[]
> i=0
> while i < len(items):
>     item=items[i]
>     i+=1
>     if not item:
>         continue
>     status=item[:2]
>     path=item[3:]
>     if status[0] in 'RC' or status[1] in 'RC':
>         if i < len(items):
>             path2=items[i]; i+=1
>             records.append((status, path, path2))
>         else:
>             records.append((status, path, None))
>     else:
>         records.append((status, path, None))
>
> def ext(path):
>     base=os.path.basename(path)
>     if '.' not in base:
>         return '[no ext]'
>     return os.path.splitext(base)[1].lower()
>
> def top(path):
>     return path.split('/')[0]
>
> print('TOTAL_RECORDS', len(records))
> print('\nBY_STATUS')
> for k,v in Counter(s for s,_,__ in records).most_common():
>     print(f'{k!r} {v}')
> print('\nBY_TOP')
> for k,v in Counter(top(p2 or p) for s,p,p2 in records).most_common(40):
>     print(f'{k} {v}')
> print('\nBY_EXTENSION')
> for k,v in Counter(ext(p2 or p) for s,p,p2 in records).most_common(60):
>     print(f'{k} {v}')
> print('\nUNTRACKED_TOP')
> for k,v in Counter(top(p) for s,p,p2 in records if s == '??').most_common(40):
>     print(f'{k} {v}')
> print('\nDELETED_TOP')
> for k,v in Counter(top(p) for s,p,p2 in records if 'D' in s).most_common(40):
>     print(f'{k} {v}')
> '@ | python -`
>
> </details>
>
> 스캔 결과는 총 1,809건입니다. 핵심은 `Elmo_EtherCAT_Test_4Axis` 쪽 기존 tracked 파일이 삭제되고, `Lasal_PRG/...` 아래로 1,004개가 새로 들어온 재배치입니다. 문서/테스트/캡처도 `docs`, `test`, `LMC_Library`로 정리되어 있습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `git ls-files --deleted | Measure-Object | Select-Object -ExpandProperty Count`
> - Ran `Get-Content -LiteralPath '.gitignore'`
> - Ran `git ls-files --others --exclude-standard | Measure-Object | Select-Object -ExpandProperty Count`
> - Ran `git status --ignored --short --untracked-files=all | Select-Object -First 220`
>
> </details>
>
> `.gitignore`는 `*.pcapng`, `*.xlsx`, `*.lba`, `*.lob` 같은 생성물/캡처를 기본 제외합니다. 현재 status에 보이는 삭제는 “이미 Git에 추적되던 파일”이라 삭제로 올라가고, 새로 추가할 파일은 ignore 규칙을 따른 1,124개입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$env:PYTHONIOENCODING='utf-8'
> @'
> import subprocess, os
> from collections import Counter
> raw = subprocess.check_output(['git','status','--porcelain=v1','-z','--untracked-files=all'])
> parts=raw.decode('utf-8', errors='replace').split('\0')
> recs=[]; i=0
> while i<len(parts):
>     item=parts[i]; i+=1
>     if not item: continue
>     s=item[:2]; p=item[3:]
>     recs.append((s,p))
>
> def ext(p):
>     b=os.path.basename(p)
>     return os.path.splitext(b)[1].lower() if '.' in b else '[no ext]'
> for status in ['??',' D',' M']:
>     print('STATUS', repr(status), 'COUNT', sum(1 for s,p in recs if s==status))
>     for k,v in Counter(ext(p) for s,p in recs if s==status).most_common(30):
>         print(f'  {k} {v}')
> '@ | python -`
>
> </details>
>
> 새 `WireShark` 캡처 원본 `.pcapng`들은 ignore 때문에 일반 `git add -A`로는 안 올라갑니다. 대신 같은 폴더의 TXT 분석본과 `LMC_PACKET_MAP.md` 같은 문서는 올라갑니다. 원본 캡처까지 저장소에 넣을지 여부가 설계상 갈리는 지점입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$env:PYTHONIOENCODING='utf-8'
> @'
> import subprocess, os
> from collections import Counter
> try:
>     out = subprocess.check_output(['git','ls-files','--others','--ignored','--exclude-standard'], text=True, encoding='utf-8', errors='replace')
> except subprocess.CalledProcessError as e:
>     out = e.output
> paths=[p for p in out.splitlines() if p]
> print('IGNORED_UNTRACKED_COUNT', len(paths))
> print('BY_TOP')
> for k,v in Counter(p.split('/')[0] for p in paths).most_common(30):
>     print(k, v)
> print('BY_EXT')
> for k,v in Counter(os.path.splitext(os.path.basename(p))[1].lower() if '.' in os.path.basename(p) else '[no ext]' for p in paths).most_common(30):
>     print(k, v)
> print('SAMPLE')
> for p in paths[:80]:
>     print(p)
> '@ | python -`
>
> </details>
>
> 원본 캡처까지 포함하려면 `-f`로 특정 경로만 강제 추가해야 합니다. 전체 ignore 파일을 강제 추가하면 `bin/obj/.vs`와 LASAL 생성물이 대량으로 들어가므로, 강제 추가는 `LMC_Library/.../WireShark/*.pcapng` 같은 의도된 캡처 원본으로만 제한해야 합니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `git ls-files --others --ignored --exclude-standard -- 'LMC_Library/LMC_API/Elmo_API_Packet2/WireShark'`
> - Ran `git ls-files --others --ignored --exclude-standard -- 'LMC_Library'`
> - Ran `git ls-files --others --exclude-standard -- 'LMC_Library'`
> - Ran `git ls-files --others --ignored --exclude-standard -- '*.pcapng'`
>
> </details>
>
> 재배치된 캡처/리포트 원본도 보존하려면 ignored `.pcapng` 44개와 `.xlsx` 일부를 강제 추가해야 합니다. 사용자가 “다 올려”라고 했기 때문에, `bin/obj/.vs` 같은 빌드 산출물은 계속 제외하고, 재배치된 실험 원본(`test/**`, `LMC_Library/**/WireShark/**`)만 선별 강제 추가하는 방향으로 진행하겠습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `git ls-files --others --ignored --exclude-standard -- 'test/**/*.xlsx' 'test/**/*.pcapng' 'test/**/*.pdf' 'test/**/*.zip'`
> - Ran `git ls-files --others --ignored --exclude-standard -- 'LMC_Library/LMC_API/Elmo_API_Packet2/WireShark/*.pcapng' 'LMC_Library/LMC_API/LMC_API/bin/*' 'LMC_Library/*.zip'`
> - Ran `git ls-files --others --ignored --exclude-standard -- 'Lasal_PRG/**/*.lba' 'Lasal_PRG/**/*.lob' 'Lasal_PRG/**/*.lhd' 'Lasal_PRG/**/ProjectInternal/**' | Measure-Object | Select-Object -ExpandProperty Count`
>
> </details>
>
> 작업 브랜치를 새로 만들고, 분류 스캔 결과를 문서로 남긴 뒤 staging 하겠습니다. 강제 추가는 재배치된 실험 원본과 LMC 전달 DLL에만 제한하고, 빌드 산출물/IDE 캐시/중복 zip은 제외합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git switch -c codex/reorganize-functional-folders-20260708`
>
> </details>
>
> 브랜치는 `codex/reorganize-functional-folders-20260708`로 분리했습니다. 이제 재스캔 결과와 포함/제외 기준을 repo 문서에 짧게 남깁니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./docs/reorganization_file_inventory_2026-07-08.md` (+41 -0)
>
> </details>
>
> 이제 전체 재배치 결과를 staging 합니다. 기본 `git add -A` 후, ignore 규칙 때문에 빠지는 캡처/리포트 원본은 지정된 경로만 `-f`로 추가합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git add -A -- .
> git add -f -- ':(glob)LMC_Library/LMC_API/Elmo_API_Packet2/WireShark/*.pcapng' 'LMC_Library/LMC_API/LMC_API/bin/LmcMotionApi.dll' ':(glob)test/**/*.pcapng' ':(glob)test/**/*.xlsx'`
>
> </details>
>
> Staging은 완료됐고 줄바꿈 경고가 많이 나왔습니다. 새로 추가된 텍스트류는 Git 설정상 LF/CRLF 정규화 경고가 뜬 것이고, 실패는 아닙니다. 이제 staged 상태와 남은 untracked/ignored를 확인합니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `git status --short --untracked-files=all | Select-Object -First 260`
> - Ran `git diff --cached --stat`
> - Ran `$env:PYTHONIOENCODING='utf-8'
> @'
> import subprocess, os
> from collections import Counter
> raw = subprocess.check_output(['git','diff','--cached','--name-status','-z'])
> parts=raw.decode('utf-8', errors='replace').split('\0')
> recs=[]; i=0
> while i < len(parts):
>     s=parts[i]; i+=1
>     if not s: continue
>     if s.startswith('R') or s.startswith('C'):
>         old=parts[i]; new=parts[i+1]; i+=2
>         recs.append((s, new))
>     else:
>         p=parts[i]; i+=1
>         recs.append((s,p))
> print('STAGED_RECORDS', len(recs))
> print('BY_STATUS')
> for k,v in Counter(s.split('\t')[0] for s,p in recs).most_common(40): print(k,v)
> print('BY_TOP')
> for k,v in Counter(p.split('/')[0] for s,p in recs).most_common(40): print(k,v)
> print('BY_EXT')
> for k,v in Counter(os.path.splitext(os.path.basename(p))[1].lower() if '.' in os.path.basename(p) else '[no ext]' for s,p in recs).most_common(60): print(k,v)
> '@ | python -`
>
> </details>
>
> Staged 결과는 1,484개입니다. Git이 427개를 rename으로 인식했고, 새 기능별 위치 추가와 기존 위치 삭제가 같이 잡혔습니다. 커밋 전에 `git diff --check`로 공백/라인 문제를 확인합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git diff --cached --check`
> - Ran `$env:PYTHONIOENCODING='utf-8'
> @'
> import subprocess
> raw = subprocess.check_output(['git','status','--porcelain=v1','-z','--untracked-files=all'])
> parts=raw.decode('utf-8', errors='replace').split('\0')
> unstaged=[]; untracked=[]; staged=[]
> i=0
> while i < len(parts):
>     item=parts[i]; i+=1
>     if not item: continue
>     xy=item[:2]; path=item[3:]
>     if xy[0] != ' ' and xy != '??': staged.append((xy,path))
>     if xy[1] != ' ' and xy != '??': unstaged.append((xy,path))
>     if xy == '??': untracked.append(path)
> print('staged_entries', len(staged))
> print('unstaged_entries', len(unstaged))
> print('untracked_entries', len(untracked))
> if unstaged:
>     print('UNSTAGED_SAMPLE')
>     for xy,p in unstaged[:20]: print(xy,p)
> if untracked:
>     print('UNTRACKED_SAMPLE')
>     for p in untracked[:20]: print(p)
> '@ | python -`
>
> </details>
</details>

`git diff --check`가 실패했습니다. 원인은 새로 들어온 LASAL 생성 `.st` 파일들의 기존 trailing whitespace / space-before-tab와 TXT 분석본의 EOF 빈 줄입니다. 내용 변경이 아니라 공백 정규화라서, text 파일만 대상으로 후행 공백과 EOF 빈 줄을 정리한 뒤 다시 검사하겠습니다.

> 지금 생각하고있냐?

> 지금 폴더를 기능별로 다 재배치했으니 폴더안에 있는 파일들을 다 재 스캔해서 종류별로 정리해서 Git에 올려
>
> 위에 하던거 이어서 계속 진행해
