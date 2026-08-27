# SetOperationMode MODE-11 Hardware/Packet Evidence

- Status: `NOT RUN`
- Qualification branch commit: `<sha>`
- Date/time: `<local + UTC>`
- Operator: `<name>`
- Test axis: `1`
- Production activation: `OFF / DO NOT MERGE`

## 1. Candidate identity

| Item | Evidence |
|---|---|
| Branch | `codex/setopmode-mode11-bench-activation` |
| Commit SHA | `<sha>` |
| Diagnostics gate | `TRUE` bench-only |
| Admin feature mask | `0x00000717` |
| `Classes.lcb` bytes / SHA-256 | `<value>` |
| project `.lcb` bytes / SHA-256 | `<value>` |
| C78/ARM build result | `<log/evidence>` |
| PLC load timestamp | `<value>` |
| DiagnosticsBuild | `<value>` |
| DiagnosticsBootId | `<value>` |
| MapRevision | `<value>` |
| Endpoint | `<ip:port>` |

## 2. MODE-11A — already CSP / zero-write

### Preconditions

| Check | Result |
|---|---|
| AxisReference | `1` |
| Fresh Admin capabilities | `<PASS/FAIL>` |
| bits 8/9/10 all advertised | `<PASS/FAIL>` |
| fresh Diagnostics identity | `<PASS/FAIL>` |
| unresolved WPF recovery journal absent | `<PASS/FAIL>` |
| competing mutation/SDO owner absent | `<PASS/FAIL>` |
| pre-read `0x6061` | `<must be 8>` |

### Start / outcome / retire

| Field | Evidence |
|---|---|
| Original Start RequestId | `<value>` |
| ClientIntentId[0..3] | `<values>` |
| Start ACK | `<capture/result>` |
| `0x7D23` packet | `<capture ref>` |
| `0x7D24` QueryRequestId | `<value>` |
| RecordState | `<Succeeded expected>` |
| ObservedModeRaw | `<8 expected>` |
| EvidenceFlags raw | `<value>` |
| WriteRequested | `<0 expected>` |
| WriteDispatched | `<0 expected>` |
| VerifyReadDispatched | `<1 expected>` |
| VerifyReadCompleted | `<1 expected>` |
| OwnerReleased | `<1 expected>` |
| ExecutorReusable | `<1 expected>` |
| RecordGeneration | `<nonzero>` |
| StartCycle / CompletionCycle | `<values>` |
| OriginalErrorId / Detail / Status | `<0 / 0 / 0 expected>` |
| post-read `0x6061` | `<8 expected>` |
| EtherCAT/SDO `0x6060` write count | `<0 required>` |
| `0x7D25` RetireRequestId | `<value>` |
| retired generation | `<must equal RecordGeneration>` |
| retirement confirmed | `<PASS/FAIL>` |
| WPF journal resolved after retire | `<PASS/FAIL>` |

### MODE-11A verdict

`<PASS / FAIL / INDETERMINATE>`

A Start ACK alone cannot produce PASS. Any observed `0x6060` write makes MODE-11A FAIL.

## 3. MODE-11B — non-CSP / exact one-write

- Initial non-CSP mode setup method: `<must be independently approved; generic D5 0x6060 prohibited>`
- Setup evidence: `<reference>`

### Preconditions

| Check | Result |
|---|---|
| pre-read `0x6061` | `<non-8>` |
| physical context valid | `<PASS/FAIL>` |
| axis standstill | `<PASS/FAIL>` |
| DS402 Fault clear | `<PASS/FAIL>` |
| DS402 Operation Enabled clear | `<PASS/FAIL>` |
| DS402 Home inactive | `<PASS/FAIL>` |
| Encoder Maintenance inactive | `<PASS/FAIL>` |
| competing SDO/mutation inactive | `<PASS/FAIL>` |

### Start / write / verify / retire

| Field | Evidence |
|---|---|
| Original Start RequestId | `<value>` |
| ClientIntentId[0..3] | `<values>` |
| Start ACK | `<capture/result>` |
| `0x7D23` packet | `<capture ref>` |
| WriteRequested | `<1 expected>` |
| WriteDispatched | `<1 expected>` |
| EtherCAT/SDO `0x6060:0` writes | `<exactly 1 required>` |
| write payload | `<one byte 08 required>` |
| replay after dispatch | `<0 required>` |
| VerifyReadDispatched | `<1 expected>` |
| VerifyReadCompleted | `<1 expected>` |
| post-read `0x6061` | `<8 expected>` |
| RecordState | `<Succeeded expected>` |
| ObservedModeRaw | `<8 expected>` |
| RecordGeneration | `<nonzero>` |
| OwnerReleased | `<1 expected>` |
| ExecutorReusable | `<1 expected>` |
| `0x7D24` packet/result | `<capture ref>` |
| `0x7D25` exact-generation retire | `<capture ref>` |
| WPF journal resolved after retire | `<PASS/FAIL>` |

### MODE-11B verdict

`<PASS / FAIL / INDETERMINATE / NOT RUN>`

Any ambiguous write-dispatch result moves to MODE-12 recovery evidence; never replay original Start.

## 4. Deviations / faults

`<record exactly; do not normalize timeout/disconnect/quarantine into ordinary failure or success>`

## 5. Overall MODE-11 verdict

- MODE-11A zero-write: `<result>`
- MODE-11B exact-one-write: `<result>`
- MODE-11 overall: `<PASS only when both required cases and packet/readback evidence are complete>`
- MODE-12 permitted to start: `<YES/NO>`
- MODE-14 production activation permitted: `NO` until MODE-12 also passes and activation review is complete
