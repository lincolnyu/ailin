using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AiLinWpfLib.Helpers;

namespace AiLinWpf.Helpers
{
    public static class ImageLoadingHelper
    {
        public static async Task<string> TryLoadWebImage(this Image image, string webUri,
                string fallbackUri = "pack://application:,,,/Images/fallback.gif",
                int downloadTimeoutMs = 10000, int attempts = int.MaxValue)
        {
            var img = await ImageHelper.TryLoadWebImageMultiAttempts(new Uri(webUri), 
                downloadTimeoutMs, attempts);
            if (img != null)
            {
                await image.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() => image.Source = img));
                return webUri;
            }
            else
            {
                var fallback = new BitmapImage(new Uri(fallbackUri));
                await image.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                   new Action(() => image.Source = fallback));
                return fallbackUri;
            }
        }
    }
}
