using System;
using System.Collections.Generic;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class SdoWriteActivationQualificationProof
    {
        private readonly LMCConnection ownerConnection;
        private readonly long sessionGeneration;
        private readonly uint diagnosticsBuild;
        private readonly uint diagnosticsBootId;
        private readonly uint mapRevision;
        private readonly ushort slaveReference;
        private readonly ushort objectIndex;
        private readonly byte subIndex;
        private readonly LMCSignalValueType valueType;
        private readonly ushort dataLength;
        private readonly long minimumIntegerValue;
        private readonly long maximumIntegerValue;
        private int revoked;

        private SdoWriteActivationQualificationProof(
            LMCConnection ownerConnection,
            long sessionGeneration,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoWriteTarget target)
        {
            this.ownerConnection = ownerConnection;
            this.sessionGeneration = sessionGeneration;
            diagnosticsBuild = capabilities.DiagnosticsBuild;
            diagnosticsBootId = capabilities.DiagnosticsBootId;
            mapRevision = capabilities.MapRevision;
            slaveReference = target.SlaveReference;
            objectIndex = target.ObjectIndex;
            subIndex = target.SubIndex;
            valueType = target.ValueType;
            dataLength = target.DataLength;
            minimumIntegerValue = target.MinimumIntegerValue;
            maximumIntegerValue = target.MaximumIntegerValue;
        }

        internal long SessionGeneration { get { return sessionGeneration; } }
        internal uint DiagnosticsBuild { get { return diagnosticsBuild; } }
        internal uint DiagnosticsBootId { get { return diagnosticsBootId; } }
        internal uint MapRevision { get { return mapRevision; } }
        internal ushort SlaveReference { get { return slaveReference; } }
        internal ushort ObjectIndex { get { return objectIndex; } }
        internal byte SubIndex { get { return subIndex; } }
        internal LMCSignalValueType ValueType { get { return valueType; } }
        internal ushort DataLength { get { return dataLength; } }
        internal long MinimumIntegerValue
        {
            get { return minimumIntegerValue; }
        }

        internal long MaximumIntegerValue
        {
            get { return maximumIntegerValue; }
        }

        internal static bool TryCapture(
            LMCConnection connection,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoWriteTarget target,
            out SdoWriteActivationQualificationProof proof)
        {
            proof = null;
            if (connection == null
                || !connection.IsConnected
                || capabilities == null
                || target == null)
            {
                return false;
            }

            var capturedSessionGeneration = connection.SessionGeneration;
            if (!HasValidCapabilityIdentity(capabilities)
                || capturedSessionGeneration <= 0
                || !capabilities.IsBoundTo(
                    connection.Diagnostics,
                    capturedSessionGeneration)
                || !IsApprovedTarget(connection.Diagnostics, target))
            {
                return false;
            }

            var candidate = new SdoWriteActivationQualificationProof(
                connection,
                capturedSessionGeneration,
                capabilities,
                target);
            if (!connection.IsConnected
                || connection.SessionGeneration != capturedSessionGeneration)
            {
                return false;
            }

            proof = candidate;
            return true;
        }

        internal bool MatchesCurrent(
            LMCConnection connection,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoWriteTarget target)
        {
            if (Volatile.Read(ref revoked) != 0)
            {
                return false;
            }

            if (!ReferenceEquals(ownerConnection, connection)
                || connection == null
                || !connection.IsConnected
                || capabilities == null
                || target == null)
            {
                Revoke();
                return false;
            }

            var currentSessionGeneration = connection.SessionGeneration;
            if (currentSessionGeneration <= 0
                || currentSessionGeneration != sessionGeneration
                || !HasValidCapabilityIdentity(capabilities)
                || !capabilities.IsBoundTo(
                    connection.Diagnostics,
                    currentSessionGeneration)
                || capabilities.DiagnosticsBuild != diagnosticsBuild
                || capabilities.DiagnosticsBootId != diagnosticsBootId
                || capabilities.MapRevision != mapRevision
                || !IsApprovedTarget(connection.Diagnostics, target)
                || !MatchesTargetTuple(target))
            {
                Revoke();
                return false;
            }

            if (!connection.IsConnected
                || connection.SessionGeneration != sessionGeneration)
            {
                Revoke();
                return false;
            }

            return Volatile.Read(ref revoked) == 0;
        }

        internal void Revoke()
        {
            Interlocked.Exchange(ref revoked, 1);
        }

        private static bool HasValidCapabilityIdentity(
            LMCDiagnosticCapabilities capabilities)
        {
            return capabilities.DiagnosticsBuild != 0
                && capabilities.DiagnosticsBootId != 0
                && capabilities.MapRevision != 0;
        }

        private static bool IsApprovedTarget(
            LMCDiagnostics diagnostics,
            LMCSdoWriteTarget candidate)
        {
            if (diagnostics == null || candidate == null)
            {
                return false;
            }

            IReadOnlyList<LMCSdoWriteTarget> approvedTargets =
                diagnostics.GetApprovedSdoWriteTargets();
            if (approvedTargets == null)
            {
                return false;
            }

            for (var index = 0; index < approvedTargets.Count; index++)
            {
                if (HasSameTargetTuple(approvedTargets[index], candidate))
                {
                    return true;
                }
            }

            return false;
        }

        private bool MatchesTargetTuple(LMCSdoWriteTarget candidate)
        {
            return candidate != null
                && candidate.SlaveReference == slaveReference
                && candidate.ObjectIndex == objectIndex
                && candidate.SubIndex == subIndex
                && candidate.ValueType == valueType
                && candidate.DataLength == dataLength
                && candidate.MinimumIntegerValue == minimumIntegerValue
                && candidate.MaximumIntegerValue == maximumIntegerValue;
        }

        private static bool HasSameTargetTuple(
            LMCSdoWriteTarget left,
            LMCSdoWriteTarget right)
        {
            return left != null
                && right != null
                && left.SlaveReference == right.SlaveReference
                && left.ObjectIndex == right.ObjectIndex
                && left.SubIndex == right.SubIndex
                && left.ValueType == right.ValueType
                && left.DataLength == right.DataLength
                && left.MinimumIntegerValue == right.MinimumIntegerValue
                && left.MaximumIntegerValue == right.MaximumIntegerValue;
        }
    }
}
