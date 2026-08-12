>     res=res.get_object()
>     fr=res.get('/Font')
>     if not fr: continue
>     fr=fr.get_object()
>     for key,ref in fr.items():
>         f=ref.get_object()
>         name=str(f.get('/BaseFont',''))
>         desc=f.get('/FontDescriptor')
>         embedded=False
>         if desc:
>             d=desc.get_object()
>             embedded=any(d.get(k) is not None for k in ['/FontFile','/FontFile2','/FontFile3'])
>         subset=bool(re.match(r'^/[A-Z]{6}\+',name))
>         fonts[(name,embedded,subset)]=fonts.get((name,embedded,subset),0)+1
> print(f'PDF_UNIQUE_FONTS={len(fonts)}')
> print(f'PDF_ALL_EMBEDDED={all(e for _,e,_ in fonts)}')
> print(f'PDF_ALL_SUBSET={all(s for _,_,s in fonts)}')
> for (n,e,s),count in sorted(fonts.items()): print(f'{n}|embedded={e}|subset={s}|uses={count}')
> '@ | & $python - 'C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'`
> - Ran `$python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> from pypdf import PdfReader
> import sys,re
> r=PdfReader(sys.argv[1]); fonts={}
> def embedded(font):
>     f=font.get_object()
>     desc=f.get('/FontDescriptor')
>     if desc:
>         d=desc.get_object()
>         if any(d.get(k) is not None for k in ('/FontFile','/FontFile2','/FontFile3')): return True
>     descendants=f.get('/DescendantFonts')
>     if descendants:
>         return all(embedded(x) for x in descendants)
>     return False
> for page in r.pages:
>     res=page.get('/Resources')
>     if not res: continue
>     fr=res.get_object().get('/Font')
>     if not fr: continue
>     for _,ref in fr.get_object().items():
>         f=ref.get_object(); name=str(f.get('/BaseFont','')); emb=embedded(ref); subset=bool(re.match(r'^/[A-Z]{6}\+',name)); fonts[(name,emb,subset)]=fonts.get((name,emb,subset),0)+1
> print(f'PDF_UNIQUE_FONT_RECORDS={len(fonts)}')
> print(f'PDF_ALL_EMBEDDED={all(e for _,e,_ in fonts)}')
> print(f'PDF_ALL_SUBSET={all(s for _,_,s in fonts)}')
> for (n,e,s),count in sorted(fonts.items()): print(f'{n}|embedded={e}|subset={s}|uses={count}')
> '@ | & $python - 'C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'`
> - Ran `Get-Content -LiteralPath 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcDistributionSemanticPolicy.ps1' -TotalCount 80; rg -n "param\(|ManualDocx|DocxText|Test-LmcDistributionManualReleasePolicy|Extract" 'LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1' 'LMC_Library/LMC_API/DistributionSemanticPolicy.ps1' | Select-Object -First 120`
> - Ran `$p='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\DistributionSemanticPolicy.ps1'; $lines=Get-Content -LiteralPath $p; for($i=110;$i -le 230;$i++){ '{0,5}: {1}' -f ($i+1),$lines[$i] }`
> - Ran `$ErrorActionPreference='Stop'
> . 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\DistributionSemanticPolicy.ps1'
> $python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
> $docx='C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'
> $pdf='C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'
> $docxText=Get-LmcDistributionPolicyDocumentText -Path $docx -PythonPath $python
> $pdfText=Get-LmcDistributionPolicyDocumentText -Path $pdf -PythonPath $python
> $result=Test-LmcDistributionManualReleasePolicy -DocxText $docxText -PdfText $pdfText
> $result | Format-List *`
> - Ran `$ErrorActionPreference='Stop'
> $docxSource='C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'
> $pdfSource='C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'
> $docxOutput='C:\work\Elmo\Elmo_Master\output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'
> $pdfOutput='C:\work\Elmo\Elmo_Master\output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'
> $docxCanonical='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'
> $pdfCanonical='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> Copy-Item -LiteralPath $docxSource -Destination $docxOutput -Force
> Copy-Item -LiteralPath $pdfSource -Destination $pdfOutput -Force
> Copy-Item -LiteralPath $docxSource -Destination $docxCanonical -Force
> Copy-Item -LiteralPath $pdfSource -Destination $pdfCanonical -Force
> foreach($p in @($docxSource,$docxOutput,$docxCanonical,$pdfSource,$pdfOutput,$pdfCanonical)){
>   $i=Get-Item -LiteralPath $p
>   $h=(Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash
>   '{0}|{1}|{2}' -f $i.FullName,$i.Length,$h
> }`
> - Ran `$ps5=Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe'; $c=@(Get-Command pwsh.exe -CommandType Application -All -ErrorAction Stop | ForEach-Object {$_.Source} | Select-Object -Unique); $valid=@($c | Where-Object {Test-Path -LiteralPath $_ -PathType Leaf} | Where-Object {(Get-Item -LiteralPath $_).Length -gt 0} | Where-Object {((Get-Item -LiteralPath $_).Attributes -band [IO.FileAttributes]::ReparsePoint) -eq 0}); "PS5=$ps5"; $valid | ForEach-Object {"PS7=$_"}`
>
> </details>
>
> 새 DOCX/PDF는 Word 기준 43쪽, Office 2016 OpenXML 오류 0, 제목·66개 heading·109개 표, manual policy 3/3을 통과했습니다. 이제 같은 현재 바이트에서 PS5.1과 PS7의 Pipeline/Semantic/Manifest 집중 검증을 병렬 실행합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> & 'C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcApiDistributionPipeline.ps1'
> if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
> & 'C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcDistributionSemanticPolicy.ps1'
> if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
> & 'C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcReleaseManifest.ps1'
> if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
> 'HOST_PS5_FOCUSED_PASS'`
> - Ran `$ErrorActionPreference='Stop'
> & 'C:\Program Files\WindowsApps\Microsoft.PowerShell_7.6.4.0_x64__8wekyb3d8bbwe\pwsh.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcApiDistributionPipeline.ps1'
> if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
> & 'C:\Program Files\WindowsApps\Microsoft.PowerShell_7.6.4.0_x64__8wekyb3d8bbwe\pwsh.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcDistributionSemanticPolicy.ps1'
> if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
> & 'C:\Program Files\WindowsApps\Microsoft.PowerShell_7.6.4.0_x64__8wekyb3d8bbwe\pwsh.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcReleaseManifest.ps1'
> if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
> 'HOST_PS7_FOCUSED_PASS'`
>
> </details>
>
> 양 호스트 집중 검증은 모두 통과했습니다: Pipeline 291/291, Semantic 52/52·policy 18/18, Manifest 108/108. 배포 코드 자체 문제는 아니고 canonical 예제 README 한 파일이 오래된 상태라는 독립 감사도 확인됐습니다. 매뉴얼 승격 커밋을 먼저 분리한 뒤 README 2개와 정책 회귀 테스트를 별도 변경으로 처리하겠습니다.
>
> <details><summary>Edited a file, ran 5 commands</summary>
>
> - Ran `git diff --check -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'; git diff -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md'; git status --short -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'`
> - Ran `$p='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\API_USER_MANUAL_KO.md'; $lines=Get-Content -LiteralPath $p; for($i=0;$i -le 34;$i++){ '{0,5}: {1}' -f ($i+1),$lines[$i] }`
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+8 -6)
> - Ran `$ErrorActionPreference='Stop'
> $python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
> $root='C:\work\Elmo\Elmo_Master'
> $outDir=Join-Path $root 'tmp\docs\manual_promotion_20260812'
> $docx=Join-Path $outDir 'LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'
> $pdf=Join-Path $outDir 'LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'
> & $python (Join-Path $root 'LMC_Library\LMC_API\Generate-ApiUserManualDocx.py') --source (Join-Path $root 'LMC_Library\LMC_API\API_USER_MANUAL_KO.md') --output $docx
> if($LASTEXITCODE -ne 0){throw "DOCX generator failed: $LASTEXITCODE"}
> $word=$null;$document=$null
> try{$word=New-Object -ComObject Word.Application;$word.Visible=$false;$word.DisplayAlerts=0;$document=$word.Documents.Open($docx,$false,$false);foreach($toc in @($document.TablesOfContents)){$toc.Update()|Out-Null};$document.Fields.Update()|Out-Null;$document.Save();$pages=$document.ComputeStatistics(2);$document.ExportAsFixedFormat($pdf,17);"WORD_PAGES=$pages"}
> finally{if($null-ne$document){$document.Close(0)};if($null-ne$word){$word.Quit()};if($null-ne$document){[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($document)};if($null-ne$word){[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($word)};[GC]::Collect();[GC]::WaitForPendingFinalizers()}
> foreach($p in @($docx,$pdf)){$i=Get-Item -LiteralPath $p;$h=(Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash;'{0}|{1}|{2}' -f $i.FullName,$i.Length,$h}`
> - Ran `$ErrorActionPreference='Stop'
> $docx='C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'
> $pdf='C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'
> Add-Type -Path 'C:\Users\dreiv\.nuget\packages\documentformat.openxml\2.16.0\lib\net46\DocumentFormat.OpenXml.dll'
> $wordDoc=[DocumentFormat.OpenXml.Packaging.WordprocessingDocument]::Open($docx,$false)
> try{$validator=[DocumentFormat.OpenXml.Validation.OpenXmlValidator]::new([DocumentFormat.OpenXml.FileFormatVersions]::Office2016);$openXmlErrors=@($validator.Validate($wordDoc));"OPENXML_OFFICE2016_ERRORS=$($openXmlErrors.Count)";if($openXmlErrors.Count-ne0){$openXmlErrors|Select-Object -First 10|ForEach-Object{$_.Description};throw 'OpenXML validation failed'}}finally{$wordDoc.Dispose()}
> $python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
> @'
> from docx import Document
> from pypdf import PdfReader
> import sys
> D=Document(sys.argv[1]); R=PdfReader(sys.argv[2])
> all_docx=[p.text for p in D.paragraphs]
> for table in D.tables:
>   for row in table.rows:
>     for cell in row.cells:
>       all_docx.extend(p.text for p in cell.paragraphs)
> docx_text='\n'.join(all_docx)
> pdf_text='\n'.join((p.extract_text() or '') for p in R.pages)
> print(f'DOCX_HEADINGS={sum(1 for p in D.paragraphs if p.style and p.style.name.startswith("Heading "))}')
> print(f'DOCX_TABLES={len(D.tables)}')
> print(f'DOCX_TITLE={D.core_properties.title}')
> print(f'PDF_PAGES={len(R.pages)}')
> print(f'DOCX_CURRENT_STATUS={"TerminalWakeBrokerCandidate" in docx_text and "13-role active Python dependency closure" in docx_text and "tracked release-input baseline" in docx_text}')
> print(f'PDF_CURRENT_STATUS={"TerminalWakeBrokerCandidate" in pdf_text and "13-role active Python dependency closure" in pdf_text and "tracked release-input" in pdf_text}')
> print(f'STALE_1_9_COUNT={docx_text.count("1.9 gate-off snapshot")+pdf_text.count("1.9 gate-off snapshot")}')
> '@ | & $python - $docx $pdf
> if($LASTEXITCODE-ne0){throw 'structure validation failed'}
> . 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\DistributionSemanticPolicy.ps1'
> $docxText=Get-LmcDistributionPolicyDocumentText -Path $docx -PythonPath $python
> $pdfText=Get-LmcDistributionPolicyDocumentText -Path $pdf -PythonPath $python
> Test-LmcDistributionManualReleasePolicy -DocxText $docxText -PdfText $pdfText | Format-List *`
> - Ran `$ErrorActionPreference='Stop'; $pdf='C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'; $renderDir='C:\work\Elmo\Elmo_Master\tmp\pdfs\manual_promotion_20260812'; & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin\pdftoppm.exe' -png -r 120 $pdf (Join-Path $renderDir 'page'); if($LASTEXITCODE-ne0){throw 'pdftoppm failed'}; "RENDERED_PAGES=$(@(Get-ChildItem -LiteralPath $renderDir -Filter 'page-*.png').Count)"`
>
> </details>
>
> Viewed 4 images
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $docxSource='C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'
> $pdfSource='C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'
> $targets=@(
>   @($docxSource,'C:\work\Elmo\Elmo_Master\output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'),
>   @($pdfSource,'C:\work\Elmo\Elmo_Master\output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'),
>   @($docxSource,'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'),
>   @($pdfSource,'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> )
> foreach($pair in $targets){Copy-Item -LiteralPath $pair[0] -Destination $pair[1] -Force}
> foreach($p in @($targets|ForEach-Object{$_[1]})){$i=Get-Item -LiteralPath $p;$h=(Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash;'{0}|{1}|{2}' -f $i.FullName,$i.Length,$h}`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> . 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\DistributionSemanticPolicy.ps1'
> $python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
> $docx='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'
> $pdf='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> $d=Get-LmcDistributionPolicyDocumentText -Path $docx -PythonPath $python
> $p=Get-LmcDistributionPolicyDocumentText -Path $pdf -PythonPath $python
> $r=Test-LmcDistributionManualReleasePolicy -DocxText $d -PdfText $p
> 'ACTUAL_MANUAL_POLICY={0}/{1}' -f $r.Result,$r.CheckCount`
> - Ran `$ErrorActionPreference='Stop'
> & 'C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcApiDistributionPipeline.ps1'
> if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
> & 'C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcDistributionSemanticPolicy.ps1'
> if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
> & 'C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcReleaseManifest.ps1'
> if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
> 'PS5_FINAL_FOCUSED_PASS'`
> - Ran `$ErrorActionPreference='Stop'
> & 'C:\Program Files\WindowsApps\Microsoft.PowerShell_7.6.4.0_x64__8wekyb3d8bbwe\pwsh.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcApiDistributionPipeline.ps1'
> if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
> & 'C:\Program Files\WindowsApps\Microsoft.PowerShell_7.6.4.0_x64__8wekyb3d8bbwe\pwsh.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcDistributionSemanticPolicy.ps1'
> if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
> & 'C:\Program Files\WindowsApps\Microsoft.PowerShell_7.6.4.0_x64__8wekyb3d8bbwe\pwsh.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Test-LmcReleaseManifest.ps1'
> if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
> 'PS7_FINAL_FOCUSED_PASS'`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $files=@(
> 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md',
> 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx',
> 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'
> )
> git diff --check -- $files
> if($LASTEXITCODE-ne0){throw 'scoped diff check failed'}
> git add -- $files
> if($LASTEXITCODE-ne0){throw 'git add failed'}
> $staged=@(git diff --cached --name-only)
> '=== STAGED ==='
> $staged
> '=== STAGED CHECK ==='
> git diff --cached --check
> if($LASTEXITCODE-ne0){throw 'cached diff check failed'}
> '=== STAGED STAT ==='
> git diff --cached --stat`
>
> </details>
>
> 매뉴얼 최종 바이트는 독립 검토에서도 현재까지 P0/P1 없이 통과했습니다. 지금 43쪽 전체 마지막 시각 스윕과 staged 3파일 재확인만 남았고, 동시에 canonical README 회귀 보강이 별도 파일 범위에서 진행 중입니다.
>
> 매뉴얼 승격 최종 독립 검토는 CLEAN입니다. DOCX/PDF 43쪽 전체, OpenXML, embedded font, 정책 3/3, staged exact 3파일까지 확인됐습니다. 이제 원본 Markdown과 canonical DOCX/PDF만 전용 커밋하고 clean detached 상태를 다시 검증합니다.
>
> <details><summary>Ran 7 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $expected=@(
> 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md',
> 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx',
> 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'
> )
> $actual=@(git diff --cached --name-only)
> if($actual.Count-ne$expected.Count){throw "staged count drift: $($actual.Count)"}
> for($i=0;$i-lt$expected.Count;$i++){if($actual[$i]-cne$expected[$i]){throw "staged path drift at $i: $($actual[$i])"}}
> git diff --cached --check
> if($LASTEXITCODE-ne0){throw 'cached diff check failed'}
> git commit -m 'docs(api): promote reviewed 2.3 manuals'
> if($LASTEXITCODE-ne0){throw "git commit failed: $LASTEXITCODE"}
> git show --stat --oneline --summary HEAD
> git status --short -- $expected`
> - Ran `$ErrorActionPreference='Stop'
> $expected=@(
> 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md',
> 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx',
