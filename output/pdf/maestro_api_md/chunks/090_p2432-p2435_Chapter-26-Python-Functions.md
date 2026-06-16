# Chapter 26 Python Functions

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 2432-2435
- Chunk: `090_p2432-p2435_Chapter-26-Python-Functions.md`

## Active Outline At Chunk Start
- p. 2432 - Chapter 26 Python Functions
  - p. 2432 - 26.1 Simplicity of MultiThreading

## Contained Bookmark Outline
- p. 2432 - Chapter 26 Python Functions
  - p. 2432 - 26.1 Simplicity of MultiThreading
  - p. 2433 - 26.2 Python Editor/Debugger
  - p. 2434 - 26.3 Python Functions

## Extracted Text

### PDF page 2432
<a id="pdf-page-2432"></a>
#### Chapter 26 Python Functions
##### 26.1 Simplicity of MultiThreading
Chapter 26 Python Functions
Python offers various possibilities: applications, AI software, games, websites, and many more.
It is clear, easy to learn syntax. It is a very popular choice for beginners.
Reduced time required for code testing because of the interactive language features.
It is the de-facto language taught at major institutions
It comes preinstalled on most Linux distributions, and is available as a package on all others
26.1 Simplicity of MultiThreading

### PDF page 2433
<a id="pdf-page-2433"></a>
##### 26.2 Python Editor/Debugger
26.2 Python Editor/Debugger
JupyterLab

Pycharm (Community) Running on Host

Running Python via EAS with / without Blockly using web-pdb interface for debugger.

### PDF page 2434
<a id="pdf-page-2434"></a>
##### 26.3 Python Functions
User can choose where the Python is to run - whether locally on host or on target.

26.3 Python Functions
import mmcpp_lib
print("import time")
import time
print("open connection")
import sys
cGlobInst = mmcpp_lib.CMMCPPGlobal_Instance() # CMMCPPGlobal
cGlobInst.SetThrowFlag(True, False)
print("set throw flag")
cConn = mmcpp_lib.CMMCConnection() # CMMCConnection

gConnHndl = cConn.ConnectIPCEx(0x7fffffff,None) # Connection Handle (for IPC only)

# cConn.Connect("192.168.1.123","192.168.1.3") # required for RPC only
# gConnHndl = cConn.ConnectionHandle() # required for RPC only
a1 = mmcpp_lib.CMMCSingleAxis() # CMMCSingleAxis
a1.InitAxisData("a01.Axis 1",gConnHndl)

v1 = mmcpp_lib.CMMCGroupAxis() # CMMCGroupAxis
v1.InitAxisData("v01",gConnHndl)

### PDF page 2435
<a id="pdf-page-2435"></a>
[No extractable text on this page.]
