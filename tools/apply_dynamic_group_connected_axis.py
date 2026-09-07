from pathlib import Path
import re

CONTROL = Path("Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st")


def replace_once(text: str, old: str, new: str, label: str) -> str:
    count = text.count(old)
    if count != 1:
        raise SystemExit(f"{label}: expected 1 match, found {count}")
    return text.replace(old, new, 1)


def replace_block(text: str, start_hex: str, next_hex: str, replacement: str, label: str) -> str:
    pattern = re.compile(rf"\n\t\t0x{start_hex}:.*?\n\t\t0x{next_hex}:", re.S)
    text, count = pattern.subn(replacement, text, count=1)
    if count != 1:
        raise SystemExit(f"{label}: expected 1 block, found {count}")
    return text


def main() -> None:
    text = CONTROL.read_text(encoding="utf-8")

    decl_anchor = "\tFUNCTION HandleRegistryCommands\n"
    helper_decl = """\tFUNCTION ResolveConnectedGroupAxisMask
\t\tVAR_OUTPUT
\t\t\tResult \t: UDINT;
\t\tEND_VAR;

\tFUNCTION AreResolvedGroupAxesPowered
\t\tVAR_INPUT
\t\t\tGroupAxisMask \t: UDINT;
\t\tEND_VAR
\t\tVAR_OUTPUT
\t\t\tResult \t: BOOL;
\t\tEND_VAR;

\tFUNCTION SetResolvedGroupAxesPower
\t\tVAR_INPUT
\t\t\tGroupAxisMask \t: UDINT;
\t\t\tEnable \t: BOOL;
\t\tEND_VAR
\t\tVAR_OUTPUT
\t\t\tResult \t: DINT;
\t\tEND_VAR;

"""
    if helper_decl not in text:
        text = replace_once(text, decl_anchor, helper_decl + decl_anchor, "helper declarations")

    impl_anchor = "FUNCTION GLOBAL LMCControlCommandService::HandleRequest\n"
    helper_impl = r"""FUNCTION LMCControlCommandService::ResolveConnectedGroupAxisMask
	VAR_OUTPUT
		Result 	: UDINT;
	END_VAR
	VAR
		groupSnapshot : ARRAY [0..303] OF USINT;
		copyResult : DINT;
		cycleCounter : UDINT;
	END_VAR

	Result := 0;
	if IsClientConnected(#InputLatch) = 0 then
		RETURN;
	end_if;
	copyResult := InputLatch.CopySnapshot(
		pDest:=#groupSnapshot[0], DestSize:=304);
	if copyResult <> 0 then
		RETURN;
	end_if;
	cycleCounter := groupSnapshot[0]$UDINT;
	if (cycleCounter = 0) |
	   (groupSnapshot[12]$UDINT <> 8) |
	   (groupSnapshot[16]$UDINT <> 0) then
		RETURN;
	end_if;

	if (IsClientConnected(#LMCAxis1) = 1) &
	   (groupSnapshot[64]$DINT <> 0) &
	   (groupSnapshot[68]$UDINT = 8) &
	   (groupSnapshot[76]$UDINT = 0) &
	   (groupSnapshot[92]$UDINT = cycleCounter) then
		Result := Result or 0x00000001;
	end_if;
	if (IsClientConnected(#LMCAxis2) = 1) &
	   (groupSnapshot[100]$DINT <> 0) &
	   (groupSnapshot[104]$UDINT = 8) &
	   (groupSnapshot[112]$UDINT = 0) &
	   (groupSnapshot[128]$UDINT = cycleCounter) then
		Result := Result or 0x00000002;
	end_if;
	if (IsClientConnected(#LMCAxis3) = 1) &
	   (groupSnapshot[136]$DINT <> 0) &
	   (groupSnapshot[140]$UDINT = 8) &
	   (groupSnapshot[148]$UDINT = 0) &
	   (groupSnapshot[164]$UDINT = cycleCounter) then
		Result := Result or 0x00000004;
	end_if;
	if (IsClientConnected(#LMCAxis4) = 1) &
	   (groupSnapshot[172]$DINT <> 0) &
	   (groupSnapshot[176]$UDINT = 8) &
	   (groupSnapshot[184]$UDINT = 0) &
	   (groupSnapshot[200]$UDINT = cycleCounter) then
		Result := Result or 0x00000008;
	end_if;

END_FUNCTION


FUNCTION LMCControlCommandService::AreResolvedGroupAxesPowered
	VAR_INPUT
		GroupAxisMask 	: UDINT;
	END_VAR
	VAR_OUTPUT
		Result 	: BOOL;
	END_VAR

	Result := FALSE;
	if (GroupAxisMask = 0) | (GroupAxisMask > LMC_OWNER_PROFILE_AXIS_MASK) then
		RETURN;
	end_if;
	Result := TRUE;
	if (GroupAxisMask and 0x00000001) <> 0 then
		if (IsClientConnected(#LMCAxis1) = 0) |
		   ((LMCAxis1.ReadAxisStatus()$UDINT and 0x00000001) = 0) then
			Result := FALSE;
		end_if;
	end_if;
	if (GroupAxisMask and 0x00000002) <> 0 then
		if (IsClientConnected(#LMCAxis2) = 0) |
		   ((LMCAxis2.ReadAxisStatus()$UDINT and 0x00000001) = 0) then
			Result := FALSE;
		end_if;
	end_if;
	if (GroupAxisMask and 0x00000004) <> 0 then
		if (IsClientConnected(#LMCAxis3) = 0) |
		   ((LMCAxis3.ReadAxisStatus()$UDINT and 0x00000001) = 0) then
			Result := FALSE;
		end_if;
	end_if;
	if (GroupAxisMask and 0x00000008) <> 0 then
		if (IsClientConnected(#LMCAxis4) = 0) |
		   ((LMCAxis4.ReadAxisStatus()$UDINT and 0x00000001) = 0) then
			Result := FALSE;
		end_if;
	end_if;

END_FUNCTION


FUNCTION LMCControlCommandService::SetResolvedGroupAxesPower
	VAR_INPUT
		GroupAxisMask 	: UDINT;
		Enable 	: BOOL;
	END_VAR
	VAR_OUTPUT
		Result 	: DINT;
	END_VAR
	VAR
		axisResult : DINT;
	END_VAR

	Result := 0;
	if (GroupAxisMask = 0) | (GroupAxisMask > LMC_OWNER_PROFILE_AXIS_MASK) then
		Result := -3;
		RETURN;
	end_if;

	if (GroupAxisMask and 0x00000001) <> 0 then
		axisResult := -2;
		if IsClientConnected(#LMCAxis1) = 1 then
			if Enable then
				axisResult := LMCAxis1.PowerOn(Mode:=LMCAXIS_MOVE_SHORTEST_WAY);
			else
				axisResult := LMCAxis1.PowerOff(Mode:=LMCAXIS_SMOOTH_STOPP);
			end_if;
		end_if;
		if (Result = 0) & (axisResult <> 0) then Result := axisResult; end_if;
	end_if;
	if (GroupAxisMask and 0x00000002) <> 0 then
		axisResult := -2;
		if IsClientConnected(#LMCAxis2) = 1 then
			if Enable then
				axisResult := LMCAxis2.PowerOn(Mode:=LMCAXIS_MOVE_SHORTEST_WAY);
			else
				axisResult := LMCAxis2.PowerOff(Mode:=LMCAXIS_SMOOTH_STOPP);
			end_if;
		end_if;
		if (Result = 0) & (axisResult <> 0) then Result := axisResult; end_if;
	end_if;
	if (GroupAxisMask and 0x00000004) <> 0 then
		axisResult := -2;
		if IsClientConnected(#LMCAxis3) = 1 then
			if Enable then
				axisResult := LMCAxis3.PowerOn(Mode:=LMCAXIS_MOVE_SHORTEST_WAY);
			else
				axisResult := LMCAxis3.PowerOff(Mode:=LMCAXIS_SMOOTH_STOPP);
			end_if;
		end_if;
		if (Result = 0) & (axisResult <> 0) then Result := axisResult; end_if;
	end_if;
	if (GroupAxisMask and 0x00000008) <> 0 then
		axisResult := -2;
		if IsClientConnected(#LMCAxis4) = 1 then
			if Enable then
				axisResult := LMCAxis4.PowerOn(Mode:=LMCAXIS_MOVE_SHORTEST_WAY);
			else
				axisResult := LMCAxis4.PowerOff(Mode:=LMCAXIS_SMOOTH_STOPP);
			end_if;
		end_if;
		if (Result = 0) & (axisResult <> 0) then Result := axisResult; end_if;
	end_if;

END_FUNCTION


"""
    if helper_impl not in text:
        text = replace_once(text, impl_anchor, helper_impl + impl_anchor, "helper implementations")

    old_locals = "\t\taxisCommandStatus : UINT;\n\t\taxisCommandErrorId : INT;\n\tEND_VAR"
    new_locals = """\t\taxisCommandStatus : UINT;
\t\taxisCommandErrorId : INT;
\t\tgroupAxisMask : UDINT;
\t\tgroupMemberCount : UDINT;
\t\tgroupAxis1Enable : DINT;
\t\tgroupAxis2Enable : DINT;
\t\tgroupAxis3Enable : DINT;
\t\tgroupAxis4Enable : DINT;
\tEND_VAR"""
    if new_locals not in text:
        text = replace_once(text, old_locals, new_locals, "HandleGroupCommands locals")

    member_block = r'''
		0x20D2:
			objectRegistryReady := FALSE;
			groupAxisMask := 0;
			groupMemberCount := 0;
			if RequestFrameSize = 9 then
				objectRegistryReady :=
					((pRequestFrame + 8)^$USINT = 1) &
					(Reference = 0x0100) &
					(IsClientConnected(#LMCRobot) = 1);
			end_if;
			if objectRegistryReady then
				groupAxisMask := ResolveConnectedGroupAxisMask();
				if groupAxisMask = 0 then objectRegistryReady := FALSE; end_if;
			end_if;
			if objectRegistryReady then
				if ResponseCapacity < 1358 then RETURN; end_if;
				_memset(dest:=pResponseFrame, usByte:=0, cntr:=1358);

				if (groupAxisMask and 0x00000001) <> 0 then
					_memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName));
					objectNameLength := _GetObjName(pThis:=LMCAxis1.pCmd, pName:=#objectName[0]);
					if (objectNameLength = 0) | (objectNameLength > 79) then objectRegistryReady := FALSE; end_if;
					(pResponseFrame + 8 + (groupMemberCount * 2))^$UINT := 1;
					(pResponseFrame + 40 + (groupMemberCount * 2))^$UINT := 0;
					_memcpy(ptr1:=pResponseFrame + 76 + (groupMemberCount * 80), ptr2:=#objectName[0], cntr:=80);
					groupMemberCount += 1;
				end_if;
				if (groupAxisMask and 0x00000002) <> 0 then
					_memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName));
					objectNameLength := _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#objectName[0]);
					if (objectNameLength = 0) | (objectNameLength > 79) then objectRegistryReady := FALSE; end_if;
					(pResponseFrame + 8 + (groupMemberCount * 2))^$UINT := 2;
					(pResponseFrame + 40 + (groupMemberCount * 2))^$UINT := 1;
					_memcpy(ptr1:=pResponseFrame + 76 + (groupMemberCount * 80), ptr2:=#objectName[0], cntr:=80);
					groupMemberCount += 1;
				end_if;
				if (groupAxisMask and 0x00000004) <> 0 then
					_memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName));
					objectNameLength := _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#objectName[0]);
					if (objectNameLength = 0) | (objectNameLength > 79) then objectRegistryReady := FALSE; end_if;
					(pResponseFrame + 8 + (groupMemberCount * 2))^$UINT := 3;
					(pResponseFrame + 40 + (groupMemberCount * 2))^$UINT := 2;
					_memcpy(ptr1:=pResponseFrame + 76 + (groupMemberCount * 80), ptr2:=#objectName[0], cntr:=80);
					groupMemberCount += 1;
				end_if;
				if (groupAxisMask and 0x00000008) <> 0 then
					_memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName));
					objectNameLength := _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#objectName[0]);
					if (objectNameLength = 0) | (objectNameLength > 79) then objectRegistryReady := FALSE; end_if;
					(pResponseFrame + 8 + (groupMemberCount * 2))^$UINT := 4;
					(pResponseFrame + 40 + (groupMemberCount * 2))^$UINT := 3;
					_memcpy(ptr1:=pResponseFrame + 76 + (groupMemberCount * 80), ptr2:=#objectName[0], cntr:=80);
					groupMemberCount += 1;
				end_if;
			end_if;

			if objectRegistryReady & (groupMemberCount > 0) then
				pResponseFrame^$UINT := 0;
				(pResponseFrame + 2)^$UINT := 1350;
				(pResponseFrame + 4)^$UDINT := 0;
				(pResponseFrame + 72)^$UINT := 0;
				(pResponseFrame + 74)^$INT := 0;
				(pResponseFrame + 1356)^$USINT := groupMemberCount$USINT;
				(pResponseFrame + 1357)^$USINT := 0;
				ResponseSize := 1358;
			else
				if ResponseCapacity < 12 then RETURN; end_if;
				_memset(dest:=pResponseFrame, usByte:=0, cntr:=12);
				pResponseFrame^$UINT := 1;
				(pResponseFrame + 2)^$UINT := 4;
				(pResponseFrame + 4)^$UDINT := 0;
				(pResponseFrame + 8)^$UINT := 1;
				(pResponseFrame + 10)^$INT := -3;
				ResponseSize := 12;
			end_if;

		0x2047:'''
    text = replace_block(text, "20D2", "2047", member_block, "GetGroupMembersInfo")

    enable_block = r'''
		0x2047:
			groupCommandInputValid := FALSE;
			if RequestFrameSize = 9 then
				groupCommandInputValid := ((pRequestFrame + 8)^$USINT = 1) & (Reference = 0x0100);
			end_if;
			if groupCommandInputValid = TRUE then
				if ResponseCapacity < 16 then RETURN; end_if;
				groupReadErrorId := -2;
				groupAxisMask := ResolveConnectedGroupAxisMask();
				if (IsClientConnected(#LMCRobot) = 1) & (groupAxisMask <> 0) then
					groupReadErrorId := -6;
					if AreResolvedGroupAxesPowered(GroupAxisMask:=groupAxisMask) then
						groupAxis1Enable := 0; groupAxis2Enable := 0;
						groupAxis3Enable := 0; groupAxis4Enable := 0;
						if (groupAxisMask and 0x1) <> 0 then groupAxis1Enable := 1; end_if;
						if (groupAxisMask and 0x2) <> 0 then groupAxis2Enable := 1; end_if;
						if (groupAxisMask and 0x4) <> 0 then groupAxis3Enable := 1; end_if;
						if (groupAxisMask and 0x8) <> 0 then groupAxis4Enable := 1; end_if;
						OwnershipState[26] := TO_DINT(CommandId);
						OwnershipState[27] := TO_DINT(Reference);
						OwnershipState[25] := 1;
						groupReadRetCode := LMCRobot.LockProfile(
							Axis1:=groupAxis1Enable, Axis2:=groupAxis2Enable,
							Axis3:=groupAxis3Enable, Axis4:=groupAxis4Enable,
							Axis5:=0, Axis6:=0, Axis7:=0, Axis8:=0, Axis9:=0);
						if groupReadRetCode = _LMCPROF_NoError then
							groupReadErrorId := 0;
						elsif groupReadRetCode$UDINT <= 32767 then
							groupReadErrorId := groupReadRetCode$DINT;
						end_if;
					end_if;
				end_if;
				_memset(dest:=pResponseFrame, usByte:=0, cntr:=16);
				pResponseFrame^$UINT := 0;
				(pResponseFrame + 2)^$UINT := 8;
				(pResponseFrame + 4)^$UDINT := 0;
				(pResponseFrame + 8)^$UDINT := TO_UDINT(Reference);
				if groupReadErrorId = 0 then
					(pResponseFrame + 12)^$UINT := 0; (pResponseFrame + 14)^$INT := 0;
				elsif (groupReadErrorId >= -32768) & (groupReadErrorId <= 32767) then
					(pResponseFrame + 12)^$UINT := 1; (pResponseFrame + 14)^$INT := groupReadErrorId$INT;
				else
					(pResponseFrame + 12)^$UINT := 1; (pResponseFrame + 14)^$INT := -6;
				end_if;
				ResponseSize := 16;
			else
				if ResponseCapacity < 12 then RETURN; end_if;
				_memset(dest:=pResponseFrame, usByte:=0, cntr:=12);
				pResponseFrame^$UINT := 1; (pResponseFrame + 2)^$UINT := 4;
				(pResponseFrame + 4)^$UDINT := 0; (pResponseFrame + 8)^$UINT := 1;
				(pResponseFrame + 10)^$INT := -3; ResponseSize := 12;
			end_if;

		0x2048:'''
    text = replace_block(text, "2047", "2048", enable_block, "GroupEnable")

    def power_block(command: str, next_command: str, enable: bool) -> str:
        enable_text = "TRUE" if enable else "FALSE"
        return f'''\n\t\t0x{command}:\n\t\t\tgroupCommandInputValid := FALSE;\n\t\t\tif RequestFrameSize = 9 then\n\t\t\t\tgroupCommandInputValid := ((pRequestFrame + 8)^$USINT = 1) & (Reference = 0x0100);\n\t\t\tend_if;\n\t\t\tif groupCommandInputValid = TRUE then\n\t\t\t\tif ResponseCapacity < 16 then RETURN; end_if;\n\t\t\t\tgroupReadErrorId := -2;\n\t\t\t\tgroupAxisMask := ResolveConnectedGroupAxisMask();\n\t\t\t\tif groupAxisMask <> 0 then\n\t\t\t\t\tOwnershipState[26] := TO_DINT(CommandId);\n\t\t\t\t\tOwnershipState[27] := TO_DINT(Reference);\n\t\t\t\t\tOwnershipState[25] := 1;\n\t\t\t\t\tgroupReadErrorId := SetResolvedGroupAxesPower(GroupAxisMask:=groupAxisMask, Enable:={enable_text});\n\t\t\t\tend_if;\n\t\t\t\t_memset(dest:=pResponseFrame, usByte:=0, cntr:=16);\n\t\t\t\tpResponseFrame^$UINT := 0; (pResponseFrame + 2)^$UINT := 8;\n\t\t\t\t(pResponseFrame + 4)^$UDINT := 0; (pResponseFrame + 8)^$UDINT := TO_UDINT(Reference);\n\t\t\t\tif groupReadErrorId = 0 then\n\t\t\t\t\t(pResponseFrame + 12)^$UINT := 0; (pResponseFrame + 14)^$INT := 0;\n\t\t\t\telsif (groupReadErrorId >= -32768) & (groupReadErrorId <= 32767) then\n\t\t\t\t\t(pResponseFrame + 12)^$UINT := 1; (pResponseFrame + 14)^$INT := groupReadErrorId$INT;\n\t\t\t\telse\n\t\t\t\t\t(pResponseFrame + 12)^$UINT := 1; (pResponseFrame + 14)^$INT := -6;\n\t\t\t\tend_if;\n\t\t\t\tResponseSize := 16;\n\t\t\telse\n\t\t\t\tif ResponseCapacity < 12 then RETURN; end_if;\n\t\t\t\t_memset(dest:=pResponseFrame, usByte:=0, cntr:=12);\n\t\t\t\tpResponseFrame^$UINT := 1; (pResponseFrame + 2)^$UINT := 4;\n\t\t\t\t(pResponseFrame + 4)^$UDINT := 0; (pResponseFrame + 8)^$UINT := 1;\n\t\t\t\t(pResponseFrame + 10)^$INT := -3; ResponseSize := 12;\n\t\t\tend_if;\n\n\t\t0x{next_command}:'''

    text = replace_block(text, "204A", "204B", power_block("204A", "204B", True), "GroupPowerOn")
    text = replace_block(text, "204B", "2085", power_block("204B", "2085", False), "GroupPowerOff")

    old_group_power = "\t\t\t\t\t\t\t\tgroupPowerState := LMCRobot.RobotIsOn();"
    new_group_power = """\t\t\t\t\t\t\t\tif AreResolvedGroupAxesPowered(
\t\t\t\t\t\t\t\t   GroupAxisMask:=ResolveConnectedGroupAxisMask()) then
\t\t\t\t\t\t\t\t\tgroupPowerState := 1;
\t\t\t\t\t\t\t\telse
\t\t\t\t\t\t\t\t\tgroupPowerState := 0;
\t\t\t\t\t\t\t\tend_if;"""
    if old_group_power in text:
        text = replace_once(text, old_group_power, new_group_power, "ownership group power observer")

    # All remaining RobotIsOn uses are aggregate group-power evidence.  Cartesian
    # motion still keeps its existing kinematic gate, so this changes only the
    # source of the power proof after power orchestration became per-axis.
    robot_power = "powerIsOn := LMCRobot.RobotIsOn();"
    aggregate_power = """if AreResolvedGroupAxesPowered(GroupAxisMask:=ResolveConnectedGroupAxisMask()) then
\t\t\t\tpowerIsOn := 1;
\t\t\telse
\t\t\t\tpowerIsOn := 0;
\t\t\tend_if;"""
    text = text.replace(robot_power, aggregate_power)

    CONTROL.write_text(text, encoding="utf-8")

    final = CONTROL.read_text(encoding="utf-8")
    required = [
        "FUNCTION LMCControlCommandService::ResolveConnectedGroupAxisMask",
        "InputLatch.CopySnapshot(",
        "groupSnapshot[92]$UDINT = cycleCounter",
        "FUNCTION LMCControlCommandService::SetResolvedGroupAxesPower",
        "Axis1:=groupAxis1Enable",
        "(pResponseFrame + 1356)^$USINT := groupMemberCount$USINT;",
        "SetResolvedGroupAxesPower(GroupAxisMask:=groupAxisMask, Enable:=TRUE)",
        "SetResolvedGroupAxesPower(GroupAxisMask:=groupAxisMask, Enable:=FALSE)",
    ]
    missing = [item for item in required if item not in final]
    if missing:
        raise SystemExit("missing dynamic group contracts: " + ", ".join(missing))
    if "Axis1:=1, Axis2:=1, Axis3:=1, Axis4:=1" in final:
        raise SystemExit("fixed four-axis LockProfile contract still present")


if __name__ == "__main__":
    main()
