>
> 전체 SourceOnly는 아직 진행 중이며 실패 출력은 없습니다. size self-test와 함께 끝나는 즉시 결과를 설계 문서에 확정하고, 마지막으로 diff/ASCII/해시/IDE 미실행 상태를 점검하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+3 -0)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+3 -0)
>
> </details>
>
> 전체 5-waiver SourceOnly와 size self-test가 최신 소스로 통과했습니다. 현재 값은 Control SHA `C976…`, 6 classes/93 methods, baseline debt 7입니다. 마지막으로 기존 Rollback/Publish focused 회귀와 파일 무결성·IDE 상태만 확인합니다.
</details>

반영했습니다. 앞으로 설계·IDE 인계 문서는 즉시 갱신하되, 매뉴얼·README·HTML은 C78/실기 안정화 후 일괄 정리합니다. 기존 매뉴얼 수정본은 보존했고 이번 단계에서는 추가 수정하지 않았습니다.

이번 작업 결과:

- `ReserveAxisOwnership`의 미선언 `preemptRecordBase` 5곳을 `probeRecordBase`로 교정했습니다. [소스](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st:3553)
- Control SHA-256: `C976CD364010EEFDFDDA8D7BC6D7655293DAD221FBEC908D50E5805CE4AFF072`
- Reserve verifier: `62/62` 공격 거부, comment-only 허용
- Ownership aggregate: `271/271` PASS
- Rollback `38/38`, Publish `69/69` PASS
- 5-waiver 전체 SourceOnly PASS
- 크기 검증: 6 classes, 93 methods, baseline debt 7, self-test `5/5` PASS
- post-C78 Reserve 2-helper 분할안을 [설계 문서](./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md:678)에 반영했습니다.
- 최신 정적 증거는 [IDE 인계 문서](./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md:1169)에 반영했습니다.

현재 Section 17의 channel 1개와 private function 8개는 generated metadata 검색 결과 모두 `0`건이라 아직 IDE 선언이 안 된 상태입니다. 다음에는 [Section 17](./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md:1359)만 LASAL IDE에서 수행한 뒤 **Save All → Rebuild하지 말고 IDE 종료**하고 알려주세요. 그다음 제가 ABI와 generated metadata를 검사하겠습니다.

아직 C78 빌드·다운로드·실축 동작을 증명한 것은 아닙니다.
