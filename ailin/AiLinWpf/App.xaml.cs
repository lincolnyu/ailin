#define NO_SINGLE_INSTANCE

using Squirrel;
using System;
#if !NO_SINGLE_INSTANDCE
using System.Threading;
#endif
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
#if !NO_SINGLE_INSTANCE
        private static Mutex _mutex = new Mutex(true, "{f0791ebc-4bff-484d-8199-e945b46bbed0}");    
#endif
        private static MainWindow _mainWindow = null;

        App()
        {
            InitializeComponent();
        }

        private static async Task CheckForUpdate()
        {
            var title = _mainWindow.Title;
            try
            {
                using (var mgr = UpdateManager.GitHubUpdateManager(GitHubHost))
                {
                    var res = await mgr.Result.UpdateApp();
                    var ver = res?.Version?.Version;
                    if (ver != null)
                    {
                        MessageBox.Show($"成功安装更新版本{ver.Major}.{ver.Minor}.{ver.Build}", title);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message + Environment.NewLine;
                if (ex.InnerException != null)
                {
                    message += ex.InnerException.Message;
                }
                MessageBox.Show("无法安装更新：\n" + message, title);
            }
        }

        [STAThread]
        static void Main()
        {
#if !NO_SINGLE_INSTANCE
            if (_mutex.WaitOne(TimeSpan.Zero, true))
            {
#endif
                App app = new App();
                _mainWindow = new MainWindow();
                app.Run(_mainWindow);
#if !DEBUG
                CheckForUpdate().Wait();
#endif

#if !NO_SINGLE_INSTANCE
                _mutex.ReleaseMutex();
            }
            else
            {
                _mainWindow.WindowState = WindowState.Normal;
            }
#endif
        }

    }
}
