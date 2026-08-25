using System;
using System.Collections.Generic;
using System.IO;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AxisDs402HomeExParameterContractTests
    {
        private const uint OriginalRequestId = 0x11223344u;
        private const uint DiagnosticsBuild = 0x55667788u;
        private const uint DiagnosticsBootId = 0x99AABBCCu;
        private const uint MapRevision = 0xDDEEFF00u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Contract.Axis.Ds402HomeEx.EngineeringValuesPreserved",
                EngineeringValuesPreserved);
            tests.Add(
                "Contract.Axis.Ds402HomeEx.MethodPolicy",
                MethodPolicy);
            tests.Add(
                "Contract.Axis.Ds402HomeEx.InvalidInputsRejected",
                InvalidInputsRejected);
            tests.Add(
                "Contract.Axis.Ds402HomeEx.ZeroSpareDefensiveCopy",
                ZeroSpareDefensiveCopy);
            tests.Add(
                "Request.Admin.Ds402HomeEx.GoldenLifecycleOffsets",
                GoldenLifecycleOffsets);
            tests.Add(
                "Contract.Admin.Ds402HomeEx.WireGuards",
                WireGuards);
            tests.Add(
                "Response.Admin.Ds402HomeEx.StartStrictSchema",
                StartStrictSchema);
        }

        private static void EngineeringValuesPreserved()
        {
            var parameters = Parameters();

            AssertEx.Equal(-123.25, parameters.Position);
            AssertEx.Equal(-4.5, parameters.DetectionVelocityLimit);
            AssertEx.Equal(250.5f, parameters.Acceleration);
            AssertEx.Equal(500.25f, parameters.VelocityHigh);
            AssertEx.Equal(25.125f, parameters.VelocityLow);
            AssertEx.Equal(0.0f, parameters.DistanceLimit);
            AssertEx.Equal(0.0f, parameters.TorqueLimit);
            AssertEx.Equal(
                LMCDs402HomeBufferMode.Aborting,
                parameters.BufferMode);
            AssertEx.Equal(1, parameters.HomingMethod);
            AssertEx.Equal(60000u, parameters.TimeLimitMilliseconds);
            AssertEx.Equal(
                5000u,
                parameters.DetectionTimeLimitMilliseconds);
        }

        private static void MethodPolicy()
        {
            foreach (var method in new[]
            {
                1, 14, 17, 30, 33, 34
            })
            {
                AssertEx.Equal(
                    LMCDs402HomeExMethodClassification
                        .StandardCandidate,
                    LMCAxisDs402HomeExParameters
                        .ClassifyHomingMethod(method));
            }

            foreach (var method in new[] { -4, -3, -2, -1 })
            {
                AssertEx.Equal(
                    LMCDs402HomeExMethodClassification
                        .GoldDriveQualificationRequired,
                    LMCAxisDs402HomeExParameters
                        .ClassifyHomingMethod(method));
                AssertEx.Throws<NotSupportedException>(
                    () => Parameters(homingMethod: method));
            }

            foreach (var method in new[] { 15, 16, 31, 32 })
            {
                AssertEx.Equal(
                    LMCDs402HomeExMethodClassification.Reserved,
                    LMCAxisDs402HomeExParameters
                        .ClassifyHomingMethod(method));
                AssertEx.Throws<NotSupportedException>(
                    () => Parameters(homingMethod: method));
            }

            AssertEx.Equal(
                LMCDs402HomeExMethodClassification.Obsolete,
                LMCAxisDs402HomeExParameters.ClassifyHomingMethod(35));
            AssertEx.Equal(
                LMCDs402HomeExMethodClassification.Unsupported,
                LMCAxisDs402HomeExParameters.ClassifyHomingMethod(36));
            AssertEx.Equal(
                LMCDs402HomeExMethodClassification.Unsupported,
                LMCAxisDs402HomeExParameters.ClassifyHomingMethod(37));
            AssertEx.Throws<NotSupportedException>(
                () => Parameters(homingMethod: 35));
            AssertEx.Throws<NotSupportedException>(
                () => Parameters(homingMethod: 36));
            AssertEx.Throws<NotSupportedException>(
                () => Parameters(homingMethod: 37));

            foreach (LMCDs402HomeBufferMode bufferMode in Enum.GetValues(
                typeof(LMCDs402HomeBufferMode)))
            {
                AssertEx.Equal(
                    bufferMode,
                    Parameters(bufferMode: bufferMode).BufferMode);
            }
        }

        private static void InvalidInputsRejected()
        {
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => Parameters(position: double.NaN));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => Parameters(
                    detectionVelocityLimit:
                        double.PositiveInfinity));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => Parameters(acceleration: 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => Parameters(velocityHigh: -1));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => Parameters(velocityLow: float.NaN));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => Parameters(distanceLimit: float.NegativeInfinity));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => Parameters(torqueLimit: -0.01f));
            AssertEx.Throws<NotSupportedException>(
                () => Parameters(distanceLimit: 1.0f));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => Parameters(
                    bufferMode: (LMCDs402HomeBufferMode)0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => Parameters(timeLimitMilliseconds: 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => Parameters(detectionTimeLimitMilliseconds: 0));

            AssertEx.Throws<ArgumentNullException>(
                () => new LMCAxisDs402HomeExParameters(
                    -123.25,
                    -4.5,
                    250.5f,
                    500.25f,
                    25.125f,
                    0.0f,
                    0.0f,
                    LMCDs402HomeBufferMode.Aborting,
                    1,
                    60000,
                    5000,
                    null));
            AssertEx.Throws<ArgumentException>(
                () => Parameters(spare: new byte[31]));
            var nonzeroSpare = new byte[32];
            nonzeroSpare[17] = 1;
            AssertEx.Throws<ArgumentException>(
                () => Parameters(spare: nonzeroSpare));
        }

        private static void ZeroSpareDefensiveCopy()
        {
            var input = new byte[32];
            var parameters = Parameters(spare: input);
            input[0] = 1;

            var first = parameters.Spare;
            AssertEx.Equal(32, first.Length);
            AssertAllZero(first);

            first[1] = 1;
            AssertAllZero(parameters.Spare);
        }

        private static void GoldenLifecycleOffsets()
        {
            var key = RecoveryKey();
            var start = LMC_AdminFrame.StartAxisDs402HomeEx(key);
            AssertEx.Equal(124, start.Length);
            AssertEx.Equal((ushort)0x7D1B, TestFrame.ReadUInt16(start, 0));
            AssertEx.Equal((ushort)116, TestFrame.ReadUInt16(start, 4));
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(start, 6));
            AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(start, 8));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(start, 10));
            AssertEx.Equal(OriginalRequestId, TestFrame.ReadUInt32(start, 12));
            AssertEx.Equal(DiagnosticsBuild, TestFrame.ReadUInt32(start, 16));
            AssertEx.Equal(DiagnosticsBootId, TestFrame.ReadUInt32(start, 20));
            AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(start, 24));
            AssertIntent(start, 28);
            AssertEx.Equal(1, TestFrame.ReadInt32(start, 44));
            AssertPlan(start, 48);
            AssertEx.Equal((ushort)LMCDs402HomeBufferMode.Aborting,
                TestFrame.ReadUInt16(start, 76));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(start, 78));
            AssertEx.Equal(60000u, TestFrame.ReadUInt32(start, 80));
            AssertEx.Equal(5000u, TestFrame.ReadUInt32(start, 84));
            AssertZeroRange(start, 88, 32);
            AssertEx.Equal(0x58453448u, TestFrame.ReadUInt32(start, 120));

            var query = LMC_AdminFrame.ReadAxisDs402HomeExOutcome(
                0xA1B2C3D4u,
                key);
            AssertEx.Equal(124, query.Length);
            AssertEx.Equal((ushort)0x7D1C, TestFrame.ReadUInt16(query, 0));
            AssertEx.Equal((ushort)116, TestFrame.ReadUInt16(query, 4));
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(query, 6));
            AssertEx.Equal(0xA1B2C3D4u, TestFrame.ReadUInt32(query, 12));
            AssertEx.Equal(DiagnosticsBuild, TestFrame.ReadUInt32(query, 16));
            AssertEx.Equal(DiagnosticsBootId, TestFrame.ReadUInt32(query, 20));
            AssertEx.Equal(MapRevision, TestFrame.ReadUInt32(query, 24));
            AssertEx.Equal(OriginalRequestId, TestFrame.ReadUInt32(query, 28));
            AssertIntent(query, 32);
            AssertEx.Equal(1, TestFrame.ReadInt32(query, 48));
            AssertPlan(query, 52);
            AssertEx.Equal((ushort)LMCDs402HomeBufferMode.Aborting,
                TestFrame.ReadUInt16(query, 80));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(query, 82));
            AssertEx.Equal(60000u, TestFrame.ReadUInt32(query, 84));
            AssertEx.Equal(5000u, TestFrame.ReadUInt32(query, 88));
            AssertZeroRange(query, 92, 32);

            var retire = LMC_AdminFrame.RetireAxisDs402HomeExOutcome(
                0x01020304u,
                key,
                7u);
            AssertEx.Equal(128, retire.Length);
            AssertEx.Equal((ushort)0x7D1D, TestFrame.ReadUInt16(retire, 0));
            AssertEx.Equal((ushort)120, TestFrame.ReadUInt16(retire, 4));
            AssertEx.Equal(0x01020304u, TestFrame.ReadUInt32(retire, 12));
            AssertEx.Equal(7u, TestFrame.ReadUInt32(retire, 124));
        }

        private static void WireGuards()
        {
            AssertEx.Throws<NotSupportedException>(() => new LMCAxisDs402HomeExExecutionPlan(
                35, -100, -4, 250, 500, 25, 0, 0,
                LMCDs402HomeBufferMode.Aborting, 60000, 5000, new byte[32]));
            AssertEx.Throws<NotSupportedException>(() => new LMCAxisDs402HomeExExecutionPlan(
                1, -100, -4, 250, 500, 25, 0, 0,
                LMCDs402HomeBufferMode.Buffered, 60000, 5000, new byte[32]));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => new LMCAxisDs402HomeExExecutionPlan(
                1, -100, -4, 250, 500, 25, 0, 0,
                LMCDs402HomeBufferMode.Aborting, 0, 5000, new byte[32]));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => new LMCAxisDs402HomeExExecutionPlan(
                1, -100, -4, 250, 500, 25, 0, 0,
                LMCDs402HomeBufferMode.Aborting, 60000, 0, new byte[32]));
            var nonzeroSpare = new byte[32];
            nonzeroSpare[0] = 1;
            AssertEx.Throws<ArgumentException>(() => new LMCAxisDs402HomeExExecutionPlan(
                1, -100, -4, 250, 500, 25, 0, 0,
                LMCDs402HomeBufferMode.Aborting, 60000, 5000, nonzeroSpare));
            AssertEx.Throws<ArgumentException>(
                () => new LMCAxisDs402HomeExClientIntentId(0, 0, 0, 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                LMC_AdminFrame.ReadAxisDs402HomeExOutcome(0, RecoveryKey()));
            AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                LMC_AdminFrame.RetireAxisDs402HomeExOutcome(
                    1,
                    RecoveryKey(),
                    0));
        }

        private static void StartStrictSchema()
        {
            var parsed = LMC_AdminParser.ParseStartAxisDs402HomeEx(
                TestFrame.Response(
                    0,
                    StartPayload(OriginalRequestId, 0, 0, 1, 0)),
                OriginalRequestId,
                1);
            AssertEx.True(parsed.Response.IsSuccess);
            AssertEx.Equal(1, parsed.HomingMethod);
            AssertEx.Equal(0u, parsed.NativeCommandState);

            AssertStartMalformed(
                StartPayload(OriginalRequestId, 0, 0, 2, 0));
            AssertStartMalformed(
                StartPayload(OriginalRequestId, 0, 0, 1, 1));

            var commonFailure = StartPayload(
                OriginalRequestId,
                1,
                5,
                0,
                0,
                16);
            var commonParsed = LMC_AdminParser.ParseStartAxisDs402HomeEx(
                TestFrame.Response(0, commonFailure),
                OriginalRequestId,
                1);
            AssertEx.False(commonParsed.Response.IsSuccess);

            var profileFailure = StartPayload(
                OriginalRequestId,
                1,
                61,
                1,
                0);
            var profileParsed = LMC_AdminParser.ParseStartAxisDs402HomeEx(
                TestFrame.Response(0, profileFailure),
                OriginalRequestId,
                1);
            AssertEx.False(profileParsed.Response.IsSuccess);

            var queryOnlyFailure = StartPayload(
                OriginalRequestId,
                1,
                53,
                1,
                0);
            AssertStartMalformed(queryOnlyFailure);
        }

        private static void AssertStartMalformed(byte[] payload)
        {
            AssertEx.Throws<InvalidDataException>(() =>
                LMC_AdminParser.ParseStartAxisDs402HomeEx(
                    TestFrame.Response(0, payload),
                    OriginalRequestId,
                    1));
        }

        private static byte[] StartPayload(
            uint requestId,
            ushort commandStatus,
            uint detailCode,
            int homingMethod,
            uint nativeCommandState,
            int length = 24)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt16(payload, 2, 0);
            TestFrame.WriteUInt16(payload, 4, commandStatus);
            TestFrame.WriteInt16(
                payload,
                6,
                commandStatus == 0 ? (short)0 : (short)-31000);
            TestFrame.WriteUInt32(payload, 8, requestId);
            TestFrame.WriteUInt32(payload, 12, detailCode);
            if (length >= 20)
            {
                TestFrame.WriteInt32(payload, 16, homingMethod);
            }
            if (length >= 24)
            {
                TestFrame.WriteUInt32(payload, 20, nativeCommandState);
            }
            return payload;
        }

        private static LMCAxisDs402HomeExRecoveryKey RecoveryKey()
        {
            return new LMCAxisDs402HomeExRecoveryKey(
                1,
                OriginalRequestId,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                new LMCAxisDs402HomeExClientIntentId(
                    0x01234567u,
                    0x89ABCDEFu,
                    0x10203040u,
                    0x50607080u),
                2,
                new LMCAxisDs402HomeExExecutionPlan(
                    1,
                    -100,
                    -4,
                    250,
                    500,
                    25,
                    0,
                    0,
                    LMCDs402HomeBufferMode.Aborting,
                    60000,
                    5000,
                    new byte[32]));
        }

        private static void AssertIntent(byte[] buffer, int offset)
        {
            AssertEx.Equal(0x01234567u, TestFrame.ReadUInt32(buffer, offset));
            AssertEx.Equal(0x89ABCDEFu, TestFrame.ReadUInt32(buffer, offset + 4));
            AssertEx.Equal(0x10203040u, TestFrame.ReadUInt32(buffer, offset + 8));
            AssertEx.Equal(0x50607080u, TestFrame.ReadUInt32(buffer, offset + 12));
        }

        private static void AssertPlan(byte[] buffer, int offset)
        {
            AssertEx.Equal(-100, TestFrame.ReadInt32(buffer, offset));
            AssertEx.Equal(-4, TestFrame.ReadInt32(buffer, offset + 4));
            AssertEx.Equal(250, TestFrame.ReadInt32(buffer, offset + 8));
            AssertEx.Equal(500, TestFrame.ReadInt32(buffer, offset + 12));
            AssertEx.Equal(25, TestFrame.ReadInt32(buffer, offset + 16));
            AssertEx.Equal(0, TestFrame.ReadInt32(buffer, offset + 20));
            AssertEx.Equal(0, TestFrame.ReadInt32(buffer, offset + 24));
        }

        private static void AssertZeroRange(
            byte[] buffer,
            int offset,
            int count)
        {
            for (var index = 0; index < count; index++)
            {
                AssertEx.Equal((byte)0, buffer[offset + index]);
            }
        }

        private static void AssertAllZero(byte[] value)
        {
            for (var index = 0; index < value.Length; index++)
            {
                AssertEx.Equal((byte)0, value[index]);
            }
        }

        private static LMCAxisDs402HomeExParameters Parameters(
            double position = -123.25,
            double detectionVelocityLimit = -4.5,
            float acceleration = 250.5f,
            float velocityHigh = 500.25f,
            float velocityLow = 25.125f,
            float distanceLimit = 0.0f,
            float torqueLimit = 0.0f,
            LMCDs402HomeBufferMode bufferMode =
                LMCDs402HomeBufferMode.Aborting,
            int homingMethod = 1,
            uint timeLimitMilliseconds = 60000,
            uint detectionTimeLimitMilliseconds = 5000,
            byte[] spare = null)
        {
            var effectiveSpare = spare;
            if (effectiveSpare == null)
            {
                effectiveSpare = new byte[32];
            }

            return new LMCAxisDs402HomeExParameters(
                position,
                detectionVelocityLimit,
                acceleration,
                velocityHigh,
                velocityLow,
                distanceLimit,
                torqueLimit,
                bufferMode,
                homingMethod,
                timeLimitMilliseconds,
                detectionTimeLimitMilliseconds,
                effectiveSpare);
        }
    }
}
