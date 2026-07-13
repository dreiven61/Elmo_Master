#ifndef LSL_ECATM
#pragma once
#define LSL_ECATM

// Defines

// Interface
#define INTERFACE_ECATM                       "IECATM"

// Ergebniscodes
// ok, kein Fehler
#define ECATM_OK                            0

#define ECATM_INVALID_ECATM_HANDLE			-1
#define ECATM_INVALID_POINTER_FOR_CALLBACK	-2
#define ECATM_INVALID_ENI_FILE_PATH		    -3
#define ECATM_TIMING_ERROR_1		        -4
#define ECATM_INVALID_LINK_LAYER_CONF		-5
#define ECATM_TIMING_ERROR_2				-6
#define ECATM_ERROR_CONVERTING_FILE_PATH	-7 
#define ECATM_LINK_LAYER_LOST			    -8
#define ECATM_DATA_NOT_SENT			        -9
#define ECATM_ENI_FILE_CAN_NOT_BE_OPENED	-10
#define ECATM_ENI_BUFFER_MALLOC_FAILED		-11
#define ECATM_ENI_SEEK_ERROR			    -12
#define ECATM_ENI_NOT_CHECKSUM_FOUND		-13
#define ECATM_ENI_CHECKSUM_DOES_NOT_MATCH	-14
#define ECATM_GIRQ_ENABLE_FAILED 			-15 
#define ECATM_ERROR_CYCCTX_LOCK_ERROR 		-16  


#define LSL_ECATM_PRE_CYC_TASK    0
#define LSL_ECATM_POST_CYC_TASK   1

#define LSL_ECATM_ACYCLIC_TASK  1
#define LSL_ECATM_CYCLIC_TASK   2

// perform a re-init request: current state-> INIT -> requested state
#define LSL_ECATM_TRANS_REINIT             (1 shl 0)
// continue with master ESM transition although a slave ESM failed
#define LSL_ECATM_TRANS_CONT_ON_FAILURE    (1 shl 1)
// ignore invalid WKC during SAFEOP -> OP transition
#define LSL_ECATM_TRANS_IGNORE_WKC_SO      (1 shl 2) 



#define LSL_ECATM_TYPE_VERSION    0x00010004
// TODO: will be added later. Don't know all of them yet.

//////////////////////////////////////////////
// STRUCTS
//////////////////////////////////////////////
TYPE 
    #pragma pack (push, 1)
    LSL_ETHERCATMASTER : STRUCT
        version                 : UDINT;

        ECATM_SetEventCallback      : pVoid;
        ECATM_UpdateCyc             : pVoid;
        ECATM_ENI_Load              : pVoid;
        ECATM_iovar_getmastercount  : pVoid;
        ECATM_raise_error           : pVoid;
        ECATM_get_send_time_amount  : pVoid;

        // Master Core functions
        ECATM_mst_get_state  : pVoid;
        ECATM_mst_req_state  : pVoid;
        ECATM_slv_get_state  : pVoid;
        ECATM_slv_req_state  : pVoid;
        ECATM_slv_ack_ind    : pVoid;
        ECATM_slv_get_status : pVoid;

        // Mailbox functions
        ECATM_sdo_dld_start : pVoid;
        ECATM_sdo_dld_poll  : pVoid;
        ECATM_sdo_upl_start : pVoid;
        ECATM_sdo_upl_poll  : pVoid;
        ECATM_sdo_dld       : pVoid;
        ECATM_sdo_upl       : pVoid;

        // Asynchronous functions
        ECATM_async_start      : pVoid;
        ECATM_async_poll       : pVoid;
        ECATM_async_cmd        : pVoid;
        ECATM_async_esc_reg_rd : pVoid;
        ECATM_async_esc_reg_wr : pVoid;

        // Distributed Clocks
        ECATM_dc_set_config     : pVoid;
        ECATM_dc_get_config     : pVoid;
        ECATM_dc_get_time_delta : pVoid;
        ECATM_dc_get_last_sof   : pVoid;
        ECATM_dc_get_slv_info   : pVoid;
        ECATM_dc_get_stats      : pVoid;

        // Additional get functions
        ECATM_get_info              : pVoid;
        ECATM_net_get_status        : pVoid;
        ECATM_eni_get_cfg_info      : pVoid;  
        ECATM_eni_get_mst_info      : pVoid;
        ECATM_eni_get_slv_info      : pVoid;
        ECATM_eni_get_cyc_info      : pVoid;
        ECATM_eni_get_pimg_info     : pVoid;
        ECATM_eni_get_pimg_ivar     : pVoid; 
        ECATM_eni_get_pimg_ovar     : pVoid; 
        ECATM_slv_get_num           : pVoid;
        ECATM_iovar_getbyid         : pVoid;
        ECATM_iovar_getbyname       : pVoid;
        ECATM_iovar_getbypdo        : pVoid;

        // VERSION 2
        // Access link layer time stamping
        ECATM_ll_get_time     : pVoid;
        ECATM_cyc_get_ll_ts   : pVoid;
        ECATM_cyc_get_ll_rtt  : pVoid;
        // Support DC slave mode by application
        ECATM_dc_get_rtt      : pVoid;
        ECATM_dc_reftime_adj  : pVoid;
        ECATM_dc_set_offset   : pVoid;
        //GenIRQ Timestamping
        ECATM_GIRQ_Start_Timestamps : pVoid;
        ECATM_GIRQ_Read_Timestamps  : pVoid;
        // (de)activate Master
        ECATM_master_activate : pVoid;
        // FOE
        ECATM_foe_dld_start : pVoid;
        ECATM_foe_dld_poll  : pVoid;
        ECATM_foe_upl_start : pVoid;
        ECATM_foe_upl_poll  : pVoid;
        ECATM_foe_abort     : pVoid;
        
        // VERSION 3        
		// SoE SSC
        ECATM_ssc_read_start  : pVoid;
        ECATM_ssc_read_poll   : pVoid;
        ECATM_ssc_write_start : pVoid;
        ECATM_ssc_write_poll  : pVoid;
        ECATM_ssc_read        : pVoid;
        ECATM_ssc_write       : pVoid;
		// SII
        ECATM_sii_read_req      : pVoid;
        ECATM_sii_write_req     : pVoid;
        ECATM_sii_poll          : pVoid;
        ECATM_sii_set_buffer    : pVoid;
        ECATM_sii_abort         : pVoid;
		
        // VERSION 4
        // Get trailing data from eni
        ECATM_eni_get_trailing	: pVoid;

    END_STRUCT;
    #pragma pack (pop)
END_TYPE

//////////////////////////////////////////////
// FUNCTIONS
//////////////////////////////////////////////
FUNCTION __CDECL GLOBAL P_ECATM_SetEventCallback
    VAR_INPUT
        masterID : UDINT;
        pFunction : pVoid;
        pThis : pVoid;
        eventtyp : DINT;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_UpdateCyc
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        CycTaskID : UDINT;
        taskSelection : UDINT;
        cycctx : pVoid; (* Struct: _ecatm_cycctx *)
        udOffsetSendTime : UDINT;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_ENI_Load
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        ENI_File : ^char;
        masterID : UDINT;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_iovar_getmastercount
    VAR_INPUT
        pudMasterCount : ^UDINT; 
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;
    
FUNCTION __CDECL GLOBAL P_ECATM_raise_error
  VAR_INPUT
        event : UDINT;
        eventtMsg : ^char;
  END_VAR
  VAR_OUTPUT
        retcode : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_get_send_time_amount
  VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        CycTaskID : UDINT;
        time_needed_for_send : ^UDINT;
  END_VAR
  VAR_OUTPUT
        retcode : DINT;
  END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_mst_get_state
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        ecatstate : ^UDINT; 
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_mst_req_state
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        ecatstate : UDINT;
        transflg : UDINT;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_slv_get_state
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
        ecatstate : ^UDINT;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_slv_req_state
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
        ecatstate : UDINT;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_slv_ack_ind
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_slv_get_status
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
        status : pVoid; (* Struct: _ecatm_slv_status *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;    

FUNCTION __CDECL GLOBAL P_ECATM_sdo_dld_start
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
        index : UINT;       
        subindex : USINT;    
        complacc : DINT;
        data : ^USINT;    (* const *)   
        datasize : UDINT;
        tmo_ms : UDINT;
        sdohdlp : pVoid; (* Struct: _ecatm_sdohdl *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_sdo_dld_poll
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        sdohdl : pVoid; (* Struct: _ecatm_sdohdl *)
        abortcode : ^UDINT;  
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_sdo_upl_start
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
        index : UINT;       
        subindex : USINT;    
        complacc : DINT;
        data : ^USINT;       
        datamax : UDINT;
        tmo_ms : UDINT;
        sdohdlp : pVoid; (* Struct: _ecatm_sdohdl *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_sdo_upl_poll
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        sdohdlp : pVoid; (* Struct: _ecatm_sdohdl *)
        datasize : ^UDINT;
        abortcode : ^UDINT;   
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_sdo_dld
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
        index : UINT;       
        subindex : USINT;    
        complacc : DINT; 
        data : ^USINT; (* const *)      
        datasize : UDINT;
        tmo_ms : UDINT;
        abortcode : ^UDINT;   
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_sdo_upl
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
        index : UINT;       
        subindex : USINT;    
        complacc : DINT;
        data : ^USINT;       
        datamax : DINT;
        datasize : ^UDINT;
        tmo_ms : UDINT;
        abortcode : ^UDINT;   
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_async_start
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        cmd : USINT;     
        addr : UDINT;    
        datasndrcv : ^USINT;     
        size : UINT;    
        tmo_ms : UDINT;
        asynchdlp : pVoid; (* Struct: _ecatm_asynchdl *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_async_poll
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        asynchdl : pVoid; (* Struct: _ecatm_asynchdl *)
        wkc : ^UINT;    
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_async_cmd
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        cmd : USINT; 
        addr : UDINT; 
        datasndrcv : ^USINT;
        size : UINT;    
        tmo_ms : UDINT;
        wkc : ^UINT;    
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_async_esc_reg_rd
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
        regaddr : UINT; 
        val : ^UDINT;    
        size : DINT;    
        tmo_ms : UDINT;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_async_esc_reg_wr
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
        regaddr : UINT; 
        val : UDINT;     
        size : DINT;    
        tmo_ms : UDINT;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_dc_set_config
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        cfg : pVoid; (* const *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_dc_get_config
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        cfg : pVoid; (* Struct: _ecatm_dc_cfg *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_dc_get_time_delta
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        deltaTimeValue32 : ^DINT; (* Pointer zu 32bit Wert der die delta Time darstellt *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_dc_get_last_sof
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        timeValue64 : ^DINT; (* Pointer zu 64bit Wert der den Timestamp in nanosec darstellt *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_dc_get_slv_info
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
        info : pVoid; (* Struct: _ecatm_dc_slv_info *) 
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_dc_get_stats
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        stats : pVoid; (* Struct: _ecatm_dc_stats *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_get_info
    VAR_INPUT
        ecatm : pVoid;  (* Struct: _ecatm_hdl *)
        info : pVoid;   (* Struct: _ecatm_info *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_net_get_status
    VAR_INPUT
        ecatm : pVoid;  (* Struct: _ecatm_hdl *)
        status : pVoid;   (* Struct: _ecatm_net_status *)        
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_eni_get_cfg_info
    VAR_INPUT
        ecatm : pVoid;  (* Struct: _ecatm_hdl *)
        info : pVoid;   (* Struct: _eceni_eccfg_info, const *)    
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_eni_get_mst_info
    VAR_INPUT
        ecatm : pVoid;  (* Struct: _ecatm_hdl *)
        info : pVoid;   (* Struct: _eceni_mst_info, const *)    
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_eni_get_slv_info
    VAR_INPUT
        ecatm : pVoid;  (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
        info : pVoid;   (* Struct: _eceni_slv_info, const *)   
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_eni_get_cyc_info
    VAR_INPUT
        ecatm : pVoid;  (* Struct: _ecatm_hdl *)
        cycidx : DINT;
        info : pVoid;   (* Struct: _eceni_cyc_info, const *)   
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_eni_get_pimg_info
    VAR_INPUT
        ecatm : pVoid;  (* Struct: _ecatm_hdl *)
        info : pVoid;  (* Struct: _eceni_pimg_info, const *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_eni_get_pimg_ivar
    VAR_INPUT
        ecatm : pVoid;  (* Struct: _ecatm_hdl *)
        ivaridx : DINT;
        ivar : pVoid;  (* Struct: _eceni_pimg_var, const *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_eni_get_pimg_ovar
    VAR_INPUT
        ecatm : pVoid;  (* Struct: _ecatm_hdl *)
        ivaridx : DINT;
        ovar : pVoid;  (* Struct: _eceni_pimg_var, const *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_slv_get_num
    VAR_INPUT
        ecatm : pVoid;  (* Struct: _ecatm_hdl *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_iovar_getbyid
    VAR_INPUT
        ecatm : pVoid;  (* Struct: _ecatm_hdl *)
        iovarid : DINT;
        iovar : pVoid; (* Struct: _eceni_iovar, const *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_iovar_getbyname
    VAR_INPUT
        ecatm : pVoid;  (* Struct: _ecatm_hdl *)
        name : ^char; (* const *)
        iovar : pVoid; (* Struct: _eceni_iovar, const *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_iovar_getbypdo
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        slvidx : UDINT;
        index : UINT;
        subindex : USINT;
        iovar : pVoid; (* Struct: _eceni_iovar, const *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;
    
FUNCTION __CDECL GLOBAL P_ECATM_ll_get_time
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        time_ns : ^UDINT; (* Pointer zu 32bit Wert *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;


FUNCTION __CDECL GLOBAL P_ECATM_cyc_get_ll_ts
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        cycctx : pVoid; (* Struct: _ecatm_cycctx *)
        sndts_ns : ^UDINT; (* Pointer zu 32bit Wert *)
        rcvts_ns : ^UDINT; (* Pointer zu 32bit Wert *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_cyc_get_ll_rtt
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        cycctx : pVoid; (* Struct: _ecatm_cycctx *)
        rtt_ns : ^UDINT; (* Pointer zu 32bit Wert *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_dc_get_rtt
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        rtt_ns : ^UDINT; (* Pointer zu 32bit Wert *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_dc_reftime_adj
    VAR_INPUT
        ecatm   : pVoid; (* Struct: _ecatm_hdl *)
        cycctx  : pVoid; (* Struct: _ecatm_cycctx *)
        time_ns : UDINT;(*32bit*)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;
    
FUNCTION __CDECL GLOBAL P_ECATM_dc_set_offset
    VAR_INPUT
        ecatm : pVoid; (* Struct: _ecatm_hdl *)
        offset : ^UDINT; (* uint64_t *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;
    
FUNCTION __CDECL GLOBAL P_ECATM_GIRQ_Start_Timestamps    
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_GIRQ_Read_Timestamps
    VAR_INPUT        
        GIRQ_tsAct_ns  : ^UDINT; (* Pointer zu 32bit Wert *)
        GIRQ_tsRT_ns   : ^UDINT; (* Pointer zu 32bit Wert *)
        GIRQ_tsTxSt_ns : ^UDINT; (* Pointer zu 32bit Wert *)
        cycctx         : pVoid;  (* Struct: _ecatm_cycctx *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_master_activate
    VAR_INPUT        
        ecatm    : pVoid; (* Struct: _ecatm_hdl *)
        activate : BOOL;
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;


FUNCTION __CDECL GLOBAL P_ECATM_foe_dld_start
    VAR_INPUT        
        ecatm     : pVoid; (* Struct: _ecatm_hdl *)
        slvidx    : UDINT;
        file      : ^char; (* const *)
        password  : UDINT;
        tmo_ms    : UDINT;
        blocksize : ^UDINT;
        trfhdlp   : pVoid; (* Struct: pointer to _ecatm_foe_trfhdl *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;
    
FUNCTION __CDECL GLOBAL P_ECATM_foe_dld_poll
    VAR_INPUT        
        ecatm   : pVoid; (* Struct: _ecatm_hdl *)
        trfhdlp : pVoid; (* Struct: _ecatm_foe_trfhdl *)
        data    : ^USINT;
        size    : UDINT;
        status  : pVoid; (* Struct: pointer to _ecatm_foe_status *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_foe_upl_start
    VAR_INPUT        
        ecatm     : pVoid; (* Struct: _ecatm_hdl *)
        slvidx    : UDINT;
        file      : ^char; (* const *)
        password  : UDINT;
        tmo_ms    : UDINT;
        blocksize : ^UDINT;
        trfhdlp   : pVoid; (* Struct: pointer to _ecatm_foe_trfhdl *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_foe_upl_poll
    VAR_INPUT        
        ecatm   : pVoid; (* Struct: _ecatm_hdl *)
        trfhdlp : pVoid; (* Struct: _ecatm_foe_trfhdl *)
        data    : ^USINT;
        status  : pVoid; (* Struct: pointer to _ecatm_foe_status *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_foe_abort
    VAR_INPUT        
        ecatm   : pVoid; (* Struct: _ecatm_hdl *)
        trfhdlp : pVoid; (* Struct: _ecatm_foe_trfhdl *)
    END_VAR
    VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_ssc_read_start
    VAR_INPUT        
        ecatm           : pVoid; (* Struct: _ecatm_hdl *)
        slvidx          : UDINT;
        driveno         : DINT;
        elementflags    : USINT;
        idn             : DINT;
        data            : ^USINT;    (* const *)
        datamax         : DINT;
        tmo_ms          : UDINT;
        sschdlp         : pVoid; (* Struct: pointer to _ecatm_sschdl *)
    END_VAR
    VAR_OUTPUT
        retcode         : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_ssc_read_poll
    VAR_INPUT        
        ecatm       : pVoid; (* Struct: _ecatm_hdl *)
        sschdlp     : pVoid; (* Struct: _ecatm_sschdl *)
        datasize    : ^UDINT;
        errorcode   : ^UINT; 
    END_VAR
    VAR_OUTPUT
        retcode     : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_ssc_write_start
    VAR_INPUT        
        ecatm           : pVoid; (* Struct: _ecatm_hdl *)
        slvidx          : UDINT;
        driveno         : DINT;
        elementflags    : USINT;
        idn             : DINT;
        data            : ^USINT;    (* const *)
        datasize        : UDINT;
        tmo_ms          : UDINT;
        sschdlp         : pVoid; (* Struct: pointer to _ecatm_sschdl *)
    END_VAR
    VAR_OUTPUT
        retcode         : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_ssc_write_poll
    VAR_INPUT        
        ecatm       : pVoid; (* Struct: _ecatm_hdl *)
        sschdlp     : pVoid; (* Struct: _ecatm_sschdl *)
        errorcode   : ^UINT; 
    END_VAR
    VAR_OUTPUT
        retcode     : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_ssc_read
    VAR_INPUT        
        ecatm           : pVoid; (* Struct: _ecatm_hdl *)
        slvidx          : UDINT;
        driveno         : DINT;
        elementflags    : USINT;
        idn             : DINT;
        data            : ^USINT;    (* const *)
        datamax         : DINT;
        datasize        : ^UDINT;
        tmo_ms          : UDINT;
        errorcode       : ^UINT; 
    END_VAR
    VAR_OUTPUT
        retcode         : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_ssc_write
    VAR_INPUT        
        ecatm           : pVoid; (* Struct: _ecatm_hdl *)
        slvidx          : UDINT;
        driveno         : DINT;
        elementflags    : USINT;
        idn             : DINT;
        data            : ^USINT;    (* const *)
        datasize        : UDINT;
        tmo_ms          : UDINT;
        errorcode       : ^UINT; 
    END_VAR
    VAR_OUTPUT
        retcode         : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_sii_read_req
    VAR_INPUT
        ecatm       : pVoid; (* Struct: _ecatm_hdl*)
        slvidx      : UDINT;
        startword   : UDINT;
        numwords	: UDINT;
		sii       	: pVoid; (* Struct: pointer to _ecatm_sii_hdl*)
    END_VAR
        VAR_OUTPUT
        retcode : DINT;
    END_VAR;
	
FUNCTION __CDECL GLOBAL P_ECATM_sii_write_req
    VAR_INPUT
        ecatm       : pVoid; (* Struct: _ecatm_hdl*)
        slvidx      : UDINT;
        startword   : UDINT;
        numwords	: UDINT;
		sii       	: pVoid; (* Struct: pointer to _ecatm_sii_hdl*)
    END_VAR
        VAR_OUTPUT
        retcode : DINT;
    END_VAR;
	
FUNCTION __CDECL GLOBAL P_ECATM_sii_poll
    VAR_INPUT
        ecatm       : pVoid; (*Struct: _ecatm_hdl*)
        sii       	: pVoid; (*Struct: _ecatm_sii_hdl*)
		status  	: pVoid; (* Struct: pointer to _ecatm_sii_status *)
    END_VAR
        VAR_OUTPUT
        retcode : DINT;
    END_VAR;

FUNCTION __CDECL GLOBAL P_ECATM_sii_set_buffer
    VAR_INPUT
        ecatm       : pVoid; (*Struct: _ecatm_hdl*)
        sii       	: pVoid; (*Struct: _ecatm_sii_hdl*)
        size   		: UDINT;
        buffer 		: ^USINT; (* const *)
    END_VAR
        VAR_OUTPUT
        retcode : DINT;
    END_VAR;
	
FUNCTION __CDECL GLOBAL P_ECATM_sii_abort
    VAR_INPUT
        ecatm       : pVoid; (*Struct: _ecatm_hdl*)
        sii       	: pVoid; (*Struct: _ecatm_sii_hdl*)
    END_VAR;
	
FUNCTION __CDECL GLOBAL P_ECATM_eni_get_trailing
    VAR_INPUT
        ecatm       : pVoid; (*Struct: _ecatm_hdl*)
        trailing   	: ^pChar;
    END_VAR
        VAR_OUTPUT
        retcode : DINT;
    END_VAR;

// Since Version 0x00010001
#define OS_ECATM_CHECK_VERSION(pECATM)                      pECATM^.version                     <> LSL_ECATM_TYPE_VERSION
#define OS_ECATM_SetEventCallback(pECATM,p1,p2,p3,p4)       pECATM^.ECATM_SetEventCallback      $ P_ECATM_SetEventCallback(p1,p2,p3,p4)
#define OS_ECATM_UpdateCyc(pECATM,p1,p2,p3,p4,p5)           pECATM^.ECATM_UpdateCyc             $ P_ECATM_UpdateCyc(p1,p2,p3,p4,p5)
#define OS_ECATM_ENI_Load(pECATM,p1,p2,p3)                  pECATM^.ECATM_ENI_Load              $ P_ECATM_ENI_Load(p1,p2,p3)
#define OS_ECATM_get_master_count(pECATM,p1)                pECATM^.ECATM_iovar_getmastercount  $ P_ECATM_iovar_getmastercount(p1)
#define OS_ECATM_raise_error(pECATM,p1,p2)                  pECATM^.ECATM_raise_error           $ P_ECATM_raise_error(p1,p2)
#define OS_ECATM_get_send_time_amount(pECATM,p1,p2,p3)      pECATM^.ECATM_get_send_time_amount  $ P_ECATM_get_send_time_amount(p1,p2,p3)
#define OS_ECATM_mst_get_state(pECATM,p1,p2)                pECATM^.ECATM_mst_get_state         $ P_ECATM_mst_get_state(p1,p2)
#define OS_ECATM_mst_req_state(pECATM,p1,p2,p3)             pECATM^.ECATM_mst_req_state         $ P_ECATM_mst_req_state(p1,p2,p3)
#define OS_ECATM_slv_get_state(pECATM,p1,p2,p3)             pECATM^.ECATM_slv_get_state         $ P_ECATM_slv_get_state(p1,p2,p3)
#define OS_ECATM_slv_req_state(pECATM,p1,p2,p3)             pECATM^.ECATM_slv_req_state         $ P_ECATM_slv_req_state(p1,p2,p3)
#define OS_ECATM_slv_ack_ind(pECATM,p1,p2)                  pECATM^.ECATM_slv_ack_ind           $ P_ECATM_slv_ack_ind(p1,p2)
#define OS_ECATM_slv_get_status(pECATM,p1,p2,p3)            pECATM^.ECATM_slv_get_status        $ P_ECATM_slv_get_status(p1,p2,p3)
#define OS_ECATM_sdo_dld_start(pECATM,p1,p2,p3,p4,p5,p6,p7,p8,p9)   pECATM^.ECATM_sdo_dld_start $ P_ECATM_sdo_dld_start(p1,p2,p3,p4,p5,p6,p7,p8,p9)
#define OS_ECATM_sdo_dld_poll(pECATM,p1,p2,p3)              pECATM^.ECATM_sdo_dld_poll          $ P_ECATM_sdo_dld_poll(p1,p2,p3)
#define OS_ECATM_sdo_upl_start(pECATM,p1,p2,p3,p4,p5,p6,p7,p8,p9)   pECATM^.ECATM_sdo_upl_start $ P_ECATM_sdo_upl_start(p1,p2,p3,p4,p5,p6,p7,p8,p9)
#define OS_ECATM_sdo_upl_poll(pECATM,p1,p2,p3,p4)           pECATM^.ECATM_sdo_upl_poll          $ P_ECATM_sdo_upl_poll(p1,p2,p3,p4)
#define OS_ECATM_sdo_dld(pECATM,p1,p2,p3,p4,p5,p6,p7,p8,p9) pECATM^.ECATM_sdo_dld               $ P_ECATM_sdo_dld(p1,p2,p3,p4,p5,p6,p7,p8,p9)
#define OS_ECATM_sdo_upl(pECATM,p1,p2,p3,p4,p5,p6,p7,p8,p9,p10)     pECATM^.ECATM_sdo_upl       $ P_ECATM_sdo_upl(p1,p2,p3,p4,p5,p6,p7,p8,p9,p10)
#define OS_ECATM_async_start(pECATM,p1,p2,p3,p4,p5,p6,p7)   pECATM^.ECATM_async_start           $ P_ECATM_async_start(p1,p2,p3,p4,p5,p6,p7)
#define OS_ECATM_async_poll(pECATM,p1,p2,p3)                pECATM^.ECATM_async_poll            $ P_ECATM_async_poll(p1,p2,p3)
#define OS_ECATM_async_cmd(pECATM,p1,p2,p3,p4,p5,p6,p7)     pECATM^.ECATM_async_cmd             $ P_ECATM_async_cmd(p1,p2,p3,p4,p5,p6,p7)
#define OS_ECATM_async_esc_reg_rd(pECATM,p1,p2,p3,p4,p5,p6) pECATM^.ECATM_async_esc_reg_rd      $ P_ECATM_async_esc_reg_rd(p1,p2,p3,p4,p5,p6)
#define OS_ECATM_async_esc_reg_wr(pECATM,p1,p2,p3,p4,p5,p6) pECATM^.ECATM_async_esc_reg_wr      $ P_ECATM_async_esc_reg_wr(p1,p2,p3,p4,p5,p6)
#define OS_ECATM_dc_set_config(pECATM,p1,p2)                pECATM^.ECATM_dc_set_config         $ P_ECATM_dc_set_config(p1,p2)
#define OS_ECATM_dc_get_config(pECATM,p1,p2)                pECATM^.ECATM_dc_get_config         $ P_ECATM_dc_get_config(p1,p2)
#define OS_ECATM_dc_get_time_delta(pECATM,p1,p2)            pECATM^.ECATM_dc_get_time_delta     $ P_ECATM_dc_get_time_delta(p1,p2)
#define OS_ECATM_dc_get_last_sof(pECATM,p1,p2)              pECATM^.ECATM_dc_get_last_sof       $ P_ECATM_dc_get_last_sof(p1,p2)
#define OS_ECATM_dc_get_slv_info(pECATM,p1,p2,p3)           pECATM^.ECATM_dc_get_slv_info       $ P_ECATM_dc_get_slv_info(p1,p2,p3)
#define OS_ECATM_dc_get_stats(pECATM,p1,p2)                 pECATM^.ECATM_dc_get_stats          $ P_ECATM_dc_get_stats(p1,p2)
#define OS_ECATM_get_info(pECATM,p1,p2)                     pECATM^.ECATM_get_info              $ P_ECATM_get_info(p1,p2)
#define OS_ECATM_net_get_status(pECATM,p1,p2)               pECATM^.ECATM_net_get_status        $ P_ECATM_net_get_status(p1,p2)
#define OS_ECATM_eni_get_cfg_info(pECATM,p1,p2)             pECATM^.ECATM_eni_get_cfg_info      $ P_ECATM_eni_get_cfg_info(p1,p2)
#define OS_ECATM_eni_get_mst_info(pECATM,p1,p2)             pECATM^.ECATM_eni_get_mst_info      $ P_ECATM_eni_get_mst_info(p1,p2)
#define OS_ECATM_eni_get_slv_info(pECATM,p1,p2,p3)          pECATM^.ECATM_eni_get_slv_info      $ P_ECATM_eni_get_slv_info(p1,p2,p3)
#define OS_ECATM_eni_get_cyc_info(pECATM,p1,p2,p3)          pECATM^.ECATM_eni_get_cyc_info      $ P_ECATM_eni_get_cyc_info(p1,p2,p3)
#define OS_ECATM_eni_get_pimg_info(pECATM,p1,p2)            pECATM^.ECATM_eni_get_pimg_info     $ P_ECATM_eni_get_pimg_info(p1,p2)
#define OS_ECATM_eni_get_pimg_ivar(pECATM,p1,p2,p3)         pECATM^.ECATM_eni_get_pimg_ivar     $ P_ECATM_eni_get_pimg_ivar(p1,p2,p3)
#define OS_ECATM_eni_get_pimg_ovar(pECATM,p1,p2,p3)         pECATM^.ECATM_eni_get_pimg_ovar     $ P_ECATM_eni_get_pimg_ovar(p1,p2,p3)
#define OS_ECATM_slv_get_num(pECATM,p1)                     pECATM^.ECATM_slv_get_num           $ P_ECATM_slv_get_num(p1)
#define OS_ECATM_iovar_getbyid(pECATM,p1,p2,p3)             pECATM^.ECATM_iovar_getbyid         $ P_ECATM_iovar_getbyid(p1,p2,p3)
#define OS_ECATM_iovar_getbyname(pECATM,p1,p2,p3)           pECATM^.ECATM_iovar_getbyname       $ P_ECATM_iovar_getbyname(p1,p2,p3)
#define OS_ECATM_iovar_getbypdo(pECATM,p1,p2,p3,p4,p5)      pECATM^.ECATM_iovar_getbypdo        $ P_ECATM_iovar_getbypdo(p1,p2,p3,p4,p5)
// Since Version 0x00010002
#define OS_ECATM_ll_get_time(pECATM,p1,p2)                  pECATM^.ECATM_ll_get_time           $ P_ECATM_ll_get_time(p1,p2)
#define OS_ECATM_cyc_get_ll_ts(pECATM,p1,p2,p3,p4)          pECATM^.ECATM_cyc_get_ll_ts         $ P_ECATM_cyc_get_ll_ts(p1,p2,p3,p4)
#define OS_ECATM_cyc_get_ll_rtt(pECATM,p1,p2,p3)            pECATM^.ECATM_cyc_get_ll_rtt        $ P_ECATM_cyc_get_ll_rtt(p1,p2,p3)
#define OS_ECATM_dc_get_rtt(pECATM,p1,p2)                   pECATM^.ECATM_dc_get_rtt            $ P_ECATM_dc_get_rtt(p1,p2)
#define OS_ECATM_dc_reftime_adj(pECATM,p1,p2,p3)            pECATM^.ECATM_dc_reftime_adj        $ P_ECATM_dc_reftime_adj(p1,p2,p3)
#define OS_ECATM_dc_set_offset(pECATM,p1,p2)                pECATM^.ECATM_dc_set_offset         $ P_ECATM_dc_set_offset(p1,p2)
#define OS_ECATM_GIRQ_Start_Timestamps(pECATM)              pECATM^.ECATM_GIRQ_Start_Timestamps $ P_ECATM_GIRQ_Start_Timestamps()
#define OS_ECATM_GIRQ_Read_Timestamps(pECATM,p1,p2,p3,p4)   pECATM^.ECATM_GIRQ_Read_Timestamps  $ P_ECATM_GIRQ_Read_Timestamps(p1,p2,p3,p4)
#define OS_ECATM_master_activate(pECATM,p1,p2)              pECATM^.ECATM_master_activate       $ P_ECATM_master_activate(p1,p2)
#define OS_ECATM_foe_dld_start(pECATM,p1,p2,p3,p4,p5,p6,p7) pECATM^.ECATM_foe_dld_start         $ P_ECATM_foe_dld_start(p1,p2,p3,p4,p5,p6,p7)
#define OS_ECATM_foe_dld_poll(pECATM,p1,p2,p3,p4,p5)        pECATM^.ECATM_foe_dld_poll          $ P_ECATM_foe_dld_poll(p1,p2,p3,p4,p5)
#define OS_ECATM_foe_upl_start(pECATM,p1,p2,p3,p4,p5,p6,p7) pECATM^.ECATM_foe_upl_start         $ P_ECATM_foe_upl_start(p1,p2,p3,p4,p5,p6,p7)
#define OS_ECATM_foe_upl_poll(pECATM,p1,p2,p3,p4)           pECATM^.ECATM_foe_upl_poll          $ P_ECATM_foe_upl_poll(p1,p2,p3,p4)
#define OS_ECATM_foe_abort(pECATM,p1,p2)                    pECATM^.ECATM_foe_abort             $ P_ECATM_foe_abort(p1,p2)
// Since Version 0x00010003
#define OS_ecatm_ssc_read_start(pECATM, p1, p2, p3, p4, p5, p6, p7, p8, p9)     pECATM^.ECATM_ssc_read_start  $ P_ECATM_ssc_read_start(p1, p2, p3, p4, p5, p6, p7, p8, p9)
#define OS_ecatm_ssc_read_poll(pECATM, p1, p2, p3, p4)                          pECATM^.ECATM_ssc_read_poll   $ P_ECATM_ssc_read_poll(p1, p2, p3, p4)
#define OS_ecatm_ssc_write_start(pECATM, p1, p2, p3, p4, p5, p6, p7, p8, p9)    pECATM^.ECATM_ssc_write_start $ P_ECATM_ssc_write_start(p1, p2, p3, p4, p5, p6, p7, p8, p9)
#define OS_ecatm_ssc_write_poll(pECATM, p1, p2, p3)                             pECATM^.ECATM_ssc_write_poll  $ P_ECATM_ssc_write_poll(p1, p2, p3)
#define OS_ecatm_ssc_read(pECATM, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10)      pECATM^.ECATM_ssc_read        $ P_ECATM_ssc_read(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10)
#define OS_ecatm_ssc_write(pECATM, p1, p2, p3, p4, p5, p6, p7, p8, p9)          pECATM^.ECATM_ssc_write       $ P_ECATM_ssc_write(p1, p2, p3, p4, p5, p6, p7, p8, p9)
#define OS_ECATM_sii_read_req(pECATM,p1,p2,p3,p4,p5)        pECATM^.ECATM_sii_read_req          $ P_ECATM_sii_read_req(p1,p2,p3,p4,p5)
#define OS_ECATM_sii_write_req(pECATM,p1,p2,p3,p4,p5)       pECATM^.ECATM_sii_write_req         $ P_ECATM_sii_write_req(p1,p2,p3,p4,p5)
#define OS_ECATM_sii_poll(pECATM,p1,p2,p3)                  pECATM^.ECATM_sii_poll             	$ P_ECATM_sii_poll(p1,p2,p3)
#define OS_ECATM_sii_set_buffer(pECATM,p1,p2,p3,p4)			pECATM^.ECATM_sii_set_buffer        $ P_ECATM_sii_set_buffer(p1,p2,p3,p4)
#define OS_ECATM_sii_abort(pECATM,p1,p2)                    pECATM^.ECATM_sii_abort             $ P_ECATM_sii_abort(p1,p2)
// Since Version 0x00010004
#define OS_ECATM_eni_get_trailing(pECATM,p1,p2)				pECATM^.ECATM_eni_get_trailing      $ P_ECATM_eni_get_trailing(p1,p2)

#endif
































