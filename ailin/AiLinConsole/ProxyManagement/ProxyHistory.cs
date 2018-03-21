using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AiLinConsole.ProxyManagement
{
    public class ProxyHistory
    {
        public class Record
        {
            public string ProxyAddress;
            public readonly Dictionary<string, DateTime> LastVisit = new Dictionary<string, DateTime>();
        }

        public readonly Dictionary<string, Record> Records = new Dictionary<string, Record>();
        
        private bool IsRecentlyVisited(DateTime lastVisit)
        {
            var now = DateTime.UtcNow;
            var diff = now - lastVisit;
            if (diff.TotalHours > 12) return true; // maybe worth trying again
            var tzi = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            var bjnow = TimeZoneInfo.ConvertTimeFromUtc(now, tzi);
            var bjLastVisit = TimeZoneInfo.ConvertTimeFromUtc(lastVisit, tzi);
            return (bjnow.Year != bjLastVisit.Year || bjnow.DayOfYear != bjLastVisit.DayOfYear);
        }

        public void Visit(string proxy, string target)
        {
            if (!Records.TryGetValue(proxy, out var record))
            {
                record = Records[proxy] = new Record
                {
                    ProxyAddress = proxy
                };
            }
            record.LastVisit[target] = DateTime.UtcNow;
        }

        public bool RecentlyVisited(string proxy, string target)
        {
            if (!Records.TryGetValue(proxy, out var r))
            {
                return false;
            }
            if (!r.LastVisit.TryGetValue(target, out var lv))
            {
                return false;
            }
            return IsRecentlyVisited(lv);
        }

        public void Load(StreamReader sr)
        {
            Records.Clear();
            Record record = null;
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (line.StartsWith("  "))
                {
                    var segs = line.Split('>');
                    var target = segs[0].Trim();
                    var lastVisit = DateTime.Parse(segs[1].Trim());
                    if (IsRecentlyVisited(lastVisit))
                    {
                        record.LastVisit[target] = lastVisit;
                    }
                }
                else
                {
                    if (record != null && record.LastVisit.Count > 0)
                    {
                        Records[record.ProxyAddress] = record;
                    }
                    record = new Record
                    {
                        ProxyAddress = line.Trim()
                    };
                }
            }
            if (record != null && record.LastVisit.Count > 0)
            {
                Records[record.ProxyAddress] = record;
            }
        }

        public void Save(StreamWriter sw)
        {
            foreach (var r in Records.Values)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{r.ProxyAddress}");
                var hasVisits = false;
                foreach(var lv in r.LastVisit)
                {
                    if (IsRecentlyVisited(lv.Value))
                    {
                        sb.AppendLine($"  {lv.Key}>{lv.Value}");
                        hasVisits = true;
                    }
                }
                if (hasVisits)
                {
                    sw.Write(sb.ToString());
                }
            }
        }
    }
}
