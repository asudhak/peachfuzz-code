#include "stdafx.h"

const int kDefaultServerPort = 4242;
const int kBufferSize = 1024;

SOCKET SetUpListener(const char* host, int port);
SOCKET AcceptConnection(SOCKET listener, sockaddr_in* remote);
bool EchoIncomingPackets(SOCKET socket);
int Run(const char* host, int port);

int main(int argc, char* argv[])
{
    WSAData wsaData;
    int ret;
    const char* host;
    int port;

    // Do we have enough command line arguments?
    if (argc < 2) {
	fprintf(stderr, "usage: %s <server-address> [server-port]\n", argv[0]);
	fprintf(stderr, "\tIf you don't pass server-port, it defaults to %d.\n", kDefaultServerPort);
        return 1;
    }

    host = argv[1];
    port = (argc >= 3) ? atoi(argv[2]) : kDefaultServerPort;

    if (argc > 3) {
	fprintf(stderr, "%d extra argument%s ignored.  FYI.\n", argc - 3, argc == 4 ? "" : "s");
    }

    if ((ret = WSAStartup(MAKEWORD(1, 1), &wsaData)) != 0) {
	fprintf(stderr, "WSAStartup() returned error code %d.\n", ret);
        return 255;
    }

    int retval = Run(host, port);

    WSACleanup();
    return retval;
}

int Run(const char* host, int port)
{
    SOCKET listener;

    printf("Establishing the listener...\n");

    listener = SetUpListener(host, htons((u_short)port));
    if (listener == INVALID_SOCKET) {
	fprintf(stderr, "\nestablish listener error: %d\n", WSAGetLastError());
        return 3;
    }

    for (;;) {
	SOCKET socket;
        sockaddr_in remote;

	printf("Waiting for a connection...\n");

        socket = AcceptConnection(listener, &remote);
        if (socket == INVALID_SOCKET) {
	    fprintf(stderr, "\naccept connection error: %d\n", WSAGetLastError());
	    return 3;
	}

	printf("Accepted connection from %s:%d.\n",
	       inet_ntoa(remote.sin_addr),
	       ntohs(remote.sin_port));

        if (!EchoIncomingPackets(socket)) {
	    fprintf(stderr, "\necho incoming packets error: %d\n", WSAGetLastError());
	    return 3;
	}

	printf("Shutting connection down...\n");

        if (closesocket(socket) != 0) {
	    fprintf(stderr, "\nshutdown connection error: %d\n", WSAGetLastError());
	    return 3;
	}

	printf("Connection is down.\n");
    }
}

SOCKET SetUpListener(const char* host, int port)
{
    SOCKET listener;
    u_long addr;
    sockaddr_in sa;
    
    addr = inet_addr(host);
    if (addr == INADDR_NONE)
	return INVALID_SOCKET;

    listener = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (listener == INVALID_SOCKET)
	return INVALID_SOCKET;

    sa.sin_family = AF_INET;
    sa.sin_addr.s_addr = addr;
    sa.sin_port = (u_short)port;

    if (bind(listener, (sockaddr*)&sa, sizeof(sa)) == SOCKET_ERROR)
	return INVALID_SOCKET;

    if (listen(listener, 1) == SOCKET_ERROR)
	return INVALID_SOCKET;

    return listener;
}

SOCKET AcceptConnection(SOCKET listener, sockaddr_in* remote)
{
    socklen_t size = sizeof(*remote);
    return accept(listener, (sockaddr*)remote, &size);
}

void CrashMe(char* in)
{
	printf("\nIn CrashMe()\n");
	char buff[10];
	// Should A/V us :)
	strcpy(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
	strcat(buff, in);
}

bool EchoIncomingPackets(SOCKET socket)
{
    char* buf = (char*)malloc(kBufferSize);
    int len;

    do {
        len = recv(socket, buf, kBufferSize, 0);
        if (len > 0) {
            printf("Received %d bytes from client.\n", len);


	    // Add a silly stack overflow
	    if (len >= 1024) {
		CrashMe(buf);
	    }
        }
        else if (len == SOCKET_ERROR) {
            return false;
        }
    } while (len != 0);

    printf("Connection closed by peer.\n");

    return true;
}
