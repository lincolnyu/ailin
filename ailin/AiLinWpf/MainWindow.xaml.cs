using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using WebKit;
using WebKit.Helpers;
using Squirrel;

namespace AiLinWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

            public StackPanel LinksPanel;
            public Hyperlink LinsPage;
            public Hyperlink LinsProfile;

            public Button RefreshButton;
            public Button VoteButton;

            public StackPanel QuestionPanel;
            public TextBlock QuestionText;
            public StackPanel ChoicesPanel;

            public RichTextBox ResultPanel;
            public Run ResultText;
            public Hyperlink ResultLink;

            public void Setup(MainWindow window)
            {
                Tab.Loaded += TabOnLoaded;
                RefreshButton.Click += RefreshTab;
                VoteButton.Click += VoteButtonClick;
                ResultLink.RequestNavigate += window.HyperlinkRequestNavigate;
                State = States.Refreshing;
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
                var result = await Navigator.SubmitAsync(s);

                var path = Path.GetTempPath();
                var replyfile = Path.Combine(path, "votereply.html");
                using (var fs = new FileStream(replyfile, FileMode.Create))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(result);
                }
                var replydata = result.ConvertGB2312ToUTF();
                var replymsg = replydata.GetVoteResponseMessage(true);
                ResultText.Text = replymsg != null? replymsg.Trim() : "无法解析投票回应";
                var uri = "file:///" + replyfile.Replace('\\', '/');
                ResultLink.NavigateUri = new Uri(uri);

                RestoreRefreshSubmitted();
                State = States.ReplyReceived;
            }

            private async void RefreshVote() => await RefreshVoteAsync();

            private async Task RefreshVoteAsync()
            {
                DisableRefresh();
                PageInfo = await Navigator.SearchForZhuLinAsync(true);
                if (PageInfo != null)
                {
                    NumVotesText.Text = PageInfo.Votes?.ToString()?? "无法获取";
                    PopularityText.Text = PageInfo.Popularity?.ToString()?? "无法获取";
                    RankText.Text = PageInfo.Rank?.ToString()?? "无法获取";
                    LinsPage.NavigateUri = PageInfo.PageUrl != null? new Uri(PageInfo.PageUrl) : null;
                    LinsProfile.NavigateUri = PageInfo.ProfileUrl != null ? new Uri(PageInfo.ProfileUrl) : null;

                    QuestionText.Text = PageInfo.Question.Title?.TrimQuestionString()?? "无法获取问题";
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
                            rb.Click += RbClick;
                            ChoicesPanel.Children.Add(rb);
                        }
                    }
                }
                else
                {
                    // TODO report error
                }
                RestoreRefresh();
                State = States.Loaded;
            }

            public void DisableRefresh()
            {
                RefreshButton.Content = "正在刷新……";
                VoteButton.IsEnabled = false;
                RefreshButton.IsEnabled = false;
                CollapseVote();
            }

            public void RestoreRefresh()
            {
                RefreshButton.IsEnabled = true;
                VoteButton.IsEnabled = true;
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

        private const int Vote1 = 43;
        private const int Vote2 = 1069;

        private List<InfoDependentUI> _infoDepUIList = new List<InfoDependentUI>();

        public MainWindow()
        {
            InitializeComponent();
            InitInfoDepdentUI();
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

                LinksPanel = VotePage1,
                LinsPage = LinsPage1,
                LinsProfile = LinsProf1,

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

                LinksPanel = VotePage2,
                LinsPage = LinsPage2,
                LinsProfile = LinsProf2,

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

        private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
