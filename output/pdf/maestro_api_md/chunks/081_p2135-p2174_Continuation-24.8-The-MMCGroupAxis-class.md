# Continuation - 24.8 The MMCGroupAxis class

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 2135-2174
- Chunk: `081_p2135-p2174_Continuation-24.8-The-MMCGroupAxis-class.md`

## Active Outline At Chunk Start
- p. 1705 - Chapter 24 Programming in C++
  - p. 2055 - 24.8 The MMCGroupAxis class

## Extracted Text

### PDF page 2135
<a id="pdf-page-2135"></a>
MC_TM_CORNER_DIST_TC_POLYNOM = 6, Not supported
MC_TM_CORNER_DIST_CV_POLYNOM3 = 7,
MC_TM_CORNER_DIST_CV_POLYNOM5 = 8,
MC_TM_CORNER_DEVIATION_MODE_PLN6 = 9,
MC_TM_CORNER_DIST_CV_POLYNOM5_NAXES = 10,
MC_TM_CORNER_DIST_CV_POLYNOM7 = 11,
MC_TM_CORNER_DEVIATION_MODE_PLN8 = 12,
MC_TM_DIST1_DIST2_DEVIATION_PLN6 = 13,
MC_TM_DIST1_DIST2_DEVIATION_PLN8 = 14,
MC_TM_LAST_MODE
Refer to the Input Parameters at section MMC_MOVEEANGLE_IN Structure for details.
MC_BUFFERED_MODE_ENUM eBufferMode
Type: MC_BUFFERED_MODE_ENUM = MC_BUFFERED_MODE enumerator
MC_BUFFERED_MODE_ENUM defines the behavior of the axis. Enumerator modes are
as follows:
MC_ABORTING_MODE = 1
MC_BUFFERED_MODE = 2
MC_BLENDING_LOW_MODE = 3
MC_BLENDING_PREVIOUS_MODE = 4
MC_BLENDING_NEXT_MODE = 5
MC_BLENDING_HIGH_MODE = 6
Refer to the Input Parameters at section MMC_MOVEEANGLE_IN Structure for details.
MC_COORD_AXES ePlain
Type: MC_COORD_AXES enumerator
The selected plane. If none-selected, is 0 for old method. This parameter is added to
support angle mode with more than two axes.

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2136
<a id="pdf-page-2136"></a>
24.8.20 TrackSyncOut
This function block offers an abstraction layer for syncing out a tracking process. In short, a dynamic PCS to
MCS transition depends on master (RT/CB) axis position. Tthis command operates a real motion profiler . For
explanatory details refer to the MMC_TrackSyncOut function.
int TrackSyncOut(
unsigned short usMaster,
double (&dbMasterOrigin)[6],
double (&dbTargetPosition)[6],
double (&dbRampTrajectoryParams)[12],
double dbMasterScaling,
double dbTime,
double dbStopDeceleration,
unsigned char ucInstantly = 1
) throw (CMMCException);

int TrackSyncOut(MMC_TRACKSYNCOUT_IN& params
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
(&dbTargetPosition)[6]
Target position in MCS coordinate system. Relevant only for Non-Immediate mode of
operation. ARRAY[6] of LREAL[x,y,z,u,v,w]
(&dbRampTrajectoryParams)[12]
When the MMC_TrackSyncOut (TSO) function is called, a special RAMP motion is
generated from the initial axis location in PCS to TargetPosition in MCS coordinate
system.
In general, the Motion divided into 2 separate parts.
Z axis motion back to "Safe Z Height".
XY plane motion (none immediate mode of operation only). If mode of operation is

### PDF page 2137
<a id="pdf-page-2137"></a>
Immediate than X & Y positions are ignored.
For Sync-Out:
The reference parameter is 'Time' (DeltaT) instead of DeltaFi/AKA MasterSyncPosiiton.
ZSafe (index 0) defines an absolute position for safe zone of z axis.
[1] T1 percentage of DeltaT until motion by X,Y begins.
[2] T2 percentage of DeltaT until motion by X,Y is ended.
[3] T3 percentage of DeltaT for Z motion up to complete.
[4] T4 percentage of DeltaT for Z motion down to start.
dbMasterScaling
The scaling of the referenece axis relative to the position of the object on the Rotary
Table (Master) and the length of the Rotary Table. LREAL uu/radian values
dbTime
Time in seconds to complete the ramp out motion. LREAL values.
dbStopDeceleration
Override node definition. LREAL values.
ucInstantly = 1
A Boolean TRUE/FALSE flag defines whether the TargetPosition should relate to the
MasterInitialPosition (0) or to the Master Position at activation time (1).
Zsafe only {immediate (1)} or x,y as well {none immediate (0)}
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
MMC_TRACKSYNCOUT_IN& params Structure
typedef struct MMC_TRACKSYNCOUT_IN {
double dbMasterOrigin[6];
double dbTargetPosition[6];
double dbRampTrajectoryParams[12];
double dbMasterScaling;
double dbTime;
double dbStopDeceleration;
TRAJECTORY_MODE_ENUM eTrajectoryMode;
unsigned short usMaster;
unsigned char ucInstantly;
unsigned char ucExecute;
unsigned char futures[32];
} MMC_TRACKSYNCOUT_IN;

### PDF page 2138
<a id="pdf-page-2138"></a>
Parameters
All parameters
Refer to the section MMC_TRACKSYNCOUT Structure for details of the parameters.

### PDF page 2139
<a id="pdf-page-2139"></a>
24.8.21 SetNormalcyMode
This function sets parameters for normalcy mode of operation. It should be noted that this setting is linked
to the selected kinematic.
int SetNormalcyMode(
MMC_NORMALCY_TYPE_ENUM eType,
MMC_NORMALCY_PLANE_ENUM ePlane
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
MMC_NORMALCY_TYPE_ENUM eType
Normalcy mode of operation. Defined by the enumerator NC_NORMALCY_TYPE_ENUM
which describes the tangent knife mode of operations.
The mode NC_NORMALCY_NONE(0) is similar to the disable normalcy mode.
The enumerator defines the following modes:
typedef enum {
NC_NORMALCY_NONE, Normalcy disabled, ordinary motion
NC_NORMALCY_LEFT, Normal left absolute motion
NC_NORMALCY_RIGHT, Normal right absolute
NC_NORMALCY_TANGENT, Tangent absolute
NC_NORMALCY_RELATIVE, Relative to tangent direction
} MMC_NORMALCY_TYPE_ENUM;
MMC_NORMALCY_PLANE_ENUM ePlane
The plane on which normalcy is activated. Defined by the enumerator
NC_NORMALCY_PLANE_ENUM which describes the optional planes for normalcy mode
of operation.
The enumerator defines the following optional planes:
typedef enum {
NC_XY_PLANE=3, X,Y plane
NC_XZ_PLANE=5, X,Z plane
NC_YZ_PLANE=6, Y,Z plane
} MMC_NORMALCY_PLANE_ENUM;
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2140
<a id="pdf-page-2140"></a>
24.8.21.1 Function Example
/*!
* \fn int SetNormalcyMode(...)
* \brief this function implements the C++ API for enable/disable
normalcy mode of operation.
* \return 0 if completed successfully, otherwise error.
*/
Int SetNormalcyMode(unsigned int conh)
{
int rc = 0;
CMMCGroupAxis ga;
try {
ga.InitAxisData("v01", conh);
ga.SetNormalcyMode(NC_NORMALCY_RIGHT, NC_YZ_PLANE);
}
catch (CMMCException& e) {
fprintf(stderr, "%s: %s, error=%d\n", __func__, e.what(),
e.error());
}
return rc;
}

### PDF page 2141
<a id="pdf-page-2141"></a>
24.8.22 SetNormalcyOff
This function disables the normalcy mode
int SetNormalcyOff(
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2142
<a id="pdf-page-2142"></a>
24.8.23 GetNormalcyMode
This function obtains parameters for normalcy mode of operation. It should be noted that this setting is
linked to the selected kinematic.
int GetNormalcyMode(
MMC_NORMALCY_TYPE_ENUM& eType,
MMC_NORMALCY_PLANE_ENUM& ePlane
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
MMC_NORMALCY_TYPE_ENUM& eType
Normalcy mode of operation. Defined by the enumerator NC_NORMALCY_TYPE_ENUM
which describes the tangent knife mode of operations.
The mode NC_NORMALCY_NONE(0) is similar to the disable normalcy mode.
The enumerator defines the following modes:
typedef enum {
NC_NORMALCY_NONE, Normalcy disabled, ordinary motion
NC_NORMALCY_LEFT, Normal left absolute motion
NC_NORMALCY_RIGHT, Normal right absolute
NC_NORMALCY_TANGENT, Tangent absolute
NC_NORMALCY_RELATIVE, Relative to tangent direction
} MMC_NORMALCY_TYPE_ENUM;
MMC_NORMALCY_PLANE_ENUM& ePlane
The plane on which normalcy is activated. Defined by the enumerator
NC_NORMALCY_PLANE_ENUM which describes the optional planes for normalcy mode
of operation.
The enumerator defines the following optional planes:
typedef enum {
NC_XY_PLANE=3, X,Y plane
NC_XZ_PLANE=5, X,Z plane
NC_YZ_PLANE=6, Y,Z plane
} MMC_NORMALCY_PLANE_ENUM;
throw (CMMCException)
Refer to the section 24.1.1MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2143
<a id="pdf-page-2143"></a>
24.8.24 RemoveAxisFromGroup
Refer to the section MMC_RemoveAxisFromGroup for details of the description, scope, and motion mode.
void RemoveAxisFromGroup(
NC_IDENT_IN_GROUP_ENUM eIdentInGroup
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
eIdentInGroup
The NC_IDENT_IN_GROUP_ENUM enumerator identifies the order and Nodes in the
group of the added axis. Performed via an enumerator to give the different axes a name
in the order, which can be coupled to the names in the kinematic model. The opt ions
are:
NC_NODE_1_ID = 0
...............
NC_NODE_16_ID = 15
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.4.1, 0.

### PDF page 2144
<a id="pdf-page-2144"></a>
24.8.25 MoveCircularAbsolute
Refer to the section MMC_MoveCircularAbsolute for details of the description, scope, and motion mode.
int MoveCircularAbsolute(
NC_ARC_SHORT_LONG_ENUM eArcShortLong,
NC_PATH_CHOICE_ENUM ePathChoice,
NC_CIRC_MODE_ENUM eCircleMode,
double dAuxPoint[] | double dEndPoint[]
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
eArcShortLong
Defines the types of supported arc length. The NC_ARC_SHORT_LONG_ENUM
enumerator options are:
MC_NONE_ARC_CHOICE = 0
MC_SHORT = 1
MC_LONG = 2
ePathChoice
Defines the NC_PATH_CHOICE_ENUM enumerator types of supported path choice. The
option are:
MC_NONE_PATH_CHOICE = 0
MC_CLOCKWISE = 1
MC_COUNTERCLOCKWISE = 2
eCircleMode
Defines the types of supported circular modes in 2D. Refer to the section Coordinate
System and kinematic transformation, and the definitions below.
The NC_CIRC_MODE_ENUM enumerator options are:
MC_NONE_CIRC_MODE = 0
MC_BORDER_CIRC_MODE = 1
MC_CENTER_CIRC_MODE = 2
MC_RADIUS_CIRC_MODE = 3
MC_ANGLE_CIRC_MODE = 4
dAuxPoint[]
Absolute position for a dimension in the coordinate system specified by the input signal
CoordSystem.
dAuxPoint can have double values in a technical unit [u].

### PDF page 2145
<a id="pdf-page-2145"></a>
dEndPoint[]
Absolute end point position for a dimension in the coordinate system specified by the
input signal CoordSystem. dEndPoint is a 2D or 3D double in technical unit [u].
eBufferMode
Refer to the structure MMC_MOVECIRCULARABSOLUTE for further details.
throw (CMMCException)
Refer to the section 24.1.1MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.8.1.

### PDF page 2146
<a id="pdf-page-2146"></a>
24.8.26 MoveCircularAbsoluteCenter
Refer to the section MMC_MoveCircularAbsoluteCenter for details of the description, scope, and motion
mode.
int MoveCircularAbsoluteCenter(
NC_ARC_SHORT_LONG_ENUM eArcShortLong,
double dBorderPoint[] | double dCenterPoint[] | double dEndPoint[]
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
eArcShortLong
Defines the types of supported arc length. The NC_ARC_SHORT_LONG_ENUM
enumerator options are:
MC_NONE_ARC_CHOICE = 0
MC_SHORT = 1
MC_LONG = 2
dBorderPoint
Absolute border position for a dimension in the coordinate system specified by the
input signal CoordSystem. dBorderPoint can have double values in a technical unit [u].
dCenterPoint
Absolute position for a dimension in the coordinate system specified by the input signal
CoordSystem. dCenterPoint can have double values in a technical unit [u].
dEndPoint
Absolute end point position for a dimension in the coordinate system specified by the
input signal CoordSystem.dEndPoint is a 2D or 3D double vector in technical unit [u].
eBufferMode
Refer to the structure MMC_MOVECIRCULARABSOLUTECENTER_IN Structure for
further details.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.8.1.

### PDF page 2147
<a id="pdf-page-2147"></a>
24.8.27 MoveCircularAbsoluteBorder
Refer to the section 7.9.6MMC_MoveCircularAbsoluteBorder for details of the description, scope, and
motion mode.
int MoveCircularAbsoluteBorder(
double dBorderPoint[] | double dEndPoint[],
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
dBorderPoint
Absolute border position for a dimension in the coordinate system specified by the
input signal CoordSystem. dBorderPoint can have double values in a technical unit [u].
dEndPoint
Absolute end point position for a dimension in the coordinate system spec ified by the
input signal CoordSystem.dEndPoint is a 2D or 3D double vector in technical unit [u].
eBufferMode
Refer to the structure MMC_MOVECIRCULARABSOLUTEBORDER_IN Structure for
further details.
throw (CMMCException)
Refer to the section 24.1.1MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.8.1.

### PDF page 2148
<a id="pdf-page-2148"></a>
24.8.28 MoveCircularAbsoluteRadius
Refer to the section MMC_MoveCircularAbsoluteRadius for details of the description, scope, and motion
mode.
int MoveCircularAbsoluteRadius(
NC_ARC_SHORT_LONG_ENUM eArcShortLong,
NC_PATH_CHOICE_ENUM ePathChoice,
double dSpearHeadPoint[], | double dEndPoint[],
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
eArcShortLong
Defines the types of supported arc length. The NC_ARC_SHORT_LONG_ENUM
enumerator options are:
MC_NONE_ARC_CHOICE = 0
MC_SHORT = 1
MC_LONG = 2
ePathChoice
Defines the NC_PATH_CHOICE_ENUM enumerator types of supported path choice. The
option are:
MC_NONE_PATH_CHOICE = 0
MC_CLOCKWISE = 1
MC_COUNTERCLOCKWISE = 2
dSpearHeadPoint
Absolute radius position for a dimension in the coordinate system specified by the input
signal CoordSystem. dSpearHeadPoint can have double values in a technical unit [u].
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
dEndPoint[]
Absolute end point position for a dimension in the coordinate system specified by the
input signal CoordSystem. dEndPoint is a 2D or 3D double in technical unit [u].
eBufferMode
Refer to the structure MOVECIRCULARABSOLUTERADIUS_IN Structure for further
details.
throw (CMMCException)
Refer to the section 24.1.1MMCException. Produces details of the error including:

### PDF page 2149
<a id="pdf-page-2149"></a>
Function Name Structure name The axis reference Error ID
Status of the axis.

### PDF page 2150
<a id="pdf-page-2150"></a>
24.8.29 MoveCircularAbsoluteAngle
Refer to the section MMC_MoveCircularAbsoluteRadius for details of the description, scope, and motion
mode.
int MoveCircularAbsoluteAngle(
double dAngle, | double dCenterPoint[],
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
dAngle
Relative angular position for the coordinate system specified by the input signal
CoordSystem. Angular double value in degrees [u], which may be positive or negative
without restriction.
dCenterPoint
Absolute position for a dimension in the coordinate system specified by the input signal
CoordSystem. dCenterPoint can have double values in a technical unit [u].
eBufferMode
Refer to the structure MOVECIRCULARABSOLUTERADIUS_IN Structure for further
details.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.8.1.

### PDF page 2151
<a id="pdf-page-2151"></a>
24.8.30 MoveLinearAbsolute
Refer to the section MMC_MoveLinearAbsolute for details of the description, scope, and motion mode.
Overloaded Function
int MoveLinearAbsolute(MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);

int MoveLinearAbsolute(float fVelocity, MC_BUFFERED_MODE_ENUM eBufferMode =
MC_ABORTING_MODE
) throw (CMMCException);

int MoveLinearAbsolute(float fVelocity, double
dbPosition[NC_MAX_NUM_AXES_IN_NODE], MC_BUFFERED_MODE_ENUM eBufferMode =
MC_ABORTING_MODE
) throw (CMMCException);

int MoveLinearAbsolute(float fVelocity, double
dbPosition[NC_MAX_NUM_AXES_IN_NODE], float fAcceleration, float fDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);

int MoveLinearAbsolute(float fVelocity, double
dbPosition[NC_MAX_NUM_AXES_IN_NODE], float fAcceleration, float fDeceleration,
float fJerk, MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);

Source GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def MMC_MoveLinearAbsoluteCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_MoveLinearAbsoluteCmd(hConn,
hAxisRef, pInParam, pOutParam)
Parameters
fVelocity
Value of the maximum velocity (not necessarily reached). Any negative or positive
double values in technical unit [u].
dbPosition
Target position for the motion of the axis when conditions are met. Any negative or
positive double values in technical unit [u].
An array of coordinates, incl. positions and orientations (Distance if Mode = RELATIVE).
The array parameter NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as
the maximum number of axis in a group.
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
eBufferMode
Refer to the structure MMC_MOVELINEARABSOLUTE_IN Structure for further details.
throw (CMMCException)

### PDF page 2152
<a id="pdf-page-2152"></a>
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.8.1.

### PDF page 2153
<a id="pdf-page-2153"></a>
24.8.31 MoveLinearRelative
Refer to the section 7.9.11MMC_MoveLinearRelative for details of the description, scope, and motion
mode.
int MoveLinearRelative(
float fVelocity, | double dbDistance[NC_MAX_NUM_AXES_IN_NODE],
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
fVelocity
Value of the maximum velocity (not necessarily reached). Any ne gative or positive
double values in technical unit [u].
dbDistance
An array [1..N] of relative distances for each dimension in the specified coordinate
system, with N being vendor specific. The array parameter
NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as the maximum number
of axis in a group.
dbDistance is a double vector array in technical unit [u].
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
eBufferMode
Refer to the structure MMC_MOVELINEARRELATIVE_IN Structure for further details.
throw (CMMCException)
Refer to the section 24.1.1MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.8.1.

### PDF page 2154
<a id="pdf-page-2154"></a>
24.8.32 GroupSetOverride
Refer to the section MMC_GroupSetOverride for details of the description, scope, and motion mode.
int GroupSetOverride(
float fVelFactor,
float fAccFactor,
float fJerkFactor,
unsigned short usUpdateVelFactorIdx
);
Source GMAS\includes\CPP\MMCGroupAxis.h
C# Definition int GroupSetOverride(
float fVelFactor,
float fAccFactor,
float fJerkFactor,
ushort usUpdateVelFactorIdx);

Python Definition def MMC_GroupSetOverrideCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_GroupSetOverrideCmd(hConn,
hAxisRef, pInParam, pOutParam)

def GroupSetOverride(self, dVelFactor, dAccFactor,
dJerkFactor, usUpdateVelFactorIdx):
return _mmcpp_lib.CMMCGroupAxis_GroupSetOverride(self,
dVelFactor, dAccFactor, dJerkFactor, usUpdateVelFactorIdx)

Parameters
fVelFactor
New override factor for the velocity. Any positive float value between [0 - 1].
fAccFactor
New override factor for the acceleration/deceleration. ACC/Jerk Factors are NOT
supported at this time. For future compatibility, enter "1" in the function call.
fJerkFactor
New override factor for the jerk. ACC/Jerk Factors are NOT supported at this time. For
future compatibility, enter "1" in the function call.
usUpdateVelFactorIdx
Index of changed velocity factor. Vendor defined. The default is 0. Has integer values of
0 - 2
This variable is not in use at this moment.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2155
<a id="pdf-page-2155"></a>
For code example, refer to the section 24.8.1.

### PDF page 2156
<a id="pdf-page-2156"></a>
24.8.33 SetParameter (multiaxes)
Sets an array Parameter. Refer to the section 6.2.35MMC_WriteParameter for details of the description,
scope, and motion mode.
void SetParameter(
double dbValue,
MMC_PARAMETER_LIST_ENUM eNumber,
int iIndex
) throw (CMMCException);
Source GMAS\includes\CPP\MMCNode.h
GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def SetParameter(self, dbValue, eNumber, iIndex):
return _mmcpp_lib.CMMCGroupAxis_SetParameter(self,
dbValue, eNumber, iIndex)

Parameters
dbValue
An array parameter with double value.
eNumber
Number of the parameter. One can also use symbolic parameter names, which are
declared as VAR CONST. Refer to the section Axis Parameters (Explanations) .
The axis parameters define the MMC_PARAMETER_LIST_ENUM eParameterNumber
values of the axis status. Refer to the section Parameters Tables for the integer
parameter definitions for the appropriate integer parameter to be used as enumerator.
iIndex
An array index parameter (only relevant for array situations). Any positive integer
values
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.

### PDF page 2157
<a id="pdf-page-2157"></a>
24.8.34 SetBoolParameter (multiaxes)
Sets the Boolean Parameter for Group Axis. Refer to the section MMC_WriteBoolParameter for details of
the description, scope, and motion mode.
void SetBoolParameter(
int32_t lValue,
MMC_PARAMETER_LIST_ENUM eNumber,
int iIndex
);
Source GMAS\includes\CPP\MMCNode.h
GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def SetBoolParameter(self, lValue, eNumber, iIndex):
return _mmcpp_lib.CMMCGroupAxis_SetBoolParameter(self,
lValue, eNumber, iIndex)

Parameters
ulValue
Any integer value of the group axes parameter. positive numeric value.
eNumber
Number of the parameter. One can also use symbolic parameter names, which are
declared as VAR CONST. Refer to the section Axis Parameters (Explanations) .
Refer to the section Axis, Group, Global, Parameters for the appropriate integer
parameter to be used as enumerator.
iIndex
Index array (only relevant for array situations). Any positive integer values
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.

### PDF page 2158
<a id="pdf-page-2158"></a>
24.8.35 GroupWriteParameter
Sets an array Parameter for group axes. Refer to the section MMC_WriteParameter for details of the
description, scope, and motion mode.
virtual void SetParameter(
double dbValue, MMC_PARAMETER_LIST_ENUM eNumber, int iIndex
) throw (CMMCException);
Source GMAS\includes\CPP\MMCNode.h
GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def MMC_GroupWriteParameter(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_GroupWriteParameter(hConn,
hAxisRef, pInParam, pOutParam)
Parameters
dbValue
An array parameter with double value.
eNumber
Number of the parameter. One can also use symbolic parameter names, which are
declared as VAR CONST. Refer to the section Axis Parameters (Explanations) . The axis
parameters define the MMC_PARAMETER_LIST_ENUM eParameterNumber values of
the axis status. Refer to the section Parameters Tables for the integer parameter
definitions for the appropriate integer parameter to be used as enumerator.
iIndex
An array index parameter (only relevant for array situations). Any positive integer
values
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.

### PDF page 2159
<a id="pdf-page-2159"></a>
24.8.36 GetBoolParameter (multiaxes)
Obtains a Boolean Parameter for group axes. Refer to the section MMC_ReadBoolParameter for details of
the description, scope, and motion mode.
int32_t GetBoolParameter(
MMC_PARAMETER_LIST_ENUM eNumber,
int iIndex
);
Source GMAS\includes\CPP\MMCNode.h
GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def GetBoolParameter(self, eNumber, iIndex):
return _mmcpp_lib.CMMCGroupAxis_GetBoolParameter(self,
eNumber, iIndex)

Parameters
eNumber
Number of the parameter. One can also use symbolic parameter names, which are
declared as VAR CONST. Refer to the section Axis Parameters (Explanations) .
The axis parameters define the MMC_PARAMETER_LIST_ENUM eParameterNumber
values of the axis status. Refer to the section Parameters Tables for the integer
parameter definitions for the appropriate integer parameter to be used as enumerator.
iIndex
An array index parameter (only relevant for array situations). Any positive integer
values
Return

lValue Boolean parameters integer value
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.

### PDF page 2160
<a id="pdf-page-2160"></a>
24.8.37 GetParameter (multiaxes)
Obtains any group axes Parameter. Refer to the section MMC_ReadParameter for details of the description,
scope, and motion mode.
double GetParameter(
MMC_PARAMETER_LIST_ENUM eNumber,
int iIndex
);
Source GMAS\includes\CPP\MMCNode.h
GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def GetParameter(self, eNumber, iIndex):
return _mmcpp_lib.CMMCGroupAxis_GetParameter(self,
eNumber, iIndex)

Parameters
eNumber
Number of the parameter. One can also use symbolic parameter names, which are
declared as VAR CONST.
Refer to the section Axis, Group, Global, Parameters for the appropriate integer
parameter to be used as enumerator.
iIndex
An array index parameter (only relevant for array situations). Any positive integer
values
Return
dbValue
Output of the specific parameter. Any Double value.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.

### PDF page 2161
<a id="pdf-page-2161"></a>
24.8.38 GroupSetPosition
Refer to the section MMC_GroupSetPosition for details of the description, scope, and motion mode.
int GroupSetPosition(
double dbPosition[],
MC_COORD_SYSTEM_ENUM eCoordSystem,
unsigned char ucMode,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
dbPosition
Target position for the motion of the axis when conditions are met. Any negative or
positive double values in technical unit [u].
eCoordSystem
Define the types of supported coordinate systems. The MC_COORD_SYSTEM_ENUM
enumerator options are:
MC_NONE_COORD = 0
MC_ACS_COORD = 1
MC_MCS_COORD = 2
MC_PCS_COORD = 3
ucMode
RELATIVE =True, ABSOLUTE = False (Default)
RELATIVE means that Position is added to the actual position value of the axis at the
time of execution. This results in a recalibration by a specified distance. ABSOLUTE
means that the actual position value of the axis is set to the value specified in the
Position parameter.
Values accepted are Boolean, TRUE/FALSE.
eBufferMode
Refer to the structure MMC_GROUPSETPOSITION_IN Structure for further details.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.8.1.

### PDF page 2162
<a id="pdf-page-2162"></a>
24.8.39 GroupReadStatus
Refer to the section MMC_GroupReadStatus for details of the description, scope, and motion mode.
uint32_t GroupReadStatus(
);
uint32_t GroupReadStatus(
unsigned short& usGroupErrorID
);
uint32_t GroupReadStatus(
unsigned short _usGroupErrorID[1]
);
Source GMAS\includes\CPP\MMCGroupAxis.h
Python Definition
def MMC_GroupReadStatusCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_GroupReadStatusCmd(hConn,
hAxisRef, pInParam, pOutParam)

class MMC_GROUPREADSTATUS_IN(object):
uiHndlr =
property(_mmcpp_lib.MMC_GROUPREADSTATUS_IN_uiHndlr_get,
_mmcpp_lib.MMC_GROUPREADSTATUS_IN_uiHndlr_set)
ucEnable =
property(_mmcpp_lib.MMC_GROUPREADSTATUS_IN_ucEnable_get,
_mmcpp_lib.MMC_GROUPREADSTATUS_IN_ucEnable_set)

class MMC_GROUPREADSTATUS_OUT(object):
ulState =
property(_mmcpp_lib.MMC_GROUPREADSTATUS_OUT_ulState_get,
_mmcpp_lib.MMC_GROUPREADSTATUS_OUT_ulState_set)
usStatus =
property(_mmcpp_lib.MMC_GROUPREADSTATUS_OUT_usStatus_get,
_mmcpp_lib.MMC_GROUPREADSTATUS_OUT_usStatus_set)
usErrorID =
property(_mmcpp_lib.MMC_GROUPREADSTATUS_OUT_usErrorID_get,
_mmcpp_lib.MMC_GROUPREADSTATUS_OUT_usErrorID_set)
usGroupErrorID =
property(_mmcpp_lib.MMC_GROUPREADSTATUS_OUT_usGroupErrorID_
get, _mmcpp_lib.MMC_GROUPREADSTATUS_OUT_usGroupErrorID_set)

Parameters
usGroupErrorID
Returned command group error ID. Signals where an group error has occurred within
the function block. These values are vendor specific. Refer to the errors listed in
sections Maestro Error IDs, and NC Profiler Error IDs.
Displays an error code integer.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name

### PDF page 2163
<a id="pdf-page-2163"></a>
The axis reference Error ID
Status of the axis.

### PDF page 2164
<a id="pdf-page-2164"></a>
24.8.40 GetStatusRegister (multiaxes)
Provides the status register. Refer to the section MMC_GetStatusRegister for details of the description,
scope, and motion mode.
unsigned int GetStatusRegister(
);
unsigned int GetStatusRegister(
MMC_GETSTATUSREGISTER_OUT& sOutput
);
Source GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def GetStatusRegister(self):
return _mmcpp_lib.CMMCGroupAxis_GetStatusRegister(self)

Parameters
MMC_GETSTATUSREGISTER_OUT& sOutput
Refer to the MMC_GETSTATUSREGISTER_OUT Structure for details of the parameters.
typedef struct mmc_getstatusregister_out
{
unsigned int uiStatusRegister;
unsigned int uiMcsLimitRegister;
unsigned short usStatus;
short usErrorID;
unsigned char ucEndMotionReason;
unsigned char cBuffer[32];
} MMC_GETSTATUSREGISTER_OUT;
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2165
<a id="pdf-page-2165"></a>
24.8.41 GetMcsLimitRegister
This function returns the MCS limit register. This is the MCS Limit Register is a 32 bit representation of the
software limit status of all kinematic directions, 16 directions * 2 limits (High\Low) = 32.
Refer to the section 10.2.19MMC_GetStatusRegister for details of the description, scope, and motion mode.
unsigned int GetMcsLimitRegister(
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2166
<a id="pdf-page-2166"></a>
24.8.42 ReadStatus
THIS FUNCTION OBTAINS THE AXIS STATUS FOR A SPECIFIC AXIS. REFER TO THE SECTION
MMC_READSTATUS FOR DETAILS OF THE DESCRIPTION, AND SCOPE.
unsigned long ReadStatus(
unsigned short& usAxisErrorID, | unsigned short& usStatusWord,
) throw (CMMCException){return GroupReadStatus(usAxisErrorID);}
Source GMAS\includes\CPP\CMMCNode.h
GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
usAxisErrorID
Returns the axis error bitwise ID defined by the following enumerators. Bitwise ID error
code:
Bit ID Enumerator
0x1 MMC_ERR_TYPE_FAULT_BIT
0x2 MMC_ERR_TYPE_HEARTBEAT
0x4 MMC_ERR_TYPE_EMERGENCY
0x8 MMC_ERR_TYPE_COMM
0x10 MMC_ERR_TYPE_CFG_FILE
usStatusWord
Drive Status text. Any text characters.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section24.4.1.

### PDF page 2167
<a id="pdf-page-2167"></a>
24.8.43 Reset (Multiaxes)
Refer to the section MMC_Reset for details of the description, scope, and communication mode.
void GroupReset(
);
Source GMAS\includes\CPP\CMMCNode.h
GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def MMC_GroupResetCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_GroupResetCmd(hConn, hAxisRef,
pInParam, pOutParam)

def GroupReset(self):
return _mmcpp_lib.CMMCGroupAxis_GroupReset(self)

class MMC_GROUPRESET_IN(object):
ucExecute =
property(_mmcpp_lib.MMC_GROUPRESET_IN_ucExecute_get,
_mmcpp_lib.MMC_GROUPRESET_IN_ucExecute_set)

class MMC_GROUPRESET_OUT(object):
usStatus =
property(_mmcpp_lib.MMC_GROUPRESET_OUT_usStatus_get,
_mmcpp_lib.MMC_GROUPRESET_OUT_usStatus_set)
usErrorID =
property(_mmcpp_lib.MMC_GROUPRESET_OUT_usErrorID_get,
_mmcpp_lib.MMC_GROUPRESET_OUT_usErrorID_set)

Parameters
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2168
<a id="pdf-page-2168"></a>
24.8.44 GetMembersInfo
Returns information about a specific group and its members. Refer to the section
MMC_GetGroupMembersInfo for details of the description, scope, and motion mode.
void GetMembersInfo(
MMC_GETGROUPMEMBERSINFO_OUT* stOutput
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
MMC_GETGROUPMEMBERSINFO_OUT* stOutput
Refer to the MMC_GETGROUPMEMBERSINFO_OUT Structure for details of the
parameters.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section MMCGroupAxis Class Functions Code Example 1 .

### PDF page 2169
<a id="pdf-page-2169"></a>
24.8.45 GroupEnable
Refer to the section MMC_GroupEnable for details of the description, scope, and motion mode.
void GroupEnable(
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.8.1.

### PDF page 2170
<a id="pdf-page-2170"></a>
24.8.46 GroupDisable
Refer to the section MMC_GroupDisable for details of the description, scope, and motion mode.
void GroupDisable(
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section MMCGroupAxis Class Functions Code Example 1 .

### PDF page 2171
<a id="pdf-page-2171"></a>
24.8.47 GroupReset
Refer to the section MMC_GroupReset for details of the description, scope, and motion mode.
void GroupReset(
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2172
<a id="pdf-page-2172"></a>
24.8.48 GroupReadError
Refer to the section MMC_GroupReadError for details of the description, scope, and motion mode.
unsigned short GroupReadError(
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 2173
<a id="pdf-page-2173"></a>
24.8.49 GroupReadActualVelocity
Refer to the section MMC_GroupReadActualVelocity for details of the description, scope, and motion
mode.
double GroupReadActualVelocity(
MC_COORD_SYSTEM_ENUM eCoordSystem,
double dVelocity[]
) throw (CMMCException);
Source GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def MMC_GroupReadActualVelocity(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_GroupReadActualVelocity(hConn,
hAxisRef, pInParam, pOutParam)

def GroupReadActualVelocity(self, eCoordSystem):
return
_mmcpp_lib.CMMCGroupAxis_GroupReadActualVelocity(self,
eCoordSystem)

Parameters
eCoordSystem
Define the types of supported coordinate systems. The MC_COORD_SYSTEM_ENUM
enumerator options are:
MC_NONE_COORD = 0
MC_ACS_COORD = 1
MC_MCS_COORD = 2
MC_PCS_COORD = 3
ePathChoice
Defines the NC_PATH_CHOICE_ENUM enumerator types of supported path choice. The
option are:
MC_NONE_PATH_CHOICE = 0
MC_CLOCKWISE = 1
MC_COUNTERCLOCKWISE = 2
dVelocity[]
Current velocity of the group:
- For ACS the velocities of the different axes
- For MCS it provides the velocity of the TCP
dVelocity any negative or positive array double value in the axis's unit [u/s].
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:

### PDF page 2174
<a id="pdf-page-2174"></a>
Function Name Structure name
The axis reference Error ID
Status of the axis.
