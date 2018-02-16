using AiLinConsole.ProxyManagement;
using AiLinConsole.QuestionManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using WebKit;

namespace AiLinConsole
{
    class Program
    {
        static Random _rand = new Random();

        static void Main(string[] args)
        {
            var ppFileName = args[0];
            var qsFileName = args[1];
            var proxyProvider = new ManualProxyProvider();
            using (var sr = new StreamReader(ppFileName))
            {
                proxyProvider.Load(sr);
            }
            var questionSolver = new StorageBasedSolver(AskHuman);
            if (File.Exists(qsFileName))
            {
                using (var qssr = new StreamReader(qsFileName))
                {
                    questionSolver.Load(qssr);
                }
            }
            var agent = new AiLinAgent(proxyProvider, questionSolver);
            agent.RunThruAllProxies(true);
            using (var qssw = new StreamWriter(qsFileName))
            {
                questionSolver.Save(qssw);
            }
        }

        private static void ShowInTextEditor(string content, 
            string textEditor = @"c:\windows\notepad.exe",
            string tempFolder = @"c:\temp",
            string filename = null)
        {
            var toCreateRandom = filename == null;
            if (toCreateRandom)
            {
                filename = _rand.Next().ToString() + ".txt";
            }
            var path = Path.Combine(tempFolder, filename);

            using (var sw = new StreamWriter(path))
            {
                sw.Write(content);
            }

            // TODO make sure this blocks or it doesn't prevent deletion
            Process.Start(textEditor, path);

            if (toCreateRandom)
            {
                File.Delete(path);
            }
        }

        private static Tuple<int, bool> AskHuman(string question, List<Question.Choice> choices)
        {
            var sb = new StringBuilder();
            sb.AppendLine(question);
            var i = 1;
            foreach (var c in choices)
            {
                sb.AppendFormat("{0}: {1}\n", i++, c.Key);
            }
            ShowInTextEditor(sb.ToString());

            while (true)
            {
                Console.Write("Please input the answer:");
                var input = Console.ReadLine();
                if (!int.TryParse(input, out var ii))
                {
                    Console.WriteLine("Answer invalid. Try again.");
                    continue;
                }
                Console.WriteLine("Are the keys indepedent? (Y for yes)");
                input = Console.ReadLine();
                var indepedent = input.Trim().ToUpper() == "Y";
                return new Tuple<int, bool>(ii, indepedent);
            }
        }
    }
}
