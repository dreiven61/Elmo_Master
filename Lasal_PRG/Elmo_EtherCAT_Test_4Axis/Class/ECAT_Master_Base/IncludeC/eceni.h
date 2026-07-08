/*[CR]**************************************************************************
   Copyright (c) by IBV - Echtzeit- und Embedded GmbH & Co. KG
   All Rights reserved.
*/

/*[FH]**************************************************************************

   PROJECT:    icNET
   MODULE:     icECAT
   FILE:       eceni.h
   CONTENTS:   EtherCAT - ENI definitions
*/

/*[CL]**************************************************************************

   DD-MM-YYYY  NNN   <change log>
   ---
   NNN: IBV, www.ibv-augsburg.net
*/

/*[MP]**************************************************************************

   References:

   [ETG2100]      ETG, "ETG2100_V1i0i0_S_R_ENI.pdf", 20.05.2009
   [ETG2100-1]    ETG, "ETG2100_V1i0i1_S_R_ENI.pdf", 25.09.2015

 */

#ifndef INCLUDE_ECENI_H_
#define INCLUDE_ECENI_H_

/*******************************************************************************
 * local includes
 ******************************************************************************/

/* c runtime */
#include <stdint.h>                    /* data type definitions */

/*******************************************************************************
 * defines
 ******************************************************************************/

/* module version */
#define ECENI_VERSION_MAJ        1
#define ECENI_VERSION_MIN        0
#define ECENI_VERSION_REV        0

/* stringification of version string */
#define ECENI_XSTR(s)            ECENI_STR(s)
#define ECENI_STR(s)             #s
#define ECENI_VERSION_STR        ECENI_XSTR(ECENI_VERSION_MAJ) "." \
                                 ECENI_XSTR(ECENI_VERSION_MIN) "." \
                                 ECENI_XSTR(ECENI_VERSION_REV)

/* Ethernet definitions */
#define ECENI_MAC_SIZE           6
#define ECENI_ETHERTYPE_SIZE     2

/* EtherCAT TransitionType, OR'ing of values is possible */
#define ECENI_TRANS_IP           (1<<0)
#define ECENI_TRANS_PS           (1<<1)
#define ECENI_TRANS_PI           (1<<2)
#define ECENI_TRANS_SP           (1<<3)
#define ECENI_TRANS_SO           (1<<4)
#define ECENI_TRANS_SI           (1<<5)
#define ECENI_TRANS_OS           (1<<6)
#define ECENI_TRANS_OP           (1<<7)
#define ECENI_TRANS_OI           (1<<8)
#define ECENI_TRANS_IB           (1<<9)
#define ECENI_TRANS_BI           (1<<10)
#define ECENI_TRANS_II           (1<<11)
#define ECENI_TRANS_PP           (1<<12)
#define ECENI_TRANS_SS           (1<<13)
#define ECENI_TRANS_OO           (1<<14)

/* EtherCat Slave Mailbox Protocols, OR' of values is possible */
#define ECENI_SLV_MBX_PROT_AOE   (1<<0)
#define ECENI_SLV_MBX_PROT_EOE   (1<<1)
#define ECENI_SLV_MBX_PROT_COE   (1<<2)
#define ECENI_SLV_MBX_PROT_SOE   (1<<3)
#define ECENI_SLV_MBX_PROT_FOE   (1<<4)
#define ECENI_SLV_MBX_PROT_VOE   (1<<5)

/* EtherCAT NMTOKEN requires */
#define ECENI_REQ_FRAME          1
#define ECENI_REQ_CYCLE          2

/* number of Sync Managers */
#define ECENI_NUM_SM             4

/* EtherCAT NMTOKEN Sync Managers Settings Types */
#define ECENI_SMS_TYPE_OUT       1
#define ECENI_SMS_TYPE_IN        2

/* EtherCAT NMTOKEN COE Command Type */
#define ECENI_COE_CSS_UPLOAD     1
#define ECENI_COE_CSS_DOWNLOAD   2

/* EtherCAT NMTOKEN Previous Port */
#define ECENI_PREVPORT_A         0
#define ECENI_PREVPORT_B         1
#define ECENI_PREVPORT_C         2
#define ECENI_PREVPORT_D         3

/* I/O variable type */
#define ECENI_IOVAR_INPUT        0x01
#define ECENI_IOVAR_OUTPUT       0x02

/* default values */
#define ECENI_CNT_UNDEFINED      (-1)
#define ECENI_SLVIDX_UNDEFINED   (-2)


// EtherCAT DataType (Base Data Types), according to ETG1020, Table 92
#define ECAT_DT_UNKNOWN          0x0000
#define ECAT_DT_BOOL             0x0001
#define ECAT_DT_BYTE             0x001E
#define ECAT_DT_WORD             0x001F
#define ECAT_DT_DWORD            0x0020
#define ECAT_DT_BIT1             0x0030
#define ECAT_DT_BIT2             0x0031
#define ECAT_DT_BIT3             0x0032
#define ECAT_DT_BIT4             0x0033
#define ECAT_DT_BIT5             0x0034
#define ECAT_DT_BIT6             0x0035
#define ECAT_DT_BIT7             0x0036
#define ECAT_DT_BIT8             0x0037
#define ECAT_DT_BITARR8          0x002D
#define ECAT_DT_BITARR16         0x002E
#define ECAT_DT_BITARR32         0x002F
#define ECAT_DT_SINT             0x0002
#define ECAT_DT_INT              0x0003
#define ECAT_DT_INT24            0x0010
#define ECAT_DT_DINT             0x0004
#define ECAT_DT_INT40            0x0012
#define ECAT_DT_INT48            0x0013
#define ECAT_DT_INT56            0x0014
#define ECAT_DT_LINT             0x0015
#define ECAT_DT_USINT            0x0005
#define ECAT_DT_UINT             0x0006
#define ECAT_DT_UINT24           0x0016
#define ECAT_DT_UDINT            0x0007
#define ECAT_DT_UINT40           0x0018
#define ECAT_DT_UINT48           0x0019
#define ECAT_DT_UINT56           0x001A
#define ECAT_DT_ULINT            0x001B
#define ECAT_DT_REAL             0x0008
#define ECAT_DT_LREAL            0x0011
#define ECAT_DT_GUID             0x001D
#define ECAT_DT_STRING           0x0009
#define ECAT_DT_ARY_BYTE         0x000A
#define ECAT_DT_ARY_UINT         0x000B
#define ECAT_DT_ARY_INT          0x0260
#define ECAT_DT_ARY_SINT         0x0261
#define ECAT_DT_ARY_DINT         0x0262
#define ECAT_DT_ARY_UDINT        0x0263

// EtherCAT DataType INT Scale value according to ETG2100, Table 41 and ETG2000, Table 68  range of the submitted data
#define ECAT_INTDSCALE_PM_10    1     /*   -10 to  +10 */
#define ECAT_INTDSCALE_PM_20    2     /*   -20 to  +20 */
#define ECAT_INTDSCALE_0TO10    3     /*     0 to   10 */
#define ECAT_INTDSCALE_0TO20    4     /*     0 to   20 */
#define ECAT_INTDSCALE_4TO20    5     /*     4 to   20 */
#define ECAT_INTDSCALE_DIV10    6     /* multiply value with 0.1 */
#define ECAT_INTDSCALE_DIV100   7     /* multiply value with 0.01 */
#define ECAT_INTDSCALE_0TO5     8     /*     0 to    5 */
#define ECAT_INTDSCALE_0TO15    9     /*     0 to   15 */
#define ECAT_INTDSCALE_0TO30    10    /*     0 to   30 */
#define ECAT_INTDSCALE_0TO50    11    /*     0 to   50 */
#define ECAT_INTDSCALE_PM_5     12    /*    -5 to   +5 */
#define ECAT_INTDSCALE_PM_2_5   13    /*  -2.5 to +2.5 */
#define ECAT_INTDSCALE_PM_100   14    /*  -100 to +100 */
#define ECAT_INTDSCALE_PM_75MV  15    /* -75mV to +75mV */
#define ECAT_INTDSCALE_OTHER    16    /* all other values */

/*******************************************************************************
 * macros
 ******************************************************************************/

/* combination of ADP and ADO to get 32bit physical address value */
#define ECENI_ADDR_PHYS(adp, ado)   (((adp) << 16) | (ado))

#define ECENI_ADDR_PHYS_ADP(addr)   ((addr >> 16) & 0xFFFF)
#define ECENI_ADDR_PHYS_ADO(addr)   (addr & 0xFFFF)

/* logical addressing */
#define ECENI_ADDR_LOG(addr)        (addr)

/*******************************************************************************
 * typedefs
 ******************************************************************************/

/*------------------------------------------------------------------------------
 * Common EtherCAT data structures
 *----------------------------------------------------------------------------*/

/* data array in protocol data unit (PDU) */
typedef struct __ecat_dataary
{
   const uint8_t        *p;            /* pointer to data buffer              */
   uint16_t             len;           /* number of data bytes                */
                                       /* if p is 0 and len is > 0 then a     */
                                       /* data array of the length 'len'      */
                                       /* filled with 0x00 is configured      */
} _eceni_dataary;

/* EtherCAT protocol data unit (PDU) */
typedef struct __ecat_pdu
{
   uint8_t              cmd;           /* ECAT_CMD_...                        */
   uint32_t             addr;          /* logical or ECAT_ADDR_PHYS(ADP,ADO)  */
   _eceni_dataary       data;          /* data + data length                  */
   uint16_t             wkc;           /* working counter                     */
} _eceni_pdu;

/* ENI: Validate */
typedef struct __eceni_cmd_validate
{
   _eceni_dataary       data;
   _eceni_dataary       datamask;      /* DataMask [0...1]                    */
   int                  timeout;
} _eceni_cmd_validate;

/* ENI: ECatCmdType */
typedef struct __eceni_cmd_type
{
   const struct __eceni_cmd_type *next;
   uint32_t             transmask;     /* ECENI_TRANS_... [0..n]              */
                                       /* values can OR'ed                    */
   char                 beforeslv;     /* BeforeSlave [0...1]                 */
                                       /* 0 if not specified                  */
   const char           *comment;      /* Comment [0...1]                     */
                                       /* "-" if not specified                */
   char                 requires;      /* ECENI_REQ_... [0...1]               */
                                       /* 0 if not specified                  */
   _eceni_pdu           pdu;           /* protocol data unit sub structure    */
   int                  retries;       /* retries [0...1]                     */
                                       /* 0 if not specified                  */
   int                  wkc;           /* expected working count in answer    */
                                       /* ECENI_CNT_UNDEFINED if not set      */

   const _eceni_cmd_validate
                        *validate;     /* validation information [0...1]      */
} _eceni_cmd_type;


/*------------------------------------------------------------------------------
 * Master
 *----------------------------------------------------------------------------*/

/* ENI: EtherCATConfig : Config : Master : Info */
typedef struct __eceni_mst_info
{
   const char           *name;
   uint8_t              dstmacaddr[ECENI_MAC_SIZE];
   uint8_t              srcmacaddr[ECENI_MAC_SIZE];
   uint8_t              ethertype [ECENI_ETHERTYPE_SIZE];
                                       /* EtherType in little-endian format   */
                                       /* default {0xa4, 0x88} for EtherCAT   */
} _eceni_mst_info;

/* ENI: EtherCATConfig : Config : Master : MailboxStates */
typedef struct __eceni_mst_mbx
{
   uint32_t             startaddr;
   int                  cnt;
} _eceni_mst_mbx;

/* ENI: EtherCATConfig : Config : Master : EoE */
typedef struct __eceni_mst_eoe
{
   int                  maxports;
   int                  maxframes;
   int                  maxmacs;
} _eceni_mst_eoe;

/* ENI: EtherCATConfig : Config : Master */
typedef struct __eceni_mst
{
   _eceni_mst_info         info;
   const _eceni_mst_mbx    *mbx;       /* MailboxStates [0...1]               */
   const _eceni_mst_eoe    *eoe;       /* Ethernet over EtherCat [0...1]      */
   const _eceni_cmd_type   *initcmd;   /* InitCmds apply all slaves [0...1]   */
} _eceni_mst;


/*------------------------------------------------------------------------------
 * Slave
 *----------------------------------------------------------------------------*/

/* ENI: EtherCATConfig : Config : Slave : Info */
typedef struct __eceni_slv_info
{
   const char           *name;
   uint16_t             physaddr;         /* 0 if not specified               */
   int16_t              autoincraddr;
   uint8_t              physics[4+1];     /* ECAT_ESC_PHYSICS__...            */
   uint32_t             vendorid;
   uint32_t             productcode;
   uint32_t             revisionno;
   uint32_t             serialno;
   const char           *productrev;      /* xs:int in [ETG2100] [0...1]      */
} _eceni_slv_info;

/* ENI: EtherCATConfig : Config : Slave : ProcessData : Send / Receive */
typedef struct __eceni_prcdata
{
   int                  bitstart;
   int                  bitlen;
} _eceni_prcdata;

/* ENI: EntryType */
typedef struct __eceni_entry
{
   const struct __eceni_entry *next;
   uint16_t             index;      /* index of this entry within the PDO     */
   uint16_t             subidx;     /* SubIndex [0...1]                       */
                                    /* 0 if not specified                     */
   int                  bitlen;
   const char           *name;      /* name [0...n]                           */
   const char           *comment;   /* comment [0...1]                        */
                                    /* "-" if not specified                   */
   int                  datatype;   /* DataType [0...1]                       */
                                    /* ECAT_DT_UNKNOWN if not specified       */
   uint8_t              dscale;     /* ECAT_INTDSCALE_... for INT datatypes   */
} _eceni_entry;

/* ENI: PDO number list */
typedef struct _eceni_pdonums
{
   const struct _eceni_pdonums *next;
   uint16_t             pdo;
} _eceni_pdonums;

// /* ENI: PdoType */
//typedef struct __eceni_pdo
//{
//   const struct __eceni_pdo *next;
//   uint16_t             index;
//   const char           *name;         /* name [1...n]                        */
//   char                 fixed;         /* 0 if not specified                  */
//   char                 mandatory;     /* 0 if not specified                  */
//   char                 virtual;       /* 0 if not specified                  */
//   int                  sm;            /* -1 if not specified                 */
//   int                  su;            /* 0 if not specified                  */
//   int                  pdoorder;      /* reserved for future                 */
//   int                  osfac;         /* 0 if not specified                  */
//   int                  osmin;         /* 0 if not specified                  */
//   int                  osmax;         /* 0 if not specified                  */
//   int                  osindexinc;    /* 0 if not specified                  */
//   const _eceni_pdonums *excludelist;  /* Exclude [0...n]                     */
//   const _eceni_entry   *entry;        /* Entry [0...n]                       */
//} _eceni_pdo;

// /* ENI: SyncManagerSettings */
//typedef struct __eceni_sm_set
//{
//   int                  type;
//   int                  minsize;       /* MinSize [0...1]                     */
//                                       /* 0 if not specified                  */
//   int                  maxsize;       /* MaxSize [0...1]                     */
//                                       /* 0 if not specified                  */
//   int                  defsize;       /* DefaultSize [0...1]                 */
//                                       /* 0 if not specified                  */
//   uint32_t             startaddr;
//   uint32_t             ctrlbyte;
//   char                 enable;
//   char                 virtual;       /* Virtual [0...1]                     */
//                                       /* 0 if not specified                  */
//   const _eceni_pdonums *pdonumlist;   /* pdo [0...n]                         */
//} _eceni_sm_set;

// /* ENI: EtherCATConfig : Config : Slave : ProcessData */
//typedef struct __eceni_slv_prcdata
//{
//   const _eceni_prcdata *snd;                /* send [0...1]                  */
//   const _eceni_prcdata *rcv;                /* receive [0...1]               */
//   const _eceni_sm_set  *sm[ECENI_NUM_SM];   /* sync manager sm[n] [0...1]    */
//   const _eceni_pdo     *rxpdo;
//   const _eceni_pdo     *txpdo;
//} _eceni_slv_prcdata;

/* ENI: EtherCATConfig : Config : Slave : Mailbox : Send : MailboxSendInfoType*/
typedef struct __eceni_slv_mbx_sinfo
{
   int                  start;
   int                  len;
} _eceni_slv_mbx_sinfo;

/* ENI: EtherCATConfig : Config : Slave : Mailbox : Send : MailboxRecvInfoType*/
typedef struct __eceni_slv_mbx_rinfo
{
   int                  start;
   int                  len;
   int                  polltime;         /* in milliseconds, 0 for not used  */
   int                  statusbitaddr;    /* -1 if not specified              */
} _eceni_slv_mbx_rinfo;

/* ENI: EtherCATConfig : Config : Slave : MailboxCmdType */
typedef struct __eceni_mbx_cmd_type
{
   const struct __eceni_mbx_cmd_type *next;
   uint32_t             transmask;     /* ECENI_TRANS_... value can be OR'ed  */
   const char           *comment;
   int                  timeout;
   _eceni_dataary       data;
   char                 disable;       /* 0 if not specified                  */
} _eceni_mbx_cmd_type;

/* ENI: EtherCATConfig : Config : Slave : Mailbox : CoE : InitCmds : InitCmd */
typedef struct __eceni_slv_coe_cmds
{
   const struct __eceni_slv_coe_cmds *next;
   uint32_t             transmask;     /* ECENI_TRANS_... value can be OR'ed  */
   const char           *comment;      /* "-" if not specified                */
   int                  timeout;
   int                  ccs;           /* ECENI_COE_CSS_...                   */
   int                  idx;
   int                  subidx;
   _eceni_dataary       data;
   char                 disable;       /* 0 if not specified                  */
   char                 fixed;         /* 0 if not specified                  */
   char                 complete;      /* 0 if not specified                  */
} _eceni_slv_coe_cmds;

/* ENI: EtherCATConfig : Config : Slave : Mailbox : CoE : Profile : ProfileType : ChannelInfo */
typedef struct __eceni_slv_coe_prf_chninfo
{
   /* linked list channel infos, 0 at end */
   const struct __eceni_slv_coe_prf_chninfo *next;
   int                  prfno;
   const char           *addinfo;      /* "-" if not specified                */
   const char           *name;         /* "-" if not specified                */
   char                 overwrite;     /* 0 if not specified                  */
} _coe_prf_chninfo;

/* ENI: EtherCATConfig : Config : Slave : Mailbox : CoE : Profile : ProfileType */
typedef struct __eceni_slv_coe_prf
{
   const _coe_prf_chninfo  *chninfo;
} _eceni_slv_coe_prf;

/* ENI: EtherCATConfig : Config : Slave : Mailbox : CoE */
typedef struct __eceni_slv_coe
{
   const _eceni_slv_coe_cmds  *initcmds;  /* initcmds [0...1]                 */
   const _eceni_slv_coe_prf   *prftyp;    /* profile typ [0...1]              */
} _eceni_slv_coe;

/* ENI: EtherCATConfig : Config : Slave : Mailbox : EoE */
typedef struct __eceni_slv_eoe
{
   const _eceni_mbx_cmd_type  *initcmds;  /* initcmds [0...1]                 */
} _eceni_slv_eoe;

/* ENI: EtherCATConfig : Config : Slave : Mailbox : FoE */
typedef struct __eceni_slv_foe
{
   const _eceni_mbx_cmd_type  *initcmds;  /* initcmds [0...1]                 */
} _eceni_slv_foe;

/* ENI: EtherCATConfig : Config : Slave : Mailbox : SoE, AoE, VoE (ignored) */

/* ENI: EtherCATConfig : Config : Slave : Mailbox */
typedef struct __eceni_slv_mbx
{
   char                 dllcmdrepeat;  /* 0 if not specified                  */
   _eceni_slv_mbx_sinfo snd;
   _eceni_slv_mbx_rinfo rcv;
   uint32_t             protmask;      /* supported device protocols          */
                                       /* ECENI_SLV_MBX_PROT_...              */
   const _eceni_slv_coe *coe;          /* CANopen over EtherCAT   [0...1]     */
   const _eceni_slv_eoe *eoe;          /* Ethernet over EtherCAT  [0...1]     */
   const _eceni_slv_foe *foe;          /* File Acces over EtherCat[0...1]     */
} _eceni_slv_mbx;

/* ENI: EtherCATConfig : Config : Slave : PreviousPort */
typedef struct __eceni_slv_prevport
{
   const struct __eceni_slv_prevport *next;
   char                 selected;   /* 0 if not specified                     */
   int                  deviceid;   /* reserved [0...1]                       */
   char                 port;       /* ECENI_PREVPORT_...                     */
   unsigned int         physaddr;   /* station address previous device [0...1]*/
                                    /* 0 if not specified                     */
} _eceni_slv_prevport;

/* ENI: EtherCATConfig : Config : Slave : HotConnect */
typedef struct __eceni_slv_hcon
{
   int                     grpmbrcnt;  /* number of slaves within HC group    */
   const _eceni_cmd_type   *idfycmds;  /* identify command [1...n]            */
} _eceni_slv_hcon;

/* ENI: EtherCATConfig : Config : Slave : DC */
typedef struct __eceni_slv_dclk
{
   char                 refclk;        /* device is/is not reference clock    */
   int                  cyctim0;       /* cycle time of Sync0 events in ns    */
   int                  cyctim1;       /* Cycle time Sync1 - Cycle Time Sync0 */
                                       /* + shift time Sync0 => cyctim1       */
   int                  timshft;       /* shift time of Sync0 events in ns    */
   char                 potrefclk;     /* potential reference clock           */
} _eceni_slv_dclk;

// /* ENI: EtherCATConfig : Config : Slave (0...n) */
//typedef struct __eceni_slv
//{
//   const struct __eceni_slv   *next;      /* linked list of slaves, 0 at end  */
//   _eceni_slv_info            info;
//   const _eceni_slv_prcdata   *prcdata;   /* process data [0...1]             */
//   const _eceni_slv_mbx       *mbx;       /* mailbox [0...1]                  */
//   const _eceni_cmd_type      *initcmd;   /* init commands [0...n]            */
//   const _eceni_slv_prevport  *prevport;  /* Previous Ports [0...n]           */
//   const _eceni_slv_hcon      *hcon;      /* HotConnect [0...1]               */
//   const _eceni_slv_dclk      *dclk;      /* distributed clocks [0...1]       */
//} _eceni_slv;


/*------------------------------------------------------------------------------
 * Cyclic
 *----------------------------------------------------------------------------*/

/* ENI: EtherCATConfig : Config : Cyclic : Info */
typedef struct __eceni_cyc_info
{
   const char           *comment;      /* comment  [0...1]                    */
                                       /* "-" if not specified                */
   int                  cyctime;       /* in usec  [0...1]                    */
                                       /* 0 if not specified                  */
   int                  priority;      /* priority [0...1]                    */
                                       /* 0 if not specified, else 1 ... 62   */
   const char           *taskid;       /* TaskId   [0...1]                    */
   int                  numfrm;        /* numFrame [0...n]                    */
} _eceni_cyc_info;

/* ENI: EtherCATConfig : Config : Cyclic : Frame : Cmd : CopyInfos */
typedef struct __eceni_cyc_cpyinfo
{
   const struct __eceni_cyc_cpyinfo *next;
   int                  srcbitoffs;
   int                  dstbitoffs;
   int                  binlen;
} _eceni_cyc_cpyinfo;

/* ENI: EtherCATConfig : Config : Cyclic : Frame : Cmd (1...n) */
typedef struct __eceni_cyc_cmd
{
   const struct __eceni_cyc_cmd *next;
   uint8_t              statemask;  /* ECAT_STATE_... value can be OR'ed[1..4]*/
   uint8_t              iovarmask;  /* ECENI_IOVAR_...value can be OR'ed      */
                                    /* bits are set if cmd contains an        */
                                    /* iovar                                  */
   const char           *comment;   /* comment [0...1]                        */
                                    /* "-" if not specified                   */
   _eceni_pdu           pdu;        /* protocol data unit sub structure       */
   int                  cnt;        /* expected WKC [0..1]                    */
                                    /* ECENI_CNT_UNDEFINED if not specified   */
   int                  inoffs;     /* input offsets                          */
                                    /* 0 if not specified                     */
   int                  outoffs;    /* output offsets                         */
                                    /* 0 if not specified                     */
   const _eceni_cyc_cpyinfo   *cpyinfo;   /* copy info [0...1]                */
} _eceni_cyc_cmd;

/* ENI: EtherCATConfig : Config : Cyclic : Frame (1...n) */
typedef struct __eceni_cyc_frame
{
   const struct __eceni_cyc_frame *next;
   const char           *comment;      /* Comment [0...1]                     */
                                       /* "-" if not specified                */
   const _eceni_cyc_cmd *cmd;          /* Cmd [1...n]                         */
} _eceni_cyc_frm;

/* ENI: EtherCATConfig : Config : Cyclic (0...n) */
typedef struct __eceni_cyc
{
   const struct __eceni_cyc   *next;   /* linked list of cyc frames, 0 at end */
   _eceni_cyc_info            info;
   const _eceni_cyc_frm       *frm;    /* cyclic frames [1...n]               */
} _eceni_cyc;


/*------------------------------------------------------------------------------
 * ProcessImage
 *----------------------------------------------------------------------------*/

/* ENI: EtherCATConfig : Config : ProcessImage : Inputs / Outputs */
typedef struct __eceni_pimg_var
{
   const char           *name;
   int                  datatype;      /* ECAT_DT_...                         */
                                       /* ECAT_DT_UNKNOWN if not specified    */
   int                  bitsize;
   int                  bitoffs;
} _eceni_pimg_var;

typedef struct __eceni_pimg_var_list
{
   const struct __eceni_pimg_var_list *next;
   _eceni_pimg_var      var;           /* variable [0...n]                    */
} _eceni_pimg_var_list;

/* ENI: EtherCATConfig : Config : ProcessImage */
typedef struct __eceni_pimg_info
{
   struct
   {
      int               bytesize;
      int               numvars;       /* number of input / output variables  */
   }                    in,
                        out;
} _eceni_pimg_info;

/* ENI: EtherCATConfig : Config : ProcessImage */
typedef struct __eceni_pimg
{
   _eceni_pimg_info           info;
   const _eceni_pimg_var_list *in;     /* input  [0...1]                      */
   const _eceni_pimg_var_list *out;    /* output [0...1]                      */
} _eceni_pimg;

/* access information to I/O variable in cyclic frame */
typedef struct __eceni_iovar
{
   uint8_t               dir;          /* direction: ECENI_IOVAR_INPUT/OUTPUT */
   int                   bitsize;      /* size of I/O variable in bits        */
   uint8_t               cycidx;       /* index of cyclic task                */
   uint8_t               frmidx;       /* index of frame in cyclic task       */
   uint16_t              frmoff;       /* byte offset in cyclic frame         */
   uint8_t               bitshift;     /* start bit in byte                   */
   const _eceni_pimg_var *var;         /* ptr to ENI config for the I/O var   */
   int                   slvidx;       /* slave index                         */
   const _eceni_entry    *pdoentry;    /* PDO entry (first if multiple PDOs   */
                                       /* entries are mapped to this I/O var) */
} _eceni_iovar;


/*
 * cycidx=0...
 * - frmidx=0...
 *
 *   | ETH Header | | ECAT Frame                                        |
 *   |            | | FrmHd | |PDU1 Hd|Data|WKC| |PDU2 Hd|Data|WKC| ... |
 *   | 6+6+2=14   | | 2     | |10     |n1  |2  | |10     |n2  |2  | ... |
 *                  ^         ^       ^
 *                  ^         ^       ^    example:
 *   <-ENI-InputOffs of PDU--->       ^    16  = (14+2)
 *   <-ENI-BitOffs of Input/Output---->    208 = (14+2+10)*8
 *                  ^                 ^
 *                  ^                 ^
 *                  <-frmoff---------->    208/8 - 16 + 2 = 12
 *   => offset of I/O var in cyclic frame:   (ENI-BitOffs/8
 *                                          - ENI-InputOffs or ENI-OutputOffs
 *                                          + FrmHd size)
 */


/*------------------------------------------------------------------------------
 * EtherCATConfig
 *----------------------------------------------------------------------------*/

/* ENI: EtherCATConfig (only dimensions) */
typedef struct __eceni_eccfg_info
{
   const char           *enifilename;
   const char           *verxml;       /* <EtherCATConfig Version> attribute  */
   int                  numslv;        /* number of configured slaves         */
   int                  numcyc;        /* number of configured cyclic tasks   */
   int                  numpimg;       /* number of process images (0 or 1)   */
} _eceni_eccfg_info;

/* ENI struct version */
typedef struct __eceni_ver
{
   unsigned char        maj;           /* major version                       */
   unsigned char        min;           /* minor version                       */
   unsigned char        rev;           /* revision                            */

} _eceni_ver;

/* _eceni_cyc_cpyinfo mapping table in ecatm process image */
typedef struct __eceni_ecatm_cpyinfo
{
                                       /* offset refers to start of ECAT frm  */
   int   srcbitoffset;                 /* bit offset in source frame          */
   int   dstbitoffset;                 /* bit offset in destination frame     */
   int   bitsize;                      /* number of bits that shall be copyed */

   int   cmdidx;                       /* index of cmd PDU WKC to evaluate    */
   int   srcfrmidx;                    /* source frame index of the cyc task  */
   int   cyctaskidx;                   /* cyclic task index owning the frames */
   int   dstfrmidx;                    /* dst frame index of the cyc task     */
} _eceni_ecatm_cpyinfo;


typedef struct __eceni_ecatm
{
   const _eceni_iovar      *iovar;     /* io access information structure     */
   int                     iovarnum;   /* number of io var structures         */
   uint32_t                iovarcrc;   /* CRC covering iovar definitions      */
   const _eceni_ecatm_cpyinfo *cpyinfos;  /* array of ecatm copy infos        */
                                       /* sort (up): taskidx,srcfrmidx,cmdidx */
   int                     numcpyinfo; /* length of the cpyinfos array        */
} _eceni_ecatm;

// /* ENI: EtherCATConfig (root of ENI configuration tree) */
//typedef struct __eceni_eccfg
//{
//   const _eceni_ver        ver;        /* version of the ENI config tree      */
//   const _eceni_eccfg_info info;       /* common information about ENI tree   */
//   const _eceni_mst        *mst;       /* Master [1]                          */
//   const _eceni_slv        *slv;       /* Slave  [0...n]                      */
//   const _eceni_cyc        *cyc;       /* Cyclic [0...n]                      */
//   const _eceni_pimg       *pimg;      /* Process Image [0...1]               */
//   _eceni_ecatm            ecatm;      /* Processed ENI information for access*/
//} _eceni_eccfg;

/******************************************************************************/

#endif /* INCLUDE_ECENI_H_ */

/*[EOF]************************************************************************/
