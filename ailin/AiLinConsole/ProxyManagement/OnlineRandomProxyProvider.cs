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

        private string _url = "https://gimmeproxy.com/api/getProxy?protocol=http&anonymityLevel=0";

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
                byte[] data;
                try
                {
                    data = client.DownloadData(_url);
                }
                catch (WebException)
                {
                    break; // TODO may make a few more attempts
                }
                // Here we assume it's UTF8
                var content = Encoding.UTF8.GetString(data);
                if (content.ParseJson() is JsonPairs jspairs)
                {
                    jspairs.TryGetValue("ipPort", out string ipport);
                    jspairs.TryGetValue("speed", out Numeric speed); // KBps
                    var proxy = new Proxy
                    {
                        Address = $"{ipport}",
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
