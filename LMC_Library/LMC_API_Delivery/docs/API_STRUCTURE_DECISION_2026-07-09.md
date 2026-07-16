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

## Target Resolution And Dispatcher

PLC object names belong to the LASAL application, not to the PC API library.
The DLL accepts the caller-provided target name and serializes it; it must not
contain a table such as `_LMCAxis1 -> axis 1`.

The canonical LASAL implementation uses this flow:

1. `TCPMotionInterface` receives `_LMCAxis1..9` through typed client channels.
2. During LASAL runtime initialization it reads each connected object's actual
   name with `_GetObjName` and builds an immutable registry.
3. `0x103C`/`0x1042` search that registry and return an opaque `UINT16`
   descriptor.
4. The PC axis/group object stores only that descriptor.
5. Later commands carry the descriptor; LASAL validates it and dispatches to
   the registered client channel.

The descriptor is not a PLC pointer and must never expose `pCmd` or another
runtime address. Value `0` is invalid, current axis descriptors are `1..9`, and
the current group descriptor is `0x0100`. These values are deployment-local and
may change after a project rebuild; callers must perform lookup again after each
new RPC connection.

This design is implemented in the tracked canonical project. LASAL IDE class
model regeneration, target build and PLC verification are still required.
Detailed rules and failure behavior are in
`LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md`.

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

The DLL must not apply `8,388,608 count/rev` or any other conversion. The WPF
example selects the PLC application UNIT in caller code and also provides an
explicit raw-DINT mode. Encoder counts remain part of the PLC/drive transmission
ratio and are not a PC API UNIT.

In the currently saved `Elmo_EtherCAT_Test_4Axis` project, `_LMCAxis1..9` use
the `mm` macro and `_JERK_PROFILE`; the saved values are `IntUnits=1 mm/rev`,
`VMax=75 mm`, `AMax=7500 mm`, and `JMax=75000 mm`. Current single-axis examples
therefore use `LMC_Units.MM`. `RPM` is not a substitute for `_LMCAxis` speed in
application units per second. The caller converts physical jerk with
`(physical jerk / 1000) * axis UNIT`, because `_LMCAxis` declares Jerk in
`Application units / sec^3 / 1000`. The DLL still performs no conversion. The
downloaded PLC profile/JMax and the `_LMCRobotBase1` kinematic-axis profile must
be verified separately.

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

Current direction contract is deliberately narrow: absolute/relative moves
accept `Shortest` only (relative sign comes from distance), while velocity
moves accept `Positive` or `Negative`, normalize the velocity sign, and require
deceleration `0` because LASAL `MoveEndless` has no deceleration input. Unsupported
combinations are rejected instead of being transmitted and ignored.
