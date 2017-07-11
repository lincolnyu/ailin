﻿using System;
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
        public static Comparison<Resource> CompareTitleAscending => (x, y) => x.Title.CompareTo(y.Title);
        public static Comparison<Resource> CompareTitleDescending => (x, y) => y.Title.CompareTo(x.Title);
        public static Comparison<Resource> CompareDateAscending => (x, y) => x.Date.CompareTo(y.Date);
        public static Comparison<Resource> CompareDateDescending => (x, y) => y.Date.CompareTo(x.Date);

        private bool AreOnSame(Comparison<Resource> x, Comparison<Resource> y)
            => x == y ||
            x == CompareTypeAscending && y == CompareTypeDescending || x == CompareTypeDescending && y == CompareTypeAscending ||
            x == CompareTitleAscending && y == CompareTitleDescending || x == CompareTitleDescending && y == CompareTitleAscending ||
            x == CompareDateAscending && y == CompareDateDescending || x == CompareDateDescending && y == CompareDateAscending;


        private int CompareForSortByTypeAscending(Resource x, Resource y)
        {
            var c = x.Type.CompareTo(y.Type);
            if (c != 0) return c;
            c = x.Date.CompareTo(y.Date);
            if (c != 0) return c;
            return string.Compare(x.Title, y.Title, StringComparison.CurrentCulture);
        }

        private int CompareForSortByTypeDescending(Resource x, Resource y)
        {
            var c = -x.Type.CompareTo(y.Type);
            if (c != 0) return c;
            c = x.Date.CompareTo(y.Date);
            if (c != 0) return c;
            return string.Compare(x.Title, y.Title, StringComparison.CurrentCulture);
        }

        private int CompareForSortByTitleAscending(Resource x, Resource y)
        {
            var c = string.Compare(x.Title, y.Title, StringComparison.CurrentCulture);
            if (c != 0) return c;
            c = x.Type.CompareTo(y.Type);
            if (c != 0) return c;
            return x.Date.CompareTo(y.Date);
        }

        private int CompareForSortByTitleDescending(Resource x, Resource y)
        {
            var c = -string.Compare(x.Title, y.Title, StringComparison.CurrentCulture);
            if (c != 0) return c;
            c = x.Type.CompareTo(y.Type);
            if (c != 0) return c;
            return x.Date.CompareTo(y.Date);
        }

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
                var tag = lbi.Tag;
                var c = lbi.Content;
                string title = null;
                if (c is StackPanel sp)
                {
                    var top = sp.Children[0];
                    if (top is TextBlock tb)
                    {
                        if (tb.Inlines.FirstInline is Hyperlink hl)
                        {
                            var run = hl.Inlines.FirstOrDefault() as Run;
                            title = run == null ? string.Empty : run.Text;
                        }
                        else
                        {
                            title = tb.Text;
                        }
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
                r.Type = Resource.Types.Series;
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
