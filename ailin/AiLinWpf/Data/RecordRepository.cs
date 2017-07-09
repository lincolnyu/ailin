using System.IO;
using System.IO.IsolatedStorage;

namespace AiLinWpf.Data
{
    public class RecordRepository
    {
        public Record Last;
        public int VoteId;
        public string FileName;

        public RecordRepository(int id)
        {
            VoteId = id;
            FileName = $"records_{id}txt";
        }

        public void Save()
        {
            using (var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                var fs = isoStore.CreateFile(FileName);
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine("1.0"); // version
                    if (Last != null)
                    {
                        sw.WriteLine(Last.ToString());
                    }
                }
            }
        }

        public bool Load()
        {
            Last = null;
            using (var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                if (!isoStore.FileExists(FileName))
                {
                    return false;
                }
                var fs = isoStore.OpenFile(FileName, FileMode.Open);
                using (var sr = new StreamReader(fs))
                {
                    var line = sr.ReadLine();
                    if (line == null) return false;
                    if (line.Trim() != "1.0") return false; // unsupported
                    if (sr.EndOfStream) return false;
                    line = sr.ReadLine();
                    if (line == null) return false;
                    Last = Record.ReadFromLine(line);
                    return Last != null;
                }
            }
        }
    }

}
