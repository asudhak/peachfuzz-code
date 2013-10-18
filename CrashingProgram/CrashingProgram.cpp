#ifdef WIN32

#include <SDKDDKVer.h>
#include <tchar.h>
#include <Windows.h>

#else

#define _tmain main
#define _TCHAR char
#define __try if(1)
#define __except(a) if(0)

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

#ifdef WIN32
	if (argc > 2)
	{
		STARTUPINFO StartInfo;
		PROCESS_INFORMATION ProcInfo;

		ZeroMemory(&StartInfo, sizeof(StartInfo));
		ZeroMemory(&ProcInfo, sizeof(ProcInfo));

		fprintf(stderr, "Creating child...\n");

		if (!CreateProcess(
			argv[0],
			TEXT(""),
			NULL,
			NULL,
			FALSE,
			0,
			NULL,
			NULL,
			&StartInfo,
			&ProcInfo))
		{
			fprintf(stderr, "Child failed!\n");
		}
		else
		{
			WaitForSingleObject(ProcInfo.hProcess, INFINITE);
		}
	}
	else
#endif
	if (argc > 1)
	{
		fprintf(stderr, "Guarding for AV exception.\n");

		__try
		{
			Bar();
		}
		__except(GetExceptionCode() == EXCEPTION_ACCESS_VIOLATION)
		{
			fprintf(stderr, "Caught AV exception.\n");
		}
	}
	else
	{
		Bar();
	}

	fprintf(stderr, "done...\n");

	return 0;
}

// end
