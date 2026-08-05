using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LasalMotionControlLib.Tests
{
    internal static class TopologyIoQualificationToolTests
    {
        private static readonly string ValidSourceFingerprint =
            new string('a', 40)
            + "/"
            + new string('b', 40)
            + "/"
            + new string('c', 40);

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.TopologyIo.OptionsAreFailClosed",
                OptionsAreFailClosed);
            tests.Add(
                "Qualification.TopologyIo.ReadOnlyAllowlistAcceptsExactFrames",
                ReadOnlyAllowlistAcceptsExactFrames);
            tests.Add(
                "Qualification.TopologyIo.MutationAndMalformedFramesRejected",
                MutationAndMalformedFramesRejected);
            tests.Add(
                "Qualification.TopologyIo.DormantCapabilitiesRequired",
                DormantCapabilitiesRequired);
            tests.Add(
                "Qualification.TopologyIo.DryRunWritesNoNetworkEvidence",
                DryRunWritesNoNetworkEvidence);
            tests.Add(
                "Qualification.TopologyIo.RawReadSequenceParsesCurrentTopology",
                RawReadSequenceParsesCurrentTopology);
            tests.Add(
                "Qualification.TopologyIo.RawInfoIsBoundedBeforeChunk",
                RawInfoIsBoundedBeforeChunk);
            tests.Add(
                "Qualification.TopologyIo.ChunkIdentityIsConsistent",
                ChunkIdentityIsConsistent);
            tests.Add(
                "Qualification.TopologyIo.HealthMatchesTopologyKind",
                HealthMatchesTopologyKind);
            tests.Add(
                "Qualification.TopologyIo.DigitalIoNodeMatchesTopology",
                DigitalIoNodeMatchesTopology);
            tests.Add(
                "Qualification.TopologyIo.CapabilityIdentityStaysStable",
                CapabilityIdentityStaysStable);
            tests.Add(
                "Qualification.TopologyIo.CheckpointFailurePreservesPrefix",
                CheckpointFailurePreservesPrefix);
        }

        private static void OptionsAreFailClosed()
        {
            AssertEx.Throws<ArgumentException>(() =>
                TopologyIoQualificationOptions.Parse(
                    new[] { "topology-io-qualify", "--dry-run" }));
            var dry = TopologyIoQualificationOptions.Parse(new[]
            {
                "topology-io-qualify",
                "--scope",
                TopologyIoQualificationOptions
                    .IntegratedReadOwnerDormantScope,
                "--dry-run"
            });
            AssertEx.False(dry.ExecuteLive);
            AssertEx.Equal(
                TopologyIoQualificationScope.IntegratedReadOwnerDormant,
                dry.Scope);
            AssertEx.True(dry.ScopeWasExplicit);

            var inventoryDry = TopologyIoQualificationOptions.Parse(new[]
            {
                "topology-io-qualify",
                "--scope",
                TopologyIoQualificationOptions.TopologyInventoryScope,
                "--dry-run"
            });
            AssertEx.Equal(
                TopologyIoQualificationScope.TopologyInventory,
                inventoryDry.Scope);
            AssertEx.True(inventoryDry.ScopeWasExplicit);

            AssertEx.Throws<ArgumentException>(() =>
                TopologyIoQualificationOptions.Parse(new[]
                {
                    "topology-io-qualify",
                    "--scope",
                    TopologyIoQualificationOptions
                        .IntegratedReadOwnerDormantScope,
                    "--dry-run",
                    "--execute-live"
                }));
            AssertEx.Throws<ArgumentException>(() =>
                TopologyIoQualificationOptions.Parse(new[]
                {
                    "topology-io-qualify",
                    "--scope",
                    TopologyIoQualificationOptions
                        .IntegratedReadOwnerDormantScope,
                    "--execute-live",
                    "--confirm",
                    "WRONG",
                    "--host",
                    "192.0.2.1",
                    "--local",
                    "192.0.2.2",
                    "--source-fingerprint",
                    ValidSourceFingerprint,
                    "--output",
                    "report.txt"
                }));
            AssertEx.Throws<ArgumentException>(() =>
                TopologyIoQualificationOptions.Parse(new[]
                {
                    "topology-io-qualify",
                    "--scope",
                    TopologyIoQualificationOptions.TopologyInventoryScope,
                    "--scope",
                    TopologyIoQualificationOptions
                        .IntegratedReadOwnerDormantScope,
                    "--dry-run"
                }));
            AssertEx.Throws<ArgumentException>(() =>
                TopologyIoQualificationOptions.Parse(new[]
                {
                    "topology-io-qualify",
                    "--scope",
                    "unknown",
                    "--dry-run"
                }));

            var live = TopologyIoQualificationOptions.Parse(new[]
            {
                "topology-io-qualify",
                "--scope",
                TopologyIoQualificationOptions
                    .IntegratedReadOwnerDormantScope,
                "--execute-live",
                "--confirm",
                TopologyIoQualificationOptions.LiveConfirmation,
                "--host",
                "192.0.2.1",
                "--local",
                "192.0.2.2",
                "--source-fingerprint",
                ValidSourceFingerprint,
                "--output",
                "report.txt"
            });
            AssertEx.True(live.ExecuteLive);
            AssertEx.Equal(4000, live.RemotePort);

            AssertEx.Throws<ArgumentException>(() =>
                TopologyIoQualificationOptions.Parse(new[]
                {
                    "topology-io-qualify",
                    "--scope",
                    TopologyIoQualificationOptions.TopologyInventoryScope,
                    "--execute-live",
                    "--confirm",
                    TopologyIoQualificationOptions.LiveConfirmation,
                    "--host",
                    "192.0.2.1",
                    "--local",
                    "192.0.2.2",
                    "--source-fingerprint",
                    ValidSourceFingerprint,
                    "--output",
                    "report.txt"
                }));
            var inventoryLive = TopologyIoQualificationOptions.Parse(new[]
            {
                "topology-io-qualify",
                "--scope",
                TopologyIoQualificationOptions.TopologyInventoryScope,
                "--execute-live",
                "--confirm",
                TopologyIoQualificationOptions
                    .TopologyInventoryLiveConfirmation,
                "--host",
                "192.0.2.1",
                "--local",
                "192.0.2.2",
                "--source-fingerprint",
                ValidSourceFingerprint,
                "--output",
                "report.txt"
            });
            AssertEx.Equal(
                TopologyIoQualificationScope.TopologyInventory,
                inventoryLive.Scope);
            AssertEx.Equal(
                ValidSourceFingerprint,
                inventoryLive.SourceFingerprint);

            AssertEx.Throws<ArgumentException>(() =>
                TopologyIoQualificationOptions.Parse(new[]
                {
                    "topology-io-qualify",
                    "--scope",
                    TopologyIoQualificationOptions.TopologyInventoryScope,
                    "--execute-live",
                    "--confirm",
                    TopologyIoQualificationOptions
                        .TopologyInventoryLiveConfirmation,
                    "--host",
                    "192.0.2.1",
                    "--local",
                    "192.0.2.2",
                    "--output",
                    "report.txt"
                }));

            AssertEx.Throws<ArgumentException>(() =>
                TopologyIoQualificationOptions.Parse(new[]
                {
                    "topology-io-qualify",
                    "--scope",
                    TopologyIoQualificationOptions.TopologyInventoryScope,
                    "--execute-live",
                    "--confirm",
                    TopologyIoQualificationOptions
                        .TopologyInventoryLiveConfirmation,
                    "--host",
                    "192.0.2.1",
                    "--local",
                    "192.0.2.2",
                    "--source-fingerprint",
                    "not-a-source-fingerprint",
                    "--output",
                    "report.txt"
                }));
        }

        private static void ReadOnlyAllowlistAcceptsExactFrames()
        {
            TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(1));
            TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                    2,
                    TopologyIoQualificationTool.ExpectedTopologyRevision,
                    0,
                    1));
            TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                LMC_DiagnosticsFrame.ReadEtherCATNodeHealth(
                    3,
                    TopologyIoQualificationTool.ExpectedTopologyRevision,
                    0xEC000001u));
            TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                LMC_DiagnosticsFrame.ReadDigitalIO(
                    4,
                    new LMCDigitalIOReadRequest(
                        TopologyIoQualificationTool.ExpectedTopologyRevision,
                        0x00010001u,
                        LMCDigitalIODirection.Input,
                        32)));

            TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(5),
                TopologyIoQualificationScope.TopologyInventory);
            TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                    6,
                    TopologyIoQualificationTool.ExpectedTopologyRevision,
                    0,
                    1),
                TopologyIoQualificationScope.TopologyInventory);
            AssertEx.Throws<InvalidOperationException>(() =>
                TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                    LMC_DiagnosticsFrame.ReadEtherCATNodeHealth(
                        7,
                        TopologyIoQualificationTool
                            .ExpectedTopologyRevision,
                        0xEC000001u),
                    TopologyIoQualificationScope.TopologyInventory));
            AssertEx.Throws<InvalidOperationException>(() =>
                TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                    LMC_DiagnosticsFrame.ReadDigitalIO(
                        8,
                        new LMCDigitalIOReadRequest(
                            TopologyIoQualificationTool
                                .ExpectedTopologyRevision,
                            0x00010001u,
                            LMCDigitalIODirection.Input,
                            32)),
                    TopologyIoQualificationScope.TopologyInventory));
        }

        private static void MutationAndMalformedFramesRejected()
        {
            var outputWrite = LMC_Frame.CreateRequest(
                LMC_CommandId.SubmitDigitalOutputWrite,
                0,
                LMC_DiagnosticsFrame.DigitalOutputWriteRequestPayloadLength);
            AssertEx.Throws<InvalidOperationException>(() =>
                TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                    outputWrite));
            AssertEx.Throws<InvalidOperationException>(() =>
                TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                    outputWrite,
                    TopologyIoQualificationScope.TopologyInventory));

            var sdoWrite = LMC_Frame.CreateRequest(
                LMC_CommandId.SubmitSdo,
                0,
                36);
            AssertEx.Throws<InvalidOperationException>(() =>
                TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                    sdoWrite));

            var lengthMismatch =
                LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(1);
            Array.Resize(ref lengthMismatch, lengthMismatch.Length + 1);
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                    lengthMismatch));

            var nonzeroReference =
                LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(1);
            TestFrame.WriteUInt16(nonzeroReference, 6, 1);
            AssertReadOnlyRequestRejected(nonzeroReference);

            var badSchema =
                LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(1);
            TestFrame.WriteUInt16(
                badSchema,
                LMC_Frame.HeaderSize,
                2);
            AssertReadOnlyRequestRejected(badSchema);

            var badFlags =
                LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(1);
            TestFrame.WriteUInt16(
                badFlags,
                LMC_Frame.HeaderSize + 2,
                1);
            AssertReadOnlyRequestRejected(badFlags);

            var zeroRequestId =
                LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(1);
            TestFrame.WriteUInt32(
                zeroRequestId,
                LMC_Frame.HeaderSize + 4,
                0);
            AssertReadOnlyRequestRejected(zeroRequestId);

            var zeroTopologyRevision =
                LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                    1,
                    TopologyIoQualificationTool.ExpectedTopologyRevision,
                    0,
                    1);
            TestFrame.WriteUInt32(
                zeroTopologyRevision,
                LMC_Frame.HeaderSize + 8,
                0);
            AssertReadOnlyRequestRejected(zeroTopologyRevision);

            var zeroChunkMaximum =
                LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                    1,
                    TopologyIoQualificationTool.ExpectedTopologyRevision,
                    0,
                    1);
            TestFrame.WriteUInt16(
                zeroChunkMaximum,
                LMC_Frame.HeaderSize + 14,
                0);
            AssertReadOnlyRequestRejected(zeroChunkMaximum);

            var excessiveChunkMaximum =
                LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                    1,
                    TopologyIoQualificationTool.ExpectedTopologyRevision,
                    0,
                    1);
            TestFrame.WriteUInt16(
                excessiveChunkMaximum,
                LMC_Frame.HeaderSize + 14,
                17);
            AssertReadOnlyRequestRejected(excessiveChunkMaximum);

            var zeroNodeId =
                LMC_DiagnosticsFrame.ReadEtherCATNodeHealth(
                    1,
                    TopologyIoQualificationTool.ExpectedTopologyRevision,
                    0xEC000001u);
            TestFrame.WriteUInt32(
                zeroNodeId,
                LMC_Frame.HeaderSize + 12,
                0);
            AssertReadOnlyRequestRejected(zeroNodeId);

            var zeroIoReference = LMC_DiagnosticsFrame.ReadDigitalIO(
                1,
                new LMCDigitalIOReadRequest(
                    TopologyIoQualificationTool.ExpectedTopologyRevision,
                    0x00010001u,
                    LMCDigitalIODirection.Input,
                    32));
            TestFrame.WriteUInt32(
                zeroIoReference,
                LMC_Frame.HeaderSize + 12,
                0);
            AssertReadOnlyRequestRejected(zeroIoReference);

            var badDirection = LMC_DiagnosticsFrame.ReadDigitalIO(
                1,
                new LMCDigitalIOReadRequest(
                    TopologyIoQualificationTool.ExpectedTopologyRevision,
                    0x00010001u,
                    LMCDigitalIODirection.Input,
                    32));
            badDirection[LMC_Frame.HeaderSize + 16] = 3;
            AssertReadOnlyRequestRejected(badDirection);

            var zeroWidth = LMC_DiagnosticsFrame.ReadDigitalIO(
                1,
                new LMCDigitalIOReadRequest(
                    TopologyIoQualificationTool.ExpectedTopologyRevision,
                    0x00010001u,
                    LMCDigitalIODirection.Input,
                    32));
            zeroWidth[LMC_Frame.HeaderSize + 17] = 0;
            AssertReadOnlyRequestRejected(zeroWidth);

            var excessiveWidth = LMC_DiagnosticsFrame.ReadDigitalIO(
                1,
                new LMCDigitalIOReadRequest(
                    TopologyIoQualificationTool.ExpectedTopologyRevision,
                    0x00010001u,
                    LMCDigitalIODirection.Input,
                    32));
            excessiveWidth[LMC_Frame.HeaderSize + 17] = 65;
            AssertReadOnlyRequestRejected(excessiveWidth);

            var badDigitalIoReserved = LMC_DiagnosticsFrame.ReadDigitalIO(
                1,
                new LMCDigitalIOReadRequest(
                    TopologyIoQualificationTool.ExpectedTopologyRevision,
                    0x00010001u,
                    LMCDigitalIODirection.Input,
                    32));
            TestFrame.WriteUInt16(
                badDigitalIoReserved,
                LMC_Frame.HeaderSize + 18,
                1);
            AssertReadOnlyRequestRejected(badDigitalIoReserved);
        }

        private static void DormantCapabilitiesRequired()
        {
            TopologyIoQualificationTool
                .ValidateTopologyInventoryCapabilities(
                    Capabilities(
                        LMCDiagnosticCapability.EtherCATTopology
                            | LMCDiagnosticCapability.EtherCATNodeHealth
                            | LMCDiagnosticCapability.DigitalIORead
                            | LMCDiagnosticCapability.DigitalIOWrite,
                        1,
                        1320,
                        2040));
            TopologyIoQualificationTool.ValidateDormantCapabilities(
                Capabilities(
                    LMCDiagnosticCapability.EtherCATTopology,
                    1,
                    1320,
                    2040));

            AssertEx.Throws<NotSupportedException>(() =>
                TopologyIoQualificationTool.ValidateDormantCapabilities(
                    Capabilities(
                        LMCDiagnosticCapability.None,
                        1,
                        1320,
                        2040)));
            AssertEx.Throws<InvalidOperationException>(() =>
                TopologyIoQualificationTool.ValidateDormantCapabilities(
                    Capabilities(
                        LMCDiagnosticCapability.EtherCATTopology
                            | LMCDiagnosticCapability.EtherCATNodeHealth,
                        1,
                        1320,
                        2040)));
            AssertEx.Throws<InvalidOperationException>(() =>
                TopologyIoQualificationTool.ValidateDormantCapabilities(
                    Capabilities(
                        LMCDiagnosticCapability.EtherCATTopology
                            | LMCDiagnosticCapability.DigitalIORead,
                        1,
                        1320,
                        2040)));
            AssertEx.Throws<InvalidOperationException>(() =>
                TopologyIoQualificationTool.ValidateDormantCapabilities(
                    Capabilities(
                        LMCDiagnosticCapability.EtherCATTopology
                            | LMCDiagnosticCapability.DigitalIOWrite,
                        1,
                        1320,
                        2040)));
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool.ValidateDormantCapabilities(
                    Capabilities(
                        LMCDiagnosticCapability.EtherCATTopology,
                        0,
                        1320,
                        2040)));
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool.ValidateDormantCapabilities(
                    Capabilities(
                        LMCDiagnosticCapability.EtherCATTopology,
                        1,
                        19,
                        2040)));
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool.ValidateDormantCapabilities(
                    Capabilities(
                        LMCDiagnosticCapability.EtherCATTopology,
                        1,
                        1320,
                        2040,
                        TopologyIoQualificationTool.ExpectedMapRevision + 1)));
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool.ValidateDormantCapabilities(
                    Capabilities(
                        LMCDiagnosticCapability.EtherCATTopology,
                        1,
                        1320,
                        2040,
                        TopologyIoQualificationTool.ExpectedMapRevision,
                        0)));
            AssertEx.Throws<NotSupportedException>(() =>
                TopologyIoQualificationTool
                    .ValidateTopologyInventoryCapabilities(
                        Capabilities(
                            LMCDiagnosticCapability.None,
                            1,
                            1320,
                            2040)));
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool
                    .ValidateTopologyInventoryCapabilities(
                        Capabilities(
                            LMCDiagnosticCapability.EtherCATTopology,
                            0,
                            1320,
                            2040)));
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool
                    .ValidateTopologyInventoryCapabilities(
                        Capabilities(
                            LMCDiagnosticCapability.EtherCATTopology,
                            1,
                            15,
                            2040)));
        }

        private static void DryRunWritesNoNetworkEvidence()
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                "LmcTopologyIoQualification-"
                    + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var output = Path.Combine(directory, "dry-run.txt");
            var inventoryOutput = Path.Combine(
                directory,
                "topology-inventory-dry-run.txt");
            try
            {
                var originalOut = Console.Out;
                int exitCode;
                try
                {
                    Console.SetOut(new StringWriter());
                    exitCode = TopologyIoQualificationTool.Run(new[]
                    {
                        "topology-io-qualify",
                        "--scope",
                        TopologyIoQualificationOptions
                            .IntegratedReadOwnerDormantScope,
                        "--dry-run",
                        "--output",
                        output
                    });
                }
                finally
                {
                    Console.SetOut(originalOut);
                }

                AssertEx.Equal(
                    TopologyIoQualificationTool.SuccessExitCode,
                    exitCode);
                var text = File.ReadAllText(output);
                AssertEx.True(text.Contains("MODE=DRY_RUN"));
                AssertEx.True(text.Contains("NETWORK_CONNECTED=FALSE"));
                AssertEx.True(text.Contains("RAW_WRITE_0x7E23=FORBIDDEN"));
                AssertEx.True(text.Contains("PLANNED_REQUEST_COUNT=17"));
                AssertEx.True(text.Contains("PLANNED_REQUEST_16_HEX="));
                AssertEx.True(text.Contains("TEST_EXECUTABLE_SHA256="));
                AssertEx.True(text.Contains("SDK_ASSEMBLY_SHA256="));
                AssertEx.Throws<IOException>(() =>
                    NewDryRunReport().Save(output));
                AssertEx.Equal(text, File.ReadAllText(output));

                try
                {
                    Console.SetOut(new StringWriter());
                    exitCode = TopologyIoQualificationTool.Run(new[]
                    {
                        "topology-io-qualify",
                        "--scope",
                        TopologyIoQualificationOptions
                            .TopologyInventoryScope,
                        "--dry-run",
                        "--output",
                        inventoryOutput
                    });
                }
                finally
                {
                    Console.SetOut(originalOut);
                }

                AssertEx.Equal(
                    TopologyIoQualificationTool.SuccessExitCode,
                    exitCode);
                var inventoryText = File.ReadAllText(inventoryOutput);
                AssertEx.True(inventoryText.Contains(
                    "QUALIFICATION_SCOPE=TOPOLOGY_INVENTORY"));
                AssertEx.True(inventoryText.Contains(
                    "RAW_ALLOWLIST=0x7E11,0x7E12"));
                AssertEx.True(inventoryText.Contains(
                    "RAW_READ_0x7E13=FORBIDDEN"));
                AssertEx.True(inventoryText.Contains(
                    "RAW_READ_0x7E22=FORBIDDEN"));
                AssertEx.True(inventoryText.Contains(
                    "RAW_WRITE_0x7E23=FORBIDDEN"));
                AssertEx.True(inventoryText.Contains(
                    "PLANNED_REQUEST_COUNT=8"));
                AssertEx.True(inventoryText.Contains(
                    "PLANNED_REQUEST_07_HEX="));
                AssertEx.False(inventoryText.Contains(
                    "PLANNED_REQUEST_08_HEX="));
            }
            finally
            {
                if (File.Exists(output))
                {
                    File.Delete(output);
                }

                if (File.Exists(inventoryOutput))
                {
                    File.Delete(inventoryOutput);
                }

                Directory.Delete(directory);
            }
        }

        private static void RawReadSequenceParsesCurrentTopology()
        {
            var exchangeCount = 0;
            var selectors = new List<string>();
            var report = new TopologyIoQualificationReport(
                TopologyIoQualificationOptions.Parse(new[]
                {
                    "topology-io-qualify",
                    "--scope",
                    TopologyIoQualificationOptions
                        .IntegratedReadOwnerDormantScope,
                    "--dry-run"
                }));
            var canonical = CurrentTopologyCanonicalBytes();
            var result = TopologyIoQualificationTool.RunReadOnlyRaw(
                request =>
                {
                    exchangeCount++;
                    selectors.Add(DescribeReadOnlySelector(request));
                    return RespondToReadOnlyRequest(request, canonical);
                },
                report);

            AssertEx.Equal(17, exchangeCount);
            var expectedSelectors = new[]
            {
                "7E11",
                "7E12:0",
                "7E12:1",
                "7E12:2",
                "7E12:3",
                "7E12:4",
                "7E12:5",
                "7E12:6",
                "7E13:EC000001",
                "7E13:EC000101",
                "7E13:EC000102",
                "7E13:EC000103",
                "7E13:EC000104",
                "7E13:EC010001",
                "7E13:EC010002",
                "7E22:00010001:1:32",
                "7E22:00010002:2:32"
            };
            AssertEx.Equal(expectedSelectors.Length, selectors.Count);
            for (var index = 0; index < expectedSelectors.Length; index++)
            {
                AssertEx.Equal(expectedSelectors[index], selectors[index]);
            }

            AssertEx.Equal(
                TopologyIoQualificationTool.ExpectedTopologyRevision,
                result.Topology.TopologyRevision);
            AssertEx.Equal(7, result.Topology.Entries.Count);
            AssertEx.Equal(7, result.Health.Count);
            var expectedEntries =
                TopologyIoQualificationTool.ExpectedCurrentTopologyEntries();
            for (var index = 0; index < expectedEntries.Length; index++)
            {
                AssertEx.Equal(
                    expectedEntries[index].NodeId,
                    result.Health[index].NodeId);
            }

            AssertEx.Equal(2, result.DigitalIo.Count);
            AssertEx.Equal(
                LMCDigitalIODirection.Input,
                result.DigitalIo[0].Direction);
            AssertEx.Equal(0x00010001u, result.DigitalIo[0].IOReference);
            AssertEx.Equal(0xEC010001u, result.DigitalIo[0].NodeId);
            AssertEx.Equal((byte)32, result.DigitalIo[0].BitWidth);
            AssertEx.Equal(
                LMCDigitalIODirection.Output,
                result.DigitalIo[1].Direction);
            AssertEx.Equal(0x00010002u, result.DigitalIo[1].IOReference);
            AssertEx.Equal(0xEC010002u, result.DigitalIo[1].NodeId);
            AssertEx.Equal((byte)32, result.DigitalIo[1].BitWidth);
            AssertEx.True(report.ToString().Contains("RAW_SCHEMA_RESULT=PASS"));

            var inventoryExchangeCount = 0;
            var inventorySelectors = new List<string>();
            var inventoryReport = NewDryRunReport(
                TopologyIoQualificationScope.TopologyInventory);
            var inventoryResult = TopologyIoQualificationTool
                .RunTopologyInventoryRaw(
                    request =>
                    {
                        inventoryExchangeCount++;
                        inventorySelectors.Add(
                            DescribeReadOnlySelector(request));
                        return RespondToReadOnlyRequest(
                            request,
                            canonical,
                            TopologyIoQualificationScope
                                .TopologyInventory);
                    },
                    inventoryReport);
            AssertEx.Equal(8, inventoryExchangeCount);
            var expectedInventorySelectors = new[]
            {
                "7E11",
                "7E12:0",
                "7E12:1",
                "7E12:2",
                "7E12:3",
                "7E12:4",
                "7E12:5",
                "7E12:6"
            };
            AssertEx.Equal(
                expectedInventorySelectors.Length,
                inventorySelectors.Count);
            for (var index = 0;
                index < expectedInventorySelectors.Length;
                index++)
            {
                AssertEx.Equal(
                    expectedInventorySelectors[index],
                    inventorySelectors[index]);
            }

            AssertEx.Equal(
                TopologyIoQualificationTool.ExpectedTopologyRevision,
                inventoryResult.Topology.TopologyRevision);
            AssertEx.Equal(7, inventoryResult.Topology.Entries.Count);
            AssertEx.Equal(0, inventoryResult.Health.Count);
            AssertEx.Equal(0, inventoryResult.DigitalIo.Count);
            var inventoryReportText = inventoryReport.ToString();
            AssertEx.Contains(
                "RAW_TOPOLOGY_REQUEST_COUNT=8",
                inventoryReportText);
            AssertEx.Contains(
                "LIVE_GATE_RESULT=STATIC_TOPOLOGY_ONLY",
                inventoryReportText);
            AssertEx.False(inventoryReportText.Contains(
                "NODE_HEALTH_00_REQUEST"));
            AssertEx.False(inventoryReportText.Contains(
                "DIGITAL_INPUT_REQUEST"));
            AssertEx.False(inventoryReportText.Contains(
                "DIGITAL_OUTPUT_SHADOW_REQUEST"));
        }

        private static void RawInfoIsBoundedBeforeChunk()
        {
            AssertRawInfoRejectedBeforeChunk(false);
            AssertRawInfoRejectedBeforeChunk(true);
        }

        private static void ChunkIdentityIsConsistent()
        {
            var exchangeCount = 0;
            var canonical = CurrentTopologyCanonicalBytes();
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool.RunReadOnlyRaw(
                    request =>
                    {
                        exchangeCount++;
                        var response = RespondToReadOnlyRequest(
                            request,
                            canonical);
                        if (TestFrame.ReadUInt16(request, 0)
                                == LMC_CommandId.GetEtherCATTopologyChunk
                            && TestFrame.ReadUInt16(
                                request,
                                LMC_Frame.HeaderSize + 12) == 0)
                        {
                            TestFrame.WriteUInt16(
                                response,
                                LMC_Frame.HeaderSize + 24,
                                8);
                        }

                        return response;
                    },
                    NewDryRunReport()));
            AssertEx.Equal(2, exchangeCount);
        }

        private static void HealthMatchesTopologyKind()
        {
            AssertHealthTopologyMismatchRejected(0xEC000001u, true);
            AssertHealthTopologyMismatchRejected(0xEC000101u, false);
        }

        private static void DigitalIoNodeMatchesTopology()
        {
            AssertDigitalIoNodeMismatchRejected(
                LMCDigitalIODirection.Input);
            AssertDigitalIoNodeMismatchRejected(
                LMCDigitalIODirection.Output);
        }

        private static void CapabilityIdentityStaysStable()
        {
            var capabilityReadCount = 0;
            var exchangeCount = 0;
            var canonical = CurrentTopologyCanonicalBytes();
            var invalidPreconditionReport = NewDryRunReport();
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool
                    .RunReadOnlyRawWithCapabilityIdentity(
                        () => Capabilities(
                            LMCDiagnosticCapability.EtherCATTopology,
                            0x01020304u,
                            1320,
                            2040,
                            TopologyIoQualificationTool.ExpectedMapRevision + 1),
                        request =>
                        {
                            exchangeCount++;
                            return RespondToReadOnlyRequest(
                                request,
                                canonical);
                        },
                        invalidPreconditionReport));
            AssertEx.Equal(0, exchangeCount);
            AssertEx.Contains(
                "DIAGNOSTICS_BOOT_ID_BEFORE=0x01020304",
                invalidPreconditionReport.ToString());
            AssertEx.Contains(
                "MAP_REVISION_BEFORE=0x957F101F",
                invalidPreconditionReport.ToString());

            exchangeCount = 0;
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool
                    .RunReadOnlyRawWithCapabilityIdentity(
                        () =>
                        {
                            capabilityReadCount++;
                            return Capabilities(
                                LMCDiagnosticCapability.EtherCATTopology,
                                capabilityReadCount == 1 ? 1u : 2u,
                                1320,
                                2040);
                        },
                        request =>
                        {
                            exchangeCount++;
                            return RespondToReadOnlyRequest(
                                request,
                                canonical);
                        },
                        NewDryRunReport()));
            AssertEx.Equal(2, capabilityReadCount);
            AssertEx.Equal(17, exchangeCount);

            capabilityReadCount = 0;
            exchangeCount = 0;
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool
                    .RunTopologyInventoryRawWithCapabilityIdentity(
                        () =>
                        {
                            capabilityReadCount++;
                            return Capabilities(
                                LMCDiagnosticCapability.EtherCATTopology
                                    | LMCDiagnosticCapability
                                        .EtherCATNodeHealth,
                                capabilityReadCount == 1 ? 1u : 2u,
                                1320,
                                2040);
                        },
                        request =>
                        {
                            exchangeCount++;
                            return RespondToReadOnlyRequest(
                                request,
                                canonical,
                                TopologyIoQualificationScope
                                    .TopologyInventory);
                        },
                        NewDryRunReport(
                            TopologyIoQualificationScope
                                .TopologyInventory)));
            AssertEx.Equal(2, capabilityReadCount);
            AssertEx.Equal(8, exchangeCount);

            capabilityReadCount = 0;
            exchangeCount = 0;
            var inventoryReport = NewDryRunReport(
                TopologyIoQualificationScope.TopologyInventory);
            TopologyIoQualificationTool
                .RunTopologyInventoryRawWithCapabilityIdentity(
                    () =>
                    {
                        capabilityReadCount++;
                        return Capabilities(
                            LMCDiagnosticCapability.EtherCATTopology,
                            0x11223344u,
                            1320,
                            2040);
                    },
                    request =>
                    {
                        exchangeCount++;
                        return RespondToReadOnlyRequest(
                            request,
                            canonical,
                            TopologyIoQualificationScope
                                .TopologyInventory);
                    },
                    inventoryReport);
            AssertEx.Equal(2, capabilityReadCount);
            AssertEx.Equal(8, exchangeCount);
            AssertEx.Contains(
                "TOPOLOGY_CAPABILITY_PRECONDITION=PASS",
                inventoryReport.ToString());
            AssertEx.Contains(
                "TOPOLOGY_CAPABILITY_POSTCONDITION=PASS",
                inventoryReport.ToString());
            AssertEx.Contains(
                "CAPABILITY_IDENTITY_RESULT=PASS",
                inventoryReport.ToString());
            AssertEx.Contains(
                "MAP_REVISION_BEFORE=0x957F101E",
                inventoryReport.ToString());
            AssertEx.Contains(
                "MAP_REVISION_AFTER=0x957F101E",
                inventoryReport.ToString());
            AssertEx.Contains(
                "DIAGNOSTICS_BUILD_BEFORE=0x00000001",
                inventoryReport.ToString());
            AssertEx.Contains(
                "RAW_TOTAL_REQUEST_COUNT=8",
                inventoryReport.ToString());
        }

        private static void CheckpointFailurePreservesPrefix()
        {
            var existing = Encoding.ASCII.GetBytes("KEEP-EXISTING");
            var nonEmpty = new MemoryStream(existing);
            nonEmpty.Position = 0;
            AssertEx.Throws<ArgumentException>(() =>
                NewDryRunReport().AttachCheckpointStream(nonEmpty));
            AssertEx.SequenceEqual(existing, nonEmpty.ToArray());

            var report = NewDryRunReport();
            var stream = new FailingAppendStream();
            report.AttachCheckpointStream(stream);
            var durablePrefix = stream.ToArray();
            stream.FailNextWrite();

            AssertEx.Throws<TopologyIoQualificationReportException>(() =>
                report.Add("FRAME_AFTER_PREFLIGHT", "MUST_NOT_TRUNCATE"));
            AssertEx.True(report.CheckpointFailed);
            AssertEx.SequenceEqual(durablePrefix, stream.ToArray());
            AssertEx.Contains(
                "FORMAT=LMC_TOPOLOGY_IO_QUALIFICATION_V2",
                Encoding.UTF8.GetString(durablePrefix));
        }

        private static void AssertRawInfoRejectedBeforeChunk(
            bool mutateRevision)
        {
            var exchangeCount = 0;
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool.RunReadOnlyRaw(
                    request =>
                    {
                        exchangeCount++;
                        if (TestFrame.ReadUInt16(request, 0)
                            != LMC_CommandId.GetEtherCATTopologyInfo)
                        {
                            throw new InvalidOperationException(
                                "A malformed topology info response reached the chunk phase.");
                        }

                        var requestId = TestFrame.ReadUInt32(
                            request,
                            LMC_Frame.HeaderSize + 4);
                        var payload = TopologyInfoPayload(requestId);
                        if (mutateRevision)
                        {
                            TestFrame.WriteUInt32(
                                payload,
                                16,
                                TopologyIoQualificationTool
                                    .ExpectedTopologyRevision + 1);
                        }
                        else
                        {
                            TestFrame.WriteUInt16(payload, 20, 8);
                            TestFrame.WriteUInt16(payload, 26, 6);
                        }

                        return TestFrame.Response(0, payload);
                    },
                    NewDryRunReport()));
            AssertEx.Equal(1, exchangeCount);
        }

        private static void AssertHealthTopologyMismatchRejected(
            uint nodeId,
            bool addDs402Data)
        {
            var canonical = CurrentTopologyCanonicalBytes();
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool.RunReadOnlyRaw(
                    request =>
                    {
                        var response = RespondToReadOnlyRequest(
                            request,
                            canonical);
                        if (TestFrame.ReadUInt16(request, 0)
                                == LMC_CommandId.ReadEtherCATNodeHealth
                            && TestFrame.ReadUInt32(
                                request,
                                LMC_Frame.HeaderSize + 12) == nodeId)
                        {
                            var flagsOffset = LMC_Frame.HeaderSize + 26;
                            var flags = TestFrame.ReadUInt16(
                                response,
                                flagsOffset);
                            if (addDs402Data)
                            {
                                flags |= (ushort)LMCEtherCATNodeHealthFlags
                                    .Ds402DataPresent;
                                TestFrame.WriteUInt32(
                                    response,
                                    LMC_Frame.HeaderSize + 56,
                                    0x1234u);
                            }
                            else
                            {
                                flags &= unchecked((ushort)~(ushort)
                                    LMCEtherCATNodeHealthFlags
                                        .Ds402DataPresent);
                                TestFrame.WriteUInt32(
                                    response,
                                    LMC_Frame.HeaderSize + 56,
                                    0);
                                TestFrame.WriteUInt32(
                                    response,
                                    LMC_Frame.HeaderSize + 60,
                                    0);
                            }

                            TestFrame.WriteUInt16(
                                response,
                                flagsOffset,
                                flags);
                        }

                        return response;
                    },
                    NewDryRunReport()));
        }

        private static void AssertDigitalIoNodeMismatchRejected(
            LMCDigitalIODirection direction)
        {
            var canonical = CurrentTopologyCanonicalBytes();
            AssertEx.Throws<InvalidDataException>(() =>
                TopologyIoQualificationTool.RunReadOnlyRaw(
                    request =>
                    {
                        var response = RespondToReadOnlyRequest(
                            request,
                            canonical);
                        if (TestFrame.ReadUInt16(request, 0)
                                == LMC_CommandId.ReadDigitalIO
                            && (LMCDigitalIODirection)request[
                                LMC_Frame.HeaderSize + 16] == direction)
                        {
                            TestFrame.WriteUInt32(
                                response,
                                LMC_Frame.HeaderSize + 24,
                                0xEC00FFFFu);
                        }

                        return response;
                    },
                    NewDryRunReport()));
        }

        private static string DescribeReadOnlySelector(byte[] request)
        {
            var command = TestFrame.ReadUInt16(request, 0);
            switch (command)
            {
                case LMC_CommandId.GetEtherCATTopologyInfo:
                    return "7E11";
                case LMC_CommandId.GetEtherCATTopologyChunk:
                    return "7E12:"
                        + TestFrame.ReadUInt16(
                            request,
                            LMC_Frame.HeaderSize + 12);
                case LMC_CommandId.ReadEtherCATNodeHealth:
                    return "7E13:"
                        + TestFrame.ReadUInt32(
                            request,
                            LMC_Frame.HeaderSize + 12).ToString("X8");
                case LMC_CommandId.ReadDigitalIO:
                    return "7E22:"
                        + TestFrame.ReadUInt32(
                            request,
                            LMC_Frame.HeaderSize + 12).ToString("X8")
                        + ":"
                        + request[LMC_Frame.HeaderSize + 16]
                        + ":"
                        + request[LMC_Frame.HeaderSize + 17];
                default:
                    throw new InvalidOperationException();
            }
        }

        private static TopologyIoQualificationReport NewDryRunReport()
        {
            return NewDryRunReport(
                TopologyIoQualificationScope.IntegratedReadOwnerDormant);
        }

        private static TopologyIoQualificationReport NewDryRunReport(
            TopologyIoQualificationScope scope)
        {
            return new TopologyIoQualificationReport(
                TopologyIoQualificationOptions.Parse(new[]
                {
                    "topology-io-qualify",
                    "--scope",
                    TopologyIoQualificationOptions.GetScopeToken(scope),
                    "--dry-run"
                }));
        }

        private static void AssertReadOnlyRequestRejected(byte[] request)
        {
            AssertEx.Throws<InvalidOperationException>(() =>
                TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                    request));
        }

        private static byte[] RespondToReadOnlyRequest(
            byte[] request,
            byte[] canonical)
        {
            return RespondToReadOnlyRequest(
                request,
                canonical,
                TopologyIoQualificationScope.IntegratedReadOwnerDormant);
        }

        private static byte[] RespondToReadOnlyRequest(
            byte[] request,
            byte[] canonical,
            TopologyIoQualificationScope scope)
        {
            TopologyIoQualificationTool.EnsureAllowedReadOnlyRequest(
                request,
                scope);
            var command = TestFrame.ReadUInt16(request, 0);
            var requestId = TestFrame.ReadUInt32(
                request,
                LMC_Frame.HeaderSize + 4);
            switch (command)
            {
                case LMC_CommandId.GetEtherCATTopologyInfo:
                    return TestFrame.Response(
                        0,
                        TopologyInfoPayload(requestId));
                case LMC_CommandId.GetEtherCATTopologyChunk:
                    return TestFrame.Response(
                        0,
                        TopologyChunkPayload(
                            requestId,
                            TestFrame.ReadUInt16(
                                request,
                                LMC_Frame.HeaderSize + 12),
                            canonical));
                case LMC_CommandId.ReadEtherCATNodeHealth:
                    return TestFrame.Response(
                        0,
                        NodeHealthPayload(
                            requestId,
                            TestFrame.ReadUInt32(
                                request,
                                LMC_Frame.HeaderSize + 12)));
                case LMC_CommandId.ReadDigitalIO:
                    return TestFrame.Response(
                        0,
                        DigitalIoPayload(
                            requestId,
                            TestFrame.ReadUInt32(
                                request,
                                LMC_Frame.HeaderSize + 12),
                            (LMCDigitalIODirection)request[
                                LMC_Frame.HeaderSize + 16],
                            request[LMC_Frame.HeaderSize + 17]));
                default:
                    throw new InvalidOperationException();
            }
        }

        private static byte[] TopologyInfoPayload(uint requestId)
        {
            var payload = CommonPayload(44, requestId);
            TestFrame.WriteUInt32(
                payload,
                16,
                TopologyIoQualificationTool.ExpectedTopologyRevision);
            TestFrame.WriteUInt16(payload, 20, 7);
            TestFrame.WriteUInt16(payload, 22, 96);
            TestFrame.WriteUInt16(payload, 24, 1);
            TestFrame.WriteUInt16(payload, 26, 5);
            TestFrame.WriteUInt16(payload, 28, 2);
            TestFrame.WriteUInt16(payload, 30, 4);
            TestFrame.WriteUInt32(payload, 32, 0x0000000Fu);
            TestFrame.WriteUInt32(payload, 36, 1);
            return payload;
        }

        private static byte[] TopologyChunkPayload(
            uint requestId,
            ushort startIndex,
            byte[] canonical)
        {
            if (startIndex >= 7)
            {
                throw new InvalidDataException();
            }

            var payload = CommonPayload(124, requestId);
            if (startIndex == 6)
            {
                TestFrame.WriteUInt16(
                    payload,
                    2,
                    (ushort)LMCDiagnosticsResponseFlags.LastChunk);
            }

            TestFrame.WriteUInt32(
                payload,
                16,
                TopologyIoQualificationTool.ExpectedTopologyRevision);
            TestFrame.WriteUInt16(payload, 20, startIndex);
            TestFrame.WriteUInt16(payload, 22, 1);
            TestFrame.WriteUInt16(payload, 24, 7);
            TestFrame.WriteUInt16(payload, 26, 96);
            Buffer.BlockCopy(
                canonical,
                startIndex * 96,
                payload,
                28,
                96);
            return payload;
        }

        private static byte[] NodeHealthPayload(
            uint requestId,
            uint nodeId)
        {
            var payload = CommonPayload(72, requestId);
            TestFrame.WriteUInt32(
                payload,
                16,
                TopologyIoQualificationTool.ExpectedTopologyRevision);
            TestFrame.WriteUInt32(payload, 20, nodeId);
            TestFrame.WriteUInt16(
                payload,
                24,
                (ushort)LMCCapturePhase.InputMapped);
            var flags = LMCEtherCATNodeHealthFlags.Configured
                | LMCEtherCATNodeHealthFlags.Detected
                | LMCEtherCATNodeHealthFlags.IdentityMatched
                | LMCEtherCATNodeHealthFlags.DataValid;
            if (nodeId >= 0xEC000101u && nodeId <= 0xEC000104u)
            {
                flags |= LMCEtherCATNodeHealthFlags.Ds402DataPresent;
            }

            TestFrame.WriteUInt16(payload, 26, (ushort)flags);
            TestFrame.WriteUInt32(payload, 28, 100);
            TestFrame.WriteUInt64(payload, 32, 100000);
            TestFrame.WriteUInt32(payload, 40, 2);
            payload[44] = 1;
            payload[45] = 8;
            TestFrame.WriteUInt32(payload, 48, 1);
            TestFrame.WriteUInt32(payload, 52, 1);
            if ((flags & LMCEtherCATNodeHealthFlags.Ds402DataPresent) != 0)
            {
                TestFrame.WriteUInt32(payload, 56, 0x1234u);
            }

            TestFrame.WriteUInt32(payload, 64, 100);
            TestFrame.WriteUInt32(payload, 68, 90);
            return payload;
        }

        private static byte[] DigitalIoPayload(
            uint requestId,
            uint ioReference,
            LMCDigitalIODirection direction,
            byte width)
        {
            var payload = CommonPayload(56, requestId);
            TestFrame.WriteUInt32(
                payload,
                16,
                TopologyIoQualificationTool.ExpectedTopologyRevision);
            TestFrame.WriteUInt32(payload, 20, ioReference);
            TestFrame.WriteUInt32(
                payload,
                24,
                direction == LMCDigitalIODirection.Input
                    ? 0xEC010001u
                    : 0xEC010002u);
            payload[28] = (byte)direction;
            payload[29] = width;
            TestFrame.WriteUInt16(
                payload,
                30,
                (ushort)LMCDigitalIOStatusFlags.Valid);
            TestFrame.WriteUInt64(payload, 32, 0xA5A55A5Au);
            TestFrame.WriteUInt64(payload, 40, 0xFFFFFFFFu);
            TestFrame.WriteUInt32(payload, 48, 100);
            if (direction == LMCDigitalIODirection.Output)
            {
                TestFrame.WriteUInt32(payload, 52, 1);
            }

            return payload;
        }

        private static byte[] CurrentTopologyCanonicalBytes()
        {
            var entries =
                TopologyIoQualificationTool.ExpectedCurrentTopologyEntries();
            var canonical = new byte[entries.Length * 96];
            for (var index = 0; index < entries.Length; index++)
            {
                var entry = entries[index];
                var offset = index * 96;
                TestFrame.WriteUInt32(canonical, offset, entry.NodeId);
                TestFrame.WriteUInt32(
                    canonical,
                    offset + 4,
                    entry.ParentNodeId);
                TestFrame.WriteUInt16(
                    canonical,
                    offset + 8,
                    entry.TopologyIndex);
                TestFrame.WriteUInt16(
                    canonical,
                    offset + 10,
                    entry.MasterSlaveIndex);
                canonical[offset + 12] = (byte)entry.NodeKind;
                TestFrame.WriteUInt16(
                    canonical,
                    offset + 14,
                    (ushort)entry.NodeFlags);
                TestFrame.WriteUInt16(
                    canonical,
                    offset + 16,
                    entry.SdoSlaveReference);
                TestFrame.WriteUInt16(
                    canonical,
                    offset + 18,
                    entry.PhysicalAxisReference);
                TestFrame.WriteUInt16(
                    canonical,
                    offset + 20,
                    entry.SlotIndex);
                TestFrame.WriteUInt32(canonical, offset + 24, entry.VendorId);
                TestFrame.WriteUInt32(
                    canonical,
                    offset + 28,
                    entry.ProductCode);
                TestFrame.WriteUInt32(
                    canonical,
                    offset + 32,
                    entry.RevisionNumber);
                TestFrame.WriteUInt32(
                    canonical,
                    offset + 36,
                    entry.SerialNumber);
                TestFrame.WriteUInt16(
                    canonical,
                    offset + 40,
                    entry.InputBytes);
                TestFrame.WriteUInt16(
                    canonical,
                    offset + 42,
                    entry.OutputBytes);
                var name = Encoding.ASCII.GetBytes(entry.Name);
                Buffer.BlockCopy(name, 0, canonical, offset + 44, name.Length);
                TestFrame.WriteUInt32(
                    canonical,
                    offset + 92,
                    entry.IOReference);
            }

            AssertEx.Equal(
                TopologyIoQualificationTool.ExpectedTopologyRevision,
                LMC_DiagnosticsParser.ComputeEtherCATTopologyRevision(entries));
            return canonical;
        }

        private static byte[] CommonPayload(int length, uint requestId)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static LMCDiagnosticCapabilities Capabilities(
            LMCDiagnosticCapability capability,
            uint bootId,
            ushort maxRequestPayloadBytes,
            ushort maxResponsePayloadBytes,
            uint mapRevision =
                TopologyIoQualificationTool.ExpectedMapRevision,
            uint diagnosticsBuild = 1)
        {
            return new LMCDiagnosticCapabilities(
                null,
                1,
                diagnosticsBuild,
                (uint)capability,
                mapRevision,
                0,
                32,
                8,
                1,
                100,
                1000,
                maxRequestPayloadBytes,
                maxResponsePayloadBytes,
                1280,
                80,
                16,
                1000,
                4,
                bootId);
        }

        private sealed class FailingAppendStream : Stream
        {
            private readonly MemoryStream inner = new MemoryStream();
            private bool failNextWrite;

            internal void FailNextWrite()
            {
                failNextWrite = true;
            }

            internal byte[] ToArray()
            {
                return inner.ToArray();
            }

            public override bool CanRead { get { return false; } }
            public override bool CanSeek { get { return true; } }
            public override bool CanWrite { get { return true; } }
            public override long Length { get { return inner.Length; } }

            public override long Position
            {
                get { return inner.Position; }
                set { inner.Position = value; }
            }

            public override void Flush()
            {
                inner.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return inner.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                inner.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (failNextWrite)
                {
                    failNextWrite = false;
                    throw new IOException("Injected append failure.");
                }

                inner.Write(buffer, offset, count);
            }
        }
    }
}
