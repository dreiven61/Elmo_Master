using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AdminDs402HomeExPublicSurfaceContractTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Contract.Admin.Ds402HomeEx.PublicPrepareRemainsClosed",
                PublicPrepareRemainsClosed);
            tests.Add(
                "Contract.Admin.Ds402HomeEx.RecoveryKeyCannotAuthorizeStart",
                RecoveryKeyCannotAuthorizeStart);
            tests.Add(
                "Contract.Admin.Ds402HomeEx.ApprovedProfileConvertsFrozenPlan",
                ApprovedProfileConvertsFrozenPlan);
            tests.Add(
                "Contract.Admin.Ds402HomeEx.ApprovedProfileRejectsMapRevisionMismatch",
                ApprovedProfileRejectsMapRevisionMismatch);
            tests.Add(
                "Contract.Admin.Ds402HomeEx.ApprovedProfileRejectsUnlistedMethod",
                ApprovedProfileRejectsUnlistedMethod);
            tests.Add(
                "Contract.Admin.Ds402HomeEx.ApprovedProfileRejectsDisabledVendorField",
                ApprovedProfileRejectsDisabledVendorField);
            tests.Add(
                "Contract.Admin.Ds402HomeEx.ApprovedProfileRejectsConvertedOverflow",
                ApprovedProfileRejectsConvertedOverflow);
        }

        private static void PublicPrepareRemainsClosed()
        {
            AssertEx.Equal(
                0,
                typeof(LMCAxisDs402HomeExExecutionPlan)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .Length,
                "The frozen DINT execution plan must not have a public constructor before an approved engineering profile exists.");
            AssertEx.Equal(
                0,
                typeof(LMCPreparedAxisDs402HomeEx)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .Length,
                "Public callers must not manufacture a prepared HomeDS402Ex Start command.");

            var publicAxisMethods = typeof(LMCSingleAxis).GetMethods(
                BindingFlags.Instance | BindingFlags.Public);
            AssertEx.False(
                publicAxisMethods.Any(method =>
                    method.Name.IndexOf(
                        "PrepareDs402HomeEx",
                        StringComparison.Ordinal) >= 0
                    || method.Name.IndexOf(
                        "PrepareLMC_HomeDS402Ex",
                        StringComparison.Ordinal) >= 0),
                "HomeDS402Ex engineering-unit Prepare must remain closed until axis profile/scale approval.");

            var publicAdminMethods = typeof(LMCAdmin).GetMethods(
                BindingFlags.Instance | BindingFlags.Public);
            AssertEx.False(
                publicAdminMethods.Any(method =>
                    method.GetParameters().Any(parameter =>
                        parameter.ParameterType
                            == typeof(LMCAxisDs402HomeExExecutionPlan))),
                "No public Admin method may accept a raw frozen HomeDS402Ex execution plan before profile approval.");
        }

        private static void RecoveryKeyCannotAuthorizeStart()
        {
            var publicAdminStartMethods = typeof(LMCAdmin).GetMethods(
                    BindingFlags.Instance | BindingFlags.Public)
                .Where(method => method.Name == "StartAxisDs402HomeEx")
                .ToArray();
            AssertEx.Equal(1, publicAdminStartMethods.Length);
            var adminParameters = publicAdminStartMethods[0].GetParameters();
            AssertEx.Equal(1, adminParameters.Length);
            AssertEx.Equal(
                typeof(LMCPreparedAxisDs402HomeEx),
                adminParameters[0].ParameterType);

            var publicAxisStartMethods = typeof(LMCSingleAxis).GetMethods(
                    BindingFlags.Instance | BindingFlags.Public)
                .Where(method =>
                    method.Name == "Ds402HomeEx"
                    || method.Name == "LMC_HomeDS402Ex")
                .ToArray();
            AssertEx.Equal(2, publicAxisStartMethods.Length);
            foreach (var method in publicAxisStartMethods)
            {
                var parameters = method.GetParameters();
                AssertEx.Equal(1, parameters.Length);
                AssertEx.Equal(
                    typeof(LMCPreparedAxisDs402HomeEx),
                    parameters[0].ParameterType);
            }

            AssertEx.False(
                typeof(LMCAdmin).GetMethods(
                        BindingFlags.Instance | BindingFlags.Public)
                    .Where(method => method.Name.IndexOf(
                        "Ds402HomeEx",
                        StringComparison.Ordinal) >= 0)
                    .Any(method => method.GetParameters().Any(parameter =>
                        parameter.ParameterType
                            == typeof(LMCAxisDs402HomeExRecoveryKey)
                        && method.Name.IndexOf(
                            "Start",
                            StringComparison.Ordinal) >= 0)),
                "A rehydrated HomeDS402Ex recovery key is read/retire identity only and must never authorize Start replay.");
        }

        private static void ApprovedProfileConvertsFrozenPlan()
        {
            var profile = ApprovedProfile(
                new[] { 1, 2 },
                detectionVelocityLimitEnabled: true,
                torqueLimitEnabled: true);
            var parameters = Parameters(
                homingMethod: 2,
                position: 1.25,
                detectionVelocityLimit: 2.5,
                torqueLimit: 0.5f);

            var plan = profile.CreateExecutionPlan(parameters, 2);

            AssertEx.Equal(2, plan.HomingMethod);
            AssertEx.Equal(1250, plan.Position);
            AssertEx.Equal(250, plan.DetectionVelocityLimit);
            AssertEx.Equal(30, plan.Acceleration);
            AssertEx.Equal(400, plan.VelocityHigh);
            AssertEx.Equal(100, plan.VelocityLow);
            AssertEx.Equal(0, plan.DistanceLimit);
            AssertEx.Equal(50, plan.TorqueLimit);
            AssertEx.Equal((uint)5000, plan.OverallTimeoutMilliseconds);
            AssertEx.Equal((uint)2000, plan.DetectionTimeoutMilliseconds);
        }

        private static void ApprovedProfileRejectsMapRevisionMismatch()
        {
            var profile = ApprovedProfile(
                new[] { 1 },
                detectionVelocityLimitEnabled: true,
                torqueLimitEnabled: true);

            AssertEx.Throws<InvalidOperationException>(() =>
                profile.CreateExecutionPlan(Parameters(), 1));
        }

        private static void ApprovedProfileRejectsUnlistedMethod()
        {
            var profile = ApprovedProfile(
                new[] { 1 },
                detectionVelocityLimitEnabled: true,
                torqueLimitEnabled: true);

            AssertEx.Throws<NotSupportedException>(() =>
                profile.CreateExecutionPlan(
                    Parameters(homingMethod: 2),
                    2));
        }

        private static void ApprovedProfileRejectsDisabledVendorField()
        {
            var profile = ApprovedProfile(
                new[] { 1 },
                detectionVelocityLimitEnabled: false,
                torqueLimitEnabled: false);

            AssertEx.Throws<NotSupportedException>(() =>
                profile.CreateExecutionPlan(
                    Parameters(detectionVelocityLimit: 1.0),
                    2));
            AssertEx.Throws<NotSupportedException>(() =>
                profile.CreateExecutionPlan(
                    Parameters(torqueLimit: 1.0f),
                    2));
        }

        private static void ApprovedProfileRejectsConvertedOverflow()
        {
            var profile = new LMCAxisDs402HomeExApprovedProfile(
                1,
                2,
                new[] { 1 },
                1000.0,
                100.0,
                10.0,
                100.0,
                LMCDs402HomeExApprovedRoundingMode.AwayFromZero,
                -100,
                100,
                true,
                false,
                true);

            AssertEx.Throws<OverflowException>(() =>
                profile.CreateExecutionPlan(
                    Parameters(position: 1.0),
                    2));
        }

        private static LMCAxisDs402HomeExApprovedProfile ApprovedProfile(
            IEnumerable<int> methods,
            bool detectionVelocityLimitEnabled,
            bool torqueLimitEnabled)
        {
            return new LMCAxisDs402HomeExApprovedProfile(
                1,
                2,
                methods,
                1000.0,
                100.0,
                10.0,
                100.0,
                LMCDs402HomeExApprovedRoundingMode.AwayFromZero,
                -2000000000,
                2000000000,
                detectionVelocityLimitEnabled,
                false,
                torqueLimitEnabled);
        }

        private static LMCAxisDs402HomeExParameters Parameters(
            int homingMethod = 1,
            double position = 1.0,
            double detectionVelocityLimit = 0.0,
            float torqueLimit = 0.0f)
        {
            return new LMCAxisDs402HomeExParameters(
                position,
                detectionVelocityLimit,
                3.0f,
                4.0f,
                1.0f,
                0.0f,
                torqueLimit,
                LMCDs402HomeBufferMode.Aborting,
                homingMethod,
                5000,
                2000);
        }
    }
}
