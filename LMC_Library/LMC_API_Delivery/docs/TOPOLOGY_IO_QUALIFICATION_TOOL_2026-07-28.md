# EtherCAT topology / CREVIS read qualification tool

## Purpose and scopes

`LasalMotionControlLib.Tests.exe topology-io-qualify` is an internal raw-read
qualification tool. It does not add a capability bypass to the production SDK or WPF.
Use an explicit `--scope` in every documented invocation.

| Scope | Current use | Raw request allowlist | Request count |
|---|---|---|---:|
| `topology-inventory` | Qualify the currently implemented static configured topology before T2 node-health/I/O ownership is installed | `0x7E11`, `0x7E12` only | 8 |
| `integrated-read-owner-dormant` | Qualify the later dormant `0x7E13` node-health and `0x7E22` digital-I/O read integration before capability bits 15/16 are enabled | `0x7E11`, `0x7E12`, `0x7E13`, `0x7E22` | 17 |

The `topology-inventory` scope is the safe scope for the current LASAL source. It sends
one topology-info request and seven single-entry chunk requests. It never sends
`0x7E13`, `0x7E22`, `0x7E23`, SDO/PI/motion/resource commands, or any other mutation.
Public capability reads and connection lifecycle traffic required by the transport are
outside the raw qualification allowlist.

## Common fail-closed and evidence contract

- Live execution requires the confirmation token matching the selected scope, explicit
  host/local IPv4 addresses, and an explicit report path.
- The report target must not already exist. The tool reserves a sibling
  `.inprogress-*.tmp` file before network access, appends and durably flushes evidence,
  and only moves the completed report to the requested path at the end. A checkpoint
  write failure stops the run without truncating the already recorded prefix.
- Every raw request must have an allowed command and exact payload length, zero header
  reserved/reference fields, schema version 1, zero flags, and a nonzero RequestId.
- `0x7E23` and all mutation commands are rejected in both scopes.
- The tool records the request/response bytes and SHA-256 for each raw exchange.

## Build

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj `
  /t:Build /p:Configuration=Release /p:Platform=AnyCPU
```

## Static topology inventory scope

### Dry run

```powershell
& LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin\Release\LasalMotionControlLib.Tests.exe `
  topology-io-qualify `
  --scope topology-inventory `
  --dry-run
```

The dry run performs no network I/O. It emits only `0x7E11` and `0x7E12` sample
frames and explicitly records `0x7E13`, `0x7E22`, and `0x7E23` as forbidden.

### Live run

```powershell
$report = 'C:\work\Elmo\evidence\topology-inventory-20260728.txt'
& LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin\Release\LasalMotionControlLib.Tests.exe `
  topology-io-qualify `
  --scope topology-inventory `
  --execute-live `
  --confirm PLC-RAW-TOPOLOGY-INVENTORY-READ `
  --host 192.168.0.3 `
  --local 192.168.0.10 `
  --port 4000 `
  --timeout-ms 3000 `
  --output $report
```

Replace both IPv4 addresses with the actual PLC and PC settings. Before and after the
eight raw topology requests, this scope reads capabilities and requires:

- capability bit 14 `EtherCATTopology` set;
- nonzero `DiagnosticsBootId`;
- request/response capacities large enough for the topology chunk contract;
- unchanged `DiagnosticsBootId`, `DiagnosticsBuild`, `CapabilityBits`, `MapRevision`,
  `MaxRequestPayloadBytes`, and `MaxResponsePayloadBytes` across the snapshot.

It then requires canonical topology revision/CRC `0x15867EEC`, seven entries, 96-byte
entry stride, one entry per chunk, five configured slaves, two slot modules, four axes,
and this exact order and identity:

| Index | NodeId | Name | Kind / parent | Vendor / product | Extra identity |
|---:|---|---|---|---|---|
| 0 | `0xEC000001` | `GL_9086_11` | EtherCAT slave | `0x0000029D / 0x474C9086` | MasterSlaveIndex 0 |
| 1 | `0xEC000101` | `Elmo_11` | EtherCAT slave | `0x0000009A / 0x00030924` | MasterSlaveIndex/SDO/axis 1 |
| 2 | `0xEC000102` | `Elmo_21` | EtherCAT slave | `0x0000009A / 0x00030924` | MasterSlaveIndex/SDO/axis 2 |
| 3 | `0xEC000103` | `Elmo_31` | EtherCAT slave | `0x0000009A / 0x00030924` | MasterSlaveIndex/SDO/axis 3 |
| 4 | `0xEC000104` | `Elmo_41` | EtherCAT slave | `0x0000009A / 0x00030924` | MasterSlaveIndex/SDO/axis 4 |
| 5 | `0xEC010001` | `GL_9086_1_Slot001` | Input slot / parent `0xEC000001` | `0x0000029D / 0x475412FA` | Slot 0, input 4 bytes, IOReference `0x00010001` |
| 6 | `0xEC010002` | `GL_9086_1_Slot011` | Output slot / parent `0xEC000001` | `0x0000029D / 0x475422BA` | Slot 1, output 4 bytes, IOReference `0x00010002` |

The parser validates the complete canonical CRC and every entry field, including flags,
revision, slot/axis references, byte widths, and names. `OVERALL_RESULT=PASS`,
`RAW_SCHEMA_RESULT=PASS`, and `LIVE_GATE_RESULT=STATIC_TOPOLOGY_ONLY` prove only the
configured static inventory and stable diagnostics identity. They do not prove current
EtherCAT state, node health, cable/disconnect behavior, physical DI, output shadow, or
PLC output control.

## Integrated read-owner dormant scope

Use this only after the LASAL IDE T2 structure, `0x7E13/0x7E22` implementations, route,
and read-owner wiring exist while capability bits 15/16/17 remain off.

```powershell
$report = 'C:\work\Elmo\evidence\topology-io-dormant-20260728.txt'
& LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin\Release\LasalMotionControlLib.Tests.exe `
  topology-io-qualify `
  --scope integrated-read-owner-dormant `
  --execute-live `
  --confirm PLC-RAW-TOPOLOGY-IO-READ `
  --host 192.168.0.3 `
  --local 192.168.0.10 `
  --port 4000 `
  --timeout-ms 3000 `
  --output $report
```

This scope additionally requires capability bits 15 `EtherCATNodeHealth`, 16
`DigitalIORead`, and 17 `DigitalIOWrite` to remain zero. In fixed order it sends one
`0x7E11`, seven `0x7E12`, seven configured-node `0x7E13`, and two `0x7E22` reads for
the GT-12FA input and GT-22BA output shadow: 17 raw requests total.

A PASS proves the dormant parser/schema/coherency path and unchanged capability
identity. It still does not prove physical input, terminal voltage, cable transitions,
or output write. The report therefore records
`LIVE_GATE_RESULT=REQUIRES_PHYSICAL_CORRELATION` and
`PCAP_EVIDENCE=NOT_CAPTURED_BY_TOOL`. Capability bits 15/16 may be enabled only after
the separate normal, disconnect/reconnect, and 32-pattern physical-correlation matrix is
captured. `0x7E23` and bit 17 remain outside this tool and gate.
