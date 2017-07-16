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
using static AiLinWpf.Helpers.ImageHelper;

namespace AiLinWpf.Data
{
    public class InfoDependentUI
    {
        public enum States
        {
            Refreshing,
            Loaded,
            LoadedWithQuestion,
            SubmittingQuestion,
            ReplyReceived,
            RefreshingAfterReply
        }

        public States State { get; set; }

        public int VoteId;
        public VotePageNavigator Navigator;
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

        public string InviteMessage;

        public MainWindow MainWindow;

        public RecordRepository Records;

        public void Setup(MainWindow window)
        {
            Records = new RecordRepository(VoteId);
            MainWindow = window;
            Tab.Loaded += TabOnLoaded;
            RefreshButton.Click += RefreshTab;
            VoteButton.Click += VoteButtonClick;
            Invite.Click += InviteButtonClick;
            InviteEmail.Click += InviteEmailButtonClick;
            ResultLink.RequestNavigate += window.HyperlinkRequestNavigate;
            State = States.Refreshing;

            LoadLast();
        }

        #region Event handlers

        private void TabOnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshVote();
        }

        private void RefreshTab(object sender, RoutedEventArgs e)
        {
            if (State == States.ReplyReceived)
            {
                ResultPanel.Visibility = Visibility.Collapsed;
                VoteButton.Visibility = Visibility.Visible;
                ChoicesPanel.IsEnabled = true;
                State = States.RefreshingAfterReply;
            }
            else
            {
                State = States.Refreshing;
            }
            RefreshVote();
        }

        private void VoteButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleVote();
        }

        private void InviteButtonClick(object sender, RoutedEventArgs e)
        {
            if (InviteMessage != null)
            {
                Clipboard.SetText(InviteMessage);
                MessageBox.Show(InviteMessage, "以下内容已经复制到剪贴板");
            }
            else
            {
                MessageBox.Show("很抱歉，链接地址未能正确获得", MainWindow.Title);
            }
        }

        private void InviteEmailButtonClick(object sender, RoutedEventArgs e)
        {
            if (InviteMessage != null)
            {
                var body = InviteMessage.Replace("\n", "%0A");
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

        #endregion

        private async void RbClick(object sender, RoutedEventArgs e)
            => await RbClickAsync(sender, e);

        private async Task RbClickAsync(object sender, RoutedEventArgs e)
        {
            DisableRefreshSubmitting();

            State = States.SubmittingQuestion;
            var rb = (RadioButton)sender;
            var c = (Question.Choice)rb.Tag;
            var s = VotePageNavigator.CreateSubmit(PageInfo, c);
            byte[] result;
            var resultLinkContainer = (TextBlock)ResultLink.Parent;
            try
            {
                result = await Navigator.SubmitAsync(s);
            }
            catch (Exception)
            {
                resultLinkContainer.Visibility = Visibility.Collapsed;
                ResultText.Text = "提交出错，请重试";
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

        private async void RefreshVote() => await RefreshVoteAsync();

        private async Task RefreshVoteAsync()
        {
            DisableRefresh();
            PageInfo = await Navigator.SearchForZhuLinAsync(true);
            if (PageInfo != null)
            {
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
            else
            {
                // TODO report error
            }

            var mobileNavigator = new VotePageNavigator(VoteId, VotePageNavigator.MobilePageUrlPattern, true);
            var mp = await mobileNavigator.SearchForZhuLinMobileUrlAsync();
            if (mp != null)
            {
                LinsPageMobile.NavigateUri = new Uri(mp);
            }

            if (mp != null || PageInfo.PageUrl != null)
            {
                var sb = new StringBuilder();
                sb.Append("亲，请为我的偶像朱琳老师投上您宝贵的一票");
                if (PageInfo.Rank != null)
                {
                    sb.Append($"，她现在在{PageInfo.Rank}位");
                }
                sb.AppendLine("。多谢了先！");
                if (mp != null)
                {
                    sb.AppendLine($"手机网址： {mp}");
                }
                if (PageInfo.PageUrl != null)
                {
                    sb.AppendLine($"普通网址： {PageInfo.PageUrl}");
                }
                InviteMessage = sb.ToString();
            }
            else
            {
                InviteMessage = null;
            }

            RestoreRefresh();
            State = States.Loaded;
        }

        public void DisableRefresh()
        {
            RefreshButton.Content = "正在刷新……";
            VoteButton.IsEnabled = false;
            RefreshButton.IsEnabled = false;
            LinsProfile.IsEnabled = false;
            LinsPage.IsEnabled = false;
            LinsPageMobile.IsEnabled = false;
            Invite.IsEnabled = false;
            InviteEmail.IsEnabled = false;
            InviteMessage = "";
            CollapseVote();
        }

        public void RestoreRefresh()
        {
            RefreshButton.IsEnabled = true;
            VoteButton.IsEnabled = true;
            LinsProfile.IsEnabled = true;
            LinsPage.IsEnabled = true;
            LinsPageMobile.IsEnabled = true;
            if (InviteMessage != null)
            {
                Invite.IsEnabled = true;
                InviteEmail.IsEnabled = true;
            }
            CollapseVote();
            RefreshButton.Content = "刷新";
        }

        public void DisableRefreshSubmitting()
        {
            RefreshButton.Content = "正在投票……";
            VoteButton.IsEnabled = false;
            RefreshButton.IsEnabled = false;
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

        public void ToggleVote()
        {
            if (VoteExpanded())
            {
                CollapseVote();
            }
            else
            {
                ExpandVote();
            }
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
