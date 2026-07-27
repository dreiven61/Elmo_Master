using System;
using System.Collections.Generic;
using System.Linq;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class BulkPartialQualificationAnalysisTests
    {
        private const uint RequestId = 0x11223344u;
        private const uint BulkId = 0xA1B2C3D4u;
        private const uint ConfigRevision = 0x01020304u;
        private const uint MapRevision = 0x957F101Eu;
        private const ushort EntryStride = 16;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.BulkPartial.BaselineAndRecovery",
                BaselineAndRecovery);
            tests.Add(
                "Qualification.BulkPartial.OneAxisExact",
                OneAxisExact);
            tests.Add(
                "Qualification.BulkPartial.CompositeOfflineFlagsAccepted",
                CompositeOfflineFlagsAccepted);
            tests.Add(
                "Qualification.BulkPartial.ScatteredAxesRejected",
                ScatteredAxesRejected);
            tests.Add(
                "Qualification.BulkPartial.WrongDetailRejected",
                WrongDetailRejected);
            tests.Add(
                "Qualification.BulkPartial.FiveOfflineRejected",
                FiveOfflineRejected);
            tests.Add(
                "Qualification.BulkPartial.RecoveryTransitionAccepted",
                RecoveryTransitionAccepted);
            tests.Add(
                "Qualification.BulkPartial.TopologyMismatchRejected",
                TopologyMismatchRejected);
            tests.Add(
                "Qualification.BulkPartial.CatalogContractMismatchRejected",
                CatalogContractMismatchRejected);
        }

        private static void BaselineAndRecovery()
        {
            var entries = CreateCatalogEntries();
            var snapshot = ParseSnapshot(entries, new int[0], false, false);

            BulkPartialQualificationAnalysis.ValidateBaseline(
                entries,
                BulkId,
                ConfigRevision,
                MapRevision,
                EntryStride,
                snapshot,
                "baseline");
            BulkPartialQualificationAnalysis.ValidateBaseline(
                entries,
                BulkId,
                ConfigRevision,
                MapRevision,
                EntryStride,
                snapshot,
                "recovery");
        }

        private static void OneAxisExact()
        {
            var entries = CreateCatalogEntries();
            var offline = Enumerable.Range(12, 6).ToArray();
            var snapshot = ParseSnapshot(entries, offline, true, false);

            var result = BulkPartialQualificationAnalysis
                .ValidateOneSlaveOffline(
                    entries,
                    BulkId,
                    ConfigRevision,
                    MapRevision,
                    EntryStride,
                    snapshot);

            AssertEx.Equal((byte)3, result.OfflineSourceIndex);
            AssertEx.Equal(6, result.InvalidEntryCount);
            AssertEx.Equal(18, result.ValidEntryCount);
        }

        private static void ScatteredAxesRejected()
        {
            var entries = CreateCatalogEntries();
            var snapshot = ParseSnapshot(
                entries,
                new[] { 0, 1, 2, 6, 7, 8 },
                true,
                false);

            AssertEx.Throws<InvalidOperationException>(
                () => BulkPartialQualificationAnalysis
                    .ValidateOneSlaveOffline(
                        entries,
                        BulkId,
                        ConfigRevision,
                        MapRevision,
                        EntryStride,
                        snapshot));
        }

        private static void CompositeOfflineFlagsAccepted()
        {
            var entries = CreateCatalogEntries();
            var offline = Enumerable.Range(6, 6).ToArray();
            var compositeStatus = LMCSignalEntryStatus.SlaveOffline
                | LMCSignalEntryStatus.SlaveNotOperational
                | LMCSignalEntryStatus.AlError;
            var snapshot = ParseSnapshot(
                entries,
                offline,
                true,
                false,
                compositeStatus,
                LMCDiagnosticsDetailCode.SlaveOffline);

            var result = BulkPartialQualificationAnalysis
                .ValidateOneSlaveOffline(
                    entries,
                    BulkId,
                    ConfigRevision,
                    MapRevision,
                    EntryStride,
                    snapshot);

            AssertEx.Equal((byte)2, result.OfflineSourceIndex);
            AssertEx.Equal(6, result.InvalidEntryCount);
            AssertEx.Equal(18, result.ValidEntryCount);
        }

        private static void WrongDetailRejected()
        {
            var entries = CreateCatalogEntries();
            var snapshot = ParseSnapshot(
                entries,
                Enumerable.Range(6, 6).ToArray(),
                true,
                true);

            AssertEx.Throws<InvalidOperationException>(
                () => BulkPartialQualificationAnalysis
                    .ValidateOneSlaveOffline(
                        entries,
                        BulkId,
                        ConfigRevision,
                        MapRevision,
                        EntryStride,
                        snapshot));
        }

        private static void FiveOfflineRejected()
        {
            var entries = CreateCatalogEntries();
            var snapshot = ParseSnapshot(
                entries,
                Enumerable.Range(18, 5).ToArray(),
                true,
                false);

            AssertEx.Throws<InvalidOperationException>(
                () => BulkPartialQualificationAnalysis
                    .ValidateOneSlaveOffline(
                        entries,
                        BulkId,
                        ConfigRevision,
                        MapRevision,
                        EntryStride,
                        snapshot));
        }

        private static void RecoveryTransitionAccepted()
        {
            var entries = CreateCatalogEntries();
            var snapshot = ParseSnapshot(
                entries,
                Enumerable.Range(18, 6).ToArray(),
                true,
                false,
                LMCSignalEntryStatus.SlaveNotOperational,
                LMCDiagnosticsDetailCode.NotReady);

            BulkPartialQualificationAnalysis.ValidateRecoveryPending(
                entries,
                BulkId,
                ConfigRevision,
                MapRevision,
                EntryStride,
                snapshot,
                4);
        }

        private static void TopologyMismatchRejected()
        {
            var entries = CreateCatalogEntries();
            entries[0] = CreateCatalogEntry(1, 2);

            AssertEx.Throws<InvalidOperationException>(
                () => BulkPartialQualificationAnalysis
                    .ValidateCatalogTopology(entries));
        }

        private static void CatalogContractMismatchRejected()
        {
            var entries = CreateCatalogEntries();
            entries[0] = CreateCatalogEntry(1, 1, true);

            AssertEx.Throws<InvalidOperationException>(
                () => BulkPartialQualificationAnalysis
                    .ValidateCatalogTopology(entries));
        }

        private static List<LMCSignalCatalogEntry> CreateCatalogEntries()
        {
            var entries = new List<LMCSignalCatalogEntry>();
            for (byte axis = 1; axis <= 4; axis++)
            {
                for (byte signalCode = 1; signalCode <= 6; signalCode++)
                {
                    entries.Add(CreateCatalogEntry(axis, signalCode));
                }
            }

            return entries;
        }

        private static LMCSignalCatalogEntry CreateCatalogEntry(
            byte axis,
            byte signalCode,
            bool wrongDirection = false)
        {
            var signalId = 0x00100000u
                | ((uint)axis << 8)
                | signalCode;
            var valueType = signalCode == 1 || signalCode == 4
                ? LMCSignalValueType.Int32
                : signalCode == 3 || signalCode == 6
                    ? LMCSignalValueType.BitField16
                    : LMCSignalValueType.BitField32;
            var byteWidth = valueType == LMCSignalValueType.BitField16
                ? (byte)2
                : (byte)4;
            var outputSignal = signalCode <= 3;
            var direction = outputSignal
                ? LMCPdoDirection.MasterToDrive
                : LMCPdoDirection.DriveToMaster;
            if (wrongDirection)
            {
                direction = direction == LMCPdoDirection.MasterToDrive
                    ? LMCPdoDirection.DriveToMaster
                    : LMCPdoDirection.MasterToDrive;
            }

            var pdoIndex = signalCode == 1
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
            return new LMCSignalCatalogEntry(
                signalId,
                checked((ushort)((axis - 1) * 6 + signalCode - 1)),
                outputSignal
                    ? LMCSignalSourceKind.PdoOutputLastTx
                    : LMCSignalSourceKind.PdoInput,
                axis,
                valueType,
                byteWidth,
                0,
                LMCSignalAccessFlags.Readable
                    | LMCSignalAccessFlags.Recordable
                    | LMCSignalAccessFlags.BulkReadable,
                LMCSignalFlags.ActivePdo
                    | LMCSignalFlags.PhysicalAxis
                    | LMCSignalFlags.InputMappedPhase,
                pdoIndex,
                signalCode == 2 ? (byte)1 : (byte)0,
                direction,
                1,
                1,
                int.MinValue,
                int.MaxValue,
                "axis" + axis + ".signal" + signalCode);
        }

        private static LMCBulkSnapshot ParseSnapshot(
            IReadOnlyList<LMCSignalCatalogEntry> entries,
            ICollection<int> offlineIndexes,
            bool partial,
            bool wrongFirstOfflineDetail,
            LMCSignalEntryStatus offlineStatus =
                LMCSignalEntryStatus.SlaveOffline,
            LMCDiagnosticsDetailCode offlineDetail =
                LMCDiagnosticsDetailCode.SlaveOffline)
        {
            var payload = new byte[56 + entries.Count * EntryStride];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt16(
                payload,
                2,
                partial
                    ? (ushort)LMCDiagnosticsResponseFlags.Partial
                    : (ushort)LMCDiagnosticsResponseFlags.None);
            TestFrame.WriteUInt32(payload, 8, RequestId);
            TestFrame.WriteUInt32(payload, 16, BulkId);
            TestFrame.WriteUInt32(payload, 20, ConfigRevision);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 28, 100);
            TestFrame.WriteUInt32(payload, 32, 1);
            TestFrame.WriteUInt32(payload, 36, 2);
            TestFrame.WriteUInt16(payload, 40, checked((ushort)entries.Count));
            TestFrame.WriteUInt16(payload, 42, EntryStride);
            payload[44] = (byte)LMCCapturePhase.InputMapped;
            TestFrame.WriteUInt32(payload, 48, 10);
            TestFrame.WriteUInt32(
                payload,
                52,
                (uint)(LMCBulkSnapshotFlags.SameCycle
                    | LMCBulkSnapshotFlags.InputMappedPhase));

            var firstOffline = true;
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                var offline = offlineIndexes.Contains(index);
                var offset = 56 + index * EntryStride;
                TestFrame.WriteUInt32(payload, offset, entry.SignalId);
                TestFrame.WriteUInt32(payload, offset + 4, (uint)(1000 + index));
                payload[offset + 8] = (byte)entry.DataType;
                payload[offset + 9] = offline
                    ? (byte)offlineStatus
                    : (byte)LMCSignalEntryStatus.Valid;
                var detail = offline
                    ? offlineDetail
                    : LMCDiagnosticsDetailCode.None;
                if (offline && firstOffline && wrongFirstOfflineDetail)
                {
                    detail = LMCDiagnosticsDetailCode.NotReady;
                }

                TestFrame.WriteUInt32(payload, offset + 12, (uint)detail);
                if (offline)
                {
                    firstOffline = false;
                }
            }

            return LMC_DiagnosticsParser.ParseBulkSnapshot(
                TestFrame.Response(0, payload),
                RequestId,
                BulkId,
                ConfigRevision,
                MapRevision,
                entries.Select(entry => entry.SignalId).ToArray());
        }
    }
}
