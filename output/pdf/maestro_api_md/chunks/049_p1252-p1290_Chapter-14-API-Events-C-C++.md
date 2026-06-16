# Chapter 14 API Events (C & C++)

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 1252-1290
- Chunk: `049_p1252-p1290_Chapter-14-API-Events-C-C++.md`

## Active Outline At Chunk Start
- p. 1252 - Chapter 14 API Events (C & C++)

## Contained Bookmark Outline
- p. 1252 - Chapter 14 API Events (C & C++)
  - p. 1253 - 14.1 Communication Byte Order
  - p. 1253 - 14.2 Communication ASYNC Replies (Events) From Drives
  - p. 1255 - 14.3 Download Firmware Notifications
  - p. 1255 - 14.4 Emergency Event (C & C++)
    - p. 1256 - 14.4.1 EmergencyEvent_Received (C++)
  - p. 1256 - 14.5 Motion Ended Event
  - p. 1256 - 14.6 HeartBeat Event
  - p. 1257 - 14.7 PDO Receive Event
    - p. 1258 - 14.7.1 Event Group equals to 5 or 6
    - p. 1258 - 14.7.2 Event Group equals to 11
    - p. 1258 - 14.7.3 Event Group Equals to 16 or 17
    - p. 1259 - 14.7.4 Event Group equals to 1 - 15 besides 5, 6, 11, 16 and 17
  - p. 1259 - 14.8 Home Ended Event (C & C++)
    - p. 1259 - 14.8.1 HomeEnded_Received (C++)
  - p. 1260 - 14.9 Modbus Write Event
  - p. 1260 - 14.10 Touch Probe Ended Event
  - p. 1260 - 14.11 Node Connected Event
  - p. 1261 - 14.12 Node Initialization Completed Event
    - p. 1261 - 14.12.1 NodeInitEvent_Received (C++)
  - p. 1262 - 14.13 Node Error Event (C & C++)
    - p. 1263 - 14.13.1 NodeErrorEvent_Received (C++)
  - p. 1264 - 14.14 Stop On Limit Event (C & C++)
    - p. 1265 - 14.14.1 StopOnLimit_Received (C++)
  - p. 1267 - 14.15 Table Underflow Event
  - p. 1267 - 14.16 Global Async Reply Event
    - p. 1268 - 14.16.1 GlobalAsyncReply_Received (C++)
  - p. 1269 - 14.17 Notification Function Block Event (C & C++)
    - p. 1270 - 14.17.1 FBNotifyEvent_Received (C++)
  - p. 1271 - 14.18 Policy Ended Event (C & C++)
    - p. 1272 - 14.18.1 PolicyEndedEvent_Received (C++)
  - p. 1273 - 14.19 Communication Event Mechanism
  - p. 1274 - 14.20 Events Mask and Enumeration
  - p. 1275 - 14.21 Asynchronous Events Callback (C & C++)
    - p. 1275 - 14.21.1 Callback Prototype
    - p. 1275 - 14.21.2 Data Structure
    - p. 1275 - 14.21.3 Event Extraction Example
    - p. 1280 - 14.21.4 Net To local Conversion
    - p. 1281 - 14.21.5 AsyncReplyEvent_Received (C++)
  - p. 1284 - 14.22 Notification and Event Function Blocks in C
    - p. 1285 - 14.22.1 MMC_InsertNotificationFb
    - p. 1288 - 14.22.2 MMC_ClearEventsMask

## Extracted Text

### PDF page 1252
<a id="pdf-page-1252"></a>
#### Chapter 14 API Events (C & C++)
Chapter 14 API Events (C & C++)
Event handling in the Maestro is the ability to capture specific events occurring within the Maestro, and send
Asynchronous Events Callback messages to a host, thus mirroring the occurrences of the event.
Note: Wherever the Status is produced as a result of the event, it may be set to zero (0) or non -zero.
If zero, then the event indicates a successful operation
If non-zero, then the event indicates an error whose specific error code can be acquired from the
ErrorID.
When this specific error code is sent with the callback message, refer to the Chapter 16 Saving
Maestro User Program Parameters for details of the error codes.
The mechanism to handle events in the Maestro involves the following:
- Communication, async replies from the drive, e.g. the function block MMC_SendSDO
- Process progress, notifies the host regarding the progress of a long ongoing process, such as Download
Firmware
- Errors in the drive notifications, per node
- PDO3 and PDO4 receive, per node
- System Errors - General system failures (Not yet implemented)
- On Motion End, per node
- On Heartbeat Error, per node
- Emergencies, per node
- Modbus writes from hosts
- Touch Probe event received
- Node Connection Event
- Node Errors
- Axis stopped due to limit
- PVT Underflow data warning
- CAN Node returning to network
- CAN ASYNC reply from drive is ready to be read
This chapter describes the situations when such an event is triggered, the data structure and format of each
event. It also includes the description of event received functions in C++.
The treatment of an event is per connection, in the open UDP port. The UDP port is automatically opened by
the Maestro function when the MMC_InitConnection function is called and MMC_Op enUdpChannel is invoked.
If the MMC_InitConnection and MMC_OpenUdpChannel functions were called with a callback function that is a
valid pointer to a callback function, a UDP listening port is automatically created. This UDP port listens for
incoming messages, actually on a thread. Once a message is received, the registered callback function is called.
After the MMC_InitConnection function is called, the GMAS_OpenUDPResponceChannel function is called. This
function sends the previously opened appropriate port, the command MMC_InitConnection, to the Maestro. In
addition to the port, a 32-bit variable is sent, stating the event types that are to be registered in the Maestro.

### PDF page 1253
<a id="pdf-page-1253"></a>
##### 14.1 Communication Byte Order
##### 14.2 Communication ASYNC Replies (Events) From Drives
31 30 29 28 27 26 25 24 23 22 21 20 19 18 17 16 15 14 13 12 11 10 9 8 7 6 5 4 3 2 1 0
TABLE_UNDERFLOW_EVT
STOP_ON_LIMIT_EVT
NODE_ERROR_EVT
TOUCH_PROBE_ENDED_EVT
MODBUS_WRITE_EVT
SYSTEMERROR_EVT
HOME_ENDED_EVT
EMIT_EVT
DRVERROR_EVT
PDORCV_EVT
HBEAT_EVT
MOTIONENDED_EVT
EMCY_EVT
DOWNLOAD_FW_EVT
ASYNC_REPLY_EVT

Figure 398: 32-bit variable
The user sends the appropriate events he wishes to receive as a callback event. There are situations when
stating the event is insufficient, and additional function calls must be made. This will be discussed per event
type.
14.1 Communication Byte Order
The user must recognize the value of the endianness (little/big endian) of the system on which his program is
executed. The message structure of asynchronous call-back buffer, as described hereby refers to data location
(offset) of different types. The user may have to convert the endianness, if his program runs on a Windows
system.
14.2 Communication ASYNC Replies (Events) From Drives
This event is received if an error (or timeout) occurred when calling an SDO download or Drive Command via
the binary interpreter mechanism. Please note that in this situation, unlike other events, the The axis
reference, is taken from offset 14 (instead of 12).
The UDP Data received is as follows:
No. Constant
Definition
Data Comment
Name Type Offset Length (bytes)
0 ASYNC_REPLY_EVT Event No. Unsigned short 0 2
Axis Ref Unsigned short 14 2
Status Unsigned short 8 2
Error ID Unsigned short 10 2
COB ID Unsigned short 12 2
Length Unsigned char 16 1 Length in bytes
Data Unsigned char 17 According to
Length

Async Event Type Unsigned char 25 1
When an asynchronous operation is successful, and this is a response for an inquiry oper ation (similar to the
binary interpreter command "get" which sends a raw data operation or SDO upload request) the status and
error ID fields will hold the value of zero, and also data is returned in the data field.
When the operation fails, the status field holds value other than zero and the error code field states a specific
error code which defines the occurrence during the operation. If the error occurred during an SDO upload or
download, the data field will hold the full CAN message returned from the node.

### PDF page 1254
<a id="pdf-page-1254"></a>
The Async Event Type field holds values from the Async events numerators group found in the
MMC_events_API.h file. The result of these asynchronous operational events are used to indicate the
following:
Response Response Type
Always Send Response SendSDO operations (upload \ download)
Binary interpreter operation Command Get
Read DI Group
Reset node operation
Motion mode change
PDO3\4 operations (mapping, canceling or choosing communication
parameter):
MMC_CfgRegParamEvPDO3Cmd, MMC_CfgRegParamEvPDO4Cmd,
MMC_CancelParamEvPDO3Cmd, MMC_CfgUserParamEvPDO3Cmd,
MMC_CancelParamEvPDO4Cmd, MMC_CfgUserParamEvPDO4Cmd,
MMC_ChangeDefaultPDOConfiguration
Virtual encoder configuration
Response is sent only on failure OS Command
Binary interpreter operation Command Set
Write DO Group
Bulk upload
Response is only sent on success
(simulates timeout on failure)
Send raw data

### PDF page 1255
<a id="pdf-page-1255"></a>
##### 14.3 Download Firmware Notifications
##### 14.4 Emergency Event (C & C++)
14.3 Download Firmware Notifications
This event is received to update the progress of an ongoing download firmware procedure. Pleas e contact to
Elmo's representative for further support.
14.4 Emergency Event (C & C++)
Emergency event is triggered by the occurrence of a CANopen device internal error situation.
The UDP Data received is as follows:
No. Constant
Definition
Data Comment
Name Type Offset Length (bytes)
5 EMCY_EVT Event No. Unsigned short 0 2
Axis Ref Unsigned short 12 2
Emergency
Error Code
Unsigned short 14 2 As per CiA
DS301
(chapter
9.2.5.3)
MSEF Unsigned char 16 5 Manufacturer-
specific error
code
ErrReg Unsigned char 15 1 Error register

### PDF page 1256
<a id="pdf-page-1256"></a>
###### 14.4.1 EmergencyEvent_Received (C++)
##### 14.5 Motion Ended Event
##### 14.6 HeartBeat Event
14.4.1 EmergencyEvent_Received (C++)
As above but for C++
EmergencyEvent_Received(
unsigned short usAxisRef,
short sEmcyCode,
char cErrReg...
)
Source GMAS\includes\CPP\MMCPPGlobal.h
.NET Definition
Parameters
usAxisRef
The axis reference
sEmcyCode
According to CiA DS301 standard emergency codes. For example:
0x3000 = Voltage generic error.
cErrReg
Refer to the section Maestro Emergency Error IDs Originating
from the Gold Servo Drive for details of the errors IDs produced with their explanation.
14.5 Motion Ended Event
Motion ended event is triggered by Maestro when an axis (single as well as group axis) motion is ended. The
user must register for this event in order to receive event notification and in addition he must call
MMC_EnableMotionEndedEventCmd and specify the particular axis from which he wishes to receive Motion
Ended Events. The UDP Data received is as follows:
No. Constant
Definition
Data Comment
Name Type Offset Length (bytes)
6 MOTIONENDED_EVT Event No. Unsigned short 0 2
Axis Ref Unsigned short 12 2
Error ID short 10 2
14.6 HeartBeat Event
Heart Beat error event is triggered by Maestro when no heartbeat event was received by Maestro from one of
the devices.
The UDP Data received is as follows:
No. Constant Data Comment

### PDF page 1257
<a id="pdf-page-1257"></a>
##### 14.7 PDO Receive Event
Definition Name Type Offset Length (bytes)
7 HBEAT_EVT Event No. Unsigned short 0 2
Axis Ref Unsigned short 12 2
14.7 PDO Receive Event
This event is triggered by the Maestro when a PDO (3 or 4) for a specific axis is received, and was configured to
be sent as notification to the user. Note that the term of the variable located at offset 14 (see below) refers to
the Event Group /PDO Number when handling DS 402/401 respectively. The definition of User Data 1 and User
Data 2 is Data which is a 32bit float for event groups 15,16. When using MMC_ConfigEventModePDO3\4 with
cyclic event mode, the notification is sent on the next Maestro cycle and after it was endian swapped. W hen
using immediate event mode, the data is sent immediately but an endian swap won't be made by the Maestro.
The UDP Data received is as follows:
No. Constant
Definition
Data Comment
Name Type Offset Length
(bytes)
8 PDORCV_EVT Event No. Unsigned
short
0 2
Axis Ref Unsigned
short
12 2
Event Group
(PDO No. if
referring to DS-
401) with possible
values of 1-17
(Refer to next
subsections)
Unsigned char 14 2 Event Group or PDO
No. is assigned by the
values 1 - 17 and the
remaining UDP data is
arranged according to
the value

### PDF page 1258
<a id="pdf-page-1258"></a>
###### 14.7.1 Event Group equals to 5 or 6
###### 14.7.2 Event Group equals to 11
###### 14.7.3 Event Group Equals to 16 or 17
14.7.1 Event Group equals to 5 or 6
The UDP Data received is as follows:
No. Constant
Definition
Data Comment
Name Type Offset Length
(bytes)
8 PDORCV_EVT Event No. Unsigned short 0 2
Axis Ref Unsigned short 12 2
Event Group Unsigned char 14 2 = 5 or 6
User Data 1 short 15 2 Result must be
multiplied by rated
current.
User Data 2 long 17 4
14.7.2 Event Group equals to 11
The UDP Data received is as follows:
No. Constant
Definition
Data Comment
Name Type Offset Length (bytes)
8 PDORCV_EVT Event No. Unsigned short 0 2
Axis Ref Unsigned short 12 2
Event Group Unsigned char 14 2 = 11
User Data Long 15 4
14.7.3 Event Group Equals to 16 or 17
The UDP Data received is as follows:
No. Constant
Definition
Data Comment
Name Type Offset Length (bytes)
8 PDORCV_EVT Event No. Unsigned short 0 2
Axis Ref Unsigned short 12 2
Event Group Unsigned char 14 2 = 16 or 17
User Data Long long 15 8

### PDF page 1259
<a id="pdf-page-1259"></a>
###### 14.7.4 Event Group equals to 1 - 15 besides 5, 6, 11, 16 and 17
##### 14.8 Home Ended Event (C & C++)
###### 14.8.1 HomeEnded_Received (C++)
14.7.4 Event Group equals to 1 - 15 besides 5, 6, 11, 16 and 17
The UDP Data received is as follows:
No. Constant
Definition
Data Comment
Name Type Offset Length (bytes)
8 PDORCV_EVT Event No. Unsigned short 0 2
Axis Ref Unsigned short 12 2
Event Group Unsigned char 14 2 = 1-15 excluding
5,6,11
User Data 1 Long 15 4 Data is 32bit float
for group 15,16.
User Data 2 long 19 4
14.8 Home Ended Event (C & C++)
This event is triggered by the Maestro when a Single axis completes the homing procedure. The UDP Data
received is as follows:
No. Constant
Definition
Data Comment
Name Type Offset Length
(bytes)
11 HOME_ENDED_EVT Event No. Unsigned short 0 2
Axis Ref Unsigned short 12 2
14.8.1 HomeEnded_Received (C++)
As above but for C++
HomeEnded_Received(
unsigned short usAxisRef,
short sErrCode
)
Source GMAS\includes\CPP\MMCPPGlobal.h
.NET Definition
Parameters
usAxisRef
The axis reference
sErrCode
0 = Homing procedure ended successfully
- 51 = Homing procedure failed

### PDF page 1260
<a id="pdf-page-1260"></a>
##### 14.9 Modbus Write Event
##### 14.10 Touch Probe Ended Event
##### 14.11 Node Connected Event
14.9 Modbus Write Event
Modbus write event is triggered by the Maestro when the user writes to the Modbus Holding registers. The
UDP Data received is as follows:
No. Constant Definition Data Comment
Name Type Offset Length (bytes)
13 MODBUS_WRITE_EVT Event No. Unsigned short 0 2
14.10 Touch Probe Ended Event
Touch Probe event is triggered by Maestro when a touch probe position is receiv ed from one of the drives on
the network. This is relevant to Ethercat drives only, and drives must be configured appropriately. The UDP
Data received is as follows:
No. Constant Definition Data Comment
Name Type Offset Length (bytes)
14 TOUCH_PROBE_ENDED_EVT

Event No. Unsigned
short

0 2
Axis Ref Unsigned
short
12 2
Touch
probe
Position
long 14 2
14.11 Node Connected Event
Node connected event is triggered by Maestro when an axis is re -connected to the network. This event will
occur in two possible situations:
- A node sent a NMT boot-up message after power on
- A node sent a heartbeat message after it was in "heartbeat error state" - a heartbeat error event
occurred prior to this event.
In both cases node will be initialized by Maestro. When a node reconnects (second scenario) it will enter error
state.The UDP Data received is as follows:
No. Constant Definition Data Comment
Name Type Offset Length (bytes)
18 NODE_CONNECTED_EVT

Event No. Unsigned short 0 2
Axis Ref Unsigned short 12 2

### PDF page 1261
<a id="pdf-page-1261"></a>
##### 14.12 Node Initialization Completed Event
###### 14.12.1 NodeInitEvent_Received (C++)
14.12 Node Initialization Completed Event
The Node initialization completed event indicates whether a successful or unsuccessful node initialization has
occurred. This event supplies the user the ending state of the initialization and if an error occurs, an error
number is indicated.
The Node initialization is performed only after a node sends boot - up message (Refer to the DS301
documentation for further details), but not after every node connection on the bus. A node connected event
will always take place before this event, but a node initialization completed event will not always occur after
node connected event. The UDP Data received is as follows:
No. Constant Definition Data Comment
Name Type Offset Length
(bytes)
20 NODE_INIT_FINISHED_EVT Event No. Unsigned
short
0 2
Axis Ref Unsigned
short
12 2
Error id short 10 0 - indicates successful
initialization
14.12.1 NodeInitEvent_Received (C++)
As above but for C++. Used in CAN communication only.
NodeInitEvent_Received(
unsigned short usError,
unsigned short usAxisRef
)
Source GMAS\includes\CPP\MMCPPGlobal.h
.NET Definition
Parameters
usAxisRef
The axis reference
usError
Obsolete

### PDF page 1262
<a id="pdf-page-1262"></a>
##### 14.13 Node Error Event (C & C++)
14.13 Node Error Event (C & C++)
Node error event is triggered when error occurs on one of axes (Single / Group). The UDP Da ta received is as
follows:
No. Constant
Definition
Data Comment
Name Type Offset Length
(bytes)
15 NODE_ERROR_EVT Event No. Unsigned
short
0 2
Axis Ref Unsigned
short
12 2
Error ID Unsigned
short
10 2 The error id field is a bitwise
field that indicates what kind
of errors have occurred. The
error bits are:
0x1 FAULT_BIT
0x2 AL_ERROR
0x4 HEARTBEAT
0x10 EMERGENCY \
CFG_FILE
0x20 UNEXPECTED_SW
0x40 COMM
0x80 AXIS_FAULT
Emergency
Code
Unsigned
short
14 2

### PDF page 1263
<a id="pdf-page-1263"></a>
###### 14.13.1 NodeErrorEvent_Received (C++)
14.13.1 NodeErrorEvent_Received (C++)
As above but for C++
NodeErrorEvent_Received(
unsigned short usAxisRef,
unsigned short sErrorID,
unsigned short usEmergencyCode
)
Source GMAS\includes\CPP\MMCPPGlobal.h
.NET Definition
Parameters
usAxisRef
The axis reference
sErrorID
Bit wise field error ID:
0x1 = FAULT_BIT
0x2 = AL_ERROR
0x4 = HEARTBEAT
0x10 = EMERGENCY
0x20 = UNEXPECTED_SW
0x40 = COMMUNICATION
0x80 = AXIS_FAULT
usEmergencyCode
According to CiA DS-301 standard emergency codes. For example:
0x3000 = Voltage generic error.

### PDF page 1264
<a id="pdf-page-1264"></a>
##### 14.14 Stop On Limit Event (C & C++)
14.14 Stop On Limit Event (C & C++)
This event is received when axes (single or group) stopped on a software /hardware limit. The UDP Data
received is as follows:
No. Constant Definition Data Comment
Name Type Offset Length
(bytes)
16 STOP_ON_LIMIT_EVT Event No. Unsigned short 0 2
Axis Ref Unsigned short 12 2
Error ID Short 10 2
Status Unsigned integer 14 4
MSC Limit Unsigned integer 18 4

### PDF page 1265
<a id="pdf-page-1265"></a>
###### 14.14.1 StopOnLimit_Received (C++)
14.14.1 StopOnLimit_Received (C++)
As above but for C++
StopOnLimit_Received(
unsigned short usAxisRef,
unsigned short usError,
unsigned int uiStatusRegister,
unsigned int uiMcsLimitRegister
)
Source GMAS\includes\CPP\MMCPPGlobal.h
.NET Definition
Parameters
usAxisRef
The axis reference
usError
Obsolete
uiStatusRegister
Defined bitwise by:
HW_LIMIT_RLS_BIT (0x00000001)
HW_LIMIT_FLS_BIT (0x00000002)
SW_ACS_LIMIT_LOW_BIT(0x00000004)
SW_ACS_LIMIT_HIGH_BIT(0x00000008)
SW_MCS_LIMIT_LOW_BIT(0x00000010)
SW_MCS_LIMIT_HIGH_BIT(0x00000020)
STOP_ON_LIMIT_MCS (0x00000040)
LIMIT_MANAGE_FSTM_BITS (0x00000180)
LIMIT_AXIS_IN_GROUP_LAST_STATE (0x00000200)
LIMIT_ACS_BITS
(HW_LIMIT_RLS_BIT|HW_LIMIT_FLS_BIT|SW_ACS_LIMIT_LOW_BIT|SW_ACS_LIMIT_HIGH_BIT)
LIMIT_MCS_BITS (SW_MCS_LIMIT_LOW_BIT|SW_MCS_LIMIT_HIGH_BIT)
LIMIT_ALL_BITS (LIMIT_ACS_BITS|LIMIT_MCS_BITS)
TARGET_REACHED (0x00000400)
NODE_SETTLED (0x00000800)
NODE_STD_STATE (0x00001000)
TRaT_FSTM_BITS (0x00006000)
TRACKING_ERROR_BIT (0x00008000)
AXIS_LINKED_MASTER (0x00010000)
AXIS_LINKED_SLAVE (0x00020000)
HOME_ATTAIN_BIT (0x00040000)
uiMcsLimitRegister

### PDF page 1266
<a id="pdf-page-1266"></a>
A 32 bit representation of the software limit status of all kinematic directions:
16 directions * 2 limits (High\Low) = 32

### PDF page 1267
<a id="pdf-page-1267"></a>
##### 14.15 Table Underflow Event
##### 14.16 Global Async Reply Event
14.15 Table Underflow Event
This event is received when the number of remaining PVT points in the table to be e xecuted by the profiler has
reached the minimal limit that was set by the user with MMC_InitTableCmd.
No. Constant Definition Data Comment
Name Type Offset Length (bytes)
17 TABLE_UNDERFLOW_EVT Event No. Unsigned short 0 2
Axis Ref Unsigned short 12 2
14.16 Global Async Reply Event
This event is received when an asynchronous operation that is not related to a specific node is ended. The
event indicates the result of this operation. When in error, the Maestro error code is returned and the status
field is different than zero. When successful, the status and error fields are zero.
No. Constant Definition Data Comment
Name Type Offset Length
(bytes)
19 GLOBAL_ASYNC_REPLY_EVT Event No. Unsigned short 0 2 The global operations that
are indicated by this event
are:
- Set sync time
- Set heartbeat
consumer
command
- Reset System end
result
The enumerators of these
events can be found in the
MMC_events_API.h header
file.
Status Unsigned short 8 2
Error ID Short 10 2
Function ID Unsigned char 12 1

### PDF page 1268
<a id="pdf-page-1268"></a>
###### 14.16.1 GlobalAsyncReply_Received (C++)
14.16.1 GlobalAsyncReply_Received (C++)
As above but for C++
GlobalAsyncReply_Received(
unsigned short usStatus,
unsigned short usError,
unsigned char ucMessageID
)
Source GMAS\includes\CPP\MMCPPGlobal.h
.NET Definition
Parameters
usStatus
Axis status
usError
The error ID of the async reply event. (or 0 upon success)
ucMessageID
Message ID according to the status of the axis
0 = Heartbeat set
1 = Sync time set
2 = Init
3 = Reset

### PDF page 1269
<a id="pdf-page-1269"></a>
##### 14.17 Notification Function Block Event (C & C++)
14.17 Notification Function Block Event (C & C++)
When function block MMC_InsertNotificationFb is inserted to a function block queue, a UDP event is generated
when called from the function block queue. The UDP event sends a EventID defined by the user,which becomes
part of the Motion and Administrative queue.

For details of the function block MMC_InsertNotificationFb, refer to section MMC_InsertNotificationFb.
The event is received when MMC_InsertNotificationFb inserted to the queue is called. When the user enters
the function block, an eventID is automatically entered. This may be any user defined Long value (for example
id=1 meaning after power on). This value will return in the event data.
No. Constant Definition Data Comment
Name Type Offse
Length
(bytes)
21 FB_NOTIFICATION_EVT Event No. Unsigned short 0 2
Axis Ref Unsigned short 12 2
Event code Long 14 4

### PDF page 1270
<a id="pdf-page-1270"></a>
###### 14.17.1 FBNotifyEvent_Received (C++)
14.17.1 FBNotifyEvent_Received (C++)
As above but for C++
FBNotifyEvent_Received(
unsigned short usAxisRef,
int iEventCode
)
Source GMAS\includes\CPP\MMCPPGlobal.h
.NET Definition
Parameters
usAxisRef
The axis reference
iEventCode
This parameter is for the user to insert the "Notify" function block and set it. It can be
any 32 bit number (negative or positive)
For example id = 1 meaning power on.
14.17.1.1 Example
The following eaxmple describes the FBNotifyEvent_Received setting in EASII:

### PDF page 1271
<a id="pdf-page-1271"></a>
##### 14.18 Policy Ended Event (C & C++)
14.18 Policy Ended Event (C & C++)
This category of events' registration is performed separately from regular events mechanism. See the Error
handling documentation for further details.
There are two types of sub events:
- System policy ended event
- Node policy ended event
The UDP Data received for these events is as follows:
No. Constant
Definition
Data Comment
Name Type Offse
Length
(bytes)
22 POLICY_ENDED_EVT Event No. Unsigned short 0 2 The policy type, end state
and error type values
match the correlating error
handling numerators.
These event enumerators
can be found at
MMC_events_API.h and
MMC_PLCopen_single_A.h
header files
Error id short 10 2 0 - indicates that policy
ended successfully
Axis Ref Unsigned short 12 2
Policy type Unsigned char 14 1 0 - node policy.
1 - system policy.
Policy end
state

Unsigned char 15 1 Indicates in which state
the policy failed, or that it
has reached the final state

### PDF page 1272
<a id="pdf-page-1272"></a>
###### 14.18.1 PolicyEndedEvent_Received (C++)
14.18.1 PolicyEndedEvent_Received (C++)
As above but for C++
PolicyEndedEvent_Received(
unsigned short usAxisRef,
short sError,
unsigned char ucPolType,
unsigned char ucPolState,
unsigned char ucErrorType
)
Source GMAS\includes\CPP\MMCPPGlobal.h
.NET Definition
Parameters
usAxisRef
The axis reference
sError
0 = Policy ended successfully.
Not 0 = The error code in which policy ended.
ucPolType
0 = node policy
1= system policy
ucPolState
Indicates in which state the policy failed or that it has reached the final state:
1 = Detected
2 = Stop
3 = Motion
4 = SAFEOP
5 = HANDLED
ucErrorType
Defines the error type according to the enumerator:
0 = PHY Error
1 = Cyclic Error
2 = Missed frames error
3 = AL Error
4 = Unexpected motion error
5 = Drive fault
6 = Quick stop
7 = Heartbeat error
8 = Emergency error
9 = Function block error

### PDF page 1273
<a id="pdf-page-1273"></a>
##### 14.19 Communication Event Mechanism
14.19 Communication Event Mechanism
[PDF field-code object omitted]
Figure 399: Communication events mechanism
Figure 399 describes the communication event mechanism in the Maestro. The HPT thread in the kernel is
scheduled every basic cycle time and is responsible for populating the Events FIFO in the kernel, when a specific
event condition is fulfilled. In the user space (other side), there is a Cycle Tim e Event thread which awakens
every time a new event is entered to the FIFO. This thread processes the new event and sends a UDP message
to the registered user on the event. The Cycle Time Event thread is only involved with cycle time based events,
i.e. events that are generated in the HPT thread scheduling period.
Another thread in the user space, involving event processing, is the Immediate Event thread. This is responsible
for processing events, which require immediate delivery to the registered user. When a condition for an event
is fulfilled, at the CAN driver level, the event's data is sent to a special FIFO in the CAN driver. The Immediate
Event thread is awakened, when a new event is added to the FIFO. The event is processed and sent via UDP
message to the registered user.
It should be noted that the default event mode is 0, No notification to user (default). Therefore, if the event
mode is not properly set to cyclic or immediate notification, no event notification will be received.
There are two advantages of the Immediate Event mechanism relative to the Cycle Time Event mechanism:
- Immediate and fast event notification to the user. No need to wait until the next cycle time in order to
deliver the event to the user.
- Events can be missed in the Cycle Time Event mechanism, e.g. If two input changes occurred between one
cycle and the next, only the last input change event will be delivered to the user, the first input change
event is lost. In the Immediate Event mechanism, the above phenomenon does not occu r.

### PDF page 1274
<a id="pdf-page-1274"></a>
##### 14.20 Events Mask and Enumeration
14.20 Events Mask and Enumeration
The following lists the Events enumerator and their specific ID, relevant to the parameter iEventsMask in C, and
iEventMask in C++. These events are therefore additionally applicable to C++ events and error states.
Events Mask Value Events Enumeration ID
eEVENT_TYPE_COMM_ASYNC_REPLY (1 << 0) ASYNC_REPLY_EVT 0
eEVENT_TYPE_DOWNLOAD_FW (1 << 4) DOWNLOAD_FW_EVT 4
eEVENT_TYPE_COMM_EMGCY (1 << 5) EMCY_EVT 5
eEVENT_TYPE_COMM_MOTION_ENDED (1 << 6) MOTIONENDED_EVT 6
eEVENT_TYPE_COMM_HEARTBEAT_ERROR (1 << 7) HBEAT_EVT 7
eEVENT_TYPE_COMM_PDO_RECEIVED (1 << 8) PDORCV_EVT 8
eEVENT_TYPE_DRIVE_ERROR (1 << 9) DRVERROR_EVT 9
eEVENT_TYPE_EMIT (1 << 10) EMIT_EVT 10
eEVENT_TYPE_HOME_ENDED (1 << 11) HOME_ENDED_EVT 11
eEVENT_TYPE_SYSTEM_ERROR (1 << 12) SYSTEMERROR_EVT 12
eEVENT_TYPE_MODBUS (1 << 13) MODBUS_WRITE_EVT 13
eEVENT_TYPE_TOUCH_PROBE_ENDED (1 << 14) TOUCH_PROBE_ENDED_EVT 14
eEVENT_TYPE_NODE_ERROR (1 << 15) NODE_ERROR_EVT 15
eEVENT_TYPE_STOP_ON_LIMIT (1 << 16) STOP_ON_LIMIT_EVT 16
eEVENT_TYPE_TABLE_UNDERFLOW (1 << 17) TABLE_UNDERFLOW_EVT 17
eEVENT_TYPE_SEND_ASYNC_EVENT (1 << 18)
eEVENT_TYPE_NODE_CONNECTED (1 << 19) NODE_CONNECTED_EVT 18
eEVENT_TYPE_GLOBAL_ASYNC_REPLY (1 << 20) GLOBAL_ASYNC_REPLY_EVT 19
eEVENT_TYPE_NODE_INIT_FINISHED (1 << 21) NODE_INIT_FINISHED_EVT 20
eEVENT_TYPE_FB_NOTIFICATION (1 << 22) FB_NOTIFICATION_EVT 21
eEVENT_TYPE_NODE_POLICY (1 << 23) POLICY_ENDED_EVT 22
Note that a policy ended event will only be sent according to the registered policy without
consideration of the "value" field

### PDF page 1275
<a id="pdf-page-1275"></a>
##### 14.21 Asynchronous Events Callback (C & C++)
###### 14.21.1 Callback Prototype
###### 14.21.2 Data Structure
###### 14.21.3 Event Extraction Example
14.21 Asynchronous Events Callback (C & C++)
This section describes the buffer data structure (pBuff character), which is provided by the Maestro together
with the Event enumerator described in section 14.20 above, as the first input parameter to the callback
function. To use asynchronous events callback, it is necessary to implement the mechanism for Callback
registration (via MMC_IPCInitConnection, or MMC_RPCInitConnection) and PDO configuration. The user must
pre-configure the system to receive Callbacks, e.g. to perform Homing or motion functions, configure PDO's
etc.
In addition to an async reply event being received on an error regardless of the event mask, the
SEND_ASYNC_EVENT event mask is set. This event triggers the callback function even when the async function
has completed successfully (error = 0).
14.21.1 Callback Prototype
The Type definition (MMC_CB_FUNC) is part of the MMC_Definitions.h header file, which also contains the
integer CallbackFunc(unsigned char* pBuff, short sSize, void* pIPSock) function.
pBuff
Buffer consisting of all data related to a specific event
sSize
Buffer size (in bytes). positive numeric format.
pIPSock
The IP socket used. This should not be changed or edited.
14.21.2 Data Structure
Each event sends a buffer with a different data structure. To extract the relevant data from the buffer
according to the Event Type, use the following definitions. These definitions are only present in the CPP header
file MMCConnection.h.
14.21.3 Event Extraction Example
This user implementation is only for Illustration purposes, but describes specifically how the pBuff data in the
xCallbackFunc function is extracted according to the Event Type, which describes where the data is situated
and exactly what the data consists of. For the various local Windows, or .NET select the NetToLocal and
endian_swaps below.
The following offset alias table will help in the understanding of the code described below.
Offset Alias Index
EVENT_ID_INDX 0
AXIS_REF_INDX 12
ASYNC_EVENT_LEN_INDX 14
PDO_GROUP_INDX 14
EVENT_CODE_INDX 14

### PDF page 1276
<a id="pdf-page-1276"></a>
PDO_DATA_INDX 15
MSG_DATA_INDX 8
EMGCY_LEN_INDX 4
EMGCY_DATA_INDX 14
TOUCHP_POS_INDX 14
ERROR_STATE_POS_INDX 10
DOWNLOADVERSION_FUNC_ID 61
User implementation function Events ID and other type definitions will compile correctly since they employ the
same user MMC_DEFINITIONS.H header file.
int UserImplementation(unsigned short usAxisRef,...) {/*do yours*/}
int xCallbackFunc(unsigned char* pBuff, short sSize, void* pIPSock) {
unsigned short usAxisRef;
unsigned short usStatus;
short sErrorId;
unsigned short usEventID;
unsigned short usCobID;
unsigned short usDataLen;
unsigned short usEventGrp = 0;
unsigned short usEmergencyCode;
unsigned long ulData1;
unsigned long ulData2;
unsigned char ucEventType;
MMC_CAN_REPLY_DATA_OUT* pCanReply;
NetToLocal((void *)&pBuff[EVENT_ID_INDX], &usEventID);
usAxisRef = (unsigned short)(*(unsigned
short*)(&pBuff[AXIS_REF_INDX]));
#ifdef WIN32
endian_swap16(&(usAxisRef));
#endif
switch(usEventID)
{
case ASYNC_REPLY_EVT:
pCanReply = (MMC_CAN_REPLY_DATA_OUT*)pBuff;
usStatus = pCanReply->usStatus;
sErrorId = pCanReply->sErrorid;
usCobID = pCanReply->usCOB_ID;
usAxisRef = pCanReply->usAxisRef;
#ifdef WIN32
endian_swap16(&(usStatus));
endian_swap16(&(sErrorId));
endian_swap16(&(usCobID));
#endif
usDataLen = pBuff[ASYNC_EVENT_LEN_INDX];
UserImplementation(usAxisRef, usStatus, sErrorId, usCobID,
usDataLen, pBuff);
break ;
case EMCY_EVT:
UserImplementation(
usAxisRef, (*(unsigned short *)&pBuff[EMGCY_DATA_INDX])); //send axis ref
end
emergency code
break ;
case MOTIONENDED_EVT:

### PDF page 1277
<a id="pdf-page-1277"></a>
NetToLocal((void *)&pBuff[MSG_DATA_INDX+2], &sErrorId);
UserImplementation(usAxisRef, sErrorId == 0); //send axis ref
end OK or not
break;
case HBEAT_EVT:
UserImplementation(usAxisRef); //send axis ref
break ;
case PDORCV_EVT:
usEventGrp = pBuff[PDO_GROUP_INDX];
switch (usEventGrp)
{
case 1:
case 2:
case 3:
case 4:
case 7:
case 8:
case 9:
case 10:
case 12:
case 13:
case 14:
case 15:
NetToLocal((void *)&pBuff[PDO_DATA_INDX], (void *)&ulData1); //type
casting as needed
NetToLocal((void *)&pBuff[PDO_DATA_INDX+4], (void *)&ulData2); //type
casting as needed
break;
case 5:
case 6:
NetToLocal((void *)&pBuff[PDO_DATA_INDX], &ulData1); //type casting
as needed
NetToLocal((void *)&pBuff[PDO_DATA_INDX+2], (void *)&ulData2);
//type casting as needed
break;
case 11:
NetToLocal((void *)&pBuff[PDO_DATA_INDX], (void *)&ulData1);
ulData2 = 0; //irrelevance
break;
default:
break;
}
UserImplementation(usAxisRef, usEventGrp, ulData1, ulData2);
break ;
case HOME_ENDED_EVT:
NetToLocal((void *)&pBuff[MSG_DATA_INDX+2], &sErrorID);
UserImplementation(usAxisRef, sErrorID); //send axis ref and
error
break ;
case MODBUS_WRITE_EVT:
//<UserImplementation();>
break ;
case TOUCH_PROBE_ENDED_EVT:
UserImplementation( usAxisRef,
*((long*)(&pBuff[TOUCHP_POS_INDX])));
break ;
case NODE_ERROR_EVT:
NetToLocal((void *)&pBuff[MSG_DATA_INDX+2], &sErrorID);

### PDF page 1278
<a id="pdf-page-1278"></a>
NetToLocal((void *)&pBuff[MSG_DATA_INDX+6], &usEmergencyCode);
UserImplementation(usAxisRef,sErrorID,usEmergencyCode); //send axis ref end
OK or not
break ;
default:
break ;
case STOP_ON_LIMIT_EVT:
TBD
break ;
case TABLE_UNDERFLOW_EVT:
TBD
break ;
case NODE_CONNECTED_EVT:
TBD
break ;
case GLOBAL_ASYNC_REPLY_EVT:
unsigned char ucFuncID = *((unsigned char*)&buffer[12]);
UserImplementation(ucFuncID,sErrorID,usStatus);
break ;
int fnCallback(unsigned char* ucBuffer, short sReqID, void* pSock)
{
unsigned char ucEventID = ucBuffer[2];
if (ucBuffer[0] == 20) //Event No. is EIP_EVENT
switch (ucEventID)
{
case NM_REQUEST_RESPONSE_RECEIVED:
// printf("NM_REQUEST_RESPONSE_RECEIVED: sReqID = %d\n", sReqID);
break;
case NM_ASSEMBLY_NEW_INSTANCE_DATA:
// printf("NM_ASSEMBLY_NEW_INSTANCE_DATA: assembly instance = %d\n",
sReqID);
break;
case NM_ASSEMBLY_NEW_MEMBER_DATA:
//New data received for the specified assembly member. sReqID
contains assembly instance.
printf("NM_ASSEMBLY_NEW_MEMBER_DATA: assembly instance = %dn",
sReqID;
break;
case NM_REQUEST_FAILED_INVALID_NETWORK_PATH:
break;
case NM_REQUEST_TIMED_OUT:
printf("NM_REQUEST_TIMED_OUT: sReqID = %d\n", sReqID);
break;
case NM_CONNECTION_ESTABLISHED:
// printf("New connection opened with instance %d\n", sReqID);
break;
case NM_CONNECTION_VERIFICATION:
printf("NM_CONNECTION_VERIFICATION\n");
break;
case NM_CONNECTION_RECONFIGURED:
printf("NM_CONNECTION_RECONFIGURED\n");
break;
case NM_CONNECTION_TIMED_OUT:
// printf("Connection with instance %d timed out\n", sReqID);
break;
case NM_CONNECTION_CLOSED:
//printf("Connection with instance %d closed\n", sReqID);
break;

### PDF page 1279
<a id="pdf-page-1279"></a>
case NM_CLIENT_OBJECT_REQUEST_RECEIVED:
printf("NM_CLIENT_OBJECT_REQUEST_RECEIVED\n");
break;
case NM_PENDING_REQUESTS_LIMIT_REACHED:
printf("NM_PENDING_REQUESTS_LIMIT_REACHED\n");
break;
default:
printf("%s Unhandled(unknown) response event. %d\n", __func__,
ucEventID);
break;
}
return 0;
}

### PDF page 1280
<a id="pdf-page-1280"></a>
###### 14.21.4 Net To local Conversion
14.21.4 Net To local Conversion
The following code was extricated from the C++ library, and is displayed here to describe the principle. It is
however, necessary to create a customized implementation for these functions/macros.
inline void NetToLocal(void* NetBuff, unsigned short *usVal)
{
memcpy((unsigned char*)usVal,(unsigned char*)NetBuff, 2);
#ifdef WIN32
endian_swap16((unsigned short *)usVal);
#endif
}
inline void NetToLocal(void* NetBuff, void *iVal)
{
memcpy((unsigned char*)iVal,(unsigned char*)NetBuff,4);
#ifdef WIN32
endian_swap32((unsigned int *)iVal);
#endif
}
inline void endian_swap16(unsigned short* x)
{*x = (*x>>8) | (*x<<8);}
inline void endian_swap32(unsigned int* x)
{*x = (*x>>24) | ((*x<<8) & 0x00FF0000) |((*x>>8) & 0x0000FF00) | (*x<<24);
}

### PDF page 1281
<a id="pdf-page-1281"></a>
###### 14.21.5 AsyncReplyEvent_Received (C++)
14.21.5 AsyncReplyEvent_Received (C++)
As above but for C++
AsyncReplyEvent_Received(
unsigned short usAxisRef,
unsigned short usStatus,
unsigned short usError,
unsigned short usCobID,
unsigned short usLength,
unsigned char* ucBuffer,
unsigned char ucAsyncEventType,
unsigned int uiAbortCode,
unsigned short usIndex,
unsigned char ucHeader,
unsigned char ucSubIndex
)
Source GMAS\includes\CPP\MMCPPGlobal.h
.NET Definition
Parameters
usAxisRef
The axis reference
usStatus
Bitwise status of the axis referenced
0x0 = OK
0x1 = MMC_REMOTE_FUNC_STATUS_BIT_DONE
0x2 = MMC_REMOTE_FUNC_STATUS_BIT_BUSY
0x4 = MMC_REMOTE_FUNC_STATUS_BIT_ACTIVE
0x8 = MMC_REMOTE_FUNC_STATUS_BIT_ABORTED
0x10 = MMC_REMOTE_FUNC_STATUS_BIT_ERROR
0x20 = MMC_REMOTE_FUNC_STATUS_BIT_IN_VELOCITY
0x40 = MMC_REMOTE_FUNC_STATUS_BIT_IN_ACCELERATION
0x80 = MMC_REMOTE_FUNC_STATUS_BIT_IN_DECELERATION
0x100 = MMC_REMOTE_FUNC_STATUS_BIT_VALID
0x200 = MMC_REMOTE_FUNC_STATUS_BIT_POWER_ENABLE_STATUS
usError
The error ID of the async reply event. (or 0 upon success)
usCobID
Used for CAN only
usLength
Data length.

### PDF page 1282
<a id="pdf-page-1282"></a>
For example in the SDO async message, the data length is the length of the CAN
message.
ucBuffer
Any message that the invoking side of the event wants to send. One example can be
CAN message.
ucAsyncEventType
The user can set a certain function that will be called when each event is received and
in the function he can set a certain handling. For example, when SDO upload event is
received, save the data in a certain register.
Defined by the enumerator ID as follows:
0 = eASYNC_EVENT_INVALID_MESSAGE
1 = eASYNC_EVENT_SDO_DOWNLOAD
2 = eASYNC_EVENT_SDO_UPLOAD
3 = eASYNC_EVENT_CHANGE_MOTION_MODE
4 = eASYNC_EVENT_INTERP_CMD_GET
5 = eASYNC_EVENT_READ_DI_GROUP
6 = eASYNC_EVENT_WRITE_DO_GROUP
7 = eASYNC_EVENT_CONFIG_REG_PARAM_EVENT_PDO3
8 = eASYNC_EVENT_CONFIG_REG_PARAM_EVENT_PDO4
9 = eASYNC_EVENT_CONFIG_USER_PARAM_EVENT_PDO3
10 = eASYNC_EVENT_CONFIG_USER_PARAM_EVENT_PDO4
11 = eASYNC_EVENT_CANCEL_PARAM_EVENT_PDO3
12 = eASYNC_EVENT_CANCEL_PARAM_EVENT_PDO4
13 = eASYNC_EVENT_RESET
14 = eASYNC_EVENT_INTERP_CMD_SET
15 = eASYNC_EVENT_SEND_RAW_DATA
16 = eASYNC_EVENT_EXECUTE_LABEL
17 = eASYNC_EVENT_CONFIG_PDO_COMM_PARAM
18 = eASYNC_EVENT_CONFIG_VIRTUAL_ENC
19 = eASYNC_EVENT_BULK_UPLOAD
20 = eASYNC_EVENT_SDO_DOWNLOAD_MOTION_PROCESS
21 = eASYNC_EVENT_SDO_UPLOAD_MOTION_PROCESS
22 = eASYNC_EVENT_VOE_DATA
uiAbortCode
CANopen abort code
usIndex
SDO index
ucHeader
Message header. It can be any message header that the invoking side of the event
wants to send.

### PDF page 1283
<a id="pdf-page-1283"></a>
ucSubIndex
SDO sub index

### PDF page 1284
<a id="pdf-page-1284"></a>
##### 14.22 Notification and Event Function Blocks in C
14.22 Notification and Event Function Blocks in C
The following Notification and event function blocks are described:
Notification and Events
MMC_InsertNotificationFb
MMC_ClearEventsMask
MMC_DisableMotionEndedEvent
MMC_EnableMotionEndedEvent
MMC_GetEventsMask
MMC_SetEventsMask

### PDF page 1285
<a id="pdf-page-1285"></a>
###### 14.22.1 MMC_InsertNotificationFb
14.22.1 MMC_InsertNotificationFb
Inserts a notification function block within a queue to trigger an event. For details refer to section
GlobalAsyncReply_Received (C++)
MMC_LIB_API int MMC_InsertNotificationFb(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_INSNOTIFICATIONFB_IN* pInParam,
OUT MMC_INSNOTIFICATIONFB_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Not Supported
Source GMAS\includes\MMC_events_API.h
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
Points to the MMC_INSNOTIFICATIONFB input data structure using the
MMC_InsertNotificationFb function.
pOutParam
Points to the MMC_INSNOTIFICATIONFB_OUT output structure receiving information
as a result of calling the MMC_InsertNotificationFb function.
Remarks
None
Scope
All
MMC_INSNOTIFICATIONFB_IN Structure
typedef struct mmc_insnotificationfb_in{
int iEventCode;
long lSpare[8];
}MMC_INSNOTIFICATIONFB_IN;
Parameters

### PDF page 1286
<a id="pdf-page-1286"></a>
iEventCode
This value will be received in the notification event data. Any int value.
lSpare[8]
For internal use only. 8 bits code data reserved.
MMC_INSNOTIFICATIONFB_OUT Structure
typedef struct mmc_insnotificationfb_out{
unsigned int uiHndl;
unsigned short usStatus;
short sErrorID;
}MMC_INSNOTIFICATIONFB_OUT;
Parameters
uiHndl
Returned function block handle. Any positive value.
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error
IDs. Displays an error code as negative or positive integers.
Figure 400 describes the function block for MMC_InsertNotificationFb

MMC_InsertNotificationFb
uiHndl Boolean
usStatus
Done, Busy, Active,
CommandAborted, Error
usErrorID
Bitwise
Error codelong lSpare[8]
integer iEventCode

Figure 400: MMC_InsertNotificationFb function block

### PDF page 1287
<a id="pdf-page-1287"></a>
14.22.1.1 Function Block Code Example
int rc;
MMC_INSNOTIFICATIONFB_IN pInParam;
MMC_INSNOTIFICATIONFB_OUT pOutParam;

pInParam.iEventCode = iEventCode;

if(MMC_InsertNotificationFb(hConn,aRef,&pInParam,&pOutParam)!=0)
{
printf("MMC_InsertNotificationFb error id %d\n",(short)pOutParam.sErrorID);
}
Receive event in callback:
unsigned short usAxisRef;
long Data=0;
usAxisRef = (unsigned short)(*(unsigned short*)(&recvBuffer[12]));
Data=*((long*)(&recvBuffer[14]));

..

case FB_NOTIFICATION_EVT:
printf("usAxisRef = %d\n",usAxisRef);
printf("Data = %ld\n",Data);
break;

### PDF page 1288
<a id="pdf-page-1288"></a>
###### 14.22.2 MMC_ClearEventsMask
14.22.2 MMC_ClearEventsMask
Clears the events mask for a specific connection depending to the input mask.
MMC_LIB_API int MMC_ClearEventsMaskCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_CLEAREVENTSMASK_IN* pInParam,
OUT MMC_CLEAREVENTSMASK_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_events_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the Connection
Handle Type. It should be noted that this connection handle is common throughout all
Maestro functions. This connection handle is returned by the Init Connection command. If
an error occurs, the function returns -1 and a MMC_LIB_API error with more details.
pInParam
Points to the MMC_CLEAREVENTSMASK input data structure using the
MMC_ClearEventsMask function.
pOutParam
Points to the MMC_CLEAREVENTSMASK_OUT output structure receiving information as a
result of calling the MMC_ClearEventsMask function.
Remarks
This involves zeroing the final event data integer of a 32-bit event data with the result of removing the event
from the Maestro. The event to be cleared will depend on the ID event input of the parameter iEventsMask .
Scope
All
MMC_CLEAREVENTSMASK_IN Structure
typedef struct{
int iEventsMask;
}MMC_CLEAREVENTSMASK_IN;
Parameters
iEventsMask
Defined according to the event IDs described in the section Events Mask and
Enumeration. Bitwise positive integer ID.

### PDF page 1289
<a id="pdf-page-1289"></a>
MMC_CLEAREVENTSMASK_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_CLEAREVENTSMASK_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Displays an error code as negative or positive integers.
Figure 401 describes the function block for MMC_ClearEventsMask

MMC_ClearEventsMask
usStatus
usErrorID
Bitwise
error code
iEventsMaskinteger

Figure 401: MMC_ClearEventsMask function block

### PDF page 1290
<a id="pdf-page-1290"></a>
14.22.2.1 Function Block Code Example
int rc;
MMC_CLEAREVENTSMASK_IN stClearEventsMask_in;
MMC_CLEAREVENTSMASK_OUT stClearEventsMask_out;
//
// Inserting the structure parameters:
stClearEventsMask_in.iEventsMask = 64; //Events mask ID 7 bit 7 is on
1000000 = 64(Dec)
//
rc = MMC_ClearEventsMaskCmd (hConn, &stClearEventsMask_in,
&stClearEventsMask_out);
if (rc != 0)
{
HandleError();
}
