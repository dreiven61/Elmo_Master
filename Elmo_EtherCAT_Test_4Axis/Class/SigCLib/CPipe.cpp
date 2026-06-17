//<NewSigmatekCFileOptimize/>
// +----------------------------------------------------------------------------------------------+
// +-[   copyright ] Sigmatek GmbH & CoKG                                                         |
// +-[      author ] kolott                                                                       |
// +-[        date ] 10.07.2017                                                                   |
// +-[ description ]------------------------------------------------------------------------------+
// |                                                                                              |
// |                                                                                              |
// +----------------------------------------------------------------------------------------------+

#include "SigCLib.h" 

#define PIPE_MASK_CPXG   0x00121314
#define PIPE_MASK_LOCK   0x005A5ABC
#define PIPE_MASK_VALID  0x01234560

// ------------------------------------------------------------------------------------------------
// constructor of cPipe
// --> record_size ....... max.bytesize of single record
// --> recode_no ......... maximum number of records in buffer
// ------------------------------------------------------------------------------------------------
// <-- NULL ... error,  <> NULL pointer to cPipe
// ================================================================================================
cPipe *cPipe_CTor(unsigned long record_size, unsigned long record_no)
{
  cPipe *retcode = (cPipe*)sigclib_malloc(sizeof(cPipe));
  
  if(retcode != NULL)
  {
    cPipe_Init(retcode, record_size, record_no);
  }
  
  return retcode;
}

// ------------------------------------------------------------------------------------------------
// destructor to destroy whole cPipe. NOTE: just use when constructor is in use
// --> p ................. pointer to cPipe
// ------------------------------------------------------------------------------------------------
// <-- always NULL
// ================================================================================================
cPipe *cPipe_DTor(cPipe *p)
{
  if(p != NULL)
  {
    cPipe_Free(p);
    sigclib_free(p);
  }
  
  return NULL;
}

// ------------------------------------------------------------------------------------------------
// initialize cPipe
// --> p ................. pointer to cPipe
// --> record_size ....... max.bytesize of single record
// --> recode_no ......... maximum number of records in buffer
// ------------------------------------------------------------------------------------------------
// <-- 1 ... success, 0 ... error
// ================================================================================================
unsigned long cPipe_Init(cPipe *p, unsigned long record_size, unsigned long record_no)
{
  p->rd          = 0;    // aktuelle leseposition
  p->wr          = 0;    // aktuelle writeposition
  p->record_no   = 0;    // gesamtanzahl der reords im recordpuffer
  p->record_size = 0;    // grösse eines records in bytes
  p->record_user = 0;    // grösse der userdaten im record
  p->data        = NULL; // pointer auf datapuffer
  p->rd_lock     = 0;    // rd_lock
  p->bitpattern  = 0;
  
  if((record_size != 0) && (record_no > 0) && ((((unsigned long)p) & 3) == 0))
  {
    unsigned long fullsize = record_size + 8; // userdaten + 2*4byte
    fullsize = ((fullsize + 3) / 4) * 4; // recordsize muss 4 byte aligned sein
    
    // folgende grössen einhalten, ansonsten funktioniert 32bit-wrap von rd- und wr-index nicht
    if     (record_no > 0x8000) record_no = 0x10000;
    else if(record_no > 0x4000) record_no = 0x08000;
    else if(record_no > 0x2000) record_no = 0x04000;
    else if(record_no > 0x1000) record_no = 0x02000;
    else if(record_no > 0x0800) record_no = 0x01000;
    else if(record_no > 0x0400) record_no = 0x00800;
    else if(record_no > 0x0200) record_no = 0x00400;
    else if(record_no > 0x0100) record_no = 0x00200;
    else if(record_no > 0x0080) record_no = 0x00100;
    else if(record_no > 0x0040) record_no = 0x00080;
    else if(record_no > 0x0020) record_no = 0x00040;
    else if(record_no > 0x0010) record_no = 0x00020;
    else if(record_no > 0x0008) record_no = 0x00010;
    else if(record_no > 0x0004) record_no = 0x00008;
    else if(record_no > 0x0002) record_no = 0x00004;
    
    unsigned long bytesize = fullsize * record_no; // gesamtgrösse des puffers in bytes
    if(sigclib_memory((void**)&p->data, bytesize) != 0)
    {
      sigclib_memset(p->data, 0, bytesize);
      p->record_no   = record_no;
      p->bitpattern  = record_no - 1;
      p->record_size = fullsize;
      p->record_user = record_size;
      return 1;
    }
  }
  
  return 0;
}

// ------------------------------------------------------------------------------------------------
// free whole cPipe
// --> p ................. pointer to cPipe
// ================================================================================================
void cPipe_Free(cPipe *p)
{
  if(p != NULL)
  {
    unsigned char *tmp = p->data;
    cPipe_Init(p, 0, 0);
    sigclib_memory((void**)&tmp, 0);
  }
}

// ------------------------------------------------------------------------------------------------
// put copy of recorddata into cPipe
// --> p ................. pointer to cPipe
// --> pdata ............. pointer to recorddata
// --> datasize .......... bytenumber of new recorddata
// ------------------------------------------------------------------------------------------------
// <-- 1 ... success, 0 ... error (cPipe is full, recorddata too big)
// ================================================================================================
unsigned long cPipe_Add(cPipe *p, void *pdata, unsigned long datasize)
{
  if(p != NULL) 
  {
    if(datasize <= p->record_user)
    {
      unsigned long nox = (p->record_no > 10)? 10 : p->record_no; // incl.bremse, 10 versuche sollten reichen
      unsigned long idx = sigclib_atomic_getU32(&p->wr);
      
      while(nox--)
      {
        unsigned long *pu32 = (unsigned long*)&p->data[(idx & p->bitpattern) * p->record_size];
        if(sigclib_atomic_cmpxchgU32(&pu32[0], 0, PIPE_MASK_CPXG) == 0) // ist record aktuell unbenützt --> diesen benützen
        {
          sigclib_memcpy(&pu32[2], pdata, datasize); // recorddaten kopieren
          sigclib_atomic_swpU32(&pu32[1], PIPE_MASK_VALID); // recorddaten auf gültig setzen
          sigclib_atomic_incU32(&p->wr); // inc wr-index
          return 1; // finito
        }
        idx++;
      }
    }
  }
  return 0;
}

// ------------------------------------------------------------------------------------------------
// get record from cPipe
// --> pd ................ pointer to destination where data of record will be copied
// --> p ................. pointer to cPipe
// ------------------------------------------------------------------------------------------------
// <-- 1 ... recorddata sucessfull copied, 0 ... no recorddata available 
// ================================================================================================
unsigned long cPipe_Get(void *pdata, cPipe *p)
{
  unsigned long retcode = 0;
  
  if(p != NULL)
  {
    if(p->wr != p->rd) // sind records vorhanden
    {
      if(sigclib_atomic_cmpxchgU32(&p->rd_lock, 0, PIPE_MASK_LOCK) == 0) // wenn kein lock --> lock
      {
        if(p->wr != p->rd) // nochmalige prüfung während rd-lock
        {
          unsigned long idx = sigclib_atomic_getU32(&p->rd) & p->bitpattern;
          unsigned long *pu32 = (unsigned long*)&p->data[idx * p->record_size];
      
          if(pu32[1] == PIPE_MASK_VALID) // ist record aktuell gültig --> diesen auslesen
          {
            if(pdata != NULL)
            {
              sigclib_memcpy(pdata, &pu32[2], p->record_user); // recorddaten kopieren
            }
            sigclib_atomic_swpU32(&pu32[1], 0); // recorddaten auf ungültig setzen
            sigclib_atomic_swpU32(&pu32[0], 0); // record auf unbenützt setzen
            sigclib_atomic_incU32(&p->rd); // inc rd-index
            retcode = 1;
          }
        }
      
        sigclib_atomic_swpU32(&p->rd_lock, 0); // unlock
      }
    }
  }
  return retcode;
}

// ------------------------------------------------------------------------------------------------
// get number of used recordplaces in cPipe
// --> p ................. pointer at cPipe
// ------------------------------------------------------------------------------------------------
// <-- number of records in cPipe 
// ================================================================================================
unsigned long cPipe_GetUsed(cPipe *p)
{
  if(p != NULL)
  {
    return (sigclib_atomic_getU32(&p->wr) - sigclib_atomic_getU32(&p->rd));
  }
  return 0;
}

// ------------------------------------------------------------------------------------------------
// get number of unused records in buffer
// --> p ................. pointer auf cPipe
// ------------------------------------------------------------------------------------------------
// <-- number of unused records in cPipe 
// ================================================================================================
unsigned long cPipe_GetUnUsed(cPipe *p)
{
  if(p != NULL)
  {
    return p->record_no - (sigclib_atomic_getU32(&p->wr) - sigclib_atomic_getU32(&p->rd));
  }
  return 0;
}


/*
// liefert immer die letzte gültige zelle
unsigned long cLiFo_Add(cPipe *p, void *pdata, unsigned long datasize)
{
  if(p != NULL) 
  {
    if(datasize <= p->record_user)
    {
      unsigned long nox = (p->record_no > 5)? 5 : p->record_no; // incl.bremse, 5 versuche sollten reichen
      unsigned long idx = sigclib_atomic_getU32(&p->wr);
      
      while(nox--)
      {
        unsigned long *pu32 = (unsigned long*)&p->data[(idx & p->bitpattern) * p->record_size];
        if(sigclib_atomic_cmpxchgU32(&pu32[0], 0, PIPE_MASK_LOCK) == 0) // wenn kein lock --> lock
        {
          sigclib_atomic_swpU32(&pu32[1], 0); // recorddaten auf ungültig setzen
          sigclib_memcpy(&pu32[2], pdata, datasize); // recorddaten kopieren
          sigclib_atomic_swpU32(&pu32[1], PIPE_MASK_VALID); // recorddaten auf gültig setzen
          sigclib_atomic_incU32(&p->wr); // inc wr-index
          sigclib_atomic_swpU32(&pu32[0], 0); // unlock
          return 1; // finito
        }
        idx++;
      }
    }
  }
  return 0;
}

unsigned long cLiFo_Get(void *pdata, cPipe *p)
{
  if(p != NULL)
  {
    unsigned long nox = (p->record_no > 5)? 5 : p->record_no; // incl.bremse, 5 versuche sollten reichen
    unsigned long idx = sigclib_atomic_getU32(&p->wr); 
    
    while(nox--)
    {
      idx--;
      unsigned long *pu32 = (unsigned long*)&p->data[(idx & p->bitpattern) * p->record_size];
  
      if(pu32[1] == PIPE_MASK_VALID) // ist record aktuell gültig --> diesen auslesen
      {
        if(sigclib_atomic_cmpxchgU32(&pu32[0], 0, PIPE_MASK_LOCK) == 0) // wenn kein lock --> lock
        {
          if(pdata != NULL)
          {
            sigclib_memcpy(pdata, &pu32[2], p->record_user); // recorddaten kopieren
          }
          sigclib_atomic_swpU32(&pu32[0], 0); // unlock
          return 1;
        }
      }
    }
  }
  
  return 0;
}
*/

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
  // Constructor
  // --> recordsize ............ erzeugt einen ActDataBuffer
  
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
  // <-- retourniert immer NULL
  
  if(phdl != NULL)
  {
    sigclib_free(phdl);
  }
  return NULL;
}

_uint32 sigclib_actdata_add(void *phdl, void *pdata, _uint32 bytesize)
{
  // neue Daten in Puffer eintragen
  // --> phdl .................. gültiges Handle
  // --> pdata ................. Daten
  // --> bytesize .............. Bytesize der Daten
  // <-- Funktion liefert bei Erfolg 1, ansonsten 0

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
  // <-- Funktion liefert einen Pointer auf die aktuellsten Daten oder NULL falls keine Daten vorhanden
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




