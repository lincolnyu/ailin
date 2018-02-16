using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AiLinConsole.ProxyManagement
{
    class ManualProxyProvider : IProxyProvider
    {
        public List<IProxy> Proxies { get; } = new List<IProxy>();

        public void Load(StreamReader sr)
        {
            Proxies.Clear();
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine().Trim();
                if (line.StartsWith("--")) continue;
                if (line.Contains("--"))
                {
                    var i = line.IndexOf("--");
                    line = line.Substring(0, i);
                }
                var proxy = Proxy.FromString(line);
                Proxies.Add(proxy);
            }
            Proxies.Sort((a, b) =>
            {
                if (a.RecommendedTimeout.HasValue && b.RecommendedTimeout.HasValue)
                {
                    return a.RecommendedTimeout.Value.CompareTo(b.RecommendedTimeout.Value);
                }
                else if (a.RecommendedTimeout.HasValue)
                {
                    return 1;
                }
                else if (b.RecommendedTimeout.HasValue)
                {
                    return -1;
                }
                return 0;
            });
        }

        public void Save(StreamWriter sw)
        {
            foreach(var proxy in Proxies)
            {
                sw.WriteLine(proxy.ToString());
            }
        }

        public IEnumerator<IProxy> GetEnumerator()
        {
            return Proxies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
