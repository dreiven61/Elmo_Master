using System;
using System.Collections.Generic;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class BulkPartialQualificationResult
    {
        internal BulkPartialQualificationResult(
            byte offlineSourceIndex,
            int invalidEntryCount,
            int validEntryCount)
        {
            OfflineSourceIndex = offlineSourceIndex;
            InvalidEntryCount = invalidEntryCount;
            ValidEntryCount = validEntryCount;
        }

        internal byte OfflineSourceIndex { get; private set; }
        internal int InvalidEntryCount { get; private set; }
        internal int ValidEntryCount { get; private set; }
    }

    internal sealed class BulkPartialD1FaultResult
    {
        internal BulkPartialD1FaultResult(
            byte offlineSourceIndex,
            uint signalId,
            uint staleRawValue)
        {
            OfflineSourceIndex = offlineSourceIndex;
            SignalId = signalId;
            StaleRawValue = staleRawValue;
        }

        internal byte OfflineSourceIndex { get; private set; }
        internal uint SignalId { get; private set; }
        internal uint StaleRawValue { get; private set; }
        internal bool FaultValueIsCurrent { get { return false; } }
    }

    internal sealed class BulkPartialQualificationInconclusiveException
        : InvalidOperationException
    {
        internal BulkPartialQualificationInconclusiveException(string message)
            : base(message)
        {
        }
    }

    internal static class BulkPartialQualificationAnalysis
    {
        private const int ExpectedSignalCount = 24;
        private const int ExpectedAxisCount = 4;
        private const int ExpectedSignalsPerAxis = 6;

        internal static IReadOnlyList<LMCSignalCatalogEntry>
            SelectRepresentativePIEntries(
                IReadOnlyList<LMCSignalCatalogEntry> entries)
        {
            ValidateCatalogTopology(entries);
            var representatives = new List<LMCSignalCatalogEntry>(
                ExpectedAxisCount);
            for (byte sourceIndex = 1;
                sourceIndex <= ExpectedAxisCount;
                sourceIndex++)
            {
                LMCSignalCatalogEntry representative = null;
                for (var index = 0; index < entries.Count; index++)
                {
                    var entry = entries[index];
                    if (entry.SourceIndex == sourceIndex
                        && (entry.SignalId & 0xFFu) == 6u)
                    {
                        representative = entry;
                        break;
                    }
                }

                if (representative == null)
                {
                    throw new InvalidOperationException(
                        "Bulk partial qualification could not select the Status Word PI for physical axis "
                        + sourceIndex
                        + ".");
                }

                representatives.Add(representative);
            }

            return representatives.AsReadOnly();
        }

        internal static void ValidateCatalogTopology(
            IReadOnlyList<LMCSignalCatalogEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException("entries");
            }

            if (entries.Count != ExpectedSignalCount)
            {
                throw new InvalidOperationException(
                    "Bulk partial qualification requires exactly 24 Catalog entries.");
            }

            var sourceCounts = new int[ExpectedAxisCount + 1];
            var sourceSignalCodes = new bool[
                ExpectedAxisCount + 1,
                ExpectedSignalsPerAxis + 1];
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null)
                {
                    throw new InvalidOperationException(
                        "Bulk partial qualification Catalog contains a null entry.");
                }

                var encodedAxis = (byte)((entry.SignalId >> 8) & 0xFFu);
                var signalCode = (byte)(entry.SignalId & 0xFFu);
                if ((entry.SignalId & 0xFFFF0000u) != 0x00100000u
                    || entry.SourceIndex < 1
                    || entry.SourceIndex > ExpectedAxisCount
                    || encodedAxis != entry.SourceIndex
                    || signalCode < 1
                    || signalCode > ExpectedSignalsPerAxis
                    || sourceSignalCodes[entry.SourceIndex, signalCode])
                {
                    throw new InvalidOperationException(
                        "Bulk partial qualification Catalog does not contain the exact axis1..4 by six-signal topology.");
                }

                ValidateCatalogEntryContract(entry, index, signalCode);

                sourceCounts[entry.SourceIndex]++;
                sourceSignalCodes[entry.SourceIndex, signalCode] = true;
            }

            for (var sourceIndex = 1;
                sourceIndex <= ExpectedAxisCount;
                sourceIndex++)
            {
                if (sourceCounts[sourceIndex] != ExpectedSignalsPerAxis)
                {
                    throw new InvalidOperationException(
                        "Bulk partial qualification requires six entries for each physical axis SourceIndex.");
                }
            }
        }

        internal static void ValidateBaseline(
            IReadOnlyList<LMCSignalCatalogEntry> entries,
            uint expectedBulkId,
            uint expectedConfigRevision,
            uint expectedMapRevision,
            ushort expectedEntryStride,
            LMCBulkSnapshot snapshot,
            string stage)
        {
            ValidateCommon(
                entries,
                expectedBulkId,
                expectedConfigRevision,
                expectedMapRevision,
                expectedEntryStride,
                snapshot,
                stage);
            if (snapshot.IsPartial
                || snapshot.Response.ResponseFlags
                    != LMCDiagnosticsResponseFlags.None)
            {
                throw new InvalidOperationException(
                    stage + " must be a non-partial Bulk snapshot.");
            }

            for (var index = 0; index < snapshot.Entries.Count; index++)
            {
                var actual = snapshot.Entries[index];
                if (actual.EntryStatus != LMCSignalEntryStatus.Valid
                    || actual.Detail != LMCDiagnosticsDetailCode.None)
                {
                    throw new InvalidOperationException(
                        stage + " contains a non-valid entry at index "
                        + index
                        + ".");
                }
            }
        }

        internal static BulkPartialQualificationResult
            ValidateOneSlaveOffline(
                IReadOnlyList<LMCSignalCatalogEntry> entries,
                uint expectedBulkId,
                uint expectedConfigRevision,
                uint expectedMapRevision,
                ushort expectedEntryStride,
                LMCBulkSnapshot snapshot)
        {
            ValidateCommon(
                entries,
                expectedBulkId,
                expectedConfigRevision,
                expectedMapRevision,
                expectedEntryStride,
                snapshot,
                "one-slave-offline fault snapshot");
            if (!snapshot.IsPartial
                || snapshot.Response.ResponseFlags
                    != LMCDiagnosticsResponseFlags.Partial)
            {
                throw new InvalidOperationException(
                    "One-slave-offline snapshot must carry only the Partial response flag.");
            }

            byte offlineSourceIndex = 0;
            var invalidCount = 0;
            var validCount = 0;
            for (var index = 0; index < snapshot.Entries.Count; index++)
            {
                var actual = snapshot.Entries[index];
                var expected = entries[index];
                if (actual.EntryStatus == LMCSignalEntryStatus.Valid
                    && actual.Detail == LMCDiagnosticsDetailCode.None)
                {
                    validCount++;
                    continue;
                }

                if (!IsSlaveOffline(actual))
                {
                    throw new InvalidOperationException(
                        "Partial Bulk entry does not contain SlaveOffline without Valid and Detail=18 at index "
                        + index
                        + ".");
                }

                if (offlineSourceIndex == 0)
                {
                    offlineSourceIndex = expected.SourceIndex;
                }
                else if (offlineSourceIndex != expected.SourceIndex)
                {
                    throw new BulkPartialQualificationInconclusiveException(
                        "INCONCLUSIVE: Partial Bulk entries span more than one physical axis; the external topology did not isolate exactly one slave.");
                }

                invalidCount++;
            }

            if (offlineSourceIndex == 0
                || invalidCount != ExpectedSignalsPerAxis
                || validCount
                    != ExpectedSignalCount - ExpectedSignalsPerAxis)
            {
                throw new InvalidOperationException(
                    "One-slave-offline snapshot must contain exactly six invalid and eighteen valid entries.");
            }

            for (var index = 0; index < entries.Count; index++)
            {
                var belongsToOfflineAxis =
                    entries[index].SourceIndex == offlineSourceIndex;
                var isOffline = IsSlaveOffline(snapshot.Entries[index]);
                if (belongsToOfflineAxis != isOffline)
                {
                    throw new InvalidOperationException(
                        "One-slave-offline invalid entries do not exactly match one SourceIndex group.");
                }
            }

            return new BulkPartialQualificationResult(
                offlineSourceIndex,
                invalidCount,
                validCount);
        }

        internal static void ValidateD1Baseline(
            uint expectedMapRevision,
            IReadOnlyList<LMCSignalCatalogEntry> representatives,
            LMCEtherCATHealth health,
            IReadOnlyList<LMCSignalValue> values)
        {
            ValidateD1Common(
                expectedMapRevision,
                representatives,
                health,
                values,
                "D1 baseline");
            ValidateD1HealthyState(health, values, "D1 baseline");
        }

        internal static BulkPartialD1FaultResult ValidateD1Fault(
            uint expectedMapRevision,
            IReadOnlyList<LMCSignalCatalogEntry> representatives,
            LMCEtherCATHealth baselineHealth,
            IReadOnlyList<LMCSignalValue> baselineValues,
            LMCEtherCATHealth faultHealth,
            IReadOnlyList<LMCSignalValue> faultValues,
            byte expectedOfflineSourceIndex)
        {
            ValidateExpectedSourceIndex(expectedOfflineSourceIndex);
            ValidateD1Common(
                expectedMapRevision,
                representatives,
                baselineHealth,
                baselineValues,
                "D1 baseline reference");
            ValidateD1HealthyState(
                baselineHealth,
                baselineValues,
                "D1 baseline reference");
            ValidateD1Common(
                expectedMapRevision,
                representatives,
                faultHealth,
                faultValues,
                "D1 fault");
            EnsureD1Advanced(
                "D1 Health baseline-to-fault",
                baselineHealth.CycleCounter,
                baselineHealth.TimestampUs,
                faultHealth.CycleCounter,
                faultHealth.TimestampUs);

            byte healthOfflineSourceIndex = 0;
            var healthOfflineCount = 0;
            for (var index = 0; index < faultHealth.Slaves.Count; index++)
            {
                var slave = faultHealth.Slaves[index];
                if (!slave.Online)
                {
                    healthOfflineCount++;
                    healthOfflineSourceIndex = checked((byte)slave.PhysicalAxis);
                }
            }

            if (healthOfflineCount > 1)
            {
                throw new BulkPartialQualificationInconclusiveException(
                    "INCONCLUSIVE: D1 Health reports more than one offline physical axis; the external topology did not isolate exactly one slave.");
            }

            if (healthOfflineCount != 1
                || healthOfflineSourceIndex != expectedOfflineSourceIndex)
            {
                throw new InvalidOperationException(
                    "D1 Health offline physical axis does not exactly match the Bulk offline SourceIndex.");
            }

            ValidateD1MasterOperational(faultHealth, "D1 fault");
            for (var index = 0; index < ExpectedAxisCount; index++)
            {
                var sourceIndex = checked((byte)(index + 1));
                var slave = faultHealth.Slaves[index];
                var value = faultValues[index];
                EnsureD1Advanced(
                    "D1 PI axis " + sourceIndex + " baseline-to-fault",
                    baselineValues[index].CycleCounter,
                    baselineValues[index].TimestampUs,
                    value.CycleCounter,
                    value.TimestampUs);

                if (sourceIndex == expectedOfflineSourceIndex)
                {
                    if (slave.Online || slave.EtherCATState == 8)
                    {
                        throw new InvalidOperationException(
                            "D1 Health did not report the expected physical axis as offline and non-OP.");
                    }

                    var status = value.Entry.EntryStatus;
                    if (value.Entry.IsValid
                        || (status & LMCSignalEntryStatus.Valid) != 0
                        || (status & LMCSignalEntryStatus.SlaveOffline)
                            != LMCSignalEntryStatus.SlaveOffline
                        || value.Entry.Detail
                            != LMCDiagnosticsDetailCode.SlaveOffline)
                    {
                        throw new InvalidOperationException(
                            "D1 fault PI must remove Valid, include SlaveOffline, and use Detail=18.");
                    }

                    EnsureD1UInt32Advanced(
                        "D1 Health fault LastStateChangeCycle",
                        baselineHealth.Slaves[index].LastStateChangeCycle,
                        slave.LastStateChangeCycle);
                    continue;
                }

                ValidateD1HealthySlaveAndValue(
                    slave,
                    value,
                    "D1 fault unaffected axis " + sourceIndex);
            }

            var affectedIndex = expectedOfflineSourceIndex - 1;
            var affectedValue = faultValues[affectedIndex];
            return new BulkPartialD1FaultResult(
                expectedOfflineSourceIndex,
                affectedValue.SignalId,
                affectedValue.RawValue32);
        }

        internal static void ValidateD1Recovery(
            uint expectedMapRevision,
            IReadOnlyList<LMCSignalCatalogEntry> representatives,
            LMCEtherCATHealth faultHealth,
            IReadOnlyList<LMCSignalValue> faultValues,
            LMCEtherCATHealth recoveryHealth,
            IReadOnlyList<LMCSignalValue> recoveryValues,
            byte expectedOfflineSourceIndex)
        {
            ValidateExpectedSourceIndex(expectedOfflineSourceIndex);
            ValidateD1Common(
                expectedMapRevision,
                representatives,
                faultHealth,
                faultValues,
                "D1 fault reference");
            ValidateD1Common(
                expectedMapRevision,
                representatives,
                recoveryHealth,
                recoveryValues,
                "D1 recovery");
            ValidateD1HealthyState(
                recoveryHealth,
                recoveryValues,
                "D1 recovery");
            EnsureD1Advanced(
                "D1 Health fault-to-recovery",
                faultHealth.CycleCounter,
                faultHealth.TimestampUs,
                recoveryHealth.CycleCounter,
                recoveryHealth.TimestampUs);

            for (var index = 0; index < ExpectedAxisCount; index++)
            {
                EnsureD1Advanced(
                    "D1 PI axis " + (index + 1)
                        + " fault-to-recovery",
                    faultValues[index].CycleCounter,
                    faultValues[index].TimestampUs,
                    recoveryValues[index].CycleCounter,
                    recoveryValues[index].TimestampUs);
            }

            var affectedIndex = expectedOfflineSourceIndex - 1;
            EnsureD1UInt32Advanced(
                "D1 Health recovery LastValidCycle",
                faultHealth.Slaves[affectedIndex].LastValidCycle,
                recoveryHealth.Slaves[affectedIndex].LastValidCycle);
            EnsureD1UInt32Advanced(
                "D1 Health recovery LastStateChangeCycle",
                faultHealth.Slaves[affectedIndex].LastStateChangeCycle,
                recoveryHealth.Slaves[affectedIndex].LastStateChangeCycle);
        }

        internal static void ValidateRecoveryPending(
            IReadOnlyList<LMCSignalCatalogEntry> entries,
            uint expectedBulkId,
            uint expectedConfigRevision,
            uint expectedMapRevision,
            ushort expectedEntryStride,
            LMCBulkSnapshot snapshot,
            byte expectedSourceIndex)
        {
            ValidateCommon(
                entries,
                expectedBulkId,
                expectedConfigRevision,
                expectedMapRevision,
                expectedEntryStride,
                snapshot,
                "one-slave recovery transition snapshot");
            if (expectedSourceIndex < 1
                || expectedSourceIndex > ExpectedAxisCount)
            {
                throw new ArgumentOutOfRangeException("expectedSourceIndex");
            }

            if (!snapshot.IsPartial
                || snapshot.Response.ResponseFlags
                    != LMCDiagnosticsResponseFlags.Partial)
            {
                throw new InvalidOperationException(
                    "A pending recovery snapshot must carry only the Partial response flag.");
            }

            var invalidCount = 0;
            for (var index = 0; index < entries.Count; index++)
            {
                var expected = entries[index];
                var actual = snapshot.Entries[index];
                if (expected.SourceIndex != expectedSourceIndex)
                {
                    if (actual.EntryStatus != LMCSignalEntryStatus.Valid
                        || actual.Detail != LMCDiagnosticsDetailCode.None)
                    {
                        throw new InvalidOperationException(
                            "Recovery transition invalidated an entry outside the original offline SourceIndex.");
                    }

                    continue;
                }

                var remainsOffline = IsSlaveOffline(actual);
                var awaitingOperational = IsSlaveNotOperational(actual);
                if (!remainsOffline && !awaitingOperational)
                {
                    throw new InvalidOperationException(
                        "Recovery transition for the expected SourceIndex is neither SlaveOffline nor SlaveNotOperational.");
                }

                invalidCount++;
            }

            if (invalidCount != ExpectedSignalsPerAxis)
            {
                throw new InvalidOperationException(
                    "Recovery transition must keep exactly the original six-entry SourceIndex group invalid until full recovery.");
            }
        }

        private static void ValidateCatalogEntryContract(
            LMCSignalCatalogEntry entry,
            int expectedCatalogIndex,
            byte signalCode)
        {
            var outputSignal = signalCode <= 3;
            var expectedSourceKind = outputSignal
                ? LMCSignalSourceKind.PdoOutputLastTx
                : LMCSignalSourceKind.PdoInput;
            var expectedDirection = outputSignal
                ? LMCPdoDirection.MasterToDrive
                : LMCPdoDirection.DriveToMaster;
            var expectedType = signalCode == 1 || signalCode == 4
                ? LMCSignalValueType.Int32
                : signalCode == 3 || signalCode == 6
                    ? LMCSignalValueType.BitField16
                    : LMCSignalValueType.BitField32;
            var expectedWidth = expectedType == LMCSignalValueType.BitField16
                ? (byte)2
                : (byte)4;
            var expectedPdoIndex = signalCode == 1
                ? (ushort)0x607A
                : signalCode == 2
                    ? (ushort)0x60FE
                    : signalCode == 3
                        ? (ushort)0x6040
                        : signalCode == 4
                            ? (ushort)0x6064
                            : signalCode == 5
                                ? (ushort)0x60FD
                                : (ushort)0x6041;
            var expectedPdoSubIndex = signalCode == 2 ? (byte)1 : (byte)0;
            var expectedAccessFlags = LMCSignalAccessFlags.Readable
                | LMCSignalAccessFlags.Recordable
                | LMCSignalAccessFlags.BulkReadable;
            var expectedSignalFlags = LMCSignalFlags.ActivePdo
                | LMCSignalFlags.PhysicalAxis
                | LMCSignalFlags.InputMappedPhase;

            if (entry.CatalogIndex != expectedCatalogIndex
                || entry.SourceKind != expectedSourceKind
                || entry.DataType != expectedType
                || entry.ByteWidth != expectedWidth
                || entry.AccessFlags != expectedAccessFlags
                || entry.SignalFlags != expectedSignalFlags
                || entry.PdoIndex != expectedPdoIndex
                || entry.PdoSubIndex != expectedPdoSubIndex
                || entry.PdoDirection != expectedDirection
                || entry.ScaleNumerator != 1
                || entry.ScaleDenominator != 1)
            {
                throw new InvalidOperationException(
                    "Bulk partial qualification Catalog entry contract mismatch at index "
                    + expectedCatalogIndex
                    + ".");
            }
        }

        private static bool IsSlaveOffline(LMCSignalValueEntry entry)
        {
            return (entry.EntryStatus & LMCSignalEntryStatus.Valid) == 0
                && (entry.EntryStatus & LMCSignalEntryStatus.SlaveOffline)
                    == LMCSignalEntryStatus.SlaveOffline
                && entry.Detail == LMCDiagnosticsDetailCode.SlaveOffline;
        }

        private static bool IsSlaveNotOperational(LMCSignalValueEntry entry)
        {
            return (entry.EntryStatus & LMCSignalEntryStatus.Valid) == 0
                && (entry.EntryStatus & LMCSignalEntryStatus.SlaveOffline) == 0
                && (entry.EntryStatus
                        & LMCSignalEntryStatus.SlaveNotOperational)
                    == LMCSignalEntryStatus.SlaveNotOperational
                && entry.Detail == LMCDiagnosticsDetailCode.NotReady;
        }

        private static void ValidateD1Common(
            uint expectedMapRevision,
            IReadOnlyList<LMCSignalCatalogEntry> representatives,
            LMCEtherCATHealth health,
            IReadOnlyList<LMCSignalValue> values,
            string stage)
        {
            if (expectedMapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("expectedMapRevision");
            }

            if (representatives == null
                || representatives.Count != ExpectedAxisCount
                || values == null
                || values.Count != ExpectedAxisCount)
            {
                throw new InvalidOperationException(
                    stage + " requires exactly four representative PI entries and values.");
            }

            if (health == null
                || health.Response == null
                || !health.Response.IsSuccess
                || health.MapRevision != expectedMapRevision
                || health.CapturePhase != LMCCapturePhase.InputMapped
                || health.Slaves == null
                || health.Slaves.Count != ExpectedAxisCount
                || (health.SnapshotSequence & 1u) != 0)
            {
                throw new InvalidOperationException(
                    stage + " Health identity, phase, count, or seqlock contract failed.");
            }

            for (var index = 0; index < ExpectedAxisCount; index++)
            {
                var sourceIndex = checked((byte)(index + 1));
                var representative = representatives[index];
                var expectedSignalId = 0x00100000u
                    | ((uint)sourceIndex << 8)
                    | 6u;
                if (representative == null
                    || representative.SourceIndex != sourceIndex
                    || representative.SignalId != expectedSignalId
                    || representative.SourceKind
                        != LMCSignalSourceKind.PdoInput
                    || representative.DataType
                        != LMCSignalValueType.BitField16
                    || representative.ByteWidth != 2
                    || representative.PdoIndex != 0x6041
                    || representative.PdoSubIndex != 0
                    || representative.PdoDirection
                        != LMCPdoDirection.DriveToMaster)
                {
                    throw new InvalidOperationException(
                        stage + " representative PI is not the ordered axis Status Word at index "
                        + index
                        + ".");
                }

                var slave = health.Slaves[index];
                if (slave == null
                    || slave.SlaveIndex != index
                    || slave.PhysicalAxis != sourceIndex)
                {
                    throw new InvalidOperationException(
                        stage + " Health axis order changed at index "
                        + index
                        + ".");
                }

                var value = values[index];
                if (value == null
                    || value.Response == null
                    || !value.Response.IsSuccess
                    || value.MapRevision != expectedMapRevision
                    || value.CapturePhase != LMCCapturePhase.InputMapped
                    || value.Entry == null
                    || value.SignalId != representative.SignalId
                    || value.ValueType != representative.DataType)
                {
                    throw new InvalidOperationException(
                        stage + " PI identity, type, map, or phase changed at index "
                        + index
                        + ".");
                }
            }
        }

        private static void ValidateD1HealthyState(
            LMCEtherCATHealth health,
            IReadOnlyList<LMCSignalValue> values,
            string stage)
        {
            ValidateD1MasterOperational(health, stage);
            for (var index = 0; index < ExpectedAxisCount; index++)
            {
                ValidateD1HealthySlaveAndValue(
                    health.Slaves[index],
                    values[index],
                    stage + " axis " + (index + 1));
            }
        }

        private static void ValidateD1MasterOperational(
            LMCEtherCATHealth health,
            string stage)
        {
            if (health.MasterState != 8
                || health.MasterFlags
                    != LMCEtherCATMasterFlags.MasterOperational
                || health.ConsecutiveInvalidCycles != 0)
            {
                throw new InvalidOperationException(
                    stage + " requires an operational EtherCAT master without an active invalid frame.");
            }
        }

        private static void ValidateD1HealthySlaveAndValue(
            LMCEtherCATSlaveHealth slave,
            LMCSignalValue value,
            string stage)
        {
            if (!slave.Online
                || slave.EtherCATState != 8
                || slave.ALStatusCode != 0
                || slave.ClassState == uint.MaxValue
                || slave.AxisError != 0
                || value.Entry.EntryStatus != LMCSignalEntryStatus.Valid
                || !value.Entry.IsValid
                || value.Entry.Detail != LMCDiagnosticsDetailCode.None)
            {
                throw new InvalidOperationException(
                    stage + " must keep Health online/OP and PI current/Valid.");
            }
        }

        private static void ValidateExpectedSourceIndex(byte sourceIndex)
        {
            if (sourceIndex < 1 || sourceIndex > ExpectedAxisCount)
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }
        }

        private static void EnsureD1Advanced(
            string fieldName,
            uint previousCycle,
            ulong previousTimestamp,
            uint currentCycle,
            ulong currentTimestamp)
        {
            EnsureD1UInt32Advanced(
                fieldName + " CycleCounter",
                previousCycle,
                currentCycle);
            var timestampDelta = unchecked(currentTimestamp - previousTimestamp);
            if (timestampDelta == 0 || timestampDelta > long.MaxValue)
            {
                throw new InvalidOperationException(
                    fieldName + " TimestampUs did not move strictly forward within the wrap-aware UInt64 window.");
            }
        }

        private static void EnsureD1UInt32Advanced(
            string fieldName,
            uint previous,
            uint current)
        {
            var forwardDelta = unchecked(current - previous);
            if (forwardDelta == 0 || forwardDelta > int.MaxValue)
            {
                throw new InvalidOperationException(
                    fieldName + " did not move strictly forward within the wrap-aware UInt32 window.");
            }
        }

        private static void ValidateCommon(
            IReadOnlyList<LMCSignalCatalogEntry> entries,
            uint expectedBulkId,
            uint expectedConfigRevision,
            uint expectedMapRevision,
            ushort expectedEntryStride,
            LMCBulkSnapshot snapshot,
            string stage)
        {
            ValidateCatalogTopology(entries);
            if (snapshot == null
                || snapshot.Response == null
                || !snapshot.Response.IsSuccess)
            {
                throw new InvalidOperationException(
                    stage + " response did not succeed.");
            }

            if (snapshot.BulkId != expectedBulkId
                || snapshot.ConfigRevision != expectedConfigRevision
                || snapshot.MapRevision != expectedMapRevision
                || snapshot.EntryCount != ExpectedSignalCount
                || snapshot.Entries.Count != ExpectedSignalCount
                || snapshot.EntryStride != expectedEntryStride)
            {
                throw new InvalidOperationException(
                    stage + " identity, entry count, or stride changed.");
            }

            var expectedFlags = LMCBulkSnapshotFlags.SameCycle
                | LMCBulkSnapshotFlags.InputMappedPhase;
            if (snapshot.CapturePhase != LMCCapturePhase.InputMapped
                || snapshot.SnapshotFlags != expectedFlags
                || (snapshot.SnapshotSequence & 1u) != 0)
            {
                throw new InvalidOperationException(
                    stage + " phase, flags, or even seqlock contract failed.");
            }

            for (var index = 0; index < entries.Count; index++)
            {
                if (snapshot.Entries[index].SignalId
                        != entries[index].SignalId
                    || snapshot.Entries[index].ValueType
                        != entries[index].DataType)
                {
                    throw new InvalidOperationException(
                        stage + " signal order or type changed at index "
                        + index
                        + ".");
                }
            }
        }
    }
}
