using AndroidCamClient;
using AndroidCamClient.JpegStream;
using ImageProcessingUtils;
using PhoneCamWithAndroidCam.ViewModels;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfUtils;

namespace PhoneCamWithAndroidCam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            SIMD.LoadAssembly();
            InitializeComponent();

            _mainViewModel = new MainViewModel(Dispatcher);
            DataContext = _mainViewModel;

            Closing += MainWindowClosing;
        }

        private void MainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _mainViewModel.Dispose();
            SIMD.UnloadAssembly();
        }
    }
}
