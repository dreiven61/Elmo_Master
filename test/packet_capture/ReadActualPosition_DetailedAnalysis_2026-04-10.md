# ReadActualPosition.pcapng Detailed Analysis

- Source file: `packet_capture/ReadActualPosition.pcapng`
- Target IP focus: `192.168.1.3`
- Analysis date: `2026-04-10`

## 1) Capture metadata

- Packet count: `33`
- Capture duration: `14.3062808 s`
- File size: `4572 bytes`
- TCP conversation with controller:
  - `192.168.1.13:4867 <-> 192.168.1.3:4000`
  - `3` packets total in this conversation (request, response, ACK)

## 2) Relevant packets (only IP 192.168.1.3 flow)

| Frame | RelTime(s) | Src | Dst | TCP Len | Flags | Payload Hex |
|---|---:|---|---|---:|---|---|
| 7 | 9.229724900 | 192.168.1.13:4867 | 192.168.1.3:4000 | 9 | PSH,ACK | `2e2000000100000000` |
| 8 | 9.229908500 | 192.168.1.3:4000 | 192.168.1.13:4867 | 24 | PSH,ACK | `000010000000000000000080550160410000000053558741` |
| 9 | 9.272621500 | 192.168.1.13:4867 | 192.168.1.3:4000 | 0 | ACK | (none) |

## 3) Timing

- Request -> Response latency:
  - `frame8.time - frame7.time = 0.0001836 s = 0.1836 ms`
- Response -> pure ACK:
  - `frame9.time - frame8.time = 0.0427130 s = 42.7130 ms`
- Interpretation:
  - Application-level response is fast (`~0.184 ms`).
  - The late pure ACK is TCP delayed ACK behavior and should not be treated as controller processing delay.

## 4) Payload decode (little-endian)

## 4-1) Request payload (`9 bytes`)

Hex:
`2e 20 00 00 01 00 00 00 00`

| Offset | Size | Type | Value | Note |
|---:|---:|---|---|---|
| 0 | 2 | `uint16` | `0x202E` | ReadActualPosition request command ID candidate |
| 2 | 2 | `uint16` | `0` | AxisRef candidate |
| 4 | 4 | `uint32` | `1` | Payload/option candidate (observed fixed in this sample) |
| 8 | 1 | `uint8` | `0` | Execute/reserved candidate |

## 4-2) Response payload (`24 bytes`)

Hex:
`00 00 10 00 00 00 00 00 00 00 00 80 55 01 60 41 00 00 00 00 53 55 87 41`

| Offset | Size | Type | Value | Note |
|---:|---:|---|---|---|
| 0 | 2 | `uint16` | `0` | status/error field candidate |
| 2 | 2 | `uint16` | `16 (0x0010)` | payload/status field candidate |
| 4 | 4 | `uint32` | `0` | status/error field candidate |
| 8 | 8 | `double` | `8391340.0` | strongly likely actual position value |
| 16 | 8 | `double` | `48933472.0` | auxiliary motion field; meaning needs protocol confirmation |

## 5) Cross-check against larger captures

Using `packet_capture/motion_tes2t_4000.tsv`:
- `24-byte` responses repeatedly decode as:
  - first `double` (offset 8): changing with motion
  - second `double` (offset 16): mostly constant around `48933472.0`

Conclusion:
- In `ReadActualPosition` response, offset `8..15` is highly likely position.
- Offset `16..23` is another motion-related numeric field, but exact semantic label (velocity setpoint vs scale vs other) is not provable from this pcap alone.

## 6) Confidence and limits

- High confidence:
  - Request/response pair identification
  - TCP payload lengths (`9` and `24`)
  - Endianness and numeric decode
  - Req->Rsp latency (`0.1836 ms`)
- Medium confidence:
  - `0x202E` as exact command ID naming
  - field labels at offsets `0..7`
- Low confidence (needs official protocol spec or more labeled captures):
  - exact meaning of response `double` at offset `16..23`

