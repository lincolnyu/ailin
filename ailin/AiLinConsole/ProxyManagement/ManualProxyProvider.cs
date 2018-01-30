using System.Collections;
using System.Collections.Generic;

namespace AiLinConsole.ProxyManagement
{
    class ManualProxyProvider : IProxyProvider
    {
        public List<IProxy> Proxies { get; } = new List<IProxy>();

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
