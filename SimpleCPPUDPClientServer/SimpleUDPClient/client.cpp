/*
Simple udp client
*/

#include <stdio.h>
#include <winsock2.h>
#include <Ws2tcpip.h>


// Need to link with Ws2_32.lib, Mswsock.lib, and Advapi32.lib
#pragma comment (lib, "Ws2_32.lib")
#pragma comment (lib, "Mswsock.lib")
#pragma comment (lib, "AdvApi32.lib")

#define SERVER "127.0.0.1"  //ip address of udp server
#define BUFLEN 2  //Max length of buffer
#define PORT_LOCAL 8887   //The port on which to listen for incoming data
#define PORT_REMOTE 8888   //The port on remote host to receive data
#define IP_LEN 256

int main(int argc, char *argv[])
{
	struct sockaddr_in server_addr, local_addr;
	SOCKET socket_client;
	char buf[BUFLEN];
	char server_ip[IP_LEN];
	WSADATA wsa_data;
	int server_addr_len = sizeof(server_addr);
	char addr_name[INET6_ADDRSTRLEN];


	if (argc < 2)
	{
		printf("Usage: client <id> [ip (default=127.0.0.1)]");
		return 0;
	}

	buf[0] = argv[1][0];
	if (argc > 2)
	{
		strcpy_s(server_ip, IP_LEN, argv[2]);
	}
	else
	{
		strcpy_s(server_ip, IP_LEN, SERVER);
	}

	printf("id=%c ip=%s\n", buf[0], server_ip);

	// Initialise winsock
	if (WSAStartup(MAKEWORD(2, 2), &wsa_data) != 0)
	{
		printf("Failed. Error Code : %d\n", WSAGetLastError());
		exit(EXIT_FAILURE);
	}

	// create socket
	if ((socket_client = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) == SOCKET_ERROR)
	{
		printf("socket() failed with error code : %d\n", WSAGetLastError());
		exit(EXIT_FAILURE);
	}

	// setup local address structure
	memset((char *)&local_addr, 0, sizeof(local_addr));
	local_addr.sin_port = htons(PORT_LOCAL);
	local_addr.sin_family = AF_INET;
	if (bind(socket_client, (struct sockaddr*)&local_addr, sizeof(local_addr)) == SOCKET_ERROR)
	{
		printf("Bind local port failed with error code : %d\n", WSAGetLastError());
		exit(EXIT_FAILURE);
	}

	// setup server address structure
	memset((char *)&server_addr, 0, sizeof(server_addr));
	server_addr.sin_family = AF_INET;
	server_addr.sin_port = htons(PORT_REMOTE);

	if (inet_pton(AF_INET, server_ip, &server_addr.sin_addr) != 1)
	{
		printf("inet_pton Failed.\n");
	}

	//start communication
	for (int i = 0; i < 100; i++)
	{
		buf[1] = i;

		printf("Sending:  %c %d   to %s  local port: %d remote port: %d\n", buf[0], buf[1],
			inet_ntop(AF_INET, &server_addr.sin_addr, addr_name, INET6_ADDRSTRLEN), ntohs(local_addr.sin_port), ntohs(server_addr.sin_port));
		//send the message
		if (sendto(socket_client, buf, BUFLEN, 0, (struct sockaddr *) &server_addr, sizeof(server_addr)) == SOCKET_ERROR)
		{
			printf("sendto() failed with error code : %d\n", WSAGetLastError());
			exit(EXIT_FAILURE);
		}

		// receive a reply and preint it
		// clear the buffer by filling null, it might have previously received data
		memset(buf, '\0', BUFLEN);

		// try to receive some data, this is a blocking call
		if (recvfrom(socket_client, buf, BUFLEN, 0, (struct sockaddr *) &server_addr, &server_addr_len) == SOCKET_ERROR)
		{
			printf("recvfrom() failed with error code : %d", WSAGetLastError());
			exit(EXIT_FAILURE);
		}

		printf("Received: %c %d from %s  local port: %d remote port: %d\n", buf[0], buf[1], 
			inet_ntop(AF_INET, &server_addr.sin_addr, addr_name, INET6_ADDRSTRLEN), ntohs(local_addr.sin_port), ntohs(server_addr.sin_port));

		Sleep(500);
	}

	closesocket(socket_client);
	WSACleanup();

	return 0;
}