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
#define BUFLEN 512  //Max length of buffer
#define PORT 8888   //The port on which to listen for incoming data

int main(void)
{
	struct sockaddr_in server_addr;
	SOCKET socket_client;
	char buf[BUFLEN];
	char message[BUFLEN];
	WSADATA wsa_data;
	int server_addr_len = sizeof(server_addr);


	//Initialise winsock
	printf("\nInitialising Winsock...");
	if (WSAStartup(MAKEWORD(2, 2), &wsa_data) != 0)
	{
		printf("Failed. Error Code : %d\n", WSAGetLastError());
		exit(EXIT_FAILURE);
	}
	printf("Initialised.\n");

	//create socket
	if ((socket_client = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) == SOCKET_ERROR)
	{
		printf("socket() failed with error code : %d\n", WSAGetLastError());
		exit(EXIT_FAILURE);
	}

	// setup address structure
	memset((char *)&server_addr, 0, sizeof(server_addr));
	server_addr.sin_family = AF_INET;
	server_addr.sin_port = htons(PORT);

	if (inet_pton(AF_INET, SERVER, &server_addr.sin_addr) != 1)
	{
		printf("inet_pton Failed.\n");
	}
	else
	{
		printf("pton success!\n");
	}

	//start communication
	while (1)
	{
		printf("Enter message : ");
		gets_s(message);

		//send the message
		if (sendto(socket_client, message, (int)strlen(message), 0, (struct sockaddr *) &server_addr, sizeof(server_addr)) == SOCKET_ERROR)
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

		puts(buf);
	}

	closesocket(socket_client);
	WSACleanup();

	return 0;
}