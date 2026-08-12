> - Ran `$docs=@(
> 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md',
> 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md',
> 'LMC_Library/LMC_API_Delivery/README.md',
> 'LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md',
> 'LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md',
> 'LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt',
> 'LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md',
> 'LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md',
> 'LMC_Library/LasalApiWpfTestApp/API_MAPPING.md',
> 'LMC_Library/LasalApiWpfTestApp/DESIGN.md',
> 'LMC_Library/LasalApiWpfTestApp/README.md',
> 'docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'
> ); git diff --check -- $docs; if(@(git diff --cached --name-only).Count -ne 0){ throw 'Staging was not empty before doc stage.' }; git add -- $docs; $staged=@(git diff --cached --name-only); $delta=@(Compare-Object $docs $staged); if($staged.Count -ne 12 -or $delta.Count -ne 0){ $staged; $delta; throw 'Unexpected staged doc scope.' }; git diff --cached --check; git diff --cached --stat; git commit -m "docs(wpf): record executable relaunch gate"`
> - Ran `git status --short; git diff --cached --check; git log -5 --oneline; $exe='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Debug/LasalMotionControlApiExample.exe'; if(Test-Path -LiteralPath $exe){ $i=Get-Item -LiteralPath $exe; $h=(Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash; [pscustomobject]@{DebugExe=$i.FullName;Bytes=$i.Length;LastWriteUtc=$i.LastWriteTimeUtc.ToString('o');SHA256=$h}|Format-List }`
>
> </details>
>
> <details><summary>Plan</summary>
>
> Actual EXE reconnect gate, PS5.1 verifier compatibility, evidence boundaries, and 12 authoritative docs are implemented, independently reviewed, and committed. The remaining live acceptance is the user's PLC retest; full distribution intentionally remains closed by the current unapproved Classes.lcb Gate D identity.
>
> - [x] 현재 reconnect 코드와 실제 EXE 회귀 경로 구현
> - [x] Debug/Release 및 binary-reference candidate 검증
> - [x] 독립 코드 리뷰와 코드 커밋
> - [x] PS5.1 LASAL 검증기 호환성 수정·양 host/실제 MSBuild 검증
> - [x] 배포 pipeline 연결·standalone candidate 검증 및 intentional Gate D STOP 확인
> - [x] 12개 권위 문서 최종 반영·독립 리뷰·커밋
>
> </details>
</details>

PC/WPF 측 수정과 자동 검증은 완료했습니다. 다만 실제 PLC 재접속 성공은 내일 실기 재시험이 필요합니다.

- X 종료 시 `0x405D ErrorId=-1`이 발생해도 로컬 연결을 최대 2회 정리하고 진단값만 보존합니다.
- 재실행 후 `0x8080 ErrorId=-1`이 같은 소켓에서 2회 지속되면, 실패 연결을 폐기하고 100ms 뒤 새 TCP 연결을 정확히 1회 시도합니다.
- 실제 EXE 종료→프로세스 exit→동일 EXE 재실행→mutex 재획득→fresh TCP 성공 경로를 검증했습니다.
- Debug/Release 전체 WPF 테스트 각각 `339/339` PASS.
- 실제 EXE relaunch gate Debug/Release 각각 `1/1` PASS, 정확히 TCP 3세션/요청 28회 `(13,2,13)`.
- 독립 코드·문서 리뷰 모두 CLEAN.

내일 시험할 파일은 [LasalMotionControlApiExample.exe](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Debug/LasalMotionControlApiExample.exe)입니다.

- SHA-256: `62B2AB41B90024C8CD07328927EED5D325471EC0E6666F1C5E0DD66521F62A99`
- 시작 로그에서 `ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V1`을 확인하면 됩니다.
- 시험 순서: Connect → X 종료 → 같은 EXE 재실행 → Connect 1회.

커밋:

- `14ccf58` — reconnect/종료 처리 수정
- `cbf2548` — 실제 EXE 종료·재실행 회귀 게이트
- `ad4af91` — PowerShell 5.1 LASAL 검증기 호환성 수정
- `dbcdade` — 권위 문서 12개 갱신

전체 Distribution 생성은 reconnect 문제가 아니라 현재 미승인 `Classes.lcb` Gate D identity 때문에 의도적으로 STOP 상태입니다. 해당 사용자 파일과 기존 미추적 자료는 건드리지 않았습니다.

<details><summary>62 previous messages</summary>

> Context compaction
>
> Source: automatic
> Status: completed
>
> 재접속 tranche는 코드·실제 EXE 회귀 게이트·문서까지 닫혔습니다. 이제 현재 HEAD와 설계 문서를 다시 기준으로 삼아, 다음 필수 개발 항목을 선정하겠습니다. 사용자 `Classes.lcb`와 기존 미추적 증거는 보존하고 PLC/LASAL Download 없이 진행 가능한 항목부터 구현·검증하겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "Elmo_Master|ReserveAxisOwnership|reconnect|Gate D" "C:\Users\dreiv\.codex\memories\MEMORY.md"`
> - Ran `$p='C:\Users\dreiv\.codex\memories\MEMORY.md'; $lines=Get-Content -LiteralPath $p; for($i=166;$i -le 191;$i++){ if($i -le $lines.Count){ '{0}:{1}' -f $i,$lines[$i-1] } }`
>
> </details>
>
> <details><summary>Ran 28 commands</summary>
>
> - Ran `git status --short; git log -8 --oneline --decorate`
> - Ran `rg -n "8\.6|ReserveAxisOwnership|Reservation|Publish|next|다음|미적용|TODO|backlog|Backlog" docs\architecture\LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md`
> - Ran `rg -n "ReserveAxisOwnership|OOP|미적용|backlog|next|다음|STOP|reconnect" docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`
> - Ran `rg -n "METHOD.*ReserveAxisOwnership|ReserveAxisOwnership|PublishPending|Publish|RollbackAxisOwnership|ValidateAxisOwnership" Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface.st`
> - Ran `rg --files | rg "TCPMotionInterface\.st$|LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP|ELMO_MASTER_CURRENT"`
> - Ran `git status --short; git log -8 --oneline --decorate`
> - Ran `rg -n "8\.6|ReserveAxisOwnership|Reservation|Publish|next|다음|미적용|TODO|backlog|Backlog" docs\architecture\LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md`
> - Ran `rg -n "ReserveAxisOwnership|OOP|미적용|backlog|next|다음|STOP|reconnect" docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`
> - Ran `rg -n "METHOD.*ReserveAxisOwnership|ReserveAxisOwnership|PublishPending|Publish|RollbackAxisOwnership|ValidateAxisOwnership" Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st`
> - Ran `rg --files Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class | rg "LMCControlCommandService.*\.st$"`
> - Ran `$p='docs\architecture\LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md'; $l=Get-Content -LiteralPath $p; 1030..1285 | % { if($_ -le $l.Count){ '{0}:{1}' -f $_,$l[$_-1] } }`
> - Ran `$p='docs\architecture\LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md'; $l=Get-Content -LiteralPath $p; 1628..1665 | % { if($_ -le $l.Count){ '{0}:{1}' -f $_,$l[$_-1] } }`
> - Ran `$p='Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'; $l=Get-Content -LiteralPath $p; 1..260 | % { if($_ -le $l.Count){ '{0}:{1}' -f $_,$l[$_-1] } }`
> - Ran `rg -n "FUNCTION LMCControlCommandService::ReserveAxisOwnership|PrepareAxisOwnershipReservation|CommitAxisOwnershipReservation|AxisOwnership.*Reservation|ReserveAxis" Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st`
> - Ran `$p='docs\architecture\LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md'; $l=Get-Content -LiteralPath $p; 900..1065 | % { if($_ -le $l.Count){ '{0}:{1}' -f $_,$l[$_-1] } }; git log -3 --oneline -- Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st`
> - Ran `$p='Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'; $l=Get-Content -LiteralPath $p; 2460..2495 | % { '{0}:{1}' -f $_,$l[$_-1] }; 4600..4640 | % { '{0}:{1}' -f $_,$l[$_-1] }; $bytes=[IO.File]::ReadAllBytes((Resolve-Path $p)); "file_bytes=$($bytes.Length) sha=$((Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash)"`
> - Ran ``$p=(Resolve-Path 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st').Path; $s=[IO.File]::ReadAllText($p,[Text.UTF8Encoding]::new($false,$true)); $start=$s.IndexOf('FUNCTION GLOBAL LMCControlCommandService::ReserveAxisOwnership',[StringComparison]::Ordinal); $token='END_FUNCTION'; $end=$s.IndexOf($token,$start,[StringComparison]::Ordinal)+$token.Length; if($end -lt $token.Length){throw 'not found'}; $block=$s.Substring($start,$end-$start); $utf8=[Text.UTF8Encoding]::new($false); $raw=$utf8.GetBytes($block); $lf=$utf8.GetBytes(($block -replace "`r`n","`n" -replace "`r","`n")); $crlf=$utf8.GetBytes((($block -replace "`r`n","`n" -replace "`r","`n") -replace "`n","`r`n")); $sha=[Security.Cryptography.SHA256]::Create(); "start=$start end=$end raw=$($raw.Length) lf=$($lf.Length) crlf=$($crlf.Length) sha=$(([BitConverter]::ToString($sha.ComputeHash($raw))).Replace('-',''))"``
> - Ran `rg -n "declar|선언|IDE|external|외부|implementation|method|function|생성|CodeGenerator|Classes\.lcb|Rebuild" docs\architecture\SIGMATEK_LASAL_coding_rules.md docs\architecture\SIGMATEK_LASAL_programming_method_study.md docs\architecture\SIGMATEK_LASAL_programming_error_prevention_guide.md`
> - Ran `$p='Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'; $l=Get-Content -LiteralPath $p; 185..380 | % { if($_ -le $l.Count){ '{0}:{1}' -f $_,$l[$_-1] } }; rg -n "^FUNCTION (GLOBAL )?LMCControlCommandService::" $p`
> - Ran `rg -n "1\.9|2\.3|DOCX|PDF|semantic|Documentation|문서" LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 LMC_Library\LMC_API\DistributionSemanticPolicy.ps1 LMC_Library\LMC_API_Distribution -g "*.md" -g "*.ps1" -g "*.txt" | Select-Object -First 240`
> - Ran `Get-ChildItem -LiteralPath LMC_Library\LMC_API_Distribution\03_API_User_Manual -Force | Select-Object Name,Length,LastWriteTimeUtc; Get-ChildItem -LiteralPath LMC_Library\LMC_API -Filter '*.docx' -Recurse | Select-Object FullName,Length,LastWriteTimeUtc; Get-ChildItem -LiteralPath LMC_Library\LMC_API -Filter '*.pdf' -Recurse | Select-Object FullName,Length,LastWriteTimeUtc; rg -n "ManualVersion|DocumentVersion|1\.9|2\.3|1\.0|Version" LMC_Library\LMC_API\DistributionSemanticPolicy.ps1 LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 LMC_Library\LMC_API\API_USER_MANUAL_KO.md`
> - Ran `Get-ChildItem -LiteralPath output -Recurse -Include *.docx,*.pdf -ErrorAction SilentlyContinue | Where-Object { $_.Name -like '*User_Manual*' -or $_.Name -like '*API*Manual*' } | Select-Object FullName,Length,LastWriteTimeUtc`
> - Ran `$p='LMC_Library\LMC_API\DistributionSemanticPolicy.ps1'; $l=Get-Content -LiteralPath $p; 1..520 | % { if($_ -le $l.Count -and ($_ -le 90 -or ($_ -ge 350 -and $_ -le 470))){ '{0}:{1}' -f $_,$l[$_-1] } }; $p2='LMC_Library\LMC_API\Build-LmcApiDistribution.ps1'; $l2=Get-Content -LiteralPath $p2; 840..900 | % { '{0}:{1}' -f $_,$l2[$_-1] }`
> - Ran `rg --files | rg "(render|manual|docx|pdf).*\.(py|ps1)$|API_USER_MANUAL.*(docx|pdf)$|pandoc|weasy|reportlab"`
> - Ran `rg --files -g "*.py" -g "*.ps1" | rg -i "manual|docx|pdf|render|markdown|distribution"`
> - Ran `Get-Content -LiteralPath LMC_Library\LMC_API\Generate-ApiUserManualDocx.py -TotalCount 240; "---PDF---"; Get-Content -LiteralPath LMC_Library\LMC_API\Generate-ApiUserManual.py -TotalCount 260`
> - Ran `rg -n "def main|ArgumentParser|add_argument|if __name__" LMC_Library\LMC_API\Generate-ApiUserManualDocx.py LMC_Library\LMC_API\Generate-ApiUserManual.py; Get-Content -LiteralPath LMC_Library\LMC_API\Generate-ApiUserManualDocx.py -Tail 90; Get-Content -LiteralPath LMC_Library\LMC_API\Generate-ApiUserManual.py -Tail 90`
> - Ran `git ls-files -- LMC_Library/LMC_API_Distribution/03_API_User_Manual output/doc output/pdf | Select-String -Pattern 'LASAL_Motion_Control_API_User_Manual_KO'; git check-attr -a -- LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf output/doc/LASAL_Motion_Control_API_User_Manual_KO_2.0-candidate.docx output/pdf/LASAL_Motion_Control_API_User_Manual_KO_2.0-candidate.pdf`
>
> </details>
>
> 현재 소스에서 바로 확인된 PC-only release gap이 하나 있습니다. Markdown 사용설명서는 `2.3-candidate`인데, 배포에 실제 포함되는 DOCX/PDF는 아직 7월의 `1.9` 스냅샷입니다. 이 두 산출물을 안전하게 갱신할 수 있는지 먼저 검증하겠습니다. 문서 레이아웃까지 확인해야 하므로 `doc`과 `pdf` 스킬의 생성·렌더·검증 절차를 사용합니다. Reserve 분리는 별도 감사 결과와 현재 Gate D STOP 조건을 대조한 뒤 진행 여부를 결정합니다.
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran ``Get-Content -LiteralPath 'C:\Users\dreiv\.codex\skills\doc\SKILL.md' -Raw; "`n---PDF SKILL---`n"; Get-Content -LiteralPath 'C:\Users\dreiv\.codex\skills\pdf\SKILL.md' -Raw``
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; & $py -c "import docx, reportlab, pypdf, pdfplumber; print('python_deps=PASS')"; Get-Command soffice,pdftoppm,pdftotext -ErrorAction SilentlyContinue | Select-Object Name,Source`
> - Ran `$candidates=@('C:\Program Files\LibreOffice\program\soffice.exe','C:\Program Files (x86)\LibreOffice\program\soffice.exe'); foreach($c in $candidates){if(Test-Path -LiteralPath $c){$c}}`
> - Ran `Get-ChildItem -LiteralPath 'C:\Program Files\Microsoft Office\root\Office16' -Filter WINWORD.EXE -ErrorAction SilentlyContinue | Select-Object FullName,Length,LastWriteTimeUtc`
> - Ran `$paths=@('C:\Program Files (x86)\Microsoft Office\root\Office16\WINWORD.EXE','C:\Program Files\Microsoft Office\Office16\WINWORD.EXE','C:\Program Files (x86)\Microsoft Office\Office16\WINWORD.EXE'); foreach($p in $paths){if(Test-Path -LiteralPath $p){Get-Item -LiteralPath $p | Select-Object FullName,Length,LastWriteTimeUtc}}`
>
> </details>
>
> DOCX/PDF 생성기는 현재 Markdown을 공통 소스로 사용하고, 필요한 Python 의존성도 모두 있습니다. 먼저 추적 파일을 건드리지 않고 임시 산출물을 만든 뒤 페이지 렌더링과 텍스트 계약을 검사하겠습니다. Word는 설치돼 있어 DOCX도 PDF로 변환해 전 페이지 시각 검증할 수 있습니다.
>
> <details><summary>Ran 9 commands</summary>
>
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; $docs='tmp\docs'; $pdfs='tmp\pdfs'; New-Item -ItemType Directory -Force -Path $docs,$pdfs | Out-Null; & $py LMC_Library\LMC_API\Generate-ApiUserManualDocx.py --source LMC_Library\LMC_API\API_USER_MANUAL_KO.md --output tmp\docs\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; & $py LMC_Library\LMC_API\Generate-ApiUserManual.py --source LMC_Library\LMC_API\API_USER_MANUAL_KO.md --output tmp\pdfs\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf; exit $LASTEXITCODE`
> - Ran `$doc=(Resolve-Path 'tmp\docs\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx').Path; $out=(Resolve-Path 'tmp\docs').Path + '\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate-word.pdf'; $word=$null; $d=$null; try { $word=New-Object -ComObject Word.Application; $word.Visible=$false; $word.DisplayAlerts=0; $d=$word.Documents.Open($doc,$false,$true); $d.ExportAsFixedFormat($out,17); $d.Close($false); $d=$null } finally { if($d -ne $null){$d.Close($false)}; if($word -ne $null){$word.Quit()} }; $py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; & $py -c "from docx import Document; from pypdf import PdfReader; from pathlib import Path; import hashlib; d=Document(r'tmp/docs/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'); print('docx_headings',sum(1 for p in d.paragraphs if p.style.name.startswith('Heading ')),'tables',len(d.tables),'paragraphs',len(d.paragraphs)); [print(p.name,len(PdfReader(str(p)).pages),p.stat().st_size,hashlib.sha256(p.read_bytes()).hexdigest().upper()) for p in [Path(r'tmp/pdfs/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'),Path(r'tmp/docs/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate-word.pdf')]]"`
> - Ran `$pdftoppm='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\bin\override\pdftoppm.cmd'; New-Item -ItemType Directory -Force -Path tmp\pdfs\pages,tmp\docs\pages | Out-Null; & $pdftoppm -png -r 100 tmp\pdfs\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf tmp\pdfs\pages\page; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; & $pdftoppm -png -r 100 tmp\docs\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate-word.pdf tmp\docs\pages\page; exit $LASTEXITCODE`
> - Ran `Get-Content -LiteralPath 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\bin\override\pdftoppm.cmd'; Get-Command pdftoppm | Format-List *`
> - Ran `Get-ChildItem -LiteralPath 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\bin' -ErrorAction SilentlyContinue | Select-Object Name,Length`
> - Ran `Get-Content -LiteralPath 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\bin\pdftoppm.cmd'`
> - Ran `Test-Path -LiteralPath 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin\pdftoppm.exe'; Get-ChildItem -LiteralPath 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler' -Force | Select-Object Name`
> - Ran `$exe='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin\pdftoppm.exe'; New-Item -ItemType Directory -Force -Path tmp\pdfs\pages,tmp\docs\pages | Out-Null; & $exe -png -r 100 (Resolve-Path 'tmp\pdfs\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf').Path ((Resolve-Path 'tmp\pdfs\pages').Path + '\page'); if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; & $exe -png -r 100 (Resolve-Path 'tmp\docs\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate-word.pdf').Path ((Resolve-Path 'tmp\docs\pages').Path + '\page'); exit $LASTEXITCODE`
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> from pathlib import Path
> from PIL import Image, ImageOps, ImageDraw
> for root in [Path(r'tmp/pdfs/pages'), Path(r'tmp/docs/pages')]:
>     files=sorted(root.glob('page-*.png'))
>     out=root.parent/'contact'
>     out.mkdir(parents=True,exist_ok=True)
>     for k in range(0,len(files),12):
>         batch=files[k:k+12]
>         tw,th=300,430
>         sheet=Image.new('RGB',(tw*4,th*3),(220,220,220))
>         draw=ImageDraw.Draw(sheet)
>         for i,p in enumerate(batch):
>             im=Image.open(p).convert('RGB')
>             im.thumbnail((tw-8,th-24))
>             x=(i%4)*tw+(tw-im.width)//2
>             y=(i//4)*th+20
>             sheet.paste(im,(x,y))
>             draw.text(((i%4)*tw+6,(i//4)*th+3),p.stem,fill=(0,0,0))
>         sheet.save(out/f'contact-{k//12+1:02d}.png')
>     print(root, len(files), len(list(out.glob('contact-*.png'))))
> '@ | & $py -`
>
> </details>
>
> Viewed 8 images
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `rg -n "TOC|Table of Contents|toc|Manual TOC" LMC_Library\LMC_API\Generate-ApiUserManualDocx.py`
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; & $py -c "from docx import Document; d=Document(); print([s.name for s in d.styles if s.name.startswith('TOC')])"`
> - Ran `rg -n "Copy-Item|sourceWpf|candidateWpf|manualDocxInput|manualPdfInput|distributionDirectory|stagingRoot|API_USER_MANUAL" LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 | Select-Object -First 180`
> - Ran `rg -n "SemanticPolicy|MANUAL_SDO|PREVIEW_PRODUCTION|ExecutableRelaunch|tested|ExampleExeSha|Build-LmcApiDistribution" LMC_Library\LMC_API\Test-LmcDistributionSemanticPolicy.ps1 LMC_Library\LMC_API\Test-LmcApiDistributionPipeline.ps1 LMC_Library\LMC_API\DistributionPipeline.ps1 | Select-Object -First 260`
>
> </details>
>
> 추가로 더 직접적인 설계 gap도 확인됐습니다. `PublishAxisOwnership`는 이미 두 helper로 분리돼 32 KB 아래인데, size verifier에는 과거 65 KB baseline debt가 남아 있어 40 KB로 다시 비대해져도 PASS합니다. 이건 실제 구조 회귀를 놓치는 P1이므로 먼저 ratchet을 닫겠습니다. LASAL source·IDE·`Classes.lcb`는 건드리지 않고 verifier와 self-test만 수정합니다.
>
> <details><summary>Edited a file, ran 2 commands</summary>
>
> - Wrote `./LMC_Library/LMC_API/Generate-ApiUserManualDocx.py` (+18 -0)
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; New-Item -ItemType Directory -Force -Path tmp\docs_v2 | Out-Null; & $py LMC_Library\LMC_API\Generate-ApiUserManualDocx.py --source LMC_Library\LMC_API\API_USER_MANUAL_KO.md --output tmp\docs_v2\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; $doc=(Resolve-Path 'tmp\docs_v2\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx').Path; $out=(Resolve-Path 'tmp\docs_v2').Path + '\rendered.pdf'; $word=$null; $d=$null; try { $word=New-Object -ComObject Word.Application; $word.Visible=$false; $word.DisplayAlerts=0; $d=$word.Documents.Open($doc,$false,$true); $d.ExportAsFixedFormat($out,17); $d.Close($false); $d=$null } finally { if($d -ne $null){$d.Close($false)}; if($word -ne $null){$word.Quit()} }; & $py -c "from pypdf import PdfReader; print('pages',len(PdfReader(r'tmp/docs_v2/rendered.pdf').pages))"`
> - Ran `$exe='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin\pdftoppm.exe'; New-Item -ItemType Directory -Force -Path tmp\docs_v2\pages | Out-Null; & $exe -f 4 -l 7 -png -r 120 (Resolve-Path 'tmp\docs_v2\rendered.pdf').Path ((Resolve-Path 'tmp\docs_v2\pages').Path+'\page'); exit $LASTEXITCODE`
>
> </details>
>
> Viewed 4 images
>
> <details><summary>Edited a file, ran 8 commands</summary>
>
> - Ran `$doc=(Resolve-Path 'tmp\docs_v2\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx').Path; $word=$null; $d=$null; try{$word=New-Object -ComObject Word.Application;$word.Visible=$false;$word.DisplayAlerts=0;$d=$word.Documents.Open($doc,$false,$true); if($d.TablesOfContents.Count -gt 0){$d.TablesOfContents.Item(1).Update()}; $seen=@{}; foreach($p in $d.Paragraphs){$n=[string]$p.Range.Style.NameLocal; if($n -match 'TOC|목차'){if(-not $seen.ContainsKey($n)){$seen[$n]=[pscustomobject]@{Count=0;Font=$p.Range.Font.Size;Before=$p.Format.SpaceBefore;After=$p.Format.SpaceAfter;Line=$p.Format.LineSpacing}};$seen[$n].Count++}}; $seen.GetEnumerator()|Sort-Object Name|ForEach-Object{"$($_.Name): count=$($_.Value.Count) font=$($_.Value.Font) before=$($_.Value.Before) after=$($_.Value.After) line=$($_.Value.Line)"};}finally{if($d -ne $null){$d.Close($false)};if($word -ne $null){$word.Quit()}}`
> - Ran `$doc=(Resolve-Path 'tmp\docs_v2\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx').Path; $word=$null;$d=$null;try{$word=New-Object -ComObject Word.Application;$word.Visible=$false;$word.DisplayAlerts=0;$d=$word.Documents.Open($doc,$false,$true);foreach($s in $d.Styles){if([string]$s.NameLocal -match 'TOC|목차'){"style=$($s.NameLocal) font=$($s.Font.Size) after=$($s.ParagraphFormat.SpaceAfter) line=$($s.ParagraphFormat.LineSpacing) built=$($s.BuiltIn)"}}}finally{if($d){$d.Close($false)};if($word){$word.Quit()}}`
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; & $py -c "import zipfile,re; p=r'tmp/docs_v2/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'; x=zipfile.ZipFile(p).read('word/styles.xml').decode('utf-8'); print([(m.group(1),m.group(2)) for m in re.finditer(r'<w:style[^>]+w:styleId=\"([^\"]*TOC[^\"]*)\"[\s\S]*?<w:name w:val=\"([^\"]+)\"',x,re.I)])"`
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> import zipfile
> from lxml import etree
> p=r'tmp/docs_v2/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'
> root=etree.fromstring(zipfile.ZipFile(p).read('word/styles.xml'))
> ns={'w':'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
> for s in root.xpath('./w:style',namespaces=ns):
>     sid=s.get('{%s}styleId'%ns['w'])
>     n=s.find('w:name',ns)
>     name=n.get('{%s}val'%ns['w']) if n is not None else ''
>     if 'toc' in sid.lower() or 'toc' in name.lower(): print(sid,name)
> '@ | & $py -`
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> import zipfile
> from lxml import etree
> p=r'tmp/docs_v2/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'
> root=etree.fromstring(zipfile.ZipFile(p).read('word/styles.xml'))
> ns={'w':'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
> for sid in ['TOC1','TOC2']:
>  s=root.xpath(f'./w:style[@w:styleId="{sid}"]',namespaces=ns)[0]
>  print(etree.tostring(s,encoding='unicode'))
> '@ | & $py -`
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> import zipfile
> from lxml import etree
> p=r'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx'
> root=etree.fromstring(zipfile.ZipFile(p).read('word/styles.xml'))
> ns={'w':'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
> for sid in ['TOC1','TOC2']:
>  s=root.xpath(f'./w:style[@w:styleId="{sid}"]',namespaces=ns)
>  print(sid, etree.tostring(s[0],encoding='unicode') if s else 'NONE')
> '@ | & $py -`
> - Wrote `./LMC_Library/LMC_API/Generate-ApiUserManualDocx.py` (+17 -0)
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; New-Item -ItemType Directory -Force -Path tmp\docs_v3 | Out-Null; & $py LMC_Library\LMC_API\Generate-ApiUserManualDocx.py --source LMC_Library\LMC_API\API_USER_MANUAL_KO.md --output tmp\docs_v3\manual.docx; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; $doc=(Resolve-Path 'tmp\docs_v3\manual.docx').Path; $out=(Resolve-Path 'tmp\docs_v3').Path+'\rendered.pdf'; $word=$null;$d=$null;try{$word=New-Object -ComObject Word.Application;$word.Visible=$false;$word.DisplayAlerts=0;$d=$word.Documents.Open($doc,$false,$true);if($d.TablesOfContents.Count -gt 0){$d.TablesOfContents.Item(1).Update()};foreach($s in $d.Styles){if([string]$s.NameLocal -match '^목차 [12]$|^TOC [12]$'){"style=$($s.NameLocal) font=$($s.Font.Size) after=$($s.ParagraphFormat.SpaceAfter) line=$($s.ParagraphFormat.LineSpacing)"}};$d.ExportAsFixedFormat($out,17);$d.Close($false);$d=$null}finally{if($d){$d.Close($false)};if($word){$word.Quit()}};& $py -c "from pypdf import PdfReader;print('pages',len(PdfReader(r'tmp/docs_v3/rendered.pdf').pages))"`
> - Ran `$exe='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin\pdftoppm.exe';New-Item -ItemType Directory -Force -Path tmp\docs_v3\pages|Out-Null;& $exe -f 4 -l 6 -png -r 120 (Resolve-Path 'tmp\docs_v3\rendered.pdf').Path ((Resolve-Path 'tmp\docs_v3\pages').Path+'\page');exit $LASTEXITCODE`
>
> </details>
>
> Viewed 3 images
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran `rg -n "25|command matrix|matrix|fault|soak|safe[- ]?stop|unsigned|서명|DLL|production|preview|RPC_INIT_FRESH_TCP_ONCE_V1|3/28|13,2,13|100 ms|ErrorId=-1|fresh TCP|default mutex|SC_CLOSE|full Distribution|standalone" LMC_Library\LMC_API\API_USER_MANUAL_KO.md | Select-Object -First 260`
> - Ran `$p='LMC_Library\LMC_API\API_USER_MANUAL_KO.md'; $l=Get-Content -LiteralPath $p; 25..145 | % { '{0}:{1}' -f $_,$l[$_-1] }; 1660..1705 | % { '{0}:{1}' -f $_,$l[$_-1] }`
> - Ran `$p='docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md';$l=Get-Content -LiteralPath $p;960..1005|%{'{0}:{1}'-f $_,$l[$_-1]};1080..1105|%{'{0}:{1}'-f $_,$l[$_-1]}`
> - Ran `$p='LMC_Library\LMC_API\Build-LmcApiDistribution.ps1';$l=Get-Content -LiteralPath $p;1..95|%{'{0}:{1}'-f $_,$l[$_-1]};$p2='LMC_Library\LMC_API\DistributionPipeline.ps1';rg -n "Get-LmcApiDistributionManualInputs|PdfPath|DocxPath|Manual" $p2; $l2=Get-Content -LiteralPath $p2;1..180|%{if($_ -le $l2.Count){'{0}:{1}'-f $_,$l2[$_-1]}}`
> - Ran `$p='LMC_Library\LMC_API\README.md';$l=Get-Content -LiteralPath $p;65..90|%{'{0}:{1}'-f $_,$l[$_-1]}`
>
> </details>
>
> <details><summary>Edited a file, ran 2 commands</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+6 -0)
> - Ran `git diff -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1 LMC_Library/LMC_API/Generate-ApiUserManualDocx.py LMC_Library/LMC_API/API_USER_MANUAL_KO.md; git diff --check -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1 LMC_Library/LMC_API/Generate-ApiUserManualDocx.py LMC_Library/LMC_API/API_USER_MANUAL_KO.md`
> - Ran `rg -n "Generate-ApiUserManual|2\.3-candidate|ManualDocxPath|ManualPdfPath" LMC_Library\LMC_API\README.md LMC_Library\LMC_API\API_DEVELOPMENT_GUIDE.md docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`
>
> </details>
>
> 진행 상황은 두 갈래입니다.
>
> - 구조 회귀: 이미 분리된 `PublishAxisOwnership`가 다시 32 KB를 넘으면 이제 verifier가 즉시 거부하도록 수정했고, PS5/PS7에서 `8/8` self-test와 current `101/98/3` inventory가 통과했습니다.
> - 배포 문서: 2.3 후보 DOCX/PDF 생성 경로를 검증 중입니다. Word 목차가 마지막 항목 하나만 3페이지에 남기는 레이아웃 결함도 발견해, built-in TOC 스타일을 고정하여 2페이지로 정리했습니다.
>
> 동시에 semantic policy와 배포 builder의 actual-EXE gate 회귀테스트 공백을 별도 구현 중입니다.
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; & $py -c "from pypdf import PdfReader; t='\n'.join((p.extract_text() or '') for p in PdfReader(r'tmp/docs_v3/rendered.pdf').pages); print('unsigned', 'AuthentiCode' in t, '25-command', '25-command' in t, 'safe-stop', 'safe-stop' in t)"`
