// Peach.Core.Debuggers.Windows.h

#pragma once

using namespace System;
#include <vcclr.h> 

void MainLoop();
void ProcessDebugEvent(DEBUG_EVENT* DebugEv);

WCHAR* _command = NULL;
DWORD _dwThreadId = 0;
HANDLE _hThread = NULL;
LPSTARTUPINFOW startUpInfo = NULL;
LPPROCESS_INFORMATION processInformation = NULL;

// Consumer says we should exit MainLoop
BOOL _ExitDebugger = FALSE;

// Debugger indicates an A/V was found
BOOL _AccessViolation = FALSE;

// Process has started
HANDLE _ProcessStarted = NULL;

// Method will run in new thead
DWORD WINAPI _CreateProcess(LPVOID lpParam)
{
	startUpInfo = new STARTUPINFOW();
	processInformation = new PROCESS_INFORMATION();

	DWORD currentThreadId = GetCurrentThreadId();

	if(!CreateProcess(
		0,			// lpApplicationName
		_command,	// lpCommandLine
		0,			// lpProcessAttributes
		0,			// lpThreadAttributes
		0,			// bInheritHandles
		1,			// dwCreationFlags
		0,			// lpEnvironment
		0,			// lpCurrentDirectory
		startUpInfo, 
		processInformation))
	{
		// TODO -- Handle Errors
	}

	CloseHandle(processInformation->hProcess);
	CloseHandle(processInformation->hThread);

	DebugSetProcessKillOnExit(TRUE);

	MainLoop();

	return 0;
}

void MainLoop()
{
	DEBUG_EVENT* debugEvent = new DEBUG_EVENT();

	SetEvent(_ProcessStarted);

	while(!_ExitDebugger && !_AccessViolation)
	{
		if(!WaitForDebugEvent(debugEvent, 100))
			continue;

		ProcessDebugEvent(debugEvent);

		if(!ContinueDebugEvent(debugEvent->dwProcessId,
			debugEvent->dwThreadId,
			DBG_EXCEPTION_NOT_HANDLED))
		{
			// TODO -- Handle this error
		}
	}
}

void ProcessDebugEvent(DEBUG_EVENT* DebugEv)
{
	switch (DebugEv->dwDebugEventCode)
	{
		case EXCEPTION_DEBUG_EVENT:
			// Process the exception code. When handling 
			// exceptions, remember to set the continuation 
			// status parameter (dwContinueStatus). This value 
			// is used by the ContinueDebugEvent function. 

			//if (verbose)
			//	System::Console.Error.WriteLine("EXCEPTION_DEBUG_EVENT");

			switch (DebugEv->u.Exception.ExceptionRecord.ExceptionCode)
			{
				case EXCEPTION_ACCESS_VIOLATION:
					// First chance: Pass this on to the system. 
					// Last chance: Display an appropriate error. 

					//if(HandleAccessViolation != null)
					//	HandleAccessViolation(DebugEv);
					_AccessViolation = TRUE;

					break;

				default:
					break;
			}

			break;

		case EXIT_PROCESS_DEBUG_EVENT:
			// Display the process's exit code. 

			//dwContinueStatus = OnExitProcessDebugEvent(DebugEv);
			//if (verbose)
			//	Console.Error.WriteLine("EXIT_PROCESS_DEBUG_EVENT");
			if (processInformation->dwProcessId == DebugEv->dwProcessId)
			{
				_ExitDebugger = TRUE;
			}

			break;

		default:
			break;
	}
}

namespace PeachCoreDebuggersWindows {

	public ref class SystemDebugger
	{

	protected:
		String ^command;

	public:

		SystemDebugger()
		{
			if(_ProcessStarted != NULL)
				CloseHandle(_ProcessStarted);

			_ProcessStarted = CreateEvent(0, TRUE, FALSE, TEXT("ProcessStarted"));
		}

		void CreateProcess(String ^command)
		{
			if(_dwThreadId != 0 || _hThread != 0)
			{
				// This is not good!
				throw gcnew System::Exception("Error, thread already created!");
			}

			pin_ptr<const wchar_t> unmngStr = PtrToStringChars(command);
			_command = wcsdup(unmngStr);

			DWORD currentThreadId = GetCurrentThreadId();

			_hThread = CreateThread(
				NULL,		// default security attributes
				0,			// use default stack size
				_CreateProcess,	// thread function
				0,			// argument to thread
				0,			// use default creation flags
				&_dwThreadId);

			WaitForSingleObject(_ProcessStarted, INFINITE);
			CloseHandle(_ProcessStarted);
			_ProcessStarted = NULL;
		}

		unsigned int dwProcessId()
		{
			return processInformation->dwProcessId;
		}

		// Did we see an access violation?
		bool HasAccessViolation()
		{
			if(_AccessViolation == TRUE)
				return true;
		}

		// Stop our debugger
		void StopDebugger()
		{
			if(_hThread == NULL)
				return;

			_ExitDebugger = TRUE;
			WaitForSingleObject(_hThread, INFINITE);
			CloseHandle(_hThread);
			_hThread = NULL;

			if(_ProcessStarted != NULL)
			{
				CloseHandle(_ProcessStarted);
				_ProcessStarted = NULL;
			}

			delete startUpInfo;
			delete processInformation;

			startUpInfo = NULL;
			processInformation = NULL;

			_dwThreadId = 0;
			_hThread = 0;
		}
	};
}

// END
