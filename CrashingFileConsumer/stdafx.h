// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#ifdef WIN32
#include "targetver.h"

#include <stdio.h>
#include <tchar.h>
#include <windows.h>
#else
#define _tmain main
#define __try if(1)
#define __except(a) if(0)
#endif


// TODO: reference additional headers your program requires here
