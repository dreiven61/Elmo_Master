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

Unit conversion is owned by the PC application that calls the API.

Motion methods accept values that are already in the LASAL/internal DINT unit
expected by the PLC parser. The API library may declare `LMC_Units` constants
for caller convenience, but packet-building code must not reference them and
the API library must not provide unit converter classes.

The normative conversion rule is:

```text
transmit DINT = physical value x selected LMC_Units constant
display value = received DINT / the same LMC_Units constant
```

The caller must select the unit for each axis and parameter, round the result,
check the signed DINT range, and pass an `int` to the API. The DLL serializes
that `int` without rescaling. The PLC passes the received DINT to the matching
LASAL motion block without a second conversion.

This keeps the conversion responsibility explicit:

- application/user code chooses the unit constants and conversion rule
- API code builds packets from already-converted DINT values
- PLC code receives DINT values and passes them to LASAL motion blocks

The DLL must not apply the legacy PMAS `8,388,608 count/rev` conversion. This
decision supersedes earlier proposals that placed forward/reverse conversion
inside the DLL.

For the current `Elmo_EtherCAT_Test_4Axis` project, a01-a04 are configured with
the `deg` macro for `IntUnits`, `VMax`, `AMax`, and `JMax`. Therefore current
single-axis position/speed/accel/decel examples use `LMC_Units.DEG`; `RPM` is
not a substitute for `_LMCAxis` speed in application units per second. Nonzero
jerk conversion and the v01 kinematic-axis profile remain explicit approval
items and must not be guessed by the DLL.

The distribution rule, unit table, overflow handling, and caller examples are
defined in `UNIT_CONVERSION_MANUAL_2026-07-10.md`.

## Naming Policy

Public methods should expose one method per actual operation. Do not keep a
second `LMC_*Cmd` or `LMC_*` alias when it only sends the same packet as the
primary method.

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

`LMCAxis` and `LMCGroup` class names may remain as short class aliases for
`LMCSingleAxis` and `LMCGroupAxis`, but method-level duplicates should be
removed. Public user-facing methods must not call other public user-facing
methods.

Internal packet builders in `LmcProtocol.cs` should name the LASAL-side target
explicitly. Use names such as `LMCAxisGetByName`, `LMCAxisPower`,
`LMCAxisMoveAbsolute`, `LMCGroupGetByName`, `LMCGroupEnable`, and
`LMCGroupMoveLinearAbsolute`. Low-level private helpers may stay generic, but
command-facing builders must not use ambiguous names such as `Power`, `Simple`,
`Velocity`, or `MoveLinear`.

## Consequence

If a caller wants to command `1.0 mm`, the caller must convert it before calling
the API, for example:

```csharp
var position = checked((int)Math.Round(1.0 * LMC_Units.MM));
axis.MoveAbsoluteEx(position, velocity, acceleration, deceleration, jerk);
```

The packet builder receives `position` as an `int` and writes that value
directly to the DINT payload.
