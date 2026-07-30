>
> Axis Stop 단계는 독립 리뷰까지 끝났고 P0–P2 결함이 없습니다. SDK Debug/Release 824/824, WPF Release smoke 114/114입니다. 이어서 Axis Reset도 한 번 승인된 `0x2024`를 타임아웃·선점 뒤 다시 보내지 않도록 `Begin(Reset 1회) / Resume(0x2028만)` 구조로 분리하고 있습니다. 의도적인 후속 PowerOn도 기존 Reset 완료 귀속을 무효화하는 엄격한 계약으로 구현합니다.
>
> Reset 모델 쪽에는 accepted continuation, timeout/cancel/status 예외의 continuation 보존, 그리고 같은 축 간섭의 expected/observed generation 증거가 들어가고 있습니다. GUI는 단순 타임아웃이면 다음 클릭에서 상태 조회만 재개하고, 실제 후속 축 명령 간섭이 확인된 경우에만 다음 명시적 Reset 클릭으로 새 Reset을 보내도록 구분할 예정입니다.
>
> 지금 Reset SDK 본체를 새 분리 상태기계로 교체하는 중이며, 병렬로 GUI의 재개/명시적 재Reset 흐름과 결정적 fake-RPC 테스트를 작성하고 있습니다. Reset 승인 후 단순 상태 실패와 실제 축 명령 간섭을 구분하는 것이 핵심이라, 전자는 `0x2028`만 재개하고 후자만 사용자의 다음 클릭으로 새 `0x2024`를 허용합니다.
>
> Reset 본체 교체가 예상보다 큰 편이라 모델·상태기계·예외 계약을 한 번에 맞추고 있습니다. GUI 쪽 설계와 smoke 시나리오는 정리됐고, SDK 메서드가 안정되는 즉시 코드 연결과 전체 빌드로 넘어갑니다. 현재까지 LASAL이나 테스트 폴더 복사는 하지 않았습니다.
>
> Reset SDK 본체는 다시 컴파일 가능한 상태로 복구됐습니다. 이제 split/간섭 계약 테스트를 추가 중이고, WPF에는 세 가지 회귀를 넣었습니다: 상태 실패 후 Reset 재전송 없이 재개, 간섭 확인 후 다음 명시 클릭에서만 새 Reset, Stop 선점 시 자동 Reset 재전송 없음. 최종 수치는 두 전체 스위트가 끝난 뒤 확정하겠습니다.
>
> WPF 쪽은 전체 Release smoke 117/117까지 통과했습니다. 정확한 wire 수는 상태 실패 후 재개가 Reset 1회/Status 4회, 간섭 후 사용자가 명시적으로 교체한 경우 Reset 총 2회이며 이전 continuation은 superseded 됩니다. SDK는 현재 23개 Reset 전용 계약까지 확장됐고, 빠진 invalid-resume·gate deadline 경계를 추가 점검 중입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+13 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisPowerOnRecoveryIntegrationTests.cs` (+14 -1)
>
> </details>
>
> Reset 전용 신규 12개 테스트 본문까지 들어가 현재 실행 중입니다. 기존 824개 회귀는 새 본체로 이미 통과했고, 지금은 split 1회 송신·세션 종료 경합·같은 축 간섭·상태 응답 폐기·다른 축 비간섭을 검증한 뒤 invalid-resume와 gate deadline 보강으로 넘어갑니다.
>
> Reset SDK 테스트는 29개까지 늘었습니다. 추가된 경계는 foreign/resolved/superseded/concurrent Resume zero-wire, mutation/status gate hard deadline, compound 단일 총 deadline, 거절된 새 Reset이 기존 pending을 지우지 않는지, ACK 게시 중 세션 종료 시 stale accepted 증거를 막는지입니다. 모두 구현된 뒤 Debug/Release 전체를 다시 돌립니다.
>
> 현재 검증 결과는 SDK clean Debug 842/842, Release 842/842, WPF Release smoke 117/117입니다. 첫 SDK 실행에서 기존 PowerState 동시성 테스트 1건이 한 번 실패했지만 같은 바이너리 재실행과 clean Debug가 연속 통과했고 Reset 신규 18개는 모두 통과했습니다. 지금 독립 리뷰로 새 coordinator 변경과 그 일시 실패의 연관 가능성을 확인 중입니다.
>
> 독립 리뷰가 최종 상태 게시 경합에서 P1 한 건을 잡았습니다. 마지막 안정 샘플을 게시해 continuation을 완료한 직후 취소/기한 검사를 다시 하면 “실패 예외인데 pending은 이미 제거된” 모순이 생길 수 있었습니다. 완료 선형화 지점 안에서 취소·기한을 먼저 판정하고, 완료가 이기면 이후 취소가 결과를 뒤집지 않도록 수정·재검증 중입니다.
>
> P1 수정은 반영됐고, 마지막 안정 샘플과 거의 동시에 취소되거나 deadline이 끝나는 두 경합 테스트를 추가했습니다. 완료 게시가 먼저 선형화되면 성공 결과가 유지되고, 완료 전 취소/기한이면 pending continuation이 남는 계약입니다. 이 추가로 최종 예상 SDK 수치는 843개이며 다시 Debug/Release를 돌리고 있습니다.
>
> Reset SDK 독립 리뷰는 종료됐고 남은 P0–P2가 없습니다. 최종 SDK Debug/Release는 845/845입니다. 이제 문서 수치·계약을 845/117 기준으로 갱신하면서, WPF 통합도 별도 리뷰와 최종 Release smoke를 한 번 더 수행하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+19 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisPowerOnRecoveryIntegrationTests.cs` (+6 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Axis Reset의 재전송 방지와 WPF 연동까지 구현은 끝났습니다. 지금 마지막으로 WPF 전체 스모크, 문서 수치, 독립 리뷰를 맞춘 뒤 현재 작업 트리 상태 그대로 정리하겠습니다.
>
> 최종 WPF 스모크는 117/117 통과했습니다. 특히 Reset 상태 확인 실패 후 재시도, 같은 축 간섭 후 명시적 재실행, Stop 선점 시나리오가 포함되어 있고 숨은 `0x2024` 재전송은 없습니다. 이제 문서와 남은 정적 검증만 닫고 있습니다.
>
> 독립 WPF 재검토도 P0–P2 문제 없이 끝났습니다. pending Reset이 남아 있는 동안 Axis 재조회와 이름 편집도 차단되어, continuation의 축 소유권이 UI에서 바뀌는 문제까지 막았습니다. 문서 갱신이 진행되는 동안 소스 잔여 패턴과 작업 목표 상태를 확인하겠습니다.
>
> 소스 잔여 패턴은 0건이고 구현 계획상 코드·자동 회귀·독립 리뷰 단계는 닫혔습니다. 문서는 오래된 현재 수치(818/818, 114/114, Group Enable 34개)를 새 기준(845/845, 117/117, 35개)으로 구분 갱신 중이며, 과거 시점 기록은 그대로 보존하고 있습니다.
>
> LASAL IDE는 건드리지 않고 SourceOnly 정적 계약을 다시 실행했으며 통과했습니다. 따라서 현재 확인된 범위는 SDK/WPF/정적 LASAL까지이고, PLC 다운로드와 실제 축·EtherCAT 동작 증명은 여전히 사용자 런타임 테스트 단계로 분리되어 있습니다.
>
> 문서의 현재 기준 수치에서 오래된 값은 모두 제거됐습니다. 핵심 API 체크리스트와 diff 검사를 마무리하는 중이며, 과거 기록과 실제 PLC 미검증 표기는 그대로 유지했습니다.
>
> 문서 내용 검토 중 현재 날짜보다 하루 앞선 `2026-07-30` 표기 4건을 찾았습니다. 이는 구현 기준일과 맞지 않으므로 모두 실제 작업일인 `2026-07-29`로 바로잡겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_SOURCE_REVIEW_2026-07-15.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+1 -1)
>
> </details>
>
> 이번 Reset 구현 슬라이스의 필수 계약은 닫혔습니다. 여기서 멈추지 않고 독립 리뷰가 남긴 낮은 우선순위 공백도 이어서 메우겠습니다: 기존 pending이 있는 상태에서 새 Reset 결과 불확실 시 보존, PowerOff 선점, ACK 직후 UI 저장 경계입니다.
>
> 추가 감사에서 같은 축 귀속 보호가 Stop/Reset에는 있지만 PowerOff continuation에는 아직 없다는 실제 설계 공백을 확인했습니다. PowerOff ACK 뒤 다른 같은 축 명령이 실행되면 나중의 Off/Standstill 상태를 원래 PowerOff의 완료로 잘못 귀속할 수 있으므로, 같은 세대 추적을 PowerOff에도 확장하겠습니다.
>
> Reset 회귀 보강과 PowerOff 귀속 구현은 서로 다른 파일로 분리해 진행 중입니다. PowerOff 쪽은 단순 카운터 추가가 아니라 ACK 시점 세대 기록, 상태 응답 publication, 취소·deadline·완료의 선형화까지 Stop/Reset과 동일한 계약으로 맞추고 있습니다.
>
> Reset의 두 번째 명시적 요청이 응답 유실로 불확실해지는 경계도 고정했습니다. 기존 continuation은 supersede되지 않고 보존되지만 세대가 달라져 재개는 zero-wire로 거부되며, SDK Debug/Release는 현재 846/846입니다. 최종 수치는 PowerOff 회귀까지 합쳐 다시 갱신하겠습니다.
>
> PowerOff 세대 모델과 typed interference 경로가 소스에 들어왔고, 전용 동시성 테스트를 작성 중입니다. 아직 편집 중인 상태라 현재 빌드 결과를 결론으로 사용하지 않고, 구현 완료 후 SDK 양 구성과 WPF 전체를 깨끗하게 다시 돌리겠습니다.
>
> WPF에는 리뷰 공백 3개가 추가됐습니다. 새 Reset 거절 시 기존 pending 보존, status-only Resume 중 PowerOff 선점, SDK ACK 수락 후 WPF 게시 전 Stop 선점까지 직접 검증하며 예상 총계는 120개입니다. PowerOff SDK 편집이 끝난 뒤 이 전체 묶음을 한 번에 실행합니다.
>
> PowerOff 코드 리뷰에서 두 경쟁 조건을 바로 잡도록 반영 중입니다. 하나는 superseded continuation을 완료로 오인할 수 있는 조건이고, 다른 하나는 ACK 수락 직후 cancel/deadline이 걸리면 accepted continuation을 잃을 수 있는 구간입니다. 둘 다 Reset에서 이미 해결한 선형화 패턴으로 통일합니다.
>
> PowerOff 리뷰에서 찾은 두 경쟁 조건까지 수정됐고 동일 소스 기준 SDK Debug/Release 855/855를 통과했습니다. 이제 독립 리뷰 두 갈래와 최종 WPF 120개 재실행으로 구현과 UI 경계를 다시 확인합니다.
>
> PowerOff 구현은 31개 전용 계약을 포함해 SDK 855/855로 확정됐고, 최종 소스 위에서 WPF도 120/120 통과했습니다. 현재 독립 리뷰가 accepted ACK 보존과 nonfinal supersede 경쟁까지 별도로 대조하고 있습니다.
>
> 독립 리뷰 결과 필수 수준의 결함은 없습니다. 다만 PowerOff에도 Reset과 같은 양방향 경계 테스트를 완성하겠습니다: 기존 pending이 있는 상태의 rejected/응답 유실 보존, 그리고 최종 완료 publication 뒤 늦은 cancel/deadline이 성공을 뒤집지 않는 경우입니다.
>
> PowerOff 독립 리뷰도 P0–P2 없이 끝났습니다. 현재 추가하는 4개는 기능 수정이 아니라 이미 구현된 보존·선형화 계약의 반대편을 고정하는 회귀입니다. 이 작업이 끝나면 수치와 문서를 다시 맞추고 이번 축 명령 안전성 묶음을 닫겠습니다.
>
> PowerOff의 양방향 회귀까지 완료되어 최종 SDK는 859/859, PowerOff 전용은 35개가 됐습니다. 현재 문서는 이 최종 수치와 새 mutation evidence/typed interference 계약으로 다시 맞추고 있으며, 소스·테스트 구현은 더 이상 변경하지 않고 있습니다.
>
> 문서 수치 교체는 완료됐고, 현재 PowerOff의 외부 귀속 한계를 “미구현”이 아니라 “구현 범위 밖”으로 정확히 바꾸는 중입니다. 즉 같은 프로세스·같은 연결 세션·같은 AxisReference는 검출하지만, PLC/다른 RPC/직접 SDO/그룹 명령은 실제 장비 시험으로만 확인합니다.
>
> 최종 정적 검증도 끝났습니다. 작업 트리 diff, 신규 파일 whitespace/conflict marker, LASAL SourceOnly가 모두 통과했고, 남은 실패는 이번 작업과 무관하게 이미 stage되어 있던 대형 히스토리 파일의 trailing whitespace뿐입니다. 그 파일은 건드리지 않았습니다.
</details>

이번 개발 체크포인트까지 구현 완료했습니다.

- Axis Reset: ACK 1회 후 `0x2028`만 재개하며 `0x2024` 자동 재전송 방지
- Axis PowerOff: 동일 축 명령 간섭을 mutation generation으로 검출하고 `LMCAxisPowerOffInterferenceException` 제공
- ACK/continuation 저장과 최종 상태·취소·deadline 판정을 원자적으로 처리
- WPF: Reset pending 보존, PowerOff/Stop 선점, confirmed interference 후에만 명시적 Reset 재실행
- 외부 PLC·다른 RPC 클라이언트·직접 SDO·그룹 명령은 process-local 귀속 범위 밖으로 명시

주요 파일:

- [LmcAxisPowerStateWait.cs](./LMC_Library/LMC_API_Delivery/src/LmcAxisPowerStateWait.cs)
- [LmcAxisResetWait.cs](./LMC_Library/LMC_API_Delivery/src/LmcAxisResetWait.cs)
- [MainWindow.xaml.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs)
- [AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md](./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md)

검증 결과:

- SDK Debug/Release: 각각 `859/859 PASS`
- WPF Release smoke: `120/120 PASS`
- 전용 계약: Reset 33개, PowerOff 35개, Stop 32개
- LASAL SourceOnly: PASS
- 독립 리뷰: P0–P2 없음
- 작업 트리 `git diff --check`: PASS

이번 추가분은 C#/WPF/문서/테스트이며 LASAL 소스와 테스트 폴더는 건드리지 않았습니다. stage/commit도 하지 않았습니다.

단, 기존에 stage되어 있던 [Elmo_Master_history_260721.md](./docs/history/Elmo_Master_history_260721.md)에 trailing whitespace가 있어 `git diff --cached --check`만 실패합니다. 이번 작업과 무관하며 그대로 보존했습니다.

실장 시험에서는 다음 세 가지를 확인하면 됩니다.

1. Reset ACK 후 상태 실패 → 두 번째 클릭에서 `0x2024`는 증가하지 않고 `0x2028`만 전송
2. pending Reset 뒤 같은 축 PowerOn/Stop/PowerOff → 자동 Reset 없이 interference 처리
3. PowerOff ACK 뒤 같은 축 명령 실행 → Resume이 typed interference로 종료되고 `0x2023` 재전송 없음

<details><summary>24 previous messages</summary>

> 현재 작업 트리를 다시 확인하고, 문서에 적힌 잔여 항목과 실제 소스의 미구현 경계를 대조해 다음 개발 대상을 선정하겠습니다. LASAL IDE는 사용하지 않고 외부 소스 편집과 PC 자동 검증만 진행합니다.
>
> 다음 단계는 하드웨어 대기 항목을 억지로 완료 처리하지 않고, 지금 소스만으로 실제 개선 가능한 항목을 고르는 것입니다. API 대칭성, WPF continuation 소유권, 문서상 IDE/PLC 의존성을 세 갈래로 동시에 감사하고 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 가장 큰 코드 공백은 Group PowerOn/PowerOff가 아직 “명령 ACK 수락 후 상태 확인 재개”를 SDK 수준에서 보존하지 못하는 부분입니다. 이 경계를 먼저 확정한 뒤 SDK와 WPF를 함께 보강하겠습니다. LASAL IDE/프로젝트 구조 변경이 필요한 항목은 이번 코드 작업과 분리합니다.
>
> 정적 감사에서 Group Power보다 먼저 막아야 할 경계가 확인됐습니다. 현재 병렬로 세 가지를 보강 중입니다: Axis PowerOn의 동일 축 mutation 귀속, GroupStop의 마지막 상태 샘플/취소/타임아웃 선형화, WPF의 Axis Stop·PowerOff continuation 소유권 및 PowerOff ACK 경계 복구입니다. 모두 명령 재전송 없이 상태 조회만 재개하도록 검증합니다.
>
> 현재 변경은 서로 겹치지 않는 파일 묶음으로 진행 중입니다. 완료되면 각 전용 테스트만 보지 않고 SDK Debug/Release 전체와 WPF Release smoke 전체를 다시 돌려, 기존 859/859·120/120 기준에서 새 카운트와 회귀 여부를 확인하겠습니다. 실제 PLC/LASAL 동작 완료로는 표시하지 않습니다.
>
> same-peer 비정상 종료 대응 소스는 현재 마스터의 `TCPIPServer`/`TCPMotionInterface`에 이미 반영돼 있고 SourceOnly 계약도 통과한 상태임을 다시 확인했습니다. 다만 `Classes.lcb` 등 LASAL 생성 메타데이터는 여전히 IDE Save/Rebuild 전 상태이므로, 이번 PC 코드 회귀와 별개로 사용자 빌드 단계가 남습니다.
>
> GroupStop 패치는 먼저 끝났고 독립 리뷰에 넘겼습니다. 핵심은 마지막 `0x2045` 결과를 게시하는 같은 임계구역에서 mutation generation, 취소/기한, 3회 안정 상태를 한 번에 결정하는 것입니다. Axis PowerOn과 WPF 패치는 아직 작성·테스트 중이므로 지금 시점에는 통합 PASS로 보고하지 않겠습니다.
>
> 독립 리뷰에서 GroupStop 패치의 보완점 두 개를 잡았습니다. 이미 승인된 continuation에 취소된 토큰으로 Resume할 때 일반 취소 예외로 빠져 evidence가 사라지는 경계와, post-write deadline 표시가 tracker 잠금 밖에서 갱신되는 경계입니다. 둘 다 지금 수정·회귀 추가 중이며, 리뷰 지적을 반영하기 전 상태는 완료로 취급하지 않습니다.
>
> Axis PowerOn 보강은 신규 9개와 당시 전체 872개 계약까지 통과했습니다. GroupStop 리뷰 보완 3개까지 합친 Debug 전체는 875/875 PASS이며, 지금 Release와 두 독립 리뷰를 마무리하고 있습니다. WPF는 현재 122/122 PASS지만 SDK 최종본과 함께 한 번 더 통합 실행하겠습니다.
>
> SDK 통합 검증은 제가 다시 실행해 Debug/Release 모두 875/875 PASS를 확인했습니다. 반면 WPF 독립 리뷰에서 “상태 확인 실패 후 Power Off 버튼 재클릭”이 기존 pending을 재개하지 않고 새 `0x2023`을 보낼 수 있는 P2를 잡았습니다. 현재 두 번째 클릭도 exact continuation의 `0x2028` status-only Resume으로 바꾸고 있습니다.
>
> WPF 재검토에서 더 중요한 탈출 경로 문제가 확인됐습니다. PowerOff continuation이 동일 축 명령 때문에 interference 상태가 되면 status-only 재개만 반복해서는 절대 완료될 수 있고, Axis PowerOn 복구가 걸린 경우 Close도 차단됩니다. Reset과 동일하게 “확인된 interference”일 때만 `Power Off Again`을 명시적으로 허용하고, 일반 timeout/status 실패는 계속 무재전송 Resume만 허용하도록 수정합니다.
>
> 현재 PowerOff UI 계약은 세 갈래로 정리했습니다: 일반 status/timeout 실패는 무재전송 Resume, monitor 실행 중 재클릭은 zero-wire 차단, typed interference가 확인된 경우에만 `Power Off Again (Confirmed Interference)`로 새 명령 1회를 허용합니다. replacement 거절 시에는 기존 pending/확인 상태를 그대로 보존하도록 테스트 중입니다.
>
> 최종 통합 기준으로 SDK Debug/Release 876/876, WPF Release 124/124, LASAL SourceOnly 계약까지 PASS했습니다. 독립 리뷰가 지적한 마지막 공백은 `Power Off Again` 거절 시 기존 confirmed-interference 상태 보존 테스트뿐이며, production 코드는 이미 그 상태를 보존합니다. 이 회귀 1건을 추가한 뒤 WPF 최종 카운트를 다시 확정하겠습니다.
>
> 구현과 독립 리뷰가 모두 닫혔습니다. 이제 코드 계약에 맞춰 현재 상태 문서의 테스트 카운트와 의미를 갱신합니다. 특히 Axis PowerOn도 mutation generation 귀속 대상에 포함하고, GroupStop 최종 publication 선형화 및 WPF PowerOff의 transient-resume/confirmed-interference replacement 정책을 명시하겠습니다.
>
> <details><summary>Edited 12 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_SOURCE_REVIEW_2026-07-15.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md` (+2 -2)
> - Wrote `./docs/architecture/AXIS_POWER_ON_ACCEPTED_ONCE_RECOVERY_2026-07-29.md` (+2 -2)
> - Wrote `./docs/architecture/AXIS_RESET_STABLE_ERROR_CLEARANCE_2026-07-29.md` (+3 -3)
> - Wrote `./docs/architecture/AXIS_STOP_STABLE_STANDSTILL_2026-07-29.md` (+2 -2)
> - Wrote `./docs/architecture/DRIVE_DS402_FAULT_ERROR_DIAGNOSTICS_2026-07-29.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+11 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+10 -0)
> - Wrote `./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md` (+13 -6)
> - Wrote `./docs/architecture/AXIS_POWER_ON_ACCEPTED_ONCE_RECOVERY_2026-07-29.md` (+16 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+9 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+5 -1)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 핵심 구현은 들어간 상태입니다. 지금은 문서의 테스트 수치와 복구 정책 설명을 최종 코드에 맞추고, 전체 회귀검증과 diff 정합성까지 확인하겠습니다. LASAL IDE나 테스트 복사본은 건드리지 않겠습니다.
>
> 문서에서 이전 체크포인트 수치가 남은 곳을 확인했습니다. 단순 숫자 교체만 하지 않고, 새 동작인 Power On 간섭 귀속·Group Stop 최종 판정·WPF Power Off 재시도 정책까지 함께 기록하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+12 -8)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+19 -12)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+17 -4)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
>
> </details>
>
> 구현 설명과 회귀 수치 정리는 끝났습니다. 이제 최종 WPF 전체 smoke를 다시 돌려 125개 결과를 독립 확인하고, 그동안 문서의 마지막 검토일과 남은 경계 표현도 맞추겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+8 -5)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -6)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+11 -8)
>
> </details>
>
> WPF Release 회귀는 최종 `125/125 PASS`로 확인됐습니다. 남은 검증은 stale 문구 검색과 작업트리·staged diff의 공백 오류 확인입니다. 기존에 사용자가 만든 staged 히스토리 변경은 별도 범위로 판정해 손대지 않겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -1)
>
> </details>
</details>

반영 완료했습니다.

- 외부 테스트에서 검증한 동일 IPv4 stale socket takeover 로직을 마스터에 반영했습니다. 테스트본과 비교해 실행 로직 차이는 없고 class명 주석·공백만 다릅니다.
  - [TCPIPServer.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPIPServer/TCPIPServer.st:163)
  - [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:402)
- Axis Power On 간섭 귀속과 최종 상태 판정, GroupStop 취소·deadline·generation 선형화를 구현했습니다.
- WPF Power Off는 명령 재전송 없이 status-only 재개하며, 확인된 간섭일 때만 `Power Off Again`을 허용하도록 수정했습니다.
  - [MainWindow.xaml.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:946)
- 현재 상태 문서도 갱신했습니다.
  - [현재 아키텍처 및 릴리스 상태](./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md:87)

검증 결과:

- SDK Debug/Release: `876/876 PASS`
- WPF Release smoke: `125/125 PASS`
- LASAL SourceOnly: `PASS`
- `git diff --check`: PASS
- 기존 staged 히스토리 파일의 trailing whitespace는 그대로 두었습니다.

이제 사용자가 마스터 LASAL 프로젝트를 `Save/Generate → Rebuild → Link` 해주면 됩니다. 현재 `Classes.lcb`가 새 `TCPIPServer` 등록 전 생성본이라 full static 검증은 그 전까지 의도적으로 실패합니다. 이번에는 커밋이나 테스트 폴더 복사는 하지 않았습니다.

<details><summary>55 previous messages</summary>

