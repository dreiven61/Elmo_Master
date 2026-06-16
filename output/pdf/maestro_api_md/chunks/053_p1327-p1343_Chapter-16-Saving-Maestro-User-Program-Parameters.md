# Chapter 16 Saving Maestro User Program Parameters

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 1327-1343
- Chunk: `053_p1327-p1343_Chapter-16-Saving-Maestro-User-Program-Parameters.md`

## Active Outline At Chunk Start
- p. 1327 - Chapter 16 Saving Maestro User Program Parameters
  - p. 1327 - 16.1 Introduction

## Contained Bookmark Outline
- p. 1327 - Chapter 16 Saving Maestro User Program Parameters
  - p. 1327 - 16.1 Introduction
  - p. 1328 - 16.2 The MMCUserParams C++ Class
    - p. 1330 - 16.2.1 Open
    - p. 1331 - 16.2.2 Close
    - p. 1332 - 16.2.3 Read
    - p. 1335 - 16.2.4 GetXmlFileRoot
    - p. 1336 - 16.2.5 GetXmlFileDescrp
    - p. 1337 - 16.2.6 SetSpeakDbgLvl
    - p. 1338 - 16.2.7 UPXML Functions Code Examples
    - p. 1341 - 16.2.8 UpxmlEg.xml - Input File Example
    - p. 1343 - 16.2.9 Program output example

## Extracted Text

### PDF page 1327
<a id="pdf-page-1327"></a>
#### Chapter 16 Saving Maestro User Program Parameters
##### 16.1 Introduction
Chapter 16 Saving Maestro User Program
Parameters
This chapter describes the method to extract UPXML (User Parameters maintained in XML form) user
application Parameters from XML files in the Maestro. The procedure uses the function within the MMCPP
libraries class for IPC to accomplish the extraction.
16.1 Introduction
Previous to adding the new class methods, modifying a specific parameter w ithin a Maestro User Application
program required the user to modify hard coded constants (modify the code), compile, and create a new
executable file. Now while using an XML file containing the Maestro parameters it is possible to edit only the
XML file. Examples of Maestro User Application program data are:
- Axis motion parameters
- Communication parameters
- Program behaviors. Flags, etc. ...
Presently, the user can now read all types of parameters using the dedicated UPXML functions. The Maestro -
UPXML functions allow the user to retrieve parameters from a textual based file and set the values to program
variables. These functions are to be supported in the CPP Library only, currently only when working with IPC.
An example of a well formed Maestro UPXML is:
<?xml version="1.0" encoding="utf-8"?>
<root xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
xsi:noNamespaceSchemaLocation="proposed.xsd">
<FILE_DESCRIPTION NAME="Parameters" VERSION="NovaScan 1236" />
<CATEGORY NAME="Profiler">
<RESOURCES NAME="a01">
<AC>10000000</AC>
<DC>10000000</DC>
<JERK>1073742336</JERK>
<DRIVE_ID>'81'</DRIVE_ID>
</RESOURCES>
<RESOURCES NAME="a02">
<AXIS_MODE>0</AXIS_MODE>
<OP_MODE>8</OP_MODE>
<KUKU>'1073742336'</KUKU>
<DRIVE_ID>0</DRIVE_ID>
</RESOURCES>
</CATEGORY >
<CATEGORY NAME="Communication">
<RESOURCES NAME="a01">
<TIMEOUT>0</TIMEOUT>
<NUM_VARS>8</NUM_VARS>
<KUKU>'1073742336'</KUKU>
</RESOURCES>
<RESOURCES NAME="a02">
<TIMEOUT>0</TIMEOUT>
<NUM_VARS>8</NUM_VARS>
<KUKU>'1073742336'</KUKU>
</RESOURCES>
</CATEGORY >

### PDF page 1328
<a id="pdf-page-1328"></a>
##### 16.2 The MMCUserParams C++ Class
<CATEGORY NAME="Misc">
<RESOURCES NAME="Global">
<DOHOMEALWAYS>0</DOHOMEALWAYS>
<SETPOSATTARGET>8</SETPOSATTARGET>
<DONOTHINGATALL>'1073742336'</DONOTHINGATALL>
</RESOURCES>
</CATEGORY >
</root>
16.2 The MMCUserParams C++ Class
Based on the well-formed Maestro UPXML, the XML elements in the data file are defined:
Element Value
The highest element name (under the "root") is
CATEGORY
User defined Attribute (value)
E.g. "Profiler" in XML element:
<CATEGORY NAME="Profiler">
E.g. "Communication" in XML element:
<CATEGORY NAME="Communication">
The element name under CATEGORY is RESOURCES User define Attribute (value)
E.g. "a02" in XML element:
<RESOURCES NAME="a02">
E.g. "Global" in XML element:
<RESOURCES NAME="Global">
The element name under RESOURCES is user defined
and its value is returned from the saved parameters
XML element. The user defines the Element name,
the saved value appears as XML data of the
element
E.g. value of 10000000 for parameter name AC:
<AC>10000000</AC>
E.g. value True for parameter name PRM032:
<PRM032>TRUE</PRM032>
Note: The elements require to be positioned in a
suitable hierarchy location on the XML file.
The MMCUserParams class therefore includes the following methods described in detail in the following
subsections:
Open Opens the XML file, with specific parameters, such as:
- File name
- File location (path)
- How to behave related to subsequence read operation from this file, case of
requested element name not found in file
Close Closes the file pointed to the XML file and release resource used for parsing the file

### PDF page 1329
<a id="pdf-page-1329"></a>
GetXmlFileRoot Function that retrieves the root-data of the XML file, and its specific description from
the XML file header.
GetXmlFileDescrp Function that retrieves data from the XML file, specifically the description of the XML
file header.
Read List of overloaded function that retrieves data for a given variable. In some cases also
ensures that the data is within specific limitations.
I n addition, for double and long variable types, array of values are returned.

### PDF page 1330
<a id="pdf-page-1330"></a>
###### 16.2.1 Open
16.2.1 Open
Opens the XML file, with specific parameters
int Open(
char* cFileName=DEFAULT_XML_FILE_NAME,
unsigned int uiFlags=UPXML_SET_DEF_REQ_FLG,
char* cFilePath=DEFAULT_XML_FILE_PATH
) throw (CMMCException);
Source GMAS\includes\CPP\MMCUserParams.h
.NET Definition
Parameters
cFileName
Where DEFAULT_XML_FILE_NAME is the file UserParams.xml, the default XML file
name.
uiFlags
If the flag is set (= UPXML_SET_DEF_REQ_FLG or 1), then a default setting is requested.
When reading from this file, if no suitable value is found in the XML data, it returns the
value of the Default parameter.
cFilePath
File path for the data to be read.
Where DEFAULT_XML_FILE_PATH is the "/mnt/jffs/usr/" default XML path when the
cFilePath parameters do not exist.
throw (CMMCException)
Refer to the section 24.1.1 MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.
Return Value
0 if OK. Otherwise error code as detailed in section 4.9 Internal Library Error IDs e.g.:
- sequence error (E.g. reopen before close previous file)
- Cannot open file (no such file or no permit ion for access).
- File format error
- File too long...

### PDF page 1331
<a id="pdf-page-1331"></a>
###### 16.2.2 Close
16.2.2 Close
Closes the file pointed to the XML file and release resource used for parsing the file
int Close(
);
Source GMAS\includes\CPP\MMCUserParams.h
.NET Definition
Return Value

0 if OK (always).

### PDF page 1332
<a id="pdf-page-1332"></a>
###### 16.2.3 Read
16.2.3 Read
List of overloaded function that retrieves data to given variable. The Values may be of a double (single or array),
long (single or array), Boolean, or String according to the number, type, and order of the parameters:
Read single value parameters
- Double, retrieve one parameter of type Double
- Long, retrieve one parameter of type Long
- Boolean, retrieve one parameter of type Boolean, ignores white space, bu t expects True / False
- String, retrieve one parameter of type string
- Read An array of parameters values
- Retrieve array of double
- Retrieve array of long
Single Value Parameters
int Read (
char* pCtgryVal,
char*.pRsrcVal,
char* pTagName,

double &dVal, | long &lVal, | Bool &bVal, | char* pStr,
double dDefault, | long lDefault, | Bool bDefault=0

double dMin=DBL_MIN, | long lMin=LONG_MIN,

double dMax=DBL_MAX, | long lMax=LONG_MAX, |
long lLen,

) throw (CMMCException);
An array of parameters values
int ReadArr (
char* pCtgryVal,
char*.pRsrcVal,
char* pTagName,

double dVal[], | long lVal[],

double dDefault, | long lDefault,

unsigned int& iActRdElm,
unsigned int iReqRdElm=1,

double dMin=DBL_MIN, |long lMin=LONG_MIN,

double dMax=DBL_MAX, | long lMax=LONG_MAX,
) throw (CMMCException);
Source GMAS\includes\CPP\MMCUserParams.h
.NET Definition
Parameters
pCtgryVal
Pointer to the NULL terminated string. The string is the value of the tag name <CATEGORY
pRsrcVal
Pointer to the NULL terminated string. The string is the value of the tag name
<RESOURCES>.
pTagName
Pointer to the NULL terminated string. The string is the Name of the users defined tag.

### PDF page 1333
<a id="pdf-page-1333"></a>
&dVal, &lVal, &bVal, pStr, dVal[], lVal[]
Reference variable to copy the value to.
dDefault, LDefault, bDefault
Default value to be set if the tag was not found.
dMin, lMin
Value read should not be less than this value. DBL_MIN has a minimum value of 1E-37
LONG_MIN is compiler depended constant.
dMax, lMax
Value read should not be greater than this value. DBL_MAX has a maximum value of 1E+37.
LONG_MAX is compiler depended constant.
lLen
Size of the read buffer.
& iActRdElm
Number of actual read elements.
iReqRdElm=1(default)
Number of read elements requested. This is the maximum value, the function will not read
more elements.
throw (CMMCException)
Refer to the section 24.1.1 MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.

### PDF page 1334
<a id="pdf-page-1334"></a>
Return Value
0 if OK. Otherwise warning whether:
- Tag not found and default value was set.
- Default value set due to exceed Min/Max.
Note: Make sure that there is sufficient space to store the read element.
The Return Value is 0 if OK. Otherwise warning whether:
- Tag not found and default value was set
- Default value set due to exceed Min/Max
Remarks
For arrays, whole array elements should be of same type, with a comma separating elements values, and the
Element values not interspaced with not broken by chars not belong to element,
Single value example:
Example:
/* For refer to tag name <CATEGORY> has value "Profiler ", put: */
pCtgrVal = "Profiler";
/* For refer to tag name <RESOURCES> has value "a01", put: */
pRsrcVal = "a01";
/* For refer to tag name <DC> */
/* (in context of other parameter, currently set to: */
/* CATEGORY="Profiler", RESOURCES="a01"), put: */
pTagName = "DC";

### PDF page 1335
<a id="pdf-page-1335"></a>
###### 16.2.4 GetXmlFileRoot
16.2.4 GetXmlFileRoot
Returns the XML file root (XSI ID values) pAtt1 and XSI Location
int GetXmlFileRoot (
char* pAtt1,
char* pAtt2,
long lLen
) throw (CMMCException);
Source GMAS\includes\CPP\MMCUserParams.h
.NET Definition
Parameters
pAtt1
Pointer to the buffer of size ILen. The attribute name: 'FILE_DESCRIPTION' is copied to
it.
pAtt2
Pointer to buffer of size ILen, the attribute name: 'VERSION' of level1 element has name
'FILE_DESCRIPTION' coping to it.
lLen
Size of pAtt1 & pAtt2 buffer in bytes. The buffer size for returned values are at least
lLen. Look for and parse XML file root into pAtt1, and xsi pAtt2. The buffer size for
return values are at least lLen.
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name
Structure name
The axis reference
Error ID
Status of the axis.
Remarks
For example, for an XML file line where:
<root xmlns:XSI=http://www.w3.org/2001/XMLSchema-instance
XSI:noNamespaceSchemaLocation="proposed.xsd">
This will return the two parameters values:
pAtt1 = http://www.w3.org/2001/XMLSchema-instance
pAtt2 = "proposed.xsd"
If both attribute founds, then rt_val= MMC_OK, Otherwise, rt_val= MMC_LIB_UPXML_NOT_FOUND
if only one attribute is found copy it and return value MMC_LIB_UPXML_NOT_FOUND .
In this case, if you wish determined about coping data, put 0 at firs location, check it after the call, if it still
there => no data copied.

### PDF page 1336
<a id="pdf-page-1336"></a>
###### 16.2.5 GetXmlFileDescrp
16.2.5 GetXmlFileDescrp
Returns the XML "file description name" represented by pAtt1, and XML file version as pAtt2, the buffer size
for return values which are at least lLen in size.
int GetXmlFileDescrp(
char* pAtt1,
char* pAtt2,
long lLen
) throw (CMMCException);
Source GMAS\includes\CPP\MMCUserParams.h
.NET Definition
Parameters
pAtt1
Pointer to the buffer of size ILen. The attribute name: 'FILE_DESCRIPTION' is copied to
it.
pAtt2
Pointer to buffer of size ILen,
the root attribute name: 'xsi:noNamespaceSchemaLocation' coping to it.
lLen
Size of pAtt1 & pAtt2 buffer in bytes. The buffer size for returned values are at least
lLen. Look for and parse XML file root into pAtt1, and xsi pAtt2. The buffer size for
return values are at least lLen.
If both attribute founds, then rt_val= MMC_OK; Otherwise,
rt_val= MMC_LIB_UPXML_NOT_FOUND
if only one attribute is found, copy it and return value MMC_LIB_UPXML_NOT_FOUND.
In this case, if you wish determined about coping data, put 0 at firs location, check it
after the call, if it still there => no data copied.
E.g.:
For XML file has the line (on level 1):
<FILE_DESCRIPTION NAME="Parameters" VERSION="NovaScan 1236" />
pAtt1 ="Parameters";
pAtt2 ="NovaScan 1236";
rt_val = MMC_OK;
throw (CMMCException)
Refer to the section MMCException. Produces details of the error including:
Function Name Structure name
The axis reference Error ID
Status of the axis.

### PDF page 1337
<a id="pdf-page-1337"></a>
###### 16.2.6 SetSpeakDbgLvl
16.2.6 SetSpeakDbgLvl
Sets the
void setSpeakDbgLvl (
unsigned int uiSpeak_lvl
);
Source GMAS\includes\CPP\MMCUserParams.h
.NET Definition
Parameters
uiSpeak_lvl
For internal use only

### PDF page 1338
<a id="pdf-page-1338"></a>
###### 16.2.7 UPXML Functions Code Examples
16.2.7 UPXML Functions Code Examples
/*
============================================================================
Name : UserParamTest.cpp
Author : Haim Hillel
Version :
Description : GMAS C++ project source file for:
test program for Class "UPXML" User Param XML.
XML file as source for read parameters (for GMAS).
============================================================================
*/

#include <sys/time.h>
#include <time.h>
#include <stdlib.h>
#include <stdio.h>

#include <iostream>
#include <ctime>
#include <unistd.h>
#include "MMC_Definitions.h"
#include "UserParamDocTest.h"
#include "MMCUserParams.h"

using namespace std;

#define BUFPRT_VALSIZE 1024
#define MAX_ARY_ELM 10

int main()
// ======
{
long min = -1000; /* def. Min number val. */
long max = 1400; /* def. Max number Val. */
long def = 1224; /* def. number Val. */

/* XML Level 1 Attribute Value;*/
/* Attribute of CATEGORY E.g: */
/* Profiler001 */
char* lvl1AttVal = "Profiler000";
/* XML Level 2 Attribute value; Attribute */
/* of RESOURCES E.g: a01 */
char* lvl2AttVal = "a00";
/* XML Level 3 Tag Name; E.g.: PRM003 */
char* lvl3Name = "PRM025";
int ind;
char bufPrt_val [BUFPRT_VALSIZE];
char bufPrt_val1[BUFPRT_VALSIZE];
long bufPrt_valSize;

### PDF page 1339
<a id="pdf-page-1339"></a>
int ErrId;
double dVal[MAX_ARY_ELM];
long lVal[MAX_ARY_ELM];
bool bVal;
unsigned int iActRdElm;
unsigned int iReqRdElm;

MMCUserParams up;

printf("\n
==================================================================== ");
printf("\n Testing UPXML Id: %s %s %s", __FILE__, __DATE__, __TIME__);
printf("\n
==================================================================== \n");

/* program seting def. file name; open def is: "UserParams.xml"
*/
ErrId = up.Open("UpxmlEg.xml", UPXML_SET_DEF_REQ_FLG);

bufPrt_valSize = BUFPRT_VALSIZE;

ErrId = up.GetXmlFileRoot(bufPrt_val, bufPrt_val1, bufPrt_valSize);
printf("\n Root: =11 ErrId=%d ======\n Val: <%s> <%s> \n", ErrId,
bufPrt_val, bufPrt_val1);

ErrId = up.GetXmlFileDescrp (bufPrt_val, bufPrt_val1, bufPrt_valSize);
printf("\n FileDescrp: =12 ErrId=%d ======\n Val: <%s> <%s> \n",
ErrId, bufPrt_val, bufPrt_val1);

ErrId = up.Read (lvl1AttVal, lvl2AttVal, lvl3Name, bufPrt_val,
bufPrt_valSize);
printf("\n buf: =14 ErrId=%d ======%s=<%s> \n", ErrId, lvl3Name,
bufPrt_val);

dVal[0] = -1;
ErrId = up.Read (lvl1AttVal, lvl2AttVal, lvl3Name, dVal[0], (double)def,
(double)min, (double)max);
printf("\n double: =15 ErrId=%d ======%s=<%f> \n", ErrId, lvl3Name,
dVal[0]);

lVal[0] = -1;
ErrId = up.Read (lvl1AttVal, lvl2AttVal, lvl3Name, lVal[0], (long)def,
(long)min, (long)max);
printf("\n long: =16 ErrId=%d ======%s=<%ld> \n", ErrId, lvl3Name,
lVal[0]);

ErrId = up.Read (lvl1AttVal, lvl2AttVal, lvl3Name, bVal, 0);
printf("\n Boolean: =17 def=0 ErrId=%d =====%s=<%d> \n", ErrId, lvl3Name,
bVal);

ErrId = up.Read (lvl1AttVal, lvl2AttVal, lvl3Name, bVal, 1);
printf("\n Boolean: =18 def=1 ErrId=%d =====%s=<%d> \n", ErrId, lvl3Name,
bVal);

iReqRdElm = 4;
for (ind=0; ind < (int)iReqRdElm; ind++)
dVal[ind] = -1.;

### PDF page 1340
<a id="pdf-page-1340"></a>
ErrId = up.ReadArr(lvl1AttVal, lvl2AttVal, lvl3Name, dVal, (double)def,
iActRdElm,
iReqRdElm, (double)min, (double)max);
printf("\n An array double: =19 #Act=%d, #Req=%d ErrId=%d =====%s=\n Val:
", iActRdElm,
iReqRdElm, ErrId, lvl3Name);
for (ind=0; ind < (int)iReqRdElm; ind++)
printf("<%f> ", dVal[ind]);
printf("\n");

for (ind=0; ind < (int)iReqRdElm; ind++)
lVal[ind] = -1;
ErrId = up.ReadArr(lvl1AttVal, lvl2AttVal, lvl3Name, lVal, (long)def,
iActRdElm,
iReqRdElm, (long)min, (long)max);
printf("\n An array long: =20 #Act=%d, #Req=%d ErrId=%d =====%s=\n Val:
", iActRdElm,
iReqRdElm, ErrId, lvl3Name);
for (ind=0; ind < (int)iReqRdElm; ind++)
printf("<%ld> ", lVal[ind]);
printf("\n");

lvl2AttVal = "a02";
lvl3Name = "BoolPrm01";
ErrId = up.Read (lvl1AttVal, lvl2AttVal, lvl3Name, bVal, 0);
printf("\n Boolean: =21 def=0 ErrId=%d =====%s=<%d> \n", ErrId, lvl3Name,
bVal);

lvl3Name = "BoolPrm02";
ErrId = up.Read (lvl1AttVal, lvl2AttVal, lvl3Name, bVal, 0);
printf("\n Boolean: =22 def=0 ErrId=%d =====%s=<%d> \n", ErrId, lvl3Name,
bVal);

up.Close();
return 0;
}

### PDF page 1341
<a id="pdf-page-1341"></a>
###### 16.2.8 UpxmlEg.xml - Input File Example
16.2.8 UpxmlEg.xml - Input File Example
<?xml version="1.0" encoding="utf-8"?>
<root xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
xsi:noNamespaceSchemaLocation="proposed.xsd">
<FILE_DESCRIPTION NAME="Parameters" VERSION="Elmo Eg 1.1" />
<CATEGORY NAME='Profiler'>
<RESOURCES NAME="a01">
<AC>1111,1111</AC>
<DC>22222222</DC>
<JERK>1073742336</JERK>
<DRIVE_ID>81</DRIVE_ID>
</RESOURCES>
<RESOURCES NAME="a02">
<AXIS_MODE>0</AXIS_MODE>
<OP_MODE>5</OP_MODE>
<KUKU>'444444444'</KUKU>
<DRIVE_ID>0</DRIVE_ID>
</RESOURCES>
</CATEGORY>

<CATEGORY NAME='Profiler000'>
<RESOURCES NAME="a00">
<AC>1111,1111</AC>
<DC>22222222</DC>
<JERK>1073742336</JERK>
<PRM025>81, 82,83</DRIVE_ID>
</RESOURCES>
<RESOURCES NAME="a02">
<AXIS_MODE>0</AXIS_MODE>
<BoolPrm01>TRUE</AXIS_MODE>
<OP_MODE>5</OP_MODE>
<KUKU>'444444444'</KUKU>
<BoolPrm02>TRUE</AXIS_MODE>

### PDF page 1342
<a id="pdf-page-1342"></a>
<DRIVE_ID>0</DRIVE_ID>
</RESOURCES>
</CATEGORY>

<CATEGORY NAME="Communication">
<RESOURCES NAME="a02">
<TIMEOUT>7</TIMEOUT>
<NUM_VARS>8</NUM_VARS>
<KUKU>'666666666'</KUKU>
</RESOURCES>
</CATEGORY>
</root>

### PDF page 1343
<a id="pdf-page-1343"></a>
###### 16.2.9 Program output example
16.2.9 Program output example
====================================================================
Testing UPXML Id: ..\UserParamDocTest.cpp May 28 2013 13:36:47
====================================================================
Root: =11 ErrId=0 ======
Val: <http://www.w3.org/2001/XMLSchema-instance> <proposed.xsd>

FileDescrp: =12 ErrId=0 ======
Val: <Parameters> <Elmo Eg 1.1>

buf: =14 ErrId=0 ======PRM025=<81, 82,83>

double: =15 ErrId=0 ======PRM025=<81.000000>

long: =16 ErrId=0 ======PRM025=<81>

**** MMCPPThrow: UPXML Read boolean iRetCode=300, iErrId=3021
Boolean: =17 def=0 ErrId=3021 =====PRM025=<0>

**** MMCPPThrow: UPXML Read boolean iRetCode=300, iErrId=3021
Boolean: =18 def=1 ErrId=3021 =====PRM025=<1>

**** MMCPPThrow: UPXML Read array iRetCode=300, iErrId=3020
An array double: =19 #Act=3, #Req=4 ErrId=3020 =====PRM025=
Val: <81.000000> <82.000000> <83.000000> <1224.000000>

**** MMCPPThrow: UPXML Read array iRetCode=300, iErrId=3020
An array long: =20 #Act=3, #Req=4 ErrId=3020 =====PRM025=
Val: <81> <82> <83> <1224>
Boolean: =21 def=0 ErrId=0 =====BoolPrm01=<1>

Boolean: =22 def=0 ErrId=0 =====BoolPrm02=<1>
