
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
// PIN Tool to find the edge bblocks arround bblocks we hit.
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

// Trace file for existing and new blocks
static FILE* existing = NULL;

// New basic blocks
static set<ADDRINT> setKnownBlocks;

// Branches not taken
static set<ADDRINT> setUnknownBlocks;

static pair<set<ADDRINT>::iterator,bool> ret;

// Do we have an existing bblocks trace?
int haveExisting = FALSE;

#if defined(TARGET_IA32)
# define FMT "%u\n"
#elif defined(TARGET_IA32E)
# if defined(TARGET_LINUX)
#  define FMT "%lu\n"
# else
#  define FMT "%llu\n"
# endif
#else
# error TARGET_IA32 or TARGET_IA32E must be defined
#endif

VOID handleInsertCall( ADDRINT src, ADDRINT dst, INT32 taken )
{
	src;
	taken;

	if(!taken)
	{
		// Lookup and see if we have already seen this one
		if(setKnownBlocks.find(dst) == setKnownBlocks.end())
			ret = setUnknownBlocks.insert(dst);
	}
	else
	{
		// Store bblocks we have already seen
		ret = setKnownBlocks.insert(dst);

		// If we had not taken this edge some other time, lets remove
		// it from our list.
		set<ADDRINT>::iterator pos = setUnknownBlocks.find(dst);
		if(pos != setUnknownBlocks.end())
			setUnknownBlocks.erase(pos);
	}
} 

VOID Instruction(INS ins, void *v)
{
	v;

	if (INS_IsBranchOrCall(ins)) 
	{
		INS_InsertCall(ins, IPOINT_BEFORE, (AFUNPTR) handleInsertCall,
			IARG_INST_PTR,
			IARG_BRANCH_TARGET_ADDR,
			IARG_BRANCH_TAKEN,
			IARG_END);
	}
}

// Called at end of run
VOID Fini(INT32 code, VOID *v)
{
	code;
	v;

	existing = fopen("cedge.known", "wb+");
	if(existing != NULL)
	{
		for (set<ADDRINT>::iterator i = setKnownBlocks.begin(); i!=setKnownBlocks.end(); ++i)
		{
			fprintf(existing, FMT, *i);
		}

		fclose(existing);
		existing = NULL;
	}

	existing = fopen("cedge.unknown", "wb+");
	if(existing != NULL)
	{
		for (set<ADDRINT>::iterator i = setUnknownBlocks.begin(); i != setUnknownBlocks.end(); ++i)
		{
			fprintf(existing, FMT, *i);
		}

		fclose(existing);
		existing = NULL;
	}
}

int main(int argc, char * argv[])
{
	// Load existing trace
	ADDRINT block = 0;

	// Load existing known edges
	existing = fopen("cedge.known", "rb+");
	if(existing != NULL)
	{
		while(!feof(existing))
		{
			if(fscanf(existing, FMT, &block) < 4)
				setKnownBlocks.insert(block);
		}
		
		fclose(existing);
		existing = NULL;
	}

	// Load existing unknown edges
	existing = fopen("cedge.unknown", "rb+");
	if(existing != NULL)
	{
		while(!feof(existing))
		{
			if(fscanf(existing, FMT, &block) < 4)
				setUnknownBlocks.insert(block);
		}

		fclose(existing);
		existing = NULL;
	}

	//trace = fopen("bblocks.out", "wb+");
	
	// Configure Pin Tools
	PIN_Init(argc, argv);
	//TRACE_AddInstrumentFunction(Trace, 0);
	INS_AddInstrumentFunction(Instruction, 0);
	PIN_AddFiniFunction(Fini, 0);
	PIN_StartProgram();

	return 0;
}

// end
