using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace AiLinWpf.Actions
{
    public class Resource
    {
        public enum Types
        {
            Uncategorized,
            Movie,
            Series,
            RadioDrama,
            Recite,
            Interview,
            Show
        }

        public DateTime Date { get; set; }
        public Types Type { get; set; }
        public string Title { get; set; }

        public ListBoxItem UI { get; set; }

        public void ColorAsPerType()
        {
            switch (Type)
            {
                case Types.Movie:
                    UI.Background = new SolidColorBrush(Colors.Silver);
                    break;
                case Types.Series:
                    UI.Background = new SolidColorBrush(Colors.LightSkyBlue);
                    break;
            }
        }
    }
}
