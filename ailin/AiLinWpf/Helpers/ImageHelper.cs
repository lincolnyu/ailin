#define ADVANCED_IMAGE_DOWNLOADING
using System;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
#if ADVANCED_IMAGE_DOWNLOADING
using System.Net;
#endif

namespace AiLinWpf.Helpers
{
    public static class ImageHelper
    {
        public static async Task<BitmapImage> TryLoadWebImageMultAttempts(string webUri,
            int downloadTotalTimeoutMs = 3000, int attempts = int.MaxValue)
        {
            BitmapImage result = null;
            var start = DateTime.UtcNow;
            for (var i = 0; i < attempts; i++)
            {
                var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                if (elapsed < downloadTotalTimeoutMs)
                {
                    var timeout = (int)Math.Ceiling(downloadTotalTimeoutMs - elapsed);
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
            string fallbackUri = "pack://application:,,,/Images/fallback.gif", 
            int downloadTimeoutMs = 10000, int attempts = int.MaxValue)
        {
            var fallback = new BitmapImage(new Uri(fallbackUri));
            await image.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() => image.Source = fallback));
            var img = await TryLoadWebImageMultAttempts(webUri, downloadTimeoutMs, attempts);
            if (img != null)
            {
                await image.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() => image.Source = img));
                return webUri;
            }
            else
            {
                return fallbackUri;
            }
        }

        private static async Task<BitmapImage> TryLoadWebImage(string uri, int downloadTimeoutMs, 
            string contentType = "image/jpeg")
        {
            try
            {
                System.Diagnostics.Trace.WriteLine($"Image {uri} download started");
#if ADVANCED_IMAGE_DOWNLOADING
                var tcs = new TaskCompletionSource<bool>();
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
                img.DownloadFailed += (s, e) => tcs.SetResult(false);
                img.DownloadCompleted += (s, e) => tcs.SetResult(true);
                img.BeginInit();
                img.StreamSource = webResponse.GetResponseStream();
                img.EndInit();

                var res = await tcs.Task;
                System.Diagnostics.Trace.WriteLine($"Image {uri} download succeeded: {res}");
                return res ? img : null;
#else                
                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(uri);
                img.EndInit();
                return img;
#endif
            }
            catch (Exception)
            {
                System.Diagnostics.Trace.WriteLine($"Image {uri} download encountered an exception");
                return null;
            }
        }
    }
}
