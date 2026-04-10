using System;
using System.Globalization;
using System.Windows;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;

namespace PmasApiWpfTestApp
{
    public partial class MainWindow
    {
        private void ButtonGroupReadStatus_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GroupReadStatusCmd", delegate
            {
                Context.EnsureGroup();
                ushort errorId = 0;
                var status = Context.GroupAxis.GroupReadStatus(ref errorId);
                Context.Log(string.Format(CultureInfo.InvariantCulture, "GroupStatus=0x{0:X}, ErrorId={1}", status, errorId));
            });
        }

        private void ButtonGroupEnable_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GroupEnableCmd", delegate
            {
                Context.EnsureGroup();
                Context.GroupAxis.GroupEnable();
            });
        }

        private void ButtonGroupDisable_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GroupDisableCmd", delegate
            {
                Context.EnsureGroup();
                Context.GroupAxis.GroupDisable();
            });
        }

        private void ButtonGroupReset_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GroupResetCmd", delegate
            {
                Context.EnsureGroup();
                Context.GroupAxis.GroupReset();
            });
        }

        private void ButtonGroupStop_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GroupStopCmd", delegate
            {
                Context.EnsureGroup();
                Context.GroupAxis.GroupStop(
                    ParseSingle(TextGroupStopDeceleration.Text),
                    ParseSingle(TextGroupStopJerk.Text),
                    (MC_BUFFERED_MODE_ENUM)ComboGroupBufferedMode.SelectedItem);
            });
        }

        private void ButtonGetGroupMembersInfo_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GetGroupMembersInfo", delegate
            {
                Context.EnsureGroup();
                var membersInfo = Context.GroupAxis.GetGroupMembersInfo();
                Context.Log(DumpObject(membersInfo));
            });
        }

        private void ButtonGroupGetStatusRegister_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GetStatusRegisterCmd", delegate
            {
                Context.EnsureGroup();
                uint statusRegister = 0;
                uint mcsLimitRegister = 0;
                byte endMotionReason = 0;
                Context.GroupAxis.GetStatusRegister(ref statusRegister, ref mcsLimitRegister, ref endMotionReason);
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "Group status register=0x{0:X}, mcsLimit=0x{1:X}, endMotionReason={2}",
                    statusRegister,
                    mcsLimitRegister,
                    endMotionReason));
            });
        }

        private void ButtonMoveLinearAbsolute_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_MoveLinearAbsoluteCmd", delegate
            {
                Context.EnsureGroup();
                var position = ParseDoubleArray(TextGroupEndPoint.Text, 16);
                Context.GroupAxis.MoveLinearAbsolute(
                    ParseSingle(TextGroupVelocity.Text),
                    position,
                    (MC_BUFFERED_MODE_ENUM)ComboGroupBufferedMode.SelectedItem);
            });
        }

        private void ButtonMoveLinearRelative_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_MoveLinearRelativeCmd", delegate
            {
                Context.EnsureGroup();
                var distance = ParseDoubleArray(TextGroupEndPoint.Text, 16);
                Context.GroupAxis.MoveLinearRelative(
                    ParseSingle(TextGroupVelocity.Text),
                    distance,
                    (MC_BUFFERED_MODE_ENUM)ComboGroupBufferedMode.SelectedItem);
            });
        }

        private void ButtonMoveLinearAbsoluteEx_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_MoveLinearAbsoluteExCmd", delegate
            {
                Context.EnsureGroup();
                ApplyGroupMotionParameters(
                    out var velocity,
                    out var acceleration,
                    out var deceleration,
                    out var jerk,
                    out var position,
                    out var transitionParams,
                    out var superimposed);

                Context.GroupAxis.MoveLinearAbsoluteEx(
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    position,
                    (MC_BUFFERED_MODE_ENUM)ComboGroupBufferedMode.SelectedItem,
                    (MC_COORD_SYSTEM_ENUM)ComboGroupCoordSystem.SelectedItem,
                    (NC_TRANSITION_MODE_ENUM)ComboGroupTransitionMode.SelectedItem,
                    transitionParams,
                    superimposed,
                    1);
            });
        }

        private void ButtonSetKinTransform_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_SetKinTransform", delegate
            {
                Context.EnsureGroup();
                var axes = Context.GetConfiguredGroupAxes();
                if (axes.Length < 1)
                {
                    throw new InvalidOperationException("Group axes are empty. Fill Group Axes CSV with at least one axis name.");
                }

                var kin = new MC_KIN_REF_CARTESIAN();
                kin.iNumAxes = Math.Min(axes.Length, 3);

                if (kin.iNumAxes >= 1)
                {
                    kin.sNode[0].eType = NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_X_AXIS_TYPE;
                    kin.sNode[0].hNode = axes[0].AxisReference;
                    kin.sNode[0].iMcsToAcsFuncID = NC_TR_FUNC_ID_ENUM.NC_TR_SHIFT_FUNC;
                    kin.sNode[0].ulTrCoef[0] = 1.0;
                    kin.sNode[0].ulTrCoef[1] = 1.0;
                    kin.sNode[0].ulTrCoef[2] = 0.0;
                }

                if (kin.iNumAxes >= 2)
                {
                    kin.sNode[1].eType = NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_Y_AXIS_TYPE;
                    kin.sNode[1].hNode = axes[1].AxisReference;
                    kin.sNode[1].iMcsToAcsFuncID = NC_TR_FUNC_ID_ENUM.NC_TR_SHIFT_FUNC;
                    kin.sNode[1].ulTrCoef[0] = 1.0;
                    kin.sNode[1].ulTrCoef[1] = 1.0;
                    kin.sNode[1].ulTrCoef[2] = 0.0;
                }

                if (kin.iNumAxes >= 3)
                {
                    kin.sNode[2].eType = NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_Z_AXIS_TYPE;
                    kin.sNode[2].hNode = axes[2].AxisReference;
                    kin.sNode[2].iMcsToAcsFuncID = NC_TR_FUNC_ID_ENUM.NC_TR_SHIFT_FUNC;
                    kin.sNode[2].ulTrCoef[0] = 1.0;
                    kin.sNode[2].ulTrCoef[1] = 1.0;
                    kin.sNode[2].ulTrCoef[2] = 0.0;
                }

                Context.GroupAxis.SetKinTransformCartesian(kin);
                Context.Log("Cartesian kinematic transform applied.");
            });
        }

        private void ButtonWaitUntilCondition_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_WaitUntilConditionFB", delegate
            {
                Context.EnsureGroup();
                Context.GroupAxis.WaitUntilConditionFB(
                    ParseDouble(TextConditionReference.Text),
                    ParseInt32(TextConditionParamId.Text),
                    ParseInt32(TextConditionParamIndex.Text),
                    (MC_CONDITIONFB_OPERATION_TYPE)ComboConditionOperation.SelectedItem,
                    ParseUInt16(TextConditionSourceAxisRef.Text),
                    1);
            });
        }

        private void ApplyGroupMotionParameters(
            out double velocity,
            out double acceleration,
            out double deceleration,
            out double jerk,
            out double[] position,
            out double[] transitionParams,
            out byte superimposed)
        {
            velocity = ParseDouble(TextGroupVelocity.Text);
            acceleration = ParseDouble(TextGroupAcceleration.Text);
            deceleration = ParseDouble(TextGroupDeceleration.Text);
            jerk = ParseDouble(TextGroupJerk.Text);
            position = ParseDoubleArray(TextGroupEndPoint.Text, 16);
            transitionParams = ParseDoubleArray(TextGroupTransitionParams.Text, 16);
            superimposed = ParseByte(TextGroupSuperimposed.Text);
        }
    }
}
