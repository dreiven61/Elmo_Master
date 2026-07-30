단, 현재 표시 가능한 것은 정적 구성 정보입니다. PLC에 `0x7E13`, `0x7E22`, `0x7E23` 처리와 Feature Bit 15~17이 아직 없으므로 CREVIS의 실시간 상태, DI, DO 값이 안 올라오는 것은 현재 기준으로 정상입니다.

동적 CREVIS 구현을 계속하려면 LASAL IDE에서 한 번만 선언과 Network 연결 구조를 만들어야 합니다. 필요한 항목은 [LASAL IDE 구조 작업서](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md)에 정리했습니다. 이 구조를 저장한 뒤 알려주시면 해당 `.st` 구현을 외부 편집 방식으로 계속 진행하겠습니다.

검증 결과:

- PC 계약 테스트 Debug/Release: `342/342 PASS`
- WPF Debug/Release 빌드: 오류·경고 없음
- LASAL `StaticTopologyOnly`: Source/전체 정적 검사 PASS
- 실장 PLC 통신과 CREVIS 실제 I/O 시험은 아직 수행하지 않음
- 이번 변경은 아직 커밋하거나 테스트 폴더로 복사하지 않았습니다.

<details><summary>22 previous messages</summary>

> 현재 작업 트리를 다시 기준으로 남은 구현 항목을 재점검하겠습니다. LASAL IDE에서 생성해야 하는 선언·Network 구조는 건드리지 않고, 그 전까지 외부 소스에서 완성할 수 있는 API·GUI·계약 테스트와 문서를 계속 구현하겠습니다.
>
> 현재 작업 트리는 대규모 미커밋 상태지만, 기존 사용자 변경과 충돌하지 않도록 관련 파일만 좁혀서 보겠습니다. 우선 사용자가 실제로 본 “CREVIS가 안 보임”을 해결하기 위해 연결 이후 정적 토폴로지 자동 로드와 상태 표시 경로를 추적 중입니다.
>
> 다음 두 작업을 병행합니다.
>
> - GUI는 연결 직후 CREVIS 정적 토폴로지를 자동 로드하고, 실패 시 이전 행을 현재값처럼 남기지 않도록 원자 갱신·실패 상태를 넣습니다.
> - 별도로 실제 프로세스 강제종료 후 mutation journal 재시작 복구/no-replay 회귀시험을 추가하고 있습니다. 이는 현재 문서에 남아 있던 PC 측 미검증 항목입니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+193 -61)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+34 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+5 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+5 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+10 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+7 -3)
>
> </details>
>
> GUI 자동 로드 변경은 Debug/Release 격리 빌드가 모두 통과했습니다. 동시에 추가한 강제종료 회귀시험도 실제 child 프로세스를 종료한 뒤 journal interlock과 byte 불변을 확인해 현재 Debug `343/343 PASS`입니다. 이어 CREVIS 상태 원인 매트릭스와 parser 강건성 시험을 추가 검증 중입니다.
>
> 현재까지 코드 경계는 그대로 유지됩니다. 자동 동작은 `0x7E00/0x7E11/0x7E12` 읽기뿐이고 motion·SDO·output write는 자동 실행하지 않습니다. 최신 GUI는 로드 실패 시 기존 CREVIS 행을 즉시 폐기하고 capability/BootId/MapRevision/오류를 화면에 남깁니다.
>
> 독립 리뷰에서 실제 race 3건이 확인됐습니다. 외부 연결 해제 시 기존 CREVIS 행이 남는 문제, 끊어진 세션의 늦은 응답이 다시 행을 채울 수 있는 문제, UI 상태 갱신 중 `RequireConnection()` 예외 가능성입니다. 지금 세 항목을 fail-closed로 수정합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+5 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+19 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+2 -1)
>
> </details>
>
> 강제종료 회귀시험도 독립 리뷰 결과를 반영합니다. 부모 테스트 runner가 비정상 종료돼도 child가 고아로 남지 않도록 parent watchdog을 추가하고, temp junction 삭제 방어와 pipe deadlock 가능성을 제거합니다. 시험 증거는 “프로세스 종료 후 journal 재개방/interlock 보존”으로 한정하고 전원손실·실제 RPC no-replay로 과장하지 않겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsMutationJournalTests.cs` (+43 -18)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsMutationJournalTests.cs` (+30 -22)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsMutationJournalTests.cs` (+1 -1)
>
> </details>
>
> 회귀시험 최종 범위는 이제 명확합니다. Debug/Release 전체 `346/346 PASS`이며, 신규 항목은 process-termination journal reopen 1개, CREVIS 상태 원인 매트릭스 1개(5 case), topology/D5 deterministic parser property 2개입니다. 문서의 오래된 `304/342` 수치와 “fuzz 없음” 문구를 현재 증거에 맞춰 정리하겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+8 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+9 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+8 -6)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+5 -3)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+3 -2)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 자동 로드와 SDO 편집 수정은 반영된 상태입니다. 지금은 새 GUI 실행본·PC 테스트·LASAL 정적 계약을 다시 확인하고, 독립 검토에서 남은 회귀 테스트 한 건까지 닫은 뒤 현재 테스트 가능한 범위를 정확히 정리하겠습니다.
>
> 독립 검토에서 실제 결함 가능성이 있는 테스트 오라클 두 건을 발견했습니다. 구현 코드는 그대로 두고, 잘못된 parser 예외나 검증 누락도 PASS로 통과하지 못하도록 fuzz 계약을 강화 중입니다. 동시에 부모 종료 감시 경로의 회귀 테스트를 추가했고 현재 Debug 전체는 `347/347 PASS`입니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+3 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+3 -3)
>
> </details>
>
> LASAL 쪽은 최신 소스로 다시 정적 대조했습니다. `SourceOnly`와 generated network/table 포함 전체 계약이 모두 PASS했습니다. 다만 이 결과는 IDE 빌드·다운로드·실기 CREVIS 응답을 증명하지 않으며, GUI는 최신 PLC가 topology capability(bit 14)를 내지 않으면 실패 원인과 capability 값을 화면에 표시하도록 바뀌었습니다.
>
> WPF Debug/Release 격리 빌드도 다시 끝났고 오류·경고 없이 실행 파일이 생성됐습니다. SDO는 송신 중에도 다음 요청 값을 편집할 수 있고, Submit만 직렬화됩니다. 단, Write 성공 후 동일 대상의 exact readback이 남은 짧은 구간은 안전상 해당 필드를 고정합니다.
>
> 테스트 오라클 강화 작업이 중간 상태에서 한 번 컴파일 실패했습니다. 제품 코드 문제는 아니고 새 테스트 helper가 덜 들어간 상태였으며, 누락 helper만 마무리하도록 범위를 제한했습니다. 이 실패는 숨기지 않고 최종 결과에서 최종 재실행 값과 구분하겠습니다.
>
> 테스트 helper 누락과 오라클 과잉 제약을 수정한 뒤 전체를 다시 돌렸습니다. 최종 PC suite는 Debug/Release 모두 `347/347 PASS`입니다. 이제 남은 것은 작업 트리·실행 파일·임시 프로세스 잔존 여부를 최종 점검하는 단계입니다.
</details>

반영했습니다.

- GUI는 Connect 직후 CREVIS/Topology를 자동 조회합니다. 정상 PLC라면 `7 nodes / CREVIS 3 entries`가 표시됩니다.
- 구버전 GUI 구분을 위해 제목에 `[CREVIS auto-load]`를 넣었습니다.
- 조회 실패 시 기존 행을 지우고 `Capabilities`, `BootId`, `MapRevision`과 오류 원인을 화면에 표시합니다.
- SDO Write 송신 중에도 다음 요청 데이터를 편집할 수 있습니다. Submit만 직렬화됩니다.
- Write 성공 후 exact readback 구간에서는 대상 변조 방지를 위해 편집이 잠깁니다.

최종 검증:

- PC Debug: `347/347 PASS`
- PC Release: `347/347 PASS`
- WPF Debug/Release 빌드 PASS
- LASAL SourceOnly/전체 정적 계약 PASS
- 중간에 발생한 새 테스트 helper 컴파일 실패와 oracle 실패 2건은 수정 후 전체 재검증했습니다.
- PLC 다운로드 및 실제 CREVIS 통신은 아직 검증하지 않았습니다.

테스트할 최신 GUI는 [Release 실행 파일](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/CodexLatest/Release/LasalMotionControlApiExample.exe)입니다. Connect 후 행이 없으면 화면에 표시되는 `LOAD FAILED`와 capability 값을 전달해 주세요. 최신 PLC에 bit 14 topology 소스가 다운로드되지 않았다면 GUI만 교체해도 CREVIS 행은 나오지 않습니다.

동적 Node Health와 DI/DO API 구현은 LASAL IDE 생성 구조가 아직 없어서 다음 단계로 남아 있습니다. 필요한 구조는 [T2 IDE 작업 인계서](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md)에 정리했습니다. 이 구조를 IDE에서 생성·저장하면 바로 외부 편집 방식으로 `0x7E13`, `0x7E22`, `0x7E23` 구현을 이어갈 수 있습니다.

이번에는 커밋·스테이징·테스트 폴더 복사를 하지 않았습니다. 기존에 스테이징돼 있던 대형 history 파일의 trailing whitespace 문제도 그대로 보존했습니다.

<details><summary>13 previous messages</summary>

> 현재 소스와 미완료 계획을 다시 대조해서, LASAL IDE 구조를 기다리지 않고 진행 가능한 다음 구현부터 이어가겠습니다. 우선 topology/I/O·SDO·진단 로드맵의 남은 항목을 병렬 점검하고, 실제 제품 경로를 진전시키는 항목을 선택하겠습니다.
>
> 감사 결과, IDE를 기다리는 동안 진행할 실제 제품 코드가 두 가지 확인됐습니다. 첫째는 WPF에만 있던 SDO Write exact-readback 안전 계약을 SDK 공용 API로 올리는 작업이고, 둘째는 bit 15/16이 활성화되는 즉시 CREVIS health/DI를 자동 갱신하는 read-only monitor입니다. 현재 bit 14만 검증할 수 있도록 topology 전용 qualifier도 병행해, 지금 PLC 단계와 다음 단계가 서로 막히지 않게 하겠습니다.
>
> 현재 구현은 세 갈래로 진행 중입니다. SDO는 raw ticket 반환에서 끝나지 않고 SDK가 exact Read 요청·세션/BootId/MapRevision·결과 바이트까지 검증하도록 올리고 있습니다. CREVIS monitor는 capability off일 때 wire 호출이 0이도록 유지하며, topology 전용 qualifier는 현재 bit 14 단계만 독립 검증하도록 분리하고 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 세 갈래 구현 결과를 합쳐서 지금부터 통합 검증하겠습니다. 먼저 SDO 검증 API, CREVIS 실시간 모니터, 현재 PLC에서 실행 가능한 topology-inventory 범위가 서로 충돌 없이 들어왔는지 확인한 뒤 테스트 등록과 문서를 맞추겠습니다.
>
> 현재 확인된 상태는 이렇습니다. CREVIS 쪽은 GUI 타이머·단일 요청·세대 검증 구조까지 들어왔고, topology-inventory는 현재 구현된 `0x7E11/0x7E12`만 8회 읽도록 분리됐습니다. SDO 쪽은 더 강한 세션 검증을 적용하면서 기존 테스트 하나가 새 계약과 맞지 않아 에이전트가 실제 연결 세션 기반으로 바로잡는 중입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
>
> </details>
>
> 통합 테스트 등록까지 반영되어 현재 자동 테스트는 357개입니다. CREVIS 모니터는 늦게 도착한 선택 변경 전 오류까지 폐기하도록 보강됐고 357/357이 한 차례 통과했습니다. 이제 독립 코드 리뷰와 Debug/Release·WPF·LASAL 정적 계약을 루트에서 다시 확인하고, 문서의 “구현됨/아직 PLC 미검증” 경계를 맞추겠습니다.
>
> PC 쪽은 이제 Debug와 Release 모두 357/357, WPF도 두 구성 모두 재빌드가 통과했습니다. 남은 검증은 LASAL 소스/네트워크 정적 계약과 inventory dry-run이며, 실제 PLC 다운로드나 CREVIS 통신은 여기서 실행하지 않습니다.
>
> 독립 리뷰에서 SDO SDK 쪽 안전 결함 세 가지를 잡았습니다. 현재 게이트가 꺼져 있어 실제 Write 위험은 없지만, 활성화 전에 남겨둘 수 없는 문제라 바로 보강 중입니다. 컨텍스트 생성에 `Completed/Success` Write 상태를 필수화하고, capability/status 출처와 원 Write ticket·대상 일치까지 SDK가 직접 강제하도록 수정하고 있습니다.
>
> SDO 쪽 리뷰 결함은 모두 닫혔고 Debug/Release 358/358까지 통과했습니다. 추가 리뷰에서 CREVIS 모니터가 SDK 호출은 1회지만 내부 capability 조회까지 합치면 실제 TCP 요청은 2회라는 문제를 찾았습니다. 지금 capability snapshot을 고정해 `0x7E13` 또는 `0x7E22` 한 프레임만 보내는 전용 경로와 실제 요청 수 검증을 추가하고 있습니다.
>
> CREVIS 쪽 2-RPC 문제도 닫혔습니다. 일반 API는 기존 호환 동작을 유지하고, 자동 모니터만 현재 세션에 묶인 capability snapshot을 써서 tick당 실제 `0x7E13` 또는 `0x7E22` 한 번만 보냅니다. 신규 전송 테스트 3건을 포함해 에이전트 검증은 Debug/Release 361/361까지 통과했습니다. 이제 루트에서 같은 결과를 재현합니다.
>
> 최종 자동 검증은 모두 끝났습니다. 다만 현재 실행 중인 GUI는 제가 종료하지 않았기 때문에, 사용자가 보고 있는 창이 수정 전 실행 파일일 수 있습니다. 실행 중인 프로세스 경로와 새 빌드 경로만 확인해서 테스트 교체 지점을 정확히 알려드리겠습니다.
</details>

반영 완료했습니다. CREVIS 정보가 안 보였던 직접 원인도 확인했습니다. 현재 실행 중인 GUI는 **7월 27일 17:19에 실행한 이전 빌드**입니다. 수정된 GUI는 7월 28일 05:10 빌드이며, 사용 중인 프로그램은 종료하지 않았습니다.

- 새 Debug GUI: [LasalMotionControlApiExample.exe](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/CodexLatest/Debug/LasalMotionControlApiExample.exe)
- 실행 후 창 제목에 `[CREVIS auto-load]`가 표시되어야 합니다.
- Connect 시 bit 14 기반 정적 `CFG` 토폴로지를 자동 로드합니다.
- `LIVE` Health/DI는 PLC bit 15/16이 아직 꺼져 있으므로 현재는 `-`로 남고, 실제 통신도 0회입니다.
- bit 15/16 활성화 후에는 tick당 `0x7E13` 또는 `0x7E22`를 정확히 한 번만 전송합니다. 추가 capability RPC는 발생하지 않습니다.

SDO Write도 수정했습니다.

- 일반 SDO 요청 실행 중에도 Slave/Index/SubIndex/Type/Length/Write Data를 계속 편집할 수 있습니다.
- Write 성공 후 exact readback 단계에서만 원 Write 대상을 보호하기 위해 편집이 잠깁니다.
- SDK 검증은 동일 owner/session, `Completed+Success`, BootId/MapRevision, capability 관측 순서, target/type/length/value를 모두 검증합니다.
- SDO Write 승인 allowlist와 PLC gate는 계속 꺼져 있습니다.

검증 결과:

- PC Debug/Release: 각각 `361/361 PASS`
- WPF Debug/Release: 빌드 PASS
- LASAL SourceOnly/full 정적 계약: PASS
- `topology-inventory` dry-run: `0x7E11` + `0x7E12`만 허용, 네트워크 송신 없음
- PLC/CREVIS 실시간 시험 및 LASAL 다운로드는 수행하지 않았습니다.
- 커밋·추가 스테이징·테스트 폴더 복사는 하지 않았습니다. 기존 staged history 파일도 그대로 유지했습니다.

핵심 소스는 [SDO 검증 API](./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsSdoWriteVerification.cs), [Topology/I/O API](./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTopologyIo.cs), [GUI CREVIS 모니터](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs)에 반영했습니다.

<details><summary>139 previous messages</summary>

> 다음 구현 슬라이스를 현재 소스와 잔여 계획에서 다시 선별하겠습니다. LASAL IDE 구조가 필요한 T2 동적 경로는 건드리지 않고, PC/SDK/WPF에서 지금 완결할 수 있는 항목 중 실제 기능 격차가 큰 것을 먼저 잡겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 남은 계획을 보면 다음 큰 항목은 D4 Double bank지만, 현재 LASAL IDE 구조 없이 안전하게 완결 가능한지부터 분리해야 합니다. 병렬로 D4의 실제 선행조건, PC/SDK 단독 구현 가능 항목, 문서와 소스의 불일치를 각각 점검하고 곧바로 구현 가능한 한 묶음을 선택하겠습니다.
>
> 현재 D4의 PLC 메모리·상태기계는 LASAL 쪽 실제 구현이 필요해 바로 건드릴 수 없지만, 클라이언트 프로토콜은 이미 Double 모드를 이해합니다. 따라서 다음 후보는 capability bit 6이 켜진 PLC에서 두 bank 동시 capture/upload·identity·release 격리를 검증하는 WPF qualification 경로이며, 지금 그 진입점과 기존 Recorder 시험 구조를 대조하고 있습니다.
>
> 다음 슬라이스는 CREVIS topology snapshot의 연결 소유권·세션 provenance 보강으로 확정하겠습니다. 현재는 revision/entry만 맞으면 다른 연결이나 재접속 전 topology 객체도 topology-bound read에 들어갈 수 있어, 실제 송신 전 차단 계약이 capability snapshot보다 약합니다. 이 부분을 SDK와 테스트에서 먼저 닫고, 이후 D4 Double qualification으로 넘어가겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTopologyIoModels.cs` (+60 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTopologyIo.cs` (+29 -23)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTopologyIo.cs` (+24 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsTopologyIoContractTests.cs` (+29 -17)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyBindingContractTests.cs` (+118 -0)
>
> </details>
>
> 독립 감사에서도 같은 계열의 공백이 확인됐습니다. Topology뿐 아니라 Signal Catalog도 owner/session 없이 alias·Bulk builder에 전달되고 있어, 동일 revision을 쓰는 다른 PLC나 재접속 전 Catalog를 오인할 수 있습니다. 두 aggregate의 provenance를 한 계약으로 묶어 이번 슬라이스에서 같이 닫겠습니다.
>
> <details><summary>Edited 10 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD1.cs` (+6 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD1Models.cs` (+54 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnostics.cs` (+15 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsPIBulkFacade.cs` (+78 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsPIBulkFacade.cs` (+4 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD2.cs` (+7 -18)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsPIBulkFacadeModels.cs` (+4 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsPIBulkFacadeContractTests.cs` (+25 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsPIBulkFacadeContractTests.cs` (+113 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/BulkQualificationCleanupOrchestratorTests.cs` (+6 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD2.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD1ContractTests.cs` (+4 -0)
