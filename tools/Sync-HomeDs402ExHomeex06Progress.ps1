[CmdletBinding()]
param()

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$progressPath = Join-Path $repoRoot 'docs\api\API_DEVELOPMENT_PROGRESS.md'
$designReadmePath = Join-Path $repoRoot 'docs\api\design\README.md'

function Replace-Once {
    param([string]$Text, [string]$Old, [string]$New, [string]$Label)
    $count = ([regex]::Matches($Text, [regex]::Escape($Old))).Count
    if ($count -ne 1) {
        throw "HOMEEX-06 progress sync refused: '$Label' expected one match, found $count"
    }
    Write-Host "PASS exact anchor: $Label"
    return $Text.Replace($Old, $New)
}

function Read-Lf {
    param([string]$Path)
    return [System.IO.File]::ReadAllText($Path).Replace("`r`n", "`n").Replace("`r", "`n")
}

function Write-Utf8NoBomLf {
    param([string]$Path, [string]$Text)
    $Text = $Text.Replace("`r`n", "`n").Replace("`r", "`n")
    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Text, $encoding)
}

$progress = Read-Lf $progressPath
$readme = Read-Lf $designReadmePath

$progress = Replace-Once $progress `
    '- 기준 branch/HEAD: `dev@925dd8258feb` + 본 current-progress 동기화' `
    '- 기준 branch/HEAD: `dev@5a98162b5d48` + HOMEEX-06 current-progress 동기화' `
    'current dev HEAD'

$progress = Replace-Once $progress `
    '- SetOperationMode MODE-13 PC/WPF recovery는 current Windows PR qualification에서' `
    "- HomeDS402Ex ``0x7D1B/0x7D1C/0x7D1D``는 HOMEEX-06에서 LASAL diagnostics route, 전용 scaffold state와 strict Start/Outcome/Retire parser를 구현했고 67-check ``SCAFFOLD_OFF`` source/static qualification을 통과했다. runtime gate와 Admin bit 11은 OFF이며 OwnerKind 7/full 116-byte owner identity, SDO/RT/motion execution은 HOMEEX-07 이후로 닫혀 있다.`n- SetOperationMode MODE-13 PC/WPF recovery는 current Windows PR qualification에서" `
    'summary HomeDS402Ex scaffold'

$oldPriority = @'
- High-priority 21개 관점: Active 17, Partial 3(SetPosition, DS402 Home, SetOperationMode),
  Missing 1(`HomeDS402Ex`)
'@
$newPriority = @'
- High-priority 21개 관점: Active 17, Partial 3(SetPosition, DS402 Home, SetOperationMode),
  Dormant 1(`HomeDS402Ex`)
'@
$progress = Replace-Once $progress $oldPriority $newPriority 'high-priority HomeDS402Ex status'

$oldHomeRow = '| DS402 Home | `0x7D15/7D16/7D17` | Dormant | method 37 source, gate FALSE, Admin bit 6 OFF; `HomeDS402Ex` 실행은 없음 |'
$newHomeRows = @'
| DS402 Home | `0x7D15/7D16/7D17` | Dormant | method 37 source, gate FALSE, Admin bit 6 OFF |
| HomeDS402Ex | `0x7D1B/7D1C/7D1D` | Dormant | HOMEEX-06 diagnostics route + dedicated scaffold state + strict Start/Outcome/Retire parser; 67-check `SCAFFOLD_OFF` PASS; runtime gate/bit 11 OFF, owner/SDO/RT/motion 미구현 |
'@
$progress = Replace-Once $progress $oldHomeRow $newHomeRows 'functional status HomeDS402Ex row'

$oldReadmeRow = '| 3 | `HomeDS402Ex` | 0% | 기존 Home과 분리된 확장 Homing 신규 구현 | [HOME_DS402_EX_DESIGN.md](HOME_DS402_EX_DESIGN.md) |'
$newReadmeRow = '| 3 | `HomeDS402Ex` | 0% | HOMEEX-06 `SCAFFOLD_OFF` source/static PASS; full-identity ownership/runtime 후속 | [HOME_DS402_EX_DESIGN.md](HOME_DS402_EX_DESIGN.md) |'
$readme = Replace-Once $readme $oldReadmeRow $newReadmeRow 'design index HomeDS402Ex row'

$oldCommandRow = '| HomeDS402Ex | `0x7D1B` | `0x7D1C` | `0x7D1D` | 설계 예약, 아직 source 미반영 |'
$newCommandRow = '| HomeDS402Ex | `0x7D1B` | `0x7D1C` | `0x7D1D` | LASAL diagnostics route/scaffold 존재, runtime gate/capability OFF |'
$readme = Replace-Once $readme $oldCommandRow $newCommandRow 'command reservation status'

$oldWave2 = '- HomeDS402Ex: 승인된 axis profile과 method allowlist를 입력으로 dormant source를 구현한다.'
$newWave2 = '- HomeDS402Ex: HOMEEX-06 dormant parser/state/outcome scaffold는 완료했다. HOMEEX-07에서 full 116-byte owner identity bank와 OwnerKind 7/ResourceKind 3 admission을 paired 구현한다.'
$readme = Replace-Once $readme $oldWave2 $newWave2 'Wave 2 HomeDS402Ex next gate'

Write-Utf8NoBomLf $progressPath $progress
Write-Utf8NoBomLf $designReadmePath $readme

$changed = @(& git -C $repoRoot diff --name-only)
if ($changed.Count -ne 2 -or
    -not ($changed -contains 'docs/api/API_DEVELOPMENT_PROGRESS.md') -or
    -not ($changed -contains 'docs/api/design/README.md')) {
    throw "HOMEEX-06 progress sync changed unexpected files: $($changed -join ', ')"
}

& git -C $repoRoot diff --check
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host 'HOMEEX-06 current-progress sync PASS.'
