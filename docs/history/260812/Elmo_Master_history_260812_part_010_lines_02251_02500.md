<details><summary>35 previous messages</summary>

> 사용자가 GD-01을 진행하는 동안 저는 두 가지를 병행합니다. 최신 GUI와 배포본의 기능 경계를 문서화하고, 방금 Rebuild로 달라진 `Classes.lcb`가 Gate D 클래스 ABI에는 영향을 주지 않았다는 증거를 현재 다운로드 세션 기준으로 정리하겠습니다. 생산 `.st` 소스와 PLC는 건드리지 않습니다.
>
> 방금 Rebuild drift 감사 결과를 먼저 공유합니다.
>
> - `Classes.lcb`는 99바이트가 달라졌지만 36개 비관련 vendor class 영역입니다.
> - `LMCDiagnosticsService`, `LMCUdpCallbackSender`, `TCPMotionInterface`, `_UDPTransceiver`의 생성 record는 커밋본과 byte-exact입니다.
> - 네 클래스의 `.st` 소스도 모두 커밋과 동일합니다.
>
> 따라서 현재 GD-01 read-only 시험은 진행해도 됩니다. 다만 이번 다운로드 이미지는 기존 sequence-4 manifest가 고정한 `Classes.lcb` 전체 바이트와 동일하다고 주장하지 않고, 별도 runtime/download evidence로 다시 봉인하겠습니다.
>
> 이거 GUI가 에러났을때 다시 연결하는거 제대로 연결 안되는 버그가 있는거 같다. 내용 확인좀해봐.
>
> 재연결 문제를 우선 확인하겠습니다. 현재 실행 EXE와 UI 로그 가능 여부를 확인하고, `Connect 실패 → Close/cleanup → 재연결` 경로에서 connection 객체·callback listener·busy 상태·retained ticket이 제대로 초기화되는지 소스를 추적하겠습니다. 우선 진단만 하고 앱을 종료하거나 코드를 수정하지는 않습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 재연결 경로를 소스와 현재 실행 로그 기준으로 확인하겠습니다. 우선 “의도적으로 재연결을 막는 복구 게이트”와 “오류 뒤 연결 객체가 남는 실제 버그”를 분리해서 보겠습니다. 앱이나 LASAL은 건드리지 않고 읽기 전용으로 조사합니다.
>
> 에러 위치임
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-077fc181-7e89-429b-8b2a-eab95ad057e5.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-aef19927-4690-46c5-acdc-d2193b553faa.png`
>
> 스크린샷으로 범위가 많이 좁혀졌습니다. 실패 지점은 TCP 연결 자체가 아니라 PLC가 `0x8080 RPC session init`에 `Status=1, ErrorId=0`을 반환한 곳입니다. GUI는 이미 `Disconnected / Stopped / Connect failed`로 정리됐으므로, 우선 “클라이언트 객체가 남아서 재연결 실패”보다는 PLC 세션 초기화가 계속 거부되는 이유를 추적하겠습니다. 디버거가 예외에서 멈춘 상태도 재연결 동작에 영향을 주는지 함께 확인합니다.
>
> 하는김에 코드도 수정해 일단 테스트는 중지했으니 니가 수정하는거부터 진행하자. 이거 PLC 에러이면 WTR에 적용된 다중 접속 에러도 수정해야되는데.... 일단 원인이 뭔지는 찾아보자..
>
> 확인했습니다. 테스트는 중지된 것으로 보고, 원인을 확정한 뒤 이 저장소의 재연결 경로까지 수정하겠습니다. 현재 가장 유력한 두 갈래는 (1) PLC가 기존 소유 세션을 정리하지 못해 새 `0x8080`을 거부하는 경우, (2) WPF/SDK가 초기화 실패 후 소켓 종료를 PLC에 충분히 전달하지 못하는 경우입니다. 둘 다 소스와 로그로 가르겠습니다. WTR은 같은 결함이 확인될 때 별도 적용 대상으로 명확히 분리하겠습니다.
>
> 현재 소스에서 한 가지는 확정됐습니다. PLC의 `0x8080` 실패 응답이 실제 원인 코드를 헤더에 넣지 않아, GUI에는 모든 PLC 초기화 거부가 똑같이 `Status=1, ErrorId=0`으로 보입니다. 재연결 차단 원인은 그 안쪽에서 callback disarm이 실패했거나 세션 소유 상태가 어긋난 경우일 가능성이 높습니다. 이 두 상태를 구분할 수 있도록 응답 계약과 재연결 상태 전이를 같이 점검 중입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 확인된 건 두 가지입니다. PC 쪽은 PLC의 4바이트 실패 응답을 잘못 파싱해 `ErrorId=-1`을 `0`으로 숨기는 확정 버그가 있고, PLC 쪽은 기존 콜백 해제 실패가 새 세션 연결을 반복 차단할 수 있습니다. 우선 안전 계약은 유지한 채 응답 파싱과 제한적 재시도부터 구현하고, PLC/WTR 공통 원인은 별도로 대조하겠습니다.
>
> 현재 화면에서 보인 `ErrorId=0`은 재연결 실패 원인이 아니라 진단 손실입니다. 실제 PLC는 동일 프레임에서 `-1`을 보냈는데 SDK가 짧은 ACK를 일반 응답으로 읽고 있습니다. 이 부분은 바로 고칠 수 있고, 재시도는 영구적인 세션 불일치를 덮지 않도록 같은 소켓에서 짧고 제한적으로만 허용하는 방향으로 검토 중입니다.
>
> 재연결 실패의 직접 회귀 지점도 찾았습니다. 최근 콜백 기능에서 `0x8080` 세션 초기화가 “이전 UDP 콜백 해제 성공”까지 요구하도록 바뀌었고, 해제가 `-8/-9`로 실패하면 새 TCP 세션도 영구 차단됩니다. WTR의 다중접속 코드는 이 콜백/RPC 계층이 없어 같은 원인은 아닙니다. 지금은 PLC에서 stale 콜백을 안전하게 세대 차단하면서 새 세션은 회복시키는 최소 수정 가능성을 확인하고 있습니다.
>
> 수정 범위는 PC SDK로 확정했습니다. PLC의 fail-closed 안전 규칙은 건드리지 않습니다. 대신 WPF가 실제 사용하는 v2 연결에서만, 정확한 `Status=1 / ErrorId=-1` 짧은 응답에 대해 같은 TCP 소켓으로 20ms 뒤 딱 한 번 재시도하고, 그 외 오류는 즉시 실패하도록 구현합니다. 영구적인 `-8/-9` 상태는 숨기지 않습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+50 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/CallbackV2ConnectionTests.cs` (+109 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+30 -0)
>
> </details>
>
> C# 수정은 적용됐습니다. 추가한 회귀 시험은 세 가지입니다: 실제 `-1` 보존, 같은 소켓 실패→성공, 영구 실패는 재시도 1회에서 중단. 이제 전체 PC 테스트와 WPF 빌드를 돌려 기존 1,111개 회귀에 영향이 없는지 확인하겠습니다.
>
> PC 전체 회귀는 `1115/1115 PASS`입니다. 재시도 성공은 같은 TCP 연결에서 `0x8080, 0x8080, 0x405C` 순서로 확인됐고, 두 번째도 실패하면 `Faulted`로 정리됩니다. 이제 GUI Release와 WPF smoke 전체를 확인하고 문서에 “PC 완화책”과 “PLC 영구 원인 미확정”을 분리해 기록하겠습니다.
>
> 라이브러리와 1,115개 테스트는 정상입니다. `dotnet msbuild`는 이 구형 .NET Framework WPF 프로젝트의 XAML 생성 타깃을 로드하지 못해 GUI 빌드만 실패했습니다(`InitializeComponent` 미생성). 코드 오류가 아니라 빌드 도구 선택 문제라서, 설치된 Visual Studio MSBuild로 다시 검증합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/CallbackV2ConnectionTests.cs` (+31 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 원인 범위는 잡혔습니다. GUI 정리 로직이 아니라 PLC의 `0x8080` 초기화 거부이고, PC 쪽에서 실제 `ErrorId=-1`을 `0`으로 숨기던 파서 결함도 확인했습니다. 지금은 “같은 소켓에서 정확히 한 번만 재시도”하는 최소 수정과 회귀 테스트를 마무리하고 있습니다. WTR은 동일 원인이 아닌 것으로 확인돼 건드리지 않습니다.
>
> 현재 수정은 PLC 안전 규칙을 우회하지 않습니다. `Version2WakeHint`에서 PLC가 보내는 정확한 단기 실패 프레임에만, 같은 TCP 소켓으로 `0x8080`을 한 번 더 보냅니다. 계속 실패하면 즉시 `Faulted`로 정리되고, 이제 화면에는 숨겨졌던 `ErrorId=-1`이 표시됩니다. 전체 SDK 테스트는 `1116/1116`, WPF 테스트는 `332/332`까지 통과했습니다.
>
> 최종 Release 재검증 중 병렬로 두 WPF 프로젝트를 빌드해 같은 `obj\Release` 파일 잠금이 한 번 발생했습니다. 코드 오류가 아니라 빌드 출력 충돌이며, 순차 빌드로 다시 확인하겠습니다.
>
> Release SDK 빌드와 전체 테스트는 현재 코드 기준 `1116/1116`으로 다시 통과했습니다. WPF Release 빌드도 통과했고, 마지막 `332`개 UI smoke를 재실행 중입니다. PLC나 LASAL 프로젝트에는 손대지 않았습니다.
>
> 오후시간이니 승인없이 계속 진행해
>
> 알겠습니다. 중간 승인 요청 없이 현재 범위 안에서 수정·검증·문서 정합성까지 계속 진행하겠습니다. PLC/LASAL의 안전 의미를 바꾸는 확대 수정은 하지 않고, 확인된 PC 재연결 경로부터 완결하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+3 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/CallbackV2ConnectionTests.cs` (+6 -0)
>
> </details>
>
> 확정된 것과 미확정된 것을 분리하면 이렇습니다. PC의 `ErrorId=0` 표시는 확실한 파서 버그였고 수정했습니다. PLC가 거부한 세부 원인은 아직 `RpcCallbackLastDisarmResult`를 보지 못해 `-8`(fence 불일치)인지 `-9`(CallbackSender 미연결)인지 확정할 수 없습니다. 그래서 PLC 메타데이터를 강제로 지우는 위험한 수정은 하지 않았고, 현재는 GUI에서 첫 실패 후 새 연결을 다시 시도할 수 있는지 WPF 레벨 회귀 테스트까지 추가 확인 중입니다.
>
> GUI 레벨 회귀 테스트도 범위를 좁혔습니다. 첫 클릭에서 `0x8080` 두 번 모두 실패한 뒤 UI가 `Disconnected/Stopped`, Connect 재활성, 내부 연결 참조 제거 상태로 돌아오는지 확인하고, 두 번째 클릭은 새 TCP 세션으로 `0x8080 → 0x405C`까지 성공하는지만 검증합니다. 운영 코드에 추가 결함은 아직 발견되지 않았습니다.
>
> 새 WPF 회귀 테스트의 단독 실행은 통과했습니다. 지금 전체 Release smoke를 다시 돌리고 있으며, 이 테스트는 첫 실패 세션에서 `0x8080`이 정확히 2회 후 종료되고 `0x405C/0x405D`가 전혀 나가지 않는지, 두 번째 클릭이 새 TCP 세션에서 정상 등록되는지까지 확인합니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+5 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -2)
>
> </details>
>
> 최종 상태는 SDK Debug/Release `1116/1116`, WPF Release `333/333`입니다. 두 번째 Connect가 새 TCP 세션을 실제로 만드는 WPF 회귀까지 통과했고, 현재 관련 변경은 스테이징하지 않은 상태로 남겨뒀습니다. 마지막 독립 diff 리뷰만 확인하고 정리해서 드리겠습니다.
</details>

수정 완료했습니다. 스크린샷의 `throw;`는 원인이 아니라 정리 후 예외를 다시 던지는 위치였습니다.

- [LmcConnection.cs](./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs:1449)
  - 실제 PLC 응답의 `ErrorId=-1`을 `0`으로 표시하던 파서 오류 수정
  - `Version2WakeHint`의 정확한 실패 프레임만 20ms 후 같은 소켓에서 1회 재시도
  - 계속 실패하거나 다른 오류·비정상 프레임이면 즉시 `Faulted` 정리
- [WPF 재연결 테스트](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs:150)
  - 첫 Connect 실패 후 `Disconnected/Stopped`, Connect 재활성, `connection=null`
  - 두 번째 Connect가 새 TCP 세션으로 `0x8080 → 0x405C` 성공하는 것까지 검증

검증 결과:

- SDK Debug/Release: `1116/1116 PASS`
- WPF Release: `333/333 PASS`
- `git diff --check`, cached check: PASS
- 스테이징/커밋하지 않음
- PLC와 WTR 소스는 수정하지 않음

PLC 쪽 실제 거부 원인은 아직 확정되지 않았습니다. [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1664)는 callback disarm 실패를 모두 wire `ErrorId=-1`로 내보냅니다. 유력 후보는 `-8` fence 불일치 또는 `-9` CallbackSender 미연결입니다.

WTR은 같은 원인이 아닙니다. [WTR TCP_IP_Interface.st](<C:/Users/dreiv/source/repos/SIGMATEK/project_repos/WTR/JC_Solution/01.WaferTransferRobot/Stations/MachineControl/WTR_PLC/Class/TCP_IP_Interface/TCP_IP_Interface.st:799>)에는 `0x8080`, RPC callback, disarm 상태가 없으므로 이번 수정 대상에서 제외했습니다.

이제 새 GUI로 재연결만 먼저 시험하면 됩니다. 계속 실패하면 LASAL Watch에서 `RpcCallbackLastDisarmResult`, `RpcCallbackRegistered`, `RpcCallbackSessionEpoch`, `SessionEpoch`, `RpcSocket`, `CurrentSock`을 확인하면 PLC 분기를 확정할 수 있습니다.

<details><summary>11 previous messages</summary>

> 설계문서와 현재 작업 트리를 다시 대조해 다음 구현 단계를 바로 잡겠습니다. 우선 Gate D 콜백 설계, 방금 확인된 재연결 수명주기 문제, 현재 미커밋 변경을 분리해서 감사한 뒤 다음 생산 코드와 검증기를 함께 진행하겠습니다.
>
> 다음 단계 후보를 설계문서 기준으로 좁히고 있습니다. 현재 확인된 중요한 점은 PLC의 negative disarm 보존이 단순 버그가 아니라 설계된 fail-closed 계약이라는 것입니다. 따라서 이를 강제로 초기화하는 수정은 설계 위반입니다. 대신 실제 런타임 시험에 부족했던 WPF의 callback 등록 정보와 accepted/rejected/duplicate/out-of-order 관측성을 다음 구현 대상으로 검토하고 있습니다.
>
> 현재 설계상 PLC의 `-8/-9` 보존은 의도된 안전 계약이므로 PLC 강제 초기화 수정은 하지 않습니다. 대신 검증이 끝난 PC 재연결 수정 4개 파일만 정확히 분리해 커밋하고, Gate D 문서는 실제 sequence-4 커밋·후속 Rebuild/Download·Classes drift 상태로 바로잡겠습니다.
>
> PC 재연결 수정은 `66b5cf2 fix(api): recover bounded RPC init reconnect`로 분리 커밋했습니다. Gate D 생산 7파일과 `Classes.lcb` drift는 포함하지 않았습니다. 현재는 후속 Rebuild로 달라진 `Classes.lcb` 99바이트가 의미상 허용 가능한 생성물 차이인지 독립 분석하고, sequence-4/Download 문서의 오래된 상태를 실제 현재 상태로 교정 중입니다.
>
> `Classes.lcb` drift는 같은 파일 크기에서 정확히 99바이트/58구간이며 33개 vendor class record에만 있습니다. Gate D 핵심 4개 class record와 `_StdLib`, `CriticalSection`은 바이트 동일하지만, 바뀐 vendor 필드의 의미를 아직 증명할 수 없어 “동등한 생성물”로 완화하지 않습니다. 현재는 해당 파일을 복구·커밋하지 않고 엄격한 증거 경계를 유지합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+2 -2)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> PC 재연결 수정은 이미 `66b5cf2`로 분리 커밋됐습니다. 이제 불명확한 `Classes.lcb` 재생성 차이를 문서에 정확히 남기고, 전체 diff/스테이징 상태를 다시 검증한 뒤 다음 PLC 시험 경계를 정리하겠습니다. PLC의 `-8/-9` fail-closed 동작은 설계 계약이므로 임의로 완화하지 않습니다.
>
> 재생성 차이를 다시 대조했습니다. 핵심 Gate D 클래스 레코드는 그대로지만, 나머지 36개 vendor 레코드의 99바이트가 불명확하게 달라졌습니다. 따라서 “그냥 새 해시로 rebaseline”하면 안 됩니다. 이 경계를 설계 문서·런북·패킷 맵에 명시하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md` (+9 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+7 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md` (+14 -7)
>
> </details>
>
> 코드·문서·스테이징 상태는 깨끗하게 검증됐습니다. 마지막으로 현재 실행 프로세스가 없는지와 실제 변경 지점을 고정해서, 바로 이어서 할 PLC 시험만 남기겠습니다.
</details>

PC 재연결 수정은 완료했고 `66b5cf2`로 커밋했습니다.

- [LmcConnection.cs](./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs:1449)
  - 숨겨지던 실제 short ACK `ErrorId=-1` 보존
  - V2의 정확한 일시 오류만 동일 소켓에서 20ms 후 1회 재시도
  - 지속 오류·다른 오류·Legacy는 재시도 없이 정상 정리
- SDK Debug/Release 각각 `1116/1116 PASS`
- WPF Release `333/333 PASS`
- PLC와 WTR 코드는 수정하지 않았습니다. `-8/-9` disarm 보존은 의도된 fail-closed 계약입니다.

다음 실기 시험은 아래 순서입니다.

1. LASAL Watch에 `RpcCallbackLastDisarmResult`, `RpcCallbackRegistered`, `RpcCallbackSessionEpoch`, `RpcCallbackBootId`, Cookie, `SessionEpoch`, `RpcSocket`, `CurrentSock`, sender endpoint/queue를 표시합니다.
2. Wireshark를 시작하고 GUI에서 `Connect`를 한 번 누릅니다.
3. 성공하면 `0x8080 → 0x8080 → 0x405C`인지 확인합니다. 이는 transient 오류 자동 복구입니다.
4. 실패하면 이제 GUI 오류가 `ErrorId=-1`로 표시되어야 합니다.
   - `LastDisarmResult=-8`: TCP/sender fence 불일치
   - `-9`: CallbackSender 런타임 연결 부재
5. 연결되면 안전한 SDO Read `0x6061:0`, `Int8`, 길이 1, timeout 1000으로 Gate D UDP→TCP 시험을 진행합니다.

주의: 현재 `Classes.lcb`는 체크포인트와 99바이트가 다릅니다. Gate D 핵심 레코드는 동일하지만 나머지 36개 vendor 레코드의 의미가 불명확하므로, 지금 PLC 시험은 탐색용입니다. 재다운로드는 필요 없습니다. 현재 LASAL과 테스트 GUI 프로세스는 모두 종료 상태입니다.

<details><summary>21 previous messages</summary>

> 실기 시험만 기다리지 않고, 설계문서가 요구하는 런타임 관측성 중 PC에서 지금 구현 가능한 부분을 계속 진행하겠습니다. 먼저 현재 문서 요구사항과 WPF/SDK 노출 상태를 병렬로 대조한 뒤, PLC ABI를 건드리지 않는 범위에서 GUI 진단 표시와 회귀 테스트를 구현하겠습니다.
>
> 구현 범위를 확정했습니다. 새 패널은 PC가 실제로 아는 값만 표시합니다: V2 등록 응답의 Version/MaxDatagram/BootId/SessionEpoch/Flags와 수신기 Accepted/Rejected/Duplicate/OutOfOrder/마지막 결정입니다. PLC 내부 `LastDisarmResult`, sender queue 같은 값은 거짓으로 추정하지 않고 Watch 전용으로 남깁니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcCallbackModels.cs` (+31 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+58 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+36 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+82 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/CallbackV2ConnectionTests.cs` (+21 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+62 -0)
>
> </details>
>
> 병렬 리뷰에서 중요한 세션 경계 문제가 확인됐습니다. 같은 `LMCConnection` 객체가 재연결될 때 이전 UDP 결정 이벤트가 UI 큐에 늦게 도착할 수 있습니다. 이벤트에 owner/lifetime/session provenance를 넣고, 카운터 snapshot을 결정 커밋과 같은 lock 안에서 생성하도록 바로 보강하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcCallbackModels.cs` (+30 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+59 -63)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+40 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+10 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+53 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+133 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnectionModels.cs` (+52 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 재연결 수정과 별개로, 지금은 PC 쪽 진단 증거를 완성하겠습니다. RPC 초기화 결과를 연결 종료 뒤에도 보존하고, 콜백 등록·세션·카운터가 오래된 연결 이벤트로 덮이지 않게 한 뒤 SDK/WPF 전체 회귀를 돌리겠습니다. LASAL 생성물과 PLC 소스는 건드리지 않습니다.
>
> 원인 진단값을 예외 문자열에만 남기지 않도록 정리 중입니다. 핵심은 `0x8080` 시도 횟수·첫 실패 ACK·최종 ACK·성공/실패/취소를 하나의 불변 스냅샷으로 만들고, 연결 정리 후에도 그 스냅샷만 보존하는 것입니다.
>
> <details><summary>Edited 5 files</summary>
