</details>

수정한 DOCX 기준으로 PDF 생성 완료했습니다.

[LASAL API 사용 설명서 PDF](./LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf)

- A4, 21페이지
- 전체 페이지 잘림·겹침 검수 완료
- 원본 DOCX는 변경하지 않음
- 화면 보존을 위해 300dpi 이미지 기반으로 제작되어 PDF 내 텍스트 검색·복사는 지원되지 않습니다.

> 작업 진행된 내용 정리해서 git에 올려줘

<details><summary>20 previous messages</summary>

> 현재 작업 트리의 실제 변경분을 먼저 확인해 목적별로 정리한 뒤 커밋하고 원격 저장소에 푸시하겠습니다. 이 작업에는 `github:yeet` 절차를 적용해 변경 범위·브랜치·원격 상태를 검증하겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-Content -Raw -LiteralPath 'C:\Users\dreiv\.codex\plugins\cache\openai-curated-remote\github\0.1.8-2841cf9749ae\skills\yeet\SKILL.md'
> rg -n -i "Elmo_Master|LMC_API_Distribution|API User Manual|LasalMotionControlLib|Git" 'C:\Users\dreiv\.codex\memories\MEMORY.md' | Select-Object -First 30`
> - Ran `gh --version
> gh auth status
> git status -sb
> git branch --show-current
> git remote -v
> git log -8 --oneline --decorate`
>
> </details>
>
> 현재 변경분은 API 소스·테스트·내부 문서·배포 패키지·사용 설명서가 한꺼번에 섞여 있습니다. `gh` CLI는 설치되어 있지 않아 PR 생성 절차는 사용할 수 없지만, 요청하신 커밋과 원격 푸시는 Git 자체로 진행할 수 있습니다. 우선 임시 생성물과 실제 배포 대상부터 분리하겠습니다.
>
> <details><summary>Ran 7 commands</summary>
>
> - Ran `git diff --stat
> git diff --name-status
> Get-ChildItem -LiteralPath 'LMC_Library\LMC_API_Distribution' -Recurse -File -ErrorAction SilentlyContinue | Sort-Object FullName | Select-Object FullName,Length
> Get-ChildItem -LiteralPath 'output\doc' -Recurse -File -ErrorAction SilentlyContinue | Sort-Object FullName | Select-Object FullName,Length
> Get-ChildItem -LiteralPath 'LMC_Library\LMC_API' -Recurse -File | Where-Object { $_.FullName -notmatch '\\.git\\' } | Sort-Object FullName | Select-Object FullName,Length`
> - Ran `git diff -- .gitignore AGENTS.md LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj LMC_Library/LMC_API_Delivery/src/LmcConnection.cs LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs LMC_Library/LMC_API_Delivery/src/Properties/AssemblyInfo.cs LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/Properties/AssemblyInfo.cs`
> - Ran `git diff -- LMC_Library/LMC_API/LMC_API/README.md LMC_Library/LMC_API_Delivery/README.md LMC_Library/LMC_API_Delivery/docs LMC_Library/LasalApiWpfTestApp/DESIGN.md LMC_Library/LasalApiWpfTestApp/README.md
> Get-Content -Raw -LiteralPath 'LMC_Library\LMC_API\README.md'
> Get-Content -Raw -LiteralPath 'LMC_Library\LMC_API\API_DEVELOPMENT_GUIDE.md'
> Get-Content -Raw -LiteralPath 'LMC_Library\LMC_API\API_SOURCE_REVIEW_2026-07-15.md'
> Get-Content -Raw -LiteralPath 'LMC_Library\LMC_API\Build-LmcApiDistribution.ps1'
> Get-Content -Raw -LiteralPath 'LMC_Library\README.md'
> Get-Content -Raw -LiteralPath 'LMC_Library\LMC_API_Distribution\README.md'
> Get-Content -Raw -LiteralPath 'LMC_Library\LMC_API_Distribution\02_Example_Program\README.md'`
> - Ran `Get-ChildItem -LiteralPath 'LMC_Library\LMC_API_Distribution\03_API_User_Manual' -File | Sort-Object Name | Format-Table Name,Length,LastWriteTime -AutoSize
> Get-ChildItem -LiteralPath 'LMC_Library\LMC_API_Distribution\01_API' -File | Format-Table Name,Length,LastWriteTime -AutoSize
> Get-ChildItem -LiteralPath 'LMC_Library\LMC_API_Distribution\02_Example_Program\Run' -File | Format-Table Name,Length,LastWriteTime -AutoSize
> Get-FileHash -Algorithm SHA256 -LiteralPath 'LMC_Library\LMC_API_Distribution\01_API\LasalMotionControlLib.dll','LMC_Library\LMC_API_Distribution\02_Example_Program\Run\LasalMotionControlLib.dll' | Format-Table Path,Hash -AutoSize
> Get-Item -LiteralPath 'output\doc\LASAL_Motion_Control_API_User_Manual_KO.docx' | Format-List FullName,Length,LastWriteTime
> Get-FileHash -Algorithm SHA256 -LiteralPath 'output\doc\LASAL_Motion_Control_API_User_Manual_KO.docx','LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx' | Format-Table Path,Hash -AutoSize`
> - Ran `$py='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
> @'
> from pathlib import Path
> from docx import Document
> from pypdf import PdfReader
> from difflib import unified_diff
> base=Path(r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual')
> for name in ['LASAL_Motion_Control_API_User_Manual_KO.docx','LASAL_Motion_Control_API_User_Manual_KO_edit.docx']:
>  p=base/name; d=Document(p)
>  parts=[]
>  for para in d.paragraphs:
>   if para.text.strip(): parts.append(para.text.strip())
>  for ti,t in enumerate(d.tables):
>   for row in t.rows:
>    parts.append(' | '.join(c.text.strip().replace('\n',' / ') for c in row.cells))
>  print(name, 'paras',len(d.paragraphs),'tables',len(d.tables),'parts',len(parts),'chars',sum(map(len,parts)), 'title', d.core_properties.title)
>  (base/(name+'.txt')).write_text('\n'.join(parts),encoding='utf-8')
> a=(base/'LASAL_Motion_Control_API_User_Manual_KO.docx.txt').read_text(encoding='utf-8').splitlines()
> b=(base/'LASAL_Motion_Control_API_User_Manual_KO_edit.docx.txt').read_text(encoding='utf-8').splitlines()
> print('TEXT_IDENTICAL', a==b)
> for line in list(unified_diff(a,b,fromfile='KO',tofile='KO_edit',n=1))[:120]: print(line)
> for name in ['LASAL_Motion_Control_API_User_Manual_KO.pdf','LASAL_Motion_Control_API_User_Manual_KO_edit.pdf']:
>  p=base/name; r=PdfReader(p)
>  text=''.join(pg.extract_text() or '' for pg in r.pages)
>  print(name,'pages',len(r.pages),'textchars',len(text),'title',r.metadata.title)
> '@ | & $py -
> Remove-Item -LiteralPath 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx.txt','LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO_edit.docx.txt' -Force`
> - Ran `Get-Content -LiteralPath '.gitignore' | Select-Object -First 140
> if (Test-Path -LiteralPath '.gitattributes') { Get-Content -LiteralPath '.gitattributes' }`
> - Ran `Get-Content -Raw -LiteralPath 'LMC_Library\LMC_API_Distribution\02_Example_Program\LasalApiWpfTestApp\LasalApiWpfTestApp.csproj'
> Get-Content -Raw -LiteralPath 'LMC_Library\LMC_API_Distribution\02_Example_Program\LasalApiWpfTestApp.sln'
> Get-Content -Raw -LiteralPath 'LMC_Library\LMC_API\Generate-ApiUserManual.py' | Select-Object -First 1
> Get-Content -Raw -LiteralPath 'LMC_Library\LMC_API\Generate-ApiUserManualDocx.py' | Select-Object -First 1`
>
> </details>
>
> 배포 폴더 안에 더 최근에 저장된 `_edit.docx/.pdf`가 추가로 존재합니다. 내용 비교 결과 `_edit`본은 편집 안내 문구를 제거한 최신 사용자 수정본이므로, 중복 배포를 피하려면 이 최신본을 정식 파일명으로 정리하는 것이 맞습니다. 먼저 최신 PDF의 21페이지 레이아웃을 검증하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `@'
> from pathlib import Path
> import fitz
> from PIL import Image, ImageDraw, ImageFont
> pdf=Path(r'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO_edit.pdf')
> out=Path(r'C:\work\Elmo\Elmo_Master\tmp\pdfs\edit_manual_verify')
> out.mkdir(parents=True,exist_ok=True)
> doc=fitz.open(pdf)
> for i,p in enumerate(doc,1):
>  pix=p.get_pixmap(matrix=fitz.Matrix(1.5,1.5),alpha=False)
>  raw=out/f'page-{i:02d}-raw.png'; pix.save(raw)
>  im=Image.open(raw).convert('RGB'); im.save(out/f'page-{i:02d}.png',format='PNG'); raw.unlink()
> font=ImageFont.load_default()
> for batch,start in enumerate((1,8,15),1):
>  nums=range(start,min(start+7,22)); tw,th=760,1074; lh,g=32,18; cols,rows=2,4
>  sheet=Image.new('RGB',(cols*tw+(cols+1)*g,rows*(th+lh)+(rows+1)*g),'#d0d0d0'); d=ImageDraw.Draw(sheet)
>  for idx,n in enumerate(nums):
>   im=Image.open(out/f'page-{n:02d}.png').convert('RGB').resize((tw,th),Image.Resampling.LANCZOS)
>   c,row=idx%cols,idx//cols; x=g+c*(tw+g); y=g+row*(th+lh+g)
>   d.rectangle((x,y,x+tw-1,y+lh-1),fill='#1b466f'); d.text((x+10,y+9),f'PAGE {n}',fill='white',font=font); sheet.paste(im,(x,y+lh))
>  sheet.save(out/f'batch-{batch}.png',format='PNG')
> print('pages',len(doc),'output',out)
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
> <details><summary>Ran 4 commands</summary>
>
> - Ran `rg -n "Generate-ApiUserManual|API_USER_MANUAL_KO|output[\\/]doc|output[\\/]pdf|Build-LmcApiDistribution|매뉴얼.*생성|Markdown.*원본|source of truth|source-of-truth" LMC_Library/LMC_API LMC_Library/LMC_API_Delivery LMC_Library/README.md LMC_Library/LMC_API_Distribution -g '*.md' -g '*.ps1' -g '*.py'`
> - Ran `Get-Content -LiteralPath 'LMC_Library\LMC_API\API_DEVELOPMENT_GUIDE.md' | Select-Object -Skip 295 -First 60
> Get-Content -LiteralPath 'LMC_Library\LMC_API\README.md' | Select-Object -First 70
> Get-Content -LiteralPath 'LMC_Library\LMC_API_Delivery\docs\USER_MANUAL_PREPARATION_2026-07-13.md' | Select-Object -First 30`
> - Ran `$p='LMC_Library\LMC_API\Build-LmcApiDistribution.ps1'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(25,60),@(145,245),@(245,335))){"--- $($range[0])-$($range[1])"; for($i=$range[0];$i -le $range[1];$i++){ if($i -le $lines.Count){'{0,4}: {1}' -f $i,$lines[$i-1]}}}`
> - Ran `Get-Content -LiteralPath 'LMC_Library\LMC_API\API_USER_MANUAL_KO.md' | Select-Object -First 35`
>
> </details>
>
> 최신 `_edit` 문서는 21페이지 모두 정상이며 텍스트 검색도 가능한 Word 직접 내보내기 PDF입니다. 이 최신본을 정식 파일명으로 승격하고, 배포 스크립트가 이후 사용자 편집본을 덮어쓰지 않도록 DOCX/PDF를 배포 기준 원본으로 바꾸겠습니다. 생성용 Markdown과 Python은 초기 초안 생성 도구로만 남깁니다.
>
> <details><summary>Edited 5 files, ran a command</summary>
>
> - Wrote `./.gitignore` (+5 -0)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+8 -3)
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+21 -40)
> - Wrote `./LMC_Library/LMC_API/README.md` (+5 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+5 -2)
> - Ran `$manualDir = 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\03_API_User_Manual'
> $latestDocx = Join-Path $manualDir 'LASAL_Motion_Control_API_User_Manual_KO_edit.docx'
> $latestPdf = Join-Path $manualDir 'LASAL_Motion_Control_API_User_Manual_KO_edit.pdf'
> $canonicalDocx = Join-Path $manualDir 'LASAL_Motion_Control_API_User_Manual_KO.docx'
> $canonicalPdf = Join-Path $manualDir 'LASAL_Motion_Control_API_User_Manual_KO.pdf'
> Copy-Item -LiteralPath $latestDocx -Destination $canonicalDocx -Force
> Copy-Item -LiteralPath $latestPdf -Destination $canonicalPdf -Force
> if ((Get-FileHash -Algorithm SHA256 -LiteralPath $latestDocx).Hash -ne (Get-FileHash -Algorithm SHA256 -LiteralPath $canonicalDocx).Hash) { throw 'DOCX promotion verification failed.' }
> if ((Get-FileHash -Algorithm SHA256 -LiteralPath $latestPdf).Hash -ne (Get-FileHash -Algorithm SHA256 -LiteralPath $canonicalPdf).Hash) { throw 'PDF promotion verification failed.' }
> Remove-Item -LiteralPath $latestDocx,$latestPdf -Force
>
> $workspace = [System.IO.Path]::GetFullPath('C:\work\Elmo\Elmo_Master').TrimEnd('\') + '\'
> $cleanupTargets = @(
>   'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\__pycache__',
>   'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\02_Example_Program\.vs',
>   'C:\work\Elmo\Elmo_Master\output\doc',
>   'C:\work\Elmo\Elmo_Master\tmp\pdfs\edit_manual_verify'
> )
> foreach ($target in $cleanupTargets) {
>   $resolved = [System.IO.Path]::GetFullPath($target)
>   if (-not $resolved.StartsWith($workspace,[System.StringComparison]::OrdinalIgnoreCase)) { throw "Unsafe cleanup target: $resolved" }
>   if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
> }
> Get-ChildItem -LiteralPath $manualDir -File | Sort-Object Name | Format-Table Name,Length,LastWriteTime -AutoSize
> Get-FileHash -Algorithm SHA256 -LiteralPath $canonicalDocx,$canonicalPdf | Format-List Path,Hash`
>
> </details>
>
> 배포 구조 정리는 끝났습니다. 최신 사용자 편집본만 정식 DOCX/PDF로 남겼고, 캐시·IDE 상태·중간 DOCX는 제거했습니다. 이제 수정된 배포 스크립트로 Release DLL 재빌드, PC 테스트, LASAL 정적 계약, 독립 예제 Debug/Release 빌드까지 한 번에 검증하겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$errors=$null; [System.Management.Automation.Language.Parser]::ParseFile((Resolve-Path 'LMC_Library\LMC_API\Build-LmcApiDistribution.ps1'),[ref]$null,[ref]$errors) | Out-Null; if($errors){$errors | Format-List; exit 1}
> git diff --check
> & 'LMC_Library\LMC_API\Build-LmcApiDistribution.ps1' -RepositoryRoot (Resolve-Path '.').Path -AllowDirty`
> - Ran `& 'LMC_Library\LMC_API\Build-LmcApiDistribution.ps1' -RepositoryRoot (Resolve-Path '.').Path -AllowDirty`
>
> </details>
>
> 전체 검증이 통과했습니다: PC 46/46, LASAL source-only/full-network 정적 계약, 개발·배포 예제 빌드, 외부 복사본 Debug/Release 독립 빌드가 모두 성공했습니다. 이제 임시 파일이 빠졌는지 최종 상태를 확인한 뒤 API·개발문서·배포패키지의 3개 목적별 커밋으로 나누겠습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `git status --short --untracked-files=all
> git diff --stat
> git diff --check
> Get-ChildItem -LiteralPath 'LMC_Library\LMC_API_Distribution' -Recurse -Force | Where-Object { $_.PSIsContainer -and $_.Name -in @('.vs','bin','obj') } | Select-Object FullName`
> - Ran `git add -- 'LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj' 'LMC_Library/LMC_API_Delivery/src/LmcConnection.cs' 'LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs' 'LMC_Library/LMC_API_Delivery/src/Properties/AssemblyInfo.cs' 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs'
> git diff --cached --check
> git diff --cached --stat
> git commit -m "feat(api): harden response data and bump preview version"`
> - Ran `git add -- 'AGENTS.md' 'LMC_Library/README.md' 'LMC_Library/LMC_API/README.md' 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md' 'LMC_Library/LMC_API/API_SOURCE_REVIEW_2026-07-15.md' 'LMC_Library/LMC_API/LMC_API/README.md' 'LMC_Library/LMC_API_Delivery/README.md' 'LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md' 'LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md' 'LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md' 'LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt' 'LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md' 'LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md' 'LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md' 'LMC_Library/LasalApiWpfTestApp/DESIGN.md' 'LMC_Library/LasalApiWpfTestApp/README.md'
> git diff --cached --check
> git diff --cached --stat
> git commit -m "docs(api): align nine-axis unit and release guidance"`
> - Ran `git add -- '.gitignore' 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md' 'LMC_Library/LMC_API/Build-LmcApiDistribution.ps1' 'LMC_Library/LMC_API/Generate-ApiUserManual.py' 'LMC_Library/LMC_API/Generate-ApiUserManualDocx.py' 'LMC_Library/LMC_API_Delivery/bin/LasalMotionControlLib.dll' 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj' 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/Properties/AssemblyInfo.cs' 'LMC_Library/LMC_API_Distribution'
> git diff --cached --check
> git diff --cached --stat
> git commit -m "build(distribution): add standalone API package and manual"`
>
> </details>
>
> 마지막 배포 커밋 검사에서 Markdown의 의도적 줄바꿈 공백과 솔루션 파일의 여분 EOF 줄이 `diff --check`에 걸렸습니다. 아직 푸시 전이므로 해당 공백만 정리하고 마지막 커밋을 수정해 검증 기준을 맞추겠습니다.
>
> <details><summary>Edited 3 files, ran 2 commands</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Distribution/02_Example_Program/LasalApiWpfTestApp.sln` (+0 -1)
> - Wrote `./LMC_Library/LMC_API_Distribution/README.md` (+1 -1)
> - Ran `git add -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md' 'LMC_Library/LMC_API_Distribution/README.md' 'LMC_Library/LMC_API_Distribution/02_Example_Program/LasalApiWpfTestApp.sln'
> git diff --cached --check
> if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
> git commit --amend --no-edit`
> - Ran `git status -sb
> git status --porcelain
> git diff --check
> git diff --cached --check
> git log --oneline --decorate -5
> git fetch origin --prune
> git status -sb
> git log --oneline origin/main..HEAD`
>
> </details>
>
> 세 커밋 모두 생성됐고, fetch 후에도 `origin/main`과 충돌 없이 로컬이 정확히 3커밋 앞선 상태입니다. 작업 트리는 깨끗하며 이제 `main`을 원격에 푸시하겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git push origin main`
> - Ran `git status -sb
> git rev-parse HEAD
> git rev-parse origin/main
> git log -3 --oneline --decorate
> $memory='C:\Users\dreiv\.codex\memories\MEMORY.md'; $lines=Get-Content -LiteralPath $memory; for($i=394;$i -le 425;$i++){ '{0}: {1}' -f $i,$lines[$i-1] }`
>
> </details>
</details>

정리 후 `origin/main`에 푸시 완료했습니다. 작업 트리도 깨끗합니다.

커밋:

- `62fcd8d` — API 응답 데이터 보호 및 `0.9.1-preview` 반영
- `8dc04e0` — 9축·UNIT·개발/배포 문서 정리
