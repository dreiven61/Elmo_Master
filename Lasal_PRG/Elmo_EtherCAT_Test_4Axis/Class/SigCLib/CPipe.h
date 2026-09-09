// +----------------------------------------------------------------------------------------------+
// +-[   copyright ] Sigmatek GmbH & CoKG                                                         |
// +-[      author ] kolott                                                                       |
// +-[        date ] 10.07.2017, revised 01.06.2026                                               |
// +-[ description ]------------------------------------------------------------------------------+
// |                                                                                              |
// | sigclib_queue:                                                                               |
// |   The most common way to transport data of arbitrary byte length between different           |
// |   threads/tasks. The maximum number of intern used records is 131072 (0x20000).              |
// |   Functionality is given by using atomic functions without usage of semaphores.              |
// |   Info: Functionality will act like a FiFo, that means first in and first out.               |
// |         If all intern records are occupied, user can't add some more.                        |
// |                                                                                              |
// | cPipe:                                                                                       |
// |   Based on sigclib_queue and is used to transport data of defined byte length                |
// |   between different threads/tasks.                                                           |
// |                                                                                              |
// | sigclib_actdata:                                                                             |
// |   Thread-safe buffer which will always serve the last valid data-record.                     |
// |   Functionality is given by using atomic functions without usage of semaphores.              |
// |                                                                                              |
// +----------------------------------------------------------------------------------------------+

#ifndef _cPipeH
 #define _cPipeH
  
 #include "DefineCompiler.h" 
  
 #ifdef cCompile // *******************************************************************************

  cExtern void    *sigclib_queue_cTor(_uint32 record_size, _uint32 record_no);
  cExtern void    *sigclib_queue_dTor(void *phdl);
  cExtern _uint32  sigclib_queue_add(void *phdl, void *pdata, _uint32 bytesize);
  cExtern _uint32  sigclib_queue_get_copy(void *phdl, void *pdata, _uint32 bytesize);
  cExtern void    *sigclib_queue_get(void *phdl, _uint32 *pbytesize);
  cExtern void    *sigclib_queue_skip(void *phdl, void *pdata);
  cExtern void     sigclib_queue_lock_push(void *phdl);
  cExtern void     sigclib_queue_lock_pop(void *phdl);
  cExtern _uint32  sigclib_queue_free(void *phdl);
  cExtern _uint32  sigclib_queue_used(void *phdl);
    
  cExtern void    *cPipe_CTor(_uint32 record_size, _uint32 record_no);
  cExtern void    *cPipe_DTor(void *phdl);
  cExtern _uint32  cPipe_Add(void *phdl, void *pdata, _uint32 datasize);
  cExtern _uint32  cPipe_Get(void *pd, void *phdl);
  cExtern _uint32  cPipe_GetUsed(void *phdl);
  cExtern _uint32  cPipe_GetUnUsed(void *phdl);
  
  cExtern void    *sigclib_actdata_cTor(_uint32 recordsize);
  cExtern void    *sigclib_actdata_dTor(void *phdl);
  cExtern _uint32  sigclib_actdata_add(void *phdl, void *pdata, _uint32 bytesize);
  cExtern void    *sigclib_actdata_get(void *phdl, _uint32 *pbytesize);
  cExtern void     sigclib_actdata_skip(void *phdl, void *pdata);
  
 #else // cCompile ********************************************************************************

  function global __cdecl sigclib_queue_cTor var_input record_size:udint; record_no:udint; end_var var_output retcode:^void; end_var;
  function global __cdecl sigclib_queue_dTor var_input phdl:^void; end_var var_output retcode:^void; end_var;
  function global __cdecl sigclib_queue_add var_input phdl:^void; pdata:^void; bytesize:udint; end_var var_output retcode:udint; end_var;
  function global __cdecl sigclib_queue_get_copy var_input phdl:^void; pdata:^void; bytesize:udint; end_var var_output retcode:udint; end_var;
  function global __cdecl sigclib_queue_get var_input phdl:^void; pbytesize:^udint; end_var var_output retcode:^void; end_var;
  function global __cdecl sigclib_queue_skip var_input phdl:^void; pdata:^void; end_var var_output retcode:^void; end_var;
  function global __cdecl sigclib_queue_lock_push var_input phdl:^void; end_var;
  function global __cdecl sigclib_queue_lock_pop var_input phdl:^void; end_var;
  function global __cdecl sigclib_queue_free var_input phdl:^void; end_var var_output retcode:udint; end_var;
  function global __cdecl sigclib_queue_used var_input phdl:^void; end_var var_output retcode:udint; end_var;
  
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
// ------------------------------------------------------------------------------------------------
// Queue, sigclib_queue
// This is the most common way to transport data between different tasks/threads
// User is able to put data of arbitrary byte size into and get them out of thread-safe buffer
// ------------------------------------------------------------------------------------------------
// ------------------------------------------------------------------------------------------------

// ------------------------------------------------------------------------------------------------
// void *sigclib_queue_cTor(_uint32 record_no, _uint32 record_size);
// This function is used to create a thread-safe databuffer of arbitrary size
// --> record_no ....... number of records in databuffer
// --> record_size ..... estimated byte size (used case) of data in single record
// function will return a valid pointer to thread-safe buffer or NULL
  
// ------------------------------------------------------------------------------------------------
// void *sigclib_queue_dTor(void *phdl);
// This function is used to destroy already created thread-safe buffer
// --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor
// Function will return NULL on success
  
// ------------------------------------------------------------------------------------------------
// _uint32 sigclib_queue_add(void *phdl, void *pdata, _uint32 bytesize);
// Function is used to add arbitrary userdata to already created thread-safe buffer
// --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor
// --> pdata ........... userdata to add
// --> bytesize ........ byte size of userdata to add
// Function will return <>0 on success or 0 if userdata not added
// Note: Buffer is able to deal with byte size bigger than given in sigclib_queue_cTor().
//       In this case memory will be allocated internally.
 
// ------------------------------------------------------------------------------------------------
// _uint32 sigclib_queue_get_copy(void *phdl, void *pdata, _uint32 bytesize)
// Use this function to get a copy of recorded data from thread-safe buffer.
// --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
// --> pdata ........... destination where copy of userdata should be filed
// --> bytesize ........ max. byte size of destination
// Function will return number of copied bytes if record was present, on the other hand 0
// Note: Unlike the sigclib_queue_get() function, do not call the sigclib_queue_skip() function afterwards.
 
// ------------------------------------------------------------------------------------------------
// void *sigclib_queue_get(void *phdl, _uint32 *pbytesize);
// Use this function to get Data from thread-safe buffer
// --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor
// --> pbytesize ....... address where byte size of userdata should be filed, or NULL if not needed
// Function will return pointer to userdata or NULL if none are present
// Note: User must call function sigclib_queue_skip() after usage of data to skip record. 
//       Otherwise the thread-safe buffer will be stuffed over time.
  
// ------------------------------------------------------------------------------------------------
// void *sigclib_queue_skip(void *phdl, void *pdata);
// Function is used to skip (free) record in thread-safe buffer
// --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor
// --> pdata ........... pointer to userdata given by function sigclib_queue_get()
// Function will return NULL

// ------------------------------------------------------------------------------------------------
// void sigclib_queue_lock_push(void *phdl);
// The function is used to lock thread-safe-buffer. While buffer is locked user is not able to add or get records.
// Use function sigclib_queue_lock_pop() to unlock buffer afterwards.
// --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
// Note: Initially, the buffer is not blocked. If the buffer is blocked by user multiple times, this blockage must also be 
//       resolved multiple times by using function sigclib_queue_lock_pop().

// ------------------------------------------------------------------------------------------------
// void sigclib_queue_lock_pop(void *phdl);
// This function is used to unlock a already locked thread-safe-buffer.
// Use function sigclib_queue_lock_pop() to unlock buffer afterwards.
// --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()

// ------------------------------------------------------------------------------------------------
// _uint32 sigclib_queue_free(void *phdl);
// This function is used to empty thread-safe buffer
// --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
// Function will return number of recent occupied records

// ------------------------------------------------------------------------------------------------
// _uint32  sigclibe_queue_used(void *phdl);
// Function is used to get actual number of occupied records in thread-safe buffer
// --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor
// Function will return number of occupied records in thread-safe buffer 

// ------------------------------------------------------------------------------------------------
// ------------------------------------------------------------------------------------------------
// cPipe
// This functionality is based on sigclib_queue and is used to transport data between different tasks/threads
// ------------------------------------------------------------------------------------------------
// ------------------------------------------------------------------------------------------------

// ------------------------------------------------------------------------------------------------
// void *cPipe_CTor(_uint32 record_size, unsigned _uint32 record_no);
// Constructor used to construct cPipe
// --> record_size ....... byte size of single record
// --> record_no ......... number of records in buffer
// Function will return valid Pointer to cPipe or NULL on error

// ------------------------------------------------------------------------------------------------
// void *cPipe_DTor(void *phdl);
// Destructor of cPipe
// --> phdl .............. valid cPipe pointer 
// Function will return NULL

// ------------------------------------------------------------------------------------------------
// _uint32 cPipe_Add(void *phdl, void *pdata, _uint32 datasize);
// function is used to add userdata into cPipe.
// --> phdl .............. valid cPipe pointer 
// --> pdata ............. pointer to userdata
// --> datasize .......... byte size of userdata
// Function will return 1 on success or 0 on error

// ------------------------------------------------------------------------------------------------
// _uint32 cPipe_Get(void *pd, void *phdl);
// function is used to get record from Pipe.
// --> pd ................ address wher recorddata should be filed 
// --> phdl .............. valid cPipe pointer 
// Function will return byte size of data on sucess or 0 if no datarecords available

// ------------------------------------------------------------------------------------------------
// _uint32 cPipe_GetUsed(void *phdl);
// Function is used to get actual number of occupied records in buffer
// --> phdl .............. valid cPipe pointer 
// Function will return number of occupied records in buffer 

// ------------------------------------------------------------------------------------------------
// _uint32 cPipe_GetUnUsed(void *phdl);
// get number of unused records in buffer
// --> phdl .............. valid cPipe pointer 
// Function will return number of actual unused records in buffer

// ------------------------------------------------------------------------------------------------
// void *sigclib_actdata_cTor(_uint32 recordsize);
// Function is used to create threadsafe databuffer to get out always the latest valid datarecord
// --> recordsize ...... maximum number of bytes to store in single record
// Function will return valid handle or NULL on error
// NOTE: Functionality is done by usage of atomic operations instead of using mutex.

// ------------------------------------------------------------------------------------------------
//  void *sigclib_actdata_dTor(void *phdl);
// Function is used to delete databuffer created by sigclib_actdata_cTor()
// --> phdl ............ valid handle
// Function will atways return NULL

// ------------------------------------------------------------------------------------------------
//  _uint32 sigclib_actdata_add(void *phdl, void *pdata, _uint32 bytesize);
// function is used to set userdata into recordbuffer
// --> phdl ............ valid handle
// --> pdata ........... pointer to userdata
// --> bytesize ........ byte size of data to add
// Function will return 1 on success on the other hand 0 (error)

// ------------------------------------------------------------------------------------------------
// void *sigclib_actdata_get(void *phdl, _uint32 *pbytesize);
// function is used to get last given userdata from recordbuffer
// --> phdl ............ valid handle
// --> pbytesize ....... address where byte size of returned userdata will be filed
// Function will return pointer to userdata or NULL if no data will be found
// NOTE: Buffer will just return the newest userdata (last added)

// ------------------------------------------------------------------------------------------------
// void sigclib_actdata_skip(void *phdl, void *pdata);
// Function is used to free (skip) userdata in databuffer. it is a must to call it after usage.
// --> phdl ............ valid handle
// --> pdata ........... pointer to userdata given by function sigclib_actdata_get()

