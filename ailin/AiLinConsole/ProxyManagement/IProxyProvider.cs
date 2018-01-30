using System;
using System.Collections.Generic;
using System.Text;

namespace AiLinConsole.ProxyManagement
{
    interface IProxyProvider : IEnumerable<IProxy>
    {
    }
}
