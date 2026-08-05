# 260805 history chunk digest: parts 235-350 and histories 2-4

## Scope and coverage

- Reviewed every selected file individually: `Elmo_Master_history_260805_1` parts `235..350` plus the sole split file for histories `2`, `3`, and `4`.
- Coverage: **119/119 readable files** (`116 + 3`), no missing part and no duplicate selection.
- Main-history logical source range: lines `58501..87280`.
- The line ranges below are the logical ranges encoded in each split filename, not line numbers in this digest.
- This is a continuation aid, not current runtime proof. Later chunks override earlier conversation checkpoints, but even the last chunk must be rechecked against live source, generated metadata, build output, downloaded PLC identity, and hardware logs before action.

## Latest continuation point

The operative end state is the final result in part 350, not the earlier successful C78 build or the older hardware logs.

1. `ReserveAxisOwnership` had five uses of undeclared `preemptRecordBase` corrected to the already-declared `probeRecordBase`. The final recorded Control SHA-256 is `C976CD364010EEFDFDDA8D7BC6D7655293DAD221FBEC908D50E5805CE4AFF072`.
2. Final static evidence recorded in the history is:
   - Reserve focused verifier `62/62` attack mutations rejected, comment-only mutation allowed.
   - Ownership aggregate `271/271`, Rollback `38/38`, Publish `69/69` PASS.
   - Five-pre-IDE-waiver full `SourceOnly` PASS.
   - Method-size inventory: 6 custom classes, 93 methods, 7 existing oversized debts, ratchet self-test `5/5` PASS.
   - Earlier focused evidence still relevant to this tranche includes diagnostics split `23/23`, TW19 barrier `37/37`, encoder `56/56`, and DS402 retirement `50/50`, but these are static checks.
3. **Blocking next step is LASAL IDE handoff Section 17.** Generated metadata contains all nine required names zero times: one hidden retained channel and eight private functions have not been declared in the IDE.
4. In one IDE visit, add exactly:
   - `TCPMotionInterface`: `HandleControlSafetyDrainPending`, `HandleRpcLifecycleCommands`.
   - `LMCControlCommandService`: hidden retained `AxisRebaseRequiredState : SvrCh_UDINT` and private `HandleAxisOwnershipSafetyRepeat`, `ReadAxisRebaseRequiredMask`, `UpdateAxisRebaseRequiredState`.
   - `LMCDiagnosticsService`: `HandleEncoderMaintenancePreemption`, `HandleAxisDs402HomeReceiptStages`, `HandleAxisDs402HomeCleanupStages`.
5. All eight functions remain private: do not make them `GLOBAL`/`VIRTUAL GLOBAL`. `AxisRebaseRequiredState` must remain hidden, file-retentive, non-visualized, and unconnected. Do not change any Network.
6. After adding them: **Save All, do not Rebuild, exit LASAL IDE, then report completion.** The next agent action is external inspection of the three generated declarations, `Classes.lcb`, channel properties, Network non-change, source hashes, and LF/CRLF method sizes.
7. Only after that inspection should the five temporary waivers be removed and default `Verify-LasalContract.ps1 -SourceOnly -ExpectedSdoWriteAxis 1` be required to PASS. Then request a C78 Rebuild, followed by `Find in Implementation` and a smoke-start-relative `%TEMP%\Lasal2.log` check for new `CInvalidArgException=0`.
8. Do not download or run another axis test before those gates. The current post-part-338/part-332 source has not been proven by a matching current C78 build, cold download, BootId, or hardware run.

## Explicit user requests and operating decisions recovered from the history

- Keep developing while waiting for IDE work, but separate source/static proof from C78, download, and real-axis proof.
- Update design and IDE-handoff documents as implementation decisions settle; freeze user/API/deployment manuals, README, and HTML until C78 and hardware behavior are stable. Preserve prior manual edits rather than churn them during implementation.
- Latest explicit IDE-control time instruction in the captured conversation is weekdays `17:30` to next day `08:00`, with Saturdays, Sundays, and Republic of Korea public holidays allowed all day. Several later summaries still say `08:30`; treat those as stale until the user reconfirms.
- IDE control permission does not authorize PLC download, real-axis motion, feature-gate activation, or other hardware mutation; those remained separately gated.
- User reports that drove the implementation sequence:
  - “TW19/TW20 still unavailable” and “topology load errors”.
  - Later, TW19 operated, but Home did not.
  - After a restart, Axis1 Home worked while later axes did not; this exposed stale completed-Home receipt/ownership blocking.
  - User completed the earlier Section 15 declaration visit; Section 17 is a later, still-unfinished IDE visit.
  - User requested that manuals no longer be updated continuously during unstable implementation.
- History 2 explicitly asked for a safe, evidence-based test order. Its durable rule remains: do not interpret ACK as completion; preserve capture/log/BootId identity and stop when DS402 Warning or provenance is unresolved.
- History 3 asked whether ActualPosition is the PLC value and whether 1 mm is 10000. The answer was: it is PLC MotionLib application-unit DINT, not encoder count; current source uses `10000 DINT = 1 mm`, subject to confirmation against the downloaded PLC.

## Last assistant actions before the captured history ended

- Corrected the Reserve source identifier defect and updated its verifier/baseline.
- Strengthened structural verification against function relocation, wrapper injection, malformed `END_FUNCTION`, cross-function block leakage, pre-commit writes, invalid replay mutation, and missing-axis loops.
- Re-ran Reserve, ownership aggregate, Rollback, Publish, size-ratchet, and five-waiver SourceOnly checks.
- Updated design/IDE-handoff evidence only; did not further update manuals/README/HTML.
- Rechecked Section 17 generated names and found all nine still absent.
- Stopped at the correct boundary: requested Section 17 Save All without Rebuild and explicitly did not claim current C78/download/runtime completion.

## Unresolved work and proof gates

| Gate | Current history-backed status | Required evidence / restriction |
|---|---|---|
| Section 17 generated ABI | **BLOCKED / absent** | One hidden retained channel + eight private declarations, exact ABI/order/properties; no Network changes; Save All then exit without Rebuild. |
| Waiver-free static contract | **Unverified** | External generated-file inspection, remove five pre-IDE waivers, default SourceOnly PASS. The waiver-removal probe currently fails as intended first at missing `HandleEncoderMaintenancePreemption`. |
| Current C78 build | **Unverified for latest source** | Rebuild only after waiver-free gate; inspect fresh compiler/linker result. Earlier `0 errors, 50 warnings` predates substantial later source changes. |
| Current IDE smoke | **Unverified for latest source** | Changed-class `Find in Implementation`; no new smoke-relative `CInvalidArgException`. Earlier four Network finds and large-class front/middle/end finds belong to the earlier build checkpoint. |
| Current PLC deployment | **Not done** | Cold download/restart only after current build/smoke; freeze new nonzero BootId, capabilities, map revision, and build identity. |
| LMC Home | **Hardware FAIL/unverified after fixes** | Earlier Axis2 outcome was `Quarantined`, `OriginalErrorId=-31000`, detail `38`, with position delta `+1`; later receipt, cancellation, tolerance, and ownership fixes have no matching hardware PASS. Prove one exact terminal success, owner release, then next-axis admission without detail `41`. |
| DS402 Home | **Gate/runtime blocked** | Latest handoff states `LMC_DIAG_DS402_HOME_ENABLED=FALSE`; prove method-37, persistent receipt/WAL, drain, cleanup, and warm-restart behavior only after current build/download. |
| Ordinary ownership | **Gate/runtime blocked** | Latest handoff states `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED=FALSE`; static identity/preemption/restore tests are not activation proof. |
| TW19 | **Past runtime evidence only** | User reported TW19 operated on an earlier PLC. Current source later added a retained “TW19 requires successful LMC Home before motion” barrier; requalify on the matching current BootId. |
| TW20 | **No exact current physical proof** | Capability and protocol static PASS are not a completed SDO/physical-effect proof. Verify motor-off/standstill, exact write, drain, stable cleanup, and distinguish command success from physical effect. |
| Topology | **Past recovery, current identity unproven** | Earlier `|` versus integer `OR` fix restored capability/topology on an older BootId; verify again after current cold download. |
| DS402 health | **Historical warning remains a qualification concern** | History 2 recorded `0x6041=0x02B3` Warning=1 and `0x603F`/AxisError gates. Re-read all on the exact current PLC; `0x2028 StatusWord` is not DS402 `0x6041`. |
| In-motion Stop / Group / CREVIS live I/O | **Still unverified in supplied histories** | Same-BootId single-axis baseline first; true non-Standstill Stop; then Group lifecycle/motion/Stop; real Fault-only Reset; CREVIS live DI/DO separately. Do not jump to these while current ABI/build/runtime gates are open. |
| Persistent rebase state | **Static design only** | Prove `AxisRebaseRequiredState` encoded word, restart/power-loss retention, no-owner bit-4 automatic drain, and atomic activation gates on target. |
| Production caller debt | **Open** | `PublishAxisOwnership` Result is ignored by 11 production callers; fail-closed consumption and general multi-axis cold-restart publication recovery remain designed/open work. |

## Contradictions and historical-versus-current caveats

- History 4 is an older snapshot. Its conclusions that LMC Home was empty, TW20 gate was false, and TW19 was fully unimplemented were later superseded by substantial implementation and a user-reported TW19 operation. Do not reuse those capability conclusions as current facts.
- History 2 also predates later IDE/source work. Its immediate “IDE ABI typed client and two Network links are missing” blocker was addressed during the long main history, but its safety ordering and unresolved same-BootId/Warning/in-motion-Stop/Group gates remain useful.
- Part 338’s `0 errors, 50 warnings`, four Network searches, and `CInvalidArgException=0` prove an earlier source checkpoint only. Parts 339-350 subsequently changed Control/Diagnostics/TCP source and verifiers and introduced Section 17 metadata requirements. Therefore the latest source is not C78-proven.
- `DiagnosticsBits=0x00000001` was traced to using boolean `|` instead of LASAL integer `OR`; later capability evidence showed `0x000C633F`, topology advertised, TW19/TW20 true, and BootId `0x14`. Those values belong to that deployment, not automatically the final source.
- “Refresh Capabilities PASS” only means the RPC completed. It does not mean a feature was advertised, armed, transmitted, completed, or physically effective.
- An early WPF log printed `LMC Home Start ... PASS`; the UI was later corrected because `0x7D13` ACK is only “accepted / outcome pending”. The actual queried Axis2 record was quarantined, not a Home PASS.
- The earlier Axis1-then-other-axis failure exposed a completed Home record incorrectly intercepting later requests. Source fixes followed, but there is no post-fix real-axis continuity proof.
- Earlier summaries repeatedly state the weekday IDE window ends at `08:30`; the later explicit user instruction changed it to `08:00`. Use `08:00` unless reconfirmed.
- The ActualPosition `10000 DINT = 1 mm` statement is a current-source/application-unit interpretation. It is not raw encoder scaling and is not confirmation of the PLC image presently downloaded.
- Post-C78 helper split designs for DS402 receipt, Rollback, Publish, and Reserve are plans unless the history explicitly says source extraction was applied. Do not add those planned helpers to Section 17; Section 17 is fixed to the current eight helpers only.

## Per-file coverage: main history 1, parts 235-350

| Part | Logical lines | Topic / decision clue |
|---:|---:|---|
| 235 | 58501-58750 | LASAL `LMCControlCommandService` variable tree; copied the existing 324-word ownership array after one stale UI element failure. |
| 236 | 58751-59000 | Pasted the array, observed `OwnershipLeaseState0`, and attempted to rename it `OwnershipPreemptedState`; UIA `set_value` failed read-only. |
| 237 | 59001-59250 | Used inline edit instead; committed and verified `OwnershipPreemptedState`. |
| 238 | 59251-59500 | Keyboard paste did not create the intended copy; reopened the menu and copied the 324-word source array for identity state. |
| 239 | 59501-59750 | Created `OwnershipLeaseState0` duplicate and began renaming it `OwnershipIdentityState`. |
| 240 | 59751-60000 | Committed `OwnershipIdentityState`, expanded `0..323`, edited high bound to `431`, and started the lease-identity copy. |
| 241 | 60001-60250 | Continued creating a lease-identity array and recopying the 324-word source after an ambiguous first paste. |
| 242 | 60251-60500 | Verified another duplicate and entered inline rename for the lease-identity state. |
| 243 | 60501-60750 | Committed `OwnershipLeaseIdentityState`; copied the 432-word identity array and started a preempted-identity duplicate. |
| 244 | 60751-61000 | Observed `OwnershipIdentityState0` for the preempted copy; transitioned from indexed accessibility to lower-level desktop control. Also records all-day weekend/ROK-holiday IDE permission. |
| 245 | 61001-61250 | Selected and renamed the duplicate ownership identity variable; desktop-only UI state made the intermediate result less observable. |
| 246 | 61251-61500 | Verified ownership variables, collapsed the variable tree, and opened ownership Global function declarations. |
| 247 | 61501-61750 | Inspected `ReserveAxisOwnership` signature and began changing its identity pointer input type. |
| 248 | 61751-62000 | Typed `void` into the type editor; screenshots had no accessibility payload, so this fragment alone is not completion proof. |
| 249 | 62001-62250 | Continued screen-coordinate editing; unsupported `ArrowDown` led to retrieval of the Window2 API documentation. |
| 250 | 62251-62500 | API documentation continued; switched to supported `Down`/`Escape` keys and resumed the type-selection workflow. |
| 251 | 62501-62750 | Retried `void` selection and moved through type matches with `Down`; still screenshot-led rather than text-confirmed. |
| 252 | 62751-63000 | Committed the type and began renaming the next input to `IdentitySize`. |
| 253 | 63001-63250 | Re-entered and committed `IdentitySize`, then navigated to its properties. |
| 254 | 63251-63500 | Opened the type property and entered `UDINT` for `IdentitySize`. |
| 255 | 63501-63750 | Committed `UDINT`, returned to the Global method tree, and opened the next method-creation context. |
| 256 | 63751-64000 | Scrolled and used method context menus to create/select the identity-validation method. |
| 257 | 64001-64250 | Renamed and committed `ValidateAxisOwnershipIdentity`. |
| 258 | 64251-64500 | Added the `pIdentity` input to the new validation method. |
| 259 | 64501-64750 | Committed `pIdentity` and opened its pointer property controls. |
| 260 | 64751-65000 | Set pointer to `true` and moved to the element-type editor. |
| 261 | 65001-65250 | Entered `void` and navigated its type matches. |
| 262 | 65251-65500 | Committed the pointer type, returned to method context, and repositioned the declaration view. |
| 263 | 65501-65750 | Added/renamed the validation input `IdentitySize`. |
| 264 | 65751-66000 | Set and committed `IdentitySize : UDINT`. |
| 265 | 66001-66250 | Declared the first new ownership ABI with exact ordering, then moved to the next Global method. |
| 266 | 66251-66500 | Committed a method name and inspected the declaration; one malformed reconstructed-window call failed, then a fresh observation succeeded. |
| 267 | 66501-66750 | Opened the method menu and added the first preemption input. |
| 268 | 66751-67000 | Named the preemption input and opened its type selector. |
| 269 | 67001-67250 | Selected and committed the preemption input as `UDINT`. |
| 270 | 67251-67500 | Added `OwnerGeneration : UDINT` to the preemption method. |
| 271 | 67501-67750 | Opened the copy-preemption method and added its destination input. |
| 272 | 67751-68000 | Filtered and selected the plain `void` destination type. |
| 273 | 68001-68250 | Enabled and verified the destination pointer property. |
| 274 | 68251-68500 | Deleted an erroneous output, returned to copy-preemption, and began adding `OwnerGeneration`. |
| 275 | 68501-68750 | Committed `OwnerGeneration : UDINT` and added `DestinationSize`. |
| 276 | 68751-69000 | Set `DestinationSize : UDINT` and inspected the output menu. |
| 277 | 69001-69250 | Added `Result` output and verified the copy-preemption signature. |
| 278 | 69251-69500 | Moved `OwnerGeneration` before destination inputs to enforce ABI order. |
| 279 | 69501-69750 | Collapsed the copy method and returned to the Global methods folder. |
| 280 | 69751-70000 | Created the cleanup-publish method and committed its name. |
| 281 | 70001-70250 | Added cleanup `AxisMask : UDINT`. |
| 282 | 70251-70500 | Added `PreemptedToken : UDINT`. |
| 283 | 70501-70750 | Added `PreemptedGeneration : UDINT`. |
| 284 | 70751-71000 | Added `SafetyToken : UDINT`. |
| 285 | 71001-71250 | Added `SafetyGeneration : UDINT`. |
| 286 | 71251-71500 | Added `CleanupKind : UINT`. |
| 287 | 71501-71750 | Added `ReportValue0 : UDINT`. |
| 288 | 71751-72000 | Added `ReportValue1 : UDINT`. |
| 289 | 72001-72250 | Added `ObservationCycle : UDINT` and reviewed the cleanup signature. |
| 290 | 72251-72500 | Added cleanup `Result` output, then switched to `LMCEcatInputLatch`. |
| 291 | 72501-72750 | Navigated the large class tree to locate InputLatch declarations; several focus/scroll steps only. |
| 292 | 72751-73000 | Switched Global/Class views and repositioned around InputLatch members. |
| 293 | 73001-73250 | Located and expanded `LMCEcatInputLatch`; records the then-used weekday `08:30` policy later superseded by `08:00`. |
| 294 | 73251-73500 | Opened InputLatch variables and selected the applied-sequence variable as a template. |
| 295 | 73501-73750 | Closed a context menu, recovered from a bad scroll argument, and selected the Variables folder. |
| 296 | 73751-74000 | Created the cancel-sequence variable and opened its type field. |
| 297 | 74001-74250 | Set the cancel-sequence variable to `UDINT` and examined ordering controls. |
| 298 | 74251-74500 | Exited accidental name edit and navigated from variables to InputLatch methods. |
| 299 | 74501-74750 | Selected the methods folder/class root and inspected method creation menus. |
| 300 | 74751-75000 | Began creating `CancelAxisZeroHome`; the declaration did not yet have inputs/outputs. |
| 301 | 75001-75250 | Searched the Global method tail for the newly created cancel method. |
| 302 | 75251-75500 | Returned through method-folder and class-root menus after the method was hard to locate. |
| 303 | 75501-75750 | Inspected desktop controls and continued trying to establish `CancelAxisZeroHome` at the class root. |
| 304 | 75751-76000 | Used class-tab and Methods menus to navigate to the correct declaration location. |
| 305 | 76001-76250 | Attempted another method creation, canceled an incorrect private method, and re-inspected Global methods. |
| 306 | 76251-76500 | Repositioned the large class tree to the Global method root. |
| 307 | 76501-76750 | Opened the class/Global method menus at the correct root. |
| 308 | 76751-77000 | Created and named the cancel method, then reopened Global menus to verify persistence. |
| 309 | 77001-77250 | Recreated/recommitted the method after the first provisional entry was not retained. |
| 310 | 77251-77500 | Tried to attach parameters and confirmed the name-only method remained provisional. |
| 311 | 77501-77750 | Determined that LASAL discards a name-only function; kept its provisional icon active so inputs/outputs could be added. |
| 312 | 77751-78000 | Refreshed/collapsed/re-expanded the Global tree while locating the provisional method. |
| 313 | 78001-78250 | `Save All` proved `CancelAxisZeroHome` was generated in source; exited and relaunched LASAL to finish its ABI. |
| 314 | 78251-78500 | Opened the canonical project after restart. |
| 315 | 78501-78750 | Waited for load and searched for the InputLatch class. |
| 316 | 78751-79000 | Searched for `CancelAxisZeroHome` and opened its result. |
| 317 | 79001-79250 | Navigated definition/Class/Global views and opened the function context menu. |
| 318 | 79251-79500 | Changed the method’s Global access to `true`, with several selector/direct-edit retries. |
| 319 | 79501-79750 | Confirmed Global access and relocated the function definition. |
| 320 | 79751-80000 | Added `OperationToken` input and opened its type list. |
| 321 | 80001-80250 | Set `OperationToken : UDINT`, added `Result : DINT`, and saved the project. |
| 322 | 80251-80500 | Reordered `CancelAxisZeroHome`, saved, and exited LASAL while preserving the existing library choice. |
| 323 | 80501-80750 | Verified generated declarations/IDE exit, then began parallel Control/Diagnostics/TCP implementation and verifier updates; identified cancel-race and large-method risks. |
| 324 | 80751-81000 | Rejected multiple apparently static-PASS source candidates after finding undeclared locals, an always-failing cleanup path, cross-axis lease-bank deletion, and identity-suffix corruption; tightened positive and negative fixtures. |
| 325 | 81001-81250 | Enumerated applications/windows while trying to reestablish a valid LASAL target. |
| 326 | 81251-81500 | Located the canonical project through Explorer; one guessed-window reconstruction failed. Reasserted that IDE permission excludes download/hardware/gate activation. |
| 327 | 81501-81750 | Read and applied safe Windows UI targeting/reobservation instructions before further IDE input. |
| 328 | 81751-82000 | Continued the desktop-control/safety instruction payload; no project mutation decision in this fragment. |
| 329 | 82001-82250 | Selected and launched the canonical `Elmo_EtherCAT_Test_4Axis` LASAL project. |
| 330 | 82251-82500 | Static suites passed, but fresh C78 Rebuild failed `16 errors/50 warnings`: `_memcmp` returns `UDINT` while six locals were `DINT`. A retry showed stale open-IDE memory overwrote the external fix, so the IDE was closed. |
| 331 | 82501-82750 | Reapplied the six type corrections and reopened the canonical project from disk in a fresh LASAL session. |
| 332 | 82751-83000 | Fresh C78 ARM Rebuild succeeded `0 errors/50 warnings` with compiler/linker done; began Comm Network smoke. The 50 warnings were recorded as C78/C81 library-version warnings. |
| 333 | 83001-83250 | Ran `Find in Implementation` through Diagnostics, Control, and Motion Network objects/channels. |
| 334 | 83251-83500 | Navigated Motion Network/InputLatch; recovered from undefined observation and unsupported helper calls. |
| 335 | 83501-83750 | Completed four Network-channel implementation searches successfully; moved to large Control-class search. |
| 336 | 83751-84000 | Found/opened a front-section Control symbol after one out-of-window click. |
| 337 | 84001-84250 | Found/opened middle and end-section Control symbols. |
| 338 | 84251-84500 | Verified large-class front/middle/end navigation, no new smoke-relative `CInvalidArgException`, exited IDE, and summarized the earlier build checkpoint. The nested history then starts Home cancellation/drain work and contains the user’s later `08:00` time change. |
| 339 | 84501-84750 | Home safety-cancel static work led to user TW/topology reports; fixed capability/TW integer masks from `|` to `OR`, value-1 TW commands, SDK/WPF tests, and deployment guidance. User later reported TW19 worked but Home did not. |
| 340 | 84751-85000 | Distinguished `Identity Home Check` from actual LMC Home, implemented cancel→drain→cleanup→terminal publication ordering, and diagnosed Axis1-only success / later-axis detail `41`; instructed no retest until fixes were rebuilt/downloaded. |
| 341 | 85001-85250 | Fixed completed Home receipt intercepting later requests; corrected WPF ACK-versus-completion wording, expanded SDK/WPF regressions, and received a log where Axis2 Home Start was accepted/pending. |
| 342 | 85251-85500 | Query showed Axis2 `Quarantined`, error `-31000/detail 38`, position delta `+1`; continued DS402 safety work and produced the earlier Section 15 IDE declaration handoff. |
| 343 | 85501-85750 | Continued Section 15 signature text; user reported declaration work complete. External inspection then enabled DS402 retained receipt/WAL/drain implementation. User explicitly froze manuals during unstable implementation. |
| 344 | 85751-86000 | Closed DS402 warm-restart/journal ownership leaks, stage-86 cross-axis cleanup risk, and partial tombstone clearing; static checks passed, manuals remained untouched. |
| 345 | 86001-86250 | Recorded that downloaded PLC was still old; completed DS402 stage-87 WAL/drain fixes and static regressions, requested Rebuild/smoke without download, then advanced atomic-gate and helper work. |
| 346 | 86251-86500 | Implemented repeated Stop/no-resend and one-time Stop→PowerOff escalation, split RPC lifecycle to keep `MsgPaser` under all-CRLF 32 KiB, added TW19→successful-LMC-Home retained barrier, and expanded Section 17; then paused the earlier smaller handoff because Diagnostics also needed three size helpers. |
| 347 | 86501-86750 | Applied byte-exact Diagnostics split into encoder preemption, DS402 receipt, and DS402 cleanup helpers; final static suites passed. Section 17 became one hidden channel + eight private helpers; added size ratchet and preemption-cleanup semantic fixtures while waiting. |
| 348 | 86751-87000 | Added DS402 receipt and Rollback semantic/attack verifiers plus post-C78 split designs; preserved LASAL source/generated/Network, kept five waivers, and repeatedly confirmed all nine Section 17 names absent. |
| 349 | 87001-87250 | Completed Publish semantic verifier/design, then Reserve audit; found and corrected `preemptRecordBase` misuse and grew structural attacks through block/lexical wrappers. Section 17 remained pending. |
| 350 | 87251-87280 | Final Reserve/ownership/Rollback/Publish/size/SourceOnly results and Control SHA recorded. All nine Section 17 generated names still zero; stop point is Save All without Rebuild, and current C78/download/hardware remain unproven. |

## Per-file coverage: histories 2, 3, and 4

| History file | Logical lines | Topic / decision clue |
|---|---:|---|
| `Elmo_Master_history_260805_2_part_001` | 00001-00107 | Safe qualification order: old ABI/Network blocker, same-BootId read-only proof, true DS402 `0x6041`/`0x603F`, single-axis and true in-motion Stop before Group, Home/TW last; ACK is not completion. Immediate blocker details are historical, test-discipline remains applicable. |
| `Elmo_Master_history_260805_3_part_001` | 00001-00032 | ActualPosition is PLC MotionLib `APPUNIT` DINT, passed through by C#; current source maps `10000` to `1 mm`, not raw encoder count, and downloaded UNIT must still be confirmed. |
| `Elmo_Master_history_260805_4_part_001` | 00001-00126 | Old Axis1 recovery/topology/TW snapshot: topology unadvertised, capability queries PASS but no reset transmitted, Home status had AxisError. It classified LMC Home/TW19/TW20 as unavailable at that time; later main-history implementation supersedes those current-state claims. |

## Resume checklist for the next thread turn

1. Re-read live `git status`, Section 17 source/handoff, generated declarations, `Classes.lcb`, and LASAL process state before assuming the history endpoint is unchanged.
2. If all nine names remain absent, ask only for the exact Section 17 IDE visit. Do not expand it with post-C78 split plans.
3. On completion, external-inspect before Rebuild; do not trust “Save All done” alone.
4. Remove all five pre-IDE waivers and require default SourceOnly PASS before C78.
5. Treat the earlier C78 `0/50`, BootId `0x14`, topology, TW19, and Axis2 quarantine logs as historical checkpoints, not evidence for the current source.
6. After current build/smoke and cold download, qualify one axis only. Record BootId/capabilities/map, `0x6041`, `0x603F`, AxisError, exact Home terminal record, owner release, and next-axis admission before proceeding.
