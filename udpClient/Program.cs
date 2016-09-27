using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace udpClient
{
    class Program
    {
        static void Main(string[] args)
        {
    
            //create socket 
            Socket updSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint dstEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8001);

            //send data
            while(true)
            {
                string content = Console.ReadLine();

                byte[] data = Encoding.ASCII.GetBytes(content);
                updSocket.SendTo(data, data.Length, SocketFlags.None, dstEndpoint);

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)sender;
                data = new byte[1024];
                recv = server.ReceiveFrom(data, ref Remote);
            }
        }
    }
}
