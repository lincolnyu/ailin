using System;
using System.Windows.Media;
using System.Globalization;
using System.Text;
using System.Collections.ObjectModel;
using AiLinLib.Media;
using AiLinWpf.Styles;
using AiLinWpf.ViewModels.Playlist;
using AiLinWpf.ViewModels.SourcesMissing;

namespace AiLinWpf.ViewModels
{
    public class MediaInfoViewModel
    {
        public enum Types
        {
            Uncategorized,
            Movie,      // 电影
            Television, // 电视剧
            RadioDrama, // 广播剧
            Recite,     // 朗诵
            Interview,  // 访谈
            Show        // 综艺
        }

        public MediaInfoViewModel(MediaInfo model)
        {
            Model = model;
            SetType();
            SetDate();
            YieldSubtitle();
            YieldBriefDescription();
            YieldPlaylists();
        }

        public MediaInfo Model { get; }

        #region Fields directly from Model

        public string Id => Model.Id;
        public string Title => Model.Title;
        public string ExternalLink => Model.ExternalLink?? "";
        public string DateStr => Model.DateStr;
        public string Category => Model.Category;
        public string Role => Model.Role;
        public string Director => Model.Director;
        public string Playwright => Model.Playwright;
        public string Producer => Model.Producer;
        public string AdaptedFrom => Model.AdaptedFrom;
        public string Remarks => Model.Remarks;

        #endregion

        #region Deduced properties

        public string Year => DateStr.Substring(0, 4);

        public string TypeStr { get; private set; }

        public string Subtitle { get; private set; }

        public string BriefDescription { get; private set; }

        public ObservableCollection<object> MediaSourceItems { get; } = new ObservableCollection<object>();

        #endregion

        public DateTime Date { get; private set; }

        public Types Type { get; private set; }

        public Brush Background { get; private set; }
        public bool BackgroundUpdatedToUI { get; set; }

        private void YieldSubtitle()
        {
            Subtitle = $"{Year}年{TypeStr}";
        }

        private void YieldBriefDescription()
        {
            var sb = new StringBuilder();
            var hasPreceding = false;
            if (Model.Role != null)
            {
                sb.Append($"饰演{Model.Role}。");
            }
            if (Model.AdaptedFrom != null)
            {
                sb.Append($"根据{Model.AdaptedFrom}改编");
                hasPreceding = true;
            }
            if (Model.Playwright != null)
            {
                if (hasPreceding)
                {
                    sb.Append("，");
                }
                sb.Append($"编剧：{Model.Playwright}");
                hasPreceding = true;
            }
            if (Model.Director != null)
            {
                if (hasPreceding)
                {
                    sb.Append("，");
                }
                sb.Append($"导演：{Model.Director}");
                hasPreceding = true;
            }
            if (Model.Producer != null)
            {
                if (hasPreceding)
                {
                    sb.Append("。");
                }
                sb.Append($"{Model.Producer}");
                hasPreceding = true;
            }
            if (hasPreceding)
            {
                sb.Append("。");
            }
            BriefDescription = sb.ToString();
        }

        private void SetType()
        {
            Background = Coloring.Transparent;
            switch (Model.Category)
            {
                case "movie":
                    Type = Types.Movie;
                    TypeStr = "电影";
                    Background = Coloring.PaleGoldenrodBrush;
                    break;
                case "tv":
                case "television":
                    Type = Types.Television;
                    TypeStr = "电视剧";
                    Background = Coloring.LightSkyBlueBrush;
                    break;
                case "radio drama":
                    Type = Types.RadioDrama;
                    TypeStr = "广播剧";
                    break;
                case "recite":
                    Type = Types.Recite;
                    TypeStr = "朗诵";
                    break;
                case "interview":
                    Type = Types.Interview;
                    TypeStr = "访谈";
                    break;
                case "show":
                    Type = Types.Show;
                    TypeStr = "综艺";
                    break;
            }
        }

        private void SetDate()
        {
            try
            {
                if (Model.DateStr.Length >= 8)
                {
                    Date = DateTime.ParseExact(Model.DateStr, "yyyyMMdd", CultureInfo.InvariantCulture);
                }
                else if (Model.DateStr.Length >= 6)
                {
                    Date = DateTime.ParseExact(Model.DateStr, "yyyyMM", CultureInfo.InvariantCulture);
                }
                else if (Model.DateStr.Length >= 4)
                {
                    Date = DateTime.ParseExact(Model.DateStr, "yyyy", CultureInfo.InvariantCulture);
                }
            }
            catch (ArgumentException)
            {
            }
        }

        private void YieldPlaylists()
        {
            MediaSourceItems.Clear();
            foreach (var source in Model.Sources)
            {
                var i = -1;
                Func<bool> isLast = () => i == source.Playlist.Count-1;
                var title = source.Playlist.Count > 0 ? source.Name + ":" : source.Name;
                var url = source.Target;
                MediaProviderLabelViewModel mp;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    mp = new MediaProviderWithUrlViewModel
                    {
                        Title = title, Url = url,
                        Margin = Layouts.GenerateLeftJustified(Layouts.StandardSeparationMarginThickness)
                    };
                }
                else
                {
                    mp = new MediaProviderLabelViewModel
                    {
                        Title = title,
                        Margin = Layouts.GenerateLeftJustified(Layouts.StandardSeparationMarginThickness)
                    };
                }
                MediaSourceItems.Add(mp);
                int? lastI = null;
                for (i++ ; i < source.Playlist.Count; i++)
                {
                    var t = source.Playlist[i];
                    var tt = t.Item1;
                    if (int.TryParse(tt, out var itt))
                    {
                        var inconsecutive = lastI.HasValue && lastI + 1 != itt;
                        if (inconsecutive)
                        {
                            MediaSourceItems.Add(EllipsisViewModel.Instance);
                        }
                        lastI = itt;
                    }
                    var tvm = new TrackViewModel { Title = tt, Url = t.Item2 };
                    tvm.Margin = Layouts.GenerateLeftJustified(isLast()? 
                        Layouts.StandardSeparationMarginThickness : Layouts.StandardTrackItemMarginThickness);
                    MediaSourceItems.Add(tvm);
                }
            }

            GeneralMissingViewModel missing = null;
            if (Model.SourcesMissing == "true")
            {
                missing = GeneralMissingViewModel.GenerateCommon();
            }
            else if (!string.IsNullOrWhiteSpace(Model.SourcesMissing))
            {
                missing = GeneralMissingViewModel.Parse(Model.SourcesMissing);
            }
            if (missing != null)
            {
                missing.Margin = Layouts.GenerateLeftJustified(Layouts.StandardSeparationMarginThickness);
                MediaSourceItems.Add(missing);
            }
        }
    }
}
