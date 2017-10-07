using JsonParser.JsonStructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AiLinLib.Media
{
    public class MediaInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }

        /// <summary>
        ///  YYYYMMDD
        /// </summary>
        public string DateStr { get; set; }

        public string Category { get; set; }
        public string Role { get; set; }
        public string Director { get; set; }
        public string Playwright { get; set; }
        public string Producer { get; set; }
        public string AdaptedFrom { get; set; }
        public string Remarks { get; set; }
        
        /// <summary>
        ///  Link to the primary descriptive page
        /// </summary>        
        public string ExternalLink { get; set; }

        public string SourcesRemarks { get; set; }

        public List<Tuple<string, string>> Songs { get; } = new List<Tuple<string, string>>();

        public List<MediaSource> Sources { get; } = new List<MediaSource>();

        public static MediaInfo TryParse(JsonPairs mediaInfo)
        {
            if (!mediaInfo.TryGetValue("id", out string id))
            {
                return null;
            }
            var mi = new MediaInfo { Id = id };
            if (mediaInfo.TryGetValue("title", out string title))
            {
                mi.Title = title;
            }
            if (mediaInfo.TryGetValue("type", out string category))
            {
                mi.Category = category;
            }
            if (mediaInfo.TryGetValue("date", out string date))
            {
                mi.DateStr = date;
            }
            if (mediaInfo.TryGetValue("role", out string role))
            {
                mi.Role = role;
            }
            if (mediaInfo.TryGetValue("director", out string director))
            {
                mi.Director = director;
            }
            if (mediaInfo.TryGetValue("producer", out string producer))
            {
                mi.Producer = producer;
            }
            if (mediaInfo.TryGetValue("playwright", out string playwright))
            {
                mi.Playwright = playwright;
            }
            if (mediaInfo.TryGetValue("adaptedFrom", out string adaptedFrom))
            {
                mi.AdaptedFrom = adaptedFrom;
            }
            if (mediaInfo.TryGetValue("remarks", out string remarks))
            {
                mi.Remarks = remarks;
            }
            if (mediaInfo.TryGetValue("link", out string link))
            {
                mi.ExternalLink = link;
            }
            if (mediaInfo.TryGetNode("songs", out JsonArray songs))
            {
                foreach (var p in songs.Items.OfType<JsonPairs>().Where(x=>x.KeyValues.Count==1))
                {
                    var pair = p.KeyValues.First();
                    if (JsonPairs.TryGetValue(pair, out string t))
                    {
                        var song = new Tuple<string, string>(pair.Key, t);
                        mi.Songs.Add(song);
                    }
                }
            }
            if (mediaInfo.TryGetNode("sources", out JsonArray sources))
            {
                // Ignoreing invalid ones
                foreach (var item in sources.Items.OfType<JsonPairs>())
                {
                    if (item.TryGetValue("name", out string name))
                    {
                        var source = new MediaSource
                        {
                            Name = name
                        };
                        if (item.TryGetValue("target", out string target))
                        {
                            source.Target = target;
                        }
                        if (item.TryGetNode("playlist", out JsonArray playlist))
                        {
                            foreach (var p in playlist.Items.OfType<JsonPairs>().Where(x=>x.KeyValues.Count==1))
                            {
                                var pair = p.KeyValues.First();
                                if (JsonPairs.TryGetValue(pair, out string t))
                                {
                                    var track = new Tuple<string, string>(pair.Key, t);
                                    source.Playlist.Add(track);
                                }
                            }
                        }
                        mi.Sources.Add(source);
                    }
                }
            }
            if (mediaInfo.TryGetValue("sourcesRemarks", out string sourcesRemarks))
            {
                mi.SourcesRemarks = sourcesRemarks;
            }
            return mi;
        }
    }
}
