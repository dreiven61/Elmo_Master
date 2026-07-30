using System;
using System.Collections.Generic;
using LasalMotionControlApiExample;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsOperationAdmissionPolicyTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Policy.DiagnosticsAdmission.SafetyReadOnlyCleanupAlwaysAllowed",
                SafetyReadOnlyCleanupAlwaysAllowed);
            tests.Add(
                "Policy.DiagnosticsAdmission.RecoveryIdentityReadOnlyQuarantine",
                RecoveryIdentityReadOnlyQuarantine);
            tests.Add(
                "Policy.DiagnosticsAdmission.NewMutationTruthTable",
                NewMutationTruthTable);
            tests.Add(
                "Policy.DiagnosticsAdmission.TrackedD5SubmitTruthTable",
                TrackedD5SubmitTruthTable);
            tests.Add(
                "Policy.DiagnosticsAdmission.TrackedD5ReadOnlyTruthTable",
                TrackedD5ReadOnlyTruthTable);
            tests.Add(
                "Policy.DiagnosticsAdmission.ExactReadbackTruthTable",
                ExactReadbackTruthTable);
            tests.Add(
                "Policy.DiagnosticsAdmission.ConnectTruthTable",
                ConnectTruthTable);
            tests.Add(
                "Policy.DiagnosticsAdmission.CloseTruthTable",
                CloseTruthTable);
            tests.Add(
                "Policy.DiagnosticsAdmission.RejectsInvalidInput",
                RejectsInvalidInput);
        }

        private static void SafetyReadOnlyCleanupAlwaysAllowed()
        {
            var operations = new[]
            {
                DiagnosticsAdmissionOperation.SafetyControl,
                DiagnosticsAdmissionOperation.NonD5ReadOnlyInspection,
                DiagnosticsAdmissionOperation.ExistingResourceCleanup
            };

            for (var stateBits = 0; stateBits < 1024; stateBits++)
            {
                var state = StateFromBits(stateBits);
                foreach (var operation in operations)
                {
                    AssertAllowed(
                        operation,
                        state,
                        operation + " stateBits=" + stateBits);
                }
            }
        }

        private static void RecoveryIdentityReadOnlyQuarantine()
        {
            var connectedState = new DiagnosticsAdmissionState(
                false,
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                true,
                false,
                false,
                true);
            var allowedOperations = new[]
            {
                DiagnosticsAdmissionOperation.NonD5ReadOnlyInspection,
                DiagnosticsAdmissionOperation.CloseConnection,
                DiagnosticsAdmissionOperation.CloseWindow
            };
            foreach (var operation in allowedOperations)
            {
                AssertAllowed(operation, connectedState, operation.ToString());
            }

            var blockedOperations = new[]
            {
                DiagnosticsAdmissionOperation.SafetyControl,
                DiagnosticsAdmissionOperation.ExistingResourceCleanup,
                DiagnosticsAdmissionOperation.NewLiveOrMutation,
                DiagnosticsAdmissionOperation.TrackedD5Submit,
                DiagnosticsAdmissionOperation.TrackedD5ReadOnlyInspection,
                DiagnosticsAdmissionOperation.RequiredExactSdoWriteReadback,
                DiagnosticsAdmissionOperation.ConnectOrReconnect
            };
            foreach (var operation in blockedOperations)
            {
                var decision = DiagnosticsOperationAdmissionPolicy.Evaluate(
                    operation,
                    connectedState);
                AssertEx.False(decision.IsAllowed, operation.ToString());
                AssertEx.Equal(
                    DiagnosticsAdmissionDenialReason.RecoveryIdentityReadOnly,
                    decision.DenialReason,
                    operation.ToString());
            }

            var disconnectedState = new DiagnosticsAdmissionState(
                false,
                false,
                false,
                true,
                false,
                false,
                false,
                false,
                true,
                false,
                false,
                true);
            AssertAllowed(
                DiagnosticsAdmissionOperation.ConnectOrReconnect,
                disconnectedState,
                "Reconnect after closing a read-only quarantine session");
        }

        private static void NewMutationTruthTable()
        {
            for (var stateBits = 0; stateBits < 1024; stateBits++)
            {
                var state = StateFromBits(stateBits);
                var decision = DiagnosticsOperationAdmissionPolicy.Evaluate(
                    DiagnosticsAdmissionOperation.NewLiveOrMutation,
                    state);
                var expectedAllowed = !state.HasUnresolvedMutation
                    && !state.MutationJournalUnavailable
                    && !state.HasUnresolvedAxisPowerOn
                    && !state.HasUnresolvedGroupPower;

                AssertEx.Equal(
                    expectedAllowed,
                    decision.IsAllowed,
                    "New mutation stateBits=" + stateBits + ".");
                AssertEx.Equal(
                    state.HasUnresolvedMutation
                        ? DiagnosticsAdmissionDenialReason.UnresolvedMutation
                        : state.MutationJournalUnavailable
                            ? DiagnosticsAdmissionDenialReason
                                .MutationJournalUnavailable
                            : state.HasUnresolvedAxisPowerOn
                                ? DiagnosticsAdmissionDenialReason
                                    .AxisPowerOnUnresolved
                            : state.HasUnresolvedGroupPower
                                ? DiagnosticsAdmissionDenialReason
                                    .GroupPowerUnresolved
                            : DiagnosticsAdmissionDenialReason.None,
                    decision.DenialReason,
                    "New mutation reason stateBits=" + stateBits + ".");
            }
        }

        private static void TrackedD5SubmitTruthTable()
        {
            for (var stateBits = 0; stateBits < 1024; stateBits++)
            {
                var state = StateFromBits(stateBits);
                var decision = DiagnosticsOperationAdmissionPolicy.Evaluate(
                    DiagnosticsAdmissionOperation.TrackedD5Submit,
                    state);
                var expectedAllowed = !state.HasUnresolvedMutation
                    && !state.MutationJournalUnavailable
                    && !state.HasUnresolvedAxisPowerOn
                    && !state.HasUnresolvedGroupPower
                    && state.OperationSlotAvailable;

                AssertEx.Equal(
                    expectedAllowed,
                    decision.IsAllowed,
                    "Tracked D5 submit stateBits=" + stateBits + ".");
                AssertEx.Equal(
                    state.HasUnresolvedMutation
                        ? DiagnosticsAdmissionDenialReason.UnresolvedMutation
                        : state.MutationJournalUnavailable
                            ? DiagnosticsAdmissionDenialReason
                                .MutationJournalUnavailable
                            : state.HasUnresolvedAxisPowerOn
                                ? DiagnosticsAdmissionDenialReason
                                    .AxisPowerOnUnresolved
                            : state.HasUnresolvedGroupPower
                                ? DiagnosticsAdmissionDenialReason
                                    .GroupPowerUnresolved
                            : !state.OperationSlotAvailable
                                ? DiagnosticsAdmissionDenialReason
                                    .OperationSlotOccupied
                                : DiagnosticsAdmissionDenialReason.None,
                    decision.DenialReason,
                    "Tracked D5 submit reason stateBits=" + stateBits + ".");
            }
        }

        private static void TrackedD5ReadOnlyTruthTable()
        {
            for (var stateBits = 0; stateBits < 1024; stateBits++)
            {
                var state = StateFromBits(stateBits);
                var decision = DiagnosticsOperationAdmissionPolicy.Evaluate(
                    DiagnosticsAdmissionOperation
                        .TrackedD5ReadOnlyInspection,
                    state);
                var expectedAllowed = !state.HasUnresolvedMutation
                    && !state.MutationJournalUnavailable
                    && state.OperationSlotAvailable;

                AssertEx.Equal(
                    expectedAllowed,
                    decision.IsAllowed,
                    "Tracked D5 read-only stateBits=" + stateBits + ".");
                AssertEx.Equal(
                    state.HasUnresolvedMutation
                        ? DiagnosticsAdmissionDenialReason.UnresolvedMutation
                        : state.MutationJournalUnavailable
                            ? DiagnosticsAdmissionDenialReason
                                .MutationJournalUnavailable
                            : !state.OperationSlotAvailable
                                ? DiagnosticsAdmissionDenialReason
                                    .OperationSlotOccupied
                                : DiagnosticsAdmissionDenialReason.None,
                    decision.DenialReason,
                    "Tracked D5 read-only reason stateBits="
                    + stateBits
                    + ".");
            }
        }

        private static void ExactReadbackTruthTable()
        {
            for (var stateBits = 0; stateBits < 1024; stateBits++)
            {
                var state = StateFromBits(stateBits);
                var decision = DiagnosticsOperationAdmissionPolicy.Evaluate(
                    DiagnosticsAdmissionOperation.RequiredExactSdoWriteReadback,
                    state);
                var expectedAllowed = state.ExactReadbackPending
                    && !state.HasD5TicketOrQuarantine
                    && !state.HasUnresolvedDigitalOutputWrite
                    && state.OperationSlotAvailable
                    && state.ExactReadbackSessionCurrent;

                AssertEx.Equal(
                    expectedAllowed,
                    decision.IsAllowed,
                    "Exact readback stateBits=" + stateBits + ".");
                AssertEx.Equal(
                    ExpectedExactReadbackReason(state),
                    decision.DenialReason,
                    "Exact readback reason stateBits=" + stateBits + ".");
            }
        }

        private static void ConnectTruthTable()
        {
            for (var stateBits = 0; stateBits < 1024; stateBits++)
            {
                var state = StateFromBits(stateBits);
                var decision = DiagnosticsOperationAdmissionPolicy.Evaluate(
                    DiagnosticsAdmissionOperation.ConnectOrReconnect,
                    state);
                var expectedAllowed = (!state.HasUnresolvedMutation
                        && !state.HasUnresolvedAxisPowerOn
                        && !state.HasUnresolvedGroupPower)
                    || !state.IsConnected;

                AssertEx.Equal(
                    expectedAllowed,
                    decision.IsAllowed,
                    "Connect stateBits=" + stateBits + ".");
                AssertEx.Equal(
                    expectedAllowed
                        ? DiagnosticsAdmissionDenialReason.None
                        : DiagnosticsAdmissionDenialReason
                            .ExternalDisconnectRequired,
                    decision.DenialReason,
                    "Connect reason stateBits=" + stateBits + ".");
            }
        }

        private static void CloseTruthTable()
        {
            var operations = new[]
            {
                DiagnosticsAdmissionOperation.CloseConnection,
                DiagnosticsAdmissionOperation.CloseWindow
            };

            for (var stateBits = 0; stateBits < 1024; stateBits++)
            {
                var state = StateFromBits(stateBits);
                foreach (var operation in operations)
                {
                    var decision = DiagnosticsOperationAdmissionPolicy.Evaluate(
                        operation,
                        state);
                    AssertEx.Equal(
                        !state.HasUnresolvedMutation
                            && !state.HasUnresolvedAxisPowerOn
                            && !state.HasUnresolvedGroupPower,
                        decision.IsAllowed,
                        operation + " stateBits=" + stateBits + ".");
                    AssertEx.Equal(
                        state.HasUnresolvedMutation
                            ? DiagnosticsAdmissionDenialReason
                                .UnresolvedMutation
                            : state.HasUnresolvedAxisPowerOn
                                ? DiagnosticsAdmissionDenialReason
                                    .AxisPowerOnUnresolved
                            : state.HasUnresolvedGroupPower
                                ? DiagnosticsAdmissionDenialReason
                                    .GroupPowerUnresolved
                            : DiagnosticsAdmissionDenialReason.None,
                        decision.DenialReason,
                        operation + " reason stateBits=" + stateBits + ".");
                }
            }
        }

        private static void RejectsInvalidInput()
        {
            AssertEx.Throws<ArgumentNullException>(
                () => DiagnosticsOperationAdmissionPolicy.Evaluate(
                    DiagnosticsAdmissionOperation.NewLiveOrMutation,
                    null));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => DiagnosticsOperationAdmissionPolicy.Evaluate(
                    (DiagnosticsAdmissionOperation)int.MaxValue,
                    StateFromBits(0)));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason.None));
        }

        private static DiagnosticsAdmissionDenialReason
            ExpectedExactReadbackReason(DiagnosticsAdmissionState state)
        {
            if (!state.ExactReadbackPending)
            {
                return DiagnosticsAdmissionDenialReason.ExactReadbackNotPending;
            }

            if (state.HasUnresolvedDigitalOutputWrite)
            {
                return DiagnosticsAdmissionDenialReason
                    .DigitalOutputWriteUnresolved;
            }

            if (state.HasD5TicketOrQuarantine)
            {
                return DiagnosticsAdmissionDenialReason
                    .D5TicketOrQuarantineUnresolved;
            }

            if (!state.OperationSlotAvailable)
            {
                return DiagnosticsAdmissionDenialReason.OperationSlotOccupied;
            }

            if (!state.ExactReadbackSessionCurrent)
            {
                return DiagnosticsAdmissionDenialReason
                    .ExactReadbackSessionMismatch;
            }

            return DiagnosticsAdmissionDenialReason.None;
        }

        private static void AssertAllowed(
            DiagnosticsAdmissionOperation operation,
            DiagnosticsAdmissionState state,
            string context)
        {
            var decision = DiagnosticsOperationAdmissionPolicy.Evaluate(
                operation,
                state);
            AssertEx.True(decision.IsAllowed, context);
            AssertEx.Equal(
                DiagnosticsAdmissionDenialReason.None,
                decision.DenialReason,
                context);
        }

        private static DiagnosticsAdmissionState StateFromBits(int bits)
        {
            return new DiagnosticsAdmissionState(
                (bits & 0x01) != 0,
                (bits & 0x02) != 0,
                (bits & 0x04) != 0,
                (bits & 0x08) != 0,
                (bits & 0x10) != 0,
                (bits & 0x20) != 0,
                (bits & 0x40) != 0,
                (bits & 0x80) != 0,
                (bits & 0x100) != 0,
                (bits & 0x200) != 0);
        }
    }
}
