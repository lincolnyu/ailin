using System;
using System.Text.RegularExpressions;

namespace WebKit.Helpers
{
    public static class VoteResponseHelper
    {
        public static Tuple<string, bool> GetVoteResponseMessage(this string page, bool cleanup = false)
        {
            var rexa = new Regex(@"alert\('([^']+)'\)");
            var ma = rexa.Match(page, 0);
            if (ma.Success)
            {
                //it looks like '1个选项被成功提交'
                var res = ma.Groups[1].Value.Contains("成功提交");
                return new Tuple<string, bool>(ma.Groups[1].Value, true);
            }
            var rex = new Regex("<td[^>]*>");
            var start = 0;
            while (true)
            {
                var m = rex.Match(page, start);
                if (m.Success)
                {
                    var style = m.Value.GetAttribute("style");
                    if (style != null && style.ToLower().Contains("font-family:'黑体'"))
                    {
                        var e = page.IndexOf("</td>", m.Index + m.Length) + "</td>".Length;
                        var s = page.Substring(m.Index, e-m.Index).GetTextContent(0);
                        if (cleanup)
                        {
                            s = Cleanup(s);
                        }
                        return new Tuple<string, bool>(s, false);
                    }
                    start = m.Index + m.Length;
                }
                else
                {
                    break;
                }
            }
            return new Tuple<string, bool>(null, false);
        }

        private static string Cleanup(string s)
        {
            var rex = new Regex("<[^>]>|[\r\n\t ]");
            s = rex.Replace(s, "");
            return s;
        }
    }
}
