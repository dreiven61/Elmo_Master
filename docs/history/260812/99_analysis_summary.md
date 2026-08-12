# Elmo Master history analysis and continuation point — 2026-08-12

## 결론

이 히스토리의 현재 작업 종점은 다음과 같다.

1. PC/WPF 재접속 정책과 release verifier는 장기간에 걸쳐 크게 강화됐다.
2. 최신 실제 장애는 PC의 UDP port 잔류가 아니라, 새 TCP 연결도
   `0x8080/ErrorId=-1`로 거부되는 PLC callback owner 잔류 경로로 좁혀졌다.
   다만 wire `0x8080/-1`만으로 PLC 내부 disarm 결과가 exact `-8`, `-9` 또는 다른 lifecycle
   경계인지 확정할 수는 없다.
3. 이를 처리하는 **exact `-8` owner-loss retirement**가 두 LASAL `.st` 파일에 구현돼
   있고 PC/정적 시험 기록도 있다.
4. 이 최신 `.st` 변경은 approved `24402...` baseline을 배치한 격리 worktree에서 incremental
   LASAL Build를 수행했다. Load 단계에는 active `DriveComL2.h` 누락 `E0015` 1건과 6 warnings가
   남았지만, explicit Build는 두 ST를 compile하고 `0 errors / 24 warnings`, `Compiler Done` 2,
   `Linker Done` 1로 성공했다.
5. Distribution wrapper의 UDP callback `Auto` state 전파 수정은 `e3c9365`에 고정했고,
   수정 뒤 clean PS5.1/PS7 SourceOnly와 full Distribution까지 완료했다.
6. Build가 `Classes.lcb`를 approved `24402...`에서 `5337...`로 바꿨고 official comparator는
   Gate D target inequality의 `REJECTED_BOUNDARY_OR_CONTRACT_DRIFT`, exit `3`으로 거부했다.
7. **PLC download 및 실제 Connect → Close → Connect 검증은 수행하지 않았다.** 따라서
   실장비 해결로 판정할 수 없다.

다음 우선순위는 release tooling이나 Build 반복이 아니다. C78 project와 IDE가 C82로 보고한
active linked-library artifact의 세대 경계 및 `Classes.lcb` Gate D record 재생성 원인을 검토해
sanctioned artifact transition을 먼저 확보해야 한다. 그 뒤 승인된 동일 image만 download해 real
reconnect를 확인한다. Full
Distribution과 최신 ST compile/link는 이미 완료했으므로 반복 대상이 아니다.

## 이 분석의 범위와 무결성

원본 `docs/history/Elmo_Master_history_260812.md`는 수정하지 않았다. 9,423줄을 250줄씩
38개로 나눴으며 마지막 청크만 173줄이다.

| 항목 | 결과 |
|---|---:|
| 원본 크기 | 891,291 bytes |
| 원본 형식 | UTF-8 no BOM, CRLF, final CRLF |
| 원본 SHA-256 | `6EA8B87D25D4DD550194F3B1ADA5B465A432E6F0A441882AEAEB750552704A19` |
| 청크 합계 | 891,291 bytes / 9,423줄 |
| 바이트 재결합 | 원본과 exact match |
| 재결합 SHA-256 | `6EA8B87D25D4DD550194F3B1ADA5B465A432E6F0A441882AEAEB750552704A19` |

전체 청크와 줄 범위는 [`00_index.md`](00_index.md), 개별 해시는
[`split_manifest.json`](split_manifest.json)에 있다. 세 구간의 직접 읽기 요약은 다음과
같다.

- [`01_chunk_digest_parts_001_013.md`](01_chunk_digest_parts_001_013.md)
- [`02_chunk_digest_parts_014_026.md`](02_chunk_digest_parts_014_026.md)
- [`03_chunk_digest_parts_027_038.md`](03_chunk_digest_parts_027_038.md)

## 전체 흐름

### 1. Gate D 선언·broker·증거 계약

- Gate D declaration exact name/ABI와 terminal-wake broker 계약을 먼저 고정했다.
- 정적 verifier, SourceOnly, WPF/fake-peer, trusted capture를 순차적으로 강화했다.
- 일반 LASAL method는 `Edit Method`/Enter로 열어 header를 확인하고, Object Network의
  Server/Client만 `Find in Implementation`으로 확인하도록 잘못된 smoke 규칙을 교정했다.
- PC/static PASS와 PLC runtime PASS를 일관되게 분리했다.

### 2. `Classes.lcb` 변동과 fail-closed Gate D

- LASAL Rebuild마다 `Classes.lcb`가 `6E...`, `990...`, `13EA...` 등으로 바뀌며 opaque
  vendor field 의미가 불명확한 문제가 반복됐다.
- 이를 임의 승인하지 않고 comparator/finalizer/bundle validator와 exact physical
  snapshot ratchet을 만들었다.
- 최종적으로 clean tracked `24402...` tuple만 승인하고 dirty main은 거부하도록 고정했다.
- 최신 owner-loss source의 격리 Build는 새 `5337...` artifact를 만들었고 Gate D target
  inequality로 exit `3` 거부됐다. Build PASS는 artifact 승인 PASS가 아니다.
- 과거의 Rebuild/Download 로그는 존재하지만 최신 owner-loss source와 결합된 download
  증거가 아니다.

### 3. PC/WPF reconnect 경계

- short ACK의 `ErrorId=-1`을 0으로 숨기던 parser 문제를 고쳤다.
- persistent `0x8080/ErrorId=-1`에만 bounded retry를 적용하고 ErrorId=0, malformed,
  transport, callback failure는 fail-closed로 유지했다.
- X-close, process exit/relaunch, mutex 재획득, fresh TCP, 동일 callback port 재사용을
  fake peer와 실제 EXE process 수준에서 검증했다.
- 다만 100ms fresh-session retry는 PC 정책이지 PLC cleanup 완료 신호가 아니다.

### 4. release pipeline·manual·provenance

- checkout EOL, PowerShell 5/7 차이, solution membership, method-size ratchet,
  `HandleRequest`, callback network snapshot, compiler/Git/Python provenance를 보강했다.
- canonical API manual 2.3을 DOCX/PDF로 재생성하고 semantic·OpenXML·전 페이지 시각
  검증을 거쳤다.
- clean full Distribution은 여러 false blocker를 제거한 뒤 wrapper의 `Auto` state 전파
  결함에서 한 번 끊겼다. 후속 구현에서 child가 관측한 상태를 strict parser로 확정해
  wrapper topology에 전달했고, 같은 clean tree의 PS5.1/PS7 SourceOnly와 full Distribution을
  모두 PASS했다.

### 5. 최신 실제 장애와 owner-loss 수정

- 실제 앱에서 첫 Connect가 네 번 연속 `0x8080/ErrorId=-1`을 받았다. 이 관측은 PC의
  callback UDP port 잔류 가설과 맞지 않고 PLC의 stale callback owner/disarm 상태를
  가리킨다.
- `TCPMotionInterface.st`는 accepted takeover와 definitive current-socket disconnect에서
  정상 fenced disarm이 exact `-8`을 반환할 때만 owner-loss retirement를 시도한다.
- `LMCUdpCallbackSender.st`는 `(SessionEpoch, CookieLo, CookieHi)=(0,0,0)`을 내부
  owner-loss sentinel로 인식하고 그 경우에만 stale fence mismatch를 우회해 endpoint와
  queue를 중앙에서 지운다. 성공 결과 `0/1`일 때만 TCP 측이 정상 disarm을 다시 호출해
  정리를 확인한다.
- ordinary `0x8080`, `0x405D`, exact `-9`, different-IP takeover, retiring/late old socket에는
  이 우회를 적용하지 않는다.

## 2026-08-12 라이브 작업 트리 재확인

아래는 히스토리 주장만 옮긴 것이 아니라 이 분석 중 현재 저장소를 다시 확인하고 격리
검증한 결과다. Main의 사용자 source/artifact는 수정하지 않았다.

| 항목 | 현재 확인값 |
|---|---|
| owner-loss source baseline | `e3c9365b953567b332d6f34f167044d98ef549eb` |
| staged 변경 | 0 |
| handoff commit 뒤 보존할 tracked drift | 1개: 사용자 `Classes.lcb` |
| LASAL2 process | 0 |
| Git worktree | main 1개 |
| tracked Network drift | 0개 |
| 현재 `Classes.lcb` SHA-256 | `D4C1FF4650499777A17854DA638269543938532520F0C5D178D61FF13BAA0C36` |
| 최신 owner-loss source | `e3c9365`에 commit됨 |

현재 `Classes.lcb`는 tracked 파일과 다르며 current verifier의 승인 상태로 간주하면 안 된다.
기존 사용자 `Classes.lcb` 변경과 많은 untracked 실험 자료는 그대로 보존했다. History 분할
산출물은 `docs/history/260812/`에만 만들었고, owner-loss/release 수정은 별도 commit
`e3c9365`에 고정했다.

현재 핵심 working-tree source SHA-256은 다음과 같다.

| 파일 | SHA-256 |
|---|---|
| `TCPMotionInterface.st` | `3463609144F078CBE19B6272973BA06DECF5B4374DE0DF286DB879F65D831C8D` |
| `LMCUdpCallbackSender.st` | `619EAF7D33208135D93D44FD4703FB385EA1115D1D1352825D6F055349C3C296` |
| `Verify-LasalUdpCallbackContract.ps1` | `59E968363FF7706C95E533BEFF127816966548ABB5CDB7399FEC3C0F0AF52433` |
| `Verify-LasalContract.ps1` | `C6D379D9F50BB8A53606879EF7663CADC9C0D29969C21D4F630992F006E7A205` |

## 증거 수준

| 대상 | 상태 | 정확한 해석 |
|---|---|---|
| owner-loss ST 구현 | `e3c9365` | exact `-8` 두 경로와 fail-closed 경계를 source/static으로 확인 |
| UDP static verifier | PS5.1/PS7 `305/305` | 두 host 장시간 suite PASS; PLC 증거 아님 |
| strict Auto parser | `12/12` positive, `15/15` negative | 모순 state/prefix/approval tuple을 fail-closed 거부 |
| SourceOnly | PS5.1/PS7 PASS | default `Auto`가 `TerminalWakeBrokerCandidate`를 wrapper에 전파 |
| Distribution pipeline | clean full Distribution PASS | preflight `14/14`, 94 source files, semantic 18, actual EXE/candidate transaction PASS |
| WPF fixed-port reconnect | 신규 두 test 각각 `1/1` PASS | fake RPC/PC 회귀이며 PLC 증거가 아님 |
| LASAL project load — 격리 worktree | command 성공 / load-only error | active `DriveComL2.h` read `E0015` 1건과 warnings 6건; project는 열림 |
| LASAL build — 최신 두 ST | compile/link PASS | `0 errors / 24 warnings`, 두 ST compile, `Compiler Done` 2, `Linker Done` 1; PLC 증거 아님 |
| post-build artifact gate | FAIL / STOP | `Classes.lcb=5337...`, comparator boundary/contract drift exit `3`, focused verifier exit `1` |
| history splitter replay | PS5.1/PS7 PASS | 각 38개, byte/hash rejoin이 원본 SHA `6EA8B87D...`와 일치 |
| PLC download — 최신 두 ST | 미수행 | PLC에 최신 수정이 들어갔다고 볼 수 없음 |
| 실제 Connect → Close → Connect | 미수행 | 사용자 장애 해결 여부 미확정 |
| wrapper Auto-state 수정 후 clean full Distribution | 완료 | source tree `47e6c141...`, release manifest `F94608C1...` |

### 격리 LASAL 시도의 정확한 증거

| 항목 | 결과 |
|---|---|
| LASAL / target | Class 2 `02.03.002` / C78 ARM project |
| pre-build `Classes.lcb` | 8,549,773 bytes / `24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861` |
| post-build `Classes.lcb` | 8,549,824 bytes / `5337BBAFE88DB10D47308ED2BED89F7B7C22BFE66D7CA739D3A872276DA308E5` (+51 bytes) |
| 두 ST | `TCPMotionInterface.st` `34636091...D831C8D`; `LMCUdpCallbackSender.st` `619EAF7D...3C296` |
| Load UI | command succeeded; `Done - 1 error(s), 6 warning(s)` |
| load-stage error | `MotionLib/Include/global.h(15)` -> missing active `Hardware/Class/_DriveMngBase/DriveComL2.h`, `E0015` |
| warning breakdown | linked `MotionLib` change/`ReducedClientDependency=false` 1개 + 5 installed libraries C82 vs project C78 |
| 재현성 | repository tracked/ignored project image와 timestamp를 worktree에 맞춘 exact input |
| 이전 multi-attempt append | load 시도 5회 포함, baseline 2,004,062 bytes 뒤 2,730,698 bytes; segment SHA-256 `6F0FC6754E95AF808CBBC912AF1DFDCC1D7256F6E68E024D0128B63966644C52`; 당시 final total 4,734,760 bytes |
| final isolated session append | 690,562 bytes / SHA-256 `1F24F1EAC8B5E983AC830F78C6739EEA3C1A2A8AF75A9CCBA0E1CAC4081DDC26`; final log 5,425,322 bytes |
| Build bounded window | 22,798 bytes / SHA-256 `B7A1F6FAFB162A9CCCC6CA429F32EFB22A1016737F449B791EF13B7B73ED24C5`; `0 errors / 24 warnings`, `Compiler Done=2`, `Linker Done=1` |
| generated LBA | TCP 539,035 bytes / `2A5AC668...D12AEF`; sender 255,550 bytes / `3D1CEE13...DAC33` |
| artifact 판정 | `REJECTED_BOUNDARY_OR_CONTRACT_DRIFT`, comparator exit `3`; Gate D target unequal, protected dependency equal |
| 종료 | library 제거 거부, Close Project succeeded, LASAL process 0, Connect/Download 0, 격리 worktree 제거 완료 |

해당 header는 active library path에 없었다. 다른 설치 세대의 파일을 active path로 복사하면
installed library/dependency generation을 임의 혼합하므로 수행하지 않았다. 이번 결과는 이
load-only error와 별개로 source compile/link가 성공할 수 있음을 확인했지만, IDE-reported
linked-library C82와 project C78의 세대 정합성을 승인한 것은 아니다.

또한 최초 ordinary disarm exact `-8` 뒤 sentinel 결과는 function-local이고, 그 결과가
`0/1`이면 confirmation helper를 재호출해 `RpcCallbackLastDisarmResult`를 다시 쓴다. Sentinel
negative이면 재호출하지 않는다. Intermediate `-8` 또는 동등한 persistent attempt/result
trace를 얻지 못하면 runbook 기준 exact owner-loss branch는 미증명이다.

## 이어서 진행할 순서

### A. 실제 reconnect 문제를 먼저 닫기

1. 현재 dirty `Classes.lcb`와 사용자 변경을 덮어쓰거나 재기준화하지 않는다.
2. 축 idle/power-off 등 기존 runtime runbook의 안전 전제를 확인한다.
3. IDE가 C82로 보고한 active linked-library artifact와 C78 project의 세대 경계, missing active
   header와 Build 시 `Classes.lcb` Gate D record 재생성 원인을 vendor 자료 또는 검토된
   설치본으로 분류한다.
   다른 세대 파일을 임의 복사하지 않는다.
4. `5337...`를 승인하거나 hash-only rebaseline하지 않는다. Reviewed transition과 sanctioned
   artifact 판정 없이는 Build/Rebuild/finalizer/Download를 반복하지 않는다.
5. exact `-8` runtime branch에는 intermediate `-8` 또는 persistent attempt/result/confirmation
   trace가 필요하다. 새 declaration이 필요하면 위 reviewed `Classes.lcb` transition과 함께 한다.
6. 승인된 동일 image만 PLC에 1회 download하고 새 boot/session 기준을 기록한다.
7. 같은 WPF 창과 같은 callback UDP port로 normal Close의
   Connect → Close → Connect를 먼저 실행한다.
8. 안전하게 재현 가능할 때만 `0x405D/ErrorId=-1` close 경계도 같은 절차로 확인한다.
9. 실패하면 `RpcCallbackLastDisarmResult`, `RpcSocket`, current socket, callback armed/depth,
    requested/bound callback endpoint를 함께 캡처한다.
10. `RpcCallbackLastDisarmResult=-9`이면 CallbackSender Network linkage를 확인한다.
    `-9`나 different-IP 경로를 강제 owner-loss로 우회하지 않는다.

완료 판정은 “build 성공”이 아니라 실제 second Connect 성공과 callback 재등록/수신까지다.

### B. 완료된 release pipeline 증거 보존

1. strict parser와 실제 child state 전파 focused 검증을 완료했다.
2. clean 검증 clone에서 PS5.1/PS7 SourceOnly와 full Distribution을 완료했다.
3. Candidate/staging/lock rollback, canonical manifest 불변과 process residue 0을 확인했다.
4. 격리 LASAL Build, artifact STOP과 해시를 기록한 뒤 임시 worktree를 등록 해제·제거했다.

## 재개용 한 문장

> exact `-8` stale callback owner-loss 정리, dual-host 정적 회귀와 clean full Distribution은
> `e3c9365` tree로 마감했고, 격리 C78/ARM incremental Build도 최신 두 ST compile/link를
> `0 errors / 24 warnings`로 통과했다. 그러나 post-build `Classes.lcb=5337...`는 Gate D target
> boundary drift로 comparator exit `3`이므로 Download 금지다. Reviewed artifact transition,
> exact `-8` persistent trace, PLC download와 실제 Connect-Close-Connect 재등록/수신이 남아 있다.
