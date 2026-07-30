>
> Source: automatic
> Status: completed
>
> D4 복구 흐름 구현은 계속 진행 중입니다. 현재 자동 복구를 잘못된 설정에 연결할 수 있는 세 가지 안전성 문제를 우선 막고 있습니다: 검증 없는 journal 종료, inventory에서 사라진 bank를 해제된 것으로 간주하는 동작, Configure 응답이 유실된 상태에서 revision 0만으로 기존 설정을 채택하는 문제입니다. 이 세 항목을 fail-closed로 고친 뒤 전체 테스트와 문서를 맞추겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleQualificationJournalBridge.cs` (+57 -32)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryJournal.cs` (+25 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryPlanner.cs` (+28 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleQualificationJournalBridgeTests.cs` (+17 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryJournalTests.cs` (+4 -24)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryPlannerTests.cs` (+50 -42)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryOrchestrator.cs` (+50 -106)
>
> </details>
>
> 복구 검토에서 나온 P1 두 건은 코드에서 차단했습니다. 이제 journal은 실제 release 완료 상태를 검증한 proof 없이는 종료할 수 없고, durable bank가 inventory에서 사라지면 정상 해제로 추정하지 않고 즉시 중단합니다. Configure 응답이 없어 revision이 0인 경우도 자동 채택하지 않도록 닫았으며, wire에 recovery nonce가 추가되기 전까지는 수동 확인 대상으로 남깁니다. 현재는 복구 결과가 실제 API release handle을 보존하도록 구조를 바꾸는 중입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+56 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+2 -2)
>
> </details>
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+29 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryJournalTests.cs` (+37 -32)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryJournal.cs` (+15 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryOrchestrator.cs` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryJournalTests.cs` (+19 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderModels.cs` (+1 -1)
>
> </details>
>
> 추가로 pre-dispatch revision 0 경로를 더 닫았습니다. 이제 그 상태에서는 0x7E4A조차 전송하지 않고, journal의 `RecordInventory`·`RecordCaptureReply` 직접 호출도 파일을 한 바이트도 바꾸지 못합니다. Debug API 485/485, WPF smoke 6/6까지 다시 통과했습니다. 다음으로 release 응답 유실 시 handle을 재사용 가능 상태로 잘못 되돌리는 문제를 보강하고 있습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderModels.cs` (+102 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorder.cs` (+44 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleQualificationJournalBridgeTests.cs` (+31 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleQualificationJournalBridge.cs` (+101 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryJournal.cs` (+22 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+7 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryOrchestrator.cs` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleQualificationJournalBridge.cs` (+146 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+92 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 0x7E4A/4B 정적 계약과 파서 음수 검증은 완료됐습니다. 지금은 방금 추가한 복구 결과 기반 해제 경로를 실제로 빌드해 확인하고, 마지막 원격 해제 성공 직후 프로세스가 죽는 내구성 구간도 함께 막겠습니다.
>
> 현재 API 488/488, WPF smoke 6/6까지 통과했습니다. CREVIS 표시는 자동 로드·수동 Reload 모두 확인됐고, SDO Write 중 입력 편집도 회귀 없이 유지됩니다. 지금 남은 핵심은 Double-bank 해제 응답 유실과 “PLC 해제 성공 직후 앱 종료” 구간이라, 이 부분은 capability를 계속 끈 상태에서 내구 journal까지 보강 중입니다.
>
> 해제 쪽은 단순 예외 처리만 바꾸는 것으로 끝내지 않고 있습니다. 각 bank의 `release intent`를 디스크에 먼저 기록하고 ACK 후 `confirmed`를 기록하는 형태로 journal을 확장 중이며, 응답이 끊기면 같은 handle을 재사용하지 못하게 하는 SDK 회귀도 병행하고 있습니다. 최종 configuration 부재는 현재 0x7E4A 오류만으로 정확히 증명할 수 없어서, 그 경우에는 자동 해결하지 않고 계속 fail-closed로 남깁니다.
>
> 현재 response-loss 회귀 5개를 추가했고, 해당 경로들은 sync/async 모두 통과했습니다. 전체 수치는 journal 전환 중이라 일시적으로 493개 중 4개가 깨져 있는데, 새 오류가 아니라 기존 exact-release 테스트를 durable intent 절차로 바꾸는 과정에서 발생한 예상 실패입니다. journal 전환이 끝나면 전체를 다시 한 번에 검증하겠습니다.
>
> 복구 절차를 더 구체화했습니다. bank 해제 intent가 남은 경우에는 exact 0x7E4A inventory에서 그 bank가 실제로 사라진 경우만 confirmed로 승격합니다. configuration 해제 intent는 0x7E4A에서 동일 configuration이 아직 존재할 때만 재-adopt 후 0x7E48을 다시 보내고, not-found나 identity 불일치는 자동 해결하지 않습니다. 0x7E4B 응답 유실 후 재접속 경로도 별도 통합 테스트로 추가 중입니다.
>
> 0x7E4B 응답 유실 통합 경로까지 추가됐습니다. 첫 적용 후 연결은 Faulted가 되고 같은 세션 재시도는 0-wire, 재접속 뒤 새 0x7E4A가 돌려준 previous-owner 값으로만 재-adopt한 다음 정상 해제하는 흐름이 Debug/Release에서 통과했습니다. 지금은 durable journal의 기존 테스트들을 새 intent/confirmed 상태 전이에 맞춰 마무리하는 중입니다.
>
> 추가 검토에서 두 가지 재시작 구간도 잡았습니다. `intent 저장 후 RPC 송신 전 종료` 때 bank가 그대로 남아 있으면 exact presence를 근거로 재채택·재해제를 허용하고, configuration release intent가 있는데 occupied bank가 다시 보이면 어떤 Adopt도 보내기 전에 모순으로 차단하도록 수정 중입니다. 이 두 경로가 빠지면 journal만 생기고 영구 복구가 막히므로 테스트까지 같이 넣고 있습니다.
>
> durable journal 전환이 끝났고 API 전체 500/500이 한 차례 통과했습니다. 사용자가 실행 중인 GUI 때문에 기본 `bin\Debug`만 잠겼으므로 그 프로세스는 건드리지 않고, 별도 출력 폴더로 Debug/Release·WPF smoke·LASAL 정적 계약을 다시 검증하겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+12 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+9 -7)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+17 -7)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+14 -13)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+6 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+7 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -0)
>
> </details>
</details>

반영 완료했습니다.

- CREVIS 정보: 연결 직후 자동 로드되며, capability가 늦게 활성화된 경우 `Reload`가 capability부터 다시 읽습니다. [MainWindow.TopologyIo.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs:828)
- SDO Write: 일반 요청 진행 중에도 다음 Slave/Index/SubIndex/Type/Value를 편집할 수 있고, Submit만 직렬화됩니다. Exact Write readback 중에만 편집이 잠깁니다. [SdoEditorAvailabilityPolicy.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/SdoEditorAvailabilityPolicy.cs:3)
- Recorder 해제 응답 유실 시 handle을 `OutcomeUnverified`로 고정해 파괴적 재시도를 막았습니다.
- Double-bank 복구 journal v2와 `0x7E4A/0x7E4B` 재접속·해제 crash-window 복구도 구현했습니다. [RecorderDoubleDurableReleaseCoordinator.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleDurableReleaseCoordinator.cs:8)

검증 결과:

- API Debug/Release: 각각 `500/500 PASS`
- WPF Debug/Release: 각각 `6/6 PASS`
- LASAL SourceOnly/full static: PASS
- LASAL custom source ASCII: PASS
- `git diff --check`: PASS
- 최종 안전성 검토: P0/P1/P2 발견 없음

실행 중인 기존 GUI는 종료하지 않았습니다. 현재 실행 파일에는 새 코드가 반영되지 않았으므로 최신 빌드로 재시작해야 CREVIS 변경을 확인할 수 있습니다.

아직 실제 PLC 증거는 아닙니다. CREVIS live I/O bit 15~17, Recorder Double bit 6/count 2, SDO Write target gate는 LASAL 빌드 및 실기 검증 전까지 계속 OFF입니다. 커밋·스테이징도 하지 않았으며, 기존 staged history 파일의 trailing whitespace 때문에 `git diff --cached --check`만 별도로 실패합니다.

<details><summary>28 previous messages</summary>

> 이어서 다음 구현 증분을 선정하겠습니다. 현재 소스와 구현 계획을 다시 대조해 하드웨어 없이 진행 가능한 항목을 먼저 잡고, Double-bank의 `ConfigRevision=0` 복구 provenance와 CREVIS 동적 I/O 쪽 남은 구조를 병렬 점검하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 다음 구현 단위를 바로 고르겠습니다. 현재 계획서와 실제 소스를 다시 대조해, LASAL IDE 구조 변경 없이 끝낼 수 있는 항목과 IDE 작업이 필수인 항목을 분리한 뒤 코드부터 진행하겠습니다.
>
> 현재 T2는 문서상의 선행 LASAL IDE 구조(`Coupler/InputSlot/OutputSlot` client와 helper 선언)가 실제 프로젝트에 아직 없습니다. 이 상태에서 외부 편집으로 `0x7E13/0x7E22`를 억지로 넣으면 생성 메타데이터와 어긋나므로, 그 경로는 건드리지 않고 D4의 남은 복구 구멍 중 선언 변경 없이 닫을 수 있는 항목을 확인 중입니다.
>
> 감사 결과를 합치면 새 명령을 먼저 만들 필요가 없습니다. 기존 `0x7E4A`가 “동일 BootId에서 Recorder 전체가 canonical Empty”인 경우에만 전용 detail code 32를 반환하도록 하면, 마지막 `0x7E48` 응답 유실을 추가 mutation 없이 정확히 종료할 수 있습니다. `0x7E4C~0x7E4D`는 이후 token-qualified Configure 복구용으로 비워 두겠습니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsModels.cs` (+27 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcErrorCatalog.cs` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderProtocol.cs` (+31 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st` (+43 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleQualificationJournalBridge.cs` (+46 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryJournal.cs` (+74 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryOrchestrator.cs` (+69 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+130 -0)
>
> </details>
>
> LASAL 쪽은 `0x7E4A`가 `ConfigRevision=0`일 때 절대 absence proof를 내지 않고, known revision + 같은 BootId + 전체 store canonical Empty일 때만 detail 32를 냅니다. 이제 C#에서는 이 결과를 일반 실패와 분리해 durable journal을 mutation 0회로 종료하도록 연결하고 있습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+180 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsProtocol.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ErrorCatalogTests.cs` (+1 -1)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 502/505까지 올라온 상태입니다. 남은 3건은 새 detail code 32가 정상 코드가 되면서, 기존 테스트가 32를 “알 수 없는 코드”로 쓰던 충돌입니다. 그 부분을 33으로 바로잡고 전체 검증까지 이어가겠습니다. T2 동적 I/O는 LASAL IDE 구조가 준비되기 전에는 반쪽 구현을 넣지 않겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsContractTests.cs` (+4 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsParserDeterministicFuzzTests.cs` (+1 -1)
>
> </details>
>
> API Debug 전체가 이제 505/505로 통과했습니다. 다음은 LASAL 쪽 0x7E4A가 정말 “동일 Boot/Map/ConfigRevision + 완전 Empty”일 때만 detail 32를 반환하는지 정적 계약을 추가하고, Release/WPF 빌드까지 확인하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
>
> </details>
>
> 정적 계약과 full LASAL 검사도 통과했습니다. 단, 외부 편집 상태라 `Classes.lcb/Networks.lcb` 동기화 검사는 명시적으로 우회한 결과이며 LASAL IDE 빌드 증거는 아닙니다. Release API 505/505, WPF Debug/Release 각 6/6도 통과했습니다. 이제 문서를 실제 상태에 맞춰 정리하겠습니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+7 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+13 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+6 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+6 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+8 -7)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+5 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+9 -1)
>
> </details>
>
> 이번 증분은 코드·문서·검증까지 닫혔습니다. 추가 리뷰에서도 P0~P2 문제는 없었습니다. CREVIS 실시간 값은 GUI 문제가 아니라 LASAL에 T2 입력 소유자 구조가 아직 없어서 막혀 있으며, 그 구조는 IDE에서 선언/네트워크 생성이 먼저 필요합니다. 현재 변경분의 최종 상태와 다음 개발 항목을 분리해 정리하겠습니다.
>
> 다음 코드-only 증분으로 바로 이어갑니다. `ConfigRevision=0`(Configure 성공 응답 자체가 유실된 경우)을 해결하려면 기존 identity만으로는 안전하지 않으므로, 0x7E4C/0x7E4D에 client-generated recovery token을 넣는 방식으로 설계를 고정한 뒤 구현하겠습니다. Double gate와 live WPF는 계속 끈 상태로 유지합니다.
>
> 다음 증분 계약은 정리됐습니다. 핵심은 기존 `0x7E40` Double을 끝까지 금지하고, `0x7E4C`에서 128-bit 일회성 token을 Configure와 함께 저장한 뒤 `0x7E4D`는 그 token으로만 read-only 조회하는 방식입니다. 응답 유실 시 4C는 절대 재전송하지 않고 4D만 반복 가능하게 하며, reboot·token 불일치·foreign owner·Fault는 모두 fail-closed로 유지합니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+34 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+13 -9)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+5 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -5)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+7 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+6 -1)
>
> </details>
>
> 구현은 세 갈래로 진행 중입니다. LASAL은 token 저장·release 시 tombstone·4D read-only 검증, SDK는 4C/4D exact byte 계약과 별도 typed absence, PC recovery는 v2/v3 구분과 `4D → durable identity → 4A → 4B` 순서를 각각 작업하고 있습니다. 기존 bit 6/count 2/live WPF gate는 변경하지 않습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 지금 요청한 두 GUI 문제를 막는 기반 작업까지 함께 묶어 진행 중입니다. CREVIS 실시간 데이터는 GUI만의 문제가 아니라 LASAL의 T2 연결 구조가 아직 없는 것이 원인으로 확인됐고, 현재는 먼저 진행 중인 Recorder 복구 API 증분을 통합·검증한 뒤 해당 T2 구조를 사용자가 LASAL IDE에서 생성할 수 있는 정확한 빌드 단계로 넘기겠습니다. SDO Write 편집 잠금 수정은 기존 변경이 유지되는지 회귀 테스트에 포함하겠습니다.
>
> LASAL 쪽 신규 복구 명령 `0x7E4C/0x7E4D`는 구현과 정적 검증까지 통과했습니다. 현재 SDK와 내구 저널을 맞추는 중이며, 정상 `0x7E40` 경로와 기능 비트는 열지 않아 기존 장비 동작에는 노출되지 않습니다. CREVIS는 GUI 표시 코드 자체보다 PLC의 `0x7E13/22/23` 실데이터 공급 경로가 빠져 있는 상태라, 그 부분은 “표시됐다고 가장하는 수정” 없이 별도 IDE 구조 작업으로 남겨 두고 있습니다.
>
> 통합 검토에서 실제 재사용 방지 결함 하나를 잡았습니다. 토큰 기반 구성을 해제한 뒤 일반 Recorder 구성을 한 번 해제하면, 기존 토큰 tombstone이 0으로 덮여 재사용될 수 있었습니다. 지금 “활성 토큰이 0이 아닐 때만 tombstone 갱신”으로 고치고 해당 순서를 정적 검증에 추가하고 있습니다. 아직 기능 비트가 꺼져 있어 현재 PLC 노출은 없지만, 켜기 전에 반드시 막아야 하는 결함입니다.
>
> 세 갈래 구현이 합쳐졌고 Debug 계약 테스트는 현재 516/516까지 통과했습니다. 이제 제가 통합 상태에서 Debug/Release, WPF 실제 컨트롤 smoke, LASAL 전체 정적 계약을 다시 돌려 서로의 변경이 섞였을 때도 깨지지 않는지 확인하겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
>
> </details>
>
> 최종 교차 검토에서 SDK 우회 경로 하나를 추가로 찾았습니다. 사용자가 `0x7E4D` 결과 객체를 직접 기존 `AdoptEmptyRecorderConfiguration`에 넘기면, 의도한 “저널에 ConfigRevision 저장 → 표준 `0x7E4A` 재확인” 순서를 건너뛸 수 있었습니다. 공개 API와 raw frame 양쪽에서 recoverable inventory를 선송신 차단하고 zero-wire 회귀 테스트를 추가한 뒤 전체 검증을 다시 돌리겠습니다.
>
> <details><summary>Edited 10 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorder.cs` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderProtocol.cs` (+6 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcErrorCatalog.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+19 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+37 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
