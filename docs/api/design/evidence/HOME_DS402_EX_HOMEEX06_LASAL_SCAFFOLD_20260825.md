# HomeDS402Ex HOMEEX-06 LASAL Scaffold Evidence

Date: 2026-08-25
Scope: source/static qualification only
Production decision: **NO-GO / capability OFF**

## 1. Qualified tranche

HOMEEX-06 is the gate-OFF LASAL parser/state/outcome scaffold. This tranche intentionally stops before ownership admission, SDO programming, RT control, controlword mutation, operation-mode mutation, setpoint alignment, homing execution, capability activation, C78 qualification, PLC load, packet proof or hardware motion proof.

Implemented source boundary:

- `TCPMotionInterface.st` routes `0x7D1B/0x7D1C/0x7D1D` through the diagnostics lifecycle path.
- `LMCDiagnosticsService.st` owns a dedicated `Ds402HomeExState[0..255]` scaffold store.
- Start, ReadOutcome and Retire have dedicated strict request parsers.
- Start requires the frozen 116-byte shape, physical axis 1..4, schema/flags/request identity, Build/BootId/MapRevision, nonzero 128-bit intent, Aborting mode, zero reserved/spare bytes, nonzero timeouts and execute token `0x58453448`.
- Query requires the frozen 116-byte full recovery key shape.
- Retire requires the frozen 120-byte full recovery key plus nonzero expected generation.
- empty scaffold outcome state returns HomeDS402Ex outcome-not-found; unexpected nonzero scaffold state fails as store-corrupt.
- `ProcessAxisDs402HomeEx` is a no-op.
- `LMC_DIAG_DS402_HOME_EX_ENABLED` remains `FALSE`.
- Admin feature mask remains `0x00000017`; bit 11 remains OFF.

No HomeDS402Ex runtime/outcome record is written in HOMEEX-06. No HomeDS402Ex handler calls AxisOwnership, an SDO executor, an RT input latch, a controlword path, an operation-mode mutation path or a motion/setpoint mutation path.

## 2. Ownership boundary moved to HOMEEX-07

The frozen ABI remains:

- OwnerKind 7
- ResourceKind 3 (`DS402_HOME_ENGINE`)
- AdmissionMode 4
- exact physical axis mask `1 << (Reference - 1)`
- active owner state 13 reserved for the later runtime tranche.

However, HOMEEX-06 does **not** add OwnerKind 7 to source. The current non-group ownership identity bank preserves a 64-byte prefix plus at most an 8-byte tail, which is not sufficient to retain the full 116-byte HomeDS402Ex Start identity. Adding OwnerKind 7 before expanding that identity storage would weaken exact lifecycle identity.

Therefore ownership reservation/validation/commit and the full owner identity extension are explicitly deferred to HOMEEX-07. HOMEEX-06 rejects any unexpected admission token/generation and stays non-executable.

## 3. Exact source application evidence

The large generated LASAL sources were not rewritten through the GitHub Contents API. An exact fail-closed transform was used only to create the tracked source commit:

- application workflow run: `32808873536`
- application job: `97684258450`
- result: **SUCCESS**
- generated tracked source commit: `40df37f4acb7c44db75439b4370fed5c8c3cf8c9`

The application gate required:

- exact pre-scaffold Git blob identities;
- exact-once source anchors;
- 7-bit ASCII output;
- canonical LF output;
- exactly the two intended LASAL tracked source files changed;
- HomeDS402Ex scaffold verifier PASS;
- SetOperationMode define-order regression PASS;
- `git diff --check` PASS before the source commit was created.

The one-shot transform artifacts were removed from the final PR after source creation. Their Actions run remains the source-application audit trail.

## 4. HOMEEX-06 final source/static qualification

Qualified PR source head before documentation-only synchronization:

- branch: `codex/home-ds402ex-lasal-scaffold`
- head: `30892f223deae6165ff9565afaa48138f04c8fd8`
- PR: `#24`

HomeDS402Ex workflow:

- run: `32809237405`
- job: `97685273091`
- result: **SUCCESS**
- HomeDS402Ex verifier: **67 checks PASS**
- state: **SCAFFOLD_OFF**
- SetOperationMode define-order regression: PASS
- diff hygiene: PASS

The 67 checks include route presence, route isolation from ownership admission, dedicated state, gate OFF, frozen detail constants, exact handler declarations/definitions, exact request sizes, spare validation, execute-token validation, Aborting-only handling, `Int32.MinValue` final-position rejection, deterministic dormant Start rejection, exact Query/Retire shapes, nonzero Retire generation, no ownership/SDO/RT/motion mutation sites, no outcome writes, no-op processor, generic D5 `0x6060` deny preservation, capability bit 11 OFF and 7-bit ASCII.

## 5. Cross-feature regression evidence

SetOperationMode C78 evidence tool on the same source head:

- run: `32809237421`
- result: **SUCCESS**
- define-order PASS
- tracked artifact identity reporting PASS
- collector self-test PASS
- diff hygiene PASS

SetOperationMode static qualification on the same source head:

- run: `32809237415`
- job: `97685273078`
- SetOperationMode static verifier: **57 checks PASS**
- SetOperationMode define-order: PASS
- diff hygiene: PASS
- full repository SourceOnly: STOP only at the pre-existing `SetPosition-augmented Classes.lcb physical identity drifted` ratchet.

That artifact ratchet STOP is not a HomeDS402Ex source regression and is not reclassified as PASS here.

## 6. Gate decision

HOMEEX-06 source/static scaffold: **PASS**.

Still not proven and therefore still closed:

- OwnerKind 7 / ResourceKind 3 source integration: HOMEEX-07
- full 116-byte ownership identity storage: HOMEEX-07
- parameter snapshot/program/restore and cleanup proof: HOMEEX-08
- fresh C78/ARM build and generated artifact qualification: HOMEEX-09
- PLC load/runtime: NOT RUN
- EtherCAT/TCP packet causal evidence: NOT RUN
- physical homing motion: NOT RUN
- Admin feature bit 11: OFF
- WPF Start UI: CLOSED
- production activation: **NO-GO**

HOMEEX-06 completion must not be used as evidence for HOMEEX-07 through HOMEEX-13.
