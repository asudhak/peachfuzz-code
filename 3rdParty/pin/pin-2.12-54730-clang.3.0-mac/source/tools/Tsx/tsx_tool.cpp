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
#include "pin.h"
int numXbeginsJitted = 0;
int numXbeginsExecuted = 0;
int numXendsJitted = 0;
int numXendsExecuted = 0;


VOID CountXbegin()
{
    numXbeginsExecuted++;
}

VOID CountXend()
{
    numXendsExecuted++;
}

VOID Instruction (INS ins, VOID *v)
{
    if (INS_IsXbegin(ins))
    {
        numXbeginsJitted++;
        INS_InsertCall(ins, IPOINT_BEFORE, (AFUNPTR)CountXbegin, IARG_END);
    }
    else if (INS_IsXend(ins))
    {
        numXendsJitted++;
        INS_InsertCall(ins, IPOINT_BEFORE, (AFUNPTR)CountXend, IARG_END);
    }
}


VOID Fini(INT32 code, VOID *v)
{
    if (numXbeginsJitted == 0)
    {
        printf ("***Error expected to instrument an Xbegin\n");
        exit (1);
    }
    if (numXendsJitted == 0)
    {
        printf ("***Error expected to instrument an Xend\n");
        exit (1);
    }
    if (numXbeginsExecuted == 0)
    {
        printf ("***Error expected to execute an Xbegin\n");
        exit (1);
    }
    if (numXendsExecuted == 0)
    {
        printf ("***Error expected to execute an Xend\n");
        exit (1);
    }
    printf ("numXbeginsJitted %d numXendsJitted %d numXbeginsExecuted %d numXendsExecuted %d\n", numXbeginsJitted, numXendsJitted, numXbeginsExecuted, numXendsExecuted);
}

int main(INT32 argc, CHAR **argv)
{
    PIN_Init(argc, argv);
    
    INS_AddInstrumentFunction(Instruction, 0);
    PIN_AddFiniFunction(Fini, 0);
    
    // Never returns
    PIN_StartProgram();
    
    return 0;
}