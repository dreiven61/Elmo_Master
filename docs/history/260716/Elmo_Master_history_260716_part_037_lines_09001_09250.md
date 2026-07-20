> 						154 목록 항목 (selectable)
> 						155 목록 항목 (selectable)
> 					156 스크롤 막대 ID: 59904
> 						157 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						158 위치 조정 위치 ID: ScrollbarThumb
> 						159 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			160 탭 항목 (selectable) Python Script
> 			161 탭 항목 (selectable) Output
> 			162 탭 항목 (selectable) Debugger
> 			163 단추 Close
> 		164 창 Splitter ID: 311498736
> 		165 Tab Network View ID: 305714776
> 			166 트리 ID: 104
> 				167 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					168 콘솔 트리 (selectable) HW_Network
> 					169 콘솔 트리 (selectable) Motion_Network
> 			170 탭 항목 (selectable) Lib
> 			171 탭 항목 (selectable) File
> 			172 탭 항목 (selectable) Global
> 			173 탭 항목 (selectable) Net
> 			174 탭 항목 (selectable) Class
> 			175 단추 Close
> 		176 Tab Properties ID: 305711584
> 			177 창 ID: 302137496
> 				178 TABLE Properties Window ID: 307037824
> 					179 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						180 단추 위쪽 스크롤 화살표 ID: UpButton
> 						181 위치 조정 위치 ID: ScrollbarThumb
> 						182 단추 페이지 아래로 ID: DownPageButton
> 						183 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					184 custom Name
> 					185 custom Revision
> 					186 custom Type
> 					187 custom Draw Connection
> 					188 custom Comment
> 					189 custom Filename
> 				190 도구 모음 ID: 59392
> 					191 단추
> 					192 단추
> 			193 탭 항목 (selectable) Properties
> 			194 단추 Close
>
> The focused UI element is 3 창 ID: 59648.
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
>   "title": "Inspect Power context menu region",
>   "code": "\nnodeRepl.write(JSON.stringify(wsMenuPower.screenshots.map((s,i)=>({i,id:s.id,originX:s.originX,originY:s.originY,width:s.width,height:s.height})),null,2));\n"
> }
> ```
>
> ```text
> [
>   {
>     "i": 0,
>     "id": "screenshot-0",
>     "originX": -1880,
>     "originY": 41,
>     "width": 1450,
>     "height": 763
>   },
>   {
>     "i": 1,
>     "id": "screenshot-1",
>     "originX": -1875,
>     "originY": 46,
>     "width": 1440,
>     "height": 753
>   },
>   {
>     "i": 2,
>     "id": "screenshot-2",
>     "originX": -1115,
>     "originY": 506,
>     "width": 195,
>     "height": 164
>   },
>   {
>     "i": 3,
>     "id": "screenshot-3",
>     "originX": -1111,
>     "originY": 670,
>     "width": 195,
>     "height": 4
>   },
>   {
>     "i": 4,
>     "id": "screenshot-4",
>     "originX": -920,
>     "originY": 510,
>     "width": 4,
>     "height": 160
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "View Power context menu",
>   "code": "\nawait nodeRepl.emitImage(wsMenuPower.screenshots[2].url);\n"
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
>   "title": "Find Power implementation",
>   "code": "\nawait sky.click({window:targetWindow, x:96, y:149, screenshotId:\"screenshot-2\"});\nawait new Promise(r=>setTimeout(r,900));\nvar wsAfterPower = await sky.get_window_state({window:targetWindow, include_text:true});\nnodeRepl.write(JSON.stringify({title:wsAfterPower.window?.title,focused:wsAfterPower.accessibility?.focused_element},null,2));\n"
> }
> ```
>
> ```text
> {
>   "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface",
>   "focused": "13 창 ID: 59648"
> }
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
>   "title": "Return to Motion network for pos lookup",
>   "code": "\nawait sky.click({window:targetWindow, x:58, y:143, screenshotId:\"screenshot-0\"});\nawait new Promise(r=>setTimeout(r,500));\nvar wsNetAgain = await sky.get_window_state({window:targetWindow, include_text:false});\nnodeRepl.write(JSON.stringify({title:wsNetAgain.window?.title,shots:wsNetAgain.screenshots.map(s=>({id:s.id,originX:s.originX,originY:s.originY,width:s.width,height:s.height}))},null,2));\n"
> }
> ```
>
> ```text
> {
>   "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network",
>   "shots": [
>     {
>       "id": "screenshot-0",
>       "originX": -1880,
>       "originY": 41,
>       "width": 1450,
>       "height": 763
>     },
>     {
>       "id": "screenshot-1",
>       "originX": -1875,
>       "originY": 46,
>       "width": 1440,
>       "height": 753
>     },
>     {
>       "id": "screenshot-2",
>       "originX": -1823,
>       "originY": 155,
>       "width": 742,
>       "height": 35
>     }
>   ]
> }
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
>   "title": "Open pos implementation lookup",
>   "code": "\nawait sky.click({window:targetWindow, x:766, y:488, mouse_button:\"right\", screenshotId:\"screenshot-0\"});\nawait new Promise(r=>setTimeout(r,350));\nvar wsMenuPos = await sky.get_window_state({window:targetWindow, include_text:false});\nnodeRepl.write(JSON.stringify(wsMenuPos.screenshots.map((s,i)=>({i,id:s.id,originX:s.originX,originY:s.originY,width:s.width,height:s.height})),null,2));\n"
> }
> ```
>
> ```text
> [
>   {
>     "i": 0,
>     "id": "screenshot-0",
>     "originX": -1880,
>     "originY": 41,
>     "width": 1450,
>     "height": 763
>   },
>   {
>     "i": 1,
>     "id": "screenshot-1",
>     "originX": -1875,
>     "originY": 46,
>     "width": 1440,
>     "height": 753
>   },
>   {
>     "i": 2,
>     "id": "screenshot-2",
>     "originX": -1114,
>     "originY": 529,
>     "width": 195,
>     "height": 164
>   },
>   {
>     "i": 3,
>     "id": "screenshot-3",
>     "originX": -1110,
>     "originY": 693,
>     "width": 195,
>     "height": 4
>   },
>   {
>     "i": 4,
>     "id": "screenshot-4",
>     "originX": -919,
>     "originY": 533,
>     "width": 4,
>     "height": 160
>   }
> ]
> ```
>
> Image output: image/jpeg
>
