using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Configuration;
using System.Xml;

namespace SfbEnforcer
{
    class ContactToEndpointProxy
    {
        protected Socket m_serverSocket;
        protected IPEndPoint m_serverEndpoint;

        protected string m_strProxySipName;
        
        static protected ContactToEndpointProxy m_endpointContact;

        public string ProxySipName { get { return m_strProxySipName; } set { m_strProxySipName = value; } }

        public bool IsEndpointProxy(string strUser)
        {
           return (m_strProxySipName.IndexOf(strUser, StringComparison.OrdinalIgnoreCase)!=-1);
        }

        static ContactToEndpointProxy()
        {
            m_endpointContact = new ContactToEndpointProxy();
        }
        public static ContactToEndpointProxy GetProxyContact()
        {
            return m_endpointContact;
        }

        protected ContactToEndpointProxy() 
        {
            //create socket 
            m_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            string strProxyIP = ConfigurationManager.AppSettings["EndpointProxyIP"];
            if(strProxyIP.Length==0)
            {
                strProxyIP = "127.0.0.1";
            }

            int nPort = 8001;
            string strProxyPort = ConfigurationManager.AppSettings["EndpointProxyPort"];
            if(strProxyPort.Length>0)
            {
                nPort = int.Parse(strProxyPort);
            }

            Console.WriteLine("EndpointProxy IP={0}, port={1}", strProxyIP, nPort);

            m_serverEndpoint = new IPEndPoint(IPAddress.Parse(strProxyIP), nPort);
        }
        protected ContactToEndpointProxy(ContactToEndpointProxy con) { }

        protected void SendMessage(string strMsg)
        {
            byte[] data = Encoding.ASCII.GetBytes(strMsg);
            m_serverSocket.SendTo(data, data.Length, SocketFlags.None, m_serverEndpoint);

            Trace.WriteLine("SendMessage:" + strMsg);
        }

        protected string RecvData()
        {
            byte[] data = new byte[2048];
            Trace.WriteLine("RecvData: begin");

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)sender;
            int nRecv = m_serverSocket.ReceiveFrom(data, ref Remote);
            string strRecv = Encoding.ASCII.GetString(data, 0, nRecv);

            Trace.WriteLine("RecvData:" + strRecv);

            return strRecv;
        }

        public void SendConfTaggingRequest(string strConfUri)
        {
           // Console.WriteLine("SendConfTaggingRequest.");
            string strRequest= string.Format("<?xml version=\"1.0\" encoding=\"utf-8\" ?><MessageInfo type=\"setTag\"><DesSipUri>{0}</DesSipUri><QueryPolicy></QueryPolicy></MessageInfo>", strConfUri);
            SendMessage(strRequest);
        }

        public void SendMessageToUser(string strToUser, string strHeader, string strBody)
        {   
            string strRequest = string.Format("<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + 
                                             "<MessageInfo type=\"notify\">" +
                                             "<DesSipUri>{0}</DesSipUri>" +
                                             "<Message><ToastMessage></ToastMessage><Header>{1}</Header>" +
                                              "<Body>{2}</Body></Message></MessageInfo>", strToUser, strHeader, strBody);
            SendMessage(strRequest);
        }
        public string GetConfTagging(string strConfFocusUri)
        {
            string strRequest = string.Format("<?xml version=\"1.0\" encoding=\"utf-8\" ?><MessageInfo type=\"getTag\"><DesSipUri>{0}</DesSipUri></MessageInfo>", strConfFocusUri);
            SendMessage(strRequest);

            string strTag = RecvData();
            return strTag;
        }

        public void GetEndpointProxyInfo()
        {
            string strRequest = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><MessageInfo type=\"getEndpointProxyInfo\"></MessageInfo>";
            SendMessage(strRequest);

            string strProxyData = RecvData();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strProxyData);

                //get sip name
                XmlNode nodeSipName = xmlDoc.DocumentElement.SelectSingleNode("//EndpointProxySipUri");
                if(nodeSipName!=null)
                {
                    m_strProxySipName = nodeSipName.InnerText;
                    Console.WriteLine("EndpointProxy name:{0}", m_strProxySipName);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

       


    }
}
