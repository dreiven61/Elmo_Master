| PC·LASAL 양쪽 소스가 있는 명령 | 약 19개 |
| 실제 PLC E2E 검증 완료 | 0/23 |
| 완전히 소스가 없는 기능 | `0x2051`, `0x20E7` |
| 의도적으로 실패하는 기능 | GroupReset, GroupStop은 LASAL에서 `-5` 반환 |

PC API 쪽에 남은 구현은 다음과 같습니다.

- `0x2051 GroupReadActualPosition`
  - 현재 [LmcProtocol.cs](./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs:116)에 command 상수만 존재
  - public API, request builder, 벡터 response parser, 테스트, WPF 기능이 없음
- `0x20E7 SetKinTransformEx/Cartesian`
  - command 상수부터 없으며 PC와 LASAL 양쪽 모두 구현 필요
- Callback
  - PC UDP listener는 있지만 현재 raw `byte[]`만 전달
  - event 종류, payload parser, typed callback, WPF 표시가 남음
- Group motion
  - coordinate/transition/buffer mode가 현재 고정값
  - 배열 길이와 motion 범위 검증도 보강 필요
- 배포
  - 최신 DLL/EXE 재빌드, 버전, release manifest, 샘플 정리가 남음

LASAL 쪽의 핵심 잔여는 다음입니다.

- `Response()`에서 직접 motion 명령을 실행하는 구조를 queue → CyWork → RtWork 구조로 이동
  현재 [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:580)는 `MsgPaser()`를 직접 호출합니다.
- GroupReset/GroupStop의 실제 LASAL 동작 결정 및 구현
  현재는 [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1054)에서 `-5`를 반환합니다.
- 실제 UDP callback sender
- `0x2051`, `0x20E7` server handler
- multi-PC session table, 축 소유권, timeout cleanup
- LASAL IDE model 등록·재생성·컴파일·PLC 다운로드
- 실제 PLC smoke test와 Wireshark 재캡처

따라서 범위를 나누면 다음과 같습니다.

- 단일 PC와 현재 구현된 축 명령만 사용하는 1차 MVP: PC API는 대부분 준비됐고, LASAL command queue/RtWork와 PLC 검증이 가장 큰 남은 작업입니다.
- 23개 명령, group, callback, multi-PC까지 포함한 전체 API: LASAL만 구현해서는 완료되지 않습니다. PC API에도 `0x2051`, `0x20E7`, typed callback 등의 구현이 필요합니다.

현재 전체 판정은 문서대로 “소스·자동 테스트 단계이며, 실제 PLC에서 검증 완료된 API는 아직 0개”가 맞습니다. 다음 진행 순서는 [API 개발 백로그](./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md:336)의 `queue/RtWork → PLC E2E → GroupReset/Stop → callback → 0x2051/0x20E7 → multi-PC·배포` 순서가 적절합니다.

> 지금 분석된 내용 메모리에 업데이트하고 있는건가? 구현 문서에도 업데이트해줘

<details><summary>5 previous messages</summary>

> 지금까지 분석 내용은 저장소 문서에는 반영했지만, Codex 장기 메모리 자체를 직접 수정한 상태는 아니었습니다. 장기 메모리는 자동으로 원본을 덮어쓰는 방식이 아니며, 이번 요청을 근거로 업데이트 노트를 추가하겠습니다. 동시에 구현 문서에 `21/23`, `19개 양쪽 소스 경로`, `E2E 0/23`, “LASAL만 구현하면 전체 완료가 아님”을 명확히 반영하겠습니다.
>
> 구현 문서에는 단순히 “21/23”만 쓰면 오해가 생기므로, `C# 호출 경로 21`, `LASAL case 21`, 그중 `GroupReset/Stop` 2개는 `-5`, 실제 E2E 완료는 `0`으로 나눠 기록하겠습니다. 이렇게 해야 “LASAL만 하면 완료”라는 잘못된 결론을 피할 수 있습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+17 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+36 -6)
> - Wrote `./docs/history/260710/99_analysis_summary.md` (+15 -0)
>
> </details>
>
> 저장소 구현 문서 3곳에 완료도 판정을 반영했습니다. 이제 Codex 메모리 원본을 직접 고치는 대신, 규칙에 따라 이후 세션에서 통합될 권위 있는 ad-hoc 업데이트 노트를 별도로 추가하겠습니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `C:/Users/dreiv/.codex/memories/extensions/ad_hoc/notes/20260710-135026-elmo-api-implementation-status.md` (+40 -0)
>
> </details>
</details>

네. 이번에는 메모리 업데이트까지 반영했습니다.

Codex 메모리는 `MEMORY.md`를 직접 수정하지 않고, 규칙에 따라 통합용 업데이트 노트를 생성합니다.

- [메모리 업데이트 노트](C:/Users/dreiv/.codex/memories/extensions/ad_hoc/notes/20260710-135026-elmo-api-implementation-status.md)

구현 문서도 업데이트했습니다.

- [API Delivery README](./LMC_Library/LMC_API_Delivery/README.md)
- [API 개발 백로그](./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)
- [히스토리 인수인계 요약](./docs/history/260710/99_analysis_summary.md)

반영 내용:

- 대상 명령 23개
- C# 호출 경로 21개
- LASAL handler 21개
- GroupReset/GroupStop은 `-5` 전용이므로 정상 기능 후보는 19개
- 실제 PLC E2E 완료는 0/23
- 전체 API는 LASAL만 구현한다고 완료되지 않음
- `0x2051`, `0x20E7`, typed callback, multi-session, 배포 작업이 PC/API 쪽에도 남음
- 다음 구현 순서를 queue/RtWork → PLC 검증 → GroupReset/Stop → callback → `0x2051`/`0x20E7` → multi-session·배포로 기록

`git diff --check`도 통과했습니다. 저장소 문서 변경은 아직 커밋하지 않았습니다.

> 그럼 API 관련 PC에 구현해야 되는 내용들 다 진행해

<details><summary>11 previous messages</summary>

> 핵심 PC 명령 23종의 프레임/응답 처리는 모두 들어간 상태입니다. 지금은 전송 오류 뒤 손상된 TCP 스트림 재사용, 취소와 재연결 경합, close 오류 누락처럼 실제 운전에서 위험한 연결 수명주기 문제를 정리하고 있습니다. 이 단계가 끝나면 WPF와 배포 문서를 합쳐 전체 검증으로 넘어갑니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+7 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/FakeRpcServer.cs` (+9 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+4 -2)
>
> </details>
>
> <details><summary>Created a file, edited a file</summary>
>
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/LasalMotionControlLibTestApp.csproj` (+5 -4)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/Properties/AssemblyInfo.cs` (+8 -0)
>
> </details>
>
> <details><summary>Edited 14 files, deleted a file</summary>
>
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/LasalMotionControlLibTestApp.csproj` (+0 -1)
> - Deleted `./LMC_Library/LasalMotionControlLibTestApp/Properties/AssemblyInfo.cs` (+0 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+45 -0)
> - Wrote `./LMC_Library/LMC_API/LMC_API/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/API_LIST.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/LMC_PACKET_MAP.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md` (+1 -1)
> - Wrote `./docs/history/260710/99_analysis_summary.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+23 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+9 -0)
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/LMC_PACKET_MAP.md` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+2 -0)
>
> </details>
>
> 연결 계층 보강과 추가 테스트까지 반영되어 PC 자동 테스트는 현재 42/42입니다. 예상하지 않은 IP에서 온 UDP callback 거부, in-flight 취소 시 세션 무효화, queued 취소 시 진행 중 요청 보호, 재연결 후 stale 축/그룹 핸들 거부까지 포함했습니다. 이제 Release 빌드와 배포 바이너리·manifest를 맞추고 있습니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/LMC_API/RELEASE_MANIFEST.md` (+34 -0)
>
> </details>
>
> <details><summary>Created a file, edited 15 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/LMC_API/README.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+31 -22)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+43 -35)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+17 -13)
> - Wrote `./docs/history/260710/99_analysis_summary.md` (+7 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+39 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+6 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+17 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+17 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+72 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+51 -3)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs` (+13 -11)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+2 -3)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs` (+15 -0)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs` (+33 -3)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/LasalMotionControlLibTestApp.sln` (+7 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+27 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+5 -0)
> - Wrote `./LMC_Library/LMC_API/LMC_API/README.md` (+1 -0)
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/MIGRATION_FROM_LMCMOTIONAPI.md` (+51 -0)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs` (+29 -17)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+8 -4)
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/API_LIST.md` (+4 -1)
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/LMC_API_함수명_커맨드ID_인자.txt` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md` (+4 -0)
>
> </details>
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/API_LIST.md` (+3 -0)
> - Wrote `./LMC_Library/LMC_API/LMC_API/README.md` (+2 -0)
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/LasalMotionControlLibTestApp.sln` (+8 -2)
> - Wrote `./LMC_Library/LMC_API/LMC_API/RELEASE_MANIFEST.md` (+4 -4)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -1)
> - Wrote `./docs/history/260710/99_analysis_summary.md` (+5 -2)
>
> </details>
>
> PC 구현과 검증은 완료됐습니다. 요청하신 연속 작업 맥락도 다음 작업에서 바로 이어갈 수 있도록 메모리와 구현 히스토리에 최종 상태를 기록하겠습니다. 이번에는 LASAL 소스를 건드리지 않았고, 커밋도 아직 만들지 않았습니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `C:/Users/dreiv/.codex/memories/extensions/ad_hoc/notes/20260710-150350-elmo-pc-api-complete.md` (+55 -0)
>
> </details>
</details>

PC 측 API 구현 범위는 완료했습니다.

- 23개 명령 request/public 경로 구현
- `0x2051`: 68-byte DINT 응답, legacy LREAL 응답 거부
- `0x20E7`: exact 1320-byte Cartesian 4축 serializer
- timeout/state/async/cancellation, stale handle 차단, callback source 검증
- WPF 비동기 실행·취소·callback 로그·신규 Group UI 적용
- UNIT 변환은 호출자 책임으로 유지
- PC 테스트 `42/42 PASS`
- LASAL static contract `PASS`
- WPF Debug/Release 빌드 및 샘플 컴파일 `PASS`
- `0.9.0-pc-api` DLL/EXE와 SHA manifest 생성

주요 문서:

- [개발 Backlog](./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)
- [API 목록](./LMC_Library/LMC_API/LMC_API/docs/API_LIST.md)
- [Release manifest](./LMC_Library/LMC_API/LMC_API/RELEASE_MANIFEST.md)
- [Legacy API 이관 가이드](./LMC_Library/LMC_API/LMC_API/docs/MIGRATION_FROM_LMCMOTIONAPI.md)
- [최신 히스토리 요약](./docs/history/260710/99_analysis_summary.md)

LASAL 소스는 수정하지 않았습니다. 남은 것은 LASAL handler, large-command staging, callback sender, multi-PC ownership과 실제 PLC 검증입니다.

커밋은 아직 만들지 않았습니다. 다음 커밋에서는 `.gitignore`에 걸린 새 DLL을 `git add -f`로 포함하고, 새 EXE·manifest·migration 문서도 함께 추가해야 합니다.

> 지금 남은 작업 리스트는 머머 있는거지?

<details><summary>1 previous message</summary>

> 현재 저장소 기준으로 다시 확인해서, PC 쪽 마무리 작업과 LASAL/PLC 쪽 실제 기능 작업을 분리해 우선순위대로 정리하겠습니다.
</details>

정리하면 PC API 소스 구현은 끝났고, 남은 핵심은 LASAL 구현과 실제 PLC 검증입니다.

| 우선순위 | 남은 작업 | 현재 상태 |
|---|---|---|
| 1 | 현재 PC 변경분 커밋 | 아직 미커밋 |
| 2 | LASAL command queue/RtWork 설계 확정 | 설계안만 존재 |
| 3 | `0x2051` LASAL 구현 | PC 완료, LASAL 없음 |
| 4 | `0x20E7` LASAL 구현 | PC 완료, LASAL 없음 |
| 5 | GroupReset/GroupStop 구현 | 현재 LASAL은 `-5 unsupported` |
| 6 | callback·다중 PC ownership | PC raw listener만 완료 |
| 7 | LASAL IDE/PLC 검증·재캡처 | 현재 E2E `0/23` |

구체적으로는 다음과 같습니다.

1. PC 변경분 커밋 및 패키지 추적

현재 로컬 DLL/EXE와 manifest hash는 일치하지만 아직 Git에 들어가지 않았습니다.
