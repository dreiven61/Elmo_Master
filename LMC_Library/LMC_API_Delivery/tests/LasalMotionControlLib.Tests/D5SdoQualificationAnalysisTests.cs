using System;
using System.Collections.Generic;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class D5SdoQualificationAnalysisTests
    {
        private const uint AbortTicketId = 0x10203040u;
        private const uint RecoveryTicketId = 0x10203041u;
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const uint SdoAbortCode = 0x06020000u;
        private const sbyte ExpectedValue = -3;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.D5Sdo.ExactAbortRecoveryAccepted",
                ExactAbortRecoveryAccepted);
            tests.Add(
                "Qualification.D5Sdo.RawAbortCodeRequired",
                RawAbortCodeRequired);
            tests.Add(
                "Qualification.D5Sdo.TimeoutAndCancelRejected",
                TimeoutAndCancelRejected);
            tests.Add(
                "Qualification.D5Sdo.AbortErrorAndResultRejected",
                AbortErrorAndResultRejected);
            tests.Add(
                "Qualification.D5Sdo.RecoveryIdentityExact",
                RecoveryIdentityExact);
            tests.Add(
                "Qualification.D5Sdo.RecoveryTerminalAndErrorExact",
                RecoveryTerminalAndErrorExact);
            tests.Add(
                "Qualification.D5Sdo.RecoveryInt8ValueExact",
                RecoveryInt8ValueExact);
            tests.Add(
                "Qualification.D5Sdo.RecoveryUInt32ValueExact",
                RecoveryUInt32ValueExact);
            tests.Add(
                "Qualification.D5Sdo.RecoveryGenericTypeRejected",
                RecoveryGenericTypeRejected);
            tests.Add(
                "Qualification.D5Sdo.RecoveryGenericValueRejected",
                RecoveryGenericValueRejected);
            tests.Add(
                "Qualification.D5Sdo.RecoveryGenericLengthRejected",
                RecoveryGenericLengthRejected);
            tests.Add(
                "Qualification.D5Sdo.LocalOrCommandFailureRejected",
                LocalOrCommandFailureRejected);
        }

        private static void ExactAbortRecoveryAccepted()
        {
            using (var connection = new LMCConnection())
            {
                var abortTicket = SdoReadTicket(
                    connection.Diagnostics,
                    AbortTicketId,
                    DiagnosticsBootId);
                var recoveryTicket = SdoReadTicket(
                    connection.Diagnostics,
                    RecoveryTicketId,
                    DiagnosticsBootId);
                var abortStatus = AbortStatus(
                    abortTicket,
                    SdoAbortCode);
                var recoveryStatus = RecoveryStatus(
                    recoveryTicket,
                    ExpectedValue);

                var result = D5SdoQualificationAnalysis
                    .ValidateAbortThenRecovery(
                        abortTicket,
                        abortStatus,
                        recoveryTicket,
                        recoveryStatus,
                        DiagnosticsBootId,
                        ExpectedValue);

                AssertEx.Equal(AbortTicketId, result.AbortTicketId);
                AssertEx.Equal(RecoveryTicketId, result.RecoveryTicketId);
                AssertEx.Equal(DiagnosticsBootId, result.DiagnosticsBootId);
                AssertEx.Equal(SdoAbortCode, result.AbortCode);
                AssertEx.Equal(ExpectedValue, result.RecoveredValue);
            }
        }

        private static void RawAbortCodeRequired()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = SdoReadTicket(
                    connection.Diagnostics,
                    AbortTicketId,
                    DiagnosticsBootId);

                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis.ValidateAbortTerminal(
                        ticket,
                        AbortStatus(ticket, 0),
                        DiagnosticsBootId));
            }
        }

        private static void TimeoutAndCancelRejected()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = SdoReadTicket(
                    connection.Diagnostics,
                    AbortTicketId,
                    DiagnosticsBootId);
                var cancelled = Status(
                    ticket,
                    LMCOperationState.Cancelled,
                    LMCOperationOutcome.Cancelled,
                    0,
                    0,
                    0,
                    LMCSignalValueType.Invalid,
                    new byte[0]);
                var timedOut = Status(
                    ticket,
                    LMCOperationState.Expired,
                    LMCOperationOutcome.TimedOut,
                    0,
                    0,
                    0,
                    LMCSignalValueType.Invalid,
                    new byte[0]);

                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis.ValidateAbortTerminal(
                        ticket,
                        cancelled,
                        DiagnosticsBootId));
                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis.ValidateAbortTerminal(
                        ticket,
                        timedOut,
                        DiagnosticsBootId));
            }
        }

        private static void AbortErrorAndResultRejected()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = SdoReadTicket(
                    connection.Diagnostics,
                    AbortTicketId,
                    DiagnosticsBootId);
                var wrongError = Status(
                    ticket,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed,
                    -1,
                    SdoAbortCode,
                    0,
                    LMCSignalValueType.Invalid,
                    new byte[0]);
                var unexpectedResult = Status(
                    ticket,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed,
                    -32000,
                    SdoAbortCode,
                    1,
                    LMCSignalValueType.Int8,
                    new byte[] { 1 });
                var writeTicket = SdoWriteTicket(
                    connection.Diagnostics,
                    AbortTicketId + 1,
                    DiagnosticsBootId);
                var writeAbort = Status(
                    writeTicket,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed,
                    -32000,
                    SdoAbortCode,
                    0,
                    LMCSignalValueType.Invalid,
                    new byte[0]);

                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis.ValidateAbortTerminal(
                        ticket,
                        wrongError,
                        DiagnosticsBootId));
                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis.ValidateAbortTerminal(
                        ticket,
                        unexpectedResult,
                        DiagnosticsBootId));
                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis.ValidateAbortTerminal(
                        writeTicket,
                        writeAbort,
                        DiagnosticsBootId));
            }
        }

        private static void RecoveryIdentityExact()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = SdoReadTicket(
                    connection.Diagnostics,
                    RecoveryTicketId,
                    DiagnosticsBootId);
                var wrongTicket = RecoveryStatus(
                    ticket,
                    ExpectedValue,
                    RecoveryTicketId + 1,
                    DiagnosticsBootId);
                var wrongBoot = RecoveryStatus(
                    ticket,
                    ExpectedValue,
                    RecoveryTicketId,
                    DiagnosticsBootId + 1);

                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis
                        .ValidateKnownValidInt8Recovery(
                            ticket,
                            wrongTicket,
                            DiagnosticsBootId,
                            ExpectedValue));
                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis
                        .ValidateKnownValidInt8Recovery(
                            ticket,
                            wrongBoot,
                            DiagnosticsBootId,
                            ExpectedValue));
            }
        }

        private static void RecoveryTerminalAndErrorExact()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = SdoReadTicket(
                    connection.Diagnostics,
                    RecoveryTicketId,
                    DiagnosticsBootId);
                var failed = Status(
                    ticket,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed,
                    -32000,
                    SdoAbortCode,
                    0,
                    LMCSignalValueType.Invalid,
                    new byte[0]);
                var completedWithError = Status(
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    -1,
                    1,
                    1,
                    LMCSignalValueType.Int8,
                    new byte[] { unchecked((byte)ExpectedValue) });

                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis
                        .ValidateKnownValidInt8Recovery(
                            ticket,
                            failed,
                            DiagnosticsBootId,
                            ExpectedValue));
                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis
                        .ValidateKnownValidInt8Recovery(
                            ticket,
                            completedWithError,
                            DiagnosticsBootId,
                            ExpectedValue));
            }
        }

        private static void RecoveryInt8ValueExact()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = SdoReadTicket(
                    connection.Diagnostics,
                    RecoveryTicketId,
                    DiagnosticsBootId);
                var wrongValue = RecoveryStatus(
                    ticket,
                    unchecked((sbyte)(ExpectedValue + 1)));
                var wrongType = Status(
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    0,
                    0,
                    1,
                    LMCSignalValueType.UInt8,
                    new byte[] { unchecked((byte)ExpectedValue) });

                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis
                        .ValidateKnownValidInt8Recovery(
                            ticket,
                            wrongValue,
                            DiagnosticsBootId,
                            ExpectedValue));
                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis
                        .ValidateKnownValidInt8Recovery(
                            ticket,
                            wrongType,
                            DiagnosticsBootId,
                            ExpectedValue));
            }
        }

        private static void RecoveryUInt32ValueExact()
        {
            using (var connection = new LMCConnection())
            {
                var expectedValue = UInt32RecoveryValue();
                var ticket = SdoReadTicket(
                    connection.Diagnostics,
                    RecoveryTicketId,
                    DiagnosticsBootId,
                    4,
                    LMCSignalValueType.UInt32);
                var status = RecoveryStatus(
                    ticket,
                    LMCSignalValueType.UInt32,
                    expectedValue);

                D5SdoQualificationAnalysis.ValidateKnownValidRecovery(
                    ticket,
                    status,
                    DiagnosticsBootId,
                    LMCSignalValueType.UInt32,
                    expectedValue);
            }
        }

        private static void RecoveryGenericTypeRejected()
        {
            using (var connection = new LMCConnection())
            {
                var expectedValue = UInt32RecoveryValue();
                var ticket = SdoReadTicket(
                    connection.Diagnostics,
                    RecoveryTicketId,
                    DiagnosticsBootId,
                    4,
                    LMCSignalValueType.UInt32);
                var wrongTicketType = SdoReadTicket(
                    connection.Diagnostics,
                    RecoveryTicketId + 1,
                    DiagnosticsBootId,
                    4,
                    LMCSignalValueType.Int32);
                var wrongStatusType = RecoveryStatus(
                    ticket,
                    LMCSignalValueType.Int32,
                    expectedValue);

                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis
                        .ValidateKnownValidRecovery(
                            wrongTicketType,
                            RecoveryStatus(
                                wrongTicketType,
                                LMCSignalValueType.Int32,
                                expectedValue),
                            DiagnosticsBootId,
                            LMCSignalValueType.UInt32,
                            expectedValue));
                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis
                        .ValidateKnownValidRecovery(
                            ticket,
                            wrongStatusType,
                            DiagnosticsBootId,
                            LMCSignalValueType.UInt32,
                            expectedValue));
            }
        }

        private static void RecoveryGenericValueRejected()
        {
            using (var connection = new LMCConnection())
            {
                var expectedValue = UInt32RecoveryValue();
                var wrongValue = UInt32RecoveryValue();
                wrongValue[3] ^= 0x01;
                var ticket = SdoReadTicket(
                    connection.Diagnostics,
                    RecoveryTicketId,
                    DiagnosticsBootId,
                    4,
                    LMCSignalValueType.UInt32);

                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis
                        .ValidateKnownValidRecovery(
                            ticket,
                            RecoveryStatus(
                                ticket,
                                LMCSignalValueType.UInt32,
                                wrongValue),
                            DiagnosticsBootId,
                            LMCSignalValueType.UInt32,
                            expectedValue));
            }
        }

        private static void RecoveryGenericLengthRejected()
        {
            using (var connection = new LMCConnection())
            {
                var expectedValue = UInt32RecoveryValue();
                var ticket = SdoReadTicket(
                    connection.Diagnostics,
                    RecoveryTicketId,
                    DiagnosticsBootId,
                    4,
                    LMCSignalValueType.UInt32);
                var wrongReportedLength = Status(
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    0,
                    0,
                    3,
                    LMCSignalValueType.UInt32,
                    expectedValue);
                var wrongPayloadLength = Status(
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    0,
                    0,
                    4,
                    LMCSignalValueType.UInt32,
                    new byte[] { 0x78, 0x56, 0x34 });

                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis
                        .ValidateKnownValidRecovery(
                            ticket,
                            wrongReportedLength,
                            DiagnosticsBootId,
                            LMCSignalValueType.UInt32,
                            expectedValue));
                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis
                        .ValidateKnownValidRecovery(
                            ticket,
                            wrongPayloadLength,
                            DiagnosticsBootId,
                            LMCSignalValueType.UInt32,
                            expectedValue));
                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis
                        .ValidateKnownValidRecovery(
                            ticket,
                            RecoveryStatus(
                                ticket,
                                LMCSignalValueType.UInt32,
                                expectedValue),
                            DiagnosticsBootId,
                            LMCSignalValueType.UInt32,
                            new byte[] { 0x78, 0x56, 0x34 }));
            }
        }

        private static void LocalOrCommandFailureRejected()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = SdoReadTicket(
                    connection.Diagnostics,
                    AbortTicketId,
                    DiagnosticsBootId);
                var commandFailure = AbortStatus(
                    ticket,
                    SdoAbortCode,
                    FailedCommandResponse());

                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis.ValidateAbortTerminal(
                        ticket,
                        null,
                        DiagnosticsBootId));
                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoQualificationAnalysis.ValidateAbortTerminal(
                        ticket,
                        commandFailure,
                        DiagnosticsBootId));
            }
        }

        private static LMCOperationTicket SdoReadTicket(
            LMCDiagnostics diagnostics,
            uint ticketId,
            uint diagnosticsBootId,
            ushort requestedResultLength = 1,
            LMCSignalValueType resultValueType = LMCSignalValueType.Int8)
        {
            return new LMCOperationTicket(
                ticketId,
                LMCOperationKind.SDORead,
                10,
                diagnosticsBootId,
                1,
                diagnostics,
                true,
                requestedResultLength,
                resultValueType);
        }

        private static LMCOperationTicket SdoWriteTicket(
            LMCDiagnostics diagnostics,
            uint ticketId,
            uint diagnosticsBootId)
        {
            return new LMCOperationTicket(
                ticketId,
                LMCOperationKind.SDOWrite,
                10,
                diagnosticsBootId,
                1,
                diagnostics,
                false,
                0,
                LMCSignalValueType.Invalid);
        }

        private static LMCOperationStatus AbortStatus(
            LMCOperationTicket ticket,
            uint abortCode,
            LMCDiagnosticsResponse response = null)
        {
            return Status(
                ticket,
                LMCOperationState.Failed,
                LMCOperationOutcome.Failed,
                -32000,
                abortCode,
                0,
                LMCSignalValueType.Invalid,
                new byte[0],
                ticket.TicketId,
                ticket.DiagnosticsBootId,
                response);
        }

        private static LMCOperationStatus RecoveryStatus(
            LMCOperationTicket ticket,
            sbyte value,
            uint? ticketId = null,
            uint? diagnosticsBootId = null)
        {
            return Status(
                ticket,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                0,
                0,
                1,
                LMCSignalValueType.Int8,
                new byte[] { unchecked((byte)value) },
                ticketId ?? ticket.TicketId,
                diagnosticsBootId ?? ticket.DiagnosticsBootId);
        }

        private static LMCOperationStatus RecoveryStatus(
            LMCOperationTicket ticket,
            LMCSignalValueType valueType,
            byte[] value)
        {
            return Status(
                ticket,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                0,
                0,
                checked((uint)value.Length),
                valueType,
                value);
        }

        private static byte[] UInt32RecoveryValue()
        {
            return new byte[] { 0x78, 0x56, 0x34, 0x12 };
        }

        private static LMCOperationStatus Status(
            LMCOperationTicket ticket,
            LMCOperationState state,
            LMCOperationOutcome outcome,
            short operationErrorId,
            uint operationDetail,
            uint resultLength,
            LMCSignalValueType resultValueType,
            byte[] resultData,
            uint? ticketId = null,
            uint? diagnosticsBootId = null,
            LMCDiagnosticsResponse response = null)
        {
            return new LMCOperationStatus(
                response ?? SuccessfulCommandResponse(),
                ticketId ?? ticket.TicketId,
                ticket.OperationKind,
                state,
                10,
                state == LMCOperationState.Queued
                        || state == LMCOperationState.Running
                    ? 0u
                    : 11u,
                outcome,
                operationErrorId,
                operationDetail,
                resultLength,
                resultValueType,
                resultData,
                diagnosticsBootId ?? ticket.DiagnosticsBootId);
        }

        private static LMCDiagnosticsResponse SuccessfulCommandResponse()
        {
            return new LMCDiagnosticsResponse(
                new LMC_Response
                {
                    IsFrameValid = true,
                    HeaderStatus = 0
                },
                1,
                LMCDiagnosticsResponseFlags.None,
                0,
                0,
                0x11223344u,
                0);
        }

        private static LMCDiagnosticsResponse FailedCommandResponse()
        {
            return new LMCDiagnosticsResponse(
                new LMC_Response
                {
                    IsFrameValid = false,
                    HeaderStatus = 1
                },
                1,
                LMCDiagnosticsResponseFlags.None,
                1,
                -1,
                0x11223344u,
                (uint)LMCDiagnosticsDetailCode.InternalError);
        }
    }
}
