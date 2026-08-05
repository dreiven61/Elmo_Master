using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminContractTests
    {
        private const uint GoldenRequestId = 0x11223344u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add("Request.Admin.GoldenBytes", RequestGoldenBytes);
            tests.Add("Response.Admin.ValidFields", ResponseValidFields);
            tests.Add("Response.Admin.MalformedRejected", MalformedRejected);
            tests.Add("Response.Admin.DomainErrorPreserved", DomainErrorPreserved);
            tests.Add("Rpc.Admin.Axis.Sync", AxisSyncIntegration);
            tests.Add("Rpc.Admin.Group.Async", GroupAsyncIntegration);
            tests.Add(
                "Rpc.Admin.Axis.HandleGenerationPinnedAcrossReconnect",
                AxisHandleGenerationPinnedAcrossReconnect);
            tests.Add(
                "Rpc.Admin.GroupAsync.HandleGenerationPinnedAcrossReconnect",
                GroupAsyncHandleGenerationPinnedAcrossReconnect);
            tests.Add("Rpc.Admin.UnsupportedServer", UnsupportedServer);
            tests.Add("Contract.Admin.FailFastAllowlist", FailFastAllowlist);
        }

        private static void RequestGoldenBytes()
        {
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "00 7D 00 00 08 00 00 00 "
                    + "01 00 00 00 44 33 22 11"),
                LMC_AdminFrame.GetCapabilities(GoldenRequestId));

            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "10 7D 00 00 0C 00 02 00 "
                    + "01 00 00 00 44 33 22 11 "
                    + "05 00 00 00"),
                LMC_AdminFrame.ReadAxisParameter(
                    GoldenRequestId,
                    2,
                    LMCAxisParameterKey.MaxAcceleration));

            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "20 7D 00 00 0C 00 00 01 "
                    + "01 00 00 00 44 33 22 11 "
                    + "05 00 00 00"),
                LMC_AdminFrame.ReadGroupParameters(
                    GoldenRequestId,
                    0x0100,
                    LMCGroupParameterSelection.PathVelocityLimit
                        | LMCGroupParameterSelection.JerkTime));
        }

        private static void ResponseValidFields()
        {
            var capabilities = LMC_AdminParser.ParseCapabilities(
                TestFrame.Response(
                    0,
                    CapabilitiesPayload(GoldenRequestId)),
                GoldenRequestId,
                17);

            AssertEx.True(
                capabilities.Supports(
                    LMCAdminFeature.AxisParameterRead
                    | LMCAdminFeature.GroupParameterRead));
            AssertEx.True(
                capabilities.Supports(
                    LMCAxisParameterKey.ReferencePosition));
            AssertEx.True(
                capabilities.Supports(
                    LMCGroupParameterSelection.All));
            AssertEx.Equal((ushort)4, capabilities.PhysicalAxisCount);
            AssertEx.Equal((ushort)0x0100, capabilities.GroupReference);
            AssertEx.Equal(
                checked((ushort)LMCErrorCatalog.CurrentCatalogVersion),
                capabilities.ErrorCatalogVersion);

            var axis = LMC_AdminParser.ParseAxisParameter(
                TestFrame.Response(
                    0,
                    AxisParameterPayload(
                        GoldenRequestId,
                        LMCAxisParameterKey.MaxVelocity,
                        LMCAdminUnit.ApplicationUnitsPerSecond,
                        123456)),
                GoldenRequestId,
                3,
                LMCAxisParameterKey.MaxVelocity);
            AssertEx.Equal((ushort)3, axis.AxisReference);
            AssertEx.Equal(123456, axis.Value);
            AssertEx.Equal(
                LMCAdminUnit.ApplicationUnitsPerSecond,
                axis.Unit);

            var selection =
                LMCGroupParameterSelection.PathVelocityLimit
                | LMCGroupParameterSelection.JerkTime;
            var group = LMC_AdminParser.ParseGroupParameters(
                TestFrame.Response(
                    0,
                    GroupParametersPayload(
                        GoldenRequestId,
                        selection,
                        100,
                        0,
                        25)),
                GoldenRequestId,
                0x0100,
                selection);

            int value;
            LMCAdminUnit unit;
            AssertEx.True(
                group.TryGetValue(
                    LMCGroupParameterKey.PathVelocityLimit,
                    out value,
                    out unit));
            AssertEx.Equal(100, value);
            AssertEx.Equal(
                LMCAdminUnit.ApplicationUnitsPerSecond,
                unit);
            AssertEx.False(
                group.TryGetValue(
                    LMCGroupParameterKey.PathAccelerationLimit,
                    out value,
                    out unit));
            AssertEx.True(
                group.TryGetValue(
                    LMCGroupParameterKey.JerkTime,
                    out value,
                    out unit));
            AssertEx.Equal(25, value);
            AssertEx.Equal(LMCAdminUnit.Milliseconds, unit);
        }

        private static void MalformedRejected()
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(0, new byte[39]),
                    GoldenRequestId,
                    1));

            var reservedCapability = CapabilitiesPayload(GoldenRequestId);
            TestFrame.WriteUInt16(reservedCapability, 38, 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(0, reservedCapability),
                    GoldenRequestId,
                    1));

            var wrongRequest = CapabilitiesPayload(GoldenRequestId + 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseCapabilities(
                    TestFrame.Response(0, wrongRequest),
                    GoldenRequestId,
                    1));

            var wrongAxisUnit = AxisParameterPayload(
                GoldenRequestId,
                LMCAxisParameterKey.MaxVelocity,
                LMCAdminUnit.ApplicationUnits,
                1);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseAxisParameter(
                    TestFrame.Response(0, wrongAxisUnit),
                    GoldenRequestId,
                    1,
                    LMCAxisParameterKey.MaxVelocity));

            var wrongSelection = GroupParametersPayload(
                GoldenRequestId,
                LMCGroupParameterSelection.All,
                1,
                2,
                3);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseGroupParameters(
                    TestFrame.Response(0, wrongSelection),
                    GoldenRequestId,
                    0x0100,
                    LMCGroupParameterSelection.PathVelocityLimit));
        }

        private static void DomainErrorPreserved()
        {
            var payload = ErrorPayload(
                GoldenRequestId,
                LMCAdminDetailCode.UnsupportedParameter);
            var exception = AssertEx.Throws<LMCAdminCommandException>(
                () => LMC_AdminParser.ParseAxisParameter(
                    TestFrame.Response(0, payload),
                    GoldenRequestId,
                    1,
                    LMCAxisParameterKey.SoftwareMinPosition));

            AssertEx.Equal((short)-31000, exception.Response.ErrorId);
            AssertEx.Equal(
                LMCAdminDetailCode.UnsupportedParameter,
                exception.Response.DetailCode);
            AssertEx.Equal(GoldenRequestId, exception.Response.RequestId);
        }

        private static void AxisSyncIntegration()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7D10,
                    TestFrame.Response(
                        0,
                        AxisParameterPayload(
                            2,
                            LMCAxisParameterKey.ReferencePosition,
                            LMCAdminUnit.ApplicationUnits,
                            -200)))
                {
                    InspectRequest = request =>
                    {
                        AssertEx.Equal((ushort)4, TestFrame.ReadUInt16(request, 6));
                        AssertEx.Equal((ushort)6, TestFrame.ReadUInt16(request, 16));
                    }
                },
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                AssertEx.NotNull(connection.Admin);
                Connect(connection, server.Port);
                var result = connection.Admin.ReadAxisParameter(
                    4,
                    LMCAxisParameterKey.ReferencePosition);
                AssertEx.Equal(-200, result.Value);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void GroupAsyncIntegration()
        {
            var selection =
                LMCGroupParameterSelection.PathAccelerationLimit
                | LMCGroupParameterSelection.JerkTime;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7D20,
                    TestFrame.Response(
                        0,
                        GroupParametersPayload(2, selection, 0, 400, 30)))
                {
                    ResponseChunks = new[] { 1, 3, 5, 7 },
                    InspectRequest = request =>
                    {
                        AssertEx.Equal((ushort)0x0100, TestFrame.ReadUInt16(request, 6));
                        AssertEx.Equal((uint)selection, TestFrame.ReadUInt32(request, 16));
                    }
                },
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var result = connection.Admin.ReadGroupParametersAsync(
                        0x0100,
                        selection,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(400, result.PathAccelerationLimit);
                AssertEx.Equal(30, result.JerkTimeMilliseconds);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AxisHandleGenerationPinnedAcrossReconnect()
        {
            var axisLookupPayload = new byte[6];
            TestFrame.WriteUInt16(axisLookupPayload, 4, 2);

            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x103C,
                    TestFrame.Response(0, axisLookupPayload)),
                new FakeRpcStep(
                    0x202B,
                    TestFrame.Response(
                        0,
                        TestFrame.Hex("02 00 00 00 00 00 00 00"))),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7D10,
                    TestFrame.Response(
                        0,
                        AxisParameterPayload(
                            2,
                            LMCAxisParameterKey.MaxVelocity,
                            LMCAdminUnit.ApplicationUnitsPerSecond,
                            2500))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, firstServer.Port);
                var axis = new LMCSingleAxis(connection, "_LMCAxis2");
                var staleGeneration = axis.SessionGeneration;

                Connect(connection, secondServer.Port);

                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Admin.ReadAxisParameter(
                        axis,
                        LMCAxisParameterKey.MaxVelocity));
                AssertEx.Contains("inactive RPC session", exception.Message);

                var core = typeof(LMCAdmin).GetMethod(
                    "ReadAxisParameterCore",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                AssertEx.NotNull(core);
                var invocation = AssertEx.Throws<TargetInvocationException>(
                    () => core.Invoke(
                        connection.Admin,
                        new object[]
                        {
                            (ushort)2,
                            LMCAxisParameterKey.MaxVelocity,
                            staleGeneration
                        }));
                var coreException = invocation.InnerException
                    as InvalidOperationException;
                AssertEx.NotNull(coreException);
                AssertEx.Contains(
                    "inactive RPC session",
                    coreException.Message);

                var currentSessionResult = connection.Admin.ReadAxisParameter(
                    2,
                    LMCAxisParameterKey.MaxVelocity);
                AssertEx.Equal(2500, currentSessionResult.Value);

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void GroupAsyncHandleGenerationPinnedAcrossReconnect()
        {
            var groupLookupPayload = new byte[6];
            TestFrame.WriteUInt16(groupLookupPayload, 4, 0x0100);
            var selection = LMCGroupParameterSelection.PathAccelerationLimit;

            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x1042,
                    TestFrame.Response(0, groupLookupPayload)),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(0, CapabilitiesPayload(1))),
                new FakeRpcStep(
                    0x7D20,
                    TestFrame.Response(
                        0,
                        GroupParametersPayload(
                            2,
                            selection,
                            0,
                            400,
                            0))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, firstServer.Port);
                var group = new LMCGroupAxis(
                    connection,
                    "_LMCRobotBase1");
                var staleGeneration = group.SessionGeneration;

                Connect(connection, secondServer.Port);

                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => connection.Admin.ReadGroupParametersAsync(
                            group,
                            selection,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Contains("inactive RPC session", exception.Message);

                var core = typeof(LMCAdmin).GetMethod(
                    "ReadGroupParametersCoreAsync",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                AssertEx.NotNull(core);
                var staleCoreTask = (Task<LMCGroupParametersResult>)core.Invoke(
                    connection.Admin,
                    new object[]
                    {
                        (ushort)0x0100,
                        selection,
                        staleGeneration,
                        CancellationToken.None
                    });
                var coreException = AssertEx.Throws<InvalidOperationException>(
                    () => staleCoreTask.GetAwaiter().GetResult());
                AssertEx.Contains(
                    "inactive RPC session",
                    coreException.Message);

                var currentSessionResult = connection.Admin
                    .ReadGroupParametersAsync(
                        0x0100,
                        selection,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(400, currentSessionResult.PathAccelerationLimit);

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void UnsupportedServer()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(
                        1,
                        TestFrame.Hex("01 00 FC FF"))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var exception = AssertEx.Throws<LMCAdminNotSupportedException>(
                    () => connection.Admin.GetCapabilities());
                AssertEx.Equal((short)-4, exception.Acknowledgement.ErrorId);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void FailFastAllowlist()
        {
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_AdminFrame.ReadAxisParameter(
                    GoldenRequestId,
                    5,
                    LMCAxisParameterKey.MaxVelocity));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_AdminFrame.ReadAxisParameter(
                    GoldenRequestId,
                    1,
                    (LMCAxisParameterKey)7));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_AdminFrame.ReadGroupParameters(
                    GoldenRequestId,
                    0x0101,
                    LMCGroupParameterSelection.All));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_AdminFrame.ReadGroupParameters(
                    GoldenRequestId,
                    0x0100,
                    LMCGroupParameterSelection.None));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_AdminFrame.ReadGroupParameters(
                    GoldenRequestId,
                    0x0100,
                    (LMCGroupParameterSelection)8));
        }

        private static byte[] CommonPayload(uint requestId, int length)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static byte[] CapabilitiesPayload(uint requestId)
        {
            var payload = CommonPayload(requestId, 40);
            TestFrame.WriteUInt32(
                payload,
                16,
                (uint)(LMCAdminFeature.AxisParameterRead
                    | LMCAdminFeature.GroupParameterRead));
            TestFrame.WriteUInt32(payload, 20, 0x3F);
            TestFrame.WriteUInt32(
                payload,
                24,
                (uint)LMCGroupParameterSelection.All);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt16(payload, 32, 0x0100);
            TestFrame.WriteUInt16(payload, 34, 3);
            TestFrame.WriteUInt16(
                payload,
                36,
                checked((ushort)LMCErrorCatalog.CurrentCatalogVersion));
            return payload;
        }

        private static byte[] AxisParameterPayload(
            uint requestId,
            LMCAxisParameterKey key,
            LMCAdminUnit unit,
            int value)
        {
            var payload = CommonPayload(requestId, 28);
            TestFrame.WriteUInt16(payload, 16, (ushort)key);
            TestFrame.WriteUInt16(
                payload,
                18,
                (ushort)LMCAdminValueType.Int32);
            TestFrame.WriteUInt16(payload, 20, (ushort)unit);
            TestFrame.WriteInt32(payload, 24, value);
            return payload;
        }

        private static byte[] GroupParametersPayload(
            uint requestId,
            LMCGroupParameterSelection selection,
            int velocity,
            int acceleration,
            int jerkTime)
        {
            var payload = CommonPayload(requestId, 32);
            TestFrame.WriteUInt32(payload, 16, (uint)selection);
            TestFrame.WriteInt32(payload, 20, velocity);
            TestFrame.WriteInt32(payload, 24, acceleration);
            TestFrame.WriteInt32(payload, 28, jerkTime);
            return payload;
        }

        private static byte[] ErrorPayload(
            uint requestId,
            LMCAdminDetailCode detail)
        {
            var payload = CommonPayload(requestId, 16);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -31000);
            TestFrame.WriteUInt32(payload, 12, (uint)detail);
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
