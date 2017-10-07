//#define TEST_DISABLE_INITIAL_LOAD
//#define TEST_INVITE

using AiLinWpf.Styles;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WebKit;
using WebKit.Helpers;
using static AiLinWpf.Helpers.ImageLoadingHelper;
using static WebKit.VotePageNavigator;

namespace AiLinWpf.Voting
{
    public class InfoDependentUI
    {
        public enum States
        {
            Init,
            Refreshing,
            Loaded,
            SubmittingQuestion,
            ReplyReceived,
            Error,
            Cancelled
        }

        public enum RefreshReasons
        {
            UserRequested,
            AfterReply,
            PossiblyExpired
        }

        private DateTime? _lastRefresh;
        private readonly TimeSpan _expiry
            = TimeSpan.FromSeconds(60);

        private string _inviteMessage;
        private DateTime _lastInviteMessageRefresh;
        private readonly TimeSpan _inviteMessageExpiry
#if TEST_INVITE
            = TimeSpan.FromSeconds(1);
        private Canceller _inviteCanceller = new Canceller();
        private bool _notFirst;
#else
            = TimeSpan.FromMinutes(15);
#endif

        private AsyncLock MobileNavLock = new AsyncLock();
        public VotePageNavigator MobileNavigator { get; private set; }

        public States State { get; private set; }

        public RefreshReasons RefreshReason { get; private set; }

        public ErrorCodes Error { get; private set; }

        public int VoteId;
        public VotePageNavigator Navigator { get; private set; }
        public PageInfo PageInfo;

        public TabItem Tab;

        public TextBlock NumVotesText;
        public TextBlock PopularityText;
        public TextBlock RankText;
        public TextBlock LastVoteTime;
        public TextBlock LastVoteResult;

        public StackPanel LinksPanel;
        public Hyperlink LinsPage;
        public Hyperlink LinsProfile;
        public Hyperlink LinsPageMobile;
        public Image LinsImage;
        public Button Invite;
        public Button InviteEmail;

        public Button RefreshButton;
        public Button VoteButton;

        public StackPanel QuestionPanel;
        public TextBlock QuestionText;
        public StackPanel ChoicesPanel;

        public RichTextBox ResultPanel;
        public Run ResultText;
        public Hyperlink ResultLink;

        public MainWindow MainWindow;

        public RecordRepository Records;

        public void Setup(MainWindow window)
        {
            Navigator = new VotePageNavigator(VoteId);
            MobileNavigator = new VotePageNavigator(VoteId, MobilePageUrlPattern, true);

            Records = new RecordRepository(VoteId);
            MainWindow = window;
            Tab.Loaded += TabOnLoaded;
            RefreshButton.Click += RefreshTab;
            VoteButton.Click += VoteButtonClick;
            Invite.Click += InviteButtonClick;
            InviteEmail.Click += InviteEmailButtonClick;
            ResultLink.RequestNavigate += window.HyperlinkRequestNavigate;
            State = States.Init;
            LoadLast();
        }

#region Event handlers

        private async void TabOnLoaded(object sender, RoutedEventArgs e)
        {
            EnableLinsPages(false);
#if !TEST_DISABLE_INITIAL_LOAD
            State = States.Refreshing;
            await RefreshVoteAsync(true);
#endif
        }

        private void CancelInvite()
        {
            MobileNavigator.CancelRefresh();
#if TEST_INVITE
            _inviteCanceller.Cancel();
#endif
        }

        private async void RefreshTab(object sender, RoutedEventArgs e)
        {
            if (State == States.Refreshing)
            {
                Navigator.CancelRefresh();
                CancelInvite();
                return;
            }
            if(State == States.SubmittingQuestion)
            {
                Navigator.CancelRefresh();
                return;
            }
            if (State == States.ReplyReceived)
            {
                ResultPanel.Visibility = Visibility.Collapsed;
                VoteButton.Visibility = Visibility.Visible;
                ChoicesPanel.IsEnabled = true;
                RefreshReason = RefreshReasons.AfterReply;
            }
            else
            {
                RefreshReason = RefreshReasons.UserRequested;
            }
            State = States.Refreshing;
            await RefreshVoteAsync();
        }

        private void VoteButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleVote();
        }

        private async void InviteButtonClick(object sender, RoutedEventArgs e)
        {
            const string cancelStr = "取消";
            if ((string)Invite.Content != cancelStr)
            {
                var oldContent = Invite.Content;
                var oldWidth = Invite.ActualWidth;
                Invite.Content = cancelStr;
                Invite.Width = oldWidth;
                var invite = await GetInviteMessageAsync();
                Invite.Content = oldContent;
                if (invite != null)
                {
                    Clipboard.SetText(invite);
                    var sb = new StringBuilder();
                    sb.AppendLine("以下内容已经复制到剪贴板：");
                    sb.AppendLine();
                    sb.Append(invite);
                    MessageBox.Show(sb.ToString(), MainWindow.Title);
                }
                else
                {
                    MessageBox.Show("很抱歉，链接地址未能正确获得", MainWindow.Title);
                }
            }
            else
            {
                CancelInvite();
            }
        }

        private async void InviteEmailButtonClick(object sender, RoutedEventArgs e)
        {
            const string cancelStr = "取消";
            if ((string)InviteEmail.Content != cancelStr)
            {
                var oldContent = InviteEmail.Content;
                var oldWidth = InviteEmail.ActualWidth;
                InviteEmail.Content = cancelStr;
                InviteEmail.Width = oldWidth;
                var invite = await GetInviteMessageAsync();
                InviteEmail.Content = oldContent;
                if (invite != null)
                {
                    var body = invite.Replace("\n", "%0A");
                    body = body.Replace("\r", "");
                    body = body.Replace("?", "%3F");
                    body = body.Replace("&", "%26");
                    var mailto = $"mailto:?subject=亲，帮我个忙&body={body}";
                    Process.Start(new ProcessStartInfo(mailto));
                }
                else
                {
                    MessageBox.Show("很抱歉，链接地址未能正确获得", MainWindow.Title);
                }
            }
            else
            {
                CancelInvite();
            }
        }

#endregion

        public async Task<string> GetInviteMessageAsync()
        {
            using (await MobileNavLock.Wait())
            {
                if (_inviteMessage == null || DateTime.UtcNow - _lastInviteMessageRefresh >= _inviteMessageExpiry)
                {
                    _inviteMessage = await RetrieveInvite_unsafe();

                    if (_inviteMessage != null)
                    {
                        _lastInviteMessageRefresh = DateTime.UtcNow;
                    }
                }
                return _inviteMessage;
            }
        }

        private async void RbClick(object sender, RoutedEventArgs e)
            => await RbClickAsync(sender, e);

        private async Task RbClickAsync(object sender, RoutedEventArgs e)
        {
            DisableRefreshSubmitting();

            State = States.SubmittingQuestion;
            var rb = (RadioButton)sender;
            var c = (Question.Choice)rb.Tag;
            var s = CreateSubmit(PageInfo, c);
            byte[] result;
            var resultLinkContainer = (TextBlock)ResultLink.Parent;
            try
            {
                result = await Navigator.SubmitAsync(s);
            }
            catch (RequestCancelled)
            {
                resultLinkContainer.Visibility = Visibility.Collapsed;
                ResultText.Text = "投票已取消，请重试";
                RestoreRefreshSubmitted();
                State = States.ReplyReceived;
                return;
            }
            catch (Exception)
            {
                resultLinkContainer.Visibility = Visibility.Collapsed;
                ResultText.Text = "提交出错，请重试";
                RestoreRefreshSubmitted();
                State = States.ReplyReceived;
                return;
            }

            var path = Path.GetTempPath();
            var replyfile = Path.Combine(path, "votereply.html");
            using (var fs = new FileStream(replyfile, FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(result);
            }
            var replydata = result.ConvertGB2312ToUTF();
            var reply = replydata.GetVoteResponseMessage(true);
            var replymsg = reply.Item1;
            var successful = reply.Item2;
            ResultText.Text = replymsg != null ? replymsg.Trim() : "无法解析投票回应，请重试";
            var uri = "file:///" + replyfile.Replace('\\', '/');
            ResultLink.NavigateUri = new Uri(uri);
            resultLinkContainer.Visibility = Visibility.Visible;

            if (successful)
            {
                RecordLastVote();
            }

            RestoreRefreshSubmitted();
            State = States.ReplyReceived;
        }

        private void RecordLastVote()
        {
            var time = DateTime.Now;
            if (Records.Last == null)
            {
                Records.Last = new Record();
            }
            Records.Last.Time = time;
            Records.Last.CopyFromPageInfo(PageInfo);
            Records.Save();

            ShowLast();
        }

        private void LoadLast()
        {
            Records.Load();
            ShowLast();
        }

        private void ShowLast()
        {
            if (Records.Last == null)
            {
                LastVoteTime.Text = "--";
                LastVoteResult.Text = "--";
                return;
            }
            LastVoteTime.Text = Records.Last.Time.ToString();
            if (Records.Last.Votes != null && Records.Last.Rank != null)
            {
                LastVoteResult.Text = string.Format("{0}票，第{1}名", Records.Last.Votes, Records.Last.Rank);
            }
        }

        private async Task RefreshVoteAsync(bool mandatory = false)
        {
            DisableRefresh();
            var res = await Navigator.SearchForZhuLinAsync(true);
            var page = res.Item1;
            var error = res.Item2;
            if (page != null && error == ErrorCodes.Success)
            {
                PageInfo = page;
                Error = error;
                NumVotesText.Text = PageInfo.Votes?.ToString() ?? "无法获取";
                PopularityText.Text = PageInfo.Popularity?.ToString() ?? "无法获取";
                RankText.Text = PageInfo.Rank?.ToString() ?? "无法获取";
                LinsPage.NavigateUri = PageInfo.PageUrl != null ? new Uri(PageInfo.PageUrl) : null;
                LinsProfile.NavigateUri = PageInfo.ProfileUrl != null ? new Uri(PageInfo.ProfileUrl) : null;
                if (PageInfo.Thumbnail != null)
                {
                    await LinsImage.TryLoadWebImage(PageInfo.Thumbnail);

                    var imageLink = (Hyperlink)((InlineUIContainer)LinsImage.Parent).Parent;
                    imageLink.NavigateUri = LinsProfile.NavigateUri;
                }

                QuestionText.Text = PageInfo.Question.Title?.TrimQuestionString() ?? "无法获取问题";
                if (PageInfo.Question.Title != null)
                {
                    ChoicesPanel.Children.Clear();
                    foreach (var c in PageInfo.Question.Choices)
                    {
                        var rb = new RadioButton
                        {
                            Tag = c,
                            Content = c.Text.TrimQuestionString()
                        };
                        rb.VerticalContentAlignment = VerticalAlignment.Center;
                        rb.Click += RbClick;
                        ChoicesPanel.Children.Add(rb);
                    }
                }
            }
            else if (!mandatory && PageInfo != null && error == ErrorCodes.Cancelled)
            {
                RestoreRefresh();
                State = States.Loaded;
                Error = ErrorCodes.Success;
                return;
            }
            else
            {
                PageInfo = page;
                Error = error;
                ProcessError();
                return;
            }

            await GetInviteMessageAsync();

            RestoreRefresh();
            State = States.Loaded;
            _lastRefresh = DateTime.UtcNow;
        }

        private void DisableRefresh()
        {
            RefreshButton.Foreground = Coloring.Black;
            switch (RefreshReason)
            {
                case RefreshReasons.UserRequested:
                case RefreshReasons.AfterReply:
                    RefreshButton.Content = "正在刷新（按此键取消刷新）……";
                    break;
                case RefreshReasons.PossiblyExpired:
                    RefreshButton.Content = "网页可能已经过期，正在刷新……";
                    break;
            }
            VoteButton.IsEnabled = false;
            RefreshButton.IsEnabled = true;
            EnableLinsPages(false);
            Invite.IsEnabled = false;
            InviteEmail.IsEnabled = false;
            CollapseVote();
        }

        private void RestoreRefresh(bool enableVoteButton = true)
        {
            RefreshButton.IsEnabled = true;
            VoteButton.IsEnabled = enableVoteButton;
            EnableLinsPages(true);
            Invite.IsEnabled = true;
            InviteEmail.IsEnabled = true;
            CollapseVote();
            RefreshButton.Content = "刷新";
        }

        private void EnableLinsPages(bool enabled)
        {
            LinsProfile.IsEnabled = enabled;
            LinsPage.IsEnabled = enabled;
            LinsPageMobile.IsEnabled = enabled;
        }

        private void ProcessError()
        {
            RefreshButton.IsEnabled = true;
            if (Error ==  ErrorCodes.Cancelled)
            {
                _lastRefresh = null;
                State = States.Cancelled;
                RestoreRefresh();
                return;
            }
            State = States.Error;
            RefreshButton.Foreground = Coloring.RedBrush;
            switch (Error)
            {
                case ErrorCodes.ParsingError:
                    RefreshButton.Content = "解析失败，点此重试";
                    break;
                case ErrorCodes.WebRequestError:
                    RefreshButton.Content = "网络错误，点此重试";
                    break;
                case ErrorCodes.TimeOutError:
                    RefreshButton.Content = "超时错误，点此重试";
                    break;
                case ErrorCodes.Cancelled:
                    break;
                default:
                    Debug.Assert(false, "未识别错误");
                    break;
            }
        }

        public void DisableRefreshSubmitting()
        {
            RefreshButton.Content = "正在投票（点此取消）……";
            RefreshButton.IsEnabled = true;
            VoteButton.IsEnabled = false;
        }

        private void RestoreRefreshSubmitted()
        {
            RefreshButton.IsEnabled = true;
            RefreshButton.Content = "关闭投票并刷新";
            VoteButton.IsEnabled = true;
            ChoicesPanel.IsEnabled = false;
            VoteButton.Visibility = Visibility.Collapsed;
            ResultPanel.Visibility = Visibility.Visible;
        }

        public async void ToggleVote()
        {
            if (VoteExpanded())
            {
                CollapseVote();
            }
            else
            {
                await RefreshIfNeeded();
                if (State == States.Loaded)
                {
                    ExpandVote();
                }
                else
                {
                    CollapseVote();
                }
            }
        }

        private async Task RefreshIfNeeded()
        {
            if (_lastRefresh == null || DateTime.UtcNow - _lastRefresh >= _expiry)
            {
                State = States.Refreshing;
                RefreshReason = RefreshReasons.PossiblyExpired;
                await RefreshVoteAsync(true);
            }
        }

        private async Task<string> RetrieveInvite_unsafe()
        {
            var resMob = await MobileNavigator.SearchForZhuLinMobileUrlAsync();
            var mp = resMob.Item1;
            if (mp != null)
            {
                LinsPageMobile.NavigateUri = new Uri(mp);
            }

#if TEST_INVITE
            try
            {
                if (_notFirst)
                {
                    await Task.Delay(10000, _inviteCanceller.Token);
                }
                else
                {
                    _notFirst = true;
                }
            }
            catch (TaskCanceledException)
            {
                return null;
            }
#endif
            if (mp != null || PageInfo.PageUrl != null)
            {
                var sb = new StringBuilder();
                sb.Append("亲，请为我的偶像朱琳老师投上您宝贵的一票");
                if (PageInfo?.Rank != null)
                {
                    sb.Append($"，她现在在{PageInfo.Rank}位");
                }
                sb.AppendLine("。多谢了先！");
                if (mp != null)
                {
                    sb.AppendLine($"手机网址： {mp}");
                }
                if (PageInfo?.PageUrl != null)
                {
                    sb.AppendLine($"普通网址： {PageInfo.PageUrl}");
                }
                return  sb.ToString();
            }
            return null;
        }

        private bool VoteExpanded()
            => QuestionPanel.Visibility == Visibility.Visible;

        private void ExpandVote()
        {
            VoteButton.Content = "暂不投票";
            QuestionPanel.Visibility = Visibility.Visible;
        }

        private void CollapseVote()
        {
            VoteButton.Content = "马上投票给她";
            QuestionPanel.Visibility = Visibility.Collapsed;
        }
    }

}
