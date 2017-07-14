using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using System;
using System.Windows.Controls;
using System.Threading.Tasks;
using AiLinWpf.Data;
using AiLinWpf.Actions;
using WebKit;
using static AiLinWpf.Helpers.ImageHelper;

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
        private ResourceList _resourceList;

        private bool? _byNameAscending;
        private bool? _byDateAscending;
        private bool? _byTypeAscending;

        private MediaStatuses _mediaStatus = MediaStatuses.Stopped;

        private bool _sorting;

        public MainWindow()
        {
            InitializeComponent();
            InitInfoDepdentUI();
            SetTitle();
            LoadPages();
            _resourceList = new ResourceList(this);
        }

        private void LoadPages()
        {
            const string tiebaImage = "http://imgsrc.baidu.com/forum/pic/item/3ac79f3df8dcd100c3cd89c7748b4710b8122f86.jpg";
            TiebaLogo.Source = TryLoadImage(tiebaImage, "pack://application:,,,/Images/tieba-fallback.png", 1);
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
                Navigator = new VotePageNavigator(Vote1),

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
                Navigator = new VotePageNavigator(Vote2),

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
                var c = _byDateAscending.Value ? ResourceList.CompareDateAscending : ResourceList.CompareDateDescending;
                BtnOrderByTime.Content = _byDateAscending.Value ? "按时间升序" : "按时间降序";
                _resourceList.Push(c);
                UpdateButtonOrder();
            }
        }

        private void BtnOrderByTypeOnClick(object sender, RoutedEventArgs e)
        {
            using (new SortingSaver(this))
            {
                _byTypeAscending = !(_byTypeAscending ?? false);
                var c = _byTypeAscending.Value ? ResourceList.CompareTypeAscending : ResourceList.CompareTypeDescending;
                BtnOrderByType.Content = _byTypeAscending.Value ? "按类型升序" : "按类型降序";
                _resourceList.Push(c);
                UpdateButtonOrder();
            }
        }

        private void BtnOrderByNameOnClick(object sender, RoutedEventArgs e)
        {
            using (new SortingSaver(this))
            {
                _byNameAscending = !(_byNameAscending ?? false);
                var c = _byNameAscending.Value ? ResourceList.CompareTitleAscending : ResourceList.CompareTitleDescending;
                BtnOrderByName.Content = _byNameAscending.Value ? "按名称升序" : "按名称降序";
                _resourceList.Push(c);
                UpdateButtonOrder();
            }
        }

        private void UpdateButtonOrder()
        {
            var index = 0;
            foreach (var c in _resourceList.Comparisons)
            {
                if (c == ResourceList.CompareDateAscending || c == ResourceList.CompareDateDescending)
                {
                    var i = OrderButtons.Children.IndexOf(BtnOrderByTime);
                    if(index != i)
                    {
                        OrderButtons.Children.RemoveAt(i);
                        OrderButtons.Children.Insert(index, BtnOrderByTime);
                    }
                }
                else if (c == ResourceList.CompareTypeAscending || c == ResourceList.CompareTypeDescending)
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
            var lbi = (ListBoxItem)item;
            if (lbi != null)
            {
                switch (lbi.Name)
                {
                    case "EShNHLXH":
                        // version on baidu pan  http://pan.baidu.com/s/1qXZN8V6
                        await TryPlayAudioInternet("http://vgmyqa-dm2306.files.1drv.com/y4mFGrhvofPXWEMEEmOLz7s-U2_Z3V308knhn7TvYGfyxheMS9lBlPjKffBBYzfBJNLh4Duf2mUNPL9nvFcF0PZ58V5whUn_QRSS9lXzyC58ipM4t6kdjg6T5o1nOHi0a7ujjRZUrx8VyQGtJkQB6S39HuSgLVmDVAY3h7VVvl_9SsbszaYgpejy3jRGDg1S1G1/audio.mp3?application=Boombox&appver=2.0&scid=XBoxMusic_Streaming&rid=8BgKL2TTIUimjGdWIZNRfQ.4");
                        break;
                    case "HFQX":
                        await TryPlayAudioInternet("http://om5.alicdn.com/587/2587/13990/172730_15984329_l.mp3?auth_key=43e9654917bd89fd67a31409b1d6c98f-1500174000-0-null");
                        break;
                    case "KXZZY":
                        await TryPlayAudioInternet("http://win.web.rc01.sycdn.kuwo.cn/resource/n2/85/34/1272694190.mp3");
                        break;
                    case "YLZhZhND":
                        await TryPlayAudioInternet("http://om5.alicdn.com/1/258/41258/243252/2811487_297631_l.mp3?auth_key=8481e4f9dbf66fe1f5f8c0c517e19043-1500260400-0-null");
                        break;
                    case "XEBLK":
                        // version on baidu pan http://pan.baidu.com/s/1qYlYRHM
                        await TryPlayAudioInternet("http://u46kbq-dm2306.files.1drv.com/y4ma_byVeSd2JkriEqCXyM4Vv4o34tkkJxc0sjijFvTnF18eJ-jquzCFe_D1FYpQjbehu1erg-XuLF_Urhf2ET5bILL25Y-gnOH-58cLjTaVSo3C4Dq-UBS1wzJYhZhCOtWv3Xxhp4tqi9c6R761qpcn-P9uFbDxQg8CSu5jlO2EJqZ1mafsnFEwgbV1qHv951X/audio.mp3?application=Boombox&appver=2.0&scid=XBoxMusic_Streaming&rid=i7N6bDngsUK%2bZMZ8FHZepA.4");
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
    }
}
