using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminMotionContractTests
    {
        private const uint GoldenRequestId = 0x11223344u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Request.Admin.GroupLinearRelative.GoldenBytes",
                RequestGoldenBytes);
            tests.Add(
                "Contract.Admin.GroupLinearRelative.FailFastValidation",
                FailFastValidation);
            tests.Add(
                "Response.Admin.GroupLinearRelative.StrictEnvelope",
                StrictEnvelope);
            tests.Add(
                "Response.Admin.GroupLinearRelative.NativeRejectPreserved",
                NativeRejectPreserved);
            tests.Add(
                "Response.Admin.ReadParsersRejectMotionDetails",
                ReadParsersRejectMotionDetails);
            tests.Add(
                "Rpc.Admin.GroupLinearRelative.Sync",
                SyncIntegration);
            tests.Add(
                "Rpc.Admin.GroupLinearRelative.GroupFacadeAsync",
                GroupFacadeAsyncIntegration);
            tests.Add(
                "Rpc.Admin.GroupLinearRelative.CapabilityGateNoDispatch",
                CapabilityGateNoDispatch);
            tests.Add(
                "Rpc.Admin.GroupLinearRelative.HandleGenerationPinned",
                HandleGenerationPinnedAcrossReconnect);
            tests.Add(
                "Rpc.Admin.GroupLinearRelative.PreparedSingleDispatchAsync",
                PreparedSingleDispatchAsync);
            tests.Add(
                "Rpc.Admin.GroupLinearRelative.PreparedCapabilityGuards",
                PreparedCapabilityGuards);
            tests.Add(
                "Rpc.Admin.GroupLinearRelative.PreparedGenerationPinned",
                PreparedGenerationPinned);
            tests.Add(
                "Rpc.Admin.GroupLinearRelative.PreparedOwnerPinned",
                PreparedOwnerPinned);
        }

        private static void RequestGoldenBytes()
        {
            var options = new LMCGroupMotionOptions
            {
                TransitionMode =
                    LMC_GROUP_TRANSITION_MODE.ContinuousDirect,
                BufferMode = LMC_BUFFER_MODE.Buffered
            };

            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "22 7D 00 00 68 00 00 01 "
                    + "01 00 00 00 44 33 22 11 "
                    + "01 00 00 00 FE FF FF FF "
                    + "03 00 00 00 FC FF FF FF "
                    + "00 00 00 00 00 00 00 00 "
                    + "00 00 00 00 00 00 00 00 "
                    + "00 00 00 00 00 00 00 00 "
                    + "00 00 00 00 00 00 00 00 "
                    + "00 00 00 00 00 00 00 00 "
                    + "00 00 00 00 00 00 00 00 "
                    + "E8 03 00 00 D0 07 00 00 "
                    + "B8 0B 00 00 A0 0F 00 00 "
                    + "00 00 00 00 02 00 00 00 "
                    + "02 00 00 00 01 00 00 00"),
                LMC_AdminFrame.GroupMoveLinearRelative(
                    GoldenRequestId,
                    0x0100,
                    new[] { 1, -2, 3, -4 },
                    1000,
                    2000,
                    3000,
                    4000,
                    options));
        }

        private static void FailFastValidation()
        {
            var validDistance = new[] { 1, 2, 3, 4 };
            var validOptions = new LMCGroupMotionOptions();

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => CreateRequest(
                    0,
                    0x0100,
                    validDistance,
                    1,
                    1,
                    1,
                    0,
                    validOptions));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => CreateRequest(
                    GoldenRequestId,
                    0x0101,
                    validDistance,
                    1,
                    1,
                    1,
                    0,
                    validOptions));
            AssertEx.Throws<ArgumentNullException>(
                () => CreateRequest(
                    GoldenRequestId,
                    0x0100,
                    null,
                    1,
                    1,
                    1,
                    0,
                    validOptions));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => CreateRequest(
                    GoldenRequestId,
                    0x0100,
                    new int[0],
                    1,
                    1,
                    1,
                    0,
                    validOptions));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => CreateRequest(
                    GoldenRequestId,
                    0x0100,
                    new int[17],
                    1,
                    1,
                    1,
                    0,
                    validOptions));

            var nonzeroReservedSlot = new int[16];
            nonzeroReservedSlot[4] = 1;
            AssertEx.Throws<ArgumentException>(
                () => CreateRequest(
                    GoldenRequestId,
                    0x0100,
                    nonzeroReservedSlot,
                    1,
                    1,
                    1,
                    0,
                    validOptions));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => CreateRequest(
                    GoldenRequestId,
                    0x0100,
                    validDistance,
                    0,
                    1,
                    1,
                    0,
                    validOptions));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => CreateRequest(
                    GoldenRequestId,
                    0x0100,
                    validDistance,
                    1,
                    0,
                    1,
                    0,
                    validOptions));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => CreateRequest(
                    GoldenRequestId,
                    0x0100,
                    validDistance,
                    1,
                    1,
                    0,
                    0,
                    validOptions));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => CreateRequest(
                    GoldenRequestId,
                    0x0100,
                    validDistance,
                    1,
                    1,
                    1,
                    -1,
                    validOptions));
            AssertEx.Throws<ArgumentNullException>(
                () => CreateRequest(
                    GoldenRequestId,
                    0x0100,
                    validDistance,
                    1,
                    1,
                    1,
                    0,
                    null));

            AssertUnsupportedOptions(
                validDistance,
                new LMCGroupMotionOptions
                {
                    CoordinateSystem = LMC_COORD_SYSTEM.Acs
                });
            AssertUnsupportedOptions(
                validDistance,
                new LMCGroupMotionOptions
                {
                    TransitionMode =
                        LMC_GROUP_TRANSITION_MODE.SmoothCubic
                });
            AssertUnsupportedOptions(
                validDistance,
                new LMCGroupMotionOptions
                {
                    BufferMode = LMC_BUFFER_MODE.BlendingHigh
                });
            AssertUnsupportedOptions(
                validDistance,
                new LMCGroupMotionOptions { Execute = false });

            var padded = CreateRequest(
                GoldenRequestId,
                0x0100,
                new[] { 5 },
                1,
                1,
                1,
                0,
                validOptions);
            AssertEx.Equal(112, padded.Length);
            AssertEx.Equal(5, TestFrame.ReadInt32(padded, 16));
            AssertEx.Equal(0, TestFrame.ReadInt32(padded, 20));
            AssertEx.Equal(0, TestFrame.ReadInt32(padded, 76));
        }

        private static void StrictEnvelope()
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseGroupMoveLinearRelative(
                    TestFrame.Response(0, new byte[15]),
                    GoldenRequestId));

            var trailing = SuccessPayload(GoldenRequestId);
            Array.Resize(ref trailing, 17);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseGroupMoveLinearRelative(
                    TestFrame.Response(0, trailing),
                    GoldenRequestId));

            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseGroupMoveLinearRelative(
                    TestFrame.Response(
                        0,
                        SuccessPayload(GoldenRequestId + 1)),
                    GoldenRequestId));

            var zeroNativeError = ErrorPayload(
                GoldenRequestId,
                0,
                LMCAdminDetailCode.NativeCommandRejected);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseGroupMoveLinearRelative(
                    TestFrame.Response(0, zeroNativeError),
                    GoldenRequestId));

            var wrongLocalError = ErrorPayload(
                GoldenRequestId,
                42,
                LMCAdminDetailCode.InvalidMotionParameters);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseGroupMoveLinearRelative(
                    TestFrame.Response(0, wrongLocalError),
                    GoldenRequestId));

            var adminNativeError = ErrorPayload(
                GoldenRequestId,
                -31000,
                LMCAdminDetailCode.NativeCommandRejected);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseGroupMoveLinearRelative(
                    TestFrame.Response(0, adminNativeError),
                    GoldenRequestId));

            var arbitraryNegativeNativeError = ErrorPayload(
                GoldenRequestId,
                -7,
                LMCAdminDetailCode.NativeCommandRejected);
            AssertEx.Throws<InvalidDataException>(
                () => LMC_AdminParser.ParseGroupMoveLinearRelative(
                    TestFrame.Response(0, arbitraryNegativeNativeError),
                    GoldenRequestId));
        }

        private static void NativeRejectPreserved()
        {
            var exception = AssertEx.Throws<LMCAdminCommandException>(
                () => LMC_AdminParser.ParseGroupMoveLinearRelative(
                    TestFrame.Response(
                        0,
                        ErrorPayload(
                            GoldenRequestId,
                            321,
                            LMCAdminDetailCode.NativeCommandRejected)),
                    GoldenRequestId));

            AssertEx.Equal((short)321, exception.Response.ErrorId);
            AssertEx.Equal(
                LMCAdminDetailCode.NativeCommandRejected,
                exception.Response.DetailCode);
            AssertEx.Equal(GoldenRequestId, exception.Response.RequestId);

            var fallback = AssertEx.Throws<LMCAdminCommandException>(
                () => LMC_AdminParser.ParseGroupMoveLinearRelative(
                    TestFrame.Response(
                        0,
                        ErrorPayload(
                            GoldenRequestId,
                            -6,
                            LMCAdminDetailCode.NativeCommandRejected)),
                    GoldenRequestId));
            AssertEx.Equal((short)-6, fallback.Response.ErrorId);
        }

        private static void ReadParsersRejectMotionDetails()
        {
            var details = new[]
            {
                LMCAdminDetailCode.InvalidMotionParameters,
                LMCAdminDetailCode.InvalidState,
                LMCAdminDetailCode.NativeCommandRejected
            };

            for (var index = 0; index < details.Length; index++)
            {
                var detail = details[index];
                var errorId = detail
                    == LMCAdminDetailCode.NativeCommandRejected
                    ? (short)1
                    : (short)-31000;
                var raw = TestFrame.Response(
                    0,
                    ErrorPayload(GoldenRequestId, errorId, detail));

                AssertEx.Throws<InvalidDataException>(
                    () => LMC_AdminParser.ParseCapabilities(
                        raw,
                        GoldenRequestId,
                        1));
                AssertEx.Throws<InvalidDataException>(
                    () => LMC_AdminParser.ParseAxisParameter(
                        raw,
                        GoldenRequestId,
                        1,
                        LMCAxisParameterKey.MaxVelocity));
                AssertEx.Throws<InvalidDataException>(
                    () => LMC_AdminParser.ParseGroupParameters(
                        raw,
                        GoldenRequestId,
                        0x0100,
                        LMCGroupParameterSelection.All));
            }
        }

        private static void SyncIntegration()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCAdminFeature.GroupLinearRelative))),
                new FakeRpcStep(
                    0x7D22,
                    TestFrame.Response(0, SuccessPayload(2)))
                {
                    InspectRequest = request =>
                    {
                        AssertEx.Equal(112, request.Length);
                        AssertEx.Equal(
                            (ushort)104,
                            TestFrame.ReadUInt16(request, 4));
                        AssertEx.Equal(
                            (ushort)0x0100,
                            TestFrame.ReadUInt16(request, 6));
                        AssertEx.Equal(10, TestFrame.ReadInt32(request, 16));
                        AssertEx.Equal(-20, TestFrame.ReadInt32(request, 20));
                        AssertEx.Equal(100, TestFrame.ReadInt32(request, 80));
                        AssertEx.Equal(1, TestFrame.ReadInt32(request, 108));
                    }
                },
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var result = connection.Admin.GroupMoveLinearRelative(
                    0x0100,
                    new[] { 10, -20, 30, -40 },
                    100,
                    200,
                    300,
                    400);
                AssertEx.True(result.IsSuccess);
                AssertEx.Equal((uint)2, result.RequestId);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void GroupFacadeAsyncIntegration()
        {
            var groupLookupPayload = new byte[6];
            TestFrame.WriteUInt16(groupLookupPayload, 4, 0x0100);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x1042,
                    TestFrame.Response(0, groupLookupPayload)),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCAdminFeature.AxisParameterRead
                                | LMCAdminFeature.GroupParameterRead
                                | LMCAdminFeature.GroupLinearRelative))),
                new FakeRpcStep(
                    0x7D22,
                    TestFrame.Response(0, SuccessPayload(2)))
                {
                    ResponseChunks = new[] { 1, 3, 5, 7 },
                    InspectRequest = request =>
                    {
                        AssertEx.Equal(2, TestFrame.ReadInt32(request, 100));
                        AssertEx.Equal(2, TestFrame.ReadInt32(request, 104));
                    }
                },
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var group = new LMCGroupAxis(
                    connection,
                    "_LMCRobotBase1");
                var result = group.MoveLinearRelativeExAsync(
                        new[] { 1, 2, 3, 4 },
                        100,
                        200,
                        300,
                        0,
                        new LMCGroupMotionOptions
                        {
                            TransitionMode =
                                LMC_GROUP_TRANSITION_MODE.ContinuousDirect,
                            BufferMode = LMC_BUFFER_MODE.Buffered
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(result.IsSuccess);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void CapabilityGateNoDispatch()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCAdminFeature.AxisParameterRead
                                | LMCAdminFeature.GroupParameterRead))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var exception = AssertEx.Throws<NotSupportedException>(
                    () => connection.Admin.GroupMoveLinearRelative(
                        0x0100,
                        new[] { 1, 2, 3, 4 },
                        100,
                        200,
                        300,
                        0));
                AssertEx.Contains("does not advertise", exception.Message);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void HandleGenerationPinnedAcrossReconnect()
        {
            var groupLookupPayload = new byte[6];
            TestFrame.WriteUInt16(groupLookupPayload, 4, 0x0100);

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
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCAdminFeature.GroupLinearRelative))),
                new FakeRpcStep(
                    0x7D22,
                    TestFrame.Response(0, SuccessPayload(2))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, firstServer.Port);
                var staleGroup = new LMCGroupAxis(
                    connection,
                    "_LMCRobotBase1");

                Connect(connection, secondServer.Port);

                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => staleGroup.MoveLinearRelativeEx(
                        new[] { 1, 2, 3, 4 },
                        100,
                        200,
                        300,
                        0));
                AssertEx.Contains("inactive RPC session", exception.Message);

                var currentResult = connection.Admin
                    .GroupMoveLinearRelative(
                        0x0100,
                        new[] { 1, 2, 3, 4 },
                        100,
                        200,
                        300,
                        0);
                AssertEx.True(currentResult.IsSuccess);

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void PreparedSingleDispatchAsync()
        {
            var groupLookupPayload = new byte[6];
            TestFrame.WriteUInt16(groupLookupPayload, 4, 0x0100);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x1042,
                    TestFrame.Response(0, groupLookupPayload)),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCAdminFeature.GroupLinearRelative))),
                new FakeRpcStep(
                    0x7D22,
                    TestFrame.Response(0, SuccessPayload(2)))
                {
                    InspectRequest = request =>
                    {
                        AssertEx.Equal((uint)2, TestFrame.ReadUInt32(request, 12));
                        AssertEx.Equal(7, TestFrame.ReadInt32(request, 16));
                    }
                },
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var group = new LMCGroupAxis(
                    connection,
                    "_LMCRobotBase1");
                var verifiedCapabilities = connection.Admin
                    .GetCapabilitiesAsync(CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                var result = group.MoveLinearRelativeExAsync(
                        new[] { 7, 8, 9, 10 },
                        100,
                        200,
                        300,
                        0,
                        new LMCGroupMotionOptions(),
                        verifiedCapabilities,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(result.IsSuccess);
                AssertEx.Equal((uint)2, result.RequestId);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PreparedCapabilityGuards()
        {
            var groupLookupPayload = new byte[6];
            TestFrame.WriteUInt16(groupLookupPayload, 4, 0x0100);

            using (var nullServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x1042,
                    TestFrame.Response(0, groupLookupPayload)),
                CloseStep()))
            using (var nullConnection = new LMCConnection())
            {
                Connect(nullConnection, nullServer.Port);
                var group = new LMCGroupAxis(
                    nullConnection,
                    "_LMCRobotBase1");

                AssertEx.Throws<ArgumentNullException>(
                    () => group.MoveLinearRelativeEx(
                        new[] { 1, 2, 3, 4 },
                        100,
                        200,
                        300,
                        0,
                        new LMCGroupMotionOptions(),
                        null));

                nullConnection.CloseConnection();
                nullServer.Verify();
            }

            using (var featureServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x1042,
                    TestFrame.Response(0, groupLookupPayload)),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCAdminFeature.AxisParameterRead
                                | LMCAdminFeature.GroupParameterRead))),
                CloseStep()))
            using (var featureConnection = new LMCConnection())
            {
                Connect(featureConnection, featureServer.Port);
                var group = new LMCGroupAxis(
                    featureConnection,
                    "_LMCRobotBase1");
                var capabilities = featureConnection.Admin.GetCapabilities();

                AssertEx.Throws<NotSupportedException>(
                    () => group.MoveLinearRelativeEx(
                        new[] { 1, 2, 3, 4 },
                        100,
                        200,
                        300,
                        0,
                        new LMCGroupMotionOptions(),
                        capabilities));

                featureConnection.CloseConnection();
                featureServer.Verify();
            }
        }

        private static void PreparedGenerationPinned()
        {
            var groupLookupPayload = new byte[6];
            TestFrame.WriteUInt16(groupLookupPayload, 4, 0x0100);

            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x1042,
                    TestFrame.Response(0, groupLookupPayload)),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCAdminFeature.GroupLinearRelative))),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x1042,
                    TestFrame.Response(0, groupLookupPayload)),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            2,
                            LMCAdminFeature.GroupLinearRelative))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, firstServer.Port);
                var staleGroup = new LMCGroupAxis(
                    connection,
                    "_LMCRobotBase1");
                var staleCapabilities = connection.Admin.GetCapabilities();

                Connect(connection, secondServer.Port);
                var currentGroup = new LMCGroupAxis(
                    connection,
                    "_LMCRobotBase1");
                var currentCapabilities = connection.Admin.GetCapabilities();

                AssertEx.Throws<InvalidOperationException>(
                    () => staleGroup.MoveLinearRelativeEx(
                        new[] { 1, 2, 3, 4 },
                        100,
                        200,
                        300,
                        0,
                        new LMCGroupMotionOptions(),
                        currentCapabilities));
                AssertEx.Throws<InvalidOperationException>(
                    () => currentGroup.MoveLinearRelativeEx(
                        new[] { 1, 2, 3, 4 },
                        100,
                        200,
                        300,
                        0,
                        new LMCGroupMotionOptions(),
                        staleCapabilities));

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void PreparedOwnerPinned()
        {
            var groupLookupPayload = new byte[6];
            TestFrame.WriteUInt16(groupLookupPayload, 4, 0x0100);

            using (var capabilityServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7D00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            LMCAdminFeature.GroupLinearRelative))),
                CloseStep()))
            using (var groupServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x1042,
                    TestFrame.Response(0, groupLookupPayload)),
                CloseStep()))
            using (var capabilityConnection = new LMCConnection())
            using (var groupConnection = new LMCConnection())
            {
                Connect(capabilityConnection, capabilityServer.Port);
                var foreignCapabilities =
                    capabilityConnection.Admin.GetCapabilities();

                Connect(groupConnection, groupServer.Port);
                var group = new LMCGroupAxis(
                    groupConnection,
                    "_LMCRobotBase1");

                AssertEx.Equal(
                    capabilityConnection.SessionGeneration,
                    groupConnection.SessionGeneration);
                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => group.MoveLinearRelativeEx(
                        new[] { 1, 2, 3, 4 },
                        100,
                        200,
                        300,
                        0,
                        new LMCGroupMotionOptions(),
                        foreignCapabilities));
                AssertEx.Contains("another connection", exception.Message);

                capabilityConnection.CloseConnection();
                groupConnection.CloseConnection();
                capabilityServer.Verify();
                groupServer.Verify();
            }
        }

        private static byte[] CreateRequest(
            uint requestId,
            ushort groupReference,
            int[] distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMCGroupMotionOptions options)
        {
            return LMC_AdminFrame.GroupMoveLinearRelative(
                requestId,
                groupReference,
                distance,
                velocity,
                acceleration,
                deceleration,
                jerk,
                options);
        }

        private static void AssertUnsupportedOptions(
            int[] distance,
            LMCGroupMotionOptions options)
        {
            AssertEx.Throws<NotSupportedException>(
                () => CreateRequest(
                    GoldenRequestId,
                    0x0100,
                    distance,
                    1,
                    1,
                    1,
                    0,
                    options));
        }

        private static byte[] CommonPayload(uint requestId, int length)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static byte[] SuccessPayload(uint requestId)
        {
            return CommonPayload(requestId, 16);
        }

        private static byte[] ErrorPayload(
            uint requestId,
            short errorId,
            LMCAdminDetailCode detail)
        {
            var payload = CommonPayload(requestId, 16);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, errorId);
            TestFrame.WriteUInt32(payload, 12, (uint)detail);
            return payload;
        }

        private static byte[] CapabilitiesPayload(
            uint requestId,
            LMCAdminFeature features)
        {
            var payload = CommonPayload(requestId, 40);
            TestFrame.WriteUInt32(payload, 16, (uint)features);
            TestFrame.WriteUInt32(payload, 20, 0x3F);
            TestFrame.WriteUInt32(
                payload,
                24,
                (uint)LMCGroupParameterSelection.All);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt16(payload, 32, 0x0100);
            TestFrame.WriteUInt16(payload, 34, 3);
            TestFrame.WriteUInt16(payload, 36, 1);
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
