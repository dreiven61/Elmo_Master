// +----------------------------------------------------------------------------------------------+
// +-[   copyright ] Sigmatek GmbH & CoKG                                                         |
// +-[      author ] kolott                                                                       |
// +-[        date ] 30.04.2023                                                                   |
// +-[ description ]------------------------------------------------------------------------------+
// |                                                                                              |
// |                                                                                              |
// +----------------------------------------------------------------------------------------------+

#ifndef _SigKeyH
 #define _SigKeyH
  
 #include "DefineCompiler.h"
 
 
 #ifdef cCompile // *******************************************************************************
 
    // #define SigCLibKey_INUSE // compile code for sigkey

    // check SigKey
    typedef _uint32( *f_SigKeyCallBack) (char *label, void *data, _uint32 datalen, void *pcookie);
    cExtern _uint32 sigclib_sigkey(const char *dpne, const char *usage, char *serno, _uint32 sizeof_serno, _uint32 key0, _uint32 key1, f_SigKeyCallBack pcallback, void *pcookie);
 
  #else // ****************************************************************************************
 
    // check sigkey
    function global __cdecl sigclib_sigkey var_input dpne:^char; usage:^char; serno:^char; sizeof_serno:udint; key0:udint; key1:udint; pcallback:^void; pcookie:^void; end_var var_output retcode:udint; end_var;
 
 #endif // ****************************************************************************************
#endif

// ------------------------------------------------------------------------------------------------
// _uint32 sigclib_sigkey(const char *dpne, const char *usage, char *serno, _uint32 sizeof_serno, _uint32 key0, _uint32 key1, f_SigKeyCallBack pcallback, void *pcookie)
// Function is used to check entire SigKey
// --> dpne ............ drive, path, name and extention of SigKey file to examine (eg. "E:\sigkey_slam.key")
// --> usage ........... usage of SigKey eg."WISP"
// --> serno ........... destination where Sigmatek S/N should be filed, or NULL
// --> sizeof_serno .... bytesize of given Sigmatek S/N destanation
// --> key0 ............ correct Key0 used to decode SigKey
// --> key1 ............ correct Key1 used to decode SigKey
// --> pcallback ....... callback to iterate datalines or NULL
// --> pcookie ......... Usercookie, eg.this-pointer
// <-- Function will return 1 on success, on the other hand 0
//
// Note: given callback to iterate datalines has to be of following prototype
//  FUNCTION __CDECL GLOBAL UserClass::SigKeyDataLineIterator
//  VAR_INPUT
//    label : ^CHAR;        // used to assign tag
//    data : ^void;         // used to be assign data ot tag
//    datalen : UDINT;      // datalength in bytes
//    pcookie : ^UserClass; // pcookie
//  END_VAR
//  VAR_OUTPUT
//    retcode : udint;
//  END_VAR
// 
//    this    := pcookie;   // set this-pointer
//    retcode := 1;         // goon
//
//  END_FUNCTION

