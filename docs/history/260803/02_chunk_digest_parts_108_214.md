# Emlo_Master history digest: parts 108-214

## 범위와 판정 원칙

- 대상: Elmo_Master_history_260803_1_part_108부터 part_214까지, 원본 26,751-53,500행, 총 107개 part.
- 아래 내용은 분할 파일의 기록을 소스 행 순서대로 요약한 것이다. 현재 소스, 현재 worktree, 현재 LASAL 프로젝트 또는 PLC 상태를 확인한 결과가 아니다.
- 스크린샷의 base64 본문은 placeholder로 치환된 상태였으며, 남은 accessibility text, 도구 제목, 편집 요약, 빌드·시험 보고만 사용했다.
- PASS는 기록 당시의 PC build, fake-RPC, 정적 계약, IDE build 결과를 뜻한다. 별도 명시가 없는 한 PLC download, 실제 축 구동, 실제 SDO 송신 완료 증거가 아니다.

## Part별 coverage

| Part | 원본 행 | 주제 힌트 |
|---:|---:|---|
| 108 | 26751-27000 | LASAL 도움말 창과 Network 연결 도움말 진입 |
| 109 | 27001-27250 | Network 연결 도움말 검색·탐색 반복 |
| 110 | 27251-27500 | 인쇄 대화상자 종료 후 연결 도움말 재검색 |
| 111 | 27501-27750 | Client-Server 연결 도움말 확인 |
| 112 | 27751-28000 | Network 편집 기본 도움말 확인 |
| 113 | 28001-28250 | 연결 관련 context help 선택 |
| 114 | 28251-28500 | 연결 도움말 본문 로드 확인 |
| 115 | 28501-28750 | LASAL Network 편집기로 복귀 |
| 116 | 28751-29000 | CREVIS Coupler endpoint 복사 절차 탐색 |
| 117 | 29001-29250 | CREVIS 서버 채널과 Network View 연결 메뉴 확인 |
| 118 | 29251-29500 | View 메뉴와 LASAL tree panel 탐색 |
| 119 | 29501-29750 | LASAL file explorer와 network file 탐색 |
| 120 | 29751-30000 | EtherCAT Network 열기와 편집 화면 확인 |
| 121 | 30001-30250 | EtherCAT/Object Network 명령과 context menu 점검 |
| 122 | 30251-30500 | Network editor menu와 연결 명령 점검 |
| 123 | 30501-30750 | 세 ClassState 연결을 만들 작업 착수 |
| 124 | 30751-31000 | Coupler ClassState server endpoint 위치 확인 |
| 125 | 31001-31250 | ClassState channel menu 확인 |
| 126 | 31251-31500 | ClassState value와 server menu 확인 |
| 127 | 31501-31750 | Coupler connection paste 첫 시도 |
| 128 | 31751-32000 | Network 상태와 EtherCAT slot server 확인 |
| 129 | 32001-32250 | InputSlot ClassState 선택 |
| 130 | 32251-32500 | InputSlot endpoint 복사·Motion Network paste |
| 131 | 32501-32750 | InputSlot 연결 적용 후 OutputSlot 탐색 |
| 132 | 32751-33000 | OutputSlot endpoint와 연결 메뉴 탐색 |
| 133 | 33001-33250 | 화면상 연결이 파일에 미기록됨을 발견 |
| 134 | 33251-33500 | Motion_Network 저장과 endpoint 재탐색 |
| 135 | 33501-33750 | Coupler client endpoint menu 확인 |
| 136 | 33751-34000 | Coupler client/server endpoint 복사 준비 |
| 137 | 34001-34250 | Coupler ClassState endpoint 확대·선택 |
| 138 | 34251-34500 | ClassState value cell 정밀 선택 |
| 139 | 34501-34750 | Coupler ClassState connection paste |
| 140 | 34751-35000 | InputSlot ClassState connection paste |
| 141 | 35001-35250 | OutputSlot ClassState connection paste |
| 142 | 35251-35500 | 세 연결 저장, CRLF verifier 수정, 구조 gate 통과 |
| 143 | 35501-35750 | LASAL Rebuild 진행·결과 확인 |
| 144 | 35751-36000 | Rebuild 0 error/32 warning 기록과 Link 실행 |
| 145 | 36001-36250 | LMCEcatInputLatch 구현 화면 열기 |
| 146 | 36251-36500 | 구현 검색과 Class View 이동 |
| 147 | 36501-36750 | Class tree 정리와 smoke 대상 탐색 |
| 148 | 36751-37000 | 구조 반영·Rebuild/Link 후 smoke 준비 |
| 149 | 37001-37250 | LASAL implementation smoke 실행 |
| 150 | 37251-37500 | smoke 명령 재탐색과 Motion Network 복귀 |
| 151 | 37501-37750 | Find in Implementation 경로 확인 |
| 152 | 37751-38000 | smoke 성공, 신규 CInvalidArgException 0 |
| 153 | 38001-38250 | library 제거 거부 후 IDE 정상 종료 |
| 154 | 38251-38500 | 464-byte snapshot·CREVIS read 경로 외부 구현 |
| 155 | 38501-38750 | LASAL IDE 재실행 |
| 156 | 38751-39000 | ClassState UDINT 대 enum 비교 오류 8곳 발견 |
| 157 | 39001-39250 | IDE 종료 후 TO_UDINT 수정 준비 |
| 158 | 39251-39500 | 앱 상태 확인 위주의 전환 구간 |
| 159 | 39501-39750 | LASAL 프로젝트 재열기 |
| 160 | 39751-40000 | 캐스팅 수정 후 Rebuild |
| 161 | 40001-40250 | Link와 LMCEcatInputLatch client 목록 확인 |
| 162 | 40251-40500 | 구현 참조와 Coupler client 검색 |
| 163 | 40501-40750 | LMCEcatInputLatch 구현부 직접 열기 |
| 164 | 40751-41000 | Class View UI 복구·재정렬 |
| 165 | 41001-41250 | smoke를 위한 IDE 재시작 |
| 166 | 41251-41500 | 프로젝트 경로 교정과 method 목록 확인 |
| 167 | 41501-41750 | CopyTopologyIoSnapshot·Coupler client smoke |
| 168 | 41751-42000 | library 유지 후 IDE 안전 종료 |
| 169 | 42001-42250 | Rebuild/Link/smoke 완료와 verifier Count 버그 수정 |
| 170 | 42251-42500 | Motion/Power/Axis1 SDO와 17-frame qualifier 정리 |
| 171 | 42501-42750 | SDO identity pinning과 transactional distribution 구현 |
| 172 | 42751-43000 | provenance candidate 성공 후 Group Reset 안정 확인 착수 |
| 173 | 43001-43250 | Group Reset no-replay·durable recovery와 stale retirement |
| 174 | 43251-43500 | Single Axis live qualification runner 구현 |
| 175 | 43501-43750 | whole-sequence journal, 영한 언어팩, callback 귀속 |
| 176 | 43751-44000 | dormant SetAxisPosition/Reference 계약과 회귀 |
| 177 | 44001-44250 | 언어팩 보강, fake-RPC race 수정, retained outcome 설계 |
| 178 | 44251-44500 | retained outcome 작업 중 Home/DS402/TW reset 요구 추가 |
| 179 | 44501-44750 | Computer Use 안전 절차와 앱 제어 초기화 |
| 180 | 44751-45000 | LASAL online read-only 상태 확인 |
| 181 | 45001-45250 | SetPosition 1053 tests와 DS402 Home 소유권 설계 |
| 182 | 45251-45500 | Windows 제어 API와 LASAL window 상태 확인 |
| 183 | 45501-45750 | Home 세 경로 병합과 IDE 구조 확인 |
| 184 | 45751-46000 | LASAL offline 전환 |
| 185 | 46001-46250 | LMCControlCommandService class 탐색 |
| 186 | 46251-46500 | class member/client menu 탐색 중 ESC 중단 |
| 187 | 46501-46750 | 중단·체크리스트·TW20 정정·IDE 제어권 재인계 |
| 188 | 46751-47000 | 앱 inventory 확인, LASAL 미실행·Wireshark capture 열림 |
| 189 | 47001-47250 | LASAL Class 2 실행 |
| 190 | 47251-47500 | tracked 프로젝트 열기 대화상자 조작 |
| 191 | 47501-47750 | 프로젝트 파일 선택과 로드 확인 |
| 192 | 47751-48000 | parameterless method 두 개뿐인 불완전 선언 확인 |
| 193 | 48001-48250 | Windows control API 재확인 |
| 194 | 48251-48500 | LMCEcatInputLatch declaration menu 열기 |
| 195 | 48501-48750 | 첫 DS402 Home variable 추가 UI 진입 |
| 196 | 48751-49000 | Ds402HomeRequestSequence 이름 입력 |
| 197 | 49001-49250 | 첫 variable type editor 탐색 |
| 198 | 49251-49500 | 첫 variable type을 UDINT로 교정 |
| 199 | 49501-49750 | IDE 상태·제어 API·선언 세션 재확인 |
| 200 | 49751-50000 | class 선언 저장과 build output 확인 |
| 201 | 50001-50250 | build message와 class add menu 확인 |
| 202 | 50251-50500 | 두 번째 mailbox variable 추가 |
| 203 | 50501-50750 | Ds402HomeAppliedSequence 이름 입력 |
| 204 | 50751-51000 | 두 번째 variable type 입력 시도 |
| 205 | 51001-51250 | UDINT 선택을 반복 확인 |
| 206 | 51251-51500 | UDINT 확정 후 기존 method tree 확인 |
| 207 | 51501-51750 | SubmitDs402HomeControl 입력 추가 시작 |
| 208 | 51751-52000 | OperationToken 입력·UDINT 지정, PC tests 보고 |
| 209 | 52001-52250 | AxisReference 입력 추가·이름 확정 |
| 210 | 52251-52500 | Command 입력 추가와 제어 API 오류 우회 |
| 211 | 52501-52750 | Command 확정 후 Result output 추가 |
| 212 | 52751-53000 | 다음 method의 OperationToken 입력·type 지정 |
| 213 | 53001-53250 | 다음 Result output과 alignment method 생성 준비 |
| 214 | 53251-53500 | SubmitDs402HomeSetpointAlignment 생성 재시도 중 끝남 |

## 시간순 phase digest

### 1. LASAL 연결 방법 조사와 CREVIS Network 구조 반영: parts 108-142

- LASAL help와 Network editor를 오가며 server/client endpoint의 Copy/Paste Connection 절차를 확인했다.
- CREVIS Coupler, InputSlot, OutputSlot의 ClassState를 Motion Network 쪽 LMCEcatInputLatch client에 잇는 세 연결을 시도했다.
- 처음에는 UI에 표시만 되고 Motion_Network.lcn에 기록되지 않아 실패로 판정했다. endpoint를 다시 선택해 paste하고 저장한 뒤 세 연결이 파일에 기록됐다고 보고했다.
- IDE가 CRLF로 저장한 ST source를 verifier가 함수 종료로 인식하지 못하는 결함을 고쳐 LF/CRLF 양쪽을 허용했다. 이 시점에는 새 handler와 관련 capability/opcode를 dormant 상태로 유지했다.

### 2. CREVIS integrated read owner 구현과 LASAL build/smoke: parts 143-169

- 최초 구조 Rebuild는 0 error, 32 warning으로 기록됐고 Link와 Find in Implementation smoke를 수행했다. smoke 시점 이후 Lasal2.log의 신규 CInvalidArgException은 0이었다.
- IDE 종료 후 LMCEcatInputLatch의 464-byte coherent snapshot, CREVIS 입력/출력 quality 계산, LMCDiagnosticsService의 읽기 handler, TCPMotionInterface route를 외부 source에 반영했다고 기록했다.
- 정적 검증은 IntegratedReadOwnerDormant로 통과했지만 실제 IDE Rebuild에서 ClassState의 UDINT 값과 t_e_VaranErrors enum을 직접 비교한 8곳이 실패했다. TO_UDINT 변환으로 고치고 verifier에도 같은 문법을 강제했다.
- 재실행 뒤 Rebuild 0 error, 20 version warning, Link 완료, 변경 class 직접-open smoke, CInvalidArgException 0을 기록했다.
- 후속 기본 checkpoint의 119/261 typed-write 실패는 source 누락이 아니라 PowerShell hashtable의 Count key 대신 객체 Count 속성을 읽은 verifier 버그였다. 수정 뒤 SourceOnly/full 모두 PASS했다. WPF Debug 227/227 PASS가 기록됐고 Release는 해당 시점에 진행 중이었다.

### 3. Motion/Power/SDO, 배포, Reset, live runner, 언어팩: parts 170-177

- Motion/Power/Stop/Reset과 Axis1 SDO Write의 WPF-SDK-TCP-LASAL 경로를 연결했다고 보고했다. SDO Write는 Axis1 0x2F00:24, Int32, 4 byte 한 건만 allowlist에 두고 0x7E50 제출 전 same-value qualification, arm, second confirmation, identity 재확인을 요구했다.
- stored BootId와 current BootId가 다른 recovery quarantine을 자동 우회하지 않고 Archive and Retire Stale Recovery로 증거를 보존·폐기한 뒤 앱 재시작·재연결하게 했다.
- topology qualifier를 live scope 필수, exact 17 frame dry-run, zero network, binary/source hash·MapRevision provenance를 남기는 fail-closed 도구로 보강했다.
- SDO proof가 identity A-B-A 또는 disconnect 뒤 살아나는 문제를 막고, SDK 송신 직전 session/build/BootId/MapRevision/target을 다시 고정했다.
- canonical distribution을 직접 덮지 않는 candidate transaction, semantic preflight, rollback, snapshot/hash/commit/worktree provenance를 구현했다. 최종 candidate 기록은 SDK 976/976, WPF 235/235, manifest 56/56, semantic 15/15, transaction 86/86이며 canonical hash가 유지됐다고 한다.
- Group Reset은 0x2049를 한 번만 보내고 0x2045와 모든 pinned member의 0x2028을 3회 연속 확인하게 했다. reconnect/restart에서는 reset을 재전송하지 않고 fresh 0x20D2 member snapshot 뒤 status-only continuation을 수행하는 durable recovery를 추가했다.
- Single Axis runner는 Power On, fresh ready, Relative Move, 실제 이동과 standstill/position 안정, Stop, Power Off 순서로 묶고 각 mutation의 one-shot/no-replay를 유지했다.
- whole-sequence journal로 Power On과 Move journal 사이 crash gap을 다뤘고, self-journal이 자기 Power/Move를 막는 결함과 BootId 0 처리 문제를 수정했다고 기록했다.
- English/한국어 즉시 전환, 선택 저장, 입력값·raw log·wire payload 보존을 구현했다. 이후 binding 손상, 동적 버튼의 영어 복귀, reverse translation 문제를 고쳐 WPF 297/297까지 올렸다.
- fake-RPC의 mutable List 동시 열거로 발생한 Collection was modified race를 immutable snapshot 반환으로 고쳐 SDK 1042/1042, WPF 297/297 반복 PASS를 기록했다.

### 4. dormant SetPosition에서 Home/DS402/TW 기능으로 전환: parts 178-187

- SetPosition retained outcome 계약은 56-byte dormant parser와 WPF durable journal까지 진행됐지만 capability bit 3/5, native SetPosition, WPF 노출은 계속 OFF로 기록됐다.
- 사용자가 MMC_Home은 MoveReference, DS402 Home은 DS402 mode 6, 전원 재투입 뒤 test-only multiturn reset을 별도 기능으로 구현해 달라고 요청했다.
- 구현 방향은 MMC Home, DS402 Home, TEST reset을 분리했다. DS402 Home은 일반 SDO로 0x6040을 쓰면 PDO가 다음 cycle에 덮으므로 LASAL 내부 단일 state machine이 ControlWord bit 4와 mode 복원을 소유해야 한다고 판단했다.
- 0x6098, 0x6060, 0x6061의 1-byte access 때문에 LMCSdoExecutor를 1/2/4-byte read/write로 확장했다고 기록했다.
- 0x7D15 start와 0x7D16 outcome parser를 HandleRequest에 직접 넣으면 32,768-byte 검증 한도를 넘어서므로 LMCDiagnosticsService private helper 두 개가 필수라고 판정했다.
- setpoint 재정렬을 위해 LMCEcatInputLatch에 LMCAxis1..4 : CltChCmd__LMCAxis와 Motion Network의 각 _LMCAxis.Control 연결이 필요하다고 정정했다. 새 server 또는 Diagnostics-to-Drive 직접 client는 금지했다.
- TW 명령의 의미를 재검증해 TW[20] / 0x3204:14를 multiturn position reset이 아닌 Encoder Error/Warning Reset으로 정정했다. 실제 multiturn position 초기화인 TW[19] / 0x3204:13은 위험 기능으로 계속 금지했다.
- 기록상 SDK 1065/1065, WPF Debug 316/316이 있었지만 PLC capability는 OFF이고 IDE 구조 전에는 Home이 fail-closed였다.
- 사용자가 직접 IDE를 편집하겠다고 하자 작업을 멈췄고, 이후 일정 때문에 다시 에이전트에게 전체 IDE 제어권과 구현 계속 권한을 주었다.

### 5. IDE 재인계 후 DS402 Home 선언 편집: parts 188-214

- LASAL Class 2를 실행해 tracked Elmo_EtherCAT_Test_4Axis 프로젝트를 열었다. 이전 사용자 편집은 LMCEcatInputLatch에 인자 없는 method 두 개만 만든 불완전 상태였다고 기록했다.
- LMCEcatInputLatch에 Ds402HomeRequestSequence와 Ds402HomeAppliedSequence를 만들고 UDINT type으로 맞추는 UI 작업을 진행했다.
- SubmitDs402HomeControl에 OperationToken, AxisReference, Command 입력과 Result 출력을 추가하는 작업, 다음 상태 조회 method에 OperationToken과 Result를 추가하는 작업이 기록됐다.
- C# SDK/WPF는 이 구간에서 Debug/Release 각각 SDK 1066/1066, WPF 317/317 PASS로 보고됐다.
- LASAL output의 1개 오류는 새 Home 코드가 아니라 설치 MotionLib가 참조한 DriveComL2.h 누락이라고 기록했다.
- SubmitDs402HomeSetpointAlignment를 만들려 했으나 첫 존재 확인은 false였다. class menu에서 다시 생성하고 이름을 입력한 직후 part_214가 끝나므로 생성 확정, signature, save, Rebuild, Link는 이 범위에 없다.

## 명시적으로 보이는 사용자 요청

1. part 178: 매 시험마다 Home이 필요하므로 MMC_Home은 MoveReference로, DS402 Home은 DS402 방식으로 구현하고, 전원 재투입 뒤 사용하는 test-only multiturn reset을 SDO 기능으로 따로 구현해 반영할 것.
2. part 187: 프로젝트를 종료했으니 IDE 전용 Client, Server, method 추가가 필요하면 정확히 알려 달라는 요청.
3. part 187: 사용자가 LASAL IDE를 직접 열어 항목을 추가하는 동안 편집을 멈추라는 요청.
4. part 187: 일정 변경 후 에이전트가 IDE 제어권을 가져가 필요한 편집과 구현을 계속해도 된다는 요청.

parts 108-177에는 다수의 assistant status/final과 도구 로그가 있으나, 이 범위 자체에서 별도의 사용자 문장이 명확히 드러나지 않는 구간도 있다. 위 목록은 실제 문장으로 식별된 요청만 적었다.

## 기록된 code·문서·IDE action

- LASAL Network: Motion_Network.lcn에 CREVIS Coupler/InputSlot/OutputSlot ClassState 연결 3개를 저장했다고 기록했다.
- LASAL source: LMCEcatInputLatch.st, LMCDiagnosticsService.st, TCPMotionInterface.st에 integrated read owner와 route를 반영했다고 기록했다.
- LASAL verifier: Verify-LasalContract.ps1의 CRLF 함수 인식, enum cast 강제, typed-write Count 계산을 수정했다.
- PC/SDK/WPF: TopologyIoQualificationTool, SDO current-session proof, Group Reset recovery, Single Axis qualification, sequence journals, UiLocalization, callback peer/session ownership, dormant SetPosition/Reference contracts, fake-RPC snapshot race를 순차 반영했다고 기록했다.
- Distribution: Build-LmcApiDistribution.ps1, DistributionPipeline.ps1, test/manifest/semantic policy와 provenance candidate 문서를 갱신했다고 기록했다.
- Home 준비: LMCSdoExecutor.st를 1/2/4-byte access로 확장하고 LMCDiagnosticsService.st와 AXIS_HOME_AND_TEST_MULTITURN_RESET_IMPLEMENTATION_2026-07-31.md를 수정했다고 기록했다.
- IDE action: Rebuild/Link/smoke를 read-owner 단계에서 완료했지만, 새 DS402 Home 단계에서는 LMCEcatInputLatch variable/method declaration 편집 도중 기록이 끝났다.
- stage, commit, push 또는 PLC download를 했다는 기록은 없다.

## Tests와 evidence

- CREVIS/read-owner: SourceOnly/full IntegratedReadOwnerDormant PASS, IDE Rebuild 0 error/20 version warning, Link 완료, implementation smoke 성공, 신규 CInvalidArgException 0.
- Motion/SDO checkpoint: SDK 975/975 또는 후속 976/976, WPF 227/227에서 235/235, exact 17/17 dry-run, network I/O 0.
- Distribution final candidate: manifest 56/56, semantic candidate 15/15, transaction 86/86, canonical hash unchanged, stage/lock 0, manual byte-identical.
- Group Reset: SDK 998/998과 후속 1006/1006, WPF 251/251과 후속 270/270.
- Single Axis: focused 8/8, WPF 278/278, SDK 1006/1006.
- Localization/callback: WPF 294/294에서 297/297, SDK 1007/1007 또는 후속 1042/1042.
- SetPosition/Home 준비: SDK 1053/1053, 후속 SDK 1065/1065 및 1066/1066, WPF 316/316 및 317/317이 기록됐다.
- 위 수치는 서로 다른 시점의 evolving snapshots다. 최신 현재 수치로 합치거나 PLC runtime PASS로 해석하면 안 된다.

## 실패·수정 기록

- 세 Network 연결이 처음에는 UI에만 보이고 파일에 저장되지 않았다. endpoint 재선택·paste·save 뒤 해결했다고 기록했다.
- verifier가 CRLF를 처리하지 못해 함수 종료를 놓쳤다. LF/CRLF regex로 수정했다.
- LASAL Rebuild가 UDINT와 t_e_VaranErrors 직접 비교 8곳을 거부했다. TO_UDINT로 수정했다.
- typed-write 119/261 실패는 hashtable Count key를 잘못 읽은 verifier 버그였다.
- WPF smoke를 병렬 실행하면 single-instance/recovery child process가 서로 기다릴 수 있어 결과를 폐기하고 직렬 재실행했다.
- distribution 작업에서 transient WPF smoke 실패, PowerShell 5.1 Python quoting SyntaxError, manual scope 차단, FileInfo/DirectoryInfo parent 차이, preview 문구, TOCTOU/provenance race가 순차 발견돼 수정됐다.
- localization에서 duplicate Title, binding 파괴 위험, SDO 편집 후 영어 복귀, 역방향 번역 문제가 발견돼 수정됐다.
- fake-RPC mutable List 열거가 Collection was modified를 일으켜 snapshot으로 고쳤다.
- Computer Use에서 stale element, 잘못된 API, 좌표 범위 오류가 반복됐고 한 차례 사용자의 물리 Esc로 즉시 중단됐다.
- 새 DS402 IDE 단계에서 DriveComL2.h 누락 오류 1개가 관측됐으며 이 범위에서 해결되지 않았다.

## 이 범위 끝의 미완료 항목

1. part_214 끝은 SubmitDs402HomeSetpointAlignment 이름 입력 직후다. method 생성·signature·save 여부부터 재확인이 필요하다.
2. checklist의 LMCControlCommandService 변수와 ProcessAxisReference, LMCDiagnosticsService의 HandleAxisDs402HomeStart, HandleAxisDs402HomeOutcome, ProcessAxisDs402Home 및 state/record 선언 완료 증거가 이 범위에 없다.
3. LMCEcatInputLatch의 LMCAxis1..4 client와 Motion Network의 _LMCAxis1..4.Control 연결 완료 증거가 없다.
4. 새 Home 선언을 반영한 generated diff, external implementation, SourceOnly/full verifier, IDE Rebuild/Link, Find in Implementation smoke와 Lasal2.log 확인이 남아 있다.
5. capability bit 4/6/18은 계속 OFF로 두도록 기록됐다. IDE 구조와 전체 검증 전 활성화하면 안 된다.
6. MMC Home은 RefSwitch/ZImpulse 등 실제 reference 입력 연결과 recipe/timeout/travel 조건 확인이 필요하다.
7. DS402 Home은 mode 6, 0x6098/0x6060/0x6061, ControlWord bit 4, statusword outcome, CSP 복귀 setpoint alignment를 한 owner가 수행하는 runtime proof가 없다.
8. TW[20]은 Encoder Error/Warning Reset이다. 사용자가 실제 multiturn position reset인 TW[19]을 원하는지, 정확한 encoder/protocol/EAS socket과 안전 조건이 확인되지 않았다.
9. DriveComL2.h 누락 dependency 오류를 먼저 재현·분류해야 한다.
10. PLC cold download, live capability, BootId/MapRevision, 실제 Home/Move/SDO packet capture, completion polling/readback, drive warning과 실축 안전 검증은 전부 미수행으로 남아 있다.
11. 현재 작업을 재개할 때는 이 digest를 live truth로 쓰지 말고 git status, tracked source, generated files, LASAL IDE project, test binaries와 PLC target을 다시 확인해야 한다.
