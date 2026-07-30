> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
>
> </details>
>
> WPF는 별도 출력 Debug/Release 모두 성공했고, 실제 배포용 `bin\Release`도 정상 갱신됐습니다. 실행 중인 Debug GUI는 이전 바이너리이므로 새 SDO/Topology 동작을 보려면 나중에 재실행해야 합니다. 다음으로 LASAL은 IDE를 건드리지 않고 추적 소스 정적 계약만 검사합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -0)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+8 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+3 -0)
>
> </details>
>
> 정적 계약은 SourceOnly/full 모두 PASS했습니다. 전체 `git diff --check`는 이번 코드가 아니라 이미 staging된 원본 히스토리 파일의 보존된 trailing whitespace 때문에 실패합니다. 원본 히스토리는 수정하지 않고, 해당 파일을 제외한 현재 개발 변경만 따로 검사해 결과를 분리하겠습니다.
>
> 현재 요청 범위의 코드·문서·빌드 검증은 끝났습니다. 실시간 CREVIS 경로는 LASAL IDE 선언이 선행돼야 하므로 그 부분을 억지로 생성하지 않겠습니다. 그 대기와 병행해, 이미 남아 있던 SDO/출력 Write의 강제 종료·전원 손실 복구용 durable journal을 PC 측에서 계속 구현하겠습니다. 이 작업은 PLC 명령이나 LASAL IDE를 건드리지 않습니다.
>
> 독립 리뷰에서 실제 안전 결함 1건이 잡혔습니다. 요청 자체는 클릭 시 고정되지만 확인창만 현재 TextBox 값을 다시 읽고 있어, preflight 대기 중 값을 바꾸면 “화면에서 확인한 값”과 “실제 송신되는 고정 값”이 달라질 수 있었습니다. 확인창도 요청 snapshot의 바이트에서 값을 표시하도록 수정합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+37 -2)
>
> </details>
>
> SDO 확인창까지 요청 snapshot 기준으로 수정했고 독립 리뷰에서 추가 P0–P2 결함이 없음을 확인했습니다. durable journal 저장 계층은 현재 별도 테스트와 함께 작성 중이며, 완료되면 먼저 저장 자체의 crash/lock/corruption 동작을 검증한 뒤 UI 흐름에 연결하겠습니다.
>
> durable journal helper와 핵심 6개 테스트 소스는 작성됐고 현재 프로젝트 등록·Debug 컴파일을 확인 중입니다. 저장 파일은 한 개의 활성 mutation만 허용하고, checksum 손상이나 두 번째 writer는 조용히 무시하지 않고 fail-closed하도록 구성하고 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 확인 결과, GUI 파서 문제가 아니라 현재 PLC 쪽에 CREVIS 실시간 I/O 서비스와 `0x7E13/0x7E22/0x7E23` 처리가 아직 없어서 Health/DI/DO 값이 올라올 수 없는 상태입니다. 우선 PC 쪽은 CREVIS 표시·출력 안전장치와 SDO Write 편집 문제를 반영했고, 지금은 중단/재접속에도 쓰기 결과를 놓치지 않도록 영속 상태 기록까지 연결한 뒤 전체 빌드·테스트를 다시 돌리겠습니다. LASAL IDE 구조 생성이 필요한 부분은 정확한 생성 목록으로 분리해 드리겠습니다.
>
> PC 쪽 쓰기 경로는 이제 “전송 직전 기록 → 수락 티켓 기록 → 종결/정확 재읽기 기록” 순서로 디스크에 남도록 연결합니다. 앱이 중간에 종료돼도 다음 실행에서 자동 재전송하지 않고, 남은 기록을 GUI에 표시해 물리 확인 전에는 새 쓰기와 동작을 막는 방식입니다. SDO 입력창은 전송 중에도 다음 값을 편집할 수 있지만, 실제 전송 바이트는 확인 시점의 스냅샷으로 고정됩니다.
>
> <details><summary>Created a file, edited 7 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+508 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+24 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+29 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+94 -42)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+56 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+26 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+4 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+32 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+12 -1)
>
> </details>
>
> 첫 WPF Release 빌드는 통과했습니다. 이제 영속 기록을 연결한 뒤에도 기존 PC 계약 테스트 304개가 Debug/Release 모두 유지되는지 확인하고, 별도 리뷰로 “전송됐는데 기록이 풀리는 경로”나 “기록 때문에 앱이 닫히지 않는 경로”가 없는지 점검 중입니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+42 -30)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+5 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+8 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+4 -3)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> PC 쪽 수정은 이미 들어갔고, 지금은 최종 통합 리뷰와 문서·빌드 검증을 마무리하겠습니다. 핵심은 두 가지입니다: SDO Write 중 편집 가능 상태를 유지하면서 실제 전송값은 스냅샷으로 고정하고, CREVIS는 현재 가능한 정적 토폴로지 표시와 아직 LASAL 실시간 서비스가 필요한 영역을 정확히 분리하겠습니다.
>
> 최종 리뷰 중 한 가지 예외 경로를 먼저 보강하겠습니다. 전송 전에 가드 설정이 실패했을 때, 인메모리 가드 해제 실패가 원래 오류나 영구 저널 정리 오류를 가리지 않도록 세 오류를 함께 보존하는 수정입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+15 -3)
>
> </details>
>
> 통합 리뷰에서 실제로 고쳐야 할 두 안전 결함이 확인됐습니다. 하나는 취소된 SDO Write 티켓의 영구 기록이 남아 현재 프로세스가 영구 잠기는 경우이고, 다른 하나는 저널 손상/두 번째 실행 시 Write는 막히지만 모션 명령이 열릴 수 있는 경우입니다. 둘 다 Stop/PowerOff와 정상 종료는 유지하면서 보강하겠습니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+11 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+139 -53)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+21 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+17 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+4 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+11 -10)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs` (+4 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+31 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+2 -2)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -1)
>
> </details>
>
> <details><summary>Edited 9 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+12 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+13 -13)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+13 -13)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+2 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+47 -13)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -1)
>
> </details>
>
> 이제 PC 재빌드와 동시에 LASAL 쪽은 IDE를 건드리지 않고 정적 계약만 다시 확인합니다. CREVIS 실시간 값은 아직 새 LASAL RT 서비스가 없어서 확인할 수 없지만, 현재 정적 토폴로지 `0x7E11/0x7E12`와 기존 D5 계약이 깨지지 않았는지는 여기서 검증할 수 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsMutationJournalTests.cs` (+23 -2)
>
> </details>
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+6 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -1)
>
> </details>
</details>

반영했습니다.

- SDO Write 선택/송신 중에도 Slave/Index/SubIndex/Type/Length/Value를 편집할 수 있습니다. 실제 송신값은 클릭 시점 스냅샷으로 고정됩니다. [MainWindow.Diagnostics.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:1828)
- `Load CREVIS / Topology`가 capability를 자동 갱신하고 `GL_9086_11`, Input/Output Slot 등 configured CREVIS 항목을 표시합니다. [MainWindow.TopologyIo.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs:152)
- SDO/output Write의 강제 종료·재시작 복구용 durable journal과 안전 interlock도 반영했습니다.
- 취소·재조회·readback 실패 시 남던 deadlock과 상태 재전이 문제까지 최종 리뷰에서 수정했습니다.

검증 결과:

- WPF Debug/Release: PASS
- PC 자동 테스트: Debug/Release 각각 `304/304 PASS`
- LASAL SourceOnly/full static contract: PASS
- 실제 LASAL IDE 빌드·다운로드와 장비 시험은 수행하지 않았습니다.

현재 정확한 제한 사항:

- 지금 GUI에서 보이는 CREVIS 정보는 configured topology입니다.
- 실시간 Health/DI/DO는 아직 안 나옵니다. PLC `0x7E13/0x7E22/0x7E23`과 capability bit 15~17이 아직 미구현입니다.
- 다음 개발을 위해 LASAL IDE에서 `LMCEtherCATIoService1` class/object, client와 Network 연결을 먼저 생성해야 합니다. 필요한 선언은 [설계 문서](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md:562)에 정리했습니다. 생성·저장 후 알려주면 외부 `.st` 편집으로 실시간 CREVIS I/O 구현을 이어가겠습니다.
- SDO Write 실제 활성화는 시험할 축 1~4 중 하나와 `UI[24] (0x2F00:24)` 미사용 여부 확인이 필요합니다. 현재 gate는 계속 OFF입니다.

커밋이나 테스트 폴더 복사는 하지 않았습니다. 기존에 staged된 대형 history 파일의 trailing whitespace도 건드리지 않았습니다.

<details><summary>12 previous messages</summary>

> 계속 진행하겠습니다. 우선 현재 작업 트리에 `LMCEtherCATIoService` 구조가 새로 생겼는지 다시 확인하고, 없더라도 IDE 구조 생성 전까지 구현 가능한 PLC wire/검증 코드와 PC 계약을 최대한 선행하겠습니다. LASAL IDE는 제어하지 않고 외부 소스만 다룹니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 계속 구현합니다. 먼저 남은 LASAL 오류 방지 가이드와 현재 진단 서비스 구조를 확인한 뒤, 새 IDE 클래스가 없어도 선행 가능한 부분과 IDE 생성 후 바로 붙일 부분을 분리해서 진행하겠습니다. CREVIS 실시간 서비스 구조는 수동으로 위조하지 않겠습니다.
>
> 소스 구조 분석은 병렬로 돌립니다. 한 쪽은 CREVIS 클래스의 실제 채널/메모리 계약, 한 쪽은 PLC 진단 서비스 삽입 지점, 한 쪽은 PC 계약 테스트의 빈틈을 각각 확인하게 하고 저는 설계 문서와 현재 구현을 직접 대조하겠습니다.
>
> 중요한 구조 문제를 하나 확인했습니다. 기존 `_LMCAxis1.LMCPreRtWorkTrigger`는 이미 `LMCEcatInputLatch1`에 연결돼 있어 새 서비스에 다시 직접 연결하면 기존 진단 래치를 끊게 됩니다. 이 연결은 유지하고, 기존 래치가 새 I/O 서비스를 RT cycle 안에서 호출하는 체인 구조로 설계를 바로잡은 뒤 구현 계약을 고정하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+20 -9)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+26 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+5 -1)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+8 -0)
>
> </details>
>
> 현재 소스를 더 대조해 보니 새 클래스를 만드는 것보다 기존 `LMCEcatInputLatch`를 CREVIS RT owner로 확장하는 편이 낫습니다. 이 클래스는 이미 1 ms pre-RT trigger, 512-byte seqlock storage, Drive 1~4/master snapshot을 갖고 있고 실제 사용 구간은 304 bytes뿐이라, 남은 영역에 coupler/slot/I/O 상태를 넣으면 기존 연결과 `0x7E10`을 그대로 보존할 수 있습니다. 이 통합 구조로 IDE 추가 항목과 handler 분리를 정리 중입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+68 -50)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+27 -33)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+4 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+8 -8)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+5 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+20 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -3)
>
> </details>
>
> 통합 owner 설계와 현재 단계 guard를 반영했습니다. 지금은 새 CREVIS client나 live handler가 일부만 들어와 capability가 거짓 활성화되는 상태를 정적 검증에서 즉시 실패시키도록 했고, 기존 static topology 단계가 계속 통과하는지 검사합니다.
>
> 전체 Debug 검증에서 PC 테스트와 LASAL 정적 계약은 진행됐지만, 사용자가 실행 중인 WPF가 `bin\Debug\LasalMotionControlLib.dll`을 잡고 있어 마지막 WPF 복사 단계만 실패했습니다. 실행 중인 GUI나 디버거는 종료하지 않겠습니다. PC 테스트는 별도로 확정하고, WPF는 임시 출력 폴더로 격리 빌드해 검증하겠습니다.
>
> 현재 기대 동작도 구분해서 확인하겠습니다. bit 14만 올라간 PLC라면 “configured CREVIS topology row”는 보여야 하지만 node health/DI/output 값은 아직 안 보이는 게 정상입니다. 테스트 폴더의 LASAL source가 master의 `0x7E11/12` 단계와 같은지도 읽기 전용으로 대조하겠습니다.
</details>

직접 원인을 확인했습니다. 현재 테스트 폴더의 LASAL 소스에는 `0x7E11/0x7E12` 자체가 없습니다. 따라서 지금 PLC에서는 CREVIS topology row도 올라올 수 없습니다. GUI 문제가 아니라 테스트 폴더 코드가 master보다 이전 버전입니다.

이번에 반영한 내용:

- 새 RT 클래스를 만들지 않고 기존 `LMCEcatInputLatch`를 CREVIS RT owner로 확장하도록 설계를 수정했습니다.
- 기존 `_LMCAxis1.LMCPreRtWorkTrigger` 연결을 유지합니다.
- native `Online`은 장치 존재 여부가 아니라 OP 상태이므로, PREOP/SAFEOP 상태를 offline으로 오판하지 않도록 health 판정 규칙을 수정했습니다.
- `HandleRequest`가 32,768-byte 제한까지 302 bytes만 남아 있어 `0x7E11/12/13/22/23`을 별도 helper로 분리하도록 했습니다.
- 부분 구현 상태에서 bit 15~17이 잘못 켜지는 것을 막는 정적 검증을 추가했습니다.

정확한 IDE 입력 항목은 [설계 문서](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md:573)에 정리했습니다. 다음 작업만 해주세요.

1. `LMCEcatInputLatch`에 `Coupler`, `InputSlot`, `OutputSlot` required client와 문서의 변수·method를 추가
2. `LMCDiagnosticsService`에 `HandleEtherCATTopologyIoRequest` private method 추가
3. Motion Network에 문서의 CREVIS 연결 3개 추가
4. 기존 `_LMCAxis1.LMCPreRtWorkTrigger -> LMCEcatInputLatch1.ClassSvr`는 그대로 유지
5. LASAL에서 저장만 하고 알려주세요. 구현부는 제가 외부에서 작성한 후 빌드를 요청하겠습니다.

검증 결과:

- PC 테스트 Debug/Release: 각각 `304/304 PASS`
