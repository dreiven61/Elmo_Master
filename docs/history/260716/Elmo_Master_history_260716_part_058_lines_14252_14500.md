
PLC에서 다음 순서로 확인하면 됩니다.

1. LASAL IDE에서 Rebuild/Link
2. `TCPMotionInterface`의 Find in Implementation 확인
3. PLC 다운로드
4. `_LMCAxis5`부터 `_LMCAxis9`까지 Load
5. Read Status → Power On → 작은 Relative Move → Read Position → Stop → Power Off
6. 가상축 5개가 1ms Motion RT 객체이므로 PLC CPU/RT load 확인

또한 저장된 Network에서는 `_LMCAxis1..9.IntUnits`가 모두 `1 mm`로 보입니다. 이전의 `10 mm/rev` 설정 논의와 다르므로 다운로드 전에 축 변환 설정을 확인해야 합니다. 이 UNIT 값은 이번 작업에서 변경하지 않았습니다.

이번 변경은 아직 커밋하지 않았습니다.

> 이거 축이 Home이 안되어 있는거를 Group화면에서 모니터링 할 수 있었으면 좋겠는데 Group Read해야 데이터를 볼수 있는건가? 아니면 SingleAxis Read를 해야 볼 수 있나?

<details><summary>1 previous message</summary>

> 현재 구현의 `Group Read Status` 응답에 축별 Home/Reference 상태가 포함되는지와, Single Axis 상태에서 어떤 비트로 확인하는지를 코드 기준으로 확인하겠습니다. 변경은 하지 않고 현재 가능한 모니터링 경로를 정확히 구분하겠습니다.
</details>

현재 구현 기준으로는 **Single Axis Read Status를 해야 Home 여부를 확인할 수 있습니다.**

- `Group Read Status`는 Group Power, Profile Lock/Standby, Robot Error만 반환합니다. 축별 Home/Reference 상태는 포함하지 않습니다. [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:2878)
- `Single Axis Read Status`는 각 축의 `_LMCAXIS_STATUS`를 반환합니다.
- Home 완료 여부는 `IsReferenced` 비트이며 마스크는 `0x00000002`입니다. [types.h](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/types.h:459)

즉 현재는 다음처럼 판단합니다.

```csharp
bool isReferenced = (axisStatus.State & 0x00000002u) != 0;
```

WPF는 현재 `State=0x...` 원시값만 표시하고 Home 상태를 별도로 해석해서 보여주지는 않습니다. [MainWindow.xaml.cs](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:2645)

Group 화면 모니터링은 다음 구조가 적합합니다.

- Group Members에서 9축 목록을 가져옴
- 별도의 `Group Read Axis Status` 명령으로 9축 상태를 한 번에 반환
- 화면에 `축 이름 / Power / Referenced(Home) / Standstill / Error` 표로 표시
- 기존 `GroupReadStatus` 패킷은 호환성을 위해 그대로 유지

PC에서 Single Axis Read를 9번 호출하는 방법도 가능하지만, TCP 왕복이 9번 발생하고 동일 시점의 상태가 아닙니다. 따라서 **Group 화면용 9축 일괄 상태 조회 API를 새로 추가하는 방식을 추천합니다.**

참고로 `_LMCRobotBase` 자체에는 “모든 로봇 축이 Reference 완료됐는가”를 나타내는 aggregate `IsReferenced` 비트가 있지만, 현재 TCP Group 응답에서는 읽어서 보내지 않습니다. 축별로 어떤 축이 Home 안 됐는지 보려면 결국 9축 개별 상태가 필요합니다.

> 그러면 Set Identity할때 각축 정보 얻어오는거 할 때 그 아래 Home Check라고해서 버튼 하나 눌러서 Home check하는게 맞지 않을까? 그래서 화면에 표시하고, 안되면 버튼없이 Set Identity를 호출할 때 같이 나갈 수 있게 하던가. 그렇게 수정해봐.

<details><summary>12 previous messages</summary>

> Group 화면의 Identity 설정 영역에 `Home Check` 버튼과 축별 결과 표시를 추가하고, `Set Identity` 실행 시에도 동일 검사를 자동 수행하도록 하겠습니다. 현재 Identity 계약은 X/Y/Z/U 4축이므로 이 네 입력 축을 검사하며, 하나라도 `IsReferenced=FALSE`이면 PLC에 Set Identity 명령을 보내지 않도록 구성하겠습니다.
>
> 현재 패킷 변경 없이 구현할 수 있습니다. Group member 목록의 9축 이름/descriptor를 가져온 뒤 각 축의 기존 `Read Status`를 순차 호출하고, `_LMCAXIS_STATUS.IsReferenced(0x2)`를 화면에 표시하겠습니다. 별도 `Home Check` 버튼을 제공하면서 `Set Identity`도 이 검사를 항상 다시 실행하게 하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcResults.cs` (+10 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+9 -3)
>
> </details>
>
> 추가 확인 결과 `Set Identity` 계약은 X/Y/Z/U 4축이고, 가상축 5~9는 Identity 대상이 아닙니다. 9축 전체를 Home 필수 조건으로 묶으면 의도적으로 Reference를 사용하지 않는 가상축 때문에 Set Identity가 막힐 수 있으므로, 이번 Home Check와 자동 차단은 Identity 입력 4축에만 적용하겠습니다. 결과에는 각 축 이름·reference·Home 상태를 개별 표시합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+38 -7)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+2 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+39 -14)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+126 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+11 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+2 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+12 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+65 -0)
>
> </details>
>
> PC API 테스트는 통과했습니다. 실행 중인 예제 프로그램과 Visual Studio 원격 디버거가 기존 Release EXE/DLL을 잠그고 있어 일반 Rebuild만 파일 복사 단계에서 실패했습니다. 실행 중인 프로그램은 건드리지 않고 별도 임시 출력 폴더로 WPF를 빌드해 코드/XAML 오류를 검증하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+17 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+5 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+1 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+3 -1)
>
> </details>
>
> 리뷰에서 Power Off 확인 뒤 이전 Home PASS 표시가 남을 수 있는 stale 상태를 발견해 함께 수정했습니다. `PowerOn=False`가 확인되면 Home 결과, Identity 준비, Profile Lock 준비를 모두 초기화합니다. 이제 최종 빌드와 테스트를 다시 확인합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 구현과 재검토까지 끝났습니다. 최종 파일 위치와 검증 결과만 다시 확인한 뒤, 실제 테스트 순서까지 정리해서 드리겠습니다.
</details>

수정 완료했습니다.

- Group 화면에 `Home Check (X/Y/Z/U)` 버튼과 축별 결과 표시 영역 추가
- 각 축의 `_LMCAXIS_STATUS.IsReferenced` 비트(`0x00000002`) 확인
- `Set Identity` 실행 시 Home Check를 자동으로 다시 수행
- 한 축이라도 미원점이면 Set Identity 패킷을 전송하지 않고 차단
- 축 이름 변경, 연결 초기화, Power Off 확인 시 기존 Home 결과 초기화
- 현재 Identity 대상인 X/Y/Z/U 4축만 검사하며, 가상축 5~9는 검사 대상에서 제외
- API 결과에 `IsReferenced` 속성 추가

주요 파일:

- [MainWindow.xaml](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:562)
- [MainWindow.xaml.cs](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:1281)
- [LmcResults.cs](./LMC_Library/LMC_API_Delivery/src/LmcResults.cs:65)
- [README.md](./LMC_Library/Codex_LASAL_WPF/README.md:36)

검증 결과:

- PC API 테스트: `46/46 PASS`
- WPF Release 빌드: 성공
- 코드 리뷰: 추가 결함 없음
- `git diff --check`: 오류 없음

현재 예제 프로그램이 실행 중이라 기존 `bin\Release` 파일은 잠겨 있습니다. 실행 중인 프로그램을 종료하고 다시 빌드·실행해야 새 Home Check 화면이 나타납니다. 아직 커밋은 하지 않았습니다.

> C:\work\Elmo\Elmo_Master\LMC_Library\Codex_LASAL_WPF
> 지금 프로젝트 솔루션 이름이 PmasApiWpfTestApp 인데 Pmas를 Lasal로 바꿔
> 이 프로젝트 안에 내용들 다 Pmas를 Lasal로 바꿔

<details><summary>7 previous messages</summary>

> 프로젝트·솔루션·어셈블리·네임스페이스·파일명과 내부 문자열까지 `Pmas` 계열을 `Lasal`로 일괄 정리하겠습니다. 먼저 실제 참조 범위와 Git 상태를 확인하고, 빌드가 끊기지 않도록 이름과 경로를 함께 변경하겠습니다.
>
> 확인 결과 실제 소스 내부의 PMAS 네임스페이스는 이미 없고, 남은 것은 솔루션 파일명·프로젝트 폴더/파일명·두 경로 참조와 이전 빌드/IDE 캐시입니다. 이를 `LasalApiWpfTestApp`으로 통일하고 오래된 생성 캐시는 제거하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp.sln` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+1 -1)
>
> </details>
>
> 추가로 새 경로를 소비하는 API 테스트 프로젝트 한 곳이 확인됐습니다. 이 참조까지 바꾸지 않으면 예제 앱 빌드 테스트가 깨지므로 함께 수정하고, 루트의 별도 레거시 PMAS/LASAL 프로젝트는 건드리지 않겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+1 -1)
>
> </details>
>
> 새 이름의 솔루션은 Release 재빌드에 성공했습니다. 이제 전체 API 테스트가 새 `LasalApiWpfTestApp.csproj` 경로를 실제로 따라가서 예제 앱까지 빌드하는지 확인하겠습니다.
>
> Debug와 Release 모두 새 이름으로 빌드됐고, 전체 API 테스트도 46/46 통과했습니다. 생성물까지 다시 검색했을 때 대상 프로젝트 폴더에는 `Pmas` 문자열이나 파일명이 남아 있지 않습니다.
</details>

변경 완료했습니다.

- `PmasApiWpfTestApp.sln` → [LasalApiWpfTestApp.sln](./LMC_Library/Codex_LASAL_WPF/LasalApiWpfTestApp.sln:5)
- `PmasApiWpfTestApp/` → `LasalApiWpfTestApp/`
- `PmasApiWpfTestApp.csproj` → [LasalApiWpfTestApp.csproj](./LMC_Library/Codex_LASAL_WPF/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj:1)
- [README 빌드 경로](./LMC_Library/Codex_LASAL_WPF/README.md:9) 수정
- [API 전체 테스트의 예제 프로젝트 참조](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj:59) 수정
- 기존 `.vs`, `bin`, `obj`의 PMAS 캐시 제거 후 새 이름으로 재생성

검증 결과:

- 대상 폴더 내 `Pmas/PMAS/pmas`: 0건
- Debug 빌드: 성공
- Release 재빌드: 성공
- 전체 API 테스트: `46/46 PASS`
- 정적 LASAL 계약 및 예제 앱 빌드: 성공
- 코드 리뷰: 누락 또는 깨진 참조 없음

출력 파일명과 네임스페이스는 원래부터 `LasalMotionControlApiExample`이어서 그대로 유지했습니다. 별도 레거시 프로젝트인 루트 `Codex_LASAL_WPF`와 `Codex_PMAS_WPF`는 변경하지 않았습니다. 아직 커밋은 하지 않았습니다.

> LasalMotionControlLib 참조걸때 상대경로로 참조걸수 있으면 상대경로로 걸게해줘
> 그리고 C:\work\Elmo\Elmo_Master\LMC_Library\Codex_LASAL_WPF 폴더명을 Codex_LASAL_WPF을 LasalApiWpfTestApp로 바꿔줘.
> 기존 테스트 앱은 안써서 지웠어.
>  해당 내용들 정리해서 추가된 내용들 종류별로 정리해서 커밋해서 올려

<details><summary>7 previous messages</summary>

> 요청한 폴더 이름 변경, 상대경로 참조 정리, 기존 테스트 앱 삭제 반영을 각각 확인한 뒤 목적별 커밋으로 나눠 현재 브랜치에 푸시하겠습니다. 커밋·푸시 범위를 안전하게 분리하기 위해 Git 게시 절차를 적용하되, PR은 요청하지 않았으므로 만들지 않겠습니다.
>
> 상대참조 자체는 이미 `ProjectReference`로 구성돼 있었습니다. 외부 폴더를 바꿔도 깊이가 같아 라이브러리 참조는 `..\..\LMC_API_Delivery\src\LasalMotionControlLib.csproj`로 유지할 수 있습니다. 삭제된 옛 테스트 앱을 아직 빌드하던 테스트 타깃은 제거하고, 새 예제 앱만 빌드하도록 정리하겠습니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/LMC_API/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+4 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+2 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
>
> </details>
>
> 폴더 rename과 참조 정리가 끝났습니다. 공용 DLL 참조는 솔루션·WPF 프로젝트·테스트 프로젝트 모두 상대경로이며, 삭제된 테스트 앱 빌드 타깃도 제거했습니다. 이제 새 최종 경로에서 두 구성과 전체 테스트를 다시 돌립니다.
>
> 검증은 모두 통과했습니다. 이제 커밋은 ① PC API/typed status, ② LASAL 9축·group dispatcher, ③ 새 WPF 예제, ④ 구형 테스트 앱 제거, ⑤ 문서 정합성 순서로 분리하겠습니다. 현재 `main`의 기존 미푸시 4개 커밋도 이번 푸시에 함께 올라갑니다.
>
> 목적별 5개 커밋을 만들었고, 삭제된 앱을 참조하던 빌드 경로도 새 예제로 교체했습니다. 푸시 전에 작업트리 청결 상태와 최종 커밋 목록을 확인하고 원격을 한 번 더 동기화하겠습니다.
>
> 푸시는 완료됐고 `origin/main`이 로컬 HEAD와 동일한 것도 확인했습니다. 마지막으로 커밋 분리와 참조 누락에 대한 독립 검토 결과만 받아 최종 요약하겠습니다.
</details>

요청한 정리·커밋·푸시를 모두 완료했습니다.

변경 사항:

- 외부 폴더:
  - `LMC_Library/Codex_LASAL_WPF`
  - → [LMC_Library/LasalApiWpfTestApp](./LMC_Library/LasalApiWpfTestApp)
- 새 솔루션: [LasalApiWpfTestApp.sln](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.sln:5)
- 프로젝트: [LasalApiWpfTestApp.csproj](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj:68)
- `LasalMotionControlLib` 참조는 모두 상대경로로 확인:
  - 솔루션: `..\LMC_API_Delivery\src\LasalMotionControlLib.csproj`
  - WPF 프로젝트: `..\..\LMC_API_Delivery\src\LasalMotionControlLib.csproj`
  - API 테스트: `..\..\src\LasalMotionControlLib.csproj`
- 삭제된 구형 테스트 앱의 빌드 타깃 제거
- `/t:RunTests`는 PC 테스트, LASAL 정적 계약, 새 WPF 예제만 실행하도록 수정

목적별 신규 커밋:

1. `d6f5fcf` `feat(api): complete group and typed status contract`
2. `3ccfb92` `feat(lasal): add group commands and nine-axis dispatch`
3. `c1fffb6` `feat(example): add LASAL API WPF application`
4. `a2f24ae` `chore(test-app): remove legacy WPF test application`
5. `fdf9487` `docs: align API and LASAL implementation guidance`

검증 결과:

- WPF Debug 빌드: 성공
- WPF Release 재빌드: 성공
- PC API 테스트: `46/46 PASS`
- LASAL source static contract: PASS
