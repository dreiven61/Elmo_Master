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
        private readonly uint baseCycleTimeUs;
        private readonly ushort maxSdoDataBytes;
        private readonly ushort slaveReference;
        private readonly ushort objectIndex;
        private readonly byte subIndex;
        private readonly LMCSignalValueType valueType;
        private readonly ushort dataLength;
        private readonly long minimumIntegerValue;
        private readonly long maximumIntegerValue;
        private readonly uint baselineTicketId;
        private readonly uint preWriteGuardTicketId;
        private readonly uint writeTicketId;
        private readonly uint readbackTicketId;
        private int revoked;

        private SdoWriteActivationQualificationProof(
            LMCConnection ownerConnection,
            long sessionGeneration,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoWriteTarget target,
            LMCOperationTicket baselineTicket,
            LMCOperationTicket preWriteGuardTicket,
            LMCOperationTicket writeTicket,
            LMCOperationTicket readbackTicket)
        {
            this.ownerConnection = ownerConnection;
            this.sessionGeneration = sessionGeneration;
            diagnosticsBuild = capabilities.DiagnosticsBuild;
            diagnosticsBootId = capabilities.DiagnosticsBootId;
            mapRevision = capabilities.MapRevision;
            baseCycleTimeUs = capabilities.BaseCycleTimeUs;
            maxSdoDataBytes = capabilities.MaxSdoDataBytes;
            slaveReference = target.SlaveReference;
            objectIndex = target.ObjectIndex;
            subIndex = target.SubIndex;
            valueType = target.ValueType;
            dataLength = target.DataLength;
            minimumIntegerValue = target.MinimumIntegerValue;
            maximumIntegerValue = target.MaximumIntegerValue;
            baselineTicketId = baselineTicket.TicketId;
            preWriteGuardTicketId = preWriteGuardTicket.TicketId;
            writeTicketId = writeTicket.TicketId;
            readbackTicketId = readbackTicket.TicketId;
        }

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
            baseCycleTimeUs = capabilities.BaseCycleTimeUs;
            maxSdoDataBytes = capabilities.MaxSdoDataBytes;
            slaveReference = target.SlaveReference;
            objectIndex = target.ObjectIndex;
            subIndex = target.SubIndex;
            valueType = target.ValueType;
            dataLength = target.DataLength;
            minimumIntegerValue = target.MinimumIntegerValue;
            maximumIntegerValue = target.MaximumIntegerValue;
            baselineTicketId = 1;
            preWriteGuardTicketId = 2;
            writeTicketId = 3;
            readbackTicketId = 4;
        }

        internal long SessionGeneration { get { return sessionGeneration; } }
        internal uint DiagnosticsBuild { get { return diagnosticsBuild; } }
        internal uint DiagnosticsBootId { get { return diagnosticsBootId; } }
        internal uint MapRevision { get { return mapRevision; } }
        internal uint BaseCycleTimeUs { get { return baseCycleTimeUs; } }
        internal ushort MaxSdoDataBytes { get { return maxSdoDataBytes; } }
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
        internal uint BaselineTicketId { get { return baselineTicketId; } }
        internal uint PreWriteGuardTicketId { get { return preWriteGuardTicketId; } }
        internal uint WriteTicketId { get { return writeTicketId; } }
        internal uint ReadbackTicketId { get { return readbackTicketId; } }

        internal static bool TryCapture(
            LMCConnection connection,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoWriteTarget target,
            LMCOperationTicket baselineTicket,
            LMCOperationTicket preWriteGuardTicket,
            LMCOperationTicket writeTicket,
            LMCOperationTicket readbackTicket,
            out SdoWriteActivationQualificationProof proof)
        {
            proof = null;
            if (connection == null
                || !connection.IsConnected
                || capabilities == null
                || target == null
                || !HasValidQualificationTickets(
                    connection,
                    capabilities,
                    target,
                    baselineTicket,
                    preWriteGuardTicket,
                    writeTicket,
                    readbackTicket))
            {
                return false;
            }

            var capturedSessionGeneration = connection.SessionGeneration;
            if (!HasValidCapabilityIdentity(capabilities)
                || capturedSessionGeneration <= 0
                || !capabilities.IsBoundTo(
                    connection.Diagnostics,
                    capturedSessionGeneration)
                || !HasRequiredTransportCapabilities(capabilities))
            {
                return false;
            }

            var candidate = new SdoWriteActivationQualificationProof(
                connection,
                capturedSessionGeneration,
                capabilities,
                target,
                baselineTicket,
                preWriteGuardTicket,
                writeTicket,
                readbackTicket);
            if (!connection.IsConnected
                || connection.SessionGeneration != capturedSessionGeneration)
            {
                return false;
            }

            proof = candidate;
            return true;
        }

        // Compatibility overload retained for focused legacy tests. Runtime
        // activation uses the four-ticket overload above.
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
                || !HasRequiredTransportCapabilities(capabilities)
                || !IsApprovedTarget(connection.Diagnostics, target))
            {
                return false;
            }
            proof = new SdoWriteActivationQualificationProof(
                connection,
                capturedSessionGeneration,
                capabilities,
                target);
            return connection.IsConnected
                && connection.SessionGeneration == capturedSessionGeneration;
        }

        internal bool MatchesCurrent(
            LMCConnection connection,
            LMCDiagnosticCapabilities capabilities)
        {
            if (Volatile.Read(ref revoked) != 0)
            {
                return false;
            }

            if (!ReferenceEquals(ownerConnection, connection)
                || connection == null
                || !connection.IsConnected
                || capabilities == null)
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
                || capabilities.BaseCycleTimeUs != baseCycleTimeUs
                || capabilities.MaxSdoDataBytes != maxSdoDataBytes
                || !HasRequiredTransportCapabilities(capabilities))
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

        internal bool MatchesCurrent(
            LMCConnection connection,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoWriteTarget target)
        {
            return MatchesTargetTuple(target)
                && MatchesCurrent(connection, capabilities);
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

        private static bool HasRequiredTransportCapabilities(
            LMCDiagnosticCapabilities capabilities)
        {
            return capabilities != null
                && capabilities.BaseCycleTimeUs != 0
                && capabilities.MaxSdoDataBytes >= 4
                && capabilities.Supports(LMCDiagnosticCapability.SDORead)
                && capabilities.Supports(LMCDiagnosticCapability.SDOWrite)
                && capabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline);
        }

        private static bool HasValidQualificationTickets(
            LMCConnection connection,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoWriteTarget target,
            LMCOperationTicket baselineTicket,
            LMCOperationTicket preWriteGuardTicket,
            LMCOperationTicket writeTicket,
            LMCOperationTicket readbackTicket)
        {
            return baselineTicket != null
                && preWriteGuardTicket != null
                && writeTicket != null
                && readbackTicket != null
                && baselineTicket.TicketId != 0
                && preWriteGuardTicket.TicketId != 0
                && writeTicket.TicketId != 0
                && readbackTicket.TicketId != 0
                && baselineTicket.TicketId != preWriteGuardTicket.TicketId
                && baselineTicket.TicketId != writeTicket.TicketId
                && baselineTicket.TicketId != readbackTicket.TicketId
                && preWriteGuardTicket.TicketId != writeTicket.TicketId
                && preWriteGuardTicket.TicketId != readbackTicket.TicketId
                && writeTicket.TicketId != readbackTicket.TicketId
                && baselineTicket.OperationKind == LMCOperationKind.SDORead
                && preWriteGuardTicket.OperationKind == LMCOperationKind.SDORead
                && writeTicket.OperationKind == LMCOperationKind.SDOWrite
                && readbackTicket.OperationKind == LMCOperationKind.SDORead
                && baselineTicket.BelongsToCurrentSession(connection)
                && preWriteGuardTicket.BelongsToCurrentSession(connection)
                && writeTicket.BelongsToCurrentSession(connection)
                && readbackTicket.BelongsToCurrentSession(connection)
                && HasTicketIdentity(baselineTicket, capabilities)
                && HasTicketIdentity(preWriteGuardTicket, capabilities)
                && HasTicketIdentity(writeTicket, capabilities)
                && HasTicketIdentity(readbackTicket, capabilities)
                && MatchesCanaryReadTicket(baselineTicket, target)
                && MatchesCanaryReadTicket(preWriteGuardTicket, target)
                && target.Matches(writeTicket.SubmittedSdoRequest)
                && MatchesCanaryReadTicket(readbackTicket, target);
        }

        private static bool HasTicketIdentity(
            LMCOperationTicket ticket,
            LMCDiagnosticCapabilities capabilities)
        {
            return ticket.DiagnosticsBootId == capabilities.DiagnosticsBootId
                && ticket.SubmissionMapRevision == capabilities.MapRevision;
        }

        private static bool MatchesCanaryReadTicket(
            LMCOperationTicket ticket,
            LMCSdoWriteTarget target)
        {
            var request = ticket == null ? null : ticket.SubmittedSdoRequest;
            return request != null
                && !request.IsWrite
                && request.SlaveReference == target.SlaveReference
                && request.ObjectIndex == target.ObjectIndex
                && request.SubIndex == target.SubIndex
                && request.ValueType == target.ValueType
                && request.DataLength == target.DataLength;
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
