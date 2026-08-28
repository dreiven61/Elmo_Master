from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
JOURNAL = ROOT / 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/DiagnosticsMutationJournal.cs'
MAIN = ROOT / 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs'
TESTS = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsMutationJournalTests.cs'
DESIGN = ROOT / 'docs/api/design/SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md'
VERIFY = ROOT / 'tools/Verify-SdoR05FullIdentity.ps1'


def replace_once(path, old, new, label):
    text = path.read_text(encoding='utf-8')
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f'{label}: expected exactly one anchor, found {count}')
    path.write_text(text.replace(old, new, 1), encoding='utf-8')


def replace_all(path, old, new, minimum, label):
    text = path.read_text(encoding='utf-8')
    count = text.count(old)
    if count < minimum:
        raise RuntimeError(f'{label}: expected at least {minimum} anchors, found {count}')
    path.write_text(text.replace(old, new), encoding='utf-8')


replace_once(JOURNAL, 'using System.IO;\n', 'using System.IO;\nusing System.Net;\n', 'System.Net import')

old_ctor = '''        internal DiagnosticsSdoWriteMutationMetadata(
            ushort slaveReference,
            ushort objectIndex,
            byte subIndex,
            LMCSignalValueType valueType,
            ushort dataLength,
            uint timeoutCycles,
            byte[] expectedWriteData)
        {
            if (slaveReference < 1 || slaveReference > 4)
            {
                throw new ArgumentOutOfRangeException(
                    "slaveReference",
                    "Durable SDO recovery supports SlaveReference 1 through 4 only.");
            }

            if (objectIndex == 0 || IsPermanentlyUnsafeObject(objectIndex))
            {
                throw new ArgumentOutOfRangeException(
                    "objectIndex",
                    "Durable SDO recovery cannot target a direct motion/control object.");
            }

            var expectedDataLength = GetCanonicalScalarDataLength(
                valueType);
            if (dataLength != expectedDataLength)
            {
                throw new ArgumentOutOfRangeException(
                    "dataLength",
                    "Durable SDO recovery requires canonical scalar length: 8-bit=1, 16-bit=2, 32-bit=4.");
            }

            if (timeoutCycles < 1 || timeoutCycles > 60000)
            {
                throw new ArgumentOutOfRangeException(
                    "timeoutCycles",
                    "Durable SDO recovery requires TimeoutCycles from 1 through 60000.");
            }

            if (expectedWriteData == null
                || expectedWriteData.Length != dataLength)
            {
                throw new ArgumentException(
                    "Expected SDO Write data must exactly match DataLength.",
                    "expectedWriteData");
            }

            SlaveReference = slaveReference;
            ObjectIndex = objectIndex;
            SubIndex = subIndex;
            ValueType = valueType;
            DataLength = dataLength;
            TimeoutCycles = timeoutCycles;
            this.expectedWriteData = (byte[])expectedWriteData.Clone();
        }

        internal ushort SlaveReference { get; private set; }
        internal ushort ObjectIndex { get; private set; }
        internal byte SubIndex { get; private set; }
        internal LMCSignalValueType ValueType { get; private set; }
        internal ushort DataLength { get; private set; }
        internal uint TimeoutCycles { get; private set; }
        internal byte[] ExpectedWriteData
        {
            get { return (byte[])expectedWriteData.Clone(); }
        }
'''
new_ctor = '''        internal DiagnosticsSdoWriteMutationMetadata(
            ushort slaveReference,
            ushort objectIndex,
            byte subIndex,
            LMCSignalValueType valueType,
            ushort dataLength,
            uint timeoutCycles,
            byte[] expectedWriteData)
            : this(
                slaveReference,
                objectIndex,
                subIndex,
                valueType,
                dataLength,
                timeoutCycles,
                null,
                0,
                0,
                expectedWriteData,
                true)
        {
        }

        internal DiagnosticsSdoWriteMutationMetadata(
            ushort slaveReference,
            ushort objectIndex,
            byte subIndex,
            LMCSignalValueType valueType,
            ushort dataLength,
            uint timeoutCycles,
            string endpointIp,
            int endpointPort,
            uint diagnosticsBuild,
            byte[] expectedWriteData)
            : this(
                slaveReference,
                objectIndex,
                subIndex,
                valueType,
                dataLength,
                timeoutCycles,
                endpointIp,
                endpointPort,
                diagnosticsBuild,
                expectedWriteData,
                false)
        {
        }

        private DiagnosticsSdoWriteMutationMetadata(
            ushort slaveReference,
            ushort objectIndex,
            byte subIndex,
            LMCSignalValueType valueType,
            ushort dataLength,
            uint timeoutCycles,
            string endpointIp,
            int endpointPort,
            uint diagnosticsBuild,
            byte[] expectedWriteData,
            bool allowLegacyIdentity)
        {
            if (slaveReference < 1 || slaveReference > 4)
            {
                throw new ArgumentOutOfRangeException(
                    "slaveReference",
                    "Durable SDO recovery supports SlaveReference 1 through 4 only.");
            }

            if (objectIndex == 0 || IsPermanentlyUnsafeObject(objectIndex))
            {
                throw new ArgumentOutOfRangeException(
                    "objectIndex",
                    "Durable SDO recovery cannot target a direct motion/control object.");
            }

            var expectedDataLength = GetCanonicalScalarDataLength(
                valueType);
            if (dataLength != expectedDataLength)
            {
                throw new ArgumentOutOfRangeException(
                    "dataLength",
                    "Durable SDO recovery requires canonical scalar length: 8-bit=1, 16-bit=2, 32-bit=4.");
            }

            if (timeoutCycles < 1 || timeoutCycles > 60000)
            {
                throw new ArgumentOutOfRangeException(
                    "timeoutCycles",
                    "Durable SDO recovery requires TimeoutCycles from 1 through 60000.");
            }

            if (expectedWriteData == null
                || expectedWriteData.Length != dataLength)
            {
                throw new ArgumentException(
                    "Expected SDO Write data must exactly match DataLength.",
                    "expectedWriteData");
            }

            if (allowLegacyIdentity)
            {
                if (endpointIp != null || endpointPort != 0 || diagnosticsBuild != 0)
                {
                    throw new ArgumentException(
                        "Legacy SDO durable identity must be completely absent.",
                        "endpointIp");
                }
            }
            else
            {
                endpointIp = NormalizeEndpointIp(endpointIp);
                if (endpointPort < 1 || endpointPort > 65535)
                {
                    throw new ArgumentOutOfRangeException("endpointPort");
                }
                if (diagnosticsBuild == 0)
                {
                    throw new ArgumentOutOfRangeException("diagnosticsBuild");
                }
            }

            SlaveReference = slaveReference;
            ObjectIndex = objectIndex;
            SubIndex = subIndex;
            ValueType = valueType;
            DataLength = dataLength;
            TimeoutCycles = timeoutCycles;
            EndpointIp = endpointIp;
            EndpointPort = endpointPort;
            DiagnosticsBuild = diagnosticsBuild;
            this.expectedWriteData = (byte[])expectedWriteData.Clone();
        }

        internal ushort SlaveReference { get; private set; }
        internal ushort ObjectIndex { get; private set; }
        internal byte SubIndex { get; private set; }
        internal LMCSignalValueType ValueType { get; private set; }
        internal ushort DataLength { get; private set; }
        internal uint TimeoutCycles { get; private set; }
        internal string EndpointIp { get; private set; }
        internal int EndpointPort { get; private set; }
        internal uint DiagnosticsBuild { get; private set; }
        internal bool HasFullDurableIdentity
        {
            get
            {
                return !string.IsNullOrEmpty(EndpointIp)
                    && EndpointPort > 0
                    && DiagnosticsBuild != 0;
            }
        }
        internal byte[] ExpectedWriteData
        {
            get { return (byte[])expectedWriteData.Clone(); }
        }

        private static string NormalizeEndpointIp(string endpointIp)
        {
            IPAddress parsed;
            if (string.IsNullOrWhiteSpace(endpointIp)
                || !IPAddress.TryParse(endpointIp, out parsed))
            {
                throw new ArgumentException(
                    "Durable SDO recovery requires a canonical IP endpoint.",
                    "endpointIp");
            }
            return parsed.ToString();
        }
'''
replace_once(JOURNAL, old_ctor, new_ctor, 'full durable SDO metadata identity')

old_caps = '''        internal DiagnosticsSdoRestartRecoveryCapabilities(
            uint diagnosticsBootId,
            uint mapRevision,
            bool supportsSdoRead,
            bool supportsGeneralInlineSdoRead,
            ushort maxSdoDataBytes)
        {
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId");
            }

            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("mapRevision");
            }

            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            SupportsSdoRead = supportsSdoRead;
            SupportsGeneralInlineSdoRead =
                supportsGeneralInlineSdoRead;
            MaxSdoDataBytes = maxSdoDataBytes;
        }

        internal uint DiagnosticsBootId { get; private set; }
        internal uint MapRevision { get; private set; }
'''
new_caps = '''        internal DiagnosticsSdoRestartRecoveryCapabilities(
            string endpointIp,
            int endpointPort,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            bool supportsSdoRead,
            bool supportsGeneralInlineSdoRead,
            ushort maxSdoDataBytes)
        {
            IPAddress parsedEndpoint;
            if (string.IsNullOrWhiteSpace(endpointIp)
                || !IPAddress.TryParse(endpointIp, out parsedEndpoint))
            {
                throw new ArgumentException("endpointIp");
            }
            if (endpointPort < 1 || endpointPort > 65535)
            {
                throw new ArgumentOutOfRangeException("endpointPort");
            }
            if (diagnosticsBuild == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBuild");
            }
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId");
            }

            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("mapRevision");
            }

            EndpointIp = parsedEndpoint.ToString();
            EndpointPort = endpointPort;
            DiagnosticsBuild = diagnosticsBuild;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            SupportsSdoRead = supportsSdoRead;
            SupportsGeneralInlineSdoRead =
                supportsGeneralInlineSdoRead;
            MaxSdoDataBytes = maxSdoDataBytes;
        }

        internal string EndpointIp { get; private set; }
        internal int EndpointPort { get; private set; }
        internal uint DiagnosticsBuild { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint MapRevision { get; private set; }
'''
replace_once(JOURNAL, old_caps, new_caps, 'restart capability full identity')

replace_once(
    JOURNAL,
    '''                && record.HasTypedSdoWriteMetadata
                && recoveredAtStartup''',
    '''                && record.HasTypedSdoWriteMetadata
                && record.SdoWriteMetadata.HasFullDurableIdentity
                && recoveredAtStartup''',
    'legacy v1/v2 automatic recovery fail closed')

replace_once(
    JOURNAL,
    '''            return capabilities != null
                && capabilities.DiagnosticsBootId
                    == record.DiagnosticsBootId
                && capabilities.MapRevision == record.IdentityRevision;''',
    '''            var metadata = record == null
                ? null
                : record.SdoWriteMetadata;
            return capabilities != null
                && metadata != null
                && metadata.HasFullDurableIdentity
                && string.Equals(
                    capabilities.EndpointIp,
                    metadata.EndpointIp,
                    StringComparison.Ordinal)
                && capabilities.EndpointPort == metadata.EndpointPort
                && capabilities.DiagnosticsBuild == metadata.DiagnosticsBuild
                && capabilities.DiagnosticsBootId
                    == record.DiagnosticsBootId
                && capabilities.MapRevision == record.IdentityRevision;''',
    'restart exact full identity match')

replace_once(
    JOURNAL,
    '''        private const int LegacyFormatVersion = 1;
        private const int FormatVersion = 2;''',
    '''        private const int LegacyFormatVersion = 1;
        private const int TypedSdoFormatVersion = 2;
        private const int FormatVersion = 3;''',
    'journal v3 format constants')

replace_once(
    JOURNAL,
    '''                && left.TimeoutCycles == right.TimeoutCycles
                && ByteArraysEqual(''',
    '''                && left.TimeoutCycles == right.TimeoutCycles
                && left.HasFullDurableIdentity
                    == right.HasFullDurableIdentity
                && string.Equals(
                    left.EndpointIp,
                    right.EndpointIp,
                    StringComparison.Ordinal)
                && left.EndpointPort == right.EndpointPort
                && left.DiagnosticsBuild == right.DiagnosticsBuild
                && ByteArraysEqual(''',
    'metadata durable identity equality')

replace_once(
    JOURNAL,
    '''                if (version != LegacyFormatVersion
                    && version != FormatVersion)''',
    '''                if (version != LegacyFormatVersion
                    && version != TypedSdoFormatVersion
                    && version != FormatVersion)''',
    'v1-v3 reader compatibility')

replace_once(
    JOURNAL,
    '''                var sdoWriteMetadata = version >= FormatVersion
                    ? ReadSdoWriteMetadata(reader)
                    : null;''',
    '''                var sdoWriteMetadata = version >= TypedSdoFormatVersion
                    ? ReadSdoWriteMetadata(reader, version)
                    : null;''',
    'v2-v3 typed metadata decode')

replace_once(
    JOURNAL,
    '''            writer.Write(expectedData.Length);
            writer.Write(expectedData);
        }

        private static DiagnosticsSdoWriteMutationMetadata
            ReadSdoWriteMetadata(BinaryReader reader)''',
    '''            writer.Write(expectedData.Length);
            writer.Write(expectedData);
            writer.Write(metadata.HasFullDurableIdentity);
            if (metadata.HasFullDurableIdentity)
            {
                WriteText(writer, metadata.EndpointIp);
                writer.Write(metadata.EndpointPort);
                writer.Write(metadata.DiagnosticsBuild);
            }
        }

        private static DiagnosticsSdoWriteMutationMetadata
            ReadSdoWriteMetadata(BinaryReader reader, int version)''',
    'v3 metadata durable identity serialization')

replace_once(
    JOURNAL,
    '''            return new DiagnosticsSdoWriteMutationMetadata(
                slaveReference,
                objectIndex,
                subIndex,
                valueType,
                dataLength,
                timeoutCycles,
                expectedData);
        }''',
    '''            if (version < FormatVersion)
            {
                return new DiagnosticsSdoWriteMutationMetadata(
                    slaveReference,
                    objectIndex,
                    subIndex,
                    valueType,
                    dataLength,
                    timeoutCycles,
                    expectedData);
            }

            var durableIdentityMarker = reader.ReadByte();
            if (durableIdentityMarker == 0)
            {
                return new DiagnosticsSdoWriteMutationMetadata(
                    slaveReference,
                    objectIndex,
                    subIndex,
                    valueType,
                    dataLength,
                    timeoutCycles,
                    expectedData);
            }
            if (durableIdentityMarker != 1)
            {
                throw new InvalidDataException(
                    "Diagnostics mutation durable identity marker is non-canonical.");
            }

            var endpointIp = ReadText(reader);
            var endpointPort = reader.ReadInt32();
            var diagnosticsBuild = reader.ReadUInt32();
            return new DiagnosticsSdoWriteMutationMetadata(
                slaveReference,
                objectIndex,
                subIndex,
                valueType,
                dataLength,
                timeoutCycles,
                endpointIp,
                endpointPort,
                diagnosticsBuild,
                expectedData);
        }''',
    'v2 legacy / v3 full identity decode')

# MainWindow: ensure current-session full identity is captured before arm.
replace_once(
    MAIN,
    '''            EnsureDiagnosticsMutationJournalCanArm("SDO Write");
            try
            {
                diagnosticsMutationJournal.Arm(''',
    '''            EnsureDiagnosticsMutationJournalCanArm("SDO Write");
            var capabilities = diagnosticCapabilities;
            if (capabilities == null
                || capabilities.DiagnosticsBuild == 0
                || capabilities.DiagnosticsBootId != diagnosticsBootId
                || capabilities.MapRevision != mapRevision
                || !capabilities.IsBoundTo(
                    ownerConnection.Diagnostics,
                    ownerConnection.SessionGeneration))
            {
                throw new InvalidOperationException(
                    "SDO Write durable arm requires fresh current-session DiagnosticsBuild/BootId/MapRevision evidence.");
            }
            var durableEndpointIp = RequiredConnectedRemoteIp();
            var durableEndpointPort = RequiredConnectedRemotePort();
            try
            {
                diagnosticsMutationJournal.Arm(''',
    'capture full durable identity before SDO arm')

replace_once(
    MAIN,
    '''                        request.DataLength,
                        request.TimeoutCycles,
                        request.WriteData));''',
    '''                        request.DataLength,
                        request.TimeoutCycles,
                        durableEndpointIp,
                        durableEndpointPort,
                        capabilities.DiagnosticsBuild,
                        request.WriteData));''',
    'persist endpoint/build in SDO metadata')

replace_once(
    MAIN,
    '''                                DiagnosticsSdoRestartRecoveryCapabilities(
                                    observedCapabilities.DiagnosticsBootId,
                                    observedCapabilities.MapRevision,''',
    '''                                DiagnosticsSdoRestartRecoveryCapabilities(
                                    RequiredConnectedRemoteIp(),
                                    RequiredConnectedRemotePort(),
                                    observedCapabilities.DiagnosticsBuild,
                                    observedCapabilities.DiagnosticsBootId,
                                    observedCapabilities.MapRevision,''',
    'restart full identity capability snapshot')

replace_once(
    MAIN,
    '''                + ", BootId=0x"
                + record.DiagnosticsBootId.ToString("X8")''',
    '''                + (record.SdoWriteMetadata != null
                    && record.SdoWriteMetadata.HasFullDurableIdentity
                    ? ", Endpoint="
                        + record.SdoWriteMetadata.EndpointIp
                        + ":"
                        + record.SdoWriteMetadata.EndpointPort.ToString(
                            CultureInfo.InvariantCulture)
                        + ", DiagnosticsBuild=0x"
                        + record.SdoWriteMetadata.DiagnosticsBuild.ToString("X8")
                    : record.Kind == DiagnosticsMutationKind.SdoWrite
                        ? ", DurableIdentity=LEGACY_INCOMPLETE"
                        : string.Empty)
                + ", BootId=0x"
                + record.DiagnosticsBootId.ToString("X8")''',
    'format durable endpoint/build evidence')

# Tests: current durable writer is v3 and includes full endpoint/build identity.
replace_once(
    TESTS,
    '''        private const uint BootId = 0x12345678u;
        private const uint SdoMapRevision = 0x10203040u;''',
    '''        private const string SdoEndpointIp = "127.0.0.1";
        private const int SdoEndpointPort = 7010;
        private const uint DiagnosticsBuild = 0x01020304u;
        private const uint BootId = 0x12345678u;
        private const uint SdoMapRevision = 0x10203040u;''',
    'test durable identity constants')

replace_all(TESTS, 'TypedSdoV2RoundTripIsImmutable', 'TypedSdoV3RoundTripIsImmutable', 2, 'rename current format test')
replace_all(TESTS, 'NonCanonicalV2MetadataMarkerFailsClosed', 'NonCanonicalV3MetadataMarkerFailsClosed', 2, 'rename current marker test')
replace_all(TESTS, 'FindV2MetadataMarkerOffset', 'FindV3MetadataMarkerOffset', 2, 'rename marker helper')

replace_once(
    TESTS,
    '''            tests.Add(
                "Qualification.MutationJournal.LegacyV1RecoveryIsZeroWire",
                LegacyV1RecoveryIsZeroWire);''',
    '''            tests.Add(
                "Qualification.MutationJournal.LegacyV1RecoveryIsZeroWire",
                LegacyV1RecoveryIsZeroWire);
            tests.Add(
                "Qualification.MutationJournal.LegacyV2TypedRecoveryIsZeroWire",
                LegacyV2TypedRecoveryIsZeroWire);''',
    'register legacy v2 zero-wire test')

replace_once(
    TESTS,
    '''            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryIdentityMismatchDoesNotRead",
                RestartRecoveryIdentityMismatchDoesNotRead);''',
    '''            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryIdentityMismatchDoesNotRead",
                RestartRecoveryIdentityMismatchDoesNotRead);
            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryEndpointMismatchDoesNotRead",
                RestartRecoveryEndpointMismatchDoesNotRead);
            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryBuildMismatchDoesNotRead",
                RestartRecoveryBuildMismatchDoesNotRead);''',
    'register full identity mismatch tests')

replace_once(
    TESTS,
    '''                        AssertEx.Equal(
                            2,
                            BitConverter.ToInt32(encoded, 8),
                            "New durable records must use journal format v2.");''',
    '''                        AssertEx.Equal(
                            3,
                            BitConverter.ToInt32(encoded, 8),
                            "New durable SDO records must use journal format v3.");''',
    'v3 format assertion')

replace_once(
    TESTS,
    '''                        AssertEx.Equal((uint)1000, metadata.TimeoutCycles);
                        AssertEx.SequenceEqual(''',
    '''                        AssertEx.Equal((uint)1000, metadata.TimeoutCycles);
                        AssertEx.True(metadata.HasFullDurableIdentity);
                        AssertEx.Equal(SdoEndpointIp, metadata.EndpointIp);
                        AssertEx.Equal(SdoEndpointPort, metadata.EndpointPort);
                        AssertEx.Equal(DiagnosticsBuild, metadata.DiagnosticsBuild);
                        AssertEx.SequenceEqual(''',
    'v3 identity roundtrip assertions')

# The marker helper now expects v3.
replace_once(TESTS, 'AssertEx.Equal(2, reader.ReadInt32());', 'AssertEx.Equal(3, reader.ReadInt32());', 'v3 marker helper header')

# Insert legacy-v2 reader/recovery proof before OutcomeUnverified test.
replace_once(
    TESTS,
    '''        private static void OutcomeUnverifiedCanBecomeReadbackMismatch()
        {''',
    '''        private static void LegacyV2TypedRecoveryIsZeroWire()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var identity = Guid.NewGuid();
                    var createdUtc = DateTime.UtcNow;
                    var journalPath = Path.Combine(
                        directoryPath,
                        DiagnosticsMutationJournal.JournalFileName);
                    WriteLegacyV2TypedSdoRecord(
                        journalPath,
                        identity,
                        createdUtc,
                        77);
                    var persistedBefore = File.ReadAllBytes(journalPath);
                    var recoverabilityCalls = 0;
                    var capabilityCalls = 0;
                    var readCalls = 0;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.True(journal.CurrentRecord.HasTypedSdoWriteMetadata);
                        AssertEx.False(
                            journal.CurrentRecord.SdoWriteMetadata
                                .HasFullDurableIdentity);
                        var result = DiagnosticsSdoRestartRecoveryOrchestrator
                            .TryRecoverAsync(
                                journal,
                                true,
                                true,
                                true,
                                false,
                                false,
                                false,
                                metadata =>
                                {
                                    recoverabilityCalls++;
                                    return true;
                                },
                                () =>
                                {
                                    capabilityCalls++;
                                    return Task.FromResult(
                                        CreateRecoveryCapabilities());
                                },
                                metadata =>
                                {
                                    readCalls++;
                                    return Task.FromResult(
                                        new byte[] { 1, 0, 0, 0 });
                                })
                            .GetAwaiter()
                            .GetResult();
                        AssertEx.Equal(
                            DiagnosticsSdoRestartRecoveryDisposition.NotEligible,
                            result.Disposition);
                        AssertEx.Equal(0, recoverabilityCalls);
                        AssertEx.Equal(0, capabilityCalls);
                        AssertEx.Equal(0, readCalls);
                    }
                    AssertEx.SequenceEqual(
                        persistedBefore,
                        File.ReadAllBytes(journalPath),
                        "Legacy v2 exact recovery must remain zero-wire and byte-preserving.");
                });
        }

        private static void OutcomeUnverifiedCanBecomeReadbackMismatch()
        {''',
    'legacy v2 full-identity fail-closed test')

# Insert endpoint/build mismatch tests before capability-state-change test.
replace_once(
    TESTS,
    '''        private static void
            RestartRecoveryCapabilityStateChangeDoesNotReadOrCommit()
        {''',
    '''        private static void RestartRecoveryEndpointMismatchDoesNotRead()
        {
            AssertFullIdentityMismatchDoesNotRead(
                "127.0.0.2",
                SdoEndpointPort,
                DiagnosticsBuild,
                BootId,
                SdoMapRevision,
                "endpoint");
        }

        private static void RestartRecoveryBuildMismatchDoesNotRead()
        {
            AssertFullIdentityMismatchDoesNotRead(
                SdoEndpointIp,
                SdoEndpointPort,
                DiagnosticsBuild + 1,
                BootId,
                SdoMapRevision,
                "DiagnosticsBuild");
        }

        private static void AssertFullIdentityMismatchDoesNotRead(
            string endpointIp,
            int endpointPort,
            uint diagnosticsBuild,
            uint bootId,
            uint mapRevision,
            string mismatchLabel)
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var capabilityCalls = 0;
                    var readCalls = 0;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmTypedTerminalSdo(
                            journal,
                            Guid.NewGuid(),
                            DateTime.UtcNow,
                            new byte[] { 1, 0, 0, 0 });
                        var result = DiagnosticsSdoRestartRecoveryOrchestrator
                            .TryRecoverAsync(
                                journal,
                                true,
                                true,
                                true,
                                false,
                                false,
                                false,
                                metadata => true,
                                () =>
                                {
                                    capabilityCalls++;
                                    return Task.FromResult(
                                        new DiagnosticsSdoRestartRecoveryCapabilities(
                                            endpointIp,
                                            endpointPort,
                                            diagnosticsBuild,
                                            bootId,
                                            mapRevision,
                                            true,
                                            true,
                                            4));
                                },
                                metadata =>
                                {
                                    readCalls++;
                                    return Task.FromResult(
                                        new byte[] { 1, 0, 0, 0 });
                                })
                            .GetAwaiter()
                            .GetResult();
                        AssertEx.Equal(
                            DiagnosticsSdoRestartRecoveryDisposition.IdentityMismatch,
                            result.Disposition,
                            mismatchLabel + " mismatch must fail before exact SDO Read.");
                        AssertEx.Equal(1, capabilityCalls);
                        AssertEx.Equal(0, readCalls);
                    }
                });
        }

        private static void
            RestartRecoveryCapabilityStateChangeDoesNotReadOrCommit()
        {''',
    'full endpoint/build mismatch tests')

# Full current metadata constructor in test helpers.
replace_once(
    TESTS,
    '''                valueType,
                dataLength,
                1000,
                expectedWriteData);''',
    '''                valueType,
                dataLength,
                1000,
                SdoEndpointIp,
                SdoEndpointPort,
                DiagnosticsBuild,
                expectedWriteData);''',
    'generic scalar test full identity')

replace_once(
    TESTS,
    '''                LMCSignalValueType.Int32,
                4,
                1000,
                expectedWriteData);''',
    '''                LMCSignalValueType.Int32,
                4,
                1000,
                SdoEndpointIp,
                SdoEndpointPort,
                DiagnosticsBuild,
                expectedWriteData);''',
    'standard SDO test full identity')

# Recovery capability helper now emits full current context.
replace_once(
    TESTS,
    '''            return new DiagnosticsSdoRestartRecoveryCapabilities(
                bootId,
                mapRevision,
                true,
                true,
                4);''',
    '''            return new DiagnosticsSdoRestartRecoveryCapabilities(
                SdoEndpointIp,
                SdoEndpointPort,
                DiagnosticsBuild,
                bootId,
                mapRevision,
                true,
                true,
                4);''',
    'test recovery capabilities full identity')

# Add legacy v2 fixture writer before legacy text helper.
replace_once(
    TESTS,
    '''        private static void WriteLegacyText(
            BinaryWriter writer,
            string value)''',
    '''        private static void WriteLegacyV2TypedSdoRecord(
            string path,
            Guid identity,
            DateTime createdUtc,
            uint ticketId)
        {
            byte[] payload;
            using (var payloadStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(
                    payloadStream,
                    Encoding.UTF8,
                    true))
                {
                    writer.Write(identity.ToByteArray());
                    writer.Write((int)DiagnosticsMutationKind.SdoWrite);
                    writer.Write((int)DiagnosticsMutationState
                        .TerminalSuccessPendingReadback);
                    writer.Write(createdUtc.Ticks);
                    writer.Write(createdUtc.AddMilliseconds(2).Ticks);
                    writer.Write(BootId);
                    writer.Write(SdoMapRevision);
                    writer.Write(SessionGeneration);
                    writer.Write(ticketId);
                    WriteLegacyText(
                        writer,
                        "Slave=1,Object=0x2F00,SubIndex=24");
                    WriteLegacyText(writer, "WriteData=01-00-00-00");
                    writer.Write(true);
                    writer.Write((ushort)1);
                    writer.Write((ushort)0x2F00);
                    writer.Write((byte)24);
                    writer.Write((int)LMCSignalValueType.Int32);
                    writer.Write((ushort)4);
                    writer.Write((uint)1000);
                    writer.Write(4);
                    writer.Write(new byte[] { 1, 0, 0, 0 });
                    writer.Flush();
                }
                payload = payloadStream.ToArray();
            }
            WriteLegacyJournal(path, 2, payload);
        }

        private static void WriteLegacyJournal(
            string path,
            int version,
            byte[] payload)
        {
            byte[] prefix;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(
                    stream,
                    Encoding.UTF8,
                    true))
                {
                    writer.Write(Encoding.ASCII.GetBytes("ELMODMJ1"));
                    writer.Write(version);
                    writer.Write(payload.Length);
                    writer.Write(payload);
                    writer.Flush();
                }
                prefix = stream.ToArray();
            }
            byte[] checksum;
            using (var sha256 = SHA256.Create())
            {
                checksum = sha256.ComputeHash(prefix);
            }
            var bytes = new byte[prefix.Length + checksum.Length];
            Buffer.BlockCopy(prefix, 0, bytes, 0, prefix.Length);
            Buffer.BlockCopy(checksum, 0, bytes, prefix.Length, checksum.Length);
            File.WriteAllBytes(path, bytes);
        }

        private static void WriteLegacyText(
            BinaryWriter writer,
            string value)''',
    'legacy v2 fixture writer')

# Design: mark R05 invariants that this implementation and pre-existing no-replay machinery now prove.
replace_once(
    DESIGN,
    '''- [ ] wire dispatch 가능성 이후 original Write automatic replay 0회
- [ ] reconnect는 ticket status 또는 exact target Read만 수행
- [ ] exact identity mismatch -> zero-wire
- [ ] terminal success라도 readback 전 mutation block 유지
- [ ] unresolved record startup recovery
- [ ] tamper/corrupt journal fail-closed

2026-08-28 current-dev R05-A:''',
    '''- [x] wire dispatch 가능성 이후 original Write automatic replay 0회
- [x] reconnect는 ticket status 또는 exact target Read만 수행
- [x] exact identity mismatch -> zero-wire
- [x] terminal success라도 readback 전 mutation block 유지
- [x] unresolved record startup recovery
- [x] tamper/corrupt journal fail-closed

2026-08-28 current-dev R05-A:''',
    'R05 invariant completion')

replace_once(
    DESIGN,
    '''다음 R05-B에서 journal v3에 Endpoint IP/port + DiagnosticsBuild를 추가하고 v1/v2 legacy record를 full-identity recovery에서 fail-closed 처리한다.''',
    '''R05-B에서는 journal format을 v3로 올려 SDO metadata에 Endpoint IP/port + DiagnosticsBuild를 함께 영속화했다. 새 SDO Write durable arm은 fresh current-session DiagnosticsBuild/BootId/MapRevision과 connected endpoint를 캡처하며, restart exact-read는 Endpoint + DiagnosticsBuild + BootId + MapRevision이 모두 일치할 때만 wire를 허용한다. v1/v2 record는 계속 읽을 수 있지만 full durable identity가 없으므로 automatic exact-read recovery는 zero-wire `NotEligible`이다.''',
    'R05-B design completion note')

VERIFY.write_text(r'''param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$journalPath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\DiagnosticsMutationJournal.cs'
$mainPath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.MutationJournal.cs'
$testPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\DiagnosticsMutationJournalTests.cs'
$designPath = Join-Path $root 'docs\api\design\SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md'
$journal = Get-Content -LiteralPath $journalPath -Raw
$main = Get-Content -LiteralPath $mainPath -Raw
$tests = Get-Content -LiteralPath $testPath -Raw
$design = Get-Content -LiteralPath $designPath -Raw
function Require-Text([string]$Text, [string]$Needle, [string]$Label) {
    if ($Text.IndexOf($Needle, [StringComparison]::Ordinal) -lt 0) {
        throw "Missing ${Label}: $Needle"
    }
}
Require-Text $journal 'private const int TypedSdoFormatVersion = 2;' 'v2 compatibility'
Require-Text $journal 'private const int FormatVersion = 3;' 'v3 writer'
Require-Text $journal 'HasFullDurableIdentity' 'full identity marker'
Require-Text $journal 'EndpointIp' 'endpoint identity'
Require-Text $journal 'DiagnosticsBuild' 'build identity'
Require-Text $journal 'version != TypedSdoFormatVersion' 'v2 reader support'
Require-Text $main 'durableEndpointIp = RequiredConnectedRemoteIp()' 'arm endpoint capture'
Require-Text $main 'capabilities.DiagnosticsBuild' 'arm build capture'
Require-Text $main 'RequiredConnectedRemotePort()' 'restart endpoint capture'
Require-Text $tests 'LegacyV2TypedRecoveryIsZeroWire' 'legacy v2 zero-wire test'
Require-Text $tests 'RestartRecoveryEndpointMismatchDoesNotRead' 'endpoint mismatch test'
Require-Text $tests 'RestartRecoveryBuildMismatchDoesNotRead' 'build mismatch test'
Require-Text $design 'R05-B에서는 journal format을 v3' 'R05-B design sync'
Write-Host 'PASS SDO-R05-B full durable identity source contract.'
''', encoding='utf-8')

print('SDO-R05-B full durable identity patch applied.')
