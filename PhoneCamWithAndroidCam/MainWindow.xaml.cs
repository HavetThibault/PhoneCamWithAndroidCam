using ImageProcessingUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using WebAPIClients;
using WpfUtils;

namespace PhoneCamWithAndroidCam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            SIMD.LoadAssembly();
            InitializeComponent();

            Closing += MainWindowClosing;
        }

        private void MainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SIMD.UnloadAssembly();
        }

        private async void MockTest(object sender, RoutedEventArgs e)
        {
            PhoneCamClient client = new("192.168.1.37");
            byte[] frame = await client.MockPhoneVideoStream();
            MemoryStream ms = new(frame);
            BitmapImage bmpImage = Utils.Convert(ms);
            ms.Close();
            MainImage.Source = bmpImage;
        }
    }
}
