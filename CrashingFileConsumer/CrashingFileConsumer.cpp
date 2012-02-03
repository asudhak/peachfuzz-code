// CrashingFileConsumer.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <windows.h>

void Function2(FILE* fd)
{
	char buffer[20];

	int len = fread(buffer, 1, 1024, fd);
	buffer[len] = 0;

	printf("Length of file is %d.\n", strlen(buffer));
}

void Function1(FILE* fd)
{
	if(fd != NULL)
		Function2(fd);
}

int _tmain(int argc, char* argv[])
{
	if(argc < 1)
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

	Function1(fd);

	fclose(fd);

	return 0;
}

