using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] data = new byte[2];
            int port_in = 8889;
            int port_out = 8888;
            int port_client_in = 8887;
            int port_client_out = 8886;

            UdpClient udpServerReceive = new UdpClient(port_in);
            UdpClient udpServerSend = new UdpClient(port_out);

            IPEndPoint remote_ip_endpoint_send = new IPEndPoint(IPAddress.Any, port_client_out);
            IPEndPoint remote_ip_endpoint_receive = new IPEndPoint(IPAddress.Any, port_client_in);

            Console.WriteLine("Waiting for a client...");

            for (int i = 0; i < 100; i++)
            {
                data = udpServerReceive.Receive(ref remote_ip_endpoint_send);
                Console.WriteLine("Received " + (byte)data[1] + " from " + (char)data[0] + " (" + ((EndPoint)remote_ip_endpoint_send).ToString() + ")");

                // Echo same message
                remote_ip_endpoint_receive.Address = remote_ip_endpoint_send.Address;
                udpServerSend.Send(data, data.Length, remote_ip_endpoint_receive);
            }
            udpServerSend.Close();
            udpServerReceive.Close();
        }
    }
}
