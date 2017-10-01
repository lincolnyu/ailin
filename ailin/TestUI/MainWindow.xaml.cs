#define TEST_ASYNC
//#define TEST_ASYNC_MULTI
//#define TEST_ADVANCED
using AiLinWpfLib.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TestUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] GoodImageUrls = new string[]
        {
            "http://www.zhulin.net/images/lzmb.jpg",
            "http://wx2.sinaimg.cn/mw690/ab98e598ly1fc5m8aizjpj21rs1fyqv2.jpg",
            "http://r1.ykimg.com/0130391F455691A356625A00E4413F737EF418-80F3-1059-40B8-4FA3147D1345",
            "http://imgsrc.baidu.com/forum/pic/item/3ac79f3df8dcd100c3cd89c7748b4710b8122f86.jpg",
            "http://www.zhulin.net/html/bbs/UploadFile/2006-8/200683115131166.jpg",
            "http://www.ttpaihang.com/image/thumb/u111108181604772795.jpg",
            "http://www.ttpaihang.com/image/thumb/u100718140719780464.jpg"
        };

        private string[] BadImageUrls = new string[]
        {
            "http://badklink/bad.jpg"
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoadImagesOnClick(object sender, RoutedEventArgs e)
        {
            await LoadImages(100);
        }

        private async void MainWindowOnLoaded(object sender, RoutedEventArgs e)
        {
            //await LoadImages();
        }

        private async Task LoadImages(int times = 1)
        {
            for (var i = 0; i < times; i++)
            {
                foreach (var url in GoodImageUrls)
                {
                    var uri = new Uri(url);
#if TEST_ASYNC
                    var img = await ImageHelper.TryLoadImageAsync(uri);
#elif TEST_ASYNC_MULTI
                    var img = await ImageHelper.TryLoadWebImageMultiAttempts(uri, 1);
#elif TEST_ADVANCED
                    var img = await ImageHelper.TryLoadImageAdvanced(uri);
#else
                    var img = ImageHelper.TryLoadImage(uri);
#endif
                    var image = new Image();
                    await image.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                      new Action(() => image.Source = img));
                    PicsList.Items.Add(image);
                }
            }
        }
    }
}
