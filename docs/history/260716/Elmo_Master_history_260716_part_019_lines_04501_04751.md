> 근거가 확정된 범위부터 열겠습니다. 이번 패치에서는 단일축 6개와 Group Enable/Disable/ReadStatus를 활성화합니다. `GroupReset`, `GroupStop`, `MoveLinear`, `0x2051`, `0x20E7`은 현재 LASAL 대응 의미가 확정되지 않아 가짜 성공을 만들지 않고 `-5`를 유지합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+8 -13)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+48 -33)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+80 -36)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+94 -71)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+77 -44)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+99 -38)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 근거가 확정된 명령만 실제 호출 경로로 열어 둔 상태입니다. 이제 정적 계약 테스트와 문서를 새 구현 범위에 맞추고, PC 테스트·소스 검증까지 끝내겠습니다. LASAL IDE 재빌드와 PLC 실동작은 이번 소스 수정 이후 별도 확인 항목으로 남깁니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+18 -14)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+81 -10)
>
> </details>
>
> 정적 계약은 새 활성 범위로 통과했습니다. 현재 활성 경로는 단일축 Power/Reset/Stop/3종 Move와 기존 Read 2종, 그룹 Enable/Disable/Status까지이며, 의미가 확정되지 않은 그룹 Reset/Stop/Linear·좌표·kinematic transform은 계속 `-5`로 차단됩니다. 이제 PC 42개 회귀 테스트와 문서 일치 여부를 확인 중입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+5 -5)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/SIGMATEK_LASAL_coding_rules.md` (+1 -1)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+2 -2)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_method_study.md` (+1 -1)
>
> </details>
>
> 리뷰에서 그룹 Enable/Disable의 미연결 오류가 `-2`가 아니라 상위비트 검사 때문에 `-6`으로 바뀌는 결함을 찾았습니다. 이건 즉시 수정합니다. GroupReadStatus는 현재 in-position만 읽어 robot/profile 오류를 보고할 근거가 없어, “완전한 그룹 상태”가 아니라 제한된 호환 상태라는 점도 소스와 문서에서 명확히 처리하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+31 -14)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+10 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+1 -1)
>
> </details>
</details>

남은 LASAL 구현을 가능한 범위까지 진행했습니다.

- 단일축 8개 명령 활성화: Power, Reset, Stop, ReadStatus, ReadPosition, Absolute, Relative, Velocity
- 그룹 3개 명령 활성화: Enable, Disable, ReadStatus
- 모든 축 호출 전 `IsClientConnected()` 검사
- 잘못된 프레임 `-3`, 연결 실패 `-2`, 미지원 조합 `-7`, 오류 범위 초과 `-6` 처리
- GroupReadStatus가 robot/profile 오류를 성공으로 숨기지 않도록 보완
- TCPMotionInterface RT Task/mailbox/atomic 경로 없음
- 미확정 명령 5개는 안전하게 `-5` 유지:
  `GroupReset`, `GroupStop`, `MoveLinear`, `GroupReadActualPosition`, `SetKinTransform`

주요 변경 파일:

- [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1181)
- [Verify-LasalContract.ps1](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1:115)
- [CyWork-only 설계서](./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md:65)
- [개발 백로그](./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md:214)

검증 결과:

- PC API 테스트: 42/42 통과
- LASAL SourceOnly 계약: 통과
- 네트워크 포함 LASAL 계약: 통과
- 변경한 LASAL 추가 코드: 7-bit ASCII 통과
- 작업 대상 파일 `git diff --check`: 통과

아직 완료되지 않은 것은 LASAL IDE 재빌드, `Find in Implementation` smoke test, PLC 다운로드 및 실제 축 시험입니다. 전체 `git diff --check`는 별도 변경인 `ECAT_Master_Base.st` 5050~5051행의 trailing whitespace 때문에만 실패합니다. 이번 변경은 아직 커밋하지 않았습니다.

> 빌드에러가 나네. 그리고 이제 완료된 api 기준으로 사용 매뉴얼을 만들어야돼. 사용 매뉴얼 만들 준비를해.
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-76e38fa7-96e7-4d81-a8cc-0702ddd8f8bc.png`

<details><summary>7 previous messages</summary>

> 에러 원인은 화면에 나온 그대로 `ReadRobotParameter(_ROBOT_STATE)` 반환형이 `DINT`가 아니라 `_LMCRobotBase::_ROBOT_MODE`인데 로컬 변수를 `DINT`로 선언한 타입 불일치입니다. 먼저 이 타입을 정확히 고쳐 재검증하고, 동시에 현재 완료 API·미지원 API·UNIT 적용 책임·필수 초기화 순서를 기준으로 사용자 매뉴얼의 목차와 예제 준비물을 정리하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+1 -1)
>
> </details>
>
> 타입 수정은 적용했고 정적 계약도 다시 통과했습니다. 매뉴얼은 “C#에 메서드가 존재함”과 “현재 LASAL에서 실제 실행 가능함”을 반드시 분리해야 합니다. 특히 GroupReset/GroupStop/MoveLinear/GroupReadActualPosition/SetKinTransform은 공개 메서드는 있지만 PLC가 `-5`를 반환하므로, 본문 사용 예제에서는 제외하고 미지원 표에만 넣겠습니다.
>
> <details><summary>Created a file, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+210 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+5 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+9 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+11 -1)
>
> </details>
>
> 매뉴얼 준비 문서를 만들고 기존 예제의 폐기된 RtWork 표현도 CyWork-only 기준으로 정리했습니다. 배포 패키지 쪽 문서/DLL은 7월 10일 preview라 지금 직접 덮어쓰지 않고, LASAL 빌드와 PLC 검증 후 최신 Delivery 소스에서 재생성하도록 출판 절차에 고정했습니다. 이제 자동 테스트와 변경 범위 점검을 마무리합니다.
>
> 주의할 점이 하나 있습니다. LASAL IDE가 파일을 연 상태에서 외부 소스를 수정했으므로, 현재 열려 있는 이전 편집 내용을 그대로 Save하면 수정이 덮어써질 수 있습니다. `TCPMotionInterface` 탭을 닫고 다시 열거나 프로젝트를 재로드한 뒤 2069행이 `if robotState = _ROBOT_ERROR$DINT then`인지 확인하고 Rebuild해 주세요.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+1 -1)
>
> </details>
</details>

빌드 오류 수정과 사용자 매뉴얼 준비를 완료했습니다.

- 원인: `DINT` 변수와 `_ROBOT_MODE` 열거형 직접 비교
- 수정: [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:2069)
  ```st
  if robotState = _ROBOT_ERROR$DINT then
  ```
- 동일 오류 재발 방지 정적 검사도 추가했습니다: [Verify-LasalContract.ps1](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1:223)

LASAL IDE에서 기존 편집 버퍼를 저장하면 외부 수정이 덮어써질 수 있습니다. `TCPMotionInterface` 탭이나 프로젝트를 다시 연 뒤 2069행을 확인하고 Rebuild해야 합니다.

매뉴얼 준비 결과:

- [사용자 매뉴얼 제작 준비서](./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md:1)
- 지원/미지원 API 구분
- 연결·RPC·callback·object lookup 사용 순서
- PC 프로그램의 UNIT 곱셈 책임
- 단일축 및 현재 지원 그룹 API 범위
- `-1`~`-8` 오류 코드
- 안전한 예제 목록과 최종 매뉴얼 목차
- 출판 전 LASAL/PLC 검증 체크리스트
- 최종 매뉴얼 원본 및 배포 패키지 경로 확정

기존 예제와 README의 오래된 RtWork/group 설명도 현재 CyWork-only 상태로 정리했습니다.

- [BasicUsage.cs](./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs:91)
- [README.md](./LMC_Library/LMC_API_Delivery/README.md:7)

검증 결과는 PC 테스트 42/42, LASAL SourceOnly 계약, 네트워크 포함 strict 계약 모두 통과했습니다. 단, 실제 LASAL Rebuild 성공은 이번 수정 재로드 후 확인해야 합니다. 성공하면 바로 최종 사용자 매뉴얼 본문 작성을 시작할 수 있습니다.

<details><summary>3 previous messages</summary>

> Context compaction
>
> Source: automatic
> Status: completed
>
> 이 증상은 소스 컴파일 오류라기보다, 프로젝트를 열 때 수행되는 라이브러리 재연결/인덱싱 단계에서 설치된 라이브러리 버전과 프로젝트 컴파일러 버전이 어긋난 상황으로 보입니다. 실제 설치 파일, 프로젝트 메타데이터, 최신 `Lasal2.log`를 대조해서 왜 빌드는 통과하는지까지 분리해 확인하겠습니다.
>
> 현재까지 확인된 핵심은 명확합니다. `MotionLib\Include\global.h` 15행이 요구하는 `Hardware\Class\_DriveMngBase\DriveComL2.h`가 실제 설치 폴더에 없습니다. 반면 `_DriveMngBase.st`, `DriveMngBase.h`, 사전 빌드된 `.lba/.lob`는 존재합니다. 이제 “설치 패키지 불일치”인지 “열기 시 전체 라이브러리 파싱만 실패하고 실제 프로젝트 빌드는 사전 빌드 객체를 써서 통과하는 것”인지 로그와 메타데이터로 좁히고 있습니다.
</details>

결론부터 말하면, `TCPMotionInterface` 소스 문제는 아닙니다. LASAL 설치 라이브러리가 서로 다른 세대로 섞여 있습니다.

확인된 사실은 다음과 같습니다.

- 시작 시 오류:
  - [MotionLib global.h](<C:/ProgramData/Sigmatek/Drive(C)/Program Files (x86)/Sigmatek/Lasal/Class2/Runtime/Library/Motion/MotionLib/Include/global.h:15>)가 `Hardware\Class\_DriveMngBase\DriveComL2.h`를 요구합니다.
  - 그런데 현재 Hardware 설치 폴더에는 그 파일이 실제로 없습니다.
- 버전 경고:
  - [프로젝트 설정](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcp:277)은 `C78`입니다.
  - 현재 설치된 `Hardware`, `MotionLib`, `OS Interface`, `System`, `Tools`는 `C81`로 빌드되어 있습니다.
- 설치 날짜도 섞여 있습니다.
  - MotionLib: 2025-07-25
  - Hardware/Class2: 2026년
  - 로그에도 구형 binary format `Project 194 → 196`, `Class 92 → 94` 변환이 기록됩니다.
- `ReducedClientDependency=false` 경고는 메인 프로젝트 설정이 아닙니다.
  - 메인 프로젝트는 `true`입니다.
  - 설치된 MotionLib 자체가 `false`라서 필요한 linked class를 자동 재연결한다는 안내입니다.

빌드가 성공하는 이유는 경로가 다르기 때문입니다.

1. 프로젝트를 열 때 IDE가 설치된 vendor `MotionLib\Include\global.h` 전체를 읽다가 누락된 `DriveComL2.h`에서 E0015를 냅니다.
2. 실제 Build에서는 프로젝트 내부 [Include/global.h](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/global.h)를 사용합니다.
3. 프로젝트 쪽 `global.h`에는 `DriveComL2.h` 참조가 없고, 현재 사용하는 class만 컴파일합니다.
4. 실제 로그에서도 13:08:01에 `Linker Done`과 `Last command succeeded`가 확인됐습니다.

따라서 현재 상태는:

- 현재 프로젝트 Compile/Link: 성공
- 이번 API/LASAL 소스 수정 때문에 발생한 오류: 아님
- 완전히 정상이라 무시 가능한 상태: 아님
- 향후 Drive 관련 class 추가나 라이브러리 재연결 시 실제 빌드 오류로 발전할 가능성: 있음

권장 조치는 다음 순서입니다.

1. LASAL IDE를 종료합니다.
2. `MotionLib`, `Hardware`, `System`, `Tools`, `OS Interface`를 동일한 LASAL 배포 세트로 Repair 또는 재설치합니다.
3. `DriveComL2.h`를 다른 구형 프로젝트에서 복사하거나 vendor `global.h`를 직접 수정하지 않습니다.
4. 그다음 컴파일러 기준을 결정합니다.
   - 현재 설치 기준을 따르면 프로젝트를 C81로 올리는 것이 장기적으로 맞습니다.
   - PLC 기준이 C78로 고정돼 있다면 라이브러리 전체를 C78 호환 세트로 맞춰야 합니다.
5. C78→C81 변경 시에는 별도 작업으로 전체 Rebuild/Link, PLC 다운로드, 장비 회귀시험까지 해야 합니다.

이번 진단에서는 소스나 프로젝트 파일을 수정하지 않았습니다.

> 이제 실제로 동작되는거 테스트 진행하면 되냐.
> 더 남은 작업은?

<details><summary>2 previous messages</summary>

> 실동작 시험 단계로 넘어갈 수 있는지 현재 빌드 결과, 지원 명령 목록, PLC 시험 전 필수 조건을 다시 대조하겠습니다. “바로 축을 움직여도 되는 항목”과 “아직 구현·검증이 남은 항목”을 분리해서 답하겠습니다.
>
> 추가로 중요한 상태가 확인됐습니다. 프로젝트를 연 과정에서 LASAL이 설치 라이브러리의 linked class를 대량으로 프로젝트에 다시 가져와 현재 작업 트리에 수많은 vendor 파일 변경과 `SafetyRoutingTables` 신규 폴더가 생겼습니다. 이 상태가 단순 생성 동기화인지 실제 라이브러리 코드 교체인지 확인하기 전에는 축 구동 시험을 승인하면 안 됩니다. 정적 계약과 변경 범위를 바로 확인하겠습니다.
</details>

네. 다음 핵심 단계는 실제 PLC E2E 시험이 맞습니다. 다만 지금 바로 축을 움직이는 것은 권장하지 않습니다.

현재 통과한 항목:

- LASAL Rebuild/Link 성공
- PC 자동 테스트 `42/42 PASS`
- CyWork-only LASAL 정적 계약 PASS
- 일반 `_TCPIPServer1`, `Config=0`, `MaxConnections=1` 확인
- `LMCAxis1..4`와 `LMCRobot` 연결 확인
- RT Task를 사용하지 않는 TCP 실행 구조 확인

축 구동 전에 남은 필수 조건은 다음과 같습니다.

1. `DriveComL2.h` E0015와 C78/C81 라이브러리 불일치 해결
   - 해결하지 않고 시험하면 진단 시험은 가능해도 최종 승인 시험으로 인정하기 어렵습니다.

2. LASAL 자동 라이브러리 동기화 변경 검토
   - 현재 Git 기준 LASAL 프로젝트에 대량의 vendor 파일 변경과 신규 `SafetyRoutingTables`가 생겼습니다.
   - 샘플 비교 결과 변경된 vendor 파일은 설치 라이브러리와 동일했습니다.
   - 의도한 라이브러리 마이그레이션인지 확인한 후 유지/정리해야 합니다.

3. 최종 Rebuild 후 `Find in Implementation`
   - `Power`, `pos`, `velo`
   - 실행 이후 새 `CInvalidArgException`이 없어야 합니다.

4. PLC Task/Core 확인
   - `_TCPIPServer1`과 `TCPMotionInterface1`이 같은 CyWork task인지
   - axis RT thread와 같은 CPU core인지
   - CyWork 우선순위가 axis RT task보다 높지 않은지 확인해야 합니다.

5. 장비 안전값 확인
