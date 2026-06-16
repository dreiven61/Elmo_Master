# Chapter 15 Error Correction Mechanism

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 1303-1326
- Chunk: `052_p1303-p1326_Chapter-15-Error-Correction-Mechanism.md`

## Active Outline At Chunk Start
- p. 1303 - Chapter 15 Error Correction Mechanism
  - p. 1303 - 15.1 2-D Error Correction

## Contained Bookmark Outline
- p. 1303 - Chapter 15 Error Correction Mechanism
  - p. 1303 - 15.1 2-D Error Correction
  - p. 1305 - 15.2 3-D Error Correction
  - p. 1306 - 15.3 Data Representation
    - p. 1307 - 15.3.1 1-D Representation
    - p. 1308 - 15.3.2 2-D Representation
    - p. 1310 - 15.3.3 3-D Representation
  - p. 1311 - 15.4 Error Correction Functions
    - p. 1312 - 15.4.1 MMC_LoadErrorCorrTable
    - p. 1315 - 15.4.2 MMC_EnableErrorCorrTable
    - p. 1318 - 15.4.3 MMC_GetErrorTableStatus
    - p. 1321 - 15.4.4 MMC_DisableErrorCorrTable
    - p. 1324 - 15.4.5 MMC_UnloadErrorCorrTable

## Extracted Text

### PDF page 1303
<a id="pdf-page-1303"></a>
#### Chapter 15 Error Correction Mechanism
##### 15.1 2-D Error Correction
Chapter 15 Error Correction Mechanism
This chapter describes the correction mechanism to correct non -linear mechanical position errors. The
correction is performed by calculating pre-defined correction values at discrete position points (as
measurement grid points) prior to the motion, creating an error correction table in the Maestro. In order to
compute the error correction value at a given point, a linear interpolation (1D or higher) between two adjacent
points in the error correction table is applied, and the value is a dded to the position calculated by the profiler.
This sum is downloaded to the drive and the position correction reported to the user. This is the actual encoder
position, which has not changed.
The Position Correction is performed in the low-level code immediately after retrieving the hardware position
values, but just before sending the next target position command. This correction is therefore transparent to
the end user and considered normal software operation.
In distributed control architecture, the 1D, 2D and 3D error compensations are implemented at the master
controller level in the Maestro, which has overall control of the multiple axes. Therefore, Maestro reads the
axes X/Y/Z etc. position data to check the actual position, calculates the Error Co mpensation Tables, and sends
the corrected intermittent target commands via the field bus network.
It is important to note, that when running in NC mode (cyclic/interpolated position), the correction is
performed continuously in real time every Sync cycle time, throughout path execution. In distributed motions,
(e.g. Profile Position), only the final target position is corrected.
15.1 2-D Error Correction
2-D Error correction refers to every correction point (x,y) on a two dimensional grid is defined as a function of
any two axes positions. The point actually defines an error for a specific axis. For instance, axis Z co rrection may
be a function of X and Y. X correction, can be a function of X itself and Y position. Therefore:
CorrectedPosition=HardwareReading+ErrorCorrection
where, the ErrorCorrection is a function of two position inputs. In general, this may be summed as:
),( βαγ f= , where, γ is the corrected position of any one of the axes. βα, are inputs to γ , and may be any of
the Maestro axes.
In reality, there can be numerous axes in the system (over the field bus). Maestro support four separate
(independent) compensation tables, either in 1D, 2D or 3D with a maximum total of six points allowed for all
four tables.
The position inputs for the table are the axes position as reported over the field bus. Usually it is the main servo
loop position feedback returning from the drive. The Corrected Position per axis, can then be defined as a
mapping function on any two different hardware position inputs. This is demonstrated in Figure 406 overpage.

### PDF page 1304
<a id="pdf-page-1304"></a>
Mapping X Mapping Y Mapping Z Mapping W
Pos X Pos Y Pos Z Pos W
Axis α Axis β Axis α Axis β Axis α Axis β Axis α Axis β

Figure 406: Schematic example of the Corrected Position axis mapping function
Looking at a general α, β grid, and we want to calculate the γ (height) from the grid, as a function of the α, β
input axes, it would look similar to the Height defined by the Error Correction Function shown in Figu re 407
below.

Figure 407: Error Correction Function 3-D Graph

### PDF page 1305
<a id="pdf-page-1305"></a>
##### 15.2 3-D Error Correction
15.2 3-D Error Correction
3-D error correction is somewhat similar to 2-D error correction. In 2-D error correction, a 2-D table and grid is
defined for any specific two axes. The idea of 3-D error correction is having multiple layered grids. As defined
for the 2-D mode, the third axis may also be defined as any of the axes, and must be user predefined (as part of
the general setup).
Generally, the correction at any point for any specific axis χ, can be defined as a function of any other three
axes (α, β, δ):
γ = f (α, β, δ)
For the purpose of a 3-D correction, a set of 2-D corrections, with identical grid points (on the α, β plan) are
used. A graphic presentation is shown in Figure 408 below.

Figure 408: Error Correction Function layered 3-D Graph
Figure 408 displays an m x n x k dimensional grid, where:
k = 3 x 2-D grids of 25 x 25 points each. All 2-D grids have the same definitions.
The method involves locating a third point (correction) called δ, defining the third dimension (Z axis in
Figure 408).
Initially, the index of two relevant 2-D m x n grids we are using is to be located. Actually, this requires searching
in the δ axis, between the two 2-D grids we are located. We are therefore searching between Layers 1 and 2
(Figure 408), performing the calculation using the identical 2-D equations on the two grids where we are
positioned in the middle. We wish to calculate 2 x γ's (one per 2-D grid) as described in the previous section for
the 2-D correction.
Layer 1
Layer 2
Layer 3

### PDF page 1306
<a id="pdf-page-1306"></a>
##### 15.3 Data Representation
15.3 Data Representation
Data representation in the controller has considerable consequences on the code efficiency.

### PDF page 1307
<a id="pdf-page-1307"></a>
###### 15.3.1 1-D Representation
15.3.1 1-D Representation
The 1D data representation remains intact, and is supported in the Maestro level. The matrix is represented as
a linear vector.
9 10 11 12
The Error Correction file is actually a tab-separated text file, which can be created in Excel and takes the
following format (example), for say each α and β value above.
Note: The Header will always start with [header] and end with [header/].
11. [header]
12. 1D Example
Table size 5
Error table dimension 1
Start position 20000
Target axis a01
Reference axes a02
Axis grid size 16
Table dimensions 5
13. [header/]
14. The final data table appears as shown below:
Note: The Data area will always start with [table] and end with [table/].
The table data can be either integer or float numbers.
15. [table]
Table #1 Start Data
0 100 0 100 0
Table #1 End Data
16. [table/]

### PDF page 1308
<a id="pdf-page-1308"></a>
###### 15.3.2 2-D Representation
15.3.2 2-D Representation
The 2-D data representation involves the error correction data for 2-D grid points, stored in the same ET. The
matrix is represented as a two dimensional matrix.
9 10 11 12
5 6 7 8
1 2 3 4
Each number in the above matrix represents the value of the error correction in the given (α, β) point.
The Error Correction file is actually a tab-separated text file, which can be created in Excel, and takes the
following format (example), for say each α and β value above.
Note: The Header will always start with [header] and end with [header/].
17. [header]
18. 2D Example
Table size 25 This is the total table size (actual number of points)
Error table dimension 2 Either 1D/2D/3D
Target axis a01 The axis to be corrected
Reference axes a02 a03 The input axes to the error correction table
Start position 20000 20000 The start position of the error correction. It may
either be an integer of floating point number.
Axis grid size 16 16 The actual resolution between sample points. It
may either be an integer of floating point number.
Table dimensions 5 5 Number of rows and columns. In a 3D table, the
third parameter will be the number of tables.
19. [header/]
20. The final data table appears as shown below:
Note: The Data area will always start with [table] and end with [table/].
The table data can be either integer or float numbers.
21. [table]
Table #1 Start Data
0 100 0 100 0
0 100 0 100 0
0 100 0 100 0
0 100 0 100 0
0 100 0 100 0

### PDF page 1309
<a id="pdf-page-1309"></a>
Table #1 End Data
[table/]

### PDF page 1310
<a id="pdf-page-1310"></a>
###### 15.3.3 3-D Representation
15.3.3 3-D Representation
The 3-D data is saved as consecutive 2-D matrices.
9 10 11 12
5 6 7 8
1 2 3 4
Grid#1
21 22 23 24
17 18 19 20
13 14 15 16
Grid#2
etc... for additional grid's.
22. 1. For each axis, the user must define the input axes α, β, δ (of which axis are α, β, δ, from the
possible Maestro Axis nodes).
23. 2. For each α, β, and δ value, the following must be inserted (refer to the 2-D Example (2D
Example):
d. a. Table size
e. b. Error table dimension
f. c. Target axis a01
g. d. Reference axis a02
h. e. Start position
i. f. Axis grid size
j. g. Table dimensions

### PDF page 1311
<a id="pdf-page-1311"></a>
##### 15.4 Error Correction Functions
15.4 Error Correction Functions
The following Error Correction functions are described:
Error Correction functions
MMC_LoadErrorCorrTable
MMC_UnloadErrorCorrTable
MMC_EnableErrorCorrTable
MMC_DisableErrorCorrTable
MMC_GetErrorTableStatus

### PDF page 1312
<a id="pdf-page-1312"></a>
###### 15.4.1 MMC_LoadErrorCorrTable
15.4.1 MMC_LoadErrorCorrTable
Loads an error correction table to memory. Error correction is then performed according t o this table.
MMC_LIB_API int MMC_LoadErrorCorrTableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_LOADERRORTABLE_IN* pInParam,
OUT MMC_LOADERRORTABLE_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS/includes/MMC_ErrorCorr_API.h
GMAS Programming(IEC 61331 Program)\ElmoGlobal
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_LOADERRORTABLE input data structure using the
MMC_LoadErrorCorrTable function.
pOutParam
Points to the MMC_LOADERRORTABLE_OUT output structure receiving information, as
a result of calling the MMC_LoadErrorCorrTable function.
Remarks
The Error Correction Table is loaded to the memory and requires enabling using the function
MMC_EnableErrorCorrTable, in order to be functional. If loaded, and enabled, the table can be retained in
memory but disabled using the function MMC_DisableErrorCorrTable. To change its functionality to enable,
perform MMC_EnableErrorCorrTable again. To unload the table from the memory perform the function
MMC_UnloadErrorCorrTable.
Scope
All
MMC_LOADERRORTABLE_IN Structure
typedef struct{
double dMaxCorrectionDelta;
NC_ERROR_TABLE_NUMBER eETNumber;
unsigned char pPathToETFile[NC_MAX_ET_FILE_PATH_LENGTH];
}MMC_LOADERRORTABLE_IN;

### PDF page 1313
<a id="pdf-page-1313"></a>
Parameters
dMaxCorrectionDelta
This parameter define the maximum allowed correction input value. If you try to insert
a table where one of the correction values is above the MaxCorrectionDelta, an error is
received; -342.
If you set this value to "0", the max correction delta is unlimited.
NC_ERROR_TABLE_NUMBER eETNumber
Defines the error table letter assigned.
NC_ERROR_TABLE_NUMBER is an enumerator describing the following values:
NC_ERROR_TABLE_A
NC_ERROR_TABLE_B
NC_ERROR_TABLE_C
NC_ERROR_TABLE_D
NC_ERROR_TABLE_E
NC_ERROR_TABLE_F
NC_ERROR_TABLE_MAX
pPathToETFile
Defines the path to the error table file.
[NC_MAX_ET_FILE_PATH_LENGTH] is the maximum size of the error table file path. It is
limited to 100.
If you set "NULL" in this input, the default file path will be used.
The default path is set to the:
/mnt/jffs/usr/ directory and the filename will be depended on the table index:
ErTBL_#.txt where the # is the table index (A,B,C,D,E,F).
MMC_LOADERRORTABLE_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_LOADERRORTABLE_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function.

### PDF page 1314
<a id="pdf-page-1314"></a>
Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Displays an error code as negative or positive integers.
Figure 409 describes the function for MMC_LoadErrorCorrTable as applied within the IEC 61131 programming.
[PDF field-code object omitted]
Figure 409: MMC_LoadErrorCorrTable function
15.4.1.1 Function Code Example
MMC_LOADERRORTABLE_IN stLoadErrorTableIn;
MMC_LOADERRORTABLE_OUT stLoadErrorTableOut;

stLoadErrorTableIn.eETNumber = NC_ERROR_TABLE_A;
strcpy(stLoadErrorTableIn.pPathToETFile, szFileName);

rc = MMC_LoadErrorCorrTableCmd(hConnHndl, &stLoadErrorTableIn,
&stLoadErrorTableOut);
if (NC_OK != rc)
{
HandleError();
}

### PDF page 1315
<a id="pdf-page-1315"></a>
###### 15.4.2 MMC_EnableErrorCorrTable
15.4.2 MMC_EnableErrorCorrTable
Enables the usage of an error correction table.
MMC_LIB_API int MMC_EnableErrorCorrTableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_ENABLEERRORTABLE_IN* pInParam,
OUT MMC_ENABLEERRORTABLE_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS/includes/MMC_ErrorCorr_API.h
GMAS Programming(IEC 61331 Program)\ElmoGlobal
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_ENABLEERRORTABLE input data structure using the
MMC_EnableErrorCorrTable function.
pOutParam
Points to the MMC_ENABLEERRORTABLE_OUT output structure receiving information,
as a result of calling the MMC_EnableErrorCorrTable function.
Remarks
The Error Correction Table is loaded to the memory and requires enabling using the function
MMC_EnableErrorCorrTable, in order to be functional. If loaded, and enabled, the table can be retained in
memory but disabled using the function MMC_DisableErrorCorrTable . To change its functionality to enable,
perform MMC_EnableErrorCorrTable again. To unload the table from the memory perform the function
MMC_UnloadErrorCorrTable.
Scope
All
MMC_ENABLEERRORTABLE_IN Structure
typedef struct{
NC_ERROR_TABLE_NUMBER eTableNumber;
}MMC_ENABLEERRORTABLE_IN;
Parameters

### PDF page 1316
<a id="pdf-page-1316"></a>
NC_ERROR_TABLE_NUMBER eTableNumber
Defines the error table letter assigned to be enabled.
NC_ERROR_TABLE_NUMBER is an enumerator describing the following values:
NC_ERROR_TABLE_A
NC_ERROR_TABLE_B
NC_ERROR_TABLE_C
NC_ERROR_TABLE_D
NC_ERROR_TABLE_E
NC_ERROR_TABLE_F
NC_ERROR_TABLE_MAX
MMC_ENABLEERRORTABLE_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_ENABLEERRORTABLE_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function.
Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Displays an error code as negative or positive integers.
Figure 410 describes the function for MMC_EnableErrorCorrTable as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 410: MMC_EnableErrorCorrTable function
15.4.2.1 Function Code Example
MMC_ENABLEERRORTABLE_IN stEnableErrorTableIn;
MMC_ENABLEERRORTABLE_OUT stEnableErrorTableOut;

stEnableErrorTableIn.eTableNumber = NC_ERROR_TABLE_A;

rc = MMC_EnableErrorCorrTableCmd(hConnHndl, &stEnableErrorTableIn,
&stEnableErrorTableOut);
if (NC_OK != rc)
{
HandleError();
}

### PDF page 1317
<a id="pdf-page-1317"></a>
[No extractable text on this page.]

### PDF page 1318
<a id="pdf-page-1318"></a>
###### 15.4.3 MMC_GetErrorTableStatus
15.4.3 MMC_GetErrorTableStatus
Function recieves the table number as input and returns an answer whether the table is loaded and/or
enabled.
MMC_LIB_API int MMC_GetErrorTableStatusCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETERRORTABLESTATUS_IN* pInParam,
OUT MMC_GETERRORTABLESTATUS_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS/includes/MMC_ErrorCorr_API.h
GMAS Programming(IEC 61331 Program)\ElmoGlobal
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_GETERRORTABLESTATUS input data structure using the
MMC_GetErrorTableStatus function.
pOutParam
Points to the MMC_GETERRORTABLESTATUS_OUT output structure receiving
information, as a result of calling the MMC_GetErrorTableStatus function.
Remarks
The Error Correction Table is loaded to the memory and requires enabling using the function
MMC_EnableErrorCorrTable, in order to be functional. If loaded, and enabled, the table can be retained in
memory but disabled using the function MMC_DisableErrorCorrTable . To change its functionality to enable,
perform MMC_EnableErrorCorrTable again. To unload the table from the memory perform the function
MMC_UnloadErrorCorrTable.
Scope
All
MMC_GETERRORTABLESTATUS_IN Structure
typedef struct{
NC_ERROR_TABLE_NUMBER eTableNumber;
}MMC_GETERRORTABLESTATUS_IN;
Parameters

### PDF page 1319
<a id="pdf-page-1319"></a>
NC_ERROR_TABLE_NUMBER eTableNumber
Defines the error table letter assigned.
NC_ERROR_TABLE_NUMBER is an enumerator describing the following valu es:
NC_ERROR_TABLE_A
NC_ERROR_TABLE_B
NC_ERROR_TABLE_C
NC_ERROR_TABLE_D
NC_ERROR_TABLE_E
NC_ERROR_TABLE_F
NC_ERROR_TABLE_MAX
MMC_GETERRORTABLESTATUS_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
unsigned char ucIsTableEnabled;
unsigned char ucIsTableLoaded;
NC_NODE_HNDL_T hReferenceAxesRef[NC_ERROR_TABLE_DIMENSION_3D];
NC_NODE_HNDL_T hTargetAxisRef;
char cFileName[NC_MAX_ET_FILE_PATH_LENGTH];
char sSpare[20];
}MMC_GETERRORTABLESTATUS_OUT;
Parameters
ucIsTableEnabled
Returns the Boolean solution to the question, whether the table is enabled or not.
ucIsTableLoaded
Returns the Boolean solution to the question, whether the table is loaded or not.
NC_NODE_HNDL_T hReferenceAxesRef
This array represent the axes references of the error correction table input.
The array [NC_ERROR_TABLE_DIMENSION_3D] is the dimension of the 3D error table.
NC_NODE_HNDL_T hTargetAxisRef
The parameter hTargetAxisRef represents the reference of the target axis.
cFileName[NC_MAX_ET_FILE_PATH_LENGTH]
Defines the file name.
[NC_MAX_ET_FILE_PATH_LENGTH] is the maximum size of the error table file path. It is
limited to 100.

### PDF page 1320
<a id="pdf-page-1320"></a>
sSpare[20]
Spare. For internal use only. Any positive integer value to a maximum of 20 characters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function.
Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Displays an error code as negative or positive integers.
Figure 411 describes the function for MMC_GetErrorTableStatus as applied within the IEC 61131 programming.
[PDF field-code object omitted]
Figure 411: MMC_GetErrorTableStatus function
15.4.3.1 Function Code Example
MMC_GETERRORTABLESTATUS_IN stGetErrorTableStatusIn;
MMC_GETERRORTABLESTATUS_OUT stGetErrorTableStatusOut;

stGetErrorTableStatusIn.eTableNumber = NC_ERROR_TABLE_A;

rc = MMC_GetErrorTableStatusCmd(hConnHndl, &stGetErrorTableStatusIn,
&stGetErrorTableStatusOut);

if (NC_OK != rc)
{
HandleError();
}

### PDF page 1321
<a id="pdf-page-1321"></a>
###### 15.4.4 MMC_DisableErrorCorrTable
15.4.4 MMC_DisableErrorCorrTable
Disables the usage of an error correction table.
MMC_LIB_API int MMC_DisableErrorCorrTableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_DISABLEERRORTABLE_IN* pInParam,
OUT MMC_DISABLEERRORTABLE_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS/includes/MMC_ErrorCorr_API.h
GMAS Programming(IEC 61331 Program)\ElmoGlobal
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_DISABLEERRORTABLE input data structure using the
MMC_DisableErrorCorrTable function.
pOutParam
Points to the MMC_DISABLEERRORTABLE_OUT output structure receiving information,
as a result of calling the MMC_DisableErrorCorrTable function.
Remarks
The Error Correction Table is loaded to the memory and requires enabling using the function
MMC_EnableErrorCorrTable, in order to be functional. If loaded, and enabled, the table can be retained in
memory but disabled using the function MMC_DisableErrorCorrTable. To change its functionality to enable,
perform MMC_EnableErrorCorrTable again. To unload the table from the memory perform the function
MMC_UnloadErrorCorrTable.
Scope
All
MMC_DISABLEERRORTABLE_IN Structure
typedef struct{
NC_ERROR_TABLE_NUMBER eTableNumber;
}MMC_DISABLEERRORTABLE_IN;
Parameters

### PDF page 1322
<a id="pdf-page-1322"></a>
NC_ERROR_TABLE_NUMBER eTableNumber
Defines the error table letter assigned to be disabled.
NC_ERROR_TABLE_NUMBER is an enumerator describing the following va lues:
NC_ERROR_TABLE_A
NC_ERROR_TABLE_B
NC_ERROR_TABLE_C
NC_ERROR_TABLE_D
NC_ERROR_TABLE_E
NC_ERROR_TABLE_F
NC_ERROR_TABLE_MAX
MMC_DISABLEERRORTABLE_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_DISABLEERRORTABLE_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function.
Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Displays an error code as negative or positive integers.
Figure 412 describes the function for MMC_DisableErrorCorrTable as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 412: MMC_DisableErrorCorrTable function
15.4.4.1 Function Code Example
MMC_DISABLEERRORTABLE_IN stDisableErrorTableIn;
MMC_DISABLEERRORTABLE_OUT stDisableErrorTableOut;

stDisableErrorTableIn.eTableNumber = NC_ERROR_TABLE_A;

rc = MMC_DisableErrorCorrTableCmd(hConnHndl, &stDisableErrorTableIn,
&stDisableErrorTableOut);
if (NC_OK != rc)
{
HandleError();
}

### PDF page 1323
<a id="pdf-page-1323"></a>
[No extractable text on this page.]

### PDF page 1324
<a id="pdf-page-1324"></a>
###### 15.4.5 MMC_UnloadErrorCorrTable
15.4.5 MMC_UnloadErrorCorrTable
Unloads an error correction table from memory.
MMC_LIB_API int MMC_UnloadErrorCorrTableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_UNLOADERRORTABLE_IN* pInParam,
OUT MMC_UNLOADERRORTABLE_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS/includes/MMC_ErrorCorr_API.h
GMAS Programming(IEC 61331 Program)\ElmoGlobal
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_UNLOADERRORTABLE input data structure using the
MMC_UnloadErrorCorrTable function.
pOutParam
Points to the MMC_UNLOADERRORTABLE_OUT output structure receiving information,
as a result of calling the MMC_UnloadErrorCorrTable function.
Remarks
The Error Correction Table is loaded to the memory and requires enabling using the function
MMC_EnableErrorCorrTable, in order to be functional. If loaded, and enabled, the table can be retained in
memory but disabled using the function MMC_DisableErrorCorrTable. To change its functionality to enable,
perform MMC_EnableErrorCorrTable again. To unload the table from the memory perform the function
MMC_UnloadErrorCorrTable.
Scope
All
MMC_UNLOADERRORTABLE_IN Structure
typedef struct{
NC_ERROR_TABLE_NUMBER eTableNumber;
}MMC_UNLOADERRORTABLE_IN;
Parameters

### PDF page 1325
<a id="pdf-page-1325"></a>
NC_ERROR_TABLE_NUMBER eTableNumber
Defines the error table letter assigned to be unloaded.
NC_ERROR_TABLE_NUMBER is an enumerator describing the following values:
NC_ERROR_TABLE_A
NC_ERROR_TABLE_B
NC_ERROR_TABLE_C
NC_ERROR_TABLE_D
NC_ERROR_TABLE_E
NC_ERROR_TABLE_F
NC_ERROR_TABLE_MAX
MMC_UNLOADERRORTABLE_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_UNLOADERRORTABLE_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function.
Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Displays an error code as negative or positive integers.
Figure 413 describes the function for MMC_UnloadErrorCorrTable as applied within the IEC 61131
programming.
[PDF field-code object omitted]
Figure 413: MMC_UnloadErrorCorrTable function
15.4.5.1 Function Code Example
MMC_UNLOADERRORTABLE_IN stUnloadErrorTableIn;
MMC_UNLOADERRORTABLE_OUT stUnloadErrorTableOut;

stUnloadErrorTableIn.eTableNumber = NC_ERROR_TABLE_A;

rc = MMC_UnloadErrorCorrTableCmd(hConnHndl, &stUnloadErrorTableIn,
&stUnloadErrorTableOut);

if (NC_OK != rc)
{
HandleError();

### PDF page 1326
<a id="pdf-page-1326"></a>
}
