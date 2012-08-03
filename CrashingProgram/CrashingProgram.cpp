// CrashingProgram.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#ifdef WIN32
#include <Windows.h>
#else
#define _tmain main
#define _TCHAR char
#endif
#include <stdlib.h>
#include <stdio.h>
#include <string.h>

int b = 0;

void Foo()
{
	char buff[10];
	fprintf(stderr, "Len of 'PEACH' %u\n", (unsigned)strlen(getenv("PEACH")));
	strcpy(buff, getenv("PEACH"));
	strcpy(buff, getenv("PEACH"));
	strcpy(buff, getenv("PEACH"));

	if(b == 0 && strlen(getenv("PEACH"))>10)
	{
		b = 1;
	}
}

void Bar()
{
	Foo();

	if(b == 1)
	{
		b = -1;
		Foo();
	}
}

int _tmain(int argc, _TCHAR* argv[])
{
	argc;
	argv;

	fprintf(stderr, "Crashing Program v0.1\n");
	Bar();

	fprintf(stderr, "done...\n");

	return 0;
}

// end
