using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class
        D5SdoWriteSameValueQualificationOrchestratorTests
    {
        private const uint DiagnosticsBootId = 0x7A5A0001u;
        private const uint MapRevision = 0x957F101Eu;
        private static readonly byte[] BaselineData =
        {
            0x78,
            0x56,
            0x34,
            0x12
        };

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.D5SdoWriteSameValue.HappyPathOrderedExact",
                HappyPathOrderedExact);
            tests.Add(
                "Qualification.D5SdoWriteSameValue.PreflightZeroWire",
                PreflightZeroWire);
            tests.Add(
                "Qualification.D5SdoWriteSameValue.MutationGatesBlockWrite",
                MutationGatesBlockWrite);
            tests.Add(
                "Qualification.D5SdoWriteSameValue.SecondSafetyBlocksMutation",
                SecondSafetyBlocksMutation);
            tests.Add(
                "Qualification.D5SdoWriteSameValue.PreWriteChangeBlocksMutation",
                PreWriteChangeBlocksMutation);
            tests.Add(
                "Qualification.D5SdoWriteSameValue.WriteTicketAdoptedBeforeValidation",
                WriteTicketAdoptedBeforeValidation);
            tests.Add(
                "Qualification.D5SdoWriteSameValue.UncertainWriteNeverReplays",
                UncertainWriteNeverReplays);
            tests.Add(
                "Qualification.D5SdoWriteSameValue.ReadbackMismatchUnresolved",
                ReadbackMismatchUnresolved);
            tests.Add(
                "Qualification.D5SdoWriteSameValue.CancelAfterArmUnresolved",
                CancelAfterArmUnresolved);
        }

        private static void HappyPathOrderedExact()
        {
            using (var harness = new Harness())
            {
                var result = Run(
                    harness.Request,
                    harness.CreateOperations());
                var scope = result.RecoveryScope;

                AssertEx.Equal("COMPLETE", scope.Stage);
                AssertEx.True(scope.SafetyVerified);
                AssertEx.True(scope.ConfirmationAccepted);
                AssertEx.True(scope.SecondSafetyVerified);
                AssertEx.True(scope.JournalArmed);
                AssertEx.True(scope.ReadbackVerified);
                AssertEx.True(scope.JournalResolved);
                AssertEx.False(scope.HasUncertainSubmissionOutcome);
                AssertEx.Equal(4, harness.AcceptedTickets.Count);
                for (var left = 0;
                    left < harness.AcceptedTickets.Count;
                    left++)
                {
                    for (var right = left + 1;
                        right < harness.AcceptedTickets.Count;
                        right++)
                    {
                        AssertEx.True(
                            harness.AcceptedTickets[left].TicketId
                                != harness.AcceptedTickets[right].TicketId);
                    }
                }
                AssertEx.SequenceEqual(
                    BaselineData,
                    scope.BaselineData);
                AssertEx.SequenceEqual(
                    BaselineData,
                    scope.WriteRequest.WriteData);
                AssertEx.SequenceEqual(
                    BaselineData,
                    scope.ReadbackStatus.ResultData);
                AssertEvents(
                    harness.Events,
                    "SUBMIT_BASELINE",
                    "WAIT_BASELINE",
                    "READ_CAPABILITIES_2",
                    "VERIFY_SAFE_AXIS_1",
                    "CONFIRM_WRITE",
                    "SUBMIT_PREWRITE_GUARD",
                    "WAIT_PREWRITE_GUARD",
                    "VERIFY_SAFE_AXIS_2",
                    "ARM_JOURNAL",
                    "SUBMIT_WRITE",
                    "ADOPT_WRITE_TICKET",
                    "MARK_WRITE_ACCEPTED",
                    "WAIT_WRITE",
                    "MARK_WRITE_TERMINAL",
                    "CREATE_CONTEXT",
                    "SUBMIT_READBACK",
                    "WAIT_READBACK",
                    "READ_CAPABILITIES_3",
                    "RESOLVE_JOURNAL");
                AssertEx.Equal(0, harness.RecoveryCount);
            }
        }

        private static void PreflightZeroWire()
        {
            using (var emptyAllowlist = new Harness())
            {
                var request = emptyAllowlist.CreateRequest(
                    new LMCSdoWriteTarget[0],
                    emptyAllowlist.InitialCapabilities);
                AssertEx.Throws<NotSupportedException>(
                    () => Run(request, emptyAllowlist.CreateOperations()));
                AssertZeroWire(emptyAllowlist);
                AssertEx.Equal(1, emptyAllowlist.RecoveryCount);
                AssertEx.Equal("PREFLIGHT", emptyAllowlist.RecoveryScope.Stage);
            }

            using (var capabilityOff = new Harness())
            {
                var capabilities = capabilityOff.Capabilities(
                    1,
                    RequiredCapabilities
                        & ~LMCDiagnosticCapability.SDOWrite);
                var request = capabilityOff.CreateRequest(
                    new[] { capabilityOff.Target },
                    capabilities);
                AssertEx.Throws<InvalidOperationException>(
                    () => Run(request, capabilityOff.CreateOperations()));
                AssertZeroWire(capabilityOff);
                AssertEx.Equal(1, capabilityOff.RecoveryCount);
            }

            using (var wrongSelectedInstance = new Harness())
            {
                var equalButUnapproved = Target();
                var request = new
                    D5SdoWriteSameValueQualificationRequest(
                        wrongSelectedInstance.Connection,
                        wrongSelectedInstance.InitialCapabilities,
                        new[] { wrongSelectedInstance.Target },
                        equalButUnapproved,
                        100);
                AssertEx.Throws<NotSupportedException>(
                    () => Run(
                        request,
                        wrongSelectedInstance.CreateOperations()));
                AssertZeroWire(wrongSelectedInstance);
            }
        }

        private static void MutationGatesBlockWrite()
        {
            using (var baselineFailure = new Harness())
            {
                baselineFailure.BaselineDataOverride = new byte[]
                {
                    0x78,
                    0x56,
                    0x34
                };
                AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        baselineFailure.Request,
                        baselineFailure.CreateOperations()));
                AssertEx.Equal(1, baselineFailure.GeneralSubmitCount);
                AssertEx.Equal(0, baselineFailure.SafetyCount);
                AssertEx.Equal(0, baselineFailure.ArmCount);
                AssertEx.Equal(0, baselineFailure.WriteSubmitCount);
            }

            using (var unsafeAxis = new Harness())
            {
                unsafeAxis.SafetyResult = false;
                AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        unsafeAxis.Request,
                        unsafeAxis.CreateOperations()));
                AssertEx.Equal(1, unsafeAxis.SafetyCount);
                AssertEx.Equal(0, unsafeAxis.ConfirmationCount);
                AssertEx.Equal(0, unsafeAxis.ArmCount);
                AssertEx.Equal(0, unsafeAxis.WriteSubmitCount);
            }

            using (var declined = new Harness())
            {
                declined.ConfirmationResult = false;
                AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        declined.Request,
                        declined.CreateOperations()));
                AssertEx.Equal(1, declined.ConfirmationCount);
                AssertEx.Equal(0, declined.ArmCount);
                AssertEx.Equal(0, declined.WriteSubmitCount);
            }

            using (var journalFailure = new Harness())
            {
                journalFailure.ArmError = new InvalidOperationException(
                    "durable arm failed");
                AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        journalFailure.Request,
                        journalFailure.CreateOperations()));
                AssertEx.Equal(1, journalFailure.ArmCount);
                AssertEx.Equal(0, journalFailure.WriteSubmitCount);
                AssertEx.True(
                    journalFailure.RecoveryScope.JournalArmAttempted);
                AssertEx.False(journalFailure.RecoveryScope.JournalArmed);
            }
        }

        private static void SecondSafetyBlocksMutation()
        {
            using (var harness = new Harness())
            {
                harness.SecondSafetyResult = false;

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        harness.Request,
                        harness.CreateOperations()));

                AssertEx.Equal(2, harness.SafetyCount);
                AssertEx.Equal(1, harness.ConfirmationCount);
                AssertEx.Equal(2, harness.GeneralSubmitCount);
                AssertEx.Equal(2, harness.WaitCount);
                AssertEx.Equal(0, harness.ArmCount);
                AssertEx.Equal(0, harness.WriteSubmitCount);
                AssertEx.True(harness.RecoveryScope.SafetyVerified);
                AssertEx.True(
                    harness.RecoveryScope.ConfirmationAccepted);
                AssertEx.False(
                    harness.RecoveryScope.SecondSafetyVerified);
                AssertEx.True(
                    harness.RecoveryScope
                        .PreWriteGuardSubmitAttempted);
                AssertEx.NotNull(
                    harness.RecoveryScope.PreWriteGuardTicket);
                AssertEx.NotNull(
                    harness.RecoveryScope.PreWriteGuardStatus);
                AssertEx.False(harness.RecoveryScope.JournalArmed);
            }
        }

        private static void PreWriteChangeBlocksMutation()
        {
            using (var harness = new Harness())
            {
                harness.PreWriteDataOverride = new byte[]
                {
                    0x79,
                    0x56,
                    0x34,
                    0x12
                };

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        harness.Request,
                        harness.CreateOperations()));

                AssertEx.Equal(1, harness.SafetyCount);
                AssertEx.Equal(2, harness.GeneralSubmitCount);
                AssertEx.Equal(2, harness.WaitCount);
                AssertEx.Equal(0, harness.ArmCount);
                AssertEx.Equal(0, harness.WriteSubmitCount);
                AssertEx.Equal(0, harness.AdoptWriteTicketCount);
                AssertEx.False(
                    harness.RecoveryScope.SecondSafetyVerified);
                AssertEx.True(
                    harness.RecoveryScope
                        .PreWriteGuardSubmitAttempted);
                AssertEx.NotNull(
                    harness.RecoveryScope.PreWriteGuardTicket);
                AssertEx.NotNull(
                    harness.RecoveryScope.PreWriteGuardStatus);
                AssertEx.False(harness.RecoveryScope.JournalArmAttempted);
                AssertEx.False(harness.RecoveryScope.JournalArmed);
            }
        }

        private static void WriteTicketAdoptedBeforeValidation()
        {
            using (var nullTicket = new Harness())
            {
                nullTicket.ReturnNullWriteTicket = true;

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        nullTicket.Request,
                        nullTicket.CreateOperations()));

                AssertEx.Equal(1, nullTicket.WriteSubmitCount);
                AssertEx.Equal(1, nullTicket.AdoptWriteTicketCount);
                AssertEx.Equal(0, nullTicket.MarkWriteAcceptedCount);
                AssertEx.Equal(2, nullTicket.WaitCount);
                AssertEx.True(nullTicket.RecoveryScope.JournalArmed);
                AssertEx.True(
                    nullTicket.RecoveryScope.WriteSubmitAttempted);
                AssertEx.True(
                    nullTicket.RecoveryScope.WriteTicket == null);
                AssertEx.Equal(
                    "ADOPT_WRITE_TICKET",
                    nullTicket.Events[nullTicket.Events.Count - 1]);
                AssertEx.Equal(1, nullTicket.RecoveryCount);
            }

            using (var adoptionFailure = new Harness())
            {
                var expected = new InvalidOperationException(
                    "durable Write ticket adoption failed");
                adoptionFailure.AdoptWriteTicketError = expected;

                var observed = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        adoptionFailure.Request,
                        adoptionFailure.CreateOperations()));

                AssertEx.True(ReferenceEquals(expected, observed));
                AssertEx.Equal(
                    1,
                    adoptionFailure.AdoptWriteTicketCount);
                AssertEx.Equal(
                    0,
                    adoptionFailure.MarkWriteAcceptedCount);
                AssertEx.Equal(2, adoptionFailure.WaitCount);
                AssertEx.True(
                    adoptionFailure.RecoveryScope.JournalArmed);
                AssertEx.NotNull(
                    adoptionFailure.RecoveryScope.WriteTicket);
                AssertEx.Equal(1, adoptionFailure.RecoveryCount);
            }

            using (var reusedTicketId = new Harness())
            {
                reusedTicketId.ReusePreWriteGuardTicketIdForWrite = true;

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        reusedTicketId.Request,
                        reusedTicketId.CreateOperations()));

                AssertEx.Equal(
                    1,
                    reusedTicketId.AdoptWriteTicketCount);
                AssertEx.Equal(
                    0,
                    reusedTicketId.MarkWriteAcceptedCount);
                AssertEx.Equal(2, reusedTicketId.WaitCount);
                AssertEx.True(
                    reusedTicketId.RecoveryScope.WriteTicket.TicketId
                        == reusedTicketId.RecoveryScope
                            .PreWriteGuardTicket.TicketId);
                AssertEx.Equal(
                    "ADOPT_WRITE_TICKET",
                    reusedTicketId.Events[
                        reusedTicketId.Events.Count - 1]);
                AssertEx.Equal(1, reusedTicketId.RecoveryCount);
            }
        }

        private static void UncertainWriteNeverReplays()
        {
            using (var harness = new Harness())
            {
                harness.WriteSubmitError = SubmissionUncertain(
                    "ambiguous Write transmission");

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        harness.Request,
                        harness.CreateOperations()));

                AssertEx.Contains("ambiguous", error.Message);
                AssertEx.Equal(1, harness.WriteSubmitCount);
                AssertEx.Equal(3, harness.GeneralSubmitCount);
                AssertEx.Equal(0, harness.ReadbackSubmitCount);
                AssertEx.Equal(0, harness.ResolveCount);
                AssertEx.True(
                    harness.RecoveryScope
                        .WriteSubmissionOutcomeUncertain);
                AssertEx.True(
                    harness.RecoveryScope
                        .HasUncertainSubmissionOutcome);
                AssertEx.True(harness.RecoveryScope.JournalArmed);
                AssertEx.False(harness.RecoveryScope.JournalResolved);
                AssertEx.Equal(1, harness.RecoveryCount);
            }
        }

        private static void ReadbackMismatchUnresolved()
        {
            using (var harness = new Harness())
            {
                harness.ReadbackDataOverride = new byte[]
                {
                    0x79,
                    0x56,
                    0x34,
                    0x12
                };

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        harness.Request,
                        harness.CreateOperations()));

                AssertEx.Equal(1, harness.WriteSubmitCount);
                AssertEx.Equal(1, harness.ReadbackSubmitCount);
                AssertEx.Equal(0, harness.ResolveCount);
                AssertEx.True(harness.RecoveryScope.JournalArmed);
                AssertEx.False(harness.RecoveryScope.ReadbackVerified);
                AssertEx.False(harness.RecoveryScope.JournalResolved);
                AssertEx.Equal(1, harness.RecoveryCount);
            }
        }

        private static void CancelAfterArmUnresolved()
        {
            using (var preCanceled = new CancellationTokenSource())
            using (var preCanceledHarness = new Harness())
            {
                preCanceled.Cancel();
                AssertEx.Throws<OperationCanceledException>(
                    () => Run(
                        preCanceledHarness.Request,
                        preCanceledHarness.CreateOperations(),
                        preCanceled.Token));
                AssertZeroWire(preCanceledHarness);
                AssertEx.Equal(1, preCanceledHarness.RecoveryCount);
                AssertEx.False(
                    preCanceledHarness.RecoveryScope.JournalArmed);
            }

            using (var cancellation = new CancellationTokenSource())
            using (var harness = new Harness())
            {
                harness.CancelAfterArm = cancellation;

                AssertEx.Throws<OperationCanceledException>(
                    () => Run(
                        harness.Request,
                        harness.CreateOperations(),
                        cancellation.Token));

                AssertEx.Equal(1, harness.ArmCount);
                AssertEx.Equal(0, harness.WriteSubmitCount);
                AssertEx.True(harness.RecoveryScope.JournalArmed);
                AssertEx.False(harness.RecoveryScope.JournalResolved);
                AssertEx.Equal(1, harness.RecoveryCount);
            }
        }

        private static D5SdoWriteSameValueQualificationResult Run(
            D5SdoWriteSameValueQualificationRequest request,
            D5SdoWriteSameValueQualificationOperations operations,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return D5SdoWriteSameValueQualificationOrchestrator.RunAsync(
                    request,
                    operations,
                    cancellationToken)
                .GetAwaiter()
                .GetResult();
        }

        private static void AssertZeroWire(Harness harness)
        {
            AssertEx.Equal(0, harness.GeneralSubmitCount);
            AssertEx.Equal(0, harness.WaitCount);
            AssertEx.Equal(0, harness.CapabilityReadCount);
            AssertEx.Equal(0, harness.SafetyCount);
            AssertEx.Equal(0, harness.ConfirmationCount);
            AssertEx.Equal(0, harness.ArmCount);
            AssertEx.Equal(0, harness.ResolveCount);
        }

        private static void AssertEvents(
            IList<string> actual,
            params string[] expected)
        {
            AssertEx.Equal(expected.Length, actual.Count);
            for (var index = 0; index < expected.Length; index++)
            {
                AssertEx.Equal(expected[index], actual[index]);
            }
        }

        private static LMCSdoWriteTarget Target()
        {
            return new LMCSdoWriteTarget(
                "Reserved diagnostic UI[24]",
                1,
                0x2F00,
                24,
                LMCSignalValueType.Int32,
                4,
                -1073741823,
                1073741823);
        }

        private static LMCDiagnosticCapability RequiredCapabilities
        {
            get
            {
                return LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOWrite
                    | LMCDiagnosticCapability.SDOReadGeneralInline;
            }
        }

        private static LMCDiagnosticsResponse Response()
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
                1,
                0);
        }

        private static InvalidOperationException SubmissionUncertain(
            string message)
        {
            return new InvalidOperationException(message);
        }

        private sealed class Harness : IDisposable
        {
            private readonly FakeRpcServer server;
            private uint nextTicketId = 100;

            internal Harness()
            {
                server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    CloseStep());
                Connection = new LMCConnection();
                Connect(Connection, server.Port);
                Target = D5SdoWriteSameValueQualificationOrchestratorTests
                    .Target();
                InitialCapabilities = Capabilities(1);
                Request = CreateRequest(
                    new[] { Target },
                    InitialCapabilities);
                Events = new List<string>();
                AcceptedTickets = new List<LMCOperationTicket>();
                SafetyResult = true;
                SecondSafetyResult = true;
                ConfirmationResult = true;
            }

            internal LMCConnection Connection { get; private set; }
            internal LMCSdoWriteTarget Target { get; private set; }
            internal LMCDiagnosticCapabilities InitialCapabilities
            {
                get;
                private set;
            }
            internal D5SdoWriteSameValueQualificationRequest Request
            {
                get;
                private set;
            }
            internal List<string> Events { get; private set; }
            internal List<LMCOperationTicket> AcceptedTickets
            {
                get;
                private set;
            }
            internal int GeneralSubmitCount { get; private set; }
            internal int WriteSubmitCount { get; private set; }
            internal int ReadbackSubmitCount { get; private set; }
            internal int WaitCount { get; private set; }
            internal int CapabilityReadCount { get; private set; }
            internal int SafetyCount { get; private set; }
            internal int ConfirmationCount { get; private set; }
            internal int ArmCount { get; private set; }
            internal int AdoptWriteTicketCount { get; private set; }
            internal int MarkWriteAcceptedCount { get; private set; }
            internal int ResolveCount { get; private set; }
            internal int RecoveryCount { get; private set; }
            internal bool SafetyResult { get; set; }
            internal bool SecondSafetyResult { get; set; }
            internal bool ConfirmationResult { get; set; }
            internal Exception ArmError { get; set; }
            internal Exception AdoptWriteTicketError { get; set; }
            internal Exception WriteSubmitError { get; set; }
            internal bool ReturnNullWriteTicket { get; set; }
            internal bool ReusePreWriteGuardTicketIdForWrite { get; set; }
            internal byte[] BaselineDataOverride { get; set; }
            internal byte[] PreWriteDataOverride { get; set; }
            internal byte[] ReadbackDataOverride { get; set; }
            internal CancellationTokenSource CancelAfterArm { get; set; }
            internal D5SdoWriteSameValueRecoveryScope RecoveryScope
            {
                get;
                private set;
            }

            internal D5SdoWriteSameValueQualificationRequest CreateRequest(
                IReadOnlyList<LMCSdoWriteTarget> approvedTargets,
                LMCDiagnosticCapabilities capabilities)
            {
                return new D5SdoWriteSameValueQualificationRequest(
                    Connection,
                    capabilities,
                    approvedTargets,
                    Target,
                    100);
            }

            internal LMCDiagnosticCapabilities Capabilities(
                long observationSequence,
                LMCDiagnosticCapability capabilityBits =
                    LMCDiagnosticCapability.SDORead
                        | LMCDiagnosticCapability.SDOWrite
                        | LMCDiagnosticCapability.SDOReadGeneralInline)
            {
                return new LMCDiagnosticCapabilities(
                    Response(),
                    Connection.SessionGeneration,
                    5,
                    (uint)capabilityBits,
                    MapRevision,
                    0,
                    0,
                    0,
                    0,
                    0,
                    1000,
                    1320,
                    2040,
                    1280,
                    80,
                    16,
                    0,
                    4,
                    DiagnosticsBootId).BindProvenance(
                        Connection.Diagnostics,
                        Connection.SessionGeneration,
                        observationSequence);
            }

            internal D5SdoWriteSameValueQualificationOperations
                CreateOperations()
            {
                return new
                    D5SdoWriteSameValueQualificationOperations
                    {
                        ReadCapabilitiesAsync = cancellationToken =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            CapabilityReadCount++;
                            var sequence = CapabilityReadCount + 1;
                            Events.Add(
                                "READ_CAPABILITIES_"
                                + sequence.ToString());
                            return Task.FromResult(
                                Capabilities(sequence));
                        },
                        SubmitAsync = SubmitAsync,
                        WaitForTerminalAsync = WaitForTerminalAsync,
                        VerifySafeAxisAsync = (
                            target,
                            cancellationToken) =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            SafetyCount++;
                            Events.Add(
                                "VERIFY_SAFE_AXIS_"
                                + SafetyCount.ToString());
                            AssertEx.True(ReferenceEquals(Target, target));
                            return Task.FromResult(
                                SafetyCount == 1
                                    ? SafetyResult
                                    : SecondSafetyResult);
                        },
                        ConfirmWriteAsync = (
                            request,
                            cancellationToken) =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            ConfirmationCount++;
                            Events.Add("CONFIRM_WRITE");
                            AssertEx.SequenceEqual(
                                BaselineData,
                                request.WriteData);
                            return Task.FromResult(ConfirmationResult);
                        },
                        ArmJournal = (scope, request, capabilities) =>
                        {
                            ArmCount++;
                            Events.Add("ARM_JOURNAL");
                            AssertEx.SequenceEqual(
                                BaselineData,
                                request.WriteData);
                            AssertEx.True(
                                ReferenceEquals(
                                    scope.PreWriteCapabilities,
                                    capabilities));
                            AssertEx.True(scope.SecondSafetyVerified);
                            AssertEx.NotNull(scope.PreWriteGuardTicket);
                            AssertEx.NotNull(scope.PreWriteGuardStatus);
                            if (ArmError != null)
                            {
                                throw ArmError;
                            }

                            if (CancelAfterArm != null)
                            {
                                CancelAfterArm.Cancel();
                            }
                        },
                        AdoptWriteTicketBeforeValidation = (
                            scope,
                            ticket) =>
                        {
                            AdoptWriteTicketCount++;
                            Events.Add("ADOPT_WRITE_TICKET");
                            AssertEx.True(scope.JournalArmed);
                            AssertEx.True(
                                ReferenceEquals(scope.WriteTicket, ticket));
                            if (AdoptWriteTicketError != null)
                            {
                                throw AdoptWriteTicketError;
                            }
                        },
                        MarkWriteAccepted = (scope, ticket) =>
                        {
                            MarkWriteAcceptedCount++;
                            Events.Add("MARK_WRITE_ACCEPTED");
                            AssertEx.True(scope.JournalArmed);
                            AssertEx.True(
                                ReferenceEquals(scope.WriteTicket, ticket));
                        },
                        MarkWriteTerminalSuccess = (
                            scope,
                            ticket,
                            status) =>
                        {
                            Events.Add("MARK_WRITE_TERMINAL");
                            AssertEx.True(scope.JournalArmed);
                            AssertEx.True(status.IsSuccessful);
                        },
                        CreateVerificationContext = (
                            request,
                            ticket,
                            status) =>
                        {
                            Events.Add("CREATE_CONTEXT");
                            return Connection.Diagnostics
                                .CreateSdoWriteVerificationContext(
                                    request,
                                    ticket,
                                    status,
                                    candidate => true);
                        },
                        SubmitReadbackAsync = SubmitReadbackAsync,
                        ResolveJournalAfterVerified = scope =>
                        {
                            ResolveCount++;
                            Events.Add("RESOLVE_JOURNAL");
                            AssertEx.True(scope.ReadbackVerified);
                            AssertEx.NotNull(scope.FinalCapabilities);
                            AssertEx.False(scope.JournalResolved);
                        },
                        RecoveryRequired = (scope, error) =>
                        {
                            RecoveryCount++;
                            RecoveryScope = scope;
                        }
                    };
            }

            private Task<LMCOperationTicket> SubmitAsync(
                LMCSdoRequest request,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                GeneralSubmitCount++;
                if (request.IsWrite)
                {
                    WriteSubmitCount++;
                    Events.Add("SUBMIT_WRITE");
                    if (WriteSubmitError != null)
                    {
                        LMCSdoSubmissionFailureContext.Attach(
                            WriteSubmitError,
                            new LMCSdoSubmissionFailureContext(
                                request,
                                LMCSdoSubmissionPhase.Submission,
                                LMCSdoSubmissionOutcome.OutcomeUncertain,
                                DiagnosticsBootId,
                                MapRevision,
                                null));
                        throw WriteSubmitError;
                    }

                    if (ReturnNullWriteTicket)
                    {
                        return Task.FromResult<LMCOperationTicket>(null);
                    }

                    return Task.FromResult(
                        AcceptTicket(
                            request,
                            LMCOperationKind.SDOWrite,
                            ReusePreWriteGuardTicketIdForWrite
                                ? (uint?)AcceptedTickets[1].TicketId
                                : null));
                }

                Events.Add(
                    GeneralSubmitCount == 1
                        ? "SUBMIT_BASELINE"
                        : "SUBMIT_PREWRITE_GUARD");
                return Task.FromResult(
                    AcceptTicket(request, LMCOperationKind.SDORead));
            }

            private Task<LMCOperationTicket> SubmitReadbackAsync(
                LMCSdoWriteVerificationContext context,
                LMCSdoRequest request,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ReadbackSubmitCount++;
                Events.Add("SUBMIT_READBACK");
                AssertEx.True(context.MatchesReadRequest(request));
                return Task.FromResult(
                    AcceptTicket(request, LMCOperationKind.SDORead));
            }

            private LMCOperationTicket AcceptTicket(
                LMCSdoRequest request,
                LMCOperationKind operationKind,
                uint? ticketId = null)
            {
                if (!ticketId.HasValue)
                {
                    nextTicketId++;
                }

                var isRead = operationKind == LMCOperationKind.SDORead;
                var ticket = new LMCOperationTicket(
                    ticketId ?? nextTicketId,
                    operationKind,
                    (ticketId ?? nextTicketId) + 1000,
                    DiagnosticsBootId,
                    MapRevision,
                    Connection.SessionGeneration,
                    Connection.Diagnostics,
                    isRead,
                    isRead ? request.DataLength : (ushort)0,
                    isRead
                        ? request.ValueType
                        : LMCSignalValueType.Invalid,
                    false,
                    0,
                    request);
                AcceptedTickets.Add(ticket);
                return ticket;
            }

            private Task<LMCOperationStatus> WaitForTerminalAsync(
                LMCOperationTicket ticket,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                WaitCount++;
                byte[] resultData;
                LMCSignalValueType valueType;
                if (ticket.OperationKind == LMCOperationKind.SDOWrite)
                {
                    Events.Add("WAIT_WRITE");
                    resultData = new byte[0];
                    valueType = LMCSignalValueType.Invalid;
                }
                else if (ticket.TicketId
                    == AcceptedTickets[0].TicketId)
                {
                    Events.Add("WAIT_BASELINE");
                    resultData = BaselineDataOverride ?? BaselineData;
                    valueType = Target.ValueType;
                }
                else if (ticket.TicketId
                    == AcceptedTickets[1].TicketId)
                {
                    Events.Add("WAIT_PREWRITE_GUARD");
                    resultData = PreWriteDataOverride ?? BaselineData;
                    valueType = Target.ValueType;
                }
                else
                {
                    Events.Add("WAIT_READBACK");
                    resultData = ReadbackDataOverride ?? BaselineData;
                    valueType = Target.ValueType;
                }

                var status = new LMCOperationStatus(
                    Response(),
                    ticket.TicketId,
                    ticket.OperationKind,
                    LMCOperationState.Completed,
                    ticket.QueuedCycle,
                    ticket.QueuedCycle + 1,
                    LMCOperationOutcome.Success,
                    0,
                    0,
                    (uint)resultData.Length,
                    valueType,
                    resultData,
                    DiagnosticsBootId).BindProvenance(
                        Connection.Diagnostics,
                        Connection.SessionGeneration);
                return Task.FromResult(status);
            }

            public void Dispose()
            {
                if (Connection != null)
                {
                    Connection.CloseConnection();
                    Connection.Dispose();
                    Connection = null;
                }

                server.Verify();
                server.Dispose();
            }

            private static void Connect(LMCConnection connection, int port)
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
            }

            private static FakeRpcStep InitStep()
            {
                var payload = new byte[24];
                TestFrame.WriteUInt32(payload, 0, 64);
                return new FakeRpcStep(
                    0x8080,
                    TestFrame.Response(0, payload));
            }

            private static FakeRpcStep CallbackStep()
            {
                return new FakeRpcStep(
                    0x405C,
                    TestFrame.Response(
                        0,
                        TestFrame.Hex("00 00 00 00")));
            }

            private static FakeRpcStep CloseStep()
            {
                return new FakeRpcStep(
                    0x405D,
                    TestFrame.Response(
                        0,
                        TestFrame.Hex("00 00 00 00")));
            }
        }
    }
}
