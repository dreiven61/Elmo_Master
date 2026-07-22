using System;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_DiagnosticsFrame
    {
        internal const int OperationIdentityRequestPayloadLength = 16;
        internal const int SubmitPiWriteRequestPayloadLength = 28;
        internal const int SubmitSdoRequestHeaderPayloadLength = 32;
        internal const ushort MaxD5InlineSdoDataBytes = 12;
        internal const int SdoResultChunkRequestPayloadLength = 28;

        internal static byte[] SubmitPIWrite(
            uint requestId,
            LMCPIWriteRequest request,
            uint diagnosticsBootId)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            RequireD5Identity(request.MapRevision, diagnosticsBootId);
            var buffer = CreateCommonRequest(
                LMC_CommandId.SubmitPIWrite,
                requestId,
                SubmitPiWriteRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, request.MapRevision);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 12, request.SignalId);
            buffer[payloadOffset + 16] = (byte)request.ValueType;
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 20, request.RawValue32);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                diagnosticsBootId);
            return buffer;
        }

        internal static byte[] SubmitSdo(
            uint requestId,
            uint expectedMapRevision,
            LMCSdoRequest request,
            uint diagnosticsBootId)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            RequireD5Identity(expectedMapRevision, diagnosticsBootId);

            var writeLength = request.IsWrite ? request.DataLength : 0;
            var payloadLength = checked(
                SubmitSdoRequestHeaderPayloadLength + writeLength);
            var buffer = CreateCommonRequest(
                LMC_CommandId.SubmitSdo,
                requestId,
                payloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;

            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 8,
                expectedMapRevision);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 12,
                request.SlaveReference);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 14,
                (ushort)request.OperationFlags);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 16,
                request.ObjectIndex);
            buffer[payloadOffset + 18] = request.SubIndex;
            buffer[payloadOffset + 19] = (byte)request.ValueType;
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 20,
                request.TimeoutCycles);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 24,
                request.DataLength);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 28,
                diagnosticsBootId);

            if (request.IsWrite)
            {
                Buffer.BlockCopy(
                    request.WriteDataUnsafe,
                    0,
                    buffer,
                    payloadOffset + 32,
                    request.DataLength);
            }

            return buffer;
        }

        internal static byte[] GetOperationStatus(
            uint requestId,
            uint ticketId,
            uint diagnosticsBootId)
        {
            return CreateOperationIdentityRequest(
                LMC_CommandId.GetOperationStatus,
                requestId,
                ticketId,
                diagnosticsBootId);
        }

        internal static byte[] CancelOperation(
            uint requestId,
            uint ticketId,
            uint diagnosticsBootId)
        {
            return CreateOperationIdentityRequest(
                LMC_CommandId.CancelOperation,
                requestId,
                ticketId,
                diagnosticsBootId);
        }

        internal static byte[] ReadSdoResultChunk(
            uint requestId,
            LMCSdoResultChunkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var ticket = request.Ticket;
            var buffer = CreateCommonRequest(
                LMC_CommandId.ReadSdoResultChunk,
                requestId,
                SdoResultChunkRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, ticket.TicketId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 12, request.OffsetBytes);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 16,
                request.RequestedByteCount);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 20, request.Sequence);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                ticket.DiagnosticsBootId);
            return buffer;
        }

        private static byte[] CreateOperationIdentityRequest(
            ushort commandId,
            uint requestId,
            uint ticketId,
            uint diagnosticsBootId)
        {
            if (ticketId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "ticketId",
                    "TicketId must be non-zero.");
            }

            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "DiagnosticsBootId must be non-zero.");
            }

            var buffer = CreateCommonRequest(
                commandId,
                requestId,
                OperationIdentityRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, ticketId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                diagnosticsBootId);
            return buffer;
        }

        private static void RequireD5Identity(
            uint expectedMapRevision,
            uint diagnosticsBootId)
        {
            if (expectedMapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedMapRevision",
                    "D5 operations require a non-zero exact Catalog revision.");
            }

            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "D5 operations require a non-zero DiagnosticsBootId.");
            }
        }
    }

    internal static partial class LMC_DiagnosticsParser
    {
        internal const int SubmitOperationPayloadLength = 32;
        internal const int OperationStatusPayloadLength = 64;
        internal const int CancelOperationPayloadLength = 28;
        internal const int SdoResultChunkResponseHeaderPayloadLength = 48;

        internal static LMCOperationSubmission ParseSubmitOperation(
            byte[] raw,
            uint expectedRequestId,
            LMCOperationKind expectedKind,
            uint expectedDiagnosticsBootId,
            string commandName)
        {
            if (expectedKind <= LMCOperationKind.None
                || expectedKind > LMCOperationKind.SDOWrite)
            {
                throw new ArgumentOutOfRangeException("expectedKind");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                commandName);
            RequireExactPayloadLength(
                response,
                SubmitOperationPayloadLength,
                commandName);
            RequireNoResponseFlags(response, commandName);

            var payload = response.TransportResponse.Payload;
            var ticketId = LMC_Frame.ReadUInt32(payload, 16);
            var operationKind = (LMCOperationKind)LMC_Frame.ReadUInt16(
                payload,
                20);
            var operationState = (LMCOperationState)LMC_Frame.ReadUInt16(
                payload,
                22);
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 28);

            if (ticketId == 0
                || operationKind != expectedKind
                || operationState != LMCOperationState.Queued
                || diagnosticsBootId == 0
                || diagnosticsBootId != expectedDiagnosticsBootId)
            {
                throw new InvalidDataException(
                    commandName + " returned an invalid ticket identity or state.");
            }

            return new LMCOperationSubmission(
                response,
                ticketId,
                operationKind,
                LMC_Frame.ReadUInt32(payload, 24),
                diagnosticsBootId);
        }

        internal static LMCOperationStatus ParseOperationStatus(
            byte[] raw,
            uint expectedRequestId,
            LMCOperationTicket expectedTicket)
        {
            if (expectedTicket == null)
            {
                throw new ArgumentNullException("expectedTicket");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "GetOperationStatus");
            RequireExactPayloadLength(
                response,
                OperationStatusPayloadLength,
                "GetOperationStatus");
            RequireNoResponseFlags(response, "GetOperationStatus");

            var payload = response.TransportResponse.Payload;
            var ticketId = LMC_Frame.ReadUInt32(payload, 16);
            var operationKind = (LMCOperationKind)LMC_Frame.ReadUInt16(
                payload,
                20);
            var state = (LMCOperationState)LMC_Frame.ReadUInt16(payload, 22);
            var outcome = (LMCOperationOutcome)LMC_Frame.ReadUInt16(
                payload,
                32);
            var operationErrorId = unchecked(
                (short)LMC_Frame.ReadUInt16(payload, 34));
            var resultLength = LMC_Frame.ReadUInt32(payload, 40);
            var resultValueType = (LMCSignalValueType)payload[44];
            var resultDataLength = payload[45];
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 60);

            if (ticketId != expectedTicket.TicketId
                || operationKind != expectedTicket.OperationKind
                || diagnosticsBootId == 0
                || diagnosticsBootId != expectedTicket.DiagnosticsBootId)
            {
                throw new InvalidDataException(
                    "GetOperationStatus returned a stale or mismatched ticket identity.");
            }

            if (LMC_Frame.ReadUInt16(payload, 46) != 0)
            {
                throw new InvalidDataException(
                    "GetOperationStatus reserved field must be zero.");
            }

            ValidateOperationStateOutcome(
                state,
                outcome,
                LMC_Frame.ReadUInt32(payload, 28),
                operationErrorId,
                LMC_Frame.ReadUInt32(payload, 36));
            ValidateOperationResult(
                expectedTicket,
                state,
                outcome,
                resultLength,
                resultValueType,
                resultDataLength,
                payload);

            var resultData = new byte[resultDataLength];
            if (resultDataLength != 0)
            {
                Buffer.BlockCopy(
                    payload,
                    48,
                    resultData,
                    0,
                    resultDataLength);
            }

            return new LMCOperationStatus(
                response,
                ticketId,
                operationKind,
                state,
                LMC_Frame.ReadUInt32(payload, 24),
                LMC_Frame.ReadUInt32(payload, 28),
                outcome,
                operationErrorId,
                LMC_Frame.ReadUInt32(payload, 36),
                resultLength,
                resultValueType,
                resultData,
                diagnosticsBootId);
        }

        internal static LMCCancelOperationResult ParseCancelOperation(
            byte[] raw,
            uint expectedRequestId,
            LMCOperationTicket expectedTicket)
        {
            if (expectedTicket == null)
            {
                throw new ArgumentNullException("expectedTicket");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "CancelOperation");
            RequireExactPayloadLength(
                response,
                CancelOperationPayloadLength,
                "CancelOperation");
            RequireNoResponseFlags(response, "CancelOperation");

            var payload = response.TransportResponse.Payload;
            var ticketId = LMC_Frame.ReadUInt32(payload, 16);
            var state = (LMCOperationState)LMC_Frame.ReadUInt16(payload, 20);
            var outcome = (LMCOperationOutcome)LMC_Frame.ReadUInt16(
                payload,
                22);
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 24);

            if (ticketId != expectedTicket.TicketId
                || state != LMCOperationState.Cancelled
                || outcome != LMCOperationOutcome.Cancelled
                || diagnosticsBootId == 0
                || diagnosticsBootId != expectedTicket.DiagnosticsBootId)
            {
                throw new InvalidDataException(
                    "CancelOperation returned an invalid or mismatched cancellation identity.");
            }

            return new LMCCancelOperationResult(
                response,
                ticketId,
                state,
                outcome,
                diagnosticsBootId);
        }

        internal static LMCSdoResultChunk ParseSdoResultChunk(
            byte[] raw,
            uint expectedRequestId,
            LMCSdoResultChunkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "ReadSDOResultChunk");
            var payload = response.TransportResponse.Payload;
            if (payload.Length < SdoResultChunkResponseHeaderPayloadLength)
            {
                throw new InvalidDataException(
                    "ReadSDOResultChunk response is shorter than its 48-byte header.");
            }

            var ticket = request.Ticket;
            var ticketId = LMC_Frame.ReadUInt32(payload, 16);
            var offsetBytes = LMC_Frame.ReadUInt32(payload, 20);
            var returnedByteCount = LMC_Frame.ReadUInt16(payload, 24);
            var sequence = LMC_Frame.ReadUInt32(payload, 28);
            var totalResultLength = LMC_Frame.ReadUInt32(payload, 32);
            var dataCrc32 = LMC_Frame.ReadUInt32(payload, 36);
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 40);
            var valueType = (LMCSignalValueType)payload[44];
            var expectedPayloadLength = checked(
                SdoResultChunkResponseHeaderPayloadLength
                + returnedByteCount);

            if (payload.Length != expectedPayloadLength
                || LMC_Frame.ReadUInt16(payload, 26) != 0
                || payload[45] != 0
                || payload[46] != 0
                || payload[47] != 0
                || ticketId != ticket.TicketId
                || offsetBytes != request.OffsetBytes
                || sequence != request.Sequence
                || totalResultLength != ticket.ExpectedResultLength
                || diagnosticsBootId != ticket.DiagnosticsBootId
                || valueType != ticket.ExpectedResultValueType
                || returnedByteCount == 0
                || returnedByteCount > request.RequestedByteCount
                || offsetBytes >= totalResultLength
                || returnedByteCount > totalResultLength - offsetBytes)
            {
                throw new InvalidDataException(
                    "ReadSDOResultChunk returned invalid length, identity, or range metadata.");
            }

            var isLastChunk = offsetBytes + returnedByteCount
                == totalResultLength;
            var expectedFlags = isLastChunk
                ? LMCDiagnosticsResponseFlags.LastChunk
                : LMCDiagnosticsResponseFlags.None;
            if (response.ResponseFlags != expectedFlags)
            {
                throw new InvalidDataException(
                    "ReadSDOResultChunk LastChunk flag does not match the returned byte range.");
            }

            var actualCrc = ComputeRecorderDataCrc32(
                payload,
                SdoResultChunkResponseHeaderPayloadLength,
                returnedByteCount);
            if (actualCrc != dataCrc32)
            {
                throw new InvalidDataException(
                    "ReadSDOResultChunk data CRC-32 does not match the payload.");
            }

            var data = new byte[returnedByteCount];
            Buffer.BlockCopy(
                payload,
                SdoResultChunkResponseHeaderPayloadLength,
                data,
                0,
                returnedByteCount);
            return new LMCSdoResultChunk(
                response,
                ticketId,
                offsetBytes,
                returnedByteCount,
                sequence,
                totalResultLength,
                dataCrc32,
                diagnosticsBootId,
                valueType,
                data);
        }

        private static void ValidateOperationStateOutcome(
            LMCOperationState state,
            LMCOperationOutcome outcome,
            uint completionCycle,
            short operationErrorId,
            uint operationDetail)
        {
            var validPair = (state == LMCOperationState.Queued
                    || state == LMCOperationState.Running)
                ? outcome == LMCOperationOutcome.NoneOrPending
                : state == LMCOperationState.Completed
                    ? outcome == LMCOperationOutcome.Success
                    : state == LMCOperationState.Failed
                        ? outcome == LMCOperationOutcome.Failed
                        : state == LMCOperationState.Cancelled
                            ? outcome == LMCOperationOutcome.Cancelled
                            : state == LMCOperationState.Expired
                                && outcome == LMCOperationOutcome.TimedOut;

            if (!validPair)
            {
                throw new InvalidDataException(
                    "GetOperationStatus returned an invalid state/outcome pair.");
            }

            var isPending = state == LMCOperationState.Queued
                || state == LMCOperationState.Running;
            if (isPending
                && (completionCycle != 0
                    || operationErrorId != 0
                    || operationDetail != 0))
            {
                throw new InvalidDataException(
                    "A pending operation must not report completion or error fields.");
            }

            if (state == LMCOperationState.Completed
                && (operationErrorId != 0 || operationDetail != 0))
            {
                throw new InvalidDataException(
                    "A successful operation must not report an operation error.");
            }

            if ((state == LMCOperationState.Cancelled
                    || state == LMCOperationState.Expired)
                && operationErrorId != 0)
            {
                throw new InvalidDataException(
                    "Cancelled or expired operations must not report OperationErrorId.");
            }
        }

        private static void ValidateOperationResult(
            LMCOperationTicket expectedTicket,
            LMCOperationState state,
            LMCOperationOutcome outcome,
            uint resultLength,
            LMCSignalValueType resultValueType,
            byte resultDataLength,
            byte[] payload)
        {
            if (resultDataLength != 0
                && resultDataLength != 1
                && resultDataLength != 2
                && resultDataLength != 4
                && resultDataLength != 8
                && resultDataLength != 12)
            {
                throw new InvalidDataException(
                    "GetOperationStatus ResultDataLength must be 0, 1, 2, 4, 8, or 12.");
            }

            var shouldContainResult = expectedTicket.ExpectsResultData
                && state == LMCOperationState.Completed
                && outcome == LMCOperationOutcome.Success;

            if (shouldContainResult)
            {
                if (resultLength != expectedTicket.ExpectedResultLength
                    || resultValueType
                        != expectedTicket.ExpectedResultValueType)
                {
                    throw new InvalidDataException(
                        "Completed SDO Read result metadata does not match the submitted request.");
                }

                if (expectedTicket.UsesExtendedResultChunks)
                {
                    if (resultDataLength != 0)
                    {
                        throw new InvalidDataException(
                            "Extended SDO Read results must be retrieved through result chunks, not the inline status field.");
                    }
                }
                else
                {
                    if (resultDataLength != expectedTicket.ExpectedResultLength)
                    {
                        throw new InvalidDataException(
                            "Inline SDO Read result length does not match the submitted request.");
                    }

                    ValidateCanonicalOperationResult(
                        resultValueType,
                        resultDataLength,
                        payload);
                }
            }
            else if (resultLength != 0
                || resultDataLength != 0
                || resultValueType != LMCSignalValueType.Invalid)
            {
                throw new InvalidDataException(
                    "PI/SDO Write and unfinished operations must not contain result data.");
            }

            for (var index = 48 + resultDataLength; index < 60; index++)
            {
                if (payload[index] != 0)
                {
                    throw new InvalidDataException(
                        "GetOperationStatus unused result tail bytes must be zero.");
                }
            }
        }

        private static void ValidateCanonicalOperationResult(
            LMCSignalValueType valueType,
            byte resultDataLength,
            byte[] payload)
        {
            const int resultOffset = 48;
            int tailOffset;
            byte expectedTail;

            switch (valueType)
            {
                case LMCSignalValueType.Bool:
                    if (payload[resultOffset] > 1)
                    {
                        throw new InvalidDataException(
                            "Completed Bool SDO Read data must begin with canonical zero or one.");
                    }

                    tailOffset = 1;
                    expectedTail = 0;
                    break;

                case LMCSignalValueType.Int16:
                    tailOffset = 2;
                    expectedTail = (payload[resultOffset + 1] & 0x80) == 0
                        ? (byte)0
                        : (byte)0xFF;
                    break;

                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    tailOffset = 2;
                    expectedTail = 0;
                    break;

                default:
                    return;
            }

            for (var index = tailOffset; index < resultDataLength; index++)
            {
                if (payload[resultOffset + index] != expectedTail)
                {
                    throw new InvalidDataException(
                        "Completed narrow SDO Read data is not canonically sign or zero extended.");
                }
            }
        }
    }
}
