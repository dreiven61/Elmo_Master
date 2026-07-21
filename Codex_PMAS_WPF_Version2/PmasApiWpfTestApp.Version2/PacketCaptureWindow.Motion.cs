using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;

namespace PmasApiWpfTestApp.Version2
{
    public partial class PacketCaptureWindow
    {
        private const uint GroupStandbyMask = 0x00020000u;
        private const uint GroupDisabledMask = 0x00010000u;
        private const uint GroupErrorMask = 0x00004000u;
        private const uint GroupMovingMask = 0x00002000u;
        private const uint GroupStoppingMask = 0x00001000u;

        private static readonly NC_AXIS_IN_GROUP_TYPE_ENUM_EX[] CaptureCartesianTypes =
        {
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_X_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_Y_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_Z_AXIS_TYPE,
            NC_AXIS_IN_GROUP_TYPE_ENUM_EX.NC_PROFILER_U_AXIS_TYPE
        };

        private MMCSingleAxis captureAxis;
        private MMCGroupAxis captureGroup;
        private string[] captureGroupMemberNames;
        private MMCSingleAxis[] captureGroupAxes;

        private void ButtonLookupAxis_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMCSingleAxis ctor (GetAxisByName + GetDriveID)", delegate
            {
                Context.EnsureConnected();
                var axisName = RequireCaptureName(TextAxisName.Text, "Axis object name");
                captureAxis = new MMCSingleAxis(axisName, Context.Handle);
                TextAxisReference.Text = captureAxis.AxisReference.ToString(CultureInfo.InvariantCulture);
                TextAxisResult.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Loaded PMAS axis {0}\r\nAxisReference={1}, DriveID={2}",
                    captureAxis.AxisName,
                    captureAxis.AxisReference,
                    captureAxis.DriveID);
                LogLocal("Axis loaded through MMCLibDotNET: " + captureAxis.AxisName);
            });
        }

        private void ButtonReadStatus_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_ReadStatusCmd", delegate
            {
                var axis = RequireCaptureAxis();
                TextAxisResult.Text = FormatCaptureAxisStatus(axis);
            });
        }

        private void ButtonReadPosition_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_ReadActualPositionCmd", delegate
            {
                var axis = RequireCaptureAxis();
                var position = axis.GetActualPosition();
                TextAxisResult.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "PMAS actual position\r\nValue={0:R}\r\nMMCLib returns controller/user units as double; no LASAL x10000 conversion was applied.",
                    position);
            });
        }

        private void ButtonPowerOn_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_PowerCmd / PowerOn", delegate
            {
                var axis = RequireCaptureAxis();
                axis.PowerOn(GetAbortingMode());
                TextAxisResult.Text = "PowerOn command accepted by MMCLib. Use Read Status for the authoritative PMAS state.";
            });
        }

        private void ButtonPowerOff_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_PowerCmd / PowerOff", delegate
            {
                var axis = RequireCaptureAxis();
                axis.PowerOff(GetAbortingMode());
                TextAxisResult.Text = "PowerOff command accepted by MMCLib. Use Read Status for the authoritative PMAS state.";
            });
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_Reset", delegate
            {
                var axis = RequireCaptureAxis();
                var result = axis.Reset();
                TextAxisResult.Text = "Reset return code=" + result.ToString(CultureInfo.InvariantCulture);
            });
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_StopCmd", delegate
            {
                var axis = RequireCaptureAxis();
                axis.Stop(
                    ReadCapturePositiveSingle(TextDeceleration.Text, "Stop deceleration"),
                    ReadCaptureNonNegativeSingle(TextJerk.Text, "Stop jerk"),
                    GetAbortingMode());
                TextAxisResult.Text = "Stop command accepted by MMCLib. Read Status to verify Standstill.";
            });
        }

        private void ButtonMoveAbsolute_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_MoveAbsoluteExCmd", delegate
            {
                var axis = RequireCaptureAxis();
                var result = axis.MoveAbsoluteEx(
                    ReadCaptureFiniteDouble(TextPosition.Text, "Position"),
                    ReadCapturePositiveDouble(TextVelocity.Text, "Velocity"),
                    ReadCapturePositiveDouble(TextAcceleration.Text, "Acceleration"),
                    ReadCapturePositiveDouble(TextDeceleration.Text, "Deceleration"),
                    ReadCaptureNonNegativeDouble(TextJerk.Text, "Jerk"),
                    GetCaptureDirection(),
                    GetAbortingMode());
                TextAxisResult.Text = FormatCaptureMotionResult("MoveAbsoluteEx", result);
            });
        }

        private void ButtonMoveRelative_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_MoveRelativeExCmd", delegate
            {
                var axis = RequireCaptureAxis();
                var result = axis.MoveRelativeEx(
                    ReadCaptureFiniteDouble(TextPosition.Text, "Distance"),
                    ReadCapturePositiveDouble(TextVelocity.Text, "Velocity"),
                    ReadCapturePositiveDouble(TextAcceleration.Text, "Acceleration"),
                    ReadCapturePositiveDouble(TextDeceleration.Text, "Deceleration"),
                    ReadCaptureNonNegativeDouble(TextJerk.Text, "Jerk"),
                    GetCaptureDirection(),
                    GetAbortingMode());
                TextAxisResult.Text = FormatCaptureMotionResult("MoveRelativeEx", result);
            });
        }

        private void ButtonMoveVelocity_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_MoveVelocityExCmd", delegate
            {
                var axis = RequireCaptureAxis();
                var result = axis.MoveVelocityEx(
                    ReadCapturePositiveDouble(TextVelocity.Text, "Velocity"),
                    ReadCapturePositiveDouble(TextAcceleration.Text, "Acceleration"),
                    ReadCapturePositiveDouble(TextDeceleration.Text, "Deceleration"),
                    ReadCaptureNonNegativeDouble(TextJerk.Text, "Jerk"),
                    GetCaptureDirection(),
                    GetAbortingMode());
                TextAxisResult.Text = FormatCaptureMotionResult("MoveVelocityEx", result)
                    + "\r\nVelocity motion continues until Stop or PowerOff.";
            });
        }

        private void ButtonLookupGroup_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_GetGroupByNameCmd", delegate
            {
                Context.EnsureConnected();
                var groupName = RequireCaptureName(TextGroupName.Text, "Group object name");
                captureGroup = new MMCGroupAxis(groupName, Context.Handle);
                captureGroupMemberNames = null;
                captureGroupAxes = null;
                TextGroupReference.Text = captureGroup.AxisReference.ToString(CultureInfo.InvariantCulture);
                TextGroupPreparationState.Text = "PMAS group loaded. Run Get Members before any X/Y/Z/U member command.";
                TextGroupResult.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Loaded PMAS group {0}\r\nGroupReference={1}",
                    captureGroup.AxisName,
                    captureGroup.AxisReference);
                LogLocal("Group loaded through MMCLibDotNET: " + captureGroup.AxisName);
            });
        }

        private void ButtonGetMembers_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_GetGroupMembersInfo + member GetAxisByName/GetDriveID", delegate
            {
                var group = RequireCaptureGroup();
                captureGroupMemberNames = null;
                captureGroupAxes = null;
                var members = group.GetGroupMembersInfo();
                var memberNames = (members.AxisNames ?? new string[0])
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name.Trim())
                    .ToArray();
                if (memberNames.Length != 4)
                {
                    captureGroupMemberNames = null;
                    captureGroupAxes = null;
                    TextGroupResult.Text = FormatObject(members);
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Version2 requires exactly four group members for X/Y/Z/U, but controller group {0} returned {1}.",
                        group.AxisName,
                        memberNames.Length));
                }

                var resolvedAxes = memberNames
                    .Select(name => new MMCSingleAxis(name, Context.Handle))
                    .ToArray();
                var memberReferences = members.AxisReferences ?? new ushort[0];
                if (memberReferences.Length < memberNames.Length)
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Controller group {0} returned {1} member names but only {2} member references.",
                        group.AxisName,
                        memberNames.Length,
                        memberReferences.Length));
                }

                for (var index = 0; index < memberNames.Length; index++)
                {
                    if (resolvedAxes[index].AxisReference != memberReferences[index])
                    {
                        throw new InvalidOperationException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Group member mismatch at {0}: controller ref={1}, resolved {2} ref={3}.",
                            index,
                            memberReferences[index],
                            resolvedAxes[index].AxisName,
                            resolvedAxes[index].AxisReference));
                    }
                }

                captureGroupMemberNames = memberNames;
                captureGroupAxes = resolvedAxes;
                TextKinAxisX.Text = memberNames[0];
                TextKinAxisY.Text = memberNames[1];
                TextKinAxisZ.Text = memberNames[2];
                TextKinAxisU.Text = memberNames[3];
                TextGroupPreparationState.Text = "Verified controller group members and cached X/Y/Z/U PMAS axis objects.";
                TextGroupResult.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Group members verified: X={0}, Y={1}, Z={2}, U={3}\r\nAxisRefs={4}\r\nGet Members also resolved each member axis by name and DriveID for later commands.",
                    memberNames[0],
                    memberNames[1],
                    memberNames[2],
                    memberNames[3],
                    string.Join(",", resolvedAxes.Select(axis => axis.AxisReference.ToString(CultureInfo.InvariantCulture))));
            });
        }

        private void ButtonGroupPowerOn_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("Group member PowerOn X/Y/Z/U", delegate
            {
                RequireCaptureGroup();
                var axes = GetCaptureIdentityAxes();
                foreach (var axis in axes)
                {
                    axis.PowerOn(GetAbortingMode());
                    LogLocal(string.Format(
                        CultureInfo.InvariantCulture,
                        "PowerOn member Name={0}, AxisReference={1}",
                        axis.AxisName,
                        axis.AxisReference));
                }

                var memberStatus = BuildCaptureRawHomeStatus(axes);
                TextKinHomeStatus.Text = memberStatus;
                TextGroupPreparationState.Text = "PowerOn issued separately to verified X/Y/Z/U members; member ReadStatus was captured immediately afterward.";
                TextGroupResult.Text = "PMAS has no MMCGroupAxis.GroupPowerOn wrapper. Four PowerOn and four member ReadStatus calls were sent.\r\n" + memberStatus;
            });
        }

        private void ButtonGroupPowerOff_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("Group member PowerOff X/Y/Z/U", delegate
            {
                RequireCaptureGroup();
                var axes = GetCaptureIdentityAxes();
                foreach (var axis in axes)
                {
                    axis.PowerOff(GetAbortingMode());
                    LogLocal(string.Format(
                        CultureInfo.InvariantCulture,
                        "PowerOff member Name={0}, AxisReference={1}",
                        axis.AxisName,
                        axis.AxisReference));
                }

                var memberStatus = BuildCaptureRawHomeStatus(axes);
                TextKinHomeStatus.Text = memberStatus;
                TextGroupPreparationState.Text = "PowerOff issued separately to verified X/Y/Z/U members; member ReadStatus was captured immediately afterward.";
                TextGroupResult.Text = "Four PowerOff and four member ReadStatus calls were sent.\r\n" + memberStatus;
            });
        }

        private void ButtonGroupReadStatus_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_GroupReadStatusCmd", delegate
            {
                var group = RequireCaptureGroup();
                ushort errorId = 0;
                var state = group.GroupReadStatus(ref errorId);
                TextGroupResult.Text = FormatCaptureGroupStatus(state, errorId);
                TextGroupPreparationState.Text = BuildCaptureGroupPreparationText(state, errorId);
            });
        }

        private void ButtonGroupEnable_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_GroupEnableCmd", delegate
            {
                RequireCaptureGroup().GroupEnable();
                TextGroupPreparationState.Text = "GroupEnable issued. Read Group Status until PMAS Standby bit 0x00020000 is set.";
                TextGroupResult.Text = "GroupEnable command accepted by MMCLib.";
            });
        }

        private void ButtonGroupDisable_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_GroupDisableCmd", delegate
            {
                RequireCaptureGroup().GroupDisable();
                TextGroupPreparationState.Text = "GroupDisable issued. Read Group Status until Disabled bit 0x00010000 is set.";
                TextGroupResult.Text = "GroupDisable command accepted by MMCLib.";
            });
        }

        private void ButtonGroupReadPosition_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_GroupReadActualPosition", delegate
            {
                var position = new double[16];
                var result = RequireCaptureGroup().GroupReadActualPosition(
                    GetSelectedGroupCoordinate(),
                    ref position);
                TextGroupResult.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "GroupReadActualPosition return={0}\r\nPosition[0..15]={1}\r\nMMCLib double values are shown directly; no LASAL x10000 conversion was applied.",
                    result,
                    FormatVector(position));
            });
        }

        private void ButtonGroupReset_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_GroupResetCmd", delegate
            {
                RequireCaptureGroup().GroupReset();
                TextGroupResult.Text = "GroupReset command accepted by MMCLib.";
            });
        }

        private void ButtonGroupStop_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_GroupStopCmd", delegate
            {
                RequireCaptureGroup().GroupStop(
                    ReadCapturePositiveSingle(TextGroupDeceleration.Text, "Group stop deceleration"),
                    ReadCaptureNonNegativeSingle(TextGroupJerk.Text, "Group stop jerk"),
                    GetSelectedGroupBufferMode());
                TextGroupResult.Text = "GroupStop command accepted. Read Group Status until PMAS Standby is stable.";
            });
        }

        private void ButtonCheckKinHome_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("PMAS raw X/Y/Z/U status check", delegate
            {
                TextKinHomeStatus.Text = BuildCaptureRawHomeStatus(GetCaptureIdentityAxes());
                TextGroupResult.Text = "Home Check displays PMAS raw ReadStatus only. It does not claim LASAL IsReferenced equivalence.";
            });
        }

        private void ButtonSetKinTransform_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_SetKinTransform / Cartesian X/Y/Z/U", delegate
            {
                var group = RequireCaptureGroup();
                var axes = GetCaptureIdentityAxes();
                TextKinHomeStatus.Text = BuildCaptureRawHomeStatus(axes);

                var kinematics = new MC_KIN_REF_CARTESIAN();
                kinematics.iNumAxes = axes.Length;
                for (var i = 0; i < axes.Length; i++)
                {
                    kinematics.sNode[i].eType = CaptureCartesianTypes[i];
                    kinematics.sNode[i].hNode = axes[i].AxisReference;
                    kinematics.sNode[i].iMcsToAcsFuncID = NC_TR_FUNC_ID_ENUM.NC_TR_SHIFT_FUNC;
                    kinematics.sNode[i].ulTrCoef[0] = 1.0;
                    kinematics.sNode[i].ulTrCoef[1] = 1.0;
                    kinematics.sNode[i].ulTrCoef[2] = 0.0;
                }

                group.SetKinTransformCartesian(kinematics);
                TextGroupPreparationState.Text = "Cartesian X/Y/Z/U identity transform configured. Run GroupEnable, then verify PMAS Standby.";
                TextGroupResult.Text = "SetKinTransformCartesian completed. Raw Home Check was informational and did not enforce LASAL IsReferenced.";
            });
        }

        private void ButtonGroupMoveLinear_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_MoveLinearAbsoluteExCmd", delegate
            {
                var position = new double[16];
                position[0] = ReadCaptureFiniteDouble(TextGroupPositionX.Text, "Group X position");
                position[1] = ReadCaptureFiniteDouble(TextGroupPositionY.Text, "Group Y position");
                position[2] = ReadCaptureFiniteDouble(TextGroupPositionZ.Text, "Group Z position");
                position[3] = ReadCaptureFiniteDouble(TextGroupPositionU.Text, "Group U position");

                var result = RequireCaptureGroup().MoveLinearAbsoluteEx(
                    ReadCapturePositiveDouble(TextGroupVelocity.Text, "Group velocity"),
                    ReadCapturePositiveDouble(TextGroupAcceleration.Text, "Group acceleration"),
                    ReadCapturePositiveDouble(TextGroupDeceleration.Text, "Group deceleration"),
                    ReadCaptureNonNegativeDouble(TextGroupJerk.Text, "Group jerk"),
                    position,
                    GetSelectedGroupBufferMode(),
                    GetSelectedGroupCoordinate(),
                    GetSelectedGroupTransition(),
                    new double[16],
                    0,
                    1);

                TextGroupResult.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "MoveLinearAbsoluteEx return={0}\r\nTarget[0..15]={1}\r\nNo LASAL x10000 conversion was applied. Read Group Status to verify completion.",
                    result,
                    FormatVector(position));
            });
        }

        private void TextAxisName_TextChanged(object sender, TextChangedEventArgs e)
        {
            captureAxis = null;
            if (TextAxisReference != null)
            {
                TextAxisReference.Text = "not loaded";
            }

            if (TextAxisResult != null)
            {
                TextAxisResult.Text = "Axis name changed. Load the PMAS axis again.";
            }
        }

        private void TextGroupName_TextChanged(object sender, TextChangedEventArgs e)
        {
            captureGroup = null;
            captureGroupMemberNames = null;
            captureGroupAxes = null;
            if (TextGroupReference != null)
            {
                TextGroupReference.Text = "not loaded";
            }

            if (TextGroupPreparationState != null)
            {
                TextGroupPreparationState.Text = "Group name changed. Load the PMAS group again.";
            }
        }

        private void TextKinAxis_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextKinHomeStatus != null)
            {
                TextKinHomeStatus.Text = "Home Check: not checked. PMAS raw ReadStatus will be shown; LASAL IsReferenced is not inferred.";
            }
        }

        private MMCSingleAxis RequireCaptureAxis()
        {
            Context.EnsureConnected();
            if (captureAxis == null)
            {
                throw new InvalidOperationException("Load an axis object first.");
            }

            return captureAxis;
        }

        private MMCGroupAxis RequireCaptureGroup()
        {
            Context.EnsureConnected();
            if (captureGroup == null)
            {
                throw new InvalidOperationException("Load a group object first.");
            }

            return captureGroup;
        }

        private MMCSingleAxis[] GetCaptureIdentityAxes()
        {
            RequireCaptureGroup();
            if (captureGroupMemberNames == null || captureGroupAxes == null
                || captureGroupMemberNames.Length != 4 || captureGroupAxes.Length != 4)
            {
                throw new InvalidOperationException("Run Get Members after loading the group. Version2 will not power or bind unverified X/Y/Z/U names.");
            }

            var uiNames = new[]
            {
                RequireCaptureName(TextKinAxisX.Text, "X axis object"),
                RequireCaptureName(TextKinAxisY.Text, "Y axis object"),
                RequireCaptureName(TextKinAxisZ.Text, "Z axis object"),
                RequireCaptureName(TextKinAxisU.Text, "U axis object")
            };
            if (!uiNames.SequenceEqual(captureGroupMemberNames, StringComparer.Ordinal))
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "X/Y/Z/U names no longer match controller group members. Expected={0}; UI={1}. Run Get Members to restore the verified mapping.",
                    string.Join(",", captureGroupMemberNames),
                    string.Join(",", uiNames)));
            }

            return captureGroupAxes;
        }

        private string BuildCaptureRawHomeStatus(MMCSingleAxis[] axes)
        {
            var builder = new StringBuilder();
            builder.AppendLine("PMAS raw ReadStatus only; this is NOT LASAL _LMCAXIS_STATUS.IsReferenced.");
            foreach (var axis in axes)
            {
                ushort axisErrorId = 0;
                ushort statusWord = 0;
                var state = axis.ReadStatus(ref axisErrorId, ref statusWord);
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "{0} Ref={1} State=0x{2:X8} AxisErrorId={3} StatusWord=0x{4:X4}\r\n",
                    axis.AxisName,
                    axis.AxisReference,
                    state,
                    axisErrorId,
                    statusWord);
            }

            builder.Append("Use PMAS homing/status definitions or EndHoming callback for a validated PMAS home decision.");
            return builder.ToString();
        }

        private string FormatCaptureAxisStatus(MMCSingleAxis axis)
        {
            ushort axisErrorId = 0;
            ushort statusWord = 0;
            var state = axis.ReadStatus(ref axisErrorId, ref statusWord);
            return string.Format(
                CultureInfo.InvariantCulture,
                "PMAS ReadStatus\r\nAxis={0}, Ref={1}\r\nState=0x{2:X8}\r\nAxisErrorId={3}\r\nStatusWord=0x{4:X4}\r\nRaw PMAS state bits are intentionally not decoded with LASAL masks.",
                axis.AxisName,
                axis.AxisReference,
                state,
                axisErrorId,
                statusWord);
        }

        private static string FormatCaptureGroupStatus(uint state, ushort errorId)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "PMAS GroupReadStatus\r\nState=0x{0:X8}, ErrorId={1}\r\nStandby={2}, Disabled={3}, Moving={4}, Stopping={5}, Error={6}\r\nGroup member power is verified through member-axis ReadStatus, not a LASAL custom Group PowerOn bit.",
                state,
                errorId,
                (state & GroupStandbyMask) != 0,
                (state & GroupDisabledMask) != 0,
                (state & GroupMovingMask) != 0,
                (state & GroupStoppingMask) != 0,
                (state & GroupErrorMask) != 0);
        }

        private static string BuildCaptureGroupPreparationText(uint state, ushort errorId)
        {
            if (errorId != 0 || (state & GroupErrorMask) != 0)
            {
                return "PMAS group reports an error. Reset and inspect member status before motion.";
            }

            if ((state & GroupStandbyMask) != 0)
            {
                return "PMAS group Standby bit is set. Kinematics/enable state is ready for a controlled move.";
            }

            if ((state & GroupDisabledMask) != 0)
            {
                return "PMAS group Disabled bit is set. Configure kinematics and enable the group before motion.";
            }

            if ((state & GroupMovingMask) != 0 || (state & GroupStoppingMask) != 0)
            {
                return "PMAS group motion/stop is active.";
            }

            return "PMAS group state is neither Standby nor Disabled. Inspect raw status and member axes.";
        }

        private MC_DIRECTION_ENUM GetCaptureDirection()
        {
            var selected = ComboDirection.SelectedItem;
            if (selected is MC_DIRECTION_ENUM)
            {
                return (MC_DIRECTION_ENUM)selected;
            }

            MC_DIRECTION_ENUM parsed;
            if (selected != null
                && Enum.TryParse(Convert.ToString(selected, CultureInfo.InvariantCulture), true, out parsed))
            {
                return parsed;
            }

            return MC_DIRECTION_ENUM.MC_POSITIVE_DIRECTION;
        }

        private static float ReadCapturePositiveSingle(string text, string fieldName)
        {
            return ConvertCaptureSingle(ReadCapturePositiveDouble(text, fieldName), fieldName);
        }

        private static float ReadCaptureNonNegativeSingle(string text, string fieldName)
        {
            return ConvertCaptureSingle(ReadCaptureNonNegativeDouble(text, fieldName), fieldName);
        }

        private static float ConvertCaptureSingle(double value, string fieldName)
        {
            if (value > float.MaxValue)
            {
                throw new ArgumentOutOfRangeException(fieldName, "Value cannot be represented as a PMAS float.");
            }

            return (float)value;
        }

        private static double ReadCapturePositiveDouble(string text, string fieldName)
        {
            var value = ReadCaptureFiniteDouble(text, fieldName);
            if (value <= 0.0)
            {
                throw new ArgumentOutOfRangeException(fieldName, fieldName + " must be a finite positive value.");
            }

            return value;
        }

        private static double ReadCaptureNonNegativeDouble(string text, string fieldName)
        {
            var value = ReadCaptureFiniteDouble(text, fieldName);
            if (value < 0.0)
            {
                throw new ArgumentOutOfRangeException(fieldName, fieldName + " must be a finite value greater than or equal to zero.");
            }

            return value;
        }

        private static double ReadCaptureFiniteDouble(string text, string fieldName)
        {
            var value = ParseDouble(text);
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(fieldName, fieldName + " must be finite.");
            }

            return value;
        }

        private string RequireCaptureName(string value, string fieldName)
        {
            var normalized = Normalize(value);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new ArgumentException(fieldName + " is empty.", fieldName);
            }

            return normalized;
        }

        private static string FormatCaptureMotionResult(string operation, int result)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} return={1}\r\nMMCLib values were sent directly; no LASAL x10000 conversion was applied.",
                operation,
                result);
        }
    }
}
