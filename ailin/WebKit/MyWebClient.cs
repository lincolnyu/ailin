using System;
using System.Net;

namespace WebKit
{
    class MyWebClient : WebClient
    {
        private CookieContainer _container = new CookieContainer();

        public CookieContainer CookieContainer => _container;

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest webRequest)
            {
                webRequest.CookieContainer = _container;
                webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            }
            return request;
        }

        public void ClearCookies()
        {
            _container = new CookieContainer();
        }
    }
}
