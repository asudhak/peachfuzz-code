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
#include <string>
#include <stdio.h>
#include <cstdlib>
#include "gather.h"
#include "gsseemu.h"


#include "sys_memory.h"

#if defined(TARGET_WINDOWS)
#include "windows.h"
#define EXPORT_CSYM extern "C" __declspec( dllexport )
#else
#error Unsupported OS
#endif

/*!
 * @return IP register value in the given exception context
 */
#if     defined(TARGET_IA32)
static ULONG_PTR GetIp(CONTEXT * pExceptContext) {return pExceptContext->Eip;}
static VOID SetIp(LPEXCEPTION_POINTERS exceptPtr, ULONG_PTR addr) {exceptPtr->ContextRecord->Eip=addr;}
#elif   defined(TARGET_IA32E)
static ULONG_PTR GetIp(CONTEXT * pExceptContext) {return pExceptContext->Rip;}
static VOID SetIp(LPEXCEPTION_POINTERS exceptPtr, ULONG_PTR addr) {exceptPtr->ContextRecord->Rip=addr;}
#else
#error Unsupported architechture
#endif

extern "C" unsigned int GetInstructionLenAndDisasm (unsigned char *ip, char *str, BOOL *insIsVgather);
extern "C" void InitXed ();
int numExceptions = 0;
int numExpectedExceptions = 0;
size_t pageSize;
char * pages;


__int32 arr1[8] = {1,2,3,4,5,6,7,8};
__int64 vind1[4] = {0,1,2,3};
int mask1[8] =         {0xFFFFFFFF, 0, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0};
int expectedMask1[8] = {0,          0, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0};
__int32 expected1[8] = {1,0,0,0, 0,0,0,0};

int mask2[8] =         {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0};
int expectedMask2[8] = {0,          0,          0xFFFFFFFF, 0, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0};
__int32 expected2[8] = {1,2,0,0, 0,0,0,0};


int mask3[8] =         {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0};
int expectedMask3[8] = {0,          0xFFFFFFFF, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0};
__int32 expected3[8] = {1,0,0,0, 0,0,0,0};

int mask4[8] =         {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0};
int expectedMask4[8] = {0,          0x0,        0x0,        0xFFFFFFFF, 0xFFFFFFFF, 0, 0xFFFFFFFF, 0};
__int32 expected4[8] = {1,2,3,0, 0,0,0,0};


__int32 vind5[8] = {0,1,2,3,4,5,6,7};
int mask5[8] =         {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
int expectedMask5[8] = {0,          0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
__int32 expected5[8] = {1,0,0,0,0,0,0,0};

int mask6[8] =         {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
int expectedMask6[8] = {0,          0,          0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
__int32 expected6[8] = {1,2,0,0,0,0,0,0};

int mask7[8] =         {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
int expectedMask7[8] = {0,          0,          0,          0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
__int32 expected7[8] = {1,2,3,0,0,0,0,0};

int mask8[8] =         {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
int expectedMask8[8] = {0,          0,          0,          0,          0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
__int32 expected8[8] = {1,2,3,4,0,0,0,0};

int mask9[8] =         {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
int expectedMask9[8] = {0,          0,          0,          0,          0,          0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
__int32 expected9[8] = {1,2,3,4,5,0,0,0};

int mask10[8] =         {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
int expectedMask10[8] = {0,          0,          0,          0,          0,          0 ,         0xFFFFFFFF, 0xFFFFFFFF};
__int32 expected10[8] = {1,2,3,4,5,6,0,0};

int mask11[8] =         {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
int expectedMask11[8] = {0,          0,          0,          0,          0,          0 ,         0,          0xFFFFFFFF};
__int32 expected11[8] = {1,2,3,4,5,6,7,0};

int mask12[8] =         {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0,          0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF};
int expectedMask12[8] = {0,          0,          0,          0,          0,          0 ,         0,          0xFFFFFFFF};
__int32 expected12[8] = {1,2,3,4,0,6,7,0};


__int64 arr13[4] = {1LL,2LL,3LL,4LL};
__int64 mask13[4] =             {0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expectedMask13[4] =     {0LL,                  0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expected13[4] =     {1LL,0LL,0LL,0LL};

__int64 mask14[4] =             {0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expectedMask14[4] =     {0LL,                  0LL,                  0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expected14[4] =     {1LL,2LL,0LL,0LL};

__int64 mask15[4] =             {0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expectedMask15[4] =     {0LL,                  0LL,                  0LL,                  0xFFFFFFFFFFFFFFFFLL};
__int64 expected15[4] =     {1LL,2LL,3LL,0LL};

__int64 mask16[4] =             {0xFFFFFFFFFFFFFFFFLL, 0LL,                  0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expectedMask16[4] =     {0LL,                  0LL,                  0LL,                  0xFFFFFFFFFFFFFFFFLL};
__int64 expected16[4] =     {1LL,0LL,3LL,0LL};

__int64 mask17[4] =             {0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expectedMask17[4] =     {0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expected17[4] =     {0LL,0LL,0LL,0LL};

__int64 mask18[4] =             {0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expectedMask18[4] =     {0LL,                  0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expected18[4] =     {1LL,0LL,0LL,0LL};

_int64 mask19[4] =             {0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expectedMask19[4] =     {0LL,                  0LL,                  0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expected19[4] =     {1LL,2LL,0LL,0LL};

__int64 mask20[4] =             {0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expectedMask20[4] =     {0LL,                  0LL,                  0LL,                  0xFFFFFFFFFFFFFFFFLL};
__int64 expected20[4] =     {1LL,2LL,3LL,0LL};

__int64 mask21[4] =             {0xFFFFFFFFFFFFFFFFLL, 0LL,                  0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expectedMask21[4] =     {0LL,                  0LL,                  0LL,                  0xFFFFFFFFFFFFFFFFLL};
__int64 expected21[4] =     {1LL,0LL,3LL,0LL};

__int64 mask22[4] =             {0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expectedMask22[4] =     {0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL, 0xFFFFFFFFFFFFFFFFLL};
__int64 expected22[4] =     {0LL,0LL,0LL,0LL};


__int32 *x;

static M256 d, YMMDestBefore, YMMIndexBefore, YMMIndexAfter, YMMMaskBefore, YMMMaskAfter;


void VerifyException(EXCEPTION_RECORD *exceptRecord, CONTEXT *exceptContext, BOOL insIsVgather)
{
    if (exceptRecord->ExceptionCode == 0xc000001d)
    {
        fprintf (stdout,"Exception is Illegal Instruction - test is running on an OS/processor without AVX2 support\n");
        if (!insIsVgather)
        {
            fprintf (stdout,"***  instruction causing exception is NOT vgather\n");
        }
        exit(1);
    }
    else if (exceptRecord->ExceptionCode == 0xc0000005)
    {
        fprintf (stdout,"Exception is Access Violation\n");
        fflush (stdout);
        if (!insIsVgather)
        {
            fprintf (stdout,"***  instruction causing exception is NOT vgather\n");
            exit(1);
        }
    }
    else
    {
        fprintf (stdout,"***Unexpected Exception type\n");
        exit(1);
    }
}

/*!
 * Exception filter for the ExecuteSafe function: copy the exception record 
 * to the specified structure.
 * @param[in] exceptPtr        pointers to the exception context and the exception 
 *                             record prepared by the system
 * @param[out] pExceptRecord   pointer to the structure that receives the 
 *                             exception record
 * @param[out] pExceptContext  pointer to the structure that receives the 
 *                             exception context
 * @return the exception disposition
 */
static int SafeExceptionFilter(LPEXCEPTION_POINTERS exceptPtr, 
                               EXCEPTION_RECORD * pExceptRecord,
                               CONTEXT * pExceptContext)
{
    numExceptions++;
    *pExceptRecord  = *(exceptPtr->ExceptionRecord);
    *pExceptContext = *(exceptPtr->ContextRecord);
    fprintf (stdout, "SafeExceptionFilter: Exception code %x Exception address %p Context IP %p\n",
                   pExceptRecord->ExceptionCode, (ULONG_PTR)(pExceptRecord->ExceptionAddress), GetIp(pExceptContext));
    fflush (stdout);
    
    // continue execution at the instruction following the exception causing instruction
    ULONG_PTR ipToContinueAt = GetIp(pExceptContext);
    char str[128];
    BOOL insIsVgather;
    UINT32 instructionLen = GetInstructionLenAndDisasm((UINT8 *)ipToContinueAt, str, &insIsVgather);
    if (0 == instructionLen)
    {
        fprintf (stderr, "***Error\n");
        exit (1);
    }
    fprintf (stdout,"exception at: %s\n", str);
    fflush (stdout);
    VerifyException(pExceptRecord, pExceptContext, insIsVgather);
    ipToContinueAt 
        = (ULONG_PTR)((UINT8 *)ipToContinueAt + instructionLen);
    fprintf (stdout, "  setting resume ip to    %p\n", ipToContinueAt);
    fflush (stdout);
    instructionLen = GetInstructionLenAndDisasm((unsigned char *)ipToContinueAt, str, &insIsVgather);
    if (0 == instructionLen)
    {
        fprintf (stderr, "***Error 0 length instruction at ip %p\n", ipToContinueAt);
        exit (1);
    }
    SetIp(exceptPtr, ipToContinueAt);

    fprintf (stdout,"    resume instruction is %s\n", str);
    fflush (stdout);
    return EXCEPTION_CONTINUE_EXECUTION;// EXCEPTION_EXECUTE_HANDLER;
}


void Test1()
{
    fprintf (stdout, "\nTest1\n");
    fflush (stdout);
    numExpectedExceptions++;
    // the first two sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 4 32bit masks: mask[0] is on, mask[1] is off, mask[2] is on, mask[3] is off
    __int32 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int32 *>((pages + pageSize)-(2*sizeof(__int32)));
    *vgatherSourceBase = arr1[0];
    *(vgatherSourceBase+1) = arr1[1];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPS ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind1];
        vmovups ymm3, [mask1];  
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherqps xmm1, [GPR + ymm2*4], xmm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {

    }
    printVi("YMM dest  before:     ", YMMDestBefore.i);
    printVi("YMM dest  after:      ", d.i);
    printVl("YMM index before:     ", YMMIndexBefore.l);
    printVl("YMM index after:      ", YMMIndexAfter.l);
    printVi("YMM mask  before:     ", YMMMaskBefore.i);
    printVi("YMM mask  after:      ", YMMMaskAfter.i);

        
  for (int i=0; i < 8; i++) {
      if (d.i[i] != expected1[i])  {
          printf("Failed: 4-byte element# %d expected %d observed %d\n", i, expected1[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.i[i] != expectedMask1[i])  {
          printf("Failed: mask 4-byte element# %d expected %x observed %x\n", i, expectedMask1[i], YMMMaskAfter.i[i]);
         exit (1);
      }
  }
  fflush (stdout);
}


void Test2()
{
    fprintf (stdout, "\nTest2\n");
    numExpectedExceptions++;
    // the first two sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 4 32bit masks: mask[0] is on, mask[1] is on, mask[2] is on, mask[3] is off
    __int32 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int32 *>((pages + pageSize)-(2*sizeof(__int32)));
    *vgatherSourceBase = arr1[0];
    *(vgatherSourceBase+1) = arr1[1];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPS ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind1];
        vmovups ymm3, [mask2];  
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherqps xmm1, [GPR + ymm2*4], xmm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVi("YMM dest  before:     ", YMMDestBefore.i);
    printVi("YMM dest  after:      ", d.i);
    printVl("YMM index before:     ", YMMIndexBefore.l);
    printVl("YMM index after:      ", YMMIndexAfter.l);
    printVi("YMM mask  before:     ", YMMMaskBefore.i);
    printVi("YMM mask  after:      ", YMMMaskAfter.i);

        
  for (int i=0; i < 8; i++) {
      if (d.i[i] != expected2[i])  {
          printf("Failed: 4-byte element# %d expected %d observed %d\n", i, expected2[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.i[i] != expectedMask2[i])  {
          printf("Failed: mask 4-byte element# %d expected %x observed %x\n", i, expectedMask2[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}


void Test3()
{
    fprintf (stdout, "\nTest3\n");
    numExpectedExceptions++;
    // the first  source of the vgather is in the accessible page, the rest are in the
    // inaccessible page
    // 4 32bit masks: mask[0] is on, mask[1] is on, mask[2] is on, mask[3] is off
    __int32 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int32 *>((pages + pageSize)-(sizeof(__int32)));
    *vgatherSourceBase = arr1[0];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPS ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind1];
        vmovups ymm3, [mask3];  
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherqps xmm1, [GPR + ymm2*4], xmm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVi("YMM dest  before:     ", YMMDestBefore.i);
    printVi("YMM dest  after:      ", d.i);
    printVl("YMM index before:     ", YMMIndexBefore.l);
    printVl("YMM index after:      ", YMMIndexAfter.l);
    printVi("YMM mask  before:     ", YMMMaskBefore.i);
    printVi("YMM mask  after:      ", YMMMaskAfter.i);

        
  for (int i=0; i < 8; i++) {
      if (d.i[i] != expected3[i])  {
          printf("Failed: 4-byte element# %d expected %d observed %d\n", i, expected3[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.i[i] != expectedMask3[i])  {
          printf("Failed: mask 4-byte element# %d expected %x observed %x\n", i, expectedMask3[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}


void Test4()
{
    fprintf (stdout, "\nTest4\n");
    numExpectedExceptions++;
    // the first three sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 4 32bit masks: mask[0] is on, mask[1] is on, mask[2] is on, mask[3] is on
    __int32 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int32 *>((pages + pageSize)-(3*sizeof(__int32)));
    *vgatherSourceBase = arr1[0];
    *(vgatherSourceBase+1) = arr1[1];
    *(vgatherSourceBase+2) = arr1[2];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPS ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind1];
        vmovups ymm3, [mask4];  
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherqps xmm1, [GPR + ymm2*4], xmm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVi("YMM dest  before:     ", YMMDestBefore.i);
    printVi("YMM dest  after:      ", d.i);
    printVl("YMM index before:     ", YMMIndexBefore.l);
    printVl("YMM index after:      ", YMMIndexAfter.l);
    printVi("YMM mask  before:     ", YMMMaskBefore.i);
    printVi("YMM mask  after:      ", YMMMaskAfter.i);

        
  for (int i=0; i < 8; i++) {
      if (d.i[i] != expected4[i])  {
          printf("Failed: 4-byte element# %d expected %d observed %d\n", i, expected4[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.i[i] != expectedMask4[i])  {
          printf("Failed: mask 4-byte element# %d expected %x observed %x\n", i, expectedMask4[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}


void Test5()
{
    fprintf (stdout, "\nTest5\n");
    numExpectedExceptions++;
    // the first  source of the vgather is in the accessible page, the rest are in the
    // inaccessible page
    // 8 32bit masks: all are on
    __int32 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int32 *>((pages + pageSize)-(sizeof(__int32)));
    *vgatherSourceBase = arr1[0];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPS ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask5];  
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdps ymm1, [GPR + ymm2*4], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVi("YMM dest  before:     ", YMMDestBefore.i);
    printVi("YMM dest  after:      ", d.i);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVi("YMM mask  before:     ", YMMMaskBefore.i);
    printVi("YMM mask  after:      ", YMMMaskAfter.i);

        
  for (int i=0; i < 8; i++) {
      if (d.i[i] != expected5[i])  {
          printf("Failed: 4-byte element# %d expected %d observed %d\n", i, expected5[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.i[i] != expectedMask5[i])  {
          printf("Failed: mask 4-byte element# %d expected %x observed %x\n", i, expectedMask5[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}

void Test6()
{
    fprintf (stdout, "\nTest6\n");
    numExpectedExceptions++;
    // the first 2 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 8 32bit masks: all are on
    __int32 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int32 *>((pages + pageSize)-(2*sizeof(__int32)));
    *vgatherSourceBase = arr1[0];
    *(vgatherSourceBase+1) = arr1[1];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPS ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask6];  
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdps ymm1, [GPR + ymm2*4], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVi("YMM dest  before:     ", YMMDestBefore.i);
    printVi("YMM dest  after:      ", d.i);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVi("YMM mask  before:     ", YMMMaskBefore.i);
    printVi("YMM mask  after:      ", YMMMaskAfter.i);

        
  for (int i=0; i < 8; i++) {
      if (d.i[i] != expected6[i])  {
          printf("Failed: 4-byte element# %d expected %d observed %d\n", i, expected6[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.i[i] != expectedMask6[i])  {
          printf("Failed: mask 4-byte element# %d expected %x observed %x\n", i, expectedMask6[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}

void Test7()
{
    fprintf (stdout, "\nTest7\n");
    numExpectedExceptions++;
    // the first 3 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 8 32bit masks: all are on
    __int32 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int32 *>((pages + pageSize)-(3*sizeof(__int32)));
    *vgatherSourceBase = arr1[0];
    *(vgatherSourceBase+1) = arr1[1];
    *(vgatherSourceBase+2) = arr1[2];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPS ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask7];  
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdps ymm1, [GPR + ymm2*4], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVi("YMM dest  before:     ", YMMDestBefore.i);
    printVi("YMM dest  after:      ", d.i);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVi("YMM mask  before:     ", YMMMaskBefore.i);
    printVi("YMM mask  after:      ", YMMMaskAfter.i);

        
  for (int i=0; i < 8; i++) {
      if (d.i[i] != expected7[i])  {
          printf("Failed: 4-byte element# %d expected %d observed %d\n", i, expected7[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.i[i] != expectedMask7[i])  {
          printf("Failed: mask 4-byte element# %d expected %x observed %x\n", i, expectedMask7[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}

void Test8()
{
    fprintf (stdout, "\nTest8\n");
    numExpectedExceptions++;
    // the first 4 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 8 32bit masks: all are on
    __int32 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int32 *>((pages + pageSize)-(4*sizeof(__int32)));
    *vgatherSourceBase = arr1[0];
    *(vgatherSourceBase+1) = arr1[1];
    *(vgatherSourceBase+2) = arr1[2];
    *(vgatherSourceBase+3) = arr1[3];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPS ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask8];  
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdps ymm1, [GPR + ymm2*4], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVi("YMM dest  before:     ", YMMDestBefore.i);
    printVi("YMM dest  after:      ", d.i);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVi("YMM mask  before:     ", YMMMaskBefore.i);
    printVi("YMM mask  after:      ", YMMMaskAfter.i);

        
  for (int i=0; i < 8; i++) {
      if (d.i[i] != expected8[i])  {
          printf("Failed: 4-byte element# %d expected %d observed %d\n", i, expected8[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.i[i] != expectedMask8[i])  {
          printf("Failed: mask 4-byte element# %d expected %x observed %x\n", i, expectedMask8[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}

void Test9()
{
    fprintf (stdout, "\nTest9\n");
    numExpectedExceptions++;
    // the first 5 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 8 32bit masks: all are on
    __int32 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int32 *>((pages + pageSize)-(5*sizeof(__int32)));
    *vgatherSourceBase = arr1[0];
    *(vgatherSourceBase+1) = arr1[1];
    *(vgatherSourceBase+2) = arr1[2];
    *(vgatherSourceBase+3) = arr1[3];
    *(vgatherSourceBase+4) = arr1[4];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPS ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask7];  
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdps ymm1, [GPR + ymm2*4], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVi("YMM dest  before:     ", YMMDestBefore.i);
    printVi("YMM dest  after:      ", d.i);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVi("YMM mask  before:     ", YMMMaskBefore.i);
    printVi("YMM mask  after:      ", YMMMaskAfter.i);

        
  for (int i=0; i < 8; i++) {
      if (d.i[i] != expected9[i])  {
          printf("Failed: 4-byte element# %d expected %d observed %d\n", i, expected7[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.i[i] != expectedMask9[i])  {
          printf("Failed: mask 4-byte element# %d expected %x observed %x\n", i, expectedMask9[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}

void Test10()
{
    fprintf (stdout, "\nTest10\n");
    numExpectedExceptions++;
    // the first 6 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 8 32bit masks: all are on
    __int32 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int32 *>((pages + pageSize)-(6*sizeof(__int32)));
    *vgatherSourceBase = arr1[0];
    *(vgatherSourceBase+1) = arr1[1];
    *(vgatherSourceBase+2) = arr1[2];
    *(vgatherSourceBase+3) = arr1[3];
    *(vgatherSourceBase+4) = arr1[4];
    *(vgatherSourceBase+5) = arr1[5];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPS ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask10];  
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdps ymm1, [GPR + ymm2*4], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    
    printVi("YMM dest  before:     ", YMMDestBefore.i);
    printVi("YMM dest  after:      ", d.i);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVi("YMM mask  before:     ", YMMMaskBefore.i);
    printVi("YMM mask  after:      ", YMMMaskAfter.i);

    for (int i=0; i < 8; i++) {
        if (d.i[i] != expected10[i])  {
            printf("Failed: 4-byte element# %d expected %d observed %d\n", i, expected10[i], d.i[i]);
            exit (1);
        }
        if (YMMMaskAfter.i[i] != expectedMask10[i])  {
            printf("Failed: mask 4-byte element# %d expected %x observed %x\n", i, expectedMask10[i], YMMMaskAfter.i[i]);
            exit (1);
        }
    }
}

void Test11()
{
    fprintf (stdout, "\nTest11\n");
    numExpectedExceptions++;
    // the first 7 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 8 32bit masks: all are on
    __int32 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int32 *>((pages + pageSize)-(7*sizeof(__int32)));
    *vgatherSourceBase = arr1[0];
    *(vgatherSourceBase+1) = arr1[1];
    *(vgatherSourceBase+2) = arr1[2];
    *(vgatherSourceBase+3) = arr1[3];
    *(vgatherSourceBase+4) = arr1[4];
    *(vgatherSourceBase+5) = arr1[5];
    *(vgatherSourceBase+6) = arr1[6];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPS ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask11];  
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdps ymm1, [GPR + ymm2*4], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVi("YMM dest  before:     ", YMMDestBefore.i);
    printVi("YMM dest  after:      ", d.i);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVi("YMM mask  before:     ", YMMMaskBefore.i);
    printVi("YMM mask  after:      ", YMMMaskAfter.i);

        
  for (int i=0; i < 8; i++) {
      if (d.i[i] != expected11[i])  {
          printf("Failed: 4-byte element# %d expected %d observed %d\n", i, expected11[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.i[i] != expectedMask11[i])  {
          printf("Failed: mask 4-byte element# %d expected %x observed %x\n", i, expectedMask11[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}


void Test12()
{
    fprintf (stdout, "\nTest12\n");
    numExpectedExceptions++;
    // the first 7 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 8 32bit masks: all are on except mask#5
    __int32 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int32 *>((pages + pageSize)-(7*sizeof(__int32)));
    *vgatherSourceBase = arr1[0];
    *(vgatherSourceBase+1) = arr1[1];
    *(vgatherSourceBase+2) = arr1[2];
    *(vgatherSourceBase+3) = arr1[3];
    *(vgatherSourceBase+4) = arr1[4];
    *(vgatherSourceBase+5) = arr1[5];
    *(vgatherSourceBase+6) = arr1[6];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPS ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask12];  
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdps ymm1, [GPR + ymm2*4], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVi("YMM dest  before:     ", YMMDestBefore.i);
    printVi("YMM dest  after:      ", d.i);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVi("YMM mask  before:     ", YMMMaskBefore.i);
    printVi("YMM mask  after:      ", YMMMaskAfter.i);

        
  for (int i=0; i < 8; i++) {
      if (d.i[i] != expected12[i])  {
          printf("Failed: 4-byte element# %d expected %d observed %d\n", i, expected12[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.i[i] != expectedMask12[i])  {
          printf("Failed: mask 4-byte element# %d expected %x observed %x\n", i, expectedMask12[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}


void Test13()
{
    fprintf (stdout, "\nTest13\n");
    numExpectedExceptions++;
    // the first  source of the vgather is in the accessible page, the rest are in the
    // inaccessible page
    // 4 64bit masks: all are on
    __int64 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int64 *>((pages + pageSize)-(sizeof(__int64)));
    *vgatherSourceBase = arr13[0];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPD ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask13];
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdpd ymm1, [GPR + xmm2*8], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVl("YMM dest  before:     ", YMMDestBefore.l);
    printVl("YMM dest  after:      ", d.l);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVl("YMM mask  before:     ", YMMMaskBefore.l);
    printVl("YMM mask  after:      ", YMMMaskAfter.l);
    

        
  for (int i=0; i < 4; i++) {
      if (d.l[i] != expected13[i])  {
          printf("Failed: 8-byte element# %d expected %d observed %d\n", i, expected13[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.l[i] != expectedMask13[i])  {
          printf("Failed: mask 8-byte element# %d expected %x observed %x\n", i, expectedMask13[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}

void Test14()
{
    fprintf (stdout, "\nTest14\n");
    numExpectedExceptions++;
    // the first 2 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 4 64bit masks: all are on
    __int64 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int64 *>((pages + pageSize)-(2*sizeof(__int64)));
    *vgatherSourceBase = arr13[0];
    *(vgatherSourceBase+1) = arr13[1];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPD ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask14];
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdpd ymm1, [GPR + xmm2*8], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVl("YMM dest  before:     ", YMMDestBefore.l);
    printVl("YMM dest  after:      ", d.l);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVl("YMM mask  before:     ", YMMMaskBefore.l);
    printVl("YMM mask  after:      ", YMMMaskAfter.l);
    

        
  for (int i=0; i < 4; i++) {
      if (d.l[i] != expected14[i])  {
          printf("Failed: 8-byte element# %d expected %d observed %d\n", i, expected14[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.l[i] != expectedMask14[i])  {
          printf("Failed: mask 8-byte element# %d expected %x observed %x\n", i, expectedMask14[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}


void Test15()
{
    fprintf (stdout, "\nTest15\n");
    numExpectedExceptions++;
    // the first 3 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 4 64bit masks: all are on
    __int64 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int64 *>((pages + pageSize)-(3*sizeof(__int64)));
    *vgatherSourceBase = arr13[0];
    *(vgatherSourceBase+1) = arr13[1];
    *(vgatherSourceBase+2) = arr13[2];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPD ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask15];
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdpd ymm1, [GPR + xmm2*8], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVl("YMM dest  before:     ", YMMDestBefore.l);
    printVl("YMM dest  after:      ", d.l);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVl("YMM mask  before:     ", YMMMaskBefore.l);
    printVl("YMM mask  after:      ", YMMMaskAfter.l);
    

        
  for (int i=0; i < 4; i++) {
      if (d.l[i] != expected15[i])  {
          printf("Failed: 8-byte element# %d expected %d observed %d\n", i, expected15[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.l[i] != expectedMask15[i])  {
          printf("Failed: mask 8-byte element# %d expected %x observed %x\n", i, expectedMask15[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}

void Test16()
{
    fprintf (stdout, "\nTest16\n");
    numExpectedExceptions++;
    // the first 3 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 4 64bit masks: all are on except the second one
    __int64 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int64 *>((pages + pageSize)-(3*sizeof(__int64)));
    *vgatherSourceBase = arr13[0];
    *(vgatherSourceBase+1) = arr13[1];
    *(vgatherSourceBase+2) = arr13[2];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPD ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask16];
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdpd ymm1, [GPR + xmm2*8], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVl("YMM dest  before:     ", YMMDestBefore.l);
    printVl("YMM dest  after:      ", d.l);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVl("YMM mask  before:     ", YMMMaskBefore.l);
    printVl("YMM mask  after:      ", YMMMaskAfter.l);
    

        
  for (int i=0; i < 4; i++) {
      if (d.l[i] != expected16[i])  {
          printf("Failed: 8-byte element# %d expected %d observed %d\n", i, expected16[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.l[i] != expectedMask16[i])  {
          printf("Failed: mask 8-byte element# %d expected %x observed %x\n", i, expectedMask16[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}


void Test17()
{
    fprintf (stdout, "\nTest17\n");
    numExpectedExceptions++;
    // all sources are in the inaccessible page
    // 4 64bit masks: all are on
    __int64 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int64 *>((pages + pageSize)-(0*sizeof(__int64)));
    

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPD ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind5];
        vmovups ymm3, [mask17];
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherdpd ymm1, [GPR + xmm2*8], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
     }
    printVl("YMM dest  before:     ", YMMDestBefore.l);
    printVl("YMM dest  after:      ", d.l);
    printVi("YMM index before:     ", YMMIndexBefore.i);
    printVi("YMM index after:      ", YMMIndexAfter.i);
    printVl("YMM mask  before:     ", YMMMaskBefore.l);
    printVl("YMM mask  after:      ", YMMMaskAfter.l);
    

        
  for (int i=0; i < 4; i++) {
      if (d.l[i] != expected17[i])  {
          printf("Failed: 8-byte element# %d expected %d observed %d\n", i, expected17[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.l[i] != expectedMask17[i])  {
          printf("Failed: mask 8-byte element# %d expected %x observed %x\n", i, expectedMask17[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}



void Test18()
{
    fprintf (stdout, "\nTest18\n");
    numExpectedExceptions++;
    // the first  source of the vgather is in the accessible page, the rest are in the
    // inaccessible page
    // 4 64bit masks: all are on
    __int64 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int64 *>((pages + pageSize)-(sizeof(__int64)));
    *vgatherSourceBase = arr13[0];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPD ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind1];
        vmovups ymm3, [mask18];
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherqpd ymm1, [GPR + ymm2*8], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVl("YMM dest  before:     ", YMMDestBefore.l);
    printVl("YMM dest  after:      ", d.l);
    printVl("YMM index before:     ", YMMIndexBefore.l);
    printVl("YMM index after:      ", YMMIndexAfter.l);
    printVl("YMM mask  before:     ", YMMMaskBefore.l);
    printVl("YMM mask  after:      ", YMMMaskAfter.l);
    

        
  for (int i=0; i < 4; i++) {
      if (d.l[i] != expected18[i])  {
          printf("Failed: 8-byte element# %d expected %d observed %d\n", i, expected18[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.l[i] != expectedMask18[i])  {
          printf("Failed: mask 8-byte element# %d expected %x observed %x\n", i, expectedMask18[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}

void Test19()
{
    fprintf (stdout, "\nTest19\n");
    numExpectedExceptions++;
    // the first 2 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 4 64bit masks: all are on
    __int64 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int64 *>((pages + pageSize)-(2*sizeof(__int64)));
    *vgatherSourceBase = arr13[0];
    *(vgatherSourceBase+1) = arr13[1];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPD ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind1];
        vmovups ymm3, [mask19];
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherqpd ymm1, [GPR + ymm2*8], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVl("YMM dest  before:     ", YMMDestBefore.l);
    printVl("YMM dest  after:      ", d.l);
    printVl("YMM index before:     ", YMMIndexBefore.l);
    printVl("YMM index after:      ", YMMIndexAfter.l);
    printVl("YMM mask  before:     ", YMMMaskBefore.l);
    printVl("YMM mask  after:      ", YMMMaskAfter.l);
    

        
  for (int i=0; i < 4; i++) {
      if (d.l[i] != expected19[i])  {
          printf("Failed: 8-byte element# %d expected %d observed %d\n", i, expected19[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.l[i] != expectedMask19[i])  {
          printf("Failed: mask 8-byte element# %d expected %x observed %x\n", i, expectedMask19[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}


void Test20()
{
    fprintf (stdout, "\nTest20\n");
    numExpectedExceptions++;
    // the first 3 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 4 64bit masks: all are on
    __int64 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int64 *>((pages + pageSize)-(3*sizeof(__int64)));
    *vgatherSourceBase = arr13[0];
    *(vgatherSourceBase+1) = arr13[1];
    *(vgatherSourceBase+2) = arr13[2];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPD ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind1];
        vmovups ymm3, [mask20];
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherqpd ymm1, [GPR + ymm2*8], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVl("YMM dest  before:     ", YMMDestBefore.l);
    printVl("YMM dest  after:      ", d.l);
    printVl("YMM index before:     ", YMMIndexBefore.l);
    printVl("YMM index after:      ", YMMIndexAfter.l);
    printVl("YMM mask  before:     ", YMMMaskBefore.l);
    printVl("YMM mask  after:      ", YMMMaskAfter.l);
    

        
  for (int i=0; i < 4; i++) {
      if (d.l[i] != expected20[i])  {
          printf("Failed: 8-byte element# %d expected %d observed %d\n", i, expected20[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.l[i] != expectedMask20[i])  {
          printf("Failed: mask 8-byte element# %d expected %x observed %x\n", i, expectedMask20[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}

void Test21()
{
    fprintf (stdout, "\nTest21\n");
    numExpectedExceptions++;
    // the first 3 sources of the vgather are in the accessible page, the rest are in the
    // inaccessible page
    // 4 64bit masks: all are on except the second one
    __int64 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int64 *>((pages + pageSize)-(3*sizeof(__int64)));
    *vgatherSourceBase = arr13[0];
    *(vgatherSourceBase+1) = arr13[1];
    *(vgatherSourceBase+2) = arr13[2];

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPD ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind1];
        vmovups ymm3, [mask20];
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherqpd ymm1, [GPR + ymm2*8], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVl("YMM dest  before:     ", YMMDestBefore.l);
    printVl("YMM dest  after:      ", d.l);
    printVl("YMM index before:     ", YMMIndexBefore.l);
    printVl("YMM index after:      ", YMMIndexAfter.l);
    printVl("YMM mask  before:     ", YMMMaskBefore.l);
    printVl("YMM mask  after:      ", YMMMaskAfter.l);
    

        
  for (int i=0; i < 4; i++) {
      if (d.l[i] != expected20[i])  {
          printf("Failed: 8-byte element# %d expected %d observed %d\n", i, expected20[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.l[i] != expectedMask20[i])  {
          printf("Failed: mask 8-byte element# %d expected %x observed %x\n", i, expectedMask20[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}


void Test22()
{
    fprintf (stdout, "\nTest22\n");
    numExpectedExceptions++;
    // all sources are in the inaccessible page
    // 4 64bit masks: all are on
    __int64 *vgatherSourceBase;
    vgatherSourceBase = reinterpret_cast<__int64 *>((pages + pageSize)-(0*sizeof(__int64)));
    

   init_long(&d, 0xdeadbeef);
   EXCEPTION_RECORD exceptRecord;
   CONTEXT exceptContext;
    __try 
    { 
        
    __asm {     /* VGATHERDPD ymm1, [rax + ymm2_vind*4], ymm3_mask */
        mov GPR, vgatherSourceBase
        vxorps  ymm1, ymm1, ymm1;
        vmovups ymm2, [vind1];
        vmovups ymm3, [mask22];
        vmovups [YMMDestBefore.ms],  ymm1;
        vmovups [YMMIndexBefore.ms], ymm2;
        vmovups [YMMMaskBefore.ms],  ymm3;
        vgatherqpd ymm1, [GPR + ymm2*8], ymm3
        vmovups [d], ymm1;
        vmovups [YMMIndexAfter.ms], ymm2;
        vmovups [YMMMaskAfter.ms],  ymm3;
    }
    }
     __except (SafeExceptionFilter(GetExceptionInformation(), &exceptRecord, &exceptContext))
    {
    }
    printVl("YMM dest  before:     ", YMMDestBefore.l);
    printVl("YMM dest  after:      ", d.l);
    printVl("YMM index before:     ", YMMIndexBefore.l);
    printVl("YMM index after:      ", YMMIndexAfter.l);
    printVl("YMM mask  before:     ", YMMMaskBefore.l);
    printVl("YMM mask  after:      ", YMMMaskAfter.l);
    

        
  for (int i=0; i < 4; i++) {
      if (d.l[i] != expected22[i])  {
          printf("Failed: 8-byte element# %d expected %d observed %d\n", i, expected22[i], d.i[i]);
          exit (1);
      }
      if (YMMMaskAfter.l[i] != expectedMask22[i])  {
          printf("Failed: mask 8-byte element# %d expected %x observed %x\n", i, expectedMask22[i], YMMMaskAfter.i[i]);
          exit (1);
      }
  }
}

int main()
{
    int i;
    InitXed();
    // Allocate two adjacent pages. First page has all access rights
    // and the second page is inaccessible.
    pageSize = GetPageSize();
    pages = (char *)MemAlloc(2*pageSize, MEM_READ_WRITE_EXEC);
    if (pages == 0) 
    {
        fprintf(stderr, "MemAlloc failed");
        exit (1);
    }
    BOOL protectStatus = MemProtect(pages + pageSize, pageSize, MEM_INACESSIBLE);
    if (!protectStatus) 
    {
        fprintf(stderr, "MemProtect failed");
        exit (1);
    }   

    Test1();
    Test2();
    Test3();
    Test4();
    Test5();
    Test6();
    Test7();
    Test8();
    Test9();
    Test10();
    Test11();
    Test12();
    Test13();
    Test14();
    Test15();
    Test16();
    Test17();
    Test18();
    Test19();
    Test20();
    Test21();
    Test22();
    
    if (numExceptions!=numExpectedExceptions)
    {
        printf ("Failed: expected numExpectedExceptions exceptions, got %d\n", numExpectedExceptions, numExceptions);
        return 1;
    }
    printf("PASSED\n");
    return 0;
}
