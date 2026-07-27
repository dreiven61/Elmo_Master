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

    internal static class BulkPartialQualificationAnalysis
    {
        private const int ExpectedSignalCount = 24;
        private const int ExpectedAxisCount = 4;
        private const int ExpectedSignalsPerAxis = 6;

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
                    throw new InvalidOperationException(
                        "Partial Bulk entries span more than one physical axis.");
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
