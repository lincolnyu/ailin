using AiLinLib.Media;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AiLinWpf.Sources
{
    public class MediaRepoManager
    {
        public enum RefreshResults
        {
            AlreadyLatest,
            Refreshed,
            FailedToDownload
        }

        public MediaRepoManager(string sourceUrl, string tempFileName)
        {
            SourceUrl = sourceUrl;
            TempFileName = tempFileName;
        }

        public string SourceUrl { get; }
        public string TempFileName { get; }
        public MediaRepository Current { get; private set; }

        public async Task Initialize(bool resetToDefault = false)
        {
            if (resetToDefault)
            {
                Reset();
            }
            else
            {
                Current = await Load();
            }
            if (Current == null)
            {
                await ResetToDefault();
            }
        }

        public void Reset()
        {
            Current = null;
            using (var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                isoStore.DeleteFile(TempFileName);
            }
        }

        public async Task ResetToDefault()
        {
            Current = await LoadDefault();
            if (Current != null)
            {
                await Save();
            }
        }

        public async Task<RefreshResults> Refresh()
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
                return toUpdate ? RefreshResults.Refreshed : RefreshResults.AlreadyLatest;
            }
            return RefreshResults.FailedToDownload;
        }

        private async Task<MediaRepository> Download()
        {
            try
            {
                var client = new WebClient
                {
                    Encoding = Encoding.UTF8
                };
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
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
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

        private async Task<MediaRepository> LoadDefault()
        {
            try
            {
                using (var fs = new FileStream(@"Jsons\media_init.json", FileMode.Open))
                using (var sr = new StreamReader(fs))
                {
                    var json = await sr.ReadToEndAsync();
                    var mr = MediaRepository.TryParse(json);
                    return mr;
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
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    await sw.WriteAsync(Current.OriginalJson);
                }
            }
        }
    }
}
