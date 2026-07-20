> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+9 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md` (+44 -24)
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+21 -11)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+32 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md` (+8 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+2 -18)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+7 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+25 -7)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+13 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+56 -15)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md` (+14 -3)
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/API_LIST.md` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md` (+1 -0)
>
> </details>
>
> 리뷰 지적도 반영했습니다. RPC 등록을 거치지 않거나 owner가 아닌 socket의 명령은 이제 오류 응답으로 막고, PC 주소는 구체적인 IPv4만 허용합니다. 배포 샘플도 PowerOn ACK를 “구동 준비 완료”로 오해하지 않도록 실제 motion 호출을 분리하고 모든 응답을 검사하게 바꿨습니다. 마지막 빌드·샘플·문서 정합성 검사를 다시 돌리겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md` (+8 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md` (+12 -0)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+6 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
>
> </details>
</details>

반영했습니다. UNIT 정책과 RPC 초기화는 1단계 구현까지 진행했습니다.

### UNIT 정책

설계 기준은 다음으로 확정했습니다.

- PC 호출자가 `물리값 × 승인된 UNIT`을 계산해 `int`로 API 호출
- DLL은 받은 `int`를 그대로 DINT 직렬화
- PLC도 재변환 없이 `_LMCAxis`/`_LMCRobot`에 전달
- read 결과는 호출자가 같은 UNIT으로 나눠 표시
- PMAS `8,388,608 count/rev` 자동 변환은 사용하지 않음

현재 a01~a04는 실제 Motion Network 설정상 Position/Speed/Accel/Decel 모두 `DEG=10000` profile입니다. `RPM`은 `_LMCAxis` 속도 UNIT이 아닙니다. Nonzero jerk는 물리 변환식이 아직 확정되지 않아 배포 예제에서는 `0`으로 제한했습니다.

- [UNIT 변환 배포 매뉴얼](./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md)
- [API 구조 설계](./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md)

### RPC 초기화와 Callback

구현한 순서는 다음과 같습니다.

```text
TCP 연결
→ 0x8080 Session Init
→ PC UDP listener 개방
→ 0x405C로 event mask, 실제 UDP port, PC IPv4 등록
→ 명령 처리
→ 0x405D ACK 후 종료
```

반영 내용:

- callback port `0` 사용 시 실제 할당된 UDP port 등록
- 구체적인 IPv4 주소만 허용
- 4바이트 ACK의 Status/ErrorId 처리
- LASAL `0x8080`, `0x405C`, `0x405D` handler 추가
- TCP 분할·합쳐진 frame을 처리하는 누적 버퍼 추가
- callback 등록을 마친 owner socket만 motion/read 명령 허용
- non-owner 명령은 오류 응답

실제 UDP callback event 송신은 아직 구현하지 않았습니다. 등록 endpoint까지만 완료됐으며, event payload 캡처가 없어서 임의 형식을 만들지 않았습니다.

또한 LASAL 상태 변수는 CodeGenerator declaration 영역에 있으므로 LASAL IDE class model에 등록하고 재생성해야 영구 반영됩니다. 현재는 소스 구현 완료, LASAL IDE build·PLC 검증 대기 상태입니다.

- [RPC 구현 및 검증 기록](./LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md)
- [LASAL 수신 구현](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:440)

### 3~6번 간단 설명

3. Target dispatcher
`a01`, `a02` 같은 이름으로 얻은 reference를 실제 `_LMCAxis1`, `_LMCAxis2`에 연결하는 부분입니다. 현재 Network는 `_LMCAxis1`만 연결돼 있어 축별 분기가 필요합니다.

4. Response parser
짧은 ACK, 값 응답, 구조체 응답을 구분하는 코드입니다. 4/8바이트 ACK는 수정했지만 `0x20D2`의 1350바이트 그룹 멤버 응답 parser는 아직 남았습니다.

5. WPF 안전 수정
현재 WPF 앱은 모든 값에 `8388608`을 곱하고, ACK 성공과 실제 Power Ready를 혼동할 수 있습니다. 축별 UNIT 적용, `IsSuccess` 검사, Power Ready 대기가 필요합니다. 배포 샘플은 자동으로 PowerOn/Move하지 않도록 수정했습니다.

6. 자동 테스트와 PLC 캡처
C# packet byte와 parser는 가짜 서버로 검사하고, 마지막에는 실제 PLC Wireshark 캡처로 동일한지 확인하는 단계입니다. 이번에 RPC 성공/실패 가짜 서버 검증은 통과했습니다. 전체 motion command 테스트와 PLC 재캡처는 남았습니다.

검증 결과는 라이브러리 Release 빌드, WPF Debug 빌드, 배포 샘플 컴파일, `git diff --check` 모두 통과했습니다. 이번 변경은 아직 커밋하지 않았습니다.

> 진행한거 정리해서 커밋해줘

<details><summary>2 previous messages</summary>

> 이번 API 분석·UNIT 정책·RPC 1단계 구현에 해당하는 파일만 골라 커밋하겠습니다. 기존 패키지 정리, 바이너리 삭제, 히스토리 수정, `_Edit` 폴더 등 사용자가 이미 작업 중이던 변경은 staging에서 제외하겠습니다.
>
> 17개 파일만 staging했고, 기존 사용자 변경은 모두 unstaged로 남아 있습니다. Staged diff는 `git diff --cached --check`를 통과했습니다. 이제 RPC lifecycle·UNIT 배포 정책·API 백로그를 한 커밋으로 기록하겠습니다.
</details>

커밋 완료했습니다.

- 커밋: `996686d Implement RPC lifecycle and document DINT API`
- 변경: 17개 파일, UNIT 매뉴얼·API 백로그·RPC 구현 문서 포함
- `git diff --cached --check` 통과
- 기존 사용자 변경은 모두 커밋에서 제외했습니다.
- 원격 push는 하지 않았습니다.

> 3~6번에 대해 대답해줄께 설계문서에 내용 반영해서 적용해줘
> 3. _LMCAxis1, _LMCAxis2는 lasal 프로그램에서 적용되야됨이 맞다고 봄. 이 내용은 lasal프로그램이 실제 object에 대한 이름을 알고 있으니 api상에서는 몰라. 그래서 실제 lasal에서 이름 뒤져서 찾고, 찾은걸로 dispatcher 하는거고 api는 테이블만 가지고 있던가 아니면 Fd처럼 쓰는 방식으로 하자.
> 4. 요건 기능 추가해서 반영할 수 있게 만들어줘
> 5. wpf 안전. 요건 예제프로그램은 더미로 8388608을 곱했는데 이건 사실 23bit 엔코더 값이고 이 내용은 미리 plc에 등록 돼있는거야. 그래서 저건 더미 프로그램이라 그렇게 한거고 실제로 정식적인 API를 만들었을때 더미프로그램을 수정하거나 실제 제대로 동작되는 프로그램을 만드는거지. 그래서 유닛연산은 미리 할 필요없이 유저가 설정하고 유저가 하는게 맞다고 봄.
> 6. 요건 구현한거 테스트하는건가? 맞다면 그대로 테스트가 정상적으로 완료될 수 있게 만들어줘.
>
> 구현내용을 다시 반영시켜서 진행해보도록하자

<details><summary>27 previous messages</summary>

> 결정사항을 이렇게 적용하겠습니다. 3번은 API가 축 이름이나 LASAL object를 알지 않고, LASAL이 이름을 실제 object로 해석한 뒤 opaque reference/FD를 반환하는 구조로 고정합니다. 4번은 command별 typed response parser를 추가하고, 5번은 `8388608`을 “23-bit encoder 더미 profile”로 명확히 격리하며, 6번은 golden-byte·parser·fake-server 테스트를 저장소에 실제 실행 가능한 프로젝트로 만들겠습니다. 먼저 LASAL에서 object 이름을 안전하게 찾는 기존 메커니즘이 있는지 확인하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 코드 기준을 다시 확인한 뒤, 병렬 조사 결과를 합쳐서 LASAL 디스패처·응답 파서·더미 단위 표기·자동 테스트를 한 번에 반영하겠습니다. 기존 사용자 변경분은 건드리지 않고 이번 범위만 별도 커밋 대상으로 관리하겠습니다.
>
> LASAL 쪽은 단순히 이름 문자열을 PC DLL에 넣는 방식이 아니라, PLC 내부에서 실제 객체명→opaque reference를 찾고 그 reference로 4개 축 client channel을 선택하는 구조로 잡고 있습니다. 현재 canonical 프로젝트는 TCP 객체가 `_LMCAxis1` 하나에만 연결돼 있어, 클래스 채널과 `.lcn` 연결도 함께 바꿔야 실제 2~4축 dispatch가 됩니다.
>
> 응답 파서는 기존 메서드 호환을 유지하면서 typed 결과를 추가하고, 정상값 `0`과 파싱 실패를 분리하는 방향으로 구현 중입니다. 동시에 WPF는 `8388608`을 제거하지 않고 “23-bit encoder 더미 프로파일”로 명확히 표시하며, 읽기 실패를 `0`으로 오판하던 안전 결함은 별도로 막겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml` (+10 -10)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs` (+86 -53)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+18 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/Motion_Network.lcn` (+10 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+28 -3)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+203 -9)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+244 -60)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+123 -31)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+4 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+15 -26)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RequestGoldenTests.cs` (+4 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+3 -3)
>
> </details>
>
> LASAL canonical 프로젝트에는 `_LMCAxis1..4` client 연결, 실제 객체명 기반 lookup, 1~4 opaque descriptor, descriptor별 Power/Stop/Reset/Read/Move dispatch를 반영했습니다. Group도 실제 `_LMCRobotBase1` 이름 lookup과 `0x20D2` 멤버 응답까지 연결했으며, 안전한 대응 함수가 확정되지 않은 GroupReset/GroupStop은 성공으로 위장하지 않고 명시적 오류를 반환하도록 했습니다.
>
> <details><summary>Created 2 files, edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+16 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+34 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+41 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md` (+87 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RESPONSE_MODEL_DESIGN_2026-07-09.md` (+17 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md` (+8 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+67 -56)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+23 -5)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청하신 3~6번 기준으로 실제 이름 기반 LASAL 디스패처, 응답 파서, WPF 더미 스케일 정책, 자동 테스트까지 반영된 상태를 다시 점검하겠습니다. 지금은 남은 문서 표현을 정리하고, C#/WPF 빌드·테스트와 LASAL 정적 교차검증을 끝내는 단계입니다.
>
> 설계의 핵심 계약은 맞게 들어가 있습니다. API는 이름을 80바이트 lookup으로만 보내고, LASAL이 연결된 객체의 실제 이름을 읽어 1~4/0x0100의 불투명 참조값으로 바꿉니다. 지금 발견된 잔여 `a01/v01` 표기는 배포 샘플과 세션 문서에서 실제 LASAL 이름으로 정리하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RequestGoldenTests.cs` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+5 -5)
>
> </details>
>
> C# 쪽은 응답 크기를 명령별로 엄격하게 검사하도록 되어 있습니다. 일반 ACK는 정확히 4/8바이트만 허용하고, 상태 12바이트·DINT 위치 8바이트·그룹 멤버 1350바이트를 각각 별도 타입으로 해석합니다. 이제 수정된 그룹 상태 요청의 descriptor 포함 계약까지 포함해 Debug/Release 테스트와 WPF 빌드를 돌리겠습니다.
>
> 정적 교차검증에서 안전 관련 한 가지를 추가로 잡았습니다. WPF의 기존 `0x80/0x200` 상태 마스크는 PMAS 캡처 값이고, 지금 LASAL 서버가 반환하는 `_LMCAXIS_STATUS` 비트와 다릅니다. 그대로 두면 축 상태를 잘못 판정할 수 있으므로 LASAL의 `PowerOn(bit 0)`과 `Standstill(bit 25)` 기준으로 교정합니다.
>
> <details><summary>Edited 12 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -3)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs` (+14 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+9 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RESPONSE_MODEL_DESIGN_2026-07-09.md` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcResults.cs` (+13 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs` (+15 -24)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs` (+7 -6)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs` (+20 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RESPONSE_MODEL_DESIGN_2026-07-09.md` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+12 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+12 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+32 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RequestGoldenTests.cs` (+7 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+5 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md` (+4 -0)
>
> </details>
>
> 최종 정적 리뷰에서 LASAL 생성 테이블의 실제 결함을 하나 발견했습니다. client 항목은 6개로 늘었는데 class table header가 기존 3개로 남아 있어 축 2~4 client가 linker에 등록되지 않을 수 있습니다. 이 값은 즉시 6으로 맞추고 테이블 개수 검증도 추가하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md` (+1 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+1 -1)
>
> </details>
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+28 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+44 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+23 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+4 -11)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+1 -1)
>
> </details>
>
> 리뷰 결과 중 실제 false-success 가능성이 있는 두 항목은 바로 막겠습니다. 32비트 LASAL 축 오류의 상위 비트가 16비트 ACK에서 잘리는 경우를 일반 오류로 매핑하고, 정식 DINT API에 없는 구형 `0x2081~0x2084` 경로는 삭제해 `-4 unsupported`로 떨어지게 하겠습니다. Direction/velocity-decel도 “받고 무시”하지 않도록 지원 범위를 검증하는 방식으로 고정합니다.
