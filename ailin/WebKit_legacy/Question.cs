using System.Collections.Generic;

namespace WebKit
{
    public class Question
    {
        public class Choice
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        public string Title { get; set; }
        public List<Choice> Choices { get; } = new List<Choice>();
    }
}
