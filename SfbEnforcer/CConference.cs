using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SfbEnforcer
{
    class CConference
    {
        public CIMCall PreIMCall { get { return m_preIMCall; } set { m_preIMCall = value; } }
        public string Creator { get { return m_strCreator; } set { m_strCreator = value; } }
        public string CreateCallID { get { return m_strCreateCallID; } set { m_strCreateCallID = value; } }
        public string FocusUri { get { return m_strFocusUri; } set { m_strFocusUri = value; } }

        protected string m_strFocusUri;
        protected string m_strCreator;


        protected CIMCall m_preIMCall;
        protected string m_strCreateCallID;

        public CConference(string strCreator, string strConfFocusUri)
        {
            m_strCreator = strCreator;
            m_strFocusUri = strConfFocusUri;
            m_preIMCall = null;
        }

        public bool IsCreator(string strUser)
        {
            if (strUser == null)
                return false;

            return m_strCreator.Equals(strUser, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsConvertedFromIMCall()
        {
            return (m_preIMCall != null);
        }

        public bool IsTheOldSpeaker(string strUser)
        {
            if((strUser==null) || (m_preIMCall==null))
            {
                return false;
            }
            else
            {
                return m_preIMCall.From.Equals(strUser, StringComparison.OrdinalIgnoreCase) ||
                       m_preIMCall.To.Equals(strUser, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    class CConferenceManager
    {
         protected List<CConference> m_lstConference;
         static protected CConferenceManager m_conferenceMgr;

         protected CConferenceManager(CConferenceManager mgr) { }

        protected CConferenceManager()
        {
            m_lstConference = new List<CConference>();
        }
        static CConferenceManager()
        {
            CConferenceManager.m_conferenceMgr = new CConferenceManager();
        }

        static public CConferenceManager GetConferenceManager()
        {
            return m_conferenceMgr;
        }

        public void AddConference(CConference conference)
        {
            lock (this)
            {
                CConference confExist = GetConferenceByFocusUri(conference.FocusUri);
                if (confExist == null)
                {
                    m_lstConference.Add(conference);
                }
            }
        }

        public void RemoveConference(CConference conference)
        {
            lock (this)
            {
                m_lstConference.Remove(conference);
            }
        }

        public CConference GetConferenceByCreateCallID(string strCallID)
        {
            lock (this)
            {
                foreach (CConference conference in m_lstConference)
                {
                    if (conference.CreateCallID.Equals(strCallID, StringComparison.OrdinalIgnoreCase))
                    {
                        return conference;
                    }
                }

                return null;
            }
        }

        public CConference GetConferenceByFocusUri(string strFocusUri)
        {
            lock (this)
            {
                foreach (CConference conference in m_lstConference)
                {
                    if (conference.FocusUri.Equals(strFocusUri, StringComparison.OrdinalIgnoreCase))
                    {
                        return conference;
                    }
                }

                return null;
            }
        }


    }
}
