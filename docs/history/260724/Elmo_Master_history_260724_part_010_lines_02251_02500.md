> }
> ```
>
> ```text
> 					260 콘솔 트리 (selectable) Sigmatek
> 					261 콘솔 트리 (selectable) _TCPIPServer_RT
> 					262 콘솔 트리 (selectable) Elmo_1
> 					263 콘솔 트리 (selectable) Elmo_2
> 					264 콘솔 트리 (selectable) Elmo_3
> 					265 콘솔 트리 (selectable) Elmo_4
> 					266 콘솔 트리 (selectable) LMCDiagnosticsService
> 					267 콘솔 트리 (selectable) LMCEcatInputLatch
> 						268 콘솔 트리 (selectable) Servers
> 						269 콘솔 트리 (selectable) Clients
> 						270 콘솔 트리 (selectable) Methods
> 							271 콘솔 트리 (selectable) Global
> 							272 콘솔 트리 (selectable) Private
> 						273 콘솔 트리 (selectable) Variables
> 						274 콘솔 트리 (selectable) Objects
> 							275 콘솔 트리 (selectable) LMCEcatInputLatch1
> 						276 콘솔 트리 (selectable) Dependencies
> 					277 콘솔 트리 (selectable) LMCRecorderStore
> 					278 콘솔 트리 (selectable) TCPMotionInterface
> 			279 탭 항목 (selectable) Lib
> 			280 탭 항목 (selectable) File
> 			281 탭 항목 (selectable) Global
> 			282 탭 항목 (selectable) Net
> 			283 탭 항목 (selectable) Class
> 			284 단추 Close
> 		285 Tab Properties ID: 298070392
> 			286 창 ID: 289142312
> 				287 TABLE Properties Window ID: 293586600
> 				288 도구 모음 ID: 59392
> 					289 단추
> 					290 단추
> ```
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nawait sky.click({window:lasalWindow, x:955, y:428, screenshotId:\"screenshot-1\"});\nnodeRepl.write(\"expanded Global methods\")\n",
>   "title": "LASAL 글로벌 메서드 확장"
> }
> ```
>
> ```text
> expanded Global methods
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nlasalState = await sky.get_window_state({window:lasalWindow, include_screenshot:false, include_text:true});\nnodeRepl.write((lasalState.accessibility?.tree || \"\").split(\"\\n\").filter(line => /LMCEcatInputLatch|Methods|Global|RtWork/.test(line)).slice(-30).join(\"\\n\"))\n",
>   "title": "LASAL RtWork 항목 확인"
> }
> ```
>
> ```text
> 			31 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65281
> 					33 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 					102 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 					267 콘솔 트리 (selectable) LMCEcatInputLatch
> 						270 콘솔 트리 (selectable) Methods
> 							271 콘솔 트리 (selectable) Global
> 								272 콘솔 트리 (selectable) RtWork
> 							277 콘솔 트리 (selectable) LMCEcatInputLatch1
> 			283 탭 항목 (selectable) Global
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nawait sky.click({window:lasalWindow, element_index:272, mouse_button:\"right\"});\nnodeRepl.write(\"opened RtWork context menu\")\n",
>   "title": "LASAL RtWork 구현 검색 메뉴"
> }
> ```
>
> ```text
> opened RtWork context menu
> ```
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nlasalState = await sky.get_window_state({window:lasalWindow, include_screenshot:true, include_text:true});\nnodeRepl.write((lasalState.accessibility?.tree || \"\").split(\"\\n\").filter(line => /implementation|Find|Reference|Edit/i.test(line)).slice(-40).join(\"\\n\"));\nfor (var shot18 of lasalState.screenshots) await nodeRepl.emitImage(shot18.url);\n",
>   "title": "LASAL Find in Implementation 항목 확인"
> }
> ```
>
> ```text
> [OMITTED FROM SPLIT COPY: embedded computer-use image/tool-state payload at source line 2,357; original character count 1,048,559; UTF-8 byte count 1,048,605; UTF-8 SHA-256 31d13f8a3f5c0f6dd8540f560ae178fc940234872ce44f5b12645b6043a5f0a0; retained unchanged in docs/history/Elmo_Master_history_260724.md.]
> ```
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.press_key({window:lasalWindow,key:\"Escape\"}); nodeRepl.write(\"closed method menu\")",
>   "title": "LASAL 구현 검색 준비"
> }
> ```
>
> ```text
> closed method menu
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nawait sky.click({window:lasalWindow, x:943, y:388, screenshotId:\"screenshot-1\"});\nnodeRepl.write(\"expanded Clients\")\n",
>   "title": "LASAL 클라이언트 채널 확장"
> }
> ```
>
> ```text
> expanded Clients
> ```
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nlasalState = await sky.get_window_state({window:lasalWindow, include_screenshot:true, include_text:true});\nnodeRepl.write((lasalState.accessibility?.tree || \"\").split(\"\\n\").filter(line => /LMCEcatInputLatch|Clients|EcatMaster|Drive[1-4]|RecorderStore/.test(line)).slice(-40).join(\"\\n\"));\nfor (var shot19 of lasalState.screenshots) await nodeRepl.emitImage(shot19.url);\n",
>   "title": "LASAL 채널 목록 확인"
> }
> ```
>
> ```text
> [OMITTED FROM SPLIT COPY: embedded computer-use image/tool-state payload at source line 2,406; original character count 1,048,511; UTF-8 byte count 1,048,605; UTF-8 SHA-256 55f086a824a913cd9b1f3df2e5b3db16988e01cc4674e3a1df3a7e4cc185073a; retained unchanged in docs/history/Elmo_Master_history_260724.md.]
> ```
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nawait sky.click({window:lasalWindow, element_index:270, mouse_button:\"right\"});\nnodeRepl.write(\"opened EcatMaster context menu\")\n",
>   "title": "LASAL 채널 구현 검색 메뉴"
> }
> ```
>
> ```text
> opened EcatMaster context menu
> ```
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nlasalState = await sky.get_window_state({window:lasalWindow, include_screenshot:true, include_text:true});\nnodeRepl.write((lasalState.accessibility?.tree || \"\").split(\"\\n\").filter(line => /implementation|Find|Reference|Edit|usage/i.test(line)).slice(-50).join(\"\\n\"));\nfor (var shot20 of lasalState.screenshots) await nodeRepl.emitImage(shot20.url);\n",
>   "title": "LASAL Find in Implementation 메뉴 확인"
> }
> ```
>
> ```text
> [OMITTED FROM SPLIT COPY: embedded computer-use image/tool-state payload at source line 2,440; original character count 1,048,568; UTF-8 byte count 1,048,606; UTF-8 SHA-256 e5402cf20e086fefbdbe109b8b8406e0e30dadb7e9fdb205d8cc4d739ce05d9d; retained unchanged in docs/history/Elmo_Master_history_260724.md.]
> ```
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 우선순위가 높은 결함 3건(Health 바인딩, Recorder 종료 경합/설명, EtherCAT latch 중복 스케줄)은 이미 수정했습니다. 지금은 LASAL 정적 계약과 IDE 빌드/링크 결과를 기준으로 남은 목록을 확정하고, D5 SDO Read를 바로 넣을 수 있는 최소 단위까지 문서·코드 경계를 맞추는 중입니다. PLC 다운로드/실기 검증은 이번 작업에서 임의로 수행하지 않겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.press_key({window:lasalWindow,key:\"Escape\"}); nodeRepl.write(\"closed\");",
>   "title": "LASAL 메뉴 닫기"
> }
> ```
>
> ```text
> closed
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "lasalState = await sky.get_window_state({window:lasalWindow,include_screenshot:true,include_text:true}); nodeRepl.write(lasalState);",
>   "title": "LASAL 상태 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 264174,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,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4l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/AH/H+FfWVFAHyb/wz74l/wCeVj/3/H+FH/DPviX/AJ5WP/f8f4V9ZUUAfJv/AAz74l/55WP/AH/H+FH/AAz74l/55WP/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'... 268191 more characters,
>       originX: 56,
>       originY: 106,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,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/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V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'... 266327 more characters,
>       originX: 61,
>       originY: 111,
