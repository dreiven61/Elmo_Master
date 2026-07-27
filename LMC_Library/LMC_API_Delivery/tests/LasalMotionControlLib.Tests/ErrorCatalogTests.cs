using System;
using System.Collections.Generic;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class ErrorCatalogTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "ErrorCatalog.Adapter.KnownAndUnknown",
                AdapterKnownAndUnknown);
            tests.Add(
                "ErrorCatalog.Domain.NumericCollision",
                DomainNumericCollision);
            tests.Add(
                "ErrorCatalog.Diagnostics.AllEnumValues",
                DiagnosticsAllEnumValues);
            tests.Add(
                "ErrorCatalog.Admin.AllEnumValues",
                AdminAllEnumValues);
            tests.Add(
                "ErrorCatalog.GroupProfile.CurrentEnumCoverage",
                GroupProfileCurrentEnumCoverage);
            tests.Add(
                "ErrorCatalog.InvalidDomain.FailClosed",
                InvalidDomainFailClosed);
        }

        private static void AdapterKnownAndUnknown()
        {
            LMCErrorDescription description;

            for (long code = -8; code <= -1; code++)
            {
                AssertEx.True(
                    LMCErrorCatalog.TryDescribe(
                        LMCErrorDomain.AdapterCommand,
                        code,
                        out description),
                    "Missing adapter catalog entry for " + code + ".");
                AssertEx.Equal(code, description.Code);
                AssertEx.Equal(
                    LMCErrorCatalog.AdapterSourceVersion,
                    description.SourceVersion);
            }

            AssertEx.True(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.AdapterCommand,
                    -1,
                    out description));
            AssertDescription(
                description,
                LMCErrorDomain.AdapterCommand,
                -1,
                "RpcSessionStateInvalid",
                LMCErrorCatalog.AdapterSourceVersion);

            AssertEx.True(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.AdapterCommand,
                    -8,
                    out description));
            AssertDescription(
                description,
                LMCErrorDomain.AdapterCommand,
                -8,
                "QueueOrFramingError",
                LMCErrorCatalog.AdapterSourceVersion);

            AssertEx.False(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.AdapterCommand,
                    7,
                    out description));
            AssertEx.Equal<LMCErrorDescription>(null, description);
        }

        private static void DomainNumericCollision()
        {
            LMCErrorDescription admin;
            LMCErrorDescription diagnostics;
            LMCErrorDescription profile;

            AssertEx.True(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.AdminDetail,
                    7,
                    out admin));
            AssertEx.True(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.DiagnosticsDetail,
                    7,
                    out diagnostics));
            AssertEx.True(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.GroupProfile,
                    7,
                    out profile));

            AssertEx.Equal("MissingClient", admin.Symbol);
            AssertEx.Equal("WriteDenied", diagnostics.Symbol);
            AssertEx.Equal("_LMCPROF_SWE_ERROR", profile.Symbol);
            AssertEx.False(
                string.Equals(
                    admin.Description,
                    diagnostics.Description,
                    StringComparison.Ordinal));
            AssertEx.False(
                string.Equals(
                    diagnostics.Description,
                    profile.Description,
                    StringComparison.Ordinal));
        }

        private static void DiagnosticsAllEnumValues()
        {
            foreach (LMCDiagnosticsDetailCode code in Enum.GetValues(
                typeof(LMCDiagnosticsDetailCode)))
            {
                LMCErrorDescription description;
                AssertEx.True(
                    LMCErrorCatalog.TryDescribe(
                        LMCErrorDomain.DiagnosticsDetail,
                        (long)code,
                        out description),
                    "Missing diagnostics catalog entry for " + code + ".");
                AssertDescription(
                    description,
                    LMCErrorDomain.DiagnosticsDetail,
                    (long)code,
                    code.ToString(),
                    LMCErrorCatalog.DiagnosticsSourceVersion);
            }

            LMCErrorDescription unknown;
            AssertEx.False(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.DiagnosticsDetail,
                    32,
                    out unknown));
            AssertEx.Equal<LMCErrorDescription>(null, unknown);
        }

        private static void AdminAllEnumValues()
        {
            foreach (LMCAdminDetailCode code in Enum.GetValues(
                typeof(LMCAdminDetailCode)))
            {
                LMCErrorDescription description;
                AssertEx.True(
                    LMCErrorCatalog.TryDescribe(
                        LMCErrorDomain.AdminDetail,
                        (long)code,
                        out description),
                    "Missing admin catalog entry for " + code + ".");
                AssertDescription(
                    description,
                    LMCErrorDomain.AdminDetail,
                    (long)code,
                    code.ToString(),
                    LMCErrorCatalog.AdminSourceVersion);
            }

            LMCErrorDescription unknown;
            AssertEx.False(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.AdminDetail,
                    12,
                    out unknown));
            AssertEx.Equal<LMCErrorDescription>(null, unknown);
        }

        private static void GroupProfileCurrentEnumCoverage()
        {
            for (long code = 0; code <= 59; code++)
            {
                AssertGroupProfileCode(code);
            }

            for (long code = 1000; code <= 1013; code++)
            {
                AssertGroupProfileCode(code);
            }

            LMCErrorDescription description;
            AssertEx.True(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.GroupProfile,
                    25,
                    out description));
            AssertEx.Equal("_LMCPROF_ARCLEN_ERROR", description.Symbol);

            AssertEx.True(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.GroupProfile,
                    26,
                    out description));
            AssertEx.Equal("_LMCPROF_RES_PATHLEN_ERROR", description.Symbol);

            AssertEx.False(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.GroupProfile,
                    60,
                    out description));
            AssertEx.Equal<LMCErrorDescription>(null, description);

            AssertEx.False(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.GroupProfile,
                    999,
                    out description));
            AssertEx.Equal<LMCErrorDescription>(null, description);

            AssertEx.False(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.GroupProfile,
                    1014,
                    out description));
            AssertEx.Equal<LMCErrorDescription>(null, description);
        }

        private static void InvalidDomainFailClosed()
        {
            LMCErrorDescription description;
            AssertEx.False(
                LMCErrorCatalog.TryDescribe(
                    (LMCErrorDomain)0,
                    -1,
                    out description));
            AssertEx.Equal<LMCErrorDescription>(null, description);

            AssertEx.False(
                LMCErrorCatalog.TryDescribe(
                    (LMCErrorDomain)99,
                    -1,
                    out description));
            AssertEx.Equal<LMCErrorDescription>(null, description);
        }

        private static void AssertGroupProfileCode(long code)
        {
            LMCErrorDescription description;
            AssertEx.True(
                LMCErrorCatalog.TryDescribe(
                    LMCErrorDomain.GroupProfile,
                    code,
                    out description),
                "Missing group profile catalog entry for " + code + ".");
            AssertEx.Equal(
                LMCErrorCatalog.GroupProfileSourceVersion,
                description.SourceVersion);
            AssertEx.Equal(
                LMCErrorCatalog.CurrentCatalogVersion,
                description.CatalogVersion);
            AssertEx.True(!string.IsNullOrWhiteSpace(description.Symbol));
            AssertEx.True(!string.IsNullOrWhiteSpace(description.Description));
            AssertEx.True(!string.IsNullOrWhiteSpace(description.Resolution));
        }

        private static void AssertDescription(
            LMCErrorDescription description,
            LMCErrorDomain domain,
            long code,
            string symbol,
            string sourceVersion)
        {
            AssertEx.NotNull(description);
            AssertEx.Equal(domain, description.Domain);
            AssertEx.Equal(code, description.Code);
            AssertEx.Equal(symbol, description.Symbol);
            AssertEx.Equal(
                LMCErrorCatalog.CurrentCatalogVersion,
                description.CatalogVersion);
            AssertEx.Equal(sourceVersion, description.SourceVersion);
            AssertEx.True(!string.IsNullOrWhiteSpace(description.Description));
            AssertEx.True(!string.IsNullOrWhiteSpace(description.Resolution));
        }
    }
}
