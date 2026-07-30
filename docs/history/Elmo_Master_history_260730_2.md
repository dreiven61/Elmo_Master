# Find Encoder Multiturn 리셋 방법

> 지금 Elmo 드라이브에 Encoder Multiturrn Error가 났는데 Sigmatek 제어기로 리셋을 시킬 수 있을까?

<details><summary>6 previous messages</summary>

> 로컬 Elmo 매뉴얼과 현재 SIGMATEK/LASAL 소스에서 두 가지를 나눠 확인하겠습니다: 제어기가 실제로 보내는 Fault Reset 경로, 그리고 `Encoder Multiturn Error`가 원인 제거 없이 리셋 가능한 오류인지입니다. 하드웨어 상태를 확인하지 않고 “리셋하면 된다”고 단정하지 않겠습니다.
>
> 현재 프로젝트 쪽은 확인됐습니다. 축 `Reset()` 명령이 LASAL의 `QuitError()`로 들어가고, 최종적으로 EtherCAT PDO `0x6040`의 DS402 `ResetFault`(bit 7)를 올리는 경로가 실제로 연결돼 있습니다. 이제 이 특정 엔코더 오류가 그 명령에 응답하는 조건을 확인 중입니다.
>
> <details><summary>Searched the web</summary>
>
> Searched the web for `site:elmomc.com "Encoder Multiturn Error" ...`
>
> Searched the web for `"Encoder Multiturn Error" ...`
>
> Searched the web for `site:elmomc.com "EE[1]" multiturn ...`
>
> Searched the web for `site:elmomc.com/wp-content/uploads "MAN-G-CR" ...`
>
> Searched the web for `'TW[20]'`
>
> Searched the web for `"0x20FC" "sub index 2" Elmo ...`
>
> Searched the web for `'EE – Expanded Error'`
>
> Searched the web for `Elmo EE[1] 0x2080 ...`
>
> Searched the web for `Elmo Gold drive firmware release notes TW[20] latest ...`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 프로젝트의 Reset은 실제로 DS402 Fault Reset(bit 7)까지 전달됩니다. 다만 이 오류는 제조사별 엔코더 상태 비트일 수 있어, 일반 Reset과 별도의 오류 래치 해제를 구분해 최종 확인 중입니다.
>
> <details><summary>Searched the web</summary>
>
> Searched the web for `site:elmomc.com "TW[20]" "0x20FC" ...`
>
> Searched the web for `"Specific serial and incremental sensor errors must be manually cleared" ...`
>
> </details>
</details>

결론: **일반 Fault Reset은 SIGMATEK 제어기로 가능합니다. 하지만 Encoder Multiturn Error는 일반 Reset만으로 안 풀릴 수 있습니다.**

현재 소스 기준 경로는 다음과 같습니다.

- PC/WPF `Reset` → `LMCAxisN.QuitError()`
- `Elmo_n1.AxErrorQuit`
- DS402 `0x6040 Controlword bit 7 = Fault Reset`

실제 구현은 [ECAT_DS402Base.st](</C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/ECAT_DS402Base/ECAT_DS402Base.st:650>)에서 확인됩니다. 약 1초 후에도 Fault가 남아 있으면 Reset을 종료하고 오류를 다시 표시합니다.

현장에서는 다음 순서가 안전합니다.

1. Servo Off 후 축과 하중을 고정합니다. 특히 수직축은 낙하 방지가 먼저입니다.
2. 리셋 전에 `MF`, `EE[1]`, `0x603F`, `0x6041`, 드라이브 펌웨어와 엔코더 모델을 기록합니다.
3. SIGMATEK의 축 Reset을 한 번 수행합니다.
4. 응답 성공만 보지 말고 `0x6041 bit 3`이 실제로 해제됐는지 확인합니다.
5. 오류가 재발하면 반복 Enable/Reset하지 말고 배터리, 엔코더 케이블·커넥터, 엔코더 자체 오류를 확인합니다.
6. 배터리 손실이 의심되면 현재 멀티턴 절대위치를 신뢰하지 말고 기계 기준으로 위치를 검증하거나 재원점 설정해야 합니다. 로컬 Elmo 문서도 배터리 손실 시 절대위치가 틀릴 수 있다고 명시합니다. [로컬 오류표](</C:/work/Elmo/Elmo_Master/output/pdf/maestro_api_md/chunks/008_p0136-p0173_4.6-NC-Profiler-Error-IDs.md:1236>)

일부 EnDat 2.2, Panasonic, Tamagawa, Sanyo/Nikon 엔코더는 오류 래치 해제를 위해 `TW[20]`, 즉 `0x20FC:02 = feedback socket 번호`가 먼저 필요할 수 있습니다. 그 후 DS402 Fault Reset을 수행합니다. 단, 이는 펌웨어와 엔코더별 조건이므로 확인 없이 쓰면 안 됩니다. `1`은 축 번호가 아니라 **Elmo feedback socket 1**을 뜻합니다. [Elmo Gold 펌웨어 릴리스 노트](https://inelso.ru/upload/NEWS/Elmo%20Gold%20Drive%20Firmware%20Release%20Notes%20Version%201.1.11.00%20B00.pdf), [Gold Line Command Reference](https://manuals.plus/m/3d0043b1f6d044b97186d20238570672fc7804b0ffcff9e4b8c08df532e280b0.pdf)

현재 프로젝트의 Reset 버튼은 일반 DS402 Reset만 보냅니다. 원격 SDO Write는 현재 차단되어 있어 `0x20FC:02` 전용 해제까지는 수행하지 않습니다. [LmcDiagnosticsD5Models.cs](</C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs:429>)

드라이브 모델·펌웨어, 엔코더 제조사/모델, EAS의 `EE[1]` 값을 알려주면 일반 Reset으로 끝나는 경우인지 `0x20FC:02`가 필요한 경우인지 정확히 좁힐 수 있습니다. 또한 현재 설명은 저장소 소스 기준이며, 이 버전이 실제 PLC에 다운로드됐는지는 별도 확인이 필요합니다.

> Elmo Gold Twitter이고 Object를 접근해서 리셋이 가능했으면 하는데 없을까

<details><summary>10 previous messages</summary>

> 있습니다. 후보는 `TW[20]`에 대응하는 `0x20FC:02`입니다. 다만 Gold Twitter의 실제 펌웨어·엔코더 소켓에 해당 객체가 노출되는지와, 현재 LASAL에서 안전하게 쓰는 경로를 바로 대조하겠습니다.
>
> <details><summary>Searched the web</summary>
>
> Searched the web for `'TW[20]'`
>
> </details>
>
> 설치된 Elmo ESI에서도 확인됐습니다. `0x20FC:02`는 `UINT(16-bit)`, write-only인 `Reset EnDAT Error`이고, 같은 객체의 `0x20FC:01`은 멀티턴 위치 자체를 지우는 별도 명령이라 사용하면 안 됩니다.
>
> <details><summary>Searched the web</summary>
>
> Searched the web for `Elmo Gold "0x20FC" "Absolute Sensors Functions" ...`
>
> </details>
>
> 구현 방향도 확인됐습니다. 이 객체는 2바이트 SDO이므로 기존 `AddASyncEntryDS402()`의 최소 4바이트 경로를 그대로 쓰면 안 되고, 축별 `LMCSdoExecutor`에 제한된 2바이트 one-shot write를 추가하는 방식이 맞습니다. 일반 SDO Write 전체 개방은 필요 없습니다.
>
> 정확한 객체 정의가 PDF 매뉴얼과 ESI에 걸쳐 있어 PDF 검증 절차를 적용합니다. 문서 버전과 페이지를 직접 맞춰, 구버전 릴리스 노트와 최신 로컬 매뉴얼의 적용 범위가 다른 부분을 구분하겠습니다.
>
> <details><summary>Searched the web</summary>
>
> Searched the web for `site:elmomc.com "MAN-G-DS301" "2.008" "0x20FC" ...`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Gold Twitter 기준으로는 `TW[20]`을 SDO로 호출하는 경로가 맞습니다. 다만 엔코더 종류에 따라 `0x20FC:02`와 범용 alias `0x3204:20` 중 적용 범위가 달라서, 그 경계와 현재 LASAL 구현 가능 여부를 마지막으로 대조하고 있습니다.
>
> <details><summary>Deleted 2 files</summary>
>
> - Deleted `./tmp/pdfs/MAN-G-CR.txt` (+0 -17609)
> - Deleted `./tmp/pdfs/MAN-G-DS301.txt` (+0 -9219)
>
> </details>
</details>

있습니다. Gold Twitter에서는 `TW[20] = Feedback Socket 번호`를 CoE SDO 객체로 실행하면 됩니다.

| 적용 대상 | Index / SubIndex | 데이터 |
|---|---|---|
| Panasonic, Tamagawa, Yaskawa, Sanyo/Nikon 등 | `16#3204 / 20(dec)` (`SubIndex=16#14`) | `UDINT`, 4바이트, 값 `1..4` |
| EnDat 2.2 전용 객체 | `16#20FC / 2` | `UINT`, 2바이트, 값 `1..4` |

`0x3204`는 `TW` 명령의 Elmo alias 객체입니다. ESI 목록에 없어도 raw SDO로 접근할 수 있으며, 펌웨어에서 alias가 지원되지 않으면 SDO Abort가 반환됩니다. [TW alias 정의](<C:/work/Elmo/NetHelp/Elmo Software NetHelp for Web/NetHelp/Documents/twwizardinternalidentification.htm:16>), [SDO alias 객체 규칙](<C:/work/Elmo/NetHelp/Elmo Software NetHelp for Web/NetHelp/Documents/objects0x3000to0x32a3elmoparametersobjects.htm:11>)

예를 들어 Tamagawa 엔코더가 EAS의 Virtual Socket 1에 설정되어 있다면:

```text
Index     = 16#3204
SubIndex  = 20       // decimal, hexadecimal 16#14
Data      = UDINT#1
Length    = 4
```

주의사항:

- 실행할 때 모터는 반드시 OFF 상태여야 합니다.
- Socket 번호는 축 번호가 아니라 EAS에 설정된 Virtual Feedback Socket `1..4`입니다. Gold Twitter는 여러 피드백 프로토콜을 지원하므로 드라이브 모델만으로 객체를 하나로 확정할 수 없습니다. [Gold Twitter](https://www.elmomc.com/product/gold-twitter/), [Virtual Socket 설명](https://www.elmomc.com/capabilities/servo-technology/advanced-feedback-technology/simplicity-in-feedback-configuration/)
- `0x20FC:01` 또는 `TW[19]`는 오류 리셋이 아니라 절대 엔코더의 Multi-turn 위치를 초기화하는 명령입니다. 의도 없이 사용하면 안 됩니다.
- Yaskawa는 매뉴얼상 `TW[20]` 실행 시 오류뿐 아니라 Multi-turn도 같이 초기화될 수 있습니다.
- 오류 플래그를 지워도 유실된 절대 위치가 복원되는 것은 아닙니다. 배터리·배선·카운터 오류 원인을 제거하고 실제 위치를 확인해야 합니다.

권장 실행 순서는 다음입니다.

1. 축 Operation Disable 및 기계 하중 안전 확보
2. `TW[20]` SDO Write
3. SDO 완료 및 Abort Code 확인
4. `0x6040` bit 7 Fault Reset 펄스
5. `0x6041` Fault bit와 실제 엔코더 오류 재확인
6. 절대 위치가 맞는지 확인 후 Enable

현재 프로젝트는 저수준 SDO Write는 가능하지만 [EtherCAT_SDOBase.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/EtherCAT_SDOBase/EtherCAT_SDOBase.st:278), PC/D5 진단 경로는 아직 Read 전용입니다. [LMCSdoExecutor.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st:299)

따라서 SIGMATEK에서 쓰려면 임의 SDO Write를 개방하기보다 `0x3204:20`, Socket `1..4`만 허용하는 전용 one-shot 리셋 명령을 추가하는 방식이 안전합니다. 엔코더 제조사/프로토콜과 EAS Socket 번호를 알려주면 사용할 객체를 정확히 하나로 확정할 수 있습니다.

> 지금 API GUI 구현되어있는게 Read만 가능한데? Write가 안된다

<details><summary>2 previous messages</summary>

> 맞습니다. 현재 구현 상태를 다시 확인해서, GUI만 막힌 것인지 아니면 PC API→PLC 전체 경로가 Read 전용인지 정확히 구분하겠습니다.
>
> 확인 결과 GUI만의 문제가 아닙니다. C# 프로토콜에는 Write 형식이 일부 준비돼 있지만, SDK allowlist와 PLC 실행기가 의도적으로 닫혀 있어 현재 종단 간 Write는 실패하도록 되어 있습니다. 리셋 객체를 쓰려면 이 두 구간을 함께 열어야 합니다.
</details>

맞습니다. 현재 GUI/API로는 리셋할 수 없습니다. 제가 앞에서 설명한 것은 “Gold Twitter가 해당 Write 객체를 지원한다”는 뜻이고, 현재 프로젝트에 Write 경로가 구현됐다는 뜻은 아닙니다.

확인된 차단 지점은 다음과 같습니다.

- GUI의 Write 입력이 비활성화되어 있습니다: [MainWindow.xaml](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:1320)
- C# SDK의 SDO Write allowlist가 비어 있습니다: [LmcDiagnosticsD5Models.cs](./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs:433)
- PLC는 Write 요청 플래그를 받으면 `detailCode := 2`로 거부합니다: [LMCDiagnosticsService.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1713)
- 실행기도 `TryStartRead`만 있으며 Write callback은 invalid 처리합니다: [LMCSdoExecutor.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st:113), [callback 검사](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st:644)

더구나 `TW[20]` 객체는 Write-only라서 현재 Read GUI로는 우회할 수도 없습니다.

권장 구현은 임의 SDO Write 전체를 개방하는 것이 아니라 전용 `Encoder Error Reset` 명령만 추가하는 것입니다.

- 일반 `TW[20]`: `0x3204`, SubIndex `20(decimal)`, `UDINT`, 4바이트
- EnDat 2.2: `0x20FC:02`, `UINT`, 2바이트
- Socket 값: `1..4`
- Motor OFF 확인 후에만 실행
- 완료 후 `0x6040` bit 7 Fault Reset
- `0x6041`, AxError 및 실제 절대 위치 확인

즉, 현재 당장 가능한 것은 EAS에서 직접 `TW[20]=socket`을 실행하는 방법이고, SIGMATEK API GUI에서 하려면 PLC 실행기·D5 서비스·C# allowlist·GUI를 함께 변경해야 합니다. 아직 코드는 변경하지 않았습니다.

> 지금 Lasal로만 사용한다면 어떤식으로 쓸수 있냐?

<details><summary>2 previous messages</summary>

> LASAL만 사용한다면 PC API를 거치지 않고 PLC 내부에서 CoE SDO Write를 한 번 실행하면 됩니다. 현재 프로젝트 구조에 맞는 호출 위치와 비동기 완료 처리 방식을 확인해서, 바로 옮겨 쓸 수 있는 형태로 정리하겠습니다.
>
> 현재 프로젝트에는 축별 `LMCSdoExecutor1..4`가 이미 Elmo 드라이브에 연결돼 있지만, 수동 실행 채널은 차단돼 있습니다. 따라서 일회성 정비는 별도의 디버그용 `EtherCAT_SDOBase` 객체로 가능하고, 상시 기능은 기존 executor에 전용 Write 메서드를 추가하는 두 방식으로 나뉩니다.
</details>

LASAL만 사용한다면 가장 빠른 방법은 디버그용 `EtherCAT_SDOBase` 객체를 추가하고 Online 화면에서 SDO를 한 번 Write하는 것입니다. 현재 `LMCSdoExecutor1..4`는 수동 Write가 막혀 있어 그대로는 사용할 수 없습니다. [차단 코드](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st:299)

### 1. LASAL Network 구성

`EtherCAT_Network`에 `EtherCAT_SDOBase` 객체를 하나 추가하고 다음처럼 연결합니다.

```text
EncoderResetSdo1.toSlave -> Elmo_11.ClassState
EncoderResetSdo2.toSlave -> Elmo_21.ClassState
EncoderResetSdo3.toSlave -> Elmo_31.ClassState
EncoderResetSdo4.toSlave -> Elmo_41.ClassState
```

필요한 축 하나만 추가해도 됩니다. 현재 축별 연결 기준은 [EtherCAT_Network.lcn](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/EtherCAT_Network/EtherCAT_Network.lcn:1561)에서 확인됩니다.

### 2. 축을 먼저 OFF

SDO 실행 전에 다음을 확인합니다.

- 축 `PowerOn = 0`
- `Standstill = 1`
- `Elmo_11.StateWord.OperationEnabled = 0`
- 다른 SDO 작업이 실행 중이지 않음
- EtherCAT은 PREOP 이상, 일반적으로 OP 상태

### 3. Online 값 설정

일반 `TW[20]` alias를 사용할 경우:

```text
ParaIndex       = 16#3204
ParaSubIndex    = 20          // decimal, 즉 16#14
CompleteAccess  = 0
ParaType        = 0           // Numeric value
ParaLength      = 4
ParaValue       = 1           // EAS virtual socket 1..4
Timeout         = 1000        // ms
```

모든 값을 설정한 다음, 마지막에 아래 값을 한 번만 Write합니다.

```text
ParaReadWrite = 1
```

EnDat 2.2 전용 객체를 쓴다면:

```text
ParaIndex       = 16#20FC
ParaSubIndex    = 2
CompleteAccess  = 0
ParaType        = 0
ParaLength      = 2
ParaValue       = 1           // socket 1..4
Timeout         = 1000
ParaReadWrite   = 1           // 마지막에 1회
```

`EtherCAT_SDOBase`는 설정한 `ParaLength` 그대로 `StartWriteSDO`를 실행하므로 2바이트와 4바이트 모두 가능합니다. [Write 구현](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/EtherCAT_SDOBase/EtherCAT_SDOBase.st:278)

### 4. 결과 확인

```text
ClassState: BUSY -> READY   성공
ClassState: ERROR           실패, ErrorCode 확인
```

`READY`는 반드시 callback 완료 후의 상태를 확인해야 합니다. Write 완료 전에는 Index, Length, Value를 바꾸면 안 됩니다.

성공 후 DS402 Fault Reset을 실행합니다.

- LASAL 로직: `LMCAxis1.QuitError()`
- Online 수동 조작: `Elmo_11.AxErrorQuit = 1` 한 번 Write

이 동작은 `0x6040` bit 7 Fault Reset을 발생시킵니다. [ECAT_DS402Base.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/ECAT_DS402Base/ECAT_DS402Base.st:650)

마지막으로 확인할 값은 다음입니다.

```text
Elmo_11.StateWord.Fault = 0
Elmo_11.AxError         = 0
절대 위치가 실제 기계 위치와 일치
```

주의할 점:

- `ParaReadWrite=0`으로 되돌리면 Write-only 객체에 Read를 실행하므로 되돌리지 않습니다.
- `ParaReadWrite=1`을 Force하거나 cyclic하게 계속 쓰면 안 됩니다.
- Startup SDO로 등록하면 재부팅마다 실행되므로 사용하면 안 됩니다.
- 같은 드라이브의 `LMCSdoExecutor`와 동시에 SDO를 실행하면 BUSY가 발생할 수 있습니다.
- 상시 기능으로 사용할 경우에는 디버그 객체를 두기보다 기존 `LMCSdoExecutor`에 `TryStartEncoderReset` 전용 메서드를 추가하는 것이 맞습니다.
