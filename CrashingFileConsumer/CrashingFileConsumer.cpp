#ifdef WIN32

#include <SDKDDKVer.h>
#include <tchar.h>
#include <windows.h>

#else

#define _tmain main
#define __try if(1)
#define __except(a) if(0)

#endif

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>

void Function2(FILE* fd)
{
	char buffer[20];
	int len = 0;

	for(; !feof(fd); len++) buffer[len] = (char)fgetc(fd);
	buffer[len] = 0;

	printf("Length of file is %d.\n", len);

	//if(rand() % 2 == 0)
	//	memset(buffer, 'A', sizeof(buffer)*1000);
}

void Function1(FILE* fd)
{
	if(fd != NULL)
		Function2(fd);
}

int _tmain(int argc, char* argv[])
{
	if(argc < 2)
	{
		printf("Error, please supply a filename to load.\n");
		return 0;
	}

	printf("Loading file \"%s\"...\n", argv[1]);

	FILE* fd = fopen(argv[1], "rb+");
	if(fd == NULL)
	{
		printf("Error, unable to open file \"%s\".\n", argv[1]);
		return 0;
	}

	__try
	{
		Function1(fd);
	}
	__except(GetExceptionCode() == EXCEPTION_ACCESS_VIOLATION)
	{
		fprintf(stderr, "Caught AV exception.\n");
	}

	fclose(fd);

	return 0;
}

