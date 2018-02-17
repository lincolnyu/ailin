using System;
using System.Collections.Generic;

namespace WebKit
{
    public class Question
    {
        public class Choice : IEquatable<Choice>, IComparable<Choice>
        {
            private string _text;
            private string _key;

            public string Text
            {
                get { return _text; }
                set
                {
                    _text = value;
                    _key = null;
                }
            }

            public string Value { get; set; }

            public string Key
            {
                get
                {
                    if (_key != null) return _key;
                    _key = Text.Trim().Replace("\n", "").Replace("\r", "");
                    return _key;
                }
            }

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
                return Key;
            }

            public static Choice FromString(string s)
            {
                return new Choice
                {
                    Text = s,
                    Value = ""
                };
            }
        }

        public string Title { get; set; }
        public List<Choice> Choices { get; } = new List<Choice>();
    }
}
