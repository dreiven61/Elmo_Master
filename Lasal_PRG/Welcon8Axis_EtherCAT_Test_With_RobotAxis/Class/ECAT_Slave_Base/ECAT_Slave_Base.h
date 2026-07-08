
#ifndef ECAT_SLAVE_BASE_H
#define ECAT_SLAVE_BASE_H

//*****************************************************************************
//** NewInst 0x8500-0x850F                                                   **
//*****************************************************************************

//Overview commands
//ECAT_SB_GET_SLVIDX         0x8500

#define ECAT_SB_GET_SLVIDX     0x8500

// Command Version 1 **

// CmdStruct
// aPara[0]$DINT              : Command Version : 1
//
// results
// uiLng := 12;
// aPara[0]$DINT : version
// aPara[4]$DINT : return Code
// aPara[8]$DINT : SlaveIndex
//
// Command Version 2 **
// Not defined

#ifdef __cplusplus
extern "C"
{
#endif

// abfrage welcher compiler *******************************

#ifndef cCompile

  #ifdef _MSC_VER
    #define cCompile
  #endif
  #ifdef _GNUC
    #define cCompile
  #endif

#endif

// extern oder extern "C" *********************************

#ifdef cCompile

  #ifndef cExtern
    #ifdef __cplusplus
      #define cExtern extern "C"
    #else
      #define cExtern extern
    #endif
  #endif

#endif

#include "..\ECAT_Master_Base\ECAT_Master_Base.h"

#ifdef cCompile
  cExtern void ECAT_SB_MapPDODataRd_C      (void* pCookie, _ecatm_cycctx* p_cycctx, long noData, const _eceni_iovar *iovar, long **ppData);
  cExtern void ECAT_SB_MapPDODataRd_2Byte_C(void* pCookie, _ecatm_cycctx *p_cycctx, long noData, const _eceni_iovar *iovar, short **ppData);
  cextern void ECAT_SB_MapPDODataRd_XByte_C(void* pCookie, _ecatm_cycctx *p_cycctx, long noData, const _eceni_iovar *iovar, long **ppData)
  cExtern void ECAT_SB_MapPDODataWr_C(void* pCookie, _ecatm_cycctx* p_cycctx, long noData, const  _eceni_iovar *iovar, const long **ppData);
  cExtern void ECAT_SB_MapPDODataWr_XByte_C(void* pCookie, _ecatm_cycctx* p_cycctx, long noData, const  _eceni_iovar *iovar, const long **ppData);

#else
  FUNCTION __CDECL GLOBAL ECAT_SB_MapPDODataRd_C VAR_INPUT pThis : pVirtualBase; p_cycctx : ^t_st_ecatm_cycctx;	noData : DINT; iovar : ^t_st_eceni_iovar; pData : ^DINT; END_VAR;
  FUNCTION __CDECL GLOBAL ECAT_SB_MapPDODataRd_2Byte_C VAR_INPUT pThis : pVirtualBase; p_cycctx : ^t_st_ecatm_cycctx;	noData : DINT; iovar : ^t_st_eceni_iovar; pData :  ^INT; END_VAR;
  FUNCTION __CDECL GLOBAL ECAT_SB_MapPDODataRd_XByte_C VAR_INPUT pThis : pVirtualBase; p_cycctx : ^t_st_ecatm_cycctx;	noData : DINT; iovar : ^t_st_eceni_iovar; pData : ^DINT; END_VAR;
  FUNCTION __CDECL GLOBAL ECAT_SB_MapPDODataWr_C VAR_INPUT pThis : pVirtualBase; p_cycctx : ^t_st_ecatm_cycctx;	noData : DINT; iovar : ^t_st_eceni_iovar; pData : ^DINT; END_VAR;
  FUNCTION __CDECL GLOBAL ECAT_SB_MapPDODataWr_XByte_C VAR_INPUT pThis : pVirtualBase; p_cycctx : ^t_st_ecatm_cycctx;	noData : DINT; iovar : ^t_st_eceni_iovar; pData : ^DINT; END_VAR;
#endif

#ifdef __cplusplus
} // extern "C"
#endif

#endif // ECAT_SLAVE_BASE_H
