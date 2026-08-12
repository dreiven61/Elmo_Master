# Chunk digest: parts 014–026

대상: `Elmo_Master_history_260812.md` 원본 3,251–6,500줄

## Part 014 — 원본 3,251–3,500줄

- 사용자 Rebuild는 성공했지만 `Classes.lcb`가 기존 `24402...`/`6E...`와 다른 세 번째
  `99014DD9...`가 됐다. 의미 분류 전 Download는 계속 금지했다(3,260–3,278).
- finalizer/validator의 OrderedDictionary, process exit, restore, EOL 결함을 고쳐 bundle을
  재생했지만 결과는 `UNSTABLE_THIRD_CLASSES_HASH_STOP`, classification 3이었다
  (3,282–3,405).
- Gate D/protected record는 exact였어도 unknown 16-bit slot 의미가 없어 승인하지 않았다
  (3,430–3,454). 증거는 IDE Rebuild와 binary/static 분석이며 PLC runtime은 없다.

## Part 015 — 원본 3,501–3,750줄

- triad/corpus 분석기를 강화하고도 모든 결과를 nonapproval/STOP으로 유지했다
  (3,501–3,608).
- 이후 로그에서 Rebuild, Connect/Download, Reset/Restart가 실행됐고 현재 Classes가 네 번째
  `13EA5823...`로 바뀐 incident를 확인했다. 282 `.lba` download와의 관계는
  `TIME_CORRELATION_ONLY`로만 기록했다(3,632–3,735).
- 공식 field semantics와 reviewed support response는 미해결이었다.

## Part 016 — 원본 3,751–4,000줄

- canonical 개발 실행 경로를 재확인해 stale distribution/copy 가설을 폐기했다
  (3,758–3,880).
- 기존 회귀가 두 번째 수동 Connect 뒤에만 성공하는 잘못된 모델임을 확인하고, exact
  persistent `0x8080/ErrorId=-1`에만 fresh TCP 1회를 허용하도록 설계를 바꿨다
  (3,878–3,929).
- 실제 사용자 stack에서 X-close의 `0x405D -1`과 relaunch 첫 connect의 `0x8080 -1` 연쇄를
  확인했다. 원인 사슬 일부는 당시 inference였고 구현은 진행 중이었다(3,943–4,000).

## Part 017 — 원본 4,001–4,250줄

- Dispose 최대 2회, 완전한 local disconnected/RPC/callback/endpoint 정리 뒤에만 close
  진단을 억제하고 그렇지 않으면 replacement를 열지 않는 계약을 구현했다(4,034–4,065).
- targeted `6/6`, SDK `1133/1133`, WPF Debug/Release `339/339`, 독립 검토 `9/9`를
  통과했다(4,098–4,142).
- 코드·테스트 두 파일을 `14ccf58`로 커밋했다(4,144–4,157). 이는 fake-peer/PC 증거이며
  PLC 다운로드나 실장비 증거는 아니다.

## Part 018 — 원본 4,251–4,500줄

- reconnect 문서를 조건부 Close, callback version, retry 경계에 맞춰 교정하고 별도 commit
  `8572ae5`를 만들었다(4,290–4,459).
- bounded fresh TCP retry, ErrorId=0/malformed/transport/callback failure no-retry, local cleanup
  진단 계약을 정리했다(4,464–4,487).
- 100ms는 PC 정책일 뿐 PLC readiness 증거가 아니며, live 실패 시
  `RpcCallbackLastDisarmResult(-8/-9)` 확인이 필요했다. process relaunch/mutex 증거도
  다음 공백으로 남았다(4,485–4,500).

## Part 019 — 원본 4,501–4,750줄

- LASAL declaration이 필요한 ReserveAxisOwnership보다 사용자 증상과 정확히 맞는 actual EXE
  exit/relaunch 경계를 먼저 닫기로 결정했다(4,560–4,594).
- 첫 프로세스 완전 종료, mutex contender 차단, 두 번째 프로세스 mutex 재획득, 동일 endpoint,
  정확히 3개 TCP session과 실행 파일 hash 불변을 요구했다(4,568–4,655).
- 초기 harness는 child 생존과 3초 scheduling 의존성 때문에 검토에서 거부됐고 보강이
  필요했다(4,685–4,724).

## Part 020 — 원본 4,751–5,000줄

- Debug/Release actual EXE에서 X-close NACK → 프로세스 종료 → mutex 차단 → 두 번째 EXE의
  fresh TCP 성공을 통과했고 WPF `339/339`도 유지했다(4,752–4,781).
- Full Distribution은 더 앞의 LASAL static contract에서 멈췄으므로 전체 PASS로 부르지 않고,
  ProjectReference 0과 binary SDK 참조를 쓰는 isolated candidate를 만들었다(4,803–5,000).
- 증거는 실제 프로세스/fake RPC PC 증거이며 PLC 증거는 아니다.

## Part 021 — 원본 5,001–5,250줄

- binary-reference candidate는 TCP `3/28`, EXE/DLL hash 불변, canonical Release SDK
  byte identity를 통과했고 temp tree도 제거됐다(5,001–5,034).
- actual-EXE gate를 `cbf2548`로 커밋했다. 이전 Distribution STOP의 원인은 PS5.1
  `MatchCollection[-1]` 동작 차이였으며 한 파일을 고쳐 `ad4af91`로 커밋했다
  (5,036–5,182).
- 이후 Distribution은 reconnect가 아니라 미승인 `Classes.lcb` identity에서 의도대로
  STOP했다(5,162–5,168).

## Part 022 — 원본 5,251–5,500줄

- reconnect PC tranche를 문서까지 닫았다. commits는 `14ccf58`, `cbf2548`, `ad4af91`,
  `dbcdade`; actual EXE Debug/Release `1/1`, TCP `3/28`이었다(5,251–5,305).
- live PLC retest는 여전히 필요했고 Full Distribution은 Gate D STOP이었다.
- 다음 audit에서 shipped DOCX/PDF가 여전히 1.9이고, method-size ratchet이 stale baseline 때문에
  regrowth를 허용하는 P1을 발견했다(5,314–5,496).

## Part 023 — 원본 5,501–5,750줄

- DOCX generator의 OOXML child-order 결함을 먼저 고치고 Word-normalized DOCX와 PDF를
  생성·추출·전 페이지 렌더했다(5,527–5,673).
- raw Markdown `###`가 마지막 페이지에 남은 것을 육안 QA로 잡아 Heading 3 처리를 추가하고
  43쪽을 다시 생성했다(5,675–5,729).
- semantic policy와 method-size ratchet은 양 PowerShell에서 검증 중이었고 최종 commit은
  다음 청크에서 이뤄졌다.

## Part 024 — 원본 5,751–6,000줄

- 자연어 semantic reversal과 version-history bypass를 닫아 양 host policy 50 tests/18 checks,
  actual docs `3/3`을 통과했다(5,756–5,824).
- pipeline 115, size `8/8` 및 `101/98/3`, Reserve `62/62`; commits `2e8b505`,
  `f8e993e`, `5c48f25`를 만들었다(5,891–5,938).
- 문서 artifact는 43쪽/OpenXML 0으로 검증했지만 Git에는 넣지 않았다. Gate D STOP과
  no Download 경계를 유지한 채 clean-checkout release audit를 시작했다(5,940–5,995).

## Part 025 — 원본 6,001–6,250줄

- solution membership/config/build, method-size exact current baseline, whole-method
  `HandleRequest` fence를 추가했다. 양 host에서 solution `129/129`, size `16/16` 및
  `101/98/3`, HandleRequest `13/13`을 통과했다(6,011–6,066).
- commits는 `88f1c57`, `d735446`이며 clean full Distribution을 시작했다(6,091–6,126).
- Debug RunTests는 UDP callback CyWork token drift에서 STOP했다. 사용자 Classes와 무관한
  clean-checkout 재현이었으며 baseline을 임의로 올리지 않았다(6,136–6,250).

## Part 026 — 원본 6,251–6,500줄

- CRLF regex가 function 경계를 합치는 문제와 ignored `.lba/.lob` 8개를 필수로 보는 문제를
  분리했다. tracked Network 15파일 exact tuple을 추가하되 기존 23-file history는 보존했다
  (6,310–6,326).
- PS5/PS7 `296/296`, focused clean current `CAPTURE`를 통과하고 verifier-only commit을
  만든 뒤 full run을 재실행했다(6,351–6,379).
- false blocker를 모두 지나 intended boundary인 “approved physical snapshot 없음”에서
  STOP했고 candidate/stage/lock rollback은 clean했다(6,402–6,425).
- 다음 공백은 release fingerprint의 LASAL 입력 누락과 mixed-EOL size baseline이었다
  (6,468–6,496).
