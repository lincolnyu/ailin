using System.Windows;

namespace AiLinWpf.Views
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow(string title)
        {
            InitializeComponent();

            Title = title;
        }
    }
}
