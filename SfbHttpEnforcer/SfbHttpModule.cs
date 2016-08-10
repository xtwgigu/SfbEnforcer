using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using System.IO;

//<modules><add name="SfbHttpModule" type="SfbHttpEnforcer.SfbHttpModule"/></modules>

namespace SfbHttpEnforcer
{
    public class SfbHttpModule : IHttpModule
    {
        protected bool m_bInited = false;

        private CHttpParserBase m_httpParser;

        public SfbHttpModule()
        {
            Trace.WriteLine("Enter SfbHttpModule construct 0");
            m_bInited = false;
            m_httpParser = null;
        }

        public String ModuleName
        {
            get { Trace.WriteLine("Enter ModuleName"); return "SfbHttpModule"; }
        }

        // In the Init function, register for HttpApplication 
        // events by adding your handlers.
        public void Init(HttpApplication application)
        {
            if (!m_bInited)
            {
                application.BeginRequest += (new EventHandler(this.ApplicationBeginRequest));
                application.EndRequest += (new EventHandler(this.ApplicationEndRequest));
                m_bInited = true;
            }
        }

        private void ApplicationBeginRequest(Object source, EventArgs e)
        {
            try
            {
                HttpApplication application = (HttpApplication)source;
                HttpContext context = application.Context;

                CHttpParserBase httpParser = GetHttpParser(context);
                if (httpParser != null)
                {
                    httpParser.BeginRequest(source, e);
                }

            }
            catch(Exception ex)
            {
                Trace.WriteLine("exception on begin request:" + ex.ToString());
            }
            

        }

        private void ApplicationEndRequest(Object source, EventArgs e)
        {
            Trace.WriteLine("Enter ApplicationEndRequest");

            //clear state, one SfbHttpModule object may process more than one HTTP request, so when end process to the request, we reset the state of this object
            if (m_httpParser != null)
            {
                HttpApplication application = (HttpApplication)source;
                m_httpParser.Reset(application);
                m_httpParser = null;
            }
        }

        public void Dispose() 
        {
        }

        //
        private CHttpParserBase GetHttpParser(HttpContext httpContext)
        {
            if(m_httpParser!=null)
            {
                return m_httpParser;
            }
            else
            {
                HttpRequest request = httpContext.Request;
                if (request.FilePath.Equals("/PersistentChat/RM/default.aspx", StringComparison.OrdinalIgnoreCase))
                {
                    m_httpParser = new CHttpParserPersistentChat();
                }
                else
                {
                    m_httpParser = new CHttpParserBase();
                }
            }

            return m_httpParser;
        }

    }
}
