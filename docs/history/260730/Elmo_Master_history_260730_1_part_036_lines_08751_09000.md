> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+7 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RESPONSE_MODEL_DESIGN_2026-07-09.md` (+5 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+2 -2)
>
> </details>
>
> 추가 외부 구현 가능 항목을 하나 더 찾았습니다. 기능 코드를 억지로 확장하는 대신, 이미 존재하는 `LMCDiagnosticsService` constructor의 Bulk 상태 초기화를 exact-once/순서/부정 변이까지 정적 gate로 강화하겠습니다.
>
> 정적 gate 작업이 예상보다 지연돼 해당 작업을 회수했습니다. 기존 verifier의 공통 helper를 재사용해 최소 범위로 직접 완성하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+147 -55)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+37 -0)
>
> </details>
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+4 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+1 -1)
>
> </details>
>
> 독립 검토에서 실제 구멍 하나를 찾았습니다. 변수 이름만 맞고 타입이 바뀐 경우(`BulkState UINT→UDINT`, `BootIdFault BOOL→UDINT`)를 놓쳤습니다. 이름뿐 아니라 38개 상태의 정확한 타입까지 고정하고 두 타입 변이도 거부하도록 보완합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+63 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+46 -0)
>
> </details>
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+1 -1)
>
> </details>
>
> 마지막 점검만 남았습니다. 전체 unstaged diff, 문서 카운트, 이번에 건드린 untracked 파일을 확인하고 기존 staged 히스토리 파일의 별도 whitespace 실패도 분리해 기록하겠습니다.
</details>

이번 구현분은 완료했습니다.

- Axis/Group lookup을 `LMCLookupResult`와 `LMCLookupException`으로 구조화했습니다. exact 6-byte 응답, nonzero descriptor, parsed/raw 실패 증거를 보존합니다. [구현](./LMC_Library/LMC_API_Delivery/src/LmcConnectionModels.cs:16)
- P1-05 typed lookup 항목을 완료로 변경했습니다. [백로그](./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md:652)
- WPF Admin/Drive 실제 MainWindow 테스트를 추가했습니다. Axis 2 선택, `_LMCAxis2`, `0x202B`, Drive mode/status 요청을 바이트 단위로 검증합니다. [WPF 테스트](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs:136)
- `LMCDiagnosticsService` constructor 정적 gate를 38개 상태의 이름·타입, 37개 scalar exact-once, `BulkSignalIds[0..23]`, 제어문 금지, 최종 `C_OK` 순서까지 강화했습니다. 15종 부정 변이도 거부합니다. [정적 검증기](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1:386)

검증 결과:

- API Debug/Release: `607/607 PASS`
- WPF Debug/Release: `30/30 PASS`
- LASAL SourceOnly/full static: PASS
- PowerShell parser: PASS
- `git diff --check`: PASS
- PLC/실장/pcap 검증은 포함하지 않았습니다.

현재 CREVIS가 GUI에 안 보이는 것은 WPF 코드 부족으로 판단되지 않습니다. 현재 소스라면 capability bit 14가 켜지고 topology revision `0x15867EEC`, 7행 중 CREVIS 3행이 표시되어야 합니다. `Load CREVIS / Topology`를 누른 뒤 [실패 상세 표시](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs:1045)의 capability hex, Build, BootId, MapRevision, 오류 문구를 보내주세요. 실행 중 PLC가 최신 소스가 아니거나 topology RPC가 실패하는 상태입니다.

다음 실제 구현은 CREVIS live Health/DI입니다. 먼저 [T2 LASAL IDE 구조 작업](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md:16)을 생성·저장해 주세요. body는 비워도 됩니다. 그다음 제가 외부 편집으로 `0x7E13/0x7E22` 구현을 이어가겠습니다.

SDO 입력 편집은 실행 중에도 유지되도록 구현·검증됐습니다. [편집 정책](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/SdoEditorAvailabilityPolicy.cs:5) 다만 실제 Write는 PLC와 SDK gate가 모두 `false`이고 allowlist도 비어 있습니다. 활성화하려면 승인할 축 번호와 `0x2F00:24` 대상 범위를 확정해야 합니다.

커밋은 하지 않았습니다. 기존에 staged된 `docs/history/Elmo_Master_history_260721.md` 한 파일에 trailing-whitespace 168건이 있어 `git diff --cached --check`만 실패합니다.

<details><summary>41 previous messages</summary>

> 다음 외부 편집 가능 항목을 다시 찾겠습니다. 기능 gate를 임의로 켜지는 않고, CREVIS T2 준비·TCP 세션 상태·입력 래치 초기화처럼 실제 다음 통합에서 필요한 source/static 계약의 빈틈을 병렬 점검하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 남아 있는 PC/정적 구현 후보를 병렬 점검한 결과를 먼저 회수하겠습니다. LASAL IDE 구조가 필요한 변경은 건드리지 않고, 외부 편집만으로 안전하게 완료할 수 있는 항목부터 이어서 반영하겠습니다.
>
> 사용자가 남긴 최신 Test2 캡처도 함께 대조하겠습니다. 여기서 PLC가 실제로 bit 14와 7-entry topology를 반환했는지 확인하면, GUI 문제인지 다운로드된 LASAL 소스 문제인지 바로 분리할 수 있습니다.
>
> 최신 캡처에서는 static topology가 실제로 `7개 노드 / CREVIS 3개`로 반환됩니다. 다만 “Read EtherCAT Health” 버튼의 실제 wire는 신규 `0x7E13`이 아니라 기존 `0x7E10`이라 Elmo 4축만 읽고 있습니다. 이제 현재 LASAL 프로젝트에 T2용 IDE client/연결 구조가 실제로 생성됐는지 소스와 네트워크를 다시 확인하겠습니다.
>
> 다음 구현 후보를 확정했습니다. PLC를 바꾸지 않고도 가치가 큰 항목은 `GroupEnable(0x2047)`의 ACK만 받고 끝내지 않고 `GroupReadStatus(0x2045)`를 polling해 실제 Locked Standby가 안정적으로 확인될 때까지 기다리는 typed API입니다. SDK 구현·회귀와 일반 WPF 적용을 나눠 병행하겠습니다.
>
> 동시에 실제 LASAL 결함 후보가 확인됐습니다. 새 TCP 소켓 연결 시 이전 세션의 queue/RPC 등록 상태를 완전히 폐기하지 않아, 빠른 재접속에서 stale READY slot 때문에 첫 handshake가 queue-full로 유실되거나 이전 callback 등록이 새 세션에 남을 수 있습니다. 새 선언 없이 기존 CONNECT 구현부에서 고칠 수 있으므로 이 항목도 우선 반영하겠습니다.
>
> TCP 재접속 reset 구현은 완료됐습니다. 이제 stale queue/RPC 상태가 새 세션으로 넘어가지 않고 `CurrentSock`이 모든 초기화 뒤 마지막에 공개됩니다. 제가 변경부를 독립 검토한 뒤 전체 회귀에 포함하겠습니다.
>
> 독립 리뷰에서 정적 gate의 허점 1건을 잡았습니다. 실제 TCP reset 구현은 문제없지만, 검증기가 “필수 구문이 실행 불가능한 IF 안에 있거나 뒤에서 다시 덮이는 변형”을 놓쳤습니다. 허용 statement inventory와 canonical epoch block까지 정확히 검사하도록 보강하겠습니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_TEST2_CAPTURE_AUDIT_2026-07-28.md` (+96 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS는 최신 캡처 기준 정적 토폴로지 데이터가 실제로 올라온 것까지 확인했습니다. 지금은 `Group Enable → Locked Standby` 자동 검증을 마무리하면서, GUI에는 재전송 없이 상태 확인을 이어가는 흐름을 연결하고 전체 회귀 검증까지 진행하겠습니다. SDO 편집 잠금 문제는 WPF에서 먼저 풀었고, 실제 Write 활성화는 PLC 측 안전 조건이 확인되기 전까지 분리해 둡니다.
>
> 캡처를 다시 대조한 결과, PLC 응답에는 `Cravis`라는 문자열 대신 `GL_9086_11`, `GL_9086_1_Slot001`, `GL_9086_1_Slot011`로 들어옵니다. 따라서 GUI에서 “CREVIS가 없다”기보다, 기존 Elmo Health 화면만 보면 안 보이는 구조였습니다. 토폴로지 화면이 이 3개 행을 자동 표시하는지 코드와 테스트를 다시 확인하고 있습니다.
>
> 그룹 쪽은 SDK가 한 번의 Enable 승인만 보존하고, 이후에는 상태 조회만 이어가도록 구현 중입니다. GUI도 이 계약을 그대로 쓰게 바꿉니다. 즉 타임아웃이나 Stop/Power Off 선점 뒤에 사용자가 다시 눌러도 `0x2047`을 재전송하지 않고 `0x2045` 확인만 재개하게 합니다.
>
> <details><summary>Created a file, edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+0 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+39 -42)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+4 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+44 -22)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+4 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+129 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+16 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+5 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+173 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+1 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+0 -2)
>
> </details>
>
> 현재 WPF Debug 회귀는 `31/31 PASS`입니다. 여기에는 CREVIS 3개 행 자동 표시, SDO Write 입력란 편집 유지, Group Enable 1회만 송신 후 상태가 흔들렸다가 3회 연속 Locked Standby가 될 때만 완료되는 새 테스트가 포함됩니다. 이제 SDK 독립 리뷰 결과를 반영하고 Release/LASAL 정적 검증까지 이어갑니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+4 -2)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+13 -9)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+11 -7)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+10 -9)
>
> </details>
>
> 동일 PLC 세션에서 같은 그룹 핸들을 두 개 만들면 중복 Enable을 우회할 수 있는 경계가 발견돼, SDK 상태를 connection/group-reference 단위로 공유하도록 확장하고 있습니다. GUI 쪽도 안전 선점으로 첫 확인이 중단된 뒤 버튼을 다시 눌렀을 때 Enable 재송신 없이 상태 조회만 재개되는 실동작 테스트를 추가하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+117 -0)
>
> </details>
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+8 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+12 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+9 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS는 현재 정적 토폴로지까지는 GUI에 올라오도록 확인됐고, 동적 상태/I/O는 아직 미구현입니다. 지금은 Group Enable 안정화 코드의 동시성·취소 경계 검토를 끝낸 뒤 전체 C#/LASAL 정적 검증까지 한 번에 마무리하겠습니다. SDO Write는 편집 가능 상태만 반영하며, 실제 Write 활성화는 PLC 측 안전 계약이 준비되기 전에는 열지 않습니다.
>
> 두 GUI 문제는 현재 소스에서 재현 방지까지 들어가 있습니다. 연결 직후 토폴로지를 자동 조회해 `Nodes=7, CREVIS=3`과 7개 행을 표시하고, SDO 요청이 진행 중이거나 Write 후 exact readback이 남아 있어도 Slave/Index/SubIndex/Type/Length/Timeout/Write Value 초안은 계속 편집됩니다. 전송 버튼만 직렬화됩니다. 이 동작은 실제 WPF 컨트롤을 조작하는 스모크 테스트로 고정했습니다.
>
> 문서 대조 중 구현과 충돌하는 오래된 설명 두 곳을 찾았습니다. 실제 코드는 pending exact readback 중에도 초안 편집을 허용하는데, 설계 문서 일부가 아직 “editor 전체 고정”이라고 적혀 있어 지금 바로 현재 동작과 맞추겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+5 -3)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+1 -1)
>
> </details>
>
> 추가로 실제 경고 문구도 “UI가 exact Read로 잠긴다”라고 남아 있어 코드 동작과 모순됐습니다. 이를 “초안은 편집 가능하지만 다음 전송만 exact Read로 제한”으로 수정했습니다. 현재 남은 핵심은 Group Enable ACK 이후 다른 핸들의 상태 조회와 동시에 완료될 때 생길 수 있는 경계 조건 1건이며, 회귀 테스트와 함께 보정 중입니다.
>
> LASAL 정적 검증은 지금 다시 실행해 SourceOnly/full 모두 PASS했고, 변경한 custom `.st`의 7-bit ASCII도 확인했습니다. Group Enable 쪽은 Disable이 다른 핸들에서 동시에 성공하는 경우를 상태 조회 실패로 잘못 분류할 수 있는 경계 1건을 추가로 잡아 회귀 테스트와 함께 수정 중입니다.
>
> 동시성 리뷰에서 한 건을 더 잡았습니다. 다른 Group 핸들의 직접 상태 조회가 세 번째 정상 샘플을 완성한 순간, 대기 중 helper가 불필요한 `0x2045`를 한 번 더 보낼 수 있었습니다. 완료 증거를 송신 직전에 다시 확인해 추가 wire 요청 없이 끝내도록 수정하고 있습니다.
>
> 첫 `Group Enable` 전송에도 같은 취소/시간 제한 경계가 필요하다는 점까지 확인했습니다. 다른 RPC 때문에 전송 대기 중이면 취소·deadline 후 `0x2047`이 뒤늦게 나가면 안 되고, 이미 전송이 시작됐다면 응답을 끝까지 받아 ACK/continuation을 보존해야 합니다. 이 계약까지 포함해 테스트를 확장하고 있습니다.
>
> Disable 동시 실행 경계도 정리 중입니다. `0x2048` ACK가 먼저라면 대기 중 helper의 `0x2045`는 0건이어야 하고, 상태 조회가 먼저라면 반드시 wire 순서상 Disable보다 앞서야 합니다. 이 순서를 shared mutation gate로 고정하고 deadlock 여부까지 재검토합니다.
>
> 전송 경계 리뷰에서 한 가지 더 발견했습니다. wire 전에 발생한 helper deadline을 transport timeout으로 오인해 정상 TCP 연결을 fault 처리할 수 있었습니다. “업무상 pre-wire 거부”와 실제 socket/write/read 실패를 분리하고, zero-wire인 경우 연결이 계속 `Connected`인지도 테스트에 넣습니다.
>
> 현재 Group 안정화 범위는 네 가지로 정리됐습니다: 상태 proof 원자화, Disable과 상태 조회 순서 고정, 첫 Enable의 pre/post-write 취소·deadline 구분, 업무상 timeout의 TCP fault 오분류 방지입니다. 소스가 안정되면 Group 전용 테스트를 먼저 통과시키고, 그 다음 전체 Debug/Release 회귀를 돌리겠습니다.
>
> 현재 변경은 테스트 수를 늘리는 수준이 아니라 motion command의 “전송됐는지 모르는 상태”를 만들지 않기 위한 경계 보강입니다. CREVIS/SDO GUI 수정은 이미 32개 WPF 스모크에서 통과했고, 이 Group 보강이 안정되는 즉시 전체 회귀와 문서 수치 갱신으로 마감하겠습니다.
>
> 문서도 현재 동작에 맞췄습니다. 과거 `0x213F`와 최신 Test2 `0x613F`를 분리했고, static CREVIS 7행은 wire 확인 완료·동적 Health/DI/DO는 미구현으로 통일했습니다. SDO 문구도 “전체 비활성”이 아니라 “초안 편집 가능, approved target/Submit만 비활성”으로 정리했습니다. Group 쪽 중간 전체 회귀는 626/626 PASS이며 추가 경합 테스트를 넣는 중입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 두 GUI 문제는 코드와 실제 WPF 컨트롤 테스트까지 반영됐습니다. 지금은 동시에 진행한 Group Enable 안정화 변경에서 `Disable`과 상태 관찰의 경합까지 닫았고, 전체 PC 테스트 634개 통과 결과를 바탕으로 Release 검증과 문서 숫자 정합성을 마무리하겠습니다.
>
> 추가 경합 검토에서 실제 결함 하나를 찾아 수정 중입니다. `GroupDisable(0x2048)`가 진행 중일 때 이미 모인 3회 상태 증거로 Enable 성공을 잘못 확정할 수 있던 창이 있었고, 이제 상태 관찰·Disable·완료 판정을 같은 순서로 직렬화했습니다. 성공/실패 ACK 양쪽 회귀 테스트까지 넣고 다시 전수 검증하고 있습니다.
>
> 동시성 재검토까지 최종 통과했습니다. 현재 결과는 API Debug/Release 각각 `636/636`, WPF actual-control smoke 각각 `32/32`입니다. 이제 이 숫자와 CREVIS 정적 표시/동적 미구현 경계, SDO 편집 가능 범위를 현행 문서에 맞춘 뒤 전체 정적 검사를 다시 돌리겠습니다.
>
> <details><summary>Edited 11 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./LMC_Library/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -7)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_SDO_WRITE_SAME_VALUE_QUALIFICATION_2026-07-28.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
>
> </details>
</details>

반영 완료했습니다.

- CREVIS 구성 정보는 Connect 후 자동 로드됩니다. `EtherCAT / CREVIS / PI` 탭에서 `Nodes=7, CREVIS=3`이 표시되며, 수동 `Load CREVIS / Topology` 버튼도 추가했습니다. [MainWindow.xaml](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:739)
- SDO 요청 실행 중에도 Slave/Index/SubIndex/Type/Data 입력값을 계속 편집할 수 있습니다. 실행 중인 요청은 시작 시점 값으로 고정되고 추가 Submit만 직렬화됩니다. [MainWindow.Diagnostics.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:2193)
- 최신 GUI는 제목에 `[CREVIS topology / editable SDO draft]`가 표시됩니다. 이 문구가 없으면 이전 실행 파일입니다. [Release GUI 실행 파일](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Release/LasalMotionControlApiExample.exe)
- 병행 개발한 Group Enable 안정 대기와 Resume/Disable 경합 처리도 완료했습니다.

검증 결과:

- API Debug/Release: 각각 `636/636 PASS`
- WPF 실제 컨트롤 smoke Debug/Release: 각각 `32/32 PASS`
