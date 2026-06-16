# Chapter 1 Introduction

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 29-39
- Chunk: `003_p0029-p0039_Chapter-1-Introduction.md`

## Active Outline At Chunk Start
- p. 29 - Chapter 1 Introduction
  - p. 29 - 1.1 What the API Does

## Contained Bookmark Outline
- p. 29 - Chapter 1 Introduction
  - p. 29 - 1.1 What the API Does
  - p. 30 - 1.2 Maestro over (Go) Standard
  - p. 30 - 1.3 Terminology
  - p. 35 - 1.4 How to Use this Document

## Extracted Text

### PDF page 29
<a id="pdf-page-29"></a>
#### Chapter 1 Introduction
##### 1.1 What the API Does
Chapter 1 Introduction
This document describes the administration and motion API of the Elmo Maestro motion controllers (Gold
Maestro and Platinum Maestro). The Maestro is Elmo's family of Network Motion Controllers. They are
network-based and RPC (or IPC) operating in conjunction with Elmo's intelligent servo drive family, to provide a
total network motion controller solution.
The Maestro is designed to support the SimplIQ Line servo drives, based on standard CAN Open network
architecture, as well as the Gold and Platinum Line families, with EtherCAT networking.
As true network controllers, Elmo's Maestro family, SimplIQ, and servo drives, share the motion processing
workload in distributed motion control architecture. The best servo performances are achieved by combining
Elmo's servo drives, and the new real-time motion control capabilities of the Maestro main controllers.
The Maestro operates as a Network Motion Controller to support:
- Full, Real-Time, Multi-Axis motion synchronization
- Advanced user programming capabilities based on well-known standards
- Deterministic control over Motions, IO's, and processes in the system
1.1 What the API Does
The purpose of the PLCopen Motion API is to produce a standardized motion control API solution without
compromising on system performances, based on the PLCopen Standard. This is achieved by the Maestro
software architecture, since the low-level controller real-time motion engine directly implements the PLCopen
motion API, therefore no intermediate layers are required, and performances are optimal.
Software interfaces to the Maestro Motion API are implemented to the users' convenience, via a dedicated API
library. This library supports the following:
- Interfacing the Maestro PLCopen Motion API from a host computer via Ethernet TCP/IP
- Interfacing the Maestro PLCopen Motion API from user programs, running on the Maestro product
An identical API library is used at the Development / Host PC with the API library accessing the same API server
to perform the desired user operations.
This same Maestro API operates with EtherCAT communication and for the SimplIQ family of netwo rk
controllers.
The user has the ability to store such programs on the Maestro FLASH, and to run them at power -up. This
naturally results in a faster and more optimized method of operating the Maestro motion API.

### PDF page 30
<a id="pdf-page-30"></a>
##### 1.2 Maestro over (Go) Standard
##### 1.3 Terminology
1.2 Maestro over (Go) Standard
The Maestro offers real-time motion control support, for full multi-axis system synchronization, using the
industry interface PLCopen for Motion Control standard. This is the Maestro over standard, applicable to the
Platinum Line (Platinum Maestro), Gold Line (Gold Maestro), and SimplIQ Line of motion controllers. The use of
native C and C++ programming support (run on the Maestro target) dramatically accelerates execution of user
level programs, while maintaining the same PLCopen Motion API definitions as a standard softw are API.
The operation of C and C++ based programs is optimal, and will result in the best overall system performance,
since they generate machine code that runs directly on the target Maestro hardware processor.
1.3 Terminology
The terminology used in this document covers language used throughout the servo drive, controller, and
communication industry and are not necessarily specific to Elmo Motion Control Ltd.
Term Explanation
ACS Axes Coordinate System:
The system of coordinates related to the physical motors.
Axis Axis is the most basic motion object and is used to control the motion of a single
motor/axis.
Blending A method for consecutive function blocks to cooperate in the transition from the first
to the next.
CAM Table Content addressable memory (CAM) table refers to a dynamic table in the Maestro
Shared Memory.
CAM The cam can be seen as a device that rotates from spherical to reciprocating (or
sometimes oscillating) motion. A common example is the camshaft of an automobile,
which takes the rotary motion of the engine and translates it into the reciprocating
motion necessary to operate the intake and exhaust valves of the cylinders.

The CAM maps the master's position to the ECAM Slave's position.
CAN Controller Area Network. Data link layer protocol for serial communication as specified
in ISO 11898-1 (1999).
CiA CAN in Automation international users and manufacturers group e.V. It is a non -profit
association promoting Controller Area Network (CAN).
COB Communication Object, consisting of one or more CAN frames. Any information
transmitted via CANopen has to be mapped into COBs.
COB-ID COB-Identifier. Identifies a COB uniquely in a CAN network. The identifier also
determines the priority of that COB in the data link layer.
CoE CANopen over EtherCAT. Defines a standard way to access the CANopen protocol and
includes an object dictionary, SDO, PDO, and emergency messages.
Contour curve An inserted curve that modifies the original path. It is the resulting curve after

### PDF page 31
<a id="pdf-page-31"></a>
Term Explanation
blending.
Coordinate system The reference system in which a coordinate or path is described.
Corner deviation The shortest distance between the programmed corner point and the contour curve.
Corner distance Distance of the start point of the contour curve to the programmed target po int.
Coupling The stage in which the master and ECAM Slave are synchronized by CAM process (aka
Engagement). It occurs when the ECAM Master reaches the MasterSyncPosition (see
the Electronic CAM chapter).
Decoupling The stage where CAM process ended. That is to say, ECAM Master and ECAM Slave
synchronization is ended (aka Disengagement).
Direction The orientation components of a vector in space.
Note: This is different from the MC_Direction input.
Drive A unit controlling a motor via the current and timing in its coils.
ECAM Master Axis, which functions as master in ECAM operation (aka CAM process).
ECAM Slave Axis, which functions as slave in ECAM operation (aka CAM process).
EoE Ethernet over EtherCAT. Fully Ethernet compatible and defines a standard way to
exchange or tunnel standard Ethernet frames. Used to create Maestro Master and
Drives as Slaves in EASII and other applications for both diagnostics and download of
files.
FB Function Block
FIFO First In, First Out. An abstraction in ways of organizing and manipulation of data
relative to time and prioritization. This expression describes the principle of a queue
processing technique or servicing conflicting demands by ordering process by first -
come, first-served (FCFS) behavior: what comes in first is handled first, what comes in
next waits until the first is finished, etc.
FoE File over EtherCAT. Similar to TFTP, enables access to any data structure in the device,
and defines a standard way to download and upload firmware and other files over the
EtherCAT network. Used as a download of hardware configuration files and updates.
Refer to the functions MMC_DownloadFoE, and MMC_GetFoEStatus
G-MAS Gold Maestro Application Software also known as the Gold Maestro Network Motion
Controller performs synchronized multi axis motions in the system (such as circle, line
etc.), using a real time communication protocol so that all drives are synchronized to a
specific SYNC signal in the system It operates as a master, independent of any host
system. In operational mode, it periodically sends data to the slaves that may override
the data that a user sends from a host system.
GoS G-MAS over SimplIQ
Group Group of axes
Group-FB The set of function blocks that can operate on a group of axes.

### PDF page 32
<a id="pdf-page-32"></a>
Term Explanation
HPT High Priority Task as against LPT (Low Priority Task), and MPT (Medium Priority Task),
used in Embedded Linux, RTOS, and Parallel Programing for programming in robotics.
This refers to the task priority in running threads, which may or may not lock
resources.
In-CAM A situation in which the ECAM Master reaches the 'Start Position' within CAM table,
regardless of whether the ECAM Master and ECAM Slave are synchronized. As said
above, at this phase it is equivalent to synchronization.
IO Input and output
IPC Inter-process communication (IPC) is a set of techniques to exchange data among
multiple threads in one or more processes. Processes may be running on one or more
network-linked systems. IPC techniques are divided into methods for passing
messages, synchronization, shared memory, and remote procedure calls (RPC). IPC
may vary, depending on the bandwidth and communication latency between the
threads, and the type of data being communicated. C Programs located on the
Maestro use the IPC method.
Maestro Refers to the Gold Maestro and Platinum Maestro motion controllers.
Masking A form of cloaking of communication addresses. The netmask is a bitm ask used to
separate the bits of the network identifier from the bits of the host identifier. It is
written in the same notation used to denote IP addresses.
MCS Machine Coordinate System:
The system of coordinates that is related to the machine. Sometimes called World
Coordinate System or Base Coordinate System.
With Cartesian built machines, MCS is a Cartesian Coordinate system The coordinate
system from the physical multiple axes ACS is linked to the MCS via a kinematic
transformation (forward and backward conversion).
Motor An actuator focused to a movement, converting electrical energy into a force or
torque.
Mutex Mutual exclusion. Mutual exclusion algorithms are used in concurrent programming to
avoid the simultaneous use of a common resource, such as a global variable, by pieces
of computer code called critical sections. A critical section is a piece of code in which a
process or thread accesses a common resource. The critical section by itself is not a
mechanism or algorithm for mutual exclusion. A program, process, or thread can have
the critical section in it without any mechanism or algorithm, which implements
mutual exclusion.
Orientation The rotational components of a vector in space.
Path Set of continuous positions and orientation information in multi-dimensional space.
This may be geometrically described as a space curve that the axes group TCP moves
along.
Path Data Description of a path, which can include additional information like velocity and
acceleration.

### PDF page 33
<a id="pdf-page-33"></a>
Term Explanation
PDO Process Data Object
PDS Power Drive System
Platinum Maestro Platinum Maestro Application Software (GMAS) also known as the Platinum Maestro
Network Motion Controller performs synchronized multi axis motions in the system
(such as circle, line etc.), using a real time communication protocol so that all drives
are synchronized to a specific SYNC signal in the system It operates as a master,
independent of any host system. In operational mode, it periodically sends data to the
slaves that may override the data that a user sends from a host system.
Position Position means a point in space that is defined by different coordinates. Depending on
the used system and transformation, it can consist of up to six dimensions
(coordinates), three Cartesian coordinates in space and three coordinates for the
orientation. In ACS, there can be even more than six coordinates. If the same position
is defined in different coordinate systems, the values of the coordinates are different.
PVT Position velocity time interpolation mode
RapidXml RapidXml is an attempt to create the fastest XML DOM parser possible while retaining
usability, portability and reasonable W3C compatibility. It is an in -situ parser written in
C++, with parsing speed approaching that of strlen() function executed on the same
data
RPC A remote procedure call (RPC) is an inter-process communication with the Maestro
host allowing a program to initiate a subroutine or procedure. The programmer
essentially writes the same code whether the subroutine is local t o the executing
program, or remote. For example, the EASII uses RPC to communicate with the
Maestro family motion controllers.
RPDO Receive Process Data Object. Communication object of a device, which contains
output data.
S point Incremental Position along the path
Scara A special kinematic for robot or handling applications.
SDO Service Data Object. Peer-to-peer communication with access to the Object Dictionary
of a CANopen device.
Speed Speed is the absolute value of the velocity without direction.
Start Position ECAM Master position in which master may start a Ramp-In process before Coupling.
It is defined as a backward distance from Sync Position. At this phase we do not
support Ram-In for CAM process, therefore Start Position for the time being is
equivalent to Sync Position (see above).
Sync Position Master position in which ECAM Master and ECAM Slave should be synchronized.
Synchronization Combines an axis or axes group (as slave) with an axis as master in order for the slave
to execute its synchronized path with the progress of the master, and therefore linked
to a single-dimension source for synchronization.

### PDF page 34
<a id="pdf-page-34"></a>
Term Explanation
TCP Tool Centre point, the point in the machine that is commanded to move, typica lly to
the center or the head of the tool. It can be described in different coordinate systems.
TPDO Transmit Process Data Object. Communication object of a device, which contains input
data.
Tracking Is characterized by an axis group that tracks with its movement, the movement of
another axis group.
Trajectory Time dependent description of the path the TCP of an axes group moves along.
Additionally to the geometrical description of the space curve, time dependent state
variables like velocity, acceleration, jerk, forces etc. are also specified.
Transition The curve / ARC between two blended successive FBs.
Velocity For a group of axes this means:
For ACS, the velocities of the different axes.
For MCS and PCS it provides the velocity of the TCP.
XML Extensible Markup Language (XML) is a markup language that defines a set of rules for
encoding documents in a format that is both human-readable and machine-readable
XML DOM Document Object Model (DOM), is an application programming interface (API) for a
valid HTML and well-formed XML documents.

### PDF page 35
<a id="pdf-page-35"></a>
##### 1.4 How to Use this Document
1.4 How to Use this Document
This document allows a programmer to write a program in C, C++, and IEC 61331, to communicate and operate
the Maestro Network Motion Controllers. The document is designed to aid the programmer in setting specific
parameters for the appropriate function blocks used by the customer. This section describes how to use the
detailed information within each function block, set, and customize its parameters.
The chapters in this document are divided according to the following sections:
Chapter 1: Introduction - An Introduction to Maestro and this document.
Chapter 2: Maestro Overview - This chapter explains the operational modes of the Maestro.
Maestro Hardware Connections - Maestro Hardware and Software Limits Handling.
Explanation of the function block behavior when queued.
Chapter 3: Maestro Hardware Connections - Instructions on how to connect to Maestro
Chapter 4: Error Handling - All Maestro and Servo Drive errors, warnings lists by ID code with possible
reasons and recommendations.
Chapter 5: Motion and Administrative - Description - Motion and Administrative Function Blocks,
describes the various single and multiple axes, including their transition modes.
Chapter 6: Motion and Administrative - Single Axis - Motion and Administrative Function Blocks,
details the various single axis function blocks.
Chapter 7: Motion and Administrative - Multi-Axis - Motion and Administrative Function Blocks,
details the various multiple axes function blocks.
Chapter 8: Position, Velocity, Time (PVT) Motion - PVT Motion explained with its applicable functions.
Chapter 9: Electronic CAM - Electronic CAM processes
Chapter 10: API Services and Operations - API Services and Operations, describes the main general
function blocks referring to the following:
- Main configuration variables
- Data Recording. Refer to section Data Recording Functions for further details.
- Resource file uploading and downloading
- Download of new firmware version
Chapter 11: Process Image(PI) - EtherCAT Process Image (PI) explanation and functions
Chapter 12: Data Recording - Data Recording. This allows the user to record internal controller
variables, store them in local a temporary array, and upload them to a host computer using
either one of the controller's communication channels.
Chapter 13: Bulk Parameters Reading - Bulk Parameters Reading to perform reading of parameters
from multiple drives with their relevant functions.
Chapter 14: API Events (C & C++) - API Events, including the mechanism to handle events in the
Maestro.
Chapter 15: Error Correction Mechanism - Describes the mechanism to correct drive position errors,

### PDF page 36
<a id="pdf-page-36"></a>
and the functions which are used to apply the correction.
Chapter 16: Maestro Hardware and Software Limits Handling - description and details.
Chapter 17: Saving Maestro User Program Parameters
Chapter 18: Network Connectivity and Configuration - The chapter on Network Connectivity, describes
all the network communications to the Maestro server.
Chapter 19: Host Communication - Modbus (Host) Communication describes and details all the Modbus
connectivity and configuration communications to the Maestro server.
Chapter 20: CANbus Drive Communication - CANbus Drive Communication describes and details all the
CANbus connectivity and configuration communications to the Maestro server.
Chapter 21: DS-401 CANbus I/O Communications - DS-401 CANbus I/O Communication Connectivity
and Configuration describes and details all the DS-401 CANbus communications to the
Maestro server.
Chapter 22: EtherCAT Drive Communication - Describes and details all the Network, Modbus (Host),
CANbus (drive), EtherCAT (drive), Interpreter Command, and EtherNETIP communications
to the Maestro server.
Chapter 23: Interpreter Command Functions - Connectivity and Configuration contains all the Network,
Modbus (Host), CANbus (drive), EtherCAT (drive), Interpreter Command, and EtherNETIP
communications to the Maestro server.
Chapter 24: Chapter 23EtherNetIP Communication - Connectivity and Configuration contains all the
Network, Modbus (Host), CANbus (drive), EtherCAT (drive), Interpreter Command, and
EtherNETIP communications to the Maestro server.
Chapter 25: Programming in C++ - Programming in C++ with equivalent functions based on the C
functions. These are wrapper functions using similar parameter details as their similarly
named C functions.
Chapter 26: IEC 61131-3 Special Functions - IEC 61131-3 Special Functions unique to IEC using simple
API functions
Each chapter describes function blocks and their parameters according to the API source files described in the
Maestro Overview chapter. Some chapters have specific parameters that are applicable throughout a section
and are therefore explained prior to the function block listings for that section. For example, the section Axis
Status contains definitions of the Axis Status Bit Masks, whose variables are used as enumerator values in
most function blocks. Certain parameters only apply within a specific function block and their details are
recorded after the definitions of that function block. For example, the explanation of Homing Functions is
detailed after the function block MMC_Home.

### PDF page 37
<a id="pdf-page-37"></a>
Figure 1: Function block layout example
Each function block (Figure 1: ) begins with a section that contains the function block's title and a short
explanation of the function. The Description explains the usage of the function block with the Scope describes
the conditions for its usage. Motion Mode defines whether the function block is supported in NC or Distributed
(Non-NC) mode.
NOTE: Links are highlighted in blue bold.
Source defines the file source of the function block, with the Function Parameters describing the main
parameters of the function itself. The logical definition must be retained in order for C to read the parameters
of the function block correctly.
Remarks describe the function and its usage in detail, with their respective.

### PDF page 38
<a id="pdf-page-38"></a>
Figure 2: Function block layout example (cont.)
The input and output structures for each function(Figure 2) display the accepted structure for the input and
output with their respective parameters. The parameters and sub -parameters and listed on the left side with
their various descriptions opposite, and any references to other sections. The description describes the usage
of each parameter and sub-parameter. With sub-parameters, the description defines specific values or
enumerator values for the parameters with their explanation and/or a reference to such.

### PDF page 39
<a id="pdf-page-39"></a>
Figure 3: diagrammatic explanation and example of a function block
At the end of each function block detailed description (Figure 3), is a diagram showing its inputs/outputs.
Further information is provided with a real program C code, and implementation, examples of its usage.
