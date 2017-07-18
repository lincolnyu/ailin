using System;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Net;

namespace AiLinWpf.Helpers
{
    public static class ImageHelper
    {
        public static async Task<BitmapImage> TryLoadWebImageMultAttempts(string webUri,
            int attempts = 3, int downloadTimeoutMs = 3000)
        {
            const int downloadPolling = 200;
            int downloadTimeoutCount = downloadTimeoutMs / downloadPolling;
            BitmapImage result = null;
            for (var i = 0; i < attempts && result == null; i++)
            {
                result = await TryLoadWebImage(webUri, downloadPolling, downloadTimeoutCount);
            }
            return result;
        }

        public static async Task<string> TryLoadWebImage(this Image image, string webUri,
            string fallbackUri = "pack://application:,,,/Images/fallback.gif",
            int attempts = 3, int downloadTimeoutMs = 5000)
        {
            var img = await TryLoadWebImageMultAttempts(webUri, attempts, downloadTimeoutMs);
            var imageUri = webUri;
            if (img == null)
            {
                imageUri = fallbackUri;
                img = new BitmapImage(new Uri(fallbackUri));
            }

            await image.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() => image.Source = img));
            return imageUri;
        }

        private static async Task<BitmapImage> TryLoadWebImage(string uri, int downloadPolling, 
            int downloadTimeoutCount = int.MaxValue, string contentType = "image/jpeg")
        {
            try
            {
                var webRequest = WebRequest.CreateDefault(new Uri(uri));
                if (contentType != null)
                {
                    webRequest.ContentType = "image/jpeg";
                }

                var infiniteTimeout = downloadTimeoutCount >= int.MaxValue / downloadPolling;
                var totalTimeout = infiniteTimeout ? System.Threading.Timeout.Infinite 
                    : downloadPolling * downloadTimeoutCount;
                
                webRequest.Timeout = totalTimeout;
                var initTime = DateTime.UtcNow;
                var webResponse = webRequest.GetResponse();
                var img = new BitmapImage()
                {
                    CreateOptions = BitmapCreateOptions.None,
                    CacheOption = BitmapCacheOption.OnLoad
                };
                var downloadFailed = false;
                var downloadCompleted = false;
                img.DownloadFailed += (s, e) => downloadFailed = true;
                img.DownloadCompleted += (s, e) => downloadCompleted = true;
                img.BeginInit();
                img.StreamSource = webResponse.GetResponseStream();
                img.EndInit();

                if (!img.IsDownloading)
                {
                    return img;
                }

                for (var i = 0; i < downloadTimeoutCount && !downloadCompleted && !downloadFailed; i++)
                {
                    var delayTime = downloadPolling;
                    if (!infiniteTimeout)
                    {
                        var elapsed = DateTime.UtcNow - initTime;
                        var left = (int)(totalTimeout - elapsed.TotalMilliseconds);
                        if (left <= 0) break;
                        if (left < delayTime) delayTime = left;
                    }
                    await Task.Delay(delayTime);
                }
                return downloadCompleted? img : null;
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
