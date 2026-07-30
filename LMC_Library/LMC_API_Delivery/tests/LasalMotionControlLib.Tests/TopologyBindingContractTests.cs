using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class TopologyBindingContractTests
    {
        private const uint TopologyRevision = 0xA1B2C3D4u;
        private const uint DriveNodeId = 0x00000101u;
        private const uint SlotNodeId = 0x00000201u;
        private const uint IOReference = 0x00000501u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Contract.TopologyBinding.ModelValidators",
                ModelValidators);
            tests.Add(
                "Contract.TopologyBinding.PreWireGuards",
                PreWireGuards);
            tests.Add(
                "Contract.TopologyBinding.ProvenancePreWireGuards",
                ProvenancePreWireGuards);
            tests.Add(
                "Contract.TopologyBinding.RawOverloadsPreserved",
                RawOverloadsPreserved);
            tests.Add(
                "Contract.TopologyBinding.DefaultLiteralCompatibility",
                DefaultLiteralCompatibility);
            tests.Add(
                "Rpc.TopologyBinding.SyncAndAsync",
                BoundFacadeSyncAndAsync);
            tests.Add(
                "Rpc.TopologyBinding.PostWireMismatchRejected",
                PostWireMismatchRejected);
        }

        private static void ModelValidators()
        {
            var topology = CreateTopology();
            LMCEtherCATTopologyEntry entry;

            AssertEx.True(topology.TryGetNode(DriveNodeId, out entry));
            AssertEx.Equal("Drive", entry.Name);
            AssertEx.True(
                topology.TryGetIOReference(IOReference, out entry));
            AssertEx.Equal(SlotNodeId, entry.NodeId);
            AssertEx.False(topology.TryGetIOReference(0, out entry));

            var driveHealth = CreateHealth(
                DriveNodeId,
                LMCEtherCATNodeHealthFlags.Configured
                    | LMCEtherCATNodeHealthFlags.Detected
                    | LMCEtherCATNodeHealthFlags.IdentityMatched
                    | LMCEtherCATNodeHealthFlags.DataValid
                    | LMCEtherCATNodeHealthFlags.Ds402DataPresent,
                0x1234,
                0x5678);
            AssertEx.Equal(
                DriveNodeId,
                topology.ValidateNodeHealth(driveHealth).NodeId);

            var defaultedDrive = CreateHealth(
                DriveNodeId,
                LMCEtherCATNodeHealthFlags.Configured
                    | LMCEtherCATNodeHealthFlags.DataDefaulted,
                0,
                0);
            AssertEx.Equal(
                DriveNodeId,
                topology.ValidateNodeHealth(defaultedDrive).NodeId);

            AssertEx.Throws<InvalidDataException>(() =>
                topology.ValidateNodeHealth(CreateHealth(
                    DriveNodeId,
                    LMCEtherCATNodeHealthFlags.Configured
                        | LMCEtherCATNodeHealthFlags.Detected
                        | LMCEtherCATNodeHealthFlags.IdentityMatched
                        | LMCEtherCATNodeHealthFlags.DataValid,
                    0,
                    0)));
            AssertEx.Throws<InvalidDataException>(() =>
                topology.ValidateNodeHealth(CreateHealth(
                    SlotNodeId,
                    LMCEtherCATNodeHealthFlags.Configured
                        | LMCEtherCATNodeHealthFlags.Detected
                        | LMCEtherCATNodeHealthFlags.IdentityMatched
                        | LMCEtherCATNodeHealthFlags.DataValid
                        | LMCEtherCATNodeHealthFlags.Ds402DataPresent,
                    0x1234,
                    0x5678)));
            AssertEx.Throws<InvalidDataException>(() =>
                topology.ValidateNodeHealth(CreateHealth(
                    0x00009999u,
                    LMCEtherCATNodeHealthFlags.Configured
                        | LMCEtherCATNodeHealthFlags.DataDefaulted,
                    0,
                    0)));
            AssertEx.Throws<InvalidDataException>(() =>
                topology.ValidateNodeHealth(CreateHealth(
                    DriveNodeId,
                    LMCEtherCATNodeHealthFlags.Configured
                        | LMCEtherCATNodeHealthFlags.DataDefaulted,
                    0,
                    0,
                    TopologyRevision + 1)));

            var request = CreateDigitalIOReadRequest();
            AssertEx.Equal(
                SlotNodeId,
                topology.ValidateDigitalIOReadRequest(request).NodeId);
            AssertEx.Throws<InvalidDataException>(() =>
                topology.ValidateDigitalIOReadRequest(
                    new LMCDigitalIOReadRequest(
                        TopologyRevision,
                        IOReference,
                        LMCDigitalIODirection.Output,
                        16)));
            AssertEx.Throws<InvalidDataException>(() =>
                topology.ValidateDigitalIOReadRequest(
                    new LMCDigitalIOReadRequest(
                        TopologyRevision,
                        IOReference,
                        LMCDigitalIODirection.Input,
                        32)));
            AssertEx.Throws<InvalidDataException>(() =>
                topology.ValidateDigitalIOReadRequest(
                    new LMCDigitalIOReadRequest(
                        TopologyRevision,
                        0x00009999u,
                        LMCDigitalIODirection.Output,
                        32)));

            var value = CreateDigitalIOValue(SlotNodeId, TopologyRevision);
            AssertEx.Equal(
                SlotNodeId,
                topology.ValidateDigitalIOValue(value).NodeId);
            AssertEx.Throws<InvalidDataException>(() =>
                topology.ValidateDigitalIOValue(
                    CreateDigitalIOValue(DriveNodeId, TopologyRevision)));
            AssertEx.Throws<InvalidDataException>(() =>
                topology.ValidateDigitalIOValue(
                    CreateDigitalIOValue(
                        SlotNodeId,
                        TopologyRevision + 1)));
            AssertEx.Throws<InvalidDataException>(() =>
                topology.ValidateDigitalIOValue(
                    CreateDigitalIOValue(
                        SlotNodeId,
                        TopologyRevision,
                        LMCDigitalIODirection.Output,
                        16)));
            AssertEx.Throws<InvalidDataException>(() =>
                topology.ValidateDigitalIOValue(
                    CreateDigitalIOValue(
                        SlotNodeId,
                        TopologyRevision,
                        LMCDigitalIODirection.Input,
                        32)));
        }

        private static void PreWireGuards()
        {
            var topology = CreateTopology();
            using (var connection = new LMCConnection())
            {
                AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                    connection.Diagnostics.ReadEtherCATNodeHealth(
                        0x00009999u,
                        topology));
                AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                    connection.Diagnostics.ReadEtherCATNodeHealthAsync(
                            0x00009999u,
                            topology,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Throws<InvalidDataException>(() =>
                    connection.Diagnostics.ReadDigitalIO(
                        topology,
                        new LMCDigitalIOReadRequest(
                            TopologyRevision,
                            IOReference,
                            LMCDigitalIODirection.Output,
                            16)));
                AssertEx.Throws<InvalidDataException>(() =>
                    connection.Diagnostics.ReadDigitalIOAsync(
                            topology,
                            new LMCDigitalIOReadRequest(
                                TopologyRevision,
                                IOReference,
                                LMCDigitalIODirection.Input,
                                32),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
            }
        }

        private static void ProvenancePreWireGuards()
        {
            AssertUnboundTopologyRejected();
            AssertForeignTopologyRejected();
            AssertStaleTopologyRejected();
        }

        private static void AssertUnboundTopologyRejected()
        {
            var topology = CreateTopology();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.EtherCATTopology
                        | LMCDiagnosticCapability.EtherCATNodeHealth
                        | LMCDiagnosticCapability.DigitalIORead),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var capabilities = connection.Diagnostics.GetCapabilities();
                AssertEx.False(topology.BelongsTo(connection));
                AssertEx.False(topology.BelongsToCurrentSession(connection));
                AssertTopologyProvenanceRejected(
                    connection,
                    topology,
                    capabilities);
                AssertEx.Equal(3, server.ReceivedRequests.Count);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertForeignTopologyRejected()
        {
            var topology = CreateTopology();
            using (var ownerServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var foreignServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.EtherCATTopology
                        | LMCDiagnosticCapability.EtherCATNodeHealth
                        | LMCDiagnosticCapability.DigitalIORead),
                CloseStep()))
            using (var ownerConnection = new LMCConnection())
            using (var foreignConnection = new LMCConnection())
            {
                Connect(ownerConnection, ownerServer.Port);
                topology.BindProvenance(
                    ownerConnection.Diagnostics,
                    ownerConnection.SessionGeneration);
                Connect(foreignConnection, foreignServer.Port);
                var capabilities =
                    foreignConnection.Diagnostics.GetCapabilities();

                AssertEx.True(topology.BelongsTo(ownerConnection));
                AssertEx.True(
                    topology.BelongsToCurrentSession(ownerConnection));
                AssertEx.False(topology.BelongsTo(foreignConnection));
                AssertTopologyProvenanceRejected(
                    foreignConnection,
                    topology,
                    capabilities);
                AssertEx.Equal(3, foreignServer.ReceivedRequests.Count);
                AssertEx.Equal(2, ownerServer.ReceivedRequests.Count);

                foreignConnection.CloseConnection();
                ownerConnection.CloseConnection();
                foreignServer.Verify();
                ownerServer.Verify();
            }
        }

        private static void AssertStaleTopologyRejected()
        {
            var topology = CreateTopology();
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, firstServer.Port);
                topology.BindProvenance(
                    connection.Diagnostics,
                    connection.SessionGeneration);
                AssertEx.True(topology.BelongsToCurrentSession(connection));
                connection.CloseConnection();
                AssertEx.False(
                    topology.BelongsToCurrentSession(connection));
                firstServer.Verify();

                using (var secondServer = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    CapabilitiesStep(
                        1,
                        LMCDiagnosticCapability.EtherCATTopology
                            | LMCDiagnosticCapability.EtherCATNodeHealth
                            | LMCDiagnosticCapability.DigitalIORead),
                    CloseStep()))
                {
                    Connect(connection, secondServer.Port);
                    var capabilities =
                        connection.Diagnostics.GetCapabilities();
                    AssertEx.True(topology.BelongsTo(connection));
                    AssertEx.False(
                        topology.BelongsToCurrentSession(connection));
                    AssertTopologyProvenanceRejected(
                        connection,
                        topology,
                        capabilities);
                    AssertEx.Equal(3, secondServer.ReceivedRequests.Count);
                    connection.CloseConnection();
                    secondServer.Verify();
                }
            }
        }

        private static void AssertTopologyProvenanceRejected(
            LMCConnection connection,
            LMCEtherCATTopology topology,
            LMCDiagnosticCapabilities capabilities)
        {
            AssertEx.Throws<InvalidOperationException>(() =>
                connection.Diagnostics.ReadEtherCATNodeHealth(
                    DriveNodeId,
                    topology));
            AssertEx.Throws<InvalidOperationException>(() =>
                connection.Diagnostics.ReadEtherCATNodeHealthAsync(
                        DriveNodeId,
                        topology,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertEx.Throws<InvalidOperationException>(() =>
                connection.Diagnostics.ReadEtherCATNodeHealthAsync(
                        DriveNodeId,
                        topology,
                        capabilities,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());

            AssertEx.Throws<InvalidOperationException>(() =>
                connection.Diagnostics.ReadDigitalIO(
                    topology,
                    CreateDigitalIOReadRequest()));
            AssertEx.Throws<InvalidOperationException>(() =>
                connection.Diagnostics.ReadDigitalIOAsync(
                        topology,
                        CreateDigitalIOReadRequest(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertEx.Throws<InvalidOperationException>(() =>
                connection.Diagnostics.ReadDigitalIOAsync(
                        topology,
                        CreateDigitalIOReadRequest(),
                        capabilities,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
        }

        private static void RawOverloadsPreserved()
        {
            var rawHealth = typeof(LMCDiagnostics).GetMethod(
                "ReadEtherCATNodeHealth",
                new[] { typeof(uint), typeof(uint) });
            var rawHealthAsync = typeof(LMCDiagnostics).GetMethod(
                "ReadEtherCATNodeHealthAsync",
                new[]
                {
                    typeof(uint),
                    typeof(uint),
                    typeof(CancellationToken)
                });
            var rawDigitalIo = typeof(LMCDiagnostics).GetMethod(
                "ReadDigitalIO",
                new[] { typeof(LMCDigitalIOReadRequest) });
            var rawDigitalIoAsync = typeof(LMCDiagnostics).GetMethod(
                "ReadDigitalIOAsync",
                new[]
                {
                    typeof(LMCDigitalIOReadRequest),
                    typeof(CancellationToken)
                });
            AssertEx.True(rawHealth != null);
            AssertEx.True(rawHealthAsync != null);
            AssertEx.True(rawDigitalIo != null);
            AssertEx.True(rawDigitalIoAsync != null);

            AssertBoundOverloadPinsTopologySession(
                typeof(LMCDiagnostics).GetMethod(
                    "ReadEtherCATNodeHealth",
                    new[] { typeof(uint), typeof(LMCEtherCATTopology) }),
                rawHealth);
            AssertBoundOverloadPinsTopologySession(
                typeof(LMCDiagnostics).GetMethod(
                    "ReadEtherCATNodeHealthAsync",
                    new[]
                    {
                        typeof(uint),
                        typeof(LMCEtherCATTopology),
                        typeof(CancellationToken)
                    }),
                rawHealthAsync);
            AssertBoundOverloadPinsTopologySession(
                typeof(LMCDiagnostics).GetMethod(
                    "ReadDigitalIO",
                    new[]
                    {
                        typeof(LMCEtherCATTopology),
                        typeof(LMCDigitalIOReadRequest)
                    }),
                rawDigitalIo);
            AssertBoundOverloadPinsTopologySession(
                typeof(LMCDiagnostics).GetMethod(
                    "ReadDigitalIOAsync",
                    new[]
                    {
                        typeof(LMCEtherCATTopology),
                        typeof(LMCDigitalIOReadRequest),
                        typeof(CancellationToken)
                    }),
                rawDigitalIoAsync);
            AssertSessionPinnedCapabilityHelpers();
        }

        private static void AssertSessionPinnedCapabilityHelpers()
        {
            var publicCapabilities = typeof(LMCDiagnostics).GetMethod(
                "GetCapabilities",
                Type.EmptyTypes);
            var pinnedCapabilities = GetNonPublicDiagnosticsMethod(
                "GetCapabilities",
                new[] { typeof(long) });
            var publicCapabilitiesAsync = typeof(LMCDiagnostics).GetMethod(
                "GetCapabilitiesAsync",
                new[] { typeof(CancellationToken) });
            var pinnedCapabilitiesAsync = GetNonPublicDiagnosticsMethod(
                "GetCapabilitiesAsync",
                new[] { typeof(long), typeof(CancellationToken) });

            AssertCallsPinnedCapabilityHelper(
                typeof(LMCDiagnostics).GetMethod(
                    "GetSignalCatalog",
                    Type.EmptyTypes),
                pinnedCapabilities,
                publicCapabilities,
                "GetSignalCatalog");
            AssertCallsPinnedCapabilityHelper(
                typeof(LMCDiagnostics).GetMethod(
                    "GetSignalCatalogAsync",
                    new[] { typeof(CancellationToken) }),
                pinnedCapabilitiesAsync,
                publicCapabilitiesAsync,
                "GetSignalCatalogAsync");
            AssertCallsPinnedCapabilityHelper(
                typeof(LMCDiagnostics).GetMethod(
                    "GetEtherCATTopology",
                    Type.EmptyTypes),
                pinnedCapabilities,
                publicCapabilities,
                "GetEtherCATTopology");
            AssertCallsPinnedCapabilityHelper(
                typeof(LMCDiagnostics).GetMethod(
                    "GetEtherCATTopologyAsync",
                    new[] { typeof(CancellationToken) }),
                pinnedCapabilitiesAsync,
                publicCapabilitiesAsync,
                "GetEtherCATTopologyAsync");
            AssertCallsPinnedCapabilityHelper(
                GetNonPublicDiagnosticsMethod(
                    "ConfigureBulkCore",
                    new[] { typeof(uint[]), typeof(long), typeof(uint) }),
                pinnedCapabilities,
                publicCapabilities,
                "ConfigureBulkCore");
            AssertCallsPinnedCapabilityHelper(
                GetNonPublicDiagnosticsMethod(
                    "SubmitPIWriteCore",
                    new[] { typeof(LMCPIWriteRequest), typeof(long) }),
                pinnedCapabilities,
                publicCapabilities,
                "SubmitPIWriteCore");
        }

        private static MethodInfo GetNonPublicDiagnosticsMethod(
            string name,
            Type[] parameterTypes)
        {
            return typeof(LMCDiagnostics).GetMethod(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                parameterTypes,
                null);
        }

        private static void AssertCallsPinnedCapabilityHelper(
            MethodInfo caller,
            MethodInfo pinnedHelper,
            MethodInfo recapturingOverload,
            string callerName)
        {
            AssertEx.True(caller != null, callerName + " must exist.");
            AssertEx.True(
                pinnedHelper != null,
                "The session-pinned capability helper must exist.");
            AssertEx.True(
                recapturingOverload != null,
                "The public recapturing capability overload must exist.");

            var executableMethod = GetExecutableMethod(caller);
            AssertEx.True(
                CallsMethod(executableMethod, pinnedHelper),
                callerName
                    + " must call the session-pinned capability helper.");
            AssertEx.False(
                CallsMethod(executableMethod, recapturingOverload),
                callerName
                    + " must not recapture the connection session through the public capability overload.");
        }

        private static void AssertBoundOverloadPinsTopologySession(
            MethodInfo boundMethod,
            MethodInfo rawMethod)
        {
            AssertEx.True(boundMethod != null);
            var executableMethod = GetExecutableMethod(boundMethod);
            var sessionGetter = typeof(LMCEtherCATTopology)
                .GetProperty(
                    "ConnectionSessionGeneration",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .GetGetMethod(true);

            AssertEx.True(
                CallsMethod(executableMethod, sessionGetter),
                "Topology-bound overload must use the topology session generation.");
            AssertEx.False(
                CallsMethod(executableMethod, rawMethod),
                "Topology-bound overload must not delegate to a raw overload that can recapture the session.");
        }

        private static MethodInfo GetExecutableMethod(MethodInfo method)
        {
            var stateMachine = (AsyncStateMachineAttribute)
                Attribute.GetCustomAttribute(
                    method,
                    typeof(AsyncStateMachineAttribute));
            return stateMachine == null
                ? method
                : stateMachine.StateMachineType.GetMethod(
                    "MoveNext",
                    BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic);
        }

        private static bool CallsMethod(
            MethodInfo caller,
            MethodInfo callee)
        {
            if (caller == null || callee == null)
            {
                return false;
            }

            var body = caller.GetMethodBody();
            var bytes = body == null ? null : body.GetILAsByteArray();
            if (bytes == null)
            {
                return false;
            }

            var token = callee.MetadataToken;
            for (var index = 0; index + 4 < bytes.Length; index++)
            {
                if ((bytes[index] == 0x28 || bytes[index] == 0x6F)
                    && BitConverter.ToInt32(bytes, index + 1) == token)
                {
                    return true;
                }
            }

            return false;
        }

        private static void DefaultLiteralCompatibility()
        {
            using (var connection = new LMCConnection())
            {
                AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                    connection.Diagnostics.ReadEtherCATNodeHealth(
                        default,
                        DriveNodeId));
                AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                    connection.Diagnostics.ReadEtherCATNodeHealthAsync(
                            default,
                            DriveNodeId,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
            }
        }

        private static void BoundFacadeSyncAndAsync()
        {
            RunBoundFacade(false);
            RunBoundFacade(true);
        }

        private static void RunBoundFacade(bool useAsync)
        {
            var topology = CreateTopology();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.EtherCATTopology
                        | LMCDiagnosticCapability.EtherCATNodeHealth),
                new FakeRpcStep(
                    LMC_CommandId.ReadEtherCATNodeHealth,
                    TestFrame.Response(0, NodeHealthPayload(2, true))),
                CapabilitiesStep(
                    3,
                    LMCDiagnosticCapability.EtherCATTopology
                        | LMCDiagnosticCapability.DigitalIORead),
                new FakeRpcStep(
                    LMC_CommandId.ReadDigitalIO,
                    TestFrame.Response(
                        0,
                        DigitalIOPayload(4, SlotNodeId))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                topology.BindProvenance(
                    connection.Diagnostics,
                    connection.SessionGeneration);
                var health = useAsync
                    ? connection.Diagnostics.ReadEtherCATNodeHealthAsync(
                            DriveNodeId,
                            topology,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : connection.Diagnostics.ReadEtherCATNodeHealth(
                        DriveNodeId,
                        topology);
                AssertEx.Equal(DriveNodeId, health.NodeId);

                var request = CreateDigitalIOReadRequest();
                var value = useAsync
                    ? connection.Diagnostics.ReadDigitalIOAsync(
                            topology,
                            request,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : connection.Diagnostics.ReadDigitalIO(
                        topology,
                        request);
                AssertEx.Equal(SlotNodeId, value.NodeId);
                AssertEx.Equal(0x11223344UL, value.Value);
                AssertEx.True(value.HasValidatedTopologyBinding);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PostWireMismatchRejected()
        {
            AssertHealthKindMismatchRejected(false);
            AssertHealthKindMismatchRejected(true);
            AssertDigitalIONodeMismatchRejected(false);
            AssertDigitalIONodeMismatchRejected(true);
        }

        private static void AssertHealthKindMismatchRejected(bool useAsync)
        {
            var topology = CreateTopology();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.EtherCATTopology
                        | LMCDiagnosticCapability.EtherCATNodeHealth),
                new FakeRpcStep(
                    LMC_CommandId.ReadEtherCATNodeHealth,
                    TestFrame.Response(0, NodeHealthPayload(2, false))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                topology.BindProvenance(
                    connection.Diagnostics,
                    connection.SessionGeneration);
                AssertEx.Throws<InvalidDataException>(() =>
                {
                    if (useAsync)
                    {
                        connection.Diagnostics.ReadEtherCATNodeHealthAsync(
                                DriveNodeId,
                                topology,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics.ReadEtherCATNodeHealth(
                            DriveNodeId,
                            topology);
                    }
                });
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertDigitalIONodeMismatchRejected(bool useAsync)
        {
            var topology = CreateTopology();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(
                    1,
                    LMCDiagnosticCapability.EtherCATTopology
                        | LMCDiagnosticCapability.DigitalIORead),
                new FakeRpcStep(
                    LMC_CommandId.ReadDigitalIO,
                    TestFrame.Response(
                        0,
                        DigitalIOPayload(2, DriveNodeId))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                topology.BindProvenance(
                    connection.Diagnostics,
                    connection.SessionGeneration);
                AssertEx.Throws<InvalidDataException>(() =>
                {
                    if (useAsync)
                    {
                        connection.Diagnostics.ReadDigitalIOAsync(
                                topology,
                                CreateDigitalIOReadRequest(),
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    else
                    {
                        connection.Diagnostics.ReadDigitalIO(
                            topology,
                            CreateDigitalIOReadRequest());
                    }
                });
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static LMCEtherCATTopology CreateTopology()
        {
            var entries = new List<LMCEtherCATTopologyEntry>
            {
                new LMCEtherCATTopologyEntry(
                    DriveNodeId,
                    0,
                    0,
                    0,
                    LMCEtherCATTopologyNodeKind.EtherCATSlave,
                    LMCEtherCATTopologyNodeFlags.HasMasterSlaveIndex
                        | LMCEtherCATTopologyNodeFlags.SupportsSdo
                        | LMCEtherCATTopologyNodeFlags.PhysicalAxis
                        | LMCEtherCATTopologyNodeFlags.Ds402Drive,
                    1,
                    1,
                    ushort.MaxValue,
                    0x9Au,
                    0x00030924u,
                    1,
                    1,
                    0,
                    0,
                    "Drive",
                    0),
                new LMCEtherCATTopologyEntry(
                    SlotNodeId,
                    DriveNodeId,
                    1,
                    ushort.MaxValue,
                    LMCEtherCATTopologyNodeKind.SlotModule,
                    LMCEtherCATTopologyNodeFlags.HasOutputs
                        | LMCEtherCATTopologyNodeFlags.HasDigitalIO,
                    0,
                    0,
                    0,
                    0x29Du,
                    0x47543242u,
                    1,
                    1,
                    0,
                    4,
                    "OutputSlot",
                    IOReference)
            };
            var info = new LMCEtherCATTopologyInfo(
                null,
                TopologyRevision,
                2,
                96,
                1,
                1,
                1,
                1,
                0x0000000Fu,
                (uint)LMCDiagnosticsCrcKind.Crc32IsoHdlc);
            return new LMCEtherCATTopology(info, entries);
        }

        private static LMCEtherCATNodeHealth CreateHealth(
            uint nodeId,
            LMCEtherCATNodeHealthFlags flags,
            uint ds402StatusWord,
            uint axisError,
            uint topologyRevision = TopologyRevision)
        {
            var detected = (flags
                & LMCEtherCATNodeHealthFlags.Detected) != 0;
            return new LMCEtherCATNodeHealth(
                null,
                topologyRevision,
                nodeId,
                LMCCapturePhase.InputMapped,
                flags,
                100,
                1000,
                2,
                detected,
                detected ? (byte)8 : (byte)0,
                0,
                detected ? 7u : 0u,
                detected ? 8u : 0u,
                ds402StatusWord,
                axisError,
                99,
                90);
        }

        private static LMCDigitalIOReadRequest CreateDigitalIOReadRequest()
        {
            return new LMCDigitalIOReadRequest(
                TopologyRevision,
                IOReference,
                LMCDigitalIODirection.Output,
                32);
        }

        private static LMCDigitalIOValue CreateDigitalIOValue(
            uint nodeId,
            uint topologyRevision,
            LMCDigitalIODirection direction = LMCDigitalIODirection.Output,
            byte bitWidth = 32)
        {
            var validMask = bitWidth == 64
                ? ulong.MaxValue
                : (1UL << bitWidth) - 1UL;
            return new LMCDigitalIOValue(
                null,
                topologyRevision,
                IOReference,
                nodeId,
                direction,
                bitWidth,
                LMCDigitalIOStatusFlags.Valid,
                0x11223344UL & validMask,
                validMask,
                100,
                direction == LMCDigitalIODirection.Output ? 1u : 0u);
        }

        private static byte[] NodeHealthPayload(
            uint requestId,
            bool includeDs402Data)
        {
            var payload = CommonPayload(72, requestId);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt32(payload, 20, DriveNodeId);
            TestFrame.WriteUInt16(
                payload,
                24,
                (ushort)LMCCapturePhase.InputMapped);
            var flags = LMCEtherCATNodeHealthFlags.Configured
                | LMCEtherCATNodeHealthFlags.Detected
                | LMCEtherCATNodeHealthFlags.IdentityMatched
                | LMCEtherCATNodeHealthFlags.DataValid;
            if (includeDs402Data)
            {
                flags |= LMCEtherCATNodeHealthFlags.Ds402DataPresent;
            }

            TestFrame.WriteUInt16(payload, 26, (ushort)flags);
            TestFrame.WriteUInt32(payload, 28, 100);
            TestFrame.WriteUInt64(payload, 32, 1000);
            TestFrame.WriteUInt32(payload, 40, 2);
            payload[44] = 1;
            payload[45] = 8;
            TestFrame.WriteUInt32(payload, 48, 7);
            TestFrame.WriteUInt32(payload, 52, 8);
            if (includeDs402Data)
            {
                TestFrame.WriteUInt32(payload, 56, 0x1234);
                TestFrame.WriteUInt32(payload, 60, 0x5678);
            }

            TestFrame.WriteUInt32(payload, 64, 99);
            TestFrame.WriteUInt32(payload, 68, 90);
            return payload;
        }

        private static byte[] DigitalIOPayload(
            uint requestId,
            uint nodeId)
        {
            var payload = CommonPayload(56, requestId);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt32(payload, 20, IOReference);
            TestFrame.WriteUInt32(payload, 24, nodeId);
            payload[28] = (byte)LMCDigitalIODirection.Output;
            payload[29] = 32;
            TestFrame.WriteUInt16(
                payload,
                30,
                (ushort)LMCDigitalIOStatusFlags.Valid);
            TestFrame.WriteUInt64(payload, 32, 0x11223344UL);
            TestFrame.WriteUInt64(payload, 40, 0xFFFFFFFFUL);
            TestFrame.WriteUInt32(payload, 48, 100);
            TestFrame.WriteUInt32(payload, 52, 1);
            return payload;
        }

        private static FakeRpcStep CapabilitiesStep(
            uint requestId,
            LMCDiagnosticCapability capabilities)
        {
            return new FakeRpcStep(
                LMC_CommandId.GetDiagnosticsCapabilities,
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(requestId, capabilities)));
        }

        private static byte[] CapabilitiesPayload(
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
            return payload;
        }

        private static byte[] CommonPayload(int length, uint requestId)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
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
