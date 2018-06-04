using Regularis.SiteSpecific;
using static Regularis.SiteSpecific.TiebaManager;

namespace Regularis
{
    class RegularLogger
    {
        const string DefaultPresenceMesage = "到";

        public TiebaManager Manager { get; } 

        public void Run()
        {

        }

        private void ReportVote()
        {

        }

        private void ReportPresence(PostNode node, string content = DefaultPresenceMesage)
        {
            Manager.Post(node, content);
        }

        private void PlayIdiomGame(PostNode postNode)
        {

        }


    }
}
