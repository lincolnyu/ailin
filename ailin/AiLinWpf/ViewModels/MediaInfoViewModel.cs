using System;
using System.Windows.Media;
using System.Globalization;
using System.Text;
using System.Collections.ObjectModel;
using AiLinLib.Media;
using AiLinWpf.Styles;
using AiLinWpf.ViewModels.Playlist;
using AiLinWpf.ViewModels.SourcesRemarks;

namespace AiLinWpf.ViewModels
{
    public class MediaInfoViewModel
    {
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

        public string Year => DateStr?.Substring(0, 4)??"";

        public int TypeId { get; private set; }
        public string TypeStr { get; private set; }

        public string Subtitle { get; private set; }

        public string BriefDescription { get; private set; }

        public ObservableCollection<object> MediaSourceItems { get; } = new ObservableCollection<object>();

        #endregion

        public DateTime Date { get; private set; }

        public Brush Background { get; private set; }
        public bool BackgroundUpdatedToUI { get; set; }

        private void YieldSubtitle()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(Year))
            {
                sb.Append($"{Year}年");
            }
            if (!string.IsNullOrWhiteSpace(TypeStr))
            {
                sb.Append(TypeStr);
            }
            Subtitle = sb.ToString();
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
                case "film":
                case "movie":
                    TypeStr = "电影";
                    Background = Coloring.PaleGoldenrodBrush;
                    TypeId = 1;
                    break;
                case "short":
                    TypeStr = "微电影";
                    Background = Coloring.PaleGoldenrodBrush;
                    TypeId = 1;
                    break;
                case "tv":
                case "television":
                    TypeStr = "电视剧";
                    Background = Coloring.LightSkyBlueBrush;
                    TypeId = 2;
                    break;
                case "radio drama":
                    TypeStr = "广播剧";
                    TypeId = 3;
                    break;
                case "recite":
                    TypeStr = "朗诵";
                    TypeId = 4;
                    break;
                case "interview":
                    TypeStr = "访谈";
                    TypeId = 5;
                    break;
                case "show":
                    TypeStr = "综艺";
                    TypeId = 6;
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
            catch (NullReferenceException)
            {
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
                var hasColon = source.Playlist.Count > 0;
                var title = source.Name.TrimEnd('：');
                var url = source.Target;
                MediaProviderLabelViewModel mp;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    mp = new MediaProviderWithUrlViewModel
                    {
                        Title = title, Url = url,
                        Margin = Layouts.GenerateLeftJustified(Layouts.StandardSeparationMarginThickness),
                        HasColon = hasColon
                    };
                }
                else
                {
                    mp = new MediaProviderLabelViewModel
                    {
                        Title = title,
                        Margin = Layouts.GenerateLeftJustified(Layouts.StandardSeparationMarginThickness),
                        HasColon = hasColon
                    };
                }
                MediaSourceItems.Add(mp);
                var checkConsecutiveness = CheckConsecutiveness();
                int? lastI = null;
                for (i++ ; i < source.Playlist.Count; i++)
                {
                    var t = source.Playlist[i];
                    var tt = t.Item1;
                    if (checkConsecutiveness)
                    {
                        if (int.TryParse(tt, out var itt))
                        {
                            var inconsecutive = lastI.HasValue && lastI + 1 != itt;
                            if (inconsecutive)
                            {
                                MediaSourceItems.Add(EllipsisViewModel.Instance);
                            }
                            lastI = itt;
                        }
                    }
                    var tvm = new TrackViewModel { Title = tt, Url = t.Item2 };
                    tvm.Margin = Layouts.GenerateLeftJustified(isLast()? 
                        Layouts.StandardSeparationMarginThickness : Layouts.StandardTrackItemMarginThickness);
                    MediaSourceItems.Add(tvm);
                }
            }

            if (Model.SourcesRemarks?.Contains("<") == true)
            {
                var xsrvm = XamlSourceRemarksViewModel.TryParseBlock(Model.SourcesRemarks);
                if (xsrvm != null)
                {
                    xsrvm.Margin = Layouts.GenerateLeftJustified(Layouts.StandardSeparationMarginThickness);
                    MediaSourceItems.Add(xsrvm);
                }
            }
            else
            {
                var gsrvm = CommonSourceRemarksViewModel.TryParse(Model.SourcesRemarks);
                if (gsrvm != null)
                {
                    gsrvm.Margin = Layouts.GenerateLeftJustified(Layouts.StandardSeparationMarginThickness);
                    MediaSourceItems.Add(gsrvm);
                }
            }
        }

        private bool CheckConsecutiveness()
            => Model.SourcesRemarks?.Contains("剧集不全")?? false;
    }
}
