using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SfbEnforcer
{
    class Enforcer
    {
        static protected string m_strManifestFile = "SfbEnforcer.am";
        static protected CSessionManager m_sessionManager = new CSessionManager();
        static protected string m_strAppGuid = "{8ACC21C0-A021-475B-8F0C-47FE33744997}";

        static void Main(string[] args)
        {
            //only one instance


            //read config
           ContactToEndpointProxy.GetProxyContact().GetEndpointProxyInfo();


            //Connect to server
            try
            {
                m_sessionManager.ConnectToServer(m_strManifestFile, "SfbEnforcer", ref m_strAppGuid);
                DisplayLog("Connect server success.");
            }
            catch (Exception e)
            {
                ///we are unable to connect, print the exception in our UI, restore ///the button state
                DisplayException(e);
            }

            //disconnect
            while(true)
            {
                string strUserCommand = Console.ReadLine();
                if (strUserCommand.Equals("q", StringComparison.OrdinalIgnoreCase))
                {
                    DisplayLog("Disconnect to server.");
                    m_sessionManager.Disconnect();
                    break;
                }
                else if(strUserCommand.Equals("lc", StringComparison.OrdinalIgnoreCase))
                {
                    CPolicy.GetPolicy().ReloadUserTags();
                }
                else
                {
                    Console.WriteLine("unrecognize command.");
                }
            }
       

        }


        static void DisplayException(Exception e)
        {
            DisplayLog(e.ToString());
        }


        static void DisplayLog(string str)
        {
            Console.WriteLine(str);
        }
    }
}
