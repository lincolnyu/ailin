﻿using System.Windows;

namespace AiLinWpf.ViewModels.Playlist
{
    public class MediaProviderLabelViewModel
    {
        public Thickness Margin { get; set; }
        public string Title { get; set; }

        public bool HasColon { get; set; }
        public string Colon => HasColon ? "：" : "";
    }
}
