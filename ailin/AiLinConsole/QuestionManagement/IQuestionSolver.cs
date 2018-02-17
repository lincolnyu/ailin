using System;
using System.Collections.Generic;
using WebKit;

namespace AiLinConsole.QuestionManagement
{
    public delegate void FeedbackDelegate(bool correct);

    public interface IQuestionSolver
    {
        Tuple<int, FeedbackDelegate> Solve(string question, List<Question.Choice> choices);
    }
}
