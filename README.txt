Peach 3 - RC 1
================

Welcome to Peach 3, a complete re-write of Peach using the 
Microsoft.NET framework.  Peach 3 is a cross-platform
fuzzer that mainly targets data consumers.

Peach 3 currently supports the following OSes:

  - Windows
  - OS X
  - Linux (e.g. Ubuntu, Redhat, etc.)


Installing from Source
----------------------

Windows Pre-requisits:
  - Microsoft.NET v4
  - Visual Studio 2010 SP1

Linux Pre-requisites:  
  - build-essential
  - mono-complete
  - g++-multilib (x86_64 only)

OS X Pre-requisites:
  - XCode 4
  - Mono SDK (2.10.10)

./waf configure
./waf build
./waf install



Copyright (c) Deja vu Security
Copyright (c) Michael Eddington
