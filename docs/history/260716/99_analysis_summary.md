# Elmo Master History 260716 Analysis Summary

## Quick resume

The history's final request was to organize the completed work and push it.
That request is complete in the live repository.

- Branch: `main`
- Live HEAD: `f8f99a299f72c118c9a243d0165368d666d0cd0f`
- `origin/main`: same commit
- Latest commit: `f8f99a2 build(distribution): add standalone API package and manual`
- Existing implementation working tree: clean
- Current untracked scope: this 260716 source history and split/handoff artifacts

There is no unfinished final Git request to resume. The next engineering task is
the LASAL IDE/PLC validation gate for the current 9-axis and group source. Start
with a current-source Rebuild/Link and Find-in-Implementation smoke. Do not begin
motion testing until the network UNIT, reference, software limits, emergency
behavior, and downloaded PLC image are confirmed.

## Analysis scope and integrity

- Source: `docs/history/Elmo_Master_history_260716.md`
- Source size: 2,060,307 bytes, 16,760 lines
- Split copies: 68 chunks under `docs/history/260716/`
- Source SHA-256:
  `a88a537aa9f6a10b31995a81afb5cbaef6bbe25b152e6d731b90fcd7a1ade3c1`
- The original remained unchanged after splitting.
- Source line 1,913 contains a 1,048,602-character JPEG/tool-state payload. Only
  that line is replaced in split copies by a hash-bearing placeholder.
- Trailing tabs on source lines 5,880, 5,884, 8,295, 8,299, 8,891, and 8,895
  are trimmed in split copies and recorded in the manifest.
- Sanitized chunk rejoin SHA-256 matches the sanitized reference:
  `274ca885076e361aa4ae21fe48c66b93f5023ce2b76eeb904c7aaaa0ed606956`.

All 68 chunks were read. History statements below are separated from facts
rechecked against the live repository on 2026-07-16.

## What the history actually accomplished

### 1. PC API contract and lifecycle

The work started from an older snapshot in which the C# request paths and live
LASAL parser did not match and `0x2051`/`0x20E7` were missing. That snapshot is
no longer current.

The PC library was brought to the captured 23-command scope plus two LASAL-local
group power extensions. The major decisions are:

- The caller converts physical values to the PLC application UNIT and passes
  `DINT`; the DLL does not rescale them.
- Connection lifecycle is TCP connect, `0x8080` init, UDP listener,
  `0x405C` callback registration, commands, and `0x405D` close.
- Request and response shapes are validated per command rather than treated as
  one generic ACK.
- Timeout, cancellation, reconnect generation, stale handles, callback source,
  and defensive response-byte copying are handled explicitly.
- `0x2051 GroupReadActualPosition` uses the LASAL 68-byte DINT response, not the
  PMAS 136-byte LREAL response.
- `0x20E7 SetKinTransformCartesian4Axis` is an exact captured-profile static
  identity configuration, not a generic dynamic kinematics API.

### 2. Canonical LASAL project and execution model

The canonical development target is the Git-tracked
`Lasal_PRG/Elmo_EtherCAT_Test_4Axis`. The untracked `_Edit` copy was abandoned.

The early depth-8 queue/RtWork/mailbox/atomic design was superseded by a later
user decision. The current design is:

- ordinary `_TCPIPServer1`
- `Config=0`, `MaxConnections=1`
- TCP framing/queue and command execution in CyWork
- no TCPMotionInterface RT task, RT mailbox, or atomic handoff

The history also established ASCII-only custom LASAL source/IDE names, strict
separation of CodeGenerator and user implementation regions, and a required
Find-in-Implementation smoke plus new-log `CInvalidArgException` check after IDE
save/build.

### 3. LASAL implementation and hardware-debug findings

The source evolved from four-axis lookup and five blocked group commands to the
current published scope. Important fixes included:

- case-insensitive lookup because LASAL runtime object names were uppercased
- `TO_DINT(...)` conversion instead of `$DINT` memory overlay for command ID,
  reference, and payload length
- explicit Jerk inputs through PC and PLC paths
- group PowerOn/PowerOff mapped to asynchronous RobotOn/RobotOff requests
- group Enable/Disable mapped to LockProfile/UnlockProfile
- GroupReset, GroupStop, MoveLinear, GroupReadActualPosition, and SetKin source
  paths added
- `_LMCAxis1..9` single-axis dispatch added; Cartesian group motion remains the
  X/Y/Z/U four-axis scope
- X/Y/Z/U Home Check added before Set Identity

The equipment investigation also exposed unresolved physical/configuration
risks: axis reference/absolute-encoder retain state, an Axis2 `BinOffset`
episode, software-end-position error 7, and a 128 mm/position-overflow episode.
These were diagnosed in history but not closed by a final full hardware
validation record.

### 4. WPF example and distribution

The dummy PMAS-style WPF copy was replaced with a real
`LasalApiWpfTestApp`. Misleading unimplemented tabs were removed, later published
group functions were added, UNIT profiles and Raw DINT were exposed, Jerk was
enabled, and repetitive arm/confirmation dialogs were removed at the user's
request.

The final external package is `LMC_Library/LMC_API_Distribution` and contains:

1. `01_API` with `LasalMotionControlLib.dll`
2. `02_Example_Program` with independent source and prebuilt runtime
3. `03_API_User_Manual` with canonical DOCX and PDF

The final version is `0.9.1-preview`, not a production release.

### 5. Final commits and push

The last three commits recorded in both history and the live repository are:

- `62fcd8d` - harden response data and bump preview version
- `8dc04e0` - align nine-axis UNIT and release guidance
- `f8f99a2` - add standalone API package and manual

They are pushed to `origin/main`.

## Superseded history snapshots

Do not resume from an intermediate conclusion without checking the later parts.

| Older history statement | Current live baseline |
|---|---|
| `0x2051` and `0x20E7` are unimplemented | PC and LASAL source paths and static contracts exist; PLC E2E is still absent |
| Five group commands intentionally return `-5` | Later source activates the published group paths; current static contract passes |
| Only four single axes are dispatched | Single-axis lookup/control is implemented for `_LMCAxis1..9`; group Cartesian motion remains four-axis |
| TCPMotionInterface needs a separate RT task/mailbox | Superseded by CyWork-only queue and ordinary TCP server |
| `0.9.0-pc-api` package is current | Current external package is `0.9.1-preview` in `LMC_API_Distribution` |
| The generated image-only PDF is final | Final Git publication promoted the later user-edited Word-export DOCX/PDF to the canonical names |

## Live repository verification on 2026-07-16

### Git

- `HEAD` and `origin/main` both resolve to
  `f8f99a299f72c118c9a243d0165368d666d0cd0f`.
- No implementation or documentation diff predates this handoff task.
- Only the provided history and the new `docs/history/260716/` artifacts are
  untracked.

### Automated checks rerun in this analysis

Using Visual Studio 2019 MSBuild against the live HEAD:

- PC request/parser/RPC suite: `46/46 PASS`
- LASAL source-only static contract: PASS
- LASAL full-network static contract: PASS
- Development WPF example Debug build: PASS
- Distribution WPF example Debug build: PASS

These checks validate source and static/network structure. They do not compile
the current project in LASAL IDE, download a PLC image, command a physical axis,
or validate actual packets.

### Live source facts

- `TCPMotionInterface.st` declares and dispatches `LMCAxis1..9`.
- The group path still requires the first four clients for Cartesian X/Y/Z/U.
- Jerk is parsed and forwarded for axis and group motion paths.
- The live Motion Network declares all nine axes with
  `ExUnits=8388608`, symbolic `IntUnits=1 mm`, and software limits
  `-10000 mm..10000 mm`.
- The current distribution README identifies `0.9.1-preview` and the three-part
  package above.

The symbolic network values are source facts, not proof that the downloaded PLC
or real mechanism uses the same encoder ratio and limits.

## Remaining gates and risks

### P0: current source must pass the LASAL/PLC gate

1. Open the tracked canonical project and Reload external source changes.
2. Rebuild/Link the current 9-axis/group source in LASAL IDE.
3. Run Find in Implementation on changed TCPMotionInterface members.
4. Check only `%TEMP%\Lasal2.log` entries created after the smoke start for a
   new `CInvalidArgException`.
5. Confirm that IDE save did not regenerate or discard the current clients,
   queue sizes, handlers, and network connections.
6. Confirm TCPMotionInterface CyWork and motion task core/priority/jitter on the
   target controller.
7. Download the exact built image and record its source commit/build identity.

### P0: hardware configuration before motion

Confirm on the real controller and mechanism:

- all nine axis connections and only the intended four group members
- application UNIT versus encoder `ExUnits/IntUnits`
- absolute encoder retain/reference state for each axis
- Axis2 and other axes' `BinOffset`/position consistency
- `SWMinPos`, `SWMaxPos`, modulo behavior, and the earlier 128 mm boundary
- hardware limits, E-stop, emergency deceleration, and safe Stop behavior

Do not infer those values from the WPF UNIT combo or Git network file alone.

### P1: packet/E2E validation

Use read-only operations first and capture every step:

1. session init/register/close
2. axis/group lookup and AxisInfo
3. ReadStatus and ReadPosition for axes 1-9
4. group members, group status, Home Check, and group position
5. controlled Power/Reset/Stop on one verified axis
6. short low-speed relative move inside independently confirmed limits
7. four-axis RobotOn -> PowerReady -> Set Identity -> LockProfile -> Move ->
   UnlockProfile -> RobotOff sequence
8. success and failure ACKs plus Wireshark re-capture for the full 23+2 scope

The current formal device result remains `0/25` verified commands until those
records exist.

### Deliberately absent or incomplete scope

- typed callback schema and actual LASAL callback event sender
- multi-PC session/motion-owner arbitration
- Home execution API; only reference/Home checks are available
- MoveCircle and generic/dynamic kinematics
- nine-axis coordinated interpolation; nine axes are independent single-axis
  control while the Cartesian group remains four-axis
- strong-name/AuthentiCode signing

`CloseConnection`, `Dispose`, and cancellation are communication cleanup, not a
machine safe-stop guarantee.

## Documentation cleanup found during live comparison

The latest external distribution intentionally exposes only the three delivery
areas. However, some internal pre-distribution documents still describe an
external `Lib` folder, API-reference bundle, `RELEASE_MANIFEST.md`, or older
four-axis descriptor/`0.9.0-pc-api` snapshots. The final build script explicitly
rejects internal manifest/metadata names from the external folder and only
prints the hashes during verification.

Before the next public package revision, reconcile at least:

- `LMC_Library/LMC_API/API_SOURCE_REVIEW_2026-07-15.md`
- `LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md`

Do not treat those stale packaging sentences as evidence that current source or
PLC verification regressed; the live code, final distribution, and later commit
history take precedence.

## Recommended immediate next action

The next action is a validation session, not another speculative API expansion:

1. LASAL IDE Reload, Rebuild/Link, and implementation-search/log smoke.
2. Export the IDE build result and current network/task settings into a dated
   validation record.
3. Confirm UNIT/reference/limits/emergency settings on the physical machine.
4. Run read-only 9-axis and four-axis group smoke with packet capture.
5. Only then arm one short relative move and continue through the group sequence.

Use these live source-of-truth documents while continuing:

- `LMC_Library/README.md`
- `LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md`
- `LMC_Library/LMC_API/API_SOURCE_REVIEW_2026-07-15.md`
- `LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md`
- `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`
- `docs/architecture/SIGMATEK_LASAL_coding_rules.md`
- `docs/architecture/SIGMATEK_LASAL_programming_method_study.md`
- `docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md`
