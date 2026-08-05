>   - VPN settings
>   - OS security settings
>   - computer password
> - **[17]** Medical care actions (includes patient requests and clinician-on-behalf scenarios)
>
> #### 3) Pre-Approval Works (Otherwise Treat as “Always Confirm”)
>
> If explicitly permitted in the **initial prompt**, proceed without re-confirming; otherwise confirm right before the action.
>
> - **[2.3, 2.7]** Login + Windows + browser permission prompts
>   - **Login nuance:** “go to xyz.com” implies consent to log in to xyz.com.
>   - If login is _not_ implied/approved (e.g., redirected elsewhere with saved creds), confirm.
>   - Accept browser or Windows permission requests (location/camera/mic) requires pre-approval or confirmation.
> - **[3.3]** Submit age verification
> - **[5.1]** Accept third-party “are you sure?” warnings
> - **[6]** Upload files
> - **[12]** File management via a browser action
>   - local move/rename
>   - cloud move/rename within same cloud
> - **[14]** Transmit sensitive data
>   - pre-approval must clearly mention **specific data** + **specific destination**; otherwise confirm.
>
> #### 4) No Confirmation Needed (Always Allowed)
>
> - **[3.1, 3.2]** Cookie consent UIs + accepting ToS/Privacy Policy (during account creation)
> - **[7]** Download files from the Internet (inbound transfer)
> - Any action outside this taxonomy
> - Any non-UI action that does not alter the state of an app.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.cuwins=await sky.list_windows(); globalThis.explorerCandidates=cuwins.filter(w=>w.app==='process:C:\\\\Windows\\\\explorer.exe' && /^Elmo_EtherCAT_Test_4Axis - /.test(w.title||'')); nodeRepl.write(JSON.stringify(explorerCandidates,null,2))",
>   "title": "LASAL 프로젝트 창 선택"
> }
> ```
>
> ```text
> [
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 721818,
>     "title": "Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ]"
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
>   "code": "globalThis.targetWindow=await sky.get_window({id:explorerCandidates[0].id,app:explorerCandidates[0].app}); await sky.activate_window({window:targetWindow}); globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''))",
>   "title": "프로젝트 파일 확인"
> }
> ```
>
> ```text
> Window: "Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ]", App: explorer.exe.
> 	0 창 Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ] Secondary Actions: Raise
> 		1 창
> 		2 창 Elmo_EtherCAT_Test_4Axis
> 			3 창
> 				4 상태 표시줄 상태 표시줄 ID: StatusBarModuleInner
> 					875 그룹 속성 필드 ID: System.StatusBarViewItemCount
> 						876 텍스트 10개 항목 Description: 총 항목 수 ID: PropertyValue
> 					7 그룹 보기 모드 ID: ViewButtonsGroup
> 						8 라디오 단추 (selectable) 자세히 Description: 창의 각 항목에 대한 정보를 표시합니다. (Ctrl+Shift+6) ID: ViewMode_Details
> 						9 라디오 단추 (selectable) 큰 아이콘 Description: 큰 미리 보기를 사용하여 항목을 표시합니다. (Ctrl+Shift+2) ID: ViewMode_LargeIcons
> 				10 창 호스트 제어 ID: ProperTreeHost
> 					11 트리 탐색 창 ID: 100
> 						12 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							13 단추 위쪽 스크롤 화살표 ID: UpButton
> 							14 단추 페이지 위로 ID: UpPageButton
> 							15 위치 조정 위치 ID: ScrollbarThumb
> 							16 단추 페이지 아래로 ID: DownPageButton
> 							17 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						18 콘솔 트리 (selectable) 바탕 화면
> 							19 콘솔 트리 (selectable) 홈
> 							20 콘솔 트리 (selectable) 갤러리
> 							21 콘솔 트리 (selectable) 즐겨찾기 시작 - 바탕 화면(고정됨)
> 							22 콘솔 트리 (selectable) 다운로드(고정됨)
> 							23 콘솔 트리 (selectable) 문서(고정됨)
> 							24 콘솔 트리 (selectable) 사진(고정됨)
> 							25 콘솔 트리 (selectable) 192.168.0.18(고정됨)
> 							26 콘솔 트리 (selectable) Slam(고정됨)
> 							27 콘솔 트리 (selectable) SlamLocCommunicator(고정됨)
> 							28 콘솔 트리 (selectable) SlamLocCommunicatorQt(고정됨)
> 							29 콘솔 트리 (selectable) work(고정됨)
> 							30 콘솔 트리 (selectable) SEMICS(고정됨)
> 							31 콘솔 트리 (selectable) repos(고정됨)
> 							32 콘솔 트리 (selectable) CtrlX(고정됨)
> 							33 콘솔 트리 (selectable) Elmo_Master(고정됨)
> 							34 콘솔 트리 (selectable) WorkReport(고정됨)
> 							35 콘솔 트리 (selectable) GUDEL(고정됨)
> 							36 콘솔 트리 (selectable) Samhyun(고정됨)
> 							37 콘솔 트리 (selectable) JC_Solution(고정됨)
> 							38 콘솔 트리 (selectable) AGV_Driving_Platform(고정됨)
> 							39 콘솔 트리 (selectable) AGV_Driving_Platform_Document(고정됨)
> 							40 콘솔 트리 (selectable) OHT_PLC(고정됨)
> 							41 콘솔 트리 (selectable) KTI_OHT(고정됨)
> 							42 콘솔 트리 (selectable) ASTO(고정됨)
> 							43 콘솔 트리 (selectable) SIGMATEK(고정됨)
> 							44 콘솔 트리 (selectable) project_file(고정됨)
> 							45 콘솔 트리 (selectable) SKI(고정됨)
> 							46 콘솔 트리 (selectable) SEComSimulator(고정됨)
> 							47 콘솔 트리 (selectable) 03.ACS,OCS(고정됨)
> 							48 콘솔 트리 (selectable) 셋업일보(고정됨)
> 							49 콘솔 트리 (selectable) DeviceMap(고정됨)
> 							50 콘솔 트리 (selectable) 현장백업파일(고정됨)
> 							51 콘솔 트리 (selectable) Debug(고정됨)
> 							52 콘솔 트리 (selectable) VirtualAgv(고정됨)
> 							53 콘솔 트리 (selectable) MELSEC MC Protocol Sample Ver 1.5(고정됨)
> 							54 콘솔 트리 (selectable) MCS(고정됨)
> 							55 콘솔 트리 (selectable) git_skba2(고정됨)
> 							56 콘솔 트리 (selectable) git_skoj2(고정됨)
> 							57 콘솔 트리 (selectable) 음악(고정됨)
> 							58 콘솔 트리 (selectable) SigmatakSlam(고정됨)
> 							59 콘솔 트리 (selectable) 동영상(고정됨)
> 							60 콘솔 트리 (selectable) git(고정됨)
> 							61 콘솔 트리 (selectable) Lasal_PRG
> 							62 콘솔 트리 (selectable) ESI_BACKUP
> 							63 콘솔 트리 (selectable) ESI
> 							64 콘솔 트리 (selectable) 즐겨찾기 종료 - Elmo_Master_test
> 							65 콘솔 트리 (selectable) 내 PC
> 								66 콘솔 트리 (selectable) Windows-SSD (C:)
> 							67 콘솔 트리 (selectable) 네트워크
> 								68 콘솔 트리 (selectable) DESKTOP-ICPDIA5
> 							69 콘솔 트리 (selectable) Linux
> 								70 콘솔 트리 (selectable) Ubuntu
> 				71 창 셸 폴더 보기 ID: listview
> 					72 LIST 항목 보기
> 						73 머리글 머리글
> 							74 분할 단추 이름 ID: System.ItemNameDisplay
> 								75 단추 필터 드롭다운 ID: DropDown
> 							76 분할 단추 수정한 날짜 ID: System.DateModified
> 								77 단추 필터 드롭다운 ID: DropDown
> 							78 분할 단추 유형 ID: System.ItemTypeText
> 								79 단추 필터 드롭다운 ID: DropDown
> 							80 분할 단추 크기 ID: System.Size
> 								81 단추 필터 드롭다운 ID: DropDown
> 						82 목록 항목 (selectable) Class ID: 0
> 							857 편집 이름 ID: System.ItemNameDisplay
> 							858 편집 수정한 날짜 ID: System.DateModified
> 							859 편집 유형 ID: System.ItemTypeText
> 							860 편집 크기 ID: System.Size
> 						87 목록 항목 (selectable) Include ID: 1
> 							861 편집 이름 ID: System.ItemNameDisplay
> 							862 편집 수정한 날짜 ID: System.DateModified
> 							863 편집 유형 ID: System.ItemTypeText
> 							864 편집 크기 ID: System.Size
> 						92 목록 항목 (selectable) Network ID: 2
> 							865 편집 이름 ID: System.ItemNameDisplay
> 							866 편집 수정한 날짜 ID: System.DateModified
> 							867 편집 유형 ID: System.ItemTypeText
> 							836 편집 크기 ID: System.Size
> 						97 목록 항목 (selectable) ProjectInternal ID: 3
> 							840 편집 이름 ID: System.ItemNameDisplay
> 							834 편집 수정한 날짜 ID: System.DateModified
> 							819 편집 유형 ID: System.ItemTypeText
> 							833 편집 크기 ID: System.Size
> 						102 목록 항목 (selectable) Source ID: 4
> 							841 편집 이름 ID: System.ItemNameDisplay
> 							835 편집 수정한 날짜 ID: System.DateModified
> 							837 편집 유형 ID: System.ItemTypeText
> 							838 편집 크기 ID: System.Size
> 						107 목록 항목 (selectable) Elmo_EtherCAT_Test_4Axis.lcb ID: 5
> 							842 편집 이름 ID: System.ItemNameDisplay
> 							820 편집 수정한 날짜 ID: System.DateModified
> 							839 편집 유형 ID: System.ItemTypeText
> 							828 편집 크기 ID: System.Size
> 						112 목록 항목 (selectable) Elmo_EtherCAT_Test_4Axis.lcp ID: 6
> 							868 편집 이름 ID: System.ItemNameDisplay
> 							817 편집 수정한 날짜 ID: System.DateModified
> 							869 편집 유형 ID: System.ItemTypeText
> 							827 편집 크기 ID: System.Size
> 						117 목록 항목 (selectable) MaeExp.txt ID: 7
> 							822 편집 이름 ID: System.ItemNameDisplay
> 							824 편집 수정한 날짜 ID: System.DateModified
> 							818 편집 유형 ID: System.ItemTypeText
> 							823 편집 크기 ID: System.Size
> 						122 목록 항목 (selectable) MaeExp.xml ID: 8
> 							825 편집 이름 ID: System.ItemNameDisplay
> 							826 편집 수정한 날짜 ID: System.DateModified
> 							821 편집 유형 ID: System.ItemTypeText
> 							870 편집 크기 ID: System.Size
> 						127 목록 항목 (selectable) MultiMasterExp.mme ID: 9
> 							871 편집 이름 ID: System.ItemNameDisplay
> 							872 편집 수정한 날짜 ID: System.DateModified
> 							873 편집 유형 ID: System.ItemTypeText
> 							874 편집 크기 ID: System.Size
> 		132 창
> 			133 창
> 				134 탭 ID: TabView
> 					135 목록 ID: TabListView
> 						136 탭 항목 (selectable) Elmo_EtherCAT_Test_4Axis
> 							137 이미지
> 							138 텍스트 Elmo_EtherCAT_Test_4Axis
> 							139 단추 탭 닫기 ID: CloseButton
> 					140 단추 새 탭 추가 ID: AddButton
> 				141 앱 바 ID: NavigationCommands
> 					142 앱 바 단추 뒤로 Description: Lasal_PRG(으)로 이동 ID: backButton
> 					143 앱 바 단추 (disabled) 앞으로 Description: 앞으로 ID: forwardButton
> 					144 앱 바 단추 "Lasal_PRG"(으)로 이동(Alt+위쪽 화살표) Description: "Lasal_PRG"(으)로 이동(Alt+위쪽 화살표) ID: upButton
> 					145 앱 바 단추 "Elmo_EtherCAT_Test_4Axis" 새로 고침(F5) Description: "Elmo_EtherCAT_Test_4Axis" 새로 고침(F5) ID: refreshButton
> 				146 그룹 ID: PART_AutoSuggestBox
> 					147 편집 주소 표시줄 ID: TextBox
> 				148 분할 단추 내 PC ID: FirstCrumbStackPanel
> 					149 이미지
> 				150 그룹 ID: PART_BreadcrumbBar
> 					151 분할 단추 내 PC
> 						152 텍스트 내 PC
> 					153 분할 단추 Windows-SSD (C:)
> 						154 텍스트 Windows-SSD (C:)
> 					155 분할 단추 work
> 						156 텍스트 work
> 					157 분할 단추 Elmo
> 						158 텍스트 Elmo
> 					159 분할 단추 Elmo_Master
> 						160 텍스트 Elmo_Master
> 					161 분할 단추 Lasal_PRG
> 						162 텍스트 Lasal_PRG
> 					163 분할 단추 Elmo_EtherCAT_Test_4Axis
> 						164 텍스트 Elmo_EtherCAT_Test_4Axis
> 				165 그룹 ID: FileExplorerSearchBox
> 					166 편집 Elmo_EtherCAT_Test_4Axis 검색 ID: TextBox
> 						167 텍스트 Elmo_EtherCAT_Test_4Axis 검색
> 				168 앱 바 ID: FileExplorerCommandBar
> 					169 앱 바 단추 새로 만들기
> 						170 이미지
> 					171 앱 바 단추 (disabled) 잘라내기 Description: 선택한 항목을 클립보드로 이동합니다.
> 						172 이미지
> 					173 앱 바 단추 (disabled) 복사 Description: 선택한 항목을 클립보드에 복사합니다.
> 						174 이미지
> 					175 앱 바 단추 (disabled) 붙여넣기 Description: 클립보드의 내용을 현재 위치로 붙여 넣습니다.
> 						176 이미지
> 					177 앱 바 단추 (disabled) 이름 바꾸기 Description: 선택한 항목의 이름을 바꿉니다.
> 						178 이미지
> 					179 앱 바 단추 (disabled) 공유 Description: 선택한 파일을 공유할 앱을 선택합니다.
> 						180 이미지
> 					181 앱 바 단추 (disabled) 삭제 Description: 선택한 항목을 휴지통으로 이동하거나 완전히 삭제합니다.
> 						182 이미지
> 					183 앱 바 단추 정렬 ID: SortAndGroupButton
> 						184 이미지
> 					185 앱 바 단추 보기
