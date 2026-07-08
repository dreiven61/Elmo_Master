> - Ran `git remote -v`
>
> </details>
>
> 상태가 매우 큽니다. 기존 파일 삭제와 새 기능별 폴더 추가가 섞여 있고, `Reports`, `test`, `Lasal_PRG`, `LMC_Library`, `docs` 쪽으로 재배치된 것으로 보입니다. 이제 전체 변경을 기계적으로 분류해서 무엇을 Git에 올릴지 확정하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$env:PYTHONIOENCODING='utf-8'
> @'
> import subprocess, os
> from collections import Counter, defaultdict
> raw = subprocess.check_output(['git','status','--porcelain=v1','-z','--untracked-files=all'])
> items = raw.decode('utf-8', errors='replace').split('\0')
> records=[]
> i=0
> while i < len(items):
>     item=items[i]
>     i+=1
>     if not item:
>         continue
>     status=item[:2]
>     path=item[3:]
>     if status[0] in 'RC' or status[1] in 'RC':
>         if i < len(items):
>             path2=items[i]; i+=1
>             records.append((status, path, path2))
>         else:
>             records.append((status, path, None))
>     else:
>         records.append((status, path, None))
>
> def ext(path):
>     base=os.path.basename(path)
>     if '.' not in base:
>         return '[no ext]'
>     return os.path.splitext(base)[1].lower()
>
> def top(path):
>     return path.split('/')[0]
>
> print('TOTAL_RECORDS', len(records))
> print('\nBY_STATUS')
> for k,v in Counter(s for s,_,__ in records).most_common():
>     print(f'{k!r} {v}')
> print('\nBY_TOP')
> for k,v in Counter(top(p2 or p) for s,p,p2 in records).most_common(40):
>     print(f'{k} {v}')
> print('\nBY_EXTENSION')
> for k,v in Counter(ext(p2 or p) for s,p,p2 in records).most_common(60):
>     print(f'{k} {v}')
> print('\nUNTRACKED_TOP')
> for k,v in Counter(top(p) for s,p,p2 in records if s == '??').most_common(40):
>     print(f'{k} {v}')
> print('\nDELETED_TOP')
> for k,v in Counter(top(p) for s,p,p2 in records if 'D' in s).most_common(40):
>     print(f'{k} {v}')
> '@ | python -`
>
> </details>
>
> 스캔 결과는 총 1,809건입니다. 핵심은 `Elmo_EtherCAT_Test_4Axis` 쪽 기존 tracked 파일이 삭제되고, `Lasal_PRG/...` 아래로 1,004개가 새로 들어온 재배치입니다. 문서/테스트/캡처도 `docs`, `test`, `LMC_Library`로 정리되어 있습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `git ls-files --deleted | Measure-Object | Select-Object -ExpandProperty Count`
> - Ran `Get-Content -LiteralPath '.gitignore'`
> - Ran `git ls-files --others --exclude-standard | Measure-Object | Select-Object -ExpandProperty Count`
> - Ran `git status --ignored --short --untracked-files=all | Select-Object -First 220`
>
> </details>
>
> `.gitignore`는 `*.pcapng`, `*.xlsx`, `*.lba`, `*.lob` 같은 생성물/캡처를 기본 제외합니다. 현재 status에 보이는 삭제는 “이미 Git에 추적되던 파일”이라 삭제로 올라가고, 새로 추가할 파일은 ignore 규칙을 따른 1,124개입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$env:PYTHONIOENCODING='utf-8'
> @'
> import subprocess, os
> from collections import Counter
> raw = subprocess.check_output(['git','status','--porcelain=v1','-z','--untracked-files=all'])
> parts=raw.decode('utf-8', errors='replace').split('\0')
> recs=[]; i=0
> while i<len(parts):
>     item=parts[i]; i+=1
>     if not item: continue
>     s=item[:2]; p=item[3:]
>     recs.append((s,p))
>
> def ext(p):
>     b=os.path.basename(p)
>     return os.path.splitext(b)[1].lower() if '.' in b else '[no ext]'
> for status in ['??',' D',' M']:
>     print('STATUS', repr(status), 'COUNT', sum(1 for s,p in recs if s==status))
>     for k,v in Counter(ext(p) for s,p in recs if s==status).most_common(30):
>         print(f'  {k} {v}')
> '@ | python -`
>
> </details>
>
> 새 `WireShark` 캡처 원본 `.pcapng`들은 ignore 때문에 일반 `git add -A`로는 안 올라갑니다. 대신 같은 폴더의 TXT 분석본과 `LMC_PACKET_MAP.md` 같은 문서는 올라갑니다. 원본 캡처까지 저장소에 넣을지 여부가 설계상 갈리는 지점입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$env:PYTHONIOENCODING='utf-8'
> @'
> import subprocess, os
> from collections import Counter
> try:
>     out = subprocess.check_output(['git','ls-files','--others','--ignored','--exclude-standard'], text=True, encoding='utf-8', errors='replace')
> except subprocess.CalledProcessError as e:
>     out = e.output
> paths=[p for p in out.splitlines() if p]
> print('IGNORED_UNTRACKED_COUNT', len(paths))
> print('BY_TOP')
> for k,v in Counter(p.split('/')[0] for p in paths).most_common(30):
>     print(k, v)
> print('BY_EXT')
> for k,v in Counter(os.path.splitext(os.path.basename(p))[1].lower() if '.' in os.path.basename(p) else '[no ext]' for p in paths).most_common(30):
>     print(k, v)
> print('SAMPLE')
> for p in paths[:80]:
>     print(p)
> '@ | python -`
>
> </details>
>
> 원본 캡처까지 포함하려면 `-f`로 특정 경로만 강제 추가해야 합니다. 전체 ignore 파일을 강제 추가하면 `bin/obj/.vs`와 LASAL 생성물이 대량으로 들어가므로, 강제 추가는 `LMC_Library/.../WireShark/*.pcapng` 같은 의도된 캡처 원본으로만 제한해야 합니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `git ls-files --others --ignored --exclude-standard -- 'LMC_Library/LMC_API/Elmo_API_Packet2/WireShark'`
> - Ran `git ls-files --others --ignored --exclude-standard -- 'LMC_Library'`
> - Ran `git ls-files --others --exclude-standard -- 'LMC_Library'`
> - Ran `git ls-files --others --ignored --exclude-standard -- '*.pcapng'`
>
> </details>
>
> 재배치된 캡처/리포트 원본도 보존하려면 ignored `.pcapng` 44개와 `.xlsx` 일부를 강제 추가해야 합니다. 사용자가 “다 올려”라고 했기 때문에, `bin/obj/.vs` 같은 빌드 산출물은 계속 제외하고, 재배치된 실험 원본(`test/**`, `LMC_Library/**/WireShark/**`)만 선별 강제 추가하는 방향으로 진행하겠습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `git ls-files --others --ignored --exclude-standard -- 'test/**/*.xlsx' 'test/**/*.pcapng' 'test/**/*.pdf' 'test/**/*.zip'`
> - Ran `git ls-files --others --ignored --exclude-standard -- 'LMC_Library/LMC_API/Elmo_API_Packet2/WireShark/*.pcapng' 'LMC_Library/LMC_API/LMC_API/bin/*' 'LMC_Library/*.zip'`
> - Ran `git ls-files --others --ignored --exclude-standard -- 'Lasal_PRG/**/*.lba' 'Lasal_PRG/**/*.lob' 'Lasal_PRG/**/*.lhd' 'Lasal_PRG/**/ProjectInternal/**' | Measure-Object | Select-Object -ExpandProperty Count`
>
> </details>
>
> 작업 브랜치를 새로 만들고, 분류 스캔 결과를 문서로 남긴 뒤 staging 하겠습니다. 강제 추가는 재배치된 실험 원본과 LMC 전달 DLL에만 제한하고, 빌드 산출물/IDE 캐시/중복 zip은 제외합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git switch -c codex/reorganize-functional-folders-20260708`
>
> </details>
>
> 브랜치는 `codex/reorganize-functional-folders-20260708`로 분리했습니다. 이제 재스캔 결과와 포함/제외 기준을 repo 문서에 짧게 남깁니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./docs/reorganization_file_inventory_2026-07-08.md` (+41 -0)
>
> </details>
>
> 이제 전체 재배치 결과를 staging 합니다. 기본 `git add -A` 후, ignore 규칙 때문에 빠지는 캡처/리포트 원본은 지정된 경로만 `-f`로 추가합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git add -A -- .
> git add -f -- ':(glob)LMC_Library/LMC_API/Elmo_API_Packet2/WireShark/*.pcapng' 'LMC_Library/LMC_API/LMC_API/bin/LmcMotionApi.dll' ':(glob)test/**/*.pcapng' ':(glob)test/**/*.xlsx'`
>
> </details>
>
> Staging은 완료됐고 줄바꿈 경고가 많이 나왔습니다. 새로 추가된 텍스트류는 Git 설정상 LF/CRLF 정규화 경고가 뜬 것이고, 실패는 아닙니다. 이제 staged 상태와 남은 untracked/ignored를 확인합니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `git status --short --untracked-files=all | Select-Object -First 260`
> - Ran `git diff --cached --stat`
> - Ran `$env:PYTHONIOENCODING='utf-8'
> @'
> import subprocess, os
> from collections import Counter
> raw = subprocess.check_output(['git','diff','--cached','--name-status','-z'])
> parts=raw.decode('utf-8', errors='replace').split('\0')
> recs=[]; i=0
> while i < len(parts):
>     s=parts[i]; i+=1
>     if not s: continue
>     if s.startswith('R') or s.startswith('C'):
>         old=parts[i]; new=parts[i+1]; i+=2
>         recs.append((s, new))
>     else:
>         p=parts[i]; i+=1
>         recs.append((s,p))
> print('STAGED_RECORDS', len(recs))
> print('BY_STATUS')
> for k,v in Counter(s.split('\t')[0] for s,p in recs).most_common(40): print(k,v)
> print('BY_TOP')
> for k,v in Counter(p.split('/')[0] for s,p in recs).most_common(40): print(k,v)
> print('BY_EXT')
> for k,v in Counter(os.path.splitext(os.path.basename(p))[1].lower() if '.' in os.path.basename(p) else '[no ext]' for s,p in recs).most_common(60): print(k,v)
> '@ | python -`
>
> </details>
>
> Staged 결과는 1,484개입니다. Git이 427개를 rename으로 인식했고, 새 기능별 위치 추가와 기존 위치 삭제가 같이 잡혔습니다. 커밋 전에 `git diff --check`로 공백/라인 문제를 확인합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git diff --cached --check`
> - Ran `$env:PYTHONIOENCODING='utf-8'
> @'
> import subprocess
> raw = subprocess.check_output(['git','status','--porcelain=v1','-z','--untracked-files=all'])
> parts=raw.decode('utf-8', errors='replace').split('\0')
> unstaged=[]; untracked=[]; staged=[]
> i=0
> while i < len(parts):
>     item=parts[i]; i+=1
>     if not item: continue
>     xy=item[:2]; path=item[3:]
>     if xy[0] != ' ' and xy != '??': staged.append((xy,path))
>     if xy[1] != ' ' and xy != '??': unstaged.append((xy,path))
>     if xy == '??': untracked.append(path)
> print('staged_entries', len(staged))
> print('unstaged_entries', len(unstaged))
> print('untracked_entries', len(untracked))
> if unstaged:
>     print('UNSTAGED_SAMPLE')
>     for xy,p in unstaged[:20]: print(xy,p)
> if untracked:
>     print('UNTRACKED_SAMPLE')
>     for p in untracked[:20]: print(p)
> '@ | python -`
>
> </details>
</details>

`git diff --check`가 실패했습니다. 원인은 새로 들어온 LASAL 생성 `.st` 파일들의 기존 trailing whitespace / space-before-tab와 TXT 분석본의 EOF 빈 줄입니다. 내용 변경이 아니라 공백 정규화라서, text 파일만 대상으로 후행 공백과 EOF 빈 줄을 정리한 뒤 다시 검사하겠습니다.

> 지금 생각하고있냐?

> 지금 폴더를 기능별로 다 재배치했으니 폴더안에 있는 파일들을 다 재 스캔해서 종류별로 정리해서 Git에 올려
>
> 위에 하던거 이어서 계속 진행해
