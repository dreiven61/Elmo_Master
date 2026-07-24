> - Wrote `./LMC_Library/README.md` (+5 -5)
>
> </details>
>
> 4축 성공 근거를 반영해 Read-only 기능은 완료 상태로 전환했습니다. 이제 저장소 전체에서 “D5 runtime 미실시”, “Slave 1~3 대기”, “gate-on download 대기” 같은 오래된 상태 문구가 남았는지 검색하고 자동시험을 다시 실행하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -4)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+2 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+4 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+1 -1)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_analysis_2026-07-22.md` (+1 -0)
>
> </details>
>
> 최종 검증도 통과했습니다: LASAL source-only/full-network 계약 PASS, PC 자동시험 103/103, WPF Release build PASS입니다. 마지막으로 문서 링크 위치와 작업트리 범위만 확인한 뒤 완료 판정을 정리하겠습니다.
</details>

문제없습니다. 현재 합의된 범위인 **4축 first-slice SDO Read 구현과 정상 경로를 완료 처리했습니다.**

| Slave | Ticket | 완료 cycle | 소요 | 결과 |
|---:|---:|---:|---:|---|
| 1 | 6 | 987507 | 43 cycles | Completed/Success |
| 2 | 7 | 990995 | 51 cycles | Completed/Success |
| 3 | 8 | 993940 | 43 cycles | Completed/Success |
| 4 | 5 | 92096 | 54 cycles | Completed/Success |

모든 축이 다음 조건을 만족했습니다.

- `0x1000:0`, UInt32, 4 bytes
- ErrorId/Detail 0
- 결과 `92 01 02 00` = `0x00020192` = 131474
- Capability `0x13F`, MaxSDO 4, BootId 5
- TCP 재전송·손실·순서 오류 없음

추가 소스 수정은 필요하지 않았습니다. 기존 shadowing 수정이 4축에서 정상 작동한 것입니다.

완료 상태를 다음처럼 정리했습니다.

- D5 first-slice SDO Read 구현: 완료
- 물리축 1~4 happy-path PLC runtime: PASS
- Ticket submit/status 및 inline result: PASS
- SDO Write, 8/12-byte, extended result: 설계 범위 밖이며 계속 capability-off
- fault/cancel/orphan과 EtherCAT mailbox frame 독립 관측: production qualification 잔여 항목

관련 설계·상태·README·backlog 문서도 일괄 갱신했습니다.

- [Slave 1~3 패킷 분석과 4축 합산 판정](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_Slave123_analysis_2026-07-22.md:9)
- [SDO 설계 완료 상태](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md:405)
- [PLC 4축 시험 기록](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md:227)
- [프로젝트 릴리스 상태](/C:/work/Elmo/Elmo_Master/docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md:128)

최종 검증도 통과했습니다.

- PC 자동시험: 103/103 PASS
- LASAL source-only/full-network 계약: PASS
- WPF Release build: PASS
- `git diff --check`: PASS
- 캡처 SHA-256 문서 대조: PASS

아직 stage/commit은 하지 않았습니다.

> SDO가 다른 인덱스에 접근할 수가 없는데? 구현이 제대로 된게 맞아?

<details><summary>3 previous messages</summary>

> 맞습니다. 지금 화면과 기존 설계는 일반 SDO가 아니라 `0x1000:0`만 허용한 “first-slice Read”일 가능성이 큽니다. UI·C# 검증·PLC allowlist 세 계층을 다시 확인해서 어디가 고정되어 있는지 정확히 짚겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 확인 결과, 단순 UI 문제는 아닙니다. 현재 구현은 UI와 PLC 서비스 정책 양쪽에서 `0x1000:0 / UInt32 / 4 bytes`로 의도적으로 제한되어 있습니다. C# SDK 검증까지 확인한 뒤 “완료 범위”와 필요한 확장 범위를 정확히 정리하겠습니다.
</details>

아니요. **임의 Index/Sub-index에 접근하는 일반 SDO 기준으로는 미완성**입니다. 제가 앞서 “SDO 기능 완료”라고 표현한 것은 잘못됐습니다. 정확히는 **Slave 1~4의 `0x1000:0`, UInt32, 4-byte Read 경로만 완료**된 상태입니다.

현재 다음 세 곳에서 고정하고 있습니다.

- UI가 Index `0x1000`, Sub-index `0`을 읽기 전용으로 고정: [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:1009)
- C# SDK가 `0x1000:0 / UInt32 / 4-byte`가 아니면 예외 처리: [LmcDiagnosticsD5Models.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs:9)
- PLC도 다른 Index/Sub-index를 `ReadDenied(detailCode=6)`로 거부: [LMCDiagnosticsService.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1665)

다행히 하부 `LMCSdoExecutor`는 전달받은 Index/Sub-index를 `EtherCAT_SDOBase`로 넘기는 구조라서, 기반 구조를 다시 만들 필요는 없습니다. [LMCSdoExecutor.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st:337)

즉, 지금까지의 캡처는 **축 1~4에서 동일한 `0x1000:0` 고정 경로가 정상**임만 증명했습니다. 일반 SDO Read 완료 증거는 아닙니다.

다음 구현은 이렇게 진행해야 합니다.

1. Index/Sub-index 편집 허용
2. C#과 PLC의 `0x1000:0` 고정 정책 제거
3. 우선 임의 Index/Sub-index의 4-byte Read 지원
4. 이후 1/2/4-byte와 UInt/Int 타입별 해석 지원
5. Write는 안전성 때문에 별도 allowlist가 준비될 때까지 계속 차단

결론적으로 사용자가 발견한 현상이 맞고, SDO Read 기능을 추가 확장해야 합니다.

> SDO Read 확장 기능 구현 진행해

<details><summary>21 previous messages</summary>

> 일반 SDO Read로 확장하겠습니다. 우선 안전하게 범위를 `임의 Index/Sub-index + 1/2/4-byte Read`로 잡고, Write는 계속 차단하겠습니다. 현재 미커밋 변경과 LASAL/C# 양쪽 계약을 먼저 확인한 뒤 UI, SDK, PLC, 테스트를 함께 맞추겠습니다.
>
> LASAL IDE 프로세스는 현재 실행 중이지 않아 외부 편집 위험은 없습니다. 기존 미커밋 변경은 이전 SDO 활성화·문서 정리분이며 그대로 보존하겠습니다. 이제 하부 `EtherCAT_SDOBase`가 실제로 반환 길이를 어떻게 제공하는지 확인해, 지원 길이를 과장하지 않고 구현하겠습니다.
>
> 하부 확인 결과 `EtherCAT_SDOBase`는 실제 반환 길이를 제공합니다. 따라서 범위를 다음처럼 확정했습니다: 임의의 비영(非零) Index와 모든 Sub-index, `Bool=1 byte`, 16-bit 타입=`2 bytes`, 32-bit/REAL 타입=`4 bytes`. 현재 공용 타입에 UInt8/Int8이 없으므로 일반 8-bit 숫자는 이번 범위에 포함하지 않고, Write는 계속 차단합니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs` (+39 -18)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Protocol.cs` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+63 -28)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+12 -9)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st` (+26 -23)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+22 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs` (+10 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs` (+41 -25)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 기존 PLC와의 호환성을 깨지 않도록 일반 SDO Read를 별도 capability로 구분하겠습니다. 현재 패치부터 다시 점검한 뒤, PLC/SDK/WPF/정적 계약/문서를 같은 규격으로 맞추고 빌드까지 확인하겠습니다.
>
> 확장 범위는 임의의 nonzero Index, Sub-index 0..255, 정확한 1/2/4-byte Read로 잡았습니다. 1-byte 객체를 실제로 읽을 수 있도록 `Bool`뿐 아니라 `Int8/UInt8/BitField8`도 SDO 전용 타입으로 추가하되, PI Catalog/Recorder의 기존 타입 범위는 그대로 유지합니다.
>
> <details><summary>Edited 11 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD1Models.cs` (+4 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs` (+9 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs` (+30 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsModels.cs` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsProtocol.cs` (+14 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+12 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+13 -11)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st` (+9 -9)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+14 -12)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
>
> </details>
>
> LASAL source 정적 계약은 통과했습니다. 이제 capability 호환 테스트와 1/2-byte 응답 테스트를 보강하기 전에 C# SDK와 WPF가 현재 상태에서 컴파일되는지 먼저 확인합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsContractTests.cs` (+36 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs` (+16 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs` (+113 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs` (+10 -1)
>
> </details>
>
> C# SDK 테스트는 103/103 통과했고, WPF도 .NET Framework용 MSBuild로 경고·오류 없이 빌드됐습니다. 다음은 문서와 활성 계약 값을 `0x213F`/일반 Read로 맞추고, LASAL 생성 메타데이터 불일치가 남는지 분리 확인하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs` (+7 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs` (+18 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs` (+3 -1)
>
> </details>
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+28 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD1ContractTests.cs` (+15 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+14 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -0)
>
> </details>
>
> 자동 검증에서 한 가지를 의도적으로 분리했습니다. `.st` 구현은 완료됐지만 공개 메서드 선언이 `TryStartRead4`에서 `TryStartRead(..., ReadLength, ...)`로 바뀌어 `Classes.lcb`는 LASAL IDE 저장 전까지 구 선언입니다. 따라서 source-only 검사는 통과하고 full 계약 검사는 지금 정확히 그 메타데이터 불일치에서 실패하도록 보강했습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+1 -1)
>
> </details>
>
> 문서는 현재 구현과 검증 경계를 맞췄습니다. 기존 `0x13F / 0x1000:0 / UInt32 4-byte` 캡처는 삭제하지 않고 legacy 실기 증거로 남겼고, 새 general-inline 기능은 아직 PLC Rebuild/download 전이므로 runtime pending으로 구분했습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 일반 SDO Read 소스 구현은 끝난 상태입니다. 지금은 문서의 현재 상태 표기와 LASAL 생성 메타데이터 불일치만 최종 점검해서, 실제로 어디까지 완료됐고 IDE에서 무엇을 한 번 갱신해야 하는지 정확히 정리하겠습니다.
>
> 검토 결과, 임의 인덱스/서브인덱스와 1·2·4바이트 타입 경로는 PLC 소스·SDK·WPF에 모두 연결되어 있습니다. 남은 결함은 구현 로직이 아니라 LASAL의 `Classes.lcb`가 아직 예전 메서드 선언(`TryStartRead4`)을 들고 있는 점 하나입니다. 최종 자동 테스트와 계약 검사를 다시 확인하겠습니다.
>
> 방금 재실행 명령은 테스트 실패가 아니라 산출물 경로를 잘못 지정해 실행 파일을 찾지 못한 것입니다. 실제 출력 경로를 찾아 같은 검증을 다시 돌리겠습니다.
>
> PC 계약은 Debug/Release 모두 104/104, LASAL source-only 계약도 통과했습니다. 전체 LASAL 계약의 실패는 예상대로 생성 메타데이터 한 건뿐이며, 소스 로직에서 추가 결함은 발견되지 않았습니다. WPF 빌드와 최종 변경 범위까지 확인하고 마무리하겠습니다.
</details>

SDO Read 확장 기능의 외부 소스 구현은 완료했습니다.

구현 범위:

- Slave: `1..4`
- Object Index: `0x0001..0xFFFF`
- Sub-index: `0..255`
- 지원 타입:
  - 1바이트: `Bool`, `Int8`, `UInt8`, `BitField8`
  - 2바이트: `Int16`, `UInt16`, `BitField16`
  - 4바이트: `Int32`, `UInt32`, `Real32`, `BitField32`
- 타입과 데이터 길이가 정확히 일치해야 요청 가능
- 기존 `0x1000:0 / UInt32 / 4-byte` PLC와의 하위 호환 유지
- SDO Write는 계속 의도적으로 차단

주요 변경 위치:

- LASAL SDO 실행기: [LMCSdoExecutor.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st:113)
- LASAL 요청 검증과 실행: [LMCDiagnosticsService.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1645)
- Capability `0x0000213F`: [TCPMotionInterface.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1055)
- PC SDK 타입/길이 정책: [LmcDiagnosticsD5Models.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs:9)
- 일반 SDO capability gate: [LmcDiagnosticsD5.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs:378)
- 편집 가능한 WPF 입력 UI: [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:1005)

검증 결과:

- SDK Debug: `104/104 PASS`
- SDK Release: `104/104 PASS`
- WPF Debug/Release: 빌드 성공
- WPF 3초 startup smoke: 성공
- LASAL source-only 계약: `PASS`
- `git diff --check`: 통과

현재 남은 필수 작업은 LASAL 생성 메타데이터 동기화입니다. `Classes.lcb`에는 아직 예전 `TryStartRead4` 선언이 남아 있어 전체 LASAL 계약 검사는 실패합니다. LASAL IDE에서 `LMCSdoExecutor` 메서드 선언을 다음과 같이 맞춰 저장하고 Rebuild해야 합니다.

```text
TryStartRead
  OperationToken : UDINT
  ObjectIndex    : UINT
