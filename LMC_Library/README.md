# LMC_Library 구성 안내

이 디렉터리는 LASAL Motion Control PC API의 개발, 예제와 배포 자료를 역할별로
분리한다.

| 폴더 | 용도 | 기준 |
|---|---|---|
| `LMC_API_Delivery` | API C# source, tests, 설계 기록 | 개발 source of truth |
| `LasalApiWpfTestApp` | source ProjectReference를 사용하는 개발/실기 진단 예제 | 내부 개발용 |
| `LMC_API` | packet 근거, 상세 개발 설명, source review, package build script | 내부 개발용 |
| `LMC_API_Distribution` | API, 독립 WPF 예제, 단일 API 사용설명서 | 외부 전달 기준 |

현재 버전은 `0.9.1-preview`다. current PC Debug/Release 테스트는 각각 752/752, 개발 WPF
Release build와 actual-control smoke는 110/110 통과했다. LASAL SourceOnly static 계약은
PASS했고, full static은 새 `TCPIPServer`의 stale `Classes.lcb` 등록을 정확히 차단했다.
send-priority 회귀는 Axis Reset, Admin `GroupMoveLinearRelative`, Group Enable
wait, D5 `SubmitSdo`/`CancelOperation`과 Recorder Trigger/Stop의 지연 ACK를
`ResultDiscarded`로 폐기하고, Recorder Release의 불확실한 결과는 `OutcomeUnverified`로
격리한다. 같은 Recorder handle의 동시 Start/Release는 wire 전에 거부하고, 네 Release surface의
`BeforeWire` 선점은 usable lease로 정확히 rollback한 뒤 명시적 retry 1회만 허용한다. WPF smoke는
delayed Configure의 accepted recovery-only handle 보존/명시 Release와 수동 Double Configure가
ordinary route/field로 유입되지 않는 fail-closed 분기를 고정한다. SDO Write policy/readiness
평가는 immutable cached snapshot을 wire 없이 사용하며
PLC bit 9와 empty SDK target을 별도 blocker로 표시한다. `Classes.lcb`의 general
`TryStartRead`/`TryStartWrite` declaration도 current source와
동기화되어 있다. BootId 5 legacy `0x13F` PLC capture에서 `0x1000:0` UInt32 4-byte SDO Read는
물리축 1~4 모두 Completed/Success를 반환했다. 2026-07-23 BootId 8 `0x213F`
general-inline capture에서는 첫 오류 뒤 Submit `ResourceBusy(9)` 고착을 재현해 executor state
machine을 수정했고, 이후 1/2/4-byte runtime 정상 동작과 관련 capture 분석을 확인했다.
후속 `Test2` capture에서는 current `0x613F`, static topology revision `0x15867EEC`와
`0x7E11` 1회 + `0x7E12` 7회의 exact 7-entry configured inventory를 확인했다. 이는
`0x7E13/0x7E22/0x7E23` dynamic health/I/O 증거가 아니다. 전체 D5
fault matrix와 최신 IDE build/download/smoke log는 남아 있다. 2026-07-23 live capture는
Admin `0x7D00/10/20/22`, 대표 absolute/relative group 경로, PowerOff final status,
`0x2051` None/ACS static alias, axis 1~4 drive read, D1 PI/D2 Bulk happy path와 D5
TypeMismatch 후 same-BootId 복구를 확인했다. 다만 기존 motion/group 25-command 전체
matrix, fault/race/soak, D4 runtime은 미완료다. D4 Double source/API와 exact recovery
계약, WPF qualification/retained-cleanup/reconnect adapter, durable journal
open/lock/status/interlock 및 restart zero-replay는 구현됐다. qualification은 recovery Guid에서
결정적 nonzero ConfigId를 만들고 Configure 전 journal을 arm한다. same-session cleanup은 third
Start가 exact `ResourceBusy`였을 때만 사용자 확인 뒤 Status/필요 시 Stop/Ready 확인과 B -> A
-> configuration 순서를 사용한다. unexpected third success 또는 ambiguous outcome이면 모든
same-session Release는 zero-wire이고 disconnect/reconnect 뒤 exact inventory inspection만
허용한다. conflicting inventory는 external/manual recovery 대상으로 남기며 자동 Release하지
않는다. 확인 checkbox는 preflight 뒤 소비되므로 실패 후 다시 확인해야 한다. confirmed-not-applied
pending intent는 동일 target의 exact intent만 재사용하고 새 intent/다른 target을 금지하며, retained
handle이 이미 ACK-success이면 wire replay 없이 durable confirm/resolve한다. reconnect는
ConfigRevision=0에서 0x7E4D -> 0x7E4A, 이후 occupied bank 0x7E49 또는 empty configuration
0x7E4B를 사용하고 partial Adopt handle을 보존한다. 확인 당시 없던 ConfigRevision 또는 exact
bank가 read-only inventory에서 발견되면 local journal만 갱신하고 Adopt/Release 전에 중단해 새
계획을 다시 확인하게 한다. config-only manual Double Configure adapter도 구현됐지만 네 D4
proof/route gate는
모두 `false`이고 PLC/live/pcap 증거는 없다. SDO Write 실행/API/GUI와 durable journal v2도
gate-off로 구현됐다. PI Write PLC handler, 8/12-byte와 extended SDO result는 미구현이다.
Phase 1의 D1/D2 기반 PI/Bulk compatibility facade는 구현됐으며, 실제 소비자가 없는 D6
static/handle wrapper는 current release에서 `Not Planned`로 닫았다. `0x2047` accepted-then-poll 수정본은 source/static 계약만
통과했으며 LASAL IDE build/download와 live ACK 재검증이 남아 있다.
Drive read facade는 실제 D5 SDO `0x6041:0` bit 3의 `HasDs402Fault`와 별도
`GetDriveErrorCode[Async]`의 `0x603F:0 UInt16/2` 결과를 노출한다. `0x2028`의 reserved
`StatusWord=0`, LASAL AxisErrorId와 이 두 drive 관측은 서로 대체하지 않는다. 새 opcode나
LASAL Network 변경은 없으며 실제 Reset 전후 drive/packet 검증은 남아 있다.
production 승인본으로 표기하지 않는다.

## 개발자 시작 위치

- 프로젝트 전체 현재 상태:
  [`../docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`](../docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)
- 구조/변경/릴리스 절차:
  [`LMC_API/API_DEVELOPMENT_GUIDE.md`](LMC_API/API_DEVELOPMENT_GUIDE.md)
- 최근 source review:
  [`LMC_API/API_SOURCE_REVIEW_2026-07-15.md`](LMC_API/API_SOURCE_REVIEW_2026-07-15.md)
- API 현재 구현 상태:
  [`LMC_API_Delivery/README.md`](LMC_API_Delivery/README.md)
- EtherCAT PI/Bulk/Recorder 내부 PLC 시험 순서:
  [`../docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md`](../docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md)
- native PMAS capture 분석과 구현 정렬:
  [`../docs/architecture/ELMO_NATIVE_API_PACKET_CAPTURE_ANALYSIS_2026-07-21.md`](../docs/architecture/ELMO_NATIVE_API_PACKET_CAPTURE_ANALYSIS_2026-07-21.md),
  [`../docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md`](../docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md)

## 사용자 전달 위치

- 패키지 안내:
  [`LMC_API_Distribution/README.md`](LMC_API_Distribution/README.md)
- 사용자 매뉴얼:
  [`LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf`](LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf)

`LMC_API/LMC_API`는 `0.9.0-pc-api` 구버전 보관본이며 새 배포에 사용하지 않는다.
