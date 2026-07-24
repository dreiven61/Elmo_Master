> 네 번째 커밋은 패킷 분석과 현재 상태 문서만 넣습니다. 여기서 native PMAS와 custom LASAL의 경계, D2 PI Bulk 매핑, D5 4-byte Read-only 목표, 최신 IDE 빌드의 정확한 시점과 warning 6줄을 고정합니다.
>
> 첫 커밋 묶음은 4개로 완료했습니다. 다음 구현 우선순위인 D5를 코드까지 추적해 보니, C# API가 아니라 LASAL의 Drive client·callback·ticket 상태기계가 비어 있습니다. 이 부분은 저장소 규칙상 LASAL IDE에서 선언·Network를 만들어야 하므로 외부 텍스트로 억지로 활성화하지 않겠습니다. 대신 먼저 capability-off 상태에서도 D5 요청 형식을 명시적으로 검증하는 fail-closed dispatcher와 자동 계약을 완성하고, 4-byte 실행부가 들어갈 정확한 gate를 고정하겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+38 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs` (+15 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+9 -0)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+23 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+5 -3)
> - Wrote `./docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs` (+11 -2)
>
> </details>
>
> D5 첫 안전 증분은 구현됐습니다. 정확한 16-byte status/cancel, 32-byte SDO header와 read/write payload shape를 검증한 뒤 capability-off에서는 계속 `UnsupportedFeature`를 반환합니다. 실제 SDO는 아직 한 건도 실행하지 않으며 capability도 `0x3F/MaxSdo=0` 그대로입니다. 이제 full-network 계약과 staged diff를 확인합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> D5 안전 전제 구현은 리뷰에서도 커밋 차단 결함이 없다고 확인됐습니다. 지금 최종 diff·라인을 다시 확인한 뒤 별도 커밋으로 묶고, 남은 미구현 범위를 실제 PLC 검증이 필요한 항목과 코드 구현 항목으로 분리해 정리하겠습니다.
>
> 첫 번째 후속 구현도 `6d2c717`로 커밋했습니다. 다음으로 패킷 감사에서 실제로 드러난 PMAS Recorder의 상태/길이 검증 누락을 처리하겠습니다. 이 부분은 PLC/LASAL 선언 변경 없이 C#에서 안전하게 완결할 수 있어, 우선 코드와 테스트 가능 지점을 병렬 점검합니다.
>
> PMAS Recorder의 `uiSr`·ready buffer·`Rl`·범위 차단 로직은 새 비교 앱에 이미 들어가 있어 중복 수정하지 않았습니다. 대신 감사에서 추적성이 약하다고 나온 D5 첫 단계 시험을 `0x1000:0 / 4 byte` 전용 회귀로 분리하고 있으며, 완료 후 전체 102개 시험을 다시 돌리겠습니다.
>
> <details><summary>Edited 12 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./LMC_Library/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md` (+1 -1)
>
> </details>
>
> PMAS Recorder는 UI 내부 검증만 있던 상태에서, 재현 가능한 단위시험이 없다는 점이 남아 있었습니다. 지금 ready-mask·buffer·header·range 판정을 순수 정책 클래스로 분리하고 테스트 프로젝트를 추가한 변경이 도착했으므로, 공개 동작이 바뀌지 않는지 diff와 빌드 결과를 검토한 뒤 별도 커밋 여부를 결정하겠습니다.
</details>

전반 검토·설계 정합화와 안전하게 진행 가능한 후속 구현까지 완료하고 6개 커밋으로 분리했습니다. 현재 `main`은 `origin/main`보다 9커밋 앞서며 push는 하지 않았습니다.

커밋:

- `d138f4b` — 260721 히스토리 분할·인계 문서
- `b02ae28` — PMAS/MMCLib native 비교 앱
- `b8e80d7` — EtherCAT Health 바인딩, Recorder Stop·Download/CSV 보강
- `a320d59` — 패킷 분석·설계·현재 상태 문서 정합화
- `6d2c717` — D5 예약 명령 fail-closed 파서
- `d5061bf` — `0x1000:0` 4-byte SDO first-slice 전용 회귀시험

주요 결과:

- 제공된 23개 패킷은 모두 PMAS/MMCLib native이며 LASAL `0x7Exx` 실기 증거가 아니라는 점을 [패킷 분석 문서](/C:/work/Elmo/Elmo_Master/docs/architecture/ELMO_NATIVE_API_PACKET_CAPTURE_ANALYSIS_2026-07-21.md:11)에 명확히 기록했습니다.
- `0x7E03/04/50`은 잘못된 요청을 `BoundsInvalid`, 정상 형식도 현재는 `UnsupportedFeature`로 거부하도록 [PLC 파서](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:681)를 추가했습니다.
- capability는 의도적으로 `0x3F`, `MaxSdoDataBytes=0`을 유지했습니다. 즉 SDO/Write Policy는 중단된 것이 아니라 PLC 실행기 미구현 상태로 안전하게 비활성화되어 있습니다.
- `0x1000:0 UInt32`에서 8-byte는 송신 전 거부하고 4-byte만 wire 계약을 통과하는 [독립 시험](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs:47)을 추가했습니다.
- PMAS Recorder의 `uiSr`, ready buffer, `Rl=0`, 선택 buffer, `from <= to < Rl` 검사는 이미 구현되어 있어 중복 수정하지 않았습니다.
- 현재 상태와 다음 단계는 [남은 구현 계획](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md:109)과 [통합 상태 문서](/C:/work/Elmo/Elmo_Master/docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md:44)에 반영했습니다.

검증 결과:

- PC 자동시험 `102/102 PASS`
- LASAL source-only/full-network 정적 계약 PASS
- LASAL WPF 및 PMAS Version2 Debug/Release 빌드 PASS
- `git diff --check`, `git diff --cached --check` PASS
- 실제 PLC 다운로드와 `0x7Exx` 실기 검증은 아직 수행하지 않음

다음 구현의 실제 선행조건은 LASAL IDE에서 Drive1~4 SDO client/callback, mailbox와 ticket 실행기를 선언·연결하는 것입니다. 이후 `0x1000:0`의 성공·busy·timeout·cancel·disconnect를 PLC에서 확인한 뒤에만 capability를 `0x13F`, `MaxSdoDataBytes=4`로 올릴 수 있습니다. D4 Double, PI/SDO Write, D6는 그 이후 순서입니다.

현재 worktree에는 사용자의 LASAL IDE 배치 메타데이터와 42MB 원본 히스토리가 그대로 남아 있습니다. 실행 변경 커밋에는 섞지 않았고 삭제하지도 않았습니다. 최신 LASAL IDE 빌드는 Recorder Stop/D5 수정 전 결과이므로 PLC 다운로드 전에 Rebuild/Link와 implementation smoke test를 다시 해야 합니다.

> 지금 PMAS 라이브러리랑 LMC 라이브러리랑 어떻게 차이나는지 알려줘

<details><summary>2 previous messages</summary>

> 현재 저장소 기준으로 비교하겠습니다. 핵심은 “같은 모션 기능을 제공하느냐”보다, 실제 통신 상대·프로토콜·책임 경계가 어떻게 다른지입니다. PMAS native 호출과 현재 LASAL용 LMC 구현을 소스·패킷·문서에서 각각 대조하겠습니다.
>
> 현재 확인된 가장 큰 차이는 이겁니다. PMAS는 PC가 Elmo Maestro에 vendor API로 직접 명령하고, LMC는 PC가 SIGMATEK PLC에 우리 DINT 계약으로 명령한 뒤 PLC가 실제 축을 구동합니다. 따라서 함수 이름이 비슷해도 DLL만 바꿔 끼우는 관계가 아니며, 단위·패킷·진단·Recorder·SDO 생명주기가 모두 다릅니다.
</details>

한 문장으로 말하면:

> PMAS 라이브러리는 PC가 Elmo Maestro Controller를 직접 제어하는 vendor native SDK이고, LMC 라이브러리는 PC 요청을 SIGMATEK PLC가 받아 LASAL Motion 객체로 실행하도록 우리가 만든 별도 어댑터 SDK입니다.

둘은 함수 이름과 화면은 비슷해도 교체 가능한 DLL이 아니며 패킷도 호환되지 않습니다.

| 구분 | PMAS / `MMCLibDotNET` | LMC / `LasalMotionControlLib` |
|---|---|---|
| 실행 경로 | PC → MMCLib → Maestro Controller | PC → LMC DLL → SIGMATEK PLC → `_LMCAxis`/`_LMCRobot` |
| 공급 형태 | Elmo vendor 바이너리 `v3.0.0.7` | 이 저장소에서 개발한 C# 소스, `0.9.1-preview` |
| 통신 | Maestro native RPC | 프로젝트 전용 `LASAL-DINT v1` |
| TCP 4000 | Controller RPC 서버 | PLC `_TCPIPServer1` |
| Motion 값 | `double` controller/user unit | signed `DINT`, C#에서는 `int` |
| 단위 변환 | `x10000` 변환 없음 | DLL이 자동 변환하지 않음. 호출자가 물리값을 DINT로 변환 |
| API 실행 | 주로 동기 wrapper | 동기/비동기 API 쌍과 `CancellationToken` 제공 |
| 기능 범위 | Elmo 전체 SDK에 가까운 넓은 기능 | 현재 LASAL 프로젝트에 필요한 검증된 일부 기능 |
| 실행 주체 | Maestro firmware | Git에 포함된 LASAL PLC adapter source |

전체 연결 구조는 [현재 아키텍처 문서](/C:/work/Elmo/Elmo_Master/docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md:54)에 정리되어 있습니다.

### Motion 차이

PMAS:

```csharp
axis.MoveAbsoluteEx(double position, double velocity, ...)
```

Controller에서 사용하는 engineering unit을 그대로 전달합니다.

LMC:

```csharp
axis.MoveAbsoluteEx(int position, int velocity, ...)
```

LASAL PLC가 받을 DINT를 전달합니다. 현재 설정에서는 예를 들어 `1 mm = 10000 DINT`이며, 변환 책임은 호출자에게 있습니다. [LMC 단위 계약](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/README.md:125)

LMC의 `Async`는 통신 응답을 기다리는 동안 UI를 막지 않기 위한 것입니다. TCP 요청 자체는 한 connection에서 직렬화되며, 실제 축 이동 완료를 의미하지는 않습니다.

### Diagnostics 차이

PMAS는 각 native API를 직접 호출합니다.

- EtherCAT Health: `MMCNetwork.GetCommDiagnosticsEx`
- PI 정보/읽기: `GetPIVarInfo`, `ReadPIVar`
- Bulk: `MMCPIBulkRead`
- Recorder: `BeginRecordingEx`
- SDO: `UploadSDO`, `DownloadSDO`

LMC에는 LASAL 전용 diagnostics 계층이 추가되어 있습니다.

- `0x7E00`: capability 협상
- `0x7E10`: PLC가 수집한 EtherCAT/slave 상태
- `0x7E20`: PI Read
- `0x7E30~33`: Bulk lifecycle
- `0x7E40~49`: Recorder lifecycle
- `0x7E50~51`: SDO ticket 설계

즉 PMAS Health는 통신 경로·오류 counter 중심이고, LMC Health는 PLC에서 조합한 slave Online, EC/AL, DS402, cycle 상태 중심입니다.

### Bulk와 Recorder

PMAS Bulk는 PC의 `MMCPIBulkRead` 객체를 이용합니다. 별도의 controller-side Bulk ID나 lease/release 계약은 없습니다.

LMC Bulk는 다음 상태를 명시적으로 관리합니다.

- `SignalId`, `MapRevision`
- `BulkId`
- Configure/Status/Snapshot/Release

Recorder도 다릅니다.

- PMAS: `BeginRecordingEx → Status → Header → Download`
- LMC: `Configure → Start → Trigger → Stop → Status → Header → Chunk → Release/Adopt`

LMC는 `BootId`, `RecordId`, `BufferId`, sequence와 CRC까지 검사합니다. 대신 현재 PLC 구현은 single-bank만 활성화되어 있고 Double-bank는 꺼져 있습니다. [PMAS/LMC 기능 매핑](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF_Version2/API_MAPPING.md:30)

### SDO / PI Write

여기가 현재 가장 큰 기능 차이입니다.

PMAS:

- `UploadSDO`/`DownloadSDO`를 동기식으로 즉시 실행
- `ReadPIVar`/`WritePIVar` 직접 실행
- operation ticket이나 cancel 개념 없음

LMC:

- `SubmitSdo`
- `GetOperationStatus`
- `CancelOperation`
- `ReadSdoResultChunk`
- Write allowlist/policy

처럼 더 안전한 비동기 ticket 방식으로 설계했습니다.

하지만 현재 PLC capability는 `0x0000003F`, `MaxSdoDataBytes=0`입니다. 따라서 LMC의 SDO/PI Write API 표면은 존재하지만 PLC 실행기는 아직 비활성화되어 `UnsupportedFeature`로 종료됩니다. [현재 D5 상태](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md:179)

### 현재 완성도

PMAS:

- 선택된 native 기능에 대한 실제 패킷 23개 확보
- PI/Bulk/Recorder/`0x1000:0` 4-byte SDO 동작 근거 있음
- 전체 PMAS 기능이나 모든 motion 조합이 검증됐다는 뜻은 아님

LMC:

- PC 자동시험 `102/102 PASS`
- LASAL source/full-network 정적 계약 PASS
- 실제 custom `0x7Exx` PLC 패킷 캡처는 아직 없음
- 최신 PLC 다운로드 및 D1~D4 실기 matrix 미실시
- typed callback sender, D4 Double, D5 SDO/Write 실행기 미완성

제 판단은 다음과 같습니다.

- Maestro를 PC에서 직접 제어하려면 PMAS가 맞습니다.
- 모션의 최종 권한을 SIGMATEK PLC에 두고 PC는 상위 명령·진단만 담당하려면 LMC 구조가 맞습니다.
- PMAS는 기능 폭과 native 성숙도가 높습니다.
- LMC는 PLC 통제, capability 협상, resource identity와 fail-closed 안전성이 강점이지만 아직 production 승인 단계는 아닙니다.

> 내가 Lasal 프로그램에 EtherCAT_SDOBase Class를 추가했으니 이걸로 EtherCAT SDO 기능을 구현하는건 어떤가? 구현할 필요 없으면 안써도 되고

<details><summary>3 previous messages</summary>

> 새 `EtherCAT_SDOBase`의 실제 인터페이스와 네트워크 연결을 먼저 보겠습니다. 기존 `ECAT_DS402Base` 안의 SDO client/callback 경로와 비교해서, D5 실행기에 재사용할 가치가 있는지와 중복·제약을 판단하겠습니다. 이번에는 코드 변경 없이 구조 판단만 하겠습니다.
>
> 새 클래스 자체는 4축 Slave에 정상 연결되어 있습니다. 다만 기존 SDO 경로와 장단점이 달라서, 현재 정적 네트워크 계약이 새 `EtherCAT_Network` 분리를 받아들이는지도 확인하겠습니다. 이 결과가 “바로 재사용 가능”인지 “먼저 프로젝트 계약을 갱신해야 하는지”를 가릅니다.
>
> 중요한 점을 하나 찾았습니다. LASAL에서 새 클래스를 저장하는 과정에 `LMCDiagnosticsService.st`의 커밋된 D5 fail-closed 코드(`0x7E03/04`, `0x7E50`)가 작업본에서 다시 사라졌고, 현재 정적 계약도 그 지점에서 실패합니다. 새 SDO 클래스 문제라기보다 IDE 저장 시 외부 수정분이 덮인 상태이므로, 향후 구현 전에 이 세 hunk를 먼저 복구해야 합니다.
</details>

판정부터 말하면:

> `EtherCAT_SDOBase`를 LMC의 정식 SDO 실행기로 그대로 사용하는 것은 권장하지 않습니다. 수동 SDO 시험과 `0x1000:0` 4-byte Read smoke test 용도로는 유용합니다.

이 클래스가 쓸모없는 것은 아닙니다.

- `StartReadSDO`를 호출하고 4-byte `ParaValue`에 결과를 받습니다.
- 실제 수신 길이를 `ParaLength`에 보존합니다.
- SDO abort code를 `ErrorCode`로 제공합니다.
- 4개 객체가 Elmo 1~4에 정상 연결되어 있습니다. [네트워크 연결](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/EtherCAT_Network/EtherCAT_Network.lcn:1456)

하지만 LMC D5에 바로 사용하기에는 문제가 있습니다.

- SDO 시작 결과의 `BUSY`와 실제 `ERROR`를 모두 `ClassState=ERROR`로 합칩니다. bounded retry와 실제 오류를 구분할 수 없습니다. [시작 처리](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/EtherCAT_SDOBase/EtherCAT_SDOBase.st:278)
- 숫자 버퍼는 `DINT` 4바이트뿐입니다.
- 숫자 Write는 4-byte 주소에 검증되지 않은 `ParaLength`를 전달하므로 4바이트 초과 사용은 위험합니다. [Write 경로](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/EtherCAT_SDOBase/EtherCAT_SDOBase.st:310)
- ticket, owner session, BootId, cancel, disconnect/orphan 처리가 없습니다.
- Remote/Visualized 채널이라 수동 조작과 LMC 요청이 충돌할 수 있습니다.
- 아직 `LMCDiagnosticsService`에는 연결되지 않았습니다.

권장 구조는 다음입니다.

```text
