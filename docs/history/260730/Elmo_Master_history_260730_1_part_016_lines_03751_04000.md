> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+42 -12)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+3 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs` (+17 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs` (+28 -0)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs` (+13 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs` (+13 -0)
>
> </details>
>
> 정적 검사는 현재 LASAL SourceOnly와 full contract 모두 통과했습니다. 다만 코드 리뷰에서 중요한 결함을 하나 더 잡았습니다. Write 응답이 유실되거나 ticket slot이 바뀐 경우, 기존 Read용 복구 로직이 “terminal이었음”만으로 격리를 풀 수 있었고 실제 주소·값 기록도 부족했습니다. 이 상태로 bit 9를 켜면 안 되므로, exact 요청 fingerprint 보존·Write 전용 격리·bit 13 readback 조건까지 먼저 보강하고 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> SDO Write 핵심 구현은 들어갔고, 지금은 “응답 유실 시 성공 여부를 오판하지 않는가”를 마지막으로 보강 중입니다. 병렬 검토 결과를 합친 뒤 API 274개 테스트, WPF Debug/Release 빌드, LASAL 정적 계약을 다시 전부 확인하겠습니다. 실제 Write 활성화 게이트는 대상 축과 `UI[24]` 미사용 확인 전까지 꺼둡니다.
>
> PLC 쪽은 별도 IDE 조작 없이 정적 계약을 다시 확인하고 있습니다. 현재 구현은 임의 주소 쓰기가 아니라, PLC와 SDK 양쪽의 동일한 승인 목록을 통과한 4바이트 Int32 대상만 쓰도록 막아 둔 구조입니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs` (+9 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs` (+3 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs` (+17 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs` (+49 -0)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+17 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+2 -1)
>
> </details>
>
> 중요한 안전 보강까지 반영됐습니다. Write가 `Failed/Expired`, `TicketNotFound`, 취소 경쟁 상태로 끝나면 성공/실패를 추정해 지우지 않고 격리합니다. 현재 PC 계약 테스트는 275/275 통과했고, 실행 중인 WPF가 기본 출력 파일을 잠근 상태라 그 프로세스는 건드리지 않고 별도 출력 폴더로 빌드를 확인 중입니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+1 -1)
>
> </details>
>
> LASAL 쪽 최종 검토에서 정적 검증기의 허점도 찾았습니다. 현재 `Classes.lcb`에는 새 Write 선언이 아직 없는데 기존 full 검증이 잘못 PASS하던 상태라, IDE 동기화 전에는 full PASS로 보지 않도록 검증기를 수정하고 있습니다. 이 부분은 사용자가 LASAL에서 Reload/Save/Build한 뒤에만 해제됩니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+14 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs` (+11 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 지금 구현된 SDO Write 흐름을 최종 점검하고 있습니다. 먼저 GUI의 성공 후 정확 읽기 확인 인터록 작업 결과를 회수한 뒤, 실행 중인 WPF는 건드리지 않고 별도 출력 폴더에서 전체 빌드·테스트를 돌리겠습니다. LASAL은 소스 검증까지만 하고, IDE 메타데이터 동기화·빌드는 요청하신 순서대로 사용자 단계로 남기겠습니다.
>
> 코드 측 안전장치는 예상대로 연결됐습니다. 현재 동작은 “Write 전송 성공 = 완료”로 끝나지 않고, 같은 Slave/Index/SubIndex/Type/Length를 다시 읽어 4바이트 값까지 일치해야 다른 변경 작업과 창 닫기가 다시 허용되는 구조입니다. 문서는 이 상태와 LASAL IDE 메타데이터 미동기화 상태를 정확히 구분해 정리하고 있습니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+9 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+5 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+13 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+9 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+36 -26)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+16 -8)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+17 -1)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+16 -10)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+8 -7)
>
> </details>
>
> PC 회귀는 Debug/Release 모두 276개로 늘어났고 이미 통과했습니다. 이제 같은 최종 파일 상태로 재실행하고, 이어서 별도 출력 경로 WPF 빌드와 Git 범위 검사를 한 번 더 확인한 뒤 커밋하겠습니다. 현재 LASAL full 검증 실패는 코드 실패가 아니라 `Classes.lcb`가 아직 IDE에서 갱신되지 않았음을 잡아낸 정상 차단입니다.
>
> 최종 독립 리뷰에서 한 가지 실제 보완점이 확인됐습니다. 값 비교 자체는 정확하지만, readback 조건이 원래 Write의 PLC 세션·BootId·MapRevision까지 묶여 있지 않았습니다. 이 상태면 재접속 뒤 다른 세션에서 우연히 같은 값이 읽혀 잠금이 풀릴 수 있으므로, 해당 identity까지 보존·대조하도록 바로 보강한 뒤 테스트 수를 다시 확정하겠습니다. 프로세스 강제 종료 대비 영속 journal은 별도 큰 작업이라 실제 Write gate는 계속 닫아 둡니다.
>
> 원 세션 identity 보강은 단순 UI 체크가 아니라 API 자체의 guarded submit 오버로드까지 넣는 방향으로 진행 중입니다. 이 오버로드는 원 Write ticket의 owner·세션 generation·BootId·MapRevision을 새 capability와 다시 맞춘 뒤에만 Read 요청을 전송하므로, 재접속 경합도 wire 전 차단합니다. 기존 API 호출 방식은 그대로 유지됩니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> SDO Write 구현은 연결된 상태이고, 지금은 마지막 안전성 검토·회귀 테스트·문서 정합성 확인 후 목적별 커밋까지 마무리하겠습니다. 실제 쓰기 허용은 안전 객체와 축이 확정되지 않아 게이트를 닫아 둔 상태로 유지합니다.
>
> PC API의 추가 identity 회귀까지 Debug/Release 각각 277/277로 통과했습니다. 문서의 이전 276개 표기와 readback의 세션·BootId·MapRevision 구속 조건을 현재 코드에 맞게 갱신하겠습니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+4 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+6 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+5 -5)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+5 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -3)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+5 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -3)
>
> </details>
>
> LASAL 정적 계약은 source 기준 PASS했고, 현재 `Classes.lcb` 미동기화를 잡아내는 full 검사는 의도대로 FAIL했습니다. stale-metadata 우회 검사는 PASS했지만 이는 LASAL 빌드 증거로 취급하지 않습니다.
>
> 두 차례 독립 검토에서도 신규 P0/P1 결함은 없었습니다. 코드·테스트와 문서를 분리해 커밋하되, 사용자가 이미 수정한 `Classes.lcb`, staged history 파일, 미추적 시험 자료는 제외하겠습니다.
</details>

SDO Write 구현과 커밋까지 완료했습니다.

- LASAL: `0x7E50` Write 처리, 4-byte 전송 버퍼, callback/timeout/cancel/orphan 처리
- API: 승인 대상 allowlist, Write 요청, owner/session/BootId/MapRevision 보호
- WPF: Read/Write 선택, 값 입력, PowerOff/Standstill 검사, 확인창, quarantine, Write 후 exact Readback 인터록
- PC 테스트: Debug/Release 각각 277/277 PASS
- WPF: Debug/Release 격리 빌드 PASS
- LASAL SourceOnly 계약: PASS
- 독립 검토: 신규 P0/P1 없음

커밋:

- `3ae7b88 feat(diagnostics): add gated SDO write workflow`
- `efed7fb docs(diagnostics): document SDO write checkpoint`

현재 실제 Write 대상은 0개입니다. 후보는 Gold `UI[24] = 0x2F00:24`, Int32/4-byte지만 안전을 위해 PLC/API의 global·축별 gate가 모두 `FALSE`입니다.

다음 진행에 필요한 것은 두 가지입니다.

1. Drive Program에서 `UI[24]`가 미사용인지 확인
2. 첫 시험 축을 1~4 중 하나로 지정

축을 알려주면 해당 한 축만 PLC/API 양쪽에서 활성화하겠습니다. 이후 사용자가 LASAL에서 `LMCSdoExecutor`, `LMCDiagnosticsService`를 Reload/Save하고 Rebuild/Link해야 합니다. 현재 full 정적 검사는 `Classes.lcb` 선언 미동기화 때문에 의도대로 실패합니다.

주의: 강제 종료 시 pending Write/readback 상태를 복구하는 durable journal은 아직 없습니다. 실험 중 앱 강제 종료는 피해야 하며 production 활성화 전에는 추가 구현이 필요합니다.

기존 `Classes.lcb`, staged history 파일과 미추적 시험 자료는 건드리거나 커밋하지 않았습니다.

<details><summary>1 previous message</summary>

> 축 선택을 기다리는 동안 멈추지 않고, SDO Write의 남은 P1인 강제 종료 후 pending/quarantine 유실 문제를 다음 구현 대상으로 잡겠습니다. 먼저 현재 소스와 설계 문서에서 영속화·재시작 복구 계약을 확정한 뒤 API/WPF/테스트를 함께 반영하겠습니다.
</details>

> 네트워크에 크래비스 I/O를 추가했어. 그런데 케이블 사정상 EtherCAT 노드 1번으로 옮기고 나머지 노드들을 한개씩 밀었다. 근데 EtherCAT 데이터는 기존이랑 동일하게 올라와.
> EtherCAT 데이터는 EtherCAT 노드 상태에 따라서 동적으로 변경되야 되는거 아님? 그리고 EtherCAT I/O를 제어할 수 있는 API도 필요할꺼 같은데, Elmo는 어떻게 되어있는지 참고해서 API를 만들자. 그걸 구현 리스트에 추가해서 반영해

<details><summary>23 previous messages</summary>

> 크래비스 추가와 노드 순서 변경이 우선입니다. 먼저 현재 Network/PDO 변경분과 실제 주소 결정 방식을 확인하겠습니다. 일반적으로 EtherCAT PDO 구조는 런타임 노드 상태에 따라 자동 재배열되지 않고 프로젝트의 ENI/논리 매핑으로 고정되므로, “값이 기존과 동일”한 현상이 정상 매핑인지 잘못된 참조인지 소스 기준으로 판정해야 합니다. 진행 중이던 Write journal 작업은 잠시 멈추고 이 항목을 구현 목록의 우선 작업으로 전환하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 변경분에서 CREVIS는 실제로 ENI의 `Slave 01`로 들어갔고 Elmo 드라이브는 `Slave 02`부터로 밀린 것이 확인됐습니다. 이제 “화면 데이터가 그대로인 이유”를 찾기 위해 물리 순서와 LASAL 객체 참조, API의 고정 4축 테이블을 각각 분리해 대조하고 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 결론부터 말하면, PDO 배치 자체가 노드의 온라인 상태에 따라 재정렬되면 안 됩니다. 프로젝트에 구성된 논리 노드/PDO 매핑은 고정하고, 각 노드의 온라인·OP·AL 상태와 데이터 유효성만 동적으로 바뀌어야 합니다. 현재 GUI가 계속 4축만 같은 번호로 보여 주는 것은 실제 네트워크가 안 바뀐 게 아니라, 진단 API가 4개 Elmo 축과 `SlaveIndex 0..3`을 고정 생성하는 한계입니다. 이 부분을 토폴로지 API와 CREVIS I/O API로 분리해 구현 목록에 반영하겠습니다.
>
> Elmo 쪽도 확인했습니다. Elmo/Maestro는 “온라인 노드만 다시 번호를 매기는 동적 PDO”가 아니라, 구성된 PI 매핑과 별도로 슬레이브 inventory/state/identity를 조회합니다. 전용 I/O는 논리 I/O 참조를 기준으로 DI 전체 읽기와 DO 전체 쓰기를 제공합니다. 우리 구현도 이 구조를 따르되, CREVIS DO는 다른 비트를 덮지 않도록 마스크 쓰기를 함께 제공하는 쪽으로 설계하겠습니다.
>
> 구현 순서는 기존 4축 Health를 억지로 5행으로 바꾸는 방식이 아닙니다. `0x7E10`은 구버전 호환용 4축 Health로 유지하고, 별도 토폴로지/가변 노드 상태 명령과 디지털 I/O 읽기·쓰기 명령을 추가하는 것으로 잡았습니다. 출력 쓰기는 처음부터 비활성 capability와 빈 allowlist로 두고, PLC의 RT 소유자가 마스크를 원자적으로 적용한 뒤 ticket 상태로 완료를 확인하도록 설계 중입니다.
>
> 1차 SDK 범위는 실제 출력 활성화가 아니라, 명령 ID·모델·요청/응답 파서·공개 메서드·negative test를 먼저 넣고 capability/allowlist는 닫아 둔 상태로 마무리하고 있습니다. 그래서 현재 PLC나 CREVIS 출력이 이 코드만으로 바로 바뀌지는 않습니다. 실제 동작은 다음 단계에서 LASAL의 읽기 snapshot과 단일 RT 출력 소유자를 연결한 뒤 사용자가 빌드/실기 확인하는 순서입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 동작을 두 층으로 분리해 반영하고 있습니다. 구성된 PDO/노드 순서는 고정하고, 런타임에서는 노드별 Online·OP·AL 상태와 데이터 유효성만 동적으로 바뀌게 하겠습니다. 동시에 Elmo 방식의 토폴로지 조회·디지털 I/O 읽기/쓰기 계약을 구현 목록과 C# API에 먼저 넣고, PLC 기능이 아직 없을 때는 capability로 확실히 차단되도록 검증하겠습니다.
>
> 현재 확인된 핵심은 다음과 같습니다. CREVIS가 `SlaveIndex 0`, 기존 Elmo 4축이 `1~4`로 밀렸지만 LASAL의 축 객체 연결은 그대로라 축 데이터가 같은 논리 축으로 보이는 것은 정상입니다. 반대로 기존 `0x7E10` 진단은 4축을 `0~3`으로 만들어 내므로 실제 5노드 토폴로지를 표현하지 못합니다. 이 호환 API는 유지하고, 실제 토폴로지·노드 상태·I/O는 별도 명령으로 분리합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+138 -123)
>
> </details>
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+10 -8)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+8 -7)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+18 -18)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+11 -8)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+103 -2)
>
> </details>
>
> C# 쪽은 현재 PLC가 지원하지 않는 명령을 실수로 보내지 않도록 닫힌 상태로 완성했습니다. 특히 출력 쓰기는 `TopologyRevision + IOReference + Mask + ExpectedOutputRevision + BootId`를 묶은 CAS 방식이고, SDK 허용 목록도 비어 있습니다. 따라서 지금 단계에서 실제 출력이 움직일 수는 없으며, PLC/LASAL RT owner를 구현하고 실기 검증하기 전까지 capability bit 14~17은 0으로 유지됩니다. 이제 Debug/Release 전체 회귀와 문서·wire 일치 검사를 돌립니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+10 -8)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -1)
>
> </details>
>
> 전체 worktree 검사에서는 제가 건드리지 않은 staged history 파일의 기존 trailing whitespace가 검출됐습니다. 이 파일은 사용자의 별도 변경이라 수정하지 않겠습니다. 이번 구현 파일만 한정한 diff 검사는 별도로 통과시키고, 커밋할 때도 정확한 파일 목록만 지정해 기존 LASAL/history 변경을 그대로 보존하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+2 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+12 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+17 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+4 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+20 -10)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+3 -2)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+2 -1)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+4 -3)
>
> </details>
>
> 최종 리뷰에서 두 가지 실제 계약 누락을 잡았습니다. 첫째, 완성된 topology가 `MasterSlaveIndex 0..N-1` 중복 없이 구성됐는지와 같은 부모의 slot index 중복을 아직 막지 않았습니다. 둘째, 한 `IOReference`가 표현할 수 있는 v1 digital I/O 폭은 방향별 최대 64비트인데 topology parser가 8바이트 초과를 허용했습니다. 둘 다 C# parser에서 차단하고 회귀 테스트를 추가한 뒤 전체 시험을 다시 돌리겠습니다.
