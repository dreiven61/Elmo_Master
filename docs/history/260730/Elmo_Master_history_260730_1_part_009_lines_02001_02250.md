다음 작업은 LASAL IDE에서 `TCPMotionInterface`의 불필요한 채널·클라이언트·메서드와 기존 TCP→축/로봇 직접 연결 10개를 삭제하고 서비스 경로만 남기는 구조 정리입니다. 로컬 프로젝트 구성요소를 실제 삭제하는 작업이므로 실행 직전 확인이 필요합니다.

지금 원본 LASAL 프로젝트에서 이 삭제를 진행해도 될까요?

> ㅇㅇ 불필요한거 정리하는 작업을 진행하는데 대신 Lasal을 이용하지 말고 외부에서 편집한담에 Lasal에 적용 확인 하도록하자. 니가 직접 Lasal을 제어하니까 성능도 떨어지고 내가 작업을 할수도 없어. 너무 비효율적인 구조다

<details><summary>6 previous messages</summary>

> 좋습니다. 앞으로 LASAL을 직접 제어하지 않겠습니다. 원본의 추적 소스와 프로젝트 연결 파일을 외부 편집으로 정리하고, 정적 검증까지 끝낸 뒤 마지막에 LASAL에서 적용·빌드 확인할 항목만 따로 정리하겠습니다. 수동 복사한 테스트 폴더와 사용자 미추적 파일은 건드리지 않습니다.
>
> 구조를 확인했습니다. 외부 편집 대상은 세 군데입니다: 클래스 소스의 생성 선언/테이블과 구현부, 통신 네트워크의 TCP 직접 축·로봇 연결 10개, 그리고 생성 네트워크 테이블의 같은 10개 항목입니다. 바이너리 `Classes.lcb`는 외부에서 억지로 수정하지 않고, 마지막 LASAL 적용 확인 때 재생성 대상으로 남깁니다.
>
> <details><summary>Created a file, edited 3 files, deleted a file</summary>
>
> - Wrote `./.codex_phase5_transform.ps1` (+186 -0)
> - Deleted `./.codex_phase5_transform.ps1` (+0 -186)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn` (+21 -57)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/ONE_Comm_Network_Table.st` (+9 -19)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+11 -10)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+2 -2)
>
> </details>
>
> 외부 텍스트 구조 정리는 적용됐습니다. `TCPMotionInterface`는 자체 채널 4/3/0, 운송 함수 8개만 남았고 TCP의 축·로봇 직접 연결 10개도 제거했습니다. 다만 LASAL 바이너리 클래스 DB(`Classes.lcb`, `Networks.lcb`)는 외부 편집이 불가능해 현재 의도적으로 이전 메타데이터 상태입니다. 이 불일치를 숨기지 않고 검증기가 실패로 잡도록 만든 뒤, 나중에 LASAL에서 재생성·적용 여부만 확인하게 하겠습니다.
>
> 검증기는 두 단계를 분리했습니다. 외부 텍스트 준비 모드는 통과해야 하고, 기본 Phase 5는 LASAL 바이너리 DB가 아직 예전 상태라 반드시 실패해야 합니다. 지금 이 두 결과를 실제로 다시 재현한 뒤 PC/WPF 회귀를 이어서 확인하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+10 -8)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+15 -13)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+23 -10)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+2 -1)
>
> </details>
</details>

Phase 5 외부 편집 단계까지 완료했습니다. LASAL은 실행하거나 제어하지 않았고 수동 테스트 복사본도 건드리지 않았습니다.

- [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:201)
  - 자체 채널 `server/client/data = 4/3/0`
  - 구현 함수 8개만 유지
  - Diagnostics 24개를 `MsgPaser`에 inline
  - 축/로봇 직접 client, 기존 domain state/helper 제거
- [Comm_Network.lcn](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn:217)
  - TCP→축/로봇 직접 연결 10개 제거
  - Control service 축/로봇 연결 10개와 service 연결 유지
- 생성 네트워크 테이블 external connection `26 → 16`
- [Phase5 검증기](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1:13)와 관련 문서 갱신

검증 결과:

- 외부 준비 모드 SourceOnly/full static: PASS
- PC Debug/Release: 각각 148/148 PASS
- WPF Debug/Release: 경고 0, 오류 0
- PowerShell/XML 파싱과 `git diff --check`: PASS
- 기본 Phase5 검증: 예상대로 FAIL
  - `Classes.lcb`, `Networks.lcb`에 기존 TCP 채널·연결 등록이 남아 있음

따라서 지금은 커밋하지 않았습니다. 나중에 LASAL에서 Reload Class 및 Network 재생성 후 `Classes.lcb`/`Networks.lcb`를 갱신하고, Rebuild/Link와 smoke test를 통과한 다음 기본 `Phase5TransportClean`을 재실행해야 최종 완료로 판정할 수 있습니다.

사용자 미추적 `TestClass/`와 packet 분석 TXT도 그대로 보존했습니다.

> 내가 지금 멀 테스트 해야되는지 정리해줘봐

<details><summary>3 previous messages</summary>

> 현재 상태 기준으로 “지금 바로 가능한 검사”와 “LASAL 적용 후 해야 할 검사”, “PLC에서만 가능한 검사”를 나눠 정리하겠습니다. 먼저 수동 테스트 폴더가 최신 Phase 5 소스를 포함하는지도 수정 없이 비교하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 중요한 점부터 확인됐습니다. 현재 `Elmo_Master_test_260724_phase4`는 최신 Phase 5 시험본이 아닙니다. TCP 클래스에는 제거 대상 축/로봇 직접 채널이 아직 남아 있고, WPF 프로젝트/실행 파일도 그 경로에는 없습니다. 이 상태로 돌리면 구버전을 시험하게 됩니다.
</details>

결론부터 말하면, **현재 테스트 폴더 그대로는 최신 Phase 5 테스트를 하면 안 됩니다. 구버전입니다.**

현재 확인된 차이:

- 테스트본 TCP 채널: `20/13/0` → 최신 기대값 `4/3/0`
- TCP→축/로봇 직접 연결: 테스트본 `10개` → 최신 기대값 `0개`
- External connection: 테스트본 `26개` → 최신 기대값 `16개`
- `C:\work\Elmo\Elmo_Master_test_260724_phase4`에는 WPF 프로젝트와 실행 파일이 없음

PC 자동 테스트 Debug/Release 각 `148/148`과 WPF 빌드는 개발본에서 이미 통과했습니다. 지금 사용자가 확인해야 할 핵심은 아래 순서입니다.

### 1. 테스트 폴더 최신화

LASAL을 닫고 다음 최신 파일을 테스트 폴더에 반영해야 합니다.

- `Class\TCPMotionInterface\TCPMotionInterface.st`
- `Network\Comm_Network\Comm_Network.lcn`

주의:

- 개발본의 `Classes.lcb`, `Networks.lcb`도 아직 오래된 메타데이터이므로 복사하지 마세요.
- `ONE_Comm_Network_Table.st`는 최종적으로 LASAL 저장/Rebuild로 재생성해서 확인해야 합니다.
- WPF는 [LasalApiWpfTestApp.sln](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.sln)을 개발 저장소에서 실행하거나, `LasalApiWpfTestApp`과 `LMC_API_Delivery`를 함께 테스트 폴더에 복사해야 합니다.

### 2. LASAL 적용 시험 — 가장 먼저

테스트 `.lcp`를 열고 다음을 확인합니다.

- `TCPMotionInterface`, `LMCControlCommandService`, `LMCDiagnosticsService`를 `Reload Class`
- TCP 자체 채널이 server/client/data `4/3/0`
- TCP 함수가 8개
- TCP→`LMCAxis1..9`, `LMCRobot` 직접 연결 0개
- `LMCControlCommandService`→축 1~9/Robot 연결 10개 유지
- Generated external connection 16개
- 외부 편집한 `.st` 구현이 옛날 코드로 덮어써지지 않았는지 확인
- Rebuild/Link 오류 0개

`Find in Implementation` smoke:

- TCP: `CyWork`, `SendData`, `MsgPaser`
- Control: `HandleRequest`, `HandleGroupCommands`, `GroupReadStatus`
- Diagnostics: `GetDiagnosticsBootId`, `HandleRequest`, `LMCDiagnosticsService`
- 시험 시작 이후 `%TEMP%\Lasal2.log`의 신규 `CInvalidArgException` 0개

여기까지 통과해야 PLC에 다운로드합니다.

### 3. PLC/WPF 최소 동작 시험

PLC Download 후 반드시 cold restart하고 새 WPF 세션으로 시작합니다.

1. E-stop, HW/SW limit, 원점 상태와 실제 이동 가능 범위 확인
2. Connect
3. `Refresh Admin Capabilities`, `Refresh Capabilities`
4. 축 Read-only:
   - Load Axis
   - Read Status
   - Read Position
5. 그룹 Read-only:
   - Load Group
   - Get Members
   - Read Status/Position
6. Diagnostics:
   - Read EtherCAT Health
   - Load PI Catalog
   - Read Selected PI
7. 축 1개 저속·소거리:
   - Power On
   - Status 확인
   - 작은 Absolute Move
   - Stop
   - Power Off
8. 그룹 저속·소거리:
   - Power On
   - Power Ready 확인
   - Set Identity/Home Check (`0x20E7`)
   - Enable
   - Locked Standby 확인
   - 작은 Move Linear Absolute
   - Stop/InPosition 확인
   - Disable
   - Power Off

### 4. 패킷에서 확인할 핵심

- 요청 1개당 응답 정확히 1개
- 응답의 command/reference가 요청과 일치
- 실제 motion이 두 번 실행되지 않음
- Disconnect/Reconnect 후 이전 응답이나 이전 motion이 재실행되지 않음
- 잘못된 size/reference/mode는 motion 없이 실패

### 나중에 할 장시간 시험

- 축 1~9 각각 Status/Position/소거리 Motion
- Group Relative/Buffered/Stop-first
- Bulk 24-entry × 100회
- Recorder soak/reconnect/adopt
- SDO offline/timeout/cancel/contention
- 10,000회 성능, P95, throughput, jitter/overrun

즉, **지금은 테스트 폴더 최신화 → LASAL `4/3/0`, 직접 연결 0개, external 16개 확인 → Rebuild/Link → PLC 최소 smoke**까지만 하면 됩니다. 상세 기준은 [Phase 5 검증 문서](./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md)에 정리돼 있습니다.

> 지금 리파짓토리 정리해서 커밋하자. 그리고 테스트용으로 커밋한 브렌치는 지워도 될꺼같아. 그렇게 정리 진행해. 그리고 불필요한 디렉토리는 내가 삭제했어. 안쓰는거 같아서. 정리해서 커밋하자.

<details><summary>6 previous messages</summary>

> 현재 작업트리, 삭제된 디렉터리, 로컬 브랜치·worktree를 먼저 대조하겠습니다. 삭제 내용은 사용자가 정리한 것으로 포함하되, 소스나 검증 자료가 잘못 빠진 것은 없는지 확인한 뒤 목적별 커밋으로 나누고 테스트용 브랜치를 안전하게 제거하겠습니다. LASAL은 실행하지 않습니다.
>
> 삭제된 추적 대상은 `Lasal_PRG/MotionTCPDemoWelcon8Axis` 하나뿐이며 190개 파일입니다. 현재 저장소에서 이 프로젝트를 참조하는 활성 경로는 발견되지 않았습니다. 테스트 브랜치는 `codex/phase4-test-260724`로 확인됐고 원격에는 없으며, 현재 Phase 5 커밋을 만든 뒤 stale worktree 메타데이터와 함께 제거하겠습니다. 이제 커밋 전 정적 계약·PC 테스트·WPF 빌드를 다시 실행합니다.
>
> 재검증 중 문서와 현재 파일 상태의 불일치를 찾았습니다. 문서는 아직 “binary metadata가 stale이고 기본 검사가 실패한다”고 적혀 있지만, 방금 우회 옵션 없이 `Phase5TransportClean` 전체 정적 계약을 실행한 결과 PASS했습니다. 수정된 `Classes.lcb`/`Networks.lcb`도 현재 transport-clean 계약을 충족합니다. 커밋 전에 문서를 현재 증거에 맞추되, LASAL IDE Rebuild/Link·PLC 검증은 여전히 미수행으로 분리해서 기록하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+17 -15)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+37 -39)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+32 -32)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+5 -5)
>
> </details>
>
> 검증 결과는 모두 통과했고 문서도 현재 증거에 맞게 정정했습니다. 커밋은 세 개로 나눕니다: ① Phase 5 LASAL 구현+정적 계약, ② 상태/검증 문서, ③ 사용자가 삭제한 미사용 Welcon 데모. 사용자 scratch 두 개는 제외합니다.
>
> 세 커밋이 완료됐습니다. 이제 사용자가 삭제한 테스트 worktree의 남은 Git 등록만 prune하고, 로컬 전용 `codex/phase4-test-260724` 브랜치를 제거합니다. 브랜치의 고유 snapshot은 현재 Phase 5 커밋으로 대체됐고 원격에는 없습니다.
</details>

정리와 커밋을 완료했습니다.

- `837758f` — Phase 5 transport-only service routing 및 정적 계약
- `a29cb33` — 현재 검증 상태와 남은 LASAL/PLC 시험 문서화
- `6ba73e8` — 미사용 `MotionTCPDemoWelcon8Axis` 190개 파일 삭제

추가 정리:

- `codex/phase4-test-260724` 브랜치 삭제
- 삭제된 테스트 worktree의 Git 메타데이터 제거
- 기존 reorg 브랜치 두 개는 유지
- 삭제한 테스트 커밋 `a0f8ebe`는 GC 전까지 해시/reflog로 임시 복구 가능

검증 결과:

- PC Debug/Release 각각 `148/148` PASS
- WPF Debug/Release 빌드 PASS
- `Phase5TransportClean` 기본 full static PASS
- `git diff --check`, `git diff --cached --check` PASS
- LASAL IDE/PLC 시험은 수행하지 않음

현재 추적 파일은 깨끗합니다. 아래 사용자 scratch만 커밋하지 않고 보존했습니다.

- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TestClass/`
- `test/packet_capture/SIGMATEK_API_Analyze/04b_Group_Absolute_DynamicTimeout_20A4.txt`

`main`은 `origin/main`보다 11개 커밋 앞서 있으며 push는 하지 않았습니다.

> 이제 테스트는 C:\work\Elmo\Elmo_Master_test에 Elmo_EtherCAT_Test_4Axis폴더만 복사해와서 여기서 테스트 할꺼야. 어짜피 GUI WPF 프로그램은 니가 편집을 하고 있어도 내가 테스트하는데 전혀 문제될께 없지만 Lasal프로그램은 외부에서 변경됐을때 문제가 될 가능성이 높아. 그래서 이렇게 테스트를 진행할 께.
>
> 일단 Lasal은 빌드 테스트를 해봤는데 정상적으로 수행됐다. 이제 나는 간단한 구동 테스트를 진행할테니 니는 이제 구현설계에서 어디까지 진행됐는지 점검하고 나머지를 소팅한담에 계속 개발을 진행해 가도록해

<details><summary>21 previous messages</summary>

> 테스트 기준은 이렇게 고정하겠습니다: 사용자는 `C:\work\Elmo\Elmo_Master_test\Elmo_EtherCAT_Test_4Axis`에서 LASAL 빌드·구동을 수행하고, 저는 그 폴더를 건드리지 않고 `C:\work\Elmo\Elmo_Master`의 개발본만 수정합니다. 방금 확인한 LASAL 빌드 성공은 사용자 보고 증거로 구분해 기록하겠습니다. 먼저 구현설계와 현재 소스를 대조해 완료·부분완료·미착수로 다시 분류한 뒤, 구동시험과 충돌하지 않는 다음 개발 항목부터 진행하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> LASAL 쪽은 지금 사용자 구동 테스트와 충돌하지 않도록 더 수정하지 않겠습니다. 설계 대비 진행률을 대조해 보니 서비스 라우팅·전송 정리는 사실상 Phase 5까지 구현됐고, 다음으로 독립 진행 가능한 우선순위는 PC 응답 수신부의 과대 payload 방어입니다. 나머지 감사 결과까지 합친 뒤 이 항목부터 구현·자동 테스트하겠습니다. 현재 생긴 `Classes.lcb`와 staged 히스토리 파일은 사용자 변경으로 보고 건드리지 않습니다.
>
> C:\work\Elmo\Elmo_Master\Lasal_PRG\Elmo_EtherCAT_Test_4Axis 폴더도 니가 요청하면 내가 빌드해서 테스트 하도록 할께.
>
> 시퀀스는 이렇게하자
>
> 니가 개발
