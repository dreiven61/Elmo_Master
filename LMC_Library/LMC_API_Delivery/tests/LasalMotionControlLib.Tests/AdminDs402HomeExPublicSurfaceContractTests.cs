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
    }
}
