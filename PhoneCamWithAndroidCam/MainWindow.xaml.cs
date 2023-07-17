using ImageProcessingUtils;
using PhoneCamWithAndroidCam.ViewModels;
using System.Windows;
using Wpf.Common.Controls;

namespace PhoneCamWithAndroidCam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow
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
