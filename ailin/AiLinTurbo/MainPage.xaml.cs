using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AiLinTurbo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private enum VoteNumber
        {
            A,
            B
        }

        private VoteNumber _voteNumber;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void VoteAClick(object sender, RoutedEventArgs e)
        {
            _voteNumber = VoteNumber.A;
        }

        private void VoteBClick(object sender, RoutedEventArgs e)
        {
            _voteNumber = VoteNumber.B;
        }
    }
}
