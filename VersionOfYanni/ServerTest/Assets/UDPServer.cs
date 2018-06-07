using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using UnityEngine;
using System.Text;
using UDPChat;

namespace UDPChat
{ 
    public class UDPServer : MonoBehaviour
    {
        #region field && state

        public bool wire, wireless, WithVires;
        public string wireId, wirelessId;
        public string ViresId;
        private string ServerId;
        public int s_Inport, s_Outport, c_Inport, c_Outport;   // Port for ingoing and outgoing
        public static List<IPEndPoint> clients = new List<IPEndPoint>(); // one element for each client.
        public static IPEndPoint ViresIpEndpointOut;
        public static IPEndPoint ClientIpEndpointIn = null, ClientIpEndpointOut = null;
        public static UdpClient serverIn = null, serverOut = null;
        public static byte[] buffer = null;
        public static byte[] data_serialized = null;
        public static byte[] dataInBytes = null;
        public UInt32[] counter = new UInt32[10];
        bool flag = false; // check the package is from vires or unity
        #endregion

        void Start()
        {
            if(WithVires)
            {
                IPAddress address = IPAddress.Parse(ViresId);
                ViresIpEndpointOut = new IPEndPoint(address, c_Outport);
                clients.Add(ViresIpEndpointOut);
            }
            if (wire) { ServerId = wireId; }
            if (wireless) { ServerId = wirelessId; }
            Init();
        }

        void Init()
        {
            Debug.Log("Server ready");
            serverIn = new UdpClient(s_Inport); //Creates a UdpClient as server for reading incoming data.                     
            ClientIpEndpointOut = new IPEndPoint(IPAddress.Any, c_Outport);//read datagrams sent from any source. 
            serverIn.BeginReceive(new AsyncCallback(OnReceive), null); // begin receive data                                                         
        }

        private void AddClient(IPEndPoint c)
        {
            clients.Add(c); // add a new client
            Debug.Log("<" + c.Address.ToString() + "> is connected");
        }

        public void OnReceive(IAsyncResult res)
        {
            buffer = serverIn.EndReceive(res, ref ClientIpEndpointOut);
            Debug.Log("End received from :"+ ClientIpEndpointOut.ToString());
            if (clients.Contains(ClientIpEndpointOut) == false)
                {AddClient(ClientIpEndpointOut); }
            MultiCast(buffer);
            serverIn.BeginReceive(new AsyncCallback(OnReceive), null);
        }

        public void MultiCast(byte[] data)
        {
            serverOut = new UdpClient(s_Outport); //Creates a UdpClient as server for reading outcoming data.
            for (int i = 0; i < clients.Count; i++)
            {
                try
                {
                    if (clients[i].Address.ToString() != ClientIpEndpointOut.Address.ToString())
                    {
                        ClientIpEndpointIn = new IPEndPoint(clients[i].Address, c_Inport);
                        serverOut.Connect(ClientIpEndpointIn);
                        serverOut.Send(data, data.Length); // send data
                        Debug.Log("The message was sent to " + ClientIpEndpointIn.ToString());
                        counter[i]++;
                        //if (counter[i] == 200)
                        //{
                        //    Debug.Log("<" + clients[i].Address.ToString() + "> is disconnected");
                        //    clients.Remove(clients[i]);
                        //}
                    }
                    //else
                    //{
                    //    counter[i] = 0;
                    //}
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }
            serverOut.Close();
        }

        public void OnApplicationQuit()
        {
            serverIn.Close();
            serverOut.Close();
        }
        public void OnDisable()
        {
            serverIn.Close();
            serverOut.Close();
        }
    }
}