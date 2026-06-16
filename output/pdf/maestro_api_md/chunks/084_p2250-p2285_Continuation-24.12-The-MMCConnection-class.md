# Continuation - 24.12 The MMCConnection class

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 2250-2285
- Chunk: `084_p2250-p2285_Continuation-24.12-The-MMCConnection-class.md`

## Active Outline At Chunk Start
- p. 1705 - Chapter 24 Programming in C++
  - p. 2210 - 24.12 The MMCConnection class

## Contained Bookmark Outline
  - p. 2269 - 24.13 The MMCNetwork class

## Extracted Text

### PDF page 2250
<a id="pdf-page-2250"></a>
}
24.12.7 MMC_RpcInitConnectionEx
Initiates RPC connection to Maestro server.
MMC_LIB_API int MMC_RpcInitConnectionEx(
IN MMC_CONNECTION_TYPE eType,
IN MMC_CONNECTION_PARAM_STRUCT sConnParam,
IN MMC_MB_CLBK pCbFunc,
IN char* cpHostIPAddr,
OUT MMC_CONNECT_HNDL* pHndl
);
Motion Mode NC - Not relevant Distributed - not relevant
Source GMAS\includes\MMC_general_API.h
Parameters
eType
[IN] Connection type (IPC(inter process communication) or RPC(remote procedure
calls)) with the following possible MMC_CONNECTION_TYPE enumerator values:
MMC_RPC_CONN_TYPE RPC connection to MMC server
MMC_IPC_CONN_TYPE IPC connection to MMC server
sConnParam
[IN] Connection parameters. (e.g. IP port, UDP port for callback)
uiTcpPort
TCP Port. Any positive integer value.
uiCbUdpPo
rt

UDP Port. Any positive integer value.
ucIp[16]
IP address. 16-bit character.
pCbFunc
[IN] MMC_MB_CLBK pointer to UDP callback function using pCbFunc. No character
value or short integer. The source for the parameter is:
GMAS\includes\MMC_definitions.h
cpHostIPAddr
[IN] Host IP Address for multiple NIC support. Values accepted are the IP address
characters. The source for the parameter is:

### PDF page 2251
<a id="pdf-page-2251"></a>
GMAS\includes\MMC_definitions.h
pHndl
[OUT] Connection handle output using pHndl, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions.
Returned by Init Connection command. If an error occurs, the function returns -1 and a
MMC_LIB_API error with more details.

Remarks
None
Scope
This function is specifically designed for RPC connections, as it includes the parameter cpHostIPAddr , the
host IP address for NIC support.
Figure 331 describes the function block for MMC_RpcInitConnectionEx.
eType enumerator
MMC_RpcInitConnectionEx
MMC_CONNECTION_TYPE
uiTcpPortinteger
uiCbUdpPortinteger
ucIp[16]IP address
MMC_CB_FUNC
pCbFunc
Integer or no value
pHndl Integer Init connection handle
cpHostIPAddrHost IP Address

Figure 331: MMC_RpcInitConnection function block
24.12.7.1 Function Block Code Example
int rc;
MMC_CONNECTION_PARAM_STRUCT stsConnParam;
//
// Inserting the structure parameters:
strcpy ((char*)stsConnParam.ucIp, "255.0.110.0"); //IP address
stsConnParam.uiCbUdpPort = 1520; //UDP Port
stsConnParam.uiTcpPort = 1910; //TCP Port
eType[1] = MMC_IPC_CONN_TYPE; //Connection type
pCbFunc = NULL; //Pointer to UDP callback
function
strcpy ((char*)cpHostIPAddr, "97.110.110.1"); //Host IP Address
//
rc = MMC_RpcInitConnection ((MMC_CONNECTION_TYPE) eType, stsConnParam,
(MMC_CB_FUNC) pCbFunc, cpHostIPAddr, &hConn);
printf("Connection State[%ld]\n", (long int)(MMC_CONNECT_HNDL) pHndl);
if (rc != 0)
{
HandleError();

### PDF page 2252
<a id="pdf-page-2252"></a>
}
MMC_IPCInitConnection.
unsigned int ConnectIPCEx(
int iEventMask,
MMC_MB_CLBK fpClbk
);
Source GMAS\includes\CPP\MMCConnection.h
Python Definition TBD
Parameters
iEventMask
Defined according to the event IDs described in the section 14.20Events Mask and
Enumeration. Bitwise positive integer ID.
MMC_MB_CLBK fpClbk
The type definition MMC_MB_CLBK is part of this header file and is simi lar to the
callback prototype described in section 14.21.1, which also conta ins the fpClbk callback
function. However, the MMC_MB_CLBK is a specific type of callback definition used to
describe functions with a variable number of parameters.
For code example, refer to the section 24.4.1

### PDF page 2253
<a id="pdf-page-2253"></a>
24.12.8 ConnectRPC
Creates an RPC connection. Any positive integer values accepted for the connection. Refer to the functions
MMC_GetLastError, 10.2.26MMC_InitConnection, MMC_RpcInitConnection, and
MMC_RpcInitConnectionEx.
unsigned int ConnectRPC(
char* cHostIP,
char* cDestIP,
int iEventMask,
MMC_CB_FUNC fpClbk
);
Source GMAS\includes\CPP\MMCConnection.h
C# Definition public static int ConnectRPC(
IPAddress destinationAddress,
IPAddress hostAddress,
int hostPort,
out int hndl,
bool pollBeforeSend = false,
int sendReceiveTimeout = 3000,
int sendReceiveSystemTimeout = 15000)
{
return MMCConnection.ConnectRPC(destinationAddress,
4000, hostAddress, hostPort, (cbFunc) null, 4026531839U,
out hndl, pollBeforeSend, sendReceiveTimeout,
sendReceiveSystemTimeout);
}

public static int ConnectRPC(
IPAddress destinationAddress,
int destinationPort,
IPAddress hostAddress,
int hostPort,
cbFunc callbackFunc,
uint eventMask,
out int hndl,
bool pollBeforeSend = false,
int sendReceiveTimeout = 3000,
int sendReceiveSystemTimeout = 15000)
{
Python Definition def ConnectRPC(self, cHostIP, cDestIP, iEventMask, fpClbk):
return _mmcpp_lib.CMMCConnection_ConnectRPC(self,
cHostIP, cDestIP, iEventMask, fpClbk)
Parameters
cHostIP
Host IP address. Character value in the format of an IP address.
cDestIP
Destination IP address. Character value in the format of an IP address.

### PDF page 2254
<a id="pdf-page-2254"></a>
iEventMask
Defined according to the event IDs described in the section Events Mask and
Enumeration. Bitwise positive integer ID.
MMC_CB_FUNC fpClbk
The type definition MMC_CB_FUNC is part of this header file and is similar to the
callback prototype described in section 14.21.1, which also conta ins the fpClbk callback
function.

### PDF page 2255
<a id="pdf-page-2255"></a>
24.12.9 ConnectRPCEx
Creates an RPC connection. Any positive integer values accepted for the connection. Refer to the functions
MMC_GetLastError, 10.2.26MMC_InitConnection, MMC_RpcInitConnection, and
MMC_RpcInitConnectionEx.
unsigned int ConnectRPCEx(
char* cHostIP,
char* cDestIP,
int iEventMask,
MMC_MB_CLBK fpClbk
);
Source GMAS\includes\CPP\MMCConnection.h
Python Definition def ConnectRPCEx(self, *args):
return _mmcpp_lib.CMMCConnection_ConnectRPCEx(self,
*args)

Parameters
cHostIP
Host IP address. Character value in the format of an IP address.
cDestIP
Destination IP address. Character value in the format of an IP address.
iEventMask
Defined according to the event IDs described in the section Events Mask and
Enumeration. Bitwise positive integer ID.
MMC_MB_CLBK fpClbk
The type definition MMC_MB_CLBK is part of this header file and is similar to the
callback prototype described in section 14.21.1, which also contains the fpClbk callback
function. However, the MMC_MB_CLBK is sa specific type of callback definition used to
describe functions with a variable number of parameters.

### PDF page 2256
<a id="pdf-page-2256"></a>
24.12.10 SetGlobalBoolParameter
Sets the global Boolean parameters. Refer to the section MMC_GlobalWriteBoolParameter for further
details.
void SetGlobalBoolParameter(
long lValue, | double dbValue,
MMC_PARAMETER_LIST_ENUM eNumber,
int iIndex
) throw (CMMCException)
Source GMAS\includes\CPP\MMCConnection.h
.NET Definition
Parameters
lValue
Input parameter. Any integer value.
dbValue
An array parameter with double value.
MMC_PARAMETER_LIST_ENUM eNumber
Number of the parameter. One can also use symbolic parameter names, which are
declared as VAR CONST. Refer to the section Axis Parameters (Explanations) .
The axis parameters define the MMC_PARAMETER_LIST_ENUM eParameterNumber
values of the axis status. Refer to the section 5.4.2Parameters Tables for the integer
parameter definitions for the appropriate integer parameter to be used as enumerator.
iIndex
An array index parameter (only relevant for array situations). Any positive integer
values.
Return
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2257
<a id="pdf-page-2257"></a>
24.12.11 GetGlobalBoolParameter
Obtain the global Boolean parameters. Refer to the section MMC_GlobalWriteBoolParameter for further
details.
long GetGlobalBoolParameter(
MMC_PARAMETER_LIST_ENUM eNumber,
int iIndex
) throw (CMMCException)
Source GMAS\includes\CPP\MMCConnection.h
.NET Definition
Parameters
MMC_PARAMETER_LIST_ENUM eNumber
Number of the parameter. One can also use symbolic parameter names, which are
declared as VAR CONST. Refer to the section Axis Parameters (Explanations) .
The axis parameters define the MMC_PARAMETER_LIST_ENUM eParameterNumber
values of the axis status. Refer to the section Parameters Tables for the integer
parameter definitions for the appropriate integer parameter to be used as enumerator.
iIndex
An array index parameter (only relevant for array situations). Any positive integer
values.
Return
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.4.1

### PDF page 2258
<a id="pdf-page-2258"></a>
24.12.11.1 Functions Example

// 16.6.7. GetGlobalBoolParameter 5.9.15. MMC_GlobalReadBoolParameter
// 16.6.6. SetGlobalBoolParameter 5.7.25.
MMC_GlobalWriteBoolParameter

// 16.2.10. GetBoolParameter
// 16.2.8. SetBoolParameter
// CMMCNode::GetBoolParameter 5.7.11. MMC_ReadBoolParameter
// CMMCNode::SetBoolParameter 5.7.24. MMC_WriteBoolParameter
// CMMCGroupAxis::GetBoolParameter 5.9.15. MMC_GroupReadBoolParameter
// CMMCGroupAxis::SetBoolParameter 5.9.17. MMC_GroupWriteBoolParameter

// 16.2.11. GetParameter
// CMMCGroupAxis::GetParameter 5.9.14. MMC_GroupReadParameter
// CMMCNode::GetParameter 5.7.16. MMC_ReadParameter
// 16.2.9. SetParameter
// CMMCGroupAxis::SetParameter 5.9.16. MMC_GroupWriteParameter
// CMMCNode::SetParameter 5.7.28. MMC_WriteParameter

// 16.4.13. GroupReadStatus 5.9.7. MMC_GroupReadStatusCmd

// 16.6.8. GetGlobalParameter 5.7.17. MMC_GlobalReadParameter
// accordint to doc. table under 5.3.2:"Real Global Parameters table -
Description" - !!! NOT IN USE !!!
void SetGetParameters(void)
// ===========================
{
unsigned long ulong;
long lValue1, lValue2, lValue3, lValue4, lValue5;
double db1, db2, db3;
int iIndex; /* index into parameters array if axis group */

iIndex = 0;

printf("\n %s:", __func__);
/* Execution time limit before FB become active, */
/* when performing EndVelocities Recalcuation [ms] */
lValue1 = cConn.GetGlobalBoolParameter(
MMC_EST_TIME_TO_BE_ACTIVEFB_THRSHLD, iIndex);
cConn.SetGlobalBoolParameter(1000,
MMC_EST_TIME_TO_BE_ACTIVEFB_THRSHLD, iIndex);

/* GMAS - Drive uf pARAMETER #3 */
lValue1 = AxisA.GetBoolParameter( MMC_I_COMM_EV_USR_3_PARAM, iIndex);
AxisA.SetBoolParameter(300, MMC_I_COMM_EV_USR_3_PARAM, iIndex);

/*
* See "Boolean Group Parameters table - Description" (Prg: 5.3.1. & 5.3.2.)
* Symbol Name Val Bitwise Permission
* =========== === ==================
* MMC_AXIS_GROUP_ID_PARAM = 4 5
* MMC_SPATIAL_OPTION_PARAM = 28 6

### PDF page 2259
<a id="pdf-page-2259"></a>
* MMC_END_MOTION_REASON = 71 5
* MMC_FB_DEPTH = 75 5
* MMC_STATUS_REGISTER = 91 5
* MMC_MCS_LIMIT_REGISTER = 92 5
*/
lValue1 = Group.GetBoolParameter( MMC_AXIS_GROUP_ID_PARAM, 0); /* Read
Only */
lValue2 = Group.GetBoolParameter( MMC_AXIS_GROUP_ID_PARAM, 1); /* Read
Only */
lValue3 = Group.GetBoolParameter( MMC_AXIS_GROUP_ID_PARAM, 2); /* Read
Only */
lValue4 = Group.GetBoolParameter( MMC_AXIS_GROUP_ID_PARAM, 3); /* Read
Only */
lValue5 = Group.GetBoolParameter( MMC_AXIS_GROUP_ID_PARAM, 4); /* Read
Only */

lValue2 = Group.GetBoolParameter( MMC_END_MOTION_REASON, iIndex); /*
Read Only */
lValue3 = Group.GetBoolParameter( MMC_FB_DEPTH, iIndex); /* Read Only
*/
lValue4 = Group.GetBoolParameter( MMC_SPATIAL_OPTION_PARAM, iIndex);
Group.SetBoolParameter(1, MMC_SPATIAL_OPTION_PARAM, iIndex); /*
Max=1 */

/*
* See "Real Group Parameters table - Description" (Prg: 5.3.2.)
* Symbol Name Val Bitwise Permission (bit0=Read only(LSB),
bit1=Read/Write...)
* =========== ==== ==================
* MMC_MAX_VELOCITY_PARAM = 15 14
* MMC_SW_MAX_VELOCITY_PARAM = 16
* MMC_SET_ACCELERATION_PARAM = 18 21
* MMC_MAX_ACCELERATION_PARAM = 19 22
* MMC_SW_MAX_ACCELERATION_PARAM = 20
* MMC_SET_DECELERATION_PARAM = 22 21
* MMC_MAX_DECELERATION_PARAM = 23 22
* MMC_SW_MAX_DECELERATION_PARAM = 24
* MMC_MAX_JERK_PARAM = 26 30
* MMC_SW_MAX_JERK_PARAM = 27
* MMC_F_COMM_EV_USR_1_PARAM = 35
*/

db1 = AxisA.GetParameter( MMC_MAX_VELOCITY_PARAM, iIndex);
AxisA.SetParameter(20000000.0,MMC_MAX_VELOCITY_PARAM, iIndex);

db2 = AxisB.GetParameter( MMC_MAX_ACCELERATION_PARAM,iIndex);
AxisB.SetParameter(22200000.0, MMC_MAX_ACCELERATION_PARAM,iIndex);
db3 = AxisB.GetParameter( MMC_MAX_ACCELERATION_PARAM,iIndex); /* Not
necessary (Test, E.g. etc...) */

db1 = Group.GetParameter( MMC_MAX_DECELERATION_PARAM,0);
Group.SetParameter(3000000.0,MMC_MAX_DECELERATION_PARAM,0);

db2 = Group.GetParameter( MMC_MAX_VELOCITY_PARAM, 1);
Group.SetParameter(3330000.0,MMC_MAX_VELOCITY_PARAM, 1);

ulong = Group.GroupReadStatus();

### PDF page 2260
<a id="pdf-page-2260"></a>
if(ulong & NC_GROUP_ERROR_STOP_MASK)
{
Group.GroupReset();
}

Group.GroupDisable();
do
{
ulong = Group.GroupReadStatus();
} while (!(ulong & NC_GROUP_DISABLED_MASK));

Group.GroupEnable();
do
{
ulong = Group.GroupReadStatus();
} while (!(NC_GROUP_STANDBY_MASK & ulong));

}

void SetGetGroupParam(void)
// ===========================
{
int iIndex; /* index into parameters array if axis group */

iIndex = 0;

MMC_READGROUPOFPARAMETERSMEMBER
GroupOfPrmRed[GROUP_OF_PARAMETERS_MAXIMUM_SIZE] =
{
{MMC_MAX_VELOCITY_PARAM, iIndex, AxisARef, 0},
{MMC_MAX_ACCELERATION_PARAM,iIndex, AxisARef, 0},
{MMC_MAX_DECELERATION_PARAM,iIndex, AxisARef, 0},
{MMC_MAX_JERK_PARAM, iIndex, AxisARef, 0},
{MMC_SW_MAX_JERK_PARAM, iIndex, AxisARef, 0}
};
MMC_WRITEGROUPOFPARAMETERSMEMBER
GroupOfPrmWrt[GROUP_OF_PARAMETERS_MAXIMUM_SIZE] =
{
{11110000, MMC_MAX_VELOCITY_PARAM, iIndex, AxisARef, 0,0,0},
{22220000, MMC_MAX_ACCELERATION_PARAM, iIndex, AxisARef, 0,0,0},
{33330000, MMC_MAX_DECELERATION_PARAM, iIndex, AxisARef, 0,0,0},
{44440000, MMC_MAX_JERK_PARAM, iIndex, AxisARef, 0,0,0},
{55550000, MMC_MAX_VELOCITY_PARAM, iIndex, AxisBRef, 0,0,0}
};
double GroupOfPrmRet[GROUP_OF_PARAMETERS_MAXIMUM_SIZE];

printf("\n %s:", __func__);
/* Read the startup def. Val.*/
AxisA.ReadGroupOfParameters (GroupOfPrmRed, 5, GroupOfPrmRet);
/* Set and change one value (above init array) */
AxisA.WriteGroupOfParametersImmediate(GroupOfPrmWrt, 1);
/* Only for demo.. - for check the new setting is in effect */
AxisA.ReadGroupOfParameters (GroupOfPrmRed, 5, GroupOfPrmRet);

/* Change one of value in above init array */

### PDF page 2261
<a id="pdf-page-2261"></a>
GroupOfPrmRed[1].usAxisRef = AxisBRef;
/* Write param - one is updated, (reff by AxisB). */
AxisB.WriteGroupOfParametersImmediate(GroupOfPrmWrt, 5);
/* read 5 param. Expec: 11110000,22220000,33330000,44440000,55550000
*/
AxisB.ReadGroupOfParameters (GroupOfPrmRed, 5, GroupOfPrmRet);
/* read 5 param. Expec: 11110000,22220000,33330000,44440000,55550000
*/
AxisA.ReadGroupOfParameters (GroupOfPrmRed, 5, GroupOfPrmRet);

/* AxisB default Val are diff. from AxisA... */
GroupOfPrmWrt[0].dbValue = 11111100.0; /* MMC_MAX_VELOCITY_PARAM
*/
GroupOfPrmWrt[2].dbValue = 33331100.0; /* MMC_MAX_DECELERATION_PARAM */
GroupOfPrmWrt[4].dbValue = 55551100.0; /* MMC_MAX_VELOCITY_PARAM */
}

### PDF page 2262
<a id="pdf-page-2262"></a>
24.12.12 GetGlobalParameter
Obtain the global parameters. Refer to the section MMC_GlobalWriteParameter for further details.
double GetGlobalParameter(
MMC_PARAMETER_LIST_ENUM eNumber,
int iIndex
) throw (CMMCException)
Source GMAS\includes\CPP\MMCConnection.h
.NET Definition
Parameters
MMC_PARAMETER_LIST_ENUM eNumber
Number of the parameter. One can also use symbolic parameter names, which are
declared as VAR CONST.
Refer to the section Axis, Group, Global, Parameters for the appropriate integer
parameter to be used as enumerator.
iIndex
An array index parameter (only relevant for array situations). Any positive integer
values.
Return
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2263
<a id="pdf-page-2263"></a>
24.12.13 SetIsToLoadGlobalParams
Defines a flag whether to load or not, the global parameters, when updating the global parameters from a
file to the Maestro. Refer to the section 10.2.37MMC_SetIsToLoadGlobalParams for a detailed explanation.
void SetIsToLoadGlobalParams(
unsigned char ucVal
) throw (CMMCException)
Source GMAS\includes\CPP\MMCConnection.h
.NET Definition
Parameters
ucVal
The function recieves either 0 (not required to load the set global parameters) or 1
(required to load the set global parameters). positive integer value
Return
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2264
<a id="pdf-page-2264"></a>
24.12.14 SetHeartBeatConsumer
Sets the consumer heartbeat as an event to the user. Refer to the section MM C_SetHeartBeatConsumer for
a detailed explanation.
void SetHeartBeatConsumer(
unsigned int uiHeartbeatTimeFactor
) throw (CMMCException)
Source GMAS\includes\CPP\MMCConnection.h
.NET Definition
Parameters
uiHeartbeatTimeFactor
Heart beat time factor is a multiple of 1 ms. The calculation of the basic cycle time
(predetermined in the Resource file), multiplied by this heartbeat time factor, and 1 ms,
will set the Heartbeat time.
Values accepted are:
0, not in use
>0, any positive value
Return
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.4.1.

### PDF page 2265
<a id="pdf-page-2265"></a>
24.12.15 CallbackFunc
Defines the callback function to be an integer with positive values.
int CallbackFunc(
unsigned char*, short, void*
)
Source GMAS\includes\CPP\MMCConnection.h
.NET Definition
Parameters
unsigned char*, short, void*
Character, short, or void with a positive value
For code example, refer to the section 24.4.1 24.12.2

### PDF page 2266
<a id="pdf-page-2266"></a>
24.12.16 RegisterEventCallback
Registers the event callback for specific type callbacks. Refer to the section 14.21Asynchronous Events Callback
for a detailed explanation of callback events.
void RegisterEventCallback(
MMC_EVENT_ENUM eClbType,
void * pfClbk
)
Source GMAS\includes\CPP\MMCConnection.h
.NET Definition
Parameters
MMC_EVENT_ENUM eClbType
The parameter MMC_EVENT_ENUM is a specialist enumerator variable with values:
MMCPP_PDORCV, MMCPP_HBEAT 0
MMCPP_MOTIONENDED 1
MMCPP_EMCY 2
MMCPP_ASYNC_REPLY 3
MMCPP_HOME_ENDED 4
MMCPP_MODBUS_WRITE 5
MMCPP_TOUCH_PROBE_ENDED 6
MMCPP_NODE_ERROR 7
MMCPP_STOP_ON_LIMIT 8
MMCPP_TABLE_UNDERFLOW 9
The eClbType parameter is called according to the above enumerator, and has v alues:
PdoRcvEventCallback
HBeateEventCallback
MotionEndEventCallback
EmergencyEventCallback
HomeEndedEventCallback
ModbusWriteEventCallback
SysErrorEventCallback
AsyncReplyEventCallback
TouchProbeEndCallback
NodeErrorEventCallback
StopOnLimitEventCallback
TableUnderflowEventCallback
The enumerator value must complement the callback type. Refer to the table in section
Event Type Definitions for details of the Event type definitions.
pfClbk

### PDF page 2267
<a id="pdf-page-2267"></a>
Points to the callback function with no value returned.
For code example, refer to the section24.4.1.

### PDF page 2268
<a id="pdf-page-2268"></a>
24.12.17 RegisterSyncTimerFunction

void RegisterSyncTimerFunction(
MMC_SYNC_TIMER_CB_FUNC func,
unsigned short usSYNCTimerTime
)
Source GMAS\includes\CPP\MMCConnection.h
.NET Definition
Parameters
MMC_SYNC_TIMER_CB_FUNC func
[IN] Points to the callback function MMC_SYNC_TIMER_CB_FUNC using
MMC_CreateSYNCTimer.
usSYNCTimerTime
[IN] Defines the time between which a synchronization message is sent as an event.

### PDF page 2269
<a id="pdf-page-2269"></a>
##### 24.13 The MMCNetwork class
24.13 The MMCNetwork class
The class MMCNetwork wraps the network communication functions detailed in the section Network Function
Blocks. The diagram in Figure 550 describes the heirarchial structure of the classes and type definitions
associated with the MMCNetwork.

Figure 550 MMCNetwork class diagram
The class MMCNetwork retains the same field parameter properties and values describ ed in this document for
the C function blocks, and while small visual changes may be made to some variables, these are transparent,
and do not change the operation of the variable.

Figure 551 Fields and methods of the MMCNetwork class
The detailed class view shown in Figure 551 describes the fields and methods associated with the
MMCNetwork class. It should be noted that Protected and Private and Protected functions together with their
operations, should be transparent to the user, and are not for general application by the user.

### PDF page 2270
<a id="pdf-page-2270"></a>
24.13.1 CMMCNetwork Class Functions Code Example
/*===================================================================
Collection of Gmas API functions (Set #3)
Examples for document: "G-MAS Administrative and Motion API.pdf".
12Sep2013
Haim Hillel
==========================================================================
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

CMMCSingleAxis AxisA, AxisB;
unsigned short AxisARef, AxisBRef;

CMMCGroupAxis Group;
MMC_MOTIONPARAMS_GROUP ParamsGroup;

MMC_SETKINTRANSFORM_IN SetKin;

char * delimit =
"==========================================================================
=";
char * strStrSnro = "\n\n\n <<<<<<<<<<<<< Start ";
char * strEndSnro1 = "\n End ";
char * strEndSnro2 = " >>>>>>>>>>>>> ";

int WaitFbDone(unsigned int break_state, CMMCSingleAxis * sng_axis);

void initAdminSingleAxis(void);
void endAdminSingleAxis(void);

void initAdminMultiAxis();
void endAdminMultiAxis(void);

void SnroEnableDisableMotionEndedEvent(int);
void EnableDisableMotionEndedEvent(void);

void SnroMoveAbsolute(int);

### PDF page 2271
<a id="pdf-page-2271"></a>
void MoveAbsoluteMoves(void);

void SnroDepthName(int);
void DepthName(void);

void SnroConnection(int);
void ConnectionTypeAndNum(void);
void SendReciveFromEthercat(int NumAmp);
void SetGetDefDigOutput(void);

int CallbackFunc(unsigned char* recvBuffer, short
recvBufferSize,void* lpsock);
int OnRunTimeError(const char *msg, unsigned int uiConnHndl,
unsigned short usAxisRef, short sErrorID, unsigned short usStatus);
void EndMotionEventCB(unsigned short usAxisRef);
void ModbusWrite_Received();
void Emergency_Received(unsigned short usAxisRef, short sEmcyCode);

/*================== Administration functions STR ======================*/

int main(int)
// ==============
{
int trace = 1;

printf("\n %s", delimit);
printf("\n %s %s %s \n", __FILE__, __DATE__, __TIME__);

try
{
SnroConnection(trace++);
SnroMoveAbsolute(trace++);
SnroEnableDisableMotionEndedEvent(trace++);
SnroDepthName(trace++);

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

### PDF page 2272
<a id="pdf-page-2272"></a>
int end_of = 0;
int iCount = 0;
unsigned int ulState;

while( ! end_of)
{
iCount ++;
end_of = 1;
/* Read Axis Status command server for specific Axis */
ulState = sng_axis->ReadStatus();
if (!(ulState & break_state))
{
end_of = 0;

WAIT_SLEEP_MILLI(20)
}
}

// MMC_SHOWNODESTAT_IN showin;
// MMC_SHOWNODESTAT_OUT showout;
// MMC_ShowNodeStatCmd(ComHndl, sng_axis->GetRef(), &showin, &showout);

return 0;
}

// 15.5.2. RegisterRTE Page 1274
void initAdminSingleAxis(void)
// ==============================
{
int iEventMask;

MMC_MOTIONPARAMS_SINGLE stSingleDefault;

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

AxisA.InitAxisData("a01", ComHndl);

/* Init default Gmas Parameters */
stSingleDefault.fEndVelocity = 0;
stSingleDefault.dbDistance = 100000;
stSingleDefault.dbPosition = 0;
stSingleDefault.fVelocity = 100000;
stSingleDefault.fAcceleration = 2000000;
stSingleDefault.fDeceleration = 10000000;

### PDF page 2273
<a id="pdf-page-2273"></a>
stSingleDefault.fJerk = 200000000;
/* MC_POSITIVE_DIRECTION, MC_SHORTEST_WAY, */
/* MC_NEGATIVE_DIRECTION, MC_CURRENT_DIRECTION */
stSingleDefault.eDirection = MC_POSITIVE_DIRECTION;
stSingleDefault.eBufferMode = MC_BUFFERED_MODE;
stSingleDefault.ucExecute = 1;

AxisA.SetDefaultParams(stSingleDefault);
}

void initAdminMultiAxis()
// =========================
{
// Source class:
// MMC_CONNECT_HNDL ComHndl;
// CMMCSingleAxis AxisA, AxisB;
// CMMCGroupAxis Group;

AxisB.InitAxisData("a02", ComHndl);
Group.InitAxisData("v01", ComHndl);

AxisARef = AxisA.GetRef();
AxisBRef = AxisB.GetRef();

Group.AddAxisToGroup(AxisARef, NC_NODE_1_ID);
Group.AddAxisToGroup(AxisBRef, NC_NODE_2_ID);
}

void endAdminSingleAxis(void)
// =============================
{
MMC_CloseConnection(ComHndl) ;
}

void endAdminMultiAxis(void)
// ================================
{
// Source class:
// CMMCGroupAxis Group;

Group.RemoveAxisFromGroup(NC_NODE_1_ID);
Group.RemoveAxisFromGroup(NC_NODE_2_ID);
}
/*================ Administration functions END ========================*/

/*================ Scenario functions STR =============================*/
void SnroMoveAbsolute(int trace)
// ================================
{
printf("%s%s -%d- ", strStrSnro, __func__, trace);

initAdminSingleAxis();

### PDF page 2274
<a id="pdf-page-2274"></a>
AxisA.PowerOn(MC_BUFFERED_MODE);
WaitFbDone(NC_AXIS_STAND_STILL_MASK, &AxisA);

MoveAbsoluteMoves();

AxisA.PowerOff(MC_BUFFERED_MODE);
WaitFbDone(NC_AXIS_DISABLED_MASK, &AxisA);

endAdminSingleAxis();

printf("%s%s -%d- %s", strEndSnro1, __func__, trace, strEndSnro2);
}

void SnroEnableDisableMotionEndedEvent(int trace)
// =================================================
{
printf("%s%s -%d- ", strStrSnro, __func__, trace);

initAdminSingleAxis();
gConn.RegisterEventCallback(MMCPP_MOTIONENDED, (void*
)EndMotionEventCB);
/* Register the callback function for Modbus and
Emergency: */
gConn.RegisterEventCallback(MMCPP_MODBUS_WRITE,(void*)ModbusWrite_Recei
ved) ;
gConn.RegisterEventCallback(MMCPP_EMCY,(void*)Emergency_Received) ;

AxisA.PowerOn(MC_BUFFERED_MODE);
WaitFbDone(NC_AXIS_STAND_STILL_MASK, &AxisA);

EnableDisableMotionEndedEvent();

AxisA.PowerOff(MC_BUFFERED_MODE);
WaitFbDone(NC_AXIS_DISABLED_MASK, &AxisA);
endAdminSingleAxis();

gConn.RegisterEventCallback(MMCPP_MOTIONENDED, NULL);

printf("%s%s -%d- %s", strEndSnro1, __func__, trace, strEndSnro2);
}

// 15.4.14. GroupEnable Page 1208
// 15.4.15. GroupDisable Page 1209
void SnroDepthName(int trace)
// =============================
{
printf("%s%s -%d- ", strStrSnro, __func__, trace);

initAdminSingleAxis();
initAdminMultiAxis();

AxisA.PowerOn(MC_BUFFERED_MODE);
WaitFbDone(NC_AXIS_STAND_STILL_MASK, &AxisA);

AxisB.PowerOn(MC_BUFFERED_MODE);
WaitFbDone(NC_AXIS_STAND_STILL_MASK, &AxisB);

### PDF page 2275
<a id="pdf-page-2275"></a>
Group.GroupEnable();

DepthName();

Group.GroupDisable();

AxisB.PowerOff(MC_BUFFERED_MODE);
AxisA.PowerOff(MC_BUFFERED_MODE);
WaitFbDone(NC_AXIS_DISABLED_MASK, &AxisB);
WaitFbDone(NC_AXIS_DISABLED_MASK, &AxisA);

endAdminMultiAxis();
endAdminSingleAxis();

printf("%s%s -%d- %s", strEndSnro1, __func__, trace, strEndSnro2);
}

void SnroConnection(int trace)
// ==============================
{
printf("%s%s -%d- ", strStrSnro, __func__, trace);

initAdminSingleAxis();

ConnectionTypeAndNum();

endAdminSingleAxis();

printf("%s%s -%d- %s", strEndSnro1, __func__, trace, strEndSnro2);
}

/*============================= Example functions STR
======================================*/

void EnableDisableMotionEndedEvent(void)
// ========================================
{
int loopInd;

printf("\n Function: %s:", __func__);
for (loopInd = 0; loopInd < 2; loopInd++)
{
if ((loopInd % 2) == 0)
{
printf("\n +++++++++ On end of motion EXPECT: <%s> : ",
EndMotionEventCB_MESSAGE);
AxisA.EnableMotionEndedEvent();
}
else
{
printf("\n +++++++++ On end of motion NOT EXPECT: <%s> : ",
EndMotionEventCB_MESSAGE);
AxisA.DisableMotionEndedEvent();
}

### PDF page 2276
<a id="pdf-page-2276"></a>
printf("\n +++++++++ Motion started...");
MoveAbsoluteMoves();
WaitFbDone(NC_AXIS_STAND_STILL_MASK, &AxisA);
printf("\n +++++++++ Motion End \n");
}
}

// 16.4.14. GroupEnable 1208
// 16.4.15. GroupDisable 1209
void DepthName(void)
// ====================
{
unsigned int iVal1, iVal2, iVal3;

printf("\n Function: %s:", __func__);
iVal1 = AxisB.GetFbDepth();

Group.GroupDisable();
AxisB.PowerOff(MC_BUFFERED_MODE);

iVal2 = AxisB.GetFbDepth();
WaitFbDone(NC_AXIS_DISABLED_MASK, &AxisB);
iVal3 = AxisB.GetFbDepth();

printf("\n +++++ oldFb=%d B4WaitDis=%d, AftWaitDis=%d +++++", iVal1,
iVal2,iVal3);

AxisB.PowerOn(MC_BUFFERED_MODE);
WaitFbDone(NC_AXIS_STAND_STILL_MASK, &AxisB);
Group.GroupEnable();

iVal1 = AxisA.GetAxisByName("a01"); /* Expected 0 */
iVal2 = AxisB.GetAxisByName("a02"); /* Expected 1 */

iVal3 = Group.GetGroupAxisByName("v01"); /* Expected 256 */

printf("\n +++++ Reff: a01=%d a02=%d, v01=%d +++++", iVal1, iVal2,
iVal3);
/*
* iVal2 = Group.GetGroupAxisByName("v02");
*/
}

void MoveAbsoluteMoves(void)
// ============================
{
printf("\n Function: %s:", __func__);
/* Move to -400000 at default speed: */
AxisA.MoveAbsolute(-40000.0);
/* Move to -200000 at speed 5000000.0 */
/* update default speed to 5000000 */
AxisA.MoveAbsolute(-200000.0, 5000000.0);
/* Change the default parameters */
AxisA.m_fAcceleration = 1000000.0;

### PDF page 2277
<a id="pdf-page-2277"></a>
AxisA.m_fDeceleration = 5000000.0;
AxisA.m_fVelocity = 100000.0;
/* Move to -300000 at default velocity */
/* v=100000 which become the new def V */
AxisA.MoveAbsolute(-300000.0);
/* Move to 310000 at velocity 80000.0 */
/* new def v=80000 */
AxisA.MoveAbsolute(310000.0, 80000.0);
/* Move abs to: 400000, with parameters: */
/* Speed=500000, Acc=1000000, Dec=1500000,*/
/* Jerk=20000000, buffer mode= */
/* MC_BUFFERED_MODE (def) */
AxisA.MoveAbsolute(400000, 500000, 1000000, 1500000, 20000000);
/* Move abs to 350000 with parameters from */
/* above command which become the default: */
/* Speed 500000, Acc=1000000 */
/* Dec=1500000, Jerk=20000000, */
/* buffer mode=MC_BUFFERED_MODE (def) */
AxisA.MoveAbsolute(350000);
}

int CallbackFunc(unsigned char* recvBuffer, short recvBufferSize, void*
lpsock)
//
===========================================================================
=====
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

### PDF page 2278
<a id="pdf-page-2278"></a>
case TABLE_UNDERFLOW_EVT:
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
===========================================================================
============================
{
printf("\n APP: MMCPPExitClbk: Run time Error in function %s, axis
ref=%d, err=%d, status=%d, bye\n",
msg, usAxisRef, sErrorID, usStatus);
fflush(stdout); fflush(stderr);

MMC_CloseConnection(uiConnHndl);
exit(0);
}

void EndMotionEventCB(unsigned short usAxisRef)
// ===============================================
{
printf("\n Function: %s: usAxisRef=%d ", __func__, (int)usAxisRef);
printf("\n\t\t %s \n", EndMotionEventCB_MESSAGE);
fflush(stdout); fflush(stderr);
}

/* Callback Function once a Modbus message is received. */
void ModbusWrite_Received()
// ===========================
{
printf("\n %s Received ", __func__) ;
fflush(stdout); fflush(stderr);
}

/* Callback Function once an Emergency is received. */
void Emergency_Received(unsigned short usAxisRef, short sEmcyCode)
// ==================================================================

### PDF page 2279
<a id="pdf-page-2279"></a>
{
printf("\n %s: Received on Axis %d. Code: %x ", __func__, usAxisRef,
sEmcyCode) ;
fflush(stdout); fflush(stderr);
}

// 15.6.11. SetHeartBeatConsumer 1248
// 15.7.4. GetNetworkInfo 1337
enum
{
eCOMM_TYPE_NONE = 0,
eCOMM_TYPE_ETHERCAT,
eCOMM_TYPE_CAN
};

void ConnectionTypeAndNum(void)
// ===============================
//
{
unsigned int uiHeartbeatTimeFactor;
int rt;
int NumFoundAmp;
char * cErrotStr;
CMMCNetwork CNet;
MMC_NETWORKINFO_OUT CanOutParams; // Can drv connection
MMC_GETCOMMSTATISTICS_OUT EthCatOutParams; // Ethcat drv connection

printf("\n Function: %s ", __func__);

CNet.SetConnHndl(ComHndl);
/* Connection type - CAN/EtherCAT */
rt = (int)gConn.GetGlobalBoolParameter(MMC_CONNECTION_TYPE_PARAM, 0);

if (rt == eCOMM_TYPE_ETHERCAT)
{
/* !!! ETERCAT !!!*/
rt = CNet.GetCommStatistic(EthCatOutParams);
if (rt != 0)
{
cErrotStr = "EtherCat failed, get statistic";
goto ConnectionTypeAndNum_exit_err;
}
NumFoundAmp = (int)EthCatOutParams.usNumOfSlaves;
printf("\n >>>>>>>>>>> %d drivers are connecting to GMAS through
ETERCAT net. ", NumFoundAmp);

/* Send and recive from UDP soket to driv */
SendReciveFromEthercat(NumFoundAmp);
}
else if (rt == eCOMM_TYPE_CAN)
{
/* !!! CAN !!!*/
/* !!! hard bit should be set in resource file*/
/* take from the resource file and actual connected... */
uiHeartbeatTimeFactor = 1; /* On for Every Cycle */

### PDF page 2280
<a id="pdf-page-2280"></a>
gConn.SetHeartBeatConsumer(uiHeartbeatTimeFactor);

rt = CNet.GetNetworkInfo (CanOutParams);
if (rt != 0)
{
cErrotStr = "Can failed, get NetworkInfo";
goto ConnectionTypeAndNum_exit_err;
}
NumFoundAmp = CanOutParams.iNumOfActiveNodes;
printf("\n >>>>>>>>>>> %d drivers are connecting to GMAS through CAN
net. ", NumFoundAmp);
}

/* Get & Set Default Mapping Digital Output */
SetGetDefDigOutput();

return;

ConnectionTypeAndNum_exit_err:
printf("\n>>> %s: *** %s %d ", __func__, cErrotStr, rt);
return;
}

/* Should create sockets according to actual number of amplifire (param
NumAmp) */
/* ...for demo (examples etc...) assume at least two Amp. exist.
*/
// 16.18.1. Create 1302
// 16.18.2. SendTo 1303
// 16.18.3. ReceiveFrom 1304
//
// 16.18.11. ElmoGetAn array 1310
// 16.18.12. ElmoGetParameter 1311

void SendReciveFromEthercat(int NumAmp)
// =======================================
{
int rt_val = 0;
int iBv;
bool bWait;
float fValue;
/* IPC type of app. */
char sAxisName[50] ;
CMMCUDP cUDP1,
cUDP2;
CMMCEoE gEoe;

printf("\n Function: %s ", __func__);

rt_val = cUDP1.Create("192.168.1.5", 5001, 0); /* Ip of first drive
(Gmas "last part IP" + 1) */
rt_val = cUDP1.SendTo("vr\r", 3);
rt_val = cUDP1.ReceiveFrom(sAxisName, 50, 100);
sAxisName[rt_val] = 0;
printf("\n Axis 0 Version: <%s> ", sAxisName);

rt_val = cUDP2.Create("192.168.1.6", 5001); /* Ip of second Gmas */

### PDF page 2281
<a id="pdf-page-2281"></a>
rt_val = cUDP2.SendTo("vr\r", 3) ;
rt_val = cUDP2.ReceiveFrom(sAxisName, 50, 100);
sAxisName[rt_val] = 0;
printf("\n Axis 1 Version: <%s> ", sAxisName);

rt_val = gEoe.Connect("192.168.1.5", 5001, bWait);
if (bWait == true)
{
usleep(10000); /* Wait 10 mili */
if (gEoe.IsWritable() == false)
{
printf("\n>>> %s: *** Amp (Drv) connection it not writable... ",
__func__);
}
}
/* AN[6] is command for get PsHv */
/* return 0 on succceed otherwise 1.*/
rt_val = gEoe.ElmoGetAn array("AN", 6, fValue);
if (rt_val != 0)
{
printf("\n>>> %s: *** Can't get EthCat Driver psHv AN[6] ",
__func__);
}
else
{
printf("\n>>> Max Power Supplay High Voltage for this driver
(ip=192.168.1.5) is %3.1f ", fValue);
}

/* BV - Maximum Motor DC Voltage (return int) */
/* return 0 on succeed, otherwise 1. */
rt_val = gEoe.ElmoGetParameter("BV", iBv);
if (rt_val != 0)
{
printf("\n>>> %s: *** Can't get Bus Driver HV ", __func__);
}
else
{
printf("\n>> Actual Bus Driver (ip=192.168.1.5) Hv is %d ", iBv);
}

gEoe.Close();
}

// 16.3.27. GetDigOutputs32Bit 1168
// 16.3.29. SetDigOutputs32Bit 1169
/* Get & Set Default Mapping Digital Output */
void SetGetDefDigOutput(void)
// =============================
{
unsigned long ulDigOutputs32bit;

printf("\n Function: %s ", __func__);

### PDF page 2282
<a id="pdf-page-2282"></a>
/* Read Digital output group 0 state */
ulDigOutputs32bit = AxisA.GetDigOutputs32bit(0);
printf("\n>>> %s: B4 action: DigOutputs32bit[#0]=0x%x ", __func__,
(unsigned int)ulDigOutputs32bit);

/* Change specific bit state of digital Output group 0 */
if ((ulDigOutputs32bit & 0x10000) != 0x00000)
{
/* ReSet specific bit of Digital output group 0 to state "0" */
AxisA.SetDigOutputs32Bit(ulDigOutputs32bit & 0xfffeffff); /* E.g:
disconnedt ps... */
}
else
{
/* Set specific bit of Digital output group 0 to state "1" */
AxisA.SetDigOutputs32Bit(ulDigOutputs32bit | 0x10000); /* E.g:
connect ps... */
}

ulDigOutputs32bit = AxisA.GetDigOutputs32bit(0);
printf("\n>>> %s: Aft action: DigOutputs32bit[#0]=0x%x ", __func__,
(unsigned int)ulDigOutputs32bit);
}

/*=================== Example functions END ===========================*/

/*================= Output STR =================================*/
#ifdef PROGRAM_OUTPUT
#endif /* PROGRAM_OUTPUT */
/*=================== Output END =================================*/

### PDF page 2283
<a id="pdf-page-2283"></a>
24.13.2 GetCommDiagnostics and ResetCommDiagnostics
Refer to the section MMC_GetEthercatCommStatistics and MMC_ResetCommDiagnostics for details of the
description, scope, and communication mode.
void GetCommDiagnostics(
MMC_GETCOMMDIAGNOSTICS_OUT& stOutParams
) throw (CMMCException);
void ResetCommDiagnostics(
MMC_RESETCOMMDIAGNOSTICS_OUT& stOutParams
) throw (CMMCException);
Source GMAS\includes\CPP\MMCNetwork.h
.NET Definition
Parameters
stOutParams
Defined output parameters function of MMC_GETCOMMDIAGNOSTICS_OUT, and
MMC_RESETCOMMDIAGNOSTICS_OUT respectively.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2284
<a id="pdf-page-2284"></a>
24.13.3 ResetCommStatistics
Refer to the section MMC_ResetCommStatistics for details of the description, scope, and communication
mode.
void ResetCommStatistics(
MMC_RESETCOMMSTATISTICS_OUT& stOutParams
) throw (CMMCException);
Source GMAS\includes\CPP\MMCNetwork.h
.NET Definition
Parameters
stOutParams
Defined output parameters function of MMC_RESETCOMMSTATISTICS_OUT.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name The axis reference Error ID
Status of the axis.

### PDF page 2285
<a id="pdf-page-2285"></a>
24.13.4 GetNetworkInfo
Refer to the section MMC_NetworkInfo for details of the description, scope, and communication mode.
int GetNetworkInfo(
MMC_GETCOMMSTATISTICS_OUT& stOutParams
) throw (CMMCException);
int GetNetworkInfo(
MMC_NETWORKINFO_OUT& stOutParams
) throw (CMMCException);
Source GMAS\includes\CPP\MMCNetwork.h
.NET Definition
Parameters
stOutParams
Defined output parameters function of MMC_GETCOMMSTATISTICS_OUT and
MMC_NETWORKINFO_OUT.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.13.1.
