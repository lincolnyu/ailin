using Squirrel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AiLinWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// <remarks>
    ///  Modified according to
    ///   https://stackoverflow.com/questions/21789899/how-to-create-single-instance-wpf-application-that-restores-the-open-window-when
    ///   https://ludovic.chabant.com/devblog/2010/04/20/writing-a-custom-main-method-for-wpf-applications/
    /// </remarks>
    public partial class App : Application
    {
        const string GitHubHost = "https://github.com/lincolnyu/ailin";
        private static Mutex _mutex = new Mutex(true, "{f0791ebc-4bff-484d-8199-e945b46bbed0}");
        private static MainWindow _mainWindow = null;

        App()
        {
            InitializeComponent();
        }

        private static async Task CheckForUpdate()
        {
            try
            {
                using (var mgr = UpdateManager.GitHubUpdateManager(GitHubHost))
                {
                    await mgr.Result.UpdateApp();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message + Environment.NewLine;
                if (ex.InnerException != null)
                {
                    message += ex.InnerException.Message;
                }
                MessageBox.Show(message, "无法安装更新");
            }
        }


        [STAThread]
        static void Main()
        {
            if (_mutex.WaitOne(TimeSpan.Zero, true))
            {
#if !DEBUG
                CheckForUpdate().Wait();
#endif
                App app = new App();
                _mainWindow = new MainWindow();
                app.Run(_mainWindow);
                _mutex.ReleaseMutex();
            }
            else
            {
                _mainWindow.WindowState = WindowState.Normal;
            }
        }

    }
}
