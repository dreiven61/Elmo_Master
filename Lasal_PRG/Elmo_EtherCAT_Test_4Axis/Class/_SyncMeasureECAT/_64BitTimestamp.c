

#include "..\sigclib\DefineCompiler.h"
#include "_64BitTimestamp.h"


void calc64BitTS(unsigned long long *p_SysTime_ns, unsigned long SysTime_sec, unsigned long AddOffset)
{
  
  
  //Die Systemuhrzeit wird als offset als 64Bit Wert, auf 1 milisekunde genau gesetzt
  
  //Die Uhrzeit (in Sekunden) wird auf ns aufgeblasen  
  *p_SysTime_ns = SysTime_sec;
  
  *p_SysTime_ns *= 1000000;  
  
  
  *p_SysTime_ns += AddOffset;  
  
  
}



void AddOffs64Bit(unsigned long long *p_timestamp64_ns,  signed long AddOffset)
{
  
  *p_timestamp64_ns += AddOffset;
  
}


signed long CalcDelta(unsigned long long *p_timestamp64_ns,  unsigned long cyctime_ns,  long optshift_ns)
{
  unsigned long cyctimehalf_ns;
  signed long   delta_new = 0;
  
  
  cyctimehalf_ns = cyctime_ns /2;  
  
  //the basis of the reference clock is the integer time of each bus cycle
  //the delta is the deviation of the measured time from the planned base time
  delta_new = (*p_timestamp64_ns + optshift_ns + cyctimehalf_ns) % cyctime_ns - cyctimehalf_ns;  
  
  return delta_new;
  
}


signed long FilterOffset(signed long *pvalue_act32, signed long value_new32, unsigned short filtersize)
{
  signed long long value64 = 0;
  signed long  value32 = 0;
  
  //the base value is Zero-> set the value as initvalue
  if ((*pvalue_act32 == 0) || (filtersize == 0))
  {
    value32 = value_new32;
  }
  else
  {
    value64 = *pvalue_act32;
    
    // multiplicate to weight the new value
    value64 *= filtersize;
    
    value64 += value_new32;
    
    
    value64 /= (filtersize+1);
    
    value32 = (signed long)value64;
  
  }
  
  *pvalue_act32 = value32;
  
  return value32;
  
}

signed long FilterValue(signed long *pvalue_act32, signed long value_new32, unsigned short filtersize)
{
  signed long long value64 = 0;
  signed long  value32 = 0;
  
  
  if (filtersize == 0)
  {
    value32 = value_new32;
  }
  else
  {
    value64 = *pvalue_act32;
    
    // multiplicate to weight the new value
    value64 *= filtersize;
    
    value64 += value_new32;
    
    
    value64 /= (filtersize+1);
    
    value32 = (signed long)value64;
  
  }
  
  *pvalue_act32 = value32;
  
  return value32;
  
}


void AddOffsets(unsigned long long *p_timestamp64_new, signed long long *p_offset1, signed long long *p_offset2
              , unsigned long long *p_timestamp64_old )
                     
{ 
     
  //calc expected timestamp 
  *p_timestamp64_new = *p_timestamp64_old;
  
  //Add offsets to get the expected timestamp for the Reverence Clock
  if (p_offset1 != 0)
    *p_timestamp64_new += *p_offset1;
  
  if (p_offset2 != 0)
    *p_timestamp64_new += *p_offset2; 
  
}


signed long CalcDelta2(unsigned long long *p_timestamp64_ns, signed long long *p_offset1, signed long long *p_offset2
                     , unsigned long long *p_ts_rtstart       , unsigned long cyctime_ns, unsigned long optshift_ns)
                     
{ 
  unsigned long cyctimehalf_ns;
  signed long   delta_new = 0;
  
  cyctimehalf_ns = cyctime_ns /2;
  
  //calc expected timestamp RT-Start(GenIRQ) for next cycle
  *p_timestamp64_ns = *p_ts_rtstart  + cyctime_ns;
  
  //Add offsets to get the expected timestamp for the Reverence Clock
  if (p_offset1 != 0)
    *p_timestamp64_ns += *p_offset1;
  
  if (p_offset2 != 0)
    *p_timestamp64_ns += *p_offset2;
  
  
  //the basis of the reference clock is the integer time of each bus cycle
  //the delta is the deviation of the measured time from the planned base time
  delta_new = (*p_timestamp64_ns + optshift_ns + cyctimehalf_ns) % cyctime_ns - cyctimehalf_ns;  
  
  return delta_new;
  
}











