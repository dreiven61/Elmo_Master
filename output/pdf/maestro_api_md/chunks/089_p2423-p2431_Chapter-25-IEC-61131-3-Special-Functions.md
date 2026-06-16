# Chapter 25 IEC 61131-3 Special Functions

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 2423-2431
- Chunk: `089_p2423-p2431_Chapter-25-IEC-61131-3-Special-Functions.md`

## Active Outline At Chunk Start
- p. 2423 - Chapter 25 IEC 61131-3 Special Functions
  - p. 2423 - 25.2 ElmoIECRTVers

## Contained Bookmark Outline
- p. 2423 - Chapter 25 IEC 61131-3 Special Functions
  - p. 2423 - 25.1 ElmoIECLibVers
  - p. 2423 - 25.2 ElmoIECRTVers
  - p. 2424 - 25.3 Elmo_RetainLoad
  - p. 2425 - 25.4 Elmo_RetainSave
  - p. 2426 - 25.5 MMC_SetImmediateExec

## Extracted Text

### PDF page 2423
<a id="pdf-page-2423"></a>
#### Chapter 25 IEC 61131-3 Special Functions
##### 25.1 ElmoIECLibVers
##### 25.2 ElmoIECRTVers
Chapter 25 IEC 61131-3 Special Functions
There are two special functions required by IEC in order to confirm the Library and Run- Time versions during
initiation. These are:
ElmoIECLibVers IEC Library version
ElmoIECRTVers IEC Run time version
25.1 ElmoIECLibVers
Reads the Elmo IEC library version during initiation of the IEC 61131 program.

Source IEC61131 Library\ElmoGlobal
Parameters
@Version
[INPUT] the present version of the library read during initiation of the IEC program. Has
a String value
RC
[OUTPUT] Return code. Short integer value
25.2 ElmoIECRTVers
Reads the Elmo IEC run-time version during initiation of the IEC 61131 program.

Source IEC61131 Library\ElmoGlobal
Parameters
@Version
[INPUT] the present run-time version read during initiation of the IEC program. Has a
String value
RC
[OUTPUT] Return code. Short integer value

### PDF page 2424
<a id="pdf-page-2424"></a>
##### 25.3 Elmo_RetainLoad
25.3 Elmo_RetainLoad
The IEC 61131-3 programming allows variables and their values to be Retained, i.e. saved within the Maestro
for loading when using a specific function that requests or requires these variables. If not previously loaded,
then when function requests them and their values, a popup will appear requesting to run the special
function Elmo_RetainLoad.
Not applicable to Maestro API C and C++ applications. Only for IEC applications.
Motion Mode N/A N/A
Source N/A
Parameters
Enable
[INPUT] enabled. Has Boolean value
Result
[OUTPUT] Result code. Has Boolean value
Remarks
None
Scope
N/A
Figure 563 describes the function for Elmo_RetainLoad as applied within the IEC 61131 programming.

Elmo_RetainLoad
EnableBoolean Result Boolean

Figure 563: Elmo_RetainLoad function

### PDF page 2425
<a id="pdf-page-2425"></a>
##### 25.4 Elmo_RetainSave
25.4 Elmo_RetainSave
The IEC 61131-3 programming allows variables and their values to be Retained, i.e. saved within the Maestro
for loading when using a specific function that requests or requires these variables. The function
Elmo_RetainSave performs this purpose.
Not applicable to Maestro API C and C++ applications. Only for IEC applications.
Motion Mode N/A N/A
Source N/A
Parameters
Enable
[INPUT] enabled. Has Boolean value
Result
[OUTPUT] Result code. Has Boolean value
Remarks
None
Scope
N/A
Figure 564 describes the function for Elmo_RetainSave as applied within the IEC 61131 programming.

Elmo_RetainSave
EnableBoolean Result Boolean

Figure 564: Elmo_RetainSave function

### PDF page 2426
<a id="pdf-page-2426"></a>
##### 25.5 MMC_SetImmediateExec
25.5 MMC_SetImmediateExec
Function blocks are collected in the Maestro queue with their execution bit turned OFF. As each function
block is executed, it is immediately turned ON. The IEC program may set the Intermediate flag to insert
several function blocks to the queue (invocation of these FBs), and then set the Intermediate flag to ON, for
the Maestro to execute them all.
Not applicable to Maestro API C and C++ applications. Only for IEC applicat ions.
Motion Mode N/A N/A
Source GMAS Programming(IEC 61331 Program)\ElmoGenAxis
Parameters
N/A
Remarks
None
Scope
N/A
Figure 565 describes the function for MMC_SetImmediateExec as applied within the IEC 61131 programming.

MMC_SetImmediateExec
ImmediateBoolean
@Axis
ExecuteBoolean
usStatus
Done, Busy, Active,
CommandAborted, Error
usErrorID
Bitwise
Error code

Figure 565: MMC_SetImmediateExec function

### PDF page 2427
<a id="pdf-page-2427"></a>
25.5.1 Tracking System Functions
The Tracking mechanism described in the sections PCS - Product Coordinate System, Tracking in Dynamic
Coordinate Transformations, Tracking Workpiece Processing on a Conveyor Belt, and is implemented using the
following functions described in the next sections:
- MC_TrackConveyorBelt
- MC_TrackRotary

### PDF page 2428
<a id="pdf-page-2428"></a>
25.5.2 MC_TrackConveyorBelt
The following table describes the input and outputs.
name Type Description
Input - Output
GroupAxis AXIS_REF Reference of group axis
The Group executing the Ramp and Track. It is defined
as the "Slave" in the Track mode.
ConveyorBelt AXIS_REF The axis reference of the master that is to follow (the
conveyor belt in this case, single axis)
Input
Execute BOOL Operates on rising edge.
ConveyorBeltOrigin ARRAY[6] of LREAL Static origin & orientation of the rotary relative to
MCS.
An array of 6 parameters,
(
transformation. This Transformation defines the Static
Origin & Orientation of the conveyor belt coordinate
system (CB base point and orientation - bottom-left
for instance), relative to the MCS.
PCSOrigin ARRAY[6] of LREAL
[x,y,z,u=0,v=0,w]
Initial position within the rotary table coordinate
system relative to ConveyorBeltOrigin.
InitialObjectPosition ARRAY[6] of LREAL
[x,y,z,u=0,v=0,w]
Position & orientation of static part within the moving
PCS.
AutoSyncReference BOOL States whether the MasterSyncPosition should relate
to MasterInitialPosition (0) or to conveyor belt position
at activation time (1).
MasterInitialPosition LREAL Master Initial Position (
), at the exact time/location in
which also the Initial osition within the Rotary Table
CS" was provided (PCS)
MasterSyncPosition LREAL Relative Position in which the Sync Position is defined
MasterScaling LREAL Scaling on master value (w) - UU/rad on master axis
BufferMode MC_BUFFER_MODE_ENU
M
Buffered/Abortion only
RotAngleUnits ENUM Deg/RAD - Existing ENUM
RampTrajectoryParam ARRAY[...] of LREAL These parameters define the ramp behavior for safety

### PDF page 2429
<a id="pdf-page-2429"></a>
s [Z Safe Height, t1-t4, spare
7 doubles]
according to user's preferences.
TrajectoryMode TRAJECTORY_MODE_ENU
M
For future use.
Output
Done BOOL The ramp transformation completed successfully
Busy BOOL Function block is listed (queued) but not yet active.
Active BOOL In ramp motion as long as no MCS or abortion was
applied
CommandAborted BOOL Command is aborted by another command which
changes the transformation for the selected coordinate
system. This might be another RotaryTable, a
ConveyorBelt or MC_SetCartesianTransform or
MC_SetCoordinateTransform.
Error BOOL Signals that an error has occurred within the function
block
ErrorID INT Error identification(0 if no error)

### PDF page 2430
<a id="pdf-page-2430"></a>
25.5.3 MC_TrackRotary
The following table describes the input and outputs.
Name Type Description
Input - Output
GroupAxis AXIS_REF Reference of group axis
The Group executing the Ramp and Track. It is
defined as the "Slave" in the Track mode.
RotaryTable AXIS_REF The axis reference of the master that is to
follow (the conveyor belt in this case, single
axis)
Input
Execute BOOL Operates on rising edge.
RotaryTableOrigin ARRAY[6] of LREAL Static origin & orientation of the rotary
relative to MCS.
An array of 6 parameters,
(
transformation. This Transformation defines
the Static Origin & Orientation of the Rotary
Table coordinate system (CB base point and
orientation - bottom-left for instance),
relative to the MCS.
PCSOrigin ARRAY[6] of LREAL
[x,y,z,u=0,v=0,w]
Initial position within the rotary table
coordinate system relative to
RotaryTableOrigin.
InitialObjectPosition ARRAY[6] of LREAL
[x,y,z,u=0,v=0,w]
Position & orientation of static part within the
moving PCS.
AutoSyncReference BOOL States whether the MasterSyncPosition
should relate to MasterInitialPosition (0) or to
rotary position at activation time (1).
MasterInitialPosition LREAL

Master Initial Position (
), at the exact
time/location in which also the Initial Position
within the Rotary Table CS was provided (PCS)
MasterSyncPosition LREAL

Relative Position in which the Sync Position is
defined
MasterScaling LREAL Scaling on master value (w) - UU/rad on
master axis
BufferMode MC_BUFFER_MODE_ENUM Buffered/Abortion only

### PDF page 2431
<a id="pdf-page-2431"></a>
RotAngleUnits ENUM Deg/RAD - Existing ENUM
RampTrajectoryParams ARRAY[...] of LREAL
[Z Safe Height, t1-t4, spare 7
doubles]
These parameters define the ramp behavior
for safety according to user's preferences.
TrajectoryMode TRAJECTORY_MODE_ENUM For future use.
Output
Done BOOL The ramp transformation completed
successfully
Busy BOOL Function block is listed (queued) but not yet
active.
Active BOOL In ramp motion as long as no MCS or abortion
was applied
CommandAborted BOOL Command is aborted by another command
which changes the transformation for the
selected coordinate system. This might be
another RotaryTable, a ConveyorBelt or
MC_SetCartesianTransform or
MC_SetCoordinateTransform.
Error BOOL Signals that an error has occurred within the
function block
ErrorID INT Error identification(0 if no error)
