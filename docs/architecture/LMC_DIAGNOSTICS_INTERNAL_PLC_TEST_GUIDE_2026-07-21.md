# LMC EtherCAT Diagnostics 내부 PLC 시험 가이드

- 작성일: 2026-07-21
- 대상: `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`
- PC 시험 앱: `LMC_Library/LasalApiWpfTestApp`
- 범위: D1 Health/Catalog/PI Read, D2 Bulk, D3 Recorder v1,
  D4 single-bank Ring/Trigger
- 제외: 고객 배포 패키지 갱신, D4 Double bank, D5 PLC PI/SDO 실행, D6 facade
- preflight 상태: PC 101/101과 최신 LASAL source/full 계약 PASS. D0-D4 통합 source의
  IDE Rebuild/Link 0 error, implementation smoke 3/3 PASS지만 이후 Recorder Stop 멱등
  패치는 최신 source Rebuild 대기다. C78/C81 version mismatch warning은 남아 있다.

현재 source 상태와 실기 판정은 구분한다.

| 단계 | 현재 source 상태 | 이 문서의 PLC 실기 상태 |
|---|---|---|
| D0 | common envelope, capability와 `0x7E00` 구현 | 아래 D1~D4 시험과 함께 검증 대기 |
| D1 | Health/Catalog/PI Read 활성 | 미실시 |
| D2 | 최대 24-entry Bulk 활성 | 미실시 |
| D3 | single-bank finite/manual Recorder 활성 | 미실시 |
| D4 | single-bank Ring, Edge/Window/Mask와 forced trigger 활성 | 미실시. Double bank는 미구현 |
| D5 | C# 공개/wire contract만 구현, PLC 실행은 fail-closed | 미구현이므로 성공 시험 대상 아님 |
| D6 | static compatibility facade 후속 설계 | 미구현 |

따라서 정적 계약과 IDE Build/Link 통과를 실제 PLC 완료로 해석하지 않는다.

## 1. 시험 전 완료 조건

다음 조건이 모두 충족되기 전에는 PLC에 다운로드하지 않는다.

1. LASAL IDE에서 `LMCDiagnosticsService`, `LMCEcatInputLatch`, `LMCRecorderStore`,
   `TCPMotionInterface`를 저장한다.
2. Rebuild와 Link가 0 error다.
3. `LMCDiagnosticsService` tree에 hidden retentive `DiagnosticsBootCounter` server와
   `GetDiagnosticsBootId` method가 보인다.
4. `Find in Implementation` smoke가 정상이고 smoke 시작 이후 `Lasal2.log`에 새
   `CInvalidArgException`이 없다.
5. PC test와 LASAL full verifier가 통과한다.

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' `
  /t:RunPcTests /p:Configuration=Release /nologo

& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' `
  /t:RunLasalNetworkContract /p:Configuration=Release /nologo
```

## 2. 안전 조건

- 첫 시험은 축 power off와 정지 상태에서 수행한다.
- D1~D4는 read/record 경로이며 motion command를 발생시키지 않아야 한다.
- PI Write와 SDO Write control은 현재 SDK allowlist와 PLC capability가 모두 off인 것이
  정상이다. 활성화해서 시험하지 않는다.
- `ControlWord`, `TargetPosition`, `TargetVelocity`, `TargetTorque` direct write는 금지한다.
- PLC reboot, diagnostics service 재초기화 또는 BootId 변경 뒤 기존 Bulk/Recorder
  handle을 재사용하지 않는다.

## 3. 연결 및 capability

1. PLC에 test build를 다운로드하고 Run 상태로 전환한다.
2. 개발 WPF를 실행한다.
3. PLC TCP endpoint에 연결하고 RPC Init을 완료한다.
4. Diagnostics capability를 읽는다.

정상 retained 경로의 기대값:

```text
DiagnosticsBuild       1
CapabilityBits         0x0000003F
MapRevision            0x957F101E
CatalogEntryCount      24
MaxBulkEntryCount      24
MaxRecorderChannels    24
RecorderBufferCount    1
MaxRecorderSamples     320000
RecorderBytesPerBank   1280000
MaxSdoDataBytes        0
DiagnosticsBootId      nonzero
```

`MaxRecorderSamples=320000`은 1채널일 때의 capability 상한이다. Configure 응답의
`AcceptedCapacity=min(requested, floor(1280000 / (channelCount * 4)))`가 실제 상한이며,
16채널은 20,000 samples, 24채널은 13,333 samples까지다.

BootId가 0이면 D2/D3와 D4 Trigger bit가 꺼지는 것이 정상 fail-closed다. 이 경우 Recorder를
강행하지 말고 `DiagnosticsBootCounter` retentive restore/write/read-back부터 확인한다.

## 4. D1 Health, Catalog, PI Read

1. Health를 연속 조회한다.
2. master state, invalid/missed frame counter, frame time, RT time을 기록한다.
3. Catalog info/chunk를 읽고 24개 entry, map revision, Catalog CRC를 확인한다.
4. 축 1~4마다 아래 6개 PDO signal만 존재하는지 확인한다.
   - SetPosition
   - DigitalOutputs
   - ControlWord
   - ActualPosition
   - DigitalInputs
   - StatusWord
5. 24개 PI를 읽고 같은 snapshot의 cycle counter/timestamp/status를 확인한다.
6. EtherCAT cable 또는 slave fault 시험에서는 값을 정상값으로 오인하지 않고
   stale/offline 상태와 AL/status 정보가 바뀌는지 확인한다.

software axis 5~9는 EtherCAT PDO Catalog에 없어야 한다.

## 5. D2 Bulk Snapshot

1. 먼저 4개 read-only signal로 Bulk를 configure한다.
2. status와 snapshot을 100회 이상 읽는다.
3. 모든 entry가 하나의 cycle counter, timestamp, map revision을 공유하는지 확인한다.
4. 24개 signal로 확대해 같은 검사를 반복한다.
5. 같은 session에서 release하고 다시 configure했을 때 stale config가 재사용되지 않는지
   확인한다.
6. 다른 session, 다른 BootId, 잘못된 map revision으로 접근하면 거부되는지 확인한다.

기록할 값:

- 요청/응답 시간
- snapshot cycle 간격
- entry status 분포
- invalid frame 전후 cycle/timestamp 변화
- configure/release 반복 후 resource leak 여부

## 6. D3 Recorder v1

첫 시험은 작은 capture로 시작한다.

```text
BufferMode          Single
TriggerType         Manual/None
Channels            2~4
SamplePeriodCycles  1
SampleCapacity      1000
```

1. Configure 후 반환된 ConfigId/Revision/BootId/OwnerSessionEpoch를 확인한다.
2. Start 후 status가 Configured -> Armed/Recording -> Completed로 전이하는지 확인한다.
3. Header를 읽고 channel metadata, sample stride, cycle/timestamp 범위를 확인한다.
4. chunk를 sequence 0부터 끝까지 내려받는다.
5. 각 chunk CRC, offset, returned count, `LastChunk`를 검증한다.
6. WPF plot과 CSV를 생성하고 sample 수와 cycle 간격을 대조한다.
7. buffer release 후 같은 identity로 chunk read가 거부되는지 확인한다.
8. 24채널과 divider/capacity 조합으로 확대한다.
9. 현재 bank 상한 시험은 16채널/20,000 samples와 24채널/13,333 samples로 수행한다.
   Configure 응답의 AcceptedCapacity, 1,280,000-byte bank 범위, PLC free RAM과 RT
   jitter를 함께 측정한다. 32채널/31,250 samples는 4,000,000-byte bank로 확장한
   후속 profile이며 현재 test build의 완료 기준이 아니다.

upload 중에는 완료 bank의 header/data hash가 변하면 안 되고 RT task가 TCP 전송 때문에
block되면 안 된다.

## 7. D4 single-bank Ring/Trigger

현재 D4 시험 대상은 하나의 물리 bank를 쓰는 Ring/Trigger뿐이다.
`RecorderBufferCount=1`, BufferId=0, capability bit 5=1과 bit 6=0을 먼저 확인한다.

1. `Ring + Edge`를 Int32 또는 BitField signal에 설정하고 pre-trigger 100,
   post-trigger 899, capacity 1000으로 시작한다.
2. pre-trigger history가 채워진 뒤 threshold를 통과시켜 자동 trigger가 한 번만
   발생하고 `TriggerIndex=100`인지 확인한다.
3. Int32 signal의 `Window`에서 `TriggerValue=lower`, `TriggerMask=upper` 경계를
   각각 통과시켜 조건과 signed 비교가 일치하는지 확인한다.
4. BitField16/32 signal의 `Mask`에서 nonzero mask로 all-set/any-set/all-clear 조건을
   각각 확인한다.
5. non-Manual Ring을 다시 시작하고 `Trigger Now (0x7E42)`로 forced trigger를 발생시킨다.
6. EtherCAT master/slave가 유효하지 않은 cycle에서는 자동 trigger가 발생하지 않고,
   정상 상태 복귀 뒤 새 history로 조건을 다시 평가하는지 확인한다.
7. 각 경우 Header/CSV에서 pre + trigger + post sample 순서, sample count와 cycle 간격을
   확인하고 완료 bank가 upload 중 변하지 않는지 확인한다.

이 시험의 통과는 Double bank를 검증하지 않는다. 두 번째 bank, BufferId=1과 capture/upload
동시 진행은 현재 source에 없다.

## 8. 재접속과 Adopt

1. 완료된 record를 release하지 않은 상태에서 PC TCP session만 종료한다.
2. 새 session으로 연결하고 capability를 다시 읽는다.
3. BootId가 같을 때만 `AdoptRecorder(recordId, bufferId, bootId)`를 호출한다.
4. 새 OwnerSessionEpoch를 받은 뒤 임의 chunk offset부터 다운로드를 재개한다.
5. PLC reboot로 BootId가 바뀐 뒤 이전 identity의 adopt가 거부되는지 확인한다.

## 9. 현재 미구현 또는 Unsupported가 정상인 기능

아래는 버튼/API contract가 보여도 PLC 성공을 기대하지 않는다.

- D4 Double Buffer
- D5 `SubmitPIWrite (0x7E21)`
- D5 operation status/cancel과 `SubmitSDO (0x7E50)`
- extended SDO `ReadSDOResultChunk (0x7E51)`
- D6 static compatibility facade

D4 Double/D5 capability bit가 0인 동안 WPF는 실행 control을 비활성화하거나 API가
호출 전 차단해야 한다. D5 exact reserved raw request를 보낸 경우 PLC는
`UnsupportedFeature`를 반환해야 한다. D6에는 호출할 PLC command 자체가 없다.

## 10. 시험 결과 기록

각 시험 결과에는 다음을 남긴다.

- Git commit 또는 dirty diff 식별자
- LASAL project/compiler version
- PLC model, firmware, cycle time, free RAM
- capability 전체 값과 BootId
- WPF/API build configuration
- 성공/실패 step과 실제 error/status
- TCP packet capture
- System Trace의 task/core/priority/jitter
- Recorder 설정, header, CSV, bank hash

이 결과가 통과한 뒤에만 검증된 DLL, 예제 프로그램, 문서를 고객 배포 폴더로 옮긴다.
