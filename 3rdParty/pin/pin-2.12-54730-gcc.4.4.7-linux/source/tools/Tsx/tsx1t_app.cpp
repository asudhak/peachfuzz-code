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
 * Force a lot of contention. run transaction of incrementing a global variable
 */

int volatile value = 0;

#if 1

/* RTM Functions */
/*! 
 * Enter speculative execution mode.
 */
static __inline int XBegin()
{
    int res = 1;
    
    /* Note that %eax must be noted as killed, because the XSR is returned
     * in %eax on abort. Other register values are restored, so don't need to be
     * killed.
     * We must also mark res as an input and an output, since otherwise the 
     * res=1 may be dropped as being dead, whereas we do need that on the
     * normal path.
     */
#if TARGET_WINDOWS
    __asm {
        // We need to save and restore eax\rax cause compiler uses it sometimes
#  if TARGET_IA32
        push eax
#  else
        push rax
# endif
        // encoding of xbegin abortLabel
        _emit 0xC7
        _emit 0xF8
        _emit 2
        _emit 0
        _emit 0
        _emit 0
        jmp   L2
abortLabel:
        mov   res,  0
    L2:
#  if TARGET_IA32
        pop eax
#  else
        pop rax
#  endif
    }
#else
    __asm__ volatile ("1: .byte  0xC7; .byte 0xF8;\n"
                      "   .long  1f-1b-6\n"
                      "    jmp   2f\n"
                      "1:  xor   %0,%0\n"
                      "2:"
                      :"=r"(res):"0"(res):"memory","%eax");
#endif
    return res;
}


/*! 
 * Transaction end 
 */
static __inline void XEnd()
{
#if TARGET_WINDOWS
    __asm  {
        _emit 0x0f
        _emit 0x01
        _emit 0xd5
    }
#else
    __asm__ volatile (".byte 0x0f; .byte 0x01; .byte 0xd5" :::"memory");
#endif
}

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
#endif

#define COUNT 1000
#define NUM_THREADS 100
LONG numAborts = 0;
int count = 1000;


void ThreadRtn (void * voidPtr)
{
    int numTries = 0;
    // transactionally add 1 to the global variable value, count times
    for(int i=0; i<COUNT; ++i ) {
        int res = 0;
        // the transaction of adding 1 to value is inside the while loop
        while( !res ) {
            numTries++;
            res = XBegin();
            if( res ) {
                value += 1;   // could be thread contention on update of value
                              // when the processor detects contention the thread
                              // state (including memory values) is set to what it was 
                              // just before the execution ofthe xbegin machine instruction, 
                              // and the IP is set to abortLabel
                
                if ((numTries % 100)==0)
                {
                     XAbort();
                }
                XEnd();       // execute xend instruction
            }
            else // res==0  means transaction failed
            {
                 InterlockedIncrement(&numAborts);
            }
        }
    }
}

int main (int argc, char ** argv)
{
    THREAD_HANDLE threadHandles[NUM_THREADS];
    int count = 1000;
    
    
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
    if (numAborts == 0)
    {
        printf ("***Error expected some aborts\n");
        return(-1);
    }
    return (0);
}

