using System;

namespace LasalMotionControlApiExample
{
    internal enum DiagnosticsAdmissionOperation
    {
        SafetyControl = 0,
        NonD5ReadOnlyInspection = 1,
        ExistingResourceCleanup = 2,
        NewLiveOrMutation = 3,
        TrackedD5Submit = 4,
        RequiredExactSdoWriteReadback = 5,
        ConnectOrReconnect = 6,
        CloseConnection = 7,
        CloseWindow = 8,
        TrackedD5ReadOnlyInspection = 9,
        RetireStaleRecoveryEvidence = 10
    }

    internal enum DiagnosticsAdmissionDenialReason
    {
        None = 0,
        UnresolvedMutation = 1,
        MutationJournalUnavailable = 2,
        OperationSlotOccupied = 3,
        ExactReadbackNotPending = 4,
        D5TicketOrQuarantineUnresolved = 5,
        DigitalOutputWriteUnresolved = 6,
        ExactReadbackSessionMismatch = 7,
        ExternalDisconnectRequired = 8,
        AxisPowerOnUnresolved = 9,
        GroupPowerUnresolved = 10,
        PowerRecoveryJournalUnavailable = 11,
        RecoveryIdentityReadOnly = 12,
        StaleRecoveryRetirementUnavailable = 13
    }

    internal sealed class DiagnosticsAdmissionState
    {
        internal DiagnosticsAdmissionState(
            bool hasUnresolvedMutation,
            bool mutationJournalUnavailable,
            bool isConnected,
            bool operationSlotAvailable,
            bool exactReadbackPending,
            bool exactReadbackSessionCurrent,
            bool hasD5TicketOrQuarantine,
            bool hasUnresolvedDigitalOutputWrite,
            bool hasUnresolvedAxisPowerOn,
            bool hasUnresolvedGroupPower,
            bool powerRecoveryJournalUnavailable = false,
            bool recoveryIdentityReadOnly = false)
        {
            HasUnresolvedMutation = hasUnresolvedMutation;
            MutationJournalUnavailable = mutationJournalUnavailable;
            IsConnected = isConnected;
            OperationSlotAvailable = operationSlotAvailable;
            ExactReadbackPending = exactReadbackPending;
            ExactReadbackSessionCurrent = exactReadbackSessionCurrent;
            HasD5TicketOrQuarantine = hasD5TicketOrQuarantine;
            HasUnresolvedDigitalOutputWrite =
                hasUnresolvedDigitalOutputWrite;
            HasUnresolvedAxisPowerOn = hasUnresolvedAxisPowerOn;
            HasUnresolvedGroupPower = hasUnresolvedGroupPower;
            PowerRecoveryJournalUnavailable =
                powerRecoveryJournalUnavailable;
            RecoveryIdentityReadOnly = recoveryIdentityReadOnly;
        }

        internal bool HasUnresolvedMutation { get; }

        internal bool MutationJournalUnavailable { get; }

        internal bool IsConnected { get; }

        internal bool OperationSlotAvailable { get; }

        internal bool ExactReadbackPending { get; }

        internal bool ExactReadbackSessionCurrent { get; }

        internal bool HasD5TicketOrQuarantine { get; }

        internal bool HasUnresolvedDigitalOutputWrite { get; }

        internal bool HasUnresolvedAxisPowerOn { get; }

        internal bool HasUnresolvedGroupPower { get; }

        internal bool PowerRecoveryJournalUnavailable { get; }

        internal bool RecoveryIdentityReadOnly { get; }
    }

    internal sealed class DiagnosticsAdmissionDecision
    {
        private static readonly DiagnosticsAdmissionDecision AllowedDecision =
            new DiagnosticsAdmissionDecision(
                true,
                DiagnosticsAdmissionDenialReason.None);

        private DiagnosticsAdmissionDecision(
            bool isAllowed,
            DiagnosticsAdmissionDenialReason denialReason)
        {
            IsAllowed = isAllowed;
            DenialReason = denialReason;
        }

        internal bool IsAllowed { get; }

        internal DiagnosticsAdmissionDenialReason DenialReason { get; }

        internal static DiagnosticsAdmissionDecision Allow()
        {
            return AllowedDecision;
        }

        internal static DiagnosticsAdmissionDecision Deny(
            DiagnosticsAdmissionDenialReason reason)
        {
            if (reason == DiagnosticsAdmissionDenialReason.None)
            {
                throw new ArgumentOutOfRangeException(nameof(reason));
            }

            return new DiagnosticsAdmissionDecision(false, reason);
        }
    }

    internal static class DiagnosticsOperationAdmissionPolicy
    {
        internal static DiagnosticsAdmissionDecision Evaluate(
            DiagnosticsAdmissionOperation operation,
            DiagnosticsAdmissionState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.RecoveryIdentityReadOnly)
            {
                switch (operation)
                {
                    case DiagnosticsAdmissionOperation
                        .NonD5ReadOnlyInspection:
                    case DiagnosticsAdmissionOperation.CloseConnection:
                    case DiagnosticsAdmissionOperation.CloseWindow:
                        return DiagnosticsAdmissionDecision.Allow();

                    case DiagnosticsAdmissionOperation
                        .RetireStaleRecoveryEvidence:
                        if (!state.IsConnected)
                        {
                            return DiagnosticsAdmissionDecision.Deny(
                                DiagnosticsAdmissionDenialReason
                                    .StaleRecoveryRetirementUnavailable);
                        }

                        return state.OperationSlotAvailable
                            ? DiagnosticsAdmissionDecision.Allow()
                            : DiagnosticsAdmissionDecision.Deny(
                                DiagnosticsAdmissionDenialReason
                                    .OperationSlotOccupied);

                    case DiagnosticsAdmissionOperation.ConnectOrReconnect:
                        return state.IsConnected
                            ? DiagnosticsAdmissionDecision.Deny(
                                DiagnosticsAdmissionDenialReason
                                    .RecoveryIdentityReadOnly)
                            : DiagnosticsAdmissionDecision.Allow();

                    case DiagnosticsAdmissionOperation.SafetyControl:
                    case DiagnosticsAdmissionOperation.ExistingResourceCleanup:
                    case DiagnosticsAdmissionOperation.NewLiveOrMutation:
                    case DiagnosticsAdmissionOperation.TrackedD5Submit:
                    case DiagnosticsAdmissionOperation
                        .RequiredExactSdoWriteReadback:
                    case DiagnosticsAdmissionOperation
                        .TrackedD5ReadOnlyInspection:
                        return DiagnosticsAdmissionDecision.Deny(
                            DiagnosticsAdmissionDenialReason
                                .RecoveryIdentityReadOnly);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(operation));
                }
            }

            switch (operation)
            {
                case DiagnosticsAdmissionOperation.SafetyControl:
                case DiagnosticsAdmissionOperation.NonD5ReadOnlyInspection:
                case DiagnosticsAdmissionOperation.ExistingResourceCleanup:
                    return DiagnosticsAdmissionDecision.Allow();

                case DiagnosticsAdmissionOperation.NewLiveOrMutation:
                    return EvaluateNewMutation(state, false, true);

                case DiagnosticsAdmissionOperation.TrackedD5Submit:
                    return EvaluateNewMutation(state, true, true);

                case DiagnosticsAdmissionOperation
                    .TrackedD5ReadOnlyInspection:
                    return EvaluateNewMutation(state, true, false);

                case DiagnosticsAdmissionOperation.RequiredExactSdoWriteReadback:
                    return EvaluateRequiredExactReadback(state);

                case DiagnosticsAdmissionOperation.ConnectOrReconnect:
                    if ((state.HasUnresolvedMutation
                            || state.HasUnresolvedAxisPowerOn
                            || state.HasUnresolvedGroupPower)
                        && state.IsConnected)
                    {
                        return DiagnosticsAdmissionDecision.Deny(
                            DiagnosticsAdmissionDenialReason
                                .ExternalDisconnectRequired);
                    }

                    return DiagnosticsAdmissionDecision.Allow();

                case DiagnosticsAdmissionOperation.CloseConnection:
                case DiagnosticsAdmissionOperation.CloseWindow:
                    if (state.HasUnresolvedMutation)
                    {
                        return DiagnosticsAdmissionDecision.Deny(
                            DiagnosticsAdmissionDenialReason
                                .UnresolvedMutation);
                    }

                    if (state.HasUnresolvedAxisPowerOn)
                    {
                        return DiagnosticsAdmissionDecision.Deny(
                            DiagnosticsAdmissionDenialReason
                                .AxisPowerOnUnresolved);
                    }

                    return state.HasUnresolvedGroupPower
                        ? DiagnosticsAdmissionDecision.Deny(
                            DiagnosticsAdmissionDenialReason
                                .GroupPowerUnresolved)
                        : DiagnosticsAdmissionDecision.Allow();

                case DiagnosticsAdmissionOperation
                    .RetireStaleRecoveryEvidence:
                    return DiagnosticsAdmissionDecision.Deny(
                        DiagnosticsAdmissionDenialReason
                            .StaleRecoveryRetirementUnavailable);

                default:
                    throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static DiagnosticsAdmissionDecision EvaluateNewMutation(
            DiagnosticsAdmissionState state,
            bool requiresOperationSlot,
            bool blocksForUnresolvedAxisPowerOn)
        {
            if (state.HasUnresolvedMutation)
            {
                return DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason.UnresolvedMutation);
            }

            if (state.MutationJournalUnavailable)
            {
                return DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason
                        .MutationJournalUnavailable);
            }

            if (blocksForUnresolvedAxisPowerOn
                && state.HasUnresolvedAxisPowerOn)
            {
                return DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason
                        .AxisPowerOnUnresolved);
            }

            if (blocksForUnresolvedAxisPowerOn
                && state.HasUnresolvedGroupPower)
            {
                return DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason
                        .GroupPowerUnresolved);
            }

            if (blocksForUnresolvedAxisPowerOn
                && state.PowerRecoveryJournalUnavailable)
            {
                return DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason
                        .PowerRecoveryJournalUnavailable);
            }

            if (requiresOperationSlot && !state.OperationSlotAvailable)
            {
                return DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason.OperationSlotOccupied);
            }

            return DiagnosticsAdmissionDecision.Allow();
        }

        private static DiagnosticsAdmissionDecision
            EvaluateRequiredExactReadback(DiagnosticsAdmissionState state)
        {
            if (!state.ExactReadbackPending)
            {
                return DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason.ExactReadbackNotPending);
            }

            if (state.HasUnresolvedDigitalOutputWrite)
            {
                return DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason
                        .DigitalOutputWriteUnresolved);
            }

            if (state.HasD5TicketOrQuarantine)
            {
                return DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason
                        .D5TicketOrQuarantineUnresolved);
            }

            if (!state.OperationSlotAvailable)
            {
                return DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason.OperationSlotOccupied);
            }

            if (!state.ExactReadbackSessionCurrent)
            {
                return DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason
                        .ExactReadbackSessionMismatch);
            }

            // HasUnresolvedMutation and an unavailable journal are expected
            // recovery states here. The exact readback is the sole narrow
            // exception that can resolve its own interlock. Its completion
            // path resolves durable evidence before clearing volatile state,
            // so a journal fault still fails closed at the commit boundary.
            return DiagnosticsAdmissionDecision.Allow();
        }
    }
}
