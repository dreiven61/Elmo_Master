# Response model design

Date: 2026-07-09

> **Status update 2026-07-16:** The initial `Question` and `Current Design
> Assessment` sections below are the historical pre-implementation baseline.
> Phase 1/2 are implemented in the current `LMC_Response`, command-specific typed
> parsers/results and defensive array copies. Phase 3 typed callback payloads
> remain pending because no callback datagram schema has been captured. See
> `../../../docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`.

## Question

The current public `LMC_Response` type is:

```csharp
public sealed class LMC_Response
{
    public byte[] Raw { get; internal set; }
    public ushort Status { get; internal set; }
    public short ErrorId { get; internal set; }

    public bool IsSuccess
    {
        get { return Status == 0 && ErrorId == 0; }
    }
}
```

`LMCConnection.Parse(...)` currently reads:

- `Status` from `raw.Length - 4`
- `ErrorId` from `raw.Length - 2`

## Evidence

The packet analysis defines the common response header like this:

| Offset | Size | Meaning |
|---:|---:|---|
| 0 | 2 | response status or reserved, normally `0` |
| 2 | 2 | response payload length |
| 4 | 4 | reserved, normally `0` |
| 8 | N | payload |

So response payload length is at offset `2`, and payload starts at offset `8`.
The response format is not simply "last 4 bytes are status/error" for every
command.

Current LASAL ack-style responses often put command result data near the end of
the payload, for example:

- payload offset `0`: reserved/result value
- payload offset `4`: command status
- payload offset `6`: error id

That makes the current tail parsing work for some 16-byte acknowledgement
responses, but it is not a valid generic response parser.

## Current Design Assessment

The current `LMC_Response` is acceptable as a temporary acknowledgement wrapper.

It is not acceptable as the final response model.

### Strengths

- Simple public API.
- Existing `Power`, `Reset`, `Stop`, and motion methods can return something
  without introducing many response classes.
- It preserves `Raw`, so no data is destroyed.
- It matches some current LASAL acknowledgement responses by accident or by
  local convention.

### Weaknesses

- It does not expose the real response header:
  - header status/reserved at offset `0`
  - payload length at offset `2`
  - header reserved field at offset `4`
- `Status` and `ErrorId` are parsed from the end of the raw packet, not from a
  command-specific schema.
- Lookup responses (`GetAxisByName`, `GetGroupByName`) carry reference values,
  not a generic tail status/error pair.
- Value responses (`ReadStatus`, `ReadPosition`) carry a value at payload offset
  `0`; if parsed generically as tail status/error, the value can be mistaken for
  an error structure.
- RPC init response has a captured payload length of `24`, but the payload
  structure is not fully decoded yet.
- It cannot distinguish malformed transport data, command failure, lookup
  failure, and a valid command returning a nonzero value.
- It encourages callers to treat `IsSuccess` as authoritative even when the
  response type does not actually contain a command status/error pair.

## Direction

Keep `LMC_Response` only as a compatibility facade, but redesign the internal
response model around a parsed response envelope plus command-specific payload
parsers.

The correct model is:

1. Parse the common response envelope.
2. Parse the payload according to the request command.
3. Return a strongly named result where the command has a meaningful value.
4. Preserve `Raw` for debugging and packet comparison.

## Proposed Types

### Common Envelope

```csharp
public sealed class LMC_Response
{
    public byte[] Raw { get; internal set; }
    public ushort HeaderStatus { get; internal set; }
    public ushort PayloadLength { get; internal set; }
    public uint HeaderReserved { get; internal set; }
    public byte[] Payload { get; internal set; }

    public bool IsFrameValid { get; internal set; }
    public bool HasCommandResult { get; internal set; }
    public ushort CommandStatus { get; internal set; }
    public short ErrorId { get; internal set; }

    public bool IsSuccess
    {
        get
        {
            return IsFrameValid
                && HeaderStatus == 0
                && (!HasCommandResult || (CommandStatus == 0 && ErrorId == 0));
        }
    }
}
```

Compatibility aliases may remain for one release:

- `Status` can return `CommandStatus` when `HasCommandResult`, otherwise
  `HeaderStatus`.
- `ErrorId` can stay as-is.

But new code should use `HeaderStatus`, `CommandStatus`, and `HasCommandResult`
explicitly.

### Command-Specific Results

Use command-specific parsing internally:

| Command group | Result shape |
|---|---|
| `0x8080` RPC init | `LMC_Response` envelope, raw payload retained |
| `0x405C` callback registration | acknowledgement result |
| `0x405D` close | acknowledgement result |
| `0x103C` axis lookup | `ushort AxisReference` plus envelope |
| `0x1042` group lookup | `ushort GroupReference` plus envelope |
| `0x2023`, `0x2024`, `0x2022`, motion commands | acknowledgement result |
| `0x2028` read status | `uint StatusRegister` plus envelope |
| `0x202E` read position | `int ActualPosition` plus envelope |
| group read status | `uint StatusRegister` plus envelope |
| `0x2051` group actual position | LASAL-DINT `int[16]`, coordinate context, status/error |
| `0x20D2` group members | 16-axis reference/device/name/count typed result |

Do not force all of these into `Status/ErrorId`.

## Parsing Rules

Common parser:

1. Require at least 8 bytes.
2. Read `HeaderStatus` from offset `0`.
3. Read `PayloadLength` from offset `2`.
4. Read `HeaderReserved` from offset `4`.
5. Verify `raw.Length == 8 + PayloadLength`.
6. Copy payload bytes from offset `8`.

Acknowledgement parser:

1. Parse the common envelope.
2. If payload length is exactly 4:
   - payload `[0]` is `CommandStatus`
   - payload `[2]` is `ErrorId`
   - mark `HasCommandResult = true`
3. Otherwise, if payload length is exactly 8:
   - payload `[4]` is `CommandStatus`
   - payload `[6]` is `ErrorId`
   - mark `HasCommandResult = true`
4. Do not use this generic parser for structured/value payloads solely because
   they are longer than 8 bytes; select the parser by command schema.

Value parser:

1. Parse the common envelope.
2. Read the command value from payload offset `0`.
3. Do not parse value bytes as `CommandStatus/ErrorId` unless the command
   schema explicitly includes that pair.

Lookup parser:

1. Parse the common envelope.
2. Read lookup reference from payload offset `4` for current captured lookup
   responses.
3. Do not use tail status/error parsing.

## Migration Plan

### Phase 1: Non-breaking cleanup

Status: implemented on 2026-07-09.

- Expand `LMC_Response` with envelope fields:
  - `HeaderStatus`
  - `PayloadLength`
  - `HeaderReserved`
  - `Payload`
  - `IsFrameValid`
  - `HasCommandResult`
  - `CommandStatus`
- Keep `Status`, `ErrorId`, and `IsSuccess` for compatibility.
- Replace `LMCConnection.Parse(...)` with envelope parsing.
- Add internal helper parsers:
  - `ParseAcknowledgement`
  - `ParseLookupReference`
  - `ParseInt32Value`
  - `ParseUInt32Value`

Implementation note:

- `Status` remains as a compatibility alias.
- Acknowledgement-returning command methods now use `ParseAcknowledgement`.
- On 2026-07-10, `ParseAcknowledgement` was extended to parse captured 4-byte
  callback/close-style acknowledgements at payload offsets `0` and `2`.
- Lookup and value methods now use command-specific parser helpers.
- Typed public result objects are not added in Phase 1.

### Phase 2: Typed public results

Status: implemented on 2026-07-10.

- Added command-specific result objects:
  - `LMCReadStatusResult`
  - `LMCReadActualPositionResult`
  - `LMCGroupReadStatusResult`
  - `LMCGroupReadActualPositionResult`
  - `LMCGroupMembersInfoResult`
  - `LMCGroupMemberInfo`
- Existing methods can remain:
  - `GetActualPosition(out LMC_Response response)`
  - `ReadStatus(out LMC_Response response)`
- New methods expose richer results without breaking existing callers:
  - `ReadStatusResult()`
  - `GetActualPositionResult()`
  - `GroupReadStatusResult()`
  - `GroupReadActualPosition(LMC_COORD_SYSTEM)`
  - `GetGroupMembersInfoResult()`
- `AxisInfoResponse` preserves and validates the constructor-time 8-byte ACK.
- axis/group lookup accepts exactly 6 payload bytes and rejects descriptor `0`.
- malformed typed payloads throw `InvalidDataException`; they do not return a
  numeric zero.
- a valid 4-byte command-error envelope is returned as an unsuccessful typed
  result with its status/error context; it is not misclassified as malformed.
- `LMCReadStatusResult.IsPowerOn` and `IsStandstill` expose canonical LASAL
  `_LMCAXIS_STATUS` bit 0 and bit 25 without PMAS mask reuse.
- `LMCReadStatusResult.StatusWord` keeps the captured field offset, but the
  canonical LASAL handler currently returns reserved value `0` until DS402
  StatusWord wiring is approved.
- `0x20D2` is parsed by exact offsets and exact 1350-byte payload length.
- `0x2051` LASAL-DINT v1 success is parsed as exact 68-byte payload:
  `DINT[16] + UINT16 function status + INT16 error`. The captured 136-byte
  legacy LREAL response is rejected rather than reinterpreted. A 4-byte
  command-error envelope remains a valid unsuccessful typed result.
- group position/member arrays are returned through defensive copies.

### Phase 3: Callback parser

- Keep callback payloads raw until captures exist.
- After callback captures exist, create `LMCCallbackMessage` and parse by
  callback command/event id.

## Recommendation

Do not keep the current tail-based `LMC_Response` parser as the final design.

The correct direction is to treat `LMC_Response` as a response envelope and use
command-specific payload parsing. This keeps the current API usable, prevents
read values from being misinterpreted as errors, and leaves room for RPC init
and callback payloads whose structure is not fully decoded yet.
