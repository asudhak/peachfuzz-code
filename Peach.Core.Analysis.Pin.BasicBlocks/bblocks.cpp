
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
#include <sstream>      // std::ostringstream
#include <stdio.h>
#include <stdarg.h>
#include <set>
#include <map>
#include <memory>

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
static set<string*> setExistingBlocks;

// images to exclude from coverage
static set<string*> excludedImages;

static pair<set<ADDRINT>::iterator,bool> ret;

// map of image name to low/high address
static map<string*, pair<ADDRINT, ADDRINT> > imageList;

// Do we have an existing bblocks trace?
char haveExisting = FALSE;

// Do we have an image exclude list?
char haveExclude = FALSE;

class Logger
{
public:
	Logger()
	{
		m_log = fopen("bblocks.log", "a");
	}

	~Logger()
	{
		if (m_log)
		{
			fclose(m_log);
			m_log = NULL;
		}
	}

	void Write(const char* fmt, ...)
	{
		va_list args;
		va_start(args, fmt);

		if (m_log)
		{
			vfprintf(m_log, fmt, args);
			fflush(m_log);
		}

		va_end(args);
	}

private:
	FILE *m_log;
};

//#define DBGLOG(x) Logger().Write x
#define DBGLOG(x)

// convert address into module + offset
pair<string*, string*> ResolveAddress(ADDRINT address)
{
	map<string*, pair<ADDRINT, ADDRINT> >::const_iterator it;

	for(it = imageList.begin(); it != imageList.end(); it++)
	{
		if(address >= (*it).second.first && address <= (*it).second.second)
		{
			ostringstream resolved;
			resolved << *(*it).first << ":" << (address - (*it).second.first) << "\n";

			return make_pair(new string(*(*it).first), new string(resolved.str()));
		}
	}

	return make_pair((string*)NULL, (string*)NULL);
}

// Method called every time an instrumented bblock is executed
VOID PIN_FAST_ANALYSIS_CALL rememberBlock(INS ins)
{
	ADDRINT address = INS_Address(ins);
	ret = setKnownBlocks.insert(address);
	if(ret.second == true)
	{
		pair<string*,string*> resolvedAddress = ResolveAddress(address);
		
		// skip addresses that don't resolve
		// an address will not resolve when we have
		// blacklisted the module
		if(resolvedAddress.first == NULL)
			return;

		fprintf(trace, "%s\n", resolvedAddress.second->c_str());
		fflush(trace);

		if(haveExisting)
		{
			fprintf(trace, "%s\n", resolvedAddress.second->c_str());
			fflush(existing);
		}
	}
}

// Strip path from image name
string* ImageName(string* fullname)
{
	size_t found;

#ifdef TARGET_WINDOWS
	found = fullname->rfind('\\');
#else
	found = fullname->rfind('/');
#endif
	
	return new string(fullname->substr(found+1));
}

// Called when an image is loaded
VOID Image(IMG img, VOID* v)
{
	v;

	string fullname = IMG_Name(img);
	string* name = ImageName(&fullname);

	// don't record images in our exclude list
	if(excludedImages.find(name) != excludedImages.end())
		return;

	imageList[name] = make_pair(IMG_LowAddress(img), IMG_HighAddress(img));
}

// Called when new code segment loaded containing bblocks
VOID Trace(TRACE trace, VOID *v)
{
	v;

	for (BBL bbl = TRACE_BblHead(trace); BBL_Valid(bbl); bbl = BBL_Next(bbl))
	{
		pair<string*, string*> resolvedPair = ResolveAddress(INS_Address(BBL_InsHead(bbl)));
		auto_ptr<string> imageName = auto_ptr<string>(resolvedPair.first);
		auto_ptr<string> resolvedAddress = auto_ptr<string>(resolvedPair.second);

		if(haveExclude && excludedImages.find(imageName.get()) != excludedImages.end())
			return;

		if(haveExisting && setExistingBlocks.find(resolvedAddress.get()) != setExistingBlocks.end())
			continue;

		BBL_InsertCall(bbl, IPOINT_ANYWHERE, AFUNPTR(rememberBlock), IARG_FAST_ANALYSIS_CALL, IARG_ADDRINT, BBL_InsHead(bbl), IARG_END);
	}
}

// Called at end of run
VOID Fini(INT32 code, VOID *v)
{
	code;
	v;

	map<string*, pair<ADDRINT, ADDRINT> >::const_iterator mapIt;
	set<string*>::const_iterator setIt;

	fclose(trace);

	if(haveExisting)
		fclose(existing);

	// free memory
	for(mapIt = imageList.begin(); mapIt != imageList.end(); mapIt++)
		delete (*mapIt).first;
	for(setIt = setExistingBlocks.begin(); setIt != setExistingBlocks.end(); setIt++)
		delete (*setIt);
}

// Called when the application starts
VOID Start(VOID* v)
{
	v;

	FILE* pid = fopen("bblocks.pid", "wb+");
	if (pid)
	{
		fprintf(pid, "%d", PIN_GetPid());
		fclose(pid);
		pid = NULL;
	}
}

int main(int argc, char * argv[])
{
	DBGLOG(("bblocks main() PID: %d\n", PIN_GetPid()));

	// Load image exclude list
	FILE* fd = fopen("bblocks.exclude", "rb+");
	if(fd != NULL)
	{
		char image[256];

		haveExclude = TRUE;
		while(!feof(fd))
		{
			if(fscanf(fd, "%s\n", image) < 4)
				excludedImages.insert(new string(image));
		}

		fclose(fd);
	}

	// Load existing trace
	existing = fopen("bblocks.existing", "rb+");
	if(existing != NULL)
	{
		char offset[256];

		haveExisting = TRUE;
		while(!feof(existing))
		{
			if(fscanf(existing, "%s\n", offset) < 4)
				setExistingBlocks.insert(new string(offset));
		}

		// Make sure we are at end of file
		fseek(existing, 0L, SEEK_END);
	}

	trace = fopen("bblocks.out", "wb+");
	
	// Configure Pin Tools
	PIN_Init(argc, argv);

	IMG_AddInstrumentFunction(Image, 0);
	TRACE_AddInstrumentFunction(Trace, 0);
	
	PIN_AddApplicationStartFunction(Start, 0);
	PIN_AddFiniFunction(Fini, 0);
	PIN_StartProgram();

	return 0;
}

// end
