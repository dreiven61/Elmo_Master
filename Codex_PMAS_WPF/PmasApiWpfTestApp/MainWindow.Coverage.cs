using System.Collections.Generic;
using PmasApiWpfTestApp.Models;

namespace PmasApiWpfTestApp
{
    public partial class MainWindow
    {
        private static IEnumerable<ApiCoverageItem> CreateCoverageItems()
        {
            return new[]
            {
                new ApiCoverageItem { FunctionName = "MMC_RpcInitConnection", Status = "Mapped", Wrapper = "MMCConnection.ConnectRPC", Area = "Connectivity", Notes = "RPC session open" },
                new ApiCoverageItem { FunctionName = "MMC_OpenUdpChannelCmdEx", Status = "Mapped", Wrapper = "ConnectRPC + GetUDPListenerPortNumber", Area = "Connectivity", Notes = "Separate open wrapper is not public; callback UDP is assigned during ConnectRPC" },
                new ApiCoverageItem { FunctionName = "MMC_GetAxisByNameCmd", Status = "Mapped", Wrapper = "new MMCSingleAxis(name, handle)", Area = "Connectivity", Notes = "Loads a single axis object" },
                new ApiCoverageItem { FunctionName = "MMC_GetGroupByNameCmd", Status = "Mapped", Wrapper = "new MMCGroupAxis(name, handle)", Area = "Connectivity", Notes = "Loads a group axis object" },
                new ApiCoverageItem { FunctionName = "MMC_GetErrorCodeDescriptionByID", Status = "Mapped", Wrapper = "MMCConnection.GetErrorCodeDescriptionByID", Area = "Diagnostics", Notes = "Returns description and resolution text" },
                new ApiCoverageItem { FunctionName = "MMC_PowerCmd", Status = "Mapped", Wrapper = "MMCSingleAxis.PowerOn / PowerOff", Area = "Single Axis", Notes = "Power on/off exposed separately in UI" },
                new ApiCoverageItem { FunctionName = "MMC_GroupReadStatusCmd", Status = "Mapped", Wrapper = "MMCGroupAxis.GroupReadStatus", Area = "Group", Notes = "Returns group state and error id" },
                new ApiCoverageItem { FunctionName = "MMC_GroupEnableCmd", Status = "Mapped", Wrapper = "MMCGroupAxis.GroupEnable", Area = "Group", Notes = "Transitions group to standby" },
                new ApiCoverageItem { FunctionName = "MMC_GroupDisableCmd", Status = "Mapped", Wrapper = "MMCGroupAxis.GroupDisable", Area = "Group", Notes = "Disables a motion group" },
                new ApiCoverageItem { FunctionName = "MMC_ConfigBulkReadCmd", Status = "Mapped", Wrapper = "MMCBulkRead.Init / Config", Area = "Bulk Read", Notes = "Supports preset and custom parameter lists" },
                new ApiCoverageItem { FunctionName = "MMC_PerformBulkReadCmd", Status = "Mapped", Wrapper = "MMCBulkRead.Perform", Area = "Bulk Read", Notes = "Reads the configured bulk sample" },
                new ApiCoverageItem { FunctionName = "MMC_Reset", Status = "Mapped", Wrapper = "MMCSingleAxis.Reset", Area = "Single Axis", Notes = "Resets axis error state" },
                new ApiCoverageItem { FunctionName = "MMC_GroupResetCmd", Status = "Mapped", Wrapper = "MMCGroupAxis.GroupReset", Area = "Group", Notes = "Resets group error state" },
                new ApiCoverageItem { FunctionName = "MMC_SendSdoCmd", Status = "Mapped", Wrapper = "MMCSingleAxis.UploadSDO / DownloadSDO", Area = "SDO", Notes = "Typed SDO wrappers for 1/2/4-byte transfers" },
                new ApiCoverageItem { FunctionName = "MMC_ReadParameter", Status = "Mapped", Wrapper = "MMCSingleAxis.GetParameter", Area = "Single Axis", Notes = "Vendor-specific numeric parameter read" },
                new ApiCoverageItem { FunctionName = "MMC_MoveRelativeExCmd", Status = "Mapped", Wrapper = "MMCSingleAxis.MoveRelativeEx", Area = "Single Axis", Notes = "Extended relative move" },
                new ApiCoverageItem { FunctionName = "MMC_ReadBoolParameter", Status = "Mapped", Wrapper = "MMCSingleAxis.GetBoolParameter", Area = "Single Axis", Notes = "Boolean parameter read" },
                new ApiCoverageItem { FunctionName = "MMC_ChngOpMode", Status = "Mapped", Wrapper = "MMCSingleAxis.SetOpMode", Area = "Single Axis", Notes = "DS402 operation mode switch" },
                new ApiCoverageItem { FunctionName = "MMC_SetPositionCmd", Status = "NotExposed", Wrapper = "No public MMCLibDotNET wrapper", Area = "Single Axis", Notes = "Public .NET wrapper v3.0.0.7 has no single-axis SetPosition method" },
                new ApiCoverageItem { FunctionName = "MMC_HomeDS402ExCmd", Status = "Mapped", Wrapper = "MMCSingleAxis.HomeDS402Ex", Area = "Single Axis", Notes = "Extended DS402 homing path" },
                new ApiCoverageItem { FunctionName = "MMC_GetPIVarInfoByAlias", Status = "Mapped", Wrapper = "MMCSingleAxis.GetPIVarInfoByAlias", Area = "PI", Notes = "PI alias lookup" },
                new ApiCoverageItem { FunctionName = "MMC_WritePIVarUShort", Status = "Mapped", Wrapper = "MMCSingleAxis.WritePIVar(VAR_TYPE.USHORT)", Area = "PI", Notes = "USHORT PI write" },
                new ApiCoverageItem { FunctionName = "MMC_ReadPIVarUShort", Status = "Mapped", Wrapper = "MMCSingleAxis.ReadPIVar(VAR_TYPE.USHORT)", Area = "PI", Notes = "USHORT PI read" },
                new ApiCoverageItem { FunctionName = "MMC_WriteParameter", Status = "Mapped", Wrapper = "MMCSingleAxis.SetParameter", Area = "Single Axis", Notes = "Vendor-specific numeric parameter write" },
                new ApiCoverageItem { FunctionName = "MMC_SetKinTransform", Status = "Mapped", Wrapper = "MMCGroupAxis.SetKinTransformCartesian", Area = "Group", Notes = "Cartesian transform builder included" },
                new ApiCoverageItem { FunctionName = "MMC_GroupStopCmd", Status = "Mapped", Wrapper = "MMCGroupAxis.GroupStop", Area = "Group", Notes = "Controlled stop for motion group" },
                new ApiCoverageItem { FunctionName = "MMC_StopCmd", Status = "Mapped", Wrapper = "MMCSingleAxis.Stop", Area = "Single Axis", Notes = "Controlled single-axis stop" },
                new ApiCoverageItem { FunctionName = "MMC_CloseConnection", Status = "Mapped", Wrapper = "MMCConnection.CloseConnection", Area = "Connectivity", Notes = "Connection shutdown" },
                new ApiCoverageItem { FunctionName = "MMC_MoveLinearAbsoluteCmd", Status = "Mapped", Wrapper = "MMCGroupAxis.MoveLinearAbsolute", Area = "Group", Notes = "Linear absolute move in group" },
                new ApiCoverageItem { FunctionName = "MMC_MoveAbsoluteExCmd", Status = "Mapped", Wrapper = "MMCSingleAxis.MoveAbsoluteEx", Area = "Single Axis", Notes = "Extended absolute move" },
                new ApiCoverageItem { FunctionName = "MMC_MoveLinearRelativeCmd", Status = "Mapped", Wrapper = "MMCGroupAxis.MoveLinearRelative", Area = "Group", Notes = "Linear relative move in group" },
                new ApiCoverageItem { FunctionName = "MMC_MoveVelocityExCmd", Status = "Mapped", Wrapper = "MMCSingleAxis.MoveVelocityEx", Area = "Single Axis", Notes = "Extended velocity move" },
                new ApiCoverageItem { FunctionName = "MMC_SetOverrideCmd", Status = "Mapped", Wrapper = "MMCSingleAxis.SetOverride", Area = "Single Axis", Notes = "Velocity/acc/jerk override" },
                new ApiCoverageItem { FunctionName = "MMC_ReadActualPositionCmd", Status = "Mapped", Wrapper = "MMCSingleAxis.GetActualPosition", Area = "Single Axis", Notes = "Actual axis position read" },
                new ApiCoverageItem { FunctionName = "MMC_GetStatusRegisterCmd", Status = "Mapped", Wrapper = "MMCSingleAxis.GetStatusRegister / MMCGroupAxis.GetStatusRegister", Area = "Diagnostics", Notes = "Status register for axis and group" },
                new ApiCoverageItem { FunctionName = "MMC_StopRecordingCmd", Status = "Mapped", Wrapper = "MMCConnection.StopRecording", Area = "Recorder", Notes = "Stops controller recording" },
                new ApiCoverageItem { FunctionName = "MMC_RecStatusCmd", Status = "Mapped", Wrapper = "MMCConnection.GetRecordingStatus", Area = "Recorder", Notes = "Reads recorder status" },
                new ApiCoverageItem { FunctionName = "MMC_BeginRecordingCmd", Status = "Mapped", Wrapper = "MMCConnection.BeginRecording", Area = "Recorder", Notes = "Starts recorder capture" },
                new ApiCoverageItem { FunctionName = "MMC_UploadDataHeaderCmd", Status = "Mapped", Wrapper = "MMCConnection.GetRecordingDataHeader", Area = "Recorder", Notes = "Reads recorder header metadata" },
                new ApiCoverageItem { FunctionName = "MMC_UploadDataCmd", Status = "Mapped", Wrapper = "MMCConnection.GetRecordingData", Area = "Recorder", Notes = "Uploads recorder samples" },
                new ApiCoverageItem { FunctionName = "MMC_ReadStatusCmd", Status = "Mapped", Wrapper = "MMCSingleAxis.ReadStatus", Area = "Single Axis", Notes = "Axis PLC state read" },
                new ApiCoverageItem { FunctionName = "MMC_MoveLinearAbsoluteExCmd", Status = "Mapped", Wrapper = "MMCGroupAxis.MoveLinearAbsoluteEx", Area = "Group", Notes = "Extended linear absolute move" },
                new ApiCoverageItem { FunctionName = "MMC_GetGroupMembersInfo", Status = "Mapped", Wrapper = "MMCGroupAxis.GetGroupMembersInfo", Area = "Group", Notes = "Returns member descriptors" },
                new ApiCoverageItem { FunctionName = "MMC_WaitUntilConditionFB", Status = "Mapped", Wrapper = "MMCSingleAxis / MMCGroupAxis.WaitUntilConditionFB", Area = "Synchronization", Notes = "Condition-based synchronization trigger" }
            };
        }
    }
}
