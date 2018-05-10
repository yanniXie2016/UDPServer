/*
    Simple UDP Server
*/
 
#include <stdio.h>
#include <winsock2.h>
#include <Ws2tcpip.h>
 
#pragma comment(lib,"ws2_32.lib") // need winsock Library
 
#define BUFLEN 2  //Max length of buffer
#define PORT 8888   //The port on which to listen for incoming data
 
int main()
{
    SOCKET socket_server;
	WSADATA wsa_data;
	struct sockaddr_in local_addr, from_addr;
    int from_addr_len, recv_len;
    char buf[BUFLEN];
	char addr_name[INET6_ADDRSTRLEN];
 
    from_addr_len = sizeof(from_addr) ;
     
    //Initialise winsock
    if (WSAStartup(MAKEWORD(2,2),&wsa_data) != 0)
    {
        printf("Failed. Error Code : %d",WSAGetLastError());
        exit(EXIT_FAILURE);
    }
     
    //Create a socket
    if((socket_server = socket(AF_INET , SOCK_DGRAM , 0 )) == INVALID_SOCKET)
    {
        printf("Could not create socket : %d\n" , WSAGetLastError());
    }
     
    //Prepare the sockaddr_in structure
    local_addr.sin_family = AF_INET;
	local_addr.sin_addr.s_addr = INADDR_ANY;
	local_addr.sin_port = htons( PORT );
     
    // Bind socket to specific local port - for clients to send to
    if( bind(socket_server, (struct sockaddr *)&local_addr, sizeof(local_addr)) == SOCKET_ERROR)
    {
        printf("Bind failed with error code : %d\n" , WSAGetLastError());
        exit(EXIT_FAILURE);
    }
 
    // keep listening for data
    while(1)
    {
        printf("Waiting for data on port %d...\n", ntohs(local_addr.sin_port));
         
        // clear the buffer by filling null, it might have previously received data
        memset(buf,'\0', BUFLEN);
         
        //try to receive some data, this is a blocking call
        if ((recv_len = recvfrom(socket_server, buf, BUFLEN, 0, (struct sockaddr *) &from_addr, &from_addr_len)) == SOCKET_ERROR)
        {
            printf("recvfrom() failed with error code : %d" , WSAGetLastError());
            exit(EXIT_FAILURE);
        }
         
        // print details of the client/peer and the data received
		printf("Server received: %c %d from %s remote port: %d local port: %d\n", buf[0], buf[1],
			inet_ntop(AF_INET, &from_addr.sin_addr, addr_name, INET6_ADDRSTRLEN), ntohs(from_addr.sin_port), ntohs(local_addr.sin_port));

        // now reply the client with the same data
        if (sendto(socket_server, buf, recv_len, 0, (struct sockaddr*) &from_addr, from_addr_len) == SOCKET_ERROR)
        {
            printf("sendto() failed with error code : %d\n" , WSAGetLastError());
            exit(EXIT_FAILURE);
        }
    }
 
    closesocket(socket_server);
    WSACleanup();
     
    return 0;
}