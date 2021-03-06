﻿namespace WebKit
{
    public class PageInfo
    {
        public string PageUrl { get; set; }
        public string ProfileUrl { get; set; }
        public int PageId { get; set; }
        public string PageContent { get; set; }
        public string Id { get; set; }
        public int? Votes { get; set; }
        public int? Popularity { get; set; }
        public int? Rank { get; set; }
        public string Thumbnail { get; set; }

        public Question Question { get; set; }
    }
}
