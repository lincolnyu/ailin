using System;
using System.Collections.Generic;

namespace WebKit
{
    public class Question
    {
        public class Choice : IEquatable<Choice>, IComparable<Choice>
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public string Key => Text.Trim();

            public int CompareTo(Choice other)
            {
                return Key.CompareTo(other.Key);
            }

            public bool Equals(Choice other)
            {
                return Key == other.Key;
            }

            public override string ToString()
            {
                return $"{Text}|{Value}";
            }

            public static Choice FromString(string s)
            {
                var segs = s.Split('|');
                if (segs.Length != 2) return null;
                var c = new Choice
                {
                    Text = segs[0],
                    Value = segs[1]
                };
                return c;
            }
        }

        public string Title { get; set; }
        public List<Choice> Choices { get; } = new List<Choice>();
    }
}
