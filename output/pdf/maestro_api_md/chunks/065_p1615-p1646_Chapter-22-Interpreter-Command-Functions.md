# Chapter 22 Interpreter Command Functions

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 1615-1646
- Chunk: `065_p1615-p1646_Chapter-22-Interpreter-Command-Functions.md`

## Active Outline At Chunk Start
- p. 1615 - Chapter 22 Interpreter Command Functions

## Contained Bookmark Outline
- p. 1615 - Chapter 22 Interpreter Command Functions
  - p. 1616 - 22.1 Get Function - Asynchronous Mode
  - p. 1617 - 22.2 MMC_ElmoExecuteLabel
    - p. 1619 - 22.2.1 Function Code Example
  - p. 1620 - 22.3 MMC_ElmoSetParameter
    - p. 1622 - 22.3.1 Function Code Example
  - p. 1623 - 22.4 MMC_ElmoGetParameter
    - p. 1625 - 22.4.1 Function Code Example
  - p. 1626 - 22.5 MMC_ElmoGetAn array
    - p. 1628 - 22.5.1 Function Code Example
  - p. 1629 - 22.6 MMC_ElmoGetAn arrayAndRetrieveData
    - p. 1631 - 22.6.1 Function Code Example
  - p. 1632 - 22.7 MMC_ElmoGetParameterAndRetrieveData
    - p. 1634 - 22.7.1 Function Code Example
  - p. 1635 - 22.8 MMC_ElmoSetAn array
    - p. 1637 - 22.8.1 Function Code Example
  - p. 1638 - 22.9 MMC_ElmoQueryOperationFIFOIndex
    - p. 1639 - 22.9.1 Function Code Example
  - p. 1640 - 22.10 MMC_ElmoQueryOperationFIFORetrieveData
    - p. 1641 - 22.10.1 Function Code Example
  - p. 1642 - 22.11 MMC_ElmoQueryOperationFIFOIndexReset
    - p. 1643 - 22.11.1 Function Code Example
  - p. 1644 - 22.12 MMC_ElmoCall
    - p. 1646 - 22.12.1 Function Block Code Example

## Extracted Text

### PDF page 1615
<a id="pdf-page-1615"></a>
#### Chapter 22 Interpreter Command Functions
Chapter 22 Interpreter Command Functions
This section describes the functions (these are not function blocks) that are downloaded to the servo driver via
Bin Interpreter or OS Interpreter mechanism. These functions may use RPC or IPC c ommunication to perform
the download. The purpose of these functions is to allow users of a command interpreter to access the servo
driver and Maestro via direct commands, and to move the axis via a list of commands. However, the use of the
command interpreter is limited to single axis and restricts the Maestro's capabilities.
The Get type functions perform in two ways:
- Synchronously
The function does not return to the Maestro server until a respons e is received from the servo driver.
- Asynchronously
The function returns immediately to the Maestro server, without waiting for a response from the servo
driver. In the Maestro, when the response from the servo driver is received, it is sent via a UDP
message to the library, where the connection listener thread processes the message.
If the message is a Binary Interpreter Get command, it is processed by checking whether the Query
operation mode is set to asynchronous query mode. If so, the uploaded data is kept per axis FIFO (the
asynchronous query FIFO) and the axis asynchronous query FIFO index is incremented.
However, if the Query operation mode is set to synchronous query mode, the response data is copied
to the user out structure parameter.
For every data stored in the FIFO, the FIFO index is incremented. The user can access the axis
asynchronous query FIFO whenever and retrieve data from the axis FIFO according to the axis FIFO
index progress, whose value can be monitored via the ElmoQueryOperationFIFO Index function.
The data returned from the Get functions, may be long or float. Therefore, storing the uploaded library data in
the axes' FIFOs and enable retrieving of the data by the user is dependently performed on the specific data
type saved.
Note: For the asynchronous Get operation, a FIFO with size 1 is managed for each axis.
If an error is sent from the servo driver because of sending an Interpreter command, the queue will be emptied
and an error will be sent to the library.

### PDF page 1616
<a id="pdf-page-1616"></a>
##### 22.1 Get Function - Asynchronous Mode
22.1 Get Function - Asynchronous Mode
Can operate in sync and async modes. To operate in this mode, the user sets the Query operation mode to
asynchronous mode. When a UDP message arrives to the connection UDP listener thread, the thread checks
that it is a SendRawData - Get response, and if so, will place the data as a specific entry in a d edicated FIFO of
the designated axis.
When it reaches the last entry at the FIFO, the index will not be incremented, until the user will reset the index
via the ElmoQueryOperationFIFOIndexReset() function. It is responsibility of the user, to reset the Que ry
Operation FIFO at the right time, to prevent the last record overrunning in the FIFO. The user can retrieve data
from the axis Query Operation FIFO at location index in any time, via the
ElmoQueryOperationFIFORetrieveData(index) function.
In order to perform Get operation in asynchronous mode, for example: et[1] for 4 axes, the following should be
performed:
- ElmoGetAn array(axisref1, "et", 1) to axis 1.
- ElmoGetAn array(axisref2, "et", 1) to axis 2.
- ElmoGetAn array(axisref3, "et", 1) to axis 3.
- ElmoGetAn array(axisref4, "et", 1) to axis 4.
- ElmoQueryOperationFIFOIndex(axisref) - to return to the current index buffer location which occupies the
asynchronous returned replies for a specific axis. When the current index location for one of the 1 -4
axes is 1, then for that axis the asynchronous reply was received and we can start retrieving it via the,
ElmoQueryOperationFIFORetrieveData(axisref, index) function.
- ElmoQueryOperationFIFORetrieveData(axisref1, 1) - Get asynchronous reply of axis 1 which located at
the first entry in the axis FIFO.
The following Interpreter Command functions are described:
Interpreter Command
MMC_ElmoExecuteLabel
MMC_ElmoSetParameter
MMC_ElmoGetParameter
MMC_ElmoGetAn array
MMC_ElmoGetAn arrayAndRetrieveData
MMC_ElmoGetParameterAndRetrieveData
MMC_ElmoSetAn array
MMC_ElmoQueryOperationFIFOIndex
MMC_ElmoQueryOperationFIFORetrieveData
MMC_ElmoQueryOperationFIFOIndexReset
MMC_ElmoCall

### PDF page 1617
<a id="pdf-page-1617"></a>
##### 22.2 MMC_ElmoExecuteLabel
22.2 MMC_ElmoExecuteLabel
Executes the user program that was downloaded via the EAS application.
MMC_LIB_API int MMC_ElmoExecuteLabel(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_INTERPEXECUTECMD_IN* pInParam,
OUT MMC_INTERPEXECUTECMD_OUT* pOutParam
);

int ElmoExecuteLabel(
const char *szCmd
)throw (CMMCException);

public void ElmoExecute(
byte[] pData,
byte ucLength
)
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_drive_comm_API.h
GMAS\includes\CPP\MMCEoE.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
hAxisRef
Axis/group reference handle type returned by the GetAxisRef command
pInParam
Points to the MMC_INTERPEXECUTECMD_IN input data structure using the
MMC_ElmoExecuteLabel function.
pOutParam
Points to the MMC_INTERPEXECUTECMD_OUT output structure receiving information,
as a result of calling the MMC_ElmoExecuteLabel function.
Remarks
This function may communicate via an RPC or IPC connection.
Scope

### PDF page 1618
<a id="pdf-page-1618"></a>
Note: When using the binary interpreter, first open a UDP channel. Otherwise, the binary interpreter
functions will return error -10(Lib error).
MMC_INTERPEXECUTECMD_IN Structure
typedef struct{
unsigned char ucLength;
unsigned char pData[NODE_ASCII_ARRAY_MAX_LENGTH];
}MMC_INTERPEXECUTECMD_IN;
Parameters
ucLength
Length of the label string. Length with the precursor format of [Metronome
command]##[label]. Any positive character values.
pData
String Data with the precursor format of [Metronome command]##[label]. Any positive
character values with a maximum length of 80 bytes
[NODE_ASCII_ARRAY_MAX_LENGTH] is the node ASCII array integers with a maximum
length of 80 bytes.
MMC_INTERPEXECUTECMD_OUT Structure
typedef struct{
unsigned short usStatus;
short usErrorID;
}MMC_INTERPEXECUTECMD_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function block .
Refer to the errors listed in sections 4.3 Maestro Error IDs, and 4.6 NC Profiler Error IDs.
Figure 505 describes the function for MMC_ElmoExecuteLabel as applied within the IEC 61131 programming
for MC_ElmoExecute.
[PDF field-code object omitted]
Figure 505: MMC_ElmoExecuteLabel function

### PDF page 1619
<a id="pdf-page-1619"></a>
###### 22.2.1 Function Code Example
22.2.1 Function Code Example
int rc;
MMC_INTERPEXECUTECMD_IN stInterpExCmd_in;
MMC_INTERPEXECUTECMD_OUT stInterpExCmd_out;
//
// Inserting the structure parameters:

stInterpExCmd_in.ucLength = sizeof ("XQ##start"); //Length of the data
strcpy((char*) stInterpExCmd_in.pData,"XQ##start"); //Data

//
rc = MMC_ElmoExecuteLabel (hConn, iAxisRef, &stInterpExCmd_in,
&stInterpExCmd_out);
if (rc != 0)
{
HandleError();
}

### PDF page 1620
<a id="pdf-page-1620"></a>
##### 22.3 MMC_ElmoSetParameter
22.3 MMC_ElmoSetParameter
Sets the Elmo drive parameter with a specific name in the servo drive.
MMC_LIB_API int MMC_ElmoSetParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN char cCmd[3],
IN unsigned char ucValType,
IN void* pVal
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_drive_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
hAxisRef
Axis/group reference handle type returned by the GetAxisRef command
cCmd[3]
Name of the parameter limited to three characters. Any positive character value with a
maximum of 2 bytes
ucValType
Data value type, whether integer or float. Integer or float with values of 0 or 1.
pVal
Pointer to data that is to be set. Point value of a maximum of 4 bytes (Void)
Remarks
These functions can communicate via an RPC or IPC connection.
Scope
Note: When using the binary interpreter, first open a UDP channel. Otherwise, the binary interpreter
functions will return error -10(Lib error).

### PDF page 1621
<a id="pdf-page-1621"></a>
Figure 506 and Figure 507 describe the function block for MMC_ElmoSetParam as applied within the IEC 61131
programming for ElmoSetFloatParam and ElmoSetIntParam. The MMC_ElmoSetXXXXParam parameters differ
in C language to the IEC.
[PDF field-code object omitted]
Figure 506: MMC_ElmoSetIntParam function block
[PDF field-code object omitted]
Figure 507: MMC_ElmoSetFloatParam function block

### PDF page 1622
<a id="pdf-page-1622"></a>
###### 22.3.1 Function Code Example
22.3.1 Function Code Example
int rc;
//
// Inserting the structure parameters:
strcpy(cCmd,"MO"); //Name of the parameter
iAn arrayIdx = 1; //Index of array element
ucValType = 0; //Data value type
strcpy(pVal, "1"); //Pointer to data that is to be set
//
rc = MMC_ElmoSetParameter (hConn, iAxisRef, cCmd, iAn arrayIdx, ucValType,
pVal);
if (rc != 0)
{
HandleError();
}

### PDF page 1623
<a id="pdf-page-1623"></a>
##### 22.4 MMC_ElmoGetParameter
22.4 MMC_ElmoGetParameter
Request to receive the Elmo parameters from the servo drive.
MMC_LIB_API int MMC_ElmoGetParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN char cCmd[3],
OUT unsigned char ucValType
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_drive_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
hAxisRef
Axis/group reference handle type returned by the GetAxisRef command
cCmd[3]
Name of the parameter limited to three characters. Any positive character value with a
maximum of 2 bytes
ucValType
Data value type, whether integer or float. Integer or float with va lues of 0 or 1.
Remarks
These functions may communicate via an RPC or IPC connection.
Scope
Note: When using the binary interpreter, first open a UDP channel. Otherwise, the binary interpreter
functions will return error -10(Lib error).

### PDF page 1624
<a id="pdf-page-1624"></a>
Figure 508 and Figure 509 describes the function block for MMC_ElmoGetParameter as applied within the IEC
61131 programming for ElmoGetFloatParam and ElmoGetIntParam. The MMC_ElmoGetXXXXParam
parameters differ in C language to the IEC. The C version waits for the sync
MMC_GetParameterAndRetrieveData to produce the pVal output. However the IEC version automatically
stores the pVal output.

MMC_CONNECT_HNDL
MMC_ElmoGetIntParam
hConn
ucEnable
usStatus
Valid, Busy, Error
usErrorID
Bitwise
Error code
Boolean
@Axis
Character cCmd[3]
0 ucValType
pVal Any value

Figure 508: MMC_ElmoGetIntParam function block

MMC_CONNECT_HNDL
MMC_ElmoGetFloatParam
hConn
ucEnable
usStatus
Valid, Busy, Error
usErrorID
Bitwise
Error code
Boolean
@Axis
Character cCmd[3]
1 ucValType
pVal Any value

Figure 509: MMC_ElmoGetFloatParam function block

### PDF page 1625
<a id="pdf-page-1625"></a>
###### 22.4.1 Function Code Example
22.4.1 Function Code Example
int rc;
//
// Inserting the structure parameters:
strcpy(cCmd,"MO"); //Name of the parameter
ucValType = 0; //Data value type
//
rc = MMC_ElmoGetParameter (hConn, iAxisRef, cCmd, ucValType);
if (rc != 0)
{
HandleError();
}

### PDF page 1626
<a id="pdf-page-1626"></a>
##### 22.5 MMC_ElmoGetAn array
22.5 MMC_ElmoGetAn array
Request to receive an element from the array parameters in the servo drive.
MMC_LIB_API int MMC_ElmoGetAn array(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN char cCmd[3],
IN short iAn arrayIdx,
IN unsigned char ucValType
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_drive_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
hAxisRef
Axis/group reference handle type returned by the GetAxisRef command
cCmd[3]
[IN] Name of the parameter limited to three characters. Any positive character value
with a maximum of 2 bytes
iAn arrayIdx
[IN] Index of array element. Any positive or negative short integer value with a
maximum of 2 bytes.
ucValType
[OUT] Data value type, whether integer or float. Integer or float with values of 0 or 1.
Remarks
These functions may communicate via an RPC or IPC connection.
Scope
Note: When using the binary interpreter, first open a UDP channel. Otherwise, the binary interpreter
functions will return error -10(Lib error).

### PDF page 1627
<a id="pdf-page-1627"></a>
Figure 511 and Figure 510 describe the function block for MMC_ElmoGetAn array as applied within the IEC
61131 programming for ElmoGetFloatArr and ElmoGetIntArr. The MMC_ElmoGetXXXX An array parameters
differ in C language to the IEC. The C version waits for the sync MMC_GetAn arrayAndRetrieveData to produce
the pVal output. However the IEC version automatically stores the pVal output.

MMC_CONNECT_HNDL
MMC_ElmoGetIntArray
hConn
ucEnable
usStatus
Valid, Busy, Error
usErrorID
Bitwise
Error code
Boolean
@Axis
Character cCmd[3]
Short iArrayIdx
0 ucValType
pVal Any value

Figure 510: MMC_ElmoGetIntAn array function block

MMC_CONNECT_HNDL
MMC_ElmoGetFloatArray
hConn
ucEnable
usStatus
Valid, Busy, Error
usErrorID
Bitwise
Error code
Boolean
@Axis
Character cCmd[3]
Short iArrayIdx
1 ucValType
pVal Any value

Figure 511: MMC_ElmoGetFloatAn array function block

### PDF page 1628
<a id="pdf-page-1628"></a>
###### 22.5.1 Function Code Example
22.5.1 Function Code Example
int rc;
//
// Inserting the structure parameters:
strcpy(cCmd,"MO"); //Name of the parameter
iAn arrayIdx = 1; //Index of array element
ucValType = 0; //Data value type
//
rc = MMC_ElmoGetAn array (hConn, iAxisRef, cCmd, iAn arrayIdx, ucValType);
if (rc != 0)
{
HandleError();
}

### PDF page 1629
<a id="pdf-page-1629"></a>
##### 22.6 MMC_ElmoGetAn arrayAndRetrieveData
22.6 MMC_ElmoGetAn arrayAndRetrieveData
Synchronously requests an element from the array parameters in the servo drive and retrieves it.
MMC_LIB_API int MMC_ElmoGetAn arrayAndRetrieveData(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN char cCmd[3],
IN short iAn arrayIdx,
IN unsigned char ucValType,
OUT void *pVal,
OUT unsigned int* uiErrorID
);
Motion
Mode
NC - Supported Distributed - Supported
Source GMAS\includes\MMC_drive_comm_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the Connection
Handle Type. It should be noted that this connection handle is common throughout all
Maestro functions. This connection handle is returned by the Init Connection command. If
an error occurs, the function returns -1 and a MMC_LIB_API error with more details.
hAxisRef
[IN] Axis/group reference handle type returned by the GetAxisRef command
cCmd[3]
[IN] Name of the parameter limited to three characters. Any positive character value with a
maximum of 2 bytes
iAn arrayIdx
[IN] Index of array element. Any positive or negative short integer value with a maximum
of 2 bytes.
ucValType
[IN] Data value type, whether integer or float. Integer or float with values of 0 or 1.
pVal
[OUT] Copies a point from where to retrieve the data. Point value of a maximum of 4 bytes
(Void)
uiErrorID
[OUT] Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and4.6 NC Profiler Error IDs.
Function init with values of 0 or positive error_id.

### PDF page 1630
<a id="pdf-page-1630"></a>
Remarks
This command will necessitate waiting for the data element to be retrieved or an error returned. No other
process command may be sent meanwhile.
Scope
For synchronous communication only

### PDF page 1631
<a id="pdf-page-1631"></a>
###### 22.6.1 Function Code Example
22.6.1 Function Code Example
int rc;
//
// Inserting the structure parameters:
strcpy(cCmd,"MO"); //Name of the parameter
iAn arrayIdx = 1; //Index of array element
ucValType = 0; //Data value type
//
rc = MMC_ElmoGetAn arrayAndRetrieveData (hConn, iAxisRef, cCmd, iAn
arrayIdx, ucValType, pVal, uiErrorID);
printf("Elmo Get An array and Retrieve Data Status[%ld] ErrId[%d]\n", (long
int)pVal, (int)uiErrorID);
if (rc != 0)
{
HandleError();
}

### PDF page 1632
<a id="pdf-page-1632"></a>
##### 22.7 MMC_ElmoGetParameterAndRetrieveData
22.7 MMC_ElmoGetParameterAndRetrieveData
Synchronously requests a parameter in the servo drive and retrieves it.
MMC_LIB_API int MMC_ElmoGetParameterAndRetrieveData(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN char cCmd[3],
IN unsigned char ucValType,
OUT void *pVal,
OUT unsigned int* uiErrorID);
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_drive_comm_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
hAxisRef
[IN] Axis/group reference handle type returned by the GetAxisRef command
cCmd[3]
[IN] Name of the parameter limited to three characters. Any positive character value
with a maximum of 2 bytes
ucValType
[IN] Data value type, whether integer or float. Integer or float with values of 0 or 1.
pVal
[OUT] Copies a point from where to retrieve the data. Point value of a maximum of 4
bytes (Void)
uiErrorID
[OUT] Returned command error ID. Signals where an error has occurred within the
function block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and NC
Profiler Error IDs. Function init with values of 0 or positive error_id.
Remarks
This command will necessitate waiting for the data parameter to be retrieved or an error returned. No other
process command may be sent meanwhile.

### PDF page 1633
<a id="pdf-page-1633"></a>
Scope
For synchronous communication only

### PDF page 1634
<a id="pdf-page-1634"></a>
###### 22.7.1 Function Code Example
22.7.1 Function Code Example
int rc;
//
// Inserting the structure parameters:
strcpy(cCmd,"MO"); //Name of the parameter
ucValType = 0; //Data value type
//
rc = MMC_ElmoGetParameterAndRetrieveData (hConn, iAxisRef, cCmd, ucValType,
pVal, uiErrorID);
printf("Elmo Get Parameter and Retrieve Data Status[%ld] ErrId[%d]\n",
(long int)pVal, (int)uiErrorID);
if (rc != 0)
{
HandleError();
}

### PDF page 1635
<a id="pdf-page-1635"></a>
##### 22.8 MMC_ElmoSetAn array
22.8 MMC_ElmoSetAn array
Sets an element from the array of parameters in the servo drive.
MMC_LIB_API int MMC_ElmoSetAn array(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN char cCmd[3],
IN short iAn arrayIdx,
IN unsigned char ucValType,
IN void* pVal
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_drive_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is t he
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
hAxisRef
[IN] Axis/group reference handle type returned by the GetAxisRef command
cCmd[3]
[IN] Name of the parameter limited to three characters. Any positive character value
with a maximum of 2 bytes
iAn arrayIdx
[IN] Index of array element. Any positive or negative short integer value with a
maximum of 2 bytes.
ucValType
[IN] Data value type, whether integer or float. Integer or float with values of 0 or 1.
pVal
[IN] Pointer to data that is to be set. Point value of a maximu m of 4 bytes (Void)
Remarks
This command will necessitate waiting for the data element to be retrieved or an error returned. No other
process command may be sent meanwhile.

### PDF page 1636
<a id="pdf-page-1636"></a>
Scope
Note: When using the binary interpreter, first open a UDP channel. Otherwise, the binary interpreter
functions will return error -10(Lib error).
Figure 512 and Figure 512 describe the function block for MMC_ElmoSetAn array as applied within the IEC
61131 programming for ElmoSetFloatArr and ElmoSetIntArr. The MMC_ElmoSetXXXX An array parameters
differ in C language to the IEC.

MMC_CONNECT_HNDL
MMC_ElmoSetIntArray
hConn
Execute
usStatus
Done, Busy, Error
usErrorID
Bitwise
Error code
Boolean
@Axis
Character cCmd[3]
Short iArrayIdx
0 ucValType
Any value pVal

Figure 512: MMC_ElmoSetIntAn array function block

MMC_CONNECT_HNDL
MMC_ElmoSetFloatArray
hConn
Execute
usStatus
Done, Busy, Error
usErrorID
Bitwise
Error code
Boolean
@Axis
Character cCmd[3]
Short iArrayIdx
1 ucValType
Any value pVal

Figure 513: MMC_ElmoSetFloatAn array function block

### PDF page 1637
<a id="pdf-page-1637"></a>
###### 22.8.1 Function Code Example
22.8.1 Function Code Example
int rc;
//
// Inserting the structure parameters:
strcpy(cCmd,"IL"); //Name of the parameter
iAn arrayIdx = 1; //Index of array element
ucValType = 0; //Data value type
strcpy(pVal, "1"); //Pointer to data that is to be set
//
rc = MMC_ElmoSetAn array (hConn, iAxisRef, cCmd, iAn arrayIdx, ucValType,
pVal);
if (rc != 0)
{
HandleError();
}

### PDF page 1638
<a id="pdf-page-1638"></a>
##### 22.9 MMC_ElmoQueryOperationFIFOIndex
22.9 MMC_ElmoQueryOperationFIFOIndex
Returns the FIFO index.
MMC_LIB_API int MMC_ElmoQueryOperationFIFOIndex(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
OUT int* iReceivedMsgIdx
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_drive_comm_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
hAxisRef
[IN] Axis/group reference handle type returned by the GetAxisRef command
iReceivedMsgIdx
[OUT] Index of the number of received messages. Function init with values 0 or
error_id.
Remarks
None
Scope
Note: When using the binary interpreter, first open a UDP channel. Otherwise, the binary interpreter
functions will return error -10(Lib error).

### PDF page 1639
<a id="pdf-page-1639"></a>
###### 22.9.1 Function Code Example
22.9.1 Function Code Example
int rc;
//
rc = MMC_ElmoQueryOperationFIFOIndex (hConn, iAxisRef, iReceivedMsgIdx);
printf("Elmo Query Operation FIFO Index Status[%ld]\n", (long
int)iReceivedMsgIdx);
if (rc != 0)
{
HandleError();
}

### PDF page 1640
<a id="pdf-page-1640"></a>
##### 22.10 MMC_ElmoQueryOperationFIFORetrieveData
22.10 MMC_ElmoQueryOperationFIFORetrieveData
Request the FIFO index to retrieve data.
MMC_LIB_API MMC_LIB_API int MMC_ElmoQueryOperationFIFORetrieveData(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
OUT void *pVal,
OUT unsigned int* uiErrorID
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_drive_comm_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
hAxisRef
[IN] Axis/group reference handle type returned by the GetAxisRef command
pVal
[OUT] Pointer to data that is to be set. Point value of a maximum of 4 bytes (Void).
uiErrorID
[OUT] Returned command error ID. Signals where an error has occurred within the
function block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler
Error IDs. Function init with values of 0 or positive error_id.
Remarks
If the FIFO index is 0, either an error or no data is received. If 1, data is received.
Scope
Note: When using the binary interpreter, first open a UDP channel. Otherwise, the binary interpreter
functions will return error -10(Lib error).

### PDF page 1641
<a id="pdf-page-1641"></a>
###### 22.10.1 Function Code Example
22.10.1 Function Code Example
int rc;
//
rc = MMC_ElmoQueryOperationFIFORetrieveData (hConn, iAxisRef, pVal,
uiErrorID);
printf("Elmo Query Operation FIFO Retrieve Data Status[%ld] ErrId[%d]\n",
(long int)pVal, (int)uiErrorID);
if (rc != 0)
{
HandleError();
}

### PDF page 1642
<a id="pdf-page-1642"></a>
##### 22.11 MMC_ElmoQueryOperationFIFOIndexReset
22.11 MMC_ElmoQueryOperationFIFOIndexReset
Erases the message FIFO to 0.
MMC_LIB_API int MMC_ElmoQueryOperationFIFOIndexReset(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_drive_comm_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
hAxisRef
[IN] Axis/group reference handle type returned by the GetAxisRef command
Remarks
None
Scope
Note: When using the binary interpreter, first open a UDP channel. Otherwise, the binary interpreter
functions will return error -10(Lib error).

### PDF page 1643
<a id="pdf-page-1643"></a>
###### 22.11.1 Function Code Example
22.11.1 Function Code Example
void MMC_ElmoQueryOperationFIFOIndexReset_wrapper(int iAxisRef)
{
int rc;
//
rc = MMC_ElmoQueryOperationFIFOIndexReset (hConn, iAxisRef);
if (rc != 0)
{
HandleError();
}
}

### PDF page 1644
<a id="pdf-page-1644"></a>
##### 22.12 MMC_ElmoCall
22.12 MMC_ElmoCall
ElmoCall is used to call a subroutine, a user program, where cCmd[3] is the name of the program
MMC_LIB_API int MMC_ElmoCall(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN char cCmd[3]
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_drive_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
hAxisRef
[IN] Axis/group reference handle type returned by the GetAxisRef command
cCmd[3]
[IN] Name of the program limited to three characters. Any positive character value with
a maximum of 2 bytes
Remarks
Some parameters are not true variables but are direct commands to operate the servo drive. ElmoCall uses
these commands to call a specific parameter from the servo drive.
Scope
All

### PDF page 1645
<a id="pdf-page-1645"></a>
Figure 514 describes the function block for MMC_ElmoCall as applied within the IEC 61131 programming.

MMC_CONNECT_HNDL
MMC_ElmoCall
hConn
Execute
usStatus
Done, Busy, Error
usErrorID
Bitwise
Error code
Boolean
@Axis
Character cCmd[3]

Figure 514: MMC_ElmoCall function block

### PDF page 1646
<a id="pdf-page-1646"></a>
###### 22.12.1 Function Block Code Example
22.12.1 Function Block Code Example
int rc;
//
// Inserting the structure parameters:
strcpy(cCmd,"BG"); //Name of the parameter
//
rc = MMC_ElmoCall (hConn, iAxisRef, cCmd);
if (rc != 0)
{
HandleError();
}
