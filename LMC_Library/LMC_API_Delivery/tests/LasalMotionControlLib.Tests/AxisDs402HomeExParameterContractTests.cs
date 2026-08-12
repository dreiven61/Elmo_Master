using System;
using System.Collections.Generic;
using System.Reflection;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AxisDs402HomeExParameterContractTests
    {
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
                "Contract.Axis.Ds402HomeEx.RemainsWireDormant",
                RemainsWireDormant);
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

        private static void RemainsWireDormant()
        {
            foreach (var featureName in Enum.GetNames(
                typeof(LMCAdminFeature)))
            {
                AssertEx.True(
                    featureName.IndexOf(
                        "Ds402HomeEx",
                        StringComparison.OrdinalIgnoreCase) < 0);
            }

            foreach (var field in typeof(LMC_CommandId).GetFields(
                BindingFlags.Static
                    | BindingFlags.Public
                    | BindingFlags.NonPublic))
            {
                AssertEx.True(
                    field.Name.IndexOf(
                        "Ds402HomeEx",
                        StringComparison.OrdinalIgnoreCase) < 0);
            }

            foreach (var method in typeof(LMC_AdminFrame).GetMethods(
                BindingFlags.Static
                    | BindingFlags.Public
                    | BindingFlags.NonPublic))
            {
                AssertEx.True(
                    method.Name.IndexOf(
                        "Ds402HomeEx",
                        StringComparison.OrdinalIgnoreCase) < 0);
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
