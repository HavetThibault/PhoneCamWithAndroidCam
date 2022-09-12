using AndroidCamClient.JpegStream;
using ImageProcessingUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WebAPIClients;
using WpfUtils;

namespace PhoneCamWithAndroidCam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PhoneCamClient phoneCamClient;
        private int _frameDisplayed;
        public MainWindow()
        {
            SIMD.LoadAssembly();
            InitializeComponent();

            phoneCamClient = new("192.168.1.37");
            _frameDisplayed = 0;

            Closing += MainWindowClosing;
        }

        private void MainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SIMD.UnloadAssembly();
            phoneCamClient.Dispose();
        }

        private void LaunchStream(object sender, RoutedEventArgs e)
        {
            new Thread(ReceiveAndDisplayPictures).Start();
        }

        private void UpdateMainPicture(MemoryStream memoryStream)
        {
            BitmapImage bmpImage = Utils.Convert(memoryStream); // The bitmap now own the stream, so you must not close the memoryStream
            MainImage.Source = bmpImage;
            LabelCountFrame.Content = ++_frameDisplayed;
        }

        private async void ReceiveAndDisplayPictures()
        {
            Stream mjpegStream = await phoneCamClient.LaunchStream();
            CannyEdgeDetection cannyEdgeDetection = new(320, 240);
            for (int i = 0; i < 1000; i++)
            {
                JpegFrame frame = PhoneCamClient.GetStreamFrame(mjpegStream);
                MemoryStream pngMemoryStream = new(frame.ToFullBytesImage());
                //Second try
                var bmpTryFrame = new Bitmap(pngMemoryStream);
                byte[] pixels = new byte[320 * 240 * 4];
                BitmapHelper.ToByteArray(bmpTryFrame, out _, pixels);
                byte[] cannyResultBuffer = cannyEdgeDetection.ApplyCannyFilter(pixels);
                /*// First try
                MemoryStream bmpMemoryStream = new(JpegConversion.ConvertJpegToBmp(pngMemoryStream));
                Bitmap bmpFrame = new(bmpMemoryStream);
                byte[] resultBuffer = BitmapHelper.CopyBytePixelArray(bmpFrame, out _, out _);
                byte[] cannyResultBuffer = cannyEdgeDetection.ApplyCannyFilter(resultBuffer);*/
                BitmapHelper.FromBgraBufferToBitmap(bmpTryFrame, cannyResultBuffer, 320, 240);
                MemoryStream cannyStream = new();
                bmpTryFrame.Save(cannyStream, ImageFormat.Bmp);
                Dispatcher.Invoke(UpdateMainPicture, cannyStream);
            }
            mjpegStream.Close();
        }
    }
}
