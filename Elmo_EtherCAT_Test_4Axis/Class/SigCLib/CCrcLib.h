#ifndef _CCrcLibH
 #define _CCrcLibH
  
  #include "DefineCompiler.h"

  #ifdef cCompile
    // compute crc32 of given data
    cExtern unsigned long  sigclib_crc32(unsigned long initval, void *psrc, unsigned long length);
    cExtern unsigned long  long sigclib_crc64(unsigned long long initval, void *psrc, unsigned long length);
    cExtern unsigned long  sigclib_strcrc(unsigned long initval, const char *ptxt);
    cExtern unsigned long  sigclib_strcrc16(unsigned long initval, const unsigned short *ptxt);
    
    cExtern unsigned long  sigclib_crc32_alternativ(unsigned long initval, const void *psrc, unsigned long length);
    cExtern unsigned long  sigclib_rolU32(unsigned long val, unsigned long irol);
    cExtern unsigned long  sigclib_rorU32(unsigned long val, unsigned long iror);
    cExtern unsigned short sigclib_rolU16(unsigned short val, unsigned long irol);
    cExtern unsigned short sigclib_rorU16(unsigned short val, unsigned long iror);
    cExtern unsigned char  sigclib_rolU08(unsigned char val, unsigned long irol);
    cExtern unsigned char  sigclib_rorU08(unsigned char val, unsigned long iror);
    
  #else
    // compute crc32 of given data
    function global __cdecl sigclib_crc32 var_input initval:udint; psrc:^void; length:udint; end_var var_output retcode:udint; end_var;
    function global __cdecl sigclib_strcrc var_input initval:udint; ptxt:^void; end_var var_output retcode:udint; end_var;
    function global __cdecl sigclib_strcrc16 var_input initval:udint; ptxt:^void; end_var var_output retcode:udint; end_var;
    
    function global __cdecl sigclib_crc32_alternativ var_input initval:udint; psrc:^void; length:udint; end_var var_output retcode:udint; end_var;
    function global __cdecl sigclib_rolU32 var_input val:udint; irol:udint; end_var var_output retcode:udint; end_var;
    function global __cdecl sigclib_rorU32 var_input val:udint; iror:udint; end_var var_output retcode:udint; end_var;
    function global __cdecl sigclib_rolU16 var_input val:uint; irol:udint; end_var var_output retcode:uint; end_var;
    function global __cdecl sigclib_rorU16 var_input val:uint; iror:udint; end_var var_output retcode:uint; end_var;
    function global __cdecl sigclib_rolU08 var_input val:usint; irol:udint; end_var var_output retcode:usint; end_var;
    function global __cdecl sigclib_rorU08 var_input val:usint; iror:udint; end_var var_output retcode:usint; end_var;
    
  #endif
  
#endif

// ************************************************************************************************
// USAGE
// ************************************************************************************************

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_crc32(unsigned long initval, void *psrc, unsigned long length);
// function will compute crc32 all over given data
// --> initval ......... seed (arbitrary initial value)
// --> psrc ............ pointer to data
// --> length .......... length of data in bytes
// <-- function will return computed crc32

// ------------------------------------------------------------------------------------------------
// unsigned long long sigclib_crc64(unsigned long long initval, void *psrc, unsigned long length);
// function will compute crc64 all over given data
// --> initval ......... seed (arbitrary initial value)
// --> psrc ............ pointer to data
// --> length .......... length of data in bytes
// <-- function will return computed crc64

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_strcrc(unsigned long initval, const char *ptxt);
// function will compute crc all over given ascii-0-string
// --> initval ......... seed (arbitrary initial value)
// --> ptxt ............ pointer to ascii-0-string
// <-- function will return computed crc

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_strcrc16(unsigned long initval, const unsigned short *ptxt);
// function will compute crc all over given uni-0-string
// --> initval ......... seed (arbitrary initial value)
// --> ptxt ............ pointer to uni-0-string
// <-- function will return computed crc

// ------------------------------------------------------------------------------------------------
// unsigned long  sigclib_crc32_alternativ(unsigned long initval, const void *psrc, unsigned long length);
// function will compute a alternative crc32 all over given data
// --> initval ......... seed (arbitrary initial value)
// --> psrc ............ pointer to data
// --> length .......... length of data in bytes
// <-- function will return computed crc32

// ------------------------------------------------------------------------------------------------
// unsigned long  sigclib_rolU32(unsigned long val, unsigned long irol);
// unsigned long  sigclib_rorU32(unsigned long val, unsigned long iror);
// unsigned short sigclib_rolU16(unsigned short val, unsigned long irol);
// unsigned short sigclib_rorU16(unsigned short val, unsigned long iror);
// unsigned char  sigclib_rolU08(unsigned char val, unsigned long irol);
// unsigned char  sigclib_rorU08(unsigned char val, unsigned long iror);
// these functions will rotate value by the number of given iterations
// --> val ............. value to rotate (rol = rotate left, ror = rotate right)
// --> irol, iror ...... iterations used to rotate
// <-- function will return rotated value
