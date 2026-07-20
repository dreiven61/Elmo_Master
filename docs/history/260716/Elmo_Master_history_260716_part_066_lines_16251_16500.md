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
>     w, h = float(page.mediabox.width), float(page.mediabox.height)
>     if abs(w-page_w) > 0.1 or abs(h-page_h) > 0.1:
>         raise RuntimeError(f'Page {i} size {w}x{h}')
> print(f'Created={out}')
> print(f'Pages={len(reader.pages)}')
> print(f'Encrypted={reader.is_encrypted}')
> print(f'Bytes={out.stat().st_size}')
> '@ | & $py -
> if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
> $source = 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_final.pdf'
> $dest = 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> Copy-Item -LiteralPath $source -Destination $dest -Force
> $items = @(
>   'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx',
>   $dest
> )
> foreach ($item in $items) {
>   $f = Get-Item -LiteralPath $item
>   $h = Get-FileHash -Algorithm SHA256 -LiteralPath $item
>   [pscustomobject]@{ Name=$f.Name; Length=$f.Length; LastWriteTime=$f.LastWriteTime; SHA256=$h.Hash }
> } | Format-Table -AutoSize`
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
>     w, h = float(page.mediabox.width), float(page.mediabox.height)
>     if abs(w-page_w) > 0.1 or abs(h-page_h) > 0.1:
>         raise RuntimeError(f'Page {i} size {w}x{h}')
> print(f'Created={out}')
> print(f'Pages={len(reader.pages)}')
> print(f'Encrypted={reader.is_encrypted}')
> print(f'Bytes={out.stat().st_size}')
> '@ | & $py -
> if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
> $source = 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_final.pdf'
> $dest = 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> Copy-Item -LiteralPath $source -Destination $dest -Force
> $items = @(
>   'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx',
>   $dest
> )
> $rows = @(foreach ($item in $items) {
>   $f = Get-Item -LiteralPath $item
>   $h = Get-FileHash -Algorithm SHA256 -LiteralPath $item
>   [pscustomobject]@{ Name=$f.Name; Length=$f.Length; LastWriteTime=$f.LastWriteTime; SHA256=$h.Hash }
> })
> $rows | Format-Table -AutoSize`
> - Ran `$pdf = 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> $outDir = 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_verify'
> New-Item -ItemType Directory -Path $outDir -Force | Out-Null
> $pdftoppm = (Get-Command pdftoppm -ErrorAction Stop).Source
> & $pdftoppm -png -r 144 $pdf (Join-Path $outDir 'page')
> if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
> Get-ChildItem -LiteralPath $outDir -Filter 'page-*.png' | Sort-Object Name | Select-Object Name,Length`
> - Ran `$cmd = Get-Command pdftoppm -ErrorAction SilentlyContinue; $cmd | Format-List *; Get-ChildItem -LiteralPath 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\bin' -Recurse -Filter 'pdftoppm*.exe' -ErrorAction SilentlyContinue | Select-Object -First 10 FullName`
> - Ran `$py = 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
> @'
> from pathlib import Path
> import fitz
> pdf = Path(r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> out = Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_verify')
> out.mkdir(parents=True, exist_ok=True)
> doc = fitz.open(pdf)
> if len(doc) != 21:
>     raise RuntimeError(f'Expected 21 pages, got {len(doc)}')
> for i, page in enumerate(doc, 1):
>     pix = page.get_pixmap(matrix=fitz.Matrix(2, 2), alpha=False)
>     pix.save(out / f'page-{i:02d}.png')
> print(f'Rendered {len(doc)} pages to {out}')
> '@ | & $py -
> if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
> Get-ChildItem -LiteralPath 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_verify' -Filter 'page-*.png' | Sort-Object Name | Select-Object Name,Length`
> - Ran `python -c "import importlib.util; print('fitz', bool(importlib.util.find_spec('fitz'))); print('pdf2image', bool(importlib.util.find_spec('pdf2image'))); print('pypdfium2', bool(importlib.util.find_spec('pypdfium2'))); print('PIL', bool(importlib.util.find_spec('PIL')))"; Get-Command gswin64c,magick,mutool -ErrorAction SilentlyContinue | Select-Object Name,Source`
> - Ran `@'
> from pathlib import Path
> import fitz
> pdf = Path(r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> out = Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_verify')
> out.mkdir(parents=True, exist_ok=True)
> doc = fitz.open(pdf)
> if len(doc) != 21:
>     raise RuntimeError(f'Expected 21 pages, got {len(doc)}')
> for i, page in enumerate(doc, 1):
>     pix = page.get_pixmap(matrix=fitz.Matrix(2, 2), alpha=False)
>     pix.save(out / f'page-{i:02d}.png')
> print(f'Rendered {len(doc)} pages to {out}')
> '@ | python -
> if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
> Get-ChildItem -LiteralPath 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_verify' -Filter 'page-*.png' | Sort-Object Name | Select-Object Name,Length`
>
> </details>
>
> 21페이지 PDF 조립과 재렌더링까지 완료했습니다. 이제 최종 PDF 자체를 페이지별로 다시 열어 1–21페이지의 잘림, 겹침, 머리글·바닥글, 페이지 번호를 전수 확인하고 있습니다.
>
> Viewed 7 images
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `@'
> from PIL import Image
> from pathlib import Path
> for n in range(1,8):
>  p=Path(fr'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\hybrid_final\page-{n:02d}.png')
>  im=Image.open(p)
>  print(n, im.mode, im.size, im.getpixel((10,10)), im.getpixel((im.width//2, im.height//2)))
> '@ | python -`
> - Ran `@'
> from PIL import Image
> from pathlib import Path
> for n in range(1,8):
>  p=Path(fr'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_verify\page-{n:02d}.png')
>  im=Image.open(p)
>  print(n, im.mode, im.size, im.getpixel((10,10)), im.getpixel((im.width//2, im.height//2)))
> '@ | python -`
>
> </details>
>
> Viewed an image
>
> <details><summary>Ran a command</summary>
>
> - Ran `@'
> from PIL import Image
> p=r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_verify\page-02.png'
> out=r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_verify\page-02-stripped.png'
> im=Image.open(p).convert('RGB')
> im.save(out, format='PNG', optimize=False)
> print(im.info)
> '@ | python -`
>
> </details>
>
> Viewed an image
>
> <details><summary>Ran a command</summary>
>
> - Ran `@'
> from pathlib import Path
> from PIL import Image
> folder = Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_verify')
> for p in sorted(folder.glob('page-[0-9][0-9].png')):
>     im = Image.open(p).convert('RGB')
>     tmp = p.with_suffix('.tmp.png')
>     im.save(tmp, format='PNG', optimize=False)
>     tmp.replace(p)
> print('Stripped PNG metadata for visual QA:', len(list(folder.glob('page-[0-9][0-9].png'))))
> '@ | python -`
>
> </details>
>
> Viewed 7 images
>
> <details><summary>Ran a command</summary>
>
> - Ran `@'
> from pathlib import Path
> from PIL import Image, ImageDraw, ImageFont
> src = Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual\image_only_verify')
> out = src / 'contact_sheets'
> out.mkdir(exist_ok=True)
> font = ImageFont.load_default()
> for batch, start in enumerate((1,8,15), 1):
>     nums = list(range(start, min(start+7,22)))
>     thumb_w, thumb_h = 760, 1074
>     label_h, gap = 32, 18
>     cols, rows = 2, 4
>     sheet = Image.new('RGB', (cols*thumb_w+(cols+1)*gap, rows*(thumb_h+label_h)+(rows+1)*gap), '#d0d0d0')
>     draw = ImageDraw.Draw(sheet)
>     for idx,n in enumerate(nums):
>         im = Image.open(src / f'page-{n:02d}.png').convert('RGB')
>         im = im.resize((thumb_w,thumb_h), Image.Resampling.LANCZOS)
>         col,row = idx%cols, idx//cols
>         x = gap + col*(thumb_w+gap)
>         y = gap + row*(thumb_h+label_h+gap)
>         draw.rectangle((x,y,x+thumb_w-1,y+label_h-1), fill='#1b466f')
>         draw.text((x+10,y+9), f'PAGE {n}', fill='white', font=font)
>         sheet.paste(im,(x,y+label_h))
>     p = out / f'batch-{batch}.png'
>     sheet.save(p, format='PNG')
>     print(p)
> '@ | python -`
>
> </details>
>
> Viewed an image
>
> Viewed an image
>
> Viewed an image
>
> <details><summary>Ran a command</summary>
>
> - Ran `$py = 'C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
> @'
> from pathlib import Path
> from pypdf import PdfReader
> pdf = Path(r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf')
> r = PdfReader(str(pdf))
> print(f'Exists={pdf.exists()}')
> print(f'Pages={len(r.pages)}')
> print(f'Encrypted={r.is_encrypted}')
> print(f'MediaBox1={float(r.pages[0].mediabox.width):.2f}x{float(r.pages[0].mediabox.height):.2f} pt')
> print(f'Title={r.metadata.title}')
> print(f'ExtractedTextChars={sum(len(p.extract_text() or "") for p in r.pages)}')
> '@ | & $py -
> if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
> $docx = 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'
> $pdf = 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> Get-FileHash -Algorithm SHA256 -LiteralPath $docx, $pdf | Format-List Path,Hash
> $cleanup = 'C:\work\Elmo\Elmo_Master\tmp\pdfs\user_edited_manual'
> $resolved = [System.IO.Path]::GetFullPath($cleanup)
> $allowed = [System.IO.Path]::GetFullPath('C:\work\Elmo\Elmo_Master\tmp\pdfs')
> if (-not $resolved.StartsWith($allowed, [System.StringComparison]::OrdinalIgnoreCase)) { throw "Unsafe cleanup target: $resolved" }
> if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
> "CleanupExists=$([bool](Test-Path -LiteralPath $resolved))"
> git status --short -- 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'`
>
> </details>
