using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Net;
using System.Net.Cache;
using System.Threading;
using System.IO;

namespace AiLinWpfLib.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        ///  Download an image using BitmapImage's own downloading mechanism
        ///  By far this is the most efficient method to perform this task
        /// </summary>
        /// <param name="uri">The URI the image is located</param>
        /// <returns>The image if successful</returns>
        public static BitmapImage TryLoadImage(Uri uri)
        {
            try
            {
                var img = new BitmapImage
                {
                    CacheOption = BitmapCacheOption.None,
                    UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache)
                };

                img.BeginInit();
                img.UriSource = uri;
                img.EndInit();
                return img;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        ///  This hopefully completely loads image asynchronously. It is based on the
        ///  example provided by the reference link.
        /// </summary>
        /// <param name="uri">The URI of the image to download</param>
        /// <param name="downloadTimeoutMs">Download time out in milliseconds, which seems to be be implemented internally</param>
        /// <returns>The image</returns>
        /// <remarks>
        ///  Reference:
        ///  https://social.msdn.microsoft.com/Forums/vstudio/en-US/32af0d68-bcdc-4d79-b8ef-de2bb52f7d29/loading-a-thread-safe-bitmapimage-from-a-url?forum=wpf
        /// </remarks>
        public static async Task<BitmapImage> TryLoadImageAsync(Uri uri, int? downloadTimeoutMs = null)
        {
            return await Task.Run(
                () =>
                {
                    try
                    {
                        var webRequest = WebRequest.CreateDefault(uri);
                        webRequest.ContentType = "image/jpeg";
                        if (downloadTimeoutMs.HasValue)
                        {
                            webRequest.Timeout = downloadTimeoutMs.Value;
                        }
                        var webResponse = webRequest.GetResponse();
                        var stream = webResponse.GetResponseStream();
                        var buffer = new byte[webResponse.ContentLength];
                        // NOTE This is the only known way that is implemented by
                        // WebResponse to actually download the data properly
                        for (var i = 0; i < webResponse.ContentLength; i++)
                        {
                            var b = stream.ReadByte();
                            if (b < 0) break;
                            buffer[i] = (byte)b;
                        }
                        var img = new BitmapImage();
                        img.BeginInit();
                        img.StreamSource = new MemoryStream(buffer);
                        img.EndInit();
                        img.Freeze();
                        return img;
                    }
                    catch (Exception)
                    {
                        // We treat any exception as failure and return null image
                        // Possible exceptions include but are not limited to
                        // FileFormatException from decoding the image
                        // InvalidOperationException from bgImg.BeginInit() etc.
                        //  tcs.SetResult(null);
                        return null;
                    }
                });
        }

        /// <summary>
        ///  Download an image from web with timeout and maximum attempts (whichever
        ///  reached first). It internally calls TryLoadImageWebRequest
        /// </summary>
        /// <param name="uri">The URI where the image is located</param>
        /// <param name="downloadTotalTimeoutMs">The total time out in milliseconds</param>
        /// <param name="attempts">The maximum attempts</param>
        /// <returns>The image if successful</returns>
        public static async Task<BitmapImage> TryLoadWebImageMultiAttempts(Uri uri,
            int downloadTotalTimeoutMs = 3000, int attempts = int.MaxValue)
        {
            var start = DateTime.UtcNow;
            for (var i = 0; i < attempts; i++)
            {
                var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
                if (elapsed < downloadTotalTimeoutMs)
                {
                    var timeout = (int)Math.Ceiling(downloadTotalTimeoutMs - elapsed);
                    var img = await TryLoadImageAsync(uri, timeout);
                    if (img != null)
                    {
                        return img;
                    }
                }
                else
                {
                    break;
                }
            }
            return null;
        }

        /// <summary>
        ///  Experimental method that is aimiing to implement the article in the remark section
        ///  to move the synchronous part of image downloading out
        /// </summary>
        /// <param name="uri">The URI</param>
        /// <returns>The image if successful</returns>
        /// <remarks>
        ///  References:
        ///   https://social.msdn.microsoft.com/Forums/vstudio/en-US/9bfb66c0-6f53-4013-a0f2-f54fe39af7cf/load-a-bitmapimage-from-uri-asynchronously?forum=wpf
        /// </remarks>
        public static async Task<BitmapImage> TryLoadImageAdvanced(Uri uri)
        {
            Action primeServicePoint = () =>
            {
                ServicePointManager.FindServicePoint(
                uri, WebRequest.DefaultWebProxy);
            };

            var tcs = new TaskCompletionSource<bool>();

            AsyncCallback downloadImage = ar =>
            {
                primeServicePoint.EndInvoke(ar);
                tcs.SetResult(true);     
            };

            primeServicePoint.BeginInvoke(downloadImage, null);

            var img = new BitmapImage
            {
                CacheOption = BitmapCacheOption.None,
                UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache)
            };
            
            return await tcs.Task.ContinueWith(b =>
            {
                if (b.Exception != null || !b.Result)
                {
                    return null;
                }
                img.Dispatcher.Invoke(() =>
                {
                    img.BeginInit();
                    img.UriSource = uri;
                    img.EndInit();
                });
                return img;
            });
        }
    }
}
