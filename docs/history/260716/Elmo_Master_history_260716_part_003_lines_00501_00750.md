>
> <details><summary>Edited 10 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+28 -34)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+55 -13)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+55 -14)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RequestGoldenTests.cs` (+31 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+6 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+13 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md` (+5 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+8 -28)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RESPONSE_MODEL_DESIGN_2026-07-09.md` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+95 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+26 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+0 -13)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+37 -20)
>
> </details>
>
> C# 테스트는 29개로 늘어 모두 통과했습니다. 이번에 발견된 LASAL client count 같은 오류가 다시 숨어들지 않도록, 동일 `RunTests` 대상에서 generated table 개수·4축 링크·구형 명령 차단·상위 오류 비트 매핑·C#/ST 주요 offset까지 확인하는 정적 계약 검사도 함께 실행되도록 추가하겠습니다.
>
> <details><summary>Created a file, edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+5 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+64 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RESPONSE_MODEL_DESIGN_2026-07-09.md` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+13 -11)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+43 -0)
>
> </details>
>
> 기존 `docs/history/260710/99_analysis_summary.md`는 이번 구현 전 상태라 “C#과 LASAL이 아직 호환되지 않는다”는 내용이 남아 있습니다. 다음 작업자가 잘못된 지점에서 시작하지 않도록, 문서 맨 앞에 이번 구현 결과와 실제 남은 P0를 우선하는 최신 재개 지점을 추가하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/history/260710/99_analysis_summary.md` (+39 -1)
>
> </details>
>
> 구현과 문서 정리는 끝났습니다. 마지막으로 Debug 전체 테스트(30개 + LASAL 정적 계약), WPF 재빌드, XML/line-ending/`git diff --check`를 한 번 더 확인하고 기존 사용자 변경과 섞이지 않았는지도 분리해서 보고하겠습니다.
>
> 병렬로 돌린 두 빌드가 같은 `src/bin/Debug`를 동시에 정리해 DLL을 지우는 충돌이 발생했습니다. 소스 오류가 아니라 공용 출력 폴더 경쟁이므로, 동일 검증을 순차 실행해 확정하겠습니다.
</details>

요청한 3~6번을 설계·소스·테스트에 반영했습니다.

- LASAL 실제 객체명 조회 → opaque descriptor → 4축 dispatcher 구현
- 4/8바이트 ACK, 상태·위치·그룹·1350바이트 멤버 응답 typed parser 구현
- WPF의 `8,388,608`을 23-bit encoder 더미 프로파일로 명시하고 DLL 내부 UNIT 변환 제거
- LASAL PowerOn/Standstill 상태 비트 교정
- legacy `0x2081~0x2084` 차단
- 32비트 LASAL 오류가 16비트 ACK에서 잘리는 false-success 방지
- 지원하지 않는 direction/deceleration 조합은 명시적으로 거부
- Debug/Release C# 테스트 `30/30 PASS`
- LASAL generated table·4축 연결·C#/ST offset 정적 계약 검사 PASS
- WPF Debug 빌드, Network XML/EOL, `git diff --check` PASS

주요 문서:

- [API 개발 백로그](./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)
- [LASAL dispatcher 설계](./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md)
- [자동 테스트 문서](./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md)
- [최신 작업 재개 문서](./docs/history/260710/99_analysis_summary.md)
- [TCPMotionInterface 구현](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st)

아직 남은 가장 중요한 P0는 TCP `Response()`에서 motion을 직접 실행하는 부분입니다. Frame을 queue에 복사하고 실제 `_LMCAxis`/`_LMCRobotBase` 호출을 동일 core의 `RtWork`로 옮겨야 실제 PLC motion 테스트가 안전합니다. 이후 LASAL IDE 재생성·컴파일·PLC 다운로드·패킷 재캡처가 필요합니다.

이번 변경은 아직 커밋하지 않았으며, 기존 사용자 dirty 파일은 stage하지 않았습니다.

> lasal 프로그램 구현하는 부분은 아직 진행하지말고 같이 진행하자. 구현 설계안만 정리해서 만들어

<details><summary>12 previous messages</summary>

> 알겠습니다. 이번에는 LASAL `.st/.lcn` 소스를 더 수정하지 않고, 현재 구현 상태를 기준으로 `Response → command queue → RtWork` 전환 설계안만 문서로 만들겠습니다. 기존 LASAL 변경도 임의로 되돌리지 않고, 구현 착수 전 함께 결정해야 할 항목과 단계별 승인 지점을 분리하겠습니다.
>
> 설계 방향은 “TCP callback에서 정상 명령을 실행하지 않고, 요청 FIFO → CyWork 조정 → RtWork 실제 축 호출 → CyWork 응답 송신”의 3단 분리로 잡겠습니다. 특히 `SendData`도 RtWork에서 빼고, disconnect 세대값·queue full·Close ACK 이후 정리까지 설계에 포함하겠습니다. 아직 확정하지 않은 queue 깊이, mutex 방식, Stop 우선순위는 승인 항목으로 남깁니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 설계안은 `Response → 요청 FIFO → CyWork 조정 → RtWork 실행 → CyWork 응답` 구조를 기준으로 정리하겠습니다. 특히 기존 미커밋 LASAL 프로토타입의 처리 여부, 큐 깊이, RT 동기화 방식, Stop 우선순위는 구현 전에 함께 승인할 항목으로 분리해 두겠습니다.
>
> 현재 코드 기준으로 한 가지를 설계안에 명확히 넣겠습니다. TCP 수신 자체도 지금은 `_TCPIPServer_RT::RtWork()`에서 실행되므로, 축 호출만 옮겨서는 RT 분리가 완성되지 않습니다. 권장안은 TCP 수신·응답을 CyWork 쪽 단일 소유로 만들고, RtWork에는 고정 크기 mailbox와 축/그룹 호출만 남기는 구조입니다.
>
> <details><summary>Created a file, edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+16 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+498 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+7 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+24 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+6 -1)
>
> </details>
>
> 설계 문서는 작성했고 현재 정적 검토 중입니다. 구현 전에 결정해야 할 항목을 D0~D9로 분리했으며, 설계 단계의 기본 권고는 `queue depth 8`, `request 96B`, `TX 4096B`, `strict FIFO`, 첫 시험은 `0x202E ReadActualPosition`입니다. 값들은 아직 승인 상태로 표시하지 않았습니다.
>
> 정적 검토에서 설계상 빠진 조건을 확인했습니다. 특히 `SizeOfTXBuffer`가 실제로는 수신 버퍼 크기에도 같이 쓰이고, 현재 서버 기본 연결 수가 16이라 단일 accumulator와 충돌합니다. 이 두 항목을 승인 결정으로 추가하고, AP 비동기 task와 CyWork 중 어느 context를 쓸지도 설계안에 명시하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+122 -47)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+12 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -1)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+4 -3)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+21 -11)
>
> </details>
</details>

LASAL 구현은 진행하지 않고 설계 문서만 작성했습니다.

- [LASAL Command Queue / RtWork 구현 설계안](./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md)
- [API 개발 Backlog](./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)
- [Delivery README](./LMC_Library/LMC_API_Delivery/README.md)

권장 구조는 다음과 같습니다.

- TCP 수신: `Config=1` AP 비동기 task
- 요청: depth-8 FIFO
- 조정·응답: `CyWork`
- 축/그룹 호출: 축과 동일 core의 `RtWork`
- P0 연결 수: 1개
- 공유 RX/TX buffer: 4096B
- request accumulator: 1024B
- 첫 검증: `0x202E ReadActualPosition`
- 실제 group motion은 별도 승인 전 차단

구현 전 결정할 내용을 D0~D15로 정리했습니다. 가장 먼저 확인할 것은 SIGMATEK의 RT-safe atomic/memory barrier(D4)와 AP/CyWork/RtWork task 배치(D10)입니다. 이 둘이 확정되지 않으면 구현을 시작하지 않도록 명시했습니다.

기존 미커밋 LASAL 프로토타입은 그대로 보존했습니다. 이번 작업 전후 해시도 동일합니다.

- `TCPMotionInterface.st`: `EF30DF54...E0E2AB`
- `Motion_Network.lcn`: `0738D11C...61E87B`

`git diff --check`와 문서 링크 검사를 통과했습니다. 커밋은 하지 않았습니다.

> 지금 정리된 내용들 리스트별로 정리해서 커밋해

<details><summary>12 previous messages</summary>

> 현재 작업트리를 기능별로 나눠 검토한 뒤 목적별 커밋으로 정리하겠습니다. 특히 이전부터 남아 있던 LASAL 프로토타입, C# API·테스트, WPF, 문서·히스토리, 생성 바이너리를 분리해서 불필요한 산출물이 섞이지 않게 하겠습니다. LASAL 구현은 추가하지 않고 현재 상태만 커밋 대상으로 판단하겠습니다.
>
> 분류 결과, `_Edit` LASAL 전체 사본과 pandoc HTML은 비교·생성 자료이고, `test-app`의 새 DLL/EXE는 7월 9일 빌드라 현재 소스보다 오래된 바이너리입니다. 이들은 커밋하지 않고, 현재 소스와 자동 테스트를 먼저 검증하겠습니다. 기존에 추적되던 구형 바이너리 삭제는 별도 정리 커밋 대상으로 보겠습니다.
>
> 검증 결과 C# 자동 테스트 30개와 LASAL 정적 계약이 모두 통과했고, WPF Debug 빌드도 성공했습니다. 누적분은 다음 순서로 분리하겠습니다: LASAL 프로토타입 체크포인트 → C# typed API·자동 테스트 → WPF 안전 예제 → 구형 앱 제거 → RT queue 설계·개발 문서. stale 바이너리와 과거 히스토리 소급 변경은 제외합니다.
>
> 기능 커밋 4개를 분리해 만들었습니다. 이제 마지막 문서 커밋에서 프로토타입을 “미커밋”이라고 적은 표현을 실제 체크포인트 상태로 고치고, 260710 handoff에 커밋 목록과 “queue/RtWork 구현은 승인 전 보류”를 명시하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+7 -7)
> - Wrote `./docs/history/260710/99_analysis_summary.md` (+17 -11)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 마지막으로 설계·백로그·인수인계 문서 묶음을 점검해 별도 커밋하겠습니다. 패키지 바이너리, 과거 히스토리 원문 수정, 403MB 비교용 LASAL 복제 폴더는 이번 커밋에서 계속 제외합니다.
>
> 문서 차이는 현재 상태와 맞습니다. 특히 프로토타입을 운영 승인으로 표현하지 않고, 다음 단계가 D0~D15 설계 승인이라는 점을 인수인계 문서에 명시했습니다. 이제 지정한 6개 문서만 스테이징해 검사하겠습니다.
>
> 검사에서 새 설계 문서 3행의 Markdown 줄끝 공백 1건만 잡혔습니다. 해당 공백을 제거한 뒤 다시 검사하고 커밋합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+1 -1)
>
> </details>
>
> 문서 커밋까지 완료했습니다. 최종 인계 전에 커밋된 상태에서 자동 테스트와 WPF Debug 빌드를 다시 실행하고, 남아 있는 미커밋 항목이 의도한 제외 목록뿐인지 확인하겠습니다.
>
> 검증은 통과했습니다: C# 30/30, LASAL static contract, WPF Debug 빌드 모두 성공했습니다. 이제 커밋 5개와 제외된 잔여 변경을 최종 대조합니다.
</details>

현재 정리된 변경을 목적별 5개 커밋으로 반영했습니다. 현재 HEAD는 `d91da48`입니다.

- `da4a912` — LASAL object dispatcher/DINT handler 프로토타입 체크포인트
- `adeb631` — 패킷 계약 강화, typed response, 자동 테스트 추가
- `6dd7eab` — WPF UNIT 정책과 응답 안전 판정 교정
- `bcbde89` — 구형 `LmcMotionApiTestApp` 제거
- `d91da48` — API 잔여 개발 목록과 LASAL command queue/RtWork 설계 문서화

주요 문서:

- [LASAL command queue/RtWork 설계안](./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md)
- [API 개발 백로그](./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)
- [최신 분석·인수인계 요약](./docs/history/260710/99_analysis_summary.md)

검증 결과:

- C# 자동 테스트: `30/30` 통과
- LASAL static contract: 통과
- WPF Debug 빌드: 성공
- `git diff --check`: 통과
- LASAL IDE 빌드와 PLC 다운로드 검증은 아직 수행하지 않음

LASAL command queue/RtWork 구현은 추가하지 않았습니다. 이미 존재하던 dispatcher 프로토타입만 별도 체크포인트로 보존했고, 다음 단계는 설계안의 D0~D15 승인입니다.

패키지의 오래된 DLL/EXE 교체, 과거 히스토리 원문 수정, 생성 HTML, 약 403MB의 `Elmo_EtherCAT_Test_4Axis_Edit` 비교 폴더는 의도적으로 커밋에서 제외했습니다.

> API 구현쪽에는 기능 구현 완료된건가? LASAL쪽에서만 구현하면 완료되는 상태인거야?

<details><summary>3 previous messages</summary>

> 현재 커밋 기준으로 C# API와 LASAL 구현을 분리해 다시 확인하겠습니다. 결론부터 말하면 “LASAL만 구현하면 전부 완료” 단계는 아닐 가능성이 높고, 누락 API와 검증 항목을 소스·패킷 매트릭스 기준으로 정확히 구분해 답하겠습니다.
>
> 현재 소스 기준 핵심 수치는 확인됐습니다. 패킷에서 확인된 23개 명령 중 PC DLL은 21개에 호출 경로가 있고, `0x2051`은 상수만 있으며 `0x20E7`은 상수·API 자체가 없습니다. 또한 기존 21개도 실제 PLC 종단 검증은 아직 0개입니다.
>
> 추가 대조에서도 동일한 결론입니다. 좁은 범위의 “단일 PC·기존 축 명령 MVP”는 LASAL 실행 구조가 주된 잔여지만, 전체 23개 명령과 callback·multi-PC·배포까지 포함하면 PC API 쪽 작업도 남아 있습니다.
</details>

아니요. 정확히는 PC C# API가 상당 부분 구현됐지만, 전체 API 완료 상태는 아닙니다.

| 구분 | 현재 상태 |
|---|---|
| PC DLL 호출 경로 | 23개 명령 중 21개 구현 |
| 자동 테스트 | C# 30/30, LASAL 정적 계약 통과 |
