using System;

namespace AiLinConsole.ProxyManagement
{
    public class Proxy : IProxy
    {
        public string Address { get; set; }

        /// <summary>
        ///  Speed in KB/s
        /// </summary>
        public double? Speed { get; set; }

        public TimeSpan? RecommendedTimeout { get; set; }

        public override string ToString()
        {
            if (RecommendedTimeout.HasValue)
            {
                return $"{Address},{RecommendedTimeout.Value.TotalSeconds}";
            }
            else
            {
                return $"{Address}";
            }
        }

        public static Proxy FromString(string s)
        {
            var segs = s.Split(',');
            if (segs.Length > 2) return null;
            var proxy = new Proxy
            {
                Address = segs[0],
                RecommendedTimeout = segs.Length > 1?
                    TimeSpan.FromSeconds(double.Parse(segs[1])) 
                    : (TimeSpan?)null
            };
            return proxy;
        }
    }
}
