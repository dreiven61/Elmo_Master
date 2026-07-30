> 							80 텍스트 (disabled) Move Relative
> 						81 단추 (disabled) Move Velocity ID: ButtonMoveVelocity
> 							82 텍스트 (disabled) Move Velocity
> 						83 텍스트 Absolute/Relative use Shortest. Relative direction comes from the distance sign. Velocity runs until Stop or PowerOff is verified.
> 					84 스크롤 막대 ID: VerticalScrollBar
> 						85 단추 (disabled) ID: PART_LineUpButton
> 						86 단추 (disabled) ID: PageUp
> 						87 단추 (disabled) ID: PageDown
> 						88 엄지 (disabled)
> 						89 단추 (disabled) ID: PART_LineDownButton
> 			90 탭 항목 (selectable) Group Motion
> 				91 텍스트 Group Motion
> 			92 탭 항목 (selectable) EtherCAT / CREVIS / PI
> 				93 텍스트 EtherCAT / CREVIS / PI
> 			94 탭 항목 (selectable) Bulk Snapshot
> 				95 텍스트 Bulk Snapshot
> 			96 탭 항목 (selectable) Recorder
> 				97 텍스트 Recorder
> 			98 탭 항목 (selectable) SDO / Write Policy
> 				99 텍스트 SDO / Write Policy
> 			100 탭 항목 (selectable) Read-only API
> 				101 텍스트 Read-only API
> 		102 그룹 Execution log / raw callback diagnostics
> 			103 단추 Execution log / raw callback diagnostics ID: HeaderSite
> 				104 텍스트 Execution log / raw callback diagnostics
> 			107 단추 Copy Log
> 				108 텍스트 Copy Log
> 			109 단추 Clear Log
> 				110 텍스트 Clear Log
> 			111 편집 ID: TextExecutionLog
> 				112 스크롤 막대 ID: VerticalScrollBar
> 					113 단추 (disabled) ID: PART_LineUpButton
> 					114 단추 (disabled) ID: PageUp
> 					115 단추 (disabled) ID: PageDown
> 					116 엄지 (disabled)
> 					117 단추 (disabled) ID: PART_LineDownButton
> 				118 스크롤 막대 ID: HorizontalScrollBar
> 					119 단추 ID: PART_LineLeftButton
> 					120 단추 ID: PageLeft
> 					121 단추 ID: PageRight
> 					122 엄지
> 					123 단추 ID: PART_LineRightButton
> 		105 상태 표시줄
> 			106 텍스트 Connect failed ID: TextOperationState
>
> Document text: ```
> [11:00:06.499] Recovered a durable accepted Axis Power Off ACK for _LMCAxis1. Reconnect to the exact identity and run status-only verification; 0x2023 will not be replayed.
> [11:00:06.513] Executable identity: Path=C:\work\elmo\elmo_master\lmc_library\lasalapiwpftestapp\lasalapiwpftestapp\bin\release\lasalmotioncontrolapiexample.exe, Version=0.9.1.0, BuildUtc=2026-07-30 01:47:10 UTC, Feature=CREVIS_TOPOLOGY_AUTOLOAD_EDITABLE_SDO_DRAFT_V2.
> [11:00:06.514] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
> [11:00:31.841] Connect started.
> [11:00:31.871] Connection state Disconnected -> Connecting
> [11:00:31.873] Connection state changed to Connecting invalidated session-bound Axis Power continuations. Exact-identity recovery remains status-only; no 0x2023 replay is automatic.
> [11:00:31.883] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0xFFFFFFFF
> [11:00:31.890] Connection state Connecting -> Connected
> [11:00:31.937] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=INITIAL, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [11:00:31.951] Connect FAILED: Reconnect Axis Power On recovery identity is blocked because BootId or MapRevision does not match the Axis Power recovery record.
> ```
> ````
>
> Image output: image/jpeg
>
> 원인은 재현됐습니다. TCP 연결은 성공했고(`Disconnected → Connecting → Connected`), RPC 초기화와 자동 Topology 조회까지 성공했습니다. 그 직후 테스트 앱이 과거의 “Axis Power Off ACK 수락 후 상태 확인 대기” 복구 기록과 현재 PLC의 `BootId/MapRevision`이 다르다고 판단해 연결을 스스로 닫고 `Connect failed`로 표시했습니다. 이제 그 복구 기록의 위치와 삭제 없이 정상 해제할 수 있는 경로를 소스에서 확인하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var obsCloseApp=globalThis.testState; if(!obsCloseApp?.accessibility) throw new Error('No test observation'); globalThis.testState=null; try { await sky.click({window:obsCloseApp.window, element_index:6}); } catch(error){ throw new Error('App close outcome unknown',{cause:error}); } await new Promise(r=>setTimeout(r,500)); globalThis.windowsAfterClose=await sky.list_windows(); nodeRepl.write(JSON.stringify(windowsAfterClose.filter(w=>/LasalMotionControlApiExample/i.test(w.app)||/LASAL Motion Control API Example/i.test(w.title)),null,2));",
>   "title": "진단용 테스트 앱 종료",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> App close outcome unknown
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.windowsCheckClose=await sky.list_windows(); nodeRepl.write(JSON.stringify(windowsCheckClose.filter(w=>/LasalMotionControlApiExample/i.test(w.app)||/LASAL Motion Control API Example/i.test(w.title)),null,2));",
>   "title": "테스트 앱 종료 여부 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> []
> ```
</details>

결론: **LASAL TCP 서버 문제는 아닙니다. 테스트 프로그램이 정상 접속한 뒤, 안전 복구 기록 불일치 때문에 스스로 연결을 끊고 있습니다.**

확인된 실제 순서:

- TCP 연결 성공
- RPC 초기화 성공
- `Connected` 전환 성공
- EtherCAT Topology 7개 조회 성공
- 그 직후 아래 오류 발생 후 앱이 자체 연결 종료
  `BootId or MapRevision does not match the Axis Power recovery record`

원인은 정확히 `BootId` 불일치입니다.

- 남아 있는 복구 기록: `_LMCAxis1`, `Power Off ACK`, `AcceptedAwaitingProof`
- 기록 BootId: `6`
- 현재 PLC BootId: `8`
- MapRevision은 양쪽 모두 `0x957F101E`로 동일
- 10:49 PLC 다운로드 후 10:51 Reset/Restart가 실행되면서 BootId가 변경됨: [Lasal2.log](C:/Users/dreiv/AppData/Local/Temp/Lasal2.log:39854)
- LASAL은 프로젝트 실행 인스턴스가 바뀔 때 BootId를 증가시킵니다: [LMCDiagnosticsService.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:270)

테스트 프로그램은 활성 복구 기록이 있으면 현재 BootId와 비교하고, 다르면 예외를 발생시킵니다: [MainWindow.AxisPowerOnRecovery.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisPowerOnRecovery.cs:680). 그 예외 때문에 이미 성공한 연결까지 다시 닫습니다: [MainWindow.xaml.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:456).

LASAL 실시간 상태도 정상입니다.

- `Port=4000`
- `_STATE_ACCEPT`
- `ErrorCode=0`
- `ConnectedClients=0`
- TCP 인터페이스 `_STATE_RUNNING`
- 설정도 `TCPIPServer`, `MaxConnections=2`: [Comm_Network.lcn](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn:43)

따라서 판정은 다음과 같습니다.

- LASAL 네트워크/서버: 정상
- 기존 세션 점유: 아님
- 직접 원인: 테스트 프로그램의 오래된 Axis Power 복구 journal
- 프로그램 문제점: 실제 접속 성공 후의 복구 거부를 단순히 `Connect failed`로 표시하며, PLC 재시작 후 처리할 명시적인 복구 경로가 없음

복구 파일은 [axis-power-on-recovery.bin](C:/Users/dreiv/AppData/Local/Elmo/LasalMotionControlApiExample/AxisPowerOnRecoveryJournal/v1/axis-power-on-recovery.bin)입니다. 다만 바로 삭제하면 이전 Power Off의 최종 상태가 검증되지 않았다는 사실까지 지워집니다. 먼저 `_LMCAxis1`의 `PowerOn=false`와 `Standstill`을 읽기 전용 경로로 확인한 다음 기록을 해제해야 합니다.

파일은 수정하지 않았고, 진단용으로 실행한 테스트 앱도 종료했습니다. 과거 Test2 성공 기록은 참고만 했으며, 이번 결론은 오늘 실제 PLC와 Release 앱으로 다시 재현해 확정했습니다.
