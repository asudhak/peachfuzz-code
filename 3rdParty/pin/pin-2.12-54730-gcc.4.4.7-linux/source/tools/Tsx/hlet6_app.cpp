/*BEGIN_LEGAL 
Intel Open Source License 

Copyright (c) 2002-2012 Intel Corporation. All rights reserved.
 
Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.  Redistributions
in binary form must reproduce the above copyright notice, this list of
conditions and the following disclaimer in the documentation and/or
other materials provided with the distribution.  Neither the name of
the Intel Corporation nor the names of its contributors may be used to
endorse or promote products derived from this software without
specific prior written permission.
 
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE INTEL OR
ITS CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
END_LEGAL */
#ifdef TARGET_WINDOWS
#include <windows.h>
#endif

#include <stdio.h>
#include <stdlib.h>
#include "hle_rtm.h"
#include "test_util.h"
#include "../threadlib/threadlib.h"

/* 
 * Simple thread test similar to the one Konrad supplied, 
 * but portable betwen windows and Linux and self-contained.
 *
 * icl /Qopenmp hle_increment.c
 * icc -openmp -o hle_increment hle_increment.c
 */

/* Normal lock based on xchg */
static __inline void lock (int *l)
{
    int value = 1;

    do {
#if (TARGET_WINDOWS)
# if (TARGET_IA32)
        __asm  {
            mov eax,l
            mov edx,value
            xchg edx,dword ptr [eax]
            mov value, edx
       }
# else
        __asm {
           mov rax,l
           mov edx,value
           xchg edx,dword ptr [rax]
           mov value, edx
        }
# endif
#else
        __asm__ volatile ("xchg %0, 0(%1);"
              : "=r"(value), "=r" (l):"0" (value), "1" (l) :"memory");
#endif
    } while (value == 1);
}

#if (TARGET_IA32)
/* Alignment check bit in EFLAGS */
#define AC_BIT  (1<<18)

static uint32_t readEflags(void)
{
    uint32_t efl;

#if (TARGET_WINDOWS)        
    __asm {
        pushfd
        pop    eax
        mov    efl,eax
    }
# else
    __asm__ volatile ("\t pushf\n"
                      "\t pop    %0" : "=r" (efl));
#endif
    return efl;
}

static void writeEflags(uint32_t efl)
{
#if (TARGET_WINDOWS)
    __asm {
        mov    eax,efl
        push   eax
        popfd   
    }
#else
    __asm__ volatile ("\t push   %0\n"
                      "\t popf" : : "r" (efl));
#endif
}

// Enable the alignment check bit. This works on Linux, but not (AFAICS)
// on Windows. Oh well.
static void setAC(uint32_t enable)
{
    uint32_t eflags = readEflags();

    if (enable)
        writeEflags(eflags | AC_BIT);
    else
        writeEflags(eflags & ~AC_BIT);
}
#else
#define setAC(x) ((void)0)
#endif

int volatile total = 0;
const int threadCount = 10;

typedef struct {
    int* lockP;
    volatile int*  vp;
    int loopCount;
    int myId;
}LOCK_AND_VALUE;

void ThreadRtn( void* voidLockP)
{
    LOCK_AND_VALUE *lockAndValue = reinterpret_cast<LOCK_AND_VALUE *>(voidLockP);

    int i;
    int myId = lockAndValue->myId;

    for (i=0; i<lockAndValue->loopCount; i++) {
        // Thread zero uses normal lock/unlock
        if (myId == 0) {
            lock (lockAndValue->lockP);
            total++;
            elidedUnlock(lockAndValue->lockP); /* Can use the elided unlock just for fun. */
        } else {
            elidedLock (lockAndValue->lockP);
            {
                int old = total;
                old = old + 1;
                total = old;
            }
            elidedUnlock(lockAndValue->lockP);
        }
    }
}



int main (int argc, char ** argv)
{
    THREAD_HANDLE threadHandles[threadCount];
    LOCK_AND_VALUE lockAndValues[threadCount];
    
    /* Allocate the lock so that it's somewhere a long way 
     * from the total. Otherwise they may be in the same cache line, 
     * which is confusing.
     */
    // Allocate the lock so that it's somewhere a long way 
    // from the total. Otherwise they may be in the same cache 
    // line, which is confusing.
    int* lockP = (int*) malloc( sizeof(int) );
    *lockP = 0; /* Ensure it starts unlocked */

    // We know this caused us problems in the past.
    setAC(1);

    for (int i=0; i<threadCount; i++)
    {
        lockAndValues[i].myId = i;
        lockAndValues[i].lockP = lockP;
        lockAndValues[i].vp = &total;
        lockAndValues[i].loopCount = 1000;
        CreateOneThread (&threadHandles[i], (THREAD_RTN_PTR)ThreadRtn, static_cast<void *>(&lockAndValues[i]));
    }
    printf ("all threads created\n");
    fflush (stdout);
    for (int i=0; i<threadCount; i++)
    {
        JoinOneThread(threadHandles[i]);
    }

    setAC(0);

    printf ("Total = %d : should be %d\n", total, threadCount*lockAndValues[0].loopCount);
    return TEST_EPILOGUE( total==(threadCount*lockAndValues[0].loopCount) );
}
