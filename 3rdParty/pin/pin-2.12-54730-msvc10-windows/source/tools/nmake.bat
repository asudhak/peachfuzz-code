@echo off
REM This batch file works in this fashion: if it gets no arguments, it checks if the environment 
REM looks like a Visual Studio Command Prompt environment. If it does it assumes that it should 
REM do nothing and calls nmake.
REM If it does get arguments it attempts to set up the appropriate environment.
REM MSVCVER - determines what version of MS Visual Studio to use when compiling the pintools. valid values are 8, 9 or 10
REM                         8 - Use Visual Studio 2005 (default)
REM                         9 - Use Visual Studio 2008
REM                        10 - Use Visual Studio 2010
REM                         if MSVCVER is not explicitly stated, default to VS 2005.
REM TARGET - determines what CPU architecture the tools will be built for. Requires Visual Studio Cross Tools (R) to work. Valid values are 'ia32' or 'ia32e'.
REM			ia32 - build tools for the Intel ia32 (TM) architecture.
REM			ia32e - build tools for the intel64/ia32e architecture.
REM			The default value is the architecture of the computer's CPU.
REM Other values are passed to the Nmakefile without further processing.

REM setlocal allows me to always use vcvarsall and not worry about anything. all settings will return to the previous state by the end of the batch file.
setlocal

REM Parsing command line options:

:parseloop
if %1.==. goto :endloop
if /i "%1"=="MSVCVER" goto :msvcversionset
if /i "%1"=="TARGET" goto :targetset
if /i "%1"=="/h" goto :usage
if /i "%1"=="/?" goto :usage
if /i "%1"=="-h" goto :usage
shift
goto :parseloop


:msvcversionset
REM if MSVCVER is explicitly stated do this:
if /i "%2"=="8" set MSVCVER=8
if /i "%2"=="9" set MSVCVER=9
if /i "%2"=="10" set MSVCVER=10
REM otherwise MSVCVER should not be set at all
if "%MSVCVER%" == "" goto :usage
shift
goto :parseloop

:targetset
set TARGET=
if /i "%2"=="ia32" set TARGET=ia32
if /i "%2"=="ia32e" set TARGET=ia32e
if /i "%TARGET%"=="" goto :usage
shift
goto :parseloop

:endloop
REM If VCINSTALLDIR isn't set it means MSVC environment variables don't exist. If it does then the other variables are probably set too and we go on with them.
if NOT "%VCINSTALLDIR%" == "" goto run_nmake
if /i "%MSVCVER%"== "10"  goto :setFor2010
if /i "%MSVCVER%"== "9"  goto :setFor2008
if /i "%MSVCVER%"== "8"  goto :setFor2005
goto :setFor2005

:setFor2005

REM If no VS80COMNTOOLS environment variable is available, we have no way of finding the installation. 
REM (Visual Studio installation sets this environment variable even in normal command line, so this probably means VS is not installed)
IF "%VS80COMNTOOLS%"=="" goto no_vc_nmake
set VCINSTALLDIR=%VS80COMNTOOLS%\..\..\VC
goto :run_nmake

:setFor2008

REM If no VS90COMNTOOLS environment variable is available, we have no way of finding the installation. 
REM (Visual Studio installation sets this environment variable even in normal command line, so this probably means VS is not installed)
IF "%VS90COMNTOOLS%"=="" goto no_vc_nmake
set VCINSTALLDIR=%VS90COMNTOOLS%\..\..\VC
goto :run_nmake

:setFor2010

REM If no VS100COMNTOOLS environment variable is available, we have no way of finding the installation. 
REM (Visual Studio installation sets this environment variable even in normal command line, so this probably means VS is not installed)
IF "%VS100COMNTOOLS%"=="" goto no_vc_nmake
set VCINSTALLDIR=%VS100COMNTOOLS%\..\..\VC
goto :run_nmake

:no_vc_nmake
echo:
echo %0: Could not find NMAKE.exe in the MSVC 8.0/9.0/10.0 directories. Using search path.
echo %0: If the problem persists, try running this file in the "Visual Studio 2005 Command Prompt",
echo %0: "Visual Studio 2008 Command Prompt" or "Visual Studio 2010 Command Prompt" according to what you require.
echo:
goto :just_run_nmake

:run_nmake
REM Setting up environment for specific target architecture.
if /i "%PROCESSOR_ARCHITEW6432%"=="amd64" set PROCESSOR_ARCHITECTURE=amd64
if /i "%PROCESSOR_ARCHITECTURE%"=="" set PROCESSOR_ARCHITECTURE=x86
if /i "%TARGET%"=="ia32" set HOST_TARGET=%PROCESSOR_ARCHITECTURE%_x86
if /i "%TARGET%"=="ia32e" set HOST_TARGET=%PROCESSOR_ARCHITECTURE%_amd64
if /i "%TARGET%"=="" set HOST_TARGET=%PROCESSOR_ARCHITECTURE%_%PROCESSOR_ARCHITECTURE%

if /i "%HOST_TARGET%"=="x86_amd64" call "%VCINSTALLDIR%\vcvarsall.bat" x86_amd64
if /i "%HOST_TARGET%"=="x86_x86" call "%VCINSTALLDIR%\vcvarsall.bat" x86
if /i "%HOST_TARGET%"=="amd64_x86" call "%VCINSTALLDIR%\vcvarsall.bat" x86
if /i "%HOST_TARGET%"=="amd64_amd64" call "%VCINSTALLDIR%\vcvarsall.bat" amd64

REM This line runs Nmake after we've set environment variables for it. 
REM No need for Nmake to process them again, so we tell it not to do so.
nmake.exe /NOLOGO /f Nmakefile SET_VCENV=0 %*
goto :end

:just_run_nmake
REM In this case we did not set the environment variables so we leave configuration to the Nmakefile. 
nmake.exe /NOLOGO /f Nmakefile %*
goto :end

:usage
echo:
echo The correct usage is:
echo %0 [MSVCVER=8^|9^|10] [TARGET=ia32^|ia32e] [debug=0^|1] [pin_home=^<Pin home directory^>]
echo:
goto :end

:end
endlocal 
