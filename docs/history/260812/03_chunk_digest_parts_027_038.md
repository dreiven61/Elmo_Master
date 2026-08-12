# Chunk digest: parts 027–038

대상: `Elmo_Master_history_260812.md` 원본 6,501–9,423줄

## Part 027 — 원본 6,501–6,750줄

- method-size ratchet을 checkout EOL과 무관하게 만들고 현재 exact baseline
  `101/98/3`을 고정했다(6,526–6,544).
- release fingerprint가 실제로 읽는 LASAL 입력 전체를 포함하도록 보강하고 관련 변경을
  커밋했다(6,572–6,608).
- clean full Distribution은 false blocker 없이 Gate D의 미승인 snapshot 경계에서
  의도대로 STOP했고 staging·lock·candidate residue는 남지 않았다(6,627–6,660).
- 증거는 clean-checkout PC/정적 검증이며 IDE·PLC 증거는 아니다.

## Part 028 — 원본 6,751–7,000줄

- candidate 생성 전에 PS5/PS7 양쪽에서 6개 tooling suite를 실행하고 결과를 비교하는
  dual-host preflight를 구현했다(6,763–6,807).
- 최초 `12/12`는 culture-dependent 정렬 때문에 host별 aggregate digest가 달라 유효한
  PASS로 인정하지 않았다. 비교와 직렬화를 ordinal 기준으로 교정했다(6,884–6,928).
- 개별 pipeline `245/245`는 통과했지만 수정된 aggregate의 최종 양-host 결과는 다음
  청크에서 확정됐다(6,980–7,000).

## Part 029 — 원본 7,001–7,250줄

- ordinal 교정 뒤 dual-host preflight `12/12`, 92개 file entry의 동일 aggregate digest,
  총 802.8초 결과를 확보했다(7,009–7,069).
- 도구 변경과 문서를 각각 `febb1b0`, `701550c`로 커밋했다(7,053–7,069).
- 이어서 provenance schema 3과 실행 역할별 기록을 시작했으며 compiler identity, host
  executable SHA, Git core binding이 빠져 있음을 찾아 보강 대상으로 남겼다
  (7,153–7,249).

## Part 030 — 원본 7,251–7,500줄

- Roslyn/Csc identity와 `UseSharedCompilation=false`, Git core, 호출 PowerShell/Python
  inventory를 provenance에 고정했다(7,266–7,312).
- 양 host `12/12`, 94개 file entry의 동일 digest를 통과한 변경을 `39c3e6f`로
  커밋했다(7,354–7,383).
- provenance gate를 mandatory suite로 승격해 `14/14`를 통과시키고 `1b9be6a`로
  커밋했다. 실제 Python 의존성 5개가 다음 점검 대상으로 확인됐다(7,442–7,498).

## Part 031 — 원본 7,501–7,750줄

- 사용 중인 Python 패키지 `lxml`, `typing_extensions`, `cryptography`, `Pillow`, `cffi`를
  확인하고 provenance 대상에 포함했다(7,508–7,531).
- 13개 실행 역할을 양 host에서 고정하고 mandatory `14/14`를 통과한 변경을
  `3c63dea`로 커밋했다(7,548–7,711).
- release tooling 경계는 강화됐지만 canonical로 배포되는 API manual이 여전히 1.9인
  사실이 다음 blocker로 남았다(7,715–7,738).

## Part 032 — 원본 7,751–8,000줄

- 검토한 2.3 DOCX/PDF를 canonical 배포 경로로 옮기고 semantic policy `3/3`, A4 43쪽,
  heading 66개, table 109개를 확인했다(7,757–7,799).
- 시각 QA에서 본문이 스스로를 `canonical 1.9`라고 부르는 모순을 발견해 commit을
  중단했다. 기존 artifact를 그대로 승인하지 않고 Markdown 원본에서 다시 생성하기로
  했다(7,832–7,845).

## Part 033 — 원본 8,001–8,250줄

- Markdown 원본에서 manual을 다시 만들어 OpenXML 오류 0, A4 43쪽, embedded/subset font
  8개와 semantic policy `3/3`을 확인했다(8,077–8,100).
- 양 host에서 pipeline `291/291`, semantic `52/52 + 18/18`, manifest `108/108`을
  통과했고 3-file review도 깨끗했다(8,226–8,228).
- 실제 commit은 다음 청크에서 이뤄졌다.

## Part 034 — 원본 8,251–8,500줄

- regenerated 2.3 manual을 `bcc6a9c`, 관련 README 정책을 `f304e8b`로 커밋했다
  (8,251–8,312).
- Gate D physical snapshot ratchet은 clean tracked `24402...` tuple만 승인하도록 만들었다.
  dirty main의 `13EA...`는 90 changed bytes/57 runs/35 owners 때문에 계속 거부했다
  (8,369–8,421).
- 양 host Gate D 검증은 진행 중이었고 최종 결과는 다음 청크에서 확정됐다
  (8,431–8,500).

## Part 035 — 원본 8,501–8,750줄

- Gate D verifier는 PS5/PS7 각각 `296/296`, clean tracked snapshot PASS, dirty main FAIL을
  확인하고 `d4204b4`로 커밋했다(8,508–8,568).
- manual/docs 동기화 변경도 `5d5aebe`로 닫았다(8,601–8,632).
- clean full Distribution은 semantic count 기대값을 `8d51cee`로 교정한 뒤 더 진행했지만
  ConfigObjects checkout-EOL 차이에서 멈췄다(8,668–8,735). 전체 pipeline PASS는 아니다.

## Part 036 — 원본 8,751–9,000줄

- 6개 synthetic EOL root를 교정해 PS5/PS7 `296/296`을 통과시키고 `105daf2`로
  커밋했다(8,753–8,870).
- full run은 mandatory `14/14`까지 갔다가 dot-source helper 이름 충돌을 발견했고 이를
  고친 commit이 현재 HEAD `5e53865`다(8,894–8,929).
- 재실행은 Debug SourceOnly까지 진행했으나 wrapper가 요청 `Auto`가 아니라 실제 child
  state를 전파해야 하는 결함을 발견했다. LASAL IDE 외부 활동 때문에 focused 재검증도
  중단했다(8,945–9,000).

## Part 037 — 원본 9,001–9,250줄

- wrapper의 Auto-state 전파 수정 설계가 진행되던 중 사용자가 임시 worktree 정리와 실제
  재접속 문제 해결을 우선 요청했다(9,001–9,010).
- 검증용 worktree 7개를 안전하게 제거해 약 5.36 GiB를 회수했고, 실제 앱은 첫 Connect부터
  네 번 연속 `0x8080/ErrorId=-1`을 받아 PC port 잔류가 아니라 PLC callback owner 상태를
  가리켰다(9,012–9,083).
- 수정 범위를 definitive current-owner loss에서 발생하는 exact `-8`로만 제한하고 `-9`,
  different-IP, late old socket, 일반 실패는 계속 fail-closed로 두기로 했다
  (9,126–9,158).
- PC에서 normal Close와 close `0x405D/ErrorId=-1` 재접속 시나리오를 반복 검증하기
  시작했다(9,171–9,246).

## Part 038 — 원본 9,251–9,423줄

- `TCPMotionInterface.st`와 `LMCUdpCallbackSender.st`에 exact `-8` owner-loss retirement
  경로를 구현하고, 기존 296 fixture에 9개 owner-loss 음성/경계 fixture를 더해
  `305/305`로 고정했다(9,254–9,283).
- PS5/PS7 각각 UDP `305/305`, Pipeline `291/291`, WPF Debug와 신규 WPF `2/2` 반복
  5회를 통과했다(9,301–9,339).
- history 종점에서 변경은 unstaged/uncommitted이며 최신 두 ST 변경에 대한 LASAL
  build/download와 실제 Connect → Close → Connect는 수행되지 않았다(9,382–9,423).
- live 실패가 남으면 `RpcCallbackLastDisarmResult`를 먼저 확인한다. `-9`는 CallbackSender
  Network 연결 문제를 뜻하므로 강제 우회하지 않는다는 경계를 명시했다(9,401–9,423).
