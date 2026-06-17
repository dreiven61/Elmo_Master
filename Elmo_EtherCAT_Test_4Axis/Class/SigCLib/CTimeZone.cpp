// +-------------------------------------------------------------------------------+
// +-[   copyright ] Sigmatek GmbH & CoKG                                          |
// +-[      author ] kolott                                                        |
// +-[        date ] 05.09.2025                                                    |
// +-[ description ]---------------------------------------------------------------+
// |                                                                               |
// |                                                                               |
// +-------------------------------------------------------------------------------+

#include "SigCLib.h" 

// Note: These two interfaces may or may not be present by definition, this does not depend on the OpSys version.
// If necessary, they can be installed at a later time by using a package. (obejoh 05.09.2025)


#define  TIMEZONE_ERR_REBOOT_NEEDED   1 // ok, but reboot needed
#define  TIMEZONE_ERR_NO              0 // ok
#define  TIMEZONE_ERR_NOTFOUND       -1 // ID/name not found
#define  TIMEZONE_ERR_INVAL          -2 // invalid parameter
#define  TIMEZONE_ERR_INTERFACE     -99 // error interface

#pragma pack(push,1)
  typedef struct 
  {
    _uint32     version;
    _uint32     id;
    const char *zonename;
  } sigclib_cil_timezone_info;

  typedef struct
  {
    _uint32 version;
    _int32 ( *CurrentGetName)(char *buf, _uint32 size);
    _int32 ( *CurrentGetId)(_int32 *id);
    _uint32 ( *GetCount)(void);
    sigclib_cil_timezone_info *( *GetAt)(_uint32 idx);
    _int32 ( *zonename2id)(const char *zonename);
    _int32 ( *SetLocalTZ)(const char *zonename);
    _int32 ( *SetSystemTZ)(const char *zonename);
    _int32 ( *SetLocalTZid)(_int32 hash);
    _int32 ( *SetSystemTZid)(_int32 hash);
  } sigclib_cil_timezone;
  
  typedef struct 
  {
    _uint32 version;
    _int32 ( *GetUtcDateTime)(void *dt, _uint32 typeDTF);
    _int32 ( *GetLocalDateTime)(void *dt, _uint32 typeDTF);
    _int32 ( *ConvertFmt)(const void *dtIn, _uint32 typeDTFin, void *dtOut, _uint32 typeDTFout);
    _int32 ( *SetUtcDateTime)(void *dt, _uint32 typeDTF);
    _int32 ( *SetLocalDateTime)(void *dt, _uint32 typeDTF);
    _int32 ( *Utc2Local)(const void *dtIn, _uint32 typeDTFin, void *dtOut, _uint32 typeDTFout);
    _int32 ( *Local2Utc)(const void *dtIn, _uint32 typeDTFin, void *dtOut, _uint32 typeDTFout);
    _uint32 ( *GetTickMS32)(void);
    void ( *GetTickMS64)(_uint64 *pTicks);
    _uint32 ( *GetTickUS32)(void);
    void ( *GetTickUS64)(_uint64 *pTicks);
  } sigclib_cil_datetime;
  
#pragma pack(pop)

static sigclib_cil_timezone *CilTimeZone = NULL; // Pointer to interface or NULL
static sigclib_cil_datetime *CilDateTime = NULL; // Pointer to interface or NULL
static _uint32 CilTimeZoneDone = 0; // Indicator if already tried to get interface
static _uint32 CilDateTimeDone = 0; // Indicator if already tried to get interface


// +-------------------------------------------------------------------------------+
// +-------------------------------------------------------------------------------+
// | LSL_TIMEZONE                                                                  |
// +-------------------------------------------------------------------------------+
// +-------------------------------------------------------------------------------+


static bool CilTimeZone_Usage(void)
{
  if(CilTimeZone != NULL)
  {
    return true;
  }
  
  if(CilTimeZoneDone == 0)
  {
    CilTimeZone = (sigclib_cil_timezone*)sigclib_cilget("LSL_TIMEZONE");
    CilTimeZoneDone = 0xA5;
  }
  
  return (CilTimeZone == NULL)? false : true;
}

//_uint32 sigclib_timezone_get_version(void)
//{
//  if(CilTimeZone_Usage() == true)
//  {
//    return CilTimeZone->version;
//  }
//  return 0;
//}

_int32 sigclib_timezone_get_current(_int32 *pid, char *dst, _uint32 sizeof_dst)
{
  if(CilTimeZone_Usage() == true)
  {
    _int32 retcode = TIMEZONE_ERR_NO;
    if((dst != NULL) && (sizeof_dst > 0))
    {
      CilTimeZone->CurrentGetName(dst, sizeof_dst);
    }
    if(pid != NULL)
    {
      retcode = CilTimeZone->CurrentGetId(pid);
    }
    return retcode;
  }
  
  return TIMEZONE_ERR_INTERFACE;
}

_uint32 sigclib_timezone_get_count(void)
{
  if(CilTimeZone_Usage() == true)
  {
    return CilTimeZone->GetCount();
  }
  
  return 0;
}

_int32 sigclib_timezone_get_info(_int32 *pid, char *dst, _uint32 sizeof_dst, _uint32 index)
{
  if(CilTimeZone_Usage() == true)
  {
    sigclib_cil_timezone_info *pinfo = CilTimeZone->GetAt(index);
    if(pinfo != NULL)
    {
      if(pinfo->zonename != NULL)
      {
        if(pid != NULL)
        {
          *pid = pinfo->id;
        }
        if((dst != NULL) && (sizeof_dst > 0))
        {
          sigclib_strlcpy(dst, pinfo->zonename, sizeof_dst);
        }
        return TIMEZONE_ERR_NO;
      }
    }
    return TIMEZONE_ERR_NOTFOUND;
  }
  
  return TIMEZONE_ERR_INTERFACE;
}

_int32 sigclib_timezone_create_id(_int32 *pid, const char *zonename)
{
  if(CilTimeZone_Usage() == true)
  {
    _int32 id = CilTimeZone->zonename2id(zonename);
    if(id >= 0) // obejoh: 17.09.2025
    {
      *pid = id;
      return TIMEZONE_ERR_NO;
    }
    return id;
  }
  return TIMEZONE_ERR_INTERFACE;
}

_int32 sigclib_timezone_set_by_name(const char *zonename)
{
  if(CilTimeZone_Usage() == true)
  {
    return CilTimeZone->SetSystemTZ(zonename);
  }
  return TIMEZONE_ERR_INTERFACE;
}

_int32 sigclib_timezone_set_by_id(_int32 id)
{
  if(CilTimeZone_Usage() == true)
  {
    return CilTimeZone->SetSystemTZid(id);
  }
  return TIMEZONE_ERR_INTERFACE;
}

// +-------------------------------------------------------------------------------+
// +-------------------------------------------------------------------------------+
// | LSL_DATETIME                                                                  |
// +-------------------------------------------------------------------------------+
// +-------------------------------------------------------------------------------+

static bool CilDateTime_Usage(void)
{
  if(CilDateTime != NULL)
  {
    return true;
  }
  
  if(CilDateTimeDone == 0)
  {
    CilDateTime = (sigclib_cil_datetime*)sigclib_cilget("LSL_DATETIME");
    CilDateTimeDone = 0xA5;
  }
  
  return (CilDateTime == NULL)? false : true;
}

//_uint32 sigclib_datetime_get_version(void)
//{
//  if(CilDateTime_Usage() == true)
//  {
//    return CilDateTime->version;
//  }
//  return 0;
//}

static _int32 sigclib_datetime_get_alternativ(void *pd, _uint32 dtf_type)
{
  SYSDATE sdate;
  SYSTIME stime;
  sigclib_tm tm;
  
  switch(dtf_type)
  {
    case 1 : // sigclib_tm
      sigclib_getsystime(&stime);
      sigclib_getsysdate(&sdate);
      sigclib_memset(pd, 0, sizeof(sigclib_tm));
      ((sigclib_tm*)pd)->tm_sec   = stime.wSecond;
      ((sigclib_tm*)pd)->tm_min   = stime.wMinute;
      ((sigclib_tm*)pd)->tm_hour  = stime.wHour;
      ((sigclib_tm*)pd)->tm_mday  = sdate.wDay;
      ((sigclib_tm*)pd)->tm_mon   = sdate.wMonth - 1;
      ((sigclib_tm*)pd)->tm_year  = sdate.wYear - 1900;
      ((sigclib_tm*)pd)->tm_wday  = sdate.wDayOfWeek;
      ((sigclib_tm*)pd)->tm_yday  = sigclib_days_since_01_jan(sdate.wYear, sdate.wMonth, sdate.wDay);
      return TIMEZONE_ERR_NO;
      
    case 3: // sigclib_systm
      sigclib_getsystime(&stime);
      sigclib_getsysdate(&sdate);
      sigclib_memset(pd, 0, sizeof(sigclib_systm));
      ((sigclib_systm*)pd)->wSecond    = stime.wSecond;
      ((sigclib_systm*)pd)->wMinute    = stime.wMinute;
      ((sigclib_systm*)pd)->wHour      = stime.wHour;
      ((sigclib_systm*)pd)->wDay       = sdate.wDay;
      ((sigclib_systm*)pd)->wMonth     = sdate.wMonth;
      ((sigclib_systm*)pd)->wYear      = sdate.wYear;
      ((sigclib_systm*)pd)->wDayOfWeek = sdate.wDayOfWeek;
      return TIMEZONE_ERR_NO;

    case 4: // int64 seconds since 01.01.1970
      if(sigclib_datetime_get_alternativ(&tm, 1) == TIMEZONE_ERR_NO)
      {
        *(_int64*)pd = sigclib_timegm64(&tm);
        return TIMEZONE_ERR_NO;
      }
      break;
      
    case 5: // int64 + int32 seconds + nanoseconds since 01.01.1970 type LSL_TIMESTAMP64
      if(sigclib_datetime_get_alternativ(&tm, 1) == TIMEZONE_ERR_NO)
      {
        sigclib_memset(pd, 0, sizeof(sigclib_timestamp64));
        ((sigclib_timestamp64*)pd)->wSec = sigclib_timegm64(&tm);
        return TIMEZONE_ERR_NO;
      }
      break;
      
    case 6: // sigclib_sysdatetime_ms, SYSDATETIMEMS
      sigclib_memset(pd, 0, sizeof(sigclib_sysdatetime_ms));
      sigclib_getsystime(&stime);
      sigclib_getsysdate(&sdate);
      ((sigclib_sysdatetime_ms*)pd)->wSecond    = stime.wSecond;
      ((sigclib_sysdatetime_ms*)pd)->wMinute    = stime.wMinute;
      ((sigclib_sysdatetime_ms*)pd)->wHour      = stime.wHour;
      ((sigclib_sysdatetime_ms*)pd)->wDay       = sdate.wDay;
      ((sigclib_sysdatetime_ms*)pd)->wMonth     = sdate.wMonth;
      ((sigclib_sysdatetime_ms*)pd)->wYear      = sdate.wYear;
      ((sigclib_sysdatetime_ms*)pd)->wDayOfWeek = sdate.wDayOfWeek;
      return TIMEZONE_ERR_NO;

    case 7: // SYSTIME
      sigclib_getsystime((SYSTIME*)pd);
      return TIMEZONE_ERR_NO;
    
    case 8: // SYSDATE
      sigclib_getsysdate((SYSDATE*)pd);
      return TIMEZONE_ERR_NO;
  }
  
  return TIMEZONE_ERR_INTERFACE;
}

static _int32 sigclib_datetime_set_alternativ(void *ps, _uint32 dtf_type)
{
  SYSDATE sdate;
  SYSTIME stime;
  sigclib_tm tm;
  
  switch(dtf_type)
  {
    case 1 : // sigclib_tm
      stime.wSecond = ((sigclib_tm*)ps)->tm_sec;
      stime.wMinute = ((sigclib_tm*)ps)->tm_min;
      stime.wHour   = ((sigclib_tm*)ps)->tm_hour;
      sdate.wDay    = ((sigclib_tm*)ps)->tm_mday;
      sdate.wMonth  = ((sigclib_tm*)ps)->tm_mon + 1;
      sdate.wYear   = ((sigclib_tm*)ps)->tm_year + 1900;
      sdate.wDayOfWeek = ((sigclib_tm*)ps)->tm_wday;
      sigclib_setsystime(&stime);
      sigclib_setsysdate(&sdate);
      return TIMEZONE_ERR_NO;
      
    case 3: // sigclib_systm
      stime.wSecond = ((sigclib_systm*)ps)->wSecond;
      stime.wMinute = ((sigclib_systm*)ps)->wMinute;
      stime.wHour   = ((sigclib_systm*)ps)->wHour;
      sdate.wDay    = ((sigclib_systm*)ps)->wDay;
      sdate.wMonth  = ((sigclib_systm*)ps)->wMonth;
      sdate.wYear   = ((sigclib_systm*)ps)->wYear;
      sdate.wDayOfWeek = ((sigclib_systm*)ps)->wDayOfWeek;
      sigclib_setsystime(&stime);
      sigclib_setsysdate(&sdate);
      return TIMEZONE_ERR_NO;

    case 4: // int64 seconds since 01.01.1970
      return sigclib_datetime_set_alternativ(sigclib_gmtime64((_int64*)ps, &tm), 1);

    case 5: // int64 + int32 seconds + nanoseconds since 01.01.1970 type LSL_TIMESTAMP64
      return sigclib_datetime_set_alternativ(&(((sigclib_timestamp64*)ps)->wSec), 4);

    case 6: // sigclib_sysdatetime_ms, SYSDATETIMEMS
      stime.wSecond    = ((sigclib_sysdatetime_ms*)ps)->wSecond;
      stime.wMinute    = ((sigclib_sysdatetime_ms*)ps)->wMinute;
      stime.wHour      = ((sigclib_sysdatetime_ms*)ps)->wHour;
      sdate.wDay       = ((sigclib_sysdatetime_ms*)ps)->wDay;
      sdate.wMonth     = ((sigclib_sysdatetime_ms*)ps)->wMonth;
      sdate.wYear      = ((sigclib_sysdatetime_ms*)ps)->wYear;
      sdate.wDayOfWeek = ((sigclib_sysdatetime_ms*)ps)->wDayOfWeek;
      sigclib_setsystime(&stime);
      sigclib_setsysdate(&sdate);
      return TIMEZONE_ERR_NO;

    case 7: // SYSTIME
      sigclib_setsystime((SYSTIME*)ps);
      return TIMEZONE_ERR_NO;
    
    case 8: // SYSDATE
      sigclib_setsysdate((SYSDATE*)ps);
      return TIMEZONE_ERR_NO;
  }
  
  return TIMEZONE_ERR_INTERFACE;
}

static _int32 sigclib_datetime_convert_to_std(sigclib_systm *pd, const void *ps, _uint32 dtf_type)
{
  sigclib_memset(pd, 0, sizeof(sigclib_systm));
  sigclib_tm tm;

  switch(dtf_type)
  {
    case 1 : // sigclib_tm
      pd->wSecond    = ((sigclib_tm*)ps)->tm_sec;
      pd->wMinute    = ((sigclib_tm*)ps)->tm_min;
      pd->wHour      = ((sigclib_tm*)ps)->tm_hour;
      pd->wDay       = ((sigclib_tm*)ps)->tm_mday;
      pd->wMonth     = ((sigclib_tm*)ps)->tm_mon + 1;
      pd->wYear      = ((sigclib_tm*)ps)->tm_year + 1900;
      pd->wDayOfWeek = ((sigclib_tm*)ps)->tm_wday;
      return TIMEZONE_ERR_NO;
      
    case 3: // sigclib_systm
      sigclib_memcpy(pd, ps, sizeof(sigclib_systm));
      return TIMEZONE_ERR_NO;

    case 4: // int64 seconds since 01.01.1970
      sigclib_gmtime64((_int64*)ps, &tm);
      return sigclib_datetime_convert_to_std(pd, &tm, 1);

    case 5: // int64 + int32 seconds + nanoseconds since 01.01.1970 type LSL_TIMESTAMP64
      sigclib_gmtime64(&(((sigclib_timestamp64*)ps)->wSec), &tm);
      if(sigclib_datetime_convert_to_std(pd, &tm, 1) == TIMEZONE_ERR_NO)
      {
        _int32 nano = ((sigclib_timestamp64*)ps)->wNanoSec;
        ((sigclib_systm*)pd)->wMilliSecond = nano / 1000000;
        ((sigclib_systm*)pd)->wMicroSecond = (nano / 1000) % 1000;
        ((sigclib_systm*)pd)->wNanoSecond = nano % 1000;
        return TIMEZONE_ERR_NO;
      }
      break;

    case 6: // sigclib_sysdatetime_ms, SYSDATETIMEMS
      pd->wSecond      = ((sigclib_sysdatetime_ms*)ps)->wSecond;
      pd->wMinute      = ((sigclib_sysdatetime_ms*)ps)->wMinute;
      pd->wHour        = ((sigclib_sysdatetime_ms*)ps)->wHour;
      pd->wDay         = ((sigclib_sysdatetime_ms*)ps)->wDay;
      pd->wMonth       = ((sigclib_sysdatetime_ms*)ps)->wMonth;
      pd->wYear        = ((sigclib_sysdatetime_ms*)ps)->wYear;
      pd->wDayOfWeek   = ((sigclib_sysdatetime_ms*)ps)->wDayOfWeek;
      pd->wMilliSecond = ((sigclib_sysdatetime_ms*)ps)->wMilliSecond;
      return TIMEZONE_ERR_NO;

    case 7: // SYSTIME
      pd->wSecond    = ((SYSTIME*)ps)->wSecond;
      pd->wMinute    = ((SYSTIME*)ps)->wMinute;
      pd->wHour      = ((SYSTIME*)ps)->wHour;
      return TIMEZONE_ERR_NO;
    
    case 8: // SYSDATE
      pd->wDay       = ((SYSDATE*)ps)->wDay;
      pd->wMonth     = ((SYSDATE*)ps)->wMonth;
      pd->wYear      = ((SYSDATE*)ps)->wYear;
      pd->wDayOfWeek = ((SYSDATE*)ps)->wDayOfWeek;
      return TIMEZONE_ERR_NO;
   }
   
   return TIMEZONE_ERR_INVAL;
}

static _int32 sigclib_datetime_convert_to_dst(void *pd, _uint32 dtf_type, sigclib_systm *ps)
{
  sigclib_tm tm;

  switch(dtf_type)
  {
    case 1 : // sigclib_tm
      sigclib_memset(pd, 0, sizeof(sigclib_tm));
      ((sigclib_tm*)pd)->tm_sec  = ps->wSecond;
      ((sigclib_tm*)pd)->tm_min  = ps->wMinute;
      ((sigclib_tm*)pd)->tm_hour = ps->wHour;
      ((sigclib_tm*)pd)->tm_mday = ps->wDay; 
      ((sigclib_tm*)pd)->tm_mon  = ps->wMonth - 1;
      ((sigclib_tm*)pd)->tm_year = ps->wYear - 1900;
      ((sigclib_tm*)pd)->tm_wday = ps->wDayOfWeek;
      ((sigclib_tm*)pd)->tm_yday = sigclib_days_since_01_jan(ps->wYear, ps->wMonth, ps->wDay);
      return TIMEZONE_ERR_NO;
      
    case 3: // sigclib_systm
      sigclib_memcpy(pd, ps, sizeof(sigclib_systm));
      return TIMEZONE_ERR_NO;

    case 4: // int64 seconds since 01.01.1970
      if(sigclib_datetime_convert_to_dst(&tm, 1, ps) == TIMEZONE_ERR_NO)
      {
        *(_int64*)pd = sigclib_timegm64(&tm);
        return TIMEZONE_ERR_NO;
      }
      break;

    case 5: // int64 + int32 seconds + nanoseconds since 01.01.1970 type LSL_TIMESTAMP64
      if(sigclib_datetime_convert_to_dst(&tm, 1, ps) == TIMEZONE_ERR_NO)
      {
        sigclib_memset(pd, 0, sizeof(sigclib_timestamp64)); // default 0
        ((sigclib_timestamp64*)pd)->wSec     = sigclib_timegm64(&tm);
        ((sigclib_timestamp64*)pd)->wNanoSec = (((((_int32)ps->wMilliSecond) * 1000) + ((_int32)ps->wMicroSecond)) * 1000) + (_int32)ps->wNanoSecond;
        return TIMEZONE_ERR_NO;
      }
      break;

    case 6: // SYSDATETIMEMS
      sigclib_memset(pd, 0, sizeof(sigclib_sysdatetime_ms)); // default 0
      ((sigclib_sysdatetime_ms*)pd)->wSecond    = ps->wSecond;
      ((sigclib_sysdatetime_ms*)pd)->wMinute    = ps->wMinute;
      ((sigclib_sysdatetime_ms*)pd)->wHour      = ps->wHour;
      ((sigclib_sysdatetime_ms*)pd)->wDay       = ps->wDay;
      ((sigclib_sysdatetime_ms*)pd)->wMonth     = ps->wMonth;
      ((sigclib_sysdatetime_ms*)pd)->wYear      = ps->wYear;
      ((sigclib_sysdatetime_ms*)pd)->wDayOfWeek = ps->wDayOfWeek;
      return TIMEZONE_ERR_NO;

    case 7: // SYSTIME
      sigclib_memset(pd, 0, sizeof(SYSTIME)); // default 0
      ((SYSTIME*)pd)->wSecond = ps->wSecond;
      ((SYSTIME*)pd)->wMinute = ps->wMinute;
      ((SYSTIME*)pd)->wHour   = ps->wHour;
      return TIMEZONE_ERR_NO;

    case 8: // SYSDATE
      sigclib_memset(pd, 0, sizeof(SYSDATE)); // default 0
      ((SYSDATE*)pd)->wDay       = ps->wDay;
      ((SYSDATE*)pd)->wMonth     = ps->wMonth;
      ((SYSDATE*)pd)->wYear      = ps->wYear;
      ((SYSDATE*)pd)->wDayOfWeek = ps->wDayOfWeek;
      return TIMEZONE_ERR_NO;
   }
   
   return TIMEZONE_ERR_INVAL;
}

static _int32 sigclib_datetime_convert_alternativ(void *pdst, _uint32 dtf_type_dst, const void *psrc, _uint32 dtf_type_src)
{
  if((dtf_type_src == 7) && (dtf_type_dst != 7))
  {
    return TIMEZONE_ERR_INVAL;  // Zeit kann nicht in etwas anderes umgewandelt werden
  }

  if(((dtf_type_dst == 7) && (dtf_type_src == 8)) || ((dtf_type_dst == 8) && (dtf_type_src == 7)))
  {
    return TIMEZONE_ERR_INVAL; // Datum in Zeit (oder umgekehrt) umwandeln geht nicht
  }
  
  sigclib_systm systm;
  if(sigclib_datetime_convert_to_std(&systm, psrc, dtf_type_src) == TIMEZONE_ERR_NO) // von src_typ auf sigclib_systm wandeln
  {
    return sigclib_datetime_convert_to_dst(pdst, dtf_type_dst, &systm); // von sigclib_systm auf dst_typ wandeln
  }
  
  return TIMEZONE_ERR_INVAL;
}

_int32 sigclib_datetime_get_utc(void *pdst, _uint32 dtf_type)
{
  if(CilDateTime_Usage() == true)
  {
    return CilDateTime->GetUtcDateTime(pdst, dtf_type);
  }
  return sigclib_datetime_get_alternativ(pdst, dtf_type); // falls interface nicht existiert
}

_int32 sigclib_datetime_set_utc(void *psrc, _uint32 dtf_type)
{
  if(CilDateTime_Usage() == true)
  {
    return CilDateTime->SetUtcDateTime(psrc, dtf_type);
  }
  return sigclib_datetime_set_alternativ(psrc, dtf_type); // falls interface nicht existiert
}

_int32 sigclib_datetime_get_local(void *pdst, _uint32 dtf_type)
{
  if(CilDateTime_Usage() == true)
  {
    return CilDateTime->GetLocalDateTime(pdst, dtf_type);
  }
  return sigclib_datetime_get_alternativ(pdst, dtf_type); // falls interface nicht existiert
}

_int32 sigclib_datetime_set_local(void *psrc, _uint32 dtf_type)
{
  if(CilDateTime_Usage() == true)
  {
    return CilDateTime->SetLocalDateTime(psrc, dtf_type);
  }
  return sigclib_datetime_set_alternativ(psrc, dtf_type); // falls interface nicht existiert
}

_int32 sigclib_datetime_convert_format(void *pdst, _uint32 dtf_type_dst, const void *psrc, _uint32 dtf_type_src)
{
  if(CilDateTime_Usage() == true)
  {
    return CilDateTime->ConvertFmt(psrc, dtf_type_src, pdst, dtf_type_dst);
  }
  return sigclib_datetime_convert_alternativ(pdst, dtf_type_dst, psrc, dtf_type_src);
}

_int32 sigclib_datetime_utc_to_local(void *pdst, _uint32 dtf_type_dst, const void *psrc, _uint32 dtf_type_src)
{
  if(CilDateTime_Usage() == true)
  {
    return CilDateTime->Utc2Local(psrc, dtf_type_src, pdst, dtf_type_dst);
  }
  return sigclib_datetime_convert_format(pdst, dtf_type_dst, psrc, dtf_type_src); // konvertieren, utc und local sind gleich
}

_int32 sigclib_datetime_local_to_utc(void *pdst, _uint32 dtf_type_dst, const void *psrc, _uint32 dtf_type_src)
{
  if(CilDateTime_Usage() == true)
  {
    return CilDateTime->Local2Utc(psrc, dtf_type_src, pdst, dtf_type_dst);
  }
  return sigclib_datetime_convert_format(pdst, dtf_type_dst, psrc, dtf_type_src); // konvertieren, utc und local sind gleich
}


// wird aktuell nicht benötigt, ist das gleiche wie sigclib_tabsolute()
//_int32 sigclib_timezone_ticks32_ms(_uint32 *pdst32)
//{
//  if(CilDateTime_Usage() == true)
//  {
//	  *pdst32 = CilDateTime->GetTickMS32();
//    return TIMEZONE_ERR_NO;
//  }
//  *pdst32 = 0;
//  return TIMEZONE_ERR_INTERFACE;
//}

// wird aktuell nicht benötigt, ist das gleiche wie sigclib_getmicrotime()
//_int32 sigclib_timezone_ticks32_us(_uint32 *pdst32)
//{
//  if(CilDateTime_Usage() == true)
//  {
//	  *pdst32 = CilDateTime->GetTickUS32();
//    return TIMEZONE_ERR_NO;
//  }
//  *pdst32 = 0;
//  return TIMEZONE_ERR_INTERFACE;
//}

// es gibt aktuell keinen Datentyp (uint64) welcher dies aufnehmen könnte
//_int32 sigclib_timezone_ticks64_ms(_uint64 *pdst64)
//{
//  if(CilDateTime_Usage() == true)
//  {
//	  CilDateTime->GetTickMS64(pdst64);
//    return TIMEZONE_ERR_NO;
//  }
//  *pdst64 = 0;
//  return TIMEZONE_ERR_INTERFACE;
//}
//
// es gibt aktuell keinen Datentyp (uint64) welcher dies aufnehmen könnte
//_int32 sigclib_timezone_ticks64_us(_uint64 *pdst64)
//{
//  if(CilDateTime_Usage() == true)
//  {
//	  CilDateTime->GetTickUS64(pdst64);
//    return TIMEZONE_ERR_NO;
//  }
//  *pdst64 = 0;
//  return TIMEZONE_ERR_INTERFACE;
//}

