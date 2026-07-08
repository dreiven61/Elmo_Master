<!--
Source: docs/history/Elmo_Master_history_260624.md
Original lines: 1501-1750
Part: 7 of 10
-->

# Elmo_Master_history_260624 part 07

Source lines: 1501-1750

>             for it in items:
>                 if isinstance(it, list):
>                     walk(it, depth+1)
>                 else:
>                     title = getattr(it, 'title', str(it))
>                     outlines.append(('  '*depth)+title)
>         walk(reader.outline)
>     except Exception as e:
>         print('outline err', type(e).__name__, e)
>     print('outline count', len(outlines))
>     for line in outlines[:80]:
>         print('OUT', line)
>     with pdfplumber.open(str(path)) as pdf:
>         for i in range(min(8, len(pdf.pages))):
>             txt = pdf.pages[i].extract_text(x_tolerance=1, y_tolerance=3) or ''
>             print(f'--- page {i+1} text first 1200 ---')
>             print(txt[:1200].replace('\n','\n'))
> '@ | python -`
>
> </details>
>
> Maestro PDF는 콘솔 기본 인코딩 때문에 일부 문자에서 출력이 끊겼습니다. UTF-8 출력으로 다시 추출하고, 두 문서 전체에서 API 함수명 패턴을 수집하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$env:PYTHONIOENCODING='utf-8'; @'
> from pathlib import Path
> import pdfplumber, re, json
> from pypdf import PdfReader
> pdfs = {
>     'snet': Path(r'C:\work\자료\EMotion\SNET-ECAT-User-Manual-25.05.08-ko\SNET-ECAT User Manual 25.05.08 ko\Chapter6_Library_(250508).pdf'),
>     'maestro': Path(r'C:\work\Elmo\Elmo_Master\Maestro Administrative and Motion API_2022_12_v2.012.pdf'),
> }
> for name,path in pdfs.items():
>     print('\n===', name, '===')
>     reader = PdfReader(str(path))
>     print('pages', len(reader.pages))
>     outlines=[]
>     page_lookup={id(page): i+1 for i,page in enumerate(reader.pages)}
>     try:
>         def walk(items, depth=0):
>             for it in items:
>                 if isinstance(it, list):
>                     walk(it, depth+1)
>                 else:
>                     title=getattr(it,'title',str(it))
>                     try:
>                         p=reader.get_destination_page_number(it)+1
>                     except Exception:
>                         p=None
>                     outlines.append((depth,title,p))
>         walk(reader.outline)
>     except Exception as e:
>         print('outline err', type(e).__name__, e)
>     print('outline count', len(outlines))
>     for depth,title,p in outlines[:120]:
>         print('OUT', '  '*depth + title, 'p='+str(p))
>     with pdfplumber.open(str(path)) as pdf:
>         for i in range(min(12, len(pdf.pages))):
>             txt = pdf.pages[i].extract_text(x_tolerance=1, y_tolerance=3) or ''
>             print(f'--- page {i+1} first 700 ---')
>             print(txt[:700])
> '@ | python -`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> PDF 두 개를 로컬에서 직접 파싱해서 API 이름, 목차 구간, 기능 카테고리를 뽑고 있습니다. 먼저 자동 추출 결과로 큰 구조를 잡고, 그 다음 사람이 볼 수 있는 비교표 형태의 md 문서로 정리하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$env:PYTHONIOENCODING='utf-8'; @'
> from pathlib import Path
> from pypdf import PdfReader
> import re, json
>
> out_dir = Path('tmp/api_pdf_extract')
> out_dir.mkdir(parents=True, exist_ok=True)
>
> docs = {
>     'snet': Path(r'C:/work/자료/EMotion/SNET-ECAT-User-Manual-25.05.08-ko/SNET-ECAT User Manual 25.05.08 ko/Chapter6_Library_(250508).pdf'),
>     'maestro': Path(r'C:/work/Elmo/Elmo_Master/Maestro Administrative and Motion API_2022_12_v2.012.pdf'),
> }
> patterns = {
>     'snet': re.compile(r'\beSnet[A-Za-z0-9_]+\b'),
>     'maestro': re.compile(r'\bMMC_[A-Za-z0-9_]+\b'),
> }
>
> def compact(s):
>     return re.sub(r'\s+', ' ', s).strip()
>
> def extract_outline(reader):
>     result = []
>     def walk(items, level=0):
>         for item in items:
>             if isinstance(item, list):
>                 walk(item, level+1)
>             else:
>                 try:
>                     title = compact(str(item.title))
>                 except Exception:
>                     title = compact(str(item))
>                 page = None
>                 try:
>                     page = reader.get_destination_page_number(item) + 1
>                 except Exception:
>                     pass
>                 result.append({'level': level, 'title': title, 'page': page})
>     try:
>         walk(reader.outline)
>     except Exception:
>         pass
>     return result
>
> summary = {}
> for key, path in docs.items():
>     reader = PdfReader(str(path))
>     name_re = patterns[key]
>     funcs = {}
>     toc_lines = []
>     chapter_lines = []
>     feature_hits = {}
>     terms = ['Group', 'Gantry', 'Interpolation', 'Trigger', 'Capture', 'MPG', 'ECAT', 'EtherCAT', 'Home', 'Homing', 'Override', 'Rollover', 'Table', 'PVT', 'ECAM', 'Kinematics', 'Transform', 'IO', 'Remote IO', 'ADC', 'DAC', 'DS402', 'Position', 'Velocity', 'Blend', 'Transition', 'InPosition', 'In-position']
>     for i, page in enumerate(reader.pages):
>         pnum = i + 1
>         try:
>             text = page.extract_text() or ''
>         except Exception as e:
>             text = ''
>         lines = [compact(x) for x in text.splitlines() if compact(x)]
>         if pnum <= (60 if key == 'snet' else 80):
>             for line in lines:
>                 if re.search(r'\b(Chapter|목차|[0-9]+\.|MMC_|eSnet)', line, re.I):
>                     toc_lines.append({'page': pnum, 'line': line[:300]})
>                 if re.search(r'Chapter\s+\d+|^\d+\s+', line, re.I):
>                     chapter_lines.append({'page': pnum, 'line': line[:300]})
>         for term in terms:
>             if re.search(r'\b' + re.escape(term) + r'\b', text, re.I):
>                 feature_hits.setdefault(term, []).append(pnum)
>         for m in name_re.finditer(text):
>             name = m.group(0)
>             info = funcs.setdefault(name, {'name': name, 'first_page': pnum, 'count': 0, 'contexts': []})
>             info['count'] += 1
>             if pnum < info['first_page']:
>                 info['first_page'] = pnum
>             if len(info['contexts']) < 3:
>                 start = max(0, text.rfind('\n', 0, m.start()))
>                 end = text.find('\n', m.end())
>                 if end == -1:
>                     end = min(len(text), m.end()+160)
>                 ctx = compact(text[start:end])
>                 if ctx and ctx not in info['contexts']:
>                     info['contexts'].append(ctx[:300])
>     funcs_sorted = sorted(funcs.values(), key=lambda x: (x['first_page'], x['name']))
>     outline = extract_outline(reader)
>     data = {
>         'path': str(path),
>         'pages': len(reader.pages),
>         'metadata': {str(k): str(v) for k, v in (reader.metadata or {}).items()},
>         'outline': outline,
>         'functions': funcs_sorted,
>         'toc_lines': toc_lines[:1000],
>         'chapter_lines': chapter_lines[:300],
>         'feature_hits': {k: {'count_pages': len(v), 'first_pages': v[:20]} for k, v in sorted(feature_hits.items())},
>     }
>     (out_dir / f'{key}_extract.json').write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding='utf-8')
>     summary[key] = {
>         'pages': len(reader.pages),
>         'function_count': len(funcs_sorted),
>         'first_functions': funcs_sorted[:25],
>         'feature_hits': data['feature_hits'],
>         'outline_first': outline[:80],
>     }
>
> print(json.dumps(summary, ensure_ascii=False, indent=2)[:20000])
> '@ | python -`
>
> </details>
</details>

> 작업 완료됐어? 아님 진행중이냐

<details><summary>22 previous messages</summary>

> 진행 중입니다. 방금 전체 PDF 파싱 명령이 오래 걸려서 중단된 상태라, 방식만 바꿔서 필요한 범위부터 먼저 뽑겠습니다. 2435페이지짜리 Maestro 문서는 전체 텍스트를 한 번에 긁는 방식보다 목차/함수명 중심으로 나눠서 추출하는 게 맞습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `Get-Command pdftotext -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source`
> - Ran `Get-Command rg -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source`
> - Ran `Get-Command pdfinfo -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source`
>
> </details>
>
> `pdftotext`가 있어서 그걸로 전환합니다. 이 방식은 PDF 전체 텍스트 변환이 훨씬 빠르고, 변환된 텍스트에서 API명과 목차를 정규식으로 뽑아 분석 문서를 만들 수 있습니다.
>
> <details><summary>Ran 7 commands</summary>
>
> - Ran `New-Item -ItemType Directory -Force -Path tmp\api_pdf_extract | Out-Null`
> - Ran `pdftotext -layout -enc UTF-8 "C:\work\Elmo\Elmo_Master\Maestro Administrative and Motion API_2022_12_v2.012.pdf" "tmp\api_pdf_extract\maestro.txt"`
> - Ran `pdftotext -layout -enc UTF-8 "C:\work\자료\EMotion\SNET-ECAT-User-Manual-25.05.08-ko\SNET-ECAT User Manual 25.05.08 ko\Chapter6_Library_(250508).pdf" "tmp\api_pdf_extract\snet.txt"`
> - Ran `pdfinfo "C:\work\자료\EMotion\SNET-ECAT-User-Manual-25.05.08-ko\SNET-ECAT User Manual 25.05.08 ko\Chapter6_Library_(250508).pdf"`
> - Ran `pdfinfo "C:\work\Elmo\Elmo_Master\Maestro Administrative and Motion API_2022_12_v2.012.pdf"`
> - Ran `Get-ChildItem tmp\api_pdf_extract\*.txt | Select-Object Name,Length`
> - Ran `$env:PYTHONIOENCODING='utf-8'; @'
> from pathlib import Path
> import re, json
> base=Path('tmp/api_pdf_extract')
> texts={k:(base/f'{k}.txt').read_text(encoding='utf-8', errors='replace').split('\f') for k in ['snet','maestro']}
> patterns={'snet':re.compile(r'\beSnet[A-Za-z0-9_]+\b'), 'maestro':re.compile(r'\bMMC_[A-Za-z0-9_]+\b')}
> summary={}
> terms=['Group','Gantry','Interpolation','interpolation','Trigger','Capture','MPG','ECAT','EtherCAT','Home','Homing','Override','Rollover','Table','PVT','ECAM','Kinematics','kinematic','Transform','IO','Remote IO','ADC','DAC','DS402','Position','Velocity','Blend','blending','Transition','InPosition','In-position','Modulo','Cam','Gear','Conveyor','Rotary','Firmware','Download','Recorder','Latch','Gantry']
> for key,pages in texts.items():
>     funcs={}
>     page_lines=[]
>     for i,text in enumerate(pages):
>         p=i+1
>         lines=[re.sub(r'\s+',' ',l).strip() for l in text.splitlines() if re.sub(r'\s+',' ',l).strip()]
>         if (key=='snet' and p<=25) or (key=='maestro' and p<=35):
>             page_lines.append({'page':p,'lines':lines[:80]})
>         for m in patterns[key].finditer(text):
>             name=m.group(0)
>             rec=funcs.setdefault(name, {'name':name,'first_page':p,'count':0,'contexts':[]})
>             rec['count'] += 1
>             start=text.rfind('\n',0,m.start()); end=text.find('\n',m.end())
>             if end<0: end=min(len(text),m.end()+200)
>             ctx=re.sub(r'\s+',' ',text[start+1:end]).strip()
>             if ctx and ctx not in rec['contexts'] and len(rec['contexts'])<2:
>                 rec['contexts'].append(ctx[:240])
>     hits={}
>     for term in terms:
>         pageshit=[]
>         rg=re.compile(re.escape(term), re.I)
>         for i,text in enumerate(pages):
>             if rg.search(text): pageshit.append(i+1)
>         if pageshit: hits[term]={'count_pages':len(pageshit),'first_pages':pageshit[:20]}
>     funcs_sorted=sorted(funcs.values(), key=lambda r:(r['first_page'], r['name']))
>     summary[key]={'pages':len(pages), 'func_count':len(funcs_sorted), 'functions':funcs_sorted[:60], 'hits':hits, 'toc_sample':page_lines[:20]}
>     (base/f'{key}_functions.json').write_text(json.dumps(funcs_sorted, ensure_ascii=False, indent=2), encoding='utf-8')
>     (base/f'{key}_summary.json').write_text(json.dumps({'pages':len(pages),'func_count':len(funcs_sorted),'hits':hits,'toc_sample':page_lines}, ensure_ascii=False, indent=2), encoding='utf-8')
> print(json.dumps({k:{'pages':v['pages'],'func_count':v['func_count'],'first_functions':v['functions'][:30], 'hits':v['hits']} for k,v in summary.items()}, ensure_ascii=False, indent=2)[:30000])
> '@ | python -`
>
> </details>
>
> 추출 결과가 나왔습니다. 현재 기준으로 SNET 문서에서는 `eSnet*` API 371개, Maestro 문서에서는 `MMC_*` 심볼이 2501개 잡혔는데, Maestro 쪽은 구조체/파라미터형 심볼까지 섞여 있어서 실제 호출 API 기준으로 한 번 더 걸러내겠습니다.
