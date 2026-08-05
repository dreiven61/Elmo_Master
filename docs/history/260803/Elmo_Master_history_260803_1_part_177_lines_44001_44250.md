> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -0)
>
> </details>
>
> 번역 범위를 넓히고 있습니다. 단순 화면 제목뿐 아니라 실제 조작과 직결되는 Power/Reset/Stop/SDO 상태 버튼, 안전 배너, 긴 ToolTip, 표 헤더까지 한국어 catalog 누락 검사를 걸고 있습니다. 반면 IP·수치 입력, raw DINT, RPC 로그와 증거 payload는 언어 전환 때문에 의미가 바뀌지 않도록 그대로 유지합니다.
>
> 정적 화면 문자열 377개는 현재 catalog 누락 0까지 채웠습니다. 첫 컴파일에서 기능 로직 문제가 아니라, 기존 저장 창에 이미 있던 `Title` 속성과 제가 추가한 지역화 `Title`이 중복된 4곳이 확인됐습니다. 중복만 정리한 뒤 다시 빌드하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+2 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+4 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/UiLocalizationTests.cs` (+12 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs` (+7 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs` (+28 -0)
>
> </details>
>
> 언어 전용 테스트는 Debug에서 7/7 통과했습니다. 이제 같은 임시 출력 방식으로 전체 WPF smoke를 돌려서 언어 전환이 기존 Motion/Power/SDO/복구 기능 테스트에 영향을 주지 않는지 확인합니다.
>
> 전체 Debug smoke는 장시간 process/recovery 시나리오 구간을 실행 중이며 아직 실패 출력은 없습니다. 이전 120초 제한 문제를 피하려고 이번에는 충분한 시간을 주었고, 테스트가 만든 프로세스만 추적하고 있습니다.
>
> Debug 전체 회귀는 296/296 통과했습니다. 실행 중인 기존 앱 PID 38872도 그대로 유지됐습니다. 같은 소스를 Release에서도 전체 재빌드·회귀한 뒤, 번역 누락/공백·문서 상태를 최종 점검하겠습니다.
>
> Release 전체 회귀도 장기 process 시나리오를 계속 실행 중입니다. 현재까지 별도 실패 출력은 없으며, 완료 후에는 정적 XAML 377개가 실제 catalog에서 모두 처리되는지도 다시 독립 대조하겠습니다.
>
> 리뷰에서 실제 회귀 가능성 하나를 찾았습니다. CREVIS 상단 요약 TextBlock은 다른 상태 Text에 바인딩되어 있는데, 현재 번역 서비스가 값을 직접 써서 한국어 전환 시 그 Binding을 끊을 수 있습니다. 바인딩된 속성은 건드리지 않도록 수정하고, 한국어 전환 뒤에도 후속 상태 갱신이 계속 전달되는 회귀 테스트를 추가하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/UiLocalizationTests.cs` (+39 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/UiLocalizationTests.cs` (+23 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 언어 선택 기능의 뼈대와 정적 화면 번역은 이미 반영됐습니다. 남아 있는 Power/Reset/Stop/Group 복구 상태의 동적 버튼·안전 안내까지 한글화하고, 바인딩 보존 회귀 테스트와 Debug/Release 전체 테스트를 다시 통과시키겠습니다. 현재 실행 중인 예제 프로그램은 종료하지 않고 임시 출력 폴더에서 검증합니다.
>
> 정적 탭·라벨뿐 아니라 실제 운전 중 상태에 따라 바뀌는 버튼 문구도 언어팩 대상이어야 합니다. 지금 확인된 누락은 Power Off 안전 인수, Reset/Stop 재시도, Group 상태 관찰·Lock/Unlock 복구 버튼들입니다. 이 문구들과 해당 안전 복구 안내를 같은 카탈로그에 묶어 회귀 테스트 대상으로 추가하고 있습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs` (+116 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs` (+107 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/UiLocalizationTests.cs` (+73 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+6 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+6 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs` (+51 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs` (+27 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs` (+18 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/UiLocalizationTests.cs` (+94 -0)
>
> </details>
>
> Debug 전체 회귀 테스트가 진행 중입니다. 언어팩 집중 테스트는 8/8 통과했고, 전체 테스트는 UI/복구 시나리오까지 포함해 시간이 더 걸리는 정상 구간입니다. 실행 중인 사용자 프로그램과는 분리된 임시 산출물을 사용하고 있습니다.
>
> Debug 전체 297개 중 296개는 통과했고, 1개가 언어팩이 아닌 기존 Fake RPC 테스트의 동시 접근(`List` 열거 중 수정)에서 실패했습니다. 기능 실패가 아니라 테스트 자체의 타이밍 경쟁으로 보이므로 같은 테스트를 단독 재실행해 재현 여부를 확인한 뒤, 전체 suite를 다시 검증하겠습니다.
>
> 단독 재실행에서는 해당 기존 AxisReset 테스트가 1/1 통과해 일시적 test-harness race임을 확인했습니다. 같은 Debug 산출물로 297개 전체를 재실행 중이며, 이 결과가 깨끗해야 Release 검증으로 넘어갑니다.
>
> Debug 전체 재실행은 아직 진행 중이며 중간 중단이나 새 실패 출력은 없습니다. UI 통합/프로세스 종료 복구 테스트가 순차 실행되는 구간이라 약 2분 정도 소요됩니다.
>
> 재검토에서 실제 사용 중 한 가지 문제가 더 확인됐습니다. 한국어 모드에서 SDO 입력값을 편집하면 해당 이벤트가 버튼 문구를 영어로 직접 다시 써서, 다음 UI 갱신 전까지 영어로 보였습니다. 이 경로는 즉시 선택 언어로 변환하도록 고치고, SDO/Recorder/Bulk의 동적 안전 툴팁도 함께 테스트하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs` (+31 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/UiLocalizationTests.cs` (+30 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/UiLocalizationTests.cs` (+6 -1)
>
> </details>
>
> SDO 편집 시 한글 버튼이 영어로 되돌아가던 경로를 고쳤고, 해당 실제 이벤트를 재현하는 테스트도 통과했습니다. 이제 그 수정까지 포함한 Debug 297개 전체 회귀를 다시 실행 중입니다.
>
> 최종 Debug 전체 실행은 계속 정상 진행 중이며 새 실패 출력은 없습니다. 완료되면 같은 소스를 Release로 재빌드해 동일한 297개를 한 번 더 확인합니다.
>
> 최종 Debug는 297/297 통과했습니다. 지금 Release 재빌드와 동일한 297개 전체 회귀를 실행 중입니다. 이 검증이 끝나면 whitespace, stage 여부, 실행 중인 사용자 프로그램 보존 상태까지 확인합니다.
>
> Release 전체 suite도 중간 오류 없이 진행 중입니다. 사용자 프로그램은 계속 실행 상태를 유지하고 있으며, 검증 프로세스는 별도 임시 폴더만 사용합니다.
>
> Release도 297/297 통과했습니다. 현재 예제 프로그램 프로세스가 종료된 상태라 파일 잠금이 없으므로, 사용자가 바로 실행할 수 있게 기본 `bin\Debug` 산출물도 새 소스로 다시 빌드하겠습니다. 소스 외 Git 추적 파일은 추가로 건드리지 않습니다.
>
> 최종 재검토에서 한국어 SDO 문구를 직접 대입한 뒤 English로 전환할 때 원문 추적이 꼬일 수 있는 역방향 문제가 발견됐습니다. 버튼에는 항상 English 원문을 먼저 기록하고 현재 언어를 적용하도록 바꾸며, `한국어 → 입력 편집 → English` 왕복까지 테스트에 고정하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/UiLocalizationTests.cs` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+4 -1)
>
> </details>
>
> `한국어 → SDO 입력 편집 → English` 왕복 테스트가 8/8 통과했습니다. 그 최종 코드로 Debug 297개 전체 회귀를 다시 확인 중입니다.
>
> 최종 왕복 수정까지 포함한 Debug 전체 테스트가 다시 297/297 통과했습니다. 마지막으로 Release 297개를 실행 중이며, 이후 기본 Debug 실행 파일을 한 번 더 갱신합니다.
>
> Release 전체 회귀도 계속 정상 진행 중입니다. 지금까지 언어팩 관련 8개와 기존 Motion/Power/SDO/복구 시나리오에서 새 실패는 없습니다.
</details>

언어팩 적용을 완료했습니다.

- 화면 우측 상단 `Language`에서 `English` / `한국어` 즉시 전환
- 선택 언어는 재실행 후에도 유지
- 정적 화면, Motion/Power/Reset/Stop/Group/SDO Write 동적 버튼, 안전 안내, 팝업 및 저장 창 번역
- SDO 입력 편집 후에도 한글 유지 및 English 복귀 정상
- Raw 로그, 증거 데이터, 입력값, 프로토콜 토큰은 원문 유지
- Windows 기본 `Yes/No/Cancel` 버튼은 Windows 표시 언어를 따름

주요 파일:

- [MainWindow.xaml](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:64)
- [UiLocalization.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/UiLocalization.cs:802)
- [UiLocalizationTests.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/UiLocalizationTests.cs:194)
- [README.md](./LMC_Library/LasalApiWpfTestApp/README.md:20)

검증 결과:

- Debug: 297/297 통과
- Release: 297/297 통과
- `git diff --check`: 통과
- 스테이징 및 커밋: 하지 않음
- 기본 Debug 실행 파일도 최신 코드로 재빌드 완료

실행 파일: [LasalMotionControlApiExample.exe](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Debug/LasalMotionControlApiExample.exe)

<details><summary>22 previous messages</summary>

> 현재 작업 트리와 개발 계획 문서를 다시 대조해 다음 미완료 항목을 선정하겠습니다. 기존의 대규모 미커밋 변경은 보존하고, 설계 문서에 명시됐지만 소스·테스트 증거가 부족한 항목 중 독립적으로 완결 가능한 다음 우선순위를 구현하겠습니다.
>
> 문서상 현재 P0/P1의 코드 구현은 대부분 끝났고, 가장 큰 미완료는 최신 PLC 다운로드와 실기 qualification입니다. 물리 장비 없이 활성 명령을 임의 실행할 수는 없으므로, 동시에 현재 소스에서 다음 코드 슬라이스로 적합한 미구현 API를 감사 중입니다. `HomeDS402Ex`와 `SetOpMode`는 소유권·배선 결정 전 구현 금지 조건이 있어 그대로 열지 않겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 PLC 다운로드 없이 확실히 진행할 수 있는 다음 항목으로, 예제/API 회귀 테스트에서 실제로 관찰된 동시 접근 불안정성을 먼저 제거하겠습니다. 병렬 감사 결과도 받아 우선순위가 설계문서와 맞는지 확인한 뒤 구현·전체 테스트·문서 동기화까지 진행하겠습니다.
>
> 설계 감사 결과도 같은 결론입니다. SetPosition durable recovery가 다음 기능 후보지만, 새 복구 로직의 “1회만 전송/no replay” 결과 자체를 믿으려면 공용 fake-RPC의 동시 열거 race를 먼저 고쳐야 합니다. 지금은 제품 동작을 바꾸지 않고 테스트 관측 계층만 스냅샷 방식으로 고정합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/FakeRpcServer.cs` (+33 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+41 -0)
>
> </details>
>
> 공용 서버는 요청 본문과 세션 번호를 같은 잠금에서 기록하고, 읽는 쪽에는 변경되지 않는 배열 스냅샷만 반환하도록 수정했습니다. 회귀시험은 연결 전 잡아 둔 스냅샷이 이후 Init/Callback/Close 요청으로 변하지 않는지, 최신 두 스냅샷의 개수가 항상 짝을 이루는지 결정적으로 검사합니다. 이제 SDK Debug 전체 회귀부터 실행합니다.
>
> SDK Debug는 새 회귀를 포함해 `1042/1042` 통과했습니다. 기존 1041건에서 스냅샷 계약 1건이 추가된 수치입니다. 같은 소스가 WPF smoke에도 링크되어 있으므로 Release SDK와 WPF Debug/Release를 순차 검증합니다.
>
> WPF Debug 전체 회귀가 실행 중입니다. 이 묶음은 실제 UI 스레드·복구 저널·fake-RPC를 함께 돌려 시간이 더 걸리며, 현재까지 프로세스가 정상 진행 중이고 중간 실패 출력은 없습니다.
>
> 같은 WPF Debug 회귀가 계속 진행 중입니다. 테스트 실행기는 완료 때 결과를 한꺼번에 내보내는 방식이라 아직 개별 PASS 수치는 보이지 않지만, 제한시간 내 정상 실행 상태입니다.
>
> WPF Debug는 `297/297` 통과했습니다. 앞서 드물게 깨졌던 Power On 복구 테스트도 이번 전체 실행에서 정상이며, 이제 Release 전체 회귀로 같은 안정성을 확인합니다.
>
> WPF Release도 계속 정상 실행 중입니다. 완료 후에는 같은 전체 묶음을 한 번 더 반복해, 단발 PASS가 아니라 동시성 flake가 재현되지 않는지 확인하겠습니다.
>
> Release 전체 회귀는 UI/child-process 종료 경계까지 포함해 약 2분 반이 걸립니다. 아직 비정상 종료나 실패 출력 없이 진행 중입니다.
>
> 반복 실행은 SDK Debug `1042`건을 먼저 통과한 뒤 WPF Debug `297`건을 다시 수행하는 중입니다. 결과는 실패와 합계만 수집하도록 줄여서, 완료 시 반복 수치만 명확히 확인하겠습니다.
>
> 두 번째 WPF Debug 전체 실행도 계속 정상 진행 중입니다. 동시성 오류가 다시 발생하면 종료 코드와 `FAIL` 행이 바로 잡히도록 실행하고 있습니다.
>
> Debug/Release 전체와 추가 반복 Debug까지 모두 같은 수치로 통과했습니다: SDK `1042/1042`, WPF `297/297`. 이제 계획·진척·자동시험 문서의 오래된 `1041/294` 수치를 실제 관측값으로 바꾸고, 이 값이 PLC 검증이 아니라 PC 회귀 증거임을 그대로 유지하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+5 -3)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+8 -4)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+9 -8)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+2 -2)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+4 -4)
>
> </details>
>
> 동시성 안정화는 끝났습니다. 다음 코드 단계는 실제 PLC 전송을 열지 않고도 진행 가능한 `SetPosition` one-shot 복구 경계로 좁혀 검토 중입니다. 현재 capability bit 3과 WPF 실행 노출은 계속 OFF로 두고, 재시작 시 자동 재전송이 절대 생기지 않는 PC 측 durable gate만 설계대로 붙일 수 있는지 먼저 대조하겠습니다.
>
> 검토 결과 코드·회귀시험에는 P1/P2 결함이 없었습니다. 다만 다른 “current” README/설계 문서에 예전 `1041/294` 수치가 남아 있어, 그 충돌까지 정리한 뒤 마무리하겠습니다.
>
> <details><summary>Edited 10 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -4)
> - Wrote `./LMC_Library/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+8 -8)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html` (+3 -3)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+4 -4)
>
> </details>
>
> SetPosition 후속 범위도 확인했습니다. 지금 바로 journal만 추가하면 응답 유실 후 같은 PLC identity에서 안전하게 해제할 권위가 없어 영구 격리만 만들게 됩니다. 따라서 `ActualPosition == Target`로 성공을 추정하지 않는다는 기존 원칙을 유지하고, PLC retained outcome 조회 또는 PowerOff+재참조 후 명시 폐기 중 하나를 먼저 승인해야 한다는 활성화 차단 조건을 설계문서에 명시합니다. 실제 버튼·capability·PLC mutation은 열지 않습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./docs/architecture/AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md` (+19 -0)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+4 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+1 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+3 -3)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+1 -1)
>
> </details>
</details>

자동 회귀의 실제 불안정 원인을 수정했습니다.

- 원인: fake-RPC worker가 요청 목록에 추가하는 동안 WPF 테스트가 같은 `List`를 열거해 `Collection was modified`가 발생했습니다.
- 수정: request/session 번호를 한 잠금에서 기록하고 읽을 때는 변경되지 않는 스냅샷을 반환합니다. [FakeRpcServer.cs](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/FakeRpcServer.cs:53)
- 결정적 회귀시험을 추가했습니다. [RpcIntegrationTests.cs](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs:49)
- 검증:
  - SDK Debug/Release: 각각 `1042/1042 PASS`
  - WPF Debug/Release: 각각 `297/297 PASS`
  - SDK/WPF Debug 추가 반복도 동일 수치 PASS
  - `git diff --check`, cached check PASS

관련 README·계획·진척 문서의 오래된 `1041/294` 수치도 현재 결과로 동기화했습니다.

다만 이것은 PC 자동시험 불안정 수정입니다. 실제 PLC의 Motion/Power/SDO Write 경로를 새로 활성화하거나 실축 검증한 것은 아닙니다.

다음 SetPosition 구현은 응답 유실 시 안전한 판정 방법이 먼저 필요합니다. `ActualPosition == Target`만으로 성공 처리하면 안 됩니다. 설계문서에 두 선택지를 기록했습니다. [SetPosition 설계](./docs/architecture/AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md:180)

권장은 PLC가 request/outcome을 보존하고 PC가 read-only로 조회하는 retained outcome ledger 방식입니다. 이 방식으로 진행할지 결정해 주면 durable journal과 zero-replay 경로부터 구현하겠습니다.

<details><summary>49 previous messages</summary>

> 다음 단계는 SetPosition의 영구 격리를 실제로 해소할 권위 있는 경로입니다. 사용자 응답이 없는 상태에서는 실송신을 열지 않고, 추천한 PLC retained outcome 방식이 기존 command ID·BootId/MapRevision·ownership 설계와 충돌하지 않는지 먼저 현재 소스에서 확정하겠습니다. 충돌이 없으면 read-only 조회 계약부터 구현하고 bit 3/native mutation은 계속 OFF로 유지합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 감사 결과가 일치했습니다. 현재 `RequestId`만으로는 재접속 후 결과를 권위 있게 식별할 수 없으므로, 먼저 dormant `SetPosition` 계약에 128-bit `IntentId`와 PLC 진단 identity를 고정하고 `0x7D14` 읽기 전용 결과 조회 계약을 SDK에 추가하겠습니다. PLC의 retentive 저장소 선언은 LASAL IDE 작업이 필요하므로, 이번 단계에서는 capability를 켜지 않고 PC 계약·테스트·문서부터 닫겠습니다.
