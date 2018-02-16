using System;

namespace AiLinConsole.ProxyManagement
{
    internal interface IProxy
    {
        string Address { get; }

        TimeSpan? RecommendedTimeout { get; }
    }
}