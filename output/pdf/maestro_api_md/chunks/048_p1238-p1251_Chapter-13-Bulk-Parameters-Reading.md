# Chapter 13 Bulk Parameters Reading

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 1238-1251
- Chunk: `048_p1238-p1251_Chapter-13-Bulk-Parameters-Reading.md`

## Active Outline At Chunk Start
- p. 1238 - Chapter 13 Bulk Parameters Reading
  - p. 1238 - 13.1 Bulk Reading Functions

## Contained Bookmark Outline
- p. 1238 - Chapter 13 Bulk Parameters Reading
  - p. 1238 - 13.1 Bulk Reading Functions
    - p. 1239 - 13.1.1 MMC_ConfigBulkRead
    - p. 1246 - 13.1.2 MMC_PerformBulkRead

## Extracted Text

### PDF page 1238
<a id="pdf-page-1238"></a>
#### Chapter 13 Bulk Parameters Reading
##### 13.1 Bulk Reading Functions
Chapter 13 Bulk Parameters Reading
This set of functions allows the user to retrieve all the parameters for a limited number of single ax es operating
simultaneously using a single function call. The Maestro allows a user program or host via Ethernet interface to
retrieve all required axis parameters. As the number of axes and parameters increases, the time to retrieve the
parameters increases proportionately. This operation is relatively slow, and this chapter explains the purpose
of bulk read, which increases the performance of the parameter's retrieval procedure dramatically.
This feature can be used in two difference scenarios:
- To import multiple parameters when using Maestro Multiple Axes in the EASII software
- To retrieve multiple parameters for multiple single axis when programming in C or C++ (or any other host
software)
13.1 Bulk Reading Functions
The following Bulk Reading functions are described:
Bulk Reading functions
MMC_ConfigBulkRead
MMC_PerformBulkRead

### PDF page 1239
<a id="pdf-page-1239"></a>
###### 13.1.1 MMC_ConfigBulkRead
13.1.1 MMC_ConfigBulkRead
Configures the function to read all parameters from multiple axes.
MMC_LIB_API int MMC_ConfigBulkReadCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_CONFIGBULKREAD_IN* pInParam,
OUT MMC_CONFIGBULKREAD_OUT* pOutParam
);
Motion Mode NC - Immaterial Distributed - Immaterial
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_CONFIGBULKREAD input data structure using the
MMC_ConfigBulkRead function.
pOutParam
Points to the MMC_CONFIGBULKREAD_OUT output structure receiving information, as
a result of calling the MMC_ConfigBulkRead function.
Remarks
The Maestro has three function block queues, large, medium, and small, function blocks. These are common
to all nodes. Every time a function block is inserted, a function block is taken from one of these queues. The
parameters iFreeLargeFbsNumber, iFreeMediumFbsNumber and iFreeSmallFbsNumber indicate the number
of free function blocks available in each queue.
Scope
All
MMC_CONFIGBULKREAD_IN Structure
typedef struct mmc_configbulkread_in{
NC_BULKREAD_PARAMETERS_UNION uBulkReadParams;
NC_BULKREAD_CONFIG_ENUM eConfiguration;
unsigned short usAxisRefAn array[NC_MAX_AXES_PER_BULK_READ];
unsigned short usNumberOfAxes;
unsigned char ucIsPreset;
}MMC_CONFIGBULKREAD_IN;

### PDF page 1240
<a id="pdf-page-1240"></a>
Parameters
uBulkReadParams
Defines what parameters will be read. It can be either one of the predefined presets, or a custom
array with user filled values. ulBulkReadParameters is an array of size 32 Bit, and to set the
number of signals to be read from the bulk read user, it is necessary to set the value of this
parameter to 0 after the signals requested. If two signals are required, then the
ulBulkReadParameters[2] value should be -1 otherwise a junk value that was in the array could be
considered as a signal requested
The Union parameter NC_BULKREAD_PARAMETERS_UNION is de fined as follows:
typedef union{
NC_BULKREAD_PRESET_ENUM eBulkReadPreset;
unsigned long ulBulkReadParameters[NC_MAX_REC_SIGNALS_NUM];
} NC_BULKREAD_PARAMETERS_UNION;
Where the user can select to use either the parameters:
NC_BULKREAD_PRESET_ENUM eBulkReadPreset
Or
unsigned long ulBulkReadParameters[NC_MAX_REC_SIGNALS_NUM]
eBulkReadPreset
The bulk read preset enumerator defined by
NC_BULKREAD_PRESET_ENUM with values:
eNC_BULKREAD_PRESET_NONE,
eNC_BULKREAD_PRESET_1,
eNC_BULKREAD_PRESET_2,
eNC_BULKREAD_PRESET_3,
eNC_BULKREAD_PRESET_4,
eNC_BULKREAD_PRESET_5,
eNC_BULKREAD_PRESET_MAX,
Each of the bulk read presets defines a fixed set of parameters to be
read:
typedef struct eNC_BULKREAD_PRESET_1{
int aPos;
int aVel;
int aTorque;
unsigned long ulAxisStatus;
unsigned int uiInputs;
OPM402 eOpMode;
}NC_BULKREAD_PRESET_1;

### PDF page 1241
<a id="pdf-page-1241"></a>
typedef struct eNC_BULKREAD_PRESET_2{
NC_BULKREAD_PRESET_1 stAxisParams;
int iFreeLargeFbsNumber;
int iFreeMediumFbsNumber
int iFreeSmallFbsNumber;
}NC_BULKREAD_PRESET_2;
typedef struct eNC_BULKREAD_PRESET_3{
int aPos;
int aVel;
int aTorque;
unsigned long ulAxisStatus;
unsigned int uiInputs;
OPM402 eOpMode;
NC_BULKREAD_VARIABLE_UNION ucCommError;
NC_BULKREAD_VARIABLE_UNION usLastEmcyErrorCode;
NC_BULKREAD_VARIABLE_UNION usControlWord;
NC_BULKREAD_VARIABLE_UNION usStatusWord;
}NC_BULKREAD_PRESET_3;
typedef struct eNC_BULKREAD_PRESET_4{
int aPos;
int aHWPos;
int iPosFollowingErr;
int aVel;
int aTorque;
unsigned long ulAxisStatus;
unsigned int uiInputs;
OPM402 eOpMode;
unsigned int uiStatusRegister;
unsigned int uiMcsLimitRegister;
}NC_BULKREAD_PRESET_4;
typedef struct eNC_BULKREAD_PRESET_5{
int aPos;
int aHWPos;
int iPosFollowingErr;
int aVel;
int aTorque;
unsigned long ulAxisStatus;
unsigned int uiInputs;
OPM402 eOpMode;
unsigned int uiStatusRegister;
unsigned int uiMcsLimitRegister;
NC_BULKREAD_VARIABLE_UNION usLastEmcyErrorCode;
NC_BULKREAD_VARIABLE_UNION usControlWord;
NC_BULKREAD_VARIABLE_UNION usStatusWord;
NC_BULKREAD_VARIABLE_UNION ucCommError;
}NC_BULKREAD_PRESET_5;

### PDF page 1242
<a id="pdf-page-1242"></a>
typedef union{
char cVar;
unsigned char ucVar;
short sVar;
unsigned short usVar;
int iVar;
unsigned int uiVar;
long lVar;
unsigned long ulVar;
}NC_BULKREAD_VARIABLE_UNION;
Where the user can select to use either of the
parameters:
Character cVar, Unsigned character ucVar, short sVar,
unsigned short usVar, integer iVar, unsigned integer
uiVar, long lVar, or un signed long ulVar.
Var is the value of the variable.
Each of these preset parameters have the following definitions:
aPos
Actual position integer. Integer values
aHWPos
Actual hardware position. Integer values
iPosFollowingErr
Position following error. Integer values
aVel
Actual velocity integer. Integer values
aTorque
Actual Torque integer. Integer values
ulAxisStatus
Status of the axis with positive bitwise
values
uiInputs
Digital inputs with any positive integer value
eOpMode
Motion mode
uiStatusRegister
This the Status Register, a 32 bit status
register, with 10 lower bits related to the

### PDF page 1243
<a id="pdf-page-1243"></a>
hardware/software limits feature. Refer to
the section below Status Register. All other
bits will be used in the future. positive
integer value.
uiMcsLimitRegister
Parameter represents the status of the MCS
limits in a specific group. positive integer
value.
iFreeLargeFbsNumber
Number of large free function blocks
available in the queue for this size.
iFreeMediumFbsNumb
er

Number of medium sized free function
blocks available in the queue for this size.
Integer value accepted
iFreeSmallFbsNumber
Number of small free function blocks
available in each queue. Integer value
accepted
usControlWord
CANopen DS402 control word. Any positive
short value.
usStatusWord
CANopen DS402 status word. Any positive
short value.
ucCommError
Input axis communication error. positive
character values.
usLastEmcyErrorCode
Last recorded emergency code.
usLastEmcyErrorCode is the emergency
error code received from the drive.
ulBulkReadParamet
ers

The array of ulBulkReadParameters defined by
[NC_MAX_REC_SIGNALS_NUM] represents a set of parameters to be
retrieved from the Maestro, with a maximum of 32 parameters.

### PDF page 1244
<a id="pdf-page-1244"></a>
NC_BULKREAD_CONFIG_ENUM eConfiguration
Defines the reading source. eBULKREAD_CONFIG_1 is reserved to the EAS application.
NC_BULKREAD_CONFIG_ENUM defines the following values:
eBULKREAD_CONFIG_NONE = -1,
eBULKREAD_CONFIG_1 = 0,
eBULKREAD_CONFIG_2 = 1,
eBULKREAD_CONFIG_3 = 2,
eBULKREAD_CONFIG_4 = 3,
eBULKREAD_CONFIG_MAX,
usAxisRefAn array
Defines the array that will contain the axis refs to be read (not masked), where
[NC_MAX_AXES_PER_BULK_READ] has a range between 1 and 100.
If an error is created, it should return the
NC_BULK_READ_NUM_OF_AXES_OUT_OF_RANGE error.
usNumberOfAxes
Defines the number of axes and is the total number of axes to be bulk read.
ucIsPreset
Whether the preset parameters are used or not. Values accepted are 0, or 1.
MMC_CONFIGBULKREAD_OUT Structure
typedef struct mmc_configbulkread_out{
float fFactorsAn array[NC_MAX_BULK_READ_READABLE_PACKET_SIZE];
unsigned short usStatus;
short sErrorID;
} MMC_CONFIGBULKREAD_OUT;
Parameters
fFactorsAn array
Defines what multiplication factor is needed to apply to each read parameter.
Dependent on the array [NC_MAX_BULK_READ_READABLE_PACKET_SIZE] with values
of 0 - 350.
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function.
Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.

### PDF page 1245
<a id="pdf-page-1245"></a>
Figure 396 describes the function for MMC_ConfigBulkRead.
[PDF field-code object omitted]
Figure 396: MMC_ConfigBulkRead function
13.1.1.1 Function Code Example
MMC_CONFIGBULKREAD_IN stCfgIn;
MMC_CONFIGBULKREAD_OUT stCfgOut;

int rc = NC_OK;

stCfgIn.eConfiguration = eBULKREAD_CONFIG_2;
stCfgIn.uBulkReadParams.eBulkReadPreset = eNC_BULKREAD_PRESET_2;
stCfgIn.ucIsPreset = 1;
stCfgIn.usAxisRefAn array[0] = 0;
stCfgIn.usAxisRefAn array[1] = 1;
stCfgIn.usNumberOfAxes = 2;

rc = MMC_ConfigBulkReadCmd(g_hConnectHndl, &stCfgIn, &stCfgOut);

etc.

### PDF page 1246
<a id="pdf-page-1246"></a>
###### 13.1.2 MMC_PerformBulkRead
13.1.2 MMC_PerformBulkRead
Reads those parameters which were configured by a call to ConfigBulkRead, from multiple axes.
MMC_LIB_API int MMC_PerformBulkReadCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_PERFORMBULKREAD_IN* pInParam,
OUT MMC_PERFORMBULKREAD_OUT* pOutParam
);
Motion Mode NC - Immaterial Distributed - Immaterial
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_PERFORMBULKREAD input data structure using the
MMC_PerformBulkRead function.
pOutParam
Points to the MMC_PERFORMBULKREAD_OUT output structure receiving information,
as a result of calling the MMC_PerformBulkRead function.
Remarks
None
Scope
All
MMC_PERFORMBULKREAD_IN Structure
typedef struct mmc_performbulkread_in{
NC_BULKREAD_CONFIG_ENUM eConfiguration;
} MMC_PERFORMBULKREAD_IN;
Parameters
eConfiguration
Defines the reading source. eBULKREAD_CONFIG_1 is reserved to the EAS application.
Acceptable values are eBULKREAD_CONFIG_1 and eBULKREAD_CONFIG_2.
NC_BULKREAD_CONFIG_ENUM defines the following values:

### PDF page 1247
<a id="pdf-page-1247"></a>
eBULKREAD_CONFIG_NONE,
eBULKREAD_CONFIG_1,
eBULKREAD_CONFIG_2,
eBULKREAD_CONFIG_MAX,
MMC_PERFORMBULKREAD_OUT Structure
typedef struct mmc_performbulkread_out{
unsigned long ulOutBuf[NC_MAX_BULK_READ_READABLE_PACKET_SIZE];
NC_BULKREAD_PRESET_ENUM eChosenPreset;
unsigned short usStatus;
short sErrorID;
} MMC_PERFORMBULKREAD_OUT;
Parameters
ulOutBuf[NC_MAX_BULK_READ_READABLE_PACKET_SIZE];
Defines the output buffer read data with a maximum
[NC_MAX_BULK_READ_READABLE_PACKET_SIZE] array size of 350.
eChosenPreset
The bulk read preset enumerator defined by NC_BULKREAD_PRESET_ENUM with
values:
eNC_BULKREAD_PRESET_NONE,
eNC_BULKREAD_PRESET_1,
eNC_BULKREAD_PRESET_2,
eNC_BULKREAD_PRESET_3,
eNC_BULKREAD_PRESET_4,
eNC_BULKREAD_PRESET_5,
eNC_BULKREAD_PRESET_MAX,
If a user defined parameters array is used for
ulBulkReadParameters[NC_MAX_REC_SIGNALS_NUM], then the value of
eChosenPreset will be eNC_BULKREAD_PRESET_MAX.
Each of the bulk read presets defines a fixed set of parameters to be
read:
typedef struct eNC_BULKREAD_PRESET_1{
int aPos;
int aVel;
int aTorque;
unsigned long ulAxisStatus;
unsigned int uiInputs;
OPM402 eOpMode;
}NC_BULKREAD_PRESET_1;

### PDF page 1248
<a id="pdf-page-1248"></a>
typedef struct eNC_BULKREAD_PRESET_2{
NC_BULKREAD_PRESET_1 stAxisParams;
int iFreeLargeFbsNumber;
int iFreeMediumFbsNumber
int iFreeSmallFbsNumber;
}NC_BULKREAD_PRESET_2;
typedef struct eNC_BULKREAD_PRESET_3{
int aPos;
int aVel;
int aTorque;
unsigned long ulAxisStatus;
unsigned int uiInputs;
OPM402 eOpMode;
NC_BULKREAD_VARIABLE_UNION ucCommError;
NC_BULKREAD_VARIABLE_UNION usLastEmcyErrorCode;
NC_BULKREAD_VARIABLE_UNION usControlWord;
NC_BULKREAD_VARIABLE_UNION usStatusWord;
}NC_BULKREAD_PRESET_3;
typedef struct eNC_BULKREAD_PRESET_4{
int aPos;
int aHWPos;
int iPosFollowingErr;
int aVel;
int aTorque;
unsigned long ulAxisStatus;
unsigned int uiInputs;
OPM402 eOpMode;
unsigned int uiStatusRegister;
unsigned int uiMcsLimitRegister;
}NC_BULKREAD_PRESET_4;
typedef struct eNC_BULKREAD_PRESET_5{
int aPos;
int aHWPos;
int iPosFollowingErr;
int aVel;
int aTorque;
unsigned long ulAxisStatus;
unsigned int uiInputs;
OPM402 eOpMode;
unsigned int uiStatusRegister;
unsigned int uiMcsLimitRegister;
NC_BULKREAD_VARIABLE_UNION usLastEmcyErrorCode;
NC_BULKREAD_VARIABLE_UNION usControlWord;
NC_BULKREAD_VARIABLE_UNION usStatusWord;
NC_BULKREAD_VARIABLE_UNION ucCommError;
}NC_BULKREAD_PRESET_5;

### PDF page 1249
<a id="pdf-page-1249"></a>
typedef union{
char cVar;
unsigned char ucVar;
short sVar;
unsigned short usVar;
int iVar;
unsigned int uiVar;
long lVar;
unsigned long ulVar;
}NC_BULKREAD_VARIABLE_UNION;
Where the user can select to use either of the parameters:
Character cVar, Unsigned character ucVar, short sVar, unsigned
short usVar, integer iVar, insigned integer uiVar, long lVar, or
un signed long ulVar.
Var is the value of the variable.
Each of these preset parameters have the following definitions:
aPos
Actual position integer. Integer values
aHWPos
Actual hardware position. Integer values
iPosFollowingErr
Position following error. Integer values/
aVel
Actual velocity integer
aTorque
Actual Torque integer
ulAxisStatus
Status of the axis with positive bitwise values
uiInputs
Digital inputs with any positive integer value
eOpMode
Motion mode
uiStatusRegister
Variable provides information on the special
status of an axis. Refer to the section 5.11.2
Interfaces. positive integer value.
uiMcsLimitRegister

### PDF page 1250
<a id="pdf-page-1250"></a>
Parameter represents the status of the MCS
limits in a specific group. positive integer
value.
iFreeLargeFbsNumber
Number of large free function blocks
avaliable in the queue for this size.
iFreeMediumFbsNumber
Number of medium sized free function blocks
avaliable in the queue for this size. Integer
value accepted
iFreeSmallFbsNumber
Number of small free function blocks
avaliable in each queue. Integer value
accepted
usControlWord
CANopen DS402 control word. Any positive
short value.
usStatusWord
CANopen DS402 status word. Any positive
short value.
ucCommError
Input axis communication error. positive
character values.
usLastEmcyErrorCode
Last recorded emergency code.
usLastEmcyErrorCode is the emergency error
code received from the drive.
ulBulkReadParameters
The array of ulBulkReadParameters defined by
[NC_MAX_REC_SIGNALS_NUM] represents a set of parameters to be
retrieved from the Maestro, with a maximum of 32 parameters.
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID

### PDF page 1251
<a id="pdf-page-1251"></a>
Returned command error ID. Signals where an error has occurred within the function.
Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 397 describes the function for MMC_PerformBulkRead.
[PDF field-code object omitted]
Figure 397: MMC_PerformBulkRead function
13.1.2.1 Function Code Example
MMC_PERFORMBULKREAD_IN stPerformBulkReadIn;
MMC_PERFORMBULKREAD_OUT stPerformBulkReadOut;
int rc = NC_OK;

stPerformBulkReadIn.eConfiguration = eBULKREAD_CONFIG_2;
rc = MMC_PerformBulkReadCmd(g_hConnectHndl, &stPerformBulkReadIn,
&stPerformBulkReadOut);
if (NC_OK != rc)
{
HandleError();
}
