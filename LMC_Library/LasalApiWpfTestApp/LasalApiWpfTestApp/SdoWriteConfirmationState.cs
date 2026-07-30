using System;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class SdoWriteConfirmationState
    {
        private Snapshot armed;

        internal bool IsArmed { get { return armed != null; } }

        internal bool TryConsumeOrArm(
            object ownerConnection,
            long ownerSessionGeneration,
            uint diagnosticsBootId,
            uint mapRevision,
            LMCSdoRequest request)
        {
            if (ownerConnection == null)
            {
                throw new ArgumentNullException("ownerConnection");
            }

            if (ownerSessionGeneration <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "ownerSessionGeneration");
            }

            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBootId");
            }

            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("mapRevision");
            }

            if (request == null || !request.IsWrite)
            {
                throw new ArgumentException(
                    "An SDO Write request is required.",
                    "request");
            }

            if (armed != null
                && armed.Matches(
                    ownerConnection,
                    ownerSessionGeneration,
                    diagnosticsBootId,
                    mapRevision,
                    request))
            {
                armed = null;
                return true;
            }

            armed = new Snapshot(
                ownerConnection,
                ownerSessionGeneration,
                diagnosticsBootId,
                mapRevision,
                request);
            return false;
        }

        internal void Clear()
        {
            armed = null;
        }

        private sealed class Snapshot
        {
            private readonly object ownerConnection;
            private readonly long ownerSessionGeneration;
            private readonly uint diagnosticsBootId;
            private readonly uint mapRevision;
            private readonly ushort slaveReference;
            private readonly LMCOperationFlags operationFlags;
            private readonly ushort objectIndex;
            private readonly byte subIndex;
            private readonly LMCSignalValueType valueType;
            private readonly uint timeoutCycles;
            private readonly ushort dataLength;
            private readonly byte[] writeData;

            internal Snapshot(
                object ownerConnection,
                long ownerSessionGeneration,
                uint diagnosticsBootId,
                uint mapRevision,
                LMCSdoRequest request)
            {
                this.ownerConnection = ownerConnection;
                this.ownerSessionGeneration = ownerSessionGeneration;
                this.diagnosticsBootId = diagnosticsBootId;
                this.mapRevision = mapRevision;
                slaveReference = request.SlaveReference;
                operationFlags = request.OperationFlags;
                objectIndex = request.ObjectIndex;
                subIndex = request.SubIndex;
                valueType = request.ValueType;
                timeoutCycles = request.TimeoutCycles;
                dataLength = request.DataLength;
                writeData = request.WriteData;
            }

            internal bool Matches(
                object candidateOwnerConnection,
                long candidateOwnerSessionGeneration,
                uint candidateDiagnosticsBootId,
                uint candidateMapRevision,
                LMCSdoRequest request)
            {
                if (!ReferenceEquals(
                        ownerConnection,
                        candidateOwnerConnection)
                    || ownerSessionGeneration
                        != candidateOwnerSessionGeneration
                    || diagnosticsBootId != candidateDiagnosticsBootId
                    || mapRevision != candidateMapRevision
                    || request == null
                    || request.SlaveReference != slaveReference
                    || request.OperationFlags != operationFlags
                    || request.ObjectIndex != objectIndex
                    || request.SubIndex != subIndex
                    || request.ValueType != valueType
                    || request.TimeoutCycles != timeoutCycles
                    || request.DataLength != dataLength)
                {
                    return false;
                }

                var candidateWriteData = request.WriteData;
                if (candidateWriteData.Length != writeData.Length)
                {
                    return false;
                }

                for (var index = 0; index < writeData.Length; index++)
                {
                    if (candidateWriteData[index] != writeData[index])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
