>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+113 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+33 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+9 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+12 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+10 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+24 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+19 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+23 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+18 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+10 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+16 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+18 -0)
>
> </details>
>
> 리뷰에서 LASAL에서 실제로 가능한 포인터 alias 우회까지 확인됐습니다. 단순 문자열 가정이 아니라 프로젝트 내 사용 문법이므로, handler에서는 `#snapshot[0]` 한 번 외 모든 주소 취득을 금지하고, RT latch는 허용된 client/buffer/atomic 주소만 허용하도록 바꾸고 있습니다. 동시에 legacy master/drive read가 반드시 연결된 분기 안에 있고 zero fallback이 ELSE에만 있도록 구조 검사를 추가합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+139 -0)
>
> </details>
>
> GUI/SDK 쪽은 이미 수정된 소스 기준으로 다시 빌드·테스트하겠습니다. 실행 중인 구버전 GUI는 종료하지 않고, 별도 `bin\CodexLatest` 출력 경로만 사용해 사용자의 현재 작업을 방해하지 않겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 최종 검토에서 실제 누락 하나가 확인됐습니다. 레거시 스냅샷 값의 “개수”만 맞추면 다른 임시 변수로 채널 읽기를 우회할 수 있었습니다. 각 로컬 값이 정확한 EtherCAT 채널 읽기와 직접 결속되도록 검증기를 보강한 뒤 전체 검증을 다시 돌리겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+60 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+18 -11)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+178 -0)
>
> </details>
>
> 세 가지 우회 경로를 모두 막았습니다: helper 전체 포인터/주소 별칭 금지, 0x7E13 응답 17개·0x7E22 응답 12개 고정, master/각 Drive 로컬을 정확한 채널 RHS와 fail-closed 기본값에 결속했습니다. 현재 소스 기준 정적 계약 SourceOnly와 전체 검증은 다시 통과했습니다. 이제 새 해시를 독립 재검토시키고 PC 테스트를 마무리합니다.
>
> 독립 공격 검토에서 한 단계 더 깊은 주석 위장 우회가 발견됐습니다. 기존 canonical 응답 줄을 주석으로 남겨 순서 검사를 속이고, 실행 코드에는 범위 밖 쓰기를 넣을 수 있었습니다. 주석 제거 후의 실제 실행 코드에서 각 offset/type/RHS가 정확히 한 번씩 존재하도록 바꾸고 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+78 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+18 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+36 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+27 -6)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+126 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+31 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -0)
>
> </details>
>
> 추가로 발견된 helper 앞단의 직접 OOB read/write와 조건부 응답 미실행도 막았습니다. 이제 요청/응답 포인터를 선언부·초기 guard·0x7E11·0x7E12·각 신규 command·최종 오류 envelope 구간별로 정확히 소유시키고, 응답 필드와 ResponseSize는 payload guard 직계 깊이에서만 허용합니다. 최신 정적 계약 검증은 다시 PASS입니다.
>
> 최종 독립 검토에서 실제 검증기 자체의 모순 두 개를 잡았습니다. 문서대로 `BuildCatalogEntry`에 응답 포인터를 넘기는 정상 코드까지 금지했고, payload arm 기준 직접 쓰기의 깊이를 0으로 잘못 계산했습니다. 전역 중복 규칙을 제거하고 command 구간별 포인터 총량으로 일원화하며, 실제 helper의 arm 표현에 맞춰 깊이를 바로잡겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+0 -20)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+9 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+16 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+17 -17)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -0)
>
> </details>
</details>

반영 완료했습니다. CREVIS가 안 보인 직접 원인은 두 가지입니다.

- 현재 실행 중인 PID 28580은 구버전 `bin\Debug` GUI이며 바이너리에 `CREVIS` 화면 코드가 없습니다.
- 테스트 폴더의 LASAL 소스에는 아직 `0x7E11/0x7E12`가 없습니다. 따라서 현재 테스트 PLC도 topology를 반환할 수 없습니다.

새 GUI는 여기 있습니다.

- [최신 Debug GUI](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/CodexLatest/Debug/LasalMotionControlApiExample.exe)
- [CREVIS 화면](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:805)
- [Topology 조회 구현](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs:152)

테스트 순서는 다음과 같습니다.

1. 기존 GUI PID 28580을 수동 종료합니다.
2. 위 `CodexLatest\Debug` GUI를 실행합니다.
3. 최신 master LASAL 소스를 테스트 폴더로 수동 복사하고 빌드·다운로드합니다.
4. 연결 후 `Load CREVIS / Topology`를 누릅니다.
5. 정상이라면 configured CREVIS 3개(coupler + input/output slot)가 표시됩니다.

SDO Write 편집 문제도 수정했습니다.

- Submit 클릭 시 immutable request를 먼저 생성합니다.
- 일반 Write/Read 진행 중에도 다음 요청 값을 편집할 수 있습니다.
- 두 번째 Submit은 기존 operation이 끝날 때까지 계속 차단됩니다.
- 성공 Write 뒤 exact readback 단계에서는 대상 보존을 위해 의도적으로 편집을 잠급니다.

관련 구현은 [MainWindow.Diagnostics.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:1020)와 [DESIGN.md](./LMC_Library/LasalApiWpfTestApp/DESIGN.md:360)에 반영했습니다.

검증 결과:

- PC Debug/Release: 각각 `315/315 PASS`
- WPF Debug/Release 빌드: PASS
- LASAL Static SourceOnly/전체 계약: PASS
- `git diff --check`: PASS
- LASAL IDE, 실행 중 GUI, 테스트 폴더는 건드리지 않았습니다.
- 커밋·스테이징도 하지 않았습니다.
- 기존에 스테이징된 history 파일의 trailing whitespace 때문에 `git diff --cached --check`만 실패합니다.

현재 LASAL은 configured topology까지만 구현됐습니다. 동적 상태와 실제 I/O인 `0x7E13/0x7E22/0x7E23`은 아직 capability off이며, 다음 단계에서 `LMCEcatInputLatch`의 CREVIS client/method/network 구조가 필요합니다.

<details><summary>18 previous messages</summary>

> 현재 소스와 구현 목록을 다시 대조해서, 사용자 IDE 작업 없이 진행 가능한 다음 조각부터 바로 구현하겠습니다. LASAL IDE 제어·테스트 폴더 복사·실행 중 GUI 종료는 하지 않고, master 소스만 외부 편집하겠습니다.
>
> 현재 live source를 확인한 결과, 필요한 LASAL IDE 구조는 아직 생성되지 않았습니다. `LMCEcatInputLatch`에는 기존 6개 client와 `CopySnapshot`만 있고 CREVIS 3개 client·신규 snapshot/mailbox method가 없습니다. 이 상태에서 implementation을 먼저 넣으면 최신 master 자체가 빌드 불가가 되므로, 우선 IDE 작업과 무관한 남은 C#/검증 항목을 끝까지 확인하고 있습니다.
>
> 설계와 검증기 사이의 단계 불일치도 확인했습니다. 설계는 node/DI 실기 검증 전 bit 15/16을 꺼두라고 하는데, 기존 `IntegratedReadOwner` 검증은 구현과 동시에 두 bit를 켜도록 강제했습니다. 먼저 `IntegratedReadOwnerDormant` 체크포인트를 추가해 구현·라우팅은 완성하되 capability는 계속 닫아둘 수 있게 정리하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+8 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+18 -7)
>
> </details>
>
> <details><summary>Created a file, edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md` (+125 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -1)
>
> </details>
>
> LASAL 구조 대기 중에도 진행 가능한 실제 GUI 결함을 찾았습니다. 불확정 digital output 결과를 ACK할 때 설계상 “물리 출력 확인” 체크가 필수인데, 현재는 경고창에서 Yes만 누르면 durable record가 해소됩니다. output 전용 확인 체크박스와 이중 검증, reset 규칙, PC 정책 테스트를 지금 구현하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 출력 미확정 ACK는 “물리 확인 체크 + 경고 확인”을 모두 요구하도록 고치고 있습니다. 동시에 정적 검증기가 LASAL 구조 일부만 생성된 상태를 놓치는 문제도 발견되어, 그 상태가 통과하지 않도록 함께 보강하겠습니다.
>
> GUI 쪽은 버튼 표시만 바꾸는 수준이 아니라, 미확정 상태의 모든 진입·해제 지점에서 확인 체크가 재사용되지 않도록 초기화하겠습니다. 확인창에서 `Yes`를 누른 뒤에도 조건을 다시 검사해 우회 가능성까지 막습니다.
>
> <details><summary>Created 2 files, edited 9 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/DigitalOutputUncertainAcknowledgementPolicy.cs` (+15 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+72 -18)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+8 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DigitalOutputUncertainAcknowledgementPolicyTests.cs` (+48 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -3)
>
> </details>
>
> 구현은 반영됐습니다. 이제 XAML/프로젝트 파일 파싱, 정책 truth-table 테스트, Debug/Release 빌드를 순서대로 돌려서 실제로 GUI가 컴파일되는지 확인하겠습니다. PLC/LASAL 실동작은 이 검증과 분리해 표시합니다.
>
> Debug 기준은 WPF 빌드와 PC 테스트 316/316 모두 통과했습니다. Release도 같은 방식으로 독립 출력 폴더에 빌드하고, 이후 정적 LASAL 체크포인트가 현재 bit 15~17 off 상태를 정확히 유지하는지 확인하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
>
> </details>
>
> 확인 결과, 지금 열려 있는 GUI(PID 28580)는 `bin\Debug`의 7월 27일 구버전입니다. 그래서 새 CREVIS 화면과 SDO 편집 변경이 보이지 않는 상태가 맞습니다. 실행 중인 프로세스는 건드리지 않았고, 새 빌드는 `bin\CodexLatest\Debug`와 `Release`에 따로 만들었습니다.
>
> 최종 리뷰에서 정적 검증기 두 군데를 더 보강할 필요가 확인됐습니다. LASAL 주석에 메서드 이름만 적혀도 부분 구현으로 오인하던 점은 제거하고, IDE에서 네트워크 연결만 일부 만든 경우도 `StaticTopologyOnly`가 반드시 실패하도록 수정하겠습니다.
>
> <details><summary>Created 2 files, edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+27 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DigitalOutputUncertainAcknowledgementPolicyTests.cs` (+32 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/DigitalOutputUncertainAcknowledgementPolicy.cs` (+31 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+31 -13)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/SdoEditorAvailabilityPolicyTests.cs` (+36 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+4 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/SdoEditorAvailabilityPolicy.cs` (+14 -0)
>
> </details>
>
> 리뷰 보강까지 반영했습니다. SDO 편집 가능 여부도 순수 정책으로 분리해 “일반 Write 진행 중 편집 가능 / 성공 Write의 exact readback 중에는 고정”을 자동 시험하도록 만들었습니다. 이제 변경 후 전체 Debug/Release를 다시 검증합니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+14 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+5 -2)
>
> </details>
</details>

원인 확인됐다. 현재 실행 중인 GUI(PID 28580)는 7월 27일 구버전 `bin\Debug` 실행 파일이다. 실행 중인 프로그램은 건드리지 않았다.

- 최신 Debug: [LasalMotionControlApiExample.exe](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/CodexLatest/Debug/LasalMotionControlApiExample.exe)
- 최신 Release: [LasalMotionControlApiExample.exe](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/CodexLatest/Release/LasalMotionControlApiExample.exe)

최신 GUI에서는 `EtherCAT / PI → Load CREVIS / Topology`을 직접 눌러야 한다. 정상이라면 CREVIS 3개 행과 Elmo 4개 행이 표시된다. 자동 로드는 아니다.

다만 현재 표시되는 것은 정적 7-node 구성 정보다. 실제 CREVIS node health와 입력·출력 값은 아직 LASAL의 `0x7E13/0x7E22/0x7E23`과 capability bit 15~17이 구현되지 않아 비활성 상태다. 따라서 현재 topology 데이터가 물리 노드 상태에 따라 변하지 않는 것도 현 코드 기준으로는 정상이다.

SDO Write도 반영했다.

- 일반 Write 요청 진행 중에는 다음 요청의 Operation, Slave, Index, SubIndex, Type, Length, Timeout, Value를 편집할 수 있다.
- 두 번째 동시 제출은 계속 차단한다.
- 성공 Write의 exact readback 단계에서만 대상 보호를 위해 의도적으로 편집기를 고정한다.
- 출력 결과 불명 ACK는 물리 출력/PLC shadow 확인 체크박스와 경고 확인을 모두 요구하도록 강화했다.

검증 결과:

