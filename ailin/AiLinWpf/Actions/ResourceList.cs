using AiLinWpf.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;

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

        private static TextBlock GetFirstTextBlock(Panel panel)
        {
            var c = panel.Children[0];
            if (c is TextBlock tb)
            {
                return tb;
            }
            else if (c is Panel p)
            {
                return GetFirstTextBlock(p);
            }
            return null;
        }

        public void ResyncFromUI()
        {
            Resources.Clear();
            foreach (var lbi in VideoList.Items.Cast<ListBoxItem>())
            {
                var tag = lbi.Tag;
                var c = lbi.Content;
                string title = null;
                if (c is Panel panel)
                {
                    var tb = GetFirstTextBlock(panel);
                    if (tb == null)
                    {
                        throw new NotSupportedException();
                    }
                    if (tb.Inlines.FirstInline is Hyperlink hl)
                    {
                        title = hl.Inlines.GetText();
                    }
                    else
                    {
                        title = tb.Text;
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
                var r = new Resource
                {
                    Title = title,
                    UI = lbi
                };
                TryDeduceFromType(title, r);
                TryParseTag(tag, r);
                r.ColorAsPerType();
                Resources.Add(r);
            }
        }

        private void TryDeduceFromType(string title, Resource r)
        {
            if (title.Contains("电影）"))
            {
                r.Type = Resource.Types.Movie;
            }
            else if (title.Contains("电视剧）"))
            {
                r.Type = Resource.Types.Television;
            }
            else
            {
                r.Type = Resource.Types.Uncategorized;
            }

            ExtractDate(title, "（([0-9]+)年", null, r);
        }

        private void TryParseTag(object tag, Resource r)
        {
            if (tag is string s)
            {
                var split = s.Split(';');
                var sdate = split[0];
                ExtractDate(sdate, "([0-9]+)年", "([0-9]+)月", r);
            }
        }

        private void ExtractDate(string s, string patternYear, string patternMonth, Resource r)
        {
            var rexYear = new Regex(patternYear);
            var mYear = rexYear.Match(s);
            if (mYear.Success)
            {
                var ystr = mYear.Groups[1].Value;
                if (int.TryParse(ystr, out int year))
                {
                    var dateSet = false;
                    if (patternMonth != null)
                    {
                        var rexMonth = new Regex(patternMonth);
                        var mMonth = rexMonth.Match(s);
                        if (mMonth.Success)
                        {
                            var mstr = mMonth.Groups[1].Value;
                            if (int.TryParse(mstr, out int month) && month > 1 && month <= 12)
                            {
                                r.Date = new DateTime(year, month, 1);
                                dateSet = true;
                            }
                        }
                    }
                    if (!dateSet)
                    {
                        r.Date = new DateTime(year, 1, 1);
                    }
                }
            }
        }
    }
}
