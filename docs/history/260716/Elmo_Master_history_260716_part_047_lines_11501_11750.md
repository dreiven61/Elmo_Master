>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./docs/architecture/SIGMATEK_LASAL_coding_rules.md` (+5 -1)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+37 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+23 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md` (+5 -1)
>
> </details>
>
> 정적 계약 두 종류와 PC API 회귀 테스트는 모두 통과했습니다. 현재 결과는 46개 전부 통과이며, 새 테스트가 동기/비동기 Axis 생성의 `-4` 보존과 잘못된 4바이트 성공 응답 거부까지 확인했습니다. 이제 예제 WPF를 별도 출력 폴더로 빌드해 실행 중인 프로그램과 충돌 없이 확인하겠습니다.
</details>

원인 확정했고 수정했습니다. `else`로 들어간 이유는 `$DINT`가 숫자 변환이 아니라 인접 메모리까지 4바이트로 재해석했기 때문입니다.

```text
ActiveRequest.CommandId = 8235 = 0x202B       정상
CommandID               = 73771 = 0x0001202B  Reference=1이 상위 WORD에 혼입

AxisRef                  = 786433 = 0x000C0001
                                            PayloadLength=12가 상위 WORD에 혼입
```

수정된 코드는 [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:506)입니다.

```st
CommandID := TO_DINT(ActiveRequest.CommandId);
AxisRef := TO_DINT(ActiveRequest.Reference);
Payload := TO_DINT(ActiveRequest.PayloadLength);
```

추가 반영 사항:

- 같은 `$DINT` 오용이 다시 들어오면 실패하도록 정적 계약 검사 추가
- PC API가 4바이트 오류 ACK를 `Status=1, ErrorId=-4`로 표시하도록 개선
- 동기/비동기 Axis 로드 회귀 테스트 추가
- LASAL 코딩 규칙과 오류 예방 문서에 overlay/숫자 변환 규칙 기록
- RT Task 및 네트워크 구성은 건드리지 않음

검증 결과:

- PC API 테스트: `46/46 PASS`
- LASAL source 계약: PASS
- LASAL network 계약: PASS
- WPF 예제 Rebuild: PASS
- `git diff --check`: PASS
- LASAL 추가 코드 ASCII 검사: PASS

현재 LASAL IDE가 `[RO]` 온라인 디버깅 상태이므로 다음 순서로 적용해야 합니다.

1. 디버깅과 breakpoint를 종료합니다.
2. IDE의 기존 버퍼를 저장하지 말고 외부 변경 파일을 Reload하거나 프로젝트를 다시 엽니다.
3. 506~508행이 `TO_DINT(...)`인지 확인합니다.
4. Rebuild/Link 후 PLC에 다시 Download합니다.
5. `Load Axis`를 다시 실행합니다.

정상 디버거 값은 다음이어야 합니다.

```text
CommandID = 8235
AxisRef   = 1
Payload   = 12
```

그다음 `0x202B` 분기로 진입하고 16바이트 AxisInfo 성공 응답을 반환해야 합니다. 계속 기존 값이 나오면 PLC에 이전 이미지가 실행 중인 것입니다. 아직 커밋은 하지 않았습니다.

> 이거 왜 Power On이 체크박스를 해야 실행됨?

<details><summary>1 previous message</summary>

> 예제 프로그램에 넣어 둔 소프트웨어 안전 인터록 때문입니다. 정확히 어떤 명령까지 체크박스로 묶여 있는지 현재 WPF 코드를 확인해서, 의도와 실제 동작을 구분해 설명하겠습니다.
</details>

체크박스는 API 요구사항이 아니라 예제 WPF에 넣은 안전 인터록입니다.

현재 UI 정책은 다음과 같습니다.

- `Power On`, `Reset`, 모션 명령, `Group Enable`: 체크박스 + 확인창 필요
- `Stop`, `Power Off`: 체크박스 없이 즉시 실행 가능
- 체크박스는 명령 한 번마다 자동으로 해제되는 one-shot 방식

[MainWindow.xaml.cs](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:1261)의 `ConfirmLiveCommand()`가 이를 강제합니다. `PowerOnAsync()` API 자체에는 이런 제한이 없습니다.

Power On은 드라이브에 토크가 걸릴 수 있어 실수로 누르는 것을 막으려고 넣었습니다. 다만 예제 사용성이 불편하다면 Power On만 체크박스 대상에서 제외하고 확인창만 남기는 방식이 적절합니다. 모션/Reset/Group Enable의 one-shot 안전장치는 유지하는 편이 안전합니다.

> 이거 안전장치 귀찮은데 없애버리자

<details><summary>4 previous messages</summary>

> one-shot `Arm` 체크박스를 예제 프로그램 전체에서 제거하겠습니다. Power On뿐 아니라 Reset, 모션, Group Enable도 체크 없이 실행되게 하되, 명령 확인창과 실제 상태 확인·Stop/Power Off·동작 중 연결 차단은 그대로 유지하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+7 -14)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+1 -46)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+3 -3)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+3 -1)
>
> </details>
>
> 체크박스 관련 참조는 UI·코드·README·설계 문서에서 모두 제거했습니다. 명령 확인창은 남아 있으므로 버튼을 누르면 바로 확인창이 뜨고, 승인하면 전송되는 흐름입니다. 지금 XAML과 Release 빌드를 검증하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+6 -12)
>
> </details>
</details>

one-shot Arm 체크박스를 완전히 제거했습니다.

- Power On, Reset, 모든 모션, Group Enable이 체크 없이 실행됩니다.
- 명령 전 Yes/No 확인창은 유지했습니다.
- Stop/Power Off와 동작 상태 감시 기능은 그대로입니다.
- 관련 README와 설계 문서도 수정했습니다.

검증 결과:

- XAML 파싱: PASS
- Debug Rebuild: PASS
- Release Rebuild: PASS
- Arm 관련 잔여 참조: 0건
- `git diff --check`: PASS

변경 파일은 [MainWindow.xaml](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:153)과 [MainWindow.xaml.cs](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:1247)입니다.

현재 실행 중인 예제 프로그램에는 반영되지 않으므로 종료한 뒤 다시 빌드·실행해야 합니다. 아직 커밋은 하지 않았습니다.

> jerk 쓸수도 있으니 jerk 입력받을수 있게 열어놔. plc에서도 jerk 받을수 있게 해놓고, 없으면 반영해.

<details><summary>12 previous messages</summary>

> WPF의 Jerk 입력을 열고, DINT 변환·패킷 offset·LASAL 파싱·실제 `_LMCAxis` 호출 인자까지 끝까지 추적하겠습니다. PLC 메서드가 Jerk를 실제로 받는 경로가 있으면 연결하고, 없다면 성공처럼 보이게 두지 않고 정확한 지원 방식으로 반영하겠습니다.
>
> 확인 결과 PLC 쪽 Jerk 수신은 이미 구현돼 있습니다. `0x209F/0x20A0`의 offset 24, `0x20A2`의 offset 20, Stop의 offset 12를 읽어 네 축의 `Jerk:=jer`로 전달합니다. 실제 차단 지점은 WPF의 읽기 전용 입력과 `JerkRaw=0` 강제였으므로 그 부분을 열고, PLC 전달 계약 검사를 보강하겠습니다. 현재 축 프로파일이 `_RAMP_PROFILE`이라 nonzero Jerk의 실제 궤적 적용은 `_JERK_PROFILE` 전환 전까지 비활성이라는 점도 화면에 명시하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+2 -4)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+41 -5)
>
> </details>
>
> 현재 파일을 다시 읽어보니 작업 도중 LASAL Network 상태가 바뀌었습니다. 처음 점검 때는 `_RAMP_PROFILE`이었지만 지금 디스크의 `_LMCAxis1..4`는 모두 `_JERK_PROFILE`이고 `JMax=75000 mm`로 저장돼 있습니다. 사용자 IDE에서 갱신된 변경일 가능성이 있으므로 이 네트워크 파일은 제가 덮어쓰지 않고, 현재 저장 상태를 Git 기준과 교차 확인하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> PLC 쪽은 이미 Jerk를 패킷에서 읽어 `_LMCAxis` 호출까지 전달하고 있습니다. 지금은 WPF 입력을 활성화한 변경을 정리하고, 해당 바이트 오프셋이 다시 빠지지 않도록 정적 검증과 매뉴얼을 맞춘 뒤 빌드까지 확인하겠습니다.
>
> 현재 저장된 Motion Network도 확인했습니다. `_LMCAxis1..4`가 모두 `_JERK_PROFILE`이고 `JMax`가 0이 아니므로, Jerk가 단순히 패킷에만 들어가고 무시되는 설정은 아닙니다. 이 조건과 PLC의 `[12]/[20]/[24]` 파싱 오프셋을 자동 계약 검사에 고정하겠습니다.
>
> <details><summary>Edited 10 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+37 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+13 -5)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+9 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+6 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+9 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+15 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md` (+29 -20)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+7 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+15 -7)
>
> </details>
>
> 정적 계약 검사는 통과했습니다. 즉 현재 소스 기준으로 Stop은 payload `[12]`, Absolute/Relative는 `[24]`, Velocity는 `[20]`에서 Jerk를 읽고, 4개 축의 모든 해당 `_LMCAxis` 호출에 그대로 전달됩니다. 이제 WPF Debug/Release와 전체 PC 패킷 테스트를 돌리겠습니다.
>
> 병렬 빌드 세 개가 공용 `obj\Release` DLL을 동시에 열어 `CS2012`가 났습니다. 코드 오류가 아니라 검증 실행 간 파일 잠금 충돌이므로, 같은 항목을 순차로 다시 실행하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+8 -8)
>
> </details>
>
> PC 빌드와 패킷 테스트도 모두 통과했습니다. WPF는 Debug/Release 둘 다 성공했고, API golden/response/RPC 테스트는 46/46 통과했습니다. 남은 것은 LASAL IDE에서 현재 Network를 Rebuild/Link한 뒤 PLC에 다운로드하고 실제 축에서 작은 값으로 확인하는 단계입니다.
</details>

Jerk 입력 및 PLC 전달 경로 반영을 완료했습니다.

- WPF Jerk 입력 활성화: [MainWindow.xaml](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:291)
- Absolute/Relative/Velocity/Stop 모두 Jerk 입력 사용
- `0` 허용, 음수·DINT 초과값 거부: [MainWindow.xaml.cs](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:1406)
- 변환식: `Jerk DINT = (물리 jerk / 1000) × UNIT`
  - 예: `1000 mm/s³` → UI에 `1` 입력 → UNIT `10000` → `10000 DINT`
- PLC는 기존에도 Jerk를 정확한 offset에서 읽어 모든 4축 `_LMCAxis` 호출에 전달하고 있었습니다: [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1247)
- 현재 저장된 축 설정은 `_JERK_PROFILE`, `JMax=75000 mm`입니다: [Motion_Network.lcn](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/Motion_Network.lcn:28)
- 해당 설정과 Jerk offset/전달 경로가 빠지면 실패하도록 정적 계약 검사를 추가했습니다: [Verify-LasalContract.ps1](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1:221)
- 단위 매뉴얼과 예제도 갱신했습니다: [UNIT_CONVERSION_MANUAL_2026-07-10.md](./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md:25)

검증 결과:

- WPF Debug/Release 빌드 성공
- PC API 테스트 46/46 통과
- LASAL SourceOnly/전체 정적 계약 통과
- `BasicUsage.cs` 컴파일 성공
- `git diff --check` 통과

이제 LASAL IDE에서 Rebuild/Link 후 PLC에 다운로드하고, 작은 Jerk 값으로 실제 축 시험하면 됩니다. 커밋은 아직 하지 않았습니다.

> 예제 프로그램 안전용으로 메세지박스 띄우는거 너무 귀찮은데 걍 없앱시다

<details><summary>4 previous messages</summary>

> Power On, Reset, 이동 명령, Group Enable 전에 뜨는 안전 확인창만 제거하겠습니다. 오류·연결 실패를 알리는 메시지박스는 진단에 필요하므로 유지하고, 버튼 클릭 즉시 명령이 전송되도록 코드와 문서를 같이 맞춘 뒤 빌드하겠습니다.
>
> 명령 전 확인창뿐 아니라, 동작 불확실 상태에서 창을 닫을 때 뜨던 확인창도 제거하겠습니다. 겹치는 motion을 막는 내부 상태 검사는 유지하고, 종료 시 자동 Stop은 보내지 않으며 경고 로그만 남기고 닫도록 하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+4 -4)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+11 -90)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+5 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
>
> </details>
>
> 확인창 호출과 관련 helper는 제거됐고, 기존의 중복 motion 차단·입력 검증·Power 상태 검사는 그대로 남겼습니다. 이제 실제 컴파일에서 이벤트 핸들러와 종료 경로가 깨지지 않았는지 확인하겠습니다.
</details>

안전 확인용 MessageBox를 전부 제거했습니다.

- Power On, Reset, Absolute/Relative/Velocity, Group Enable은 상태·입력 검사 후 즉시 전송됩니다.
- 동작 중복 방지 로직은 유지했습니다.
- motion 진행 가능성이 있어도 종료 확인창 없이 닫힙니다. 자동 Stop은 전송하지 않습니다.
- 화면 상단에는 즉시 전송된다는 고정 경고만 표시합니다: [MainWindow.xaml](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:166)
- 명령 처리 코드: [MainWindow.xaml.cs](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:261)

검증 결과:

- MessageBox 사용 0건
- Debug/Release 빌드 성공
- XAML 파싱 및 diff 검사 통과
- 문서 반영 완료

아직 커밋하지 않았습니다.

> 이거 1번은 모든 기능이 정상 동작하는데 2번~4번은 서보온 오프만 되고 나머지 구동에 관련된 동작이 안되는데?\
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-632c110f-cce1-4c99-8f0f-ffe64de94c9e.png`
