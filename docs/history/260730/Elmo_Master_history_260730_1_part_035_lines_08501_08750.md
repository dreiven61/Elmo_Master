> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 반영된 CREVIS 표시와 SDO 편집 동작을 기준으로, 다음에 바로 진행 가능한 누락 항목을 소스에서 다시 좁히고 있습니다. PLC/LASAL 구조 변경이 필요한 부분은 건드리지 않고, 지금 가능한 API·WPF·정적 검증 항목부터 이어서 구현하겠습니다.
>
> 다음 구현은 세 갈래로 병행하겠습니다.
>
> - WPF: SDO 동일값 Write의 `PASS/복구 필요` 결과가 UI 갱신 때 사라지는 문제 수정
> - API: Submit ACK와 최종 완료를 혼동하지 않도록 1/2/4-byte SDO Read 완료형 공개 API 추가
> - 정적 검증: `LMCDiagnosticsService` 생성자의 D5/Boot 초기화 누락을 잡도록 LASAL 계약 검사 강화
>
> CREVIS live T2와 SDO Write gate 활성화는 여전히 LASAL IDE 구조 생성 및 시험 축 승인 전에는 진행하지 않습니다.
>
> 세 항목 모두 구현 중입니다. 현재까지 확인된 중요한 경계는 그대로입니다: CREVIS 구성 정보는 WPF에서 표시할 수 있지만, Online/AL/DI/DO live 값은 PLC의 `0x7E13/0x7E22` 구조가 생기기 전에는 실제 데이터로 만들 수 없습니다. 이번 변경에서는 그 사실을 숨기지 않고 표시·검증만 강화합니다.
>
> WPF 결과 보존 수정과 LASAL 정적 계약 강화는 끝났고 각각 테스트를 통과했습니다. SDO Read 완료형 API는 지금 전체 회귀를 돌리는 중이며, 단순 ACK가 아니라 같은 ticket의 `Completed/Success`와 exact 타입·길이·데이터까지 확인한 결과만 반환하도록 검토하고 있습니다.
>
> 이제 통합 검증으로 넘어갑니다. 공개 SDO Read API 신규 7개를 포함한 Debug 전체는 이미 `603/603 PASS`했고 checked-overflow 빌드도 통과했습니다. 제가 Release와 WPF 양 구성, LASAL 정적 계약을 다시 순서대로 확인한 뒤 문서 수치와 현재 구현 상태를 맞추겠습니다.
>
> 첫 통합 Debug 재실행은 코드 결함이 아니라, 직전에 만든 `/checked+` 테스트 바이너리를 MSBuild가 “최신”으로 오판해 그대로 재사용하면서 기존 unchecked 전제 테스트 313개가 오버플로로 실패했습니다. 일반 설정으로 강제 Rebuild한 뒤 다시 판정하겠습니다. 이 실패는 숨기지 않고 최종 검증 기록에 구분해 남깁니다.
>
> <details><summary>Edited 12 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+9 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+6 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+6 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+5 -3)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -7)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+7 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_SDO_WRITE_SAME_VALUE_QUALIFICATION_2026-07-28.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
>
> </details>
</details>

반영 완료했습니다.

- 새 WPF는 연결 후 구성 기준 7개 노드와 CREVIS 3개 모듈을 자동 표시합니다.
- SDO Write 진행 중에도 다음 요청의 Slave/Index/SubIndex/Type/Value를 편집할 수 있고, 실제 Submit만 직렬화됩니다.
- SDO 동일값 Write의 준비 상태와 마지막 실행 결과를 분리했습니다. UI 갱신 후에도 `PASS`나 `RECOVERY REQUIRED`가 사라지지 않습니다.
- `ReadSdoInline[Async]` 공개 API를 추가했습니다. ACK가 아니라 같은 ticket의 `Completed/Success`와 정확한 타입·길이·데이터까지 확인합니다.
- LASAL 정적 검사에 `LMCDiagnosticsService` D5/Boot 초기화 28개 exact-once 검증을 추가했습니다.

검증 결과:

- API Debug/Release: `603/603 PASS`
- WPF Debug/Release smoke: `23/23 PASS`
- LASAL SourceOnly/full static: PASS
- `git diff --check`: PASS
- 첫 Debug 증분 실행은 이전 `/checked+` 바이너리 재사용으로 실패했지만, 일반 설정 강제 Rebuild 후 `603/603`으로 재확인했습니다.

새 GUI는 [Release 실행 파일](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Release/LasalMotionControlApiExample.exe)을 사용하면 됩니다.

현재 제한은 명확합니다.

- CREVIS의 구성 정보는 표시되지만 실제 Online/AL/DI/DO 값은 LASAL의 `0x7E13/0x7E22` T2 구조가 아직 없어 `UNAVAILABLE`이 정상입니다.
- 실제 SDO Write는 승인 target과 PLC/SDK gate가 모두 닫혀 있어 송신하지 않았습니다.
- 기존 staged history 파일의 trailing whitespace 때문에 `git diff --cached --check`는 계속 실패합니다. 이번 변경에서는 해당 사용자 파일을 수정하지 않았고 커밋도 하지 않았습니다.

CREVIS live 구현을 계속하려면 [T2 LASAL IDE 구조 작업서](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md)대로 IDE에서 client/method/network 구조를 생성한 뒤 Rebuild/Link 결과가 필요합니다.

<details><summary>19 previous messages</summary>

> 현재 worktree와 구현 계획을 다시 대조해서, LASAL IDE 작업을 기다리지 않고 진행 가능한 다음 항목을 고르겠습니다. CREVIS T2와 실제 SDO Write gate는 그대로 닫아 두고, API·WPF·정적 계약에서 남은 실질적인 누락을 병행 점검하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 소스 기준으로 CREVIS 라이브 데이터가 안 올라오는 원인은 다시 확인했습니다. 화면 문제가 아니라 LASAL 쪽 T2 구조와 `0x7E13/0x7E22` 경로가 아직 실제 프로젝트에 없기 때문입니다. 남은 API·GUI 점검 결과까지 합쳐, IDE 없이 안전하게 진행할 수 있는 다음 구현부터 이어가겠습니다.
>
> 다음 작업은 세 갈래로 병행합니다.
>
> - API: SDO Read가 이미 받은 terminal 결과를 취소 상태로 덮어쓰는 결함 수정
> - WPF: 기존 Submit/Refresh는 유지하고, 1/2/4바이트 Read를 terminal까지 기다리는 원클릭 경로 추가
> - LASAL 정적 검증: Write 활성화 전에 `LMCSdoExecutor` 명시적 초기화가 반드시 존재하도록 차단 조건 추가
>
> CREVIS 실시간 값은 WPF에서 우회 구현하지 않습니다. 현재 막힌 지점은 LASAL IDE에서 추가해야 하는 T2 클래스/네트워크 구조라서, 그 전까지는 구성 정보와 “live unavailable”을 명확히 표시하는 수준이 맞습니다.
>
> API와 GUI 작업은 진행 중입니다. 이번 보강의 핵심은 “ACK를 받았는가”가 아니라, 이미 수신한 terminal 상태와 마지막 nonterminal 상태를 잃지 않는 것입니다. 따라서 취소·timeout 후에도 ticket과 마지막 상태를 GUI의 수동 복구 경로에서 그대로 확인할 수 있게 맞추고 있습니다.
>
> API 보강은 완료됐고 Debug 전체 테스트가 `605/605 PASS`했습니다. terminal 성공/실패 응답이 도착한 경우에는 뒤늦은 UI 취소가 그 증거를 덮지 않으며, 아직 Running/Queued인 경우의 취소·timeout은 `LastObservedStatus`를 보존합니다. 이제 WPF와 LASAL 정적 gate 결과를 합쳐 교차 검증하겠습니다.
>
> WPF도 구현 단계는 끝났고 smoke `25/25 PASS`까지 확인됐습니다. 새 버튼은 일반 SDO Read 전용이며 Write와 exact Write readback에는 사용되지 않습니다. 마지막으로 capability-off zero-wire와 전체 Debug/Release 조합을 제가 다시 검증한 뒤 결과를 정리하겠습니다.
>
> PC 측 교차 검증은 현재 API Debug/Release `605/605`, WPF Debug/Release `26/26`까지 모두 통과했습니다. 이어서 LASAL은 IDE를 건드리지 않고 SourceOnly/full 정적 계약만 다시 확인합니다. 실제 PLC 다운로드와 CREVIS live 값은 이 결과와 별개입니다.
>
> <details><summary>Edited 12 files</summary>
>
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+16 -13)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+5 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -7)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_SDO_WRITE_SAME_VALUE_QUALIFICATION_2026-07-28.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+8 -4)
>
> </details>
>
> 최종 리뷰에서 정적 gate와 WPF 로그에 보강할 지점이 발견됐습니다. 그대로 두지 않고 마무리합니다.
>
> - constructor gate가 초기화 문장의 “존재”만 보는 허점을 막아, 조기 `RETURN`, 조건문 안 초기화, Idle 이후 재변경도 거부
> - Inline Read에 PC wait 취소 버튼을 실제 연결
> - terminal failure를 “ticket preserved”라고 잘못 기록하던 상태 로그 수정 및 smoke 추가
>
> 이 보강 후 전체 카운트가 다시 바뀌므로 테스트와 문서를 한 번 더 맞춥니다.
>
> <details><summary>Edited 9 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_SDO_WRITE_SAME_VALUE_QUALIFICATION_2026-07-28.md` (+1 -1)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 구성 정보가 GUI에 자동 표시되도록 하고, SDO 작업 중에도 입력값을 계속 편집할 수 있게 반영했습니다. 지금은 문서의 남은 표현을 맞춘 뒤 WPF Debug/Release와 LASAL 정적 계약을 다시 검증하겠습니다. 실제 CREVIS 온라인 상태와 I/O 값은 별도 PLC 구조(T2)가 아직 필요하므로, 구성 정보와 실시간 정보를 구분해서 결과를 정리하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+6 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -2)
>
> </details>
>
> 최종 문서 계약도 실제 동작에 맞췄습니다. 이제 동일 소스를 기준으로 WPF Debug/Release 29개 회귀와 LASAL SourceOnly/full 정적 검사를 병렬로 돌립니다. 이 검사는 PC 빌드·정적 계약 확인이며 PLC 다운로드나 실기 CREVIS 검증으로 간주하지 않습니다.
>
> WPF는 Debug/Release 모두 29/29 통과했고, LASAL 정적 계약도 SourceOnly/full 모두 통과했습니다. 마지막으로 SDO Write 활성화가 현재 constructor 누락 상태에서 확실히 차단되는지, 미추적 테스트 파일과 전체 diff의 공백 오류, 문서의 오래된 테스트 수 표기를 확인하겠습니다.
>
> 미추적 파일 검사에서 실제 공백 오류가 아니라 Git의 LF→CRLF 경고를 오류로 오인했습니다. 파일 내용 문제는 아니며, 자동 줄바꿈 경고를 제외하도록 검사를 바로 다시 실행합니다.
>
> GUI 회귀는 최신 빌드로 확정됐습니다. API 쪽도 이번 SDO Read/취소 증거 보존 변경을 포함해 Debug/Release 전체 605개를 한 번 더 재빌드 검증하고, 그 결과를 이번 전달 기준으로 고정하겠습니다.
>
> 전체 API 재빌드는 성공했지만, 요약 한 줄만 추출하는 보조 명령의 공백 패턴이 실제 출력 형식과 맞지 않아 그 보조 확인만 실패했습니다. 테스트 자체 실패가 아니며, 원문 마지막 줄을 그대로 읽어 Debug/Release 결과를 분리 확인합니다.
</details>

반영 완료했습니다.

- 최신 WPF는 연결 직후 CREVIS 구성 topology를 자동 조회합니다. 정상이라면 7개 항목 중 CREVIS 3개(커플러·입력 슬롯·출력 슬롯)가 표시됩니다.
- 창 제목에 `[CREVIS topology / editable SDO draft]`가 없으면 이전 실행 파일입니다.
- SDO 요청 중에도 Slave/Index/SubIndex/Type/Length/Value를 편집할 수 있습니다. 실행 중인 요청은 immutable snapshot으로 유지되고 Submit만 직렬화됩니다.
- `Read SDO Inline`과 `Cancel Inline Wait (PC only)`도 추가했습니다. PC 대기만 취소하며 PLC 요청을 재전송하거나 취소하지 않습니다.

최신 실행 파일:

- [Debug WPF](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Debug/LasalMotionControlApiExample.exe)
- [Release WPF](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Release/LasalMotionControlApiExample.exe)

검증 결과:

- API Debug/Release: 각각 `605/605 PASS`
- WPF Debug/Release: 각각 `29/29 PASS`
- LASAL SourceOnly/full 정적 계약: PASS
- `git diff --check`: PASS
- PLC 빌드·다운로드·실기 검증은 아직 수행하지 않음

현재 중요한 제한도 있습니다.

- CREVIS 구성 정보는 PLC capability bit 14와 `0x7E11/0x7E12`가 최신 PLC에 있어야 표시됩니다. 최신 WPF인데도 안 보이면 `Load CREVIS / Topology`를 눌러 표시되는 오류와 capability 값을 확인해야 합니다.
- 실제 Online/AL State/DI/DO는 아직 bit 15/16과 `0x7E13/0x7E22` LASAL 구조가 없어서 `UNAVAILABLE`이 정상입니다. 다음 작업은 [T2 IDE 구조 인계서](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md)에 정의돼 있습니다.
- 실제 SDO Write는 SDK allowlist와 `LMCSdoExecutor` constructor가 아직 활성화되지 않아 zero-wire 차단 상태입니다.

현재 LASAL 프로젝트를 빌드·다운로드한 뒤 최신 WPF로 CREVIS 구성 행부터 확인하면 됩니다. 기존 staged history 파일의 trailing whitespace 때문에 `git diff --cached --check`는 아직 실패하며, 이번 작업은 별도로 stage/commit하지 않았습니다.

<details><summary>23 previous messages</summary>

> 계속 진행하겠습니다. 현재 남은 항목 중 LASAL IDE 구조 생성이 필요한 T2/SDO Write 활성화는 건드리지 않고, 외부 편집만으로 완결할 수 있는 다음 구현 항목을 현재 소스와 백로그에서 다시 추려 API·WPF·LASAL 정적 계약을 병렬 점검한 뒤 바로 구현하겠습니다.
>
> 점검 결과, 큰 기능 블록은 대부분 외부 코드까지 구현돼 있고 남은 핵심은 IDE 생성 구조나 PLC 실기 조건에 걸려 있습니다. 그 사이 놓친 실제 코드 공백도 확인 중이며, 현재 한 가지는 `Read-only API` 탭의 drive 상태 UI가 SDK 단위시험만 있고 MainWindow fake-RPC 통합 검증이 비어 있다는 점입니다. 기능 확장보다 먼저 이 통합 경계를 고정할지, 더 직접적인 API 기능 공백이 있는지 계속 대조하고 있습니다.
>
> 다음 증분을 세 갈래로 확정했습니다.
>
> - API: 아직 부분 상태인 Axis/Group lookup을 공개 typed result와 typed exception으로 완성합니다. Wire/LASAL은 바꾸지 않습니다.
> - WPF: 이미 구현된 Admin/Drive read 탭을 실제 MainWindow와 fake RPC로 끝까지 검증해 event wiring·capability gate·표시 누락을 잡습니다.
> - LASAL 정적 계약: D4 Double-bank 활성화 전에 `LMCRecorderStore` constructor 초기화 누락·중복·조기 공개를 verifier가 확실히 차단하도록 강화합니다.
>
> 서로 다른 파일 영역이라 병렬 구현한 뒤 제가 통합 검토와 전체 Debug/Release 검증을 맡겠습니다.
>
> 구현은 세 영역에서 진행 중입니다. 현재까지 production wire나 capability를 넓히는 변경은 없고, 새 API는 기존 `0x103C/0x1042` lookup 응답을 구조화해 노출하는 범위로 제한했습니다. CREVIS·SDO Write의 미검증 capability는 계속 닫힌 상태를 유지합니다.
>
> API와 WPF 코드 반영은 끝났고 현재 자동 회귀를 돌리는 중입니다. typed lookup은 기존 생성자·`CreateAsync`·reference 속성을 유지하면서 성공 응답과 실패 raw/response를 별도 객체로 보존하도록 들어갔습니다. LASAL 쪽은 대용량 recorder 데이터 자체를 부팅 때 지우지 않고, 접근을 여는 metadata/state 초기화 순서만 엄격히 검증하도록 작업 중입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 세 작업 결과를 취합한 뒤 전체 회귀 테스트와 문서 동기화까지 이어서 마무리하겠습니다. 실제 PLC/CREVIS 표시 여부는 정적 검증과 분리해 남은 조건을 명확히 적겠습니다.
>
> API와 WPF 증분은 각각 로컬 테스트를 통과했습니다. 지금은 두 변경을 독립적으로 다시 검토하면서 LASAL 정적 검증기 결과를 기다리고 있습니다.
>
> 독립 검토와 병행해 API·WPF의 Debug/Release 전체 테스트를 다시 돌립니다. 이 결과는 PC 측 회귀 증거이며 PLC/CREVIS 실기 증거로 간주하지 않습니다.
>
> 동시에 네 빌드를 걸자 공용 `obj` 파일 잠금이 충돌했습니다. 코드 실패가 아니라 빌드 출력 경합이므로, 이제 순차로 다시 실행해 확정하겠습니다. LASAL 정적 gate는 SourceOnly/full 모두 통과했습니다.
>
> API 독립 검토에서 차단 결함은 없었습니다. 다만 Group async 실패, Axis sync typed 예외, 과잉 길이 lookup의 의미를 더 직접 고정할 수 있어 해당 테스트 세 가지까지 보강하겠습니다.
>
> <details><summary>Edited 9 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+12 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+9 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+9 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+5 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+9 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -5)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_SDO_WRITE_SAME_VALUE_QUALIFICATION_2026-07-28.md` (+2 -2)
>
> </details>
>
> 보강된 테스트 기준으로 최종 Debug/Release를 다시 확인합니다. 현재 예상 기준은 API 607/607, WPF 30/30입니다.
>
> 최종 회귀는 API 607/607, WPF 30/30으로 Debug/Release 모두 통과했습니다. 문서도 이 기준으로 맞췄고, 이제 whitespace·정적 계약·잔여 구현 항목을 다시 점검합니다.
>
