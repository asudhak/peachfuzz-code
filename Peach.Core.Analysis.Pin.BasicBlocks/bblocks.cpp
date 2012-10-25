
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

//
// PIN Tool to find all basic blocks a program hits.
//  This PIN Tool is intended for use with Peach.
//
//  Code based on examples from PIN documentation.
//

#include <iostream>
#include <fstream>
#include <stdio.h>
#include <set>

#if defined(_MSC_VER)
#pragma warning(push)
#pragma warning(disable: 4100) // Unreferenced formal parameter
#pragma warning(disable: 4127) // Conditional expression is constant
#pragma warning(disable: 4245) // Signed/unsigned mismatch
#pragma warning(disable: 4512) // Assignment operator could not be generated
#endif

#include <pin.H>

#if defined(_MSC_VER)
#pragma warning(pop)
#endif

using namespace std;

// Trace file for new blocks
static FILE* trace = NULL;

// Trace file for existing and new blocks
static FILE* existing = NULL;

// New basic blocks
static set<ADDRINT> setKnownBlocks;

// Existing known bblocks
static set<ADDRINT> setExistingBlocks;

static pair<set<ADDRINT>::iterator,bool> ret;

// Do we have an existing bblocks trace?
int haveExisting = FALSE;

#ifdef TARGET_WINDOWS
#define FMT "%u\n"
#else
#define FMT "%zu\n"
#endif

// Method called every time an instrumented bblock is executed
VOID PIN_FAST_ANALYSIS_CALL rememberBlock(ADDRINT bbl)
{
	ret = setKnownBlocks.insert(bbl);
	if(ret.second == true)
	{
		fprintf(trace, FMT, (size_t)bbl);
		fflush(trace);

		if(haveExisting)
		{
			fprintf(existing, FMT, (size_t)bbl);
			fflush(existing);
		}
	}
}

// Called when new code segment loaded containing bblocks
VOID Trace(TRACE trace, VOID *v)
{
	v;

	for (BBL bbl = TRACE_BblHead(trace); BBL_Valid(bbl); bbl = BBL_Next(bbl))
	{
		if(!haveExisting || setExistingBlocks.find(BBL_Address(bbl)) == setExistingBlocks.end())
			BBL_InsertCall(bbl, IPOINT_ANYWHERE, AFUNPTR(rememberBlock), IARG_FAST_ANALYSIS_CALL, IARG_ADDRINT, BBL_Address(bbl), IARG_END);
	}
}

// Called at end of run
VOID Fini(INT32 code, VOID *v)
{
	code;
	v;

	fclose(trace);

	if(haveExisting)
		fclose(existing);
}

int main(int argc, char * argv[])
{
	// Load existing trace
	ADDRINT block = 0;
	existing = fopen("bblocks.existing", "rb+");
	if(existing != NULL)
	{
		haveExisting = TRUE;
		while(!feof(existing))
		{
			if(fscanf(existing, FMT, &block) < 4)
				setExistingBlocks.insert(block);
		}

		// Make sure we are at end of file
		fseek(existing, 0L, SEEK_END);
	}

	trace = fopen("bblocks.out", "wb+");
	
	// Configure Pin Tools
	PIN_Init(argc, argv);
	TRACE_AddInstrumentFunction(Trace, 0);
	PIN_AddFiniFunction(Fini, 0);
	PIN_StartProgram();

	return 0;
}

// end
