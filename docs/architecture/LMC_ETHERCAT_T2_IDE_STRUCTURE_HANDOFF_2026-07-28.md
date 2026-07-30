# EtherCAT T2 LASAL IDE 구조 작업 인계

- 날짜: 2026-07-28
- 대상: `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`
- 목적: `0x7E13` node health와 `0x7E22` digital I/O read implementation을 외부 편집기로
  작성하기 전에 LASAL CodeGenerator와 Object Network가 소유하는 구조만 생성한다.
- 이 단계에서는 capability bit 15/16/17을 켜지 않고 `0x7E23` output mailbox도 만들지 않는다.

## 1. 현재 확인 상태

현재 `LMCEcatInputLatch`에는 `EcatMaster`, `Drive1..4`, `RecorderStore` 여섯 client와
`RtWork`, `CopySnapshot`만 있다. `Motion_Network`도 같은 legacy 연결만 갖고 있다.
`LMCDiagnosticsService`에는 `HandleEtherCATTopologyIoRequest` 선언이 없으며 `0x7E11/0x7E12`는
아직 `HandleRequest` 안에 있다.

따라서 아래 구조를 IDE에서 생성하기 전에는 implementation source를 추가하지 않는다.

2026-07-29 재확인에서도 `Coupler/InputSlot/OutputSlot`,
`HandleEtherCATTopologyIoRequest`, `0x7E13/0x7E22/0x7E23` route는 tracked LASAL source/network에
없다. 현재 WPF가 7개 configured node와 CREVIS 3행을 표시하는 것은 정적 `0x7E11/0x7E12`
inventory까지의 결과다. capability bits 15~17이 0인 현재 상태에서 live health/DI/DO 값이
올라오지 않는 것은 구현 경계와 일치하며, WPF 표시 문제로 판정하지 않는다.

또한 현재 `.lcp`가 등록한 아래 CREVIS/slot support source 디렉터리는 Git 미추적 상태다.

- `Class/ECAT_SlotBase`
- `Class/ECAT_SlotMng`
- `Class/EtherCATSlot_Hub_Base`
- `Class/GL_9086_1`
- `Class/GL_9086_1_Slot00`
- `Class/GL_9086_1_Slot01`

이 파일들은 사용하지 않는 임시 디렉터리가 아니다. clean checkout에서도 LASAL build가 재현되도록
별도 generated/vendor-source tranche로 검토·커밋하기 전까지 삭제하거나 cleanup 대상에 넣지 않는다.

## 2. LMCEcatInputLatch class

기존 class를 LASAL IDE에서 열어 아래 항목을 추가한다. 이름, 대소문자와 type을 그대로 사용한다.

### Required external clients

| Name | Type |
|---|---|
| `Coupler` | `CltChCmd_GL_9086_1` |
| `InputSlot` | `CltChCmd_GL_9086_1_Slot00` |
| `OutputSlot` | `CltChCmd_GL_9086_1_Slot01` |

세 client 모두 `Required=true`, `Internal=false`로 만든다.

### Variables

| Name | Type |
|---|---|
| `OutputRevision` | `UDINT` |
| `OutputObserved` | `BOOL` |
| `OutputPreviousValid` | `BOOL` |
| `OutputPreviousValue` | `UDINT` |

기존 `PublishSequence`와 `SnapshotBytes : ARRAY [0..511] OF USINT`는 변경하지 않는다.

### Global methods

1. `CopyTopologyIoSnapshot`

   - input `pDest : ^Void`
   - input `DestSize : UDINT`
   - output `Result : DINT`

2. `AdvanceOutputRevision`

   - output `Revision : UDINT`

method implementation은 비워 둬도 된다. IDE 저장 뒤 외부 편집 단계에서 구현한다.

## 3. LMCDiagnosticsService class

기존 class에 private method `HandleEtherCATTopologyIoRequest`를 추가한다. `GLOBAL` method로 만들지
않는다.

### Inputs

| Name | Type |
|---|---|
| `CommandId` | `UINT` |
| `pRequest` | `^USINT` |
| `RequestSize` | `UDINT` |
| `pResponse` | `^USINT` |
| `ResponseCapacity` | `UDINT` |
| `CallerSessionEpoch` | `UDINT` |
| `CurrentDiagnosticsBootId` | `UDINT` |

### Output

| Name | Type |
|---|---|
| `ResponseSize` | `DINT` |

이 method도 IDE 구조와 declaration만 생성한다. body는 저장 뒤 외부에서 구현한다.

## 4. Motion_Network 연결

기존 `LMCEcatInputLatch1` object와 RT trigger/master/drive/recorder 연결을 유지하고 아래 세 연결만
추가한다.

| Source | Destination |
|---|---|
| `LMCEcatInputLatch1.Coupler` | `GL_9086_11.ClassState` |
| `LMCEcatInputLatch1.InputSlot` | `GL_9086_1_Slot001.ClassState` |
| `LMCEcatInputLatch1.OutputSlot` | `GL_9086_1_Slot011.ClassState` |

`Comm_Network`에 새 diagnostics client를 만들지 않고 `LMCPreRtWorkTrigger` 연결도 추가하거나
이동하지 않는다.

## 5. 저장과 확인

1. 두 class와 `Motion_Network`를 저장한다.
2. Reload/Rebuild/Link를 실행한다.
3. 변경한 두 class에서 `Find in Implementation` smoke test를 수행한다.
4. smoke 시작 시점 이후 `%TEMP%\Lasal2.log`에 새 `CInvalidArgException`이 없는지 확인한다.
5. 생성된 `.st`, `.lcb`, `.lcn`, `ONE_Motion_Network_Table.st`, channel header와 `.lcp` 변경을
   그대로 master working tree에 남긴다.

외부 편집기로 generated declaration/table/binary를 보정하거나 이름을 바꾸지 않는다. 빌드 오류가
있으면 원문 전체와 파일/line을 전달한다.

## 6. 다음 외부 구현 단계

위 구조가 생성되면 다음 순서로 진행한다.

1. 외부 implementation 편집 전에 아래 구조-only checkpoint 통과

```powershell
& LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1 `
  -RepositoryRoot C:\work\Elmo\Elmo_Master `
  -TopologyIoCheckpoint IdeStructureReady
```

`IdeStructureReady`는 IDE가 생성한 세 typed client, exact `Motion_Network` 연결, 네 변수와 세 method
stub만 검증한다. `0x7E13/0x7E22/0x7E23` route는 없어야 하며 capability bit 15~17도 계속 0이어야
한다. 현재 tree는 client가 6개뿐이라 이 checkpoint를 의도대로 통과하지 않는다.

2. `LMCEcatInputLatch` 464-byte coherent snapshot과 CREVIS health/I/O source 구현
3. `CopyTopologyIoSnapshot`, `AdvanceOutputRevision` 구현
4. `LMCDiagnosticsService` helper로 기존 `0x7E11/0x7E12` 이동
5. `0x7E13/0x7E22` exact request/response 구현과 TCP route 추가
6. 아래 dormant checkpoint 통과

```powershell
& LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1 `
  -RepositoryRoot C:\work\Elmo\Elmo_Master `
  -TopologyIoCheckpoint IntegratedReadOwnerDormant
```

`IntegratedReadOwnerDormant` checkpoint는 read owner와 route/handler를 모두 요구하지만
capability bit 15/16은 계속 0으로
강제한다. 다음 internal read-only 도구로 raw live node/DI를 검증한 뒤에만
`IntegratedReadOwner`로 전환한다.

```powershell
& LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/bin/Release/LasalMotionControlLib.Tests.exe `
  topology-io-qualify `
  --scope integrated-read-owner-dormant `
  --execute-live `
  --confirm PLC-RAW-TOPOLOGY-IO-READ `
  --host <PLC IPv4> --local <PC IPv4> --output <new report path>
```

상세 안전 계약과 report 판정은
`LMC_Library/LMC_API_Delivery/docs/TOPOLOGY_IO_QUALIFICATION_TOOL_2026-07-28.md`를 따른다.
이 도구는 `0x7E23`과 모든 mutation command를 거부하고 capability를 직접 켜지 않는다.
