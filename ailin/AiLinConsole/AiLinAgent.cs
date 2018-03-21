using AiLinConsole.ProxyManagement;
using AiLinConsole.QuestionManagement;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using WebKit;
using WebKit.Helpers;

namespace AiLinConsole
{
    class AiLinAgent
    {
        public delegate void SuppressVoteDelegate();
        public delegate void VoteStartedDelegate(int voteId, IProxy proxy, SuppressVoteDelegate suppress);
        public delegate void VoteResultDelegate(int voteId, IProxy proxy, 
            bool successful, bool solvedByUser, string replyMsg);

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

        public event VoteStartedDelegate VoteStarted;
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
            _cancelledSync = true;
            if (_vpn != null)
            {
                _vpn.CancelRefresh();
            }
        }

        public static bool IsIncorrect(string replyMsg)
            => replyMsg?.Contains("回答错误")?? false;

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
                    var suppressVote = false;
                    void suppress()
                    {
                        suppressVote = true;
                    }
                    VoteStarted?.Invoke(voteId, proxy, suppress);
                    if (suppressVote) continue;
                    _vpn = new VotePageNavigator(voteId);
                    TimeSpan? timeout = null;
                    if (proxy != null)
                    {
                        _vpn.ClearCookies();
                        _vpn.SetProxy(proxy.Address);
                        timeout = proxy.RecommendedTimeout;
                    }
                    try
                    {
                        var pi = RunWithTimeout(timeout,
                        () => _vpn.SearchForZhuLin().Item1, _vpn);
                        if (_cancelledSync) break;
                        if (pi != null)
                        {
                            bool incorrect;
                            do
                            {
                                var q = pi.Question.Title;
                                var choices = pi.Question.Choices;
                                var solres = QuestionSolver.Solve(q, choices);
                                if (_cancelledSync || solres == null) break;
                                var sol = solres.Item1;
                                if (sol >= 0)
                                {
                                    var s = VotePageNavigator.CreateSubmit(pi, sol);
                                    var res = RunWithTimeout(timeout, () => _vpn.Submit(s), _vpn);
                                    if (_cancelledSync) break;
                                    var replydata = res.ConvertGB2312ToUTF();
                                    var reply = replydata.GetVoteResponseMessage(true);
                                    var replymsg = reply.Item1;
                                    var successful = reply.Item2;
                                    incorrect = IsIncorrect(replymsg);
                                    solres.Item2?.Invoke(!incorrect);
                                    VoteResultReceived?.Invoke(voteId, proxy, successful,
                                        solres.Item2 != null, replymsg);
                                }
                                else
                                {
                                    incorrect = true;
                                    VoteResultReceived?.Invoke(voteId, proxy, false,
                                        true, "Invalid answer");
                                }
                            } while (incorrect);
                        }
                        else
                        {
                            VoteResultReceived?.Invoke(voteId, proxy, false, false, "Network or parsing error");
                            break;
                        }
                    }
                    catch (WebException)
                    {
                        VoteResultReceived?.Invoke(voteId, proxy, false, false, "Web exception encountered");
                    }
                }
            }
            _vpn = null;
        }
    }
}
