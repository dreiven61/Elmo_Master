# SetOperationMode MODE-11 Machine-Checkable Evidence JSON

This sidecar format is used only to make the MODE-11A / MODE-11B acceptance checks deterministic after the
actual C78/PLC/hardware run. The packet capture, PLC observation and physical evidence remain primary evidence;
this JSON is a structured transcription of those observations.

Verifier:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Verify-SetOperationModeMode11Evidence.ps1 `
  -EvidencePath .\mode11a.json
```

A verifier PASS does not authenticate a capture file and does not create hardware evidence that was not actually
observed. It only checks that the supplied evidence is internally consistent with the frozen SetOperationMode
contract.

## 1. Common candidate fields

`Candidate` must contain:

- `Branch`: exactly `codex/setopmode-mode11-bench-activation`
- `CommitSha`: exact 40-hex tested commit
- `DiagnosticsGate`: exactly `TRUE`
- `AdminFeatureMask`: numeric `1815` (`0x00000717`)
- `ActiveSourceDiffSha256`, `DiagnosticsSourceSha256`, `ControlSourceSha256`, `ClassesSha256`,
  `ProjectSha256`: exact 64-hex SHA-256 values
- `ClassesBytes`, `ProjectBytes`: nonzero
- `DiagnosticsBuild`, `DiagnosticsBootId`, `MapRevision`: all nonzero and from the same loaded image
- `AxisReference`: `1` for initial MODE-11 qualification
- `Endpoint`, `PlcLoadTimestamp`: non-empty evidence strings
- `C78BuildPassed`, `PlcLoadPassed`: both `true`

Use `Capture-SetOperationModeMode11BenchCheckpoint.ps1` to obtain the active source/artifact identity rather
than manually reconstructing it.

## 2. Common run fields

`Run` must contain:

- nonzero `StartRequestId`, `QueryRequestId`, `RetireRequestId`
- exactly four `ClientIntentId` UInt32 words, not all zero
- `StartAckAccepted = true`
- `StartPacketCount = 1`
- `StartReplayCount = 0`
- `QueryPacketCount >= 1`
- `RetirePacketCount = 1`
- non-empty `TcpPacketReference`, `SdoPacketReference`
- `RecordState = "Succeeded"`
- `ObservedModeRaw = 8`
- `PostRead6061 = 8`
- `OriginalCommandStatus = 0`, `OriginalErrorId = 0`, `OriginalDetailCode = 0`
- `NativeCommandState = 0`
- nonzero `StartCycle`, `CompletionCycle >= StartCycle`
- nonzero `RecordGeneration`
- `RetiredGeneration == RecordGeneration`
- `RetirementConfirmed = true`
- `JournalResolvedAfterRetire = true`
- `Verdict = "PASS"`

Known `EvidenceFlags` bits are exactly:

| Bit | Value | Meaning |
|---:|---:|---|
| 0 | 1 | WriteRequested |
| 1 | 2 | WriteDispatched |
| 2 | 4 | VerifyReadDispatched |
| 3 | 8 | VerifyReadCompleted |
| 4 | 16 | OwnerReleased |
| 5 | 32 | ExecutorReusable |

Unknown evidence bits are rejected.

## 3. MODE-11A specific contract

- `Case = "MODE-11A"`
- `PreRead6061 = 8`
- `EvidenceFlags` must contain bits 2/3/4/5 and must not contain bits 0/1; the normal exact value is `60`
- `Sdo6060WriteCount = 0`
- `Sdo6060WritePayloadHex = []`

Any observed `0x6060` write makes MODE-11A fail.

## 4. MODE-11B specific contract

- `Case = "MODE-11B"`
- `PreRead6061` must be non-8
- all six evidence bits must be present; the normal exact value is `63`
- `Sdo6060WriteCount = 1`
- `Sdo6060WritePayloadHex = ["08"]`
- `Preconditions.InitialNonCspSetupApproved = true`
- `Preconditions.InitialNonCspSetupReference` must identify the separately approved setup method/evidence
- `PhysicalContextValid`, `AxisStandstill`, `Ds402FaultClear`, `Ds402OperationEnabledClear`,
  `Ds402HomeInactive`, `EncoderMaintenanceInactive`, `CompetingMutationInactive` must all be `true`

Generic D5 `0x6060` write is not an approved setup method.

## 5. MODE-11A example shape

```json
{
  "SchemaVersion": 1,
  "Case": "MODE-11A",
  "Candidate": {
    "Branch": "codex/setopmode-mode11-bench-activation",
    "CommitSha": "0123456789abcdef0123456789abcdef01234567",
    "DiagnosticsGate": "TRUE",
    "AdminFeatureMask": 1815,
    "ActiveSourceDiffSha256": "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
    "DiagnosticsSourceSha256": "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
    "ControlSourceSha256": "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
    "ClassesSha256": "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD",
    "ProjectSha256": "EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE",
    "ClassesBytes": 8635373,
    "ProjectBytes": 634865,
    "DiagnosticsBuild": 1,
    "DiagnosticsBootId": 1,
    "MapRevision": 1,
    "AxisReference": 1,
    "Endpoint": "<ip:port>",
    "PlcLoadTimestamp": "<timestamp>",
    "C78BuildPassed": true,
    "PlcLoadPassed": true
  },
  "Run": {
    "PreRead6061": 8,
    "PostRead6061": 8,
    "StartRequestId": 1,
    "QueryRequestId": 2,
    "RetireRequestId": 3,
    "ClientIntentId": [1, 2, 3, 4],
    "StartAckAccepted": true,
    "StartPacketCount": 1,
    "StartReplayCount": 0,
    "QueryPacketCount": 1,
    "RetirePacketCount": 1,
    "TcpPacketReference": "<capture reference>",
    "SdoPacketReference": "<capture reference>",
    "RecordState": "Succeeded",
    "ObservedModeRaw": 8,
    "OriginalCommandStatus": 0,
    "OriginalErrorId": 0,
    "OriginalDetailCode": 0,
    "NativeCommandState": 0,
    "EvidenceFlags": 60,
    "StartCycle": 1,
    "CompletionCycle": 2,
    "RecordGeneration": 1,
    "RetiredGeneration": 1,
    "RetirementConfirmed": true,
    "JournalResolvedAfterRetire": true,
    "Sdo6060WriteCount": 0,
    "Sdo6060WritePayloadHex": [],
    "Verdict": "PASS",
    "Preconditions": {}
  }
}
```

Replace every illustrative identity/capture value with the exact observed value. Do not treat the example as
qualification evidence.
