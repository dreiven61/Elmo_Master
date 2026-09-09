//<NewSigmatekCFileOptimize/>
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

#include "SigCLib.h" 

typedef struct
{
  void   *ptr;        // pointer to userdata
  _uint32 state;      // atomic state of record
  _uint32 datasize;   // byte size of userdata in record
  _uint08 data[4];    // userdata, do not change size
} _tQueueRecord;

typedef struct        // note: structure keeps legacy to previous version
{
  _uint32 rd, wr;     // rd/wr index
  _uint32 id, no;     // identifier + number of records
  _uint32 recordsize; // byte size of each record
  _uint32 datasize;   // max. byte size of userdata in single record
  _uint32 bitpattern; // bittpattern used for indexing records
  _uint32 fill, fillmax; // actual load, max load
  _uint08 data[4];    // records
} _tQueue;

#define Q_Spacer                    4      // spacer
#define Q_Identifier                0xCAFEBA00 // low byte has to be 0 to ensure lock_push() and lock_pop()
#define QRst_FREE                   0x0000 // do not change value
#define QRst_BUSY_WR                0xBEBA // record is busy with write
#define QRst_BUSY_RD                0xABAE // record is busy with read
#define QRst_READY                  0xB055 // record is ready done and valid
#define intern_queue_lock_push(__p) sigclib_atomic_incU32(&((__p)->id))
#define intern_queue_lock_pop(__p)  sigclib_atomic_decU32(&((__p)->id))

inline _uint32 sigclib_queue_potenz2(_uint32 no, _uint32 ceiling)
{
  // The function returns the next higher or equal power of two.
  if(no > ceiling) { no = ceiling; }
  _uint32 potenz = 2;
  while(1)
  {
    if(no <= potenz) { return potenz; }
    potenz = potenz * 2;
  }
  
  return 2;
}

inline _tQueue *sigclib_queue_chkhdl_exact(void *phdl)
{
  // Function will check if given pointer is a valid handle.
  _tQueue *pq = (_tQueue *)phdl;
  if(pq != NULL)
  {
    if(pq->id == Q_Identifier)
    {
      return pq;
    }
  }
  return NULL;
}

inline _tQueue *sigclib_queue_chkhdl(void *phdl)
{
  // Function will check if given pointer is a valid handle.
  _tQueue *pq = (_tQueue *)phdl;
  if(pq != NULL)
  {
    if((pq->id & 0xFFFFFF00) == Q_Identifier)
    {
      return pq;
    }
  }
  return NULL;
}

static void sigclib_queue_free_record(_tQueueRecord *pr)
{
  // The function will free and release specified record.
  void *pd = pr->data;
  void *ph = pr->ptr;
  pr->ptr = NULL;
  pr->datasize = 0;
  sigclib_atomic_setU32(&pr->state, QRst_FREE); // release record
  if(ph != pd)
  {
    sigclib_free(ph); // free memory
  }
}

static _uint32 sigclib_queue_free_record_all(_tQueue *pq)
{
  _uint32 retcode = 0;
  _uint32 nox = pq->no;
  while(nox--) // iterate all records
  {
    _tQueueRecord *pr = (_tQueueRecord*)(&pq->data[nox * pq->recordsize]);
    if(sigclib_atomic_cmpxchgU32(&pr->state, QRst_READY, QRst_BUSY_RD) == QRst_READY)
    {
      sigclib_queue_free_record(pr);
      retcode += 1;
    }
  }
  
  return retcode;
}

void *sigclib_queue_cTor(_uint32 record_size, _uint32 record_no)
{
  // This function is used to create a thread-safe databuffer of arbitrary size.
  // Functionality is given without usage of semaphores and is as performant as possible.
  // --> record_no ....... number of records in databuffer
  // --> record_size ..... estimated byte size (used case) of data in single record
  // function will return a valid pointer to thread-safe buffer or NULL on error
  
  if((record_no > 0) && (record_size != 0))
  {
    if(record_no <= 16) { record_no += Q_Spacer; } // if user desires a buffer with few entries, all of them should be available.
    
    record_no = sigclib_queue_potenz2(record_no, 0x20000); // It must be a power of 2, otherwise 32-bit wrap of rd and wr indexes will not work.
  
    _uint32 rec_head = sizeof(_tQueueRecord) - 4; // byte size of recordheader
    _uint32 datasize = record_size + rec_head; // byte size of entire single record
    while(datasize & 3) { datasize ++; } // 32bit aligned
    record_size = datasize - rec_head;
  
    _tQueue *pq = (_tQueue*)sigclib_calloc((datasize * record_no) + sizeof(_tQueue), 1);
    if(pq != NULL)
    {
      pq->no = record_no;
      pq->fillmax = record_no - Q_Spacer; // care spacer, maximum fill level
      pq->datasize = record_size;
      pq->recordsize = datasize;
      pq->bitpattern = record_no - 1;
      pq->id = Q_Identifier;
      
      sigclib_atomic_setU32(&pq->rd, 0);
      sigclib_atomic_setU32(&pq->wr, 0);
      
      return (void*)pq;
    }
  }
  
  return NULL;
}

void *sigclib_queue_dTor(void *phdl)
{
  // This function is used to destroy already created thread-safe buffer
  // --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
  // Function will return NULL on success
  
  _tQueue *pq = sigclib_queue_chkhdl(phdl);
  if(pq != NULL)
  {
    intern_queue_lock_push(pq); // lock queue, access is therefore blocked
    sigclib_queue_free_record_all(pq); // iterate all records an free
    sigclib_free(pq);
    return NULL;
  }
  
  return phdl;
}

_uint32 sigclib_queue_add(void *phdl, void *pdata, _uint32 bytesize)
{
  // Function is used to add arbitrary userdata to already created thread-safe buffer
  // --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
  // --> pdata ........... userdata to add
  // --> bytesize ........ byte size of userdata to add
  // Function will return <>0 on success or 0 if userdata not added
  // Note: Buffer is able to deal with byte size bigger than given record_size in sigclib_queue_cTor().
  //       In this case memory will be allocated internally. There is no need for user to care this allocation.
  
  _tQueue *pq = sigclib_queue_chkhdl_exact(phdl);
  if(pq != NULL) // && (pdata != NULL) && (bytesize != 0)) // ensure 0 bytes in single record
  {
    void *pd = NULL;
    if(bytesize > pq->datasize)
    {
      pd = sigclib_malloc(bytesize);
      if(pd == NULL)
      {
        return 0;
      }
    }
    
    _uint32 nox = sigclib_atomic_getU32(&pq->fill);
    if(nox <= pq->fillmax) // spacer, necessary because the wr should not overtake rd index and the add function can be called by multiple tasks simultaneously
    {
      _uint32 idx = sigclib_atomic_incU32(&pq->wr);
      _tQueueRecord *pr = (_tQueueRecord*)&pq->data[(idx & pq->bitpattern) * pq->recordsize];
      _uint32 rst = sigclib_atomic_cmpxchgU32(&pr->state, QRst_FREE, QRst_BUSY_WR);
      if(rst == QRst_FREE)
      {
        sigclib_atomic_incU32(&pq->fill);
        pr->ptr = (pd != NULL)? pd : pr->data;
        sigclib_memcpy(pr->ptr, pdata, bytesize);
        pr->datasize = bytesize; //xigclib_atomic_setU32(&pr->datasize, bytesize);
        sigclib_atomic_setU32(&pr->state, QRst_READY); // set ready
        return 1;
      }
    }
    
    sigclib_free(pd);
  }
  
  return 0;
}

_uint32 sigclib_queue_get_copy(void *phdl, void *pdata, _uint32 bytesize)
{
  // Use this function to get a copy of recorded data from thread-safe buffer
  // --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
  // --> pdata ........... destination where copy of userdata should be filed
  // --> bytesize ........ max. byte size of destination
  // Function will return number of copied bytes if record was present, on the other hand 0

  _tQueue *pq = sigclib_queue_chkhdl_exact(phdl);
  if(pq != NULL)
  {
    _uint32 ird = sigclib_atomic_getU32(&pq->rd);
    _tQueueRecord *pr = (_tQueueRecord*)(&pq->data[(ird & pq->bitpattern) * pq->recordsize]);
    if(sigclib_atomic_cmpxchgU32(&pr->state, QRst_READY, QRst_BUSY_RD) == QRst_READY)
    {
      sigclib_atomic_incU32(&pq->rd); // inc rd
      _uint32 retcode = (pdata != NULL)? pr->datasize : 0; // check NULL-pointer
      sigclib_memcpy(pdata, pr->ptr, (bytesize < retcode)? bytesize : retcode);
      sigclib_atomic_decU32(&pq->fill);
      sigclib_queue_free_record(pr); // free record
      return retcode;
    }
  }
  
  return 0;
}

void *sigclib_queue_get(void *phdl, _uint32 *pbytesize)
{
  // Use this function to get data from thread-safe buffer
  // --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
  // --> pbytesize ....... address where byte size of userdata should be filed, or NULL if not needed
  // Function will return pointer to userdata or NULL if none are present
  // Note: User must call function sigclib_queue_skip() after usage of data to skip record. 
  //       Otherwise the thread-safe buffer will be stuffed over time.

  _tQueue *pq = sigclib_queue_chkhdl_exact(phdl);
  if(pq != NULL)
  {
    _uint32 ird = sigclib_atomic_getU32(&pq->rd);
    _tQueueRecord *pr = (_tQueueRecord*)(&pq->data[(ird & pq->bitpattern) * pq->recordsize]);
    if(sigclib_atomic_cmpxchgU32(&pr->state, QRst_READY, QRst_BUSY_RD) == QRst_READY)
    {
      sigclib_atomic_incU32(&pq->rd); // inc rd
      if(pbytesize != NULL)
      {
        *pbytesize = pr->datasize;
      }
      return pr->ptr;
    }
  }
  
  if(pbytesize != NULL)
  {
    *pbytesize = 0;
  }
  
  return NULL;
}

void *sigclib_queue_skip(void *phdl, void *pdata)
{
  // Function is used to skip (free) record in thread-safe buffer
  // --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
  // --> pdata ........... pointer to userdata given by function sigclib_queue_get()
  // Function will return NULL

  if(pdata != NULL)
  {
    _tQueue *pq = sigclib_queue_chkhdl(phdl);
    if(pq != NULL)
    {
      _uint32 nox = pq->no;
      _uint08 *pi = pq->data;
      _uint32 idx = sigclib_atomic_getU32(&pq->rd);
      
      while(nox--)
      {
        idx--;
        _tQueueRecord *pr = (_tQueueRecord*)(&pi[(idx & pq->bitpattern) * pq->recordsize]);
        if(pr->ptr == pdata)
        {
          sigclib_atomic_decU32(&pq->fill);
          sigclib_queue_free_record(pr); // free record
          return NULL;
        }
      }
    }
  }
  
  return pdata;
}

_uint32 sigclib_queue_free(void *phdl)
{
  // This function is used to empty thread-safe buffer
  // --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
  // Function will return number of recent occupied records
  
  _tQueue *pq = sigclib_queue_chkhdl(phdl);
  if(pq != NULL)
  {
    intern_queue_lock_push(pq); // lock queue, access is therefore blocked
    _uint32 retcode = sigclib_queue_free_record_all(pq); // iterate all records an free
    sigclib_memset(pq->data, 0, pq->no * pq->recordsize); // set data to 0
    sigclib_atomic_setU32(&pq->fill, 0);
    sigclib_atomic_setU32(&pq->wr, 0);
    sigclib_atomic_setU32(&pq->rd, 0);
    intern_queue_lock_pop(pq); // unlock queue, access is therefore guaranteed
    
    return retcode;
  }
  
  return 0;
}

void sigclib_queue_lock_push(void *phdl)
{
  // The function is used to lock thread-safe-buffer. While buffer is locked user is not able to add or get records.
  // Use function sigclib_queue_lock_pop() to unlock buffer afterwards.
  // --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
  // Note: Initially, the buffer is not blocked. If the buffer is blocked by user multiple times, this blockage must also be 
  //       resolved multiple times by using function sigclib_queue_lock_pop().
  
  _tQueue *pq = sigclib_queue_chkhdl(phdl);
  if(pq != NULL)
  {
    intern_queue_lock_push(pq);
  }
}

void sigclib_queue_lock_pop(void *phdl)
{
  // This function is used to unlock a already locked thread-safe-buffer.
  // Use function sigclib_queue_lock_pop() to unlock buffer afterwards.
  // --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
  
  _tQueue *pq = sigclib_queue_chkhdl(phdl);
  if(pq != NULL)
  {
    intern_queue_lock_pop(pq);
  }
}

_uint32 sigclib_queue_used(void *phdl)
{
  // The function is used to determine the actual number of occupied records in thread-safe buffer.
  // --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
  // Function will return the actual number of occupied records in thread-safe buffer
  
  _tQueue *pq = sigclib_queue_chkhdl(phdl);
  if(pq != NULL)
  {
    return sigclib_atomic_getU32(&pq->fill);
  }
  return 0;
}

void *cPipe_CTor(_uint32 record_size, _uint32 record_no)
{
  // constructor of cPipe
  // --> record_size ..... max.byte size of single record
  // --> recode_no ....... maximum number of records in buffer
  // Function will return a valid pointer to cPipe or NULL on error
  return sigclib_queue_cTor(record_size, record_no);
}

void *cPipe_DTor(void *phdl)
{
  // Destructor of cPipe. NOTE.
  // --> phdl ............ pointer to cPipe
  // Function will return NULL on success
  return sigclib_queue_dTor(phdl);
}

_uint32 cPipe_Add(void *phdl, void *pdata, _uint32 datasize)
{
  // Function ist used to userdefined data into cPipe
  // --> phdl ............ pointer to cPipe
  // --> pdata ........... pointer to userdata
  // --> datasize ........ byte size of usedefined data
  // Function will return <>0 on success, on the other hand 0
  
  _tQueue *pq = sigclib_queue_chkhdl(phdl);
  if(pq != NULL)
  {
    if(datasize <= pq->datasize)
    {
      return sigclib_queue_add(phdl, pdata, datasize);
    }
  }
  return 0;
}

_uint32 cPipe_Get(void *pdata, void *phdl)
{
  // Function is used to get a copy of data from cPipe
  // --> pd .............. pointer to destination where user data should be filed
  // --> phdl ............ pointer to cPipe
  // Function will return 1 when record including userdata is found or 0 if no record is available
  return (sigclib_queue_get_copy(phdl, pdata, 0xFFFFFFFF) != 0)? 1 : 0; // legacy 1/0
}

_uint32 cPipe_GetUsed(void *phdl)
{
  // The function is used to determine the actual number of occupied records in thread-safe buffer.
  // --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
  // Function will return the actual number of occupied records in thread-safe buffer
  
  return sigclib_queue_used(phdl);
}

_uint32 cPipe_GetUnUsed(void *phdl)
{
  // The function is used to determine the actual number of unused records in thread-safe buffer.
  // --> phdl ............ pointer to buffer, created by function sigclib_queue_cTor()
  // Function will return the actual number of unused records in thread-safe buffer
  
  _tQueue *pq = sigclib_queue_chkhdl(phdl);
  if(pq != NULL)
  {
    _uint32 no = sigclib_atomic_getU32(&pq->fill);
    return (no < pq->fillmax)? (pq->fillmax - no) : 0;
  }
  return 0;
}

#define tDataRec_DATAUNUSED    0  // Note: Wert nicht ändern
#define tDataRec_DATAREADY     1  // Note: Wert nicht ändern
#define tDataRec_DATACOLLECT   2  // Note: Wert nicht ändern
#define tDataRec_DATABLOCKED   3  // Note: Wert nicht ändern

#define tDataRecBuffDepth      3

typedef struct
{
  void       *pData;                    // Pointer to Userdata
  _uint32     bytesize;                 // aktuelle Byteanzahl der Daten 
  _uint32     tix;                      // incremental Wert zum Speicherzeitpunkt 
  _uint32     inuse;                    // 0...unused, 1...datacollect, 2...ready, 3...blocked
}
tActDataRec;

typedef struct
{
  tActDataRec Data[tDataRecBuffDepth];  // Records
  _uint32     RecByteSize;              // maximale Byteanzahl der Daten in einem Record
  _uint32     NextTix;                  // incremental Wert
}
tActDataBuff;

static tActDataRec *SeekUnusedOrOldestNonBlocked(tActDataBuff *p)
{
  _uint32 state = 0;
  tActDataRec *prec = NULL;

  do
  {
    _uint32 timemax = 0;
    tActDataRec *pi = p->Data;
    _uint32 tax = sigclib_atomic_getU32(&p->NextTix);

    prec = NULL;

    _uint32 nox = sigclib_arraysize(p->Data);
    while (nox--)
    {
      _uint32 tmp = sigclib_atomic_getU32(&pi->inuse);
      if (tmp < tDataRec_DATACOLLECT)
      {
        if (tmp == tDataRec_DATAUNUSED)
        {
          nox = 0; // finito
          state = tmp;
          prec = pi;
        }
        else
        {
          _uint32 diff = tax - pi->tix;
          if (diff >= timemax)
          {
            state = tmp;
            prec = pi;
            timemax = diff;
          }
        }
      }
      pi++;
    }

    if (prec != NULL)
    {
      if (sigclib_atomic_cmpxchgU32(&prec->inuse, state, tDataRec_DATACOLLECT) == state)
      {
        return prec;
      }
    }
  } while (prec != NULL);

  return NULL;
}

void *sigclib_actdata_cTor(_uint32 recordsize)
{
  // Function is used to create a thread-safe buffer to put in datasets and serve the latest valid dataset
  // --> recordsize ............ maximum size of single record
  // Function will return valid pointer to thread-safe buffer, ot NULL on error
  
  _uint32 size0 = sizeof(tActDataBuff);
  while(size0 & 3) { size0++; }           // 32Bit align
  while(recordsize & 3) { recordsize++; } // 32Bit align
  
  _uint08 *ph = (_uint08*)sigclib_calloc(1, size0 + (recordsize * tDataRecBuffDepth));
  if(ph != NULL)
  {
    tActDataBuff *p = (tActDataBuff*)ph;
    for(_uint32 i=0; i<sigclib_arraysize(p->Data); i++)
    {
      p->Data[i].pData = (void*)&ph[size0 + i * recordsize];
      p->Data[i].inuse = tDataRec_DATAUNUSED;
    }
    
    p->RecByteSize = recordsize;
    return (void*)p;
  }
  return NULL;
}

void *sigclib_actdata_dTor(void *phdl)
{
  // Destructor
  // Function will always return NULL
  
  if(phdl != NULL)
  {
    sigclib_free(phdl);
  }
  return NULL;
}

_uint32 sigclib_actdata_add(void *phdl, void *pdata, _uint32 bytesize)
{
  // neue Daten in Puffer eintragen
  // --> phdl .................. valid pointer to thread-safe buffer
  // --> pdata ................. user data
  // --> bytesize .............. byte size of userdata
  // Function will return 1 on success, on the oterr hand 0

  if (phdl != NULL)
  {
    tActDataBuff *p = (tActDataBuff*)phdl;

    if (bytesize <= p->RecByteSize)
    {
      tActDataRec *prec = SeekUnusedOrOldestNonBlocked(p);
      if (prec != NULL)
      {
        sigclib_memcpy(prec->pData, pdata, bytesize);
        prec->tix = sigclib_atomic_incU32(&p->NextTix);//sigclib_tabsolute();
        prec->bytesize = bytesize;
        sigclib_atomic_setU32(&prec->inuse, tDataRec_DATAREADY);
        return 1;
      }
    }
  }

  return 0;
}

void *sigclib_actdata_get(void *phdl, _uint32 *pbytesize)
{
  // Pointer auf aktuelle Daten im Puffer ermitteln
  // --> phdl .................. gültiges Handle
  // --> bytesize .............. NULL oder die Adresse wo anzahl der aktuellen DatenBytes eingetragen wird
  // Funktion liefert einen Pointer auf die aktuellsten Daten oder NULL falls keine Daten vorhanden
  // NOTE: Nachdem Daten verarbeitet wurden muss Funktion ActDataBuff_Skip() aufgerufen werden.
  
  if(phdl != NULL)
  {
    tActDataBuff *p = (tActDataBuff*)phdl;
    tActDataRec *prec = NULL;
  
    _uint32 timemax = 0xFFFFFFFF;
    _uint32 tax = sigclib_atomic_getU32(&p->NextTix); //sigclib_tabsolute();
    tActDataRec *pi = p->Data;
    _uint32 nox = sigclib_arraysize(p->Data);

    while (nox--)
    {
      if (sigclib_atomic_cmpxchgU32(&pi->inuse, tDataRec_DATAREADY, tDataRec_DATABLOCKED) == tDataRec_DATAREADY)
      {
        _uint32 diff = tax - pi->tix;
        if ((prec == NULL) || (diff < timemax))
        {
          if (prec != NULL)
          { // es gibt einen neueren record
            sigclib_atomic_setU32(&prec->inuse, tDataRec_DATAUNUSED);
          }
          prec = pi;
          timemax = diff;
        }
        else
        {
          sigclib_atomic_setU32(&pi->inuse, tDataRec_DATAUNUSED);
        }
      }
      pi++;
    }
 
    if (prec != NULL)
    {
      if (pbytesize != NULL)
      {
        *pbytesize = prec->bytesize;
      }

      return (void*)prec->pData;
    }
  }

  if (pbytesize != NULL)
  {
    *pbytesize = 0;
  }

  return NULL;
}

void sigclib_actdata_skip(void *phdl, void *pdata)
{
  // diese Funktion muss nach ActDataBuff_GetActual() aufgerufen werden.
  // --> phdl .................. gültiges Handle
  // --> pdata ................. Pointer welcher von Funktion ActDataBuff_GetActual() retourniert wurde
  
  if ((phdl != NULL) && (pdata != NULL))
  {
    tActDataBuff *p = (tActDataBuff*)phdl;
    tActDataRec  *pi = p->Data;

    _uint32 nox = sigclib_arraysize(p->Data);
    while (nox--)
    {
      if (pi->pData == pdata)
      {
        sigclib_atomic_setU32(&pi->inuse, tDataRec_DATAUNUSED);
        return;
      }
      pi++;
    }
  }
}

