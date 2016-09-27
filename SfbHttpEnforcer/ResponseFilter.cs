using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Web;

namespace SfbHttpEnforcer
{
    class ResponseFilter : Stream
    {
        public ResponseFilter(HttpResponse response)
        {
            m_oldFilter = response.Filter;
            m_streamContentBuf = new MemoryStream(1024);
            m_httpResponse = response;
        }

        protected Stream m_oldFilter = null;
        protected MemoryStream m_streamContentBuf;
        protected HttpResponse m_httpResponse;

      public override bool CanRead
        {
            get { return true; }
        }
       
        public override bool CanSeek
        {
            get { return true; }
        }
        public override bool CanWrite
        {
            get { return true; }
        }
        public override void Close()
        {
            //write content back to the old stream
            m_streamContentBuf.Position = 0;
            m_streamContentBuf.CopyTo(m_oldFilter);
            m_oldFilter.Flush();
            m_oldFilter.Close();

            //if it is a text content, we output it 
            try
            {
                Trace.WriteLine("ContentType:" + m_httpResponse.ContentType);
                Trace.WriteLine("ContentLen:" + m_httpResponse.Headers["Content-Length"]);
                if (m_httpResponse.ContentType.Contains("text"))
                {
                    /*
                    m_streamContentBuf.Position = 0;
                    StreamReader contentReader = new StreamReader(m_streamContentBuf);
                    String strContent = contentReader.ReadToEnd();
                   Trace.WriteLine(strContent); */
                    FileStream f = new FileStream("c:\\sfbEnforcer\\" + Guid.NewGuid().ToString() + ".txt", FileMode.CreateNew);
                    m_streamContentBuf.Position = 0;
                    m_streamContentBuf.CopyTo(f);
                    f.Flush();
                    f.Close();
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
    
           
        }
        public override void Flush()
        {
            m_streamContentBuf.Flush();
        }
        public override long Length
        {
            get { return 0; }
        }
        public override long Position
        {
            get { return m_streamContentBuf.Position; }
            set { m_streamContentBuf.Position = value; }
        }
        public override long Seek(long offset, System.IO.SeekOrigin direction)
        {
            return 0;
        }
        public override void SetLength(long length)
        {
            //do nothing
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            m_streamContentBuf.Write(buffer, offset, count);
        }



        //help method
        protected void WriteToOldStream(string strContent)
        {
            try
            {
                byte[] byteContent = System.Text.Encoding.UTF8.GetBytes(strContent);
                m_oldFilter.Write(byteContent, 0, byteContent.Length);
                m_oldFilter.Flush();
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

    }
}
