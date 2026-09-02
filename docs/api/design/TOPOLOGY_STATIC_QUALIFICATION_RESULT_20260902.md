# TOPO-C0 Static Qualification Result — 2026-09-02

- source branch: `dev`
- inspected source HEAD: `b4e43a56f0980d329fbb8e0a80ce924ebc0a0855`
- scope: current source, editable Motion Network XML, generated Motion Network table, and dormant activation contracts
- result: **STATIC TRANCHE PASS / TOPO-C0 OVERALL OPEN**
- production posture: **NO-GO**

## Implemented verifier

`tools/Verify-CurrentPhysicalTopology.ps1` was added and is also invoked first by
`tools/Verify-HomeDs402H37CurrentDevRegression.ps1`.

The verifier fails closed on the following drift:

- the three physical-drive masks are not exactly `0x00000003`
- Axis1/2 are not physical or Axis3..9 are not simulation in the current network defaults
- `SimulationSetup` retained servers, first-scan forwarding, write forwarding, or 1:1 client routing changes
- editable `Motion_Network.lcn` and generated `ONE_Motion_Network_Table.st` no longer agree with the nine-axis mapping
- InputLatch, Ownership, or Diagnostics no longer mask-gates the four legacy physical slots
- Encoder Maintenance loses the configured-physical-mask fail-fast path or detail code 44
- HomeDS402 five-value activation is mixed rather than atomic all-OFF/all-ON
- SetPosition Store/max-jump/capability fail-closed controls are enabled

## Local result

Executed from the repository root:

```powershell
.\tools\Verify-CurrentPhysicalTopology.ps1
.\tools\Verify-HomeDs402H37CurrentDevRegression.ps1
.\tools\Verify-SetPositionCurrentSourceInventory.ps1
```

Observed results:

- TOPO-C0 static verifier: **154 checks PASS**
- H37 activation contract: **46 checks PASS**
- H37 ownership contract: **21 checks PASS**
- H37 method-size contract: **10 checks PASS**
- H37 WPF durable no-replay contract: **36 checks PASS**
- H37 current-dev top-level contract: **18 checks PASS**, including the TOPO-C0 nested verifier invocation
- SetPosition SP-C0 current source inventory: **39 checks PASS**

## Verified current source facts

- logical axes: Axis1..Axis9
- configured physical drives: Axis1/2
- simulation defaults: Axis3..9
- runtime physical mask: `0x00000003` in InputLatch, Ownership, and Diagnostics
- `SimulationSetup1.Simul_Axis_N -> _LMCAxisN.SimulateMode`: exact 1:1 mapping for N=1..9
- HomeDS402 five-value activation in tracked source: atomic ON
- HomeDS402 Admin capability: `0x00000757` bit 6 ON
- Diagnostics operational capability: `0x0000613F` unchanged; its bit 6 is RecorderDoubleBank, not HomeDS402
- SetPosition activation: OFF
- original mutation replay policy: unchanged; recovery remains read/retire only

## Remaining TOPO-C0 evidence

This result does not close TOPO-C0. The following evidence still requires the LASAL IDE and/or the target PLC:

- fresh LASAL C78/ARM Compile/Rebuild/Link with 0 errors
- `SimulationSetup` class/method direct-open and Motion Network smoke
- generated `Classes.lcb` and `Networks.lcb` identity from that exact build; no blind ratchet
- cold/restart boot proof for Axis1/2 `SimulateMode=0` and Axis3..9 `SimulateMode=1`
- startup ownership ready proof with absent Drive3/4
- Encoder Maintenance Axis1/2 positive and Axis3/4 deterministic unavailable runtime proof
- boot ordering proof between `SimulationSetup::Init` and ownership/command admission

Therefore this is source/static evidence only. It is not LASAL compile, PLC download, PLC runtime, packet, hardware, or production PASS.

The currently running PLC image and WPF process predate the HomeDS402 activation
source changes. They remain old runtime evidence until a fresh LASAL build/link/download
and WPF restart are performed.

## Next gate

Build/link/download this exact tree, capture the fresh C78/generated-artifact identity,
then perform the Axis1 Method 37 one-shot qualification. SetPosition SP-C1 remains
blocked until vendor `CheckSum.CRC32` golden fixtures and LASAL IDE-generated `_FileSys`
ABI evidence are available.
