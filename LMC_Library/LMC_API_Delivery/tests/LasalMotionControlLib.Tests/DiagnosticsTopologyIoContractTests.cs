using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsTopologyIoContractTests
    {
        private const uint RequestId = 0x11223344u;
        private const uint TopologyRevision = 0xA1B2C3D4u;
        private const uint LasalTopologyRevision = 0x15867EECu;
        private const ushort LasalTopologyNodeCount = 7;
        private const uint NodeId = 0x00000101u;
        private const uint IOReference = 0x00000501u;
        private const uint DiagnosticsBootId = 0x10203040u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Request.DiagnosticsTopologyIo.GoldenBytes",
                RequestGoldenBytes);
            tests.Add(
                "Response.EtherCATTopology.GoldenFields",
                TopologyGoldenFields);
            tests.Add(
                "Response.EtherCATTopology.LasalSevenNodeGolden",
                LasalSevenNodeTopologyGolden);
            tests.Add(
                "Response.EtherCATTopology.MalformedRejected",
                TopologyMalformedRejected);
            tests.Add(
                "Response.EtherCATNodeHealth.GoldenAndMalformed",
                NodeHealthGoldenAndMalformed);
            tests.Add(
                "Response.DigitalIO.GoldenAndMalformed",
                DigitalIOGoldenAndMalformed);
            tests.Add(
                "Contract.DigitalOutputWrite.TicketAndPolicy",
                DigitalOutputWriteTicketAndPolicy);
            tests.Add(
                "Contract.DigitalOutputWrite.SubmissionTracker",
                DigitalOutputWriteSubmissionTracker);
            tests.Add(
                "Contract.DigitalOutputWrite.NullFailureContext",
                DigitalOutputWriteNullFailureContext);
            tests.Add(
                "Rpc.DigitalOutputWrite.DetachedRequestPreWire",
                DigitalOutputWriteDetachedRequestPreWire);
            tests.Add(
                "Rpc.DigitalOutputWrite.RawSnapshotRejectedPreWire",
                DigitalOutputWriteRawSnapshotRejectedPreWire);
            tests.Add(
                "Rpc.DigitalOutputWrite.BootIdentityPreWire",
                DigitalOutputWriteBootIdentityPreWire);
            tests.Add(
                "Rpc.DiagnosticsTopologyIo.CapabilityOffPreWire",
                CapabilityOffPreWire);
            tests.Add(
                "Rpc.DiagnosticsTopologyIo.DownloadAndRead",
                TopologyDownloadAndDigitalIORead);
            tests.Add(
                "Rpc.EtherCATNodeHealth.SyncAndAsync",
                EtherCATNodeHealthSyncAndAsync);
            tests.Add(
                "Rpc.DiagnosticsTopologyIo.ReadFacadePreWireGuards",
                ReadFacadePreWireGuards);
            tests.Add(
                "Rpc.DiagnosticsTopologyIo.PinnedHealthSingleRead",
                PinnedHealthSingleRead);
            tests.Add(
                "Rpc.DiagnosticsTopologyIo.PinnedDigitalInputSingleRead",
                PinnedDigitalInputSingleRead);
            tests.Add(
                "Rpc.DiagnosticsTopologyIo.PinnedSnapshotPreWireGuards",
                PinnedSnapshotPreWireGuards);
            tests.Add(
                "Rpc.DiagnosticsTopologyIo.LasalSevenNodeDownload",
                LasalSevenNodeTopologyDownload);
            tests.Add(
                "Rpc.DigitalOutputWrite.EmptyAllowlistPreWire",
                DigitalOutputWriteEmptyAllowlistPreWire);
            tests.Add(
                "Rpc.DigitalOutputWrite.CoreWireAccepted",
                DigitalOutputWriteCoreWireAccepted);
            tests.Add(
                "Rpc.DigitalOutputWrite.ExplicitRejectionContext",
                DigitalOutputWriteExplicitRejectionContext);
            tests.Add(
                "Rpc.DigitalOutputWrite.OutcomeUncertainContext",
                DigitalOutputWriteOutcomeUncertainContext);
            tests.Add(
                "Rpc.DigitalOutputWrite.AcceptedSessionRaceContext",
                DigitalOutputWriteAcceptedSessionRaceContext);
        }

        private static void RequestGoldenBytes()
        {
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "11 7E 00 00 08 00 00 00 "
                    + "01 00 00 00 44 33 22 11"),
                LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(RequestId));

            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "12 7E 00 00 10 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "D4 C3 B2 A1 10 00 08 00"),
                LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                    RequestId,
                    TopologyRevision,
                    16,
                    8));

            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "13 7E 00 00 10 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "D4 C3 B2 A1 01 01 00 00"),
                LMC_DiagnosticsFrame.ReadEtherCATNodeHealth(
                    RequestId,
                    TopologyRevision,
                    NodeId));

            var readRequest = new LMCDigitalIOReadRequest(
                TopologyRevision,
                IOReference,
                LMCDigitalIODirection.Output,
                64);
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "22 7E 00 00 14 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "D4 C3 B2 A1 01 05 00 00 "
                    + "02 40 00 00"),
                LMC_DiagnosticsFrame.ReadDigitalIO(RequestId, readRequest));

            var writeRequest = new LMCDigitalOutputWriteRequest(
                TopologyRevision,
                IOReference,
                0x1122004455007788UL,
                0xFFFF00FFFF00FFFFUL,
                0x0A0B0C0Du);
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "23 7E 00 00 28 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "D4 C3 B2 A1 01 05 00 00 "
                    + "88 77 00 55 44 00 22 11 "
                    + "FF FF 00 FF FF 00 FF FF "
                    + "0D 0C 0B 0A 40 30 20 10"),
                LMC_DiagnosticsFrame.SubmitDigitalOutputWrite(
                    RequestId,
                    writeRequest,
                    DiagnosticsBootId));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                    RequestId,
                    0,
                    0,
                    1));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                    RequestId,
                    TopologyRevision,
                    0,
                    17));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.SubmitDigitalOutputWrite(
                    RequestId,
                    writeRequest,
                    0));
        }

        private static void TopologyGoldenFields()
        {
            var info = LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                TestFrame.Response(0, TopologyInfoPayload(RequestId)),
                RequestId);

            AssertEx.Equal(TopologyRevision, info.TopologyRevision);
            AssertEx.Equal((ushort)2, info.TotalNodeCount);
            AssertEx.Equal((ushort)96, info.EntryStride);
            AssertEx.Equal((ushort)16, info.MaxEntriesPerChunk);
            AssertEx.Equal((ushort)1, info.ConfiguredSlaveCount);
            AssertEx.Equal((ushort)1, info.SlotModuleCount);
            AssertEx.Equal((ushort)1, info.PhysicalAxisCount);
            AssertEx.Equal(
                LMCEtherCATTopologyFlags.FixedStride
                    | LMCEtherCATTopologyFlags.NameAscii7Bit
                    | LMCEtherCATTopologyFlags.CanonicalCrc
                    | LMCEtherCATTopologyFlags.OpaqueNodeId,
                info.TopologyFlags);

            var chunk = LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                TestFrame.Response(
                    0,
                    TopologyChunkPayload(RequestId)),
                RequestId,
                TopologyRevision,
                0,
                2);

            AssertEx.Equal((ushort)2, chunk.ReturnedCount);
            AssertEx.Equal((ushort)2, chunk.TotalNodeCount);
            AssertEx.Equal((ushort)0, chunk.Entries[0].MasterSlaveIndex);
            AssertEx.True(chunk.Entries[0].HasMasterSlaveIndex);
            AssertEx.Equal(ushort.MaxValue, chunk.Entries[0].SlotIndex);
            AssertEx.Equal(IOReference, chunk.Entries[0].IOReference);
            AssertEx.Equal("axis-io-slave", chunk.Entries[0].Name);
            AssertEx.Equal(
                LMCEtherCATTopologyNodeKind.SlotModule,
                chunk.Entries[1].NodeKind);
            AssertEx.Equal(ushort.MaxValue, chunk.Entries[1].MasterSlaveIndex);
            AssertEx.False(chunk.Entries[1].HasMasterSlaveIndex);
            AssertEx.Equal((ushort)0, chunk.Entries[1].SlotIndex);
            AssertEx.Equal(0x00000502u, chunk.Entries[1].IOReference);
            AssertEx.True(
                LMC_DiagnosticsParser.ComputeEtherCATTopologyRevision(
                    chunk.Entries) != 0);
        }

        private static void LasalSevenNodeTopologyGolden()
        {
            var expectedEntries = LasalTopologyEntries();
            var canonical = LasalTopologyCanonicalBytes(expectedEntries);
            AssertLasalTopologyCanonicalBytes(canonical, expectedEntries);
            AssertEx.Equal(
                LasalTopologyRevision,
                LMC_DiagnosticsParser.ComputeEtherCATTopologyRevision(
                    expectedEntries));

            var info = LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                TestFrame.Response(0, LasalTopologyInfoPayload(RequestId)),
                RequestId);
            AssertEx.Equal(LasalTopologyRevision, info.TopologyRevision);
            AssertEx.Equal(LasalTopologyNodeCount, info.TotalNodeCount);
            AssertEx.Equal((ushort)96, info.EntryStride);
            AssertEx.Equal((ushort)1, info.MaxEntriesPerChunk);
            AssertEx.Equal((ushort)5, info.ConfiguredSlaveCount);
            AssertEx.Equal((ushort)2, info.SlotModuleCount);
            AssertEx.Equal((ushort)4, info.PhysicalAxisCount);
            AssertEx.Equal(0x0000000Fu, info.TopologyFlagsValue);
            AssertEx.Equal(1u, info.CrcKindValue);

            var parsedEntries = new List<LMCEtherCATTopologyEntry>(
                LasalTopologyNodeCount);
            for (ushort index = 0; index < LasalTopologyNodeCount; index++)
            {
                var requestId = checked(RequestId + index);
                var payload = LasalTopologyChunkPayload(
                    requestId,
                    index,
                    canonical);
                AssertEx.Equal(124, payload.Length);
                AssertEx.Equal(
                    index == LasalTopologyNodeCount - 1
                        ? (ushort)LMCDiagnosticsResponseFlags.LastChunk
                        : (ushort)LMCDiagnosticsResponseFlags.None,
                    TestFrame.ReadUInt16(payload, 2));

                var chunk = LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                    TestFrame.Response(0, payload),
                    requestId,
                    LasalTopologyRevision,
                    index,
                    1);
                AssertEx.Equal(index, chunk.StartIndex);
                AssertEx.Equal((ushort)1, chunk.ReturnedCount);
                AssertEx.Equal(LasalTopologyNodeCount, chunk.TotalNodeCount);
                AssertLasalTopologyEntry(
                    expectedEntries[index],
                    chunk.Entries[0]);
                parsedEntries.Add(chunk.Entries[0]);
            }

            AssertEx.Equal(
                LasalTopologyRevision,
                LMC_DiagnosticsParser.ComputeEtherCATTopologyRevision(
                    parsedEntries));
        }

        private static void LasalSevenNodeTopologyDownload()
        {
            RunLasalSevenNodeTopologyDownload(false);
            RunLasalSevenNodeTopologyDownload(true);
        }

        private static void TopologyMalformedRejected()
        {
            var wrongInfoFlags = TopologyInfoPayload(RequestId);
            TestFrame.WriteUInt32(wrongInfoFlags, 32, 0x00000007u);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                    TestFrame.Response(0, wrongInfoFlags),
                    RequestId));

            var wrongInfoRevision = TopologyInfoPayload(RequestId);
            TestFrame.WriteUInt32(wrongInfoRevision, 16, 0);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                    TestFrame.Response(0, wrongInfoRevision),
                    RequestId));

            var wrongSlaveSentinel = TopologyChunkPayload(RequestId);
            TestFrame.WriteUInt16(wrongSlaveSentinel, 28 + 10, ushort.MaxValue);
            AssertMalformedTopologyChunk(wrongSlaveSentinel);

            var wrongModuleSentinel = TopologyChunkPayload(RequestId);
            TestFrame.WriteUInt16(
                wrongModuleSentinel,
                28 + 96 + 10,
                0);
            AssertMalformedTopologyChunk(wrongModuleSentinel);

            var wrongIOReference = TopologyChunkPayload(RequestId);
            TestFrame.WriteUInt32(wrongIOReference, 28 + 92, 0);
            AssertMalformedTopologyChunk(wrongIOReference);

            var oversizedDigitalIO = TopologyChunkPayload(RequestId);
            TestFrame.WriteUInt16(oversizedDigitalIO, 28 + 40, 9);
            AssertMalformedTopologyChunk(oversizedDigitalIO);

            var digitalIOWithoutDirection = TopologyChunkPayload(RequestId);
            TestFrame.WriteUInt16(
                digitalIOWithoutDirection,
                28 + 14,
                (ushort)(LMCEtherCATTopologyNodeFlags.HasMasterSlaveIndex
                    | LMCEtherCATTopologyNodeFlags.SupportsSdo
                    | LMCEtherCATTopologyNodeFlags.PhysicalAxis
                    | LMCEtherCATTopologyNodeFlags.Ds402Drive
                    | LMCEtherCATTopologyNodeFlags.HasDigitalIO));
            TestFrame.WriteUInt16(digitalIOWithoutDirection, 28 + 40, 0);
            TestFrame.WriteUInt16(digitalIOWithoutDirection, 28 + 42, 0);
            AssertMalformedTopologyChunk(digitalIOWithoutDirection);

            var wrongEntryIndex = TopologyChunkPayload(RequestId);
            TestFrame.WriteUInt16(wrongEntryIndex, 28 + 96 + 8, 7);
            AssertMalformedTopologyChunk(wrongEntryIndex);

            var missingLastFlag = TopologyChunkPayload(RequestId);
            TestFrame.WriteUInt16(missingLastFlag, 2, 0);
            AssertMalformedTopologyChunk(missingLastFlag);

            AssertCompleteTopologyAmbiguitiesRejected();
        }

        private static void AssertCompleteTopologyAmbiguitiesRejected()
        {
            var baseChunk = ParseTopologyChunk(
                TopologyChunkPayload(RequestId),
                2);
            var baseInfo = LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                TestFrame.Response(0, TopologyInfoPayload(RequestId)),
                RequestId);
            var childBeforeParent = new List<LMCEtherCATTopologyEntry>
            {
                CloneTopologyEntry(baseChunk.Entries[1], 0),
                CloneTopologyEntry(baseChunk.Entries[0], 1)
            };
            AssertEx.Throws<InvalidDataException>(
                () => LMCDiagnostics.ValidateCompleteTopology(
                    baseInfo,
                    childBeforeParent));

            var twoSlaveInfoPayload = TopologyInfoPayload(RequestId);
            TestFrame.WriteUInt16(twoSlaveInfoPayload, 26, 2);
            TestFrame.WriteUInt16(twoSlaveInfoPayload, 28, 0);
            TestFrame.WriteUInt16(twoSlaveInfoPayload, 30, 2);
            var twoSlaveInfo = LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                TestFrame.Response(0, twoSlaveInfoPayload),
                RequestId);

            var duplicateMasterPayload = TopologyChunkPayload(RequestId);
            ConvertSecondEntryToSlave(duplicateMasterPayload, 0, 2, 2);
            var duplicateMasterChunk = ParseTopologyChunk(
                duplicateMasterPayload,
                2);
            AssertEx.Throws<InvalidDataException>(
                () => LMCDiagnostics.ValidateCompleteTopology(
                    twoSlaveInfo,
                    duplicateMasterChunk.Entries));

            var duplicateAxisPayload = TopologyChunkPayload(RequestId);
            ConvertSecondEntryToSlave(duplicateAxisPayload, 1, 2, 1);
            var duplicateAxisChunk = ParseTopologyChunk(
                duplicateAxisPayload,
                2);
            AssertEx.Throws<InvalidDataException>(
                () => LMCDiagnostics.ValidateCompleteTopology(
                    twoSlaveInfo,
                    duplicateAxisChunk.Entries));

            var duplicateSdoPayload = TopologyChunkPayload(RequestId);
            ConvertSecondEntryToSlave(duplicateSdoPayload, 1, 1, 2);
            var duplicateSdoChunk = ParseTopologyChunk(
                duplicateSdoPayload,
                2);
            AssertEx.Throws<InvalidDataException>(
                () => LMCDiagnostics.ValidateCompleteTopology(
                    twoSlaveInfo,
                    duplicateSdoChunk.Entries));

            var twoSlotInfoPayload = TopologyInfoPayload(RequestId);
            TestFrame.WriteUInt16(twoSlotInfoPayload, 20, 3);
            TestFrame.WriteUInt16(twoSlotInfoPayload, 28, 2);
            var twoSlotInfo = LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                TestFrame.Response(0, twoSlotInfoPayload),
                RequestId);
            var duplicateSlots = new List<LMCEtherCATTopologyEntry>(
                baseChunk.Entries)
            {
                new LMCEtherCATTopologyEntry(
                    0x00000103u,
                    NodeId,
                    2,
                    ushort.MaxValue,
                    LMCEtherCATTopologyNodeKind.SlotModule,
                    LMCEtherCATTopologyNodeFlags.HasInputs
                        | LMCEtherCATTopologyNodeFlags.HasDigitalIO,
                    0,
                    0,
                    0,
                    669,
                    1196692218,
                    65536,
                    0,
                    2,
                    0,
                    "duplicate-slot",
                    0x00000503u)
            };
            AssertEx.Throws<InvalidDataException>(
                () => LMCDiagnostics.ValidateCompleteTopology(
                    twoSlotInfo,
                    duplicateSlots));
        }

        private static void NodeHealthGoldenAndMalformed()
        {
            var health = LMC_DiagnosticsParser.ParseEtherCATNodeHealth(
                TestFrame.Response(0, NodeHealthPayload(RequestId)),
                RequestId,
                TopologyRevision,
                NodeId);

            AssertEx.Equal(TopologyRevision, health.TopologyRevision);
            AssertEx.Equal(NodeId, health.NodeId);
            AssertEx.Equal(LMCCapturePhase.InputMapped, health.CapturePhase);
            AssertEx.True(health.Online);
            AssertEx.Equal((byte)8, health.EtherCATState);
            AssertEx.Equal(0x1122334455667788UL, health.TimestampMicroseconds);
            AssertEx.Equal((uint)2, health.SnapshotSequence);
            AssertEx.True(
                (health.HealthFlags
                    & LMCEtherCATNodeHealthFlags.DataValid) != 0);
            AssertEx.False(
                (health.HealthFlags
                    & LMCEtherCATNodeHealthFlags.DataDefaulted) != 0);
            AssertEx.Equal(0x1234u, health.DS402StatusWord);

            var offlineDefaultedPayload = NodeHealthPayload(RequestId);
            TestFrame.WriteUInt16(
                offlineDefaultedPayload,
                26,
                (ushort)(LMCEtherCATNodeHealthFlags.Configured
                    | LMCEtherCATNodeHealthFlags.DataDefaulted));
            offlineDefaultedPayload[44] = 0;
            offlineDefaultedPayload[45] = 0;
            TestFrame.WriteUInt32(offlineDefaultedPayload, 56, 0);
            TestFrame.WriteUInt32(offlineDefaultedPayload, 60, 0);
            var offlineDefaulted =
                LMC_DiagnosticsParser.ParseEtherCATNodeHealth(
                    TestFrame.Response(0, offlineDefaultedPayload),
                    RequestId,
                    TopologyRevision,
                    NodeId);
            AssertEx.False(offlineDefaulted.Online);
            AssertEx.True(
                (offlineDefaulted.HealthFlags
                    & LMCEtherCATNodeHealthFlags.DataDefaulted) != 0);

            var detectedButOffline = NodeHealthPayload(RequestId);
            detectedButOffline[44] = 0;
            AssertMalformedNodeHealth(detectedButOffline);

            var detectedWithoutState = NodeHealthPayload(RequestId);
            detectedWithoutState[45] = 0;
            AssertMalformedNodeHealth(detectedWithoutState);

            var notConfigured = NodeHealthPayload(RequestId);
            TestFrame.WriteUInt16(
                notConfigured,
                26,
                (ushort)(LMCEtherCATNodeHealthFlags.Detected
                    | LMCEtherCATNodeHealthFlags.IdentityMatched
                    | LMCEtherCATNodeHealthFlags.DataValid
                    | LMCEtherCATNodeHealthFlags.Ds402DataPresent));
            AssertMalformedNodeHealth(notConfigured);

            var validAndDefaulted = NodeHealthPayload(RequestId);
            TestFrame.WriteUInt16(validAndDefaulted, 26, 0x003F);
            AssertMalformedNodeHealth(validAndDefaulted);

            var defaultedWithDs402 = NodeHealthPayload(RequestId);
            TestFrame.WriteUInt16(
                defaultedWithDs402,
                26,
                (ushort)(LMCEtherCATNodeHealthFlags.Configured
                    | LMCEtherCATNodeHealthFlags.DataDefaulted));
            defaultedWithDs402[44] = 0;
            defaultedWithDs402[45] = 0;
            AssertMalformedNodeHealth(defaultedWithDs402);

            var oddSequence = NodeHealthPayload(RequestId);
            TestFrame.WriteUInt32(oddSequence, 40, 3);
            AssertMalformedNodeHealth(oddSequence);
        }

        private static void DigitalIOGoldenAndMalformed()
        {
            var request = new LMCDigitalIOReadRequest(
                TopologyRevision,
                IOReference,
                LMCDigitalIODirection.Output,
                64);
            var value = LMC_DiagnosticsParser.ParseDigitalIO(
                TestFrame.Response(0, DigitalIOPayload(RequestId)),
                RequestId,
                request);

            AssertEx.Equal(TopologyRevision, value.TopologyRevision);
            AssertEx.Equal(IOReference, value.IOReference);
            AssertEx.Equal(NodeId, value.NodeId);
            AssertEx.Equal(LMCDigitalIODirection.Output, value.Direction);
            AssertEx.Equal((byte)64, value.BitWidth);
            AssertEx.True(value.IsValid);
            AssertEx.Equal(0x1122334455667788UL, value.Value);
            AssertEx.Equal(ulong.MaxValue, value.ValidMask);
            AssertEx.Equal(0x01020304u, value.OutputRevision);

            var defaultedPayload = DigitalIOPayload(RequestId);
            TestFrame.WriteUInt16(
                defaultedPayload,
                30,
                (ushort)(LMCDigitalIOStatusFlags.NodeOffline
                    | LMCDigitalIOStatusFlags.DataDefaulted));
            TestFrame.WriteUInt64(defaultedPayload, 32, 0);
            TestFrame.WriteUInt64(defaultedPayload, 40, 0);
            var defaulted = LMC_DiagnosticsParser.ParseDigitalIO(
                TestFrame.Response(0, defaultedPayload),
                RequestId,
                request);
            AssertEx.False(defaulted.IsValid);
            AssertEx.Equal(0UL, defaulted.Value);
            AssertEx.Equal(0UL, defaulted.ValidMask);

            var defaultedWithValue = (byte[])defaultedPayload.Clone();
            TestFrame.WriteUInt64(defaultedWithValue, 32, 1);
            AssertMalformedDigitalIO(defaultedWithValue, request);

            var missingDefaulted = (byte[])defaultedPayload.Clone();
            TestFrame.WriteUInt16(
                missingDefaulted,
                30,
                (ushort)LMCDigitalIOStatusFlags.NodeOffline);
            AssertMalformedDigitalIO(missingDefaulted, request);

            var defaultedWithoutCause = (byte[])defaultedPayload.Clone();
            TestFrame.WriteUInt16(
                defaultedWithoutCause,
                30,
                (ushort)LMCDigitalIOStatusFlags.DataDefaulted);
            AssertMalformedDigitalIO(defaultedWithoutCause, request);

            var validAndDefaulted = DigitalIOPayload(RequestId);
            TestFrame.WriteUInt16(
                validAndDefaulted,
                30,
                (ushort)(LMCDigitalIOStatusFlags.Valid
                    | LMCDigitalIOStatusFlags.DataDefaulted));
            AssertMalformedDigitalIO(validAndDefaulted, request);

            var validAndStale = DigitalIOPayload(RequestId);
            TestFrame.WriteUInt16(
                validAndStale,
                30,
                (ushort)(LMCDigitalIOStatusFlags.Valid
                    | LMCDigitalIOStatusFlags.StaleFrame));
            AssertMalformedDigitalIO(validAndStale, request);

            var missingValidBits = DigitalIOPayload(RequestId);
            TestFrame.WriteUInt64(missingValidBits, 40, 0x7FFFFFFFFFFFFFFFUL);
            AssertMalformedDigitalIO(missingValidBits, request);

            var inputWithOutputRevision = DigitalIOPayload(RequestId);
            inputWithOutputRevision[28] = (byte)LMCDigitalIODirection.Input;
            var inputRequest = new LMCDigitalIOReadRequest(
                TopologyRevision,
                IOReference,
                LMCDigitalIODirection.Input,
                64);
            AssertMalformedDigitalIO(inputWithOutputRevision, inputRequest);

            var narrowRequest = new LMCDigitalIOReadRequest(
                TopologyRevision,
                IOReference,
                LMCDigitalIODirection.Output,
                8);
            var upperBits = DigitalIOPayload(RequestId);
            upperBits[29] = 8;
            TestFrame.WriteUInt64(upperBits, 40, 0xFFUL);
            AssertMalformedDigitalIO(upperBits, narrowRequest);

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCDigitalIOReadRequest(
                    0,
                    IOReference,
                    LMCDigitalIODirection.Input,
                    1));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCDigitalIOReadRequest(
                    TopologyRevision,
                    IOReference,
                    LMCDigitalIODirection.Invalid,
                    1));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCDigitalIOReadRequest(
                    TopologyRevision,
                    IOReference,
                    LMCDigitalIODirection.Input,
                    65));
        }

        private static void DigitalOutputWriteTicketAndPolicy()
        {
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCDigitalOutputWriteRequest(
                    0,
                    IOReference,
                    0,
                    1,
                    1));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCDigitalOutputWriteRequest(
                    TopologyRevision,
                    0,
                    0,
                    1,
                    1));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCDigitalOutputWriteRequest(
                    TopologyRevision,
                    IOReference,
                    0,
                    0,
                    1));
            AssertEx.Throws<ArgumentException>(
                () => new LMCDigitalOutputWriteRequest(
                    TopologyRevision,
                    IOReference,
                    2,
                    1,
                    1));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCDigitalOutputWriteRequest(
                    TopologyRevision,
                    IOReference,
                    0,
                    1,
                    0));

            var inputSnapshot = new LMCDigitalIOValue(
                null,
                TopologyRevision,
                IOReference,
                NodeId,
                LMCDigitalIODirection.Input,
                1,
                LMCDigitalIOStatusFlags.Valid,
                0,
                1,
                1,
                1);
            AssertEx.Throws<ArgumentException>(
                () => LMCDigitalOutputWriteRequest.FromOutputSnapshot(
                    inputSnapshot,
                    0,
                    1));
            var invalidOutputSnapshot = new LMCDigitalIOValue(
                null,
                TopologyRevision,
                IOReference,
                NodeId,
                LMCDigitalIODirection.Output,
                1,
                LMCDigitalIOStatusFlags.None,
                0,
                1,
                1,
                1);
            AssertEx.Throws<ArgumentException>(
                () => LMCDigitalOutputWriteRequest.FromOutputSnapshot(
                    invalidOutputSnapshot,
                    0,
                    1));
            var narrowOutputSnapshot = new LMCDigitalIOValue(
                null,
                TopologyRevision,
                IOReference,
                NodeId,
                LMCDigitalIODirection.Output,
                1,
                LMCDigitalIOStatusFlags.Valid,
                0,
                1,
                1,
                1);
            AssertEx.Throws<ArgumentException>(
                () => LMCDigitalOutputWriteRequest.FromOutputSnapshot(
                    narrowOutputSnapshot,
                    0,
                    2));

            var submission = LMC_DiagnosticsParser.ParseSubmitOperation(
                TestFrame.Response(0, SubmitPayload(RequestId)),
                RequestId,
                LMCOperationKind.DigitalOutputWrite,
                DiagnosticsBootId,
                "SubmitDigitalOutputWrite");
            AssertEx.Equal(0x01020304u, submission.TicketId);
            AssertEx.Equal(
                LMCOperationKind.DigitalOutputWrite,
                submission.OperationKind);

            var dispatchRejected = AssertEx.Throws<
                LMCDiagnosticsDispatchRejectedException>(
                    () => LMC_DiagnosticsParser.ParseSubmitOperation(
                        TestFrame.Response(
                            1,
                            SubmitPayload(RequestId)),
                        RequestId,
                        LMCOperationKind.DigitalOutputWrite,
                        DiagnosticsBootId,
                        "SubmitDigitalOutputWrite"));
            AssertEx.Equal((ushort)1, dispatchRejected.Response.HeaderStatus);

            var shortRejected = AssertEx.Throws<
                LMCDiagnosticsDispatchRejectedException>(
                    () => LMC_DiagnosticsParser.ParseSubmitOperation(
                        TestFrame.Response(
                            0,
                            TestFrame.Hex("01 00 FE FF")),
                        RequestId,
                        LMCOperationKind.DigitalOutputWrite,
                        DiagnosticsBootId,
                        "SubmitDigitalOutputWrite"));
            AssertEx.Equal((short)-2, shortRejected.Response.ErrorId);

            AssertEx.Throws<LMCDiagnosticsNotSupportedException>(
                () => LMC_DiagnosticsParser.ParseSubmitOperation(
                    TestFrame.Response(
                        0,
                        TestFrame.Hex("01 00 FC FF")),
                    RequestId,
                    LMCOperationKind.DigitalOutputWrite,
                    DiagnosticsBootId,
                    "SubmitDigitalOutputWrite"));

            using (var connection = new LMCConnection())
            {
                AssertEx.Equal(
                    0,
                    connection.Diagnostics
                        .GetApprovedDigitalOutputWriteReferences().Count);

                var ticket = new LMCOperationTicket(
                    0x01020304u,
                    LMCOperationKind.DigitalOutputWrite,
                    9,
                    DiagnosticsBootId,
                    TopologyRevision,
                    0,
                    connection.Diagnostics,
                    false,
                    0,
                    LMCSignalValueType.Invalid);
                AssertEx.Equal(0u, ticket.SubmissionMapRevision);
                AssertEx.Equal(
                    TopologyRevision,
                    ticket.SubmissionTopologyRevision);

                var legacyTicket = new LMCOperationTicket(
                    2,
                    LMCOperationKind.PIWrite,
                    9,
                    DiagnosticsBootId,
                    0x01020304u,
                    0,
                    connection.Diagnostics,
                    false,
                    0,
                    LMCSignalValueType.Invalid);
                AssertEx.Equal(0x01020304u, legacyTicket.SubmissionMapRevision);
                AssertEx.Equal(0u, legacyTicket.SubmissionTopologyRevision);
                var status = LMC_DiagnosticsParser.ParseOperationStatus(
                    TestFrame.Response(
                        0,
                        CompletedOutputWritePayload(RequestId)),
                    RequestId,
                    ticket);
                AssertEx.Equal(
                    LMCOperationKind.DigitalOutputWrite,
                    status.OperationKind);
                AssertEx.Equal(LMCOperationOutcome.Success, status.Outcome);
                var mismatchedSubmitCycle =
                    CompletedOutputWritePayload(RequestId);
                TestFrame.WriteUInt32(
                    mismatchedSubmitCycle,
                    24,
                    ticket.QueuedCycle + 1u);
                AssertEx.Throws<InvalidDataException>(
                    () => LMC_DiagnosticsParser.ParseOperationStatus(
                        TestFrame.Response(0, mismatchedSubmitCycle),
                        RequestId,
                        ticket));

                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => new LMCOperationTicket(
                        1,
                        (LMCOperationKind)5,
                        1,
                        DiagnosticsBootId,
                        TopologyRevision,
                        0,
                        connection.Diagnostics,
                        false,
                        0,
                        LMCSignalValueType.Invalid));
            }
        }

        private static void DigitalOutputWriteSubmissionTracker()
        {
            var request = new LMCDigitalOutputWriteRequest(
                TopologyRevision,
                IOReference,
                0,
                1,
                1);

            var rejectedTracker =
                new LMCDigitalOutputWriteSubmissionAttemptTracker(request);
            rejectedTracker.BeginSessionPreflight();
            rejectedTracker.BeginCapabilityPreflight();
            rejectedTracker.RecordCapabilityIdentity(DiagnosticsBootId);
            rejectedTracker.BeginSubmission();
            rejectedTracker.MarkSubmissionOutcomeUncertain();
            var uncertain = rejectedTracker.CreateFailureContext();
            AssertEx.Equal(
                LMCDigitalOutputWriteSubmissionPhase.Submission,
                uncertain.Phase);
            AssertEx.Equal(
                LMCDigitalOutputWriteSubmissionOutcome.OutcomeUncertain,
                uncertain.SubmissionOutcome);
            AssertEx.True(ReferenceEquals(request, uncertain.Request));
            AssertEx.Equal(DiagnosticsBootId, uncertain.DiagnosticsBootId);
            AssertEx.Equal(TopologyRevision, uncertain.TopologyRevision);
            AssertEx.Equal<LMCOperationTicket>(null, uncertain.Ticket);

            rejectedTracker.MarkSubmissionRejected();
            var rejected = rejectedTracker.CreateFailureContext();
            AssertEx.Equal(
                LMCDigitalOutputWriteSubmissionOutcome.Rejected,
                rejected.SubmissionOutcome);

            using (var connection = new LMCConnection())
            {
                var acceptedTracker =
                    new LMCDigitalOutputWriteSubmissionAttemptTracker(
                        request);
                acceptedTracker.BeginSessionPreflight();
                acceptedTracker.BeginCapabilityPreflight();
                acceptedTracker.RecordCapabilityIdentity(DiagnosticsBootId);
                acceptedTracker.BeginSubmission();
                acceptedTracker.MarkSubmissionOutcomeUncertain();
                var ticket = new LMCOperationTicket(
                    0x01020304u,
                    LMCOperationKind.DigitalOutputWrite,
                    9,
                    DiagnosticsBootId,
                    TopologyRevision,
                    0,
                    connection.Diagnostics,
                    false,
                    0,
                    LMCSignalValueType.Invalid);
                acceptedTracker.MarkSubmissionAccepted(ticket);
                var accepted = acceptedTracker.CreateFailureContext();
                AssertEx.Equal(
                    LMCDigitalOutputWriteSubmissionPhase
                        .PostSubmissionValidation,
                    accepted.Phase);
                AssertEx.Equal(
                    LMCDigitalOutputWriteSubmissionOutcome.Accepted,
                    accepted.SubmissionOutcome);
                AssertEx.True(ReferenceEquals(ticket, accepted.Ticket));

                var error = new InvalidOperationException("test");
                LMCDigitalOutputWriteSubmissionFailureContext.Attach(
                    error,
                    accepted);
                LMCDigitalOutputWriteSubmissionFailureContext attached;
                AssertEx.True(
                    LMCDigitalOutputWriteSubmissionFailureContext.TryGet(
                        error,
                        out attached));
                AssertEx.True(ReferenceEquals(accepted, attached));
            }
        }

        private static void DigitalOutputWriteNullFailureContext()
        {
            LMCDigitalOutputWriteSubmissionFailureContext ignored;
            AssertEx.False(
                LMCDigitalOutputWriteSubmissionFailureContext.TryGet(
                    null,
                    out ignored));

            using (var connection = new LMCConnection())
            {
                var syncError = AssertEx.Throws<ArgumentNullException>(
                    () => connection.Diagnostics.SubmitDigitalOutputWrite(
                        null));
                AssertNullDigitalOutputWriteFailureContext(syncError);

                var asyncError = AssertEx.Throws<ArgumentNullException>(
                    () => connection.Diagnostics
                        .SubmitDigitalOutputWriteAsync(
                            null,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertNullDigitalOutputWriteFailureContext(asyncError);
            }
        }

        private static void AssertNullDigitalOutputWriteFailureContext(
            Exception error)
        {
            LMCDigitalOutputWriteSubmissionFailureContext context;
            AssertEx.True(
                LMCDigitalOutputWriteSubmissionFailureContext.TryGet(
                    error,
                    out context));
            AssertEx.Equal<LMCDigitalOutputWriteRequest>(
                null,
                context.Request);
            AssertEx.Equal(
                LMCDigitalOutputWriteSubmissionPhase.RequestValidation,
                context.Phase);
            AssertEx.Equal(
                LMCDigitalOutputWriteSubmissionOutcome.NotAttempted,
                context.SubmissionOutcome);
            AssertEx.Equal(0u, context.DiagnosticsBootId);
            AssertEx.Equal(0u, context.TopologyRevision);
            AssertEx.Equal<LMCOperationTicket>(null, context.Ticket);
        }

        private static void DigitalOutputWriteDetachedRequestPreWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var request = new LMCDigitalOutputWriteRequest(
                    TopologyRevision,
                    IOReference,
                    0,
                    1,
                    1);
                AssertEx.False(request.IsSnapshotBound);

                var syncError = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.SubmitDigitalOutputWrite(
                        request));
                AssertDetachedDigitalOutputWriteFailureContext(
                    syncError,
                    request);

                var asyncError = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics
                        .SubmitDigitalOutputWriteAsync(
                            request,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertDetachedDigitalOutputWriteFailureContext(
                    asyncError,
                    request);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertDetachedDigitalOutputWriteFailureContext(
            Exception error,
            LMCDigitalOutputWriteRequest request)
        {
            LMCDigitalOutputWriteSubmissionFailureContext context;
            AssertEx.True(
                LMCDigitalOutputWriteSubmissionFailureContext.TryGet(
                    error,
                    out context));
            AssertEx.True(ReferenceEquals(request, context.Request));
            AssertEx.Equal(
                LMCDigitalOutputWriteSubmissionPhase.SessionPreflight,
                context.Phase);
            AssertEx.Equal(
                LMCDigitalOutputWriteSubmissionOutcome.NotAttempted,
                context.SubmissionOutcome);
            AssertEx.Equal(0u, context.DiagnosticsBootId);
            AssertEx.Equal(0u, context.TopologyRevision);
        }

        private static void DigitalOutputWriteBootIdentityPreWire()
        {
            var bits = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EtherCATNodeHealth
                | LMCDiagnosticCapability.DigitalIORead
                | LMCDiagnosticCapability.DigitalIOWrite;
            var changedBootId = DiagnosticsBootId + 1u;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            bits,
                            DiagnosticsBootId))),
                new FakeRpcStep(
                    0x7E22,
                    TestFrame.Response(0, DigitalIOPayload(2))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(3, bits, changedBootId))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var shadow = connection.Diagnostics.ReadDigitalIO(
                    CreateDigitalOutputTopology(connection),
                    new LMCDigitalIOReadRequest(
                        TopologyRevision,
                        IOReference,
                        LMCDigitalIODirection.Output,
                        64));
                var request = connection.Diagnostics
                    .CreateDigitalOutputWriteRequest(shadow, 0, 1);

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.SubmitDigitalOutputWrite(
                        request));
                LMCDigitalOutputWriteSubmissionFailureContext context;
                AssertEx.True(
                    LMCDigitalOutputWriteSubmissionFailureContext.TryGet(
                        error,
                        out context));
                AssertEx.Equal(
                    LMCDigitalOutputWriteSubmissionPhase
                        .CapabilityPreflight,
                    context.Phase);
                AssertEx.Equal(
                    LMCDigitalOutputWriteSubmissionOutcome.NotAttempted,
                    context.SubmissionOutcome);
                AssertEx.Equal(changedBootId, context.DiagnosticsBootId);
                AssertEx.Equal(
                    DiagnosticsBootId,
                    request.SourceDiagnosticsBootId);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void CapabilityOffPreWire()
        {
            AssertCapabilityOff(diagnostics =>
                diagnostics.GetEtherCATTopologyInfo());
            AssertCapabilityOff(diagnostics =>
                diagnostics.GetEtherCATTopology());
            AssertCapabilityOff(diagnostics =>
                diagnostics.GetEtherCATTopologyChunk(
                    TopologyRevision,
                    0,
                    1));
            AssertCapabilityOff(diagnostics =>
                diagnostics.ReadEtherCATNodeHealth(
                    TopologyRevision,
                    NodeId));
            AssertCapabilityOff(diagnostics =>
                diagnostics.ReadDigitalIO(
                    new LMCDigitalIOReadRequest(
                        TopologyRevision,
                        IOReference,
                        LMCDigitalIODirection.Input,
                        1)));
            AssertCapabilityOff(diagnostics =>
                diagnostics.GetEtherCATTopologyInfoAsync(
                    CancellationToken.None).GetAwaiter().GetResult());
            AssertCapabilityOff(diagnostics =>
                diagnostics.GetEtherCATTopologyAsync(
                    CancellationToken.None).GetAwaiter().GetResult());
            AssertCapabilityOff(diagnostics =>
                diagnostics.GetEtherCATTopologyChunkAsync(
                    TopologyRevision,
                    0,
                    1,
                    CancellationToken.None).GetAwaiter().GetResult());
            AssertCapabilityOff(diagnostics =>
                diagnostics.ReadEtherCATNodeHealthAsync(
                    TopologyRevision,
                    NodeId,
                    CancellationToken.None).GetAwaiter().GetResult());
            AssertCapabilityOff(diagnostics =>
                diagnostics.ReadDigitalIOAsync(
                    new LMCDigitalIOReadRequest(
                        TopologyRevision,
                        IOReference,
                        LMCDigitalIODirection.Input,
                        1),
                    CancellationToken.None).GetAwaiter().GetResult());
        }

        private static void DigitalOutputWriteEmptyAllowlistPreWire()
        {
            var bits = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EtherCATNodeHealth
                | LMCDiagnosticCapability.DigitalIORead
                | LMCDiagnosticCapability.DigitalIOWrite;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(1, bits, DiagnosticsBootId))),
                new FakeRpcStep(
                    0x7E22,
                    TestFrame.Response(0, DigitalIOPayload(2))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(3, bits, DiagnosticsBootId))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(4, bits, DiagnosticsBootId))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var shadow = connection.Diagnostics.ReadDigitalIO(
                    CreateDigitalOutputTopology(connection),
                    new LMCDigitalIOReadRequest(
                        TopologyRevision,
                        IOReference,
                        LMCDigitalIODirection.Output,
                        64));
                AssertEx.True(shadow.BelongsToCurrentSession(connection));
                AssertEx.True(shadow.HasValidatedTopologyBinding);
                AssertEx.Equal(DiagnosticsBootId, shadow.DiagnosticsBootId);
                AssertEx.Equal(bits, shadow.SourceCapabilities);
                var request = connection.Diagnostics
                    .CreateDigitalOutputWriteRequest(shadow, 0, 1);
                AssertEx.True(request.IsSnapshotBound);
                AssertEx.True(request.BelongsToCurrentSession(connection));
                AssertEx.Equal(
                    DiagnosticsBootId,
                    request.SourceDiagnosticsBootId);
                AssertEx.Equal(ulong.MaxValue, request.SourceValidMask);
                var syncError = AssertEx.Throws<UnauthorizedAccessException>(
                    () => connection.Diagnostics.SubmitDigitalOutputWrite(
                        request));
                AssertDigitalOutputWritePreWireContext(
                    syncError,
                    request);

                var asyncError =
                    AssertEx.Throws<UnauthorizedAccessException>(
                        () => connection.Diagnostics
                            .SubmitDigitalOutputWriteAsync(
                                request,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                AssertDigitalOutputWritePreWireContext(
                    asyncError,
                    request);

                using (var foreignConnection = new LMCConnection())
                {
                    AssertEx.False(shadow.BelongsTo(foreignConnection));
                    AssertEx.False(request.BelongsTo(foreignConnection));
                }

                connection.CloseConnection();
                AssertEx.False(
                    shadow.BelongsToCurrentSession(connection));
                AssertEx.False(
                    request.BelongsToCurrentSession(connection));
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics
                        .CreateDigitalOutputWriteRequest(shadow, 0, 1));
                server.Verify();
            }
        }

        private static void AssertDigitalOutputWritePreWireContext(
            Exception error,
            LMCDigitalOutputWriteRequest request)
        {
            LMCDigitalOutputWriteSubmissionFailureContext context;
            AssertEx.True(
                LMCDigitalOutputWriteSubmissionFailureContext.TryGet(
                    error,
                    out context));
            AssertEx.True(ReferenceEquals(request, context.Request));
            AssertEx.Equal(
                LMCDigitalOutputWriteSubmissionPhase.CapabilityPreflight,
                context.Phase);
            AssertEx.Equal(
                LMCDigitalOutputWriteSubmissionOutcome.NotAttempted,
                context.SubmissionOutcome);
            AssertEx.Equal(DiagnosticsBootId, context.DiagnosticsBootId);
            AssertEx.Equal(TopologyRevision, context.TopologyRevision);
            AssertEx.Equal<LMCOperationTicket>(null, context.Ticket);
        }

        private static void DigitalOutputWriteRawSnapshotRejectedPreWire()
        {
            var bits = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EtherCATNodeHealth
                | LMCDiagnosticCapability.DigitalIORead
                | LMCDiagnosticCapability.DigitalIOWrite;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            bits,
                            DiagnosticsBootId))),
                new FakeRpcStep(
                    0x7E22,
                    TestFrame.Response(0, DigitalIOPayload(2))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var rawObservation = connection.Diagnostics.ReadDigitalIO(
                    new LMCDigitalIOReadRequest(
                        TopologyRevision,
                        IOReference,
                        LMCDigitalIODirection.Output,
                        64));
                AssertEx.False(
                    rawObservation.HasValidatedTopologyBinding);
                var error = AssertEx.Throws<InvalidOperationException>(() =>
                    connection.Diagnostics.CreateDigitalOutputWriteRequest(
                        rawObservation,
                        0,
                        1));
                AssertEx.True(
                    error.Message.IndexOf(
                        "topology-bound",
                        StringComparison.Ordinal) >= 0);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void DigitalOutputWriteCoreWireAccepted()
        {
            RunDigitalOutputWriteWireAccepted(false);
            RunDigitalOutputWriteWireAccepted(true);
        }

        private static void RunDigitalOutputWriteWireAccepted(bool useAsync)
        {
            var submitStep = CreateDigitalOutputWriteSubmitStep(
                TestFrame.Response(0, SubmitPayload(4)));
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                DigitalOutputWriteCapabilitiesStep(1),
                new FakeRpcStep(
                    0x7E22,
                    TestFrame.Response(0, DigitalIOPayload(2))),
                DigitalOutputWriteCapabilitiesStep(3),
                submitStep,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                ConnectTopologyIoTestConnection(connection, server.Port);
                var request = CreateBoundDigitalOutputWriteRequest(connection);
                var ticket = InvokeDigitalOutputWriteWithPolicy(
                    connection,
                    request,
                    useAsync);

                AssertEx.Equal(0x01020304u, ticket.TicketId);
                AssertEx.Equal(
                    LMCOperationKind.DigitalOutputWrite,
                    ticket.OperationKind);
                AssertEx.Equal(9u, ticket.QueuedCycle);
                AssertEx.Equal(DiagnosticsBootId, ticket.DiagnosticsBootId);
                AssertEx.Equal(
                    TopologyRevision,
                    ticket.SubmissionTopologyRevision);
                AssertEx.True(request.BelongsToCurrentSession(connection));
                AssertEx.True(ticket.BelongsToCurrentSession(connection));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void DigitalOutputWriteExplicitRejectionContext()
        {
            RunDigitalOutputWriteExplicitRejection(false);
            RunDigitalOutputWriteExplicitRejection(true);
        }

        private static void RunDigitalOutputWriteExplicitRejection(
            bool dispatcherRejection)
        {
            var submitResponse = dispatcherRejection
                ? TestFrame.Response(1, SubmitPayload(4))
                : TestFrame.Response(
                    0,
                    DigitalOutputWriteDomainErrorPayload(4));
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                DigitalOutputWriteCapabilitiesStep(1),
                new FakeRpcStep(
                    0x7E22,
                    TestFrame.Response(0, DigitalIOPayload(2))),
                DigitalOutputWriteCapabilitiesStep(3),
                CreateDigitalOutputWriteSubmitStep(submitResponse),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                ConnectTopologyIoTestConnection(connection, server.Port);
                var request = CreateBoundDigitalOutputWriteRequest(connection);
                var error = AssertEx.Throws<InvalidOperationException>(
                    () => InvokeDigitalOutputWriteWithPolicy(
                        connection,
                        request,
                        dispatcherRejection));
                var context = RequireDigitalOutputWriteFailureContext(error);

                AssertEx.Equal(
                    LMCDigitalOutputWriteSubmissionPhase.Submission,
                    context.Phase);
                AssertEx.Equal(
                    LMCDigitalOutputWriteSubmissionOutcome.Rejected,
                    context.SubmissionOutcome);
                AssertEx.Equal(DiagnosticsBootId, context.DiagnosticsBootId);
                AssertEx.Equal(TopologyRevision, context.TopologyRevision);
                AssertEx.Equal<LMCOperationTicket>(null, context.Ticket);
                if (dispatcherRejection)
                {
                    AssertEx.True(
                        error is LMCDiagnosticsDispatchRejectedException);
                }
                else
                {
                    AssertEx.True(error is LMCDiagnosticsCommandException);
                }

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void DigitalOutputWriteOutcomeUncertainContext()
        {
            RunDigitalOutputWriteOutcomeUncertain(false);
            RunDigitalOutputWriteOutcomeUncertain(true);
        }

        private static void RunDigitalOutputWriteOutcomeUncertain(
            bool responseLoss)
        {
            var submitStep = responseLoss
                ? CreateDigitalOutputWriteSubmitStep(new byte[0])
                : CreateDigitalOutputWriteSubmitStep(
                    TestFrame.Response(0, CommonPayload(31, 4)));
            submitStep.CloseAfterResponse = responseLoss;
            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                DigitalOutputWriteCapabilitiesStep(1),
                new FakeRpcStep(
                    0x7E22,
                    TestFrame.Response(0, DigitalIOPayload(2))),
                DigitalOutputWriteCapabilitiesStep(3),
                submitStep
            };
            if (!responseLoss)
            {
                steps.Add(CloseStep());
            }

            using (var server = new FakeRpcServer(steps.ToArray()))
            using (var connection = new LMCConnection())
            {
                ConnectTopologyIoTestConnection(connection, server.Port);
                var request = CreateBoundDigitalOutputWriteRequest(connection);
                var error = AssertEx.Throws<Exception>(
                    () => InvokeDigitalOutputWriteWithPolicy(
                        connection,
                        request,
                        responseLoss));
                var context = RequireDigitalOutputWriteFailureContext(error);

                AssertEx.Equal(
                    LMCDigitalOutputWriteSubmissionPhase.Submission,
                    context.Phase);
                AssertEx.Equal(
                    LMCDigitalOutputWriteSubmissionOutcome.OutcomeUncertain,
                    context.SubmissionOutcome);
                AssertEx.Equal(DiagnosticsBootId, context.DiagnosticsBootId);
                AssertEx.Equal(TopologyRevision, context.TopologyRevision);
                AssertEx.Equal<LMCOperationTicket>(null, context.Ticket);
                AssertEx.False(error is LMCDiagnosticsCommandException);
                AssertEx.False(
                    error is LMCDiagnosticsDispatchRejectedException);

                if (!responseLoss)
                {
                    AssertEx.True(error is InvalidDataException);
                    connection.CloseConnection();
                }

                server.Verify();
            }
        }

        private static void DigitalOutputWriteAcceptedSessionRaceContext()
        {
            RunDigitalOutputWriteAcceptedSessionRace(false);
            RunDigitalOutputWriteAcceptedSessionRace(true);
        }

        private static void RunDigitalOutputWriteAcceptedSessionRace(
            bool useAsync)
        {
            LMCConnection connection = null;
            Thread closeThread = null;
            Exception closeError = null;
            var submitStep = CreateDigitalOutputWriteSubmitStep(
                TestFrame.Response(0, SubmitPayload(4)));
            submitStep.InspectRequest = request =>
            {
                AssertDigitalOutputWriteSubmitRequest(request);
                closeThread = new Thread(
                    () =>
                    {
                        try
                        {
                            connection.CloseConnection();
                        }
                        catch (Exception error)
                        {
                            closeError = error;
                        }
                    })
                {
                    IsBackground = true,
                    Name = "LMC output-write accepted-ticket session-race close"
                };
                closeThread.Start();
                if (!SpinWait.SpinUntil(
                        () => connection.State == LMCConnectionState.Closing,
                        3000))
                {
                    throw new TimeoutException(
                        "The output-write session-race close did not enter Closing state.");
                }
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                DigitalOutputWriteCapabilitiesStep(1),
                new FakeRpcStep(
                    0x7E22,
                    TestFrame.Response(0, DigitalIOPayload(2))),
                DigitalOutputWriteCapabilitiesStep(3),
                submitStep,
                CloseStep()))
            using (connection = new LMCConnection())
            {
                ConnectTopologyIoTestConnection(connection, server.Port);
                var request = CreateBoundDigitalOutputWriteRequest(connection);
                var error = AssertEx.Throws<InvalidOperationException>(
                    () => InvokeDigitalOutputWriteWithPolicy(
                        connection,
                        request,
                        useAsync));
                var context = RequireDigitalOutputWriteFailureContext(error);

                AssertEx.Equal(
                    LMCDigitalOutputWriteSubmissionPhase
                        .PostSubmissionValidation,
                    context.Phase);
                AssertEx.Equal(
                    LMCDigitalOutputWriteSubmissionOutcome.Accepted,
                    context.SubmissionOutcome);
                AssertEx.True(ReferenceEquals(request, context.Request));
                AssertEx.Equal(0x01020304u, context.Ticket.TicketId);
                AssertEx.Equal(
                    LMCOperationKind.DigitalOutputWrite,
                    context.Ticket.OperationKind);
                AssertEx.Equal(DiagnosticsBootId, context.DiagnosticsBootId);
                AssertEx.Equal(TopologyRevision, context.TopologyRevision);

                AssertEx.True(
                    closeThread != null && closeThread.Join(3000),
                    "The output-write session-race close did not finish.");
                if (closeError != null)
                {
                    throw new InvalidOperationException(
                        "The output-write session-race close failed.",
                        closeError);
                }

                server.Verify();
            }
        }

        private static void EtherCATNodeHealthSyncAndAsync()
        {
            RunEtherCATNodeHealthIntegration(false);
            RunEtherCATNodeHealthIntegration(true);
        }

        private static void ReadFacadePreWireGuards()
        {
            AssertTopologyIoPreWire<NotSupportedException>(
                LMCDiagnosticCapability.EtherCATTopology,
                1320,
                2040,
                1280,
                diagnostics => diagnostics.ReadEtherCATNodeHealth(
                    TopologyRevision,
                    NodeId));
            AssertTopologyIoPreWire<NotSupportedException>(
                LMCDiagnosticCapability.EtherCATTopology,
                1320,
                2040,
                1280,
                diagnostics => diagnostics.ReadEtherCATNodeHealthAsync(
                        TopologyRevision,
                        NodeId,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertTopologyIoPreWire<NotSupportedException>(
                LMCDiagnosticCapability.EtherCATTopology,
                1320,
                2040,
                1280,
                diagnostics => diagnostics.ReadDigitalIO(
                    CreateDigitalIOReadRequest()));
            AssertTopologyIoPreWire<NotSupportedException>(
                LMCDiagnosticCapability.EtherCATTopology,
                1320,
                2040,
                1280,
                diagnostics => diagnostics.ReadDigitalIOAsync(
                        CreateDigitalIOReadRequest(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());

            AssertTopologyIoPreWire<InvalidDataException>(
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.EtherCATNodeHealth,
                1320,
                68,
                0,
                diagnostics => diagnostics.ReadEtherCATNodeHealth(
                    TopologyRevision,
                    NodeId));
            AssertTopologyIoPreWire<InvalidDataException>(
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.EtherCATNodeHealth,
                1320,
                68,
                0,
                diagnostics => diagnostics.ReadEtherCATNodeHealthAsync(
                        TopologyRevision,
                        NodeId,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());

            // A valid capabilities envelope already requires a 68-byte response
            // limit, so the 56-byte 0x7E22 response cannot be its limiting leg.
            // Exercise the independently negotiated 20-byte request limit.
            AssertTopologyIoPreWire<InvalidDataException>(
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.DigitalIORead,
                19,
                2040,
                1280,
                diagnostics => diagnostics.ReadDigitalIO(
                    CreateDigitalIOReadRequest()));
            AssertTopologyIoPreWire<InvalidDataException>(
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.DigitalIORead,
                19,
                2040,
                1280,
                diagnostics => diagnostics.ReadDigitalIOAsync(
                        CreateDigitalIOReadRequest(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
        }

        private static void PinnedHealthSingleRead()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                TopologyIoCapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.EtherCATTopology
                        | LMCDiagnosticCapability.EtherCATNodeHealth
                        | LMCDiagnosticCapability.DigitalIORead),
                new FakeRpcStep(
                    LMC_CommandId.ReadEtherCATNodeHealth,
                    TestFrame.Response(0, NodeHealthPayload(2))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                ConnectTopologyIoTestConnection(connection, server.Port);
                var capabilities = connection.Diagnostics.GetCapabilities();
                var health = connection.Diagnostics
                    .ReadEtherCATNodeHealthAsync(
                        NodeId,
                        CreatePinnedReadTopology(connection),
                        capabilities,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(NodeId, health.NodeId);
                AssertEx.Equal(TopologyRevision, health.TopologyRevision);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PinnedDigitalInputSingleRead()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                TopologyIoCapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.EtherCATTopology
                        | LMCDiagnosticCapability.EtherCATNodeHealth
                        | LMCDiagnosticCapability.DigitalIORead),
                new FakeRpcStep(
                    LMC_CommandId.ReadDigitalIO,
                    TestFrame.Response(0, DigitalInputPayload(2))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                ConnectTopologyIoTestConnection(connection, server.Port);
                var capabilities = connection.Diagnostics.GetCapabilities();
                var value = connection.Diagnostics.ReadDigitalIOAsync(
                        CreatePinnedReadTopology(connection),
                        CreatePinnedDigitalInputRequest(),
                        capabilities,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(NodeId, value.NodeId);
                AssertEx.Equal(LMCDigitalIODirection.Input, value.Direction);
                AssertEx.True(value.HasValidatedTopologyBinding);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PinnedSnapshotPreWireGuards()
        {
            AssertPinnedForeignSnapshotRejected();
            AssertPinnedUnboundSnapshotRejected();
            AssertPinnedStaleSnapshotRejected();
            AssertPinnedMissingCapabilityRejected();
            AssertPinnedPayloadLimitsRejected();
            AssertPinnedTopologyRequestRejected();
        }

        private static void AssertPinnedForeignSnapshotRejected()
        {
            using (var ownerServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                TopologyIoCapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.EtherCATTopology
                        | LMCDiagnosticCapability.EtherCATNodeHealth),
                CloseStep()))
            using (var foreignServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var ownerConnection = new LMCConnection())
            using (var foreignConnection = new LMCConnection())
            {
                ConnectTopologyIoTestConnection(
                    ownerConnection,
                    ownerServer.Port);
                var capabilities = ownerConnection.Diagnostics
                    .GetCapabilities();
                ConnectTopologyIoTestConnection(
                    foreignConnection,
                    foreignServer.Port);

                AssertEx.Throws<InvalidOperationException>(() =>
                    foreignConnection.Diagnostics
                        .ReadEtherCATNodeHealthAsync(
                            NodeId,
                            CreatePinnedReadTopology(foreignConnection),
                            capabilities,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                foreignConnection.CloseConnection();
                ownerConnection.CloseConnection();
                foreignServer.Verify();
                ownerServer.Verify();
            }
        }

        private static void AssertPinnedUnboundSnapshotRejected()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                ConnectTopologyIoTestConnection(connection, server.Port);
                var capabilities = LMC_DiagnosticsParser.ParseCapabilities(
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            RequestId,
                            LMCDiagnosticCapability.EtherCATTopology
                                | LMCDiagnosticCapability.EtherCATNodeHealth,
                            0)),
                    RequestId,
                    connection.SessionGeneration);

                AssertEx.Throws<InvalidOperationException>(() =>
                    connection.Diagnostics.ReadEtherCATNodeHealthAsync(
                            NodeId,
                            CreatePinnedReadTopology(connection),
                            capabilities,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertPinnedStaleSnapshotRejected()
        {
            LMCDiagnosticCapabilities staleCapabilities;
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                TopologyIoCapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.EtherCATTopology
                        | LMCDiagnosticCapability.EtherCATNodeHealth),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                ConnectTopologyIoTestConnection(connection, firstServer.Port);
                staleCapabilities = connection.Diagnostics.GetCapabilities();
                connection.CloseConnection();
                firstServer.Verify();

                using (var secondServer = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    CloseStep()))
                {
                    ConnectTopologyIoTestConnection(
                        connection,
                        secondServer.Port);
                    AssertEx.Throws<InvalidOperationException>(() =>
                        connection.Diagnostics.ReadEtherCATNodeHealthAsync(
                                NodeId,
                                CreatePinnedReadTopology(connection),
                                staleCapabilities,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());

                    connection.CloseConnection();
                    secondServer.Verify();
                }
            }
        }

        private static void AssertPinnedMissingCapabilityRejected()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                TopologyIoCapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.EtherCATTopology),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                ConnectTopologyIoTestConnection(connection, server.Port);
                var capabilities = connection.Diagnostics.GetCapabilities();
                AssertEx.Throws<NotSupportedException>(() =>
                    connection.Diagnostics.ReadEtherCATNodeHealthAsync(
                            NodeId,
                            CreatePinnedReadTopology(connection),
                            capabilities,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Throws<NotSupportedException>(() =>
                    connection.Diagnostics.ReadDigitalIOAsync(
                            CreatePinnedReadTopology(connection),
                            CreatePinnedDigitalInputRequest(),
                            capabilities,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertPinnedPayloadLimitsRejected()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                TopologyIoCapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.EtherCATTopology
                        | LMCDiagnosticCapability.EtherCATNodeHealth
                        | LMCDiagnosticCapability.DigitalIORead,
                    19,
                    68,
                    0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                ConnectTopologyIoTestConnection(connection, server.Port);
                var capabilities = connection.Diagnostics.GetCapabilities();
                AssertEx.Throws<InvalidDataException>(() =>
                    connection.Diagnostics.ReadEtherCATNodeHealthAsync(
                            NodeId,
                            CreatePinnedReadTopology(connection),
                            capabilities,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Throws<InvalidDataException>(() =>
                    connection.Diagnostics.ReadDigitalIOAsync(
                            CreatePinnedReadTopology(connection),
                            CreatePinnedDigitalInputRequest(),
                            capabilities,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertPinnedTopologyRequestRejected()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                TopologyIoCapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.EtherCATTopology
                        | LMCDiagnosticCapability.EtherCATNodeHealth
                        | LMCDiagnosticCapability.DigitalIORead),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                ConnectTopologyIoTestConnection(connection, server.Port);
                var capabilities = connection.Diagnostics.GetCapabilities();
                var topology = CreatePinnedReadTopology(connection);
                AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                    connection.Diagnostics.ReadEtherCATNodeHealthAsync(
                            NodeId + 1,
                            topology,
                            capabilities,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Throws<InvalidDataException>(() =>
                    connection.Diagnostics.ReadDigitalIOAsync(
                            topology,
                            new LMCDigitalIOReadRequest(
                                TopologyRevision,
                                IOReference,
                                LMCDigitalIODirection.Input,
                                32),
                            capabilities,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunEtherCATNodeHealthIntegration(bool useAsync)
        {
            var healthStep = new FakeRpcStep(
                0x7E13,
                TestFrame.Response(0, NodeHealthPayload(2)));
            healthStep.InspectRequest = request =>
            {
                AssertEx.Equal(24, request.Length);
                AssertEx.Equal((ushort)16, TestFrame.ReadUInt16(request, 4));
                AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 6));
                AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 8));
                AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 10));
                AssertEx.Equal(2u, TestFrame.ReadUInt32(request, 12));
                AssertEx.Equal(
                    TopologyRevision,
                    TestFrame.ReadUInt32(request, 16));
                AssertEx.Equal(NodeId, TestFrame.ReadUInt32(request, 20));
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCDiagnosticCapability.EtherCATTopology
                                | LMCDiagnosticCapability.EtherCATNodeHealth,
                            0))),
                healthStep,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var health = useAsync
                    ? connection.Diagnostics.ReadEtherCATNodeHealthAsync(
                            TopologyRevision,
                            NodeId,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : connection.Diagnostics.ReadEtherCATNodeHealth(
                        TopologyRevision,
                        NodeId);
                AssertEx.Equal(TopologyRevision, health.TopologyRevision);
                AssertEx.Equal(NodeId, health.NodeId);
                AssertEx.True(health.Online);
                AssertEx.Equal(LMCCapturePhase.InputMapped, health.CapturePhase);
                AssertEx.Equal((uint)2, health.SnapshotSequence);
                AssertEx.Equal((byte)8, health.EtherCATState);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static LMCDigitalIOReadRequest CreateDigitalIOReadRequest()
        {
            return new LMCDigitalIOReadRequest(
                TopologyRevision,
                IOReference,
                LMCDigitalIODirection.Output,
                64);
        }

        private static void AssertTopologyIoPreWire<TException>(
            LMCDiagnosticCapability capabilities,
            ushort maxRequestPayloadBytes,
            ushort maxResponsePayloadBytes,
            ushort maxChunkDataBytes,
            Action<LMCDiagnostics> action)
            where TException : Exception
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            capabilities,
                            0,
                            maxRequestPayloadBytes,
                            maxResponsePayloadBytes,
                            maxChunkDataBytes))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                AssertEx.Throws<TException>(() => action(connection.Diagnostics));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void TopologyDownloadAndDigitalIORead()
        {
            RunTopologyDownloadIntegration(false);
            RunTopologyDownloadIntegration(true);
            AssertTopologyCrcMismatchRejected();
            RunDigitalIOReadIntegration(false);
            RunDigitalIOReadIntegration(true);
        }

        private static void RunTopologyDownloadIntegration(bool useAsync)
        {
            var seedChunk = LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                TestFrame.Response(
                    0,
                    TopologyChunkPayload(RequestId)),
                RequestId,
                TopologyRevision,
                0,
                2);
            var canonicalRevision =
                LMC_DiagnosticsParser.ComputeEtherCATTopologyRevision(
                    seedChunk.Entries);
            var infoPayload = TopologyInfoPayload(2, canonicalRevision);
            TestFrame.WriteUInt16(infoPayload, 24, 1);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCDiagnosticCapability.EtherCATTopology,
                            0))),
                new FakeRpcStep(
                    0x7E11,
                    TestFrame.Response(0, infoPayload)),
                new FakeRpcStep(
                    0x7E12,
                    TestFrame.Response(
                        0,
                        TopologyChunkSlicePayload(
                            3,
                            canonicalRevision,
                            0))),
                new FakeRpcStep(
                    0x7E12,
                    TestFrame.Response(
                        0,
                        TopologyChunkSlicePayload(
                            4,
                            canonicalRevision,
                            1))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var topology = useAsync
                    ? connection.Diagnostics.GetEtherCATTopologyAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : connection.Diagnostics.GetEtherCATTopology();
                AssertEx.Equal(canonicalRevision, topology.TopologyRevision);
                AssertEx.Equal(2, topology.Entries.Count);
                AssertEx.True(topology.BelongsTo(connection));
                AssertEx.True(
                    topology.BelongsToCurrentSession(connection));
                LMCEtherCATTopologyEntry ioNode;
                AssertEx.True(topology.TryGetNode(NodeId, out ioNode));
                AssertEx.Equal(IOReference, ioNode.IOReference);

                connection.CloseConnection();
                AssertEx.True(topology.BelongsTo(connection));
                AssertEx.False(
                    topology.BelongsToCurrentSession(connection));
                server.Verify();
            }
        }

        private static void RunLasalSevenNodeTopologyDownload(bool useAsync)
        {
            var expectedEntries = LasalTopologyEntries();
            var canonical = LasalTopologyCanonicalBytes(expectedEntries);
            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCDiagnosticCapability.EtherCATTopology,
                            0))),
                new FakeRpcStep(
                    0x7E11,
                    TestFrame.Response(0, LasalTopologyInfoPayload(2)))
            };

            for (ushort startIndex = 0;
                startIndex < LasalTopologyNodeCount;
                startIndex++)
            {
                var expectedStartIndex = startIndex;
                var expectedRequestId = checked(3u + expectedStartIndex);
                var step = new FakeRpcStep(
                    0x7E12,
                    TestFrame.Response(
                        0,
                        LasalTopologyChunkPayload(
                            expectedRequestId,
                            expectedStartIndex,
                            canonical)));
                step.InspectRequest = request =>
                {
                    AssertEx.Equal(24, request.Length);
                    AssertEx.Equal((ushort)16, TestFrame.ReadUInt16(request, 4));
                    AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 6));
                    AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 8));
                    AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 10));
                    AssertEx.Equal(
                        expectedRequestId,
                        TestFrame.ReadUInt32(request, 12));
                    AssertEx.Equal(
                        LasalTopologyRevision,
                        TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal(
                        expectedStartIndex,
                        TestFrame.ReadUInt16(request, 20));
                    AssertEx.Equal(
                        (ushort)1,
                        TestFrame.ReadUInt16(request, 22));
                };
                steps.Add(step);
            }

            steps.Add(CloseStep());
            using (var server = new FakeRpcServer(steps.ToArray()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var topology = useAsync
                    ? connection.Diagnostics.GetEtherCATTopologyAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : connection.Diagnostics.GetEtherCATTopology();
                AssertEx.Equal(LasalTopologyRevision, topology.TopologyRevision);
                AssertEx.Equal(
                    (ushort)1,
                    topology.Info.MaxEntriesPerChunk);
                AssertEx.Equal(
                    (int)LasalTopologyNodeCount,
                    topology.Entries.Count);
                for (var index = 0; index < expectedEntries.Length; index++)
                {
                    AssertLasalTopologyEntry(
                        expectedEntries[index],
                        topology.Entries[index]);
                }

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertTopologyCrcMismatchRejected()
        {
            var seedChunk = ParseTopologyChunk(
                TopologyChunkPayload(RequestId),
                2);
            var staleRevision =
                LMC_DiagnosticsParser.ComputeEtherCATTopologyRevision(
                    seedChunk.Entries) ^ 1u;
            var infoPayload = TopologyInfoPayload(2, staleRevision);
            TestFrame.WriteUInt16(infoPayload, 24, 1);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCDiagnosticCapability.EtherCATTopology,
                            0))),
                new FakeRpcStep(
                    0x7E11,
                    TestFrame.Response(0, infoPayload)),
                new FakeRpcStep(
                    0x7E12,
                    TestFrame.Response(
                        0,
                        TopologyChunkSlicePayload(
                            3,
                            staleRevision,
                            0))),
                new FakeRpcStep(
                    0x7E12,
                    TestFrame.Response(
                        0,
                        TopologyChunkSlicePayload(
                            4,
                            staleRevision,
                            1))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                AssertEx.Throws<InvalidDataException>(
                    () => connection.Diagnostics.GetEtherCATTopology());

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunDigitalIOReadIntegration(bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCDiagnosticCapability.EtherCATTopology
                                | LMCDiagnosticCapability.DigitalIORead,
                            0))),
                new FakeRpcStep(
                    0x7E22,
                    TestFrame.Response(0, DigitalIOPayload(2))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var request = new LMCDigitalIOReadRequest(
                    TopologyRevision,
                    IOReference,
                    LMCDigitalIODirection.Output,
                    64);
                var value = useAsync
                    ? connection.Diagnostics.ReadDigitalIOAsync(
                            request,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : connection.Diagnostics.ReadDigitalIO(request);
                AssertEx.True(value.IsValid);
                AssertEx.Equal(0x1122334455667788UL, value.Value);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static LMCEtherCATTopologyEntry[] LasalTopologyEntries()
        {
            var entries = new List<LMCEtherCATTopologyEntry>
            {
                new LMCEtherCATTopologyEntry(
                    0xEC000001u,
                    0,
                    0,
                    0,
                    LMCEtherCATTopologyNodeKind.EtherCATSlave,
                    LMCEtherCATTopologyNodeFlags.HasMasterSlaveIndex
                        | LMCEtherCATTopologyNodeFlags.IoCoupler,
                    0,
                    0,
                    ushort.MaxValue,
                    669,
                    1196200070,
                    65536,
                    0,
                    0,
                    0,
                    "GL_9086_11",
                    0)
            };

            var driveNames = new[]
            {
                "Elmo_11",
                "Elmo_21",
                "Elmo_31",
                "Elmo_41"
            };
            for (ushort axis = 1; axis <= driveNames.Length; axis++)
            {
                entries.Add(new LMCEtherCATTopologyEntry(
                    checked(0xEC000100u + axis),
                    0,
                    axis,
                    axis,
                    LMCEtherCATTopologyNodeKind.EtherCATSlave,
                    LMCEtherCATTopologyNodeFlags.HasMasterSlaveIndex
                        | LMCEtherCATTopologyNodeFlags.SupportsSdo
                        | LMCEtherCATTopologyNodeFlags.PhysicalAxis
                        | LMCEtherCATTopologyNodeFlags.Ds402Drive,
                    axis,
                    axis,
                    ushort.MaxValue,
                    154,
                    198948,
                    66592,
                    0,
                    0,
                    0,
                    driveNames[axis - 1],
                    0));
            }

            entries.Add(new LMCEtherCATTopologyEntry(
                0xEC010001u,
                0xEC000001u,
                5,
                ushort.MaxValue,
                LMCEtherCATTopologyNodeKind.SlotModule,
                LMCEtherCATTopologyNodeFlags.HasInputs
                    | LMCEtherCATTopologyNodeFlags.HasDigitalIO,
                0,
                0,
                0,
                669,
                1196692218,
                0,
                0,
                4,
                0,
                "GL_9086_1_Slot001",
                0x00010001u));
            entries.Add(new LMCEtherCATTopologyEntry(
                0xEC010002u,
                0xEC000001u,
                6,
                ushort.MaxValue,
                LMCEtherCATTopologyNodeKind.SlotModule,
                LMCEtherCATTopologyNodeFlags.HasOutputs
                    | LMCEtherCATTopologyNodeFlags.HasDigitalIO,
                0,
                0,
                1,
                669,
                1196696250,
                0,
                0,
                0,
                4,
                "GL_9086_1_Slot011",
                0x00010002u));

            return entries.ToArray();
        }

        private static byte[] LasalTopologyCanonicalBytes(
            IReadOnlyList<LMCEtherCATTopologyEntry> entries)
        {
            var canonical = new byte[checked(entries.Count * 96)];
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                var offset = index * 96;
                TestFrame.WriteUInt32(canonical, offset, entry.NodeId);
                TestFrame.WriteUInt32(canonical, offset + 4, entry.ParentNodeId);
                TestFrame.WriteUInt16(canonical, offset + 8, entry.TopologyIndex);
                TestFrame.WriteUInt16(
                    canonical,
                    offset + 10,
                    entry.MasterSlaveIndex);
                canonical[offset + 12] = (byte)entry.NodeKind;
                TestFrame.WriteUInt16(
                    canonical,
                    offset + 14,
                    (ushort)entry.NodeFlags);
                TestFrame.WriteUInt16(
                    canonical,
                    offset + 16,
                    entry.SdoSlaveReference);
                TestFrame.WriteUInt16(
                    canonical,
                    offset + 18,
                    entry.PhysicalAxisReference);
                TestFrame.WriteUInt16(canonical, offset + 20, entry.SlotIndex);
                TestFrame.WriteUInt32(canonical, offset + 24, entry.VendorId);
                TestFrame.WriteUInt32(canonical, offset + 28, entry.ProductCode);
                TestFrame.WriteUInt32(
                    canonical,
                    offset + 32,
                    entry.RevisionNumber);
                TestFrame.WriteUInt32(canonical, offset + 36, entry.SerialNumber);
                TestFrame.WriteUInt16(canonical, offset + 40, entry.InputBytes);
                TestFrame.WriteUInt16(canonical, offset + 42, entry.OutputBytes);
                var nameBytes = Encoding.ASCII.GetBytes(entry.Name);
                Buffer.BlockCopy(
                    nameBytes,
                    0,
                    canonical,
                    offset + 44,
                    nameBytes.Length);
                TestFrame.WriteUInt32(canonical, offset + 92, entry.IOReference);
            }

            return canonical;
        }

        private static void AssertLasalTopologyCanonicalBytes(
            byte[] canonical,
            IReadOnlyList<LMCEtherCATTopologyEntry> entries)
        {
            AssertEx.Equal(LasalTopologyNodeCount * 96, canonical.Length);
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                var offset = index * 96;
                AssertEx.Equal(entry.NodeId, TestFrame.ReadUInt32(canonical, offset));
                AssertEx.Equal(
                    entry.ParentNodeId,
                    TestFrame.ReadUInt32(canonical, offset + 4));
                AssertEx.Equal(
                    entry.TopologyIndex,
                    TestFrame.ReadUInt16(canonical, offset + 8));
                AssertEx.Equal(
                    entry.MasterSlaveIndex,
                    TestFrame.ReadUInt16(canonical, offset + 10));
                AssertEx.Equal((byte)entry.NodeKind, canonical[offset + 12]);
                AssertEx.Equal((byte)0, canonical[offset + 13]);
                AssertEx.Equal(
                    (ushort)entry.NodeFlags,
                    TestFrame.ReadUInt16(canonical, offset + 14));
                AssertEx.Equal(
                    entry.SdoSlaveReference,
                    TestFrame.ReadUInt16(canonical, offset + 16));
                AssertEx.Equal(
                    entry.PhysicalAxisReference,
                    TestFrame.ReadUInt16(canonical, offset + 18));
                AssertEx.Equal(
                    entry.SlotIndex,
                    TestFrame.ReadUInt16(canonical, offset + 20));
                AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(canonical, offset + 22));
                AssertEx.Equal(
                    entry.VendorId,
                    TestFrame.ReadUInt32(canonical, offset + 24));
                AssertEx.Equal(
                    entry.ProductCode,
                    TestFrame.ReadUInt32(canonical, offset + 28));
                AssertEx.Equal(
                    entry.RevisionNumber,
                    TestFrame.ReadUInt32(canonical, offset + 32));
                AssertEx.Equal(
                    entry.SerialNumber,
                    TestFrame.ReadUInt32(canonical, offset + 36));
                AssertEx.Equal(
                    entry.InputBytes,
                    TestFrame.ReadUInt16(canonical, offset + 40));
                AssertEx.Equal(
                    entry.OutputBytes,
                    TestFrame.ReadUInt16(canonical, offset + 42));

                var nameBytes = Encoding.ASCII.GetBytes(entry.Name);
                for (var nameIndex = 0; nameIndex < nameBytes.Length; nameIndex++)
                {
                    AssertEx.Equal(
                        nameBytes[nameIndex],
                        canonical[offset + 44 + nameIndex]);
                }
                for (var paddingIndex = nameBytes.Length;
                    paddingIndex < 48;
                    paddingIndex++)
                {
                    AssertEx.Equal(
                        (byte)0,
                        canonical[offset + 44 + paddingIndex]);
                }
                AssertEx.Equal(
                    entry.IOReference,
                    TestFrame.ReadUInt32(canonical, offset + 92));
            }
        }

        private static void AssertLasalTopologyEntry(
            LMCEtherCATTopologyEntry expected,
            LMCEtherCATTopologyEntry actual)
        {
            AssertEx.Equal(expected.NodeId, actual.NodeId);
            AssertEx.Equal(expected.ParentNodeId, actual.ParentNodeId);
            AssertEx.Equal(expected.TopologyIndex, actual.TopologyIndex);
            AssertEx.Equal(expected.MasterSlaveIndex, actual.MasterSlaveIndex);
            AssertEx.Equal(expected.NodeKind, actual.NodeKind);
            AssertEx.Equal(expected.NodeFlags, actual.NodeFlags);
            AssertEx.Equal(expected.SdoSlaveReference, actual.SdoSlaveReference);
            AssertEx.Equal(
                expected.PhysicalAxisReference,
                actual.PhysicalAxisReference);
            AssertEx.Equal(expected.SlotIndex, actual.SlotIndex);
            AssertEx.Equal(expected.VendorId, actual.VendorId);
            AssertEx.Equal(expected.ProductCode, actual.ProductCode);
            AssertEx.Equal(expected.RevisionNumber, actual.RevisionNumber);
            AssertEx.Equal(expected.SerialNumber, actual.SerialNumber);
            AssertEx.Equal(expected.InputBytes, actual.InputBytes);
            AssertEx.Equal(expected.OutputBytes, actual.OutputBytes);
            AssertEx.Equal(expected.Name, actual.Name);
            AssertEx.Equal(expected.IOReference, actual.IOReference);
        }

        private static byte[] LasalTopologyInfoPayload(uint requestId)
        {
            var payload = CommonPayload(44, requestId);
            TestFrame.WriteUInt32(payload, 16, LasalTopologyRevision);
            TestFrame.WriteUInt16(payload, 20, LasalTopologyNodeCount);
            TestFrame.WriteUInt16(payload, 22, 96);
            TestFrame.WriteUInt16(payload, 24, 1);
            TestFrame.WriteUInt16(payload, 26, 5);
            TestFrame.WriteUInt16(payload, 28, 2);
            TestFrame.WriteUInt16(payload, 30, 4);
            TestFrame.WriteUInt32(payload, 32, 0x0000000Fu);
            TestFrame.WriteUInt32(payload, 36, 1);
            return payload;
        }

        private static byte[] LasalTopologyChunkPayload(
            uint requestId,
            ushort startIndex,
            byte[] canonical)
        {
            if (startIndex >= LasalTopologyNodeCount
                || canonical == null
                || canonical.Length != LasalTopologyNodeCount * 96)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            var payload = CommonPayload(124, requestId);
            if (startIndex == LasalTopologyNodeCount - 1)
            {
                TestFrame.WriteUInt16(
                    payload,
                    2,
                    (ushort)LMCDiagnosticsResponseFlags.LastChunk);
            }
            TestFrame.WriteUInt32(payload, 16, LasalTopologyRevision);
            TestFrame.WriteUInt16(payload, 20, startIndex);
            TestFrame.WriteUInt16(payload, 22, 1);
            TestFrame.WriteUInt16(payload, 24, LasalTopologyNodeCount);
            TestFrame.WriteUInt16(payload, 26, 96);
            Buffer.BlockCopy(canonical, startIndex * 96, payload, 28, 96);
            return payload;
        }

        private static byte[] TopologyInfoPayload(
            uint requestId,
            uint topologyRevision = TopologyRevision)
        {
            var payload = CommonPayload(44, requestId);
            TestFrame.WriteUInt32(payload, 16, topologyRevision);
            TestFrame.WriteUInt16(payload, 20, 2);
            TestFrame.WriteUInt16(payload, 22, 96);
            TestFrame.WriteUInt16(payload, 24, 16);
            TestFrame.WriteUInt16(payload, 26, 1);
            TestFrame.WriteUInt16(payload, 28, 1);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt32(payload, 32, 0x0000000Fu);
            TestFrame.WriteUInt32(payload, 36, 1);
            return payload;
        }

        private static byte[] TopologyChunkPayload(
            uint requestId,
            uint topologyRevision = TopologyRevision)
        {
            var payload = CommonPayload(28 + 2 * 96, requestId);
            TestFrame.WriteUInt16(
                payload,
                2,
                (ushort)LMCDiagnosticsResponseFlags.LastChunk);
            TestFrame.WriteUInt32(payload, 16, topologyRevision);
            TestFrame.WriteUInt16(payload, 20, 0);
            TestFrame.WriteUInt16(payload, 22, 2);
            TestFrame.WriteUInt16(payload, 24, 2);
            TestFrame.WriteUInt16(payload, 26, 96);

            WriteTopologyEntry(
                payload,
                28,
                NodeId,
                0,
                0,
                0,
                LMCEtherCATTopologyNodeKind.EtherCATSlave,
                LMCEtherCATTopologyNodeFlags.HasMasterSlaveIndex
                    | LMCEtherCATTopologyNodeFlags.SupportsSdo
                    | LMCEtherCATTopologyNodeFlags.PhysicalAxis
                    | LMCEtherCATTopologyNodeFlags.HasInputs
                    | LMCEtherCATTopologyNodeFlags.HasOutputs
                    | LMCEtherCATTopologyNodeFlags.Ds402Drive
                    | LMCEtherCATTopologyNodeFlags.HasDigitalIO,
                1,
                1,
                ushort.MaxValue,
                8,
                8,
                "axis-io-slave",
                IOReference);
            WriteTopologyEntry(
                payload,
                28 + 96,
                0x00000102u,
                NodeId,
                1,
                ushort.MaxValue,
                LMCEtherCATTopologyNodeKind.SlotModule,
                LMCEtherCATTopologyNodeFlags.HasInputs
                    | LMCEtherCATTopologyNodeFlags.HasDigitalIO,
                0,
                0,
                0,
                2,
                0,
                "digital-input-module",
                0x00000502u);

            return payload;
        }

        private static byte[] TopologyChunkSlicePayload(
            uint requestId,
            uint topologyRevision,
            ushort startIndex)
        {
            if (startIndex > 1)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            var full = TopologyChunkPayload(requestId, topologyRevision);
            var payload = CommonPayload(28 + 96, requestId);
            if (startIndex == 1)
            {
                TestFrame.WriteUInt16(
                    payload,
                    2,
                    (ushort)LMCDiagnosticsResponseFlags.LastChunk);
            }

            TestFrame.WriteUInt32(payload, 16, topologyRevision);
            TestFrame.WriteUInt16(payload, 20, startIndex);
            TestFrame.WriteUInt16(payload, 22, 1);
            TestFrame.WriteUInt16(payload, 24, 2);
            TestFrame.WriteUInt16(payload, 26, 96);
            Buffer.BlockCopy(
                full,
                28 + startIndex * 96,
                payload,
                28,
                96);
            return payload;
        }

        private static void WriteTopologyEntry(
            byte[] payload,
            int offset,
            uint nodeId,
            uint parentNodeId,
            ushort topologyIndex,
            ushort masterSlaveIndex,
            LMCEtherCATTopologyNodeKind nodeKind,
            LMCEtherCATTopologyNodeFlags nodeFlags,
            ushort sdoSlaveReference,
            ushort physicalAxisReference,
            ushort slotIndex,
            ushort inputBytes,
            ushort outputBytes,
            string name,
            uint ioReference)
        {
            TestFrame.WriteUInt32(payload, offset, nodeId);
            TestFrame.WriteUInt32(payload, offset + 4, parentNodeId);
            TestFrame.WriteUInt16(payload, offset + 8, topologyIndex);
            TestFrame.WriteUInt16(payload, offset + 10, masterSlaveIndex);
            payload[offset + 12] = (byte)nodeKind;
            TestFrame.WriteUInt16(payload, offset + 14, (ushort)nodeFlags);
            TestFrame.WriteUInt16(payload, offset + 16, sdoSlaveReference);
            TestFrame.WriteUInt16(payload, offset + 18, physicalAxisReference);
            TestFrame.WriteUInt16(payload, offset + 20, slotIndex);
            TestFrame.WriteUInt32(payload, offset + 24, 0x0000009Au);
            TestFrame.WriteUInt32(payload, offset + 28, 0x00001001u);
            TestFrame.WriteUInt32(payload, offset + 32, 1);
            TestFrame.WriteUInt32(payload, offset + 36, topologyIndex + 1u);
            TestFrame.WriteUInt16(payload, offset + 40, inputBytes);
            TestFrame.WriteUInt16(payload, offset + 42, outputBytes);
            var nameBytes = Encoding.ASCII.GetBytes(name);
            Buffer.BlockCopy(nameBytes, 0, payload, offset + 44, nameBytes.Length);
            TestFrame.WriteUInt32(payload, offset + 92, ioReference);
        }

        private static byte[] NodeHealthPayload(uint requestId)
        {
            var payload = CommonPayload(72, requestId);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt32(payload, 20, NodeId);
            TestFrame.WriteUInt16(payload, 24, (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt16(
                payload,
                26,
                (ushort)(LMCEtherCATNodeHealthFlags.Configured
                    | LMCEtherCATNodeHealthFlags.Detected
                    | LMCEtherCATNodeHealthFlags.IdentityMatched
                    | LMCEtherCATNodeHealthFlags.DataValid
                    | LMCEtherCATNodeHealthFlags.Ds402DataPresent));
            TestFrame.WriteUInt32(payload, 28, 100);
            TestFrame.WriteUInt64(payload, 32, 0x1122334455667788UL);
            TestFrame.WriteUInt32(payload, 40, 2);
            payload[44] = 1;
            payload[45] = 8;
            TestFrame.WriteUInt16(payload, 46, 0);
            TestFrame.WriteUInt32(payload, 48, 7);
            TestFrame.WriteUInt32(payload, 52, 8);
            TestFrame.WriteUInt32(payload, 56, 0x1234u);
            TestFrame.WriteUInt32(payload, 60, 0x5678u);
            TestFrame.WriteUInt32(payload, 64, 99);
            TestFrame.WriteUInt32(payload, 68, 90);
            return payload;
        }

        private static byte[] DigitalIOPayload(uint requestId)
        {
            var payload = CommonPayload(56, requestId);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt32(payload, 20, IOReference);
            TestFrame.WriteUInt32(payload, 24, NodeId);
            payload[28] = (byte)LMCDigitalIODirection.Output;
            payload[29] = 64;
            TestFrame.WriteUInt16(
                payload,
                30,
                (ushort)LMCDigitalIOStatusFlags.Valid);
            TestFrame.WriteUInt64(payload, 32, 0x1122334455667788UL);
            TestFrame.WriteUInt64(payload, 40, ulong.MaxValue);
            TestFrame.WriteUInt32(payload, 48, 100);
            TestFrame.WriteUInt32(payload, 52, 0x01020304u);
            return payload;
        }

        private static byte[] DigitalInputPayload(uint requestId)
        {
            var payload = DigitalIOPayload(requestId);
            payload[28] = (byte)LMCDigitalIODirection.Input;
            TestFrame.WriteUInt32(payload, 52, 0);
            return payload;
        }

        private static byte[] SubmitPayload(uint requestId)
        {
            var payload = CommonPayload(32, requestId);
            TestFrame.WriteUInt32(payload, 16, 0x01020304u);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCOperationKind.DigitalOutputWrite);
            TestFrame.WriteUInt16(
                payload,
                22,
                (ushort)LMCOperationState.Queued);
            TestFrame.WriteUInt32(payload, 24, 9);
            TestFrame.WriteUInt32(payload, 28, DiagnosticsBootId);
            return payload;
        }

        private static byte[] CompletedOutputWritePayload(uint requestId)
        {
            var payload = CommonPayload(64, requestId);
            TestFrame.WriteUInt32(payload, 16, 0x01020304u);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCOperationKind.DigitalOutputWrite);
            TestFrame.WriteUInt16(
                payload,
                22,
                (ushort)LMCOperationState.Completed);
            TestFrame.WriteUInt32(payload, 24, 9);
            TestFrame.WriteUInt32(payload, 28, 10);
            TestFrame.WriteUInt16(
                payload,
                32,
                (ushort)LMCOperationOutcome.Success);
            TestFrame.WriteUInt32(payload, 60, DiagnosticsBootId);
            return payload;
        }

        private static byte[] CommonPayload(int length, uint requestId)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static byte[] CapabilitiesPayload(
            uint requestId,
            LMCDiagnosticCapability capabilities,
            uint diagnosticsBootId,
            ushort maxRequestPayloadBytes = 1320,
            ushort maxResponsePayloadBytes = 2040,
            ushort maxChunkDataBytes = 1280)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 1);
            TestFrame.WriteUInt32(payload, 20, (uint)capabilities);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, maxRequestPayloadBytes);
            TestFrame.WriteUInt16(payload, 46, maxResponsePayloadBytes);
            TestFrame.WriteUInt16(payload, 48, maxChunkDataBytes);
            TestFrame.WriteUInt16(payload, 50, 80);
            TestFrame.WriteUInt16(payload, 52, 16);
            TestFrame.WriteUInt32(payload, 64, diagnosticsBootId);
            return payload;
        }

        private static void ConvertSecondEntryToSlave(
            byte[] payload,
            ushort masterSlaveIndex,
            ushort sdoSlaveReference,
            ushort physicalAxisReference)
        {
            var offset = 28 + 96;
            TestFrame.WriteUInt32(payload, offset + 4, 0);
            TestFrame.WriteUInt16(
                payload,
                offset + 10,
                masterSlaveIndex);
            payload[offset + 12] =
                (byte)LMCEtherCATTopologyNodeKind.EtherCATSlave;
            TestFrame.WriteUInt16(
                payload,
                offset + 14,
                (ushort)(LMCEtherCATTopologyNodeFlags.HasMasterSlaveIndex
                    | LMCEtherCATTopologyNodeFlags.SupportsSdo
                    | LMCEtherCATTopologyNodeFlags.PhysicalAxis
                    | LMCEtherCATTopologyNodeFlags.HasInputs
                    | LMCEtherCATTopologyNodeFlags.HasDigitalIO));
            TestFrame.WriteUInt16(
                payload,
                offset + 16,
                sdoSlaveReference);
            TestFrame.WriteUInt16(
                payload,
                offset + 18,
                physicalAxisReference);
            TestFrame.WriteUInt16(payload, offset + 20, ushort.MaxValue);
        }

        private static LMCEtherCATTopologyEntry CloneTopologyEntry(
            LMCEtherCATTopologyEntry source,
            ushort topologyIndex)
        {
            return new LMCEtherCATTopologyEntry(
                source.NodeId,
                source.ParentNodeId,
                topologyIndex,
                source.MasterSlaveIndex,
                source.NodeKind,
                source.NodeFlags,
                source.SdoSlaveReference,
                source.PhysicalAxisReference,
                source.SlotIndex,
                source.VendorId,
                source.ProductCode,
                source.RevisionNumber,
                source.SerialNumber,
                source.InputBytes,
                source.OutputBytes,
                source.Name,
                source.IOReference);
        }

        private static LMCEtherCATTopologyChunk ParseTopologyChunk(
            byte[] payload,
            ushort maxEntries)
        {
            return LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                TestFrame.Response(0, payload),
                RequestId,
                TopologyRevision,
                0,
                maxEntries);
        }

        private static void AssertMalformedTopologyChunk(byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                    TestFrame.Response(0, payload),
                    RequestId,
                    TopologyRevision,
                    0,
                    2));
        }

        private static void AssertMalformedNodeHealth(byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATNodeHealth(
                    TestFrame.Response(0, payload),
                    RequestId,
                    TopologyRevision,
                    NodeId));
        }

        private static void AssertMalformedDigitalIO(
            byte[] payload,
            LMCDigitalIOReadRequest request)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseDigitalIO(
                    TestFrame.Response(0, payload),
                    RequestId,
                    request));
        }

        private static FakeRpcStep DigitalOutputWriteCapabilitiesStep(
            uint requestId)
        {
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(
                        requestId,
                        LMCDiagnosticCapability.EtherCATTopology
                            | LMCDiagnosticCapability.EtherCATNodeHealth
                            | LMCDiagnosticCapability.DigitalIORead
                            | LMCDiagnosticCapability.DigitalIOWrite,
                        DiagnosticsBootId)));
        }

        private static FakeRpcStep TopologyIoCapabilitiesStep(
            uint requestId,
            LMCDiagnosticCapability capabilities,
            ushort maxRequestPayloadBytes = 1320,
            ushort maxResponsePayloadBytes = 2040,
            ushort maxChunkDataBytes = 1280)
        {
            return new FakeRpcStep(
                LMC_CommandId.GetDiagnosticsCapabilities,
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(
                        requestId,
                        capabilities,
                        0,
                        maxRequestPayloadBytes,
                        maxResponsePayloadBytes,
                        maxChunkDataBytes)));
        }

        private static FakeRpcStep CreateDigitalOutputWriteSubmitStep(
            byte[] response)
        {
            return new FakeRpcStep(0x7E23, response)
            {
                InspectRequest = AssertDigitalOutputWriteSubmitRequest
            };
        }

        private static void AssertDigitalOutputWriteSubmitRequest(
            byte[] request)
        {
            AssertEx.Equal(48, request.Length);
            AssertEx.Equal((ushort)40, TestFrame.ReadUInt16(request, 4));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 6));
            AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 8));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 10));
            AssertEx.Equal(4u, TestFrame.ReadUInt32(request, 12));
            AssertEx.Equal(
                TopologyRevision,
                TestFrame.ReadUInt32(request, 16));
            AssertEx.Equal(IOReference, TestFrame.ReadUInt32(request, 20));
            AssertEx.Equal(1UL, TestFrame.ReadUInt64(request, 24));
            AssertEx.Equal(1UL, TestFrame.ReadUInt64(request, 32));
            AssertEx.Equal(0x01020304u, TestFrame.ReadUInt32(request, 40));
            AssertEx.Equal(DiagnosticsBootId, TestFrame.ReadUInt32(request, 44));
        }

        private static LMCDigitalOutputWriteRequest
            CreateBoundDigitalOutputWriteRequest(LMCConnection connection)
        {
            var shadow = connection.Diagnostics.ReadDigitalIO(
                CreateDigitalOutputTopology(connection),
                new LMCDigitalIOReadRequest(
                    TopologyRevision,
                    IOReference,
                    LMCDigitalIODirection.Output,
                    64));
            AssertEx.True(shadow.BelongsToCurrentSession(connection));
            AssertEx.True(shadow.HasValidatedTopologyBinding);
            return connection.Diagnostics.CreateDigitalOutputWriteRequest(
                shadow,
                1,
                1);
        }

        private static LMCEtherCATTopology CreateDigitalOutputTopology(
            LMCConnection connection = null)
        {
            var entries = new List<LMCEtherCATTopologyEntry>
            {
                new LMCEtherCATTopologyEntry(
                    NodeId,
                    0,
                    0,
                    ushort.MaxValue,
                    LMCEtherCATTopologyNodeKind.SlotModule,
                    LMCEtherCATTopologyNodeFlags.HasOutputs
                        | LMCEtherCATTopologyNodeFlags.HasDigitalIO,
                    0,
                    0,
                    0,
                    0x0000029Du,
                    0x47543242u,
                    1,
                    1,
                    0,
                    8,
                    "OutputSlot",
                    IOReference)
            };
            var info = new LMCEtherCATTopologyInfo(
                null,
                TopologyRevision,
                1,
                96,
                1,
                0,
                1,
                0,
                0x0000000Fu,
                (uint)LMCDiagnosticsCrcKind.Crc32IsoHdlc);
            var topology = new LMCEtherCATTopology(info, entries);
            return connection == null
                ? topology
                : topology.BindProvenance(
                    connection.Diagnostics,
                    connection.SessionGeneration);
        }

        private static LMCEtherCATTopology CreatePinnedReadTopology(
            LMCConnection connection = null)
        {
            var entries = new List<LMCEtherCATTopologyEntry>
            {
                new LMCEtherCATTopologyEntry(
                    NodeId,
                    0,
                    0,
                    1,
                    LMCEtherCATTopologyNodeKind.EtherCATSlave,
                    LMCEtherCATTopologyNodeFlags.HasMasterSlaveIndex
                        | LMCEtherCATTopologyNodeFlags.SupportsSdo
                        | LMCEtherCATTopologyNodeFlags.PhysicalAxis
                        | LMCEtherCATTopologyNodeFlags.Ds402Drive
                        | LMCEtherCATTopologyNodeFlags.HasInputs
                        | LMCEtherCATTopologyNodeFlags.HasDigitalIO,
                    1,
                    1,
                    ushort.MaxValue,
                    0x0000009Au,
                    0x00001001u,
                    1,
                    1,
                    8,
                    0,
                    "axis-input-slave",
                    IOReference)
            };
            var info = new LMCEtherCATTopologyInfo(
                null,
                TopologyRevision,
                1,
                96,
                1,
                1,
                0,
                1,
                0x0000000Fu,
                (uint)LMCDiagnosticsCrcKind.Crc32IsoHdlc);
            var topology = new LMCEtherCATTopology(info, entries);
            return connection == null
                ? topology
                : topology.BindProvenance(
                    connection.Diagnostics,
                    connection.SessionGeneration);
        }

        private static LMCDigitalIOReadRequest
            CreatePinnedDigitalInputRequest()
        {
            return new LMCDigitalIOReadRequest(
                TopologyRevision,
                IOReference,
                LMCDigitalIODirection.Input,
                64);
        }

        private static LMCOperationTicket
            InvokeDigitalOutputWriteWithPolicy(
            LMCConnection connection,
            LMCDigitalOutputWriteRequest request,
            bool useAsync)
        {
            AssertEx.True(request.BelongsToCurrentSession(connection));
            var predicate = new Func<uint, bool>(ioReference =>
                ioReference == IOReference);
            var parameterTypes = useAsync
                ? new[]
                {
                    typeof(LMCDigitalOutputWriteRequest),
                    typeof(CancellationToken),
                    typeof(Func<uint, bool>)
                }
                : new[]
                {
                    typeof(LMCDigitalOutputWriteRequest),
                    typeof(Func<uint, bool>)
                };
            var method = typeof(LMCDiagnostics).GetMethod(
                useAsync
                    ? "SubmitDigitalOutputWriteAsync"
                    : "SubmitDigitalOutputWrite",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                parameterTypes,
                null);
            AssertEx.True(
                method != null,
                "The private output-write wrapper policy seam was not found.");

            try
            {
                if (useAsync)
                {
                    var task = (Task<LMCOperationTicket>)method.Invoke(
                        connection.Diagnostics,
                        new object[]
                        {
                            request,
                            CancellationToken.None,
                            predicate
                        });
                    return task.GetAwaiter().GetResult();
                }

                return (LMCOperationTicket)method.Invoke(
                    connection.Diagnostics,
                    new object[] { request, predicate });
            }
            catch (TargetInvocationException error)
                when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }

        private static LMCDigitalOutputWriteSubmissionFailureContext
            RequireDigitalOutputWriteFailureContext(Exception error)
        {
            LMCDigitalOutputWriteSubmissionFailureContext context;
            AssertEx.True(
                LMCDigitalOutputWriteSubmissionFailureContext.TryGet(
                    error,
                    out context),
                "Expected a digital-output submission failure context.");
            return context;
        }

        private static byte[] DigitalOutputWriteDomainErrorPayload(
            uint requestId)
        {
            var payload = CommonPayload(16, requestId);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -32000);
            TestFrame.WriteUInt32(
                payload,
                12,
                (uint)LMCDiagnosticsDetailCode.InvalidState);
            return payload;
        }

        private static void ConnectTopologyIoTestConnection(
            LMCConnection connection,
            int port)
        {
            connection.RpcInitConnection(
                "127.0.0.1",
                port,
                "127.0.0.1",
                0,
                LMCConnection.DefaultEventMask);
        }

        private static void AssertCapabilityOff(Action<LMCDiagnostics> action)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCDiagnosticCapability.None,
                            0))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                AssertEx.Throws<NotSupportedException>(
                    () => action(connection.Diagnostics));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return new FakeRpcStep(0x8080, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CallbackStep()
        {
            return new FakeRpcStep(
                0x405C,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }
    }
}
