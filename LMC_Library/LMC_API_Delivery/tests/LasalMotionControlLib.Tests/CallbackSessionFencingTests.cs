using System;
using System.Collections.Generic;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class CallbackSessionFencingTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "CallbackFence.AcceptedValue.IsOnlyTypedWakeHint",
                AcceptedValueIsOnlyTypedWakeHint);
            tests.Add(
                "CallbackFence.StaleDuplicateOutOfOrder.DoNotAdvance",
                StaleDuplicateOutOfOrderDoNotAdvance);
            tests.Add(
                "CallbackFence.Sequence.WrapUsesSerialArithmetic",
                SequenceWrapUsesSerialArithmetic);
            tests.Add(
                "CallbackFence.Reconnect.RejectsOldSessionIdentity",
                ReconnectRejectsOldSessionIdentity);
            tests.Add(
                "CallbackFence.UnknownIdentifiers.FailClosedAsMalformed",
                UnknownIdentifiersFailClosedAsMalformed);
        }

        private static void AcceptedValueIsOnlyTypedWakeHint()
        {
            var policy = CallbackProtocolTests.ApprovedPolicy();
            var fence = CallbackProtocolTests.ApprovedFence(policy);
            var receiver = new LMCCallbackReceiverFence(fence, policy);
            var encoded = Encode(fence, policy, 10, new byte[] { 1, 2, 3 });

            var decision = Evaluate(receiver, encoded, 7);

            AssertEx.True(decision.IsAccepted);
            AssertEx.Equal(
                LMCCallbackFenceDecisionKind.AcceptedWakeHint,
                decision.Kind);
            AssertEx.Equal(LMCCallbackProtocolError.None, decision.ProtocolError);
            AssertEx.NotNull(decision.WakeHint);
            AssertEx.False(decision.WakeHint.IsAuthoritative);
            AssertEx.True(decision.WakeHint.RequiresAuthoritativeTcpQuery);
            AssertEx.Equal(10UL, decision.WakeHint.Sequence);
            AssertEx.Equal(fence.BootId, decision.WakeHint.BootId);
            AssertEx.Equal(fence.SessionEpoch, decision.WakeHint.SessionEpoch);
            AssertEx.Equal(fence.Cookie, JoinCookie(decision.WakeHint));
            AssertEx.SequenceEqual(
                new byte[] { 1, 2, 3 },
                decision.WakeHint.Payload);

            var callerCopy = decision.WakeHint.Payload;
            callerCopy[0] = 0xFF;
            AssertEx.SequenceEqual(
                new byte[] { 1, 2, 3 },
                decision.WakeHint.Payload,
                "A wake hint must not expose mutable parser state.");
            AssertEx.True(receiver.HasAcceptedSequence);
            AssertEx.Equal(10UL, receiver.LastAcceptedSequence);
            AssertEx.Equal(1L, receiver.AcceptedCount);
            AssertEx.Equal(0L, receiver.RejectedCount);

            var sourceCopy = fence.ExpectedSourceIPv4;
            sourceCopy[0] = 0;
            AssertEx.SequenceEqual(
                CallbackProtocolTests.ApprovedSourceIPv4(),
                fence.ExpectedSourceIPv4,
                "The expected callback source must be immutable to callers.");
        }

        private static void StaleDuplicateOutOfOrderDoNotAdvance()
        {
            var policy = CallbackProtocolTests.ApprovedPolicy();
            var fence = CallbackProtocolTests.ApprovedFence(policy);
            var receiver = new LMCCallbackReceiverFence(fence, policy);
            var sequence10 = Encode(fence, policy, 10, new byte[0]);

            AssertKind(
                LMCCallbackFenceDecisionKind.AcceptedWakeHint,
                Evaluate(receiver, sequence10, 7));
            AssertKind(
                LMCCallbackFenceDecisionKind.DuplicateSequence,
                Evaluate(receiver, sequence10, 7));
            AssertKind(
                LMCCallbackFenceDecisionKind.OutOfOrderSequence,
                Evaluate(
                    receiver,
                    Encode(fence, policy, 9, new byte[0]),
                    7));
            AssertKind(
                LMCCallbackFenceDecisionKind.StaleListenerGeneration,
                Evaluate(
                    receiver,
                    Encode(fence, policy, 11, new byte[0]),
                    6));

            AssertRejectedProtocol(
                LMCCallbackFenceDecisionKind.UnexpectedSourceAddress,
                LMCCallbackProtocolError.CallbackSourceAddressInvalid,
                receiver.Evaluate(sequence10, 7, null));
            var wrongSource = CallbackProtocolTests.ApprovedSourceIPv4();
            wrongSource[3]++;
            AssertRejectedProtocol(
                LMCCallbackFenceDecisionKind.UnexpectedSourceAddress,
                LMCCallbackProtocolError.CallbackSourceAddressMismatch,
                receiver.Evaluate(sequence10, 7, wrongSource));

            var sequence11 = Encode(fence, policy, 11, new byte[0]);
            var staleBoot = Clone(sequence11);
            TestFrame.WriteUInt32(staleBoot, 16, fence.BootId + 1);
            AssertRejectedProtocol(
                LMCCallbackFenceDecisionKind.StaleBootId,
                LMCCallbackProtocolError.StaleBootId,
                Evaluate(receiver, staleBoot, 7));

            var staleSession = Clone(sequence11);
            TestFrame.WriteUInt32(
                staleSession,
                20,
                fence.SessionEpoch + 1);
            AssertRejectedProtocol(
                LMCCallbackFenceDecisionKind.StaleSessionEpoch,
                LMCCallbackProtocolError.StaleSessionEpoch,
                Evaluate(receiver, staleSession, 7));

            var staleCookie = Clone(sequence11);
            TestFrame.WriteUInt32(staleCookie, 24, fence.CookieLo + 1);
            AssertRejectedProtocol(
                LMCCallbackFenceDecisionKind.StaleCookie,
                LMCCallbackProtocolError.StaleCookie,
                Evaluate(receiver, staleCookie, 7));

            AssertKind(
                LMCCallbackFenceDecisionKind.AcceptedWakeHint,
                Evaluate(receiver, sequence11, 7));
            AssertEx.Equal(
                11UL,
                receiver.LastAcceptedSequence,
                "Rejected datagrams must not advance the sequence fence.");
            AssertEx.Equal(2L, receiver.AcceptedCount);
            AssertEx.Equal(8L, receiver.RejectedCount);
            AssertEx.Equal(1L, receiver.DuplicateCount);
            AssertEx.Equal(1L, receiver.OutOfOrderCount);
        }

        private static void SequenceWrapUsesSerialArithmetic()
        {
            var policy = CallbackProtocolTests.ApprovedPolicy();
            var fence = CallbackProtocolTests.ApprovedFence(policy);
            var receiver = new LMCCallbackReceiverFence(fence, policy);

            AssertAccepted(receiver, fence, policy, ulong.MaxValue - 1);
            AssertAccepted(receiver, fence, policy, ulong.MaxValue);
            AssertAccepted(receiver, fence, policy, 0);
            AssertAccepted(receiver, fence, policy, 1);
            AssertKind(
                LMCCallbackFenceDecisionKind.DuplicateSequence,
                Evaluate(
                    receiver,
                    Encode(fence, policy, 1, new byte[0]),
                    7));
            AssertKind(
                LMCCallbackFenceDecisionKind.OutOfOrderSequence,
                Evaluate(
                    receiver,
                    Encode(fence, policy, ulong.MaxValue, new byte[0]),
                    7));
            AssertKind(
                LMCCallbackFenceDecisionKind.OutOfOrderSequence,
                Evaluate(
                    receiver,
                    Encode(
                        fence,
                        policy,
                        unchecked(1UL + 0x8000000000000000UL),
                        new byte[0]),
                    7));

            AssertEx.Equal(1UL, receiver.LastAcceptedSequence);
            AssertEx.Equal(4L, receiver.AcceptedCount);
            AssertEx.Equal(3L, receiver.RejectedCount);
            AssertEx.Equal(1L, receiver.DuplicateCount);
            AssertEx.Equal(2L, receiver.OutOfOrderCount);
        }

        private static void ReconnectRejectsOldSessionIdentity()
        {
            var policy = CallbackProtocolTests.ApprovedPolicy();
            var oldFence = CallbackProtocolTests.ApprovedFence(policy);
            var newFence = new LMCCallbackSessionFence(
                8,
                CallbackProtocolTests.ApprovedSourceIPv4(),
                oldFence.RegisteredEventMask,
                oldFence.MaxDatagramBytes,
                oldFence.BootId + 1,
                oldFence.SessionEpoch + 1,
                oldFence.CookieLo + 1,
                oldFence.CookieHi);
            var oldDatagram = Encode(oldFence, policy, 1, new byte[0]);
            var newDatagram = Encode(newFence, policy, 1, new byte[0]);
            var oldReceiver = new LMCCallbackReceiverFence(oldFence, policy);
            var newReceiver = new LMCCallbackReceiverFence(newFence, policy);

            AssertRejectedProtocol(
                LMCCallbackFenceDecisionKind.StaleBootId,
                LMCCallbackProtocolError.StaleBootId,
                Evaluate(newReceiver, oldDatagram, 8));
            AssertRejectedProtocol(
                LMCCallbackFenceDecisionKind.StaleBootId,
                LMCCallbackProtocolError.StaleBootId,
                Evaluate(oldReceiver, newDatagram, 7));
            AssertKind(
                LMCCallbackFenceDecisionKind.StaleListenerGeneration,
                Evaluate(newReceiver, newDatagram, 7));
            AssertKind(
                LMCCallbackFenceDecisionKind.AcceptedWakeHint,
                Evaluate(newReceiver, newDatagram, 8));

            AssertEx.Equal(1L, newReceiver.AcceptedCount);
            AssertEx.Equal(2L, newReceiver.RejectedCount);
        }

        private static void UnknownIdentifiersFailClosedAsMalformed()
        {
            var approvedPolicy = CallbackProtocolTests.ApprovedPolicy();
            var fence = CallbackProtocolTests.ApprovedFence(approvedPolicy);
            var encoded = Encode(fence, approvedPolicy, 1, new byte[0]);
            TestFrame.WriteUInt16(encoded, 10, 0x2469);
            var receiver = new LMCCallbackReceiverFence(fence, approvedPolicy);

            AssertRejectedProtocol(
                LMCCallbackFenceDecisionKind.Malformed,
                LMCCallbackProtocolError.EventIdentifierNotApproved,
                Evaluate(receiver, encoded, 7));
            AssertEx.False(receiver.HasAcceptedSequence);
            AssertEx.Equal(0L, receiver.AcceptedCount);
            AssertEx.Equal(1L, receiver.RejectedCount);

            AssertEx.Throws<ArgumentException>(
                () => new LMCCallbackReceiverFence(
                    fence,
                    new LMCCallbackProtocolPolicy(
                        value => { throw new InvalidOperationException(); },
                        (eventType, eventId) => true,
                        value => true,
                        (status, errorId) => true)));

            var semanticCallCount = 0;
            var provenanceFirstPolicy = new LMCCallbackProtocolPolicy(
                value => true,
                (eventType, eventId) =>
                {
                    semanticCallCount++;
                    throw new InvalidOperationException();
                },
                value => true,
                (status, errorId) => true);
            var provenanceFirstReceiver = new LMCCallbackReceiverFence(
                fence,
                provenanceFirstPolicy);
            var staleCookie = Encode(fence, approvedPolicy, 2, new byte[0]);
            TestFrame.WriteUInt16(staleCookie, 10, 0x2469);
            TestFrame.WriteUInt32(staleCookie, 24, fence.CookieLo + 1);
            AssertRejectedProtocol(
                LMCCallbackFenceDecisionKind.StaleCookie,
                LMCCallbackProtocolError.StaleCookie,
                Evaluate(provenanceFirstReceiver, staleCookie, 7));
            AssertEx.Equal(
                0,
                semanticCallCount,
                "Untrusted semantics must not run before provenance checks.");
        }

        private static void AssertAccepted(
            LMCCallbackReceiverFence receiver,
            LMCCallbackSessionFence fence,
            LMCCallbackProtocolPolicy policy,
            ulong sequence)
        {
            AssertKind(
                LMCCallbackFenceDecisionKind.AcceptedWakeHint,
                Evaluate(
                    receiver,
                    Encode(fence, policy, sequence, new byte[0]),
                    7));
        }

        private static LMCCallbackFenceDecision Evaluate(
            LMCCallbackReceiverFence receiver,
            byte[] datagram,
            long listenerGeneration)
        {
            return receiver.Evaluate(
                datagram,
                listenerGeneration,
                CallbackProtocolTests.ApprovedSourceIPv4());
        }

        private static byte[] Encode(
            LMCCallbackSessionFence fence,
            LMCCallbackProtocolPolicy policy,
            ulong sequence,
            byte[] payload)
        {
            return LMCCallbackProtocol.EncodeDatagram(
                CallbackProtocolTests.ApprovedDatagram(sequence, payload),
                fence,
                policy);
        }

        private static void AssertKind(
            LMCCallbackFenceDecisionKind expected,
            LMCCallbackFenceDecision actual)
        {
            AssertEx.Equal(expected, actual.Kind);
            AssertEx.Equal(
                expected == LMCCallbackFenceDecisionKind.AcceptedWakeHint,
                actual.IsAccepted);
        }

        private static void AssertRejectedProtocol(
            LMCCallbackFenceDecisionKind expectedKind,
            LMCCallbackProtocolError expectedError,
            LMCCallbackFenceDecision actual)
        {
            AssertKind(expectedKind, actual);
            AssertEx.Equal(expectedError, actual.ProtocolError);
            AssertEx.Equal<LMCCallbackWakeHint>(null, actual.WakeHint);
        }

        private static ulong JoinCookie(LMCCallbackWakeHint hint)
        {
            return ((ulong)hint.CookieHi << 32) | hint.CookieLo;
        }

        private static byte[] Clone(byte[] value)
        {
            return (byte[])value.Clone();
        }
    }
}
