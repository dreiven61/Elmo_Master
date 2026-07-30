using System;
using System.Collections.Generic;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class RecorderHeaderSemanticCanonicalizerTests
    {
        private const string ExpectedSha256 =
            "BCFED04939E7EFFEA6428376EF74C1C1"
            + "F3A8EAF235F0BE749B1498D657B49977";

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.RecorderHeader.CanonicalLayoutLittleEndian",
                CanonicalLayoutLittleEndian);
            tests.Add(
                "Qualification.RecorderHeader.TransportResponseExcluded",
                TransportResponseExcluded);
            tests.Add(
                "Qualification.RecorderHeader.SemanticMutationChangesEvidence",
                SemanticMutationChangesEvidence);
            tests.Add(
                "Qualification.RecorderHeader.EvidenceOwnsCanonicalBytes",
                EvidenceOwnsCanonicalBytes);
            tests.Add(
                "Qualification.RecorderHeader.NullRejected",
                NullRejected);
        }

        private static void CanonicalLayoutLittleEndian()
        {
            var evidence = RecorderHeaderSemanticCanonicalizer
                .CreateEvidence(CreateHeader(CreateResponse(1, 0xA1)));
            var expected = TestFrame.Hex(
                "4C 4D 43 52 48 44 52 31 "
                + "04 03 02 01 14 13 12 11 24 23 22 21 "
                + "34 33 32 31 44 43 42 41 54 53 52 51 "
                + "02 00 02 0F 00 "
                + "64 63 62 61 74 73 72 71 "
                + "02 00 02 01 84 83 82 81 01 01 "
                + "94 93 92 91 A4 A3 A2 A1 B4 B3 B2 B1 "
                + "C4 C3 C2 C1 D4 D3 D2 D1 E4 E3 E2 E1 "
                + "F4 F3 F2 F1 01 01 01 01 02 02 02 02 "
                + "03 03 03 03 DD DC DB DA ED EC EB EA "
                + "04 01 10 00 04 02 10 00");

            AssertEx.Equal("LMCRHDR1", evidence.FormatId);
            AssertEx.Equal(
                RecorderHeaderSemanticCanonicalizer.FixedByteCount + 8,
                evidence.CanonicalByteCount);
            AssertEx.SequenceEqual(
                expected,
                evidence.CopyCanonicalBytes());
            AssertEx.Equal(ExpectedSha256, evidence.Sha256);
        }

        private static void TransportResponseExcluded()
        {
            var firstHeader = CreateHeader(CreateResponse(1, 0xA1));
            var secondHeader = CreateHeader(
                CreateResponse(0xF0E0D0C0u, 0xB2));
            var first = RecorderHeaderSemanticCanonicalizer
                .CreateEvidence(firstHeader);
            var second = RecorderHeaderSemanticCanonicalizer
                .CreateEvidence(secondHeader);

            AssertEx.SequenceEqual(
                first.CopyCanonicalBytes(),
                second.CopyCanonicalBytes());
            AssertEx.Equal(first.Sha256, second.Sha256);

            var capture = new RecorderDoubleBankCaptureLease(
                new object(),
                firstHeader.DiagnosticsBootId,
                firstHeader.ConfigId,
                firstHeader.ConfigRevision,
                firstHeader.RecordId,
                firstHeader.BufferId,
                new object(),
                new object(),
                false);
            var firstCaptureEvidence =
                new RecorderDoubleBankCaptureEvidence(
                    capture,
                    firstHeader,
                    new byte[] { 1, 2, 3, 4 });
            var secondCaptureEvidence =
                new RecorderDoubleBankCaptureEvidence(
                    capture,
                    secondHeader,
                    new byte[] { 1, 2, 3, 4 });

            AssertEx.Equal(
                ExpectedSha256,
                firstCaptureEvidence.HeaderSha256);
            AssertEx.Equal(
                firstCaptureEvidence.HeaderSha256,
                secondCaptureEvidence.HeaderSha256);
        }

        private static void SemanticMutationChangesEvidence()
        {
            var baseline = RecorderHeaderSemanticCanonicalizer
                .CreateEvidence(CreateHeader(null));
            var changedOverflow = RecorderHeaderSemanticCanonicalizer
                .CreateEvidence(
                    CreateHeader(null, 0x01020304u));
            var changedSignalOrder = RecorderHeaderSemanticCanonicalizer
                .CreateEvidence(
                    CreateHeader(
                        null,
                        0xEAEBECEDu,
                        new uint[] { 0x00100204u, 0x00100104u }));

            AssertEx.False(
                string.Equals(
                    baseline.Sha256,
                    changedOverflow.Sha256,
                    StringComparison.Ordinal));
            AssertEx.False(
                string.Equals(
                    baseline.Sha256,
                    changedSignalOrder.Sha256,
                    StringComparison.Ordinal));
        }

        private static void EvidenceOwnsCanonicalBytes()
        {
            var evidence = RecorderHeaderSemanticCanonicalizer
                .CreateEvidence(CreateHeader(null));
            var firstCopy = evidence.CopyCanonicalBytes();
            firstCopy[0] ^= 0xFF;

            var secondCopy = evidence.CopyCanonicalBytes();
            AssertEx.Equal((byte)0x4C, secondCopy[0]);
            AssertEx.Equal(ExpectedSha256, evidence.Sha256);
        }

        private static void NullRejected()
        {
            AssertEx.Throws<ArgumentNullException>(
                () => RecorderHeaderSemanticCanonicalizer.Serialize(null));
            AssertEx.Throws<ArgumentNullException>(
                () => RecorderHeaderSemanticCanonicalizer
                    .CreateEvidence(null));
        }

        private static LMCDiagnosticsResponse CreateResponse(
            uint requestId,
            byte rawMarker)
        {
            var transport = new LMC_Response
            {
                Raw = new byte[] { rawMarker, 0xEE, 0xFF },
                Payload = new byte[] { rawMarker },
                HeaderReserved = requestId ^ 0xFFFFFFFFu,
                IsFrameValid = true
            };

            return new LMCDiagnosticsResponse(
                transport,
                1,
                requestId == 1
                    ? LMCDiagnosticsResponseFlags.None
                    : LMCDiagnosticsResponseFlags.LastChunk,
                0,
                0,
                requestId,
                requestId == 1 ? 0u : 0xDEADBEEFu);
        }

        private static LMCRecorderHeader CreateHeader(
            LMCDiagnosticsResponse response,
            uint overflowCount = 0xEAEBECEDu,
            IList<uint> signalIds = null)
        {
            return new LMCRecorderHeader(
                response,
                0x01020304u,
                0x11121314u,
                0x21222324u,
                0x31323334u,
                0x41424344u,
                0x51525354u,
                LMCCapturePhase.PreOutput,
                LMCRecorderStopReason.UserStop,
                LMCRecorderHeaderFlags.CaptureComplete
                    | LMCRecorderHeaderFlags.TriggerPresent
                    | LMCRecorderHeaderFlags.UserStopped
                    | LMCRecorderHeaderFlags.DataCrcPresent,
                0x61626364u,
                0x71727374u,
                0x0102,
                0x81828384u,
                LMCRecorderDataEncoding.SampleMajorRaw32LittleEndian,
                LMCRecorderDataCrcPolicy.Crc32IsoHdlc,
                0x91929394u,
                0xA1A2A3A4u,
                0xB1B2B3B4u,
                0xC1C2C3C4u,
                0xD1D2D3D4u,
                0xE1E2E3E4u,
                0xF1F2F3F4u,
                0x01010101u,
                0x02020202u,
                0x03030303u,
                0xDADBDCDDu,
                overflowCount,
                signalIds ?? new uint[]
                {
                    0x00100104u,
                    0x00100204u
                });
        }
    }
}
