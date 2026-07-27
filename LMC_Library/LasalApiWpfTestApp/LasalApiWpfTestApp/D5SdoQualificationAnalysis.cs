using System;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class D5SdoAbortRecoveryAnalysisResult
    {
        internal D5SdoAbortRecoveryAnalysisResult(
            uint abortTicketId,
            uint recoveryTicketId,
            uint diagnosticsBootId,
            uint abortCode,
            sbyte recoveredValue)
        {
            AbortTicketId = abortTicketId;
            RecoveryTicketId = recoveryTicketId;
            DiagnosticsBootId = diagnosticsBootId;
            AbortCode = abortCode;
            RecoveredValue = recoveredValue;
        }

        internal uint AbortTicketId { get; private set; }
        internal uint RecoveryTicketId { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint AbortCode { get; private set; }
        internal sbyte RecoveredValue { get; private set; }
    }

    internal static class D5SdoQualificationAnalysis
    {
        private const short EtherCatSdoAbortErrorId = -32000;

        internal static D5SdoAbortRecoveryAnalysisResult
            ValidateAbortThenRecovery(
                LMCOperationTicket abortTicket,
                LMCOperationStatus abortStatus,
                LMCOperationTicket recoveryTicket,
                LMCOperationStatus recoveryStatus,
                uint expectedDiagnosticsBootId,
                sbyte expectedRecoveryValue)
        {
            ValidateAbortTerminal(
                abortTicket,
                abortStatus,
                expectedDiagnosticsBootId);
            ValidateKnownValidInt8Recovery(
                recoveryTicket,
                recoveryStatus,
                expectedDiagnosticsBootId,
                expectedRecoveryValue);

            if (abortTicket.TicketId == recoveryTicket.TicketId)
            {
                throw new InvalidOperationException(
                    "SDO abort and recovery must use distinct operation tickets.");
            }

            return new D5SdoAbortRecoveryAnalysisResult(
                abortTicket.TicketId,
                recoveryTicket.TicketId,
                expectedDiagnosticsBootId,
                abortStatus.OperationDetail,
                unchecked((sbyte)recoveryStatus.ResultData[0]));
        }

        internal static void ValidateAbortTerminal(
            LMCOperationTicket expectedTicket,
            LMCOperationStatus status,
            uint expectedDiagnosticsBootId)
        {
            ValidateSdoTicketAndStatusIdentity(
                expectedTicket,
                status,
                expectedDiagnosticsBootId,
                "SDO abort");

            if (status.State != LMCOperationState.Failed
                || status.Outcome != LMCOperationOutcome.Failed)
            {
                throw new InvalidOperationException(
                    "SDO abort evidence must be a Failed/Failed terminal operation; cancellation and timeout are not abort evidence.");
            }

            if (status.OperationErrorId != EtherCatSdoAbortErrorId)
            {
                throw new InvalidOperationException(
                    "SDO abort evidence must report OperationErrorId=-32000.");
            }

            if (status.OperationDetail == 0)
            {
                throw new InvalidOperationException(
                    "SDO abort evidence must contain the non-zero raw EtherCAT SDO abort code in OperationDetail.");
            }

            if (status.ResultLength != 0
                || status.ResultValueType != LMCSignalValueType.Invalid
                || status.ResultData.Length != 0)
            {
                throw new InvalidOperationException(
                    "A failed SDO abort operation must not contain result data.");
            }
        }

        internal static void ValidateKnownValidInt8Recovery(
            LMCOperationTicket expectedTicket,
            LMCOperationStatus status,
            uint expectedDiagnosticsBootId,
            sbyte expectedValue)
        {
            ValidateKnownValidRecovery(
                expectedTicket,
                status,
                expectedDiagnosticsBootId,
                LMCSignalValueType.Int8,
                new byte[] { unchecked((byte)expectedValue) });
        }

        internal static void ValidateKnownValidRecovery(
            LMCOperationTicket expectedTicket,
            LMCOperationStatus status,
            uint expectedDiagnosticsBootId,
            LMCSignalValueType expectedValueType,
            byte[] expectedValue)
        {
            if (expectedValue == null)
            {
                throw new ArgumentNullException("expectedValue");
            }

            var expectedLength = GetExpectedRecoveryValueLength(
                expectedValueType);
            if (expectedValue.Length != expectedLength)
            {
                throw new InvalidOperationException(
                    "SDO recovery expected value length does not match the expected value type.");
            }

            ValidateSdoTicketAndStatusIdentity(
                expectedTicket,
                status,
                expectedDiagnosticsBootId,
                "SDO recovery");

            if (expectedTicket.RequestedResultLength != expectedLength
                || expectedTicket.ResultValueType != expectedValueType)
            {
                throw new InvalidOperationException(
                    "SDO recovery ticket type and length must match the exact expected read value.");
            }

            if (status.State != LMCOperationState.Completed
                || status.Outcome != LMCOperationOutcome.Success
                || !status.IsSuccessful)
            {
                throw new InvalidOperationException(
                    "SDO recovery evidence must be a Completed/Success operation.");
            }

            if (status.OperationErrorId != 0
                || status.OperationDetail != 0)
            {
                throw new InvalidOperationException(
                    "Successful SDO recovery must report OperationErrorId=0 and OperationDetail=0.");
            }

            var resultData = status.ResultData;
            if (status.ResultLength != expectedLength
                || status.ResultValueType != expectedValueType
                || resultData.Length != expectedLength
                || !ByteArraysEqual(resultData, expectedValue))
            {
                throw new InvalidOperationException(
                    "SDO recovery must return the exact expected value type, length, and bytes.");
            }
        }

        private static int GetExpectedRecoveryValueLength(
            LMCSignalValueType valueType)
        {
            switch (valueType)
            {
                case LMCSignalValueType.Bool:
                case LMCSignalValueType.Int8:
                case LMCSignalValueType.UInt8:
                case LMCSignalValueType.BitField8:
                    return 1;
                case LMCSignalValueType.Int16:
                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    return 2;
                case LMCSignalValueType.Int32:
                case LMCSignalValueType.UInt32:
                case LMCSignalValueType.Real32:
                case LMCSignalValueType.BitField32:
                    return 4;
                default:
                    throw new InvalidOperationException(
                        "SDO recovery expected value type is not supported.");
            }
        }

        private static bool ByteArraysEqual(byte[] left, byte[] right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (var index = 0; index < left.Length; index++)
            {
                if (left[index] != right[index])
                {
                    return false;
                }
            }

            return true;
        }

        private static void ValidateSdoTicketAndStatusIdentity(
            LMCOperationTicket expectedTicket,
            LMCOperationStatus status,
            uint expectedDiagnosticsBootId,
            string stage)
        {
            if (expectedTicket == null)
            {
                throw new ArgumentNullException("expectedTicket");
            }

            if (expectedDiagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedDiagnosticsBootId",
                    "D5 SDO qualification requires a stable non-zero DiagnosticsBootId.");
            }

            if (status == null)
            {
                throw new InvalidOperationException(
                    stage
                    + " did not produce an operation status; local transport failure cannot pass as an SDO abort.");
            }

            if (expectedTicket.OperationKind != LMCOperationKind.SDORead)
            {
                throw new InvalidOperationException(
                    stage + " qualification permits SDO Read tickets only.");
            }

            if (expectedTicket.DiagnosticsBootId
                    != expectedDiagnosticsBootId
                || status.TicketId != expectedTicket.TicketId
                || status.OperationKind != expectedTicket.OperationKind
                || status.DiagnosticsBootId != expectedDiagnosticsBootId)
            {
                throw new InvalidOperationException(
                    stage
                    + " status does not match the exact expected ticket and DiagnosticsBootId.");
            }

            if (status.Response == null
                || !status.Response.IsSuccess
                || status.Response.ErrorId != 0
                || status.Response.Detail != LMCDiagnosticsDetailCode.None)
            {
                throw new InvalidOperationException(
                    stage
                    + " requires a successful GetOperationStatus command response.");
            }
        }
    }
}
