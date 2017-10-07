using System.Windows;

namespace AiLinWpf.ViewModels.Playlist
{
    public class TrackViewModel
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public bool HasUrl => !string.IsNullOrWhiteSpace(Url);

        public Thickness Margin { get; set; }
    }
}
