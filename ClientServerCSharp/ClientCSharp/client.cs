using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ClientCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            String ip_address = "127.0.0.1";
            byte[] data = new byte[2];  
            byte[] data_recv = new byte[2];

            int port_out = 8886;
            int port_in = 8887;
            int port_server_in = 8889;
            int port_server_out = 8888;

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

            UdpClient udpClientSend = new UdpClient(port_out);
            UdpClient udpClientReceive = new UdpClient(port_in);

            IPEndPoint remote_ip_endpoint_receive = new IPEndPoint(IPAddress.Parse(ip_address), port_server_in);
            IPEndPoint remote_ip_endpoint_send = new IPEndPoint(IPAddress.Parse(ip_address), port_server_out);

            udpClientSend.Connect(remote_ip_endpoint_receive);

            for (int i = 0; i < 100; i++)
            {
                data[1] = (byte)i;

                udpClientSend.Send(data, data.Length);

                data_recv = udpClientReceive.Receive(ref remote_ip_endpoint_send);
                Console.WriteLine("Received " + (byte)data_recv[1] + " from " + (char)data_recv[0] + " (" + ((EndPoint)remote_ip_endpoint_send).ToString() + ")");

                Thread.Sleep(200);
            }

            udpClientReceive.Close();
            udpClientSend.Close();
        }
    }
}
