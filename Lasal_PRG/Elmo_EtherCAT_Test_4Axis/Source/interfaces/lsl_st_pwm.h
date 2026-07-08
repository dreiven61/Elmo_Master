#ifndef  __PWM_H
#pragma once
#define  __PWM_H

// Interface
#define INTERFACE_PWM				"PWM"

// IDs of PWM Outputs
#define SIGM_INOUTPUT_PWM_1			0x3001
#define SIGM_INOUTPUT_PWM_2			0x3002
#define SIGM_INOUTPUT_PWM_3			0x3003
#define SIGM_INOUTPUT_PWM_4			0x3004

TYPE
	#pragma pack (push, 1)
	LSL_PWM_TYPE : STRUCT
		version	: UDINT;
		PWM_Config : pVoid;
		PWM_Enable : pVoid;
		PWM_Disable : pVoid;
	END_STRUCT;
	#pragma pack (pop)
END_TYPE

////////////////////////////////////////////////////////////////////////////////
// LASAL Function Prototypes
////////////////////////////////////////////////////////////////////////////////
FUNCTION __CDECL GLOBAL P_PWM_Config
	VAR_INPUT
		pwmID	: UDINT;
		duty_ns	: DINT;
		period_ns	: DINT;
	END_VAR
	VAR_OUTPUT
		retval	: DINT;
	END_VAR;

FUNCTION __CDECL GLOBAL P_PWM_Enable
	VAR_INPUT
		pwmID	: UDINT;
	END_VAR
	VAR_OUTPUT
		retval 	: DINT;
	END_VAR;

FUNCTION __CDECL GLOBAL P_PWM_Disable
	VAR_INPUT
		pwmID	: UDINT;
	END_VAR
	VAR_OUTPUT
		retval 	: DINT;
	END_VAR;

////////////////////////////////////////////////////////////////////////////////
// LASAL MACROS
////////////////////////////////////////////////////////////////////////////////
#define OS_PWM_CONFIG(pPWM,p1,p2,p3) pPWM^.PWM_Config $ P_PWM_Config(p1,p2,p3)
#define OS_PWM_ENABLE(pPWM,p1) pPWM^.PWM_Enable $ P_PWM_Enable(p1)
#define OS_PWM_DISABLE(pPWM,p1) pPWM^.PWM_Disable $ P_PWM_Disable(p1)


#endif
