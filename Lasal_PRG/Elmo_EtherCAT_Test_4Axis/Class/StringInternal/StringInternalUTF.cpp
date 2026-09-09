//<NewSigmatekCFileOptimize/>
// +----------------------------------------------------------------------------------------------+
// +-[   copyright ] Sigmatek GmbH & CoKG                                                         |
// +-[      author ] kolott                                                                       |
// +-[        date ] 04.03.2026                                                                   |
// +-[ description ]------------------------------------------------------------------------------+
// |                                                                                              |
// |                                                                                              |
// +----------------------------------------------------------------------------------------------+

#ifndef cExtern
  #ifdef __cplusplus
    #define cExtern extern "C"
  #else
    #define cExtern extern 
  #endif
#endif

#ifndef NULL
  #define NULL 0
#endif

cExtern unsigned long stilib_strcpy8(char *dst, unsigned long dst_bytesize, const char *src);
cExtern unsigned long stilib_strcpy16(unsigned short *dst, unsigned long dst_bytesize, const unsigned short *src);
cExtern unsigned long stilib_utf8_check(const char *src);
cExtern unsigned long stilib_utf16_check(const unsigned short *src);
cExtern unsigned long stilib_strupr8(char *src);
cExtern unsigned long stilib_strlwr8(char *src);
cExtern unsigned long stilib_strupr16(unsigned short *src);
cExtern unsigned long stilib_strlwr16(unsigned short *src);
cExtern unsigned long stilib_utf8_to_utf16(unsigned short *pdst_utf16, unsigned long dst_bytesize, const char *psrc_utf8);
cExtern unsigned long stilib_utf8_to_utf16_len(const char *psrc_utf8);
cExtern unsigned long stilib_utf8_to_u8(char *pdst_u8, unsigned long dst_bytesize, const char *psrc_utf8, unsigned char chr_unknown);
cExtern unsigned long stilib_utf8_to_u8_len(const char *psrc_utf8, unsigned char chr_unknown);
cExtern unsigned long stilib_utf8_to_u16(unsigned short *pdst_u16, unsigned long dst_bytesize, const char *psrc_utf8, unsigned short chr_unknown);
cExtern unsigned long stilib_utf8_to_u16_len(const char *psrc_utf8, unsigned short chr_unknown);
cExtern unsigned long stilib_u8_to_utf8(char *pdst_utf8, unsigned long dst_bytesize, const char *psrc_u8);
cExtern unsigned long stilib_u8_to_utf8_len(const char *psrc_u8);
cExtern unsigned long stilib_u8_to_u16(unsigned short *pdst_u16, unsigned long dst_bytesize, const char *psrc_u8);
cExtern unsigned long stilib_u8_to_u16_len(const char *psrc_u8);
cExtern unsigned long stilib_u8_to_utf16(unsigned short *pdst_utf16, unsigned long dst_bytesize, const char *psrc_u8);
cExtern unsigned long stilib_u8_to_utf16_len(const char *psrc_u8);
cExtern unsigned long stilib_utf16_to_utf8(char *pdst_utf8, unsigned long dst_bytesize, const unsigned short *psrc_utf16);
cExtern unsigned long stilib_utf16_to_utf8_len(const unsigned short *psrc_utf16);
cExtern unsigned long stilib_utf16_to_u8(char *pdst_u8, unsigned long dst_bytesize, const unsigned short *psrc_utf16, unsigned char chr_unknown);
cExtern unsigned long stilib_utf16_to_u8_len(const unsigned short *psrc_utf16, unsigned char chr_unknown);
cExtern unsigned long stilib_utf16_to_u16(unsigned short *pdst_u16, unsigned long dst_bytesize, const unsigned short *psrc_utf16, unsigned short chr_unknown);
cExtern unsigned long stilib_utf16_to_u16_len(const unsigned short *psrc_utf16, unsigned short chr_unknown);
cExtern unsigned long stilib_u16_to_utf8(char *pdst_utf8, unsigned long dst_bytesize, const unsigned short *psrc_u16);
cExtern unsigned long stilib_u16_to_utf8_len(const unsigned short *psrc_u16);
cExtern unsigned long stilib_u16_to_u8(char *pdst_u8, unsigned long dst_bytesize, const unsigned short *psrc_u16, unsigned char chr_unknown);
cExtern unsigned long stilib_u16_to_u8_len(const unsigned short *psrc_u16, unsigned char chr_unknown);
cExtern unsigned long stilib_u16_to_utf16(unsigned short *pdst_utf16, unsigned long dst_bytesize, const unsigned short *psrc_u16);
cExtern unsigned long stilib_u16_to_utf16_len(const unsigned short *psrc_u16);

#define stilib_ReplaceChr08 '_'

// +----------------------------------------------------------------------------------------------+
// +----------------------------------------------------------------------------------------------+
// | deeds
// +----------------------------------------------------------------------------------------------+
// +----------------------------------------------------------------------------------------------+

unsigned long stilib_strcpy8(char *dst, unsigned long dst_bytesize, const char *src)
{
  // This function will copy 0-terminated string src to destination
  // --> src ............. String used to copy
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> dst ............. Destination
  // Function will return bytelength of destination string, excluding 0-terminator. e.g. return strlen(dst)
  
  unsigned long retcode = 0;
  if ((dst_bytesize > 0) && (dst != NULL))
  {
    dst_bytesize--; // reserved for final 0
    if(src != NULL)
    {
      while((*src != 0) && (dst_bytesize != 0))
      {
        *dst++ = *src++;
        retcode++;
        dst_bytesize--;
      }
    }
    *dst = 0;
  }
  
  return retcode;
}

unsigned long stilib_strcpy16(unsigned short *dst, unsigned long dst_bytesize, const unsigned short *src)
{
  // This function will copy 0-terminated u16-string src to destination
  // --> src ............. u16 string used to copy
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> dst ............. Destination
  // Function will return wordlength of destination string, excluding 0-terminator. e.g. return strlen16(dst)
  
  unsigned long retcode = 0;
  dst_bytesize = dst_bytesize >> 1; // that's the way i'll do it 
  if ((dst_bytesize > 0) && (dst != NULL))
  {
    dst_bytesize--; // reserved for final 0
    if(src != NULL)
    {
      while((*src != 0) && (dst_bytesize != 0))
      {
        *dst++ = *src++;
        retcode++;
        dst_bytesize--;
      }
    }
    *dst = 0;
  }
  
  return retcode;
}

unsigned long stilib_utf8_check(const char *src)
{
  // This function checks whether a UTF8-coded string is present or not.
  // --> src ............. String to be examined in U8 or UTF8 format
  // The function returns 1 if a UTF8-coded string was passed, otherwise 0.
  // Note: If any in given string encoding does not match UTF8, 0 is returned, even if another UTF8 encoding has already been found.

  unsigned long retcode = 0;
  const unsigned char *ps = (const unsigned char*)src;
  
  if (ps != NULL)
  {
    while (*ps != 0)
    {
      if (*ps & 0x80)
      {
        unsigned char chr = *ps;
        if ((chr & 0xE0) == 0xC0) 
        {
          if ((ps[1] & 0xC0) != 0x80)
          { 
            return 0; // no UTF8
          }
          ps++;
          retcode = 1;
        }
        else if ((chr & 0xF0) == 0xE0)
        {
          if (((ps[1] & 0xC0) != 0x80) || ((ps[2] & 0xC0) != 0x80))
          { 
            return 0; // no UTF8
          }
          ps += 2;
          retcode = 1;
        }
        else if ((chr & 0xF8) == 0xF0)
        {
          if (((ps[1] & 0xC0) != 0x80) || ((ps[2] & 0xC0) != 0x80) || ((ps[3] & 0xC0) != 0x80))
          {
            return 0; // no UTF8
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

unsigned long stilib_utf16_check(const unsigned short *src)
{
  // This function checks whether a UTF16-encoded string is present or not.
  // --> src .... String to be examined in U16 or UTF16 format
  // The function returns 1 if a UTF16 encoded string was passed, otherwise 0.
  // Note: If an encoding does not match UTF16, 0 is returned, even if another UTF16 encoding has already been found in the string.

  unsigned long retcode = 0; // number of used bytes in destination
  unsigned short *ps = (unsigned short*)src;

  if (ps != NULL)
  {
    while (*ps)
    {
      unsigned long chr = *ps++;
      if ((chr & 0xFC00) == 0xD800) // hi-surrogate
      {
        if ((ps[0] & 0xFC00) != 0xDC00) // low-surrogate
        {
          return 0;
        }
        ps++;
        retcode = 1;
      }
    }
  }

  return retcode;
}

unsigned long stilib_strupr8(char *src)
{
  // This function will convert all small characters (a-z) to capital characters (A-Z) in U8 or UTF8-coded string
  // --> src ... 0-terminated u8 string to convert
  // Function will return bytelength of string, excluding 0-terminator. e.g. return strlen(src)
  unsigned long retcode = 0;
  if(src != NULL)
  {
    unsigned char *pa = (unsigned char*)src;
    while(*pa)
    {
      if((*pa >= 'a') && (*pa <= 'z'))
      {
        *pa = *pa - 32;
      }
      retcode++;
      pa++;
    }
  }
  
  return retcode;
}
  
unsigned long stilib_strlwr8(char *src)
{
  // This function will convert all capital characters (A-Z) to small characters (a-z) in U8 or UTF8-coded string
  // --> src ... 0-terminated u8 string to convert
  // Function will return bytelength of string, excluding 0-terminator. e.g. return strlen(src)
  unsigned long retcode = 0;
  if(src != NULL)
  {
    unsigned char *pa = (unsigned char*)src;
    while(*pa)
    {
      if((*pa <= 'Z') && (*pa >= 'A'))
      {
        *pa = *pa + 32;
      }
      retcode++;
      pa++;
    }
  }
  
  return retcode;
}

unsigned long stilib_strupr16(unsigned short *src)
{
  // This function will convert all small characters (a-z) to capital characters (A-Z) in U16 or UTF16-coded string
  // --> src ... 0-terminated u16 string to convert
  // Function will return wordlength of string, excluding 0-terminator. e.g. return strlen16(src)
  unsigned long retcode = 0;
  if(src != NULL)
  {
    unsigned short *pa = (unsigned short*)src;
    while(*pa)
    {
      if((*pa >= 'a') && (*pa <= 'z'))
      {
        *pa = *pa - 32;
      }
      retcode++;
      pa++;
    }
  }
  
  return retcode;
}
  
unsigned long stilib_strlwr16(unsigned short *src)
{
  // This function will convert all capital characters (A-Z) to small characters (a-z) in U16 or UTF16-coded string
  // --> src ... 0-terminated u16 string to convert
  // Function will return wordlength of string, excluding 0-terminator. e.g. return strlen16(src)
  unsigned long retcode = 0;
  if(src != NULL)
  {
    unsigned short *pa = (unsigned short*)src;
    while(*pa)
    {
      if((*pa <= 'Z') && (*pa >= 'A'))
      {
        *pa = *pa + 32;
      }
      retcode++;
      pa++;
    }
  }
  
  return retcode;
}

// +----------------------------------------------------------------------------------------------+
// +----------------------------------------------------------------------------------------------+
// | convert UTF8 to ...
// +----------------------------------------------------------------------------------------------+
// +----------------------------------------------------------------------------------------------+

unsigned long stilib_utf8_to_utf16(unsigned short *pdst_utf16, unsigned long dst_bytesize, const char *psrc_utf8)
{
  // Convert 0-terminated UTF8-coded string to 0-terminated UTF16-coded string
  // --> pdst_utf16 ...... destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_utf8 ....... UTF8-coded and 0-terminated sourcestring
  // Function will return wordlength of UTF16-coded destination string, excluding 0-terminator. e.g. return strlen16(dst)
  // Note: Each glyph out of UTF16-range will be converted to 'stilib_ReplaceChr08'. This cannot happen if given string is in correct UTF8 format.
  //       In case of too small destination, the destination string will be truncated.

  unsigned long retcode = 0; // number of used words in destination
  unsigned long dstsize = dst_bytesize >> 1; // that's the way i'll do it

  if ((dstsize > 0) && (pdst_utf16 != NULL))
  {
    dstsize--; // reserved for final 0
    unsigned char *ps = (psrc_utf8 != NULL)? (unsigned char*)psrc_utf8 : (unsigned char*)"";

    while (*ps != 0)
    {
      unsigned long chr = *ps++;
      if (chr & 0x0080)
      {
        if ((chr & 0x00E0) == 0x00C0)
        {
          if ((ps[0] & 0xC0) == 0x80) // 1 followbyte
          {
            chr = (chr & 0x001F);
            chr = (chr << 6) | (ps[0] & 0x3F);
            ps += 1;
          }
        }
        else if ((chr & 0x00F0) == 0x00E0)
        {
          if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80)) // 2 followbyte
          {
            chr = (chr & 0x000F);
            chr = (chr << 6) | (ps[0] & 0x3F);
            chr = (chr << 6) | (ps[1] & 0x3F);
            ps += 2;
          }
        }
        else if ((chr & 0x00F8) == 0x00F0)
        {
          if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80) && ((ps[2] & 0xC0) == 0x80)) // 3 followbyte
          {
            chr = (chr & 0x0007);
            chr = (chr << 6) | (ps[0] & 0x3F);
            chr = (chr << 6) | (ps[1] & 0x3F);
            chr = (chr << 6) | (ps[2] & 0x3F);
            ps += 3;
          }
        }
      }

      if (chr >= 0xD800) // begin of surrogate
      {
        if (chr <= 0xDFFF) // check high + low surrogate area (0xD800 - 0xDFFF)
        {
          chr = stilib_ReplaceChr08; // invalid
        }
        else if (chr > 0x10FFFF) // check out of range
        {
          chr = stilib_ReplaceChr08; // invalid
        }
      }

      // encode character to utf-16
      if (chr <= 0xFFFF)
      {
        if (dstsize < 1) { break; } // truncate
        *pdst_utf16++ = (unsigned short)chr;
        dstsize -= 1;
        retcode += 1;
      }
      else // if (chr <= 0x10FFFF)
      {
        if (dstsize < 2) { break; } // truncate
        chr -= 0x10000;
        *pdst_utf16++ = (unsigned short)((chr >> 10) | 0xD800); // high-surrogate
        *pdst_utf16++ = (unsigned short)((chr & 0x3FF) | 0xDC00); // low-surrogate
        dstsize -= 2;
        retcode += 2;
      }
    }

    *pdst_utf16 = 0; // 0-terminator
  }

  return retcode;
}

unsigned long stilib_utf8_to_utf16_len(const char *psrc_utf8)
{
  // Compute wordlength of UTF16-coded string created from given UTF8-coded string excluding 0-terminator.
  // --> psrc_utf8 ....... UTF8-coded and 0-terminated sourcestring
  // Function will return wordlength of UTF16-coded destination string, excluding 0-terminator. e.g. return strlen16(dst)

  unsigned long retcode = 0; // number of used words in destination
  unsigned char *ps = (psrc_utf8 != NULL) ? (unsigned char*)psrc_utf8 : (unsigned char*)"";

  while (*ps != 0)
  {
    unsigned long chr = *ps++;
    if (chr & 0x0080)
    {
      if ((chr & 0x00E0) == 0x00C0)
      {
        if ((ps[0] & 0xC0) == 0x80) // 1 followbyte
        {
          chr = (chr & 0x001F);
          chr = (chr << 6) | (ps[0] & 0x3F);
          ps += 1;
        }
      }
      else if ((chr & 0x00F0) == 0x00E0)
      {
        if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80)) // 2 followbyte
        {
          chr = (chr & 0x000F);
          chr = (chr << 6) | (ps[0] & 0x3F);
          chr = (chr << 6) | (ps[1] & 0x3F);
          ps += 2;
        }
      }
      else if ((chr & 0x00F8) == 0x00F0)
      {
        if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80) && ((ps[2] & 0xC0) == 0x80)) // 3 followbyte
        {
          chr = (chr & 0x0007);
          chr = (chr << 6) | (ps[0] & 0x3F);
          chr = (chr << 6) | (ps[1] & 0x3F);
          chr = (chr << 6) | (ps[2] & 0x3F);
          ps += 3;
        }
      }
    }

    if (chr >= 0xD800) // begin of surrogate
    {
      if (chr <= 0xDFFF) // check high + low surrogate area (0xD800 - 0xDFFF)
      {
        chr = stilib_ReplaceChr08; // invalid
      }
      else if (chr > 0x10FFFF) // check out of range
      {
        chr = stilib_ReplaceChr08; // invalid
      }
    }

    // encode character to utf-16
    if (chr <= 0xFFFF)
    {
      retcode += 1;
    }
    else // if (chr <= 0x10FFFF)
    {
      retcode += 2;
    }
  }

  return retcode;
}

unsigned long stilib_utf8_to_u8(char *pdst_u8, unsigned long dst_bytesize, const char *psrc_utf8, unsigned char chr_unknown)
{
  // Convert 0-terminated UTF8-coded string to 0-terminated U8-coded string
  // --> pdst_u8 ......... destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_utf8 ....... UTF8-coded and 0-terminated sourcestring
  // --> chr_unknown ..... each glyphindex outside of destination range will be exchanged to. Give 0 to skip each glyph outside of range.
  // Function will return bytelength of U8-coded destination-string, excluding 0-terminator. e.g. return strlen(dst)
  // Note: In case of too small destination, the destination string will be truncated.

  unsigned long retcode = 0; // number of used bytes in destination
  unsigned char *pd = (unsigned char*)pdst_u8;

  if ((dst_bytesize > 0) && (pd != NULL))
  {
    dst_bytesize--; // reserved for final 0
    unsigned char *ps = (psrc_utf8 != NULL)? (unsigned char*)psrc_utf8 : (unsigned char*)"";

    while ((*ps != 0) && (dst_bytesize !=  0))
    {
      unsigned long chr = *ps++;
      if (chr & 0x0080)
      {
        if ((chr & 0x00E0) == 0x00C0)
        {
          if ((ps[0] & 0xC0) == 0x80) // 1 followbyte
          {
            chr = (chr & 0x001F);
            chr = (chr << 6) | (ps[0] & 0x3F);
            ps += 1;
          }
        }
        else if ((chr & 0x00F0) == 0x00E0)
        {
          if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80)) // 2 followbyte
          {
            chr = (chr & 0x000F);
            chr = (chr << 6) | (ps[0] & 0x3F);
            chr = (chr << 6) | (ps[1] & 0x3F);
            ps += 2;
          }
        }
        else if ((chr & 0x00F8) == 0x00F0)
        {
          if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80) && ((ps[2] & 0xC0) == 0x80)) // 3 followbyte
          {
            chr = (chr & 0x0007);
            chr = (chr << 6) | (ps[0] & 0x3F);
            chr = (chr << 6) | (ps[1] & 0x3F);
            chr = (chr << 6) | (ps[2] & 0x3F);
            ps += 3;
          }
        }
      }

      if(chr <= 0xFF)
      {
        *pd++ = (unsigned char)chr;
        dst_bytesize -= 1;
        retcode += 1;
      }
      else if(chr_unknown != 0)
      {
        *pd++ = chr_unknown;
        dst_bytesize -= 1;
        retcode += 1;
      }
    }

    *pd = 0; // 0-terminator
  }

  return retcode;
}

unsigned long stilib_utf8_to_u8_len(const char *psrc_utf8, unsigned char chr_unknown)
{
  // Compute bytelength of U8-coded string created from given UTF8-coded string excluding 0-terminator.
  // --> psrc_utf8 ....... UTF8-coded and 0-terminated sourcestring
  // --> chr_unknown ..... each glyphindex outside of destination range will be exchanged to. Give 0 to skip each glyph outside of range.
  // Function will return bytelength of U8-coded destination string, excluding 0-terminator. e.g. return strlen(dst)

  unsigned long retcode = 0; // number of used bytes in destination
  unsigned char *ps = (psrc_utf8 != NULL)? (unsigned char*)psrc_utf8 : (unsigned char*)"";

  while (*ps != 0)
  {
    unsigned long chr = *ps++;
    if (chr & 0x0080)
    {
      if ((chr & 0x00E0) == 0x00C0)
      {
        if ((ps[0] & 0xC0) == 0x80) // 1 followbyte
        {
          chr = (chr & 0x001F);
          chr = (chr << 6) | (ps[0] & 0x3F);
          ps += 1;
        }
      }
      else if ((chr & 0x00F0) == 0x00E0)
      {
        if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80)) // 2 followbyte
        {
          chr = (chr & 0x000F);
          chr = (chr << 6) | (ps[0] & 0x3F);
          chr = (chr << 6) | (ps[1] & 0x3F);
          ps += 2;
        }
      }
      else if ((chr & 0x00F8) == 0x00F0)
      {
        if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80) && ((ps[2] & 0xC0) == 0x80)) // 3 followbyte
        {
          chr = (chr & 0x0007);
          chr = (chr << 6) | (ps[0] & 0x3F);
          chr = (chr << 6) | (ps[1] & 0x3F);
          chr = (chr << 6) | (ps[2] & 0x3F);
          ps += 3;
        }
      }
    }

    if((chr <= 0xFF) || (chr_unknown != 0))
    {
      retcode += 1;
    }
  }

  return retcode;
}

unsigned long stilib_utf8_to_u16(unsigned short *pdst_u16, unsigned long dst_bytesize, const char *psrc_utf8, unsigned short chr_unknown)
{
  // Convert 0-terminated UTF8-coded string to 0-terminated U16-coded string
  // --> pdst_u16 ........ destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_utf8 ....... UTF8-coded and 0-terminated sourcestring
  // --> chr_unknown ..... each glyphindex outside of destination range will be exchanged to. Give 0 to skip each glyph outside of range.
  // Function will return wordlength of U16-coded destination-string, excluding 0-terminator. e.g. return strlen16(dst)
  // Note: Each glyph out of U16-range will be converted to 'chr_unknown'.
  //       In case of too small destination, the destination string will be truncated.

  unsigned long retcode = 0; // number of used words in destination
  unsigned long dstsize = dst_bytesize >> 1; // that's the way i'll do it

  if ((dstsize > 0) && (pdst_u16 != NULL))
  {
    dstsize--; // reserved for final 0
    unsigned char *ps = (psrc_utf8 != NULL)? (unsigned char*)psrc_utf8 : (unsigned char*)"";

    while ((*ps != 0) && (dstsize != 0))
    {
      unsigned long chr = *ps++;
      if (chr & 0x0080)
      {
        if ((chr & 0x00E0) == 0x00C0)
        {
          if ((ps[0] & 0xC0) == 0x80) // 1 followbyte
          {
            chr = (chr & 0x001F);
            chr = (chr << 6) | (ps[0] & 0x3F);
            ps += 1;
          }
        }
        else if ((chr & 0x00F0) == 0x00E0)
        {
          if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80)) // 2 followbyte
          {
            chr = (chr & 0x000F);
            chr = (chr << 6) | (ps[0] & 0x3F);
            chr = (chr << 6) | (ps[1] & 0x3F);
            ps += 2;
          }
        }
        else if ((chr & 0x00F8) == 0x00F0)
        {
          if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80) && ((ps[2] & 0xC0) == 0x80)) // 3 followbyte
          {
            chr = (chr & 0x0007);
            chr = (chr << 6) | (ps[0] & 0x3F);
            chr = (chr << 6) | (ps[1] & 0x3F);
            chr = (chr << 6) | (ps[2] & 0x3F);
            ps += 3;
          }
        }
      }

      if (chr >= 0xD800) // begin of surrogate
      {
        if (chr <= 0xDFFF) // check high + low surrogate area (0xD800 - 0xDFFF)
        {
          chr = stilib_ReplaceChr08; // invalid
        }
        else if (chr > 0x10FFFF) // check out of range
        {
          chr = stilib_ReplaceChr08; // invalid
        }
      }

      if(chr <= 0xFFFF)
      {
        *pdst_u16++ = (unsigned short)chr;
        dstsize -= 1;
        retcode += 1;
      }
      else if(chr_unknown != 0)
      {
        *pdst_u16++ = chr_unknown;
        dstsize -= 1;
        retcode += 1;
      }
    }

    *pdst_u16 = 0; // 0-terminator
  }

  return retcode;
}

unsigned long stilib_utf8_to_u16_len(const char *psrc_utf8, unsigned short chr_unknown)
{
  // Compute wordlength of U16-coded string created from given UTF8-coded string excluding 0-terminator.
  // --> psrc_utf8 ....... UTF8-coded and 0-terminated sourcestring
  // --> chr_unknown ..... each glyphindex outside of destination range will be exchanged to. Give 0 to skip each glyph outside of range.
  // Function will return wordlength of U16-coded destination string, excluding 0-terminator. e.g. return strlen16(dst)

  unsigned long retcode = 0; // number of used words in destination
  unsigned char *ps = (psrc_utf8 != NULL)? (unsigned char*)psrc_utf8 : (unsigned char*)"";

  while (*ps != 0)
  {
    unsigned long chr = *ps++;
    if (chr & 0x0080)
    {
      if ((chr & 0x00E0) == 0x00C0)
      {
        if ((ps[0] & 0xC0) == 0x80) // 1 followbyte
        {
          chr = (chr & 0x001F);
          chr = (chr << 6) | (ps[0] & 0x3F);
          ps += 1;
        }
      }
      else if ((chr & 0x00F0) == 0x00E0)
      {
        if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80)) // 2 followbyte
        {
          chr = (chr & 0x000F);
          chr = (chr << 6) | (ps[0] & 0x3F);
          chr = (chr << 6) | (ps[1] & 0x3F);
          ps += 2;
        }
      }
      else if ((chr & 0x00F8) == 0x00F0)
      {
        if (((ps[0] & 0xC0) == 0x80) && ((ps[1] & 0xC0) == 0x80) && ((ps[2] & 0xC0) == 0x80)) // 3 followbyte
        {
          chr = (chr & 0x0007);
          chr = (chr << 6) | (ps[0] & 0x3F);
          chr = (chr << 6) | (ps[1] & 0x3F);
          chr = (chr << 6) | (ps[2] & 0x3F);
          ps += 3;
        }
      }
    }

    if((chr <= 0xFFFF) || (chr_unknown != 0))
    {
      retcode += 1;
    }
  }

  return retcode;
}

// +----------------------------------------------------------------------------------------------+
// +----------------------------------------------------------------------------------------------+
// | convert U8 to ...
// +----------------------------------------------------------------------------------------------+
// +----------------------------------------------------------------------------------------------+

unsigned long stilib_u8_to_utf8(char *pdst_utf8, unsigned long dst_bytesize, const char *psrc_u8)
{
  // Convert 0-terminated U8-coded string to 0-terminated UTF8-coded string
  // --> pdst_utf8 ....... destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_u8 ......... U8-coded and 0-terminated sourcestring
  // Function will return bytelength of UTF8-coded destination-string, excluding 0-terminator. e.g. return strlen(dst)
  // Note: In case of too small destination, the destination string will be truncated.

  unsigned long retcode = 0; // number of used bytes in destination
  unsigned char *pd = (unsigned char*)pdst_utf8;

  if ((dst_bytesize > 0) && (pd != NULL))
  {
    unsigned char *ps = ((psrc_u8 != NULL) ? (unsigned char*)psrc_u8 : (unsigned char*)"");
    dst_bytesize--; // reserved for final 0
    while (*ps != 0)
    {
      unsigned char chr = *ps++;

      // encode glyphindex to utf-8
      if (chr <= 0x7F)
      {
        if (dst_bytesize < 1) { break; } // truncate
        *pd++ = (unsigned char)chr;
        dst_bytesize -= 1;
        retcode += 1;
      }
      else 
      {
        if (dst_bytesize < 2) { break; } // truncate
        *pd++ = 0xC0 | (unsigned char)(chr >> 6);
        *pd++ = 0x80 | (unsigned char)(chr & 0x3F);
        dst_bytesize -= 2;
        retcode += 2;
      }
    }

    *pd = 0; // 0-terminator
  }

  return retcode;
}

unsigned long stilib_u8_to_utf8_len(const char *psrc_u8)
{
  // Compute bytelength of UTF8-coded string created from given U8-coded string excluding 0-terminator.
  // --> psrc_u8 ......... U8-coded and 0-terminated sourcestring
  // Function will return bytelength of UTF8-coded destination string, excluding 0-terminator. e.g. return strlen(dst)

  unsigned long retcode = 0; // number of used bytes in destination
  if (psrc_u8 != NULL)
  {
    unsigned char *ps = (unsigned char*)psrc_u8;
    while (*ps != 0)
    {
      unsigned char chr = *ps++;
      retcode += (chr <= 0x7F)? 1 : 2;
    }
  }
  return retcode;
}

unsigned long stilib_u8_to_u16(unsigned short *pdst_u16, unsigned long dst_bytesize, const char *psrc_u8)
{
  // Convert 0-terminated U8-coded string to 0-terminated U16-coded string
  // --> pdst_u16 ........ destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_u8 ......... U8-coded and 0-terminated sourcestring
  // Function will return wordlength of U16-coded destination string, excluding 0-terminator. e.g. return strlen16(dst)
  // Note: In case of too small destination, the destination string will be truncated.

  unsigned long retcode = 0; // number of used words in destination
  unsigned long dstsize = dst_bytesize >> 1; // that's the way i'll do it  
  
  if ((dstsize > 0) && (pdst_u16 != NULL))
  {
    if (psrc_u8 != NULL)
    {
      dstsize--; // reserved for final 0
      unsigned char *ps = (unsigned char*)psrc_u8;    
      while ((*ps != 0) && (dstsize != 0))
      {
        *pdst_u16++ = *ps++;
        retcode++;
        dstsize--;
      }
    }
    *pdst_u16 = 0; // 0-terminator
  }
  return retcode;
}

unsigned long stilib_u8_to_u16_len(const char *psrc_u8)
{
  // Compute wordlength of U16-coded string created from given U8-coded string excluding 0-terminator.
  // --> psrc_u8 ......... U8-coded and 0-terminated sourcestring
  // Function will return wordlength of U16-coded destination string, excluding 0-terminator. e.g. return strlen16(dst)

  unsigned long retcode = 0; // number of used words in destination
  if (psrc_u8 != NULL)
  {
    while (*psrc_u8 != 0)
    {
      psrc_u8++;
      retcode++;
    }
  }
  return retcode;
}

unsigned long stilib_u8_to_utf16(unsigned short *pdst_utf16, unsigned long dst_bytesize, const char *psrc_u8)
{
  // Convert 0-terminated U8-coded string to 0-terminated UTF16-coded string
  // --> pdst_utf16 ...... destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_u8 ......... U8-coded and 0-terminated sourcestring
  // Function will return wordlength of U16-coded destination-string, excluding 0-terminator. e.g. return strlen16(dst)
  // Note: In case of too small destination, the destination string will be truncated.

  unsigned long retcode = 0; // number of used words in destination
  unsigned long dstsize = dst_bytesize >> 1; // that's the way i'll do it  
  
  if ((dstsize > 0) && (pdst_utf16 != NULL))
  {
    if (psrc_u8 != NULL)
    {
      dstsize--; // reserved for final 0
      unsigned char *ps = (unsigned char*)psrc_u8;
      while ((*ps != 0) && (dstsize != 0))
      {
        *pdst_utf16++ = *ps++;
        retcode++;
        dstsize--;
      }
    }
    *pdst_utf16 = 0; // 0-terminator
  }
  return retcode;
}

unsigned long stilib_u8_to_utf16_len(const char *psrc_u8)
{
  // Compute wordlength of UTF16-coded string created from given U8-coded string excluding 0-terminator.
  // --> psrc_u8 ......... U8-coded and 0-terminated sourcestring
  // Function will return wordlength of UTF16-coded destination string, excluding 0-terminator. e.g. return strlen16(dst)

  unsigned long retcode = 0; // number of used words in destination
  if (psrc_u8 != NULL)
  {
    while (*psrc_u8 != 0)
    {
      psrc_u8++;
      retcode++;
    }
  }
  return retcode;
}

// +----------------------------------------------------------------------------------------------+
// +----------------------------------------------------------------------------------------------+
// | convert UTF16 to ...
// +----------------------------------------------------------------------------------------------+
// +----------------------------------------------------------------------------------------------+

unsigned long stilib_utf16_to_utf8(char *pdst_utf8, unsigned long dst_bytesize, const unsigned short *psrc_utf16)
{
  // Convert 0-terminated UTF16-coded string to 0-terminated UTF8-coded string
  // --> pdst_utf8 ....... destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_utf16 ...... UTF16-coded and 0-terminated sourcestring
  // Function will return bytelength of UTF8-coded destination string, excluding 0-terminator. e.g. return strlen(dst)
  // Note: Each glyph out of UTF8-range will be converted to 'stilib_ReplaceChr08'. This cannot happen if given string is in correct UTF16 format.
  //       In case of too small destination, the destination string will be truncated.

  unsigned long retcode = 0; // number of used bytes in destination
  unsigned char *pd = (unsigned char*)pdst_utf8;

  if ((dst_bytesize > 0) && (pd != NULL))
  {
    if(psrc_utf16 != NULL)
    {
      dst_bytesize--; // reserved for final 0
      while (*psrc_utf16 != 0)
      {
        unsigned long chr = *psrc_utf16++;
        if (chr >= 0xD800) // begin surrogates
        {
          if (chr <= 0xDBFF) // check high-surrogate (0xD800 - 0xDBFF)
          {
            unsigned long low = *psrc_utf16++;
            if ((low >= 0xDC00) && (low <= 0xDFFF)) // check low-surrogate (0xDC00 - 0xDFFF)
            {
              chr = (((chr & 0x03FF) << 10) | (low & 0x03FF)) + 0x10000; // compute glyphindex
            }
            else
            {
              break; // invalid surrogate pair --> truncate
            }
          }
          else if (chr <= 0xDFFF) // check low-surrogate (0xDC00 - 0xDFFF)
          {
            break; // invalid glyphindex --> truncate
          }
        }

        if (chr > 0x10FFFF) // check if glyphindex is out of utf-8 range
        {
          chr = stilib_ReplaceChr08; // glyphindex out of utf-8 range
        }

        // encode glyphindex to utf-8
        if (chr <= 0x7F)
        {
          if (dst_bytesize < 1) { break; } // truncate
          *pd++ = (unsigned char)chr;
          dst_bytesize -= 1;
          retcode += 1;
        }
        else if (chr <= 0x7FF)
        {
          if (dst_bytesize < 2) { break; } // truncate
          *pd++ = 0xC0 | (unsigned char)(chr >> 6);
          *pd++ = 0x80 | (unsigned char)(chr & 0x3F);
          dst_bytesize -= 2;
          retcode += 2;
        }
        else if (chr <= 0xFFFF)
        {
          if (dst_bytesize < 3) { break; } // truncate
          *pd++ = 0xE0 | (unsigned char)(chr >> 12);
          *pd++ = 0x80 | (unsigned char)((chr >> 6) & 0x3F);
          *pd++ = 0x80 | (unsigned char)(chr & 0x3F);
          dst_bytesize -= 3;
          retcode += 3;
        }
        else // if (chr <= 0x10FFFF) // already done
        {
          if (dst_bytesize < 4) { break; } // truncate
          *pd++ = 0xF0 | (unsigned char)(chr >> 18);
          *pd++ = 0x80 | (unsigned char)((chr >> 12) & 0x3F);
          *pd++ = 0x80 | (unsigned char)((chr >> 6) & 0x3F);
          *pd++ = 0x80 | (unsigned char)(chr & 0x3F);
          dst_bytesize -= 4;
          retcode += 4;
        }
      }
    }

    *pd = 0; // 0-terminator
  }

  return retcode;
}

unsigned long stilib_utf16_to_utf8_len(const unsigned short *psrc_utf16)
{
  // Compute bytelength of UTF8-coded string created from given UTF16-coded string excluding 0-terminator.
  // --> psrc_utf16 ...... UTF16-coded and 0-terminated sourcestring
  // Function will return bytelength of UTF8-coded destination string, excluding 0-terminator. e.g. return strlen(dst)

  unsigned long retcode = 0; // number of used bytes in destination
  
  if(psrc_utf16 != NULL)
  {
    while (*psrc_utf16 != 0)
    {
      unsigned long chr = *psrc_utf16++;
      if (chr >= 0xD800) // begin surrogates
      {
        if (chr <= 0xDBFF) // check high-surrogate (0xD800 - 0xDBFF)
        {
          unsigned long low = *psrc_utf16++;
          if ((low >= 0xDC00) && (low <= 0xDFFF)) // check low-surrogate (0xDC00 - 0xDFFF)
          {
            chr = (((chr & 0x03FF) << 10) | (low & 0x03FF)) + 0x10000; // compute glyphindex
          }
          else
          {
            break; // invalid surrogate pair --> truncate
          }
        }
        else if (chr <= 0xDFFF) // check low-surrogate (0xDC00 - 0xDFFF)
        {
          break; // invalid glyphindex --> truncate
        }
      }

      if (chr > 0x10FFFF) // check if glyphindex is out of utf-8 range
      {
        chr = stilib_ReplaceChr08; // glyphindex out of utf-8 range
      }

      // encode glyphindex to utf-8
      if (chr <= 0x7F)
      {
        retcode += 1;
      }
      else if (chr <= 0x7FF)
      {
        retcode += 2;
      }
      else if (chr <= 0xFFFF)
      {
        retcode += 3;
      }
      else // if (chr <= 0x10FFFF) // already done
      {
        retcode += 4;
      }
    }
  }
  return retcode;
}

unsigned long stilib_utf16_to_u8(char *pdst_u8, unsigned long dst_bytesize, const unsigned short *psrc_utf16, unsigned char chr_unknown)
{
  // Convert 0-terminated UTF16-coded string to 0-terminated U8-coded string
  // --> pdst_u8 ......... destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_utf16 ...... UTF-16-coded and 0-terminated sourcestring
  // --> chr_unknown ..... each glyphindex outside of destination range will be exchanged to. Give 0 to skip each glyph outside of range.
  // Function will return bytelength of U8-coded destination-string, excluding 0-terminator. e.g. return strlen(dst)
  // Note: Each glyph out of U8-range will be converted to 'chr_unknown'.
  //       In case of too small destination, the destination string will be truncated.

  unsigned long retcode = 0; // number of used bytes in destination
  unsigned char *pd = (unsigned char*)pdst_u8;

  if ((dst_bytesize > 0) && (pd != NULL))
  {
    if(psrc_utf16 != NULL)
    {
      dst_bytesize--; // reserved for final 0
      while ((*psrc_utf16 != 0) && (dst_bytesize != 0))
      {
        unsigned long chr = *psrc_utf16++;
        if (chr >= 0xD800) // begin surrogates
        {
          if (chr <= 0xDBFF) // check high-surrogate (0xD800 - 0xDBFF)
          {
            unsigned long low = *psrc_utf16++;
            if ((low >= 0xDC00) && (low <= 0xDFFF)) // check low-surrogate (0xDC00 - 0xDFFF)
            {
              chr = (((chr & 0x03FF) << 10) | (low & 0x03FF)) + 0x10000; // compute glyphindex
            }
            else
            {
              break; // invalid surrogate pair --> truncate
            }
          }
          else if (chr <= 0xDFFF) // check low-surrogate (0xDC00 - 0xDFFF)
          {
            break; // invalid glyphindex --> truncate
          }
        }

        if(chr <= 0xFF)
        {
          *pd++ = (unsigned char)chr;
          retcode++;
          dst_bytesize--;
        }
        else if(chr_unknown != 0)
        {
          *pd++ = chr_unknown;
          retcode++;
          dst_bytesize--;
        }
      }
    }
    *pd = 0; // 0-terminator
  }

  return retcode;
}

unsigned long stilib_utf16_to_u8_len(const unsigned short *psrc_utf16, unsigned char chr_unknown)
{
  // Compute bytelength of U8-coded string created from given UTF16-coded string excluding 0-terminator.
  // --> psrc_utf16 ...... UTF16-coded and 0-terminated sourcestring
  // --> chr_unknown ..... each glyphindex outside of destination range will be exchanged to. Give 0 to skip each glyph outside of range.
  // Function will return bytelength of U8-coded destination string, excluding 0-terminator. e.g. return strlen(dst)
  
  unsigned long retcode = 0; // number of used bytes in destination
  if (psrc_utf16 != NULL)
  {
    unsigned short *ps = (unsigned short*)psrc_utf16;
    while (*ps != 0)
    {
      unsigned long chr = *ps++;
      if (chr >= 0xD800) // begin surrogates
      {
        if (chr <= 0xDBFF) // check high-surrogate (0xD800 - 0xDBFF)
        {
          unsigned long low = *ps++;
          if ((low >= 0xDC00) && (low <= 0xDFFF)) // check low-surrogate (0xDC00 - 0xDFFF)
          {
            chr = (((chr & 0x03FF) << 10) | (low & 0x03FF)) + 0x10000; // compute glyphindex
          }
          else
          {
            break; // invalid surrogate pair --> truncate
          }
        }
        else if (chr <= 0xDFFF) // check low-surrogate (0xDC00 - 0xDFFF)
        {
          break; // invalid glyphindex --> truncate
        }
      }

      if((chr <= 0xFF) || (chr_unknown != 0))
      {
        retcode++;
      }
    }
  }

  return retcode;
}

unsigned long stilib_utf16_to_u16(unsigned short *pdst_u16, unsigned long dst_bytesize, const unsigned short *psrc_utf16, unsigned short chr_unknown)
{
  // Convert 0-terminated UTF16-coded string to 0-terminated U16-coded string
  // --> pdst_u16 ........ destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_utf16 ...... UTF16-coded and 0-terminated sourcestring
  // --> chr_unknown ..... each glyphindex outside of destination range will be exchanged to. Give 0 to skip each glyph outside of range.
  // Function will return wordlength of U16-coded destination-string, excluding 0-terminator. e.g. return strlen16(dst)
  // Note: Each glyph out of U16-range will be converted to 'chr_unknown'.
  //       In case of too small destination, the destination string will be truncated.

  unsigned long retcode = 0; // number of used words in destination
  unsigned short *pd = pdst_u16;
  unsigned long dstsize = dst_bytesize >> 1; // that's the way i'll do it

  if ((dstsize > 0) && (pd != NULL))
  {
    if(psrc_utf16 != NULL)
    {
      dstsize--; // reserved for final 0
      while ((*psrc_utf16 != 0) && (dstsize != 0))
      {
        unsigned long chr = *psrc_utf16++;
        if (chr >= 0xD800) // begin surrogates
        {
          if (chr <= 0xDBFF) // check high-surrogate (0xD800 - 0xDBFF)
          {
            unsigned long low = *psrc_utf16++;
            if ((low >= 0xDC00) && (low <= 0xDFFF)) // check low-surrogate (0xDC00 - 0xDFFF)
            {
              chr = (((chr & 0x03FF) << 10) | (low & 0x03FF)) + 0x10000; // compute glyphindex
            }
            else
            {
              break; // invalid surrogate pair --> truncate
            }
          }
          else if (chr <= 0xDFFF) // check low-surrogate (0xDC00 - 0xDFFF)
          {
            break; // invalid glyphindex --> truncate
          }
        }

        if(chr <= 0xFFFF)
        {
          *pd++ = (unsigned short)chr;
          retcode++;
          dstsize--;
        }
        else if(chr_unknown != 0)
        {
          *pd++ = chr_unknown;
          retcode++;
          dstsize--;
        }
      }
    }
    *pd = 0; // 0-terminator
  }

  return retcode;
}

unsigned long stilib_utf16_to_u16_len(const unsigned short *psrc_utf16, unsigned short chr_unknown)
{
  // Compute wordlength of U16-coded string created from given UTF16-coded string excluding 0-terminator.
  // --> psrc_utf16 ...... UTF16-coded and 0-terminated sourcestring
  // --> chr_unknown ..... each glyphindex outside of destination range will be exchanged to. Give 0 to skip each glyph outside of range.
  // Function will return wordlength of U16-coded destination string, excluding 0-terminator. e.g. return strlen16(dst)
  
  unsigned long retcode = 0; // number of used words in destination

  if (psrc_utf16 != NULL)
  {
    while (*psrc_utf16 != 0)
    {
      unsigned long chr = *psrc_utf16++;
      if (chr >= 0xD800) // begin surrogates
      {
        if (chr <= 0xDBFF) // check high-surrogate (0xD800 - 0xDBFF)
        {
          unsigned long low = *psrc_utf16++;
          if ((low >= 0xDC00) && (low <= 0xDFFF)) // check low-surrogate (0xDC00 - 0xDFFF)
          {
            chr = (((chr & 0x03FF) << 10) | (low & 0x03FF)) + 0x10000; // compute glyphindex
          }
          else
          {
            break; // invalid surrogate pair --> truncate
          }
        }
        else if (chr <= 0xDFFF) // check low-surrogate (0xDC00 - 0xDFFF)
        {
          break; // invalid glyphindex --> truncate
        }
      }

      if((chr <= 0xFFFF) || (chr_unknown != 0))
      {
        retcode++;
      }
    }
  }

  return retcode;
}

// +----------------------------------------------------------------------------------------------+
// +----------------------------------------------------------------------------------------------+
// | convert U16 to ...
// +----------------------------------------------------------------------------------------------+
// +----------------------------------------------------------------------------------------------+

unsigned long stilib_u16_to_utf8(char *pdst_utf8, unsigned long dst_bytesize, const unsigned short *psrc_u16)
{
  // Convert 0-terminated U16-coded string to 0-terminated UTF8-coded string
  // --> pdst_utf8 ....... destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_u16 ........ U16-coded and 0-terminated sourcestring
  // Function will return bytelength of UTF8-coded destination string, excluding 0-terminator. e.g. return strlen(dst)
  // Note: In case of too small destination, the destination string will be truncated.

  unsigned long retcode = 0; // number of used bytes in destination
  unsigned char *pd = (unsigned char*)pdst_utf8;

  if ((dst_bytesize > 0) && (pd != NULL))
  {
    if(psrc_u16 != NULL)
    {
      dst_bytesize--; // reserved for final 0
      while (*psrc_u16 != 0)
      {
        unsigned short chr = *psrc_u16++;

        // encode glyphindex to utf-8
        if (chr <= 0x7F)
        {
          if (dst_bytesize < 1) { break; } // truncate
          *pd++ = (unsigned char)chr;
          dst_bytesize -= 1;
          retcode += 1;
        }
        else if (chr <= 0x7FF)
        {
          if (dst_bytesize < 2) { break; } // truncate
          *pd++ = 0xC0 | (unsigned char)(chr >> 6);
          *pd++ = 0x80 | (unsigned char)(chr & 0x3F);
          dst_bytesize -= 2;
          retcode += 2;
        }
        else // if (chr <= 0xFFFF) // always
        {
          if (dst_bytesize < 3) { break; } // truncate
          *pd++ = 0xE0 | (unsigned char)(chr >> 12);
          *pd++ = 0x80 | (unsigned char)((chr >> 6) & 0x3F);
          *pd++ = 0x80 | (unsigned char)(chr & 0x3F);
          dst_bytesize -= 3;
          retcode += 3;
        }
      }
    }
    
    *pd = 0; // 0-terminator
  }

  return retcode;
}

unsigned long stilib_u16_to_utf8_len(const unsigned short *psrc_u16)
{
  // Compute bytelength of UTF8-coded string created from given U16-coded string excluding 0-terminator.
  // --> psrc_u16 ........ U16-coded and 0-terminated sourcestring
  // Function will return bytelength of UTF8-coded destination string, excluding 0-terminator. e.g. return strlen(dst)

  unsigned long retcode = 0; // number of used bytes in destination

  if (psrc_u16 != NULL)
  {
    while (*psrc_u16 != 0)
    {
      unsigned short chr = *psrc_u16++;

      // encode glyphindex to utf-8
      if (chr <= 0x7F)
      {
        retcode += 1;
      }
      else if (chr <= 0x7FF)
      {
        retcode += 2;
      }
      else // if (chr <= 0xFFFF) // always
      {
        retcode += 3;
      }
    }
  }

  return retcode;
}

unsigned long stilib_u16_to_u8(char *pdst_u8, unsigned long dst_bytesize, const unsigned short *psrc_u16, unsigned char chr_unknown)
{
  // Convert 0-terminated U16-coded string to 0-terminated U8-coded string
  // --> pdst_u8 ......... destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_u16 ........ U16-coded and 0-terminated sourcestring
  // --> chr_unknown ..... each glyphindex outside of destination range will be exchanged to. Give 0 to skip each glyph outside of range.
  // Function will return bytelength of U8-coded destination string, excluding 0-terminator. e.g. return strlen(dst)
  // Note: In case of too small destination, the destination string will be truncated.

  unsigned long retcode = 0; // number of used bytes in destination
  unsigned char *pd = (unsigned char*)pdst_u8;

  if ((dst_bytesize > 0) && (pd != NULL))
  {
    if (psrc_u16 != NULL)
    {
      const unsigned short *ps = psrc_u16; 
      dst_bytesize--; // reserved for final 0
      while ((*ps != 0) && (dst_bytesize != 0))
      {
        unsigned short chr = *ps++;
        if(chr <= 0xFF)
        {
          *pd++ = (unsigned char)chr;
          retcode++;
          dst_bytesize--;
        }
        else if(chr_unknown != 0)
        {
          *pd++ = chr_unknown;
          retcode++;
          dst_bytesize--;
        }
      }
    }
    
    *pd = 0; // 0-terminator
  }

  return retcode;
}

unsigned long stilib_u16_to_u8_len(const unsigned short *psrc_u16, unsigned char chr_unknown)
{
  // Compute bytelength of U8-coded string created from given U16-coded string excluding 0-terminator.
  // --> psrc_u16 ........ U16-coded and 0-terminated sourcestring
  // --> chr_unknown ..... each glyphindex outside of destination range will be exchanged to. Give 0 to skip each glyph outside of range.
  // Function will return bytelength of U8-coded destination string, excluding 0-terminator. e.g. return strlen(dst)
  
  unsigned long retcode = 0; // number of used bytes in destination

  if (psrc_u16 != NULL)
  {
    while (*psrc_u16 != 0)
    {
      if((*psrc_u16 <= 0xFF) || (chr_unknown != 0))
      {
        retcode++;
      }  
      psrc_u16++;
    }
  }

  return retcode;
}

unsigned long stilib_u16_to_utf16(unsigned short *pdst_utf16, unsigned long dst_bytesize, const unsigned short *psrc_u16)
{
  // Convert 0-terminated U16-coded string to 0-terminated UTF16-coded string
  // --> pdst_utf16 ...... destinationbuffer
  // --> dst_bytesize .... bytesize of destinationbuffer
  // --> psrc_u16 ........ U16-coded and 0-terminated sourcestring
  // Function will return wordlength of UTF16-coded destination string, excluding 0-terminator. e.g. return strlen16(dst)
  // Note: In case of too small destination, the destination string will be truncated.
  
  unsigned long retcode = 0; // number of used words in destination
  unsigned long dstsize = dst_bytesize >> 1; // that's the way i'll do it

  if ((dstsize > 0) && (pdst_utf16 != NULL))
  {
    if (psrc_u16 != NULL)
    {
      dstsize--; // reserved for final 0
      const unsigned short *ps = psrc_u16;

      while ((*ps != 0) && (dstsize != 0))
      {
        unsigned short chr = *ps++;

        if ((chr >= 0xD800) && (chr <= 0xDFFF)) // check surrogate range
        {
          chr = stilib_ReplaceChr08; // invalid
        }

        *pdst_utf16++ = chr;
        dstsize -= 1;
        retcode += 1;
      }

      *pdst_utf16 = 0; // 0-terminator
    }
  }
  
  return retcode;
}

unsigned long stilib_u16_to_utf16_len(const unsigned short *psrc_u16)
{
  // Compute wordlength of UTF16-coded string created from given U16-coded string excluding 0-terminator.
  // --> psrc_u16 ........ U16-coded and 0-terminated sourcestring
  // Function will return wordlength of UTF16-coded destination string, excluding 0-terminator. e.g. return strlen16(dst)

  unsigned long retcode = 0; // number of used words in destination

  if (psrc_u16 != NULL)
  {
    while (*psrc_u16 != 0)
    {
      psrc_u16++;
      retcode += 1;
    }
  }
  
  return retcode;
}



