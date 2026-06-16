# Chapter 2 Maestro Overview

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 40-48
- Chunk: `004_p0040-p0048_Chapter-2-Maestro-Overview.md`

## Active Outline At Chunk Start
- p. 40 - Chapter 2 Maestro Overview

## Contained Bookmark Outline
- p. 40 - Chapter 2 Maestro Overview
  - p. 45 - 2.1 Using the EASII Application
  - p. 45 - 2.2 Maestro Operation Modes
    - p. 46 - 2.2.1 NC Motions
    - p. 46 - 2.2.2 Distributed / Standard DS-402 (stand-alone) Drive
    - p. 46 - 2.2.3 Maestro Axes and Node Definitions
    - p. 48 - 2.2.4 Maestro to Servo Drive Interfaces

## Extracted Text

### PDF page 40
<a id="pdf-page-40"></a>
#### Chapter 2 Maestro Overview
Chapter 2 Maestro Overview
The Maestro general communication and API architecture is described below in Figure 4.

Figure 4: GoS System Software Structure - Development and Application

### PDF page 41
<a id="pdf-page-41"></a>
The Maestro system supports the following programing interfaces:
Operation System Library
Hosts (TCP/IP) .NET Library
Win32 Library - C and C++ Libraries
Internally (IPC) C and C++ Libraries
IEC 61131-3
VxWorks Host .NET Library
Win32 Library - C and C++ Libraries
The Maestro API implements direct binary communication interface using the TCP/IP and internal connection
shown in Figure 4. Using the direct binary API is a faster and more efficient connection method to produce best
system performances. The underlying Motion API is the PLCopen (the API server), and Elmo's Software API's
above it, which export the same functions in both cases. An identical API library is used. Elmo provides t he
dll/lib for Win32 based environments and a library for interfacing the Maestro from within the Maestro, when
writing user programs. The only difference is the initial "Initialization" method. The API library accesses the
same API server to perform the desired user operations.
The API defines a set of function blocks, with the following attributes:
Attribute Explanation
Simplicity Ease of use, towards the application program builder and installation & maintenance .
Efficiency In the number of function blocks, directed to efficiency in design and understanding.
Consistency Conforms to the IEC 61131- 3 standard.
The IEC function blocks reflect the actual function block's as they appear in the IEC
window of the Elmo Application Studio (EAS) application.
Therefore the 'C' function output parameters by the name of usStatus includes Error -
a 1 bitwise parameter, Done, etc., when compared with the same IEC function block,
the IEC version should display and include all relevant bits (error, done, busy, etc...).
Elmo's IEC 61131- 3 function blocks and functions have all array parameters on the
input side whether or not it functions as input or output parameter, and immaterial
whether the C function equivalent has the same parameters as input or output.
It should be noted that while every effort is made to make sure that all C functions
conform to the outputs of IEC functions, in practice, this is not always practical due to
the nature of the IEC functions.
Universality Hardware independent
Flexibility Future extensions / range of application
These function blocks are sectioned according to their motion axes, and communication protocols. The API
therefore consists of a series of grouped source files divided by the following subjects:
General Source: GMAS\includes\MMC_general_API.h
Includes all main configuration and firmware download
function blocks.

### PDF page 42
<a id="pdf-page-42"></a>
Main Definitions Source: GMAS\includes\MMC_definitions.h
Includes all main and basic definitions for the function
blocks.
Single Axis Motion Source: GMAS\includes\MMC_PLCopen_single_API.h
Includes administrative and motion function blocks
involved in the single axis motion.
Group Axes Motion Source: GMAS\includes\MMC_PLCopen_group_API.h
Includes administrative and motion function blocks
involved in multi-axes motion.
Position, Velocity, Time (PVT) Motion The PV/PVT special motion class describes the path given by
positionnegativelocity pairs per axis, and an optional time
interval given per system (expressing as time per axis is
unnecessary), and is applicable both for single and multi-
axis motion.
Electronic CAM Cam links a master to one or more slaves in a position /
position mode
API Services and Operations This chapter describes the API services and operations for
the Maestro, and involves the following:
- Main configuration variables
- Maestro Preoperational Mode
- EtherCAT Configuration Mode
- Data Recording.
- Resource file uploading and downloading
- Download new firmware version
Process Image (PI) Process Image is the cyclic data transferred between the
EtherCAT Master to the Slave. The PI mechanism allows the
user to read and write this cyclic data.
Data Recording
Bulk Parameters Reading Source: GMAS\includes\
Includes functions to read multiple parameters from
multiple axis at the same instant.
API Events (C & C++) Source: GMAS\includes\MMC_events_API.h
Includes function blocks that read and write events to and
from the Maestro.
Error Correction Mechanism Source: GMAS\includes\MMC_ErrorCorr_API.h
Includes error correction functions for 1D, 2D and 3D
modes.
Maestro Hardware & software Limits
Handling

Saving Maestro User Program

### PDF page 43
<a id="pdf-page-43"></a>
Parameters
Network Connectivity and
Configuration
Source: GMAS\includes\MMC_network_API.h
Includes all basic network functions blocks necessary to
communicate with the Maestro Network Motion Controller.
Host Communication Source: GMAS\includes\MMC_host_comm_API.h
Includes all Modbus function blocks necessary to
communicate with the Maestro Network Motion Controller.
CANbus Drive Communication
DS-401 CANbus I/O Communications Source: GMAS\includes\MMC_DS401_API.h
Includes major DS-401 communication functions for DI and
DO intended for I/O modules.
EtherCAT Drive Communication Source: GMAS\includes\MMC_ECATIO_API.h
Includes major EtherCAT communication functions for
analog and digital I/Os.
Interpreter Command Functions
EtherNetIP Communication
C++ Functions Source: GMAS\includes\CPP\MMCXXXXXXX.h
Includes all C++ class function mirroring functions and
function blocks described in detail for C programming
IEC 61131-3 Special Functions

### PDF page 44
<a id="pdf-page-44"></a>
Detailed Development/Host PC
Gold Maestro (Linux OS)
Gold Maestro Firmware
TCP/IP
EtherCAT or CAN Bus
User C
Applications
TCP/IP
Gateway
G-MAS Programming
Eclipse
G-MAS C IDE
G-MAS C Programming
and Debugging
Gateway
G-MAS Library
TCP/IP
Elmo Application Studio (EAS)
System Configuration, used to configure the
EtherCAT process data and network
Application, Setup, Configuration, Tuning
G-MAS API
Program
EtherCAT

Figure 5: GoS System Software Structure -Host PC Development
In addition, since the Maestro operates as a master, independent of any host system, in operational mode, it
periodically sends data to the slaves, that may override the data that a user sends from a host system.
Therefore, for example, the user cannot tune an axis if the axis is in operational mode. To prevent this and
allow the Elmo Application Studio (EAS) application (Figure 5) to operate via the Maestro CANbus and
EtherCAT, specific API functions are called to change the Maestro operation and allow these applications to
function.
The following table lists the function blocks called as part of a special API to allow the EAS application to
function and revert the Maestro to operational mode when their operation is completed:
Application Function Blocks

### PDF page 45
<a id="pdf-page-45"></a>
##### 2.1 Using the EASII Application
##### 2.2 Maestro Operation Modes
Application Function Blocks
EAS MMC_ChangeToPreOPMode
MMC_ChangeToOperationMode
GetGMASOperationMode
MMC_EnableEthercatConfigMode
MMC_DisableEthercatConfigMode
MMC_IsEthercatConfigMode
Important: It should be noted that connecting to the Maestro is only allowed using one user application at
a time. Connecting two applications to the Maestro in parallel may cause serious problems to the Maestro
library. When performing multiple RPC connections to the Maestro, the multiple connections must be opened
from the same user application.
2.1 Using the EASII Application
For the EAS application to monitor and perform motions, the Maestro cannot ope rate in the background. The
special API function MMC_ChangeToPreOPMode changes the EtherCAT and CANbus communication from the
Maestro to Pre-Operation mode, causing the following:
Communication Operation
EtherCAT No process cycle operates
CANbus No outputs via CAN, and no state machines run
These API functions change the Maestro mode so that the Maestro operation is transparent and no messages
transfer between the Maestro and the drives.
In order to configure the EtherCAT network (EtherCAT Configuration Mode) via the EAS application, the
Maestro must be set to EtherCAT Configuration mode. The user is then able to perform the operations. The API
then employs the specific functions to change the Maestro back to operational mode.
2.2 Maestro Operation Modes
To optimize the device network usage, the Maestro supports two modes of operating axes present on the
Device Network:
- NC Axes - for Numeric Control Axes
- Distributed - for axes not under strict numeric control
The main difference between these modes is the way the motion profile is calculated, and as a result, the
synchronization level achieved.
In general, for axes not requiring low level (network) motion synchronization, the Distributed mode should be
used, allowing the servo drives to generate their own motion trajectory, thus reducing network load. In this
case, synchronized motions like ECAM, based on an external master encoder can still be executed. For highly
synchronized motions, generated by the Master controller (referred to under t he PLCopen definitions as group
vector motions), the NC mode should be used.

### PDF page 46
<a id="pdf-page-46"></a>
###### 2.2.1 NC Motions
###### 2.2.2 Distributed / Standard DS-402 (stand-alone) Drive
###### 2.2.3 Maestro Axes and Node Definitions
2.2.1 NC Motions
In this mode, the Maestro controls the motion, handling the axis (and motion) State (as defined by the PLCopen
Standard), and calculating the motion profile as part of its real-time loop process (NC Cycle). Servo drives
operating with a Maestro master under this mode will run under the DS -402 motion modes e.g.; Interpolated
position, or one of the Cyclic Sync modes (Position/Velocity).
2.2.2 Distributed / Standard DS-402 (stand-alone) Drive
In this mode, the Maestro uses the servo drives own DS-402 operation modes, where the drive itself controls
its own profiling as part of its Real Time process. The Maestro only synchronizes start/stop and general
activation functions but is not responsible to the low-level real-time profile generation.
The Maestro can mix NC and Distributed axes in the same network configuration, thus optimizing usage of
network and processor resources. The definition of the axis type (NC or Distributed) can be changed during
operation using the ChangeOpMode (operation mode) command.
2.2.3 Maestro Axes and Node Definitions
The Maestro controls the following axis types:
- Single Axis as NC axis
- Single Axis as Distributed axis
- Group of axes, as NC axes (only). A group is a collection of axes, which can execute spatial vector motions
In the Maestro architecture, all axes' names have to be defined in advance, in the system resource file, a
dedicated (XML format) file, which defines the following:
- Number of active axes in the system
- For each axis, whether it is operating in NC or Distributed mode.
- Groups of axis must be predefined by the user. However, the link of actual axes to groups can be
performed in run-time (using specific API functions, e.g. AddAxisToGroup() and
RemoveAxisFromGroup()).
- Basic Maestro network cycle (used for NC as well as Distributed), to access the axis position, commands
etc.
For each of the above, the Maestro hold an internal software object Node. Currently, two types of nodes are
defined:
- Single Axis Nodes: NC or Distributed.
- Group (Vector) Nodes: NC only, and can be Group Single axis (NC) only.
Additional Nodes types are supported by the Maestro, such as DS -401 IO, DS-301, and DS-406 modules.
The Max Axes and Node numbers and their combinations are defined as follows:
Tc The Minimal Time of the Master NC Cycle, also known as the Control System Update Rate
In EtherCAT communication, Tc defines the Minimal Distributed Clock Cycle Time of the system or
the Cycle Simultaneous Update Rates:

### PDF page 47
<a id="pdf-page-47"></a>
>= 250 us for up to 16 axes
500 us for 32 axes
1 mSec. for 64 axes
Cycle Jitter: < 1 us, based on Master DC (Distributed Clock) support, for the full network
In CANbus communication, Tc defines the Sync Time of the CAN network:
Cycle Update Rate >= 1 mSec. (CAN physical network limitations only)
Cycle Jitter: < 100 us for CAN Sync message initiation (actual jitter dependent on the CAN
network's physical limitations)
N The max number of Single Axis Nodes in the system. Currently limited to 64 axes (NC and
Distributed, altogether).
V The max number of Group/Vector nodes that can be simultaneously defined in the system.
Currently limited to 16 axes (this is in addition to the 64 single axis nodes).
Va The max number of Group/Vector nodes that can be simultane ously running in the system.
Currently limited to 6 Group/Vectors.
Vn The max number of physical axes that can be simultaneously linked to a specific Group/Vector
node. Currently limited to 16 physical axes.
Mc The max number of devices that can be accessed in a single Master Cycle, via the Communication
link. This number depends if CAN or EtherCAT communications are used. This number also
depends on the current selected Tc.
Mp The max number of NC axes that can be handled in a single Master Cycle. This includes both Single
axis, as well Vector/Group nodes. Generally speaking, Mc and Mp can be different numbers.
Currently, they are equal and limited to 20.
In order to reduce the Maestro cycle computations, each axis can define an axis period and an axis offset time
that is related of course to the Tc base time.
For example: A typical NC system nodes/axes distribution may have:
- 8 Physical axes (nodes: a1 / a8) - all NC.
- 1 group (node v1, linked to physical axes a6, a7, a8).
- Group v1 is running on each master cycle (Tc), calculating a spatial (vector) profiled motion (in 3D space),
and the actual projections to the physical axes linked to it are a6, a7, a8.
- The physical axes nodes are running together as follows:
- a1, a2 are running together, each 2xTc cycles.
- a3, a4 are running together, each 2xTc cycles.
- a5 is running each 4xTc cycles.

### PDF page 48
<a id="pdf-page-48"></a>
###### 2.2.4 Maestro to Servo Drive Interfaces
Figure 6: Typical NC Configuration
In this example, the Max Mp is 6 (although we have 8 axes). The cycle is repeating itself once every 4xTc cycles .
2.2.4 Maestro to Servo Drive Interfaces
The Maestro manages all motion commands sent to the servo drives, via the CANopen DS -402 standard. This is
relevant to the Maestro CAN hardware interface, and to the EtherCAT protocol implementing CoE (CAN Over
EtherCAT).
For axes (Nodes) that operate in NC mode, Maestro uses the DS-402 motion modes: Interpolated position, or
one of the Cyclic Sync modes (Position/Vel).
For axes (Nodes) operating in Distributed mode, where the servo drive manages its own profiler and real -time
motion execution, it is assumed that the servo drive supports the relevant requested motion modes.
Motion Modes that are part of the PLCMotion API definition, but are NOT supported by the DS -402 interface,
will not be available in standard DS-402 servo drives when working in Distributed mode (unless specific Vendor
Types objects are defined, e.g. ECAM in drive level, etc. as implemented for example in Elmo servo drives).
Note: Although the above describes and relates to Elmo DS-402 compatible servo drives, the Maestro
design does not limit the operation of any DS-402 compatible servo drives as well.
