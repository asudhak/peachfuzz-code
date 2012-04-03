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
// <ORIGINAL-AUTHOR>: Vladimir Vladimirov
// <COMPONENT>: vsdbg
// <FILE-TYPE>: component public header

#ifndef VSDBG_API_HPP
#define VSDBG_API_HPP

/*! @mainpage VSDBG pintool.
 *
 * The VSDBG pintool provides integration of PinADX with Visual Studio debugger.
 */

/*! @brief VSDBG pintool. */
namespace VSDBG {

/*!
 * @note
 * Static linking with VSDBG.dll is not supported.
 * Client should retrieve relevant API function pointer with WINDOWS::GetProcAddress()
 * and invoke the function using corresponding signature provided below.
 */

/*!
 * Signature of VSDBG.dll export function "SetPinArgs"
 * Sets current list of Pin command line options for debuggee launch mode.
 * The list is actual for any subsequent debuggee launch session until next SetPinArgs.
 *
 *  @param[in]  argc, argv  Pin command line options that VSDBG uses in debuggee launch mode.
 *                          Specified in format of wmain function supporting wide characters.
 *
 *  @return    false if VSDBG is not yet initialized or there is active debugging session.
 */
typedef bool (* SetPinArgs_t)(int argc, const wchar_t * argv[]);

/*!
 * Signature of VSDBG.dll export function "NotifyDebuggerConnection"
 * Instructs VSDBG to connect to debuggee using provided port number
 * when VSDBG encounters call to DebugActiveProcess with specified PID.
 *
 *  @param[in] pid      Process ID of launched debuggee
 *  @param[in] port     Port number to establish connection between the debuggee and debugger
 *  @param[in] stopped  true instructs VSDBG to suspend execution of debuggee application
 *                        in debugger once connection is established
 *                      false means resume execution of the debuggee application
 *  @param[in] hEvent   handle to manual-reset nonsignaled event object
 *                      which is signaled once the connection is successfully established
 *
 *  @return    false if VSDBG is not yet initialized or there is active debugging session.
 */
typedef bool (* NotifyDebuggerConnection_t)(unsigned int pid, int port, bool stopped, void *hEvent);

// Custom breakpoints API

/*!
 * Proceed status of custom breakpoint.
 */
typedef enum {
    CUSTOM_BP_STATUS_SKIP,
        // Skip this breakpoint, perform regular "skip debug event" action
    CUSTOM_BP_STATUS_PEND,
        // Hold VSDBG in command mode until Visual Studio debugger orders to continue broken thread
    CUSTOM_BP_STATUS_EMIT
        // Emit regular breakpoint event with context of this custom breakpoint
} CUSTOM_BP_STATUS;

/*!
 * This callback is invoked in dedicated thread of VSDBG in Visual Studio process that hosts client's plugin.
 * Provides current custom breakpoint information to VSDBG client that registered the callback.
 * Returns decision regarding further breakpoint status.
 *
 * @param[in]  tid  O/S ID of thread that triggered the custom breakpoint.
 * @param[in]  msg  Full text of custom breakpoint message.
 * @param[inopt] v  The client's callback value provided upon registration.
 *
 * @return  Custom breakpoint status.
 *          If CUSTOM_BP_STATUS_PEND is returned, it is expected the callback function initiates an activity
 *          to switch Visual Studio debugger to its command mode (for instance, schedules "Break All" command).
 *          VSDBG remains in command mode until Visual Studio debugger explicitly resumes execution of thread
 *          that triggered the breakpoint.
 */
typedef CUSTOM_BP_STATUS (* CUSTOM_BREAKPOINT_CALLBACK)(unsigned int tid, const wchar_t *msg, void *v);

/*!
 * Signature of VSDBG.dll export function "AddCustomBreakpointFunction"
 * This function registers custom breakpoint callback function. Subsequent call replaces previous registration.
 *
 * @param[in]  fun  Pointer to callback function that is invoked when custom breakpoint is delivered.
 *                  Special zero pointer value is specified to unregister previous callback function.
 * @param[inopt] v  The client's callback value. It is passed back to client when fun is invoked.
 * @param[in]  timeLimit The time limit (in milliseconds) in which the custom breakpoint callback function
 *                  is supposed to return. Value INFINITE disables time limit.
 * @param[in]  timeLimitStatus The breakpoint status that is reported when the time limit has been expired.
 *                  Allowed values are CUSTOM_BP_STATUS_SKIP and CUSTOM_BP_STATUS_EMIT.
 *
 * @return     false if not invoked in primary debugger process or invalid or inconsistent arguments.
 */
typedef bool (* AddCustomBreakpointFunction_t)(CUSTOM_BREAKPOINT_CALLBACK fun, void *v,
                                               unsigned int timeLimit, CUSTOM_BP_STATUS timeLimitStatus);

}

#endif // file guard
