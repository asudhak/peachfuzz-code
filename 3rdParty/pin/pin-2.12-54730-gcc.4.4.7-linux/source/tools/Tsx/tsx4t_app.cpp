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
#include "../threadlib/threadlib.h"


/* 
 * Force a lot of contention.
 * icc -openmp -o fight_rtm fight_rtm.c
 */

int volatile value = 0;



/*!
 * Transaction abort, with immediate zero. 
 */
static __inline void XAbort()
{
#if (TARGET_WINDOWS)
    __asm  {
        _emit 0xc6
        _emit 0xf8
        _emit 0x00
    }
#else
    __asm__ volatile (".byte 0xC6; .byte 0xF8; .byte 0x00" :::"memory");
#endif
}

#define COUNT 1000
#define NUM_THREADS 100
LONG numAborts = 0;
int count = 1000;

void ThreadRtn (void * voidPtr)
{
    int i;
    // transactionally add 1 to the global variable value, count times
    for( i=0; i<COUNT; ++i ) {
        int res = 0;
        // the transaction of adding 1 to value is inside the while loop
        while( !res ) {
            __asm {
                // encoding of xbegin abortLabel
                _emit 0xC7
                _emit 0xF8
#if (TARGET_IA32E)
                _emit 0x14
#else
                _emit 0x13
#endif
                _emit 0
                _emit 0
                _emit 0
                mov res, 1
                add value, 1
                // encoding of xend
                _emit 0x0f
                _emit 0x01
                _emit 0xd5
                jmp L2
            abortLabel:
                mov res, 0
            L2:
                nop
            }
            
            if (res==0)//  means transaction failed
            {
                 InterlockedIncrement(&numAborts);
            }
        }
    }
}

int main (int argc, char ** argv)
{
    THREAD_HANDLE threadHandles[NUM_THREADS];

    for (int i=0; i<NUM_THREADS; i++)
    {
        CreateOneThread (&threadHandles[i], (THREAD_RTN_PTR)ThreadRtn, NULL);
    }

    printf ("all threads created\n");
    fflush (stdout);
    for (int i=0; i<NUM_THREADS; i++)
    {
        JoinOneThread(threadHandles[i]);
    }
    printf ("Value = %d, should be %d  numAborts %d\n", value, NUM_THREADS*COUNT, numAborts);
    if (NUM_THREADS*COUNT != value)
    {
        printf ("***Error value is not as expected\n");
        return(-1);
    }
    return (0);
  
}

