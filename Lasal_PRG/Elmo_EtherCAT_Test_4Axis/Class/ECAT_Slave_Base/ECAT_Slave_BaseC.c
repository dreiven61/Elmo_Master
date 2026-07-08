
#include "..\ECAT_Master_Base\IncludeC\ecatm.h"
#include "..\ECAT_Master_Base\IncludeC\ecatm_iovar.h"
#include "..\ECAT_Master_Base\IncludeC\eceni.h"
#include <..\class\sigclib\sigclibnew.h>

// Methods that extract data from an EtherCat Frame and copies it to class data. (Rd)
// Methods that takes class data and copies it into an EtherCat Frame (Wr)
// 
// Parameters:
// pCookie  .. not used (this pointer that is needed if the callback is implemented in an st-Class)
// p_cycctx .. Points to a cyclic task context. 
//             The cyclic task context is used to get information of the last received EtherCAT frames of the corresponding cyclic task 
//             and to access the corresponding process variables.
// noData   .. No of process variables to copied form / to frame.
// *iovar   .. List of iovar, contains noData elements.
//             iovar .. access information to I/O variable in cyclic frame.
// ppData   .. List of data pointers, contains noData elements.
//             pData:  Destination for that data that is read from the EtherCAT frame (Rd).
//                     Source for the data that is copied from the EtherCAT frame (Wr).

// Read data from p_cycctx and copies iovar->bitsize/8 byte to pData
// Warning! Does not check the memory size of pData
void ECAT_SB_MapPDODataRdNByte_C(_ecatm_cycctx *p_cycctx, const _eceni_iovar *iovar, long *pData)
{
  uint8_t  *srcvalptr = &((p_cycctx)->rcvfrm[(iovar->frmidx)][(iovar->frmoff)]);
  
  // Ignore bitshift is set or if there is less than 1 byte data
  if ((iovar->bitshift == 0) && (iovar->bitsize >= 8))
  {
    memcpy(pData, srcvalptr, iovar->bitsize/8);
  }  
}

// Read data from p_cycctx and maps it to 4 Byte Variables (Usaly Server)
extern "C" void ECAT_SB_MapPDODataRd_C(void* pCookie, _ecatm_cycctx *p_cycctx, long noData, const _eceni_iovar *iovar, long **ppData)
{

  int8_t   int8;
  int16_t  int16;  

  for (int i=0; i < noData; i++)
  {
  
    switch (iovar->bitsize)
    {
      //No need to check the datatype
      case 1:  ECATM_IVARHDL_RD1 (p_cycctx, iovar, *ppData);
               break;
               
      case 8:  
      
        //Check the datatype here otherwise the value will not be displayed correct on a DINT/UDINT server
        if (iovar->var->datatype == ECAT_DT_SINT)
        {
          ECATM_IVARHDL_RD8 (p_cycctx, iovar, &int8);
          **ppData = (int32_t)(int8);  // Map 1Byte signed to 4byte signed 
        }
        else if ( (iovar->var->datatype == ECAT_DT_USINT) || 
                  (iovar->var->datatype == ECAT_DT_BYTE) )
        {
          ECATM_IVARHDL_RD8 (p_cycctx, iovar, *ppData);
        }
        break;
        
      case 16: 
      
        //Check the datatype here otherwise the value will not be displayed correct on a DINT/UDINT server
        if (iovar->var->datatype == ECAT_DT_INT)
        {
          ECATM_IVARHDL_RD16(p_cycctx, iovar, &int16);
          **ppData = (int32_t)(int16); // Map 2Byte signed to 4byte signed 
        }
        else if ( (iovar->var->datatype == ECAT_DT_UINT) || 
                  (iovar->var->datatype == ECAT_DT_WORD) )
        {
          ECATM_IVARHDL_RD16 (p_cycctx, iovar, *ppData);
        }
        break;
      
      //No need to check the datatype: Value will be shown correct on server (Either UDINT oder DINT)
      case 32: 
        ECATM_IVARHDL_RD32(p_cycctx, iovar, *ppData);
        break;
               
      default: ECAT_SB_MapPDODataRdNByte_C(p_cycctx, iovar, *ppData);
    }
    
    iovar++;
    ppData++;
  }
}

// Read data from p_cycctx and copies iovar->bitsize/8 byte to pData
// Warning! Does not check the memory size of pData
void ECAT_SB_MapPDODataRdNByte_2Byte_C(_ecatm_cycctx *p_cycctx, const _eceni_iovar *iovar, short *pData)
{
  uint8_t  *srcvalptr = &((p_cycctx)->rcvfrm[(iovar->frmidx)][(iovar->frmoff)]);
  
  // Ignore bitshift is set or if there is less than 1 byte data
  if ((iovar->bitshift == 0) && (iovar->bitsize >= 8))
  {
    memcpy(pData, srcvalptr, iovar->bitsize/8);
  }  
}

// Read data from p_cycctx and maps it to 2 Byte Variables
extern "C" void ECAT_SB_MapPDODataRd_2Byte_C(void* pCookie, _ecatm_cycctx *p_cycctx, long noData, const _eceni_iovar *iovar, short **ppData)
{

  int8_t   int8;

  for (int i=0; i < noData; i++)
  {
  
    switch (iovar->bitsize)
    {
      //No need to check the datatype
      case 1:  ECATM_IVARHDL_RD1 (p_cycctx, iovar, *ppData);
               break;
               
      case 8:  
      
        //Check the datatype here otherwise the value will not be displayed correct on a DINT/UDINT server
        if (iovar->var->datatype == ECAT_DT_SINT)
        {
          ECATM_IVARHDL_RD8 (p_cycctx, iovar, &int8);
          **ppData = (int16_t)(int8);  // Map 1Byte signed to 2byte signed 
        }
        else if ( (iovar->var->datatype == ECAT_DT_USINT) || 
                  (iovar->var->datatype == ECAT_DT_BYTE) )
        {
          ECATM_IVARHDL_RD8 (p_cycctx, iovar, *ppData);
        }
        break;
        
      case 16: 
      
        //No need to check the datatype: Both sides are 16 Bit
        ECATM_IVARHDL_RD16 (p_cycctx, iovar, *ppData);
               
      default: ECAT_SB_MapPDODataRdNByte_2Byte_C(p_cycctx, iovar, *ppData);
    }
    
    iovar++;
    ppData++;
  }
}

// Read data from p_cycctx and maps it to x Byte Variables, x is defined by iovar->var->datatype
extern "C" void ECAT_SB_MapPDODataRd_XByte_C(void* pCookie, _ecatm_cycctx *p_cycctx, long noData, const _eceni_iovar *iovar, long **ppData)
{
  for (int i=0; i < noData; i++)
  {
  
    switch (iovar->bitsize)
    {
      //No need to check the datatype
      case 1:  
        ECATM_IVARHDL_RD1 (p_cycctx, iovar, (int8_t*)(*ppData) );  // Map 1 Bit to 1 Byte
        break;
               
      case 8:  
        
        ECATM_IVARHDL_RD8 (p_cycctx, iovar, (int8_t*)(*ppData) );  // Map 1 Byte to 1 Byte
        break;
        
      case 16: 
      
        ECATM_IVARHDL_RD16 (p_cycctx, iovar, (int16_t*)(*ppData) );  // Map 2 Byte to 2 Byte
        break;
      
      case 32: 
      
        ECATM_IVARHDL_RD32(p_cycctx, iovar, *ppData); // Map 4 Byte to 4 Byte
        break;
               
      default: ECAT_SB_MapPDODataRdNByte_C(p_cycctx, iovar, *ppData);
    }
    
    iovar++;
    ppData++;
  }
}

void ECAT_SB_MapPDODataWrNByte_C(_ecatm_cycctx *p_cycctx, const _eceni_iovar *iovar, const long *pData)
{
  uint8_t  *dstvalptr = &((p_cycctx)->sndfrm[(iovar->frmidx)][(iovar->frmoff)]);
  
  // Ignore bitshift is set or if there is less than 1 byte data
  if ((iovar->bitshift == 0) && (iovar->bitsize >= 8))
  {
    memcpy(dstvalptr, pData, iovar->bitsize/8);
  }  
}

// Read data from 4 Byte Variables (Usaly Server) and maps them to p_cycctx
extern "C" void ECAT_SB_MapPDODataWr_C(void* pCookie, _ecatm_cycctx *p_cycctx, long noData, const _eceni_iovar *iovar, const long **ppData)
{
  for (int i=0; i < noData; i++)
  {
  
    switch (iovar->bitsize)
    {
      case 1:  ECATM_OVARHDL_WR1 (p_cycctx, iovar, **ppData);
               break;
               
      case 8:  ECATM_OVARHDL_WR8 (p_cycctx, iovar, **ppData);
               break;
               
      case 16: ECATM_OVARHDL_WR16(p_cycctx, iovar, **ppData);
               break;
               
      case 32: ECATM_OVARHDL_WR32(p_cycctx, iovar, **ppData);
               break; 
               
      default: ECAT_SB_MapPDODataWrNByte_C(p_cycctx, iovar, *ppData);
               break;
    }
    
    iovar++;
    ppData++;
  }
}

// Read data from x Byte Variables and maps it to p_cycctx , x is defined by iovar->var->datatype
extern "C" void ECAT_SB_MapPDODataWr_XByte_C(void* pCookie, _ecatm_cycctx *p_cycctx, long noData, const _eceni_iovar *iovar, const long **ppData)
{
  for (int i=0; i < noData; i++)
  {
  
    switch (iovar->bitsize)
    {
      case 1:  ECATM_OVARHDL_WR1 (p_cycctx, iovar, (int8_t)(**ppData) );  // Map 1 Byte to 1 Bit 
               break;
               
      case 8:  ECATM_OVARHDL_WR8 (p_cycctx, iovar, (int8_t)**ppData); // Map 1 Byte to 1 Byte
               break;
               
      case 16: ECATM_OVARHDL_WR16(p_cycctx, iovar, (int16_t)**ppData);  // Map 2 Byte to 2 Byte
               break;
               
      case 32: ECATM_OVARHDL_WR32(p_cycctx, iovar, **ppData); // Map 4 Byte to 4 Byte
               break; 
               
      default: ECAT_SB_MapPDODataWrNByte_C(p_cycctx, iovar, *ppData);
               break;
    }
    
    iovar++;
    ppData++;
  }
}