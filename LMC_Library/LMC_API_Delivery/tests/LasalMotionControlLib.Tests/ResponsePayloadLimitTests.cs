using System;
using System.Collections.Generic;
using System.Reflection;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class ResponsePayloadLimitTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add("Contract.ResponsePayloadLimits.AllCommands", AllCommandsHaveExactLimits);
            tests.Add("Contract.ResponsePayloadLimits.UnknownRejected", UnknownCommandRejected);
        }

        private static void AllCommandsHaveExactLimits()
        {
            var expected = new Dictionary<ushort, int>
            {
                { LMC_CommandId.RpcSessionInit, 24 },
                { LMC_CommandId.RpcCallbackRegistration, 4 },
                { LMC_CommandId.CloseConnection, 4 },
                { LMC_CommandId.GetAxisByName, 6 },
                { LMC_CommandId.GetGroupByName, 6 },
                { LMC_CommandId.Power, 8 },
                { LMC_CommandId.Reset, 8 },
                { LMC_CommandId.Stop, 8 },
                { LMC_CommandId.AxisInfo, 8 },
                { LMC_CommandId.ReadStatus, 12 },
                { LMC_CommandId.ReadPosition, 8 },
                { LMC_CommandId.MoveAbsolute, 8 },
                { LMC_CommandId.MoveRelative, 8 },
                { LMC_CommandId.MoveVelocity, 8 },
                { LMC_CommandId.GetMembers, 1350 },
                { LMC_CommandId.GroupStatus, 12 },
                { LMC_CommandId.GroupProfileLock, 8 },
                { LMC_CommandId.GroupProfileUnlock, 8 },
                { LMC_CommandId.GroupReset, 8 },
                { LMC_CommandId.GroupPowerOn, 8 },
                { LMC_CommandId.GroupPowerOff, 8 },
                { LMC_CommandId.GroupStop, 8 },
                { LMC_CommandId.GroupPosition, 68 },
                { LMC_CommandId.MoveLinear, 8 },
                { LMC_CommandId.SetKinTransformEx, 4 },
                { LMC_CommandId.GetAdminCapabilities, 40 },
                { LMC_CommandId.ReadAxisParameter, 28 },
                { LMC_CommandId.ReadGroupParameters, 32 },
                { LMC_CommandId.GroupMoveLinearRelative, 16 },
                { LMC_CommandId.GetDiagnosticsCapabilities, 68 },
                { LMC_CommandId.GetSignalCatalogInfo, 36 },
                { LMC_CommandId.GetSignalCatalogChunk, 1308 },
                { LMC_CommandId.GetOperationStatus, 64 },
                { LMC_CommandId.CancelOperation, 28 },
                { LMC_CommandId.ReadEtherCATHealth, 200 },
                { LMC_CommandId.GetEtherCATTopologyInfo, 44 },
                { LMC_CommandId.GetEtherCATTopologyChunk, 1564 },
                { LMC_CommandId.ReadEtherCATNodeHealth, 72 },
                { LMC_CommandId.ReadPI, 52 },
                { LMC_CommandId.SubmitPIWrite, 32 },
                { LMC_CommandId.ReadDigitalIO, 56 },
                { LMC_CommandId.SubmitDigitalOutputWrite, 32 },
                { LMC_CommandId.ConfigureBulk, 36 },
                { LMC_CommandId.ReadBulkStatus, 36 },
                { LMC_CommandId.ReadBulkSnapshot, 568 },
                { LMC_CommandId.ReleaseBulk, 16 },
                { LMC_CommandId.ConfigureRecorder, 56 },
                { LMC_CommandId.StartRecorder, 40 },
                { LMC_CommandId.TriggerRecorder, 16 },
                { LMC_CommandId.StopRecorder, 16 },
                { LMC_CommandId.ReadRecorderStatus, 76 },
                { LMC_CommandId.ReadRecorderHeader, 240 },
                { LMC_CommandId.ReadRecorderChunk, 1972 },
                { LMC_CommandId.ReleaseRecorderBuffer, 16 },
                { LMC_CommandId.ReleaseRecorder, 16 },
                { LMC_CommandId.AdoptRecorder, 36 },
                { LMC_CommandId.ReadRecorderBankInventory, 88 },
                { LMC_CommandId.AdoptEmptyRecorderConfiguration, 40 },
                { LMC_CommandId.ConfigureRecoverableDoubleRecorder, 72 },
                { LMC_CommandId.ReadRecoverableRecorderBankInventory, 104 },
                { LMC_CommandId.SubmitSdo, 32 },
                { LMC_CommandId.ReadSdoResultChunk, 1968 }
            };

            var commandFields = typeof(LMC_CommandId).GetFields(
                BindingFlags.Static | BindingFlags.NonPublic);
            var commandCount = 0;

            foreach (var field in commandFields)
            {
                if (!field.IsLiteral || field.FieldType != typeof(ushort))
                {
                    continue;
                }

                commandCount++;
                var command = (ushort)field.GetRawConstantValue();
                AssertEx.True(
                    expected.ContainsKey(command),
                    "Missing response payload limit expectation for "
                    + field.Name
                    + ".");
                AssertEx.Equal(
                    expected[command],
                    LMC_ResponsePayloadLimits.GetMaximumPayloadLength(command),
                    "Unexpected response payload limit for "
                    + field.Name
                    + ".");
            }

            AssertEx.Equal(expected.Count, commandCount);
        }

        private static void UnknownCommandRejected()
        {
            var exception = AssertEx.Throws<NotSupportedException>(
                () => LMC_ResponsePayloadLimits.GetMaximumPayloadLength(0xFFFF));

            AssertEx.Contains("0xFFFF", exception.Message);
        }
    }
}
