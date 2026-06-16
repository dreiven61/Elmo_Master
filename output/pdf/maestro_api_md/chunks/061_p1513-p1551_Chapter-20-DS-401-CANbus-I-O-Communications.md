# Chapter 20 DS-401 CANbus I/O Communications

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 1513-1551
- Chunk: `061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md`

## Active Outline At Chunk Start
- p. 1513 - Chapter 20 DS-401 CANbus I/O Communications
  - p. 1513 - 20.1 DS-401 Function Blocks

## Contained Bookmark Outline
- p. 1513 - Chapter 20 DS-401 CANbus I/O Communications
  - p. 1513 - 20.1 DS-401 Function Blocks
    - p. 1514 - 20.1.1 MMC_CancelGeneralRPDO3
    - p. 1516 - 20.1.2 MMC_CancelGeneralRPDO4
    - p. 1519 - 20.1.3 MMC_CancelGeneralTPDO3
    - p. 1521 - 20.1.4 MMC_CancelGeneralTPDO4
    - p. 1524 - 20.1.5 MMC_ConfigGeneralRPDO3
    - p. 1526 - 20.1.6 MMC_ConfigGeneralRPDO4
    - p. 1529 - 20.1.7 MMC_ConfigGeneralTPDO3
    - p. 1531 - 20.1.8 MMC_ConfigGeneralTPDO4
    - p. 1534 - 20.1.9 MMC_DisableDS401DIChangedEvent
    - p. 1537 - 20.1.10 MMC_EnableDS401DIChangedEvent
    - p. 1540 - 20.1.11 MMC_ReadDS401DIGroup
    - p. 1543 - 20.1.12 MMC_ReadDS401DInput
    - p. 1546 - 20.1.13 MMC_WriteDS401DOGroup
    - p. 1549 - 20.1.14 MMC_WriteDS401DOutput

## Extracted Text

### PDF page 1513
<a id="pdf-page-1513"></a>
#### Chapter 20 DS-401 CANbus I/O Communications
##### 20.1 DS-401 Function Blocks
Chapter 20 DS-401 CANbus I/O Communications
This section details the CANbus input and output communication to the Maestro (DS -401 digital and analog
input/output modules). This form of communication uses the CANopen protocol and device profile
specification for embedded systems used in automation.
The purpose of Input/Output modules is to connect sensors and actuators to CANopen networks. In
operational mode, input data can be transmitted from the inputs via TP DOs. By default, the PDO transmission
is triggered by an interrupt (event). Optionally, PDOs may be transmitted synchronously or remotely requested.
In addition, it is possible to read input data via SDO communication from another module, or to write data via
SDO to the network, if the module provides SDO client functions. Output data can be received via RPDO by
those Input/Output modules that have output capabilities. Output data can also be received via SDO
communication services. However, the main purpose of SDO communication is to configure an Input/Output
module. Via SDO, the module can receive Input/Output configuration data, and parameters for converting data
into meaningful measurements and so on. Input/Output modules compliant with this device profile use pre-
defined PDOs. The default mapping of application objects into TPDO and respectively RPDO may be changed
via SDO, if variable PDO mapping is supported. An Input/Output module may provide optionally Sync
producer/consumer, Time-Stamp producer/consumer, and Emergency producer/consumer functions. For new
servo driver designs, it is highly recommended to support Heartbeat functions.
It should be noted that the valid bit and additional logic added to the PDO mappi ng sequence require that the
MMC_CancelGeneralXXPDOX functions must be called prior to calling the relevant
MMC_ConfigGeneralXXPDOX function.
20.1 DS-401 Function Blocks
The following DS-401 I/O communication function blocks are described:

DS-401 I/O Communications
MMC_CancelGeneralRPDO3
MMC_CancelGeneralRPDO4
MMC_CancelGeneralTPDO3
MMC_CancelGeneralTPDO4
MMC_ConfigGeneralRPDO3
MMC_ConfigGeneralRPDO4
MMC_ConfigGeneralTPDO3
MMC_ConfigGeneralTPDO4
MMC_DisableDS401DIChangedEvent
MMC_EnableDS401DIChangedEvent
MMC_ReadDS401DIGroup
MMC_ReadDS401DInput
MMC_WriteDS401DOGroup
MMC_WriteDS401DOutput

### PDF page 1514
<a id="pdf-page-1514"></a>
###### 20.1.1 MMC_CancelGeneralRPDO3
20.1.1 MMC_CancelGeneralRPDO3
Cancels the general configuration of the DS-401 node or Maestro for RX at PDO3.
MMC_LIB_API int MMC_CancelGeneralRPDO3(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CANCELGENERALRPDO3_IN* pInParam,
OUT MMC_CANCELGENERALRPDO3_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_CANCELGENERALRPDO3 input data structure using the
MMC_CancelGeneralRPDO3 function.
pOutParam
Points to the MMC_CANCELGENERALRPDO3_OUT output structure receiving
information, as a result of calling the MMC_CancelGeneralRPDO3 function.
Remarks
Cancels communications to read/write PDO's sent from the Maestro or host.
Scope
All
MMC_CANCELGENERALRPDO3_IN Structure
typedef struct{
unsigned char ucDummy;
}MMC_CANCELGENERALRPDO3_IN;

### PDF page 1515
<a id="pdf-page-1515"></a>
Parameters
ucDummy
Dummy values. Any positive character value.
MMC_CANCELGENERALRPDO3_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_CANCELGENERALRPDO3_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and 4.6NC Profiler
Error IDs.
Figure 472 describes the function block for MMC_CancelGeneralRPDO3 as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 472: MMC_CancelGeneralRPDO3 function block
20.1.1.1 Function Block Code Example
Refer to the example in section 20.1.4.1.

### PDF page 1516
<a id="pdf-page-1516"></a>
###### 20.1.2 MMC_CancelGeneralRPDO4
20.1.2 MMC_CancelGeneralRPDO4
Cancels the general configuration of the DS-401 node or Maestro for RX at PDO4.
MMC_LIB_API int MMC_CancelGeneralRPDO4(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CANCELGENERALRPDO4_IN* pInParam,
OUT MMC_CANCELGENERALRPDO4_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_CANCELGENERALRPDO4 input data structure using the
MMC_CancelGeneralRPDO4 function.
pOutParam
Points to the MMC_CANCELGENERALRPDO4_OUT output structure receiving
information, as a result of calling the MMC_CancelGeneralRPDO4 function.
Remarks
Cancels communications to read/write PDO's sent from the Maestro or host.
Scope
All
MMC_CANCELGENERALRPDO4_IN Structure
typedef struct{
unsigned char ucDummy;
}MMC_CANCELGENERALRPDO4_IN;

### PDF page 1517
<a id="pdf-page-1517"></a>
Parameters
ucDummy
Dummy values. Any positive character value.
MMC_CANCELGENERALRPDO4_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_CANCELGENERALRPDO4_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and 4.6NC Profiler Error
IDs.
Figure 473 describes the function block for MMC_CancelGeneralRPDO4 as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 473: MMC_CancelGeneralRPDO4 function block
20.1.2.1 Function Block Code Example
MMC_CANCELGENERALRPDO3_IN Cancel3InParam;
MMC_CANCELGENERALRPDO3_OUT Cancel3OutParam;
MMC_CANCELGENERALRPDO4_IN Cancel4InParam;
MMC_CANCELGENERALRPDO4_OUT Cancel4OutParam;

rc = MMC_CancelGeneralRPDO3(ConnHndl,hAxisRef &
Cancel3InParam,&Cancel3OutParam);
if(rc != 0)
{
printf("MMC_CancelGeneralRPDO3 failed, error
%d\n",Cancel3OutParam.sErrorID);
}

rc =
MMC_CancelGeneralRPDO4(ConnHndl,hAxisRef,&Cancel4InParam,&Cancel4OutParam);
if(rc != 0)
{

### PDF page 1518
<a id="pdf-page-1518"></a>
printf("MMC_CancelGeneralRPDO4 failed, error
%d\n",Cancel4OutParam.sErrorID);
}

### PDF page 1519
<a id="pdf-page-1519"></a>
###### 20.1.3 MMC_CancelGeneralTPDO3
20.1.3 MMC_CancelGeneralTPDO3
Cancels the general configuration of the DS-401 node or Maestro for TX at PDO3.
MMC_LIB_API int MMC_CancelGeneralTPDO3(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CANCELGENERALTPDO3_IN* pInParam,
OUT MMC_CANCELGENERALTPDO3_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_CANCELGENERALTPDO3 input data structure using the
MMC_CancelGeneralTPDO3 function.
pOutParam
Points to the MMC_CANCELGENERALTPDO3_OUT output structure receiving
information, as a result of calling the MMC_CancelGeneralTPDO3 function.
Remarks
Cancels communications to read/write PDO's sent from the Maestro or host.
Scope
All
MMC_CANCELGENERALTPDO3_IN Structure
typedef struct{
unsigned char ucDummy;
}MMC_CANCELGENERALTPDO3_IN;

### PDF page 1520
<a id="pdf-page-1520"></a>
Parameters
ucDummy
Dummy values. Any positive character value.
MMC_CANCELGENERALTPDO3_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_CANCELGENERALTPDO3_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error
IDs.
Figure 474 describes the function block for MMC_CancelGeneralTPDO3 as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 474: MMC_CancelGeneralTPDO3 function block
20.1.3.1 Function Block Code Example
Refer to the example in section 20.1.4.1.

### PDF page 1521
<a id="pdf-page-1521"></a>
###### 20.1.4 MMC_CancelGeneralTPDO4
20.1.4 MMC_CancelGeneralTPDO4
Cancels the general configuration of the DS-401 node or Maestro for TX at PDO4.
MMC_LIB_API int MMC_CancelGeneralTPDO4(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CANCELGENERALTPDO4_IN* pInParam,
OUT MMC_CANCELGENERALTPDO4_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_CANCELGENERALTPDO4 input data structure using the
MMC_CancelGeneralTPDO4 function.
pOutParam
Points to the MMC_CANCELGENERALTPDO4_OUT output structure receiving
information, as a result of calling the MMC_CancelGeneralTPDO4 function.
Remarks
Cancels communications to read/write PDO's sent from the Maestro or host.
Scope
All
MMC_CANCELGENERALTPDO4_IN Structure
typedef struct{
unsigned char ucDummy;
}MMC_CANCELGENERALTPDO4_IN;

### PDF page 1522
<a id="pdf-page-1522"></a>
Parameters
ucDummy
Dummy values. Any positive character value.
MMC_CANCELGENERALTPDO4_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_CANCELGENERALTPDO4_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error
IDs.
Figure 475 describes the function block for MMC_CancelGeneralTPDO4 as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 475: MMC_CancelGeneralTPDO4 function block

### PDF page 1523
<a id="pdf-page-1523"></a>
20.1.4.1 Function Block Code Example
MMC_CANCELGENERALTPDO3_IN Cancel3InParam;
MMC_CANCELGENERALTPDO3_OUT Cancel3OutParam;
MMC_CANCELGENERALTPDO4_IN Cancel4InParam;
MMC_CANCELGENERALTPDO4_OUT Cancel4OutParam;

rc = MMC_CancelGeneralTPDO3(ConnHndl,hAxisRef &
Cancel3InParam,&Cancel3OutParam);
if(rc != 0)
{
printf("MMC_CancelGeneralTPDO3 failed, error
%d\n",Cancel3OutParam.sErrorID);
}

rc =
MMC_CancelGeneralTPDO4(ConnHndl,hAxisRef,&Cancel4InParam,&Cancel4OutParam);
if(rc != 0)
{
printf("MMC_CancelGeneralTPDO4 failed, error
%d\n",Cancel4OutParam.sErrorID);
}

### PDF page 1524
<a id="pdf-page-1524"></a>
###### 20.1.5 MMC_ConfigGeneralRPDO3
20.1.5 MMC_ConfigGeneralRPDO3
Generally configures the DS-401 node or Maestro for RX at PDO3.
MMC_LIB_API int MMC_ConfigGeneralRPDO3(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGGENERALRPDO3_IN* pInParam,
OUT MMC_CONFIGGENERALRPDO3_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
GMAS Programming(IEC 61331 Program)\ElmoSingleAxis
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle i s common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
hAxisRef
Axis/group reference handle type returned by the GetAxisRef command
pInParam
Points to the MMC_CONFIGGENERALRPDO3 input data structure using the
MMC_ConfigGeneralRPDO3 function.
pOutParam
Points to the MMC_CONFIGGENERALRPDO3_OUT output structure receiving
information, as a result of calling the MMC_ConfigGeneralRPDO3 function.
Remarks
Opens communications to read/write PDO's sent from the Maestro or host. Make sure to map the PDOs by
itself before using these APIs.
Scope
All
MMC_CONFIGGENERALRPDO3_IN Structure
typedef struct{
unsigned char ucEventType;
unsigned char ucPDOCommParam;

### PDF page 1525
<a id="pdf-page-1525"></a>
unsigned char ucPDOLength;
}MMC_CONFIGGENERALRPDO3_IN;
Parameters
ucEventType
Defines which group of events are to be transferred from the Maestro. Refer to the
section 19.3PDO Mapping the correct definition to be used. Any positive character
values are acceptable.
ucPDOCommParam
PDO communications parameter. Has the following positive character values:
PDO_COM_PARAM_SYNC 0x01
PDO_COM_PARAM_ASYNC 0xFF
ucPDOLength
Indicates the number of bytes to be sent as an RPDO, RPDO message. It can contain 1 -8
bytes of data.
MMC_CONFIGGENERALRPDO3_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_CONFIGGENERALRPDO3_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and 4.6NC Profiler Error
IDs.
Figure 476 describes the function block for MMC_ConfigGeneralRPDO3 as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 476: MMC_ConfigGeneralRPDO3 function block
20.1.5.1 Function Block Code Example
Refer to the example in section Function Block Code Example.

### PDF page 1526
<a id="pdf-page-1526"></a>
###### 20.1.6 MMC_ConfigGeneralRPDO4
20.1.6 MMC_ConfigGeneralRPDO4
Generally configures the DS-401 node or Maestro for RX at PDO4.
MMC_LIB_API int MMC_ConfigGeneralRPDO4(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGGENERALRPDO4_IN* pInParam,
OUT MMC_CONFIGGENERALRPDO4_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_CONFIGGENERALRPDO4 input data structure using the
MMC_ConfigGeneralRPDO4 function.
pOutParam
Points to the MMC_CONFIGGENERALRPDO4_OUT output structure receiving
information, as a result of calling the MMC_ConfigGeneralRPDO4 function .
Remarks
Opens communications to read/write PDO's sent from the Maestro or host. Make sure to map the PDOs by
itself before using these APIs.
Scope
All
MMC_CONFIGGENERALRPDO4_IN Structure
typedef struct{
unsigned char ucEventType;
unsigned char ucPDOCommParam;

### PDF page 1527
<a id="pdf-page-1527"></a>
unsigned char ucPDOLength;
}MMC_CONFIGGENERALRPDO4_IN;
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
Indicates the number of bytes to be sent as an RPDO, RPDO message. It can contain 1-8
bytes of data.
MMC_CONFIGGENERALRPDO4_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_CONFIGGENERALRPDO4_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and 4.6NC Profiler Error
IDs.
Figure 477 describes the function block for MMC_ConfigGeneralRPDO4
[PDF field-code object omitted]
Figure 477: MMC_ConfigGeneralRPDO4 function block

### PDF page 1528
<a id="pdf-page-1528"></a>
20.1.6.1 Function Block Code Example
MMC_CONFIGGENERALRPDO3_IN ConfigRPDO3InParam;
MMC_CONFIGGENERALRPDO3_OUT ConfigRPDO3OutParam;
MMC_CONFIGGENERALRPDO4_IN ConfigRPDO4InParam;
MMC_CONFIGGENERALRPDO4_OUT ConfigRPDO4OutParam;

ConfigRPDO3InParam.ucEventType = 16;
ConfigRPDO4InParam.ucEventType = 17;
ConfigRPDO3InParam.ucPDOCommParam = 0x01;
ConfigRPDO4InParam.ucPDOCommParam = 0x01;
ConfigRPDO3InParam.ucPDOLength = 2;
ConfigRPDO4InParam.ucPDOLength = 2;

rc = MMC_ConfigGeneralRPDO3(ConnHndl,hAxisRef,&
ConfigRPDO3InParam,&ConfigRPDO3OutParam);
if(rc != 0)
{
printf("MMC_ConfigGeneralRPDO3 failed, error
%d\n",ConfigRPDOOutParam.sErrorID);
}

rc =
MMC_ConfigGeneralRPDO4(ConnHndl,hAxisRef,&ConfigRPDO4InParam,&ConfigRPDO4Ou
tParam);
if(rc != 0)
{
printf("MMC_ConfigGeneralRPDO4 failed, error
%d\n",ConfigRPDO4OutParam.sErrorID);
}

### PDF page 1529
<a id="pdf-page-1529"></a>
###### 20.1.7 MMC_ConfigGeneralTPDO3
20.1.7 MMC_ConfigGeneralTPDO3
Generally configures the DS-401 node or Maestro for TX at PDO3.
MMC_LIB_API int MMC_ConfigGeneralTPDO3(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGGENERALTPDO3_IN* pInParam,
OUT MMC_CONFIGGENERALTPDO3_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_CONFIGGENERALTPDO3_IN input data structure using the
MMC_ConfigGeneralTPDO3 function.
pOutParam
Points to the MMC_CONFIGGENERALTPDO3_OUT output structure receiving
information, as a result of calling the MMC_ConfigGeneralTPDO3 function .
Remarks
Opens communications to read/write PDO's sent from the Maestro or host. Make sure to map the PDOs by
itself before using these APIs.
Scope
All
MMC_CONFIGGENERALTPDO3_IN Struzcture
typedef struct{
unsigned char ucEventType;
}MMC_CONFIGGENERALTPDO3_IN;

### PDF page 1530
<a id="pdf-page-1530"></a>
Parameters
ucEventType
Defines which group of events are to be transferred from the Maestro. Refer to the
section PDO Mapping the correct definition to be used. Any positive character values
are acceptable.
MMC_CONFIGGENERALTPDO3_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_CONFIGGENERALTPDO3_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and 4.6 NC Profiler
Error IDs.
Figure 478 describes the function block for MMC_ConfigGeneralTPDO3 as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 478: MMC_ConfigGeneralTPDO3 function block
20.1.7.1 Function Block Code Example
Refer to the example in section 20.1.8.1.

### PDF page 1531
<a id="pdf-page-1531"></a>
###### 20.1.8 MMC_ConfigGeneralTPDO4
20.1.8 MMC_ConfigGeneralTPDO4
Generally configures the DS-401 node or Maestro for TX at PDO4.
MMC_LIB_API int MMC_ConfigGeneralTPDO4(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGGENERALTPDO4_IN* pInParam,
OUT MMC_CONFIGGENERALTPDO4_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_CONFIGGENERALTPDO4 input data structure using the
MMC_ConfigGeneralTPDO4 function.
pOutParam
Points to the MMC_CONFIGGENERALTPDO4_OUT output structure receiving
information, as a result of calling the MMC_ConfigGeneralTPDO4 function.
Remarks
Opens communications to read/write PDO's sent from the Maestro or host. Make sure to map the PDOs by
itself before using these APIs.
Scope
All
MMC_CONFIGGENERALTPDO4_IN Structure
typedef struct{
unsigned char ucEventType;
}MMC_CONFIGGENERALTPDO4_IN;

### PDF page 1532
<a id="pdf-page-1532"></a>
Parameters
ucEventType
Defines which group of events are to be transferred from the Maestro. Refer to the
section 19.3PDO Mapping the correct definition to be used. Any positive character
values are acceptable.
MMC_CONFIGGENERALTPDO4_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_CONFIGGENERALTPDO4_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and 4.6NC Profiler Error
IDs.
Figure 479 describes the function block for MMC_ConfigGeneralTPDO4 as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 479: MMC_ConfigGeneralTPDO4 function block
20.1.8.1 Function Block Code Example
MMC_CONFIGGENERALTPDO3_IN ConfigTPDO3InParam;
MMC_CONFIGGENERALTPDO3_OUT ConfigTPDO3OutParam;
MMC_CONFIGGENERALTPDO4_IN ConfigTPDO4InParam;
MMC_CONFIGGENERALTPDO4_OUT ConfigTPDO4OutParam;

ConfigTPDO3OutParam.ucEventType = 16;
ConfigTPDO4InParam.ucEventType = 17;
rc = MMC_ConfigGeneralTPDO3(ConnHndl,hAxisRef,&
ConfigTPDO3InParam,&ConfigTPDO3OutParam);
if(rc != 0)
{
printf("MMC_ConfigGeneralTPDO3 failed, error
%d\n",ConfigTPDO3OutParam.sErrorID);
}

### PDF page 1533
<a id="pdf-page-1533"></a>
rc =
MMC_ConfigGeneralTPDO4(ConnHndl,hAxisRef,&ConfigTPDO4InParam,&ConfigTPDO4Ou
tParam);
if(rc != 0)
{
printf("MMC_ConfigGeneralTPDO4 failed, error
%d\n",ConfigTPDO4OutParam.sErrorID);
}

### PDF page 1534
<a id="pdf-page-1534"></a>
###### 20.1.9 MMC_DisableDS401DIChangedEvent
20.1.9 MMC_DisableDS401DIChangedEvent
Disables a DS401 digital input event change against an I/O module.
MMC_LIB_API int MMC_DisableDS401DIChangedEvent(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_DISABLEDICHANGEDEVENT_IN* pInParam,
OUT MMC_DISABLEDICHANGEDEVENT_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_DISABLEDICHANGEDEVENT input data structure using the
MMC_DisableDS401DIChangedEvent function.
pOutParam
Points to the MMC_DISABLEDICHANGEDEVENT_OUT output structure receiving
information, as a result of calling the MMC_DisableDS401DIChangedEvent function.
Remarks
When disabled, prevents any DS401 digital input event change being sent from the I/O module to the
Maestro and then host server (if connected).
Scope
All
MMC_DISABLEDICHANGEDEVENT_IN Structure
typedef struct{
unsigned char ucDummy;
}MMC_DISABLEDICHANGEDEVENT_IN;

### PDF page 1535
<a id="pdf-page-1535"></a>
Parameters
ucDummy
Dummy data input. Any positive character value.
MMC_DISABLEDICHANGEDEVENT_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_DISABLEDICHANGEDEVENT_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and NC Profiler Error
IDs.
Figure 480 describes the function block for MMC_DisableDS401DIChangedEvent as applied within the IEC
61131 programming.
[PDF field-code object omitted]
Figure 480: MMC_DisableDS401DIChangedEvent function block

### PDF page 1536
<a id="pdf-page-1536"></a>
20.1.9.1 Function Block Code Example
int rc;
MMC_DISABLEDICHANGEDEVENT_IN stDisableDIChangeEv_in;
MMC_DISABLEDICHANGEDEVENT_OUT stDisableDIChangeEv_out;
//
// Inserting the structure parameters:
stDisableDIChangeEv_in.ucDummy = 1; //Dummy data input
//
rc = MMC_DisableDS401DIChangedEvent (hConn, iAxisRef,
&stDisableDIChangeEv_in, &stDisableDIChangeEv_out);
if (rc != 0)
{
HandleError();
}

### PDF page 1537
<a id="pdf-page-1537"></a>
###### 20.1.10 MMC_EnableDS401DIChangedEvent
20.1.10 MMC_EnableDS401DIChangedEvent
Enables an DS401 digital input event change.
MMC_LIB_API int MMC_EnableDS401DIChangedEvent (
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_ENABLEDICHANGEDEVENT_IN* pInParam,
OUT MMC_ENABLEDICHANGEDEVENT_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_ENABLEDICHANGEDEVENT input data structure using the
MMC_EnableDS401DIChangedEvent function.
pOutParam
Points to the MMC_ENABLEDICHANGEDEVENT_OUT output structure receiving
information, as a result of calling the MMC_EnableDS401DIChange dEvent function.
Remarks
When enabled, any DS401 digital input event change is sent from the I/O module to the Maestro and then
host server (if connected).
Scope
All
MMC_ENABLEDICHANGEDEVENT_IN Structure
typedef struct{
unsigned char ucDummy;
}MMC_ENABLEDICHANGEDEVENT_IN;

### PDF page 1538
<a id="pdf-page-1538"></a>
Parameters
ucDummy
Dummy data input. Any positive character value.
MMC_ENABLEDICHANGEDEVENT_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_ENABLEDICHANGEDEVENT_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error
IDs.
Figure 481 describes the function block for MMC_EnableDS401DIChangedEvent as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 481: MMC_EnableDS401DIChangedEvent function block

### PDF page 1539
<a id="pdf-page-1539"></a>
20.1.10.1 Function Block Code Example
int rc;
MMC_ENABLEDICHANGEDEVENT_IN stEnableDIChangeEv_in;
MMC_ENABLEDICHANGEDEVENT_OUT stEnableDIChangeEv_out;
//
// Inserting the structure parameters:
stEnableDIChangeEv_in.ucDummy = 1; //Dummy data input
//
rc = MMC_EnableDS401DIChangedEvent (hConn, iAxisRef,
&stEnableDIChangeEv_in, &stEnableDIChangeEv_out);
if (rc != 0)
{
HandleError();
}

### PDF page 1540
<a id="pdf-page-1540"></a>
###### 20.1.11 MMC_ReadDS401DIGroup
20.1.11 MMC_ReadDS401DIGroup
Reads the DS-401 digital inputs of a group of 8 digital I/Os.
MMC_LIB_API int MMC_ReadDS401DIGroup(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READDIGROUP_IN* pInParam,
OUT MMC_READDIGROUP_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_READDIGROUP input data structure using the MMC_ReadDIGroup
function.
pOutParam
Points to the MMC_READDIGROUP_OUT output structure receiving information, as a
result of calling the MMC_ReadDIGroup function.
Remarks
A group consists of 8 I/O connections with a possible maximum of 8 groups and therefore 64 I/O
connections.
Scope
All
MMC_READDIGROUP_IN Structure
typedef struct{
unsigned char ucGroupIndex;
}MMC_READDIGROUP_IN;
Parameters

### PDF page 1541
<a id="pdf-page-1541"></a>
ucGroupIndex
Group index of 8 I/O's up to a max of 64 I/O's. positive Integer (character) values of
[9 onwards]
Note that It is possible to write to the lower 8 groups (64 bits) using the function
MMC_WriteDS401DOutput that writes to the first 64 bits (long long variable) if they are
valid. MMC_WriteDS401DOGroup only writes to the upper bits.
MMC_READDIGROUP_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_READDIGROUP_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error
IDs.

### PDF page 1542
<a id="pdf-page-1542"></a>
Figure 482 describes the function block for MMC_Read DS401DIGroup
[PDF field-code object omitted]
Figure 482: MMC_ReadDS401DIGroup function block
20.1.11.1 Function Block Code Example
int rc;
MMC_READDIGROUP_IN stReadDIGroup_in;
MMC_READDIGROUP_OUT stReadDIGroup_out;
//
// Inserting the structure parameters:
stReadDIGroup_in.ucGroupIndex = 21; //Group index
//
rc = MMC_ReadDS401DIGroup (hConn, iAxisRef, &stReadDIGroup_in,
&stReadDIGroup_out);
printf("ErrId[%d]\n", (short)stReadDIGroup_out.sErrorID);
if (rc != 0)
{
HandleError();
}

### PDF page 1543
<a id="pdf-page-1543"></a>
###### 20.1.12 MMC_ReadDS401DInput
20.1.12 MMC_ReadDS401DInput
Reads the DS-401 digital input of all 64 bit I/O's in one action, increasing the communication speed
proportionately versus reading 8 x groups of 8 I/O's.
MMC_LIB_API int MMC_ReadDS401DInput(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READDI_IN* pInParam,
OUT MMC_READDI_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_READDI input data structure using the MMC_ReadDS401DInput
function.
pOutParam
Points to the MMC_READDI_OUT output structure receiving information, as a result of
calling the MMC_ReadDS401DInput function.
Remarks
None
Scope
All
MMC_READDI_IN Structure
typedef struct{
unsigned char dummy;
}MMC_READDI_IN;

### PDF page 1544
<a id="pdf-page-1544"></a>
Parameters
dummy
Dummy data input. Any positive character value.
MMC_READDI_OUT Structure
typedef struct{
#ifdef WIN32
unsigned __int64 ulliDI;
#else
unsigned long long int ulliDI;
#endif
unsigned short usStatus;
short sErrorID;
}MMC_READDI_OUT;
Parameters
__int64 ulliDI or ulliDI
If function is defined for WIN32 then use __int64 ulliDI, else use ulliDI. Any positive,
negative (Win32) or positive 64bit (8 bytes) character and/or integer.
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error
IDs.
Figure 483 describes the function block for MMC_ReadDS401DInput as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 483: MMC_ReadDS401DInput function block
20.1.12.1 Function Block Code Example
int rc;
MMC_READDI_IN stReadDI_in;
MMC_READDI_OUT stReadDI_out;
//
// Inserting the structure parameters:
stReadDI_in.dummy = 1; //dummy input
//
rc = MMC_ReadDS401DInput (hConn, iAxisRef, &stReadDI_in, &stReadDI_out);

### PDF page 1545
<a id="pdf-page-1545"></a>
printf("DS-401 Input Status[%ld] ErrId[%d]\n", (long
int)stReadDI_out.ulliDI, (short)stReadDI_out.sErrorID);
if (rc != 0)
{
HandleError();
}

### PDF page 1546
<a id="pdf-page-1546"></a>
###### 20.1.13 MMC_WriteDS401DOGroup
20.1.13 MMC_WriteDS401DOGroup
Writes the DS-401 digital outputs of a group of 8 I/O's to the Maestro.
MMC_LIB_API int MMC_WriteDS401DOGroup(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEDOGROUP_IN* pInParam,
OUT MMC_WRITEDOGROUP_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_WRITEDOGROUP input data structure using the
MMC_WriteDS401DOGroup function.
pOutParam
Points to the MMC_WRITEDOGROUP_OUT output structure receiving information, as a
result of calling the MMC_WriteDS401DOGroup function.
Remarks
A group consists of 8 I/O connections with a possible maximum of 8 groups an d therefore 64 I/O
connections.
Scope
All
MMC_WRITEDOGROUP_IN Structure
typedef struct{
unsigned char ucGroupIndex;
unsigned char ucVal;

### PDF page 1547
<a id="pdf-page-1547"></a>
}MMC_WRITEDOGROUP_IN;
Parameters
ucGroupIndex
Group index of 8 I/O's up to a max of 64 I/O's. positive Integer (character) values of
[9 onwards]
Note that It is possible to write to the lower 8 groups (64 bits) using the function
MMC_WriteDS401DOutput that writes to the first 64 bits (long long variable) if they are
valid. MMC_WriteDS401DOGroup only writes to the upper bits.
ucVal
Digital output value of the 0 - 8 bit data in a group, ranging from 0 - 255. Any positive
character value.
MMC_WRITEDOGROUP_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_WRITEDOGROUP_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error
IDs.
Figure 484 describes the function block for MMC_WriteDS401DOGroup as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 484: MMC_WriteDS401DOGroup function block
20.1.13.1 Function Block Code Example
int rc;
MMC_WRITEDOGROUP_IN stWriteDOGroup_in;
MMC_WRITEDOGROUP_OUT stWriteDOGroup_out;
//
// Inserting the structure parameters:
stWriteDOGroup_in.ucGroupIndex = 10; //Index of the group axes
stWriteDOGroup_in.ucVal = 9; //Digital output value

### PDF page 1548
<a id="pdf-page-1548"></a>
//
rc = MMC_WriteDS401DOGroup (hConn, iAxisRef, &stWriteDOGroup_in,
&stWriteDOGroup_out);
if (rc != 0)
{
HandleError();
}

### PDF page 1549
<a id="pdf-page-1549"></a>
###### 20.1.14 MMC_WriteDS401DOutput
20.1.14 MMC_WriteDS401DOutput
Writes to all of the DS-401 digital outputs assigned to TPDO1 at once, up to 64 bit I/O's in one action,
increasing the communication speed proportionately versus writing to 8 x groups of 8 I/O's.
MMC_LIB_API int MMC_WriteDS401DOutput(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEDO_IN* pInParam,
OUT MMC_WRITEDO_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_DS401_API.h
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
Points to the MMC_WRITEDO input data structure using the
MMC_WriteDS401DOutput function.
pOutParam
Points to the MMC_WRITEDO_OUT output structure receiving information, as a result
of calling the MMC_WriteDS401DOutput function.
Remarks
None
Scope
All
MMC_WRITEDO_IN Structure
typedef struct{
#ifdef WIN32
unsigned __int64 ulliDO;

### PDF page 1550
<a id="pdf-page-1550"></a>
#else
unsigned long long int ulliDO;
#endif
}MMC_WRITEDO_IN;
Parameters
__int64 ulliDO or ulliDO
If function is defined for WIN32 then use __int64 ulliDO, else use ulliDO. Any positive,
negative (Win32) or positive 64bit (8 bytes) character and/or integer.
MMC_WRITEDO_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_WRITEDO_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.

### PDF page 1551
<a id="pdf-page-1551"></a>
Figure 485 describes the function block for MMC_WriteDS401DOutput as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 485: MMC_WriteDS401DOutput function block
20.1.14.1 Function Block Code Example
int rc;
MMC_WRITEDO_IN stWriteDO_in;
MMC_WRITEDO_OUT stWriteDO_out;
//
// Inserting the structure parameters:
stWriteDO_in.ulliDO = 1; //Value to write to digital outputs
//
rc = MMC_WriteDS401DOutput (hConn, iAxisRef, &stWriteDO_in,
&stWriteDO_out);
if (rc != 0)
{
HandleError();
}
