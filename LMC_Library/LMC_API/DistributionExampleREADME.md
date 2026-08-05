# LasalApiWpfTestApp

This .NET Framework 4.8 WPF example uses the public API from the packaged DLL.
It has no reference to the internal API source project.
This example remains preview software and is not production approved.

## Build and run

Open `LasalApiWpfTestApp.sln` in Visual Studio 2019 or later and build x64
Debug or Release. The project reference is fixed to:

```text
..\..\01_API\LasalMotionControlLib.dll
```

`Run/LasalMotionControlApiExample.exe` is the Release build created for this
candidate. Keep the DLL beside the executable.

## Before sending commands

1. Verify the PLC IP, TCP port, PC local IPv4, and callback UDP port.
2. Connect and load the required axis or group by its LASAL object name.
3. Read status and position before Power or Motion commands.
4. Confirm E-stop, hardware/software limits, UNIT, reference state, and the
   permitted motion envelope on the actual machine.
5. Treat an ACK as acceptance, then poll typed status to a stable final state
   and read back the final position.
6. Use explicit Stop and Power Off procedures. Close, timeout, and cancellation
   do not stop the machine.

Only one example process may own its local recovery journals. If the stored
BootId or MapRevision does not match the connected PLC, the app enters a
read-only recovery quarantine. Use `Archive and Retire Stale Recovery` only
after confirming the mismatch; retirement does not claim that an old command
completed on the current PLC.

## Manual Axis 1 SDO Write

The only approved target is `Slave 1 / 0x2F00:24 / Int32 / 4 bytes` (Gold
UI[24]). Manual Write remains disabled until the same current connection,
session, DiagnosticsBuild, BootId, MapRevision, and target pass the distinct
baseline, pre-write guard, Write, and guarded-readback ticket sequence. The
second confirmation performs a fresh identity check inside the SDK mutation
gate before command `0x7E50` can be sent. Any mismatch or disconnect
permanently revokes the proof. Axis 2..4 and all other targets remain blocked.

PI Write, D4 Double Recorder, dynamic bits 15..17, and digital output command
`0x7E23` are not enabled in this preview candidate.
