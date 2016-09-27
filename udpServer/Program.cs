using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace udpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //create socket and bind local port       
            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 8001);
            newsock.Bind(ip);

      
            //wait client 
            while(true)
            {
                //recv data
                EndPoint Remote = (EndPoint)( new IPEndPoint(IPAddress.Any, 0));

                 int recv;
                 byte[] data = new byte[1024];
                 recv = newsock.ReceiveFrom(data, ref Remote);
                 data[recv] = 0;
                 Console.WriteLine("Message received from {0}: {1} ", Remote.ToString(), Encoding.ASCII.GetString(data, 0, recv) );

                //reply
                newsock.SendTo(data, data.Length, SocketFlags.None, Remote);
            }
          
        }
    }
}
