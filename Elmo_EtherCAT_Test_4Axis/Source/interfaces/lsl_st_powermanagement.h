#ifndef  __POWERMANAGEMENT_H
#pragma once
#define  __POWERMANAGEMENT_H

// Interface
#define INTERFACE_POWERMANAGEMENT                       "POWERMANAGEMENT"

#define POWERMANAGEMENT_REBOOT_WAIT_ENDLESS 	0x01

TYPE
	#pragma pack (push, 1)
	LSL_POWERMANAGEMENT_TYPE : STRUCT
		version	: UDINT;
		PM_Reboot: pVoid;
	END_STRUCT;
	#pragma pack (pop)
END_TYPE

////////////////////////////////////////////////////////////////////////////////
// LASAL Function Prototypes
////////////////////////////////////////////////////////////////////////////////
// Callback Prototype
	
FUNCTION __CDECL GLOBAL P_PM_Reboot
	VAR_INPUT
		flags	: UDINT;
	END_VAR
	VAR_OUTPUT
		retval	: DINT;
	END_VAR;

////////////////////////////////////////////////////////////////////////////////
// LASAL MACROS
////////////////////////////////////////////////////////////////////////////////

#define OS_PM_Reboot(pPowerManagement,p1) pPowerManagement^.PM_Reboot $ P_PM_Reboot(p1)

#endif