using AndroidCamClient;
using ImageProcessingUtils;
using PhoneCamWithAndroidCam.Threads;
using ProcessingPipelines.PipelineFeeder;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfUtils;

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class MainViewModel : BindableClass
    {
        private BindableClass _currentViewModel;
        private DisplayStreamViewModel _displayStreamViewModel;

        public BindableClass CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public MainViewModel(Dispatcher uiDispatcher)
        {
            _displayStreamViewModel = new DisplayStreamViewModel(uiDispatcher);
            CurrentViewModel = _displayStreamViewModel;
        }

        public void Dispose()
        {
            _displayStreamViewModel.Dispose();
        }
    }
}
