using System;
using System.Collections.Generic;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminDs402HomeCurrentPositionZeroContractTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Contract.Admin.Ds402Home.CurrentPositionZeroOnly",
                CurrentPositionZeroOnly);
            tests.Add(
                "Request.Admin.Ds402Home.CurrentPositionZeroGolden",
                CurrentPositionZeroGolden);
        }

        private static void CurrentPositionZeroOnly()
        {
            AssertEx.Equal(
                37,
                LMCAxisDs402HomeParameters
                    .CurrentPositionZeroHomingMethod);
            AssertEx.Equal(
                0,
                LMCAxisDs402HomeParameters
                    .CurrentPositionZeroHomeOffset);

            var parameters = CurrentPositionZeroParameters();
            AssertEx.Equal(37, parameters.HomingMethod);
            AssertEx.Equal(0, parameters.Position);
            AssertEx.Equal(0, parameters.Velocity);
            AssertEx.Equal(0, parameters.Acceleration);
            AssertEx.Equal(0, parameters.DistanceLimit);
            AssertEx.Equal(0, parameters.TorqueLimit);

            var obsolete = AssertEx.Throws<NotSupportedException>(
                () => Parameters(35, 0));
            AssertEx.Contains("method 35 is obsolete", obsolete.Message);
            AssertEx.Throws<NotSupportedException>(
                () => Parameters(34, 0));
            AssertEx.Throws<NotSupportedException>(
                () => Parameters(37, 1));
            AssertEx.Throws<NotSupportedException>(
                () => Parameters(37, -1));
            AssertEx.Throws<NotSupportedException>(
                () => new LMCAxisDs402HomeParameters(
                    1,
                    0,
                    0,
                    0,
                    LMCDs402HomeBufferMode.Aborting,
                    60000));
            AssertEx.Throws<NotSupportedException>(
                () => new LMCAxisDs402HomeParameters(
                    0,
                    1,
                    0,
                    0,
                    LMCDs402HomeBufferMode.Aborting,
                    60000));
        }

        private static void CurrentPositionZeroGolden()
        {
            var frame = LMC_AdminFrame.StartAxisDs402Home(
                new LMCAxisDs402HomeRecoveryKey(
                    1,
                    0x11223344u,
                    0x55667788u,
                    0x99AABBCCu,
                    0xDDEEFF00u,
                    new LMCAxisDs402HomeClientIntentId(
                        0x01020304u,
                        0x11121314u,
                        0x21222324u,
                        0x31323334u),
                    2,
                    CurrentPositionZeroParameters()));

            AssertEx.Equal((ushort)0x7D15, TestFrame.ReadUInt16(frame, 0));
            AssertEx.Equal((ushort)72, TestFrame.ReadUInt16(frame, 4));
            AssertEx.Equal(37, TestFrame.ReadInt32(frame, 44));
            AssertEx.Equal(0, TestFrame.ReadInt32(frame, 48));
            AssertEx.Equal(0, TestFrame.ReadInt32(frame, 52));
            AssertEx.Equal(0, TestFrame.ReadInt32(frame, 56));
            AssertEx.Equal(0, TestFrame.ReadInt32(frame, 60));
            AssertEx.Equal(0, TestFrame.ReadInt32(frame, 64));
            AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(frame, 68));
            AssertEx.Equal(60000u, TestFrame.ReadUInt32(frame, 72));
        }

        private static LMCAxisDs402HomeParameters
            CurrentPositionZeroParameters()
        {
            return new LMCAxisDs402HomeParameters(60000);
        }

        private static LMCAxisDs402HomeParameters Parameters(
            int homingMethod,
            int position)
        {
            return new LMCAxisDs402HomeParameters(
                homingMethod,
                position,
                0,
                0,
                0,
                0,
                LMCDs402HomeBufferMode.Aborting,
                60000);
        }
    }
}
