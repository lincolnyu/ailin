using AiLinConsole.ProxyManagement;
using AiLinConsole.QuestionManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using WebKit;

namespace AiLinConsole
{
    class Program : IDisposable
    {
        const string DefaultTempFolder = @"c:\temp";

        private static Random _rand = new Random();

        private bool _showReplyMsg;
        private string _tempFolder;
        private string _ppFileName;
        private string _qsFileName;
        private string _proxyHistoryFileName;
        private Thread _thread;
        private AiLinAgent _agent;
        private ProxyHistory _proxyHistory = new ProxyHistory();
        private bool _cancelled = false;

        Program()
        {
            _thread = new Thread(MainFuncThread);
            _thread.Start();
        }

        ~Program()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_thread != null)
            {
                _cancelled = true;
                if (_agent != null)
                {
                    _agent.Cancel();
                }
                _thread.Join();
            }
        }

        void MainFuncThread()
        {
            if (File.Exists(_proxyHistoryFileName))
            {
                using (var sr = new StreamReader(_proxyHistoryFileName))
                {
                    _proxyHistory.Load(sr);
                }
            }
            IProxyProvider proxyProvider = null;
            if (_ppFileName != null)
            {
                proxyProvider = new ManualProxyProvider();
                using (var sr = new StreamReader(_ppFileName))
                {
                    ((ManualProxyProvider)proxyProvider).Load(sr);
                }
            }
            else
            {
                proxyProvider = new OnlineRandomProxyProvider(p=>
                {
                    return new Tuple<bool, bool>(true, false);
                });
            }
            StorageBasedSolver questionSolver = null;
            try
            {
                questionSolver = new StorageBasedSolver(AskHuman);
                if (File.Exists(_qsFileName))
                {
                    using (var qssr = new StreamReader(_qsFileName))
                    {
                        questionSolver.Load(qssr);
                    }
                }
                _agent = new AiLinAgent(proxyProvider, questionSolver);
                _agent.VoteStarted += AgentOnVoteStarted;
                _agent.VoteResultReceived += AgentOnResultReceived;
                _agent.RunThruAllProxies(true);
            }
            finally
            {
                if (_qsFileName != null && questionSolver != null)
                {
                    PrepareFileAndWrite(_qsFileName, sw => questionSolver.Save(sw));
                }
                if (_proxyHistoryFileName != null)
                {
                    PrepareFileAndWrite(_proxyHistoryFileName, sw => _proxyHistory.Save(sw));
                }
            }
            Console.WriteLine("Main working thread terminated.");
        }

        private void AgentOnVoteStarted(int voteId, IProxy proxy, AiLinAgent.SuppressVoteDelegate suppress)
        {
            var proxyAddress = proxy?.Address ?? "noproxy";
            if (_proxyHistory.RecentlyVisited(proxyAddress, voteId.ToString()))
            {
                suppress();
                Console.Write($"Vote {voteId}");
                if (proxy != null)
                {
                    Console.Write($" via proxy {proxyAddress}");
                }
                Console.WriteLine(" canceled due to recent use");
            }
            else
            {
                _proxyHistory.Visit(proxyAddress, voteId.ToString());
                Console.Write($"Vote {voteId} started");
                if (proxy != null)
                {
                    Console.Write($" via proxy {proxy.Address}");
                }
                Console.WriteLine();
            }
        }

        private void AgentOnResultReceived(int voteId, IProxy proxy,
            bool successful, bool solvedByUser, string replyMsg)
        {
            Console.Write($"Vote {voteId}");
            if (!solvedByUser)
            {
                Console.Write(" solved with existing knowledge");
            }
            if (proxy != null)
            {
                Console.Write($" with proxy {proxy.Address}");
            }
            if (successful)
            {
                Console.WriteLine(" succeeded");
            }
            else if (!_cancelled)
            {
                var incorrect = AiLinAgent.IsIncorrect(replyMsg);
                if (incorrect)
                {
                    Console.WriteLine(" failed. Answer incorrect. Try again.");
                }
                else
                {
                    if (_showReplyMsg)
                    {
                        var textRes = ShowInTextEditor(replyMsg, _tempFolder);
                        var textFileName = Path.GetFileNameWithoutExtension(textRes.Item2);
                        Console.WriteLine($" failed. See text '{textFileName}' popped up for detail.");
                    }
                    else
                    {
                        // TODO we should do logging
                        Console.WriteLine($" failed.");
                    }
                }
            }
            else
            {
                Console.WriteLine(" aborted by user.");
            }
        }

        static void Main(string[] args)
        {
            var itf = Array.IndexOf(args, "-tf");
            var tf = DefaultTempFolder;
            if (itf >= 0)
            {
                tf = args[itf + 1];
            }
            var sigfn = GenerateRandomTempFile(tf, "lock.");
            using (var sw = new StreamWriter(sigfn))
            {
                sw.WriteLine("This file was created for execution signaling");
            }
            Console.WriteLine($"File '{sigfn}' was created for execution signaling. Delete the file to abort this application");

            var iqs = Array.IndexOf(args, "-qs");
            var iph = Array.IndexOf(args, "-ph");
            var ipp = Array.IndexOf(args, "-pp");
            var inoreply = Array.IndexOf(args, "-noreply");

            using (var program = new Program
            {
                _qsFileName = iqs >= 0 ? args[iqs + 1] : null,
                _proxyHistoryFileName = iph >= 0 ? args[iph + 1] : null,
                _ppFileName = ipp >= 0 ? args[ipp + 1] : null,
                _showReplyMsg = inoreply < 0,
                _tempFolder = tf
            })
            {
                while (File.Exists(sigfn))
                {
                    Thread.Sleep(1000);
                }
            }
            if (File.Exists(sigfn))
            {
                File.Delete(sigfn);
            }
            Console.WriteLine("End of main function reached.");
        }

        private static void PrepareFileAndWrite(string filename, Action<StreamWriter> writeAction)
        {
            var dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (var sw = new StreamWriter(filename))
            {
                writeAction(sw);
            }
        }

        private static string GenerateRandomTempFile(string tempFolder, string prefix = "")
        {
            if(!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            string temppath;
            do
            {
                var filename = prefix + _rand.Next().ToString() + ".txt";
                temppath = Path.Combine(tempFolder, filename);
            } while (File.Exists(temppath));
            return temppath;
        }

        private static Tuple<Process, string> ShowInTextEditor(string content, 
            string tempFolder = DefaultTempFolder,
            string textEditor = @"c:\windows\notepad.exe",
            string filename = null)
        {
            var toCreateRandom = filename == null;
            var path = toCreateRandom ? GenerateRandomTempFile(tempFolder)
                : Path.Combine(tempFolder, filename);

            PrepareFileAndWrite(path, sw => sw.Write(content));
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (var sw = new StreamWriter(path))
            {
                sw.Write(content);
            }

            // TODO make sure this blocks or it doesn't prevent deletion
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = textEditor,
                    Arguments = path
                }
            };

            process.Start();

            var thread = new Thread(() =>
            {
                 process.WaitForExit();
                 if (toCreateRandom)
                 {
                     File.Delete(path);
                 }
             });
            thread.Start();
            return new Tuple<Process, string>(process, path);
        }

        private Tuple<int, bool> AskHuman(string question, List<Question.Choice> choices)
        {
            var sb = new StringBuilder();
            sb.AppendLine(question);
            var i = 1;
            foreach (var c in choices)
            {
                sb.AppendFormat("{0}: {1}", i++, c.Key);
                sb.AppendLine();
            }
            var textRes = ShowInTextEditor(sb.ToString(), _tempFolder);
            var textProc = textRes.Item1;

            while (!_cancelled)
            {
                Console.Write("Please input the answer: ");
                var input = Console.ReadLine();
                if (_cancelled) break;
                if (!int.TryParse(input, out var ii))
                {
                    Console.WriteLine("Answer invalid. Try again.");
                    continue;
                }
                Console.Write("Are the keys indepedent(Y for yes)? ");
                input = Console.ReadLine();
                if (_cancelled) break;
                var indepedent = input.Trim().ToUpper() == "Y";
                if (!textProc.HasExited)
                {
                    textProc.Kill();
                }
                return new Tuple<int, bool>(ii-1, indepedent);
            }
            return null;
        }
    }
}
