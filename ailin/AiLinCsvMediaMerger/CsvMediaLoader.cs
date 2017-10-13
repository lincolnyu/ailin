using AiLinLib.Media;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace AiLinCsvMediaMerger
{
    public static class CsvMediaLoader
    {
        public static MediaRepository Load(StreamReader sr)
        {
            var mr = new MediaRepository();
            var line = sr.ReadLine();
            var fieldCount = line.Split(',').Length;
            while (!sr.EndOfStream)
            {
                var fields = sr.GetNextEntrty().ToArray();
                var mi = new MediaInfo();
                var index = 0;
                if (!string.IsNullOrWhiteSpace(fields[index]))
                {
                    mi.Id = fields[index];
                }
                index++;
                if (!string.IsNullOrWhiteSpace(fields[index]))
                {
                    mi.Title = fields[index];
                }
                index++;
                if (!string.IsNullOrWhiteSpace(fields[index]))
                {
                    mi.DateStr = ParseDate(fields[index]);
                }
                index++;
                if (!string.IsNullOrWhiteSpace(fields[index]))
                {
                    mi.Category = fields[2];
                }
                index++;
                if (!string.IsNullOrWhiteSpace(fields[index]))
                {
                    mi.Role = fields[index];
                }
                index++;
                if (!string.IsNullOrWhiteSpace(fields[index]))
                {
                    mi.Director = fields[index];
                }
                index++;
                if (!string.IsNullOrWhiteSpace(fields[index]))
                {
                    mi.Playwright = fields[index];
                }
                index++;
                if (!string.IsNullOrWhiteSpace(fields[index]))
                {
                    mi.AdaptedFrom = fields[index];
                }
                index++;
                if (!string.IsNullOrWhiteSpace(fields[index]))
                {
                    mi.Producer = fields[index];
                }
                index++;
                if (!string.IsNullOrWhiteSpace(fields[index]))
                {
                    var links = fields[index];
                    links = links.Trim('"');
                    var h1 = links.IndexOf("http");
                    if (h1 >= 0)
                    {
                        var h2 = links.IndexOf("http", h1 + "http".Length);
                        if (h2 < 0)
                        {
                            mi.ExternalLink = links.Trim();
                        }
                        else
                        {
                            mi.ExternalLink = links.Substring(h1, h2 - h1).Trim();
                        }
                    }
                }
                index++;
                mr.MediaList.Add(mi);
            }
            return mr;
        }

        private static IEnumerable<string> GetNextEntrty(this StreamReader sr)
        {
            int? lastChar = null;
            var sb = new StringBuilder();
            while (!sr.EndOfStream)
            {
                var ch = sr.Read();
                if (ch == 0x0a)
                {
                    if (lastChar == 0x0d)
                    {
                        sb.Remove(sb.Length - 1, 1);
                        yield return sb.ToString();
                        break;
                    }
                    else
                    {
                        sb.Append((char)ch);
                    }
                }
                else if ((char)ch == ',')
                {
                    yield return sb.ToString();
                    sb.Clear();
                }
                else
                {
                    sb.Append((char)ch);
                }
                lastChar = ch;
            }
        }

        public static string ParseDate(string v)
        {
            var segs = v.Split('.');
            var sb = new StringBuilder();
            for (var i = 0; i < Math.Min(segs.Length, 3); i++)
            {
                var seg = segs[i];
                if (i == 0)
                {
                    sb.Append(seg.PadTo(4));
                }
                else if (i == 1)
                {
                    sb.Append(seg.PadTo(2));
                }
                else
                {
                    sb.Append(seg.PadTo(2));
                }
            }
            return sb.ToString();
        }

        private static string PadTo(this string a, int len)
            => a.PadLeft(len, '0');
    }
}
