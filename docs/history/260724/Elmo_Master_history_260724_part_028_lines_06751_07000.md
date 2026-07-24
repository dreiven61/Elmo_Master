>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.scroll({window:observation.window,screenshotId:sid,x:1780,y:380,scrollX:0,scrollY:-1100});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write((state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/\\.st|EtherCAT|SDO/.test(x)).slice(-100).join(\"\\n\"));}",
>   "title": "EtherCAT 소스 구간으로 되돌리기",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.scroll({window:observation.window,screenshotId:sid,x:1780,y:370,scrollX:0,scrollY:-420});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write((state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/\\.st|EtherCAT|SDO/.test(x)).slice(-100).join(\"\\n\"));}",
>   "title": "EtherCAT 소스 구간 찾기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 						288 콘솔 트리 (selectable) _LMCAxisVis.st
> 						289 콘솔 트리 (selectable) _LMCAxisVisInt.st
> 						290 콘솔 트리 (selectable) _LMCAxisVisLogHandle.st
> 						291 콘솔 트리 (selectable) _LMCAxisVisLogViewer.st
> 						292 콘솔 트리 (selectable) _LMCAxisVisPara.st
> 						293 콘솔 트리 (selectable) _LMCAxisVOVMonitoring.st
> 						294 콘솔 트리 (selectable) _LMCBaseCoord.st
> 						295 콘솔 트리 (selectable) _LMCBeltAxis.st
> 						296 콘솔 트리 (selectable) _LMCCalcModelBase.st
> 						297 콘솔 트리 (selectable) _LMCCalcModelController.st
> 						298 콘솔 트리 (selectable) _LMCMath_SO3.st
> 						299 콘솔 트리 (selectable) _LMCMathFunctions.st
> 						300 콘솔 트리 (selectable) _LMCModuleBase.st
> 						301 콘솔 트리 (selectable) _LMCProfile.st
> 						302 콘솔 트리 (selectable) _LMCProfileBase.st
> 						303 콘솔 트리 (selectable) _LMCProfileBuffer.st
> 						304 콘솔 트리 (selectable) _LMCProfileLog.st
> 						305 콘솔 트리 (selectable) _LMCProfileVis.st
> 						306 콘솔 트리 (selectable) _LMCProfileVisAxis.st
> 						307 콘솔 트리 (selectable) _LMCProfileVisInt.st
> 						308 콘솔 트리 (selectable) _LMCProfileVisMovePara.st
> 						309 콘솔 트리 (selectable) _LMCPublisher.st
> 						310 콘솔 트리 (selectable) _LMCRefBase.st
> 						311 콘솔 트리 (selectable) _LMCRobotBase.st
> 						312 콘솔 트리 (selectable) _LMCRobotLog.st
> 						313 콘솔 트리 (selectable) _LMCSafety.st
> 						314 콘솔 트리 (selectable) _LMCSplineBuffer.st
> 						315 콘솔 트리 (selectable) _LMCTableBuffer.st
> 						316 콘솔 트리 (selectable) _LMCTool.st
> 						317 콘솔 트리 (selectable) _MultiTask.st
> 						318 콘솔 트리 (selectable) _OSKernel.st
> 						319 콘솔 트리 (selectable) _SerLib.st
> 						320 콘솔 트리 (selectable) _StdLib.st
> 						321 콘솔 트리 (selectable) _SyncMeasure.st
> 						322 콘솔 트리 (selectable) _SyncMeasureECAT.st
> 						323 콘솔 트리 (selectable) _TaskObjectControl.st
> 						324 콘솔 트리 (selectable) _TCPIPServer.st
> 						325 콘솔 트리 (selectable) _TCPIPServer_RT.st
> 						326 콘솔 트리 (selectable) _TCPIPServerInterface.st
> 						327 콘솔 트리 (selectable) ASCII_BIN.st
> 						328 콘솔 트리 (selectable) BusInterfaceSDIASInternal.st
> 						341 콘솔 트리 (selectable) Controller.st
> 						342 콘솔 트리 (selectable) CP313_IOs.st
> 						343 콘솔 트리 (selectable) CP733_IOs.st
> 						347 콘솔 트리 (selectable) CriticalSection.st
> 						357 콘솔 트리 (selectable) DiasMaster.st
> 						358 콘솔 트리 (selectable) DiasMasterC.st
> 						359 콘솔 트리 (selectable) DiasMasterMain.st
> 						360 콘솔 트리 (selectable) DiasMasterPostScan.st
> 						361 콘솔 트리 (selectable) ECAT_DS402Base.st
> 						362 콘솔 트리 (selectable) ECAT_Master_Base.st
> 						363 콘솔 트리 (selectable) ECAT_Slave_Base.st
> 						365 콘솔 트리 (selectable) EEPROM.st
> 						366 콘솔 트리 (selectable) Elmo_1.st
> 						367 콘솔 트리 (selectable) Elmo_2.st
> 						368 콘솔 트리 (selectable) Elmo_3.st
> 						369 콘솔 트리 (selectable) Elmo_4.st
> 						370 콘솔 트리 (selectable) EtherCAT_Hub_Base.st
> 						371 콘솔 트리 (selectable) EtherCAT_PLC.st
> 						372 콘솔 트리 (selectable) EtherCAT_SDOBase.st
> 						373 콘솔 트리 (selectable) GetCILInterface.st
> 						374 콘솔 트리 (selectable) Hub_Base.st
> 						375 콘솔 트리 (selectable) Hub_Base_Root.st
> 						376 콘솔 트리 (selectable) HwBase.st
> 						377 콘솔 트리 (selectable) HwBaseCDIAS.st
> 						378 콘솔 트리 (selectable) HwcLogging.st
> 						379 콘솔 트리 (selectable) HwControl.st
> 						380 콘솔 트리 (selectable) HwControlBase.st
> 						381 콘솔 트리 (selectable) HWRtPostScan.st
> 						382 콘솔 트리 (selectable) I2CCDIAS.st
> 						383 콘솔 트리 (selectable) LMCDiagnosticsService.st
> 						384 콘솔 트리 (selectable) LMCEcatInputLatch.st
> 						385 콘솔 트리 (selectable) LMCRecorderStore.st
> 						386 콘솔 트리 (selectable) Merker.st
> 						387 콘솔 트리 (selectable) MerkerEx.st
> 						388 콘솔 트리 (selectable) MoveSplineTable.st
> 						389 콘솔 트리 (selectable) OsiBaseNew.st
> 						390 콘솔 트리 (selectable) PosController.st
> 						391 콘솔 트리 (selectable) RamFile.st
> 						392 콘솔 트리 (selectable) SafetyCDIAS_Base.st
> 						393 콘솔 트리 (selectable) SafetyManager.st
> 						394 콘솔 트리 (selectable) SafetyRoutingTables.st
> 						395 콘솔 트리 (selectable) SafetyUDP.st
> 						396 콘솔 트리 (selectable) SdiasBase.st
> 						397 콘솔 트리 (selectable) SdiasHubBase.st
> 						398 콘솔 트리 (selectable) SdiasManager.st
> 						399 콘솔 트리 (selectable) SdiasPLC.st
> 						400 콘솔 트리 (selectable) SigCLib.st
> 						401 콘솔 트리 (selectable) String.st
> 						402 콘솔 트리 (selectable) StringInternal.st
> 						403 콘솔 트리 (selectable) SyncCall.st
> 						404 콘솔 트리 (selectable) TCPMotionInterface.st
> 						405 콘솔 트리 (selectable) Varan_Base.st
> 						406 콘솔 트리 (selectable) VaranManager.st
> 						407 콘솔 트리 (selectable) VaranManager_1.st
> 						408 콘솔 트리 (selectable) VaranManager_2.st
> 						409 콘솔 트리 (selectable) VaranManager_Base.st
> 						410 콘솔 트리 (selectable) VirtualBaseInit.st
> 						411 콘솔 트리 (selectable) WatchdogDias.st
> 						412 콘솔 트리 (selectable) XXXBase_IOs.st
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.click({window:observation.window,element_index:372});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:false,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(JSON.stringify({focused:state.accessibility?.focused_element,selected:state.accessibility?.selected_elements},null,2));}",
>   "title": "EtherCAT SDO 기반 클래스 선택",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> element 372 has no cached bounds
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(\"EtherCAT_SDOBase visible at source tree\");",
>   "title": "SDO 기반 클래스 다시 관찰",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> EtherCAT_SDOBase visible at source tree
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.click({window:observation.window,screenshotId:sid,x:1762,y:275});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(JSON.stringify({focused:state.accessibility?.focused_element,selected:state.accessibility?.selected_elements},null,2));}",
>   "title": "EtherCAT_SDOBase 선택",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {}
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"Shift_L+F10\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write((state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/메뉴|Derive|Class|Open|Edit/.test(x)).slice(-120).join(\"\\n\"));}",
>   "title": "SDO 기반 클래스 메뉴 열기",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"Return\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write((state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/EtherCAT_SDOBase|Class View|Class Editor|Secondary Actions/.test(x)).slice(0,120).join(\"\\n\"));}",
>   "title": "EtherCAT_SDOBase 클래스 열기",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.click({window:observation.window,screenshotId:sid,x:1740,y:298});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.window.title));}",
>   "title": "EtherCAT_SDOBase 편집기 열기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Elmo_3
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.click({window:observation.window,screenshotId:sid,x:1770,y:275,click_count:2});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.window.title));}",
>   "title": "EtherCAT_SDOBase 클래스 편집기 열기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - EtherCAT_SDOBase.st [RO]
> ```
