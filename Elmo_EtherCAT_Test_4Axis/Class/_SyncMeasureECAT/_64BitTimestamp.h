#ifndef _64BITTIMESTAMP_H
#define _64BITTIMESTAMP_H

#include "..\sigclib\DefineCompiler.h"

#ifdef cCompile

  cExtern void calc64BitTS(unsigned long long *p_SysTime_ns, unsigned long SysTime_sec, unsigned long AddOffset);
  
  cExtern void AddOffs64Bit(unsigned long long *p_timestamp64_ns,  signed long AddOffset);
  
  cExtern signed long CalcDelta(unsigned long long *p_timestamp64_ns,  unsigned long cyctime_ns,  long optshift_ns);
  
  cExtern signed long FilterOffset(signed long *pvalue_act32, signed long value_new32, unsigned short filtersize);
  
  cExtern signed long FilterValue(signed long *pvalue_act32, signed long value_new32, unsigned short filtersize);
  
  cExtern void AddOffsets(unsigned long long *p_timestamp64_new, signed long long *p_offset1, signed long long *p_offset2
              , unsigned long long *p_timestamp64_old );
  
  cExtern signed long CalcDelta2(unsigned long long *p_timestamp64_ns, signed long long *p_offset1, signed long long *p_offset2
                               , unsigned long long *p_ts_rtstart       , unsigned long cyctime_ns, unsigned long optshift_ns);

#else

  function global __cdecl calc64BitTS
    var_input
      p_SysTime_ns : ^udint;
      SysTime_sec : udint;
      AddOffset   : udint;      
    end_var;
    
  function global __cdecl AddOffs64Bit
    var_input
      p_timestamp64_ns : ^udint;
      AddOffset      : dint;          
    end_var;
    
  function global __cdecl CalcDelta
    var_input
      p_timestamp64_ns : ^udint;
      cyctime_ns     : udint;
      optshift_ns    : dint;      
    end_var
    VAR_output
    	Delta          : DINT;
    END_VAR;
    
  function global __cdecl FilterOffset
    var_input
      pvalue_act32 : ^dint;
      value_new32 : dint;
      filtersize  : uint;      
    end_var
    VAR_output
    	value_out       : DINT;
    END_VAR;
    
  function global __cdecl FilterValue
    var_input
      pvalue_act32 : ^dint;
      value_new32 : dint;
      filtersize  : uint;      
    end_var
    VAR_output
    	value_out       : DINT;
    END_VAR;
    
  function global __cdecl AddOffsets
    var_input
      p_timestamp64_new : ^udint;
      p_offset1         : ^udint;
      p_offset2         : ^udint;
      p_timestamp64_old : ^udint;            
    end_var;
    
    
    
  function global __cdecl CalcDelta2
    var_input
      p_timestamp64_ns : ^udint;
      p_offset1        : ^udint;
      p_offset2        : ^udint;
      p_ts_rtstart     : ^udint;
      cyctime_ns       : udint;
      optshift_ns      : udint;      
    end_var
    VAR_output
    	Delta          : DINT;
    END_VAR;


#endif


#endif

