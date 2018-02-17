using JsonParser;
using JsonParser.JsonStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AiLinConsole.ProxyManagement
{
    class OnlineRandomProxyProvider : IProxyProvider
    {
        /// <summary>
        ///  Filter provided by user
        /// </summary>
        /// <param name="proxy">The proxy to be checked by user</param>
        /// <returns>
        ///  First bool determines whether the proxy is to be yield returned
        ///  The second bool determines whether the enumeration loop should quit in this
        ///  iteration 
        /// </returns>
        public delegate Tuple<bool, bool> FilterDelegate(Proxy proxy);

        public const string Url = "https://gimmeproxy.com/api/getProxy?protocol=http";

        private FilterDelegate _filter;

        public OnlineRandomProxyProvider(FilterDelegate filter)
        {
            _filter = filter;
        }

        public IEnumerator<IProxy> GetEnumerator()
        {
            var client = new WebClient();
            while (true)
            {
                var data = client.DownloadData(Url);
                // Here we assume it's UTF8
                var content = Encoding.UTF8.GetString(data);
                if (content.ParseJson() is JsonPairs jspairs)
                {
                    jspairs.TryGetValue("ip", out string ip);
                    jspairs.TryGetValue("port", out string port);
                    jspairs.TryGetValue("speed", out Numeric speed); // KBps
                    var proxy = new Proxy
                    {
                        Address = $"{ip}:{port}",
                        Speed = double.Parse(speed.Value)
                        // TODO work out timeout
                    };
                    var r = _filter(proxy);
                    if (r.Item1)
                    {
                        yield return proxy;
                    }
                    if (r.Item2)
                    {
                        break;
                    }
                }
                else
                {
                    var r = _filter(null);
                    if (r.Item2)
                    {
                        break;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
