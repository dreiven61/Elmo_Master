using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsD2ContractTests
    {
        private const uint GoldenRequestId = 0x11223344u;
        private const uint MapRevision = 0x957F101Eu;
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const uint BulkId = 0xA1B2C3D4u;
        private const uint ConfigRevision = 0x01020304u;
        private const uint Signal1 = 0x00100104u;
        private const uint Signal2 = 0x00100105u;
        private const uint Signal3 = 0x00100106u;

        private static readonly uint[] Signals =
        {
            Signal1,
            Signal2,
            Signal3
        };

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Request.DiagnosticsD2.GoldenBytes",
                DiagnosticsD2RequestGoldenBytes);
            tests.Add(
                "Response.BulkStatus.GoldenAndMalformed",
                BulkStatusGoldenAndMalformed);
            tests.Add(
                "Response.BulkSnapshot.GoldenAndPartial",
                BulkSnapshotGoldenAndPartial);
            tests.Add(
                "Response.BulkSnapshot.MalformedRejected",
                BulkSnapshotMalformedRejected);
            tests.Add(
                "Response.ReleaseBulk.Contract",
                ReleaseBulkContract);
            tests.Add(
                "Rpc.DiagnosticsD2.SyncAndAsync",
                DiagnosticsD2SyncAndAsync);
            tests.Add(
                "Rpc.DiagnosticsD2.StatefulCancellationBoundary",
                DiagnosticsD2StatefulCancellationBoundary);
        }

        private static void DiagnosticsD2RequestGoldenBytes()
        {
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "30 7E 00 00 20 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "1E 10 7F 95 DD CC BB AA "
                    + "03 00 00 00 04 01 10 00 "
                    + "05 01 10 00 06 01 10 00"),
                LMC_DiagnosticsFrame.ConfigureBulk(
                    GoldenRequestId,
                    MapRevision,
                    0xAABBCCDDu,
                    Signals));

            AssertBulkIdentityRequest(
                0x7E31,
                LMC_DiagnosticsFrame.ReadBulkStatus(
                    GoldenRequestId,
                    BulkId,
                    ConfigRevision,
                    MapRevision));
            AssertBulkIdentityRequest(
                0x7E32,
                LMC_DiagnosticsFrame.ReadBulkSnapshot(
                    GoldenRequestId,
                    BulkId,
                    ConfigRevision,
                    MapRevision));
            AssertBulkIdentityRequest(
                0x7E33,
                LMC_DiagnosticsFrame.ReleaseBulk(
                    GoldenRequestId,
                    BulkId,
                    ConfigRevision,
                    MapRevision));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ConfigureBulk(
                    0,
                    MapRevision,
                    0,
                    Signals));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ConfigureBulk(
                    GoldenRequestId,
                    0,
                    0,
                    Signals));
            AssertEx.Throws<ArgumentNullException>(
                () => LMC_DiagnosticsFrame.ConfigureBulk(
                    GoldenRequestId,
                    MapRevision,
                    0,
                    null));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ConfigureBulk(
                    GoldenRequestId,
                    MapRevision,
                    0,
                    new uint[0]));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ConfigureBulk(
                    GoldenRequestId,
                    MapRevision,
                    0,
                    new uint[33]));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ConfigureBulk(
                    GoldenRequestId,
                    MapRevision,
                    0,
                    new[] { Signal1, 0u }));
            AssertEx.Throws<ArgumentException>(
                () => LMC_DiagnosticsFrame.ConfigureBulk(
                    GoldenRequestId,
                    MapRevision,
                    0,
                    new[] { Signal1, Signal1 }));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ReadBulkStatus(
                    GoldenRequestId,
                    0,
                    ConfigRevision,
                    MapRevision));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ReadBulkSnapshot(
                    GoldenRequestId,
                    BulkId,
                    0,
                    MapRevision));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_DiagnosticsFrame.ReleaseBulk(
                    GoldenRequestId,
                    BulkId,
                    ConfigRevision,
                    0));
        }

        private static void BulkStatusGoldenAndMalformed()
        {
            var configured = LMC_DiagnosticsParser.ParseConfigureBulk(
                TestFrame.Response(
                    0,
                    BulkStatusPayload(
                        GoldenRequestId,
                        LMCBulkState.Pending,
                        0)),
                GoldenRequestId,
                MapRevision,
                BulkId,
                3);

            AssertEx.Equal(BulkId, configured.BulkId);
            AssertEx.Equal(ConfigRevision, configured.ConfigRevision);
            AssertEx.Equal(MapRevision, configured.MapRevision);
            AssertEx.Equal(LMCBulkState.Pending, configured.State);
            AssertEx.Equal((ushort)3, configured.SignalCount);
            AssertEx.Equal(0u, configured.ActivationCycle);
            AssertEx.False(configured.IsActive);

            var active = LMC_DiagnosticsParser.ParseBulkStatus(
                TestFrame.Response(
                    0,
                    BulkStatusPayload(
                        GoldenRequestId,
                        LMCBulkState.Active,
                        100)),
                GoldenRequestId,
                BulkId,
                ConfigRevision,
                MapRevision,
                3);
            AssertEx.True(active.IsActive);
            AssertEx.Equal(100u, active.ActivationCycle);

            var failed = LMC_DiagnosticsParser.ParseBulkStatus(
                TestFrame.Response(
                    0,
                    BulkStatusPayload(
                        GoldenRequestId,
                        LMCBulkState.Failed,
                        0)),
                GoldenRequestId,
                BulkId,
                ConfigRevision,
                MapRevision,
                3);
            AssertEx.Equal(LMCBulkState.Failed, failed.State);

            var wrongBulkId = BulkStatusPayload(
                GoldenRequestId,
                LMCBulkState.Active,
                100);
            TestFrame.WriteUInt32(wrongBulkId, 16, BulkId + 1);
            AssertBulkStatusMalformed(wrongBulkId);

            var zeroConfigRevision = BulkStatusPayload(
                GoldenRequestId,
                LMCBulkState.Active,
                100);
            TestFrame.WriteUInt32(zeroConfigRevision, 20, 0);
            AssertBulkStatusMalformed(zeroConfigRevision);

            var wrongMapRevision = BulkStatusPayload(
                GoldenRequestId,
                LMCBulkState.Active,
                100);
            TestFrame.WriteUInt32(wrongMapRevision, 24, MapRevision + 1);
            AssertBulkStatusMalformed(wrongMapRevision);

            var emptyState = BulkStatusPayload(
                GoldenRequestId,
                LMCBulkState.Empty,
                0);
            AssertBulkStatusMalformed(emptyState);

            var unknownState = BulkStatusPayload(
                GoldenRequestId,
                LMCBulkState.Active,
                100);
            TestFrame.WriteUInt16(unknownState, 28, 4);
            AssertBulkStatusMalformed(unknownState);

            var wrongCount = BulkStatusPayload(
                GoldenRequestId,
                LMCBulkState.Active,
                100);
            TestFrame.WriteUInt16(wrongCount, 30, 2);
            AssertBulkStatusMalformed(wrongCount);

            var flags = BulkStatusPayload(
                GoldenRequestId,
                LMCBulkState.Active,
                100);
            TestFrame.WriteUInt16(
                flags,
                2,
                (ushort)LMCDiagnosticsResponseFlags.Partial);
            AssertBulkStatusMalformed(flags);

            var truncated = BulkStatusPayload(
                GoldenRequestId,
                LMCBulkState.Active,
                100);
            Array.Resize(ref truncated, truncated.Length - 1);
            AssertBulkStatusMalformed(truncated);

            var configureFailed = BulkStatusPayload(
                GoldenRequestId,
                LMCBulkState.Failed,
                0);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseConfigureBulk(
                    TestFrame.Response(0, configureFailed),
                    GoldenRequestId,
                    MapRevision,
                    BulkId,
                    3));

            var requestedIdMismatch = BulkStatusPayload(
                GoldenRequestId,
                LMCBulkState.Pending,
                0);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseConfigureBulk(
                    TestFrame.Response(0, requestedIdMismatch),
                    GoldenRequestId,
                    MapRevision,
                    BulkId + 1,
                    3));

            var domainError = DomainErrorPayload(
                GoldenRequestId,
                LMCDiagnosticsDetailCode.ResourceBusy);
            var exception = AssertEx.Throws<LMCDiagnosticsCommandException>(
                () => LMC_DiagnosticsParser.ParseBulkStatus(
                    TestFrame.Response(0, domainError),
                    GoldenRequestId,
                    BulkId,
                    ConfigRevision,
                    MapRevision,
                    3));
            AssertEx.Equal(
                LMCDiagnosticsDetailCode.ResourceBusy,
                exception.Response.Detail);

            Array.Resize(ref domainError, domainError.Length + 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseBulkStatus(
                    TestFrame.Response(0, domainError),
                    GoldenRequestId,
                    BulkId,
                    ConfigRevision,
                    MapRevision,
                    3));
        }

        private static void BulkSnapshotGoldenAndPartial()
        {
            var snapshot = LMC_DiagnosticsParser.ParseBulkSnapshot(
                TestFrame.Response(
                    0,
                    BulkSnapshotPayload(GoldenRequestId, false)),
                GoldenRequestId,
                BulkId,
                ConfigRevision,
                MapRevision,
                Signals);

            AssertEx.Equal(BulkId, snapshot.BulkId);
            AssertEx.Equal(ConfigRevision, snapshot.ConfigRevision);
            AssertEx.Equal(MapRevision, snapshot.MapRevision);
            AssertEx.Equal(100u, snapshot.CycleCounter);
            AssertEx.Equal(0x0000000200000001ul, snapshot.TimestampUs);
            AssertEx.Equal((ushort)16, snapshot.EntryStride);
            AssertEx.Equal(LMCCapturePhase.InputMapped, snapshot.CapturePhase);
            AssertEx.Equal(10u, snapshot.SnapshotSequence);
            AssertEx.Equal(
                LMCBulkSnapshotFlags.SameCycle
                    | LMCBulkSnapshotFlags.InputMappedPhase,
                snapshot.SnapshotFlags);
            AssertEx.Equal((ushort)3, snapshot.EntryCount);
            AssertEx.Equal(3, snapshot.Entries.Count);
            AssertEx.Equal(Signal1, snapshot.Entries[0].SignalId);
            AssertEx.Equal(-12345, snapshot.Entries[0].RawInt32);
            AssertEx.Equal(
                LMCSignalValueType.BitField32,
                snapshot.Entries[1].ValueType);
            AssertEx.True(snapshot.Entries[2].IsValid);
            AssertEx.False(snapshot.IsPartial);

            var partial = LMC_DiagnosticsParser.ParseBulkSnapshot(
                TestFrame.Response(
                    0,
                    BulkSnapshotPayload(GoldenRequestId, true)),
                GoldenRequestId,
                BulkId,
                ConfigRevision,
                MapRevision,
                Signals);
            AssertEx.True(partial.IsPartial);
            AssertEx.False(partial.Entries[1].IsValid);
            AssertEx.Equal(
                LMCSignalEntryStatus.SlaveOffline,
                partial.Entries[1].EntryStatus);
            AssertEx.Equal(
                LMCDiagnosticsDetailCode.SlaveOffline,
                partial.Entries[1].Detail);
        }

        private static void BulkSnapshotMalformedRejected()
        {
            var wrongBulkId = BulkSnapshotPayload(GoldenRequestId, false);
            TestFrame.WriteUInt32(wrongBulkId, 16, BulkId + 1);
            AssertBulkSnapshotMalformed(wrongBulkId);

            var wrongConfigRevision = BulkSnapshotPayload(GoldenRequestId, false);
            TestFrame.WriteUInt32(wrongConfigRevision, 20, ConfigRevision + 1);
            AssertBulkSnapshotMalformed(wrongConfigRevision);

            var wrongMapRevision = BulkSnapshotPayload(GoldenRequestId, false);
            TestFrame.WriteUInt32(wrongMapRevision, 24, MapRevision + 1);
            AssertBulkSnapshotMalformed(wrongMapRevision);

            var wrongCount = BulkSnapshotPayload(GoldenRequestId, false);
            TestFrame.WriteUInt16(wrongCount, 40, 2);
            AssertBulkSnapshotMalformed(wrongCount);

            var wrongStride = BulkSnapshotPayload(GoldenRequestId, false);
            TestFrame.WriteUInt16(wrongStride, 42, 15);
            AssertBulkSnapshotMalformed(wrongStride);

            var invalidPhase = BulkSnapshotPayload(GoldenRequestId, false);
            invalidPhase[44] = 0;
            AssertBulkSnapshotMalformed(invalidPhase);

            var nonzeroReserved = BulkSnapshotPayload(GoldenRequestId, false);
            nonzeroReserved[45] = 1;
            AssertBulkSnapshotMalformed(nonzeroReserved);

            var oddSequence = BulkSnapshotPayload(GoldenRequestId, false);
            TestFrame.WriteUInt32(oddSequence, 48, 11);
            AssertBulkSnapshotMalformed(oddSequence);

            var missingSameCycle = BulkSnapshotPayload(GoldenRequestId, false);
            TestFrame.WriteUInt32(
                missingSameCycle,
                52,
                (uint)LMCBulkSnapshotFlags.InputMappedPhase);
            AssertBulkSnapshotMalformed(missingSameCycle);

            var mixedPhase = BulkSnapshotPayload(GoldenRequestId, false);
            TestFrame.WriteUInt32(
                mixedPhase,
                52,
                (uint)(LMCBulkSnapshotFlags.SameCycle
                    | LMCBulkSnapshotFlags.InputMappedPhase
                    | LMCBulkSnapshotFlags.PreOutputPhase));
            AssertBulkSnapshotMalformed(mixedPhase);

            var unknownFlag = BulkSnapshotPayload(GoldenRequestId, false);
            TestFrame.WriteUInt32(unknownFlag, 52, 0x80000003u);
            AssertBulkSnapshotMalformed(unknownFlag);

            var lastChunk = BulkSnapshotPayload(GoldenRequestId, false);
            TestFrame.WriteUInt16(
                lastChunk,
                2,
                (ushort)LMCDiagnosticsResponseFlags.LastChunk);
            AssertBulkSnapshotMalformed(lastChunk);

            var partialWithoutInvalid = BulkSnapshotPayload(
                GoldenRequestId,
                false);
            TestFrame.WriteUInt16(
                partialWithoutInvalid,
                2,
                (ushort)LMCDiagnosticsResponseFlags.Partial);
            AssertBulkSnapshotMalformed(partialWithoutInvalid);

            var invalidWithoutPartial = BulkSnapshotPayload(
                GoldenRequestId,
                true);
            TestFrame.WriteUInt16(invalidWithoutPartial, 2, 0);
            AssertBulkSnapshotMalformed(invalidWithoutPartial);

            var wrongSignalOrder = BulkSnapshotPayload(GoldenRequestId, false);
            TestFrame.WriteUInt32(wrongSignalOrder, 56, Signal2);
            AssertBulkSnapshotMalformed(wrongSignalOrder);

            var entryReserved = BulkSnapshotPayload(GoldenRequestId, false);
            TestFrame.WriteUInt16(entryReserved, 56 + 10, 1);
            AssertBulkSnapshotMalformed(entryReserved);

            var truncated = BulkSnapshotPayload(GoldenRequestId, false);
            Array.Resize(ref truncated, truncated.Length - 1);
            AssertBulkSnapshotMalformed(truncated);

            var oversized = BulkSnapshotPayload(GoldenRequestId, false);
            Array.Resize(ref oversized, oversized.Length + 1);
            AssertBulkSnapshotMalformed(oversized);

            AssertEx.Throws<ArgumentNullException>(
                () => LMC_DiagnosticsParser.ParseBulkSnapshot(
                    TestFrame.Response(
                        0,
                        BulkSnapshotPayload(GoldenRequestId, false)),
                    GoldenRequestId,
                    BulkId,
                    ConfigRevision,
                    MapRevision,
                    null));
        }

        private static void ReleaseBulkContract()
        {
            var response = LMC_DiagnosticsParser.ParseReleaseBulk(
                TestFrame.Response(
                    0,
                    CommonPayload(16, GoldenRequestId)),
                GoldenRequestId);
            AssertEx.True(response.IsSuccess);

            var flags = CommonPayload(16, GoldenRequestId);
            TestFrame.WriteUInt16(
                flags,
                2,
                (ushort)LMCDiagnosticsResponseFlags.Partial);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseReleaseBulk(
                    TestFrame.Response(0, flags),
                    GoldenRequestId));

            var oversized = CommonPayload(17, GoldenRequestId);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseReleaseBulk(
                    TestFrame.Response(0, oversized),
                    GoldenRequestId));

            var domainError = DomainErrorPayload(
                GoldenRequestId,
                LMCDiagnosticsDetailCode.HandleOrGenerationStale);
            var exception = AssertEx.Throws<LMCDiagnosticsCommandException>(
                () => LMC_DiagnosticsParser.ParseReleaseBulk(
                    TestFrame.Response(0, domainError),
                    GoldenRequestId));
            AssertEx.Equal(
                LMCDiagnosticsDetailCode.HandleOrGenerationStale,
                exception.Response.Detail);
        }

        private static void DiagnosticsD2SyncAndAsync()
        {
            RunDiagnosticsD2Integration(false);
            RunDiagnosticsD2Integration(true);
        }

        private static void RunDiagnosticsD2Integration(bool useAsync)
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
                    Signals),
                request);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                configureStep,
                new FakeRpcStep(
                    0x7E31,
                    TestFrame.Response(
                        0,
                        BulkStatusPayload(3, LMCBulkState.Active, 100))),
                new FakeRpcStep(
                    0x7E32,
                    TestFrame.Response(
                        0,
                        BulkSnapshotPayload(4, false))),
                new FakeRpcStep(
                    0x7E33,
                    TestFrame.Response(0, CommonPayload(16, 5))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var callerSignalIds = (uint[])Signals.Clone();
                LMCBulkConfiguration configuration;
                LMCBulkStatus status;
                LMCBulkSnapshot snapshot;

                if (useAsync)
                {
                    configuration = connection.Diagnostics.ConfigureBulkAsync(
                            callerSignalIds,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    status = connection.Diagnostics.ReadBulkStatusAsync(
                            configuration,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    snapshot = connection.Diagnostics.ReadBulkAsync(
                            configuration,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    configuration = connection.Diagnostics.ConfigureBulk(
                        callerSignalIds);
                    status = connection.Diagnostics.ReadBulkStatus(configuration);
                    snapshot = connection.Diagnostics.ReadBulk(configuration);
                }

                callerSignalIds[0] = 0;
                AssertEx.Equal(DiagnosticsBootId, configuration.DiagnosticsBootId);
                AssertEx.Equal(BulkId, configuration.BulkId);
                AssertEx.Equal(ConfigRevision, configuration.ConfigRevision);
                AssertEx.Equal(Signal1, configuration.SignalIds[0]);
                AssertEx.True(status.IsActive);
                AssertEx.Equal(100u, snapshot.CycleCounter);

                using (var otherConnection = new LMCConnection())
                {
                    AssertEx.Throws<InvalidOperationException>(
                        () => otherConnection.Diagnostics.ReadBulk(configuration));
                }

                if (useAsync)
                {
                    connection.Diagnostics.ReleaseBulkAsync(
                            configuration,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    connection.Diagnostics.ReleaseBulk(configuration);
                }

                AssertEx.True(configuration.IsReleased);
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.ReadBulkStatus(configuration));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void DiagnosticsD2StatefulCancellationBoundary()
        {
            using (var configureCancellation = new CancellationTokenSource())
            using (var releaseCancellation = new CancellationTokenSource())
            {
                var configureStep = new FakeRpcStep(
                    0x7E30,
                    TestFrame.Response(
                        0,
                        BulkStatusPayload(2, LMCBulkState.Pending, 0)))
                {
                    InspectRequest = request => configureCancellation.Cancel()
                };
                var releaseStep = new FakeRpcStep(
                    0x7E33,
                    TestFrame.Response(0, CommonPayload(16, 3)))
                {
                    InspectRequest = request => releaseCancellation.Cancel()
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    new FakeRpcStep(
                        0x7E00,
                        TestFrame.Response(0, CapabilitiesPayload(1))),
                    configureStep,
                    releaseStep,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask);

                    var configuration =
                        connection.Diagnostics.ConfigureBulkAsync(
                                Signals,
                                configureCancellation.Token)
                            .GetAwaiter()
                            .GetResult();
                    AssertEx.True(configureCancellation.IsCancellationRequested);
                    AssertEx.Equal(BulkId, configuration.BulkId);

                    connection.Diagnostics.ReleaseBulkAsync(
                            configuration,
                            releaseCancellation.Token)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(releaseCancellation.IsCancellationRequested);
                    AssertEx.True(configuration.IsReleased);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void AssertBulkIdentityRequest(
            ushort expectedCommand,
            byte[] actual)
        {
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    expectedCommand.ToString("X4").Substring(2, 2)
                    + " "
                    + expectedCommand.ToString("X4").Substring(0, 2)
                    + " 00 00 14 00 00 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "D4 C3 B2 A1 04 03 02 01 "
                    + "1E 10 7F 95"),
                actual);
        }

        private static void AssertBulkStatusMalformed(byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseBulkStatus(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    BulkId,
                    ConfigRevision,
                    MapRevision,
                    3));
        }

        private static void AssertBulkSnapshotMalformed(byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_DiagnosticsParser.ParseBulkSnapshot(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    BulkId,
                    ConfigRevision,
                    MapRevision,
                    Signals));
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
            TestFrame.WriteUInt16(payload, 30, 3);
            TestFrame.WriteUInt32(payload, 32, activationCycle);
            return payload;
        }

        private static byte[] BulkSnapshotPayload(
            uint requestId,
            bool partial)
        {
            var payload = CommonPayload(
                56 + Signals.Length * 16,
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
            TestFrame.WriteUInt16(payload, 40, 3);
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
                unchecked((uint)-12345),
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
            WriteValueEntry(
                payload,
                88,
                Signal3,
                0x00001234u,
                LMCSignalValueType.BitField16,
                LMCSignalEntryStatus.Valid,
                LMCDiagnosticsDetailCode.None);
            return payload;
        }

        private static void WriteValueEntry(
            byte[] payload,
            int offset,
            uint signalId,
            uint rawValue32,
            LMCSignalValueType valueType,
            LMCSignalEntryStatus entryStatus,
            LMCDiagnosticsDetailCode detailCode)
        {
            TestFrame.WriteUInt32(payload, offset, signalId);
            TestFrame.WriteUInt32(payload, offset + 4, rawValue32);
            payload[offset + 8] = (byte)valueType;
            payload[offset + 9] = (byte)entryStatus;
            TestFrame.WriteUInt32(payload, offset + 12, (uint)detailCode);
        }

        private static byte[] CapabilitiesPayload(uint requestId)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 2);
            TestFrame.WriteUInt32(
                payload,
                20,
                (uint)(LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.BulkSnapshot));
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt16(payload, 28, 24);
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
