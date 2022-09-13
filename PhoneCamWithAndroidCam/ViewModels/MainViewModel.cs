using AndroidCamClient;
using System;
using WpfUtils;

namespace PhoneCamWithAndroidCam.ViewModels
{
    internal class MainViewModel : BindableClass, IDisposable
    {
        private int _fps = 0;
        private bool _isStreaming = false;
        private PhoneCamClient _phoneCamClient;

        public int Fps
        {
            get => _fps;
            set => SetProperty(ref _fps, value);
        }

        public bool IsStreaming
        {
            get => _isStreaming;
            set => SetProperty(ref _isStreaming, value);
        }

        public RelayCommand CommandLaunchStreaming { get; set; }

        public MainViewModel()
        {
            CommandLaunchStreaming = new RelayCommand(LaunchStreaming, CanLaunchStreaming);
            _phoneCamClient = new("192.168.1.37");
        }

        public void LaunchStreaming()
        {
            IsStreaming = true;

        }

        public bool CanLaunchStreaming()
        {
            return !IsStreaming;
        }

        public void StopStreaming()
        {

        }

        public bool CanStopStreaming()
        {
            return IsStreaming;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
