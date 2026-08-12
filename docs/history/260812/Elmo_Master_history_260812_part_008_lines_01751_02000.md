- [SIGMATEK_LASAL_coding_rules.md](./docs/architecture/SIGMATEK_LASAL_coding_rules.md:187)
- [SIGMATEK_LASAL_programming_error_prevention_guide.md](./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md:228)
- [Gate D runtime runbook](./LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md:82)
- [Gate D verification summary](./test/Reports_Lasal/C78_20260810_udp_callback_gate_d/verification_summary.md:15)

현재/재사용 검증 문서에도 같은 규칙을 전파했습니다. `git diff --check`와 cached check는 통과했고, 충돌 마커와 stale한 전역 “Find 사용 불가” 문구는 없습니다. 파일은 stage/commit하지 않았습니다.

> 테스트 해야되는 리스트부터 알려줘

<details><summary>14 previous messages</summary>

> 규칙 수정이 끝났으니 Gate D 개발 흐름을 다시 이어갑니다. 먼저 현재 작업공간·LASAL 종료 상태·method-smoke 산출물과 설계문서의 다음 gate를 재확인한 뒤, 사용자에게 동일 UI 작업을 다시 요구하지 않고 가능한 자동 검증부터 닫겠습니다.
>
> 현재 LASAL은 종료 상태이고, Gate D focused pin과 capture tool identity도 일치합니다. 이제 manifest를 만들지 않는 bootstrap `ValidateOnly`를 안정된 현재 snapshot에서 다시 실행합니다. 이 검증이 통과해야 tooling-only commit과 trusted sequence-4 캡처로 넘어갈 수 있습니다.
>
> method-smoke 자동 artifact는 현재 만들 수 없다는 것이 확인됐습니다. 기존 수동 확인 세션은 baseline의 pre-extension에 들어 있고, post-baseline 로그 증분은 0바이트라서 현재 도구는 fail-closed 합니다. 수동 확인은 유효하게 유지하고, 이를 새 세션처럼 꾸며 자동 PASS로 만들지는 않겠습니다. 이 artifact는 sequence-4 캡처의 필수 조건이 아니므로 캡처 검증은 계속 진행합니다.
>
> bootstrap `ValidateOnly`는 정상적으로 self-test 구간을 실행 중이며, 현재까지 manifest·임시 stage·Git stage는 생성되지 않았습니다. 이 실행이 끝날 때까지 worktree에는 쓰지 않고 결과를 기다립니다.
>
> 다음 gate 감사에서 문서의 중요한 stale 상태도 확인했습니다. 현재 생산 전이는 4파일이 아니라 의도한 Network 배치를 포함한 정확히 7파일이며, 최신 strict Rebuild 증거도 PID 7288이 아니라 `GateDVisualLayout` PID 480입니다. bootstrap 종료 후 이 현재 증거로 문서를 교정하고, 7파일 기준 full-static을 실행하겠습니다.
>
> 테스트 해야되는 리스트부터 알려줘
>
> 먼저 테스트 목록을 확정하면 아래 순서입니다. **지금은 아직 Download하지 마세요.** 제가 sequence-4 정적 캡처까지 닫은 뒤 1회 Download부터 시작합니다.
>
> 1. 준비/기준값 기록
>    - 모든 축·그룹·로봇 정지 및 Power Off, 대기 명령 없음
>    - WPF TCP 4000 / UDP 5000, 실제 TCP source IP와 callback IP 일치
>    - BootId, SessionEpoch, TicketId와 PLC/WPF 카운터 baseline 기록
>    - Wireshark: `ip.addr==<PLC_IP> && (tcp.port==4000 || udp.port==5000)`
>
> 2. 연결 및 callback 등록
>    - WPF Connect
>    - `0x8080 → 0x405C`
>    - protocol v2, mask bit 1, nonzero BootId/SessionEpoch, sender Armed 확인
>
> 3. GD-01 정상 wake
>    - low-level `Submit SDO Read`
>    - Slave 1–4 중 시험축, `0x6061:0`, `Int8`, length `1`, timeout `1000`
>    - 정확한 52-byte UDP `LMC2`, `EventId=TicketId`
>    - UDP 뒤 TCP `0x7E03` 정확히 1회
>    - UI는 UDP 수신만으로 바뀌지 않고 `0x7E03` 응답 뒤 Completed로 변경
>    - 기대 delta: Attempt `+1`, Enqueued `+1`, Rejected `+0`
>
> 4. GD-02 연속 두 ticket
>    - 서로 다른 SDO Read 2회
>    - 각 TicketId마다 UDP 1개와 `0x7E03` 1회
>    - 중복 wake·재시도 없음
>    - 기대 delta: Attempt/Enqueued 각각 `+2`, Rejected `0`
>
> 5. GD-05A UDP loss + 수동 복구
>    - TCP는 유지하고 inbound UDP 5000만 일시 차단
>    - 자동 `0x7E03`과 UI terminal 갱신이 없어야 함
>    - 같은 retained ticket의 `Refresh Ticket` 1회
>    - 새 `0x7E50` 제출 없이 `0x7E03` 1회로 결과 복구
>
> 6. GD-05B polling fallback
>    - UDP 없이 `Read SDO Inline (wait terminal)` 경로 확인
>    - bounded `0x7E03` polling으로 완료
>    - 이 항목은 polling 증거이며 callback wake 증거로 취급하지 않음
>
> 7. GD-04 callback disarm
>    - WPF `Close` 1회
>    - `0x405D` 확인
>    - callback tuple과 sender endpoint가 clear되고 늦은 old UDP가 상태를 바꾸지 않아야 함
>
> 8. 재연결
>    - 새 연결에서 새 SessionEpoch/BootId 확인
>    - 이전 ticket/session wake 무시
>    - 새 `0x6061:0` read로 GD-01 재통과
>
> 9. 별도 도구가 필요한 음성 테스트
>    - duplicate, reorder, stale/foreign BootId·session·ticket: PC fake/proxy
>    - `EventId=0`, sender unavailable, pending-close race: 승인된 PLC harness
>    - same-IP takeover: WPF 단일 인스턴스 제한 때문에 승인된 second client 필요
>    - different-IP takeover rejection: 서로 다른 실제 source IP client 필요
>    - 승인된 nonexistent read-only object가 없으면 GD-03은 `NOT RUN`
>
> 각 항목마다 반드시 함께 남길 것은 `pcap + PLC counter delta + WPF log/UI + BootId/SessionEpoch/TicketId`입니다. 현재 실행 중인 bootstrap 검증은 계속 진행하고 있습니다.
>
> bootstrap `ValidateOnly`가 통과했습니다.
>
> - 상태: `TerminalWakeBrokerCandidate`
> - LASAL PID: 0
> - output/manifest 생성: 없음
> - planned manifest: `3,225,878 bytes / E0490DC3…E508E4`
> - 신뢰 등급: uncommitted tool을 사용한 `UNTRUSTED` preflight
>
> 이제 최신 `GateDVisualLayout` 7파일 증거로 문서를 교정하고 full-static을 실행한 뒤, trust-anchor 두 파일의 commit 경계를 준비하겠습니다.
>
> 사용자가 수행할 테스트 목록은 확정했고, 지금은 그 시험에 들어가기 전 정적 gate를 닫는 단계입니다. `GateDVisualLayout` bounded Rebuild 증거와 전체 LASAL 정적 계약을 한 명령으로 재검증하겠습니다.
>
> full-static은 Gate D 자체가 아니라 일반 정적 verifier의 기존 결함에서 중단됐습니다. Gate D focused current 검증과 method-size/ownership 단계는 통과했지만, `Verify-LasalContract.ps1`가 초기화되지 않은 `$stage87AdapterCallPattern`을 참조합니다. 이 실제 blocker를 수정하고 같은 명령을 재실행하겠습니다.
>
> 독립 소스 감사 결과, 현재 7개 production 파일에는 설계 위반 blocker가 없습니다. one-attempt receipt, 두 호출 위치, pending-close/session/BootId fence, `EventId=0` 거부, counter 및 Network topology가 모두 일치합니다. 지금 수정 중인 것은 production 코드가 아니라 full-static verifier의 변수 scope 오류 하나입니다.
>
> verifier 원인은 정확히 확인됐고 수정했습니다. 정규식 패턴이 잘못된 함수의 지역 scope에 정의돼 사용 함수에서 strict-mode 미정의가 났던 문제입니다. PS5/PS7 AST와 self-test 67/67은 통과했습니다. 이제 동일한 GateDVisualLayout full-static을 처음부터 다시 실행합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 다운로드는 언제 진행하면되냐
</details>

아직 다운로드하면 안 됩니다.

다운로드 시점은 제가 아래 작업을 끝낸 뒤입니다.

1. Gate D 검증 도구 2개를 별도 커밋
2. Trusted `ValidateOnly` PASS
3. Sequence-4 체크포인트 manifest 캡처
4. LASAL 변경 7개 파일과 manifest를 하나의 커밋으로 고정
5. 최종 해시·Git 상태 확인

그다음 제가 **“다운로드 가능”**이라고 알리면:

- 전체 축/그룹 정지 및 Power Off
- WPF 연결 해제
- LASAL에서 승인된 이미지로 **Download 1회**
- 다운로드 후 별도 Reset/Rebuild/Restart는 하지 않음
- 새 `BootId` 확인
- WPF 연결 후 `0x6061:0` SDO Read부터 런타임 시험 시작

리빌드와 세 메서드 Implementation 확인은 이미 완료된 것으로 반영했습니다. 지금부터 다운로드 전 체크포인트 고정 작업을 이어가겠습니다.

> 이제 다운로드 해도되냐

<details><summary>46 previous messages</summary>

> 다운로드 전 마지막 게이트를 계속 진행합니다. 우선 현재 작업공간과 실행 중인 검증 프로세스를 다시 고정 확인한 뒤, 검증 도구 2개만 별도 커밋하고 trusted 검증·체크포인트 캡처·7개 생산 파일 원자 커밋 순서로 진행하겠습니다. 이 동안 LASAL은 계속 닫아 두세요.
>
> 현재 재확인 결과는 좋습니다. LASAL 0개, manifest/임시 stage 0개, index 비어 있음, 두 trust-anchor 해시가 최종 핀과 일치하고 전체 `diff --check`도 통과했습니다. 이제 두 파일의 Gate D 핀·schema·전이 계약만 마지막으로 확인한 뒤 정확히 그 두 파일만 stage하겠습니다.
>
> 두 trust-anchor 파일만 stage됐고, 다른 파일은 index에 들어가지 않았습니다. staged whitespace 검사도 통과했습니다. 지금 이 두 파일만 `test(lasal): freeze Gate D callback checkpoint tools`로 고정하겠습니다.
>
> trust-anchor 커밋은 `bb5fd93`이며 두 도구가 HEAD와 완전히 일치합니다. LASAL 0개, stage 0개, manifest 없음도 재확인했습니다. 이제 bootstrap 옵션 없이 trusted `ValidateOnly`를 실행합니다. 이 검증은 약 10–15분 걸릴 수 있으며, 작업공간은 끝날 때까지 그대로 유지합니다.
>
> trusted `ValidateOnly`가 정상 실행 중입니다. 아직 오류 출력은 없고, 내부 verifier self-test 구간을 진행하고 있습니다. manifest를 만드는 경로가 아니므로 현재까지 생산 파일이나 stage 변화는 없습니다.
>
> 검증 프로세스는 계속 살아 있고 조기 실패 신호는 없습니다. 이 단계는 focused verifier의 전체 음성 fixture와 현재 7파일 전이 계약을 다시 실행하므로 시간이 걸립니다. 작업공간은 고정 상태로 유지 중입니다.
>
> 아직 출력 없이 정상 진행 중입니다. trusted 실행은 이전 bootstrap보다 더 엄격하게 committed-clean 도구 identity와 Gate C→D 전이 7개 경로를 함께 검사합니다. 종료 결과가 나올 때까지 다른 파일은 건드리지 않겠습니다.
>
> 현재까지 stderr/stdout 모두 비어 있고 프로세스가 중단되지 않았습니다. 이는 기존 장시간 self-test 패턴과 일치합니다. manifest는 여전히 생성하지 않는 `ValidateOnly` 단계입니다.
>
> 검증은 계속 진행 중이며 아직 조기 실패가 없습니다. 이번 결과가 PASS하면 바로 같은 고정 상태에서 physical sequence-4 manifest 캡처로 넘어갑니다.
>
> 현재 약 5분 경과했습니다. 이전 동일 verifier 전체 실행은 7–14분 범위였으므로 아직 정상 범위입니다. 작업공간과 LASAL 상태는 변경하지 않고 있습니다.
>
> self-test가 계속 진행 중이고 오류 출력은 없습니다. 이 결과가 끝나기 전에는 stage/commit/capture 어느 것도 수행하지 않습니다.
>
> 검증 프로세스는 여전히 정상 실행 중입니다. committed-clean trust-anchor와 현재 생산 대상 해시를 동시에 확인하는 구간이라 CPU 작업 시간이 길지만, 현재까지 실패 징후는 없습니다.
>
> 약 8분 경과했습니다. 이전 bootstrap에서 확인된 장시간 경로와 동일하게 진행 중이며, 출력은 성공 또는 실패가 확정될 때 한 번에 나오는 형태입니다.
>
> 아직 실행 중입니다. 현재 단계가 끝나면 `toolTrust=committed-clean`, `outputCreated=false`, `state=TerminalWakeBrokerCandidate`를 모두 확인해야만 캡처로 진행합니다.
>
> 약 10분 경과했고 여전히 오류 출력 없이 동작 중입니다. 타임아웃은 30분으로 잡아 두었으므로 검증을 중간에 끊지 않고 완료 결과를 받겠습니다.
>
> trusted verifier가 계속 실행 중입니다. 현재 파일·Git index·LASAL을 건드린 작업은 없으며, 체크포인트 출력도 아직 생성되지 않았습니다.
>
> 약 12분 경과했습니다. 이전 bootstrap 완료 시간이 약 14분이었으므로 마무리 구간으로 보고 있습니다. 결과가 나오면 manifest 부재와 status 안정성까지 즉시 재확인하겠습니다.
>
> 계속 정상 실행 중이며 stderr는 없습니다. 이 검증 자체가 오래 걸리는 것이고, 현재까지 실패나 외부 변경 신호는 없습니다.
>
> 약 14분 경과했습니다. 내부 전체 fixture가 끝나는 시점에 단일 PASS/FAIL 요약이 출력됩니다. 프로세스를 그대로 완료시키고 있습니다.
>
> trusted `ValidateOnly`가 845.5초 후 PASS했습니다.
>
> - `toolTrust=committed-clean`
> - `state=TerminalWakeBrokerCandidate`
> - `Lasal2PID=0`
> - `outputCreated=false`
> - planned manifest: 3,224,159 bytes / `A67AD147…B5344`
>
> 이제 status가 정확히 그대로이고 임시 파일이 없는지 확인한 다음, 같은 상태에서 실제 sequence-4 manifest를 캡처합니다.
>
> status 해시가 trusted 검증 전후 동일하고, manifest/임시 파일/stage 모두 0입니다. 이제 실제 `-Capture`를 실행합니다. 완료되면 새 manifest의 해시, 내부 7경로 전이, tool trust, Git stage 상태를 검증한 뒤에만 생산 커밋으로 넘어갑니다.
>
> physical capture가 정상 실행 중입니다. 아직 오류 출력은 없고, manifest publish 전 전체 verifier/self-test를 다시 수행하는 구간입니다. LASAL과 작업 파일은 그대로 두고 있습니다.
>
> 캡처 프로세스는 계속 정상 동작 중입니다. 성공 시 마지막 단계에서만 create-new 방식으로 manifest를 쓰고 즉시 재검증하므로, 중간 출력이 없는 것이 정상입니다.
>
> 현재까지 조기 실패 신호는 없습니다. trusted `ValidateOnly`와 동일한 7경로·Sender/Diagnostics identity·Gate C 부모 전이를 다시 확인한 뒤 실제 파일을 봉인합니다.
>
> 약 4분 경과했습니다. 이전 trusted 검증과 비슷한 실행 시간으로 예상되며, 작업공간은 캡처 시작 snapshot 그대로 유지 중입니다.
>
> 캡처는 계속 진행 중이고 stderr/stdout은 아직 없습니다. 성공 결과가 확정될 때까지 별도 검사 명령도 실행하지 않아 snapshot을 흔들지 않겠습니다.
>
> 약 6분 경과했습니다. verifier 전체 fixture가 CPU에서 실행되는 구간이며, 현재까지 실패나 외부 변경 신호는 없습니다.
>
> 이제 다운로드 해도되냐
>
> 아직 다운로드하면 안 됩니다. 지금 trusted physical sequence-4 캡처가 실행 중이고 아직 완료 출력이 나오지 않았습니다.
>
> 남은 조건은 두 가지입니다.
>
> 1. 캡처 PASS 및 manifest 검증
> 2. 정확한 생산 7파일 + manifest 원자 커밋
>
> 두 조건을 끝내고 제가 **“지금 다운로드 가능”**이라고 명시한 뒤에만 1회 다운로드하세요. 현재 LASAL은 계속 닫아 두면 됩니다.
>
> 캡처는 계속 정상 실행 중이며 조기 실패 출력은 없습니다. 완료 직후 manifest 내부 전이와 해시를 검사하고 생산 커밋까지 바로 이어가겠습니다.
>
> 약 9분 경과했습니다. trusted ValidateOnly가 14분 정도 걸렸으므로 여전히 정상 범위입니다. 다운로드 승인 전 마지막 캡처 게이트를 그대로 완료시키고 있습니다.
>
> 캡처 프로세스는 살아 있고 오류 출력이 없습니다. 이 단계가 끝나도 바로 다운로드가 아니라, 생성된 manifest와 7개 생산 파일을 한 커밋으로 고정한 뒤 최종 확인이 필요합니다.
>
> 약 11분 경과했습니다. 현재까지 캡처 snapshot을 깨는 파일 변경이나 LASAL 실행은 없었습니다. 최종 PASS/FAIL 출력을 기다리고 있습니다.
>
> 계속 정상 진행 중입니다. 성공하면 manifest가 create-new로 생성되고, 도구가 즉시 재독해 hash와 commit-binding을 검증합니다. 그 결과까지 확인해야 합니다.
>
> 약 13분 경과했습니다. 이전 검증 시간에 근접했고 아직 오류는 없습니다. manifest 생성 여부는 프로세스 종료 후에만 검사하겠습니다.
>
> 캡처가 마무리 구간에 들어간 것으로 보입니다. 프로세스는 여전히 정상 실행 중이며, 중단 없이 최종 출력까지 기다립니다.
>
> 약 15분 경과했습니다. 30분 제한 안에서 정상 실행 중이며, 현재까지 stderr가 없습니다. 완료 후 생산 커밋 검증까지 제가 이어서 처리합니다.
>
> 캡처는 아직 실행 중입니다. trusted 검증보다 publish·재독·commit-binding 검사가 추가되어 조금 더 오래 걸릴 수 있습니다. 실패 징후는 없습니다.
>
> 약 17분 경과했습니다. 프로세스를 강제로 끊을 이유가 없고 정상 범위 내이므로 그대로 완료시키고 있습니다. LASAL은 계속 닫힌 상태를 유지해 주세요.
>
> 계속 실행 중이며 오류 출력은 없습니다. 성공 결과가 나오기 전에는 다운로드 승인하지 않습니다.
>
> physical sequence-4 캡처가 1,006초 후 PASS했습니다.
>
> - `finalCommitGuardEqual=true`
> - `atomicMoveLast=true`
> - manifest 3,224,159 bytes
> - SHA-256 `5CD3EC2C…9944A4`
> - LASAL PID 0
>
> 이제 manifest 내부의 Gate C→D 정확한 7경로 전이와 도구/생산 identity를 재검증합니다. 그 뒤 정확히 생산 7파일과 이 manifest만 stage·커밋하겠습니다. 아직 다운로드는 잠시 기다려 주세요.
>
> manifest 재검증 결과도 정확합니다.
>
> - trust: `committed-clean`
> - parent: `GateC_DerivedCandidate`
> - current: `GateD_TerminalWakeBrokerCandidate`
> - 변경 경로 수: 정확히 7
> - Sender/raw cross-check: true
> - 입력 안정성·atomic move: true
> - 최종 LASAL PID: 0
>
> 이제 7개 생산 파일의 실제 해시를 manifest와 대조한 뒤 manifest를 포함한 8개 경로만 stage합니다.
>
> 방금 중단은 파일 변조가 아니라 제가 Sender 예상 해시를 수기로 잘못 적은 탓입니다. 실제 파일은 캡처가 봉인한 `C334A6C6…`이고, capture PASS 이후 파일이 바뀐 증거는 없습니다. 수기 상수 대신 manifest 안의 각 artifact identity를 직접 읽어 7개 파일을 다시 대조하겠습니다.
>
