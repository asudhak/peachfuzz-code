/***********************************************************************
 main.cpp - The main() routine for all the "Basic Winsock" suite of
    programs from the Winsock Programmer's FAQ.  This function parses
    the command line, starts up Winsock, and calls an external function
    called DoWinsock to do the actual work.

 This program is hereby released into the public domain.  There is
 ABSOLUTELY NO WARRANTY WHATSOEVER for this product.  Caveat hacker.
***********************************************************************/
#include "stdafx.h"
#include <winsock.h>

#include <stdlib.h>
#include <iostream>

using namespace std;


//// Prototypes ////////////////////////////////////////////////////////

extern int DoWinsock(const char* pcHost, int nPort);


//// Constants /////////////////////////////////////////////////////////

// Default port to connect to on the server
const int kDefaultServerPort = 4242;


//// main //////////////////////////////////////////////////////////////

int main(int argc, char* argv[])
{
    // Do we have enough command line arguments?
    if (argc < 2) {
        cerr << "usage: " << argv[0] << " <server-address> " <<
                "[server-port]" << endl << endl;
        cerr << "\tIf you don't pass server-port, it defaults to " <<
                kDefaultServerPort << "." << endl;
        return 1;
    }

    // Get host and (optionally) port from the command line
    const char* pcHost = argv[1];
    int nPort = kDefaultServerPort;
    if (argc >= 3) {
        nPort = atoi(argv[2]);
    }

    // Do a little sanity checking because we're anal.
    int nNumArgsIgnored = (argc - 3);
    if (nNumArgsIgnored > 0) {
        cerr << nNumArgsIgnored << " extra argument" <<
                (nNumArgsIgnored == 1 ? "" : "s") << 
                " ignored.  FYI." << endl;
    }

    // Start Winsock up
    WSAData wsaData;
	int nCode;
    if ((nCode = WSAStartup(MAKEWORD(1, 1), &wsaData)) != 0) {
		cerr << "WSAStartup() returned error code " << nCode << "." <<
				endl;
        return 255;
    }

    // Call the main example routine.
    int retval = DoWinsock(pcHost, nPort);

    // Shut Winsock back down and take off.
    WSACleanup();
    return retval;
}

