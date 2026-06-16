# Continuation - 24.8 The MMCGroupAxis class

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 2095-2134
- Chunk: `080_p2095-p2134_Continuation-24.8-The-MMCGroupAxis-class.md`

## Active Outline At Chunk Start
- p. 1705 - Chapter 24 Programming in C++
  - p. 2055 - 24.8 The MMCGroupAxis class

## Extracted Text

### PDF page 2095
<a id="pdf-page-2095"></a>
// 15.4.14.GroupSetOverride
// 15.4.15.GroupSetPosition
void GroupOverrideAndPosition(void)
// ===================================
{
double dbPosition[3];
float fVelFactor,
fAccFactor,
fJerkFactor;

float fVelocity,
fAcceleration,
fDeceleration,
fJerk;

unsigned short usUpdateVelFactorIdx;
int ind,
rt_val;

printf("\n Function: %s: ", __func__);

// GroupSetPosition is Not released yet... - Not supported
//
// MC_COORD_SYSTEM_ENUM eCoordSystem;
// unsigned char ucMode;
//
// Group.m_dEndPoint[0] = 1000000.0;
// Group.m_dEndPoint[1] = 2000000.0;
// Group.m_dEndPoint[2] = 0.0;
// ParamsGroup.eTransitionMode = MC_TM_NONE_MODE;
// Group.SetDefaultParams(ParamsGroup);
// Group.MoveLinearAbsolute(MC_ABORTING_MODE);

// dbPosition[0] = 500000.0;
// dbPosition[1] = 750000.0;
// dbPosition[2] = 0.0;
// eCoordSystem = MC_MCS_COORD;
/* RELATIVE =True, ABSOLUTE = False (Default) */
// ucMode = ABSOLUTE;
/* MC_ABORTING_MODE is the defalut mode */
// Group.GroupSetPosition(dbPosition, eCoordSystem, ucMode);

fVelocity = 100000.0;
fAcceleration = fDeceleration = 10000000;
fJerk = 100000000.0;
usUpdateVelFactorIdx = 0; /* Meanwhile only 0 is support */
dbPosition[2] = 0.0;

for (ind=0; ind<6; ind++)
{
if (ind==2)
{
WaitGrpDone(NC_GROUP_STANDBY_MASK);
fVelFactor = 0.5;
fAccFactor = 0.5;
fJerkFactor= 0.5;
rt_val = Group.GroupSetOverride(fVelFactor, fAccFactor,
fJerkFactor, usUpdateVelFactorIdx);

### PDF page 2096
<a id="pdf-page-2096"></a>
}

if (ind==4)
{
WaitGrpDone(NC_GROUP_STANDBY_MASK);
fVelFactor = 1.0;
fAccFactor = 1.0;
fJerkFactor= 1.0;
rt_val = Group.GroupSetOverride(fVelFactor, fAccFactor,
fJerkFactor, usUpdateVelFactorIdx);
}

dbPosition[0] = 0.0;
dbPosition[1] = 0.0;
Group.MoveLinearAbsolute(fVelocity, dbPosition, fAcceleration,
fDeceleration, fJerk, MC_BUFFERED_MODE);
dbPosition[0] = 100000.0;
dbPosition[1] = 100000.0;
Group.MoveLinearAbsolute(fVelocity, dbPosition, fAcceleration,
fDeceleration, fJerk, MC_BUFFERED_MODE);
}

WaitGrpDone(NC_GROUP_STANDBY_MASK);
}

int CallbackFunc(unsigned char* recvBuffer, short recvBufferSize, void*
lpsock)
//
=========================================================================
{
printf("\n *********** STR Func: %s *********** ", __func__);

/* Which function ID was received ... */
switch(recvBuffer[1])
{
case ASYNC_REPLY_EVT:
printf("\n ASYNC event Reply ");
break ;
case EMCY_EVT:
printf("\n Emergency Event received ");
break ;
case MOTIONENDED_EVT:
printf("\n Motion Ended Event received ");
break ;
case HBEAT_EVT:
printf("\n H Beat Fail Event received ");
break ;
case PDORCV_EVT:
printf("\n PDO Received Event received - Updating Inputs ");
break ;
case DRVERROR_EVT:
printf("\n Drive Error Received Event received ");
break ;
case HOME_ENDED_EVT:
printf("\n Home Ended Event received ");
break ;
case SYSTEMERROR_EVT:
printf("\n System Error Event received ");
case TABLE_UNDERFLOW_EVT:

### PDF page 2097
<a id="pdf-page-2097"></a>
printf("\n Underflow event received ");
break ;
case MODBUS_WRITE_EVT:
printf("\n ModBus Write event received ");
break ;
case TOUCH_PROBE_ENDED_EVT:
printf("\n Touch Probe event received ");
break ;
default:
printf("\n Default.... Whatever arrived event received ");
break;
}

printf("\n *********** END Func: %s *********** ", __func__);
fflush(stdout); fflush(stderr);

return 1 ;
}

int OnRunTimeError(const char *msg, unsigned int uiConnHndl, unsigned
short usAxisRef, short sErrorID, unsigned short usStatus)
//
=========================================================================
{
printf("\n APP: MMCPPExitClbk: Run time Error in function %s, axis
ref=%d, err=%d, status=%d, bye\n",
msg, usAxisRef, sErrorID, usStatus);
fflush(stdout); fflush(stderr);
MMC_CloseConnection(uiConnHndl);
exit(0);
}
/*================ Example functions END ===============================*/

/*================ Output STR ==========================================*/
#ifdef PROGRAM_OUTPUT
Output example EAS movment record:

#endif /* PROGRAM_OUTPUT */
/*================ Output END ========================================*/

### PDF page 2098
<a id="pdf-page-2098"></a>
24.8.2 MMCGroupAxis Class Functions Code Example 4
The following is an example of group axes motion in 3-D using the MoveCircularXXXXXXX set of functions.
/*
=========================================================================
Collection of Gmas API functions (Set #5)
Examples for document: "G-MAS Administrative and Motion API.pdf".
12Sep2013
Haim Hillel
========================================================================
*/

#include <iostream>

#include "MMC_Definitions.h"
#include "mmcpplib.h"

#define EndMotionEventCB_MESSAGE "!!!!END MOTION EVENT MESSAGE!!!"

#ifdef WIN32
#define WAIT_SLEEP_MILLI(WAIT_MILLI_SEC) Sleep(WAIT_MILLI_SEC);
#else
#define WAIT_SLEEP_MILLI(WAIT_MILLI_SEC) usleep(WAIT_MILLI_SEC*1000);
#endif

using namespace std;

CMMCConnection gConn;
MMC_CONNECT_HNDL ComHndl;

CMMCSingleAxis AxisX, AxisY, AxisZ;
unsigned short AxisXRef, AxisYRef, AxisZRef;

CMMCGroupAxis Group;
double S_Factor_For_Polynomial_Transition;
MMC_MOTIONPARAMS_GROUP ParamsGroup;

MMC_SETKINTRANSFORM_IN SetKin;

double StarPoi[][2] =
{
{ 7500., 0.},
{12500., 13100.},
{ 200., 5600.},
{14700., 5600.},
{ 3340., 13750.},
{ 7500., 7500.} // the Center cordination
};

char * delimit =
"=======================================================================";
char * strStrSnro = "\n\n\n <<<<<<<<<<<<< Start ";
char * strEndSnro1 = "\n End ";
char * strEndSnro2 = " >>>>>>>>>>>>> ";

### PDF page 2099
<a id="pdf-page-2099"></a>
int WaitFbDone(unsigned int break_state, CMMCSingleAxis *
sng_axis);

void initAdminMultiAxis();
void endAdminMultiAxis(void);

void SnroMoveComplex3d(int);
void MoveCircularAbsolute3d(void);

int CallbackFunc(unsigned char* recvBuffer, short
recvBufferSize,void* lpsock);
int OnRunTimeError(const char *msg, unsigned int uiConnHndl,
unsigned short usAxisRef, short sErrorID, unsigned short usStatus);

/*========================= Administration functions STR
==================================*/

int main(int)
// ==============
{
int trace = 1;

printf("\n %s", delimit);
printf("\n %s %s %s \n", __FILE__, __DATE__, __TIME__);

try
{
SnroMoveComplex3d(trace++);

}
catch (CMMCException excp)
{
printf("\n %s", delimit);
printf("\n %s", delimit);
printf("\n ERROR: Axis=%d <%s> error=%d, status=%d.
",excp.axisRef(), excp.what(), (short)excp.error(), excp.status());
printf("\n %s", delimit);
printf("\n %s", delimit);
exit(0);
}

printf("\n End of %s ", __FILE__);
printf("\n %s\n\n", delimit);
return 0;
}

int WaitFbDone(unsigned int break_state, CMMCSingleAxis * sng_axis)
//=====================================================================
{
int end_of = 0;
int iCount = 0;
unsigned int ulState;

while( ! end_of)
{
iCount ++;
end_of = 1;
/* Read Axis Status command server for specific Axis */

### PDF page 2100
<a id="pdf-page-2100"></a>
ulState = sng_axis->ReadStatus();
if (!(ulState & break_state))
{
end_of = 0;

WAIT_SLEEP_MILLI(20)
}
}

// MMC_SHOWNODESTAT_IN showin;
// MMC_SHOWNODESTAT_OUT showout;
// MMC_ShowNodeStatCmd(ComHndl, sng_axis->GetRef(), &showin,
&showout);

return 0;
}

void WaitGrpDone(unsigned int groupStatusMsk)
// =============================================
{
unsigned int uiStatusRegister;

uiStatusRegister = Group.GroupReadStatus();
while((uiStatusRegister & groupStatusMsk) != groupStatusMsk)
{
WAIT_SLEEP_MILLI(2)
uiStatusRegister = Group.GroupReadStatus();
}
}

void initAdminMultiAxis()
// =========================
{
/* Source class: */
/* MMC_CONNECT_HNDL ComHndl; */
/* CMMCSingleAxis AxisX, AxisY; */
/* CMMCGroupAxis Group; */
int iEventMask;

printf("\n Function: %s: ", __func__);

/* CallbackFunc in ConnectIPCEx call if there */
/* is no calling to 'RegisterEventCallback' */
iEventMask = 0x7fffffff;
ComHndl = gConn.ConnectIPCEx(iEventMask, (MMC_MB_CLBK)CallbackFunc);
/* Put Null param Val for no CallbackFunc */
/* ComHndl = gConn.ConnectIPCEx(iEventMask, NULL); */
/* Should Not calling, called inside 'ConnectIPCEx' */
/* rt_val = MMC_OpenUdpChannelCmdEx(g_ComHndl, &openudp_param_in,
&openudp_param_out); */

/* Register Run Time Error Callback function */
CMMCPPGlobal::Instance()->RegisterRTE(OnRunTimeError);

### PDF page 2101
<a id="pdf-page-2101"></a>
AxisX.InitAxisData("a01", ComHndl);
AxisY.InitAxisData("a02", ComHndl);
AxisZ.InitAxisData("a03", ComHndl);

Group.InitAxisData("v01", ComHndl);

AxisXRef = AxisX.GetRef();
AxisYRef = AxisY.GetRef();
AxisZRef = AxisZ.GetRef();

/* Set by default the EndPoint=StartPoint */
ParamsGroup.dEndPoint[0] = 0.0;
ParamsGroup.dEndPoint[1] = 0.0;
ParamsGroup.dEndPoint[2] = 0.0;

ParamsGroup.fVelocity = 100000.0;
ParamsGroup.fAcceleration = 8000000.0;
ParamsGroup.fDeceleration = 8000000.0;
ParamsGroup.fJerk = 100000000.0;
ParamsGroup.eCoordSystem = MC_MCS_COORD;
ParamsGroup.eBufferMode = MC_BUFFERED_MODE;
// ParamsGroup.eTransitionMode = MC_TM_CORNER_DEVIATION_MODE_PLN6;
ParamsGroup.eTransitionMode = MC_TM_NONE_MODE;
ParamsGroup.fTransitionParameter[0] = 2000.0;
ParamsGroup.ucExecute = 1;

Group.SetDefaultParams(ParamsGroup);

/* Parameters for Kinematic Transformation */
SetKin.eBufferMode = MC_BUFFERED_MODE;
SetKin.eType[0] = NC_X_AXIS_TYPE;
SetKin.eType[1] = NC_Y_AXIS_TYPE;
SetKin.eType[2] = NC_Z_AXIS_TYPE;

SetKin.hNode[0] = AxisX.GetRef();
SetKin.hNode[1] = AxisY.GetRef();
SetKin.hNode[2] = AxisZ.GetRef();

SetKin.iMcsToAcsFuncID[0] = NC_TR_SHIFT_FUNC;
SetKin.iMcsToAcsFuncID[1] = NC_TR_SHIFT_FUNC;
SetKin.iMcsToAcsFuncID[2] = NC_TR_SHIFT_FUNC;

SetKin.iNumAxes = 3;
SetKin.ucExecute = 1;

SetKin.ulTrCoef[0][0] = 1;
SetKin.ulTrCoef[0][1] = 1;
SetKin.ulTrCoef[0][2] = 1;

SetKin.ulTrCoef[1][0] = 1;
SetKin.ulTrCoef[1][1] = 1;
SetKin.ulTrCoef[1][2] = 1;

SetKin.ulTrCoef[2][0] = 1;
SetKin.ulTrCoef[2][1] = 1;
SetKin.ulTrCoef[2][2] = 1;

### PDF page 2102
<a id="pdf-page-2102"></a>
Group.SetKinTransform(SetKin);

/* Set the factor for Polynominal Transition */
S_Factor_For_Polynomial_Transition = 0.4; // The default is 1.4

/* SetParameter(double dbValue, MMC_PARAMETER_LIST_ENUM eNumber, int
iIndex); */
Group.SetParameter(S_Factor_For_Polynomial_Transition, MMC_S_FACTOR,
0);
}

void endAdminMultiAxis(void)
// ================================
{
// Source class:
// CMMCGroupAxis Group;

printf("\n Function: %s: ", __func__);

MMC_CloseConnection(ComHndl) ;

// The two functions below can be called as shown, or called from the EAS
application (configuration file), but, if define in configuration (EAS
show axis in V01 group)
// they shuld not be call here! (the axis is already in group...)
// Group.RemoveAxisFromGroup(NC_NODE_1_ID);
// Group.RemoveAxisFromGroup(NC_NODE_2_ID);
// Group.RemoveAxisFromGroup(NC_NODE_3_ID);
}
/*============= Administration functions END ===========================*/

/*============== Scenario functions STR ===============================*/

void SnroMoveComplex3d(int trace)
// ==============================
{
printf("%s%s -%d- ", strStrSnro, __func__, trace);

initAdminMultiAxis();

AxisX.PowerOn(MC_BUFFERED_MODE);
AxisY.PowerOn(MC_BUFFERED_MODE);
AxisZ.PowerOn(MC_BUFFERED_MODE);
WaitFbDone(NC_AXIS_STAND_STILL_MASK, &AxisX);
WaitFbDone(NC_AXIS_STAND_STILL_MASK, &AxisY);
WaitFbDone(NC_AXIS_STAND_STILL_MASK, &AxisZ);

Group.GroupEnable();

MoveCircularAbsolute3d();

Group.GroupDisable();

AxisY.PowerOff(MC_BUFFERED_MODE);
AxisX.PowerOff(MC_BUFFERED_MODE);
AxisZ.PowerOff(MC_BUFFERED_MODE);

### PDF page 2103
<a id="pdf-page-2103"></a>
WaitFbDone(NC_AXIS_DISABLED_MASK, &AxisY);
WaitFbDone(NC_AXIS_DISABLED_MASK, &AxisX);
WaitFbDone(NC_AXIS_DISABLED_MASK, &AxisZ);

endAdminMultiAxis();

printf("%s%s -%d- %s", strEndSnro1, __func__, trace, strEndSnro2);
}

/*============ Example functions STR =================================*/

// 15.4.7. MoveCircularAbsolute
// 15.4.8. MoveCircularAbsoluteCenter
// 15.4.9. MoveCircularAbsoluteBorder
// 15.4.10.MoveCircularAbsoluteRadius
void MoveCircularAbsolute3d(void)
// ================================
{
int rt_val;
NC_ARC_SHORT_LONG_ENUM eArcShortLong;
NC_PATH_CHOICE_ENUM ePathChoice;
NC_CIRC_MODE_ENUM eCircleMode;
double dAuxPoint[NC_MAX_NUM_AXES_IN_NODE];
double dCenterPoint[NC_MAX_NUM_AXES_IN_NODE];
double dBorderPoint[NC_MAX_NUM_AXES_IN_NODE];
MC_BUFFERED_MODE_ENUM eBufferMode;

printf("\n Function: %s: ", __func__);

ParamsGroup.fVelocity = 100000.0;

Group.m_dEndPoint[0] = 0.0;
Group.m_dEndPoint[1] = 0.0;
Group.m_dEndPoint[2] = 0.0;

Group.MoveLinearAbsolute(MC_ABORTING_MODE);

eArcShortLong = MC_NONE_ARC_CHOICE; // MC_LONG;
ePathChoice = MC_NONE_PATH_CHOICE; // MC_CLOCKWISE
MC_COUNTERCLOCKWISE MC_CLOCKWISE;
eCircleMode = MC_BORDER_CIRC_MODE;
eBufferMode = MC_BUFFERED_MODE; // MC_ABORTING_MODE = the
defalut

Group.m_dAuxPoint[0] = 20000.0; // Point on border
Group.m_dAuxPoint[1] = 0.0;
Group.m_dAuxPoint[2] = 0.0;

Group.m_dEndPoint[0] = 10000.0; // End point
Group.m_dEndPoint[1] = 0.0;
Group.m_dEndPoint[2] =-10000.0;

rt_val = Group.MoveCircularAbsolute(eArcShortLong, ePathChoice,
eCircleMode, eBufferMode);

dAuxPoint[0] = 10000.0; // Circular Center point

### PDF page 2104
<a id="pdf-page-2104"></a>
dAuxPoint[1] = 0.0;
dAuxPoint[2] = 0.0;

Group.m_dEndPoint[0] = 10000.0; // Circular End point, Circular start
point = (end of last motion)
Group.m_dEndPoint[1] = -10000.0;
Group.m_dEndPoint[2] = 0.0;

eCircleMode = MC_CENTER_CIRC_MODE;
eArcShortLong = MC_LONG;
rt_val = Group.MoveCircularAbsolute(eArcShortLong, ePathChoice,
eCircleMode, dAuxPoint, eBufferMode);

Group.m_dAuxPoint[0] = 10000.0; // Circular center
Group.m_dAuxPoint[1] = -10000.0;
Group.m_dAuxPoint[2] = 5000.0;

Group.m_dEndPoint[0] = 5000.0; // Circular end point
Group.m_dEndPoint[1] = -10000.0;
Group.m_dEndPoint[2] = 5000.0;

// 2D & 3D
rt_val = Group.MoveCircularAbsoluteCenter(eArcShortLong, eBufferMode);

Group.m_dEndPoint[0] = -20000.0; // Start point of
MoveCircularAbsoluteCenter... (End point of MoveLinearAbs)
Group.m_dEndPoint[1] = 0.0;
Group.m_dEndPoint[2] = -20000.0;
rt_val = Group.MoveLinearAbsolute(MC_BUFFERED_MODE);

eArcShortLong = MC_LONG;

dCenterPoint[0] = 0.0; // Circular center
dCenterPoint[1] = 0.0;
dCenterPoint[2] = 0.0;

Group.m_dEndPoint[0] = -20000.0; // Circular end point
Group.m_dEndPoint[1] = -20000.0;
Group.m_dEndPoint[2] = 0.0;

eBufferMode = MC_BUFFERED_MODE;
// 2D & 3D
rt_val = Group.MoveCircularAbsoluteCenter(eArcShortLong, dCenterPoint,
eBufferMode);

// Start point, end of previous move
dBorderPoint[0] = -20000.0; // Circular border (end of
previous).
dBorderPoint[1] = -40000.0;
dBorderPoint[2] = 0.0;

Group.m_dEndPoint[0] = -20000.0; // Circular end point
Group.m_dEndPoint[1] = -10000.0;
Group.m_dEndPoint[2] = -20000.0;
// 2D & 3D
rt_val = Group.MoveCircularAbsoluteBorder(dBorderPoint, eBufferMode);

Group.m_dEndPoint[0] = 10000.0; // Start point
Group.m_dEndPoint[1] = 0.0;

### PDF page 2105
<a id="pdf-page-2105"></a>
Group.m_dEndPoint[2] = 0.0;
Group.MoveLinearAbsolute(MC_BUFFERED_MODE);

Group.m_dEndPoint[0] = 0.0; // End point
Group.m_dEndPoint[1] = 10000.0;
Group.m_dEndPoint[2] = 0.0;

Group.m_dAuxPoint[0] = 0.0; // Radios location
Group.m_dAuxPoint[1] = 0.0;
Group.m_dAuxPoint[2] = 10000.0;

ePathChoice = MC_CLOCKWISE;
// 3D (not 2D)
rt_val = Group.MoveCircularAbsoluteRadius(eArcShortLong, ePathChoice,
eBufferMode);

WaitGrpDone(NC_GROUP_STANDBY_MASK);
}

int CallbackFunc(unsigned char* recvBuffer, short recvBufferSize, void*
lpsock)
//
========================================================================
{
printf("\n *********** STR Func: %s *********** ", __func__);

/* Which function ID was received ... */
switch(recvBuffer[1])
{
case ASYNC_REPLY_EVT:
printf("\n ASYNC event Reply ");
break ;
case EMCY_EVT:
printf("\n Emergency Event received ");
break ;
case MOTIONENDED_EVT:
printf("\n Motion Ended Event received ");
break ;
case HBEAT_EVT:
printf("\n H Beat Fail Event received ");
break ;
case PDORCV_EVT:
printf("\n PDO Received Event received - Updating Inputs ");
break ;
case DRVERROR_EVT:
printf("\n Drive Error Received Event received ");
break ;
case HOME_ENDED_EVT:
printf("\n Home Ended Event received ");
break ;
case SYSTEMERROR_EVT:
printf("\n System Error Event received ");
case TABLE_UNDERFLOW_EVT:
printf("\n Underflow event received ");
break ;
case MODBUS_WRITE_EVT:
printf("\n ModBus Write event received ");
break ;

### PDF page 2106
<a id="pdf-page-2106"></a>
case TOUCH_PROBE_ENDED_EVT:
printf("\n Touch Probe event received ");
break ;
default:
printf("\n Default.... Whatever arrived event received ");
break;
}

printf("\n *********** END Func: %s *********** ", __func__);
fflush(stdout); fflush(stderr);

return 1 ;
}

int OnRunTimeError(const char *msg, unsigned int uiConnHndl, unsigned
short usAxisRef, short sErrorID, unsigned short usStatus)
//
========================================================================
{
printf("\n APP: MMCPPExitClbk: Run time Error in function %s, axis
ref=%d, err=%d, status=%d, bye\n",
msg, usAxisRef, sErrorID, usStatus);
fflush(stdout); fflush(stderr);

MMC_CloseConnection(uiConnHndl);
exit(0);
}

/*================= Example functions END ==========================*/

/*================= Output STR ===================================*/
#ifdef PROGRAM_OUTPUT
Output example EAS movment record:

#endif /* PROGRAM_OUTPUT */
/*================== Output END =================================*/

### PDF page 2107
<a id="pdf-page-2107"></a>
24.8.3 MMCGroupAxis(MMCGroupAxis& axis)
This function initiates the Group axis and includes the function InitAxisData that initiates the axis name and
retrieves a session handler, and GetGroupAxisByName which access es the group function name. Refer to the
section MMC_GetAxisByName for details of the description, scope, and motion mode.
public:
CMMCGroupAxis();
virtual ~CMMCGroupAxis();
CMMCGroupAxis(CMMCGroupAxis& axis);

void InitAxisData(const char* cName, MMC_CONNECT_HNDL uHandle) throw
(CMMCException);

int GetGroupAxisByName(const char* cName) throw (CMMCException);

double m_dAuxPoint[NC_MAX_NUM_AXES_IN_NODE];
double m_dEndPoint[NC_MAX_NUM_AXES_IN_NODE];
float m_fVelocity;
float m_fAcceleration;
float m_fDeceleration;
float m_fJerk;
float m_fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];
MC_COORD_SYSTEM_ENUM m_eCoordSystem;
NC_TRANSITION_MODE_ENUM m_eTransitionMode;
NC_ARC_SHORT_LONG_ENUM m_eArcShortLong;
NC_PATH_CHOICE_ENUM m_ePathChoice;
NC_CIRC_MODE_ENUM m_eCircleMode;
unsigned char m_ucSuperimposed;
unsigned char m_ucExecute;
unsigned int m_uiExecDelayMs;
Source GMAS\includes\CPP\CMMCGroupAxis.h
.NET Definition
Parameters
void InitAxisData(const char* cName, MMC_CONNECT_HNDL uHandle) throw (CMMCException)
Refer to the next function InitAxisData for details.
int GetGroupAxisByName(const char* cName) throw (CMMCException)
Refer to the next function GetGroupAxisByName for details.
double m_dAuxPoint[NC_MAX_NUM_AXES_IN_NODE];

### PDF page 2108
<a id="pdf-page-2108"></a>
double m_dEndPoint[NC_MAX_NUM_AXES_IN_NODE];
float m_fVelocity;
float m_fAcceleration;
float m_fDeceleration;
float m_fJerk;
float m_fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];
MC_COORD_SYSTEM_ENUM m_eCoordSystem;
NC_TRANSITION_MODE_ENUM m_eTransitionMode;
NC_ARC_SHORT_LONG_ENUM m_eArcShortLong;
NC_PATH_CHOICE_ENUM m_ePathChoice;
NC_CIRC_MODE_ENUM m_eCircleMode;
unsigned char m_ucSuperimposed;
unsigned char m_ucExecute;
unsigned int m_uiExecDelayMs;
Refer to the section MMC_MOTIONPARAMS_GROUP() for details of the parameters.

### PDF page 2109
<a id="pdf-page-2109"></a>
24.8.4 InitAxisData
This function initiates an axis name and retrieves a session handler. Refer to the section
MMC_GetAxisByName for details of the description, scope, and motion mode.
ivirtual void InitAxisData(
const char* cName,
MMC_CONNECT_HNDL uHandle
) throw (CMMCException)
Source GMAS\includes\CPP\CMMCGroupAxis.h
.NET Definition
Parameters
cName
Tag/assembly name as declared in XML configuration file.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.
Refer to the function example in section 24.5.6

### PDF page 2110
<a id="pdf-page-2110"></a>
24.8.5 GetGroupAxisByName
Refer to the section MMC_GetAxisByName for details of the description, scope, and motion mode. This
function accesses the group function name.
int GetGroupAxisByName(
const char* cName
) throw (CMMCException);
Source GMAS\includes\CPP\MMCAxis.h
Python Definition def MMC_GetGroupByNameCmd(hConn, pInParam, pOutParam):
return _mmcpp_lib.MMC_GetGroupByNameCmd(hConn,
pInParam, pOutParam)

Parameters
cName
Tag/assembly name as declared in XML configuration file.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name The axis reference
Error ID Status of the axis.
24.8.5.1 Functions Code Example

// 16.2.5. GetFbDepth 5.7.3. MMC_GetFbDepth Cmd
// unsigned int CMMCMotionAxis::GetFbDepth()
// 16.2.6. GetAxisByName 10.3.17. MMC_GetAxisByName Cmd
//int CMMCAxis::GetAxisByName(const char* cName)
// 16.2.7. GetGroupAxisByName 10.3.18. MMC_GetGroupByName Cmd
// int CMMCGroupAxis::GetGroupAxisByName(const char* cName)
void DepthName(void)
// ====================
{
unsigned int iVal1, iVal2, iVal3;

printf("\n %s:", __func__);
iVal1 = AxisB.GetFbDepth();

Group.GroupDisable();
AxisB.PowerOff(MC_BUFFERED_MODE);

iVal2 = AxisB.GetFbDepth();
WaitFbDone(NC_AXIS_DISABLED_MASK, &AxisB);
iVal3 = AxisB.GetFbDepth();

printf("\n +++++ oldFb=%d B4WaitDis=%d, AftWaitDis=%d +++++", iVal1,
iVal2,iVal3);

AxisB.PowerOn(MC_BUFFERED_MODE);

### PDF page 2111
<a id="pdf-page-2111"></a>
WaitFbDone(NC_AXIS_STAND_STILL_MASK, &AxisB);
Group.GroupEnable();

iVal1 = AxisA.GetAxisByName("a01"); /* Expected 0 */
iVal2 = AxisB.GetAxisByName("a02"); /* Expected 1 */
// iVal3 = AxisA.GetAxisByName("A01"); /* It case sensitive - Not
define - exception... */
iVal1 = Group.GetGroupAxisByName("v01"); /* Expected 256 */
/*
* iVal2 = Group.GetGroupAxisByName("v02");
*/
}

### PDF page 2112
<a id="pdf-page-2112"></a>
24.8.6 SetDefaultParams
Sets the multiple axes' default parameters and overwrites the class default parameters.
void SetDefaultParams(
const MMC_MOTIONPARAMS_GROUP& stGroupAxisParams
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
stGroupAxisParams
stGroupAxisParams references the structure MMC_MOTIONPARAMS_GROUP with
default parameters, and either returns none, or throws CMMCException on failure.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
MMC_MOTIONPARAMS_GROUP Structure
typedef struct{
unsigned char ucExecute;
double dAuxPoint[NC_MAX_NUM_AXES_IN_NODE];
double dEndPoint[NC_MAX_NUM_AXES_IN_NODE];
float fVelocity;
float fAcceleration;
float fDeceleration;
float fJerk;
float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];
MC_COORD_SYSTEM_ENUM eCoordSystem;
NC_TRANSITION_MODE_ENUM eTransitionMode;
MC_BUFFERED_MODE_ENUM eBufferMode;
unsigned char ucSuperimposed;
unsigned int m_uiExecDelayMs;
}MMC_MOTIONPARAMS_GROUP;
Parameters
All parameters
Refer to the section MMC_MOTIONPARAMS_GROUP() for details of the parameters.

### PDF page 2113
<a id="pdf-page-2113"></a>
24.8.6.1 Function Code Example

// 16.4.1. void CMMCGroupAxis:: SetDefaultParams(const
MMC_MOTIONPARAMS_GROUP& stGroupAxisParams) (- no corresponding MMC_ C
func)
void SetGrpKinDef(void)
// =======================
{
printf("\n %s:", __func__);
/* Parameters for Kinematic Transformation */
SetKin.eBufferMode = MC_BUFFERED_MODE;
SetKin.eType[0] = NC_X_AXIS_TYPE;
SetKin.eType[1] = NC_Y_AXIS_TYPE;

SetKin.hNode[0] = AxisA.GetRef();
SetKin.hNode[1] = AxisB.GetRef();

SetKin.iMcsToAcsFuncID[0] = NC_TR_SHIFT_FUNC;
SetKin.iMcsToAcsFuncID[1] = NC_TR_SHIFT_FUNC;

SetKin.iNumAxes = 2;
SetKin.ucExecute = 1;

SetKin.ulTrCoef[0][0] = 1;
SetKin.ulTrCoef[0][1] = 1;
SetKin.ulTrCoef[0][2] = 0;

SetKin.ulTrCoef[1][0] = 1;
SetKin.ulTrCoef[1][1] = 1;
SetKin.ulTrCoef[1][2] = 0;
/* Set by default the EndPoint=StartPoint */
ParamsGroup.dEndPoint[0] = StarPoi[0][0];
ParamsGroup.dEndPoint[1] = StarPoi[0][1];

ParamsGroup.fVelocity = 100000;
ParamsGroup.fAcceleration = 2000000;
ParamsGroup.fDeceleration = 2000000;
ParamsGroup.fJerk = 10000000;
ParamsGroup.eCoordSystem = MC_MCS_COORD;
ParamsGroup.eBufferMode = MC_BUFFERED_MODE;
ParamsGroup.eTransitionMode = MC_TM_CORNER_DEVIATION_MODE_PLN6;
ParamsGroup.fTransitionParameter[0] = 2000;
ParamsGroup.ucExecute = 1;

Group.SetKinTransform(SetKin);
Group.SetDefaultParams(ParamsGroup);
}

### PDF page 2114
<a id="pdf-page-2114"></a>
24.8.7 SetCartesianKinematics
Refer to MMC_SetKinTransformCartesian for details of the description, scope, and motion mode.
This structure is deprecated, and therefore will be removed within the near future.
void SetCartesianKinematics(
MC_KIN_REF_CARTESIAN stCart
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
MC_KIN_REF_CARTESIAN stCart
Refer to the parameter definition in the structure MC_KIN_REF_CARTESIAN for details.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2115
<a id="pdf-page-2115"></a>
24.8.8 SetDeltaRobotKinematics
Refer to MMC_SetKinTransformDelta for details of the description, scope, and motion mode.
This structure is deprecated, and therefore will be removed within the near future.
void SetDeltaRobotKinematics(
MC_KIN_REF_DELTA stDelta
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
MC_KIN_REF_DELTA stDelta
Refer to the parameter definition in the structure MC_KIN_REF_DELTA for details.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2116
<a id="pdf-page-2116"></a>
24.8.9 SetKinematic
Refer to MMC_SetKinTransformEx for details of the description, scope, and motion mode.
This function will be deprecated in the future.
void SetKinematic(
MC_KIN_REF stInput,
NC_KIN_TYPE eKinType
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
MC_KIN_REF stInput
Refer to the parameter definition in the function MMC_SETKINTRANSFORMEX_IN
Structure for details.
NC_KIN_TYPE eKinType
Refer to the parameter definition in the function MMC_SETKINTRANSFORMEX_IN
Structure for details.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2117
<a id="pdf-page-2117"></a>
24.8.10 SetKinTransform
Sets the multiple axes' default parameters and overwrites the class default parameters.
This structure is deprecated, and therefore will be removed within the near future.
void SetKinTransform(
MMC_SETKINTRANSFORM_IN& stInParam
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
stInParam
stInParam references the structure MMC_SETKINTRANSFORM_IN with default
parameters, and either returns none, or throws CMMCException on failure.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
MMC_SETKINTRANSFORM_IN Structure
Parameters
All parameters
Refer to the section MMC_SETKINTRANSFORM_IN Structure for details of the
parameters.
For code example, refer to the section 24.8.1.

### PDF page 2118
<a id="pdf-page-2118"></a>
24.8.11 SetCartesianTransform
Sets the MCS to PCS parameters for group's kinematic transformation.
int SetCartesianTransform(
[MMC_SETCARTESIANTRANSFORM_IN* stInParam]

double (&dbOffset)[3],
double (&dbRotAngle)[3],
PCS_ROTATION_ANGLE_UNITS_ENUM eRotAngleUnits=PCS_DEGREE,
MC_BUFFERED_MODE_ENUM eBufferMode=MC_BUFFERED_MODE,
MC_EXECUTION_MODE eExecutionMode=eMMC_EXECUTION_MODE_IMMEDIATE]
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
MMC_SETCARTESIANTRANSFORM_IN* stInParam
stInParam references the structure MMC_SETCARTESIANTRANSFORM_IN with default
parameters, and either returns none, or throws CMMCException on failure.
(&dbOffset)[3]
X,Y,Z translation components' offsets. Any positive or negative values
(&dbRotAngle)[3]
U,V,W rotation angles. Any positive or negative values
PCS_ROTATION_ANGLE_UNITS_ENUM eRotAngleUnits=PCS_DEGREE
The rotational units used to defined the angle units used in PCS to MCS transformations
The enumerator PCS_ROTATION_ANGLE_UNITS_ENUM is defined by the following:
PCS_DEGREE = 0
PCS_RADIAN = 1
MC_BUFFERED_MODE_ENUM eBufferMode=MC_BUFFERED_MODE
The MC_BUFFERED_MODE_ENUM enumerator defines the behavior of the axis. Modes
are as follows:
MC_ABORTING_MODE = 1
MC_BUFFERED_MODE = 2
MC_BLENDING_LOW_MODE = 3
MC_BLENDING_PREVIOUS_MODE = 4
MC_BLENDING_NEXT_MODE = 5
MC_BLENDING_HIGH_MODE = 6
Buffered The next function block affects the axis as soon as the previous

### PDF page 2119
<a id="pdf-page-2119"></a>
movement is completed.
MC_EXECUTION_MODE eExecutionMode=eMMC_EXECUTION_MODE_IMMEDIATE
Execution mode enumerator defining whether the execution is immediate or queued,
with the following values:
eMMC_EXECUTION_MODE_IMMEDIATE = 0,
eMMC_EXECUTION_MODE_QUEUED
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
MMC_SETCARTESIANTRANSFORM_IN Structure
Parameters
All parameters
Refer to the section MMC_SETCARTESIANTRANSFORM_IN Structure for details of the
parameters.

### PDF page 2120
<a id="pdf-page-2120"></a>
24.8.12 ReadCartesianTransform
Read parameters previously set by SetCartesianTransform.
int ReadCartesianTransform(
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2121
<a id="pdf-page-2121"></a>
24.8.13 TrackConveyorBelt
This function block provides an abstraction layer for a conveyor, allowing the user to track objects moving
on a straight line in space on a conveyor belt. In short, a dynamic MCS to PCS trans ition depends on the
conveyor axis position. For explanatory details refer to the MMC_TrackConveyorBelt function.
int TrackConveyorBelt(
unsigned short usMaster,
double(&dbMasterOrigin)[6],
double(&dbPCSOrigin)[6],
double(&dbInitialObjectPosition)[6],
double(&dbRampTrajectoryParams)[12],
double dbMasterInitialPosition,
double dbMasterSyncPosition,
double dbMasterScaling,
unsigned char ucAutoSyncPosition = 1,
PCS_ROTATION_ANGLE_UNITS_ENUM eRotAngleUnits = PCS_RADIAN,
PCS_REF_AXIS_SRC_ENUM eSourceType = NC_PCS_TARGET_POS
) throw (CMMCException);

int TrackConveyorBelt(MMC_TRACKSYNCIN_IN& params
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
usMaster
Master axis which should be tracked. This is the positive short axis reference of the
master that is to be followed.
(&dbMasterOrigin)[6]
This is the Master Origin (whether on a Rotary Table or Conveyor Belt) where the values
should match the active Master origin.
An array of 6 parameters, (x,y,z,u,v,w), defining the Static 6DoF (6 degrees of freedom)
T(MCS->CBOrigin) transformation. This Transformation defines the Static Origin and
Orientation of the master table coordinated system, relative to the MCS.
ARRAY[6] of LREAL[x,y,z,u,v,w] values.
(&dbPCSOrigin)[6]
Initial PCS reference axis position of the conveyor belt. PCS coordinate system Initial
Position (dynamic) within the conveyor belt coordinated system.
An array of 6 parameters (x,y,z,u,v,w), defining the dynamic 6DoF T(RTOrigin->PCS)
transformation. This transformation defines the initial position (Origin and Orientation)
of the Moving PCS, relative to the conveyor belt origin (and rotation point) coordinated
system.
During the RAMP function implementation, the limited u=v=0 allows movement of the
PCS XY plane only to be parallel to the Conveyor Belt and MCS XY planes. ARRAY[6] of
LREAL [x,y,z,u,v,w] values

### PDF page 2122
<a id="pdf-page-2122"></a>
(&dbInitialObjectPosition)[6]
X,Y,Z,U,V,W initial object's position on the conveyor belt. Static part position (r elative)
within the (dynamic) PCS. A static 6DoF location (x, y, z, u, v, w) defining the (static) Part
position and orientation within the moving PCS, relative to the PCS.
This is the Position PPCS defined in Figure 166 above. This must be a static position.
The RAMP function assumes that this position does NOT change within the PCS during
the RAMP part.
During the RAMP function implementation, the limited x, y, z, u=0, v=0, w=0 all ows only
translational offsets, i.e. no rotations allowed.
ARRAY[6] of LREAL[x,y,z,u,v,w] values
(&dbRampTrajectoryParams)[12]
When the MMC_TrackConveyorBelt function is called, a special RAMP motion is
generated. This motion consists of two separate parts.
- XY plane motion
- Z axis motion
The purpose is to allow the Robot Z axis to retract/approach from/to the initial/final
positions, in a vertical way, and enable definition of the Safe Z Height.
The parameter MasterSyncPosition defines a distance from the initial position of the
master to the synchronized point. If we define DeltaF
i to be DeltaFi = MasterSyncPosition in
distance units then the following is true
For Sync-In
ZSafe (index 0) defines an absolute position for safe zone of z axis.
[1] T1 percentage of DeltaFi until motion by X,Y begins.
[2] T2 percentage of DeltaFi until motion by X,Y is ended.
[3] T3 percentage of DeltaFi for Z motion up to complete.
[4] T4 percentage of DeltaFi for Z motion down to start.
Example 1. T1 = 0.15, T2 = 0.85, T3 = 0.15, T4 = 0.85
Example 2. T1 = 0.15, T2 = 0.85, T3 = 0.20, T4 = 0.80
ARRAY[12] of LREAL[<Z Safe Z Height>, t1-t4, spare 7 doubles] values
dbMasterInitialPosition
Indicates the Master Initial Position (w), at the same exact time/location as the PCS
coordinate system initial position was provided within the Conveyor Table coordinated
system, i.e. initial position set by user (e.g. at activation time, camera event, etc.)
The RampTargetPosition (as calculated by the given inputs) and the Master Initial
Position (w), must be latched simultaneoiusly at the same Time.
LREAL values
dbMasterSyncPosition
The dbMasterSyncPosition is defined relative to the dbMasterInitialPosition w as given
by the user or directly related to the Master's actual position at activation time as

### PDF page 2123
<a id="pdf-page-2123"></a>
indicated by the AutoSyncReference flag i.e. relative distance from initial position.
When the Master axis (RT) reaches this position, the Robot reaches the pTa rgetPosition
and the group axis status become synchronous.
The distance, which is defined by the relative parameter MasterSyncPosition is defined
as DeltaFi.
LREAL values
dbMasterScaling
The scaling of the referenece axis relative to the position of the obj ect on the Conveyor
Belt (Master) and the length of the Conveyor Belt. Scaling for position/speed (mainly
counts to radians)
ucAutoSyncPosition = 1
A Boolean TRUE/FALSE flag defines whether the MasterSyncPosition should relate to
the MasterInitialPosition (0) or to the conveyor belt position at activation time (1).
There are two options:
- AutoSyncReference=1 means MasterSyncPosition relative to conveyor belt position
at activation time
- AutoSyncReference=0 means MasterSyncPosition relative to MasterInitialPosition
PCS_ROTATION_ANGLE_UNITS_ENUM eRotAngleUnits = PCS_RADIAN
The rotational units used to defined the angle units used in PCS to MCS transformations
The enumerator PCS_ROTATION_ANGLE_UNITS_ENUM is defined by the following:
PCS_DEGREE = 0
PCS_RADIAN = 1
PCS_REF_AXIS_SRC_ENUM eSourceType = NC_PCS_TARGET_POS
This is a reference type for dynamic PCS/MCS transformation. The enumerator
PCS_REF_AXIS_SRC_ENUM has the following values:
NC_PCS_TARGET_POS = 0
NC_PCS_ACTUAL_POS = 1
NC_PCS_AUX_POS = 2
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
MMC_TRACKSYNCIN_IN& params Structure
typedef struct MMC_TRACKSYNCIN_IN {
double dbMasterOrigin[6];
double dbPCSOrigin[6];
double dbInitialObjectPosition[6];
double dbMasterInitialPosition;

### PDF page 2124
<a id="pdf-page-2124"></a>
double dbMasterSyncPosition;
double dbMasterScaling;
double dbRampTrajectoryParams[12];
MC_BUFFERED_MODE_ENUM eBufferMode;
PCS_ROTATION_ANGLE_UNITS_ENUM eRotAngleUnits;
TRAJECTORY_MODE_ENUM eTrajectoryMode;
unsigned short usMaster;
unsigned char ucAutoSyncPosition;
unsigned char ucExecute;
unsigned char ucSpare[32];
} MMC_TRACKSYNCIN_IN;
Parameters
All parameters
Refer to the section MMC_TRACKCONVEYOR_IN Structure for details of the
parameters.

### PDF page 2125
<a id="pdf-page-2125"></a>
24.8.14 TrackRotaryTable
This function block offers an abstraction layer for a rotary table, allowing the user to track objects moving on
a cyclic space. In short, a dynamic MCS to PCS transition depends on rotary table axis position. This
command operates a real motion profiler, as opposed to the MMC_TrackRotaryTable function which is an
Administrative command. For explanatory details refer to the MMC_TrackRotaryTable function.
int TrackRotaryTable(
unsigned short usMaster,
double(&dbMasterOrigin)[6],
double(&dbPCSOrigin)[6],
double(&dbInitialObjectPosition)[6],
double(&dbRampTrajectoryParams)[12],
double dbMasterInitialPosition,
double dbMasterSyncPosition,
double dbMasterScaling,
unsigned char ucAutoSyncPosition = 1,
PCS_ROTATION_ANGLE_UNITS_ENUM eRotAngleUnits = PCS_RADIAN,
PCS_REF_AXIS_SRC_ENUM eSourceType = NC_PCS_TARGET_POS
) throw (CMMCException);

int TrackRotaryTable(MMC_TRACKSYNCIN_IN& params
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
usMaster
Master axis which should be tracked. This is the positive short axis reference of the
master that is to be followed.
(&dbMasterOrigin)[6]
This is the Master Origin (whether on a Rotary Table or Conveyor Belt) where the values
should match the active Master origin.
An array of 6 parameters, (x,y,z,u,v,w), defining the Static 6DoF (6 degrees of freedom)
T(MCS->CBOrigin) transformation. This Transformation defines the Static Origin and
Orientation of the master table coordinated system, relative to the MCS.
ARRAY[6] of LREAL[x,y,z,u,v,w] values.
(&dbPCSOrigin)[6]
Initial PCS reference axis position of the conveyor belt. PCS coordinate system Initial
Position (dynamic) within the Rotary Table coordinated system.
An array of 6 parameters (x,y,z,u,v,w), defining the dynamic 6DoF T(RTOrigin->PCS)
transformation. This transformation defines the initial position (Origin and Orientation)
of the Moving PCS, relative to the Rotary Table origin (and rotation point) coordinated
system.
During the RAMP function implementation, the limited u=v=0 allows movement of the
PCS XY plane only to be parallel to the Rotary Table and MCS XY planes. ARRAY[6] of

### PDF page 2126
<a id="pdf-page-2126"></a>
LREAL [x,y,z,u,v,w] values
(&dbInitialObjectPosition)[6]
X,Y,Z,U,V,W initial object's position on the Rotary Table. Static part position (relative)
within the (dynamic) PCS. A static 6DoF location (x, y, z, u, v, w) defining the (static) Part
position and orientation within the moving PCS, relative to the PCS.
This is the Position PPCS defined in Figure 168 above. This must be a static position.
The RAMP function assumes that this position does NOT change within the PCS during
the RAMP part.
During the RAMP function implementation, the limited x, y, z, u=0, v=0, w=0 allows only
translational offsets, i.e. no rotations allowed.
ARRAY[6] of LREAL[x,y,z,u,v,w] values
(&dbRampTrajectoryParams)[12]
When the MMC_TrackRotaryTable function is called, a special RAMP motion is
generated. This motion consists of two separate parts.
- XY plane motion
- Z axis motion
The purpose is to allow the Robot Z axis to retract/approach from/to the initial/final
positions, in a vertical way, and enable definition of the Safe Z Height.
The parameter MasterSyncPosition defines a distance from the initial position of the
master to the synchronized point. If we define DeltaF
i to be DeltaFi = MasterSyncPosition in
distance units then the following is true
For Sync-In
ZSafe (index 0) defines an absolute position for safe zone of z axis.
[1] T1 percentage of DeltaFi until motion by X,Y begins.
[2] T2 percentage of DeltaFi until motion by X,Y is ended.
[3] T3 percentage of DeltaFi for Z motion up to complete.
[4] T4 percentage of DeltaFi for Z motion down to start.
Example 1. T1 = 0.15, T2 = 0.85, T3 = 0.15, T4 = 0.85
Example 2. T1 = 0.15, T2 = 0.85, T3 = 0.20, T4 = 0.80
ARRAY[12] of LREAL[<Z Safe Z Height>, t1-t4, spare 7 doubles] values
dbMasterInitialPosition
Indicates the Master Initial Position (w), at the same exact time/location as the PCS
coordinate system initial position was provided within the Rotary Table coordinated
system, i.e. initial position set by user (e.g. at activation time, camera event, etc.)
The RampTargetPosition (as calculated by the given inputs) and the Master Initial
Position (w), must be latched simultaneoiusly at the same Time.
LREAL values
dbMasterSyncPosition
The dbMasterSyncPosition is defined relative to the dbMasterInitialPosition w as given

### PDF page 2127
<a id="pdf-page-2127"></a>
by the user or directly related to the Master's actual position at activation time as
indicated by the AutoSyncReference flag i.e. relative distance from initial positio n.
When the Master axis (RT) reaches this position, the Robot reaches the pTargetPosition
and the group axis status become synchronous.
The distance, which is defined by the relative parameter MasterSyncPosition is defined
as DeltaF
i.
LREAL values
dbMasterScaling
The scaling of the referenece axis relative to the position of the object on the Rotary
Table (Master) and the length of the Rotary Table. Scaling for position/speed (mainly
counts to radians)
ucAutoSyncPosition = 1
A Boolean TRUE/FALSE flag defines whether the MasterSyncPosition should relate to
the MasterInitialPosition (0) or to the Rotary Table position at activation time (1) . There
are two options:
- AutoSyncReference=1 means MasterSyncPosition relative to Rotary Table position
at activation time
- AutoSyncReference=0 means MasterSyncPosition relative to MasterInitialPosition
PCS_ROTATION_ANGLE_UNITS_ENUM eRotAngleUnits = PCS_RADIAN
The rotational units used to defined the angle units used in PCS to MCS transformations
The enumerator PCS_ROTATION_ANGLE_UNITS_ENUM is defined by the following:
PCS_DEGREE = 0
PCS_RADIAN = 1
PCS_REF_AXIS_SRC_ENUM eSourceType = NC_PCS_TARGET_POS
This is a reference type for dynamic PCS/MCS transformation. The enumerator
PCS_REF_AXIS_SRC_ENUM has the following values:
NC_PCS_TARGET_POS = 0
NC_PCS_ACTUAL_POS = 1
NC_PCS_AUX_POS = 2
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
MMC_TRACKSYNCIN_IN& params Structure
typedef struct MMC_TRACKSYNCIN_IN {
double dbMasterOrigin[6];
double dbPCSOrigin[6];
double dbInitialObjectPosition[6];

### PDF page 2128
<a id="pdf-page-2128"></a>
double dbMasterInitialPosition;
double dbMasterSyncPosition;
double dbMasterScaling;
double dbRampTrajectoryParams[12];
MC_BUFFERED_MODE_ENUM eBufferMode;
PCS_ROTATION_ANGLE_UNITS_ENUM eRotAngleUnits;
TRAJECTORY_MODE_ENUM eTrajectoryMode;
unsigned short usMaster;
unsigned char ucAutoSyncPosition;
unsigned char ucExecute;
unsigned char ucSpare[32];
} MMC_TRACKSYNCIN_IN;
Parameters
All parameters
Refer to the section MMC_TRACKROTARY_IN Structure for details of the parameters.

### PDF page 2129
<a id="pdf-page-2129"></a>
24.8.15 SetKinTransformDelta
Sets the kinematic transformation parameters (MSC to ACS) for the Delta robot. Refer to
MMC_SetKinTransformDelta for details of the description, scope, and motion mode.
int SetKinTransformDelta(
IN MMC_KINTRANSFORM_DELTA_IN& pInParam
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Input Parameters
MMC_KINTRANSFORM_DELTA_IN& pInParam
pInParam references the structure MMC_KINTRANSFORM_DELTA_IN with default
parameters, and either returns none, or throws CMMCException on failure.
throw (CMMCException)
Refer to the section 24.1.1MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
MMC_KINTRANSFORM_DELTA_IN Structure
Parameters
All parameters
Refer to the section MMC_KINTRANSFORM_DELTA_IN Structure for details of the
parameters.

### PDF page 2130
<a id="pdf-page-2130"></a>
24.8.16 SetKinTransformCartesian
Sets the parameters kinematic transformation (MSC to ACS) for Cartesian system. Refer to
7.10.19MMC_SetKinTransformCartesian for details of the description, scope, and motion mode.
int SetKinTransformCartesian(
IN MMC_KINTRANSFORM_CARTESIAN_IN& pInParam
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def MMC_SetKinTransformCartesian(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_SetKinTransformCartesian(hConn,
hAxisRef, pInParam, pOutParam)

def SetKinTransformCartesian(self, i_params, ucLinearUU=0,
ucRotaryUU=0):
return
_mmcpp_lib.CMMCGroupAxis_SetKinTransformCartesian(self,
i_params, ucLinearUU, ucRotaryUU)
Input Parameters
MMC_KINTRANSFORM_CARTESIAN_IN& pInParam
pInParam references the structure MMC_KINTRANSFORM_CARTESIAN_IN with default
parameters, and either returns none, or throws CMMCException on failure.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
MMC_KINTRANSFORM_CARTESIAN_IN Structure
Parameters
All parameters
Refer to the section MMC_KINTRANSFORM_CARTESIAN_IN Structure for details of the
parameters.

### PDF page 2131
<a id="pdf-page-2131"></a>
24.8.17 SetKinTransformScara
Sets the kinematic transformation parameters (MSC to ACS) for the SCARA robot. Refer to
7.10.20MMC_SetKinTransformScara for details of the description, scope, and motion mode.
int SetKinTransformScara(
IN MMC_KINTRANSFORM_SCARA_IN& pInParam
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Input Parameters
MMC_KINTRANSFORM_SCARA_IN& pInParam
pInParam references the structure MMC_KINTRANSFORM_SCARA_IN with default
parameters, and either returns none, or throws CMMCException on failure.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
MMC_KINTRANSFORM_SCARA_IN Structure
Parameters
All parameters
Refer to the section MMC_KINTRANSFORM_SCARA_IN Structure for details of the
parameters.

### PDF page 2132
<a id="pdf-page-2132"></a>
24.8.18 SetKinTransformThreeLink
Sets the kinematic transformation parameters (MSC to ACS) for the THREELINK robot. Refer to
7.10.21MMC_SetKinTransformThreeLink for details of the description, scope, and motion mode.
int SetKinTransformThreeLink(
IN MMC_KINTRANSFORM_THREELINK_IN& pInParam
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Input Parameters
MMC_KINTRANSFORM_THREELINK_IN& pInParam
pInParam references the structure MMC_KINTRANSFORM_THREELINK_IN with default
parameters, and either returns none, or throws CMMCException on failure.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
MMC_KINTRANSFORM_THREELINK_IN Structure
Parameters
All parameters
Refer to the section MMC_KINTRANSFORM_THREELINK_IN Structure for details of the
parameters.

### PDF page 2133
<a id="pdf-page-2133"></a>
24.8.19 MoveAngle
Performs an Arc motion, where the motion will perform in one plane perpendicular to another in space (XY,
XZ, or YZ). Refer to 7.10.21MMC_MoveAngle for details of the description, scope, and motion mode.
int MoveAngle(
double (&dCenter)[MAX_CENTER_POINTS],
double dAngle,
double dVelocity,
double dAcceleration,
double dDeceleration,
double dJerk,
double (&dTransitionParameter)[NC_MAX_NUM_AXES_IN_NODE],
NC_TRANSITION_MODE_ENUM eTransitionMode = MC_TM_NONE_MODE,
MC_COORD_AXES ePlain=NC_XY_AXES ,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE,
double dHelixPos=0
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def MMC_MoveAngle(hConn, hAxisRef, pInParam, pOutParam):
return _mmcpp_lib.MMC_MoveAngle(hConn, hAxisRef,
pInParam, pOutParam)
def MoveAngle(self, *args):
return _mmcpp_lib.CMMCGroupAxis_MoveAngle(self, *args)
Input Parameters
(&dCenter) [NC_MAX_NUM_AXES_IN_NODE]
Type: double NC_MAX_NUM_AXES_IN_NODE array
Double array [1..N] of absolute positions of the center point in the circular for each
dimension in the coordinate system specified by the input signal CoordSystem, with N
being vendor specific.
The array parameter NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as
the maximum number of axis in a group.
dCenterPoint can have vector array [1....2] double values in a technical unit [u].
Note: When used in the overloaded function without this parameter, the data of the
Center Point will entered in the member m_AuxPoint.
dAngle
Type: double
Double value of the relative angular position for the coordinate system specified by the
input signal CoordSystem.
Angular values are in degrees [u], which may be positive or negative without restriction.

### PDF page 2134
<a id="pdf-page-2134"></a>
dVelocity
Type: double
Double value of the maximum velocity of the rotation of axis.
Refer to the Input Parameters at section MMC_MOVEEANGLE_IN Structure for details.
dAcceleration
Type: double
Double value of the acceleration (increasing energy of the motor).
Refer to the Input Parameters at section MMC_MOVEEANGLE_IN Structure for details.
dDeceleration
Type: double
Double value of the deceleration when stopping.
Refer to the Input Parameters at section MMC_MOVEEANGLE_IN Structure for details.
dJerk
Type: double
Double value of the Jerk prior to acceleration.
Refer to the Input Parameters at section MMC_MOVEEANGLE_IN Structure for details.
(&dTransitionParameter) [NC_MAX_NUM_AXES_IN_NODE]
Type: NC_MAX_NUM_AXES_IN_NODE array
Double array [1..N] of the axes values which characterize the contour curve depending
on the transition mode. The array parameter NC_MAX_NUM_AXES_IN_NODE is limited
to 16, and defined as the maximum number of axis in a group.
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
Refer to the Input Parameters at section MMC_MOVEEANGLE_IN Structure for details.
NC_TRANSITION_MODE_ENUM eTransitionMode
Type: NC_TRANSITION_MODE_ENUM = MC_TM_NONE_MODE enumerator
The enumerator NC_TRANSITION_MODE_ENUM defines the supported transition
modes. The enumerator options are:
MC_TM_NONE_MODE = 0,
MC_TM_MAX_VELOCITY_MODE = 1, Not supported at this time
MC_TM_DEFINED_VELOCITY_MODE = 2,
MC_TM_CORNER_DISTANCE_MODE = 3,
MC_TM_MAX_CORNER_DEVIATION_MODE = 4,
MC_TM_SWITCH_RADIUS_MODE = 5,
