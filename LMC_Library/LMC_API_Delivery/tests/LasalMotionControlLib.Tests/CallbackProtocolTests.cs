using System;
using System.Collections.Generic;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class CallbackProtocolTests
    {
        private const uint ApprovedMask = 0x11223344u;
        private const ushort ApprovedEventType = 0x2468;
        private const uint ApprovedEventId = 0xA1B2C3D4u;
        private const byte ApprovedDeliveryClass = 0x7E;
        private const ushort ApprovedStatus = 0x1234;
        private const short ApprovedErrorId = -1234;

        private static readonly byte[] CallbackIPv4 =
            new byte[] { 0xC0, 0xA8, 0x01, 0x0A };
        private static readonly byte[] SourceIPv4 =
            new byte[] { 0x0A, 0x01, 0x02, 0x03 };

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "CallbackProtocol.Legacy405C.Remains12Request4Response",
                Legacy405CRemains12Request4Response);
            tests.Add(
                "CallbackProtocol.V2Registration.ExactLittleEndianWire",
                V2RegistrationExactLittleEndianWire);
            tests.Add(
                "CallbackProtocol.V2Registration.ValidationMatrix",
                V2RegistrationValidationMatrix);
            tests.Add(
                "CallbackProtocol.Lmc2Datagram.Exact52ByteHeaderAndBounds",
                Lmc2DatagramExact52ByteHeaderAndBounds);
            tests.Add(
                "CallbackProtocol.Lmc2Datagram.ValidationMatrix",
                Lmc2DatagramValidationMatrix);
            tests.Add(
                "CallbackProtocol.UnknownIdentifiers.FailClosed",
                UnknownIdentifiersFailClosed);
        }

        private static void Legacy405CRemains12Request4Response()
        {
            AssertEx.Equal(
                12,
                LMCCallbackProtocol.LegacyRegistrationRequestPayloadBytes);
            AssertEx.Equal(
                4,
                LMCCallbackProtocol.LegacyRegistrationResponsePayloadBytes);
            AssertEx.SequenceEqual(
                TestFrame.Hex(
                    "5C 40 00 00 0C 00 00 00 "
                    + "44 33 22 11 04 03 02 01 7F 00 00 01"),
                LMC_Frame.RpcCallbackRegistration(
                    0x11223344u,
                    0x01020304,
                    new byte[] { 127, 0, 0, 1 }));

            var response = LMCConnection.ParseAcknowledgement(
                TestFrame.Response(
                    0,
                    TestFrame.Hex("34 12 2E FB")));
            AssertEx.True(response.HasCommandResult);
            AssertEx.Equal((ushort)0x1234, response.CommandStatus);
            AssertEx.Equal((short)-1234, response.ErrorId);
        }

        private static void V2RegistrationExactLittleEndianWire()
        {
            var policy = ApprovedPolicy();
            var request = ApprovedRequest();
            var expectedPayload = TestFrame.Hex(
                "44 33 22 11 34 12 00 00 C0 A8 01 0A "
                + "02 00 00 02 DD CC BB AA 44 33 22 11 "
                + "00 00 00 00 00 00 00 00");

            AssertEx.Equal(32, expectedPayload.Length);
            AssertEx.SequenceEqual(
                expectedPayload,
                LMCCallbackProtocol.EncodeRegistrationV2Payload(
                    request,
                    policy));
            AssertEx.SequenceEqual(
                TestFrame.Request(0x405C, 0, expectedPayload),
                LMCCallbackProtocol.CreateRegistrationV2Request(
                    request,
                    policy));

            var parsedRequest = LMCCallbackProtocol
                .ParseRegistrationV2Payload(
                    expectedPayload,
                    CallbackIPv4,
                    policy);
            AssertEx.True(parsedRequest.IsAccepted);
            AssertEx.Equal(ApprovedMask, parsedRequest.Value.EventMask);
            AssertEx.Equal(0x1234, parsedRequest.Value.CallbackPort);
            AssertEx.SequenceEqual(
                CallbackIPv4,
                parsedRequest.Value.CallbackIPv4);
            AssertEx.Equal((ushort)2, parsedRequest.Value.ProtocolVersion);
            AssertEx.Equal((ushort)512, parsedRequest.Value.RequestedMaxDatagram);
            AssertEx.Equal(0x11223344AABBCCDDUL, parsedRequest.Value.ClientCookie);
            AssertEx.Equal(0u, parsedRequest.Value.Flags);
            AssertEx.Equal(0u, parsedRequest.Value.Reserved);

            var responsePayload = ApprovedResponsePayload();
            AssertEx.Equal(20, responsePayload.Length);
            var parsedResponse = LMCCallbackProtocol
                .ParseRegistrationV2Response(
                    responsePayload,
                    request,
                    SourceIPv4,
                    7,
                    policy);
            AssertEx.True(parsedResponse.IsAccepted);
            AssertEx.Equal(ApprovedStatus, parsedResponse.Value.Status);
            AssertEx.Equal(ApprovedErrorId, parsedResponse.Value.ErrorId);
            AssertEx.Equal((ushort)2, parsedResponse.Value.AcceptedVersion);
            AssertEx.Equal((ushort)512, parsedResponse.Value.AcceptedMaxDatagram);
            AssertEx.Equal(0x01020304u, parsedResponse.Value.DiagnosticsBootId);
            AssertEx.Equal(0x55667788u, parsedResponse.Value.SessionEpoch);
            AssertEx.Equal(0u, parsedResponse.Value.AcceptedFlags);
            AssertEx.Equal(7L, parsedResponse.Value.SessionFence.ListenerGeneration);
            AssertEx.SequenceEqual(
                SourceIPv4,
                parsedResponse.Value.SessionFence.ExpectedSourceIPv4);
            AssertEx.Equal(ApprovedMask, parsedResponse.Value.SessionFence.RegisteredEventMask);
            AssertEx.Equal(
                0x11223344AABBCCDDUL,
                parsedResponse.Value.SessionFence.Cookie);

            var copiedAddress = parsedRequest.Value.CallbackIPv4;
            copiedAddress[0] = 0;
            AssertEx.SequenceEqual(
                CallbackIPv4,
                parsedRequest.Value.CallbackIPv4,
                "Parsed callback address must be immutable to callers.");
        }

        private static void V2RegistrationValidationMatrix()
        {
            var policy = ApprovedPolicy();
            var good = LMCCallbackProtocol.EncodeRegistrationV2Payload(
                ApprovedRequest(),
                policy);

            AssertRegistrationError(
                null,
                CallbackIPv4,
                policy,
                LMCCallbackProtocolError.NullPayload);
            AssertRegistrationError(
                new byte[31],
                CallbackIPv4,
                policy,
                LMCCallbackProtocolError.WrongLength);
            AssertRegistrationError(
                good,
                null,
                policy,
                LMCCallbackProtocolError.CallbackAddressInvalid);
            AssertRegistrationMutation(
                good,
                12,
                3,
                LMCCallbackProtocolError.ProtocolVersionMismatch);
            AssertRegistrationUInt32Mutation(
                good,
                0,
                0,
                LMCCallbackProtocolError.RegistrationMaskNotApproved);
            AssertRegistrationUInt32Mutation(
                good,
                4,
                0,
                LMCCallbackProtocolError.CallbackPortOutOfRange);
            AssertRegistrationUInt16Mutation(
                good,
                14,
                51,
                LMCCallbackProtocolError.MaxDatagramOutOfRange);
            var zeroCookie = Clone(good);
            TestFrame.WriteUInt32(zeroCookie, 16, 0);
            TestFrame.WriteUInt32(zeroCookie, 20, 0);
            AssertRegistrationError(
                zeroCookie,
                CallbackIPv4,
                policy,
                LMCCallbackProtocolError.CookieZero);
            AssertRegistrationUInt32Mutation(
                good,
                24,
                1,
                LMCCallbackProtocolError.FlagsNonZero);
            AssertRegistrationUInt32Mutation(
                good,
                28,
                1,
                LMCCallbackProtocolError.ReservedNonZero);

            var wrongPeer = Clone(CallbackIPv4);
            wrongPeer[3]++;
            AssertRegistrationError(
                good,
                wrongPeer,
                policy,
                LMCCallbackProtocolError.CallbackAddressMismatch);
            AssertRegistrationError(
                good,
                new byte[3],
                policy,
                LMCCallbackProtocolError.CallbackAddressInvalid);
            AssertEx.Throws<ArgumentException>(
                () => LMCCallbackProtocol.EncodeRegistrationV2Payload(
                    new LMCCallbackRegistrationV2Request(
                        ApprovedMask,
                        0x1234,
                        new byte[3],
                        512,
                        1,
                        1),
                    policy));

            var response = ApprovedResponsePayload();
            AssertResponseError(
                null,
                ApprovedRequest(),
                7,
                policy,
                LMCCallbackProtocolError.NullPayload);
            AssertResponseError(
                new byte[19],
                ApprovedRequest(),
                7,
                policy,
                LMCCallbackProtocolError.WrongLength);
            AssertResponseError(
                response,
                ApprovedRequest(),
                0,
                policy,
                LMCCallbackProtocolError.ListenerGenerationInvalid);
            var missingSource = LMCCallbackProtocol
                .ParseRegistrationV2Response(
                    response,
                    ApprovedRequest(),
                    null,
                    7,
                    policy);
            AssertEx.False(missingSource.IsAccepted);
            AssertEx.Equal(
                LMCCallbackProtocolError.CallbackSourceAddressInvalid,
                missingSource.Error);
            AssertResponseMutation(
                response,
                0,
                0,
                LMCCallbackProtocolError.RegistrationResultNotApproved);
            AssertResponseMutation(
                response,
                4,
                3,
                LMCCallbackProtocolError.ProtocolVersionMismatch);
            AssertResponseMutation(
                response,
                6,
                51,
                LMCCallbackProtocolError.AcceptedMaxDatagramInvalid);
            AssertResponseUInt32Mutation(
                response,
                8,
                0,
                LMCCallbackProtocolError.BootIdZero);
            AssertResponseUInt32Mutation(
                response,
                12,
                0,
                LMCCallbackProtocolError.SessionEpochZero);
            AssertResponseUInt32Mutation(
                response,
                16,
                1,
                LMCCallbackProtocolError.FlagsNonZero);

            var smallerRequest = new LMCCallbackRegistrationV2Request(
                ApprovedMask,
                0x1234,
                CallbackIPv4,
                256,
                0xAABBCCDDu,
                0x11223344u);
            AssertResponseError(
                response,
                smallerRequest,
                7,
                policy,
                LMCCallbackProtocolError.AcceptedMaxDatagramInvalid);
            AssertEx.Throws<ArgumentException>(
                () => LMCCallbackProtocol.CreateRegistrationV2Request(
                    ApprovedRequest(),
                    LMCCallbackProtocolPolicy.FailClosed));
        }

        private static void Lmc2DatagramExact52ByteHeaderAndBounds()
        {
            var policy = ApprovedPolicy();
            var fence = ApprovedFence(policy);
            var write = ApprovedDatagram(
                0x1122334455667788UL,
                new byte[] { 0xAA, 0xBB, 0xCC });
            var expected = TestFrame.Hex(
                "4C 4D 43 32 02 00 34 00 37 00 68 24 "
                + "04 00 00 00 04 03 02 01 88 77 66 55 "
                + "DD CC BB AA 44 33 22 11 88 77 66 55 "
                + "44 33 22 11 D4 C3 B2 A1 04 03 02 01 "
                + "03 00 7E 00 AA BB CC");

            AssertEx.Equal(55, expected.Length);
            var encoded = LMCCallbackProtocol.EncodeDatagram(
                write,
                fence,
                policy);
            AssertEx.SequenceEqual(expected, encoded);
            AssertEx.Equal(52, LMCCallbackProtocol.DatagramHeaderBytes);
            AssertEx.Equal(512, LMCCallbackProtocol.MaxDatagramBytes);
            AssertEx.Equal(460, LMCCallbackProtocol.MaxPayloadBytes);

            var parsed = LMCCallbackProtocol.ParseDatagram(
                encoded,
                fence,
                policy);
            AssertEx.True(parsed.IsAccepted);
            AssertEx.Equal(ApprovedEventType, parsed.Value.EventType);
            AssertEx.Equal(4u, parsed.Value.EventMaskBit);
            AssertEx.Equal(0x1122334455667788UL, parsed.Value.Sequence);
            AssertEx.Equal(ApprovedEventId, parsed.Value.EventId);
            AssertEx.Equal(0x01020304u, parsed.Value.PlcTimeMs);
            AssertEx.Equal(ApprovedDeliveryClass, parsed.Value.DeliveryClass);
            AssertEx.SequenceEqual(
                new byte[] { 0xAA, 0xBB, 0xCC },
                parsed.Value.Payload);

            var maximumPayload = new byte[460];
            maximumPayload[0] = 0xA5;
            maximumPayload[459] = 0x5A;
            var maximum = LMCCallbackProtocol.EncodeDatagram(
                ApprovedDatagram(1, maximumPayload),
                fence,
                policy);
            AssertEx.Equal(512, maximum.Length);
            AssertEx.True(
                LMCCallbackProtocol.ParseDatagram(maximum, fence, policy)
                    .IsAccepted);
            AssertEx.Throws<ArgumentException>(
                () => LMCCallbackProtocol.EncodeDatagram(
                    ApprovedDatagram(2, new byte[461]),
                    fence,
                    policy));
        }

        private static void Lmc2DatagramValidationMatrix()
        {
            var policy = ApprovedPolicy();
            var fence = ApprovedFence(policy);
            var good = LMCCallbackProtocol.EncodeDatagram(
                ApprovedDatagram(1, new byte[] { 0xAA, 0xBB, 0xCC }),
                fence,
                policy);

            AssertDatagramError(
                null,
                fence,
                policy,
                LMCCallbackProtocolError.NullPayload);
            AssertDatagramError(
                new byte[51],
                fence,
                policy,
                LMCCallbackProtocolError.WrongLength);
            AssertDatagramByteMutation(
                good,
                0,
                0,
                LMCCallbackProtocolError.MagicMismatch);
            AssertDatagramUInt16Mutation(
                good,
                4,
                3,
                LMCCallbackProtocolError.ProtocolVersionMismatch);
            AssertDatagramUInt16Mutation(
                good,
                6,
                51,
                LMCCallbackProtocolError.HeaderLengthMismatch);
            AssertDatagramUInt16Mutation(
                good,
                8,
                54,
                LMCCallbackProtocolError.DatagramLengthMismatch);
            AssertDatagramByteMutation(
                good,
                51,
                1,
                LMCCallbackProtocolError.FlagsNonZero);
            AssertDatagramUInt16Mutation(
                good,
                48,
                2,
                LMCCallbackProtocolError.PayloadLengthMismatch);
            AssertDatagramUInt32Mutation(
                good,
                12,
                0,
                LMCCallbackProtocolError.EventMaskNotSingleBit);
            AssertDatagramUInt32Mutation(
                good,
                12,
                3,
                LMCCallbackProtocolError.EventMaskNotSingleBit);
            AssertDatagramUInt32Mutation(
                good,
                12,
                0x80000000u,
                LMCCallbackProtocolError.EventMaskNotSubscribed);
            AssertDatagramUInt16Mutation(
                good,
                10,
                0x2469,
                LMCCallbackProtocolError.EventIdentifierNotApproved);
            AssertDatagramByteMutation(
                good,
                50,
                0x7F,
                LMCCallbackProtocolError.DeliveryClassNotApproved);
            AssertDatagramUInt32Mutation(
                good,
                16,
                0x01020305u,
                LMCCallbackProtocolError.StaleBootId);
            AssertDatagramUInt32Mutation(
                good,
                20,
                0x55667789u,
                LMCCallbackProtocolError.StaleSessionEpoch);
            AssertDatagramUInt32Mutation(
                good,
                24,
                0xAABBCCDEu,
                LMCCallbackProtocolError.StaleCookie);

            var tooLarge = new byte[513];
            Buffer.BlockCopy(good, 0, tooLarge, 0, good.Length);
            AssertDatagramError(
                tooLarge,
                fence,
                policy,
                LMCCallbackProtocolError.DatagramTooLarge);

            var declaredPayloadTooLarge = new byte[512];
            Buffer.BlockCopy(good, 0, declaredPayloadTooLarge, 0, good.Length);
            TestFrame.WriteUInt16(declaredPayloadTooLarge, 8, 512);
            TestFrame.WriteUInt16(declaredPayloadTooLarge, 48, 461);
            AssertDatagramError(
                declaredPayloadTooLarge,
                fence,
                policy,
                LMCCallbackProtocolError.PayloadTooLarge);

            var smallFence = new LMCCallbackSessionFence(
                7,
                SourceIPv4,
                ApprovedMask,
                54,
                fence.BootId,
                fence.SessionEpoch,
                fence.CookieLo,
                fence.CookieHi);
            AssertDatagramError(
                good,
                smallFence,
                policy,
                LMCCallbackProtocolError.DatagramTooLarge);
        }

        private static void UnknownIdentifiersFailClosed()
        {
            var failClosed = LMCCallbackProtocolPolicy.FailClosed;
            AssertEx.Throws<ArgumentException>(
                () => LMCCallbackProtocol.EncodeRegistrationV2Payload(
                    ApprovedRequest(),
                    failClosed));

            var policyWithoutApprovedIdentifiers =
                new LMCCallbackProtocolPolicy(
                    value => value == ApprovedMask,
                    null,
                    null,
                    (status, errorId) => status == ApprovedStatus
                        && errorId == ApprovedErrorId);
            var fence = ApprovedFence(ApprovedPolicy());
            var encoded = LMCCallbackProtocol.EncodeDatagram(
                ApprovedDatagram(1, new byte[0]),
                fence,
                ApprovedPolicy());
            AssertDatagramError(
                encoded,
                fence,
                policyWithoutApprovedIdentifiers,
                LMCCallbackProtocolError.EventIdentifierNotApproved);

            var eventThrowingPolicy = new LMCCallbackProtocolPolicy(
                value => value == ApprovedMask,
                (eventType, eventId) =>
                    { throw new InvalidOperationException(); },
                value => value == ApprovedDeliveryClass,
                (status, errorId) => status == ApprovedStatus
                    && errorId == ApprovedErrorId);
            AssertDatagramError(
                encoded,
                fence,
                eventThrowingPolicy,
                LMCCallbackProtocolError.EventIdentifierNotApproved);

            var deliveryThrowingPolicy = new LMCCallbackProtocolPolicy(
                value => value == ApprovedMask,
                (eventType, eventId) => eventType == ApprovedEventType
                    && eventId == ApprovedEventId,
                value => { throw new InvalidOperationException(); },
                (status, errorId) => status == ApprovedStatus
                    && errorId == ApprovedErrorId);
            AssertDatagramError(
                encoded,
                fence,
                deliveryThrowingPolicy,
                LMCCallbackProtocolError.DeliveryClassNotApproved);

            var resultThrowingPolicy = new LMCCallbackProtocolPolicy(
                value => value == ApprovedMask,
                (eventType, eventId) => true,
                value => true,
                (status, errorId) =>
                    { throw new InvalidOperationException(); });
            AssertResponseError(
                ApprovedResponsePayload(),
                ApprovedRequest(),
                7,
                resultThrowingPolicy,
                LMCCallbackProtocolError.RegistrationResultNotApproved);

            var throwingPolicy = new LMCCallbackProtocolPolicy(
                value => { throw new InvalidOperationException(); },
                (eventType, eventId) =>
                    { throw new InvalidOperationException(); },
                value => { throw new InvalidOperationException(); },
                (status, errorId) =>
                    { throw new InvalidOperationException(); });
            AssertRegistrationError(
                LMCCallbackProtocol.EncodeRegistrationV2Payload(
                    ApprovedRequest(),
                    ApprovedPolicy()),
                CallbackIPv4,
                throwingPolicy,
                LMCCallbackProtocolError.RegistrationMaskNotApproved);

            AssertEx.Throws<ArgumentException>(
                () => LMCCallbackProtocol.EncodeDatagram(
                    new LMCCallbackDatagramWrite(
                        ApprovedEventType,
                        3,
                        1,
                        ApprovedEventId,
                        0,
                        ApprovedDeliveryClass,
                        new byte[0]),
                    fence,
                    ApprovedPolicy()));
            AssertEx.Throws<ArgumentException>(
                () => LMCCallbackProtocol.EncodeDatagram(
                    new LMCCallbackDatagramWrite(
                        0x2469,
                        4,
                        1,
                        ApprovedEventId,
                        0,
                        ApprovedDeliveryClass,
                        new byte[0]),
                    fence,
                    ApprovedPolicy()));
            AssertEx.Throws<ArgumentException>(
                () => LMCCallbackProtocol.EncodeDatagram(
                    new LMCCallbackDatagramWrite(
                        ApprovedEventType,
                        4,
                        1,
                        ApprovedEventId,
                        0,
                        0x7F,
                        new byte[0]),
                    fence,
                    ApprovedPolicy()));
            AssertEx.Throws<ArgumentException>(
                () => LMCCallbackProtocol.EncodeDatagram(
                    new LMCCallbackDatagramWrite(
                        ApprovedEventType,
                        4,
                        1,
                        ApprovedEventId,
                        0,
                        ApprovedDeliveryClass,
                        1,
                        new byte[0]),
                    fence,
                    ApprovedPolicy()));
        }

        internal static LMCCallbackProtocolPolicy ApprovedPolicy()
        {
            return new LMCCallbackProtocolPolicy(
                value => value == ApprovedMask,
                (eventType, eventId) => eventType == ApprovedEventType
                    && eventId == ApprovedEventId,
                value => value == ApprovedDeliveryClass,
                (status, errorId) => status == ApprovedStatus
                    && errorId == ApprovedErrorId);
        }

        internal static LMCCallbackRegistrationV2Request ApprovedRequest()
        {
            return new LMCCallbackRegistrationV2Request(
                ApprovedMask,
                0x1234,
                CallbackIPv4,
                512,
                0xAABBCCDDu,
                0x11223344u);
        }

        internal static byte[] ApprovedResponsePayload()
        {
            return TestFrame.Hex(
                "34 12 2E FB 02 00 00 02 "
                + "04 03 02 01 88 77 66 55 00 00 00 00");
        }

        internal static LMCCallbackSessionFence ApprovedFence(
            LMCCallbackProtocolPolicy policy)
        {
            var parsed = LMCCallbackProtocol.ParseRegistrationV2Response(
                ApprovedResponsePayload(),
                ApprovedRequest(),
                SourceIPv4,
                7,
                policy);
            AssertEx.True(parsed.IsAccepted);
            return parsed.Value.SessionFence;
        }

        internal static byte[] ApprovedSourceIPv4()
        {
            return Clone(SourceIPv4);
        }

        internal static LMCCallbackDatagramWrite ApprovedDatagram(
            ulong sequence,
            byte[] payload)
        {
            return new LMCCallbackDatagramWrite(
                ApprovedEventType,
                4,
                sequence,
                ApprovedEventId,
                0x01020304u,
                ApprovedDeliveryClass,
                payload);
        }

        private static void AssertRegistrationMutation(
            byte[] source,
            int offset,
            byte value,
            LMCCallbackProtocolError expected)
        {
            var mutated = Clone(source);
            mutated[offset] = value;
            AssertRegistrationError(
                mutated,
                CallbackIPv4,
                ApprovedPolicy(),
                expected);
        }

        private static void AssertRegistrationUInt16Mutation(
            byte[] source,
            int offset,
            ushort value,
            LMCCallbackProtocolError expected)
        {
            var mutated = Clone(source);
            TestFrame.WriteUInt16(mutated, offset, value);
            AssertRegistrationError(
                mutated,
                CallbackIPv4,
                ApprovedPolicy(),
                expected);
        }

        private static void AssertRegistrationUInt32Mutation(
            byte[] source,
            int offset,
            uint value,
            LMCCallbackProtocolError expected)
        {
            var mutated = Clone(source);
            TestFrame.WriteUInt32(mutated, offset, value);
            AssertRegistrationError(
                mutated,
                CallbackIPv4,
                ApprovedPolicy(),
                expected);
        }

        private static void AssertRegistrationError(
            byte[] payload,
            byte[] expectedPeerIPv4,
            LMCCallbackProtocolPolicy policy,
            LMCCallbackProtocolError expected)
        {
            var result = LMCCallbackProtocol.ParseRegistrationV2Payload(
                payload,
                expectedPeerIPv4,
                policy);
            AssertEx.False(result.IsAccepted);
            AssertEx.Equal(expected, result.Error);
        }

        private static void AssertResponseMutation(
            byte[] source,
            int offset,
            ushort value,
            LMCCallbackProtocolError expected)
        {
            var mutated = Clone(source);
            TestFrame.WriteUInt16(mutated, offset, value);
            AssertResponseError(
                mutated,
                ApprovedRequest(),
                7,
                ApprovedPolicy(),
                expected);
        }

        private static void AssertResponseUInt32Mutation(
            byte[] source,
            int offset,
            uint value,
            LMCCallbackProtocolError expected)
        {
            var mutated = Clone(source);
            TestFrame.WriteUInt32(mutated, offset, value);
            AssertResponseError(
                mutated,
                ApprovedRequest(),
                7,
                ApprovedPolicy(),
                expected);
        }

        private static void AssertResponseError(
            byte[] payload,
            LMCCallbackRegistrationV2Request request,
            long listenerGeneration,
            LMCCallbackProtocolPolicy policy,
            LMCCallbackProtocolError expected)
        {
            var result = LMCCallbackProtocol.ParseRegistrationV2Response(
                payload,
                request,
                SourceIPv4,
                listenerGeneration,
                policy);
            AssertEx.False(result.IsAccepted);
            AssertEx.Equal(expected, result.Error);
        }

        private static void AssertDatagramByteMutation(
            byte[] source,
            int offset,
            byte value,
            LMCCallbackProtocolError expected)
        {
            var mutated = Clone(source);
            mutated[offset] = value;
            AssertDatagramError(
                mutated,
                ApprovedFence(ApprovedPolicy()),
                ApprovedPolicy(),
                expected);
        }

        private static void AssertDatagramUInt16Mutation(
            byte[] source,
            int offset,
            ushort value,
            LMCCallbackProtocolError expected)
        {
            var mutated = Clone(source);
            TestFrame.WriteUInt16(mutated, offset, value);
            AssertDatagramError(
                mutated,
                ApprovedFence(ApprovedPolicy()),
                ApprovedPolicy(),
                expected);
        }

        private static void AssertDatagramUInt32Mutation(
            byte[] source,
            int offset,
            uint value,
            LMCCallbackProtocolError expected)
        {
            var mutated = Clone(source);
            TestFrame.WriteUInt32(mutated, offset, value);
            AssertDatagramError(
                mutated,
                ApprovedFence(ApprovedPolicy()),
                ApprovedPolicy(),
                expected);
        }

        private static void AssertDatagramError(
            byte[] datagram,
            LMCCallbackSessionFence fence,
            LMCCallbackProtocolPolicy policy,
            LMCCallbackProtocolError expected)
        {
            var result = LMCCallbackProtocol.ParseDatagram(
                datagram,
                fence,
                policy);
            AssertEx.False(result.IsAccepted);
            AssertEx.Equal(expected, result.Error);
        }

        private static byte[] Clone(byte[] value)
        {
            return value == null ? null : (byte[])value.Clone();
        }
    }
}
