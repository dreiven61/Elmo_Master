
- `CapabilityBits=0x3F`
- `MaxSdoDataBytes=0`
- `LMC_DIAG_D5_SDO_READ_ENABLED=FALSE`

남은 것은 최신 implementation smoke와 PLC 다운로드 후 실제 `0x1000:0` SDO runtime 검증입니다. 푸시는 하지 않았으며 현재 `main`은 `origin/main`보다 16개 커밋 앞서 있습니다.

> 테스트 프로그램에 SDO 기능이 미구현중인건가? 미구현된 부분 있으면 구현해줘

<details><summary>3 previous messages</summary>

> 테스트 프로그램의 SDO 탭을 현재 소스 기준으로 다시 확인하겠습니다. UI 버튼만 있는 상태인지, SDK 호출·상태 조회·취소까지 연결됐는지, PLC capability가 꺼져 있을 때의 표시가 적절한지까지 점검하고 실제 누락분은 구현한 뒤 빌드와 계약 테스트로 검증하겠습니다.
>
> 현재 작업 트리에 LASAL IDE가 다시 만든 것으로 보이는 `Class/Classes.lcb` 미커밋 변경이 하나 있습니다. 이번 테스트 프로그램 작업에서는 이 파일을 건드리거나 커밋하지 않고 그대로 보존하겠습니다.
>
> 감사 결과, WPF/SDK의 first-slice SDO Read 흐름은 이미 구현돼 있습니다. 비활성 원인은 PLC가 `SDORead`를 광고하지 않는 것입니다. 실제 시험 가능 상태로 만들려면 PLC gate, capability bit와 `MaxSdoDataBytes`를 함께 열어야 하는데 현재 LASAL이 실행 중이고 `Classes.lcb`도 변경 상태라 외부 `.st` 편집은 보류했습니다.
</details>

결론부터 말하면 테스트 프로그램의 first-slice SDO Read는 이미 구현돼 있습니다.

- Submit/Refresh/Cancel/결과 저장: [MainWindow.Diagnostics.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:828)
- 입력과 버튼: [MainWindow.xaml](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:981)
- 현재 지원 범위: 축 1~4, `0x1000:0`, UInt32 4-byte Read

버튼이 비활성인 원인은 PLC 쪽입니다.

- SDO gate가 `FALSE`: [LMCDiagnosticsService.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:216)
- Capability가 `0x3F`: [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1054)
- `MaxSdoDataBytes=0`: [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1045)

실기 시험을 위해서는 세 값을 동시에 다음처럼 변경해야 합니다.

```text
LMC_DIAG_D5_SDO_READ_ENABLED = TRUE
CapabilityBits               = 0x0000013F
MaxSdoDataBytes              = 4
```

다만 현재 LASAL이 실행 중이고 `Classes.lcb`에도 미커밋 변경이 있습니다. 이전에 정한 규칙대로 외부 `.st` 편집을 진행하지 않았습니다. LASAL을 저장하고 완전히 종료한 뒤 알려주세요. 그러면 gate/capability, 정적 계약, 문서를 함께 수정하고 PC 테스트와 WPF 빌드까지 진행하겠습니다.

SDO Write와 4바이트 초과 Extended Read는 실제 미구현 범위지만, 현재 검증된 Read-only 범위와 별개이며 write allowlist와 상태 제한이 필요하므로 이번 활성화에는 포함하지 않겠습니다.

> lasal 종료했으니 편집ㅂ 진행해

<details><summary>13 previous messages</summary>

> LASAL 종료 상태와 현재 미커밋 파일을 먼저 확인한 뒤, first-slice SDO Read 시험용 gate·capability·MaxSDO를 한 세트로 변경하겠습니다. 사용자 `Classes.lcb` 변경은 그대로 보존하고 이번 수정 범위에 포함하지 않겠습니다.
>
> first-slice Read만 시험 활성화합니다. SDO Write, 8/12-byte Read와 `0x7E51` extended 결과는 executor·allowlist 범위를 벗어나므로 계속 차단합니다. Capability는 BootId가 유효한 경우에만 `0x13F / MaxSDO=4`가 되도록 fail-closed 구조를 유지하겠습니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -3)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+1 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+5 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+11 -9)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+13 -10)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+30 -24)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+26 -23)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+32 -29)
>
> </details>
>
> Capability wire 값이 바뀌므로 테스트 화면 문서뿐 아니라 `DINT_PACKET_MAP`과 내부 PLC 시험 가이드의 기대값도 같이 맞추겠습니다. 과거 캡처 분석의 당시 baseline은 이력으로 남기되, 현재 source 상태를 설명하는 문서는 stale 값이 없도록 정리합니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+12 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+5 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+24 -17)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+39 -19)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> LASAL이 닫힌 상태를 다시 확인한 뒤, 방금 활성화한 SDO Read 시험 경로와 문서의 남은 불일치를 정리하겠습니다. 사용자 변경인 `Classes.lcb`는 건드리지 않고, 소스·정적 계약·C# 빌드까지 검증한 다음 LASAL에서 Reload/Rebuild할 정확한 항목만 넘기겠습니다.
>
> 확인 결과 WPF의 SDO 화면 자체는 이미 구현돼 있었고, 이번 편집의 핵심은 PLC 시험 프로필의 세 계약값을 동시에 여는 것입니다: `D5 gate=TRUE`, `CapabilityBits=0x13F`, `MaxSdoDataBytes=4`. 이제 과거 `0x3F/SDO off` 상태를 “현재”라고 적은 문서만 선별해서 고치고 있습니다.
>
> <details><summary>Edited 9 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+15 -15)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+29 -25)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_NATIVE_API_PACKET_CAPTURE_ANALYSIS_2026-07-21.md` (+4 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+20 -17)
> - Wrote `./docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md` (+10 -7)
> - Wrote `./LMC_Library/LMC_API/README.md` (+5 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+3 -2)
> - Wrote `./LMC_Library/README.md` (+6 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md` (+4 -4)
>
> </details>
>
> 첫 정적 검증은 통과했습니다. 현재 계약은 연결 끊김/BootId 0에서는 기존처럼 `MaxSDO=0`으로 닫히고, 정상 BootId일 때만 `0x13F/4 bytes`를 광고합니다. 이제 103개 PC 회귀와 WPF 빌드를 실제로 다시 돌려 문서 수치까지 확정하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -2)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+2 -1)
>
> </details>
>
> 독립 검토에서도 계약 결함은 나오지 않았습니다. 단 하나, capability 코드 주석이 D2/D3만 언급해 현재 D4/D5 조건과 어긋나던 부분을 D2–D5로 바로잡았습니다. 실기 전 마지막 정적 검증을 한 번 더 마무리합니다.
</details>

편집 완료했습니다. 테스트 프로그램의 SDO Read UI/SDK는 이미 구현돼 있었고, PLC 시험 경로만 활성화했습니다.

- [LMCDiagnosticsService.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:216): D5 Read gate `TRUE`
- [TCPMotionInterface.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1055): 정상 BootId에서 `CapabilityBits=0x13F`
- [TCPMotionInterface.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1061): `MaxSdoDataBytes=4`
- 연결 끊김이나 BootId 0에서는 capability와 MaxSDO가 0인 fail-closed 동작 유지
- 관련 계약 테스트와 현재 상태 문서도 함께 갱신

검증 결과:

- PC 회귀: `103/103 PASS`
- WPF Debug/Release 빌드: PASS
- LASAL source/network 정적 계약: PASS
- `git diff --check`: PASS
- 변경 LASAL 소스: 7-bit ASCII
- 사용자 변경 [Classes.lcb](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb)은 수정·스테이징하지 않았습니다.
- 아직 커밋하지 않았습니다.

이제 LASAL에서 다음 순서로 확인하면 됩니다.

1. 프로젝트를 열고 외부 소스 변경을 디스크 기준으로 Reload합니다.
2. `LMCDiagnosticsService`, `TCPMotionInterface`를 확인합니다.
3. Rebuild/Link를 실행합니다.
4. 빌드가 성공하면 PLC에 다운로드합니다.
5. WPF에서 `Refresh Capabilities` 후 `Bits=0x0000013F`, `MaxSDO=4`, nonzero BootId를 확인합니다.
6. `SDO / Write Policy`에서 축 1~4, `0x1000:0`, UInt32, 4 bytes로 `Submit SDO Read → Refresh Ticket`을 시험합니다.

실기 절차는 [PLC 시험 가이드](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md:178)에 정리했습니다.

SDO Write, 8/12-byte Read, extended result와 PI Write는 안전 allowlist와 실기 검증이 없으므로 계속 비활성입니다. LASAL 빌드 결과를 주시면 로그 확인 후 커밋까지 정리하겠습니다.

> 이거보면 편집을 못하는데 왜 그런거야?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-7164d683-6bae-455e-acb2-386f2eca9c2c.png`

<details><summary>3 previous messages</summary>

> 화면 고장이 아니라 현재 SDO “first slice” 정책 때문에 일부 입력을 의도적으로 잠근 상태로 보입니다. 소스의 활성화 조건과, 화면에 나온 `TimedOut` 결과까지 같이 대조해 정확히 구분하겠습니다.
>
> 화면 잠금은 의도된 정책이 맞습니다. 그런데 하단의 `SubmitCycle=CompletionCycle` 즉시 timeout은 별개이며, 소스에서 실제 결함도 확인됐습니다. 요청값과 보관 필드가 대소문자만 다른 이름이라 LASAL에서 자기 자신에게 대입되어 timeout 값이 0으로 남는 구조입니다. 공식 ST 식별자 규칙까지 교차 확인 중입니다.
>
> <details><summary>Searched the web</summary>
>
> Searched the web for `site:help.plc.abb.com IEC 61131 identifiers not case sensitive variable scope local global ...`
>
> </details>
</details>

편집이 안 되는 것은 의도적으로 잠가 둔 것입니다. 현재 화면은 범용 SDO 도구가 아니라 “first-slice 검증용”입니다.

현재 허용 범위:

- 편집 가능: Slave `1..4`, Timeout
- 고정: Read, `0x1000:0`, UInt32, 4 bytes
- 미지원: 임의 Object, SDO Write, 8/12-byte, extended result
- PI Write: capability bit 7과 allowlist가 꺼져 있어 비활성

이 잠금은 [MainWindow.Diagnostics.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:1255)에서 명시적으로 설정하고, 값을 강제로 풀어도 [first-slice 검증](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:1980)에서 거부합니다.

버튼 상태도 정상입니다.

- `Download Result`: 현재 4-byte 결과는 Ticket에 inline으로 오므로 사용하지 않음
- `Save Result`: 성공 결과가 없어서 비활성
- `Cancel Ticket`: 이미 `Expired`인 terminal ticket이라 취소 불가
- `Submit PI Write`: Write capability와 allowlist가 꺼져 있어 비활성

다만 하단의 timeout은 별도 코드 결함입니다.

화면에 `SubmitCycle=58941`, `CompletionCycle=58941`로 같은 cycle에 즉시 timeout 됐습니다. 원인은 [LMCDiagnosticsService.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:947)의 로컬 변수들이 클래스 멤버와 대소문자만 다르기 때문입니다.

```st
SdoTimeoutCycles := sdoTimeoutCycles;
```

IEC 61131-3 식별자는 대소문자를 구분하지 않고, 동일 이름이면 로컬 변수가 우선합니다. 따라서 이것은 멤버 대입이 아니라 로컬 자기 대입입니다. [ABB 식별자 규칙](https://help.plc.abb.com/_san_rule_sa0029.html), [ABB 변수 범위 설명](https://help.plc.abb.com/variables_overview.html)

결과적으로 멤버 Timeout이 0으로 남아 [timeout 판정](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:733)에서 `0 >= 0`이 되어 즉시 `Expired`, `0x05040000`이 발생했습니다. Slave/Index/SubIndex도 같은 문제가 있습니다.

수정할 때는 로컬 네 개를 다음처럼 완전히 다른 이름으로 바꾸고 정적 계약 테스트도 보강해야 합니다.

```text
requestSdoSlaveReference
requestSdoObjectIndex
requestSdoSubIndex
requestSdoTimeoutCycles
```

즉, 편집 불가는 현재 제한 설계이고, 즉시 timeout은 실제 버그입니다.

> 1,2,3,4 바꿔가면서 한건데 어떤거같냐? 제대로 동작중임?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-abfbe5fd-4b47-4d7c-9d7a-aecd3a078420.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-3eb7f0d5-3f8b-49fa-8fba-de46f065a05f.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-fb2cbe4e-00ac-48dc-ad73-6354c4915baf.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-3b460c23-1fab-4a11-bc04-7405cffdd454.png`

<details><summary>1 previous message</summary>

> 현재 네 장은 축 1~4 모두 “요청 접수(Queued)”까지는 성공했다는 증거입니다. 하지만 실제 SDO Read 성공 결과는 아직 화면에 없어서, 각 ticket의 terminal 상태와 현재 PLC 소스의 shadowing 수정 여부를 같이 확인하겠습니다.
</details>

아직 “제대로 SDO Read가 됐다”고 볼 수 없습니다. 네 장 모두 축 1~4의 요청이 PLC에 접수되어 `Queued` ticket이 발급된 상태까지만 보여줍니다.

확인된 정상 부분:

- Ticket ID가 `5 → 6 → 7 → 8`로 증가
- Queued cycle도 증가
- BootId가 모두 `0x00000004`로 동일
- 축 1~4 요청 모두 RPC 오류 없이 접수됨
- 현재 Queued 상태라 `Submit`은 꺼지고 `Refresh/Cancel`이 켜지는 것도 정상

하지만 로그의 `Submit SDO Read PASS`, `Refresh Diagnostics Operation PASS`는 RPC 호출이 예외 없이 끝났다는 뜻일 뿐입니다. [RunOperationAsync](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:1603)는 내부 SDO 결과와 관계없이 API 호출이 반환되면 `PASS`를 기록합니다.

실제 성공 화면은 다음과 같아야 합니다.

```text
State=Completed, Outcome=Success
ErrorId=0, Detail=0x00000000
ResultType=UInt32, ResultLength=4, Data=xx xx xx xx
```

현재 네 화면에는 모두 다음 문구만 있습니다.

```text
Refresh the ticket until it reaches a terminal state.
```
