> 전체 정적/PC 회귀도 통과했습니다: SDK Debug/Release `975/975`, WPF Debug/Release `233/233`, LASAL SourceOnly/full 정적 계약 PASS입니다. 다만 최종 감사에서 같은 세션 중 PLC identity가 A→B→A로 흔들리면 이전 SDO 검증이 되살아날 수 있는 P1을 1건 찾았습니다. 실제 전송 안전성을 위해 이 경우 검증을 영구 폐기하도록 지금 바로 보완하고 재시험하겠습니다.
>
> 최종 감사 범위는 세 가지로 좁혀졌습니다: 전송 직전 identity 고정, A→B→A에서 이전 검증 영구 폐기, 비정상 연결 종료 시 검증 폐기입니다. 세 경로 모두 자동 테스트로 고정한 뒤 전체 회귀를 다시 돌릴 예정입니다.
>
> 세 보완 경로의 집중 시험이 통과했습니다. SDK는 새 identity-pinned 전송 시험을 포함해 Debug `976/976`, WPF proof/lifecycle 시험은 A→B→A·disconnect·상태 이벤트 폐기까지 모두 PASS했습니다. 지금 Debug/Release 전체 회귀와 최종 EXE 해시를 다시 산출하고 있습니다.
>
> <details><summary>Edited 19 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+4 -1)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+3 -0)
> - Wrote `./LMC_Library/LMC_API/README.md` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+5 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+5 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+5 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -1)
> - Wrote `./docs/architecture/LMC_SDO_WRITE_SAME_VALUE_QUALIFICATION_2026-07-28.md` (+13 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+4 -3)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./LMC_Library/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -7)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+5 -5)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+7 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+1 -2)
>
> </details>
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./docs/history/260730/99_analysis_summary.md` (+9 -8)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+5 -6)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+31 -33)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+2 -2)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+7 -7)
>
> </details>
>
> 문서도 current 결과로 맞추고 있습니다. 배포용 `LMC_API_Distribution`은 현재 소스와 동기화되지 않은 이전 gate-off 묶음이라 건드리지 않고, 개발 Release 실행 파일만 다시 검증합니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+2 -2)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+1 -1)
> - Wrote `./docs/history/260730/99_analysis_summary.md` (+7 -4)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+1 -0)
>
> </details>
</details>

수정 완료했습니다. 이제 안전 조건을 통과하면 실제 Motion/Power/Axis1 SDO Write가 전송됩니다.

핵심 변경:

- Recovery identity mismatch 상태에서 `Archive and Retire Stale Recovery`로 기존 기록을 보존·종료한 뒤 새 세션에서 제어 기능을 다시 활성화합니다.
- Manual SDO Write는 검증된 session/build/BootId/MapRevision/target을 SDK 전송 직전 다시 비교합니다. 불일치하면 `0x7E50`을 보내지 않습니다.
- Identity가 A→B→A로 돌아오거나 연결이 끊겨도 이전 SDO 검증은 영구 폐기됩니다.
- SDO Write 허용 범위는 Axis1 `0x2F00:24`, `Int32`, 4-byte만입니다. Axis2~4는 계속 차단됩니다.

주요 구현은 [LmcDiagnosticsD5.cs](./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs:419), [MainWindow.Diagnostics.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:1759), [MainWindow.RecoveryRecordRetirement.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecoveryRecordRetirement.cs:236)에 반영했습니다.

사용 순서:

1. 현재 화면처럼 identity mismatch가 나오면 장비 상태를 직접 확인합니다.
2. 확인 체크박스 → `Archive and Retire Stale Recovery` → 최종 확인을 누릅니다.
3. 프로그램이 종료되면 다시 실행하고 PLC에 재연결합니다.
4. 축을 Load하고 상태를 읽은 뒤 `Power On → 작은 Motion → Stop → Power Off` 순으로 확인합니다.
5. SDO는 `Refresh Capabilities` 후 네 확인 항목을 체크하고 Same-Value Qualification을 실행합니다. 이 과정 자체가 현재 값과 동일한 Write를 1회 전송합니다.
6. PASS 후 원하는 승인 범위 값을 입력하고 `Arm SDO Write` → 동일 요청으로 `Confirm & Submit SDO Write`를 누릅니다.

최신 개발 실행 파일:

[LasalMotionControlApiExample.exe](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Release/LasalMotionControlApiExample.exe)

- EXE SHA-256: `363E613DCE768C269A74EFFC8CB3FF253C52875568E71CDC39B32D9E5956AFD5`
- DLL SHA-256: `2C1393058188B7484A45F5CC9ECC9485F6ADE13EAC9CE78A9E4577EF96925C7D`
- SDK Debug/Release: `976/976 PASS`
- WPF Debug/Release: `235/235 PASS`
- LASAL SourceOnly/full 정적 계약: PASS
- `git diff --check`, cached check: PASS

현재 `LMC_API_Distribution`은 이전 gate-off 배포본이므로 사용하지 마십시오. 또한 이번 검증에서는 실제 PLC 다운로드나 Motion/Power/SDO 명령을 보내지 않았습니다. SDO capability bit 9가 없는 기존 PLC라면 변경된 LASAL 프로젝트를 Rebuild/Link/download해야 합니다. 커밋은 하지 않았습니다.

<details><summary>34 previous messages</summary>

> 다음 로컬 P0로 넘어가겠습니다. PLC 다운로드·실기 시험은 장비 작업이 필요하므로 건드리지 않고, 문서에서 남은 핵심으로 표시된 `LMC_API_Distribution`의 원본 무변경 staging 조립과 semantic preflight 경로를 현재 소스 기준으로 먼저 감사하고 구현하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 스크립트는 검증 전에 기존 패키지 DLL을 덮어쓰고, 예제 빌드 산출물과 Run 폴더도 직접 바꿉니다. 이제 실제 배포 폴더는 읽기 전용 입력으로만 취급하고, 같은 볼륨의 새 후보 디렉터리에서만 조립·빌드·정책 검증하도록 경계를 바꾸고 있습니다.
>
> Semantic preflight가 외부 DOCX와 PDF의 실제 문구까지 교차 검증해야 하므로, 문서·PDF 검증 지침을 적용해 텍스트 일치와 두 형식 간 동기화 여부를 확인하겠습니다. 이 단계는 기존 매뉴얼을 편집하지 않습니다.
>
> <details><summary>Created 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/DistributionExampleREADME.md` (+47 -0)
> - Wrote `./LMC_Library/LMC_API/DistributionREADME.md` (+48 -0)
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+746 -0)
>
> </details>
>
> Semantic 검사에서 실제 모순도 하나 잡혔습니다. 패킷 맵 상단은 Axis 1 Write 활성으로 갱신됐지만, 하단 상세 설명은 아직 “bit 9=0, 모든 gate=false, allowlist empty”라고 남아 있습니다. 이건 검사 예외로 숨길 내용이 아니라 실제 문서 결함이므로 현재 소스 계약에 맞게 바로잡겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+16 -11)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+3 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+1 -1)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 실제 전송 경로 구현은 완료된 상태이고, 지금은 그 기능이 배포본에서 다시 비활성화되지 않도록 후보 패키지 생성·의미 검증을 마무리하겠습니다. 현재 배포 설명서가 아직 “SDO Write 비활성”이라고 적힌 경우에는 배포를 막되, 기존 배포 폴더는 절대 건드리지 않는지까지 확인합니다.
>
> 후보 생성 파이프라인과 독립 테스트 파일까지 준비됐습니다. 먼저 PowerShell 5.1 기준 구문·단위 테스트를 돌리고, 그 다음 실제 전체 빌드가 오래된 매뉴얼을 정확히 차단하는지 확인하겠습니다.
>
> 단위 검증은 통과했습니다: 매니페스트 56/56, 의미 정책 22/22(정책 항목 15개), 배포 트랜잭션 52/52입니다. 이제 실제 SDK·WPF 전체 빌드를 후보 폴더에만 수행해, 예상 차단점과 기존 배포본 불변성을 함께 측정합니다.
>
> 전체 회귀 빌드가 진행 중입니다. 출력이 버퍼링되어 아직 중간 로그는 없지만 프로세스는 정상 실행 중이며, 완료되면 차단 코드·기존 배포 해시·임시 폴더 잔여물을 한 번에 확인합니다.
>
> 아직 회귀 빌드가 실행 중입니다. 이 검증은 SDK Debug/Release, LASAL 정적 계약, WPF Debug/Release, 후보 프로그램 빌드를 모두 거치므로 시간이 걸립니다. 중단하지 않고 끝까지 받아 보겠습니다.
>
> 빌드는 계속 살아 있습니다. 현재까지 조기 실패는 없으므로 컴파일/테스트 구간을 통과 중인 것으로 보이며, 최종적으로 의미 검증에서만 차단되는지가 핵심입니다.
>
> 3분 이상 걸리고 있지만 테스트 프로세스가 종료되지 않은 상태입니다. 사용 중인 실행 파일을 덮어쓰는 작업은 없고, 별도 후보 staging에서만 작업하고 있습니다.
>
> 전체 검증은 예상한 매뉴얼 단계까지 가지 못하고 WPF Release 스모크 테스트에서 먼저 실패했습니다. 다만 안전성 검증은 통과했습니다: 기존 배포 해시는 전후 동일했고, 후보·staging·lock 잔여물은 모두 0입니다. 이제 스모크 테스트의 실제 실패 항목을 직접 실행해 원인을 고치겠습니다.
>
> 직접 재현에서도 WPF Release 스모크가 오래 실행되고 있습니다. 첫 전체 빌드 실패가 코드 단정 실패인지 실행 시간/프로세스 문제인지 구분하기 위해, 이번에는 스모크 실행의 원문 출력을 그대로 받고 있습니다.
>
> 직접 Release 스모크는 `235/235`로 재현 통과했습니다. 첫 실패는 재현되지 않은 일시적 실행 실패였고, 최종 정책 파일까지 안정된 상태에서 전체 배포 빌드를 한 번 더 수행합니다. 이번에는 실패 원문도 보존해 정확한 최종 차단점을 확인합니다.
>
> 두 번째 전체 회귀는 계속 진행 중이며 현재 조기 종료는 없습니다. 이번 실행은 소스가 더 이상 병행 변경되지 않는 안정된 상태라 최종 판정에 사용합니다.
>
> SDK와 WPF 양쪽 구성 전체를 다시 검사 중입니다. 이 단계가 끝나면 실제 오래된 DOCX/PDF 설명 때문에 차단되는지, 아니면 구현 자체에 다른 결함이 있는지가 확정됩니다.
>
> 전체 회귀가 3분을 넘겼지만 프로세스는 정상 실행 중입니다. 이전 Release 스모크 단독 실행이 약 110초였으므로 전체 두 구성 검증 시간 범위 안입니다.
>
> 아직 테스트 실행 중입니다. 배포 폴더는 스냅샷만 읽었고, 현재도 canonical 패키지에는 쓰지 않는 경로로 진행되고 있습니다.
>
> 두 번째 실행에서 실제 빌드 결함을 찾았습니다. 코드/스모크가 아니라 배포 스크립트가 DOCX 검사용 Python 문자열의 따옴표를 PowerShell 5.1에서 잘못 전달해 `SyntaxError`가 났습니다. 기존 배포는 여전히 완전 불변이고 임시 잔여물도 없습니다. 이 인자 전달을 고친 뒤 같은 전체 검증을 다시 수행합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+3 -2)
>
> </details>
>
> PowerShell 5.1 인자 전달을 수정했고, 같은 DOCX에서 제목 61개·표 105개를 정상 검출했습니다. 매니페스트 회귀도 56/56입니다. 이제 수정된 스크립트로 최종 전체 실행을 다시 돌립니다.
>
> 최종 전체 실행은 계속 진행 중입니다. 이번에는 앞서 깨졌던 DOCX 검사 구간까지 수정된 상태이며, 그 이후의 의미 정책 차단 결과를 기다리고 있습니다.
>
> 현재까지 조기 실패 없이 SDK/LASAL/WPF 회귀 구간을 수행 중입니다. 완료 로그에서 테스트 총계와 첫 의미 차단 코드를 함께 추려 보고하겠습니다.
>
> 회귀 실행은 정상적으로 계속되고 있습니다. 기존 배포 폴더는 시작 해시를 고정해 두었고, 종료 시 같은 스냅샷 함수로 재검증합니다.
>
> 약 4분 경과했습니다. 앞선 실행 시간과 유사하며, 프로세스는 살아 있습니다. 최종 결과가 나오기 전에는 배포 성공으로 간주하지 않겠습니다.
>
> 최종 판정이 나왔습니다. 전체 빌드는 수정된 DOCX 검사까지 통과했고, 의도대로 `MANUAL_SDO_WRITE_SCOPE`에서만 차단됐습니다. 즉 실제 Motion/Power/Axis1 SDO Write 구현은 코드에 있지만, 기존 배포 DOCX/PDF가 아직 “SDO Write 비활성” 상태라 새 배포 후보를 만들지 않은 것입니다. 기존 배포 해시는 동일했고 후보·staging·lock 잔여물은 0입니다. 이제 이 사실을 개발 문서에 정확히 반영하겠습니다.
>
> <details><summary>Created a file, edited 7 files</summary>
>
> - Wrote `./docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md` (+159 -0)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+12 -7)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+17 -9)
> - Wrote `./LMC_Library/LMC_API/README.md` (+4 -1)
> - Wrote `./LMC_Library/LMC_API/README.md` (+23 -0)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+61 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+4 -4)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+10 -6)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+13 -8)
>
> </details>
>
> 문서에는 “전송 구현/PC 검증 완료”와 “PLC live 미검증”, “배포 후보는 stale 매뉴얼 때문에 차단”을 분리해서 반영했습니다. 내부 개발 가이드 HTML은 Markdown 원본에서 다시 생성하고, 상태 대시보드는 기존 맞춤 레이아웃을 유지한 채 같은 판정으로 동기화했습니다.
>
> 최종 스크립트 검증은 모두 통과했습니다: 구문 7개 전부 PASS, manifest 56/56, semantic 28/28(15개 정책), transaction 52/52입니다. 마지막으로 whitespace, 산출물 해시, 임시 잔여물, 현재 Git 변경 범위를 확인합니다.
</details>

수정 완료했습니다. 새 개발 Release 실행본에서는 실제 Motion/Power 전송과 Axis1 SDO Write 전송이 가능합니다.

[최신 예제 프로그램 실행](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Release/LasalMotionControlApiExample.exe)

- 화면의 노란 recovery quarantine이 표시되면 장비 상태를 직접 확인한 후 `Archive and Retire Stale Recovery`를 실행하고 프로그램을 재시작해야 Motion/Power가 활성화됩니다.
- SDO Write는 안전상 Axis1 `0x2F00:24 Int32/4`만 허용합니다.
- SDO 탭에서 네 가지 확인 항목 선택 → `Run Same-Value SDO Write Qualification` PASS → `Arm SDO Write` → 동일 요청으로 `Confirm & Submit SDO Write`를 눌러야 실제 전송됩니다.
- 임의 SDO 주소와 Axis2~4 Write는 계속 차단됩니다.

검증 결과:

- SDK Debug/Release: `976/976` PASS
- WPF Debug/Release: `235/235` PASS
- 배포 manifest: `56/56` PASS
- semantic policy: `28/28`, 정책 15개 PASS
- 배포 transaction: `52/52` PASS
- Release EXE SHA-256: `363E613D...59E5956AFD5`
- `git diff --check`, cached check PASS

주의할 점은 기존 [LMC_API_Distribution](./LMC_Library/LMC_API_Distribution) 안의 EXE는 아직 구버전입니다. 기존 DOCX/PDF가 SDO Write 비활성 상태로 작성되어 있어 새 배포본 생성을 `MANUAL_SDO_WRITE_SCOPE`에서 의도적으로 차단했습니다. 기존 배포 폴더 해시는 변경되지 않았고 임시 후보/staging/lock도 남지 않았습니다.

상세 기록은 [transactional 배포 설계](./docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md)와 [예제 프로그램 절차](./LMC_Library/LasalApiWpfTestApp/README.md:785)에 반영했습니다.

실제 PLC/모터 전송은 이번 작업에서 수행하지 않았습니다. 최신 LASAL 프로그램 다운로드와 PLC bit 9/BootId/MapRevision이 일치해야 현장에서 최종 활성화됩니다. 커밋은 하지 않았습니다.

<details><summary>33 previous messages</summary>

> 다음 단계는 문서에 남은 P0 중 장비 없이 닫을 수 있는 항목을 현재 소스 기준으로 다시 선별하겠습니다. 배포 매뉴얼 불일치, clean-baseline 재현, 아직 source-level로 남은 기능 공백을 병렬 점검한 뒤 가장 큰 미완료 항목을 바로 구현하겠습니다.
>
> 현재 문서 기준으로 장비 없이 닫을 수 있는 가장 직접적인 P0는 외부 사용자 매뉴얼 후보입니다. canonical 배포본은 그대로 두고, 업데이트된 Markdown에서 별도 DOCX/PDF 후보를 만들어 의미 정책과 레이아웃을 검증하는 방향을 검토하겠습니다. 이 작업은 문서·PDF 산출물이라 `doc`과 `pdf` 스킬의 렌더·검증 절차를 적용합니다.
>
> 내부 매뉴얼에는 핵심 설명이 이미 있지만, 외부 정책 검사가 요구하는 “유일한 target”, “identity-pinned”, “exact target” 표현이 모호하고 표지 버전도 기존 배포본과 같은 1.9라 후보로 쓰기엔 불충분합니다. 2.0 후보로 명확히 구분하고 해당 안전 계약을 규범 문장으로 고정하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+15 -7)
>
> </details>
>
> 2.0 후보 DOCX/PDF를 canonical 밖 `output/doc`, `output/pdf`에 생성했습니다. 이제 PDF 직접 산출물과 Word에서 실제로 렌더한 DOCX를 각각 페이지 이미지로 변환해 전체 페이지의 잘림·겹침·한글 깨짐을 검사합니다. canonical 배포 문서는 계속 변경하지 않습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 다음 P0는 확정됐습니다. 현재 기능 구현이 아니라 오래된 canonical DOCX/PDF를 고정 입력으로 쓰는 배포 경로가 blocker이므로, 생성한 `2.0-candidate` 매뉴얼을 명시적으로 받도록 빌드 스크립트를 수정하고 실제 sibling candidate까지 통과시키겠습니다. canonical 배포본은 건드리지 않습니다.
