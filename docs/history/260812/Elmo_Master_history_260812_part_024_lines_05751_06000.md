> - Ran `pwsh -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1`
> - Ran `powershell.exe -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1`
>
> </details>
>
> <details><summary>Edited 2 files, ran 13 commands</summary>
>
> - Ran `rg -n "RPC_INIT_FRESH_TCP_ONCE_V1|exact canonical|same-socket|fresh TCP|actual-EXE|SC_CLOSE|3/28|full Distribution|25-command|D1/D2/D5|safe-stop|strong-name|AuthentiCode|E-stop" LMC_Library/LMC_API/API_USER_MANUAL_KO.md`
> - Ran `Get-Content LMC_Library/LMC_API/API_USER_MANUAL_KO.md | Select-Object -Skip 270 -First 125`
> - Ran `Get-Content LMC_Library/LMC_API/DistributionSemanticPolicy.ps1 | Select-Object -Skip 200 -First 80`
> - Wrote `./LMC_Library/LMC_API/DistributionSemanticPolicy.ps1` (+21 -26)
> - Wrote `./LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1` (+1 -0)
> - Wrote `./LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1` (+26 -0)
> - Ran `rg -n "semantic_reversal" LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1; Get-Content LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1 | Select-Object -Skip 470 -First 145`
> - Ran `pwsh -NoProfile -ExecutionPolicy Bypass -File tmp/manual_2_3_final_audit/check-final-manual-policy.ps1`
> - Ran `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tmp/manual_2_3_final_audit/check-final-manual-policy.ps1`
> - Ran `powershell.exe -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1`
> - Ran `pwsh -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1`
> - Ran `& 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -c "from pypdf import PdfReader; import sys; t='\n'.join((p.extract_text() or '') for p in PdfReader(sys.argv[1]).pages); print(t[t.find('exact canonical')-100:t.find('exact canonical')+500]); print('---UI---'); i=t.find('UI operation'); print(t[i-100:i+400]); print('---KOREAN---'); i=t.find('같은 TCP'); print(i, t[i-100:i+400])" output/pdf/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf`
> - Wrote `./LMC_Library/LMC_API/DistributionSemanticPolicy.ps1` (+2 -2)
> - Wrote `./LMC_Library/LMC_API/DistributionSemanticPolicy.ps1` (+1 -1)
> - Ran `pwsh -NoProfile -ExecutionPolicy Bypass -File tmp/manual_2_3_final_audit/check-final-manual-policy.ps1`
> - Ran `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tmp/manual_2_3_final_audit/check-final-manual-policy.ps1`
> - Ran `pwsh -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1`
> - Ran `powershell.exe -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1`
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+21 -5)
> - Wrote `./LMC_Library/LMC_API/README.md` (+9 -2)
>
> </details>
>
> 후보 문서는 최종 해시가 고정됐고, DOCX OpenXML 오류 0·PDF 43쪽 전수 시각 검수까지 통과했습니다. 다만 release semantic gate의 자연어 반대 주장 우회가 리뷰에서 드러나 필수 문구를 canonical assertion으로 좁혔고, 현재 실제 DOCX/PDF 추출문도 PS 5.1/7 양쪽 3/3 PASS입니다. 최종 독립 재리뷰를 기다리는 동안 설계문서에는 Reserve split이 왜 지금 IDE 단계에서 멈춰야 하는지와 current byte/hash 기준을 반영했습니다.
>
> <details><summary>Edited 2 files, ran 13 commands</summary>
>
> - Ran `$d='output/doc/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'; $p='output/pdf/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'; foreach($f in @($d,$p)){ $i=Get-Item $f; $h=(Get-FileHash $f -Algorithm SHA256).Hash; "$f bytes=$($i.Length) sha=$h ignored=$(git check-ignore -q -- $f; if($LASTEXITCODE -eq 0){'true'}else{'false'})" }; "WINWORD_COUNT=$(@(Get-Process WINWORD -ErrorAction SilentlyContinue).Count)"`
> - Ran `git status --short; git diff --stat; git diff --check; git diff --cached --check`
> - Ran `rg -n "2\.3-candidate|RPC_INIT_FRESH_TCP_ONCE_V1|A23211A5|9E82A467|101/98/3|37968C3A|87158964|IDE.*declaration|current.*STOP" LMC_Library/LMC_API/README.md LMC_Library/LMC_API/API_USER_MANUAL_KO.md docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`
> - Ran `git ls-files output/doc output/pdf | Select-Object -First 60; git check-ignore -v output/doc/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx output/pdf/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf; Get-ChildItem output/doc -File | Select-Object Name,Length | Sort-Object Name | Select-Object -Last 10; Get-ChildItem output/pdf -File | Select-Object Name,Length | Sort-Object Name | Select-Object -Last 10`
> - Ran `pwsh -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1`
> - Ran `powershell.exe -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1`
> - Ran `pwsh -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1`
> - Ran `powershell.exe -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1`
> - Ran `pwsh -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master' -RunSelfTest`
> - Ran `powershell.exe -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master' -RunSelfTest`
> - Ran `pwsh -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master'`
> - Ran `powershell.exe -NoProfile -ExecutionPolicy Bypass -File LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master'`
> - Ran `& 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -m py_compile LMC_Library/LMC_API/Generate-ApiUserManualDocx.py`
> - Wrote `./LMC_Library/LMC_API/DistributionSemanticPolicy.ps1` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1` (+14 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 마지막으로 남은 버전 판정 우회(`2.4`가 현재 버전인데 이력에 `2.3-candidate`만 있는 경우)를 막았습니다. 지금 PS7·PS5.1에서 정책 전체와 실제 DOCX/PDF를 다시 검증하고, 독립 리뷰 결과까지 받은 뒤 우리 변경만 목적별로 커밋하겠습니다. 사용자 `Classes.lcb`와 기존 미추적 증거는 건드리지 않습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `& pwsh -NoLogo -NoProfile -File .\LMC_Library\LMC_API\Test-LmcDistributionSemanticPolicy.ps1`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\LMC_Library\LMC_API\Test-LmcDistributionSemanticPolicy.ps1`
> - Ran `& pwsh -NoLogo -NoProfile -File .\tmp\manual_2_3_final_audit\check-final-manual-policy.ps1`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tmp\manual_2_3_final_audit\check-final-manual-policy.ps1`
>
> </details>
>
> 정책 우회는 닫혔습니다. PS7·PS5.1 모두 `50 tests / 18 checks`, 실제 DOCX/PDF도 양쪽에서 `3/3 PASS`했고 독립 정책 리뷰도 CLEAN입니다. 이제 문서·생성기 최종 리뷰와 커밋 범위만 고정하고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short; git log -5 --oneline; git diff --stat; git diff --check; git diff --cached --check`
>
> </details>
>
> <details><summary>Ran 8 commands</summary>
>
> - Ran `rg -n "param\(|SelfTest|Current" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 | Select-Object -First 40; rg -n "param\(" LMC_Library\LMC_API\Test-LmcApiDistributionPipeline.ps1 | Select-Object -First 10`
> - Ran `& pwsh -NoLogo -NoProfile -File .\LMC_Library\LMC_API\Test-LmcApiDistributionPipeline.ps1`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\LMC_Library\LMC_API\Test-LmcApiDistributionPipeline.ps1`
> - Ran `& pwsh -NoLogo -NoProfile -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 -RunSelfTest`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 -RunSelfTest`
> - Ran `& pwsh -NoLogo -NoProfile -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1`
> - Ran `python -m py_compile .\LMC_Library\LMC_API\Generate-ApiUserManualDocx.py`
>
> </details>
>
> 전체 재검증 중 크기 검증기를 기본 인자로 호출했을 때 이 환경의 한 PowerShell 프로세스에서 `$PSScriptRoot` 기본값 평가가 비어 실패했습니다. 코드 실패가 아니라 호출 방식 문제라 저장소 루트를 명시해 동일 검증을 다시 돌립니다. 파일은 수정되지 않았습니다.
>
> <details><summary>Ran 9 commands</summary>
>
> - Ran `& pwsh -NoLogo -NoProfile -File .\LMC_Library\LMC_API\Test-LmcApiDistributionPipeline.ps1`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\LMC_Library\LMC_API\Test-LmcApiDistributionPipeline.ps1`
> - Ran `& pwsh -NoLogo -NoProfile -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master' -RunSelfTest`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master' -RunSelfTest`
> - Ran `& pwsh -NoLogo -NoProfile -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master'`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master'`
> - Ran `python -m py_compile .\LMC_Library\LMC_API\Generate-ApiUserManualDocx.py`
> - Ran `rg -n "Debug/Release Rebuild|OpenXmlValidator|OpenXML|OpenXml" LMC_Library\LMC_API\README.md docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md LMC_Library\LMC_API\API_USER_MANUAL_KO.md`
> - Ran `$p='LMC_Library\LMC_API\README.md'; Get-Content -LiteralPath $p | Select-Object -Skip 10 -First 10; Get-Content -LiteralPath $p | Select-Object -Skip 123 -First 12; $a='docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'; Get-Content -LiteralPath $a | Select-Object -Skip 984 -First 12`
>
> </details>
>
> 문서 최종 교차검토에서 표현 두 곳을 바로잡습니다. Rebuild 성공과 `339/339` smoke를 분리하고, DOCX 검증기는 실제 사용한 Office 2016 대상 validator임을 명시합니다. 동작·아티팩트는 바뀌지 않습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
>
> </details>
>
> <details><summary>Ran 7 commands</summary>
>
> - Ran `$files=@('output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx','output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'); foreach($f in $files){$i=Get-Item -LiteralPath $f; $h=Get-FileHash -Algorithm SHA256 -LiteralPath $f; [pscustomobject]@{Path=$f;Length=$i.Length;SHA256=$h.Hash}} | Format-Table -AutoSize`
> - Ran `$files=@('output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx','output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'); $rows=@(foreach($f in $files){$i=Get-Item -LiteralPath $f; $h=Get-FileHash -Algorithm SHA256 -LiteralPath $f; [pscustomobject]@{Path=$f;Length=$i.Length;SHA256=$h.Hash}}); $rows | Format-Table -AutoSize`
> - Ran `git diff -- LMC_Library/LMC_API/README.md LMC_Library/LMC_API/API_USER_MANUAL_KO.md docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md`
> - Ran `rg -n "AxisOwnershipReserveVerifierSelfTestOnly" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1 | Select-Object -First 10`
> - Ran `Get-Content -LiteralPath LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1 -TotalCount 65`
> - Ran `& pwsh -NoLogo -NoProfile -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master' -AxisOwnershipReserveVerifierSelfTestOnly`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master' -AxisOwnershipReserveVerifierSelfTestOnly`
>
> </details>
>
> 문서 리뷰에서 2026-08-05 수치를 `current`로 남긴 네 문장을 찾았습니다. 값 자체를 바꾸는 대신 당시 snapshot으로 명확히 고쳐, 2026-08-11 current handoff와 충돌하지 않게 정리합니다.
>
> <details><summary>Edited a file, ran a command</summary>
>
> - Ran `$p='docs\architecture\LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md'; $ranges=@(@{s=307;n=15},@{s=1032;n=12},@{s=1070;n=12},@{s=1171;n=12}); foreach($x in $ranges){'---'; Get-Content -LiteralPath $p | Select-Object -Skip ($x.s-1) -First $x.n}`
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+4 -4)
>
> </details>
>
> 모든 독립 리뷰가 CLEAN입니다. 최종 검증은 정책 `50/18`, 배포 파이프라인 `115`, 크기 ratchet `8/8` 및 `101/98/3`, Reserve `62/62`가 PS7·PS5.1에서 모두 통과했습니다. 이제 정확한 경로만 세 묶음으로 커밋합니다: 크기 debt, 배포 gate, 매뉴얼·현황 문서 순서입니다.
>
> <details><summary>Ran 13 commands</summary>
>
> - Ran `git diff --check; git diff --cached --check; git status --short`
> - Ran `git add -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1 docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md`
> - Ran `git diff --cached --name-only; git diff --cached --check; git diff --cached --stat`
> - Ran `git commit -m "test(lasal): retire publish method size debt"`
> - Ran `git add -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/DistributionSemanticPolicy.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1`
> - Ran `git diff --cached --name-only; git diff --cached --check; git diff --cached --stat`
> - Ran `git commit -m "test(distribution): harden executable and manual gates"`
> - Ran `git add -- LMC_Library/LMC_API/API_USER_MANUAL_KO.md LMC_Library/LMC_API/Generate-ApiUserManualDocx.py LMC_Library/LMC_API/README.md docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`
> - Ran `git diff --cached --name-only; git diff --cached --check; git diff --cached --stat`
> - Ran `git commit -m "docs(api): prepare reviewed 2.3 manual candidate"`
> - Ran `git log -3 --oneline; git show --stat --oneline --summary 2e8b505; git show --stat --oneline --summary f8e993e; git show --stat --oneline --summary 5c48f25; git status --short; git diff --check; git diff --cached --check`
> - Ran `$doc='output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'; $pdf='output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'; $di=Get-Item -LiteralPath $doc; $pi=Get-Item -LiteralPath $pdf; Write-Output ('DOCX_BYTES=' + $di.Length); Write-Output ('DOCX_SHA256=' + (Get-FileHash -Algorithm SHA256 -LiteralPath $doc).Hash); Write-Output ('PDF_BYTES=' + $pi.Length); Write-Output ('PDF_SHA256=' + (Get-FileHash -Algorithm SHA256 -LiteralPath $pdf).Hash); Write-Output ('HEAD=' + (git rev-parse HEAD)); Write-Output ('STAGED=' + ((git diff --cached --name-only | Measure-Object).Count))`
> - Ran `git diff --check dbcdade..HEAD; git diff --name-only dbcdade..HEAD; git diff --stat dbcdade..HEAD`
>
> </details>
>
> <details><summary>Plan</summary>
>
> 현재 HEAD와 설계 대비 감사 결과, Reserve 본체 분리는 IDE 선언 및 clean Classes 상태가 선행되어야 하므로 지금 적용하지 않습니다. 대신 실제로 닫을 수 있는 필수 회귀 두 건(퇴역 Publish size debt 재발 방지, 2.3 배포 매뉴얼/semantic release gate)을 구현 중입니다.
>
> - [x] 현재 Git 상태·권위 설계 문서·최근 완료 범위를 고정한다
> - [x] 설계 대비 다음 필수 구현 gap을 병렬 감사한다
> - [ ] Publish size ratchet과 2.3 배포 문서/semantic release gate를 구현한다
> - [ ] PS5/PS7 계약·문서 렌더·semantic/pipeline 회귀·독립 리뷰를 수행한다
> - [ ] 현재 STOP 경계와 다음 Reserve IDE handoff를 문서화하고 커밋 단위를 정리한다
>
> </details>
</details>

이번 개발 tranche를 완료하고 3개 커밋으로 분리했습니다.

- `2e8b505` — LASAL method-size debt 정리
- `f8e993e` — Distribution executable/manual gate 강화
- `5c48f25` — API 매뉴얼 2.3 후보 및 현황 문서 갱신

검증 결과:

- PS7·PS5.1 semantic policy: `50 tests / 18 checks PASS`
- Distribution pipeline: 각각 `115 assertions PASS`
- Method-size verifier: `8/8`, 현재 `101 methods / 98 under-limit / 3 debt`
- Reserve verifier: 각각 `62/62 PASS`
- 실제 DOCX/PDF policy: 각각 `3/3 PASS`
- `git diff --check`, cached check, Python compile PASS
- 독립 리뷰 3건 모두 P0/P1 없음

검토용 결과물:

- [DOCX](./output/doc/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx): 93,238 bytes, SHA-256 `A23211A5...C3182`
- [PDF](./output/pdf/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf): 1,013,620 bytes, SHA-256 `9E82A467...8E70`

DOCX Office 2016 대상 OpenXML 검증 오류 `0`, PDF는 A4 43쪽이며 전체 렌더와 embedded/subset font `8/8`을 확인했습니다. 두 결과물은 로컬 검토용이며 Git에는 포함하지 않았습니다.

중요한 경계는 그대로입니다. Current `Classes.lcb` Gate D identity STOP 때문에 full Distribution, LASAL IDE Save/Rebuild, PLC Download는 실행하지 않았습니다. 다음 `ReserveAxisOwnership` 분리는 reviewed Gate D transition 후 LASAL IDE에서 private helper 두 개를 먼저 선언해야 진행할 수 있습니다. 사용자 `Classes.lcb`와 기존 미추적 증거는 모두 보존했습니다.

<details><summary>644 previous messages</summary>

> 다음 tranche를 바로 이어갑니다. 우선 현재 HEAD와 설계문서의 미완료 항목을 다시 대조해, Gate D/IDE에 막힌 Reserve split을 억지로 건드리지 않으면서도 최종 설계에 직접 필요한 다음 구현을 선정하겠습니다. PC-only로 닫을 수 있는 항목은 구현·검증·문서·커밋까지 진행합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg -n "Elmo|LMC|ReserveAxisOwnership|method size|distribution" C:\Users\dreiv\.codex\memories\MEMORY.md | Select-Object -First 40`
>
> </details>
>
> <details><summary>Ran 11 commands</summary>
>
> - Ran `git status --short; git log -6 --oneline; rg -n "미완료|미적용|남았|다음 (적용|tranche|단계)|후속|P0|P1|TODO|필수" docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md docs\architecture\LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md LMC_Library\LMC_API\README.md | Select-Object -First 240`
> - Ran `$p='docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'; Get-Content -LiteralPath $p | Select-Object -Skip 1078 -First 95; Get-Content -LiteralPath $p | Select-Object -Skip 900 -First 75`
> - Ran `rg -n "LMCRecorderStore|HandleRequest|LMCEcatInputLatch|RtWork|Recorder|InputLatch" docs\architecture\LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md | Select-Object -First 180`
> - Ran `Get-Content -LiteralPath docs\architecture\LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md | Select-Object -Skip 260 -First 86`
> - Ran `rg -n "미구현|부분 구현|52/65|65개|미지원|not implemented|NotSupported|지원하지" docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md docs\PMAS_LASAL_Integrated_Analysis_2026-04-10.md LMC_Library\LMC_API LMC_Library\LMC_API_Delivery\src | Select-Object -First 250`
> - Ran `Get-Content -LiteralPath docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md | Select-Object -Skip 340 -First 60`
> - Ran `rg -n "HomeDS402Ex|SetOpMode|Set Operation Mode|OpMode|operation mode" . --glob '!**/bin/**' --glob '!**/obj/**' --glob '!Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb' --glob '!LMC_Library/LMC_API_Distribution/**' | Select-Object -First 250`
> - Ran `Get-Content -LiteralPath docs\status\API_DEVELOPMENT_PLAN_2026-07-30.md | Select-Object -Skip 285 -First 130; Get-Content -LiteralPath docs\architecture\MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md | Select-Object -Skip 390 -First 110`
> - Ran `Get-Content -LiteralPath LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 -TotalCount 125; rg -n "Candidate|Output|ManualDocxPath|ManualPdfPath|AllowDirty|WhatIf|DryRun|Destination|Sibling" LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 | Select-Object -First 120`
> - Ran `Get-Content -LiteralPath LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 | Select-Object -Skip 640 -First 345`
> - Ran `rg -n "function Resolve-LmcDistributionManualInputs|New-LmcDistributionManualInputSnapshot|Manual.*Path" LMC_Library\LMC_API\DistributionPipeline.ps1 | Select-Object -First 90; Get-Content -LiteralPath LMC_Library\LMC_API\DistributionPipeline.ps1 | Select-Object -Skip 605 -First 230`
>
> </details>
>
> 설계상 가장 직접적인 PC-only 다음 단계는 `clean checkout에서 최종 Distribution candidate를 끝까지 재검증`하는 항목입니다. 현재 작업 폴더의 사용자 `Classes.lcb`를 건드리지 않기 위해 HEAD 전용 분리 worktree를 만들고, 그 안에 검토 완료된 DOCX/PDF 바이트만 복사해 transactional candidate build를 시도하겠습니다. 이 작업은 PLC/LASAL IDE/Download를 실행하지 않으며 canonical 배포본을 덮어쓰지 않습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `$p='C:\work\Elmo\Elmo_Master_release_verify_5c48f25'; $resolved=[System.IO.Path]::GetFullPath($p); Write-Output ('TARGET=' + $resolved); Write-Output ('EXISTS=' + (Test-Path -LiteralPath $resolved)); git worktree list --porcelain`
> - Ran `git worktree add --detach C:\work\Elmo\Elmo_Master_release_verify_5c48f25 5c48f25388ab9e083596d6c1084ae9976d82badc`
> - Ran `$src='C:\work\Elmo\Elmo_Master'; $dst='C:\work\Elmo\Elmo_Master_release_verify_5c48f25'; $docDir=Join-Path $dst 'output\doc'; $pdfDir=Join-Path $dst 'output\pdf'; New-Item -ItemType Directory -Path $docDir -Force | Out-Null; New-Item -ItemType Directory -Path $pdfDir -Force | Out-Null; $docName='LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'; $pdfName='LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'; Copy-Item -LiteralPath (Join-Path $src ('output\doc\'+$docName)) -Destination (Join-Path $docDir $docName); Copy-Item -LiteralPath (Join-Path $src ('output\pdf\'+$pdfName)) -Destination (Join-Path $pdfDir $pdfName); Write-Output ('DOCX=' + (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $docDir $docName)).Hash); Write-Output ('PDF=' + (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $pdfDir $pdfName)).Hash); git -C $dst status --short`
>
> </details>
>
> 새 clean-checkout 실행 전에 실제 P1 공백을 찾았습니다. 배포 README는 사용자가 `.sln`을 열고 빌드하도록 안내하지만, 현재 pipeline은 solution을 복사만 하고 구성·프로젝트 membership을 검증하거나 빌드하지 않습니다. 잘못된 project path/GUID, extra project, Release `Build.0` 누락도 direct `.csproj`와 Run EXE gate를 통과할 수 있습니다. 이 계약을 먼저 구현한 뒤 clean checkout 전체 candidate를 검증하겠습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `Get-Content -LiteralPath LMC_Library\LMC_API_Distribution\02_Example_Program\LasalApiWpfTestApp.sln; Get-Content -LiteralPath LMC_Library\LMC_API_Distribution\02_Example_Program\LasalApiWpfTestApp\LasalApiWpfTestApp.csproj | Select-Object -First 80; Get-Content -LiteralPath LMC_Library\LMC_API\DistributionExampleREADME.md -TotalCount 40`
> - Ran `rg -n "function Invoke-LmcMSBuild" LMC_Library\LMC_API\Build-LmcApiDistribution.ps1; Get-Content -LiteralPath LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 | Select-Object -Skip 320 -First 125`
> - Ran `Get-Content -LiteralPath LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 | Select-Object -Skip 575 -First 70`
>
> </details>
>
> 동시에 설계문서가 직접 요구한 두 번째 PC-only 공백도 확인됐습니다. `LMCControlCommandService.HandleRequest`는 크기와 route shape만 검증돼 있어, body 내부의 균형 잡힌 의미 변경을 놓칠 수 있다고 설계서가 명시합니다. LASAL source를 수정하지 않고 전용 semantic/lexical fence와 적대 회귀를 추가하겠습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `git status --short; git worktree list --porcelain`
> - Ran `rg -n "solution|\.sln|candidate.*build|배포.*solution|Visual Studio" LMC_Library\LMC_API\README.md LMC_Library\LMC_API\DistributionExampleREADME.md docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md docs\status\API_DEVELOPMENT_PLAN_2026-07-30.md | Select-Object -First 120`
