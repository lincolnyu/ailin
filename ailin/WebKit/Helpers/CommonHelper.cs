using System;
using System.Text;
using System.Text.RegularExpressions;

namespace WebKit.Helpers
{
    public static class CommonHelper
    {
#if NETCOREAPP2_0
        static CommonHelper()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
#endif

        public static string ConvertGB2312ToUTF(this byte[] b)
        {
            var enc = Encoding.GetEncoding(936);
            return enc.GetString(b);
        }

        public static string UTFToGB2312HtmlString(this string u)
        {
            var enc = Encoding.GetEncoding(936);
            var bs = enc.GetBytes(u);
            var sb = new StringBuilder();
            foreach (var b in bs)
            {
                sb.AppendFormat("%{0:X2}", b);
            }
            return sb.ToString();
        }

        public static string UrlInHtmlToUrl(this string urlInHtml)
        {
            var url = urlInHtml.Replace("&amp;", "&");
            return url;
        }

        public static Tuple<string, int> GetNextInput(this string s, int start, bool ignoreCase = true)
        {
            var pattern = "<input [^>]*>";
            var rex = ignoreCase? new Regex(pattern, RegexOptions.IgnoreCase) : new Regex(pattern);
            var m = rex.Match(s, start);
            if (m.Success)
            {
                return new Tuple<string, int>(m.Value, m.Index);
            }
            return null;
        }

        public static string TrimQuestionString(this string s)
        {
            return s.Trim(' ', '\t', '\r', '\n');
        }

        public static string GetAttribute(this string s, string attributeName, bool ignoreCase = true)
        {
            var ro = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            var rexs = new Regex[3]
            {
                new Regex(attributeName + "[ ]*=[ ]*([^ \"\']+)[ >]", ro),
                new Regex(attributeName + "[ ]*=[ ]*\"([^\"]*)\"", ro),
                new Regex(attributeName + "[ ]*=[ ]*\'([^\']*)\'", ro)
            };
            foreach (var rex in rexs)
            {
                var m = rex.Match(s);
                if (m.Success)
                {
                    return m.Groups[1].Value;
                }
            }
            return null;
        }

        public static int? GetAttributeInt(this string s, string attributeName, StringComparison sc = StringComparison.OrdinalIgnoreCase)
        {
            var index = s.IndexOf(attributeName + "=", 0, sc)
                + attributeName.Length + 1;
            var rex = new Regex("[ ]*([0-9]+)");
            var m = rex.Match(s, index);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int v))
            {
                return v;
            }
            return null;
        }
    }
}
