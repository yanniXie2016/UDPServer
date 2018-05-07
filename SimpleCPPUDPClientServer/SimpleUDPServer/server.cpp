/*
    Simple UDP Server
*/
 
#include <stdio.h>
#include <winsock2.h>
#include <Ws2tcpip.h>
 
#pragma comment(lib,"ws2_32.lib") // need winsock Library
 
#define BUFLEN 512  //Max length of buffer
#define PORT 8888   //The port on which to listen for incoming data
 
int main()
{
    SOCKET socket_server;
	WSADATA wsa_data;
	struct sockaddr_in local, from;
    int from_len, recv_len;
    char buf[BUFLEN];
	char addr_name[INET6_ADDRSTRLEN];
 
    from_len = sizeof(from) ;
     
    //Initialise winsock
    printf("\nInitialising Winsock...");
    if (WSAStartup(MAKEWORD(2,2),&wsa_data) != 0)
    {
        printf("Failed. Error Code : %d",WSAGetLastError());
        exit(EXIT_FAILURE);
    }
    printf("Initialised.\n");
     
    //Create a socket
    if((socket_server = socket(AF_INET , SOCK_DGRAM , 0 )) == INVALID_SOCKET)
    {
        printf("Could not create socket : %d\n" , WSAGetLastError());
    }
    printf("Socket created.\n");
     
    //Prepare the sockaddr_in structure
    local.sin_family = AF_INET;
	local.sin_addr.s_addr = INADDR_ANY;
	local.sin_port = htons( PORT );
     
    //Bind
    if( bind(socket_server ,(struct sockaddr *)&local, sizeof(local)) == SOCKET_ERROR)
    {
        printf("Bind failed with error code : %d\n" , WSAGetLastError());
        exit(EXIT_FAILURE);
    }
    printf("Bind done.\n");
 
    //keep listening for data
    while(1)
    {
        printf("Waiting for data...\n");
         
        //clear the buffer by filling null, it might have previously received data
        memset(buf,'\0', BUFLEN);
         
        //try to receive some data, this is a blocking call
        if ((recv_len = recvfrom(socket_server, buf, BUFLEN, 0, (struct sockaddr *) &from, &from_len)) == SOCKET_ERROR)
        {
            printf("recvfrom() failed with error code : %d" , WSAGetLastError());
            exit(EXIT_FAILURE);
        }
         
        //print details of the client/peer and the data received
		printf("Received packet from %s:%d\n", inet_ntop(AF_INET, &from.sin_addr, addr_name, INET6_ADDRSTRLEN), ntohs(from.sin_port));
        printf("Data: %s\n" , buf);
         
        //now reply the client with the same data
        if (sendto(socket_server, buf, recv_len, 0, (struct sockaddr*) &from, from_len) == SOCKET_ERROR)
        {
            printf("sendto() failed with error code : %d\n" , WSAGetLastError());
            exit(EXIT_FAILURE);
        }
    }
 
    closesocket(socket_server);
    WSACleanup();
     
    return 0;
}