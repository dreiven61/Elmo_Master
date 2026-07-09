# API Structure Decision 2026-07-09

## Scope

This decision applies to `LMC_Library/LMC_API_Delivery`.

`Codex_LASAL_WPF` is a dummy/test implementation and is not the source of truth
for the delivered DLL.

## Packet Model

The delivered DLL keeps the PMAS/LMC-style object model:

1. A connection object owns the TCP session.
2. An axis object is created with an axis name and connection.
3. The axis object resolves the name once with `GetAxisByName`.
4. The axis object stores the returned axis reference.
5. Motion/status methods use the stored reference when building packets.

The caller should not pass `axisName` or `axisRef` to every motion method.
The axis object already owns that state.

For multi-PC operation, the TCP session must become an explicit socket-scoped
LMC session on the LASAL side. Keep the current 8-byte motion header unchanged
until both the PC DLL and LASAL parser are updated together. See
`SESSION_MANAGEMENT_DESIGN_2026-07-09.md`.

`LMCConnection.RpcInitConnection(...)` now follows the captured RPC connection
sequence (`0x8080` session init, then `0x405C` callback registration). See
`RPC_CONNECTION_PACKET_DECISION_2026-07-09.md`.

## Unit Policy

Unit conversion is outside the API implementation.

Motion methods accept values that are already in the LASAL/internal DINT unit
expected by the PLC parser. The API library may declare `LMC_Units` constants
for caller convenience, but packet-building code must not reference them and
the API library must not provide unit converter classes.

This keeps the conversion responsibility explicit:

- application/user code chooses the unit constants and conversion rule
- API code builds packets from already-converted DINT values
- PLC code receives DINT values and passes them to LASAL motion blocks

## Naming Policy

Primary public methods should follow the PMAS/LMC-style wrapper style where
practical:

- `LMCSingleAxis`
- `LMCGroupAxis`
- `MoveAbsoluteEx`
- `MoveRelativeEx`
- `MoveVelocityEx`
- `GetActualPosition`
- `ReadStatus`
- `PowerOn`
- `PowerOff`
- `Reset`
- `Stop`

Legacy `LMCAxis`, `LMCGroup`, and `LMC_*` method names may remain as
compatibility wrappers, but they must delegate to the primary API and must not
perform hidden unit conversion.

## Consequence

If a caller wants to command `1.0 mm`, the caller must convert it before calling
the API, for example:

```csharp
const int MM = 10000;
var position = checked((int)Math.Round(1.0 * MM));
axis.MoveAbsoluteEx(position, velocity, acceleration, deceleration, jerk);
```

The packet builder receives `position` as an `int` and writes that value
directly to the DINT payload.
