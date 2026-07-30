#ifndef ECAT_SLOTMNG_H
#define ECAT_SLOTMNG_H

//*****************************************************************************
//** NewInst 0x8440-0x845F                                                   **
//*****************************************************************************
#define ECSLOTMNG_CMD_MIN 0x8440
#define ECSLOTMNG_CMD_MAX 0x845F

#define ECSLOTMNG_INSTALL_CALLBACK   0x8440
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
// aPara[11]$^t_e_ECAT_STATE  : Not used. Needed compabillity with ECAT_M_INSTALL_CALLBACK
// 
// results
// uiLng := 8;
// aPara[0]$DINT : return Code 
// aPara[4]$DINT : version 
//
// Command Version 3 **
// Not defined

#define ECSLOTMNG_LOGIN_SLAVE        0x8441
// Command Version 1 ** 

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
// aPara[1]$UDINT             : Slave Index
// aPara[2]$pVirtualBase      : This pointer
// aPara[3]$DINT              : Required
// aPara[4]$BDINT             : bdInitModuleFlags (if and when InitModule method of slave should be called, see defines ECAT_INIT_MODUL...)
// aPara[5]$BDINT             : Optionbits (see t_sECSlotbOptions )
// results
// uiLng := 12;
// aPara[0]$DINT : return Code 
// aPara[4]$DINT : version 
// aPara[8]$pVoid : ecatmhdl (Handle of an EtherCAT master stack instance)
//
// Command Version 3 **
// Not defined


#define ECSLOTMNG_GET_ECATMHDL       0x8442
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

#define ECSLOTMNG_GET_ECATSLAVEIDX   0x8443
// CmdStruct
// aPara[0]$DINT              : Command Version : 1

// results
// uiLng := 12;
// aPara[0]$DINT : returncode
// aPara[4]$DINT : version
// aPara[8]$UDINT : Slave Index
// 
// Command Version 2 ** 
// Not defined


#define ECSLOTMNG_CALL_INIT_MODULE   0x8444
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

  TYPE
  
    t_sECSlotbOptions : BDINT
    [
        1 ECXXX_SafetySlotsNeeded,
        2 ECXXX_AsyCANIFNeeded,
        3 ECXXX_SerialIFNeeded_5Entries,
        4 ECXXX_SerialIFNeeded_10Entries,
        5 ECXXX_SerialIFNeeded_20Entries
    ];
    
//    t_sECSlotInfo : STRUCT
//      pThis                   : ^ECAT_SlotBase;
//      Slot                    : UDINT;
//      bRequired               : BOOL;
//      bdInitModuleFlags       : BDINT;
//      bdInitModuleFinished    : BDINT;
//      bOptionsFlags           : t_sECSlotbOptions;
//    END_STRUCT;

  END_TYPE
    
#endif

