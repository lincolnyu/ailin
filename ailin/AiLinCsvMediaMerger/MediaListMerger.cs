using AiLinLib.Media;
using System.Collections.Generic;

namespace AiLinCsvMediaMerger
{
    public static class MediaListMerger
    {
        /// <summary>
        ///  Merges source to target, source takes precedence
        /// </summary>
        /// <param name="source">The source</param>
        /// <param name="target">The target</param>
        /// <param name="honorSourceOrder">Whether to honor the order in source</param>
        public static void Merge(MediaRepository source, MediaRepository target,
            bool honorSourceOrder=false)
        {
            if (source.Version != null)
            {
                target.Version = source.Version;
            }
            foreach (var srcmi in source.MediaList)
            {
                var id = srcmi.Id;
                if (id == null) continue;
                if (!target.IdToInfo.TryGetValue(id, out var tgtmi))
                {
                    tgtmi = new MediaInfo { Id = id };
                    target.AddMedia(tgtmi);
                }
                if (srcmi.Title != null)
                {
                    tgtmi.Title = srcmi.Title;
                }
                if (srcmi.DateStr != null)
                {
                    tgtmi.DateStr = srcmi.DateStr;
                }
                if (srcmi.Category != null)
                {
                    tgtmi.Category = srcmi.Category;
                }
                if (srcmi.Role != null)
                {
                    tgtmi.Role = srcmi.Role;
                }
                if (srcmi.Director != null)
                {
                    tgtmi.Director = srcmi.Director;
                }
                if (srcmi.Playwright != null)
                {
                    tgtmi.Playwright = srcmi.Playwright;
                }
                if (srcmi.AdaptedFrom != null)
                {
                    tgtmi.AdaptedFrom = srcmi.AdaptedFrom;
                }
                if (srcmi.ExternalLink != null)
                {
                    tgtmi.ExternalLink = srcmi.ExternalLink;
                }
            }
            if (honorSourceOrder)
            {
                var srcIdToIndex = new Dictionary<string, int>();
                for (var i = 0; i < source.MediaList.Count; i++)
                {
                    var srcmi = source.MediaList[i];
                    srcIdToIndex[srcmi.Id] = i;
                }
                var oldTgtIdToIndex = new Dictionary<string, int>();
                for (var i = 0; i < target.MediaList.Count; i++)
                {
                    var tgtmi = target.MediaList[i];
                    oldTgtIdToIndex[tgtmi.Id] = i;
                }
                target.MediaList.Sort((a, b) =>
                {
                    if (srcIdToIndex.TryGetValue(a.Id, out var srcia)
                        && srcIdToIndex.TryGetValue(b.Id, out var srcib))
                    {
                        return srcia.CompareTo(srcib);
                    }
                    if (oldTgtIdToIndex.TryGetValue(a.Id, out var tgtia)
                        && oldTgtIdToIndex.TryGetValue(b.Id, out var tgtib))
                    {
                        return tgtia.CompareTo(tgtib);
                    }
                    if (a.DateStr != null && b.DateStr != null)
                    {
                        var c = a.DateStr.CompareTo(b.DateStr);
                        if (c != 0) return c;
                    }
                    if (a.Title != null && b.Title != null)
                    {
                        var c = a.Title.CompareTo(b.Title);
                        if (c != 0) return c;
                    }
                    return a.Id.CompareTo(b.Id);
                });
            }
        }
    }
}
