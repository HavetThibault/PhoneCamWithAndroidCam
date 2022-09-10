using AndroidCamClient.JpegStream;
using ImageProcessingUtils;
using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            SIMD.LoadAssembly();
            InitializeComponent();

            phoneCamClient = new("192.168.1.37");

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
        }

        private async void ReceiveAndDisplayPictures()
        {
            Stream mjpegStream = await phoneCamClient.LaunchStream();
            for (int i = 0; i < 1000; i++)
            {
                JpegFrame frame = PhoneCamClient.GetStreamFrame(mjpegStream);
                MemoryStream memoryStream = new(frame.ToFullBytesImage());
                Dispatcher.Invoke(UpdateMainPicture, memoryStream);
            }
            mjpegStream.Close();
        }
    }
}
