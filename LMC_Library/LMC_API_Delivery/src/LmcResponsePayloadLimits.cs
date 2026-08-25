using System;

namespace LasalMotionControlLib
{
    internal static class LMC_ResponsePayloadLimits
    {
        private const int ShortAcknowledgementPayloadLength = 4;
        private const int AcknowledgementPayloadLength = 8;
        private const int LookupPayloadLength = 6;
        private const int ReadStatusPayloadLength = 12;
        private const int GroupMembersPayloadLength = 1350;
        private const int GroupPositionPayloadLength = 68;
        private const int RpcSessionInitPayloadLength = 24;
        private const int CallbackRegistrationV2PayloadLength = 20;
        private const int EtherCATSlaveCount = 4;

        internal static int GetMaximumPayloadLength(ushort command)
        {
            switch (command)
            {
                case LMC_CommandId.RpcSessionInit:
                    return RpcSessionInitPayloadLength;

                case LMC_CommandId.RpcCallbackRegistration:
                    return CallbackRegistrationV2PayloadLength;

                case LMC_CommandId.CloseConnection:
                case LMC_CommandId.SetKinTransformEx:
                    return ShortAcknowledgementPayloadLength;

                case LMC_CommandId.GetAxisByName:
                case LMC_CommandId.GetGroupByName:
                    return LookupPayloadLength;

                case LMC_CommandId.Power:
                case LMC_CommandId.Reset:
                case LMC_CommandId.Stop:
                case LMC_CommandId.AxisInfo:
                case LMC_CommandId.ReadPosition:
                case LMC_CommandId.MoveAbsolute:
                case LMC_CommandId.MoveRelative:
                case LMC_CommandId.MoveVelocity:
                case LMC_CommandId.GroupProfileLock:
                case LMC_CommandId.GroupProfileUnlock:
                case LMC_CommandId.GroupReset:
                case LMC_CommandId.GroupPowerOn:
                case LMC_CommandId.GroupPowerOff:
                case LMC_CommandId.GroupStop:
                case LMC_CommandId.MoveLinear:
                    return AcknowledgementPayloadLength;

                case LMC_CommandId.ReadStatus:
                case LMC_CommandId.GroupStatus:
                    return ReadStatusPayloadLength;

                case LMC_CommandId.GetMembers:
                    return GroupMembersPayloadLength;

                case LMC_CommandId.GroupPosition:
                    return GroupPositionPayloadLength;

                case LMC_CommandId.GetAdminCapabilities:
                    return LMC_AdminParser.CapabilitiesPayloadLength;

                case LMC_CommandId.ReadAxisParameter:
                    return LMC_AdminParser.AxisParameterPayloadLength;

                case LMC_CommandId.SetAxisPosition:
                    return LMC_AdminParser.SetAxisPositionResponsePayloadLength;

                case LMC_CommandId.ReadAxisSetPositionOutcome:
                case LMC_CommandId.RetireAxisSetPositionOutcome:
                    return LMC_AdminParser
                        .AxisSetPositionOutcomeResponsePayloadLength;

                case LMC_CommandId.StartAxisHome:
                    return LMC_AdminParser
                        .StartLmcHomeResponsePayloadLength;

                case LMC_CommandId.StartAxisDs402Home:
                    return LMC_AdminParser
                        .StartAxisDs402HomeResponsePayloadLength;

                case LMC_CommandId.ReadAxisDs402HomeOutcome:
                    return LMC_AdminParser
                        .AxisDs402HomeOutcomeResponsePayloadLength;

                case LMC_CommandId.RetireAxisDs402HomeOutcome:
                    return LMC_AdminParser
                        .AxisDs402HomeOutcomeRetirementResponsePayloadLength;

                case LMC_CommandId.StartAxisDs402HomeEx:
                    return LMC_AdminParser
                        .StartAxisDs402HomeExResponsePayloadLength;

                case LMC_CommandId.ReadAxisDs402HomeExOutcome:
                case LMC_CommandId.RetireAxisDs402HomeExOutcome:
                    return LMC_AdminParser
                        .AxisDs402HomeExOutcomeResponsePayloadLength;

                case LMC_CommandId.ReadAxisHomeOutcome:
                case LMC_CommandId.RetireAxisHomeOutcome:
                    return LMC_AdminParser
                        .LmcHomeOutcomeResponsePayloadLength;

                case LMC_CommandId.ReadGroupParameters:
                    return LMC_AdminParser.GroupParametersPayloadLength;

                case LMC_CommandId.GroupMoveLinearRelative:
                    return LMC_AdminParser.CommonResponsePayloadLength;

                case LMC_CommandId.StartAxisSetOperationMode:
                    return LMC_AdminParser
                        .StartAxisSetOperationModeResponsePayloadLength;

                case LMC_CommandId.ReadAxisSetOperationModeOutcome:
                case LMC_CommandId.RetireAxisSetOperationModeOutcome:
                    return LMC_AdminParser
                        .AxisSetOperationModeOutcomeResponsePayloadLength;

                case LMC_CommandId.GetDiagnosticsCapabilities:
                    return LMC_DiagnosticsParser.CapabilitiesPayloadLength;

                case LMC_CommandId.GetSignalCatalogInfo:
                    return LMC_DiagnosticsParser.CatalogInfoPayloadLength;

                case LMC_CommandId.ConfigureBulk:
                case LMC_CommandId.ReadBulkStatus:
                    return LMC_DiagnosticsParser.BulkStatusPayloadLength;

                case LMC_CommandId.GetSignalCatalogChunk:
                    return LMC_DiagnosticsParser.CatalogChunkHeaderPayloadLength
                        + (LMC_DiagnosticsFrame.MaxCatalogEntriesPerChunk
                            * LMC_DiagnosticsParser.CatalogEntryStride);

                case LMC_CommandId.GetOperationStatus:
                    return LMC_DiagnosticsParser.OperationStatusPayloadLength;

                case LMC_CommandId.CancelOperation:
                    return LMC_DiagnosticsParser.CancelOperationPayloadLength;

                case LMC_CommandId.ReadEtherCATHealth:
                    return LMC_DiagnosticsParser.HealthHeaderPayloadLength
                        + (EtherCATSlaveCount
                            * LMC_DiagnosticsParser.SlaveHealthEntryStride);

                case LMC_CommandId.GetEtherCATTopologyInfo:
                    return LMC_DiagnosticsParser.TopologyInfoPayloadLength;

                case LMC_CommandId.GetEtherCATTopologyChunk:
                    return LMC_DiagnosticsParser.TopologyChunkHeaderPayloadLength
                        + (LMC_DiagnosticsFrame.MaxTopologyEntriesPerChunk
                            * LMC_DiagnosticsParser.TopologyEntryStride);

                case LMC_CommandId.ReadEtherCATNodeHealth:
                    return LMC_DiagnosticsParser.NodeHealthPayloadLength;

                case LMC_CommandId.ReadPI:
                    return LMC_DiagnosticsParser.ReadPIPayloadLength;

                case LMC_CommandId.ReadDigitalIO:
                    return LMC_DiagnosticsParser.DigitalIOPayloadLength;

                case LMC_CommandId.SubmitPIWrite:
                case LMC_CommandId.SubmitSdo:
                case LMC_CommandId.SubmitDigitalOutputWrite:
                    return LMC_DiagnosticsParser.SubmitOperationPayloadLength;

                case LMC_CommandId.ReadBulkSnapshot:
                    return LMC_DiagnosticsParser.BulkSnapshotHeaderPayloadLength
                        + (LMC_DiagnosticsFrame.MaxBulkSignalCount
                            * LMC_DiagnosticsParser.SignalValueEntryStride);

                case LMC_CommandId.ReleaseBulk:
                case LMC_CommandId.TriggerRecorder:
                case LMC_CommandId.StopRecorder:
                case LMC_CommandId.ReleaseRecorderBuffer:
                case LMC_CommandId.ReleaseRecorder:
                    return LMC_DiagnosticsParser.CommonResponsePayloadLength;

                case LMC_CommandId.ConfigureRecorder:
                    return LMC_DiagnosticsParser.ConfigureRecorderResponsePayloadLength;

                case LMC_CommandId.ConfigureRecoverableDoubleRecorder:
                    return LMC_DiagnosticsParser
                        .ConfigureRecoverableDoubleRecorderResponsePayloadLength;

                case LMC_CommandId.StartRecorder:
                    return LMC_DiagnosticsParser.StartRecorderResponsePayloadLength;

                case LMC_CommandId.ReadRecorderStatus:
                    return LMC_DiagnosticsParser.RecorderStatusResponsePayloadLength;

                case LMC_CommandId.AdoptRecorder:
                    return LMC_DiagnosticsParser.AdoptRecorderResponsePayloadLength;

                case LMC_CommandId.ReadRecorderBankInventory:
                    return LMC_DiagnosticsParser
                        .RecorderBankInventoryResponsePayloadLength;

                case LMC_CommandId.AdoptEmptyRecorderConfiguration:
                    return LMC_DiagnosticsParser
                        .AdoptEmptyRecorderConfigurationResponsePayloadLength;

                case LMC_CommandId.ReadRecoverableRecorderBankInventory:
                    return LMC_DiagnosticsParser
                        .RecoverableRecorderBankInventoryResponsePayloadLength;

                case LMC_CommandId.ReadRecorderHeader:
                    return LMC_DiagnosticsParser.RecorderHeaderResponseHeaderPayloadLength
                        + (LMC_DiagnosticsFrame.MaxRecorderChannelCount
                            * sizeof(uint));

                case LMC_CommandId.ReadRecorderChunk:
                    return LMC_DiagnosticsParser.RecorderChunkResponseHeaderPayloadLength
                        + LMC_DiagnosticsFrame.AbsoluteMaxRecorderChunkDataBytes;

                case LMC_CommandId.ReadSdoResultChunk:
                    return LMC_DiagnosticsParser.SdoResultChunkResponseHeaderPayloadLength
                        + LMC_DiagnosticsFrame.AbsoluteMaxRecorderChunkDataBytes;

                case LMC_CommandId.StartEncoderMaintenance:
                    return LMC_DiagnosticsParser
                        .StartEncoderMaintenanceResponsePayloadLength;

                case LMC_CommandId.ReadEncoderMaintenanceOutcome:
                case LMC_CommandId.RetireEncoderMaintenanceOutcome:
                    return LMC_DiagnosticsParser
                        .EncoderMaintenanceOutcomeResponsePayloadLength;

                default:
                    throw new NotSupportedException(
                        "RPC command 0x"
                        + command.ToString("X4")
                        + " does not have a response payload limit.");
            }
        }
    }
}
