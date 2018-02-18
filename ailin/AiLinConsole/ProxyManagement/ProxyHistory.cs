using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AiLinConsole.ProxyManagement
{
    public class ProxyHistory
    {
        public readonly TimeSpan MaxHistoryLen = TimeSpan.FromDays(1);

        public class Record
        {
            public string ProxyAddress;
            public readonly Dictionary<string, DateTime> LastVisit = new Dictionary<string, DateTime>();
        }

        public readonly Dictionary<string, Record> Records = new Dictionary<string, Record>();

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
            var elapse = DateTime.UtcNow - lv;
            return elapse < MaxHistoryLen;
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
                    var elapse = DateTime.UtcNow - lastVisit;
                    if (elapse < MaxHistoryLen)
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
                    var elapse = DateTime.UtcNow - lv.Value;
                    if (elapse < MaxHistoryLen)
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
