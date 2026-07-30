using System;
using System.Collections.Generic;
using System.IO;

namespace LasalMotionControlLib
{
    internal static partial class LMC_DiagnosticsFrame
    {
        internal const ushort MaxRecorderChannelCount = 32;
        internal const int ConfigureRecorderRequestHeaderPayloadLength = 56;
        internal const int ConfigureRecoverableDoubleRecorderRequestHeaderPayloadLength = 72;
        internal const int RecorderIdentityRequestPayloadLength = 28;
        internal const int RecorderChunkRequestPayloadLength = 32;
        internal const int AdoptRecorderRequestPayloadLength = 20;
        internal const int RecorderBankInventoryRequestPayloadLength = 24;
        internal const int AdoptEmptyRecorderConfigurationRequestPayloadLength = 28;
        internal const int RecoverableRecorderBankInventoryRequestPayloadLength = 36;
        internal const int RecorderRecoveryTokenLength = 16;
        internal const ushort AbsoluteMaxRecorderChunkDataBytes = 1920;

        internal static byte[] ConfigureRecorder(
            uint requestId,
            uint expectedMapRevision,
            LMCRecorderConfiguration configuration,
            uint diagnosticsBootId)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (configuration.BufferMode == LMCRecorderBufferMode.Double)
            {
                throw new NotSupportedException(
                    "Double-bank Recorder configurations require ConfigureRecoverableDoubleRecorder and an exact recovery token.");
            }

            RequireRecorderMapRevision(expectedMapRevision);
            RequireRecorderBootId(diagnosticsBootId);

            return CreateConfigureRecorderRequest(
                LMC_CommandId.ConfigureRecorder,
                requestId,
                expectedMapRevision,
                configuration,
                diagnosticsBootId,
                ConfigureRecorderRequestHeaderPayloadLength,
                56,
                Guid.Empty);
        }

        internal static byte[] ConfigureRecoverableDoubleRecorder(
            uint requestId,
            uint expectedMapRevision,
            LMCRecorderConfiguration configuration,
            uint diagnosticsBootId,
            Guid recoveryToken)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (configuration.BufferMode != LMCRecorderBufferMode.Double)
            {
                throw new ArgumentException(
                    "Recoverable Recorder configuration requires Double buffer mode.",
                    "configuration");
            }

            if (configuration.RequestedConfigId == 0)
            {
                throw new ArgumentException(
                    "Recoverable Double-bank Recorder configuration requires a caller-selected non-zero RequestedConfigId.",
                    "configuration");
            }

            RequireRecorderRecoveryToken(recoveryToken);
            RequireRecorderMapRevision(expectedMapRevision);
            RequireRecorderBootId(diagnosticsBootId);
            return CreateConfigureRecorderRequest(
                LMC_CommandId.ConfigureRecoverableDoubleRecorder,
                requestId,
                expectedMapRevision,
                configuration,
                diagnosticsBootId,
                ConfigureRecoverableDoubleRecorderRequestHeaderPayloadLength,
                72,
                recoveryToken);
        }

        private static byte[] CreateConfigureRecorderRequest(
            ushort commandId,
            uint requestId,
            uint expectedMapRevision,
            LMCRecorderConfiguration configuration,
            uint diagnosticsBootId,
            int headerPayloadLength,
            int signalPayloadOffset,
            Guid recoveryToken)
        {
            var payloadLength = checked(
                headerPayloadLength
                + configuration.SignalIds.Count * sizeof(uint));
            var buffer = CreateCommonRequest(
                commandId,
                requestId,
                payloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;

            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, expectedMapRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                configuration.RequestedConfigId);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 16,
                configuration.SamplePeriodCycles);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 18,
                configuration.ChannelCount);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 20,
                configuration.SampleCapacity);
            buffer[payloadOffset + 24] = (byte)configuration.BufferMode;
            buffer[payloadOffset + 25] = (byte)configuration.TriggerType;
            buffer[payloadOffset + 26] = (byte)configuration.TriggerValueType;
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 28,
                configuration.PreTriggerSamples);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 32,
                configuration.PostTriggerSamples);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 36,
                configuration.TriggerSignalId);
            buffer[payloadOffset + 40] = (byte)configuration.TriggerOperator;
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 44,
                configuration.TriggerValue);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 48,
                configuration.TriggerMask);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 52,
                diagnosticsBootId);

            if (recoveryToken != Guid.Empty)
            {
                WriteRecorderRecoveryToken(
                    buffer,
                    payloadOffset + 56,
                    recoveryToken);
            }

            for (var index = 0; index < configuration.SignalIds.Count; index++)
            {
                LMC_Frame.WriteUInt32(
                    buffer,
                    payloadOffset
                        + signalPayloadOffset
                        + index * sizeof(uint),
                    configuration.SignalIds[index]);
            }

            return buffer;
        }

        internal static byte[] StartRecorder(
            uint requestId,
            LMCRecorderConfigurationHandle configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            RequireRecorderConfigurationIdentity(configuration);
            var buffer = CreateCommonRequest(
                LMC_CommandId.StartRecorder,
                requestId,
                RecorderIdentityRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, configuration.ConfigId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                configuration.ConfigRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 16,
                configuration.MapRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 20,
                configuration.OwnerSessionEpoch);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                configuration.DiagnosticsBootId);
            return buffer;
        }

        internal static byte[] StopRecorder(
            uint requestId,
            LMCRecorderIdentity identity)
        {
            return CreateRecorderIdentityRequest(
                LMC_CommandId.StopRecorder,
                requestId,
                identity,
                true);
        }

        internal static byte[] TriggerRecorder(
            uint requestId,
            LMCRecorderIdentity identity)
        {
            return CreateRecorderIdentityRequest(
                LMC_CommandId.TriggerRecorder,
                requestId,
                identity,
                true);
        }

        internal static byte[] ReadRecorderStatus(
            uint requestId,
            LMCRecorderIdentity identity)
        {
            return CreateRecorderIdentityRequest(
                LMC_CommandId.ReadRecorderStatus,
                requestId,
                identity,
                false);
        }

        internal static byte[] ReadRecorderHeader(
            uint requestId,
            LMCRecorderIdentity identity)
        {
            return CreateRecorderIdentityRequest(
                LMC_CommandId.ReadRecorderHeader,
                requestId,
                identity,
                false);
        }

        internal static byte[] ReadRecorderChunk(
            uint requestId,
            LMCRecorderChunkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            RequireRecorderIdentity(request.Identity, false);
            var buffer = CreateCommonRequest(
                LMC_CommandId.ReadRecorderChunk,
                requestId,
                RecorderChunkRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 8,
                request.Identity.RecordId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                request.Identity.BufferId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 16,
                request.OffsetSample);
            LMC_Frame.WriteUInt16(
                buffer,
                payloadOffset + 20,
                request.RequestedSampleCount);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 24, request.Sequence);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 28,
                request.Identity.DiagnosticsBootId);
            return buffer;
        }

        internal static byte[] ReleaseRecorderBuffer(
            uint requestId,
            LMCRecorderIdentity identity)
        {
            return CreateRecorderIdentityRequest(
                LMC_CommandId.ReleaseRecorderBuffer,
                requestId,
                identity,
                true);
        }

        internal static byte[] ReleaseRecorder(
            uint requestId,
            LMCRecorderConfigurationHandle configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            RequireRecorderConfigurationIdentity(configuration);
            var buffer = CreateCommonRequest(
                LMC_CommandId.ReleaseRecorder,
                requestId,
                RecorderIdentityRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, configuration.ConfigId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                configuration.ConfigRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 16,
                configuration.MapRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 20,
                configuration.OwnerSessionEpoch);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                configuration.DiagnosticsBootId);
            return buffer;
        }

        internal static byte[] ReleaseRecorder(
            uint requestId,
            LMCRecoveredRecorderConfigurationLease configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            RequireRecoveredRecorderConfigurationIdentity(configuration);
            var buffer = CreateCommonRequest(
                LMC_CommandId.ReleaseRecorder,
                requestId,
                RecorderIdentityRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, configuration.ConfigId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                configuration.ConfigRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 16,
                configuration.MapRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 20,
                configuration.OwnerSessionEpoch);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                configuration.DiagnosticsBootId);
            return buffer;
        }

        internal static byte[] ReleaseRecorder(
            uint requestId,
            LMCRecorderIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            if (identity.ConfigId == 0
                || identity.ConfigRevision == 0
                || identity.OwnerSessionEpoch == 0)
            {
                throw new ArgumentException(
                    "Recorder identity does not contain configuration ownership metadata. Read status or header after AdoptRecorder first.",
                    "identity");
            }

            RequireRecorderMapRevision(identity.MapRevision);
            RequireRecorderBootId(identity.DiagnosticsBootId);
            var buffer = CreateCommonRequest(
                LMC_CommandId.ReleaseRecorder,
                requestId,
                RecorderIdentityRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, identity.ConfigId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                identity.ConfigRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 16,
                identity.MapRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 20,
                identity.OwnerSessionEpoch);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                identity.DiagnosticsBootId);
            return buffer;
        }

        internal static byte[] AdoptRecorder(
            uint requestId,
            uint diagnosticsBootId,
            uint recordId,
            uint bufferId)
        {
            RequireRecorderBootId(diagnosticsBootId);
            if (recordId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "recordId",
                    "RecordId must be non-zero.");
            }

            return CreateAdoptRecorderRequest(
                requestId,
                diagnosticsBootId,
                recordId,
                bufferId);
        }

        internal static byte[] AdoptActiveRecorder(
            uint requestId,
            uint diagnosticsBootId)
        {
            RequireRecorderBootId(diagnosticsBootId);
            return CreateAdoptRecorderRequest(
                requestId,
                diagnosticsBootId,
                0,
                0);
        }

        internal static byte[] ReadRecorderBankInventory(
            uint requestId,
            uint diagnosticsBootId,
            uint configId,
            uint mapRevision,
            uint configRevision)
        {
            RequireRecorderBootId(diagnosticsBootId);
            if (configId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "configId",
                    "Recorder bank inventory requires a non-zero ConfigId.");
            }

            RequireRecorderMapRevision(mapRevision);
            var buffer = CreateCommonRequest(
                LMC_CommandId.ReadRecorderBankInventory,
                requestId,
                RecorderBankInventoryRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 8,
                diagnosticsBootId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 12, configId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 16, mapRevision);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 20, configRevision);
            return buffer;
        }

        internal static byte[] ReadRecoverableRecorderBankInventory(
            uint requestId,
            uint diagnosticsBootId,
            uint configId,
            uint mapRevision,
            Guid recoveryToken)
        {
            RequireRecorderBootId(diagnosticsBootId);
            if (configId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "configId",
                    "Recoverable Recorder bank inventory requires a non-zero ConfigId.");
            }

            RequireRecorderMapRevision(mapRevision);
            RequireRecorderRecoveryToken(recoveryToken);
            var buffer = CreateCommonRequest(
                LMC_CommandId.ReadRecoverableRecorderBankInventory,
                requestId,
                RecoverableRecorderBankInventoryRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 8,
                diagnosticsBootId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 12, configId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 16, mapRevision);
            WriteRecorderRecoveryToken(
                buffer,
                payloadOffset + 20,
                recoveryToken);
            return buffer;
        }

        internal static byte[] AdoptEmptyRecorderConfiguration(
            uint requestId,
            LMCRecorderBankInventory inventory)
        {
            RequireEmptyRecorderConfigurationInventory(inventory);
            var buffer = CreateCommonRequest(
                LMC_CommandId.AdoptEmptyRecorderConfiguration,
                requestId,
                AdoptEmptyRecorderConfigurationRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 8,
                inventory.DiagnosticsBootId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 12,
                inventory.ConfigId);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 16,
                inventory.ConfigRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 20,
                inventory.MapRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                inventory.ConfigurationOwnerSessionEpoch);
            return buffer;
        }

        private static byte[] CreateAdoptRecorderRequest(
            uint requestId,
            uint diagnosticsBootId,
            uint recordId,
            uint bufferId)
        {
            var buffer = CreateCommonRequest(
                LMC_CommandId.AdoptRecorder,
                requestId,
                AdoptRecorderRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, recordId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 12, bufferId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 16, diagnosticsBootId);
            return buffer;
        }

        private static byte[] CreateRecorderIdentityRequest(
            ushort commandId,
            uint requestId,
            LMCRecorderIdentity identity,
            bool requireOwner)
        {
            RequireRecorderIdentity(identity, requireOwner);
            var buffer = CreateCommonRequest(
                commandId,
                requestId,
                RecorderIdentityRequestPayloadLength);
            var payloadOffset = LMC_Frame.HeaderSize;
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 8, identity.RecordId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 12, identity.BufferId);
            LMC_Frame.WriteUInt32(buffer, payloadOffset + 16, identity.MapRevision);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 20,
                identity.OwnerSessionEpoch);
            LMC_Frame.WriteUInt32(
                buffer,
                payloadOffset + 24,
                identity.DiagnosticsBootId);
            return buffer;
        }

        private static void RequireRecorderConfigurationIdentity(
            LMCRecorderConfigurationHandle configuration)
        {
            if (configuration.ConfigId == 0
                || configuration.ConfigRevision == 0
                || configuration.OwnerSessionEpoch == 0)
            {
                throw new ArgumentException(
                    "Recorder configuration identity fields must be non-zero.",
                    "configuration");
            }

            RequireRecorderMapRevision(configuration.MapRevision);
            RequireRecorderBootId(configuration.DiagnosticsBootId);
        }

        private static void RequireRecoveredRecorderConfigurationIdentity(
            LMCRecoveredRecorderConfigurationLease configuration)
        {
            if (configuration.ConfigId == 0
                || configuration.ConfigRevision == 0
                || configuration.OwnerSessionEpoch == 0)
            {
                throw new ArgumentException(
                    "Recovered Recorder configuration identity fields must be non-zero.",
                    "configuration");
            }

            RequireRecorderMapRevision(configuration.MapRevision);
            RequireRecorderBootId(configuration.DiagnosticsBootId);
        }

        private static void RequireEmptyRecorderConfigurationInventory(
            LMCRecorderBankInventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }

            RequireRecorderBootId(inventory.DiagnosticsBootId);
            RequireRecorderMapRevision(inventory.MapRevision);
            if (inventory.ConfigId == 0
                || inventory.ConfigRevision == 0
                || inventory.IsRecoverable
                || inventory.RecoveryToken != Guid.Empty
                || inventory.ConfigurationOwnerSessionEpoch == 0
                || !inventory.IsConfigurationOwnerSessionClosed
                || inventory.ConfigurationState != LMCRecorderState.Configured
                || inventory.BufferMode != LMCRecorderBufferMode.Double
                || inventory.RecorderBufferCount != 2
                || inventory.OccupiedBanks.Count != 0)
            {
                throw new ArgumentException(
                    "AdoptEmptyRecorderConfiguration requires an exact standard 0x7E4A closed, empty, two-bank Recorder inventory without a recovery token.",
                    "inventory");
            }
        }

        private static void RequireRecorderIdentity(
            LMCRecorderIdentity identity,
            bool requireOwner)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            if (identity.RecordId == 0
                || (requireOwner && identity.OwnerSessionEpoch == 0))
            {
                throw new ArgumentException(
                    "Recorder identity fields are invalid for this command.",
                    "identity");
            }

            RequireRecorderMapRevision(identity.MapRevision);
            RequireRecorderBootId(identity.DiagnosticsBootId);
        }

        private static void RequireRecorderMapRevision(uint mapRevision)
        {
            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "mapRevision",
                    "Recorder operations require a non-zero exact Catalog revision.");
            }
        }

        private static void RequireRecorderBootId(uint diagnosticsBootId)
        {
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "Recorder operations require a non-zero DiagnosticsBootId.");
            }
        }

        private static void RequireRecorderRecoveryToken(Guid recoveryToken)
        {
            if (recoveryToken == Guid.Empty)
            {
                throw new ArgumentException(
                    "Recorder recovery tokens must be nonempty.",
                    "recoveryToken");
            }
        }

        private static void WriteRecorderRecoveryToken(
            byte[] buffer,
            int offset,
            Guid recoveryToken)
        {
            var bytes = recoveryToken.ToByteArray();
            Buffer.BlockCopy(
                bytes,
                0,
                buffer,
                offset,
                RecorderRecoveryTokenLength);
        }
    }

    internal sealed class LMCRecorderAdoption
    {
        internal LMCRecorderAdoption(
            LMCDiagnosticsResponse response,
            uint diagnosticsBootId,
            uint recordId,
            uint bufferId,
            uint ownerSessionEpoch,
            LMCRecorderState state)
        {
            Response = response;
            DiagnosticsBootId = diagnosticsBootId;
            RecordId = recordId;
            BufferId = bufferId;
            OwnerSessionEpoch = ownerSessionEpoch;
            State = state;
        }

        internal LMCDiagnosticsResponse Response { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint RecordId { get; private set; }
        internal uint BufferId { get; private set; }
        internal uint OwnerSessionEpoch { get; private set; }
        internal LMCRecorderState State { get; private set; }
    }

    internal static partial class LMC_DiagnosticsParser
    {
        internal const int ConfigureRecorderResponsePayloadLength = 56;
        internal const int ConfigureRecoverableDoubleRecorderResponsePayloadLength = 72;
        internal const int StartRecorderResponsePayloadLength = 40;
        internal const int RecorderStatusResponsePayloadLength = 76;
        internal const int RecorderHeaderResponseHeaderPayloadLength = 112;
        internal const int RecorderChunkResponseHeaderPayloadLength = 52;
        internal const int AdoptRecorderResponsePayloadLength = 36;
        internal const int RecorderBankInventoryResponsePayloadLength = 88;
        internal const int RecoverableRecorderBankInventoryResponsePayloadLength = 104;
        internal const int RecorderBankInventoryEntryStride = 20;
        internal const int AdoptEmptyRecorderConfigurationResponsePayloadLength = 40;

        private const ushort KnownRecorderHeaderFlagsMask = 0x000F;

        internal static LMCRecorderConfigurationHandle ParseConfigureRecorder(
            byte[] raw,
            uint expectedRequestId,
            LMCRecorderConfiguration configuration,
            LMCDiagnosticCapabilities capabilities,
            long connectionSessionGeneration,
            LMCDiagnostics owner)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            return ParseConfigureRecorderCore(
                raw,
                expectedRequestId,
                configuration,
                capabilities,
                connectionSessionGeneration,
                owner,
                "ConfigureRecorder",
                ConfigureRecorderResponsePayloadLength,
                Guid.Empty);
        }

        internal static LMCRecorderConfigurationHandle
            ParseConfigureRecoverableDoubleRecorder(
                byte[] raw,
                uint expectedRequestId,
                LMCRecorderConfiguration configuration,
                Guid recoveryToken,
                LMCDiagnosticCapabilities capabilities,
                long connectionSessionGeneration,
                LMCDiagnostics owner)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (configuration.BufferMode != LMCRecorderBufferMode.Double
                || configuration.RequestedConfigId == 0)
            {
                throw new ArgumentException(
                    "Token-qualified Recorder parsing requires Double buffer mode and a caller-selected non-zero RequestedConfigId.",
                    "configuration");
            }

            if (recoveryToken == Guid.Empty)
            {
                throw new ArgumentException(
                    "Recorder recovery tokens must be nonempty.",
                    "recoveryToken");
            }

            return ParseConfigureRecorderCore(
                raw,
                expectedRequestId,
                configuration,
                capabilities,
                connectionSessionGeneration,
                owner,
                "ConfigureRecoverableDoubleRecorder",
                ConfigureRecoverableDoubleRecorderResponsePayloadLength,
                recoveryToken);
        }

        private static LMCRecorderConfigurationHandle
            ParseConfigureRecorderCore(
                byte[] raw,
                uint expectedRequestId,
                LMCRecorderConfiguration configuration,
                LMCDiagnosticCapabilities capabilities,
                long connectionSessionGeneration,
                LMCDiagnostics owner,
                string operationName,
                int responsePayloadLength,
                Guid recoveryToken)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("capabilities");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                operationName);
            RequireExactPayloadLength(
                response,
                responsePayloadLength,
                operationName);
            RequireNoResponseFlags(response, operationName);

            var payload = response.TransportResponse.Payload;
            if (recoveryToken != Guid.Empty
                && !RecorderRecoveryTokenEquals(payload, 56, recoveryToken))
            {
                throw new InvalidDataException(
                    operationName
                        + " returned a mismatched recovery token echo.");
            }

            var configId = LMC_Frame.ReadUInt32(payload, 16);
            var configRevision = LMC_Frame.ReadUInt32(payload, 20);
            var mapRevision = LMC_Frame.ReadUInt32(payload, 24);
            var acceptedCapacity = LMC_Frame.ReadUInt32(payload, 28);
            var reservedDataBytes = LMC_Frame.ReadUInt32(payload, 32);
            var state = (LMCRecorderState)LMC_Frame.ReadUInt16(payload, 36);
            var channelCount = LMC_Frame.ReadUInt16(payload, 38);
            var sampleStrideBytes = LMC_Frame.ReadUInt16(payload, 40);
            var recorderBufferCount = LMC_Frame.ReadUInt16(payload, 42);
            var capturePhase = (LMCCapturePhase)LMC_Frame.ReadUInt16(payload, 44);
            var ownerSessionEpoch = LMC_Frame.ReadUInt32(payload, 48);
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 52);

            var expectedStride = checked((ushort)(configuration.ChannelCount * 4));
            var expectedDataBytes = checked(
                (ulong)acceptedCapacity
                * expectedStride
                * recorderBufferCount);
            if (configId == 0
                || (configuration.RequestedConfigId != 0
                    && configId != configuration.RequestedConfigId)
                || configRevision == 0
                || mapRevision != capabilities.MapRevision
                || acceptedCapacity == 0
                || acceptedCapacity > configuration.SampleCapacity
                || expectedDataBytes > uint.MaxValue
                || reservedDataBytes != (uint)expectedDataBytes
                || state != LMCRecorderState.Configured
                || channelCount != configuration.ChannelCount
                || sampleStrideBytes != expectedStride
                || recorderBufferCount == 0
                || recorderBufferCount > capabilities.RecorderBufferCount
                || !IsRecorderCapturePhase(capturePhase)
                || LMC_Frame.ReadUInt16(payload, 46) != 0
                || ownerSessionEpoch == 0
                || diagnosticsBootId != capabilities.DiagnosticsBootId)
            {
                throw new InvalidDataException(
                    operationName
                        + " returned invalid or mismatched configuration metadata.");
            }


            if (configuration.TriggerType != LMCRecorderTriggerType.Manual
                && (ulong)configuration.PreTriggerSamples
                    + 1
                    + configuration.PostTriggerSamples > acceptedCapacity)
            {
                throw new InvalidDataException(
                    operationName
                        + " accepted too little capacity for its trigger windows.");
            }

            if (configuration.BufferMode == LMCRecorderBufferMode.Single
                && recorderBufferCount != 1)
            {
                throw new InvalidDataException(
                    "A single-bank Recorder configuration must reserve exactly one bank.");
            }

            if (configuration.BufferMode == LMCRecorderBufferMode.Ring
                && recorderBufferCount != 1)
            {
                throw new InvalidDataException(
                    "A ring Recorder configuration must reserve exactly one bank.");
            }

            if (configuration.BufferMode == LMCRecorderBufferMode.Double
                && recorderBufferCount != 2)
            {
                throw new InvalidDataException(
                    "A double-bank Recorder configuration must reserve exactly two banks.");
            }

            return new LMCRecorderConfigurationHandle(
                response,
                configuration,
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision,
                acceptedCapacity,
                checked((uint)configuration.SamplePeriodCycles
                    * capabilities.BaseCycleTimeUs),
                reservedDataBytes,
                state,
                sampleStrideBytes,
                recorderBufferCount,
                capturePhase,
                ownerSessionEpoch,
                capabilities.MaxChunkDataBytes,
                connectionSessionGeneration,
                owner,
                recoveryToken);
        }

        internal static LMCRecorderIdentity ParseStartRecorder(
            byte[] raw,
            uint expectedRequestId,
            LMCRecorderConfigurationHandle configuration,
            long connectionSessionGeneration,
            LMCDiagnostics owner)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "StartRecorder");
            RequireExactPayloadLength(
                response,
                StartRecorderResponsePayloadLength,
                "StartRecorder");
            RequireNoResponseFlags(response, "StartRecorder");

            var payload = response.TransportResponse.Payload;
            var recordId = LMC_Frame.ReadUInt32(payload, 16);
            var bufferId = LMC_Frame.ReadUInt32(payload, 20);
            var state = (LMCRecorderState)LMC_Frame.ReadUInt16(payload, 24);
            var ownerSessionEpoch = LMC_Frame.ReadUInt32(payload, 28);
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 36);

            if (recordId == 0
                || bufferId >= configuration.RecorderBufferCount
                || state != LMCRecorderState.Armed
                || LMC_Frame.ReadUInt16(payload, 26) != 0
                || ownerSessionEpoch != configuration.OwnerSessionEpoch
                || diagnosticsBootId != configuration.DiagnosticsBootId)
            {
                throw new InvalidDataException(
                    "StartRecorder returned an invalid Recorder identity.");
            }

            return new LMCRecorderIdentity(
                response,
                diagnosticsBootId,
                recordId,
                bufferId,
                configuration.ConfigId,
                configuration.ConfigRevision,
                configuration.MapRevision,
                ownerSessionEpoch,
                state,
                LMC_Frame.ReadUInt32(payload, 32),
                configuration.AcceptedCapacity,
                configuration.CapturePhase,
                configuration.SamplePeriodUs,
                configuration.Configuration.BufferMode,
                configuration.Configuration.TriggerType,
                configuration.Configuration.PreTriggerSamples,
                configuration.Configuration.PostTriggerSamples,
                true,
                configuration.MaxChunkDataBytes,
                configuration.SignalIds,
                connectionSessionGeneration,
                owner,
                false);
        }

        internal static LMCDiagnosticsResponse ParseStopRecorder(
            byte[] raw,
            uint expectedRequestId)
        {
            return ParseRecorderCommonOnly(raw, expectedRequestId, "StopRecorder");
        }

        internal static LMCDiagnosticsResponse ParseTriggerRecorder(
            byte[] raw,
            uint expectedRequestId)
        {
            return ParseRecorderCommonOnly(
                raw,
                expectedRequestId,
                "TriggerRecorder");
        }

        internal static LMCRecorderStatus ParseRecorderStatus(
            byte[] raw,
            uint expectedRequestId,
            LMCRecorderIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "ReadRecorderStatus");
            RequireExactPayloadLength(
                response,
                RecorderStatusResponsePayloadLength,
                "ReadRecorderStatus");
            RequireNoResponseFlags(response, "ReadRecorderStatus");

            var payload = response.TransportResponse.Payload;
            var recordId = LMC_Frame.ReadUInt32(payload, 16);
            var bufferId = LMC_Frame.ReadUInt32(payload, 20);
            var configId = LMC_Frame.ReadUInt32(payload, 24);
            var configRevision = LMC_Frame.ReadUInt32(payload, 28);
            var mapRevision = LMC_Frame.ReadUInt32(payload, 32);
            var state = (LMCRecorderState)LMC_Frame.ReadUInt16(payload, 36);
            var capturePhase = (LMCCapturePhase)payload[38];
            var stopReason = (LMCRecorderStopReason)payload[39];
            var sampleCount = LMC_Frame.ReadUInt32(payload, 40);
            var capacity = LMC_Frame.ReadUInt32(payload, 44);
            var triggerIndex = LMC_Frame.ReadUInt32(payload, 48);
            var startCycle = LMC_Frame.ReadUInt32(payload, 52);
            var ownerSessionEpoch = LMC_Frame.ReadUInt32(payload, 68);
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 72);
            var frozen = state == LMCRecorderState.Ready
                || state == LMCRecorderState.Uploading;
            // Status does not carry the original trigger/buffer shape for an
            // adopted identity. Before freeze (or an observed trigger), a ring's
            // oldest retained sample and therefore StartCycle may legitimately move.
            var mutableStartCycle = !frozen
                && triggerIndex == uint.MaxValue
                && (!identity.HasConfigurationShape
                    || identity.TriggerType != LMCRecorderTriggerType.Manual
                    || sampleCount == 0);

            if (recordId != identity.RecordId
                || bufferId != identity.BufferId
                || configId == 0
                || (identity.ConfigId != 0 && configId != identity.ConfigId)
                || configRevision == 0
                || (identity.ConfigRevision != 0
                    && configRevision != identity.ConfigRevision)
                || mapRevision != identity.MapRevision
                || state < LMCRecorderState.Armed
                || state > LMCRecorderState.Fault
                || !IsRecorderCapturePhase(capturePhase)
                || (identity.CapturePhase != LMCCapturePhase.None
                    && capturePhase != identity.CapturePhase)
                || stopReason > LMCRecorderStopReason.Error
                || capacity == 0
                || sampleCount > capacity
                || (stopReason == LMCRecorderStopReason.SampleCountComplete
                    && sampleCount != capacity)
                || (identity.Capacity != 0 && capacity != identity.Capacity)
                || (triggerIndex != uint.MaxValue
                    && triggerIndex >= sampleCount)
                || (identity.HasConfigurationShape
                    && identity.TriggerType == LMCRecorderTriggerType.Manual
                    && (triggerIndex != uint.MaxValue
                        || stopReason == LMCRecorderStopReason.TriggerComplete))
                || (identity.HasConfigurationShape
                    && identity.TriggerType != LMCRecorderTriggerType.Manual
                    && ((triggerIndex == uint.MaxValue
                            && sampleCount > identity.PreTriggerSamples)
                        || (triggerIndex != uint.MaxValue
                            && triggerIndex != identity.PreTriggerSamples)
                        || (triggerIndex != uint.MaxValue
                            && sampleCount > checked(
                                identity.PreTriggerSamples
                                + 1
                                + identity.PostTriggerSamples))
                        || stopReason
                            == LMCRecorderStopReason.SampleCountComplete
                        || (stopReason == LMCRecorderStopReason.TriggerComplete
                            && sampleCount != checked(
                                identity.PreTriggerSamples
                                + 1
                                + identity.PostTriggerSamples))))
                || ownerSessionEpoch == 0
                || (identity.OwnerSessionEpoch != 0
                    && ownerSessionEpoch != identity.OwnerSessionEpoch)
                || (identity.HasAcceptedStartCycleMetadata
                    && (mutableStartCycle
                        || startCycle != identity.AcceptedStartCycle))
                || diagnosticsBootId != identity.DiagnosticsBootId)
            {
                throw new InvalidDataException(
                    "ReadRecorderStatus returned invalid or mismatched Recorder metadata.");
            }

            if ((!frozen && state != LMCRecorderState.Fault
                    && stopReason != LMCRecorderStopReason.None)
                || (frozen && stopReason == LMCRecorderStopReason.None)
                || (stopReason == LMCRecorderStopReason.TriggerComplete
                    && triggerIndex == uint.MaxValue))
            {
                throw new InvalidDataException(
                    "ReadRecorderStatus state and StopReason are inconsistent.");
            }

            return new LMCRecorderStatus(
                response,
                recordId,
                bufferId,
                configId,
                configRevision,
                mapRevision,
                state,
                capturePhase,
                stopReason,
                sampleCount,
                capacity,
                triggerIndex,
                startCycle,
                LMC_Frame.ReadUInt32(payload, 56),
                LMC_Frame.ReadUInt32(payload, 60),
                LMC_Frame.ReadUInt32(payload, 64),
                ownerSessionEpoch,
                diagnosticsBootId);
        }

        internal static LMCRecorderHeader ParseRecorderHeader(
            byte[] raw,
            uint expectedRequestId,
            LMCRecorderIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "ReadRecorderHeader");
            if (response.TransportResponse.PayloadLength
                < RecorderHeaderResponseHeaderPayloadLength)
            {
                throw new InvalidDataException(
                    "ReadRecorderHeader response is shorter than its 112-byte header.");
            }

            RequireNoResponseFlags(response, "ReadRecorderHeader");
            var payload = response.TransportResponse.Payload;
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 16);
            var recordId = LMC_Frame.ReadUInt32(payload, 20);
            var bufferId = LMC_Frame.ReadUInt32(payload, 24);
            var configId = LMC_Frame.ReadUInt32(payload, 28);
            var configRevision = LMC_Frame.ReadUInt32(payload, 32);
            var mapRevision = LMC_Frame.ReadUInt32(payload, 36);
            var capturePhase = (LMCCapturePhase)payload[40];
            var stopReason = (LMCRecorderStopReason)payload[41];
            var headerFlagsValue = LMC_Frame.ReadUInt16(payload, 42);
            var sampleCount = LMC_Frame.ReadUInt32(payload, 44);
            var capacity = LMC_Frame.ReadUInt32(payload, 48);
            var channelCount = LMC_Frame.ReadUInt16(payload, 52);
            var sampleStrideBytes = LMC_Frame.ReadUInt16(payload, 54);
            var samplePeriodUs = LMC_Frame.ReadUInt32(payload, 56);
            var dataEncoding = (LMCRecorderDataEncoding)payload[60];
            var dataCrcPolicy = (LMCRecorderDataCrcPolicy)payload[61];
            var triggerIndex = LMC_Frame.ReadUInt32(payload, 64);
            var startCycle = LMC_Frame.ReadUInt32(payload, 68);

            if (diagnosticsBootId != identity.DiagnosticsBootId
                || recordId != identity.RecordId
                || bufferId != identity.BufferId
                || configId == 0
                || (identity.ConfigId != 0 && configId != identity.ConfigId)
                || configRevision == 0
                || (identity.ConfigRevision != 0
                    && configRevision != identity.ConfigRevision)
                || mapRevision != identity.MapRevision
                || !IsRecorderCapturePhase(capturePhase)
                || (identity.CapturePhase != LMCCapturePhase.None
                    && capturePhase != identity.CapturePhase)
                || stopReason == LMCRecorderStopReason.None
                || stopReason > LMCRecorderStopReason.Error
                || (headerFlagsValue & ~KnownRecorderHeaderFlagsMask) != 0
                || (headerFlagsValue
                    & (ushort)LMCRecorderHeaderFlags.CaptureComplete) == 0
                || capacity == 0
                || sampleCount > capacity
                || (stopReason == LMCRecorderStopReason.SampleCountComplete
                    && sampleCount != capacity)
                || (identity.Capacity != 0 && capacity != identity.Capacity)
                || channelCount == 0
                || channelCount > LMC_DiagnosticsFrame.MaxRecorderChannelCount
                || sampleStrideBytes != channelCount * 4
                || samplePeriodUs == 0
                || (identity.SamplePeriodUs != 0
                    && samplePeriodUs != identity.SamplePeriodUs)
                || dataEncoding
                    != LMCRecorderDataEncoding.SampleMajorRaw32LittleEndian
                || dataCrcPolicy < LMCRecorderDataCrcPolicy.None
                || dataCrcPolicy > LMCRecorderDataCrcPolicy.Crc32IsoHdlc
                || LMC_Frame.ReadUInt16(payload, 62) != 0
                || (triggerIndex != uint.MaxValue && triggerIndex >= sampleCount)
                || (identity.HasAcceptedStartCycleMetadata
                    && startCycle != identity.AcceptedStartCycle))
            {
                throw new InvalidDataException(
                    "ReadRecorderHeader returned invalid or mismatched capture metadata.");
            }

            var flags = (LMCRecorderHeaderFlags)headerFlagsValue;
            var triggerPresent = (flags & LMCRecorderHeaderFlags.TriggerPresent) != 0;
            var userStopped = (flags & LMCRecorderHeaderFlags.UserStopped) != 0;
            var crcPresent = (flags & LMCRecorderHeaderFlags.DataCrcPresent) != 0;
            if (triggerPresent != (triggerIndex != uint.MaxValue)
                || userStopped != (stopReason == LMCRecorderStopReason.UserStop)
                || crcPresent
                    != (dataCrcPolicy == LMCRecorderDataCrcPolicy.Crc32IsoHdlc))
            {
                throw new InvalidDataException(
                    "ReadRecorderHeader flags do not match its trigger, stop, or CRC metadata.");
            }

            if (stopReason == LMCRecorderStopReason.TriggerComplete
                && !triggerPresent)
            {
                throw new InvalidDataException(
                    "TriggerComplete requires trigger metadata in the Recorder header.");
            }

            if (identity.HasConfigurationShape
                && identity.TriggerType == LMCRecorderTriggerType.Manual
                && (triggerPresent
                    || stopReason == LMCRecorderStopReason.TriggerComplete))
            {
                throw new InvalidDataException(
                    "A manual Recorder configuration cannot return trigger metadata.");
            }

            if (identity.HasConfigurationShape
                && identity.TriggerType != LMCRecorderTriggerType.Manual
                && ((triggerIndex == uint.MaxValue
                        && sampleCount > identity.PreTriggerSamples)
                    || (triggerIndex != uint.MaxValue
                        && triggerIndex != identity.PreTriggerSamples)
                    || (triggerIndex != uint.MaxValue
                        && sampleCount > checked(
                            identity.PreTriggerSamples
                            + 1
                            + identity.PostTriggerSamples))
                    || stopReason
                        == LMCRecorderStopReason.SampleCountComplete
                    || (stopReason == LMCRecorderStopReason.TriggerComplete
                        && sampleCount != checked(
                            identity.PreTriggerSamples
                            + 1
                            + identity.PostTriggerSamples))))
            {
                throw new InvalidDataException(
                    "ReadRecorderHeader does not match the configured trigger sample shape.");
            }

            if (!triggerPresent
                && (LMC_Frame.ReadUInt32(payload, 72) != 0
                    || LMC_Frame.ReadUInt32(payload, 88) != 0
                    || LMC_Frame.ReadUInt32(payload, 92) != 0))
            {
                throw new InvalidDataException(
                    "A Recorder header without a trigger must zero trigger cycle and timestamp fields.");
            }

            var expectedLength = checked(
                RecorderHeaderResponseHeaderPayloadLength
                + channelCount * sizeof(uint));
            RequireExactPayloadLength(response, expectedLength, "ReadRecorderHeader");

            var signalIds = new List<uint>(channelCount);
            var uniqueSignalIds = new HashSet<uint>();
            for (var index = 0; index < channelCount; index++)
            {
                var signalId = LMC_Frame.ReadUInt32(payload, 112 + index * 4);
                if (signalId == 0 || !uniqueSignalIds.Add(signalId))
                {
                    throw new InvalidDataException(
                        "ReadRecorderHeader contains a zero or duplicate SignalId.");
                }

                if (identity.ChannelCount != 0
                    && (identity.ChannelCount != channelCount
                        || identity.SignalIds[index] != signalId))
                {
                    throw new InvalidDataException(
                        "ReadRecorderHeader SignalId order does not match the configured Recorder order.");
                }

                signalIds.Add(signalId);
            }

            return new LMCRecorderHeader(
                response,
                diagnosticsBootId,
                recordId,
                bufferId,
                configId,
                configRevision,
                mapRevision,
                capturePhase,
                stopReason,
                flags,
                sampleCount,
                capacity,
                sampleStrideBytes,
                samplePeriodUs,
                dataEncoding,
                dataCrcPolicy,
                triggerIndex,
                startCycle,
                LMC_Frame.ReadUInt32(payload, 72),
                LMC_Frame.ReadUInt32(payload, 76),
                LMC_Frame.ReadUInt32(payload, 80),
                LMC_Frame.ReadUInt32(payload, 84),
                LMC_Frame.ReadUInt32(payload, 88),
                LMC_Frame.ReadUInt32(payload, 92),
                LMC_Frame.ReadUInt32(payload, 96),
                LMC_Frame.ReadUInt32(payload, 100),
                LMC_Frame.ReadUInt32(payload, 104),
                LMC_Frame.ReadUInt32(payload, 108),
                signalIds);
        }

        internal static LMCRecorderChunk ParseRecorderChunk(
            byte[] raw,
            uint expectedRequestId,
            LMCRecorderChunkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!request.Identity.HasFrozenHeaderMetadata)
            {
                throw new InvalidOperationException(
                    "Read Recorder header before parsing Recorder chunks.");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "ReadRecorderChunk");
            if (response.TransportResponse.PayloadLength
                < RecorderChunkResponseHeaderPayloadLength)
            {
                throw new InvalidDataException(
                    "ReadRecorderChunk response is shorter than its 52-byte header.");
            }

            if ((response.ResponseFlags & LMCDiagnosticsResponseFlags.Partial) != 0)
            {
                throw new InvalidDataException(
                    "ReadRecorderChunk does not define the Partial response flag.");
            }

            var payload = response.TransportResponse.Payload;
            var recordId = LMC_Frame.ReadUInt32(payload, 16);
            var bufferId = LMC_Frame.ReadUInt32(payload, 20);
            var offsetSample = LMC_Frame.ReadUInt32(payload, 24);
            var returnedSampleCount = LMC_Frame.ReadUInt16(payload, 28);
            var channelCount = LMC_Frame.ReadUInt16(payload, 30);
            var sequence = LMC_Frame.ReadUInt32(payload, 32);
            var totalSamples = LMC_Frame.ReadUInt32(payload, 36);
            var sampleStrideBytes = LMC_Frame.ReadUInt16(payload, 40);
            var dataByteCount = LMC_Frame.ReadUInt16(payload, 42);
            var dataCrc32 = LMC_Frame.ReadUInt32(payload, 44);
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 48);
            var lastChunk = (response.ResponseFlags
                & LMCDiagnosticsResponseFlags.LastChunk) != 0;

            var endSample = (ulong)offsetSample + returnedSampleCount;
            var expectedDataByteCount = checked(
                (uint)returnedSampleCount * sampleStrideBytes);
            if (recordId != request.Identity.RecordId
                || bufferId != request.Identity.BufferId
                || offsetSample != request.OffsetSample
                || returnedSampleCount == 0
                || returnedSampleCount > request.RequestedSampleCount
                || channelCount == 0
                || channelCount > LMC_DiagnosticsFrame.MaxRecorderChannelCount
                || channelCount != request.Identity.ChannelCount
                || sequence != request.Sequence
                || totalSamples == 0
                || totalSamples != request.Identity.FrozenSampleCount
                || (request.Identity.Capacity != 0
                    && totalSamples > request.Identity.Capacity)
                || endSample > totalSamples
                || sampleStrideBytes != channelCount * 4
                || sampleStrideBytes
                    != request.Identity.FrozenSampleStrideBytes
                || expectedDataByteCount != dataByteCount
                || dataByteCount > request.Identity.MaxChunkDataBytes
                || diagnosticsBootId != request.Identity.DiagnosticsBootId
                || lastChunk != (endSample == totalSamples))
            {
                throw new InvalidDataException(
                    "ReadRecorderChunk returned invalid or mismatched chunk metadata.");
            }

            var expectedLength = checked(
                RecorderChunkResponseHeaderPayloadLength + dataByteCount);
            RequireExactPayloadLength(response, expectedLength, "ReadRecorderChunk");
            var data = new byte[dataByteCount];
            Buffer.BlockCopy(
                payload,
                RecorderChunkResponseHeaderPayloadLength,
                data,
                0,
                data.Length);

            if (request.Identity.DataCrcPolicy
                == LMCRecorderDataCrcPolicy.None)
            {
                if (dataCrc32 != 0)
                {
                    throw new InvalidDataException(
                        "ReadRecorderChunk must return zero DataCrc32 when CRC is disabled.");
                }
            }
            else if (ComputeRecorderDataCrc32(data, 0, data.Length) != dataCrc32)
            {
                throw new InvalidDataException(
                    "ReadRecorderChunk DataCrc32 does not match the returned Data bytes.");
            }

            return new LMCRecorderChunk(
                response,
                recordId,
                bufferId,
                offsetSample,
                returnedSampleCount,
                channelCount,
                sequence,
                totalSamples,
                sampleStrideBytes,
                dataCrc32,
                diagnosticsBootId,
                data);
        }

        internal static LMCDiagnosticsResponse ParseReleaseRecorderBuffer(
            byte[] raw,
            uint expectedRequestId)
        {
            return ParseRecorderCommonOnly(
                raw,
                expectedRequestId,
                "ReleaseRecorderBuffer");
        }

        internal static LMCDiagnosticsResponse ParseReleaseRecorder(
            byte[] raw,
            uint expectedRequestId)
        {
            return ParseRecorderCommonOnly(raw, expectedRequestId, "ReleaseRecorder");
        }

        internal static LMCRecorderAdoption ParseAdoptRecorder(
            byte[] raw,
            uint expectedRequestId,
            uint expectedDiagnosticsBootId,
            uint expectedRecordId,
            uint expectedBufferId)
        {
            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "AdoptRecorder");
            RequireExactPayloadLength(
                response,
                AdoptRecorderResponsePayloadLength,
                "AdoptRecorder");
            RequireNoResponseFlags(response, "AdoptRecorder");

            var payload = response.TransportResponse.Payload;
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 16);
            var recordId = LMC_Frame.ReadUInt32(payload, 20);
            var bufferId = LMC_Frame.ReadUInt32(payload, 24);
            var ownerSessionEpoch = LMC_Frame.ReadUInt32(payload, 28);
            var state = (LMCRecorderState)LMC_Frame.ReadUInt16(payload, 32);
            var discoverActive = expectedRecordId == 0
                && expectedBufferId == 0;
            var identityMatches = discoverActive
                ? recordId != 0 && bufferId == 0
                : expectedRecordId != 0
                    && recordId == expectedRecordId
                    && bufferId == expectedBufferId;
            var stateIsValid = discoverActive
                ? state >= LMCRecorderState.Armed
                    && state <= LMCRecorderState.Uploading
                : state >= LMCRecorderState.Armed
                    && state <= LMCRecorderState.Fault;
            if (diagnosticsBootId != expectedDiagnosticsBootId
                || !identityMatches
                || ownerSessionEpoch == 0
                || !stateIsValid
                || LMC_Frame.ReadUInt16(payload, 34) != 0)
            {
                throw new InvalidDataException(
                    "AdoptRecorder returned invalid or mismatched Recorder ownership metadata.");
            }

            return new LMCRecorderAdoption(
                response,
                diagnosticsBootId,
                recordId,
                bufferId,
                ownerSessionEpoch,
                state);
        }

        internal static LMCRecorderBankInventory ParseRecorderBankInventory(
            byte[] raw,
            uint expectedRequestId,
            uint expectedDiagnosticsBootId,
            uint expectedConfigId,
            uint expectedMapRevision,
            uint expectedConfigRevision)
        {
            LMCDiagnosticsResponse response;
            try
            {
                response = ParseSuccessfulCommand(
                    raw,
                    expectedRequestId,
                    "ReadRecorderBankInventory");
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                if (exception.Response != null
                    && exception.Response.Detail
                        == LMCDiagnosticsDetailCode
                            .RecorderConfigurationAbsent
                    && expectedDiagnosticsBootId != 0
                    && expectedConfigId != 0
                    && expectedConfigRevision != 0
                    && expectedMapRevision != 0)
                {
                    throw new LMCRecorderConfigurationAbsentException(
                        "ReadRecorderBankInventory proved that the exact Recorder configuration is absent and the Recorder store is canonically empty.",
                        exception.Response,
                        expectedDiagnosticsBootId,
                        expectedConfigId,
                        expectedConfigRevision,
                        expectedMapRevision,
                        exception);
                }

                throw;
            }
            RequireExactPayloadLength(
                response,
                RecorderBankInventoryResponsePayloadLength,
                "ReadRecorderBankInventory");
            RequireNoResponseFlags(response, "ReadRecorderBankInventory");
            return ParseRecorderBankInventoryPayload(
                response,
                expectedDiagnosticsBootId,
                expectedConfigId,
                expectedMapRevision,
                expectedConfigRevision,
                "ReadRecorderBankInventory",
                Guid.Empty);
        }

        internal static LMCRecorderBankInventory
            ParseRecoverableRecorderBankInventory(
                byte[] raw,
                uint expectedRequestId,
                uint expectedDiagnosticsBootId,
                uint expectedConfigId,
                uint expectedMapRevision,
                Guid expectedRecoveryToken)
        {
            if (expectedRecoveryToken == Guid.Empty)
            {
                throw new ArgumentException(
                    "Recorder recovery tokens must be nonempty.",
                    "expectedRecoveryToken");
            }

            LMCDiagnosticsResponse response;
            try
            {
                response = ParseSuccessfulCommand(
                    raw,
                    expectedRequestId,
                    "ReadRecoverableRecorderBankInventory");
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                if (exception.Response != null
                    && exception.Response.Detail
                        == LMCDiagnosticsDetailCode
                            .RecorderConfigurationAbsent
                    && expectedDiagnosticsBootId != 0
                    && expectedConfigId != 0
                    && expectedMapRevision != 0)
                {
                    throw new
                        LMCRecoverableRecorderConfigurationAbsentException(
                            "ReadRecoverableRecorderBankInventory proved that the exact recovery token is absent and the Recorder store is canonically empty.",
                            exception.Response,
                            expectedDiagnosticsBootId,
                            expectedConfigId,
                            expectedMapRevision,
                            expectedRecoveryToken,
                            exception);
                }

                throw;
            }

            RequireExactPayloadLength(
                response,
                RecoverableRecorderBankInventoryResponsePayloadLength,
                "ReadRecoverableRecorderBankInventory");
            RequireNoResponseFlags(
                response,
                "ReadRecoverableRecorderBankInventory");
            if (!RecorderRecoveryTokenEquals(
                    response.TransportResponse.Payload,
                    88,
                    expectedRecoveryToken))
            {
                throw new InvalidDataException(
                    "ReadRecoverableRecorderBankInventory returned a mismatched recovery token echo.");
            }

            return ParseRecorderBankInventoryPayload(
                response,
                expectedDiagnosticsBootId,
                expectedConfigId,
                expectedMapRevision,
                0,
                "ReadRecoverableRecorderBankInventory",
                expectedRecoveryToken);
        }

        private static LMCRecorderBankInventory
            ParseRecorderBankInventoryPayload(
                LMCDiagnosticsResponse response,
                uint expectedDiagnosticsBootId,
                uint expectedConfigId,
                uint expectedMapRevision,
                uint expectedConfigRevision,
                string operationName,
                Guid recoveryToken)
        {
            var payload = response.TransportResponse.Payload;
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 16);
            var configId = LMC_Frame.ReadUInt32(payload, 20);
            var configRevision = LMC_Frame.ReadUInt32(payload, 24);
            var mapRevision = LMC_Frame.ReadUInt32(payload, 28);
            var configurationOwnerSessionEpoch =
                LMC_Frame.ReadUInt32(payload, 32);
            var configurationClosedSessionEpoch =
                LMC_Frame.ReadUInt32(payload, 36);
            var configurationState =
                (LMCRecorderState)LMC_Frame.ReadUInt16(payload, 40);
            var bufferMode = (LMCRecorderBufferMode)payload[42];
            var recorderBufferCount = payload[43];
            var occupiedBankCount = payload[44];

            if (diagnosticsBootId != expectedDiagnosticsBootId
                || configId == 0
                || configId != expectedConfigId
                || configRevision == 0
                || (expectedConfigRevision != 0
                    && configRevision != expectedConfigRevision)
                || mapRevision != expectedMapRevision
                || configurationOwnerSessionEpoch == 0
                || (configurationClosedSessionEpoch != 0
                    && configurationClosedSessionEpoch
                        != configurationOwnerSessionEpoch)
                || configurationState < LMCRecorderState.Configured
                || configurationState > LMCRecorderState.Uploading
                || bufferMode != LMCRecorderBufferMode.Double
                || recorderBufferCount != 2
                || occupiedBankCount > recorderBufferCount
                || payload[45] != 0
                || LMC_Frame.ReadUInt16(payload, 46) != 0)
            {
                throw new InvalidDataException(
                    operationName
                        + " returned invalid or mismatched configuration metadata.");
            }

            if (occupiedBankCount == 0
                && configurationState != LMCRecorderState.Configured)
            {
                throw new InvalidDataException(
                    "An empty Recorder bank inventory must report Configured state.");
            }

            var occupiedBanks = new List<LMCRecorderBankInventoryEntry>(
                occupiedBankCount);
            var recordIds = new HashSet<uint>();
            uint previousBufferId = 0;
            uint commonClosedSessionEpoch = 0;
            for (var index = 0; index < recorderBufferCount; index++)
            {
                var offset = 48 + index * RecorderBankInventoryEntryStride;
                var recordId = LMC_Frame.ReadUInt32(payload, offset);
                var bufferId = LMC_Frame.ReadUInt32(payload, offset + 4);
                var ownerSessionEpoch =
                    LMC_Frame.ReadUInt32(payload, offset + 8);
                var closedSessionEpoch =
                    LMC_Frame.ReadUInt32(payload, offset + 12);
                var state = (LMCRecorderState)LMC_Frame.ReadUInt16(
                    payload,
                    offset + 16);
                var reserved = LMC_Frame.ReadUInt16(payload, offset + 18);

                if (index >= occupiedBankCount)
                {
                    if (recordId != 0
                        || bufferId != 0
                        || ownerSessionEpoch != 0
                        || closedSessionEpoch != 0
                        || state != LMCRecorderState.Empty
                        || reserved != 0)
                    {
                        throw new InvalidDataException(
                            operationName
                                + " contains data after OccupiedBankCount.");
                    }

                    continue;
                }

                if (recordId == 0
                    || !recordIds.Add(recordId)
                    || bufferId >= recorderBufferCount
                    || (index != 0 && bufferId <= previousBufferId)
                    || ownerSessionEpoch != configurationOwnerSessionEpoch
                    || (closedSessionEpoch != 0
                        && closedSessionEpoch != ownerSessionEpoch)
                    || closedSessionEpoch
                        != configurationClosedSessionEpoch
                    || state < LMCRecorderState.Armed
                    || state > LMCRecorderState.Uploading
                    || reserved != 0)
                {
                    throw new InvalidDataException(
                        operationName
                            + " contains an invalid or ambiguous occupied-bank entry.");
                }

                previousBufferId = bufferId;
                commonClosedSessionEpoch = closedSessionEpoch;
                occupiedBanks.Add(new LMCRecorderBankInventoryEntry(
                    recordId,
                    bufferId,
                    ownerSessionEpoch,
                    closedSessionEpoch,
                    state));
            }

            if (occupiedBankCount != 0
                && commonClosedSessionEpoch
                    != configurationClosedSessionEpoch)
            {
                throw new InvalidDataException(
                    operationName + " ownership closure is inconsistent.");
            }

            return new LMCRecorderBankInventory(
                response,
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision,
                configurationOwnerSessionEpoch,
                configurationClosedSessionEpoch,
                configurationState,
                bufferMode,
                recorderBufferCount,
                occupiedBanks,
                recoveryToken);
        }

        internal static LMCRecoveredRecorderConfigurationLease
            ParseAdoptEmptyRecorderConfiguration(
                byte[] raw,
                uint expectedRequestId,
                LMCRecorderBankInventory inventory,
                long connectionSessionGeneration,
                LMCDiagnostics owner)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }

            if (inventory.DiagnosticsBootId == 0
                || inventory.ConfigId == 0
                || inventory.ConfigRevision == 0
                || inventory.MapRevision == 0
                || inventory.IsRecoverable
                || inventory.RecoveryToken != Guid.Empty
                || inventory.ConfigurationOwnerSessionEpoch == 0
                || !inventory.IsConfigurationOwnerSessionClosed
                || inventory.ConfigurationState != LMCRecorderState.Configured
                || inventory.BufferMode != LMCRecorderBufferMode.Double
                || inventory.RecorderBufferCount != 2
                || inventory.OccupiedBanks.Count != 0)
            {
                throw new ArgumentException(
                    "AdoptEmptyRecorderConfiguration parser requires an exact standard 0x7E4A closed, empty, two-bank Recorder inventory without a recovery token.",
                    "inventory");
            }

            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                "AdoptEmptyRecorderConfiguration");
            RequireExactPayloadLength(
                response,
                AdoptEmptyRecorderConfigurationResponsePayloadLength,
                "AdoptEmptyRecorderConfiguration");
            RequireNoResponseFlags(
                response,
                "AdoptEmptyRecorderConfiguration");

            var payload = response.TransportResponse.Payload;
            var diagnosticsBootId = LMC_Frame.ReadUInt32(payload, 16);
            var configId = LMC_Frame.ReadUInt32(payload, 20);
            var configRevision = LMC_Frame.ReadUInt32(payload, 24);
            var mapRevision = LMC_Frame.ReadUInt32(payload, 28);
            var ownerSessionEpoch = LMC_Frame.ReadUInt32(payload, 32);
            var state = (LMCRecorderState)LMC_Frame.ReadUInt16(payload, 36);
            var bufferMode = (LMCRecorderBufferMode)payload[38];
            var recorderBufferCount = payload[39];

            if (diagnosticsBootId != inventory.DiagnosticsBootId
                || configId == 0
                || configId != inventory.ConfigId
                || configRevision == 0
                || configRevision != inventory.ConfigRevision
                || mapRevision != inventory.MapRevision
                || ownerSessionEpoch == 0
                || ownerSessionEpoch
                    == inventory.ConfigurationOwnerSessionEpoch
                || state != LMCRecorderState.Configured
                || bufferMode != LMCRecorderBufferMode.Double
                || recorderBufferCount != 2)
            {
                throw new InvalidDataException(
                    "AdoptEmptyRecorderConfiguration returned invalid or mismatched configuration ownership metadata.");
            }

            return new LMCRecoveredRecorderConfigurationLease(
                response,
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision,
                inventory.ConfigurationOwnerSessionEpoch,
                ownerSessionEpoch,
                state,
                bufferMode,
                recorderBufferCount,
                connectionSessionGeneration,
                owner);
        }

        internal static uint ComputeRecorderDataCrc32(
            byte[] data,
            int offset,
            int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (offset < 0 || count < 0 || offset > data.Length - count)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            var crc = 0xFFFFFFFFu;
            for (var index = offset; index < offset + count; index++)
            {
                crc ^= data[index];
                for (var bit = 0; bit < 8; bit++)
                {
                    crc = (crc & 1u) != 0
                        ? (crc >> 1) ^ 0xEDB88320u
                        : crc >> 1;
                }
            }

            return crc ^ 0xFFFFFFFFu;
        }

        private static LMCDiagnosticsResponse ParseRecorderCommonOnly(
            byte[] raw,
            uint expectedRequestId,
            string commandName)
        {
            var response = ParseSuccessfulCommand(
                raw,
                expectedRequestId,
                commandName);
            RequireExactPayloadLength(
                response,
                CommonResponsePayloadLength,
                commandName);
            RequireNoResponseFlags(response, commandName);
            return response;
        }

        private static bool RecorderRecoveryTokenEquals(
            byte[] payload,
            int offset,
            Guid expectedRecoveryToken)
        {
            if (payload == null
                || offset < 0
                || offset > payload.Length
                    - LMC_DiagnosticsFrame.RecorderRecoveryTokenLength)
            {
                return false;
            }

            var expected = expectedRecoveryToken.ToByteArray();
            var difference = 0;
            for (var index = 0;
                index < LMC_DiagnosticsFrame.RecorderRecoveryTokenLength;
                index++)
            {
                difference |= payload[offset + index] ^ expected[index];
            }

            return difference == 0;
        }

        private static bool IsRecorderCapturePhase(LMCCapturePhase phase)
        {
            return phase == LMCCapturePhase.InputMapped
                || phase == LMCCapturePhase.PreOutput;
        }
    }
}
