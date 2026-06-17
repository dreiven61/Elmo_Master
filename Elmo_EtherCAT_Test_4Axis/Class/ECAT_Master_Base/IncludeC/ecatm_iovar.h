/*[CR]**************************************************************************
   Copyright (c) by IBV - Echtzeit- und Embedded GmbH & Co. KG
   All Rights reserved.
*/

/*[FH]**************************************************************************

   PROJECT:    icNET
   MODULE:     icECAT
   FILE:       ecatm_iovar.h
   CONTENTS:   EtherCAT - processimage access functions
*/

/*[CL]**************************************************************************

   DD-MM-YYYY  NNN   <change log>
   ---
   NNN: IBV, www.ibv-augsburg.net
*/

/*[MP]**************************************************************************
 */

#ifndef INCLUDE_ECATM_IOVAR_H_
#define INCLUDE_ECATM_IOVAR_H_

/*******************************************************************************
 * local includes
 ******************************************************************************/

// #include "iclog.h"

/*******************************************************************************
 * macros
 ******************************************************************************/
/*
 * helper access macros
 */

/* bitmask generation for read modify write */
#define LOWER_N_BITMASK(n)            ((1 << (n)) -1 )

/* get 16bit value from image */
#define GETCUR_16(ovar__, curval__) \
              curval__ = *ovar__++; \
              curval__ = curval__ | ((*ovar__) << 8);

/* get 16bit value from image */
#define GETCUR_32(ovar__, curval__) \
              curval__ = *ovar__++; \
              curval__ = curval__ | ((*ovar__++) <<  8); \
              curval__ = curval__ | ((*ovar__++) << 16); \
              curval__ = curval__ | ((*ovar__)   << 24); \

/*----------------------------------------------------------------------------*/

/* call function to access fixed size values */
#define ECATM_VAR_ACC(cycctx__, func__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
do{ \
   const _eceni_iovar *iovarint__; \
   if (iovarid__ != -1)\
   {\
      ICLOGFATAL(ecatm_iovar_getbyid((cycctx__)->ecatm, (iovarid__), &iovarint__))\
   }\
   else\
      iovarint__ = iovar__;\
   \
   ICLOGFATAL(func__((cycctx__), (iovarint__), (valp__)))\
}while (0)

/*----------------------------------------------------------------------------*/

/* call function to access multiple bits/bytes */
#define ECATM_VAR_ACC_N(cycctx__, func__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, size__) \
do{ \
   const _eceni_iovar *iovarint__; \
   if (iovarid__ != -1)\
      ICLOGFATAL(ecatm_iovar_getbyid((cycctx__)->ecatm, (iovarid__), &iovarint__))\
   else\
      iovarint__ = iovar__;\
   \
   ICLOGFATAL(func__((cycctx__), (iovarint__), (valp__), (size__)))\
}while (0)

/*----------------------------------------------------------------------------*/

/*
 * common access macros (for fast and enhanced checks access)
 */
#define ECATM_IVAR_RDNBIT(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, nbit__) \
   ECATM_VAR_ACC_N(cycctx__, ecatm_iovar_in_rdnbit, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, nbit__)

#define ECATM_IVAR_RDNBYTE(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, size__) \
   ECATM_VAR_ACC_N(cycctx__, ecatm_iovar_in_rdnbytes, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, size__)

#define ECATM_OVAR_WRNBIT(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, nbit__) \
   ECATM_VAR_ACC_N(cycctx__, ecatm_iovar_out_wrnbit, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, nbit__)

#define ECATM_OVAR_WRNBYTE(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, size__) \
   ECATM_VAR_ACC_N(cycctx__, ecatm_iovar_out_wrnbytes, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, size__)

#define ECATM_OVAR_RDNBIT(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, nbit__) \
   ECATM_VAR_ACC_N(cycctx__, ecatm_iovar_out_rdnbit, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, nbit__)

#define ECATM_OVAR_RDNBYTE(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, size__) \
   ECATM_VAR_ACC_N(cycctx__, ecatm_iovar_out_rdnbytes, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__, size__)

#if COMPILE_ECATM_ENABLE_ENH_CHECKS
/*
 * access macros to I/O variables in cyclic frames. a) with enhanced checks
 */
/*
 * Read access on input image
 */

#define ECATM_IVAR_RD1(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
   ECATM_VAR_ACC(cycctx__, ecatm_iovar_in_rd1, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__)

#define ECATM_IVAR_RD8(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
   ECATM_VAR_ACC(cycctx__, ecatm_iovar_in_rd8, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__)

#define ECATM_IVAR_RD16(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
   ECATM_VAR_ACC(cycctx__, ecatm_iovar_in_rd16, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__)

#define ECATM_IVAR_RD32(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
   ECATM_VAR_ACC(cycctx__, ecatm_iovar_in_rd32, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__)

/*----------------------------------------------------------------------------*/
/*
 * Write access on output image
 */

#define ECATM_OVAR_WR1(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
   ECATM_VAR_ACC(cycctx__, ecatm_iovar_out_wr1, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__)

#define ECATM_OVAR_WR8(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
   ECATM_VAR_ACC(cycctx__, ecatm_iovar_out_wr8, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__)


#define ECATM_OVAR_WR16(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
   ECATM_VAR_ACC(cycctx__, ecatm_iovar_out_wr16, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__)

#define ECATM_OVAR_WR32(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
   ECATM_VAR_ACC(cycctx__, ecatm_iovar_out_wr32, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__)

/*----------------------------------------------------------------------------*/
/*
 * Read access on output image
 */

#define ECATM_OVAR_RD1(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
   ECATM_VAR_ACC(cycctx__, ecatm_iovar_out_rd1, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__)

#define ECATM_OVAR_RD8(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
   ECATM_VAR_ACC(cycctx__, ecatm_iovar_out_rd8, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__)

#define ECATM_OVAR_RD16(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
   ECATM_VAR_ACC(cycctx__, ecatm_iovar_out_rd16, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__)

#define ECATM_OVAR_RD32(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
   ECATM_VAR_ACC(cycctx__, ecatm_iovar_out_rd32, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__)

#else

/*
 * access macros to I/O variables in cyclic frames. b) fast macros without checks
 */

/*
 * Read access on input image
 */
/* read bit from input image  */
#define ECATM_IVAR_RD1(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
{ \
   uint8_t curval__ = ((cycctx__)->rcvfrm[(frmidx__)][(frmoff__)]); \
  *(valp__) = curval__ & (1 << (bitshift__)) ? 1 : 0; \
}

/* read 8bit from input image */
#define ECATM_IVAR_RD8(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
{ \
   uint8_t tmpvar__ = ((cycctx__)->rcvfrm[(frmidx__)][(frmoff__ + 1)]); \
   uint8_t curval__ = ((cycctx__)->rcvfrm[(frmidx__)][(frmoff__)]); \
  *(valp__) = bitshift__ == 0 ? curval__ : \
                               (curval__ >> (bitshift__)) | \
                               (tmpvar__ << (8 - (bitshift__)));\
}

/* read 16bit from input image */
#define ECATM_IVAR_RD16(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
{ \
   uint16_t tmpvar__ = (uint16_t)((cycctx__)->rcvfrm[(frmidx__)][(frmoff__ + 2)]); \
   uint8_t  *curvalptr__ =      &((cycctx__)->rcvfrm[(frmidx__)][(frmoff__)]);\
   uint16_t curval__;\
   GETCUR_16(curvalptr__, curval__); \
  *(valp__) = bitshift__ == 0 ? curval__ : \
                               (curval__ >> (bitshift__)) | \
                               (tmpvar__ << (16 - (bitshift__)));\
}

/* read 32bit from input image */
#define ECATM_IVAR_RD32(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
{ \
   uint32_t tmpvar__ = (uint32_t)((cycctx__)->rcvfrm[(frmidx__)][(frmoff__ + 4)]); \
   uint8_t  *curvalptr__ =      &((cycctx__)->rcvfrm[(frmidx__)][(frmoff__)]);\
   uint32_t curval__;\
   GETCUR_32(curvalptr__, curval__); \
  *(valp__) = bitshift__ == 0 ? curval__ : \
                               (curval__ >> (bitshift__)) | \
                               (tmpvar__ << (32 - (bitshift__)));\
}

/*----------------------------------------------------------------------------*/
/*
 * Write access on output image
 */

/* bit write access for shifted and non shifted values on output image */
#define ECATM_OVAR_WR1(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, val__) \
{ \
   uint8_t *ovar__ = &((cycctx__)->sndfrm[(frmidx__)][(frmoff__)]); \
   *ovar__ = (val__) == 0 ? *ovar__ &~ (1 << (bitshift__)) : \
                          *ovar__ |  (1 << (bitshift__)) ; \
}

/*----------------------------------------------------------------------------*/

/* byte write access for non shifted values on output image */
#define ECATM_OVAR_WR8NOSHIFT(cycctx__, frmidx__, frmoff__, bitshift__, val__) \
{ \
   uint8_t *ovar__ = &((cycctx__)->sndfrm[(frmidx__)][(frmoff__)]); \
   *ovar__ = (uint8_t) (val__); \
}

/* byte write access for shifted values on output image */
#define ECATM_OVAR_WR8SHIFT(cycctx__, frmidx__, frmoff__, bitshift__, val__) \
{ \
   uint8_t *ovar__ = &((cycctx__)->sndfrm[(frmidx__)][(frmoff__)]); \
   *ovar__ = (*ovar__ &  LOWER_N_BITMASK(bitshift__)) | (uint8_t) ((val__) << (bitshift__)); \
   ovar__++; \
   *ovar__ = (*ovar__ & ~LOWER_N_BITMASK(bitshift__)) | (uint8_t) (val__) >> ((8 - (bitshift__))); \
}

/* common byte write access macro */
#define ECATM_OVAR_WR8(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, val__) \
do { \
   if ((bitshift__ == 0)) \
      ECATM_OVAR_WR8NOSHIFT((cycctx__), (frmidx__), (frmoff__), (bitshift__), (val__)) \
   else \
      ECATM_OVAR_WR8SHIFT((cycctx__), (frmidx__), (frmoff__), (bitshift__), (val__)) \
} while (0)

/*----------------------------------------------------------------------------*/

/* 16bit write access for non shifted values on output image */
#define ECATM_OVAR_WR16NOSHIFT(cycctx__, frmidx__, frmoff__, bitshift__, val__) \
{ \
   uint8_t *ovar__ = &((cycctx__)->sndfrm[(frmidx__)][(frmoff__)]); \
   *ovar__++ = (uint8_t) ((val__) >> (0)); \
   *ovar__   = (uint8_t) ((val__) >> (8)); \
 \
}

/* 16bit write access for shifted values */
#define ECATM_OVAR_WR16SHIFT(cycctx__, frmidx__, frmoff__, bitshift__, val__) \
{ \
   uint8_t *ovar__ = &((cycctx__)->sndfrm[(frmidx__)][(frmoff__)]); \
   *ovar__   = (*ovar__ &  LOWER_N_BITMASK(bitshift__)) | (uint8_t) ((val__) << (bitshift__)); \
   ovar__++; \
   *ovar__++ = (uint8_t) ((val__) >> (8 - (bitshift__))); \
   *ovar__   = (*ovar__ & ~LOWER_N_BITMASK(bitshift__)) | (uint8_t) ((val__) >> ((16 - (bitshift__)))); \
 \
}

/* common 16bit write access macro on output image */
#define ECATM_OVAR_WR16(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, val__) \
do { \
   if ((bitshift__ == 0)) \
      ECATM_OVAR_WR16NOSHIFT((cycctx__), (frmidx__), (frmoff__), (bitshift__), (val__)) \
   else \
      ECATM_OVAR_WR16SHIFT((cycctx__), (frmidx__), (frmoff__), (bitshift__), (val__)) \
} while (0)

/*----------------------------------------------------------------------------*/

/* 32bit write access for non shifted values on output image */
#define ECATM_OVAR_WR32NOSHIFT(cycctx__, frmidx__, frmoff__, bitshift__, val__) \
{ \
   uint8_t *ovar__ = &((cycctx__)->sndfrm[(frmidx__)][(frmoff__)]); \
   *ovar__++ = (uint8_t) ((val__) >> (0)); \
   *ovar__++ = (uint8_t) ((val__) >> (8)); \
   *ovar__++ = (uint8_t) ((val__) >> (16)); \
   *ovar__   = (uint8_t) ((val__) >> (24)); \
 \
}

/* 32bit write access for shifted values on output image */
#define ECATM_OVAR_WR32SHIFT(cycctx__, frmidx__, frmoff__, bitshift__, val__) \
{ \
   uint8_t *ovar__ = &((cycctx__)->sndfrm[(frmidx__)][(frmoff__)]); \
   *ovar__   = (*ovar__ &  LOWER_N_BITMASK(bitshift__)) | (uint8_t) ((val__) << (bitshift__)); \
   ovar__++; \
   *ovar__++ = (uint8_t) ((val__) >> (8 - (bitshift__))); \
   *ovar__++ = (uint8_t) ((val__) >> (16 - (bitshift__))); \
   *ovar__++ = (uint8_t) ((val__) >> (24 - (bitshift__))); \
   *ovar__   = (*ovar__ & ~LOWER_N_BITMASK(bitshift__)) | (uint8_t) ((uint64_t)(val__) >> ((32 - (bitshift__)))); \
 \
}

/* common 32bit write access macro on output image */
#define ECATM_OVAR_WR32(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, val__) \
do { \
   if ((bitshift__ == 0)) \
      ECATM_OVAR_WR32NOSHIFT((cycctx__), (frmidx__), (frmoff__), (bitshift__), (val__)) \
   else \
      ECATM_OVAR_WR32SHIFT((cycctx__), (frmidx__), (frmoff__), (bitshift__), (val__)) \
} while (0)

/*----------------------------------------------------------------------------*/
/*
 * Read access on output image
 */
/* read bit from input image  */
#define ECATM_OVAR_RD1(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
{ \
   uint16_t curval__ = ((cycctx__)->sndfrm[(frmidx__)][(frmoff__)]); \
  *(valp__) = curval__ & (1 << (bitshift__)) ? 1 : 0; \
}

/* read 8bit from output image */
#define ECATM_OVAR_RD8(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
{ \
   uint8_t tmpvar__ = ((cycctx__)->sndfrm[(frmidx__)][(frmoff__ + 1)]); \
   uint8_t curval__ = ((cycctx__)->sndfrm[(frmidx__)][(frmoff__)]); \
  *(valp__) = bitshift__ == 0 ? curval__ : \
                               (curval__ >>      (bitshift__)) | \
                               (tmpvar__ << (8 - (bitshift__)));\
}

/* read 16bit from output image */
#define ECATM_OVAR_RD16(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
{ \
   uint16_t tmpvar__ = (uint16_t)((cycctx__)->sndfrm[(frmidx__)][(frmoff__ + 2)]); \
   uint8_t  *curvalptr__ =      &((cycctx__)->sndfrm[(frmidx__)][(frmoff__)]);\
   uint16_t curval__;\
   GETCUR_16(curvalptr__, curval__); \
  *(valp__) = bitshift__ == 0 ? curval__ : \
                               (curval__ >>       (bitshift__)) | \
                               (tmpvar__ << (16 - (bitshift__)));\
}

/* read 32bit from output image */
#define ECATM_OVAR_RD32(cycctx__, iovar__, iovarid__, cycidx__, frmidx__, frmoff__, bitshift__, valp__) \
{ \
   uint32_t tmpvar__ = (uint32_t)((cycctx__)->sndfrm[(frmidx__)][(frmoff__ + 4)]); \
   uint8_t  *curvalptr__ =      &((cycctx__)->sndfrm[(frmidx__)][(frmoff__)]);\
   uint32_t curval__;\
   GETCUR_32(curvalptr__, curval__); \
  *(valp__) = bitshift__ == 0 ? curval__ : \
                               (curval__ >>       (bitshift__)) | \
                               (tmpvar__ << (32 - (bitshift__)));\
}

#endif

/******************************************************************************/

#endif /* INCLUDE_ECATM_IOVAR_H_ */

/*[EOF]************************************************************************/
