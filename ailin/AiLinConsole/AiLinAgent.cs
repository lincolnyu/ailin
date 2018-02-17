using AiLinConsole.ProxyManagement;
using AiLinConsole.QuestionManagement;
using System;
using System.Linq;
using System.Threading;
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
        
        public int[] VoteIds = 
        {
            VotePageNavigator.DefaultVoteId,
            VotePageNavigator.SecondVoteId
        };
        private VotePageNavigator _vpn;
        private bool _cancelledSync = false;

        public event VoteResultDelegate VoteResultReceived;

        public static T RunWithTimeout<T>(TimeSpan? timeout, 
            Func<T> func, VotePageNavigator cancellee)
        {
            if (timeout.HasValue)
            {
                T res = default(T);
                var t = new Thread(()=>
                {
                    res = func();
                });
                t.Start();
                if (!t.Join(timeout.Value))
                {
                    cancellee.CancelRefresh();
                    t.Join();
                }
                return res;
            }
            else
            {
                return func();
            }
        }

        public void Cancel()
        {
            if (_vpn != null)
            {
                _vpn.CancelRefresh();
            }
            _cancelledSync = true;
        }

        public void RunThruAllProxies(bool doNoProxyToo = false)
        {
            _cancelledSync = false;
            var proxies = doNoProxyToo ?
                Enumerable.Concat(new Proxy[] { null }, ProxyProvider) 
                : ProxyProvider;
            foreach (var proxy in proxies)
            {
                if (_cancelledSync) break;
                foreach (var voteId in VoteIds)
                {
                    if (_cancelledSync) break;
                    _vpn = new VotePageNavigator(voteId);
                    TimeSpan? timeout = null;
                    if (proxy != null)
                    {
                        _vpn.SetProxy(proxy.Address);
                        timeout = proxy.RecommendedTimeout;
                    }
                    var pi = RunWithTimeout(timeout, 
                        () => _vpn.SearchForZhuLin().Item1, _vpn);
                    if (_cancelledSync) break;
                    if (pi != null)
                    {
                        var q = pi.Question.Title;
                        var choices = pi.Question.Choices;
                        var sol = QuestionSolver.Solve(q, choices);
                        var s = VotePageNavigator.CreateSubmit(pi, sol);

                        var res = RunWithTimeout(timeout, ()=> _vpn.Submit(s), _vpn);
                        if (_cancelledSync) break;
                        var replydata = res.ConvertGB2312ToUTF();
                        var reply = replydata.GetVoteResponseMessage(true);
                        var replymsg = reply.Item1;
                        var successful = reply.Item2;
                        VoteResultReceived?.Invoke(voteId, proxy, successful, replymsg);
                    }
                    else
                    {
                        VoteResultReceived?.Invoke(voteId, proxy, false, "Network or parsing error");
                    }
                }
            }
            _vpn = null;
        }
    }
}
