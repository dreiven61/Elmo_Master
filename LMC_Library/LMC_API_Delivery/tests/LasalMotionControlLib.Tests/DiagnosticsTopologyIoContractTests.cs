using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsTopologyIoContractTests
    {
        private const uint RequestId = 0x11223344u;
        private const uint TopologyRevision = 0xA1B2C3D4u;
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
                "Rpc.DiagnosticsTopologyIo.CapabilityOffPreWire",
                CapabilityOffPreWire);
            tests.Add(
                "Rpc.DiagnosticsTopologyIo.DownloadAndRead",
                TopologyDownloadAndDigitalIORead);
            tests.Add(
                "Rpc.DigitalOutputWrite.EmptyAllowlistPreWire",
                DigitalOutputWriteEmptyAllowlistPreWire);
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
                diagnostics.SubmitDigitalOutputWrite(
                    new LMCDigitalOutputWriteRequest(
                        TopologyRevision,
                        IOReference,
                        0,
                        1,
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
            AssertCapabilityOff(diagnostics =>
                diagnostics.SubmitDigitalOutputWriteAsync(
                    new LMCDigitalOutputWriteRequest(
                        TopologyRevision,
                        IOReference,
                        0,
                        1,
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
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                AssertEx.Throws<UnauthorizedAccessException>(
                    () => connection.Diagnostics.SubmitDigitalOutputWrite(
                        new LMCDigitalOutputWriteRequest(
                            TopologyRevision,
                            IOReference,
                            0,
                            1,
                            1)));

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
                LMCEtherCATTopologyEntry ioNode;
                AssertEx.True(topology.TryGetNode(NodeId, out ioNode));
                AssertEx.Equal(IOReference, ioNode.IOReference);

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
            uint diagnosticsBootId)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 1);
            TestFrame.WriteUInt32(payload, 20, (uint)capabilities);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
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
