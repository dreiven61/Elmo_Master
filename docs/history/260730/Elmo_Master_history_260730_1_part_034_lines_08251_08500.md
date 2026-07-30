> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoDisconnectOrphanQualificationOrchestrator.cs` (+89 -17)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.D5Disconnect.cs` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+26 -16)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+40 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+136 -0)
>
> </details>
>
> 코드 수정은 끝났고, 지금부터 회귀를 돌립니다. 우선 API 전체 테스트에서 D5 계약·retry deadline·capability drift를 확인한 뒤 WPF 실제 컨트롤 smoke로 CREVIS/SDO 편집/zero-wire gate/취소 커밋을 검증하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoDisconnectOrphanQualificationOrchestrator.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoDisconnectOrphanQualificationOrchestrator.cs` (+5 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+33 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+36 -0)
>
> </details>
>
> 현재 PC 측 회귀는 API 587/587, WPF 20/20까지 통과했습니다. 남은 PLC orphan 판정은 지금 소스만으로는 증거를 읽을 방법이 없으므로, LASAL IDE에서 생성해야 하는 선언과 이후 외부 구현 경계를 별도 인계 문서로 고정합니다. 이 문서가 생겨도 capability나 `orphanQualified`는 아직 켜지지 않습니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./docs/architecture/LMC_D5_ORPHAN_WITNESS_IDE_HANDOFF_2026-07-28.md` (+152 -0)
>
> </details>
>
> Debug 회귀는 모두 통과했습니다. 이제 같은 소스로 Release를 다시 빌드해 이전 수치가 아닌 현재 수치로 문서를 확정하겠습니다. 이 검증은 PC/가짜 RPC 범위이며 PLC 다운로드나 실제 EtherCAT 증거로 확대 해석하지 않습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+14 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+10 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+10 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+10 -10)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+12 -11)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+12 -12)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+6 -7)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 표시와 SDO Write 편집 잠금 문제는 반영됐고, 현재는 D5 단선 복구 경로의 마지막 자동 검증 공백인 “기존 연결 종료 → 새 연결 복구” 2세션 GUI 테스트까지 붙일 수 있는지 점검 중입니다. 기존 단일 세션 테스트 서버를 깨지 않는 범위로 구현 가능하면 바로 추가하고 Debug/Release 전체 검증까지 다시 돌리겠습니다.
>
> 2세션 테스트용 서버 확장은 기존 587개 API 테스트를 그대로 통과했습니다. 이제 실제 GUI 핸들러를 끝까지 실행해, 첫 연결의 RPC Close 미전송·새 연결 채택·두 번의 복구 SDO·CREVIS 재로딩·증거 ledger 정리까지 한 번에 검증하는 스모크를 추가하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+168 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+287 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+52 -1)
>
> </details>
>
> 첫 통합 실행에서 새 테스트만 실패했고 기존 20개는 모두 통과했습니다. 연결 전환 자체인지 scripted 응답 계약인지 구분할 수 있도록 실패 시 GUI의 구조화 로그와 서버 요청 위치를 함께 노출한 뒤 원인을 바로 좁히겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+6 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+17 -13)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+2 -2)
>
> </details>
>
> 2세션 전체 GUI 경로가 이제 통과했습니다: 구 세션 29요청/새 세션 22요청, 구 세션 `0x405D` 없음, 새 연결 채택 후 다른 topology revision의 CREVIS 데이터 재로딩, 복구 ticket 2개, quarantine 0까지 확인했습니다. 현재 Debug WPF 스모크는 `21/21 PASS`이며, 문서 수치와 테스트 설명을 이에 맞추겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+5 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+6 -5)
>
> </details>
</details>

반영 완료했습니다.

- CREVIS: Connect 및 재연결 후 topology를 자동 로드하고, 이전 세션 정보가 남지 않도록 수정했습니다. 구성 정보는 `Configured CREVIS entries=3`으로 표시됩니다.
- SDO Write: Write/상태 확인 중에도 다음 요청의 Slave/Index/Type/Value 등을 편집할 수 있습니다. 실제 Submit만 직렬화됩니다.
- D5 단선 복구: 실제 GUI 핸들러를 old/new TCP 2세션으로 검증하는 테스트를 추가했습니다. old 세션 `0x405D` 미전송, 새 연결 채택, 복구 SDO 2회, quarantine 해제, 새 CREVIS topology 재로딩까지 확인합니다.

검증 결과:

- API Debug/Release: `587/587 PASS`
- WPF Debug/Release: `21/21 PASS`
- 빌드: 경고 0, 오류 0
- `git diff --check`: PASS
- 최종 리뷰: 차단 결함 없음

주요 검증 코드는 [WpfMainWindowIntegrationTests.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs:610), 2세션 서버 확장은 [FakeRpcServer.cs](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/FakeRpcServer.cs:20)에 있습니다.

단, 현재 보장되는 것은 CREVIS의 configured topology 표시입니다. 실시간 노드 상태와 DI/DO 값은 LASAL 측 `0x7E13/0x7E22` live handler가 아직 필요합니다. 이번 결과도 PC fake-RPC 검증이며 실제 PLC 동작이나 TCP RST/PLC orphan 증거는 아닙니다.

커밋이나 staging은 하지 않았습니다. `git diff --cached --check`는 기존에 staged된 `docs/history/Elmo_Master_history_260721.md`의 trailing whitespace 때문에 계속 실패합니다.

<details><summary>25 previous messages</summary>

> 다음 미완료 항목을 현재 소스와 구현계획에서 다시 대조한 뒤, LASAL IDE 없이 진행 가능한 가장 앞선 작업부터 구현하겠습니다. 우선 CREVIS live I/O, D5 PLC witness, SDO Write gate 중 무엇이 외부 편집만으로 실제 진척 가능한지 분리하고, 선택한 항목을 코드·테스트·문서까지 맞추겠습니다.
>
> 점검 결과 CREVIS live 값(`0x7E13/0x7E22`)은 LASAL IDE에서 client/method/network 구조를 먼저 생성해야 해서 지금 외부 편집만으로는 진행할 수 없습니다. 대신 이미 dormant로 완성된 SDO Write 경로에 “승인 후 바로 실행 가능한 activation qualification runner”가 빠져 있으므로, gate는 계속 닫아 둔 채 same-value Write·exact readback·복구/증거 보존을 자동 판정하는 코어와 테스트부터 구현하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 확인된 경계는 명확합니다. CREVIS의 구성 정보는 GUI까지 반영됐지만, 실시간 Online/AL/DI/출력 상태는 LASAL IDE에서 선언·네트워크 구조를 먼저 만든 뒤에만 연결할 수 있습니다. 그 구조는 외부 편집으로 합성하지 않고, 지금은 승인 전에도 안전하게 검증 가능한 SDO Write 자격검증 경로를 구현하면서 GUI 편집 잠금 회귀도 함께 재검증하겠습니다.
>
> SDO Write 쪽은 sentinel 값을 쓰고 복구하는 방식은 제외합니다. 현재 단일 mutation journal로는 복구 단계 사이의 크래시 창을 안전하게 막을 수 없습니다. 대신 승인된 대상의 현재 4바이트를 먼저 읽고, 그 값 그대로 Write한 뒤 정확히 다시 읽는 최소 안전 자격검증 흐름으로 구현합니다. 승인 게이트가 닫힌 현재 빌드에서는 첫 단계에서 전송 0회로 차단됩니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+66 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+18 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md` (+1 -0)
>
> </details>
>
> GUI에는 새 Write 자격검증 영역을 추가 중입니다. 현재 SDK allowlist가 비어 있으므로 버튼은 명확히 `CLOSED` 상태로 보이고, 강제로 핸들러를 호출해도 PLC 요청을 보내기 전에 차단되도록 구성합니다. 기존 SDO 편집 필드는 요청 실행 중에도 계속 편집 가능하며, 전송 버튼만 현재 티켓/정확 readback 규칙에 따라 직렬화됩니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md` (+2 -1)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+3 -0)
>
> </details>
>
> <details><summary>Created a file, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.SdoWrite.cs` (+768 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+2 -0)
>
> </details>
>
> 자격검증 core는 6개 새 계약 테스트를 포함해 API 전체 `593/593`를 통과했습니다. 지금 WPF adapter를 연결했고, 다음 빌드에서 실제 컴파일 오류와 gate-off 전송 0회 동작을 잡겠습니다. 이 단계에서도 PLC/LASAL 코드나 Write 활성화 상수는 건드리지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+66 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 작업분의 안전성 리뷰부터 회수한 뒤, SDO Write 동일값 검증 경로의 저널 처리 문제를 정리하고 Debug/Release 정적 검증과 문서 갱신까지 이어가겠습니다. LASAL IDE는 제어하지 않으며, CREVIS 실시간 T2 경로는 IDE 구조 추가 전까지 구현 완료로 표시하지 않겠습니다.
>
> 동일값 Write 경로의 핵심 순서는 맞지만, 저널을 해제한 뒤 로그 기록이 실패하면 화면이 “복구 필요”로 오인할 수 있는 경계가 확인됐습니다. 저널 해제 콜백은 상태 변경만 수행하도록 좁히고, 성공 로그는 오케스트레이터 완료 뒤의 비결정적 부가 작업으로 분리하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.SdoWrite.cs` (+49 -41)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+3 -1)
>
> </details>
>
> GUI 쪽 정리는 반영했습니다. 일반 SDO 편집 잠금 완화는 그대로 유지되고, 안전 검증용 동일값 Write 화면은 실행 중 입력 고정만 유지합니다. 또한 실제 Write가 저널에 기록된 뒤에는 취소 버튼 자체를 비활성화해 “눌렀지만 무시됨” 상태를 없앴습니다. 이제 전체 PC 테스트와 WPF 통합 테스트를 다시 돌립니다.
>
> 리뷰에서 실제로 고쳐야 할 안전 결함 3개가 나왔습니다.
>
> - 확인 창을 오래 열어둔 사이 축 상태가 바뀔 수 있으므로, 최종 확인 뒤 안전축을 다시 검사해야 합니다.
> - Wire에서 승인된 Write ticket은 내용 검증 전에 먼저 영구 보존해야 합니다.
> - 최초 Read 값이 확인 과정 중 바뀌면 이전 값을 다시 쓰는 상황을 막기 위해, Write 직전 두 번째 Read로 값 불변을 증명해야 합니다.
>
> 이 세 항목을 반영한 뒤에만 Release 검증으로 넘어가겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+11 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.SdoWrite.cs` (+7 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+6 -1)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.SdoWrite.cs` (+115 -27)
>
> </details>
>
> LASAL 외부 정적 검증은 현재 허용된 범위에서 모두 통과했습니다. SourceOnly와 generated metadata 포함 full static 모두 PASS입니다. 다만 CREVIS 실시간 T2 체크포인트는 예상대로 `0x7E13`과 `0x7E22` 라우트가 없어서 실패했습니다. 즉 GUI의 구성 정보 표시는 준비됐지만 Online/상태/DI 실시간 값은 아직 LASAL IDE 구조 추가 전입니다.
>
> 보강된 경로는 Debug에서 API `596/596`, WPF 실제 컨트롤 smoke `22/22`로 통과했습니다. 현재 gate가 닫힌 상태라 실제 SDO Write는 전혀 송신되지 않았습니다. 이제 동일 소스를 Release로 반복 검증하고, 수정된 안전 경계에 대해 한 차례 더 독립 리뷰를 진행 중입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.SdoWrite.cs` (+14 -0)
>
> </details>
>
> <details><summary>Edited 9 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.SdoWrite.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_SDO_WRITE_SAME_VALUE_QUALIFICATION_2026-07-28.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -4)
>
> </details>
>
> 최종 안전 순서는 `확인 → 값 불변 guard Read → 최종 축 안전 재검사 → journal → Write`로 조정했습니다. guard Read 대기 때문에 축 안전 판정이 낡는 문제까지 제거했고, 최종 안전 검사 뒤 Write 전에는 추가 비동기 I/O가 없습니다. 문서도 이 실제 순서와 API `596/596`, WPF `22/22`에 맞췄습니다.
</details>

반영 완료했습니다.

- CREVIS 구성 정보는 GUI에서 자동 표시됩니다. 다만 실시간 Online/상태/DI는 LASAL의 `0x7E13`, `0x7E22` 구조가 없어 아직 표시되지 않습니다. 필요한 IDE 작업은 [T2 구조 작업서](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md)에 정리했습니다.
- 일반 SDO Write 진행 중에도 다음 요청 데이터를 편집할 수 있습니다. Submit만 직렬화됩니다.
- 동일값 Write 검증기는 `baseline Read → 사용자 확인 → 값 불변 guard Read → 최종 축 안전검사 → journal → Write 1회 → exact Readback`으로 강화했습니다.
- Write ticket은 의미 검증 전에 영구 보존하며, sentinel·자동 복원·재전송은 없습니다.
- 현재 PLC/SDK Write gate는 계속 꺼져 있으므로 실제 Write는 송신되지 않습니다.

검증 결과:

- API Debug/Release: `596/596 PASS`
- WPF Debug/Release: `22/22 PASS`
- LASAL SourceOnly/full static: PASS
- CREVIS live T2 checkpoint: 예상대로 `0x7E13`, `0x7E22` 부재로 FAIL
- `git diff --check`: PASS
- `git diff --cached --check`: 기존 staged [Elmo_Master_history_260721.md](./docs/history/Elmo_Master_history_260721.md:2508)의 trailing whitespace 때문에 FAIL

실제 SDO Write 활성화 전에는 `UI[24]` 미사용 확인과 시험 축 1개 지정이 필요합니다. 상세 절차는 [동일값 Write 검증서](./docs/architecture/LMC_SDO_WRITE_SAME_VALUE_QUALIFICATION_2026-07-28.md:23)에 있습니다. PLC/live 성공으로는 아직 판정하지 않았고 커밋도 하지 않았습니다.

<details><summary>9 previous messages</summary>

> 다음 단계는 현재 소스와 구현 계획을 다시 대조해, 사용자 확인이나 LASAL IDE 구조가 없어도 진행 가능한 항목부터 고르겠습니다. CREVIS T2와 SDO gate 활성화는 그대로 차단 상태로 유지하고, 정적/PC 범위에서 실제 완성도를 높일 다음 항목을 병렬로 감사하겠습니다.
>
