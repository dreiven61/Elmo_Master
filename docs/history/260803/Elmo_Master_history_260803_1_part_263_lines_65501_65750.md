>
> ```json
> {
>   "title": "Open LASAL file menu",
>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.click({window:observation.window,screenshotId:sid,x:43,y:42});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(0,1800));}"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			3684 창 Motion_Network Secondary Actions: Raise ID: 65282
> 				3685 창 ID: 59648
> 					3686 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						3687 단추 위쪽 스크롤 화살표 ID: UpButton
> 						167616 단추 페이지 위로 ID: UpPageButton
> 						3688 위치 조정 위치 ID: ScrollbarThumb
> 						3689 단추 페이지 아래로 ID: DownPageButton
> 						3690 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					3691 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						3692 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						166430 단추 페이지 왼쪽으로 ID: UpPageButton
> 						3693 위치 조정 위치 ID: ScrollbarThumb
> 						3695 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					3696 위치 조정 (disabled)
> 			136003 창 Comm_Network Secondary Actions: Raise ID: 65287
> 				136004 창 ID: 59648
> 					136005 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						136006 단추 위쪽 스크롤 화살표 ID: UpButton
> 						137384 단추 페이지 위로 ID: UpPageButton
> 						136007 위치 조정 위치 ID: ScrollbarThumb
> 						136008 단추 페이지 아래로 ID: DownPageButton
> 						136009 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					136010 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						136011 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						136012 위치 조정 위치 ID: ScrollbarThumb
> 						136013 단추 페이지 오른쪽으로 ID: DownPageButton
> 						136014 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					136015 위치 조정 (disabled)
> 			132746 창 Comm_Network.lcn Secondary Actions: Raise ID: 65286
> 				132747 창 ID: 59648
> 					132748 창 <?xml version="1.0" encoding="ISO-8859-1" ?> <Network Name="Comm_Network" Revision="0.0"> <!-- List of Components in this network --> <Components> <Object Name = "LMCControlCommandService1" GUID = "{5E164D6C-7E45-4BA4-B0F7-F9DBCCE8C71B}" Class = "LMCControlCommandService" Position = "(930,1380)" Visualized
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Choose LASAL Open File",
>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.click({window:observation.window,screenshotId:sid,x:84,y:265});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(JSON.stringify({tree:(globalThis.state.accessibility?.tree||\"\").slice(0,1900),shots:globalThis.state.screenshots.map(s=>({w:s.width,h:s.height,x:s.originX,y:s.originY,z:s.zIndex}))}));}"
> }
> ```
>
> ```text
> {"tree":"","shots":[{"w":1450,"h":763,"x":-1880,"y":41,"z":0},{"w":1440,"h":753,"x":-1875,"y":46,"z":1},{"w":1904,"h":1016,"x":-1912,"y":-3,"z":2}]}
> ```
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
>   "title": "Enter Motion network source path",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.type_text({window:observation.window,text:\"Network\\\\Motion_Network\\\\Motion_Network.lcn\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(JSON.stringify({shots:globalThis.state.screenshots.map(s=>({w:s.width,h:s.height,x:s.originX,y:s.originY,z:s.zIndex}))}));}"
> }
> ```
>
> ```text
> {"shots":[{"w":1450,"h":763,"x":-1880,"y":41,"z":0},{"w":1440,"h":753,"x":-1875,"y":46,"z":1},{"w":1904,"h":1016,"x":-1912,"y":-3,"z":2}]}
> ```
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
>   "title": "Open Motion network source file",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"Return\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(JSON.stringify({tree:(globalThis.state.accessibility?.tree||\"\").slice(0,1200),shots:globalThis.state.screenshots.map(s=>({w:s.width,h:s.height,x:s.originX,y:s.originY,z:s.zIndex}))}));}"
> }
> ```
>
> ```text
> {"tree":"","shots":[{"w":1450,"h":763,"x":-1880,"y":41,"z":0},{"w":1440,"h":753,"x":-1875,"y":46,"z":1}]}
> ```
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
>   "title": "Go to Motion network connections",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"Ctrl+g\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(JSON.stringify({tree:(globalThis.state.accessibility?.tree||\"\").slice(0,1200),shots:globalThis.state.screenshots.map(s=>({w:s.width,h:s.height,x:s.originX,y:s.originY,z:s.zIndex}))}));}"
> }
> ```
>
> ```text
> {"tree":"Window: \"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network.lcn\", App: Lasal2.exe.\n\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network.lcn Secondary Actions: Raise\n\t\t1 창 작업 영역 ID: 59648\n\t\t\t173984 창 Motion_Network.lcn Secondary Actions: Raise ID: 65288\n\t\t\t\t173985 창 ID: 59648\n\t\t\t\t\t173986 창 <?xml version=\"1.0\" encoding=\"ISO-8859-1\" ?> <Network Name=\"Motion_Network\" Revision=\"0.0\"> <!-- List of Components in this network --> <Components> <Object Name = \"_LMCAxis1\" GUID = \"{BC662A7E-E49E-4110-AD6B-E49F8C8FE6F2}\" Class = \"_LMCAxis\" Position = \"(3090,300)\" Visualized = \"true\" Remotely = \"true\" RealTime = \"1 ms\" CyclicTime = \"10 ms\" BackgroundTime = \"100 ms\"> <Channels> <Server Name=\"AbsEncoder\" Value=\"1\"/> <Server Name=\"ActPos\"/> <Server Name=\"ActVelocity\"/> <Server Name=\"AEmergency\" Value=\"0\"/> <Server Name=\"AMax\" Value=\"7500 mm\"/> <Server Name=\"AxisError\"/> <Server Name=\"AxisStatus\"/> <Server Name=\"Control\"/> <Server Name=\"Destination\"/> <Server Name=\"ErrorConfig\" Value=\"2#0000000001000000\"/> <Server Name=\"ExUnits\" Value=\"8388608\"/> <Server Name=\"IntUnits\" Value=\"1 mm\"/> <Server Name=\"JMax\" Value=\"75000 mm\"/> <Server Name=\"Modulo\" Value=\"0\"/","shots":[{"w":1450,"h":763,"x":-1880,"y":41,"z":0},{"w":1440,"h":753,"x":-1875,"y":46,"z":1},{"w":260,"h":133,"x":-1285,"y":352,"z":2}]}
> ```
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
>   "title": "Enter connection line number",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.type_text({window:observation.window,text:\"4751\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(String(globalThis.state.accessibility?.focused_element||\"\"));}"
> }
> ```
>
> ```text
> 174537 대화 상자 Goto line Secondary Actions: Raise
> ```
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
>   "title": "Navigate to connection list",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"Return\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(JSON.stringify({focused:globalThis.state.accessibility?.focused_element,selectedText:globalThis.state.accessibility?.selected_text}));}"
> }
> ```
>
> ```text
> {}
> ```
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
>   "title": "Position cursor after first connection",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"End\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(String(globalThis.state.accessibility?.focused_element||\"\"));}"
> }
> ```
>
> ```text
> 173986 창 <?xml version="1.0" encoding="ISO-8859-1" ?> <Network Name="Motion_Network" Revision="0.0"> <!-- List of Components in this network --> <Components> <Object Name = "_LMCAxis1" GUID = "{BC662A7E-E49E-4110-AD6B-E49F8C8FE6F2}" Class = "_LMCAxis" Position = "(3090,300)" Visualized = "true" Remotely = "true" RealTime = "1 ms" CyclicTime = "10 ms" BackgroundTime = "100 ms"> <Channels> <Server Name="AbsEncoder" Value="1"/> <Server Name="ActPos"/> <Server Name="ActVelocity"/> <Server Name="AEmergency" Value="0"/> <Server Name="AMax" Value="7500 mm"/> <Server Name="AxisError"/> <Server Name="AxisStatus"/> <Server Name="Control"/> <Server Name="Destination"/> <Server Name="ErrorConfig" Value="2#0000000001000000"/> <Server Name="ExUnits" Value="8388608"/> <Server Name="IntUnits" Value="1 mm"/> <Server Name="JMax" Value="75000 mm"/> <Server Name="Modulo" Value="0"/> <Server Name="MoveType" Value="_JERK_PROFILE"/> <Server Name="Override"/> <Server Name="PosMode"/> <Server Name="Resolution" Value="1"/> <Server Name="SafeAxisState"/> <Server Name="SetPos"/> <Server Name="SetVelocity"/> <Server Name="SimulateMode" Value="0"/> <Server Name="SWMaxPos" Value="10000 mm"/> <Server Name="SWMinPos" Value="-10000 mm"/> <Server Name="Tjerk" Value="0"/> <Server Name="VMax" Value="75 mm"/> <Client Name="ActPosition"/> <Client Name="Config" Value="0"/> <Client Name="Emergency"/> <Client Name="HWError"/> <Client Name="HWMax"/> <Client Name="HWMin"/> <Client Name="HWReady"/> <Client Name="ID" Value="0"/> <Client Name="LatchPos"/> <Client Name="LMCController"/> <Client Name="LMCPostRtWorkTrigger"/> <Client Name="LMCPreRtWorkTrigger" DrawChnConn="Text"/> <Client Name="Provider"/> <Client Name="RefSwitch"/> <Client Name="ZImpulse"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{C43A02B1-A319-4132-B2FA-36A920C832E8}" Class="_LMCAxisBase"> <Channels> <Server Name="ActPos"/> <Server Name="ActVelocity"/> <Server Name="AEmergency" Value="0"/> <Server Name="AMax" Value="7500 mm"/> <Server Name="AxisError"/> <Server Name="AxisStatus"/> <Server Name="Control"/> <Server Name="Destination"/> <Server Name="ErrorConfig" Value="2#0000000001000000"/> <Server Name="ExUnits" Value="8388608"/> <Server Name="IntUnits" Value="1 mm"/> <Server Name="JMax" Value="75000 mm"/> <Server Name="Modulo" Value="0"/> <Server Name="MoveType" Value="_JERK_PROFILE"/> <Server Name="Override"/> <Server Name="PosMode"/> <Server Name="Resolution" Value="1"/> <Server Name="SafeAxisState"/> <Server Name="SetPos"/> <Server Name="SetVelocity"/> <Server Name="SimulateMode" Value="0"/> <Server Name="SWMaxPos" Value="10000 mm"/> <Server Name="SWMinPos" Value="-10000 mm"/> <Server Name="Tjerk" Value="0"/> <Server Name="VMax" Value="75 mm"/> <Client Name="ActPosition"/> <Client Name="LMCAbsEncoder"/> <Client Name="LMCController"/> <Client Name="LMCMath"/> <Client Name="LMCPostRtWorkTrigger"/> <Client Name="LMCPreRtWorkTrigger"/> <Client Name="LMCPublisher"/> <Client Name="LMCReference"/> <Client Name="LMCSafety"/> <Client Name="TaskObjectControl"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{E661412E-354E-4C9A-A848-2B297415C10D}" Class="_AxisBase"> <Channels> <Server Name="AxisStatus"/> <Server Name="Control"/> ID: 10000
> ```
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
>   "title": "Move to end of connection line",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"END\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(String(globalThis.state.accessibility?.focused_element||\"\"));}"
> }
> ```
>
> ```text
> 173986 창 <?xml version="1.0" encoding="ISO-8859-1" ?> <Network Name="Motion_Network" Revision="0.0"> <!-- List of Components in this network --> <Components> <Object Name = "_LMCAxis1" GUID = "{BC662A7E-E49E-4110-AD6B-E49F8C8FE6F2}" Class = "_LMCAxis" Position = "(3090,300)" Visualized = "true" Remotely = "true" RealTime = "1 ms" CyclicTime = "10 ms" BackgroundTime = "100 ms"> <Channels> <Server Name="AbsEncoder" Value="1"/> <Server Name="ActPos"/> <Server Name="ActVelocity"/> <Server Name="AEmergency" Value="0"/> <Server Name="AMax" Value="7500 mm"/> <Server Name="AxisError"/> <Server Name="AxisStatus"/> <Server Name="Control"/> <Server Name="Destination"/> <Server Name="ErrorConfig" Value="2#0000000001000000"/> <Server Name="ExUnits" Value="8388608"/> <Server Name="IntUnits" Value="1 mm"/> <Server Name="JMax" Value="75000 mm"/> <Server Name="Modulo" Value="0"/> <Server Name="MoveType" Value="_JERK_PROFILE"/> <Server Name="Override"/> <Server Name="PosMode"/> <Server Name="Resolution" Value="1"/> <Server Name="SafeAxisState"/> <Server Name="SetPos"/> <Server Name="SetVelocity"/> <Server Name="SimulateMode" Value="0"/> <Server Name="SWMaxPos" Value="10000 mm"/> <Server Name="SWMinPos" Value="-10000 mm"/> <Server Name="Tjerk" Value="0"/> <Server Name="VMax" Value="75 mm"/> <Client Name="ActPosition"/> <Client Name="Config" Value="0"/> <Client Name="Emergency"/> <Client Name="HWError"/> <Client Name="HWMax"/> <Client Name="HWMin"/> <Client Name="HWReady"/> <Client Name="ID" Value="0"/> <Client Name="LatchPos"/> <Client Name="LMCController"/> <Client Name="LMCPostRtWorkTrigger"/> <Client Name="LMCPreRtWorkTrigger" DrawChnConn="Text"/> <Client Name="Provider"/> <Client Name="RefSwitch"/> <Client Name="ZImpulse"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{C43A02B1-A319-4132-B2FA-36A920C832E8}" Class="_LMCAxisBase"> <Channels> <Server Name="ActPos"/> <Server Name="ActVelocity"/> <Server Name="AEmergency" Value="0"/> <Server Name="AMax" Value="7500 mm"/> <Server Name="AxisError"/> <Server Name="AxisStatus"/> <Server Name="Control"/> <Server Name="Destination"/> <Server Name="ErrorConfig" Value="2#0000000001000000"/> <Server Name="ExUnits" Value="8388608"/> <Server Name="IntUnits" Value="1 mm"/> <Server Name="JMax" Value="75000 mm"/> <Server Name="Modulo" Value="0"/> <Server Name="MoveType" Value="_JERK_PROFILE"/> <Server Name="Override"/> <Server Name="PosMode"/> <Server Name="Resolution" Value="1"/> <Server Name="SafeAxisState"/> <Server Name="SetPos"/> <Server Name="SetVelocity"/> <Server Name="SimulateMode" Value="0"/> <Server Name="SWMaxPos" Value="10000 mm"/> <Server Name="SWMinPos" Value="-10000 mm"/> <Server Name="Tjerk" Value="0"/> <Server Name="VMax" Value="75 mm"/> <Client Name="ActPosition"/> <Client Name="LMCAbsEncoder"/> <Client Name="LMCController"/> <Client Name="LMCMath"/> <Client Name="LMCPostRtWorkTrigger"/> <Client Name="LMCPreRtWorkTrigger"/> <Client Name="LMCPublisher"/> <Client Name="LMCReference"/> <Client Name="LMCSafety"/> <Client Name="TaskObjectControl"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{E661412E-354E-4C9A-A848-2B297415C10D}" Class="_AxisBase"> <Channels> <Server Name="AxisStatus"/> <Server Name="Control"/> ID: 10000
> ```
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
>   "title": "Create insertion line for axis clients",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"Return\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(String(globalThis.state.accessibility?.focused_element||\"\"));}"
> }
> ```
>
> ```text
> 173986 창 <?xml version="1.0" encoding="ISO-8859-1" ?> <Network Name="Motion_Network" Revision="0.0"> <!-- List of Components in this network --> <Components> <Object Name = "_LMCAxis1" GUID = "{BC662A7E-E49E-4110-AD6B-E49F8C8FE6F2}" Class = "_LMCAxis" Position = "(3090,300)" Visualized = "true" Remotely = "true" RealTime = "1 ms" CyclicTime = "10 ms" BackgroundTime = "100 ms"> <Channels> <Server Name="AbsEncoder" Value="1"/> <Server Name="ActPos"/> <Server Name="ActVelocity"/> <Server Name="AEmergency" Value="0"/> <Server Name="AMax" Value="7500 mm"/> <Server Name="AxisError"/> <Server Name="AxisStatus"/> <Server Name="Control"/> <Server Name="Destination"/> <Server Name="ErrorConfig" Value="2#0000000001000000"/> <Server Name="ExUnits" Value="8388608"/> <Server Name="IntUnits" Value="1 mm"/> <Server Name="JMax" Value="75000 mm"/> <Server Name="Modulo" Value="0"/> <Server Name="MoveType" Value="_JERK_PROFILE"/> <Server Name="Override"/> <Server Name="PosMode"/> <Server Name="Resolution" Value="1"/> <Server Name="SafeAxisState"/> <Server Name="SetPos"/> <Server Name="SetVelocity"/> <Server Name="SimulateMode" Value="0"/> <Server Name="SWMaxPos" Value="10000 mm"/> <Server Name="SWMinPos" Value="-10000 mm"/> <Server Name="Tjerk" Value="0"/> <Server Name="VMax" Value="75 mm"/> <Client Name="ActPosition"/> <Client Name="Config" Value="0"/> <Client Name="Emergency"/> <Client Name="HWError"/> <Client Name="HWMax"/> <Client Name="HWMin"/> <Client Name="HWReady"/> <Client Name="ID" Value="0"/> <Client Name="LatchPos"/> <Client Name="LMCController"/> <Client Name="LMCPostRtWorkTrigger"/> <Client Name="LMCPreRtWorkTrigger" DrawChnConn="Text"/> <Client Name="Provider"/> <Client Name="RefSwitch"/> <Client Name="ZImpulse"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{C43A02B1-A319-4132-B2FA-36A920C832E8}" Class="_LMCAxisBase"> <Channels> <Server Name="ActPos"/> <Server Name="ActVelocity"/> <Server Name="AEmergency" Value="0"/> <Server Name="AMax" Value="7500 mm"/> <Server Name="AxisError"/> <Server Name="AxisStatus"/> <Server Name="Control"/> <Server Name="Destination"/> <Server Name="ErrorConfig" Value="2#0000000001000000"/> <Server Name="ExUnits" Value="8388608"/> <Server Name="IntUnits" Value="1 mm"/> <Server Name="JMax" Value="75000 mm"/> <Server Name="Modulo" Value="0"/> <Server Name="MoveType" Value="_JERK_PROFILE"/> <Server Name="Override"/> <Server Name="PosMode"/> <Server Name="Resolution" Value="1"/> <Server Name="SafeAxisState"/> <Server Name="SetPos"/> <Server Name="SetVelocity"/> <Server Name="SimulateMode" Value="0"/> <Server Name="SWMaxPos" Value="10000 mm"/> <Server Name="SWMinPos" Value="-10000 mm"/> <Server Name="Tjerk" Value="0"/> <Server Name="VMax" Value="75 mm"/> <Client Name="ActPosition"/> <Client Name="LMCAbsEncoder"/> <Client Name="LMCController"/> <Client Name="LMCMath"/> <Client Name="LMCPostRtWorkTrigger"/> <Client Name="LMCPreRtWorkTrigger"/> <Client Name="LMCPublisher"/> <Client Name="LMCReference"/> <Client Name="LMCSafety"/> <Client Name="TaskObjectControl"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{E661412E-354E-4C9A-A848-2B297415C10D}" Class="_AxisBase"> <Channels> <Server Name="AxisStatus"/> <Server Name="Control"/> ID: 10000
> ```
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
>   "title": "Insert four axis control connections",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.type_text({window:observation.window,text:\"<Connection Source=\\\"LMCEcatInputLatch1.LMCAxis1\\\" Destination=\\\"_LMCAxis1.Control\\\"/>\\r\\n\\t\\t<Connection Source=\\\"LMCEcatInputLatch1.LMCAxis2\\\" Destination=\\\"_LMCAxis2.Control\\\"/>\\r\\n\\t\\t<Connection Source=\\\"LMCEcatInputLatch1.LMCAxis3\\\" Destination=\\\"_LMCAxis3.Control\\\"/>\\r\\n\\t\\t<Connection Source=\\\"LMCEcatInputLatch1.LMCAxis4\\\" Destination=\\\"_LMCAxis4.Control\\\"/>\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(String(globalThis.state.accessibility?.focused_element||\"\"));}"
> }
> ```
>
> ```text
> 173986 창 <?xml version="1.0" encoding="ISO-8859-1" ?> <Network Name="Motion_Network" Revision="0.0"> <!-- List of Components in this network --> <Components> <Object Name = "_LMCAxis1" GUID = "{BC662A7E-E49E-4110-AD6B-E49F8C8FE6F2}" Class = "_LMCAxis" Position = "(3090,300)" Visualized = "true" Remotely = "true" RealTime = "1 ms" CyclicTime = "10 ms" BackgroundTime = "100 ms"> <Channels> <Server Name="AbsEncoder" Value="1"/> <Server Name="ActPos"/> <Server Name="ActVelocity"/> <Server Name="AEmergency" Value="0"/> <Server Name="AMax" Value="7500 mm"/> <Server Name="AxisError"/> <Server Name="AxisStatus"/> <Server Name="Control"/> <Server Name="Destination"/> <Server Name="ErrorConfig" Value="2#0000000001000000"/> <Server Name="ExUnits" Value="8388608"/> <Server Name="IntUnits" Value="1 mm"/> <Server Name="JMax" Value="75000 mm"/> <Server Name="Modulo" Value="0"/> <Server Name="MoveType" Value="_JERK_PROFILE"/> <Server Name="Override"/> <Server Name="PosMode"/> <Server Name="Resolution" Value="1"/> <Server Name="SafeAxisState"/> <Server Name="SetPos"/> <Server Name="SetVelocity"/> <Server Name="SimulateMode" Value="0"/> <Server Name="SWMaxPos" Value="10000 mm"/> <Server Name="SWMinPos" Value="-10000 mm"/> <Server Name="Tjerk" Value="0"/> <Server Name="VMax" Value="75 mm"/> <Client Name="ActPosition"/> <Client Name="Config" Value="0"/> <Client Name="Emergency"/> <Client Name="HWError"/> <Client Name="HWMax"/> <Client Name="HWMin"/> <Client Name="HWReady"/> <Client Name="ID" Value="0"/> <Client Name="LatchPos"/> <Client Name="LMCController"/> <Client Name="LMCPostRtWorkTrigger"/> <Client Name="LMCPreRtWorkTrigger" DrawChnConn="Text"/> <Client Name="Provider"/> <Client Name="RefSwitch"/> <Client Name="ZImpulse"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{C43A02B1-A319-4132-B2FA-36A920C832E8}" Class="_LMCAxisBase"> <Channels> <Server Name="ActPos"/> <Server Name="ActVelocity"/> <Server Name="AEmergency" Value="0"/> <Server Name="AMax" Value="7500 mm"/> <Server Name="AxisError"/> <Server Name="AxisStatus"/> <Server Name="Control"/> <Server Name="Destination"/> <Server Name="ErrorConfig" Value="2#0000000001000000"/> <Server Name="ExUnits" Value="8388608"/> <Server Name="IntUnits" Value="1 mm"/> <Server Name="JMax" Value="75000 mm"/> <Server Name="Modulo" Value="0"/> <Server Name="MoveType" Value="_JERK_PROFILE"/> <Server Name="Override"/> <Server Name="PosMode"/> <Server Name="Resolution" Value="1"/> <Server Name="SafeAxisState"/> <Server Name="SetPos"/> <Server Name="SetVelocity"/> <Server Name="SimulateMode" Value="0"/> <Server Name="SWMaxPos" Value="10000 mm"/> <Server Name="SWMinPos" Value="-10000 mm"/> <Server Name="Tjerk" Value="0"/> <Server Name="VMax" Value="75 mm"/> <Client Name="ActPosition"/> <Client Name="LMCAbsEncoder"/> <Client Name="LMCController"/> <Client Name="LMCMath"/> <Client Name="LMCPostRtWorkTrigger"/> <Client Name="LMCPreRtWorkTrigger"/> <Client Name="LMCPublisher"/> <Client Name="LMCReference"/> <Client Name="LMCSafety"/> <Client Name="TaskObjectControl"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{E661412E-354E-4C9A-A848-2B297415C10D}" Class="_AxisBase"> <Channels> <Server Name="AxisStatus"/> <Server Name="Control"/> ID: 10000
> ```
>
> Image output: image/jpeg
