using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WebKit;
using WebKit.Helpers;

namespace AiLinWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Vote1 = 43;
        private const int Vote2 = 1069;

        private VotePageNavigator _vpn1 = new VotePageNavigator(Vote1);
        private VotePageNavigator _vpn2 = new VotePageNavigator(Vote2);

        private PageInfo _pi1;
        private PageInfo _pi2;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Tab1OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshVote1();
        }

        private void Tab2OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshVote2();
        }

        private void RefreshTab1(object sender, RoutedEventArgs e)
        {
            RefreshVote1();
        }

        private void RefreshTab2(object sender, RoutedEventArgs e)
        {
            RefreshVote2();
        }

        private async void RefreshVote1()
        {
            DisableRefresh(Refresh1, VoteBtn1, QuestionForm1);
            _pi1 = await _vpn1.SearchForZhuLinAsync(true);
            if (_pi1 != null)
            {
                NumVotes1.Text = _pi1.Votes.ToString();
                Popularity1.Text = _pi1.Popularity.ToString();
                LinsPage1.NavigateUri = new Uri(_pi1.PageUrl);

                Question1.Text = _pi1.Question.Title.TrimQuestionString();
                Choices1.Children.Clear();
                foreach (var c in _pi1.Question.Choices)
                {
                    var rb = new RadioButton
                    {
                        Tag = c,
                        Content = c.Text.TrimQuestionString()
                    };
                    rb.Click += Rb1Click;
                    Choices1.Children.Add(rb);
                }
            }
            else
            {
                // TODO report error
            }
            RestoreRefresh(Refresh1, VoteBtn1, QuestionForm1);
        }

        private async void RefreshVote2()
        {
            DisableRefresh(Refresh2, VoteBtn2, QuestionForm2);
            _pi2 = await _vpn2.SearchForZhuLinAsync(true);
            if (_pi2 != null)
            {
                NumVotes2.Text = _pi2.Votes.ToString();
                Popularity2.Text = _pi2.Popularity.ToString();
                LinsPage2.NavigateUri = new Uri(_pi2.PageUrl);

                Question2.Text = _pi2.Question.Title.TrimQuestionString();
                Choices2.Children.Clear();
                foreach (var c in _pi2.Question.Choices)
                {
                    var rb = new RadioButton
                    {
                        Tag = c,
                        Content = c.Text.TrimQuestionString()
                    };
                    rb.Click += Rb2Click;
                    Choices2.Children.Add(rb);
                }
            }
            else
            {
                // TODO report error
            }
            RestoreRefresh(Refresh2, VoteBtn2, QuestionForm2);
        }

        private async void Rb1Click(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton)sender;
            var c = (Question.Choice)rb.Tag;
            var s = VotePageNavigator.CreateSubmit(_pi1, c);
            var result = await _vpn1.SubmitAsync(s);
        }

        private async void Rb2Click(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton)sender;
            var c = (Question.Choice)rb.Tag;
            var s = VotePageNavigator.CreateSubmit(_pi2, c);
            var result = await _vpn2.SubmitAsync(s);
        }

        private void DisableRefresh(Button refreshButton, Button voteButton, StackPanel sp)
        {
            refreshButton.Content = "正在刷新……";
            voteButton.IsEnabled = false;
            CollapseVote(voteButton, sp);
            refreshButton.IsEnabled = false;
        }

        private void RestoreRefresh(Button refreshButton, Button voteButton, StackPanel sp)
        {
            refreshButton.IsEnabled = true;
            voteButton.IsEnabled = true;
            CollapseVote(voteButton, sp);
            refreshButton.Content = "刷新";
        }

        private void VoteTab1(object sender, RoutedEventArgs e)
        {
            ToggleVote(VoteBtn1, QuestionForm1);
        }

        private void VoteTab2(object sender, RoutedEventArgs e)
        {
            ToggleVote(VoteBtn2, QuestionForm2);
        }

        private void ToggleVote(Button btn, StackPanel sp)
        {
            if (VoteExpanded(sp))
            {
                CollapseVote(btn, sp);
            }
            else
            {
                ExpandVote(btn, sp);
            }
        }

        private bool VoteExpanded(StackPanel sp)
        {
            return sp.Visibility == Visibility.Visible;
        }

        private void ExpandVote(Button btn, StackPanel sp)
        {
            btn.Content = "暂不投票";
            sp.Visibility = Visibility.Visible;
        }

        private void CollapseVote(Button btn, StackPanel sp)
        {
            btn.Content = "马上投票给她";
            sp.Visibility = Visibility.Collapsed;
        }
    }
}
