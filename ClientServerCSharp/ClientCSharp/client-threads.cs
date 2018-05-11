using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ClientCSharp
{
    class Program
    {
        static int n_loops = 100;

        static int port_client_out = 8886;
        static int port_server_in = 8889;
        static int port_client_in = 8887;
        static int port_server_out = 8888;

        public static void CollectStatesFromServer(object udpClientObject)
        {
            byte[] data_recv = new byte[2];
            UdpClient udpClientReceive = (UdpClient)udpClientObject;

            IPEndPoint remote_ip_endpoint_send = new IPEndPoint(IPAddress.Any, port_server_out);

            while(true)
            {
                data_recv = udpClientReceive.Receive(ref remote_ip_endpoint_send);
                Console.WriteLine("Received " + (byte)data_recv[1] + " from " + (char)data_recv[0] + " (" + ((EndPoint)remote_ip_endpoint_send).ToString() + ")");
            }
        }

        static void Main(string[] args)
        {
            String ip_address = "127.0.0.1";
            byte[] data = new byte[2];  

            if (args.Length > 0)
            {
                data[0] = (byte)args[0][0];
                if(args.Length > 1)
                {
                    ip_address = args[1];
                }
            }
            else
            {
                Console.WriteLine("Usage: client <client id> [remote IP address (127.0.0.1 is default)]");
                return;
            }
            Console.WriteLine("Client ID: " + (char)data[0] + " Server IP: " + ip_address);

            UdpClient udpClientReceive = new UdpClient(port_client_in);
            UdpClient udpClientSend = new UdpClient(port_client_out);

            Console.CancelKeyPress += delegate {
                Console.WriteLine("Exit gracefully.");
                udpClientSend.Close();
                udpClientReceive.Close();
            };

            Thread thread_collect_status = new Thread(new ParameterizedThreadStart(CollectStatesFromServer));
            thread_collect_status.Start(udpClientReceive);

            IPEndPoint remote_ip_endpoint_receive = new IPEndPoint(IPAddress.Parse(ip_address), port_server_in);
            udpClientSend.Connect(remote_ip_endpoint_receive);

            for (int i = 0; i < n_loops; i++)
            {
                data[1] = (byte)i;
                udpClientSend.Send(data, data.Length);
                Thread.Sleep(200);
            }

            Thread.Sleep(1000);  // Allow for last messages from server to arrive
            thread_collect_status.Abort();
            udpClientSend.Close();
            udpClientReceive.Close();
        }
    }
}
