
#ifndef _C_SPLINE_H_
  #define _C_SPLINE_H_
  
FUNCTION GLOBAL __cdecl XT_CubicSpline
  VAR_INPUT
  	a1        : LREAL;
  	a2        : LREAL;
  	a3        : LREAL;
  	pathAct   : LREAL;
  	pathSoll  : LREAL;   
    ptr_retXt : ^LREAL;	
  END_VAR;

FUNCTION GLOBAL __cdecl VT_CubicSpline
  VAR_INPUT
  	a1        : LREAL;
  	a2        : LREAL;
  	a3        : LREAL;
  	pathAct   : LREAL;
  	pathSoll  : LREAL;   
    ptr_retXt : ^LREAL;	
  END_VAR;  
  
FUNCTION GLOBAL __cdecl AT_CubicSpline
  VAR_INPUT
  	a1        : LREAL;
  	a2        : LREAL;
  	a3        : LREAL;
  	pathAct   : LREAL;
  	pathSoll  : LREAL;   
    ptr_retXt : ^LREAL;	
  END_VAR; 
 

FUNCTION GLOBAL __cdecl XT_QuintSpline
  VAR_INPUT
  	a1        : LREAL;
  	a2        : LREAL;
  	a3        : LREAL;
  	a4        : LREAL;
  	a5        : LREAL;
  	pathAct   : LREAL;
  	pathSoll  : LREAL;    
    ptr_retXt : ^LREAL;	
  END_VAR;
  
FUNCTION GLOBAL __cdecl VT_QuintSpline
  VAR_INPUT
  	a1        : LREAL;
  	a2        : LREAL;
  	a3        : LREAL;
  	a4        : LREAL;
  	a5        : LREAL;
  	pathAct   : LREAL;
  	pathSoll  : LREAL;    
    ptr_retXt : ^LREAL;	
  END_VAR;
  
FUNCTION GLOBAL __cdecl AT_QuintSpline
  VAR_INPUT
  	a1        : LREAL;
  	a2        : LREAL;
  	a3        : LREAL;
  	a4        : LREAL;
  	a5        : LREAL;
  	pathAct   : LREAL;
  	pathSoll  : LREAL;    
    ptr_retXt : ^LREAL;	
  END_VAR;    
  
FUNCTION GLOBAL __cdecl JT_QuintSpline
  VAR_INPUT
  	a1        : LREAL;
  	a2        : LREAL;
  	a3        : LREAL;
  	a4        : LREAL;
  	a5        : LREAL;
  	pathAct   : LREAL;
  	pathSoll  : LREAL;    
    ptr_retXt : ^LREAL;	
  END_VAR;  
  
FUNCTION GLOBAL __cdecl dJT_QuintSpline
  VAR_INPUT
  	a1        : LREAL;
  	a2        : LREAL;
  	a3        : LREAL;
  	a4        : LREAL;
  	a5        : LREAL;
  	pathAct   : LREAL;
  	pathSoll  : LREAL;    
    ptr_retXt : ^LREAL;	
  END_VAR;   
  
#endif