using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using System.IO;

namespace SfbHttpEnforcer
{
    class CHttpParserBase
    {
        protected Stream m_oldFilterStream;

        public CHttpParserBase()
        {
            m_oldFilterStream = null;
        }

        public virtual void BeginRequest(Object source, EventArgs e)
        {
            try
            {
                HttpApplication application = (HttpApplication)source;
                HttpContext context = application.Context;
                string filePath = context.Request.FilePath;

                Trace.WriteLine("Enter ApplicationBeginRequest 2,method:" + context.Request.HttpMethod + " path:" + filePath);

                if (context.Request.HttpMethod.Equals("post", StringComparison.OrdinalIgnoreCase))
                {
                    if (context.Request.InputStream != null)
                    {
                        long lOldPosition = context.Request.InputStream.Position; //we must save the old position and reset it to this old value after we finished our process

                        StreamReader reader = new StreamReader(context.Request.InputStream);
                        string strContent = reader.ReadToEnd();

                        context.Request.InputStream.Position = lOldPosition;  //set old position

                        Trace.WriteLine("Post data:" + strContent);
                    }
                }

                //replace response filter
                ReplaceResponseFilter(context.Response);
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
   

        }

        public virtual void Reset(HttpApplication application)
        {
            //reset response filter
            if ((m_oldFilterStream != null) && (application.Context.Response.Filter != null))
            {
                application.Context.Response.Filter = m_oldFilterStream;
                m_oldFilterStream = null;
            }
        }

        protected virtual ResponseFilter CreateResponseFilter(HttpResponse httpResponse)
        {
            if(httpResponse.Filter!=null)
            {
                return new ResponseFilter(httpResponse);
            }
            else
            {
                return null;
            }
        }

        protected void ReplaceResponseFilter(HttpResponse httpResponse)
        {
            if(httpResponse.Filter!=null)
            {
                ResponseFilter newResponseFilter = CreateResponseFilter(httpResponse);
                m_oldFilterStream = httpResponse.Filter;
                httpResponse.Filter = newResponseFilter;
            }
        }
    }
}
