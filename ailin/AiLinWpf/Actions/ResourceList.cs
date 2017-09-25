using AiLinWpf.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace AiLinWpf.Actions
{
    public class ResourceList
    {
        public MainWindow MainWindow { get; }

        public ListBox VideoList => MainWindow.VideoList;
        public List<Resource> Resources { get; } = new List<Resource>();

        public ResourceList(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            ResyncFromUI();
        }

        public List<Comparison<Resource>> Comparisons = new List<Comparison<Resource>>();

        public static Comparison<Resource> CompareTypeAscending => (x, y) => x.Type.CompareTo(y.Type);
        public static Comparison<Resource> CompareTypeDescending => (x, y) => y.Type.CompareTo(x.Type);
        public static Comparison<Resource> CompareTitleAscending => (x, y) => ChineseHelper.Compare(x.Title, y.Title);
        public static Comparison<Resource> CompareTitleDescending => (x, y) => ChineseHelper.Compare(y.Title, x.Title);
        public static Comparison<Resource> CompareDateAscending => (x, y) => x.Date.CompareTo(y.Date);
        public static Comparison<Resource> CompareDateDescending => (x, y) => y.Date.CompareTo(x.Date);

        private bool AreOnSame(Comparison<Resource> x, Comparison<Resource> y)
            => x == y ||
            x == CompareTypeAscending && y == CompareTypeDescending || x == CompareTypeDescending && y == CompareTypeAscending ||
            x == CompareTitleAscending && y == CompareTitleDescending || x == CompareTitleDescending && y == CompareTitleAscending ||
            x == CompareDateAscending && y == CompareDateDescending || x == CompareDateDescending && y == CompareDateAscending;

        public void Push(Comparison<Resource> c)
        {
            for (var i = 0; i < Comparisons.Count; i++)
            {
                var x = Comparisons[i];
                if (AreOnSame(x, c))
                {
                    Comparisons.RemoveAt(i);
                    break;
                }
            }
            Comparisons.Insert(0, c);
            Sort();
        }

        private int Compare(Resource x, Resource y)
        {
            foreach (var c in Comparisons)
            {
                var cr = c(x, y);
                if (cr != 0) return cr;
            }
            return 0;
        }

        public void Sort()
        {
            Resources.Sort(Compare);
            VideoList.Items.Clear();
            foreach (var r in Resources)
            {
                VideoList.Items.Add(r.UI);
            }
        }

        public void ResyncFromUI()
        {
            Resources.Clear();
            foreach (var lbi in VideoList.Items.Cast<ListBoxItem>())
            {
                var sync = new MediaUiSyncer(lbi, null);
                sync.Pull();
                var r = sync.Resource;
                Resources.Add(r);
            }
        }

        public void InjectToUI()
        {
            foreach (var r in Resources)
            {
                var lbi = r.UI;
                var mus = new MediaUiSyncer(lbi, r);
                mus.Push();
            }
        }        
    }
}
