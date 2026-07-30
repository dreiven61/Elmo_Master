using System;
using System.Collections.Generic;
using System.IO;
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
            tests.Add(
                "Qualification.BulkPartial.D1BaselineFaultRecovery",
                D1BaselineFaultRecovery);
            tests.Add(
                "Qualification.BulkPartial.D1FaultValidRejected",
                D1FaultValidRejected);
            tests.Add(
                "Qualification.BulkPartial.D1BulkHealthAxisMismatchRejected",
                D1BulkHealthAxisMismatchRejected);
            tests.Add(
                "Qualification.BulkPartial.D1MultipleOfflineInconclusive",
                D1MultipleOfflineInconclusive);
            tests.Add(
                "Qualification.BulkPartial.D1RecoveryFreshnessRequired",
                D1RecoveryFreshnessRequired);
            tests.Add(
                "Qualification.BulkPartial.D1HealthyAxisErrorRejected",
                D1HealthyAxisErrorRejected);
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

            AssertEx.Throws<
                BulkPartialQualificationInconclusiveException>(
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
            AssertEx.Throws<InvalidDataException>(
                () => ParseSnapshot(
                    entries,
                    Enumerable.Range(6, 6).ToArray(),
                    true,
                    true));
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

        private static void D1BaselineFaultRecovery()
        {
            var entries = CreateCatalogEntries();
            var representatives = BulkPartialQualificationAnalysis
                .SelectRepresentativePIEntries(entries);
            AssertEx.Equal(4, representatives.Count);
            AssertEx.Equal(0x00100106u, representatives[0].SignalId);
            AssertEx.Equal(0x00100406u, representatives[3].SignalId);

            var baseline = CreateD1Stage(
                representatives,
                100,
                1000,
                new byte[0],
                new byte[0]);
            var fault = CreateD1Stage(
                representatives,
                200,
                2000,
                new byte[] { 2 },
                new byte[] { 2 });
            var recovery = CreateD1Stage(
                representatives,
                300,
                3000,
                new byte[0],
                new byte[0]);

            BulkPartialQualificationAnalysis.ValidateD1Baseline(
                MapRevision,
                representatives,
                baseline.Health,
                baseline.Values);
            var faultResult = BulkPartialQualificationAnalysis
                .ValidateD1Fault(
                    MapRevision,
                    representatives,
                    baseline.Health,
                    baseline.Values,
                    fault.Health,
                    fault.Values,
                    2);
            AssertEx.Equal((byte)2, faultResult.OfflineSourceIndex);
            AssertEx.Equal(representatives[1].SignalId, faultResult.SignalId);
            AssertEx.Equal(
                baseline.Values[1].RawValue32,
                faultResult.StaleRawValue);
            AssertEx.False(faultResult.FaultValueIsCurrent);

            BulkPartialQualificationAnalysis.ValidateD1Recovery(
                MapRevision,
                representatives,
                fault.Health,
                fault.Values,
                recovery.Health,
                recovery.Values,
                2);
        }

        private static void D1FaultValidRejected()
        {
            var representatives = BulkPartialQualificationAnalysis
                .SelectRepresentativePIEntries(CreateCatalogEntries());
            var baseline = CreateD1Stage(
                representatives,
                100,
                1000,
                new byte[0],
                new byte[0]);
            var faultWithCurrentPi = CreateD1Stage(
                representatives,
                200,
                2000,
                new byte[] { 2 },
                new byte[0]);

            AssertEx.Throws<InvalidOperationException>(
                () => BulkPartialQualificationAnalysis.ValidateD1Fault(
                    MapRevision,
                    representatives,
                    baseline.Health,
                    baseline.Values,
                    faultWithCurrentPi.Health,
                    faultWithCurrentPi.Values,
                    2));
        }

        private static void D1BulkHealthAxisMismatchRejected()
        {
            var representatives = BulkPartialQualificationAnalysis
                .SelectRepresentativePIEntries(CreateCatalogEntries());
            var baseline = CreateD1Stage(
                representatives,
                100,
                1000,
                new byte[0],
                new byte[0]);
            var fault = CreateD1Stage(
                representatives,
                200,
                2000,
                new byte[] { 3 },
                new byte[] { 3 });

            AssertEx.Throws<InvalidOperationException>(
                () => BulkPartialQualificationAnalysis.ValidateD1Fault(
                    MapRevision,
                    representatives,
                    baseline.Health,
                    baseline.Values,
                    fault.Health,
                    fault.Values,
                    2));
        }

        private static void D1MultipleOfflineInconclusive()
        {
            var representatives = BulkPartialQualificationAnalysis
                .SelectRepresentativePIEntries(CreateCatalogEntries());
            var baseline = CreateD1Stage(
                representatives,
                100,
                1000,
                new byte[0],
                new byte[0]);
            var fault = CreateD1Stage(
                representatives,
                200,
                2000,
                new byte[] { 2, 3 },
                new byte[] { 2, 3 });

            AssertEx.Throws<
                BulkPartialQualificationInconclusiveException>(
                () => BulkPartialQualificationAnalysis.ValidateD1Fault(
                    MapRevision,
                    representatives,
                    baseline.Health,
                    baseline.Values,
                    fault.Health,
                    fault.Values,
                    2));
        }

        private static void D1RecoveryFreshnessRequired()
        {
            var representatives = BulkPartialQualificationAnalysis
                .SelectRepresentativePIEntries(CreateCatalogEntries());
            var fault = CreateD1Stage(
                representatives,
                200,
                2000,
                new byte[] { 2 },
                new byte[] { 2 });
            var recovery = CreateD1Stage(
                representatives,
                300,
                3000,
                new byte[0],
                new byte[0]);
            recovery.Values[1] = ParsePI(
                representatives[1],
                fault.Values[1].CycleCounter,
                3002,
                LMCSignalEntryStatus.Valid,
                LMCDiagnosticsDetailCode.None);

            AssertEx.Throws<InvalidOperationException>(
                () => BulkPartialQualificationAnalysis.ValidateD1Recovery(
                    MapRevision,
                    representatives,
                    fault.Health,
                    fault.Values,
                    recovery.Health,
                    recovery.Values,
                    2));
        }

        private static void D1HealthyAxisErrorRejected()
        {
            var representatives = BulkPartialQualificationAnalysis
                .SelectRepresentativePIEntries(CreateCatalogEntries());
            var baseline = CreateD1Stage(
                representatives,
                100,
                1000,
                new byte[0],
                new byte[0],
                new byte[] { 3 });

            AssertEx.Throws<InvalidOperationException>(
                () => BulkPartialQualificationAnalysis.ValidateD1Baseline(
                    MapRevision,
                    representatives,
                    baseline.Health,
                    baseline.Values));
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

        private static D1Stage CreateD1Stage(
            IReadOnlyList<LMCSignalCatalogEntry> representatives,
            uint cycleCounter,
            ulong timestampUs,
            ICollection<byte> healthOfflineAxes,
            ICollection<byte> piOfflineAxes,
            ICollection<byte> axisErrorAxes = null)
        {
            var values = new List<LMCSignalValue>(representatives.Count);
            for (var index = 0; index < representatives.Count; index++)
            {
                var sourceIndex = checked((byte)(index + 1));
                var offline = piOfflineAxes.Contains(sourceIndex);
                values.Add(
                    ParsePI(
                        representatives[index],
                        cycleCounter + sourceIndex,
                        timestampUs + sourceIndex,
                        offline
                            ? LMCSignalEntryStatus.SlaveOffline
                                | LMCSignalEntryStatus
                                    .SlaveNotOperational
                            : LMCSignalEntryStatus.Valid,
                        offline
                            ? LMCDiagnosticsDetailCode.SlaveOffline
                            : LMCDiagnosticsDetailCode.None));
            }

            return new D1Stage(
                ParseHealth(
                    cycleCounter,
                    timestampUs,
                    healthOfflineAxes,
                    axisErrorAxes ?? new byte[0]),
                values);
        }

        private static LMCEtherCATHealth ParseHealth(
            uint cycleCounter,
            ulong timestampUs,
            ICollection<byte> offlineAxes,
            ICollection<byte> axisErrorAxes)
        {
            const ushort healthHeaderLength = 72;
            const ushort healthEntryStride = 32;
            var payload = new byte[
                healthHeaderLength + 4 * healthEntryStride];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, RequestId);
            TestFrame.WriteUInt32(payload, 16, MapRevision);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt16(payload, 22, 4);
            TestFrame.WriteUInt32(payload, 24, cycleCounter);
            TestFrame.WriteUInt32(payload, 28, (uint)timestampUs);
            TestFrame.WriteUInt32(payload, 32, (uint)(timestampUs >> 32));
            TestFrame.WriteUInt16(payload, 36, 8);
            TestFrame.WriteUInt16(
                payload,
                38,
                (ushort)LMCEtherCATMasterFlags.MasterOperational);
            TestFrame.WriteUInt32(payload, 44, 7);
            TestFrame.WriteUInt32(payload, 48, 150);
            TestFrame.WriteUInt32(payload, 52, 250);
            TestFrame.WriteUInt32(payload, 56, 300);
            TestFrame.WriteUInt32(payload, 60, 450);
            TestFrame.WriteUInt32(payload, 64, cycleCounter * 2);
            TestFrame.WriteUInt16(payload, 68, healthEntryStride);

            for (ushort index = 0; index < 4; index++)
            {
                var sourceIndex = checked((byte)(index + 1));
                var offline = offlineAxes.Contains(sourceIndex);
                var offset = healthHeaderLength + index * healthEntryStride;
                TestFrame.WriteUInt16(payload, offset, index);
                TestFrame.WriteUInt16(payload, offset + 2, sourceIndex);
                payload[offset + 4] = offline ? (byte)0 : (byte)1;
                payload[offset + 5] = offline ? (byte)0 : (byte)8;
                TestFrame.WriteUInt32(
                    payload,
                    offset + 8,
                    offline ? 0u : 8u);
                TestFrame.WriteUInt32(
                    payload,
                    offset + 12,
                    offline ? uint.MaxValue : 1u);
                TestFrame.WriteUInt32(payload, offset + 16, 0x1237);
                TestFrame.WriteUInt32(
                    payload,
                    offset + 20,
                    axisErrorAxes.Contains(sourceIndex) ? 0x1234u : 0u);
                TestFrame.WriteUInt32(
                    payload,
                    offset + 24,
                    offline ? cycleCounter - 20 : cycleCounter);
                TestFrame.WriteUInt32(
                    payload,
                    offset + 28,
                    offline ? cycleCounter - 10 : cycleCounter - 50);
            }

            return LMC_DiagnosticsParser.ParseEtherCATHealth(
                TestFrame.Response(0, payload),
                RequestId);
        }

        private static LMCSignalValue ParsePI(
            LMCSignalCatalogEntry representative,
            uint cycleCounter,
            ulong timestampUs,
            LMCSignalEntryStatus status,
            LMCDiagnosticsDetailCode detail)
        {
            var payload = new byte[52];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, RequestId);
            TestFrame.WriteUInt32(payload, 16, MapRevision);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt32(payload, 24, cycleCounter);
            TestFrame.WriteUInt32(payload, 28, (uint)timestampUs);
            TestFrame.WriteUInt32(payload, 32, (uint)(timestampUs >> 32));
            TestFrame.WriteUInt32(payload, 36, representative.SignalId);
            TestFrame.WriteUInt32(
                payload,
                40,
                0x1200u + representative.SourceIndex);
            payload[44] = (byte)representative.DataType;
            payload[45] = (byte)status;
            TestFrame.WriteUInt32(payload, 48, (uint)detail);
            return LMC_DiagnosticsParser.ParsePI(
                TestFrame.Response(0, payload),
                RequestId,
                MapRevision,
                representative.SignalId,
                representative.DataType);
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

        private sealed class D1Stage
        {
            internal D1Stage(
                LMCEtherCATHealth health,
                List<LMCSignalValue> values)
            {
                Health = health;
                Values = values;
            }

            internal LMCEtherCATHealth Health { get; private set; }
            internal List<LMCSignalValue> Values { get; private set; }
        }
    }
}
