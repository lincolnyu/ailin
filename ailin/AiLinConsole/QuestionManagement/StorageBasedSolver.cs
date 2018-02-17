using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebKit;

namespace AiLinConsole.QuestionManagement
{
    class StorageBasedSolver : IQuestionSolver
    {
        public class Answer
        {
            public List<Tuple<List<Question.Choice>, int>> ChoicesInstances 
                = new List<Tuple<List<Question.Choice>, int>>();

            public List<string> PossibleAnswers = new List<string>();
        }

        private AskHumanDelegate _askHuman;

        public delegate Tuple<int, bool> AskHumanDelegate(string question, List<Question.Choice> choices);

        public Dictionary<string, Answer> Answers = new Dictionary<string, Answer>();

        public StorageBasedSolver(AskHumanDelegate askHuman)
        {
            _askHuman = askHuman ?? throw new ArgumentException("Must provide a non-null askHuman method");
        }

        public void Save(StreamWriter sw)
        {
            foreach (var answer in Answers)
            {
                sw.WriteLine(answer.Key);
                var a = answer.Value;
                foreach(var ci in a.ChoicesInstances)
                {
                    sw.WriteLine("  {0}", ci.Item2);
                    foreach (var c in ci.Item1)
                    {
                        sw.WriteLine("    {0}", c.ToString());
                    }
                }
                sw.WriteLine("  Possible:");
                foreach (var pa in a.PossibleAnswers)
                {
                    sw.WriteLine("    {0}", pa);
                }
            }
        }

        public void Load(StreamReader sr)
        {
            Answer curr = null;
            List<Question.Choice> currC = null;
            List<string> currPA = null;
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var spaces = line.TakeWhile(x => x == ' ').Count();
                if (spaces == 0)
                {
                    var key = line;
                    curr = Answers[key] = new Answer();
                }
                else if (spaces == 2)
                {
                    var si = line.TrimStart();
                    if (si == "Possible:")
                    {
                        currPA = curr.PossibleAnswers;
                        currC = null;
                    }
                    else
                    {
                        var i = int.Parse(si);
                        currC = new List<Question.Choice>();
                        currPA = null;
                        curr.ChoicesInstances.Add(new Tuple<List<Question.Choice>, int>(currC, i));
                    }
                }
                else if (spaces >= 4)
                {
                    if (currPA != null)
                    {
                        currPA.Add(line.Substring(4));
                    }
                    else if (currC != null)
                    {
                        var ci = Question.Choice.FromString(line.Substring(4));
                        currC.Add(ci);
                    }
                }
            }
        }

        public Tuple<int, FeedbackDelegate> Solve(string question, 
            List<Question.Choice> choices)
        {
            if (Answers.TryGetValue(question, out var answer))
            {
                foreach(var ci in answer.ChoicesInstances)
                {
                    var r = Match(choices, ci.Item1, ci.Item2);
                    if (r != null)
                    {
                        return new Tuple<int, FeedbackDelegate>(r.Value, null);
                    }
                }
                for (var i = 0; i < choices.Count; i++)
                {
                    var ci = choices[i];
                    if (answer.PossibleAnswers.Contains(ci.Key))
                    {
                        return new Tuple<int, FeedbackDelegate>(i, null);
                    }
                }
            }
            else
            {
                answer = Answers[question] = new Answer();
            }
            var tuple = _askHuman(question, choices);
            if (tuple == null) return null;
            var index = tuple.Item1;
            var indepedent = tuple.Item2;
            var key = choices[index].Key;
            void fb(bool correct)
            {
                if (correct)
                {
                    answer.ChoicesInstances.Add(
                        new Tuple<List<Question.Choice>, int>(choices, index));
                    if (indepedent)
                    {
                        answer.PossibleAnswers.Add(key);
                    }
                }
            }
            return new Tuple<int, FeedbackDelegate>(index, fb);
        }

        private int? Match(List<Question.Choice> question, List<Question.Choice> orderedExisting, int iAnswer)
        {
            if (question.Count != orderedExisting.Count) return null;
            var qo = question.OrderBy(x => x.Text).ToArray();
            for (var i = 0; i < question.Count; i++)
            {
                var c1 = question[i];
                var c2 = qo[i];
                if (!c1.Equals(c2)) return null;
            }
            for (var i = 0; i < question.Count; i++)
            {
                if (question[i].Equals(orderedExisting[iAnswer]))
                {
                    return i;
                }
            }
            return null;
        }
    }
}
