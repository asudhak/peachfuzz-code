
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

#define UNUSED_ARG(x) x;

#if defined(_MSC_VER)
#pragma warning(disable: 4127) // Conditional expression is constant

#pragma warning(push)
#pragma warning(disable: 4100) // Unreferenced formal parameter
#pragma warning(disable: 4245) // Signed/unsigned mismatch
#pragma warning(disable: 4512) // Assignment operator could not be generated
#endif

#include <pin.H>

#if defined(_MSC_VER)
#pragma warning(pop)
#endif

#include <iostream>
#include <fstream>
#include <sstream>
#include <string>

#include "uthash.h"

#if 0

extern "C"
{
	void __stdcall OutputDebugStringA(const char* lpOutputString);
}

#else

void OutputDebugStringA(const char* str)
{
	UNUSED_ARG(str);
}

#endif

#define DBG(x) do { if (KnobDebug) fileDbg.Dbg x; } while (0)

template <bool b>
struct StaticAssert {};

template <>
struct StaticAssert<true>
{
	static void assert() {}
};


static std::string MakeFileName(const std::string& fullName)
{
#ifdef TARGET_WINDOWS
	char sep = '\\';
#else
	char sep = '/';
#endif
	size_t idx = fullName.rfind(sep);
	return fullName.substr(idx + 1);
}

class NonCopyable
{
protected:
	NonCopyable() {}
	~NonCopyable() {}

private:
	NonCopyable(const NonCopyable&);
	NonCopyable operator=(const NonCopyable&);
};

class ImageRec;

class ImageName : NonCopyable
{
public:
	typedef const std::string& key_type;

	ImageName(const ImageRec* pImg);

	const ImageRec*    record;
	size_t             keylen;
	const char*        key;
	UT_hash_handle     hh;
};

class ImageRec : NonCopyable
{
private:

public:
	typedef size_t key_type;

	ImageRec(IMG img)
		: fullName(IMG_Name(img))
		, fileName(MakeFileName(fullName))
		, lowAddress(IMG_LowAddress(img))
		, highAddress(IMG_HighAddress(img))
		, excluded(false)
		, conflict(NULL)
		, key(IMG_Id(img))
	{
	}

	const std::string fullName;    // Full absolute path to file
	const std::string fileName;    // Just the name of the file
	const ADDRINT     lowAddress;
	const ADDRINT     highAddress;
	bool              excluded;
	const ImageName*  conflict;

	size_t            key;
	UT_hash_handle    hh;
};

ImageName::ImageName(const ImageRec* pImg)
	: record(pImg)
	, keylen(pImg->fileName.size())
	, key(pImg->fileName.c_str())
{
}

class BlockRec : NonCopyable
{
private:
	static std::string MakeTrace(const ImageRec& img, ADDRINT addr)
	{
		if (img.excluded)
			return "";

		std::stringstream ss;
		ss << img.fileName << ": " << (addr - img.lowAddress) << std::endl;
		return ss.str();
	}

public:
	typedef size_t key_type;

	BlockRec(const ImageRec& img, ADDRINT addr)
		: trace(MakeTrace(img, addr))
		, fileName(img.fileName)
		, countRun(0)
		, countAdd(1)
		, excluded(img.excluded)
		, existing(false)
		, key(addr)
	{
	}

	const BlockRec* Next() const
	{
		return (const BlockRec*)hh.next;
	}

	const std::string  trace;    // Name of trace
	const std::string& fileName; // Name of image address is part of
	size_t             countRun; // Count of BlockExecuted()
	size_t             countAdd; // Count of Trace()
	bool               excluded;
	bool               existing;
	size_t             key;
	UT_hash_handle     hh;
};

class StringRec : NonCopyable
{
public:
	typedef const std::string key_type;

	StringRec(const std::string& val)
		: value(val)
		, keylen(value.size())
		, key(value.c_str())
	{
	}

	const std::string value;
	size_t            keylen;
	const char*       key;
	UT_hash_handle    hh;
};

template<typename TVal>
class HashTable : NonCopyable
{
	typedef typename TVal::key_type TKey;

public:
	HashTable()
		: table(NULL)
	{
	}

	~HashTable()
	{
		TVal *cur, *tmp;

		HASH_ITER(hh, table, cur, tmp)
		{
			HASH_DEL(table, cur);
			delete tmp;
		}
	}

	TVal* Find(const TKey& key)
	{
		return FindImpl(key);
	}

	void Add(TVal* value)
	{
		AddImpl(value->key, value);
	}

	size_t Count() const
	{
		return HASH_COUNT(table);
	}

	const TVal* Head() const
	{
		return table;
	}

private:
	TVal* FindImpl(size_t key)
	{
		TVal* value;
		HASH_FIND_INT(table, &key, value);
		return value;
	}

	TVal* FindImpl(const std::string& key)
	{
		TVal* value;
		const char* str = key.c_str();
		size_t strlen = key.size();
		HASH_FIND(hh, table, str, (unsigned)strlen, value);
		return value;
	}

	void AddImpl(size_t, TVal* value)
	{
		HASH_ADD_INT(table, key, value);
	}

	void AddImpl(const std::string&, TVal* value)
	{
		HASH_ADD_KEYPTR(hh, table, value->key, (unsigned)value->keylen, value);
	}

	TVal*  table;
};

struct File : NonCopyable
{
public:
	File()
		: m_pFile(NULL)
	{
	}

	~File()
	{
		Close();
	}

	void Open(const std::string& name, const std::string& mode)
	{
		Close();

		m_pFile = fopen(name.c_str(), mode.c_str());
	}

	void Close()
	{
		if (m_pFile)
		{
			fclose(m_pFile);
			m_pFile = NULL;
		}
	}

	void Write(const std::string& value)
	{
		if (m_pFile)
		{
			fwrite(value.c_str(), 1, value.size(), m_pFile);
		}
	}

	void Write(const char* fmt, ...)
	{
		va_list args;
		va_start(args, fmt);

		if (m_pFile)
		{
			vfprintf(m_pFile, fmt, args);
		}

		va_end(args);
	}

	void Dbg(const char* fmt, ...)
	{
		va_list args;
		va_start(args, fmt);

		if (m_pFile)
		{
			char buf[2048];
			int len = vsnprintf(buf, sizeof(buf) - 2, fmt, args);
			if (len == -1)
				len = sizeof(buf) - 2;
			buf[len++] = '\n';
			buf[len] = '\0';

			fwrite(buf, 1, len, m_pFile);

			OutputDebugStringA(buf);
		}


		va_end(args);
	}

private:
	FILE* m_pFile;
};

typedef HashTable<StringRec> Strings_t;
typedef HashTable<BlockRec> Blocks_t;
typedef HashTable<ImageRec> Images_t;
typedef HashTable<ImageName> ImageNames_t;

static Strings_t excludedImages;
static Strings_t existingTraces;
static ImageNames_t includedImages;
static Blocks_t blocks;
static Images_t images;

File fileOut;
File fileExisting;
File fileDbg;

KNOB<BOOL> KnobDebug(KNOB_MODE_WRITEONCE, "pintool", "debug", "0", "Enable debug logging.");

bool ReadAllLines(const std::string& fileName, Strings_t& lines)
{
	std::ifstream fin(fileName.c_str(), std::ifstream::binary);
	if (!fin)
		return false;

	std::string line;
	while (std::getline(fin, line))
	{
		size_t end = line.size() - 1;
		if (end > 0 && line[end] == '\r')
			line.resize(end);

		if (!lines.Find(line))
			lines.Add(new StringRec(line));
	}

	return !fin.bad();
}

// Prints the usage and exits
INT32 Usage()
{
	PIN_ERROR( "This Pintool prints a trace of all basic blocks\n"
		+ KNOB_BASE::StringKnobSummary() + "\n");
	return -1;
}

// Called whenever a basic block is executed
VOID PIN_FAST_ANALYSIS_CALL BlockExecuted(VOID* v)
{
	BlockRec* pBlock = reinterpret_cast<BlockRec*>(v);

	if (pBlock->countRun++ == 0)
	{
		fileOut.Write(pBlock->trace);
		fileExisting.Write(pBlock->trace);
	}
}

// Called every time a new image is loaded
VOID Image(IMG img, VOID* v)
{
	UNUSED_ARG(v);

	ImageRec* pImg = new ImageRec(img);
	pImg->excluded = !!excludedImages.Find(pImg->fileName);
	pImg->conflict = includedImages.Find(pImg->fileName);

	images.Add(pImg);

	if (pImg->excluded)
	{
		DBG(("Excluding image: %s", pImg->fullName.c_str()));
	}
	else if (pImg->conflict)
	{
		const ImageRec* pOther = pImg->conflict->record;

		if (pImg->fullName == pOther->fullName)
		{
			// Add for tracking - is the same fullName
			DBG(("Duplicate image names detected: %s", pImg->fileName.c_str()));
			includedImages.Add(new ImageName(pImg));
		}
		else
		{
			// Ignore, since we have two different fullNames
			DBG(("Conflicting image names detected: %s", pImg->fileName.c_str()));
		}

		DBG(("  Id: %lu, Name: %s", (unsigned long)pOther->key, pOther->fullName.c_str()));
		DBG(("  Id: %lu, Name: %s", (unsigned long)pImg->key, pOther->fullName.c_str()));
	}
	else
	{
		includedImages.Add(new ImageName(pImg));
		DBG(("Loaded image: %s", pImg->fullName.c_str()));
	}
}

// Called every time a new trace is encountered
VOID Trace(TRACE trace, VOID *v)
{
	UNUSED_ARG(v);

	for (BBL bbl = TRACE_BblHead(trace); BBL_Valid(bbl); bbl = BBL_Next(bbl))
	{
		// Grab the first instruction of the block
		INS ins = BBL_InsHead(bbl);
		if (!ins.is_valid())
		{
			DBG(("Could not get 1st instruction for basic block: %zu", (size_t)BBL_Address(bbl)));
			continue;
		}

		ADDRINT addr = INS_Address(ins);

		// If we have visited this basic block before, ignore
		BlockRec* pBlock = blocks.Find(addr);
		if (pBlock != NULL)
		{
			if (!pBlock->existing && !pBlock->excluded)
			{
				DBG(("Ignoring duplicate trace for basic block '%s: %llu'",
					pBlock->fileName.c_str(), (unsigned long long)pBlock->key));
			}

			pBlock->countAdd++;
			continue;
		}

		// If image could not be resolved, ignore
		IMG img = IMG_FindByAddress(addr);
		if (!img.is_valid())
		{
			DBG(("Could not get image for basic block: %llu", (unsigned long long)addr));
			continue;
		}

		// Build a record for tracking this basic block
		ImageRec* pImg = images.Find(IMG_Id(img));
		pBlock = new BlockRec(*pImg, addr);

		// Check if trace for block already exists
		pBlock->existing = !!existingTraces.Find(pBlock->trace);

		// Ensure we are tracking this basic block record
		blocks.Add(pBlock);

		// If trace is already captured or is excluded, ignore
		if (pBlock->existing || pBlock->excluded)
			continue;

		// Record basic block when it is executed
		BBL_InsertCall(
			bbl,
			IPOINT_ANYWHERE,
			AFUNPTR(BlockExecuted),
			IARG_FAST_ANALYSIS_CALL,
			IARG_PTR,
			pBlock,
			IARG_END);
	}
}

// Called when the application starts
VOID Start(VOID* v)
{
	UNUSED_ARG(v);

	std::ofstream fout("bblocks.pid", std::ofstream::binary | std::ofstream::trunc);
	fout << PIN_GetPid();
}

// Called when the application exits
VOID Fini(INT32 code, VOID *v)
{
	UNUSED_ARG(code);
	UNUSED_ARG(v);

	unsigned long existing = 0, excluded = 0, dupes = 0, run = 0;

	for (const BlockRec* it = blocks.Head(); it != NULL; it = it->Next())
	{
		if (it->existing)
			++existing;

		if (it->excluded)
			++excluded;

		if (it->countAdd > 1)
			++dupes;

		if (it->countRun > 0)
			++run;
	}

	DBG(("Finished:"));
	DBG((" All Images     : %lu", (unsigned long)images.Count()));
	DBG((" Excluded Images: %lu",(unsigned long) excludedImages.Count()));
	DBG((" Included Images: %lu", (unsigned long)includedImages.Count()));
	DBG((" Existing Traces: %lu", (unsigned long)existingTraces.Count()));
	DBG((" Basic Blocks   : %lu", (unsigned long)blocks.Count()));
	DBG(("  Existing      : %lu", existing));
	DBG(("  Excluded      : %lu", excluded));
	DBG(("  Duplicates    : %lu", dupes));
	DBG(("  Executed      : %lu", run));
}

int main(int argc, char* argv[])
{
	// Expect size_t and ADDRINT to be the same
	StaticAssert<sizeof(size_t) == sizeof(ADDRINT)>::assert();

	// Ensure library initializes correctly
	if (PIN_Init(argc, argv))
		return Usage();

	{
		Strings_t foo;
		foo.Add(new StringRec("Hello"));

		std::stringstream ss;
		ss << "He" << "llo";
		std::string tgt = ss.str();

		StringRec* pRec = foo.Find(tgt);
		if (pRec == NULL)
			return 1;
	}

	// Read images to ignore
	ReadAllLines("bblocks.exclude", excludedImages);

	// If we can read existing traces, open file for updates with new traces
	if (ReadAllLines("bblocks.existing", existingTraces))
		fileExisting.Open("bblocks.existing", "ab");

	// Open file to log new traces to
	fileOut.Open("bblocks.out", "wb");

	if (KnobDebug)
		fileDbg.Open("bblocks.log", "wb");

	// Register callbacks
	IMG_AddInstrumentFunction(Image, NULL);
	TRACE_AddInstrumentFunction(Trace, NULL);
	PIN_AddApplicationStartFunction(Start, NULL);
	PIN_AddFiniFunction(Fini, NULL);

	// Start program, never returns
	PIN_StartProgram();

	return 0;
}

