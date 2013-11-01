/*BEGIN_LEGAL 
Intel Open Source License 

Copyright (c) 2002-2013 Intel Corporation. All rights reserved.
 
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
// <ORIGINAL-AUTHOR>: Greg Lueck
// <COMPONENT>: pinvm
// <FILE-TYPE>: public header

#ifndef PINSYNC_MAC_HPP
#define PINSYNC_MAC_HPP

#include "sync.hpp"
#include "sync/os-posix.hpp"


namespace PINVM {


// We use spin-locks on Mac mostly because they are easy to implement.  We
// should investigate other options in the future.  We cannot user "barecrt"
// on Mac because Yield() and GetTime() are not defined there.  Instead, we
// use POSIX calls from libc.  We should probably fix this since using libc
// might cause isolation issues between Pin and application or Pin and tool
// (e.g. due to TLS usage).


/*!
 * Basic non-recursive lock.
 */
typedef SYNC::SIMPLE_LOCK_SPIN<SYNC::OS_POSIX> PINSYNC_LOCK;

/*!
 * Basic non-recursive lock with POD semantics.
 */
typedef SYNC::SIMPLE_LOCK_SAFEPOD_SPIN<SYNC::OS_POSIX> PINSYNC_POD_LOCK;

/*!
 * Basic non-recursive lock with SAFEPOD semantics.
 */
typedef SYNC::SIMPLE_LOCK_SAFEPOD_SPIN<SYNC::OS_POSIX> PINSYNC_SAFEPOD_LOCK;

/*!
 * Read-writer lock.
 */
typedef SYNC::READER_WRITER_LOCK_SPIN<SYNC::OS_POSIX> PINSYNC_RWLOCK;

/*!
 * Read-writer lock with POD semantics.
 */
typedef SYNC::READER_WRITER_LOCK_SAFEPOD_SPIN<SYNC::OS_POSIX> PINSYNC_POD_RWLOCK;

/*!
 * Binary semaphore.
 */
typedef SYNC::SEMAPHORE_SPIN<SYNC::OS_POSIX> PINSYNC_SEMAPHORE;

/*!
 * Binary semaphore with POD semantics.
 */
typedef SYNC::SEMAPHORE_SAFEPOD_SPIN<SYNC::OS_POSIX> PINSYNC_POD_SEMAPHORE;

} // namespace
#endif // file guard
