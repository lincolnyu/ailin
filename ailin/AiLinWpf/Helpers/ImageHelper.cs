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
            int downloadTotalTimeoutMs = 3000)
        {
            BitmapImage result = null;
            var start = DateTime.UtcNow;
            for (; ; )
            {
                var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                if (downloadTotalTimeoutMs > elapsed)
                {
                    var timeout = (int)Math.Floor(downloadTotalTimeoutMs - elapsed);
                    result = await TryLoadWebImage(webUri, timeout);
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        public static async Task<string> TryLoadWebImage(this Image image, string webUri,
            string fallbackUri = "pack://application:,,,/Images/fallback.gif", int downloadTimeoutMs = 10000)
        {
            var img = await TryLoadWebImageMultAttempts(webUri, downloadTimeoutMs);
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

        private static async Task<BitmapImage> TryLoadWebImage(string uri, int downloadTimeoutMs, 
            string contentType = "image/jpeg")
        {
            try
            {
                var webRequest = WebRequest.CreateDefault(new Uri(uri));
                if (contentType != null)
                {
                    webRequest.ContentType = "image/jpeg";
                }

                webRequest.Timeout = downloadTimeoutMs;
                var webResponse = webRequest.GetResponse();
                var img = new BitmapImage()
                {
                    CreateOptions = BitmapCreateOptions.None,
                    CacheOption = BitmapCacheOption.OnLoad
                };
                var tcs = new TaskCompletionSource<BitmapImage>();
                img.DownloadFailed += (s, e) => tcs.SetResult(null);
                img.DownloadCompleted += (s, e) => tcs.SetResult(img);
                img.BeginInit();
                img.StreamSource = webResponse.GetResponseStream();
                img.EndInit();

                if (!img.IsDownloading)
                {
                    return img;
                }

                return await tcs.Task;
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
