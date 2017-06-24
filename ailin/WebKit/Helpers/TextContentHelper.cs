using System;
using System.Text;

namespace WebKit.Helpers
{
    public static class TextContentHelper
    {
        public static string GetTextContent(this string str, int start = 0)
        {
            var n = str.GetNextNode(start, x => false);
            switch (n.EndingType)
            {
                case HtmlNodeHelper.NodeInfo.EndingTypes.NoTags:
                case HtmlNodeHelper.NodeInfo.EndingTypes.ClosingTagOnly:
                    return str.Substring(start, n.ContentEnd - start);
                default:
                    break;
            }

            var sb = new StringBuilder();
            sb.Append(str.Substring(start, n.Start - start));

            var open = str.Substring(n.Start, n.ContentStart - n.Start);
            var style = open.GetAttribute("style");
            if (style == null || !style.ToLower().Contains("display:none"))
            {
                var inner = str.Substring(n.ContentStart, n.ContentLength);
                sb.Append(inner.GetTextContent());
            }
            if (n.End < str.Length)
            {
                sb.Append(str.GetTextContent(n.End));
            }
            return sb.ToString();
        }
    }
}
