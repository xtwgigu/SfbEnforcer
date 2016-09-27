using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
/*
            //get info from xml
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load("k:\\test\\confCreate.xml");

            XmlNamespaceManager xmlnsm = new XmlNamespaceManager(xmldoc.NameTable);
          //  xmlnsm.AddNamespace("xmlns","urn:ietf:params:xml:ns:cccp");
          //  xmlnsm.AddNamespace("myaddconference", "urn:ietf:params:xml:ns:cccp");
            xmlnsm.AddNamespace("mscp", "http://schemas.microsoft.com/rtc/2005/08/cccpextensions");
            xmlnsm.AddNamespace("ci", "urn:ietf:params:xml:ns:conference-info");
            xmlnsm.AddNamespace("msci" , "http://schemas.microsoft.com/rtc/2005/08/confinfoextensions");


            XmlNode confIDNode = xmldoc.DocumentElement.SelectSingleNode("//msci:conference-id", xmlnsm);

            string strID = confIDNode.InnerText;

*/
           
           
            //get info from xml
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load("k:\\test\\confCreateResonse.xml");

            xmldoc.DocumentElement.SetAttribute("xmlns", "");
            // must serialize and reload for this to take effect
            XmlDocument newDoc = new XmlDocument();
            newDoc.LoadXml(xmldoc.OuterXml);

            XmlNode confInfoNode = xmldoc.DocumentElement.SelectSingleNode("//conference-info");
            if (confInfoNode != null)
            {
                string strEntity = confInfoNode.Attributes["entity"].Value;

                int i = 0;
            }
          
/*
            //
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Users><user name=\"john.tyler\" level=\"3\"/><user name=\"abraham.lincoln\" level=\"2\"/><user name=\"jimmy.carter\" level=\"1\"/></Users>");

            XmlNodeList lstNode = xmlDoc.DocumentElement.SelectNodes("//user");
            foreach (XmlNode userNode in lstNode)
            {
                string strUserName = userNode.Attributes["name"].Value;
                string strUserLevel = userNode.Attributes["level"].Value;

                Console.WriteLine("User:{0}, Level:{1}", strUserName, strUserLevel);
            }
 * */
/*
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><MessageInfo type=\"endpointProxyInfo\"><Message><EndpointProxySipUri>abcd</EndpointProxySipUri></Message></MessageInfo>");

            //get sip name
            XmlNode nodeSipName = xmlDoc.DocumentElement.SelectSingleNode("//EndpointProxySipUri");
            if (nodeSipName != null)
            {
                string strName  = nodeSipName.InnerText;
                Console.WriteLine("EndpointProxy name:{0}", strName);
            }
            else
            {
                Console.WriteLine("nodeSipName is mepty");
            }
 * */

            int n = 0;
        }
    }
}
