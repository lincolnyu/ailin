using AiLinWpf.Sources;
using AiLinWpf.Styles;
using System;
using System.Windows.Controls;

namespace AiLinWpf.Actions
{
    public class Resource : MediaInfo
    {
        public enum Types
        {
            Uncategorized,
            Movie,      // 电影
            Television, // 电视剧
            RadioDrama, // 广播剧
            Recite,     // 朗诵
            Interview,  // 访谈
            Show        // 综艺
        }

        public DateTime Date { get; set; }
        public Types Type { get; set; }

        public ListBoxItem UI { get; set; }

        public void ColorAsPerType()
        {
            switch (Type)
            {
                case Types.Movie:
                    UI.Background = Coloring.PaleGoldenrodBrush;
                    break;
                case Types.Television:
                    UI.Background = Coloring.LightSkyBlueBrush;
                    break;
            }
        }
    }
}
