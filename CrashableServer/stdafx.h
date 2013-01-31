#pragma once

#ifdef WIN32

#ifndef _WIN32_WINNT		// Allow use of features specific to Windows XP or later.                   
#define _WIN32_WINNT 0x0501	// Change this to the appropriate value to target other versions of Windows.
#endif						

#include <stdio.h>
#include <winsock2.h>
#include <tchar.h>
#include <windows.h>

typedef int socklen_t;

#else

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <unistd.h>

typedef int SOCKET;

struct WSAData {};

#define SOCKET_ERROR -1
#define INVALID_SOCKET -1
#define _tmain main

#define MAKEDWORD(a,b) (0)
#define WSAStartup(a,b) (0)

inline int closesocket(int s) { return close(s); }
inline void WSACleanup() {}
inline int WSAGetLastError() { return errno; }

#endif
