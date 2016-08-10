using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Rtc.Sip;
using System.Xml;
using System.Diagnostics;
using System.Configuration;

namespace SfbEnforcer
{
    class CSIPParser
    {
        static protected CSIPParser m_sipParser;
        static CSIPParser()
        {
            m_sipParser = new CSIPParser();
        }

        static public CSIPParser GetParser()
        {
            return m_sipParser;
        }

        protected CSIPParser() { }
        protected CSIPParser(CSIPParser parser){}


        public void ParseSIPRequest(Request request, ref Response ourResponse)
        {
           if (request.StandardMethod == Request.StandardMethodType.Invite)
            {
                SIP_INVITE_TYPE emInviteType = CSIPTools.GetInviteRequestType(request);
                if (emInviteType == SIP_INVITE_TYPE.INVITE_IM_INVITE)
                {
                    ParseIMCallInviteRequest(request);
                }
                else if (emInviteType == SIP_INVITE_TYPE.INVITE_CONF_INVITE)
                {
                    ParseConferenceInviteRequest(request,ref ourResponse);
                }
                else if (emInviteType == SIP_INVITE_TYPE.INVITE_CONF_ENTER)
                {
                    ParseConferenceEnterRequest(request, ref ourResponse);
                }
            }
            else if (request.StandardMethod == Request.StandardMethodType.Service)
            {
                SIP_SERVICE_TYPE emServiceType = CSIPTools.GetServiceRequestType(request);
                if (emServiceType == SIP_SERVICE_TYPE.SERVICE_CONFERENCE_CREATE)
                {
                   //ParseConferenceCreateRequest(request); we get the conference create action on the response
                }
            }
           else if(request.StandardMethod == Request.StandardMethodType.Info)
           {
              ParseInfoSipRequest(request, ref ourResponse);
           }
//            else if(request.StandardMethod == Request.StandardMethodType.Message)
//            {
//                string strMsSender = CSIPTools.GetUserAtHost(request.AllHeaders.FindFirst("Ms-Sender").Value);
//                bool bIsEndProxy = ContactToEndpointProxy.GetProxyContact().IsEndpointProxy(strMsSender);
// 
// 
//            }
        }

        public void ParseSIPResponse(Response sipResponse)
        {   
            try
            {
                Header ContentTypeHdr = sipResponse.AllHeaders.FindFirst(CSIPTools.SIP_HDR_CONTENTTYPE);
                if(ContentTypeHdr!=null)
                {
                    string strContentType = ContentTypeHdr.Value;
                    if (strContentType.Equals("application/cccp+xml", StringComparison.OrdinalIgnoreCase))
                    {
                        ParseCCCPResponse(sipResponse);
                    }
                }
       
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }



        }


        public void ParseCCCPResponse(Response sipResponse)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(sipResponse.Content);
            XmlNode rootNode = xmldoc.DocumentElement;
            if (rootNode == null)
                return;

            XmlNode firstChild = rootNode.FirstChild;
            if(firstChild != null)
            {
                if (firstChild.Name.Equals("addConference", StringComparison.OrdinalIgnoreCase))
                {
                    ParseConferenceCreateResponse(sipResponse, xmldoc);
                }
                else if(firstChild.Name.Equals("getConferences", StringComparison.OrdinalIgnoreCase))
                {
                    ParseConferenceGetResponse(sipResponse, xmldoc);
                }
            }
        }

        public void ParseConferenceGetResponse(Response sipResponse, XmlDocument xmlResponse)
        {
            //get info from xml
            try
            {
                XmlNode confInfoNode = xmlResponse.DocumentElement.FirstChild.FirstChild.FirstChild;
                if (confInfoNode != null)
                {
                    string strFocusUri = confInfoNode.Attributes["entity"].Value;
                    string strFromUser = CSIPTools.GetUserAtHost(sipResponse.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);

                    CConference conf = new CConference(strFromUser, strFocusUri);
                    CConferenceManager.GetConferenceManager().AddConference(conf);

                    Console.WriteLine("Get static conference, Creator={0},confEntry={1}", conf.Creator, conf.FocusUri);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            } 
        }

        public void ParseConferenceCreateResponse(Response sipResponse, XmlDocument xmlResponse)
        {
            //get info from xml
            try
            {
                XmlNode confInfoNode = xmlResponse.DocumentElement.FirstChild.FirstChild;
                if (confInfoNode != null)
                {
                    string strFocusUri = confInfoNode.Attributes["entity"].Value;
                    string strFromUser = CSIPTools.GetUserAtHost(sipResponse.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);

                    CConference conf = new CConference(strFromUser, strFocusUri);
                    CConferenceManager.GetConferenceManager().AddConference(conf);

                    Console.WriteLine("Conference Create, Creator={0},confEntry={1}", conf.Creator,  conf.FocusUri);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void ParseIMCallInviteRequest(Request sipRequest)
        {
            string strCallID = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_CALLID).Value;
            string strFromUser = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);
            string strTo = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_TO).Value);
            string strConverID = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_CONVERSTATIONID).Value;

            CIMCall imCall = new CIMCall(strFromUser, strTo, strCallID, strConverID);
            CIMCallManager.GetIMCallManager().AddIMCall(imCall);
        }


        public void ParseConferenceCreateRequest(Request sipRequest)
        { 
            CConference conf = GetConferenceFromRequest(sipRequest);
            if(conf != null)
            {
                CConferenceManager.GetConferenceManager().AddConference(conf);
            }

        }

        protected CConference GetConferenceFromRequest(Request sipRequest)
        {
            string strFromUser = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);
            string strConfID="";
            //get info from xml
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(sipRequest.Content);

            XmlNamespaceManager xmlnsm = new XmlNamespaceManager(xmldoc.NameTable);
            xmlnsm.AddNamespace("myaddconference", "urn:ietf:params:xml:ns:cccp");
            xmlnsm.AddNamespace("mscp", "http://schemas.microsoft.com/rtc/2005/08/cccpextensions");
            xmlnsm.AddNamespace("ci", "urn:ietf:params:xml:ns:conference-info");
            xmlnsm.AddNamespace("msci", "http://schemas.microsoft.com/rtc/2005/08/confinfoextensions");

            XmlNode confIDNode = xmldoc.DocumentElement.SelectSingleNode("//msci:conference-id", xmlnsm);
            if(confIDNode!=null)
            {
                strConfID = confIDNode.InnerText;
            }


            //
            if(strConfID.Length>0)
            {
                CConference conf = new CConference(strFromUser, strConfID);
                string strCallID = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_CALLID).Value;
                conf.CreateCallID = strCallID;
                return conf;
            }

            return null;
        }

        protected void ParseConferenceInviteRequest(Request sipRequest, ref Response ourResponse)
        {
            //get conference information from request
            string strConfUri="";
            string strConverSationID="";
            GetConfInviteRequestInfo(sipRequest, ref strConfUri, ref strConverSationID);

            //get conference
            CConference conf = CConferenceManager.GetConferenceManager().GetConferenceByFocusUri(strConfUri);
            if(conf != null)
            {
                //if this Invited request based on the previous IM conversation
                if(conf.PreIMCall==null)
                {
                    CIMCall imCall = CIMCallManager.GetIMCallManager().GetIMCallByConversationID(strConverSationID);
                    if (imCall != null)
                    {
                        conf.PreIMCall = imCall;
                    }
                }
            

                //
                string strToUser = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_TO).Value);
                string strFromUser = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);

                if (ContactToEndpointProxy.GetProxyContact().IsEndpointProxy(strToUser))
                {
                    Console.WriteLine("Invite EndpointProxy, Auto allowed", conf.FocusUri);
                }
                else if(conf.IsTheOldSpeaker(strToUser))
                {//Invite the user that in the previous conversation
                    Console.WriteLine("Invite Old User, Auto allowed.");

                }
                else 
                {
                    Console.WriteLine("Invite other user, Need check policy");

                    string strConfTag = ContactToEndpointProxy.GetProxyContact().GetConfTagging(strConfUri);
                    bool bAllow = CPolicy.GetPolicy().CheckPolicy(strConfTag, strToUser);
                    if(!bAllow)
                    {
                        Console.WriteLine("Deny Invite:{0}", strToUser);

                        string strErrorCode = ConfigurationManager.AppSettings["ErrorCodeDenyInvite"];
                         ourResponse = sipRequest.CreateResponse(int.Parse(strErrorCode));
                        ourResponse.AllHeaders.Add(new Header("warning", "399 lcs.microsoft.com \"invite not allowed by policy\""));
                       
                        //send notify message
                       ContactToEndpointProxy.GetProxyContact().SendMessageToUser("sip:" + strFromUser, "Deny Invite", "Your invite to " + strToUser + " is denied by SFB Enforcer");
                    }
                    else
                    {
                         Console.WriteLine("Allow Invite:{0}", strToUser);
                    }
                }
            }


        }

        protected void GetConfInviteRequestInfo(Request sipRequest, ref string strConfUri, ref string strConverationID)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(sipRequest.Content);

            XmlNode confUriNode = xmldoc.DocumentElement.SelectSingleNode("//focus-uri");
            if(confUriNode != null)
            {
                strConfUri = confUriNode.InnerText;
            }

            XmlNode confConverationID = xmldoc.DocumentElement.SelectSingleNode("//conversation-id");
            if(confConverationID != null)
            {
                strConverationID = confConverationID.InnerText;
            }
        }

        protected void ParseConferenceEnterRequest(Request sipRequest, ref Response ourResponse)
        {
            string strConfEntry = CSIPTools.GetUriFromSipAddrHdr(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_TO).Value);
            CConference conf = CConferenceManager.GetConferenceManager().GetConferenceByFocusUri(strConfEntry);

            if(conf != null)
            {
                string strFromHdr = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value;
                string strFromUser = CSIPTools.GetUserAtHost(strFromHdr);

                if (ContactToEndpointProxy.GetProxyContact().IsEndpointProxy(strFromUser))
                {
                    Console.WriteLine("Auto allowed EndpointProxy Enter conference");
                }
                else if(conf.IsCreator(strFromUser))
                {//the creator enter the conference
                    Console.WriteLine("Auto allowed the Creator:{0} Enter conference", strFromUser);
                   // if (!conf.IsConvertedFromIMCall())
                    {
                     //   Console.WriteLine("Send tag request when creator enter.");
                        ContactToEndpointProxy.GetProxyContact().SendConfTaggingRequest(strConfEntry);
                    }
                }
                else if(conf.IsTheOldSpeaker(strFromUser))
                {//old speaker enter the conference
                    Console.WriteLine("Auto allowed the old speaker:{0} Enter conference", strFromUser);
                }
                else
                {//other user enter the conference
                  Console.WriteLine("other user Enter conference:{0}, need check policy", strFromUser );

                  //check 
                  string strConfTag = ContactToEndpointProxy.GetProxyContact().GetConfTagging(strConfEntry);
                  bool bAllow = CPolicy.GetPolicy().CheckPolicy(strConfTag, strFromUser);
                  if (!bAllow)
                  {
                      Console.WriteLine("Deny Enter meeting:{0}", strFromUser);
                      string strErrorCode = ConfigurationManager.AppSettings["ErrorCodeDenyInvite"];
                      ourResponse = sipRequest.CreateResponse(int.Parse(strErrorCode));

                      //send notify message
                      ContactToEndpointProxy.GetProxyContact().SendMessageToUser("sip:" + strFromUser, "Deny Enter Meeting", "Your entry to the meeting is denied by SFB Enforcer");
                  }
                  else
                  {
                      Console.WriteLine("Allow Enter meeting:{0}", strFromUser);
                  }

                }
            }
        }

        protected void ParseInfoSipRequest(Request sipRequest, ref Response ourResponse)
        {
            try
            {
                string strContentLength = sipRequest.AllHeaders.FindFirst("content-length").Value;
                int nContentLen = int.Parse(strContentLength);
                string strContentType = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_CONTENTTYPE).Value;

                if ((nContentLen > 10) && strContentType.Equals("text/plain"))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(sipRequest.Content);
                    string strInfoType = xmlDoc.DocumentElement.Name;
                    if (strInfoType.Equals("xccos", StringComparison.OrdinalIgnoreCase))
                    {
                        ParsePersistentChatRoomInfoRequest(sipRequest, ref ourResponse);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        protected void ParsePersistentChatRoomInfoRequest(Request sipRequest, ref Response ourResponse)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sipRequest.Content);
                XmlNode firstChildNode = xmlDoc.DocumentElement.FirstChild;
                if (firstChildNode == null)
                {
                    return;
                }


                if (firstChildNode.Name.Equals("cmd", StringComparison.OrdinalIgnoreCase))
                {
                    string cmdType = firstChildNode.Attributes["id"].Value;
                    if (cmdType.Equals("cmd:join", StringComparison.OrdinalIgnoreCase))
                    {//join chat room

                        string strRoomID = "";
                        XmlNode nodeChanid = firstChildNode.FirstChild.FirstChild;
                        if(nodeChanid!=null)
                        {
                            strRoomID = nodeChanid.Attributes["value"].Value;
                        }

                        CChatRoom chatRoom = CChatRoomManager.GetChatRoomManager().GetChatRoomByID(strRoomID);
                        if(chatRoom!=null)
                        {
                            string strFromUser = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);

                            if(!chatRoom.IsCreator(strFromUser))
                            {
                                Console.WriteLine("Deny {0} join chat room:{1}", strFromUser, chatRoom.Name);
                                ourResponse = sipRequest.CreateResponse(405);


                            }
                            else
                            {
                                Console.WriteLine("Allow {0} join chat room:{1}", strFromUser, chatRoom.Name);
                            }
                        }

                    }
                }
                if (firstChildNode.Name.Equals("rpl", StringComparison.OrdinalIgnoreCase))
                {
                    string cmdType = firstChildNode.Attributes["id"].Value;
                    if (cmdType.Equals("rpl:chancreate", StringComparison.OrdinalIgnoreCase))
                    {//create chat room
                        ParsePersistentChatRoomCreateResult(sipRequest, xmlDoc);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        protected void ParsePersistentChatRoomCreateResult(Request sipRequest, XmlDocument xmlContent)
        {
            Console.WriteLine("Enter ParsePersistentChatRoomCreateResult");
            try
            {
                XmlNode xmlRoot = xmlContent.DocumentElement;

                //get chat room information from result
                XmlNode nodeResult = xmlRoot.FirstChild.FirstChild.NextSibling;
                if((nodeResult!=null) && (nodeResult.Attributes["code"].Value.Equals("200")))
                {
                    XmlNode nodeData = nodeResult.NextSibling;
                    if(nodeData!=null && nodeData.Name.Equals("data"))
                    {
                        string strRoomCreator = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_TO).Value);

                        string strRoomName="", strRoomDesc="", strRoomUri="";
                        XmlNode nodeChanib = nodeData.FirstChild;
                        if(nodeChanib!=null)
                        {
                            strRoomName = nodeChanib.Attributes["name"].Value;
                            strRoomDesc = nodeChanib.Attributes["description"].Value;
                            strRoomUri = nodeChanib.Attributes["uri"].Value;
                        }

                        CChatRoom room = new CChatRoom(strRoomName, strRoomUri, strRoomCreator, strRoomDesc);
                        CChatRoomManager.GetChatRoomManager().AddChatRoom(room);

                        Console.WriteLine("Chat room created, Creator:{0}, Name:{1}, Uri:{2}", strRoomCreator, strRoomName, strRoomUri);
                    }
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
