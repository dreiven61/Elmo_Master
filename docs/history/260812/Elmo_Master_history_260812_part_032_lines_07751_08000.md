> Copy-Item -LiteralPath $srcPdf -Destination $dstPdf -Force
> Get-FileHash -Algorithm SHA256 -LiteralPath $dstDoc,$dstPdf | ForEach-Object { $item=Get-Item -LiteralPath $_.Path; '{0}|{1}|{2}' -f $_.Hash,$item.Length,$_.Path }
> git status --short -- LMC_Library/LMC_API_Distribution/03_API_User_Manual`
>
> </details>
>
> canonical manual 두 파일을 검토본과 byte-identical하게 교체했습니다. DOCX `A232…3182`/93,238B, PDF `9E82…8E70`/1,013,620B입니다. 변경은 Git에 추적돼 기존 1.9로 복구 가능하며, 다른 파일은 이 단계에서 바꾸지 않았습니다. 이제 실제 canonical 경로로 semantic·구조·렌더 검증을 실행합니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `$repo=(Resolve-Path '.').Path; $candidate=(Resolve-Path 'LMC_Library\LMC_API_Distribution').Path; $py=Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; . .\LMC_Library\LMC_API\DistributionSemanticPolicy.ps1; $result=Test-LmcDistributionSemanticPolicy -RepositoryRoot $repo -CandidateRoot $candidate -PythonPath $py; $result | ConvertTo-Json -Depth 6; & $py -B -c "from docx import Document; from pypdf import PdfReader; import sys,json; d=Document(sys.argv[1]); r=PdfReader(sys.argv[2]); print(json.dumps({'pages':len(r.pages),'headings':sum(1 for p in d.paragraphs if p.style.name.startswith('Heading ')),'tables':len(d.tables),'title':d.core_properties.title},ensure_ascii=False))" (Join-Path $candidate '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx') (Join-Path $candidate '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')`
> - Ran `rg -n "function Test-LmcDistributionManualReleasePolicy|function Get-LmcDistributionPolicyDocumentText" LMC_Library/LMC_API/DistributionSemanticPolicy.ps1; Get-Content LMC_Library/LMC_API/DistributionSemanticPolicy.ps1 | Select-Object -Skip 280 -First 125`
> - Ran `$candidate=(Resolve-Path 'LMC_Library\LMC_API_Distribution').Path; $py=Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; $docx=Join-Path $candidate '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'; $pdf=Join-Path $candidate '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'; . .\LMC_Library\LMC_API\DistributionSemanticPolicy.ps1; $docxText=Get-LmcDistributionPolicyDocumentText -Path $docx -PythonPath $py; $pdfText=Get-LmcDistributionPolicyDocumentText -Path $pdf -PythonPath $py; $policy=Test-LmcDistributionManualReleasePolicy -DocxText $docxText -PdfText $pdfText; $policy | ConvertTo-Json -Depth 4; & $py -B -c "from docx import Document; from pypdf import PdfReader; import sys,json; d=Document(sys.argv[1]); r=PdfReader(sys.argv[2]); print(json.dumps({'pages':len(r.pages),'headings':sum(1 for p in d.paragraphs if p.style.name.startswith('Heading ')),'tables':len(d.tables),'title':d.core_properties.title},ensure_ascii=False))" $docx $pdf`
>
> </details>
>
> manual 자체 정책은 실제 canonical 경로에서 `3/3 PASS`, 구조는 PDF 43쪽·DOCX heading 66·table 109·제목 일치로 통과했습니다. 전체 semantic gate의 `PREVIEW_PRODUCTION_NO_GO`는 manual이 아니라 canonical Distribution README가 아직 preview 경계를 못 쓴 별도 후속 문제입니다. 지금 43쪽을 렌더해 시각 상태를 확인합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$out='tmp\pdfs\manual_promotion_3c63dea'; New-Item -ItemType Directory -Force -Path $out | Out-Null; $pdf=(Resolve-Path 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf').Path; & pdftoppm.cmd -r 120 -png $pdf (Join-Path (Resolve-Path $out).Path 'page') | Out-Null; $py=Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; & $py -B -c "from PIL import Image,ImageDraw; from pathlib import Path; import math,sys; root=Path(sys.argv[1]); pages=sorted(root.glob('page-*.png'),key=lambda p:int(p.stem.split('-')[-1])); assert len(pages)==43,len(pages); tw,th=300,424; cols,rows=3,2; margin=24; font=ImageDraw.Draw(Image.new('RGB',(1,1))).getfont();
> for si in range(math.ceil(len(pages)/(cols*rows))):
>  subset=pages[si*cols*rows:(si+1)*cols*rows]; sheet=Image.new('RGB',(cols*(tw+margin)+margin,rows*(th+52)+margin),'white'); d=ImageDraw.Draw(sheet)
>  for i,p in enumerate(subset):
>   im=Image.open(p).convert('RGB'); im.thumbnail((tw,th)); x=margin+(i%cols)*(tw+margin); y=margin+(i//cols)*(th+52); sheet.paste(im,(x,y)); d.text((x,y+th+8),f'Page {int(p.stem.split(chr(45))[-1])}',fill='black',font=font)
>  sheet.save(root/f'contact-{si+1:02d}.png')
> print(f'pages={len(pages)} contacts={math.ceil(len(pages)/6)}')" (Resolve-Path $out).Path; Get-ChildItem $out -Filter 'contact-*.png' | Select-Object Name,Length`
>
> </details>
>
> 렌더 명령의 Poppler wrapper가 내부 경로를 찾지 못해 페이지가 생성되지 않았습니다. PDF/파일 문제는 아니고 실행 wrapper 문제입니다. 실제 Poppler 실행파일 경로를 확인해 같은 43쪽 렌더를 다시 수행합니다.
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran `$cmd=(Get-Command pdftoppm.cmd).Source; $cmd; Get-Content -Raw $cmd; Get-ChildItem -Path (Split-Path -Parent (Split-Path -Parent $cmd)) -Recurse -Filter pdftoppm.exe -ErrorAction SilentlyContinue | Select-Object -First 10 FullName`
> - Ran `$base='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies'; Get-ChildItem -LiteralPath (Join-Path $base 'native\poppler') -Force -Recurse -ErrorAction SilentlyContinue | Select-Object -First 40 FullName; Get-ChildItem -Path $base -Recurse -Filter 'pdftoppm*' -ErrorAction SilentlyContinue | Select-Object FullName`
> - Ran `Get-Content -Raw 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\bin\pdftoppm.cmd'`
> - Ran `$exe='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin\pdftoppm.exe'; & $exe -v`
> - Ran `$out=(Resolve-Path 'tmp\pdfs\manual_promotion_3c63dea').Path; $pdf=(Resolve-Path 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf').Path; $exe='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin\pdftoppm.exe'; & $exe -r 120 -png $pdf (Join-Path $out 'page'); if($LASTEXITCODE -ne 0){throw "pdftoppm failed $LASTEXITCODE"}; $py=Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; & $py -B -c "from PIL import Image,ImageDraw; from pathlib import Path; import math,sys; root=Path(sys.argv[1]); pages=sorted(root.glob('page-*.png'),key=lambda p:int(p.stem.split('-')[-1])); assert len(pages)==43,len(pages); tw,th=300,424; cols,rows=3,2; margin=24; font=ImageDraw.Draw(Image.new('RGB',(1,1))).getfont();
> for si in range(math.ceil(len(pages)/(cols*rows))):
>  subset=pages[si*cols*rows:(si+1)*cols*rows]; sheet=Image.new('RGB',(cols*(tw+margin)+margin,rows*(th+52)+margin),'white'); d=ImageDraw.Draw(sheet)
>  for i,p in enumerate(subset):
>   im=Image.open(p).convert('RGB'); im.thumbnail((tw,th)); x=margin+(i%cols)*(tw+margin); y=margin+(i//cols)*(th+52); sheet.paste(im,(x,y)); d.text((x,y+th+8),f'Page {int(p.stem.split(chr(45))[-1])}',fill='black',font=font)
>  sheet.save(root/f'contact-{si+1:02d}.png')
> print(f'pages={len(pages)} contacts={math.ceil(len(pages)/6)}')" $out; Get-ChildItem $out -Filter 'contact-*.png' | Select-Object Name,Length`
>
> </details>
>
> canonical PDF 43쪽을 120dpi로 렌더했고 8개 contact sheet를 만들었습니다. 이제 전 페이지에서 잘림·겹침·빈 페이지·폰트 깨짐을 확인하고, 이상이 없으면 임시 렌더를 정리합니다.
>
> Viewed 8 images
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 검토된 2.3 매뉴얼 두 파일은 이미 canonical 경로에 정확한 바이트로 반영됐습니다. 지금 43쪽 렌더를 나눠 최종 육안 점검하고, 양 PowerShell 호스트 집중 검증을 거친 뒤 매뉴얼 2파일만 별도 커밋하겠습니다. 그 다음 방금 드러난 배포 README 정책 불일치와 Gate D를 순서대로 닫겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-ChildItem -LiteralPath 'C:\work\Elmo\Elmo_Master\tmp\pdfs\manual_promotion_3c63dea' -Filter 'contact-*.png' | Sort-Object Name | Select-Object Name,Length,FullName | Format-Table -AutoSize`
>
> </details>
>
> Viewed 2 images
>
> Viewed 2 images
>
> Viewed 2 images
>
> Viewed 2 images
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-ChildItem -LiteralPath 'C:\work\Elmo\Elmo_Master\tmp\pdfs\manual_promotion_3c63dea' -Filter 'page-*.png' | Sort-Object Name | Select-Object -First 5 Name,Length; Get-ChildItem -LiteralPath 'C:\work\Elmo\Elmo_Master\tmp\pdfs\manual_promotion_3c63dea' -Filter 'page-*.png' | Sort-Object Name | Select-Object -Last 3 Name,Length`
>
> </details>
>
> Viewed 3 images
>
> 전체 43쪽은 잘림·겹침·빈 페이지 없이 렌더됐지만, 최종 확대 검사에서 승격을 막는 문구 1건을 찾았습니다. 3쪽이 아직 “canonical 매뉴얼은 1.9”라고 적고 있어, 이 2.3 파일을 canonical로 복사하면 자기모순이 됩니다. 현재 바이너리는 커밋하지 않고, 원본 Markdown의 상태 문구를 먼저 고친 뒤 DOCX/PDF를 다시 생성·검수하겠습니다.
>
> <details><summary>Edited a file, ran 5 commands</summary>
>
> - Ran `rg -n --hidden --glob '!bin/**' --glob '!obj/**' "canonical.*1\.9|1\.9.*canonical|출판 후보 상태|candidate 원본|production 배포 매뉴얼" .`
> - Ran `$p='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\API_USER_MANUAL_KO.md'; $lines=Get-Content -LiteralPath $p; for($i=35;$i -le 65;$i++){ '{0,5}: {1}' -f ($i+1),$lines[$i] }`
> - Ran `rg -n "API_USER_MANUAL_KO|LASAL_Motion_Control_API_User_Manual_KO_2\.3|2\.3-candidate" LMC_Library tools docs -g '*.py' -g '*.ps1' -g '*.md' | Select-Object -First 200`
> - Ran `git status --short -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf' 'output/doc/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx' 'output/pdf/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'; git diff --numstat -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md'`
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+7 -6)
> - Ran `$p='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\README.md'; $lines=Get-Content -LiteralPath $p; for($i=235;$i -le 285;$i++){ '{0,5}: {1}' -f ($i+1),$lines[$i] }`
>
> </details>
>
> 문구를 `2.3-candidate = tracked canonical release-input baseline`으로 바로잡았습니다. 이제 원본 Markdown에서 새 DOCX를 만들고 Word로 목차·필드를 갱신한 같은 파일에서 PDF를 내보내겠습니다. 이 단계는 문서 입력 승격일 뿐 production 승인이나 PLC 증거로 표기하지 않습니다.
>
> <details><summary>Ran 12 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
> $root='C:\work\Elmo\Elmo_Master'
> $outDir=Join-Path $root 'tmp\docs\manual_promotion_20260812'
> New-Item -ItemType Directory -Path $outDir -Force | Out-Null
> $docx=Join-Path $outDir 'LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'
> $pdf=Join-Path $outDir 'LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'
> & $python (Join-Path $root 'LMC_Library\LMC_API\Generate-ApiUserManualDocx.py') --source (Join-Path $root 'LMC_Library\LMC_API\API_USER_MANUAL_KO.md') --output $docx
> if($LASTEXITCODE -ne 0){ throw "DOCX generator failed: $LASTEXITCODE" }
> $word=$null
> $document=$null
> try {
>   $word=New-Object -ComObject Word.Application
>   $word.Visible=$false
>   $word.DisplayAlerts=0
>   $document=$word.Documents.Open($docx,$false,$false)
>   foreach($toc in @($document.TablesOfContents)){ $toc.Update() | Out-Null }
>   $document.Fields.Update() | Out-Null
>   $document.Save()
>   $pages=$document.ComputeStatistics(2)
>   $document.ExportAsFixedFormat($pdf,17)
>   [pscustomobject]@{WordPages=$pages;Docx=$docx;Pdf=$pdf}
> }
> finally {
>   if($null -ne $document){$document.Close(0)}
>   if($null -ne $word){$word.Quit()}
>   if($null -ne $document){[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($document)}
>   if($null -ne $word){[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($word)}
>   [GC]::Collect()
>   [GC]::WaitForPendingFinalizers()
> }
> Get-Item -LiteralPath $docx,$pdf | Select-Object FullName,Length
> Get-FileHash -Algorithm SHA256 -LiteralPath $docx,$pdf | Select-Object Path,Hash`
> - Ran `$paths=@('C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx','C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'); foreach($p in $paths){$i=Get-Item -LiteralPath $p;$h=(Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash; '{0}|{1}|{2}' -f $i.FullName,$i.Length,$h}`
> - Ran `rg -n "OpenXmlValidator|DocumentFormat\.OpenXml" . --glob '!docs/history/**' --glob '!bin/**' --glob '!obj/**' | Select-Object -First 100`
> - Ran `$candidates=@('C:\Users\dreiv\.nuget\packages\documentformat.openxml','C:\Users\dreiv\.cache\codex-runtimes'); foreach($c in $candidates){ if(Test-Path -LiteralPath $c){ Get-ChildItem -LiteralPath $c -Recurse -Filter 'DocumentFormat.OpenXml.dll' -ErrorAction SilentlyContinue | Select-Object -First 20 FullName } }`
> - Ran `$ErrorActionPreference='Stop'
> $docx='C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'
> $assembly='C:\Users\dreiv\.nuget\packages\documentformat.openxml\2.16.0\lib\net46\DocumentFormat.OpenXml.dll'
> Add-Type -Path $assembly
> $wordDoc=[DocumentFormat.OpenXml.Packaging.WordprocessingDocument]::Open($docx,$false)
> try {
>   $validator=[DocumentFormat.OpenXml.Validation.OpenXmlValidator]::new([DocumentFormat.OpenXml.FileFormatVersions]::Office2016)
>   $errors=@($validator.Validate($wordDoc))
>   "OPENXML_OFFICE2016_ERRORS=$($errors.Count)"
>   $errors | Select-Object -First 10 | ForEach-Object { $_.Description }
> }
> finally {$wordDoc.Dispose()}
> $python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
> @'
> from pathlib import Path
> from docx import Document
> from pypdf import PdfReader
> import sys
> p_docx=Path(sys.argv[1]); p_pdf=Path(sys.argv[2])
> doc=Document(p_docx)
> headings=sum(1 for p in doc.paragraphs if p.style and p.style.name.startswith('Heading '))
> text='\n'.join(p.text for p in doc.paragraphs)
> print(f'DOCX_PARAGRAPHS={len(doc.paragraphs)}')
> print(f'DOCX_HEADINGS={headings}')
> print(f'DOCX_TABLES={len(doc.tables)}')
> print(f'DOCX_TITLE={doc.core_properties.title}')
> print(f'DOCX_REPLACEMENT_CHARS={text.count(chr(0xfffd))}')
> print(f'STATUS_CANONICAL_BASELINE={"tracked release-input baseline" in text}')
> print(f'STATUS_STALE_1_9={"1.9 gate-off snapshot" in text}')
> reader=PdfReader(p_pdf)
> print(f'PDF_PAGES={len(reader.pages)}')
> print(f'PDF_ENCRYPTED={reader.is_encrypted}')
> pdftext='\n'.join((p.extract_text() or '') for p in reader.pages)
> print(f'PDF_REPLACEMENT_CHARS={pdftext.count(chr(0xfffd))}')
> print(f'PDF_STATUS_CANONICAL_BASELINE={"tracked release-input baseline" in pdftext}')
> print(f'PDF_STATUS_STALE_1_9={"1.9 gate-off snapshot" in pdftext}')
> '@ | & $python - $docx 'C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'
> if($LASTEXITCODE -ne 0){throw "Python validation failed: $LASTEXITCODE"}`
> - Ran `$python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> from docx import Document
> import sys
> p=sys.argv[1]
> for i,para in enumerate(Document(p).paragraphs):
>     t=para.text
>     if 'canonical' in t or 'release-input' in t or '승격' in t:
>         print(i,repr(t))
> '@ | & $python - 'C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'`
> - Ran `$python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> from docx import Document
> import sys
> D=Document(sys.argv[1])
> texts=[]
> texts.extend(p.text for p in D.paragraphs)
> for ti,t in enumerate(D.tables):
>   for ri,row in enumerate(t.rows):
>     for ci,cell in enumerate(row.cells):
>       for pi,p in enumerate(cell.paragraphs):
>         if 'canonical' in p.text or 'release-input' in p.text or '승격' in p.text or '1.9 gate-off' in p.text:
>           print(ti,ri,ci,pi,repr(p.text))
> '@ | & $python - 'C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx'`
> - Ran `$python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> from pypdf import PdfReader
> import sys
> text=PdfReader(sys.argv[1]).pages[2].extract_text() or ''
> for line in text.splitlines():
>     if 'canonical' in line or 'release-input' in line or '승격' in line or '1.9 gate-off' in line:
>         print(repr(line))
> '@ | & $python - 'C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'`
> - Ran `$ErrorActionPreference='Stop'
> $renderDir='C:\work\Elmo\Elmo_Master\tmp\pdfs\manual_promotion_20260812'
> New-Item -ItemType Directory -Path $renderDir -Force | Out-Null
> $pdf='C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'
> $pdftoppm='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin\pdftoppm.exe'
> & $pdftoppm -png -r 120 $pdf (Join-Path $renderDir 'page')
> if($LASTEXITCODE -ne 0){throw "pdftoppm failed: $LASTEXITCODE"}
> $pages=@(Get-ChildItem -LiteralPath $renderDir -Filter 'page-*.png' | Sort-Object Name)
> "RENDERED_PAGES=$($pages.Count)"
> $pdffonts='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin\pdffonts.exe'
> & $pdffonts $pdf`
> - Ran `Get-ChildItem -LiteralPath 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler' -Recurse -Filter 'pdffonts*' -ErrorAction SilentlyContinue | Select-Object FullName`
> - Ran `$old='C:\work\Elmo\Elmo_Master\tmp\pdfs\manual_promotion_3c63dea'; $new='C:\work\Elmo\Elmo_Master\tmp\pdfs\manual_promotion_20260812'; $rows=@(); foreach($n in 1..43){$name=('page-{0:00}.png' -f $n); $op=Join-Path $old $name; $np=Join-Path $new $name; $oh=(Get-FileHash -Algorithm SHA256 -LiteralPath $op).Hash; $nh=(Get-FileHash -Algorithm SHA256 -LiteralPath $np).Hash; if($oh -ne $nh){$rows += [pscustomobject]@{Page=$n;OldLength=(Get-Item -LiteralPath $op).Length;NewLength=(Get-Item -LiteralPath $np).Length;OldHash=$oh;NewHash=$nh}}}; "DIFFERING_RASTER_PAGES=$($rows.Count)"; $rows | Format-Table -AutoSize`
> - Ran `$python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> from pathlib import Path
> from PIL import Image, ImageChops
> import sys
> old=Path(sys.argv[1]); new=Path(sys.argv[2])
> for n in range(1,44):
>     a=Image.open(old/f'page-{n:02d}.png').convert('RGB')
>     b=Image.open(new/f'page-{n:02d}.png').convert('RGB')
>     if a.size != b.size:
>         print(n,'SIZE',a.size,b.size); continue
>     d=ImageChops.difference(a,b)
>     bbox=d.getbbox()
>     if bbox:
>         hist=d.histogram()
>         changed=sum(v for i,v in enumerate(hist) if i%256)
>         # number of pixels with any channel changed
>         mask=d.convert('L')
>         pix=sum(1 for v in mask.getdata() if v)
>         print(f'{n:02d}|pixels={pix}|ratio={pix/(a.width*a.height):.8f}|bbox={bbox}')
> '@ | & $python - 'C:\work\Elmo\Elmo_Master\tmp\pdfs\manual_promotion_3c63dea' 'C:\work\Elmo\Elmo_Master\tmp\pdfs\manual_promotion_20260812'`
>
> </details>
>
> Viewed 4 images
>
> <details><summary>Ran 7 commands</summary>
>
> - Ran `$python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; @'
> from pypdf import PdfReader
> import sys,re
> r=PdfReader(sys.argv[1])
> fonts={}
> for page in r.pages:
>     res=page.get('/Resources')
>     if not res: continue
