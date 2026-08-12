# Chunk digest: parts 001–013

대상: `Elmo_Master_history_260812.md` 원본 1–3,250줄

증거 표기:

- `PC/정적`: 소스, fake peer, 빌드, PowerShell verifier 또는 파일/해시 검증
- `C78/IDE`: LASAL IDE build/rebuild/log 또는 생성물 검사
- `PLC`: download, Online/Watch, 실제 패킷·장비 동작

## Part 001 — 원본 1–250줄

- Gate D 변수·파라미터·출력 10개를 현재 설계/verifier의 exact name으로 맞추도록
  교정했다. `D5` 접두어는 LASAL 문법이 아니라 프로젝트 계약상 요구다(140–199).
- `.st`와 method ABI는 맞았지만 `Classes.lcb`에 rename alternate-name과 offset 0이
  남아, 신규 변수 6개를 삭제 후 최종 이름으로 다시 만드는 절차를 정했다(212–248).
- 증거: 소스/생성 ABI 정적 검사. Build·PLC 검증 없음.

## Part 002 — 원본 251–500줄

- 사용자 재생성 작업이 canonical 프로젝트에 반영되지 않았고 `Classes.lcb`도 그대로인
  사실을 확인했다. 정확한 `.lcp`에서 변수 6개만 재생성하도록 다시 지정했다(264–281).
- LASAL UI 자동화를 준비했으나 상태 캡처 런타임 문제가 이어졌다(339–437).
- 증거: 파일 timestamp·로그·생성물 정적 검사. 실제 재생성은 미완료.

## Part 003 — 원본 501–750줄

- UI 자동화는 LASAL 실행까지 성공했지만 상태 캡처가 반복 실패해 IDE를 종료했다
  (539–713).
- Gate D broker 계약을 `TryTake=-1/0/1`, one-shot claim,
  `Attempt=Enqueued+Rejected`, retry/outbox 없음으로 고정하고 Diagnostics/Sender/TCP 및
  설계문서를 수정했다(713–731).
- Delivery `1111/1111`은 PASS했지만 WPF·전용 verifier·C78·PLC는 당시 진행 중이었다
  (733–746).

## Part 004 — 원본 751–1,000줄

- PC `1111/1111`, WPF `332/332`, declaration `17/17`, SourceOnly와 음성 fixture
  `276/276`을 통과했고 실제 트리를 `TerminalWakeBrokerCandidate`로 분류했다
  (755–786).
- `ProductionApproved=false`, `NeedsRebaseline=true`를 유지했다. CUA가 Esc로 중단돼
  그 시점 C78/PLC 검증은 실행되지 않았다(831–843).
- 사용자 빌드 뒤 첫 Download 성공과 두 번째 restart abort를 분리했고, strict Rebuild의
  `lsl_st_mt=38` 실측으로 잘못된 verifier pin 37을 교정했다(868–987).

## Part 005 — 원본 1,001–1,250줄

- build/정적 PASS와 runtime 증거를 분리하고 pcap, WPF 로그, PLC counter, ticket tuple을
  runtime 증거로 요구했다(1,001–1,016).
- strict Rebuild는 `0 errors / 76 warnings`로 통과했다. 일반 method 검증은
  `Find in Implementation`이 아니라 class tree에서 직접 열어 exact header를 확인하는
  것으로 바로잡았다(1,049–1,143).
- 다음 순서는 direct-open smoke → trusted capture/atomic commit → Download → PLC runtime으로
  정리됐다(1,188–1,238).

## Part 006 — 원본 1,251–1,500줄

- idle/power-off 전제, untracked `TestClass` hard-pin 제거, EventId/mismatch 음성 조건과
  root `.lcb` exact delta 검사를 보강했다(1,256–1,313).
- focused 결과를 PASS가 아니라 `CAPTURE`로 교정하고 production 미승인을 유지했다
  (1,327–1,340).
- 사용자가 의도한 Network visual layout 변경은 보존했다. 23개 Source/Destination 쌍은
  동일해 기능 topology는 유지됐지만 전이 범위는 7파일로 확장됐다(1,421–1,498).

## Part 007 — 원본 1,501–1,750줄

- isolated Rebuild 1회가 `0 errors / 76 warnings`, 입력 10/10 불변으로 통과했다
  (1,511–1,616).
- method-smoke 자동 도구의 정상 종료 lifecycle 순서 모델이 틀린 결함을 발견했다
  (1,643–1,686).
- Object Network Server/Client만 `Find in Implementation`, 일반 function/method는
  `Edit Method` 또는 Enter로 연다는 규칙을 AGENTS/가이드에 반영했다(1,692–1,746).

## Part 008 — 원본 1,751–2,000줄

- GD-01, 두 ticket, UDP 유실/manual refresh, polling, disarm, reconnect와 별도 harness
  음성시험을 runtime 목록으로 확정했다(1,774–1,829).
- trust-anchor `bb5fd93`, trusted ValidateOnly 845.5초 PASS, physical capture 1,006초 PASS와
  정확한 7-path 전이를 확보했다(1,884–1,997).
- 이는 committed-clean static/capture 증거이며 PLC runtime 증거는 아니었다.

## Part 009 — 원본 2,001–2,250줄

- 7개 production 파일과 manifest를 `5543579`로 원자 커밋하고 post-commit 검증 뒤
  Download 1회를 승인했다(2,001–2,036).
- LASAL 로그에는 `282 files`, `Download Ok`, `Project successfully loaded`가 남았지만
  직전 Rebuild/auto-save로 sealed image 동일성은 별도 감사 대상이었다(2,067–2,081).
- GUI 항목과 LASAL Watch/pcap 항목을 분리하고 GD-01 read-only SDO 절차를 정리했다
  (2,086–2,247). 이후 GUI 재접속 버그가 새 핵심 문제가 됐다(2,249).

## Part 010 — 원본 2,251–2,500줄

- 화면 오류는 TCP connect 자체가 아니라 PLC `0x8080` init 거부였고, PC가 short ACK의
  `ErrorId=-1`을 0으로 숨기는 파서 문제를 확인했다(2,282–2,299).
- V2 exact transient frame에만 동일 socket 20ms 후 1회 retry하고 그 외는 fail-closed로
  두는 bounded PC fix를 만들었다(2,301–2,313).
- SDK `1116/1116`, WPF `333/333`, commit `66b5cf2`를 확보했지만 PLC의 실제 `-8/-9`
  분기는 아직 확정하지 못했다(2,357–2,457).

## Part 011 — 원본 2,501–2,750줄

- RPC init/callback provenance UI를 추가해 SDK `1117/1117`, WPF `333/333`, commit
  `f337feca`를 만들었다(2,510–2,579).
- stale Dispatcher 오염 회귀를 추가해 WPF `334/334`, 문서 commit `ad7c8b1`을 남겼다
  (2,585–2,635).
- `Classes.lcb=6E115876...`는 승인하지 않고 binary patch/baseline 증거만 보존한 뒤
  isolated Rebuild 1회를 요청했다(2,709–2,748).

## Part 012 — 원본 2,751–3,000줄

- comparator는 tracked `24402...`만 승인하고 `6E...`를
  `REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT`로 분류했다. 99 bytes/58 runs/36 owners 중
  target 4개와 protected 2개 record는 exact였다(2,761–2,804).
- finalizer/validator는 경로·ledger·atomic publish·ADS 공격과 PS5/PS7 차이를
  fail-closed로 보강했다(2,840–2,948).
- ErrorId=0 no-retry와 Requested/Bound callback 표시도 추가했으나 post-baseline Rebuild와
  final bundle은 아직 없었다(2,963–2,999).

## Part 013 — 원본 3,001–3,250줄

- bundle validator를 완결해 `531abdd`, 관련 문서를 `15ae250`으로 커밋했다
  (3,006–3,063).
- GD-N10A/N13/N14 raw-wire harness는 fake peer/dry-run PC 증거로만 `bff3bc7`에 고정하고
  live PLC 증거로 승격하지 않았다(3,074–3,138).
- same-IP takeover candidate와 different-IP reject 계약을 source/document에 맞췄다
  (3,184–3,192).
- baseline 이후 Rebuild가 없어 당시 목표는 blocked였다. 다음 외부 단계는 canonical LASAL
  Rebuild 1회였고 Download는 금지됐다(3,194–3,249). 이 중간 상태는 이후 청크에서
  Rebuild·Download·추가 수정으로 갱신된다.
