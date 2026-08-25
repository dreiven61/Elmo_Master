using System;
using System.Collections.Generic;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminDs402HomeExRecoveryContractTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Contract.Admin.Ds402HomeEx.RehydrateExactReadOnlyIdentity",
                RehydrateExactReadOnlyIdentity);
            tests.Add(
                "Contract.Admin.Ds402HomeEx.RehydrateFailsClosed",
                RehydrateFailsClosed);
        }

        private static void RehydrateExactReadOnlyIdentity()
        {
            var intent = new LMCAxisDs402HomeExClientIntentId(
                0x01020304u,
                0x11121314u,
                0x21222324u,
                0x31323334u);
            var key = LMCAxisDs402HomeExRecovery.Rehydrate(
                LMCAdmin.ProtocolSchemaVersion,
                0x11223344u,
                0x55667788u,
                0x99AABBCCu,
                0xDDEEFF00u,
                intent,
                2,
                1,
                -12345,
                250,
                500,
                700,
                300,
                9000,
                1000,
                LMCDs402HomeBufferMode.Aborting,
                60000,
                15000,
                new byte[LMCAxisDs402HomeExExecutionPlan.SpareLength]);

            AssertEx.Equal((ushort)2, key.AxisReference);
            AssertEx.Equal(0x11223344u, key.OriginalRequestId);
            AssertEx.Equal(0x55667788u, key.DiagnosticsBuild);
            AssertEx.Equal(0x99AABBCCu, key.DiagnosticsBootId);
            AssertEx.Equal(0xDDEEFF00u, key.MapRevision);
            AssertEx.True(key.ClientIntentId.Equals(intent));
            AssertEx.Equal(1, key.ExecutionPlan.HomingMethod);
            AssertEx.Equal(-12345, key.ExecutionPlan.Position);
            AssertEx.Equal(250, key.ExecutionPlan.DetectionVelocityLimit);
            AssertEx.Equal(500, key.ExecutionPlan.Acceleration);
            AssertEx.Equal(700, key.ExecutionPlan.VelocityHigh);
            AssertEx.Equal(300, key.ExecutionPlan.VelocityLow);
            AssertEx.Equal(9000, key.ExecutionPlan.DistanceLimit);
            AssertEx.Equal(1000, key.ExecutionPlan.TorqueLimit);
            AssertEx.Equal(60000u, key.ExecutionPlan.OverallTimeoutMilliseconds);
            AssertEx.Equal(15000u, key.ExecutionPlan.DetectionTimeoutMilliseconds);
        }

        private static void RehydrateFailsClosed()
        {
            var intent = new LMCAxisDs402HomeExClientIntentId(1, 2, 3, 4);
            var zeroSpare = new byte[LMCAxisDs402HomeExExecutionPlan.SpareLength];

            AssertEx.Throws<NotSupportedException>(() =>
                LMCAxisDs402HomeExRecovery.Rehydrate(
                    1, 1, 2, 3, 4, intent, 1,
                    -1, 0, 1, 1, 1, 1, 1, 1,
                    LMCDs402HomeBufferMode.Aborting,
                    1000, 1000, zeroSpare));

            AssertEx.Throws<NotSupportedException>(() =>
                LMCAxisDs402HomeExRecovery.Rehydrate(
                    1, 1, 2, 3, 4, intent, 1,
                    1, 0, 1, 1, 1, 1, 1, 1,
                    LMCDs402HomeBufferMode.Buffered,
                    1000, 1000, zeroSpare));

            var dirtySpare = new byte[LMCAxisDs402HomeExExecutionPlan.SpareLength];
            dirtySpare[31] = 1;
            AssertEx.Throws<ArgumentException>(() =>
                LMCAxisDs402HomeExRecovery.Rehydrate(
                    1, 1, 2, 3, 4, intent, 1,
                    1, 0, 1, 1, 1, 1, 1, 1,
                    LMCDs402HomeBufferMode.Aborting,
                    1000, 1000, dirtySpare));

            AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                LMCAxisDs402HomeExRecovery.Rehydrate(
                    2, 1, 2, 3, 4, intent, 1,
                    1, 0, 1, 1, 1, 1, 1, 1,
                    LMCDs402HomeBufferMode.Aborting,
                    1000, 1000, zeroSpare));
        }
    }
}
