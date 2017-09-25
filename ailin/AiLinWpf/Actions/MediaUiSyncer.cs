using AiLinWpf.Helpers;
using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;

namespace AiLinWpf.Actions
{
    /// <summary>
    ///  This class knows the current layout of the listbox item and syncs the resource data with it.
    /// </summary>
    /// <remarks>
    ///  This can be temporary and will be removed once a full MVVM pattern is used (e.g. for ver 1.8).
    /// </remarks>
    public class MediaUiSyncer
    {
        public MediaUiSyncer(ListBoxItem lbi, Resource r)
        {
            ListBoxItem = lbi;
            Resource = r;
        }

        public ListBoxItem ListBoxItem { get; }
        public Resource Resource { get; private set; }

        public void Sync()
        {
            Pull();
            Push();
        }

        public void Pull()
        {
            var tag = ListBoxItem.Tag;
            var id = ListBoxItem.Name;
            var c = ListBoxItem.Content;
            string title = null;
            if (c is Panel panel)
            {
                title = GetTitle(panel);
            }
            else
            {
                throw new NotSupportedException();
            }
            if (Resource == null)
            {
                Resource = new Resource();
            }
            Resource.Title = title;
            Resource.UI = ListBoxItem;
            if (!string.IsNullOrWhiteSpace(id))
            {
                Resource.Id = id;
            }
            TryDeduceTypeFromTitle(title, Resource);
            TryParseTag(tag, Resource);
            Resource.ColorAsPerType();
        }

        public void Push()
        {

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

        private string GetTitle(Panel panel)
        {
            var tb = GetFirstTextBlock(panel);
            if (tb == null)
            {
                throw new NotSupportedException();
            }
            if (tb.Inlines.FirstInline is Hyperlink hl)
            {
                return hl.Inlines.GetText();
            }
            else
            {
                return tb.Text;
            }
        }

        private void TryDeduceTypeFromTitle(string title, Resource r)
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
