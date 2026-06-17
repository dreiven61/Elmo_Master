// +----------------------------------------------------------------------------------------------+
// +-[   copyright ] Sigmatek GmbH & CoKG                                                         |
// +-[      author ] kolott                                                                       |
// +-[        date ] 04.09.2014                                                                   |
// +-[ description ]------------------------------------------------------------------------------+
// |                                                                                              |
// |                                                                                              |
// +----------------------------------------------------------------------------------------------+

#include "SigCLib.h"

#define SA75187 // avoid GCC compilerwarning when using option "-O2"

#pragma pack (push, 1)

typedef struct 
{ 
  unsigned long  interface;
  unsigned short address;
  void*          handle;
  unsigned long  ipaddress;
  unsigned long  port;
  unsigned long  free[9];
}
_sit_comdef;

//typedef struct
//{
//	_UINT uiCmd;
//	DINT aPara[MAXCMDPARA-1];
//}CmdStruct;

typedef struct
{
  unsigned short lng;
  unsigned char  data[254];
}
_sit_results;

typedef enum
{
  iprREADY,		// interpreter command processed
  iprERROR,		// i.e. wrong parameters
  iprERROR_BUSY,	// i.e. stopping movement
  iprBUSY,			// interpreter command processing
  iprQUIT,
  iprSTOP_BP,		// interpreter is on an Breakpoint
  iprSINGLESTEP	// interpreter is in Singlestep
}
_sit_iprStates;

#ifdef _TARGETARCH_ARM
	#pragma pack (push, 4)
#endif
typedef struct 
{ 
  unsigned long  lasalid;
  unsigned short channel;
  unsigned short varpos;
  unsigned short xtime;
  unsigned long  userid;
}
_sit_comregdata;
#ifdef _TARGETARCH_ARM
 #pragma pack (pop)
#endif

typedef void (*RefListCallBack) (unsigned long userid, unsigned long value);


#pragma pack(pop)


cExtern unsigned short LOGIN(void*);
cExtern unsigned short LOGOUT(void*); 
cExtern _sit_iprStates TXCOMMANDEX(unsigned long command, unsigned long length, unsigned char *data, _sit_comdef *pcomdef, _sit_results *presu, unsigned long ressize, unsigned short *preason);
cExtern void           INSTALLCALLBACK(void*);
cExtern unsigned short TXUPD(_sit_comregdata *pcell, _sit_comdef *pcomdef);
cExtern void           StartStopRefresh(_sit_comdef *pcomdef, unsigned short count, unsigned short channel);



#define I_GET_OBJ            0  // name -> handle
#define I_GET_OBJ_CLS        1  // name -> handle and class name
#define I_READ_CLASS         2  // index -> class info
#define I_WRITE              3  // write to server
#define I_WRITE_TO_CLNT      4  // write to the server connected to a client
#define I_READ_OBJECT        5  // class header, index -> object info
#define I_READ_CHANNEL       6  // read the channel
#define I_READ_METHOD        7  // class header, index -> method info
#define I_READ               8  // handle -> read server channel
#define I_STARTPROG          9  // program no, label no -> start parallel
#define I_RUNPROG           10  // program no, label no -> start sequential
#define I_CMD               11  // handle, cmd, paras -> execute method
#define I_DELAY             12  // time -> wait till time elapsed
#define I_NOP               13  // ==== not used
#define I_LBL               14  // ==== not used
#define I_GOTO              15  // goto label
#define I_ENDPROG           16  // terminates the current ipr program
#define I_CALL              17  // subroutine call
#define I_RET               18  // return from subroutine
#define I_FUNCTION          19  // ======= not used
#define I_ENDFUNCTION       20  // ======= not used
#define I_INC               21  // handle -> increments data image
#define I_DEC               22  // handle -> decrements data image
#define I_START_LOAD_PR     23  // prepare OPsystem for new program to load
#define I_LOAD_PROG         24  // load new program code
#define I_COMMENT           25  // line is not executed
#define I_JMPIF             26  // jump if condition is true
#define I_WAITFOR           27  // wait until condition is true
#define I_SETFORTIME        28  // ======= not used
#define I_GETPROG           29  // load program code from the PLC
#define I_GETPROGSTATE      30  // program no -> state
#define I_CHECK_FOR_LOAD    31  // check if new progs are to load
#define I_GET_DESC_CRC      32  // CRC of all LASAL-descriptor lists in the PLC
#define I_READ_CONNECT      33  // get the client connection
#define I_GET_CLS           34  // get the class name from an object's address
#define I_GET_OBJ_NAME      35  // get object name from class name and object addr
#define I_READ_CLT          36  // call read method of connected server
#define I_STOPPROG          37  // stops a interpreter program
#define I_GET_PROGNR        38  // get programnumber of programname
#define I_GET_ACT_OFFSET    39  // get the actual offset of the program
#define I_GET_TRIGGER_COUNT 40  // get the count of the trigger
#define I_GET_VERSION       41  // returns the loader version
#define I_NEW_OBJ           42  // get the programname of the programnumber
#define I_CONNECT           43
#define I_SET_CYCLE         44
#define I_DEL               45
#define I_STOP_IPR_CHEK_FOR_LOAD 46 //stop all interpreter and checkforload
#define I_REGISTER   		    47	// for login and to inform the PLC that the channel is still used
#define I_RELEASE			      48	// release the communication channel at end of debug session
#define I_TRY_SOFTLOAD_IPR	49	// Try to load an program from the temporary memory in a soft mode
#define I_END_SOFTLOAD_IPR	50	// End the softload
#define I_GET_CFL_CYCLE 	  51	// Get the Cycle time of the CheckForLoad
#define I_GET_CALLED_IPR	  52	// Liefert die Nummer des Interpreters der von diesem Ipr aufgerufen wurde
#define I_GET_CALLED_FROM	  53	// Bringt die Nummer des Iprs von dem dieser Ipr aufgerufen wurde
#define I_INIT				      54	// calls the init-method of a server or an object
#define I_GET_CLS_BY_NAME	  55	// gets a class header pointer for a given class name
#define I_GET_NXT_DERIVED	  56	// get next derived class of a base class
#define I_LOCK              57  // lock communication buffer
#define I_UNLOCK            58  // unlock communcation buffer
#define I_LSLCMD            59
#define I_CMD_DEBUGIP       60  // gleich wie I_CMD, nur wird für die Serveradresse der this-Pointer (DebugIp Objekt) verwendet
#define I_CREATE_PROG       61  // ++pr:test
#define I_LOAD_PROG2        62  // wie I_LOAD_PROG jedoch ohne den Fehler mit der falschen Länge bei der Checksummenberechnung
#define I_START_IPR         63  // ++pr:test
#define I_GET_GLOBAL_ADDR   64
#define I_GET_DATA          65
#define I_GETPROGSTATE_ALL  66  // program state
#define I_SET_DATA          67
#define I_GET_STACKINFO     68
#define I_GET_OBJ_LIST      69
#define I_VISU_RDY          70
#define I_ILLEGAL           71


#define P_IMMED        0 // data source is a constant
#define P_VARIA        1 // data source is a channel
#define P_SYS          2 // data source are system variables
#define P_POPEN        3 // open parenthesis
#define P_PCLOSE       4 // close parenthesis
#define P_ADD          5 // addition
#define P_SUB          6 // subtraction
#define P_COMMA        7 //
#define P_EOL          8 // end of line
#define P_EQ           9 // comparisons
#define P_NEQ         10
#define P_GT          11
#define P_GEQ         12
#define P_LT          13
#define P_LEQ         14
#define P_NOT         15 // binary negation
#define P_AND         16
#define P_OR          17
#define P_XOR         18
#define P_USER_STREAM 19
#define P_MUL         20
#define P_DIV         21
#define P_ILLEGAL     22

#define COMLINK_OK                          0x0000 // no error
#define COMLINK_IPR                         0x0001 // the status code comes from the interpreter
#define COMLINK_ERR_MAXENTRIES              0xFFEF // Maximum number of entries in list reached
#define COMLINK_ERR_GENERAL                 0xFFF0 // general unrecoverable error, without information of the cause.
#define COMLINK_ERR_TIMEOUT                 0xFFF1 // timeout while waiting for a response
#define COMLINK_ERR_BUF_TOO_SMALL           0xFFF2 // the size of the specified buffer is too small
#define COMLINK_ERR_WOULDBLOCK              0xFFF3 // the function failed because a call would block (i.e. another task is in this function)
#define COMLINK_ERR_LOGIN_REQUIRED          0xFFF8 // a login is required
#define COMLINK_ERR_INVALID_RESPONSE        0xFFF9 // server sent an invalid response
#define COMLINK_ERR_ADDRINUSE               0xFFFA // address is already in use
#define COMLINK_ERR_CLIENT_NOTREADY         0xFFFB // the client interface functions are not available
#define COMLINK_ERR_UNSUPPORTED_INTERFACE   0xFFFC // the specified interface type is not supported
#define COMLINK_ERR_MAXCONN                 0xFFFD // maximum number of connections exceeded
#define COMLINK_ERR_NOCONNECTION            0xFFFE // no connection to the server
#define COMLINK_ERR_INVALID_PARAM           0xFFFF // invalid parameter specified

#define ACCESS_DENIED   0x80000010

typedef struct
{
  _sit_comdef   comdef;
  unsigned long cell_no[2];     // Anzahl der Einträge in refreshlist
  _uint32       siv_resultidx;  // vorsicht, nur atomic access!
  _sit_results  siv_result[16]; // Anzahl nicht ändern
}
_sit_login;

static RefListCallBack _siv_callback = NULL;

static void init_sit_login(_sit_login *p)
{
  sigclib_memset(p, 0, sizeof(_sit_login));
}

static void free_sit_login(_sit_login *p)
{
  init_sit_login(p);
}

cExtern void sigclib_irq_PLC(_sit_comdef *pcomdef, unsigned char *pdata)
{
  unsigned long userid = *(unsigned long*)&pdata[0];
//  unsigned long cellidx    = *(unsigned short*)&pdata[4];
  unsigned long value  = *(unsigned long*)&pdata[6];
  
  if(_siv_callback != NULL)
  {
    _siv_callback(userid, value);
  }
}

static void sigclib_installcallback_intern(_sit_login *pol)
{
  typedef struct 
  { 
    void *callbackfkt;
    void *handle;
    void *pthis;
  }
  _sit_scallback;
  _sit_scallback scb;
  
  scb.callbackfkt = (void*)sigclib_irq_PLC;
  scb.handle      = (void*)pol->comdef.handle;
  scb.pthis       = NULL;
  INSTALLCALLBACK(&scb);
}

void *sigclib_plc_Login(unsigned long interface, unsigned long address)
{
  _sit_login *pol = NULL;
  
  if(sigclib_memory((void**)&pol, sizeof(_sit_login)) != 0)
  {
    init_sit_login(pol);
    
    pol->comdef.interface = interface;
    pol->comdef.address   = (unsigned short)address;
    pol->comdef.ipaddress = address;
    
    if(LOGIN(&pol->comdef) == COMLINK_OK)
    {
      sigclib_installcallback_intern(pol);
      return pol;
    }
    
    free_sit_login(pol);
    sigclib_memory((void**)&pol, 0);
  }
  
  return NULL;
}

void sigclib_plc_Logout(void *hdl)
{
  _sit_login *pol = (_sit_login*)hdl; 

  if(pol != NULL)
  {
    LOGOUT(&pol->comdef);
    
    free_sit_login(pol);
    sigclib_memory((void**)&pol, 0);
  }
}

void sigclib_plc_ReflistInstallCallback(void* callback)
{
  _siv_callback = (RefListCallBack)callback;
}

unsigned long sigclib_plc_ReflistAddSvr(void *hdl, unsigned long lasalid, unsigned long userid, unsigned long time_ms, unsigned long channel)
{
  _sit_login *pol = (_sit_login*)hdl; 

  if((pol != NULL) && (lasalid != 0) && (channel < 2))
  {
    _sit_comregdata cell;
    cell.lasalid = lasalid;
    cell.channel = (unsigned short)channel;
    cell.varpos  = pol->cell_no[channel];
    
    if(channel == 0)
    {
      if(cell.varpos > 999)
      {
        return 0; // kein platz mehr in statischer liste
      }
    }
    else
    {
      cell.varpos += 1000; // dyn.list immer index + 1000
    }
    cell.xtime   = time_ms;
    cell.userid  = userid;
    unsigned long err = TXUPD(&cell, &pol->comdef);
    
    if((err == COMLINK_OK) || (err == COMLINK_ERR_GENERAL)) // blöd: aber es kann sein dass COMLINK_ERR_GENERAL zurückgeliefert wird obwohl alles ok
    {
      pol->cell_no[channel] ++; // anzahl der reflisteinträge erhöhen
      return 1;
    }
  }
  return 0;
}

unsigned long sigclib_plc_ReflistAddVar(void *hdl, unsigned long lasalid, unsigned long userid, unsigned long time_ms, unsigned long channel)
{
  return sigclib_plc_ReflistAddSvr(hdl, lasalid, userid, time_ms | 0x4000, channel);
}

unsigned long sigclib_plc_ReflistAddStr(void *hdl, unsigned long lasalid, unsigned long userid, unsigned long time_ms, unsigned long channel)
{
  return sigclib_plc_ReflistAddSvr(hdl, lasalid, userid, time_ms | 0x8000, channel);
}

unsigned long sigclib_plc_ReflistStart(void *hdl, unsigned long channel)
{
  _sit_login *pol = (_sit_login*)hdl; 

  if((pol != NULL) && (channel < 2))
  {
    StartStopRefresh(&pol->comdef, pol->cell_no[channel], (unsigned short)channel);
    return 1;
  }
  return 0;
}

unsigned long sigclib_plc_ReflistStop(void *hdl, unsigned long channel)
{
  _sit_login *pol = (_sit_login*)hdl; 

  if((pol != NULL) && (channel < 2))
  {
    StartStopRefresh(&pol->comdef, 0, (unsigned short)channel);
    pol->cell_no[channel] = 0;
    return 1;
  }
  return 0;
}

unsigned long sigclib_plc_TxCommand(void *hdl, void **presult, unsigned long command, const unsigned char *pdata, unsigned long datalength)
{
  _sit_login *pol = (_sit_login*)hdl; 
 
  _sit_results *ptres = NULL;
  *presult = (void*)ptres;

  if(pol != NULL)
  {
    _uint32 idx = sigclib_atomic_incU32(&pol->siv_resultidx) & 0x0F;
    ptres = &pol->siv_result[idx];
    ptres->lng = 0;
    
    unsigned short reason;
    if(TXCOMMANDEX(command, datalength, (unsigned char*)pdata, &pol->comdef, ptres, sizeof(pol->siv_result[0]), &reason) == iprREADY)
    {
      *presult = (void*)ptres;
      return 1;
    }
  }
  return 0;
}

unsigned long sigclib_plc_GetDscCrc(void *hdl, unsigned long *pcrc)
{
  _sit_results *presu;
  
  if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_GET_DESC_CRC, NULL, 0) == 1)
  {
   #ifndef SA75187
    *pcrc = *(unsigned long*)&presu->data[0];
   #else 
    unsigned long tmp = sigclib_GetByteByByte32(&presu->data[0]);
    *pcrc = tmp;
   #endif
    return 1;
  }

  return 0;
}

unsigned long sigclib_plc_KeepAlive(void *hdl)
{
  _sit_results *presu;
  
  if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_GET_VERSION, NULL, 0) == 1)
  {
    if(presu->lng == 4)
    {
      return 1;
    }
  }
  
  return 0;
}

unsigned long sigclib_plc_GetLoaderVersion(void *hdl)
{
  unsigned long retcode = 0;
  _sit_results *presu;
  
  if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_GET_VERSION, NULL, 0) == 1)
  {
    if(presu->lng == 4)
    {
     #ifndef SA75187
      retcode = *(unsigned short*)presu->data;
     #else
      retcode = sigclib_GetByteByByte16(presu->data);
     #endif
    }
  }
  
  return retcode;
}

unsigned long sigclib_plc_GetLasalIdSvr(void *hdl, const char *label)
{
  _sit_results *presu;
  unsigned long len = sigclib_strlen(label);
  
  if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_GET_OBJ, (const unsigned char*)label, len + 1) == 1)
  {
   #ifndef SA75187
    return *(unsigned long*)&presu->data[0];
   #else
    unsigned long retcode = sigclib_GetByteByByte32(&presu->data[0]);
    return retcode;
   #endif 
  }
  
  return 0;
}

unsigned long sigclib_plc_GetLasalIdSvrEx(void *hdl, unsigned long *pid, unsigned char *pdata, unsigned long datasize, unsigned long no)
{
  // format:
  // [1] .. länge von token
  // [ .... token (objektname ohne abschliessende 0)
  // [1] .. länge von token
  // [ .... token (servername1 ohne abschliessende 0)
  // [1] .. länge von token
  // [ .... token (servername2 ohne abschliessende 0)
  // ...... 
  // [1] .. 0 (abschliessende 0)
        
  // send stream to plc

  _sit_results *presu;

  if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_GET_OBJ_LIST, (const unsigned char*)pdata, datasize) == 1)
  {
    unsigned long nox = (presu->lng - 2) / 6; // 6byte pro server
    if(nox == no)
    {
      unsigned char *ph = presu->data;
      while(nox--)
      {
       #ifndef SA75187 // -O3
        unsigned long id = *(unsigned long*)ph;
       #else
        unsigned long id = sigclib_GetByteByByte32(ph);
       #endif
        ph += 4;
        unsigned long channel = *(unsigned short*)ph;
        ph += 2;
        *pid++ = (channel < 3)? id : 0;
      }
      
      return 1;
    }
  }
  
  return 0;
}

unsigned long sigclib_plc_GetLasalIdVar(void *hdl, const char *label)
{
  _sit_results *presu;
  unsigned long len = sigclib_strlen(label);
  
  if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_GET_GLOBAL_ADDR, (const unsigned char*)label, len + 1) == 1)
  {
   #ifndef SA75187
    return *(unsigned long*)&presu->data[0];
   #else
    unsigned long retcode = sigclib_GetByteByByte32(&presu->data[0]);
    return retcode;
   #endif
  }
  
  return 0;
}

unsigned long sigclib_plc_CallReadMethod(void *hdl, unsigned long *pdst, unsigned long lasalid)
{
  if(lasalid != 0)
  {
    _sit_results *presu;
    unsigned char tmp[20];

		//[1] ... prefix P_VARIA
		//[4] ... lasalid
		//[1] ... prefix P_EOL
    
    tmp[0] = P_VARIA;
   #ifndef SA75187
    *(unsigned long*)&tmp[1] = lasalid;
   #else
    sigclib_SetByteByByte32(&tmp[1], lasalid);
   #endif
    tmp[5] = P_EOL;

    if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_READ, (const unsigned char*)tmp, 8) == 1)
    {
     #ifndef SA75187
      *pdst = *(unsigned long*)presu->data;
     #else
      unsigned long tmp32 = sigclib_GetByteByByte32(presu->data);
      *pdst = tmp32;
     #endif
      return 1;
    }
  }
  
  *pdst = 0;
  return 0;
}

unsigned long sigclib_plc_CallWriteMethod(void *hdl, unsigned long lasalid, unsigned long value)
{
  if(lasalid != 0)
  {
    _sit_results *presu;
    unsigned char tmp[20];
  
  	//[4] ... lasalid
		//[1] ... prefix P_IMMED
		//[4] ... value
		//[1] ... prefix P_EOL
    
   #ifndef SA75187
    *(unsigned long*)&tmp[0] = lasalid;
    tmp[4] = P_IMMED;
    *(unsigned long*)&tmp[5] = value;
    tmp[9] = P_EOL;
   #else
    sigclib_SetByteByByte32(&tmp[0], lasalid);
    tmp[4] = P_IMMED;
    sigclib_SetByteByByte32(&tmp[5], value);
    tmp[9] = P_EOL;
   #endif
  
    if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_WRITE, (const unsigned char*)tmp, 10) == 1)
    {
     #ifndef SA75187
      if(*(unsigned long*)&presu->data[0] == ACCESS_DENIED)
     #else
      unsigned long res = sigclib_GetByteByByte32(&presu->data[0]);
      if(res == ACCESS_DENIED)
     #endif
      {
        return 2; // ACCESS_DENIED
      }
      return 1; // success
    }
  }
  return 0; // error
}

void *sigclib_plc_CallNewInstMethod(void *hdl, unsigned long lasalid, unsigned short uicmd, void *para0, unsigned long para_no)
{
  if((lasalid != 0) && (para_no < 30))
  {
    unsigned long *para = (unsigned long*)para0;
    unsigned char tx[254];
    unsigned long length = 6;
    
    sigclib_SetByteByByte32(&tx[0], lasalid);
    sigclib_SetByteByByte16(&tx[4], uicmd);
    unsigned char *ph = &tx[6];
    while (para_no--)
    {
      ph[0] = P_IMMED;
      sigclib_SetByteByByte32(&ph[1], para[0]);
      ph += 5;
      length += 5;
      para++;
      if (para_no > 0)
      {
        ph[0] = P_COMMA;
        ph++;
        length += 1;
      }
    }
    ph[0] = P_EOL;
    length += 1;

    _sit_results *presu = NULL;
    if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_CMD, (const unsigned char*)tx, length) == 1)
    {
      return (void*)presu;
    }
  }
  
  return NULL;
}

unsigned long sigclib_plc_GetString(void *hdl, void *dst, unsigned long *pchrsize, unsigned long lasalid, unsigned long maxbytesize)
{
  // [4] .... lasalid
  // [2] .... 1
  // [1] .... P_EOL

  if(lasalid != 0)
  {
    _sit_results *presu;
    unsigned char tmp[32];

   #ifndef SA75187
    *(unsigned long*)&tmp[0]  = lasalid;
    *(unsigned short*)&tmp[4] = 1;
   #else
    sigclib_SetByteByByte32(&tmp[0], lasalid);
    tmp[4] = 1;
    tmp[5] = 0;
   #endif
    *(unsigned char*)&tmp[6]  = P_EOL;

    if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_CMD, (const unsigned char*)tmp, 7) == 1)
    {
     #ifndef SA75187
      unsigned long bytelength = *(unsigned long*)presu->data;
     #else
      unsigned long bytelength = sigclib_GetByteByByte32(presu->data);
     #endif
      unsigned char *psrc      = &presu->data[4];
      unsigned long chrsize    = 1; // annahme string ist im asciicode format
      
      if(bytelength > 1)
      {
        if(*(unsigned short*)psrc == 0x0200)
        {
          // string ist im unicode format
          chrsize     = 2;
          psrc       += 2;
          bytelength -= 2;
        }
      }
      
      if(maxbytesize >= chrsize)
      {
        maxbytesize -= chrsize; // maximale länge berichtigen damit final 0 platz findet
        
        if(bytelength > maxbytesize)
        {
          bytelength = maxbytesize; // datenlänge begrenzen weil in dst nicht so viel platz hat
        }
        
        unsigned char *pd = (unsigned char*)dst;
        unsigned long offset = 0 ;
        do
        {
          unsigned long partlen = (bytelength > 250)? 250 : bytelength;
          sigclib_memcpy(pd, psrc, partlen);
          pd         += partlen;
          bytelength -= partlen;
          offset     += partlen;
          pd[0]       = 0; // ascii-0-string
          if(chrsize == 2)
          {
            pd[1]     = 0; // ab jetzt uni-0-string
          }
          
          if(bytelength > 0)
          {
            // rest auch noch holen
           #ifndef SA75187
            *(unsigned long*)&tmp[0]  = lasalid;
            *(unsigned short*)&tmp[4] = 3;
            *(unsigned char*)&tmp[6]  = P_IMMED;
            *(unsigned long*)&tmp[7]  = offset;
            *(unsigned char*)&tmp[11] = P_EOL;
           #else
            sigclib_SetByteByByte32(&tmp[0], lasalid);
            *(unsigned char*)&tmp[4] = 3;
            *(unsigned char*)&tmp[5] = 0;
            *(unsigned char*)&tmp[6]  = P_IMMED;
            sigclib_SetByteByByte32(&tmp[7], offset);
            *(unsigned char*)&tmp[11] = P_EOL;
           #endif
           
            if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_CMD, (const unsigned char*)tmp, 12) != 1)
            {
              return 0; // fehler aufgetreten
            }
            psrc = &presu->data[0];
          }
        }
        while(bytelength != 0);
        
        *pchrsize = chrsize;
      
        return 1;
      }
    }
  }

  return 0;
}

unsigned long sigclib_plc_SetString(void *hdl, const void *src, unsigned long chrsize, unsigned long lasalid)
{
  // [4] .... lasalid
  // [2] .... 4
  // [1] .... P_USER_STREAM
  // [4] .... data bytelength
  // [4] .... data byteoffset

  if(lasalid != 0)
  {
    _sit_results *presu;
    unsigned short final;
    unsigned char tmp[256];
    unsigned long datalen, offset = 0;
    unsigned char *pd, *ps = (unsigned char*)src;

    if(src == NULL)
    {
      final = 0;
      src   = (const void*)&final;
    }
    
    if(chrsize > 1) // uni-0-string
    {
      datalen = sigclib_strlen16((const unsigned short*)src) + 1; // inclusive final 0
      datalen = datalen * 2;
      chrsize = 2;
    }
    else // ascii-0-string
    {
      datalen = sigclib_strlen((const char*)src) + 1; // inclusive final 0
      chrsize = 1;
    }
    
    do
    {
      unsigned long part = (datalen > 80)? 80 : datalen;
      
     #ifndef SA75187
      *(unsigned long*)&tmp[0]  = lasalid;
      *(unsigned short*)&tmp[4] = 4;
      *(unsigned char*)&tmp[6]  = P_USER_STREAM;
      *(unsigned long*)&tmp[7]  = part;
      *(unsigned long*)&tmp[11] = offset;
     #else
      sigclib_SetByteByByte32(&tmp[0], lasalid);
      *(unsigned char*)&tmp[4] = 4;
      *(unsigned char*)&tmp[5] = 0;
      *(unsigned char*)&tmp[6]  = P_USER_STREAM;
      sigclib_SetByteByByte32(&tmp[7], part);
      sigclib_SetByteByByte32(&tmp[11], offset);
     #endif
  
      pd = &tmp[15];
      if((offset == 0) && (chrsize == 2))
      {
        // unicode kennung eintragen
        *(unsigned char*)&tmp[15] = 0;
        *(unsigned char*)&tmp[16] = chrsize;
       #ifndef SA75187
        *(unsigned long*)&tmp[7] += 2;
       #else
        unsigned long tmp32 = sigclib_GetByteByByte32(&tmp[7]);
        tmp32 += 2;
        sigclib_SetByteByByte32(&tmp[7], tmp32);
       #endif
        pd     = &tmp[17];
        offset = 2;
      }
        
      sigclib_memcpy(pd, ps, part);
      ps += part;
      
      pd[part] = P_EOL;
     #ifndef SA75187
      unsigned long tmp32 = *(unsigned long*)&tmp[7];
     #else
      unsigned long tmp32 = sigclib_GetByteByByte32(&tmp[7]);
     #endif
     
      if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_CMD, (const unsigned char*)tmp, tmp32 + 16) != 1)
      {
        return 0; // fehler aufgetreten
      }
      
      offset += part;
      datalen -= part;
    }
    while(datalen > 0);
    
    return 1;
  }

  return 0;
}

unsigned long sigclib_plc_SetDataAt(void *hdl, void *src, unsigned long address, unsigned long bytelength)
{
  if(address != 0)
  {
    unsigned char tmp[256];
    _sit_results *presu;
    unsigned char *ps = (unsigned char*)src;
    
    while(bytelength != 0)
    {
      unsigned long partlen = (bytelength > 128)? 128 : bytelength;
    
     #ifndef SA75187
      *(unsigned long*)&tmp[0] = address;
     #else
      sigclib_SetByteByByte32(&tmp[0], address);
     #endif
      *(unsigned char*)&tmp[4] = partlen;
      sigclib_memcpy(&tmp[5], ps, partlen);
      
      if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_SET_DATA, (const unsigned char*)tmp, partlen+5) != 1)
      {
        return 0; // fehler aufgetreten
      }
      
      ps         += partlen;
      bytelength -= partlen;
      address    += partlen;
    }      
    return 1;
  }
  
  return 0;
}

unsigned long sigclib_plc_GetDataAt(void *hdl, void *dst, unsigned long address, unsigned long bytelength)
{
  if(address != 0)
  {
    unsigned char tmp[32];
    _sit_results *presu;
    unsigned char *pd = (unsigned char*)dst;
    
    while(bytelength != 0)
    {
      unsigned long partlen = (bytelength > 250)? 250 : bytelength;
    
     #ifndef SA75187
      *(unsigned long*)&tmp[0] = address;
      *(unsigned long*)&tmp[4] = partlen;
     #else
      sigclib_SetByteByByte32(&tmp[0], address);
      sigclib_SetByteByByte32(&tmp[4], partlen);
     #endif
     
      if(sigclib_plc_TxCommand(hdl, (void**)&presu, I_GET_DATA, (const unsigned char*)tmp, 8) != 1)
      {
        return 0; // fehler aufgetreten
      }
      
      sigclib_memcpy(pd, &presu->data[0], partlen);
      
      pd         += partlen;
      bytelength -= partlen;
      address    += partlen;
    }
    return 1;
    
  }
  
  return 0;
}


