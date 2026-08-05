>
> 분할은 단순 줄 이동처럼 보여도 상태 샘플 시점과 preemption 결과가 같은 scan에서 유지돼야 하므로, helper ABI를 최소화하면서 원문 실행 순서를 보존하는 검토가 길어지고 있습니다. 속도보다 무동작변경 증거를 우선하고 있으며, 정적 verifier 보강은 이미 완료된 상태입니다.
>
> 상태 샘플 중복 문제는 해결했습니다. Encoder helper는 기존 `CopyAxisOwnershipPreemption`과 시간 샘플을 main에 그대로 두고 snapshot pointer와 이미 읽은 값만 받습니다. DS402 cleanup helper도 main에서 한 번 읽은 13개 값만 받아 client/time 재호출 없이 실행하도록 ABI를 확정했습니다.
>
> 독립 리뷰에서 verifier의 false negative 두 개를 재현했습니다. 현재 PLC 소스는 정상인데도, Commit 시작부터 validation 사이에 native call을 삽입하거나 validation 뒤 arm 전에 `PublishAxisOwnership()`을 호출해도 검증기가 놓쳤습니다. 이 검증기 결함을 먼저 막고 fixture에 재현 케이스를 추가하겠습니다.
>
> 독립 리뷰가 추가로 codec 실패 반환값 누락도 잡았습니다. `Result := -1`이 `0`으로 바뀌면 잘못된 retained 입력을 성공으로 오인할 수 있는데 기존 fixture가 놓쳤습니다. 현재 PLC 구현값은 정상이고, 검증기와 음성 변이를 보강하는 중입니다.
>
> 소스 분할은 적용됐고 현재 측정값은 Encoder main/helper 25.8/14.4 KB, DS402 main/receipt/cleanup 30.4/15.8/19.8 KB로 모두 경계 아래입니다. 이제 기존 verifier가 함수 위치 변경을 놓치거나 오판하지 않는지 회귀 검사를 돌립니다.
>
> 현재 소스 분할 뒤 기존 encoder 음성 fixture `56/56`은 그대로 통과했습니다. 전체 SourceOnly가 실패한 이유는 기능 회귀가 아니라 기존 검증기가 DS402 ACTIVE fence를 원래 단일 method 안에서만 찾도록 고정돼 있기 때문입니다. source가 확정되면 검증기를 main+helper 구조에 맞춰 갱신하겠습니다.
>
> 분할 소스는 CRLF/ASCII로 확정됐고, 이전 canonical CRLF 원문을 역인라인했을 때 SHA-256이 byte-exact 일치했습니다. 이제 별도 리뷰와 split-aware verifier를 추가 중이며, 그 검증이 끝나기 전에는 IDE handoff 문구를 확정하지 않겠습니다.
>
> 추가로 `LMCDiagnosticsService` 전체 25개 FUNCTION을 재측정했고 현재 32,768-byte 초과는 0개입니다. 이번 hard gate는 이 class에만 적용하고, 다른 class의 기존 대형 method는 별도 단계적 분할 대상으로 남겨 불필요하게 전체 검증을 막지 않겠습니다.
>
> 분할 리뷰의 현재 소스 의미는 정상입니다. helper 선언이 generated class metadata에 없는 것은 예상된 pre-IDE 상태이며, 이것 때문에 지금 Rebuild하면 안 됩니다. verifier에 새 waiver를 추가해 “소스 구현은 검사하되 IDE 선언 누락만 임시 허용”하도록 분리하고 있습니다.
>
> 검증기 보강 범위는 확정했습니다. 새 helper ABI, call/guard 순서, 중복 sampling 금지, cleanup TRUE/FALSE token 분기, `LMCDiagnosticsService` 전체 method 크기를 한 전용 fixture로 묶고 있습니다. 이 검사가 통과해야만 IDE 선언 목록을 최종 확정합니다.
>
> 소스 독립 리뷰도 PASS했습니다. pointer offset, 모든 DS402 receipt stage, cleanup safety-token 분기, 초기화와 RETURN 전파를 확인했고 실제 소스 blocker는 없습니다. 남은 것은 split-aware verifier와 IDE private 선언 3개뿐입니다.
>
> 전용 diagnostics split fixture는 현재 20개 bounded mutation을 포함합니다. 세 helper의 GLOBAL 승격, ABI 순서 변경, immediate guard 제거, 시간/입력 snapshot 재샘플, cleanup safety token 누출, cycle 인자 변경, method size 초과를 각각 거부하도록 구성했습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 반영했습니다. 설계·IDE 인계 문서는 구현과 함께 갱신하되, 매뉴얼은 C78 빌드와 실기 검증 결과가 안정될 때까지 동결하겠습니다. 지금 진행 중인 진단 클래스 분할 검증과 한 번에 처리할 IDE 선언 목록부터 마무리하겠습니다.
>
> 현재 소스 분할 자체는 독립 검토까지 통과했습니다. 남은 작업은 기존 단일 메서드 전제의 검증기를 새 3-helper 구조에 맞추고, 선언이 아직 없는 상태를 정확히 한 개의 임시 waiver로만 허용하는 것입니다. 이 단계가 끝날 때까지는 Rebuild를 요청하지 않겠습니다.
>
> 검증기 전용 negative fixture는 현재 23/23, TW19 barrier fixture는 37/37까지 통과했습니다. 이제 전체 SourceOnly에서 분리 전 메서드 위치를 가정하던 검사를 하나씩 새 helper 위치로 옮기는 중이며, 기능 gate를 약화시키는 방식으로 우회하지 않고 있습니다.
>
> 설계 문서는 요청하신 원칙대로 즉시 갱신했습니다. 이번 변경은 설계/IDE 인계 문서 2개뿐이고, 매뉴얼·README·HTML은 손대지 않았습니다. IDE 인계는 이제 hidden channel 1개와 private helper 8개를 한 번에 선언하는 형태로 정리됐습니다.
>
> 전체 검증은 한 번에 약 1분이 걸리고, 현재 실패들은 모두 분리로 인해 검사 위치가 바뀐 항목입니다. 별도 읽기 전용 감사를 병렬로 돌려 남은 “기존 큰 메서드 안에 있어야 한다”는 가정을 전수 확인하고 있습니다. 소스 기능 로직은 추가로 바꾸지 않고 검증기만 정합화하고 있습니다.
>
> 핵심 정적 gate는 통과했습니다. 현재 결과는 5-waiver 전체 SourceOnly PASS, diagnostics split 23/23, TW19 barrier 37/37, encoder 56/56, ownership 247/247, DS402 retirement 50/50입니다. 이제 제가 최종 파일 상태와 waiver 제거 시 정확한 선언 누락 실패를 독립 재확인한 뒤 IDE 작업만 요청하겠습니다.
>
> 검증기 수정이 끝났고 독립 감사에서도 남은 단일 메서드 가정이 없음을 확인했습니다. 이제 고정된 최종 파일로 focused 회귀와 전체 SourceOnly를 제가 다시 실행하겠습니다. 이 검증 후에만 사용자께 IDE 선언 1회 작업을 부탁드리겠습니다.
>
> waiver 제거 검사는 의도한 위치에서 정확히 실패했습니다: `HandleEncoderMaintenancePreemption`의 IDE 선언 누락입니다. 방금 병렬 실행은 이 예상 실패를 전체 호출의 실패로 취급해 PASS 출력까지 회수하지 못했으므로, 5-waiver 전체 검증은 단독으로 다시 실행하겠습니다.
>
> 최종 독립 재실행도 통과했습니다: 전체 SourceOnly 82.6초 PASS, DS402 retirement 50/50 PASS입니다. 이제 이 확정 수치만 설계 문서에 기록하고, 매뉴얼은 그대로 동결한 채 IDE 인계 단계로 넘기겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_ENCODER_MAINTENANCE_TW19_TW20_FIXED_ONE_ACTIVATION_2026-08-04.md` (+11 -3)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+12 -3)
>
> </details>
</details>

반영했습니다. 설계·IDE 인계 문서는 즉시 갱신하고, 매뉴얼은 C78·PLC 실기 결과가 안정될 때까지 동결합니다.

현재 정적 작업 결과:

- `LMCDiagnosticsService` 25개 메서드 모두 32,768 bytes 미만
- 최대 `ProcessAxisDs402Home`: 30,376 bytes
- 전체 5-waiver SourceOnly PASS
- Diagnostics split 23/23, TW19 barrier 37/37
- Encoder 56/56, Ownership 247/247, DS402 retirement 50/50 PASS
- 매뉴얼·README·HTML은 이번 작업에서 추가 갱신하지 않음

이제 LASAL IDE에서 [Section 17](./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md:1250) 작업을 부탁합니다.

- hidden server channel 1개
- `TCPMotionInterface` private function 2개
- `LMCControlCommandService` private function 3개
- `LMCDiagnosticsService` private function 3개
- 모두 `GLOBAL`로 만들지 않기
- Network 변경 없음
- 완료 후 **Save All → Rebuild하지 말고 IDE 종료**

끝나면 알려주세요. 제가 generated 선언과 `Classes.lcb`를 먼저 검사하겠습니다.

<details><summary>8 previous messages</summary>

> IDE 완료 보고 전이므로 먼저 현재 생성물에 선언 8개와 hidden channel이 실제로 생겼는지 재확인하겠습니다. 아직 없으면 Rebuild는 건드리지 않고, 그동안 병렬로 진행 가능한 다음 source-only 개발 항목을 설계 문서에서 골라 계속 진행하겠습니다.
>
> 생성 선언은 아직 0개이고 LASAL IDE도 실행 중이 아니므로, 현재는 IDE 완료를 기다리는 상태가 맞습니다. 기다리는 동안 사용자께 두 번째 IDE 방문을 만들지 않도록, 남아 있는 대형 LASAL 메서드와 설계 문서의 P0/P1 항목을 병렬 감사해 지금 한 번의 선언 작업에 같이 묶을 가치가 있는지 먼저 판단하겠습니다.
>
> 현재까지 확인된 우선순위는 명확합니다. P0-B의 다음 증거가 바로 Section 17 선언 생성이며, P0-C 이후 항목은 PLC 다운로드나 실축 증거가 필요합니다. 따라서 현재 handoff를 불필요하게 늘릴지 여부는 남은 대형 메서드 감사 결과로만 결정하고, 근거가 약하면 기존 8개 선언을 그대로 유지하겠습니다.
>
> 감사 결론은 “현재 IDE 인계는 확장하지 않는다”입니다. 누락은 정확히 8개 helper와 1개 channel뿐이며, 다른 class의 대형 메서드는 별도 단계가 안전합니다. 대신 지금은 선언이나 source를 건드리지 않고, 프로젝트 custom 메서드 7개의 크기를 매번 재현 가능하게 보여주는 scoped inventory/verifier를 추가하겠습니다. 이는 사용자가 지적한 대형 class 검색 문제를 다음 분할 순서로 관리하기 위한 정적 도구입니다.
>
> 현재 custom service 6개에는 초과 메서드가 정확히 7개이고, 그 외 13개는 vendor/framework라 분할 대상에서 제외했습니다. 새 도구는 이 7개를 “허용”하는 것이 아니라 현재 크기 이상으로 늘지 못하게 하고, 새 초과 메서드는 즉시 실패시키는 감소 전용 ratchet으로 만들고 있습니다. LASAL source와 Section 17은 그대로 유지합니다.
>
> baseline 측정은 7/7 정확히 일치했습니다. 출력은 30,000 bytes 이상만 보여 `HandleAxisCommands`의 남은 68-byte 여유까지 눈에 띄게 하고, 전체 함수 수와 debt 수는 PASS 요약에 남기도록 정리 중입니다.
>
> 독립 검증까지 끝나 새 크기 ratchet 자체는 6 classes/93 methods, self-test 5/5로 통과했습니다. 이제 이를 단독 도구로만 남기지 않고 전체 SourceOnly 실행에 연결하고, 성능 설계 문서에 현재 7개 debt와 다음 분할 순서를 기록하겠습니다. LASAL source·Section 17·매뉴얼은 계속 건드리지 않습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+22 -5)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+34 -0)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+5 -0)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+1 -1)
>
> </details>
</details>

추가 개발을 진행했습니다.

- [method-size ratchet verifier](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1) 추가
- Custom service 6개, 메서드 93개 전수 검사
- 기존 oversized debt 7개는 크기 증가 금지
- 신규 32,768-byte 초과 메서드는 즉시 실패
- `HandleAxisCommands`는 32,700 bytes로 남은 여유가 68 bytes임을 경고
- self-test 5/5 PASS
- 전체 5-waiver SourceOnly에 통합 후 83.5초 PASS
- [성능 설계 문서](./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md:268) 갱신

LASAL source, Network, [Section 17](./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md:1255), 매뉴얼은 변경하지 않았습니다.

현재 generated declaration은 여전히 0개입니다. 이제 Section 17의 hidden channel 1개와 private function 8개를 IDE에서 추가한 뒤 **Save All → Rebuild하지 말고 IDE 종료**해 주세요. 이후 제가 generated ABI를 검사하고 5개 waiver를 제거하겠습니다.

<details><summary>20 previous messages</summary>

> Section 17 완료 보고가 아직 없어 현재 생성 선언 상태부터 다시 확인하겠습니다. 선언이 여전히 없다면 handoff는 확장하지 않고, 다음 분할 대상인 `PublishAxisOwnershipPreemptionCleanup`의 현재 동작을 먼저 독립 fixture로 고정해 C78 기준선 이후 안전하게 분리할 준비를 진행하겠습니다.
>
> 기존 ownership 검증이 cleanup을 일부 다루고 있어 새 평행 계약을 만들면 중복됩니다. 감사에서 확인된 실제 공백은 정상 validation 이전 쓰기, replay 무변경, quarantine observer bit, incomplete bank 보존, token/generation operand 교차, Result domain, 최종 commit 중복 쓰기입니다. 이 공백만 기존 검증기에 집중 보강하겠습니다.
>
> 현재 cleanup 메서드는 기존 verifier가 상당 부분 보호하고 있어 별도 평행 fixture는 만들지 않습니다. 대신 기존 공통 assertion 안에 누락된 mutation fence만 추가하는 방향으로 좁혔습니다. 실제 LASAL 구현과 Section 17은 그대로 두고, future split 전 의미 보존 기준만 강화합니다.
>
> 후속 split 설계도 확정했습니다. 현재 handoff에는 넣지 않고 C78 기준선 이후 `ValidateAxisOwnershipPreemptionReplacement` private helper 1개로만 분리하면 GLOBAL adapter 29,301 bytes, helper 8,487 bytes가 됩니다. 이 계획은 설계 문서에 “미적용 계획”으로만 기록하고, 실제 source 선언은 아직 추가하지 않겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+41 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 반영해서 진행 중입니다. 설계·IDE handoff 문서는 검증 근거가 생길 때만 바로 갱신하고, 매뉴얼/README/HTML은 C78 빌드와 실기 결과가 안정될 때까지 동결하겠습니다. 지금은 LASAL 소스와 Section 17 선언 목록은 건드리지 않고, 이후 분할 대상인 ownership preemption cleanup의 의미 검증기를 먼저 강화하고 있습니다.
>
> 현재 기준선은 그대로입니다. TCP/Control/Diagnostics 소스 해시가 모두 이전 값과 일치하고, `Classes.lcb`에는 Section 17의 9개 이름이 아직 각각 0개이며 LASAL IDE도 실행 중이지 않습니다. 따라서 지금 진행 중인 작업은 검증기와 설계문서에만 한정되어 있습니다.
>
> focused 검증기는 단순 패턴 추가가 아니라 상태 변경 순서를 고정하는 작업이라, 현재 정상 메서드의 실제 경계를 먼저 계수했습니다. 확인값은 pre-commit 영속 쓰기 8개, `Result` 분포 `-1×1/-2×5/-3×8/0×1/1×1`입니다. 이 값들을 기준으로 허용되지 않은 조기 쓰기, replay 중 쓰기, `Result := 2` 같은 의미 회귀를 차단하는 중입니다.
>
> 독립 감사도 같은 결론을 냈습니다. cleanup 전체 영속 대입은 정확히 26개이고, `_memset/_memcpy`는 별도로 금지해야 incomplete quarantine의 보존 bank 파괴 우회를 잡을 수 있습니다. LASAL 구현은 아직 한 줄도 바꾸지 않았고, 이 검증기가 통과하기 전에는 예정된 메서드 분할도 적용하지 않습니다.
>
> <details><summary>Created a file, edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+204 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+8 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+208 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+18 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalAxisOwnershipPreemptionCleanup.Fixture.ps1` (+26 -0)
>
> </details>
>
> focused 검증기를 직접 구현했고, 전용 자체시험에서 16/16 변이가 모두 차단됐습니다. 이제 같은 검증기를 기존 ownership 종합 자체시험에 한 번만 합산했는지와, 기존 247개 계약에 회귀가 없는지를 확인하겠습니다.
>
> 기존 ownership 종합 자체시험도 `263/263`으로 통과했습니다(기존 247 + 신규 focused 16). 메서드 크기 debt ratchet 자체시험도 5/5 그대로 PASS했습니다. 이제 5개 pre-IDE waiver를 유지한 전체 `SourceOnly`를 다시 돌려, 다른 계약과의 통합 회귀가 없는지 확인합니다.
>
> 전체 5-waiver `SourceOnly`도 81.6초에 PASS했습니다. 확인된 기준은 service 6개/메서드 93개, under-limit 86개, 기존 size debt 7개이며 TCP·Control·Diagnostics 해시도 변하지 않았습니다. 현재는 새 검증기 코드 자체를 독립 리뷰 중이고, 리뷰가 끝나면 설계문서에 이 정적 증거만 기록하겠습니다. 매뉴얼은 건드리지 않습니다.
>
> 독립 리뷰에서 실제로 빠져 있던 3개 우회가 확인됐습니다. `OwnershipLeaseState/StartupState` 및 `+=` 쓰기, replacement 두 번째 전체축 루프의 Axis 9 누락, quarantine 루프 내부 `EXIT`가 기존 focused 검사에 통과했습니다. 이건 그대로 두면 안 되므로, 두 루프를 각각 독립 scope로 잘라 본문 전체를 고정하고 모든 `Ownership*State` 복합 대입을 인벤토리에 포함시키겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+53 -26)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+15 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+10 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+37 -0)
>
> </details>
>
> 리뷰에서 찾은 우회를 모두 막은 뒤 focused 자체시험은 `22/22`, ownership 종합은 `269/269`으로 다시 PASS했습니다. 특히 리뷰가 실제 주입했던 `LeaseState`, `StartupState`, `+=`, Axis 9 누락, `EXIT` 다섯 변이를 별도로 재현해 모두 거부되는 것도 확인했습니다. 최종 통합 `SourceOnly`를 한 번 더 확인하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+5 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+10 -0)
>
> </details>
>
> 최종 검증은 focused `24/24`, ownership 종합 `271/271`, 5-waiver 전체 `SourceOnly` PASS입니다. 독립 재검토도 차단 결함 없음으로 끝났습니다. 이제 이 결과를 성능 설계문서와 IDE handoff의 정적 증거 절에만 반영하고, Section 17 선언 목록과 매뉴얼은 그대로 두겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+4 -1)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+16 -0)
>
> </details>
</details>

반영했습니다. 이번 작업에서는 매뉴얼·README·HTML을 수정하지 않았고, 설계문서만 갱신했습니다.

완료 내용:

- preemption cleanup focused 검증기 및 전용 fixture 추가
  - focused mutation `24/24` 거부
  - ownership 종합 `271/271` 거부
  - method-size ratchet `5/5` PASS
  - 5-waiver 전체 `SourceOnly` PASS
- 독립 리뷰에서 발견한 State 우회, Axis 9 누락, quarantine `EXIT` 우회를 모두 수정하고 재검토 PASS
- LASAL TCP/Control/Diagnostics 소스 해시는 그대로이며 Section 17 선언 이름은 `Classes.lcb`에서 아직 모두 0개입니다.
- C78 빌드·다운로드·실축 검증은 아직 수행하지 않았습니다.

변경 근거는 [focused verifier](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1:96), [fixture](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalAxisOwnershipPreemptionCleanup.Fixture.ps1:1), [분할 설계](./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md:302)에 기록했습니다.

다음은 사용자가 LASAL IDE에서 [Section 17](./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md:1258) 그대로 hidden channel 1개와 private function 8개를 선언해 주세요. `GLOBAL/VIRTUAL GLOBAL` 및 Network 변경 없이 `Save All → Rebuild하지 말고 IDE 종료` 후 알려주시면 됩니다. 이후 제가 생성 선언을 검사하고 waiver 5개를 제거한 default `SourceOnly`를 통과시킨 뒤 C78 Rebuild를 부탁드리겠습니다.

<details><summary>21 previous messages</summary>

> Section 17 IDE 작업을 기다리는 동안에도 정적 작업은 계속 진행하겠습니다. 현재 C78 기준선을 흔들지 않도록 LASAL 소스·선언 목록은 그대로 두고, 다음 분할 후보인 `PublishAxisOwnershipDs402Receipt`의 기존 계약·누락된 의미 검증·안전한 분할 경계를 먼저 조사하겠습니다. 매뉴얼은 계속 동결합니다.
>
> 다음 oversized 대상은 현재 `PublishAxisOwnershipDs402Receipt`이며 실제 크기는 all-CRLF `47,507` bytes입니다. 소스를 바꾸지 않은 상태에서 확인한 결과, tokenless Stage-87 분기 전체가 항상 반환하므로 그 분기 하나를 private helper로 옮기면 public/helper 모두 제한 아래로 들어가는 구조가 가장 단순합니다. 동시에 기존 provider verifier가 조기 State 쓰기, 세 곳의 token/generation 교차, 추가 `Result := 3`을 실제로 허용한다는 것도 재현했습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 반영하겠습니다. 설계문서와 IDE handoff는 구현 판단이 바뀔 때 바로 갱신하되, 매뉴얼·README·배포용 HTML은 C78 빌드와 실기 검증이 안정된 뒤 한 번에 정리하겠습니다. 지금 진행 중인 DS402 receipt 검증 작업에서도 이 기준을 지키겠습니다.
>
> DS402 receipt 함수는 분할 전에 의미가 흔들리지 않도록 먼저 고정하고 있습니다. 현재 확인된 안전한 분할 경계는 Stage-87 복구 분기 하나이며, 이 경계를 쓰면 public adapter와 private helper가 각각 CRLF 기준 22,784바이트와 26,175바이트로 제한 안에 들어옵니다. 실제 LASAL 소스 분할은 Section 17/C78 이후로 미루고, 지금은 정적 검증기와 설계문서만 보강합니다.
>
> <details><summary>Created a file, edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+245 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+16 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+221 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+21 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402OwnerReceiptProvider.Fixture.ps1` (+26 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+12 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -4)
>
> </details>
>
> 집중 검증기는 현재 기준 소스를 통과했고, 기존 17개에 새 의미 훼손 22개를 더한 총 39개 음성 fixture를 모두 거부했습니다. 지금은 기존 ownership 271개 회귀와 전체 SourceOnly를 다시 돌리면서, 별도 검토 에이전트가 새 검증기가 우연한 문법 오류가 아니라 의도한 의미 훼손을 잡는지 독립 확인 중입니다.
>
> 전체 SourceOnly는 새 DS402 receipt 검증까지는 통과했지만, 뒤쪽의 기존 D5 SDO Write production gate에서 중단됐습니다. 이는 이번 변경과 무관한 현재 저장소 상태(`ExpectedSdoWriteAxis` 기본값 0 대 실제 gate 설정)입니다. 현재 소스의 축 설정을 확인한 뒤, 그 설정을 명시해서 전체 검증을 다시 실행하겠습니다.
>
> 현재 설정은 Axis 1 UI[24] SDO Write만 명시적으로 활성화된 상태라 `-ExpectedSdoWriteAxis 1`이 맞습니다. 그 조건으로 전체 SourceOnly를 재실행 중이며, 앞선 실패는 검증 옵션 불일치였고 소스 회귀는 아닙니다.
>
