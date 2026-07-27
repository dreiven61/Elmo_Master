using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class BulkQualificationCleanupOrchestratorTests
    {
        private const uint MapRevision = 0x957F101Eu;
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const uint BulkId = 0xA1B2C3D4u;
        private const uint ConfigRevision = 0x01020304u;
        private const uint Signal1 = 0x00100104u;
        private const uint Signal2 = 0x00100105u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.Bulk.CancelReleasesAndPreservesPrimary",
                CancelReleasesAndPreservesPrimary);
            tests.Add(
                "Qualification.Bulk.ReleaseFailureIsNotPass",
                ReleaseFailureIsNotPass);
            tests.Add(
                "Qualification.Bulk.PrimaryAndReleaseFailureAggregate",
                PrimaryAndReleaseFailureAggregate);
            tests.Add(
                "Qualification.Bulk.PreReaderFailurePreservesPrimary",
                PreReaderFailurePreservesPrimary);
        }

        private static void CancelReleasesAndPreservesPrimary()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1),
                ConfigureStep(2),
                new FakeRpcStep(
                    0x7E33,
                    TestFrame.Response(0, CommonPayload(16, 3))),
                CloseStep()))
            using (var connection = Connect(server))
            {
                var reader = CreateReader(connection);
                var primary = new OperationCanceledException(
                    "qualification canceled after Configure ACK");

                var observed = AssertEx.Throws<OperationCanceledException>(
                    () => BulkQualificationCleanupOrchestrator
                        .ReleaseAndRethrowPrimaryAsync(
                            reader,
                            primary,
                            ReleaseAsync,
                            CreateAggregate)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(ReferenceEquals(primary, observed));
                AssertEx.True(reader.IsReleased);
                AssertCommandCounts(server, 1);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void ReleaseFailureIsNotPass()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1),
                ConfigureStep(2),
                new FakeRpcStep(
                    0x7E33,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            3,
                            LMCDiagnosticsDetailCode.ResourceBusy))),
                new FakeRpcStep(
                    0x7E33,
                    TestFrame.Response(0, CommonPayload(16, 4))),
                CloseStep()))
            using (var connection = Connect(server))
            {
                var reader = CreateReader(connection);
                var observed = AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => BulkQualificationCleanupOrchestrator
                        .ReleaseAndRethrowPrimaryAsync(
                            reader,
                            null,
                            ReleaseAsync,
                            CreateAggregate)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCDiagnosticsDetailCode.ResourceBusy,
                    observed.Response.Detail);
                AssertEx.False(reader.IsReleased);

                var released = BulkQualificationCleanupOrchestrator
                    .ReleaseAndRethrowPrimaryAsync(
                        reader,
                        null,
                        ReleaseAsync,
                        CreateAggregate)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(released);
                AssertEx.True(reader.IsReleased);
                AssertCommandCounts(server, 2);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PrimaryAndReleaseFailureAggregate()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1),
                ConfigureStep(2),
                new FakeRpcStep(
                    0x7E33,
                    TestFrame.Response(
                        0,
                        DomainErrorPayload(
                            3,
                            LMCDiagnosticsDetailCode.ResourceBusy))),
                CloseStep()))
            using (var connection = Connect(server))
            {
                var reader = CreateReader(connection);
                var primary = new InvalidOperationException(
                    "partial snapshot validation failed");
                var observed = AssertEx.Throws<InvalidOperationException>(
                    () => BulkQualificationCleanupOrchestrator
                        .ReleaseAndRethrowPrimaryAsync(
                            reader,
                            primary,
                            ReleaseAsync,
                            CreateAggregate)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(observed.InnerException is AggregateException);
                var aggregate = (AggregateException)observed.InnerException;
                AssertEx.Equal(2, aggregate.InnerExceptions.Count);
                AssertEx.True(ReferenceEquals(
                    primary,
                    aggregate.InnerExceptions[0]));
                AssertEx.True(
                    aggregate.InnerExceptions[1]
                        is LMCDiagnosticsCommandException);
                AssertEx.False(reader.IsReleased);
                AssertCommandCounts(server, 1);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PreReaderFailurePreservesPrimary()
        {
            var releaseCount = 0;
            var primary = new InvalidOperationException(
                "Configure failed before returning a Bulk reader");
            var observed = AssertEx.Throws<InvalidOperationException>(
                () => BulkQualificationCleanupOrchestrator
                    .ReleaseAndRethrowPrimaryAsync(
                        null,
                        primary,
                        reader =>
                        {
                            releaseCount++;
                            return Task.FromResult(false);
                        },
                        CreateAggregate)
                    .GetAwaiter()
                    .GetResult());

            AssertEx.True(ReferenceEquals(primary, observed));
            AssertEx.Equal(0, releaseCount);
        }

        private static async Task<bool> ReleaseAsync(
            LMCPIBulkReader reader)
        {
            await reader.ReleaseAsync(CancellationToken.None);
            return true;
        }

        private static Exception CreateAggregate(
            Exception primary,
            Exception cleanup)
        {
            return new InvalidOperationException(
                "Bulk primary and cleanup both failed.",
                new AggregateException(primary, cleanup));
        }

        private static LMCPIBulkReader CreateReader(
            LMCConnection connection)
        {
            var builder = connection.Diagnostics.CreatePIBulkBuilder(
                CreateCatalog());
            builder.AddEntry(Signal1);
            builder.AddEntry(Signal2);
            return builder.Configure();
        }

        private static LMCSignalCatalog CreateCatalog()
        {
            var entries = new List<LMCSignalCatalogEntry>
            {
                CreateEntry(
                    Signal1,
                    0,
                    "axis1.actual_position",
                    LMCSignalValueType.Int32),
                CreateEntry(
                    Signal2,
                    1,
                    "axis1.digital_inputs",
                    LMCSignalValueType.BitField32)
            };
            var info = new LMCSignalCatalogInfo(
                null,
                MapRevision,
                2,
                80,
                40,
                4,
                0x0000000Fu,
                1);
            return new LMCSignalCatalog(info, entries);
        }

        private static LMCSignalCatalogEntry CreateEntry(
            uint signalId,
            ushort catalogIndex,
            string alias,
            LMCSignalValueType dataType)
        {
            return new LMCSignalCatalogEntry(
                signalId,
                catalogIndex,
                LMCSignalSourceKind.PdoInput,
                1,
                dataType,
                4,
                0,
                LMCSignalAccessFlags.Readable
                    | LMCSignalAccessFlags.BulkReadable,
                LMCSignalFlags.InputMappedPhase,
                0x6064,
                0,
                LMCPdoDirection.DriveToMaster,
                1,
                1,
                int.MinValue,
                int.MaxValue,
                alias);
        }

        private static LMCConnection Connect(FakeRpcServer server)
        {
            var connection = new LMCConnection();
            connection.RpcInitConnection(
                "127.0.0.1",
                server.Port,
                "127.0.0.1",
                0,
                LMCConnection.DefaultEventMask);
            return connection;
        }

        private static FakeRpcStep CapabilitiesStep(uint requestId)
        {
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(0, CapabilitiesPayload(requestId)));
        }

        private static FakeRpcStep ConfigureStep(uint requestId)
        {
            return new FakeRpcStep(
                0x7E30,
                TestFrame.Response(
                    0,
                    BulkStatusPayload(
                        requestId,
                        LMCBulkState.Pending,
                        0)));
        }

        private static byte[] CapabilitiesPayload(uint requestId)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 2);
            TestFrame.WriteUInt32(
                payload,
                20,
                (uint)(LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.PIRead
                    | LMCDiagnosticCapability.BulkSnapshot));
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt16(payload, 28, 2);
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

        private static byte[] CommonPayload(int length, uint requestId)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static byte[] DomainErrorPayload(
            uint requestId,
            LMCDiagnosticsDetailCode detail)
        {
            var payload = CommonPayload(16, requestId);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -32000);
            TestFrame.WriteUInt32(payload, 12, (uint)detail);
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
                TestFrame.Response(0, new byte[4]));
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, new byte[4]));
        }

        private static void AssertCommandCounts(
            FakeRpcServer server,
            int expectedReleaseCount)
        {
            var releaseCount = 0;
            var statusOrSnapshotCount = 0;
            foreach (var request in server.ReceivedRequests)
            {
                var command = TestFrame.ReadUInt16(request, 0);
                if (command == 0x7E33)
                {
                    releaseCount++;
                }
                else if (command == 0x7E31 || command == 0x7E32)
                {
                    statusOrSnapshotCount++;
                }
            }

            AssertEx.Equal(expectedReleaseCount, releaseCount);
            AssertEx.Equal(0, statusOrSnapshotCount);
        }
    }
}
