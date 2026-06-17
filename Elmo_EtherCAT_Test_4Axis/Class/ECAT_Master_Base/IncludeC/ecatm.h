/*[CR]**************************************************************************
   Copyright (c) by IBV - Echtzeit- und Embedded GmbH & Co. KG
   All Rights reserved.
*/

/*[FH]**************************************************************************

   PROJECT:    icNET
   MODULE:     ecatm
   FILE:       ecatm.h
   CONTENTS:   EtherCAT master stack - API
*/

/*[CL]**************************************************************************

   DD-MM-YYYY  NNN   <change log>
   ---
   NNN: IBV, www.ibv-augsburg.net
*/

#ifndef INCLUDE_ECATM_H_
#define INCLUDE_ECATM_H_

// /*******************************************************************************
//  * compile configuration
//  ******************************************************************************/
//
//#include "ecatm_config.h"

/*******************************************************************************
 * local includes
 ******************************************************************************/

/* c runtime */
#include <stdint.h>                    /* data type definitions */

// /* project */
//#include "ethll.h"                     /* link layer driver interface */
//#include "eceni.h"                     /* EtherCAT ENI definitions */
//#include "ecatm_evt.h"                 /* Event API */
//#include "ecatm_iovar.h"               /* IO variable access API */

/*******************************************************************************
 * defines
 ******************************************************************************/

/*
 * common dimensions
 */

/* device index (0..n => slave, -1 => master) */
#define ECATM_MST_DEVIDX               (-1)

/* max. number of Ethernet link layer ports */
#define ECATM_MAX_NUM_ETHPORTS         1

/* max. number of EtherCAT master tasks (include acyclic task) */
#define ECATM_MAX_TASKS                16

/* task idx for acyclic task */
#define ECATM_ACYC_TASK_IDX            (-1)

/*
 * transflg: behaviour of master state transition
 */

/* perform a re-init request: current state-> INIT -> requested state */
#define ECATM_TRANS_REINIT             (1<<0)

/* continue with master ESM transition although a slave ESM failed */
#define ECATM_TRANS_CONT_ON_FAILURE    (1<<1)

/* ignore invalid WKC during SAFEOP -> OP transition */
#define ECATM_TRANS_IGNORE_WKC_SO      (1<<2)

/*
 * slave status flags
 */
/* connection to slave lost */
#define ECATM_SLV_STATUS_ERR_IND       (1<<0)

/* link to slave is lost */
#define ECATM_SLV_STATUS_NOCOMM        (1<<1)

/*
 * _ecatm_net_status: network status flags
 */
/* WKC is not as configured */
#define ECATM_NET_STATUS_WRONG_WKC     (1<<0)

/* at least one slave is in state unequal to the master state */
#define ECATM_NET_STATUS_SLV_DIFF2MST  (1<<1)

/* master link down */
#define ECATM_NET_STATUS_MST_LINK_DOWN (1<<2)

/* master deactivated */
#define ECATM_NET_STATUS_MST_DEACTIVATED (1<<3)

/*
 * _ecatm_prcstatus: frame processing status
 */
/* _ecatm_prcstatus: at least one slave in ... state */
#define ECATM_PRCSTAT_SLV_IN_INIT      (1<<0)
#define ECATM_PRCSTAT_SLV_IN_PREOP     (1<<1)
#define ECATM_PRCSTAT_SLV_IN_SAFEOP    (1<<2)
#define ECATM_PRCSTAT_SLV_IN_OP        (1<<3)

/* _ecatm_prcstatus: changed */
#define ECATM_PRCSTAT_CHANGED          (1<<8)

/* _ecatm_prcstatus: Link error */
#define ECATM_PRCSTAT_LINK_ERROR       (1<<9)

/* _ecatm_prcstatus: Lost frame */
#define ECATM_PRCSTAT_FRM_LOST         (1<<10)

/* _ecatm_prcstatus: bad response PDU */
#define ECATM_PRCSTAT_BAD_RSP          (1<<11)

/* _ecatm_prcstatus: bad WKC of at least one command */
#define ECATM_PRCSTAT_BAD_WKC          (1<<12)

/* _ecatm_prcstatus: input process data is not updated in this cycle
 * (at least in parts)
 */
#define ECATM_PRCSTAT_INPUTS_OLD       (1<<13)

/* _ecatm_prcstatus: input process data is not updated in this cycle
 * (at least in parts)
 */
#define ECATM_PRCSTAT_INPUTS_INVAL     (1<<14)

/*
 *  _ecatm_prcstatus: ESM transition in progress:
 * reqmststate = destination state
 * mststate    = source state
 */
#define ECATM_PRCSTAT_ESM_TRANS        (1<<15)


/*
 * general configuration flags
 */

/* force a backward transition after ESM slave transition failed */
#define ECATM_CFG_FLG_ESMSLV_BACKONFAIL (1<<0)

/*
 * network supervision configuration flags
 */
/* enable network supervision */
#define ECATM_SUP_CFG_ENABLE           (1<<0)

/* allow supervision to auto acknowledge an error indication of a slave */
#define ECATM_SUP_CFG_AUTO_ACK         (1<<1)

/* allow supervision to use the ESM if current state differs from expected */
#define ECATM_SUP_CFG_AUTO_RESTATE     (1<<2)


/*
 * Logging flags of master stack
 */

/* max. number of cyclic task to support cyclic logging */
#define ECATMLOG_PIMG_CYC_MAX          4

/* log mask for master stack                                                    32-bit logmask:*/
#define ECATMLOG_PIMG_CYC(cycidx)      (1<<(ICLOG_OTHER_SHIFT+ (cycidx)))       /* .. .. .n .. */
#define ECATMLOG_EVT                   (1<<(ICLOG_OTHER_SHIFT+4))               /* .. .. 1. .. */
#define ECATMLOG_ESM                   (1<<(ICLOG_OTHER_SHIFT+5))               /* .. .. 2. .. */
#define ECATMLOG_INITC                 (1<<(ICLOG_OTHER_SHIFT+6))               /* .. .. 4. .. */
#define ECATMLOG_ASYNC                 (1<<(ICLOG_OTHER_SHIFT+7))               /* .. .. 8. .. */
#define ECATMLOG_COE                   (1<<(ICLOG_OTHER_SHIFT+8))               /* .. .1 .. .. */
#define ECATMLOG_COE_MORE              (1<<(ICLOG_OTHER_SHIFT+8))               /* .. .1 .. .. */
#define ECATMLOG_MBX                   (1<<(ICLOG_OTHER_SHIFT+9))               /* .. .2 .. .. */
#define ECATMLOG_SUP                   (1<<(ICLOG_OTHER_SHIFT+10))              /* .. .4 .. .. */
#define ECATMLOG_ACYC                  (1<<(ICLOG_OTHER_SHIFT+12))              /* .. 1. .. .. */
#define ECATMLOG_ECATFRM               (1<<(ICLOG_OTHER_SHIFT+13))              /* .. 2. .. .. */
#define ECATMLOG_MON                   (1<<(ICLOG_OTHER_SHIFT+14))              /* .. 4. .. .. */
#define ECATMLOG_DEBUG                 (1<<(ICLOG_OTHER_SHIFT+15))              /* .. 8. .. .. */
#define ECATMLOG_DC                    (1<<(ICLOG_OTHER_SHIFT+16))              /* .1 .. .. .. */

/*******************************************************************************
 * typedefs
 ******************************************************************************/

/* handle for EtherCAT master stack instance */
typedef struct __ecatm_hdldata *_ecatm_hdl;

// /* event function callback */
// typedef void (*_ecatm_evtfct) (_ecatm_hdl ecatm, _ecatm_evt *evt);

/* acyclic process function callback */
typedef void (*_ecatm_acycprcfct) (_ecatm_hdl ecatm, void *arg);

/* EtherCAT state (ECAT_ST_...) */
typedef uint32_t _ecat_state;

/* status flags for frame processing */
typedef uint32_t _ecatm_prcstatus;

#pragma pack (push,1)
/* context for cyclic task, used to access process image */
typedef struct __ecatm_cycctx
{
   _ecatm_hdl           ecatm;         /* master stack instance handle        */
   int                  cycidx;        /* cyclic task index                   */
   void                 *arg;          /* argument from cyclic configuration  */
   _ecatm_prcstatus     prcstatus;     /* status flags for frame processing   */
   _ecat_state          mststate;      /* master state (saved for this cycle) */
   _ecat_state          reqmststate;   /* requested master state of ESM       */
   uint16_t             wkcrcv;        /* received WKC (num active slaves)    */
                                       /* internally: ------------------------*/
   uint16_t             padding1;                                    
   struct __ecatm_cyc   *__cyc;        /* cyclic task handle                  */
   struct __ecatm_cyc_frminfo
                        *__frminfo;    /* cyc frame info array                */
#ifdef _LSL_TARGETARCH_ARM
   uint32_t             padding2;
#endif   
   uint64_t             nettime;       /* last SOF at the reference slave     */
   uint64_t             synctime;      /* optimal sync time                   */
   uint8_t              **sndfrm;      /* array of send frames                */
   uint8_t              **rcvfrm;      /* array of last valid receive frames  */
   int                  numframes;     /* number of frames used by this task  */
#ifdef _LSL_TARGETARCH_ARM   
   int                  padding3;
#endif
} _ecatm_cycctx;
#pragma pack (pop) 

/* process function callback */
typedef int (*_ecatm_prcfct) (_ecatm_cycctx *cycctx);

// /* (runtime) configuration for EtherCAT master stack */
//typedef struct __ecatm_cfg
//{
//   /* general */
//   uint8_t              flags;         /* ECATM_CFG_FLG_...                   */
//
//   int                  esmtmo_msec;   /* stack timeout for master ESM sync   */
//   int                  esmtransflags; /* default ESM transition flags        */
//
//   /* link layer configuration */
//   struct _ethll
//   {
//      const _ethll_funcs
//                        *funcs;        /* link layer driver functions         */
//      _ethll_hdl        hdl;           /* handle to link layer drv instance   */
//
//   }                    ethll[ECATM_MAX_NUM_ETHPORTS];
//
//   /* frame handling */
//   int                  frmtmo_usec;   /* timeout for receiving response frame*/
//   int                  numrxbuf;      /* number of frm buffers for receiving */
//
//   /* acyclic task */
//   _ecatm_evtfct        acycevtfct;    /* event handler for acyclic task      */
//
//   /* network supervision */
//   struct __ecatm_supcfg
//   {
//      int               poll_usec;     /* (minimal) slave supervision interval*/
//      uint8_t           maxrestate;    /* maximum number of restate retries   */
//                                       /* afterwards slaves is reseted to init*/
//                                       /* 0 => feature disabled               */
//      uint8_t           flags;         /* supervision config flags            */
//                                       /* ECATM_SUP_CONFIG_...                */
//   } sup;
//} _ecatm_cfg;

/* EtherCAT master information */
typedef struct __ecatm_info
{
   const char           *ver_ecatm;    /* version string of ecatm             */

   int                  numlldrv;      /* number of link layer drivers        */
} _ecatm_info;

// /* configuration of cyclic task */
//typedef struct __ecatm_cyccfg
//{
//   int                  cyctime_us;    /* cycle time in usec                  */
//   _ecatm_prcfct        prcfct;        /* process function callout            */
//   void                 *arg;          /* argument, passed to process callback*/
//   _ecatm_evtfct        evtfct;        /* event function callout              */
//
//                                       /* buffer for iovar access from other  */
//                                       /* task                                */
//   int                  iovarbufnum;   /* number of elements                  */
//   int                  iovarbufsize;  /* data buffer size                    */
//
//} _ecatm_cyccfg;

// /* value union of I/O variable */
//typedef union __ecatm_ioval
//{
//                                       /* contains value if data type is ...  */
//   uint8_t              uint8;         /*   BOOL, BYTE, BIT*, BITARR8, USINT  */
//   uint16_t             uint16;        /*   WORD, BITARR16, UINT,             */
//   uint32_t             uint32;        /*   DWORD, BITARR32, UINT24, UDINT    */
//   uint64_t             uint64;        /*   UINT40, UINT48, UINT56, ULINT     */
//   int8_t               int8;          /*   SINT                              */
//   int16_t              int16;         /*   INT *                             */
//   int32_t              int32;         /*   INT24, DINT                       */
//   int64_t              int64;         /*   INT40, INT48, INT56, LINT         */
//   float                real;          /*   REAL                              */
//   double               lreal;         /*   LREAL                             */
//
//   struct                              /*   GUID, STRING, ARR_*, UNKNOWN      */
//   {
//      uint8_t           *p;
//      int               size;
//   }                    mem;
//   struct                              /* (used internally for serializing)   */
//   {
//      uint32_t          uint32a;
//      uint32_t          uint32b;
//   }                    uint32ab;
//} _ecatm_ioval;

/*------------------------------------------------------------------------------
 * statistics
 *----------------------------------------------------------------------------*/

/* statistics for cyclic and acyclic task */
typedef struct __ecatm_task_stats
{
   unsigned int         numloops;      /* number of loops                     */

   unsigned int         cyctime_us;    /* cycle time set for the task         */

   struct                              /* error counter for ...               */
   {
      unsigned int      frm;           /* ECAT frm with bad content           */
      unsigned int      wkc;           /* unexpected working cnt              */
      unsigned int      olddata;       /* invalid or old process data         */
   }                    errcnt;
} _ecatm_task_stats;

/* statistics of link layer */
typedef struct __ecatm_ll_stats
{
   unsigned int         frmsnd;        /* number of sent ECAT frames          */
   unsigned int         frmrcv_ok;     /* number of received ECAT frames      */
   unsigned int         frmrcv_nonecat;/* number of received other frames     */
   unsigned int         frmrcv_tmo;    /* number of timeouts waiting for frm  */
   unsigned int         frmrcv_linkdown;  /* rcv timeout, link is down        */
   unsigned int         rcvbusyloops;  /* busy waiting loops for rcv frames   */
} _ecatm_ll_stats;

/*------------------------------------------------------------------------------
 * Network supervision
 *----------------------------------------------------------------------------*/

/* slave status structure */
typedef struct __ecatm_slv_status
{
   _ecat_state          curstate;      /* current EtherCAT state              */
   _ecat_state          expstate;      /* expected EtherCAT state             */
   uint16_t             alstatuscode;  /* AL_STATUS register value            */
   uint8_t              flags;         /* ECATM_SLV_STATUS_...                */
} _ecatm_slv_status;

/* network status information */
typedef struct __ecatm_net_status
{
   int                  num_slv_exp;   /* number of expected slave (with ESC) */
   int                  num_slv_curr;  /* number of slaves currently detected */
   uint16_t             cycbrdstatus;  /* Bit-OR of all ESC AL-Status regs    */
   uint8_t              flags;         /* ECATM_NET_STATUS_...                */
} _ecatm_net_status;

/*******************************************************************************
 * macro access functions for iovars
 ******************************************************************************/

/*
 * macros to access an I/O variable by access defines
 */

/* read access on input image */
#define ECATM_IVARDEF_RD1(cycctx__, def__, valp__)             ECATM_IVAR_RD1(    cycctx__, 0, def__, valp__)
#define ECATM_IVARDEF_RD8(cycctx__, def__, valp__)             ECATM_IVAR_RD8(    cycctx__, 0, def__, valp__)
#define ECATM_IVARDEF_RD16(cycctx__, def__, valp__)            ECATM_IVAR_RD16(   cycctx__, 0, def__, valp__)
#define ECATM_IVARDEF_RD32(cycctx__, def__, valp__)            ECATM_IVAR_RD32(   cycctx__, 0, def__, valp__)
#define ECATM_IVARDEF_RDNBIT(cycctx__, def__, valp__, nbit__)  ECATM_IVAR_RDNBIT( cycctx__, 0, def__, valp__, nbit__)
#define ECATM_IVARDEF_RDNBYTE(cycctx__, def__, valp__, size__) ECATM_IVAR_RDNBYTE(cycctx__, 0, def__, valp__, size__)

/* write access on output image */
#define ECATM_OVARDEF_WR1(cycctx__, def__, val__)              ECATM_OVAR_WR1(    cycctx__, 0, def__, val__)
#define ECATM_OVARDEF_WR8(cycctx__, def__, val__)              ECATM_OVAR_WR8(    cycctx__, 0, def__, val__)
#define ECATM_OVARDEF_WR16(cycctx__, def__, val__)             ECATM_OVAR_WR16(   cycctx__, 0, def__, val__)
#define ECATM_OVARDEF_WR32(cycctx__, def__, val__)             ECATM_OVAR_WR32(   cycctx__, 0, def__, val__)
#define ECATM_OVARDEF_WRNBIT(cycctx__, def__, valp__, nbit__)  ECATM_OVAR_WRNBIT( cycctx__, 0, def__, valp__, nbit__)
#define ECATM_OVARDEF_WRNBYTE(cycctx__, def__, valp__, size__) ECATM_OVAR_WRNBYTE(cycctx__, 0, def__, valp__, size__)

/* read access on output image */
#define ECATM_OVARDEF_RD1(cycctx__, def__, valp__)             ECATM_OVAR_RD1(    cycctx__, 0, def__, valp__)
#define ECATM_OVARDEF_RD8(cycctx__, def__, valp__)             ECATM_OVAR_RD8(    cycctx__, 0, def__, valp__)
#define ECATM_OVARDEF_RD16(cycctx__, def__, valp__)            ECATM_OVAR_RD16(   cycctx__, 0, def__, valp__)
#define ECATM_OVARDEF_RD32(cycctx__, def__, valp__)            ECATM_OVAR_RD32(   cycctx__, 0, def__, valp__)
#define ECATM_OVARDEF_RDNBIT(cycctx__, def__, valp__, nbit__)  ECATM_OVAR_RDNBIT( cycctx__, 0, def__, valp__, nbit__)
#define ECATM_OVARDEF_RDNBYTE(cycctx__, def__, valp__, size__) ECATM_OVAR_RDNBYTE(cycctx__, 0, def__, valp__, size__)

/*----------------------------------------------------------------------------*/

/*
 * macros to access an I/O variable by I/O var handle
 */

/* read access on input image */
#define ECATM_IVARHDL_RD1(cycctx__, iov__, valp__)             ECATM_IVAR_RD1(    cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__)
#define ECATM_IVARHDL_RD8(cycctx__, iov__, valp__)             ECATM_IVAR_RD8(    cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__)
#define ECATM_IVARHDL_RD16(cycctx__, iov__, valp__)            ECATM_IVAR_RD16(   cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__)
#define ECATM_IVARHDL_RD32(cycctx__, iov__, valp__)            ECATM_IVAR_RD32(   cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__)
#define ECATM_IVARHDL_RDNBIT(cycctx__, iov__, valp__,  nbit__) ECATM_IVAR_RDNBIT( cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__, nbit__)
#define ECATM_IVARHDL_RDNBYTE(cycctx__, iov__, valp__, size__) ECATM_IVAR_RDNBYTE(cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__, size__)

/* write access on output image */
#define ECATM_OVARHDL_WR1(cycctx__, iov__, val__)              ECATM_OVAR_WR1(    cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, val__)
#define ECATM_OVARHDL_WR8(cycctx__, iov__, val__)              ECATM_OVAR_WR8(    cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, val__)
#define ECATM_OVARHDL_WR16(cycctx__, iov__, val__)             ECATM_OVAR_WR16(   cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, val__)
#define ECATM_OVARHDL_WR32(cycctx__, iov__, val__)             ECATM_OVAR_WR32(   cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, val__)
#define ECATM_OVARHDL_WRNBIT(cycctx__, iov__, valp__,  nbit__) ECATM_OVAR_WRNBIT( cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__, nbit__)
#define ECATM_OVARHDL_WRNBYTE(cycctx__, iov__, valp__, size__) ECATM_OVAR_WRNBYTE(cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__, size__)

/* read access on output image */
#define ECATM_OVARHDL_RD1(cycctx__, iov__, valp__)             ECATM_OVAR_RD1(    cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__)
#define ECATM_OVARHDL_RD8(cycctx__, iov__, valp__)             ECATM_OVAR_RD8(    cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__)
#define ECATM_OVARHDL_RD16(cycctx__, iov__, valp__)            ECATM_OVAR_RD16(   cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__)
#define ECATM_OVARHDL_RD32(cycctx__, iov__, valp__)            ECATM_OVAR_RD32(   cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__)
#define ECATM_OVARHDL_RDNBIT(cycctx__, iov__, valp__,  nbit__) ECATM_OVAR_RDNBIT( cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__, nbit__)
#define ECATM_OVARHDL_RDNBYTE(cycctx__, iov__, valp__, size__) ECATM_OVAR_RDNBYTE(cycctx__, iov__, -1, (iov__)->cycidx, (iov__)->frmidx, (iov__)->frmoff, (iov__)->bitshift, valp__, size__)

// /*******************************************************************************
// * function prototypes
// ******************************************************************************/
//
// /* EtherCAT master functions */
//int ecatm_create              (_ecatm_hdl *ecatm, const _ecatm_cfg *cfg);
//int ecatm_destroy             (_ecatm_hdl ecatm);
//int ecatm_get_info            (_ecatm_hdl ecatm, _ecatm_info *info);
//int ecatm_net_get_status      (_ecatm_hdl ecatm, _ecatm_net_status *status);
//
// /*EtherCAT network information (ENI) */
//int ecatm_eni_link            (_ecatm_hdl ecatm, const _eceni_eccfg *cfg);
//int ecatm_eni_unlink          (_ecatm_hdl ecatm, const _eceni_eccfg **cfg);
//int ecatm_eni_lock            (_ecatm_hdl ecatm, int *chgcnt);
//int ecatm_eni_unlock          (_ecatm_hdl ecatm);
//int ecatm_eni_set_static      (_ecatm_hdl ecatm, const _eceni_eccfg *cfg);
//int ecatm_eni_get_cfg_info    (_ecatm_hdl ecatm, const _eceni_eccfg_info **info);
//int ecatm_eni_get_mst_info    (_ecatm_hdl ecatm, const _eceni_mst_info **info);
//int ecatm_eni_get_slv_info    (_ecatm_hdl ecatm, int slvidx, const _eceni_slv_info **info);
//int ecatm_eni_get_cyc_info    (_ecatm_hdl ecatm, int cycidx, const _eceni_cyc_info **info);
//int ecatm_eni_get_pimg_info   (_ecatm_hdl ecatm, const _eceni_pimg_info **info);
//int ecatm_eni_get_pimg_ivar   (_ecatm_hdl ecatm, int ivaridx, const _eceni_pimg_var **ivar);
//int ecatm_eni_get_pimg_ovar   (_ecatm_hdl ecatm, int ovaridx, const _eceni_pimg_var **ovar);
//
// /* EtherCAT master handling */
//int ecatm_mst_activate        (_ecatm_hdl ecatm, int activate);
//_ecat_state
//    ecatm_mst_get_state       (_ecatm_hdl ecatm);
//int ecatm_mst_req_state       (_ecatm_hdl ecatm, _ecat_state ecatstate, int transflg);
//
// /* EtherCAT slave handling */
//int  ecatm_slv_get_num        (_ecatm_hdl ecatm);
//_ecat_state
//     ecatm_slv_get_state      (_ecatm_hdl ecatm, int slvidx);
//int  ecatm_slv_req_state      (_ecatm_hdl ecatm, int slvidx, _ecat_state ecatstate);
//void ecatm_slv_get_status     (_ecatm_hdl ecatm, int slvidx, _ecatm_slv_status *status);
//int  ecatm_slv_ack_ind        (_ecatm_hdl ecatm, int slvidx);
//
// /* cyclic command handling */
//int ecatm_cyc_create          (_ecatm_hdl ecatm, int cycidx, const _ecatm_cyccfg *cyccfg);
//int ecatm_cyc_destroy         (_ecatm_hdl ecatm, int cycidx);
//int ecatm_cyc_task            (_ecatm_hdl ecatm, int cycidx);
//int ecatm_cyc_ctx_get_lock    (_ecatm_hdl ecatm, int cycidx, _ecatm_cycctx **cycctx);
//void ecatm_cyc_ctx_unlock     (_ecatm_hdl ecatm, _ecatm_cycctx *cycctx);
//
// /* acyclic command handling */
//int ecatm_acyc_attach         (_ecatm_hdl ecatm, _ecatm_acycprcfct acycprcfct, void *arg);
//int ecatm_acyc_task           (_ecatm_hdl ecatm, int *morework);
//
// /* general I/O variable functions */
//int ecatm_iovar_get_iovarlist (_ecatm_hdl hdl, const _eceni_iovar **iovar, int *iovarnum);
//int ecatm_iovar_check_iovar_crc
//                              (_ecatm_hdl hdl, uint32_t crc);
//int ecatm_iovar_getbyid       (_ecatm_hdl hdl, int iovarid, const _eceni_iovar **iovar);
//int ecatm_iovar_getbypdo      (_ecatm_hdl hdl, int slvidx, uint16_t index, uint8_t subindex, const _eceni_iovar **iovar);
//int ecatm_iovar_getbyname     (_ecatm_hdl hdl, const char *name, const _eceni_iovar **iovar);
//
///* functions to access an I/O variable by I/O var handle */
//int ecatm_iovar_in_rd1        (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint8_t  *data);
//int ecatm_iovar_in_rd8        (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint8_t  *data);
//int ecatm_iovar_in_rd16       (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint16_t *data);
//int ecatm_iovar_in_rd32       (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint32_t *data);
//int ecatm_iovar_in_rdnbit     (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint8_t  *data, int nbit);
//int ecatm_iovar_in_rdnbytes   (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint8_t  *buf,  int *size);
//
//int ecatm_iovar_out_wr1       (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint8_t  data);
//int ecatm_iovar_out_wr8       (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint8_t  data);
//int ecatm_iovar_out_wr16      (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint16_t data);
//int ecatm_iovar_out_wr32      (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint32_t data);
//int ecatm_iovar_out_wrnbit    (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint8_t  *data, int nbit);
//int ecatm_iovar_out_wrnbytes  (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint8_t  *buf,  int size);
//
//int ecatm_iovar_out_rd1       (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint8_t  *data);
//int ecatm_iovar_out_rd8       (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint8_t  *data);
//int ecatm_iovar_out_rd16      (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint16_t *data);
//int ecatm_iovar_out_rd32      (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint32_t *data);
//int ecatm_iovar_out_rdnbit    (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint8_t  *data, int nbit);
//int ecatm_iovar_out_rdnbytes  (_ecatm_cycctx *cycctx, const _eceni_iovar *iovar, uint8_t  *buf,  int *size);
//
//int ecatm_ioval_sizemax       (_ecatm_hdl ecatm, int cycidx, int *maxsize);
//int ecatm_ioval_rd            (_ecatm_cycctx *cycctx, int iovarid, _ecatm_ioval *ioval);
//int ecatm_ioval_wr            (_ecatm_cycctx *cycctx, int iovarid, _ecatm_ioval *ioval);
//int ecatm_ioval_wr_dq         (_ecatm_hdl ecatm     , int iovarid, _ecatm_ioval *ioval);
//
// /* statistics */
//int ecatm_task_stats          (_ecatm_hdl ecatm, int cycidx, _ecatm_task_stats *stats);
//int ecatm_ll_stats            (_ecatm_hdl ecatm, _ecatm_ll_stats *stats);
//
// /* access to Ethernet link layer driver */
//int ecatm_drvethll_info       (_ecatm_hdl ecatm, int ethllidx, _ethll_info *info);
//int ecatm_drvethll_status     (_ecatm_hdl ecatm, int ethllidx, _ethll_link_status *status);
//int ecatm_drvethll_stats      (_ecatm_hdl ecatm, int ethllidx, _ethll_nic_stats *stats);

/******************************************************************************/

#endif /* INCLUDE_ECATM_H_ */

/*[EOF]************************************************************************/
