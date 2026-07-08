<!--
Source: docs/history/Elmo_Master_history_260624.md
Original lines: 1751-2000
Part: 8 of 10
-->

# Elmo_Master_history_260624 part 08

Source lines: 1751-2000

>
> <details><summary>Ran 6 commands</summary>
>
> - Ran `rg -n "MMC_MoveLinearAbsolute|MMC_GroupReadStatus|MMC_SetKinTransform|MMC_MoveAbsolute" tmp\api_pdf_extract\maestro.txt`
> - Ran `rg -n "^\s*[0-9]+(\.[0-9]+)+\.?\s+MMC_" tmp\api_pdf_extract\maestro.txt | Select-Object -First 80`
> - Ran `rg -n "^\s*[0-9]+(\.[0-9]+)+\.?\s+eSnet" tmp\api_pdf_extract\snet.txt | Select-Object -First 80`
> - Ran `rg -n "eSnetMove|eSnet.*Interpolation|eSnet.*Trigger|eSnet.*Capture" tmp\api_pdf_extract\snet.txt`
> - Ran `$env:PYTHONIOENCODING='utf-8'; @'
> from pathlib import Path
> import re, json
> base=Path('tmp/api_pdf_extract')
>
> def read_pages(name): return (base/f'{name}.txt').read_text(encoding='utf-8', errors='replace').split('\f')
>
> def clean(s): return re.sub(r'\s+', ' ', s).strip()
>
> # Parse SNET TOC lines from early pages
> snet_pages=read_pages('snet')
> maestro_pages=read_pages('maestro')
>
> def parse_entries(pages, prefix):
>     entries=[]; tops=[]
>     text='\n'.join(pages[:25 if prefix=='eSnet' else 30])
>     for raw in text.splitlines():
>         line=clean(raw)
>         if not line: continue
>         # top-level numbered chapter/section
>         mtop=re.match(r'^(\d+)\.\s+(.+?)\s+\.{3,}\s+(\d+)\s*$', line)
>         if mtop:
>             tops.append({'number':mtop.group(1), 'title':clean(mtop.group(2)), 'page':int(mtop.group(3))})
>         # function-ish entries
>         m=re.match(r'^(\d+(?:\.\d+)+)\.?\s+((?:eSnet|MMC_)[A-Za-z0-9_/()]+(?:\s*/\s*(?:eSnet|MMC_)[A-Za-z0-9_/()]+)?)\s+\.{3,}\s+(\d+)\s*$', line)
>         if m:
>             entries.append({'section':m.group(1), 'name':clean(m.group(2)), 'page':int(m.group(3)), 'line':line})
>     # Deduplicate by section,name,page preserving first
>     seen=set(); out=[]
>     for e in entries:
>         key=(e['section'],e['name'],e['page'])
>         if key not in seen:
>             seen.add(key); out.append(e)
>     return tops,out
>
> snet_tops,snet_entries=parse_entries(snet_pages,'eSnet')
> mae_tops,mae_entries=parse_entries(maestro_pages,'MMC_')
>
> # For Maestro include headings from entire doc not just TOC, but only section lines and unique names
> heading_re=re.compile(r'^(\d+(?:\.\d+)+)\s+(MMC_[A-Za-z0-9_]+(?:/MMC_[A-Za-z0-9_]+)?)\s*$')
> for i,page in enumerate(maestro_pages):
>     for raw in page.splitlines():
>         line=clean(raw)
>         m=heading_re.match(line)
>         if m:
>             mae_entries.append({'section':m.group(1), 'name':m.group(2), 'page':i+1, 'line':line})
> # Dedup maestro keeping lower page from TOC if same section/name
> seen={}
> for e in mae_entries:
>     k=(e['section'], e['name'])
>     if k not in seen or e['page'] < seen[k]['page']:
>         seen[k]=e
> mae_entries=sorted(seen.values(), key=lambda e: [int(x) for x in e['section'].split('.')])
>
> # Categorize by first number or first two numbers
> def cat_snet(e):
>     major=e['section'].split('.')[0]
>     top=next((t['title'] for t in snet_tops if t['number']==major), '')
>     return f'{major}. {top}' if top else major
>
> def cat_mae(e):
>     parts=e['section'].split('.')
>     major=parts[0]
>     if major=='4': return '4. Error Handling'
>     if major=='6':
>         return '6. Single Axis Motion/Admin' if len(parts)>1 and parts[1]=='1' else '6. Single Axis auxiliary/status/parameter'
>     if major=='7':
>         if len(parts)>1:
>             return {
>               '7':'7. Multi-axis normalcy',
>               '9':'7. Group/path motion',
>               '10':'7. Group configuration/status/kinematics'
>             }.get(parts[1], '7. Multi-Axis')
>         return '7. Multi-Axis'
>     if major=='8': return '8. Tables'
>     if major=='9': return '9. User programs / services'
>     if major=='10': return '10. API Services / administrative'
>     if major=='11': return '11. Communication / system'
>     if major=='12': return '12. EtherCAT / configuration'
>     if major=='13': return '13. Utilities'
>     if major=='14': return '14. Recorder / diagnostics'
>     if major=='15': return '15. C# wrapper'
>     if major=='16': return '16. Python wrapper'
>     return major
>
> from collections import defaultdict
> cat_s=defaultdict(list); cat_m=defaultdict(list)
> for e in snet_entries: cat_s[cat_snet(e)].append(e)
> for e in mae_entries: cat_m[cat_mae(e)].append(e)
>
> # Extract prototypes for representative APIs
> reps_s=['eSnetConnect','eSnetMoveSingleEx','eSnetMoveLine','eSnetMoveArcRadius','eSnetStartTrigger','eSnetGetCapturePosition','eSnetGetAxisStatus','eSnetServoOn','eSnetReadActualPosition','eSnetEcatReadDomainRxData']
> reps_m=['MMC_RpcInitConnection','MMC_MoveAbsolute','MMC_ReadActualPosition','MMC_GroupReadStatus','MMC_MoveLinearAbsolute','MMC_SetKinTransform','MMC_TrackConveyorBelt','MMC_CreateTable','MMC_ECATWriteSdo','MMC_OpenUdpChannelCmdEx']
>
> def find_context(pages, name, width=3):
>     out=[]
>     pat=re.compile(re.escape(name))
>     for i,page in enumerate(pages):
>         lines=page.splitlines()
>         for idx,line in enumerate(lines):
>             if pat.search(line):
>                 lo=max(0,idx-width); hi=min(len(lines),idx+width+1)
>                 ctx=' | '.join(clean(x) for x in lines[lo:hi] if clean(x))
>                 out.append({'page':i+1,'context':ctx[:800]})
>                 if len(out)>=2: return out
>     return out
>
> result={
>  'snet': {'tops':snet_tops, 'entries':snet_entries, 'categories':{k:v for k,v in cat_s.items()}, 'contexts':{n:find_context(snet_pages,n) for n in reps_s}},
>  'maestro': {'tops':mae_tops, 'entries':mae_entries, 'categories':{k:v for k,v in cat_m.items()}, 'contexts':{n:find_context(maestro_pages,n) for n in reps_m}},
> }
> (base/'api_parsed.json').write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
>
> print('SNET tops', len(snet_tops), 'entries', len(snet_entries))
> for k,v in cat_s.items(): print('SNET', k, len(v))
> print('\nMAESTRO tops', len(mae_tops), 'entries', len(mae_entries))
> for k,v in cat_m.items(): print('MAESTRO', k, len(v))
> print('\nSNET sample entries:', snet_entries[:20])
> print('\nMAESTRO sample entries:', mae_entries[:80])
> '@ | python -`
> - Ran `$env:PYTHONIOENCODING='utf-8'; @'
> from pathlib import Path
> import json
> p=Path('tmp/api_pdf_extract/api_parsed.json')
> data=json.loads(p.read_text(encoding='utf-8'))
> for doc in ['snet','maestro']:
>     print('\n##', doc)
>     for cat, entries in data[doc]['categories'].items():
>         names=[e['name'] for e in entries]
>         print(f'{cat}: {len(names)}')
>         print(', '.join(names[:25]) + (' ...' if len(names)>25 else ''))
> '@ | python -`
>
> </details>
>
> 한 가지 정정합니다. Maestro에서 `MMC_*`를 단순 카운트하면 구조체명과 래퍼 코드가 같이 잡힙니다. 그래서 지금은 목차의 섹션 단위 API로 다시 분류했고, native Motion/Admin 쪽과 EtherCAT/통신/DS401/Elmo 드라이브 제어 쪽을 분리해서 정리하고 있습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "Chapter\s+[0-9]+|^\s*[0-9]+\.?\s+[A-Za-z].*\.\.\." tmp\api_pdf_extract\maestro.txt | Select-Object -First 220`
> - Ran `rg -n "^\s*[0-9]+\.\s+.*\.\.\." tmp\api_pdf_extract\snet.txt | Select-Object -First 120`
>
> </details>
>
> 비교표의 성능 항목은 정량 벤치마크가 아니라 “문서상 구조로 판단되는 실시간성/처리 방식”으로 표기하겠습니다. 두 PDF 모두 동일 조건의 ms/us 실측값을 제공하지 않아서, 숫자 비교를 만들면 근거 없는 자료가 됩니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Content -Path tmp\api_pdf_extract\maestro.txt -Encoding UTF8 -TotalCount 620 | Select-Object -Skip 480`
>
> </details>
>
> 목차 기준 주요 API 섹션은 SNET 300개, Maestro 329개로 잡혔습니다. 여기서 Maestro 24장 이후는 C++/IEC/Python 래퍼와 중복 설명이 섞이므로 비교표에는 핵심 기능 비교와 래퍼 지원 여부를 따로 분리하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Content -Path tmp\api_pdf_extract\maestro.txt -Encoding UTF8 -TotalCount 1120 | Select-Object -Skip 620`
>
> </details>
>
> 추가로 Maestro에는 `MMC_*`가 아닌 `Eip*` EtherNet/IP 함수가 있어서 이것도 비교 대상에 포함하겠습니다. 단순 prefix 검색만 하면 EtherNet/IP 지원이 누락됩니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$env:PYTHONIOENCODING='utf-8'; @'
> from pathlib import Path
> import re, json
> base=Path('tmp/api_pdf_extract')
> mae=(base/'maestro.txt').read_text(encoding='utf-8', errors='replace')
> # collect TOC function-like lines with any API family
> entries=[]
> for raw in mae.splitlines():
>     line=re.sub(r'\s+',' ',raw).strip()
>     m=re.match(r'^(\d+(?:\.\d+)+)\s+([A-Za-z_][A-Za-z0-9_()]+(?:/[A-Za-z_][A-Za-z0-9_()]+)?)\s+\.{3,}\s+(\d+)\s*$', line)
>     if not m: continue
>     name=m.group(2)
>     if re.match(r'^(MMC_|MC_|Eip|EIP|EmergencyEvent|HomeEnded|NodeInitEvent|NodeErrorEvent|StopOnLimit|GlobalAsyncReply|AsyncReplyEvent|FBNotifyEvent|PolicyEndedEvent)', name):
>         entries.append({'section':m.group(1),'name':name,'page':int(m.group(3)),'line':line})
> seen={};
> for e in entries:
>     k=(e['section'],e['name'])
>     if k not in seen: seen[k]=e
> entries=sorted(seen.values(), key=lambda e: [int(x) for x in e['section'].split('.')])
> print('total function-like TOC entries',len(entries))
> from collections import defaultdict
> by=defaultdict(list)
> for e in entries: by[e['section'].split('.')[0]].append(e)
> for k in sorted(by, key=lambda x:int(x)):
>     print(k, len(by[k]), ', '.join(e['name'] for e in by[k][:20]))
> (base/'maestro_toc_function_entries.json').write_text(json.dumps(entries,ensure_ascii=False,indent=2),encoding='utf-8')
> '@ | python -`
>
> </details>
>
> 이제 문서 파일 3개를 생성하겠습니다. 내용은 원본 PDF 페이지/목차 근거, 개별 API 분석, 그리고 한눈에 보는 기능/성능 비교표로 나눕니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `New-Item -ItemType Directory -Force -Path docs\api_analysis | Out-Null`
> - Ran ``$env:PYTHONIOENCODING='utf-8'; @'
> from pathlib import Path
> import json, re, textwrap
> from collections import defaultdict
>
> root=Path('.').resolve()
> out=Path('docs/api_analysis')
> base=Path('tmp/api_pdf_extract')
> parsed=json.loads((base/'api_parsed.json').read_text(encoding='utf-8'))
> mae_eip=json.loads((base/'maestro_toc_function_entries.json').read_text(encoding='utf-8'))
>
> def md_escape(s):
>     return str(s).replace('|','\\|')
>
> def code_list(names, limit=None):
>     if limit is not None and len(names)>limit:
>         show=names[:limit]
>         return ', '.join(f'`{md_escape(n)}`' for n in show)+f', ... 외 {len(names)-limit}개'
>     return ', '.join(f'`{md_escape(n)}`' for n in names)
>
> def write(path, text):
>     path.write_text(text.strip()+"\n", encoding='utf-8', newline='\n')
>
> snet_title_map={
>  '6':'로그 정보 남기기','8':'축 파라미터 설정','22':'위치/속도 Override','24':'겐트리 동기 구동','25':'겐트리 원점 검색','31':'입/출력 제어 (SNET-RTEX-IO Slave)','39':'Trigger 출력 (SNET-ECAT) - 특정 위치 트리거'
> }
> # Normalize snet categories
> snet_groups=[]
> for cat, entries in parsed['snet']['categories'].items():
>     major=cat.split('.')[0]
>     title=cat
>     if cat.strip().isdigit(): title=f'{major}. {snet_title_map.get(major, cat)}'
>     snet_groups.append((int(major), title, entries))
> snet_groups=sorted(snet_groups, key=lambda x:x[0])
>
> chapter_titles={
>  '4':'Error Handling','5':'Motion/Admin Description','6':'Motion/Admin - Single Axis','7':'Motion/Admin - Multi-Axis','8':'Position, Velocity, Time (PVT) Motion','9':'Electronic CAM','10':'API Services and Operations','11':'Process Image (PI)','12':'Data Recording','13':'Bulk Parameters Reading','14':'API Events (C/C++)','15':'Error Correction Mechanism','16':'Saving Maestro User Program Parameters','17':'Network Connectivity and Configuration','18':'Host Communication / Modbus','19':'CANbus Drive Communication','20':'DS-401 CANbus I/O Communication','21':'EtherCAT Drive Communication','22':'Interpreter Command Functions','23':'EtherNet/IP Communication','24':'Programming in C++ wrapper','25':'IEC 61131-3 Special Functions','26':'Python Functions wrapper'
> }
> # Re-categorize Maestro from parsed entries
> maestro_entries=parsed['maestro']['entries'][:]
> # Add EIP entries that are not in MMC parsed list
> existing={(e['section'],e['name']) for e in maestro_entries}
> for e in mae_eip:
>     if e['section'].split('.')[0]=='23' and (e['section'],e['name']) not in existing:
