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
    class ResponseFilterPersistentChat : ResponseFilter
    {
        public ResponseFilterPersistentChat(HttpResponse response)
            : base(response)
        {
        }

        public override void Close()
        {
            try
            {
                //get string content
                m_streamContentBuf.Position = 0;
                StreamReader contentReader = new StreamReader(m_streamContentBuf);
                String strContent = contentReader.ReadToEnd();
                Trace.WriteLine(strContent);

                //added customer tags ui
                int nPos = strContent.IndexOf("Delete this chat room </span>", StringComparison.OrdinalIgnoreCase);
                if (nPos != -1)
                {
                    int nLineEndPos = strContent.IndexOf("</tr>", nPos);
                    if (nLineEndPos != -1)
                    {
                        strContent = strContent.Insert(nLineEndPos + 5, "<tr><td class=\"RoomBtItemSpacing\"><input id=\"cbNxlControlRoom\" type=\"checkbox\" onclick=\"mainWnd.SetNxlControlRoom();\" /><span id=\"lblNxlControlRoom\" class=\"RoomDisableText\"> Set this room controlled by Nextlabs SFB Enforcer</span></td></tr><tr><td class=\"RoomBtItemSpacing\"><span id=\"lblNxlControlRoom\" class=\"RoomDisableText\">Select the Level of this Room:</span><select><option style=\"width:200px\">Low</option><option style=\"width:200px\">Middle</option><option style=\"width:200px\">High</option></select></td></tr>");
                    }
                }

                //add javascript
                nPos = strContent.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
                if(nPos!=-1)
                {
                    nPos = strContent.LastIndexOf("</script>", nPos, StringComparison.OrdinalIgnoreCase);
                    if(nPos!=-1)
                    {
                        strContent = strContent.Insert(nPos + "</script>".Length, "\r\n<script src=\"JScripts/NLRoomForm.js\" type=\"text/javascript\"></script>");
                        Trace.WriteLine("Jscript updated");
                    }
                }


                //modified the onclick event on "Create button"
                strContent = strContent.Replace("mainWnd.CreateRoom(); return false;", "mainWnd.NLCreateRoom(); return false;");

                //write back
                WriteToOldStream(strContent);
                m_oldFilter.Close();
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
      
        }
    }
}
