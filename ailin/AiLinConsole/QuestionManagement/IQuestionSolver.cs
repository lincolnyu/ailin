using System.Collections.Generic;
using WebKit;

namespace AiLinConsole.QuestionManagement
{
    interface IQuestionSolver
    {
        int Solve(string question, List<Question.Choice> choices);
    }
}
