#ifndef _CPlcOnlineH
  #define _CPlcOnlineH

  #include "DefineCompiler.h"

  #ifdef cCompile
    // login at plc. function will return valid pointer (handle) on success, on the other hand NULL
    // interface  1 ... intern
    // interface 10 ... tcpip, 0x15740A0A .. 10.10.116.22
    cExtern void*         sigclib_plc_Login(unsigned long interface, unsigned long address);
    // logout at plc
    cExtern void          sigclib_plc_Logout(void *hdl);
    // TxCommand
    cExtern unsigned long sigclib_plc_TxCommand(void *hdl, void**presult, unsigned long command, const unsigned char *pdata, unsigned long datalength);
    // get lasalid of given server
    cExtern unsigned long sigclib_plc_GetLasalIdSvr(void *hdl, const char *label);
    // get multi lasalid of given object+server1+server2...
    cExtern unsigned long sigclib_plc_GetLasalIdSvrEx(void *hdl, unsigned long *pid, unsigned char *pdata, unsigned long datasize, unsigned long no);
    // get lasalid of given global variable
    cExtern unsigned long sigclib_plc_GetLasalIdVar(void *hdl, const char *label);
    // call read-method of given server by using lasalid
    cExtern unsigned long sigclib_plc_CallReadMethod(void *hdl, unsigned long *pdst, unsigned long lasalid);
    // call write-method of given server by using lasalid
    cExtern unsigned long sigclib_plc_CallWriteMethod(void *hdl, unsigned long lasalid, unsigned long value);
    // call newinst-method of given server by using lasalid
    cExtern void*         sigclib_plc_CallNewInstMethod(void *hdl, unsigned long lasalid, unsigned short uicmd, void *para, unsigned long parano);
    
    // get descriptor-crc from plc. function will return 1 in case of success, otherwise 0
    cExtern unsigned long sigclib_plc_GetDscCrc(void *hdl, unsigned long *pcrc);
    // keep connection to plc alive. function will return 1 in case of success, otherwise 0
    cExtern unsigned long sigclib_plc_KeepAlive(void *hdl);
    // function will return loaderversion from plc, in case of error function will return 0
    cExtern unsigned long sigclib_plc_GetLoaderVersion(void *hdl);
    
    // add server to refreshlist. function will return 1 in case of success, otherwise 0
    cExtern unsigned long sigclib_plc_ReflistAddSvr(void *hdl, unsigned long lasalid, unsigned long userid, unsigned long time_ms, unsigned long channel);
    // add variable to refreshlist. function will return 1 in case of success, otherwise 0
    cExtern unsigned long sigclib_plc_ReflistAddVar(void *hdl, unsigned long lasalid, unsigned long userid, unsigned long time_ms, unsigned long channel);
    // add string-server to refreshlist. function will return 1 in case of success, otherwise 0
    cExtern unsigned long sigclib_plc_ReflistAddStr(void *hdl, unsigned long lasalid, unsigned long userid, unsigned long time_ms, unsigned long channel);
    // start refreshlist at plc. function will return 1 in case of success, otherwise 0
    cExtern unsigned long sigclib_plc_ReflistStart(void *hdl, unsigned long channel);
    // stop refreshlist at plc. function will return 1 in case of success, otherwise 0
    cExtern unsigned long sigclib_plc_ReflistStop(void *hdl, unsigned long channel);
    // install callback at refreshlist. callback has to be of prototype extern "C" void CallBack(unsigned long userid, unsigned long value);
    cExtern void          sigclib_plc_ReflistInstallCallback(void *callback);
    
    // get string from plc. function will return 1 in case of success, otherwise 0
    cExtern unsigned long sigclib_plc_GetString(void *hdl, void *dst, unsigned long *pchrsize, unsigned long lasalid, unsigned long maxbytesize);
    // set string at plc. function will return 1 in case of success, otherwise 0
    cExtern unsigned long sigclib_plc_SetString(void *hdl, const void *src, unsigned long chrsize, unsigned long lasalid);
    
    // get data from address in plc. function will return 1 in case of success, otherwise 0
    cExtern unsigned long sigclib_plc_GetDataAt(void *hdl, void *dst, unsigned long address, unsigned long bytelength);
    // set data at address in plc. function will return 1 in case of success, otherwise 0
    cExtern unsigned long sigclib_plc_SetDataAt(void *hdl, void *src, unsigned long address, unsigned long bytelength);
    
  #else
    // login at plc. function will return valid pointer (handle) on success, on the other hand NULL
    function global __cdecl sigclib_plc_Login var_input interface:udint; address:udint; end_var var_output retcode:^void; end_var;
    // logout at plc
    function global __cdecl sigclib_plc_Logout var_input hdl:^void; end_var;
    // TxCommand
    function global __cdecl sigclib_plc_TxCommand var_input hdl:^void; presult:^pvoid; command:udint; pdata:^usint; datalength:udint; end_var var_output retcode:udint; end_var;
    // get lasalid of given server
    function global __cdecl sigclib_plc_GetLasalIdSvr var_input hdl:^void; label:^char; end_var var_output retcode:udint; end_var;
    // get multi lasalid of given object+server1+server2...
    function global __cdecl sigclib_plc_GetLasalIdSvrEx var_input hdl:^void; pid:^udint; pdata:^usint; datasize:udint; no:udint; end_var var_output retcode:udint; end_var;
    // get lasalid of given global variable
    function global __cdecl sigclib_plc_GetLasalIdVar var_input hdl:^void; label:^char; end_var var_output retcode:udint; end_var;
    // call read-method of given server by using lasalid
    function global __cdecl sigclib_plc_CallReadMethod var_input hdl:^void; pdst:^udint; lasalid:udint; end_var var_output retcode:udint; end_var;
    // call write-method of given server by using lasalid
    function global __cdecl sigclib_plc_CallWriteMethod var_input hdl:^void; lasalid:udint; value:udint; end_var var_output retcode:udint; end_var;
    // call newinst-method of given server by using lasalid
    function global __cdecl sigclib_plc_CallNewInstMethod var_input hdl:^void; lasalid:udint; uicmd:uint; para:^void; parano:udint; end_var var_output retcode:^void; end_var;
    
    
    // get descriptor-crc from plc. function will return 1 in case of success, otherwise 0
    function global __cdecl sigclib_plc_GetDscCrc var_input hdl:^void; pcrc:^udint; end_var var_output retcode:udint; end_var;
    // keep connection to plc alive. function will return 1 in case of success, otherwise 0
    function global __cdecl sigclib_plc_KeepAlive var_input hdl:^void; end_var var_output retcode:udint; end_var;
    // function will return loaderversion from plc, in case of error function will return 0
    function global __cdecl sigclib_plc_GetLoaderVersion var_input hdl:^void; end_var var_output retcode:udint; end_var;
    
    // add server to refreshlist. function will return 1 in case of success, otherwise 0
    function global __cdecl sigclib_plc_ReflistAddSvr var_input hdl:^void; lasalid:udint; userid:udint; time_ms:udint; channel:udint; end_var var_output retcode:udint; end_var;
    // add variable to refreshlist. function will return 1 in case of success, otherwise 0
    function global __cdecl sigclib_plc_ReflistAddVar var_input hdl:^void; lasalid:udint; userid:udint; time_ms:udint; channel:udint; end_var var_output retcode:udint; end_var;
    // add string-server to refreshlist. function will return 1 in case of success, otherwise 0
    function global __cdecl sigclib_plc_ReflistAddStr var_input hdl:^void; lasalid:udint; userid:udint; time_ms:udint; channel:udint; end_var var_output retcode:udint; end_var;
    // start refreshlist at plc. function will return 1 in case of success, otherwise 0
    function global __cdecl sigclib_plc_ReflistStart var_input hdl:^void; channel:udint; end_var var_output retcode:udint; end_var;
    // stop refreshlist at plc. function will return 1 in case of success, otherwise 0
    function global __cdecl sigclib_plc_ReflistStop var_input hdl:^void; channel:udint; end_var var_output retcode:udint; end_var;
    // install callback at refreshlist. callback has to be of prototype extern "C" void CallBack(unsigned long userid, unsigned long value);
    function global __cdecl sigclib_plc_ReflistInstallCallback var_input callback:^void; end_var;
    
    // get string from plc. function will return 1 in case of success, otherwise 0
    function global __cdecl sigclib_plc_GetString var_input hdl:^void; dst:^void; pchrsize:^udint; lasalid:udint; maxbytesize:udint; end_var var_output retcode:udint; end_var;
    // set string at plc. function will return 1 in case of success, otherwise 0
    function global __cdecl sigclib_plc_SetString var_input hdl:^void; src:^void; chrsize:udint; lasalid:udint; end_var var_output retcode:udint; end_var;
    
    // get data from address in plc. function will return 1 in case of success, otherwise 0
    function global __cdecl sigclib_plc_GetDataAt var_input hdl:^void; dst:^void; address:udint; bytelenght:udint; end_var var_output retcode:udint; end_var;
    // set data at address in plc. function will return 1 in case of success, otherwise 0
    function global __cdecl sigclib_plc_SetDataAt var_input hdl:^void; src:^void; address:udint; bytelength:udint; end_var var_output retcode:udint; end_var;

  #endif

  
#endif

// ------------------------------------------------------------------------------------------------
// void* sigclib_plc_Login(unsigned long interface, unsigned long address);
// Function is used to open a connection (ComLink) to other PLC
// --> interface ....... 1=local, 6=can1, 7=can2, 10=tcp1, 11=tcp2
// --> address ......... Address of PLC (IPv4 or cannode)
// <-- Function will return a valid handle, or NULL on error
// Info: IPv4 10.11.12.255 eqates to address=0xFF0C0B0A

// ------------------------------------------------------------------------------------------------
// void sigclib_plc_Logout(void *hdl);
// Function is used to close a ready done connection to other PLC
// --> hdl ............. handle given at call of function sigclib_plc_Login()

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_plc_GetLasalIdSvr(void *hdl, const char *label);
// Function is used to get LasalId of server or object
// --> hdl ............. valid handle given at call of function sigclib_plc_Login()
// --> label ........... ascii-0-terminated name of Server or object
// <-- Function will return a valid LasalId, or 0 on error

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_plc_CallReadMethod(void *hdl, unsigned long *pdst, unsigned long lasalid);
// call read-method of given server by using LasalId
// --> hdl ............. valid handle given at call of function sigclib_plc_Login()
// --> pdst ............ pointer to location where value from server should be filed 
// --> lasalid ......... valid LasalId of server to read
// <-- Function will return 1 on success, on the other hand 0

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_plc_CallWriteMethod(void *hdl, unsigned long lasalid, unsigned long value);
// call write-method of given server by using LasalId
// --> hdl ............. valid handle given at call of function sigclib_plc_Login()
// --> lasalid ......... valid LasalId of server to write
// --> value ........... value to write
// <-- Function will return 1 on success, on the other hand 0

// ------------------------------------------------------------------------------------------------
// void* sigclib_plc_CallNewInstMethod(void *hdl, unsigned long lasalid, unsigned short uicmd, void *para, unsigned long parano);
// call newinst method at given LasalId (NewInst-Server)
// Important: Do not try to call a NewInst-method at server where no such method exists. You will probably crash the enclosed PLC.
// --> hdl ............. valid handle given at call of function sigclib_plc_Login()
// --> lasalid ......... valid LasalId of NewInst-Server
// --> uicmd ........... individual step of NewInst
// --> para ............ pointer to individual 32bit parameter, or NULL
// --> parano .......... number of given 32bit parameter, or 0
// <-- Function will return a valid pointer to result of NewInst, or NULL on error

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_plc_GetString(void *hdl, void *dst, unsigned long *pchrsize, unsigned long lasalid, unsigned long maxbytesize);
// Function is used to get string from PLC by using LasalId
// --> hdl ............. valid handle given at call of function sigclib_plc_Login()
// --> dst ............. destination, where string should be filed
// --> pchrsize ........ destination, where format of string should be filed (1=ascii-0-string, 2=uni-0-string)
// --> lasalid ......... valid LasalId of NewInst-Server
// --> maxbytesize ..... sizeof destination in bytes wher string should be filed
// <-- Function will return 1 on success, on the other hand 0

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_plc_SetString(void *hdl, const void *src, unsigned long chrsize, unsigned long lasalid);
// Function is used to set string at PLC by using LasalId
// --> hdl ............. valid handle given at call of function sigclib_plc_Login()
// --> src ............. given string to set
// --> chrsize ......... format of given string to set (1=ascii-0-string, 2=uni-0-string)
// --> lasalid ......... valid LasalId of NewInst-Server
// <-- Function will return 1 on success, on the other hand 0

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_plc_GetDscCrc(void *hdl, unsigned long *pcrc);
// call function to get descriptor-crc from enclosed plc.
// Note: As long as nothing has changed to the sources of project the descriptor-crc will be the same after powerup.
//       In this case you are allowed to use LasalId of previous session, because there are no changes.
// --> hdl ............. valid handle given at call of function sigclib_plc_Login()
// --> pcrc ............ location where 32bit descriptor-crc should be filed
// <-- function will return 1 in case of success, otherwise 0

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_plc_KeepAlive(void *hdl);
// Function is used to keep connection to plc alive
// --> hdl ............. valid handle given at call of function sigclib_plc_Login()
// <-- function will return 1 in case of success, otherwise 0

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_plc_GetLoaderVersion(void *hdl);
// Function is used to get loaderversion of enclosed plc.
// --> hdl ............. valid handle given at call of function sigclib_plc_Login()
// <-- function will return valid loaderversion, in case of error 0


