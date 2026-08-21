# SetPosition 최우선 개발 설계

- 대상: No.58 `MMC_SetPositionCmd`
- 현재 진행도: 25%
- current 상태: `Dormant`, fail-closed
- 기존 command: `0x7D12 Start`, `0x7D14 ReadOutcome`, `0x7D1A Retire`
- activation: Store/ordinary ownership/capability/native 실행 모두 OFF

## 1. 목표와 현재 경계

SDK, wire, volatile Store ABI, RT observation-only preflight와 Control/TCP P1 lifecycle은 이미
존재한다. 남은 핵심은 project-deployed durable backend, RT exactly-once native executor,
terminal-before-release와 WPF recovery 연결이다.

현재 안전 잠금은 그대로 유지한다.

- `LMC_ADMIN_SET_POSITION_STORE_CONFIGURED = FALSE`
- Control/TCP ordinary ownership `FALSE`
- 축 1~4 `SetPositionMaxJump = 0`
- Admin capability `0x00000017`, bits 3/5/7 OFF
- Admin SetPosition path의 native `.SetPosition()` call 0
- Store backing은 336 UDINT, 1,344-byte ordinary volatile `VAR_GLOBAL`

이 문서는 기존 상세 설계
[Axis SetPosition 비동기 RT executor 및 복구 설계](../../architecture/AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md)의
후속 구현 큐다. 기존 frozen P0/P1 ABI와 `-12/-13/-14` 의미를 다시 설계하지 않는다.

## 2. 완료된 구성

| 구성 | current 파일 | 상태 |
|---|---|---|
| SDK start/query/retire | `LMC_Library/LMC_API_Delivery/src/LmcAxisSetPosition*.cs`, `LmcAdminSetAxisPosition*.cs` | 구현 |
| WPF recovery journal core | `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/AxisSetPositionRecoveryJournal.cs` | core만 구현, MainWindow 미연결 |
| Store Begin/Commit/Read/Retire | `Class/LMCSetPositionStore/LMCSetPositionStore.st` | volatile scaffold |
| RT preflight | `Class/LMCEcatInputLatch/LMCEcatInputLatch.st` | observation-only |
| Control async lifecycle | `Class/LMCControlCommandService/LMCControlCommandService.st` | OwnershipActive에서 pending 정지 |
| TCP pending/quarantine | `Class/TCPMotionInterface/TCPMotionInterface.st` | `-13/-14/-12` 분리 구현 |

## 3. SP-01 durable backend

### 3.1 RAMex production 판정: NO-GO

`VAR_GLOBAL RETAIN`과 장비별 `autoexec.lsl` 설정은 사용하지 않는다. 공식 System library의
file-backed `RAMex UseFile=1`을 감사한 결과 storage-only 실험에는 쓸 수 있지만 production
Store의 durability barrier로는 사용할 수 없다.

- `SetDataAt` 성공은 RAM mirror 갱신과 async job enqueue 성공이지 file write 성공이 아니다.
- `SRamFileAsyncInfo.GetAsyncState()`는 모든 RAMex/Retentive=File의 global busy/idle만 주며
  request별 성공/실패를 주지 않는다.
- `GetDataAt`은 같은 RAM mirror를 읽으므로 physical file reopen/readback 증거가 아니다.
- startup load 실패는 내부 create/reset과 구분할 public ABI가 없다.
- partial write로 RAMex outer CRC가 깨지면 한 파일 내부 A/B journal 전체가 load 거부될 수 있다.

따라서 RAMex idle이나 same-boot mirror match를 durable success로 반환하지 않는다. 구형
`RamFile` 또는 수백 개의 scalar `Retentive=File` server로 우회하지도 않는다.

### 3.2 선택한 다음 설계: `_FileSys` 이중 파일 A/B

현재 프로젝트에 포함된 `_FileSys` rev1.20 위에 SetPosition 전용 backend를 만든다.
`FileWrite_AV1(Async)`와 `GetAsyncState(ID, Erg)`로 request별 완료/bytes-written/error를
확인하고, close 뒤 `FileRead_AV1`/`FileReadV1`으로 실제 파일을 다시 열어 검증한다.
write handle은 `ATT_COMMITTED`를 사용한다. 공식 문서에 atomic replace가 보장되지 않으므로
rename으로 active 파일을 덮어쓰지 않는다.

`_FileSys`는 async request의 file name과 data buffer를 복사하지 않는다. 따라서 file path,
2,048-byte write/read buffer와 marker buffer는 backend class field로 고정하고 해당 request가
완료될 때까지 변경하거나 해제하지 않는다. async request ID와 file handle은 별도 field로
보존하며 backend instance당 one-in-flight만 허용한다. 모든 async API는 `Async:=1`로 호출하고
즉시 반환값이 양수인 request ID일 때만 poll을 시작한다.

`GetAsyncState(RequestId, #OperationResult)`의 exact 판정은 다음과 같다.

- `RT_NOT_STARTED`와 `RT_IN_PROGRESS`: pending, field/buffer/path 변경 금지
- return `0`: operation completed; 그때만 `OperationResult`를 단계별 기대값과 대조
- `RT_INVALID_ID`, `RT_ERG_DELETED`와 그 밖의 return: uncertainty/error, 실패
- `FileOpen_A`: `OperationResult > 0`인 file handle을 별도 보존
- `Filelseek_AV1`: `OperationResult = 36`
- `FileWrite_AV1`: `OperationResult = 2048` 또는 marker 단계의 `8`
- `FileRead_AV1`: `OperationResult = 2048`
- `FileLength_A`: `OperationResult = 2048`
- `FileClose_A`: `OperationResult = 0`; 결과를 주지 않는 동기 close로 대체하지 않음

두 fixed slot 파일은 각각 2,048 bytes로 한다. 2,048은 이 설계의 고정 크기 선택이며 공식
정렬 요구라고 주장하지 않는다. 각 파일은 64-byte header, exact 1,344-byte ledger body와
640-byte zero padding을 가진다.

64-byte header는 다음 exact layout을 사용한다.

| Offset | Field | Contract |
|---:|---|---|
| 0 | Magic | U32 `0x50534D4C`; file bytes `4C 4D 53 50` (`LMSP`) |
| 4 | Schema | U32 `0x00000001` |
| 8 | HeaderBytes | U32 64 |
| 12 | FileBytes | U32 2048 |
| 16 | Generation | nonzero U32 |
| 20 | GenerationInverse | bitwise NOT Generation |
| 24 | BodyBytes | U32 1344 |
| 28 | BodyCRC32 | exact ledger body CRC |
| 32 | HeaderCRC32 | offsets 0..31과 44..63의 CRC; CRC field와 marker pair는 제외 |
| 36 | CommitMarker | invalid 0 또는 U32 `0x54494D43`; bytes `43 4D 49 54` (`CMIT`) |
| 40 | CommitMarkerInverse | invalid 0 또는 U32 `0xABB6B2BC` |
| 44..63 | Reserved | all zero |

HeaderCRC가 generation과 body metadata를 보호하고 marker/complement pair가 torn marker write를
검출한다. BodyCRC만으로 generation을 신뢰하지 않는다. Padding 640 bytes도 모두 zero인지
검증한다.

모든 U32는 little-endian file byte order다. CRC는 기존 Store와 같은
`CheckSum.CRC32(pBuffer, len, CrcStart:=0)` 반환 UDINT를 추가 final XOR 없이 그대로 저장한다.
BodyCRC는 bytes 64..1407을 한 번 계산한다. HeaderCRC는 marker pair와 CRC field를 제외하고
`crc:=CheckSum.CRC32(#Header[0],32,0)` 뒤
`crc:=CheckSum.CRC32(#Header[44],20,crc)` 순서로 이어 계산한다. cross-host 구현 전에는 이
vendor CRC 호출의 golden file fixture를 고정하며 임의의 IEEE CRC variant로 대체하지 않는다.

고정 경로는 `C:\LMCSP_A.BIN`과 `C:\LMCSP_B.BIN`이다. 두 NUL-terminated ASCII path array는
class field로 유지한다. startup read는 free-space 검사에 선행하며 A/B `FileOpen_A`/read 결과로
media availability를 판정한다. 기존 valid slot이 있으면 disk free가 16 KiB 미만이어도 read/CRC,
outcome query와 recovery read를 계속 허용한다. `GetDiskSpace("C:", ...) = 0`, nonzero
sector/cluster 정보와 최소 16 KiB free space는 `BeginWrite`, terminal commit/retire 같은 새 mutation
admission 직전에만 확인한다. `DRIVE_NOT_FOUND(-4)`, `WRONG_MEDIA(-7)`,
`INVALID_FILE_SYSTEM(-8)`, `DISK_FULL(-22)`, `DRIVE_NOT_READY(-29)` 또는 16 KiB 미만은
`WriteUnavailable/StorageDegraded`를 latch해 새 write만 막으며, 이미 검증된 active slot의 read
증거를 지우지 않는다. startup `FileOpen_A`/read 자체가 실패한 경우만 `StorageUnavailable`로
분류하고 `FILE_NOT_FOUND(-9)`와 구분한다.

1. startup은 A와 B를 각각 reopen/read하고 magic/schema/body length/zero padding/body CRC/
   header CRC/generation inverse/commit marker pair를 검증한다.
2. generation 비교는 unsigned serial arithmetic을 사용한다. `0 < (A-B) mod 2^32 <
   0x80000000`일 때만 A가 newer다. delta 0에서 image가 다르거나 delta `0x80000000`이면
   ambiguous corruption으로 fail-closed한다. next generation은 0을 건너뛴다.
3. 두 파일이 모두 `FILE_NOT_FOUND`면 application runtime에서 생성하지 않는다. production PLC
   source에는 `CommissioningRequest`, erase/provision method, TCP/Admin command, Network Server,
   debug-writable request field를 두지 않는다. 초기 A/B는 PLC application이 STOPPED이고 project가
   unload된 factory deployment 단계에서 host-side
   `tools/Generate-LmcSetPositionStoreImages.ps1`가 generation 1 empty ledger 두 개를 생성하고,
   승인된 deployment bundle이 `C:\LMCSP_A.BIN`과 `C:\LMCSP_B.BIN`을 같은 작업으로 설치한다.
   host-side bundle manifest는 controller serial, source revision, schema, 두 파일의 exact
   2,048-byte length와 SHA-256을 기록한다. manifest는 PLC에 복사하지 않으며 PLC runtime이 읽거나
   SHA-256을 계산하지 않는다. PLC의 최초 `Provisioned` 판정은 A/B가 각각 internal header/body CRC,
   marker pair와 generation 1을 통과하고 두 full image가 exact 동일한 경우로만 고정한다. host는
   설치 후 두 파일을 다시 PC로 내려받아 manifest SHA-256과 대조한다. 이 방식은 출하 PLC마다
   `autoexec.lsl` 또는 runtime 변수 값을 따로 수정하지 않는다.
   host manifest/receipt는 controller가 아니라 승인된 manufacturing evidence store의
   `<ReceiptRoot>/<ControllerSerial>/deployment_receipts.jsonl`에 append-only로 보존한다. 각 record는
   `ReceiptSchema=1`, `ControllerSerial`, `State`, `SourceRevision`, `ImageSchema`, `ImageASha256`,
   `ImageBSha256`, `StopEvidenceSha256`, `PreviousReceiptSha256`, `Utc`, `OperatorId`를 갖고 record
   canonical UTF-8 bytes의 SHA-256으로 다음 record와 연결한다. 첫 record는 제조 inventory가 발급한
   `FactoryNew`; upload 직전 `FactoryInstallStarted`; exact readback 성공 뒤
   `VerifiedFactoryEmpty`다. future SP-07A activation tooling은 gate image를 만들기 전에
   `ActivationAuthorized`, 처음 활성 image를 load한 뒤 `Activated`를 append해야 한다.
4. commit은 inactive slot만 `ATT_CREATE_ALWAYS | ATT_COMMITTED`로 marker 0인 전체 2,048-byte
   image를 쓴다.
5. request별 write result와 exact bytes-written을 확인하고 close/reopen/full read/CRC를 검증한다.
6. 두 번째 write는 `ATT_CREATE_ALWAYS`를 재사용하지 않는다. `ATT_READ_WRITE |
   ATT_COMMITTED`로 같은 inactive file을 열고 exact offset 36으로 seek한 뒤 commit marker와
   inverse 8 bytes만 쓴다. exact 8-byte result와 close 완료를 확인한다.
7. file을 다시 열어 exact length 2,048, full 2,048-byte read, header/body CRC, marker pair와
   generation을 다시 검증한다.
8. 두 번째 readback까지 exact해야 generation을 publish하고 Store Commit 성공을 반환한다.
9. 어느 단계든 실패하면 기존 active slot은 그대로 두고 새 slot은 invalid로 취급한다.
10. terminal tombstone도 같은 two-phase write와 full readback barrier를 통과한다.

startup slot 판정은 아래 표를 따른다.

| A | B | 판정 |
|---|---|---|
| Missing | Missing | Unprovisioned; ordinary path fail-closed, exact factory-new receipt chain일 때만 bundle 설치 허용 |
| Valid | Valid | serial-newer 선택; 같은 generation+같은 full image는 A 선택, 같은 generation+다른 image 또는 half-range delta는 Corrupt |
| Valid | Missing/Invalid | Corrupt; missing/invalid 쪽이 더 최신이었을 가능성 때문에 restart에서 자동 복구/선택 금지 |
| Missing/Invalid | Valid | 위와 동일 |
| Missing/Invalid | Missing/Invalid | Corrupt 또는 ProvisionIncomplete; ordinary path mutation 0 |
| I/O error | any | StorageUnavailable; retry 전 active 선택 금지 |
| any | I/O error | 위와 동일 |

current runtime에서 inactive write가 실패한 경우에는 publish 전의 known active file을 변경하지
않지만 `StorageDegraded`를 latch하고 새 mutation을 차단한다. restart 뒤에는 위 표대로 한쪽
valid만으로 재개하지 않는다. factory deployment가 중단되어 A만 설치됐거나 marker 0/truncated
file이 남으면 ordinary startup은 `ProvisionIncomplete`다. production application에는
`AllowEraseIncompleteProvision` 같은 erase 권한이 없다. 복구는 PLC application을 STOP/unload한
상태에서 기존 A/B를 먼저 보존·수집한다. 아래 exact factory-new receipt chain이 증명될 때만 승인된
factory deployment bundle을 다시 적용한다. 이 조건이 아니면 valid production generation의 존재
여부와 관계없이 RMA/데이터 복구 대상으로 fail-closed한다. host-side manifest는 교체 후 readback으로
새로 생성되는 배포 증거이며 controller storage의 일부가 아니다.

factory transport는 LASAL CLASS 2 `Debug -> File Transfer`의 PC-to-PLC upload와 PLC-to-PC
readback으로 고정한다. 작업자는 project STOP/unload 화면과 controller serial을 증거로 저장하고,
`tools/Verify-LmcSetPositionStoreDeployment.ps1 -Manifest <path> -ReadbackA <path>
-ReadbackB <path> -ControllerSerial <serial> -StopEvidence <path> -ReceiptRoot <path>`를 실행해
두 readback의 length/SHA, 서로 동일한 generation-1 image, manifest identity와 receipt chain을
확인하고 `VerifiedFactoryEmpty`를 append한다. 검증 exit 0 전에는 project start, Store gate 변경
또는 capability activation을 금지한다. vendor File Transfer 자동화가 별도 검증되기 전까지 이
절차를 one-click/atomic deployment라고 부르지 않는다.

empty generation-1 bundle의 최초 설치 또는 중단 후 재적용은 receipt chain이 exact
`FactoryNew -> FactoryInstallStarted`이고 그 serial에 `VerifiedFactoryEmpty`,
`ActivationAuthorized`, `Activated`가 한 번도 없을 때만 허용한다. chain이 없거나 끊겼거나,
provenance가 불명확하거나, 이전 successful deployment/activation record가 하나라도 있으면 A/B가
모두 Missing/Invalid여도 empty bundle을 적용하지 않고 RMA/data recovery로 보낸다. 즉 controller
files가 유실됐다는 이유로 factory-new 상태를 추론하지 않는다.

이 순서는 atomic rename에 의존하지 않고 power loss 중에도 기존 active slot을 덮어쓰지 않는다.
Store ABI는 synchronous 성공으로 위장하지 않고 `BeginWrite/PollWrite/Readback` 상태 머신으로
분리한다.

### 3.3 수정 대상

- 신규 SetPosition file backend class와 `_FileSys` client declaration: LASAL IDE에서 생성
- 신규 host-side `tools/Generate-LmcSetPositionStoreImages.ps1`,
  `tools/Start-LmcSetPositionStoreDeployment.ps1`,
  `tools/Verify-LmcSetPositionStoreDeployment.ps1`와 receipt-chain/factory upload/readback
  절차·negative 시험
- `Class/LMCSetPositionStore/LMCSetPositionStore.st`
- `Class/LMCSetPositionStore/global_LMCSetPositionStore.st`
- `Class/LMCSetPositionStore/LMCSetPositionStore.h`
- `Elmo_EtherCAT_Test_4Axis.lcp`
- `Network/Comm_Network/Comm_Network.lcn`
- `Network/Comm_Network/ONE_Comm_Network_Table.st`

## 4. SP-02 RT claim/native executor

기존 preflight mailbox/result는 유지하고 별도 versioned execution mailbox를 IDE에서 생성한다.

1. Control이 exact tuple과 Store record generation을 execution mailbox에 publish한다.
2. RT는 preflight snapshot을 신뢰하지 않고 ownership, axis state, limit와 tuple을 다시 읽는다.
3. RT가 `Claimed`를 먼저 publish한 뒤에만 native 실행 단계로 들어간다.
4. `NativeCount`를 `0 -> 1`로 만든 단일 논리 call site에서만 다음을 실행한다.

   `SetPosition(Mode:=LMCAXIS_SET_ACTPOS_APPUNIT_DEST, Position:=TargetPosition)`

5. 같은 tuple 재관찰은 저장된 executor state만 반환하고 native call을 반복하지 않는다.
6. native 거부는 terminal candidate로 기록하되 durable commit 전에는 response하지 않는다.
7. native 수락 뒤 서로 다른 RT cycle의 position/state sample 3개가 안정될 때만 terminal
   success candidate를 만든다.
8. post-claim timeout, owner drift, torn result와 crash ambiguity는 terminal success로 축소하지
   않고 Armed record와 quarantine을 보존한다.

수정 대상은 `LMCEcatInputLatch.st`와 `LMCControlCommandService.st`다. 실제 축 1~4의
task/core/priority가 동일 RT 안전 경계를 제공하지 않으면 먼저 Motion Network 배치를 수정하고
그 증거를 기록한다.

## 5. SP-03 terminal-before-release

고정 순서는 다음과 같다.

`RT terminal proof -> durable terminal commit -> full readback -> ownership release -> TCP response`

- terminal commit/readback 불확실: `-12`, response/release 0회
- claim/native/context 불확실: `-14`, Armed 보존, 자동 replay 0회
- in-progress: `-13`, 같은 request/socket/session만 poll
- exact stored replay: native call 0회
- restart 후 Armed-only: Indeterminate, operator recovery 필요
- response loss: `0x7D14` query와 exact-generation `0x7D1A` retire만 허용

## 6. SP-04 WPF recovery 연결

1. wire write 전 journal을 `ArmedBeforeDispatch`로 저장한다.
2. startup unresolved journal은 `RecoveryRequired`로 열고 신규 mutation을 차단한다.
3. capability bits 3/5/7과 exact endpoint/build/BootId/MapRevision을 다시 확인한다.
4. query는 original command를 재전송하지 않는다.
5. exact terminal snapshot과 record generation을 journal에 저장한 뒤 retire한다.
6. exact retirement 성공 뒤에만 journal을 `Resolved`로 바꾼다.

수정 대상:

- `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml`
- `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs`
- `AxisSetPositionRecoveryJournal.cs`
- `LasalApiWpfTestApp.SmokeTests/AxisSetPositionRecoveryJournalTests.cs`

## 7. 작업 체크리스트

- [x] `SP-01A` RAMex completion/readback ABI 감사; production durability 요구 불충족으로 NO-GO
- [ ] `SP-01B` `_FileSys` 2 x 2,048-byte A/B file, per-request completion, CRC,
  marker-last, reopen/readback, boot selection과 tombstone 구현
- [ ] `SP-01C` Store async start/poll/readback ABI 및 fault mutation test 구현
- [ ] `SP-02A` 축 1~4 task id/core/priority와 InputLatch execution cycle 증거 기록
- [ ] `SP-02B` versioned execution mailbox/result를 LASAL IDE로 선언
- [ ] `SP-02C` claim-before-native, single call site와 duplicate-zero verifier 구현
- [ ] `SP-02D` stable-3 terminal observer와 post-claim quarantine 구현
- [ ] `SP-03A` terminal commit/readback-before-release/response 구현
- [ ] `SP-04A` WPF MainWindow journal/interlock 연결과 startup recovery test 구현
- [ ] `SP-05A` SourceOnly, method-size, C78, generated artifact를 같은 tree에서 통과
- [ ] `SP-06A` storage cold-cycle과 response-loss/query/retire 실기 검증
- [ ] `SP-06B` 축별 승인 max-jump로 small correction, fault/reconnect/packet matrix 검증
- [ ] `SP-07A` Store/ordinary ownership/max-jump/capability bits 3/5/7 paired activation

## 8. activation 금지 조건

다음 중 하나라도 충족하지 않으면 SetPosition은 계속 OFF다.

- `_FileSys` request별 completion, reopen/readback 또는 cold-cycle durability가 불명확함
- task/core/priority가 축별로 증명되지 않음
- native logical call site가 하나가 아니거나 duplicate call mutation test가 없음
- stable-3 전에 terminal success를 만들 수 있음
- terminal durable readback 전에 owner release/response가 가능함
- WPF startup recovery가 original mutation을 재전송할 수 있음
- 축별 application-approved max-jump와 physical correction matrix가 없음
