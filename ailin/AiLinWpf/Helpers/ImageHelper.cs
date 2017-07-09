using System;
using System.Windows.Media.Imaging;

namespace AiLinWpf.Helpers
{
    public static class ImageHelper
    {
        public static BitmapImage TryLoadImage(string uri, string fallbackUri = "pack://application:,,,/Images/fallback.gif", int attempts = 3)
        {
            BitmapImage img = null;
            for (var i = 0; i < attempts; i++)
            {
                try
                {
                    img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(uri);
                    img.EndInit();
                    break;
                }
                catch (Exception)
                {
                    img = null;
                }
            }
            if (img == null)
            {
                img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(fallbackUri);
                img.EndInit();
            }
            return img;
        }
    }
}
