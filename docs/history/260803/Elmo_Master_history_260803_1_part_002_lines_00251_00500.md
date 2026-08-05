>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [
>   {
>     "app": "process:C:\\work\\Elmo\\Elmo_Master\\LMC_Library\\LasalApiWpfTestApp\\LasalApiWpfTestApp\\bin\\Debug\\LasalMotionControlApiExample.exe",
>     "id": 7150956,
>     "title": "LASAL Motion Control API Example v0.9.1.0 [CREVIS topology / editable SDO draft]"
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
>   "code": "globalThis.targetWindow = await sky.get_window({id: windowsNow[0].id, app: windowsNow[0].app});\nawait sky.activate_window({window: targetWindow});\nglobalThis.state = await sky.get_window_state({window: targetWindow, include_screenshot: false, include_text: true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));",
>   "title": "현재 연결 로그 다시 읽기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "ChatGPT", App: ChatGPT.exe.
> 	0 창 ChatGPT Secondary Actions: Raise
> 		1 창 (disabled)
> 		2 영역 ChatGPT
> 			3 영역
> 				4 영역
> 					5 영역
> 						6 영역
> 							7 영역
> 			8 영역
> 				9 영역
> 					10 단추 최소화 ID: view_1
> 					11 단추 복구 ID: view_3
> 					12 단추 닫기 ID: view_4
>
> The focused UI element is 0 창 ChatGPT Secondary Actions: Raise.
> ```
>
> 현재 PLC는 13:53에 canonical master가 새로 다운로드·재기동된 사실이 확인됐고, 예제 앱은 그 직후 실행됐습니다. journal은 그보다 앞선 BootId 6의 Power Off 미확정 기록이라 불일치 가능성이 매우 높습니다. 이를 확정하려고 모션 명령 없이 SDK의 `GetDiagnosticsCapabilities(0x7E00)`만 한 번 읽고 정상 종료하겠습니다.
</details>

원인은 확정됐습니다. LASAL/TCP 서버가 끊는 게 아니라, 예제 WPF가 recovery 검증 실패 후 스스로 연결을 닫습니다.

- 활성 Axis Power journal: `_LMCAxis1` Power Off, `AcceptedAwaitingProof`
- journal BootId: `6`
- 방금 읽기 전용 `0x7E00`으로 확인한 PLC BootId: `11`
- MapRevision: 양쪽 모두 `0x957F101E`로 일치

연결 흐름은 다음과 같습니다.

1. TCP/RPC 연결 성공
2. Topology 조회 성공
3. [Axis Power recovery 검사](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisPowerOnRecovery.cs:666)에서 `BootId 6 != 11` 감지
4. 예외 발생
5. [Connect 예외 처리](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:471)가 이미 연결된 TCP를 닫음

오래된 기록은 [axis-power-on-recovery.bin](/C:/Users/dreiv/AppData/Local/Elmo/LasalMotionControlApiExample/AxisPowerOnRecoveryJournal/v1/axis-power-on-recovery.bin)입니다. 파일 손상이 아니라 체크섬이 정상인 과거 Power Off 미확정 기록입니다.

왜 지금 발생했는지도 확인됐습니다. journal은 10:45에 BootId 6으로 만들어졌고, canonical PLC 프로젝트는 13:53에 다시 다운로드·링크·로드됐습니다. [LASAL 로그](/C:/Users/dreiv/AppData/Local/Temp/Lasal2.log:66249)에도 download와 project load 성공이 기록돼 있습니다. 재기동 후 BootId가 11로 바뀌었습니다.

Axis Stop journal은 `Resolved`이고 Motion·Group 관련 활성 record는 없어 현재 차단 원인은 Axis Power journal 하나입니다.

`axis-power-on-recovery.bin`을 바로 삭제하면 이전 Power Off ACK의 “완료 미확정” 정보까지 사라지므로 삭제하지 않는 것이 맞습니다. 다음 수정 대상은 BootId mismatch 때 연결 자체는 제한된 read-only 상태로 유지하고, 오래된 recovery record를 명시적으로 처리할 수 있게 하는 WPF 정책입니다. 이번 진단에서는 파일을 변경하거나 삭제하지 않았습니다.

> 문제를 해결해

<details><summary>24 previous messages</summary>

> journal은 삭제하지 않고 코드로 해결하겠습니다. 목표는 stale BootId가 있어도 TCP/RPC 연결을 유지하되, 안전한 read-only 진단만 허용하고 기존 Power Off 명령을 완료로 오인하거나 재전송하지 않도록 만드는 것입니다. 현재 dirty 변경을 먼저 대조한 뒤 회귀 테스트까지 추가하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 실행 중인 Debug 예제는 건드리지 않고, 사용자 작업과 겹치는 파일만 최소 범위로 수정하겠습니다. 검증은 파일 잠금을 피하기 위해 Release 빌드와 모의 서버 통합 테스트로 진행합니다.
>
> 디버그 종료했어
>
> Debug 종료 확인했습니다. 따라서 수정 후 Release뿐 아니라 Debug도 다시 빌드해 실제 실행 파일까지 갱신하겠습니다. 현재 원인은 오래된 BootId 복구 기록 하나이며, 해결 방향은 “기록 삭제”가 아니라 연결을 유지한 읽기 전용 격리 상태입니다.
>
> <details><summary>Edited 11 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/DiagnosticsOperationAdmissionPolicy.cs` (+41 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+9 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+9 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+10 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+10 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+99 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+10 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+24 -9)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+18 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+11 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisCommandRecovery.cs` (+7 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisPowerOnRecovery.cs` (+7 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.GroupPowerRecovery.cs` (+7 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.GroupProfileLockRecovery.cs` (+7 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs` (+7 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsOperationAdmissionPolicyTests.cs` (+70 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMotionRecoveryIntegrationTests.cs` (+49 -9)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisPowerOnRecoveryIntegrationTests.cs` (+139 -0)
>
> </details>
>
> 구현을 넣었습니다. 불일치 시 연결은 유지되지만 모든 제어·D5·정리·qualification 경로는 전역 차단되고, 일반 비-D5 읽기와 Close/Exit만 허용됩니다. 이제 실제 증상과 같은 `Power Off + AcceptedAwaitingProof + BootId 불일치` 회귀 테스트부터 돌립니다.
>
> 전체 smoke 테스트가 실행 중입니다. 컴파일 단계에서는 아직 오류가 나오지 않았고, WPF 통합 테스트가 순차 실행되는 구간입니다.
>
> 핵심 회귀 4건은 모두 통과했습니다. 실제 증상과 같은 Axis Power 기록에서 연결 유지, 비-D5 `0x7E00` 재조회, 제어 명령 0건, Close 후 journal 상태 불변까지 확인했습니다. 공용 라이브러리 테스트도 975/975 통과했습니다. 이제 기존 WPF 복구 시나리오 전체와 전체 smoke를 확인합니다.
>
> Axis Power 관련 30/30, Motion Recovery 21/21이 통과했습니다. 전체 WPF smoke는 아직 실행 중이며, 현재까지 실패 출력은 없습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+9 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+6 -0)
>
> </details>
>
> 전체 WPF smoke도 208/208 통과했고 Debug 실행 파일까지 재빌드했습니다. 마지막으로 사용자의 실제 복구 journal 해시를 보존한 채 새 Debug 앱에서 Connect→읽기 전용 격리 유지→Close를 확인하겠습니다. 제어 명령은 누르지 않습니다.
>
> 실제 PLC 연결까지 확인했습니다. 새 Debug 앱은 `BootId 6 → 11` 불일치를 표시하면서도 `Connected`를 유지했고, Load/Power On/Power Off/Stop은 모두 비활성화됐으며 Close/Exit는 정상 동작했습니다. journal SHA-256도 전후 동일하고 포트 4000 소켓은 0개로 정리됐습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+57 -56)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+1 -2)
>
> </details>
>
> 내부 리뷰에서 공존 상태 하나를 추가로 찾았습니다. Axis Power stale journal과 D5 orphan/진단 mutation journal이 동시에 존재하면 일부 특수 cleanup/ACK 경로가 중앙 정책을 우회할 수 있습니다. 실제 현재 journal에는 해당 기록이 없지만, “읽기 전용 격리” 계약을 지키도록 이 우회 경로도 막고 테스트하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+22 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+20 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+3 -6)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisPowerOnRecoveryIntegrationTests.cs` (+53 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 남은 우회 경로 하나(디지털 출력 불확실 결과 확인)를 같은 읽기 전용 격리 정책으로 막고, 축 전원 복구 테스트의 시점 기준을 바로잡겠습니다. 그 뒤 전체 빌드·스모크 테스트와 최종 코드 리뷰까지 다시 확인하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisPowerOnRecoveryIntegrationTests.cs` (+7 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+8 -2)
>
> </details>
>
> 수정은 반영됐습니다. 이제 먼저 축 전원 BootId/맵 불일치 두 사례를 각각 실행해 격리 진입 이후 저널 상태가 닫기 전후로 그대로 유지되는지 확인하겠습니다.
>
> 두 핵심 불일치 회귀 테스트는 각각 통과했습니다. 지금 전체 WPF 스모크와 라이브러리 테스트, Debug/Release 빌드를 다시 돌려 특수 경로 차단이 다른 기능을 깨지 않았는지 확인 중입니다.
>
> 전체 스모크는 아직 실행 중이며 중간 실패 출력은 없습니다. 이 테스트에는 별도 프로세스 복구 시나리오가 포함돼 있어 개별 테스트보다 시간이 더 걸립니다.
>
> 전체 회귀 결과는 WPF 208/208, 라이브러리 975/975로 모두 통과했고 최종 리뷰에서도 차단급 문제나 남은 우회 경로는 없었습니다. 마지막으로 작업 파일 범위·공백 오류·실제 복구 저널/소켓 상태만 재확인하겠습니다.
</details>

해결했습니다. 실제 PLC 연결에서 더 이상 즉시 끊기지 않고 `Connected` 상태가 유지됩니다.

원인은 저장된 Axis Power 복구 저널의 BootId가 `6`, 현재 PLC BootId가 `11`이라서, 기존 코드가 복구 식별자 불일치를 연결 실패로 처리해 TCP를 닫았던 것입니다. MapRevision은 양쪽 모두 `0x957F101E`였습니다.

수정 내용:

- 식별자 불일치 시 연결을 유지하고 읽기 전용 격리로 전환: [MainWindow.xaml.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:474)
- 일반 non-D5 조회와 Close/Exit만 허용: [DiagnosticsOperationAdmissionPolicy.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/DiagnosticsOperationAdmissionPolicy.cs:140)
- 제어·Write·D5·qualification·cleanup/ACK는 송신 전에 차단
- 기존 복구 저널은 자동 실행·삭제·해제하지 않고 그대로 보존
- 동작 및 안전 기준 문서화: [README.md](./LMC_Library/LasalApiWpfTestApp/README.md:838)

검증 결과:

- 실제 PLC: 연결 유지, 읽기 전용 경고 표시, 정상 Close 성공
- 복구 저널 SHA-256 전후 동일: `D065A332...10E025`
- WPF 스모크: `208/208 PASS`
- 라이브러리 테스트: `975/975 PASS`
- Debug/Release 빌드 성공
- 종료 후 앱 프로세스 및 포트 4000 연결: `0`
- 관련 파일 공백 검사 통과

현재 BootId 불일치는 실제로 남아 있으므로 앱은 의도적으로 읽기 전용입니다. 제어 기능을 다시 사용하려면 기존 PowerOff 복구 저널을 별도의 확인 절차로 안전하게 해소해야 하며, 저널을 임의 삭제하지는 않았습니다.

> 지금까지 진행된 내용 종류별로 정리해서 커밋 푸쉬해

<details><summary>3 previous messages</summary>

> GitHub 게시 작업이므로 `github:yeet` 절차를 사용하겠습니다. 먼저 현재 브랜치·원격과 대규모 dirty 변경을 읽고, 기존 작업을 덮어쓰지 않으면서 기능·문서·히스토리처럼 검증 가능한 종류별 커밋으로 나눈 뒤 푸시하겠습니다.
>
> 현재 `main`이 원격보다 2커밋 앞서 있고, 소스·LASAL·WPF·문서·캡처가 한 작업트리에 섞여 있습니다. `git add -A`는 하지 않고 각 범위를 병렬로 분류한 다음, 기존 2커밋까지 포함한 안전한 게시 브랜치를 만들겠습니다.
>
> `github:yeet`의 필수 조건인 GitHub CLI가 현재 PATH에서 발견되지 않았습니다. 아직 브랜치 생성·스테이지·커밋은 하지 않았고, 알려진 설치 위치에 실행 파일이 있는지만 확인한 뒤 없으면 설치가 필요합니다.
</details>

현재 커밋·푸시는 시작하지 않았습니다. `github:yeet` 게시 절차의 필수 조건인 GitHub CLI(`gh`)가 설치되어 있지 않습니다.

PowerShell에서 실행해 주세요.

```powershell
winget install --id GitHub.cli -e
gh auth login
gh auth status
```

인증 완료 후 알려주시면 이어서:

- 소스/API
- WPF 예제와 복구 기능
- 테스트
- LASAL 프로젝트
- 문서·히스토리

순서로 분리 커밋하고, 검증 후 브랜치를 푸시하겠습니다. 현재 브랜치 생성·스테이징·커밋 변경은 전혀 하지 않았습니다.

> 커밋만 진행해

<details><summary>19 previous messages</summary>

> 알겠습니다. `gh` 없이 현재 `main`에서 로컬 커밋만 진행하고 푸시·PR·브랜치 생성은 하지 않겠습니다. 기존 변경을 기능 종류별로 감사한 뒤 명시적으로 스테이징하여 여러 커밋으로 나누겠습니다.
>
> 커밋 구조는 구현, 테스트, LASAL/프로젝트, 배포 도구, 문서, 캡처 증거, 히스토리 아카이브로 나누는 방향입니다. 특히 원문 히스토리는 바이트 보존이 필요하므로 공백을 고치는 대신 Git 공백 검사 예외를 경로 한정으로 적용할지 검증하겠습니다.
>
> 감사 결과, WPF는 공용 파일이 기능을 가로질러 전체 1커밋이 안전하고, LASAL은 IDE 바이너리에 TCP와 CREVIS가 함께 기록돼 snapshot 1커밋이 안전합니다. 커밋 전 확인된 두 가지 불일치(프로젝트 nesting 1건, 최신 시험 수치)를 수정하고, 이미 분할본이 커밋된 42MB짜리 `260721` 원본은 인덱스에서만 제외하겠습니다.
>
> <details><summary>Edited 9 files</summary>
>
> - Wrote `./.gitattributes` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -6)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+1 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+3 -3)
