# Chapter 18 Host Communication

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 1387-1409
- Chunk: `056_p1387-p1409_Chapter-18-Host-Communication.md`

## Active Outline At Chunk Start
- p. 1387 - Chapter 18 Host Communication
  - p. 1387 - 18.1 Modbus Communication Function Blocks

## Contained Bookmark Outline
- p. 1387 - Chapter 18 Host Communication
  - p. 1387 - 18.1 Modbus Communication Function Blocks
    - p. 1388 - 18.1.1 MMC_MbusIsRunning
    - p. 1390 - 18.1.2 MMC_MbusReadCoilsTable
    - p. 1393 - 18.1.3 MMC_MbusReadHoldingRegisterTable
    - p. 1396 - 18.1.4 MMC_MbusReadInputsTable
    - p. 1399 - 18.1.5 MMC_MbusStartServer
    - p. 1401 - 18.1.6 MMC_MbusStopServer
    - p. 1404 - 18.1.7 MMC_MbusWriteCoilsTable
    - p. 1407 - 18.1.8 MMC_MbusWriteHoldingRegisterTable

## Extracted Text

### PDF page 1387
<a id="pdf-page-1387"></a>
#### Chapter 18 Host Communication
##### 18.1 Modbus Communication Function Blocks
Chapter 18 Host Communication
Host communications consists of Modbus communications and will in the future consist of further
communication devices and protocols.
18.1 Modbus Communication Function Blocks
The Modbus interface allows the client to communicate using TCP/IP protocol with a Maestro compiled
program, and manipulate the functions of axis motions via the windows client Modbus program. Modbus
always uses default port 502, to poll data from Maestro, and update shared data.
Registry values adjust the movement of the drive, whereas Coil values act as switches, and are therefore 0 or 1.
The server API operates the Modbus using specific Registry or Coil tables, which are:
- Read/write to register values
- Read/write to coils Boolean values
- Read input coils
- Close Modbus
This is performed using a specific thread in MultiAxisControl that listens to a specific port, for changes in values
in registers/coils, through client's Modbus application. However, the Modbus Read values for both the coils
and registry tables cannot be input from external sources.
The server can read/write to specific registers/coils and use the values of the Modbus registers/coils, to
start/stop axes, or move engines to specific destinations using input parameters that come from Modbus.
These parameters can be altered through the application connected to the Maestro server.
The Modbus application can connect to a specific Maestro IP using a specific table ID opene d using the
Maestro's client application, start address, number of available values, read/write registers, read/write coils,
and via the Modbus application, alter running axes, and movement of axes through values shared between the
Maestro client's C application, and the Windows Modbus application.
The Windows Modbus application can alter the table ID it uses, read/write registers, read/write coils, read
inputs, and change the refresh rates of Modbus shared data displayed in the application.
The following Modbus communication function blocks are described:
Modbus Communication
MMC_MbusIsRunning
MMC_MbusReadCoilsTable
MMC_MbusReadHoldingRegisterTable
MMC_MbusReadInputsTable
MMC_MbusStartServer
MMC_MbusStopServer
MMC_MbusWriteCoilsTable
MMC_MbusWriteHoldingRegisterTable

### PDF page 1388
<a id="pdf-page-1388"></a>
###### 18.1.1 MMC_MbusIsRunning
18.1.1 MMC_MbusIsRunning
Signals that the Modbus connection is operational.
MMC_LIB_API int MMC_MbusIsRunning(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSISRUNNING_IN* pInParam,
OUT MMC_MODBUSISRUNNING_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_host_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_MODBUSISRUNNING input data structure using the
MMC_MbusIsRunning function.
pOutParam
Points to the MMC_MODBUSISRUNNING_OUT output structure receiving information,
as a result of calling the MMC_MbusIsRunning function.
Remarks
This function block checks whether the Modbus thread is running, and returns the isrunning parameter with
value 1, if the Modbus is running.
Scope
Not limited
MMC_MODBUSISRUNNING_IN Structure
typedef struct{
unsigned char dummy;
}MMC_MODBUSISRUNNING_IN;
Parameters
dummy
Modbus is connected to the server ID, with the following values:

### PDF page 1389
<a id="pdf-page-1389"></a>
MODBUS_NOT_STARTED = 0
MODBUS_RUNNING=1
MMC_MODBUSISRUNNING_OUT Structure
typedef struct{
unsigned short isrunning;
unsigned short usStatus;
short sErrorID;
}MMC_MODBUSISRUNNING_OUT;
Parameters
isrunning
Returns 1 if Modbus is running (MODBUS_RUNNING), otherwise 0
(MODBUS_NOT_STARTED).
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and 4.6NC Profiler Error
IDs.
Figure 428 describes the function block for MMC_MbusIsRunning as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 428: MMC_MbusIsRunning function block
18.1.1.1 Function Block Code Example
int rc;
MMC_MODBUSISRUNNING_IN stMbusIsRunning_in;
MMC_MODBUSISRUNNING_OUT stMbusIsRunning_out;
//
// Inserting the structure parameters:
stMbusIsRunning_in.dummy = 1; // Modbus is running (Boolean)
//
rc = MMC_MbusIsRunning (hConn, &stMbusIsRunning_in, &stMbusIsRunning_out);
if (rc != 0)
{
HandleError();
}

### PDF page 1390
<a id="pdf-page-1390"></a>
###### 18.1.2 MMC_MbusReadCoilsTable
18.1.2 MMC_MbusReadCoilsTable
Reads part of Modbus coils table.
MMC_LIB_API int MMC_MbusReadCoilsTable(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSREADCOILS_IN* pInParam,
OUT MMC_MODBUSREADCOILS_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_host_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_MODBUSREADCOILS input data structure using the
MMC_MbusReadCoilsTable function.
pOutParam
Points to the MMC_MODBUSREADCOILS_OUT output structure receiving information,
as a result of calling the MMC_MbusReadCoilsTable function.
Remarks
Reads the coils table inside the Modbus where every value >0, is similar to Boolean value 1 in the coil. The
function block variables include start reference, and reference count number of parameters to read. The
internal output parameter is coilsArr with Modbus values.
Scope
Not limited
MMC_MODBUSREADCOILS_IN Structure
typedef struct{
int startRef;
int refCnt;
}MMC_MODBUSREADCOILS_IN;
Parameters
startRef

### PDF page 1391
<a id="pdf-page-1391"></a>
Start Reference from the base coil table of linear parameters. Any positive integer
values accepted
refCnt
Reference count. Any positive integer values
MMC_MODBUSREADCOILS_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
char coilsArr[MODBUS_IPC_READ_VALUES];
}MMC_MODBUSREADCOILS_OUT;
Parameters
coilsArr
Value of the coils array, with 250 as the maximum number of items to read from
Modbus coils table. An array of positive string values.
[MODBUS_IPC_READ_VALUES] has a an array value of [0....250]
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
negative or positive integer values
Figure 429 describes the function block for MMC_MbusReadCoilsTable as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 429: MMC_MbusReadCoilsTable function block

18.1.2.1 Function Block Code Example
int rc;
MMC_MODBUSREADCOILS_IN stMbusReadCoils_in;
MMC_MODBUSREADCOILS_OUT stMbusReadCoils_out;
//
// Inserting the structure parameters:
stMbusReadCoils_in.startRef = 0; // Start Reference from the base coil
table of linear parameters
stMbusReadCoils_in.refCnt = 249;// Reference count

//

### PDF page 1392
<a id="pdf-page-1392"></a>
rc = MMC_MbusReadCoilsTable (hConn, &stMbusReadCoils_in,
&stMbusReadCoils_out);
printf("Mbus Coils Table Status[%ld][%ld] ErrId[%d]\n", (long
int)stMbusReadCoils_out.coilsArr[0], (long
int)stMbusReadCoils_out.coilsArr[1], (short)stMbusReadCoils_out.sErrorID);
if (rc != 0)
{
HandleError();
}

### PDF page 1393
<a id="pdf-page-1393"></a>
###### 18.1.3 MMC_MbusReadHoldingRegisterTable
18.1.3 MMC_MbusReadHoldingRegisterTable
Reads part of Modbus holding register table or the holding registers.
MMC_LIB_API int MMC_MbusReadHoldingRegisterTable(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSREADHOLDINGREGISTERSTABLE_IN *pInParam,
OUT MMC_MODBUSREADHOLDINGREGISTERSTABLE_OUT *pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_host_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_MODBUSREADHOLDINGREGISTERSTABLE input data structure
using the MMC_MbusReadHoldingRegisterTable function.
pOutParam
Points to the MMC_MODBUSREADHOLDINGREGISTERSTABLE_OUT output structure
receiving information, as a result of calling the MMC_MbusReadHoldingRegisterTable
function.
Remarks
Reads the registers table inside the Modbus, with parameters including, start reference, and reference
count number of parameters to read. The internal output parameter is regArr with Modbus values.
For IEC programming, the MC_MBReadHoldingRegisters allows greater flexibility for the inputs.
Scope
Not limited
MMC_MODBUSREADHOLDINGREGISTERSTABLE_IN Structure
typedef struct
{
int startRef;
int refCnt;
}MMC_MODBUSREADHOLDINGREGISTERSTABLE_IN;
Parameters

### PDF page 1394
<a id="pdf-page-1394"></a>
startRef
Start Reference from the base holding register table of linear parameters. Any positive
integer values accepted
refCnt
Reference count. Any positive integer values
MMC_MODBUSREADHOLDINGREGISTERSTABLE_OUT Structure
typedef struct
{
short regArr[MODBUS_IPC_READ_VALUES];
unsigned short usStatus;
short usErrorID;
}MMC_MODBUSREADHOLDINGREGISTERSTABLE_OUT;
Parameters
regArr[MODBUS_IPC_READ_VALUES]
Displays the array values of the registry tables, with 250 as the maximum number of
items to read from Modbus registry table. An array of positive string values.
[MODBUS_IPC_READ_VALUES] has an array value of [0....250]
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and 4.6NC Profiler Error
IDs. negative or positive integer values.
Figure 430 describes the function block for MMC_MbusReadHoldingRegisterTable as applied within the IEC
61131 programming.
[PDF field-code object omitted]
Figure 430: MMC_MbusReadHoldingRegisterTable function block

Figure 431: MC_MbusReadHoldingRegisters IEC function

### PDF page 1395
<a id="pdf-page-1395"></a>
Figure 432: MC_MbusReadHoldingRegisterTable IEC function
18.1.3.1 Function Block Code Example
int rc;
MMC_MODBUSREADHOLDINGREGISTERSTABLE_IN stMbusReadHoldingTable_in;
MMC_MODBUSREADHOLDINGREGISTERSTABLE_OUT stMbusReadHoldingTable_out;
//
// Inserting the structure parameters:
stMbusReadHoldingTable_in.startRef = 0;//Start Reference from the base
coil table of linear parameters
stMbusReadHoldingTable_in.refCnt = 249;// Reference count
//
rc = MMC_MbusReadHoldingRegisterTable (hConn, &stMbusReadHoldingTable_in,
&stMbusReadHoldingTable_out);
printf("Mbus Read Holding Register Table Status[%ld][%ld] ErrId[%d]\n",
(long int)stMbusReadHoldingTable_out.regArr[0], (long
int)stMbusReadHoldingTable_out.regArr[1],
(short)stMbusReadHoldingTable_out.sErrorID);
if (rc != 0)
{
HandleError();
}

### PDF page 1396
<a id="pdf-page-1396"></a>
###### 18.1.4 MMC_MbusReadInputsTable
18.1.4 MMC_MbusReadInputsTable
Reads inputs to the Modbus Inputs Table.
MMC_LIB_API int MMC_MbusReadInputsTable(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSREADINPUTS_IN *pInParam,
OUT MMC_MODBUSREADINPUTS_OUT *pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_host_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_MODBUSREADINPUTS input data structure using the
MMC_MbusReadInputsTable function.
pOutParam
Points to the MMC_MODBUSREADINPUTS_OUT output structure receiving
information, as a result of calling the MMC_MbusReadInputsTable function.
Remarks
Reads the Inputs table inside the Modbus, with parameters including, start reference, and reference count
number of parameters to read. The internal output parameter is InputsArr with Modbus values. The inputs
table cannot be changed by the Windows Modbus application, or using this API.
Scope
Not limited
MMC_MODBUSREADINPUTS_IN Structure
typedef struct{
int startRef;
int refCnt;
}MMC_MODBUSREADINPUTS_IN;
Parameters
startRef

### PDF page 1397
<a id="pdf-page-1397"></a>
Start reference from the base inputs table of linear parameters. Any positive integer
values accepted
refCnt
Reference count. Any positive integer values
MMC_MODBUSREADINPUTS_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
char inputsArr[MODBUS_IPC_READ_VALUES];
}MMC_MODBUSREADINPUTS_OUT;
Parameters
inputsArr[MODBUS_IPC_READ_VALUES]
Value of the inputs array, with 250 as the maximum number of items to read from the
Modbus inputs table. An array of positive string values.
[MODBUS_IPC_READ_VALUES] has a an array value of [0....250]
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and 4.6NC Profiler Error
IDs. negative or positive integer values
Figure 433 describes the function block for MMC_MbusReadInputsTable
[PDF field-code object omitted]
Figure 433: MMC_MbusReadInputsTable function block
18.1.4.1 Function Block Code Example
int rc;
MMC_MODBUSREADINPUTS_IN stMbusReadInputs_in;
MMC_MODBUSREADINPUTS_OUT stMbusReadInputs_out;
//
// Inserting the structure parameters:
stMbusReadInputs_in.startRef = 0; // Start Reference from the base coil
table of linear parameters
stMbusReadInputs_in.refCnt = 250;// Reference count
//
rc = MMC_MbusReadInputsTable (hConn, &stMbusReadInputs_in,
&stMbusReadInputs_out);
printf("Mbus Read Inputs Table Status[%ld][%ld] ErrId[%d]\n", (long
int)stMbusReadInputs_out.inputsArr[0], (long

### PDF page 1398
<a id="pdf-page-1398"></a>
int)stMbusReadInputs_out.inputsArr[1],
(short)stMbusReadInputs_out.sErrorID);
if (rc != 0)
{
HandleError();
}

### PDF page 1399
<a id="pdf-page-1399"></a>
###### 18.1.5 MMC_MbusStartServer
18.1.5 MMC_MbusStartServer
Starts the Modbus server listening thread with an ID value as a parameter.
MMC_LIB_API int MMC_MbusStartServer(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSSTARTSERVER_IN *pInParam,
OUT MMC_MODBUSSTARTSERVER_OUT *pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_host_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_MODBUSSTARTSERVER input data structure using the
MMC_MbusStartServer function.
pOutParam
Points to the MMC_MODBUSSTARTSERVER_OUT output structure receiving
information, as a result of calling the MMC_MbusStartServer function.
Remarks
None
Scope
Not limited
MMC_MODBUSSTARTSERVER_IN Structure
typedef struct{
unsigned short id;
}MMC_MODBUSSTARTSERVER_IN;
Parameters
id
Modbus start server enumerator ID has the following values:
MODBUS_NOT_STARTED = 0

### PDF page 1400
<a id="pdf-page-1400"></a>
MODBUS_RUNNING = 1
MODBUS_STOPPED = 2
After the Maestro is powered-up, Modbus server is in the MODBUS_NOT_STARTED
state - Initial state, the Modbus server does not exist.
After the Modbus server state is changed to MODBUS _RUNNING - the Modbus server
is created, transmissions from Modbus clients will be handled by the server.
When the Modbus server state is changed to MODBUS_STOPPED - the Modbus server
connection is removed, no transmissions will be handled from different Modbus clients.
MMC_MODBUSSTARTSERVER_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_MODBUSSTARTSERVER_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
negative or positive integer values
Figure 434 describes the function block for MMC_MbusStartServer
[PDF field-code object omitted]
Figure 434: MMC_MbusStartServer function block
18.1.5.1 Function Block Code Example
int rc;
MMC_MODBUSSTARTSERVER_IN stMbusStartServer_in;
MMC_MODBUSSTARTSERVER_OUT stMbusStartServer_out;
//
// Inserting the structure parameters:
stMbusStartServer_in.id = 1; // Modbus start server enumerator ID
//
rc = MMC_MbusStartServer (hConn, &stMbusStartServer_in,
&stMbusStartServer_out);
if (rc != 0)
{
HandleError();
}

### PDF page 1401
<a id="pdf-page-1401"></a>
###### 18.1.6 MMC_MbusStopServer
18.1.6 MMC_MbusStopServer
Stops the Modbus server listening thread.
MMC_LIB_API int MMC_MbusStopServer(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSSTOPSERVER_IN* pInParam,
OUT MMC_MODBUSSTOPSERVER_OUT *pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_host_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HND L is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_MODBUSSTOPSERVER input data structure using the
MMC_MbusStopServer function.
pOutParam
Points to the MMC_MODBUSSTOPSERVER_OUT output structure receiving
information, as a result of calling the MMC_MbusStopServer function.
Remarks
None
Scope
Not limited
MMC_MODBUSSTOPSERVER_IN Structure
typedef struct{
unsigned char dummy;
}MMC_MODBUSSTOPSERVER_IN;
Parameters
dummy
Dummy Modbus stops server input. 0, 1, 2 integer values accepted.

### PDF page 1402
<a id="pdf-page-1402"></a>
MMC_MODBUSSTOPSERVER_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_MODBUSSTOPSERVER_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 435 describes the function block for MMC_MbusStopServer
[PDF field-code object omitted]
Figure 435: MMC_MbusStopServer function block

### PDF page 1403
<a id="pdf-page-1403"></a>
18.1.6.1 Function Block Code Example
int rc;
MMC_MODBUSSTOPSERVER_IN stMbusStopServer_in;
MMC_MODBUSSTOPSERVER_OUT stMbusStopServer_out;
//
// Inserting the structure parameters:
stMbusStopServer_in.dummy = 0; // Modbus stops server input
//
rc = MMC_MbusStopServer (hConn, &stMbusStopServer_in,
&stMbusStopServer_out);
if (rc != 0)
{
HandleError();
}

### PDF page 1404
<a id="pdf-page-1404"></a>
###### 18.1.7 MMC_MbusWriteCoilsTable
18.1.7 MMC_MbusWriteCoilsTable
Writes to part of Modbus coils table inside the Modbus where every parameter >0, is similar to Boolean
value 1.
MMC_LIB_API int MMC_MbusWriteCoilsTable(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSWRITECOILS_IN *pInParam,
OUT MMC_MODBUSWRITECOILS_OUT *pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_host_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_MODBUSWRITECOILS input data structure using the
MMC_MbusWriteCoilsTable function.
pOutParam
Points to the MMC_MODBUSWRITECOILS_OUT output structure receiving information,
as a result of calling the MMC_MbusWriteCoilsTable function.
Remarks
The function parameters include start reference, reference count number of parameters, and table of
parameters.
Scope
Not limited
MMC_MODBUSWRITECOILS_IN Structure
typedef struct{
int startRef;
int refCnt;
char coilsArr[MODBUS_IPC_WRITE_VALUES];
}MMC_MODBUSWRITECOILS_IN;
Parameters

### PDF page 1405
<a id="pdf-page-1405"></a>
startRef
Start Reference from the base coil table of linear parameters. Any positive integer
values accepted
refCnt
Reference count. Any positive integer values
coilsArr
Value of the coils array, with 250 as the maximum number of items to read from
Modbus coils table. An array of positive string values.
[MODBUS_IPC_READ_VALUES] has a an array value of [0....250]
MMC_MODBUSWRITECOILS_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_MODBUSWRITECOILS_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
negative or positive integer values
Figure 436 describes the function block for MMC_MbusWriteCoilsTable
[PDF field-code object omitted]
Figure 436: MMC_MbusWriteCoilsTable function block
18.1.7.1 Function Block Code Example
int rc;
MMC_MODBUSWRITECOILS_IN stMbusWriteCoils_in;
MMC_MODBUSWRITECOILS_OUT stMbusWriteCoils_out;
//
// Inserting the structure parameters:
stMbusWriteCoils_in.startRef = 0; // Start Reference from the base
coil table of linear parameters
stMbusWriteCoils_in.refCnt = 249; // Reference count
stMbusWriteCoils_in.coilsArr[10] = 2; // Reference count
//
rc = MMC_MbusWriteCoilsTable (hConn, &stMbusWriteCoils_in,
&stMbusWriteCoils_out);
if (rc != 0)

### PDF page 1406
<a id="pdf-page-1406"></a>
{
HandleError();
}

### PDF page 1407
<a id="pdf-page-1407"></a>
###### 18.1.8 MMC_MbusWriteHoldingRegisterTable
18.1.8 MMC_MbusWriteHoldingRegisterTable
Writes to part of the Modbus register table inside the Modbus.
MMC_LIB_API int MMC_MbusWriteHoldingRegisterTable(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_IN* pInParam,
OUT MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_host_comm_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_MODBUSWRITEHOLDINGREGISTERSTABLE input data structure
using the MMC_MbusWriteHoldingRegisterTable function.
pOutParam
Points to the MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_OUT output structure
receiving information, as a result of calling the MMC_MbusWriteHoldingRegisterTable
function.
Remarks
The function parameters include start reference, reference count number of parameters to write, and the
register table with values to write into the Modbus table.
For IEC programming, the MC_MBWriteHoldingRegisters allows greater flexibility for the inputs.
Scope
Not limited
MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_IN Structure
typedef struct{
int startRef;
int refCnt;
short regArr[MODBUS_IPC_WRITE_VALUES];
}MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_IN;

### PDF page 1408
<a id="pdf-page-1408"></a>
Parameters
startRef
Start Reference from the base coil table of linear parameters. Any positi ve integer
values accepted
refCnt
Reference count. Any positive integer values
regArr
An array value of the register table, with 250 as the maximum number of items to write
from the Modbus registry table.
[MODBUS_IPC_READ_VALUES] has a an array value of [0....250]
MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and 4.6NC Profiler Error
IDs. negative or positive integer values
Figure 437 describes the function block for MMC_MbusWriteHoldingRegisterTable
[PDF field-code object omitted]
Figure 437: MMC_MbusWriteHoldingRegisterTable function block

Figure 438: MC_MbusWriteHoldingRegisters IEC function

### PDF page 1409
<a id="pdf-page-1409"></a>
Figure 439: MC_MbusWriteHoldingRegisterTable IEC function
18.1.8.1 Function Block Code Example
int rc;
MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_IN stMbusWriteHoldingRegTable_in;
MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_OUT stMbusWriteHoldingRegTable_out;
//
// Inserting the structure parameters:
stMbusWriteHoldingRegTable_in.startRef = 0; // Start Reference from the base
coil table of linear parameters
stMbusWriteHoldingRegTable_in.refCnt = 249; // Reference count
stMbusWriteHoldingRegTable_in.regArr[10] = 65534; // An array value of the
register table
//
rc = MMC_MbusWriteHoldingRegisterTable (hConn,
&stMbusWriteHoldingRegTable_in,
&stMbusWriteHoldingRegTable_out);
if (rc != 0)
{
HandleError();
}
