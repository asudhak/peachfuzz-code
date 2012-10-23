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

#ifndef __HLE_RTM_H
#define __HLE_RTM_H

#define FUND_CPU_IA32    1
#define FUND_CPU_INTEL64 2

#define FUND_OS_WINDOWS  1
#define FUND_OS_LINUX    2

/* Instruction prefixes */
#define OP_XACQUIRE 0xF2
#define OP_XRELEASE 0xF3
#define OP_LOCK    0xF0

#define STRINGIZE_INTERNAL(arg) #arg
#define STRINGIZE(arg) STRINGIZE_INTERNAL(arg)

#if TARGET_WINDOWS
#ifndef _WINDEF_
// from msdn : Windows Data Types
#define WINAPI __stdcall
typedef unsigned long DWORD;
#if __cplusplus
extern "C"  {
#endif
void WINAPI Sleep( DWORD millisec );
long WINAPI SwitchToThread(void);
#if __cplusplus
}
#endif
#endif /* _WINDEF_ */
#endif /* TARGET_WINDOWS */

#if TARGET_WINDOWS
# define yield() SwitchToThread()
#else
# include <sched.h>
# define yield() sched_yield()
#endif

#include "hle_stdint.h"

static __inline int tryElidedLock( int* l )
{
    int value = 1;

#if TARGET_WINDOWS
# if TARGET_IA32
    __asm {
        mov eax,l
        mov edx,value
        _emit OP_XACQUIRE
        _emit OP_LOCK
        xchg edx, dword ptr [eax]
        mov value, edx
    }
# else
    __asm {
        mov rax,l
        mov edx,value
        _emit OP_XACQUIRE
        _emit OP_LOCK
        xchg edx, dword ptr [rax]
        mov value, edx
    }
# endif
#else
    __asm__ volatile (".byte " STRINGIZE(OP_XACQUIRE)"; lock; xchgl %0, 0(%1);"
                     : "=r"(value), "=r"(l) : "0"(value), "1"(l) : "memory" );
#endif

    return value != 1;
}

static __inline void pauseInstruction()
{
#if TARGET_WINDOWS
    __asm pause
#else
    __asm__ volatile ("pause");
#endif
}

static __inline void hle_machine_pause( int delay ) {
    int i;
    for( i=0; i<delay; ++i ) {
        pauseInstruction();
    }
}

static __inline void exp_backoff( int* cp ) {
    if( *cp<=16 ) {
        hle_machine_pause( *cp );
        /* Pause twice as long the next time. */
        *cp *= 2;
    } else {
        yield();
    }
}

/*! 
 * Elided lock based on xchg.
 *
 * Starting from the HLE emulator release v2556, the 'LOCK' prefix 
 * is NOT REQUIRED for xchg.  Having it is also allowed.  
 * Whether LOCK is present or not, HLE will recognize this as a valid 
 * elided lock sequence.
 * @param [in] l -- address to the lock word
 */
static __inline void elidedLock( int* l )
{
    if( !tryElidedLock( l ) ) {
        /* 
         * We failed to claim the lock, but we probably *are* executing 
         * speculatively.  If we try to claim the lock again, we'll fail 
         * again until the holder releases it.  When the lock is freed,
         * all waiting threads that are speculatively trying to claim the 
         * lock would abort because releasing the lock changes the lock 
         * value.  All threads then try to claim the lock non-speculatively
         * (hence, the lemmings effect).  Therefore we want to abort the 
         * speculation here and then try to reclaim the lock speculatively
         * again.  The pauseInstruction() instruction does that for us.
         */
        int count = 1;
        do {
            exp_backoff( &count );
        } while ( !tryElidedLock( l ) );
    }
}

static __inline void test_and_elidedLock( int* l )
{
    do {
        int count = 1;
        while( *l==1 )
            exp_backoff( &count );
    } while( !tryElidedLock( l ) );
}

/*!
 * Elided unlock using xchg, 
 * though 'mov' would be semantically OK 
 * @param [in] l -- address to the lock word
 */
static __inline void elidedUnlock( int* l )
{
    int value = 0;
#if (TARGET_WINDOWS)
# if (TARGET_IA32)
    __asm  {
        mov eax,l
        mov edx,value
        _emit OP_XRELEASE
        _emit OP_LOCK
        xchg edx,dword ptr [eax]
    }
# else
    __asm {
        mov rax,l
        mov edx,value
        _emit OP_XRELEASE
        _emit OP_LOCK
        xchg edx,dword ptr [rax]
     }
# endif
#else
    __asm__ volatile (".byte " STRINGIZE(OP_XRELEASE)"; lock; xchgl %0, 0(%1)" : 
                      "=r"(value), "=r"(l) : "0"(value), "1"(l) : "memory" );
#endif
}

/*!
 * Elided lock based on increment.
 * @param [in] l -- address to the lock word
 */
static __inline void elidedLockInc( int* l )
{
#if TARGET_WINDOWS
# if TARGET_IA32
    __asm  {
        mov eax,l
        _emit OP_XACQUIRE
        _emit OP_LOCK
        inc dword ptr [eax]
   }
# else
    __asm {
        mov rax,l
        _emit OP_XACQUIRE
        _emit OP_LOCK
        inc dword ptr [rax]
   }
# endif
#else    
    __asm__ volatile (".byte " STRINGIZE(OP_XACQUIRE)"; lock; incl (%0)" : :"r" (l):"memory");
#endif
}

/*!
 * Elided unlock based on increment.
 * @param [in] l -- address to the lock word
 */
static __inline void elidedUnlockDec( int* l )
{
#if (TARGET_WINDOWS)
# if (TARGET_IA32)
    __asm  {
        mov eax,l
        _emit OP_XRELEASE
        _emit OP_LOCK
        dec dword ptr [eax]
    }
# else
    __asm {
        mov rax,l
        _emit OP_XRELEASE
        _emit OP_LOCK
        dec dword ptr [rax]
    }
# endif
#else    
    __asm__ volatile (".byte "STRINGIZE(OP_XRELEASE)"; lock; decl (%0)" : :"r" (l):"memory");
#endif
}

/* Elision friendly ticket lock.
 * This works, but may not really be worthwhile, since it suffers from the Lemming effect,
 * and it's unclear why you would be using a ticket lock if you don't expect it to have
 * people queued on it...
 */
struct MyTicketLock
{
    int volatile MyTicketLockHead;                                /* Ticket currently being serviced */
    int volatile MyTicketLockTail;                                /* Next free ticket to be taken */
};

static int claimElidedTicket( struct MyTicketLock* tl )
{
    register int myTicket;

#if (TARGET_WINDOWS)
# if (TARGET_IA32)
    _asm {
        mov   eax,1;
        mov   ecx,tl;
        _emit OP_XACQUIRE;
        _emit OP_LOCK;
        xadd  [ecx].MyTicketLockTail,eax;
        mov   myTicket, eax;
    }
# else
    _asm {
        mov   eax,1;
        mov   rcx,tl;
        _emit OP_XACQUIRE;
        _emit OP_LOCK;
        xadd  [rcx].MyTicketLockTail,eax;
        mov   myTicket, eax;
    }
# endif
#else
    myTicket = 1;
    __asm__ volatile (".byte " STRINGIZE(OP_XACQUIRE) ";"
                      "lock; xadd %0,4(%1)" : "=r"(myTicket) : "r"(tl), "0"(myTicket) : "memory");
#endif

    return myTicket;
}

static void waitForMyTurn (struct MyTicketLock *tl, int myTicket)
{
    int count = 1;
    /* Wait for our turn */
    while( myTicket!=tl->MyTicketLockHead )
        exp_backoff( &count );
}

static void releaseElidedTicketLock (struct MyTicketLock *tl)
{
    /* Only the unlocker ever writes to MyTicketLockHead, so there's no need 
     * to use a locked increment on it. Logically we need an sfence 
     * before the increment, except that the previous memory operation
     * was the lock cmpxchg, which is a fence anyway, so there are no 
     * writes which haven't already been fenced that could drop below 
     * the increment.
     *
     * The logic here is that if no-one has claimed another ticket, then 
     * we put our ticket back, otherwise we increment the MyTicketLockHead so that 
     * people with later tickets get served.
     */
       
#if (TARGET_WINDOWS)
# if (TARGET_IA32)
    _asm {
        mov   ebx,tl            ;
        mov   eax,[ebx].MyTicketLockHead    ;
        mov   ecx, eax          ;
        inc   eax               ; Compute expected MyTicketLockTail if no-one has claimed a ticket
        _emit OP_XRELEASE;
        lock cmpxchg  [ebx].MyTicketLockTail,ecx;
        jz    UnLocked          ; No one had, so we succesfully put our ticket back
        inc  [ebx].MyTicketLockHead;        ; Someone has claimed another so we must increment MyTicketLockHead
    UnLocked:
    }
# else
    _asm {
        mov   rbx,tl
        mov   eax,[rbx].MyTicketLockHead;
        mov   ecx, eax
        inc   eax
        _emit OP_XRELEASE;
        lock cmpxchg  [rbx].MyTicketLockTail,ecx;
        jz    UnLocked;
        inc  [rbx].MyTicketLockHead;
    UnLocked:
    }
# endif
#else
    int headValue = tl->MyTicketLockHead;
    int nextValue = headValue+1;
    __asm__ volatile (
                      "   .byte " STRINGIZE(OP_XRELEASE) ";"
                      "   lock; cmpxchgl %0,4(%1);"
                      "   jz 1f;"
                      "   incl (%1);"
                      "1:": "=r"(headValue) : "r"(tl), "0"(headValue), "a"(nextValue) : "memory");
#endif
}

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
        _emit 0xC7
        _emit 0xF8
        _emit 2
        _emit 0
        _emit 0
        _emit 0
        jmp   L2
        mov   res, 0
    L2:
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

/*! 
 * Additional version of XBegin which returns -1 on speculation, 
 * and the value of EAX on an abort.
 */
static __inline int XBeginEax()
{
    int res = -1;
    
#if TARGET_WINDOWS
#if TARGET_INTEL64
    _asm {
        _emit 0xC7
        _emit 0xF8
        _emit 2
        _emit 0
        _emit 0
        _emit 0
        jmp   L2
        mov   res, eax
    L2:
    }
#else /* TARGET_IA32 */
    _asm {
        _emit 0xC7
        _emit 0xF8
        _emit 2
        _emit 0
        _emit 0
        _emit 0
        jmp   L2
        mov   res, eax
    L2:
    }
#endif /* TARGET_INTEL64 */
#else /* LINUX */
    /* Note that %eax must be noted as killed (clobbered), because 
     * the XSR is returned in %eax(%rax) on abort.  Other register 
     * values are restored, so don't need to be killed.
     *
     * We must also mark 'res' as an input and an output, since otherwise 
     * 'res=-1' may be dropped as being dead, whereas we do need the 
     * assignment on the successful (i.e., non-abort) path.
     */
#if TARGET_INTEL64
    __asm__ volatile ("1: .byte  0xC7; .byte 0xF8;\n"
                      "   .long  1f-1b-6\n"
                      "    jmp   2f\n"
                      "1:  movl  %%eax,%0\n"
                      "2:"
                      :"=r"(res):"0"(res):"memory","%eax");
#else /* if TARGET_IA32 */
    __asm__ volatile ("1: .byte  0xC7; .byte 0xF8;\n"
                      "   .long  1f-1b-6\n"
                      "    jmp   2f\n"
                      "1:  movl  %%eax,%0\n"
                      "2:"
                      :"=r"(res):"0"(res):"memory","%eax");
#endif /* TARGET_INTEL64 */
#endif /* TARGET_WINDOWS */
    return res;
}

/* This is a macro, the argument must be a single byte constant which
 * can be evaluated by the inline assembler, since it is emitted as a
 * byte into the assembly code.
 */
#if TARGET_WINDOWS
#define emitXAbort(ARG) \
    _asm _emit 0xc6     \
    _asm _emit 0xf8     \
    _asm _emit ARG
#else
#define emitXAbort(ARG) \
    __asm__ volatile (".byte 0xC6; .byte 0xF8; .byte " STRINGIZE(ARG) :::"memory");
#endif

/* Xabort bits in EAX */
#if IDB_SUPPORT || INTEL_PRIVATE
#include "../Emulator/rtm_idb.h"
#endif

/* isintx */
#if TARGET_WINDOWS
#define emitXTEST() \
    _asm _emit 0x0F  \
    _asm _emit 0x01  \
    _asm _emit 0xD6
#else
#define emitXTEST() \
    __asm__ volatile (".byte 0x0F; .byte 0x01; .byte 0xD6" :::"memory");
#endif

static __inline int IsInTx()
{
#if TARGET_WINDOWS
    int8_t res = 0;
    emitXTEST()
    __asm {
        setz ah
        mov  res, ah
    }
    return res==0;
#else
    int8_t res = 0;
    __asm__ __volatile__ (".byte 0x0F; .byte 0x01; .byte 0xD6;\n" 
                          "setz %0" : "=r"(res) : : "memory" );
#endif
    return res==0;
}

/* xnmov */
#if TARGET_WINDOWS
#define emitXNMOV() \
    _asm _emit 0x0F  \
    _asm _emit 0x38  \
    _asm _emit 0xF4
#define emitOperandModifier() _asm _emit 0x66
#define emitOperandModifierRexW() _asm _emit 0x48
#else
#define emitXNMOV() __asm__ volatile (".byte 0x0F; .byte 0x38; .byte 0xF4;" :::"memory")
#define emitOperandModifier66() __asm__ volatile (".byte 0x66;" :::"memory")
#define emitOperandModifierRexW() __asm__ volatile (".byte 0x48;" :::"memory")
#endif

#if TARGET_WINDOWS
#define EMIT_SIZE_MODIFIER_SIZE_MOD_16  _emit 0x66
#define EMIT_SIZE_MODIFIER_SIZE_MOD_64  _emit 0x48
#define EMIT_SIZE_MODIFIER_SIZE_MOD_NONE
/* datatype, addr width, addr reg, data width, data reg, size mod */
#define XNMOV_TEMPLATE(ADR,DT,AW,AR,DW,DR,SM) \
    DT v;                                     \
    _asm { mov AR , ADR  }                    \
    _asm    EMIT_SIZE_MODIFIER_##SM           \
    _asm    _emit 0x0F                        \
    _asm    _emit 0x38                        \
    _asm    _emit 0xF4                        \
    _asm    _emit 0x00                        \
    _asm {    mov v , DR       }              \
    return v;
#if TARGET_IA32
#define ADDR_WIDTH  l
#define RAX_EAX     eax 
#else /* TARGET_INTEL64 */
#define ADDR_WIDTH  q
#define RAX_EAX     rax 
#endif /* TARGET_IA32 */
#define SIZE_MOD_16 16
#define SIZE_MOD_64 64
#define SIZE_MOD_NONE NONE
#else /* TARGET_LINUX */
/* datatype, addr width, addr reg, data width, data reg, size mod */
#define XNMOV_TEMPLATE(ADR,DT,AW,AR,DW,DR,SM) \
    DT v; \
    __asm__ __volatile__ ("mov" STRINGIZE(AW) " %0, %%" STRINGIZE(AR) "\n" : : "r"(ADR) : "memory" ); \
    __asm__ __volatile__ (SM ".byte 0x0F; .byte 0x38; .byte 0xF4; .byte 0x00; \n" : : : "memory" ); \
    __asm__ __volatile__ ("mov" #DW " %%" #DR ", %0\n" : "=r"(v) : : "memory" );  \
    return v;
#if TARGET_IA32
#define ADDR_WIDTH  l
#define RAX_EAX     eax 
#else /* TARGET_INTEL64 */
#define ADDR_WIDTH  q
#define RAX_EAX     rax 
#endif /* TARGET_IA32 */
#define SIZE_MOD_16 ".byte 0x66; "
#define SIZE_MOD_64 ".byte 0x48; "
#define SIZE_MOD_NONE ""
#endif /* TARGET_WINDOWS */

static __inline int16_t read_nx_16( void* addr )
{
    XNMOV_TEMPLATE(addr,int16_t,ADDR_WIDTH,RAX_EAX,w,ax,SIZE_MOD_16);
}

static __inline int32_t read_nx_32( void* addr )
{
    XNMOV_TEMPLATE(addr,int32_t,ADDR_WIDTH,RAX_EAX,l,eax,SIZE_MOD_NONE);
}

#if TARGET_INTEL64
static __inline int64_t read_nx_64( void* addr )
{
    XNMOV_TEMPLATE(addr,int64_t,ADDR_WIDTH,RAX_EAX,q,rax,SIZE_MOD_64);
}
#endif /* TARGET_INTEL64 */

#endif /* __HLE_RTM_H */
