
#ifndef _SigCLibNew_H
 #define _SigCLibNew_H

 #include "DefineCompiler.h"

 #ifdef cCompile

  #include "SigCLib.h"

  #define isalnum      sigclib_isalnum
  #define isalpha      sigclib_isalpha
  #define isdigit      sigclib_isdigit
  #define islower      sigclib_islower
  #define isprint      sigclib_isprint
  #define ispunct      sigclib_ispunct
  #define isspace      sigclib_isspace
  #define isupper      sigclib_isupper
  #define isxdigit     sigclib_isxdigit

  #define strcasecmp   sigclib_strcasecmp
  #define strcat       sigclib_strcat
  #define strchr       sigclib_strchr
  #define strcmp       sigclib_strcmp
  #define strcpy       sigclib_strcpy
  #define strcspn      sigclib_strcspn
  #define stricmp      sigclib_stricmp
  #define strlen       sigclib_strlen
  #define strlwr       sigclib_strlwr
  #define strncasecmp  sigclib_strncasecmp
  #define strncat      sigclib_strncat
  #define strncmp      sigclib_strncmp
  #define strncpy      sigclib_strncpy
  #define strpbrk      sigclib_strpbrk
  #define strrchr      sigclib_strrchr
  #define strspn       sigclib_strspn
  #define strstr       sigclib_strstr
  #define strtod       sigclib_strtod
  #define strtof       sigclib_strtof
  #define strtok_r     sigclib_strtok_r 
  #define strtol       sigclib_strtol
  #define strtoll      sigclib_strtoll
  #define strtoul      sigclib_strtoul
  #define strupr       sigclib_strupr
  #define strlcpy      sigclib_strlcpy
  #define strlcat      sigclib_strlcat
  
  #define tolower      sigclib_tolower
  #define toupper      sigclib_toupper
  #define atoi         sigclib_atoi
  #define itoa         sigclib_itoa
  #define uitoa        sigclib_uitoa
  #define lltoa        sigclib_lltoa
  #define ulltoa       sigclib_ulltoa
  #define atol         sigclib_atol
  #define atoll        sigclib_atoll
  #define atof         sigclib_atof
  #define ftoa         sigclib_ftoa
  #define memset       sigclib_memset
  #define memcpy       sigclib_memcpy
  #define memmove      sigclib_memmove
  #define memcmp       sigclib_memcmp
  #define memchr       sigclib_memchr
  #define memicmp      sigclib_memicmp
  #define hsort        sigclib_hsort
  #define hfind        sigclib_hfind
  #define printf       sigclib_printf
 #ifdef sigclib_sprintf
  #undef sigclib_sprintf
 #endif
  #define sigclib_sprintf sigclib_get_sprintf_fptr()
  #define sprintf      sigclib_sprintf
  
  // CMemLib
  #define free         sigclib_free
  #define malloc       sigclib_malloc
  #define calloc       sigclib_calloc
  #define realloc      sigclib_realloc
  
  // CNetworkLib
  #define htonl        sigclib_htonl
  #define htons        sigclib_htons
  #define ntohl        sigclib_ntohl
  #define ntohs        sigclib_ntohs
  
  // CSortLib
  #define qsort        sigclib_qsort
  #define bsearch      sigclib_bsearch
  
  // CTimeLib
  #define srand        sigclib_srand
  #define rand         sigclib_rand
  
  // CStrLib
  #define isxdigit     sigclib_isxdigit
  #define isspace      sigclib_isspace
  #define ispunct      sigclib_ispunct
  #define isalnum      sigclib_isalnum
  #define isalpha      sigclib_isalpha
  #define isprint      sigclib_isprint
  
  // CStrScn
  #define sscanf       sigclib_sscanf
  
 #endif
#endif
