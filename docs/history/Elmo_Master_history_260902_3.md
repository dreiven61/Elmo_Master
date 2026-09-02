# Estimate ctrlX porting effort

> 우리가 개발해놓은 시스템을 다른 시스템에 포팅한다고 하면 얼마나 걸릴까? 예를들어 보쉬 ctrlX 로 옮긴다고하면 얼마쯤 걸릴지 산정해봐.
> 개발 문서는 C:\Users\dreiv\source\repos\BOSCH\CtrlX\PLC Engineering\GUDEL에 개발문서들을 참고해서 만든다고 친다고 하면
> 이번 내용은 실제로 개발 문서가 필요한게 아니라 일정을 산정해보고 싶어서 대략적인 일정 맨데이가 필요해서 요청하는거야. 분석한걸 문서로 만들 필요는 없어.

<details><summary>7 previous messages</summary>

> 기존 Elmo 시스템의 실제 구현 범위와 Bosch ctrlX 쪽 개발 자료를 각각 확인한 뒤, PC/PLC 이식·통합·실기 시운전을 분리해 맨데이 범위로 산정하겠습니다. 결과만 대화로 정리하고 별도 문서는 만들지 않겠습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n -i "Elmo_Master|ctrlX|port|EventMask|SetPosition|Gate D|GD-01|GD-04" "C:\Users\dreiv\.codex\memories\MEMORY.md"`
> - Ran `$p='C:\Users\dreiv\.codex\memories\MEMORY.md'; $lines=Get-Content -LiteralPath $p; for($i=0;$i -lt [Math]::Min(58,$lines.Count);$i++){ '{0}:{1}' -f ($i+1),$lines[$i] }`
> - Ran `$p='C:\Users\dreiv\.codex\memories\rollout_summaries\2026-08-12T08-29-42-yBIn-elmo_master_test_plan_eventmask_and_live_gates.md'; if(Test-Path -LiteralPath $p){$lines=Get-Content -LiteralPath $p; for($i=0;$i -lt $lines.Count;$i++){ '{0}:{1}' -f ($i+1),$lines[$i] }} else {'NOT_FOUND'}`
>
> </details>
>
> 산정은 “소스가 컴파일되는 수준”과 “ctrlX 실장비에서 축이 안전하게 동작하고 패킷·콜백까지 검증된 수준”을 같은 완료로 보지 않겠습니다. 특히 현재 Elmo 쪽 미완료 항목은 포팅 공수에 그대로 전가되지 않도록, 원천 기능 완성 공수와 순수 포팅 공수를 나눠 보겠습니다.
>
> <details><summary>Explored 2 files, ran 18 commands</summary>
>
> - Ran `git status --short --branch; git log -1 --date=iso --pretty=format:'%h %ad %s'`
>
> - Ran `Get-ChildItem -LiteralPath . -Force | Select-Object Mode,LastWriteTime,Length,Name | Format-Table -AutoSize`
>
> - Ran `$p='C:\Users\dreiv\source\repos\BOSCH\CtrlX\PLC Engineering\GUDEL'; if(Test-Path -LiteralPath $p){ Get-ChildItem -LiteralPath $p -Force | Select-Object Mode,LastWriteTime,Length,Name | Format-Table -AutoSize; 'FILES'; rg --files $p | Select-Object -First 300 } else { 'NOT_FOUND' }`
>
> - Ran `'ROOT'; Get-Content -LiteralPath 'README.md' -Raw; 'SRC'; Get-Content -LiteralPath 'src\README.md' -Raw`
>
> <details><summary>Explored a file</summary>
>
> - Read `./AGENTS.md`
>
> </details>
>
> - Ran `rg -n "^#|^##|^###|Current|현재|PASS|NO-GO|미완|23|Axis|TCP|Callback|EventMask|SetPosition" 'docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md' 'README.md' 'docs\PMAS_LASAL_Integrated_Analysis_2026-04-10.md'`
>
> - Ran `rg -n "^#|^##|^###|Current|현재|미완|검증|PASS|NO-GO|AxisInterface|ModeGantry|EtherCAT|Modbus|ctrlX|SoftMotion|Motion|limit|제약|risk|위험" 'docs\gantry-project-current-state-and-document-audit-2026-07-16.md' 'docs\gantry-controller-architecture-ko.md' 'docs\gantry-controller-implementation-notes-ko.md' 'docs\gantry-protocol-v2-runtime-notes-ko.md' 'docs\ctrlx-plc-git-workflow.md'`
>
> - Ran `Get-Content -LiteralPath 'README.md' -Raw; '---API README---'; Get-Content -LiteralPath 'LMC_Library\LMC_API_Delivery\README.md' -Raw`
>
> - Ran `rg -n "^##|^###" 'docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'`
>
> - Ran `$p='docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'; $l=Get-Content -LiteralPath $p; $ranges=@(@(575,697),@(698,814),@(815,980),@(1148,1296),@(1374,1537)); foreach($x in $ranges){ "---$($x[0])-$($x[1])---"; for($i=$x[0]-1;$i -lt [Math]::Min($x[1],$l.Count);$i++){ '{0}:{1}' -f ($i+1),$l[$i] } }`
>
> <details><summary>Explored a file</summary>
>
> - Read `./docs/gantry-project-current-state-and-document-audit-2026-07-16.md`
>
> </details>
>
> - Ran `git status --short --branch; git log -1 --date=iso --pretty=format:'%h %ad %s'`
>
> - Ran `$roots=@('Codex_PMAS_WPF','LMC_Library\LMC_API_Delivery\src','LMC_Library\LasalApiWpfTestApp','Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class','Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network','Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Include'); foreach($root in $roots){$fs=Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue | Where-Object {$_.Extension -in '.cs','.xaml','.st','.h','.cpp','.c','.txt'}; $lines=0; foreach($f in $fs){$lines+=(Get-Content -LiteralPath $f.FullName -ErrorAction SilentlyContinue | Measure-Object -Line).Lines}; [pscustomobject]@{Root=$root;Files=$fs.Count;Lines=$lines}} | Format-Table -AutoSize`
>
> - Ran `rg -n -i "ctrlX PLC Engineering|ctrlX CORE|ctrlX3|AxisInterface|library version|라이브러리 버전|version|firmware|SoftMotion|PLCopen|Motion task|EtherCAT" README.md src docs -g '*.md' -g '*.st' | Select-Object -First 500`
>
> - Ran `$roots=@('src\GVL','src\POUs','src\Types','src\ModbusTestTool'); $out=foreach($root in $roots){$fs=Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue | Where-Object {$_.Extension -in '.cs','.xaml','.st'}; $lines=0; foreach($f in $fs){$lines+=(Get-Content -LiteralPath $f.FullName -ErrorAction SilentlyContinue | Measure-Object -Line).Lines}; [pscustomobject]@{Root=$root;Files=$fs.Count;Lines=$lines}}; $out | Format-Table -AutoSize`
>
> - Ran `$roots=@('src\GVL','src\POUs','src\Types','src\ModbusTestTool'); $out=foreach($root in $roots){$fs=@(Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue | Where-Object {$_.Extension -in '.cs','.xaml','.st' -and $_.FullName -notmatch '\\(bin|obj|\.vs)\\'}); $lines=0; foreach($f in $fs){$lines+=(Get-Content -LiteralPath $f.FullName -ErrorAction SilentlyContinue | Measure-Object -Line).Lines}; [pscustomobject]@{Root=$root;Files=$fs.Count;Lines=$lines}}; $out | Format-Table -AutoSize`
>
> - Ran `$roots=@('Codex_PMAS_WPF','LMC_Library\LMC_API_Delivery\src','LMC_Library\LasalApiWpfTestApp','Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class','Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network','Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Include'); $out=foreach($root in $roots){$fs=@(Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue | Where-Object {$_.Extension -in '.cs','.xaml','.st','.h','.cpp','.c','.txt' -and $_.FullName -notmatch '\\(bin|obj|\.vs|Reports)\\'}); $lines=0; foreach($f in $fs){$lines+=(Get-Content -LiteralPath $f.FullName -ErrorAction SilentlyContinue | Measure-Object -Line).Lines}; [pscustomobject]@{Root=$root;Files=$fs.Count;Lines=$lines}}; $out | Format-Table -AutoSize`
>
> - Ran `if(Test-Path -LiteralPath 'docs\history\260819\99_analysis_summary.md'){Get-Content -LiteralPath 'docs\history\260819\99_analysis_summary.md' -Raw}else{'NOT_FOUND'}`
>
> - Ran `rg -n "method-size|Method budget|STOP|NO-GO|Download|runtime|SetPosition|Classes.lcb|PASS" 'docs\history\260819' 'docs\architecture\AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md' | Select-Object -First 500`
>
> - Ran `git diff --stat; git diff -- 'docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md' | Select-Object -First 500`
>
> </details>
>
> GUDEL 자료를 보면 ctrlX 쪽에 이미 6축 AxisInterface, `ModeGantry`, Modbus 서버, 4 ms 주기 운전 골격이 있습니다. 따라서 완전 신규 개발보다는 유리하지만, Elmo의 74개 wire 명령·세션/콜백·진단 계층은 GUDEL의 현재 Modbus 제어 구조와 직접 호환되지 않아 별도 어댑터/백엔드 구현으로 잡아야 합니다.
>
> <details><summary>Explored 4 files, ran 3 commands, searched the web</summary>
>
> Searched the web for `site:docs.automation.boschrexroth.com ctrlX PLC Engineering PLC Libraries Version 3 AxisInterface ...`
>
> <details><summary>Explored 3 files</summary>
>
> - Read `./docs/plc-coding-rules.md`
> - Read `./docs/ctrlx-plc-git-workflow.md`
> - Read `./docs/gantry-controller-implementation-notes-ko.md`
>
> </details>
>
> - Ran `rg -n -i "TCPIP|socket|IL_Modbus|ModbusTCP|CXA_.*Socket|SysSocket|TCP server|raw tcp" src docs '매뉴얼' -g '*.st' -g '*.md' -g '*.txt' | Select-Object -First 500`
>
> Searched the web for `site:docs.automation.boschrexroth.com ctrlX PLC socket communication library TCP server CXA ...`
>
> - Ran `$p='docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'; $l=Get-Content -LiteralPath $p; for($i=698;$i -le 779;$i++){ '{0}:{1}' -f ($i+1),$l[$i] }`
>
> - Ran `$fs=@(Get-ChildItem -LiteralPath 'docs' -File -Filter '*.md' | Where-Object {$_.Name -notlike '~*'}); $lines=0; foreach($f in $fs){$lines+=(Get-Content -LiteralPath $f.FullName | Measure-Object -Line).Lines}; [pscustomobject]@{CurrentMarkdownFiles=$fs.Count;Lines=$lines}; $fs | Sort-Object Length -Descending | Select-Object -First 15 Name,Length | Format-Table -AutoSize`
>
> <details><summary>Explored a file</summary>
>
> - Read `./docs/history/260819/99_analysis_summary.md`
>
> </details>
>
> </details>
>
> 현재 범위를 그대로 호환시키는 기준에서는 단순 ST 변환이 아니라 “ctrlX용 PLC 백엔드 재구현”에 가깝습니다. 잠정 기준치는 약 190 MD이며, 기능을 핵심 모션 MVP로 줄이면 65~90 MD까지 낮출 수 있습니다. 남은 두 분석에서 재사용 범위와 위험 항목을 대조 중입니다.
</details>

결론부터 말하면, **Bosch ctrlX로 현재 활성 기능을 실장비 검증까지 포함해 포팅하면 기준 약 190 MD**가 현실적입니다.

- 내부 목표: **170~200 MD**
- 견적·대외 일정: 위험 여유 15% 포함 **약 220 MD**
- 1명: 약 **8.5~9개월**
- 2명 병렬: 약 **5~6개월**
- 핵심 모션 MVP만: **65~90 MD**

### 범위별 차이

| 완료 수준 | 예상 공수 |
|---|---:|
| 1축 기술 PoC | 5~10 MD |
| 4축 Power/Reset/Stop/Position/Abs/Rel/Velocity 중심 MVP | 65~90 MD |
| PLC 구현 및 PC/static 회귀까지 | 80~130 MD |
| 현재 활성 기능 전체 호환 + IDE/download/실축/packet/fault/soak | **170~200 MD** |
| 미완성 기능까지 활성화한 production 확대 | 220~300 MD 이상 |

여기서 “전체 호환”은 기존 TCP/UDP wire와 C# DLL/WPF를 유지하고, ctrlX PLC가 기존 SIGMATEK PLC처럼 동작하게 만드는 방식입니다.

현재 시스템은 C# command ID 74개, PLC route 73개, 25개 motion/group 명령, 4 physical + 5 simulated axis, D1~D5 진단, Recorder, SDO ticket, UDP callback까지 포함합니다. 단순 ST 문법 변환 수준이 아닙니다. 자세한 범위는 [현재 Elmo command matrix](</C:/work/Elmo/Elmo_Master/docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md:721>)에서 확인됩니다.

### 기준 189 MD의 구성

| 작업 | 기준 MD |
|---|---:|
| 현재 working tree 기준선 고정, 74 ID 호환표, 아키텍처 결정 | 8 |
| ctrlX 앱·라이브러리·Elmo ESI·EtherCAT·task 구성 및 PoC | 14 |
| TCP 4000, frame/queue, init/register/close, session/takeover | 18 |
| 단축 lookup/power/reset/stop/read/abs/rel/velocity | 22 |
| Group power/lock/reset/stop 및 4축 Cartesian abs/rel | 24 |
| Admin, D1~D5, Bulk/Recorder, topology/I/O, SDO, callback | 34 |
| 기존 C# DLL/WPF 호환 및 전체 회귀·안정화 | 20 |
| ctrlX IDE compile, project/task 확인, download, jitter bench | 16 |
| 실축·packet·fault·disconnect·callback·soak | 25 |
| GUDEL 기반 핵심 개발·통신·시운전 문서 | 8 |
| **합계** | **189 MD** |

증거 수준으로 나누면 대략 다음과 같습니다.

- PC/source/static/자동시험/문서: 약 124 MD
- ctrlX IDE project/compile 확인: 약 14 MD
- PLC download/restart/task/jitter bench: 약 16 MD
- 실축·패킷·fault/soak: 약 25 MD
- 안정화·릴리스: 약 10 MD

앞 단계 PASS를 다음 단계 PASS로 계산하면 안 됩니다.

### GUDEL 자료로 줄어드는 부분

GUDEL에는 이미 6축 AxisInterface, `ModeGantry`, 4 ms task 구조, 통신·상태기계·시운전 문서 틀이 있습니다. 이를 통해 프로젝트 구조와 문서 작성에서 약 **8~15 MD**를 줄일 수 있습니다. [GUDEL 실행 구조](</C:/Users/dreiv/source/repos/BOSCH/CtrlX/PLC Engineering/GUDEL/README.md:35>)

하지만 GUDEL은 Bosch IndraDrive/SoE 중심이고, Elmo는 CoE/CiA402·기존 커스텀 TCP/UDP 계약입니다. 따라서 다음은 그대로 재사용되지 않습니다.

- Elmo ESI/PDO/SDO 및 축 profile
- LASAL TCP server/session/queue
- 4축 Cartesian group 의미
- D3/D4 Recorder
- D5 SDO ticket executor
- UDP callback/EventMask producer

ctrlX에서 raw TCP 자체는 `CXA_SocketComm` 등을 통해 구현 가능하지만, PLC/Motion/EtherCAT Master 앱 구성과 라이선스가 먼저 고정돼야 합니다. [Bosch ctrlX First Steps](https://docs.automation.boschrexroth.com/pdf/document/ID1701163_140072404?filename=First+Steps.pdf&lang=eng), [Bosch TCP/UDP guidance](https://community.boschrexroth.com/ctrlx-core-25gnfzl4/post/tcp-or-udp-communication-NZ1Uf1HjtJk3uEx) 또한 외부 CoE 드라이브는 제조사별 CiA402 object 대응을 확인해야 하므로, ESI import 성공만으로 AxisInterface 호환을 확정할 수 없습니다. [Bosch Axis Profiles](https://docs.automation.boschrexroth.com/pdf/document/ID1642578_88513882?filename=Window+%E2%80%93+Axis+Profiles&lang=eng)

### 추가 공수가 발생하는 조건

- Elmo가 ctrlX AxisInterface에 바로 올라가지 않아 직접 DS402 어댑터 필요: **+15~30 MD**
- 기존 Cartesian group 의미를 ctrlX에서 동일하게 구현하기 어려움: **+15~25 MD**
- 다중 EventMask 구현: **+10~20 MD**
- 현재 비활성 SetPosition, DS402 Home, Double Recorder, SDO Write 활성화: **+25~50 MD**
- GUDEL처럼 전체 영문/한글 문서와 generated HTML까지 제작: **+15~25 MD**
- Safety PLC, STO, SIL/PL 인증: 이 산정과 별도

따라서 보고용 한 줄은 다음이 적절합니다.

> **ctrlX 전체 호환 포팅은 기준 190 MD, 2인 기준 5~6개월이며, 대외 견적은 220 MD로 잡는 것이 안전하다.**

착수한다면 첫 5~10 MD를 1축 PoC로 사용해 Elmo ESI, EtherCAT OP/DC, PDO/SDO, scaling, Power/Move/Stop, fault reset, AxisInterface 적용 가능성을 확인한 뒤 나머지 공수를 재산정하는 것이 좋습니다. 요청대로 파일은 작성하거나 수정하지 않았습니다.
