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
/*
 * @ORIGINAL_AUTHOR: Jim Cownie
 */

/*
 * Generate a variety of reasons for abort inside RTM and check that we report them 
 * correctly in the resulting EAX value.
 *
 * We test 
 *   * SEGV: protection fault (attempt to write to address zero)
 *   * SEGV (attempt to read from address zero)
 *   * Illegal instruction
 *   * Explicit abort (with two different immediates)
 *
 * The mapping of abort reasons to bits in EAX after the abort is somewhat
 * unclear in the document we have.
 *    What aborts should map to RETRY?
 *    How should synchronous exceptions be reported?
 * Best I can tell they report as RETRY, which seems weird, but they don't fit
 * under any of the other categories.
 */
#if TARGET_WINDOWS
#include <windows.h>
#endif
#include <stdio.h>
#include <assert.h>

#include "test_rtm.h"
#include "test_util.h"

static void generateWriteSEGV()
{
    *(int volatile *)0 = -1;
}

static void generateReadSEGV()
{
    *(int volatile *)0;
}


static void execute_unfriendly()
{
#if (TARGET_WINDOWS)
    _asm {
        pause
    }
#else
    __asm__ volatile ("pause\n" : : : "memory" );
#endif
}

// when the nesting level is exceeded, the transcation aborts
// and the control jumps to the outermost xbegin's abort handler
static void exceed_nesting_depth()
{
    XBeginEax();
    exceed_nesting_depth();
}

static void generateAbort99()
{
    emitXAbort(0x99);
}

static void generateAbort1()
{
    emitXAbort(0x01);
}

static void abort_1_in_nested_rtm()
{
    XBeginEax();
    emitXAbort(0x01);
}

/* Assume the cache is less than 2MiB, so if we write 2MiB we'll cause a capacity abort. */
#define INTS_IN_BUFFER (2*1024*1024/sizeof(int))
static int largeBuffer[INTS_IN_BUFFER];

static void generateCapacityAbort()
{
    // Ideally we'd like to know the full cache parameters here, 
    // but it's enough to know the line size, if we assume the cache 
    // is relatively small.
    int i;
    for( i=0; i<INTS_IN_BUFFER; i+=CACHE_LINE_SIZE/sizeof(int) )
        largeBuffer[i] += 1;
}

struct TestDefinition {
    const char* name;
    void (*function)();
    int  expectedEAX;
};

/*
 * We used to expect
 *  'write SEGV' to return XA_mask_retry
 *  'read SEGV'  to return XA_mask_retry
 *  'illegal op' to return XA_mask_retry
 *  'cache capacity' to return XA_mask_capacity
 * But real HSW A1 (as of Feb 16, 2012) returns 0's 
 * for all the above cases
 */
static struct TestDefinition tests_rtm[] = {
   // {"write SEGV",     generateWriteSEGV,     0},
   // {"read SEGV",      generateReadSEGV,      0},
   // {"unfriendly",     execute_unfriendly,    0},
   // {"cache capacity", generateCapacityAbort, 0},
   // {"nesting depth",  exceed_nesting_depth,  XA_mask_nested},
   // {"abort_in_nested_rtm", abort_1_in_nested_rtm,   (0x01<<XA_xabort_value_shift)|XA_mask_xabort|XA_mask_nested},
    
    {"xabort 0x01",    generateAbort1,        (0x01<<XA_xabort_value_shift) | XA_mask_xabort},
    {"xabort 0x99",    generateAbort99,       (0x99<<XA_xabort_value_shift) | XA_mask_xabort}
};

/*
 * from rtm.h, we have 
    XA_mask_xabort                           
    XA_mask_lost_monitoring_line_snoop_read  
    XA_mask_lost_monitoring_line_snoop_write 
    XA_mask_cache_capacity                   
    XA_mask_buffer_exceed                    
    XA_mask_unfriendly_instruction           
    XA_mask_hw_interrupt                     
    XA_mask_exception                        
    XA_mask_nesting_depth_exceed             
    XA_mask_cache_miss                       
    XA_mask_debugging_event                  

  We cannot emulate XA_mask_buffer_exceed, XA_mask_hw_interrupt,
  and XA_mask_cache_miss.

  We test XA_mask_debugging_event as part of idb support.

  We cannot distinguish XA_mask_lost_monitoring_line_snoop_read  
  and XA_mask_lost_monitoring_line_snoop_write. 

  To-Do: XA_mask_unfriendly_instruction. XA_mask_nesting_depth_exceed
*/

int do_test_abort99( )
{
    int tested, i, e=0;
    int eaxValue = XBeginEax();
    if( eaxValue==-1 ) {
        // we are speculating
        emitXAbort(0x99);
        XEnd();
        ++e;
        printf ("  unexpected spec success\n");
    } else { // Speculation failed, that's what we expected, check the status value.
        e += eaxValue!=((0x99<<XA_xabort_value_shift) | XA_mask_xabort);
        printf ("  abort eaxValue %p expectedEAX %p\n", (void *)eaxValue, (void *)((0x99<<XA_xabort_value_shift) | XA_mask_xabort));
    }

    return e;
}

int do_test_abort1( )
{
    int tested, i, e=0;
        int eaxValue = XBeginEax();
        if( eaxValue==-1 ) {
            // we are speculating
            emitXAbort(0x01);
            XEnd();
            ++e;
            printf ("  unexpected spec success\n");
        } else { // Speculation failed, that's what we expected, check the status value.
            e += eaxValue!=((0x01<<XA_xabort_value_shift) | XA_mask_xabort);
            printf ("  abort eaxValue %p expectedEAX %p\n", (void *)eaxValue, (void *)((0x01<<XA_xabort_value_shift) | XA_mask_xabort));
        }

    return e;
}

int do_test_capacity_abort( )
{
    int tested, i, e=0;
        
        int eaxValue = XBeginEax();
        if( eaxValue==-1 ) {
            // we are speculating
            // Ideally we'd like to know the full cache parameters here, 
            // but it's enough to know the line size, if we assume the cache 
            // is relatively small.
            {
                int i;
                for( i=0; i<INTS_IN_BUFFER; i+=CACHE_LINE_SIZE/sizeof(int) )
                    largeBuffer[i] += 1;
            }
            XEnd();
            ++e;
            printf ("  unexpected spec success\n");
        } else { // Speculation failed, that's what we expected, check the status value.
            e += (eaxValue!=0);
            printf ("  abort eaxValue %p expectedEAX %p\n", (void *)eaxValue, (void *)(0));
        }

    return e;
}


int do_test_writesegv_abort( )
{
    int tested, i, e=0;
        
        int eaxValue = XBeginEax();
        if( eaxValue==-1 ) {
            *(int volatile *)0 = -1;
            XEnd();
            ++e;
            printf ("  unexpected spec success\n");
        } else { // Speculation failed, that's what we expected, check the status value.
            e += (eaxValue!=0);
            printf ("  abort eaxValue %p expectedEAX %p\n", (void *)eaxValue, (void *)(0));
        }

    return e;
}

int do_test_readsegv_abort( )
{
    int tested=1, i, e=0;
        
        int eaxValue = XBeginEax();
        if( eaxValue==-1 ) {
            tested = *(int volatile *)0;
            XEnd();
            ++e;
            printf ("  unexpected spec success tested %d\n", tested);
        } else { // Speculation failed, that's what we expected, check the status value.
            e += (eaxValue!=0);
            printf ("  abort eaxValue %p expectedEAX %p\n", (void *)eaxValue, (void *)(0));
        }

    return e;
}

int do_test_unfriendly_abort( )
{
    int tested, i, e=0;
        
        int eaxValue = XBeginEax();
        if( eaxValue==-1 ) {
#if (TARGET_WINDOWS)
    _asm {
        pause
    }
#else
    __asm__ volatile ("pause\n" : : : "memory" );
#endif
            XEnd();
            ++e;
            printf ("  unexpected spec success\n");
        } else { // Speculation failed, that's what we expected, check the status value.
            e += (eaxValue!=0);
            printf ("  abort eaxValue %p expectedEAX %p\n", (void *)eaxValue, (void *)(0));
        }

    return e;
}



int main(int argc, char ** argv)
{
    int errors=0;

    printf ("do_test_abort99\n");
    errors += do_test_abort99();
    printf ("do_test_abort1\n");
    errors += do_test_abort1();
    printf ("do_test_capacity_abort\n");
    errors += do_test_capacity_abort();
    printf ("do_test_readsegv_abort\n");
    errors += do_test_readsegv_abort();
    printf ("do_test_writesegv_abort\n");
    errors += do_test_writesegv_abort();
    printf ("do_test_unfriendly_abort\n");
    errors += do_test_unfriendly_abort();

    return TEST_EPILOGUE( errors==0 ); 
}
