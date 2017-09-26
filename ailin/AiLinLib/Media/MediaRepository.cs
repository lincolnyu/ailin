﻿using System.Collections.Generic;
using JsonParser;
using JsonParser.JsonStructures;
using System;

namespace AiLinLib.Media
{
    public class MediaRepository
    {
        private class MediaIdComparer : IComparer<MediaInfo>
        {
            public static readonly MediaIdComparer Instance = new MediaIdComparer();
            public int Compare(MediaInfo x, MediaInfo y) => x.Id.CompareTo(y.Id);
        }

        public MediaRepository()
        {
        }

        public string OriginalJson { get; private set; }

        public string Version { get; private set; }

        public List<MediaInfo> MediaList { get; } = new List<MediaInfo>();

        public MediaInfo this[string id]
        {
            get
            {
                var query = new MediaInfo { Id = id };
                var index = MediaList.BinarySearch(query, MediaIdComparer.Instance);
                if (index < 0) return null;
                return MediaList[index];
            }
        }

        public static MediaRepository TryParse(string json)
        {
            if (json.ParseJson() is JsonPairs pairs)
            {
                var mr = TryParseMainPairs(pairs);
                if (mr != null)
                {
                    mr.OriginalJson = json;
                }
                return mr;
            }
            return null;
        }

        private static MediaRepository TryParseMainPairs(JsonPairs pairs)
        {
            if (!pairs.TryGetValue("version", out string version))
            {
                return null;
            }

            if (!pairs.TryGetNode<JsonArray>("mediaList", out var mediaList))
            {
                return null;
            }

            var mr = new MediaRepository
            {
                Version = version,
            };
            foreach (var item in mediaList.Items)
            {
                if (item is JsonPairs mediaInfo)
                {
                    var mi = MediaInfo.TryParse(mediaInfo);
                    if (mi != null)
                    {
                        mr.MediaList.Add(mi);
                    }
                }
            }
            return mr;
        }

        private void Sort() => MediaList.Sort(MediaIdComparer.Instance);

        public static int? CompareVersions(MediaRepository mr1, MediaRepository mr2)
            => CompareVersions(mr1.Version, mr2.Version);

        public static int? CompareVersions(string a, string b)
        {
            var aEmpty = string.IsNullOrWhiteSpace(a);
            var bEmpty = string.IsNullOrWhiteSpace(b);
            if (aEmpty && bEmpty) return 0;
            if (aEmpty && !bEmpty) return -1;
            if (!aEmpty && bEmpty) return 1;
            var splitA = a.Split('.');
            var splitB = b.Split('.');
            for (var i = 0; i < Math.Min(splitA.Length, splitB.Length); i++)
            {
                var segA = splitA[i];
                var segB = splitB[i];
                if (uint.TryParse(segA, out var ia) && uint.TryParse(segB, out var ib))
                {
                    var c = ia.CompareTo(ib);
                    if (c != 0) return c;
                }
                else
                {
                    return null;
                }
            }
            return splitA.Length.CompareTo(splitB.Length);
        }
    }
}
