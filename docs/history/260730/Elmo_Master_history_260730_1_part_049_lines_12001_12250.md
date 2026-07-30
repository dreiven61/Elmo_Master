
이번에는 스테이징·커밋과 테스트 폴더 복사는 하지 않았습니다.

<details><summary>14 previous messages</summary>

> 계속 진행하겠습니다. 현재 소스와 설계 문서에서 “이미 구현됨 / PC 검증만 됨 / LASAL IDE·실기 대기 / 아직 미구현”을 다시 대조해, IDE 없이 진행 가능한 다음 우선순위 기능을 바로 구현하겠습니다. 먼저 SDK·WPF·LASAL 세 축을 병렬로 감사하겠습니다.
>
> 현재 설계와 히스토리까지 대조한 결과, 기존에 명시된 다음 코드 단계는 `Axis Velocity Override`입니다. Homing·SetPosition은 물리 Reference 입력과 상태 계약이 아직 없어 계속 차단하는 게 맞고, Override는 기존 `_LMCAxis.Override` 채널과 현재 Control Service 연결만으로 구현할 수 있어 새 LASAL 객체/Network가 필요 없습니다. 병렬 감사 결과로 이 우선순위를 한 번 더 확인한 뒤 이 기능을 SDK→LASAL→WPF→검증 순서로 넣겠습니다.
>
> 감사 결과 우선순위를 조정합니다. `Axis Velocity Override`는 구현 경로는 명확하지만 현재 설계상 ownership·지속성 계약을 실기 gate 뒤 확정하도록 보류돼 있습니다. 반면 우선순위 ‘상’인 `Group Disable(0x2048)`은 아직 ACK만 보고 끝나며, 강제 종료 후 안정적인 Disabled 상태를 무재전송으로 증명하는 경로가 없습니다. 따라서 먼저 Group Disable을 SDK와 WPF의 accepted-once 영속 복구까지 완성하고, 그 다음 Override로 넘어가겠습니다.
>
> LASAL 쪽은 추가 수정 없이도 계약이 성립하는 것을 확인했습니다. `0x2048` ACK는 Unlock 호출과 즉시 `LockState=0`까지 확인하지만, 재시작 복구 완료 조건은 전원이 켜진 상태에서 `IsDisabled && !IsStandby`가 3회 연속이어야 합니다. 단순 `IsDisabled`만 보면 PowerOff를 Disable 완료로 오판하므로 SDK 테스트에 이 분기를 포함해 구현 중입니다.
>
> Group Disable은 기존 Group Enable/Power/Stop과 같은 세션 공유 코디네이터에 통합하고 있습니다. 직접 `0x2048` 재호출과 accepted continuation이 동시에 존재하지 않도록 raw API도 replay guard를 갖게 하며, WPF 저널은 기존 파일을 v2로 올려 Lock→Unlock을 같은 durable record에서 원자 교체하도록 구현 중입니다.
>
> 현재 저널 v2에는 방향 필드와 legacy v1 호환, Lock→Unlock 원자 교체, 손상된 방향값 fail-closed 테스트까지 들어갔습니다. SDK는 같은 세션 코디네이터에 Disable continuation을 추가했으며, 성공·PowerOff 간섭 경로를 먼저 컴파일 가능한 최소 단위로 닫고 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Group Disable은 안전상 “응답 수신”이 아니라 `PowerOn + Disabled + !Standby`가 3회 연속 확인돼야 완료로 보도록 구현 중입니다. 방금 WPF 복구 저널은 Lock/Unlock 방향을 구분하는 v2로 확장됐고 단위 테스트 10건이 통과했습니다. 이제 SDK 구현을 받아 검토한 뒤, GUI의 비정상 종료·재시작 복구까지 연결하겠습니다.
>
> 현재 GUI 쪽에서 가장 위험한 기존 동작은 `0x2048` ACK 직후 저널을 해제하고 “Disable 완료”로 표시하던 부분입니다. 이를 바꿔 ACK 이후에도 복구 저널을 유지하고, 중간에 GUI가 종료되면 재시작 후 `0x2045` 상태 조회만 이어가도록 연결하고 있습니다. 전원이 꺼진 상태는 Disable 성공으로 오인하지 않도록 별도 간섭 상태로 처리합니다.
>
> 저널 리뷰에서 실제 결함 하나를 잡았습니다. Unlock 전송 결과가 불확실해 `RecoveryRequired`가 된 뒤, 사용자가 명시적으로 Disable을 재시도해 ACK를 받아도 기존 상태 전이 규칙이 이를 거부하는 문제입니다. 자동 재전송은 계속 금지하되, Unlock 방향에서만 명시적 재시도 ACK를 받아 `AcceptedAwaitingProof`로 전이할 수 있게 수정하고 테스트를 추가하고 있습니다.
>
> 추가 리뷰에서 손상된 복구 파일이 16 KiB 상한 검증 전에 통째로 메모리에 읽히는 문제도 확인했습니다. 파일 길이를 먼저 검사한 뒤 제한된 크기만 읽도록 고치고, 과대 파일 회귀 테스트를 넣고 있습니다. 정상 흐름뿐 아니라 장애·변조 시에도 fail-closed가 유지되는지 함께 확인하겠습니다.
>
> GUI 1차 통합은 완료됐고 Release 빌드도 통과했습니다. 이제 Disable은 ACK만으로 완료 처리하지 않으며, 복구 저널 테스트는 11/11 통과했습니다. 다음으로 기존 GUI 테스트를 새 3회 상태 증거 계약에 맞추고, 프로세스를 ACK 직후 강제 종료한 뒤 재시작해 `0x2048`이 다시 나가지 않는 실제 복구 테스트를 추가하겠습니다.
>
> SDK 리뷰에서도 동시성 결함을 추가로 확인했습니다. ACK 저널 기록용 observer가 끝나기 전에 다른 스레드가 상태 조회를 시작할 수 있는 창, 그리고 마감시간 직후 도착한 마지막 상태를 성공으로 인정할 수 있는 경계 조건입니다. observer 완료 전 상태 전송을 막고, 상태 publication 시점에 취소·마감시간을 다시 검사하도록 수정 중입니다.
>
> ACK 처리와 연결 종료가 동시에 발생하는 더 중요한 경합도 발견했습니다. 기존 새 코드가 ACK를 세션에 게시한 뒤 continuation을 별도로 등록해서, 그 사이 Close/Reconnect가 끼면 새 세션에 오래된 pending 상태가 남을 수 있었습니다. ACK·continuation·pending 등록을 동일 세션 publication 안에서 원자 처리하도록 구조를 바꾸고 회귀 테스트를 추가하고 있습니다.
</details>

<details><summary>69 previous messages</summary>

> Group Disable의 ACK 이후 안정 상태 확인과 비정상 종료 복구를 계속 마무리하겠습니다. 먼저 병행 작업의 현재 파일·테스트 상태를 다시 확인하고, 남은 동시성 경합과 WPF fail-closed 조건을 수정한 뒤 SDK/WPF 전체 회귀 테스트까지 통과시키겠습니다.
>
> 현재 소스에는 앞서 잡은 경합 수정이 실제로 반영돼 있습니다. SDK에는 17개 Disable 전용 계약 테스트가 추가됐고, GUI에도 저널 장애 시 활성 Lock takeover·Unlock 재시도를 전송 전에 막는 fail-closed 조건이 들어갔습니다. 지금은 이 변경들이 전체 900여 개 SDK 테스트와 WPF 전체 스모크에서 함께 통과하는지 확인하는 단계입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Group Disable 전체 검증을 마무리하는 중입니다. 독립 리뷰에서 PowerOff가 먼저 정상 완료된 뒤 늦게 도착한 Disable 실패가 복구 상태를 다시 오염시키는 경쟁 조건을 확인했습니다. 이 건은 즉시 수정하고 재현 테스트까지 추가하겠습니다.
>
> 추가로 SDK와 WPF 사이의 교착도 확인했습니다. Disable 확인 중 PowerOff가 개입하면 기존 Disable continuation이 SDK에 영구 pending으로 남아 이후 Disable·Close·재연결까지 막을 수 있습니다. PowerOff 완료 시 기존 continuation을 명시적으로 폐기하는 계약을 SDK에 추가하고, WPF가 안정된 PowerOff 증거 뒤 그 계약을 호출하도록 함께 고치겠습니다.
>
> 현재 수정은 세 축으로 진행 중입니다: SDK의 stable PowerOff 증거 기반 Disable continuation 폐기, WPF의 늦은 결과 오염 방지, Unlock 미확정 중 Move 완전 차단입니다. 단위 테스트만 통과시키고 끝내지 않고, ACK 직후 프로세스를 강제 종료한 뒤 새 프로세스에서 `0x2048` 재전송 없이 `0x2045` 3회로 복구되는 시험까지 넣고 있습니다.
>
> SDK 쪽은 이제 Release `941/941 PASS`까지 올라왔습니다. 현재 WPF에서는 PowerOff 완료와 Disable 실패 처리 순서가 뒤바뀌는 경우를 재현해 continuation 폐기 재시도까지 검증 중이며, 그 다음 실제 자식 프로세스 강제 종료/재시작 시험을 붙이는 단계입니다. PLC나 LASAL 동작을 완료로 간주한 것은 아직 아닙니다.
>
> LASAL 실제 `0x2048` handler를 다시 대조한 결과, 실패 ACK를 “명백한 무효과”로 취급하면 안 됩니다. `UnlockProfile()`이 성공했어도 같은 주기의 `LockState`가 아직 0이 아니면 실패 응답이 나올 수 있으므로, NACK 뒤 잠김 상태를 복원해 Move를 허용하는 것은 위험합니다. 이 분기는 즉시 철회하고 오직 wire 전 `NotAttempted`만 복원 대상으로 제한합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+7 -12)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Group Disable 마무리부터 이어서 진행하겠습니다. 지금은 LASAL의 `0x2048` 계약 검증을 “명령 접수 후 상태 폴링” 방식으로 맞추고, 병행 중인 WPF 프로세스 강제 종료 복구 테스트와 독립 리뷰 결과를 합친 뒤 전체 회귀 검증까지 돌리겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+17 -5)
>
> </details>
>
> LASAL 정적 계약은 방금 통과했습니다. `0x2048`은 이제 `UnlockProfile()`의 native 접수 결과만 ACK로 반환하고, 실제 해제 완료는 `0x2045`의 안정 상태 3회로 증명하도록 소스와 검증기가 일치합니다. WPF 쪽 강제 종료/재시작 시험도 우선 11/11 통과했으며, 전체 스모크와 PowerOff 경합의 마지막 실패 경로를 재확인 중입니다.
>
> SDK 전체 회귀도 `941/941`로 통과했습니다. 독립 리뷰에서 잡힌 마지막 UI 오표기—Unlock 접수 후 검증 대기 상태를 이미 “unlocked”로 표시하던 문제—도 수정되어, 이제 “Disabled proof pending / status-only resume, `0x2048` 재전송 없음”으로 표시됩니다. 현재 이 수정까지 포함한 WPF 전체 회귀를 실행 중입니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+16 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+17 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+11 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+11 -5)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+14 -4)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+1 -1)
>
> </details>
>
> Group Disable 단계는 현재 기준으로 마감 가능합니다: SDK `941/941`, WPF `175/175`, LASAL SourceOnly PASS이며, 프로세스 Kill 후 `0x2048=0`, `0x2045=3`, 저널 재획득/해결까지 확인했습니다. 문서도 ACK=접수, 완료=`0x2045` 안정 증명으로 바로잡았습니다. 이제 병행해서 Axis Stop/Reset도 같은 수준의 재시작 내구성으로 올리고 있습니다.
>
> Axis 단계에서 추가로 필요한 계약도 확인했습니다. Reset이 미완료인 상태에서 Stop을 보내는 안전 takeover와, 더 최신의 안정 PowerOff가 pending Stop을 종료시키는 처리는 WPF가 내부 상태를 강제로 지우면 안 되므로 SDK의 원자 API로 구현 중입니다. 반대 방향인 Stop→Reset은 wire 전 차단하고, 실제 재시작 시험은 Stop/Reset 각각 원 명령 0회·상태 3회가 되도록 고정합니다.
>
> Axis SDK의 기본 골격은 들어갔습니다. Stop/Reset accepted observer latch, 명시적 `SupersededBySafetyStop`·`SupersededByStablePowerOff` 상태, cross-session status-only 결과/예외 모델까지 추가됐고, 현재 원자 publication과 takeover/retire를 연결하고 있습니다. WPF 쪽은 공용 AxisCommand 저널을 Stop/Reset 4-state, exact identity, bounded SHA-256·single-writer·atomic replace 방식으로 작성 중입니다.
>
> 독립 리뷰에서 두 가지 재전송 위험을 잡아 바로 반영했습니다. pending Stop/Reset 상태에서 legacy raw Stop/Reset 호출이 같은 명령을 다시 보내던 경로는 SDK wire 전 guard로 막았고, Reset→Stop takeover가 확실한 pre-wire 실패로 끝날 때 durable Reset 기록이 사라지던 문제는 exact snapshot 원자 복원으로 보완했습니다. 전송 가능성이 조금이라도 있거나 새 기록이 생겼으면 복원하지 않고 fail-closed로 남깁니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 계속 진행하겠습니다. 현재는 Axis Stop/Reset의 비정상 종료 복구를 마무리하는 단계입니다. SDK 쪽 재전송 차단과 Reset→Stop 전환 경쟁 조건, WPF 쪽 복구 저널 롤백·프로세스 재시작 경계를 먼저 닫고, 두 테스트 묶음을 전체 실행하겠습니다.
>
> 독립 리뷰에서 하나 더 잡았습니다. 재시작 후 상태만 읽는 Stop/Reset 확인 중에 같은 프로세스의 다른 축 명령이 끼면, 예전 3개 샘플로 복구 저널을 잘못 해제할 수 있었습니다. 원인 귀속을 새로 주장하지는 않되, 확인 시작 시점의 축 mutation 세대를 고정하고 중간·최종 publication에서 바뀌면 typed inconclusive로 실패하도록 보강 중입니다. 외부 클라이언트 명령까지 감지할 수 없다는 경계도 문서에 명시하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 진행 중인 Axis Stop/Reset 복구 구현과 테스트를 먼저 마무리한 뒤, 외부 테스트에서 검증된 비정상 종료 소켓 takeover 수정이 개발 프로젝트에 실제로 반영됐는지 소스 단위로 다시 대조하겠습니다. LASAL IDE는 건드리지 않고 추적 소스만 수정·정적 검증하겠습니다.
>
> 현재 확인된 핵심은 두 가지입니다. 개발 트리에는 이미 `TCPIPServer` 파생 클래스, 동일 IP 비교, 기존 소켓 shutdown, `MaxConnections=2` 계약이 들어와 있습니다. 다만 사용자가 지정한 테스트 문서 두 개는 현재 전달된 경로에 존재하지 않아, 실제 테스트 폴더 위치와 WTR 히스토리 기록을 찾아 개발본과 바이트·상태 전이 수준으로 대조하고 있습니다.
>
> 외부 시험본과 마스터를 직접 비교했습니다. `Comm_Network.lcn`과 생성 table source는 SHA-256까지 동일하고, 두 핵심 ST 파일 차이는 주석/마지막 빈 줄뿐입니다. 즉 검증된 takeover 로직은 이미 개발본에 들어와 있습니다. 지금은 이 상태를 깨지 않도록 LASAL SourceOnly 계약을 재검증하면서 Axis 복구 구현의 남은 race 테스트를 병행하고 있습니다.
>
> LASAL 정적 계약은 현재 `PASS`입니다. 이 통과는 takeover 소스·네트워크 연결·기존 프로토콜 계약이 서로 맞는다는 뜻이고, LASAL IDE 빌드나 PLC 동작 증거는 아닙니다. Axis 쪽에서는 SDK 전용 92개 회귀가 먼저 통과했고, 지금 예외 경계와 WPF 재시작 복구를 보강 중입니다.
>
> 중요한 경쟁 조건이 확인됐습니다. Reset 상태 확인용 `0x2028` 응답이 이미 지연 중이면, 현재 단일 TCP 요청 직렬화 때문에 긴급 Stop `0x2022`도 그 응답이나 timeout을 기다립니다. 지금 상태로는 “Stop이 Reset 확인을 즉시 선점한다”고 말할 수 없으므로, SDK 전송 계층에서 안전 명령 지연을 막는 계약으로 수정한 뒤 테스트하겠습니다.
>
> 해결 방향은 확정했습니다. 이미 전송된 `0x2028`을 취소한 뒤 같은 TCP 스트림을 재사용하면 응답 순서가 깨질 수 있으므로, 기존 transport를 즉시 폐기하고 같은 endpoint로 새 RPC 세션을 만든 다음 BootId/MapRevision/축 참조를 다시 확인한 뒤 Stop을 한 번만 보냅니다. 재연결·신원 확인 전 실패는 Stop 미전송으로 처리하고, 전송 이후 불명확한 실패는 자동 재전송하지 않습니다.
>
> 전송 계층 수정이 들어왔습니다. 새 API는 기존 TCP client를 동기적으로 detach/close하고 세션을 `Faulted`로 전환하며, 이 작업 자체가 Stop을 보냈다고 주장하지 않습니다. 이제 WPF가 같은 연결 객체를 새 세션으로 초기화하고 동일 축 신원을 다시 확인하는 연결부와, 지연된 이전 응답이 새 세션을 오염시키지 않는 회귀를 붙이고 있습니다.
>
> 추가로 NACK 경계를 바로잡고 있습니다. Stop 요청이 PLC에서 명시적으로 거절된 경우에는 축 상태가 바뀌지 않았으므로, 이전 Reset 복구를 그대로 이어갈 수 있어야 합니다. 반대로 ACK 유실처럼 전송 결과가 불명확하면 Stop 기록을 남기고 재전송을 막습니다. 연결 상태 이벤트가 이 결정을 먼저 덮어쓰지 않도록 재연결 구간도 하나의 원자적 복구 절차로 묶고 있습니다.
>
> WPF 정적 리뷰에서 새 세션 전환 후 늦게 도착한 이전 연결 이벤트가 새 Stop continuation을 지울 수 있는 문제가 추가로 확인됐습니다. 같은 연결 객체를 재사용하지 않고, 기존 transport를 먼저 폐기한 뒤 새 `LMCConnection` 객체로 교체하는 구조로 단순화하는 쪽을 검토 중입니다. 이렇게 하면 이전 세션 이벤트는 객체 identity로 바로 격리됩니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 외부 소켓 수정 반영은 확인된 상태이고, 지금은 그 위에 진행 중인 Axis Stop/Reset 복구 경로를 마무리하고 있습니다. 방금 독립 리뷰에서 재시작 시 Motion 저널과 Axis Stop 저널이 함께 남는 경우, 그리고 ACK 후 저널 저장 실패 시 SDK의 숨은 continuation이 남는 두 가지 차단 문제가 확인됐습니다. 구현 담당에게 즉시 반영시키고 전체 회귀까지 확인하겠습니다.
>
> 현재 SDK 쪽은 안전 중단과 NACK 복구까지 구현되어 Stop/Reset 집중 테스트 96/96, 관련 Power/raw 7/7을 통과했고 전체 Release 회귀를 실행 중입니다. WPF는 기존 연결을 폐기하고 새 연결 객체에서 신원 확인 후 Stop을 보내는 구조로 바꿨으며, 재시작·저널 저장 실패 회귀를 추가하고 있습니다. LASAL 소켓 네 파일은 외부에서 시험한 현재 테스트 소스와 SHA-256이 모두 동일합니다.
>
> SDK 전체 회귀는 집중 테스트와 달리 중간에서 장시간 정지해, 기존 테스트의 무한 대기 여부를 추적하고 있습니다. 이 결과를 PASS로 취급하지 않고 막힌 테스트를 특정해 시간 제한이 있는 회귀로 고친 뒤 다시 전체 실행하겠습니다. WPF 쪽은 Reset/Stop 모두 저널 저장 실패 후 같은 세션에서는 명령 재전송 없이 status-only로 복구하도록 수정됐습니다.
>
> WPF 신규 Axis 복구 통합 테스트 11개를 등록했고, 기대값 오류 1건을 제외한 실제 구현 경로 10개가 먼저 통과했습니다. 포함 범위는 Stop/Reset 명령 1회+상태 3회, 재시작 시 명령 0회, held Reset 연결 강제 중단 후 새 연결 Stop, NACK 시 기존 Reset 복원, Motion 저널과 Stop 저널의 해제 순서입니다. 지금은 pre-wire 실패, post-write 불확실성, 실제 child-process 강제 종료 회귀를 추가하고 있습니다.
>
> 안전 중단 API에서 두 경합도 추가로 닫았습니다. 일반 Close가 응답 대기 중이어도 안전 중단은 lifecycle lock 뒤에 막히지 않으며, Reset을 수락한 세션 번호와 정확히 일치할 때만 해당 transport를 끊습니다. 다른 세션이면 연결을 건드리지 않고 Stop 0회로 기존 Reset을 복원합니다. 실제 프로세스 강제 종료 후 Stop/Reset 명령 재전송 0회·상태 조회 3회·저널 잠금 재획득 테스트도 추가됐습니다.
>
> SDK 최종 Release 전체 회귀는 `974/974 PASS`로 닫혔습니다. WPF는 실제 프로세스 종료 Stop/Reset 2건까지 통과했고, Motion+Stop을 두 번 연속 강제 종료하는 마지막 경계와 세션 불일치 회귀를 실행 중입니다. 그 사이 확정된 계약을 Stop/Reset 설계 문서와 진행 현황에 맞추겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./docs/architecture/AXIS_RESET_STABLE_ERROR_CLEARANCE_2026-07-29.md` (+14 -4)
> - Wrote `./docs/architecture/AXIS_STOP_STABLE_STANDSTILL_2026-07-29.md` (+32 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+17 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+10 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+16 -2)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -5)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md` (+7 -0)
> - Wrote `./docs/architecture/ELMO_TCP_SAME_PEER_TAKEOVER_TEST_2026-07-29.md` (+4 -4)
>
> </details>
>
> 마지막 독립 리뷰에서 실제 기능 결함 1건이 더 확인됐습니다. Reset이 아주 좁은 경합 구간에서 먼저 완료되면 기존 WPF가 사용자의 Stop 요청을 0회 송신한 채 처리 완료로 끝낼 수 있었습니다. Reset 완료는 정지 완료가 아니므로 잘못입니다. 이 경우에도 같은 클릭에서 Stop 1회·상태 3회·최종 D0 확인을 반드시 수행하도록 수정하고, Reset 완료 시점이 abort 전/재연결 후인 두 경합을 고정 테스트로 추가하고 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/AXIS_STOP_STABLE_STANDSTILL_2026-07-29.md` (+2 -0)
>
> </details>
>
> Reset 완료 경합 수정이 들어갔고 두 타이밍 회귀도 등록됐습니다. 이제 abort 전에 Reset이 끝난 경우는 기존 연결에서, 재연결 뒤 끝난 경우는 새 연결에서 동일한 durable Stop 요청을 그대로 이어 `0x2022`를 정확히 한 번 보냅니다. ACK 직후 durable accepted 기록 전에 프로세스가 죽는 경계도 자동 재전송 0회로 고정한 뒤 전체 WPF 회귀를 돌리겠습니다.
>
> LASAL 쪽은 IDE를 열지 않고 다시 정적 계약만 확인했고 `Phase5TransportClean / StaticTopologyOnly`가 PASS했습니다. 이는 외부 시험 소켓 코드가 마스터 source/network에 들어왔다는 정적 증거이며, 마스터 LASAL Save/Rebuild/Link·다운로드·PLC 재시험을 대신하지는 않습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./docs/architecture/AXIS_STOP_STABLE_STANDSTILL_2026-07-29.md` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/AXIS_RESET_STABLE_ERROR_CLEARANCE_2026-07-29.md` (+2 -1)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 외부에서 검증한 동일 피어 소켓 인계 코드는 이미 마스터 소스에 반영된 상태입니다. 지금은 남아 있는 WPF 복구 경계 테스트와 문서의 시험 수치를 맞춘 뒤, 작업 트리·정적 계약을 다시 확인하겠습니다. LASAL IDE 제어, 테스트 폴더 재복사, 커밋은 하지 않습니다.
>
> 외부 시험 폴더와 마스터의 소켓 관련 4개 파일은 이전 비교에서 바이트 단위로 같았습니다. 최종 전달 전 같은 비교를 한 번 더 하고, 새 강제종료 테스트는 저널 GUID뿐 아니라 endpoint·축 식별자·Boot/Map 정보까지 직접 보존 검증하도록 강화하고 있습니다.
>
> 현재 확인된 구현 경계는 분명합니다. 비정상 종료 뒤 같은 IP 클라이언트가 들어오면 기존 소켓을 종료 상태로 넘기고 새 소켓만 RPC owner가 되며, 이전 소켓의 늦은 callback은 새 세션을 끊지 못하게 격리됩니다. PC 쪽도 Stop/Reset ACK 직후 종료 구간을 실제 자식 프로세스 강제종료로 검증 중이며, ACK 자체를 완료로 간주하지 않습니다.
>
> 현재 전체 WPF Release 회귀를 다시 돌리고 있습니다. 이 시험은 fake RPC/자식 프로세스 기반 PC 증거이고, 마스터 LASAL의 Save·Generate·Rebuild·Link나 PLC 다운로드·실기 재접속 시험을 대신하지 않습니다. 외부 시험 소켓 파일 4개는 방금 재비교에서도 모두 SHA-256 동일했습니다.
>
> 첫 전체 회귀 결과는 아직 합격이 아닙니다: `205개 중 192 PASS, 13 FAIL`입니다. 새 Stop/Reset 저널·강제종료 시험 17개는 모두 통과했지만, 기존 Reset/Power/Motion 시험의 예상 wire가 새 최종 D0 재확인·재연결 계약과 맞지 않는 항목이 남았습니다. 단순히 기대값을 낮추지 않고 실제 회귀인지 오래된 시험 기대값인지 분류해 수정한 뒤 전체를 다시 돌리겠습니다.
>
> 문서에는 최신 Debug 수치도 따로 적혀 있어, WPF 수정과 충돌하지 않는 SDK Debug 출력을 별도로 재빌드·전량 실행하겠습니다. Release `974/974`와 같은 계약인지 확인하고, 실제 PLC 시험과는 분리해 기록합니다.
>
> SDK Debug도 최신 소스로 `974/974 PASS`했습니다. 따라서 SDK는 Debug와 Release 모두 `974/974`로 기록할 수 있습니다. WPF 13건은 기존 시험의 엄격한 wire 기대를 새 계약에 맞추는 과정이며, 수정 후 `205/205`가 실제로 나오기 전에는 합격으로 보고하지 않겠습니다.
>
> 13건을 다시 분류한 결과는 Reset 7, Motion 복구 5, Axis Power On 1입니다. 일부는 최종 D0 확인이 추가돼 fake RPC 단계가 부족한 시험이고, 일부는 “확정된 간섭 뒤 사용자가 명시적으로 재시도”하는 UI 경로의 실제 회귀였습니다. 후자는 생산 코드를 복구하되 accepted 명령 자동 재전송 금지는 그대로 유지하고 있습니다. 현재 빌드는 오류 없이 통과한 상태입니다.
>
> 현재 Release 빌드는 계속 오류 0입니다. Motion 복구 30개를 묶어 재실행 중이며, Motion+Stop의 최종 identity는 Axis 저널과 Motion 저널 각각에 대해 D0을 따로 확인하도록 시험을 더 엄격하게 맞췄습니다. accepted Stop 모니터 중에는 Stop 재전송 버튼을 비활성으로 유지하고, 더 높은 우선순위의 Power Off만 계속 허용합니다.
>
> Motion 계열은 현재 29/30까지 정리됐습니다. 남은 1건은 Power Off 모니터 중 확정 간섭 뒤 replacement가 timeout 나는 경로라 wire/log 순서를 확인 중입니다. 그 다음 Reset 7건을 묶어 재검증하고 전체 205개를 다시 실행합니다.
>
> 남은 Motion 1건의 원인은 확인됐습니다. timeout 문제가 아니라 확정 간섭 후 status-only Resume 전에 생산 코드가 요구하는 exact-identity D0 요청을 시험 서버가 준비하지 않아 EOF가 난 것이었습니다. 해당 D0 단계를 추가하고 capability read 증가량도 정확히 검증하도록 고쳤으며, 이제 Reset 묶음을 실행한 뒤 Motion 30개를 다시 확인합니다.
>
> 독립 리뷰에서 실제 P1 하나를 찾았습니다. Reset이 막 완료된 뒤 Stop이 valid NACK를 받는 좁은 경로에서, 마지막 D0 identity 확인 없이 Stop tombstone을 지워 BootId/MapRevision 변경을 놓칠 수 있었습니다. 이 경로는 즉시 수정해 exact endpoint·축·BootId·MapRevision이 다시 일치할 때만 resolve하고, 확인 실패나 mismatch면 `RecoveryRequired`를 유지하도록 만들겠습니다. 일치/불일치 회귀도 추가한 뒤 전량 재시험합니다.
>
> P1 수정 방향은 확정했습니다. completed Reset 선행 상태에서 Stop NACK가 나면 현재 축으로 D0 exact identity를 비동기 재검증하고, 일치할 때만 Stop tombstone을 해제합니다. D0 실패나 mismatch면 Stop은 `RecoveryRequired`로 남고 Reset은 다시 활성화하지 않습니다. 현재 호출 경로를 비동기로 바꾸고 match/mismatch 시험을 추가 중입니다.
>
> P1 수정과 회귀가 통과했습니다. `CompletedReset` 필터 `4/4 PASS`: NACK 뒤 D0 일치 시만 resolve하고, MapRevision mismatch에서는 Reset 1회·Stop 1회·status 3회·D0 3회를 확인한 뒤 Stop identity/predecessor를 유지한 `RecoveryRequired`로 남기며 Close/Reset UI를 차단합니다. 이제 남은 Reset/Motion 묶음과 전체 Release를 순서대로 재실행합니다.
>
> 남은 Reset 5건은 모두 원인이 분류됐습니다. 명시 재시도 2건은 새 pre-dispatch D0 단계 누락, Power Off 선점 1건은 durable Reset이 남아 있는데도 Close 가능을 기대한 오래된 시험, Stop 선점 2건은 새 pinned abort→새 세션→축 재조회 계약 전의 fixture입니다. 안전 계약을 낮추지 않고 이 순서를 실제 wire 단계로 바꾸고 있습니다.
>
> Reset 묶음은 현재 `4/7 PASS`까지 올라왔습니다. 명령 1회, status 실패 후 Resume, 확정 간섭 뒤 명시 재시도, Power Off 선점 경로가 통과했습니다. 남은 3건 중 1건은 wire/state는 맞고 표시 문자열만 오래된 기대값이며, Stop takeover 2건은 새 세션 단계 중 누락 요청을 wire/state 진단으로 좁히고 있습니다.
>
> Reset 전용 `7/7 PASS`했습니다. 두 takeover 시험은 실제 2개 세션을 쓰고, 이전 세션 Stop 0회·새 세션 Stop 1회·status 3회·최종 D0을 유지합니다. 원인은 새 세션 RequestId가 1부터 다시 시작하는데 fixture가 이전 세션 번호를 쓴 것이었고, 생산 코드 결함은 아니었습니다. 이제 Motion 복구 30개를 다시 실행합니다.
>
> Motion 복구 `30/30 PASS`, AxisCommand integration `18/18`, 저널 `9/9`, 실제 자식 프로세스 강제종료 `4/4`가 모두 통과했습니다. 강제종료 묶음에는 Stop/Reset accepted 재시작, ACK 직후 durable mark 전 종료, Motion+Stop 순차 해소가 포함됩니다. Axis Power 묶음 뒤 전체 Release를 실행합니다.
>
> Axis Power도 `28/28 PASS`했습니다. 확정 간섭 뒤 명시적 replacement Power Off는 송신 전 D0과 완료 후 D0을 모두 요구하고, 시험에서 총 D0 7회를 엄격히 확인합니다. 이제 최신 소스로 전체 `206개` Release smoke를 돌리고 있습니다. 예상치가 아니라 실제 `206/206` 결과가 나온 뒤 문서와 최종 상태를 확정하겠습니다.
>
> 최신 전체 WPF Release 결과는 `206/206 PASS`이며 빌드 warning/error도 `0/0`입니다. 이제 오래된 “현재 시험 수치”만 `SDK Debug/Release 974/974`, `WPF Release 206/206`으로 맞추고, 역사적 체크포인트 수치는 함부로 덮지 않겠습니다. 그 다음 전체 diff/스테이지 상태와 LASAL SourceOnly를 최종 점검합니다.
>
> <details><summary>Edited 10 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+8 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -3)
> - Wrote `./docs/architecture/AXIS_RESET_STABLE_ERROR_CLEARANCE_2026-07-29.md` (+7 -4)
> - Wrote `./docs/architecture/AXIS_STOP_STABLE_STANDSTILL_2026-07-29.md` (+9 -4)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+6 -5)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+32 -40)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+1 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+12 -13)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
