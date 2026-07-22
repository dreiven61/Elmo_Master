using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsD1ContractTests
    {
        private const uint GoldenRequestId = 0x11223344u;
        private const uint MapRevision = 0x957F101Eu;
        private const uint FirstSignalId = 0x00100101u;
        private const uint PositionSignalId = 0x00100104u;
        private const uint StatusSignalId = 0x00100106u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Request.DiagnosticsD1.GoldenBytes",
                DiagnosticsD1RequestGoldenBytes);
            tests.Add(
                "Response.SignalCatalog.GoldenFields",
                SignalCatalogGoldenFields);
            tests.Add(
                "Response.SignalCatalog.MalformedRejected",
                SignalCatalogMalformedRejected);
            tests.Add(
                "Response.SignalCatalog.CanonicalCrc",
                SignalCatalogCanonicalCrc);
            tests.Add(
                "Response.EtherCATHealth.GoldenFields",
                EtherCATHealthGoldenFields);
            tests.Add(
                "Response.EtherCATHealth.MalformedRejected",
                EtherCATHealthMalformedRejected);
            tests.Add(
                "Response.ReadPI.GoldenAndMalformed",
                ReadPIGoldenAndMalformed);
            tests.Add(
                "Rpc.DiagnosticsD1.SyncAndAsync",
                DiagnosticsD1SyncAndAsync);
            tests.Add(
                "Rpc.DiagnosticsD1.CapabilityRequiredBeforeCommand",
                DiagnosticsD1CapabilityRequiredBeforeCommand);
            tests.Add(
                "Rpc.SignalCatalog.CrcMismatchRejected",
                SignalCatalogCrcMismatchRejected);
            tests.Add(
                "Rpc.DiagnosticsD1.CatalogCacheInvalidated",
                DiagnosticsD1CatalogCacheInvalidated);
        }

        private static void DiagnosticsD1RequestGoldenBytes()
        {
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "01 7E 00 00 08 00 00 00 "
                    + "01 00 00 00 44 33 22 11"),
                LMC_DiagnosticsFrame.GetSignalCatalogInfo(GoldenRequestId));

            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "02 7E 00 00 10 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "1E 10 7F 95 10 00 08 00"),
                LMC_DiagnosticsFrame.GetSignalCatalogChunk(
                    GoldenRequestId,
                    MapRevision,
                    16,
                    8));

            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "10 7E 00 00 08 00 00 00 "
                    + "01 00 00 00 44 33 22 11"),
                LMC_DiagnosticsFrame.ReadEtherCATHealth(GoldenRequestId));

            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "20 7E 00 00 14 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "1E 10 7F 95 04 01 10 00 "
                    + "04 00 00 00"),
                LMC_DiagnosticsFrame.ReadPI(
                    GoldenRequestId,
                    MapRevision,
                    PositionSignalId,
                    LMCSignalValueType.Int32));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.GetSignalCatalogInfo(0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.GetSignalCatalogChunk(
                    GoldenRequestId,
                    MapRevision,
                    0,
                    0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.GetSignalCatalogChunk(
                    GoldenRequestId,
                    MapRevision,
                    0,
                    17));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ReadPI(
                    GoldenRequestId,
                    MapRevision,
                    0,
                    LMCSignalValueType.Invalid));

            using (var connection = new LMCConnection())
            {
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => connection.Diagnostics.ReadPI(
                        PositionSignalId,
                        0,
                        LMCSignalValueType.Int32));
            }
        }

        private static void SignalCatalogGoldenFields()
        {
            var info = LMC_DiagnosticsParser.ParseSignalCatalogInfo(
                TestFrame.Response(0, CatalogInfoPayload(GoldenRequestId, 24)),
                GoldenRequestId);

            AssertEx.Equal(MapRevision, info.MapRevision);
            AssertEx.Equal((ushort)24, info.TotalCount);
            AssertEx.Equal((ushort)80, info.EntryStride);
            AssertEx.Equal((ushort)40, info.AliasBytes);
            AssertEx.Equal((ushort)4, info.SignalIdBytes);
            AssertEx.Equal(
                LMCSignalCatalogFlags.FixedStride
                    | LMCSignalCatalogFlags.AliasAscii7Bit
                    | LMCSignalCatalogFlags.CanonicalCrc
                    | LMCSignalCatalogFlags.OpaqueSignalId,
                info.CatalogFlags);
            AssertEx.Equal(
                LMCDiagnosticsCrcKind.Crc32IsoHdlc,
                info.CrcKind);

            var chunk = LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                TestFrame.Response(
                    0,
                    CatalogChunkPayload(GoldenRequestId, 0, 16, 24)),
                GoldenRequestId,
                MapRevision,
                0,
                16);

            AssertEx.Equal((ushort)16, chunk.ReturnedCount);
            AssertEx.Equal((ushort)24, chunk.TotalCount);
            AssertEx.Equal(FirstSignalId, chunk.Entries[0].SignalId);
            AssertEx.Equal((ushort)0, chunk.Entries[0].CatalogIndex);
            AssertEx.Equal(LMCSignalSourceKind.PdoOutputLastTx, chunk.Entries[0].SourceKind);
            AssertEx.Equal(LMCSignalValueType.Int32, chunk.Entries[0].DataType);
            AssertEx.Equal((byte)4, chunk.Entries[0].ByteWidth);
            AssertEx.Equal((ushort)0x607A, chunk.Entries[0].PdoIndex);
            AssertEx.Equal(LMCPdoDirection.MasterToDrive, chunk.Entries[0].PdoDirection);
            AssertEx.Equal("axis1.target_position_last_tx", chunk.Entries[0].Alias);
            AssertEx.Equal(PositionSignalId, chunk.Entries[3].SignalId);
            AssertEx.Equal(LMCSignalSourceKind.PdoInput, chunk.Entries[3].SourceKind);
            AssertEx.Equal("axis1.actual_position", chunk.Entries[3].Alias);
            AssertEx.Equal(StatusSignalId, chunk.Entries[5].SignalId);
            AssertEx.Equal(LMCSignalValueType.BitField16, chunk.Entries[5].DataType);
            AssertEx.Equal("axis1.status_word", chunk.Entries[5].Alias);

            var currentRevisionChunk = LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                TestFrame.Response(
                    0,
                    CatalogChunkPayload(GoldenRequestId, 0, 1, 24)),
                GoldenRequestId,
                0,
                0,
                1);
            AssertEx.Equal(MapRevision, currentRevisionChunk.MapRevision);

            var emptyLastChunk = LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                TestFrame.Response(
                    0,
                    CatalogChunkPayload(GoldenRequestId, 24, 0, 24)),
                GoldenRequestId,
                MapRevision,
                24,
                1);
            AssertEx.Equal((ushort)0, emptyLastChunk.ReturnedCount);
        }

        private static void SignalCatalogMalformedRejected()
        {
            var wrongInfoStride = CatalogInfoPayload(GoldenRequestId, 24);
            TestFrame.WriteUInt16(wrongInfoStride, 22, 79);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogInfo(
                    TestFrame.Response(0, wrongInfoStride),
                    GoldenRequestId));

            var zeroRevisionInfo = CatalogInfoPayload(GoldenRequestId, 24);
            TestFrame.WriteUInt32(zeroRevisionInfo, 16, 0);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogInfo(
                    TestFrame.Response(0, zeroRevisionInfo),
                    GoldenRequestId));

            var domainError = CommonPayload(16, GoldenRequestId);
            TestFrame.WriteUInt16(domainError, 4, 1);
            TestFrame.WriteInt16(domainError, 6, -32000);
            TestFrame.WriteUInt32(
                domainError,
                12,
                (uint)LMCDiagnosticsDetailCode.MapRevisionMismatch);
            var exception = AssertEx.Throws<LMCDiagnosticsCommandException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogInfo(
                    TestFrame.Response(0, domainError),
                    GoldenRequestId));
            AssertEx.Equal(
                LMCDiagnosticsDetailCode.MapRevisionMismatch,
                exception.Response.Detail);

            var extendedDomainError = new byte[17];
            Buffer.BlockCopy(domainError, 0, extendedDomainError, 0, domainError.Length);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogInfo(
                    TestFrame.Response(0, extendedDomainError),
                    GoldenRequestId));

            var wrongMap = CatalogChunkPayload(GoldenRequestId, 0, 1, 1);
            TestFrame.WriteUInt32(wrongMap, 16, MapRevision + 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                    TestFrame.Response(0, wrongMap),
                    GoldenRequestId,
                    MapRevision,
                    0,
                    1));

            var invalidAlias = CatalogChunkPayload(GoldenRequestId, 0, 1, 1);
            invalidAlias[28 + 36] = 0x80;
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                    TestFrame.Response(0, invalidAlias),
                    GoldenRequestId,
                    MapRevision,
                    0,
                    1));

            var partial = CatalogChunkPayload(GoldenRequestId, 0, 1, 1);
            TestFrame.WriteUInt16(
                partial,
                2,
                (ushort)(LMCDiagnosticsResponseFlags.Partial
                    | LMCDiagnosticsResponseFlags.LastChunk));
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                    TestFrame.Response(0, partial),
                    GoldenRequestId,
                    MapRevision,
                    0,
                    1));

            var shortNonFinal = CatalogChunkPayload(GoldenRequestId, 0, 1, 24);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                    TestFrame.Response(0, shortNonFinal),
                    GoldenRequestId,
                    MapRevision,
                    0,
                    2));

            var missingLast = CatalogChunkPayload(GoldenRequestId, 16, 8, 24);
            TestFrame.WriteUInt16(missingLast, 2, 0);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                    TestFrame.Response(0, missingLast),
                    GoldenRequestId,
                    MapRevision,
                    16,
                    8));

            var earlyLast = CatalogChunkPayload(GoldenRequestId, 0, 16, 24);
            TestFrame.WriteUInt16(
                earlyLast,
                2,
                (ushort)LMCDiagnosticsResponseFlags.LastChunk);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                    TestFrame.Response(0, earlyLast),
                    GoldenRequestId,
                    MapRevision,
                    0,
                    16));

            var zeroScale = CatalogChunkPayload(GoldenRequestId, 0, 1, 1);
            TestFrame.WriteInt32(zeroScale, 28 + 24, 0);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                    TestFrame.Response(0, zeroScale),
                    GoldenRequestId,
                    MapRevision,
                    0,
                    1));

            var sdoOnlyValueType = CatalogChunkPayload(
                GoldenRequestId,
                0,
                1,
                1);
            sdoOnlyValueType[28 + 8] = (byte)LMCSignalValueType.Int8;
            sdoOnlyValueType[28 + 9] = 1;
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                    TestFrame.Response(0, sdoOnlyValueType),
                    GoldenRequestId,
                    MapRevision,
                    0,
                    1));

            var nonzeroReserved = CatalogChunkPayload(GoldenRequestId, 0, 1, 1);
            TestFrame.WriteUInt32(nonzeroReserved, 28 + 76, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                    TestFrame.Response(0, nonzeroReserved),
                    GoldenRequestId,
                    MapRevision,
                    0,
                    1));

            var compatibleSystem = CatalogChunkPayload(GoldenRequestId, 0, 1, 1);
            var entryOffset = 28;
            compatibleSystem[entryOffset + 6] = (byte)LMCSignalSourceKind.System;
            compatibleSystem[entryOffset + 7] = 0;
            compatibleSystem[entryOffset + 8] = (byte)LMCSignalValueType.UInt32;
            compatibleSystem[entryOffset + 9] = 4;
            TestFrame.WriteUInt16(compatibleSystem, entryOffset + 10, 0);
            TestFrame.WriteUInt16(
                compatibleSystem,
                entryOffset + 14,
                (ushort)(LMCSignalFlags.InputMappedPhase
                    | LMCSignalFlags.HealthSignal));
            TestFrame.WriteUInt16(compatibleSystem, entryOffset + 16, 0);
            compatibleSystem[entryOffset + 18] = 0;
            compatibleSystem[entryOffset + 19] = (byte)LMCPdoDirection.None;
            TestFrame.WriteInt32(compatibleSystem, entryOffset + 28, 0);
            TestFrame.WriteInt32(compatibleSystem, entryOffset + 32, -1);
            Array.Clear(compatibleSystem, entryOffset + 36, 40);
            WriteAscii(compatibleSystem, entryOffset + 36, "system.custom_counter");
            var compatibleChunk = LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                TestFrame.Response(0, compatibleSystem),
                GoldenRequestId,
                MapRevision,
                0,
                1);
            AssertEx.Equal(
                LMCSignalSourceKind.System,
                compatibleChunk.Entries[0].SourceKind);

            var truncated = CatalogChunkPayload(GoldenRequestId, 0, 1, 1);
            Array.Resize(ref truncated, truncated.Length - 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                    TestFrame.Response(0, truncated),
                    GoldenRequestId,
                    MapRevision,
                    0,
                    1));
        }

        private static void SignalCatalogCanonicalCrc()
        {
            var canonical = new byte[24 * 80];
            for (ushort catalogIndex = 0; catalogIndex < 24; catalogIndex++)
            {
                WriteCatalogEntry(
                    canonical,
                    catalogIndex * 80,
                    catalogIndex);
            }

            AssertEx.Equal(MapRevision, Crc32IsoHdlc(canonical));
        }

        private static void EtherCATHealthGoldenFields()
        {
            var health = LMC_DiagnosticsParser.ParseEtherCATHealth(
                TestFrame.Response(0, HealthPayload(GoldenRequestId)),
                GoldenRequestId);

            AssertEx.Equal(MapRevision, health.MapRevision);
            AssertEx.Equal(LMCCapturePhase.InputMapped, health.CapturePhase);
            AssertEx.Equal(100u, health.CycleCounter);
            AssertEx.Equal(0x0000000200000001ul, health.TimestampUs);
            AssertEx.Equal((ushort)8, health.MasterState);
            AssertEx.Equal(
                LMCEtherCATMasterFlags.MasterOperational,
                health.MasterFlags);
            AssertEx.Equal(0u, health.ConsecutiveInvalidCycles);
            AssertEx.Equal(7u, health.InvalidCycleTotal);
            AssertEx.Equal(150u, health.FrameTimeUs);
            AssertEx.Equal(250u, health.FrameTimeMaxUs);
            AssertEx.Equal(300u, health.RtTimeUs);
            AssertEx.Equal(450u, health.RtTimeMaxUs);
            AssertEx.Equal(10u, health.SnapshotSequence);
            AssertEx.Equal(4, health.Slaves.Count);
            AssertEx.Equal((ushort)0, health.Slaves[0].SlaveIndex);
            AssertEx.Equal((ushort)1, health.Slaves[0].PhysicalAxis);
            AssertEx.True(health.Slaves[0].Online);
            AssertEx.Equal((byte)8, health.Slaves[0].EtherCATState);
            AssertEx.Equal((ushort)0, health.Slaves[0].ALStatusCode);
            AssertEx.Equal(99u, health.Slaves[1].LastStateChangeCycle);
        }

        private static void EtherCATHealthMalformedRejected()
        {
            var zeroMapRevision = HealthPayload(GoldenRequestId);
            TestFrame.WriteUInt32(zeroMapRevision, 16, 0);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATHealth(
                    TestFrame.Response(0, zeroMapRevision),
                    GoldenRequestId));

            var oddSequence = HealthPayload(GoldenRequestId);
            TestFrame.WriteUInt32(oddSequence, 64, 11);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATHealth(
                    TestFrame.Response(0, oddSequence),
                    GoldenRequestId));

            var wrongPhase = HealthPayload(GoldenRequestId);
            TestFrame.WriteUInt16(wrongPhase, 20, 2);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATHealth(
                    TestFrame.Response(0, wrongPhase),
                    GoldenRequestId));

            var invalidOnline = HealthPayload(GoldenRequestId);
            invalidOnline[72 + 4] = 2;
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATHealth(
                    TestFrame.Response(0, invalidOnline),
                    GoldenRequestId));

            var wrongSlaveCount = HealthPayload(GoldenRequestId);
            TestFrame.WriteUInt16(wrongSlaveCount, 22, 3);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATHealth(
                    TestFrame.Response(0, wrongSlaveCount),
                    GoldenRequestId));

            var invalidEtherCATState = HealthPayload(GoldenRequestId);
            invalidEtherCATState[72 + 5] = 5;
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATHealth(
                    TestFrame.Response(0, invalidEtherCATState),
                    GoldenRequestId));

            var nonzeroReserved = HealthPayload(GoldenRequestId);
            TestFrame.WriteUInt16(nonzeroReserved, 70, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATHealth(
                    TestFrame.Response(0, nonzeroReserved),
                    GoldenRequestId));

            var wrongSlaveIndex = HealthPayload(GoldenRequestId);
            TestFrame.WriteUInt16(wrongSlaveIndex, 72, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATHealth(
                    TestFrame.Response(0, wrongSlaveIndex),
                    GoldenRequestId));

            var wrongPhysicalAxis = HealthPayload(GoldenRequestId);
            TestFrame.WriteUInt16(wrongPhysicalAxis, 72 + 2, 2);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATHealth(
                    TestFrame.Response(0, wrongPhysicalAxis),
                    GoldenRequestId));

            var truncated = HealthPayload(GoldenRequestId);
            Array.Resize(ref truncated, truncated.Length - 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseEtherCATHealth(
                    TestFrame.Response(0, truncated),
                    GoldenRequestId));
        }

        private static void ReadPIGoldenAndMalformed()
        {
            var value = LMC_DiagnosticsParser.ParsePI(
                TestFrame.Response(0, PIPayload(GoldenRequestId)),
                GoldenRequestId,
                MapRevision,
                PositionSignalId,
                LMCSignalValueType.Int32);

            AssertEx.Equal(MapRevision, value.MapRevision);
            AssertEx.Equal(LMCCapturePhase.InputMapped, value.CapturePhase);
            AssertEx.Equal(100u, value.CycleCounter);
            AssertEx.Equal(0x0000000200000001ul, value.TimestampUs);
            AssertEx.Equal(PositionSignalId, value.SignalId);
            AssertEx.Equal(unchecked((uint)-12345), value.RawValue32);
            AssertEx.Equal(-12345, value.RawInt32);
            AssertEx.Equal(LMCSignalValueType.Int32, value.ValueType);
            AssertEx.Equal(LMCSignalEntryStatus.Valid, value.EntryStatus);
            AssertEx.True(value.IsValid);

            var wrongSignal = PIPayload(GoldenRequestId);
            TestFrame.WriteUInt32(wrongSignal, 36, StatusSignalId);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParsePI(
                    TestFrame.Response(0, wrongSignal),
                    GoldenRequestId,
                    MapRevision,
                    PositionSignalId,
                    LMCSignalValueType.Int32));

            var validAndStale = PIPayload(GoldenRequestId);
            validAndStale[45] = (byte)(LMCSignalEntryStatus.Valid
                | LMCSignalEntryStatus.StaleFrame);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParsePI(
                    TestFrame.Response(0, validAndStale),
                    GoldenRequestId,
                    MapRevision,
                    PositionSignalId,
                    LMCSignalValueType.Int32));

            var nonzeroHeaderReserved = PIPayload(GoldenRequestId);
            TestFrame.WriteUInt16(nonzeroHeaderReserved, 22, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParsePI(
                    TestFrame.Response(0, nonzeroHeaderReserved),
                    GoldenRequestId,
                    MapRevision,
                    PositionSignalId,
                    LMCSignalValueType.Int32));

            var nonzeroEntryReserved = PIPayload(GoldenRequestId);
            TestFrame.WriteUInt16(nonzeroEntryReserved, 46, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParsePI(
                    TestFrame.Response(0, nonzeroEntryReserved),
                    GoldenRequestId,
                    MapRevision,
                    PositionSignalId,
                    LMCSignalValueType.Int32));

            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParsePI(
                    TestFrame.Response(
                        0,
                        PIPayload(GoldenRequestId),
                        1),
                    GoldenRequestId,
                    MapRevision,
                    PositionSignalId,
                    LMCSignalValueType.Int32));

            var currentValue = LMC_DiagnosticsParser.ParsePI(
                TestFrame.Response(0, PIPayload(GoldenRequestId)),
                GoldenRequestId,
                0,
                PositionSignalId,
                LMCSignalValueType.Int32);
            AssertEx.Equal(MapRevision, currentValue.MapRevision);

            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParsePI(
                    TestFrame.Response(0, PIPayload(GoldenRequestId)),
                    GoldenRequestId,
                    MapRevision,
                    PositionSignalId,
                    LMCSignalValueType.UInt32));

            var validSigned16 = PIPayload(GoldenRequestId);
            TestFrame.WriteUInt32(validSigned16, 40, 0xFFFFFFFFu);
            validSigned16[44] = (byte)LMCSignalValueType.Int16;
            LMC_DiagnosticsParser.ParsePI(
                TestFrame.Response(0, validSigned16),
                GoldenRequestId,
                MapRevision,
                PositionSignalId,
                LMCSignalValueType.Int16);

            var invalidSigned16 = PIPayload(GoldenRequestId);
            TestFrame.WriteUInt32(invalidSigned16, 40, 0x0000FFFFu);
            invalidSigned16[44] = (byte)LMCSignalValueType.Int16;
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParsePI(
                    TestFrame.Response(0, invalidSigned16),
                    GoldenRequestId,
                    MapRevision,
                    PositionSignalId,
                    LMCSignalValueType.Int16));

            var invalidUnsigned16 = PIPayload(GoldenRequestId);
            TestFrame.WriteUInt32(invalidUnsigned16, 40, 0x00010001u);
            invalidUnsigned16[44] = (byte)LMCSignalValueType.BitField16;
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParsePI(
                    TestFrame.Response(0, invalidUnsigned16),
                    GoldenRequestId,
                    MapRevision,
                    PositionSignalId,
                    LMCSignalValueType.BitField16));

            var invalidBool = PIPayload(GoldenRequestId);
            TestFrame.WriteUInt32(invalidBool, 40, 2);
            invalidBool[44] = (byte)LMCSignalValueType.Bool;
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParsePI(
                    TestFrame.Response(0, invalidBool),
                    GoldenRequestId,
                    MapRevision,
                    PositionSignalId,
                    LMCSignalValueType.Bool));
        }

        private static void DiagnosticsD1SyncAndAsync()
        {
            RunDiagnosticsD1Integration(false);
            RunDiagnosticsD1Integration(true);
        }

        private static void DiagnosticsD1CapabilityRequiredBeforeCommand()
        {
            AssertD1CapabilityRejected(
                LMCDiagnosticCapability.None,
                diagnostics => diagnostics.GetSignalCatalogInfo(),
                "SignalCatalog");
            AssertD1CapabilityRejected(
                LMCDiagnosticCapability.None,
                diagnostics => diagnostics.GetSignalCatalogChunkAsync(
                        MapRevision,
                        0,
                        1,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult(),
                "SignalCatalog");
            AssertD1CapabilityRejected(
                LMCDiagnosticCapability.None,
                diagnostics => diagnostics.GetSignalCatalog(),
                "SignalCatalog");
            AssertD1CapabilityRejected(
                LMCDiagnosticCapability.None,
                diagnostics => diagnostics.ReadEtherCATHealthAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult(),
                "EtherCATHealth");
            AssertD1CapabilityRejected(
                LMCDiagnosticCapability.SignalCatalog,
                diagnostics => diagnostics.ReadPI(PositionSignalId),
                "PIRead");
        }

        private static void AssertD1CapabilityRejected(
            LMCDiagnosticCapability advertisedCapabilities,
            Action<LMCDiagnostics> invoke,
            string expectedCapabilityName)
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
                            advertisedCapabilities))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<NotSupportedException>(
                    () => invoke(connection.Diagnostics));
                AssertEx.Contains(expectedCapabilityName, exception.Message);
                AssertEx.True(connection.IsConnected);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunDiagnosticsD1Integration(bool useAsync)
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
                            LMCDiagnosticCapability.EtherCATHealth
                                | LMCDiagnosticCapability.SignalCatalog
                                | LMCDiagnosticCapability.PIRead))),
                new FakeRpcStep(
                    0x7E01,
                    TestFrame.Response(0, CatalogInfoPayload(2, 24))),
                new FakeRpcStep(
                    0x7E02,
                    TestFrame.Response(0, CatalogChunkPayload(3, 0, 16, 24))),
                new FakeRpcStep(
                    0x7E02,
                    TestFrame.Response(0, CatalogChunkPayload(4, 16, 8, 24))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            5,
                            LMCDiagnosticCapability.EtherCATHealth
                                | LMCDiagnosticCapability.SignalCatalog
                                | LMCDiagnosticCapability.PIRead))),
                new FakeRpcStep(
                    0x7E10,
                    TestFrame.Response(0, HealthPayload(6))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            7,
                            LMCDiagnosticCapability.EtherCATHealth
                                | LMCDiagnosticCapability.SignalCatalog
                                | LMCDiagnosticCapability.PIRead))),
                new FakeRpcStep(
                    0x7E20,
                    TestFrame.Response(0, PIPayload(8))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                LMCSignalCatalog catalog;
                LMCEtherCATHealth health;
                LMCSignalValue value;

                if (useAsync)
                {
                    catalog = connection.Diagnostics.GetSignalCatalogAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    health = connection.Diagnostics.ReadEtherCATHealthAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    value = connection.Diagnostics.ReadPIAsync(
                            PositionSignalId,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    catalog = connection.Diagnostics.GetSignalCatalog();
                    health = connection.Diagnostics.ReadEtherCATHealth();
                    value = connection.Diagnostics.ReadPI(PositionSignalId);
                }

                AssertEx.Equal(24, catalog.Entries.Count);
                AssertEx.Equal(
                    PositionSignalId,
                    catalog.GetByAlias("axis1.actual_position").SignalId);
                LMCSignalCatalogEntry missing;
                AssertEx.False(catalog.TryGetByAlias("Axis1.Actual_Position", out missing));
                AssertEx.Equal(4, health.Slaves.Count);
                AssertEx.Equal(-12345, value.RawInt32);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void SignalCatalogCrcMismatchRejected()
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
                            LMCDiagnosticCapability.SignalCatalog,
                            1))),
                new FakeRpcStep(
                    0x7E01,
                    TestFrame.Response(0, CatalogInfoPayload(2, 1))),
                new FakeRpcStep(
                    0x7E02,
                    TestFrame.Response(0, CatalogChunkPayload(3, 0, 1, 1))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<InvalidDataException>(
                    () => connection.Diagnostics.GetSignalCatalog());
                AssertEx.Contains("canonical CRC", exception.Message);
                AssertEx.True(connection.IsConnected);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void DiagnosticsD1CatalogCacheInvalidated()
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
                            LMCDiagnosticCapability.SignalCatalog
                                | LMCDiagnosticCapability.PIRead))),
                new FakeRpcStep(
                    0x7E01,
                    TestFrame.Response(0, CatalogInfoPayload(2, 24))),
                new FakeRpcStep(
                    0x7E20,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            3,
                            LMCDiagnosticsDetailCode.MapRevisionMismatch))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            4,
                            LMCDiagnosticCapability.SignalCatalog
                                | LMCDiagnosticCapability.PIRead))),
                new FakeRpcStep(
                    0x7E01,
                    TestFrame.Response(0, CatalogInfoPayload(5, 24))),
                new FakeRpcStep(
                    0x7E20,
                    TestFrame.Response(0, PIPayload(6))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => connection.Diagnostics.ReadPI(PositionSignalId));
                AssertEx.Equal(
                    LMCDiagnosticsDetailCode.MapRevisionMismatch,
                    exception.Response.Detail);

                var value = connection.Diagnostics.ReadPI(PositionSignalId);
                AssertEx.Equal(-12345, value.RawInt32);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static byte[] CapabilitiesPayload(
            uint requestId,
            LMCDiagnosticCapability capabilities,
            ushort catalogEntryCount = 24)
        {
            var payload = CommonPayload(68, requestId);
            var hasD1Capability = capabilities != LMCDiagnosticCapability.None;
            var hasCatalog = (capabilities
                & LMCDiagnosticCapability.SignalCatalog) != 0;

            TestFrame.WriteUInt32(payload, 16, 2);
            TestFrame.WriteUInt32(payload, 20, (uint)capabilities);
            TestFrame.WriteUInt32(
                payload,
                24,
                hasD1Capability ? MapRevision : 0);
            TestFrame.WriteUInt16(
                payload,
                28,
                hasCatalog ? catalogEntryCount : (ushort)0);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt16(payload, 50, 80);
            TestFrame.WriteUInt16(payload, 52, 16);
            return payload;
        }

        private static byte[] CatalogInfoPayload(uint requestId, ushort totalCount)
        {
            var payload = CommonPayload(36, requestId);
            TestFrame.WriteUInt32(payload, 16, MapRevision);
            TestFrame.WriteUInt16(payload, 20, totalCount);
            TestFrame.WriteUInt16(payload, 22, 80);
            TestFrame.WriteUInt16(payload, 24, 40);
            TestFrame.WriteUInt16(payload, 26, 4);
            TestFrame.WriteUInt32(payload, 28, 0x0000000Fu);
            TestFrame.WriteUInt32(payload, 32, 1);
            return payload;
        }

        private static byte[] CatalogChunkPayload(
            uint requestId,
            ushort startIndex,
            ushort returnedCount,
            ushort totalCount)
        {
            var payload = CommonPayload(
                28 + returnedCount * 80,
                requestId,
                startIndex + returnedCount == totalCount ? (ushort)2 : (ushort)0);
            TestFrame.WriteUInt32(payload, 16, MapRevision);
            TestFrame.WriteUInt16(payload, 20, startIndex);
            TestFrame.WriteUInt16(payload, 22, returnedCount);
            TestFrame.WriteUInt16(payload, 24, totalCount);
            TestFrame.WriteUInt16(payload, 26, 80);

            for (var index = 0; index < returnedCount; index++)
            {
                WriteCatalogEntry(
                    payload,
                    28 + index * 80,
                    checked((ushort)(startIndex + index)));
            }

            return payload;
        }

        private static void WriteCatalogEntry(
            byte[] payload,
            int offset,
            ushort catalogIndex)
        {
            var physicalAxis = catalogIndex / 6 + 1;
            var signalCode = catalogIndex % 6 + 1;
            var signalId = 0x00100000u
                | ((uint)physicalAxis << 8)
                | (uint)signalCode;
            var isPosition = signalCode == 1 || signalCode == 4;
            var isOutput = signalCode <= 3;
            var isBitField16 = signalCode == 3 || signalCode == 6;
            var isBitField32 = signalCode == 2 || signalCode == 5;
            var dataType = isPosition
                ? LMCSignalValueType.Int32
                : isBitField16
                    ? LMCSignalValueType.BitField16
                    : LMCSignalValueType.BitField32;
            var aliasSuffixes = new[]
            {
                "target_position_last_tx",
                "digital_outputs_last_tx",
                "control_word_last_tx",
                "actual_position",
                "digital_inputs",
                "status_word"
            };
            var pdoIndices = new ushort[]
            {
                0x607A,
                0x60FE,
                0x6040,
                0x6064,
                0x60FD,
                0x6041
            };
            TestFrame.WriteUInt32(
                payload,
                offset,
                signalId);
            TestFrame.WriteUInt16(payload, offset + 4, catalogIndex);
            payload[offset + 6] = (byte)(isOutput
                ? LMCSignalSourceKind.PdoOutputLastTx
                : LMCSignalSourceKind.PdoInput);
            payload[offset + 7] = (byte)physicalAxis;
            payload[offset + 8] = (byte)dataType;
            payload[offset + 9] = isBitField16 ? (byte)2 : (byte)4;
            TestFrame.WriteUInt16(payload, offset + 10, isPosition ? (ushort)1 : (ushort)0);
            TestFrame.WriteUInt16(
                payload,
                offset + 12,
                (ushort)(LMCSignalAccessFlags.Readable
                    | LMCSignalAccessFlags.Recordable
                    | LMCSignalAccessFlags.BulkReadable));
            TestFrame.WriteUInt16(
                payload,
                offset + 14,
                (ushort)(LMCSignalFlags.ActivePdo
                    | LMCSignalFlags.PhysicalAxis
                    | LMCSignalFlags.InputMappedPhase));
            TestFrame.WriteUInt16(
                payload,
                offset + 16,
                pdoIndices[signalCode - 1]);
            payload[offset + 18] = signalCode == 2 ? (byte)1 : (byte)0;
            payload[offset + 19] = (byte)(isOutput
                ? LMCPdoDirection.MasterToDrive
                : LMCPdoDirection.DriveToMaster);
            TestFrame.WriteInt32(payload, offset + 20, 1);
            TestFrame.WriteInt32(payload, offset + 24, 1);
            TestFrame.WriteInt32(payload, offset + 28, isPosition ? int.MinValue : 0);
            TestFrame.WriteInt32(
                payload,
                offset + 32,
                isPosition ? int.MaxValue : isBitField16 ? 65535 : -1);
            WriteAscii(
                payload,
                offset + 36,
                "axis"
                    + physicalAxis
                    + "."
                    + aliasSuffixes[signalCode - 1]);
        }

        private static byte[] HealthPayload(uint requestId)
        {
            var payload = CommonPayload(72 + 4 * 32, requestId);
            TestFrame.WriteUInt32(payload, 16, MapRevision);
            TestFrame.WriteUInt16(payload, 20, (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt16(payload, 22, 4);
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(payload, 28, 1);
            TestFrame.WriteUInt32(payload, 32, 2);
            TestFrame.WriteUInt16(payload, 36, 8);
            TestFrame.WriteUInt16(
                payload,
                38,
                (ushort)LMCEtherCATMasterFlags.MasterOperational);
            TestFrame.WriteUInt32(payload, 40, 0);
            TestFrame.WriteUInt32(payload, 44, 7);
            TestFrame.WriteUInt32(payload, 48, 150);
            TestFrame.WriteUInt32(payload, 52, 250);
            TestFrame.WriteUInt32(payload, 56, 300);
            TestFrame.WriteUInt32(payload, 60, 450);
            TestFrame.WriteUInt32(payload, 64, 10);
            TestFrame.WriteUInt16(payload, 68, 32);

            WriteSlaveHealth(payload, 72, 0, 1, 99);
            WriteSlaveHealth(payload, 104, 1, 2, 99);
            WriteSlaveHealth(payload, 136, 2, 3, 99);
            WriteSlaveHealth(payload, 168, 3, 4, 99);
            return payload;
        }

        private static void WriteSlaveHealth(
            byte[] payload,
            int offset,
            ushort slaveIndex,
            ushort physicalAxis,
            uint lastStateChangeCycle)
        {
            TestFrame.WriteUInt16(payload, offset, slaveIndex);
            TestFrame.WriteUInt16(payload, offset + 2, physicalAxis);
            payload[offset + 4] = 1;
            payload[offset + 5] = 8;
            TestFrame.WriteUInt16(payload, offset + 6, 0);
            TestFrame.WriteUInt32(payload, offset + 8, 8);
            TestFrame.WriteUInt32(payload, offset + 12, 1);
            TestFrame.WriteUInt32(payload, offset + 16, 0x1237);
            TestFrame.WriteUInt32(payload, offset + 20, 0);
            TestFrame.WriteUInt32(payload, offset + 24, 100);
            TestFrame.WriteUInt32(payload, offset + 28, lastStateChangeCycle);
        }

        private static byte[] PIPayload(uint requestId)
        {
            var payload = CommonPayload(52, requestId);
            TestFrame.WriteUInt32(payload, 16, MapRevision);
            TestFrame.WriteUInt16(payload, 20, (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(payload, 28, 1);
            TestFrame.WriteUInt32(payload, 32, 2);
            TestFrame.WriteUInt32(payload, 36, PositionSignalId);
            TestFrame.WriteUInt32(payload, 40, unchecked((uint)-12345));
            payload[44] = (byte)LMCSignalValueType.Int32;
            payload[45] = (byte)LMCSignalEntryStatus.Valid;
            return payload;
        }

        private static byte[] CommonPayload(
            int length,
            uint requestId,
            ushort responseFlags = 0)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt16(payload, 2, responseFlags);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static byte[] DomainErrorPayload(
            uint requestId,
            LMCDiagnosticsDetailCode detailCode)
        {
            var payload = CommonPayload(16, requestId);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -32000);
            TestFrame.WriteUInt32(payload, 12, (uint)detailCode);
            return payload;
        }

        private static void WriteAscii(byte[] buffer, int offset, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
        }

        private static uint Crc32IsoHdlc(byte[] bytes)
        {
            var crc = 0xFFFFFFFFu;
            foreach (var value in bytes)
            {
                crc ^= value;
                for (var bit = 0; bit < 8; bit++)
                {
                    crc = (crc & 1u) != 0
                        ? (crc >> 1) ^ 0xEDB88320u
                        : crc >> 1;
                }
            }

            return crc ^ 0xFFFFFFFFu;
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
