using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace WebKit
{
    public class SubmitHandler
    {
        public NameValueCollection KeyValues { get; } = new NameValueCollection();

        public HashSet<string> Reserverd { get; } = new HashSet<string>
        {
            "post_data"
        };

        public PageInfo RefPage { get; internal set; }

        public int A;
        public int B;
        public int C;

        public bool Process(string page)
        {
            // TODO this is quite rigid, currently getting all the hidden plus a few specified
            for (var start = 0; start >= 0; start = AddNext(page, start))
            {
            }
            var r = new Random();
            if (GetMouzCoeffs(page))
            {
                var mx = r.Next(500, 900);
                var my = r.Next(1600, 1900);
                var mz = A * mx + B * my + C;
                KeyValues["moux"] = mx.ToString();
                KeyValues["mouy"] = my.ToString();
                KeyValues["mouz"] = mz.ToString();
                return true;
            }
            return false;
        }

        private int AddNext(string page, int start)
        {
            string input;
            int index;
            string name;
            while (true)
            {
                var ii = page.GetNextInput(start);
                if (ii == null) return -1;
                input = ii.Item1;
                index = ii.Item2;
                name = input.GetAttribute("name");
                if (name != null && Reserverd.Contains(name))
                {
                    break;
                }
                var t = input.GetAttribute("type");
                if (t != null && t.Equals("hidden", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                start = index + input.Length;
            }
            var val = input.GetAttribute("value");
            if (name == "post_data")
            {
                val = val.UTFToGB2312HtmlString();
            }
            KeyValues[name] = val;
            return index + input.Length;
        }

        private bool GetMouzCoeffs(string page)
        {
            // something like document.getElementById('mouz').value = 1 * mx + 4 * my + 78;
            const string target = "document.getElementById('mouz').value";
            var index = page.IndexOf(target) + target.Length;
            index = page.IndexOf('=', index);
            var end = page.IndexOf(';', index);
            var s = page.Substring(index, end - index);
            var m = Regex.Match(s, @"([0-9]+)[ ]*\*[ ]*mx[ ]*\+[ ]*([0-9]+)[ ]*\*[ ]*my[ ]*\+([0-9]+)");
            if (m.Success)
            {
                var sa = m.Groups[1].Value;
                var sb = m.Groups[2].Value;
                var sc = m.Groups[3].Value;
                if (int.TryParse(sa, out int a) && int.TryParse(sb, out int b) && int.TryParse(sc, out int c))
                {
                    A = a;
                    B = b;
                    C = c;
                    return true;
                }
            }
            return false;
        }
    }
}
