using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerCSharp
{
    class Program
    {
        static int port_client_out = 8886;
        static int port_server_in = 8889;
        static int port_client_in = 8887;
        static int port_server_out = 8888;

        private struct Client_message
        {
            public char id;  // ID represented by a character e.g. 'z'
            public byte val; // generic value
            public IPAddress address; // ip address of the client
        }
        static List<Client_message> client_message_list = new List<Client_message>();
        static List<IPAddress> clients = new List<IPAddress>();

        public static void CollectStatesFromClient(object udpClientObject)
        {
            byte[] data_recv = new byte[2];
            UdpClient udpServerReceive = (UdpClient)udpClientObject;

            IPEndPoint remote_ip_endpoint_send = new IPEndPoint(IPAddress.Any, port_client_out);

            while(true)
            {
                data_recv = udpServerReceive.Receive(ref remote_ip_endpoint_send);
                Console.WriteLine("Received " + (byte)data_recv[1] + " from " + (char)data_recv[0] + " (" + ((EndPoint)remote_ip_endpoint_send).ToString() + ")");

                Client_message msg = new Client_message
                {
                    id = (char)data_recv[0],
                    val = data_recv[1],
                    address = remote_ip_endpoint_send.Address
                };

                if (!clients.Contains(remote_ip_endpoint_send.Address))
                { 
                    lock (clients)
                    {
                        clients.Add(remote_ip_endpoint_send.Address);
                    }
                }

                lock (client_message_list) 
                {
                    client_message_list.Add(msg);
                }
            }
        }

        static void Main(string[] args)
        {
            byte[] data = new byte[2];

            UdpClient udpServerSend = new UdpClient(port_server_out);
            UdpClient udpServerReceive = new UdpClient(port_server_in);

            Console.CancelKeyPress += delegate {
                Console.WriteLine("Exit gracefully.");
                udpServerSend.Close();
                udpServerReceive.Close();
            };

            Thread thread_collect_status = new Thread(new ParameterizedThreadStart(Program.CollectStatesFromClient));
            thread_collect_status.Start(udpServerReceive);

            IPEndPoint remote_ip_endpoint_receive = new IPEndPoint(IPAddress.Any, port_client_in);

            Console.WriteLine("Waiting for clients...");

            while (true)
            {
                // Send list of messages
                lock (client_message_list)
                {
                    if (client_message_list.Count > 0)
                    {
                        Console.WriteLine("Echo " + client_message_list.Count + " messages.");
                        foreach (Client_message msg in client_message_list)
                        {
                            data[0] = (byte)msg.id;
                            data[1] = msg.val;

                            lock (clients)
                            {
                                foreach (IPAddress addr in clients)
                                {
                                    remote_ip_endpoint_receive.Address = addr;
                                    udpServerSend.Send(data, data.Length, remote_ip_endpoint_receive);
                                }
                            }
                        }
                        client_message_list.Clear();
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}
