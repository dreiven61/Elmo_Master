# Continuation - 24.6 The MMCMotionAxis class

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 2033-2054
- Chunk: `078_p2033-p2054_Continuation-24.6-The-MMCMotionAxis-class.md`

## Active Outline At Chunk Start
- p. 1705 - Chapter 24 Programming in C++
  - p. 1993 - 24.6 The MMCMotionAxis class

## Contained Bookmark Outline
  - p. 2050 - 24.7 The DLLMMCPP_API MMC_MOTIONPARAMS_GROUP class

## Extracted Text

### PDF page 2033
<a id="pdf-page-2033"></a>
24.6.26 CamTableSelect
This function selects a table by input handler. Refer to the section MMC_CamTableSelect for details of the
description, and scope.
MC_PATH_REF CamTableSelect(
const MC_CamRef& CamTableDescr,
unsigned int uiStartMode=0,
unsigned char ucIsMasterPosAbsolute,
unsigned char ucIsSlavePosAbsolute
) throw (CMMCException);
Source GMAS\includes\CPP\MMCMotionAxis.h
.NET Definition
Parameters
MC_CamRef& CamTableDescr
MC_CamRef Input/output parameter of MC_CamTableSelect is an Elmo specific data
type. Refer to MC_CAMREF for deails of this data type.
uiStartMode
Overrides uiStartMode of MC_CamTableSelect. Reserved for future use of Ramp-In and
other options.
ucIsMasterPosAbsolute
Boolean parameter value. If 1 GMAS refers to master column as absolute values,
otherwise 0 as relative values.
ucIsSlavePosAbsolute
Boolean parameter value. If 1 GMAS refers to slave column as absolute values,
otherwise 0 as relative values.
throw (CMMCException)
Refer to the MMCException. Produces details of the error including; Function Name,
Structure name, The axis reference, Error ID, Status of the axis

### PDF page 2034
<a id="pdf-page-2034"></a>
24.6.27 CamIn
MC_CamIn executes the CAM process. Refer to the examples in section Application Example for details of
the description, and scope.
int CamIn(
unsigned short usMaster,
MC_BUFFERED_MODE_ENUM eBufferMode,
unsigned int uiCamTableID,
CURVE_TYPE_ENUM eCurveType,
unsigned int ucAutoOffset = 0,
ECAM_PERIODIC_ENUM ePeriodic=eCAM_NON_PERIODIC,
double dbMasterSyncPosition=0.0,
double dbMasterStartDistance = 0,
unsigned int uiStartMode = 0,
double dbMasterOffset = 0,
double dbSlaveOffset = 0,
double dbMasterScaling = 1,
double dbSlaveScaling = 1,
ECAM_VALUE_SRC_ENUM eMasterValueSource = eECAM_SET_VALUE
) throw (CMMCException);
Source GMAS\includes\CPP\MMCMotionAxis.h
Python Definition def MMC_CamInCmd(hConn, hAxisRef, pInParam, pOutParam):
return _mmcpp_lib.MMC_CamInCmd(hConn, hAxisRef,
pInParam, pOutParam)
class MMC_CAMIN_IN(object):
dbMasterOffset =
property(_mmcpp_lib.MMC_CAMIN_IN_dbMasterOffset_get,
_mmcpp_lib.MMC_CAMIN_IN_dbMasterOffset_set)
dbSlaveOffset =
property(_mmcpp_lib.MMC_CAMIN_IN_dbSlaveOffset_get,
_mmcpp_lib.MMC_CAMIN_IN_dbSlaveOffset_set)
dbMasterScaling =
property(_mmcpp_lib.MMC_CAMIN_IN_dbMasterScaling_get,
_mmcpp_lib.MMC_CAMIN_IN_dbMasterScaling_set)
dbSlaveScaling =
property(_mmcpp_lib.MMC_CAMIN_IN_dbSlaveScaling_get,
_mmcpp_lib.MMC_CAMIN_IN_dbSlaveScaling_set)
dbMasterStartDistance =
property(_mmcpp_lib.MMC_CAMIN_IN_dbMasterStartDistance_get,
_mmcpp_lib.MMC_CAMIN_IN_dbMasterStartDistance_set)
dbMasterSyncPosition =
property(_mmcpp_lib.MMC_CAMIN_IN_dbMasterSyncPosition_get,
_mmcpp_lib.MMC_CAMIN_IN_dbMasterSyncPosition_set)
eMasterValueSource =
property(_mmcpp_lib.MMC_CAMIN_IN_eMasterValueSource_get,
_mmcpp_lib.MMC_CAMIN_IN_eMasterValueSource_set)
eBufferMode =
property(_mmcpp_lib.MMC_CAMIN_IN_eBufferMode_get,
_mmcpp_lib.MMC_CAMIN_IN_eBufferMode_set)
uiStartMode =
property(_mmcpp_lib.MMC_CAMIN_IN_uiStartMode_get,
_mmcpp_lib.MMC_CAMIN_IN_uiStartMode_set)
eCurveType =
property(_mmcpp_lib.MMC_CAMIN_IN_eCurveType_get,
_mmcpp_lib.MMC_CAMIN_IN_eCurveType_set)
ePeriodicMode =
property(_mmcpp_lib.MMC_CAMIN_IN_ePeriodicMode_get,

### PDF page 2035
<a id="pdf-page-2035"></a>
_mmcpp_lib.MMC_CAMIN_IN_ePeriodicMode_set)
uiCamTableID =
property(_mmcpp_lib.MMC_CAMIN_IN_uiCamTableID_get,
_mmcpp_lib.MMC_CAMIN_IN_uiCamTableID_set)
usMaster =
property(_mmcpp_lib.MMC_CAMIN_IN_usMaster_get,
_mmcpp_lib.MMC_CAMIN_IN_usMaster_set)
ucAutoOffset =
property(_mmcpp_lib.MMC_CAMIN_IN_ucAutoOffset_get,
_mmcpp_lib.MMC_CAMIN_IN_ucAutoOffset_set)
ucExecute =
property(_mmcpp_lib.MMC_CAMIN_IN_ucExecute_get,
_mmcpp_lib.MMC_CAMIN_IN_ucExecute_set)
ucSpare =
property(_mmcpp_lib.MMC_CAMIN_IN_ucSpare_get,
_mmcpp_lib.MMC_CAMIN_IN_ucSpare_set)
Parameters
usMaster
Master axis which should be tracked. This is the positive short axis reference of the
master that is to be followed.
MC_BUFFERED_MODE_ENUM eBufferMode
The MC_BUFFERED_MODE_ENUM enumerator defines the behavior of the axis. Modes
are as follows, but only the Buffered Mode is supported:
MC_ABORTING_MODE = 1
MC_BUFFERED_MODE = 2
MC_BLENDING_LOW_MODE = 3
MC_BLENDING_PREVIOUS_MODE = 4
MC_BLENDING_NEXT_MODE = 5
MC_BLENDING_HIGH_MODE = 6
Aborting Default mode without buffering. The next function block aborts an
ongoing motion and the command affects the axis immediately.
The buffer is cleared. This motion will be executed regardless o f
the Boolean ucExecute status which may be False(0) or True(1).
Buffered The next function block affects the axis as soon as the previous
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

### PDF page 2036
<a id="pdf-page-2036"></a>
BlendingHigh Blending with highest velocity of function block 1 and function
block 2 at end-position of function block1.
ucAutoOffset = 0
Auto offset. Adjust slave position in table to axis position when master reaches Sync
Position.
ECAM_PERIODIC_ENUM ePeriodic=eCAM_NON_PERIODIC
Describes the perodicity mode of the function, according to the following options:
eCAM_NON_PERIODIC = 0 One shot
eCAM_PERIODIC = 1 periodic
eCAM_PERIODIC_LINEAR = 2 periodic-linear
dbMasterSyncPosition=0.0
Defined as relative to first phase of master in CAM table. If table is relative then it is defined as
relative master position just like any other phase in table.
dbMasterStartDistance = 0
Backward distance from dbMasterSyncPosition to allow Ramp-In. Untill we have Ram-In
implemented it is always zero.
uiStartMode = 0
Overrides uiStartMode of MC_CamTableSelect
dbMasterOffset = 0
Master offset from the master definition in the CAM table.
dbSlaveOffset = 0
Slave offset from slave definition in CAM table.
dbMasterScaling = 1
Master scaling of the master definition in the CAM table.
dbSlaveScaling = 1
Slave scaling of the master definition in the CAM table
ECAM_VALUE_SRC_ENUM eMasterValueSource = eECAM_SET_VALUE
The Master source value is defined by the eMasterValueSource input parameter. It may
be a Maestro parameter for target position, actual position (integer) or some kind of
auxiliary. If the master axis operates in modulo mode, then the target position uses the
Maestro parameter as a source for the target modulated position (UU).
The Master value source dependant on whether set, actual or based on another value,
according the ECAM_VALUE_SRC_ENUM enumerator.
eECAM_SET_VALUE = 0

### PDF page 2037
<a id="pdf-page-2037"></a>
eECAM_ACTUAL_VALUE = 1
eECAM_AUX_VALUE = 2
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including; Function
Name, Structure name, The axis reference, Error ID, Status of the axis
24.6.28 CamOut
MC_CamOut performs an MC_Stop on the slave axis. Refer to the section MMC_CamOut for details of the
description, and scope.
int CamOut(
) throw (CMMCException);
Source GMAS\includes\CPP\MMCMotionAxis.h
Python Definition def MMC_CamOutCmd(hConn, hAxisRef, pInParam, pOutParam):
return _mmcpp_lib.MMC_CamOutCmd(hConn, hAxisRef,
pInParam, pOutParam)
class MMC_CAMOUT_IN(object):
ucExecute =
property(_mmcpp_lib.MMC_CAMOUT_IN_ucExecute_get,
_mmcpp_lib.MMC_CAMOUT_IN_ucExecute_set)
Parameters
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including; Function
Name, Structure name, The axis reference, Error ID, Status of the axis

### PDF page 2038
<a id="pdf-page-2038"></a>
24.6.29 CamSetProperty
his function sets specific properties of the CAM function. It was created for a specific situation whereby the
ECAM periodic motion is to be stopped using a non-periodic motion. Refer to the section
MMC_CamSetProperty for details of the description, and scope.
int CamSetProperty(
ECAM_PROPERTIES_ENUM eProperty,
ECAM_PERIODIC_ENUM ePeriodicMode
) throw (CMMCException);
Source GMAS\includes\CPP\MMCMotionAxis.h
Python Definition def MMC_CamSetPropertyCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_CamSetPropertyCmd(hConn,
hAxisRef, pInParam, pOutParam)
def CamSetProperty(self, eProperty, ePeriodicMode):
return
_mmcpp_lib.CMMCMotionAxis_CamSetProperty(self, eProperty,
ePeriodicMode)
Parameters

ECAM_PERIODIC_ENUM ePeriodicMode
Describes the periodicity mode of the function, according to the following options:
eCAM_NON_PERIODIC = 0 One shot
eCAM_PERIODIC = 1 periodic
eCAM_PERIODIC_LINEAR = 2 periodic-linear
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including Function
Name, Structure name, The axis reference, Error ID, Status of the axis
24.6.30 CamTableUnload
This method unloads the ECAM table from the Maestro. Refer to the section MMC_Unlo adTable for details
of the description, and scope.
void CamTableUnload (
void
);
Source GMAS\includes\CPP\MMCMotionAxis.h
.NET Definition
Parameters
void

### PDF page 2039
<a id="pdf-page-2039"></a>
Function takes no parameters

### PDF page 2040
<a id="pdf-page-2040"></a>
24.6.31 CamTableAdd
This method appends points to the current ECAM table. Refer to the section 9.6.4MMC_CamTableAdd for
details of the description, and scope.
int CamTableAdd(
MC_PATH_REF hMemHandle,
double *dbTable,
unsigned short usColumns,
unsigned long ulNumberOfPoints
) throw (CMMCException);
Source GMAS\includes\CPP\MMCMotionAxis.h
Python Definition def MMC_CamTableAddCmd(hConn, pInParam, pOutParam):
return _mmcpp_lib.MMC_CamTableAddCmd(hConn, pInParam,
pOutParam)
def CamTableAdd(self, hMemHandle, dbTable, usColumns,
ulNumberOfPoints):
return _mmcpp_lib.CMMCMotionAxis_CamTableAdd(self,
hMemHandle, dbTable, usColumns, ulNumberOfPoints)

Remarks
The prerequisite to using this function is a call to MC_CamTableInit.
Scope
Loads CAM tables from an array in a user program into the Maestro. The user should be aware of the
number of columns used for each row (a point).
- Use an array of type double.
- The array must contain a sequence of rows (points), one by one.
- The columns order must be as follows: master, slave, curve type.
- Each row must contain the slave position. Master position and curve type are optional.
- If the Master gap is fixed then no row contains a master column, otherwise it does.
- If the curve type parameter is defined by user, a special column for curve type must be supplied. I f not
defined, it should not be supplied.
Parameters
MC_PATH_REF hMemHandle
MC_PATH_REF enumerator handle to a journal entry where the pointer to the shared
memory is located. MC_PATH_REF is the journal entry path reference.
hMemHandle can have integer values.
*dbTable
Pointer to the Table of values. Table of limited length in values, limited to 170 values
max.

### PDF page 2041
<a id="pdf-page-2041"></a>
usColumns
Number of columns in the array. positive number
ulStartIndex
Index to manually append from. Any positive values accepted. Used only in manual
mode.
ulNumberOfPoints
Number of points in the rows to append. Any positive values accepted.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including; Function
Name, Structure name, The axis reference, Error ID, Status of the axis

### PDF page 2042
<a id="pdf-page-2042"></a>
24.6.32 CamTableAddEx
The CamTableAddEx function is used to add an unlimited number of rows to an existing table.
int CamTableAddEx(
MC_PATH_REF hMemHandle,
double *dbTable,
unsigned short usColumns,
unsigned long ulNumberOfPoints)
throw (CMMCException);
Source GMAS\includes\CPP\MMCMotionAxis.h
Python Definition def CamTableAddEx(self, hMemHandle, dbTable, usColumns,
ulNumberOfPoints):
return _mmcpp_lib.CMMCMotionAxis_CamTableAddEx(self,
hMemHandle, dbTable, usColumns, ulNumberOfPoints)

Remarks
The prerequisite to using this function is a call to MC_CamTableInit. This API allows users to add unlimited
number of rows.
Scope
Loads CAM tables from an array in a user program into the Maestro. The user should be aware of the
number of columns used for each row (a point).
- Use an array of type double.
- The array must contain a sequence of rows (points), one by one.
- The columns order must be as follows: master, slave, curve type.
- Each row must contain the slave position. Master position and curve type are optional.
- If the Master gap is fixed then no row contains a master column, otherwise it does.
- If the curve type parameter is defined by user, a special column for curve type must be supplied. If not
defined, it should not be supplied.
Parameters
MC_PATH_REF hMemHandle
MC_PATH_REF enumerator handle to a journal entry where the pointer to the shared
memory is located. MC_PATH_REF is the journal entry path reference.
hMemHandle can have integer values.
*dbTable
Pointer to the Table of values. Table of unlimited limited length in values.
usColumns
Number of columns depends on ucIsFixedGap and eCurveType as input parameters of
CamTableInit.
ulNumberOfPoints

### PDF page 2043
<a id="pdf-page-2043"></a>
Number of points in the rows to append. Any positive values accepted.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including; Function
Name, Structure name, The axis reference, Error ID, Status of the axis

### PDF page 2044
<a id="pdf-page-2044"></a>
24.6.33 CamTableSet
This method sets the number of points and number of columns in the array of the ECAM table. Refer to the
section MC_CamTableSet for details of the description, and scope.
int CamTableSet(
MC_PATH_REF hMemHandle,
double *dbTable,
unsigned short usColumns,
unsigned long ulStartIndex,
unsigned long ulNumberOfPoints
) throw (CMMCException);
Source GMAS\includes\CPP\MMCMotionAxis.h
.NET Definition
Parameters
MC_PATH_REF hMemHandle
MC_PATH_REF enumerator handle to a journal entry where the pointer to the shared
memory is located. MC_PATH_REF is the journal entry path reference.
hMemHandle can have integer values.
dTable
Pointer to the Table of values. Table of limited length in values, limited to 170 values
max.
usColumns
Number of columns in the array. positive number
ulStartIndex
Index to manually append from. Any positive values accepted. Used only in manual
mode.
ulNumberOfPoints
Number of points in the rows to append. Any positive values accepted.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including; Function
Name, Structure name, The axis reference, Error ID, Status of the axis

### PDF page 2045
<a id="pdf-page-2045"></a>
24.6.34 ReadGroupofParameters
Refer to the section MMC_ReadGroupOfParameters for details of the description, scope, and motion mode.
void ReadGroupOfParameters(
MMC_READGROUPOFPARAMETERSMEMBER sParameters
[GROUP_OF_PARAMETERS_MAXIMUM_SIZE],
unsigned char ucNumberOfParameters,
double* dbOutVal
);
Source GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def MMC_ReadGroupOfParameters(hConn, pInParam, pOutParam):
return _mmcpp_lib.MMC_ReadGroupOfParameters(hConn,
pInParam, pOutParam)
Parameters
MMC_READGROUPOFPARAMETERSMEMBER sParameters
[GROUP_OF_PARAMETERS_MAXIMUM_SIZE]
Type: array
The array of parameters of the group for reading.
An array with the maximum value of GROUP_OF_PARAMETERS_MAXIMUM_SIZE is 5
MMC_READGROUPOFPARAMETERSMEMBER structure
class MMC_READGROUPOFPARAMETERSMEMBER(object):
eParameterNumber =
property(_mmcpp_lib.MMC_READGROUPOFPARAMETERSMEMBER_eParameterNu
mber_get,
_mmcpp_lib.MMC_READGROUPOFPARAMETERSMEMBER_eParameterNumber_set)
iParameterIndex =
property(_mmcpp_lib.MMC_READGROUPOFPARAMETERSMEMBER_iParameterIn
dex_get,
_mmcpp_lib.MMC_READGROUPOFPARAMETERSMEMBER_iParameterIndex_set)
usAxisRef =
property(_mmcpp_lib.MMC_READGROUPOFPARAMETERSMEMBER_usAxisRef_ge
t, _mmcpp_lib.MMC_READGROUPOFPARAMETERSMEMBER_usAxisRef_set)
usPadding =
property(_mmcpp_lib.MMC_READGROUPOFPARAMETERSMEMBER_usPadding_ge
t, _mmcpp_lib.MMC_READGROUPOFPARAMETERSMEMBER_usPadding_set)
eParameterNumber
Type: enumerator
Parameter's number in list of parameters (MMC_PARAMETER_LIST_ENUM). One can also use
symbolic parameter names, which are declared as VAR CONST. Refer to the parameters table
list; Axis, Group, Global, Parameters for the appropriate integer parameter to be used as
enumerator.
iParameterIndex
Type: array integer

### PDF page 2046
<a id="pdf-page-2046"></a>
An array index parameter (only relevant for parameters defined as array)
usAxisRef
Type: unsigned short
The group axis Reference handle
usPadding
Type: unsigned short
Alignment padding of data. This parameter is not in use at this time
dbOutVal [GROUP_OF_PARAMETERS_MAXIMUM_SIZE]
Type: double array
The array of parameters of the group for reading.
An array with the maximum value of GROUP_OF_PARAMETERS_MAXIMUM_SIZE is 5
ucNumberOfParameters
Type: unsigned char
The total number of parameters in the group to be read. positive character value.

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.4.1, 0.

### PDF page 2047
<a id="pdf-page-2047"></a>
24.6.35 WriteGroupofParameters
Refer to the section MMC_WriteGroupOfParameters for details of the description, scope, and motion mode.
WriteGroupOfParametersImmediate(
MMC_WRITEGROUPOFPARAMETERSMEMBER
sParameters[GROUP_OF_PARAMETERS_MAXIMUM_SIZE],
unsigned char ucNumberOfParameters);

void WriteGroupOfParametersQueued(
MMC_WRITEGROUPOFPARAMETERSMEMBER
sParameters[GROUP_OF_PARAMETERS_MAXIMUM_SIZE],
unsigned char ucNumberOfParameters);
Source GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def WriteGroupOfParametersImmediate(self, sParameters,
ucNumberOfParameters):
return
_mmcpp_lib.CMMCMotionAxis_WriteGroupOfParametersImmediate(self,
sParameters, ucNumberOfParameters)

def WriteGroupOfParametersQueued(self, sParameters,
ucNumberOfParameters):
return
_mmcpp_lib.CMMCMotionAxis_WriteGroupOfParametersQueued(self,
sParameters, ucNumberOfParameters)
class MMC_WRITEGROUPOFPARAMETERSMEMBER(object):
dbValue =
property(_mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_dbValue_ge
t, _mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_dbValue_set)
eParameterNumber =
property(_mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_eParameter
Number_get,
_mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_eParameterNumber_se
t)
iParameterIndex =
property(_mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_iParameter
Index_get,
_mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_iParameterIndex_set
)
usAxisRef =
property(_mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_usAxisRef_
get, _mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_usAxisRef_set)
usPadding1 =
property(_mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_usPadding1
_get,
_mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_usPadding1_set)
usPadding2 =
property(_mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_usPadding2
_get,
_mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_usPadding2_set)
usPadding3 =
property(_mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_usPadding3
_get,
_mmcpp_lib.MMC_WRITEGROUPOFPARAMETERSMEMBER_usPadding3_set)

### PDF page 2048
<a id="pdf-page-2048"></a>
Parameters

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.4.1, 0.

### PDF page 2049
<a id="pdf-page-2049"></a>
24.6.36 WriteGroupofParametersEx
Refer to the section MMC_WriteGroupOfParametersEx for details of the description, scope, and motion mode.
void WriteGroupOfParametersImmediateEX(
MMC_WRITEGROUPOFPARAMETERSMEMBEREX
sParameters[GROUP_OF_PARAMETERS_MAXIMUM_SIZE],
unsigned char ucNumberOfParameters);
void WriteGroupOfParametersQueuedEX(
MMC_WRITEGROUPOFPARAMETERSMEMBEREX
sParameters[GROUP_OF_PARAMETERS_MAXIMUM_SIZE],
unsigned char ucNumberOfParameters);
Source GMAS\includes\CPP\MMCGroupAxis.h
Python Definition def MMC_WriteGroupOfParametersEX(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_WriteGroupOfParametersEX(hConn,
hAxisRef, pInParam, pOutParam)
Parameters

throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.
For code example, refer to the section 24.4.1, 0.

### PDF page 2050
<a id="pdf-page-2050"></a>
##### 24.7 The DLLMMCPP_API MMC_MOTIONPARAMS_GROUP class
24.7 The DLLMMCPP_API
MMC_MOTIONPARAMS_GROUP class
The class DLLMMCPP_API MMC_MOTIONPARAMS_GROUP wraps the multiple axes functions detailed in the
section 24.7.1.
The diagram in Figure 541 describes the heirarchial structure of the classes and type def initions associated with
the CMMCGroupAxis.

Figure 541 MMC_MOTIONPARAMS_GROUP class Fields diagram
The class DLLMMCPP_API MMC_MOTIONPARAMS_GROUP retains the same field parameter properties and
values described in this document for the C function blocks, and while small visual changes may be made to
some variables, these are transparent, and do not change the operation of the variable.
It should be noted that Private functions and their operation should be transparent to the user, and are not for
general application by the user.
The detailed class view shown in Figure 541, describes the fields and methods associated with the
DLLMMCPP_API MMC_MOTIONPARAMS_GROUP class. These are generally default parameters, which can be
operated using their default values. However if the user wishes to change the defaults, refer to the relevant
parameter section in the manual.

### PDF page 2051
<a id="pdf-page-2051"></a>
24.7.1 MMC_MOTIONPARAMS_GROUP()
Defines the group motion parameters for arrays of drives.
class DLLMMCPP_API MMC_MOTIONPARAMS_GROUP
{
public:
MMC_MOTIONPARAMS_GROUP();
//
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
NC_ARC_SHORT_LONG_ENUM eArcShortLong;
NC_PATH_CHOICE_ENUM ePathChoice;
NC_CIRC_MODE_ENUM eCircleMode;
unsigned int m_uiExecDelayMs;
unsigned char ucSuperimposed;
unsigned char ucExecute;
//
};
Source GMAS\includes\CPP\MMCGroupAxis.h
.NET Definition
Parameters
ucExecute
Start the execution command at the rising edge. Boolean TRUE/FALSE values.
dAuxPoint[NC_MAX_NUM_AXES_IN_NODE]
An array [1..N] of absolute positions for each dimension in the coordinate system
specified by the input signal CoordSystem, with N being vendor specific. The array
parameter NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as the
maximum number of axis in a group.
dAuxPoint can have vector array [1....3] double values in a technical unit [u].
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
dEndPoint[NC_MAX_NUM_AXES_IN_NODE]
An array [1..N] of absolute end point positions for each dimension in the coordinate
system specified by the input signal CoordSystem, with N being vendor specific. The
array parameter NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as the
maximum number of axis in a group.
dEndPoint is a 2D or 3D double vector array in technical unit [u].

### PDF page 2052
<a id="pdf-page-2052"></a>
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
fVelocity
Value of the maximum velocity (not necessarily reached) in which the path is defined.
Any positive float value in u/s
fAcceleration
Value of the acceleration (increasing energy of the motor). Any positive float value in
u/s2.
fDeceleration
Float value of the deceleration when stopping (decreasing energy of the motor). Any
positive float value in u/s2
fJerk
Maximum float value of the Jerk. Any positive value in u/s3
fTransitionParameter [NC_MAX_NUM_AXES_IN_NODE]
Depending on the transition mode, different supplier specific transition parameters can
be used which characterize the contour curve. The array parameter
NC_MAX_NUM_AXES_IN_NODE is limited to 16, and defined as the maximum number
of axis in a group.
fTransitionParameter can have any positive float value in appropriate units, dependant
on the TransitionMode parameter. Refer to the section Coordinate System and
kinematic transformation.
[NC_MAX_NUM_AXES_IN_NODE] is an array of values [2....15].
MC_COORD_SYSTEM_ENUM eCoordSystem
Define the types of supported coordinate systems. The MC_COORD_SYSTEM_ENUM
enumerator options are:
MC_NONE_COORD = 0
MC_ACS_COORD = 1
MC_MCS_COORD = 2
MC_PCS_COORD = 3
NC_TRANSITION_MODE_ENUM eTransitionMode
Define the supported NC_TRANSITION_MODE_ENUM enumerator transition modes.
Refer to the section Multiple Axes Motion Control - Transition and Buffer Modes and
options below. The options are:
MC_TM_NONE_MODE = 0,
MC_TM_MAX_VELOCITY_MODE = 1, Not supported at this time
MC_TM_DEFINED_VELOCITY_MODE = 2,
MC_TM_CORNER_DISTANCE_MODE = 3,
MC_TM_MAX_CORNER_DEVIATION_MODE = 4,
MC_TM_SWITCH_RADIUS_MODE = 5,

### PDF page 2053
<a id="pdf-page-2053"></a>
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
MC_BUFFERED_MODE_ENUM eBufferMode
The MC_BUFFERED_MODE_ENUM enumerator defines the behavior of the axis. Modes
are as follows:
MC_ABORTING_MODE = 1
MC_BUFFERED_MODE = 2
MC_BLENDING_LOW_MODE = 3
MC_BLENDING_PREVIOUS_MODE = 4
MC_BLENDING_NEXT_MODE = 5
MC_BLENDING_HIGH_MODE = 6
Aborting Default mode without buffering. The next function block aborts an
ongoing motion and the command affects the axis immediately.
The buffer is cleared. This motion will be executed regardless of
the Boolean ucExecute status which may be False(0) or True(1).
Buffered The next function block affects the axis as soon as the previous
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
NC_ARC_SHORT_LONG_ENUM eArcShortLong
Defines the types of supported arc length. The NC_ARC_SHORT_LONG_ENUM
enumerator options are:
MC_NONE_ARC_CHOICE = 0

### PDF page 2054
<a id="pdf-page-2054"></a>
MC_SHORT = 1
MC_LONG = 2
NC_PATH_CHOICE_ENUM ePathChoice
Defines the NC_PATH_CHOICE_ENUM enumerator types of supported path choice. The
option are:
MC_NONE_PATH_CHOICE = 0
MC_CLOCKWISE = 1
MC_COUNTERCLOCKWISE = 2
NC_CIRC_MODE_ENUM eCircleMode
Defines the types of supported circular modes in 2D. Refer to the section Coordinate
System and kinematic transformation. The NC_CIRC_MODE_ENUM enumerator
options are:
MC_NONE_CIRC_MODE = 0
MC_BORDER_CIRC_MODE = 1
MC_CENTER_CIRC_MODE = 2
MC_RADIUS_CIRC_MODE = 3
MC_ANGLE_CIRC_MODE = 4
m_uiExecDelayMs
The delay in execution of the next action (in msecs). Any positive integer value.
ucSuperimposed
Whether the option to superimpose is operated or not. Values accepted are Boolean
TRUE/FALSE.
