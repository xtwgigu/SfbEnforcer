using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SfbEnforcer
{
    class CChatRoom
    {
        public string Name { get { return m_strName; } }
        public string CreatorUri{get {return m_strCreatorUri;}}
        public string Uri{ get{return m_strUri;}}
        public string ID{ get{return m_strID;}}
        public string Description { get { return m_strDesc; } }

        protected string m_strName;
        protected string m_strCreatorUri;
        protected string m_strUri;
        protected string m_strID;
        protected string m_strDesc;

        public CChatRoom(string strName, string strUri, string strCreatorUri, string strRoomDesc="")
        {
            m_strName = strName;
            m_strUri = strUri;
            m_strCreatorUri = strCreatorUri;
            m_strDesc = strRoomDesc;

            //get id from Uri;
            int nIndex = strUri.LastIndexOf('/');
            if(nIndex!=-1)
            {
                m_strID = strUri.Substring(nIndex + 1);
            }
        }

        public bool IsCreator(string strName)
        {
            return m_strCreatorUri.Equals(strName, StringComparison.OrdinalIgnoreCase);
        }
    }

    class CChatRoomManager
    {
        protected List<CChatRoom> m_lstChatRoom;
        static protected CChatRoomManager m_chatRoomManager;
        static CChatRoomManager()
        {
            m_chatRoomManager = new CChatRoomManager();
        }

        static public CChatRoomManager GetChatRoomManager()
        {
            return m_chatRoomManager;
        }

        protected CChatRoomManager()
        {
            m_lstChatRoom = new List<CChatRoom>();
        }

        public void AddChatRoom(CChatRoom room)
        {
            lock(this)
            {
                //check room exist
                if(GetChatRoomByUri(room.Uri)==null)
                {
                    m_lstChatRoom.Add(room);
                }
            }
        }

        public CChatRoom GetChatRoomByUri(string strRoomUri)
        {
            lock (this)
            {
                foreach (CChatRoom room in m_lstChatRoom)
                {
                    if (room.Uri.Equals(strRoomUri, StringComparison.OrdinalIgnoreCase))
                    {
                        return room;
                    }
                }
            }
            return null;
        }

        public CChatRoom GetChatRoomByID(string strRoomID)
        {
            lock(this)
            {
                foreach(CChatRoom room in m_lstChatRoom)
                {
                    if(room.ID.Equals(strRoomID))
                    {
                        return room;
                    }
                }
            }

            return null;
        }


    }
}
