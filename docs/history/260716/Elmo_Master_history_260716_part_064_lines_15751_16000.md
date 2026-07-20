> print('replacement_char=', '\ufffd' in t)
> print('required_tokens=', {x:(x in t) for x in ['LMCConnection','MoveAbsoluteExAsync','GroupPowerOnAsync','MoveLinearAbsoluteExAsync','UNIT']})
> print('sha256=',__import__('hashlib').sha256(p.read_bytes()).hexdigest().upper())
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -`
>
> </details>
>
> 푸터 보정된 최종 PDF가 생성됐고 21페이지, 주요 API 문자열, 한글 문자 추출 모두 정상입니다. 보정본의 21개 페이지를 마지막으로 다시 확인하겠습니다.
>
> Viewed 7 images
>
> 첫 보정 방식에서도 일부 페이지의 Word 표 배경이 PDF 내용 스트림 위에서 푸터를 다시 가리는 것을 확인했습니다. 새 PDF 페이지에 원본 페이지를 먼저 배치한 뒤 푸터를 최상위에 그리는 방식으로 바꾸겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> @'
> try:
>  import fitz
>  print('fitz_available',fitz.__doc__.splitlines()[0])
> except Exception as e:
>  print('fitz_unavailable',repr(e))
> from pypdf import PdfReader
> p=r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> r=PdfReader(p)
> for n in [2,4,8,10,12,16,20]:
>  a=r.pages[n-1].get('/Annots')
>  print('page',n,'annots',0 if a is None else len(a))
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -`
> - Ran `$ErrorActionPreference='Stop'
> @'
> from io import BytesIO
> from pathlib import Path
> from pypdf import PdfReader, PdfWriter
> from pypdf._page import PageObject
> from pypdf.generic import NameObject
> from reportlab.pdfgen import canvas
> from reportlab.lib.colors import HexColor, white
> src=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> out=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\normalized_test.pdf')
> r=PdfReader(str(src));wri=PdfWriter()
> for idx,orig in enumerate(r.pages,1):
>     w=float(orig.mediabox.width);h=float(orig.mediabox.height)
>     page=PageObject.create_blank_page(width=w,height=h)
>     page.merge_page(orig, over=True)
>     if '/Annots' in orig:
>         page[NameObject('/Annots')]=orig['/Annots']
>     if idx>1:
>         b=BytesIO();c=canvas.Canvas(b,pagesize=(w,h))
>         c.setFillColor(white);c.rect(0,0,w,38,stroke=0,fill=1);c.rect(0,h-38,w,38,stroke=0,fill=1)
>         c.setFillColor(HexColor('#123F6B'));c.setFont('Helvetica-Bold',7.2);c.drawRightString(w-89,h-28,'LASAL MOTION CONTROL API')
>         c.setFillColor(HexColor('#6E7C89'));c.setFont('Helvetica',7.2);c.drawRightString(w-45,h-28,'API GUIDE');c.drawRightString(w-65,24,'LasalMotionControlLib 0.9.1-preview')
>         c.setFillColor(HexColor('#222222'));c.setFont('Helvetica',8);c.drawRightString(w-45,24,str(idx));c.save();b.seek(0)
>         page.merge_page(PdfReader(b).pages[0],over=True)
>     wri.add_page(page)
> with out.open('wb') as f:wri.write(f)
> print('written',out.stat().st_size,'pages',len(PdfReader(str(out)).pages))
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -
> $test='C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\normalized_test'
> if(Test-Path -LiteralPath $test){Remove-Item -LiteralPath $test -Recurse -Force}
> New-Item -ItemType Directory -Force -Path $test | Out-Null
> $pdftoppm=(Get-Command pdftoppm.exe).Source
> & $pdftoppm -png -r 144 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\normalized_test.pdf' (Join-Path $test 'page')
> @'
> from pathlib import Path
> from PIL import Image
> base=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\normalized_test')
> out=base/'crops';out.mkdir()
> for i in [2,4,8,10,12,16,20]:
>  p=base/f'page-{i:02d}.png';im=Image.open(p);w,h=im.size
>  im.crop((int(w*.62),int(h*.91),w,h)).save(out/f'footer-{i:02d}.png')
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -`
>
> </details>
>
> Viewed 7 images
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> @'
> from io import BytesIO
> from pathlib import Path
> from pypdf import PdfReader,PdfWriter
> from pypdf._page import PageObject
> from reportlab.pdfgen import canvas
> from reportlab.lib.colors import red
> src=r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> orig=PdfReader(src).pages[3]
> for flag in [True,False]:
>  w=float(orig.mediabox.width);h=float(orig.mediabox.height)
>  page=PageObject.create_blank_page(width=w,height=h)
>  page.merge_page(orig,over=True)
>  b=BytesIO();c=canvas.Canvas(b,pagesize=(w,h));c.setFillColor(red);c.rect(0,0,w,38,fill=1,stroke=0);c.save();b.seek(0)
>  page.merge_page(PdfReader(b).pages[0],over=flag)
>  out=Path(fr'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\red_{flag}.pdf')
>  wr=PdfWriter();wr.add_page(page);out.write_bytes(b'')
>  with out.open('wb') as f:wr.write(f)
>  print(out)
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -
> $pdftoppm=(Get-Command pdftoppm.exe).Source
> & $pdftoppm -f 1 -l 1 -png -r 72 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\red_True.pdf' 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\red_true'
> & $pdftoppm -f 1 -l 1 -png -r 72 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\red_False.pdf' 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\red_false'`
>
> </details>
>
> Viewed 2 images
>
> 가려짐이 아니라 PDF 병합 과정에서 Word가 사용한 글꼴 리소스 이름과 보정용 기본 글꼴 이름이 충돌한 것이 원인이었습니다. 고유 이름으로 임베드한 Arial 글꼴을 사용해 다시 병합하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> @'
> from io import BytesIO
> from pathlib import Path
> from pypdf import PdfReader, PdfWriter
> from pypdf._page import PageObject
> from pypdf.generic import NameObject
> from reportlab.pdfgen import canvas
> from reportlab.lib.colors import HexColor, white
> from reportlab.pdfbase import pdfmetrics
> from reportlab.pdfbase.ttfonts import TTFont
> reg=r'C:\Windows\Fonts\arial.ttf';bold=r'C:\Windows\Fonts\arialbd.ttf'
> pdfmetrics.registerFont(TTFont('LmcGuideFooterRegular',reg))
> pdfmetrics.registerFont(TTFont('LmcGuideFooterBold',bold))
> src=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> out=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\normalized_ttf_test.pdf')
> r=PdfReader(str(src));wr=PdfWriter()
> for idx,orig in enumerate(r.pages,1):
>  w=float(orig.mediabox.width);h=float(orig.mediabox.height)
>  page=PageObject.create_blank_page(width=w,height=h);page.merge_page(orig,over=True)
>  if '/Annots' in orig: page[NameObject('/Annots')]=orig['/Annots']
>  if idx>1:
>   b=BytesIO();c=canvas.Canvas(b,pagesize=(w,h))
>   c.setFillColor(white);c.rect(0,0,w,38,fill=1,stroke=0);c.rect(0,h-38,w,38,fill=1,stroke=0)
>   c.setFillColor(HexColor('#123F6B'));c.setFont('LmcGuideFooterBold',7.2);c.drawRightString(w-89,h-28,'LASAL MOTION CONTROL API')
>   c.setFillColor(HexColor('#6E7C89'));c.setFont('LmcGuideFooterRegular',7.2);c.drawRightString(w-45,h-28,'API GUIDE');c.drawRightString(w-65,24,'LasalMotionControlLib 0.9.1-preview')
>   c.setFillColor(HexColor('#222222'));c.setFont('LmcGuideFooterRegular',8);c.drawRightString(w-45,24,str(idx));c.save();b.seek(0)
>   page.merge_page(PdfReader(b).pages[0],over=True)
>  wr.add_page(page)
> with out.open('wb') as f:wr.write(f)
> print('written',out.stat().st_size)
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -
> $test='C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\normalized_ttf_test'
> if(Test-Path -LiteralPath $test){Remove-Item -LiteralPath $test -Recurse -Force}
> New-Item -ItemType Directory -Force -Path $test | Out-Null
> $pdftoppm=(Get-Command pdftoppm.exe).Source
> & $pdftoppm -png -r 144 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\normalized_ttf_test.pdf' (Join-Path $test 'page')
> @'
> from pathlib import Path
> from PIL import Image
> base=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\normalized_ttf_test')
> out=base/'crops';out.mkdir()
> for i in [2,4,8,10,12,16,20]:
>  p=base/f'page-{i:02d}.png';im=Image.open(p);w,h=im.size
>  im.crop((int(w*.62),int(h*.91),w,h)).save(out/f'footer-{i:02d}.png')
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -`
>
> </details>
>
> Viewed 7 images
>
> Word PDF의 일부 페이지가 텍스트 클리핑 상태를 비정상적으로 유지해 이후에 추가한 PDF 텍스트까지 잘라내고 있습니다. 머리글·바닥글을 고해상도 이미지 스트립으로 올려 이 클리핑 영향을 제거하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> @'
> from io import BytesIO
> from pathlib import Path
> from PIL import Image,ImageDraw,ImageFont
> from pypdf import PdfReader,PdfWriter
> from pypdf._page import PageObject
> from pypdf.generic import NameObject
> from reportlab.pdfgen import canvas
> from reportlab.lib.utils import ImageReader
>
> src=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> out=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\raster_strip_test.pdf')
> r=PdfReader(str(src));wr=PdfWriter();dpi=300
> reg=r'C:\Windows\Fonts\arial.ttf';bold=r'C:\Windows\Fonts\arialbd.ttf'
>
> def right_text(draw,xy,text,font,fill):
>     x,y=xy;b=draw.textbbox((0,0),text,font=font);draw.text((x-(b[2]-b[0]),y),text,font=font,fill=fill)
>
> def strips(wpt,hpt,page_num):
>     sw=round(wpt*dpi/72);sh=round(38*dpi/72);scale=dpi/72
>     header=Image.new('RGB',(sw,sh),'white');hd=ImageDraw.Draw(header)
>     fbold=ImageFont.truetype(bold,round(7.2*scale));freg=ImageFont.truetype(reg,round(7.2*scale))
>     baseline=round(10*scale)
>     right_text(hd,(round((wpt-89)*scale),baseline),'LASAL MOTION CONTROL API',fbold,(18,63,107))
>     right_text(hd,(round((wpt-45)*scale),baseline),'API GUIDE',freg,(110,124,137))
>     footer=Image.new('RGB',(sw,sh),'white');fd=ImageDraw.Draw(footer)
>     fnum=ImageFont.truetype(reg,round(8*scale));fy=round(10*scale)
>     right_text(fd,(round((wpt-65)*scale),fy),'LasalMotionControlLib 0.9.1-preview',freg,(110,124,137))
>     right_text(fd,(round((wpt-45)*scale),fy),str(page_num),fnum,(34,34,34))
>     return header,footer
>
> for idx,orig in enumerate(r.pages,1):
>     w=float(orig.mediabox.width);h=float(orig.mediabox.height)
>     page=PageObject.create_blank_page(width=w,height=h);page.merge_page(orig,over=True)
>     if '/Annots' in orig: page[NameObject('/Annots')]=orig['/Annots']
>     if idx>1:
>         header,footer=strips(w,h,idx)
>         b=BytesIO();c=canvas.Canvas(b,pagesize=(w,h))
>         c.drawImage(ImageReader(footer),0,0,width=w,height=38,mask=None)
>         c.drawImage(ImageReader(header),0,h-38,width=w,height=38,mask=None)
>         c.save();b.seek(0);page.merge_page(PdfReader(b).pages[0],over=True)
>     wr.add_page(page)
> with out.open('wb') as f:wr.write(f)
> print('written',out.stat().st_size)
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -
> $test='C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\raster_strip_test'
> if(Test-Path -LiteralPath $test){Remove-Item -LiteralPath $test -Recurse -Force}
> New-Item -ItemType Directory -Force -Path $test | Out-Null
> $pdftoppm=(Get-Command pdftoppm.exe).Source
> & $pdftoppm -png -r 144 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\raster_strip_test.pdf' (Join-Path $test 'page')
> @'
> from pathlib import Path
> from PIL import Image
> base=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\raster_strip_test')
> out=base/'crops';out.mkdir()
> for i in [2,4,8,10,12,16,20]:
>  p=base/f'page-{i:02d}.png';im=Image.open(p);w,h=im.size
>  im.crop((int(w*.62),int(h*.91),w,h)).save(out/f'footer-{i:02d}.png')
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -`
>
> </details>
>
> Viewed 7 images
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$names=@('gswin64c.exe','gswin32c.exe','mgs.exe','mutool.exe','qpdf.exe','pdftocairo.exe'); foreach($n in $names){$c=Get-Command $n -ErrorAction SilentlyContinue; if($c){[pscustomobject]@{Name=$n;Path=$c.Source}}} | Format-Table -AutoSize`
> - Ran `$names=@('gswin64c.exe','gswin32c.exe','mgs.exe','mutool.exe','qpdf.exe','pdftocairo.exe'); $found=@(); foreach($n in $names){$c=Get-Command $n -ErrorAction SilentlyContinue; if($c){$found += [pscustomobject]@{Name=$n;Path=$c.Source}}}; $found | Format-Table -AutoSize`
>
> </details>
>
> PDF 자체를 Ghostscript로 한 번 정규화해 Word의 비정상 클리핑 상태를 닫은 다음 머리글·바닥글을 추가하겠습니다. 본문은 벡터/텍스트 상태로 유지하고 전체 페이지 래스터화는 하지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $src='C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> $norm='C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\ghostscript_normalized.pdf'
> if(Test-Path -LiteralPath $norm){Remove-Item -LiteralPath $norm -Force}
> & 'C:\Program Files\MiKTeX\miktex\bin\x64\mgs.exe' -sDEVICE=pdfwrite -dCompatibilityLevel=1.7 -dPDFSETTINGS=/prepress -dNOPAUSE -dQUIET -dBATCH "-sOutputFile=$norm" $src
> if($LASTEXITCODE -ne 0){throw "mgs failed: $LASTEXITCODE"}
> @'
