using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LasalMotionControlApiExample;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static class WpfTopologyIoLiveEvidenceTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.TopologyIoLiveEvidence.CapacityDropAndSequence",
                CapacityDropAndSequence);
            tests.Add(
                "Wpf.TopologyIoLiveEvidence.HealthAndDiFieldFidelity",
                HealthAndDiFieldFidelity);
            tests.Add(
                "Wpf.TopologyIoLiveEvidence.FailureHasNoPriorSample",
                FailureHasNoPriorSample);
            tests.Add(
                "Wpf.TopologyIoLiveEvidence.TextCsvEscapeAndNoBom",
                TextCsvEscapeAndNoBom);
        }

        private static void CapacityDropAndSequence()
        {
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new TopologyIoLiveEvidenceJournal(
                    TopologyIoLiveEvidenceJournal.MaximumCapacity + 1));

            var journal = new TopologyIoLiveEvidenceJournal(3);
            for (var index = 0; index < 5; index++)
            {
                var appended = journal.Append(
                    TopologyIoLiveEvidenceRecord.CreateFailure(
                        CreateHealthContext(
                            TopologyIoLiveEvidenceOrigin.Auto,
                            "CREVIS-" + index),
                        new DateTime(
                            2026,
                            7,
                            29,
                            1,
                            0,
                            index,
                            DateTimeKind.Utc),
                        "TimeoutException",
                        "request " + index));
                AssertEx.Equal((ulong)(index + 1), appended.JournalSequence);
            }

            var snapshot = journal.CaptureSnapshot();
            AssertEx.Equal(3, snapshot.Capacity);
            AssertEx.Equal((ulong)2, snapshot.DroppedOldestCount);
            AssertEx.Equal((ulong)5, snapshot.LastSequence);
            AssertEx.Equal(3, snapshot.Records.Count);
            AssertEx.Equal((ulong)3, snapshot.Records[0].JournalSequence);
            AssertEx.Equal((ulong)4, snapshot.Records[1].JournalSequence);
            AssertEx.Equal((ulong)5, snapshot.Records[2].JournalSequence);

            var mutableView = snapshot.Records
                as IList<TopologyIoLiveEvidenceRecord>;
            AssertEx.True(
                mutableView != null && mutableView.IsReadOnly,
                "The journal snapshot exposed a mutable record collection.");

            journal.Append(
                TopologyIoLiveEvidenceRecord.CreateFailure(
                    CreateHealthContext(
                        TopologyIoLiveEvidenceOrigin.Manual,
                        "CREVIS-later"),
                    new DateTime(
                        2026,
                        7,
                        29,
                        1,
                        1,
                        0,
                        DateTimeKind.Utc),
                    "IOException",
                    "later"));
            AssertEx.Equal(
                3,
                snapshot.Records.Count,
                "A captured snapshot changed after the journal was appended.");
            AssertEx.Equal(
                (ulong)5,
                snapshot.LastSequence,
                "A captured snapshot's last sequence changed after capture.");
        }

        private static void HealthAndDiFieldFidelity()
        {
            var journal = new TopologyIoLiveEvidenceJournal(4);
            var healthContext = new TopologyIoLiveEvidenceContext(
                TopologyIoLiveEvidenceOrigin.Auto,
                TopologyIoLiveEvidenceKind.Health,
                "127.0.0.1:4000",
                19,
                0x10203040u,
                0xE245539Au,
                0x0003C000u,
                "automatic connect load",
                0x99887766u,
                0x11223344u,
                "CREVIS Coupler",
                1,
                0,
                null,
                null,
                "NodeHealth(0x11223344)",
                71,
                null,
                null);
            var health = journal.Append(
                TopologyIoLiveEvidenceRecord.CreateHealthSuccess(
                    healthContext,
                    new DateTime(
                        2026,
                        7,
                        29,
                        2,
                        3,
                        4,
                        567,
                        DateTimeKind.Utc),
                    901,
                    902,
                    903,
                    "Configured|Detected|DataValid|Ds402DataPresent",
                    true,
                    true,
                    0x08,
                    0x0011,
                    0x01020304u,
                    0x05060708u,
                    0x1234u,
                    0xA0B0C0D0u,
                    897,
                    898));

            AssertEx.Equal(TopologyIoLiveEvidenceOrigin.Auto, health.Context.Origin);
            AssertEx.Equal(TopologyIoLiveEvidenceKind.Health, health.Context.Kind);
            AssertEx.Equal("127.0.0.1:4000", health.Context.Endpoint);
            AssertEx.Equal((long)19, health.Context.SessionGeneration);
            AssertEx.Equal(0x10203040u, health.Context.DiagnosticsBootId);
            AssertEx.Equal(0xE245539Au, health.Context.MapRevision);
            AssertEx.Equal(0x0003C000u, health.Context.CapabilityBits);
            AssertEx.Equal(0x99887766u, health.Context.TopologyRevision);
            AssertEx.Equal(0x11223344u, health.Context.NodeId);
            AssertEx.Equal((uint?)71, health.Context.RequestId);
            AssertEx.Equal(TopologyIoLiveEvidenceOutcome.Success, health.Outcome);
            AssertEx.Equal((uint?)901, health.CycleCounter);
            AssertEx.Equal((uint?)902, health.PlcSnapshotSequence);
            AssertEx.Equal((ulong?)903, health.PlcTimestampMicroseconds);
            AssertEx.Equal((bool?)true, health.DataValid);
            AssertEx.Equal((bool?)true, health.Online);
            AssertEx.Equal((byte?)0x08, health.EtherCATState);
            AssertEx.Equal((ushort?)0x0011, health.ALStatusCode);
            AssertEx.Equal((uint?)0x1234, health.DS402StatusWord);
            AssertEx.Equal((uint?)0xA0B0C0D0, health.AxisError);
            AssertEx.False(health.Value.HasValue);
            AssertEx.False(health.ValidMask.HasValue);

            var diContext = new TopologyIoLiveEvidenceContext(
                TopologyIoLiveEvidenceOrigin.Manual,
                TopologyIoLiveEvidenceKind.DI,
                "127.0.0.1:4000",
                19,
                0x10203040u,
                0xE245539Au,
                0x0003C000u,
                "manual reload",
                0x99887766u,
                0x55667788u,
                "CREVIS DI slot",
                2,
                0,
                0,
                0x0000CAFEu,
                "DigitalIORead(DI,16)",
                72,
                "Input",
                16);
            var di = journal.Append(
                TopologyIoLiveEvidenceRecord.CreateDigitalInputSuccess(
                    diContext,
                    new DateTime(
                        2026,
                        7,
                        29,
                        2,
                        3,
                        5,
                        DateTimeKind.Utc),
                    904,
                    905,
                    906,
                    "Valid",
                    true,
                    0xA55Au,
                    0xFFFFu,
                    "Input",
                    16,
                    0));

            AssertEx.Equal(TopologyIoLiveEvidenceOrigin.Manual, di.Context.Origin);
            AssertEx.Equal(TopologyIoLiveEvidenceKind.DI, di.Context.Kind);
            AssertEx.Equal((uint?)0x0000CAFEu, di.Context.IOReference);
            AssertEx.Equal("Input", di.Context.RequestedDirection);
            AssertEx.Equal((byte?)16, di.Context.RequestedBitWidth);
            AssertEx.Equal((ulong?)0xA55Au, di.Value);
            AssertEx.Equal((ulong?)0xFFFFu, di.ValidMask);
            AssertEx.Equal("Input", di.Direction);
            AssertEx.Equal((byte?)16, di.BitWidth);
            AssertEx.Equal((uint?)0, di.OutputRevision);
            AssertEx.False(di.Online.HasValue);
            AssertEx.False(di.DS402StatusWord.HasValue);
        }

        private static void FailureHasNoPriorSample()
        {
            var context = CreateHealthContext(
                TopologyIoLiveEvidenceOrigin.Manual,
                "CREVIS");
            var journal = new TopologyIoLiveEvidenceJournal(4);
            journal.Append(
                TopologyIoLiveEvidenceRecord.CreateHealthSuccess(
                    context,
                    new DateTime(
                        2026,
                        7,
                        29,
                        3,
                        0,
                        0,
                        DateTimeKind.Utc),
                    101,
                    102,
                    103,
                    "DataValid",
                    true,
                    true,
                    8,
                    0,
                    1,
                    2,
                    3,
                    4,
                    99,
                    100));
            var failure = journal.Append(
                TopologyIoLiveEvidenceRecord.CreateFailure(
                    context,
                    new DateTime(
                        2026,
                        7,
                        29,
                        3,
                        0,
                        1,
                        DateTimeKind.Utc),
                    "InvalidDataException",
                    "PLC payload rejected"));

            AssertEx.Equal(TopologyIoLiveEvidenceOutcome.Failure, failure.Outcome);
            AssertEx.False(failure.CycleCounter.HasValue);
            AssertEx.False(failure.PlcSnapshotSequence.HasValue);
            AssertEx.False(failure.PlcTimestampMicroseconds.HasValue);
            AssertEx.Equal<string>(null, failure.Quality);
            AssertEx.False(failure.DataValid.HasValue);
            AssertEx.False(failure.Value.HasValue);
            AssertEx.False(failure.ValidMask.HasValue);
            AssertEx.Equal<string>(null, failure.Direction);
            AssertEx.False(failure.BitWidth.HasValue);
            AssertEx.False(failure.OutputRevision.HasValue);
            AssertEx.False(failure.Online.HasValue);
            AssertEx.False(failure.EtherCATState.HasValue);
            AssertEx.False(failure.ALStatusCode.HasValue);
            AssertEx.False(failure.SlaveState.HasValue);
            AssertEx.False(failure.ClassState.HasValue);
            AssertEx.False(failure.DS402StatusWord.HasValue);
            AssertEx.False(failure.AxisError.HasValue);
            AssertEx.False(failure.LastValidCycle.HasValue);
            AssertEx.False(failure.LastStateChangeCycle.HasValue);
            AssertEx.Equal("InvalidDataException", failure.ErrorType);
            AssertEx.Equal("PLC payload rejected", failure.ErrorMessage);
        }

        private static void TextCsvEscapeAndNoBom()
        {
            var context = new TopologyIoLiveEvidenceContext(
                TopologyIoLiveEvidenceOrigin.Manual,
                TopologyIoLiveEvidenceKind.DI,
                "127.0.0.1:4000",
                21,
                0x01020304u,
                0x05060708u,
                0x0003C000u,
                "manual, \"reload\"",
                0x11121314u,
                0x15161718u,
                "CREVIS, \"DI\"\r\nslot",
                4,
                1,
                2,
                0x191A1B1Cu,
                "read, \"DI\"",
                88,
                "Input",
                8);
            var journal = new TopologyIoLiveEvidenceJournal(2);
            journal.Append(
                TopologyIoLiveEvidenceRecord.CreateFailure(
                    context,
                    new DateTime(
                        2026,
                        7,
                        29,
                        4,
                        0,
                        0,
                        DateTimeKind.Utc),
                    "IOException",
                    "bad, \"frame\"\r\nretry denied"));
            var snapshot = journal.CaptureSnapshot();
            var text = snapshot.BuildTextExport();
            var csv = snapshot.BuildCsvExport();

            AssertEx.Contains(
                "BOUNDARY=current-session gate passed before commit; successes are PLC responses parsed by the PC; failures are read-attempt evidence with no copied sample fields",
                text);
            AssertEx.Contains(
                "NOT PROOF=physical cable order, actual DI voltage or contact, physical DO feedback, or PLC implementation completeness",
                text);
            AssertEx.Contains(
                "NodeName=CREVIS, \"DI\"\\r\\nslot",
                text);
            AssertEx.Contains(
                "ErrorMessage=bad, \"frame\"\\r\\nretry denied",
                text);
            AssertEx.Contains(
                "\"manual, \"\"reload\"\"\"",
                csv);
            AssertEx.Contains(
                "\"CREVIS, \"\"DI\"\"\r\nslot\"",
                csv);
            AssertEx.Contains(
                "\"bad, \"\"frame\"\"\r\nretry denied\"",
                csv);
            AssertEx.Contains(
                "\"NOT PROOF=physical cable order, actual DI voltage or contact, physical DO feedback, or PLC implementation completeness\"",
                csv);
            AssertEx.Contains(
                "BOUNDARY=current-session gate passed before commit; successes are PLC responses parsed by the PC; failures are read-attempt evidence with no copied sample fields",
                csv);

            var directory = Path.Combine(
                Path.GetTempPath(),
                "ElmoTopologyIoLiveEvidence_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                AssertNoBomRoundTrip(
                    Path.Combine(directory, "live-evidence.txt"),
                    text);
                AssertNoBomRoundTrip(
                    Path.Combine(directory, "live-evidence.csv"),
                    csv);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static TopologyIoLiveEvidenceContext CreateHealthContext(
            TopologyIoLiveEvidenceOrigin origin,
            string nodeName)
        {
            return new TopologyIoLiveEvidenceContext(
                origin,
                TopologyIoLiveEvidenceKind.Health,
                "127.0.0.1:4000",
                7,
                0x10203040u,
                0xE245539Au,
                0x0003C000u,
                "automatic connect load",
                0x01010101u,
                0x02020202u,
                nodeName,
                1,
                0,
                null,
                null,
                "NodeHealth(0x02020202)",
                null,
                null,
                null);
        }

        private static void AssertNoBomRoundTrip(
            string path,
            string expected)
        {
            TopologyIoLiveEvidenceFile.SaveUtf8NoBom(path, expected);
            var bytes = File.ReadAllBytes(path);
            AssertEx.True(bytes.Length != 0);
            AssertEx.False(
                bytes.Length >= 3
                    && bytes[0] == 0xEF
                    && bytes[1] == 0xBB
                    && bytes[2] == 0xBF,
                "Live topology/I/O evidence was written with a UTF-8 BOM.");
            AssertEx.Equal(expected, Encoding.UTF8.GetString(bytes));
        }
    }
}
