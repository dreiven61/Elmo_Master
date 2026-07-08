// +----------------------------------------------------------------------------------------------+
// +-[   copyright ] Sigmatek GmbH & CoKG                                                         |
// +-[      author ] kolott                                                                       |
// +-[        date ] 10.07.2017                                                                   |
// +-[ description ]------------------------------------------------------------------------------+
// |                                                                                              |
// | cPipe_CTor():                                                                                |
// |   Entspricht einem FiFo, arbeitet taskübergreifend und funktioniert ohne Semaphor (Mutex)    |
// |   Es dürfen von verschiedenen Taskprioritäten Records eingetragen und auch ausgelesen werden.|
// |   NOTE:                                                                                      |
// |     Optimale Anzahl an Records sind: 2, 4, 8, 16, 32, 46, 128, 256...                        |
// |     maximale Anzahl an Records ist: 65536                                                    |
// |                                                                                              |
// | sigclib_actdata_cTor():                                                                      |
// |   Dieser Puffer stellt immer den aktuellsten Datarecord (falls vorhanden) zur Verfügung,     |
// |   ist threadsafe und arbeitet ohne Semaphor (Mutex)                                          |
// |                                                                                              |
// +----------------------------------------------------------------------------------------------+

#ifndef _cPipeH
 #define _cPipeH
  
 #include "DefineCompiler.h" 
  
 #ifdef cCompile // *******************************************************************************

  #pragma pack(push, 1)
   typedef struct
   {
     unsigned long rd;          // index readposition
     unsigned long wr;          // index writeposition
     unsigned long rd_lock;     // rd_lock
     unsigned long record_no;   // number of record in buffer
     unsigned long record_size; // bytesize of single record in buffer
     unsigned long record_user; // bytesize of userdata in buffer
     unsigned long bitpattern;  // bitpattern used for %
     unsigned char *data;       // pointer to recordbuffer
   }
   cPipe;
  #pragma pack(pop)
  
  cExtern cPipe        *cPipe_CTor(unsigned long record_size, unsigned long record_no);
  cExtern cPipe        *cPipe_DTor(cPipe *p);
  cExtern unsigned long cPipe_Init(cPipe *p, unsigned long record_size, unsigned long record_no);
  cExtern void          cPipe_Free(cPipe *p);
  cExtern unsigned long cPipe_Add(cPipe *p, void *pdata, unsigned long datasize);
  cExtern unsigned long cPipe_Get(void *pd, cPipe *p);
  cExtern unsigned long cPipe_GetUsed(cPipe *p);
  cExtern unsigned long cPipe_GetUnUsed(cPipe *p);
  
  cExtern void         *sigclib_actdata_cTor(_uint32 recordsize);
  cExtern void         *sigclib_actdata_dTor(void *phdl);
  cExtern _uint32       sigclib_actdata_add(void *phdl, void *pdata, _uint32 bytesize);
  cExtern void         *sigclib_actdata_get(void *phdl, _uint32 *pbytesize);
  cExtern void          sigclib_actdata_skip(void *phdl, void *pdata);
  
 #else // cCompile ********************************************************************************
 
  function global __cdecl cPipe_CTor var_input record_size:udint; record_no:udint; end_var var_output retcode:^void; end_var;
  function global __cdecl cPipe_DTor var_input p:^void; end_var var_output retcode:^void; end_var;
  function global __cdecl cPipe_Add var_input p:^void; pdata:^void; datasize:udint; end_var var_output retcode:udint; end_var;
  function global __cdecl cPipe_Get var_input pd:^void; p:^void; end_var var_output retcode:udint; end_var;
  function global __cdecl cPipe_GetUsed var_input p:^void; end_var var_output retcode:udint; end_var;
  function global __cdecl cPipe_GetUnUsed var_input p:^void; end_var var_output retcode:udint; end_var;
  
  function global __cdecl sigclib_actdata_cTor var_input recordsize:udint; end_var var_output retcode:^void; end_var;
  function global __cdecl sigclib_actdata_dTor var_input phdl:^void; end_var var_output retcode:^void; end_var;
  function global __cdecl sigclib_actdata_add var_input phdl:^void; pdata:^void; bytesize:udint; end_var var_output retcode:udint; end_var;
  function global __cdecl sigclib_actdata_get var_input phdl:^void; pbytesize:^udint; end_var var_output retcode:^void; end_var;
  function global __cdecl sigclib_actdata_skip var_input phdl:^void; pdata:^void; end_var;
  
 #endif // cCompile *******************************************************************************
 
#endif

// ------------------------------------------------------------------------------------------------
// cPipe *cPipe_CTor(unsigned long record_size, unsigned long record_no);
// Constructor used to construct cPipe
// --> record_size ..... bytesize of single record
// --> record_no ....... number of records in buffer
// <-- Function will return valid Pointer to pipe or NULL on error

// ------------------------------------------------------------------------------------------------
// cPipe *cPipe_DTor(cPipe *p);
// Destructor of cPipe (use only when constructor is in use)
// --> p ............... valid cPipe pointer 
// <-- Function will always return NULL

// ------------------------------------------------------------------------------------------------
// unsigned long cPipe_Init(cPipe *p, unsigned long record_size, unsigned long record_no);
// initialize cPipe. NOTE: Do not use when CTor is in use
// --> p ............... valid cPipe pointer 
// --> record_size ..... bytesize of single record
// --> record_no ....... number of records in buffer
// <-- Function will return 0 when error occured.

// ------------------------------------------------------------------------------------------------
// void cPipe_Free(cPipe *p);
// free cPipe. NOTE: Do not use when CTor is in use.
// --> p ............... valid cPipe pointer 

// ------------------------------------------------------------------------------------------------
// unsigned long cPipe_Add(cPipe *p, void *pdata, unsigned long datasize);
// function is used to add new record. 
// --> p ............... valid cPipe pointer 
// --> pdata ........... pointer to userdata
// --> datasize ........ bytesize of userdata
// <-- Function will return 1 on success or 0 on error

// ------------------------------------------------------------------------------------------------
// unsigned long cPipe_Get(void *pd, cPipe *p);
// function is used to get record from Pipe.
// --> pd .............. address wher recorddata should be filed 
// --> p -.............. valid cPipe pointer 
// <-- Function will return 1 on success (got valid recorddata) on the other hand 0

// ------------------------------------------------------------------------------------------------
// unsigned long cPipe_GetUsed(cPipe *p);
// get number of used records in buffer
// --> p ............... valid cPipe pointer 
// <-- Function will return number of used records in buffer

// ------------------------------------------------------------------------------------------------
// unsigned long cPipe_GetUnUsed(cPipe *p);
// get number of unused records in buffer
// --> p ............... valid cPipe pointer 
// <-- Function will return number of unused records in buffer

// ------------------------------------------------------------------------------------------------
// void *sigclib_actdata_cTor(_uint32 recordsize);
// Function is used to create threadsafe databuffer to get out always the latest valid datarecord
// --> recordsize ...... maximum number of bytes to store in single record
// <-- function will return valid handle or NULL on error
// NOTE: Functionality is done by usage of atomic operations instead of using mutex.

// ------------------------------------------------------------------------------------------------
//  void *sigclib_actdata_dTor(void *phdl);
// Function is used to delete databuffer created by sigclib_actdata_cTor()
// --> phdl ............ valid handle
// <-- function will atways return NULL

// ------------------------------------------------------------------------------------------------
//  _uint32 sigclib_actdata_add(void *phdl, void *pdata, _uint32 bytesize);
// function is used to set userdata into recordbuffer
// --> phdl ............ valid handle
// --> pdata ........... pointer to userdata
// --> bytesize ........ bytesize of data to add
// <-- Function will return 1 on success on the other hand 0 (error)

// ------------------------------------------------------------------------------------------------
// void *sigclib_actdata_get(void *phdl, _uint32 *pbytesize);
// function is used to get last given userdata from recordbuffer
// --> phdl ............ valid handle
// --> pbytesize ....... address where bytesize of returned userdata will be filed
// <-- function will return pointer to userdata or NULL if no data will be found
// NOTE: Buffer will just return the newest userdata (last added)

// ------------------------------------------------------------------------------------------------
// void sigclib_actdata_skip(void *phdl, void *pdata);
// Function is used to free (skip) userdata in databuffer. it is a must to call it after usage.
// --> phdl ............ valid handle
// --> pdata ........... pointer to userdata given by function sigclib_actdata_get()

