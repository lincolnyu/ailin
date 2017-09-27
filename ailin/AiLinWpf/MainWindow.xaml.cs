using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using System;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading.Tasks;
using AiLinWpf.Data;
using AiLinWpf.ViewModels;
using System.Linq;
using AiLinWpf.Helpers;
using System.Windows.Input;
using static AiLinWpf.Helpers.ImageHelper;
using AiLinWpf.Sources;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Controls.Primitives;

namespace AiLinWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum MediaStatuses
        {
            Stopped,
            Opened,
            Ended,
            Failed
        }

        private class SortingSaver : IDisposable
        {
            private MainWindow _mainWindow;
            private bool _wasSorting;
            private object _selectedItem;

            public SortingSaver(MainWindow mw)
            {
                _mainWindow = mw;
                _wasSorting = _mainWindow._sorting;
                _selectedItem = _mainWindow.VideoList.SelectedItem;
                _mainWindow._sorting = true;
            }

            public void Dispose()
            {
                _mainWindow.VideoList.SelectedItem = _selectedItem;
                _mainWindow._sorting = _wasSorting;
            }
        }

        private const int Vote1 = 43;
        private const int Vote2 = 1069;

        private List<InfoDependentUI> _infoDepUIList = new List<InfoDependentUI>();
        private MediaListViewModel _mediaList;

        private bool? _byNameAscending;
        private bool? _byDateAscending;
        private bool? _byTypeAscending;

        private MediaStatuses _mediaStatus = MediaStatuses.Stopped;

        private bool _sorting;

        private bool _suppressSearchBoxTextChangedHandling = false;
        private string _searchTarget;
        private List<Tuple<FrameworkElement, FrameworkElement>> _highlightedPairs
            = new List<Tuple<FrameworkElement, FrameworkElement>>();
        private List<ListBoxItem> _highlightedItems = new List<ListBoxItem>();
        private int? _currentFocused;

        public const string MediaListUrl = "http://localhost:80/exetel/apps/ailin/media.json";
        public const string MediaListFileName = "media.json";

        public MainWindow()
        {
            InitializeComponent();
            InitInfoDepdentUI();
            SetTitle();
        }

        private async void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            ShowPlaceholderText();
            await LoadImages();
            await RefreshAndMergeMediaList();
        }

        private delegate Task DownloadTask();

        private async Task RefreshAndMergeMediaList()
        {
            var mediaRepoManager = new MediaRepoManager(MediaListUrl, MediaListFileName);
            await mediaRepoManager.Initialize();
            await mediaRepoManager.Refresh();
            _mediaList = new MediaListViewModel(mediaRepoManager.Current);
            VideoList.ItemsSource = _mediaList.MediaViewModels;
            VideoList.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorOnStatusChanged;
            VideoList.ItemContainerGenerator.ItemsChanged += ItemContainerGeneratorOnItemsChanged;
        }

        private void ItemContainerGeneratorOnStatusChanged(object sender, EventArgs e)
        {
            ColorVideoListItems(VideoList.Items);
        }

        private void ItemContainerGeneratorOnItemsChanged(object sender, ItemsChangedEventArgs e)
        {
        }
        
        private void ColorVideoListItems(IList items)
        {
            foreach (var item in items.Cast<MediaInfoViewModel>().Where(i=>!i.BackgroundUpdatedToUI))
            {
                var lbi = (ListBoxItem)VideoList.ItemContainerGenerator.ContainerFromItem(item);
                if (lbi != null)
                {
                    lbi.Background = item.Background;
                    item.BackgroundUpdatedToUI = true;
                }
            }
        }

        private async Task LoadImages()
        {
            const string tiebaFallback = "pack://application:,,,/Images/tieba-fallback.png";
            const string generalFallback = "pack://application:,,,/Images/fallback.gif";

            DownloadTask[] downloads = {
                async () =>
                {
                    var uri = await LZhMBLogo.TryLoadWebImage("http://www.zhulin.net/images/lzmb.jpg", generalFallback, 1);
                    await LZhMBLogo.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        LZhMBLogo.Stretch = uri == generalFallback ? Stretch.Uniform : Stretch.None));
                },
                async ()=>
                {
                    await ZhLYMHLogo.TryLoadWebImage("http://wx2.sinaimg.cn/mw690/ab98e598ly1fc5m8aizjpj21rs1fyqv2.jpg", generalFallback, 1, 5000);
                },
                async ()=>
                {
                    await LoveChinaLogo.TryLoadWebImage("http://r1.ykimg.com/0130391F455691A356625A00E4413F737EF418-80F3-1059-40B8-4FA3147D1345", generalFallback, 1, 5000);
                },
                async ()=>
                {
                    const string tiebaImage = "http://imgsrc.baidu.com/forum/pic/item/3ac79f3df8dcd100c3cd89c7748b4710b8122f86.jpg";
                    await TiebaLogo.TryLoadWebImage(tiebaImage, tiebaFallback, 1, 5000);
                },
                async ()=>
                {
                    await CollectionLogo.TryLoadWebImage("http://www.zhulin.net/html/bbs/UploadFile/2006-8/200683115131166.jpg", generalFallback, 1, 5000);
                }
            };

            var tasks = downloads.Select(dl => dl()).ToArray();
            await Task.WhenAll(tasks);  
        }

        private void SetTitle()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var ver = assembly.GetName().Version;
            Title = $"爱琳投票助手（版本{ver.Major}.{ver.Minor}.{ver.Build}）";
        }

        private void InitInfoDepdentUI()
        {
            _infoDepUIList.Add(new InfoDependentUI
            {
                VoteId = Vote1,

                Tab = Tab1,

                NumVotesText = NumVotes1,
                PopularityText = Popularity1,
                RankText = Rank1,
                LastVoteTime = LastVoteTime1,
                LastVoteResult = LastVoteResult1,

                LinksPanel = VotePage1,
                LinsPage = LinsPage1,
                LinsProfile = LinsProf1,
                LinsPageMobile = LinsPageMobile1,
                LinsImage = ProfileImage1,
                Invite = Invite1,
                InviteEmail = InviteEmail1,

                RefreshButton = Refresh1,
                VoteButton = VoteBtn1,

                QuestionPanel = QuestionForm1,
                QuestionText = Question1,
                ChoicesPanel = Choices1,

                ResultPanel = Result1,
                ResultText = ResultText1,
                ResultLink = ResultPage1
            });

            _infoDepUIList.Add(new InfoDependentUI
            {
                VoteId = Vote2,

                Tab = Tab2,

                NumVotesText = NumVotes2,
                PopularityText = Popularity2,
                RankText = Rank2,
                LastVoteTime = LastVoteTime2,
                LastVoteResult = LastVoteResult2,

                LinksPanel = VotePage2,
                LinsPage = LinsPage2,
                LinsProfile = LinsProf2,
                LinsPageMobile = LinsPageMobile2,
                LinsImage = ProfileImage2,
                Invite = Invite2,
                InviteEmail = InviteEmail2,

                RefreshButton = Refresh2,
                VoteButton = VoteBtn2,

                QuestionPanel = QuestionForm2,
                QuestionText = Question2,
                ChoicesPanel = Choices2,

                ResultPanel = Result2,
                ResultText = ResultText2,
                ResultLink = ResultPage2
            });

            foreach (var idui in _infoDepUIList)
            {
                idui.Setup(this);
            }
        }

        public void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void BtnOrderByTimeOnClick(object sender, RoutedEventArgs e)
        {
            using (new SortingSaver(this))
            {
                _byDateAscending = !(_byDateAscending ?? false);
                var c = _byDateAscending.Value ? MediaListViewModel.CompareDateAscending : MediaListViewModel.CompareDateDescending;
                BtnOrderByTime.Content = _byDateAscending.Value ? "时间升序" : "时间降序";
                _mediaList.Push(c);
                UpdateButtonOrder();
            }
        }

        private void BtnOrderByTypeOnClick(object sender, RoutedEventArgs e)
        {
            using (new SortingSaver(this))
            {
                _byTypeAscending = !(_byTypeAscending ?? false);
                var c = _byTypeAscending.Value ? MediaListViewModel.CompareTypeAscending : MediaListViewModel.CompareTypeDescending;
                BtnOrderByType.Content = _byTypeAscending.Value ? "类型升序" : "类型降序";
                _mediaList.Push(c);
                UpdateButtonOrder();
            }
        }

        private void BtnOrderByNameOnClick(object sender, RoutedEventArgs e)
        {
            using (new SortingSaver(this))
            {
                _byNameAscending = !(_byNameAscending ?? false);
                var c = _byNameAscending.Value ? MediaListViewModel.CompareTitleAscending : MediaListViewModel.CompareTitleDescending;
                BtnOrderByName.Content = _byNameAscending.Value ? "名称升序" : "名称降序";
                _mediaList.Push(c);
                UpdateButtonOrder();
            }
        }

        private void UpdateButtonOrder()
        {
            var index = 0;
            foreach (var c in _mediaList.Comparisons)
            {
                if (c == MediaListViewModel.CompareDateAscending || c == MediaListViewModel.CompareDateDescending)
                {
                    var i = OrderButtons.Children.IndexOf(BtnOrderByTime);
                    if(index != i)
                    {
                        OrderButtons.Children.RemoveAt(i);
                        OrderButtons.Children.Insert(index, BtnOrderByTime);
                    }
                }
                else if (c == MediaListViewModel.CompareTypeAscending || c == MediaListViewModel.CompareTypeDescending)
                {
                    var i = OrderButtons.Children.IndexOf(BtnOrderByType);
                    if (index != i)
                    {
                        OrderButtons.Children.RemoveAt(i);
                        OrderButtons.Children.Insert(index, BtnOrderByType);
                    }
                }
                else
                {
                    var i = OrderButtons.Children.IndexOf(BtnOrderByName);
                    if (index != i)
                    {
                        OrderButtons.Children.RemoveAt(i);
                        OrderButtons.Children.Insert(index, BtnOrderByName);
                    }
                }
                index++;
            }
        }

        private async void VideoListOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_sorting)
            {
                return;
            }

            if (PlaySongs.IsChecked == true)
            {
                await PlaySelected();
            }
        }

        private async Task PlaySelected()
        {
            if (VideoList == null)
            {
                return;
            }
            var item = VideoList.SelectedItem;
            var mivm = (MediaInfoViewModel)item;
            if (mivm != null)
            {
                switch (mivm.Id)
                {
                    case "EShNHZXH":
                        await TryPlayAudioInternet("http://quanben.azurewebsites.net/media/fblxzhf.mp3");
                        break;
                    case "HFQX":
                        await TryPlayAudioInternet("http://quanben.azurewebsites.net/media/wlsh.mp3");
                        break;
                    case "KXZZY":
                        await TryPlayAudioInternet("http://win.web.rc01.sycdn.kuwo.cn/resource/n2/85/34/1272694190.mp3");
                        break;
                    case "WZTTDN":
                        await TryPlayAudioInternet("http://quanben.azurewebsites.net/media/wzttdn.mp3");
                        break;
                    case "YLZhZhND":
                        await TryPlayAudioInternet("http://quanben.azurewebsites.net/media/a-morning-in-cornwell.mp3");
                        break;
                    case "XEBLK":
                        await TryPlayAudioInternet("http://quanben.azurewebsites.net/media/xeblk.mp3");
                        break;
                    case "XJRSh":
                        await TryPlayAudioInternet("http://quanben.azurewebsites.net/media/xjrsh.mp3");
                        break;
                    case "XYJ":
                        await TryPlayAudioInternet("http://win.web.ra01.sycdn.kuwo.cn/resource/n1/192/21/55/3063801691.mp3");
                        break;
                    default:
                        StopPlayingAudio();
                        break;
                }
            }
            else
            {
                StopPlayingAudio();
            }
        }

        private void StopPlayingAudio()
        {
            AudioPlayer.Stop();
        }

        private void PlayAudioResource(string audioName)
        {
            AudioPlayer.Stop();
            var uri = new Uri(@"Audio/" + audioName + ".wma", UriKind.Relative);
            AudioPlayer.Source = uri;
            AudioPlayer.Play();
        }

        private async Task<bool> TryPlayAudioInternet(params string[] urls)
        {
            foreach (var url in urls)
            {
                var res = await PlayAudioInternet(url);
                if (res) return true;
            }
            return false;
        }

        private async Task<bool> PlayAudioInternet (string url)
        {
            try
            {
                AudioPlayer.Stop();
                var uri = new Uri(url);
                AudioPlayer.Source = uri;
                AudioPlayer.Play();

                await WaitUntilMediaStatusChangedToOtherThanStop();

                return _mediaStatus == MediaStatuses.Opened;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task WaitUntilMediaStatusChangedToOtherThanStop()
        {
            while (_mediaStatus == MediaStatuses.Stopped)
            {
                await Task.Delay(500);
            }
        }

        private void AudioPlayerOnMediaOpened(object sender, RoutedEventArgs e)
        {
            SetPlayStatus(MediaStatuses.Opened);
        }

        private void AudioPlayerOnMediaEnded(object sender, RoutedEventArgs e)
        {
            SetPlayStatus(MediaStatuses.Ended);
        }

        private void AudioPlayerOnMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            SetPlayStatus(MediaStatuses.Failed);
        }

        private void SetPlayStatus(MediaStatuses status)
        {
            _mediaStatus = status;
        }
        
        private void PlaySongsOnUnchecked(object sender, RoutedEventArgs e)
        {
            AudioPlayer.Stop();
        }

        private async void PlaySongsOnChecked(object sender, RoutedEventArgs e)
        {
            await PlaySelected();
        }

        private void FriendlyLinksOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            FriendlyLinks.IsSelected = true;
        }

        private void SearchBoxGotFocus(object sender, RoutedEventArgs e)
        {
            var saved = _suppressSearchBoxTextChangedHandling;
            _suppressSearchBoxTextChangedHandling = true;

            if (string.IsNullOrWhiteSpace(_searchTarget))
            {
                SearchBox.Text = "";
            }

            _suppressSearchBoxTextChangedHandling = saved;
        }

        private void SearchBoxLostFocus(object sender, RoutedEventArgs e)
        {
            ShowPlaceholderText();
        }

        private void SearchBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_suppressSearchBoxTextChangedHandling)
            {
                UpdateSearchTarget(SearchBox.Text);
            }
        }

        private void ShowPlaceholderText()
        {
            var saved = _suppressSearchBoxTextChangedHandling;
            _suppressSearchBoxTextChangedHandling = true;

            if (String.IsNullOrWhiteSpace(_searchTarget))
            {
                SearchBox.Text = "搜索...";
            }

            _suppressSearchBoxTextChangedHandling = saved;
        }

        private void BtnClearOnClick(object sender, RoutedEventArgs e)
        {
            UpdateSearchTarget("");
            ShowPlaceholderText();
        }

        private void BtnSearchOnClick(object sender, RoutedEventArgs e)
        {
            SearchAndHighlight();
        }

        private void SearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_currentFocused == null)
                {
                    SearchAndHighlight();
                }
                else
                {
                    var shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                    if (shift)
                    {
                        _currentFocused--;
                        if (_currentFocused.Value < 0)
                        {
                            _currentFocused = _highlightedItems.Count-1;
                        }
                    }
                    else
                    {
                        _currentFocused++;
                        if (_currentFocused.Value >= _highlightedItems.Count)
                        {
                            _currentFocused = 0;
                        }
                    }
                    var lbi = _highlightedItems[_currentFocused.Value];
                    VideoList.ScrollIntoView(lbi);
                    lbi.IsSelected = true;
                }
            }
        }

        private void SearchAndHighlight()
        {
            DeHighlight();
            if (string.IsNullOrWhiteSpace(_searchTarget))
            {
                return;
            }
            var first = true;
            var search = _searchTarget.Trim();
            var count = 0;
            foreach (var item in VideoList.Items)
            {
                var lbi = (ListBoxItem)VideoList.ItemContainerGenerator.ContainerFromItem(item);
                var p = lbi.GetFirstPanelFromDatabound();
                if (p != null)
                {
                    var tbs = p.GetAllTexts().ToList();
                    var pairs = tbs.Highlight(search);
                    var nonEmpty = false;
                    foreach (var pair in pairs)
                    {
                        nonEmpty = true;
                        _highlightedPairs.Add(pair);
                    }
                    if (nonEmpty)
                    {
                        _highlightedItems.Add(lbi);
                        _currentFocused = 0;
                        if (first)
                        {
                            VideoList.ScrollIntoView(lbi);
                            lbi.IsSelected = true;
                            first = false;
                        }
                        count++;
                    }
                }
            }
            if (count > 0)
            {
                MatchCount.Text = $"找到{count}条记录";
            }
            else
            {
                MatchCount.Text = $"没有找到匹配项";
            }
        }

        private void DeHighlight()
        {
            _highlightedPairs.DeHighlight();
            _highlightedPairs.Clear();
            _highlightedItems.Clear();
            _currentFocused = null;
            MatchCount.Text = "";
        }

        private void UpdateSearchTarget(string target)
        {
            _searchTarget = target;
            DeHighlight();
        }

        private void MainKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                SearchBox.Focus();
            }
        }
    }
}
