using System;
using System.Collections.Generic;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class D5SdoRecoveryScopeDecision
    {
        internal D5SdoRecoveryScopeDecision(
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
            ScopeCode = scopeCode;
            ScopeText = scopeText;
            EvidenceCount = evidenceCount;
            OwnerChangedEvidenceCount = ownerChangedEvidenceCount;
            SameOwnerEvidenceCount = sameOwnerEvidenceCount;
            BootChangedEvidenceCount = bootChangedEvidenceCount;
            MapChangedEvidenceCount = mapChangedEvidenceCount;
            SameIdentityEvidenceCount = sameIdentityEvidenceCount;
            NewConnectionRecovery = newConnectionRecovery;
            MixedEvidenceSessions = mixedEvidenceSessions;
        }

        internal string ScopeCode { get; private set; }
        internal string ScopeText { get; private set; }
        internal int EvidenceCount { get; private set; }
        internal int OwnerChangedEvidenceCount { get; private set; }
        internal int SameOwnerEvidenceCount { get; private set; }
        internal int BootChangedEvidenceCount { get; private set; }
        internal int MapChangedEvidenceCount { get; private set; }
        internal int SameIdentityEvidenceCount { get; private set; }
        internal bool NewConnectionRecovery { get; private set; }
        internal bool MixedEvidenceSessions { get; private set; }
    }

    internal static class D5SdoRecoveryScopePolicy
    {
        internal static void RequireReadRecoveryEvidence(
            IReadOnlyList<D5SdoQuarantineEvidence> evidence)
        {
            if (evidence == null)
            {
                throw new ArgumentNullException("evidence");
            }

            if (evidence.Count == 0)
            {
                throw new ArgumentException(
                    "D5 recovery scope requires quarantine evidence.",
                    "evidence");
            }

            for (var index = 0; index < evidence.Count; index++)
            {
                RequireEvidence(evidence[index], index);
            }
        }

        internal static D5SdoRecoveryScopeDecision Evaluate(
            IReadOnlyList<D5SdoQuarantineEvidence> evidence,
            LMCConnection currentConnection,
            uint currentDiagnosticsBootId,
            uint currentMapRevision)
        {
            RequireReadRecoveryEvidence(evidence);

            if (currentConnection == null)
            {
                throw new ArgumentNullException("currentConnection");
            }

            if (currentDiagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "currentDiagnosticsBootId");
            }

            if (currentMapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "currentMapRevision");
            }

            var first = RequireEvidence(evidence[0], 0);
            var ownerChangedEvidenceCount = 0;
            var sameOwnerEvidenceCount = 0;
            var bootChangedEvidenceCount = 0;
            var mapChangedEvidenceCount = 0;
            var sameIdentityEvidenceCount = 0;
            var allEvidenceShareOwner = true;
            var allEvidenceShareSubmissionIdentity = true;

            for (var index = 0; index < evidence.Count; index++)
            {
                var item = RequireEvidence(evidence[index], index);
                var hasCurrentOwner = ReferenceEquals(
                    item.OwnerConnection,
                    currentConnection);
                if (hasCurrentOwner)
                {
                    sameOwnerEvidenceCount++;
                }
                else
                {
                    ownerChangedEvidenceCount++;
                }

                if (item.DiagnosticsBootId != currentDiagnosticsBootId)
                {
                    bootChangedEvidenceCount++;
                }

                if (item.MapRevision != currentMapRevision)
                {
                    mapChangedEvidenceCount++;
                }

                if (hasCurrentOwner
                    && item.DiagnosticsBootId == currentDiagnosticsBootId
                    && item.MapRevision == currentMapRevision)
                {
                    sameIdentityEvidenceCount++;
                }

                if (!ReferenceEquals(
                    item.OwnerConnection,
                    first.OwnerConnection))
                {
                    allEvidenceShareOwner = false;
                }

                if (item.DiagnosticsBootId != first.DiagnosticsBootId
                    || item.MapRevision != first.MapRevision)
                {
                    allEvidenceShareSubmissionIdentity = false;
                }
            }

            var allEvidenceMatchCurrentIdentity =
                sameIdentityEvidenceCount == evidence.Count;
            var sameOwnerForAllEvidence =
                sameOwnerEvidenceCount == evidence.Count;
            var ownerChangedForAllEvidence =
                ownerChangedEvidenceCount == evidence.Count;
            var isNewDiagnosticsIdentitySession =
                sameOwnerForAllEvidence
                && allEvidenceShareSubmissionIdentity
                && !allEvidenceMatchCurrentIdentity;
            var isNewConnectionSession =
                ownerChangedForAllEvidence
                && allEvidenceShareOwner
                && allEvidenceShareSubmissionIdentity;
            var mixedEvidenceSessions =
                !allEvidenceMatchCurrentIdentity
                && !isNewDiagnosticsIdentitySession
                && !isNewConnectionSession;

            var scopeCode = allEvidenceMatchCurrentIdentity
                ? "same_owner_connection_recovery"
                : isNewDiagnosticsIdentitySession
                    ? "new_diagnostics_identity_session"
                    : isNewConnectionSession
                        ? "new_connection_session"
                        : "mixed_evidence_sessions";
            var scopeText = allEvidenceMatchCurrentIdentity
                ? "same-owner connection recovery"
                : isNewDiagnosticsIdentitySession
                    ? "new diagnostics identity session"
                    : isNewConnectionSession
                        ? "new connection session"
                        : "mixed evidence sessions";

            return new D5SdoRecoveryScopeDecision(
                scopeCode,
                scopeText,
                evidence.Count,
                ownerChangedEvidenceCount,
                sameOwnerEvidenceCount,
                bootChangedEvidenceCount,
                mapChangedEvidenceCount,
                sameIdentityEvidenceCount,
                isNewConnectionSession,
                mixedEvidenceSessions);
        }

        private static D5SdoQuarantineEvidence RequireEvidence(
            D5SdoQuarantineEvidence evidence,
            int index)
        {
            if (evidence == null)
            {
                throw new ArgumentException(
                    "D5 recovery scope evidence contains a null entry at index "
                        + index
                        + ".",
                    "evidence");
            }

            if (evidence.OwnerConnection == null
                || evidence.DiagnosticsBootId == 0
                || evidence.MapRevision == 0)
            {
                throw new ArgumentException(
                    "D5 recovery scope evidence contains an invalid owner or submission identity at index "
                        + index
                        + ".",
                    "evidence");
            }

            if (evidence.OperationKind != LMCOperationKind.SDORead)
            {
                throw new InvalidOperationException(
                    "D5 SDO Read recovery proof cannot clear quarantine evidence '"
                        + evidence.EvidenceId
                        + "' at index "
                        + index
                        + " for operation "
                        + evidence.OperationKind
                        + ". Automatic recovery is unavailable for SDO Write uncertainty; the quarantine must remain active.");
            }

            return evidence;
        }
    }
}
