using AiLinLib.Media;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading.Tasks;

namespace AiLinWpf.Sources
{
    public class MediaRepoManager
    {
        public MediaRepoManager(string sourceUrl, string tempFileName)
        {
            SourceUrl = sourceUrl;
            TempFileName = tempFileName;
        }

        public string SourceUrl { get; }
        public string TempFileName { get; }
        public MediaRepository Current { get; private set; }

        public async Task Initialize()
        {
            Current = await Load();
        }

        public async Task Refresh()
        {
            var repo = await Download();
            if (repo != null)
            {
                var toUpdate = Current == null;
                if (!toUpdate)
                {
                    var c = MediaRepository.CompareVersions(Current, repo);
                    toUpdate = c != null && c < 0;
                }
                if (toUpdate)
                {
                    Current = repo;
                    // TODO should try mutliple times?
                    await Save();
                }
            }
        }

        private async Task<MediaRepository> Download()
        {
            try
            {
                var client = new WebClient();
                var json = await client.DownloadStringTaskAsync(SourceUrl);
                return MediaRepository.TryParse(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<MediaRepository> Load()
        {
            try
            {
                using (var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
                {
                    var fs = isoStore.OpenFile(TempFileName, FileMode.Open);
                    using (var sr = new StreamReader(fs))
                    {
                        var json = await sr.ReadToEndAsync();
                        var mr = MediaRepository.TryParse(json);
                        return mr;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task Save()
        {
            using (var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                var fs = isoStore.CreateFile(TempFileName);
                using (var sw = new StreamWriter(fs))
                {
                    await sw.WriteAsync(Current.OriginalJson);
                }
            }
        }
    }
}
