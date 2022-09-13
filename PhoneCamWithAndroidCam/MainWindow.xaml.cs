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

        /*private void UpdateMainPicture(MemoryStream memoryStream)
        {
            BitmapImage bmpImage = Utils.Convert(memoryStream); // The bitmap now own the stream, so you must not close the memoryStream
            MainImage.Source = bmpImage;
            LabelCountFrame.Content = ++_frameDisplayed;
        }

        private void UpdateFps(int fps)
        {
            LabelFps.Content = fps;
        }

        private async void ReceiveAndDisplayPictures()
        {
            Stream mjpegStream = await phoneCamClient.LaunchStream();
            CannyEdgeDetection cannyEdgeDetection = new(320, 240);
            Stopwatch watch = new();
            int framesNbrInASecond = 0;
            for (int i = 0; i < 1000; i++)
            {
                watch.Start();
                JpegFrame jpegFrame = PhoneCamClient.GetStreamFrame(mjpegStream);
                MemoryStream jpegMemoryStream = new(jpegFrame.ToFullBytesImage());
                var bmpTryFrame = new Bitmap(jpegMemoryStream);
                byte[] pixels = new byte[320 * 240 * 4];
                BitmapHelper.ToByteArray(bmpTryFrame, out _, pixels);
                byte[] cannyResultBuffer = cannyEdgeDetection.ApplyCannyFilter(pixels);
                BitmapHelper.FromBgraBufferToBitmap(bmpTryFrame, cannyResultBuffer, 320, 240);
                MemoryStream cannyStream = new();
                bmpTryFrame.Save(cannyStream, ImageFormat.Jpeg);
                Dispatcher.Invoke(UpdateMainPicture, cannyStream);
                watch.Stop();
                framesNbrInASecond++;
                if (watch.ElapsedMilliseconds > 1000)
                {
                    watch.Reset();
                    Dispatcher.Invoke(UpdateFps, framesNbrInASecond);
                    framesNbrInASecond = 0;
                }
            }
            mjpegStream.Close();
        }*/
    }
}
