//<NewSigmatekCFileOptimize/>
// +----------------------------------------------------------------------------------------------+
// +-[   copyright ] Sigmatek GmbH & CoKG                                                         |
// +-[      author ] kolott                                                                       |
// +-[        date ] 26.07.2019                                                                   |
// +-[ description ]------------------------------------------------------------------------------+
// |                                                                                              |
// |                                                                                              |
// +----------------------------------------------------------------------------------------------+

#if defined(_GNUC) && !defined(__OPTIMIZE__)
 #warning Optimization is OFF
#endif 

#include "SigCLib.h"
#include "SigCLibKey.h"
#include "CStrLibIntern.h"

static const unsigned char TabBase64Encode[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

static const unsigned char TabBase64Decode[] = { 0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
                                                 0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
                                                 0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0x3E,0xFF,0xFF,0xFF,0x3F,
                                                 0x34,0x35,0x36,0x37,0x38,0x39,0x3A,0x3B,0x3C,0x3D,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
                                                 0xFF,0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,0x0B,0x0C,0x0D,0x0E,
                                                 0x0F,0x10,0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0xFF,0xFF,0xFF,0xFF,0xFF,
                                                 0xFF,0x1A,0x1B,0x1C,0x1D,0x1E,0x1F,0x20,0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,
                                                 0x29,0x2A,0x2B,0x2C,0x2D,0x2E,0x2F,0x30,0x31,0x32,0x33,0xFF,0xFF,0xFF,0xFF,0xFF};

// ************************************************************************************************
// ************************************************************************************************
// utf8
// ************************************************************************************************
// ************************************************************************************************

unsigned long sigclib_utf8_to_ascii_crop(void *dst_ascii, unsigned long dst_bytesize, const void *src_utf8, const char unknown_chr)
{
  // Diese Funktion konvertiert einen ASCII oder UTF8 codierten String in einen ASCII codierten String
  // INFO: Funktion kann mit beiden src-Formaten umgehen, dst_ascii darf auch NULL sein. Dann wird nur die Länge in Zeichen berechnet
  // --> dst_ascii ..... Destination wo ascii-String abgelegt wird
  // --> dst_bytesize .. Bytesize von Detstination
  // --> src_utf8 ...... zu konvertierender String im ASCII oder UTF8 Format
  // --> unknown_chr ... Ersatzzeichen für unbekannten Glyph, oder 0 wenn kein Ersatzzeichen gewünscht ist.
  // Funktion retourniert die Anzahl der Bytes in dst_ascii ohne final 0
  // Vorsicht: Muss gleicher Algorithmus wie bei Funktion sigclib_utf8_to_str16() und sigclib_utf8_check() sein

  unsigned long retcode = 0;
  unsigned char *ps = (unsigned char*)src_utf8;
  unsigned char *pd = (unsigned char*)dst_ascii;

  while (*ps)
  {
    unsigned long chr = *ps++;
    if (chr & 0x0080)
    {
      if ((chr & 0x00E0) == 0x00C0)
      {
        if ((ps[0] & 0xC0) == 0x80) // 1 folgebyte
        {
          chr = (chr & 0x001F);
          chr = (chr << 6) | (ps[0] & 0x3F);
          ps += 1;
        }
      }
      else if ((chr & 0x00F0) == 0x00E0)
      {
        if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80)) // 2 folgebytes
        {
          chr = (chr & 0x000F);
          chr = (chr << 6) | (ps[0] & 0x3F);
          chr = (chr << 6) | (ps[1] & 0x3F);
          ps += 2;
        }
      }
      else if ((chr & 0x00F8) == 0x00F0)
      {
        if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80) && ((ps[2] & 0xC0) == 0x80)) // 3 folgebytes
        {
          chr = (chr & 0x0007);
          chr = (chr << 6) | (ps[0] & 0x3F);
          chr = (chr << 6) | (ps[1] & 0x3F);
          chr = (chr << 6) | (ps[2] & 0x3F);
          ps += 3;
        }
      }
      
      if(chr > 255)
      {
        chr = unknown_chr;
      }
    }
    
    if(chr != 0)
    {
      retcode++;
      if((pd != NULL) && (dst_bytesize > 1)) // grösser 1 wegen final-0
      {
        *pd++ = *(unsigned char*)&chr;
        dst_bytesize --;
      }
    }
  }
  
  if((pd != NULL) && (dst_bytesize > 0))
  {
    *pd = 0; // final 0
  }
  
  return retcode;
}

unsigned long sigclib_utf8_to_ascii(void *dst_ascii, const void *src_utf8)
{
  return sigclib_utf8_to_ascii_crop(dst_ascii, 0xFFFFFFFF, src_utf8, '?');
}

unsigned long sigclib_utf8_to_str16_crop(void *dst_uni, unsigned long dst_bytesize, const void *src_utf8)
{
  // Diese Funktion konvertiert einen ASCII oder UTF8 codierten String in einen U16 codierten String
  // INFO: Funktion kann mit beiden src-Formaten umgehen, dst_uni darf auch NULL sein. Dann wird nur die Länge in Zeichen berechnet
  // --> dst_uni ....... Destination wo U16-String abgelegt wird
  // --> dst_bytesize .. Bytesize von Detstination
  // --> src_utf8 ...... zu konvertierender String im ASCII oder UTF8 Format
  // Funktion retourniert die Anzahl der Zeichen in dst_uni.
  // Vorsicht: Muss gleicher Algorithmus wie bei Funktion sigclib_utf8_to_ascii() und sigclib_utf8_check() sein

  unsigned long retcode = 0;
  unsigned char *ps = (unsigned char*)src_utf8;
  unsigned short *pd = (unsigned short*)dst_uni;
  unsigned long dst_cnt = dst_bytesize / 2;

  while (*ps)
  {
    unsigned long chr = *ps++;
    if (chr & 0x0080)
    {
      if ((chr & 0x00E0) == 0x00C0)
      {
        if ((ps[0] & 0xC0) == 0x80) // 1 folgebyte
        {
          chr = (chr & 0x001F);
          chr = (chr << 6) | (ps[0] & 0x3F);
          ps += 1;
        }
      }
      else if ((chr & 0x00F0) == 0x00E0)
      {
        if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80)) // 2 folgebytes
        {
          chr = (chr & 0x000F);
          chr = (chr << 6) | (ps[0] & 0x3F);
          chr = (chr << 6) | (ps[1] & 0x3F);
          ps += 2;
        }
      }
      else if ((chr & 0x00F8) == 0x00F0)
      {
        if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80) && ((ps[2] & 0xC0) == 0x80)) // 3 folgebytes
        {
          chr = (chr & 0x0007);
          chr = (chr << 6) | (ps[0] & 0x3F);
          chr = (chr << 6) | (ps[1] & 0x3F);
          chr = (chr << 6) | (ps[2] & 0x3F);
          ps += 3;
          
          if(chr > 0xFFFF)
          {
            chr = '?';
          }
        }
      }
    }
    
    if(dst_cnt > 1) // grösser 1 wegen final-0
    {
      dst_cnt--;
      retcode++;
      if(pd != NULL)
      {
        *pd++ = (unsigned short)chr;
      }
    }
  }
  
  if((pd != NULL) && (dst_cnt > 0))
  {
    *pd = 0; // final 0
  }
  
  return retcode;
}

unsigned long sigclib_utf8_to_str16(void *dst_uni, const void *src_utf8)
{
  // Diese Funktion konvertiert einen ASCII oder UTF8 codierten String in einen U16 codierten String
  // INFO: Funktion kann mit beiden src-Formaten umgehen, dst_uni darf auch NULL sein. Dann wird nur die Länge in Zeichen berechnet
  // --> dst_uni ..... Destination wo U16-String abgelegt wird
  // --> src_utf8 .... zu konvertierender String im ASCII oder UTF8 Format
  // Funktion retourniert die Anzahl der Zeichen in dst_uni.
  // Vorsicht: Muss gleicher Algorithmus wie bei Funktion sigclib_utf8_to_ascii() und sigclib_utf8_check() sein

  return sigclib_utf8_to_str16_crop(dst_uni, 0xFFFFFFFF, src_utf8);
}

unsigned long sigclib_utf8_check(const void *src)
{
  // Diese Funktion überprüft ob hier ein UTF8-codierter String vorliegt oder nicht
  // --> src .... zu untersuchender String im ASCII oder UTF8 Format
  // Funktion retourniert 1 wenn ein UTF8 codierter String übergeben wurde, ansonsten 0
  // Note: Falls eine Codierung nicht utf8 entspricht wird 0 retourniert, auch wenn bereits eine andere utf8 Codierung gefunden wurde.

  _uint32 retcode = 0;
  _uint08 *ps = (_uint08*)src;
  
  if (ps != NULL)
  {
    while (*ps)
    {
      if (*ps & 0x80)
      {
        _uint08 chr = *ps;
        if ((chr & 0xE0) == 0xC0) 
        {
          if ((ps[1] & 0xC0) != 0x80)
          { 
            return 0; // kein UTF-8
          }
          ps++;
          retcode = 1;
        }
        else if ((chr & 0xF0) == 0xE0)
        {
          if (((ps[1] & 0xC0) != 0x80) || ((ps[2] & 0xC0) != 0x80))
          { 
            return 0; // kein UTF-8
          }
          ps += 2;
          retcode = 1;
        }
        else if ((chr & 0xF8) == 0xF0)
        {
          if (((ps[1] & 0xC0) != 0x80) || ((ps[2] & 0xC0) != 0x80) || ((ps[3] & 0xC0) != 0x80))
          {
            return 0; // kein UTF-8
          }
          ps += 3;
          retcode = 1;
        }
      }
      ps++;
    }
  }

  return retcode;
}

unsigned long sigclib_utf8_to_str16_len(const void *src_utf8)
{
  // Diese Funktion berechnet die Anzahl an Characters im U16 String
  // --> src_utf8 .... zu konvertierender String im ASCII oder UTF8 Format
  // Funktion retourniert die Anzahl an Characters
  // Vorsicht: Muss gleicher Algorithmus wie bei Funktion sigclib_utf8_to_str16() sein
  
  return sigclib_utf8_to_str16(NULL, src_utf8);
}

unsigned long sigclib_utf8_to_ascii_len(const void *src_utf8)
{
  // Diese Funktion berechnet die Anzahl an Characters/Bytes im ascii String
  // --> src_utf8 .... zu konvertierender String im ASCII oder UTF8 Format
  // Funktion retourniert die Anzahl an Characters
  // Vorsicht: Muss gleicher Algorithmus wie bei Funktion sigclib_utf8_to_ascii() sein

  return sigclib_utf8_to_ascii(NULL, src_utf8);
}

unsigned long sigclib_str16_to_utf8(void *dst_utf8, const void *src_uni)
{
  // Note: gleicher Algorithmus wird in Funktion sigclib_utf8_len() verwendet

  unsigned long  retcode = 0;
  unsigned short *ps = (unsigned short*)src_uni;
  unsigned char  *pd = (unsigned char*)dst_utf8;

  while(*ps)
  {
    unsigned short chr = *ps++;
    if(chr < 0x0080)
    {
      *pd++    = *(unsigned char*)&chr;
      retcode += 1;
    }
    else if(chr < 0x0800)
    {
      *pd++    = (unsigned char)(0x00C0 | (chr >> 6));
      *pd++    = (unsigned char)(0x0080 | (chr & 0x003F));
      retcode += 2;
    }
    else
    {
      *pd++    = (unsigned char)(0x00E0 | (chr >> 12));
      *pd++    = (unsigned char)(0x0080 | ((chr >> 6) & 0x003F));
      *pd++    = (unsigned char)(0x0080 | (chr & 0x003F));
      retcode += 3;
    }
  }
  *pd = 0;
  
  return retcode;
}

unsigned long sigclib_ascii_to_utf8_crop(void *dst_utf8, unsigned long dst_bytesize, const void *src_ascii)
{
  // Note: gleicher Algorithmus wird in Funktion sigclib_utf8_len() verwendet
  
  unsigned long retcode = 0;
  unsigned char *ps = (unsigned char*)src_ascii;
  unsigned char *pd = (unsigned char*)dst_utf8;

  while(*ps)
  {
    unsigned char chr = *ps++;
    if(chr < 0x0080)
    {
      if(dst_bytesize > 1) // grösser 1 wegen final-0 
      {
        *pd++    = *(unsigned char*)&chr;
        retcode += 1;
        dst_bytesize -= 1;
      }
    }
    else if(dst_bytesize > 2) // grösser 2 wegen final-0
    {
      *pd++    = (unsigned char)(0x00C0 | (chr >> 6));
      *pd++    = (unsigned char)(0x0080 | (chr & 0x003F));
      retcode += 2;
      dst_bytesize -= 2;
    }
  }
  
  if(dst_bytesize > 0)
  {
    *pd = 0;
  }
  
  return retcode;
}

unsigned long sigclib_ascii_to_utf8(void *dst_utf8, const void *src_ascii)
{
  return sigclib_ascii_to_utf8_crop(dst_utf8, 0xFFFFFFFF, src_ascii);
}

unsigned long sigclib_ascii_to_utf8_len(const void *src_ascii)
{
  // funktion berechnet die Länge des utf8 codierten String anhand von src in ASCII
  // --> src ....... string to compute (ASCII-0-String)
  // retourniert wird die Anzahl der Zeichen ohne final-0, entspricht dem Ergebnis von sigclib_strlen()
  
  // Vorsicht: Muss gleicher Algorithmus wie bei Funktion sigclib_ascii_to_utf8() sein
  
  unsigned long retcode = 0;
  unsigned char *ps = (unsigned char*)src_ascii;
  while(*ps)
  {
    retcode ++;
    unsigned char chr = *ps++;
    if(chr & 0x80)
    {
      retcode ++;
    }
  }
  
  return retcode;
}

unsigned long sigclib_str16_to_utf8_len(const void *src_uni)
{
  // funktion berechnet die Länge des utf8 codierten String anhand von src in UNICODE
  // --> src ....... string to compute (UNI-0-String)
  // retourniert wird die Anzahl der Zeichen ohne final-0, entspricht dem Ergebnis von sigclib_strlen()
  
  // Vorsicht: Muss gleicher Algorithmus wie bei Funktion sigclib_str16_to_utf8() sein
  
  unsigned long retcode = 0;
  
  unsigned short *ps = (unsigned short*)src_uni;
  while(*ps)
  {
    unsigned short chr = *ps++;
    if(chr < 0x0080)
    {
      retcode += 1;
    }
    else if(chr < 0x0800)
    {
      retcode += 2;
    }
    else
    {
      retcode += 3;
    }
  }
  
  return retcode;
}

// ************************************************************************************************
// ************************************************************************************************
// utf16
// ************************************************************************************************
// ************************************************************************************************

typedef unsigned short tUTF16;
typedef unsigned char  tUTF8;

static const unsigned char FirstByteMark[7]   = { 0x00, 0x00, 0xC0, 0xE0, 0xF0, 0xF8, 0xFC };
static const unsigned long OffsetsFromUTF8[6] = { 0x00000000UL, 0x00003080UL, 0x000E2080UL, 0x03C82080UL, 0xFA082080UL, 0x82082080UL };
static const unsigned char BytesFromUTF8[256] = {	0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                                  0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                                  0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                                  0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                                  0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                                  0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                                                  1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1, 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                                                  2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2, 3,3,3,3,3,3,3,3,4,4,4,4,5,5,5,5 };

#define UTF16_ReplaceChr      0x0000FFFDUL
#define UTF16_SurrogateHighStart  0xD800UL
#define UTF16_SurrogateHighEnd    0xDBFFUL
#define UTF16_SurrogateLowStart   0xDC00UL
#define UTF16_SurrogateLowEnd     0xDFFFUL

static unsigned long sigclib_utf16_to_utf8_intern(tUTF8 **ppd, const tUTF8 *pdend, tUTF16 **pps, const tUTF16 *psend)
{
  unsigned long retcode = 0;
  tUTF16 *ps = *pps;
  tUTF8  *pd = *ppd;

  while(ps < psend)
  {
    unsigned long byteno = 2;
    unsigned long chr = *ps++;

    if((chr >= UTF16_SurrogateHighStart) && (chr <= UTF16_SurrogateHighEnd) && (ps < psend))
    {
      unsigned long chr2 = *ps;
      if((chr2 >= UTF16_SurrogateLowStart) && (chr2 <= UTF16_SurrogateLowEnd))
      {
        chr = ((chr - UTF16_SurrogateHighStart) << 10) + (chr2 - UTF16_SurrogateLowStart) + 0x0010000UL;
        ps++;
      }
    }

    if(chr < 0x80)               { byteno = 1; }
    else if(chr < 0x800)         { byteno = 2; }
    else if(chr < 0x10000)       { byteno = 3; }
    else if(chr < 0x200000)      { byteno = 4; }
    else if(chr < 0x4000000)     { byteno = 5; }
    else if(chr <= 0x7FFFFFFFUL) { byteno = 6; }
    else { chr = UTF16_ReplaceChr; }

    pd += byteno;

    if(pd > pdend)
    {
      pd -= byteno;
      retcode = 1; // destbuffer zu klein
      break;
    }

    switch(byteno) // note: "Durchläufer"
    {
      case 6: *--pd = (chr|0x80) & 0xBF; chr >>= 6;
      case 5: *--pd = (chr|0x80) & 0xBF; chr >>= 6;
      case 4: *--pd = (chr|0x80) & 0xBF; chr >>= 6;
      case 3: *--pd = (chr|0x80) & 0xBF; chr >>= 6;
      case 2: *--pd = (chr|0x80) & 0xBF; chr >>= 6;
      case 1: *--pd = chr|FirstByteMark[byteno];
    }

    pd += byteno;
  }

  *pps = ps;
  *ppd = pd;

  return retcode;
}

static unsigned long sigclib_utf8_to_utf16_intern(tUTF16 **ppd, const tUTF16 *pdend, tUTF8 **pps, const tUTF8 *psend)
{
  unsigned long retcode = 0;
  tUTF8  *ps = *pps;
  tUTF16 *pd = *ppd;

  while(ps < psend)
  {
    unsigned long chr = 0;
    unsigned long ex_bytes = BytesFromUTF8[*ps];

    if((ps + ex_bytes) > psend)
    {
      retcode = 2; // fehler sourcebuffer
      break;
    }

    switch(ex_bytes) // note: "Durchläufer"
    {
      case 5: chr += *ps++; chr <<= 6;
      case 4: chr += *ps++; chr <<= 6;
      case 3: chr += *ps++; chr <<= 6;
      case 2: chr += *ps++; chr <<= 6;
      case 1: chr += *ps++; chr <<= 6;
      case 0: chr += *ps++;
    }

    chr -= OffsetsFromUTF8[ex_bytes];

    if(pd >= pdend)
    {
      retcode = 1; // destbuffer zu klein
      break;
    }

    if(chr <= 0x0000FFFFUL)
    {
      *pd++ = chr;
    }
    else if(chr > 0x0010FFFFUL)
    {
      *pd++ = UTF16_ReplaceChr;
    }
    else
    {
      if((pd + 1) >= pdend)
      {
        retcode = 1; // destbuffer zu klein
        break;
      }
      
      chr   -= 0x0010000UL;
      *pd++ = (chr >> 10) + UTF16_SurrogateHighStart;
      *pd++ = (chr & 0x3FFUL) + UTF16_SurrogateLowStart;
    }
  }

  *pps = ps;
  *ppd = pd;

  return retcode;
}

unsigned long sigclib_utf8_to_utf16(unsigned short *pdst_utf16, unsigned long dst_bytesize, const char *psrc_utf8, unsigned long no)
{
  // convert UTF8 to UTF16
  // --> pdst_utf16 ...... destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_utf8........ UTF8-coded sourcebuffer
  // --> no .............. length UTF8-coded sourcebuffer
  // function will return length of UTF16-codec destinationstring or 0 in case of error (destinationbuffer too small, format-error in sourcebuffer)

  unsigned short *pdst = pdst_utf16;
  unsigned char  *psrc = (unsigned char*)psrc_utf8;
  unsigned long  dmax  = dst_bytesize / 2;
  if(sigclib_utf8_to_utf16_intern(&pdst, pdst + dmax, &psrc, psrc + no) == 0)
  {
    unsigned long len = pdst - pdst_utf16; // das ist strlen16(utf16)
    if(len < dmax)
    {
      *pdst = 0;
    }
    return len;
  }
  
  return 0;
}

unsigned long sigclib_utf16_to_utf8(unsigned char *pdst_utf8, unsigned long dst_bytesize, unsigned short *psrc_utf16, unsigned long no)
{
  // convert UTF16 to UTF8
  // --> pdst_utf8 ....... destinationbuffer
  // --> dst_byte_size ... bytesize of destinationbuffer
  // --> psrc_utf16 ...... UTF16-coded sourcebuffer
  // --> no .............. length of UTF16-coded sourcebuffer
  // function will return length of UTF8-codec destinationstring or 0 in case of error (destinationbuffer too small, format-error in sourcebuffer)
  
  unsigned char  *pdst = pdst_utf8;
  unsigned short *psrc = psrc_utf16;
  if(sigclib_utf16_to_utf8_intern(&pdst, pdst+dst_bytesize, &psrc, psrc+no) == 0)
  {
    unsigned long len = pdst - pdst_utf8; // das ist strlen(utf8)
    if(len < dst_bytesize)
    {
      *pdst = 0;
    }
    return len;
  }
  
  return 0;
}

// ************************************************************************************************
// ************************************************************************************************
// Base64
// ************************************************************************************************
// ************************************************************************************************

unsigned long sigclib_base64_encode_size(unsigned long srcsize)
{
  unsigned long iteration = (srcsize + 2) / 3;
  return 4 * iteration;
}

unsigned long sigclib_base64_encode(unsigned char *dst, unsigned long dstsize, void *src0, unsigned long srcsize)
{
  if((srcsize > 0) && (src0 != NULL) && (dst != NULL))
  {
    unsigned char *src      = (unsigned char*)src0;
    unsigned long iteration = (srcsize + 2) / 3;
    unsigned long retcode   = (4 * iteration);
    long srclen             = (long)srcsize;
    
    if(dstsize >= retcode)
    {
      while(iteration--)
      {
        unsigned char byte, rest, idx;
        
        byte   = *src++;
        *dst++ = TabBase64Encode[byte >> 2];
        rest   = byte & 0x03;
        srclen --;
    
        byte = (srclen > 0)? *src++ : 0;
        *dst++ = TabBase64Encode[(rest << 4) | (byte >> 4)];
        rest = byte & 0x0F;
        srclen --;

        byte = (srclen > 0)? *src++ : 0;
        idx = (rest << 2) | (byte >> 6);
        rest = byte & 0x3F;
        *dst++ = (srclen > -1)? TabBase64Encode[idx] : '=';
        srclen --;

        *dst++ = (srclen > -1)? TabBase64Encode[rest] : '=';
      }
      
      return retcode;
    }
  }
  
  return 0;
}

unsigned long sigclib_base64_decode(void *dst0, unsigned long dstsize, const unsigned char *src, unsigned long srcsize)
{
  unsigned long retcode = 0;
  if((srcsize > 0) && (src != NULL) && (dst0 != NULL))
  {
    unsigned char *dst = (unsigned char*)dst0;
        
    unsigned char code[4];
    unsigned long idx = 0;
    
    while(srcsize--)
    {
      unsigned char byte = *src++;
      if((byte != '\n') && (byte != '=')) // werden ignoriert
      {
        if(byte >= sizeof(TabBase64Decode))
        {
          return 0; // something wrong with sourcedata
        }
        
        unsigned char tmp = TabBase64Decode[byte];
        
        if(tmp == 0xFF)
        {
          return 0; // something wrong with sourcedata
        }
        
        code[idx] = tmp;
        idx = (idx + 1) & 0x03;
      
        if(idx == 0)
        {
          if(dstsize < 3)
          {
            return 0;
          }
          dst[0] = (code[0] << 2) | (code[1] >> 4);
          dst[1] = (code[1] << 4) | (code[2] >> 2);
          dst[2] = (code[2] << 6) | (code[3]);
          dst += 3;
          retcode += 3;
          dstsize -= 3;
        }
      }
    }
    
    if(idx > 1)
    {
      if(dstsize < 1)
      {
        return 0;
      }
      dst[0] = (code[0] << 2) | (code[1] >> 4);
      retcode++;
      dstsize--;

      if(idx > 2)
      {
        if(dstsize < 1)
        {
          return 0;
        }
        dst[1] = (code[1] << 4) | (code[2] >> 2);
        retcode++;
        dstsize--;
      }
    }
  }
  
  return retcode;
}


// ************************************************************************************************
// ************************************************************************************************
// binary en/decode
// ************************************************************************************************
// ************************************************************************************************

void sigclib_intern_decode_bin(void *p0, unsigned long bytesize, unsigned long key0, unsigned long key1);
void sigclib_intern_encode_bin(void *p0, unsigned long bytesize, unsigned long key0, unsigned long key1);

void sigclib_decode_bin(void *p0, unsigned long bytesize, unsigned long key0, unsigned long key1)
{
  sigclib_intern_decode_bin(p0, bytesize, key0, key1);
}

void sigclib_encode_bin(void *p0, unsigned long bytesize, unsigned long key0, unsigned long key1)
{
  sigclib_intern_encode_bin(p0, bytesize, key0, key1);
}


// ************************************************************************************************
// ************************************************************************************************
// text en/decode
// ************************************************************************************************
// ************************************************************************************************

void sigclib_intern_decode_txt(unsigned char *p, unsigned long bytesize, unsigned long key);
void sigclib_intern_encode_txt(unsigned char *p, unsigned long bytesize, unsigned long key);

void sigclib_decode_txt(void *p0, unsigned long bytesize, unsigned long key)
{
  if((p0 != NULL) && (bytesize > 0))
  {
    sigclib_intern_decode_txt((unsigned char*)p0, bytesize, key);
  }
}

void sigclib_encode_txt(void *p0, unsigned long bytesize, unsigned long key)
{
  if((p0 != NULL) && (bytesize > 0))
  {
    sigclib_intern_encode_txt((unsigned char*)p0, bytesize, key);
  }
}

// ************************************************************************************************
// ************************************************************************************************
// parse
// ************************************************************************************************
// ************************************************************************************************

static const char *sigclib_parse_SkipIntern(const char *pl, const char *skip, _uint32 len)
{
  // diese Funktion skipt alle Zeichen in 'pl' wenn sie ident mit einem der Zeichen in string 'skip' sind solange bis ein Zeichen welches nicht in 'skip' enthalten ist, gefunden wird.
  // retourniert wird der korrigierte Pointer

  const char *p0 = pl;
  do
  {
    p0 = pl;
    _uint32 nox = len;
    const char *ph = skip;
    while (nox--)
    {
      if (*pl == *ph)
      {
        pl++;
        break;
      }
      ph++;
    }
  } while (pl != p0);

  return pl;
}

static const char *sigclib_parse_SkipSpacer(const char *pl)
{
  // diese Funktion skipt alle [SPACE] und [TAB] bis zum Auftreten eines anderen Zeichen
  // retourniert wird der korrigierte pointer
  while ((pl[0] == ' ') || (pl[0] == '\t'))
  {
    pl++;
  }
  return pl;
}

static const char *sigclib_parse_SkipInternCaseInSensitive(const char *pl, const char *skip, _uint32 len)
{
  // diese Funktion skipt alle Zeichen in 'pl' wenn sie ident mit einem der Zeichen in string 'skip' sind solange bis ein Zeichen welches nicht in 'skip' enthalten ist, gefunden wird.
  // retourniert wird der korrigierte Pointer

  const char *p0 = pl;
  do
  {
    p0 = pl;
    _uint32 nox = len;
    const char *ph = skip;
    while (nox--)
    {
      if (_TOLOWER(*pl) == _TOLOWER(*ph))
      {
        pl++;
        break;
      }
      ph++;
    }
  } while (pl != p0);

  return pl;
}

static _uint32 sigclib_parse_U64_intern(const char **pp, _uint64 *pval64, _uint32 *pbase, bool just_dezimal)
{
  // Funktion parst die nachfolgende u64 Zahl
  // Wurde eine gültige Zahl gefunden wird 1 retourniert und 'pp' entspreched korrigiert.
  // Gültige Formate: 0xFF, 123, +123
  // --> pp ............. Source
  // --> pval64 ......... Destination
  // --> pbase .......... erkanntes Zahlenbasis
  // --> just_dezimal ... true wenn nur dezimale Zahlen als gültig anerkannt werden

  const char *pl = *pp;
  _uint64 val64 = 0;
  _uint64 oval64 = 0;
  bool st = false;
  bool run = true;
  
  _uint32 base = 10;
  if ((just_dezimal == false) && (pl[0] == '0') && ((pl[1] == 'x') || (pl[1] == 'X'))) // 0x
  {
    if(_ISXDIGIT(pl[2])) // 0123456789 ABCDEF abcdef
    {
      base = 16;
    }
  }

  if (base == 16) // hexadezimale Interpretation der Zahl
  {
    pl++;
    do
    {
      pl++;
      switch (*pl)
      {
        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
          val64 = (val64 * 16) + (pl[0] - '0');
          st = true;
          break;

        case 'A':
        case 'B':
        case 'C':
        case 'D':
        case 'E':
        case 'F':
          val64 = (val64 * 16) + (pl[0] - 'A' + 10);
          st = true;
          break;

        case 'a':
        case 'b':
        case 'c':
        case 'd':
        case 'e':
        case 'f':
          val64 = (val64 * 16) + (pl[0] - 'a' + 10);
          st = true;
          break;

        default:
          run = false;
          break;
      }
      
      if(oval64 != (val64 / 16))
      {
        if(run == true)
        {
          return 0; // overflow detected
        }
      }
      oval64 = val64;
      
    } while (run == true);
  }
  else // dezimale Interpretation der Zahl
  {
    pl--;
    do
    {
      pl++;
      switch (*pl)
      {
        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
          val64 = (val64 * 10) + (pl[0] - '0');
          if(oval64 != (val64 / 10))   
          {
            return 0; // overflow detected
          }
          oval64 = val64;
          st = true;
          break;

        default:
          run = false;
          break;
      }
    } while (run == true);
  }
  
  if (st == true)
  {
    *pp = pl;
    *pval64 = val64;
    
    if(pbase != NULL)
    {
      *pbase = base;
    }
    
    return 1;
  }
  
  return 0;
}

static _int32 sigclib_parse_check_and_skip_sign(const char **pp, bool just_plus)
{
  // Funktion kontrolliert nachfolgendes Zeichen auf '+' oder '-'
  // Wurde '+' sign gefunden, wird 1 retourniert, wenn '-' gefunden wird, -1 retourniert, sonst 0 retourniert
  // --> pp ............. Source
  // --> just_plus ...... true wenn nur '+' als gültig anerkannt werden soll

  const char *pl = *pp;
  
  if(*pl == '+')
  {
    *pp = pl + 1;
    return 1;
  }
  else if((*pl == '-') && (just_plus == false))
  {
    *pp = pl + 1;
    return -1;
  }
  
  return 0;
}

_uint32 sigclib_parse_U64(const char **pp, _uint64 *pval64)
{
  // Funktion parst die nachfolgende u64 Zahl
  // Wurde eine gültige Zahl gefunden wird 1 retourniert und 'pp' entspreched korrigiert.
  // Gültige Formate: 0xFF, 123, +123

  const char *pl = sigclib_parse_SkipSpacer(*pp); // skip [SPACE], [TAB]
  sigclib_parse_check_and_skip_sign(&pl, true);
  if(sigclib_parse_U64_intern(&pl, pval64, NULL, false) != 0)
  {
    *pp = pl;
    return 1;
  }
  return 0;
}

static _f64 sigclib_parse_F64_exponent(const char **pp, _f64 fval64)
{
  // folgende Schreibweise wird unterstützt: "e-03", "E+3"

  const char *pl = *pp;
  if((*pl == 'e') || (*pl == 'E')) // check 'e'
  {
    pl += 1;
    _int32 neg = sigclib_parse_check_and_skip_sign(&pl, false); // check sign
    if(_ISDIGIT(*pl))
    {
      _uint64 uval64;
      if(sigclib_parse_U64_intern(&pl, &uval64, NULL, true) != 0) // get dezimalen exponent
      {
        _uint32 nox = (_uint32)uval64;
        if(neg < 0)
        {
          while(nox--)
          {
            fval64 = fval64 / 10;
          }
        }
        else
        {
          while(nox--)
          {
            fval64 = fval64 * 10;
          }
        }
        *pp = pl; // bingo
      }
    }
  }
  
  return fval64;
}

_uint32 sigclib_parse_F64(const char **pp, _f64 *pfval64)
{
  // Funktion parst die nachfolgende f64 Zahl
  // Wurde eine gültige Zahl gefunden wird 1 retourniert und 'pp' entspreched korrigiert.
  // Gültige Formate: 0xFF, 123, 123.456, -123.456, +123.456, 1.9e-02, 1.9E+02, .456

  const char *pl = sigclib_parse_SkipSpacer(*pp); // skip [SPACE], [TAB]
  _int32 neg = sigclib_parse_check_and_skip_sign(&pl, false);
  pl = sigclib_parse_SkipSpacer(pl); // skip [SPACE], [TAB]

  _uint32 base = 10; // Default
  _uint64 uval64 = 0; // Vorkommazahl
  _uint32 predec = (*pl == '.')? 1 : 0; // Vorkommazahl gültig (zB Format: ".3")
  
  if(predec == 0)
  {
    predec = sigclib_parse_U64_intern(&pl, &uval64, &base, false); // Vorkommazahl parsen
  }
  
  if (predec != 0)
  {
    _f64 val64 = (_f64)uval64;
    if ((*pl == '.') && (base == 10)) // '.' nur wenn eine dezimal Zahl detektiert wurde kann ein Kommapunkt folgen
    {
      pl++;
      if(_ISDIGIT(*pl)) // check ob zeichen 0-9
      {
        _uint64 divi64 = 1;
        while (*pl == '0')
        {
          divi64 *= 10;
          pl++;
        }

        if (sigclib_parse_U64_intern(&pl, &uval64, NULL, true) != 0) // Nachkomma immer nur dezimal
        {
          _f64 nk = (_f64)uval64;
          while (uval64 > 0)
          {
            uval64 /= 10;
            nk /= 10;
          }
          if (divi64 > 1)
          {
            nk = nk / (_f64)divi64;
          }
          val64 += nk;
        }
      }
      else
      {
        return 0; // wrong syntax
      }
    }
    
    if(neg < 0)
    {
      val64 = -val64;
    }
    
    if((*pl == 'e') || (*pl == 'E')) // ggf. Exponent vorhanden
    {
      if(base == 10) // nur bei dezimaler schreibweise
      {
        val64 = sigclib_parse_F64_exponent(&pl, val64); // do Exponent
      }
    }

    *pfval64 = val64;
    *pp = pl;
    return 1;
  }

  return 0;
}

_uint32 sigclib_parse_S64(const char **pp, _int64 *pval64)
{
  // Funktion parst die nachfolgende s64 Zahl
  // Wurde eine gültige Zahl gefunden wird 1 retourniert und 'pp' entspreched korrigiert.
  // Gültige Formate: 0xFF, 123, -123, +123

  const char *pl = sigclib_parse_SkipSpacer(*pp); // skip [SPACE], [TAB]
  _int32 neg = sigclib_parse_check_and_skip_sign(&pl, false);
  pl = sigclib_parse_SkipSpacer(pl); // skip [SPACE], [TAB]

  _uint32 base;
  _uint64 uval64;
  if (sigclib_parse_U64_intern(&pl, &uval64, &base, false) != 0)
  {
    if((base == 16) && (neg >= 0)) // falls Hexzahl ohne '-' Vorzeichen
    {
      *pval64 = *(_int64*)&uval64;
      *pp = pl;
      return 1;
    }
    else
    {
      if (uval64 <= 0x7FFFFFFFFFFFFFFFLL)
      {
        _int64 ival = *(_int64*)&uval64;
        *pval64 = (neg < 0) ? -ival : ival;
        *pp = pl;
        return 1;
      }
      else if ((uval64 == 0x8000000000000000LL) && (neg < 0))
      {
        *pval64 = 0x8000000000000000LL;
        *pp = pl;
        return 1;
      }
    }
  }
  
  return 0;
}

_uint32 sigclib_parse_U32(const char **pp, _uint32 *pval32)
{
  // Funktion parst die nachfolgende u32 Zahl
  // Wurde eine gültige Zahl gefunden wird 1 retourniert und 'pp' entspreched korrigiert.
  // Gültige Formate: 0xFF, 123, +123

  const char *pl = *pp;
  _uint64 val64;
  if (sigclib_parse_U64(&pl, &val64) != 0)
  {
    if (val64 <= 0xFFFFFFFF)
    {
      *pp = pl;
      *pval32 = (_uint32)val64;
      return 1;
    }
  }
  return 0;
}

static _uint32 sigclib_parse_U32naked(const char **pp, _uint32 *pval32)
{
  // Funktion überprüft ob ein unsigned 32Bit Wert ohne Vorzeichen ist
  const char *pl = sigclib_parse_SkipSpacer(*pp);
  if(_ISDIGIT(*pl)) // check ob zeichen 0-9
  {
    if(sigclib_parse_U32(&pl, pval32) != 0)
    {
      *pp = pl;
      return 1;
    }
  }
  return 0;
}

_uint32 sigclib_parse_S32(const char **pp, _int32 *pval32)
{
  // Funktion parst die nachfolgende s32 Zahl
  // Wurde eine gültige Zahl gefunden wird 1 retourniert und 'pp' entspreched korrigiert.
  // Gültige Formate: 0xFF, 123, -123, +123
  
  const char *pl = sigclib_parse_SkipSpacer(*pp); // skip [SPACE], [TAB]
  _int32 neg = sigclib_parse_check_and_skip_sign(&pl, false);
  pl = sigclib_parse_SkipSpacer(pl); // skip [SPACE], [TAB]

  _uint32 base;
  _uint64 uval64;
  if(sigclib_parse_U64_intern(&pl, &uval64, &base, false) != 0)
  {
    if(uval64 <= 0xFFFFFFFF)
    {
      if((base == 16) && (neg >= 0)) // falls Hexzahl ohne '-' Vorzeichen
      {
        *pval32 = sigclib_GetByteByByte32(&uval64); // ansonsten "break strict-aliasing" warning
        *pp = pl;
        return 1;
      }
      else
      {
        if (uval64 <= 0x7FFFFFFF)
        {
          _int32 ival = (_int32)uval64;
          *pval32 = (neg < 0) ? -ival : ival;
          *pp = pl;
          return 1;
        }
        else if ((uval64 == 0x80000000) && (neg < 0))
        {
          *pval32 = 0x80000000; //-2147483648LL
          *pp = pl;
          return 1;
        }
      }
    }
  }
  return 0;
}

_uint32 sigclib_parse_F32(const char **pp, _f32 *pfval)
{
  // Funktion parst die nachfolgende f32 Zahl
  // Wurde eine gültige Zahl gefunden wird 1 retourniert und 'pp' entspreched korrigiert.
  // Gültige Formate: 0xFF, 123, 123.456, -123.456, +123.456

  const char *pl = *pp;
  _f64 fval64;
  if (sigclib_parse_F64(&pl, &fval64) != 0)
  {
    *pfval = (_f32)fval64;
    *pp = pl;
    return 1;
  }
  
  return 0;
}

_uint32 sigclib_parse_IsEOL(const char *pl)
{
  // Funktion überprüft ob bereits das Ende des zu parsenden Strings erreicht wurde
  // Folgende Zeihcen werden als Ende interprtiert: [\0],[LF],[FF] oder [CR]
  pl = sigclib_parse_SkipSpacer(pl); // skip [SPACE] und [TAB]
  
  if ((pl[0] == 0) || (pl[0] == 10) || (pl[0] == 12) || (pl[0] == 13)) // [\0], [LF], [FF] oder [CR]
  {
    return 1;
  }
  
  return 0;
}

_uint32 sigclib_parse_IsChrAny(const char **pp, const char *str, _uint32 case_sensitive)
{
  // Funktion überprüft ob das nächste Zeichen eines der in 'str' definierten Zeichen ist.
  // In diesem Fall wird das gefundene Zeichen retourniert und 'pp' entsprechend korrigiert

  const char *pl1 = sigclib_parse_SkipSpacer(*pp); // skip [SPACE] und [TAB]
  const char *pl = pl1;

  _uint32 len = 0;
  const char *pi = str;
  while(*pi++ != 0) { len++; } // inline strlen
  _uint32 len0 = len;

  if (case_sensitive != 0)
  {
    char chr = *pl;
    while (len--)
    {
      if (chr == str[len])
      {
        *pp = pl + 1;
        return chr;
      }
    }
  }
  else
  {
    char chr = _TOLOWER(*pl);
    while (len--)
    {
      if (chr == _TOLOWER(str[len]))
      {
        *pp = pl + 1;
        return *pl; // nicht chr wegen _TOLOWER
      }
    }
  }
  
  if(pl1 != *pp) // Anfangs [SPACE] oder [TAB] bereits entfernt
  {
    // Check ob in 'str' das '**pp' Zeichen enthalten ist.
    // Das **pp Zeichen kann in diesem speziellen Fall nur [TAB] oder [SPACE] sein.
    char chr = **pp;
    while (len0--)
    {
      if (chr == str[len0])
      {
        *pp = pl1;
        return chr;
      }
    }
  }

  return 0;
}

_uint32 sigclib_parse_IsToken(const char **pp, const char *searchee, _uint32 solid_token, _uint32 case_sensitive)
{
  // --> solid_token .... überprüft ob es wirklich ein ganzer Token ist (1) oder nur der Beginn eines Token sein muss (0)
  // Funktion überprüft ob folgender Token mit 'searchee' beginnt
  // falls ja, wird pp entsprechend der Länge von 'serachee' korrigiert und 1 retourniert, ansonsten 0

  const char *pl = sigclib_parse_SkipSpacer(*pp); // skip [SPACE] und [TAB]
  
  if(pl[0] == 0)
  {
    *pp = pl;
    return (searchee[0] == 0)? 1 : 0;
  }  
  
  _uint32 searchee_len;
  if (case_sensitive != 0)
  {
    searchee_len = sigclib_strstart(pl, searchee);
  }
  else
  {
    searchee_len = sigclib_stristart(pl, searchee);
  }
  
  if (searchee_len != 0)
  {
    pl += searchee_len;
    if ((solid_token == 0) || (*pl == ' ') || (*pl == 0))
    {
      *pp = pl;
      return 1;
    }

    if (_ISPUNCT(*pl)) // !"#$%&'()*+,-./:;<=>?@[\]^_`{|}~
    {
      *pp = pl;
      return 1;
    }
    
    if (_ISSPACE(*pl)) // \0, SPACE, TAB, CR, LF, FF ...
    {
      *pp = pl;
      return 1;
    }
  }

  return 0;
}

_uint32 sigclib_parse_Skip(const char **pp, const char *skip, _uint32 case_sensitive)
{
  // diese Funktion skipt alle Zeichen in '*pp' wenn sie ident mit einem der Zeichen in string 'skip' sind solange bis ein Zeichen welches nicht in 'skip' enthalten ist, gefunden wird.
  // retourniert wird die Anzahl der geskippten Zeichen.
  
  _uint32 len = 0;
  const char *pi = skip;
  while(*pi++ != 0) { len++; } // inline strlen
  
  if(len > 0)
  {
    const char *p0 = *pp;
    const char *pl;
  
    if(case_sensitive != 0)
    {
      pl = sigclib_parse_SkipIntern(p0, skip, len);
    }
    else
    {
      pl = sigclib_parse_SkipInternCaseInSensitive(p0, skip, len);
    }
  
    if(pl != p0)
    {
      *pp = pl;
      return (_uint32)(pl - p0);
    }
  }
  
  return 0;
}

static _uint32 sigclib_parse_TextIntermediateSame(const char **pp, char *pd, _uint32 sizepd, const char c)
{
  // Funktion kopiert Text zwischen zB. "" und retourniert dessen Länge inclusive final 0, eine Länge 0 ist somit nur im Fehlerfall möglich.

  const char *pl = sigclib_parse_SkipSpacer(*pp); // skip [SPACE] und [TAB]
  if (*pl == c)
  {
    pl++;
    const char *p0 = pl;
    _uint32 cnt = 0;
    while (*pl != 0)
    {
      if (*pl == c)
      {
        *pp = pl + 1; // rücksicherung
        if ((sizepd > 0) && (pd != NULL))
        {
          sizepd -= 1; // final 0
          cnt = (cnt <= sizepd) ? cnt : sizepd;
          sigclib_memcpy(pd, p0, cnt);
          pd[cnt] = 0;
          return cnt + 1;
        }
        return 0;
      }

      cnt++;
      pl++;
    }
  }

  if ((sizepd > 0) && (pd != NULL))
  {
    *pd = 0;
  }

  return 0;
}

_uint32 sigclib_parse_TextIntermediate(const char **pp, char *pd, _uint32 sizepd, const char *between)
{
  // Funktion kopiert Text zwischen inbetween[0] und inbetween[1] und retourniert dessen Länge inclusive final 0, eine Länge 0 ist somit nur im Fehlerfall möglich.
  // Info: Klammerebenen werden berücksichtigt

  if (between == NULL)
  {
    between = "\"\""; // default
  }

  if (between[0] != 0) // ansonsten kann kein Anfang gefunden werden
  {
    if ((between[0] == between[1]) || (between[1] == 0)) // wenn beide gleich oder nur eins
    {
      return sigclib_parse_TextIntermediateSame(pp, pd, sizepd, between[0]);
    }

    const char *pl = sigclib_parse_SkipSpacer(*pp); // skip [SPACE] und [TAB]
    if (*pl == between[0])
    {
      pl++;
      const char *p0 = pl;
      _uint32 depth = 1;
      _uint32 cnt = 0;
      while (*pl != 0)
      {
        if (*pl == between[0])
        {
          depth++;
        }
        else if (*pl == between[1])
        {
          depth--;
          if (depth == 0)
          {
            *pp = pl + 1; // rücksicherung
            if ((sizepd > 0) && (pd != NULL))
            {
              sizepd -= 1; // final 0
              cnt = (cnt <= sizepd) ? cnt : sizepd;
              sigclib_memcpy(pd, p0, cnt);
              pd[cnt] = 0;
              return cnt + 1;
            }
            return 0;
          }
        }

        cnt++;
        pl++;
      }
    }
  }

  if ((sizepd > 0) && (pd != NULL))
  {
    *pd = 0;
  }

  return 0;
}

static bool sigclib_parse_IsNum_intern(const char *pl, bool space, bool punkt, bool plus, bool minus)
{
  // Funktion überprüft ob als nächstes eine Zahl kommt
  if (space == true)
  {
    pl = sigclib_parse_SkipSpacer(pl); // skip [SPACE] und [TAB]
  }
  switch (*pl)
  {
    case '.': // zB: .31
      if (punkt == true)
      {
        return sigclib_parse_IsNum_intern(pl + 1, false, false, false, false); // 0123456789
      }
      break;
    case '-': // zB: -314
      if (minus == true)
      {
        return sigclib_parse_IsNum_intern(pl + 1, true, true, false, false); //.0123456789
      }
      break;
    case '+': // zB: "+314" oder "+.314"
      if (plus == true)
      {
        return sigclib_parse_IsNum_intern(pl + 1, true, true, false, false); // .0123456789
      }
      break;
    case '0': // zB: 03
    case '1':
    case '2':
    case '3':
    case '4':
    case '5':
    case '6':
    case '7':
    case '8':
    case '9': return true;
  }

  return false;
}

_uint32 sigclib_parse_IsNum(const char *pl, bool just_unsigned_int)
{
  // Funktion überprüft ob als nächstes eine Zahl kommt
  bool st = (just_unsigned_int != 0) ? false : true;
  return (sigclib_parse_IsNum_intern(pl, true, st, true, st) == true)? 1 : 0;
}

static _uint32 sigclib_parse_IpAddressIntern(const char **pp, _uint32 *paddr, _uint32 *pport, const char *separator)
{
  // Format: 10.10.116.22:5555, als Trennzeichen sind "separator" gültig. Portnummer ist immer mit ':' getrennt
  // parse IpAddress + ggf. Port
  // pport darf auch NULL sein

  const char *pl = *pp;
  _uint32 a[4];
  if (sigclib_parse_U32naked(&pl, &a[0]) != 0) // XX
  {
    if (sigclib_parse_IsChrAny(&pl, separator, 1) != 0) // eines dieser zeichen '.'',''-', [TAB] und [SPACE]
    {
      if (sigclib_parse_U32naked(&pl, &a[1]) != 0) // XX.XX
      {
        if (sigclib_parse_IsChrAny(&pl, separator, 1) != 0) // eines dieser zeichen '.'',''-', [TAB] und [SPACE]
        {
          if (sigclib_parse_U32naked(&pl, &a[2]) != 0) // XX.XX.XX
          {
            if (sigclib_parse_IsChrAny(&pl, separator, 1) != 0) // eines dieser zeichen '.'',''-', [TAB] und [SPACE]
            {
              if (sigclib_parse_U32naked(&pl, &a[3]) != 0) // XX.XX.XX.XX
              { 
                if ((a[0] < 256) && (a[1] < 256) && (a[2] < 256) && (a[3] < 256))
                {
                  const char *pll = pl;
                  if ((pport != NULL) && (sigclib_parse_IsChrAny(&pll, ":", 1) == ':'))
                  {
                    if(sigclib_parse_U32naked(&pll, pport) != 0)
                    {
                      pl = pll;
                    }
                  }
                  *paddr = (a[3] << 24) | (a[2] << 16) | (a[1] << 8) | (a[0]);
                  *pp = pl;
                  return 1;
                }
              }
            }
          }
        }
      }
    }
  }

  return 0;
}

_uint32 sigclib_parse_IpAddress(const char **pp, _uint32 *paddr, _uint32 *pport)
{
  // Format: 10.10.116.22:5555, als Trennzeichen sind '.'',''-', [TAB] und [SPACE] gültig. Portnummer ist immer mit ':' getrennt
  // parse IpAddress + ggf. Port
  // pport darf auch NULL sein

  return sigclib_parse_IpAddressIntern(pp, paddr, pport, ".,- \t");
}

static _uint32 sigclib_parse_DateIntern(const char **pp, _uint32 *pdate, const char *separator, _uint32 check)
{
  // parse Date 2023.08.24, als Trennzeichen sind "separator" gültig.
  // Datum wird ggf. auch auf numerische Richtigkeit geprüft und im LasalFormat geliefert

  const char *pl = *pp;

  _uint32 year, month, day;
  if (sigclib_parse_U32naked(&pl, &year) != 0)
  {
    if (sigclib_parse_IsChrAny(&pl, separator, 1) != 0) // eines dieser zeichen
    {
      if (sigclib_parse_U32naked(&pl, &month) != 0)
      {
        if (sigclib_parse_IsChrAny(&pl, separator, 1) != 0) // eines dieser zeichen
        {
          if (sigclib_parse_U32naked(&pl, &day) != 0)
          {
            if ((day > 31) && (year <= 31))
            {
              // Format: 24.08.2024
              _uint32 m = year;
              year = day;
              day = m;
            }

            if ((year > 0) && (year < 0xFFFF) && (month >= 1) && (month <= 12) && (day >= 1) && (day <= 31))
            {
              _uint32 ldate = sigclib_date_to_lasal(year, month, day);
              
              if ((check == 0) || (sigclib_lasal_to_date(NULL, NULL, NULL, ldate) != 0)) // check
              {
                if (pdate != NULL)
                {
                  *pdate = ldate;
                }
                *pp = pl;
                return 1;
              }
            }
          }
        }
      }
    }
  }

  return 0;
}

_uint32 sigclib_parse_Date(const char **pp, _uint32 *pdate)
{
  // parse Date 2023.08.24, als Trennzeichen sind '.'',''-' und '/' gültig.
  // Datum wird auch auf numerische Richtigkeit geprüft und im LasalFormat geliefert
  
  return sigclib_parse_DateIntern(pp, pdate, ".,-/", 1);
}

static _uint32 sigclib_parse_TimeIntern(const char **pp, _uint32 *ptime, const char *separator, _uint32 check)
{
  // parse Time 23:46:17 oder 11:46:17pm, als Trennzeichen sind "separator" gültig.
  // Time wird ggf. auch auf numerische Richtigkeit geprüft und im LasalFormat geliefert
  // falls keine Sekunden angegeben wurden, werden diese auf 0 gesetzt

  const char *pl = *pp;

  _uint32 hour, min;
  if (sigclib_parse_U32naked(&pl, &hour) != 0)
  {
    if (sigclib_parse_IsChrAny(&pl, separator, 1) != 0) // eines dieser zeichen
    {
      if (sigclib_parse_U32naked(&pl, &min) != 0)
      {
        _uint32 sec = 0;
        const char *pll = pl;
        if (sigclib_parse_IsChrAny(&pll, separator, 1) != 0) // eines dieser zeichen
        {
          if (sigclib_parse_U32naked(&pll, &sec) != 0)
          {
            pl = pll;
          }
        }

        // check am/pm
        if(sigclib_parse_IsToken(&pl, "am", 1, 0) != 0) // check am
        {
          if(hour == 12)
          {
            hour =  0;
          }
        }
        else if(sigclib_parse_IsToken(&pl, "pm", 1, 0) != 0) // check pm
        {
          if((hour > 0) && (hour < 12))
          {
            hour += 12;
          }
        }

        if ((hour < 24) && (min < 60) && (sec < 60))
        {
          _uint32 ltime = sigclib_time_to_lasal(hour, min, sec);
          if ((check == 0) || (sigclib_lasal_to_time(NULL, NULL, NULL, ltime) != 0)) // check
          {
            if (ptime != NULL)
            {
              *ptime = ltime;
            }
            *pp = pl;
            return 1;
          }
        }
      }
    }
  }

  return 0;
}

_uint32 sigclib_parse_Time(const char **pp, _uint32 *ptime)
{
  // parse Time 23:46:17 oder 11:46:17pm, als Trennzeichen sind ':''.'',' und '-' gültig.
  // Time wird auch auf numerische Richtigkeit geprüft und im LasalFormat geliefert
  // falls keine sekunden angegeben wurden, werden diese auf 0 gesetzt

  return sigclib_parse_TimeIntern(pp, ptime, ":.,-", 1);
}

static bool sigclib_parse_IsRegularCharacter(char chr)
{
  if (chr <= 'z')
  {
    if ((chr >= 'a') || ((chr >= 'A') && (chr <= 'Z')) || (chr == '_') || ((chr >= '0') && (chr <= '9')))
    { 
      return true; 
    }
  }
  
  return false;
}

_uint32 sigclib_parse_RegularExpression(const char **pp, char *pd, _uint32 sizepd)
{
  // Es wird eine RegularExpression aus Bytestream ausgelesen.
  // Eine RegularExpression beinhaltet 'A'-'Z', 'a'-'z', '0'-'9' und '_'. Am Beginn darf aber kein '0'-'9' vorkommen.
  // Beispiel: "_Hello33_World1"
  // Funktion retourniert die Anzahl an Zeichen in geparster RegularExpression
  
  _uint32 retcode = 0;
  const char *pl = sigclib_parse_SkipSpacer(*pp);
  if ((*pl >= 'A') && (sigclib_parse_IsRegularCharacter(*pl) == true)) // 'A'-'Z', 'a'-'z' oder '_', (!)KEIN '0'-'9'
  {
    do
    {
      if (sizepd > 1)
      {
        *pd++ = *pl;
        sizepd--;
      }
      pl++;
      retcode++;
    } while (sigclib_parse_IsRegularCharacter(*pl)); // 'A'-'Z', 'a'-'z', '0'-'9' oder '_' 

    *pp = pl;
  }

  if (sizepd > 0)
  {
    *pd = 0;
  }

  return retcode;
}

static bool sigclib_parse_IsAnyChrOf(char chr, const char *fin, _uint32 len)
{
  while (len--)
  {
    if (chr == *fin++)
    {
      return true;
    }
  }
  
  return false;
}

_uint32 sigclib_parse_Token(const char **pp, char *pd, _uint32 sizepd, const char *separator)
{
  // Es wird eine Token bis zum Auftreten von einem Zeichen aus 'separator' aus Bytestream ausgelesen.
  // Funktion retourniert die Anzahl an Zeichen in geparsten Token
  
  _uint32 retcode = 0;
  const char *fin = (separator != NULL)? separator : "";
  _uint32 fin_len = sigclib_strlen(fin) + 1; // +1 somit wird auch \0 überprüft
  
  const char *pl = sigclib_parse_SkipSpacer(*pp);
  while (sigclib_parse_IsAnyChrOf(*pl, fin, fin_len) == false)
  {
    if (sizepd > 1)
    {
      *pd++ = *pl;
      sizepd--;
    }
    pl++;
    retcode++;
  } 

  if (sizepd > 0)
  {
    *pd = 0;
  }

  *pp = pl;

  return retcode;
}


// ************************************************************************************************
// ************************************************************************************************
// arbitrary
// ************************************************************************************************
// ************************************************************************************************


// ------------------------------------------------------------------------------------------------
// Funktion konvertiert einen String in lasal-datestamp
// Als Trennzeichen gelten [SPACE],[/],[.],[_] und [-]
// --> date........ Pointer wo ermittelter lasal-datestamp abgelegt werden soll
// --> text........ Pointer auf zu decodierenden ascii-0-string (Beispiel: 2023/12/20, 20.12.2023, 20-12-2023)
// ------------------------------------------------------------------------------------------------
// <-- ein Pointer auf das erste Zeichen nach dem eigentlichen date-string
// ================================================================================================
char *sigclib_string_to_date(unsigned long *date, const char *text)
{
  *date = 0;
  if(text == NULL)
  {
    return NULL;
  }

  const char *pl = text;
  if(sigclib_parse_DateIntern(&pl, date, "/._- ", 0) != 0)
  {
    return (char*)pl;
  }
  
  return (char*)text;  
}

// ------------------------------------------------------------------------------------------------
// Funktion konvertiert einen String in lasal-timestamp
// Als Trennzeichen gelten [SPACE],[:],[.],[_] und [-]
// weiters sind die case-insensitiven Zeichenfolgen "am" und "pm" gültig
// --> time........ Pointer wo ermittelter lasal-timestamp abgelegt werden soll
// --> text........ Pointer auf zu decodierenden ascii-0-string
// ------------------------------------------------------------------------------------------------
// <-- ein Pointer auf das erste Zeichen nach dem eigentlichen time-string
// ================================================================================================
char *sigclib_string_to_time(unsigned long *time, const char *text)
{
  *time = 0xFFFFFFFF;
  if(text == NULL)
  {
    return (char*)text;
  }
  
  const char *pl = text;
  if(sigclib_parse_TimeIntern(&pl, time, ":,_- ", 0) != 0)
  {
    return (char*)pl;
  }

  return (char*)text;
}

// ------------------------------------------------------------------------------------------------
// Funktion konvertiert einen String in IpV4
// Als Trennzeichen gelten [SPACE],[:],[.] und [_]
// --> dst ......... Address of Destination where u32-ip-address should be filed
// --> txtin ....... IP-Address-String
// ------------------------------------------------------------------------------------------------
// <-- ein Pointer auf das erste Zeichen nach dem eigentlichen IpV4-string
// ================================================================================================
cExtern char *sigclib_u32ipaddress(unsigned long *dst, const char *txtin) // snert
{
  const char *pl = txtin;
  
  while((pl[0] != 0) && (_ISDIGIT(pl[0]) == 0)) // legacy: skip all preceeding non 0-9
  {
    pl++;
  }

  if(sigclib_parse_IpAddressIntern(&pl, dst, NULL, ":._ ") != 0) // parse IpV4
  {
    return (char*)pl;
  }
  
  *dst = 0; // default
  return (char*)txtin;
}

