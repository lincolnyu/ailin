using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WebKit.Helpers
{
    public static class CommonHelper
    {
        public enum UrlType
        {
            Absolute,
            Rootbased,
            Relative
        }

        public static UrlType GetUrlType(this string link)
        {
            if (link.StartsWith("."))
            {
                return UrlType.Relative;
            }
            if (link.StartsWith("/"))
            {
                return UrlType.Rootbased;
            }
            if (link.StartsWith("http://") || link.StartsWith("https://"))
            {
                return UrlType.Absolute;
            }
            return UrlType.Relative;
        }
        
        public static string RelativeToAbsolute(this string orig, string url)
        {
            var urlType = url.GetUrlType();
            if (urlType == UrlType.Absolute)
            {
                return url;
            }
            var baseUrl = orig.GetBaseUrl();
            if (urlType == UrlType.Rootbased)
            {
                return baseUrl + url;
            }
            return CombineUrl(orig, url);
        }

        private static string CombineUrl(string abs, string b)
        {
            var slash = GetRootSlashPosition(abs);
            var segsa = abs.Substring(slash + 1).Split('/');
            var segsb = abs.Split('/');
            var ai = segsa.Length;
            var eliminating = true;
            var sb = new StringBuilder();
            foreach (var segb in segsb)
            {
                if (eliminating)
                {
                    if (segb == "..")
                    {
                        ai--;
                    }
                    else if (segb != ".")
                    {
                        eliminating = false;
                        sb.Append(abs.Substring(slash));
                        for (var i = 0; i < ai; i++)
                        {
                            sb.Append('/');
                            sb.Append(segsa[i]);
                        }
                    }
                }
                if (!eliminating)
                {
                    sb.Append('/');
                    sb.Append(segb);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        ///  Get the base url (excluding the tailing /)
        /// </summary>
        /// <param name="abs">The original url</param>
        /// <returns>The base url</returns>
        public static string GetBaseUrl(this string abs)
        {
            var slashPosition = abs.GetRootSlashPosition();
            return abs.Substring(0, slashPosition);
        }

        private static int GetRootSlashPosition(this string abs)
        {
            int start = 0;
            if (abs.StartsWith("http://"))
            {
                start += "http://".Length;
            }
            else if (abs.StartsWith("https://"))
            {
                start += "https://".Length;
            }
            var index = abs.IndexOf('/', start);
            return index < 0 ? abs.Length : index;
        }

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
