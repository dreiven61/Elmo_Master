using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsD5ContractTests
    {
        private const uint GoldenRequestId = 0x11223344u;
        private const uint MapRevision = 0x957F101Eu;
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const uint TicketId = 0x10203040u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Policy.DiagnosticsD5.PIWriteValidation",
                PIWriteValidation);
            tests.Add(
                "Policy.DiagnosticsD5.SdoRequestValidation",
                SdoRequestValidation);
            tests.Add(
                "Policy.DiagnosticsD5.SdoReadValidation",
                SdoReadValidation);
            tests.Add(
                "Policy.DiagnosticsD5.SdoWriteTargetPolicy",
                SdoWriteTargetPolicy);
            tests.Add(
                "Request.DiagnosticsD5.GoldenBytes",
                D5RequestGoldenBytes);
            tests.Add(
                "Response.DiagnosticsD5.SubmitTicket",
                SubmitTicketContract);
            tests.Add(
                "Response.DiagnosticsD5.OperationStatus",
                OperationStatusContract);
            tests.Add(
                "Response.DiagnosticsD5.CancelOperation",
                CancelOperationContract);
            tests.Add(
                "Rpc.DiagnosticsD5.SyncAndAsync",
                DiagnosticsD5SyncAndAsync);
            tests.Add(
                "Rpc.DiagnosticsD5.NarrowReadSyncAndAsync",
                DiagnosticsD5NarrowReadSyncAndAsync);
            tests.Add(
                "Rpc.DiagnosticsD5.RequiredIdentitySubmitSyncAndAsync",
                RequiredIdentitySubmitSyncAndAsync);
            tests.Add(
                "Rpc.DiagnosticsD5.IdentityPinnedSdoWritePreWire",
                IdentityPinnedSdoWritePreWire);
        }

        private static void IdentityPinnedSdoWritePreWire()
        {
            RunIdentityPinnedSdoWriteMismatch(6, DiagnosticsBootId, MapRevision);
            RunIdentityPinnedSdoWriteMismatch(
                5,
                DiagnosticsBootId + 1,
                MapRevision);
            RunIdentityPinnedSdoWriteMismatch(
                5,
                DiagnosticsBootId,
                MapRevision + 1);
            RunIdentityPinnedSdoWriteSuccess();
        }

        private static void RunIdentityPinnedSdoWriteMismatch(
            uint freshBuild,
            uint freshBootId,
            uint freshMapRevision)
        {
            var requiredCapabilitiesBits =
                LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOWrite
                | LMCDiagnosticCapability.SDOReadGeneralInline;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            MapRevision,
                            DiagnosticsBootId,
                            5,
                            requiredCapabilitiesBits))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            2,
                            freshMapRevision,
                            freshBootId,
                            freshBuild,
                            requiredCapabilitiesBits))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var requiredCapabilities =
                    connection.Diagnostics.GetCapabilities();
                var target = connection.Diagnostics
                    .GetApprovedSdoWriteTargets()[0];
                var request = target.CreateRequest(17, 100);

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics
                        .SubmitSdoWriteIdentityPinnedAsync(
                            request,
                            requiredCapabilities,
                            target,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                var context = RequireSdoSubmissionFailureContext(error);
                AssertEx.Equal(
                    LMCSdoSubmissionPhase.CapabilityPreflight,
                    context.Phase);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.NotAttempted,
                    context.SubmissionOutcome);
                AssertEx.Equal(4, server.ReceivedRequests.Count);
                AssertEx.Equal(
                    (ushort)0x7E00,
                    TestFrame.ReadUInt16(
                        server.ReceivedRequests[3],
                        0));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunIdentityPinnedSdoWriteSuccess()
        {
            const uint writeTicketId = 0x71717171u;
            var requiredCapabilitiesBits =
                LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOWrite
                | LMCDiagnosticCapability.SDOReadGeneralInline;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            MapRevision,
                            DiagnosticsBootId,
                            5,
                            requiredCapabilitiesBits))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            2,
                            MapRevision,
                            DiagnosticsBootId,
                            5,
                            requiredCapabilitiesBits))),
                new FakeRpcStep(
                    0x7E50,
                    TestFrame.Response(
                        0,
                        SubmitPayload(
                            3,
                            writeTicketId,
                            LMCOperationKind.SDOWrite,
                            DiagnosticsBootId)))
                {
                    InspectRequest = frame =>
                    {
                        AssertEx.Equal(
                            MapRevision,
                            TestFrame.ReadUInt32(frame, 16));
                        AssertEx.Equal(
                            DiagnosticsBootId,
                            TestFrame.ReadUInt32(frame, 36));
                    }
                },
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var requiredCapabilities =
                    connection.Diagnostics.GetCapabilities();
                var target = connection.Diagnostics
                    .GetApprovedSdoWriteTargets()[0];
                var request = target.CreateRequest(17, 100);

                var ticket = connection.Diagnostics
                    .SubmitSdoWriteIdentityPinnedAsync(
                        request,
                        requiredCapabilities,
                        target,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(writeTicketId, ticket.TicketId);
                AssertEx.Equal(DiagnosticsBootId, ticket.DiagnosticsBootId);
                AssertEx.Equal(MapRevision, ticket.SubmissionMapRevision);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PIWriteValidation()
        {
            var writable = CatalogEntry(
                0x00200001u,
                LMCSignalValueType.UInt16,
                LMCSignalAccessFlags.Readable
                    | LMCSignalAccessFlags.WritableByPolicy,
                0x2000,
                0,
                100);
            var catalog = Catalog(writable);
            var request = new LMCPIWriteRequest(
                catalog,
                writable,
                LMCSignalValueType.UInt16,
                50);
            AssertEx.Equal(MapRevision, request.MapRevision);
            AssertEx.Equal(0x00200001u, request.SignalId);
            AssertEx.Equal(50u, request.RawValue32);

            var readOnly = CatalogEntry(
                0x00200002u,
                LMCSignalValueType.UInt16,
                LMCSignalAccessFlags.Readable,
                0x2001,
                0,
                100);
            AssertEx.Throws<InvalidOperationException>(
                () => new LMCPIWriteRequest(
                    Catalog(readOnly),
                    readOnly,
                    LMCSignalValueType.UInt16,
                    1));

            AssertEx.Throws<ArgumentException>(
                () => new LMCPIWriteRequest(
                    catalog,
                    writable,
                    LMCSignalValueType.Int32,
                    50));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCPIWriteRequest(
                    catalog,
                    writable,
                    LMCSignalValueType.UInt16,
                    101));

            var boolOnlyOne = CatalogEntry(
                0x00200005u,
                LMCSignalValueType.Bool,
                LMCSignalAccessFlags.WritableByPolicy,
                0x2002,
                1,
                1);
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCPIWriteRequest(
                    Catalog(boolOnlyOne),
                    boolOnlyOne,
                    LMCSignalValueType.Bool,
                    0));
            AssertEx.Equal(
                1u,
                new LMCPIWriteRequest(
                    Catalog(boolOnlyOne),
                    boolOnlyOne,
                    LMCSignalValueType.Bool,
                    1).RawValue32);

            var controlWord = CatalogEntry(
                0x00200003u,
                LMCSignalValueType.BitField16,
                LMCSignalAccessFlags.WritableByPolicy,
                0x6040,
                0,
                65535);
            AssertEx.Throws<InvalidOperationException>(
                () => new LMCPIWriteRequest(
                    Catalog(controlWord),
                    controlWord,
                    LMCSignalValueType.BitField16,
                    6));

            var targetPosition = CatalogEntry(
                0x00200004u,
                LMCSignalValueType.Int32,
                LMCSignalAccessFlags.WritableByPolicy,
                0x607A,
                int.MinValue,
                int.MaxValue);
            AssertEx.Throws<InvalidOperationException>(
                () => new LMCPIWriteRequest(
                    Catalog(targetPosition),
                    targetPosition,
                    LMCSignalValueType.Int32,
                    10));

            AssertPIWriteCatalogProvenancePreWire();
        }

        private static void AssertPIWriteCatalogProvenancePreWire()
        {
            AssertUnboundPIWriteCatalogRejected();
            AssertForeignPIWriteCatalogRejected();
            AssertStalePIWriteCatalogRejected();
        }

        private static void AssertUnboundPIWriteCatalogRejected()
        {
            var request = ProvenancePIWriteRequest();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                AssertEx.Throws<InvalidOperationException>(() =>
                    connection.Diagnostics.SubmitPIWrite(request));
                AssertEx.Throws<InvalidOperationException>(() =>
                    connection.Diagnostics.SubmitPIWriteAsync(
                            request,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertForeignPIWriteCatalogRejected()
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
                var request = ProvenancePIWriteRequest(ownerConnection);
                Connect(foreignConnection, foreignServer.Port);

                AssertEx.Throws<InvalidOperationException>(() =>
                    foreignConnection.Diagnostics.SubmitPIWrite(request));
                AssertEx.Throws<InvalidOperationException>(() =>
                    foreignConnection.Diagnostics.SubmitPIWriteAsync(
                            request,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                foreignConnection.CloseConnection();
                ownerConnection.CloseConnection();
                foreignServer.Verify();
                ownerServer.Verify();
            }
        }

        private static void AssertStalePIWriteCatalogRejected()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, firstServer.Port);
                var request = ProvenancePIWriteRequest(connection);
                connection.CloseConnection();
                firstServer.Verify();

                using (var secondServer = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    CloseStep()))
                {
                    Connect(connection, secondServer.Port);
                    AssertEx.Throws<InvalidOperationException>(() =>
                        connection.Diagnostics.SubmitPIWrite(request));
                    AssertEx.Throws<InvalidOperationException>(() =>
                        connection.Diagnostics.SubmitPIWriteAsync(
                                request,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    connection.CloseConnection();
                    secondServer.Verify();
                }
            }
        }

        private static LMCPIWriteRequest ProvenancePIWriteRequest(
            LMCConnection ownerConnection = null)
        {
            var writable = CatalogEntry(
                0x00200011u,
                LMCSignalValueType.UInt16,
                LMCSignalAccessFlags.WritableByPolicy,
                0x2002,
                0,
                100);
            var catalog = Catalog(writable);
            if (ownerConnection != null)
            {
                catalog.BindProvenance(
                    ownerConnection.Diagnostics,
                    ownerConnection.SessionGeneration);
            }

            return new LMCPIWriteRequest(
                catalog,
                writable,
                LMCSignalValueType.UInt16,
                1);
        }

        private static void SdoWriteTargetPolicy()
        {
            IReadOnlyList<LMCSdoWriteTarget> approved;
            using (var connection = new LMCConnection())
            {
                approved = connection.Diagnostics.GetApprovedSdoWriteTargets();
                AssertEx.Equal(1, approved.Count);
            }

            AssertEx.Equal("Reserved diagnostic UI[24]", approved[0].DisplayName);
            AssertEx.Equal((ushort)1, approved[0].SlaveReference);
            AssertEx.Equal((ushort)0x2F00, approved[0].ObjectIndex);
            AssertEx.Equal((byte)24, approved[0].SubIndex);
            AssertEx.Equal(LMCSignalValueType.Int32, approved[0].ValueType);
            AssertEx.Equal((ushort)4, approved[0].DataLength);
            AssertEx.Equal(-1073741823L, approved[0].MinimumIntegerValue);
            AssertEx.Equal(1073741823L, approved[0].MaximumIntegerValue);
            LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(
                approved[0].CreateRequest(0, 100));

            var genericWrites = new[]
            {
                LMCSdoRequest.CreateWrite(1, 0x2000, 1,
                    LMCSignalValueType.UInt8, TestFrame.Hex("5A"), 100),
                LMCSdoRequest.CreateWrite(2, 0x2001, 2,
                    LMCSignalValueType.UInt16, TestFrame.Hex("34 12"), 100),
                LMCSdoRequest.CreateWrite(3, 0x3000, 7,
                    LMCSignalValueType.Int32, TestFrame.Hex("FE FF FF FF"), 100),
                LMCSdoRequest.CreateWrite(4, 0x3001, 255,
                    LMCSignalValueType.Real32, TestFrame.Hex("00 00 80 3F"), 100)
            };
            foreach (var generic in genericWrites)
            {
                LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(generic);
            }

            foreach (var blockedObject in new ushort[]
            {
                0x6040, 0x6060, 0x607A, 0x60FF, 0x6071, 0x3204, 0x20FC
            })
            {
                var blocked = LMCSdoRequest.CreateWrite(
                    1, blockedObject, 0, LMCSignalValueType.UInt16,
                    TestFrame.Hex("00 00"), 100);
                AssertEx.Throws<NotSupportedException>(
                    () => LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(blocked));
            }

            var outOfRangeUi24 = LMCSdoRequest.CreateWrite(
                4, 0x2F00, 24, LMCSignalValueType.Int32,
                TestFrame.Hex("FF FF FF 7F"), 100);
            AssertEx.Throws<NotSupportedException>(
                () => LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(
                    outOfRangeUi24));

            AssertEx.Throws<ArgumentNullException>(
                () => LMCDiagnosticsWritePolicy
                    .RequireSdoWriteVerificationCapabilities(null));
            AssertEx.Throws<NotSupportedException>(
                () => LMCDiagnosticsWritePolicy
                    .RequireSdoWriteVerificationCapabilities(
                        SdoCapabilities(LMCDiagnosticCapability.SDOWrite
                            | LMCDiagnosticCapability.SDORead)));
            AssertEx.Throws<NotSupportedException>(
                () => LMCDiagnosticsWritePolicy
                    .RequireSdoWriteVerificationCapabilities(
                        SdoCapabilities(LMCDiagnosticCapability.SDOWrite
                            | LMCDiagnosticCapability.SDOReadGeneralInline)));
            LMCDiagnosticsWritePolicy.RequireSdoWriteVerificationCapabilities(
                SdoCapabilities(LMCDiagnosticCapability.SDOWrite
                    | LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline));
        }

        private static void SdoRequestValidation()
        {
            var read = LMCSdoRequest.CreateRead(
                1,
                0x6064,
                0,
                LMCSignalValueType.Int32,
                4,
                100);
            AssertEx.False(read.IsWrite);
            AssertEx.Equal((ushort)4, read.DataLength);
            AssertEx.Equal(0, read.WriteData.Length);

            AssertEx.Equal(
                (ushort)1,
                LMCSdoRequest.CreateRead(
                    1,
                    0x6061,
                    0,
                    LMCSignalValueType.Int8,
                    1,
                    100).DataLength);
            AssertEx.Equal(
                (ushort)2,
                LMCSdoRequest.CreateRead(
                    1,
                    0x6041,
                    0,
                    LMCSignalValueType.BitField16,
                    2,
                    100).DataLength);

            var source = TestFrame.Hex("34 12 00 00");
            var write = LMCSdoRequest.CreateWrite(
                1,
                0x2000,
                1,
                LMCSignalValueType.UInt16,
                source,
                200);
            source[0] = 0;
            AssertEx.True(write.IsWrite);
            AssertEx.Equal((byte)0x34, write.WriteData[0]);
            var returned = write.WriteData;
            returned[0] = 0;
            AssertEx.Equal((byte)0x34, write.WriteData[0]);

            var arbitraryWrite = LMCSdoRequest.CreateWrite(
                1,
                0x6061,
                0,
                LMCSignalValueType.Int8,
                TestFrame.Hex("08 00 00 00"),
                200);
            AssertEx.Equal((ushort)1, arbitraryWrite.SlaveReference);
            AssertEx.Equal((ushort)0x6061, arbitraryWrite.ObjectIndex);
            AssertEx.Equal((byte)0, arbitraryWrite.SubIndex);
            AssertEx.Equal(LMCSignalValueType.Int8, arbitraryWrite.ValueType);
            AssertEx.True(arbitraryWrite.IsWrite);

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMCSdoRequest.CreateRead(
                    0,
                    0x6064,
                    0,
                    LMCSignalValueType.Int32,
                    4,
                    100));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMCSdoRequest.CreateRead(
                    1,
                    0,
                    0,
                    LMCSignalValueType.Int32,
                    4,
                    100));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMCSdoRequest.CreateRead(
                    1,
                    0x6064,
                    0,
                    LMCSignalValueType.Invalid,
                    4,
                    100));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMCSdoRequest.CreateRead(
                    1,
                    0x6064,
                    0,
                    LMCSignalValueType.Int32,
                    2,
                    100));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMCSdoRequest.CreateRead(
                    1,
                    0x6064,
                    0,
                    LMCSignalValueType.Int32,
                    4,
                    0));
            AssertEx.Throws<ArgumentException>(
                () => LMCSdoRequest.CreateWrite(
                    1,
                    0x2000,
                    0,
                    LMCSignalValueType.UInt16,
                    TestFrame.Hex("34 12 FF 00"),
                    100));
            AssertEx.Throws<ArgumentException>(
                () => LMCSdoRequest.CreateWrite(
                    1,
                    0x2000,
                    0,
                    LMCSignalValueType.Int16,
                    TestFrame.Hex("00 80 00 00"),
                    100));
            AssertEx.Throws<ArgumentException>(
                () => LMCSdoRequest.CreateWrite(
                    1,
                    0x2000,
                    0,
                    LMCSignalValueType.Bool,
                    TestFrame.Hex("02 00 00 00"),
                    100));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMCSdoRequest.CreateWrite(
                    1,
                    0x2000,
                    0,
                    LMCSignalValueType.UInt32,
                    new byte[13],
                    100));

            using (var connection = new LMCConnection())
            {
                var unsafeWrite = LMCSdoRequest.CreateWrite(
                    1,
                    0x6040,
                    0,
                    LMCSignalValueType.BitField16,
                    TestFrame.Hex("06 00 00 00"),
                    100);
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.SubmitSdo(unsafeWrite));

            }
        }

        private static void SdoReadValidation()
        {
            foreach (var valueType in new[]
            {
                LMCSignalValueType.Bool,
                LMCSignalValueType.Int8,
                LMCSignalValueType.UInt8,
                LMCSignalValueType.BitField8
            })
            {
                AssertEx.Equal(
                    (ushort)1,
                    LMCDiagnosticsSdoPolicy.ExpectedReadLength(valueType));
            }

            foreach (var valueType in new[]
            {
                LMCSignalValueType.Int16,
                LMCSignalValueType.UInt16,
                LMCSignalValueType.BitField16
            })
            {
                AssertEx.Equal(
                    (ushort)2,
                    LMCDiagnosticsSdoPolicy.ExpectedReadLength(valueType));
            }

            foreach (var valueType in new[]
            {
                LMCSignalValueType.Int32,
                LMCSignalValueType.UInt32,
                LMCSignalValueType.Real32,
                LMCSignalValueType.BitField32
            })
            {
                AssertEx.Equal(
                    (ushort)4,
                    LMCDiagnosticsSdoPolicy.ExpectedReadLength(valueType));
            }

            var legacyRead = LMCSdoRequest.CreateRead(
                1,
                0x1000,
                0,
                LMCSignalValueType.UInt32,
                4,
                100);
            AssertEx.True(
                LMCDiagnosticsSdoPolicy.IsLegacyFirstSliceRead(legacyRead));
            AssertEx.False(
                LMCDiagnosticsSdoPolicy.IsLegacyFirstSliceRead(
                    LMCSdoRequest.CreateRead(
                        1,
                        0x1018,
                        1,
                        LMCSignalValueType.UInt32,
                        4,
                        100)));

            LMCDiagnosticsSdoPolicy.RequireReadAllowed(
                legacyRead);
            LMCDiagnosticsSdoPolicy.RequireReadAllowed(
                LMCSdoRequest.CreateRead(
                    4,
                    0xFFFF,
                    255,
                    LMCSignalValueType.UInt32,
                    4,
                    60000));
            LMCDiagnosticsSdoPolicy.RequireReadAllowed(
                LMCSdoRequest.CreateRead(
                    2,
                    0x6041,
                    0,
                    LMCSignalValueType.BitField16,
                    2,
                    100));
            LMCDiagnosticsSdoPolicy.RequireReadAllowed(
                LMCSdoRequest.CreateRead(
                    3,
                    0x6061,
                    0,
                    LMCSignalValueType.Int8,
                    1,
                    100));
            LMCDiagnosticsSdoPolicy.RequireReadAllowed(
                LMCSdoRequest.CreateRead(
                    3,
                    0x2000,
                    1,
                    LMCSignalValueType.UInt8,
                    1,
                    100));

            AssertSdoReadRejected(
                LMCSdoRequest.CreateWrite(
                    1,
                    0x1000,
                    0,
                    LMCSignalValueType.UInt32,
                    TestFrame.Hex("00 00 00 00"),
                    100));
            AssertSdoReadRejected(
                LMCSdoRequest.CreateRead(
                    5,
                    0x1000,
                    0,
                    LMCSignalValueType.UInt32,
                    4,
                    100));
            AssertSdoReadRejected(
                LMCSdoRequest.CreateRead(
                    1,
                    0x6041,
                    0,
                    LMCSignalValueType.UInt16,
                    4,
                    100));
            AssertSdoReadRejected(
                LMCSdoRequest.CreateRead(
                    1,
                    0x1000,
                    1,
                    LMCSignalValueType.Bool,
                    1,
                    60001));
            AssertSdoReadRejected(
                LMCSdoRequest.CreateRead(
                    1,
                    0x1000,
                    0,
                    LMCSignalValueType.Int16,
                    4,
                    100));
            AssertSdoReadRejected(
                LMCSdoRequest.CreateRead(
                    1,
                    0x1000,
                    0,
                    LMCSignalValueType.Int32,
                    8,
                    100));
            AssertSdoReadRejected(
                LMCSdoRequest.CreateRead(
                    1,
                    0x1000,
                    0,
                    LMCSignalValueType.UInt32,
                    8,
                    100));
        }

        private static void AssertSdoReadRejected(
            LMCSdoRequest request)
        {
            AssertEx.Throws<NotSupportedException>(
                () => LMCDiagnosticsSdoPolicy
                    .RequireReadAllowed(request));
        }

        private static void D5RequestGoldenBytes()
        {
            var read = LMCSdoRequest.CreateRead(
                1,
                0x6064,
                0,
                LMCSignalValueType.Int32,
                4,
                100);
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "50 7E 00 00 20 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "1E 10 7F 95 01 00 00 00 "
                    + "64 60 00 04 64 00 00 00 "
                    + "04 00 00 00 EF CD AB 89"),
                LMC_DiagnosticsFrame.SubmitSdo(
                    GoldenRequestId,
                    MapRevision,
                    read,
                    DiagnosticsBootId));

            var highIdentityRead = LMCSdoRequest.CreateRead(
                4,
                0xFFFF,
                255,
                LMCSignalValueType.Int8,
                1,
                100);
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "50 7E 00 00 20 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "1E 10 7F 95 04 00 00 00 "
                    + "FF FF FF 09 64 00 00 00 "
                    + "01 00 00 00 EF CD AB 89"),
                LMC_DiagnosticsFrame.SubmitSdo(
                    GoldenRequestId,
                    MapRevision,
                    highIdentityRead,
                    DiagnosticsBootId));

            var statusWordRead = LMCSdoRequest.CreateRead(
                2,
                0x6041,
                0,
                LMCSignalValueType.BitField16,
                2,
                100);
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "50 7E 00 00 20 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "1E 10 7F 95 02 00 00 00 "
                    + "41 60 00 07 64 00 00 00 "
                    + "02 00 00 00 EF CD AB 89"),
                LMC_DiagnosticsFrame.SubmitSdo(
                    GoldenRequestId,
                    MapRevision,
                    statusWordRead,
                    DiagnosticsBootId));

            var write = LMCSdoRequest.CreateWrite(
                2,
                0x2000,
                1,
                LMCSignalValueType.UInt32,
                TestFrame.Hex("78 56 34 12"),
                250);
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "50 7E 00 00 24 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "1E 10 7F 95 02 00 01 00 "
                    + "00 20 01 05 FA 00 00 00 "
                    + "04 00 00 00 EF CD AB 89 "
                    + "78 56 34 12"),
                LMC_DiagnosticsFrame.SubmitSdo(
                    GoldenRequestId,
                    MapRevision,
                    write,
                    DiagnosticsBootId));

            AssertOperationIdentityRequest(
                0x7E03,
                LMC_DiagnosticsFrame.GetOperationStatus(
                    GoldenRequestId,
                    TicketId,
                    DiagnosticsBootId));
            AssertOperationIdentityRequest(
                0x7E04,
                LMC_DiagnosticsFrame.CancelOperation(
                    GoldenRequestId,
                    TicketId,
                    DiagnosticsBootId));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.SubmitSdo(
                    0,
                    MapRevision,
                    read,
                    DiagnosticsBootId));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.SubmitSdo(
                    GoldenRequestId,
                    0,
                    read,
                    DiagnosticsBootId));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.GetOperationStatus(
                    GoldenRequestId,
                    0,
                    DiagnosticsBootId));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.CancelOperation(
                    GoldenRequestId,
                    TicketId,
                    0));
        }

        private static void SubmitTicketContract()
        {
            var submission = LMC_DiagnosticsParser.ParseSubmitOperation(
                TestFrame.Response(
                    0,
                    SubmitPayload(
                        GoldenRequestId,
                        TicketId,
                        LMCOperationKind.SDORead,
                        DiagnosticsBootId)),
                GoldenRequestId,
                LMCOperationKind.SDORead,
                DiagnosticsBootId,
                "SubmitSDO");
            AssertEx.Equal(TicketId, submission.TicketId);
            AssertEx.Equal(LMCOperationKind.SDORead, submission.OperationKind);
            AssertEx.Equal(100u, submission.QueuedCycle);

            var wrongKind = SubmitPayload(
                GoldenRequestId,
                TicketId,
                LMCOperationKind.SDOWrite,
                DiagnosticsBootId);
            AssertSubmitMalformed(wrongKind);

            var zeroTicket = SubmitPayload(
                GoldenRequestId,
                0,
                LMCOperationKind.SDORead,
                DiagnosticsBootId);
            AssertSubmitMalformed(zeroTicket);

            var running = SubmitPayload(
                GoldenRequestId,
                TicketId,
                LMCOperationKind.SDORead,
                DiagnosticsBootId);
            TestFrame.WriteUInt16(
                running,
                22,
                (ushort)LMCOperationState.Running);
            AssertSubmitMalformed(running);

            var wrongBoot = SubmitPayload(
                GoldenRequestId,
                TicketId,
                LMCOperationKind.SDORead,
                DiagnosticsBootId + 1);
            AssertSubmitMalformed(wrongBoot);

            var domainError = DomainErrorPayload(
                GoldenRequestId,
                LMCDiagnosticsDetailCode.SlaveOffline);
            var exception = AssertEx.Throws<LMCDiagnosticsCommandException>(
                () => LMC_DiagnosticsParser.ParseSubmitOperation(
                    TestFrame.Response(0, domainError),
                    GoldenRequestId,
                    LMCOperationKind.SDORead,
                    DiagnosticsBootId,
                    "SubmitSDO"));
            AssertEx.Equal(
                LMCDiagnosticsDetailCode.SlaveOffline,
                exception.Response.Detail);

            foreach (var detail in new[]
            {
                LMCDiagnosticsDetailCode.ResourceBusy,
                LMCDiagnosticsDetailCode.WriteDenied,
                LMCDiagnosticsDetailCode.UnsafeWriteBlocked,
                LMCDiagnosticsDetailCode.InvalidState,
                LMCDiagnosticsDetailCode.InternalError,
                LMCDiagnosticsDetailCode.BootIdMismatch
            })
            {
                var policyError = AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => LMC_DiagnosticsParser.ParseSubmitOperation(
                        TestFrame.Response(
                            0,
                            DomainErrorPayload(GoldenRequestId, detail)),
                        GoldenRequestId,
                        LMCOperationKind.SDOWrite,
                        DiagnosticsBootId,
                        "SubmitSDO"));
                AssertEx.Equal(detail, policyError.Response.Detail);
            }
        }

        private static void OperationStatusContract()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(
                    connection.Diagnostics,
                    LMCOperationKind.SDORead,
                    true,
                    4,
                    LMCSignalValueType.Int32);
                var completedPayload = StatusPayload(
                    GoldenRequestId,
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    4,
                    LMCSignalValueType.Int32,
                    TestFrame.Hex("78 56 34 12"));
                var status = LMC_DiagnosticsParser.ParseOperationStatus(
                    TestFrame.Response(0, completedPayload),
                    GoldenRequestId,
                    ticket);
                AssertEx.True(status.IsTerminal);
                AssertEx.True(status.IsSuccessful);
                AssertEx.Equal(0x12345678u, BitConverter.ToUInt32(status.ResultData, 0));
                var copy = status.ResultData;
                copy[0] = 0;
                AssertEx.Equal((byte)0x78, status.ResultData[0]);

                var int8Ticket = Ticket(
                    connection.Diagnostics,
                    LMCOperationKind.SDORead,
                    true,
                    1,
                    LMCSignalValueType.Int8);
                var int8Status = LMC_DiagnosticsParser.ParseOperationStatus(
                    TestFrame.Response(
                        0,
                        StatusPayload(
                            GoldenRequestId,
                            int8Ticket,
                            LMCOperationState.Completed,
                            LMCOperationOutcome.Success,
                            1,
                            LMCSignalValueType.Int8,
                            TestFrame.Hex("FE"))),
                    GoldenRequestId,
                    int8Ticket);
                AssertEx.SequenceEqual(TestFrame.Hex("FE"), int8Status.ResultData);

                var bitField16Ticket = Ticket(
                    connection.Diagnostics,
                    LMCOperationKind.SDORead,
                    true,
                    2,
                    LMCSignalValueType.BitField16);
                var bitField16Status = LMC_DiagnosticsParser.ParseOperationStatus(
                    TestFrame.Response(
                        0,
                        StatusPayload(
                            GoldenRequestId,
                            bitField16Ticket,
                            LMCOperationState.Completed,
                            LMCOperationOutcome.Success,
                            2,
                            LMCSignalValueType.BitField16,
                            TestFrame.Hex("37 12"))),
                    GoldenRequestId,
                    bitField16Ticket);
                AssertEx.SequenceEqual(
                    TestFrame.Hex("37 12"),
                    bitField16Status.ResultData);
                var mismatchedBitField16 = StatusPayload(
                    GoldenRequestId,
                    bitField16Ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    2,
                    LMCSignalValueType.BitField16,
                    TestFrame.Hex("37"));
                AssertStatusMalformed(
                    bitField16Ticket,
                    mismatchedBitField16);

                var boolTicketExact = Ticket(
                    connection.Diagnostics,
                    LMCOperationKind.SDORead,
                    true,
                    1,
                    LMCSignalValueType.Bool);
                var invalidBool = StatusPayload(
                    GoldenRequestId,
                    boolTicketExact,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    1,
                    LMCSignalValueType.Bool,
                    TestFrame.Hex("02"));
                AssertStatusMalformed(boolTicketExact, invalidBool);

                var pendingPayload = StatusPayload(
                    GoldenRequestId,
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending,
                    0,
                    LMCSignalValueType.Invalid,
                    new byte[0]);
                TestFrame.WriteUInt32(pendingPayload, 28, 0);
                var pending = LMC_DiagnosticsParser.ParseOperationStatus(
                    TestFrame.Response(0, pendingPayload),
                    GoldenRequestId,
                    ticket);
                AssertEx.False(pending.IsTerminal);

                var failedPayload = StatusPayload(
                    GoldenRequestId,
                    ticket,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed,
                    0,
                    LMCSignalValueType.Invalid,
                    new byte[0]);
                TestFrame.WriteInt16(failedPayload, 34, -32000);
                TestFrame.WriteUInt32(failedPayload, 36, 0x05040005u);
                var failed = LMC_DiagnosticsParser.ParseOperationStatus(
                    TestFrame.Response(0, failedPayload),
                    GoldenRequestId,
                    ticket);
                AssertEx.Equal(0x05040005u, failed.OperationDetail);

                var wrongIdentity = (byte[])completedPayload.Clone();
                TestFrame.WriteUInt32(wrongIdentity, 16, TicketId + 1);
                AssertStatusMalformed(ticket, wrongIdentity);

                var wrongBoot = (byte[])completedPayload.Clone();
                TestFrame.WriteUInt32(
                    wrongBoot,
                    60,
                    DiagnosticsBootId + 1);
                AssertStatusMalformed(ticket, wrongBoot);

                var wrongPair = (byte[])completedPayload.Clone();
                TestFrame.WriteUInt16(
                    wrongPair,
                    32,
                    (ushort)LMCOperationOutcome.Failed);
                AssertStatusMalformed(ticket, wrongPair);

                var reserved = (byte[])completedPayload.Clone();
                TestFrame.WriteUInt16(reserved, 46, 1);
                AssertStatusMalformed(ticket, reserved);

                var wrongLength = (byte[])completedPayload.Clone();
                TestFrame.WriteUInt32(wrongLength, 40, 8);
                AssertStatusMalformed(ticket, wrongLength);

                var dirtyTail = (byte[])completedPayload.Clone();
                dirtyTail[59] = 1;
                AssertStatusMalformed(ticket, dirtyTail);

                var uint16Ticket = Ticket(
                    connection.Diagnostics,
                    LMCOperationKind.SDORead,
                    true,
                    4,
                    LMCSignalValueType.UInt16);
                var canonicalUInt16 = StatusPayload(
                    GoldenRequestId,
                    uint16Ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    4,
                    LMCSignalValueType.UInt16,
                    TestFrame.Hex("34 12 00 00"));
                AssertEx.True(
                    LMC_DiagnosticsParser.ParseOperationStatus(
                        TestFrame.Response(0, canonicalUInt16),
                        GoldenRequestId,
                        uint16Ticket).IsSuccessful);
                var noncanonicalUInt16 = (byte[])canonicalUInt16.Clone();
                noncanonicalUInt16[50] = 1;
                AssertStatusMalformed(uint16Ticket, noncanonicalUInt16);

                var int16Ticket = Ticket(
                    connection.Diagnostics,
                    LMCOperationKind.SDORead,
                    true,
                    4,
                    LMCSignalValueType.Int16);
                var noncanonicalInt16 = StatusPayload(
                    GoldenRequestId,
                    int16Ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    4,
                    LMCSignalValueType.Int16,
                    TestFrame.Hex("00 80 00 00"));
                AssertStatusMalformed(int16Ticket, noncanonicalInt16);

                var boolTicket = Ticket(
                    connection.Diagnostics,
                    LMCOperationKind.SDORead,
                    true,
                    4,
                    LMCSignalValueType.Bool);
                var noncanonicalBool = StatusPayload(
                    GoldenRequestId,
                    boolTicket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    4,
                    LMCSignalValueType.Bool,
                    TestFrame.Hex("02 00 00 00"));
                AssertStatusMalformed(boolTicket, noncanonicalBool);

                var pendingWithData = (byte[])pendingPayload.Clone();
                TestFrame.WriteUInt32(pendingWithData, 40, 4);
                pendingWithData[44] = (byte)LMCSignalValueType.Int32;
                pendingWithData[45] = 4;
                AssertStatusMalformed(ticket, pendingWithData);

                var writeTicket = Ticket(
                    connection.Diagnostics,
                    LMCOperationKind.SDOWrite,
                    false,
                    0,
                    LMCSignalValueType.Invalid);
                var writeCompleted = StatusPayload(
                    GoldenRequestId,
                    writeTicket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    0,
                    LMCSignalValueType.Invalid,
                    new byte[0]);
                AssertEx.True(
                    LMC_DiagnosticsParser.ParseOperationStatus(
                        TestFrame.Response(0, writeCompleted),
                        GoldenRequestId,
                        writeTicket).IsSuccessful);

                var writeWithResult = (byte[])writeCompleted.Clone();
                TestFrame.WriteUInt32(writeWithResult, 40, 4);
                writeWithResult[44] = (byte)LMCSignalValueType.UInt32;
                writeWithResult[45] = 4;
                AssertStatusMalformed(writeTicket, writeWithResult);
            }
        }

        private static void CancelOperationContract()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(
                    connection.Diagnostics,
                    LMCOperationKind.SDORead,
                    true,
                    4,
                    LMCSignalValueType.Int32);
                var payload = CancelPayload(GoldenRequestId, ticket);
                var result = LMC_DiagnosticsParser.ParseCancelOperation(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    ticket);
                AssertEx.Equal(TicketId, result.TicketId);
                AssertEx.Equal(LMCOperationState.Cancelled, result.State);

                var running = (byte[])payload.Clone();
                TestFrame.WriteUInt16(
                    running,
                    20,
                    (ushort)LMCOperationState.Running);
                AssertCancelMalformed(ticket, running);

                var wrongBoot = (byte[])payload.Clone();
                TestFrame.WriteUInt32(wrongBoot, 24, DiagnosticsBootId + 1);
                AssertCancelMalformed(ticket, wrongBoot);

                var oversized = (byte[])payload.Clone();
                Array.Resize(ref oversized, oversized.Length + 1);
                AssertCancelMalformed(ticket, oversized);

                var domainError = DomainErrorPayload(
                    GoldenRequestId,
                    LMCDiagnosticsDetailCode.InvalidState);
                var exception = AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => LMC_DiagnosticsParser.ParseCancelOperation(
                        TestFrame.Response(0, domainError),
                        GoldenRequestId,
                        ticket));
                AssertEx.Equal(
                    LMCDiagnosticsDetailCode.InvalidState,
                    exception.Response.Detail);
            }
        }

        private static void DiagnosticsD5SyncAndAsync()
        {
            RunDiagnosticsD5Integration(false);
            RunDiagnosticsD5Integration(true);
        }

        private static void DiagnosticsD5NarrowReadSyncAndAsync()
        {
            RunDiagnosticsD5NarrowRead(false);
            RunDiagnosticsD5NarrowRead(true);
        }

        private static void RequiredIdentitySubmitSyncAndAsync()
        {
            RunGuardedSdoIdentitySuccess(false);
            RunGuardedSdoIdentitySuccess(true);
            RunGuardedSdoCapabilityMismatch(false, true);
            RunGuardedSdoCapabilityMismatch(true, true);
            RunGuardedSdoCapabilityMismatch(false, false);
            RunGuardedSdoCapabilityMismatch(true, false);
            RunGuardedSdoForeignOwner(false);
            RunGuardedSdoForeignOwner(true);
            RunGuardedSdoStaleSession(false);
            RunGuardedSdoStaleSession(true);
            RunGuardedSdoInvalidProvenance(false, 0);
            RunGuardedSdoInvalidProvenance(true, 0);
            RunGuardedSdoInvalidProvenance(false, 1);
            RunGuardedSdoInvalidProvenance(true, 1);
            RunGuardedSdoInvalidProvenance(false, 2);
            RunGuardedSdoInvalidProvenance(true, 2);
            RunGuardedSdoInvalidProvenance(false, 3);
            RunGuardedSdoInvalidProvenance(true, 3);
        }

        private static void RunGuardedSdoIdentitySuccess(bool useAsync)
        {
            const uint guardedTicketId = 0x51515151u;
            var request = GuardedReadRequest();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                SdoSubmitStep(
                    2,
                    guardedTicketId,
                    request.SlaveReference,
                    request.ObjectIndex,
                    request.SubIndex,
                    request.ValueType,
                    request.DataLength),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var requiredIdentityTicket = RequiredWriteIdentityTicket(
                    connection);
                AssertEx.True(requiredIdentityTicket
                    .BelongsToCurrentSession(connection));

                var submitted = SubmitGuardedSdo(
                    connection,
                    request,
                    requiredIdentityTicket,
                    useAsync);
                AssertEx.Equal(guardedTicketId, submitted.TicketId);
                AssertEx.Equal(
                    DiagnosticsBootId,
                    submitted.DiagnosticsBootId);
                AssertEx.Equal(
                    MapRevision,
                    submitted.SubmissionMapRevision);
                AssertEx.True(submitted.BelongsToCurrentSession(connection));
                AssertEx.Equal(4, server.ReceivedRequests.Count);
                AssertEx.Equal(
                    (ushort)0x7E50,
                    TestFrame.ReadUInt16(
                        server.ReceivedRequests[3],
                        0));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunGuardedSdoCapabilityMismatch(
            bool useAsync,
            bool mismatchBootId)
        {
            var request = GuardedReadRequest();
            var currentBootId = mismatchBootId
                ? DiagnosticsBootId + 1
                : DiagnosticsBootId;
            var currentMapRevision = mismatchBootId
                ? MapRevision
                : MapRevision + 1;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            currentMapRevision,
                            currentBootId))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var requiredIdentityTicket = RequiredWriteIdentityTicket(
                    connection);

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => SubmitGuardedSdo(
                        connection,
                        request,
                        requiredIdentityTicket,
                        useAsync));
                var context = RequireSdoSubmissionFailureContext(error);
                AssertEx.Equal(
                    LMCSdoSubmissionPhase.CapabilityPreflight,
                    context.Phase);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.NotAttempted,
                    context.SubmissionOutcome);
                AssertEx.Equal(currentBootId, context.DiagnosticsBootId);
                AssertEx.Equal(
                    currentMapRevision,
                    context.MapRevision);
                AssertEx.True(context.Ticket == null);
                AssertEx.Equal(3, server.ReceivedRequests.Count);
                AssertEx.Equal(
                    (ushort)0x7E00,
                    TestFrame.ReadUInt16(
                        server.ReceivedRequests[2],
                        0));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunGuardedSdoForeignOwner(bool useAsync)
        {
            var request = GuardedReadRequest();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var foreignConnection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var foreignTicket = RequiredWriteIdentityTicket(
                    foreignConnection);
                AssertEx.False(foreignTicket
                    .BelongsToCurrentSession(connection));

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => SubmitGuardedSdo(
                        connection,
                        request,
                        foreignTicket,
                        useAsync));
                AssertSessionPreflightFailure(error);
                AssertEx.Equal(2, server.ReceivedRequests.Count);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunGuardedSdoStaleSession(bool useAsync)
        {
            var request = GuardedReadRequest();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var staleTicket = RequiredWriteIdentityTicket(connection);
                AssertEx.True(staleTicket
                    .BelongsToCurrentSession(connection));
                Connect(connection, server.Port);
                AssertEx.False(staleTicket
                    .BelongsToCurrentSession(connection));

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => SubmitGuardedSdo(
                        connection,
                        request,
                        staleTicket,
                        useAsync));
                AssertSessionPreflightFailure(error);
                AssertEx.Equal(2, server.ReceivedRequests.Count);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunGuardedSdoInvalidProvenance(
            bool useAsync,
            int invalidKind)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                LMCOperationTicket invalidTicket;
                if (invalidKind == 0)
                {
                    var submittedRead = GuardedReadRequest();
                    invalidTicket = new LMCOperationTicket(
                        0x50505050u,
                        LMCOperationKind.SDORead,
                        100,
                        DiagnosticsBootId,
                        MapRevision,
                        connection.SessionGeneration,
                        connection.Diagnostics,
                        true,
                        submittedRead.DataLength,
                        submittedRead.ValueType,
                        submittedSdoRequest: submittedRead);
                }
                else if (invalidKind == 1)
                {
                    invalidTicket = new LMCOperationTicket(
                        0x50505050u,
                        LMCOperationKind.PIWrite,
                        100,
                        DiagnosticsBootId,
                        MapRevision,
                        connection.SessionGeneration,
                        connection.Diagnostics,
                        false,
                        0,
                        LMCSignalValueType.Invalid);
                }
                else if (invalidKind == 2)
                {
                    invalidTicket = new LMCOperationTicket(
                        0x50505050u,
                        LMCOperationKind.SDOWrite,
                        100,
                        DiagnosticsBootId,
                        MapRevision,
                        connection.SessionGeneration,
                        connection.Diagnostics,
                        false,
                        0,
                        LMCSignalValueType.Invalid);
                }
                else
                {
                    invalidTicket = RequiredWriteIdentityTicket(
                        connection,
                        GuardedWriteRequest(1));
                }

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => SubmitGuardedSdo(
                        connection,
                        GuardedReadRequest(),
                        invalidTicket,
                        useAsync));
                AssertSessionPreflightFailure(error);
                AssertEx.Equal(2, server.ReceivedRequests.Count);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static LMCSdoRequest GuardedReadRequest()
        {
            return LMCSdoRequest.CreateRead(
                2,
                0x2000,
                3,
                LMCSignalValueType.UInt32,
                4,
                100);
        }

        private static LMCSdoRequest GuardedWriteRequest(
            ushort slaveReference = 2)
        {
            return LMCSdoRequest.CreateWrite(
                slaveReference,
                0x2000,
                3,
                LMCSignalValueType.UInt32,
                new byte[] { 0x78, 0x56, 0x34, 0x12 },
                100);
        }

        private static LMCOperationTicket RequiredWriteIdentityTicket(
            LMCConnection connection,
            LMCSdoRequest submittedWriteRequest = null)
        {
            return new LMCOperationTicket(
                0x50505050u,
                LMCOperationKind.SDOWrite,
                100,
                DiagnosticsBootId,
                MapRevision,
                connection.SessionGeneration,
                connection.Diagnostics,
                false,
                0,
                LMCSignalValueType.Invalid,
                submittedSdoRequest: submittedWriteRequest
                    ?? GuardedWriteRequest());
        }

        private static LMCOperationTicket SubmitGuardedSdo(
            LMCConnection connection,
            LMCSdoRequest request,
            LMCOperationTicket requiredIdentityTicket,
            bool useAsync)
        {
            return useAsync
                ? connection.Diagnostics.SubmitSdoAsync(
                        request,
                        requiredIdentityTicket,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult()
                : connection.Diagnostics.SubmitSdo(
                    request,
                    requiredIdentityTicket);
        }

        private static void AssertSessionPreflightFailure(
            Exception error)
        {
            var context = RequireSdoSubmissionFailureContext(error);
            AssertEx.Equal(
                LMCSdoSubmissionPhase.SessionPreflight,
                context.Phase);
            AssertEx.Equal(
                LMCSdoSubmissionOutcome.NotAttempted,
                context.SubmissionOutcome);
            AssertEx.Equal(0u, context.DiagnosticsBootId);
            AssertEx.Equal(0u, context.MapRevision);
            AssertEx.True(context.Ticket == null);
        }

        private static LMCSdoSubmissionFailureContext
            RequireSdoSubmissionFailureContext(Exception error)
        {
            LMCSdoSubmissionFailureContext context;
            AssertEx.True(
                LMCSdoSubmissionFailureContext.TryGet(
                    error,
                    out context),
                "Expected an SDO submission failure context.");
            return context;
        }

        private static void Connect(
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

        private static void RunDiagnosticsD5NarrowRead(bool useAsync)
        {
            const uint int8TicketId = 0x33333333u;
            const uint bitField16TicketId = 0x44444444u;
            var int8Request = LMCSdoRequest.CreateRead(
                1,
                0x6061,
                0,
                LMCSignalValueType.Int8,
                1,
                100);
            var bitField16Request = LMCSdoRequest.CreateRead(
                2,
                0x6041,
                0,
                LMCSignalValueType.BitField16,
                2,
                100);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                SdoSubmitStep(
                    2,
                    int8TicketId,
                    1,
                    0x6061,
                    0,
                    LMCSignalValueType.Int8,
                    1),
                new FakeRpcStep(
                    0x7E03,
                    TestFrame.Response(
                        0,
                        IntegrationStatusPayload(
                            3,
                            int8TicketId,
                            LMCSignalValueType.Int8,
                            TestFrame.Hex("FE")))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(4))),
                SdoSubmitStep(
                    5,
                    bitField16TicketId,
                    2,
                    0x6041,
                    0,
                    LMCSignalValueType.BitField16,
                    2),
                new FakeRpcStep(
                    0x7E03,
                    TestFrame.Response(
                        0,
                        IntegrationStatusPayload(
                            6,
                            bitField16TicketId,
                            LMCSignalValueType.BitField16,
                            TestFrame.Hex("37 12")))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                LMCOperationTicket int8Ticket;
                LMCOperationStatus int8Status;
                LMCOperationTicket bitField16Ticket;
                LMCOperationStatus bitField16Status;
                if (useAsync)
                {
                    int8Ticket = connection.Diagnostics.SubmitSdoAsync(
                            int8Request,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    int8Status = connection.Diagnostics.GetOperationStatusAsync(
                            int8Ticket,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    bitField16Ticket = connection.Diagnostics.SubmitSdoAsync(
                            bitField16Request,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    bitField16Status = connection.Diagnostics
                        .GetOperationStatusAsync(
                            bitField16Ticket,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    int8Ticket = connection.Diagnostics.SubmitSdo(int8Request);
                    int8Status = connection.Diagnostics.GetOperationStatus(
                        int8Ticket);
                    bitField16Ticket = connection.Diagnostics.SubmitSdo(
                        bitField16Request);
                    bitField16Status = connection.Diagnostics.GetOperationStatus(
                        bitField16Ticket);
                }

                AssertEx.Equal((ushort)1, int8Ticket.RequestedResultLength);
                AssertEx.SequenceEqual(
                    TestFrame.Hex("FE"),
                    int8Status.ResultData);
                AssertEx.Equal(
                    (ushort)2,
                    bitField16Ticket.RequestedResultLength);
                AssertEx.SequenceEqual(
                    TestFrame.Hex("37 12"),
                    bitField16Status.ResultData);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static FakeRpcStep SdoSubmitStep(
            uint requestId,
            uint ticketId,
            ushort slaveReference,
            ushort objectIndex,
            byte subIndex,
            LMCSignalValueType valueType,
            ushort dataLength)
        {
            return new FakeRpcStep(
                0x7E50,
                TestFrame.Response(
                    0,
                    SubmitPayload(
                        requestId,
                        ticketId,
                        LMCOperationKind.SDORead,
                        DiagnosticsBootId)))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal((ushort)32, TestFrame.ReadUInt16(request, 4));
                    AssertEx.Equal(
                        slaveReference,
                        TestFrame.ReadUInt16(request, 20));
                    AssertEx.Equal(objectIndex, TestFrame.ReadUInt16(request, 24));
                    AssertEx.Equal(subIndex, request[26]);
                    AssertEx.Equal((byte)valueType, request[27]);
                    AssertEx.Equal(dataLength, TestFrame.ReadUInt16(request, 32));
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        TestFrame.ReadUInt32(request, 36));
                }
            };
        }

        private static void RunDiagnosticsD5Integration(bool useAsync)
        {
            var firstTicketId = 0x11111111u;
            var secondTicketId = 0x22222222u;
            var request = LMCSdoRequest.CreateRead(
                1,
                0x1018,
                1,
                LMCSignalValueType.UInt32,
                4,
                100);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7E50,
                    TestFrame.Response(
                        0,
                        SubmitPayload(
                            2,
                            firstTicketId,
                            LMCOperationKind.SDORead,
                            DiagnosticsBootId))),
                new FakeRpcStep(
                    0x7E03,
                    TestFrame.Response(
                        0,
                        IntegrationStatusPayload(3, firstTicketId))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(4))),
                new FakeRpcStep(
                    0x7E50,
                    TestFrame.Response(
                        0,
                        SubmitPayload(
                            5,
                            secondTicketId,
                            LMCOperationKind.SDORead,
                            DiagnosticsBootId))),
                new FakeRpcStep(
                    0x7E04,
                    TestFrame.Response(
                        0,
                        IntegrationCancelPayload(6, secondTicketId))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                LMCOperationTicket first;
                LMCOperationStatus status;
                LMCOperationTicket second;

                if (useAsync)
                {
                    first = connection.Diagnostics.SubmitSdoAsync(
                            request,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    status = connection.Diagnostics.GetOperationStatusAsync(
                            first,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    second = connection.Diagnostics.SubmitSdoAsync(
                            request,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    connection.Diagnostics.CancelOperationAsync(
                            second,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    first = connection.Diagnostics.SubmitSdo(request);
                    status = connection.Diagnostics.GetOperationStatus(first);
                    second = connection.Diagnostics.SubmitSdo(request);
                    connection.Diagnostics.CancelOperation(second);
                }

                AssertEx.Equal(firstTicketId, first.TicketId);
                AssertEx.Equal(DiagnosticsBootId, first.DiagnosticsBootId);
                AssertEx.Equal(MapRevision, first.SubmissionMapRevision);
                AssertEx.True(status.IsSuccessful);
                AssertEx.Equal(0x12345678u, BitConverter.ToUInt32(status.ResultData, 0));

                using (var otherConnection = new LMCConnection())
                {
                    AssertEx.Throws<InvalidOperationException>(
                        () => otherConnection.Diagnostics.GetOperationStatus(first));
                }

                connection.CloseConnection();
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.GetOperationStatus(first));
                server.Verify();
            }
        }

        private static void AssertOperationIdentityRequest(
            ushort commandId,
            byte[] actual)
        {
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    commandId.ToString("X4").Substring(2, 2)
                    + " "
                    + commandId.ToString("X4").Substring(0, 2)
                    + " 00 00 10 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "40 30 20 10 EF CD AB 89"),
                actual);
        }

        private static void AssertSubmitMalformed(byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseSubmitOperation(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    LMCOperationKind.SDORead,
                    DiagnosticsBootId,
                    "SubmitSDO"));
        }

        private static void AssertStatusMalformed(
            LMCOperationTicket ticket,
            byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseOperationStatus(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    ticket));
        }

        private static void AssertCancelMalformed(
            LMCOperationTicket ticket,
            byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseCancelOperation(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    ticket));
        }

        private static LMCSignalCatalog Catalog(
            params LMCSignalCatalogEntry[] entries)
        {
            var info = new LMCSignalCatalogInfo(
                null,
                MapRevision,
                checked((ushort)entries.Length),
                80,
                40,
                4,
                0x0F,
                1);
            return new LMCSignalCatalog(info, entries);
        }

        private static LMCSignalCatalogEntry CatalogEntry(
            uint signalId,
            LMCSignalValueType valueType,
            LMCSignalAccessFlags accessFlags,
            ushort pdoIndex,
            int minimum,
            int maximum)
        {
            return new LMCSignalCatalogEntry(
                signalId,
                0,
                LMCSignalSourceKind.PlcApplication,
                1,
                valueType,
                4,
                0,
                accessFlags,
                LMCSignalFlags.PreOutputPhase,
                pdoIndex,
                0,
                LMCPdoDirection.None,
                1,
                1,
                minimum,
                maximum,
                "test.write");
        }

        private static LMCOperationTicket Ticket(
            LMCDiagnostics owner,
            LMCOperationKind kind,
            bool expectsData,
            ushort expectedLength,
            LMCSignalValueType expectedType)
        {
            return new LMCOperationTicket(
                TicketId,
                kind,
                100,
                DiagnosticsBootId,
                MapRevision,
                0,
                owner,
                expectsData,
                expectedLength,
                expectedType);
        }

        private static byte[] SubmitPayload(
            uint requestId,
            uint ticketId,
            LMCOperationKind kind,
            uint diagnosticsBootId)
        {
            var payload = CommonPayload(32, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
            TestFrame.WriteUInt16(payload, 20, (ushort)kind);
            TestFrame.WriteUInt16(
                payload,
                22,
                (ushort)LMCOperationState.Queued);
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(payload, 28, diagnosticsBootId);
            return payload;
        }

        private static byte[] StatusPayload(
            uint requestId,
            LMCOperationTicket ticket,
            LMCOperationState state,
            LMCOperationOutcome outcome,
            uint resultLength,
            LMCSignalValueType resultType,
            byte[] resultData)
        {
            var payload = CommonPayload(64, requestId);
            TestFrame.WriteUInt32(payload, 16, ticket.TicketId);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)ticket.OperationKind);
            TestFrame.WriteUInt16(payload, 22, (ushort)state);
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(payload, 28, 200);
            TestFrame.WriteUInt16(payload, 32, (ushort)outcome);
            TestFrame.WriteUInt32(payload, 40, resultLength);
            payload[44] = (byte)resultType;
            payload[45] = checked((byte)resultData.Length);
            Buffer.BlockCopy(resultData, 0, payload, 48, resultData.Length);
            TestFrame.WriteUInt32(
                payload,
                60,
                ticket.DiagnosticsBootId);
            return payload;
        }

        private static byte[] CancelPayload(
            uint requestId,
            LMCOperationTicket ticket)
        {
            var payload = CommonPayload(28, requestId);
            TestFrame.WriteUInt32(payload, 16, ticket.TicketId);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCOperationState.Cancelled);
            TestFrame.WriteUInt16(
                payload,
                22,
                (ushort)LMCOperationOutcome.Cancelled);
            TestFrame.WriteUInt32(
                payload,
                24,
                ticket.DiagnosticsBootId);
            return payload;
        }

        private static byte[] CapabilitiesPayload(
            uint requestId,
            uint mapRevision = MapRevision,
            uint diagnosticsBootId = DiagnosticsBootId,
            uint diagnosticsBuild = 5,
            LMCDiagnosticCapability capabilityBits =
                LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOReadGeneralInline)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, diagnosticsBuild);
            TestFrame.WriteUInt32(
                payload,
                20,
                (uint)capabilityBits);
            TestFrame.WriteUInt32(payload, 24, mapRevision);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 60, 4);
            TestFrame.WriteUInt32(payload, 64, diagnosticsBootId);
            return payload;
        }

        private static LMCDiagnosticCapabilities SdoCapabilities(
            LMCDiagnosticCapability capabilities)
        {
            return new LMCDiagnosticCapabilities(
                null,
                1,
                1,
                (uint)capabilities,
                MapRevision,
                0,
                0,
                0,
                0,
                0,
                1000,
                1320,
                2040,
                1280,
                80,
                16,
                0,
                4,
                DiagnosticsBootId);
        }

        private static byte[] IntegrationStatusPayload(
            uint requestId,
            uint ticketId)
        {
            return IntegrationStatusPayload(
                requestId,
                ticketId,
                LMCSignalValueType.UInt32,
                TestFrame.Hex("78 56 34 12"));
        }

        private static byte[] IntegrationStatusPayload(
            uint requestId,
            uint ticketId,
            LMCSignalValueType valueType,
            byte[] resultData)
        {
            using (var connection = new LMCConnection())
            {
                var ticket = new LMCOperationTicket(
                    ticketId,
                    LMCOperationKind.SDORead,
                    100,
                    DiagnosticsBootId,
                    MapRevision,
                    0,
                    connection.Diagnostics,
                    true,
                    checked((ushort)resultData.Length),
                    valueType);
                return StatusPayload(
                    requestId,
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    checked((uint)resultData.Length),
                    valueType,
                    resultData);
            }
        }

        private static byte[] IntegrationCancelPayload(
            uint requestId,
            uint ticketId)
        {
            using (var connection = new LMCConnection())
            {
                var ticket = new LMCOperationTicket(
                    ticketId,
                    LMCOperationKind.SDORead,
                    100,
                    DiagnosticsBootId,
                    MapRevision,
                    0,
                    connection.Diagnostics,
                    true,
                    4,
                    LMCSignalValueType.UInt32);
                return CancelPayload(requestId, ticket);
            }
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
