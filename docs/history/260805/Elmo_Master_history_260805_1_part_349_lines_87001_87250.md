> 검증 대상의 의미도 따로 정리합니다. 이 함수의 Home 영수증은 같은 서비스 인스턴스에서의 warm continuation일 뿐 cold-restart 저널이 아니고, 일반 다축 clear/restore는 transaction이 아닙니다. 또한 생산 호출 21곳 중 11곳이 반환값을 소비하지 않으므로, 이 부분은 “완료 증거”가 아니라 별도 호출자 부채로 설계문서에 명시하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+121 -0)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+28 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+15 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+32 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+12 -2)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+4 -3)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+5 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+12 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+26 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+42 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+5 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+27 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+96 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+24 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+14 -88)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+33 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+29 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+19 -2)
>
> </details>
>
> 최종 독립 감사에서 선언·pragma·macro·구현 블록을 통째로 다른 위치로 옮겨도 내용 해시는 같아지는 위치 우회까지 확인했습니다. class 내부 선언, pragma/generated table, macro 영역, custom 구현의 상대 순서를 모두 고정했고, 마지막 집중 회귀를 다시 돌리고 있습니다.
>
> 독립 감사는 종료됐고, 현재 고정한 정적 범위에서 추가로 재현되는 우회는 없다는 결론입니다. 지금 병렬 실행 중인 것은 ownership 전체 `271`개 부정 회귀와 다섯-waiver 전체 SourceOnly 계약입니다. 이 둘이 끝나면 문서 수치와 Section 17 요청만 정리하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -1)
>
> </details>
>
> 집중 검증은 최종 `69/69 PASS`입니다. 전체 회귀에서 발견된 것은 구현 문제가 아니라 verifier가 method 앞뒤 공백까지 동일하다고 가정한 비교 오류였고, method 본문 비교로 바로잡았습니다. 전체 ownership/SourceOnly 두 검증은 다시 실행 중입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+8 -3)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+7 -2)
>
> </details>
</details>

`PublishAxisOwnership` 정적 계약과 post-C78 분리 설계를 완료했습니다.

- 집중 부정 검증 `69/69 PASS`
- ownership 전체 회귀 `271/271 PASS`
- 다섯-waiver 전체 SourceOnly PASS
- method-size 검증 `5/5 PASS`, 기존 debt `7` 불변
- LASAL source·generated declaration·Network는 수정하지 않음
- 이번 작업에서 매뉴얼·README·HTML은 수정하지 않음
- Home receipt는 warm continuation일 뿐 cold-restart journal이 아니며, 반환값을 무시하는 production caller 11곳은 별도 부채로 명시함
- post-C78 분리는 private helper 2개로 설계했으며 Section 17에는 포함하지 않음

근거는 [검증기](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1:1193), [분리 설계](./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md:540), [정적 증거](./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md:1234)에 반영했습니다.

이제 LASAL IDE에서 [Section 17](./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md:1322)의 hidden retained channel 1개와 private function 8개를 선언해 주세요. Network 변경 없이 `Save All → Rebuild하지 말고 IDE 종료` 후 알려주시면, 제가 generated ABI를 검사하고 다섯 waiver를 제거하겠습니다.

<details><summary>41 previous messages</summary>

> Section 17 선언은 아직 사용자 IDE 작업을 기다리는 상태로 두고, 그 사이 다음 큰 size debt인 `ReserveAxisOwnership`을 진행하겠습니다. 이번에도 LASAL source·generated metadata·Network와 매뉴얼은 건드리지 않고, 현재 구현의 semantic fence와 post-C78 분리 설계를 먼저 완성하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 알겠습니다. 앞으로 구현 중에는 설계·IDE handoff 문서만 필요한 시점에 갱신하고, 매뉴얼·README·HTML은 C78 빌드와 실기 동작이 안정된 뒤 한 번에 정리하겠습니다. 지금 진행 중인 `ReserveAxisOwnership` 정적 검증과 분할 설계도 그 원칙으로 마무리하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -0)
>
> </details>
>
> 현재 소스는 건드리지 않고 있습니다. `ReserveAxisOwnership`의 ABI, 포인터 출력, Home 영속 receipt 선행 검사, rebase 차단, 반복 요청 무변이 반환, lease/preemption 스냅샷의 magic-last 순서를 고정하는 전용 검증기를 추가하는 중입니다. 이 단계의 결과는 설계문서에만 반영하고 매뉴얼은 그대로 둡니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+532 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+547 -0)
>
> </details>
>
> 정적 감사에서 C78 전 반드시 고쳐야 할 실제 결함을 하나 찾았습니다. `ReserveAxisOwnership`가 선언되지 않은 `preemptRecordBase`를 5곳에서 사용하고 있습니다. 같은 함수에 이미 선언된 동일 용도의 `probeRecordBase`로 교정하겠습니다. 공개 ABI나 IDE 선언은 바뀌지 않으며, 이후 검증 해시를 새 기준으로 다시 산출하겠습니다.
>
> <details><summary>Created a file, edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+11 -11)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+14 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+44 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalAxisOwnershipReserve.Fixture.ps1` (+26 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+22 -7)
>
> </details>
>
> P0 오타를 교정한 새 기준에서 전용 검증은 `53/53` 변조 거부와 주석-only 허용까지 통과했습니다. 이제 기존 Publish/Rollback/통합 ownership 검증이 새 소스와 충돌하지 않는지 회귀 확인하고 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1` (+3 -3)
>
> </details>
>
> 회귀 검증은 아직 실행 중입니다. 현재까지 새 Reserve 전용 `53/53`, 기존 Publish `69/69`, Rollback `38/38`, size debt `5/5`가 통과했고, ownership 통합 변조 세트를 확인하는 단계입니다.
>
> 통합 검증은 계속 진행 중입니다. 독립 공격 검토에서 구현 본문을 잘못된 생성 영역으로 옮겨도 통과하는 위치 검증 누락을 추가로 찾았습니다. 현재 실행이 끝나는 즉시 Reserve의 직전/직후 구현 순서를 고정하고, relocation 음성 fixture를 추가해 다시 전부 돌리겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+36 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+27 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+0 -26)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+44 -5)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+36 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+26 -3)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+10 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+25 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 반영했습니다. 앞으로 작업 중에는 설계·IDE 인계 문서만 즉시 맞추고, 매뉴얼·README·HTML은 C78 빌드와 실기 동작이 안정된 뒤 한 번에 정리하겠습니다. 지금은 `ReserveAxisOwnership` 정적 방어와 post-C78 분할 설계만 마무리하겠습니다.
>
> `ReserveAxisOwnership` 본문을 다른 함수 내부로 숨겨도 통과하던 마지막 구조 우회 조건을 회귀 fixture로 추가하고 있습니다. 이 검사는 LASAL 선언이나 실행 로직을 바꾸지 않고 검증기만 강화합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+27 -6)
>
> </details>
>
> focused 검증기는 이제 56/56으로 통과했고, 주석만 바꾼 정상 fixture도 계속 허용됩니다. 독립 재공격 검토를 병행하면서 설계 문서에는 현재 소스 해시·두 helper 분할안·C78 이후 적용 경계를 기록하겠습니다. 매뉴얼 계열은 건드리지 않습니다.
>
> 독립 재공격에서 `END_FUNCTION;`처럼 문법적으로 잘못된 종료 토큰을 line-anchor 밖에 숨기는 우회가 하나 더 확인됐습니다. 모든 lexical `END_FUNCTION` 토큰을 세도록 바꾸고, 해당 공격을 57번째 회귀 fixture로 고정하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+24 -7)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+170 -0)
>
> </details>
>
> 추가 공격에서 함수 본문 전체를 top-level `IF FALSE THEN ... END_IF;`로 감싸도 기존 hash가 유지되는 우회가 확인됐습니다. 바로 앞 `HandleRequest END_FUNCTION`과 뒤 `ValidateAxisOwnership` 사이의 lexical gap을 공백 전용으로 고정하고 58번째 fixture로 막겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+46 -9)
>
> </details>
>
> 더 넓게 세 함수를 통째로 감싼 공격도 재현됐습니다. 특정 두 gap만 보지 않고 custom implementation 전체의 모든 함수 간 gap과 양 끝 경계를 고정하고, 각 함수 안에서 `IF/CASE/FOR/WHILE` open/close가 자체 완결되는지도 검사하겠습니다. 이렇게 해야 wrapper가 함수 본문에서 시작해 다른 함수에서 끝나는 변형까지 막을 수 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+118 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+19 -4)
>
> </details>
>
> 구조 우회 방어는 60개 공격 fixture까지 통과했고, fixture 자체의 엉뚱한 예외를 PASS로 오인하지 않도록 허용 오류 prefix도 좁혔습니다. 이제 전체 ownership aggregate와 5-waiver SourceOnly를 재실행해 국소 검사가 전체 계약을 깨지 않았는지 확인합니다.
>
> 독립 검토가 count만 맞춘 `END_IF` 선행 공격을 추가로 찾아냈습니다. 실행 중인 전체 검사는 구버전 검증기 결과가 되므로 중단하고, 함수별 block token을 순서대로 stack 검사한 뒤 최신 상태에서 다시 돌리겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+55 -16)
>
> </details>
>
> LASAL/ST의 `REPEAT … END_REPEAT`도 실제 vendor source에서 쓰이므로 stack 대상에 추가합니다. `UNTIL`은 중간 조건이고, 함수 경계를 넘는 `REPEAT` 공격을 62번째 fixture로 고정합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+14 -3)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+32 -1)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+1 -1)
>
> </details>
>
> 설계·IDE 인계 문서에는 현재 62개 구조 공격과 post-C78 두-helper 계획을 반영했습니다. 독립 최종 공격 검토가 끝나는 동안 ownership aggregate만 먼저 돌려 count와 통합 경로를 확인하겠습니다.
>
> aggregate는 동일한 대형 source를 여러 mutation으로 반복 검사해서 시간이 걸리고 있습니다. 현재까지 실패 출력은 없고, 완료되면 최신 총 fixture 수를 문서에 확정하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+3 -0)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+3 -0)
>
> </details>
>
> 독립 검토는 Reserve의 ABI·본문·top-level 가용성을 바꾸는 우회가 더 없다고 판정했습니다. 다른 함수의 body-only 변경은 이 fence의 의도적 비범위로 설계 문서에 명시했고, 이제 최신 62-fence 기준 전체 5-waiver SourceOnly와 size debt를 실행합니다.
