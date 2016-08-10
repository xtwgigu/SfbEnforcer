using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SfbEnforcer
{
    class CUserTag
    {
        public string strUserName;
        public List<KeyValuePair<string, string>> tags;

        public CUserTag()
        {
            tags = new List<KeyValuePair<string, string>>();
        }
    }

    class CPolicy
    {
        protected List<CUserTag> m_lstUserTag;

        static protected CPolicy m_policy;

        static public CPolicy GetPolicy()
        {
            return m_policy;
        }
       
        static CPolicy()
        {
            m_policy = new CPolicy();
        }

        protected CPolicy() {
            m_lstUserTag = new List<CUserTag>();
            ReadUserTags();
        }
        protected CPolicy(CPolicy p) { }

        public void ReloadUserTags()
        {
            ReadUserTags();
        }

        protected void ReadUserTags()
        {
            m_lstUserTag.Clear();

            //
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("userlevel.xml");

            XmlNodeList lstNode = xmlDoc.DocumentElement.SelectNodes("//user");
            foreach(XmlNode userNode in lstNode)
            {
                string strUserName = userNode.Attributes["name"].Value;
                string strUserLevel = userNode.Attributes["level"].Value;

                Console.WriteLine("User:{0}, Level:{1}", strUserName, strUserLevel);

                CUserTag userTag = new CUserTag();
                userTag.strUserName = strUserName;
                userTag.tags.Add(new KeyValuePair<string,string>("level", strUserLevel));
                m_lstUserTag.Add(userTag);
            }
        }


        public bool CheckPolicy(string strConfTag, string strUser)
        {
            if(strConfTag.Length==0)
            {
                Console.WriteLine("check policy return false because conference tag is mepty");
                return false;
            }

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strConfTag);

                XmlNode xmlRoot = xmlDoc.DocumentElement;
                string strType = xmlRoot.Attributes["type"].Value;
                if(strType.Equals("tag", StringComparison.OrdinalIgnoreCase))
                {
                    //get user level
                    string strUserLevel = "";
                    foreach(CUserTag userTag in m_lstUserTag)
                    {
                        if(strUser.IndexOf(userTag.strUserName, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            foreach(KeyValuePair<string,string> tag in userTag.tags)
                            {
                                if(tag.Key.Equals("level", StringComparison.OrdinalIgnoreCase))
                                {
                                    strUserLevel = tag.Value;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    if(strUserLevel.Length==0)
                    {
                        Console.WriteLine("check policy return false because user:{0} level is empty", strUser);
                        return false;
                    }


                    //get conference level
                    string strConfLevel = "";
                    XmlNodeList lstTagNode = xmlRoot.SelectNodes("//Tag");
                    foreach(XmlNode tagNode in lstTagNode)
                    {
                        string strTagName = tagNode.Attributes["name"].Value;
                        if(strTagName.Equals("level", StringComparison.OrdinalIgnoreCase))
                        {
                            strConfLevel = tagNode.Attributes["value"].Value;
                            break;
                        } 
                    }

                    if(strConfLevel.Length == 0)
                    {
                        Console.WriteLine("check policy return false because conference level is empty");
                        return false;
                    }

                    //compare user level and conference level
                    int nConfLevel = int.Parse(strConfLevel);
                    int nUserLevel = int.Parse(strUserLevel);
                    bool bAllow = nUserLevel >= nConfLevel;
                    Console.WriteLine("check policy return {0}, because userLevel:{1}, conference level:{2}", bAllow.ToString(), nUserLevel, nConfLevel);
                    return bAllow;
                }
                else
                {
                    //strtype = error
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("check policy return false because exception happened");
                return false;
            }
          
            return false;
        }
    }

}
