- LASAL SourceOnly/full 정적 계약: PASS
- WPF 격리 Debug/Release 빌드: PASS
- 일반 Debug 빌드는 실행 중인 WPF가 DLL을 잠가 복사 단계만 실패했습니다. 실행 프로그램은 종료하지 않았습니다.
- 테스트 폴더 복사, 커밋, staging은 하지 않았습니다.

현재 단계 guard는 [Verify-LasalContract.ps1](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1:2549)에 반영돼 있습니다.

<details><summary>93 previous messages</summary>

> IDE 구조가 아직 생성되지 않았는지 먼저 다시 확인하겠습니다. 없다면 다음 단계 implementation을 바로 붙일 수 있도록 RT snapshot/mailbox의 순수 계약 테스트와 verifier phase 구조를 구현해 두겠습니다. 테스트 폴더나 실행 중인 GUI는 건드리지 않습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> LASAL IDE 구조가 아직 추가되지 않은 상태에서도 진행 가능한 부분부터 이어가겠습니다. 우선 EtherCAT I/O의 1 ms 동작 규칙을 PC 참조 모델 테스트로 고정하고, 이후 IDE 구조가 들어오는 단계별로 정적 검증기가 바로 실패 원인을 구분하도록 확장하겠습니다. 이 테스트는 PLC 실기 검증과는 명확히 분리합니다.
>
> 참조 모델의 핵심은 세 가지로 고정하겠습니다. CREVIS 4바이트는 Byte0가 최하위 바이트이고, PREOP/SAFEOP의 “존재”와 OP 상태의 “유효 PDO”를 분리하며, 출력은 nonzero revision 기반 CAS와 단일 mailbox로만 변경합니다. 특히 invalid 응답에서도 SDK 계약상 output revision은 0이 될 수 없도록 초기값을 1로 둡니다.
>
> <details><summary>Created a file, edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/EtherCATIoRtReferenceModelTests.cs` (+603 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
>
> </details>
>
> 첫 컴파일에서 테스트 코드의 named argument 하나가 실제 매개변수명과 달라 실패했습니다. 동작 설계 문제가 아니라 테스트 작성 오류이므로 바로 수정하고 재실행합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/EtherCATIoRtReferenceModelTests.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+38 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+126 -17)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+70 -2)
>
> </details>
>
> 검증기는 이제 `StaticTopologyOnly → IntegratedReadOwner → IntegratedOutputOwnerDormant` 세 단계로 구분됩니다. 현재 저장소는 첫 단계이므로 기존 상태가 그대로 PASS해야 하며, 이후 단계에서는 CREVIS client 3개·464바이트 seqlock·진단 helper·단일 mailbox를 각각 강제합니다. 먼저 기본 단계 회귀가 깨지지 않았는지 확인하겠습니다.
>
> 출력 쓰기는 구현이 들어와도 즉시 활성화되지 않도록 PLC 쪽 global/per-node gate와 정확한 GT-22BA target/mask 상수를 먼저 고정하겠습니다. 현재는 두 gate 모두 `FALSE`이고 SDK allowlist도 비어 있어 실제 `0x7E23` 송신은 계속 차단됩니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+5 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+34 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+31 -9)
>
> </details>
>
> 참조 모델과 단계 검증기는 기본 checkpoint에서 정상 통과했습니다. 다음으로 별도 검토를 병행해 revision/quality/mailbox 규칙에 빠진 경계가 없는지 확인하고, 저는 Release 및 full LASAL static contract를 실행하겠습니다.
>
> 로컬 SIGMATEK enum을 다시 대조하니 참조 모델 상수가 잘못됐습니다. 실제 값은 `_ClassOk=0`, `_NoHardware=5`인데 테스트 모델이 임의 값으로 되어 있었습니다. 테스트 자체가 잘못된 상태였으므로 실제 헤더 값으로 즉시 수정합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/EtherCATIoRtReferenceModelTests.cs` (+2 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+22 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+0 -20)
>
> </details>
>
> 별도 검토에서 세 가지 안전 공백이 확인됐습니다. node health의 missed-frame freshness, mailbox의 publish/consume/completion 원자성, disabled write gate의 실제 control-flow 차단입니다. 이건 테스트 강도 문제라 바로 보완하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/EtherCATIoRtReferenceModelTests.cs` (+153 -19)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/EtherCATIoRtReferenceModelTests.cs` (+14 -14)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/EtherCATIoRtReferenceModelTests.cs` (+15 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+31 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+90 -14)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+154 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+19 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+20 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/EtherCATIoRtReferenceModelTests.cs` (+19 -0)
>
> </details>
>
> 보완 내용은 실제 LASAL 구현에 직접 걸리도록 만들었습니다. 네 byte 각각의 masked read/write, source 연결 guard, master missed-frame/identity quality, output revision 전이, request/completion publish 순서, 실패 completion까지 단계 검증기가 요구합니다. 기본 static 단계와 PC 회귀를 다시 돌립니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 소스 기준으로 마지막 정적 검증을 다시 돌린 뒤, 실제 CREVIS 표시가 막힌 지점과 다음 LASAL IDE 구조 작업을 정확히 분리하겠습니다. 실행 중인 WPF/LASAL은 건드리지 않고 마스터 소스만 확인합니다.
>
> 검증 스크립트 호출 인자가 현재 파일의 실제 정의와 달라 한 번 실패했습니다. 소스 문제는 아니며, 매개변수 정의를 확인해 올바른 명령으로 즉시 재실행하겠습니다.
>
> 정적 검사 자체가 아니라 테스트 실행 방식이 프로젝트 형식과 맞지 않아 묶음 실행이 중단됐습니다. 기존 .NET Framework 테스트 실행 파일을 직접 사용하고, 각 결과를 분리해 확인하겠습니다.
>
> PC 계약은 Debug/Release 모두 314/314, LASAL 현재 단계 정적 계약도 통과했습니다. 이제 GUI가 실제로 어떤 경로에서 CREVIS를 표시하고 SDO 편집 잠금을 해제하는지 소스까지 다시 대조하고 있습니다.
>
> 현재 실행 중인 GUI는 잠긴 기존 Debug DLL을 사용 중일 가능성이 큽니다. 프로세스는 종료하지 않고, 최신 소스를 별도 출력 폴더로 빌드해 바로 비교 실행할 수 있는 바이너리를 만들겠습니다.
>
> 원인은 확인됐습니다. 지금 실행 중인 GUI는 17:19 빌드이고, CREVIS/SDO 편집 수정 소스는 20:16 이후라 실행 바이너리에 아직 들어가 있지 않습니다. 최신 별도 빌드는 21:20에 성공했으며 기존 프로세스는 그대로 두었습니다.
>
> 최종 리뷰에서 정적 검증이 놓칠 수 있는 경쟁 조건 3건이 발견됐습니다. 실제 PLC 코드를 무리하게 앞당기지 않고, 지금 만든 reference model/verifier를 먼저 보강한 뒤 314개 테스트와 정적 계약을 다시 돌리겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/EtherCATIoRtReferenceModelTests.cs` (+15 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+20 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+12 -3)
>
> </details>
>
> 리뷰 범위를 넓혀보니 live read/write 구현 전에 정적 gate가 더 엄격해야 했습니다. 연결 끊김 시 이전 PDO를 재사용하는 경우, snapshot publish 순서, handler의 모든 wire offset과 cancel CAS 결과까지 검증하도록 보강 중입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+98 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+28 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+8 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+104 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+67 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+25 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+8 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+9 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+22 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -0)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+9 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
>
> </details>
>
> 최신 GUI를 기존 실행 프로세스와 분리해 마스터 저장소의 무시되는 `bin/CodexLatest` 경로에도 빌드합니다. 사용자가 기존 GUI를 종료할 때 정상 Debug 폴더를 덮어쓰지 않고 바로 최신 화면을 확인할 수 있게 하려는 조치입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 GUI 문제는 실행 중인 파일이 수정 전 바이너리인 점과 테스트 PLC 쪽 소스가 구버전인 점이 겹쳐 있습니다. 우선 SDO 편집 잠금 수정과 CREVIS 표시 코드가 들어간 별도 빌드는 확보했고, 지금은 출력 쓰기 계약에서 남은 세 가지 안전성 검증까지 보강한 뒤 전체 테스트를 다시 돌리겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+59 -12)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+11 -1)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+46 -24)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+7 -0)
>
> </details>
>
> 안전 검증 쪽에서는 추가 결함도 하나 확인했습니다. 확장 snapshot이 “올바른 offset에 한 번 쓰였는지”만 보던 상태라 CREVIS coupler/slot과 I/O 값이 서로 뒤바뀌어도 통과할 수 있었고, 지금은 각 offset의 실제 원본 변수까지 고정하도록 보강했습니다.
>
> PC 테스트 314개 자체는 모두 통과했습니다. 전체 Debug 타깃은 사용자가 실행 중인 기존 GUI가 `bin\Debug` DLL을 잡고 있어서 마지막 WPF 복사 단계만 실패했습니다. 그 프로세스는 건드리지 않고, 이후 검증은 별도 출력 폴더와 WPF를 제외한 PC 테스트 타깃으로 계속하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+37 -18)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+101 -18)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+42 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+39 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+43 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+50 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+29 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+30 -5)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+20 -7)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+4 -3)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 두 현상은 소스 결함과 실행 중인 구버전의 영향을 분리해서 확인하겠습니다. 현재 수정본 기준으로 CREVIS 표시 경로, SDO Write 입력 잠금, LASAL 정적 계약을 다시 검증하고 남은 구현을 계속 진행하겠습니다. LASAL IDE나 테스트 폴더는 건드리지 않겠습니다.
>
> 현재 작업트리에는 CREVIS GUI와 SDO 편집 잠금 수정이 들어가 있습니다. 다만 실행 중인 GUI와 테스트 PLC 소스가 수정 시점보다 오래된 상태라, 최신 소스 검증과 “실행본 교체가 필요한 문제”를 구분해 확인 중입니다.
>
> 정적 계약은 현재 단계에서 정상입니다. 아직 IDE 구조가 없는 `IntegratedReadOwner`/`IntegratedOutputOwnerDormant` 단계는 의도대로 각각 `0x7E13/0x7E22` 및 `0x7E23` 미등록 때문에 거부되어, 미완성 코드를 완료로 오인하지 않습니다. 이제 C# Debug/Release 회귀를 다시 돌립니다.
>
> C# Debug/Release는 각각 315/315 통과했고, 최신 WPF 실행본도 기존 실행 프로세스를 종료하지 않은 채 별도 출력 폴더에 다시 빌드했습니다. 현재 실행 중인 17:19 버전은 그대로 두었으므로 사용자의 디버깅 세션에는 손대지 않았습니다.
>
> 최종 검토에서 한 가지 계약 불일치를 찾았습니다. PREOP/SAFEOP의 물리 노드는 “검출됨”이어야 하는데 verifier가 `nativeOnline`까지 검출 조건에 묶고 있었습니다. 이 조건은 데이터 유효성에만 남기도록 바로 수정합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+0 -1)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+75 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+24 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+13 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+32 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+21 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+21 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+25 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+30 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+5 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+42 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+37 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+36 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+12 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+72 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+34 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+43 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+5 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -3)
