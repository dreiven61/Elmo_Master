using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class RequestGoldenTests
    {
        private const ushort Reference = 0x1234;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add("Request.RpcLifecycle.GoldenBytes", RpcLifecycleGoldenBytes);
            tests.Add("Request.NameLookup.GoldenBytes", NameLookupGoldenBytes);
            tests.Add("Request.AxisControlAndRead.GoldenBytes", AxisControlAndReadGoldenBytes);
            tests.Add("Request.AxisStop.Validation", AxisStopValidation);
            tests.Add("Request.AxisMotion.GoldenBytes", AxisMotionGoldenBytes);
            tests.Add("Request.GroupControlAndRead.GoldenBytes", GroupControlAndReadGoldenBytes);
            tests.Add("Request.GroupLinear.GoldenBytes", GroupLinearGoldenBytes);
            tests.Add("Request.GroupContract.ValidationMatrix", GroupContractValidationMatrix);
            tests.Add("Request.GroupPositionAndKinematics.GoldenBytes", GroupPositionAndKinematicsGoldenBytes);
            tests.Add("Request.RawDint.IsNotRescaled", RawDintIsNotRescaled);
        }

        private static void RpcLifecycleGoldenBytes()
        {
            AssertEx.SequenceEqual(
                TestFrame.Hex("80 80 00 00 01 00 00 00 00"),
                LMC_Frame.RpcSessionInit());

            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "5C 40 00 00 0C 00 00 00 "
                    + "44 33 22 11 04 03 02 01 7F 00 00 01"),
                LMC_Frame.RpcCallbackRegistration(
                    0x11223344u,
                    0x01020304,
                    new byte[] { 127, 0, 0, 1 }));

            AssertEx.SequenceEqual(
                TestFrame.Hex("5D 40 00 00 01 00 00 00 00"),
                LMC_Frame.CloseConnection());
        }

        private static void NameLookupGoldenBytes()
        {
            AssertEx.SequenceEqual(
                NameLookupRequest(0x103C, "_LMCAxis1"),
                LMC_Frame.LMCAxisGetByName("_LMCAxis1"));

            AssertEx.SequenceEqual(
                NameLookupRequest(0x1042, "_LMCRobotBase1"),
                LMC_Frame.LMCGroupGetByName("_LMCRobotBase1"));

            AssertEx.Throws<ArgumentException>(
                () => LMC_Frame.LMCAxisGetByName(string.Empty));
            AssertEx.Throws<ArgumentException>(
                () => LMC_Frame.LMCAxisGetByName("축1"));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCAxisGetByName(new string('A', 80)));
        }

        private static void AxisControlAndReadGoldenBytes()
        {
            AssertEx.SequenceEqual(
                TestFrame.Request(0x202B, Reference, IntPayload(5, 0, 1)),
                LMC_Frame.LMCAxisInfo(Reference));

            AssertEx.SequenceEqual(
                TestFrame.Request(
                    0x2023,
                    Reference,
                    new byte[] { 1, 0, 0, 0, 1, 1, 0, 1 }),
                LMC_Frame.LMCAxisPower(Reference, true));

            AssertEx.SequenceEqual(
                TestFrame.Request(
                    0x2023,
                    Reference,
                    new byte[] { 1, 0, 0, 0, 0, 1, 0, 1 }),
                LMC_Frame.LMCAxisPower(Reference, false));

            AssertEx.SequenceEqual(
                TestFrame.Request(0x2024, Reference, new byte[] { 1 }),
                LMC_Frame.LMCAxisReset(Reference));

            AssertEx.SequenceEqual(
                TestFrame.Request(0x2028, Reference, IntPayload(Reference, 1)),
                LMC_Frame.LMCAxisReadStatus(Reference));

            AssertEx.SequenceEqual(
                TestFrame.Request(0x202E, Reference, new byte[] { 0 }),
                LMC_Frame.LMCAxisReadPosition(Reference));

            AssertEx.SequenceEqual(
                TestFrame.Request(
                    0x2022,
                    Reference,
                    IntPayload(0x01020304, 2, 1, 1)),
                LMC_Frame.LMCAxisStop(Reference, 0x01020304, 2));
        }

        private static void AxisStopValidation()
        {
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCAxisStop(Reference, 0, 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCAxisStop(Reference, -1, 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCAxisStop(Reference, 1, -1));

            LMC_Frame.LMCAxisStop(Reference, 1, 0);
        }

        private static void AxisMotionGoldenBytes()
        {
            var commonPayload = IntPayload(
                -2,
                0x01020304,
                int.MinValue,
                int.MaxValue,
                -123456789,
                (int)LMC_DIRECTION.Shortest,
                1,
                1);

            AssertEx.SequenceEqual(
                TestFrame.Request(0x209F, Reference, commonPayload),
                LMC_Frame.LMCAxisMoveAbsolute(
                    Reference,
                    -2,
                    0x01020304,
                    int.MinValue,
                    int.MaxValue,
                    -123456789,
                    LMC_DIRECTION.Shortest));

            AssertEx.SequenceEqual(
                TestFrame.Request(0x20A0, Reference, commonPayload),
                LMC_Frame.LMCAxisMoveRelative(
                    Reference,
                    -2,
                    0x01020304,
                    int.MinValue,
                    int.MaxValue,
                    -123456789,
                    LMC_DIRECTION.Shortest));

            AssertEx.SequenceEqual(
                TestFrame.Request(
                    0x20A2,
                    Reference,
                    IntPayload(
                        0x01020304,
                        int.MinValue,
                        0,
                        -123456789,
                        (int)LMC_DIRECTION.Positive,
                        1)),
                LMC_Frame.LMCAxisMoveVelocity(
                    Reference,
                    0x01020304,
                    int.MinValue,
                    0,
                    -123456789,
                    LMC_DIRECTION.Positive));

            AssertEx.SequenceEqual(
                TestFrame.Request(
                    0x20A2,
                    Reference,
                    IntPayload(-123, 1, 0, 0, (int)LMC_DIRECTION.Negative, 1)),
                LMC_Frame.LMCAxisMoveVelocity(
                    Reference,
                    123,
                    1,
                    0,
                    0,
                    LMC_DIRECTION.Negative));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCAxisMoveAbsolute(
                    Reference, 1, 1, 1, 1, 0, LMC_DIRECTION.Negative));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCAxisMoveRelative(
                    Reference, 1, 1, 1, 1, 0, LMC_DIRECTION.Positive));
            AssertEx.Throws<ArgumentException>(
                () => LMC_Frame.LMCAxisMoveVelocity(
                    Reference, 1, 1, 1, 0, LMC_DIRECTION.Positive));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCAxisMoveVelocity(
                    Reference, 1, 1, 0, 0, LMC_DIRECTION.Shortest));
        }

        private static void GroupControlAndReadGoldenBytes()
        {
            AssertEx.SequenceEqual(
                TestFrame.Request(0x20D2, Reference, new byte[] { 1 }),
                LMC_Frame.LMCGroupGetMembersInfo(Reference));

            AssertEx.SequenceEqual(
                TestFrame.Request(0x2047, Reference, new byte[] { 1 }),
                LMC_Frame.LMCGroupEnable(Reference));

            AssertEx.SequenceEqual(
                TestFrame.Request(0x2048, Reference, new byte[] { 1 }),
                LMC_Frame.LMCGroupDisable(Reference));

            AssertEx.SequenceEqual(
                TestFrame.Request(0x204A, Reference, new byte[] { 1 }),
                LMC_Frame.LMCGroupPowerOn(Reference));

            AssertEx.SequenceEqual(
                TestFrame.Request(0x204B, Reference, new byte[] { 1 }),
                LMC_Frame.LMCGroupPowerOff(Reference));

            AssertEx.SequenceEqual(
                TestFrame.Request(0x2049, Reference, new byte[] { 1 }),
                LMC_Frame.LMCGroupReset(Reference));

            AssertEx.SequenceEqual(
                TestFrame.Request(
                    0x2045,
                    Reference,
                    IntPayload(Reference, 1)),
                LMC_Frame.LMCGroupReadStatus(Reference));

            AssertEx.SequenceEqual(
                TestFrame.Request(
                    0x2051,
                    Reference,
                    new byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }),
                LMC_Frame.LMCGroupReadActualPosition(
                    Reference,
                    LMC_COORD_SYSTEM.None));

            AssertEx.SequenceEqual(
                TestFrame.Request(
                    0x2051,
                    Reference,
                    new byte[] { 1, 0, 0, 0, 1, 0, 0, 0 }),
                LMC_Frame.LMCGroupReadActualPosition(
                    Reference,
                    LMC_COORD_SYSTEM.Acs));

            AssertEx.SequenceEqual(
                TestFrame.Request(
                    0x2085,
                    Reference,
                    IntPayload(0x01020304, 2, 1, 1)),
                LMC_Frame.LMCGroupStop(Reference, 0x01020304, 2));
        }

        private static void GroupLinearGoldenBytes()
        {
            var positions = new[] { 1, -2, 0x01020304 };
            var payload = new byte[96];

            TestFrame.WriteInt32(payload, 0, 1);
            TestFrame.WriteInt32(payload, 4, -2);
            TestFrame.WriteInt32(payload, 8, 0x01020304);
            TestFrame.WriteInt32(payload, 64, 10);
            TestFrame.WriteInt32(payload, 68, 20);
            TestFrame.WriteInt32(payload, 72, 30);
            TestFrame.WriteInt32(payload, 76, 40);
            TestFrame.WriteInt32(payload, 80, 0);
            TestFrame.WriteInt32(payload, 84, 0);
            TestFrame.WriteInt32(payload, 88, 1);
            TestFrame.WriteInt32(payload, 92, 1);

            AssertEx.SequenceEqual(
                TestFrame.Request(0x20A4, Reference, payload),
                LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference,
                    positions,
                    10,
                    20,
                    30,
                    40));

            var options = new LMCGroupMotionOptions
            {
                CoordinateSystem = LMC_COORD_SYSTEM.None,
                TransitionMode = LMC_GROUP_TRANSITION_MODE.ContinuousDirect,
                BufferMode = LMC_BUFFER_MODE.Buffered,
                Execute = true
            };

            TestFrame.WriteInt32(payload, 80, 0);
            TestFrame.WriteInt32(payload, 84, 2);
            TestFrame.WriteInt32(payload, 88, 2);
            TestFrame.WriteInt32(payload, 92, 1);

            AssertEx.SequenceEqual(
                TestFrame.Request(0x20A4, Reference, payload),
                LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference,
                    positions,
                    10,
                    20,
                    30,
                    40,
                    options));

        }

        private static void GroupContractValidationMatrix()
        {
            AssertEx.Throws<NotSupportedException>(
                () => LMC_Frame.LMCGroupReadActualPosition(
                    Reference,
                    LMC_COORD_SYSTEM.Mcs));
            AssertEx.Throws<NotSupportedException>(
                () => LMC_Frame.LMCGroupReadActualPosition(
                    Reference,
                    LMC_COORD_SYSTEM.Pcs));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupReadActualPosition(
                    Reference,
                    (LMC_COORD_SYSTEM)4));

            AssertEx.Throws<ArgumentNullException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference, null, 1, 1, 1, 1));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference, new int[0], 1, 1, 1, 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference, new int[17], 1, 1, 1, 1));
            AssertEx.Throws<ArgumentException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference, new[] { 1, 2, 3, 4, 5 }, 1, 1, 1, 0));

            var sixteenSlots = new int[16];
            sixteenSlots[0] = 1;
            sixteenSlots[1] = 2;
            sixteenSlots[2] = 3;
            sixteenSlots[3] = 4;
            LMC_Frame.LMCGroupMoveLinearAbsolute(
                Reference, sixteenSlots, 1, 1, 1, 0);

            sixteenSlots[15] = 1;
            AssertEx.Throws<ArgumentException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference, sixteenSlots, 1, 1, 1, 0));

            AssertEx.Throws<ArgumentNullException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference,
                    new[] { 1 },
                    1,
                    1,
                    1,
                    0,
                    null));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference, new[] { 1 }, 0, 1, 1, 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference, new[] { 1 }, 1, 0, 1, 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference, new[] { 1 }, 1, 1, 0, 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference, new[] { 1 }, 1, 1, 1, -1));

            foreach (var coordinateSystem in new[]
            {
                LMC_COORD_SYSTEM.Acs,
                LMC_COORD_SYSTEM.Mcs,
                LMC_COORD_SYSTEM.Pcs
            })
            {
                AssertEx.Throws<NotSupportedException>(
                    () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                        Reference,
                        new[] { 1 },
                        1,
                        1,
                        1,
                        0,
                        new LMCGroupMotionOptions
                        {
                            CoordinateSystem = coordinateSystem
                        }));
            }

            foreach (var transitionMode in new[]
            {
                LMC_GROUP_TRANSITION_MODE.SmoothParabolic,
                LMC_GROUP_TRANSITION_MODE.SmoothCubic,
                LMC_GROUP_TRANSITION_MODE.SmoothQuintic
            })
            {
                AssertEx.Throws<NotSupportedException>(
                    () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                        Reference,
                        new[] { 1 },
                        1,
                        1,
                        1,
                        0,
                        new LMCGroupMotionOptions
                        {
                            TransitionMode = transitionMode
                        }));
            }

            foreach (var bufferMode in new[]
            {
                LMC_BUFFER_MODE.BlendingLow,
                LMC_BUFFER_MODE.BlendingPrevious,
                LMC_BUFFER_MODE.BlendingNext,
                LMC_BUFFER_MODE.BlendingHigh
            })
            {
                AssertEx.Throws<NotSupportedException>(
                    () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                        Reference,
                        new[] { 1 },
                        1,
                        1,
                        1,
                        0,
                        new LMCGroupMotionOptions
                        {
                            BufferMode = bufferMode
                        }));
            }

            AssertEx.Throws<NotSupportedException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference,
                    new[] { 1 },
                    1,
                    1,
                    1,
                    0,
                    new LMCGroupMotionOptions { Execute = false }));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference,
                    new[] { 1 },
                    1,
                    1,
                    1,
                    1,
                    new LMCGroupMotionOptions
                    {
                        TransitionMode = (LMC_GROUP_TRANSITION_MODE)1
                    }));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference,
                    new[] { 1 },
                    1,
                    1,
                    1,
                    0,
                    new LMCGroupMotionOptions
                    {
                        CoordinateSystem = (LMC_COORD_SYSTEM)4
                    }));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupMoveLinearAbsolute(
                    Reference,
                    new[] { 1 },
                    1,
                    1,
                    1,
                    0,
                    new LMCGroupMotionOptions
                    {
                        BufferMode = (LMC_BUFFER_MODE)7
                    }));

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupStop(Reference, -1, 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupStop(Reference, 1, -1));
            AssertEx.Throws<ArgumentException>(
                () => LMC_Frame.LMCGroupStop(Reference, 0, 1));
            LMC_Frame.LMCGroupStop(Reference, 0, 0);
        }

        private static void GroupPositionAndKinematicsGoldenBytes()
        {
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => LMC_Frame.LMCGroupReadActualPosition(
                    Reference,
                    (LMC_COORD_SYSTEM)4));

            var payload = new byte[1320];
            for (var index = 0; index < 4; index++)
            {
                var nodeOffset = index * 40;
                TestFrame.WriteDouble(payload, nodeOffset, 1.0);
                TestFrame.WriteDouble(payload, nodeOffset + 8, 1.0);
                TestFrame.WriteDouble(payload, nodeOffset + 16, 0.0);
                TestFrame.WriteInt32(payload, nodeOffset + 24, 1);
                TestFrame.WriteUInt32(payload, nodeOffset + 28, (uint)index);
                TestFrame.WriteInt32(payload, nodeOffset + 32, index);
            }

            TestFrame.WriteInt32(payload, 640, 4);
            TestFrame.WriteInt32(payload, 1304, 0);
            TestFrame.WriteInt32(payload, 1308, 2);
            payload[1312] = 1;

            var expected = TestFrame.Request(0x20E7, 0x0100, payload);
            var transform = LMCCartesianKinematicTransform.CreateFourAxis(0, 1, 2, 3);
            var actual = LMC_Frame.LMCGroupSetKinTransformCartesian(
                0x0100,
                transform);

            AssertEx.SequenceEqual(expected, actual);

            using (var sha256 = SHA256.Create())
            {
                AssertEx.SequenceEqual(
                    TestFrame.Hex(
                        "67 8D 48 44 A8 81 E6 97 8F 83 DA DB CF 7E 27 A9 "
                        + "2B 19 AC 94 0A 99 73 24 1C 70 13 95 6B 2D 34 CF"),
                    sha256.ComputeHash(actual),
                    "0x20E7 frame differs from the captured golden frame.");
            }

            AssertEx.Throws<ArgumentException>(
                () => LMC_Frame.LMCGroupSetKinTransformCartesian(
                    Reference,
                    new LMCCartesianKinematicTransform(
                        new[]
                        {
                            LMCKinematicNode.CreateIdentityShift(
                                1,
                                LMC_KINEMATIC_AXIS_TYPE.X)
                        })));
        }

        private static void RawDintIsNotRescaled()
        {
            var callerConvertedPosition = checked(90 * LMC_Units.DEG);
            var request = LMC_Frame.LMCAxisMoveAbsolute(
                Reference,
                callerConvertedPosition,
                LMC_Units.DEG,
                -1,
                int.MinValue,
                int.MaxValue,
                LMC_DIRECTION.Shortest);

            AssertEx.SequenceEqual(
                TestFrame.Hex("A0 BB 0D 00"),
                Slice(request, 8, 4),
                "Caller-provided position DINT was rescaled.");
            AssertEx.SequenceEqual(
                TestFrame.Hex("10 27 00 00"),
                Slice(request, 12, 4),
                "Caller-provided velocity DINT was rescaled.");
            AssertEx.SequenceEqual(
                TestFrame.Hex("FF FF FF FF"),
                Slice(request, 16, 4),
                "Signed DINT was not serialized verbatim.");
            AssertEx.SequenceEqual(
                TestFrame.Hex("00 00 00 80"),
                Slice(request, 20, 4),
                "DINT minimum was not serialized verbatim.");
            AssertEx.SequenceEqual(
                TestFrame.Hex("FF FF FF 7F"),
                Slice(request, 24, 4),
                "DINT maximum was not serialized verbatim.");
        }

        private static byte[] NameLookupRequest(ushort command, string name)
        {
            var payload = new byte[80];
            var bytes = Encoding.ASCII.GetBytes(name);

            Buffer.BlockCopy(bytes, 0, payload, 0, bytes.Length);
            return TestFrame.Request(command, 0, payload);
        }

        private static byte[] IntPayload(params int[] values)
        {
            var payload = new byte[values.Length * 4];

            for (var index = 0; index < values.Length; index++)
            {
                TestFrame.WriteInt32(payload, index * 4, values[index]);
            }

            return payload;
        }

        private static byte[] Slice(byte[] value, int offset, int count)
        {
            var result = new byte[count];
            Buffer.BlockCopy(value, offset, result, 0, count);
            return result;
        }
    }
}
