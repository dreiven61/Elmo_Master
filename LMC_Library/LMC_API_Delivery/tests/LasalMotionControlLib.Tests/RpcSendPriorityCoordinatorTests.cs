using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class RpcSendPriorityCoordinatorTests
    {
        private const ushort AxisReference = 0x0001;
        private const ushort GroupReference = 0x0100;
        private const uint MapRevision = 0x957F101Eu;
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const uint RecorderConfigId = 0x10203040u;
        private const uint RecorderConfigRevision = 0x01020304u;
        private const uint RecorderOwnerSessionEpoch = 0x55667788u;
        private const uint RecorderReconnectedOwnerSessionEpoch = 0x66778899u;
        private const uint RecorderRecordId = 0xA1B2C3D4u;
        private const uint RecorderSignal1 = 0x00100104u;
        private const uint RecorderSignal2 = 0x00100105u;
        private const uint TopologyRevision = 0xA1B2C3D4u;
        private const uint TopologyNodeId = 0x00000101u;
        private const uint IOReference = 0x00000501u;
        private static readonly Guid RecorderRecoveryToken = new Guid(
            "01234567-89ab-cdef-0123-456789abcdef");

        private enum RecorderAcceptedPath
        {
            Configure = 0,
            ConfigureRecoverableDouble = 1,
            Start = 2,
            AdoptExact = 3,
            AdoptActive = 4,
            AdoptEmpty = 5
        }

        private enum RecorderReleasePath
        {
            ConfigurationHandle = 0,
            IdentityBuffer = 1,
            RecoveredConfiguration = 2,
            AdoptedIdentityConfiguration = 3
        }

        private static readonly uint[] RecorderSignals =
        {
            RecorderSignal1,
            RecorderSignal2
        };

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Rpc.SendPriority.AsyncLocalFlowsThroughExchangeAsyncTaskRun",
                AsyncLocalFlowsThroughExchangeAsyncTaskRun);
            tests.Add(
                "Rpc.SendPriority.DelayedInFlightPreemptsQueuedNormalForStop",
                DelayedInFlightPreemptsQueuedNormalForStop);
            tests.Add(
                "Rpc.SendPriority.CompoundScopeSecondRpcPreempted",
                CompoundScopeSecondRpcPreempted);
            tests.Add(
                "Rpc.SendPriority.PriorityScopeOverridesInheritedStaleNormal",
                PriorityScopeOverridesInheritedStaleNormal);
            tests.Add(
                "Rpc.SendPriority.StalePriorityScopePreemptedBeforeWire",
                StalePriorityScopePreemptedBeforeWire);
            tests.Add(
                "Rpc.SendPriority.OutOfOrderDisposeRecoversScopeStack",
                OutOfOrderDisposeRecoversScopeStack);
            tests.Add(
                "Rpc.SendPriority.SdoSubmissionPreemptedBeforeWire",
                SdoSubmissionPreemptedBeforeWire);
            tests.Add(
                "Rpc.SendPriority.DigitalOutputSubmissionPreemptedBeforeWire",
                DigitalOutputSubmissionPreemptedBeforeWire);
            tests.Add(
                "Rpc.SendPriority.DigitalOutputSubmissionSyncDelayedAckDiscarded",
                DigitalOutputSubmissionSyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.DigitalOutputSubmissionAsyncDelayedAckDiscarded",
                DigitalOutputSubmissionAsyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.EtherCATNodeHealthSyncDelayedAckDiscarded",
                EtherCATNodeHealthSyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.EtherCATNodeHealthAsyncDelayedAckDiscarded",
                EtherCATNodeHealthAsyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.DigitalIOReadSyncDelayedAckDiscarded",
                DigitalIOReadSyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.DigitalIOReadAsyncDelayedAckDiscarded",
                DigitalIOReadAsyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.ResultPublicationIsAtomicWithReservation",
                ResultPublicationIsAtomicWithReservation);
            tests.Add(
                "Rpc.SendPriority.GroupGenericSyncDelayedAckDiscarded",
                GroupGenericSyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.GroupGenericAsyncDelayedAckDiscarded",
                GroupGenericAsyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.AxisGenericSyncDelayedAckDiscarded",
                AxisGenericSyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.AxisGenericAsyncDelayedAckDiscarded",
                AxisGenericAsyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.AdminGroupMoveSyncDelayedAckDiscarded",
                AdminGroupMoveSyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.AdminGroupMoveAsyncDelayedAckDiscarded",
                AdminGroupMoveAsyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.GroupEnableWaitDelayedAckDiscarded",
                GroupEnableWaitDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.SubmitSdoSyncDelayedAckDiscarded",
                SubmitSdoSyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.SubmitSdoAsyncDelayedAckDiscarded",
                SubmitSdoAsyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.CancelOperationSyncDelayedAckDiscarded",
                CancelOperationSyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.CancelOperationAsyncDelayedAckDiscarded",
                CancelOperationAsyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.RecorderConfigureSyncDelayedAckAcceptedResultPreserved",
                () => RunRecorderAcceptedResultDelayedAck(
                    RecorderAcceptedPath.Configure,
                    false));
            tests.Add(
                "Rpc.SendPriority.RecorderConfigureAsyncDelayedAckAcceptedResultPreserved",
                () => RunRecorderAcceptedResultDelayedAck(
                    RecorderAcceptedPath.Configure,
                    true));
            tests.Add(
                "Rpc.SendPriority.RecorderRecoverableConfigureSyncDelayedAckAcceptedResultPreserved",
                () => RunRecorderAcceptedResultDelayedAck(
                    RecorderAcceptedPath.ConfigureRecoverableDouble,
                    false));
            tests.Add(
                "Rpc.SendPriority.RecorderRecoverableConfigureAsyncDelayedAckAcceptedResultPreserved",
                () => RunRecorderAcceptedResultDelayedAck(
                    RecorderAcceptedPath.ConfigureRecoverableDouble,
                    true));
            tests.Add(
                "Rpc.SendPriority.RecorderStartSyncDelayedAckAcceptedResultPreserved",
                () => RunRecorderAcceptedResultDelayedAck(
                    RecorderAcceptedPath.Start,
                    false));
            tests.Add(
                "Rpc.SendPriority.RecorderStartAsyncDelayedAckAcceptedResultPreserved",
                () => RunRecorderAcceptedResultDelayedAck(
                    RecorderAcceptedPath.Start,
                    true));
            tests.Add(
                "Rpc.SendPriority.RecorderAdoptSyncDelayedAckAcceptedResultPreserved",
                () => RunRecorderAcceptedResultDelayedAck(
                    RecorderAcceptedPath.AdoptExact,
                    false));
            tests.Add(
                "Rpc.SendPriority.RecorderAdoptAsyncDelayedAckAcceptedResultPreserved",
                () => RunRecorderAcceptedResultDelayedAck(
                    RecorderAcceptedPath.AdoptExact,
                    true));
            tests.Add(
                "Rpc.SendPriority.RecorderAdoptActiveSyncDelayedAckAcceptedResultPreserved",
                () => RunRecorderAcceptedResultDelayedAck(
                    RecorderAcceptedPath.AdoptActive,
                    false));
            tests.Add(
                "Rpc.SendPriority.RecorderAdoptActiveAsyncDelayedAckAcceptedResultPreserved",
                () => RunRecorderAcceptedResultDelayedAck(
                    RecorderAcceptedPath.AdoptActive,
                    true));
            tests.Add(
                "Rpc.SendPriority.RecorderAdoptEmptySyncDelayedAckAcceptedResultPreserved",
                () => RunRecorderAcceptedResultDelayedAck(
                    RecorderAcceptedPath.AdoptEmpty,
                    false));
            tests.Add(
                "Rpc.SendPriority.RecorderAdoptEmptyAsyncDelayedAckAcceptedResultPreserved",
                () => RunRecorderAcceptedResultDelayedAck(
                    RecorderAcceptedPath.AdoptEmpty,
                    true));
            tests.Add(
                "Rpc.SendPriority.RecorderTriggerSyncDelayedAckDiscarded",
                RecorderTriggerSyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.RecorderTriggerAsyncDelayedAckDiscarded",
                RecorderTriggerAsyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.RecorderStopSyncDelayedAckDiscarded",
                RecorderStopSyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.RecorderStopAsyncDelayedAckDiscarded",
                RecorderStopAsyncDelayedAckDiscarded);
            tests.Add(
                "Rpc.SendPriority.RecorderBufferReleaseSyncDelayedAckQuarantined",
                RecorderBufferReleaseSyncDelayedAckQuarantined);
            tests.Add(
                "Rpc.SendPriority.RecorderBufferReleaseAsyncDelayedAckQuarantined",
                RecorderBufferReleaseAsyncDelayedAckQuarantined);
            tests.Add(
                "Rpc.SendPriority.RecorderHandleReleaseSyncDelayedAckQuarantined",
                RecorderHandleReleaseSyncDelayedAckQuarantined);
            tests.Add(
                "Rpc.SendPriority.RecorderHandleReleaseAsyncDelayedAckQuarantined",
                RecorderHandleReleaseAsyncDelayedAckQuarantined);
            tests.Add(
                "Rpc.SendPriority.RecorderRecoveredReleaseSyncDelayedAckQuarantined",
                RecorderRecoveredReleaseSyncDelayedAckQuarantined);
            tests.Add(
                "Rpc.SendPriority.RecorderRecoveredReleaseAsyncDelayedAckQuarantined",
                RecorderRecoveredReleaseAsyncDelayedAckQuarantined);
            tests.Add(
                "Rpc.SendPriority.RecorderIdentityReleaseSyncDelayedAckQuarantined",
                RecorderIdentityReleaseSyncDelayedAckQuarantined);
            tests.Add(
                "Rpc.SendPriority.RecorderIdentityReleaseAsyncDelayedAckQuarantined",
                RecorderIdentityReleaseAsyncDelayedAckQuarantined);
            tests.Add(
                "Rpc.Recorder.ConcurrentStartGuardsSyncAndAsync",
                RecorderConcurrentStartGuardsSyncAndAsync);
            tests.Add(
                "Rpc.Recorder.ReleaseBeforeWireRollsBackAllLeaseTypes",
                RecorderReleaseBeforeWireRollsBackAllLeaseTypes);
        }

        private static void AsyncLocalFlowsThroughExchangeAsyncTaskRun()
        {
            const string operation = "AsyncLocal stale normal";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };
            var normal = LMC_Frame.LMCGroupEnable(GroupReference);
            var command = LMC_Frame.GetRequestCommand(normal);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                Connect(connection, server.Port);
                var expectedGeneration = coordinator.CurrentGeneration;
                var actualGeneration = coordinator.ReservePrioritySend();

                LMCSendPreemptedException error;
                using (coordinator.BeginPreemptibleScope(
                    expectedGeneration,
                    operation))
                {
                    error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => connection.ExchangeAsync(
                                normal,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                }

                AssertPreempted(
                    error,
                    operation,
                    command,
                    expectedGeneration,
                    actualGeneration);
                AssertEx.True(connection.IsConnected);

                connection.CloseConnection();
                server.Verify();
                AssertCommands(server, 0x8080, 0x405C, 0x405D);
            }
        }

        private static void DelayedInFlightPreemptsQueuedNormalForStop()
        {
            const string normalOperation = "Queued normal command";
            const string priorityOperation = "Reserved priority Stop";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };
            var activeRequest = LMC_Frame.LMCGroupReadStatus(GroupReference);
            var normalRequest = LMC_Frame.LMCGroupEnable(GroupReference);
            var stopRequest = LMC_Frame.LMCGroupStop(GroupReference, 1, 0);

            using (var activeRequestReceived = new ManualResetEventSlim(false))
            using (var releaseActiveRequest = new ManualResetEventSlim(false))
            using (var normalScopeReady = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    LMC_Frame.GetRequestCommand(activeRequest),
                    TestFrame.Response(0, new byte[12]))
                {
                    InspectRequest = request =>
                    {
                        activeRequestReceived.Set();
                        AssertEx.True(
                            releaseActiveRequest.Wait(5000),
                            "The delayed in-flight request was not released.");
                    }
                },
                new FakeRpcStep(
                    LMC_Frame.GetRequestCommand(stopRequest),
                    TestFrame.Response(0, new byte[8])),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var activeTask = connection.ExchangeAsync(
                        activeRequest,
                        CancellationToken.None);
                    AssertEx.True(
                        activeRequestReceived.Wait(2000),
                        "The in-flight request did not reach the server.");

                    var expectedGeneration = coordinator.CurrentGeneration;
                    var normalTask = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                normalScopeReady.Set();
                                return connection.ExchangeAsync(
                                        normalRequest,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });
                    AssertEx.True(
                        normalScopeReady.Wait(2000),
                        "The normal request did not enter its queued scope.");

                    var reservedGeneration = coordinator.ReservePrioritySend();
                    byte[] stopResponse;
                    byte[] activeResponse;
                    using (coordinator.BeginPriorityScope(
                        reservedGeneration,
                        priorityOperation))
                    {
                        var stopTask = connection.ExchangeAsync(
                            stopRequest,
                            CancellationToken.None);
                        releaseActiveRequest.Set();
                        activeResponse = activeTask.GetAwaiter().GetResult();
                        stopResponse = stopTask.GetAwaiter().GetResult();
                    }

                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => normalTask.GetAwaiter().GetResult());
                    AssertPreempted(
                        error,
                        normalOperation,
                        LMC_Frame.GetRequestCommand(normalRequest),
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.Equal(20, activeResponse.Length);
                    AssertEx.Equal(16, stopResponse.Length);
                    AssertEx.True(connection.IsConnected);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        0x8080,
                        0x405C,
                        LMC_Frame.GetRequestCommand(activeRequest),
                        LMC_Frame.GetRequestCommand(stopRequest),
                        0x405D);
                }
                finally
                {
                    releaseActiveRequest.Set();
                }
            }
        }

        private static void CompoundScopeSecondRpcPreempted()
        {
            const string operation = "Compound normal operation";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };
            var firstRequest = LMC_Frame.LMCGroupReadStatus(GroupReference);
            var secondRequest = LMC_Frame.LMCGroupEnable(GroupReference);
            long reservedGeneration = 0;

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    LMC_Frame.GetRequestCommand(firstRequest),
                    TestFrame.Response(0, new byte[12]))
                {
                    InspectRequest = request =>
                    {
                        reservedGeneration = coordinator.ReservePrioritySend();
                    }
                },
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                Connect(connection, server.Port);
                var expectedGeneration = coordinator.CurrentGeneration;

                byte[] firstResponse;
                LMCSendPreemptedException error;
                using (coordinator.BeginPreemptibleScope(
                    expectedGeneration,
                    operation))
                {
                    firstResponse = connection.ExchangeAsync(
                            firstRequest,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => connection.ExchangeAsync(
                                secondRequest,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                }

                AssertEx.Equal(20, firstResponse.Length);
                AssertPreempted(
                    error,
                    operation,
                    LMC_Frame.GetRequestCommand(secondRequest),
                    expectedGeneration,
                    reservedGeneration);
                AssertEx.True(connection.IsConnected);

                connection.CloseConnection();
                server.Verify();
                AssertCommands(
                    server,
                    0x8080,
                    0x405C,
                    LMC_Frame.GetRequestCommand(firstRequest),
                    0x405D);
            }
        }

        private static void PriorityScopeOverridesInheritedStaleNormal()
        {
            const string normalOperation = "Inherited stale normal";
            const string priorityOperation = "Nested priority Stop";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };
            var normalRequest = LMC_Frame.LMCGroupEnable(GroupReference);
            var stopRequest = LMC_Frame.LMCGroupStop(GroupReference, 1, 0);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    LMC_Frame.GetRequestCommand(stopRequest),
                    TestFrame.Response(0, new byte[8])),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                Connect(connection, server.Port);
                var expectedGeneration = coordinator.CurrentGeneration;
                var reservedGeneration = coordinator.ReservePrioritySend();

                LMCSendPreemptedException firstError;
                LMCSendPreemptedException nestedNormalError;
                LMCSendPreemptedException secondError;
                byte[] stopResponse;
                using (coordinator.BeginPreemptibleScope(
                    expectedGeneration,
                    normalOperation))
                {
                    firstError = AssertEx.Throws<LMCSendPreemptedException>(
                        () => connection.ExchangeAsync(
                                normalRequest,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());

                    using (coordinator.BeginPreemptibleScope(
                        reservedGeneration,
                        "Nested fresh normal"))
                    {
                        nestedNormalError =
                            AssertEx.Throws<LMCSendPreemptedException>(
                                () => connection.ExchangeAsync(
                                        normalRequest,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult());
                    }

                    using (coordinator.BeginPriorityScope(
                        reservedGeneration,
                        priorityOperation))
                    {
                        stopResponse = connection.ExchangeAsync(
                                stopRequest,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }

                    secondError = AssertEx.Throws<LMCSendPreemptedException>(
                        () => connection.ExchangeAsync(
                                normalRequest,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                }

                AssertPreempted(
                    firstError,
                    normalOperation,
                    LMC_Frame.GetRequestCommand(normalRequest),
                    expectedGeneration,
                    reservedGeneration);
                AssertPreempted(
                    nestedNormalError,
                    normalOperation,
                    LMC_Frame.GetRequestCommand(normalRequest),
                    expectedGeneration,
                    reservedGeneration);
                AssertPreempted(
                    secondError,
                    normalOperation,
                    LMC_Frame.GetRequestCommand(normalRequest),
                    expectedGeneration,
                    reservedGeneration);
                AssertEx.Equal(16, stopResponse.Length);
                AssertEx.True(connection.IsConnected);

                connection.CloseConnection();
                server.Verify();
                AssertCommands(
                    server,
                    0x8080,
                    0x405C,
                    LMC_Frame.GetRequestCommand(stopRequest),
                    0x405D);
            }
        }

        private static void StalePriorityScopePreemptedBeforeWire()
        {
            const string operation = "Superseded priority Stop";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };
            var stopRequest = LMC_Frame.LMCGroupStop(GroupReference, 1, 0);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                Connect(connection, server.Port);
                var generationA = coordinator.ReservePrioritySend();

                LMCSendPreemptedException error;
                long generationB;
                using (coordinator.BeginPriorityScope(
                    generationA,
                    operation))
                {
                    generationB = coordinator.ReservePrioritySend();
                    error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => connection.ExchangeAsync(
                                stopRequest,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                }

                AssertPreempted(
                    error,
                    operation,
                    LMC_Frame.GetRequestCommand(stopRequest),
                    generationA,
                    generationB);
                AssertEx.True(connection.IsConnected);

                connection.CloseConnection();
                server.Verify();
                AssertCommands(server, 0x8080, 0x405C, 0x405D);
            }
        }

        private static void OutOfOrderDisposeRecoversScopeStack()
        {
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };
            var freshRequest = LMC_Frame.LMCGroupReadStatus(GroupReference);
            var unscopedRequest = LMC_Frame.LMCGroupEnable(GroupReference);
            byte[] freshResponse = null;
            byte[] unscopedResponse = null;

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    LMC_Frame.GetRequestCommand(freshRequest),
                    TestFrame.Response(0, new byte[12])),
                new FakeRpcStep(
                    LMC_Frame.GetRequestCommand(unscopedRequest),
                    TestFrame.Response(0, new byte[8])),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                Connect(connection, server.Port);

                Task.Run(
                    () =>
                    {
                        var generation = coordinator.CurrentGeneration;
                        var outer = coordinator.BeginPreemptibleScope(
                            generation,
                            "Outer scope");
                        var inner = coordinator.BeginPreemptibleScope(
                            generation,
                            "Inner scope");
                        try
                        {
                            AssertEx.Throws<InvalidOperationException>(
                                () => outer.Dispose());
                            inner.Dispose();
                            outer.Dispose();

                            var freshGeneration =
                                coordinator.ReservePrioritySend();
                            using (coordinator.BeginPreemptibleScope(
                                freshGeneration,
                                "Fresh scope"))
                            {
                                freshResponse = connection.ExchangeAsync(
                                        freshRequest,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }

                            unscopedResponse = connection.ExchangeAsync(
                                    unscopedRequest,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        }
                        finally
                        {
                            inner.Dispose();
                            outer.Dispose();
                        }
                    })
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(20, freshResponse.Length);
                AssertEx.Equal(16, unscopedResponse.Length);
                AssertEx.True(connection.IsConnected);

                connection.CloseConnection();
                server.Verify();
                AssertCommands(
                    server,
                    0x8080,
                    0x405C,
                    LMC_Frame.GetRequestCommand(freshRequest),
                    LMC_Frame.GetRequestCommand(unscopedRequest),
                    0x405D);
            }
        }

        private static void SdoSubmissionPreemptedBeforeWire()
        {
            const string operation = "SDO submission";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };
            var request = LMCSdoRequest.CreateRead(
                1,
                0x1018,
                1,
                LMCSignalValueType.UInt32,
                4,
                100);
            long reservedGeneration = 0;

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, SdoCapabilitiesPayload(1)))
                {
                    InspectRequest = capabilityRequest =>
                    {
                        reservedGeneration = coordinator.ReservePrioritySend();
                    }
                },
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                Connect(connection, server.Port);
                var expectedGeneration = coordinator.CurrentGeneration;

                LMCSendPreemptedException error;
                using (coordinator.BeginPreemptibleScope(
                    expectedGeneration,
                    operation))
                {
                    error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => connection.Diagnostics.SubmitSdoAsync(
                                request,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                }

                AssertPreempted(
                    error,
                    operation,
                    0x7E50,
                    expectedGeneration,
                    reservedGeneration);

                LMCSdoSubmissionFailureContext context;
                AssertEx.True(
                    LMCSdoSubmissionFailureContext.TryGet(error, out context),
                    "Expected the SDO submission failure context.");
                AssertEx.True(ReferenceEquals(request, context.Request));
                AssertEx.Equal(
                    LMCSdoSubmissionPhase.Submission,
                    context.Phase);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.NotAttempted,
                    context.SubmissionOutcome);
                AssertEx.Equal(DiagnosticsBootId, context.DiagnosticsBootId);
                AssertEx.Equal(MapRevision, context.MapRevision);
                AssertEx.Equal<LMCOperationTicket>(null, context.Ticket);

                var disarmCount = 0;
                var preserveCount = 0;
                var quarantineCount = 0;
                string disarmState = null;
                string disarmDetail = null;
                D5ExternalReadFailureOrchestrator.RouteSubmissionFailure(
                    error,
                    (state, detail) =>
                    {
                        disarmCount++;
                        disarmState = state;
                        disarmDetail = detail;
                    },
                    (ticket, bootId, mapRevision) => preserveCount++,
                    (failure, failureContext) => quarantineCount++);
                AssertEx.Equal(1, disarmCount);
                AssertEx.Equal(0, preserveCount);
                AssertEx.Equal(0, quarantineCount);
                AssertEx.Equal("PRE_SUBMISSION_FAILURE", disarmState);
                AssertEx.Equal(
                    "Submission:LMCSendPreemptedException",
                    disarmDetail);
                AssertEx.True(connection.IsConnected);

                connection.CloseConnection();
                server.Verify();
                AssertCommands(server, 0x8080, 0x405C, 0x7E00, 0x405D);
            }
        }

        private static void DigitalOutputSubmissionPreemptedBeforeWire()
        {
            const string operation = "Digital output submission";
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EtherCATNodeHealth
                | LMCDiagnosticCapability.DigitalIORead
                | LMCDiagnosticCapability.DigitalIOWrite;
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };
            long reservedGeneration = 0;

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        TopologyIoCapabilitiesPayload(
                            1,
                            capabilities))),
                new FakeRpcStep(
                    0x7E22,
                    TestFrame.Response(0, DigitalOutputIoPayload(2))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        TopologyIoCapabilitiesPayload(
                            3,
                            capabilities)))
                {
                    InspectRequest = capabilityRequest =>
                    {
                        reservedGeneration = coordinator.ReservePrioritySend();
                    }
                },
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                Connect(connection, server.Port);
                var shadow = connection.Diagnostics.ReadDigitalIO(
                    CreateDigitalOutputTopology(connection),
                    new LMCDigitalIOReadRequest(
                        TopologyRevision,
                        IOReference,
                        LMCDigitalIODirection.Output,
                        64));
                AssertEx.True(shadow.BelongsToCurrentSession(connection));
                AssertEx.True(shadow.HasValidatedTopologyBinding);
                var request = connection.Diagnostics
                    .CreateDigitalOutputWriteRequest(shadow, 1, 1);
                AssertEx.True(request.BelongsToCurrentSession(connection));

                var expectedGeneration = coordinator.CurrentGeneration;
                LMCSendPreemptedException error;
                using (coordinator.BeginPreemptibleScope(
                    expectedGeneration,
                    operation))
                {
                    error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => InvokeDigitalOutputWriteWithPolicy(
                            connection,
                            request));
                }

                AssertPreempted(
                    error,
                    operation,
                    0x7E23,
                    expectedGeneration,
                    reservedGeneration);
                LMCDigitalOutputWriteSubmissionFailureContext context;
                AssertEx.True(
                    LMCDigitalOutputWriteSubmissionFailureContext.TryGet(
                        error,
                        out context),
                    "Expected the digital-output submission failure context.");
                AssertEx.True(ReferenceEquals(request, context.Request));
                AssertEx.Equal(
                    LMCDigitalOutputWriteSubmissionPhase.Submission,
                    context.Phase);
                AssertEx.Equal(
                    LMCDigitalOutputWriteSubmissionOutcome.NotAttempted,
                    context.SubmissionOutcome);
                AssertEx.Equal(DiagnosticsBootId, context.DiagnosticsBootId);
                AssertEx.Equal(
                    TopologyRevision,
                    context.TopologyRevision);
                AssertEx.Equal<LMCOperationTicket>(null, context.Ticket);
                AssertEx.True(connection.IsConnected);

                connection.CloseConnection();
                server.Verify();
                AssertCommands(
                    server,
                    0x8080,
                    0x405C,
                    0x7E00,
                    0x7E22,
                    0x7E00,
                    0x405D);
            }
        }

        private static void
            DigitalOutputSubmissionSyncDelayedAckDiscarded()
        {
            RunDigitalOutputSubmissionDelayedAckDiscarded(false);
        }

        private static void
            DigitalOutputSubmissionAsyncDelayedAckDiscarded()
        {
            RunDigitalOutputSubmissionDelayedAckDiscarded(true);
        }

        private static void RunDigitalOutputSubmissionDelayedAckDiscarded(
            bool useAsync)
        {
            const string priorityOperation =
                "Priority GroupStop during digital output submission";
            const uint ticketId = 0x01020304u;
            var normalOperation = useAsync
                ? "Delayed async digital output submission"
                : "Delayed sync digital output submission";
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EtherCATNodeHealth
                | LMCDiagnosticCapability.DigitalIORead
                | LMCDiagnosticCapability.DigitalIOWrite;
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var submitReceived = new ManualResetEventSlim(false))
            using (var releaseSubmit = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    LMC_CommandId.GetDiagnosticsCapabilities,
                    TestFrame.Response(
                        0,
                        TopologyIoCapabilitiesPayload(1, capabilities))),
                new FakeRpcStep(
                    LMC_CommandId.ReadDigitalIO,
                    TestFrame.Response(0, DigitalOutputIoPayload(2))),
                GroupLookupStep(),
                new FakeRpcStep(
                    LMC_CommandId.GetDiagnosticsCapabilities,
                    TestFrame.Response(
                        0,
                        TopologyIoCapabilitiesPayload(3, capabilities))),
                DelayedDigitalOutputSubmitStep(
                    4,
                    ticketId,
                    submitReceived,
                    releaseSubmit),
                GroupStopStep(),
                new FakeRpcStep(
                    LMC_CommandId.GetDiagnosticsCapabilities,
                    TestFrame.Response(
                        0,
                        TopologyIoCapabilitiesPayload(5, capabilities))),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var shadow = connection.Diagnostics.ReadDigitalIO(
                        CreateDigitalOutputTopology(connection),
                        new LMCDigitalIOReadRequest(
                            TopologyRevision,
                            IOReference,
                            LMCDigitalIODirection.Output,
                            64));
                    var request = connection.Diagnostics
                        .CreateDigitalOutputWriteRequest(shadow, 1, 1);
                    var group = new LMCGroup(
                        connection,
                        "_LMCRobotBase1");
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var delayedSubmission = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                return useAsync
                                    ? InvokeDigitalOutputWriteWithPolicy(
                                        connection,
                                        request)
                                    : InvokeDigitalOutputWriteWithPolicySync(
                                        connection,
                                        request);
                            }
                        });

                    AssertEx.True(
                        submitReceived.Wait(2000),
                        "The delayed digital-output submission did not reach the server.");
                    var reservedGeneration = coordinator.ReservePrioritySend();
                    var priorityStop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                priorityOperation))
                            {
                                return group.GroupStop(1000, 0);
                            }
                        });

                    releaseSubmit.Set();
                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => delayedSubmission.GetAwaiter().GetResult());
                    var stopResponse = priorityStop.GetAwaiter().GetResult();

                    AssertResultDiscarded(
                        error,
                        normalOperation,
                        LMC_CommandId.SubmitDigitalOutputWrite,
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.True(stopResponse.IsSuccess);

                    LMCDigitalOutputWriteSubmissionFailureContext context;
                    AssertEx.True(
                        LMCDigitalOutputWriteSubmissionFailureContext.TryGet(
                            error,
                            out context),
                        "Expected the accepted digital-output failure context.");
                    AssertEx.True(ReferenceEquals(request, context.Request));
                    AssertEx.Equal(
                        LMCDigitalOutputWriteSubmissionPhase
                            .PostSubmissionValidation,
                        context.Phase);
                    AssertEx.Equal(
                        LMCDigitalOutputWriteSubmissionOutcome.Accepted,
                        context.SubmissionOutcome);
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        context.DiagnosticsBootId);
                    AssertEx.Equal(
                        TopologyRevision,
                        context.TopologyRevision);
                    AssertEx.NotNull(context.Ticket);
                    AssertEx.Equal(ticketId, context.Ticket.TicketId);
                    AssertEx.Equal(
                        LMCOperationKind.DigitalOutputWrite,
                        context.Ticket.OperationKind);
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        context.Ticket.DiagnosticsBootId);
                    AssertEx.Equal(
                        TopologyRevision,
                        context.Ticket.SubmissionTopologyRevision);

                    var refreshed = connection.Diagnostics.GetCapabilities();
                    AssertEx.Equal(DiagnosticsBootId, refreshed.DiagnosticsBootId);
                    AssertEx.True(connection.IsConnected);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        LMC_CommandId.RpcSessionInit,
                        LMC_CommandId.RpcCallbackRegistration,
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        LMC_CommandId.ReadDigitalIO,
                        LMC_CommandId.GetGroupByName,
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        LMC_CommandId.SubmitDigitalOutputWrite,
                        LMC_CommandId.GroupStop,
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        LMC_CommandId.CloseConnection);
                }
                finally
                {
                    releaseSubmit.Set();
                }
            }
        }

        private static void EtherCATNodeHealthSyncDelayedAckDiscarded()
        {
            RunTopologyIoReadDelayedAckDiscarded(true, false);
        }

        private static void EtherCATNodeHealthAsyncDelayedAckDiscarded()
        {
            RunTopologyIoReadDelayedAckDiscarded(true, true);
        }

        private static void DigitalIOReadSyncDelayedAckDiscarded()
        {
            RunTopologyIoReadDelayedAckDiscarded(false, false);
        }

        private static void DigitalIOReadAsyncDelayedAckDiscarded()
        {
            RunTopologyIoReadDelayedAckDiscarded(false, true);
        }

        private static void RunTopologyIoReadDelayedAckDiscarded(
            bool nodeHealth,
            bool useAsync)
        {
            const string priorityOperation =
                "Priority GroupStop during topology I/O read";
            var normalOperation = "Delayed "
                + (useAsync ? "async " : "sync ")
                + (nodeHealth
                    ? "EtherCAT node-health read"
                    : "digital-I/O read");
            var command = nodeHealth
                ? LMC_CommandId.ReadEtherCATNodeHealth
                : LMC_CommandId.ReadDigitalIO;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology
                | (nodeHealth
                    ? LMCDiagnosticCapability.EtherCATNodeHealth
                    : LMCDiagnosticCapability.DigitalIORead);
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var readReceived = new ManualResetEventSlim(false))
            using (var releaseRead = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                GroupLookupStep(),
                new FakeRpcStep(
                    LMC_CommandId.GetDiagnosticsCapabilities,
                    TestFrame.Response(
                        0,
                        TopologyIoCapabilitiesPayload(1, capabilities))),
                DelayedTopologyIoReadStep(
                    command,
                    2,
                    readReceived,
                    releaseRead),
                GroupStopStep(),
                new FakeRpcStep(
                    LMC_CommandId.GetDiagnosticsCapabilities,
                    TestFrame.Response(
                        0,
                        TopologyIoCapabilitiesPayload(3, capabilities))),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var group = new LMCGroup(
                        connection,
                        "_LMCRobotBase1");
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var delayedRead = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                if (nodeHealth)
                                {
                                    if (useAsync)
                                    {
                                        connection.Diagnostics
                                            .ReadEtherCATNodeHealthAsync(
                                                TopologyRevision,
                                                TopologyNodeId,
                                                CancellationToken.None)
                                            .GetAwaiter()
                                            .GetResult();
                                    }
                                    else
                                    {
                                        connection.Diagnostics
                                            .ReadEtherCATNodeHealth(
                                                TopologyRevision,
                                                TopologyNodeId);
                                    }
                                }
                                else
                                {
                                    var request = new LMCDigitalIOReadRequest(
                                        TopologyRevision,
                                        IOReference,
                                        LMCDigitalIODirection.Output,
                                        64);
                                    if (useAsync)
                                    {
                                        connection.Diagnostics
                                            .ReadDigitalIOAsync(
                                                request,
                                                CancellationToken.None)
                                            .GetAwaiter()
                                            .GetResult();
                                    }
                                    else
                                    {
                                        connection.Diagnostics.ReadDigitalIO(
                                            request);
                                    }
                                }
                            }
                        });

                    AssertEx.True(
                        readReceived.Wait(2000),
                        "The delayed topology I/O read did not reach the server.");
                    var reservedGeneration = coordinator.ReservePrioritySend();
                    var priorityStop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                priorityOperation))
                            {
                                return group.GroupStop(1000, 0);
                            }
                        });

                    releaseRead.Set();
                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => delayedRead.GetAwaiter().GetResult());
                    var stopResponse = priorityStop.GetAwaiter().GetResult();

                    AssertResultDiscarded(
                        error,
                        normalOperation,
                        command,
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.True(stopResponse.IsSuccess);
                    var refreshed = connection.Diagnostics.GetCapabilities();
                    AssertEx.Equal(DiagnosticsBootId, refreshed.DiagnosticsBootId);
                    AssertEx.True(connection.IsConnected);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        LMC_CommandId.RpcSessionInit,
                        LMC_CommandId.RpcCallbackRegistration,
                        LMC_CommandId.GetGroupByName,
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        command,
                        LMC_CommandId.GroupStop,
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        LMC_CommandId.CloseConnection);
                }
                finally
                {
                    releaseRead.Set();
                }
            }
        }

        private static void ResultPublicationIsAtomicWithReservation()
        {
            const string operation = "Atomic result publication";
            const ushort command = 0x2045;
            using (var publicationEntered = new ManualResetEventSlim(false))
            using (var releasePublication = new ManualResetEventSlim(false))
            using (var reservationStarted = new ManualResetEventSlim(false))
            {
                var coordinator = new LMCSendPriorityCoordinator();
                var expectedGeneration = coordinator.CurrentGeneration;
                var publishCount = 0;

                using (coordinator.BeginPreemptibleScope(
                    expectedGeneration,
                    operation))
                {
                    var publication = Task.Run(
                        () => coordinator.PublishResult(
                            command,
                            () =>
                            {
                                publicationEntered.Set();
                                AssertEx.True(
                                    releasePublication.Wait(5000),
                                    "The result publication was not released.");
                                Interlocked.Increment(ref publishCount);
                            }));
                    AssertEx.True(
                        publicationEntered.Wait(2000),
                        "The result publication did not enter its commit boundary.");

                    var reservation = Task.Run(
                        () =>
                        {
                            reservationStarted.Set();
                            return coordinator.ReservePrioritySend();
                        });
                    AssertEx.True(
                        reservationStarted.Wait(2000),
                        "The priority reservation task did not start.");

                    try
                    {
                        AssertEx.False(
                            reservation.Wait(100),
                            "Priority reservation interleaved with result publication.");
                    }
                    finally
                    {
                        releasePublication.Set();
                    }

                    publication.GetAwaiter().GetResult();
                    var actualGeneration = reservation.GetAwaiter().GetResult();
                    AssertEx.Equal(expectedGeneration + 1, actualGeneration);
                    AssertEx.Equal(1, publishCount);
                }
            }
        }

        private static void GroupGenericSyncDelayedAckDiscarded()
        {
            const string normalOperation = "Delayed sync GroupReset";
            const string priorityOperation = "Priority sync GroupStop";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var resetReceived = new ManualResetEventSlim(false))
            using (var releaseReset = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                GroupLookupStep(),
                DelayedGroupResetStep(resetReceived, releaseReset),
                GroupStopStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var group = new LMCGroup(connection, "_LMCRobotBase1");
                    var mutationCoordinator = connection
                        .GetGroupEnableWaitCoordinator(
                            group.SessionGeneration,
                            group.GroupReference);
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var delayedReset = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                return group.GroupReset();
                            }
                        });

                    AssertEx.True(
                        resetReceived.Wait(2000),
                        "The delayed sync GroupReset did not reach the server.");
                    AssertEx.Equal(1L, mutationCoordinator.MutationGeneration);

                    var reservedGeneration = coordinator.ReservePrioritySend();
                    var priorityStop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                priorityOperation))
                            {
                                return group.GroupStop(1000, 0);
                            }
                        });

                    releaseReset.Set();
                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => delayedReset.GetAwaiter().GetResult());
                    var stopResponse = priorityStop.GetAwaiter().GetResult();

                    AssertResultDiscarded(
                        error,
                        normalOperation,
                        LMC_CommandId.GroupReset,
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.Equal(2L, mutationCoordinator.MutationGeneration);
                    AssertEx.True(connection.IsConnected);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        0x8080,
                        0x405C,
                        0x1042,
                        LMC_CommandId.GroupReset,
                        LMC_CommandId.GroupStop,
                        0x405D);
                }
                finally
                {
                    releaseReset.Set();
                }
            }
        }

        private static void GroupGenericAsyncDelayedAckDiscarded()
        {
            const string normalOperation = "Delayed async GroupReset";
            const string priorityOperation = "Priority async GroupStop";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var resetReceived = new ManualResetEventSlim(false))
            using (var releaseReset = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                GroupLookupStep(),
                DelayedGroupResetStep(resetReceived, releaseReset),
                GroupStopStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var group = new LMCGroup(connection, "_LMCRobotBase1");
                    var mutationCoordinator = connection
                        .GetGroupEnableWaitCoordinator(
                            group.SessionGeneration,
                            group.GroupReference);
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var delayedReset = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                return group.GroupResetAsync(
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    AssertEx.True(
                        resetReceived.Wait(2000),
                        "The delayed async GroupReset did not reach the server.");
                    AssertEx.Equal(1L, mutationCoordinator.MutationGeneration);

                    var reservedGeneration = coordinator.ReservePrioritySend();
                    var priorityStop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                priorityOperation))
                            {
                                return group.GroupStopAsync(
                                        1000,
                                        0,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    releaseReset.Set();
                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => delayedReset.GetAwaiter().GetResult());
                    var stopResponse = priorityStop.GetAwaiter().GetResult();

                    AssertResultDiscarded(
                        error,
                        normalOperation,
                        LMC_CommandId.GroupReset,
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.Equal(2L, mutationCoordinator.MutationGeneration);
                    AssertEx.True(connection.IsConnected);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        0x8080,
                        0x405C,
                        0x1042,
                        LMC_CommandId.GroupReset,
                        LMC_CommandId.GroupStop,
                        0x405D);
                }
                finally
                {
                    releaseReset.Set();
                }
            }
        }

        private static void AxisGenericSyncDelayedAckDiscarded()
        {
            const string normalOperation = "Delayed sync Axis Reset";
            const string priorityOperation = "Priority sync Axis Stop";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var resetReceived = new ManualResetEventSlim(false))
            using (var releaseReset = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                DelayedAxisResetStep(resetReceived, releaseReset),
                AxisStopStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var axis = new LMCAxis(connection, "_LMCAxis1");
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var delayedReset = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                return axis.Reset();
                            }
                        });

                    AssertEx.True(
                        resetReceived.Wait(2000),
                        "The delayed sync Axis Reset did not reach the server.");

                    var reservedGeneration = coordinator.ReservePrioritySend();
                    var priorityStop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                priorityOperation))
                            {
                                return axis.Stop(1000, 0);
                            }
                        });

                    releaseReset.Set();
                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => delayedReset.GetAwaiter().GetResult());
                    var stopResponse = priorityStop.GetAwaiter().GetResult();

                    AssertResultDiscarded(
                        error,
                        normalOperation,
                        LMC_CommandId.Reset,
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.True(connection.IsConnected);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        LMC_CommandId.RpcSessionInit,
                        LMC_CommandId.RpcCallbackRegistration,
                        LMC_CommandId.GetAxisByName,
                        LMC_CommandId.AxisInfo,
                        LMC_CommandId.Reset,
                        LMC_CommandId.Stop,
                        LMC_CommandId.CloseConnection);
                }
                finally
                {
                    releaseReset.Set();
                }
            }
        }

        private static void AxisGenericAsyncDelayedAckDiscarded()
        {
            const string normalOperation = "Delayed async Axis Reset";
            const string priorityOperation = "Priority async Axis Stop";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var resetReceived = new ManualResetEventSlim(false))
            using (var releaseReset = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                DelayedAxisResetStep(resetReceived, releaseReset),
                AxisStopStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var axis = new LMCAxis(connection, "_LMCAxis1");
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var delayedReset = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                return axis.ResetAsync(CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    AssertEx.True(
                        resetReceived.Wait(2000),
                        "The delayed async Axis Reset did not reach the server.");

                    var reservedGeneration = coordinator.ReservePrioritySend();
                    var priorityStop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                priorityOperation))
                            {
                                return axis.StopAsync(
                                        1000,
                                        0,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    releaseReset.Set();
                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => delayedReset.GetAwaiter().GetResult());
                    var stopResponse = priorityStop.GetAwaiter().GetResult();

                    AssertResultDiscarded(
                        error,
                        normalOperation,
                        LMC_CommandId.Reset,
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.True(connection.IsConnected);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        LMC_CommandId.RpcSessionInit,
                        LMC_CommandId.RpcCallbackRegistration,
                        LMC_CommandId.GetAxisByName,
                        LMC_CommandId.AxisInfo,
                        LMC_CommandId.Reset,
                        LMC_CommandId.Stop,
                        LMC_CommandId.CloseConnection);
                }
                finally
                {
                    releaseReset.Set();
                }
            }
        }

        private static void AdminGroupMoveSyncDelayedAckDiscarded()
        {
            const string normalOperation = "Delayed sync Admin GroupMoveLinearRelative";
            const string priorityOperation = "Priority sync GroupStop";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var moveReceived = new ManualResetEventSlim(false))
            using (var releaseMove = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                GroupLookupStep(),
                AdminCapabilitiesStep(),
                DelayedAdminGroupMoveStep(moveReceived, releaseMove),
                GroupStopStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var group = new LMCGroup(connection, "_LMCRobotBase1");
                    var capabilities = connection.Admin.GetCapabilities();
                    var mutationCoordinator = connection
                        .GetGroupEnableWaitCoordinator(
                            group.SessionGeneration,
                            group.GroupReference);
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var delayedMove = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                return connection.Admin.GroupMoveLinearRelative(
                                    group,
                                    new[] { 10, -20, 30, -40 },
                                    100,
                                    200,
                                    300,
                                    0,
                                    new LMCGroupMotionOptions(),
                                    capabilities);
                            }
                        });

                    AssertEx.True(
                        moveReceived.Wait(2000),
                        "The delayed sync Admin group move did not reach the server.");
                    AssertEx.Equal(1L, mutationCoordinator.MutationGeneration);

                    var reservedGeneration = coordinator.ReservePrioritySend();
                    var priorityStop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                priorityOperation))
                            {
                                return group.GroupStop(1000, 0);
                            }
                        });

                    releaseMove.Set();
                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => delayedMove.GetAwaiter().GetResult());
                    var stopResponse = priorityStop.GetAwaiter().GetResult();

                    AssertResultDiscarded(
                        error,
                        normalOperation,
                        LMC_CommandId.GroupMoveLinearRelative,
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.Equal(2L, mutationCoordinator.MutationGeneration);
                    AssertEx.True(connection.IsConnected);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        LMC_CommandId.RpcSessionInit,
                        LMC_CommandId.RpcCallbackRegistration,
                        LMC_CommandId.GetGroupByName,
                        LMC_CommandId.GetAdminCapabilities,
                        LMC_CommandId.GroupMoveLinearRelative,
                        LMC_CommandId.GroupStop,
                        LMC_CommandId.CloseConnection);
                }
                finally
                {
                    releaseMove.Set();
                }
            }
        }

        private static void AdminGroupMoveAsyncDelayedAckDiscarded()
        {
            const string normalOperation = "Delayed async Admin GroupMoveLinearRelative";
            const string priorityOperation = "Priority async GroupStop";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var moveReceived = new ManualResetEventSlim(false))
            using (var releaseMove = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                GroupLookupStep(),
                AdminCapabilitiesStep(),
                DelayedAdminGroupMoveStep(moveReceived, releaseMove),
                GroupStopStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var group = new LMCGroup(connection, "_LMCRobotBase1");
                    var capabilities = connection.Admin.GetCapabilities();
                    var mutationCoordinator = connection
                        .GetGroupEnableWaitCoordinator(
                            group.SessionGeneration,
                            group.GroupReference);
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var delayedMove = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                return connection.Admin
                                    .GroupMoveLinearRelativeAsync(
                                        group,
                                        new[] { 10, -20, 30, -40 },
                                        100,
                                        200,
                                        300,
                                        0,
                                        new LMCGroupMotionOptions(),
                                        capabilities,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    AssertEx.True(
                        moveReceived.Wait(2000),
                        "The delayed async Admin group move did not reach the server.");
                    AssertEx.Equal(1L, mutationCoordinator.MutationGeneration);

                    var reservedGeneration = coordinator.ReservePrioritySend();
                    var priorityStop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                priorityOperation))
                            {
                                return group.GroupStopAsync(
                                        1000,
                                        0,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    releaseMove.Set();
                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => delayedMove.GetAwaiter().GetResult());
                    var stopResponse = priorityStop.GetAwaiter().GetResult();

                    AssertResultDiscarded(
                        error,
                        normalOperation,
                        LMC_CommandId.GroupMoveLinearRelative,
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.Equal(2L, mutationCoordinator.MutationGeneration);
                    AssertEx.True(connection.IsConnected);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        LMC_CommandId.RpcSessionInit,
                        LMC_CommandId.RpcCallbackRegistration,
                        LMC_CommandId.GetGroupByName,
                        LMC_CommandId.GetAdminCapabilities,
                        LMC_CommandId.GroupMoveLinearRelative,
                        LMC_CommandId.GroupStop,
                        LMC_CommandId.CloseConnection);
                }
                finally
                {
                    releaseMove.Set();
                }
            }
        }

        private static void GroupEnableWaitDelayedAckDiscarded()
        {
            const string normalOperation = "Delayed GroupEnable wait ACK";
            const string priorityOperation = "Priority GroupStop after GroupEnable";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var enableReceived = new ManualResetEventSlim(false))
            using (var releaseEnable = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                GroupLookupStep(),
                DelayedGroupEnableStep(enableReceived, releaseEnable),
                GroupStopStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var group = new LMCGroup(connection, "_LMCRobotBase1");
                    var mutationCoordinator = connection
                        .GetGroupEnableWaitCoordinator(
                            group.SessionGeneration,
                            group.GroupReference);
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var delayedEnable = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                return group
                                    .GroupEnableAndWaitForLockedStandbyAsync(
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    AssertEx.True(
                        enableReceived.Wait(2000),
                        "The delayed GroupEnable wait ACK did not reach the server.");
                    AssertEx.Equal(1L, mutationCoordinator.MutationGeneration);

                    var reservedGeneration = coordinator.ReservePrioritySend();
                    var priorityStop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                priorityOperation))
                            {
                                return group.GroupStop(1000, 0);
                            }
                        });

                    releaseEnable.Set();
                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => delayedEnable.GetAwaiter().GetResult());
                    var stopResponse = priorityStop.GetAwaiter().GetResult();

                    AssertResultDiscarded(
                        error,
                        normalOperation,
                        LMC_CommandId.GroupProfileLock,
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.Equal(2L, mutationCoordinator.MutationGeneration);
                    AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                        null,
                        group.PendingGroupEnableWaitContinuation);
                    AssertEx.True(connection.IsConnected);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        LMC_CommandId.RpcSessionInit,
                        LMC_CommandId.RpcCallbackRegistration,
                        LMC_CommandId.GetGroupByName,
                        LMC_CommandId.GroupProfileLock,
                        LMC_CommandId.GroupStop,
                        LMC_CommandId.CloseConnection);
                }
                finally
                {
                    releaseEnable.Set();
                }
            }
        }

        private static void SubmitSdoSyncDelayedAckDiscarded()
        {
            RunSubmitSdoDelayedAckDiscarded(false);
        }

        private static void SubmitSdoAsyncDelayedAckDiscarded()
        {
            RunSubmitSdoDelayedAckDiscarded(true);
        }

        private static void RunSubmitSdoDelayedAckDiscarded(bool useAsync)
        {
            const uint ticketId = 0x51525354u;
            var normalOperation = useAsync
                ? "Delayed async SubmitSdo ACK"
                : "Delayed sync SubmitSdo ACK";
            var priorityOperation = useAsync
                ? "Priority GroupStop after async SubmitSdo"
                : "Priority GroupStop after sync SubmitSdo";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };
            var request = LMCSdoRequest.CreateRead(
                1,
                0x1018,
                1,
                LMCSignalValueType.UInt32,
                4,
                100);

            using (var submitReceived = new ManualResetEventSlim(false))
            using (var releaseSubmit = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                GroupLookupStep(),
                new FakeRpcStep(
                    LMC_CommandId.GetDiagnosticsCapabilities,
                    TestFrame.Response(0, SdoCapabilitiesPayload(1))),
                DelayedSdoSubmitStep(
                    2,
                    ticketId,
                    submitReceived,
                    releaseSubmit),
                GroupStopStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var group = new LMCGroup(connection, "_LMCRobotBase1");
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var delayedSubmit = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                return useAsync
                                    ? connection.Diagnostics.SubmitSdoAsync(
                                            request,
                                            CancellationToken.None)
                                        .GetAwaiter()
                                        .GetResult()
                                    : connection.Diagnostics.SubmitSdo(request);
                            }
                        });

                    AssertEx.True(
                        submitReceived.Wait(2000),
                        "The delayed SubmitSdo ACK did not reach the server.");
                    var reservedGeneration = coordinator.ReservePrioritySend();
                    var priorityStop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                priorityOperation))
                            {
                                return group.GroupStop(1000, 0);
                            }
                        });

                    releaseSubmit.Set();
                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => delayedSubmit.GetAwaiter().GetResult());
                    var stopResponse = priorityStop.GetAwaiter().GetResult();

                    AssertResultDiscarded(
                        error,
                        normalOperation,
                        LMC_CommandId.SubmitSdo,
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.True(connection.IsConnected);

                    LMCSdoSubmissionFailureContext context;
                    AssertEx.True(
                        LMCSdoSubmissionFailureContext.TryGet(
                            error,
                            out context),
                        "Expected accepted SubmitSdo failure evidence.");
                    AssertEx.True(ReferenceEquals(request, context.Request));
                    AssertEx.Equal(
                        LMCSdoSubmissionPhase.PostSubmissionValidation,
                        context.Phase);
                    AssertEx.Equal(
                        LMCSdoSubmissionOutcome.Accepted,
                        context.SubmissionOutcome);
                    AssertEx.NotNull(context.Ticket);
                    AssertEx.Equal(ticketId, context.Ticket.TicketId);
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        context.DiagnosticsBootId);
                    AssertEx.Equal(MapRevision, context.MapRevision);
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        context.Ticket.DiagnosticsBootId);
                    AssertEx.Equal(
                        MapRevision,
                        context.Ticket.SubmissionMapRevision);
                    AssertEx.True(
                        ReferenceEquals(
                            request,
                            context.Ticket.SubmittedSdoRequest));

                    var disarmCount = 0;
                    var preserveCount = 0;
                    var quarantineCount = 0;
                    string disarmState = null;
                    string disarmDetail = null;
                    LMCOperationTicket preservedTicket = null;
                    D5ExternalReadFailureOrchestrator.RouteSubmissionFailure(
                        error,
                        (state, detail) =>
                        {
                            disarmCount++;
                            disarmState = state;
                            disarmDetail = detail;
                        },
                        (ticket, bootId, mapRevision) =>
                        {
                            preserveCount++;
                            preservedTicket = ticket;
                            AssertEx.Equal(DiagnosticsBootId, bootId);
                            AssertEx.Equal(MapRevision, mapRevision);
                        },
                        (failure, failureContext) => quarantineCount++);
                    AssertEx.Equal(1, disarmCount);
                    AssertEx.Equal(1, preserveCount);
                    AssertEx.Equal(0, quarantineCount);
                    AssertEx.Equal("KNOWN_TICKET_PRESERVED", disarmState);
                    AssertEx.Equal(
                        "post_submission_validation:LMCSendPreemptedException",
                        disarmDetail);
                    AssertEx.True(
                        ReferenceEquals(context.Ticket, preservedTicket));

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        LMC_CommandId.RpcSessionInit,
                        LMC_CommandId.RpcCallbackRegistration,
                        LMC_CommandId.GetGroupByName,
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        LMC_CommandId.SubmitSdo,
                        LMC_CommandId.GroupStop,
                        LMC_CommandId.CloseConnection);
                }
                finally
                {
                    releaseSubmit.Set();
                }
            }
        }

        private static void CancelOperationSyncDelayedAckDiscarded()
        {
            RunCancelOperationDelayedAckDiscarded(false);
        }

        private static void CancelOperationAsyncDelayedAckDiscarded()
        {
            RunCancelOperationDelayedAckDiscarded(true);
        }

        private static void RunCancelOperationDelayedAckDiscarded(bool useAsync)
        {
            const uint ticketId = 0x61626364u;
            var normalOperation = useAsync
                ? "Delayed async CancelOperation ACK"
                : "Delayed sync CancelOperation ACK";
            var priorityOperation = useAsync
                ? "Priority GroupStop after async CancelOperation"
                : "Priority GroupStop after sync CancelOperation";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };
            var request = LMCSdoRequest.CreateRead(
                1,
                0x1018,
                1,
                LMCSignalValueType.UInt32,
                4,
                100);

            using (var cancelReceived = new ManualResetEventSlim(false))
            using (var releaseCancel = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                GroupLookupStep(),
                new FakeRpcStep(
                    LMC_CommandId.GetDiagnosticsCapabilities,
                    TestFrame.Response(0, SdoCapabilitiesPayload(1))),
                new FakeRpcStep(
                    LMC_CommandId.SubmitSdo,
                    TestFrame.Response(
                        0,
                        SdoSubmitPayload(2, ticketId))),
                DelayedCancelOperationStep(
                    3,
                    ticketId,
                    cancelReceived,
                    releaseCancel),
                GroupStopStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var group = new LMCGroup(connection, "_LMCRobotBase1");
                    var ticket = connection.Diagnostics.SubmitSdo(request);
                    AssertEx.Equal(ticketId, ticket.TicketId);

                    var expectedGeneration = coordinator.CurrentGeneration;
                    var delayedCancel = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                if (useAsync)
                                {
                                    connection.Diagnostics.CancelOperationAsync(
                                            ticket,
                                            CancellationToken.None)
                                        .GetAwaiter()
                                        .GetResult();
                                }
                                else
                                {
                                    connection.Diagnostics.CancelOperation(ticket);
                                }
                            }
                        });

                    AssertEx.True(
                        cancelReceived.Wait(2000),
                        "The delayed CancelOperation ACK did not reach the server.");
                    var reservedGeneration = coordinator.ReservePrioritySend();
                    var priorityStop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                priorityOperation))
                            {
                                return group.GroupStop(1000, 0);
                            }
                        });

                    releaseCancel.Set();
                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => delayedCancel.GetAwaiter().GetResult());
                    var stopResponse = priorityStop.GetAwaiter().GetResult();

                    AssertResultDiscarded(
                        error,
                        normalOperation,
                        LMC_CommandId.CancelOperation,
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.True(connection.IsConnected);
                    LMCSdoSubmissionFailureContext context;
                    AssertEx.False(
                        LMCSdoSubmissionFailureContext.TryGet(
                            error,
                            out context));

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        LMC_CommandId.RpcSessionInit,
                        LMC_CommandId.RpcCallbackRegistration,
                        LMC_CommandId.GetGroupByName,
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        LMC_CommandId.SubmitSdo,
                        LMC_CommandId.CancelOperation,
                        LMC_CommandId.GroupStop,
                        LMC_CommandId.CloseConnection);
                }
                finally
                {
                    releaseCancel.Set();
                }
            }
        }

        private static void RunRecorderAcceptedResultDelayedAck(
            RecorderAcceptedPath path,
            bool useAsync)
        {
            var command = RecorderAcceptedCommand(path);
            var requestId = path == RecorderAcceptedPath.Start ? 3u : 2u;
            var pathName = RecorderAcceptedPathName(path);
            var normalOperation = "Delayed "
                + (useAsync ? "async " : "sync ")
                + pathName
                + " result";
            var priorityOperation = "Priority GroupStop after "
                + (useAsync ? "async " : "sync ")
                + pathName;
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };
            var inventory = EmptyClosedRecorderBankInventory(99);

            using (var resultReceived = new ManualResetEventSlim(false))
            using (var releaseResult = new ManualResetEventSlim(false))
            {
                var steps = new List<FakeRpcStep>
                {
                    InitStep(),
                    CallbackStep(),
                    GroupLookupStep(),
                    new FakeRpcStep(
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        TestFrame.Response(
                            0,
                            path == RecorderAcceptedPath.AdoptActive
                                ? RecorderSingleBankCapabilitiesPayload(1)
                                : RecorderCapabilitiesPayload(1)))
                };

                if (path == RecorderAcceptedPath.Start)
                {
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.ConfigureRecorder,
                            TestFrame.Response(
                                0,
                                RecorderConfigurePayload(2))));
                }

                steps.Add(
                    DelayedRecorderAcceptedResultStep(
                        path,
                        command,
                        requestId,
                        RecorderAcceptedResultPayload(path, requestId),
                        resultReceived,
                        releaseResult));
                steps.Add(GroupStopStep());
                AddRecorderAcceptedCleanupSteps(steps, path, requestId);
                steps.Add(CloseStep());

                using (var server = new FakeRpcServer(steps.ToArray()))
                using (var connection = new LMCConnection(options))
                {
                    try
                    {
                        Connect(connection, server.Port);
                        var group = new LMCGroup(
                            connection,
                            "_LMCRobotBase1");
                        LMCRecorderConfigurationHandle sourceHandle = null;
                        if (path == RecorderAcceptedPath.Start)
                        {
                            sourceHandle = connection.Diagnostics
                                .ConfigureRecorder(
                                    RecorderManualConfiguration());
                        }

                        var expectedGeneration = coordinator.CurrentGeneration;
                        var delayedResult = Task.Run(
                            () =>
                            {
                                using (coordinator.BeginPreemptibleScope(
                                    expectedGeneration,
                                    normalOperation))
                                {
                                    InvokeRecorderAcceptedOperation(
                                        connection,
                                        path,
                                        useAsync,
                                        inventory,
                                        sourceHandle);
                                }
                            });

                        AssertEx.True(
                            resultReceived.Wait(2000),
                            "The delayed Recorder accepted result did not reach the server.");
                        var reservedGeneration =
                            coordinator.ReservePrioritySend();
                        var priorityStop = Task.Run(
                            () =>
                            {
                                using (coordinator.BeginPriorityScope(
                                    reservedGeneration,
                                    priorityOperation))
                                {
                                    return group.GroupStop(1000, 0);
                                }
                            });

                        releaseResult.Set();
                        var error = AssertEx.Throws<LMCSendPreemptedException>(
                            () => delayedResult.GetAwaiter().GetResult());
                        var stopResponse = priorityStop.GetAwaiter().GetResult();

                        AssertResultDiscarded(
                            error,
                            normalOperation,
                            command,
                            expectedGeneration,
                            reservedGeneration);
                        AssertEx.True(stopResponse.IsSuccess);
                        AssertEx.True(connection.IsConnected);

                        LMCRecorderAcceptedResultFailureContext context;
                        AssertEx.True(
                            LMCRecorderAcceptedResultFailureContext.TryGet(
                                error,
                                out context));
                        AssertRecorderAcceptedResultContext(
                            path,
                            command,
                            context,
                            sourceHandle);
                        AssertRecorderAcceptedNormalUseBlocked(
                            connection,
                            path,
                            context,
                            sourceHandle);
                        CleanupRecorderAcceptedResult(
                            connection,
                            path,
                            context,
                            sourceHandle);

                        connection.CloseConnection();
                        server.Verify();
                        AssertCommands(
                            server,
                            RecorderAcceptedExpectedCommands(path));
                    }
                    finally
                    {
                        releaseResult.Set();
                    }
                }
            }
        }

        private static void InvokeRecorderAcceptedOperation(
            LMCConnection connection,
            RecorderAcceptedPath path,
            bool useAsync,
            LMCRecorderBankInventory inventory,
            LMCRecorderConfigurationHandle sourceHandle)
        {
            switch (path)
            {
                case RecorderAcceptedPath.Configure:
                    if (useAsync)
                    {
                        connection.Diagnostics.ConfigureRecorderAsync(
                                RecorderManualConfiguration(),
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics.ConfigureRecorder(
                            RecorderManualConfiguration());
                    }

                    return;

                case RecorderAcceptedPath.ConfigureRecoverableDouble:
                    if (useAsync)
                    {
                        connection.Diagnostics
                            .ConfigureRecoverableDoubleRecorderAsync(
                                RecorderRecoverableDoubleConfiguration(),
                                RecorderRecoveryToken,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics
                            .ConfigureRecoverableDoubleRecorder(
                                RecorderRecoverableDoubleConfiguration(),
                                RecorderRecoveryToken);
                    }

                    return;

                case RecorderAcceptedPath.Start:
                    if (useAsync)
                    {
                        connection.Diagnostics.StartRecorderAsync(
                                sourceHandle,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics.StartRecorder(sourceHandle);
                    }

                    return;

                case RecorderAcceptedPath.AdoptExact:
                    if (useAsync)
                    {
                        connection.Diagnostics.AdoptRecorderAsync(
                                DiagnosticsBootId,
                                RecorderRecordId,
                                0,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics.AdoptRecorder(
                            DiagnosticsBootId,
                            RecorderRecordId,
                            0);
                    }

                    return;

                case RecorderAcceptedPath.AdoptActive:
                    if (useAsync)
                    {
                        connection.Diagnostics.AdoptActiveRecorderAsync(
                                DiagnosticsBootId,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics.AdoptActiveRecorder(
                            DiagnosticsBootId);
                    }

                    return;

                case RecorderAcceptedPath.AdoptEmpty:
                    if (useAsync)
                    {
                        connection.Diagnostics
                            .AdoptEmptyRecorderConfigurationAsync(
                                inventory,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics
                            .AdoptEmptyRecorderConfiguration(inventory);
                    }

                    return;

                default:
                    throw new ArgumentOutOfRangeException("path");
            }
        }

        private static void AssertRecorderAcceptedNormalUseBlocked(
            LMCConnection connection,
            RecorderAcceptedPath path,
            LMCRecorderAcceptedResultFailureContext context,
            LMCRecorderConfigurationHandle sourceHandle)
        {
            if (path == RecorderAcceptedPath.Configure
                || path
                    == RecorderAcceptedPath.ConfigureRecoverableDouble)
            {
                var error = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.StartRecorder(
                        context.ConfigurationHandle));
                AssertEx.Contains("recovery-only", error.Message);
                return;
            }

            if (path == RecorderAcceptedPath.Start)
            {
                var identityError = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.GetRecorderHeader(
                        context.Identity));
                AssertEx.Contains("recovery-only", identityError.Message);
                var sourceError = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.StartRecorder(sourceHandle));
                AssertEx.Contains("recovery-only", sourceError.Message);
                return;
            }

            if (path == RecorderAcceptedPath.AdoptExact
                || path == RecorderAcceptedPath.AdoptActive)
            {
                var error = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.GetRecorderHeader(
                        context.Identity));
                AssertEx.Contains("recovery-only", error.Message);
            }
        }

        private static void CleanupRecorderAcceptedResult(
            LMCConnection connection,
            RecorderAcceptedPath path,
            LMCRecorderAcceptedResultFailureContext context,
            LMCRecorderConfigurationHandle sourceHandle)
        {
            if (path == RecorderAcceptedPath.Configure
                || path
                    == RecorderAcceptedPath.ConfigureRecoverableDouble)
            {
                connection.Diagnostics.ReleaseRecorder(
                    context.ConfigurationHandle);
                AssertEx.True(context.ConfigurationHandle.IsReleased);
                return;
            }

            if (path == RecorderAcceptedPath.Start)
            {
                connection.Diagnostics.StopRecorder(context.Identity);
                var status = connection.Diagnostics.GetRecorderStatus(
                    context.Identity);
                AssertEx.Equal(LMCRecorderState.Ready, status.State);
                connection.Diagnostics.ReleaseRecorderBuffer(
                    context.Identity);
                connection.Diagnostics.ReleaseRecorder(sourceHandle);
                AssertEx.True(context.Identity.IsBufferReleased);
                AssertEx.True(sourceHandle.IsReleased);
                return;
            }

            if (path == RecorderAcceptedPath.AdoptExact
                || path == RecorderAcceptedPath.AdoptActive)
            {
                var status = connection.Diagnostics.GetRecorderStatus(
                    context.Identity);
                AssertEx.Equal(LMCRecorderState.Ready, status.State);
                connection.Diagnostics.ReleaseRecorderBuffer(
                    context.Identity);
                connection.Diagnostics.ReleaseRecorder(context.Identity);
                AssertEx.True(context.Identity.IsBufferReleased);
                AssertEx.True(context.Identity.IsRecorderReleased);
                return;
            }

            connection.Diagnostics.ReleaseRecorder(
                context.RecoveredConfigurationLease);
            AssertEx.True(context.RecoveredConfigurationLease.IsReleased);
        }

        private static ushort RecorderAcceptedCommand(
            RecorderAcceptedPath path)
        {
            switch (path)
            {
                case RecorderAcceptedPath.Configure:
                    return LMC_CommandId.ConfigureRecorder;
                case RecorderAcceptedPath.ConfigureRecoverableDouble:
                    return LMC_CommandId.ConfigureRecoverableDoubleRecorder;
                case RecorderAcceptedPath.Start:
                    return LMC_CommandId.StartRecorder;
                case RecorderAcceptedPath.AdoptExact:
                case RecorderAcceptedPath.AdoptActive:
                    return LMC_CommandId.AdoptRecorder;
                case RecorderAcceptedPath.AdoptEmpty:
                    return LMC_CommandId.AdoptEmptyRecorderConfiguration;
                default:
                    throw new ArgumentOutOfRangeException("path");
            }
        }

        private static string RecorderAcceptedPathName(
            RecorderAcceptedPath path)
        {
            switch (path)
            {
                case RecorderAcceptedPath.Configure:
                    return "ConfigureRecorder";
                case RecorderAcceptedPath.ConfigureRecoverableDouble:
                    return "ConfigureRecoverableDoubleRecorder";
                case RecorderAcceptedPath.Start:
                    return "StartRecorder";
                case RecorderAcceptedPath.AdoptExact:
                    return "AdoptRecorder";
                case RecorderAcceptedPath.AdoptActive:
                    return "AdoptActiveRecorder";
                case RecorderAcceptedPath.AdoptEmpty:
                    return "AdoptEmptyRecorderConfiguration";
                default:
                    throw new ArgumentOutOfRangeException("path");
            }
        }

        private static LMCRecorderAcceptedOperation RecorderAcceptedOperation(
            RecorderAcceptedPath path)
        {
            switch (path)
            {
                case RecorderAcceptedPath.Configure:
                    return LMCRecorderAcceptedOperation.ConfigureRecorder;
                case RecorderAcceptedPath.ConfigureRecoverableDouble:
                    return LMCRecorderAcceptedOperation
                        .ConfigureRecoverableDoubleRecorder;
                case RecorderAcceptedPath.Start:
                    return LMCRecorderAcceptedOperation.StartRecorder;
                case RecorderAcceptedPath.AdoptExact:
                    return LMCRecorderAcceptedOperation.AdoptRecorder;
                case RecorderAcceptedPath.AdoptActive:
                    return LMCRecorderAcceptedOperation.AdoptActiveRecorder;
                case RecorderAcceptedPath.AdoptEmpty:
                    return LMCRecorderAcceptedOperation
                        .AdoptEmptyRecorderConfiguration;
                default:
                    throw new ArgumentOutOfRangeException("path");
            }
        }

        private static byte[] RecorderAcceptedResultPayload(
            RecorderAcceptedPath path,
            uint requestId)
        {
            switch (path)
            {
                case RecorderAcceptedPath.Configure:
                    return RecorderConfigurePayload(requestId);
                case RecorderAcceptedPath.ConfigureRecoverableDouble:
                    return RecorderRecoverableConfigurePayload(
                        requestId,
                        RecorderRecoveryToken);
                case RecorderAcceptedPath.Start:
                    return RecorderStartPayload(requestId);
                case RecorderAcceptedPath.AdoptExact:
                case RecorderAcceptedPath.AdoptActive:
                    return RecorderAdoptPayload(requestId);
                case RecorderAcceptedPath.AdoptEmpty:
                    return RecorderAdoptEmptyConfigurationPayload(requestId);
                default:
                    throw new ArgumentOutOfRangeException("path");
            }
        }

        private static void AddRecorderAcceptedCleanupSteps(
            ICollection<FakeRpcStep> steps,
            RecorderAcceptedPath path,
            uint requestId)
        {
            if (path == RecorderAcceptedPath.Configure
                || path
                    == RecorderAcceptedPath.ConfigureRecoverableDouble
                || path == RecorderAcceptedPath.AdoptEmpty)
            {
                steps.Add(
                    new FakeRpcStep(
                        LMC_CommandId.ReleaseRecorder,
                        TestFrame.Response(
                            0,
                            CommonPayload(16, requestId + 1))));
                return;
            }

            if (path == RecorderAcceptedPath.Start)
            {
                steps.Add(
                    new FakeRpcStep(
                        LMC_CommandId.StopRecorder,
                        TestFrame.Response(0, CommonPayload(16, 4))));
                steps.Add(
                    new FakeRpcStep(
                        LMC_CommandId.ReadRecorderStatus,
                        TestFrame.Response(0, RecorderStatusPayload(5))));
                steps.Add(
                    new FakeRpcStep(
                        LMC_CommandId.ReleaseRecorderBuffer,
                        TestFrame.Response(0, CommonPayload(16, 6))));
                steps.Add(
                    new FakeRpcStep(
                        LMC_CommandId.ReleaseRecorder,
                        TestFrame.Response(0, CommonPayload(16, 7))));
                return;
            }

            steps.Add(
                new FakeRpcStep(
                    LMC_CommandId.ReadRecorderStatus,
                    TestFrame.Response(0, RecorderStatusPayload(3))));
            steps.Add(
                new FakeRpcStep(
                    LMC_CommandId.ReleaseRecorderBuffer,
                    TestFrame.Response(0, CommonPayload(16, 4))));
            steps.Add(
                new FakeRpcStep(
                    LMC_CommandId.ReleaseRecorder,
                    TestFrame.Response(0, CommonPayload(16, 5))));
        }

        private static void AssertRecorderAcceptedResultContext(
            RecorderAcceptedPath path,
            ushort command,
            LMCRecorderAcceptedResultFailureContext context,
            LMCRecorderConfigurationHandle sourceHandle)
        {
            AssertEx.NotNull(context);
            AssertEx.Equal(RecorderAcceptedOperation(path), context.Operation);
            AssertEx.Equal(command, context.Command);
            AssertEx.Equal(DiagnosticsBootId, context.DiagnosticsBootId);
            AssertEx.Equal(MapRevision, context.MapRevision);
            AssertEx.True(context.IsAcceptedResultRecoveryOnly);

            if (path == RecorderAcceptedPath.Configure
                || path
                    == RecorderAcceptedPath.ConfigureRecoverableDouble)
            {
                AssertEx.Equal(
                    LMCRecorderAcceptedResultKind.ConfigurationHandle,
                    context.ResultKind);
                AssertEx.True(
                    ReferenceEquals(
                        context.AcceptedResult,
                        context.ConfigurationHandle));
                AssertEx.True(context.Identity == null);
                AssertEx.True(context.RecoveredConfigurationLease == null);
                AssertEx.True(context.SourceConfigurationHandle == null);
                AssertEx.True(
                    context.ConfigurationHandle
                        .IsAcceptedResultRecoveryOnly);
                AssertEx.Equal(RecorderConfigId, context.ConfigId);
                AssertEx.Equal(
                    RecorderConfigRevision,
                    context.ConfigRevision);
                AssertEx.Equal(
                    RecorderOwnerSessionEpoch,
                    context.OwnerSessionEpoch);
                AssertEx.Equal(
                    path
                        == RecorderAcceptedPath.ConfigureRecoverableDouble
                            ? RecorderRecoveryToken
                            : Guid.Empty,
                    context.RecoveryToken);
                return;
            }

            if (path == RecorderAcceptedPath.Start)
            {
                AssertEx.Equal(
                    LMCRecorderAcceptedResultKind.Identity,
                    context.ResultKind);
                AssertEx.True(
                    ReferenceEquals(context.AcceptedResult, context.Identity));
                AssertEx.True(context.ConfigurationHandle == null);
                AssertEx.True(context.RecoveredConfigurationLease == null);
                AssertEx.True(
                    ReferenceEquals(
                        sourceHandle,
                        context.SourceConfigurationHandle));
                AssertEx.True(context.Identity.IsAcceptedResultRecoveryOnly);
                AssertEx.True(sourceHandle.IsAcceptedResultRecoveryOnly);
                AssertEx.Equal(RecorderRecordId, context.RecordId);
                AssertEx.Equal((uint)0, context.BufferId);
                AssertEx.Equal(RecorderConfigId, context.ConfigId);
                AssertEx.Equal(
                    RecorderConfigRevision,
                    context.ConfigRevision);
                AssertEx.Equal(
                    RecorderOwnerSessionEpoch,
                    context.OwnerSessionEpoch);
                return;
            }

            if (path == RecorderAcceptedPath.AdoptExact
                || path == RecorderAcceptedPath.AdoptActive)
            {
                AssertEx.Equal(
                    LMCRecorderAcceptedResultKind.Identity,
                    context.ResultKind);
                AssertEx.True(
                    ReferenceEquals(context.AcceptedResult, context.Identity));
                AssertEx.True(context.ConfigurationHandle == null);
                AssertEx.True(context.RecoveredConfigurationLease == null);
                AssertEx.True(context.SourceConfigurationHandle == null);
                AssertEx.True(context.Identity.IsAcceptedResultRecoveryOnly);
                AssertEx.Equal(RecorderRecordId, context.RecordId);
                AssertEx.Equal((uint)0, context.BufferId);
                AssertEx.Equal((uint)0, context.ConfigId);
                AssertEx.Equal((uint)0, context.ConfigRevision);
                AssertEx.Equal(
                    RecorderOwnerSessionEpoch,
                    context.OwnerSessionEpoch);
                return;
            }

            AssertEx.Equal(
                LMCRecorderAcceptedResultKind.RecoveredConfigurationLease,
                context.ResultKind);
            AssertEx.True(
                ReferenceEquals(
                    context.AcceptedResult,
                    context.RecoveredConfigurationLease));
            AssertEx.True(context.ConfigurationHandle == null);
            AssertEx.True(context.Identity == null);
            AssertEx.True(context.SourceConfigurationHandle == null);
            AssertEx.True(
                context.RecoveredConfigurationLease
                    .IsAcceptedResultRecoveryOnly);
            AssertEx.Equal(RecorderConfigId, context.ConfigId);
            AssertEx.Equal(RecorderConfigRevision, context.ConfigRevision);
            AssertEx.Equal(
                RecorderOwnerSessionEpoch,
                context.PreviousOwnerSessionEpoch);
            AssertEx.Equal(
                RecorderReconnectedOwnerSessionEpoch,
                context.OwnerSessionEpoch);
        }

        private static ushort[] RecorderAcceptedExpectedCommands(
            RecorderAcceptedPath path)
        {
            var commands = new List<ushort>
            {
                LMC_CommandId.RpcSessionInit,
                LMC_CommandId.RpcCallbackRegistration,
                LMC_CommandId.GetGroupByName,
                LMC_CommandId.GetDiagnosticsCapabilities
            };
            if (path == RecorderAcceptedPath.Start)
            {
                commands.Add(LMC_CommandId.ConfigureRecorder);
            }

            commands.Add(RecorderAcceptedCommand(path));
            commands.Add(LMC_CommandId.GroupStop);
            if (path == RecorderAcceptedPath.Start)
            {
                commands.Add(LMC_CommandId.StopRecorder);
                commands.Add(LMC_CommandId.ReadRecorderStatus);
                commands.Add(LMC_CommandId.ReleaseRecorderBuffer);
                commands.Add(LMC_CommandId.ReleaseRecorder);
            }
            else if (path == RecorderAcceptedPath.AdoptExact
                || path == RecorderAcceptedPath.AdoptActive)
            {
                commands.Add(LMC_CommandId.ReadRecorderStatus);
                commands.Add(LMC_CommandId.ReleaseRecorderBuffer);
                commands.Add(LMC_CommandId.ReleaseRecorder);
            }
            else
            {
                commands.Add(LMC_CommandId.ReleaseRecorder);
            }

            commands.Add(LMC_CommandId.CloseConnection);
            return commands.ToArray();
        }

        private static void RecorderTriggerSyncDelayedAckDiscarded()
        {
            RunRecorderControlDelayedAckDiscarded(true, false);
        }

        private static void RecorderTriggerAsyncDelayedAckDiscarded()
        {
            RunRecorderControlDelayedAckDiscarded(true, true);
        }

        private static void RecorderStopSyncDelayedAckDiscarded()
        {
            RunRecorderControlDelayedAckDiscarded(false, false);
        }

        private static void RecorderStopAsyncDelayedAckDiscarded()
        {
            RunRecorderControlDelayedAckDiscarded(false, true);
        }

        private static void RunRecorderControlDelayedAckDiscarded(
            bool trigger,
            bool useAsync)
        {
            var command = trigger
                ? LMC_CommandId.TriggerRecorder
                : LMC_CommandId.StopRecorder;
            var operationName = trigger ? "TriggerRecorder" : "StopRecorder";
            var normalOperation = "Delayed "
                + (useAsync ? "async " : "sync ")
                + operationName
                + " ACK";
            var priorityOperation = "Priority GroupStop after "
                + (useAsync ? "async " : "sync ")
                + operationName;
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var mutationReceived = new ManualResetEventSlim(false))
            using (var releaseMutation = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                GroupLookupStep(),
                new FakeRpcStep(
                    LMC_CommandId.GetDiagnosticsCapabilities,
                    TestFrame.Response(0, RecorderCapabilitiesPayload(1))),
                new FakeRpcStep(
                    LMC_CommandId.ConfigureRecorder,
                    TestFrame.Response(0, RecorderConfigurePayload(2))),
                new FakeRpcStep(
                    LMC_CommandId.StartRecorder,
                    TestFrame.Response(0, RecorderStartPayload(3))),
                DelayedRecorderMutationStep(
                    command,
                    4,
                    mutationReceived,
                    releaseMutation),
                GroupStopStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    Connect(connection, server.Port);
                    var group = new LMCGroup(connection, "_LMCRobotBase1");
                    var configuration = trigger
                        ? RecorderTriggerConfiguration()
                        : RecorderManualConfiguration();
                    var handle = connection.Diagnostics.ConfigureRecorder(
                        configuration);
                    var identity = connection.Diagnostics.StartRecorder(handle);
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var delayedMutation = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                normalOperation))
                            {
                                if (trigger)
                                {
                                    if (useAsync)
                                    {
                                        connection.Diagnostics
                                            .TriggerRecorderAsync(
                                                identity,
                                                CancellationToken.None)
                                            .GetAwaiter()
                                            .GetResult();
                                    }
                                    else
                                    {
                                        connection.Diagnostics.TriggerRecorder(
                                            identity);
                                    }
                                }
                                else if (useAsync)
                                {
                                    connection.Diagnostics.StopRecorderAsync(
                                            identity,
                                            CancellationToken.None)
                                        .GetAwaiter()
                                        .GetResult();
                                }
                                else
                                {
                                    connection.Diagnostics.StopRecorder(identity);
                                }
                            }
                        });

                    AssertEx.True(
                        mutationReceived.Wait(2000),
                        "The delayed Recorder control ACK did not reach the server.");
                    var reservedGeneration = coordinator.ReservePrioritySend();
                    var priorityStop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                priorityOperation))
                            {
                                return group.GroupStop(1000, 0);
                            }
                        });

                    releaseMutation.Set();
                    var error = AssertEx.Throws<LMCSendPreemptedException>(
                        () => delayedMutation.GetAwaiter().GetResult());
                    var stopResponse = priorityStop.GetAwaiter().GetResult();

                    AssertResultDiscarded(
                        error,
                        normalOperation,
                        command,
                        expectedGeneration,
                        reservedGeneration);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.True(connection.IsConnected);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        LMC_CommandId.RpcSessionInit,
                        LMC_CommandId.RpcCallbackRegistration,
                        LMC_CommandId.GetGroupByName,
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        LMC_CommandId.ConfigureRecorder,
                        LMC_CommandId.StartRecorder,
                        command,
                        LMC_CommandId.GroupStop,
                        LMC_CommandId.CloseConnection);
                }
                finally
                {
                    releaseMutation.Set();
                }
            }
        }

        private static void RecorderConcurrentStartGuardsSyncAndAsync()
        {
            foreach (var firstStartAsync in new[] { false, true })
            {
                foreach (var competingCallAsync in new[] { false, true })
                {
                    RunRecorderConcurrentStartGuard(
                        firstStartAsync,
                        competingCallAsync,
                        false);
                    RunRecorderConcurrentStartGuard(
                        firstStartAsync,
                        competingCallAsync,
                        true);
                }
            }
        }

        private static void RunRecorderConcurrentStartGuard(
            bool firstStartAsync,
            bool competingCallAsync,
            bool attemptRelease)
        {
            using (var startReceived = new ManualResetEventSlim(false))
            using (var releaseStart = new ManualResetEventSlim(false))
            {
                var startStep = new FakeRpcStep(
                    LMC_CommandId.StartRecorder,
                    TestFrame.Response(0, RecorderStartPayload(3)))
                {
                    InspectRequest = request =>
                    {
                        startReceived.Set();
                        AssertEx.True(
                            releaseStart.Wait(5000),
                            "The first Recorder Start response was not released.");
                    }
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    new FakeRpcStep(
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        TestFrame.Response(
                            0,
                            RecorderCapabilitiesPayload(1))),
                    new FakeRpcStep(
                        LMC_CommandId.ConfigureRecorder,
                        TestFrame.Response(
                            0,
                            RecorderConfigurePayload(2))),
                    startStep,
                    new FakeRpcStep(
                        LMC_CommandId.StopRecorder,
                        TestFrame.Response(0, CommonPayload(16, 4))),
                    new FakeRpcStep(
                        LMC_CommandId.ReadRecorderStatus,
                        TestFrame.Response(0, RecorderStatusPayload(5))),
                    new FakeRpcStep(
                        LMC_CommandId.ReleaseRecorderBuffer,
                        TestFrame.Response(0, CommonPayload(16, 6))),
                    new FakeRpcStep(
                        LMC_CommandId.ReleaseRecorder,
                        TestFrame.Response(0, CommonPayload(16, 7))),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    Connect(connection, server.Port);
                    var handle = connection.Diagnostics.ConfigureRecorder(
                        RecorderManualConfiguration());
                    var firstStart = firstStartAsync
                        ? connection.Diagnostics.StartRecorderAsync(
                            handle,
                            CancellationToken.None)
                        : Task.Run(
                            () => connection.Diagnostics.StartRecorder(
                                handle));

                    try
                    {
                        AssertEx.True(
                            startReceived.Wait(2000),
                            "The first Recorder Start request did not reach the server.");
                        var rejected = Task.Run(
                            () => AssertEx.Throws<InvalidOperationException>(
                                () =>
                                {
                                    if (attemptRelease)
                                    {
                                        if (competingCallAsync)
                                        {
                                            connection.Diagnostics
                                                .ReleaseRecorderAsync(
                                                    handle,
                                                    CancellationToken.None)
                                                .GetAwaiter()
                                                .GetResult();
                                        }
                                        else
                                        {
                                            connection.Diagnostics
                                                .ReleaseRecorder(handle);
                                        }

                                        return;
                                    }

                                    if (competingCallAsync)
                                    {
                                        connection.Diagnostics
                                            .StartRecorderAsync(
                                                handle,
                                                CancellationToken.None)
                                            .GetAwaiter()
                                            .GetResult();
                                    }
                                    else
                                    {
                                        connection.Diagnostics.StartRecorder(
                                            handle);
                                    }
                                }));
                        AssertEx.True(
                            rejected.Wait(1000),
                            attemptRelease
                                ? "Recorder Release waited on the transport instead of rejecting the in-progress Start before wire."
                                : "A second Recorder Start waited on the transport instead of rejecting the in-progress Start before wire.");
                        var error = rejected.GetAwaiter().GetResult();
                        AssertEx.Contains(
                            attemptRelease
                                ? "currently being started"
                                : "already being started",
                            error.Message);
                    }
                    finally
                    {
                        releaseStart.Set();
                    }

                    var identity = firstStart.GetAwaiter().GetResult();
                    AssertEx.Equal(RecorderRecordId, identity.RecordId);
                    connection.Diagnostics.StopRecorder(identity);
                    var status = connection.Diagnostics.GetRecorderStatus(
                        identity);
                    AssertEx.Equal(LMCRecorderState.Ready, status.State);
                    connection.Diagnostics.ReleaseRecorderBuffer(identity);
                    connection.Diagnostics.ReleaseRecorder(handle);
                    AssertEx.True(identity.IsBufferReleased);
                    AssertEx.True(handle.IsReleased);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommands(
                        server,
                        LMC_CommandId.RpcSessionInit,
                        LMC_CommandId.RpcCallbackRegistration,
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        LMC_CommandId.ConfigureRecorder,
                        LMC_CommandId.StartRecorder,
                        LMC_CommandId.StopRecorder,
                        LMC_CommandId.ReadRecorderStatus,
                        LMC_CommandId.ReleaseRecorderBuffer,
                        LMC_CommandId.ReleaseRecorder,
                        LMC_CommandId.CloseConnection);
                }
            }
        }

        private static void RecorderReleaseBeforeWireRollsBackAllLeaseTypes()
        {
            RunRecorderReleaseBeforeWireRollback(
                RecorderReleasePath.ConfigurationHandle);
            RunRecorderReleaseBeforeWireRollback(
                RecorderReleasePath.IdentityBuffer);
            RunRecorderReleaseBeforeWireRollback(
                RecorderReleasePath.RecoveredConfiguration);
            RunRecorderReleaseBeforeWireRollback(
                RecorderReleasePath.AdoptedIdentityConfiguration);
        }

        private static void RunRecorderReleaseBeforeWireRollback(
            RecorderReleasePath path)
        {
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };
            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    LMC_CommandId.GetDiagnosticsCapabilities,
                    TestFrame.Response(
                        0,
                        RecorderCapabilitiesPayload(1)))
            };
            var expectedCommands = new List<ushort>
            {
                LMC_CommandId.RpcSessionInit,
                LMC_CommandId.RpcCallbackRegistration,
                LMC_CommandId.GetDiagnosticsCapabilities
            };

            switch (path)
            {
                case RecorderReleasePath.ConfigurationHandle:
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.ConfigureRecorder,
                            TestFrame.Response(
                                0,
                                RecorderConfigurePayload(2))));
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.ReleaseRecorder,
                            TestFrame.Response(0, CommonPayload(16, 4))));
                    expectedCommands.Add(LMC_CommandId.ConfigureRecorder);
                    expectedCommands.Add(LMC_CommandId.ReleaseRecorder);
                    break;

                case RecorderReleasePath.IdentityBuffer:
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.ConfigureRecorder,
                            TestFrame.Response(
                                0,
                                RecorderConfigurePayload(2))));
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.StartRecorder,
                            TestFrame.Response(
                                0,
                                RecorderStartPayload(3))));
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.ReleaseRecorderBuffer,
                            TestFrame.Response(0, CommonPayload(16, 5))));
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.ReleaseRecorder,
                            TestFrame.Response(0, CommonPayload(16, 6))));
                    expectedCommands.Add(LMC_CommandId.ConfigureRecorder);
                    expectedCommands.Add(LMC_CommandId.StartRecorder);
                    expectedCommands.Add(LMC_CommandId.ReleaseRecorderBuffer);
                    expectedCommands.Add(LMC_CommandId.ReleaseRecorder);
                    break;

                case RecorderReleasePath.RecoveredConfiguration:
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.AdoptEmptyRecorderConfiguration,
                            TestFrame.Response(
                                0,
                                RecorderAdoptEmptyConfigurationPayload(2))));
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.ReleaseRecorder,
                            TestFrame.Response(0, CommonPayload(16, 4))));
                    expectedCommands.Add(
                        LMC_CommandId.AdoptEmptyRecorderConfiguration);
                    expectedCommands.Add(LMC_CommandId.ReleaseRecorder);
                    break;

                case RecorderReleasePath.AdoptedIdentityConfiguration:
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.AdoptRecorder,
                            TestFrame.Response(
                                0,
                                RecorderAdoptPayload(2))));
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.ReadRecorderStatus,
                            TestFrame.Response(
                                0,
                                RecorderStatusPayload(3))));
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.ReleaseRecorderBuffer,
                            TestFrame.Response(0, CommonPayload(16, 4))));
                    steps.Add(
                        new FakeRpcStep(
                            LMC_CommandId.ReleaseRecorder,
                            TestFrame.Response(0, CommonPayload(16, 6))));
                    expectedCommands.Add(LMC_CommandId.AdoptRecorder);
                    expectedCommands.Add(LMC_CommandId.ReadRecorderStatus);
                    expectedCommands.Add(LMC_CommandId.ReleaseRecorderBuffer);
                    expectedCommands.Add(LMC_CommandId.ReleaseRecorder);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("path");
            }

            steps.Add(CloseStep());
            expectedCommands.Add(LMC_CommandId.CloseConnection);

            using (var server = new FakeRpcServer(steps.ToArray()))
            using (var connection = new LMCConnection(options))
            {
                Connect(connection, server.Port);
                LMCRecorderConfigurationHandle handle = null;
                LMCRecorderIdentity identity = null;
                LMCRecoveredRecorderConfigurationLease recovered = null;
                Action release;
                Action assertRolledBack;
                Action assertReleased;
                Action cleanup = () => { };

                switch (path)
                {
                    case RecorderReleasePath.ConfigurationHandle:
                        handle = connection.Diagnostics.ConfigureRecorder(
                            RecorderManualConfiguration());
                        release = () =>
                            connection.Diagnostics.ReleaseRecorder(handle);
                        assertRolledBack = () =>
                        {
                            AssertEx.False(handle.IsReleased);
                            AssertEx.False(handle.IsReleaseOutcomeUnverified);
                        };
                        assertReleased = () => AssertEx.True(
                            handle.IsReleased);
                        break;

                    case RecorderReleasePath.IdentityBuffer:
                        handle = connection.Diagnostics.ConfigureRecorder(
                            RecorderManualConfiguration());
                        identity = connection.Diagnostics.StartRecorder(handle);
                        release = () => connection.Diagnostics
                            .ReleaseRecorderBuffer(identity);
                        assertRolledBack = () =>
                        {
                            AssertEx.False(identity.IsBufferReleased);
                            AssertEx.False(
                                identity.IsBufferReleaseOutcomeUnverified);
                        };
                        assertReleased = () => AssertEx.True(
                            identity.IsBufferReleased);
                        cleanup = () => connection.Diagnostics
                            .ReleaseRecorder(handle);
                        break;

                    case RecorderReleasePath.RecoveredConfiguration:
                        recovered = connection.Diagnostics
                            .AdoptEmptyRecorderConfiguration(
                                EmptyClosedRecorderBankInventory(99));
                        release = () => connection.Diagnostics
                            .ReleaseRecorder(recovered);
                        assertRolledBack = () =>
                        {
                            AssertEx.False(recovered.IsReleased);
                            AssertEx.False(
                                recovered.IsReleaseOutcomeUnverified);
                        };
                        assertReleased = () => AssertEx.True(
                            recovered.IsReleased);
                        break;

                    case RecorderReleasePath.AdoptedIdentityConfiguration:
                        identity = connection.Diagnostics.AdoptRecorder(
                            DiagnosticsBootId,
                            RecorderRecordId,
                            0);
                        connection.Diagnostics.GetRecorderStatus(identity);
                        connection.Diagnostics.ReleaseRecorderBuffer(identity);
                        release = () => connection.Diagnostics
                            .ReleaseRecorder(identity);
                        assertRolledBack = () =>
                        {
                            AssertEx.False(identity.IsRecorderReleased);
                            AssertEx.False(
                                identity.IsRecorderReleaseOutcomeUnverified);
                        };
                        assertReleased = () => AssertEx.True(
                            identity.IsRecorderReleased);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("path");
                }

                var operation = "Recorder "
                    + path
                    + " release before-wire rollback";
                var expectedGeneration = coordinator.CurrentGeneration;
                var reservedGeneration = coordinator.ReservePrioritySend();
                LMCSendPreemptedException error;
                using (coordinator.BeginPreemptibleScope(
                    expectedGeneration,
                    operation))
                {
                    error = AssertEx.Throws<LMCSendPreemptedException>(release);
                }

                AssertPreempted(
                    error,
                    operation,
                    path == RecorderReleasePath.IdentityBuffer
                        ? LMC_CommandId.ReleaseRecorderBuffer
                        : LMC_CommandId.ReleaseRecorder,
                    expectedGeneration,
                    reservedGeneration);
                assertRolledBack();
                release();
                assertReleased();
                cleanup();

                connection.CloseConnection();
                server.Verify();
                AssertCommands(server, expectedCommands.ToArray());
            }
        }

        private static void RecorderBufferReleaseSyncDelayedAckQuarantined()
        {
            RunRecorderBufferReleaseDelayedAckQuarantined(false);
        }

        private static void RecorderBufferReleaseAsyncDelayedAckQuarantined()
        {
            RunRecorderBufferReleaseDelayedAckQuarantined(true);
        }

        private static void RunRecorderBufferReleaseDelayedAckQuarantined(
            bool useAsync)
        {
            LMCRecorderIdentity identity = null;
            RunRecorderReleaseDelayedAckQuarantined(
                useAsync,
                "Recorder buffer release",
                LMC_CommandId.ReleaseRecorderBuffer,
                4,
                new[]
                {
                    new FakeRpcStep(
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        TestFrame.Response(
                            0,
                            RecorderCapabilitiesPayload(1))),
                    new FakeRpcStep(
                        LMC_CommandId.ConfigureRecorder,
                        TestFrame.Response(0, RecorderConfigurePayload(2))),
                    new FakeRpcStep(
                        LMC_CommandId.StartRecorder,
                        TestFrame.Response(0, RecorderStartPayload(3)))
                },
                connection =>
                {
                    var handle = connection.Diagnostics.ConfigureRecorder(
                        RecorderManualConfiguration());
                    identity = connection.Diagnostics.StartRecorder(handle);
                    return () =>
                    {
                        if (useAsync)
                        {
                            connection.Diagnostics.ReleaseRecorderBufferAsync(
                                    identity,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        }
                        else
                        {
                            connection.Diagnostics.ReleaseRecorderBuffer(
                                identity);
                        }
                    };
                },
                connection =>
                {
                    AssertEx.False(identity.IsBufferReleased);
                    AssertEx.True(identity.IsBufferReleaseOutcomeUnverified);
                    var reuse = AssertEx.Throws<InvalidOperationException>(
                        () => connection.Diagnostics.GetRecorderStatus(identity));
                    AssertEx.Contains("outcome is unverified", reuse.Message);
                },
                LMC_CommandId.GetDiagnosticsCapabilities,
                LMC_CommandId.ConfigureRecorder,
                LMC_CommandId.StartRecorder,
                LMC_CommandId.ReleaseRecorderBuffer);
        }

        private static void RecorderHandleReleaseSyncDelayedAckQuarantined()
        {
            RunRecorderHandleReleaseDelayedAckQuarantined(false);
        }

        private static void RecorderHandleReleaseAsyncDelayedAckQuarantined()
        {
            RunRecorderHandleReleaseDelayedAckQuarantined(true);
        }

        private static void RunRecorderHandleReleaseDelayedAckQuarantined(
            bool useAsync)
        {
            LMCRecorderConfigurationHandle handle = null;
            RunRecorderReleaseDelayedAckQuarantined(
                useAsync,
                "Recorder handle release",
                LMC_CommandId.ReleaseRecorder,
                3,
                new[]
                {
                    new FakeRpcStep(
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        TestFrame.Response(
                            0,
                            RecorderCapabilitiesPayload(1))),
                    new FakeRpcStep(
                        LMC_CommandId.ConfigureRecorder,
                        TestFrame.Response(0, RecorderConfigurePayload(2)))
                },
                connection =>
                {
                    handle = connection.Diagnostics.ConfigureRecorder(
                        RecorderManualConfiguration());
                    return () =>
                    {
                        if (useAsync)
                        {
                            connection.Diagnostics.ReleaseRecorderAsync(
                                    handle,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        }
                        else
                        {
                            connection.Diagnostics.ReleaseRecorder(handle);
                        }
                    };
                },
                connection =>
                {
                    AssertEx.False(handle.IsReleased);
                    AssertEx.True(handle.IsReleaseOutcomeUnverified);
                    var reuse = AssertEx.Throws<InvalidOperationException>(
                        () => connection.Diagnostics.StartRecorder(handle));
                    AssertEx.Contains("outcome is unverified", reuse.Message);
                },
                LMC_CommandId.GetDiagnosticsCapabilities,
                LMC_CommandId.ConfigureRecorder,
                LMC_CommandId.ReleaseRecorder);
        }

        private static void RecorderRecoveredReleaseSyncDelayedAckQuarantined()
        {
            RunRecorderRecoveredReleaseDelayedAckQuarantined(false);
        }

        private static void RecorderRecoveredReleaseAsyncDelayedAckQuarantined()
        {
            RunRecorderRecoveredReleaseDelayedAckQuarantined(true);
        }

        private static void RunRecorderRecoveredReleaseDelayedAckQuarantined(
            bool useAsync)
        {
            LMCRecoveredRecorderConfigurationLease lease = null;
            var inventory = EmptyClosedRecorderBankInventory(99);
            RunRecorderReleaseDelayedAckQuarantined(
                useAsync,
                "Recovered Recorder configuration release",
                LMC_CommandId.ReleaseRecorder,
                3,
                new[]
                {
                    new FakeRpcStep(
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        TestFrame.Response(
                            0,
                            RecorderCapabilitiesPayload(1))),
                    new FakeRpcStep(
                        LMC_CommandId.AdoptEmptyRecorderConfiguration,
                        TestFrame.Response(
                            0,
                            RecorderAdoptEmptyConfigurationPayload(2)))
                },
                connection =>
                {
                    lease = connection.Diagnostics
                        .AdoptEmptyRecorderConfiguration(inventory);
                    return () =>
                    {
                        if (useAsync)
                        {
                            connection.Diagnostics.ReleaseRecorderAsync(
                                    lease,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        }
                        else
                        {
                            connection.Diagnostics.ReleaseRecorder(lease);
                        }
                    };
                },
                connection =>
                {
                    AssertEx.False(lease.IsReleased);
                    AssertEx.True(lease.IsReleaseOutcomeUnverified);
                },
                LMC_CommandId.GetDiagnosticsCapabilities,
                LMC_CommandId.AdoptEmptyRecorderConfiguration,
                LMC_CommandId.ReleaseRecorder);
        }

        private static void RecorderIdentityReleaseSyncDelayedAckQuarantined()
        {
            RunRecorderIdentityReleaseDelayedAckQuarantined(false);
        }

        private static void RecorderIdentityReleaseAsyncDelayedAckQuarantined()
        {
            RunRecorderIdentityReleaseDelayedAckQuarantined(true);
        }

        private static void RunRecorderIdentityReleaseDelayedAckQuarantined(
            bool useAsync)
        {
            LMCRecorderIdentity identity = null;
            RunRecorderReleaseDelayedAckQuarantined(
                useAsync,
                "Adopted Recorder identity release",
                LMC_CommandId.ReleaseRecorder,
                5,
                new[]
                {
                    new FakeRpcStep(
                        LMC_CommandId.GetDiagnosticsCapabilities,
                        TestFrame.Response(
                            0,
                            RecorderCapabilitiesPayload(1))),
                    new FakeRpcStep(
                        LMC_CommandId.AdoptRecorder,
                        TestFrame.Response(0, RecorderAdoptPayload(2))),
                    new FakeRpcStep(
                        LMC_CommandId.ReadRecorderStatus,
                        TestFrame.Response(0, RecorderStatusPayload(3))),
                    new FakeRpcStep(
                        LMC_CommandId.ReleaseRecorderBuffer,
                        TestFrame.Response(0, CommonPayload(16, 4)))
                },
                connection =>
                {
                    identity = connection.Diagnostics.AdoptRecorder(
                        DiagnosticsBootId,
                        RecorderRecordId,
                        0);
                    connection.Diagnostics.GetRecorderStatus(identity);
                    connection.Diagnostics.ReleaseRecorderBuffer(identity);
                    return () =>
                    {
                        if (useAsync)
                        {
                            connection.Diagnostics.ReleaseRecorderAsync(
                                    identity,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        }
                        else
                        {
                            connection.Diagnostics.ReleaseRecorder(identity);
                        }
                    };
                },
                connection =>
                {
                    AssertEx.True(identity.IsBufferReleased);
                    AssertEx.False(identity.IsRecorderReleased);
                    AssertEx.True(identity.IsRecorderReleaseOutcomeUnverified);
                    var reuse = AssertEx.Throws<InvalidOperationException>(
                        () => connection.Diagnostics.GetRecorderStatus(identity));
                    AssertEx.Contains("outcome is unverified", reuse.Message);
                },
                LMC_CommandId.GetDiagnosticsCapabilities,
                LMC_CommandId.AdoptRecorder,
                LMC_CommandId.ReadRecorderStatus,
                LMC_CommandId.ReleaseRecorderBuffer,
                LMC_CommandId.ReleaseRecorder);
        }

        private static void RunRecorderReleaseDelayedAckQuarantined(
            bool useAsync,
            string operationName,
            ushort command,
            uint requestId,
            IEnumerable<FakeRpcStep> setupSteps,
            Func<LMCConnection, Action> prepareMutation,
            Action<LMCConnection> assertQuarantined,
            params ushort[] operationCommands)
        {
            var normalOperation = "Delayed "
                + (useAsync ? "async " : "sync ")
                + operationName
                + " ACK";
            var priorityOperation = "Priority GroupStop after "
                + (useAsync ? "async " : "sync ")
                + operationName;
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var mutationReceived = new ManualResetEventSlim(false))
            using (var releaseMutation = new ManualResetEventSlim(false))
            {
                var steps = new List<FakeRpcStep>
                {
                    InitStep(),
                    CallbackStep(),
                    GroupLookupStep()
                };
                steps.AddRange(setupSteps);
                steps.Add(
                    DelayedRecorderMutationStep(
                        command,
                        requestId,
                        mutationReceived,
                        releaseMutation));
                steps.Add(GroupStopStep());
                steps.Add(CloseStep());

                using (var server = new FakeRpcServer(steps.ToArray()))
                using (var connection = new LMCConnection(options))
                {
                    try
                    {
                        Connect(connection, server.Port);
                        var group = new LMCGroup(
                            connection,
                            "_LMCRobotBase1");
                        var mutation = prepareMutation(connection);
                        var expectedGeneration = coordinator.CurrentGeneration;
                        var delayedMutation = Task.Run(
                            () =>
                            {
                                using (coordinator.BeginPreemptibleScope(
                                    expectedGeneration,
                                    normalOperation))
                                {
                                    mutation();
                                }
                            });

                        AssertEx.True(
                            mutationReceived.Wait(2000),
                            "The delayed Recorder release ACK did not reach the server.");
                        var reservedGeneration =
                            coordinator.ReservePrioritySend();
                        var priorityStop = Task.Run(
                            () =>
                            {
                                using (coordinator.BeginPriorityScope(
                                    reservedGeneration,
                                    priorityOperation))
                                {
                                    return group.GroupStop(1000, 0);
                                }
                            });

                        releaseMutation.Set();
                        var error = AssertEx.Throws<LMCSendPreemptedException>(
                            () => delayedMutation.GetAwaiter().GetResult());
                        var stopResponse = priorityStop.GetAwaiter().GetResult();

                        AssertResultDiscarded(
                            error,
                            normalOperation,
                            command,
                            expectedGeneration,
                            reservedGeneration);
                        AssertEx.True(stopResponse.IsSuccess);
                        AssertEx.True(connection.IsConnected);
                        assertQuarantined(connection);
                        var retry = AssertEx.Throws<InvalidOperationException>(
                            mutation);
                        AssertEx.Contains(
                            "outcome is unverified",
                            retry.Message);

                        connection.CloseConnection();
                        server.Verify();
                        var expectedCommands = new List<ushort>
                        {
                            LMC_CommandId.RpcSessionInit,
                            LMC_CommandId.RpcCallbackRegistration,
                            LMC_CommandId.GetGroupByName
                        };
                        expectedCommands.AddRange(operationCommands);
                        expectedCommands.Add(LMC_CommandId.GroupStop);
                        expectedCommands.Add(LMC_CommandId.CloseConnection);
                        AssertCommands(server, expectedCommands.ToArray());
                    }
                    finally
                    {
                        releaseMutation.Set();
                    }
                }
            }
        }

        private static void AssertPreempted(
            LMCSendPreemptedException error,
            string operation,
            ushort command,
            long expectedGeneration,
            long actualGeneration)
        {
            AssertEx.NotNull(error);
            AssertEx.Equal(operation, error.Operation);
            AssertEx.Equal(command, error.Command);
            AssertEx.Equal(expectedGeneration, error.ExpectedGeneration);
            AssertEx.Equal(actualGeneration, error.ActualGeneration);
            AssertEx.Equal(LMCSendPreemptionPhase.BeforeWire, error.Phase);
            AssertEx.Equal(
                operation
                    + " was cancelled before command 0x"
                    + command.ToString("X4", CultureInfo.InvariantCulture)
                    + " transmission because a newer Stop or Power Off request was reserved.",
                error.Message);
        }

        private static void AssertResultDiscarded(
            LMCSendPreemptedException error,
            string operation,
            ushort command,
            long expectedGeneration,
            long actualGeneration)
        {
            AssertEx.NotNull(error);
            AssertEx.Equal(operation, error.Operation);
            AssertEx.Equal(command, error.Command);
            AssertEx.Equal(expectedGeneration, error.ExpectedGeneration);
            AssertEx.Equal(actualGeneration, error.ActualGeneration);
            AssertEx.Equal(
                LMCSendPreemptionPhase.ResultDiscarded,
                error.Phase);
            AssertEx.Contains(
                "response for command 0x"
                    + command.ToString("X4", CultureInfo.InvariantCulture)
                    + " was discarded",
                error.Message);
        }

        private static void AssertCommands(
            FakeRpcServer server,
            params ushort[] expectedCommands)
        {
            AssertEx.Equal(expectedCommands.Length, server.ReceivedRequests.Count);
            for (var index = 0; index < expectedCommands.Length; index++)
            {
                AssertEx.Equal(
                    expectedCommands[index],
                    TestFrame.ReadUInt16(server.ReceivedRequests[index], 0),
                    "Unexpected wire command at index " + index + ".");
            }
        }

        private static LMCOperationTicket InvokeDigitalOutputWriteWithPolicy(
            LMCConnection connection,
            LMCDigitalOutputWriteRequest request)
        {
            var parameterTypes = new[]
            {
                typeof(LMCDigitalOutputWriteRequest),
                typeof(CancellationToken),
                typeof(Func<uint, bool>)
            };
            var method = typeof(LMCDiagnostics).GetMethod(
                "SubmitDigitalOutputWriteAsync",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                parameterTypes,
                null);
            AssertEx.True(
                method != null,
                "The private output-write wrapper policy seam was not found.");

            try
            {
                var task = (Task<LMCOperationTicket>)method.Invoke(
                    connection.Diagnostics,
                    new object[]
                    {
                        request,
                        CancellationToken.None,
                        new Func<uint, bool>(ioReference =>
                            ioReference == IOReference)
                    });
                return task.GetAwaiter().GetResult();
            }
            catch (TargetInvocationException error)
                when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }

        private static LMCOperationTicket
            InvokeDigitalOutputWriteWithPolicySync(
                LMCConnection connection,
                LMCDigitalOutputWriteRequest request)
        {
            var parameterTypes = new[]
            {
                typeof(LMCDigitalOutputWriteRequest),
                typeof(Func<uint, bool>)
            };
            var method = typeof(LMCDiagnostics).GetMethod(
                "SubmitDigitalOutputWrite",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                parameterTypes,
                null);
            AssertEx.True(
                method != null,
                "The private sync output-write policy seam was not found.");

            try
            {
                return (LMCOperationTicket)method.Invoke(
                    connection.Diagnostics,
                    new object[]
                    {
                        request,
                        new Func<uint, bool>(ioReference =>
                            ioReference == IOReference)
                    });
            }
            catch (TargetInvocationException error)
                when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }

        private static LMCEtherCATTopology CreateDigitalOutputTopology(
            LMCConnection connection)
        {
            var entries = new List<LMCEtherCATTopologyEntry>
            {
                new LMCEtherCATTopologyEntry(
                    TopologyNodeId,
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
            return new LMCEtherCATTopology(info, entries).BindProvenance(
                connection.Diagnostics,
                connection.SessionGeneration);
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

        private static FakeRpcStep GroupLookupStep()
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, GroupReference);
            return new FakeRpcStep(0x1042, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisLookupStep()
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, AxisReference);
            return new FakeRpcStep(
                LMC_CommandId.GetAxisByName,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisInfoStep()
        {
            var payload = new byte[8];
            TestFrame.WriteUInt32(payload, 0, AxisReference);
            return new FakeRpcStep(
                LMC_CommandId.AxisInfo,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep DelayedAxisResetStep(
            ManualResetEventSlim resetReceived,
            ManualResetEventSlim releaseReset)
        {
            return new FakeRpcStep(
                LMC_CommandId.Reset,
                TestFrame.Response(0, new byte[8]))
            {
                InspectRequest = request =>
                {
                    resetReceived.Set();
                    AssertEx.True(
                        releaseReset.Wait(5000),
                        "The delayed Axis Reset response was not released.");
                }
            };
        }

        private static FakeRpcStep AxisStopStep()
        {
            return new FakeRpcStep(
                LMC_CommandId.Stop,
                TestFrame.Response(0, new byte[8]));
        }

        private static FakeRpcStep DelayedGroupResetStep(
            ManualResetEventSlim resetReceived,
            ManualResetEventSlim releaseReset)
        {
            return new FakeRpcStep(
                LMC_CommandId.GroupReset,
                TestFrame.Response(0, new byte[8]))
            {
                InspectRequest = request =>
                {
                    resetReceived.Set();
                    AssertEx.True(
                        releaseReset.Wait(5000),
                        "The delayed GroupReset response was not released.");
                }
            };
        }

        private static FakeRpcStep DelayedGroupEnableStep(
            ManualResetEventSlim enableReceived,
            ManualResetEventSlim releaseEnable)
        {
            return new FakeRpcStep(
                LMC_CommandId.GroupProfileLock,
                TestFrame.Response(0, new byte[8]))
            {
                InspectRequest = request =>
                {
                    enableReceived.Set();
                    AssertEx.True(
                        releaseEnable.Wait(5000),
                        "The delayed GroupEnable response was not released.");
                }
            };
        }

        private static FakeRpcStep DelayedSdoSubmitStep(
            uint requestId,
            uint ticketId,
            ManualResetEventSlim submitReceived,
            ManualResetEventSlim releaseSubmit)
        {
            return new FakeRpcStep(
                LMC_CommandId.SubmitSdo,
                TestFrame.Response(
                    0,
                    SdoSubmitPayload(requestId, ticketId)))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        requestId,
                        TestFrame.ReadUInt32(request, 12));
                    submitReceived.Set();
                    AssertEx.True(
                        releaseSubmit.Wait(5000),
                        "The delayed SubmitSdo response was not released.");
                }
            };
        }

        private static FakeRpcStep DelayedCancelOperationStep(
            uint requestId,
            uint ticketId,
            ManualResetEventSlim cancelReceived,
            ManualResetEventSlim releaseCancel)
        {
            return new FakeRpcStep(
                LMC_CommandId.CancelOperation,
                TestFrame.Response(
                    0,
                    CancelOperationPayload(requestId, ticketId)))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        requestId,
                        TestFrame.ReadUInt32(request, 12));
                    AssertEx.Equal(
                        ticketId,
                        TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        TestFrame.ReadUInt32(request, 20));
                    cancelReceived.Set();
                    AssertEx.True(
                        releaseCancel.Wait(5000),
                        "The delayed CancelOperation response was not released.");
                }
            };
        }

        private static FakeRpcStep DelayedDigitalOutputSubmitStep(
            uint requestId,
            uint ticketId,
            ManualResetEventSlim submitReceived,
            ManualResetEventSlim releaseSubmit)
        {
            return new FakeRpcStep(
                LMC_CommandId.SubmitDigitalOutputWrite,
                TestFrame.Response(
                    0,
                    DigitalOutputSubmitPayload(requestId, ticketId)))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        requestId,
                        TestFrame.ReadUInt32(request, 12));
                    AssertEx.Equal(
                        TopologyRevision,
                        TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal(
                        IOReference,
                        TestFrame.ReadUInt32(request, 20));
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        TestFrame.ReadUInt32(request, 44));
                    submitReceived.Set();
                    AssertEx.True(
                        releaseSubmit.Wait(5000),
                        "The delayed digital-output response was not released.");
                }
            };
        }

        private static FakeRpcStep DelayedTopologyIoReadStep(
            ushort command,
            uint requestId,
            ManualResetEventSlim readReceived,
            ManualResetEventSlim releaseRead)
        {
            if (command != LMC_CommandId.ReadEtherCATNodeHealth
                && command != LMC_CommandId.ReadDigitalIO)
            {
                throw new ArgumentOutOfRangeException("command");
            }

            var payload = command == LMC_CommandId.ReadEtherCATNodeHealth
                ? EtherCATNodeHealthPayload(requestId)
                : DigitalOutputIoPayload(requestId);
            return new FakeRpcStep(
                command,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        requestId,
                        TestFrame.ReadUInt32(request, 12));
                    AssertEx.Equal(
                        TopologyRevision,
                        TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal(
                        command == LMC_CommandId.ReadEtherCATNodeHealth
                            ? TopologyNodeId
                            : IOReference,
                        TestFrame.ReadUInt32(request, 20));
                    readReceived.Set();
                    AssertEx.True(
                        releaseRead.Wait(5000),
                        "The delayed topology I/O read response was not released.");
                }
            };
        }

        private static FakeRpcStep DelayedRecorderMutationStep(
            ushort command,
            uint requestId,
            ManualResetEventSlim mutationReceived,
            ManualResetEventSlim releaseMutation)
        {
            return new FakeRpcStep(
                command,
                TestFrame.Response(0, CommonPayload(16, requestId)))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        requestId,
                        TestFrame.ReadUInt32(request, 12));
                    mutationReceived.Set();
                    AssertEx.True(
                        releaseMutation.Wait(5000),
                        "The delayed Recorder mutation response was not released.");
                }
            };
        }

        private static FakeRpcStep DelayedRecorderAcceptedResultStep(
            RecorderAcceptedPath path,
            ushort command,
            uint requestId,
            byte[] payload,
            ManualResetEventSlim resultReceived,
            ManualResetEventSlim releaseResult)
        {
            return new FakeRpcStep(
                command,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(
                        requestId,
                        TestFrame.ReadUInt32(request, 12));
                    InspectRecorderAcceptedRequest(path, request);
                    resultReceived.Set();
                    AssertEx.True(
                        releaseResult.Wait(5000),
                        "The delayed Recorder accepted response was not released.");
                }
            };
        }

        private static void InspectRecorderAcceptedRequest(
            RecorderAcceptedPath path,
            byte[] request)
        {
            switch (path)
            {
                case RecorderAcceptedPath.Configure:
                    AssertEx.Equal(
                        MapRevision,
                        TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal((uint)0, TestFrame.ReadUInt32(request, 20));
                    AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 24));
                    AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(request, 26));
                    AssertEx.Equal((uint)3, TestFrame.ReadUInt32(request, 28));
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        TestFrame.ReadUInt32(request, 60));
                    return;

                case RecorderAcceptedPath.ConfigureRecoverableDouble:
                    AssertEx.Equal(
                        MapRevision,
                        TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal(
                        RecorderConfigId,
                        TestFrame.ReadUInt32(request, 20));
                    AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 24));
                    AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(request, 26));
                    AssertEx.Equal((uint)10, TestFrame.ReadUInt32(request, 28));
                    AssertEx.Equal(
                        (byte)LMCRecorderBufferMode.Double,
                        request[32]);
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        TestFrame.ReadUInt32(request, 60));
                    var tokenBytes = RecorderRecoveryToken.ToByteArray();
                    for (var index = 0; index < tokenBytes.Length; index++)
                    {
                        AssertEx.Equal(tokenBytes[index], request[64 + index]);
                    }

                    return;

                case RecorderAcceptedPath.Start:
                    AssertEx.Equal(
                        RecorderConfigId,
                        TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal(
                        RecorderConfigRevision,
                        TestFrame.ReadUInt32(request, 20));
                    AssertEx.Equal(
                        MapRevision,
                        TestFrame.ReadUInt32(request, 24));
                    AssertEx.Equal(
                        RecorderOwnerSessionEpoch,
                        TestFrame.ReadUInt32(request, 28));
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        TestFrame.ReadUInt32(request, 32));
                    return;

                case RecorderAcceptedPath.AdoptExact:
                    AssertEx.Equal(
                        RecorderRecordId,
                        TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal((uint)0, TestFrame.ReadUInt32(request, 20));
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        TestFrame.ReadUInt32(request, 24));
                    return;

                case RecorderAcceptedPath.AdoptActive:
                    AssertEx.Equal((uint)0, TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal((uint)0, TestFrame.ReadUInt32(request, 20));
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        TestFrame.ReadUInt32(request, 24));
                    return;

                case RecorderAcceptedPath.AdoptEmpty:
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        TestFrame.ReadUInt32(request, 16));
                    AssertEx.Equal(
                        RecorderConfigId,
                        TestFrame.ReadUInt32(request, 20));
                    AssertEx.Equal(
                        RecorderConfigRevision,
                        TestFrame.ReadUInt32(request, 24));
                    AssertEx.Equal(
                        MapRevision,
                        TestFrame.ReadUInt32(request, 28));
                    AssertEx.Equal(
                        RecorderOwnerSessionEpoch,
                        TestFrame.ReadUInt32(request, 32));
                    return;

                default:
                    throw new ArgumentOutOfRangeException("path");
            }
        }

        private static FakeRpcStep GroupStopStep()
        {
            return new FakeRpcStep(
                LMC_CommandId.GroupStop,
                TestFrame.Response(0, new byte[8]));
        }

        private static FakeRpcStep AdminCapabilitiesStep()
        {
            var payload = AdminCommonPayload(1, 40);
            TestFrame.WriteUInt32(
                payload,
                16,
                (uint)LMCAdminFeature.GroupLinearRelative);
            TestFrame.WriteUInt32(payload, 20, 0x3Fu);
            TestFrame.WriteUInt32(
                payload,
                24,
                (uint)LMCGroupParameterSelection.All);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt16(payload, 32, GroupReference);
            TestFrame.WriteUInt16(payload, 34, 3);
            TestFrame.WriteUInt16(payload, 36, 1);
            return new FakeRpcStep(
                LMC_CommandId.GetAdminCapabilities,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep DelayedAdminGroupMoveStep(
            ManualResetEventSlim moveReceived,
            ManualResetEventSlim releaseMove)
        {
            return new FakeRpcStep(
                LMC_CommandId.GroupMoveLinearRelative,
                TestFrame.Response(0, AdminCommonPayload(2, 16)))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal((uint)2, TestFrame.ReadUInt32(request, 12));
                    AssertEx.Equal(GroupReference, TestFrame.ReadUInt16(request, 6));
                    moveReceived.Set();
                    AssertEx.True(
                        releaseMove.Wait(5000),
                        "The delayed Admin group move response was not released.");
                }
            };
        }

        private static byte[] AdminCommonPayload(uint requestId, int length)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static LMCRecorderConfiguration RecorderManualConfiguration()
        {
            return new LMCRecorderConfiguration(RecorderSignals, 1, 3);
        }

        private static LMCRecorderConfiguration
            RecorderRecoverableDoubleConfiguration()
        {
            return new LMCRecorderConfiguration(
                RecorderSignals,
                1,
                10,
                LMCRecorderBufferMode.Double,
                LMCRecorderTriggerType.Edge,
                LMCSignalValueType.Int32,
                4,
                5,
                RecorderSignal1,
                LMCRecorderTriggerOperator.RisingEdge,
                100,
                0,
                RecorderConfigId);
        }

        private static LMCRecorderConfiguration RecorderTriggerConfiguration()
        {
            return new LMCRecorderConfiguration(
                RecorderSignals,
                1,
                3,
                LMCRecorderBufferMode.Ring,
                LMCRecorderTriggerType.Edge,
                LMCSignalValueType.Int32,
                1,
                1,
                RecorderSignal1,
                LMCRecorderTriggerOperator.RisingEdge,
                100,
                0);
        }

        private static byte[] RecorderCapabilitiesPayload(uint requestId)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 3);
            TestFrame.WriteUInt32(
                payload,
                20,
                (uint)(LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.RecorderSingleBank
                    | LMCDiagnosticCapability.RecorderTrigger
                    | LMCDiagnosticCapability.RecorderDoubleBank));
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt16(payload, 28, 24);
            TestFrame.WriteUInt16(payload, 30, 32);
            TestFrame.WriteUInt16(payload, 32, 32);
            TestFrame.WriteUInt16(payload, 34, 2);
            TestFrame.WriteUInt32(payload, 36, 100);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 16);
            TestFrame.WriteUInt16(payload, 50, 80);
            TestFrame.WriteUInt16(payload, 52, 16);
            TestFrame.WriteUInt32(payload, 56, 800);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return payload;
        }

        private static byte[] RecorderSingleBankCapabilitiesPayload(
            uint requestId)
        {
            var payload = RecorderCapabilitiesPayload(requestId);
            TestFrame.WriteUInt32(
                payload,
                20,
                (uint)(LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.RecorderSingleBank
                    | LMCDiagnosticCapability.RecorderTrigger));
            TestFrame.WriteUInt16(payload, 34, 1);
            return payload;
        }

        private static byte[] RecorderConfigurePayload(uint requestId)
        {
            var payload = CommonPayload(56, requestId);
            TestFrame.WriteUInt32(payload, 16, RecorderConfigId);
            TestFrame.WriteUInt32(payload, 20, RecorderConfigRevision);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 28, 3);
            TestFrame.WriteUInt32(payload, 32, 24);
            TestFrame.WriteUInt16(
                payload,
                36,
                (ushort)LMCRecorderState.Configured);
            TestFrame.WriteUInt16(payload, 38, 2);
            TestFrame.WriteUInt16(payload, 40, 8);
            TestFrame.WriteUInt16(payload, 42, 1);
            TestFrame.WriteUInt16(
                payload,
                44,
                (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt32(
                payload,
                48,
                RecorderOwnerSessionEpoch);
            TestFrame.WriteUInt32(payload, 52, DiagnosticsBootId);
            return payload;
        }

        private static byte[] RecorderRecoverableConfigurePayload(
            uint requestId,
            Guid recoveryToken)
        {
            var payload = CommonPayload(72, requestId);
            TestFrame.WriteUInt32(payload, 16, RecorderConfigId);
            TestFrame.WriteUInt32(payload, 20, RecorderConfigRevision);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 28, 10);
            TestFrame.WriteUInt32(payload, 32, 160);
            TestFrame.WriteUInt16(
                payload,
                36,
                (ushort)LMCRecorderState.Configured);
            TestFrame.WriteUInt16(payload, 38, 2);
            TestFrame.WriteUInt16(payload, 40, 8);
            TestFrame.WriteUInt16(payload, 42, 2);
            TestFrame.WriteUInt16(
                payload,
                44,
                (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt32(
                payload,
                48,
                RecorderOwnerSessionEpoch);
            TestFrame.WriteUInt32(payload, 52, DiagnosticsBootId);
            var tokenBytes = recoveryToken.ToByteArray();
            Buffer.BlockCopy(
                tokenBytes,
                0,
                payload,
                56,
                tokenBytes.Length);
            return payload;
        }

        private static byte[] RecorderStartPayload(uint requestId)
        {
            var payload = CommonPayload(40, requestId);
            TestFrame.WriteUInt32(payload, 16, RecorderRecordId);
            TestFrame.WriteUInt32(payload, 20, 0);
            TestFrame.WriteUInt16(
                payload,
                24,
                (ushort)LMCRecorderState.Armed);
            TestFrame.WriteUInt32(
                payload,
                28,
                RecorderOwnerSessionEpoch);
            TestFrame.WriteUInt32(payload, 32, 0);
            TestFrame.WriteUInt32(payload, 36, DiagnosticsBootId);
            return payload;
        }

        private static byte[] RecorderAdoptPayload(uint requestId)
        {
            var payload = CommonPayload(36, requestId);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 20, RecorderRecordId);
            TestFrame.WriteUInt32(payload, 24, 0);
            TestFrame.WriteUInt32(
                payload,
                28,
                RecorderOwnerSessionEpoch);
            TestFrame.WriteUInt16(
                payload,
                32,
                (ushort)LMCRecorderState.Ready);
            return payload;
        }

        private static byte[] RecorderStatusPayload(uint requestId)
        {
            var payload = CommonPayload(76, requestId);
            TestFrame.WriteUInt32(payload, 16, RecorderRecordId);
            TestFrame.WriteUInt32(payload, 20, 0);
            TestFrame.WriteUInt32(payload, 24, RecorderConfigId);
            TestFrame.WriteUInt32(payload, 28, RecorderConfigRevision);
            TestFrame.WriteUInt32(payload, 32, MapRevision);
            TestFrame.WriteUInt16(
                payload,
                36,
                (ushort)LMCRecorderState.Ready);
            payload[38] = (byte)LMCCapturePhase.InputMapped;
            payload[39] = (byte)LMCRecorderStopReason.SampleCountComplete;
            TestFrame.WriteUInt32(payload, 40, 3);
            TestFrame.WriteUInt32(payload, 44, 3);
            TestFrame.WriteUInt32(payload, 48, uint.MaxValue);
            TestFrame.WriteUInt32(payload, 52, 100);
            TestFrame.WriteUInt32(payload, 56, 102);
            TestFrame.WriteUInt32(
                payload,
                68,
                RecorderOwnerSessionEpoch);
            TestFrame.WriteUInt32(payload, 72, DiagnosticsBootId);
            return payload;
        }

        private static byte[] RecorderAdoptEmptyConfigurationPayload(
            uint requestId)
        {
            var payload = CommonPayload(40, requestId);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 20, RecorderConfigId);
            TestFrame.WriteUInt32(payload, 24, RecorderConfigRevision);
            TestFrame.WriteUInt32(payload, 28, MapRevision);
            TestFrame.WriteUInt32(
                payload,
                32,
                RecorderReconnectedOwnerSessionEpoch);
            TestFrame.WriteUInt16(
                payload,
                36,
                (ushort)LMCRecorderState.Configured);
            payload[38] = (byte)LMCRecorderBufferMode.Double;
            payload[39] = 2;
            return payload;
        }

        private static LMCRecorderBankInventory
            EmptyClosedRecorderBankInventory(uint requestId)
        {
            return LMC_DiagnosticsParser.ParseRecorderBankInventory(
                TestFrame.Response(
                    0,
                    EmptyRecorderBankInventoryPayload(requestId)),
                requestId,
                DiagnosticsBootId,
                RecorderConfigId,
                MapRevision,
                RecorderConfigRevision);
        }

        private static byte[] EmptyRecorderBankInventoryPayload(
            uint requestId)
        {
            var payload = CommonPayload(88, requestId);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 20, RecorderConfigId);
            TestFrame.WriteUInt32(payload, 24, RecorderConfigRevision);
            TestFrame.WriteUInt32(payload, 28, MapRevision);
            TestFrame.WriteUInt32(
                payload,
                32,
                RecorderOwnerSessionEpoch);
            TestFrame.WriteUInt32(
                payload,
                36,
                RecorderOwnerSessionEpoch);
            TestFrame.WriteUInt16(
                payload,
                40,
                (ushort)LMCRecorderState.Configured);
            payload[42] = (byte)LMCRecorderBufferMode.Double;
            payload[43] = 2;
            payload[44] = 0;
            return payload;
        }

        private static byte[] SdoCapabilitiesPayload(uint requestId)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 5);
            TestFrame.WriteUInt32(
                payload,
                20,
                (uint)(LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline));
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 60, 4);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return payload;
        }

        private static byte[] SdoSubmitPayload(
            uint requestId,
            uint ticketId)
        {
            var payload = CommonPayload(32, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCOperationKind.SDORead);
            TestFrame.WriteUInt16(
                payload,
                22,
                (ushort)LMCOperationState.Queued);
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(payload, 28, DiagnosticsBootId);
            return payload;
        }

        private static byte[] CancelOperationPayload(
            uint requestId,
            uint ticketId)
        {
            var payload = CommonPayload(28, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCOperationState.Cancelled);
            TestFrame.WriteUInt16(
                payload,
                22,
                (ushort)LMCOperationOutcome.Cancelled);
            TestFrame.WriteUInt32(payload, 24, DiagnosticsBootId);
            return payload;
        }

        private static byte[] TopologyIoCapabilitiesPayload(
            uint requestId,
            LMCDiagnosticCapability capabilities)
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
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return payload;
        }

        private static byte[] EtherCATNodeHealthPayload(uint requestId)
        {
            var payload = CommonPayload(72, requestId);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt32(payload, 20, TopologyNodeId);
            TestFrame.WriteUInt16(
                payload,
                24,
                (ushort)LMCCapturePhase.InputMapped);
            TestFrame.WriteUInt16(
                payload,
                26,
                (ushort)(LMCEtherCATNodeHealthFlags.Configured
                    | LMCEtherCATNodeHealthFlags.Detected
                    | LMCEtherCATNodeHealthFlags.IdentityMatched
                    | LMCEtherCATNodeHealthFlags.DataValid));
            TestFrame.WriteUInt32(payload, 28, 100);
            TestFrame.WriteUInt64(payload, 32, 0x1122334455667788UL);
            TestFrame.WriteUInt32(payload, 40, 2);
            payload[44] = 1;
            payload[45] = 8;
            TestFrame.WriteUInt32(payload, 48, 7);
            TestFrame.WriteUInt32(payload, 52, 8);
            TestFrame.WriteUInt32(payload, 64, 99);
            TestFrame.WriteUInt32(payload, 68, 90);
            return payload;
        }

        private static byte[] DigitalOutputIoPayload(uint requestId)
        {
            var payload = CommonPayload(56, requestId);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt32(payload, 20, IOReference);
            TestFrame.WriteUInt32(payload, 24, TopologyNodeId);
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

        private static byte[] DigitalOutputSubmitPayload(
            uint requestId,
            uint ticketId)
        {
            var payload = CommonPayload(32, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
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

        private static byte[] CommonPayload(int length, uint requestId)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }
    }
}
