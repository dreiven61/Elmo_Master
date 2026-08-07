using System;

namespace LasalMotionControlLib
{
    internal static class LMCCallbackProtocol
    {
        internal const ushort ProtocolVersion2 = 2;
        internal const int LegacyRegistrationRequestPayloadBytes = 12;
        internal const int LegacyRegistrationResponsePayloadBytes = 4;
        internal const int RegistrationV2RequestPayloadBytes = 32;
        internal const int RegistrationV2ResponsePayloadBytes = 20;
        internal const int DatagramHeaderBytes = 52;
        internal const int MaxDatagramBytes = 512;
        internal const int MaxPayloadBytes = 460;

        private const int RegistrationEventMaskOffset = 0;
        private const int RegistrationCallbackPortOffset = 4;
        private const int RegistrationCallbackIPv4Offset = 8;
        private const int RegistrationProtocolVersionOffset = 12;
        private const int RegistrationRequestedMaxOffset = 14;
        private const int RegistrationCookieLoOffset = 16;
        private const int RegistrationCookieHiOffset = 20;
        private const int RegistrationFlagsOffset = 24;
        private const int RegistrationReservedOffset = 28;

        private const int ResponseStatusOffset = 0;
        private const int ResponseErrorIdOffset = 2;
        private const int ResponseAcceptedVersionOffset = 4;
        private const int ResponseAcceptedMaxOffset = 6;
        private const int ResponseBootIdOffset = 8;
        private const int ResponseSessionEpochOffset = 12;
        private const int ResponseAcceptedFlagsOffset = 16;

        private const int DatagramMagicOffset = 0;
        private const int DatagramVersionOffset = 4;
        private const int DatagramHeaderBytesOffset = 6;
        private const int DatagramBytesOffset = 8;
        private const int DatagramEventTypeOffset = 10;
        private const int DatagramEventMaskBitOffset = 12;
        private const int DatagramBootIdOffset = 16;
        private const int DatagramSessionEpochOffset = 20;
        private const int DatagramCookieLoOffset = 24;
        private const int DatagramCookieHiOffset = 28;
        private const int DatagramSequenceLoOffset = 32;
        private const int DatagramSequenceHiOffset = 36;
        private const int DatagramEventIdOffset = 40;
        private const int DatagramPlcTimeMsOffset = 44;
        private const int DatagramPayloadBytesOffset = 48;
        private const int DatagramDeliveryClassOffset = 50;
        private const int DatagramFlagsOffset = 51;

        private static readonly byte[] DatagramMagic =
            new byte[] { 0x4C, 0x4D, 0x43, 0x32 };

        internal static byte[] CreateRegistrationV2Request(
            LMCCallbackRegistrationV2Request registration,
            LMCCallbackProtocolPolicy policy)
        {
            var payload = EncodeRegistrationV2Payload(registration, policy);
            var request = LMC_Frame.CreateRequest(
                LMC_CommandId.RpcCallbackRegistration,
                0,
                RegistrationV2RequestPayloadBytes);
            Buffer.BlockCopy(
                payload,
                0,
                request,
                LMC_Frame.HeaderSize,
                payload.Length);
            return request;
        }

        internal static byte[] EncodeRegistrationV2Payload(
            LMCCallbackRegistrationV2Request registration,
            LMCCallbackProtocolPolicy policy)
        {
            var error = ValidateRegistration(
                registration,
                null,
                false,
                policy);
            ThrowIfInvalid(error, "registration");

            var payload = new byte[RegistrationV2RequestPayloadBytes];
            WriteUInt32(
                payload,
                RegistrationEventMaskOffset,
                registration.EventMask);
            WriteInt32(
                payload,
                RegistrationCallbackPortOffset,
                registration.CallbackPort);
            Buffer.BlockCopy(
                registration.CallbackIPv4,
                0,
                payload,
                RegistrationCallbackIPv4Offset,
                4);
            WriteUInt16(
                payload,
                RegistrationProtocolVersionOffset,
                ProtocolVersion2);
            WriteUInt16(
                payload,
                RegistrationRequestedMaxOffset,
                registration.RequestedMaxDatagram);
            WriteUInt32(
                payload,
                RegistrationCookieLoOffset,
                registration.ClientCookieLo);
            WriteUInt32(
                payload,
                RegistrationCookieHiOffset,
                registration.ClientCookieHi);
            WriteUInt32(payload, RegistrationFlagsOffset, registration.Flags);
            WriteUInt32(
                payload,
                RegistrationReservedOffset,
                registration.Reserved);
            return payload;
        }

        internal static LMCCallbackParseResult<LMCCallbackRegistrationV2Request>
            ParseRegistrationV2Payload(
                byte[] payload,
                byte[] expectedPeerIPv4,
                LMCCallbackProtocolPolicy policy)
        {
            if (payload == null)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Request>
                    .Reject(LMCCallbackProtocolError.NullPayload);
            }
            if (payload.Length != RegistrationV2RequestPayloadBytes)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Request>
                    .Reject(LMCCallbackProtocolError.WrongLength);
            }

            var callbackIPv4 = new byte[4];
            Buffer.BlockCopy(
                payload,
                RegistrationCallbackIPv4Offset,
                callbackIPv4,
                0,
                callbackIPv4.Length);
            if (ReadUInt16(payload, RegistrationProtocolVersionOffset)
                != ProtocolVersion2)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Request>
                    .Reject(LMCCallbackProtocolError.ProtocolVersionMismatch);
            }

            var registration = new LMCCallbackRegistrationV2Request(
                ReadUInt32(payload, RegistrationEventMaskOffset),
                ReadInt32(payload, RegistrationCallbackPortOffset),
                callbackIPv4,
                ReadUInt16(payload, RegistrationRequestedMaxOffset),
                ReadUInt32(payload, RegistrationCookieLoOffset),
                ReadUInt32(payload, RegistrationCookieHiOffset),
                ReadUInt32(payload, RegistrationFlagsOffset),
                ReadUInt32(payload, RegistrationReservedOffset));
            var error = ValidateRegistration(
                registration,
                expectedPeerIPv4,
                true,
                policy);
            if (error != LMCCallbackProtocolError.None)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Request>
                    .Reject(error);
            }

            return LMCCallbackParseResult<LMCCallbackRegistrationV2Request>
                .Accept(registration);
        }

        internal static LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
            ParseRegistrationV2Response(
                byte[] payload,
                LMCCallbackRegistrationV2Request registration,
                byte[] expectedSourceIPv4,
                long listenerGeneration,
                LMCCallbackProtocolPolicy policy)
        {
            if (payload == null)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
                    .Reject(LMCCallbackProtocolError.NullPayload);
            }
            if (payload.Length != RegistrationV2ResponsePayloadBytes)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
                    .Reject(LMCCallbackProtocolError.WrongLength);
            }

            var registrationError = ValidateRegistration(
                registration,
                null,
                false,
                policy);
            if (registrationError != LMCCallbackProtocolError.None)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
                    .Reject(registrationError);
            }
            if (listenerGeneration <= 0)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
                    .Reject(
                        LMCCallbackProtocolError.ListenerGenerationInvalid);
            }
            if (!IsValidIPv4(expectedSourceIPv4))
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
                    .Reject(
                        LMCCallbackProtocolError.CallbackSourceAddressInvalid);
            }

            var status = ReadUInt16(payload, ResponseStatusOffset);
            var errorId = ReadInt16(payload, ResponseErrorIdOffset);
            if (policy == null
                || !policy.ApprovesRegistrationResult(status, errorId))
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
                    .Reject(
                        LMCCallbackProtocolError.RegistrationResultNotApproved);
            }

            var acceptedVersion = ReadUInt16(
                payload,
                ResponseAcceptedVersionOffset);
            if (acceptedVersion != ProtocolVersion2)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
                    .Reject(LMCCallbackProtocolError.ProtocolVersionMismatch);
            }

            var acceptedMax = ReadUInt16(payload, ResponseAcceptedMaxOffset);
            if (!IsMaxDatagramValid(acceptedMax)
                || acceptedMax > registration.RequestedMaxDatagram)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
                    .Reject(
                        LMCCallbackProtocolError.AcceptedMaxDatagramInvalid);
            }

            var bootId = ReadUInt32(payload, ResponseBootIdOffset);
            if (bootId == 0)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
                    .Reject(LMCCallbackProtocolError.BootIdZero);
            }
            var sessionEpoch = ReadUInt32(
                payload,
                ResponseSessionEpochOffset);
            if (sessionEpoch == 0)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
                    .Reject(LMCCallbackProtocolError.SessionEpochZero);
            }
            var acceptedFlags = ReadUInt32(
                payload,
                ResponseAcceptedFlagsOffset);
            if (acceptedFlags != 0)
            {
                return LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
                    .Reject(LMCCallbackProtocolError.FlagsNonZero);
            }

            var fence = new LMCCallbackSessionFence(
                listenerGeneration,
                expectedSourceIPv4,
                registration.EventMask,
                acceptedMax,
                bootId,
                sessionEpoch,
                registration.ClientCookieLo,
                registration.ClientCookieHi);
            var response = new LMCCallbackRegistrationV2Response(
                status,
                errorId,
                acceptedVersion,
                acceptedMax,
                bootId,
                sessionEpoch,
                acceptedFlags,
                fence);
            return LMCCallbackParseResult<LMCCallbackRegistrationV2Response>
                .Accept(response);
        }

        internal static byte[] EncodeDatagram(
            LMCCallbackDatagramWrite datagram,
            LMCCallbackSessionFence fence,
            LMCCallbackProtocolPolicy policy)
        {
            if (datagram == null)
            {
                throw new ArgumentNullException("datagram");
            }
            var fenceError = ValidateFence(fence, policy);
            ThrowIfInvalid(fenceError, "fence");
            var payload = datagram.Payload;
            var datagramError = ValidateDatagramFields(
                datagram.EventType,
                datagram.EventMaskBit,
                datagram.EventId,
                datagram.DeliveryClass,
                datagram.Flags,
                payload.Length,
                fence,
                policy);
            ThrowIfInvalid(datagramError, "datagram");

            var datagramBytes = DatagramHeaderBytes + payload.Length;
            var buffer = new byte[datagramBytes];
            Buffer.BlockCopy(
                DatagramMagic,
                0,
                buffer,
                DatagramMagicOffset,
                DatagramMagic.Length);
            WriteUInt16(buffer, DatagramVersionOffset, ProtocolVersion2);
            WriteUInt16(buffer, DatagramHeaderBytesOffset, DatagramHeaderBytes);
            WriteUInt16(buffer, DatagramBytesOffset, datagramBytes);
            WriteUInt16(buffer, DatagramEventTypeOffset, datagram.EventType);
            WriteUInt32(
                buffer,
                DatagramEventMaskBitOffset,
                datagram.EventMaskBit);
            WriteUInt32(buffer, DatagramBootIdOffset, fence.BootId);
            WriteUInt32(
                buffer,
                DatagramSessionEpochOffset,
                fence.SessionEpoch);
            WriteUInt32(buffer, DatagramCookieLoOffset, fence.CookieLo);
            WriteUInt32(buffer, DatagramCookieHiOffset, fence.CookieHi);
            WriteUInt32(
                buffer,
                DatagramSequenceLoOffset,
                (uint)(datagram.Sequence & uint.MaxValue));
            WriteUInt32(
                buffer,
                DatagramSequenceHiOffset,
                (uint)(datagram.Sequence >> 32));
            WriteUInt32(buffer, DatagramEventIdOffset, datagram.EventId);
            WriteUInt32(buffer, DatagramPlcTimeMsOffset, datagram.PlcTimeMs);
            WriteUInt16(buffer, DatagramPayloadBytesOffset, payload.Length);
            buffer[DatagramDeliveryClassOffset] = datagram.DeliveryClass;
            buffer[DatagramFlagsOffset] = datagram.Flags;
            Buffer.BlockCopy(
                payload,
                0,
                buffer,
                DatagramHeaderBytes,
                payload.Length);
            return buffer;
        }

        internal static LMCCallbackParseResult<LMCCallbackDatagram>
            ParseDatagram(
                byte[] buffer,
                LMCCallbackSessionFence fence,
                LMCCallbackProtocolPolicy policy)
        {
            if (buffer == null)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(LMCCallbackProtocolError.NullPayload);
            }
            var fenceError = ValidateFenceShape(fence);
            if (fenceError != LMCCallbackProtocolError.None)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(fenceError);
            }
            if (buffer.Length < DatagramHeaderBytes)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(LMCCallbackProtocolError.WrongLength);
            }
            if (buffer.Length > MaxDatagramBytes
                || buffer.Length > fence.MaxDatagramBytes)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(LMCCallbackProtocolError.DatagramTooLarge);
            }
            for (var index = 0; index < DatagramMagic.Length; index++)
            {
                if (buffer[DatagramMagicOffset + index]
                    != DatagramMagic[index])
                {
                    return LMCCallbackParseResult<LMCCallbackDatagram>
                        .Reject(LMCCallbackProtocolError.MagicMismatch);
                }
            }
            if (ReadUInt16(buffer, DatagramVersionOffset) != ProtocolVersion2)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(LMCCallbackProtocolError.ProtocolVersionMismatch);
            }
            if (ReadUInt16(buffer, DatagramHeaderBytesOffset)
                != DatagramHeaderBytes)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(LMCCallbackProtocolError.HeaderLengthMismatch);
            }
            if (ReadUInt16(buffer, DatagramBytesOffset) != buffer.Length)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(LMCCallbackProtocolError.DatagramLengthMismatch);
            }
            if (buffer[DatagramFlagsOffset] != 0)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(LMCCallbackProtocolError.FlagsNonZero);
            }

            var payloadBytes = ReadUInt16(
                buffer,
                DatagramPayloadBytesOffset);
            if (payloadBytes > MaxPayloadBytes)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(LMCCallbackProtocolError.PayloadTooLarge);
            }
            if (DatagramHeaderBytes + payloadBytes != buffer.Length)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(LMCCallbackProtocolError.PayloadLengthMismatch);
            }

            var eventType = ReadUInt16(buffer, DatagramEventTypeOffset);
            var eventMaskBit = ReadUInt32(
                buffer,
                DatagramEventMaskBitOffset);
            var bootId = ReadUInt32(buffer, DatagramBootIdOffset);
            var sessionEpoch = ReadUInt32(
                buffer,
                DatagramSessionEpochOffset);
            var cookieLo = ReadUInt32(buffer, DatagramCookieLoOffset);
            var cookieHi = ReadUInt32(buffer, DatagramCookieHiOffset);
            var eventId = ReadUInt32(buffer, DatagramEventIdOffset);
            var deliveryClass = buffer[DatagramDeliveryClassOffset];

            if (bootId != fence.BootId)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(LMCCallbackProtocolError.StaleBootId);
            }
            if (sessionEpoch != fence.SessionEpoch)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(LMCCallbackProtocolError.StaleSessionEpoch);
            }
            if (cookieLo != fence.CookieLo || cookieHi != fence.CookieHi)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(LMCCallbackProtocolError.StaleCookie);
            }

            var fieldError = ValidateDatagramFields(
                eventType,
                eventMaskBit,
                eventId,
                deliveryClass,
                buffer[DatagramFlagsOffset],
                payloadBytes,
                fence,
                policy);
            if (fieldError != LMCCallbackProtocolError.None)
            {
                return LMCCallbackParseResult<LMCCallbackDatagram>
                    .Reject(fieldError);
            }

            var sequence = ((ulong)ReadUInt32(
                    buffer,
                    DatagramSequenceHiOffset) << 32)
                | ReadUInt32(buffer, DatagramSequenceLoOffset);
            var payload = new byte[payloadBytes];
            Buffer.BlockCopy(
                buffer,
                DatagramHeaderBytes,
                payload,
                0,
                payload.Length);
            var datagram = new LMCCallbackDatagram(
                eventType,
                eventMaskBit,
                bootId,
                sessionEpoch,
                cookieLo,
                cookieHi,
                sequence,
                eventId,
                ReadUInt32(buffer, DatagramPlcTimeMsOffset),
                deliveryClass,
                payload);
            return LMCCallbackParseResult<LMCCallbackDatagram>
                .Accept(datagram);
        }

        private static LMCCallbackProtocolError ValidateRegistration(
            LMCCallbackRegistrationV2Request registration,
            byte[] expectedPeerIPv4,
            bool requireExpectedPeer,
            LMCCallbackProtocolPolicy policy)
        {
            if (registration == null)
            {
                return LMCCallbackProtocolError.NullPayload;
            }
            if (policy == null
                || registration.EventMask == 0
                || !policy.ApprovesRegistrationMask(registration.EventMask))
            {
                return LMCCallbackProtocolError.RegistrationMaskNotApproved;
            }
            if (registration.CallbackPort < 1
                || registration.CallbackPort > 65535)
            {
                return LMCCallbackProtocolError.CallbackPortOutOfRange;
            }
            var callbackIPv4 = registration.CallbackIPv4;
            if (callbackIPv4.Length != 4)
            {
                return LMCCallbackProtocolError.CallbackAddressInvalid;
            }
            if (requireExpectedPeer && !IsValidIPv4(expectedPeerIPv4))
            {
                return LMCCallbackProtocolError.CallbackAddressInvalid;
            }
            if (expectedPeerIPv4 != null)
            {
                if (!IsValidIPv4(expectedPeerIPv4))
                {
                    return LMCCallbackProtocolError.CallbackAddressInvalid;
                }
                for (var index = 0; index < callbackIPv4.Length; index++)
                {
                    if (callbackIPv4[index] != expectedPeerIPv4[index])
                    {
                        return LMCCallbackProtocolError.CallbackAddressMismatch;
                    }
                }
            }
            if (!IsMaxDatagramValid(registration.RequestedMaxDatagram))
            {
                return LMCCallbackProtocolError.MaxDatagramOutOfRange;
            }
            if (registration.ClientCookie == 0)
            {
                return LMCCallbackProtocolError.CookieZero;
            }
            if (registration.Flags != 0)
            {
                return LMCCallbackProtocolError.FlagsNonZero;
            }
            if (registration.Reserved != 0)
            {
                return LMCCallbackProtocolError.ReservedNonZero;
            }
            return LMCCallbackProtocolError.None;
        }

        private static LMCCallbackProtocolError ValidateFence(
            LMCCallbackSessionFence fence,
            LMCCallbackProtocolPolicy policy)
        {
            var shapeError = ValidateFenceShape(fence);
            if (shapeError != LMCCallbackProtocolError.None)
            {
                return shapeError;
            }
            if (policy == null
                || !policy.ApprovesRegistrationMask(
                    fence.RegisteredEventMask))
            {
                return LMCCallbackProtocolError.RegistrationMaskNotApproved;
            }
            return LMCCallbackProtocolError.None;
        }

        internal static LMCCallbackProtocolError ValidateReceiverFence(
            LMCCallbackSessionFence fence,
            LMCCallbackProtocolPolicy policy)
        {
            return ValidateFence(fence, policy);
        }

        private static LMCCallbackProtocolError ValidateFenceShape(
            LMCCallbackSessionFence fence)
        {
            if (fence == null)
            {
                return LMCCallbackProtocolError.NullPayload;
            }
            if (fence.ListenerGeneration <= 0)
            {
                return LMCCallbackProtocolError.ListenerGenerationInvalid;
            }
            if (!IsValidIPv4(fence.ExpectedSourceIPv4))
            {
                return LMCCallbackProtocolError.CallbackSourceAddressInvalid;
            }
            if (fence.RegisteredEventMask == 0)
            {
                return LMCCallbackProtocolError.RegistrationMaskNotApproved;
            }
            if (!IsMaxDatagramValid(fence.MaxDatagramBytes))
            {
                return LMCCallbackProtocolError.MaxDatagramOutOfRange;
            }
            if (fence.BootId == 0)
            {
                return LMCCallbackProtocolError.BootIdZero;
            }
            if (fence.SessionEpoch == 0)
            {
                return LMCCallbackProtocolError.SessionEpochZero;
            }
            if (fence.Cookie == 0)
            {
                return LMCCallbackProtocolError.CookieZero;
            }
            return LMCCallbackProtocolError.None;
        }

        private static LMCCallbackProtocolError ValidateDatagramFields(
            ushort eventType,
            uint eventMaskBit,
            uint eventId,
            byte deliveryClass,
            byte flags,
            int payloadBytes,
            LMCCallbackSessionFence fence,
            LMCCallbackProtocolPolicy policy)
        {
            if (flags != 0)
            {
                return LMCCallbackProtocolError.FlagsNonZero;
            }
            if (eventMaskBit == 0
                || (eventMaskBit & (eventMaskBit - 1)) != 0)
            {
                return LMCCallbackProtocolError.EventMaskNotSingleBit;
            }
            if ((fence.RegisteredEventMask & eventMaskBit) == 0)
            {
                return LMCCallbackProtocolError.EventMaskNotSubscribed;
            }
            if (payloadBytes < 0 || payloadBytes > MaxPayloadBytes)
            {
                return LMCCallbackProtocolError.PayloadTooLarge;
            }
            if (DatagramHeaderBytes + payloadBytes
                > fence.MaxDatagramBytes)
            {
                return LMCCallbackProtocolError.DatagramTooLarge;
            }
            if (policy == null
                || !policy.ApprovesEventIdentifier(eventType, eventId))
            {
                return LMCCallbackProtocolError.EventIdentifierNotApproved;
            }
            if (!policy.ApprovesDeliveryClass(deliveryClass))
            {
                return LMCCallbackProtocolError.DeliveryClassNotApproved;
            }
            return LMCCallbackProtocolError.None;
        }

        private static bool IsMaxDatagramValid(int value)
        {
            return value >= DatagramHeaderBytes && value <= MaxDatagramBytes;
        }

        private static bool IsValidIPv4(byte[] value)
        {
            return value != null && value.Length == 4;
        }

        private static void ThrowIfInvalid(
            LMCCallbackProtocolError error,
            string parameterName)
        {
            if (error != LMCCallbackProtocolError.None)
            {
                throw new ArgumentException(
                    "UDP callback protocol validation failed: " + error + ".",
                    parameterName);
            }
        }

        private static ushort ReadUInt16(byte[] buffer, int offset)
        {
            return (ushort)(buffer[offset]
                | (buffer[offset + 1] << 8));
        }

        private static short ReadInt16(byte[] buffer, int offset)
        {
            return unchecked((short)ReadUInt16(buffer, offset));
        }

        private static uint ReadUInt32(byte[] buffer, int offset)
        {
            return (uint)(buffer[offset]
                | (buffer[offset + 1] << 8)
                | (buffer[offset + 2] << 16)
                | (buffer[offset + 3] << 24));
        }

        private static int ReadInt32(byte[] buffer, int offset)
        {
            return unchecked((int)ReadUInt32(buffer, offset));
        }

        private static void WriteUInt16(
            byte[] buffer,
            int offset,
            int value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        private static void WriteUInt32(
            byte[] buffer,
            int offset,
            uint value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        private static void WriteInt32(
            byte[] buffer,
            int offset,
            int value)
        {
            WriteUInt32(buffer, offset, unchecked((uint)value));
        }
    }

    public sealed class LMCCallbackReceiverFence
    {
        private const ulong ForwardHalfRange = 0x8000000000000000UL;

        private readonly object sync = new object();
        private readonly LMCCallbackSessionFence sessionFence;
        private readonly LMCCallbackProtocolPolicy policy;
        private bool hasAcceptedSequence;
        private ulong lastAcceptedSequence;
        private long acceptedCount;
        private long rejectedCount;
        private long duplicateCount;
        private long outOfOrderCount;

        public LMCCallbackReceiverFence(
            LMCCallbackSessionFence sessionFence,
            LMCCallbackProtocolPolicy policy)
        {
            if (sessionFence == null)
            {
                throw new ArgumentNullException("sessionFence");
            }
            if (policy == null)
            {
                throw new ArgumentNullException("policy");
            }

            var fenceError = LMCCallbackProtocol.ValidateReceiverFence(
                sessionFence,
                policy);
            if (fenceError != LMCCallbackProtocolError.None)
            {
                throw new ArgumentException(
                    "UDP callback receiver fence validation failed: "
                    + fenceError
                    + ".",
                    "sessionFence");
            }

            this.sessionFence = sessionFence;
            this.policy = policy;
        }

        public LMCCallbackFenceDecision Evaluate(
            byte[] datagram,
            long listenerGeneration,
            byte[] sourceIPv4)
        {
            lock (sync)
            {
                if (listenerGeneration != sessionFence.ListenerGeneration)
                {
                    rejectedCount++;
                    return Reject(
                        LMCCallbackFenceDecisionKind.StaleListenerGeneration,
                        LMCCallbackProtocolError.None);
                }
                if (!IsValidIPv4(sourceIPv4))
                {
                    rejectedCount++;
                    return Reject(
                        LMCCallbackFenceDecisionKind.UnexpectedSourceAddress,
                        LMCCallbackProtocolError.CallbackSourceAddressInvalid);
                }
                if (!SameIPv4(
                    sessionFence.ExpectedSourceIPv4,
                    sourceIPv4))
                {
                    rejectedCount++;
                    return Reject(
                        LMCCallbackFenceDecisionKind.UnexpectedSourceAddress,
                        LMCCallbackProtocolError.CallbackSourceAddressMismatch);
                }

                var parsed = LMCCallbackProtocol.ParseDatagram(
                    datagram,
                    sessionFence,
                    policy);
                if (!parsed.IsAccepted)
                {
                    rejectedCount++;
                    return Reject(MapProtocolError(parsed.Error), parsed.Error);
                }

                var sequence = parsed.Value.Sequence;
                if (hasAcceptedSequence)
                {
                    var delta = unchecked(sequence - lastAcceptedSequence);
                    if (delta == 0)
                    {
                        rejectedCount++;
                        duplicateCount++;
                        return Reject(
                            LMCCallbackFenceDecisionKind.DuplicateSequence,
                            LMCCallbackProtocolError.None);
                    }
                    if (delta >= ForwardHalfRange)
                    {
                        rejectedCount++;
                        outOfOrderCount++;
                        return Reject(
                            LMCCallbackFenceDecisionKind.OutOfOrderSequence,
                            LMCCallbackProtocolError.None);
                    }
                }

                hasAcceptedSequence = true;
                lastAcceptedSequence = sequence;
                acceptedCount++;
                return new LMCCallbackFenceDecision(
                    LMCCallbackFenceDecisionKind.AcceptedWakeHint,
                    LMCCallbackProtocolError.None,
                    new LMCCallbackWakeHint(parsed.Value));
            }
        }

        public LMCCallbackSessionFence SessionFence
        {
            get { return sessionFence; }
        }
        public bool HasAcceptedSequence
        {
            get
            {
                lock (sync)
                {
                    return hasAcceptedSequence;
                }
            }
        }
        public ulong LastAcceptedSequence
        {
            get
            {
                lock (sync)
                {
                    if (!hasAcceptedSequence)
                    {
                        throw new InvalidOperationException(
                            "No callback sequence has been accepted.");
                    }
                    return lastAcceptedSequence;
                }
            }
        }
        public long AcceptedCount
        {
            get
            {
                lock (sync)
                {
                    return acceptedCount;
                }
            }
        }
        public long RejectedCount
        {
            get
            {
                lock (sync)
                {
                    return rejectedCount;
                }
            }
        }
        public long DuplicateCount
        {
            get
            {
                lock (sync)
                {
                    return duplicateCount;
                }
            }
        }
        public long OutOfOrderCount
        {
            get
            {
                lock (sync)
                {
                    return outOfOrderCount;
                }
            }
        }

        private static LMCCallbackFenceDecision Reject(
            LMCCallbackFenceDecisionKind kind,
            LMCCallbackProtocolError error)
        {
            return new LMCCallbackFenceDecision(kind, error, null);
        }

        private static LMCCallbackFenceDecisionKind MapProtocolError(
            LMCCallbackProtocolError error)
        {
            switch (error)
            {
                case LMCCallbackProtocolError.StaleBootId:
                    return LMCCallbackFenceDecisionKind.StaleBootId;
                case LMCCallbackProtocolError.StaleSessionEpoch:
                    return LMCCallbackFenceDecisionKind.StaleSessionEpoch;
                case LMCCallbackProtocolError.StaleCookie:
                    return LMCCallbackFenceDecisionKind.StaleCookie;
                default:
                    return LMCCallbackFenceDecisionKind.Malformed;
            }
        }

        private static bool IsValidIPv4(byte[] value)
        {
            return value != null && value.Length == 4;
        }

        private static bool SameIPv4(byte[] expected, byte[] actual)
        {
            if (!IsValidIPv4(expected) || !IsValidIPv4(actual))
            {
                return false;
            }

            for (var index = 0; index < 4; index++)
            {
                if (expected[index] != actual[index])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
