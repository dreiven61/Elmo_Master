>
> Source: automatic
> Status: completed
>
> PC 회귀는 Delivery `1111/1111`, WPF `332/332` 모두 통과했습니다. 선언 검증기도 Windows PowerShell 5.1과 PowerShell 7에서 각각 `17/17` 통과했고, 예전 이름 흔적·storage 0은 빌드 전 메타데이터라 ABI 실패 사유가 아닌 것으로 정리했습니다. 지금은 Gate D 구현 전용 검증기와 전체 LASAL 정적 검증기를 마무리하고 있습니다.
>
> 구현 diff를 다시 대조하면서 문서의 과거 문장 하나가 현재 상태와 충돌하는 것을 찾았습니다. “production caller가 아직 없다”는 Gate D 구현 전 표현이라, 현재는 “후보 호출자는 생겼지만 검증·빌드·PLC 증거 전에는 활성화 승인 아님”으로 바로잡겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md` (+6 -5)
>
> </details>
>
> 소스 자체는 현재 설계 계약과 일치합니다. 특히 종료 예정 소켓을 잘못 깨우지 않도록 `CurrentSock <> 0`과 `PendingClosedSessionEpoch = 0`까지 broker fence에 포함했고, terminal tuple은 먼저 1회 소비한 뒤 enqueue 성공/거절 카운터로만 귀결되며 재시도·outbox는 만들지 않습니다. 아직 남은 핵심은 이 구조를 양성/음성 fixture로 고정하는 검증기 통과입니다.
>
> 전용 verifier의 Gate D 양성 fixture와 PowerShell 구문 검사는 이미 통과했습니다. 첫 전체 self-test는 실패가 아니라 120초 제한에 걸려 중단된 상태라, 제한을 늘려 음성 fixture까지 전수 실행 중입니다. 전체 LASAL verifier도 구문 검사는 통과했고 지금 실제 SourceOnly 첫 실행에 들어갔습니다.
>
> 전수 self-test와 SourceOnly는 현재 실제 PowerShell 프로세스로 계속 실행 중입니다. LASAL 소스 3개는 별도로 재검사해 `7-bit ASCII`, 전 행 `CRLF`, final CRLF, bare LF 0을 확인했고, 정의/호출 수는 `TryTake 1`, broker 정의 1·호출 2, `PublishEvent` 호출 1, `EventId=0` 차단 1로 정확합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+3 -2)
>
> </details>
>
> 양성 fixture뿐 아니라 실제 작업트리도 별도로 확인하겠습니다. 전용 verifier를 `TerminalWakeBrokerCandidate` 상태로 실행해, 현재 소스·생성 ABI·보호 파일·Network 무변경을 한 번에 대조합니다.
>
> 실제 작업트리 전용 검증은 통과했습니다: 상태가 정확히 `TerminalWakeBrokerCandidate`, LASAL IDE 종료, `ProductionApproved=false`, `NeedsRebaseline=true`로 판정됐습니다. `Classes.lcb` ABI와 프로젝트 DB를 읽었고, `.lcp`·Include·Network·보호 파일은 모두 기존 계약과 일치했습니다. 이는 정적 후보 통과이며 C78 빌드나 PLC 런타임 통과를 뜻하지는 않습니다.
>
> 전체 LASAL 정적 verifier도 마무리됐습니다. Gate D 양성 1개와 신규 음성 7개가 모두 통과했고, 실제 `SourceOnly` 전수 검증도 249.5초 후 exit 0으로 끝났습니다. 기존 Gate A/B/C 상태는 조건부로 유지되어 과거 계약을 Gate D 규칙으로 덮어쓰지 않았습니다.
>
> 전용 verifier 전수 결과도 확정됐습니다: self-test PASS, 음성 fixture `276/276` 전부 정상 거부, 실제 트리 `TerminalWakeBrokerCandidate` PASS입니다. 특히 Gate C의 역사적 `EventId=0` 허용은 유지하고 Gate D에서만 `EventId=0 → -6`으로 전환해 과거 증거를 훼손하지 않았습니다. 잔류 테스트 프로세스도 없습니다.
>
> 캡처 절차 감사에서 실제 결함 하나를 찾았습니다. Gate D가 새로 바꾼 `LMCDiagnosticsService.st`를 verifier는 읽지만, sequence-4 manifest에는 그 파일의 명시적 artifact/hash와 verifier 교차검증이 빠져 있습니다. 전체 디렉터리 hash로 간접 봉인되기는 하지만 Gate D 핵심 production surface이므로, C78 전에 이 누락을 보완하고 verifier 출력에도 Diagnostics identity를 추가하겠습니다.
>
> 검증 결과를 기존 자동시험 문서에도 현재 날짜 기준으로 분리 기록하겠습니다. 정적/PC 통과와 C78·PLC 미실행을 같은 문장에 섞지 않고 별도 gate로 남깁니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+12 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+3 -2)
>
> </details>
>
> <details><summary>Called 2 tools</summary>
>
> MCP tool call
>
> node_repl.js_reset
>
> ```json
> {}
> ```
>
> ```text
> js kernel reset
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 앱 상태 확인",
>   "code": "if (!globalThis.sky) {\n  const { sky } = await import(\"@oai/sky\");\n  globalThis.sky = sky;\n}\nvar appsGateD = await sky.list_apps();\nnodeRepl.write(appsGateD.filter(a => /lasal/i.test((a.displayName||\"\")+\" \"+a.id)));"
> }
> ```
>
> ```text
> Computer Use was stopped by the user with the physical Escape key. Stop your work, do not call further Computer Use tools in this turn, and send a final message noting that the user stopped Computer Use.
> ```
>
> </details>
</details>

컴퓨터 제어가 `Esc`로 중단되어 LASAL IDE 작업은 즉시 멈췄습니다. C78 Rebuild·Link·Download는 실행하지 않았습니다.

현재까지 완료:

- Gate D source 구현
- 전용 verifier: PASS, 음성 fixture `276/276`
- 전체 LASAL SourceOnly: PASS
- PC Delivery: `1111/1111`
- WPF: `332/332`
- 실제 트리: `TerminalWakeBrokerCandidate`
- 상태: `ProductionApproved=false`, `NeedsRebaseline=true`

남은 작업은 C78 Rebuild/IDE smoke와 sequence-4 checkpoint capture, 이후 PLC runtime 검증입니다.

> 빌드해서 이상없는거 확인했어. 이제 내가 어떤거 테스트 해야되는지 리스트 알려주고 너는 개발 이어서 진행해.

<details><summary>72 previous messages</summary>

> 확인했습니다. 빌드 성공은 C78 정적/컴파일 gate로만 반영하고, PLC 동작 확인 항목은 별도로 정리하겠습니다. 먼저 빌드 이후 생성물·로그·현재 diff를 재검증한 뒤, 사용자가 바로 실행할 테스트 목록을 우선순위대로 드리고 저는 Gate D checkpoint와 남은 검증 도구를 계속 닫겠습니다.
>
> 빌드만 한 상태라면 실제 동작 시험 전에는 새 프로젝트를 PLC에 Download하고 재시작해야 합니다. 그 전까지는 아래 목록의 “정적 확인”까지만 완료된 것입니다.
>
> 사용자가 우선 실행할 runtime 핵심 순서는 다음입니다.
>
> 1. 정상 terminal wake: callback v2를 `mask=1`, datagram `52`로 등록 → D5 Read 1건 완료 → UDP `LMC2` 52바이트 1건 → 같은 TicketId로 TCP `0x7E03` 조회 1회 → UI 최종 상태 일치.
> 2. callback 미등록/Disarm: D5는 정상 완료되지만 UDP는 0건, polling으로 결과 확인, 재등록 뒤에도 이전 ticket을 재전송하지 않아야 함.
> 3. 실패 terminal: 안전하게 실패시킬 수 있는 D5 Read 1건에서 Failed terminal UDP가 정확히 1건이고, 최종 실패 원인은 TCP `0x7E03` 결과만 반영.
> 4. 중복 UDP: 같은 packet을 2회 넣어도 authoritative 조회/UI 완료가 중복되지 않아야 함.
> 5. UDP 유실: packet을 버려도 polling fallback으로 최종 상태를 찾아야 하며 command truth가 달라지면 안 됨.
> 6. stale/foreign: 이전 TicketId, 잘못된 BootId/session, 다른 source IP packet은 모두 무시되고 현재 UI를 덮어쓰지 않아야 함.
> 7. 재연결: old session packet은 무시, 새 연결에서 callback 재등록 후 새 ticket만 wake. same-IP takeover와 different-IP rejection은 별도 시험.
> 8. 종료 중 연결: pending-close/disconnect 시 terminal이 생겨도 retiring session에 enqueue하지 않고 재시도도 하지 않아야 함.
> 9. 카운터: 온라인 watch에서 `Attempt = Enqueued + Rejected`(포화 전), 정상 1건은 Attempt/Enqueued +1, local mismatch·sender 거절은 Attempt/Rejected +1.
> 10. 안전 경계: UDP만으로 UI/PLC 상태를 완료 처리하지 않고, 반드시 TCP `0x7E03` 응답 뒤에만 최종 상태가 바뀌어야 함.
>
> 정확한 조작값과 패킷 필드까지 포함한 실행표는 현재 소스와 WPF UI 명칭에 맞춰 다시 뽑고 있습니다. 위험한 D5 Write 대신 가능한 한 read-only 경로로 구성하겠습니다.
>
> 로그에서 중요한 구분이 확인됐습니다. 10:35 C78 Build와 내부 Linker는 실행됐고 변경 3개 class가 실제 compile됐습니다. 동시에 Download 명령도 두 번 기록되어 첫 번째는 전송 성공 기록이 있고, 두 번째는 실패했습니다. 따라서 “빌드 성공”은 맞지만 PLC 적용 상태는 단순히 성공으로 확정하면 안 됩니다. 해당 구간을 더 좁게 감사해 어떤 단계가 성공/실패했는지 분리하겠습니다.
>
> 빌드 후 재검증에서 새 blocker가 하나 나왔습니다. `Classes.lcb`가 빌드로 재생성되면서 `CriticalSection`의 `lsl_st_mt` dependency record 수가 기존 38에서 37로 바뀌었습니다. 소스 로직 오류라는 뜻은 아니고, 빌드 전 생성 메타데이터 pin이 stale해졌다는 뜻입니다. 현재 post-build `.lcb` 구조를 정상 fixture와 비교해 37이 올바른 재생성 결과인지 확인한 뒤 verifier를 재기준화하겠습니다.
>
> 현재 로그 기준으로는 다음까지 확인됐습니다.
>
> - 10:35 C78 `Build project`: 변경된 Diagnostics/Sender/TCP 3개 모두 compile
> - source compiler error `0`, warning `60` (`W0069=28`, `W0070=21`, `W0072=11`)
> - Linker `Done`
> - 첫 Download: 282개 전송, PLC Link 성공, `Download Ok`
> - 두 번째 Download: restart 단계 실패·abort
> - 이후 재연결 성공 및 PLC의 `Project successfully loaded` 확인
> - 해당 LASAL 세션의 `CInvalidArgException=0`
>
> 따라서 최초 Download의 현재 코드 실행 가능성은 높지만, 두 번째 실패까지 지워서 “모든 다운로드 성공”이라고 부르지는 않겠습니다. runtime 시험 시작 시 새 BootId/연결 상태를 먼저 확인하는 이유입니다.
>
> <details><summary>Created a file, edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md` (+167 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md` (+16 -10)
>
> </details>
>
> post-build 재기준화는 진전됐습니다. C78이 `Classes.lcb`를 정리하면서 생긴 두 변화—`lsl_st_mt` 중복 1건 제거와 실제 variable token 2개 생성—를 synthetic fixture와 실제 C78 산출물로 구분해 verifier에 반영했습니다. 현재 실제 post-build tree는 다시 `TerminalWakeBrokerCandidate` PASS입니다. 마지막으로 이 보정 뒤 음성 fixture `276/276` 전수를 재실행 중이며, 끝나기 전에는 pin을 고정하지 않겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md` (+4 -1)
>
> </details>
>
> 사용자용 실행표는 새 runbook으로 정리했습니다. 첫 양성 시험은 `Get Drive Operation Mode`나 `Read SDO Inline`이 아니라, SDO 패널의 저수준 `Submit SDO Read`로 `0x6061:0 / Int8 / 1 byte / timeout 1000`을 1회 보내야 합니다. 다른 두 버튼은 자체 polling이 섞여 `UDP → exact 0x7E03` 인과를 증명하기 어렵습니다. runbook에는 정상·실패·재연결·유실 fallback과 14개 negative packet 조건을 분리해 두었습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 빌드 결과는 그대로 “C78 증분 Build 성공 + 첫 Download 성공, 두 번째 Download 중단”으로 구분해 기록하고 있습니다. 지금은 최종 검증기 재시험, Gate D 해시 고정/검증, 그리고 실제 PLC 시험 절차서 보강을 마무리하겠습니다.
>
> 문서 보강은 완료됐습니다. 현재 결과에는 증분 Build와 첫 Download 성공만 적고, strict Rebuild·물리 sequence-4·실제 UDP 인과 증거는 아직 없다고 명시했습니다. 장시간 276개 검증 케이스의 최종 재실행은 계속 진행 중입니다.
>
> 최종 focused verifier는 post-build 생성물까지 포함해 `276/276`과 `TerminalWakeBrokerCandidate`를 다시 통과했습니다. 이제 검토된 canonical 해시를 Gate D 캡처 도구에 고정하고, 실제 캡처가 아닌 bootstrap 검증까지 수행하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./test/Reports_Lasal/C78_20260807_udp_callback_gate_b/Capture-UdpCallbackGateBCheckpoint.ps1` (+6 -6)
>
> </details>
>
> 캡처 도구 self-test가 실패한 것은 아니고, 60초 실행 제한에 걸려 중단됐습니다. 동일 시험을 더 긴 명령 제한으로 다시 시작하되 30초 단위로 상태를 확인하겠습니다.
>
> 캡처 도구의 양성/음성 fixture가 아직 실행 중입니다. 이 검사는 production 파일을 쓰거나 manifest를 만들지 않는 순수 self-test입니다.
>
> 캡처 도구 self-test는 `43`개 양성, `81`개 음성 fixture 모두 통과했고 manifest는 만들지 않았습니다. 이제 현재 미커밋 도구와 post-build tree를 대상으로 한 bootstrap `ValidateOnly`를 실행합니다.
>
> bootstrap 검증은 도구 해시, Git 상태, 생성물·소스·Network/Include 보호 범위, focused verifier 출력의 상호 일치를 함께 확인하므로 시간이 걸립니다. 아직 오류 출력 없이 진행 중입니다.
>
> 현재까지 production 파일을 쓰거나 PLC에 접근하는 동작은 없습니다. 이 단계가 통과해도 “정식 sequence-4 manifest 캡처 완료”가 아니라, 미커밋 도구 기준 후보 검증 통과로만 기록합니다.
>
> focused 검증 부분은 장시간 음성 fixture가 아니라 실제 트리 전체 해시·계약 검증입니다. 완료되면 이어서 일반 `SourceOnly` 검증도 동일한 Gate D 상태로 한 번 더 돌리겠습니다.
>
> bootstrap 검증도 오류가 아니라 5분 제한에 먼저 걸렸습니다. 이 도구가 내부에서 일반 verifier까지 호출해 약 4분 이상 추가로 소요되는 구조라, 실행 제한을 늘려 다시 수행하고 30초 단위로 확인하겠습니다.
>
> 긴 bootstrap 재실행은 정상적으로 진행 중입니다. 이번에는 내부 276개 self-test와 current-tree 검증이 끝날 수 있도록 충분한 실행 제한을 둔 상태입니다.
>
> 현재 단계는 해시를 고정한 뒤 그 고정값 자체가 실제 verifier 파일과 일치하는지도 다시 확인합니다. 통과 전에는 sequence-4 완료나 production 승인으로 올리지 않습니다.
>
> 동시에 확정된 사용자 측 우선 시험은 저수준 `Submit SDO Read` 경로입니다. `Read SDO Inline`은 자체 polling 경로라서 UDP wake의 인과 시험에는 사용하지 않는 것으로 절차서를 고정했습니다.
>
> 실제 PLC PASS 판정에는 화면만으로 부족합니다. 최종 체크리스트는 fresh BootId, PLC counter delta, PLC-origin 52-byte UDP, 그 뒤 정확한 `0x7E03`, TCP 응답 후 UI terminal 갱신을 한 묶음으로 요구합니다.
>
> bootstrap의 앞단 Git·artifact inventory 검사가 끝나고 focused verifier self-test 구간으로 넘어간 것으로 보입니다. 현재까지 표준 출력에 실패 메시지는 없습니다.
>
> 프로세스 상태를 확인했고, 지금은 내부 focused verifier의 `-RunSelfTest`를 실행 중입니다. 중복 검증 프로세스나 LASAL IDE 프로세스는 없습니다.
>
> 문서와 절차서 수정은 이미 끝났고, 이 검증이 실행되는 동안에는 Git snapshot을 바꾸지 않도록 추가 편집을 멈춘 상태입니다.
>
> runtime 시험표에는 operator가 안전하게 만들 수 없는 조건도 분리했습니다. `EventId=0`, sender-local mismatch, forged duplicate/reorder는 production UI 시험이 아니라 PC fake 또는 별도 승인 PLC harness 항목입니다.
>
> bootstrap 검증은 계약 오류가 아니라 실행 중간에 `LASAL2 PID 4832`가 새로 떠서 fail-close 됐습니다. 사용자 앱은 제가 종료하지 않겠습니다. 우선 해당 프로세스와 시작 시각만 확인하고, LASAL을 닫은 뒤 재실행해야 하는 단계로 분리하겠습니다.
>
> 중요한 동시 변경도 확인했습니다. LASAL이 11:17에 다시 열리면서 11:18에 `Classes.lcb`와 프로젝트 `.lcb`를 재생성했습니다. 세 `.st` 소스는 그대로지만 생성물 해시가 바뀌었으므로, 지금 상태를 앞선 post-build 검증과 같은 것으로 간주하지 않겠습니다. LASAL 종료 후 재검증이 필요합니다.
>
> LASAL이 열린 동안에는 current-tree verifier를 억지로 통과시키지 않겠습니다. 대신 IDE와 무관한 PowerShell 파서, C78 evidence 도구 self-test, 문서/런북 형식 검사를 마무리하겠습니다.
>
> 프로젝트는 닫았어. 내가 해야될 일을 미리 알려줘
>
> 확인했습니다. 먼저 LASAL 종료와 방금 재생성된 `.lcb` 상태를 검증하겠습니다. 그동안 사용자가 준비할 일은 1순위 정상 wake, 2순위 연속 두 ticket, 3순위 UDP loss 후 수동 refresh, 4순위 reconnect입니다. 정확한 조작값과 증거 목록은 검증 후 바로 정리해 드리겠습니다.
>
> LASAL 종료는 확인됐습니다. 다만 방금 열린 세션이 `Classes.lcb`를 다시 pre-build 형태(`lsl_st_mt` 38개)로 만들었고, focused verifier는 post-build 기준 37개라서 현재 검증은 FAIL입니다. 소스 오류가 아니라 생성물 상태 문제입니다. 최신 LASAL 로그에서 실제로 Build/Rebuild/Save가 있었는지 확인하겠습니다.
>
> 로그를 다시 확인하니 사용자가 방금 정확히 `Rebuild project`를 두 번 실행했습니다. 따라서 현재 38개는 “미빌드 stale”이 아니라 strict Rebuild가 실제 생성한 형태입니다. 앞서 증분 Build 결과 37만 보고 verifier를 37로 고정한 판단이 틀렸습니다. verifier를 strict Rebuild 실측값 38로 바로 보정하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1` (+2 -2)
>
> </details>
>
> 현재 strict Rebuild 생성물은 focused verifier를 통과했습니다. 정적 snapshot을 마칠 때까지만 LASAL을 다시 열지 말아 주세요.
>
> 미리 준비할 것은 다음입니다.
>
> 1. Wireshark 필터: `ip.addr == <PLC_IP> && (tcp.port == 4000 || udp.port == 5000)`
> 2. WPF callback mode: `Version2WakeHint`, UDP max `52`
> 3. 실제 축 Slave 번호 1~4 중 하나
> 4. 저수준 SDO 값: `Read / 0x6061 / sub 0 / Int8 / length 1 / timeout 1000`
> 5. 시험 전후 PLC counter 화면 또는 캡처 준비
>
> 정상 시험에서는 반드시 `Submit SDO Read`를 쓰고, `Read SDO Inline`이나 `Refresh Ticket`은 누르지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./test/Reports_Lasal/C78_20260807_udp_callback_gate_b/Capture-UdpCallbackGateBCheckpoint.ps1` (+1 -1)
>
> </details>
>
> strict Rebuild 실측에 맞춰 verifier의 잘못된 `37` 가정을 `38`로 교정했고 current-tree 검증이 PASS했습니다. 새 canonical 해시도 캡처 도구에 다시 고정했습니다. 이제 bootstrap 검증을 재실행합니다. 이 동안 LASAL은 계속 닫아 두면 됩니다.
>
> 정적 검증이 끝난 뒤 사용자가 실제로 할 첫 시험은 이것 하나입니다.
>
> - WPF Connect → `Listening <local-ip>:5000`
> - 저수준 `Submit SDO Read` 1회
> - 기대 순서: TCP `0x7E50` → UDP `LMC2` 52바이트 1개 → 같은 TicketId의 TCP `0x7E03` 1회
> - UI는 UDP 수신만으로 바뀌면 안 되고, `0x7E03` 응답 뒤에만 Completed로 바뀌어야 합니다.
> - 기대 PLC delta: Attempt `+1`, Enqueued `+1`, Rejected `+0`입니다.
>
> 최신 로그 감사에서 strict Rebuild 두 번의 차이를 확인했습니다. 첫 Rebuild는 `Classes.lcb` 저장 오류 2건 때문에 인정하지 않습니다. 두 번째 Rebuild는 C78/ARM, source error 0, Compiler/Linker Done, 명령 성공으로 clean strict Rebuild입니다. 지금 검증하는 생성물은 두 번째 결과입니다.
>
> 실제 시험 전에 한 가지가 더 필요합니다. 방금 clean strict Rebuild 결과는 PLC에 Download되지 않았습니다. Reset/Restart만 했고 Download 명령은 0회였습니다. 정적 snapshot이 끝나면 제가 “이제 열어도 됨”이라고 알리고, 그때 새 세션에서 정확히 한 번 Rebuild → post-build smoke → 한 번 Download를 하도록 순서를 드리겠습니다.
>
