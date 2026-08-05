> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -2)
> - Wrote `./docs/architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md` (+7 -5)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+4 -4)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+6 -6)
> - Wrote `./LMC_Library/LMC_API/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -4)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -7)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+5 -5)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+7 -7)
>
> </details>
>
> 문서 수치와 계약 설명은 최신 구현에 맞췄습니다. 지금 제가 별도로 WPF 전체 251개를 다시 돌리고 있으며, 여기까지 SDK 998/998 두 구성과 LASAL SourceOnly/full static 계약은 모두 재현됐습니다.
>
> WPF 전체 smoke가 SDK보다 오래 걸려 계속 실행 중입니다. 별도 에이전트 실행에서는 Debug/Release 251/251이 이미 통과했고, 현재 재실행은 같은 결과를 독립 재현하기 위한 확인입니다.
>
> 독립 재실행에서도 WPF Debug 251/251이 통과했습니다. Release 251개를 같은 방식으로 돌리는 중이며, 완료 뒤에는 diff/프로젝트 등록/문서 누락만 최종 점검하고 끝냅니다.
</details>

수정 완료했습니다. 예제 앱은 실제 Motion/Power/SDO Write 송신 경로를 사용하며, Group Reset 때문에 기능이 영구 비활성화되는 문제도 정리했습니다.

- Group Reset은 `0x2049`를 한 번만 전송합니다.
- 이후 `0x2045`와 각 멤버 축의 `0x2028` 상태만 조회해 오류 해제 3회 연속을 확인합니다.
- Reset 후 Power/Identity/Home/Profile 준비 상태를 무효화합니다. Power On → Set Identity → Enable을 다시 완료해야 Motion이 활성화됩니다.
- Reset 확인 중에는 위험 명령을 차단하고 Stop, Power Off, 안전 Disable만 허용합니다.
- 안전 명령이 거부되면 Reset 확인 상태를 보존하고, 수락되거나 결과 불명이면 정확히 종료합니다.
- 연결이 끊겨도 Reset이나 Motion 명령을 자동 재전송하지 않습니다.

주요 구현:

- [LmcGroupResetWait.cs](./LMC_Library/LMC_API_Delivery/src/LmcGroupResetWait.cs)
- [MainWindow.GroupResetRecovery.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.GroupResetRecovery.cs)
- [Group Reset 설계 문서](./docs/architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md)

검증 결과:

- SDK Debug/Release: 각각 `998/998 PASS`
- WPF Debug/Release: 각각 `251/251 PASS`
- Group Reset 집중 시험: Debug/Release 각각 `16/16 PASS`
- LASAL SourceOnly/full network static 계약: PASS
- 독립 리뷰: P0/P1 잔여 결함 없음
- `git diff --check`, `git diff --cached --check`: PASS

아직 실제 PLC 다운로드 후 실축 Motion/Power/SDO Write를 실행한 증거는 없습니다. 현재 결과는 PC/fake-RPC/정적 계약 검증입니다. 스테이징과 커밋도 하지 않았습니다.

<details><summary>43 previous messages</summary>

> 이어서 진행하겠습니다. 먼저 현재 소스와 개발 계획의 미완료 항목을 다시 대조해, 방금 끝낸 Group Reset 다음으로 PC에서 독립 구현·검증 가능한 최우선 기능을 선정한 뒤 바로 구현하겠습니다.
>
> 다음 기능은 Group Reset의 재연결·프로세스 재시작 복구로 확정했습니다. 현재 코드는 연결 손실 시 Reset 상태를 의도적으로 폐기하므로, 정확한 그룹/멤버/PLC identity를 명령 직전에 영속화하고 재시작 뒤에는 `0x2049` 없이 상태 조회만 수행하도록 확장합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 진행 중인 Group Reset 재접속·프로세스 재시작 복구 구현을 이어갑니다. 우선 SDK, 영구 저널, WPF 통합 세 작업 결과를 합쳐서 “Reset 재전송 없이 상태만 검증”되는지 코드와 테스트로 교차 확인하겠습니다.
>
> 현재 워크트리는 기존 사용자 변경이 매우 많아 이번 기능과 무관한 파일은 건드리지 않습니다. 새 복구 경로의 핵심 검증 기준은 세 가지입니다: 명령 직전 저널 기록, 재시작 후 PLC·그룹·멤버 exact-match, 그리고 복구 중 `0x2049` 전송 0회입니다.
>
> SDK 쪽은 새 세션에 이전 결과를 붙일 때도 저장된 멤버 목록을 그대로 신뢰하지 않고, 먼저 현재 PLC에서 `0x20D2`를 1회 읽어 순서·이름·reference·device까지 비교하도록 잡았습니다. 하나라도 다르면 상태 조회도 하지 않고 저널을 유지한 채 격리합니다.
>
> 복구 세션에서 mutation generation이 0인 초기 상태도 정상 기준값으로 취급합니다. 이후 0→1 변화가 생기면 간섭으로 잡히도록 별도 “기준값 캡처됨” 증거를 넣고, 저널 OperationId는 실제 continuation 게시 성공 시에만 소비해 snapshot mismatch나 timeout 뒤 재시도까지 막는 오류를 피하고 있습니다.
>
> WPF 복구 경로는 연결만 성공했다고 일반 제어를 열지 않습니다. 연결 단계에서는 endpoint와 PLC build/BootId/MapRevision만 확인하고, 정확한 그룹을 Load할 때 SDK가 단 한 번의 fresh `0x20D2`로 멤버 snapshot을 검증한 뒤에만 Reset 상태조회 continuation을 게시하도록 통합하고 있습니다.
>
> 현재 검토 중인 또 하나의 경계는 “결과 불명”과 “성공”을 섞지 않는 것입니다. 재시작 후 all-clear가 확인돼도 이전 Reset ACK가 유실된 기록이면 UI에는 현재 오류가 안정적으로 0이라는 사실만 표시하고, 과거 Reset 명령 성공으로 표기하지 않도록 분리합니다.
>
> SDK 구현은 들어왔고 현재 7개 회귀군을 추가하는 중입니다. 여기에는 command-before observer 실패 시 Reset 0회·연결 재사용, exact attach 후 status-only Resume, 모든 member identity mismatch, 중복·동시 attach, generation 0 기준 간섭, cancel/timeout typed failure가 포함됩니다.
>
> 영구 저널에는 원격 PLC 주소뿐 아니라 실제 바인딩된 PC local IPv4와 callback UDP port도 함께 저장합니다. 따라서 다른 NIC·다른 callback port로 재연결한 세션은 같은 PLC IP여도 복구 세션으로 인정되지 않고, 명령·상태조회 없이 격리됩니다.
>
> SDK Debug는 제가 별도로 다시 실행해 `1005/1005 PASS`, 경고·오류 0을 확인했습니다. 이제 영구 저널과 WPF 통합이 들어오는 즉시 저널 원자성, 재시작 후 Reset 0회, stable proof 뒤 journal-first resolve 순서를 집중 검토합니다.
>
> 이제 구현 이름과 wire 계약이 확정됐으므로 관련 설계 문서를 실제 동작에 맞춰 갱신합니다. 문서에는 SDK/PC 자동시험 완료와 PLC/runtime 미검증을 계속 분리해 기록하겠습니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./docs/architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md` (+16 -7)
> - Wrote `./docs/architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md` (+37 -15)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+8 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+11 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+15 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+11 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+12 -6)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+9 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+10 -5)
>
> </details>
>
> 독립 SDK 리뷰에서 실제 결함 1건을 잡았습니다. prepared observer가 `OperationCanceledException`을 던지면 SDK 취소로 잘못 분류되던 문제입니다. wire는 0회였지만 원인이 왜곡되므로 observer 실패인 `LMCGroupResetSubmissionException`으로 보존하도록 수정하고 회귀를 추가 중입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Group Reset 내구 저널 자체는 독립 테스트 10/10으로 완료됐습니다. 이제 WPF 연결부와 SDK 예외 분류 수정이 합쳐지는 즉시 전체 Debug/Release 회귀와 재시작 무재전송 계약을 검증하겠습니다.
>
> SDK 독립 리뷰에서 실제 결함 2건을 바로잡았습니다. 준비 콜백의 취소 예외가 사용자 취소로 오인되던 문제와, 상태 조회 중 모든 취소 예외를 deadline으로 오인하던 문제입니다. 수정 후 SDK Debug/Release가 각각 1006/1006 통과했습니다.
>
> WPF 내구 연결 파일이 생성됐고, 현재 endpoint/Build/BootId/MapRevision를 먼저 확인한 뒤 exact group과 fresh `0x20D2` snapshot으로만 attach하도록 연결됐습니다. 이전 session generation은 저장된 명령 증거로만 사용하며 새 session과 비교하지 않는 것도 확인했습니다.
>
> 독립 WPF 리뷰에서 두 번째 실제 dead-end를 찾았습니다. 이전 Reset 결과가 `OutcomeUncertain`인 재시작 복구는 attach까지 성공해도 UI 플래그가 continuation을 숨겨 status-only 확인을 시작하지 못했습니다. 담당 구현에 수정과 Accepted/OutcomeUncertain 양쪽 회귀를 추가하도록 반영했습니다.
>
> 문서도 구현과 함께 맞추고 있습니다. 상태표에서 “Group Reset durable restart recovery 미구현”으로 남은 항목을 exact identity·fresh member snapshot·status-only/no-replay 구현으로 갱신하되, PLC/runtime 검증은 계속 미완료로 분리하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+10 -5)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+8 -5)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+29 -2)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html` (+17 -5)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+9 -2)
>
> </details>
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html` (+10 -7)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+5 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+8 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+8 -4)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 화면에서 막는 직접 원인은 일반 기능 고장이 아니라, 이전 PLC의 복구 레코드와 현재 PLC의 `BootId`가 달라 앱이 쓰기 명령 전체를 격리한 상태입니다. 현재 작업 중인 복구/폐기 경로까지 포함해 실제 Motion·Power·SDO Write가 안전하게 다시 활성화되도록 소스와 테스트를 바로 확인해 수정하겠습니다.
>
> 현재 소스에는 이 격리를 무시하는 우회가 아니라, 오래된 복구 레코드를 증거와 함께 폐기한 뒤 앱을 재시작·재연결해서 쓰기를 다시 여는 절차가 들어가 있습니다. 다만 첨부 화면은 그 UI가 없는 이전 빌드(`editable SDO draft`)라서, 최신 소스가 실제 실행 파일에 반영되는지와 세 기능의 버튼 조건을 함께 검증 중입니다.
>
> 현재 단일 Axis Power 불일치(첨부 화면과 같은 경우)는 새 폐기 패널을 통해 재활성화되는 것을 16개 집중 테스트로 확인했습니다. 추가 점검에서 “정상 identity 레코드와 오래된 레코드가 동시에 남은 경우”에는 빠져나갈 수 없는 조합을 발견해, 오래된 항목만 폐기하고 정상 항목은 다음 재시작에서 복구하도록 보완하고 있습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+1 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+1 -1)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md` (+29 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
>
> </details>
>
> 실제 전송 경로 자체는 이미 연결돼 있습니다. 현재 확인된 구조는 Motion/Power는 복구 인터록 해제 후 바로 wire 명령 경로로 들어가고, SDO Write는 Axis 1의 승인된 `0x2F00:24 Int32/4` 대상에 한해 현재 세션의 Same-Value 검증을 통과하면 `0x7E50` 제출 경로가 열리는 방식입니다. 지금은 그 앞단의 stale 레코드 해제 후 재진입 회귀를 마무리하는 중입니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+5 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+7 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+6 -3)
> - Wrote `./docs/architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+4 -1)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html` (+4 -1)
>
> </details>
>
> 복구 경계도 실제 프로세스를 강제 종료하는 방식으로 보강했습니다. `0x2049` 전, ACK 후 첫 status 전, 첫 status round 후의 세 지점 모두 재시작 세션에서 Reset 재전송 0회, member snapshot 재확인 1회, status-only 완료를 확인했고 이제 전체 Debug/Release 회귀를 돌리고 있습니다.
>
> 전체 WPF 회귀가 장시간 실행되는 구간입니다. 여기서 확인하는 것은 단순 버튼 활성화뿐 아니라, 기존 Axis/Group/SDO 복구·안전 명령이 이번 변경으로 깨지지 않았는지까지 포함한 269개 계약입니다.
>
> 전체 회귀가 끝났습니다. 최신 소스 기준 SDK는 Debug/Release 각각 `1006/1006`, WPF는 각각 `270/270` 전부 통과했습니다. 중간에 기존 Group Stop 지연 응답 테스트의 capability step 누락도 드러나 보정한 뒤 두 구성 모두 다시 전량 통과시켰습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 막는 원인은 통신 자체가 아니라, 이전 PLC 식별자로 남은 복구 저널 때문에 새 PLC 세션이 안전 격리된 것입니다. 자동 삭제하지 않고, 오래된 기록만 증거와 함께 폐기한 뒤 재시작해서 실제 명령 경로를 다시 여는 방식으로 마무리 검증 중입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+1 -1)
>
> </details>
>
> 수정된 경로는 “격리 해제 후 바로 임의 쓰기 허용”이 아닙니다. Motion/Power는 기존 실제 RPC 송신을 다시 활성화하고, SDO Write는 현재 승인된 Axis 1 `0x2F00:24`만 동일값 4-ticket 검증과 2회 확인을 거쳐 실제 `0x7E50`으로 전송되도록 제한했습니다. 이제 최종 Release 회귀를 한 번 더 확인하겠습니다.
>
> Release 회귀는 아직 실행 중입니다. 현재까지 즉시 실패는 없고, 별도 프로세스 강제 종료/재시작까지 포함하는 복구 테스트라 약 2분 정도 걸립니다. 실제 PLC에 명령을 보내는 시험은 하지 않고 fake RPC로 wire 순서와 재전송 금지를 확인하는 단계입니다.
>
> 최종 Release 검증 결과는 WPF `270/270`, SDK `1006/1006`, LASAL source/network 정적 계약 모두 PASS입니다. 이제 문서 수치·바이너리 해시와 새로 추가된 미추적 소스까지 공백/등록 누락을 확인한 뒤 결과를 넘기겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GROUP_API_IMPLEMENTATION_2026-07-14.md` (+4 -3)
>
> </details>
</details>

수정 완료했습니다. Motion/Power는 실제 RPC 명령을 전송하고, SDO Write는 승인된 대상에 한해 실제 `0x7E50`을 전송합니다.

사용 순서:

1. 최신 빌드인 [LasalMotionControlApiExample.exe](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Release/LasalMotionControlApiExample.exe)를 실행합니다.
2. 창 제목이 `qualified Axis1 UI24 SDO Write`인지 확인합니다. `editable SDO draft`라면 이전 실행 파일입니다.
3. Connect 후 표시되는 `Archive and retire stale recovery evidence`에서 설비 상태를 직접 확인하고 체크박스 → `Archive and Retire Stale Recovery`를 누릅니다.
4. 프로그램이 종료되면 재실행하고 다시 연결합니다.
5. exact-current 복구 기록이 남아 있으면 `Resume ... Verification (No Replay)`를 먼저 완료합니다.
6. 이후 Power/Motion 버튼과 SDO qualification이 활성화됩니다.

SDO Write 범위는 현재 Axis 1의 `0x2F00:24`, Int32/4-byte만 허용합니다. 네 가지 확인 조건과 Same-Value qualification을 통과한 뒤, 첫 클릭으로 Arm하고 동일 요청을 다시 클릭해야 실제 전송됩니다. 다른 SDO와 Axis 2~4는 계속 차단합니다.

주요 구현은 [retirement 처리](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecoveryRecordRetirement.cs:262), [SDO 실제 제출](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:1752), [PLC SDO 허용 정책](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:249)에 반영했습니다.

검증 결과:

- WPF Release: `270/270 PASS`
- SDK Release: `1006/1006 PASS`
- LASAL source/network 정적 계약: PASS
- `git diff --check`: PASS
- 커밋/스테이징: 하지 않음

자동 시험은 PC/fake RPC 검증입니다. SDO Write를 실 PLC에서 사용하려면 변경된 LASAL 프로젝트를 IDE에서 빌드·다운로드해야 합니다.

<details><summary>34 previous messages</summary>

> 설계·구현 문서의 현재 체크포인트를 다시 소스와 대조해 다음 미완료 항목을 고르겠습니다. 문서에 적힌 “남은 일”을 그대로 믿지 않고, 실제 코드·테스트·PLC 경계를 확인한 뒤 가장 높은 우선순위 기능부터 구현하겠습니다.
>
> 문서 대조 결과, 새 PLC opcode를 늘리는 것보다 먼저 P0-C의 축별 실기 절차를 자동화하는 것이 맞습니다. 현재 Power/Move/Stop/PowerOff 송신 코어는 있지만 이를 한 번의 검증 시나리오로 묶어 정확한 명령 수·최종 위치·안전 복구 증거를 남기는 runner가 없습니다. 이 runner를 기존 durable/no-replay 경로 위에 구현하고 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 이어서 진행하겠습니다. 지금은 `Single Axis` 실제 Power/Motion 경로를 자동 검증하는 qualification runner 구현을 회수한 뒤, 기존 내구성 기록·무재전송 경계를 우회하지 않았는지 먼저 검토하고 전체 빌드/회귀 검증까지 끝내겠습니다.
>
> 안전 계약을 한 단계 더 좁혔습니다. `PowerOn` 상태만 참이어도 축 오류가 남아 있을 수 있으므로, 자동 이동 직전에는 `PowerOn + Referenced + Standstill + AxisError=0`을 별도로 확인하게 합니다. 이 확인이나 물리 안전 체크가 실패하면 Move는 물론 조회 RPC까지 0건이어야 합니다.
