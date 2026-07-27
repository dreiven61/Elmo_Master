using System;
using System.Collections.Generic;
using System.IO;

namespace LasalMotionControlLib.Tests
{
    internal static class NegativeWireToolTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add("NegativeWire.DryRunIsDefault", DryRunIsDefault);
            tests.Add("NegativeWire.LiveGuardIsExact", LiveGuardIsExact);
            tests.Add("NegativeWire.ScenarioAllowlistIsExact", ScenarioAllowlistIsExact);
            tests.Add("NegativeWire.MalformedCatalogRequestGolden", MalformedCatalogRequestGolden);
            tests.Add("NegativeWire.StaleIdentityMutatesOneField", StaleIdentityMutatesOneField);
            tests.Add("NegativeWire.BulkIdentityFramesGolden", BulkIdentityFramesGolden);
            tests.Add("NegativeWire.MotionAndWriteCommandsRejected", MotionAndWriteCommandsRejected);
            tests.Add("NegativeWire.AllowedCommandShapeIsFixed", AllowedCommandShapeIsFixed);
            tests.Add("NegativeWire.ErrorEnvelopeMustBeExact", ErrorEnvelopeMustBeExact);
        }

        private static void DryRunIsDefault()
        {
            var options = NegativeWireOptions.Parse(
                new[] { "negative-wire", "--scenario", "stale-map" });
            AssertEx.False(options.ExecuteLive);
            AssertEx.Equal(NegativeWireScenario.StaleMapRevision, options.Scenario);
            AssertEx.Equal(4000, options.RemotePort);
            AssertEx.Equal(3000, options.TimeoutMilliseconds);
        }

        private static void LiveGuardIsExact()
        {
            AssertEx.Throws<ArgumentException>(
                () => NegativeWireOptions.Parse(new[]
                {
                    "negative-wire", "--execute-live",
                    "--host", "192.0.2.10", "--local", "192.0.2.20",
                    "--output", "result.txt"
                }));
            AssertEx.Throws<ArgumentException>(
                () => NegativeWireOptions.Parse(new[]
                {
                    "negative-wire", "--execute-live",
                    "--confirm", "plc-raw-negative",
                    "--host", "192.0.2.10", "--local", "192.0.2.20",
                    "--output", "result.txt"
                }));
            AssertEx.Throws<ArgumentException>(
                () => NegativeWireOptions.Parse(new[]
                {
                    "negative-wire", "--execute-live",
                    "--confirm", NegativeWireOptions.LiveConfirmation,
                    "--host", "host.example", "--local", "192.0.2.20",
                    "--output", "result.txt"
                }));
            AssertEx.Throws<ArgumentException>(
                () => NegativeWireOptions.Parse(new[]
                {
                    "negative-wire", "--execute-live",
                    "--confirm", NegativeWireOptions.LiveConfirmation,
                    "--host", "192.0.2.10", "--local", "192.0.2.20"
                }));
        }

        private static void ScenarioAllowlistIsExact()
        {
            AssertEx.Throws<ArgumentException>(
                () => NegativeWireOptions.Parse(new[]
                {
                    "negative-wire", "--scenario", "Stale-Map"
                }));
            AssertEx.Throws<ArgumentException>(
                () => NegativeWireOptions.Parse(new[]
                {
                    "negative-wire", "--scenario", "raw-command"
                }));
            AssertEx.Throws<ArgumentException>(
                () => NegativeWireOptions.Parse(new[]
                {
                    "Negative-Wire", "--dry-run"
                }));
        }

        private static void MalformedCatalogRequestGolden()
        {
            const uint requestId = 0x11223344u;
            var request = NegativeWireTool.CreateMalformedCatalogInfoRequest(requestId);
            AssertEx.Equal(17, request.Length);
            AssertEx.Equal(LMC_CommandId.GetSignalCatalogInfo, TestFrame.ReadUInt16(request, 0));
            AssertEx.Equal((ushort)9, TestFrame.ReadUInt16(request, 4));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 6));
            AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(request, 8));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 10));
            AssertEx.Equal(requestId, TestFrame.ReadUInt32(request, 12));
            AssertEx.Equal((byte)0, request[16]);
        }

        private static void StaleIdentityMutatesOneField()
        {
            const uint requestId = 0x11223344u;
            const uint mapRevision = 0x957F101Eu;
            const uint bootId = 0x01020304u;
            var staleMap = NegativeWireTool.CreateStaleMapRequest(requestId, mapRevision);
            AssertEx.Equal(LMC_CommandId.GetSignalCatalogChunk, TestFrame.ReadUInt16(staleMap, 0));
            AssertEx.Equal(requestId, TestFrame.ReadUInt32(staleMap, 12));
            AssertEx.Equal(NegativeWireTool.MakeDifferentNonZero(mapRevision), TestFrame.ReadUInt32(staleMap, 16));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(staleMap, 20));
            AssertEx.Equal((ushort)1, TestFrame.ReadUInt16(staleMap, 22));

            var staleBoot = NegativeWireTool.CreateStaleBootRequest(requestId, bootId);
            AssertEx.Equal(LMC_CommandId.GetOperationStatus, TestFrame.ReadUInt16(staleBoot, 0));
            AssertEx.Equal((uint)1, TestFrame.ReadUInt32(staleBoot, 16));
            AssertEx.Equal(NegativeWireTool.MakeDifferentNonZero(bootId), TestFrame.ReadUInt32(staleBoot, 20));
            AssertEx.Equal((uint)1, NegativeWireTool.MakeDifferentNonZero(uint.MaxValue));
        }

        private static void BulkIdentityFramesGolden()
        {
            const uint requestId = 0x11223344u;
            const uint bulkId = 0x21u;
            const uint configRevision = 0x31u;
            const uint mapRevision = 0x957F101Eu;
            using (var connection = new LMCConnection())
            {
                var status = new LMCBulkStatus(null, bulkId, configRevision, mapRevision, LMCBulkState.Active, 1, 10);
                var configuration = new LMCBulkConfiguration(
                    status, 7, 1, connection.Diagnostics, new[] { 0x101u });
                var stale = NegativeWireTool.CreateStaleConfigRequest(requestId, configuration);
                AssertEx.Equal(LMC_CommandId.ReadBulkStatus, TestFrame.ReadUInt16(stale, 0));
                AssertEx.Equal(bulkId, TestFrame.ReadUInt32(stale, 16));
                AssertEx.Equal(configRevision + 1, TestFrame.ReadUInt32(stale, 20));
                AssertEx.Equal(mapRevision, TestFrame.ReadUInt32(stale, 24));

                var duplicate = NegativeWireTool.CreateDuplicateBulkReleaseRequest(
                    requestId, bulkId, configRevision, mapRevision);
                AssertEx.Equal(LMC_CommandId.ReleaseBulk, TestFrame.ReadUInt16(duplicate, 0));
                AssertEx.Equal(bulkId, TestFrame.ReadUInt32(duplicate, 16));
                AssertEx.Equal(configRevision, TestFrame.ReadUInt32(duplicate, 20));
                AssertEx.Equal(mapRevision, TestFrame.ReadUInt32(duplicate, 24));
            }
        }

        private static void MotionAndWriteCommandsRejected()
        {
            foreach (var command in new ushort[]
            {
                LMC_CommandId.GroupStop,
                LMC_CommandId.GetAdminCapabilities,
                LMC_CommandId.SubmitPIWrite,
                LMC_CommandId.SubmitSdo,
                LMC_CommandId.ConfigureRecorder
            })
            {
                var request = LMC_Frame.CreateRequest(command, 0, 8);
                AssertEx.Throws<InvalidOperationException>(
                    () => NegativeWireTool.EnsureAllowedRawRequest(request));
            }
        }

        private static void AllowedCommandShapeIsFixed()
        {
            var normalCatalogInfo = LMC_DiagnosticsFrame.GetSignalCatalogInfo(1);
            AssertEx.Throws<InvalidOperationException>(
                () => NegativeWireTool.EnsureAllowedRawRequest(normalCatalogInfo));
            var lengthMismatch = NegativeWireTool.CreateMalformedCatalogInfoRequest(1);
            Array.Resize(ref lengthMismatch, lengthMismatch.Length + 1);
            AssertEx.Throws<InvalidDataException>(
                () => NegativeWireTool.EnsureAllowedRawRequest(lengthMismatch));

            var nonzeroReference = NegativeWireTool.CreateMalformedCatalogInfoRequest(1);
            TestFrame.WriteUInt16(nonzeroReference, 6, 1);
            AssertEx.Throws<InvalidOperationException>(
                () => NegativeWireTool.EnsureAllowedRawRequest(nonzeroReference));

            var wrongSchema = NegativeWireTool.CreateMalformedCatalogInfoRequest(1);
            TestFrame.WriteUInt16(wrongSchema, 8, 2);
            AssertEx.Throws<InvalidOperationException>(
                () => NegativeWireTool.EnsureAllowedRawRequest(wrongSchema));

            var wrongFixedByte = NegativeWireTool.CreateMalformedCatalogInfoRequest(1);
            wrongFixedByte[16] = 1;
            AssertEx.Throws<InvalidOperationException>(
                () => NegativeWireTool.EnsureAllowedRawRequest(wrongFixedByte));

            var duplicate = NegativeWireTool.CreateDuplicateBulkReleaseRequest(
                1,
                1,
                1,
                1);
            AssertEx.Throws<InvalidOperationException>(
                () => NegativeWireTool.EnsureScenarioRequest(
                    duplicate,
                    NegativeWireScenario.DuplicateBulkRelease,
                    false));
            NegativeWireTool.EnsureScenarioRequest(
                duplicate,
                NegativeWireScenario.DuplicateBulkRelease,
                true);

            var temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                "LmcNegativeWire-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryDirectory);
            var existingReport = Path.Combine(temporaryDirectory, "existing.txt");
            File.WriteAllText(existingReport, "sentinel");
            try
            {
                AssertEx.Throws<IOException>(() =>
                {
                    string ignored;
                    using (NegativeWireTool.ReserveLiveReport(
                        existingReport,
                        out ignored))
                    {
                    }
                });
                AssertEx.Equal("sentinel", File.ReadAllText(existingReport));
            }
            finally
            {
                File.Delete(existingReport);
                Directory.Delete(temporaryDirectory);
            }
        }

        private static void ErrorEnvelopeMustBeExact()
        {
            const uint requestId = 0x11223344u;
            var parsed = NegativeWireTool.ValidateNegativeResponse(
                DiagnosticsError(requestId, LMCDiagnosticsDetailCode.MapRevisionMismatch),
                requestId,
                LMCDiagnosticsDetailCode.MapRevisionMismatch);
            AssertEx.Equal(LMCDiagnosticsDetailCode.MapRevisionMismatch, parsed.Detail);
            AssertEx.Throws<InvalidDataException>(
                () => NegativeWireTool.ValidateNegativeResponse(
                    DiagnosticsError(requestId, LMCDiagnosticsDetailCode.HandleOrGenerationStale),
                    requestId,
                    LMCDiagnosticsDetailCode.MapRevisionMismatch));
            var reserved = TestFrame.Response(
                0,
                CommonErrorPayload(requestId, LMCDiagnosticsDetailCode.MapRevisionMismatch),
                1);
            AssertEx.Throws<InvalidDataException>(
                () => NegativeWireTool.ValidateNegativeResponse(
                    reserved, requestId, LMCDiagnosticsDetailCode.MapRevisionMismatch));
            var overlength = new byte[17];
            Buffer.BlockCopy(
                CommonErrorPayload(requestId, LMCDiagnosticsDetailCode.MapRevisionMismatch),
                0, overlength, 0, 16);
            AssertEx.Throws<InvalidDataException>(
                () => NegativeWireTool.ValidateNegativeResponse(
                    TestFrame.Response(0, overlength),
                    requestId,
                    LMCDiagnosticsDetailCode.MapRevisionMismatch));
        }

        private static byte[] DiagnosticsError(uint requestId, LMCDiagnosticsDetailCode detail)
        {
            return TestFrame.Response(0, CommonErrorPayload(requestId, detail));
        }

        private static byte[] CommonErrorPayload(uint requestId, LMCDiagnosticsDetailCode detail)
        {
            var payload = new byte[16];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt16(payload, 2, 0);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -32000);
            TestFrame.WriteUInt32(payload, 8, requestId);
            TestFrame.WriteUInt32(payload, 12, (uint)detail);
            return payload;
        }
    }
}
