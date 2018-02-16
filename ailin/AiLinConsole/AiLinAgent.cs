using AiLinConsole.ProxyManagement;
using AiLinConsole.QuestionManagement;
using System.Linq;
using WebKit;
using WebKit.Helpers;

namespace AiLinConsole
{
    class AiLinAgent
    {
        public delegate void VoteResultDelegate(int voteId, IProxy proxy, bool successful, string replyMsg);

        public IProxyProvider ProxyProvider { get; }
        public IQuestionSolver QuestionSolver { get; }

        public AiLinAgent(IProxyProvider proxyProvider, IQuestionSolver questionSolver)
        {
            ProxyProvider = proxyProvider;
            QuestionSolver = questionSolver;
        }
        
        public int[] VoteIds = {
            VotePageNavigator.DefaultVoteId,
            VotePageNavigator.SecondVoteId
        };

        public event VoteResultDelegate VoteResultReceived;

        public void RunThruAllProxies(bool doNoProxyToo = false)
        {
            var proxies = doNoProxyToo ?
                Enumerable.Concat(new Proxy[] { null }, ProxyProvider) 
                : ProxyProvider;
            foreach (var proxy in proxies)
            {
                foreach (var voteId in VoteIds)
                {
                    var vpn = new VotePageNavigator(voteId);
                    if (proxy != null)
                    {
                        vpn.SetProxy(proxy.Address);
                    }
                    var pi = vpn.SearchForZhuLin().Item1;
                    var q = pi.Question.Title;
                    var choices = pi.Question.Choices;
                    var sol = QuestionSolver.Solve(q, choices);
                    var s = VotePageNavigator.CreateSubmit(pi, sol);
                    var res = vpn.Submit(s);
                    var replydata = res.ConvertGB2312ToUTF();
                    var reply = replydata.GetVoteResponseMessage(true);
                    var replymsg = reply.Item1;
                    var successful = reply.Item2;
                    VoteResultReceived?.Invoke(voteId, proxy, successful, replymsg);
                }
            }
        }
    }
}
