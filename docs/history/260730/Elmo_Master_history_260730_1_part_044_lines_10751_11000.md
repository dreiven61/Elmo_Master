>
> 현재 두 갈래 모두 실제 결함을 닫는 중입니다. CREVIS 검증기는 Windows CRLF와 `0x/16#/10진수` CASE 표기까지 동일 명령으로 인식하도록 수정하고 있고, Axis PowerOn은 active status poll과 수동 PowerOff 해제가 서로 경합하지 않도록 동시성 계약을 추가하고 있습니다. 아직 PLC/LASAL live capability는 켜지 않았습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 동적 경로의 구조 검증 보강과 Axis PowerOn 재전송 방지 복구를 계속 진행 중입니다. 현재 단계에서는 LASAL IDE를 건드리지 않고, 외부 소스·검증기·WPF/SDK만 완성한 뒤 사용자 빌드가 필요한 최소 구조 변경만 분리해 전달하겠습니다.
>
> 검증기 재검토에서 인라인 중첩 `CASE` 뒤의 라우트가 누락되는 마지막 허점 1건이 확인됐습니다. 실제 구현 게이트가 잘못 통과할 수 있는 수준이라, 해당 형식도 거부하도록 파서를 보정하고 회귀 fixture까지 넣겠습니다.
>
> Axis PowerOn 쪽은 현재 `0x2023` 1회 수락 후 상태 확인이 중단돼도 같은 명령을 재전송하지 않는 경로까지 연결됐습니다. 재시작 시에는 동일 endpoint/BootId/MapRevision/축 참조를 확인하고, 불확실하면 PowerOff와 3회 연속 안전 상태 확인만 허용하도록 마무리·회귀 테스트 중입니다.
>
> CREVIS 검증기는 실제 라우트가 누락돼도 통과하던 두 가지 인라인 `CASE` 우회를 모두 막았습니다. 이제 제가 독립적으로 SourceOnly/full 정적 게이트와 “IDE 구조 미완성은 반드시 실패” 조건을 다시 실행해 확인하겠습니다.
>
> PowerOn 회귀는 단순 단위 테스트가 아니라 실제 자식 WPF 프로세스를 강제 종료한 뒤 재시작하는 시나리오까지 추가됐습니다. 첫 프로세스에서 `0x2023`은 정확히 1회만 나가고, 두 번째 프로세스에서는 `0x2028` 상태 조회만 수행했는지 검증하는 중입니다.
>
> 외부에서 테스트했는데 클라이언트가 비정상적으로 종료될때 소켓이 정상종료 안되는 문제가 있었어. 이 문제 해결했으니 지금 개발중인 소스코드에 이 내용 반영하고 개발된 소스코드 반영해
>
> User context
>
> Attachments:
> - ELMO_TCP_SAME_PEER_TAKEOVER_TEST.md: `C:/work/Elmo/Elmo_Master_test/Elmo_EtherCAT_Test_4Axis/ELMO_TCP_SAME_PEER_TAKEOVER_TEST.md`
> - WTR_TCP_SAME_PEER_TAKEOVER_TEST.md: `C:/work/Elmo/Elmo_Master_test/Elmo_EtherCAT_Test_4Axis/WTR_TCP_SAME_PEER_TAKEOVER_TEST.md`
> - WTR_history_260729.md: `C:/Users/dreiv/source/repos/SIGMATEK/project_repos/WTR/JC_Solution/01.WaferTransferRobot/output/doc/history/WTR_history_260729.md`
>
> 외부 테스트에서 확인된 “비정상 종료 클라이언트의 기존 세션이 새 동일 peer 연결을 막는 문제”를 현재 마스터에 역반영하겠습니다. 테스트 폴더의 두 절차서와 WTR 히스토리, 실제 LASAL 소스를 비교해 변경 근거와 정확한 코드 차이를 먼저 확정하고, LASAL IDE는 사용하지 않은 채 추적 `.st` 구현에 적용한 뒤 기존 개발분 검증도 함께 이어가겠습니다.
>
> LASAL 소스 역반영이므로 저장소 규칙에 따라 코딩 규칙·프로그래밍 방법·오류 방지 가이드를 다시 대조합니다. 이후 테스트 프로젝트에서 런타임 확인된 takeover 관련 소스만 선별하고, `.lba` 같은 생성물은 마스터에 복사하지 않겠습니다.
>
> <details><summary>Created a file, edited 2 files, deleted a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+17 -3)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+132 -10)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+25 -11)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPIPServer/TCPIPServer.st` (+204 -0)
> - Deleted `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_TCPIPServer_RT/_TCPIPServer_RT.st` (+0 -196)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcp` (+1 -1)
>
> </details>
>
> 테스트본 전체를 덮어쓰지 않고 takeover 관련 파일만 선별 반영하고 있습니다. 현재 `TCPMotionInterface`와 새 `TCPIPServer` 소스는 반영됐고, WTR의 다른 방식이나 테스트본의 별도 Recorder 타입 수정은 섞지 않았습니다. 이제 테스트에서 생성된 Comm Network 구조를 동일하게 옮기되 빌드 산출물은 제외합니다.
>
> <details><summary>Created 4 files, edited 10 files, deleted 3 files</summary>
>
> - Deleted `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn` (+0 -291)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn` (+321 -0)
> - Deleted `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/ONE_Comm_Network_Table.st` (+0 -229)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/ONE_Comm_Network_Table.st` (+231 -0)
> - Deleted `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/ConfigObjects.st` (+0 -238)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/ConfigObjects.st` (+239 -0)
> - Wrote `./docs/architecture/ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md` (+175 -0)
> - Wrote `./docs/architecture/ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md` (+7 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+18 -2)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_coding_rules.md` (+5 -3)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+9 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+8 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md` (+2 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+3 -2)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+16 -11)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GROUP_API_IMPLEMENTATION_2026-07-14.md` (+1 -1)
> - Wrote `./docs/architecture/MotionTCPDemo_vs_Elmo_EtherCAT_Test_4Axis_Analysis_2026-07-03.md` (+4 -2)
>
> </details>
>
> 테스트본 비교에서 takeover와 별개로 `LMCRecorderStore`의 기존 LASAL 타입 오류 3건도 실제 수정돼 있는 것을 확인했습니다. 이 세 건은 전체 폴더 복사가 아니라 `DINT ↔ UDINT` 비교식만 명시 변환으로 별도 반영해, 알려진 빌드 오류를 다시 들고 가지 않도록 하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st` (+3 -3)
> - Wrote `./docs/architecture/ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md` (+7 -4)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 외부 테스트에서 검증된 same-peer takeover 수정은 개발본 소스에 선택적으로 반영된 상태입니다. 지금은 세 가지를 마무리하겠습니다: LASAL 정적 계약 검증기 갱신 결과 확인, 소스 리뷰 반영, C# 전체 회귀 테스트 재실행입니다. LASAL 바이너리 메타데이터는 임의 복사하지 않고 개발본을 사용자가 재빌드해야 갱신되도록 유지합니다.
>
> 회귀 테스트는 실행 중입니다. 이번에는 기존 테스트 폴더의 바이너리를 가져다 쓰는 것이 아니라, 개발 저장소에서 마지막으로 빌드된 SDK/WPF 산출물을 직접 실행해 프로세스 재시작 복구까지 확인하고 있습니다.
>
> PC 쪽은 현재 소스로 다시 확인했습니다. SDK Debug/Release는 각각 722/722, WPF smoke Release는 105/105로 모두 통과했습니다. 오래된 문서의 715/715·96/96 표기는 지금 확인된 수치로 맞추겠습니다.
>
> <details><summary>Edited 10 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./LMC_Library/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -6)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
>
> </details>
>
> 리뷰에서 실제 결함 하나가 확인됐습니다. Axis Power On 결과가 미확정인 동안 motion과 Close는 막히지만, SDO Write·Digital Output Write·Recorder 같은 진단 변경 작업은 아직 열려 있었습니다. 문서 계약과 어긋나므로, read-only 조회와 명시적 Power Off 복구는 그대로 허용하면서 새 진단 mutation만 공통 admission에서 차단하도록 수정하겠습니다.
>
> 추가로 더 중요한 경계가 발견됐습니다. SDK는 Power On ACK 직후 continuation을 만들지만, WPF journal은 상태 대기가 끝난 뒤에야 `AcceptedAwaitingProof`로 저장하고 있었습니다. 그래서 “ACK는 받았고 첫 status 응답을 기다리는 중”에 프로세스가 죽으면 status-only 재개 대신 보수적인 Power Off 복구로 떨어집니다. ACK 직후 journal을 동기적으로 내린 다음 polling을 시작하도록 바꾸고, 그 정확한 중간 시점에서 프로세스를 강제 종료하는 회귀 테스트도 추가하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+12 -3)
> - Wrote `./docs/architecture/ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md` (+7 -5)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md` (+7 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -1)
>
> </details>
>
> same-peer 쪽은 정적 검증까지 끝났습니다. SourceOnly 계약은 통과했고, full 계약은 예상대로 개발본 `Classes.lcb`·IDE export가 아직 이전 `_TCPIPServer_RT` 이름이라 명시적으로 중단됩니다. 이 부분은 소스 오류가 아니라 마스터 프로젝트 Save/Rebuild 전 상태입니다. 지금은 Axis Power On의 ACK 직후 내구성 경계를 수정하고 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -5)
>
> </details>
>
> Axis 수정은 세 경계를 함께 묶어 진행 중입니다: ACK 직후 journal flush, 같은 프로세스의 연결 손실→재연결 status-only 복구, 미확정 Power On 동안 새 진단 변경 작업 차단입니다. 조회·cleanup·Power Off 같은 복구 경로는 계속 허용되도록 admission을 분리했고, 기존 API 호출과 모호해지는 오버로드도 제거하도록 점검 중입니다.
>
> 새 테스트는 의도한 위험 시점을 직접 잡습니다. 첫 `0x2028` 응답을 서버에서 보류한 상태에서 journal이 이미 `AcceptedAwaitingProof`이고 WPF 작업이 아직 실행 중임을 확인한 뒤 프로세스를 강제 종료합니다. 별도로 동일 프로세스 연결 손실→재연결에서도 두 번째 세션의 `0x2023=0`, `0x2028`만 전송되는 경로를 실행 중입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 동시 작업 중이던 보완 구현은 현재 SDK 724/724, WPF 107/107까지 통과했습니다. 이제 실제 변경분을 다시 검토하고, 사용자가 열어 둔 LASAL 프로젝트 상태는 건드리지 않은 채 정적 계약과 현재 빌드 메타데이터만 재확인하겠습니다.
>
> 이제부터는 제가 같은 명령으로 독립 재검증합니다. 우선 SDK Debug/Release를 각각 실행한 뒤 WPF 전체 smoke를 실행하고, 테스트 수치가 확정되면 현재 상태를 설명하는 문서만 새 수치로 맞추겠습니다.
>
> 독립 실행 결과는 SDK Debug/Release 각각 724/724, WPF Release 전체 108/108입니다. WPF 수가 앞선 중간 보고보다 1개 늘어난 이유는 같은 세션 취소 후 continuation 재개 회귀가 마지막에 추가됐기 때문이며, 현재 전체 실행으로 그 테스트까지 통과했습니다.
>
> LASAL은 소스 계약이 PASS했고, full 계약은 현재도 `Classes.lcb`가 새 `TCPIPServer` 클래스를 아직 등록하지 않은 상태라 의도대로 FAIL입니다. 즉 외부 편집 소스 반영은 끝났지만, master 프로젝트에서 LASAL Save/Rebuild를 한 번 해야 생성 메타데이터가 동기화됩니다.
>
> 최종 코드 리뷰에서 동작상 P0/P1은 없었습니다. 다만 observer의 정상 1회, ACK 거절/전송 전 취소의 0회, 전송 후 취소의 1회를 직접 고정하는 SDK 테스트가 비어 있어 그 경계만 추가한 뒤 전체 수치를 다시 확정하겠습니다.
>
> <details><summary>Edited 12 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AxisPowerStateWaitContractTests.cs` (+111 -4)
> - Wrote `./docs/architecture/AXIS_POWER_ON_ACCEPTED_ONCE_RECOVERY_2026-07-29.md` (+30 -13)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+15 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+5 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./LMC_Library/README.md` (+4 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+6 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
>
> </details>
>
> 코드와 문서는 현재 수치로 맞췄습니다. 마지막으로 whitespace, LASAL ASCII, 작업 프로세스 잔존, 변경 파일 상태를 확인합니다. 스테이징된 과거 대용량 history 파일은 제가 건드리지 않으며, 그 파일의 기존 trailing whitespace는 별도 상태로 보고하겠습니다.
>
> 사용자가 열어 둔 LASAL 프로세스가 지금은 모두 종료된 상태라, 종료 과정에서 IDE 메타데이터가 저장됐을 가능성을 배제하지 않고 full 계약을 한 번 더 확인하겠습니다.
</details>

반영 완료했습니다.

- 외부 시험에서 성공한 동일-IP stale socket takeover를 master에 선별 반영했습니다.
  - [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:402)
  - [TCPIPServer.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPIPServer/TCPIPServer.st:163)
  - [Comm_Network.lcn](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn:62)
  - `MaxConnections=2`, 같은 IPv4의 기존 socket shutdown 요청, 새 owner 교체, 기존 session/queue/RPC 초기화, 늦은 데이터·disconnect 격리를 포함합니다.
- 테스트 프로젝트 전체와 `.lba/.lob/.ldi/ProjectInternal`은 복사하지 않았습니다.
- 사용자 외부 PLC 시험의 동일-IP 비정상 종료 후 재접속은 PASS지만, master 프로젝트 자체의 LASAL 빌드/runtime 증거는 아직 아닙니다.
- 상세 판정 기준은 [ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md](./docs/architecture/ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md:1)에 정리했습니다.

병행 개발도 반영했습니다.

- Axis Power On ACK 직후 journal을 `AcceptedAwaitingProof`로 저장하고 첫 `0x2028`보다 먼저 영속화
- 같은 프로세스 disconnect/reconnect와 프로세스 재시작 모두 `0x2023` 재전송 없이 status-only 복구
- Axis Power On 미해결 중 SDO Write, Digital Output, Bulk/Recorder mutation 차단
- safety, read-only, cleanup, 필수 exact readback은 허용
- 관련 계약: [AXIS_POWER_ON_ACCEPTED_ONCE_RECOVERY_2026-07-29.md](./docs/architecture/AXIS_POWER_ON_ACCEPTED_ONCE_RECOVERY_2026-07-29.md:1)

검증 결과:

- SDK Debug/Release: 각각 `725/725 PASS`
- WPF Release 전체 smoke: `108/108 PASS`
- LASAL SourceOnly: PASS
- `git diff --check`: PASS
- LASAL 변경 소스 7-bit ASCII: PASS
- LASAL full static: 의도한 FAIL
  - 현재 `Classes.lcb`가 새 `Class\TCPIPServer\TCPIPServer.st`를 아직 등록하지 않았습니다.
- 기존에 staged된 대용량 history 파일의 trailing whitespace 때문에 `git diff --cached --check`만 별도로 실패합니다. 이번 변경과는 무관하며 건드리지 않았습니다.

이제 master LASAL 프로젝트에서 다음 작업이 필요합니다.

1. `C:\work\Elmo\Elmo_Master\Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcp`를 엽니다.
2. `TCPMotionInterface`, `TCPIPServer`를 Reload Class한 뒤 Save/Rebuild/Link합니다.
3. `Find in Implementation`으로 다음을 확인합니다.
   - `TCPIPServer::SetSocketParameter`
   - `TCPMotionInterface::CurrentPeerIPv4`
   - `RetiringSock`
   - `LastTakeoverResult`
4. 빌드 오류 또는 경고를 알려주면 full static을 다시 확인하겠습니다.

성공 빌드 확인 전이므로 커밋, 스테이징, 테스트 폴더 복사는 하지 않았습니다. CREVIS 동적 I/O IDE 구조 작업도 이번 socket 변경과 별개로 남아 있습니다.

<details><summary>30 previous messages</summary>

> 계속 진행하겠습니다. 현재 LASAL full gate는 사용자 Rebuild를 기다리는 상태이므로, 그동안 남아 있는 Axis journal 오류 격리 회귀를 먼저 닫고, 동시에 CREVIS/Diagnostics에서 IDE 없이 다음으로 구현 가능한 항목을 현재 소스 기준으로 다시 소팅하겠습니다.
>
> 현재 worktree에는 외부 추가 변경이나 실행 중인 LASAL 프로세스가 없습니다. 다음 회귀는 단순 테스트용 플래그를 심기보다, disconnect 정리 순서를 작은 orchestrator로 분리해 Axis journal 예외가 나도 후속 정리가 계속된다는 계약을 직접 검증하는 방향이 가장 안전한지 확인 중입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 다음 CREVIS 단계도 확인됐습니다. 현재 C# API/GUI 쪽은 준비돼 있지만 LASAL 쪽은 아직 `StaticTopologyOnly`이고, 다음 단계는 IDE에서 typed client 3개와 네트워크 연결을 먼저 등록해야 합니다. 이 부분은 임의로 텍스트 편집하지 않고, 우선 지금 진행 중인 소켓 종료 시 journal 오류 격리 회귀를 완성하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisPowerOnRecoveryIntegrationTests.cs` (+173 -0)
>
> </details>
>
> 실제 파일 잠금으로 Axis journal의 `File.Replace`를 실패시키는 회귀를 추가했습니다. 이제 실제 `CloseConnectionAsync → ConnectionStateChanged` 경로를 타면서 motion·group·diagnostics·topology 정리가 끝까지 실행되는지 먼저 단독 시험합니다.
>
> 방금 실패는 새 회귀 로직이 아니라 이 WPF 프로젝트를 `dotnet run`으로 직접 빌드해 XAML 생성이 빠진 도구 선택 오류입니다(`InitializeComponent` 전체 미생성). 기존에 통과했던 Visual Studio Build Tools/MSBuild 경로로 즉시 다시 검증하겠습니다.
>
> 단독 회귀는 PASS했습니다. 동시에 전체 WPF 109건 회귀를 돌리고, CREVIS IDE 선행 작업과 겹치지 않는 다음 구현으로 Axis Reset의 “명령 1회 + 오류 해제 3회 안정 확인” SDK 계약을 병행 착수했습니다.
>
> 전체 WPF 회귀도 `109/109 PASS`입니다. 실패 주입에서도 Axis 기록은 `ArmedBeforeDispatch`로 fail-closed 유지되고, motion/group/diagnostics 정리와 topology 초기화·UI 갱신은 정상 완료됐습니다. 이 검증 결과를 현재 상태 문서에 맞춘 뒤 Axis Reset 구현을 합칩니다.
>
