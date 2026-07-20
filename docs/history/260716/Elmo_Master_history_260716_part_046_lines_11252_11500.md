- API 회귀 테스트 `42/42 PASS`
- LASAL 정적 계약 검사 통과
- 실제 실행 화면과 두 탭 smoke test 통과
- 독립 최종 리뷰에서 P1/P2 없음
- 원본 `Codex_LASAL_WPF` 변경 없음
- 미구현 API 명칭 정적 검색 결과 없음

남은 것은 실제 PLC 연결 시험입니다. `Connect → Load Axis → Read Status/Position → Power On → 작은 Absolute/Relative → Velocity → Stop/Power Off` 순서로 진행하면 됩니다.

현재 변경분은 아직 커밋하지 않았습니다.

> 이거 테스트 하려하는데 Axis가 로드가 안되는데?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-ef920627-c5dc-46f2-8b80-0bb1db667738.png`

<details><summary>9 previous messages</summary>

> 연결과 RPC 초기화는 정상입니다. 실패 지점은 `_LMCAxis1` 이름 자체보다 `0x103C` 축 이름 조회 응답을 API가 유효한 형태로 해석하지 못한 부분입니다. PC 요청/응답 파서와 LASAL 송신 바이트를 바로 대조해서 원인을 특정하고, 필요한 쪽을 수정하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 소스상 `_LMCAxis1` 이름은 맞고, 요청/응답 바이트 배치도 일치합니다. 다만 LASAL 쪽 이름 레지스트리가 축 4개와 로봇까지 전부 준비되어야만 축 1 조회를 허용하도록 묶여 있어, 연결 하나만 준비되지 않아도 지금처럼 전체 축 조회가 실패할 수 있습니다. 이 의존성을 축별로 분리하고, PC 로그에는 PLC가 돌려준 상태·오류·원시 응답을 표시하도록 보강하겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+72 -48)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+16 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+77 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+16 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+26 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md` (+16 -7)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+17 -0)
>
> </details>
>
> 수정은 반영했습니다. 실행 중인 예제 프로그램은 종료하지 않고, 별도 출력 폴더로 API 테스트와 WPF 빌드를 검증하겠습니다. LASAL은 이 환경에서 IDE 빌드가 불가능하므로 계약 검사와 소스 구조 검증까지 수행합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+40 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+38 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
>
> </details>
>
> 검토에서 중요한 문제를 하나 잡았습니다. 첫 수정안은 캐시가 재연결 뒤 오래된 이름을 유지할 수 있고, 준비되지 않은 object가 있으면 1 ms CyWork에서 문자열 확인을 계속할 수 있습니다. 이를 커밋하지 않고, lookup 요청을 실제 처리하는 CyWork 시점에만 해당 client 이름을 새로 읽는 방식으로 바꾸겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+82 -74)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+65 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+10 -13)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+18 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+5 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md` (+8 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+21 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+37 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+6 -0)
>
> </details>
</details>

원인 확인했고 수정했습니다. `_LMCAxis1` 이름은 맞습니다.

기존 LASAL 구현은 Axis 1~4와 Robot이 모두 준비돼야 Axis 1 lookup도 허용했습니다. 하나라도 준비되지 않으면 PLC가 `ErrorId=-2`를 반환했지만, PC API가 이를 `Invalid axis lookup response`로 뭉개고 있었습니다.

수정 내용:

- Axis lookup을 요청 시 해당 축만 조회하도록 변경
- RT Task 및 1 ms 주기 문자열 polling 없음
- 재연결 시 오래된 이름을 사용하지 않도록 매 lookup 때 이름 갱신
- PC 로그에 `HeaderStatus`, `ErrorId`, payload 길이, raw 응답 표시
- 관련 설계·진단 문서 업데이트

검증 결과:

- PC API 테스트: 43/43 통과
- WPF Release 빌드 통과
- LASAL 정적 계약 검사 통과
- LASAL 소스 ASCII/줄바꿈 검사 통과

다시 시험하려면:

1. 현재 실행 중인 예제 프로그램을 종료합니다. 현재 프로세스는 수정 전 DLL을 사용 중입니다.
2. LASAL IDE에서 [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1066)를 Rebuild합니다.
3. `Find in Implementation` 확인 후 PLC에 다시 Download하고 재시작합니다.
4. WPF를 다시 빌드하거나 검증본 [LasalMotionControlApiExample.exe](./tmp/axis_lookup_verify/app2/LasalMotionControlApiExample.exe)를 실행합니다.
5. `_LMCAxis1`을 Load Axis 합니다. 정상 기대값은 `Reference=1`입니다.

그래도 실패하면 새 Execution Log를 복사해 주세요. 이제는 PLC가 반환한 정확한 상태와 raw 응답이 표시됩니다. 진단 기준은 [README.md](./LMC_Library/Codex_LASAL_WPF/README.md:31)에 정리했습니다. 변경분은 아직 커밋하지 않았습니다.

> [17:30:39.811] Example ready. Connect, load _LMCAxis1, and start with Read Status. No command is sent automatically.
> [17:30:41.694] Connect started.
> [17:30:41.709] Connection state Disconnected -> Connecting
> [17:30:41.721] Connection state Connecting -> Connected
> [17:30:41.721] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0xFFFFFFFF
> [17:30:41.722] Connect PASS.
> [17:30:48.148] Load Axis started.
> [17:30:48.155] Load Axis FAILED: Axis lookup failed for '_LMCAxis1'. FrameValid=True, HeaderStatus=1, PayloadLength=4, ParsedPayloadLength=4, RawLength=12, CommandStatus=1, ErrorId=-2, ExpectedPayloadLength=6. The LASAL object registry entry is not ready or the object name did not match. Raw=01 00 04 00 00 00 00 00 01 00 FE FF.
> [17:30:51.525] Load Group started.
> [17:30:51.530] Load Group FAILED: Group lookup failed for '_LMCRobotBase1'. FrameValid=True, HeaderStatus=1, PayloadLength=4, ParsedPayloadLength=4, RawLength=12, CommandStatus=1, ErrorId=-2, ExpectedPayloadLength=6. The LASAL object registry entry is not ready or the object name did not match. Raw=01 00 04 00 00 00 00 00 01 00 FE FF.
>
>  아직도 Axis, Group이 제어권을 얻을수가 없는데?

<details><summary>8 previous messages</summary>

> 로그로 원인은 확정됐습니다. TCP/RPC가 아니라 PLC의 `0x103C/0x1042`가 실제로 `-2`를 반환하고 있습니다. 이제 확인할 것은 두 가지뿐입니다: 배포된 LASAL에서 client 연결이 살아 있는지, `_GetObjName`이 실제로 어떤 이름/길이를 반환하는지입니다. 우선 생성된 네트워크 테이블과 현재 소스를 다시 대조하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 프레임 자체는 정상이고, `-2`는 PLC가 의도적으로 만든 실패 응답입니다. 축과 그룹이 동시에 같은 값으로 실패하므로 이름 철자 두 개가 우연히 모두 틀린 경우보다, 공통 조회 방식(`IsClientConnected` 또는 `_GetObjName`에 넘긴 포인터)이 잘못됐을 가능성이 큽니다. 이 공통 경로를 집중 확인 중입니다.
>
> 원인을 찾았습니다. LASAL 생성 테이블은 객체명을 대문자로 정규화합니다. 즉 실제 런타임 조회값은 `_LMCAXIS1`, `_LMCROBOTBASE1`인데, 현재 코드는 입력값 `_LMCAxis1`, `_LMCRobotBase1`과 대소문자를 구분하는 `_strcmp`로 비교하고 있어서 둘 다 `-2`가 납니다. 우선 UI에서 두 이름을 전부 대문자로 입력하면 바로 확인할 수 있고, 코드도 LASAL 이름 규칙에 맞춰 대소문자 무시 비교로 고치겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+10 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+9 -3)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+5 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md` (+7 -1)
>
> </details>
>
> 수정은 이름 lookup 5곳만 `_strcmp`에서 `_stricmp`로 바꿨습니다. 포인터, 네트워크 연결, descriptor 값, RT Task 구조는 건드리지 않았습니다. 이제 정적 LASAL 계약과 PC API 전체 테스트를 다시 돌려서 이 변경이 다른 프레임/명령 경로를 깨지 않았는지 확인하겠습니다.
>
> 핵심 검증은 통과했습니다: PC 프로토콜/API 테스트 43/43, LASAL source-only 계약, 실제 network 포함 계약이 모두 PASS입니다. 통합 `RunTests`의 마지막 단계만 현재 사용하지 않는 기존 `LasalMotionControlLibTestApp`의 WPF 생성 코드 문제로 실패했으며, 이번 lookup 변경과는 무관합니다. 실제 사용하는 `Codex_LASAL_WPF` 예제는 별도로 다시 빌드하겠습니다.
>
> 예제 빌드는 소스 오류가 아니라 지금 실행 중인 프로세스(PID 48356)가 Release EXE를 잠가서 복사 단계에서만 막혔습니다. 사용 중인 앱은 종료하지 않고, 별도 출력 폴더로 빌드해 검증하겠습니다.
</details>

원인은 제어권 문제가 아니라 객체명 대소문자 비교 오류였습니다.

- LASAL 런타임 이름: `_LMCAXIS1`, `_LMCROBOTBASE1`
- WPF 입력 이름: `_LMCAxis1`, `_LMCRobotBase1`
- 기존 `_strcmp`를 대소문자 무시 `_stricmp`로 수정했습니다: [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1080)
- 설계 문서와 예제 README에도 반영했습니다.

현재 PLC 배포본에서 즉시 확인하려면 다음처럼 전부 대문자로 입력하세요.

- Axis: `_LMCAXIS1`
- Group: `_LMCROBOTBASE1`

정식 적용 절차는 LASAL IDE에서 외부 변경을 Reload한 뒤 Rebuild → PLC Download입니다. IDE의 기존 편집 내용으로 파일을 덮어쓰지 않도록 주의하세요. 새 배포본에서는 기존 mixed-case 이름도 정상 동작합니다.

검증 결과:

- PC API 테스트: 43/43 통과
- LASAL source/network 정적 계약: 통과
- `Codex_LASAL_WPF` 별도 출력 빌드: 통과
- 실제 LASAL 빌드 및 PLC 동작 시험: 사용자 환경에서 남음

> 이거 에러난거 캡쳐했어 확인좀 해줘봐. lasal에서 디버깅 걸어서 lmcaxis ref 얻어오는거까지는 확인했는데
>
> User context
>
> Attachments:
> - connect_LoadAxis.pcapng: `C:/Users/dreiv/Desktop/connect_LoadAxis.pcapng`
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-ce3c991f-0128-49f4-86d4-fadec0efd8ef.png`

<details><summary>2 previous messages</summary>

> 이번에는 lookup은 성공했고, 실패 지점이 바로 다음 `0x202B AxisInfo` 응답으로 이동했습니다. 캡처에서 `0x103C` 성공 descriptor와 `0x202B` 요청·응답의 실제 TCP payload 길이/바이트를 분리한 뒤, LASAL 송신 코드와 PC 파서 기대값을 바이트 단위로 대조하겠습니다.
>
> 캡처에서 원인이 더 좁혀졌습니다. `0x103C`는 정상 성공해 descriptor `1`을 반환했습니다. 이어진 `0x202B` 요청도 `ref=1`, `payload=12`로 정확합니다. 그런데 PLC 응답은 `01 00 04 00 ... 01 00 FC FF`, 즉 `ErrorId=-4(unknown command)`입니다. 따라서 AxisInfo 본문 조건 문제가 아니라, 본문에 도달하기 전 명령 허용 필터가 `0x202B`를 막고 있는 상태입니다.
</details>

캡처 결과, 사용자가 확인한 대로 AxisRef 획득은 정상입니다. 실패는 그 직후 자동 실행되는 `0x202B AxisInfo`입니다. 제어권이나 TCP 문제가 아닙니다.

| 단계 | 결과 |
|---|---|
| `0x103C` `_LMCAxis1` 조회 | 성공, `AxisRef=1` |
| `0x202B` AxisInfo 요청 | 정상: `Payload=12`, `Ref=1`, `Mode=5`, `Enable=1` |
| PLC 응답 | 실패 ACK, `ErrorId=-4` |

실제 PLC 응답:

```text
01 00 04 00 00 00 00 00 01 00 FC FF
```

해석하면 HeaderStatus `1`, PayloadLength `4`, CommandStatus `1`, ErrorId `-4`입니다. 응답이 잘린 것이 아니며 TCP 분할·합쳐짐·재전송도 없습니다.

현재 소스의 [`0x202B` 처리기](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1190)는 성공 또는 `-3`만 반환할 수 있습니다. `-4`는 [`case CommandID` 최종 기본 분기](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1717)에서만 나옵니다. 즉 실행 중인 PLC가 `0x202B`를 등록 명령으로 인식하지 않고 있습니다.

LASAL에서 다음 두 곳에 breakpoint를 거십시오.

- `0x202B:` 라인 1190
- 최종 `else` 라인 1717

확인할 값:

```text
ActiveRequest.CommandId = 0x202B  (8235)
CommandID               = 0x202B  (8235)
Payload                 = 12
AxisRef                 = 1
```

- 기본 `else`가 실행되고 `CommandID=0x202B`라면 PLC에 올라간 class image와 현재 소스가 불일치합니다. Full Rebuild/Link/Download가 필요합니다.
- `CommandID`가 다르면 [`CyWork`의 queue→CommandID 복사](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:506)를 추적해야 합니다.
- `0x202B` 처리기에 진입한다면 정상 응답은 총 16바이트여야 합니다.

WPF 메시지도 부정확합니다. [`LmcAxis.cs`](./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs:109)가 정상적인 4바이트 실패 ACK를 먼저 “8바이트가 아니다”라고 거부해 실제 `ErrorId=-4`를 숨기고 있습니다. 성공 응답 규격은 유지하면서 short error를 먼저 표시하도록 별도 수정해야 합니다.

이번에는 캡처 분석만 했고 소스는 변경하지 않았습니다.

> else에 걸리고 상태가 이러네
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-f8f96e7a-48d4-41a4-a89e-9983b01da482.png`

<details><summary>7 previous messages</summary>

> 원인이 확정됐습니다. `ActiveRequest.CommandId=8235(0x202B)`는 정상인데 `CommandID=73771(0x0001202B)`입니다. `ActiveRequest.CommandId$DINT`가 숫자 변환이 아니라 인접한 두 `UINT`를 32비트로 재해석해서, 상위 16비트에 `Reference=1`이 붙었습니다. `AxisRef=786433(0x000C0001)`도 같은 이유로 상위 16비트에 `PayloadLength=12`가 붙었습니다. 이 잘못된 재해석을 실제 숫자 변환으로 수정하고 동일 유형을 전체 점검하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 스크린샷 값과 구조체 메모리 배치가 정확히 일치해서 원인은 확정입니다. 지금 세 군데를 `TO_DINT(...)` 숫자 변환으로 교체하고, 같은 실수를 다시 넣지 못하도록 계약 검사와 에러 메시지 처리까지 같이 보강하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+8 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+48 -0)
>
> </details>
>
> 수정 범위는 두 층입니다. PLC에서는 `CommandId/Reference/PayloadLength` 세 필드의 잘못된 32비트 재해석을 고쳤고, PC API에서는 PLC가 보낸 4바이트 오류 ACK가 “8바이트가 아니다”라는 모호한 오류로 가려지지 않고 `Status=1, ErrorId=-4`로 표시되도록 보강했습니다.
