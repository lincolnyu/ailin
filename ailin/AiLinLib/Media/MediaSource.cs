using System;
using System.Collections.Generic;

namespace AiLinLib.Media
{
    public class MediaSource
    {
        public string Name { get; set; }
        public string Target { get; set; }
        public List<Tuple<string, string>> Playlist { get; } = new List<Tuple<string, string>>();
    }
}
