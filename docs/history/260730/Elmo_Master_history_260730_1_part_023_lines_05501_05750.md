- WPF Debug/Release 빌드 PASS
- PC 테스트 Debug/Release 각각 `319/319 PASS`
- LASAL `StaticTopologyOnly` SourceOnly/full PASS
- `git diff --check` PASS
- 기존 staged 히스토리 파일의 공백 때문에 `git diff --cached --check`는 계속 실패하며, 해당 파일은 건드리지 않았다.

실제 CREVIS 상태/I/O 구현을 계속하려면 먼저 LASAL IDE에서 client·method 선언·network 연결을 생성해야 한다. 정확한 작업 목록은 [LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md)에 정리했다. 이 구조 생성과 LASAL 빌드가 끝나면 외부 편집으로 RT snapshot과 `0x7E13/0x7E22` 구현을 바로 이어가면 된다.

<details><summary>21 previous messages</summary>

> 먼저 master의 LASAL 구조가 사용자의 IDE 작업으로 바뀌었는지 다시 확인하겠습니다. 구조가 생성돼 있으면 바로 `0x7E13/0x7E22` 구현으로 들어가고, 아직 없으면 빌드를 깨지 않는 범위에서 동적 CREVIS 구현 코드와 검증 계약을 더 준비하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 말한 두 가지는 최신 소스에 반영되어 있습니다. 다만 지금 실행 중인 GUI는 7월 27일 빌드라 CREVIS 표시와 SDO 편집 개선이 보이지 않습니다. 실행 중인 프로그램은 건드리지 않고, 다음 동적 I/O 구현 구간을 계속 진행하겠습니다.
>
> LASAL 쪽은 IDE가 만들어야 하는 CREVIS client·method·Motion Network 연결이 아직 없어서, 그 선언을 손으로 합성하지는 않겠습니다. 대신 capability bit 15/16을 켜기 전에 실제 `0x7E13/0x7E22`를 검증할 수 있는 읽기 전용 raw 시험 도구가 비어 있는 것을 확인했고, 지금 그 구간을 구현 중입니다. `0x7E23` 출력 쓰기는 이 도구에서 명시적으로 차단합니다.
>
> 동적 read capability를 켜기 전 시험 경로는 GUI에 우회 버튼을 넣지 않고, 테스트 실행 파일의 별도 읽기 전용 모드로 분리합니다. 이 방식이면 production SDK의 capability 차단은 그대로 유지하면서도 PLC 구현 직후 raw frame과 응답 증거를 남길 수 있습니다.
>
> <details><summary>Created 2 files, edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationTool.cs` (+1193 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+8 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationToolTests.cs` (+264 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationTool.cs` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationToolTests.cs` (+315 -6)
>
> </details>
>
> <details><summary>Created a file, edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+17 -38)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/TOPOLOGY_IO_QUALIFICATION_TOOL_2026-07-28.md` (+92 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md` (+13 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+7 -2)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 확인할 핵심은 두 가지입니다. 최신 GUI에는 정적 CREVIS 표시와 SDO 편집 개선이 들어갔지만, 실행 중인 프로그램은 어제 빌드라 반영되지 않았습니다. 추가한 동적 I/O 판정 도구는 독립 리뷰까지 받은 뒤 결과와 남은 LASAL 작업을 정확히 구분해 정리하겠습니다.
>
> 한 가지 문서 불일치도 발견했습니다. WPF 설계 문서 한 곳이 이전 자동시험 수치인 `319/319`를 아직 현재값처럼 적고 있어, 리뷰 결과와 함께 `325/325`로 바로잡겠습니다. 기존 캡처 문서에 남은 옛 capability 값은 당시 실측 기록이므로 변경하지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
>
> </details>
>
> 독립 리뷰에서 실제로 보강해야 할 세 가지가 확인됐습니다. 토폴로지 청크의 전체 개수 일관성, I/O 응답의 NodeId 결속, 17개 raw read 전후의 DiagnosticsBootId 동일성을 추가해 서로 다른 PLC 실행 상태가 한 증거로 섞이지 못하게 막겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationTool.cs` (+54 -10)
>
> </details>
>
> 리뷰 범위를 더 확장했습니다. 잘못된 `0x7E11`이 최대 65,535회 요청을 유도할 수 있는 지연 검증과, 전원 차단 시 `.inprogress`에 프레임이 남지 않는 보고서 내구성 문제도 함께 수정합니다. 이 도구는 capability를 우회해 raw read를 허용하므로 일반 API보다 더 엄격하게 고정하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationTool.cs` (+99 -16)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationTool.cs` (+39 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationToolTests.cs` (+137 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationToolTests.cs` (+139 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationToolTests.cs` (+172 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationToolTests.cs` (+58 -0)
>
> </details>
>
> 보강한 6개 시험까지 포함해 Debug 전체가 `331/331 PASS`했습니다. 이제 문서의 현재 시험 수치와 CREVIS 모듈명 오타를 맞춘 뒤, Release·WPF 격리 빌드와 정적 LASAL 계약 검사를 다시 돌리겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/TOPOLOGY_IO_QUALIFICATION_TOOL_2026-07-28.md` (+3 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+5 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/TOPOLOGY_IO_QUALIFICATION_TOOL_2026-07-28.md` (+7 -3)
>
> </details>
>
> Release 전체도 `331/331 PASS`, 최신 GUI Debug/Release 재빌드와 `StaticTopologyOnly` SourceOnly/full 검사도 통과했습니다. 다음 단계인 동적 read-owner 검사는 예상대로 아직 LASAL IDE 구조가 없어 실패하는지 오류 지점을 별도로 확인하고 있습니다. 기존에 stage된 대형 히스토리 파일의 공백 오류는 이번 변경과 무관하며 건드리지 않겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationTool.cs` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationToolTests.cs` (+7 -0)
>
> </details>
>
> 모든 보강 후 Debug/Release가 다시 `331/331 PASS`했습니다. 마지막으로 같은 리뷰어에게 지적사항이 실제 수정본에서 모두 닫혔는지만 짧게 재검토시키고, 그 결과를 반영해 사용자 테스트 절차를 정리하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationTool.cs` (+21 -3)
>
> </details>
</details>

반영 완료했습니다. GUI에서 CREVIS가 안 보인 직접 원인은 현재 실행 중인 프로그램이 7월 27일 구버전이기 때문입니다.

- 기존 GUI는 종료하지 않았습니다.
- 최신 GUI: [LasalMotionControlApiExample.exe](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/CodexLatest/Debug/LasalMotionControlApiExample.exe)
- 최신 GUI 실행 후 `Connect` → `Load CREVIS / Topology`를 누르면 GL-9086, Elmo 4축, I/O 슬롯 2개 등 정적 7개 항목이 표시됩니다.
- CREVIS Node Health와 실제 DI/DO 값은 아직 LASAL의 `0x7E13/0x7E22`가 없어서 표시되지 않습니다. 버튼이 비활성화되는 것이 현재 정상입니다.

SDO Write는 다음처럼 변경했습니다.

- Write 요청 실행 중에도 다음 요청의 Slave/Index/SubIndex/Type/Data를 편집할 수 있습니다.
- 실행 중인 요청은 클릭 시점 값으로 복사되므로 이후 편집에 영향받지 않습니다.
- 두 번째 요청의 동시 전송은 계속 차단됩니다.
- Write 성공 후 exact readback이 필요한 상태만 안전상 편집기를 고정합니다.

검증 결과:

- API Debug/Release: 각각 `331/331 PASS`
- 최신 WPF Debug/Release: 빌드 PASS
- LASAL `StaticTopologyOnly`: SourceOnly/full 모두 PASS
- `IntegratedReadOwnerDormant`: 예상대로 `0x7E13`, `0x7E22` route 부재로 FAIL
- `git diff --check`: PASS

다음 LASAL 구현을 위해서는 [IDE 구조 생성 인계서](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md:1)의 client channel과 method를 LASAL IDE에서 한 번 생성해야 합니다. 그 작업이 끝나면 제가 외부 편집으로 `0x7E13/0x7E22` 구현을 이어가겠습니다.

이번에는 커밋·스테이징·테스트 폴더 복사를 하지 않았습니다. 기존에 stage된 대형 히스토리 파일의 trailing whitespace도 건드리지 않았습니다.

<details><summary>15 previous messages</summary>

> LASAL IDE 구조를 기다리는 동안 멈추지 않고, 설계 문서와 현재 소스를 다시 대조해 IDE 없이 진행 가능한 다음 구현을 추려 바로 반영하겠습니다. 특히 CREVIS API/WPF의 남은 계약, SDO 편집 흐름, 외부 `.st`에서 선행 가능한 부분을 병렬로 점검합니다.
>
> 현재 로드맵상 실제 PLC 동적 값의 다음 단계는 IDE가 만드는 client/method/network 구조에 묶여 있습니다. 그 구조를 임의 합성하지는 않되, 그 전에 SDK/WPF나 정적 검증 계약에서 빠진 부분이 있으면 먼저 닫는 방향으로 계속 진행 중입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> LASAL 쪽은 확인 결과, 사용자가 IDE에서 선언·네트워크 구조를 먼저 만든 뒤에만 안전하게 진행할 수 있습니다. 그동안 멈추지 않고, 지금 코드에서 실제로 빠진 두 가지—SDO 세션 변경 시 영구 잠금 가능성과 CREVIS 응답의 토폴로지 결속 검증—를 먼저 구현하겠습니다.
>
> SDO의 일반 전송 중 편집 잠금은 이미 요청 스냅샷과 제출 직렬화를 분리해 해제돼 있습니다. 추가로 발견된 문제는, 쓰기 성공 뒤 필수 readback 상태에서 연결 세션이 바뀌면 현재 프로세스에서 안전하게 해제할 길이 없어 영구 잠금이 된다는 점입니다. 이 경우만 “물리 확인 + 명시적 ACK + durable tombstone”으로 풀리도록 보완하고 있습니다.
>
> <details><summary>Created a file, edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+112 -17)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/StaleSdoWriteReadbackRecoveryPolicy.cs` (+48 -0)
>
> </details>
>
> 새 WPF Debug 빌드는 경고·오류 없이 통과했습니다. CREVIS 쪽은 SDK에 토폴로지 revision, NodeId, DS402 의미, I/O 폭과 방향을 실제 topology entry에 묶어 검증하는 overload와 회귀 테스트를 병렬로 추가했고, 지금 Release 검증까지 마무리 중입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+8 -3)
>
> </details>
>
> <details><summary>Created a file, edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/StaleSdoWriteReadbackRecoveryPolicyTests.cs` (+209 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyIoQualificationTool.cs` (+6 -23)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+14 -10)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+4 -3)
>
> </details>
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -2)
>
> </details>
>
> 현재 SDK는 Debug/Release 각각 340/340까지 전부 통과했습니다. CREVIS 화면은 이제 raw 응답을 바로 표시하지 않고, 선택한 topology와 NodeId·DS402·I/O 방향·bit 폭이 모두 맞아야 표시하도록 바뀌었습니다. 마지막으로 최신 인자 순서가 반영된 격리 WPF 빌드와 정적 계약을 다시 고정하겠습니다.
>
> 독립 리뷰에서 안전 결함은 없었지만, 두 방어심층 공백을 확인했습니다. 재시작 복구 경로에서도 pending exact readback이 하나라도 있으면 무조건 차단하도록 조건을 명시하고, “guard 재검증 → durable resolve → pending clear” 순서를 작은 테스트 가능한 커밋 함수로 분리해 영속화 실패 시 clear되지 않는 회귀를 고정하겠습니다.
>
> <details><summary>Edited 13 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+74 -39)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/StaleSdoWriteReadbackRecoveryPolicy.cs` (+55 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/StaleSdoWriteReadbackRecoveryPolicyTests.cs` (+59 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTopologyIo.cs` (+8 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTopologyIoModels.cs` (+17 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsTopologyIoContractTests.cs` (+99 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyBindingContractTests.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+8 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+16 -5)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+8 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+16 -2)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
</details>

반영 완료했습니다.

- SDO 요청 실행 중에도 편집할 수 있습니다. 실행 버튼을 누른 순간의 값만 스냅샷으로 전송됩니다.
- SDO Write 성공 후 Readback 안전 잠금은 유지됩니다. 연결 변경으로 Readback이 불가능해진 경우에만 명시적 확인으로 복구할 수 있습니다.
- CREVIS 응답은 현재 토폴로지와 일치해야만 GUI에 반영되도록 수정했습니다.
- 토폴로지 검증 없이 읽은 출력값으로는 Digital Output Write를 수행할 수 없습니다.

현재 실행 중이던 GUI는 이전 빌드입니다. 작업을 방해하지 않도록 종료하지 않았습니다. 최신 빌드는 다음 파일입니다.

[최신 Debug GUI 실행 파일](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/CodexLatest/Debug/LasalMotionControlApiExample.exe)

최신 GUI에서는 다음 순서로 확인하면 됩니다.

1. `Connect`
2. `Load CREVIS / Topology`
3. 설정된 CREVIS 구성 7개 행 확인

