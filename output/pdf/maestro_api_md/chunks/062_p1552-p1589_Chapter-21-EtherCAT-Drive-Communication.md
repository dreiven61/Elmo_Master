# Chapter 21 EtherCAT Drive Communication

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 1552-1589
- Chunk: `062_p1552-p1589_Chapter-21-EtherCAT-Drive-Communication.md`

## Active Outline At Chunk Start
- p. 1552 - Chapter 21 EtherCAT Drive Communication

## Contained Bookmark Outline
- p. 1552 - Chapter 21 EtherCAT Drive Communication
  - p. 1553 - 21.1 Elmo EtherCAT
  - p. 1554 - 21.2 Elmo Slave Drives
  - p. 1555 - 21.3 EtherCAT with Maestro
  - p. 1556 - 21.4 EtherCAT Gateway
  - p. 1556 - 21.5 EtherCAT Redundancy in the Platinum Maestro
    - p. 1557 - 21.5.1 Introduction
    - p. 1558 - 21.5.2 Description
    - p. 1559 - 21.5.3 Redundancy Functionality
    - p. 1563 - 21.5.4 EAS Configuration
    - p. 1564 - 21.5.5 Platinum Maestro API
  - p. 1566 - 21.6 EtherCAT Aliasing support in the Platinum Maestro
    - p. 1566 - 21.6.1 Introduction
    - p. 1566 - 21.6.2 Description
    - p. 1568 - 21.6.3 Supported Device Identification Methods
    - p. 1569 - 21.6.4 EAS EtherCAT Configuration Tool Support
    - p. 1570 - 21.6.5 Platinum Maestro API
  - p. 1571 - 21.7 EtherCAT Function Blocks
    - p. 1572 - 21.7.1 MMC_DisableEthercatConfigMode
    - p. 1574 - 21.7.2 MMC_EnableEthercatConfigMode
    - p. 1576 - 21.7.3 MMC_ECATIODisableDIChangedEvent
    - p. 1578 - 21.7.4 MMC_ECATIOEnableDIChangedEvent
    - p. 1580 - 21.7.5 MMC_ECATIOReadDigitalInput
    - p. 1583 - 21.7.6 MMC_ECATIOReadAnalogInput
    - p. 1586 - 21.7.7 MMC_ECATIOWriteAnalogOutput
    - p. 1588 - 21.7.8 MMC_ECATIOWriteDigitalOutput

## Extracted Text

### PDF page 1552
<a id="pdf-page-1552"></a>
#### Chapter 21 EtherCAT Drive Communication
Chapter 21 EtherCAT Drive Communication
Ethernet for Control Automation Technology (EtherCAT) is an open high performance Ethernet -based fieldbus
system, which uses the family of industrial computer network protocols used for real-time distributed control,
now standardized as IEC 61158. It is a highly flexible Ethernet network protocol that runs over a fast real time
Master-Slave network.
The EtherCAT communication speed is up to 100 Mbps full duplex and can include a maximum of 65,535
stations in a single network configuration such as Ethernet star, line or tree without using switches.
Figure 486 describes a network of EtherCAT slaves in a ring topology. The Master controls the traffic in the
network by initiating the transactions.

SLAVE SLAVE SLAVE
MASTER

Figure 486: EtherCAT Network Configuration
Usually, a control system requires the following in periodic time intervals:
- Inputs
Latched Sensors Data such as Positions, Velocities, Currents, System Status, IO's etc.,
- Outputs
Control Law commands, or Trajectory Information, or Higher Drive Level Commands.
The specific nature of the data transferred via the network depends on the operation mode o f the slave drive.
The Device Profile describes the application parameters and the functional behavior of the devices including
the device class-specific state machines. A common standard for Drive Device Profiles is the DS -402, CANopen
Device Profile, and CoE (Can Over EtherCAT).
The EtherCAT protocol is optimized for process data and is transported directly within the standard IEEE 802.3
Ethernet frame. Each Ethernet frame can include several EtherCAT frames, each serving another slave.
EtherCAT network use a processing on the fly, whereby the Ethernet frame is received and processed while the
telegram passes through the device. The frames only delay by a fraction of a microsecond in each node. Using
EtherCAT, the entire network can be addressed with just one frame.
The data sequence is independent of the physical order of the nodes in the network; addressing can be in any
order. Broadcast, multicast and communication between slaves are possible and must be performed by the
master device.
The EtherCAT protocol can be inserted into UDP/IP datagrams. This also enables any control with an Ethernet
protocol stack to address EtherCAT systems.
Using the Master configuration tool, the Master scans the EtherCAT network and uses the EtherCAT Slave
library to compare the slave memory area that includes information about t he slave such as Vendor ID, Product
Code, and Slave Configuration.

### PDF page 1553
<a id="pdf-page-1553"></a>
##### 21.1 Elmo EtherCAT
21.1 Elmo EtherCAT
The ELMO environment comprises of three levels (Figure 487):
- EAS EtherCAT configuration tools
- EtherCAT Maestro master
- Elmo EtherCAT slave drives

EAS
EtherCAT Configuration Tool
(Offline)
G-MAS
EtherCAT MASTER
(Online)
Configuration
Binary information
Configuration
XML information
EEPROM
Information
ELMO DRIVES
EtherCAT SLAVE

Figure 487: EtherCAT Environment

### PDF page 1554
<a id="pdf-page-1554"></a>
##### 21.2 Elmo Slave Drives
21.2 Elmo Slave Drives
The following diagram describes the EtherCAT communication of the drive.

Figure 488: Layered Communication protocol in EtherCAT
Physical Layer
The Physical layer of the EtherCAT is a 100Mbits/sec Ethernet port over twisted per cable.
Data Link Layer
This supports two mechanisms of data transfer:
- Process data
Allows writing and reading data simultaneously. This mode is used to transfer the Process data objects
(PDO). The PDO transfers via SYNC Manager 2 (PDO_Out) and SYNC Manager 3 (PDO_IN)
- Mailbox
The mailbox mechanism assure that the data will reach to the target. The mailbox is used to transfer
the SDOs. The SDO transfers via SYNC Manager 0 (MailboxOut) and SYNC Manager 1 (MailboxIn). SDO
objects are used for user triggered access. With SDO services, all of the OD's entries can be read or
written. The SDO transport works in asynchronous mode only.
The Elmo drive supports the following communication protocols:

### PDF page 1555
<a id="pdf-page-1555"></a>
##### 21.3 EtherCAT with Maestro
CoE
(CANopen over EtherCAT)
Defines a standard way to access the CANopen protocol and inc ludes
an object dictionary, SDO, PDO and emergency messages.
EoE
(Ethernet over EtherCAT)
Fully Ethernet compatible, defines a standard way to exchange or
tunnel standard Ethernet frames.
FoE
(File over EtherCAT)
Similar to TFTP, enables access to any data structure in the device, and
defines a standard way to download and upload firmware and other
files.
The Object Dictionary (OD) contains parameters, application data and the mapping information between the
process data interface and application data (PDO mapping). Its entries can be accessed via the Service Data
Object (SDO).
An Object Dictionary is a naming system that provides a unique identifier to each data item or "object"
communicated over the CoE protocol. An object is identified by an index, and if a complex object, by a sub-
index as well. CoE and EoE protocols require a set of mandatory objects.
Elmo drive supports distributed clock in order to synchronize between the Master and Slaves on the EtherCAT
network.
21.3 EtherCAT with Maestro
While a single servo drive can run as a stand-alone drive, without the Maestro, using its inner profiler and filter.
In order to perform synchronized multi axis motions in the system (such as circle, line etc..), a real time
communication protocol must be used, and all drives must be synchronized to a specific SYNC signal in the
system. The EtherCAT communication protocol enables synchronization of all the controllers to the same SYNC
signal by updating the drives in the system with the Maestro's master time. Thus, all drives in the system are
synchronized to the master clock, and all generate an interrupt at exactly the same time.
A profiler can run in the Maestro, on the condition that the axis (axes) is defined as a vector axis (axes). A
vector axis may consist of 1 - 16 axes. The Multi Axis Indexer (MAI) is the profiler that runs within the Maestro,
which sends (via a high priority interrupt routine) a calculated set point to t he axes in the system and can
perform vector calculations for up to 16 axes. The profiler EtherCAT and CAN outputs are points that are to be
sent to the specific drives belonging to the vector. Therefore, a number of combination options are available:
- 1 x 16 axes (One vector profiler performing profiles for 16 axes),
- 16 x 1 axes (16 profilers for 16 vector axes), or,
- Any combination of M x N axes - as long as M x N < 16
The SYNC interrupt signal to the drives is based on the ET1100 component in the servo dr ive. The master
Maestro does not receive this signal, but can calculate when the SYNC signal is generated. This is because the
master EtherCAT in the Maestro is responsible for updating the SYNC cycle time in the servo drives, and
therefore knows when the SYNC is generated. The MAI can operate at varying cycle times, dependent on a
number of parameters, such as the:
- Desired response from the system
- Number of axes participating in the MAI. The more axes, the higher the cycle rate

### PDF page 1556
<a id="pdf-page-1556"></a>
##### 21.4 EtherCAT Gateway
##### 21.5 EtherCAT Redundancy in the Platinum Maestro
21.4 EtherCAT Gateway
Under normal circumstances, the Maestro task manages the EtherCAT communications and all devices
connected via the EtherCAT. In this situation, adding or removing communications to an Input/Output module
or servo drive involves programming the specific EtherCAT resource file. This can involve a quite complex
procedure. A more sophisticated alternative exits to stop the operation of the Maestro normal task manager
for EtherCAT via the MMC_EnableEthercatConfigMode function block, and directly add or remove the
Input/Output module or servo drive, automatically updating the EtherCAT resource file during the process. To
perform this Gateway process, the EAS application is used. When the process is completed, the function block
MMC_DisableEthercatConfigMode is run in the Maestro to return the task management to the Maestro using
the updated EtherCAT resource file.
21.5 EtherCAT Redundancy in the Platinum Maestro

### PDF page 1557
<a id="pdf-page-1557"></a>
###### 21.5.1 Introduction
21.5.1 Introduction
Cable redundancy is designed to compensate for the failure of a communication cable section in the EtherCAT
system. A ring topology, which normally is operated in both directions, is therefore to be used. Both branches
can nevertheless still be reached if the ring is interrupted at some point.

### PDF page 1558
<a id="pdf-page-1558"></a>
###### 21.5.2 Description
21.5.2 Description
A second network port is used for ring closure at the EtherCAT master control system. Both cyclic and acyclic
frames are sent simultaneously through both ports, and are transported through the system.
In the absence of any fault, the cyclic and acyclic frames are sent via the primary port (Master 1). In the case of
cable break or node failure - the Platinum Maestro will also send data via the redundancy port (Master 2).

Figure 489: EtherCAT Redundancy Example
The cable redundancy is single-error tolerant, i.e. communication with the slaves can continue if the cable is
interrupted in one place.
When the communication is restored the original communication direction is restored. If the communication is
interrupted in more than one place, all connections have to be restored before another fault may occur.
It is also possible to start up the system under redundancy conditions.
Due to the nature of this principle, a closed ring topology is most suited to redundant c able operation.
Note:
NO additional Hardware is required to support the Redundancy Feature. It merely needs to be configured a as
part of the Ethercat Configuration.

### PDF page 1559
<a id="pdf-page-1559"></a>
###### 21.5.3 Redundancy Functionality
21.5.3 Redundancy Functionality
In the case of a cable break - all types of EtherCAT communications (process data and mailbox protocols) are
supported without any restrictions.
It is necessary to differentiate between a Cable Break and Node Failure. Obviously - if the node ceases to work
(power failure, etc ...) - it will need to be fixed / replaced.
The following cases are handled by the Platinum Maestro:
- Without Redundancy
- With Redundancy
21.5.3.1 Without Redundancy
In normal operation and when there are no cable errors - the Network works normally:

Cable Failure without Redundancy - in the case of a cable break at the two last drives - they will be lost from
the network:

Node Failure without redundancy - in case of a node failure - the last 2 drives will be lost from the network:

### PDF page 1560
<a id="pdf-page-1560"></a>
[No extractable text on this page.]

### PDF page 1561
<a id="pdf-page-1561"></a>
21.5.3.2 With Redundancy
Normal Operation

Cable Failure with Redundancy - The Fieldbus Remains Fully Operational:

Node Failure - With Redundancy - Fieldbus Remains Fully Operational except for the failed node of course:

### PDF page 1562
<a id="pdf-page-1562"></a>
In addition - the following cases are also handled by the Platinum Maestro:
- Platinum Maestro remains operational in case of a cable break between the primary port (Master 1)
and the first slave.
- Platinum Maestro remains operational in case of a cable break between the redundancy port (Master
2) and the last slave.
- Platinum Maestro remains operational in case the cable was fixed.

### PDF page 1563
<a id="pdf-page-1563"></a>
###### 21.5.4 EAS Configuration
21.5.4 EAS Configuration
In order to work with Redundancy, it must be configured via the EtherCAT Configuration Tool.
1. In the Distributed Clock page - the user must select - "Master DC" as the reference clock.

2. From the Master Tab, set the Redundancy check-box:

3. From the Error Policies window, set all of the PHY error policy to "disabled" for all slaves on the bus.

### PDF page 1564
<a id="pdf-page-1564"></a>
###### 21.5.5 Platinum Maestro API
21.5.5 Platinum Maestro API
A new API has been added:
MMC_GetCommDiagnosticsEx
This is similar to:
MMC_GetCommDiagnostics
With the additional information in YELLOW:
typedef struct
{
unsigned short usStatus;
unsigned short usErrorID;
unsigned short usMainSlaveCount;
unsigned short usRedundancySlaveCount;
unsigned short usNetworkState;
MMC_ETHERCAT_DIAGNOSTICS_INFO pDiagnosticsSlavesArr[ETHERCAT_ID_MAX];
unsigned long ulSpare[50];
}MMC_GETCOMMDIAGNOSTICSEX_OUT;
usMainSlaveCount - Returns the number of slaves currently detected on the Primary port (Master1) NIC. The
value will be 0 if the cable was disconnected at the first slave.
usRedundancySlaveCount - Returns the number of slaves currently detected on the Redundancy port (Master2)
NIC. The value will be 0 if the master is operating normally (without redundancy).
usNetworkState - can be one of the following values:
0 - State when redundancy is not used
1 - State when redundancy is used and In-port of a slave is connected to the Primary port.
2 - State when redundancy is used and In-port of a slave is connected to the Redundant port.
3 - State when redundancy is used and there is a break between slaves.
0x100 - Flag that indicates that EtherCAT frames reach the opposite card when Redundancy is used.

The following case:

### PDF page 1565
<a id="pdf-page-1565"></a>
Reports - usMainSlaveCount = 3
usRedundancySlaveCount = 0

Reports - usMainSlaveCount = 2
usRedundancySlaveCount = 2

### PDF page 1566
<a id="pdf-page-1566"></a>
##### 21.6 EtherCAT Aliasing support in the Platinum Maestro
###### 21.6.1 Introduction
###### 21.6.2 Description
21.6 EtherCAT Aliasing support in the Platinum Maestro
21.6.1 Introduction
This document describes the functionality of the EtherCAT Explicit Device Identif ication mechanism (Aliasing)
supported within the Maestro. This is supported both in the Gold Maestro and Platinum Maestro over
EtherCAT network.
21.6.2 Description
The EtherCAT specification defines numerous methods for setting / reading an explicit device ident ification
over the EtherCAT Network.
The device identification is a numeric value which is used by the EtherCAT Master (the Maestro or other
Master) in order to identify a slave independently to its position on the network. It must be a unique value on
the network and is 16 bit length [0..65535].
The device identification value may be configured at the slave level, in two ways:
- ID selector
- Configured Station Alias
21.6.2.1 ID Selector
This usually consists of a DIP switch on the Slave which defines the slave ID.

In Elmo Drives, the EtherCAT address of each axis is specified by two switches. The position of the EtherCAT
switches may either be found, inside the battery compartment as in the Gold Duet, or in other positions on the
drive or drivers interface.

### PDF page 1567
<a id="pdf-page-1567"></a>
Use a fine screwdriver to set the low and the high bytes of the EtherCAT address.
21.6.2.2 Configured Station Alias
The address [0..65536] is permanently stored in the E²PROM of the Ether CAT slave and can be read by the
EtherCAT master.

### PDF page 1568
<a id="pdf-page-1568"></a>
###### 21.6.3 Supported Device Identification Methods
21.6.3 Supported Device Identification Methods
There are three device identification methods supported by the Maestro:
Requesting ID - This method is suggested for slaves with an ID selector (DIP Switch) and a
uController
In this method the slave should load the identification value which wa s
configured via the ID selector to 0x134 ESC register
- This is the most commonly used mode and is supported in Elmo Drives that
have DIP switches on their hardware. Gold and Platinum Drives
Direct ID - This method is suggested for slaves with an ID selector (DIP Switch) but
without a uController
- The identification value can be read via the master at a dedicated ESC register
- which must be stored at the slave's ESI file. This is the ADO - Address Offset
- This method is not supported by Elmo Drives
Configured Station Alias - All slaves can support this method
- The configured station alias is stored via the Maestro at the Slave EEPROM and
is loaded automatically to 0x12 ESC register after power-cycle or reset
- There is no need for an ID selector (DIP Switch) as using this method is merely
reading a register where the ID is saved
- This mode means that the EtherCAT Configuration Tool - Or a specific method
for writing to the Slave EEPROM Must be used
- This method is not supported by the Elmo Gold Drive.

### PDF page 1569
<a id="pdf-page-1569"></a>
###### 21.6.4 EAS EtherCAT Configuration Tool Support
21.6.4 EAS EtherCAT Configuration Tool Support
The EAS consists of dedicated interfaces for supporting the above methods.
In general the ESI file belonging to the EtherCAT device contains the information regarding which addressing
methods the slave supports. The EAS disables the modes that are not supported.
The user selects one the above three identification methods within the EAS slave's identification window. The
chosen report method which is selected by user will be delivered to the master (via the Maestro master XML
file).

The configured and detected Alias may be read from the Master level within the EAS EtherCAT Configuration
Tool:

When the slave identification method has been enabled, and has be en detected on the bus, the Maestro
master will verify the slave's identification value, before communication with the slave.
In case there is a mismatch between the identification preconfigured value and the actual value which is read
from the slave, the master will enter an INVALID CONFIGURATION state.

### PDF page 1570
<a id="pdf-page-1570"></a>
###### 21.6.5 Platinum Maestro API
21.6.5 Platinum Maestro API
A new API has been added: MC_GetSlaveScanAlias
C:

typedef struct mmc_getslavescanalias_in
{
unsigned char ucDummy;
} MMC_GETSLAVESCANALIAS_IN;

typedef struct mmc_getslavescanalias_out
{
unsigned short usStatus;
unsigned short usErrorID;
unsigned short usNumberOfAlias;
unsigned short usScanAlias[256];
unsigned short usScanAliasIndex[256];
unsigned char ucSpare[256];
} MMC_GETSLAVESCANALIAS_OUT;

MMC_LIB_API int GetSlaveScanAlias(MMC_CONNECT_HNDL hConn,IN MMC_GETSLAVESCANALIAS_IN*
pInParam,MMC_GETSLAVESCANALIAS_OUT* pOutParam);

C++:
GetSlaveScanAlias(MMC_GETSLAVESCANALIAS_OUT &pOutParam)

usNumberOfAlias -> Number of Alias axes
usScanAlias -> Alias value
usScanAliasIndex -> The actual index of the axis

This API allows us to read the Aliasing detected during Runtime.

### PDF page 1571
<a id="pdf-page-1571"></a>
##### 21.7 EtherCAT Function Blocks
21.7 EtherCAT Function Blocks
The following EtherCAT drive communication function blocks are described, with the exception of
MMC_ETHERCAT_DIAGNOSTICS_INFO, which is a structure:

Drive Communication
MMC_DisableEthercatConfigMode
MMC_EnableEthercatConfigMode
MMC_ECATIODisableDIChangedEvent
MMC_ECATIOEnableDIChangedEvent
MMC_GetCommStatistics
MMC_GetCommDiagnostics
MMC_Get ReactorStatistics
MMC_IsEthercatConfigMode
MMC_ECATIOReadDigitalInput
MMC_ECATIOReadAnalogInput
MMC_ResetCommDiagnostics
MMC_ResetCommStatistics
MMC_SendSDO
MMC_ECATIOWriteAnalogOutput
MMC_ECATIOWriteDigitalOutput

### PDF page 1572
<a id="pdf-page-1572"></a>
###### 21.7.1 MMC_DisableEthercatConfigMode
21.7.1 MMC_DisableEthercatConfigMode
Disables the EtherCAT configuration mode. Enables the Maestro task manager to disable direct
programming of the Maestro via the Gateway.
MMC_LIB_API int MMC_DisableEthercatConfigMode(
IN MMC_CONNECT_HNDL hConn,
(IN MMC_DISABLE_ECATCONFIGMODE_IN* pInParam)
OUT MMC_DISABLE_ECATCONFIGMODE_OUT* pOutParam
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
pInParam
Points to the MMC_DISABLE_ECATCONFIGMODE input data structure using the
MMC_DisableEthercatConfigMode function.
pOutParam
Points to the MMC_DISABLE_ECATCONFIGMODE_OUT output structure receiving
information, as a result of calling the MMC_DisableEthercatConfigMode function.
Remarks
Extension of the CANopen technology disabling the Gateway communication between a host system and the
Maestro.
Scope
All
MMC_DISABLE_ECATCONFIGMODE_IN Structure
typedef struct{
unsigned char ucDummy;
}MMC_DISABLE_ECATCONFIGMODE_IN;
Parameters
ucDummy
Dummy value. Any negative or positive character.

### PDF page 1573
<a id="pdf-page-1573"></a>
MMC_DISABLE_ECATCONFIGMODE_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_DISABLE_ECATCONFIGMODE_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and 4.6 NC Profiler
Error IDs. Displays an error code as negative or positive integers.
Figure 490 describes the function block for DisableEthercatConfigMode.
[PDF field-code object omitted]
Figure 490: DisableEthercatConfigMode function block
21.7.1.1 Function Block Code Example
int rc;
MMC_DISABLE_ECATCONFIGMODE_IN stDisableEcatConfigMode_in;
MMC_DISABLE_ECATCONFIGMODE_OUT stDisableEcatConfigMode_out;
//
// Inserting the structure parameters:
stDisableEcatConfigMode_in.ucDummy = 1; // Dummy input
//
rc = MMC_DisableEthercatConfigMode (hConn, &stDisableEcatConfigMode_out);
if (rc != 0)
{
HandleError();
}

### PDF page 1574
<a id="pdf-page-1574"></a>
###### 21.7.2 MMC_EnableEthercatConfigMode
21.7.2 MMC_EnableEthercatConfigMode
Disables the Maestro task manager to enable direct programming of the Maestro via the Gateway.
MMC_LIB_API int MMC_EnableEthercatConfigMode(
IN MMC_CONNECT_HNDL hConn,
OUT MMC_ENABLE_ECATCONFIGMODE_OUT* pOutParam
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
pOutParam
Points to the MMC_ENABLE_ECATCONFIGMODE_OUT output structure receiving
information, as a result of calling the MMC_EnableEthercatConfigMode function.
Remarks
Extension of the CANopen technology enabling the Gateway communication between a host system and the
Maestro.
Scope
All
MMC_ENABLE_ECATCONFIGMODE_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_ENABLE_ECATCONFIGMODE_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID

### PDF page 1575
<a id="pdf-page-1575"></a>
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and 4.6NC Profiler
Error IDs. Displays an error code as negative or positive integers.
Figure 491 describes the function block for MMC_EnableEthercatConfigMode
[PDF field-code object omitted]
Figure 491: MMC_EnableEthercatConfigMode function block
21.7.2.1 Function Block Code Example
int rc;
MMC_ENABLE_ECATCONFIGMODE_IN stEnableEcatConfigMode_in;
MMC_ENABLE_ECATCONFIGMODE_OUT stEnableEcatConfigMode_out;
//
// Inserting the structure parameters:
stEnableEcatConfigMode_in.ucDummy = 1; // Dummy input
//
rc = MMC_EnableEthercatConfigMode (hConn, &stEnableEcatConfigMode_out);
if (rc != 0)
{
HandleError();
}

### PDF page 1576
<a id="pdf-page-1576"></a>
###### 21.7.3 MMC_ECATIODisableDIChangedEvent
21.7.3 MMC_ECATIODisableDIChangedEvent
Disables an EtherCAT I/O input event change against an I/O module.
MMC_LIB_API int MMC_ECATIODisableDIChangedEvent (
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_DISABLEDICHANGEDEVENT_IN* pInParam,
OUT MMC_DISABLEDICHANGEDEVENT_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_ECATIO_API.h
GMAS Programming(IEC 61331 Program)\ElmoECATIO
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
MMC_ECATIODisableDIChangedEvent function.
pOutParam
Points to the MMC_DISABLEDICHANGEDEVENT_OUT output structure receiving
information, as a result of calling the MMC_ECATIODisableDICha ngedEvent function.
Remarks
When enabled, any EtherCAT I/O input event change is sent from the I/O module to the Maestro and then
host server (if connected).
Scope
All
MMC_DISABLEDICHANGEDEVENT_IN Structure
typedef struct{
unsigned char ucDummy;
}MMC_DISABLEDICHANGEDEVENT_IN;

### PDF page 1577
<a id="pdf-page-1577"></a>
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
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and 4.6 NC Profiler
Error IDs.
Figure 492 describes the function block for MMC_ECATIODisableDIChangedEvent as applied within the IEC
61131 programming.
[PDF field-code object omitted]
Figure 492: MMC_ECATIODisableDIChangedEvent function block
21.7.3.1 Function Block Code Example
int rc;
MMC_DISABLEDICHANGEDEVENT_IN stDisableDIChangeEv_in;
MMC_DISABLEDICHANGEDEVENT_OUT stDisableDIChangeEv_out;
//
// Inserting the structure parameters:
stDisableDIChangeEv_in.ucDummy = 1; //Dummy data input
//
rc = MMC_ECATIODisableDIChangedEvent (hConn, iAxisRef,
&stDisableDIChangeEv_in, &stDisableDIChangeEv_out);
if (rc != 0)
{
HandleError();
}

### PDF page 1578
<a id="pdf-page-1578"></a>
###### 21.7.4 MMC_ECATIOEnableDIChangedEvent
21.7.4 MMC_ECATIOEnableDIChangedEvent
Enables an EtherCAT I/O input event change.
MMC_LIB_API int MMC_EnableDS401DIChangedEvent(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_ENABLEDICHANGEDEVENT_IN* pInParam,
OUT MMC_ENABLEDICHANGEDEVENT_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_ECATIO_API.h
GMAS Programming(IEC 61331 Program)\ElmoECATIO
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
MMC_ECATIOEnableDIChangedEvent function.
pOutParam
Points to the MMC_ENABLEDICHANGEDEVENT_OUT output structure receiving
information, as a result of calling the MMC_ECATIOEnableDIChangedEvent function.
Remarks
When enabled, any EtherCAT I/O input event change is sent from the I/O module to the Maestro and then
host server (if connected).
Scope
All
MMC_ENABLEDICHANGEDEVENT_IN Structure
typedef struct{
unsigned char ucDummy;
}MMC_ENABLEDICHANGEDEVENT_IN;

### PDF page 1579
<a id="pdf-page-1579"></a>
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
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and 4.6 NC Profiler
Error IDs.
Figure 493 describes the function block for MMC_ECATIOEnableDIChangedEvent as applied within the IEC
61131 programming.
[PDF field-code object omitted]
Figure 493: MMC_ECATIOEnableDIChangedEvent function block
21.7.4.1 Function Block Code Example
int rc;
MMC_ENABLEDICHANGEDEVENT_IN stEnableDIChangeEv_in;
MMC_ENABLEDICHANGEDEVENT_OUT stEnableDIChangeEv_out;
//
// Inserting the structure parameters:
stEnableDIChangeEv_in.ucDummy = 1; //Dummy data input
//
rc = MMC_ECATIOEnableDIChangedEvent (hConn, iAxisRef,
&stEnableDIChangeEv_in, &stEnableDIChangeEv_out);
if (rc != 0)
{
HandleError();
}

### PDF page 1580
<a id="pdf-page-1580"></a>
###### 21.7.5 MMC_ECATIOReadDigitalInput
21.7.5 MMC_ECATIOReadDigitalInput
Reads the EtherCAT I/O input of all 64 bit I/O's in one action, increasing the communication speed
proportionately versus reading 8 x groups of 8 I/O's.
MMC_LIB_API int MMC_ECATIOReadDigitalInput (
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READDI_IN* pInParam,
OUT MMC_READDI_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_ECATIO_API.h
GMAS Programming(IEC 61331 Program)\ElmoECATIO
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
Points to the MMC_READDI input data structure using the
MMC_ECATIOReadDigitalInput function.
pOutParam
Points to the MMC_READDI_OUT output structure receiving information, as a result of
calling the MMC_ECATIOReadDigitalInput function.
Remarks
None
Scope
All
MMC_READDI_IN Structure
typedef struct{
unsigned char dummy;
}MMC_READDI_IN;

### PDF page 1581
<a id="pdf-page-1581"></a>
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
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and 4.6 NC Profiler
Error IDs.
Figure 494 describes the function block for MMC_ECATIOReadDigitalInput as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 494: MMC_ECATIOReadDigitalInput function block
21.7.5.1 Function Block Code Example
int rc;
MMC_READDI_IN stReadDI_in;
MMC_READDI_OUT stReadDI_out;
//
// Inserting the structure parameters:
stReadDI_in.dummy = 1; //dummy input
//

### PDF page 1582
<a id="pdf-page-1582"></a>
rc = MMC_ECATIOReadDigitalInput (hConn, iAxisRef, &stReadDI_in,
&stReadDI_out);
printf("EtherCAT Input Status[%ld] ErrId[%d]\n", (long
int)stReadDI_out.ulliDI, (short)stReadDI_out.sErrorID);
if (rc != 0)
{
HandleError();
}

### PDF page 1583
<a id="pdf-page-1583"></a>
###### 21.7.6 MMC_ECATIOReadAnalogInput
21.7.6 MMC_ECATIOReadAnalogInput
Reads the EtherCAT I/O analog input.
MMC_LIB_API int MMC_ECATIOReadAnalogInput(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READAI_IN* pInParam,
OUT MMC_READAI_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_ECATIO_API.h
GMAS Programming(IEC 61331 Program)\ElmoECATIO
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
Points to the MMC_READAI input data structure using the
MMC_ECATIOReadAnalogInput function.
pOutParam
Points to the MMC_READAI_OUT output structure receiving information, as a result of
calling the MMC_ECATIOReadAnalogInput function.
Remarks
None
Scope
All
MMC_READAI_IN Structure
typedef struct mmc_readai_in{
unsigned char ucIndex;
}MMC_READAI_IN;

### PDF page 1584
<a id="pdf-page-1584"></a>
Parameters
ucIndex
Analog input index. Any positive character value.
MMC_READAI_OUT Structure
typedef struct mmc_readai_out{
short sAI;
unsigned short usStatus;
short sErrorID;
}MMC_READAI_OUT;
Parameters
sAI
Analog Input value. Any positive value.
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and 4.6 NC Profiler
Error IDs.
Figure 495 describes the function block for MMC_ECATIOReadAnalogInput as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 495: MMC_ECATIOReadAnalogInput function block
21.7.6.1 Function Block Code Example
int rc;
MMC_READDI_IN stReadDI_in;
MMC_READDI_OUT stReadDI_out;
//
// Inserting the structure parameters:
stReadDI_in.dummy = 1; //dummy input
//
rc = MMC_ECATIOReadAnalogInput (hConn, iAxisRef, &stReadDI_in,
&stReadDI_out);
printf("EtherCAT Analog Input Status[%ld] ErrId[%d]\n", (long
int)stReadDI_out.ulliDI, (short)stReadDI_out.sErrorID);
if (rc != 0)
{
HandleError();

### PDF page 1585
<a id="pdf-page-1585"></a>
}

### PDF page 1586
<a id="pdf-page-1586"></a>
###### 21.7.7 MMC_ECATIOWriteAnalogOutput
21.7.7 MMC_ECATIOWriteAnalogOutput
Writes to the EtherCAT I/O analog outputs.
MMC_LIB_API int MMC_ECATIOWriteAnalogOutput(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEAO_IN* pInParam,
OUT MMC_WRITEAO_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_ECATIO_API.h
GMAS Programming(IEC 61331 Program)\ElmoECATIO
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
Points to the MMC_WRITEAO input data structure using the
MMC_ECATIOWriteAnalogOutput function.
pOutParam
Points to the MMC_WRITEAO_OUT output structure receiving information, as a result
of calling the MMC_ECATIOWriteAnalogOutput function.
Remarks
None
Scope
All
MMC_WRITEAO_IN Structure
typedef struct mmc_writeao_in{
short sAO;
unsigned char ucIndex;
}MMC_WRITEAO_IN;

### PDF page 1587
<a id="pdf-page-1587"></a>
Parameters
sAO
Analog Output value. Any positive value.
ucIndex
Analog input index. Any positive character value.
MMC_WRITEAO_OUT Structure
typedef struct mmc_writeao_out{
unsigned short usStatus;
short sErrorID;
}MMC_WRITEAO_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 496 describes the function block for MMC_ECATIOWriteAnalogOutput as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 496: MMC_ECATIOWriteAnalogOutput function block
21.7.7.1 Function Block Code Example
int rc;
MMC_WRITEDO_IN stWriteDO_in;
MMC_WRITEDO_OUT stWriteDO_out;
//
// Inserting the structure parameters:
stWriteDO_in.ulliDO = 1; //Index of the group axes
//
rc = MMC_ECATIOWriteDigitalOutput (hConn, iAxisRef, &stWriteDO_in,
&stWriteDO_out);
if (rc != 0)
{
HandleError();
}

### PDF page 1588
<a id="pdf-page-1588"></a>
###### 21.7.8 MMC_ECATIOWriteDigitalOutput
21.7.8 MMC_ECATIOWriteDigitalOutput
Writes to the EtherCAT I/O outputs of all 64 bit I/O's in one action, increasing the communication speed
proportionately versus writing to 8 x groups of 8 I/O's.
MMC_LIB_API int MMC_ECATIOWriteDigitalOutput(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEDO_IN* pInParam,
OUT MMC_WRITEDO_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_ECATIO_API.h
GMAS Programming(IEC 61331 Program)\ElmoECATIO
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
MMC_ECATIOWriteDigitalOutput function.
pOutParam
Points to the MMC_WRITEDO_OUT output structure receiving information, as a result
of calling the MMC_ECATIOWriteDigitalOutput function.
Remarks
None
Scope
All
MMC_WRITEDO_IN Structure
typedef struct{
#ifdef WIN32
unsigned __int64 ulliDO;

### PDF page 1589
<a id="pdf-page-1589"></a>
#else
unsigned long long int ulliDO;
#endif
}MMC_WRITEDO_IN;
Parameters
__int64 ulliDI or ulliDI
If function is defined for WIN32 then use __int64 ulliDI, else use ulliDI. Any positive,
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
Figure 497 describes the function block for MMC_ECATIOWriteDigitalOutput as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 497: MMC_ECATIOWriteDigitalOutput function block
21.7.8.1 Function Block Code Example
int rc;
MMC_WRITEDO_IN stWriteDO_in;
MMC_WRITEDO_OUT stWriteDO_out;
//
// Inserting the structure parameters:
stWriteDO_in.ulliDO = 1; //Index of the group axes
//
rc = MMC_ECATIOWriteDigitalOutput (hConn, iAxisRef, &stWriteDO_in,
&stWriteDO_out);
if (rc != 0)
{
HandleError();
}
