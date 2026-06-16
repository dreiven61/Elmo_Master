using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;

namespace PmasApiWpfTestApp
{
    public partial class MainWindow
    {
        private static readonly NC_AXIS_IN_GROUP_TYPE_ENUM_EX[] DefaultCartesianNodeTypes =
        {
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_X_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_Y_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_Z_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_U_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_V_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_W_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N1_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N2_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N3_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N4_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N5_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N6_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N7_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N8_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N9_AXIS_TYPE
        };

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
                ApplyGroupMembersInfoToUi(membersInfo);
            });
        }

        private void ButtonGroupReadMemberStatus_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("Group member ReadStatus", delegate
            {
                EnsureGroupLoadedFromText();
                var axes = GetGroupAxesFromUi();
                ReadGroupMemberStatus(axes);
            });
        }

        private void ButtonGroupPowerOnMembers_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("Group member PowerOn", delegate
            {
                EnsureGroupLoadedFromText();
                var axes = GetGroupAxesFromUi();
                PowerOnGroupMembers(axes);
                ReadGroupMemberStatus(axes);
            });
        }

        private void ButtonGroupPowerOffMembers_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("Group member PowerOff", delegate
            {
                EnsureGroupLoadedFromText();
                var axes = GetGroupAxesFromUi();
                DisableGroupBeforeMemberPowerOff();
                PowerOffGroupMembers(axes);
                ReadGroupMemberStatus(axes);
            });
        }

        private void ButtonPrepareGroupMcs_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("Prepare Group MCS", delegate
            {
                EnsureGroupLoadedFromText();

                var membersInfo = Context.GroupAxis.GetGroupMembersInfo();
                Context.Log(DumpObject(membersInfo));
                ApplyGroupMembersInfoToUi(membersInfo);

                var axes = GetGroupAxesFromUi();
                PowerOnGroupMembers(axes);
                ReadGroupMemberStatus(axes);

                ApplyCartesianTransform(axes);

                Context.GroupAxis.GroupEnable();
                Context.Log("GroupEnable completed inside Prepare Group MCS after SetKinTransform.");
                ReadGroupPositionSnapshot("After Prepare Group MCS", (MC_COORD_SYSTEM_ENUM)ComboGroupCoordSystem.SelectedItem);
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

        private void ButtonGroupReadPosition_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("GroupReadActualTargetPosition", delegate
            {
                Context.EnsureGroup();
                ReadGroupPositionSnapshot("Manual Read Group Position", (MC_COORD_SYSTEM_ENUM)ComboGroupCoordSystem.SelectedItem);
            });
        }

        private void ButtonMoveLinearAbsolute_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_MoveLinearAbsoluteCmd", delegate
            {
                RunMoveLinearAbsoluteWithUiParameters("MMC_MoveLinearAbsoluteCmd");
            });
        }

        private void ButtonMoveLinearRelative_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_MoveLinearRelativeCmd", delegate
            {
                RunMoveLinearRelativeWithUiParameters("MMC_MoveLinearRelativeCmd");
            });
        }

        private void ButtonMoveLinearAbsoluteEx_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_MoveLinearAbsoluteExCmd", delegate
            {
                RunMoveLinearAbsoluteWithUiParameters("MMC_MoveLinearAbsoluteExCmd");
            });
        }

        private void ButtonSetKinTransform_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_SetKinTransform", delegate
            {
                EnsureGroupLoadedFromText();
                ApplyCartesianTransform(GetGroupAxesFromUi());
            });
        }

        private void ButtonApplySafeGroupDefaults_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("Safe Test Values", delegate
            {
                TextGroupVelocity.Text = "8388608";
                TextGroupAcceleration.Text = "8388608000";
                TextGroupDeceleration.Text = "8388608000";
                TextGroupJerk.Text = "8388608000000";
                TextGroupEndPoint.Text = BuildDefaultGroupEndpoint();
                TextGroupTransitionParams.Text = "1,1,0,0";
                TextGroupSuperimposed.Text = "0";
                SelectComboItemByName(ComboGroupBufferedMode, "MC_ABORTING_MODE", "MC_ABORTING");
                SelectComboItemByName(ComboGroupCoordSystem, "MC_MCS_COORD");
                SelectComboItemByName(ComboGroupTransitionMode, "MC_TM_NONE_MODE");
                Context.Log("Group motion defaults applied from current test values.");
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

        private void RunMoveLinearAbsoluteWithUiParameters(string commandName)
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

            var bufferedMode = (MC_BUFFERED_MODE_ENUM)ComboGroupBufferedMode.SelectedItem;
            var coordSystem = (MC_COORD_SYSTEM_ENUM)ComboGroupCoordSystem.SelectedItem;
            var transitionMode = (NC_TRANSITION_MODE_ENUM)ComboGroupTransitionMode.SelectedItem;

            LogGroupMotionRequest(commandName, "Absolute", velocity, acceleration, deceleration, jerk, position, coordSystem, transitionMode);
            ReadGroupPositionSnapshot("Before " + commandName, coordSystem);

            Context.GroupAxis.MoveLinearAbsoluteEx(
                velocity,
                acceleration,
                deceleration,
                jerk,
                position,
                bufferedMode,
                coordSystem,
                transitionMode,
                transitionParams,
                superimposed,
                1);

            Context.Log(commandName + " command accepted. Motion completion is asynchronous; check EndMotion and Group position logs.");
            ReadGroupPositionSnapshot("After " + commandName, coordSystem);
        }

        private void RunMoveLinearRelativeWithUiParameters(string commandName)
        {
            Context.EnsureGroup();
            ApplyGroupMotionParameters(
                out var velocity,
                out var acceleration,
                out var deceleration,
                out var jerk,
                out var distance,
                out var transitionParams,
                out var superimposed);

            var bufferedMode = (MC_BUFFERED_MODE_ENUM)ComboGroupBufferedMode.SelectedItem;
            var coordSystem = (MC_COORD_SYSTEM_ENUM)ComboGroupCoordSystem.SelectedItem;
            var transitionMode = (NC_TRANSITION_MODE_ENUM)ComboGroupTransitionMode.SelectedItem;

            LogGroupMotionRequest(commandName, "Relative", velocity, acceleration, deceleration, jerk, distance, coordSystem, transitionMode);
            ReadGroupPositionSnapshot("Before " + commandName, coordSystem);

            Context.GroupAxis.MoveLinearRelativeEx(
                velocity,
                acceleration,
                deceleration,
                jerk,
                distance,
                bufferedMode,
                coordSystem,
                transitionMode,
                transitionParams,
                superimposed,
                1);

            Context.Log(commandName + " command accepted. Motion completion is asynchronous; check EndMotion and Group position logs.");
            ReadGroupPositionSnapshot("After " + commandName, coordSystem);
        }

        private void LogGroupMotionRequest(
            string commandName,
            string mode,
            double velocity,
            double acceleration,
            double deceleration,
            double jerk,
            double[] vector,
            MC_COORD_SYSTEM_ENUM coordSystem,
            NC_TRANSITION_MODE_ENUM transitionMode)
        {
            Context.Log(string.Format(
                CultureInfo.InvariantCulture,
                "{0} request. Mode={1}, Coord={2}, Transition={3}, Vel={4}, Acc={5}, Dec={6}, Jerk={7}, Vector[0..15]={8}",
                commandName,
                mode,
                coordSystem,
                transitionMode,
                velocity,
                acceleration,
                deceleration,
                jerk,
                FormatVector(vector)));
        }

        private void EnsureGroupLoadedFromText()
        {
            Context.EnsureConnected();

            var groupName = NormalizeNumeric(TextGroupName.Text);
            var groupAxes = NormalizeNumeric(TextGroupAxes.Text);
            if (Context.GroupAxis == null || !string.Equals(Context.GroupName, groupName, StringComparison.Ordinal))
            {
                Context.LoadGroup(groupName, groupAxes);
                return;
            }

            Context.UpdateGroupAxisNames(groupAxes);
        }

        private MMCSingleAxis[] GetGroupAxesFromUi()
        {
            Context.UpdateGroupAxisNames(NormalizeNumeric(TextGroupAxes.Text));
            var axes = Context.GetConfiguredGroupAxes();
            if (axes.Length == 0)
            {
                throw new InvalidOperationException("Group axes are empty. Use MMC_GetGroupMembersInfo or fill Group Axes CSV with real axis names.");
            }

            return axes;
        }

        private void PowerOnGroupMembers(MMCSingleAxis[] axes)
        {
            var bufferedMode = (MC_BUFFERED_MODE_ENUM)ComboGroupBufferedMode.SelectedItem;
            foreach (var axis in axes)
            {
                axis.PowerOn(bufferedMode);
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "PowerOn issued. Name={0}, AxisRef={1}, DriveID={2}",
                    axis.AxisName,
                    axis.AxisReference,
                    axis.DriveID));
            }
        }

        private void PowerOffGroupMembers(MMCSingleAxis[] axes)
        {
            var bufferedMode = (MC_BUFFERED_MODE_ENUM)ComboGroupBufferedMode.SelectedItem;
            foreach (var axis in axes)
            {
                axis.PowerOff(bufferedMode);
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "PowerOff issued. Name={0}, AxisRef={1}, DriveID={2}",
                    axis.AxisName,
                    axis.AxisReference,
                    axis.DriveID));
            }
        }

        private void DisableGroupBeforeMemberPowerOff()
        {
            Context.EnsureGroup();
            try
            {
                Context.GroupAxis.GroupDisable();
                Context.Log("GroupDisable issued before member PowerOff.");
            }
            catch (MMCException ex)
            {
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "GroupDisable before PowerOff failed. Continuing with member PowerOff. Command={0}, MMCError={1}, Status={2}, AxisRef={3}",
                    ex.CommandID,
                    ex.MMCError,
                    ex.Status,
                    ex.AxisRef));
            }
        }

        private void ReadGroupMemberStatus(MMCSingleAxis[] axes)
        {
            foreach (var axis in axes)
            {
                ushort axisErrorId = 0;
                ushort statusWord = 0;
                axis.ReadStatus(ref axisErrorId, ref statusWord);
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "MemberStatus Name={0}, AxisRef={1}, DriveID={2}, AxisErrorID={3}, StatusWord=0x{4:X4}",
                    axis.AxisName,
                    axis.AxisReference,
                    axis.DriveID,
                    axisErrorId,
                    statusWord));
            }
        }

        private void ReadGroupPositionSnapshot(string label, MC_COORD_SYSTEM_ENUM coordSystem)
        {
            try
            {
                var actualPosition = new double[16];
                var actualResult = Context.GroupAxis.GroupReadActualPosition(coordSystem, ref actualPosition);
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}: Actual({1}) Result={2}, Pos[0..15]={3}",
                    label,
                    coordSystem,
                    actualResult,
                    FormatVector(actualPosition)));
            }
            catch (Exception ex)
            {
                Context.Log(label + ": GroupReadActualPosition failed. " + ex.Message);
            }

            try
            {
                var targetPosition = new double[16];
                var targetResult = Context.GroupAxis.GroupReadTargetPosition(coordSystem, ref targetPosition);
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}: Target({1}) Result={2}, Pos[0..15]={3}",
                    label,
                    coordSystem,
                    targetResult,
                    FormatVector(targetPosition)));
            }
            catch (Exception ex)
            {
                Context.Log(label + ": GroupReadTargetPosition failed. " + ex.Message);
            }
        }

        private static string FormatVector(double[] values)
        {
            if (values == null)
            {
                return "<null>";
            }

            return string.Join(",", values
                .Take(16)
                .Select(value => value.ToString("0.###", CultureInfo.InvariantCulture)));
        }

        private void ApplyCartesianTransform(MMCSingleAxis[] axes)
        {
            Context.EnsureGroup();
            if (axes.Length < 1)
            {
                throw new InvalidOperationException("Group axes are empty. Fill Group Axes CSV with at least one real axis name.");
            }

            if (axes.Length > DefaultCartesianNodeTypes.Length)
            {
                throw new InvalidOperationException("Default Cartesian transform supports up to 15 axes: X,Y,Z,U,V,W plus N1-N9 service axes.");
            }

            var kin = new MC_KIN_REF_CARTESIAN();
            kin.iNumAxes = axes.Length;

            for (var i = 0; i < kin.iNumAxes; i++)
            {
                kin.sNode[i].eType = DefaultCartesianNodeTypes[i];
                kin.sNode[i].hNode = axes[i].AxisReference;
                kin.sNode[i].iMcsToAcsFuncID = NC_TR_FUNC_ID_ENUM.NC_TR_SHIFT_FUNC;
                kin.sNode[i].ulTrCoef[0] = 1.0;
                kin.sNode[i].ulTrCoef[1] = 1.0;
                kin.sNode[i].ulTrCoef[2] = 0.0;
            }

            Context.GroupAxis.SetKinTransformCartesian(kin);

            var mapping = string.Join(",", axes
                .Take(kin.iNumAxes)
                .Select((axis, index) => string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}->{1}[{2}]",
                    axis.AxisName,
                    GetCartesianAxisLabel(DefaultCartesianNodeTypes[index]),
                    GetMcsVectorIndex(DefaultCartesianNodeTypes[index]))));
            Context.Log(string.Format(
                CultureInfo.InvariantCulture,
                "Cartesian kinematic transform applied. Mapping={0}, Coef[BackRatio,ForwardRatio,BackShift]=1,1,0",
                mapping));
        }

        private string BuildDefaultGroupEndpoint()
        {
            var axisCount = SplitValues(NormalizeNumeric(TextGroupAxes.Text)).Length;
            if (axisCount <= 0)
            {
                axisCount = 3;
            }

            var valueCount = Math.Max(3, axisCount);

            var values = new string[valueCount];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = "8388608";
            }

            return string.Join(",", values);
        }

        private static string GetCartesianAxisLabel(NC_AXIS_IN_GROUP_TYPE_ENUM_EX axisType)
        {
            var name = axisType.ToString();
            const string prefix = "NC_PROFILER_";
            const string suffix = "_AXIS_TYPE";
            if (name.StartsWith(prefix, StringComparison.Ordinal) && name.EndsWith(suffix, StringComparison.Ordinal))
            {
                return name.Substring(prefix.Length, name.Length - prefix.Length - suffix.Length);
            }

            return name;
        }

        private static int GetMcsVectorIndex(NC_AXIS_IN_GROUP_TYPE_ENUM_EX axisType)
        {
            switch (axisType)
            {
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_X_AXIS_TYPE:
                    return 0;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_Y_AXIS_TYPE:
                    return 1;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_Z_AXIS_TYPE:
                    return 2;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_U_AXIS_TYPE:
                    return 3;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_V_AXIS_TYPE:
                    return 4;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_W_AXIS_TYPE:
                    return 5;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N1_AXIS_TYPE:
                    return 6;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N2_AXIS_TYPE:
                    return 7;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N3_AXIS_TYPE:
                    return 8;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N4_AXIS_TYPE:
                    return 9;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N5_AXIS_TYPE:
                    return 10;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N6_AXIS_TYPE:
                    return 11;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N7_AXIS_TYPE:
                    return 12;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N8_AXIS_TYPE:
                    return 13;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_N9_AXIS_TYPE:
                    return 14;
                case NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_S_AXIS_TYPE:
                    return 15;
                default:
                    return -1;
            }
        }

        private void ApplyGroupMembersInfoToUi(object membersInfo)
        {
            var axisNames = ExtractStringArrayMember(membersInfo, "AxisNames");
            if (axisNames.Length == 0)
            {
                Context.Log("GroupMembersInfo did not expose AxisNames. Group Axes CSV was not changed.");
                return;
            }

            var csv = string.Join(",", axisNames);
            TextGroupAxes.Text = csv;
            Context.UpdateGroupAxisNames(csv);

            var axisReferences = ExtractUInt16ArrayMember(membersInfo, "AxisReferences");
            Context.Log(string.Format(
                CultureInfo.InvariantCulture,
                "Group Axes CSV updated from GroupMembersInfo: {0}{1}",
                csv,
                axisReferences.Length == 0 ? string.Empty : " / AxisRefs=" + string.Join(",", axisReferences.Select(item => item.ToString(CultureInfo.InvariantCulture)))));
        }

        private static string[] ExtractStringArrayMember(object source, string memberName)
        {
            var value = GetPublicMemberValue(source, memberName);
            if (value == null)
            {
                return new string[0];
            }

            var text = value as string;
            if (text != null)
            {
                return SplitValues(text)
                    .Select(item => item.Trim('\0').Trim())
                    .Where(item => item.Length > 0)
                    .ToArray();
            }

            var enumerable = value as IEnumerable;
            if (enumerable == null)
            {
                return new string[0];
            }

            var names = new List<string>();
            foreach (var item in enumerable)
            {
                var name = Convert.ToString(item, CultureInfo.InvariantCulture);
                if (name == null)
                {
                    continue;
                }

                name = name.Trim('\0').Trim();
                if (name.Length > 0)
                {
                    names.Add(name);
                }
            }

            return names.ToArray();
        }

        private static ushort[] ExtractUInt16ArrayMember(object source, string memberName)
        {
            var value = GetPublicMemberValue(source, memberName);
            var enumerable = value as IEnumerable;
            if (enumerable == null || value is string)
            {
                return new ushort[0];
            }

            var values = new List<ushort>();
            foreach (var item in enumerable)
            {
                if (item == null)
                {
                    continue;
                }

                try
                {
                    values.Add(Convert.ToUInt16(item, CultureInfo.InvariantCulture));
                }
                catch
                {
                }
            }

            return values.ToArray();
        }

        private static object GetPublicMemberValue(object source, string memberName)
        {
            if (source == null)
            {
                return null;
            }

            var type = source.GetType();
            var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (field != null)
            {
                return field.GetValue(source);
            }

            var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property != null && property.CanRead && property.GetIndexParameters().Length == 0)
            {
                try
                {
                    return property.GetValue(source, null);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static void SelectComboItemByName(System.Windows.Controls.ComboBox combo, params string[] names)
        {
            foreach (var item in combo.Items)
            {
                var itemName = Convert.ToString(item, CultureInfo.InvariantCulture);
                if (names.Any(name => string.Equals(name, itemName, StringComparison.OrdinalIgnoreCase)))
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }
    }
}
