> 계속 진행하겠습니다. 우선 현재 소스와 남은 구현 계획을 다시 대조해서, LASAL IDE 없이 외부 편집·PC 자동검증까지 끝낼 수 있는 다음 우선순위 항목을 선정한 뒤 바로 구현하겠습니다.
>
> 현재 계획을 대조한 결과, 동적 CREVIS `0x7E13/0x7E22/0x7E23`은 사용자가 LASAL IDE에서 T2 구조를 먼저 생성해야 해서 지금 바로 진행할 수 없습니다. 그 사이 코드만으로 완결 가능한 다음 안전 경계인 Group Reset의 “ACK 1회 + `0x2045` 상태 확인 + 재전송 금지” 계약을 우선 후보로 검토 중입니다.
>
> 다음 구현은 Group Power On/Off accepted-once 복구로 확정했습니다. 현재는 ACK가 돌아온 뒤에만 WPF의 휘발성 플래그가 생기므로, 송신 후 응답 유실이나 강제 종료 시 `0x204A/0x204B`가 재전송될 수 있는 실제 공백이 있습니다. 먼저 SDK의 Begin/Resume/compound·generation 귀속을 구현하고, 이어서 WPF durable restart 복구까지 연결하겠습니다.
>
> SDK 구현은 기존 Group Enable/Stop과 같은 coordinator를 공유하도록 진행 중입니다. 중요한 경계는 새 Power 명령이 wire에 닿은 순간 generation을 기록하고, ACK 뒤에는 exact continuation만 남기며, 이후 같은 group mutation이 끼면 상태를 원 명령의 완료로 잘못 귀속하지 않는 것입니다. 기존 read-only `WaitForPowerStateAsync`는 호환을 위해 유지합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 같은 피어 소켓 takeover 반영분은 그대로 유지한 채, 지금은 Group Power On/Off를 “ACK 이후 상태 확인까지 재전송 없이 복구 가능”하도록 SDK와 WPF에 확장하고 있습니다. SDK 구현 결과를 먼저 검토·검증한 뒤 WPF 영속 복구까지 이어가겠습니다.
>
> 현재 확인된 핵심 결함은 Group Power 상태가 메모리 플래그에만 있어 프로그램 재시작 시 ACK/불확실성 증거가 사라지는 점입니다. 이를 단일 영속 저널로 보존하고, 재시작 후에는 `0x2045` 상태 조회만 수행하며 `0x204A/0x204B`를 자동 재전송하지 않도록 구현 범위를 확정했습니다. Power Off 재전송도 실제 간섭 또는 여전히 Power On 상태가 확인된 경우에만 열도록 제한합니다.
>
> SDK 쪽에는 우선 새 continuation/evidence 모델이 들어갔고, 지금 공용 group coordinator와 실제 Begin/Resume 경로를 연결하는 중입니다. WPF 쪽은 저널 파일과 UI 연동을 분리해 병렬 구현하고 있으며, 기존 Group Enable/Profile Lock/Motion 복구 경로를 침범하지 않는지 함께 확인하고 있습니다.
>
> 안전 경계 두 가지를 추가로 정리했습니다. 미확정 Power On을 Power Off로 넘길 때는 저널을 먼저 한 번의 원자 교체로 바꿔 중간 크래시 공백을 없애고, 이미 Power Off 복구 중 재전송하는 경우에는 기존 false 목표 기록을 유지한 채 ACK가 실제 수락된 시점에만 상태를 갱신합니다. 거절·송신 전 실패에서는 기존 복구 증거가 사라지지 않습니다.
>
> WPF 1단계가 저장됐습니다. 이제 재시작 시 저널을 열고, 미송신/불확실 기록을 안전 복구 상태로 올리며, endpoint·group name/ref·BootId·MapRevision이 정확히 일치할 때만 복구하도록 하는 기반이 들어갔습니다. 다음 단계에서 실제 Power On/Off 버튼을 SDK continuation과 연결하고 UI 재전송 차단을 적용합니다.
>
> 저널 자체 검증은 10/10 통과했고 SDK도 현재 Debug 컴파일을 통과합니다. 전체 WPF smoke에서 새 기능 로직이 아닌 테스트 정리 경로가 새 `GroupPowerRecovery\journal.lock`을 닫지 않는 2건을 발견했습니다. 생산 코드의 종료 체인과 테스트의 강제 정리 경로를 함께 보완해 이 실패도 없애는 중입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 그룹 Power On/Off의 “명령 1회 수락 후 상태 조회만 재개” 구현을 이어서 마무리하겠습니다. 먼저 진행 중이던 SDK 전체 회귀 결과를 회수하고, 병렬 작업 중인 WPF 복구 경로를 합친 뒤 경쟁 조건과 재시작 내구성까지 다시 검증하겠습니다.
>
> SDK의 기존 889개 회귀는 통과했지만, 독립 검토에서 중요한 교착 가능성을 발견했습니다. 수락 콜백 실행 중 SDK가 상태 조회/변경 잠금을 잡고 있어 콜백이 상태 조회나 Power Off로 재진입하면 멈출 수 있는 구조입니다. 지금 잠금 해제 순서를 축 API와 동일하게 고치고, 실제 재진입을 제한 시간 내 완료시키는 회귀 테스트를 추가하고 있습니다.
>
> SDK 교착 수정이 반영됐고, 수락 콜백에서 상태 조회와 안전 Power Off를 재진입시키는 시험까지 추가했습니다. 현재 SDK는 Debug/Release 각각 896/896 통과했습니다. 이제 WPF의 영속 저널·재시작 복구·오래된 작업이 새 기록을 덮는 경쟁 조건을 최종 점검하고 전체 GUI 회귀를 확인하겠습니다.
>
> WPF 기본 회귀는 현재 135/135까지 통과했습니다. 다만 이 수치는 새 영속 복구 시나리오를 넣기 전 결과이므로 완료로 보지 않고 있습니다. 지금 별도 통합 시험에 재시작 후 status-only 확인, Power On→Power Off 원자 전환, 오래된 실패가 새 기록/재전송 권한을 훼손하지 않는 경우를 추가하고 있습니다.
>
> 독립 WPF 검토에서 두 가지 실제 결함을 추가로 잡았습니다. 첫째, 이전 status-only 검증이 늦게 끝나면 새 Power Off 기록을 잘못 승격할 수 있었습니다. 둘째, 저널 쓰기 실패로 `Armed`가 남은 Power Off에서 실제 `PowerOn=true`를 확인해도 재시도 버튼은 켜지지만 전송 직전 내부 검사에서 막히는 모순이 있었습니다. 두 경로 모두 기록 identity와 상태 전이를 고치고 결정론적 경쟁 시험으로 고정 중입니다.
>
> 생산 코드 수정본은 현재 WPF Release 빌드까지 통과했습니다. 이제 테스트 구현 범위를 더 키우지 않고 핵심 경쟁 조건부터 디스크에 고정하도록 정리했습니다. 먼저 5개 핵심 시나리오를 실행 가능한 상태로 만든 다음, 남는 경계만 추가하겠습니다.
>
> 새 WPF 영속 복구 회귀 5개가 추가됐고 필터 실행은 5/5 통과했습니다. 여기에는 실제 저널 재개, 구 작업의 늦은 실패, `Armed` Power Off 승격, 직접 호출 zero-wire 방어, 전송 전 실패 시 재시도 권한 보존이 포함됩니다. 지금 전체 WPF 회귀와 독립 최종 검토를 병행하고 있습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 외부 테스트에서 확인된 동일 클라이언트 IP의 비정상 종료 후 재접속 처리까지 개발 소스에 반영된 상태입니다. 지금은 병합된 LASAL·SDK·WPF 변경을 다시 교차 검증하고, LASAL IDE에서 사용자가 수행해야 할 빌드 경계만 분리해 확정하겠습니다.
>
> 독립 검토에서도 잔여 P0/P1/P2 결함은 없었고, WPF 전체 회귀는 Release 140/140으로 확인됐습니다. 이어서 SDK Release 901개와 LASAL SourceOnly 정적 계약을 루트 환경에서 한 번 더 재실행하고 있습니다.
>
> 이번 반영분 자체는 정적·PC 회귀 기준으로 닫혔습니다. 개발은 멈추지 않고, 남은 목록에서 LASAL IDE 없이 안전하게 진행 가능한 다음 항목과 ‘ACK 직후 프로세스 강제 종료→재시작’ 내구성 자동시험 가능성을 병행 점검하겠습니다.
>
> 비교 결과, 테스트본과 마스터의 `TCPIPServer.st`는 공백을 제외하면 동일하고 `TCPMotionInterface.st`의 실행 로직도 동일합니다. `Comm_Network`의 `TCPIPServer / MaxConnections=2 / Config=0` 구성도 일치합니다. 현재 차이는 마스터의 `Classes.lcb` 등 IDE 생성 메타데이터가 아직 이전 `_TCPIPServer_RT`를 가리키는 점뿐이라, 이 부분은 외부 편집으로 덮지 않고 LASAL Save/Rebuild/Link 단계로 남겨뒀습니다.
>
> 자동시험 측면에서도 한 단계 더 보강 중입니다. 기존 Axis Power에 있던 실제 child-process 강제 종료 패턴을 Group Power On/Off에 그대로 적용할 수 있어, ACK 후 프로세스를 죽인 뒤 journal lock 재획득과 재시작 세션의 `0x2045` 상태조회만 발생하고 `0x204A/0x204B` 재전송은 0회인지 확인하는 시험을 추가하고 있습니다. 운영 코드는 건드리지 않는 범위입니다.
>
> 추가 시험은 기존 프로세스 하네스를 재사용해 On/Off 두 시나리오를 넣는 중이며, 모든 child/server 대기는 기존 15초 제한을 유지합니다. 현재까지 운영 소스 변경은 없고 SmokeTests 두 파일만 건드리는 범위입니다.
>
> Group Power 내구성 시험까지 추가됐습니다. 실제 child를 ACK 직후 강제 종료한 뒤 새 프로세스에서 On/Off 모두 명령 재전송 0회, 상태조회 3회, journal lock 재획득과 동일 identity의 `Resolved`까지 확인했고 전체 WPF Release가 141/141 PASS했습니다. 이제 문서의 140개 표기와 검증 범위를 141개 기준으로 맞추겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+7 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
>
> </details>
>
> 다음 구현 항목은 LASAL IDE 없이 진행 가능한 Axis Power Off의 durable 재시작 복구로 정했습니다. 기존 Power On 저널을 방향 포함 단일 v2 저널로 확장해 On→Off 인계를 원자화하고, SDK에 Power Off accepted observer를 추가한 뒤 WPF에서 재시작 시 `0x2028` 상태조회만 허용하는 방식입니다. SDK 계약, 저널 마이그레이션, WPF 연결 지점을 병행 분리해 진행하겠습니다.
>
> Axis Power Off 확장은 기존 wire 명령을 바꾸지 않습니다. 핵심은 ACK 게시 직후 죽는 창을 SDK observer로 닫고, 기존 Power On 저널 파일을 v2로 올려 방향을 함께 저장하는 것입니다. 별도 Off 저널은 만들지 않아 On→Off 전환이 두 파일에 걸쳐 비원자적으로 깨지는 구조를 피하고 있습니다.
>
> SDK 쪽은 accepted observer와 경쟁 조건 5개를 추가해 Debug 906/906까지 통과했습니다. 현재 Release 전체 회귀를 돌리는 동안 WPF는 unified journal을 이용해 fresh Off, On→Off 원자 takeover, 재시작 status-only, 명시적 Off Again 정책을 통합 중입니다.
>
> SDK Release는 루트에서도 906/906으로 재확인했습니다. WPF 통합은 현재 Power On 기존 동작을 유지하면서 Off 방향 상태를 추가하는 단계이며, 특히 이전 worker의 늦은 성공·실패가 새 Off 레코드나 `Resolved` tombstone을 건드리지 않도록 record identity를 작업 시작 시 고정하는 구조로 맞추고 있습니다.
>
> WPF는 현재 양방향 helper를 교체하는 중이라 잠시 컴파일이 깨지는 중간 상태입니다. 독립 리뷰가 이 상태에서 난 `CS0103`는 저널 결함이 아니라 아직 handler 연결이 끝나지 않은 live-edit 경계로 확인했고, 통합 완료 후 같은 reviewer가 전체 Release를 다시 검증합니다.
>
> 양방향 recovery helper 1차 반영은 끝났고, 지금은 기존 Power On reflection 테스트와 private wrapper 호환을 유지하면서 오래된 volatile `axisPowerOffAcceptedForRecovery` 경로를 제거하고 handler를 새 exact-record 흐름에 연결하는 중입니다. admission enum과 외부 wire 계약은 그대로 유지합니다.
>
> Power On과 Power Off handler 교체가 끝났습니다. 첫 시도에서 일반 `dotnet build`는 이 구형 WPF 프로젝트의 XAML target을 제대로 적용하지 못해 generated symbol 오류가 쏟아졌고, 이는 코드 판정에 쓸 수 없는 빌드 경로입니다. 저장소가 쓰는 VS2019 MSBuild smoke target으로 전환해 cleanup/UI 연결 후 유효 빌드를 확인합니다.
>
> 첫 유효 production gate는 닫혔습니다. VS2019 Release WPF 빌드가 통과했고 기존 Axis Power On recovery 9/9도 유지됐습니다. 동일 세션 continuation Resume에는 불필요한 capability refresh를 하지 않고, 재시작처럼 continuation이 없는 경우에만 fresh BootId/MapRevision을 확인하도록 기존 Group 패턴과 맞췄습니다. 이제 durable Off 전용 회귀와 경고 0개 정리를 진행 중입니다.
>
> 독립 리뷰에서 P1 후보 하나를 잡았습니다. 동일 identity라도 이미 `Resolved`인 tombstone을 오래된 Power Off 실패가 다시 활성 상태처럼 취급해 replacement 권한이나 pending 표시를 되살릴 수 있는 경로입니다. Group 쪽과 동일하게 `IsActive`까지 요구하도록 수정하고, resolve 뒤 늦은 failure가 tombstone을 바꾸지 않는 회귀를 추가하도록 바로 되돌렸습니다.
>
> 새 Off recovery 3개 회귀는 통과했습니다. 전체 회귀에서 기존 fake-RPC fixture 8개가 새 final identity 확인 `0x7E00`을 예상하지 않아 실패한 것을 확인했습니다. 운영 결함이 아니라 두 durable journal을 각각 해제하기 전 최종 BootId/MapRevision을 확인하는 새 계약과 fixture가 어긋난 것이므로, 해당 시퀀스만 최소 보강한 뒤 전체 150개를 다시 돌립니다.
>
> 추가 경쟁 조건도 잡혔습니다. 오래된 Power Off 성공 결과가 `Complete...`에서 stale로 거부된 뒤에도 caller가 replacement 권한을 무조건 지우는 경로였습니다. completion 적용 여부를 반환하거나 권한 정리를 helper 내부에만 두도록 바꾸고, 새 Off recovery의 replacement 권한이 보존되는 회귀를 추가하고 있습니다.
>
> 두 번째 경쟁 조건도 수정했고, 늦은 성공 결과가 더 새 Off `RecoveryRequired + replacement=true` 상태를 건드리지 않는 별도 회귀를 추가했습니다. 현재 durable Off 집중 시험은 4개가 되었고, 기존 motion/Stop fixture도 새 계약대로 “status-only에서 interference 확인 → 사용자 명시 Power Off Again 1회” 순서로 맞춘 뒤 전체 재실행 중입니다.
>
> journal unavailable interlock도 더 정밀하게 다듬고 있습니다. 단순히 기존 diagnostics-journal 장애 플래그에 합치면 read-only D5까지 막혀 계약을 깨기 때문에, 별도 `PowerRecoveryJournalUnavailable` admission으로 분리해 새 mutation과 D5 submit만 차단하고 safety, 일반/추적 read-only, cleanup, connect/close는 허용합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 Axis Power Off 복구 통합의 마지막 정합성 검토 단계입니다. 방금 발견된 “ACK가 없던 재시작 경로도 Accepted로 기록되는” 잘못된 증거 문구를 수정하고, 전체 WPF 회귀와 실제 프로세스 강제 종료·재시작 테스트까지 마무리하겠습니다.
>
> 증거 문구 수정은 소스에 반영됐습니다. 이제 `AcceptedAwaitingProof`였던 경우에만 ACK 수락을 주장하고, `Armed/RecoveryRequired` 재시작은 “상태 조회로 안전 상태를 증명했지만 accepted ACK는 주장하지 않음”으로 분리됩니다. 현재 동시성 회귀 테스트 결과를 기다리는 동안 강제 종료 테스트 구조도 맞춰 보고 있습니다.
>
> 현재 전체 WPF 회귀는 153개 중 151개 통과입니다. 두 실패는 기능 핵심 자체가 아니라 테스트 간 상태 누출/fixture 기대값 문제로 좁혀졌고, 동시에 안전 명령 ACK 경계에서 motion 복구 저널을 갱신하지 않던 실제 결함 1건을 찾아 수정 중입니다. 이 회귀를 154/154로 만든 뒤 Axis Power Off 강제 종료 테스트를 추가하면 최종 목표는 155/155입니다.
>
> 안전 명령 ACK 경계 수정이 들어간 전체 스모크가 현재 실행 중입니다. 이 경로는 “Power Off 명령이 ACK된 뒤 상태 확인 중 프로세스가 끊겨도, 기존 motion 불확실성 저널이 안전 명령 수락을 놓치지 않는가”를 검증합니다. 단순 UI 성공 여부가 아니라 재시작 후 명령 재전송 방지까지 보는 계약입니다.
>
> 핵심 WPF 결함 두 건은 수정됐고 파일도 안정화됐습니다. 독립 검토자는 현재 155개 전체 회귀를 새로 빌드해 검증 중이며, 별도 작업으로 Axis Power Off를 ACK 직후 강제 종료한 뒤 재시작하는 실제 프로세스 테스트를 추가하고 있습니다. 최종 회귀 수는 156개가 될 예정입니다.
>
> LASAL 소스 계약 검증은 다시 통과했습니다. 이는 `.st/.lcp/.lcn` 정적 정합성 통과이고, 마스터 프로젝트의 LASAL IDE Save/Rebuild/Link 및 PLC 런타임 검증을 대신하지는 않습니다. 현재 WPF 독립 회귀와 프로세스 강제 종료 회귀가 병행 중입니다.
>
> 독립 검증 결과, 프로세스 강제 종료 테스트 추가 전 기준으로 WPF Release 전체 `155/155 PASS`, SDK Release `906/906 PASS`이며 남은 P0~P2 리뷰 지적은 없습니다. 지금 추가 중인 마지막 테스트는 Axis Power Off의 실제 child-process Kill 후 저널 잠금 재획득과 재시작 `0x2023` 무재전송을 검증합니다.
>
> 새 강제 종료 회귀는 코드 반영까지 완료됐습니다. 확인 항목은 첫 프로세스의 Power Off `0x2023(false)` 1회, 살아 있는 동안 단일 writer 잠금 유지, 종료 후 `AcceptedAwaitingProof/ExpectedPowerOn=false` 보존, 재시작 세션의 `0x2023` 0회·`0x2028` 3회·동일 identity `Resolved`입니다. 현재 집중 테스트를 실행 중입니다.
>
> 최종 전체 WPF 회귀도 제가 다시 실행해 `156/156 PASS`를 확인했습니다. 이제 README·설계·현재 상태 문서의 낡은 `901/901`, `141/141` 수치를 `906/906`, `156/156`으로 맞추고, Axis Power Off 방향성 저널·강제 종료 복구·ACK 경계 경쟁 조건을 문서 계약에 반영합니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+9 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+14 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+12 -4)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -7)
> - Wrote `./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md` (+32 -10)
>
> </details>
>
> 최종 검증은 모두 끝났습니다. SDK Release `906/906`, WPF Release `156/156`, LASAL SourceOnly 정적 계약은 통과했습니다. 전체 LASAL 계약은 예상대로 마스터 `Classes.lcb`가 아직 `_TCPIPServer_RT` 기준이라 실패하며, 이 부분만 사용자가 LASAL에서 Save/Rebuild/Link해야 해소됩니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md` (+1 -0)
>
> </details>
</details>

반영 완료했습니다.

- 외부 시험에서 검증한 동일 IP stale socket takeover 코드를 개발 프로젝트에 반영했습니다.
  - [TCPIPServer.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPIPServer/TCPIPServer.st)
  - [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st)
  - Comm Network 및 프로젝트 등록 파일까지 외부 시험본과 실행 코드가 동일함을 확인했습니다.
- Axis Power Off SDK/WPF도 계속 개발했습니다.
  - 방향을 기록하는 durable journal v2
  - ACK 직후 첫 상태 조회 전에 저널 저장
  - 재시작 시 `0x2023` 재전송 없이 `0x2028` 상태 조회만 수행
  - stale observer, 연결 끊김, tombstone 경쟁 조건 수정
  - 실제 WPF child-process 강제 종료·재시작 회귀 추가

검증 결과:

- SDK Release: `906/906 PASS`
- WPF Release: `156/156 PASS`
- Axis Power Off 강제 종료 재시작:
  - 첫 세션 `0x2023(false)` 1회
  - 재시작 세션 `0x2023` 0회
  - `0x2028` 3회
  - journal lock 재획득 및 동일 identity `Resolved`
- LASAL SourceOnly 계약: PASS
- `git diff --check`: PASS

이제 마스터 LASAL 프로젝트에서 `Save/Generate → Rebuild All → Link`를 부탁합니다. 현재 전체 정적 계약은 [Classes.lcb](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb)가 아직 `_TCPIPServer_RT` 기준이라 의도대로 실패합니다.

커밋·스테이징·테스트 폴더 복사는 하지 않았습니다. 기존 staged 히스토리 파일의 후행 공백 문제도 그대로 보존했습니다.

<details><summary>18 previous messages</summary>

> 다음 구현 항목을 현재 소스와 설계 문서에서 다시 소팅하겠습니다. LASAL IDE 동기화는 사용자 빌드 단계로 남겨 두고, 그와 독립적으로 진행 가능한 SDK/WPF의 다음 미완료 계약을 찾아 바로 구현하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 외부 테스트에서 확인된 동일 클라이언트 비정상 종료 재접속 처리는 개발 소스에 반영된 상태입니다. 지금은 그 변경과 충돌하지 않는 PC 측 다음 항목으로, Group Enable/Profile Lock의 실제 프로세스 강제 종료 후 복구 테스트·보강 여부를 소스 기준으로 확정하고 있습니다. LASAL IDE 저장/빌드가 필요한 부분은 건드리지 않겠습니다.
>
> 확인 결과 단순 테스트 누락보다 실제 구현 공백이 하나 있습니다. Group Enable은 현재 전송 전에만 journal을 기록해서, `0x2047` ACK 직후 WPF가 강제 종료되면 “ACK 수락됨” 경계를 보존하지 못합니다. Group Power처럼 ACK observer와 `AcceptedAwaitingProof` 상태를 추가해 재시작 후 `0x2047`을 재전송하지 않고 `0x2045` 상태 조회만으로 이어가는 방향이 맞습니다. 이 항목을 우선 구현하겠습니다.
>
> 구현 범위를 세 갈래로 나눴습니다. SDK는 ACK 직후 callback과 cross-session 상태 전용 조회를, WPF는 journal 상태 전환과 정확한 endpoint/group/BootId/MapRevision 복구를, 회귀는 실제 child process `Kill()` 시나리오를 각각 작업 중입니다. 핵심 합격 조건은 재시작 세션에서 `0x2047 = 0회`, `0x2045 = 안정 샘플 3회`입니다.
>
> 현재 SDK와 회귀 소스 편집은 시작됐고, 기존 journal 파일 호환성을 위해 상태 숫자 `1/2/3`은 유지한 채 `AcceptedAwaitingProof=4`로 추가하고 있습니다. 또한 ACK가 확실한 연결 끊김은 Disable-only로 강등하지 않고 status-only 복구로 유지하되, ACK 자체가 불확실한 `ArmedBeforeDispatch`만 기존 안전 복구 경로로 남깁니다.
>
> 중간 소스 검토에서는 기존 journal과의 하위 호환, ACK callback이 첫 상태 조회보다 먼저 실행되는 순서, 재시작용 read-only 결과/evidence 타입까지 들어갔습니다. 안전 예약이 ACK 이후 끼어든 경우에는 Accepted를 의도적으로 `RecoveryRequired`로 승격하고, 단순 프로세스 종료·연결 단절은 Accepted를 유지하도록 두 경로를 분리했습니다. 이제 통합 컴파일에서 기존 경로 회귀를 잡는 단계로 넘어가고 있습니다.
>
> 핵심 회귀는 통과했습니다: journal `7/7`, Group Enable 통합 `12/12`, 실제 child `Kill()` 후 재시작 `1/1`입니다. 전체 159개에서는 관련 없는 기존 Motion Recovery 테스트 하나가 타임아웃했고 단독 재실행은 통과했으므로, 전체 suite를 한 번 더 실행해 최종 수치를 확정하겠습니다.
>
> 두 번째 전체 실행은 `159/159 PASS`로 확인됐습니다. 제가 시작한 독립 재실행도 아직 진행 중이며, 끝까지 받아 일시적 타이밍 실패가 반복되지 않는지 한 번 더 확인하겠습니다.
>
> 전체 suite의 반복 타임아웃 원인도 확인했습니다. 실패한 기존 테스트가 차단된 `Load Axis` 비동기 핸들러의 종료를 기다리지 않고 바로 다음 `Power Off`를 클릭해, 간헐적으로 클릭이 `operationRunning` 때문에 무시되는 테스트 동기화 문제였습니다. 생산 코드 문제는 아니며, 해당 핸들러 완료 barrier를 추가한 뒤 반복 실행으로 고정하고 있습니다.
>
> 반복 타임아웃을 단순히 늘리지 않고 원인까지 수정했습니다. 차단된 Axis reload가 UI 버튼 비활성화에만 의존해 직접 핸들러 호출 시 lookup RPC 1개를 보내던 생산 코드 공백이 드러났고, active Axis Power recovery + 기존 handle이면 RPC 전에 차단하도록 보강했습니다. 해당 테스트 `5/5`, Power Off 묶음 `7/7` 통과했으며 이제 전체 159개를 수정 후 기준으로 재실행합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 재현되던 기존 WPF 비동기 테스트 경합과 실제 Load Axis 진입 차단 누락까지 수정했습니다. 지금 수정 후 전체 WPF 159개 검증의 최종 결과를 회수하고, 독립 리뷰와 정적 검사를 마무리하겠습니다.
>
> 독립 리뷰에서 한 가지 실제 오류 경로를 찾았습니다. Group Enable ACK 직후 저널 저장이 실패하면 SDK 콜백이 작업 스레드에서 실행되어 WPF 로그 UI를 직접 건드릴 수 있습니다. 이 경로를 UI 스레드로 마샬링하고, 저장 실패를 작업 스레드에서 강제하는 회귀 테스트까지 추가한 뒤 최종 전체 검증을 다시 돌리겠습니다.
>
> 현재 기준으로 LASAL 사람 편집 소스 계약은 통과했습니다. 전체 계약이 막히는 이유도 코드가 아니라 `Classes.lcb`가 이전 `_TCPIPServer_RT` 등록을 들고 있기 때문으로 다시 확인됐습니다. 이 부분은 사용자가 마스터 LASAL 프로젝트에서 Save/Rebuild 해야 해소됩니다. WPF 리뷰 수정이 끝나는 대로 전체 160개 수준 검증을 다시 실행하겠습니다.
>
> 리뷰 수정 전 전체 WPF 검증은 재현성 있게 `159/159 PASS`했습니다. 추가 수정은 정상 동작 경로가 아니라 “ACK 수신 후 저널 파일 저장 실패” 예외 경로만 다루고 있으며, 해당 회귀 테스트 작성·실행 중입니다.
>
> SDK 전체 PC 검증도 현재 소스에서 `911/911 PASS`했습니다. 남은 자동 검증은 리뷰에서 추가한 저널 저장 실패 테스트와 그 반영 후 WPF 전체 재실행뿐입니다.
>
> 최종 반영 후 전체 WPF 검증은 `160/160 PASS`했습니다. 이제 문서 수치, 공백 오류, 잔류 테스트 프로세스, 변경 범위를 마지막으로 확인하고 사용자 LASAL 빌드·실기 시험 항목만 분리해 전달하겠습니다.
</details>

반영 완료했습니다.

- 동일 IP의 신규 연결을 감지하면 기존 stale 소켓을 종료하고 신규 소켓으로 owner를 교체하도록 [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:427)와 [TCPIPServer.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPIPServer/TCPIPServer.st:163), Network/LCP를 반영했습니다.
- 테스트 폴더의 5개 관련 파일과 주석·공백을 제외한 실행 구조가 모두 동일함을 확인했습니다.
- 다른 IP의 신규 연결은 기존 owner를 유지하고 거절합니다.
- WPF 강제 종료에도 Group Enable ACK를 영속 기록하여 재시작 후 `0x2047`을 재전송하지 않고 `0x2045` 상태 조회만 수행하도록 SDK/WPF를 보강했습니다.
- ACK 후 저널 저장 실패가 UI cross-thread 예외로 가려지는 문제도 [MainWindow.GroupProfileLockRecovery.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.GroupProfileLockRecovery.cs:652)에서 수정했습니다.

검증 결과:

- SDK Release: `911/911 PASS`
- WPF Release smoke: `160/160 PASS`
- LASAL SourceOnly 정적 계약: PASS
- `git diff --check`: PASS
- LASAL 전체 계약: FAIL — [Classes.lcb](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb)가 아직 이전 `_TCPIPServer_RT` 등록을 보유하고 있습니다.

다음 순서로 진행하면 됩니다.

1. 마스터 LASAL 프로젝트를 열어 Save/Rebuild/Link
2. 빌드 오류를 나에게 전달
3. 빌드 성공 후 사용자가 테스트 폴더로 복사
4. 동일 IP 클라이언트 강제 종료→재접속 시험
5. Group Enable ACK 직후 WPF 강제 종료→재시작 후 `0x2047=0`, `0x2045=3` 확인
