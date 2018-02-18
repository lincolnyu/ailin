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

        private string _ppFileName;
        private string _qsFileName;
        private Thread _thread;
        private AiLinAgent _agent;
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
                    return new Tuple<bool, bool>(p.Speed > 60, false);
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
                _agent.RunThruAllProxies(false);
            }
            finally
            {
                if (questionSolver != null)
                {
                    PrepareFileAndWrite(_qsFileName, sw => questionSolver.Save(sw));
                }
            }
            Console.WriteLine("Main working thread terminated.");
        }

        private void AgentOnVoteStarted(int voteId, IProxy proxy)
        {
            Console.Write($"Vote {voteId} started");
            if (proxy != null)
            {
                Console.WriteLine($" on proxy {proxy.Address}");
            }
        }

        private void AgentOnResultReceived(int voteId, IProxy proxy,
            bool successful, string replyMsg)
        {
            Console.Write($"Vote {voteId}");
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
                    var textRes = ShowInTextEditor(replyMsg);
                    var textFileName = Path.GetFileNameWithoutExtension(textRes.Item2);
                    Console.WriteLine($" failed. See text '{textFileName}' popped up for detail.");
                }
            }
            else
            {
                Console.WriteLine(" aborted by user.");
            }
        }

        static void Main(string[] args)
        {
            var sigfn = GenerateRandomTempFile(DefaultTempFolder, "lock.");
            using (var sw = new StreamWriter(sigfn))
            {
                sw.WriteLine("This file was created for execution signaling");
            }
            Console.WriteLine($"File '{sigfn}' was created for execution signaling. Delete the file to abort this application");

            using (var program = new Program
            {
                _qsFileName = args[0],
                _ppFileName = args.Length > 1 ? args[1] : null,
            })
            {
                while (File.Exists(sigfn))
                {
                    Thread.Sleep(1000);
                }
                Console.WriteLine("End of main function reached.");
            }
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
            string temppath;
            do
            {
                var filename = prefix + _rand.Next().ToString() + ".txt";
                temppath = Path.Combine(tempFolder, filename);
            } while (File.Exists(temppath));
            return temppath;
        }

        private static Tuple<Process, string> ShowInTextEditor(string content, 
            string textEditor = @"c:\windows\notepad.exe",
            string tempFolder = DefaultTempFolder,
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
            var textRes = ShowInTextEditor(sb.ToString());
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
