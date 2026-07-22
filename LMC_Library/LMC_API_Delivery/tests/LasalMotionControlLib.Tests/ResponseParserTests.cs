using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class ResponseParserTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add("Response.Envelope.Valid", EnvelopeValid);
            tests.Add("Response.Envelope.Malformed", EnvelopeMalformed);
            tests.Add("Response.Acknowledgement.FourAndEightBytes", AcknowledgementFourAndEightBytes);
            tests.Add("Response.Lookup.Reference", LookupReference);
            tests.Add("Response.LegacyPrimitive.ValueAndFailure", LegacyPrimitiveValueAndFailure);
            tests.Add("Response.Typed.ReadStatus", TypedReadStatus);
            tests.Add("Response.Typed.ReadActualPosition", TypedReadActualPosition);
            tests.Add("Response.Typed.GroupReadStatus", TypedGroupReadStatus);
            tests.Add("Response.Typed.GroupReadActualPosition", TypedGroupReadActualPosition);
            tests.Add("Response.Typed.CapturedRawGolden", TypedCapturedRawGolden);
            tests.Add("Response.Typed.GroupMembers", TypedGroupMembers);
            tests.Add("Response.Typed.ShortCommandErrors", TypedShortCommandErrors);
            tests.Add("Response.Typed.MalformedPayloads", TypedMalformedPayloads);
        }

        private static void EnvelopeValid()
        {
            var raw = TestFrame.Response(
                0,
                new byte[] { 1, 2, 3 },
                0x89ABCDEFu);
            var response = LMCConnection.Parse(raw);

            AssertEx.True(response.IsFrameValid);
            AssertEx.True(response.IsSuccess);
            AssertEx.Equal((ushort)0, response.HeaderStatus);
            AssertEx.Equal((ushort)3, response.PayloadLength);
            AssertEx.Equal(0x89ABCDEFu, response.HeaderReserved);
            AssertEx.SequenceEqual(new byte[] { 1, 2, 3 }, response.Payload);
            AssertEx.SequenceEqual(raw, response.Raw);

            var mutablePayload = response.Payload;
            mutablePayload[0] = 0xFF;
            AssertEx.SequenceEqual(new byte[] { 1, 2, 3 }, response.Payload);

            var mutableRaw = response.Raw;
            mutableRaw[0] = 0xFF;
            AssertEx.SequenceEqual(raw, response.Raw);

            var headerError = LMCConnection.Parse(
                TestFrame.Response(7, new byte[0]));
            AssertEx.True(headerError.IsFrameValid);
            AssertEx.False(headerError.IsSuccess);
            AssertEx.Equal((ushort)7, headerError.Status);
        }

        private static void EnvelopeMalformed()
        {
            AssertInvalidEnvelope(null);
            AssertInvalidEnvelope(new byte[0]);
            AssertInvalidEnvelope(new byte[7]);

            var shortPayload = TestFrame.Response(0, new byte[] { 1, 2, 3, 4 });
            Array.Resize(ref shortPayload, shortPayload.Length - 1);
            AssertInvalidEnvelope(shortPayload);

            var trailingByte = TestFrame.Response(0, new byte[] { 1, 2, 3, 4 });
            Array.Resize(ref trailingByte, trailingByte.Length + 1);
            trailingByte[trailingByte.Length - 1] = 0xAA;
            AssertInvalidEnvelope(trailingByte);

            var impossiblePayload = new byte[8];
            TestFrame.WriteUInt16(impossiblePayload, 2, ushort.MaxValue);
            AssertInvalidEnvelope(impossiblePayload);
        }

        private static void AcknowledgementFourAndEightBytes()
        {
            var shortSuccess = LMCConnection.ParseAcknowledgement(
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
            AssertEx.True(shortSuccess.IsFrameValid);
            AssertEx.True(shortSuccess.HasCommandResult);
            AssertEx.True(shortSuccess.IsSuccess);
            AssertEx.Equal((ushort)0, shortSuccess.CommandStatus);
            AssertEx.Equal((short)0, shortSuccess.ErrorId);

            var shortError = LMCConnection.ParseAcknowledgement(
                TestFrame.Response(0, TestFrame.Hex("10 00 F8 FF")));
            AssertEx.True(shortError.HasCommandResult);
            AssertEx.False(shortError.IsSuccess);
            AssertEx.Equal((ushort)0x0010, shortError.CommandStatus);
            AssertEx.Equal((short)-8, shortError.ErrorId);

            var longError = LMCConnection.ParseAcknowledgement(
                TestFrame.Response(
                    0,
                    TestFrame.Hex("44 33 22 11 10 00 F8 FF")));
            AssertEx.True(longError.HasCommandResult);
            AssertEx.False(longError.IsSuccess);
            AssertEx.Equal((ushort)0x0010, longError.CommandStatus);
            AssertEx.Equal((short)-8, longError.ErrorId);

            var structured = LMCConnection.ParseAcknowledgement(
                TestFrame.Response(0, new byte[12]));
            AssertEx.False(
                structured.HasCommandResult,
                "A structured 12-byte payload must not be parsed as an ACK.");

            AssertEx.True(
                LMCConnection.ParseCommandAcknowledgement(
                    TestFrame.Response(0, TestFrame.Hex("00 00 00 00")),
                    "test").IsSuccess);
            AssertEx.True(
                LMCConnection.ParseCommandAcknowledgement(
                    TestFrame.Response(
                        0,
                        TestFrame.Hex("34 12 00 00 00 00 00 00")),
                    "test").IsSuccess);
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseCommandAcknowledgement(
                    TestFrame.Response(0, new byte[0]),
                    "test"));
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseCommandAcknowledgement(
                    TestFrame.Response(0, new byte[12]),
                    "test"));

            AssertEx.True(
                LMCConnection.ParseShortAcknowledgement(
                    TestFrame.Response(0, TestFrame.Hex("00 00 00 00")),
                    "captured-short").IsSuccess);
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseShortAcknowledgement(
                    TestFrame.Response(0, new byte[8]),
                    "captured-short"));
        }

        private static void LookupReference()
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, 0xBEEF);

            LMC_Response response;
            ushort reference;
            var parsed = LMCConnection.TryParseLookupReference(
                TestFrame.Response(0, payload),
                out response,
                out reference);

            AssertEx.True(parsed);
            AssertEx.True(response.IsSuccess);
            AssertEx.Equal((ushort)0xBEEF, reference);

            parsed = LMCConnection.TryParseLookupReference(
                TestFrame.Response(2, payload),
                out response,
                out reference);
            AssertEx.False(parsed);
            AssertEx.Equal((ushort)0, reference);

            parsed = LMCConnection.TryParseLookupReference(
                TestFrame.Response(0, new byte[5]),
                out response,
                out reference);
            AssertEx.False(parsed);

            parsed = LMCConnection.TryParseLookupReference(
                TestFrame.Response(0, new byte[7]),
                out response,
                out reference);
            AssertEx.False(parsed);

            parsed = LMCConnection.TryParseLookupReference(
                TestFrame.Response(0, new byte[6]),
                out response,
                out reference);
            AssertEx.False(parsed);

            var errorPayload = new byte[4];
            TestFrame.WriteUInt16(errorPayload, 0, 1);
            TestFrame.WriteInt16(errorPayload, 2, -2);
            var lookupError = LMCConnection.CreateLookupFailureException(
                "Axis",
                "_LMCAxis1",
                TestFrame.Response(1, errorPayload));

            AssertEx.Contains("Axis lookup failed for '_LMCAxis1'", lookupError.Message);
            AssertEx.Contains("FrameValid=True", lookupError.Message);
            AssertEx.Contains("HeaderStatus=1", lookupError.Message);
            AssertEx.Contains("PayloadLength=4", lookupError.Message);
            AssertEx.Contains("CommandStatus=1", lookupError.Message);
            AssertEx.Contains("ErrorId=-2", lookupError.Message);
            AssertEx.Contains("object registry entry is not ready", lookupError.Message);
            AssertEx.Contains(
                "Raw=01 00 04 00 00 00 00 00 01 00 FE FF",
                lookupError.Message);

            var zeroReferenceError = LMCConnection.CreateLookupFailureException(
                "Axis",
                "_LMCAxis1",
                TestFrame.Response(0, new byte[6]));
            AssertEx.Contains("Reference=0", zeroReferenceError.Message);
            AssertEx.Contains("zero descriptor", zeroReferenceError.Message);

            var truncatedLookup = new byte[8];
            TestFrame.WriteUInt16(truncatedLookup, 2, 6);
            var truncatedError = LMCConnection.CreateLookupFailureException(
                "Axis",
                "_LMCAxis1",
                truncatedLookup);
            AssertEx.Contains("FrameValid=False", truncatedError.Message);
            AssertEx.Contains("PayloadLength=6", truncatedError.Message);
            AssertEx.Contains("ParsedPayloadLength=0", truncatedError.Message);
            AssertEx.Contains("RawLength=8", truncatedError.Message);

            var malformedAckPayload = new byte[8];
            TestFrame.WriteUInt16(malformedAckPayload, 4, 1);
            TestFrame.WriteInt16(malformedAckPayload, 6, -2);
            var malformedAckError = LMCConnection.CreateLookupFailureException(
                "Axis",
                "_LMCAxis1",
                TestFrame.Response(1, malformedAckPayload));
            AssertEx.False(
                malformedAckError.Message.IndexOf(
                    "object registry entry is not ready",
                    StringComparison.Ordinal) >= 0);

            var longRaw = new byte[256];
            for (var index = 0; index < longRaw.Length; index++)
            {
                longRaw[index] = 0xAA;
            }

            var longRawError = LMCConnection.CreateLookupFailureException(
                "Axis",
                "_LMCAxis1",
                longRaw);
            AssertEx.Contains("RawLength=256", longRawError.Message);
            AssertEx.Contains(" ...", longRawError.Message);
            AssertEx.True(longRawError.Message.Length < 1024);
        }

        private static void LegacyPrimitiveValueAndFailure()
        {
            var payload = new byte[4];
            TestFrame.WriteUInt32(payload, 0, 0xFEDCBA98u);

            LMC_Response response;
            var unsignedValue = LMCConnection.ParseUInt32Value(
                TestFrame.Response(0, payload),
                out response);
            AssertEx.Equal(0xFEDCBA98u, unsignedValue);
            AssertEx.True(response.IsSuccess);

            TestFrame.WriteInt32(payload, 0, -123456789);
            var signedValue = LMCConnection.ParseInt32Value(
                TestFrame.Response(0, payload),
                out response);
            AssertEx.Equal(-123456789, signedValue);
            AssertEx.True(response.IsSuccess);

            signedValue = LMCConnection.ParseInt32Value(
                TestFrame.Response(9, payload),
                out response);
            AssertEx.Equal(0, signedValue);
            AssertEx.False(response.IsSuccess);

            signedValue = LMCConnection.ParseInt32Value(
                new byte[7],
                out response);
            AssertEx.Equal(0, signedValue);
            AssertEx.False(response.IsFrameValid);
        }

        private static void TypedReadStatus()
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, 0x40000280u);
            TestFrame.WriteUInt16(payload, 4, 0x4000);
            TestFrame.WriteInt16(payload, 6, 0);
            TestFrame.WriteUInt16(payload, 8, 0);
            TestFrame.WriteUInt16(payload, 10, 0xABCD);

            var result = LMCConnection.ParseReadStatusResult(
                TestFrame.Response(0, payload));

            AssertEx.Equal(0x40000280u, result.State);
            AssertEx.Equal((ushort)0x4000, result.FunctionStatus);
            AssertEx.Equal((short)0, result.ErrorId);
            AssertEx.Equal((ushort)0, result.AxisErrorId);
            AssertEx.Equal((ushort)0xABCD, result.StatusWord);
            AssertEx.False(result.HasCommandError);
            AssertEx.True(result.IsSuccess);
            AssertEx.True(result.Response.IsFrameValid);

            // The canonical LASAL server returns native _LMCAXIS_STATUS:
            // PowerOn bit 0, IsReferenced bit 1, and Standstill bit 25.
            TestFrame.WriteUInt32(payload, 0, 0x02000003u);
            TestFrame.WriteUInt16(payload, 4, 0);
            result = LMCConnection.ParseReadStatusResult(
                TestFrame.Response(0, payload));
            AssertEx.Equal(0x02000003u, result.State);
            AssertEx.True(result.IsPowerOn);
            AssertEx.True(result.IsReferenced);
            AssertEx.True(result.IsStandstill);
            AssertEx.True(result.IsSuccess);

            TestFrame.WriteUInt32(payload, 0, 0x02000001u);
            result = LMCConnection.ParseReadStatusResult(
                TestFrame.Response(0, payload));
            AssertEx.False(result.IsReferenced);

            TestFrame.WriteUInt16(payload, 4, 0x0010);
            TestFrame.WriteInt16(payload, 6, -8);
            result = LMCConnection.ParseReadStatusResult(
                TestFrame.Response(0, payload));
            AssertEx.True(result.HasCommandError);
            AssertEx.False(result.IsSuccess);
            AssertEx.Equal((short)-8, result.ErrorId);

            TestFrame.WriteUInt16(payload, 4, 0);
            TestFrame.WriteInt16(payload, 6, 0);
            TestFrame.WriteUInt16(payload, 8, 7);
            result = LMCConnection.ParseReadStatusResult(
                TestFrame.Response(0, payload));
            AssertEx.False(result.IsSuccess);
            AssertEx.Equal((ushort)7, result.AxisErrorId);
        }

        private static void TypedReadActualPosition()
        {
            var payload = new byte[8];
            TestFrame.WriteInt32(payload, 0, -123456789);
            TestFrame.WriteUInt16(payload, 4, 0x4000);
            TestFrame.WriteInt16(payload, 6, 0);

            var result = LMCConnection.ParseReadActualPositionResult(
                TestFrame.Response(0, payload));

            AssertEx.Equal(-123456789, result.PositionRaw);
            AssertEx.Equal((ushort)0x4000, result.FunctionStatus);
            AssertEx.Equal((short)0, result.ErrorId);
            AssertEx.False(result.HasCommandError);
            AssertEx.True(result.IsSuccess);

            TestFrame.WriteUInt16(payload, 4, 0x0010);
            TestFrame.WriteInt16(payload, 6, -22);
            result = LMCConnection.ParseReadActualPositionResult(
                TestFrame.Response(0, payload));
            AssertEx.True(result.HasCommandError);
            AssertEx.False(result.IsSuccess);
            AssertEx.Equal((short)-22, result.ErrorId);
        }

        private static void TypedGroupReadStatus()
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, 0x00000080u);
            TestFrame.WriteUInt16(payload, 4, 0x4000);
            TestFrame.WriteInt16(payload, 6, 0);
            TestFrame.WriteUInt16(payload, 8, 0);
            TestFrame.WriteUInt16(payload, 10, 0xBEEF);

            var result = LMCConnection.ParseGroupReadStatusResult(
                TestFrame.Response(0, payload));

            AssertEx.Equal(0x00000080u, result.State);
            AssertEx.Equal((ushort)0x4000, result.FunctionStatus);
            AssertEx.Equal((short)0, result.ErrorId);
            AssertEx.Equal((ushort)0, result.GroupErrorId);
            AssertEx.False(result.HasCommandError);
            AssertEx.True(result.IsSuccess);
            AssertEx.False(result.IsPowerOn);
            AssertEx.False(result.IsStandby);
            AssertEx.False(result.IsEnabled);
            AssertEx.False(result.IsDisabled);

            TestFrame.WriteUInt32(payload, 0, 0x00040000u);
            result = LMCConnection.ParseGroupReadStatusResult(
                TestFrame.Response(0, payload));
            AssertEx.True(result.IsPowerOn);
            AssertEx.False(result.IsStandby);
            AssertEx.False(result.IsEnabled);
            AssertEx.False(result.IsDisabled);

            TestFrame.WriteUInt32(payload, 0, 0x00020000u);
            result = LMCConnection.ParseGroupReadStatusResult(
                TestFrame.Response(0, payload));
            AssertEx.False(result.IsPowerOn);
            AssertEx.True(result.IsStandby);
            AssertEx.True(result.IsEnabled);
            AssertEx.False(result.IsDisabled);

            TestFrame.WriteUInt32(payload, 0, 0x00010000u);
            result = LMCConnection.ParseGroupReadStatusResult(
                TestFrame.Response(0, payload));
            AssertEx.False(result.IsPowerOn);
            AssertEx.False(result.IsStandby);
            AssertEx.False(result.IsEnabled);
            AssertEx.True(result.IsDisabled);

            TestFrame.WriteUInt16(payload, 8, 99);
            result = LMCConnection.ParseGroupReadStatusResult(
                TestFrame.Response(0, payload));
            AssertEx.False(result.IsSuccess);
            AssertEx.Equal((ushort)99, result.GroupErrorId);
        }

        private static void TypedGroupReadActualPosition()
        {
            var payload = new byte[68];
            var expected = new[]
            {
                1,
                -2,
                0x01020304,
                int.MinValue,
                int.MaxValue,
                7,
                -8,
                9,
                -10,
                0,
                0,
                0,
                0,
                0,
                0,
                0
            };

            for (var index = 0; index < expected.Length; index++)
            {
                TestFrame.WriteInt32(payload, index * 4, expected[index]);
            }

            TestFrame.WriteUInt16(payload, 64, 0x4000);
            TestFrame.WriteInt16(payload, 66, 0);

            var result = LMCConnection.ParseGroupReadActualPositionResult(
                TestFrame.Response(0, payload),
                LMC_COORD_SYSTEM.Acs);

            AssertEx.Equal(LMC_COORD_SYSTEM.Acs, result.CoordinateSystem);
            AssertEx.Equal((ushort)0x4000, result.FunctionStatus);
            AssertEx.Equal((short)0, result.ErrorId);
            AssertEx.True(result.IsSuccess);
            AssertIntSequence(expected, result.PositionsRaw);

            var clone = result.PositionsRaw;
            clone[0] = 999;
            AssertEx.Equal(1, result.PositionsRaw[0]);

            // Raw/older clients can still reach the PLC with MCS. Preserve its
            // explicit -7 command-error context even though the public builder
            // now rejects MCS before sending a request.
            var shortError = LMCConnection.ParseGroupReadActualPositionResult(
                TestFrame.Response(0, TestFrame.Hex("10 00 F9 FF")),
                LMC_COORD_SYSTEM.Mcs);
            AssertEx.False(shortError.IsSuccess);
            AssertEx.True(shortError.HasCommandError);
            AssertEx.Equal(LMC_COORD_SYSTEM.Mcs, shortError.CoordinateSystem);
            AssertEx.Equal((short)-7, shortError.ErrorId);
            AssertEx.Equal(16, shortError.PositionsRaw.Length);

            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseGroupReadActualPositionResult(
                    TestFrame.Response(0, new byte[136]),
                    LMC_COORD_SYSTEM.Acs),
                "The LASAL-DINT group parser must reject the legacy LREAL response.");
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseGroupReadActualPositionResult(
                    TestFrame.Response(0, new byte[67]),
                    LMC_COORD_SYSTEM.Acs));
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseGroupReadActualPositionResult(
                    TestFrame.Response(0, new byte[69]),
                    LMC_COORD_SYSTEM.Acs));
        }

        private static void TypedCapturedRawGolden()
        {
            var readStatus = LMCConnection.ParseReadStatusResult(
                TestFrame.Hex(
                    "00 00 0C 00 00 00 00 00 "
                    + "80 00 00 40 00 00 00 00 00 00 B7 12"));

            AssertEx.Equal(0x40000080u, readStatus.State);
            AssertEx.Equal((ushort)0, readStatus.FunctionStatus);
            AssertEx.Equal((short)0, readStatus.ErrorId);
            AssertEx.Equal((ushort)0, readStatus.AxisErrorId);
            AssertEx.Equal((ushort)0x12B7, readStatus.StatusWord);
            AssertEx.True(readStatus.IsSuccess);
            AssertEx.True(readStatus.Response.IsSuccess);

            var groupStatus = LMCConnection.ParseGroupReadStatusResult(
                TestFrame.Hex(
                    "00 00 0C 00 00 00 00 00 "
                    + "00 00 02 40 00 00 00 00 00 00 54 77"));

            AssertEx.Equal(0x40020000u, groupStatus.State);
            AssertEx.Equal((ushort)0, groupStatus.FunctionStatus);
            AssertEx.Equal((short)0, groupStatus.ErrorId);
            AssertEx.Equal((ushort)0, groupStatus.GroupErrorId);
            AssertEx.True(groupStatus.IsSuccess);
            AssertEx.True(groupStatus.Response.IsSuccess);
        }

        private static void TypedGroupMembers()
        {
            var payload = GroupMembersPayload(4, 0x4000, 0);
            var result = LMCConnection.ParseGroupMembersInfoResult(
                TestFrame.Response(0, payload));

            AssertEx.Equal((byte)4, result.AxisCount);
            AssertEx.Equal((ushort)0x4000, result.FunctionStatus);
            AssertEx.Equal((short)0, result.ErrorId);
            AssertEx.False(result.HasCommandError);
            AssertEx.True(result.IsSuccess);
            AssertEx.Equal(16, result.AxisReferences.Length);
            AssertEx.Equal(16, result.DeviceIds.Length);
            AssertEx.Equal(16, result.AxisNames.Length);
            AssertEx.Equal(4, result.Members.Length);
            AssertEx.Equal((ushort)0x1000, result.AxisReferences[0]);
            AssertEx.Equal((ushort)2, result.AxisReferences[2]);
            AssertEx.Equal((ushort)3, result.AxisReferences[3]);
            AssertEx.Equal((ushort)0x2001, result.DeviceIds[1]);
            AssertEx.Equal("a01", result.AxisNames[0]);
            AssertEx.Equal("a04", result.AxisNames[3]);
            AssertEx.Equal(80, result.AxisNames[15].Length);
            AssertEx.Equal(2, result.Members[2].Index);
            AssertEx.Equal((ushort)2, result.Members[2].AxisReference);
            AssertEx.Equal((ushort)0x2002, result.Members[2].DeviceId);
            AssertEx.Equal("a03", result.Members[2].AxisName);

            var clonedReferences = result.AxisReferences;
            clonedReferences[0] = 0;
            AssertEx.Equal((ushort)0x1000, result.AxisReferences[0]);

            var clonedNames = result.AxisNames;
            clonedNames[0] = "changed";
            AssertEx.Equal("a01", result.AxisNames[0]);

            payload = GroupMembersPayload(9, 0, 0);
            result = LMCConnection.ParseGroupMembersInfoResult(
                TestFrame.Response(0, payload));
            AssertEx.True(result.IsSuccess);
            AssertEx.Equal((byte)9, result.AxisCount);
            AssertEx.Equal(9, result.Members.Length);
            AssertEx.Equal((ushort)0x1008, result.Members[8].AxisReference);
            AssertEx.Equal((ushort)0x2008, result.Members[8].DeviceId);
            AssertEx.Equal("a09", result.Members[8].AxisName);

            payload = GroupMembersPayload(2, 0x0010, -8);
            result = LMCConnection.ParseGroupMembersInfoResult(
                TestFrame.Response(0, payload));
            AssertEx.True(result.HasCommandError);
            AssertEx.False(result.IsSuccess);
            AssertEx.Equal((short)-8, result.ErrorId);
        }

        private static void TypedShortCommandErrors()
        {
            var shortError = TestFrame.Response(
                1,
                TestFrame.Hex("10 00 FD FF"));

            var axisStatus = LMCConnection.ParseReadStatusResult(shortError);
            AssertEx.False(axisStatus.IsSuccess);
            AssertEx.Equal((short)-3, axisStatus.ErrorId);
            AssertEx.Equal((ushort)1, axisStatus.Response.HeaderStatus);

            var position = LMCConnection.ParseReadActualPositionResult(shortError);
            AssertEx.False(position.IsSuccess);
            AssertEx.Equal((short)-3, position.ErrorId);

            var groupStatus = LMCConnection.ParseGroupReadStatusResult(shortError);
            AssertEx.False(groupStatus.IsSuccess);
            AssertEx.Equal((short)-3, groupStatus.ErrorId);

            var members = LMCConnection.ParseGroupMembersInfoResult(shortError);
            AssertEx.False(members.IsSuccess);
            AssertEx.Equal((short)-3, members.ErrorId);
            AssertEx.Equal((byte)0, members.AxisCount);
        }

        private static void TypedMalformedPayloads()
        {
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseReadStatusResult(
                    TestFrame.Response(0, new byte[11])));
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseReadStatusResult(
                    TestFrame.Response(0, new byte[13])));
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseReadActualPositionResult(
                    TestFrame.Response(0, new byte[16])),
                "The LASAL-DINT parser must reject the legacy LREAL response.");
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseGroupReadStatusResult(
                    TestFrame.Response(0, new byte[11])));
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseGroupMembersInfoResult(
                    TestFrame.Response(0, new byte[1349])));
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseGroupMembersInfoResult(
                    TestFrame.Response(0, new byte[1351])));
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseGroupMembersInfoResult(
                    TestFrame.Response(0, GroupMembersPayload(17, 0, 0))));

            var truncated = TestFrame.Response(
                0,
                GroupMembersPayload(4, 0, 0));
            Array.Resize(ref truncated, truncated.Length - 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseGroupMembersInfoResult(truncated));

            var trailing = TestFrame.Response(
                0,
                GroupMembersPayload(4, 0, 0));
            Array.Resize(ref trailing, trailing.Length + 1);
            AssertEx.Throws<InvalidDataException>(
                () => LMCConnection.ParseGroupMembersInfoResult(trailing));
        }

        private static void AssertInvalidEnvelope(byte[] raw)
        {
            var response = LMCConnection.Parse(raw);
            AssertEx.False(response.IsFrameValid);
            AssertEx.False(response.IsSuccess);
        }

        private static byte[] GroupMembersPayload(
            byte count,
            ushort functionStatus,
            short errorId)
        {
            var payload = new byte[1350];

            for (var index = 0; index < 16; index++)
            {
                var reference = (ushort)(0x1000 + index);
                if (index == 2 || index == 3)
                {
                    reference = (ushort)index;
                }

                TestFrame.WriteUInt16(payload, index * 2, reference);
                TestFrame.WriteUInt16(payload, 32 + index * 2, (ushort)(0x2000 + index));
            }

            TestFrame.WriteUInt16(payload, 64, functionStatus);
            TestFrame.WriteInt16(payload, 66, errorId);

            WriteFixedAscii(payload, 68 + 0 * 80, "a01");
            WriteFixedAscii(payload, 68 + 1 * 80, "a02");
            WriteFixedAscii(payload, 68 + 2 * 80, "a03");
            WriteFixedAscii(payload, 68 + 3 * 80, "a04");
            WriteFixedAscii(payload, 68 + 4 * 80, "a05");
            WriteFixedAscii(payload, 68 + 5 * 80, "a06");
            WriteFixedAscii(payload, 68 + 6 * 80, "a07");
            WriteFixedAscii(payload, 68 + 7 * 80, "a08");
            WriteFixedAscii(payload, 68 + 8 * 80, "a09");

            for (var index = 0; index < 80; index++)
            {
                payload[68 + 15 * 80 + index] = (byte)'Z';
            }

            payload[1348] = count;
            payload[1349] = 0xA5;
            return payload;
        }

        private static void WriteFixedAscii(byte[] buffer, int offset, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
        }

        private static void AssertIntSequence(int[] expected, int[] actual)
        {
            AssertEx.NotNull(actual);
            AssertEx.Equal(expected.Length, actual.Length);

            for (var index = 0; index < expected.Length; index++)
            {
                AssertEx.Equal(expected[index], actual[index]);
            }
        }
    }
}
