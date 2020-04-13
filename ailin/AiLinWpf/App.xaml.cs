//using Squirrel;
using System;
#if !NO_SINGLE_INSTANDCE
using System.Threading;
#endif
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
#if !NO_SINGLE_INSTANCE
        private static Mutex _mutex = new Mutex(true, "{f0791ebc-4bff-484d-8199-e945b46bbed0}");    
#endif
        private static MainWindow _mainWindow = null;

        App()
        {
            InitializeComponent();
        }

        [STAThread]
        static void Main()
        {
            try
            {
#if !NO_SINGLE_INSTANCE
                if (_mutex.WaitOne(TimeSpan.Zero, true))
                {
#endif
                    App app = new App();
                    _mainWindow = new MainWindow();
                    app.Run(_mainWindow);

#if !NO_SINGLE_INSTANCE
                }
                else
                {
                    _mainWindow.WindowState = WindowState.Normal;
                }
#endif
            }
            catch (Exception)
            {
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
    }
}
