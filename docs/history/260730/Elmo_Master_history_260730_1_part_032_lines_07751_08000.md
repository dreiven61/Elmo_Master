</details>

반영 완료했습니다.

- CREVIS: Connect 시 자동 표시되고, 초기 capability OFF였어도 `Reload`로 재조회됩니다. [MainWindow.TopologyIo.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs:851)
- SDO Write: RPC 실행 중에도 입력값 편집이 가능합니다. Submit만 직렬화되며, 정확한 Write-readback 확인 중에만 편집을 잠급니다. [SdoEditorAvailabilityPolicy.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/SdoEditorAvailabilityPolicy.cs:3)
- D4 Double-bank: qualification, retained cleanup, reconnect recovery adapter와 durable Release 재시도 계약까지 보강했습니다. [MainWindow.Qualification.RecorderDouble.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs:580)
- 예상 밖의 세 번째 Start 성공·불명확 결과는 자동 Release하지 않고 zero-wire로 차단합니다.
- Release 확인은 매 실행 시 다시 요구하며, pending intent는 같은 target만 재사용합니다.
- 관련 문서 8개도 현재 동작과 일치하도록 수정했습니다.

검증 결과:

- API Debug/Release: 각각 `545/545 PASS`
- WPF Debug/Release: 각각 `12/12 PASS`
- 빌드 오류·경고: `0`
- `git diff --check`: PASS
- 추가 코드 리뷰: P1/P2 결함 없음

D4 실기 proof gate 3개는 아직 `false`입니다. 따라서 PLC에서 Double-bank live qualification/recovery는 아직 활성화하지 않았습니다. 이번에는 LASAL IDE, PLC 다운로드, 테스트 폴더 복사, stage/commit을 하지 않았습니다.

<details><summary>11 previous messages</summary>

> 다음 구현 항목을 현재 소스와 잔여 계획에서 다시 소팅하겠습니다. 실기 증거가 필요한 gate는 닫아 둔 채, 아직 코드로 끝낼 수 있는 항목을 먼저 골라 구현·회귀 검증까지 이어가겠습니다. LASAL IDE와 테스트 폴더는 건드리지 않습니다.
>
> 현재 소스 기준으로 EtherCAT I/O의 PC SDK·WPF·read-only qualifier는 이미 구현돼 있고, 다음 실제 기능인 `0x7E13/0x7E22`는 LASAL IDE가 생성해야 하는 client/method/network 구조가 아직 없습니다. 그 구조를 외부에서 위조하지 않는 조건을 지키면서, 병행 감사로 다른 code-only 잔여 결함이 있는지 확인하고 있습니다.
>
> 다음 code-only 결함을 찾았습니다. D4 reconnect의 occupied configuration Release가 서버에서 명확히 거부되어 “전송됐지만 적용 안 됨”으로 판정되면, handle과 durable intent는 재사용 가능하지만 현재 coordinator가 pending intent를 무조건 막아 같은 exact target 재시도를 영구 차단합니다. 이 비대칭을 bank·empty configuration 경로와 동일하게 수정하고 회귀를 추가하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 먼저 D4 복구 경로의 “전송되지 않은 점유 configuration Release 재시도” 교착을 바로 수정하겠습니다. 이건 LASAL IDE 없이 끝낼 수 있는 코드 결함이고, CREVIS PLC 구현은 필요한 IDE 구조가 아직 없어서 그 다음 경계로 분리해 두겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+99 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleDurableReleaseCoordinator.cs` (+11 -12)
>
> </details>
>
> 첫 결함은 자동 회귀 546/546으로 닫았습니다. 이어서 reconnect 시 사용자가 확인한 “빈 configuration”과 실제 inventory에서 발견된 bank 목록이 달라질 수 있는 경로를 점검 중입니다. 실제 해제 대상이 새로 생기면 같은 클릭에서 바로 해제하지 않고, 정확한 bank 목록을 다시 보여준 뒤 재확인하도록 바꾸는 방향입니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryPlanner.cs` (+122 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryOrchestrator.cs` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+21 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+13 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs` (+21 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs` (+17 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+61 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryPlannerTests.cs` (+119 -0)
>
> </details>
>
> 두 번째 안전 보강도 구현했습니다. 이제 reconnect recovery는 4D/4A 읽기와 로컬 journal 갱신까지만 허용하고, 확인 당시 없던 revision이나 bank가 발견되면 Adopt/Release 전에 멈춰 새 계획을 다시 확인하게 됩니다. 지금은 exact-target 행렬과 실제 orchestrator의 zero-mutation 경로를 함께 빌드 검증하겠습니다.
>
> Debug 회귀는 API 552/552, WPF 12/12로 통과했습니다. Release도 같은 조합으로 확인 중이며, 이 결과는 PC 코드/GUI 정적 증거일 뿐 PLC·실기 통과로 확대하지 않겠습니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./LMC_Library/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./LMC_Library/README.md` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+5 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+6 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -1)
>
> </details>
</details>

반영 완료했습니다.

- SDO Write 중에도 Slave/Index/SubIndex/Type/Value 편집 가능
  - 중복 Submit 버튼만 잠김
  - 정확한 Write readback 복구 중에만 관련 필드 고정
- D4 점유 configuration의 confirmed-not-applied Release를 동일 intent/handle로 재시도하도록 수정
- reconnect inventory에서 새 revision/bank가 발견되면 Adopt/Release 전에 중단하고 재확인하도록 보강
- Debug/Release:
  - API 552/552 PASS
  - WPF 12/12 PASS
  - LASAL SourceOnly `Phase5TransportClean + StaticTopologyOnly` PASS
- working diff whitespace 검사 PASS
  - cached 검사는 기존 staged history 파일의 trailing whitespace 때문에만 실패
- 커밋·스테이징·LASAL IDE 조작·PLC 시험은 하지 않았습니다.

CREVIS는 구분해야 합니다. 현재 GUI는 설정된 CREVIS Coupler/DI/DO를 자동 표시하지만, 실제 상태와 I/O 값은 아직 PLC에서 올라오지 않습니다. LASAL에 `0x7E13/0x7E22` handler, latch client와 Motion Network 연결이 아직 없기 때문입니다.

다음 단계는 사용자가 LASAL IDE에서 [T2 IDE 구조 인계서](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md:18)의 client/method/network 구조를 생성하는 것입니다. 완료되면 제가 외부 편집으로 464-byte snapshot과 `0x7E13/0x7E22` 구현을 이어가고, 그다음 빌드를 요청하겠습니다.

<details><summary>10 previous messages</summary>

> 다음 code-only 증분으로 CREVIS live GUI 경로를 실제 capability가 켜진 fake PLC 응답까지 검증하겠습니다. 먼저 현재 LASAL 구조가 추가됐는지 재확인하고, 여전히 없으면 제품 코드를 억지로 합성하지 않고 WPF의 `0x7E13/0x7E22` 실제 호출·갱신·late-response 폐기 경로를 보강하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> LASAL 쪽은 다시 확인됐습니다. CREVIS 장치/슬롯 객체는 생겼지만, live 경로에 필요한 latch client·method·diagnostics handler·`0x7E13/0x7E22` route가 아직 없어 PLC 데이터가 GUI에 올라올 수 없는 상태입니다. 이 구조는 IDE 생성 후 외부 구현해야 하므로 건드리지 않고, 지금은 WPF가 capability-on 응답을 실제로 받아 표시하는 경로를 자동 검증하고 있습니다.
>
> WPF 수신 경로를 독립 점검하면서 실제 결함 2개를 찾았습니다. 수동 Health/DI/DO 조회 중 선택을 바꾸면 늦은 이전 응답이 새 선택 화면을 덮을 수 있었고, Health와 DI가 오류 상태를 공유해 한쪽 성공이 다른 쪽 오류를 지울 수 있었습니다. 둘 다 현재 증분에서 함께 수정하고 회귀 테스트까지 넣겠습니다.
>
> 제품 수정은 끝났고 Debug WPF 빌드는 이미 통과했습니다. 현재는 늦은 응답 선택 변경과 Health/DI 오류 분리의 결정적 fake-PLC 회귀 테스트를 추가 중입니다. 이 테스트까지 끝낸 뒤 Debug/Release 전체 수치와 LASAL 정적 계약을 다시 맞추겠습니다.
>
> 회귀 리뷰에서 화면 증거 관련 2건을 더 잡았습니다. 성공 뒤 조회 실패 시 상세 패널이 이전 값을 새 값처럼 남길 수 있었고, 양방향 I/O 행에서는 자동 DI 표시가 출력 shadow 증거를 가려도 Submit이 살아 있을 수 있었습니다. 실제 출력 제어에서 혼동을 만들 수 있어 이것도 fail-closed로 수정한 뒤 테스트하겠습니다.
>
> 후속 fail-closed 수정까지 반영됐습니다. 자동 DI가 출력 shadow 증거를 가리지 않으며, 수동 입력/출력 재조회 실패 시 오래된 shadow와 확인 상태를 해제합니다. 이제 테스트 코드가 이 동작과 stale 오류 표시를 고정하도록 마무리한 뒤 전체 smoke를 실행하겠습니다.
>
> 회귀 테스트 작성까지 끝났습니다. 지금부터 WPF actual-control smoke 15개를 Debug/Release로 실행하고, 실패가 있으면 그 자리에서 원인까지 고치겠습니다.
>
> Debug/Release WPF smoke는 모두 15/15 통과했습니다. API도 앞서 Debug/Release 552/552, LASAL static SourceOnly/full도 PASS했습니다. 이제 문서의 테스트 수치와 “PC에서 검증된 것 / PLC live는 아직 없는 것” 경계를 정확히 갱신하겠습니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -2)
> - Wrote `./LMC_Library/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+7 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+11 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+14 -4)
>
> </details>
</details>

이번 증분은 완료했습니다. 다만 실제 PLC의 CREVIS live 정보는 아직 올라올 수 없습니다.

- WPF가 capability bits 14~16 환경에서 `0x7E13` Health와 `0x7E22` DI를 표시하도록 검증했습니다.
- 늦은 수동 응답이 새 선택 정보나 Output shadow를 덮는 문제를 수정했습니다.
- Health/DI 오류·stale·cycle을 채널별로 분리했습니다.
- mixed-I/O에서 오래된 Output shadow로 Submit되는 경로를 fail-closed 처리했습니다.
- SDO Write는 일반 RPC 진행 중에도 편집 가능하며 Submit만 직렬화됩니다. exact readback interlock일 때만 의도적으로 고정됩니다.

주요 코드는 [MainWindow.TopologyIo.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs:322), 테스트는 [WpfMainWindowIntegrationTests.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs:27)에 반영했습니다.

검증 결과:

- API Debug/Release: 각각 552/552 PASS
- WPF Debug/Release: 각각 15/15 PASS
- LASAL `StaticTopologyOnly`: SourceOnly/full PASS
- 독립 최종 리뷰: P1/P2 없음
- `git diff --check`: PASS
- 이번 작업은 커밋·스테이징하지 않았습니다.

실제 CREVIS live가 안 나오는 원인은 LASAL에 `0x7E13/0x7E22` route, latch client/method, diagnostics handler가 아직 없기 때문입니다. 현재는 bit 14의 configured 7행/CREVIS 3행까지만 실제 PLC에서 가능합니다. 다음 IDE 구조 작업은 [T2 IDE 인계서](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md:5)대로 생성해야 합니다. 그 구조가 생기면 제가 외부 편집으로 snapshot·handler·route 구현을 바로 이어가겠습니다.

참고로 `git diff --cached --check`는 기존에 staged된 `docs/history/Elmo_Master_history_260721.md`의 공백 때문에 실패하며, 이번 변경과는 무관합니다.

<details><summary>19 previous messages</summary>

> 다음 증분은 현재 막힌 CREVIS PLC 구조를 우회해 가짜 기능을 더 만드는 대신, 구현 목록에서 IDE 구조 없이 실제로 전진시킬 수 있는 다음 항목을 소스 기준으로 다시 소팅하겠습니다. CREVIS T2 대기와 병행 가능한 API/WPF/LASAL 외부 구현 후보를 찾아 가장 우선순위 높은 것을 바로 구현하겠습니다.
>
> 현재 문서상 P1~P4의 큰 항목은 대부분 “코드 미완료”가 아니라 LASAL IDE 생성·PLC download·실기 증거 대기 상태입니다. 그래서 중복 기능을 더 만들지 않도록, 남은 제품 결함과 실제 시험 준비 코드의 빈틈을 병렬로 감사하고 있습니다.
>
> 우선 구현 후보가 나왔습니다. CREVIS `0x7E22` parser가 invalid 응답의 nonzero stale 값이나 `DataDefaulted` 누락을 받아들이는 P2입니다. 실제 PLC가 잘못된 값을 보내면 Write는 막혀도 GUI/qualification이 오래된 I/O 값을 정상 계약처럼 표시할 수 있으므로, 이 경로를 먼저 fail-closed로 고치고 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> P2 Digital I/O 파서 보강은 반영됐습니다. 지금 변경 내용을 직접 검토한 뒤, 병렬 작업 중인 D1 고장 시나리오와 D5 연결 단절 검증 코어까지 합쳐 빌드·회귀 테스트하겠습니다. 아직 PLC 실증 완료로 간주하지는 않습니다.
>
> D1 고장 시나리오는 “고장 축 raw 값이 0이어야 한다”로 잘못 판정하지 않도록 정리했습니다. 정확한 기준은 해당 축의 `Valid` 해제, `SlaveOffline`, detail 18이고, 남아 있는 raw 값은 stale 표시로만 남기며 현재값으로 쓰지 않습니다. 동시에 두 축 이상이 offline이면 PASS가 아니라 INCONCLUSIVE로 처리합니다.
>
> D1 통합 중 실제 계약 불일치를 찾았습니다. 현재 PLC의 `0x7E20 ReadPI`는 offline 상태를 보내면서도 detail 18을 쓰지 않습니다. WPF 판정만 완화하면 결함을 숨기므로, 기존 LASAL 구현 영역에 offline=18(그 외 invalid=11) 매핑까지 같이 반영하겠습니다. IDE는 제어하지 않고 추적된 `.st`만 외부 편집합니다.
>
> P2 파서 보강은 독립 리뷰에서도 P1/P2 결함이 없었습니다. 설계의 네 조건(`Value=0`, `ValidMask=0`, `DataDefaulted`, 실제 원인 bit)을 코드·계약 테스트·fuzz·패킷 문서가 동일하게 적용하고 있습니다. 이제 D1/LASAL과 D5 결과를 합쳐 전체 회귀를 돌립니다.
>
> D1의 실제 PLC 응답 결함은 소스에 반영됐습니다. `ReadPI`는 이제 유효값이면 detail 0, offline이면 18, 그 밖의 invalid이면 11을 offset 48에 명시적으로 씁니다. 같은 규칙을 C# 파서에도 적용해 잘못된 PLC payload를 조용히 수용하지 않도록 보강 중입니다.
>
> 추가로 실제 GUI 결함도 확인했습니다. PI/Bulk 표가 invalid 상태에서도 마지막 `RawValue32`를 현재값처럼 다시 표시하고 있었습니다. 이건 로그만 바로잡아서는 안 되므로, invalid 행은 `UNAVAILABLE`/`-`로 표시하고 raw는 현재값으로 쓰지 않도록 GUI도 함께 수정합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 진행 중인 두 축을 마무리하겠습니다. 먼저 D1의 CREVIS/PI 오프라인 표시와 1축 장애 판정, D5의 SDO 연결 단절·고아 티켓 검증 코어를 회수해 교차 검토한 뒤 전체 PC·정적 검증을 돌리겠습니다. 실제 CREVIS 수신은 LASAL 구조 생성과 PLC 빌드·다운로드 전까지는 완료로 보지 않겠습니다.
>
> D1 쪽에는 요청한 두 표시 수정이 들어왔습니다. PI 카탈로그와 Bulk 표 모두 유효하지 않은 값은 원시값 대신 `UNAVAILABLE`로 표시하고, 정상축 판정에는 `AxisError == 0`도 포함됐습니다. 지금은 이 판정이 실제 오프라인 축과 Bulk의 `OfflineSourceIndex`를 정확히 일치시키는지 테스트 계약을 점검하고 있습니다.
>
> D1 증분은 자동 검증까지 완료됐습니다: PC Debug/Release 573/573, WPF smoke 16/16, LASAL SourceOnly 정적 계약 PASS입니다. 별도 검토를 한 번 더 붙였습니다. 단, `0x7E20`의 DetailCode 기록은 소스·정적 계약만 확인된 상태라서 사용자 쪽 LASAL 빌드·다운로드 후 실제 오프라인 시험이 필요합니다.
>
> D5 단절·고아 티켓 증분은 현재 UI에 연결하지 않은 검증 코어로만 유지하고 있습니다. 검토 중 두 가지 경쟁 조건을 추가로 잡았습니다: terminal 관측 직후 취소 시 증거를 지우지 않는 조건과, 마지막 PASS 기록/clear 직전에 구·신 연결 상태를 다시 확인하는 조건입니다. 이 두 회귀까지 넣은 뒤 전체 빌드를 돌립니다.
>
> D5 코어 자체 검토도 끝났습니다. Running 관측만 고아 티켓 합격으로 분류하고, Queued는 애플리케이션 복구까지만 인정합니다. 외부 단절·두 복구 티켓·BootId/MapRevision·원장 ABA·취소·PASS 로그 실패를 모두 보수적으로 처리하며, UI나 연결 종료 동작은 아직 붙이지 않았습니다. 이제 제가 Debug/Release 전체와 WPF·LASAL 정적 검증을 다시 실행합니다.
>
> 전체 재실행 결과는 PC Debug/Release 575/575, WPF Debug/Release 16/16 PASS입니다. 특히 GUI 회귀가 두 요청을 직접 확인합니다: CREVIS 정적 구성 7행 중 3개 CREVIS 행 자동 표시, 그리고 일반 SDO 작업 중 Write 입력 편집 유지입니다. 다음은 PLC 소스의 `ReadPI DetailCode` 정적 계약과 현재 topology gate를 확인합니다.
>
> 독립 리뷰에서도 D1 관련 P1/P2 결함은 없었습니다. 현재 실패가 확인된 항목은 코드 회귀가 아니라 CREVIS live 경로의 구조 누락입니다. `IntegratedReadOwnerDormant`가 `0x7E13/0x7E22` route 부재로 의도대로 실패했으며, 따라서 GUI의 정적 CREVIS 행과 실제 동적 Health/DI를 문서에서 분리해 기록하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+17 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+10 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+7 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+7 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -3)
>
> </details>
</details>

반영 완료했습니다.

- CREVIS: WPF는 연결 후 정적 topology를 자동 로드하며 `Nodes=7`, `CREVIS entries=3`으로 표시합니다. 수동 `Reload CREVIS / Topology`도 지원합니다.
- SDO Write 편집: 일반 RPC나 SDO ticket 진행 중에도 입력값 편집이 가능하고, Submit만 직렬화됩니다.
- PI/Bulk: 오프라인·무효 값의 stale raw는 `UNAVAILABLE`로 표시하고 Status/Detail은 유지합니다.
- LASAL `0x7E20`: `Valid=Detail 0`, `SlaveOffline=18`, 기타 invalid=11로 응답합니다.
- D1: Bulk 오프라인 축과 Health/PI 축을 교차 검증하는 baseline→fault→recovery 절차를 추가했습니다.
- D5: disconnect/orphan UI 독립 검증 코어와 회귀 17개를 추가했습니다. 아직 WPF 버튼에는 연결하지 않았습니다.

검증 결과:

- C# Debug/Release: 각각 `575/575 PASS`
- WPF Debug/Release: 각각 `16/16 PASS`
- LASAL SourceOnly/full 정적 계약: PASS
- 독립 코드 리뷰: P1/P2 없음
- `git diff --check`: PASS
- 커밋·스테이징은 하지 않았습니다.
- `git diff --cached --check`는 기존 staged history 파일의 trailing whitespace 때문에 실패합니다. 이번 변경에서 생긴 오류는 아닙니다.

