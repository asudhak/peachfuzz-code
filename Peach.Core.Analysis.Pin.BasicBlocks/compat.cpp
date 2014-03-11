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

uint64_t GetProcessTicks(int pid)
{
	HANDLE hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, pid);
	if (NULL == hProcess)
		return 0;

	FILETIME creationTime, exitTime, kernelTime, userTime;

	BOOL bSuccess = GetProcessTimes(hProcess, &creationTime, &exitTime, &kernelTime, &userTime);

	CloseHandle(hProcess);

	if (!bSuccess)
		return 0;

	ULARGE_INTEGER i;
	i.LowPart = kernelTime.dwLowDateTime;
	i.HighPart = kernelTime.dwHighDateTime;

	uint64_t ret = i.QuadPart;

	i.LowPart = userTime.dwLowDateTime;
	i.HighPart = userTime.dwHighDateTime;

	ret += i.QuadPart;

	return ret;
}

#else

void DebugWrite(const char* msg)
{
	msg;
}

#if defined(linux)

#include <sys/stat.h>
#include <sys/types.h>
#include <fcntl.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>

uint64_t GetProcessTicks(int pid)
{
	char P_cmd[16];
	char P_state;
	int P_pid;
	int P_ppid, P_pgrp, P_session, P_tty_num, P_tpgid;
	unsigned long P_flags, P_min_flt, P_cmin_flt, P_maj_flt, P_cmaj_flt, P_utime, P_stime;

	// Follows minimal.c from procps linux tools
	char buf[800]; /* about 40 fields, 64-bit decimal is about 20 chars */
	int num;
	int fd;
	char* tmp;
	snprintf(buf, 32, "/proc/%d/stat", pid);
	if ( (fd = open(buf, O_RDONLY, 0) ) == -1 ) return 0;
	num = read(fd, buf, sizeof buf - 1);
	close(fd);
	if(num<80) return 0;
	buf[num] = '\0';
	tmp = strrchr(buf, ')');      /* split into "PID (cmd" and "<rest>" */
	*tmp = '\0';                  /* replace trailing ')' with NUL */
	/* parse these two strings separately, skipping the leading "(". */
	memset(P_cmd, 0, sizeof P_cmd);          /* clear */
	sscanf(buf, "%d (%15c", &P_pid, P_cmd);  /* comm[16] in kernel */
	num = sscanf(tmp + 2,                    /* skip space after ')' too */
		"%c "
		"%d %d %d %d %d "
		"%lu %lu %lu %lu %lu %lu %lu ",
		&P_state,
		&P_ppid, &P_pgrp, &P_session, &P_tty_num, &P_tpgid,
		&P_flags, &P_min_flt, &P_cmin_flt, &P_maj_flt, &P_cmaj_flt, &P_utime, &P_stime
	);

	if(num < 13) return 0;
	if(P_pid != pid) return 0;

	return P_utime + P_stime;
}

#elif defined(__APPLE__)

#include <sys/time.h>
#include <sys/proc.h>
#include <sys/proc_info.h>
#include <libproc.h>

uint64_t GetProcessTicks(int pid)
{
	struct proc_taskinfo ti;
	int err = proc_pidinfo(pid, PROC_PIDTASKINFO, 0, &ti, sizeof(ti));

	if (err != sizeof(ti))
		return 0;

	// Is in nanoseconds, convert to milliseconds
	// otherwise we will probably never be idle
	uint64_t ret = ti.pti_total_user;
	ret += ti.pti_total_system;
	ret /= 1000000;
	return ret;
}

#else

#error Missing GetProcessTicks Implementation

#endif

#endif

