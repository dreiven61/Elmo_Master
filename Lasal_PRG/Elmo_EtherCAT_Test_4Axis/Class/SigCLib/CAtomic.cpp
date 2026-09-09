//<NewSigmatekCFileOptimize/>
// +-------------------------------------------------------------------------------+
// +-[   copyright ] Sigmatek GmbH & CoKG                                          |
// +-[      author ] grumat, kolott                                                |
// +-[        date ] 08.07.2016                                                    |
// +-[ description ]---------------------------------------------------------------+
// |                                                                               |
// |                                                                               |
// +-------------------------------------------------------------------------------+

#include "SigCLib.h"

void sigclib_DMB(void)
{
  // Function will perform DMB-instruction on arm-processor
  // This ensures that other cores see the new data before they see the set flag.
  
  // Example:
  // STR R0, [data]  ; write data
  // DMB             ; DMB-instruction
  // STR R1, [flag]  ; set flag that data is valid

 #ifdef _LSL_TARGETARCH_ARM 
  __asm__ __volatile__(
    "dmb\n\t"
    ::: "memory");
 #endif
}

#ifndef sigclib_atomic_oldstyle

#if defined(_MSC_VER)
  // MSVC compiler
  // This compiler needs hand-coded x86 snippets
  #define _LSL_USE_ARM_ATOMIC_ASM		0
  #define _LSL_USE_X86_ATOMIC_ASM		0
  #define _LSL_USE_ATOMIC_BUILTINS	0
#elif defined(__clang__)
  // Modern GCC/Clang has the __atomic builtins
  #define _LSL_USE_ARM_ATOMIC_ASM		0
  #define _LSL_USE_X86_ATOMIC_ASM		0
  #define _LSL_USE_ATOMIC_BUILTINS	1
#elif _LSL_TARGETARCH_ARM
  // LC2 ARM GCC Compiler
  // This compiler needs hand-coded ARMv7 snippets
  #define _LSL_USE_ARM_ATOMIC_ASM		1
  #define _LSL_USE_X86_ATOMIC_ASM		0
  #define _LSL_USE_ATOMIC_BUILTINS	0
#elif _LSL_TARGETARCH_X86
  // Old Intel GCC compiler
  #define _LSL_USE_ARM_ATOMIC_ASM		0
  #define _LSL_USE_X86_ATOMIC_ASM		1
  #define _LSL_USE_ATOMIC_BUILTINS	0
#else
  // Unknown
  #error Unsupported compiler found!
#endif

inline long sigclib_atomic_cmpxchg(volatile long *mem, long cmpVal, long newVal)
{
#if defined(_MSC_VER)
  __asm{
    mov eax, cmpVal
    mov ebx, newVal
    mov edi, mem
    lock cmpxchg [edi], ebx
  }
#elif _LSL_USE_ARM_ATOMIC_ASM
  int old, tmp;
  __asm__ __volatile__(
    "dmb\n\t"
    "1:\n\t"
    "mov     %1, #0\n\t"
    "ldrex   %0, [%2]\n\t"
    "teq     %0, %3\n\t"
    "strexeq %1, %4, [%2]\n\t"
    "cmp     %1, #0\n\t"
    "bne     1b\n\t"
    "clrex\n\t"
    "dmb\n\t"
    : "=&r"(old), "=&r"(tmp)
    : "r"(mem), "r"(cmpVal), "r"(newVal)
    : "cc", "memory");
  return old;
#elif _LSL_USE_X86_ATOMIC_ASM
  long retVal;
  asm volatile( 
    "lock cmpxchgl %2, %1"
    : "=a" (retVal), "+m" (*mem)
    : "r" (newVal), "0" (cmpVal)
    : "memory"
  ); 
  return retVal;
#elif _LSL_USE_ATOMIC_BUILTINS
  long old = cmpVal;
  __atomic_compare_exchange_n(mem, &old, newVal, false, __ATOMIC_SEQ_CST, __ATOMIC_SEQ_CST);
  return old;	// identical semantics to __sync_val_compare_and_swap
#else
  #error Unsupported compiler found!
#endif
}

inline long sigclib_atomic_fetch_add(volatile long *mem, long addVal)
{
#if defined(_MSC_VER)
  __asm{
    mov edi, mem
    mov eax, addVal
    lock xadd [edi], eax
  }
#elif _LSL_USE_ARM_ATOMIC_ASM
  int old, sum, status;
  __asm__ __volatile__(
    "dmb\n\t"
    "1:\n\t"
    "ldrex   %0, [%3]\n\t"	   // old = *p
    "add     %1, %0, %4\n\t"   // tmp = old + v
    "strex   %2, %1, [%3]\n\t" // if (*p = tmp) succeeded, %2==0
    "teq     %2, #0\n\t"
    "bne     1b\n\t"
    "dmb\n\t"
    : "=&r"(old), "=&r"(sum), "=&r"(status)
    : "r"(mem), "r"(addVal)
    : "cc", "memory");
  return old; // fetch_add returns the old value
#elif _LSL_USE_X86_ATOMIC_ASM
  asm volatile(
    "lock xadd %0, (%1);"
    : "=a"(addVal)
    : "r"(mem), "a"(addVal)
    : "memory"
  );
  return addVal;
#elif _LSL_USE_ATOMIC_BUILTINS
  return __atomic_fetch_add(mem, addVal, __ATOMIC_SEQ_CST);
#else
  #error Unsupported compiler found!
#endif
}

unsigned long sigclib_atomic_incU32(unsigned long *pValue)
{
  return sigclib_atomic_fetch_add((long *)pValue, 1);
}

unsigned long sigclib_atomic_decU32(unsigned long *pValue)
{
  return sigclib_atomic_fetch_add((long *)pValue, -1);
}

unsigned long sigclib_atomic_addU32(unsigned long *pValue, unsigned long addVal)
{
  return sigclib_atomic_fetch_add((long *)pValue, addVal);
}

unsigned long sigclib_atomic_subU32(unsigned long *pValue, unsigned long subVal)
{
  return sigclib_atomic_fetch_add((long *)pValue, -subVal);
}

unsigned long sigclib_atomic_cmpxchgU32(unsigned long *pValue, unsigned long cmpVal, unsigned long newVal)
{
  return (unsigned long)sigclib_atomic_cmpxchg((long *)pValue, (long)cmpVal, (long)newVal);
}

long sigclib_atomic_incS32(long *pValue)
{
  return sigclib_atomic_fetch_add(pValue, 1);
}

long sigclib_atomic_decS32(long *pValue)
{
  return sigclib_atomic_fetch_add(pValue, -1);
}

long sigclib_atomic_addS32(long *pValue, long addVal)
{
  return sigclib_atomic_fetch_add(pValue, addVal);
}

long sigclib_atomic_subS32(long *pValue, long subVal)
{
  return sigclib_atomic_fetch_add(pValue, -subVal);
}

long sigclib_atomic_cmpxchgS32(long *pValue, long cmpVal, long newVal)
{
  return sigclib_atomic_cmpxchg(pValue, cmpVal, newVal);
}

unsigned long sigclib_atomic_getU32(unsigned long *pValue)
{
#if defined(_MSC_VER)
  __asm{
    mov edi, pValue
    mov eax, dword ptr [edi]
  }
#elif _LSL_USE_ARM_ATOMIC_ASM
  unsigned long val;
  __asm__ __volatile__(
    "ldr     %0, [%1]\n\t"
    "dmb\n\t"
    : "=&r"(val)
    : "r"(pValue)
    : "cc", "memory");
  return val; // fetch_add returns the old value
#elif _LSL_USE_X86_ATOMIC_ASM
  unsigned long val;
  asm volatile(
    ".intel_syntax noprefix\n\t"
    "mov %0, dword ptr [%1]\n\t"
    ".att_syntax prefix\n\t"
    : "=r&"(val)
    : "r"(pValue)
    : "memory", "cc");
  return val; // fetch_add returns the old value
#elif _LSL_USE_ATOMIC_BUILTINS
  return __atomic_load_n(pValue, __ATOMIC_SEQ_CST);
#else
  #error Unsupported compiler
#endif
}	

long sigclib_atomic_getS32(long *pValue)
{
#if defined(_MSC_VER)
  // TODO
  __asm{
    mov edi, pValue
    mov eax, dword ptr [edi]
  }
#elif _LSL_USE_ARM_ATOMIC_ASM
  long val;
  __asm__ __volatile__(
    "ldr     %0, [%1]\n\t"
    "dmb\n\t"
    : "=&r"(val)
    : "r"(pValue)
    : "cc", "memory");
  return val; // fetch_add returns the old value
#elif _LSL_USE_X86_ATOMIC_ASM
  long val;
  asm volatile(
    ".intel_syntax noprefix\n\t"
    "mov %0, dword ptr [%1]\n\t"
    ".att_syntax prefix\n\t"
    : "=r&"(val)
    : "r"(pValue)
    : "memory", "cc");
  return val; // fetch_add returns the old value
#elif _LSL_USE_ATOMIC_BUILTINS
  return __atomic_load_n(pValue, __ATOMIC_SEQ_CST);
#else
  #error Unsupported compiler
#endif
}	
	
#define __CMPX_SPINLOCK_S32(_ALU) { do{oldval=sigclib_atomic_getS32(pValue);newval=(_ALU);res=sigclib_atomic_cmpxchgS32(pValue,oldval,newval);}while(res!=oldval); }
#define __CMPX_SPINLOCK_U32(_ALU) { do{oldval=sigclib_atomic_getU32(pValue);newval=(_ALU);res=sigclib_atomic_cmpxchgU32(pValue,oldval,newval);}while(res!=oldval); }

unsigned long sigclib_atomic_andU32(unsigned long *pValue, unsigned long bitVal)
{
  unsigned long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_U32(oldval & bitVal);
  return oldval;
}

long sigclib_atomic_andS32(long *pValue, long bitVal)
{
  long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_S32(oldval & bitVal);
  return oldval;
}

unsigned long sigclib_atomic_nandU32(unsigned long *pValue, unsigned long bitVal)
{
  unsigned long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_U32(~(oldval & bitVal));
  return oldval;
}

long sigclib_atomic_nandS32(long *pValue, long bitVal)
{
  long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_S32(~(oldval & bitVal));
  return oldval;
}

unsigned long sigclib_atomic_orU32(unsigned long *pValue, unsigned long bitVal)
{
  unsigned long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_U32(oldval | bitVal);
  return oldval;
}

long sigclib_atomic_orS32(long *pValue, long bitVal)
{
  long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_S32(oldval | bitVal);
  return oldval;
}

unsigned long sigclib_atomic_xorU32(unsigned long *pValue, unsigned long bitVal)
{
  unsigned long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_U32(oldval ^ bitVal);
  return oldval;
}

long sigclib_atomic_xorS32(long *pValue, long bitVal)
{
  long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_S32(oldval ^ bitVal);
  return oldval;
}

void sigclib_atomic_setU32(unsigned long *pValue, unsigned long value)
{
#if defined(_MSC_VER)
  __asm{
    mov edi, mem
    mov eax, value
    xchg DWORD PTR [edi], eax
  }
#elif _LSL_USE_ARM_ATOMIC_ASM
  asm volatile(
    "dmb\n\t"
    "str %1, [%0]\n\t"
    "dmb\n\t"
    : 
    : "r"(pValue), "r"(value)
    : "memory"
  );
#elif _LSL_USE_X86_ATOMIC_ASM
  asm volatile(
    ".intel_syntax noprefix\n\t"
    "xchg DWORD PTR [%0], %1\n\t"
    ".att_syntax noprefix\n\t"
    : 
    : "r"(pValue), "r"(value)
    : "memory"
  );
#elif _LSL_USE_ATOMIC_BUILTINS
  __atomic_store_n(pValue, value, __ATOMIC_SEQ_CST);
#else
  #error Unsupported compiler found!
#endif
}

unsigned long sigclib_atomic_swpU32(unsigned long *pValue, unsigned long value)
{
#if defined(_MSC_VER)
  __asm{
    mov edi, mem
    mov eax, value
    xchg DWORD PTR [edi], eax
  }
#elif _LSL_USE_ARM_ATOMIC_ASM
  unsigned long old, tmp;
  asm volatile(
    "dmb\n\t"
    "1:\n\t"
    "ldrex  %0, [%2]\n\t"
    "strex  %1, %3, [%2]\n\t"
    "cmp    %1, #0\n\t"
    "bne    1b\n\t"
    "dmb\n\t"
    : "=&r"(old), "=&r"(tmp)
    : "r"(pValue), "r"(value)
    : "memory"
  );
  return old;
#elif _LSL_USE_X86_ATOMIC_ASM
  asm volatile(
    ".intel_syntax noprefix\n\t"
    "xchg DWORD PTR [%1], %0\n\t"
    ".att_syntax noprefix\n\t"
    : "+r"(value)
    : "r"(pValue)
    : "memory"
  );
  return value;
#elif _LSL_USE_ATOMIC_BUILTINS
  return __atomic_exchange_n(pValue, value, __ATOMIC_SEQ_CST);
#else
  #error Unsupported compiler found!
#endif
}

long sigclib_atomic_swpS32(long *pValue, long swpVal)
{
  return (long)sigclib_atomic_swpU32((unsigned long *)pValue, (unsigned long)swpVal);
}

void sigclib_atomic_setS32(long *pValue, long value)
{
  sigclib_atomic_setU32((unsigned long*)pValue, (unsigned long)value);
}

void *sigclib_atomic_cmpxchgPtr(void **pPointer, void *cmpPtr, void *newPtr)
{
 #if __SIZEOF_POINTER__ == 4
  return (void*)sigclib_atomic_cmpxchgU32((unsigned long*)pPointer, (unsigned long)cmpPtr, (unsigned long)newPtr);
 #elif _LSL_USE_ATOMIC_BUILTINS
  void *expected = cmpPtr;
  __atomic_compare_exchange_n(
    pPointer,		// pointer to the atomic object
    &expected,		// pointer to expected old value
    newPtr,			// desired new value
    0,				// weak = 0  => strong CAS
    __ATOMIC_SEQ_CST,
    __ATOMIC_SEQ_CST
  );
  return expected;
 #else
  #error Unsupported function sigclib_atomic_cmpxchgPtr() found !
 #endif
}

#else // sigclib_atomic_oldstyle

#ifdef _LSL_TARGETARCH_X86

inline long sigclib_atomic_cmpxchg(volatile long *mem, long cmpVal, long newVal)
{
 #ifdef _MSC_VER
    __asm{
      mov eax, cmpVal
      mov ebx, newVal
      mov edi, mem
      lock cmpxchg [edi], ebx
    }
 #else
  long retVal;
	asm volatile( 
    "lock cmpxchgl %2, %1"
    : "=a" (retVal), "+m" (*mem)
    : "r" (newVal), "0" (cmpVal)
    : "memory"
  ); 
  return retVal;
 #endif
}

inline long sigclib_atomic_fetch_add(volatile long *mem, long addVal)
{
 #ifdef _MSC_VER
    __asm{
      mov edi, mem
      mov eax, addVal
      lock xadd [edi], eax
    }
 #else
  asm volatile(
    "lock xadd %0, (%1);"
    : "=a"(addVal)
    : "r"(mem), "a"(addVal)
    : "memory"
  );
  return addVal;
 #endif
}

unsigned long sigclib_atomic_incU32(unsigned long *pValue)
{
  return sigclib_atomic_fetch_add((long*)pValue, 1);
}

unsigned long sigclib_atomic_decU32(unsigned long *pValue)
{
  return sigclib_atomic_fetch_add((long*)pValue, -1);
}

unsigned long sigclib_atomic_addU32(unsigned long *pValue, unsigned long addVal)
{
  return sigclib_atomic_fetch_add((long*)pValue, addVal);
}

unsigned long sigclib_atomic_subU32(unsigned long *pValue, unsigned long subVal)
{
  return sigclib_atomic_fetch_add((long*)pValue, -subVal);
}
 
unsigned long sigclib_atomic_cmpxchgU32(unsigned long *pValue, unsigned long cmpVal, unsigned long newVal)
{
  return (unsigned long)sigclib_atomic_cmpxchg((long*)pValue, (long)cmpVal, (long)newVal);
}
  
long sigclib_atomic_incS32(long *pValue)
{
  return sigclib_atomic_fetch_add(pValue, 1);
}

long sigclib_atomic_decS32(long *pValue)
{
  return sigclib_atomic_fetch_add(pValue, -1);
}

long sigclib_atomic_addS32(long *pValue, long addVal)
{
  return sigclib_atomic_fetch_add(pValue, addVal);
}

long sigclib_atomic_subS32(long *pValue, long subVal)
{
  return sigclib_atomic_fetch_add(pValue, -subVal);
}

long sigclib_atomic_cmpxchgS32(long *pValue, long cmpVal, long newVal)
{
  return sigclib_atomic_cmpxchg(pValue, cmpVal, newVal);
}

#endif

void *sigclib_atomic_cmpxchgPtr(void **pPointer, void *cmpPtr, void *newPtr)
{
 #if sigclib_sizeof_ptr == 4
  return (void*)sigclib_atomic_cmpxchgU32((unsigned long*)pPointer, (unsigned long)cmpPtr, (unsigned long)newPtr);
 #else
  #error Unsupported function sigclib_atomic_cmpxchgPtr() found !
 #endif
}

#define __CMPX_SPINLOCK_S32(_ALU) { do{oldval=sigclib_atomic_getS32(pValue);newval=(_ALU);res=sigclib_atomic_cmpxchgS32(pValue,oldval,newval);}while(res!=oldval); }
#define __CMPX_SPINLOCK_U32(_ALU) { do{oldval=sigclib_atomic_getU32(pValue);newval=(_ALU);res=sigclib_atomic_cmpxchgU32(pValue,oldval,newval);}while(res!=oldval); }

unsigned long sigclib_atomic_andU32(unsigned long *pValue, unsigned long bitVal)
{
  unsigned long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_U32(oldval & bitVal);
  return oldval;
}

long sigclib_atomic_andS32(long *pValue, long bitVal)
{
  long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_S32(oldval & bitVal);
  return oldval;
}

unsigned long sigclib_atomic_nandU32(unsigned long *pValue, unsigned long bitVal)
{
  unsigned long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_U32(~(oldval & bitVal));
  return oldval;
}

long sigclib_atomic_nandS32(long *pValue, long bitVal)
{
  long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_S32(~(oldval & bitVal));
  return oldval;
}

unsigned long sigclib_atomic_orU32(unsigned long *pValue, unsigned long bitVal)
{
  unsigned long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_U32(oldval | bitVal);
  return oldval;
}

long sigclib_atomic_orS32(long *pValue, long bitVal)
{
  long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_S32(oldval | bitVal);
  return oldval;
}

unsigned long sigclib_atomic_xorU32(unsigned long *pValue, unsigned long bitVal)
{
  unsigned long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_U32(oldval ^ bitVal);
  return oldval;
}

long sigclib_atomic_xorS32(long *pValue, long bitVal)
{
  long oldval, newval, res; // note: used for spinlock !
  __CMPX_SPINLOCK_S32(oldval ^ bitVal);
  return oldval;
}

#endif // sigclib_atomic_oldstyle
