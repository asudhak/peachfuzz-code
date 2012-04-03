/*BEGIN_LEGAL 
Intel Open Source License 

Copyright (c) 2002-2011 Intel Corporation. All rights reserved.
 
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


/* ===================================================================== */
/*! @file
 * Insert a call before and after a stdcall function in probe mode.
 */

/* ===================================================================== */
#include "pin.H"

#if defined (TARGET_WINDOWS)
namespace WINDOWS
{
#include<Windows.h>
}
#endif
#include <stdio.h>
using namespace std;

/* ===================================================================== */
typedef int ( __stdcall * FuncType)( int, int, int, int, int, int, int, int, int, int );

/* ===================================================================== */
/* Analysis routines  */
/* ===================================================================== */

VOID Before()
{
}


VOID After(  )
{
}


/* ===================================================================== */
/* Instrumentation routines  */
/* ===================================================================== */

VOID Sanity(IMG img, RTN rtn)
{
    if ( PIN_IsProbeMode() && ! RTN_IsSafeForProbedInsertion( rtn ) )
    {
        printf ("Cannot insert calls around %s() in %s\n", RTN_Name(rtn).c_str(), IMG_Name(img).c_str());
        exit(1);
    }
}

/* ===================================================================== */
VOID ImageLoad(IMG img, VOID *v)
{
       // Walk through the symbols in the symbol table.
       //
    for (SYM sym = IMG_RegsymHead(img); SYM_Valid(sym); sym = SYM_Next(sym))
    {
        string undFuncName = PIN_UndecorateSymbolName(SYM_Name(sym), UNDECORATION_NAME_ONLY);

        //  Find the LocalAlloc function.
        if (undFuncName == "StdCallFunctionToBeReplacedByPin")
        {
            RTN rtn = RTN_FindByAddress(IMG_LowAddress(img) + SYM_Value(sym));
            if (RTN_Valid(rtn))
            {
                Sanity(img, rtn);
                     
                printf ( "Inserting calls before/after StdCallFunctionToBeReplacedByPin() in %s at address %x\n",
                        IMG_Name(img).c_str(), RTN_Address(rtn));
                     
                PROTO protoOfStdCallFunction1ToBeReplacedByPin 
                  = PROTO_Allocate( PIN_PARG(void *), 
                                  CALLINGSTD_STDCALL,
                                  "protoOfStdCallFunction1ToBeReplacedByPin", 
                                  PIN_PARG(char), 
                                  PIN_PARG(int),
                                  PIN_PARG(char), 
                                  PIN_PARG(int),
                                  PIN_PARG_END() );
                     
                RTN_InsertCallProbed(
                    rtn, IPOINT_AFTER, AFUNPTR( After ),
                    IARG_PROTOTYPE, protoOfStdCallFunction1ToBeReplacedByPin,
                    IARG_END	);

                
                     
                RTN_InsertCallProbed(
                    rtn, IPOINT_BEFORE, AFUNPTR( Before ),
                    IARG_PROTOTYPE, protoOfStdCallFunction1ToBeReplacedByPin,
                    IARG_END);
                     
                     
                PROTO_Free( protoOfStdCallFunction1ToBeReplacedByPin );
            }
        }

        else if (undFuncName == "StdCallFunction2ToBeReplacedByPin")
        {
            RTN rtn = RTN_FindByAddress(IMG_LowAddress(img) + SYM_Value(sym));
            if (RTN_Valid(rtn))
            {
                Sanity(img, rtn);
                     
                printf ( "Inserting call before StdCallFunction2ToBeReplacedByPin() in %s at address %x\n",
                        IMG_Name(img).c_str(), RTN_Address(rtn));
                     
                PROTO protoOfStdCallFunction2ToBeReplacedByPin 
                  = PROTO_Allocate( PIN_PARG(void *), 
                                  CALLINGSTD_STDCALL,
                                  "protoOfStdCallFunction2ToBeReplacedByPin", 
                                  PIN_PARG(char), 
                                  PIN_PARG(int),
                                  PIN_PARG(char), 
                                  PIN_PARG(int),
                                  PIN_PARG_END() );
                
                     
                RTN_InsertCallProbed(
                    rtn, IPOINT_BEFORE, AFUNPTR( Before ),
                    IARG_PROTOTYPE, protoOfStdCallFunction2ToBeReplacedByPin,
                    IARG_END);
                     
                     
                PROTO_Free( protoOfStdCallFunction2ToBeReplacedByPin );
            }
        }

        else if (undFuncName == "StdCallFunction3ToBeReplacedByPin")
        {
            RTN rtn = RTN_FindByAddress(IMG_LowAddress(img) + SYM_Value(sym));
            if (RTN_Valid(rtn))
            {
                Sanity(img, rtn);
                     
                printf ( "Inserting call after StdCallFunction3ToBeReplacedByPin() in %s at address %x\n",
                        IMG_Name(img).c_str(), RTN_Address(rtn));
                     
                PROTO protoOfStdCallFunction3ToBeReplacedByPin 
                  = PROTO_Allocate( PIN_PARG(void *), 
                                  CALLINGSTD_STDCALL,
                                  "protoOfStdCallFunction3ToBeReplacedByPin", 
                                  PIN_PARG(char), 
                                  PIN_PARG(int),
                                  PIN_PARG(char), 
                                  PIN_PARG(int),
                                  PIN_PARG_END() );
                
                     
                RTN_InsertCallProbed(
                    rtn, IPOINT_AFTER, AFUNPTR( After ),
                    IARG_PROTOTYPE, protoOfStdCallFunction3ToBeReplacedByPin,
                    IARG_END);
                     
                     
                PROTO_Free( protoOfStdCallFunction3ToBeReplacedByPin );
            }
        }
    }
}



/* ===================================================================== */

int main(INT32 argc, CHAR *argv[])
{
    PIN_InitSymbols();
    
    PIN_Init(argc, argv);
    
    IMG_AddInstrumentFunction(ImageLoad, 0);
    
    PIN_StartProgramProbed();
    
    return 0;
}



/* ===================================================================== */
/* eof */
/* ===================================================================== */
