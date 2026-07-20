> New-Item -ItemType Directory -Force -Path $tmpDir | Out-Null
> if(Test-Path -LiteralPath $tmpPdf){ Remove-Item -LiteralPath $tmpPdf -Force }
> $docxInfo=Get-Item -LiteralPath $docx
> $docxHash=(Get-FileHash -LiteralPath $docx -Algorithm SHA256).Hash
> $word=$null
> $document=$null
> try {
>   $word=New-Object -ComObject Word.Application
>   $word.Visible=$false
>   $word.DisplayAlerts=0
>   $document=$word.Documents.Open($docx,$false,$true)
>   $document.ExportAsFixedFormat($tmpPdf,17)
> } finally {
>   if($null -ne $document){ $document.Close(0); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($document) }
>   if($null -ne $word){ $word.Quit(); [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($word) }
>   [GC]::Collect(); [GC]::WaitForPendingFinalizers()
> }
> if(-not (Test-Path -LiteralPath $tmpPdf)){ throw 'Word PDF export did not create a file.' }
> $tmpInfo=Get-Item -LiteralPath $tmpPdf
> if($tmpInfo.Length -lt 1024){ throw "Exported PDF is unexpectedly small: $($tmpInfo.Length) bytes" }
> Copy-Item -LiteralPath $tmpPdf -Destination $finalPdf -Force
> [pscustomobject]@{
>   SourceDocx=$docx
>   SourceModified=$docxInfo.LastWriteTime
>   SourceBytes=$docxInfo.Length
>   SourceSHA256=$docxHash
>   OutputPdf=$finalPdf
>   OutputBytes=(Get-Item -LiteralPath $finalPdf).Length
>   OutputSHA256=(Get-FileHash -LiteralPath $finalPdf -Algorithm SHA256).Hash
> } | Format-List`
>
> </details>
>
> PDF 변환은 완료됐습니다. 이제 페이지 수·문자 추출을 확인하고 모든 페이지를 이미지로 렌더링해 잘림, 겹침, 깨진 글자가 없는지 검사하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $pdf='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> $docx='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'
> $render='C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\pages'
> if(Test-Path -LiteralPath $render){ Remove-Item -LiteralPath $render -Recurse -Force }
> New-Item -ItemType Directory -Force -Path $render | Out-Null
> @'
> from pathlib import Path
> from pypdf import PdfReader
> from docx import Document
> pdf=Path(r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> docx=Path(r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx')
> r=PdfReader(str(pdf))
> text='\n'.join((p.extract_text() or '') for p in r.pages)
> d=Document(str(docx))
> doc_text='\n'.join(p.text for p in d.paragraphs)
> print('pages=',len(r.pages))
> print('pdf_chars=',len(text),'docx_paragraph_chars=',len(doc_text))
> print('replacement_char=', '\ufffd' in text)
> print('title_present=', 'LASAL' in text and 'Motion Control API' in text)
> print('api_tokens=', {x:(x in text) for x in ['LMCConnection','MoveAbsoluteExAsync','GroupPowerOnAsync','MoveLinearAbsoluteExAsync','UNIT']})
> print('encrypted=',r.is_encrypted)
> print('page_sizes=',sorted({(round(float(p.mediabox.width),2),round(float(p.mediabox.height),2)) for p in r.pages}))
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -
> $pdftoppm=(Get-Command pdftoppm.exe -ErrorAction SilentlyContinue).Source
> if(-not $pdftoppm){
>   $candidates=@('C:\Program Files\poppler\Library\bin\pdftoppm.exe','C:\Program Files\MiKTeX\miktex\bin\x64\pdftoppm.exe')
>   $pdftoppm=$candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
> }
> if(-not $pdftoppm){ throw 'pdftoppm.exe not found' }
> & $pdftoppm -png -r 144 $pdf (Join-Path $render 'page')
> Get-ChildItem -LiteralPath $render -Filter '*.png' | Sort-Object Name | Select-Object Name,Length | Format-Table -AutoSize`
>
> </details>
>
> Viewed 7 images
>
> Viewed 7 images
>
> Viewed 7 images
>
> 모든 페이지 본문은 정상적으로 렌더링됐습니다. 일부 짝수 페이지의 두 자리 페이지 번호가 화면상 한 자리처럼 보여 원본 렌더링 문제인지 확대 표시 문제인지 하단 영역을 추가 확인하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> @'
> from pathlib import Path
> from PIL import Image
> base=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\pages')
> out=base/'footer_crops'
> out.mkdir(exist_ok=True)
> for i in [10,12,14,16,18,20]:
>     p=base/f'page-{i:02d}.png'
>     im=Image.open(p)
>     w,h=im.size
>     crop=im.crop((int(w*0.62), int(h*0.91), w, h))
>     crop.save(out/f'footer-{i:02d}.png')
>     print(i, im.size, crop.size)
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -`
>
> </details>
>
> Viewed 6 images
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> @'
> from docx import Document
> from docx.shared import Inches
> p=r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'
> d=Document(p)
> print('sections',len(d.sections))
> for i,s in enumerate(d.sections):
>     def inch(v): return None if v is None else round(v.inches,3)
>     print(i, 'top',inch(s.top_margin),'bottom',inch(s.bottom_margin),'left',inch(s.left_margin),'right',inch(s.right_margin),'header',inch(s.header_distance),'footer',inch(s.footer_distance),'start',s.start_type)
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -`
> - Ran `$ErrorActionPreference='Stop'
> @'
> from pypdf import PdfReader
> p=r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> r=PdfReader(p)
> for n in [10,12,14,16,18,20]:
>     t=(r.pages[n-1].extract_text() or '').splitlines()
>     print('PAGE',n,'LAST',repr(t[-8:]))
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -`
>
> </details>
>
> 원본 DOCX의 본문 표가 하단 푸터 영역을 일부 덮어 쓰는 현상이 확인됐습니다. 원본은 그대로 두고, 임시 사본의 하단 여백만 소폭 늘려 페이지 번호와 푸터가 정상 보이도록 변환 가능한지 시험하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $src='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'
> $tmpDir='C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\margin_test'
> $tmpDocx=Join-Path $tmpDir 'manual_margin_test.docx'
> $tmpPdf=Join-Path $tmpDir 'manual_margin_test.pdf'
> if(Test-Path -LiteralPath $tmpDir){ Remove-Item -LiteralPath $tmpDir -Recurse -Force }
> New-Item -ItemType Directory -Force -Path $tmpDir | Out-Null
> @'
> from docx import Document
> from docx.shared import Inches
> src=r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'
> out=r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\margin_test\manual_margin_test.docx'
> d=Document(src)
> for s in d.sections:
>     s.bottom_margin=Inches(0.82)
> d.save(out)
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -
> $word=$null;$doc=$null
> try {
>   $word=New-Object -ComObject Word.Application
>   $word.Visible=$false;$word.DisplayAlerts=0
>   $doc=$word.Documents.Open($tmpDocx,$false,$false)
>   foreach($toc in $doc.TablesOfContents){ $toc.Update() | Out-Null }
>   $doc.ExportAsFixedFormat($tmpPdf,17)
>   $doc.Close(0)
>   [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($doc);$doc=$null
> } finally {
>   if($null -ne $doc){$doc.Close(0);[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($doc)}
>   if($null -ne $word){$word.Quit();[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($word)}
>   [GC]::Collect();[GC]::WaitForPendingFinalizers()
> }
> @'
> from pypdf import PdfReader
> p=r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\margin_test\manual_margin_test.pdf'
> r=PdfReader(p)
> print('pages',len(r.pages),'bytes',__import__('pathlib').Path(p).stat().st_size)
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -
> $pdftoppm=(Get-Command pdftoppm.exe).Source
> & $pdftoppm -png -r 144 $tmpPdf (Join-Path $tmpDir 'page')
> @'
> from pathlib import Path
> from PIL import Image
> base=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\margin_test')
> out=base/'footer_crops';out.mkdir()
> for p in sorted(base.glob('page-*.png')):
>     i=int(p.stem.split('-')[-1])
>     if i>=10:
>         im=Image.open(p);w,h=im.size
>         im.crop((int(w*.62),int(h*.91),w,h)).save(out/f'footer-{i:02d}.png')
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -`
>
> </details>
>
> Viewed 12 images
>
> 하단 여백만 바꾸면 본문 페이지 분할 위치에 따라 다른 페이지의 푸터가 가려지는 것을 확인했습니다. 따라서 DOCX 내용과 페이지 구성은 그대로 유지하고, 최종 PDF에서 머리글·바닥글 영역만 동일한 문서 스타일로 다시 올려 모든 페이지 번호가 보이게 처리하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> @'
> from io import BytesIO
> from pathlib import Path
> from pypdf import PdfReader, PdfWriter
> from reportlab.pdfgen import canvas
> from reportlab.lib.colors import HexColor, white
>
> src=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> out=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\LASAL_Motion_Control_API_User_Manual_KO_polished.pdf')
> final=Path(r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> reader=PdfReader(str(src))
> writer=PdfWriter()
> blue=HexColor('#123F6B')
> gray=HexColor('#6E7C89')
> for index,page in enumerate(reader.pages, start=1):
>     if index > 1:
>         w=float(page.mediabox.width); h=float(page.mediabox.height)
>         buf=BytesIO()
>         c=canvas.Canvas(buf,pagesize=(w,h))
>         c.setFillColor(white)
>         c.rect(0,0,w,38,stroke=0,fill=1)
>         c.rect(0,h-38,w,38,stroke=0,fill=1)
>         c.setFillColor(blue)
>         c.setFont('Helvetica-Bold',7.2)
>         c.drawRightString(w-89,h-28,'LASAL MOTION CONTROL API')
>         c.setFillColor(gray)
>         c.setFont('Helvetica',7.2)
>         c.drawRightString(w-45,h-28,'API GUIDE')
>         c.drawRightString(w-65,24,'LasalMotionControlLib 0.9.1-preview')
>         c.setFillColor(HexColor('#222222'))
>         c.setFont('Helvetica',8.0)
>         c.drawRightString(w-45,24,str(index))
>         c.save();buf.seek(0)
>         overlay=PdfReader(buf).pages[0]
>         page.merge_page(overlay, over=True)
>     writer.add_page(page)
> with out.open('wb') as f: writer.write(f)
> # Replace only after the polished file is complete and readable.
> check=PdfReader(str(out))
> if len(check.pages)!=len(reader.pages):
>     raise RuntimeError('page count changed during footer repair')
> final.write_bytes(out.read_bytes())
> print('pages',len(check.pages))
> print('output',final)
> print('bytes',final.stat().st_size)
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -
> $render='C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\final_pages'
> if(Test-Path -LiteralPath $render){Remove-Item -LiteralPath $render -Recurse -Force}
> New-Item -ItemType Directory -Force -Path $render | Out-Null
> $pdf='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> $pdftoppm=(Get-Command pdftoppm.exe).Source
> & $pdftoppm -png -r 144 $pdf (Join-Path $render 'page')
> @'
> from pathlib import Path
> from pypdf import PdfReader
> p=Path(r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> r=PdfReader(str(p))
> t='\n'.join((x.extract_text() or '') for x in r.pages)
> print('final_pages=',len(r.pages))
