using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Rtc.Sip;

namespace SfbEnforcer
{
    public enum SIP_INVITE_TYPE
    {
        INVITE_UNKNOWN = 0,
        INVITE_IM_INVITE,   //invite to instance message
        INVITE_CONF_INVITE, //invite to a conference
        INVITE_CONF_ENTER,  //enter a conference
    };

    public enum SIP_SERVICE_TYPE
    {
        SERVICE_UNKNOWN = 0,
        SERVICE_CONFERENCE_CREATE, // Create conference
    };

    class CSIPTools
    {
        //SIP header name
        static public string SIP_HDR_CONTENTTYPE = "content-type";
        static public string SIP_HDR_CALLID = "call-id";
        static public string SIP_HDR_TO = "to";
        static public string SIP_HDR_FROM = "from";
        static public string SIP_HDR_CONVERSTATIONID = "Ms-Conversation-ID";
        static public string SIP_HDR_SEQ = "cseq";


         static public SIP_INVITE_TYPE GetInviteRequestType(Request request)
         {
             Header toHeader = request.AllHeaders.FindFirst(SIP_HDR_TO);
             if(!IsConferenceEndPoint(toHeader.Value))
             {
                 Header contentType = request.AllHeaders.FindFirst(SIP_HDR_CONTENTTYPE);
                 if(contentType != null)
                 {
                     if (contentType.Value.Equals("application/sdp"))
                     {
                         return SIP_INVITE_TYPE.INVITE_IM_INVITE;
                     }
                     else if (contentType.Value.Equals("application/ms-conf-invite+xml"))
                     {
                         return SIP_INVITE_TYPE.INVITE_CONF_INVITE;
                     }
                 }
             }
             else
             {
                 return SIP_INVITE_TYPE.INVITE_CONF_ENTER;
             }

             return SIP_INVITE_TYPE.INVITE_UNKNOWN;
          
         }

        static public SIP_SERVICE_TYPE GetServiceRequestType(Request request)
         {
            try
            {
                Header toHeader = request.AllHeaders.FindFirst(SIP_HDR_TO);
                if (toHeader.Value.Contains("app:conf:focusfactory"))
                {
                    Header contentType = request.AllHeaders.FindFirst(SIP_HDR_CONTENTTYPE);
                    if (contentType.Value.Equals("application/cccp+xml"))
                    {
                        return SIP_SERVICE_TYPE.SERVICE_CONFERENCE_CREATE;
                    }
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
     

            return SIP_SERVICE_TYPE.SERVICE_UNKNOWN;
         }

        static public bool IsConferenceEndPoint(string strEndpoint)
        {
            return strEndpoint.Contains("opaque=app:conf");
        }

        static public string GetConfIDFromConfEntry(string strConfEntry)
        {
            if (strConfEntry == null)
                return null;

            int nIndex = strConfEntry.LastIndexOf(':');
            if(nIndex != -1)
            {
                return strConfEntry.Substring(nIndex + 1);
            }

            return null;
        }
        static public string GetUriFromSipAddrHdr(string strHdr)
        {
            if (strHdr == null) 
                return null;

            string uri = strHdr;
            int index1 = strHdr.IndexOf('<');
            if (index1 != -1)
            {
                int index2 = strHdr.IndexOf('>');
                ///address, extract uri
                uri = strHdr.Substring(index1 + 1, index2 - index1 - 1);
                return uri;
            }

            return uri;
        }

        static public string GetUserAtHost(string sipAddressHeader)
        {
            if (sipAddressHeader == null) return null;

            string uri = null;

            /// If the header has < > present, then extract the uri
            /// else treat the input as uri
            int index1 = sipAddressHeader.IndexOf('<');

            if (index1 != -1)
            {
                int index2 = sipAddressHeader.IndexOf('>');
                ///address, extract uri
                uri = sipAddressHeader.Substring(index1 + 1, index2 - index1 - 1);
            }
            else
            {
                uri = sipAddressHeader;
            }

            ///chop off all parameters. we assume that there is no
            ///semicolon in the user part (which is allowed in some cases!)
            index1 = uri.IndexOf(';');
            if (index1 != -1)
            {
                uri = uri.Substring(0, index1 - 1);
            }

            ///we will process only SIP uri (thus no sips or tel)
            ///and wont accept those without user names
            if (uri.StartsWith("sip:") == false ||
                uri.IndexOf('@') == -1)
                return null;

            ///now we have sip:user@host most likely, with some exceptions that
            /// are ignored
            ///  1) user part contains semicolon separated user parameters
            ///  2) user part also has the password (as in sip:user:pwd@host)
            ///  3) some hex escaped characters are present in user part
            ///  4) the host part also has the port (Contact header for example)

            return uri.Substring("sip:".Length /* uri.Substring(4) */);
        }
    }
}
