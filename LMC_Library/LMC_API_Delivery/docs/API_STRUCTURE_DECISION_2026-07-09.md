# API Structure Decision 2026-07-09

## Scope

This decision applies to `LMC_Library/LMC_API_Delivery`.

`Codex_LASAL_WPF` is a dummy/test implementation and is not the source of truth
for the delivered DLL.

## Packet Model

The delivered DLL keeps the PMAS/MMCLib object model:

1. A connection object owns the TCP session.
2. An axis object is created with an axis name and connection.
3. The axis object resolves the name once with `GetAxisByName`.
4. The axis object stores the returned axis reference.
5. Motion/status methods use the stored reference when building packets.

The caller should not pass `axisName` or `axisRef` to every motion method.
The axis object already owns that state.

## Unit Policy

Unit conversion helpers are public user utilities only.

The API implementation must not call the unit conversion helper internally.
Motion methods accept values that are already in the LASAL/internal DINT unit
expected by the PLC parser.

This keeps the conversion responsibility explicit:

- application/user code chooses the unit conversion rule
- API code builds packets from already-converted DINT values
- PLC code receives DINT values and passes them to LASAL motion blocks

## Naming Policy

Primary public methods should follow the PMAS/MMCLib wrapper style where
practical:

- `MMCSingleAxis`
- `MMCGroupAxis`
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

`Units` and `UnitConverter` are the primary user-facing unit helper names.
Legacy `LMC_Units` and `LMC_UnitConverter` may remain as compatibility aliases.

## Consequence

If a caller wants to command `1.0 mm`, the caller must convert it before calling
the API, for example:

```csharp
var units = new UnitConverter();
var position = units.PositionToInternal(1.0);
axis.MoveAbsoluteEx(position, velocity, acceleration, deceleration, jerk);
```

The packet builder receives `position` as an `int` and writes that value
directly to the DINT payload.
