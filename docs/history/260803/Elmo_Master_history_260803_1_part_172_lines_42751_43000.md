>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+19 -7)
> - Wrote `./LMC_Library/LMC_API/DistributionPipeline.ps1` (+85 -0)
> - Wrote `./LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1` (+118 -0)
>
> </details>
>
> 입력 선택기 첫 단위 테스트에서 PowerShell `FileInfo`와 `DirectoryInfo`의 부모 속성 차이를 잡았습니다. 기능 설계 문제는 아니고 경로 상위 탐색 구현 오류라 바로 수정한 뒤, 실패를 숨기지 않도록 테스트 종료 코드도 별도로 확인하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/DistributionPipeline.ps1` (+6 -1)
>
> </details>
>
> 입력 선택기 회귀는 `63/63`, semantic policy는 `28/28`(15개 정책), manifest는 `56/56` 통과했습니다. 이제 검수한 DOCX/PDF의 정확한 바이트를 fingerprint에 포함한 실제 sibling candidate 빌드를 실행해 canonical 전후 해시까지 비교합니다.
>
> 실제 candidate 빌드는 진행 중입니다. 이 단계는 SDK Debug/Release, LASAL 정적 계약, WPF Debug/Release, candidate 자체 빌드와 semantic preflight를 모두 다시 실행하므로 몇 분 걸립니다. 현재까지 조기 실패나 transaction 잔여물 신호는 없습니다.
>
> 계속 실행 중입니다. 출력은 전체 검증이 끝날 때 한꺼번에 반환되는 방식이라 중간 PASS 수는 아직 보이지 않지만, 프로세스는 종료되지 않았고 timeout에도 걸리지 않았습니다. 완료 즉시 candidate/manifest/manual hash와 canonical 보존을 분리 확인하겠습니다.
>
> 현재 2분 경과했고 전체 회귀 단계가 계속 돌고 있습니다. 이번 빌드는 `AllowDirty`를 명시해 현재 작업트리를 증거에 `DIRTY-OVERRIDE`로 남기되, 입력 파일 해시와 source commit을 manifest에 함께 고정하도록 했습니다.
>
> 리뷰에서 manifest 결합의 TOCTOU 가능성 1건을 찾았습니다. 현재 빌드는 끝까지 보내 1차 측정값만 확보하되, 이 결과를 최종 후보로 인정하지 않고 pre-lock 입력 해시와 transaction baseline을 강제로 일치시키는 수정을 추가한 뒤 새 경로에서 다시 검증하겠습니다.
>
> 1차 실빌드는 299초 후 정상적으로 fail-closed 됐습니다. 새 blocker는 매뉴얼/README 전체에 요구하는 “preview는 production 승인 아님” 문구 불일치이며, canonical 해시 동일·candidate 미생성·stage/lock 0으로 rollback은 정확했습니다. 이제 이 정책 문구와 방금 발견한 manifest 해시 결합 결함을 함께 고칩니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/DistributionPipeline.ps1` (+146 -5)
> - Wrote `./LMC_Library/LMC_API/DistributionPipeline.ps1` (+3 -3)
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+113 -15)
> - Wrote `./LMC_Library/LMC_API/DistributionExampleREADME.md` (+1 -0)
> - Wrote `./LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1` (+109 -0)
>
> </details>
>
> TOCTOU와 ignored-input provenance 결함을 반영했습니다. 매뉴얼은 transaction lock 아래 메모리 snapshot으로 고정되고, manifest는 그 snapshot이 포함된 실제 baseline hash만 기록하며, promotion 직전 원본을 다시 검사합니다. 비canonical 매뉴얼은 이제 `-AllowDirty` 없이는 거부되고 항상 `dirty-preview`입니다. 회귀는 `79/79`로 늘려 통과했습니다.
>
> 강화된 2차 실제 빌드가 진행 중입니다. 이번에는 1차와 달리 example README의 no-go 문구가 사전 추출 검사에서 DOCX/PDF 포함 전부 PASS했고, 빌드 중 사용하는 매뉴얼 바이트도 외부 경로를 다시 읽지 않는 snapshot입니다.
>
> 2차 빌드는 약 2분째 정상 진행 중입니다. 완료 판정은 단순 build 성공이 아니라 semantic 15개 정책, manifest schema 2 검증, staged 매뉴얼 hash 일치, 입력 재해시, canonical tree 불변을 모두 통과해야 합니다.
>
> 현재도 조기 오류 없이 회귀가 계속되고 있습니다. 이 빌드가 성공해도 PLC/실축 검증으로 부르지는 않으며, PC build/static contract와 배포 candidate 무결성 증거로만 기록합니다.
>
> 재감사에서 원 3건은 닫힌 것으로 확인됐고, 추가로 Git `SourceCommit/WorktreeState`도 transaction 밖에서 읽는 동일 계열의 provenance race가 남아 있음을 확인했습니다. 2차 결과는 측정용으로만 받고, metadata까지 lock 내부 prepared input에 묶은 뒤 최종 빌드를 한 번 더 수행합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/DistributionPipeline.ps1` (+5 -0)
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+5 -11)
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+101 -20)
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+18 -12)
> - Wrote `./LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1` (+50 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+15 -9)
>
> </details>
>
> 최종 provenance 보강도 끝났습니다. `SourceCommit/WorktreeState`를 lock 안에서 준비하고 promotion 직전에 다시 확인하며, transaction 자체 lock/stage는 Git 상태 계산에서 제외합니다. `86/86` 회귀가 통과했으므로 새 경로에서 최종 실제 candidate를 생성합니다.
>
> 최종 빌드가 전체 회귀를 실행 중입니다. 지금 확인할 핵심은 이전 성공 수치 재현뿐 아니라 manifest의 commit/worktree/input hash가 같은 transaction snapshot에 묶이고, promotion 직전 재검증이 통과하는지입니다.
>
> 최종 정적 재감사도 blocker 없음으로 끝났습니다. 실제 빌드는 계속 진행 중이며, 남은 검증은 full candidate 실행 결과와 사후 hash/manifest 대조입니다.
>
> 약 3분째 조기 실패 없이 진행 중입니다. 성공 후에는 최종 candidate의 DOCX/PDF가 검수본과 byte-identical인지, manifest에 schema 2·`dirty-preview`·실제 input hash가 기록됐는지 별도 검사하겠습니다.
>
> 전체 빌드는 마지막 회귀/패키지 검증 구간으로 보입니다. 현재 프로세스는 계속 실행 중이고, transaction이 아직 완료되지 않았으므로 결과를 앞서 성공으로 판단하지 않고 있습니다.
>
> 최종 candidate가 성공했습니다. `semantic 15/15`, `transaction committed=True`, canonical 해시 전후 동일, stage/lock 0입니다. 이제 검수본과 candidate 매뉴얼의 byte hash, manifest 필드, 상태 문서의 기존 blocker 문구를 실제 결과로 맞춥니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md` (+50 -31)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+14 -12)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+17 -16)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+13 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+7 -2)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+7 -7)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+12 -11)
>
> </details>
>
> 최종 provenance candidate와 혼동되지 않도록 제가 만든 이전 중간 candidate(`...manual_2_0_hardened`)만 정리하겠습니다. 경로·부모·reparse 부재를 확인한 뒤 삭제하며, canonical과 최종 candidate는 유지합니다. 이 중간 산출물은 휴지통 복구 대상은 아니지만 같은 빌드로 재생성 가능합니다.
>
> 중간 candidate 정리는 완료했고 최종 provenance candidate만 남겼습니다. 이제 소스/문서 회귀, whitespace, HTML 기본 구조, final manual/manifest/canonical 무결성을 한 번에 재확인합니다.
</details>

수정 완료했습니다. 이제 최신 개발본에서는 실제 Motion/Power 명령과 Axis 1 SDO Write 전송 경로가 열립니다.

- Motion/Power: stale recovery identity를 archive/retire하고 앱을 재시작하면 활성화됩니다.
- SDO Write: Axis 1의 `0x2F00:24(UI[24])`, `Int32/4-byte`만 허용됩니다.
- SDO 실행 순서: Same-Value 4-ticket qualification → `Arm SDO Write` → `Confirm & Submit SDO Write`
- 새 LASAL 소스를 PLC에 다운로드해 capability bit 9가 광고되어야 합니다. 구 PLC 소스에서는 계속 차단되는 것이 정상입니다.
- Axis 2~4 및 다른 SDO target은 계속 차단됩니다.

실행할 최신 산출물은 [LasalMotionControlApiExample.exe](./LMC_Library/LMC_API_Distribution_candidate_20260731_manual_2_0_provenance/02_Example_Program/Run/LasalMotionControlApiExample.exe)입니다. 기존 canonical Distribution은 의도적으로 구버전 그대로이므로 그것을 실행하면 기능이 여전히 비활성일 수 있습니다.

검증 결과:

- SDK Debug/Release: `976/976`
- WPF Debug/Release: `235/235`
- Manifest: `56/56`
- Semantic policy: `28/28`, 실제 candidate `15/15`
- Transaction pipeline: `86/86`
- canonical 해시 전후 동일: `3AE733AF...CA1CA`
- stage/lock 잔여물: `0`
- DOCX/PDF는 검수본과 배포본이 byte-identical
- 최종 manifest: [RELEASE_MANIFEST.md](./LMC_Library/LMC_API_Distribution_candidate_20260731_manual_2_0_provenance/RELEASE_MANIFEST.md)

단, 이것은 PC/static/package PASS입니다. PLC 다운로드 후 실제 Motion/Power/SDO Write 실기 전송은 아직 검증하지 않았습니다. 중간 검증용 candidate 하나는 삭제했으며 휴지통 복구는 안 되지만 동일 빌드로 재생성할 수 있습니다. 커밋은 하지 않았습니다.

<details><summary>33 previous messages</summary>

> 다음 구현 항목은 설계/요구 문서에서 확인된 P1 안전 공백인 Group Reset 완료 검증으로 잡겠습니다. 현재는 `0x2049` ACK 뒤 이전 준비 상태가 남아 즉시 Group Move가 가능하므로, Reset 1회 전송 후 group/member status를 3회 검증하기 전까지 mutation을 차단하는 경로를 구현하고 전체 회귀로 확인하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Group Reset의 “ACK만 받고 완료로 간주하는” 경로를 계속 수정하겠습니다. 먼저 병렬 감사 결과를 합친 뒤, SDK의 1회 Reset 전송·재개 가능한 상태 검증과 WPF의 강제 인터록을 구현하고 회귀 테스트까지 돌리겠습니다. PLC 실기 완료로는 간주하지 않겠습니다.
>
> 감사 결과로 핵심이 확인됐습니다. `0x2045`만 세 번 읽는 것으로는 부족합니다. PLC의 `0x2049`는 각 축 오류 해제 요청만 내므로, 매 검증 라운드마다 그룹 상태 1회와 캡처된 모든 멤버 축 상태를 읽어 전부 오류 없음이 3회 연속일 때만 완료로 처리하도록 구현하겠습니다. SDK와 WPF를 파일 범위가 겹치지 않게 나눠 병렬 적용합니다.
>
> 현재 구현 경계는 이렇게 고정했습니다: Reset을 보내기 전에 멤버 목록을 읽어 고정하고, ACK 수락 즉시 기존 Power/Identity/Profile 준비 상태를 폐기합니다. 이후 검증 실패나 타임아웃이 나도 같은 Reset을 재전송하지 않고 상태 읽기만 재개합니다. Stop·Power Off는 계속 사용할 수 있게 유지합니다.
>
> 문서 쪽은 새 Group Reset 계약 문서가 생성됐습니다. SDK 쪽은 기존 Axis Reset 수준의 세션·재개·간섭 처리 규칙을 맞추느라 작업량이 큰 편이라 아직 구현 중입니다. 파일이 합쳐지는 즉시 먼저 컴파일 오류부터 잡고, 그다음 실제 송신 순서와 재전송 금지를 테스트하겠습니다.
>
> 구현 정책을 한 가지 조정했습니다. 이번 단계의 Reset 재개 증거는 동일 연결 세션에만 묶습니다. 별도 영구 저널 없이 재시작 복구까지 됐다고 과장하지 않으며, 연결이 끊기면 준비 상태를 모두 비활성화하고 자동 Reset 재전송은 하지 않게 합니다.
>
> SDK 모델은 들어왔고 WPF가 사용하는 필드명과 맞췄습니다. 현재는 공통 코디네이터와 실제 Begin/Resume 송신 경로를 연결하는 단계입니다. 이 연결이 끝나기 전에는 빌드 결과를 유효한 구현으로 보지 않겠습니다.
>
> 문서 독립 검토에서 과장 가능성을 잡았습니다. 최종 문서는 “이 SDK가 현재 9축 토폴로지를 증명한다”가 아니라 “PLC가 이번 세션에 반환한 유효한 멤버 스냅샷 전체의 오류 해제만 증명한다”로 낮추겠습니다. 예상 토폴로지 일치는 별도 검증 대상입니다.
>
> 첫 독립 코드 리뷰에서 WPF의 중요한 경계가 하나 더 드러났습니다. Reset 요청이 전송됐지만 ACK 전에 연결이 끊긴 경우를 단순 실패로 처리하면 새 Reset이 다시 열립니다. 이를 별도 “전송 결과 불명” 인터록으로 보존하고, 명확한 사전 실패/NACK과 구분하도록 수정 중입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 남은 핵심은 기능 자체보다 동시성 안전성입니다. 특히 Reset ACK 기록 중 Stop/PowerOff가 끼어들어 복구 상태를 덮어쓰는 경합과, compound Disable 경로가 잘못 차단되는 문제를 먼저 정리한 뒤 전체 테스트와 문서를 맞추겠습니다.
>
> Reset 이후 허용되는 안전 명령도 결과를 구분해야 합니다. Stop/PowerOff/Disable 요청이 PLC에서 명확히 거부된 경우에는 기존 Reset 증거를 버리지 않고, 수락되었거나 결과가 불명확할 때만 Reset 완료 귀속을 폐기하도록 SDK와 WPF를 같은 기준으로 맞추고 있습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md` (+43 -25)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+22 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+28 -10)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+31 -13)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+24 -13)
>
> </details>
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+18 -13)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+37 -28)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+12 -0)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+32 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+10 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+10 -2)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+1 -1)
>
> </details>
>
> 설계 문서와 사용자 문서도 구현 기준으로 전환했습니다. raw `GroupReset[Async]`만 ACK-only로 남고, WPF는 stable member 검증을 사용한다는 점, same-session 한계, 안전 명령/NACK 구분, 그리고 기존 배포 후보가 이번 소스 변경 전 입력이라 stale해졌다는 점까지 반영하고 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+10 -9)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+5 -4)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html` (+31 -6)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md` (+7 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
>
> </details>
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+8 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./docs/architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md` (+6 -0)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+5 -1)
>
> </details>
>
> SDK 리뷰에서 막아야 할 범위를 정리했습니다. SDK는 같은 그룹 명령을 선제 차단하고 pinned 축 변경은 generation으로 귀속을 무효화하며, WPF가 실제 UI의 축 Power/Move/Reset을 선제 차단합니다. 축 Stop/PowerOff가 실제로 수락되거나 결과 불명이면 새 exact reconciliation API로 SDK와 UI의 pending 상태를 동시에 종료합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md` (+6 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 구현은 거의 닫혔습니다. 현재 공유 소스 기준 SDK Debug 996/996, WPF 핵심 회귀 16/16까지 통과했고, 남은 일은 SDK 리뷰 지적을 반영한 최종 Debug/Release 전체 재검증과 문서의 정확한 테스트 수 갱신입니다.
>
> 최종 SDK Debug는 998/998 PASS로 올라왔습니다. 특히 지연 NACK과 진행 중 Resume의 경쟁, observer 재진입, 안전 명령의 generation 복원/종료를 모두 결정적 테스트로 고정했습니다. 이제 Release와 WPF 전체 결과를 서로 독립적으로 재확인합니다.
>
> 독립 리뷰도 P0/P1 잔여 결함 없음으로 끝났습니다. WPF는 실제 송신 경로를 직렬화하고, 미해결 Reset 동안에는 위험 명령만 막고 Stop/PowerOff/안전 Disable만 허용하며, 유효 NACK이면 기존 continuation을 정확히 보존합니다.
>
> WPF 전체도 Debug/Release 각각 251/251 PASS로 확정했습니다. 문서 감사에서 오래된 976/235 수치뿐 아니라, “Reset 뒤 readiness를 유지한다”는 현재 구현과 반대인 문장 하나가 발견돼 함께 바로잡고 있습니다.
>
> <details><summary>Edited 21 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./LMC_Library/README.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html` (+8 -5)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+2 -2)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+8 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+8 -5)
