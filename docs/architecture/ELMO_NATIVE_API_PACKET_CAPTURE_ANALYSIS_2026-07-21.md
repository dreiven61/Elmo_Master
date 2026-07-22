# Elmo native API packet capture analysis

- Date: 2026-07-21
- Scope: 23 captures under `test/packet_capture/Elmo_API_Analyze`
- Compared applications:
  - `기존프로그램`: Elmo example using native MMCLib calls
  - `신규프로그램`: `Codex_PMAS_WPF_Version2`

## 1. 결론

제공된 23개 캡처는 모두 Maestro TCP port 4000으로 전송된 PMAS/MMCLib native
RPC다. LASAL diagnostics command `0x7Exx`는 하나도 포함되어 있지 않다. 따라서
이 자료는 native API 기능의 의미와 호출 순서를 확인하는 기준이며, 현재 custom
LASAL-DINT protocol의 binary 호환성 또는 PLC 실장 성공을 증명하지 않는다.

신규 프로그램의 PI metadata/read/bulk, EtherCAT communication diagnostics, Stop,
status, header와 4-byte SDO read packet은 대응하는 native API와 일치한다. 확인된
실제 결함은 Recorder download 순서다. 신규 캡처는 ready buffer가 없고 header
`Rl=0`인데 `[0..63]` download를 요청해 `Status=0x0010`, `ErrorID=-3`을 받았다.

추가로 다음 정렬 항목을 확인했다.

- native PI bulk는 `0x1102/0x1103`이다. Generic parameter bulk
  `0x10C9/0x10CA`와 다르다.
- native terminal-ready 상태에서도 Stop은 성공한다. Custom Recorder도 검증된
  동일 identity에 대해 Ready/Uploading Stop을 멱등 성공으로 처리할 수 있다.
- SDO 실측 범위는 `0x1000:0` UInt32 4-byte read 하나다. 8/12-byte 지원 근거가
  아니다.
- native PI Write 성공은 custom PI/SDO Write 안전 정책을 완화할 근거가 아니다.

## 2. 분석 방법

Wireshark `tshark`의 TCP stream reassembly를 사용하고 `tcp.len > 0`인 application
payload만 분석했다. Ethernet padding이나 ACK-only packet을 application response로
세지 않았다. 모든 capture의 payload message는 하나의 TCP segment 안에 있었으며
재전송, 손실 또는 분할은 관찰되지 않았다.

Endpoint는 다음과 같다.

| 프로그램 | Client | Server |
|---|---|---|
| 신규 | `192.168.99.14:5077` | `192.168.99.20:4000` |
| 기존 | `192.168.99.14:6594` | `192.168.99.20:4000` |
| 기존 `MMCUploadDataCmd` | `192.168.99.14:7464` | `192.168.99.20:4000` |

관찰된 native little-endian envelope는 다음과 같다.

```text
request
0  UINT16 CommandId
2  UINT16 Numerator
4  UINT16 PayloadLength
6  UINT16 AxisReference

response
0  UINT16 EchoedNumerator
2  UINT16 PayloadLength
4  UINT32 Reserved
```

LASAL project-local header의 offset 2는 항상 0인 Reserved다. Native MMCLib의
Numerator/echo와 의미가 다르며 두 protocol은 binary compatible하지 않다.
축 1의 `MMC_GetPIVarInfo` request
`ef2000000400010000000000`은 offset 4의 payload length 4와 offset 6의
AxisReference 1을 직접 확인하는 anchor다.

## 3. 신규 프로그램 capture

| Capture | Native call | Wire 결과 | 판정 |
|---|---|---|---|
| `Configure_Selected.pcapng` | `0x20EF MMC_GetPIVarInfo` | input PI index 0, 16 bit, bit offset 0, `0x6041:0`, UShort, alias `I0x6041.0` | 성공. Bulk configure가 아니라 metadata 조회다. |
| `Load_PI_Catalog.pcapng` | `0x103C`, `0x202B`, `0x20EF` | `a01` -> AxisRef 0, drive info와 PI metadata 조회 | 성공. User-configured axis/index 목록이며 LASAL global catalog가 아니다. |
| `Read_EtherCAT_Health.pcapng` | `0x1130 MMC_GetCommDiagnosticsEx` | Main count 4, redundancy 0, NetworkState 0, 100-entry port counter array all zero | 성공. Path/port counter이며 Online/AL/DS402 상태가 아니다. |
| `Read_Selected_PI(Sequential).pcapng` | `0x20EB MMC_ReadPIVar` | UShort `0x02B1=689` | 성공. 여러 항목은 sequential RPC이며 same-cycle 보장이 없다. |
| `Read_Snapshot.pcapng` | `0x1102`, `0x1103` | PI bulk config 후 first value 689 반환 | 성공. Same response의 unused fixed-array tail은 값으로 해석하지 않는다. |
| `Recorder_Stop.pcapng` | `0x1038 MMC_StopRecordingCmd` | Status 0, ErrorID 0 | 성공. Ready buffer 생성 증거는 아니다. |
| `Recorder_Refresh_Status.pcapng` | `0x1037 MMC_RecStatusCmd` | `uiRr=0`, `uiSr=0x0000` | 명령은 성공했으나 Arming, ready buffer 없음이다. |
| `Recorder_Read_Header.pcapng` | `0x1036 MMC_UploadDataHeaderCmd` | `Rc/Rg/Rl/Rv/Rp/Ti/Ts=0` | 명령 envelope는 성공했지만 유효한 record가 없다. |
| `Recorder_Download.pcapng` | `0x1036 MMC_UploadDataCmd` | request `[0..63]`, response `Status=0x0010`, `ErrorID=-3` | 실패. `Rl=0`인데 64개 sample을 요청했다. |
| `Submit_SDO_0x1000_0.pcapng` | `0x203E MMC_SendSdoCmd` | Upload, length 4, `0x1000:0`, data 0, Status/Error 0 | UInt32 4-byte synchronous read 성공. Ticket은 없다. |

주요 exact payload anchor는 다음과 같다.

```text
Configure selected request
ef2000000400000000000000

Sequential PI read request/response
eb2000000400000000000000
0000080000000000b102000000000000

Recorder status request/response
371000000100000000
00000c0000000000000000000000000000000000

Recorder download request/response prefix
361000000c000000000000003f00000000000000
00007c05000000001000fdff3f000000...

SDO request/response
3e2017001000000000000000040000000000001000020000
17000c0000000000000000000400000000000000
```

SDO response의 첫 `0x0017`은 request Numerator 23의 echo이며 error가 아니다.

## 4. 기존 Elmo example capture

| Capture | Native call | 결과 및 비교 기준 |
|---|---|---|
| `MMCGetPIVarInfo.pcapng` | `0x20EF` | 신규 Configure Selected와 같은 PI metadata다. |
| `MMCGetPIVarInfoByAlias.pcapng` | `0x2100` | alias `I0x6041.0`을 input PI index 0으로 역조회한다. |
| `MMCReadPIVar.pcapng` | `0x20EF`, `0x20EB` | type 확인 후 value 689를 읽는다. 신규 sequential read와 같다. |
| `MMCWritePIVar.pcapng` | `0x20EC` | output PI index 0에 UShort 689 write 성공이다. Safety policy 기준은 아니다. |
| `MMCConfigureBulkReadPI.pcapng` | axis lookup + `0x20EF` | 4축의 PI index 0/1 metadata를 준비한다. |
| `MMCPerformBulkReadCmdPI.pcapng` | `0x1102`, `0x1103` | 8개 PI entry를 configure/read한다. 신규 Snapshot과 동일한 API pair다. |
| `MMCConfigBulkReadCmd.pcapng` | `0x10C9` | 임의 parameter preset bulk configure다. PI bulk와 별개다. |
| `MMCPerformBulkReadCmd.pcapng` | `0x10CA` | parameter preset bulk result를 반환한다. |
| `MMCBeginRecordingCmd.pcapng` | `0x1035` | `Rg=1`, `Rl=64`, `Rc=0`, Rv/Rp zero, success. 신호 없는 기록이라 data 기준으로 쓰지 않는다. |
| `MMCRecStatusCmd.pcapng` | `0x1037` | `uiRr=0`, `uiSr=0x0104`: No Trigger + Buffer 1 ready다. |
| `MMCStopRecordingCmd.pcapng` | `0x1038` | 위 terminal-ready 상태 뒤에도 success다. |
| `MMCUploadDataHeaderCmd.pcapng` | `0x1036` | `Rg=1`, `Rl=64`, `Ti=0`, `Ts=1000`, success다. |
| `MMCUploadDataCmd.pcapng` | `0x1036` | `[0..63]` request 후 application response 없이 server FIN이다. 성공 기준에서 제외한다. |

기존 PI catalog 실측은 다음과 같다.

| Axis/ref | PI index | Bit offset | PDO | Type | Alias |
|---|---:|---:|---|---|---|
| a01 / 0 | 0 | 0 | `0x6041:0` | UShort | `I0x6041.0` |
| a01 / 0 | 1 | 16 | `0x6064:0` | Int32 | `I0x6064.0` |
| a02 / 1 | 0 | 160 | `0x6041:0` | UShort | `I0x6041.0` |
| a02 / 1 | 1 | 176 | `0x6064:0` | Int32 | `I0x6064.0` |
| a03 / 2 | 0 | 320 | `0x6041:0` | UShort | `I0x6041.0` |
| a03 / 2 | 1 | 336 | `0x6064:0` | Int32 | `I0x6064.0` |
| a04 / 3 | 0 | 480 | `0x6041:0` | UShort | `I0x6041.0` |
| a04 / 3 | 1 | 496 | `0x6064:0` | Int32 | `I0x6064.0` |

BitOffset은 진단 가치가 있지만 현재 custom D1 catalog v1 schema에는 없다. Reserved
field를 재해석하지 않고 향후 explicit schema version에서만 추가한다.

## 5. Recorder failure reconstruction

Native `uiSr`의 lower 8 bits는 phase, 다음 8 bits는 ready buffer mask다.

| Value | Phase |
|---:|---|
| 0 | Arming |
| 1 | Waiting Opposite Trigger |
| 2 | Waiting Trigger |
| 3 | Trigger Detected |
| 4 | No Trigger |

| Ready mask | 의미 | PMAS buffer index |
|---:|---|---:|
| 0 | none | - |
| 1 | Buffer 1 ready | 0 |
| 2 | Buffer 2 ready | 1 |
| 3 | both ready | 0, 1 |

신규 capture의 순서는 다음과 같다.

```text
Stop success
  -> Status uiSr=0x0000 (no ready buffer)
  -> Header Rl=0
  -> Download [0..63]
  -> Status=0x0010, ErrorID=-3
```

이 값은 raw byte 추정만이 아니다. MMCLibDotNET v3.0.0.7의
`UploadDataArgsOut.DataIn(1412, response)`을 little-endian으로 적용한 결과가
`Status=0x0010`, `ErrorID=-3`, `UpdatData[0]=63`으로 재현됐다. Status/Error가
nonzero이므로 `63`은 sample success data로 사용하면 안 된다.

따라서 올바른 workflow는 다음이다.

```text
Begin Recording
  -> Stop or natural completion
  -> poll Status until selected buffer is ready
  -> read Header and require Rl > 0
  -> require 0 <= From <= To < Rl
  -> Download the same ready buffer
  -> optionally Export CSV from PC memory
```

## 6. 구현 반영 판정

1-5는 2026-07-21 source/document에 반영 완료했다. 6은 설계 상한을 확정했으며 PLC
실행부는 다음 증분이다.

1. PMAS Health row filter와 표시 문자열에 `InvalidFramesPort0..3`를 포함한다.
2. PMAS Recorder에 selected PI -> native `uiRv`/bit mask conversion helper를 추가한다.
3. PMAS Recorder가 `uiSr`, selected buffer readiness, global header `Rl`과 range를
   검증한 뒤에만 header/download를 수행한다.
4. Custom LASAL Recorder는 identity, MapRevision, BootId, owner 검사를 통과한
   Ready/Uploading Stop을 idempotent success로 처리한다.
5. D2 대응 native API를 PI bulk `0x1102/0x1103`으로 정정한다.
6. 최초 D5 증분은 native 실측 범위에 맞춰 4-byte SDO Read-only와
   `MaxSdoDataBytes=4`로 제한했다. 분석 당시 baseline은 `CapabilityBits=0x0000003F`,
   `MaxSdoDataBytes=0`이었고, legacy test source는 `0x0000013F`를 광고했다. 해당 후속
   capture에서 Slave 1~4 `0x1000:0` UInt32 4-byte happy path가 모두
   Completed/Success를 반환했다. 이 사실은 역사적 fixed-vector runtime 증거로 보존한다.
   현재 source는 bit 13 `SDOReadGeneralInline`을 추가한 `0x0000213F`를 광고하고,
   nonzero ObjectIndex, 임의 U8 SubIndex와 ValueType에 정확히 맞는 1/2/4-byte Read를
   허용한다. Write, 8/12-byte와 extended result는 비활성이다. general-inline runtime,
   fault matrix와 mailbox frame 독립 관측 전에는 production 승인값이 아니다.

반영하지 않는다.

- Native opcode 또는 native binary layout을 custom `0x7Exx` protocol에 복제하지 않는다.
- Native direct PI Write를 근거로 custom Write capability/allowlist를 열지 않는다.
- Native fixed array의 stale tail bytes를 custom response에 재현하지 않는다.
- `MMCUploadDataCmd.pcapng`는 response가 없으므로 download 성공 baseline으로 쓰지 않는다.

## 7. 남은 실기 검증

이 분석만으로 다음은 완료 판정할 수 없다.

- Custom `0x7E10`, `0x7E45`, `0x7E46`, `0x7E50`의 live PLC capture
- Custom Recorder가 실제로 `1 x Header + N x Chunk`로 동작하는지
- D3/D4 sample timing, CRC, reconnect/adopt와 immutable upload
- D5 4-byte read의 busy, timeout, cancel, disconnect/orphan 처리
- PMAS UI에서 `uiSr=0x0000` 차단과 `0x0104` ready flow의 smoke test

## 8. 재현 명령

```powershell
$tshark = 'C:\Program Files\Wireshark\tshark.exe'
$captureRoot = 'C:\work\Elmo\Elmo_Master\test\packet_capture\Elmo_API_Analyze'

Get-ChildItem -LiteralPath $captureRoot -Recurse -Filter '*.pcapng' |
  Sort-Object FullName |
  ForEach-Object {
    "===== $($_.FullName) ====="
    & $tshark -r $_.FullName -o tcp.desegment_tcp_streams:true `
      -Y 'tcp.len > 0' -T fields -E separator='|' `
      -e frame.number -e frame.time_relative `
      -e ip.src -e tcp.srcport -e ip.dst -e tcp.dstport `
      -e tcp.stream -e tcp.seq_raw -e tcp.len -e tcp.payload
  }
```
