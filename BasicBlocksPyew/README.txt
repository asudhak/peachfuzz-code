Peach 3 - Basic Block Finder
============================

Copyright (c) Deja vu Security
Michael Eddington

This is a project to help replace PIN tools by identifying all basic blocks in an
executable.  We can use this to set breakpoints for code coverage.

The following files are written by Deja vu Security and do *not* fall under
GPL.  They are licensed using MIT License as is the rest of Peach.

 * basicblocks.py -- Core code utalizing pyew to locate BB in binary
 * setup.py -- Distutils project to build basicblocks.exe
 
Usage:

 basicblocks.exe target.exe
 
Will output to console all basic blocks found.

