//#define SIMULATE_FAILED_LOAD
//#define TEST_FALLBACK_IMAGES
//#define USE_FALLBACK_IMAGES_ONLY
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
using AiLinWpf.Voting;
using AiLinWpf.ViewModels;
using System.Linq;
using AiLinWpf.Helpers;
using System.Windows.Input;
using static AiLinWpf.Helpers.ImageLoadingHelper;
using AiLinWpf.Sources;
using System.Windows.Documents;

namespace AiLinWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Types

        public enum MediaStatuses
        {
            Stopped,
            Opened,
            Ended,
            Failed
        }

        private enum RefreshOptions
        {
            NoRefresh,
            RefreshWithNoMessage,
            RefreshWithMessage
        }

        private delegate Task DownloadTask();

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

        #endregion

        #region Fields

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

        public const string MediaListUrl = "http://quanben.azurewebsites.net/apps/ailin/media.json.txt";

        /// <summary>
        ///  The name of the file in the local storage that keeps the latest media list updated from the server
        /// </summary>
        public const string MediaListFileName = "media.json";

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            SetTitle();
            InitInfoDepdentUI();
            BindProxySettings();
        }

        #endregion

        #region Event handlers

        private async void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            ShowPlaceholderText();
            await InitLoadAndRefreshMediaList();
            await LoadImages();
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

        private bool HasSearchResults()
            => _highlightedItems.Count > 0;

        private void SearchBoxLostFocus(object sender, RoutedEventArgs e)
        {
            ShowPlaceholderText();
            if (!HasSearchResults())
            {
                EndSearch();
            }
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

        private async void BtnSearchOnClick(object sender, RoutedEventArgs e)
        {
            PrepareForSearch();
            await SearchAndHighlight();
            ScrollToFirstHighlightedIfAny();
        }

        private async void SearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PrepareForSearch();
                if (_currentFocused == null)
                {
                    await SearchAndHighlight();
                }
                else
                {
                    var shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                    if (shift)
                    {
                        _currentFocused--;
                        if (_currentFocused.Value < 0)
                        {
                            _currentFocused = _highlightedItems.Count - 1;
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
                }
                ScrollToFirstHighlightedIfAny();
            }
        }

        private void ScrollToFirstHighlightedIfAny()
        {
            if(_currentFocused.HasValue && _currentFocused.Value >= 0 && _currentFocused.Value < _highlightedItems.Count)
            {
                var lbi = _highlightedItems[_currentFocused.Value];
                var obj = lbi.DataContext;
                VideoList.ScrollIntoView(obj);
                lbi.IsSelected = true;
            }
        }

        private async void MediaItemOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                SearchBox.Focus();
            }
            else if (e.Key == Key.F5)
            {
                var force = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                await RedownloadMediaList(force);
            }
            else if (e.Key == Key.R && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                await ResetMediaList();
            }
        }

        private void HyperlinkCopyAddressOnClick(object sender, RoutedEventArgs e)
        {
            if (((ContextMenu)((MenuItem)e.Source).Parent).PlacementTarget is TextBlock tb
                && tb.Inlines.FirstInline is Hyperlink hl)
            {
                var uri = hl.NavigateUri;
                Clipboard.SetText(uri.AbsoluteUri);
                MessageBox.Show($"链接'{uri}'已经复制到剪贴板", Title);
            }
            else
            {
                MessageBox.Show($"抱歉！出错了，无法产生并复制链接", Title);
            }
        }

        #endregion

        private void PrepareForSearch()
        {
            VideoList.SetValue(VirtualizingPanel.IsVirtualizingProperty, false);
        }

        private void EndSearch()
        {
            VideoList.SetValue(VirtualizingPanel.IsVirtualizingProperty, true);
        }

        private async Task RedownloadMediaList(bool force)
        {
            var caption = force ? "是否确认重新下载媒体列表？" : "是否确认刷新媒体列表？";
            var res = MessageBox.Show(caption, Title, MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                await LoadAndRefreshMediaList(force, RefreshOptions.RefreshWithMessage);
            }
        }

        private async Task ResetMediaList()
        {
            var res = MessageBox.Show("是否确认重置媒体列表？", Title, MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                var mediaRepoManager = new MediaRepoManager(MediaListUrl, MediaListFileName);
                await mediaRepoManager.Initialize();
                mediaRepoManager.Reset();
                await mediaRepoManager.ResetToDefault();

                _mediaList = new MediaListViewModel(mediaRepoManager.Current);
                VideoList.ItemsSource = _mediaList.MediaViewModels;

                MessageBox.Show("已重置为默认媒体列表。", Title);
            }
        }

        private async Task InitLoadAndRefreshMediaList()
        {
#if SIMULATE_FAILED_LOAD
            await LoadAndRefreshMediaList(false, RefreshOptions.NoRefresh);
#else
            await LoadAndRefreshMediaList(false, RefreshOptions.RefreshWithNoMessage);
#endif
        }

        private async Task LoadAndRefreshMediaList(bool force, RefreshOptions refreshOption)
        {
            var mediaRepoManager = new MediaRepoManager(MediaListUrl, MediaListFileName);
            await mediaRepoManager.Initialize();
            if (force)
            {
                mediaRepoManager.Reset();
            }
            var res = default(MediaRepoManager.RefreshResults);
            if (refreshOption != RefreshOptions.NoRefresh)
            {
                res = await mediaRepoManager.Refresh();
                if (res ==  MediaRepoManager.RefreshResults.FailedToDownload)
                {
                    await mediaRepoManager.ResetToDefault();
                }
            }

            _mediaList = new MediaListViewModel(mediaRepoManager.Current);
            VideoList.ItemsSource = _mediaList.MediaViewModels;

            if (refreshOption == RefreshOptions.RefreshWithMessage)
            {
                switch (res)
                {
                    case MediaRepoManager.RefreshResults.Refreshed:
                        MessageBox.Show("成功下载并更新列表。", Title);
                        break;
                    case MediaRepoManager.RefreshResults.FailedToDownload:
                        MessageBox.Show("下载列表失败。", Title);
                        break;
                    case MediaRepoManager.RefreshResults.AlreadyLatest:
                        MessageBox.Show("已经是最新列表，无需更新。", Title);
                        break;
                }
            }
        }

        private async Task LoadImages()
        {
            DownloadTask[] downloads = {
                async () =>
                {
                    const string fallback = "pack://application:,,,/Images/init-zhulin-small.jpg";
                    var uri = await LZhMBLogo.TryLoadWebImage(
#if USE_FALLBACK_IMAGES_ONLY
                        fallback,
#elif TEST_FALLBACK_IMAGES
                        "http://bad/image/link1.jpg",
#else
                        "http://www.zhulin.net/images/lzmb.jpg",
#endif
                        fallback, 5000, 1);
                    await LZhMBLogo.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        LZhMBLogo.Stretch = uri == fallback ? Stretch.Uniform : Stretch.None));
                },
                async ()=>
                {
                    const string fallback = "pack://application:,,,/Images/init-weibo-small.jpg";
                    await ZhLYMHLogo.TryLoadWebImage(
#if USE_FALLBACK_IMAGES_ONLY
                        fallback,
#elif TEST_FALLBACK_IMAGES
                        "http://bad/image/link2.jpg",
#else
                        "http://wx2.sinaimg.cn/mw690/ab98e598ly1fc5m8aizjpj21rs1fyqv2.jpg", 
#endif
                        fallback, 5000, 1);
                },
                async ()=>
                {
                    const string fallback = "pack://application:,,,/Images/init-lovechina66-small.jpg";
                    await LoveChinaLogo.TryLoadWebImage(
#if USE_FALLBACK_IMAGES_ONLY
                        fallback,
#elif TEST_FALLBACK_IMAGES
                        "http://bad/image/link3.jpg",
#else
                        "http://r1.ykimg.com/0130391F455691A356625A00E4413F737EF418-80F3-1059-40B8-4FA3147D1345", 
#endif
                        fallback, 5000, 1);
                },
                async ()=>
                {
                    const string fallback = "pack://application:,,,/Images/init-tieba.jpg";
                    await TiebaLogo.TryLoadWebImage(
#if USE_FALLBACK_IMAGES_ONLY
                        fallback,
#elif TEST_FALLBACK_IMAGES
                        "http://bad/image/link4.jpg",
#else
                        "http://imgsrc.baidu.com/forum/pic/item/3ac79f3df8dcd100c3cd89c7748b4710b8122f86.jpg",
#endif
                        fallback, 5000, 1);
                },
                async ()=>
                {
                    const string fallback = "pack://application:,,,/Images/init-giantpost-small.jpg";
                    await CollectionLogo.TryLoadWebImage(
#if USE_FALLBACK_IMAGES_ONLY
                        fallback,
#elif TEST_FALLBACK_IMAGES
                        "http://bad/image/link5.jpg",
#else
                        "http://www.zhulin.net/html/bbs/UploadFile/2006-8/200683115131166.jpg", 
#endif
                        fallback, 5000, 1);
                }
            };

#if LOAD_IMAGE_IN_PARALLEL
            var tasks = downloads.Select(dl => dl()).ToArray();
            await Task.WhenAll(tasks);
#else
            foreach (var dl in downloads)
            {
                await dl();
            }
#endif
            Trace.WriteLine("All image downloading processes have been completed.");
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
                ResultLink = ResultPage1,

                ProxyEnabled = ProxyEnabled1,
                Proxy = Proxy1
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
                ResultLink = ResultPage2,

                ProxyEnabled = ProxyEnabled2,
                Proxy = Proxy2
            });

            foreach (var idui in _infoDepUIList)
            {
                idui.Setup(this);
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
                var bgm = mivm.Model.Songs.Count == 1 ? mivm.Model.Songs.First() : 
                    mivm.Model.Songs.FirstOrDefault(x => x.Item1 == "bgm");
                if (bgm != null && !string.IsNullOrWhiteSpace(bgm.Item2))
                {
                    await TryPlayAudioInternet(bgm.Item2);
                }
                else
                {
                    StopPlayingAudio();
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

        private async Task<ListBoxItem> GetListBoxItem(object item)
        {
            while (true)
            {
                var tcs = new TaskCompletionSource<bool>();
                EventHandler handler = null;
                VideoList.ItemContainerGenerator.StatusChanged += handler = (sender, e) =>
                {
                    VideoList.ItemContainerGenerator.StatusChanged -= handler;
                    tcs.SetResult(true);
                };
                var lbi = (ListBoxItem)VideoList.ItemContainerGenerator.ContainerFromItem(item);
                if (lbi != null)
                {
                    VideoList.ItemContainerGenerator.StatusChanged -= handler;
                    return lbi;
                }
                else
                {
                    await tcs.Task;
                }
            }
        }

        private async Task SearchAndHighlight()
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
                ListBoxItem lbi = null;
                lbi = await GetListBoxItem(item);
                var p = lbi.GetFirst<Panel>();
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

        private void BindProxySettings()
        {
            Proxy1.TextChanged += (s, e) =>
            {
                Proxy2.Text = Proxy1.Text;
            };
            Proxy2.TextChanged += (s, e) =>
            {
                Proxy1.Text = Proxy2.Text;
            };
            ProxyEnabled1.Checked += (s, e) =>
            {
                ProxyEnabled2.IsChecked = ProxyEnabled1.IsChecked;
            };
            ProxyEnabled1.Unchecked += (s, e) =>
            {
                ProxyEnabled2.IsChecked = ProxyEnabled1.IsChecked;
            };
            ProxyEnabled2.Checked += (s, e) =>
            {
                ProxyEnabled1.IsChecked = ProxyEnabled2.IsChecked;
            };
            ProxyEnabled2.Unchecked += (s, e) =>
            {
                ProxyEnabled1.IsChecked = ProxyEnabled2.IsChecked;
            };
        }
    }
}
