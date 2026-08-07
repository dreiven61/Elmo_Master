# BootId 0x1B Group Stop, safe shutdown, and axis-status runtime evidence

작성일: 2026-08-05 (KST)

## 1. Evidence set and boundary

This report analyzes the four Test5 text/pcap pairs captured on the established TCP session
`10.10.150.13:3125 <-> 10.10.150.1:4000`.

| Evidence | SHA-256 |
|---|---|
| `GroupStop.txt` | `2953D67C0BDDA129E441C4405729B09C1B682EEB53DE3C2633954D4F7D75E7DA` |
| `GroupStop.pcapng` | `332506B76B561CAC2DCC7D8A454E17846A812ABB1AD415F5ED99BBE26E607BCB` |
| `Stable=3_3.txt` | `4C60B86545A76CF6FD494CC4AFA83A4C7EAF402014530BA9F108F404CDD7D479` |
| `Stable=3_3.pcapng` | `14318D9EB4D045961057C41BDA16896DC5509C0BEBD3DB5F35EF5563D8395088` |
| `안전 종료 확인.txt` | `DE71ABF317E280684C5319F0AE00203E0348ADB2746F93E375EAB19C0A50D6E1` |
| `안전 종료 확인.pcapng` | `0953C447930D684AF761B8248B4EE4644AEC904C84EDED16CDA5D984599C825C` |
| `축별 드라이브 상태 읽기.txt` | `5CDEB5056B28765E9A61294B4D0F8BBB79F69CA721BFFC81F2A377AEC4498610` |
| `축별 드라이브 상태 읽기.pcapng` | `409A0A455F436F5B81F949C6EA53386618032BA306B62C42DAC3412319E26CA2` |

The capability responses in the Group Stop and safe-shutdown captures contain
`BootId=0x0000001B` and `MapRevision=0x957F101E`, matching the preceding Home/group checkpoint.
The Stable and per-axis captures contain no capability response of their own. Their same-session
association is instead supported by the identical TCP four-tuple and continuous raw sequence/ACK
boundaries across the four files. No SYN, FIN, RST, retransmission, fast retransmission, lost
segment, out-of-order segment, or duplicate ACK was found in any capture. All four captures begin
and end in an already established session, so they do not prove connection establishment or
liveness after capture end. These facts remove obvious TCP replay/loss as an explanation for the
results, but do not prove electrical STO, drive-main-power removal, or mechanical stopping distance.

## 2. Actual in-motion Group Stop

`GroupStop.pcapng` contains three `0x20A4` Group absolute moves, three `0x2085` Group Stops,
55 `0x2045` Group status reads, three `0x2051` position reads, and nine `0x7E00` capability
queries. Every request has one response, there is no TCP retransmission, and each trial has exactly
one `0x20A4` mutation and one `0x2085` mutation. Status and capability reads are intentional repeats.

| Trial | Wire Move request / non-standstill response | Wire Stop request / response | Post-Stop status proof |
|---:|---|---|---|
| 1 | `16:25:06.343212` / `16:25:06.619936` | `16:25:07.472642` / `16:25:07.474037` | five polls; final three `0x40060000` |
| 2 | `16:25:08.992637` / `16:25:09.271985` | `16:25:10.014761` / `16:25:10.016057` | five polls; final three `0x40060000` |
| 3 | `16:25:11.331141` / `16:25:11.608039` | `16:25:12.466875` / `16:25:12.468163` | five polls; final three `0x40060000` |

For every trial, the status at motion observation and the first two post-Stop samples is
`0x40040000`. The final three samples are `0x40060000`: the project-local Group power-ready bit
and the Maestro-compatible standby bit are both set. Current SDK semantics require that standby
bit to be powered, profile-locked, and in-position. The text log independently reports
`Stable=3/3` and terminal safety verification PASS.

The first move starts at position `963235` and the next trial starts at `957592`; the second next
trial starts at `952489`. Each target was zero, and the third log estimated roughly 480 seconds to
natural completion before Stop was issued about one second later. Therefore these are genuine
non-standstill Stop trials, not natural target completion mislabeled as Stop success. The Stop ACK
is only dispatch acceptance; terminal proof comes from the later three status-only samples.

## 3. Independent stable-status sample

`Stable=3_3.pcapng` contains exactly three `0x2045` requests and three responses. All response
states are `0x40060000` with zero function error and zero Group error. It independently confirms
three consecutive powered locked-standby observations from `16:30:05.753` through
`16:30:07.529`. The text file records three separate manual `Read Group Status` actions; this pair
does not prove an internal continuation counter, one verifier invocation, or its sampling cadence.

## 4. Group Disable and Power On/Off shutdown sequence

The safe-shutdown capture contains one `0x2048` Group Disable, four `0x204B` Group Power Off,
three `0x204A` Group Power On, 24 `0x2045` status reads, and two `0x7E00` capability queries.
Each mutation appears exactly once in its phase and is followed by three status-only reads.

| Phase | Mutation result | Three-sample terminal state |
|---|---|---|
| Disable | one success response | `0x40050000`: powered, disabled/unlocked |
| Initial Power Off | one success response | `0x40010000`: power-ready clear, disabled |
| Power On 1 | one success response | `0x40050000` |
| Power Off 2 | one success response | `0x40010000` |
| Power On 2 | one success response | `0x40050000` |
| Power Off 3 | one success response | `0x40010000` |
| Power On 3 | one success response | `0x40050000` |
| Final Power Off | one success response | `0x40010000` |

The last status response at `16:34:38.714` is `0x40010000`, followed by the text-log PASS at
`16:34:38.730`. Verification did not replay a Power command. This closes the software-level Group
Disable and final stable Power Off gate for BootId `0x1B`.

## 5. Per-axis `0x2028` status capture

Across the new capture there are seven successful axis lookup/info pairs while selecting and
re-selecting `_LMCAxis1..4`, interleaved with five `Read Axis Status` actions. The responses are:

| Axis | Reads | Native LASAL state | Function status | ErrorId | AxisErrorId | reserved StatusWord |
|---:|---:|---|---:|---:|---:|---:|
| 1 | 2 | `0x2290020E` both times | `0` | `0` | `0` | `0` |
| 2 | 1 | `0x22D0020E` | `0` | `0` | `0` | `0` |
| 3 | 1 | `0x22D0020E` | `0` | `0` | `0` | `0` |
| 4 | 1 | `0x22D0420E` | `0` | `0` | `0` | `0` |

All four native state words have the project-local `Referenced` and `Standstill` bits set and the
`PowerOn` bit clear; Axis1 repeats the identical state after five seconds. The wire contains only
`0x2028` for these five reads. It contains no `0x7E50` SubmitSDO and no `0x7E03` operation-status
poll. Therefore this capture proves successful LASAL Power Off, Standstill, Referenced, and
`AxisErrorId=0` observations; it does **not** read or prove DS402 `0x6041`, operation mode
`0x6061`, or drive error code `0x603F`. The `StatusWord` field returned by current `0x2028` is
reserved zero and must not be interpreted as DS402 `0x6041`.

The remaining set native bits are also decoded rather than discarded. All four axes report
`InPosition`, `FiltRdy`, `EmergStop`, `NoRefMeth`, `NoPostRtWork`, and `ReadyToPowerOn`;
Axis2..4 additionally report `NoPreRtWork`, while Axis4 alone reports `ActDirFlg`. The vendor
definition permits `EmergStop` when an axis is deactivated as well as when an error stops it, so
`PowerOn=0` together with `AxisErrorId=0` does not make that bit an error proof. The tracked
canonical Motion Network connects only `_LMCAxis1.LMCPreRtWorkTrigger` to
`LMCEcatInputLatch1.ClassSvr`, connects no axis `LMCPostRtWorkTrigger`, and connects no axis
`LMCReference`. The observed `NoPreRtWork`/`NoPostRtWork`/`NoRefMeth` pattern therefore matches
the current topology and is not classified as a new runtime fault by this capture.

Historical captures happened to show the same native LASAL state words together with separate SDO
reads, but their old SDO values cannot be transferred to this BootId `0x1B` observation.

## 6. Closed and remaining runtime gates

Closed on BootId `0x1B` by this evidence:

- actual non-standstill Group Stop, exactly one Stop per trial, three trials;
- status-only stable Group Standby `3/3` after every Stop;
- Group Disable readback, repeated Power On/Off readback, and final stable Power Off;
- successful `0x2028` LASAL status and zero AxisErrorId for Axis1..4.

Still open:

- per-axis physical-drive composite read: `0x2028`, then fixed SDO `0x6041:0` and `0x6061:0`;
- per-axis separate drive error-code SDO `0x603F:0`;
- single-axis motion and true in-motion Axis Stop, Axis1 first and then Axis2..4;
- electrical safe-state/STO and measured stopping behavior if those are qualification requirements;
- `AxisRebaseRequiredState` restart/power-loss retention and the remaining ownership semantic gates.
