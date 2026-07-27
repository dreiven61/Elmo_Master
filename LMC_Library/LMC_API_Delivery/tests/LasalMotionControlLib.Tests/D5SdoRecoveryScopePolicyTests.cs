using System;
using System.Collections.Generic;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class D5SdoRecoveryScopePolicyTests
    {
        private const uint BootId1 = 0x12345678u;
        private const uint BootId2 = 0x89ABCDEFu;
        private const uint MapRevision1 = 0x10203040u;
        private const uint MapRevision2 = 0x50607080u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.D5RecoveryScope.SameOwnerExact",
                SameOwnerExact);
            tests.Add(
                "Qualification.D5RecoveryScope.SameOwnerNewIdentity",
                SameOwnerNewIdentity);
            tests.Add(
                "Qualification.D5RecoveryScope.HomogeneousPreviousOwner",
                HomogeneousPreviousOwner);
            tests.Add(
                "Qualification.D5RecoveryScope.CurrentAndForeignOwnersMixed",
                CurrentAndForeignOwnersMixed);
            tests.Add(
                "Qualification.D5RecoveryScope.MultiplePreviousOwnersMixed",
                MultiplePreviousOwnersMixed);
            tests.Add(
                "Qualification.D5RecoveryScope.SubmissionIdentityMixed",
                SubmissionIdentityMixed);
            tests.Add(
                "Qualification.D5RecoveryScope.WriteEvidenceFailsClosed",
                WriteEvidenceFailsClosed);
            tests.Add(
                "Qualification.D5RecoveryScope.ValidationFailsClosed",
                ValidationFailsClosed);
        }

        private static void SameOwnerExact()
        {
            using (var current = new LMCConnection())
            {
                var decision = Evaluate(
                    current,
                    Evidence(1, current, BootId2, MapRevision2),
                    Evidence(2, current, BootId2, MapRevision2));

                AssertDecision(
                    decision,
                    "same_owner_connection_recovery",
                    "same-owner connection recovery",
                    2,
                    0,
                    2,
                    0,
                    0,
                    2,
                    false,
                    false);
            }
        }

        private static void SameOwnerNewIdentity()
        {
            using (var current = new LMCConnection())
            {
                var oldBootOnly = Evaluate(
                    current,
                    Evidence(1, current, BootId1, MapRevision2),
                    Evidence(2, current, BootId1, MapRevision2));

                AssertDecision(
                    oldBootOnly,
                    "new_diagnostics_identity_session",
                    "new diagnostics identity session",
                    2,
                    0,
                    2,
                    2,
                    0,
                    0,
                    false,
                    false);

                var oldMapOnly = Evaluate(
                    current,
                    Evidence(3, current, BootId2, MapRevision1),
                    Evidence(4, current, BootId2, MapRevision1));
                AssertDecision(
                    oldMapOnly,
                    "new_diagnostics_identity_session",
                    "new diagnostics identity session",
                    2,
                    0,
                    2,
                    0,
                    2,
                    0,
                    false,
                    false);
            }
        }

        private static void HomogeneousPreviousOwner()
        {
            using (var current = new LMCConnection())
            using (var previous = new LMCConnection())
            {
                var decision = Evaluate(
                    current,
                    Evidence(1, previous, BootId1, MapRevision1),
                    Evidence(2, previous, BootId1, MapRevision1));

                AssertDecision(
                    decision,
                    "new_connection_session",
                    "new connection session",
                    2,
                    2,
                    0,
                    2,
                    2,
                    0,
                    true,
                    false);
            }
        }

        private static void CurrentAndForeignOwnersMixed()
        {
            using (var current = new LMCConnection())
            using (var previous = new LMCConnection())
            {
                var decision = Evaluate(
                    current,
                    Evidence(1, current, BootId2, MapRevision2),
                    Evidence(2, previous, BootId1, MapRevision1));

                AssertDecision(
                    decision,
                    "mixed_evidence_sessions",
                    "mixed evidence sessions",
                    2,
                    1,
                    1,
                    1,
                    1,
                    1,
                    false,
                    true);
            }
        }

        private static void MultiplePreviousOwnersMixed()
        {
            using (var current = new LMCConnection())
            using (var previous1 = new LMCConnection())
            using (var previous2 = new LMCConnection())
            {
                var decision = Evaluate(
                    current,
                    Evidence(1, previous1, BootId1, MapRevision1),
                    Evidence(2, previous2, BootId1, MapRevision1));

                AssertDecision(
                    decision,
                    "mixed_evidence_sessions",
                    "mixed evidence sessions",
                    2,
                    2,
                    0,
                    2,
                    2,
                    0,
                    false,
                    true);
            }
        }

        private static void SubmissionIdentityMixed()
        {
            using (var current = new LMCConnection())
            {
                var mixedBoot = Evaluate(
                    current,
                    Evidence(1, current, BootId1, MapRevision2),
                    Evidence(2, current, BootId2, MapRevision2));

                AssertDecision(
                    mixedBoot,
                    "mixed_evidence_sessions",
                    "mixed evidence sessions",
                    2,
                    0,
                    2,
                    1,
                    0,
                    1,
                    false,
                    true);

                var mixedMap = Evaluate(
                    current,
                    Evidence(3, current, BootId2, MapRevision1),
                    Evidence(4, current, BootId2, MapRevision2));
                AssertDecision(
                    mixedMap,
                    "mixed_evidence_sessions",
                    "mixed evidence sessions",
                    2,
                    0,
                    2,
                    0,
                    1,
                    1,
                    false,
                    true);
            }
        }

        private static void WriteEvidenceFailsClosed()
        {
            using (var current = new LMCConnection())
            {
                var writeEvidence = Evidence(
                    2,
                    current,
                    BootId2,
                    MapRevision2,
                    LMCOperationKind.SDOWrite);
                AssertEx.Equal(
                    LMCOperationKind.SDOWrite,
                    writeEvidence.OperationKind);

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => Evaluate(
                        current,
                        Evidence(1, current, BootId2, MapRevision2),
                        writeEvidence));
                AssertEx.Contains("evidence-2", error.Message);
                AssertEx.Contains("index 1", error.Message);
                AssertEx.Contains("SDOWrite", error.Message);
                AssertEx.Contains(
                    "Automatic recovery is unavailable",
                    error.Message);
                AssertEx.Contains(
                    "quarantine must remain active",
                    error.Message);

                var directGuardError =
                    AssertEx.Throws<InvalidOperationException>(
                        () => D5SdoRecoveryScopePolicy
                            .RequireReadRecoveryEvidence(
                                new[] { writeEvidence }));
                AssertEx.Contains("SDOWrite", directGuardError.Message);
            }
        }

        private static void ValidationFailsClosed()
        {
            using (var current = new LMCConnection())
            {
                var evidence = Evidence(
                    1,
                    current,
                    BootId2,
                    MapRevision2);
                AssertEx.Throws<ArgumentNullException>(
                    () => D5SdoRecoveryScopePolicy.Evaluate(
                        null,
                        current,
                        BootId2,
                        MapRevision2));
                AssertEx.Throws<ArgumentException>(
                    () => D5SdoRecoveryScopePolicy.Evaluate(
                        new D5SdoQuarantineEvidence[0],
                        current,
                        BootId2,
                        MapRevision2));
                AssertEx.Throws<ArgumentException>(
                    () => D5SdoRecoveryScopePolicy.Evaluate(
                        new D5SdoQuarantineEvidence[] { null },
                        current,
                        BootId2,
                        MapRevision2));
                AssertEx.Throws<ArgumentException>(
                    () => D5SdoRecoveryScopePolicy.Evaluate(
                        new[]
                        {
                            Evidence(2, null, BootId2, MapRevision2)
                        },
                        current,
                        BootId2,
                        MapRevision2));
                AssertEx.Throws<ArgumentException>(
                    () => D5SdoRecoveryScopePolicy.Evaluate(
                        new[]
                        {
                            Evidence(3, current, 0, MapRevision2)
                        },
                        current,
                        BootId2,
                        MapRevision2));
                AssertEx.Throws<ArgumentException>(
                    () => D5SdoRecoveryScopePolicy.Evaluate(
                        new[]
                        {
                            Evidence(4, current, BootId2, 0)
                        },
                        current,
                        BootId2,
                        MapRevision2));
                AssertEx.Throws<ArgumentNullException>(
                    () => D5SdoRecoveryScopePolicy.Evaluate(
                        new[] { evidence },
                        null,
                        BootId2,
                        MapRevision2));
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => D5SdoRecoveryScopePolicy.Evaluate(
                        new[] { evidence },
                        current,
                        0,
                        MapRevision2));
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => D5SdoRecoveryScopePolicy.Evaluate(
                        new[] { evidence },
                        current,
                        BootId2,
                        0));
            }
        }

        private static D5SdoRecoveryScopeDecision Evaluate(
            LMCConnection currentConnection,
            params D5SdoQuarantineEvidence[] evidence)
        {
            return D5SdoRecoveryScopePolicy.Evaluate(
                evidence,
                currentConnection,
                BootId2,
                MapRevision2);
        }

        private static D5SdoQuarantineEvidence Evidence(
            long entryId,
            LMCConnection ownerConnection,
            uint diagnosticsBootId,
            uint mapRevision,
            LMCOperationKind operationKind = LMCOperationKind.SDORead)
        {
            var request = operationKind == LMCOperationKind.SDOWrite
                ? LMCSdoRequest.CreateWrite(
                    1,
                    0x2000,
                    1,
                    LMCSignalValueType.UInt32,
                    new byte[] { 0x78, 0x56, 0x34, 0x12 },
                    100)
                : null;
            return new D5SdoQuarantineEvidence(
                entryId,
                1,
                0,
                diagnosticsBootId,
                mapRevision,
                operationKind,
                request,
                1,
                100,
                ownerConnection,
                "test-stage",
                "test-reason",
                "evidence-" + entryId);
        }

        private static void AssertDecision(
            D5SdoRecoveryScopeDecision decision,
            string scopeCode,
            string scopeText,
            int evidenceCount,
            int ownerChangedEvidenceCount,
            int sameOwnerEvidenceCount,
            int bootChangedEvidenceCount,
            int mapChangedEvidenceCount,
            int sameIdentityEvidenceCount,
            bool newConnectionRecovery,
            bool mixedEvidenceSessions)
        {
            AssertEx.NotNull(decision);
            AssertEx.Equal(scopeCode, decision.ScopeCode);
            AssertEx.Equal(scopeText, decision.ScopeText);
            AssertEx.Equal(evidenceCount, decision.EvidenceCount);
            AssertEx.Equal(
                ownerChangedEvidenceCount,
                decision.OwnerChangedEvidenceCount);
            AssertEx.Equal(
                sameOwnerEvidenceCount,
                decision.SameOwnerEvidenceCount);
            AssertEx.Equal(
                bootChangedEvidenceCount,
                decision.BootChangedEvidenceCount);
            AssertEx.Equal(
                mapChangedEvidenceCount,
                decision.MapChangedEvidenceCount);
            AssertEx.Equal(
                sameIdentityEvidenceCount,
                decision.SameIdentityEvidenceCount);
            AssertEx.Equal(
                newConnectionRecovery,
                decision.NewConnectionRecovery);
            AssertEx.Equal(
                mixedEvidenceSessions,
                decision.MixedEvidenceSessions);
        }
    }
}
