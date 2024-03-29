﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WebKit;
using WebKit.Helpers;

namespace TestConsole
{
    class Program
    {
        static void TestDesktop(int voteid = VotePageNavigator.DefaultVoteId)
        {
            const string qf = @"d:\temp\question.txt";
            const string submit = @"d:\temp\submit.txt";
            const string reply = @"d:\temp\reply.html";

            var vpn = new VotePageNavigator(voteid);
            var pi = vpn.SearchForZhuLin().Item1;

            using (var sw = new StreamWriter(qf))
            {
                sw.WriteLine("链接地址：{0}", pi.PageUrl);
                sw.WriteLine("明星ID：{0}", pi.Id);
                sw.WriteLine("已得选票：{0}", pi.Votes);
                sw.WriteLine("人气：{0}", pi.Popularity);
                sw.WriteLine();
                sw.WriteLine("问题: {0}", pi.Question.Title);
                var i = 0;
                foreach (var a in pi.Question.Choices)
                {
                    sw.WriteLine("{0}: {1}", ++i, a.Text.TrimQuestionString());
                }
            }
            Process.Start(qf);

            Console.WriteLine("Input your answer below");
            var userIn = Console.ReadLine();
            if (int.TryParse(userIn, out int sel))
            {
                var s = VotePageNavigator.CreateSubmit(pi, sel - 1);
                using (var sw = new StreamWriter(submit))
                {
                    sw.WriteLine("Below the line is the request: ");
                    sw.WriteLine("------------------------------");
                    foreach (var k in s.KeyValues.AllKeys.OrderBy(x => x))
                    {
                        sw.WriteLine($"{k}: {s.KeyValues.Get(k)}");
                    }
                }
                Process.Start(submit);

                var res = vpn.Submit(s);
                using (var fs = new FileStream(reply, FileMode.Create))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(res);
                }
                Process.Start(reply);
            }
        }

        static void TestMobile(int voteid = VotePageNavigator.DefaultVoteId)
        {
            var vpn = new VotePageNavigator(voteid, VotePageNavigator.MobilePageUrlPattern, true);
            var url = vpn.SearchForZhuLinMobileUrl();
            Console.WriteLine($"mobile-url: {url}");
        }

        static void Main(string[] args)
        {
            TestMobile();
        }
    }
}
