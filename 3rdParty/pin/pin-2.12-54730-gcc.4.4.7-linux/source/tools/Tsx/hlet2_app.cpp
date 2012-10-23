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



#include "hle_rtm.h"
#include "test_util.h"

volatile int value32 = 0;
volatile int value64 = 0;
volatile int value128 = 0;

const int threadCount = 500;

typedef struct {
    int* lockP;
    volatile int*  vp;
    int loopCount;
}LOCK_AND_VALUE;

void ThreadRtn( void* voidLockP)
{
    LOCK_AND_VALUE *lockAndValue = reinterpret_cast<LOCK_AND_VALUE *>(voidLockP);
    int i;
    for( i=0; i<lockAndValue->loopCount; ++i ) {
        elidedLock (lockAndValue->lockP);
        *(lockAndValue->vp) += 1;
        elidedUnlock(lockAndValue->lockP);
    } 
}

LOCK_AND_VALUE lockAndValue;

int main (int argc, char ** argv)
{
    THREAD_HANDLE threadHandles[threadCount];
    
    /* Allocate the lock so that it's somewhere a long way 
     * from the total. Otherwise they may be in the same cache line, 
     * which is confusing.
     */
    int* lockP = (int *)malloc (sizeof(int));
    *lockP = 0;  /* Ensure it starts unlocked */

    
    lockAndValue.lockP = lockP;
    lockAndValue.vp = &value32;
    lockAndValue.loopCount = 32;

    
    for (int i=0; i<threadCount; i++)
    {
        CreateOneThread (&threadHandles[i], (THREAD_RTN_PTR)ThreadRtn, static_cast<void *>(&lockAndValue));
    }
    printf ("all threads created\n");
    fflush (stdout);
    for (int i=0; i<threadCount; i++)
    {
        JoinOneThread(threadHandles[i]);
    }
    printf ("Final value = %d, should be %d\n", *(lockAndValue.vp), lockAndValue.loopCount*threadCount);

    lockAndValue.vp = &value64;
    lockAndValue.loopCount = 64;
    for (int i=0; i<threadCount; i++)
    {
        CreateOneThread (&threadHandles[i], (THREAD_RTN_PTR)ThreadRtn, static_cast<void *>(&lockAndValue));
    }
    printf ("all threads created\n");
    fflush (stdout);
    for (int i=0; i<threadCount; i++)
    {
        JoinOneThread(threadHandles[i]);
    }
    printf ("Final value = %d, should be %d\n", *(lockAndValue.vp), lockAndValue.loopCount*threadCount);

    lockAndValue.vp = &value128;
    lockAndValue.loopCount = 128;
    for (int i=0; i<threadCount; i++)
    {
        CreateOneThread (&threadHandles[i], (THREAD_RTN_PTR)ThreadRtn, static_cast<void *>(&lockAndValue));
    }
    printf ("all threads created\n");
    fflush (stdout);
    for (int i=0; i<threadCount; i++)
    {
        JoinOneThread(threadHandles[i]);
    }
    printf ("Final value = %d, should be %d\n", *(lockAndValue.vp), lockAndValue.loopCount*threadCount);
    

    return TEST_EPILOGUE( (value32==32*threadCount) && (value64==64*threadCount) && (value128==128*threadCount) );
}
