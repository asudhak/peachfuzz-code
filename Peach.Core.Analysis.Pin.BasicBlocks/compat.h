#include <string>

#ifdef WIN32
#define strncasecmp _strnicmp
#else
#include <strings.h>
#endif

#define UNUSED_ARG(x) x;

#include <stdint.h>
#include <errno.h>

void DebugWrite(const char* msg);

uint64_t GetProcessTicks(int pid); 

std::string GetFullFileName(const std::string& fileName);

class WinDirHelper
{
private:
	std::string m_SystemDir;
	std::string m_SystemX86Dir;
	bool m_IgnoreSystemDir;

public:
	WinDirHelper(bool ignoreSystemDir);

	bool IsSystem(const std::string& fileName) const
	{
		if (!m_IgnoreSystemDir)
			return false;

		if (!m_SystemDir.empty() && 0 == strncasecmp(m_SystemDir.c_str(), fileName.c_str(), m_SystemDir.length()))
			return true;

		if (!m_SystemX86Dir.empty() && 0 == strncasecmp(m_SystemX86Dir.c_str(), fileName.c_str(), m_SystemX86Dir.length()))
			return true;

		return false;
	}
};
