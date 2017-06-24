using System.Text.RegularExpressions;

namespace WebKit.Helpers
{
    public static class VoteResponseHelper
    {
        public static string GetVoteResponseMessage(this string page, bool cleanup = false)
        {
            var rexa = new Regex(@"alert\('([^']+)'\)");
            var ma = rexa.Match(page, 0);
            if (ma.Success)
            {
                return ma.Groups[1].Value;
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
                        return s;
                    }
                    start = m.Index + m.Length;
                }
                else
                {
                    break;
                }
            }
            return null;
        }

        private static string Cleanup(string s)
        {
            var rex = new Regex("<[^>]>|[\r\n\t ]");
            s = rex.Replace(s, "");
            return s;
        }
    }
}
