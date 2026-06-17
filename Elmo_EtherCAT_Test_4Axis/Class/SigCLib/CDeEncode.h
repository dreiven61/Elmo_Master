// +----------------------------------------------------------------------------------------------+
// +-[   copyright ] Sigmatek GmbH & CoKG                                                         |
// +-[      author ] kolott                                                                       |
// +-[        date ] 26.07.2019                                                                   |
// +-[ description ]------------------------------------------------------------------------------+
// |                                                                                              |
// |                                                                                              |
// +----------------------------------------------------------------------------------------------+

#ifndef _cDeEncodeH
 #define _cDeEncodeH
  
 #include "DefineCompiler.h" 
  
 #ifdef cCompile // *******************************************************************************
 
    // convert unicode-0-string to utf8-0-string, function will return number of used bytes in destination, excluding final zero
    cExtern unsigned long sigclib_str16_to_utf8(void *dst_utf8, const void *src_uni);
    
    // convert utf8-0-string to unicode-0-string, function will return number of characters in destination
    cExtern unsigned long sigclib_utf8_to_str16(void *dst_uni, const void *src_utf8);

    // convert utf8-0-string to unicode-0-string, function will return number of characters in destination
    cExtern unsigned long sigclib_utf8_to_str16_crop(void *dst_uni, unsigned long dst_bytesize, const void *src_utf8);

    // convert ascii-0-string to utf8-0-string, function will return number of used bytes in destination, excluding final zero
    cExtern unsigned long sigclib_ascii_to_utf8(void *dst_utf8, const void *src_ascii);

    // convert ascii-0-string to utf8-0-string, function will return number of used bytes in destination, excluding final zero
    cExtern unsigned long sigclib_ascii_to_utf8_crop(void *dst_utf8, unsigned long dst_bytesize, const void *src_ascii);
    
    // convert utf8-0-string to ascii-0-string, function will return number of characters in destination
    cExtern unsigned long sigclib_utf8_to_ascii(void *dst_ascii, const void *src_utf8);

    // convert utf8-0-string to ascii-0-string, function will return number of characters in destination
    cExtern unsigned long sigclib_utf8_to_ascii_crop(void *dst_ascii, unsigned long dst_bytesize, const void *src_utf8, const char unknown_chr='?');

    // convert UTF8 to UTF16
    cExtern unsigned long sigclib_utf8_to_utf16(unsigned short *pdst_utf16, unsigned long dst_bytesize, const char *psrc_utf8, unsigned long no);

    // check if given 0-terminated string in utf8-format
    cExtern unsigned long sigclib_utf8_check(const void *src);

    // convert UTF16 to UTF8
    cExtern unsigned long sigclib_utf16_to_utf8(unsigned char *pdst_utf8, unsigned long dst_bytesize, unsigned short *psrc_utf16, unsigned long no);

    // compute amount of bytes (exclusive 0-terminator) used in utf-8 formatted string when ascii-0-String is given
    cExtern unsigned long sigclib_ascii_to_utf8_len(const void *src_ascii);
    
    // compute amount of bytes (exclusive 0-terminator) used in ascii-formatted string when utf8-0-String is given
    cExtern unsigned long sigclib_utf8_to_ascii_len(const void *src_utf8);
    
    // compute amount of characters used in str16-formatted string when utf8-0-String is given
    cExtern unsigned long sigclib_utf8_to_str16_len(const void *src_utf8);
    
    // compute amount of bytes (exclusive 0-terminator) used in utf-8 formatted string when UNICODE-0-String is given
    cExtern unsigned long sigclib_str16_to_utf8_len(const void *src_uni);

    // compute size of base64 encoded data
    cExtern unsigned long sigclib_base64_encode_size(unsigned long srcsize);

    // convert data by using Base64 encoding 
    cExtern unsigned long sigclib_base64_encode(unsigned char *dst, unsigned long dstsize, void *src0, unsigned long srcsize);
    
    // decode base64 encoded data
    cExtern unsigned long sigclib_base64_decode(void *dst0, unsigned long dstsize, const unsigned char *src, unsigned long srcsize);
 
    // inplace encode of binary data
    cExtern void sigclib_encode_bin(void *p0, unsigned long bytesize, unsigned long key0, unsigned long key1 = 0);
    
    // inplace decode of binary data
    cExtern void sigclib_decode_bin(void *p0, unsigned long bytesize, unsigned long key0, unsigned long key1 = 0);

    // inplace encode of textual data
    cExtern void sigclib_encode_txt(void *p0, unsigned long bytesize, unsigned long key);

    // inplace decode of textual data
    cExtern void sigclib_decode_txt(void *p0, unsigned long bytesize, unsigned long key);
    
    // parse U64 value out of ascii-string 
    cExtern _uint32 sigclib_parse_U64(const char **pp, _uint64 *pval64);
    
    // parse S64 value out of ascii-string 
    cExtern _uint32 sigclib_parse_S64(const char **pp, _int64 *pval64);
    
    // parse F64 value out of ascii-string 
    cExtern _uint32 sigclib_parse_F64(const char **pp, _f64 *pfval64);
    
    // parse U32 value out of ascii-string 
    cExtern _uint32 sigclib_parse_U32(const char **pp, _uint32 *pval32);
    
    // parse S32 value out of ascii-string 
    cExtern _uint32 sigclib_parse_S32(const char **pp, _int32 *pval32);
    
    // parse F32 value out of ascii-string 
    cExtern _uint32 sigclib_parse_F32(const char **pp, _f32 *pfval);
    
    // parse if next character in ascii-string is of any chr of string 'str'
    cExtern _uint32 sigclib_parse_IsChrAny(const char **pp, const char *str, _uint32 case_sensitive);
    
    // parse and skip all characters given with 'skip' in ascii-string
    cExtern _uint32 sigclib_parse_Skip(const char **pp, const char *skip, _uint32 case_sensitive);
    
    // parse if next token starts with 'searchee'
    cExtern _uint32 sigclib_parse_IsToken(const char **pp, const char *searchee, _uint32 solid_token, _uint32 case_sensitive);

    // parse text between e.g. "()"
    cExtern _uint32 sigclib_parse_TextIntermediate(const char **pp, char *pd, _uint32 sizepd, const char *between);

    // parse if next token is of numeric type in ascii-string
    cExtern _uint32 sigclib_parse_IsNum(const char *pl, _uint32 just_unsigned);

    // parse if end of ascii-string reached
    cExtern _uint32 sigclib_parse_IsEOL(const char *pl);

    // parse IPv4 address + port (10.20.30.40:5060) out of ascii-string
    cExtern _uint32 sigclib_parse_IpAddress(const char **pp, _uint32 *paddr, _uint32 *pport);
    
    // parse date out of ascii-string. Format 2025.5.14 or 14.05.2025
    cExtern _uint32 sigclib_parse_Date(const char **pp, _uint32 *pdate);
    
    // parse time out of ascii-string. Format 13:00:04, 13:00, 11:05pm
    cExtern _uint32 sigclib_parse_Time(const char **pp, _uint32 *ptime);
    
    // parse RegularExpression out of ascii-string
    cExtern _uint32 sigclib_parse_RegularExpression(const char **pp, char *pd, _uint32 sizepd);

    // parse Token till any caharcter of 'separator' occures
    cExtern _uint32 sigclib_parse_Token(const char **pp, char *pd, _uint32 sizepd, const char *separator);
 
 
 #else // *****************************************************************************************
    // convert unicode-0-string to utf8-0-string, function will return number of used bytes in destination, excluding final zero
    function global __cdecl sigclib_str16_to_utf8 var_input dst:^void; src:^void; end_var var_output retcode:udint; end_var;
    
    // convert utf8-0-string to unicode-0-string, function will return number of characters in destination
    function global __cdecl sigclib_utf8_to_str16 var_input dst:^void; src:^void; end_var var_output retcode:udint; end_var;

    // convert ascii-0-string to utf8-0-string, function will return number of used bytes in destination, excluding final zero
    function global __cdecl sigclib_ascii_to_utf8 var_input dst:^void; src:^void; end_var var_output retcode:udint; end_var;

    // convert ascii-0-string to utf8-0-string, function will return number of used bytes in destination, excluding final zero
    function global __cdecl sigclib_ascii_to_utf8_crop var_input dst:^void; dst_bytesize:udint; src:^void; end_var var_output retcode:udint; end_var;
    
    // convert utf8-0-string to unicode-0-string, function will return number of characters in destination
    function global __cdecl sigclib_utf8_to_str16_crop var_input dst:^void; dst_bytesize:udint; src:^void; end_var var_output retcode:udint; end_var;
    
    // convert utf8-0-string to ascii-0-string, function will return number of characters in destination
    function global __cdecl sigclib_utf8_to_ascii var_input dst:^void; src:^void; end_var var_output retcode:udint; end_var;

    // convert utf8-0-string to ascii-0-string, function will return number of characters in destination
    function global __cdecl sigclib_utf8_to_ascii_crop var_input dst:^void; dst_bytesize:udint; src:^void; unknown_chr:char:='?'; end_var var_output retcode:udint; end_var;

    // convert UTF8 to UTF16
    function global __cdecl sigclib_utf8_to_utf16 var_input pdst0:^uint; dst_bytesize:udint; psrc0:^char; no:udint; end_var var_output retcode:udint; end_var;

    // check if given 0-terminated string is in utf8-format
    function global __cdecl sigclib_utf8_check var_input src:^void; end_var var_output retcode : udint; end_var;

    // convert UTF16 to UTF8
    function global __cdecl sigclib_utf16_to_utf8 var_input pdst0:^char; dst_bytesize:udint; psrc0:^uint; no:udint; end_var var_output retcode:udint; end_var;

    // compute amount of bytes (exclusive 0-terminator) used in utf-8 formatted string when ascii-0-String is given
    function global __cdecl sigclib_ascii_to_utf8_len var_input src_ascii:^void; end_var var_output retcode:udint; end_var;
    
    // compute amount of characters/bytes (exclusive 0-terminator) used in ascii-formatted string when utf8-0-String is given
    function global __cdecl sigclib_utf8_to_ascii_len var_input src_utf8:^void; end_var var_output retcode:udint; end_var;
    
    // compute amount of characters used in str16-formatted string when utf8-0-String is given
    function global __cdecl sigclib_utf8_to_str16_len var_input src_utf8:^void; end_var var_output retcode:udint; end_var;
    
    // compute amount of bytes (exclusive 0-terminator) used in utf-8 formatted string when UNICODE-0-String is given
    function global __cdecl sigclib_str16_to_utf8_len var_input src_uni:^void; end_var var_output retcode:udint; end_var;

    // compute size of base64 encoded data
    function global __cdecl sigclib_base64_encode_size var_input srcsize:udint; end_var var_output retcode:udint; end_var;
 
    // convert data by using Base64 encoding 
    function global __cdecl sigclib_base64_encode var_input dst:^usint; dstsize:udint; src0:^void; srcsize:udint; end_var var_output retcode:udint; end_var;
    
    // decode base64 encoded data
    function global __cdecl sigclib_base64_decode var_input dst0:^void; dstsize:udint; src:^usint; srcsize:udint; end_var var_output retcode:udint; end_var;

    // inplace encode of binary data
    function global __cdecl sigclib_encode_bin var_input p0:^void; bytesize:udint; key:udint; key1:udint:=0; end_var;
    
    // inplace decode of binary data
    function global __cdecl sigclib_decode_bin var_input p0:^void; bytesize:udint; key:udint; key1:udint:=0; end_var;
 
    // inplace encode of textual data
    function global __cdecl sigclib_encode_txt var_input p0:^void; bytesize:udint; key:udint; end_var;

    // inplace decode of textual data
    function global __cdecl sigclib_decode_txt var_input p0:^void; bytesize:udint; key:udint; end_var;


    // parse F64 value out of ascii-string 
    function global __cdecl sigclib_parse_F64 var_input pp:^pvoid; pfval:^lreal; end_var var_output retcode:udint; end_var;
    
    // parse U64 value out of ascii-string 
    function global __cdecl sigclib_parse_U64 var_input pp:^pvoid; pfval:^void; end_var var_output retcode:udint; end_var;
    
    // parse S64 value out of ascii-string 
    function global __cdecl sigclib_parse_S64 var_input pp:^pvoid; pfval:^void; end_var var_output retcode:udint; end_var;

    // parse U32 value out of ascii-string 
    function global __cdecl sigclib_parse_U32 var_input pp:^pvoid; pval32:^udint; end_var var_output retcode:udint; end_var;
    
    // parse S32 value out of ascii-string 
    function global __cdecl sigclib_parse_S32 var_input pp:^pvoid; pval32:^dint; end_var var_output retcode:udint; end_var;
    
    // parse F32 value out of ascii-string 
    function global __cdecl sigclib_parse_F32 var_input pp:^pvoid; pfval:^real; end_var var_output retcode:udint; end_var;
    
    // parse if next character in ascii-string is of any chr of string 'str'
    function global __cdecl sigclib_parse_IsChrAny var_input pp:^pvoid; str:^char; case_sensitive:udint; end_var var_output retcode:udint; end_var;
    
    // parse and skip all characters given with 'skip' in ascii-string
    function global __cdecl sigclib_parse_Skip var_input pp:^pvoid; skip:^char; case_sensitive:udint; end_var var_output retcode:udint; end_var;
    
    // parse if next token starts with 'searchee'
    function global __cdecl sigclib_parse_IsToken var_input pp:^pvoid; searchee:^char; solid_token:udint; case_sensitive:udint; end_var var_output retcode:udint; end_var;

    // parse text between e.g. "()"
    function global __cdecl sigclib_parse_TextIntermediate var_input pp:^pvoid; pd:^char; sizepd:udint; between:^char; end_var var_output retcode:udint; end_var;

    // parse if next token is of numeric type in ascii-string
    function global __cdecl sigclib_parse_IsNum var_input pl:^void; just_unsigned:udint; end_var var_output retcode:udint; end_var;

    // parse if end of ascii-string reached
    function global __cdecl sigclib_parse_IsEOL var_input pl:^void; end_var var_output retcode:udint; end_var;
        
    // parse IPv4 address + port (10.20.30.40:5060) out of ascii-string
    function global __cdecl sigclib_parse_IpAddress var_input pp:^pvoid; paddr:^udint; pport:^udint; end_var var_output retcode:udint; end_var;
    
    // parse date out of ascii-string. Format 2025.5.14 or 14.05.2025
    function global __cdecl sigclib_parse_Date var_input pp:^pvoid; pdate:^udint; end_var var_output retcode:udint; end_var;
    
    // parse time out of ascii-string. Format 13:00:04, 13:00, 11:05pm
    function global __cdecl sigclib_parse_Time var_input pp:^pvoid; ptime:^udint; end_var var_output retcode:udint; end_var;
    
    // parse RegularExpression out of ascii-string
    function global __cdecl sigclib_parse_RegularExpression var_input pp:^pvoid; pd:^char; sizepd:udint; end_var var_output retcode:udint; end_var;
 
    // parse Token till any caharcter of 'separator' occures
    function global __cdecl sigclib_parse_Token var_input pp:^pvoid; pd:^char; sizepd:udint; separator:^char; end_var var_output retcode:udint; end_var;
 
 
 #endif // ****************************************************************************************
#endif



// ************************************************************************************************
// USAGE
// ************************************************************************************************

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_str16_to_utf8(void *dst_utf8, const void *src_uni);
// convert unicode-0-string to utf8-0-string
// --> dst_utf8 ........ destinationbuffer (utf8-0-string)
// --> src_uni ......... sourcedata (unicode-0-string)
// <-- function will return number of used bytes in destination, excluding final zero
    
// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_utf8_to_str16(void *dst_uni, const void *src_utf8);
// convert ascii or utf8 coded 0 terminated string to unicode-0-string
// --> dst_uni ......... destinationbuffer
// --> src_utf8 ........ sourcedata (utf8- or ascii-0-string)
// <-- function will return number of characters in destination, excluding final zero
// NOTE: Each non representable glyph will be exchanged by '?'

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_utf8_to_str16_crop(void *dst_uni, unsigned long dst_bytesize, const void *src_utf8);
// convert ascii or utf8 coded 0 terminated string to unicode-0-string. If destination is not big enough, converted string will be cropped to dst_bytesize.
// --> dst_uni ......... destinationbuffer
// --> dst_bytesize .... bytesize of destinationbuffer
// --> src_utf8 ........ sourcedata (utf8- or ascii-0-string)
// <-- function will return number of characters in destination, excluding final zero
// NOTE: Each non representable glyph will be exchanged by '?'

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_ascii_to_utf8(void *dst_utf8, const void *src_ascii);
// convert ascii-0-string to utf8-0-string
// --> dst_utf8 ........ destinationbuffer (utf8-0-string)
// --> src_ascii ....... sourcedata (ascii-0-string)
// <-- function will return number of used bytes in destination, excluding final zero

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_ascii_to_utf8_crop(void *dst_utf8, unsigned long dst_bytesize, const void *src_ascii);
// convert ascii-0-string to utf8-0-string. If destination is not big enough, converted string will be cropped to dst_bytesize.
// --> dst_utf8 ........ destinationbuffer (utf8-0-string)
// --> dst_bytesize .... bytesize of destinationbuffer
// --> src_ascii ....... sourcedata (ascii-0-string)
// <-- function will return number of used bytes in destination, excluding final zero
    
// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_utf8_to_ascii(void *dst_ascii, const void *src_utf8);
// convert utf8-0-string to ascii-0-string
// --> dst_ascii ....... destinationbuffer
// --> src_utf8 ........ sourcedata (utf8-0-string)
// <-- function will return number of characters in destination
// NOTE: Each non representable glyph will be exchanged by '?'

// ------------------------------------------------------------------------------------------------
// cExtern unsigned long sigclib_utf8_to_ascii_crop(void *dst_ascii, unsigned long dst_bytesize, const void *src_utf8, const char unknown_chr='?');
// convert utf8-0-string to ascii-0-string
// --> dst_ascii ....... destinationbuffer
// --> dst_bytesize .... bytesize of destinationbuffer
// --> src_utf8 ........ sourcedata (utf8-0-string)
// --> unknown_chr ..... Each non representable glyph will be exchanged to. 0 Is a valid number as well. In that case each non representable glyph will not be transfered.
// <-- function will return number of characters in destination
// NOTE: Each non representable glyph will be exchanged by 'unknown_chr'

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_utf8_check(const void *src);
// check if given 0-terminated string is in UTF8-format
// --> src ............. given 0-terminated string in ascii or UTF8 format
// <-- function will return 1 when UTF8 coding is detected, on the other hand 0

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_utf8_to_ascii_len(const void *src_utf8);
// compute amount of characters/bytes (exclusive 0-terminator) used in ascii-formatted string when utf8-0-String is given
// --> src_utf8 ........ source, 0-terminated-UTF8-string to compute
// Function will return length of ascii-coded string after conversion from UTF8.

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_utf8_to_str16_len(const void *src_utf8);
// compute amount of characters used in str16-formatted string when utf8-0-String or ascii-string is given
// --> src_utf8 ........ source, 0-terminated-UTF8-string to compute
// Function will return length of U16-coded string after conversion from UTF8.

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_ascii_to_utf8_len(const void *src_ascii);
// compute amount of bytes (exclusive 0-terminator) used in utf-8 formatted string when ascii-0-String is given
// --> src_ascii ....... source, 0-terminated-ascii-string to compute
// Function will return length of UTF8-coded destinationstring. Returnvalue is equal to result of sigclib_strlen(utf8)

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_str16_to_utf8_len(const void *src_uni);
// compute amount of bytes (exclusive 0-terminator) used in utf-8 formatted string UNICODE-0-String is given
// --> src_uni ......... source, 0-terminated-unicode-string to compute
// --> chrsize ......... Size of Single Character in Source. 1= ASCII-0-String, 2=UNICODE-0-String
// Function will return length of UTF8-coded destinationstring. Returnvalue is equal to result of sigclib_strlen(utf8)

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_utf16_to_utf8(unsigned char *pdst_utf8, unsigned long dst_bytesize, unsigned short *psrc_utf16, unsigned long no)
// convert UTF16 to UTF8
// --> pdst_utf8 ....... destinationbuffer
// --> dst_bytesize .... bytesize of destinationbuffer
// --> psrc_utf16 ...... UTF16-coded sourcebuffer
// --> no .............. length of UTF16-coded sourcebuffer
// function will return length of UTF8-codec destinationstring or 0 in case of error (destinationbuffer too small, format-error in sourcebuffer)

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_utf8_to_utf16(unsigned short *pdst_utf16, unsigned long dst_bytesize, const char *psrc_utf8, unsigned long no)
// convert UTF8 to UTF16
// --> pdst_utf16 ...... destinationbuffer
// --> dst_bytesize .... bytesize of destinationbuffer
// --> psrc_utf8 ....... UTF8-coded sourcebuffer
// --> no .............. length UTF8-coded sourcebuffer
// function will return length of UTF16-coded destinationstring or 0 in case of error (destinationbuffer too small, format-error in sourcebuffer)

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_base64_encode_size(unsigned long srcsize);
// compute size of base64 encoded data
// --> srzsize ......... size in bytes of data to encode
// <-- function will return number of bytes used for encoded data

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_base64_encode(unsigned char *dst, unsigned long dstsize, void *src0, unsigned long srcsize);
// Encodes the given data with Base64.
// Encoding is used to make binary data survive transport through transport layers that are not 8-bit clean, eg mail bodies.
// --> dst ............. destimationbubffer where encoded data should be filed
// --> dstsize ......... bytesize of destinatonbuffer
// --> src0 ............ soutcedata to encode
// --> srcsize ......... size of soutcedata to encode in byte
// <-- function will return amount of encoded bytes, in case of error 0 (destination buffer too small)
// NOTE: encoded data needs about 133% of the original data size. Therefore, the destination nuffer must be made available accordingly.

// ------------------------------------------------------------------------------------------------
// unsigned long sigclib_base64_decode(void *dst0, unsigned long dstsize, const unsigned char *src, unsigned long srcsize);
// Decodes from Base64 given data.
// --> dst0 ............ destimationbubffer where encoded data should be filed
// --> dstsize ......... bytesize of destinaitonbuffer
// --> src ............. Base64 encoded soutcedata to decode
// --> srcsize ......... length of soutcedata to decode
// <-- function will return amount of decoded bytes, in case of error 0 (wrong src-data or destination buffer too small)

// ------------------------------------------------------------------------------------------------
// void sigclib_encode_bin(void *p0, unsigned long bytesize, unsigned long key);
// inplace encode of binary data
// --> p0 .............. data to encode
// --> bytesize ........ size of bytes to encode    
// --> key0, key1 ...... arbitrary userkeys (necessary to decode)
// NOTE: data can be decoded by using function sigclib_decode_bin() with same bytelength and keys
   
// ------------------------------------------------------------------------------------------------
// void sigclib_decode_bin(void *p0, unsigned long bytesize, unsigned long key);
// inplace decode of binary data, encoded by using function sigclib_encode_bin()
// --> p0 .............. data to decode
// --> bytesize ........ size of bytes to decode    
// --> key0, key1 ...... arbitrary userkey (same value used to encode)

// ------------------------------------------------------------------------------------------------
// void sigclib_encode_txt(void *p0, unsigned long bytesize, unsigned long key);
// inplace encode of text-data (just ASCII 32 - ASCII 126 characters will be affected by this function)
// --> p0 .............. data to encode
// --> bytesize ........ size of bytes to encode    
// --> key ............. arbitrary userkey (necessary to decode)
// NOTE: data can be decoded by using function sigclib_decode_txt() with same bytelength and key
   
// ------------------------------------------------------------------------------------------------
// void sigclib_decode_txt(void *p0, unsigned long bytesize, unsigned long key);
// inplace decode of text-data, encoded by using function sigclib_encode_txt()
// --> p0 .............. data to decode
// --> bytesize ........ size of bytes to decode    
// --> key ............. arbitrary userkey (same value used to encode)

// ------------------------------------------------------------------------------------------------
// parse 64Bit unsigned value out of ascii-string. All preceeding [SPACE] and [TAB] will be ignored.
// _uint32 sigclib_parse_U64(const char **pp, _uint64 *pval64);
// --> pp .............. address of pointer to ascii-string in use
// --> pval64 .......... pointer where parsed 64bit unsigned value should be filed
// function will return 1 on success, on the other hand 0
// NOTE: in case of success given pointer will be set right after parsed token

// ------------------------------------------------------------------------------------------------
// parse 64Bit signed value out of ascii-string. All preceeding [SPACE] and [TAB] will be ignored.
// _uint32 sigclib_parse_S64(const char **pp, _int64 *pval64);
// --> pp .............. address of pointer to ascii-string in use
// --> pval64 .......... pointer where parsed signed 64bit value should be filed
// function will return 1 on success, on the other hand 0
// NOTE: in case of success given pointer will be set right after parsed token
    
// ------------------------------------------------------------------------------------------------
// parse 64Bit floating point value out of ascii-string. All preceeding [SPACE] and [TAB] will be ignored.
// NOTE: decimal inclusive exponent spelling and hexadecimal format is supported (e.g.: 0xFF, 123, 123.456, -123.456, +123.456, 1.9e-02, 1.9E+2, .456)
// _uint32 sigclib_parse_F64(const char **pp, _f64 *pfval64);
// --> pp .............. address of pointer to ascii-string in use
// --> pfval64 ......... pointer where parsed 64bit floating point value should be filed
// function will return 1 on success, on the other hand 0
// NOTE: in case of success given pointer will be set right after parsed token
    
// ------------------------------------------------------------------------------------------------
// parse 32Bit unsigned value out of ascii-string. All preceeding [SPACE] and [TAB] will be ignored.
// _uint32 sigclib_parse_U32(const char **pp, _uint32 *pval32);
// --> pp .............. address of pointer to ascii-string in use
// --> pval32 .......... Pointer where parsed 32bit unsigned value should be filed
// function will return 1 on success, on the other hand 0
// NOTE: in case of success given pointer will be set right after parsed token
    
// ------------------------------------------------------------------------------------------------
// parse 32Bit signed value out of ascii-string. All preceeding [SPACE] and [TAB] will be ignored.
// _uint32 sigclib_parse_S32(const char **pp, _int32 *pval32);
// --> pp .............. address of pointer to ascii-string in use
// --> pval32 .......... pointer where parsed 32bit signed value should be filed
// function will return 1 on success, on the other hand 0
// NOTE: in case of success given pointer will be set right after parsed token
    
// ------------------------------------------------------------------------------------------------
// parse F32 value out of ascii-string. All preceeding [SPACE] and [TAB] will be ignored.
// _uint32 sigclib_parse_F32(const char **pp, _f32 *pfval);
// --> pp .............. address of pointer to ascii-string in use
// --> pfval ........... pointer where parsed 32bit floating point value should be filed
// function will return 1 on success, on the other hand 0
// NOTE: in case of success given pointer will be set right after parsed token
    
// ------------------------------------------------------------------------------------------------
// parse if next character in given ascii-string is of any character of given character set 'str'.
// _uint32 sigclib_parse_IsChrAny(const char **pp, const char *str, _uint32 case_sensitive);
// --> pp .............. address of pointer to ascii-string in use
// --> str ............. characterset to ask for
// --> case_sensitive .. 0 = caseInSensitive check, 1 = caseSensitive check
// function will return 'chr' on success, on the other hand 0
// NOTE: in case of success given pointer will be set right after parsed character
    
// ------------------------------------------------------------------------------------------------
// parse and skip all preceeding occurencies of characters given with 'skip' in ascii-string till any other character, which is not given by 'skip' is detected.
// _uint32 sigclib_parse_Skip(const char **pp, const char *skip, _uint32 case_sensitive);
// --> pp .............. address of pointer to ascii-string in use
// --> skip ............ pointer to ascii-0-string if characters to skip
// --> case_sensitive .. 0 = caseInSensitive skip, 1 = caseSensitive skip
// function will return number of skipped characters or 0
// NOTE: in case of success given pointer will be set right after last skipped token
    
// ------------------------------------------------------------------------------------------------
// parse if next token starts with 'searchee'. All preceeding [SPACE] and [TAB] will be ignored.
// _uint32 sigclib_parse_IsToken(const char **pp, const char *searchee, _uint32 case_sensitive, _uint32 solid_token);
// --> pp .............. address of pointer to ascii-string in use
// --> searchee ........ ascii-0-terminated token used to check
// --> solid_token ..... 1 = searchee has to be solid in parsed string, 0 = searchhe is just the beginning of parsed string 
// --> case_sensitive .. 0 = caseInSensitive check, 1 = caseSensitive check
// function will return 1 on success, on the other hand 0
// NOTE: in case of success given pointer will be set right after parsed token

// ------------------------------------------------------------------------------------------------
// parse text between e.g. "()". 
// _uint32 sigclib_parse_TextIntermediate(const char **pp, char *pd, _uint32 sizepd, const char *between);
// --> pp .............. address of pointer to ascii-string in use
// --> pd .............. pointer to destination where parsed text should be filed
// --> sizedp .......... bytesize of destination to file text
// --> between ......... pointer to start- and end-characters. e.g. "()" or "<>". If NULL is given, text between '"' will be parsed.
// function will return number of parsed characters inclusive(!) final-zero. Therefore on success retcode is always >0. In case of any error function will return 0.
// NOTE: * in case of success given pointer will be set right after parsed end-character
//       * function will care bracket level

// ------------------------------------------------------------------------------------------------
// parse if next token is of numeric type in ascii-string. All preceeding [SPACE] and [TAB] will be ignored.
// _uint32 sigclib_parse_IsNum(const char *pl, _uint32 just_unsigned);
// --> pl .............. pointer to ascii-string in use
// --> just_unsigned ... 1 = check just for unsigned value, 0 = check for signed and unsigned value
// function will return 1 on success, on the other hand 0

// ------------------------------------------------------------------------------------------------
// parse if end of ascii-string reached. All preceeding [SPACE] and [TAB] will be ignored. 
// INFO: occurences of '\0', [CR], [LF] or [FF] will be interpreted as EOL.
// _uint32 sigclib_parse_IsEOL(const char *pl);
// --> pl .............. pointer to ascii-string in use
// function will return 1 on success, on the other hand 0

// ------------------------------------------------------------------------------------------------
// parse IPv4 address + port (10.20.30.40:5060) out of ascii-string. All preceeding [SPACE] and [TAB] will be ignored.
// Following separators are valid: '.'','[SPACE] and ':' to seperate IpPort if applicated
// _uint32 sigclib_parse_IpAddress(const char **pp, _uint32 *paddr, _uint32 *pport);
// --> pp .............. address of pointer to ascii-string in use
// --> paddr ........... pointer where parsed IPv4 should be filed
// --> pport ........... NULL or pointer where enclosed 'port' should be filed
// function will return 1 on success, on the other hand 0
// NOTE: in case of success given pointer will be set right after parsed token
// e.g.: "1.2.3.4" will be converted to 0x04030201

// ------------------------------------------------------------------------------------------------
// parse Date out of ascii-string.
// Following format is valid: "2023.08.24" or "24.8.2023"
// valid separartors are '.'',''-' and '/'
// _uint32 sigclib_parse_Date(const char **pp, _uint32 *pdate);
// --> pp .............. address of pointer to ascii-string in use
// --> pdate ........... pointer where parsed lasal_date should be filed
// function will return 1 on success, on the other hand 0
// NOTE: in case of success given pointer will be set right after parsed data

// ------------------------------------------------------------------------------------------------
// parse Time out of ascii-string.
// Following format is valid: "23:04:00" or "11:04pm"
// valid separartors are ':''.'',' and '-'
// _uint32 sigclib_parse_Time(const char **pp, _uint32 *ptime);
// --> pp .............. address of pointer to ascii-string in use
// --> ptime ........... pointer where parsed lasal_time should be filed
// function will return 1 on success, on the other hand 0
// NOTE: in case of success given pointer will be set right after parsed data

// ------------------------------------------------------------------------------------------------
// parse RegularExpression out of ascii-string
// _uint32 sigclib_parse_RegularExpression(const char **pp, char *pd, _uint32 sizepd);
// --> pp .............. address of pointer to ascii-string in use
// --> pd .............. pointer to destination where parsed RegularExpression should be filed
// --> sizedp .......... bytesize of destination where to file RegularExpression
// function will return number of parsed characters on success, on the other hand 0
// NOTE: in case of success given pointer will be set right after parsed data

// ------------------------------------------------------------------------------------------------
// parse Token till any character of 'separator' occures.
// _uint32 sigclib_parse_Token(const char **pp, char *pd, _uint32 sizepd, const char *separator);
// --> pp .............. address of pointer to ascii-string in use
// --> pd .............. pointer to destination where parsed string should be filed
// --> sizedp .......... bytesize of destination where to file string
// --> separator ....... 0-terminated characterset to break parsing
// function will return number of parsed characters on success, on the other hand 0
// NOTE: given pointer will be set right after parsed data (e.g. will point to first character found to break in given string pp)
