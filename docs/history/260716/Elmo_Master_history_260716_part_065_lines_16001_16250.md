> from io import BytesIO
> from pathlib import Path
> from PIL import Image,ImageDraw,ImageFont
> from pypdf import PdfReader,PdfWriter
> from reportlab.pdfgen import canvas
> from reportlab.lib.utils import ImageReader
> norm=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\ghostscript_normalized.pdf')
> out=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\ghostscript_overlay_test.pdf')
> r=PdfReader(str(norm));wr=PdfWriter();dpi=300
> reg=r'C:\Windows\Fonts\arial.ttf';bold=r'C:\Windows\Fonts\arialbd.ttf'
> def right(draw,x,y,text,font,color):
>  b=draw.textbbox((0,0),text,font=font);draw.text((x-(b[2]-b[0]),y),text,font=font,fill=color)
> def overlay(w,h,n):
>  scale=dpi/72;sw=round(w*scale);sh=round(38*scale)
>  hr=Image.new('RGB',(sw,sh),'white');d=ImageDraw.Draw(hr)
>  fr=ImageFont.truetype(reg,round(7.2*scale));fb=ImageFont.truetype(bold,round(7.2*scale));fn=ImageFont.truetype(reg,round(8*scale));y=round(10*scale)
>  right(d,round((w-89)*scale),y,'LASAL MOTION CONTROL API',fb,(18,63,107));right(d,round((w-45)*scale),y,'API GUIDE',fr,(110,124,137))
>  ft=Image.new('RGB',(sw,sh),'white');d=ImageDraw.Draw(ft);right(d,round((w-65)*scale),y,'LasalMotionControlLib 0.9.1-preview',fr,(110,124,137));right(d,round((w-45)*scale),y,str(n),fn,(34,34,34))
>  b=BytesIO();c=canvas.Canvas(b,pagesize=(w,h));c.drawImage(ImageReader(ft),0,0,width=w,height=38);c.drawImage(ImageReader(hr),0,h-38,width=w,height=38);c.save();b.seek(0)
>  return PdfReader(b).pages[0]
> for i,p in enumerate(r.pages,1):
>  if i>1:p.merge_page(overlay(float(p.mediabox.width),float(p.mediabox.height),i),over=True)
>  wr.add_page(p)
> with out.open('wb') as f:wr.write(f)
> print('norm_bytes',norm.stat().st_size,'out_bytes',out.stat().st_size,'pages',len(r.pages))
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -
> $test='C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\ghostscript_overlay_test'
> if(Test-Path -LiteralPath $test){Remove-Item -LiteralPath $test -Recurse -Force}
> New-Item -ItemType Directory -Force -Path $test | Out-Null
> $pdftoppm=(Get-Command pdftoppm.exe).Source
> & $pdftoppm -png -r 144 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\ghostscript_overlay_test.pdf' (Join-Path $test 'page')
> @'
> from pathlib import Path
> from PIL import Image
> base=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\ghostscript_overlay_test');out=base/'crops';out.mkdir()
> for i in [2,4,8,10,12,16,20]:
>  im=Image.open(base/f'page-{i:02d}.png');w,h=im.size;im.crop((int(w*.62),int(h*.91),w,h)).save(out/f'footer-{i:02d}.png')
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -`
>
> </details>
>
> Ghostscript가 Windows 백슬래시 경로 인자를 출력 파일 옵션으로 해석하지 못해 정규화 단계만 실패했습니다. 출력 파일은 아직 교체되지 않았으며, 슬래시 경로로 다시 실행하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $src='C:/work/Elmo/Elmo_Master/tmp/pdfs/user_edited_manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'
> $norm='C:/work/Elmo/Elmo_Master/tmp/pdfs/user_edited_manual/ghostscript_normalized.pdf'
> if(Test-Path -LiteralPath $norm){Remove-Item -LiteralPath $norm -Force}
> $args=@('-sDEVICE=pdfwrite','-dCompatibilityLevel=1.7','-dPDFSETTINGS=/prepress','-dNOPAUSE','-dQUIET','-dBATCH',"-sOutputFile=$norm",$src)
> & 'C:\Program Files\MiKTeX\miktex\bin\x64\mgs.exe' $args
> if($LASTEXITCODE -ne 0){throw "mgs failed: $LASTEXITCODE"}
> if(-not(Test-Path -LiteralPath $norm)){throw 'normalized PDF missing'}
> @'
> from io import BytesIO
> from pathlib import Path
> from PIL import Image,ImageDraw,ImageFont
> from pypdf import PdfReader,PdfWriter
> from reportlab.pdfgen import canvas
> from reportlab.lib.utils import ImageReader
> norm=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\ghostscript_normalized.pdf')
> out=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\ghostscript_overlay_test.pdf')
> r=PdfReader(str(norm));wr=PdfWriter();dpi=300
> reg=r'C:\Windows\Fonts\arial.ttf';bold=r'C:\Windows\Fonts\arialbd.ttf'
> def right(draw,x,y,text,font,color):
>  b=draw.textbbox((0,0),text,font=font);draw.text((x-(b[2]-b[0]),y),text,font=font,fill=color)
> def overlay(w,h,n):
>  scale=dpi/72;sw=round(w*scale);sh=round(38*scale)
>  fr=ImageFont.truetype(reg,round(7.2*scale));fb=ImageFont.truetype(bold,round(7.2*scale));fn=ImageFont.truetype(reg,round(8*scale));y=round(10*scale)
>  hr=Image.new('RGB',(sw,sh),'white');d=ImageDraw.Draw(hr);right(d,round((w-89)*scale),y,'LASAL MOTION CONTROL API',fb,(18,63,107));right(d,round((w-45)*scale),y,'API GUIDE',fr,(110,124,137))
>  ft=Image.new('RGB',(sw,sh),'white');d=ImageDraw.Draw(ft);right(d,round((w-65)*scale),y,'LasalMotionControlLib 0.9.1-preview',fr,(110,124,137));right(d,round((w-45)*scale),y,str(n),fn,(34,34,34))
>  b=BytesIO();c=canvas.Canvas(b,pagesize=(w,h));c.drawImage(ImageReader(ft),0,0,width=w,height=38);c.drawImage(ImageReader(hr),0,h-38,width=w,height=38);c.save();b.seek(0);return PdfReader(b).pages[0]
> for i,p in enumerate(r.pages,1):
>  if i>1:p.merge_page(overlay(float(p.mediabox.width),float(p.mediabox.height),i),over=True)
>  wr.add_page(p)
> with out.open('wb') as f:wr.write(f)
> print('norm_bytes',norm.stat().st_size,'out_bytes',out.stat().st_size,'pages',len(r.pages))
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -
> $test='C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\ghostscript_overlay_test'
> if(Test-Path -LiteralPath $test){Remove-Item -LiteralPath $test -Recurse -Force}
> New-Item -ItemType Directory -Force -Path $test | Out-Null
> $pdftoppm=(Get-Command pdftoppm.exe).Source
> & $pdftoppm -png -r 144 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\ghostscript_overlay_test.pdf' (Join-Path $test 'page')
> @'
> from pathlib import Path
> from PIL import Image
> base=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\ghostscript_overlay_test');out=base/'crops';out.mkdir()
> for i in [2,4,8,10,12,16,20]:
>  im=Image.open(base/f'page-{i:02d}.png');w,h=im.size;im.crop((int(w*.62),int(h*.91),w,h)).save(out/f'footer-{i:02d}.png')
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -`
>
> </details>
>
> Viewed 7 images
>
> 벡터 상태를 계속 보정하는 방식은 Word가 만든 페이지별 클리핑 편차 때문에 안정적이지 않았습니다. 최종본은 300dpi 렌더링 화면을 최상위 시각 레이어로 사용하고, 원본 PDF 텍스트와 목차 링크는 그 아래에 유지하는 하이브리드 PDF로 만들겠습니다. 화면은 깨끗하고 검색 가능한 텍스트도 보존됩니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $raw='C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> $work='C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\hybrid_final'
> if(Test-Path -LiteralPath $work){Remove-Item -LiteralPath $work -Recurse -Force}
> New-Item -ItemType Directory -Force -Path $work | Out-Null
> $pdftoppm=(Get-Command pdftoppm.exe).Source
> & $pdftoppm -png -r 300 $raw (Join-Path $work 'page')
> @'
> from io import BytesIO
> from pathlib import Path
> from PIL import Image,ImageDraw,ImageFont
> from pypdf import PdfReader,PdfWriter
> from pypdf.generic import NameObject
> from reportlab.pdfgen import canvas
> from reportlab.lib.utils import ImageReader
>
> raw=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> work=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\hybrid_final')
> out=work/'LASAL_Motion_Control_API_User_Manual_KO.pdf'
> reader=PdfReader(str(raw));writer=PdfWriter();dpi=300
> reg=r'C:\Windows\Fonts\arial.ttf';bold=r'C:\Windows\Fonts\arialbd.ttf'
> fr=ImageFont.truetype(reg,30);fb=ImageFont.truetype(bold,30);fn=ImageFont.truetype(reg,33)
>
> def right(draw,x,y,text,font,color):
>     b=draw.textbbox((0,0),text,font=font)
>     draw.text((x-(b[2]-b[0]),y),text,font=font,fill=color)
>
> for idx,orig in enumerate(reader.pages,1):
>     img_path=work/f'page-{idx:02d}.png'
>     im=Image.open(img_path).convert('RGB')
>     if idx>1:
>         wpx,hpx=im.size; band=round(38*dpi/72); scale=dpi/72
>         draw=ImageDraw.Draw(im)
>         draw.rectangle((0,0,wpx,band),fill='white')
>         draw.rectangle((0,hpx-band,wpx,hpx),fill='white')
>         y=round(10*scale)
>         right(draw,round((float(orig.mediabox.width)-89)*scale),y,'LASAL MOTION CONTROL API',fb,(18,63,107))
>         right(draw,round((float(orig.mediabox.width)-45)*scale),y,'API GUIDE',fr,(110,124,137))
>         fy=hpx-band+y
>         right(draw,round((float(orig.mediabox.width)-65)*scale),fy,'LasalMotionControlLib 0.9.1-preview',fr,(110,124,137))
>         right(draw,round((float(orig.mediabox.width)-45)*scale),fy,str(idx),fn,(34,34,34))
>     im.save(img_path,format='PNG',optimize=True)
>     w=float(orig.mediabox.width);h=float(orig.mediabox.height)
>     b=BytesIO();c=canvas.Canvas(b,pagesize=(w,h))
>     c.drawImage(ImageReader(str(img_path)),0,0,width=w,height=h,mask=None)
>     c.showPage();c.save();b.seek(0)
>     page=PdfReader(b).pages[0]
>     # Keep searchable source text under the rendered visual layer.
>     page.merge_page(orig,over=False)
>     if '/Annots' in orig:
>         page[NameObject('/Annots')]=orig['/Annots']
>     writer.add_page(page)
> writer.add_metadata({
>     '/Title':'LASAL Motion Control API 기능 설명서',
>     '/Subject':'LASAL Motion Control API 기능, UNIT 및 반환값',
>     '/Creator':'Microsoft Word source converted and visually verified',
> })
> with out.open('wb') as f:writer.write(f)
> print('hybrid_pdf',out)
> print('bytes',out.stat().st_size)
> print('pages',len(PdfReader(str(out)).pages))
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -
> $hybrid=Join-Path $work 'LASAL_Motion_Control_API_User_Manual_KO.pdf'
> $final='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> Copy-Item -LiteralPath $hybrid -Destination $final -Force
> $verify=Join-Path $work 'verify_pages'
> New-Item -ItemType Directory -Force -Path $verify | Out-Null
> & $pdftoppm -png -r 144 $final (Join-Path $verify 'page')
> @'
> from pathlib import Path
> from pypdf import PdfReader
> import hashlib
> p=Path(r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> r=PdfReader(str(p));t='\n'.join((x.extract_text() or '') for x in r.pages)
> print('final_pages',len(r.pages))
> print('replacement_char','\ufffd' in t)
> print('searchable_chars',len(t))
> print('required_tokens',{x:(x in t) for x in ['LMCConnection','MoveAbsoluteExAsync','GroupPowerOnAsync','MoveLinearAbsoluteExAsync','UNIT']})
> print('sha256',hashlib.sha256(p.read_bytes()).hexdigest().upper())
> '@ | & 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -`
>
> </details>
>
> Viewed 7 images
>
> 원본 텍스트 레이어를 아래에 합치는 과정에서도 Word 페이지의 클리핑 스트림이 최종 화면에 다시 영향을 주는 것을 확인했습니다. 검색 레이어 보존은 포기하고, 검증된 300dpi 화면만으로 최종 PDF를 만들겠습니다. 원본 DOCX는 그대로 남아 있으므로 수정 가능한 원본에는 영향이 없습니다.
>
> Viewed 6 images
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$src = 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\hybrid_final'; Get-ChildItem -LiteralPath $src -Filter 'page-*.png' | Sort-Object Name | Select-Object Name,Length; python -c "import reportlab; print(reportlab.Version)"`
> - Ran `@'
> from pathlib import Path
> from reportlab.pdfgen import canvas
> from reportlab.lib.utils import ImageReader
> from pypdf import PdfReader
>
> src_dir = Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\hybrid_final')
> out = Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_final.pdf')
> images = sorted(src_dir.glob('page-*.png'))
> if len(images) != 21:
>     raise SystemExit(f'Expected 21 images, found {len(images)}')
> page_w, page_h = 595.32, 841.92
> c = canvas.Canvas(str(out), pagesize=(page_w, page_h), pageCompression=1)
> c.setTitle('LASAL Motion Control API User Manual KO')
> c.setAuthor('LASAL Motion Control API')
> c.setSubject('LASAL Motion Control API User Manual')
> for img_path in images:
>     c.drawImage(ImageReader(str(img_path)), 0, 0, width=page_w, height=page_h, preserveAspectRatio=False, mask='auto')
>     c.showPage()
> c.save()
> reader = PdfReader(str(out))
> assert len(reader.pages) == 21, len(reader.pages)
> for i, page in enumerate(reader.pages, 1):
>     box = page.mediabox
>     w, h = float(box.width), float(box.height)
>     if abs(w-page_w) > 0.1 or abs(h-page_h) > 0.1:
>         raise RuntimeError(f'Page {i} size {w}x{h}')
> print(f'Created {out}')
> print(f'Pages={len(reader.pages)}, Encrypted={reader.is_encrypted}, Size={out.stat().st_size}')
> '@ | python -
> $source = 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_final.pdf'
> $dest = 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> Copy-Item -LiteralPath $source -Destination $dest -Force
> Get-FileHash -Algorithm SHA256 -LiteralPath 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx', $dest | Select-Object Path,Hash
> Get-Item -LiteralPath $dest | Select-Object FullName,Length,LastWriteTime`
>
> </details>
>
> 최종 PDF 조립 단계에서 기본 Python 3.8과 최신 ReportLab 간 호환 오류가 확인됐습니다. 페이지 이미지 자체는 정상이며, 번들 문서 런타임이나 다른 PDF 조립 방식으로 즉시 교체해 진행하겠습니다.
>
> <details><summary>Ran 7 commands</summary>
>
> - Ran `$py = 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
> @'
> from pathlib import Path
> from reportlab.pdfgen import canvas
> from reportlab.lib.utils import ImageReader
> from pypdf import PdfReader
>
> src_dir = Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\hybrid_final')
> out = Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_final.pdf')
> images = sorted(src_dir.glob('page-*.png'))
> if len(images) != 21:
>     raise SystemExit(f'Expected 21 images, found {len(images)}')
