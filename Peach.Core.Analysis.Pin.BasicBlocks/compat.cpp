#include "compat.h"

#ifdef WIN32

#define WIN32_LEAN_AND_MEAN

#include <Shlobj.h>

#pragma comment(lib, "shell32.lib")

WinDirHelper::WinDirHelper(bool ignoreSystemDir)
	: m_IgnoreSystemDir(ignoreSystemDir)
{
	HRESULT hr;
	char szPath[MAX_PATH];

	hr = ::SHGetFolderPath(NULL, CSIDL_SYSTEM, NULL, 0, szPath);
	if (SUCCEEDED(hr))
		m_SystemDir = szPath;

	hr = ::SHGetFolderPath(NULL, CSIDL_SYSTEMX86, NULL, 0, szPath);
	if (SUCCEEDED(hr))
		m_SystemX86Dir = szPath;

	if (m_SystemDir == m_SystemX86Dir)
		m_SystemX86Dir.clear();
}

void DebugWrite(const char* msg)
{
	if (IsDebuggerPresent())
		OutputDebugString(msg);
}

#else

void DebugWrite(const char* msg)
{
	msg;
}

#endif

