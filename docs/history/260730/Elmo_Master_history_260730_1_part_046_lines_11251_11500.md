>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+8 -6)
> - Wrote `./docs/architecture/ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md` (+3 -2)
>
> </details>
>
> 새 deadline 회귀 3개를 포함한 SDK Debug 전체가 `778/778 PASS`했습니다. 이제 동일 결과로 Release를 확인하는 동안, 현재 문서의 오래된 `752/762/775` 및 WPF `110` 수치를 `778/111` 기준으로 정리하겠습니다.
>
> <details><summary>Edited 14 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md` (+1 -1)
> - Wrote `./docs/architecture/AXIS_POWER_ON_ACCEPTED_ONCE_RECOVERY_2026-07-29.md` (+2 -2)
> - Wrote `./docs/architecture/AXIS_RESET_STABLE_ERROR_CLEARANCE_2026-07-29.md` (+3 -3)
> - Wrote `./docs/architecture/DRIVE_DS402_FAULT_ERROR_DIAGNOSTICS_2026-07-29.md` (+3 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -6)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
> - Wrote `./docs/architecture/AXIS_STOP_STABLE_STANDSTILL_2026-07-29.md` (+11 -1)
>
> </details>
>
> 추가 경합 점검에서 응답 drain 직후 deadline이 걸리는 아주 좁은 구간에 “실제 연결 해제 여부”가 잘못 표시될 수 있는 경우가 발견됐습니다. transport detach 성공값을 원자적으로 추적하도록 마지막 보정 후 전체 테스트를 다시 돌리고 있습니다.
>
> Stop 쪽 결함은 `16/16`, 전체 `778/778`로 닫혔습니다. 같은 legacy drain 패턴이 기존 Reset과 PowerOff에도 남아 있다는 감사 결과가 나왔으므로, 이미 만든 안전 facade끼리 deadline/evidence 계약이 달라지지 않도록 두 경로도 병렬로 같은 기준에 맞추겠습니다.
>
> Reset·PowerOff까지 같은 계약으로 보정됐고 최종 SDK 개별 실행은 Debug/Release `784/784 PASS`입니다. 이제 문서 수치를 `784`로 맞추고, 루트에서 SDK·WPF·LASAL 검증을 한 번 더 독립 실행하겠습니다.
>
> <details><summary>Edited 14 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md` (+1 -1)
> - Wrote `./docs/architecture/AXIS_POWER_ON_ACCEPTED_ONCE_RECOVERY_2026-07-29.md` (+1 -1)
> - Wrote `./docs/architecture/AXIS_RESET_STABLE_ERROR_CLEARANCE_2026-07-29.md` (+2 -2)
> - Wrote `./docs/architecture/AXIS_STOP_STABLE_STANDSTILL_2026-07-29.md` (+1 -1)
> - Wrote `./docs/architecture/DRIVE_DS402_FAULT_ERROR_DIAGNOSTICS_2026-07-29.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+0 -0)
> - Wrote `./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md` (+10 -3)
> - Wrote `./docs/architecture/AXIS_RESET_STABLE_ERROR_CLEARANCE_2026-07-29.md` (+11 -2)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+6 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+6 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md` (+3 -1)
>
> </details>
</details>

반영 완료했습니다.

- 외부 시험본의 same-peer takeover 로직을 마스터 [TCPIPServer.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPIPServer/TCPIPServer.st:163), [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:402), Comm Network에 반영했습니다.
- 외부 시험본과 실행 로직 및 `.lcp/.lcn` 구조가 동일함을 대조했습니다.
- Stop·Reset·PowerOff의 무응답 deadline과 전송 직전 취소 경합도 보정했습니다. write 이후 응답이 없으면 연결을 `Faulted`로 전환하고 명령을 자동 재전송하지 않습니다.
- SDK Debug/Release: 각각 `784/784 PASS`
- WPF Release smoke: `111/111 PASS`
- LASAL SourceOnly: PASS
- `git diff --check`: PASS

마스터 LASAL에서 남은 작업은 다음입니다.

1. [Elmo_EtherCAT_Test_4Axis.lcp](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcp) 열기
2. `TCPIPServer`, `TCPMotionInterface`, `Comm_Network` Save/Generate
3. 전체 Rebuild/Link
4. `SetSocketParameter`, `ConnSocketInfo`, `DataHandling` Find in Implementation 확인
5. 새 `CInvalidArgException`이 없는지 확인

현재 full static은 이전 생성물인 `Classes.lcb`에서 먼저 중단됩니다. `Networks.lcb`, 관련 `.lba`, 루트 `.lcb`, `MaeExp.*`, `MultiMasterExp.mme`도 재생성이 필요합니다. 시험 폴더 생성물을 마스터에 직접 복사하면 현재 개발분이 유실될 수 있으므로 복사하지 마십시오.

외부 시험 프로젝트의 동일 IP 재접속은 PLC 런타임 PASS지만, 마스터 프로젝트 자체의 Rebuild/Link와 PLC 다운로드는 아직 미검증입니다. 이번에는 커밋·스테이징·테스트 폴더 복사를 하지 않았습니다. 기존 staged 히스토리 파일의 trailing whitespace 때문에 `git diff --cached --check`만 계속 실패합니다.

상세 상태는 [same-peer 시험 문서](./docs/architecture/ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md:177)에 정리했습니다.

<details><summary>91 previous messages</summary>

> 계속 진행하겠습니다. LASAL IDE 재생성이 필요한 부분은 사용자 빌드 대기 상태로 두고, 그와 독립적으로 진행 가능한 다음 핵심 작업을 현재 소스·설계 문서에서 다시 소팅한 뒤 바로 구현하겠습니다. 우선 Stop의 WPF preemption 구조와 남은 CREVIS/API 항목을 병렬 감사하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 다음 구현 단위는 기존 계획대로 `Axis Stop`의 “명령 1회 전송 + 상태 전용 재개” 구조를 먼저 확정하겠습니다. 이 구조가 잡혀야 WPF에서 긴 정지 확인 중에도 다른 안전 명령을 막지 않고, `0x2022` 재전송 없이 모니터링만 이어갈 수 있습니다. 동시에 진행 중인 코드·UI·로드맵 검토 결과를 받아 범위를 고정하겠습니다.
>
> 현재 소스에서 확인한 핵심 차이는 명확합니다. `Power Off`는 이미 `Begin(0x2023 1회)`과 `Resume(0x2028만)`로 분리돼 있지만, `Stop`은 WPF가 여전히 `StopAsync` 후 별도 폴링을 수행합니다. 따라서 SDK에 세션·축에 묶인 Stop continuation을 추가하고, WPF가 그 continuation만 재개하도록 바꾸는 범위로 좁혀 진행 중입니다.
>
> 병렬 검토 결과도 같은 우선순위를 확인했습니다. CREVIS 동적 `0x7E13/0x7E22`는 아직 LASAL IDE에서 객체·채널·네트워크 구조 생성이 필요한 상태라 지금 외부 편집만으로 밀면 계약을 위반합니다. 그래서 이번에는 LASAL을 건드리지 않고 Stop split·WPF 회귀를 끝낸 뒤, 다음 소스-only 항목인 Axis Power On의 post-write deadline 보강으로 이어가겠습니다.
>
> Stop SDK 쪽은 현재 continuation 상태 모델과 동시성 계약까지 작성 중이고, WPF 쪽은 실제로 생성된 API 형태를 확인한 뒤 붙이도록 분리했습니다. 여기서 중요한 검증은 단순 성공 경로뿐 아니라 오래된 continuation·중복 Resume이 wire를 전혀 쓰지 않는지, 상태 응답이 끊기면 연결을 `Faulted`로 격리하는지까지 포함합니다.
>
> 현재 SDK 파일은 기존 구현을 부분 수정하는 방식이 아니라 계약 전체를 한 번에 교체하는 중이라 잠시 파일이 빠진 상태였습니다. 의도된 교체 작업임을 확인했고, 완전한 소스가 복원되기 전에는 빌드나 WPF 연결을 시작하지 않도록 막아 두었습니다. 복원 직후 제가 독립적으로 diff와 동시성 경계를 다시 검토하겠습니다.
>
> 교체 작업이 예상보다 오래 걸려 원본 파일이 비어 있는 시간이 길어졌습니다. 해당 작업자에게 테스트 확장보다 컴파일 가능한 파일 복원을 우선하도록 지시했습니다. 복원이 지연되면 즉시 작업을 회수해 제가 직접 복구하겠습니다. 현재 다른 파일에는 그 중간 상태를 전제로 한 변경이 적용되지 않았습니다.
>
> Stop SDK의 전체 파일은 복원됐고 현재 컴파일은 통과했습니다. 제가 확인한 구현은 `MutationGate`를 ACK·continuation 게시까지 유지하고, Resume 시 공유 `StatusObservationGate`만 점유하며, 오래된/완료된 continuation을 첫 wire 전에 거부합니다. 이제 집중 회귀와 WPF 연결을 마무리하는 단계입니다.
>
> 집중 회귀 9개를 추가해 실행 중입니다. 범위는 Begin/Resume 정확한 wire 순서, timeout 뒤 Resume 재시도 시 `0x2022` 미재전송, 새 Stop에 의한 기존 continuation supersede, 모니터 preemption, 동시 Begin/Resume, 재접속 후 stale continuation zero-wire, pending Power On 증거 오염 방지입니다.
>
> Stop 단계는 완료됐습니다. SDK Debug/Release `793/793`, WPF Release smoke `112/112`가 통과했고, Stop 상태 확인 중 명시적 Power Off가 선점해도 Stop은 1회만 전송되는 회귀까지 포함했습니다. 이어서 이미 확인된 다음 결함인 Axis Power On의 “write 후 응답 없음” total-deadline 미적용을 보강하고 있습니다.
>
> Power On 보강은 기존 durable journal 순서를 그대로 유지하면서 진행합니다. 즉 ACK 결과 게시 → pending continuation 설치 → mutation gate 해제 → journal observer 호출 순서는 바꾸지 않습니다. 바뀌는 것은 ACK나 `0x2028` 응답이 deadline까지 오지 않을 때 연결을 재사용하지 않고 `Faulted`로 격리하고, `NotAttempted/OutcomeUncertain/Accepted` 증거를 구분하는 부분입니다.
>
> 독립 코드 리뷰에서 두 건을 바로 잡아야 합니다. 하나는 custom `StableSampleCount`로 Begin한 continuation을 간편 Resume 오버로드가 기본값 3으로 잘못 거부하는 SDK 호환 문제이고, 다른 하나는 ACK 게시 직후 deadline/cancel 예외에 continuation이 들어 있어도 WPF가 이를 놓치는 경계입니다. 둘 다 다음 wire 전에 결정되는 exact-once 문제라 지금 바로 수정합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxisStopWait.cs` (+11 -4)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+36 -4)
>
> </details>
>
> Stop 리뷰에서 나온 두 경계 수정도 반영했습니다. custom stable count Resume과 ACK-게시 deadline continuation 보존 테스트가 추가됐고, 재검토에서 새 P0–P2는 없었습니다. Power On 쪽은 이제 accepted 경로뿐 아니라 재시작용 read-only `WaitForPowerStateAsync`도 같은 total-deadline/Faulted 격리 규칙으로 맞추고 있습니다.
>
> 현재까지 확인된 자동 회귀 증분은 Stop 11개(초기 9개 + 리뷰 보강 2개), Power On deadline 6개입니다. Power On 테스트 본문을 붙인 뒤 전체 suite를 다시 돌리고, 그 최종 숫자로 문서의 기존 `784/784`, `111/111` 표기를 한 번에 갱신하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+79 -0)
>
> </details>
>
> Power On SDK Debug 전체가 `801/801`로 통과했습니다. 추가로 WPF 실패 화면에 submission outcome, ACK 유무, status poll 수, transport invalidation을 표시하도록 연결했습니다. 이제 Release와 WPF smoke를 독립 실행해 API 변경이 복구 journal 흐름을 깨지 않았는지 확인합니다.
>
> 문서 갱신은 기능 설명을 고치기 전에 현재 검증 숫자부터 일괄 정규화하겠습니다. 이번 최종 기준은 SDK Debug/Release `801/801`, WPF Release smoke `112/112`이며, 실제 PLC·축 증거와 혼동하지 않도록 기존 경계 문구는 유지합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/AXIS_STOP_STABLE_STANDSTILL_2026-07-29.md` (+47 -13)
> - Wrote `./docs/architecture/AXIS_POWER_ON_ACCEPTED_ONCE_RECOVERY_2026-07-29.md` (+26 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -4)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+11 -0)
>
> </details>
>
> Power On 독립 리뷰에서는 의미 불일치 1건만 나왔습니다. read-only `WaitForPowerStateAsync`가 실제로는 ACK도 continuation도 없이 `0x2028`만 사용하면서 결과에 `ReusedAcceptedAcknowledgement=true`를 넣고 있었습니다. wire 동작 문제는 아니지만 공개 결과가 거짓이므로 `false`로 고치고 성공 회귀에 고정하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxisPowerStateWait.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AxisPowerStateWaitContractTests.cs` (+8 -0)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+21 -6)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> PowerOn/Stop 경계 보강까지 반영된 상태를 기준으로, 마지막 1줄 수정 재검증과 문서 정합성을 먼저 닫겠습니다. 그 다음 LASAL IDE는 건드리지 않고 다음 우선순위인 Group 정지·전원 경로의 post-write 타임아웃 취약점을 병렬로 감사해 계속 구현하겠습니다.
>
> 문서에서 구현 전 상태로 남아 있던 Stop 문구는 이미 새 Begin/Resume 계약으로 교정된 것을 확인했습니다. 현재는 자동 테스트를 재실행하는 동안 LASAL 소스 정적 계약과 오래된 테스트 개수 표기를 별도로 검사하고 있습니다.
>
> 최신 1줄 수정까지 포함해 자동 검증은 다시 통과했습니다: SDK Debug 801/801, Release 801/801, WPF Release smoke 112/112이며 실패는 모두 0입니다. LASAL SourceOnly 계약도 PASS했고, 이 결과는 PC/fake-RPC·정적 검증이지 실제 PLC/축 동작 증거는 아닙니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_SOURCE_REVIEW_2026-07-15.md` (+4 -2)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+74 -0)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+24 -2)
>
> </details>
>
> 다음 취약점은 확인됐습니다. Group Stop과 공용 `0x2045` 상태 조회가 “전체 타임아웃”이라고 표시되지만, 실제로는 송신 후 응답이 없으면 타임아웃을 넘겨 계속 기다리고 연결도 재사용 가능한 것으로 남습니다. 우선 이 무응답 경계를 fail-closed로 바꾸고, 실제 write-commit 이후에만 “명령이 송신됐을 수 있음”을 기록하는 G1 범위를 구현하겠습니다.
>
> G1 수정 범위는 WPF나 LASAL을 건드리지 않고 SDK 3개 모델/구현과 Group 계약 테스트에만 제한했습니다. 핵심 회귀 기준은 “송신 전 취소는 0회·연결 재사용”, “송신 후 무응답은 정해진 시각에 연결 Faulted”, “이미 ACK된 Stop의 상태 무응답은 Accepted 증거 유지”입니다.
>
> G1의 새 계약 테스트까지 Debug 804/804가 통과했습니다. 무응답 Stop ACK·무응답 Stop 상태·무응답 Group power 상태가 정해진 deadline에 종료되고 연결이 `Faulted`로 전환되는 것을 확인했으며, 송신 직전 취소/기한 만료는 Stop wire 0회로 유지됩니다. Release와 독립 diff 리뷰를 마무리 중입니다.
>
> G1은 독립 리뷰에서도 P0–P2 문제가 없었습니다. 이제 실제 재전송 위험을 없애는 G2로 넘어갑니다: Group Stop을 `Begin(0x2085 1회)`과 `Resume(0x2045만)`으로 분리하고, accepted continuation이 timeout·취소·상태 오류 뒤에도 남도록 SDK부터 구현하겠습니다.
>
> G2 SDK는 기존 Group Enable의 coordinator와 Group Stop의 mutation-generation 검증을 합쳐야 해서, 단순 메서드 분할만 하지 않고 stale session·중복 Resume·다른 Group mutation 간섭까지 zero-wire 계약으로 고정하고 있습니다. WPF 쪽은 일반 버튼과 qualification cleanup이 같은 accepted continuation을 재사용하도록 별도 변경 지점을 감사 중입니다.
>
> G2 구현은 현재 continuation 모델과 coordinator 상태부터 반영 중입니다. 기존 raw Group mutation이 끼면 과거 Stop proof를 잘못 완료 처리하지 않도록 mutation generation을 continuation에 그대로 귀속시키고 있으며, 새 Begin이 실제로 accept된 경우에만 이전 continuation을 supersede하도록 잡았습니다.
>
> G2 내부 구현은 기존 804/804 회귀를 깨지 않고 컴파일·전체 테스트를 통과했습니다. 지금은 새 split 전용 11개 테스트를 추가해 `0x2085` 1회, Resume `0x2045` 전용, supersede/stale/concurrent zero-wire, 간섭·무응답·선점 경계를 고정하는 단계입니다.
>
> SDK G2는 Debug/Release 815/815로 완료됐습니다. 이제 WPF를 두 갈래로 나눕니다: 일반 Group Stop 버튼은 Begin 후 즉시 durable accepted 기록을 남기고 status-only Resume으로 전환하며, qualification cleanup은 이미 accept된 continuation이 있으면 새 `0x2085`를 보내지 않고 같은 continuation만 재개하도록 수정합니다.
>
> WPF 변경은 두 경로의 공통 불변식을 맞추는 중입니다. accepted ACK는 command gate를 놓기 전에 durable journal과 volatile continuation에 같이 기록하고, 그 이후 모니터·cleanup은 `0x2045`만 사용합니다. reconnect에서는 continuation을 직렬화하지 않으며 자동 `0x2085` 재송신도 하지 않습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Group Stop의 재전송 방지 구조를 WPF 일반 동작과 Qualification 경로에 각각 반영 중입니다. 두 변경이 합쳐지는 대로 컴파일 충돌을 정리하고, SDK·WPF·LASAL 정적 계약을 한 번에 다시 검증하겠습니다.
>
> SDK 독립 리뷰에서 구현 결함은 없었고, 동시 Resume 테스트 한 건이 실제 guard 분기를 확정하지 못하는 테스트 경쟁 조건만 확인됐습니다. 이 테스트를 결정적으로 고친 뒤 전체 계약 테스트를 다시 돌리겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/GroupStopWaitContractTests.cs` (+7 -2)
>
> </details>
>
> 현재 SDK는 리뷰에서 지적된 테스트 경쟁 조건까지 수정한 뒤 Debug `815/815`를 다시 통과했습니다. WPF 쪽은 Stop ACK를 한 번만 남긴 뒤 상태 조회만 재개하도록 합쳐졌고, 지금은 일반 버튼·Qualification cleanup의 선점/실패 회귀 테스트를 실행 중입니다.
>
> WPF 전체 회귀는 최종적으로 Release `114/114`를 통과했습니다. 새 회귀에서 일반 Stop 선점은 `0x2085=1, 0x204B=1, 0x2045=4`, Qualification 실패 후 cleanup은 `0x2085=1, 0x2045=4`로 확인돼, 수락된 Stop의 재전송은 없습니다. 다음 단계로 Group Enable의 ACK/status 무응답도 total deadline에서 연결을 무효화하도록 SDK를 보강 중입니다.
>
> Group Enable 쪽은 기존 동작과 충돌하던 지점을 정리했습니다. 변경 후에는 pre-write 취소/시간초과는 `NotAttempted`로 0건 송신, write 뒤 ACK 무응답은 `OutcomeUncertain`으로 연결 `Faulted`, ACK 수락 뒤 status 무응답은 동일 continuation을 보존하도록 테스트를 맞추고 있습니다. 기존 send-priority 선점 예외도 그대로 전달되도록 호환성을 유지했습니다.
>
> Group Enable hardening까지 완료됐고 SDK Debug/Release가 각각 `818/818`을 통과했습니다. ACK 무응답은 Enable 1건 후 continuation 없이 `OutcomeUncertain/Faulted`, status 무응답은 Enable 1건·Status 1건 후 accepted continuation을 유지합니다. 이제 이 변경을 독립 리뷰하고 WPF `114/114` 및 LASAL 정적 계약을 최종 재실행하겠습니다.
>
> `dotnet msbuild`는 이 .NET Framework WPF 프로젝트의 XAML build task를 로드하지 못해 `InitializeComponent` 대량 오류가 났습니다. 소스 오류가 아니라 빌드 도구 선택 문제이므로, 이전과 같은 Visual Studio MSBuild로 다시 실행해 실제 WPF 결과를 확인하겠습니다.
>
> 독립 리뷰에서 Group Enable에 실제로 고쳐야 할 세션 경쟁 1건이 발견됐습니다. 최종 `0x2045` 응답 게시가 Close/Reopen과 선형화되지 않아 오래된 세션의 마지막 샘플이 성공으로 반영될 수 있는 경로입니다. session-bound 게시로 바꾸고 Close/Reopen 회귀를 추가한 뒤, evidence 경과시간 0으로 남는 보조 문제도 같이 수정하겠습니다.
>
> 현재 자동 검증 기준은 SDK `818/818`, WPF `114/114`, LASAL SourceOnly PASS입니다. 다만 Group Enable의 Close/Reopen 경쟁 수정이 추가되면 SDK 수치가 다시 늘어나므로 문서의 최종 숫자는 그 수정과 Axis Stop mutation-attribution 회귀까지 끝난 뒤 한 번 더 맞추겠습니다.
>
> Group Enable 리뷰 수정도 통과했습니다. SDK는 이제 `819/819`이며, Close/Reopen 경쟁에서 이전 세션의 마지막 `0x2045`는 관찰되지 않고 continuation은 `PollCount=2` 상태로 pending 유지됩니다. 새 세션에서 Enable/Status 자동 재전송도 0건입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Group Enable 재검토는 이상 없이 끝났고 Release 기준 819/819가 확인됐습니다. 지금은 같은 축에서 후속 명령이 들어왔을 때 이전 Axis Stop 완료 판정을 잘못 인정하지 않도록 하는 변경과 테스트를 마무리한 뒤, 전체 회귀 검증까지 이어가겠습니다.
>
> Axis Stop 간섭 방지는 SDK 테스트 5개를 추가해 Debug 824/824까지 통과했습니다. 같은 축의 후속 명령은 이전 Stop 증명을 무효화하고, 다른 축·실제 송신 전 취소·입력 검증 실패는 무효화하지 않는 경계가 확인됐습니다. 이 예외가 GUI에서도 원인과 세대 번호를 보이도록 WPF 표시 경로도 함께 맞추겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+19 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+7 -7)
>
> </details>
>
> 현재 Axis Stop 변경은 Release 전체 회귀와 독립 코드 리뷰를 동시에 돌리고 있습니다. 이 단계는 C# SDK/WPF 쪽만 변경하며 LASAL 소스·프로젝트 파일은 건드리지 않습니다. 다음 구현 후보인 Axis Reset의 `Reset 1회 + 상태 조회만 재개` 분리 계약도 소스 기준으로 먼저 검토 중입니다.
