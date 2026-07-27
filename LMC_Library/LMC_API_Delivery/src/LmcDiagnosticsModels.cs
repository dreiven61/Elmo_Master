using System;

namespace LasalMotionControlLib
{
    [Flags]
    public enum LMCDiagnosticCapability : uint
    {
        None = 0,
        EtherCATHealth = 1u << 0,
        SignalCatalog = 1u << 1,
        PIRead = 1u << 2,
        BulkSnapshot = 1u << 3,
        RecorderSingleBank = 1u << 4,
        RecorderTrigger = 1u << 5,
        RecorderDoubleBank = 1u << 6,
        PIWrite = 1u << 7,
        SDORead = 1u << 8,
        SDOWrite = 1u << 9,
        ApplicationPhaseSnapshot = 1u << 10,
        ExtendedWkcDiagnostics = 1u << 11,
        ExtendedSdoResultChunk = 1u << 12,
        SDOReadGeneralInline = 1u << 13
    }

    [Flags]
    public enum LMCDiagnosticsResponseFlags : ushort
    {
        None = 0,
        Partial = 1 << 0,
        LastChunk = 1 << 1
    }

    public enum LMCDiagnosticsDetailCode : uint
    {
        None = 0,
        UnsupportedSchema = 1,
        UnsupportedFeature = 2,
        MapRevisionMismatch = 3,
        SignalNotFound = 4,
        TypeMismatch = 5,
        ReadDenied = 6,
        WriteDenied = 7,
        UnsafeWriteBlocked = 8,
        ResourceBusy = 9,
        HandleOrGenerationStale = 10,
        NotReady = 11,
        BoundsInvalid = 12,
        MixedCapturePhase = 13,
        BufferNotFrozen = 14,
        BufferOverwritten = 15,
        RtMailboxFull = 16,
        SdoAbort = 17,
        SlaveOffline = 18,
        InvalidState = 19,
        CapacityExceeded = 20,
        RecordNotFound = 21,
        BufferIdentityMismatch = 22,
        TicketNotFound = 23,
        InternalError = 24,
        BootIdMismatch = 25
    }

    public sealed class LMCDiagnosticsResponse
    {
        internal LMCDiagnosticsResponse(
            LMC_Response transportResponse,
            ushort schemaVersion,
            LMCDiagnosticsResponseFlags responseFlags,
            ushort commandStatus,
            short errorId,
            uint requestId,
            uint detailCode)
        {
            TransportResponse = transportResponse;
            SchemaVersion = schemaVersion;
            ResponseFlags = responseFlags;
            CommandStatus = commandStatus;
            ErrorId = errorId;
            RequestId = requestId;
            DetailCode = detailCode;
        }

        public LMC_Response TransportResponse { get; private set; }
        public ushort SchemaVersion { get; private set; }
        public LMCDiagnosticsResponseFlags ResponseFlags { get; private set; }
        public ushort CommandStatus { get; private set; }
        public short ErrorId { get; private set; }
        public uint RequestId { get; private set; }
        public uint DetailCode { get; private set; }

        public LMCDiagnosticsDetailCode Detail
        {
            get { return (LMCDiagnosticsDetailCode)DetailCode; }
        }

        public bool IsSuccess
        {
            get
            {
                return TransportResponse != null
                    && TransportResponse.IsFrameValid
                    && TransportResponse.HeaderStatus == 0
                    && CommandStatus == 0
                    && ErrorId == 0;
            }
        }
    }

    public sealed class LMCDiagnosticCapabilities
    {
        internal LMCDiagnosticCapabilities(
            LMCDiagnosticsResponse response,
            long connectionSessionGeneration,
            uint diagnosticsBuild,
            uint capabilityBits,
            uint mapRevision,
            ushort catalogEntryCount,
            ushort maxBulkSignals,
            ushort maxRecorderChannels,
            ushort recorderBufferCount,
            uint maxRecorderSamples,
            uint baseCycleTimeUs,
            ushort maxRequestPayloadBytes,
            ushort maxResponsePayloadBytes,
            ushort maxChunkDataBytes,
            ushort catalogEntryStride,
            ushort signalValueEntryStride,
            uint recorderBytesPerBank,
            ushort maxSdoDataBytes,
            uint diagnosticsBootId)
        {
            Response = response;
            ConnectionSessionGeneration = connectionSessionGeneration;
            DiagnosticsBuild = diagnosticsBuild;
            CapabilityBits = capabilityBits;
            MapRevision = mapRevision;
            CatalogEntryCount = catalogEntryCount;
            MaxBulkSignals = maxBulkSignals;
            MaxRecorderChannels = maxRecorderChannels;
            RecorderBufferCount = recorderBufferCount;
            MaxRecorderSamples = maxRecorderSamples;
            BaseCycleTimeUs = baseCycleTimeUs;
            MaxRequestPayloadBytes = maxRequestPayloadBytes;
            MaxResponsePayloadBytes = maxResponsePayloadBytes;
            MaxChunkDataBytes = maxChunkDataBytes;
            CatalogEntryStride = catalogEntryStride;
            SignalValueEntryStride = signalValueEntryStride;
            RecorderBytesPerBank = recorderBytesPerBank;
            MaxSdoDataBytes = maxSdoDataBytes;
            DiagnosticsBootId = diagnosticsBootId;
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint DiagnosticsBuild { get; private set; }
        public uint CapabilityBits { get; private set; }
        public uint MapRevision { get; private set; }
        public ushort CatalogEntryCount { get; private set; }
        public ushort MaxBulkSignals { get; private set; }
        public ushort MaxRecorderChannels { get; private set; }
        public ushort RecorderBufferCount { get; private set; }
        public uint MaxRecorderSamples { get; private set; }
        public uint BaseCycleTimeUs { get; private set; }
        public ushort MaxRequestPayloadBytes { get; private set; }
        public ushort MaxResponsePayloadBytes { get; private set; }
        public ushort MaxChunkDataBytes { get; private set; }
        public ushort CatalogEntryStride { get; private set; }
        public ushort SignalValueEntryStride { get; private set; }
        public uint RecorderBytesPerBank { get; private set; }
        public ushort MaxSdoDataBytes { get; private set; }
        public uint DiagnosticsBootId { get; private set; }

        internal long ConnectionSessionGeneration { get; private set; }

        public LMCDiagnosticCapability Capabilities
        {
            get { return (LMCDiagnosticCapability)CapabilityBits; }
        }

        public bool HasStableDiagnosticsBootId
        {
            get { return DiagnosticsBootId != 0; }
        }

        public bool Supports(LMCDiagnosticCapability capability)
        {
            return (Capabilities & capability) == capability;
        }
    }

    public class LMCDiagnosticsCommandException : InvalidOperationException
    {
        internal LMCDiagnosticsCommandException(
            string message,
            LMCDiagnosticsResponse response)
            : this(message, response, null)
        {
        }

        internal LMCDiagnosticsCommandException(
            string message,
            LMCDiagnosticsResponse response,
            Exception innerException)
            : base(message, innerException)
        {
            Response = response;
        }

        public LMCDiagnosticsResponse Response { get; private set; }
    }

    public sealed class LMCDiagnosticsNotSupportedException : NotSupportedException
    {
        internal LMCDiagnosticsNotSupportedException(
            string message,
            LMC_Response response)
            : base(message)
        {
            Response = response;
        }

        public LMC_Response Response { get; private set; }
    }
}
