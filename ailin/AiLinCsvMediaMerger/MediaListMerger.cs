using AiLinLib.Media;

namespace AiLinCsvMediaMerger
{
    public static class MediaListMerger
    {
        /// <summary>
        ///  Merges source to target, source takes precedence
        /// </summary>
        /// <param name="source">The source</param>
        /// <param name="target">The target</param>
        public static void Merge(MediaRepository source, MediaRepository target)
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
        }
    }
}
