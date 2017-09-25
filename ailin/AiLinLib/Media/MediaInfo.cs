namespace AiLinLib.Media
{
    public class MediaInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
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
    }
}
