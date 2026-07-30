using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DriveReadFacadeTests
    {
        private const uint MapRevision = 0x957F101Eu;
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const ushort AxisReference = 1;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Result.AxisStatus.ReadSuccessVersusAxisError",
                AxisStatusReadSuccessVersusAxisError);
            tests.Add(
                "Rpc.DriveRead.OperationModeSyncAndAsync",
                OperationModeSyncAndAsync);
            tests.Add(
                "Rpc.DriveRead.CompositeSyncAndAsync",
                DriveStatusCompositeSyncAndAsync);
            tests.Add(
                "Rpc.DriveRead.TerminalFailurePreservesStatus",
                TerminalFailurePreservesStatus);
            tests.Add(
                "Rpc.DriveRead.TerminalWaitIsBounded",
                TerminalWaitIsBounded);
            tests.Add(
                "Rpc.DriveRead.CommandFailureExposesStageAndTicket",
                CommandFailureExposesStageAndTicket);
            tests.Add(
                "Rpc.DriveRead.NonDomainSubmitFailureRemainsUnknown",
                NonDomainSubmitFailureRemainsUnknown);
            tests.Add(
                "Rpc.DriveRead.NonDomainPreSubmissionContext",
                NonDomainPreSubmissionContext);
            tests.Add(
                "Rpc.DriveRead.NonDomainStatusContext",
                NonDomainStatusContext);
            tests.Add(
                "Rpc.DriveRead.CompositeSecondSubmitContext",
                CompositeSecondSubmitContext);
            tests.Add(
                "Rpc.DriveRead.PreSubmitCancellationContext",
                PreSubmitCancellationContext);
            tests.Add(
                "Rpc.DriveRead.ScopeAndStaleSession",
                ScopeAndStaleSession);
            tests.Add(
                "Rpc.DriveRead.AsyncCancellationPreservesTicket",
                AsyncCancellationPreservesTicket);
            tests.Add(
                "Rpc.DriveRead.StatusRpcCancellationPreservesConnection",
                StatusRpcCancellationPreservesConnection);
        }

        private static void AxisStatusReadSuccessVersusAxisError()
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, 0x00000020u);
            TestFrame.WriteUInt16(payload, 8, 0x0012);

            var result = LMCConnection.ParseReadStatusResult(
                TestFrame.Response(0, payload));

            AssertEx.True(result.IsReadSuccessful);
            AssertEx.True(result.HasAxisError);
            AssertEx.Equal((ushort)0x0012, result.AxisErrorFlags);
            AssertEx.False(result.IsSuccess);
        }

        private static void OperationModeSyncAndAsync()
        {
            RunOperationMode(false, TestFrame.Hex("08"));
            RunOperationMode(true, TestFrame.Hex("FE"));
        }

        private static void RunOperationMode(
            bool useAsync,
            byte[] resultData)
        {
            const uint ticketId = 0x11111111u;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CapabilitiesStep(1),
                SdoSubmitStep(
                    2,
                    ticketId,
                    0x6061,
                    LMCSignalValueType.Int8,
                    1,
                    100),
                OperationStatusStep(
                    3,
                    ticketId,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    LMCSignalValueType.Int8,
                    resultData),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var result = useAsync
                    ? axis.GetDriveOperationModeAsync(
                            100,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : axis.GetDriveOperationMode(100);

                AssertEx.Equal(AxisReference, result.AxisReference);
                AssertEx.Equal(unchecked((sbyte)resultData[0]), result.RawValue);
                AssertEx.Equal(ticketId, result.Ticket.TicketId);
                AssertEx.True(result.OperationStatus.IsSuccessful);
                AssertEx.Equal(resultData[0] == 8, result.IsKnownMode);
                AssertEx.Equal(result.IsKnownMode, result.IsDefined);
                if (resultData[0] == 8)
                {
                    AssertEx.Equal(
                        LMCDriveOperationMode.CyclicSynchronousPosition,
                        result.Mode);
                }

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void DriveStatusCompositeSyncAndAsync()
        {
            RunDriveStatusComposite(false, true);
            RunDriveStatusComposite(true, false);
        }

        private static void RunDriveStatusComposite(
            bool useAsync,
            bool hasDs402Fault)
        {
            const uint statusWordTicketId = 0x22222222u;
            const uint operationModeTicketId = 0x33333333u;
            var axisStatusPayload = new byte[12];
            TestFrame.WriteUInt32(axisStatusPayload, 0, 0x00000020u);
            TestFrame.WriteUInt16(axisStatusPayload, 8, 0x0012);
            TestFrame.WriteUInt16(axisStatusPayload, 10, 0);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                new FakeRpcStep(
                    0x2028,
                    TestFrame.Response(0, axisStatusPayload)),
                CapabilitiesStep(1),
                SdoSubmitStep(
                    2,
                    statusWordTicketId,
                    0x6041,
                    LMCSignalValueType.BitField16,
                    2,
                    100),
                OperationStatusStep(
                    3,
                    statusWordTicketId,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending,
                    LMCSignalValueType.Invalid,
                    new byte[0]),
                OperationStatusStep(
                    4,
                    statusWordTicketId,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending,
                    LMCSignalValueType.Invalid,
                    new byte[0]),
                OperationStatusStep(
                    5,
                    statusWordTicketId,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    LMCSignalValueType.BitField16,
                    TestFrame.Hex(hasDs402Fault ? "08 08" : "00 08")),
                CapabilitiesStep(6),
                SdoSubmitStep(
                    7,
                    operationModeTicketId,
                    0x6061,
                    LMCSignalValueType.Int8,
                    1,
                    100),
                OperationStatusStep(
                    8,
                    operationModeTicketId,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    LMCSignalValueType.Int8,
                    TestFrame.Hex("08")),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var status = useAsync
                    ? axis.ReadDriveStatusAsync(
                            100,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                    : axis.ReadDriveStatus(100);

                AssertEx.False(status.IsAtomicSnapshot);
                AssertEx.True(status.IsReadSuccessful);
                AssertEx.True(status.AxisStatus.IsReadSuccessful);
                AssertEx.True(status.AxisStatus.HasAxisError);
                AssertEx.False(status.AxisStatus.IsSuccess);
                AssertEx.Equal((ushort)0, status.AxisStatus.StatusWord);
                AssertEx.Equal(
                    (ushort)(hasDs402Fault ? 0x0808 : 0x0800),
                    status.Ds402StatusWord);
                AssertEx.Equal((ushort)0x0012, status.AxisErrorFlags);
                AssertEx.True(status.HasAxisError);
                AssertEx.Equal(hasDs402Fault, status.HasDs402Fault);
                AssertEx.True(status.IsLasalPositionLimitActive);
                AssertEx.True(status.HasSoftwareMinimumLimitError);
                AssertEx.False(status.HasSoftwareMaximumLimitError);
                AssertEx.False(status.HasHardwareMinimumLimitError);
                AssertEx.True(status.HasHardwareMaximumLimitError);
                AssertEx.True(status.IsDs402InternalLimitActive);
                AssertEx.True(status.HasAnyLimitIndication);
                AssertEx.Equal(
                    statusWordTicketId,
                    status.StatusWordTicket.TicketId);
                AssertEx.Equal(
                    operationModeTicketId,
                    status.OperationModeResult.Ticket.TicketId);
                AssertEx.Equal(
                    LMCDriveOperationMode.CyclicSynchronousPosition,
                    status.OperationMode);
                AssertEx.Equal((sbyte)8, status.OperationModeRaw);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void TerminalFailurePreservesStatus()
        {
            const uint ticketId = 0x44444444u;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CapabilitiesStep(1),
                SdoSubmitStep(
                    2,
                    ticketId,
                    0x6061,
                    LMCSignalValueType.Int8,
                    1,
                    100),
                OperationStatusStep(
                    3,
                    ticketId,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed,
                    LMCSignalValueType.Invalid,
                    new byte[0],
                    -55,
                    0x12345678u),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<LMCSdoReadOperationException>(
                    () => axis.GetDriveOperationMode(100));

                AssertEx.Equal(ticketId, exception.Ticket.TicketId);
                AssertEx.Equal(
                    LMCOperationState.Failed,
                    exception.OperationStatus.State);
                AssertEx.Equal((short)-55, exception.OperationStatus.OperationErrorId);
                AssertEx.Equal(
                    0x12345678u,
                    exception.OperationStatus.OperationDetail);
                var failureContext = RequireFailureContext(exception);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.StatusPolling,
                    failureContext.Phase);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.Accepted,
                    failureContext.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.Equal(
                    LMCSdoReadSubmissionOutcome.Accepted,
                    failureContext.CurrentSdoAttempt.SubmissionOutcome);
                AssertEx.True(failureContext.CurrentSdoAttempt.IsTerminal);
                AssertEx.True(ReferenceEquals(
                    exception.Ticket,
                    failureContext.CurrentSdoAttempt.Ticket));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void TerminalWaitIsBounded()
        {
            const uint ticketId = 0x55555555u;
            var pollLimit = LMCDiagnostics.GetInlineSdoTerminalPollLimit(1);
            AssertEx.Equal(33, pollLimit);
            AssertEx.Equal(
                60032,
                LMCDiagnostics.GetInlineSdoTerminalPollLimit(60000));
            AssertEx.Equal(
                1,
                LMCDiagnostics.GetInlineSdoPollDelayMilliseconds(1));
            AssertEx.Equal(
                1,
                LMCDiagnostics.GetInlineSdoPollDelayMilliseconds(1000));
            AssertEx.Equal(
                2,
                LMCDiagnostics.GetInlineSdoPollDelayMilliseconds(1001));
            AssertEx.Equal(
                4,
                LMCDiagnostics.GetInlineSdoPollDelayMilliseconds(4000));
            AssertEx.Throws<InvalidDataException>(
                () => LMCDiagnostics.GetInlineSdoPollDelayMilliseconds(0));

            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CapabilitiesStep(1, 4000),
                SdoSubmitStep(
                    2,
                    ticketId,
                    0x6061,
                    LMCSignalValueType.Int8,
                    1,
                    1)
            };

            for (var poll = 0; poll < pollLimit; poll++)
            {
                steps.Add(OperationStatusStep(
                    checked((uint)(3 + poll)),
                    ticketId,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending,
                    LMCSignalValueType.Invalid,
                    new byte[0]));
            }

            steps.Add(CloseStep());

            using (var server = new FakeRpcServer(steps.ToArray()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<
                    LMCSdoReadPollingTimeoutException>(
                    () => axis.GetDriveOperationMode(1));

                AssertEx.Equal(ticketId, exception.Ticket.TicketId);
                AssertEx.Equal(33, exception.PollCount);
                AssertEx.Contains("after 33 status polls", exception.Message);
                AssertEx.Contains("was not cancelled", exception.Message);
                AssertEx.Contains(ticketId.ToString(), exception.Message);
                var failureContext = RequireFailureContext(exception);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.Accepted,
                    failureContext.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.False(failureContext.CurrentSdoAttempt.IsTerminal);
                AssertEx.True(ReferenceEquals(
                    exception.Ticket,
                    failureContext.CurrentSdoAttempt.Ticket));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void CommandFailureExposesStageAndTicket()
        {
            AssertCommandFailureContract(
                false,
                LMCSdoReadCommandStage.CapabilityPreflight);
            AssertCommandFailureContract(
                true,
                LMCSdoReadCommandStage.CapabilityPreflight);
            AssertCommandFailureContract(
                false,
                LMCSdoReadCommandStage.Submission);
            AssertCommandFailureContract(
                true,
                LMCSdoReadCommandStage.Submission);
            AssertCommandFailureContract(
                false,
                LMCSdoReadCommandStage.StatusPolling);
            AssertCommandFailureContract(
                true,
                LMCSdoReadCommandStage.StatusPolling);
            AssertCompositeSecondStatusFailureContract();
        }

        private static void AssertCommandFailureContract(
            bool useAsync,
            LMCSdoReadCommandStage expectedStage)
        {
            const uint ticketId = 0x51525354u;
            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep()
            };

            if (expectedStage == LMCSdoReadCommandStage.CapabilityPreflight)
            {
                steps.Add(DomainErrorStep(
                    0x7E00,
                    1,
                    LMCDiagnosticsDetailCode.ResourceBusy,
                    68));
            }
            else
            {
                steps.Add(CapabilitiesStep(1));
            }

            if (expectedStage == LMCSdoReadCommandStage.StatusPolling)
            {
                steps.Add(SdoSubmitStep(
                    2,
                    ticketId,
                    0x6061,
                    LMCSignalValueType.Int8,
                    1,
                    100));
                steps.Add(DomainErrorStep(
                    0x7E03,
                    3,
                    LMCDiagnosticsDetailCode.ResourceBusy));
            }
            else if (expectedStage == LMCSdoReadCommandStage.Submission)
            {
                steps.Add(DomainErrorStep(
                    0x7E50,
                    2,
                    LMCDiagnosticsDetailCode.ResourceBusy));
            }

            steps.Add(CloseStep());

            using (var server = new FakeRpcServer(steps.ToArray()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<LMCSdoReadCommandException>(
                    () =>
                    {
                        if (useAsync)
                        {
                            axis.GetDriveOperationModeAsync(
                                    100,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        }
                        else
                        {
                            axis.GetDriveOperationMode(100);
                        }
                    });

                AssertEx.Equal(expectedStage, exception.Stage);
                AssertEx.NotNull(exception.Response);
                AssertEx.Equal(
                    LMCDiagnosticsDetailCode.ResourceBusy,
                    exception.Response.Detail);
                AssertEx.True(
                    exception.InnerException is LMCDiagnosticsCommandException);
                var innerException =
                    (LMCDiagnosticsCommandException)exception.InnerException;
                AssertEx.True(ReferenceEquals(
                    exception.Response,
                    innerException.Response));
                LMCDiagnosticsCommandException compatibleException = exception;
                AssertEx.Equal(exception.Response, compatibleException.Response);
                AssertEx.Contains(
                    "Stage=" + expectedStage,
                    exception.Message);

                if (expectedStage == LMCSdoReadCommandStage.StatusPolling)
                {
                    AssertEx.NotNull(exception.Ticket);
                    AssertEx.Equal(ticketId, exception.Ticket.TicketId);
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        exception.Ticket.DiagnosticsBootId);
                    AssertEx.Equal(
                        LMCOperationKind.SDORead,
                        exception.Ticket.OperationKind);
                    AssertEx.Contains(ticketId.ToString(), exception.Message);
                }
                else
                {
                    AssertEx.True(exception.Ticket == null);
                }

                var failureContext = RequireFailureContext(exception);
                AssertEx.Equal(1, failureContext.SdoAttempts.Count);
                var expectedPhase = expectedStage
                    == LMCSdoReadCommandStage.CapabilityPreflight
                    ? LMCDriveReadAttemptPhase.CapabilityPreflight
                    : expectedStage == LMCSdoReadCommandStage.Submission
                        ? LMCDriveReadAttemptPhase.Submission
                        : LMCDriveReadAttemptPhase.StatusPolling;
                AssertEx.Equal(expectedPhase, failureContext.Phase);
                var expectedOutcome = expectedStage
                    == LMCSdoReadCommandStage.CapabilityPreflight
                    ? LMCSdoSubmissionOutcome.NotAttempted
                    : expectedStage == LMCSdoReadCommandStage.Submission
                        ? LMCSdoSubmissionOutcome.Rejected
                        : LMCSdoSubmissionOutcome.Accepted;
                AssertEx.Equal(
                    expectedOutcome,
                    failureContext.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.Equal(
                    exception.Ticket,
                    failureContext.CurrentSdoAttempt.Ticket);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertCompositeSecondStatusFailureContract()
        {
            const uint statusWordTicketId = 0x61626364u;
            const uint operationModeTicketId = 0x71727374u;
            var axisStatusPayload = new byte[12];

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                new FakeRpcStep(
                    0x2028,
                    TestFrame.Response(0, axisStatusPayload)),
                CapabilitiesStep(1),
                SdoSubmitStep(
                    2,
                    statusWordTicketId,
                    0x6041,
                    LMCSignalValueType.BitField16,
                    2,
                    100),
                OperationStatusStep(
                    3,
                    statusWordTicketId,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    LMCSignalValueType.BitField16,
                    TestFrame.Hex("00 08")),
                CapabilitiesStep(4),
                SdoSubmitStep(
                    5,
                    operationModeTicketId,
                    0x6061,
                    LMCSignalValueType.Int8,
                    1,
                    100),
                DomainErrorStep(
                    0x7E03,
                    6,
                    LMCDiagnosticsDetailCode.ResourceBusy),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<LMCSdoReadCommandException>(
                    () => axis.ReadDriveStatusAsync(
                            100,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCSdoReadCommandStage.StatusPolling,
                    exception.Stage);
                AssertEx.NotNull(exception.Ticket);
                AssertEx.Equal(
                    operationModeTicketId,
                    exception.Ticket.TicketId);
                var failureContext = RequireFailureContext(exception);
                AssertEx.Equal(2, failureContext.SdoAttempts.Count);
                AssertEx.True(failureContext.SdoAttempts[0].IsTerminal);
                AssertEx.Equal(
                    (ushort)0x6041,
                    failureContext.SdoAttempts[0].Request.ObjectIndex);
                AssertEx.Equal(
                    (ushort)0x6061,
                    failureContext.SdoAttempts[1].Request.ObjectIndex);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.Accepted,
                    failureContext.SdoAttempts[1].GenericSubmissionOutcome);
                AssertEx.False(failureContext.SdoAttempts[1].IsTerminal);
                AssertEx.True(ReferenceEquals(
                    exception.Ticket,
                    failureContext.SdoAttempts[1].Ticket));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void NonDomainSubmitFailureRemainsUnknown()
        {
            AssertNonDomainSubmitFailure(false, false);
            AssertNonDomainSubmitFailure(true, true);
        }

        private static void AssertNonDomainSubmitFailure(
            bool useAsync,
            bool responseLoss)
        {
            FakeRpcStep submitStep;
            if (responseLoss)
            {
                submitStep = new FakeRpcStep(0x7E50, new byte[0])
                {
                    CloseAfterResponse = true
                };
            }
            else
            {
                submitStep = new FakeRpcStep(
                    0x7E50,
                    TestFrame.Response(0, CommonPayload(31, 2)));
            }

            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CapabilitiesStep(1),
                submitStep
            };
            if (!responseLoss)
            {
                steps.Add(CloseStep());
            }

            using (var server = new FakeRpcServer(steps.ToArray()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<Exception>(
                    () =>
                    {
                        if (useAsync)
                        {
                            axis.GetDriveOperationModeAsync(
                                    100,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        }
                        else
                        {
                            axis.GetDriveOperationMode(100);
                        }
                    });

                AssertEx.False(exception is LMCSdoReadCommandException);
                var failureContext = RequireFailureContext(exception);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.Submission,
                    failureContext.Phase);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.OutcomeUncertain,
                    failureContext.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.True(failureContext.CurrentSdoAttempt.Ticket == null);
                if (!responseLoss)
                {
                    AssertEx.True(exception is InvalidDataException);
                    connection.CloseConnection();
                }

                server.Verify();
            }
        }

        private static void NonDomainPreSubmissionContext()
        {
            AssertMalformedCapabilityContext(false);
            AssertMalformedCapabilityContext(true);
            AssertAxisStatusFailureContext();
        }

        private static void AssertMalformedCapabilityContext(bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(0, CommonPayload(67, 1))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<InvalidDataException>(
                    () =>
                    {
                        if (useAsync)
                        {
                            axis.GetDriveOperationModeAsync(
                                    100,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        }
                        else
                        {
                            axis.GetDriveOperationMode(100);
                        }
                    });

                var context = RequireFailureContext(exception);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.CapabilityPreflight,
                    context.Phase);
                AssertEx.Equal(1, context.SdoAttempts.Count);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.NotAttempted,
                    context.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.True(context.CurrentSdoAttempt.Ticket == null);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertAxisStatusFailureContext()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                new FakeRpcStep(
                    0x2028,
                    TestFrame.Response(0, new byte[0])),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<InvalidDataException>(
                    () => axis.ReadDriveStatus(100));
                var context = RequireFailureContext(exception);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.AxisStatusRead,
                    context.Phase);
                AssertEx.False(context.AxisStatusReadCompleted);
                AssertEx.Equal(0, context.SdoAttempts.Count);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void NonDomainStatusContext()
        {
            AssertMalformedStatusContext(false);
            AssertMalformedStatusContext(true);
            AssertLostStatusContext();
        }

        private static void AssertLostStatusContext()
        {
            const uint ticketId = 0x85868788u;
            var lostStatus = new FakeRpcStep(0x7E03, new byte[0])
            {
                CloseAfterResponse = true
            };
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CapabilitiesStep(1),
                SdoSubmitStep(
                    2,
                    ticketId,
                    0x6061,
                    LMCSignalValueType.Int8,
                    1,
                    100),
                lostStatus))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<Exception>(
                    () => axis.GetDriveOperationModeAsync(
                            100,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.False(exception is LMCSdoReadCommandException);
                var context = RequireFailureContext(exception);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.StatusPolling,
                    context.Phase);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.Accepted,
                    context.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.Equal(
                    ticketId,
                    context.CurrentSdoAttempt.Ticket.TicketId);
                AssertEx.True(
                    context.CurrentSdoAttempt.LastOperationStatus == null);

                server.Verify();
            }
        }

        private static void AssertMalformedStatusContext(bool useAsync)
        {
            const uint ticketId = 0x81828384u;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CapabilitiesStep(1),
                SdoSubmitStep(
                    2,
                    ticketId,
                    0x6061,
                    LMCSignalValueType.Int8,
                    1,
                    100),
                new FakeRpcStep(
                    0x7E03,
                    TestFrame.Response(0, CommonPayload(63, 3))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<InvalidDataException>(
                    () =>
                    {
                        if (useAsync)
                        {
                            axis.GetDriveOperationModeAsync(
                                    100,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        }
                        else
                        {
                            axis.GetDriveOperationMode(100);
                        }
                    });

                var context = RequireFailureContext(exception);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.StatusPolling,
                    context.Phase);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.Accepted,
                    context.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.Equal(
                    ticketId,
                    context.CurrentSdoAttempt.Ticket.TicketId);
                AssertEx.True(
                    context.CurrentSdoAttempt.LastOperationStatus == null);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void CompositeSecondSubmitContext()
        {
            AssertCompositeSecondSubmitContext(false);
            AssertCompositeSecondSubmitContext(true);
        }

        private static void PreSubmitCancellationContext()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");
                cancellation.Cancel();

                var exception = AssertEx.Throws<OperationCanceledException>(
                    () => axis.GetDriveOperationModeAsync(
                            100,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.False(exception is LMCSdoReadWaitCanceledException);
                var context = RequireFailureContext(exception);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.CapabilityPreflight,
                    context.Phase);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.NotAttempted,
                    context.CurrentSdoAttempt.GenericSubmissionOutcome);
                AssertEx.True(context.CurrentSdoAttempt.Ticket == null);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AssertCompositeSecondSubmitContext(bool useAsync)
        {
            const uint statusWordTicketId = 0x91929394u;
            var axisStatusPayload = new byte[12];
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                new FakeRpcStep(
                    0x2028,
                    TestFrame.Response(0, axisStatusPayload)),
                CapabilitiesStep(1),
                SdoSubmitStep(
                    2,
                    statusWordTicketId,
                    0x6041,
                    LMCSignalValueType.BitField16,
                    2,
                    100),
                OperationStatusStep(
                    3,
                    statusWordTicketId,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    LMCSignalValueType.BitField16,
                    TestFrame.Hex("00 08")),
                CapabilitiesStep(4),
                new FakeRpcStep(
                    0x7E50,
                    TestFrame.Response(0, CommonPayload(31, 5))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var axis = new LMCAxis(connection, "_LMCAxis1");

                var exception = AssertEx.Throws<InvalidDataException>(
                    () =>
                    {
                        if (useAsync)
                        {
                            axis.ReadDriveStatusAsync(
                                    100,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        }
                        else
                        {
                            axis.ReadDriveStatus(100);
                        }
                    });

                var context = RequireFailureContext(exception);
                AssertEx.True(context.AxisStatusReadCompleted);
                AssertEx.Equal(2, context.SdoAttempts.Count);
                AssertEx.True(context.SdoAttempts[0].IsTerminal);
                AssertEx.Equal(
                    LMCSdoSubmissionOutcome.OutcomeUncertain,
                    context.SdoAttempts[1].GenericSubmissionOutcome);
                AssertEx.Equal(
                    (ushort)0x6061,
                    context.SdoAttempts[1].Request.ObjectIndex);
                AssertEx.True(context.SdoAttempts[1].Ticket == null);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void ScopeAndStaleSession()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(5),
                AxisInfoStep(5),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server);
                var unsupportedAxis = new LMCAxis(
                    connection,
                    "_LMCAxis5");

                AssertEx.Throws<NotSupportedException>(
                    () => unsupportedAxis.GetDriveOperationMode(100));

                connection.CloseConnection();
                server.Verify();
            }

            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                AxisLookupStep(),
                AxisInfoStep(),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, firstServer);
                var staleAxis = new LMCAxis(connection, "_LMCAxis1");

                connection.RpcInitConnection(
                    "127.0.0.1",
                    secondServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => staleAxis.GetDriveOperationMode(100));
                AssertEx.Contains("inactive RPC session", exception.Message);
                var failureContext = RequireFailureContext(exception);
                AssertEx.Equal(
                    LMCDriveReadAttemptPhase.FacadePreflight,
                    failureContext.Phase);
                AssertEx.Equal(0, failureContext.SdoAttempts.Count);

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void AsyncCancellationPreservesTicket()
        {
            const uint ticketId = 0x66666666u;
            using (var cancellation = new CancellationTokenSource())
            {
                var pendingStatus = OperationStatusStep(
                    3,
                    ticketId,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending,
                    LMCSignalValueType.Invalid,
                    new byte[0]);
                pendingStatus.AfterResponse = request =>
                {
                    Thread.Sleep(20);
                    cancellation.Cancel();
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    AxisLookupStep(),
                    AxisInfoStep(),
                    CapabilitiesStep(1, 100000),
                    SdoSubmitStep(
                        2,
                        ticketId,
                        0x6061,
                        LMCSignalValueType.Int8,
                        1,
                        100),
                    pendingStatus,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    Connect(connection, server);
                    var axis = new LMCAxis(connection, "_LMCAxis1");

                    var exception = AssertEx.Throws<
                        LMCSdoReadWaitCanceledException>(
                        () => axis.GetDriveOperationModeAsync(
                                100,
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());

                    AssertEx.Equal(ticketId, exception.Ticket.TicketId);
                    AssertEx.True(exception.CancellationToken.IsCancellationRequested);
                    AssertEx.Contains("was not cancelled", exception.Message);
                    var failureContext = RequireFailureContext(exception);
                    AssertEx.Equal(
                        LMCSdoSubmissionOutcome.Accepted,
                        failureContext.CurrentSdoAttempt.GenericSubmissionOutcome);
                    AssertEx.False(failureContext.CurrentSdoAttempt.IsTerminal);
                    AssertEx.True(ReferenceEquals(
                        exception.Ticket,
                        failureContext.CurrentSdoAttempt.Ticket));

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void StatusRpcCancellationPreservesConnection()
        {
            const uint ticketId = 0x77777777u;
            using (var cancellation = new CancellationTokenSource())
            {
                var pendingStatus = OperationStatusStep(
                    3,
                    ticketId,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending,
                    LMCSignalValueType.Invalid,
                    new byte[0]);
                pendingStatus.ResponseDelayMilliseconds = 50;
                pendingStatus.AllowClientDisconnectAfterRequest = true;
                pendingStatus.InspectRequest = request =>
                    cancellation.Cancel();

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    AxisLookupStep(),
                    AxisInfoStep(),
                    CapabilitiesStep(1, 100000),
                    SdoSubmitStep(
                        2,
                        ticketId,
                        0x6061,
                        LMCSignalValueType.Int8,
                        1,
                        100),
                    pendingStatus,
                    OperationStatusStep(
                        4,
                        ticketId,
                        LMCOperationState.Completed,
                        LMCOperationOutcome.Success,
                        LMCSignalValueType.Int8,
                        TestFrame.Hex("08")),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    Connect(connection, server);
                    var axis = new LMCAxis(connection, "_LMCAxis1");

                    var exception = AssertEx.Throws<
                        LMCSdoReadWaitCanceledException>(
                        () => axis.GetDriveOperationModeAsync(
                                100,
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());

                    AssertEx.Equal(ticketId, exception.Ticket.TicketId);
                    AssertEx.True(connection.IsConnected);
                    AssertEx.Equal(
                        LMCConnectionState.Connected,
                        connection.State);
                    var failureContext = RequireFailureContext(exception);
                    AssertEx.Equal(
                        LMCSdoSubmissionOutcome.Accepted,
                        failureContext.CurrentSdoAttempt.GenericSubmissionOutcome);
                    AssertEx.False(failureContext.CurrentSdoAttempt.IsTerminal);

                    var status = connection.Diagnostics.GetOperationStatus(
                        exception.Ticket);
                    AssertEx.True(status.IsSuccessful);
                    AssertEx.Equal(
                        LMCOperationState.Completed,
                        status.State);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void Connect(
            LMCConnection connection,
            FakeRpcServer server)
        {
            connection.RpcInitConnection(
                "127.0.0.1",
                server.Port,
                "127.0.0.1",
                0,
                LMCConnection.DefaultEventMask);
        }

        private static LMCDriveReadFailureContext RequireFailureContext(
            Exception exception)
        {
            LMCDriveReadFailureContext context;
            AssertEx.True(
                LMCDriveReadFailureContext.TryGet(exception, out context),
                "Expected typed drive-read failure context.");
            AssertEx.NotNull(context);
            return context;
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return new FakeRpcStep(0x8080, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CallbackStep()
        {
            return new FakeRpcStep(
                0x405C,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep AxisLookupStep(
            ushort axisReference = AxisReference)
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, axisReference);
            return new FakeRpcStep(
                0x103C,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisInfoStep(
            ushort axisReference = AxisReference)
        {
            var payload = new byte[8];
            TestFrame.WriteUInt32(payload, 0, axisReference);
            return new FakeRpcStep(
                0x202B,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CapabilitiesStep(
            uint requestId,
            uint baseCycleTimeUs = 1000)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 5);
            TestFrame.WriteUInt32(
                payload,
                20,
                (uint)(LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline));
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 40, baseCycleTimeUs);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 60, 4);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep SdoSubmitStep(
            uint requestId,
            uint ticketId,
            ushort objectIndex,
            LMCSignalValueType valueType,
            ushort dataLength,
            uint timeoutCycles)
        {
            var payload = CommonPayload(32, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCOperationKind.SDORead);
            TestFrame.WriteUInt16(
                payload,
                22,
                (ushort)LMCOperationState.Queued);
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(payload, 28, DiagnosticsBootId);

            return new FakeRpcStep(
                0x7E50,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal(AxisReference, TestFrame.ReadUInt16(request, 20));
                    AssertEx.Equal(objectIndex, TestFrame.ReadUInt16(request, 24));
                    AssertEx.Equal((byte)0, request[26]);
                    AssertEx.Equal((byte)valueType, request[27]);
                    AssertEx.Equal(timeoutCycles, TestFrame.ReadUInt32(request, 28));
                    AssertEx.Equal(dataLength, TestFrame.ReadUInt16(request, 32));
                }
            };
        }

        private static FakeRpcStep OperationStatusStep(
            uint requestId,
            uint ticketId,
            LMCOperationState state,
            LMCOperationOutcome outcome,
            LMCSignalValueType resultType,
            byte[] resultData,
            short operationErrorId = 0,
            uint operationDetail = 0)
        {
            var safeData = resultData ?? new byte[0];
            var payload = CommonPayload(64, requestId);
            TestFrame.WriteUInt32(payload, 16, ticketId);
            TestFrame.WriteUInt16(
                payload,
                20,
                (ushort)LMCOperationKind.SDORead);
            TestFrame.WriteUInt16(payload, 22, (ushort)state);
            TestFrame.WriteUInt32(payload, 24, 100);
            TestFrame.WriteUInt32(
                payload,
                28,
                state == LMCOperationState.Queued
                        || state == LMCOperationState.Running
                    ? 0u
                    : 200u);
            TestFrame.WriteUInt16(payload, 32, (ushort)outcome);
            TestFrame.WriteInt16(payload, 34, operationErrorId);
            TestFrame.WriteUInt32(payload, 36, operationDetail);
            TestFrame.WriteUInt32(
                payload,
                40,
                outcome == LMCOperationOutcome.Success
                    ? checked((uint)safeData.Length)
                    : 0u);
            payload[44] = (byte)resultType;
            payload[45] = checked((byte)safeData.Length);
            Buffer.BlockCopy(safeData, 0, payload, 48, safeData.Length);
            TestFrame.WriteUInt32(payload, 60, DiagnosticsBootId);

            return new FakeRpcStep(
                0x7E03,
                TestFrame.Response(0, payload));
        }

        private static byte[] CommonPayload(int length, uint requestId)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static FakeRpcStep DomainErrorStep(
            ushort commandId,
            uint requestId,
            LMCDiagnosticsDetailCode detail,
            int payloadLength = 16)
        {
            var payload = CommonPayload(payloadLength, requestId);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -32000);
            TestFrame.WriteUInt32(payload, 12, (uint)detail);
            return new FakeRpcStep(
                commandId,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }
    }
}
