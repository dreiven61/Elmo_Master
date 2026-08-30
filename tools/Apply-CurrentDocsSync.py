from pathlib import Path
import re

manual_path = Path('docs/api/API_MANUAL.md')
progress_path = Path('docs/api/API_DEVELOPMENT_PROGRESS.md')


def replace_once(text, old, new, label):
    if old not in text:
        raise SystemExit('missing expected block: ' + label)
    return text.replace(old, new, 1)


def replace_section(text, start_heading, end_heading, new_section, label):
    pattern = re.compile(
        re.escape(start_heading) + r'.*?(?=' + re.escape(end_heading) + r')',
        re.S)
    updated, count = pattern.subn(new_section.rstrip() + '\n\n', text, count=1)
    if count != 1:
        raise SystemExit('expected one section for %s, got %d' % (label, count))
    return updated


manual = manual_path.read_text(encoding='utf-8')
manual = replace_once(
    manual,
    '문서 버전: 2.4-development\n적용 API: LasalMotionControlLib 0.9.1-preview\n대상 환경: Windows, .NET Framework 4.8\n기준일: 2026-08-20',
    '문서 버전: 2.5-development\n적용 API: LasalMotionControlLib 0.9.1-preview\n대상 환경: Windows, .NET Framework 4.8\n기준일: 2026-08-31',
    'manual header')
manual = replace_once(
    manual,
    '| 2.4-development | 2026-08-20 | current API 문서 위치 통합, SetPosition P1/volatile backing/fail-closed 계약과 최신 PLC image load 경계 반영 |',
    '| 2.4-development | 2026-08-20 | current API 문서 위치 통합, SetPosition P1/volatile backing/fail-closed 계약과 최신 PLC image load 경계 반영 |\n| 2.5-development | 2026-08-31 | SetOperationMode PP/PV/IP/CSP qualification-active 계약, Generic SDO R03~R05, branch cleanup, 17:28 capability freshness ordering blocker와 current 실기 절차 반영 |',
    'manual revision row')
manual = replace_once(
    manual,
    '| SDO Write | 축 1 UI[24] exact target만 source/IDE build 승인 | bit 9와 exact identity를 확인한 제한 시험만 허용; current 실기 mutation evidence 미완료 |',
    '| SDO Write | Generic scalar policy source-active / qualification-active | physical axis 1..4의 safe non-semantic 1/2/4-byte Write 계약이 구현됐으나 hardware PASS는 미완료; semantic/dedicated-owner raw object는 계속 차단 |',
    'manual support SDO row')
manual = replace_once(
    manual,
    '| SDO Write | 축 1 UI[24] exact target만 제한 | fresh identity와 read-before/write/readback 절차가 필수; 축 2~4 차단 |',
    '| SDO Write | Generic scalar qualification-active | fresh identity, safe drive state, exact request preview와 durable no-replay가 필수; hardware write/readback matrix는 미완료 |',
    'manual diagnostics SDO row')

setop_section = '''### 3.11.1 SetOperationMode current qualification contract

2.5-development SDK/source의 SetOperationMode는 CSP-only scaffold가 아니다. current `dev`는
PP(1), PV(3), IP(7), CSP(8)를 `0x018A` supported-mode mask로 광고하고 Admin
Start/Outcome/Retire triad를 활성화한다. Homing(6)은 이 API가 아니라 HomeDS402 계열이 소유한다.

| 단계 | `LMCSingleAxis` API | Command | current source |
|---|---|---:|---|
| Prepare | `PrepareSetOperationMode` | wire 없음 | current capability/identity validation |
| Start once | `SetOperationMode[Async]` | `0x7D23` | qualification-active |
| Exact outcome query | `ReadSetOperationModeOutcome[Async]` | `0x7D24` | qualification-active |
| Exact terminal retirement | `RetireSetOperationModeOutcome[Async]` | `0x7D25` | qualification-active |

Start ACK는 completion evidence가 아니며 prepared command는 one-shot이다. result가 불확실한
경우 `0x7D23` 또는 원 `0x6060` Write를 자동 replay하지 않는다. recovery는 exact durable
identity로 outcome/current-mode observation/retirement만 수행한다. raw Generic SDO로
`0x6060`을 직접 쓰는 것은 계속 금지한다.

실제 cross-mode 후보는 Start 전에 fresh `ReadDriveStatusAsync()`로 LASAL status,
DS402 `0x6041`, `0x6061`을 읽고 `Standstill=True`, DS402 Fault=False,
OperationEnabled=False를 요구한다. current mode가 requested mode와 같으면 PLC lifecycle은
`SucceededNoWrite`가 될 수 있으므로 CSP->CSP 성공만으로 `0x6060` Write 성공을 증명하지 않는다.

### 3.11.2 2026-08-28 17:28 실기 blocker

Axis1 current CSP(8)에서 PP/PV/IP 요청은 모두 `StatusWord=0x02D0`으로 cross-mode preflight를
통과했다. 그러나 다음 단계에서 아래 host exception으로 종료됐다.

```text
The supplied diagnostics capabilities are not the current observation.
```

현재 원인은 PLC reject가 아니라 capability observation ordering이다. WPF가 Diagnostics capability
observation N을 저장한 뒤 `ReadDriveStatusAsync()`가 `0x6041`/`0x6061` inline D5 Read를 수행하고,
각 submission 내부 `Diagnostics.GetCapabilities()`가 observation을 N+1/N+2로 진행시킨다. 이후
`PrepareSetOperationMode(... observation N ...)`이 `requireCurrentObservation=true`에서 stale로
거부된다.

따라서 이 재현에서는 durable journal arm, `0x7D23`, 실제 `0x6060` mutation까지 도달하지 않았다.
현재 corrective ordering은 다음으로 고정한다.

```text
Admin capability / selected-mode 확인
-> GetPhysicalAxis
-> fresh ReadDriveStatus preflight
-> FINAL Diagnostics capability refresh
-> capability/admission validation
-> PrepareSetOperationMode
-> durable ArmBeforeDispatch
-> Start exactly once
```

freshness fence, Build/BootId/MapRevision identity, one-shot confirmation, DS402 safety fence와
no-replay 정책을 완화해서 해결하지 않는다. 해당 ordering fix와 regression이 `dev`에 반영되기 전까지
PP/PV/IP physical mode-change PASS로 판정하지 않는다.'''
manual = replace_section(
    manual,
    '### 3.11.1 SetOperationMode 개발 SDK',
    '## 3.12 Move 완료와 restart recovery 경계',
    setop_section,
    'manual SetOperationMode section')

mutation_section = '''## 6.10 Mutation API 정책

| Public API 계약 | current source / 차단 근거 | 판정 |
|---|---|---|
| `SubmitPIWrite[Async]` | PI Write capability/allowlist OFF | 실행 금지 |
| SDO `CreateWrite` + `SubmitSdo[Async]` | R03 generic scalar policy + R04 exact editor/preview + R05 durable recovery 통합 | qualification-active / hardware PASS 미완료 |
| `SubmitDigitalOutputWrite[Async]` | DO capability/route/owner/allowlist 없음 | 실행 금지 |
| Recoverable Double Recorder | Double capability/route gate OFF, single bank | 실행 금지 |

Generic SDO Write는 physical axis 1..4의 canonical scalar width 1/2/4 byte를 대상으로 한다.
ordinary Write는 live axis가 `Standstill=True`, DS402 Fault=False, OperationEnabled=False여야 하며,
PLC generic admission은 non-enabled base state `0x40`(Switch On Disabled), `0x21`(Ready To Switch On),
`0x23`(Switched On)만 허용한다. `0x27` Operation Enabled와 기타 unsafe state는 차단한다.

다음 raw object는 semantic/dedicated-owner 경로가 있으므로 Generic SDO Write에서 계속 금지한다.

```text
0x6040 Controlword
0x6060 Modes of operation
0x607A Target position
0x60FF Target velocity
0x6071 Target torque
0x3204 / 0x20FC project-owned maintenance objects
```

WPF ordinary editor는 exact request preview와 reserved/semantic warning을 표시한다. Write 결과가
불확실한 경우 자동 재전송하지 않으며 R05 durable record는 endpoint + DiagnosticsBuild + BootId +
MapRevision + exact request identity에 묶인다. restart recovery는 read-only 결과 확인 경로만 허용한다.

과거 Axis1 UI[24] `0x2F00:24` same-value four-ticket qualification은 특정 live qualification preset으로
남아 있지만, 더 이상 Generic SDO API 전체의 유일한 허용 target으로 해석하지 않는다.

현재 source/PC regression은 통과했지만 실제 safe non-semantic object의 1/2/4-byte Write + exact
readback hardware matrix는 아직 완료되지 않았다. 따라서 production mutation 승인으로 해석하지 않는다.'''
manual = replace_section(
    manual,
    '## 6.10 Mutation API 정책',
    '## 6.11 Request/result와 provenance 확인',
    mutation_section,
    'manual mutation policy section')

if '현재 PLC source에는 `0x7D23/0x7D24/0x7D25` dormant' in manual:
    raise SystemExit('stale SetOperationMode dormant text remains')
if '규범적으로 유일한 SDO Write target은 Axis 1' in manual:
    raise SystemExit('stale Axis1-only SDO text remains')
manual_path.write_text(manual, encoding='utf-8')

progress = progress_path.read_text(encoding='utf-8')
progress = replace_once(
    progress,
    '- 문서 버전: 1.2-current\n- 기준일: 2026-08-27\n- API: `LasalMotionControlLib 0.9.1-preview`\n- 기준 branch/HEAD: `dev@1f741bfd08e9d75a52f7edd03862ef26ac562edd`',
    '- 문서 버전: 1.3-current\n- 기준일: 2026-08-31\n- API: `LasalMotionControlLib 0.9.1-preview`\n- current integration branch: `dev`\n- reviewed source baseline before this docs sync: `db954731c27c30f43f706b101276b81b022bd60a`',
    'progress header')
progress = replace_once(
    progress,
    '- SetOperationMode는 owner/SDO/no-replay/preemption/outcome/D5 deny + MODE-10 source/static + MODE-13\n  WPF recovery가 구현됐다. compile gate와 bits 8/9/10은 OFF다.',
    '- SetOperationMode는 PP/PV/IP/CSP lifecycle, supported mask `0x018A`, durable no-replay recovery와 live cross-mode preflight까지 qualification-active다. 17:28 실기에서 preflight 후 Diagnostics capability observation이 stale되는 host ordering blocker가 확인됐으며 실제 `0x6060` mutation은 아직 미도달이다.\n- Generic SDO는 R03 generic 1/2/4-byte scalar Write, R04 exact editor/preview, R05 durable no-replay recovery와 safe-state corrective가 통합됐다. source gate는 ON이지만 physical Write/readback PASS는 아직 아니다.',
    'progress current summary')
progress = replace_once(
    progress,
    '| SetOperationMode | `0x7D23/7D24/7D25` | Dormant | owner/SDO/no-replay/preemption/outcome/D5 deny/WPF recovery 존재; compile gate/bits 8..10 OFF |',
    '| SetOperationMode | `0x7D23/7D24/7D25` | Limited | PP/PV/IP/CSP qualification-active; current blocker는 preflight 뒤 Diagnostics capability freshness ordering, physical `0x6060` dispatch 미도달 |',
    'progress SetOperationMode row')
progress = replace_once(
    progress,
    '| D5 SDO Write | `0x7E50` write | Limited | Axis1 exact `0x2F00:24 Int32/4`만, `0x6060` permanent deny |',
    '| D5 SDO Write | `0x7E50` write | Limited | generic safe non-semantic 1/2/4-byte scalar policy + durable recovery 통합; OperationEnabled/semantic raw object deny, hardware readback matrix 미완료 |',
    'progress SDO row')

setop_progress = '''## 6. SetOperationMode current checkpoint

current `dev` source truth:

- qualification activation ON
- Admin Start/Outcome/Retire triad ON
- `SetOperationModeSupportedMask=0x018A` = PP(1), PV(3), IP(7), CSP(8)
- Homing(6)은 HomeDS402/HomeDS402Ex 소유
- durable pre-dispatch arm, exact outcome, exact-generation retirement
- Write-dispatched 이후 original Start/`0x6060` replay 금지
- raw Generic SDO `0x6060` permanent deny
- same-target `SucceededNoWrite`와 real cross-mode를 구분
- cross-mode preflight: Standstill=True, Fault=False, OperationEnabled=False

PR #58 software evidence:

- API Debug full suite 1200/1200 PASS
- Generic SDO WPF focused smoke 17/17 PASS
- API/WPF Debug + Release build PASS
- corrective/static verifier PASS

이 evidence는 physical SetOperationMode PASS가 아니다.

2026-08-28 17:28 live finding:

```text
Axis1 currentMode=8 -> requestedMode=3 : preflight PASS, StatusWord=0x02D0
Axis1 currentMode=8 -> requestedMode=1 : preflight PASS, StatusWord=0x02D0
Axis1 currentMode=8 -> requestedMode=7 : preflight PASS, StatusWord=0x02D0
Axis1 currentMode=8 -> requestedMode=8 : same-target no-write candidate
```

모든 시도는 이후 다음 host exception으로 종료됐다.

```text
The supplied diagnostics capabilities are not the current observation.
```

root cause는 capability freshness ordering이다.

```text
RefreshDiagnosticsCapabilities -> observation N
ReadDriveStatusAsync
  -> 0x6041 inline D5 -> Diagnostics.GetCapabilities -> N+1
  -> 0x6061 inline D5 -> Diagnostics.GetCapabilities -> N+2
PrepareSetOperationMode(cached N)
  -> requireCurrentObservation=true
  -> stale reject / ZERO mutation wire
```

따라서 현재 blocker는 PLC supported-mode reject가 아니며 `0x7D23`/`0x6060`까지 도달하지 않았다.
corrective는 freshness fence 제거가 아니라 preflight 뒤 FINAL Diagnostics capability refresh를 수행하는
ordering fix다.

완료 조건:

1. old observation이 preflight 후 stale reject되는 safety regression 유지
2. preflight 후 final Diagnostics refresh한 current observation으로 Prepare 성공
3. final refresh와 Prepare 사이 capability-producing call 없음
4. Prepare 성공 전 journal/Start mutation 0회
5. software regression green 후 Axis1 PP/PV/IP/CSP physical matrix 재개'''
progress = replace_section(
    progress,
    '## 6. SetOperationMode current checkpoint',
    '## 7. HomeDS402Ex current checkpoint',
    setop_progress,
    'progress SetOperationMode checkpoint')

priorities = '''## 9. current 개발 우선순위

1. **SetOperationMode capability freshness ordering fix** — fresh drive preflight 뒤 FINAL Diagnostics capability refresh
2. focused regression — stale old observation reject + final-current observation Prepare success + zero-wire boundary
3. updated `dev` API/WPF Debug/Release validation
4. Axis1 SetOperationMode PP/PV/IP/CSP physical matrix (`0x6060` exact-one-write / `0x6061` readback)
5. Axis1 Generic SDO safe non-semantic 1/2/4-byte Write + exact readback matrix
6. SetOperationMode/Generic SDO timeout, disconnect, response-loss, durable no-replay recovery matrix
7. Axis2..4 확대
8. HomeDS402 fresh C78/generated artifact + hardware matrix
9. HomeDS402Ex approved profile / artifact closure
10. SetPosition issue #44 external blocker closure 후 durable A/B backend + RT exactly-once'''
progress = replace_section(
    progress,
    '## 9. current 개발 우선순위',
    '## 10. branch / qualification 상태',
    priorities,
    'progress priorities')

branches = '''## 10. branch / qualification 상태

- remote branch는 현재 `main`, `dev` 두 개만 유지한다.
- 2026-08-28 cleanup에서 기존 `codex/*` 29개가 모두 `dev` ancestor임을 확인한 뒤 삭제했다.
- 열린 PR은 현재 없다.
- `dev`가 유일한 integration / current qualification source truth다.
- qualification 중 blocker를 찾았다는 이유로 장기 branch를 새로 누적하지 않는다.
- 기능 작업 branch가 필요한 경우 작업 -> 검증 -> `dev` merge -> 즉시 삭제 원칙을 적용한다.
- source SHA, generated artifact, PLC loaded image, WPF binary identity를 같은 qualification evidence set으로 기록한다.'''
progress = replace_section(
    progress,
    '## 10. branch / qualification 상태',
    '## 11. production release gate',
    branches,
    'progress branch state')

if 'compile gate와 bits 8/9/10은 OFF' in progress:
    raise SystemExit('stale SetOperationMode gate-off summary remains')
progress_path.write_text(progress, encoding='utf-8')

print('Current API manual and development progress updated.')
