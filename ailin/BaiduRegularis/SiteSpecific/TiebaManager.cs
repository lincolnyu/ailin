using System;
using System.Collections.Generic;

namespace Regularis.SiteSpecific
{
    class TiebaManager
    {
        public class PostNode
        {
            // TODO implementation
        }

        public class BaiduCookie
        {
            public string BaiduId { get; set; }
            public string TiebaUserType { get; set; }
            public string Bduss { get; set; }
            public string TiebaUid { get; set; }
            public string BdshareFirstime { get; set; }
            public int RplnGuide { get; set; } = 1;
            public int WiseDevice { get; set; } = 0;

            public override string ToString()
                => $"BAIDUID={BaiduId}; TIEBA_USERTYPE={TiebaUserType}; BDUSS={Bduss}; TIEABUID={TiebaUid}; bdshare_firstime={BdshareFirstime}; rpln_guide={RplnGuide}; wise_device={WiseDevice}";

            public void FromString(string cookie)
            {
                throw new NotImplementedException();
            }
        }

        public string UserName { get; }
        public string Password { get; }
        
        public BaiduCookie Cookie { get; }

        public TiebaManager(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public void LogIn()
        {
            throw new NotImplementedException();
        }

        public void LogOut()
        {
            throw new NotImplementedException();
        }

        #region Node content

        public void Post(PostNode node, string content)
        {
            throw new NotImplementedException();
        }

        public string Retrieve(PostNode node)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Node navigation

        public PostNode GotoBar(string barName)
        {
            throw new NotImplementedException();
        }

        public PostNode GetNextSibling(PostNode node)
        {
            throw new NotImplementedException();
        }

        public PostNode GetPreviousSibling(PostNode node)
        {
            throw new NotImplementedException();
        }

        public PostNode GetParent(PostNode node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PostNode> GetChildren(PostNode node)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
