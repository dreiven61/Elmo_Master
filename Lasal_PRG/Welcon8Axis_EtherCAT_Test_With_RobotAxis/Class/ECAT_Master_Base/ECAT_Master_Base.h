
#ifndef ECAT_MASTER_BASE_H
#define  ECAT_MASTER_BASE_H

//*****************************************************************************
//** NewInst 0x8400-0x841F                                                   **
//*****************************************************************************
#define ECAT_MASTER_CMD_MIN 0x8400
#define ECAT_MASTER_CMD_MAX 0x841F

//Overview commands
//ECAT_M_INSTALL_CALLBACK         0x8400
//ECAT_M_LOGIN_SLAVE              0x8401
//ECAT_M_SET_SLAVE_INFO           0x8402
//ECAT_M_SDO_CALLBACK             0x8403
//ECAT_M_GET_SLAVE_CYCIDX         0x8404
//ECAT_M_EVENT_CALLBACK           0x8405
//ECAT_M_GET_SUPPORTED_FEATURES   0x8406
//ECAT_M_CHECK_SLAVE              0x8407
//ECAT_M_CALL_INIT_MODULE         0x8408
//ECAT_M_GET_HW_PATH              0x8409
//ECAT_M_GET_ECATMHDL             0x840A
//ECAT_M_ASYCOM_CALLBACK          0x840B
//ECAT_M_SET_CLASSSTATE           0x840C
//ECAT_M_START_SDO                0x840D
//ECAT_M_START_ASYCOM             0x840E
//ECAT_M_START_FOE                0x840F
//ECAT_M_FOE_CALLBACK             0x8410
//ECAT_M_START_SSC                0x8411
//ECAT_M_SSC_CALLBACK             0x8412
//ECAT_M_CANCEL_POST_INIT         0x8413

#define ECAT_M_INSTALL_CALLBACK     0x8400

// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$DINT              : ECAT cyc Task ID, from which to call the passed callbacks
// aPara[2]$pVirtualBase      : This Pointer
// aPara[3]$ECAT_MapPDOData   : Pointer callbackmethod PreScan
// aPara[4]$DINT              : Number of data PreScan
// aPara[5]$^t_st_eceni_iovar : Pointer to mapping info PreScan, points to Para[3] elements!
// aPara[6]$^Data             : Pointer to destination Data PreScan, points to Para[3] elements!
// aPara[7]$ECAT_MapPDOData   : Pointer callbackmethod PostScan
// aPara[8]$DINT              : Number of data PostScan
// aPara[9]$^t_st_eceni_iovar : Pointer to mapping info PostScan, points to Para[7] elements!
// aPara[10]$^Data            : Pointer to source Data PostScan, points to Para[7] elements!
//
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
//
// Command Version 2 **
// aPara[0]$DINT              : Command Version : 2
// aPara[1]$DINT              : ECAT cyc Task ID, from which to call the passed callbacks
// aPara[2]$pVirtualBase      : This Pointer
// aPara[3]$ECAT_MapPDOData   : Pointer callbackmethod PreScan
// aPara[4]$DINT              : Number of data PreScan
// aPara[5]$^t_st_eceni_iovar : Pointer to mapping info PreScan, points to Para[3] elements!
// aPara[6]$^Data             : Pointer to destination Data PreScan, points to Para[3] elements!
// aPara[7]$ECAT_MapPDOData   : Pointer callbackmethod PostScan
// aPara[8]$DINT              : Number of data PostScan
// aPara[9]$^t_st_eceni_iovar : Pointer to mapping info PostScan, points to Para[7] elements!
// aPara[10]$^Data            : Pointer to source Data PostScan, points to Para[7] elements!
// aPara[11]$^t_e_ECAT_STATE  : Pointer t_e_ECAT_STATE of the ECAT Module. Used to check if callbacks in UpdateRt (>= ECAT_STATE_SAFEOP), UpdateRtPostScan (>= ECAT_STATE_OP) should be done.
//
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
//
// Command Version 3 **
// Not defined

#define ECAT_M_LOGIN_SLAVE        0x8401
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$UDINT             : Slave Index
// aPara[2]$pVirtualBase      : This pointer
// aPara[3]$DINT              : Required
// aPara[4]$BDINT             : bdInitModuleFlags (if and when InitModule method of slave should be called, see defines ECAT_INIT_MODUL...)
// results
// uiLng := 12;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
// aPara[8]$pVoid : ecatmhdl (Handle of an EtherCAT master stack instance)
//
// Command Version 2 **
// CmdStruct
// aPara[0]$DINT              : Command Version : 2
// aPara[1]$UDINT             : Slave Index
// aPara[2]$pVirtualBase      : This pointer
// aPara[3]$DINT              : Required
// aPara[4]$BDINT             : bdInitModuleFlags (if and when InitModule method of slave should be called, see defines ECAT_INIT_MODUL...)
// aPara[5]$BDINT             : Optionbits (Bit.1 = 1 .. VoE needed for this module )
// results
// uiLng := 12;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
// aPara[8]$pVoid : ecatmhdl (Handle of an EtherCAT master stack instance)
//
// Command Version 3 **
// Not defined

#define ECAT_M_SET_SLAVE_INFO        0x8402
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$pVoid             : ^t_st_eceni_slv_info

// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
//
// Command Version 2 **
// Not defined



#define ECAT_M_SDO_CALLBACK        0x8403
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$UDINT             : Returncode (<> 0 ... Error)
// aPara[2]$BOOL              : Read or Write (0 ... Read , 1 ... Write)
// aPara[3]$DINT              : Datalength
// aPara[4]                   : Data (Datalength bytes)
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
//
// Command Version 2 **
// Not defined

#define ECAT_M_GET_SLAVE_CYCIDX    0x8404
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1

// results
// uiLng := 12;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
// aPara[8]$DINT : cycidx
//
// Command Version 2 **
// Not defined

#define ECAT_M_EVENT_CALLBACK     0x8405
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$pVoid             : pecatmhdl
// aPara[2]$^t_st_ecatm_evt   : event
// aPara[3]$DINT              : EventType (Info,warning,error)
// aPara[4]$UINT              : Callbackinfo (from where the callback is called from (cyclic or realtime)
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version

//
// Command Version 2 **
// Not defined



#define ECAT_M_GET_SUPPORTED_FEATURES 0x8406
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// results
// uiLng := 12;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
// aPara[8]$BDINT : supported Features (Bitfield)

//
// Command Version 2 **
// Not defined


#define ECAT_M_CHECK_SLAVE            0x8407
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$UDINT             : Product Code to Check
// aPara[2]$UDINT             : Revision Number to Check
// aPara[3]$UDINT             : Vendor ID
// aPara[4]$UDINT             : SerialNo

// results
// uiLng := 8;
// aPara[0]$DINT : returncode
// aPara[4]$DINT : version
//
// Command Version 2 **
// Not defined


#define ECAT_M_CALL_INIT_MODULE       0x8408
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$t_e_ECAT_STATE    : Current Master ECAT State

// results
// uiLng := 8;
// aPara[0]$DINT : returncode
// aPara[4]$DINT : version
//
// Command Version 2 **
// Not defined


#define ECAT_M_GET_HW_PATH            0x8409
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1

// results
// uiLng := 12;
// aPara[0]$DINT : returncode
// aPara[4]$DINT : version
// aPara[8]$^void : pointer to Hw path
//
// Command Version 2 **
// Not defined
//
// CmdStruct
// aPara[0]$DINT              : Command Version : 2

// results
// uiLng := 16;
// aPara[0]$DINT : returncode
// aPara[4]$DINT : version
// aPara[8]$^void : pointer to Hw path
// aPara[9]$HSINT : Next Escape Sequence
// aPara[10]$USINT : Next Port No
//
// Command Version 3 **
// Not defined

#define ECAT_M_GET_ECATMHDL           0x840A
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1

// results
// uiLng := 12;
// aPara[0]$DINT : returncode
// aPara[4]$DINT : version
// aPara[8]$pVoid : ecatmhdl (Handle of an EtherCAT master stack instance)
//
// Command Version 2 **
// Not defined




#define ECAT_M_ASYCOM_CALLBACK        0x840B
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$UDINT             : Returncode (<> 0 ... Error)
// aPara[2]$BOOL              : Read or Write (0 ... Read , 1 ... Write)
// aPara[3]$DINT              : adress
// aPara[4]                   : Not used
// aPara[5]                   : Datalength Bytes
// aPara[6]$UDINT             : Working counter (gets incremented with every successful read or write)
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
//
// Command Version 2 **
// Not defined

#define ECAT_M_SET_CLASSSTATE           0x840C
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$t_e_VaranErrors   : ClassState
// aPara[2]$DINT              : Online
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
//
// Command Version 2 **
// Not defined

#define ECAT_M_START_SDO                0x840D

// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$DINT              : Read: 0, Write : 1
// aPara[2]$HDINT             : hIndex
// aPara[3]$HDINT             : hsSubIndex
// aPara[4]$DINT              : dCompleteAccess
// aPara[5]$^USINT            : pReadWriteData
// aPara[6]$UDINT             : udReadWriteLength
// aPara[7]$UDINT             : udTimeout
// aPara[8]$pVoid             : callback this pointer
//
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
//
// Command Version 2 **
// CmdStruct
// aPara[0]$DINT              : Command Version : 2
// aPara[1]$DINT              : Read: 0, Write : 1
// aPara[2]$HDINT             : hIndex
// aPara[3]$HDINT             : hsSubIndex
// aPara[4]$DINT              : dCompleteAccess
// aPara[5]$^USINT            : pReadWriteData
// aPara[6]$UDINT             : udReadWriteLength
// aPara[7]$UDINT             : udTimeout
// aPara[8]$pVoid             : callback this pointer
// aPara[9]$BDINT             : Flags : Bit1 Ignore Slot Increment; Bit2-32 Reserved must be set to 0
//
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
//
// Command Version 3 **
// Not defined

#define ECAT_M_START_ASYCOM             0x840E

// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$DINT              : Read: 0, Write : 1
// aPara[2]$HDINT             : Address
// aPara[3]$pVoid             : pReadWriteData
// aPara[4]$UINT              : uiReadWriteLength
// aPara[5]$UDINT             : udTimeout
// aPara[6]$pVoid             : callback this pointer
//
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
//
// Command Version 2 **
// Not defined

#define ECAT_M_START_FOE                0x840F
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$DINT              : Read or Write or arbort (0 ... Read , 1 ... Write, -1 abort)
// aPara[2]$^Char             : FileName on the ECAT Slave
// aPara[3]$UDINT             : PassWord (Optionl, 0 = not used
// aPara[4]$pVoid             : pFile: pointer to data the is written to the file, file content is written to this databuffer
// aPara[5]$UDINT             : FileSize: Number of data byte  that are written to the file, Maximum Size of the dababuffer (Read is aborted if file exceeds buffer)
// aPara[6]$UDINT             : udTimeout
// aPara[7]$pVoid             : callback this pointer
//
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
//
// Command Version 2 **
// Not defined

#define ECAT_M_FOE_CALLBACK             0x8410
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$iprstate          : READY .. Done, ERROR .. Error, Busy .. Transfer aktive
// aPara[2]$DINT              : Read or Write or arbort (0 ... Read , 1 ... Write, -1 abort)
// aPara[3]$^CHAR             : FileName
// aPara[4]$UDINT             : FileSizeTransferred
// aPara[5]$UINT              : Errorcode from the Slave
// aPara[6]$^CHAR             : ErrorString from the Slave (optional)
// aPara[7]$DINT              : ErrorCode from OS
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
//
// Command Version 2 **
// Not defined

#define ECAT_M_START_SSC                0x8411
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$DINT              : Read: 0, Write : 1
// aPara[2]$DINT              : driveno
// aPara[3]$USINT             : elementflags
// aPara[4]$DINT              : IDN
// aPara[5]$^USINT            : pReadWriteData
// aPara[6]$UDINT             : udReadWriteLength
// aPara[7]$UDINT             : udTimeout
// aPara[8]$pVoid             : callback this pointer

#define ECAT_M_SSC_CALLBACK             0x8412
// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$DINT              : Returncode (<> 0 ... Error)
// aPara[2]$BOOL              : Read or Write (0 ... Read , 1 ... Write)
// aPara[3]$DINT              : driveno
// aPara[4]$USINT             : elementflags
// aPara[5]$DINT              : IDN
// aPara[6]$UDINT             : Datalength
// aPara[7]$UINT              : SoE transfer errocode or 0 on success.
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version
//
// Command Version 2 **
// Not defined

#define ECAT_M_CANCEL_POST_INIT         0x8413
// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$DINT              : Reason for call, see defines below
// results
// uiLng := 8;
// aPara[0]$DINT : return Code
// aPara[4]$DINT : version

#define ECAT_M_CANCEL_POST_INIT_REASON_DEBUG              0
#define ECAT_M_CANCEL_POST_INIT_REASON_WAIT4SAFETYCONFIG  1

//*****************************************************************************
//** DEFINES                                                                 **
//*****************************************************************************

// Return Code definition
// General
#define ECAT_RETOK    0
#define ECAT_RETBUSY 11

#define ECAT_RETERR  -1
#define ECAT_RETERR_CMD_NOT_SUPPORTED   -10
#define ECAT_RETERR_CMD_VERSION         -11
#define ECAT_RETERR_CMD_PARAMETER       -12
#define ECAT_RETERR_CMD_WRONG_MST_STATE -13
#define ECAT_RETERR_CMD_OUT_OF_MEM      -14
#define ECAT_RETERR_CMD_WRONG_SLV_STATE -15
#define ECAT_RETERR_CMD_INVALID_SLV     -16

#define ECAT_RETERR_CMD_INVALID_HWPATH_ESCAPE_SEQU  -20

#define ECAT_RETERR_CMD_CYCIDX_NOTFOUND -100
#define ECAT_RETERR_CMD_COEIDX_NOTFOUND -101
#define ECAT_RETERR_CMD_COEIDX_NOTLIN   -102



// mask for Ethercat Slave to tell master in which EtherCAT state steps the InitModule should be called from the module
#define ECAT_INIT_MODULE_NO_CALL              2#0000
#define ECAT_INIT_MODULE_CALL_INIT            2#0001
#define ECAT_INIT_MODULE_CALL_PREOP           2#0010
#define ECAT_INIT_MODULE_CALL_SAFEOP          2#0100
#define ECAT_INIT_MODULE_CALL_OP              2#1000

#define ECAT_INIT_MODULE_CALL_BITMASK         2#1111

// mask for Ethercat Slave which InitModule Failed
#define ECAT_INIT_MODULE_ERROR_SHL_OFFSET      8
#define ECAT_INIT_MODULE_ERROR_INIT            ECAT_INIT_MODULE_CALL_INIT     shl ECAT_INIT_MODULE_ERROR_SHL_OFFSET
#define ECAT_INIT_MODULE_ERROR_PREOP           ECAT_INIT_MODULE_CALL_PREOP    shl ECAT_INIT_MODULE_ERROR_SHL_OFFSET
#define ECAT_INIT_MODULE_ERROR_SAFEOP          ECAT_INIT_MODULE_CALL_SAFEOP   shl ECAT_INIT_MODULE_ERROR_SHL_OFFSET
#define ECAT_INIT_MODULE_ERROR_OP              ECAT_INIT_MODULE_CALL_OP       shl ECAT_INIT_MODULE_ERROR_SHL_OFFSET
#define ECAT_INIT_MODULE_ERROR_BITMASK         ECAT_INIT_MODULE_CALL_BITMASK  shl ECAT_INIT_MODULE_ERROR_SHL_OFFSET

// Debug info
// #define ECAT_DEBUG


#ifdef ECAT_DEBUG
  #define ECAT_DEBUG_NO_POST_INIT
//  #define ECAT_DEBUG_PDO_MAPPING
//  #define ECAT_DEBUG_EVENT_CALLBACKS  //Attention: Events will be logged (maybe logging in realtime)
//  #define ECAT_DEBUG_TIMEMEASURE
//  #define ECAT_DEBUG_TIMEMEASURE2
//  #define ECAT_DEBUG_FRAMETIME
//  #define ECAT_DEBUG_DISABLE_REQUIRED

//  #define ECAT_TRACE_VIEW_LOG

  #ifdef ECAT_TRACE_VIEW_LOG

    #define ECAT_SYSTRACE_PRESCAN_UPDATECYC   0x2000
    #define ECAT_SYSTRACE_PRESCAN_CALLBACKS   0x2001
    #define ECAT_SYSTRACE_PRESCAN_UPDATECYC_UNLOCK   0x2002

    #define ECAT_SYSTRACE_POSTSCAN_UPDATECYC  0x2010
    #define ECAT_SYSTRACE_POSTSCAN_CALLBACKS  0x2011

    //Postinit Trace
    #define ECAT_SYSTRACE_POSTINIT_START_INIT           0x2020
    #define ECAT_SYSTRACE_POSTINIT_CALL_INITMODULE      0x2021
    #define ECAT_SYSTRACE_POSTINIT_CHECK_ETHERCAT_STATE 0x2022
    #define ECAT_SYSTRACE_POSTINIT_IS_SYNCRHON          0x2023
    #define ECAT_SYSTRACE_POSTINIT_INIT_FINISHED        0x2024
    #define ECAT_SYSTRACE_POSTINIT_CALL_INITSLOTMODULE  0x2025

    #define ECAT_SYSTRACE_POSTINIT_EVTCALLBACK          0x2030

  #endif

#endif

// I/O variable type
#define ECAT_ECENI_IOVAR_INPUT        0x01
#define ECAT_ECENI_IOVAR_OUTPUT       0x02

//Type for method AddPdoEntry in slave base
#define ECAT_PDO_TYPE_INPUT         0
#define ECAT_PDO_TYPE_OUTPUT        1

#define ECENI_ST_MAC_SIZE           6
#define ECENI_ST_ETHERTYPE_SIZE     2

// event types
#define ECATM_EVTT_ST_INFO          0 //When used as filter setting: Logs Info, Warning and Error Events
#define ECATM_EVTT_ST_WARN          1 //When used as filter setting: Logs Warning and Error Events
#define ECATM_EVTT_ST_ERROR         2 //When used as filter setting: Logs Error Events
#define ECATM_EVTT_ST_USR           3
#define ECATM_EVTT_ST_NUM           4

#define ECATM_INIT_LINK_TIMEOUT                               10000 //Timeout for waiting for the EtherCAT Link
#define ECATM_INIT_CALLINITMODULE_TIMEOUT                      5000 //Timeout for calling the InitModule method of the single EtherCAT slaves
#define ECATM_INIT_CALLINITMODULE_TIMEOUT_WAIT4SAFETYCONFIG  300000 //Timeout for calling the InitModule method of the single EtherCAT slaves if an SCP is waiting for its Safety Configuration
#define ECATM_WAIT_FOR_SYNCHRON_TIMEOUT                      120000 //Timeout while waiting for the syncmeasure to be synchron

// time to wait (in µs) until other tasks in the same or lower priority than post init have done their stuff
#define ECATM_WAIT_TIME_IN_POST_INIT  1000


//EtherCAT commands
#define ECATM_CMD_NOP                0x00  // no operation
#define ECATM_CMD_APRD               0x01  // auto-inc. physical read
#define ECATM_CMD_APWR               0x02  // auto-inc. physical write
#define ECATM_CMD_APRW               0x03  // auto-inc. physical read / write
#define ECATM_CMD_FPRD               0x04  // configured physical read
#define ECATM_CMD_FPWR               0x05  // configured physical write
#define ECATM_CMD_FPRW               0x06  // configured physical read / write
#define ECATM_CMD_BRD                0x07  // broadcast read
#define ECATM_CMD_BWR                0x08  // broadcast write
#define ECATM_CMD_BRW                0x09  // broadcast read / write
#define ECATM_CMD_LRD                0x0A  // logical read
#define ECATM_CMD_LWR                0x0B  // logical write
#define ECATM_CMD_LRW                0x0C  // logical read / write
#define ECATM_CMD_ARMW               0x0D  // auto-inc. phys. RD multiple WR
#define ECATM_CMD_FRMW               0x0E  // configured phys. RD multiple WR
#define ECATM_CMD_FIRST              ECAT_CMD_NOP      // for range check
#define ECATM_CMD_LAST               ECAT_CMD_FRMW     // for range check


#define ECATM_CIL_SOE_SUPPORT               0x00010003
#define ECATM_CIL_EEPROM_SUPPORT            0x00010003
#define ECATM_CIL_ENI_GET_TRAILING_SUPPORT  0x00010004

//*****************************************************************************
//** TYPES                                                                   **
//*****************************************************************************

  TYPE

//# if defined(_WIN32)
//typedef signed char        int8_t;
//typedef short              int16_t;
//typedef int                int32_t;
//typedef long long          int64_t;
//typedef unsigned char      uint8_t;
//typedef unsigned short     uint16_t;
//typedef unsigned int       uint32_t;
//typedef unsigned long long uint64_t;
//#endif
//
//#if defined(_WIN64)
//typedef signed char        int8_t;
//typedef short              int16_t;
//typedef int                int32_t;
//typedef long long          int64_t;
//typedef unsigned char      uint8_t;
//typedef unsigned short     uint16_t;
//typedef unsigned int       uint32_t;
//typedef unsigned long long uint64_t;
//#endif


//    /* ENI: EtherCATConfig (only dimensions) */
//    typedef struct __eceni_eccfg_info
//    {
//       const char           *enifilename;
//       const char           *verxml;       /* <EtherCATConfig Version> attribute  */
//       int                  numslv;        /* number of configured slaves         */
//       int                  numcyc;        /* number of configured cyclic tasks   */
//       int                  numpimg;       /* number of process images (0 or 1)   */
//    } _eceni_eccfg_info;

    t_st_eceni_eccfg_info : STRUCT
      enifilename : ^char;
      verxml      : ^char;
      numslv      : DINT;
      numcyc      : DINT;
      numpimg     : DINT;
    END_STRUCT;


//    /* descriptor for a send or receive frame */
//    typedef struct __ecatm_dlfrm_desc
//    {
//       struct __ecatm_dlfrm_desc
//                            *next;         /* ptr to next frame (only in rcvqueue)*/
//       uint8_t              *buf;          /* ptr to LL EtherCAT frame            */
//       int                  size;          /* size of the frame                   */
//       int                  len;           /* snd: filled len in frame (with HDR) */
//                                           /* rcv: size of ECAT frame, without CRC*/
//
//    } _ecatm_dlfrm_desc;

    t_st_ecatm_dlfrm_desc : STRUCT
      next      : ^t_st_ecatm_dlfrm_desc;
      buf       : ^USINT;
      size      : DINT;
      len       : DINT;
    END_STRUCT;

//    /* ECAT frame handle (consisting of send and corresponding receive frame)     */
//    typedef struct __ecatm_dlfrm
//    {
//       _ecatm_dlfrm_desc    snd;           /* send frame descriptor               */
//       _ecatm_dlfrm_desc    *rcv;          /* receive frame descriptor            */
//                                           /* => linked to frm handle at runtime  */
//                                           /* 0 if no frame was received          */
//       uint8_t              idx;           /* frame index (placed in IDX field)   */
//    } _ecatm_dlfrm;

    t_st_ecatm_dlfrm : STRUCT
      snd       : t_st_ecatm_dlfrm_desc;
      rcv       : ^t_st_ecatm_dlfrm_desc;
      idx       : USINT;
    END_STRUCT;

//------------------------------------------------------------------------------
// Cyclic
//------------------------------------------------------------------------------


//    /* ENI: EtherCATConfig : Config : Cyclic : Info */
//    typedef struct __eceni_cyc_info
//    {
//       const char           *comment;      /* comment  [0...1]                    */
//                                           /* "-" if not specified                */
//       int                  cyctime;       /* in usec  [0...1]                    */
//                                           /* 0 if not specified                  */
//       int                  priority;      /* priority [0...1]                    */
//                                           /* 0 if not specified, else 1 ... 62   */
//       const char           *taskid;       /* TaskId   [0...1]                    */
//       int                  numfrm;        /* numFrame [0...n]                    */
//    } _eceni_cyc_info;

    t_st_eceni_cyc_info : STRUCT
      comment   : ^char;
      cyctime   : DINT;
      priority  : DINT;
      taskid    : ^char;
      numfrm    : DINT;
    END_STRUCT;

//    /* ENI: EtherCATConfig : Config : Cyclic : Frame : Cmd : CopyInfos */
//    typedef struct __eceni_cyc_cpyinfo
//    {
//       const struct __eceni_cyc_cpyinfo *next;
//       int                  srcbitoffs;
//       int                  dstbitoffs;
//       int                  binlen;
//    } _eceni_cyc_cpyinfo;

    t_st_eceni_cyc_cpyinfo : STRUCT
      next          : ^t_st_eceni_cyc_cpyinfo;
      srcbitoffs    : DINT;
      dstbitoffs    : DINT;
      binlen        : DINT;
    END_STRUCT;

//    /* ENI: EtherCATConfig : Config : Cyclic : Frame (1...n) */
//    typedef struct __eceni_cyc_frame
//    {
//       const struct __eceni_cyc_frame *next;
//       const char           *comment;      /* Comment [0...1]                     */
//                                           /* "-" if not specified                */
//       const _eceni_cyc_cmd *cmd;          /* Cmd [1...n]                         */
//    } _eceni_cyc_frm;

    t_st_eceni_cyc_frm : STRUCT
      next          : ^t_st_eceni_cyc_frm;
      comment       : ^CHAR;
      cmd           : ^t_st_eceni_cyc_cmd;
    END_STRUCT;

//    /* ENI: EtherCATConfig : Config : Cyclic : Frame : Cmd (1...n) */
//    typedef struct __eceni_cyc_cmd
//    {
//       const struct __eceni_cyc_cmd *next;
//       uint8_t              statemask;  /* ECAT_STATE_... value can be OR'ed[1..4]*/
//       uint8_t              iovarmask;  /* ECENI_IOVAR_...value can be OR'ed      */
//                                        /* bits are set if cmd contains an        */
//                                        /* iovar                                  */
//       const char           *comment;   /* comment [0...1]                        */
//                                        /* "-" if not specified                   */
//       _eceni_pdu           pdu;        /* protocol data unit sub structure       */
//       int                  cnt;        /* expected WKC [0..1]                    */
//                                        /* ECENI_CNT_UNDEFINED if not specified   */
//       int                  inoffs;     /* input offsets                          */
//                                        /* 0 if not specified                     */
//       int                  outoffs;    /* output offsets                         */
//                                        /* 0 if not specified                     */
//       const _eceni_cyc_cpyinfo   *cpyinfo;   /* copy info [0...1]                */
//    } _eceni_cyc_cmd;

    t_st_eceni_cyc_cmd : STRUCT
      next          : ^t_st_eceni_cyc_cmd;
      statemask     : USINT;
      iovarmask     : USINT;
      comment       : ^CHAR;
      pdu           : t_st_eceni_pdu;
      cnt           : DINT;
      inoffs        : DINT;
      outoffs       : DINT;
      cpyinfo       : ^t_st_eceni_cyc_cpyinfo;
    END_STRUCT;


//    /* ENI: EtherCATConfig : Config : Cyclic : Frame (1...n) */
//    typedef struct __eceni_cyc_frame
//    {
//       const struct __eceni_cyc_frame *next;
//       const char           *comment;      /* Comment [0...1]                     */
//                                           /* "-" if not specified                */
//       const _eceni_cyc_cmd *cmd;          /* Cmd [1...n]                         */
//    } _eceni_cyc_frm;

    t_st_eceni_cyc_frame : STRUCT
      next      : ^t_st_eceni_cyc_frame;
      comment   : ^CHAR;
      cmd       : t_st_eceni_cyc_cmd;
    END_STRUCT;

//    /* ENI: EtherCATConfig : Config : Cyclic (0...n) */
//    typedef struct __eceni_cyc
//    {
//       const struct __eceni_cyc   *next;   /* linked list of cyc frames, 0 at end */
//       _eceni_cyc_info            info;
//       const _eceni_cyc_frm       *frm;    /* cyclic frames [1...n]               */
//    } _eceni_cyc;

    t_st_eceni_cyc : STRUCT
      next      : ^t_st_eceni_cyc;
      info      : t_st_eceni_cyc_info;
      frm       : t_st_eceni_cyc_frm;
    END_STRUCT;


//    /* configuration of cyclic task */
//    typedef struct __ecatm_cyccfg
//    {
//       int                  cyctime_us;    /* cycle time in usec                  */
//       _ecatm_prcfct        prcfct;        /* process function callout            */
//       void                 *arg;          /* argument, passed to process callback*/
//       _ecatm_evtfct        evtfct;        /* event function callout              */
//
//                                           /* buffer for iovar access from other  */
//                                           /* task                                */
//       int                  iovarbufnum;   /* number of elements                  */
//       int                  iovarbufsize;  /* data buffer size                    */
//
//    } _ecatm_cyccfg;

    t_st_ecatm_cyccfg : STRUCT
      cyctime_us    : DINT;
      prcfct        : pVoid;
      arg           : pVoid;
      evtfct        : pVoid;
      iovarbufnum   : DINT;
      iovarbufsize  : DINT;
    END_STRUCT;

//    /* information for one of the cyclic frames */
//    typedef struct __ecatm_cyc_frminfo
//    {
//       /* ptr to table with frame pairs, num entries: num frames of cyc task      */
//       _ecatm_dlfrm      new;              /* cyclic frame used for send/recv     */
//       _ecatm_dlfrm      last;             /* last valid frame, used for pimg acc.*/
//       int               allnops;          /* 1 if all commands are NOPs          */
//    } _ecatm_cycfrminfo;

    t_st_ecatm_cycfrminfo : STRUCT
      new        : t_st_ecatm_dlfrm;
      last       : t_st_ecatm_dlfrm;
      allnops    : DINT;
    END_STRUCT;

//    /* descriptor queue handle */
//    typedef struct __ecatm_dq
//    {
//       int            descsize;            /* size of caller's descriptor         */
//       int            itemsize;            /* size of one desc item (incl. hd)    */
//       int            numdesc;             /* number of descriptors               */
//       uint8_t        *descbuf;            /* buffer for descriptors              */
//       uint8_t        *descwr;             /* write ptr in desc ring buffer       */
//                                           /* 0 => no buffer allocated            */
//       uint8_t        *descrd;             /* read  ptr in desc ring buffer (or 0)*/
//
//       int            databufsize;         /* size of data buffer                 */
//       uint8_t        *databuf;            /* (ring-) buffer for data             */
//       uint8_t        *datawr;             /* write ptr in data ring buffer       */
//       uint8_t        *datard;             /* read  ptr in data ring buffer (or 0)*/
//
//       _ecatm_mtx_cyc mtx;                 /* mutex hdl for thread save           */
//
//    } _ecatm_dq;


    t_st_ecatm_dq : STRUCT

      descsize        : DINT;
      itemsize        : DINT;
      numdesc         : DINT;
      descbuf         : ^USINT;
      descwr          : ^USINT;
      descrd          : ^USINT;
      databufsize     : DINT;
      databuf         : ^USINT;
      datawr          : ^USINT;
      datard          : ^USINT;
      smtx             : pVoid;  //t_st_ecatm_mtx_cyc;
    END_STRUCT;

//    /* statistics for cyclic and acyclic task */
//    typedef struct __ecatm_task_stats
//    {
//       unsigned int         numloops;      /* number of loops                     */
//
//       unsigned int         cyctime_us;    /* cycle time set for the task         */
//
//       struct                              /* error counter for ...               */
//       {
//          unsigned int      frm;           /* ECAT frm with bad content           */
//          unsigned int      wkc;           /* unexpected working cnt              */
//          unsigned int      olddata;       /* invalid or old process data         */
//       }                    errcnt;
//    } _ecatm_task_stats;

    t_st_ecatm_task_stats : STRUCT
      numloops        : UDINT;
      cyctime_us      : UDINT;
      frm             : UDINT;
      wkc             : UDINT;
      olddata         : UDINT;
    END_STRUCT;


//    /*
//     * _ecatm_prcstatus: frame processing status
//     */
//    /* _ecatm_prcstatus: at least one slave in ... state */
//    #define ECATM_PRCSTAT_SLV_IN_INIT      (1<<0)
//    #define ECATM_PRCSTAT_SLV_IN_PREOP     (1<<1)
//    #define ECATM_PRCSTAT_SLV_IN_SAFEOP    (1<<2)
//    #define ECATM_PRCSTAT_SLV_IN_OP        (1<<3)
//
//    /* _ecatm_prcstatus: changed */
//    #define ECATM_PRCSTAT_CHANGED          (1<<8)
//
//    /* _ecatm_prcstatus: Link error */
//    #define ECATM_PRCSTAT_LINK_ERROR       (1<<9)
//
//    /* _ecatm_prcstatus: Lost frame */
//    #define ECATM_PRCSTAT_FRM_LOST         (1<<10)
//
//    /* _ecatm_prcstatus: bad response PDU */
//    #define ECATM_PRCSTAT_BAD_RSP          (1<<11)
//
//    /* _ecatm_prcstatus: bad WKC of at least one command */
//    #define ECATM_PRCSTAT_BAD_WKC          (1<<12)
//
//    /* _ecatm_prcstatus: input process data is not updated in this cycle
//     * (at least in parts)
//     */
//    #define ECATM_PRCSTAT_INPUTS_OLD       (1<<13)
//
//    /* _ecatm_prcstatus: input process data is not updated in this cycle
//     * (at least in parts)
//     */
//    #define ECATM_PRCSTAT_INPUTS_INVAL     (1<<14)
//
//    /*
//     *  _ecatm_prcstatus: ESM transition in progress:
//     * reqmststate = destination state
//     * mststate    = source state
//     */
//    #define ECATM_PRCSTAT_ESM_TRANS        (1<<15)

//    /* status flags for frame processing */
//    typedef uint32_t _ecatm_prcstatus;




//DC Modes
#define ECATM_DC_CFG_MODE_MST         0
#define ECATM_DC_CFG_MODE_NO_DC       1
#define ECATM_DC_CFG_MODE_SLV_LL_TS   2





#define ECATM_PRCSTAT_ST_SLV_IN_INIT    16#00001
#define ECATM_PRCSTAT_ST_SLV_IN_PREOP   16#00002
#define ECATM_PRCSTAT_ST_SLV_IN_SAFEOP  16#00004
#define ECATM_PRCSTAT_ST_SLV_IN_OP      16#00008

#define ECATM_PRCSTAT_ST_CHANGED        16#00100

#define ECATM_PRCSTAT_ST_LINK_ERROR     16#00200

#define ECATM_PRCSTAT_ST_FRM_LOST       16#00400

#define ECATM_PRCSTAT_ST_BAD_RSP        16#00800

#define ECATM_PRCSTAT_ST_BAD_WKC        16#01000

#define ECATM_PRCSTAT_ST_INPUTS_OLD     16#02000

#define ECATM_PRCSTAT_ST_INPUTS_INVAL   16#04000

#define ECATM_PRCSTAT_ST_ESM_TRANS      16#08000

#define ECATM_PRCSTAT_ST_FRAME_NOT_SENT 16#10000


//#define ECATM_PRCSTAT_ST_MASK_ERROR   ( ECATM_PRCSTAT_ST_LINK_ERROR OR
//    ECATM_PRCSTAT_ST_FRM_LOST OR ECATM_PRCSTAT_ST_BAD_RSP OR ECATM_PRCSTAT_ST_BAD_WKC OR ECATM_PRCSTAT_ST_INPUTS_OLD OR ECATM_PRCSTAT_ST_INPUTS_INVAL )


#define ECATM_PRCSTAT_ST_MASK_ERROR ( ECATM_PRCSTAT_ST_LINK_ERROR OR ECATM_PRCSTAT_ST_INPUTS_INVAL OR ECATM_PRCSTAT_ST_FRAME_NOT_SENT)

#define ECATM_PRCSTAT_ST_MASK_WARN  ( ECATM_PRCSTAT_ST_FRM_LOST OR ECATM_PRCSTAT_ST_BAD_RSP OR ECATM_PRCSTAT_ST_BAD_WKC OR ECATM_PRCSTAT_ST_INPUTS_OLD)

#define ECATM_PRCSTAT_ST_MASK_ERROR_OR_WARN (ECATM_PRCSTAT_ST_MASK_ERROR OR ECATM_PRCSTAT_ST_MASK_WARN)


  t_stb_ecatm_prcstat : BDINT
	  [
	    1 SLV_IN_INIT,
	    2 SLV_IN_PREOP,
	    3 SLV_IN_SAFEOP,
	    4 SLV_IN_OP,
	    5 Bit4,
	    6 Bit5,
	    7 Bit6,
	    8 Bit7,
	    9 CHANGED,
	    10 LINK_ERROR,
	    11 FRM_LOST,
	    12 BAD_RSP,
	    13 BAD_WKC,
	    14 INPUTS_OLD,
	    15 INPUTS_INVAL,
      16 ESM_TRANS,
      17 FRAME_NOT_SENT,
	  ];


//    /* handle of EtherCAT master stack instance */
//    typedef struct __ecatm_hdldata
//    {
//       _ecatm_cfg           cfg;           /* saved ECAT master configuration     */
//       _ecatm_monsrv        monsrv;        /* monitor server information          */
//       _ecatm_eni           eni;           /* ENI related information             */
//
//    #if COMPILE_ECATM_ENABLE_ASYNC
//       _ecatm_async         async;         /* asynchronous transfer               */
//    #endif
//
//    #if COMPILE_ECATM_ENABLE_MBX
//       _ecatm_mbx           mbx;           /* mailbox related information         */
//    #endif
//
//    #if COMPILE_ECATM_ENABLE_COE
//       _ecatm_coe           coe;           /* CoE related information             */
//    #endif
//
//    #if COMPILE_ECATM_ENABLE_SUP
//       _ecatm_sup           sup;           /* data of supervision module          */
//    #endif
//
//    #if COMPILE_ECATM_ENABLE_DC
//       _ecatm_dc            dc;            /* Distributed clocks module */
//    #endif
//
//       _ecatm_net           net;           /* EtherCAT network information        */
//       _ecatm_acyc          acyc;          /* acyclic task                        */
//       _ecatm_cyc           *cyc;          /* ptr to array for cyclic task info   */
//       _ecatm_esmmst        mst;           /* master related information          */
//       _ecatm_esmslv        esmslv;        /* all slaves related information      */
//       _ecatm_slv           *slv;          /* ptr to array with slave info        */
//       _ecatm_dlfrm_hdl     frm;           /* frame layer handle                  */
//       _ecatm_ll_hdl        ll;            /* link layer handle                   */
//
//    } _ecatm_hdldata;
//

    t_st_ecatm_hdldata : STRUCT

      cfg     : t_st_ecatm_cfg;           // saved ECAT master configuration
//      monsrv  : t_st_ecatm_monsrv;        // monitor server information
//      eni     : t_st_ecatm_eni;           // ENI related information
//      async   : t_st_ecatm_async;         // asynchronous transfer
//      mbx     : t_st_ecatm_mbx;           // mailbox related information
//      coe     : t_st_ecatm_coe;           // CoE related information
//      sup     : t_st_ecatm_sup;           // data of supervision module
//      dc      : t_st_ecatm_dc;            // Distributed clocks module
//      net     : t_st_ecatm_net;           // EtherCAT network information
//      acyc    : t_st_ecatm_acyc;          // acyclic task
      cyc       : ^t_st_ecatm_cyc;          // ptr to array for cyclic task info
      mst     : t_st_ecatm_esmmst;        // master related information
      esmslv  : t_st_ecatm_esmslv;        // all slaves related information
      slv     : ^t_st_ecatm_slv;          // ptr to array with slave info
      frm     : t_st_ecatm_dlfrm_hdl;     // frame layer handle
      ll      : t_st_ecatm_ll_hdl;        // link layer handle

  END_STRUCT;

  t_st_ecatm_hdl : ^t_st_ecatm_hdldata;

    //Dummy Structs
    t_st_ecatm_cfg : STRUCT


//       flags          : USINT;
//       esmtmo_msec    : DINT;
//       esmtransflags  : DINT;
//       ethll          : ARRAY[0..ECATM_MAX_NUM_ETHPORTS] OF t_st_ethll;
//       frmtmo_usec    : DINT;
//       numrxbuf       : DINT;
//       acycevtfct     : t_st_ecatm_ectfct;
//       sup            : t_st_ecatm_supcfg;
      Dummmy : ARRAY[0..2767] OF USINT;
    END_STRUCT;

    t_st_ethll : STRUCT
      pFuncs : ^t_st_ethll;
      //hdl    : t_st_ethll_hdl;
    END_STRUCT;


    t_st_ecatm_monsrv : STRUCT
      Dummmy : DINT;
    END_STRUCT;
    t_st_ecatm_eni : STRUCT
      Dummmy : DINT;
    END_STRUCT;
    t_st_ecatm_async : STRUCT
      Dummmy : DINT;
    END_STRUCT;
    t_st_ecatm_mbx : STRUCT
      Dummmy : DINT;
    END_STRUCT;
    t_st_ecatm_coe : STRUCT
      Dummmy : DINT;
    END_STRUCT;
    t_st_ecatm_sup : STRUCT
      Dummmy : DINT;
    END_STRUCT;
    t_st_ecatm_dc : STRUCT
      Dummmy : DINT;
    END_STRUCT;
    t_st_ecatm_net : STRUCT
      Dummmy : DINT;
    END_STRUCT;
    t_st_ecatm_acyc : STRUCT
      Dummmy : DINT;
    END_STRUCT;

//    /* handle for this layer */
//    typedef struct __ecatm_cyc
//    {
//       _ecatm_cyccfg     cfg;              /* cyclic configuration                */
//       const _eceni_cyc  *enicyc;          /* ptr to ENI config of cyclic task    */
//       const _eceni_cyc_cmd
//                         *brdstatecmd,     /* command to BRD state of all slaves  */
//                         *mbxreadirqcmd;   /* command with mbx read written flags */
//       volatile int      active;           /* 1 if master is activated            */
//
//       _ecatm_cycfrminfo *frminfo;         /* ptr to table cyclic frame info      */
//                                           /* array size: num frames of cyc task  */
//
//       _ecat_state       mstnextstate;     /* next state of the mst ESM           */
//       int               firstloop;        /* 1 in the first loop of the cyc task */
//       _ecatm_mtx_cyc    frmmtx;           /* mutex for locking cyclic rcv frame  */
//       _ecatm_cycctx     cycctx;           /* context for process function        */
//
//       _ecatm_dq         iovarbuf;         /* handle to the iovarbuf used for     */
//                                           /* inter task iovarval write           */
//
//       _ecatm_task_stats stats;            /* task statistics                     */
//       uint64_t          logtslast;        /* timestamp of last logging           */
//       int               logthiscycle;     /* 1 to indicate to log this cycle     */
//
//       int               cpyinfoidx;       /* idx of first cpyinfo table entry    */
//                                           /* -1 => no cpyinfo for the task set   */
//
//    } _ecatm_cyc;

    t_st_ecatm_cyc : STRUCT

      cfg             : t_st_ecatm_cyccfg;
      enicyc          : ^t_st_eceni_cyc;
      brdstatecmd     : ^t_st_eceni_cyc_cmd;
      mbxreadirqcmd   : ^t_st_eceni_cyc_cmd;
      active          : DINT;
      frminfo         : t_st_ecatm_cycfrminfo;

      mstnextstate    : t_e_ECAT_STATE;
      firstloop       : DINT;
      frmmtx          : pVoid; //t_st_ecatm_mtx_cyc;
      cycctx          : ^t_st_ecatm_cycctx;

      iovarbuf        : t_st_ecatm_dq;
      stats           : t_st_ecatm_task_stats;
      logtslast       : ARRAY[0..1] OF DINT;
      logthiscycle    : DINT;

      cpyinfoidx      : DINT;

    END_STRUCT;

    t_st_ecatm_esmmst : STRUCT
      Dummmy : DINT;
    END_STRUCT;
    t_st_ecatm_esmslv : STRUCT
      Dummmy : DINT;
    END_STRUCT;
    t_st_ecatm_slv : STRUCT
      Dummmy : DINT;
    END_STRUCT;
    t_st_ecatm_dlfrm_hdl : STRUCT
      Dummmy : DINT;
    END_STRUCT;
    t_st_ecatm_ll_hdl : STRUCT
      Dummmy : DINT;
    END_STRUCT;








//    /* context for cyclic task, used to access process image */
//    typedef struct __ecatm_cycctx
//    {
//       _ecatm_hdl           ecatm;         /* master stack instance handle        */
//       int                  cycidx;        /* cyclic task index                   */
//       void                 *arg;          /* argument from cyclic configuration  */
//       _ecatm_prcstatus     prcstatus;     /* status flags for frame processing   */
//       _ecat_state          mststate;      /* master state (saved for this cycle) */
//       _ecat_state          reqmststate;   /* requested master state of ESM       */
//       uint16_t             wkcrcv;        /* received WKC (num active slaves)    */
//                                           /* internally: ------------------------*/
//       struct __ecatm_cyc   *__cyc;        /* cyclic task handle                  */
//       struct __ecatm_cyc_frminfo
//                            *__frminfo;    /* cyc frame info array                */
//       uint64_t             nettime;       /* last SOF at the reference slave     */
//       uint64_t             synctime;      /* optimal sync time                   */
//       uint8_t              **sndfrm;      /* array of send frames                */
//       uint8_t              **rcvfrm;      /* array of last valid receive frames  */
//       int                  numframes;     /* number of frames used by this task  */
//    } _ecatm_cycctx;

#pragma pack(push, 1)
    t_st_ecatm_cycctx : STRUCT
      ecatm             : pVoid;
      cycidx            : DINT;
      arg               : pVoid;
      prcstatus         : UDINT;
      mststate          : t_e_ECAT_STATE;
      reqmststate       : t_e_ECAT_STATE;
      wkcrcv            : UINT;
      padding1          : UINT;
      __cyc             : ^t_st_ecatm_cyc;
      __frminfo         : ^t_st_ecatm_cycfrminfo;
      padding2          : UDINT;
      nettime           : ARRAY [0..1] of UDINT;
      synctime          : ARRAY [0..1] of UDINT;
      sndfrm            : ^pVoid;
      rcvfrm            : ^pVoid;
      numframes         : DINT;
      padding3          : UDINT;
    END_STRUCT;
#pragma pack(pop)

//    /* ENI: EntryType */
//    typedef struct __eceni_entry
//    {
//       const struct __eceni_entry *next;
//       uint16_t             index;      /* index of this entry within the PDO     */
//       uint16_t             subidx;     /* SubIndex [0...1]                       */
//                                        /* 0 if not specified                     */
//       int                  bitlen;
//       const char           *name;      /* name [0...n]                           */
//       const char           *comment;   /* comment [0...1]                        */
//                                        /* "-" if not specified                   */
//       int                  datatype;   /* DataType [0...1]                       */
//                                        /* ECAT_DT_UNKNOWN if not specified       */
//       uint8_t              dscale;     /* ECAT_INTDSCALE_... for INT datatypes   */
//    } _eceni_entry;


    t_st_eceni_entry: STRUCT
      next      : ^t_st_eceni_entry;
      index     : UINT;
      subidx    : UINT;
      bitlen    : DINT;
      name      : ^CHAR;
      comment   : ^CHAR;
      datatype  : DINT;
      dscale    : USINT;
    END_STRUCT;


//    /* ENI: EtherCATConfig : Config : ProcessImage */
//    typedef struct __eceni_pimg_info
//    {
//       struct
//       {
//          int               bytesize;
//          int               numvars;       /* number of input / output variables  */
//       }                    in,
//                            out;
//    } _eceni_pimg_info;

    t_st_eceni_pimg_info_detail: STRUCT
      bytesize  : DINT;
      inumvars  : DINT;
    END_STRUCT;
    t_st_eceni_pimg_info: STRUCT
      in    : t_st_eceni_pimg_info_detail;
      out   : t_st_eceni_pimg_info_detail;
    END_STRUCT;


//    /* ENI: EtherCATConfig : Config : ProcessImage : Inputs / Outputs */
//    typedef struct __eceni_pimg_var
//    {
//       const char           *name;
//       int                  datatype;      /* ECAT_DT_...                         */
//                                           /* ECAT_DT_UNKNOWN if not specified    */
//       int                  bitsize;
//       int                  bitoffs;
//    } _eceni_pimg_var;

    t_st_eceni_pimg_var: STRUCT
      pName      : ^CHAR;
      datatype  : DINT;
      bitsize   : DINT;
      bitoffs   : DINT;
    END_STRUCT;

//    /* access information to I/O variable in cyclic frame */
//    typedef struct __eceni_iovar
//    {
//       uint8_t               dir;          /* direction: ECENI_IOVAR_INPUT/OUTPUT */
//       int                   bitsize;      /* size of I/O variable in bits        */
//       uint8_t               cycidx;       /* index of cyclic task                */
//       uint8_t               frmidx;       /* index of frame in cyclic task       */
//       uint16_t              frmoff;       /* byte offset in cyclic frame         */
//       uint8_t               bitshift;     /* start bit in byte                   */
//       const _eceni_pimg_var *var;         /* ptr to ENI config for the I/O var   */
//       int                   slvidx;       /* slave index                         */
//       const _eceni_entry    *pdoentry;    /* PDO entry (first if multiple PDOs   */
//                                           /* entries are mapped to this I/O var) */
//    } _eceni_iovar;

    t_st_eceni_iovar : STRUCT
      dir       : USINT;
      bitsize   : DINT;
      cycidx    : USINT;
      frmidx    : USINT;
      frmoff    : UINT;
      bitshift  : USINT;
      pVar      : ^t_st_eceni_pimg_var;
      slvidx    : DINT;
      pdoentry  : ^t_st_eceni_entry;

    END_STRUCT;


//    /* ENI: EtherCATConfig : Config : Slave : Info */
//    typedef struct __eceni_slv_info
//    {
//       const char           *name;
//       uint16_t             physaddr;         /* 0 if not specified               */
//       int16_t              autoincraddr;
//       uint8_t              physics[4+1];     /* ECAT_ESC_PHYSICS__...            */
//       uint32_t             vendorid;
//       uint32_t             productcode;
//       uint32_t             revisionno;
//       uint32_t             serialno;
//       const char           *productrev;      /* xs:int in [ETG2100] [0...1]      */
//    } _eceni_slv_info;

    t_st_eceni_slv_info : STRUCT
      name        : ^CHAR;
      physaddr    : UINT;
      autoincaddr : INT;
      physics     : ARRAY[0..4] OF USINT;
      vendorid    : UDINT;
      productcode : UDINT;
      revisionno  : UDINT;
      serialno    : UDINT;
      productrev  : ^CHAR;

    END_STRUCT;

//    typedef struct __ecatm_info
//    {
//       const char  *ver_ecatm;
//       int         numlldrv;
//    } _ecatm_info;

    t_st_ecatm_info : STRUCT
      ver_ecatm   : ^CHAR;
      numlldrv    : DINT;
    END_STRUCT;


    t_sSlaveInfo : STRUCT
      pThis                   : ^ECAT_Slave_Base;
      udSlaveIndex            : UDINT;
      bRequired               : BOOL;
      bdInitModuleFlags       : BDINT;
      bdInitModuleFinished    : BDINT;
      udModuleInitDuration_us : UDINT;
      pSlaveInfo              : ^t_st_eceni_slv_info;
      pNext                   : ^t_sSlaveInfo;
    END_STRUCT;

    t_sSlaveLoginList : STRUCT
      udMaxSlaves       : UDINT;
      udLoggedInSlaves  : USINT;
      pSlaves           : ^t_sSlaveInfo;
    END_STRUCT;


//    /* network related DC statistic informations */
//    typedef struct __ecatm_dc_stats
//    {
//       int            cycle_thr_low;       /* lowest delta before sync missed     */
//       int            cycle_thr_high;      /* highest delta before sync missed    */
//
//       unsigned int   thr_missed_low;      /* counter of missed sync thresholds   */
//       unsigned int   thr_missed_high;     /* counter of missed sync thresholds   */
//
//       int            delta_min;           /* lowest delta time measured          */
//       int            delta_max;           /* highest delta time measured         */
//
//       int            cycles_missed;       /* counter of compl. missed sync cycle */
//                                           /* or multiple frames in sync cycle    */
//
//       int            syncwin_cnt;         /* counted sync windows checks         */
//
//       uint32_t       cyctime;             /* cycle time of cyclic task with ARMW */
//
//       uint32_t       syncwin_max;         /* last measured sync window (nsec)    */
//                                           /* max value of ESC register 0x092C    */
//                                           /* 0xffffffff for invalid sync window  */
//    } _ecatm_dc_stats;

    t_st_ecatm_dc_stats : STRUCT
      cycle_thr_low   : DINT;
      cycle_thr_high  : DINT;

      thr_missed_low  : UDINT;
      thr_missed_high : UDINT;

      delta_min       : DINT;
      delta_max       : DINT;

      cycles_missed   : DINT;

      syncwin_cnt     : DINT;

      cyctime         : UDINT;

      syncwin_max     : UDINT;

    END_STRUCT;

//    /* configuration of DC module */
//    /* set 0 to use default values (check defines above ECATM_DC_CFG_DEF_...) */
//    typedef struct __ecatm_dc_cfg
//    {
//       int            mst_shift;           /* SOF relative in SYNC cycle [%]      */
//
//       /* configuration for DC drift compensation (PREOP-SAFEOP transition)       */
//       uint32_t       maxdrift_ns;         /* max. drift between slave systime    */
//                                           /* and reference slave systime         */
//       int            driftcomp_tmo;       /* timeout for drift comp [msec]       */
//
//       int            syncwin_chkcyctime;  /* checking sync window period [msec]  */
//                                           /* 0: disabled                         */
//
//       uint16_t       syncguardcycles;     /* SYNC0 guard cycles between set sync */
//                                           /* and activate sync (default 50)      */
//    } _ecatm_dc_cfg;


    t_st_ecatm_dc_cfg : STRUCT

      mst_shift             : DINT;
      maxdrift_ns           : UDINT;
      driftcomp_tmo         : DINT;
      syncwin_chkcyctime    : DINT;
      syncguardcycles       : UINT;
      dcmode                : USINT;
      padding1              : USINT;
    END_STRUCT;


//    /* slave related DC informations */
//    typedef struct __ecatm_dc_slv_info
//    {
//       int            flags;               /* ECATM_DC_SLV_FLG_...                */
//       int32_t        delay;               /* delay of slave to ref slave in ns   */
//                                           /* -1 invalid delay                    */
//       uint32_t       syncwin;             /* last read sync window (ESC 0x092C)  */
//    }_ecatm_dc_slv_info;

    t_st_ecatm_dc_slv_info : STRUCT
      flags         : DINT;
      delay         : DINT;
      syncwin       : UDINT;
    END_STRUCT;


//    /* network status information */
//    typedef struct __ecatm_net_status
//    {
//       int                  num_slv_exp;   /* number of expected slave (with ESC) */
//       int                  num_slv_curr;  /* number of slaves currently detected */
//       uint16_t             cycbrdstatus;  /* Bit-OR of all ESC AL-Status regs    */
//       uint8_t              flags;         /* ECATM_NET_STATUS_...                */
//    } _ecatm_net_status;

    t_stb_ecatm_net_statusflags : BSINT
	  [
	    1 ECATM_NET_STATUS_WRONG_WKC,
	    2 ECATM_NET_STATUS_SLV_DIFF2MST,
	    3 ECATM_NET_STATUS_MST_LINK_DOWN,
	  ];

    t_st_ecatm_net_status : STRUCT
      num_slv_exp   : DINT;
      num_slv_curr  : DINT;
      cycbrdstatus  : UINT;
      flags         : t_stb_ecatm_net_statusflags;
      padding1      : USINT;
    END_STRUCT;

//    /* ENI: EtherCATConfig : Config : Master : Info */
//    typedef struct __eceni_mst_info
//    {
//       const char           *name;
//       uint8_t              dstmacaddr[ECENI_MAC_SIZE];
//       uint8_t              srcmacaddr[ECENI_MAC_SIZE];
//       uint8_t              ethertype [ECENI_ETHERTYPE_SIZE];
//                                           /* EtherType in little-endian format   */
//                                           /* default {0xa4, 0x88} for EtherCAT   */
//    } _eceni_mst_info;


    t_st_eceni_mst_info : STRUCT
      name        : ^CHAR;
      dstmacaddr  : ARRAY [0..ECENI_ST_MAC_SIZE-1] OF USINT;
      srcmacaddr  : ARRAY [0..ECENI_ST_MAC_SIZE-1] OF USINT;
      ethertype   : ARRAY [0..ECENI_ST_ETHERTYPE_SIZE-1] OF USINT;
    END_STRUCT;

// text size for events
#define ECATM_EVT_ST_TXT_SIZE       (100+1)

    t_e_st_ecatm_evtc :
    (
      ECATM_EVTC_FIRST         := 999 ,
      ECATM_EVTC_BUSY          := 1000, //* 1000 */, ECATM_EVTT_INFO
      ECATM_EVTC_ERR_PARAM            , //* 1001 */, ECATM_EVTT_ERROR
      ECATM_EVTC_ERR_BUFSIZE          , //* 1002 */, ECATM_EVTT_ERROR
      ECATM_EVTC_ERR_NOT_SUPPORTED    , //* 1003 */, ECATM_EVTT_ERROR
      ECATM_EVTC_ERR_TIMEOUT          , //* 1004 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MEM_ALLOC            , //* 1005 */, ECATM_EVTT_ERROR
      ECATM_EVTC_ENI_ERR              , //* 1006 */, ECATM_EVTT_ERROR
      ECATM_EVTC_ENI_WARN             , //* 1007 */, ECATM_EVTT_WARN
      ECATM_EVTC_CYC_PRCSTATUS_CHG    , //* 1008 */, ECATM_EVTT_INFO
      ECATM_EVTC_CYC_NOT_DESTROYED    , //* 1009 */, ECATM_EVTT_ERROR
      ECATM_EVTC_CYC_ERR_NO_RCV_TS    , //* 1010 */, ECATM_EVTT_ERROR
      ECATM_EVTC_CYC_ERR_NO_SND_TS    , //* 1011 */, ECATM_EVTT_ERROR
      ECATM_EVTC_IOVAR_BAD_CFG        , //* 1012 */, ECATM_EVTT_ERROR
      ECATM_EVTC_IOVAR_BAD_ID         , //* 1013 */, ECATM_EVTT_ERROR
      ECATM_EVTC_IOVAR_BAD_PDO        , //* 1014 */, ECATM_EVTT_ERROR
      ECATM_EVTC_IOVAR_BAD_NAME       , //* 1015 */, ECATM_EVTT_ERROR
      ECATM_EVTC_IOVAR_BAD_CYCIDX     , //* 1016 */, ECATM_EVTT_ERROR
      ECATM_EVTC_IOVAR_NO_DATA        , //* 1017 */, ECATM_EVTT_ERROR
      ECATM_EVTC_IOVAR_BAD_CRC        , //* 1018 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MST_NO_ENICFG        , //* 1019 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MST_ENI_LOADED       , //* 1020 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MST_ACTIVATED        , //* 1021 */, ECATM_EVTT_INFO
      ECATM_EVTC_MST_DEACTIVATED      , //* 1022 */, ECATM_EVTT_INFO
      ECATM_EVTC_MST_STATE_CHANGE     , //* 1023 */, ECATM_EVTT_INFO
      ECATM_EVTC_MST_TRANS_FAILED     , //* 1024 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MST_ACTIVATE_ERR     , //* 1025 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MST_ESM_TMO          , //* 1026 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SLV_STATE_CHANGE     , //* 1027 */, ECATM_EVTT_INFO
      ECATM_EVTC_SLV_TRANS_FAILED     , //* 1028 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SLV_ACKERR_FAILED    , //* 1029 */, ECATM_EVTT_ERROR
      ECATM_EVTC_ESM_BAD_CUR_STATE    , //* 1030 */, ECATM_EVTT_ERROR
      ECATM_EVTC_ESM_BAD_REQ_STATE    , //* 1031 */, ECATM_EVTT_ERROR
      ECATM_EVTC_ESM_REQ_BUSY         , //* 1032 */, ECATM_EVTT_WARN
      ECATM_EVTC_INITC_RCV_TMO        , //* 1033 */, ECATM_EVTT_ERROR
      ECATM_EVTC_INITC_VALID_WKC      , //* 1034 */, ECATM_EVTT_ERROR
      ECATM_EVTC_INITC_VALID_LEN      , //* 1035 */, ECATM_EVTT_ERROR
      ECATM_EVTC_INITC_VALID_DATA     , //* 1036 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MBX_BAD_RSP_MBXHD    , //* 1037 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MBX_WR_FAILED        , //* 1038 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MBX_RD_REPEAT_FAIL   , //* 1039 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MBX_WR_REPEAT_FAIL   , //* 1040 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MBX_RD_FAILED        , //* 1041 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MBX_ERR_UNKNOWN_PROT , //* 1042 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MBX_ERR_REPLY        , //* 1043 */, ECATM_EVTT_ERROR
      ECATM_EVTC_COE_BAD_RSP          , //* 1044 */, ECATM_EVTT_ERROR
      ECATM_EVTC_COE_SDO_TRF_ERR      , //* 1045 */, ECATM_EVTT_ERROR
      ECATM_EVTC_COE_SDO_TRF_TMO      , //* 1046 */, ECATM_EVTT_ERROR
      ECATM_EVTC_COE_SDO_UPL_BUFSIZE  , //* 1047 */, ECATM_EVTT_ERROR
      ECATM_EVTC_COE_SDO_ABORT_REQ    , //* 1048 */, ECATM_EVTT_ERROR
      ECATM_EVTC_COE_EMERGENCY_MSG    , //* 1049 */, ECATM_EVTT_ERROR
      ECATM_EVTC_EOE_INITC_BAD_RSP    , //* 1050 */, ECATM_EVTT_ERROR
      ECATM_EVTC_EOE_MBX_TRF_ERR      , //* 1051 */, ECATM_EVTT_ERROR
      ECATM_EVTC_EOE_INITC_TMO        , //* 1052 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FOE_TRF_TMO          , //* 1053 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FOE_BAD_RSP          , //* 1054 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FOE_BAD_FRAMENO      , //* 1055 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FOE_MBX_TOOSMALL     , //* 1056 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FOE_PARAM            , //* 1057 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FRM_RCV_Q_EMPTY      , //* 1058 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FRM_RCV_LEN_TOO_BIG  , //* 1059 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FRM_RCV_BAD_LEN      , //* 1060 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FRM_RCV_BAD_TYPE     , //* 1061 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FRM_BAD_RSP_PDU      , //* 1062 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FRM_BAD_RSP_WKC      , //* 1063 */, ECATM_EVTT_WARN
      ECATM_EVTC_FRM_FULL             , //* 1064 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FRM_BAD_CMD          , //* 1065 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FRM_NO_RCV           , //* 1066 */, ECATM_EVTT_ERROR
      ECATM_EVTC_CFG_MAX_SIZE         , //* 1067 */, ECATM_EVTT_ERROR
      ECATM_EVTC_LLETH_ERR            , //* 1068 */, ECATM_EVTT_ERROR
      ECATM_EVTC_LLETH_RCV_TMO        , //* 1069 */, ECATM_EVTT_WARN
      ECATM_EVTC_LLETH_FRM_NOTSENT    , //* 1070 */, ECATM_EVTT_WARN
      ECATM_EVTC_LLETH_LD_FRM_NOTSENT , //* 1071 */, ECATM_EVTT_WARN
      ECATM_EVTC_LLETH_LINK_DOWN      , //* 1072 */, ECATM_EVTT_WARN
      ECATM_EVTC_LLETH_RX_REARM       , //* 1073 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MON_ERR_TCP          , //* 1074 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MON_ERR_CONN         , //* 1075 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MON_ERR_BAD_MSG      , //* 1076 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MON_ERR_MSGBUFSIZE   , //* 1077 */, ECATM_EVTT_ERROR
      ECATM_EVTC_MON_ERR_VERCHECK     , //* 1078 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SUP_SLV_NUM_CHG      , //* 1079 */, ECATM_EVTT_WARN
      ECATM_EVTC_SUP_SLV_STATE_CHG    , //* 1080 */, ECATM_EVTT_WARN
      ECATM_EVTC_SUP_SLV_ALSTATUS_CHG , //* 1081 */, ECATM_EVTT_WARN
      ECATM_EVTC_SUP_SLV_WKC_UNEXPECT , //* 1082 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SUP_NET_REINIT_REQ   , //* 1083 */, ECATM_EVTT_WARN
      ECATM_EVTC_SUP_SLV_COMM_CHG     , //* 1084 */, ECATM_EVTT_WARN
      ECATM_EVTC_DC_MAX_RETRY         , //* 1085 */, ECATM_EVTT_ERROR
      ECATM_EVTC_DC_SKIPPED           , //* 1086 */, ECATM_EVTT_WARN
      ECATM_EVTC_DC_STATIC_DRIFT      , //* 1087 */, ECATM_EVTT_ERROR
      ECATM_EVTC_DC_NOT_SUPPORTED     , //* 1088 */, ECATM_EVTT_ERROR
      ECATM_EVTC_DC_REFSLAVE_32BIT    , //* 1089 */, ECATM_EVTT_WARN
      ECATM_EVTC_DC_SET_SYNC          , //* 1090 */, ECATM_EVTT_ERROR
      ECATM_EVTC_DC_SYNC_WINDOW       , //* 1091 */, ECATM_EVTT_WARN
      ECATM_EVTC_DC_FOUND             , //* 1092 */, ECATM_EVTT_INFO
      ECATM_EVTC_DC_SYNC_DONE         , //* 1093 */, ECATM_EVTT_INFO
      ECATM_EVTC_DC_CYCLE_PERIOD      , //* 1094 */, ECATM_EVTT_WARN
      ECATM_EVTC_DC_SLV_OFFSET_REQ    , //* 1095 */, ECATM_EVTT_INFO
      ECATM_EVTC_DC_DISABLED          , //* 1096 */, ECATM_EVTT_WARN
      ECATM_EVTC_ESM_DC_FAILED        , //* 1097 */, ECATM_EVTT_ERROR
      ECATM_EVTC_EOE_MBX_BAD_RSP      , //* 1098 */, ECATM_EVTT_ERROR
      ECATM_EVTC_EOE_FRM_TRF_TMO      , //* 1099 */, ECATM_EVTT_WARN
      ECATM_EVTC_EOE_FRM_DROP_HDL     , //* 1100 */, ECATM_EVTT_WARN
      ECATM_EVTC_EOE_FRM_DROP_SIZE    , //* 1101 */, ECATM_EVTT_WARN
      ECATM_EVTC_EOE_SAT_MAC_ADD      , //* 1102 */, ECATM_EVTT_INFO
      ECATM_EVTC_EOE_SAT_MAC_RM       , //* 1103 */, ECATM_EVTT_INFO
      ECATM_EVTC_FRM_BAD_FRMIDX       , //* 1104 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FRM_CFG_OP_SIZE      , //* 1105 */, ECATM_EVTT_ERROR
      ECATM_EVTC_CRD_STATUS_CHANGED   , //* 1106 */, ECATM_EVTT_INFO
      ECATM_EVTC_SOE_SSC_NOTIFY       , //* 1107 */, ECATM_EVTT_INFO
      ECATM_EVTC_SOE_SSC_TRF_TMO      , //* 1108 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SOE_SSC_TRF_ERR      , //* 1109 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SOE_SSC_BAD_RSP      , //* 1110 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SOE_SSC_RD_BUFSIZE   , //* 1111 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SOE_SSC_RD_FRAG      , //* 1112 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SII_EEPROM_BUSY_TMO  , //* 1113 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SII_EEPROM_PDI_TMO   , //* 1114 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SII_INVALID_VERSION  , //* 1115 */, ECATM_EVTT_ERROR
      ECATM_EVTC_HC_TOO_MANY_DEVICES  , //* 1116 */, ECATM_EVTT_ERROR
      ECATM_EVTC_HC_GRP_SEARCH        , //* 1117 */, ECATM_EVTT_INFO
      ECATM_EVTC_HC_GRP_FOUND         , //* 1118 */, ECATM_EVTT_INFO
      ECATM_EVTC_HC_GRP_NO_MATCH      , //* 1119 */, ECATM_EVTT_ERROR
      ECATM_EVTC_HC_GRP_DISCONNECTED  , //* 1120 */, ECATM_EVTT_INFO
      ECATM_EVTC_HC_PORT_CHANGED      , //* 1121 */, ECATM_EVTT_INFO
      ECATM_EVTC_HC_PORT_OPEN         , //* 1122 */, ECATM_EVTT_INFO
      ECATM_EVTC_HC_MAN_PORT_CLOSED   , //* 1123 */, ECATM_EVTT_ERROR
      ECATM_EVTC_HC_MAN_PORT_OPENING  , //* 1124 */, ECATM_EVTT_WARN
      ECATM_EVTC_HC_TOPO_INVALID      , //* 1125 */, ECATM_EVTT_ERROR
      ECATM_EVTC_HC_SLV_NOT_CONNECTED , //* 1126 */, ECATM_EVTT_ERROR
      ECATM_EVTC_DC_PROPDLY_UPDATE    , //* 1127 */, ECATM_EVTT_INFO
      ECATM_EVTC_DC_PROPDLY_FINISHED  , //* 1128 */, ECATM_EVTT_INFO
      ECATM_EVTC_DC_SLV_SYNC_TIMOUT   , //* 1129 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FOE_ERROR_RSP        , //* 1130 */, ECATM_EVTT_ERROR
      ECATM_EVTC_FOE_USER_ABORT       , //* 1131 */, ECATM_EVTT_INFO
      ECATM_EVTC_FOE_UNEXP_RSP        , //* 1132 */, ECATM_EVTT_WARN
      ECATM_EVTC_MBX_WR_PROT_FAILED   , //* 1133 */, ECATM_EVTT_ERROR
      ECATM_EVTC_ESM_INVALID_WKC_SO   , //* 1134 */, ECATM_EVTT_WARN
      ECATM_EVTC_SDOINFO_BAD_RESPONE  , //* 1135 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SDOINFO_UNEXP        , //* 1136 */, ECATM_EVTT_WARN
      ECATM_EVTC_SDOINFO_BAD_FRG      , //* 1137 */, ECATM_EVTT_ERROR
      ECATM_EVTC_SDOINFO_EMPTY        , //* 1138 */, ECATM_EVTT_ERROR
      ECATM_EVTC_LAST

	  )$DINT;
//    /* time stamp with msec */
//    typedef struct __ostime_ms
//    {
//       uint32_t       time;                /* seconds since Jan 1, 1970 UTC       */
//                                           /* on some platforms since start       */
//       int            msec;                /* milliseconds in the second          */
//
//    } _ostime_ms;

    t_st_ecatm_ostime_ms : STRUCT

      timesec   : UDINT;
      msec      : DINT;

    END_STRUCT;

//    /* event information in event callout from ECAT master stack */
//    typedef struct __ecatm_evt
//    {
//       _ostime_ms           ts;
//       uint32_t             evtcode;
//       int                  arg1;
//       int                  arg2;
//       int                  arg3;
//       int                  arg4;
//       char                 txt[ECATM_EVT_TXT_SIZE];
//    } _ecatm_evt;

    t_st_ecatm_evt : STRUCT

      ts      : t_st_ecatm_ostime_ms;
      evtcode : t_e_st_ecatm_evtc;

      arg1    : DINT;
      arg2    : DINT;
      arg3    : DINT;
      arg4    : DINT;

      txt   : array [0..ECATM_EVT_ST_TXT_SIZE] of char;

    END_STRUCT;


//    /* slave status structure */
//    typedef struct __ecatm_slv_status
//    {
//       _ecat_state          curstate;      /* current EtherCAT state              */
//       _ecat_state          expstate;      /* expected EtherCAT state             */
//       uint16_t             alstatuscode;  /* AL_STATUS register value            */
//       uint8_t              flags;         /* ECATM_SLV_STATUS_...                */
//    } _ecatm_slv_status;
//

#pragma pack(push, 4)
    t_st_ecatm_slv_status : STRUCT

      curstate      : t_e_ECAT_STATE;
      expstate      : t_e_ECAT_STATE;
      alstatuscode  : UINT;
      flags         : USINT;
      padding1      : USINT;      //1byte for padding
    END_STRUCT;

#pragma pack(pop)

//
//    /* handle for SDO transfer (upload and download) */
//    typedef struct __ecatm_sdodata
//    {
//       struct __ecatm_sdodata   *prev,
//                                *next;     /* linked list of mbx handles          */
//
//                                           /* information saved from the SDO cmd  */
//       int               slvidx;           /* target slave index                  */
//       uint16_t          index;            /* Index of the CoE Object             */
//       uint8_t           subindex;         /* Subindex of the CoE Object          */
//       int               complacc;         /* use complete access                 */
//       uint8_t           *data;            /* data buffer pointer                 */
//       int               datasize;         /* data size                           */
//       uint64_t          ts_tmo_ms;        /* timeout of the transmission in ms   */
//       uint8_t           is_segmented;     /* use segmented upload or download    */
//       uint8_t           toggle;           /* toggle flag of segmented transfer   */
//       int               trfsize;          /* transferred data size               */
//       int               trmsize;          /* dl: data size currently transmitted */
//                                           /* ul: total data size transmitted     */
//
//       _ecatm_sem        sem;              /* posted on timeout or end of trans   */
//
//       _ecatm_mbxhdl     mbxhdl;
//       _coestm           statefct;         /* current state handler               */
//       uint32_t          abortcode;        /* abort code if transfer is aborted   */
//       int               err;              /* error code of transfer              */
//
//    } _ecatm_sdodata;
//
//
    t_st_ecatm_sdodata : STRUCT

      prev          : ^t_st_ecatm_sdodata;
      next          : ^t_st_ecatm_sdodata;

      slvidx        : DINT;
      index         : HINT;
      subindex      : HSINT;
      complacc      : DINT;
      data          : ^USINT;
      datasize      : UINT;
      ts_tmo_ms     : ARRAY[0..1] OF UDINT;
      is_segmented  : USINT;
      toggle        : USINT;
      trfsize       : DINT;
      trmsize       : DINT;

      sem           : pVoid; //t_st_ecatm_sem;

      mbxhdl        : t_st_ecatm_mbxhdl;
      statefct      : pVoid;
      abortcode     : UDINT;
      err           : DINT;


    END_STRUCT;
//
//    /* handle for SDO transfer */
//    typedef struct __ecatm_sdodata *_ecatm_sdohdl;
    t_st_ecatm_sdohdl : ^t_st_ecatm_sdodata;


//    /* handle for mailbox transfer (write or read service) */
//    typedef struct __ecatm_mbxhdl
//    {
//       struct __ecatm_mbxhdl   *prev,
//                               *next;      /* linked list of mbx handles          */
//
//       /* setup information for mailbox transfer ---------------------------------*/
//       int            slvidx;              /* addressed slave                     */
//       int            datasize;            /* WR: size of data, RD: size of buffer*/
//       uint8_t        prot;                /* ECAT_MBX_PROT_...                   */
//       uint8_t        free;                /* 1 if handle is free                 */
//
//       /* management information for mailbox transfer ----------------------------*/
//       uint8_t        activatereg;         /* activate register of sync manager 0 */
//       uint8_t        repeatcnt;           /* Number of repeats before error      */
//       uint8_t        *repeatbuf;          /* repeat data buffer                  */
//       _mbxstm        statefct;            /* current state handler               */
//       int            frmoffset;           /* offset of PDU in frame              */
//       int            frmoffsetlastbyte;   /* offset of pdu for last byte         */
//                                           /* -1 => mailbox PDU was padded        */
//       int            err;                 /* WR: error code of mbx transfer      */
//
//    } _ecatm_mbxhdldata;


    t_st_ecatm_mbxhdl : STRUCT

      prev              : ^t_st_ecatm_mbxhdl;
      next              : ^t_st_ecatm_mbxhdl;

      slvidx            : DINT;
      datasize          : DINT;
      prot              : USINT;
      free              : USINT;

      activatereg       : USINT;
      repeatcnt         : USINT;
      repeatbuf         : ^USINT;
      statefct          : pVoid;
      frmoffset         : DINT;
      frmoffsetlastbyte : DINT;
      err               : DINT;

    END_STRUCT;

//    /* handle for an asynchronous transfer */
//    typedef struct __ecatm_asynchdl
//    {
//       struct __ecatm_asynchdl *prev,
//                               *next;      /* linked list of async handles        */
//
//                                           /* information saved from transfer cmd */
//       _eceni_pdu           pdu;           /* PDU with saved data from async cmd  */
//       int                  frmoffset;     /* offset of PDU in frame              */
//       uint64_t             ts_tmo_ms;     /* timestamp for timeout               */
//       _asyncstm            statefct;      /* current state handler               */
//       int                  err;           /* error code of transfer              */
//       _ecatm_sem           sem;           /* semaphore for event to blocking cmd */
//    } _ecatm_asynchdldata;


    t_st_ecatm_asyncdata : STRUCT

      prev      : ^t_st_ecatm_asyncdata;
      next      : ^t_st_ecatm_asyncdata;

      pdu       : t_st_eceni_pdu;
      frmoffset : DINT;
      ts_tmo_ms : ARRAY [0..1] OF UDINT;
      statefct  : pVoid;
      err       : DINT;
      sem       : pVoid; //t_st_ecatm_sem;

    END_STRUCT;

//    /* handle for asynchronous transfer */
//    typedef struct __ecatm_asynchdl *_ecatm_asynchdl;
    t_st_ecatm_asynchdl : ^t_st_ecatm_asyncdata;


//    /* data array in protocol data unit (PDU) */
//    typedef struct __ecat_dataary
//    {
//       const uint8_t        *p;            /* pointer to data buffer              */
//       uint16_t             len;           /* number of data bytes                */
//                                           /* if p is 0 and len is > 0 then a     */
//                                           /* data array of the length 'len'      */
//                                           /* filled with 0x00 is configured      */
//    } _eceni_dataary;

    t_st_eceni_dataary : STRUCT
      pData   : ^USINT;
      len     : UINT;
    END_STRUCT;


//    /* EtherCAT protocol data unit (PDU) */
//    typedef struct __ecat_pdu
//    {
//       uint8_t              cmd;           /* ECAT_CMD_...                        */
//       uint32_t             addr;          /* logical or ECAT_ADDR_PHYS(ADP,ADO)  */
//       _eceni_dataary       data;          /* data + data length                  */
//       uint16_t             wkc;           /* working counter                     */
//    } _eceni_pdu;

    t_st_eceni_pdu : STRUCT

      cmd   : USINT;
      addr  : UDINT;
      data  : t_st_eceni_dataary;
      wkc   : UINT;
      padding1 : UINT;

    END_STRUCT;

  // state of FoE transfer
#define ECATM_FOE_INVAL             0  // internal use (handle is freed)
#define ECATM_FOE_IDLE              1  // transfer completed; handle freed
#define ECATM_FOE_DATA_READY        2  // data chunk filled
#define ECATM_FOE_WAIT_BUFFER       3  // buffer transferred move to next
#define ECATM_FOE_BUSY              4  // FoE transfer is busy
#define ECATM_FOE_ERROR             5  // FoE transfer failed; handle freed

#define ECATM_FOE_MAXSTRLEN   32


///* struct for FoE transfer status */
//typedef struct __ecatm_foe_status
//{
//   int              state;             /* ECATM_FOE_...                       */
//   int              transf_bytes;      /* current transferred bytes           */
//   int              lastsize;          /* size of last packet received        */
//   uint16_t         error_code;        /* error code received from slave      */
//   char             error_str[ECATM_FOE_MAXSTRLEN];  /* error string of slv   */
//} _ecatm_foe_status;

    t_st_ecatm_foe_status : STRUCT

      state         : DINT;
      transf_bytes  : DINT;
      lastsize      : DINT;
      error_code    : UINT;
      error_str     : array [0..ECATM_FOE_MAXSTRLEN-1] of CHAR;

    END_STRUCT;

  // state of SII transfer: ECATM_SII_STATE_
#define ECATM_SII_STATE_INVAL       0 // invalid state (internal)
#define ECATM_SII_STATE_BUSY        1 // SII in progress
#define ECATM_SII_STATE_DATA_READY  2 // data buffer in status is valid
#define ECATM_SII_STATE_WAIT_BUFFER 3 // ecatm_sii_set_buffer
#define ECATM_SII_STATE_DONE        4 // SII transfer done (check return)

 // use for numwords to read until the end of the SII (based on the SIZE field)
#define ECATM_SII_READ_TILL_END     0xffffffff

//
#define ECATM_SII_MIN_READ_BUFFER_SIZE 8 // Mininmal neede size for read buffer for ecatm_sii_set_buffer

///* struct for common device identification */
//typedef union __ecatm_sii_devident
//{
//   /* result of the SII request */
//   struct
//   {
//      uint32_t vendorid;               /* vendor ID    0x08                   */
//      uint32_t productcode;            /* product Code 0x0A                   */
//      uint32_t revisionno;             /* revision     0x0C                   */
//      uint32_t serialno;               /* serialnumber 0x0E                   */
//   } i;
//
//   /* internal data for SII processing */
//   struct
//   {
//      uint8_t buffer[16];
//   } priv;
//} _ecatm_sii_devident;

    t_st_ecatm_sii_devident  : STRUCT

      vendorid    : UDINT;
      productcode : UDINT;
      revisionno  : UDINT;
      serialno    : UDINT;

    END_STRUCT;

//typedef struct __ecatm_sii_status
//{
//   uint8_t  *buf;
//   uint32_t bufsize;
//   uint8_t  state;
//} _ecatm_sii_status;

    t_st_ecatm_sii_status   : STRUCT

      buf : ^USINT;
      bufsize : UDINT;
      state : USINT;

    END_STRUCT;

  END_TYPE

#endif
