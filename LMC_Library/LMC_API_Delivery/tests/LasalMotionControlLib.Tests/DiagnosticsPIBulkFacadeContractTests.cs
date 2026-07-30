using System;
using System.Collections.Generic;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsPIBulkFacadeContractTests
    {
        private const uint MapRevision = 0x957F101Eu;
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const uint BulkId = 0xA1B2C3D4u;
        private const uint ConfigRevision = 0x01020304u;
        private const uint Signal1 = 0x00100104u;
        private const uint Signal2 = 0x00100105u;
        private const uint Signal3 = 0x00100106u;
        private const uint DeniedSignal = 0x00100107u;
        private const uint UnsupportedTypeSignal = 0x00100108u;
        private const uint WrongWidthSignal = 0x00100109u;

        private static readonly uint[] BulkSignals =
        {
            Signal1,
            Signal2
        };

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Facade.PIBulk.BuilderValidation",
                BuilderValidation);
            tests.Add(
                "Facade.PIBulk.SyncWireAndLocalGetEntry",
                SyncWireAndLocalGetEntry);
            tests.Add(
                "Facade.PIBulk.AsyncAndPreCancellation",
                AsyncAndPreCancellation);
            tests.Add(
                "Facade.PIBulk.ExactRevisionRejectedBeforeConfigure",
                ExactRevisionRejectedBeforeConfigure);
            tests.Add(
                "Facade.PIBulk.ReconnectRejectsReaderBeforeWire",
                ReconnectRejectsReaderBeforeWire);
            tests.Add(
                "Facade.PIBulk.CatalogProvenancePreWire",
                CatalogProvenancePreWire);
            tests.Add(
                "Facade.PI.AliasReadUsesD1Wire",
                AliasReadUsesD1Wire);
        }

        private static void BuilderValidation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var catalog = CreateCatalog(MapRevision, connection);
                var builder = connection.Diagnostics.CreatePIBulkBuilder(catalog);
                AssertEx.Equal(0, builder.Count);
                AssertEx.False(builder.IsFrozen);

                builder.AddEntry("axis1.actual_position");
                builder.AddEntry(Signal2);
                AssertEx.Equal(2, builder.Count);
                AssertEx.Equal(Signal1, builder.Entries[0].SignalId);
                AssertEx.Equal(Signal2, builder.Entries[1].SignalId);

                AssertEx.Throws<ArgumentException>(
                    () => builder.AddEntry(Signal1));
                AssertEx.Throws<InvalidOperationException>(
                    () => builder.AddEntry(Signal3));
                AssertEx.Throws<InvalidOperationException>(
                    () => builder.AddEntry(DeniedSignal));
                AssertEx.Throws<System.IO.InvalidDataException>(
                    () => builder.AddEntry(UnsupportedTypeSignal));
                AssertEx.Throws<System.IO.InvalidDataException>(
                    () => builder.AddEntry(WrongWidthSignal));
                AssertEx.Throws<KeyNotFoundException>(
                    () => builder.AddEntry(0x00FFFFFFu));

                AssertEx.True(builder.RemoveEntry(Signal2));
                AssertEx.False(builder.RemoveEntry(Signal2));
                builder.Clear();
                AssertEx.Equal(0, builder.Count);
                AssertEx.Throws<InvalidOperationException>(
                    () => builder.Configure());
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void SyncWireAndLocalGetEntry()
        {
            var configureStep = new FakeRpcStep(
                0x7E30,
                TestFrame.Response(
                    0,
                    BulkStatusPayload(2, LMCBulkState.Pending, 0)));
            configureStep.InspectRequest = request => AssertEx.SequenceEqual(
                LMC_DiagnosticsFrame.ConfigureBulk(
                    2,
                    MapRevision,
                    0,
                    BulkSignals),
                request);

            var statusStep = new FakeRpcStep(
                0x7E31,
                TestFrame.Response(
                    0,
                    BulkStatusPayload(3, LMCBulkState.Active, 100)));
            statusStep.InspectRequest = request => AssertEx.SequenceEqual(
                LMC_DiagnosticsFrame.ReadBulkStatus(
                    3,
                    BulkId,
                    ConfigRevision,
                    MapRevision),
                request);

            var snapshotStep = new FakeRpcStep(
                0x7E32,
                TestFrame.Response(0, BulkSnapshotPayload(4, true)));
            snapshotStep.InspectRequest = request => AssertEx.SequenceEqual(
                LMC_DiagnosticsFrame.ReadBulkSnapshot(
                    4,
                    BulkId,
                    ConfigRevision,
                    MapRevision),
                request);

            var releaseStep = new FakeRpcStep(
                0x7E33,
                TestFrame.Response(0, CommonPayload(16, 5)));
            releaseStep.InspectRequest = request => AssertEx.SequenceEqual(
                LMC_DiagnosticsFrame.ReleaseBulk(
                    5,
                    BulkId,
                    ConfigRevision,
                    MapRevision),
                request);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, MapRevision),
                configureStep,
                statusStep,
                snapshotStep,
                releaseStep,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);

                var builder = connection.Diagnostics.CreatePIBulkBuilder(
                    CreateCatalog(MapRevision, connection));
                builder.AddEntry(Signal1);
                builder.AddEntry(Signal2);
                var reader = builder.Configure();

                AssertEx.True(builder.IsFrozen);
                AssertEx.Throws<InvalidOperationException>(
                    () => builder.Clear());
                AssertEx.Equal(MapRevision, reader.Configuration.MapRevision);
                AssertEx.False(reader.HasSnapshot);
                AssertEx.Throws<InvalidOperationException>(
                    () => reader.GetEntry(Signal1));

                AssertEx.True(reader.ReadStatus().IsActive);
                var snapshot = reader.Upload();
                AssertEx.True(snapshot.IsPartial);
                AssertEx.True(reader.HasSnapshot);
                AssertEx.Equal(snapshot, reader.LatestSnapshot);
                AssertEx.Equal(unchecked((int)0xFFFE1DC0u),
                    reader.GetEntry("axis1.actual_position").RawInt32);

                LMCSignalValueEntry partialEntry;
                AssertEx.True(reader.TryGetEntry(Signal2, out partialEntry));
                AssertEx.False(partialEntry.IsValid);
                AssertEx.Equal(
                    LMCSignalEntryStatus.SlaveOffline,
                    partialEntry.EntryStatus);
                AssertEx.Equal(
                    LMCDiagnosticsDetailCode.SlaveOffline,
                    partialEntry.Detail);

                LMCSignalValueEntry missing;
                AssertEx.False(reader.TryGetEntry(0x00FFFFFFu, out missing));

                reader.Release();
                AssertEx.True(reader.IsReleased);
                AssertEx.False(reader.HasSnapshot);
                AssertEx.Throws<InvalidOperationException>(
                    () => reader.GetEntry(Signal1));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AsyncAndPreCancellation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, MapRevision),
                new FakeRpcStep(
                    0x7E30,
                    TestFrame.Response(
                        0,
                        BulkStatusPayload(2, LMCBulkState.Pending, 0))),
                new FakeRpcStep(
                    0x7E31,
                    TestFrame.Response(
                        0,
                        BulkStatusPayload(3, LMCBulkState.Active, 100))),
                new FakeRpcStep(
                    0x7E32,
                    TestFrame.Response(0, BulkSnapshotPayload(4, false))),
                new FakeRpcStep(
                    0x7E33,
                    TestFrame.Response(0, CommonPayload(16, 5))),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                Connect(connection, server.Port);

                var builder = connection.Diagnostics.CreatePIBulkBuilder(
                    CreateCatalog(MapRevision, connection));
                builder.AddEntry(Signal1);
                builder.AddEntry(Signal2);

                cancellation.Cancel();
                AssertEx.Throws<OperationCanceledException>(
                    () => builder.ConfigureAsync(cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.False(builder.IsConfiguring);
                AssertEx.False(builder.IsFrozen);

                var reader = builder.ConfigureAsync(CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(reader.ReadStatusAsync(CancellationToken.None)
                    .GetAwaiter()
                    .GetResult()
                    .IsActive);
                var uploadTask = reader.UploadAsync(CancellationToken.None);
                var releaseTask = reader.ReleaseAsync(CancellationToken.None);
                var snapshot = uploadTask.GetAwaiter().GetResult();
                AssertEx.False(snapshot.IsPartial);
                releaseTask.GetAwaiter().GetResult();
                AssertEx.True(reader.IsReleased);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void ExactRevisionRejectedBeforeConfigure()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, MapRevision),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);

                var builder = connection.Diagnostics.CreatePIBulkBuilder(
                    CreateCatalog(MapRevision + 1, connection));
                builder.AddEntry(Signal1);
                AssertEx.Throws<InvalidOperationException>(
                    () => builder.Configure());
                AssertEx.False(builder.IsFrozen);
                AssertEx.False(builder.IsConfiguring);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void ReconnectRejectsReaderBeforeWire()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, MapRevision),
                new FakeRpcStep(
                    0x7E30,
                    TestFrame.Response(
                        0,
                        BulkStatusPayload(2, LMCBulkState.Pending, 0))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, firstServer.Port);
                var builder = connection.Diagnostics.CreatePIBulkBuilder(
                    CreateCatalog(MapRevision, connection));
                builder.AddEntry(Signal1);
                builder.AddEntry(Signal2);
                var reader = builder.Configure();
                connection.CloseConnection();
                firstServer.Verify();

                using (var secondServer = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    CloseStep()))
                {
                    Connect(connection, secondServer.Port);
                    AssertEx.Throws<InvalidOperationException>(
                        () => reader.Upload());
                    connection.CloseConnection();
                    secondServer.Verify();
                }
            }
        }

        private static void CatalogProvenancePreWire()
        {
            AssertUnboundCatalogRejected();
            AssertForeignCatalogRejected();
            AssertStaleCatalogRejected();
        }

        private static void AssertUnboundCatalogRejected()
        {
            var catalog = CreateCatalog(MapRevision);
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                AssertEx.False(catalog.BelongsTo(connection));
                AssertEx.False(catalog.BelongsToCurrentSession(connection));
                AssertEx.Throws<InvalidOperationException>(() =>
                    connection.Diagnostics.CreatePIBulkBuilder(catalog));
                AssertEx.Throws<InvalidOperationException>(() =>
                    connection.Diagnostics.ReadPI(
                        catalog,
                        "axis1.actual_position"));
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertForeignCatalogRejected()
        {
            using (var ownerServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var foreignServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var ownerConnection = new LMCConnection())
            using (var foreignConnection = new LMCConnection())
            {
                Connect(ownerConnection, ownerServer.Port);
                var catalog = CreateCatalog(
                    MapRevision,
                    ownerConnection);
                Connect(foreignConnection, foreignServer.Port);

                AssertEx.True(catalog.BelongsTo(ownerConnection));
                AssertEx.True(
                    catalog.BelongsToCurrentSession(ownerConnection));
                AssertEx.False(catalog.BelongsTo(foreignConnection));
                AssertEx.Throws<InvalidOperationException>(() =>
                    foreignConnection.Diagnostics.CreatePIBulkBuilder(
                        catalog));
                AssertEx.Throws<InvalidOperationException>(() =>
                    foreignConnection.Diagnostics.ReadPIAsync(
                            catalog,
                            "axis1.actual_position",
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                foreignConnection.CloseConnection();
                ownerConnection.CloseConnection();
                foreignServer.Verify();
                ownerServer.Verify();
            }
        }

        private static void AssertStaleCatalogRejected()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, firstServer.Port);
                var catalog = CreateCatalog(MapRevision, connection);
                var builder = connection.Diagnostics.CreatePIBulkBuilder(
                    catalog);
                builder.AddEntry(Signal1);
                AssertEx.True(catalog.BelongsToCurrentSession(connection));
                connection.CloseConnection();
                AssertEx.False(
                    catalog.BelongsToCurrentSession(connection));
                firstServer.Verify();

                using (var secondServer = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    CloseStep()))
                {
                    Connect(connection, secondServer.Port);
                    AssertEx.True(catalog.BelongsTo(connection));
                    AssertEx.False(
                        catalog.BelongsToCurrentSession(connection));
                    AssertEx.Throws<InvalidOperationException>(() =>
                        builder.Configure());
                    AssertEx.False(builder.IsConfiguring);
                    AssertEx.False(builder.IsFrozen);
                    AssertEx.Throws<InvalidOperationException>(() =>
                        connection.Diagnostics.ReadPI(
                            catalog,
                            "axis1.actual_position"));
                    connection.CloseConnection();
                    secondServer.Verify();
                }
            }
        }

        private static void AliasReadUsesD1Wire()
        {
            var syncRead = new FakeRpcStep(
                0x7E20,
                TestFrame.Response(
                    0,
                    ReadPIPayload(2, Signal1, 0x00012345u)));
            syncRead.InspectRequest = request => AssertEx.SequenceEqual(
                LMC_DiagnosticsFrame.ReadPI(
                    2,
                    MapRevision,
                    Signal1,
                    LMCSignalValueType.Int32),
                request);

            var asyncRead = new FakeRpcStep(
                0x7E20,
                TestFrame.Response(
                    0,
                    ReadPIPayload(4, Signal1, 0x00023456u)));
            asyncRead.InspectRequest = request => AssertEx.SequenceEqual(
                LMC_DiagnosticsFrame.ReadPI(
                    4,
                    MapRevision,
                    Signal1,
                    LMCSignalValueType.Int32),
                request);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1, MapRevision),
                syncRead,
                CapabilitiesStep(3, MapRevision),
                asyncRead,
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var catalog = CreateCatalog(MapRevision, connection);

                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.ReadPI(
                        catalog,
                        "axis1.denied"));
                AssertEx.Throws<System.IO.InvalidDataException>(
                    () => connection.Diagnostics.ReadPI(
                        catalog,
                        "axis1.unsupported_type"));
                AssertEx.Throws<System.IO.InvalidDataException>(
                    () => connection.Diagnostics.ReadPI(
                        catalog,
                        "axis1.wrong_width"));

                var first = connection.Diagnostics.ReadPI(
                    catalog,
                    "axis1.actual_position");
                AssertEx.Equal(0x00012345u, first.RawValue32);

                var second = connection.Diagnostics.ReadPIAsync(
                        catalog,
                        "axis1.actual_position",
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(0x00023456u, second.RawValue32);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static LMCSignalCatalog CreateCatalog(
            uint mapRevision,
            LMCConnection connection = null)
        {
            var entries = new List<LMCSignalCatalogEntry>
            {
                CreateEntry(
                    Signal1,
                    0,
                    "axis1.actual_position",
                    LMCSignalValueType.Int32,
                    4,
                    LMCSignalAccessFlags.Readable
                        | LMCSignalAccessFlags.BulkReadable,
                    LMCSignalFlags.InputMappedPhase),
                CreateEntry(
                    Signal2,
                    1,
                    "axis1.digital_inputs",
                    LMCSignalValueType.BitField32,
                    4,
                    LMCSignalAccessFlags.Readable
                        | LMCSignalAccessFlags.BulkReadable,
                    LMCSignalFlags.InputMappedPhase),
                CreateEntry(
                    Signal3,
                    2,
                    "axis1.target_position_last_tx",
                    LMCSignalValueType.Int32,
                    4,
                    LMCSignalAccessFlags.Readable
                        | LMCSignalAccessFlags.BulkReadable,
                    LMCSignalFlags.PreOutputPhase),
                CreateEntry(
                    DeniedSignal,
                    3,
                    "axis1.denied",
                    LMCSignalValueType.UInt32,
                    4,
                    LMCSignalAccessFlags.None,
                    LMCSignalFlags.InputMappedPhase),
                CreateEntry(
                    UnsupportedTypeSignal,
                    4,
                    "axis1.unsupported_type",
                    LMCSignalValueType.Int8,
                    1,
                    LMCSignalAccessFlags.Readable
                        | LMCSignalAccessFlags.BulkReadable,
                    LMCSignalFlags.InputMappedPhase),
                CreateEntry(
                    WrongWidthSignal,
                    5,
                    "axis1.wrong_width",
                    LMCSignalValueType.Int16,
                    4,
                    LMCSignalAccessFlags.Readable
                        | LMCSignalAccessFlags.BulkReadable,
                    LMCSignalFlags.InputMappedPhase)
            };

            var info = new LMCSignalCatalogInfo(
                null,
                mapRevision,
                checked((ushort)entries.Count),
                80,
                40,
                4,
                0x0000000Fu,
                1);
            var catalog = new LMCSignalCatalog(info, entries);
            return connection == null
                ? catalog
                : catalog.BindProvenance(
                    connection.Diagnostics,
                    connection.SessionGeneration);
        }

        private static LMCSignalCatalogEntry CreateEntry(
            uint signalId,
            ushort catalogIndex,
            string alias,
            LMCSignalValueType dataType,
            byte byteWidth,
            LMCSignalAccessFlags accessFlags,
            LMCSignalFlags signalFlags)
        {
            return new LMCSignalCatalogEntry(
                signalId,
                catalogIndex,
                LMCSignalSourceKind.PdoInput,
                1,
                dataType,
                byteWidth,
                0,
                accessFlags,
                signalFlags,
                0x6064,
                0,
                LMCPdoDirection.DriveToMaster,
                1,
                1,
                int.MinValue,
                int.MaxValue,
                alias);
        }

        private static FakeRpcStep CapabilitiesStep(
            uint requestId,
            uint mapRevision)
        {
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(requestId, mapRevision)));
        }

        private static byte[] CapabilitiesPayload(
            uint requestId,
            uint mapRevision)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 2);
            TestFrame.WriteUInt32(
                payload,
                20,
                (uint)(LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.PIRead
                    | LMCDiagnosticCapability.BulkSnapshot));
            TestFrame.WriteUInt32(payload, 24, mapRevision);
            TestFrame.WriteUInt16(payload, 28, 6);
            TestFrame.WriteUInt16(payload, 30, 32);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt16(payload, 50, 80);
            TestFrame.WriteUInt16(payload, 52, 16);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return payload;
        }

        private static byte[] BulkStatusPayload(
            uint requestId,
            LMCBulkState state,
            uint activationCycle)
        {
            var payload = CommonPayload(36, requestId);
            TestFrame.WriteUInt32(payload, 16, BulkId);
            TestFrame.WriteUInt32(payload, 20, ConfigRevision);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt16(payload, 28, (ushort)state);
            TestFrame.WriteUInt16(payload, 30, 2);
            TestFrame.WriteUInt32(payload, 32, activationCycle);
            return payload;
        }

        private static byte[] BulkSnapshotPayload(
            uint requestId,
            bool partial)
        {
            var payload = CommonPayload(
                56 + BulkSignals.Length * 16,
                requestId,
                partial
                    ? (ushort)LMCDiagnosticsResponseFlags.Partial
                    : (ushort)0);
            TestFrame.WriteUInt32(payload, 16, BulkId);
            TestFrame.WriteUInt32(payload, 20, ConfigRevision);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 28, 100);
            TestFrame.WriteUInt32(payload, 32, 1);
            TestFrame.WriteUInt32(payload, 36, 2);
            TestFrame.WriteUInt16(payload, 40, 2);
            TestFrame.WriteUInt16(payload, 42, 16);
            payload[44] = (byte)LMCCapturePhase.InputMapped;
            TestFrame.WriteUInt32(payload, 48, 10);
            TestFrame.WriteUInt32(
                payload,
                52,
                (uint)(LMCBulkSnapshotFlags.SameCycle
                    | LMCBulkSnapshotFlags.InputMappedPhase));

            WriteValueEntry(
                payload,
                56,
                Signal1,
                0xFFFE1DC0u,
                LMCSignalValueType.Int32,
                LMCSignalEntryStatus.Valid,
                LMCDiagnosticsDetailCode.None);
            WriteValueEntry(
                payload,
                72,
                Signal2,
                0x89ABCDEFu,
                LMCSignalValueType.BitField32,
                partial
                    ? LMCSignalEntryStatus.SlaveOffline
                    : LMCSignalEntryStatus.Valid,
                partial
                    ? LMCDiagnosticsDetailCode.SlaveOffline
                    : LMCDiagnosticsDetailCode.None);
            return payload;
        }

        private static byte[] ReadPIPayload(
            uint requestId,
            uint signalId,
            uint rawValue)
        {
            var payload = CommonPayload(52, requestId);
            TestFrame.WriteUInt32(payload, 16, MapRevision);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(payload, 28, 1);
            TestFrame.WriteUInt32(payload, 32, 2);
            WriteValueEntry(
                payload,
                36,
                signalId,
                rawValue,
                LMCSignalValueType.Int32,
                LMCSignalEntryStatus.Valid,
                LMCDiagnosticsDetailCode.None);
            return payload;
        }

        private static void WriteValueEntry(
            byte[] payload,
            int offset,
            uint signalId,
            uint rawValue,
            LMCSignalValueType valueType,
            LMCSignalEntryStatus status,
            LMCDiagnosticsDetailCode detail)
        {
            TestFrame.WriteUInt32(payload, offset, signalId);
            TestFrame.WriteUInt32(payload, offset + 4, rawValue);
            payload[offset + 8] = (byte)valueType;
            payload[offset + 9] = (byte)status;
            TestFrame.WriteUInt32(payload, offset + 12, (uint)detail);
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

        private static void Connect(LMCConnection connection, int port)
        {
            connection.RpcInitConnection(
                "127.0.0.1",
                port,
                "127.0.0.1",
                0,
                LMCConnection.DefaultEventMask);
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return new FakeRpcStep(
                0x8080,
                TestFrame.Response(0, payload));
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
