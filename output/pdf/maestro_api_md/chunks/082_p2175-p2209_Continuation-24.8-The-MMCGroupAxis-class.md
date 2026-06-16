# Continuation - 24.8 The MMCGroupAxis class

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 2175-2209
- Chunk: `082_p2175-p2209_Continuation-24.8-The-MMCGroupAxis-class.md`

## Active Outline At Chunk Start
- p. 1705 - Chapter 24 Programming in C++
  - p. 2055 - 24.8 The MMCGroupAxis class

## Contained Bookmark Outline
  - p. 2190 - 24.9 The MMCDS401Axis class
  - p. 2200 - 24.10 The MMCDS406Axis class
  - p. 2203 - 24.11 The MMCECATIO class

## Extracted Text

### PDF page 2175
<a id="pdf-page-2175"></a>
24.8.50 AddAxisToGroup
Refer to the section MMC_AddAxisToGroup for details of the description, scope, and motion mode.
void AddAxisToGroup(
NC_NODE_HNDL_T hNode,
NC_IDENT_IN_GROUP_ENUM eIdentInGroup
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
hNode
The NC_NODE_HNDL_T enumerator defines the Node handle transition. The axis ref
parameter.
hNode can have any positive numeric value.
eIdentInGroup
The NC_IDENT_IN_GROUP_ENUM enumerator identifies the order and Nodes in the
group of the added axis. Performed via an enumerator to give the different axes a name
in the order, which can be coupled to the names in the kinematic model. The options
are:
NC_NODE_1_ID = 0
...............
NC_NODE_16_ID = 15
throw (CMMCException)
Refer to the sectionMMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section MMCGroupAxis Class Functions Code Example 1.

### PDF page 2176
<a id="pdf-page-2176"></a>
24.8.51 GroupReadActualPosition
Refer to the section MMC_GroupReadActualPosition for details of the description, scope, and motion
mode.
int GroupReadActualPosition(
MC_COORD_SYSTEM_ENUM eCoordSystem,
double dbPosition[NC_MAX_NUM_AXES_IN_NODE]
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def MMC_GroupReadActualPosition(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_GroupReadActualPosition(hConn, hAxisRef, pInParam,
pOutParam)
Parameters
MC_COORD_SYSTEM_ENUM eCoordSystem
Define the types of supported coordinate systems. The MC_COORD_SYSTEM_ENUM
enumerator options are:
MC_NONE_COORD = 0
MC_ACS_COORD = 1
MC_MCS_COORD = 2
MC_PCS_COORD = 3
dbPosition[]
Target position for the motion of the axis when conditions are met. Any negative or
positive double values in technical unit [u].
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2177
<a id="pdf-page-2177"></a>
24.8.52 GroupStop
Refer to the section MMC_GroupStop for details of the description, scope, and motion mode.
void GroupStop(
float fDeceleration,
float fJerk,
MC_BUFFERED_MODE_ENUM eBufferMode
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
fDeceleration
Float value of the deceleration when stopping (decreasing energy of the motor). Any
positive float value in u/s2
fJerk
Float value of the Jerk. Any positive value in u/s3
eBufferMode
Refer to the structure MMC_GROUPSTOP_IN Structure for further details.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2178
<a id="pdf-page-2178"></a>
24.8.53 GroupHalt
Refer to the section MMC_GroupHalt for details of the description, scope, and motion mode.
void GroupHalt(
float fDeceleration,
float fJerk,
MC_BUFFERED_MODE_ENUM eBufferMode
) throw (CMMCException);
Overloaded
void GroupHalt(float fDeceleration, float fJerk, MC_BUFFERED_MODE_ENUM
eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);

void GroupHalt(float fDeceleration, MC_BUFFERED_MODE_ENUM eBufferMode =
MC_ABORTING_MODE
) throw (CMMCException);

void GroupHalt(MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def MMC_GroupHaltCmd(hConn, hAxisRef, pInParam, pOutParam):
return _mmcpp_lib.MMC_GroupHaltCmd(hConn, hAxisRef,
pInParam, pOutParam)
def GroupHalt(self, fDeceleration, fJerk, eBufferMode):
return _mmcpp_lib.CMMCGroupAxis_GroupHalt(self,
fDeceleration, fJerk, eBufferMode)
Parameters
fDeceleration
Float value of the deceleration when stopping (decreasing energy of the motor). Any
positive float value in u/s2
fJerk
Float value of the Jerk. Any positive value in u/s3
eBufferMode
Refer to the structure MMC_GROUPHALT_IN Structure for further details.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2179
<a id="pdf-page-2179"></a>
24.8.54 MoveLinearAbsoluteRepetitive
Refer to the function block in section MMC_MoveLinearAbsoluteRepetitive for details of the description,
scope, and motion mode.
int MoveLinearAbsoluteRepetitive(
float fVelocity, | double dbPosition[NC_MAX_NUM_AXES_IN_NODE],
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
fVelocity
Value of the maximum velocity (not necessarily reached). Any negat ive or positive
double values in technical unit [u].
dbPosition
Target position for the motion of the axis when conditions are met. Any negative or
positive double values in technical unit [u].
An array of coordinates, incl. positions and orientations (Distance if Mode = RELATIVE).
The array parameter NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as
the maximum number of axis in a group.
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
eBufferMode
Refer to the MMC_MOVELINEARABSOLUTEREPETITIVE_IN Structure.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2180
<a id="pdf-page-2180"></a>
24.8.55 MoveLinearRelativeRepetitive
Refer to MMC_MoveLinearRelativeRepetitive for details of the description, scope, and motion mode.
int MoveLinearRelativeRepetitive(
float fVelocity, | double dbDistance[NC_MAX_NUM_AXES_IN_NO DE],
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
fVelocity
Value of the maximum velocity (not necessarily reached). Any negative or positive
double values in technical unit [u].
dbDistance
An array [1..N] of relative distances for each dimension in the specified coordinate
system, with N being vendor specific. The array parameter
NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as the maximum number
of axis in a group.
dbDistance is a double vector array in technical unit [u].
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
eBufferMode
Refer to the MMC_MOVELINEARRELATIVEREPETITIVE_IN Structure
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2181
<a id="pdf-page-2181"></a>
24.8.56 MovePolynomAbsolute
Refer to MMC_MovePolynomAbsolute for details of the description, scope, and motion mode.
int MovePolynomAbsolute(
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
float fVelocity, | double dbAuxPoint[NC_MAX_NUM_AXES_IN_NODE] |
double dbDistance[NC_MAX_NUM_AXES_IN_NODE] | double dbPosition[NC_MAX_NUM_AXES_IN_NODE] |
float fAcceleration | float fDeceleration | float fJerk
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
Refer to the MMC_MOVELINEARRELATIVEREPETITIVE_IN Structure.
fVelocity
Value of the maximum velocity (not necessarily reached). Any negative or positive
double values in technical unit [u].
dbAuxPoint[NC_MAX_NUM_AXES_IN_NODE]
An array [1..N] of relative distances for each dimension in the specified coordinate
system, with N being vendor specific. The array parameter
NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as the maximum number
of axis in a group.
dbDistance is a double vector array in technical unit [u].
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
dbDistance[NC_MAX_NUM_AXES_IN_NODE]
An array [1..N] of relative distances for each dimension in the specified coordinate
system, with N being vendor specific. The array parameter
NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as the maximum number
of axis in a group.
dbDistance is a double vector array in technical unit [u].
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
dbPosition[NC_MAX_NUM_AXES_IN_NODE]
Target position for the motion of the axis when conditions are met. Any negative or
positive double values in technical unit [u].
An array of coordinates, incl. positions and orientations (Distance if Mode = RELATIVE).
The array parameter NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as
the maximum number of axis in a group.
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].

### PDF page 2182
<a id="pdf-page-2182"></a>
fAcceleration
Value of the acceleration (increasing energy of the motor). Any positive float value in
u/s2.
fDeceleration
Float value of the deceleration when stopping (decreasing energy of the motor). Any
positive float value in u/s
2
fJerk
Maximum float value of the Jerk. Any positive value in u/s3
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2183
<a id="pdf-page-2183"></a>
24.8.57 MoveLinearAdditive
Refer to the sections MMC_MoveLinearAdditive and MMC_MoveLinearAdditiveEx for details of the
description, scope, and motion mode. The MMC_MoveLinearAdditiveEx parameter is for further accuracy in
setting the parameters. The double parmeters allow setting of an 8 bit value.
int MoveLinearAdditive(
[float fVelocity,]
[double dbDistance[NC_MAX_NUM_AXES_IN_NODE],]
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
fVelocity
Value of the maximum velocity (not necessarily reached). Any negative or positive
double values in technical unit [u].
dbDistance
An array [1..N] of relative distances for each dimension in the specified coordinate
system, with N being vendor specific. The array parameter
NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as the maximum number
of axis in a group.
dbDistance is a double vector array in technical unit [u].
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
Refer to the MMC_MOVELINEARADDITIVE_IN Structure.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2184
<a id="pdf-page-2184"></a>
24.8.58 MovePath
Refer to section MMC_MovePath for details of the description, scope, and motion mode.
void MovePath(
MC_PATH_REF hMemHandle,
[float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE]]
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
[MC_COORD_SYSTEM_ENUM eCoordSystem = MC_MCS_COORD]
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
hMemHandle
MC_PATH_REF enumerator handle to a journal entry where the pointer to the shared
memory is located. MC_PATH_REF is the journal entry path reference.
hMemHandle has integer values.
float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE
Depending on the transition mode, different supplier specific transition parameters can
be used which characterize the contour curve. The array parameter
NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as the maximum number
of axis in a group.
fTransitionParameter can have any positive float value in appropriat e units, dependant
on the TransitionMode parameter. Refer to the section Special Robot Transformations .
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
The MC_BUFFERED_MODE_ENUM enumerator defines the behavior of the axis. Refer
to the MMC_MOVEPATH_IN Structure or further details. Modes are as follows, but
only the Buffered Mode is supported:
MC_ABORTING_MODE = 1
MC_BUFFERED_MODE = 2,
MC_BLENDING_LOW_MODE = 3
MC_BLENDING_PREVIOUS_MODE = 4
MC_BLENDING_NEXT_MODE = 5
MC_BLENDING_HIGH_MODE = 6
Aborting Default mode without buffering. The next function block aborts an
ongoing motion and the command affects the axis immediately.
The buffer is cleared. This motion will be executed regardless of
the Boolean ucExecute status which may be False(0) or True(1).
Buffered The next function block affects the axis as soon as the previous

### PDF page 2185
<a id="pdf-page-2185"></a>
movement is completed.
BlendingLow The next function block controls the axis after the previous
function block has finished (equivalent to buffered), but the axis
will not stop between the movements. The velocity is blended with
the lowest velocity of both commands (1 and 2) at the first end-
position (1).
BlendingPreviou
s
Blending with the velocity of function block 1 at the end-position
of this block
BlendingNext Blending with the velocity of function block 2 at end-position of
function block1
BlendingHigh Blending with highest velocity of function block 1 and function
block 2 at end-position of function block1.
MC_COORD_SYSTEM_ENUM eCoordSystem
Define the types of supported coordinate systems. The MC_COORD_SYSTEM_ENUM
enumerator options are:
MC_NONE_COORD = 0
MC_ACS_COORD = 1
MC_MCS_COORD = 2
MC_PCS_COORD = 3
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2186
<a id="pdf-page-2186"></a>
24.8.59 PathDeselect
Refer to MMC_PathUnselect for details of the description, scope, and motion mode.
int PathDeselect(
MC_PATH_REF hMemHandle
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
MC_PATH_REF hMemHandle
MC_PATH_REF enumerator handle to a journal entry where the pointer to the shared mem ory is
located. MC_PATH_REF is the journal entry path reference.
hMemHandle has integer values.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name The axis reference
Error ID Status of the axis.

### PDF page 2187
<a id="pdf-page-2187"></a>
24.8.60 PathSelect
Refer to the function MMC_PathSelect for details of the description, scope, and motion mode.
unsigned int PathSelect(
[unsigned char ucExecute = 1]
MC_PATH_DATA_REF pPathToSplineFile
[MC_COORD_SYSTEM_ENUM eCoordSystem]
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
ucExecute = 1
Start the execution command at the rising edge. Boolean TRUE/FALSE values.
MC_PATH_DATA_REF pPathToSplineFile
This string describes where the splines data file is located. Values accepted are any
characters describing a file path.
MC_PATH_DATA_REF Where the enumerator MC_PATH_DATA_REF describes the I/O
definition of the path data reference using the array
[NC_MAX_SPLINES_FILE_PATH_LENGTH] that defines the
maximum length of the splines file path data.
MC_PATH_DATA_REF can have values of any characters.
NC_MAX_SPLINES_FILE_PATH_LENGTH can have any numeric
value.
MC_COORD_SYSTEM_ENUM eCoordSystem
Define the types of supported coordinate systems. The MC_COORD_SYSTEM_ENUM
enumerator options are:
MC_NONE_COORD = 0
MC_ACS_COORD = 1
MC_MCS_COORD = 2
MC_PCS_COORD = 3
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.

### PDF page 2188
<a id="pdf-page-2188"></a>
24.8.61 PathGetLengths
Retrieves the length values of specified segments in a spline tabl e. The buffer must comply to the number of
values, which the programmer expects to recieve and cannot be greater than 170 elements.
unsigned int PathGetLengths(
MC_PATH_REF hMemHandle,
unsigned int uiStartIndex,
unsigned int uiNumOfSegments,
double *dbValues
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
MC_PATH_REF hMemHandle
MC_PATH_REF enumerator handle to a journal entry where the pointer to the shared memory
is located. MC_PATH_REF is the journal entry path reference.
hMemHandle has integer values.
uiStartIndex
Specifies the table segment from which to start collecting the length values of specified
segments in a spline table. Any positive value.
uiNumOfSegments
Specifies the number of segments for the length values collection. Any positive value.
dbValues
The buffer in which this function stores the collected values. Any positive values.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.

### PDF page 2189
<a id="pdf-page-2189"></a>
24.8.62 EthercatWriteMemoryRange
Refer to MMC_SetKinTransformEx for details of the description, scope, and motion mode.
void EthercatWriteMemoryRange(
unsigned short usRegAddr,
unsigned char ucLength,
unsigned char pData[ETHERCAT_MEMORY_WRITE_MAX_SIZE]
) throw (CMMCException){return;}
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
MC_KIN_REF_DELTA stDelta
Refer to the parameter definition in the function MMC_SetKinTransformEx for details.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2190
<a id="pdf-page-2190"></a>
##### 24.9 The MMCDS401Axis class
24.9 The MMCDS401Axis class
The class MMCDS401Axis class wraps the DS-401 parameter functions detailed for the C function
MMC_SendSDO. The class MMCDS401Axis retains the same field parameter properties and values described in
this document for the C function blocks, and while small visual changes may be made to some variables, these
are transparent, and do not change the operation of the variable.

Figure 544 Fields and methods of the CMMCDS401Axis class
The detailed class view shown in Figure 544 describes the fields and methods associated with the
CMMCDS401Axis class. It should be noted that Private and Protected functions and their operation should be
transparent to the user, and are not for general application by the user.

### PDF page 2191
<a id="pdf-page-2191"></a>
24.9.1 ConfigGeneralRPDO3
This function configures the Maestro to receive a general PDO3 message. Refer to the section
MMC_ConfigGeneralRPDO3 for details of the description, and scope.
void ConfigGeneralRPDO3(
unsigned char ucEventType,
unsigned char ucPDOCommParam,
unsigned char ucPDOLength
) throw(CMMCException);
Source GMAS\includes\CPP\ CMMCDS401Axis.h
Python Definition def MMC_ConfigGeneralRPDO3(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_ConfigGeneralRPDO3(hConn,
hAxisRef, pInParam, pOutParam)
Parameters
ucEventType
Defines which group of events are to be transferred from the Maestro. Refer to the
section PDO Mapping the correct definition to be used. Any positive character values
are acceptable.
ucPDOCommParam
PDO communications parameter. Has the following positive character values:
PDO_COM_PARAM_SYNC 0x01
PDO_COM_PARAM_ASYNC 0xFF
PDO_COM_PARAM_EVENT 0xFE
PDO events are only possible when the input argument ucPDOCommParam, is
PDO_COM_PARAM_EVENT.
ucPDOLength
Indicates the number of bytes to be sent as an RPDO, RPDO message. It can contain 1 -8
bytes of data.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2192
<a id="pdf-page-2192"></a>
24.9.2 ConfigGeneralRPDO4
This function configures the Maestro to receive a general PDO3 message. Refer to the section
20.1.8MMC_ConfigGeneralTPDO4 for details of the description, and scope.
void ConfigGeneralRPDO4(
unsigned char ucEventType,
unsigned char ucPDOCommParam,
unsigned char ucPDOLength
) throw(CMMCException);
Source GMAS\includes\CPP\ CMMCDS401Axis.h
Python Definition def MMC_ConfigGeneralRPDO4(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_ConfigGeneralRPDO4(hConn,
hAxisRef, pInParam, pOutParam)
Parameters
ucEventType
Defines which group of events are to be transferred from the Maestro. Refer to the
section 19.3PDO Mapping the correct definition to be used. Any positive character
values are acceptable.
ucPDOCommParam
PDO communications parameter. Has the following positive character values:
PDO_COM_PARAM_SYNC 0x01
PDO_COM_PARAM_ASYNC 0xFF
PDO_COM_PARAM_EVENT 0xFE
PDO events are only possible when the input argument ucPDOCommParam, is
PDO_COM_PARAM_EVENT.
ucPDOLength
Indicates the number of bytes to be sent as an RPDO, RPDO message. It can contain 1 -8
bytes of data.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2193
<a id="pdf-page-2193"></a>
24.9.3 CancelGeneralRPDO3
This function cancels the Maestro configuration from receiving general PDO3 messages. Refer to the section
MMC_CancelGeneralRPDO3 for details of the description, and scope.
void CancelGeneralRPDO3(
) throw(CMMCException)
Source GMAS\includes\CPP\ CMMCDS401Axis.h
Python Definition def MMC_CancelGeneralRPDO3(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_CancelGeneralRPDO3(hConn,
hAxisRef, pInParam, pOutParam)

class MMC_CANCELGENERALRPDO3_IN(object):
ucDummy =
property(_mmcpp_lib.MMC_CANCELGENERALRPDO3_IN_ucDummy_get,
_mmcpp_lib.MMC_CANCELGENERALRPDO3_IN_ucDummy_set)

class MMC_CANCELGENERALRPDO3_OUT(object):
usStatus =
property(_mmcpp_lib.MMC_CANCELGENERALRPDO3_OUT_usStatus_get
, _mmcpp_lib.MMC_CANCELGENERALRPDO3_OUT_usStatus_set)
usErrorID =
property(_mmcpp_lib.MMC_CANCELGENERALRPDO3_OUT_usErrorID_ge
t, _mmcpp_lib.MMC_CANCELGENERALRPDO3_OUT_usErrorID_set)
Parameters

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2194
<a id="pdf-page-2194"></a>
24.9.4 CancelGeneralRPDO4
This function cancels the Maestro configuration from receiving general PDO4 messages. Refer to the section
MMC_CancelGeneralRPDO4 for details of the description, and scope.
void CancelGeneralRPDO4(
) throw(CMMCException)
Source GMAS\includes\CPP\ CMMCDS401Axis.h
.NET Definition
Parameters

throw (CMMCException)
Refer to the sectionMMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2195
<a id="pdf-page-2195"></a>
24.9.5 ConfigGeneralRPDO4
This function cancels the Maestro configuration from receiving general PDO4 messages. Refer to the section
MMC_CancelGeneralRPDO4 for details of the description, and scope.
void CancelGeneralRPDO4(
) throw(CMMCException)
Source GMAS\includes\CPP\ CMMCDS401Axis.h
.NET Definition
Parameters

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2196
<a id="pdf-page-2196"></a>
24.9.6 ConfigGeneralTPDO3
This function configures the Maestro to transmit general PDO3 messages. Refer to the section
20.1.7MMC_ConfigGeneralTPDO3 for details of the description, and scope.
void ConfigGeneralTPDO3(
unsigned char ucEventType
) throw(CMMCException)
Source GMAS\includes\CPP\ CMMCDS401Axis.h
.NET Definition
Parameters
ucEventType
Defines which group of events are to be transferred from the Maestro. Refer to the
section 19.3PDO Mapping the correct definition to be used. Any positive character
values are acceptable.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2197
<a id="pdf-page-2197"></a>
24.9.7 ConfigGeneralTPDO4
This function configures the Maestro to transmit general PDO43 messages. Refer to the section
20.1.8MMC_ConfigGeneralTPDO4 for details of the description, and scope.
void ConfigGeneralTPDO4(
unsigned char ucEventType
) throw(CMMCException)
Source GMAS\includes\CPP\ CMMCDS401Axis.h
.NET Definition
Parameters
ucEventType
Defines which group of events are to be transferred from the Maestro. Refer to the
section PDO Mapping the correct definition to be used. Any positive character values
are acceptable.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2198
<a id="pdf-page-2198"></a>
24.9.8 CancelGeneralTPDO3
This function cancels the Maestro configuration from transmitting general PDO3 messages. Refer to the
section MMC_CancelGeneralTPDO3 for details of the description, and scope.
void CancelGeneralTPDO3(
) throw(CMMCException)
Source GMAS\includes\CPP\ CMMCDS401Axis.h
Python Definition def MMC_CancelGeneralTPDO3(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_CancelGeneralTPDO3(hConn,
hAxisRef, pInParam, pOutParam)

class MMC_CANCELGENERALTPDO3_IN(object):
ucDummy =
property(_mmcpp_lib.MMC_CANCELGENERALTPDO3_IN_ucDummy_get,
_mmcpp_lib.MMC_CANCELGENERALTPDO3_IN_ucDummy_set)

class MMC_CANCELGENERALTPDO3_OUT(object):
usStatus =
property(_mmcpp_lib.MMC_CANCELGENERALTPDO3_OUT_usStatus_get
, _mmcpp_lib.MMC_CANCELGENERALTPDO3_OUT_usStatus_set)
usErrorID =
property(_mmcpp_lib.MMC_CANCELGENERALTPDO3_OUT_usErrorID_ge
t, _mmcpp_lib.MMC_CANCELGENERALTPDO3_OUT_usErrorID_set)
Parameters

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2199
<a id="pdf-page-2199"></a>
24.9.9 CancelGeneralTPDO4
This function cancels the Maestro configuration from transmitting general PDO4 messages. Refer to the
section MMC_CancelGeneralTPDO4 for details of the description, and scope.
void CancelGeneralRPDO3(
) throw(CMMCException)
Source GMAS\includes\CPP\ CMMCDS401Axis.h
Python Definition def MMC_CancelGeneralTPDO4(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_CancelGeneralTPDO4(hConn,
hAxisRef, pInParam, pOutParam)

class MMC_CANCELGENERALTPDO4_IN(object):
ucDummy =
property(_mmcpp_lib.MMC_CANCELGENERALTPDO4_IN_ucDummy_get,
_mmcpp_lib.MMC_CANCELGENERALTPDO4_IN_ucDummy_set)

class MMC_CANCELGENERALTPDO4_OUT(object):
usStatus =
property(_mmcpp_lib.MMC_CANCELGENERALTPDO4_OUT_usStatus_get
, _mmcpp_lib.MMC_CANCELGENERALTPDO4_OUT_usStatus_set)
usErrorID =
property(_mmcpp_lib.MMC_CANCELGENERALTPDO4_OUT_usErrorID_ge
t, _mmcpp_lib.MMC_CANCELGENERALTPDO4_OUT_usErrorID_set)
Parameters

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2200
<a id="pdf-page-2200"></a>
##### 24.10 The MMCDS406Axis class
24.10 The MMCDS406Axis class
The purpose of encoders is to detect positions of any kind of machine tools. Encoders detect positions and
transmit the position values or provide speed, acceleration, and jerk values, across the CANopen network. The
encoder may receive configuration information via SDO, and in the NMT state operation, the position value
may be transmitted using synchronous PDO. Additionally, the encoders may transmit a PDO asynchronously,
scheduled by the elapsing of the event timer.
The CANopen device profile defines two encoder classes, a standard device class 1 (C1) and an extended device
class 2 (C2). The standard device C1 specifies basic functions provided by each device. The C2 extended device
provides a variety of features with mandatory and optional functions. The mandatory functions of both, C1 and
C2, are necessary to ensure non-manufacturer specific operations of a device.
By defining mandatory device characteristics in C1, the operation of the basi c network and encoder is ensured.
By defining extended C2, a degree of defined flexibility may be built-in. By leaving 'hooks' for optional and
manufacturer-specific functions, the device developer is not constrained to an out -of-date standard.
The CiA DS406 device profile for encoders specifies the CANopen interface of absolute linear and rotary
encoders. Besides position and velocity output, the profile describes also acceleration and jerk outputs, and
specifies several configuration parameters, e.g. the code sequence (complement) that determines the counting
direction, in which the output code increases or decreases. The resolution parameter is used to configure a
given number of steps for each revolution. The profile specification covers complete cam fun ctionality with
hysteresis, and it is possible to describe multi-sensor modules implemented in a single CANopen encoder
device.
The encoder profile specifies the following operation modes:
Mode Profile
Event-timer Current position value is sampled and transmitted periodically.
Synchronous Current position is sampled and transmitted after the reception of the Sync
message.
The remote mode based on remotely requested PDOs is not recommended due to several general problems
that occur when CAN remote frames are used.
The class CMMCDS406Axis wraps the parameter functions. The class CMMCDS406Axis reta ins the same field
parameter properties and values described in this document for the C function blocks, and while small visual
changes may be made to some variables, these are transparent, and do not change the operation of the
variable.

### PDF page 2201
<a id="pdf-page-2201"></a>
Figure 545 Fields and methods of the CMMCDS406Axis class
The detailed class view shown in Figure 545 describes the fields and methods associated with the
CMMCDS406Axis class. It should be noted that Private and Protected functions and their operation should be
transparent to the user, and are not for general application by the user.

### PDF page 2202
<a id="pdf-page-2202"></a>
24.10.1 GetActualPosition
Refer to the similar function block described in section MMC_ReadActualPosition for details of the
description, scope, and motion mode.
double GetActualPosition(
) throw (CMMCException)
Source GMAS\includes\CPP\ MMCDS406Axis.h
.NET Definition
Parameters

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.
For code example, refer to the section 24.3.3.

### PDF page 2203
<a id="pdf-page-2203"></a>
##### 24.11 The MMCECATIO class
24.11 The MMCECATIO class
The class MMCECATIO wraps the parameter functions. The class MMCECATIO retains the same field parameter
properties and values described in this document for the C function blocks, and while small visual changes may
be made to some variables, these are transparent, and do not change the operation of the variable.

Figure 546 Fields and methods of the CMMCECATIO class
The detailed class view shown in Figure 546 describes the fields and methods associated with the MMCECATIO
class. It should be noted that Private and Protected functions and their operation should be transparent to the
user, and are not for general application by the user.

### PDF page 2204
<a id="pdf-page-2204"></a>
24.11.1 ECATIOEnableDIChangedEvent
Refer to the similar function block described in section MMC_ECATIOEnableDIChangedEvent for details of
the description, scope, and motion mode.
void ECATIOEnableDIChangedEvent(
) throw (CMMCException)
Source GMAS\includes\CPP\CMMCECATIO.h
.NET Definition
Parameters

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.

### PDF page 2205
<a id="pdf-page-2205"></a>
24.11.2 ECATIODisableDIChangedEvent
Refer to the similar function block described in section MMC_ECATIODisableDIChangedEvent for details of
the description, scope, and motion mode.
void ECATIODisableDIChangedEvent(
) throw (CMMCException)
Source GMAS\includes\CPP\CMMCECATIO.h
.NET Definition
Parameters

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.

### PDF page 2206
<a id="pdf-page-2206"></a>
24.11.3 ECATIOReadDigitalInput
Refer to the similar function block described in section MMC_ECATIOReadDigitalInput for details of the
description, scope, and motion mode.
(Unsigned Long Long) MMCPPULL_T ECATIOReadDigitalInput(
) throw (CMMCException)
Source GMAS\includes\CPP\CMMCECATIO.h
.NET Definition
Parameters
(Unsigned Long Long) MMCPPULL_T
Parameter is an 8 Byte variable with ECATIOReadDigitalInput having positive values
ranging from 0 to unlimited
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.

### PDF page 2207
<a id="pdf-page-2207"></a>
24.11.4 ECATIOWriteDigitalOutput
Refer to the similar function block described in section MMC_ECATIOWriteDigitalOutput for details of the
description, scope, and motion mode.
void ECATIOWriteDigitalOutput(
(Unsigned Long Long) MMCPPULL_T ulliDO
) throw (CMMCException)
Source GMAS\includes\CPP\CMMCECATIO.h
.NET Definition
Parameters
(Unsigned Long Long) MMCPPULL_T ulliDO
Parameter is an 8 Byte variable with ulliDO having positive values ranging from 0 to
unlimited
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID

Status of the axis.

### PDF page 2208
<a id="pdf-page-2208"></a>
24.11.5 ECATIOReadAnalogInput
Refer to the section MMC_ECATIOReadAnalogInput for details of the description, and scope.
short ECATIOReadAnalogInput(
unsigned char ucIndex
) throw(CMMCException)
Source GMAS\includes\CPP\CMMCECATIO.h
.NET Definition
Parameters
ucIndex
Analog input index. Any positive character value.
throw (CMMCException)
Refer to the section 24.1.1MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2209
<a id="pdf-page-2209"></a>
24.11.6 ECATIOWriteAnalogOutput
Refer to the section MMC_ECATIOWriteAnalogOutput for details of the description, and scope.
void ECATIOWriteAnalogOutput(
unsigned char ucIndex,
short sAOValue
) throw(CMMCException)
Source GMAS\includes\CPP\CMMCECATIO.h
.NET Definition
Parameters
ucIndex
Analog input index. Any positive character value.
sAOValue
Analog Output value. Any positive value.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
