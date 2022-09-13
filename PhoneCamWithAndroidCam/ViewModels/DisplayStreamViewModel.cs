using AndroidCamClient;
using ImageProcessingUtils;
using ImageProcessingUtils.Pipeline;
using PhoneCamWithAndroidCam.Threads;
using ProcessingPipelines.ImageProcessingPipeline;
using ProcessingPipelines.PipelineFeeder;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfUtils;
using BitmapFrame = ImageProcessingUtils.Pipeline.BitmapFrame;

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class DisplayStreamViewModel : BindableClass, IDisposable
    {
        private int _fps = 0;
        private bool _isStreaming = false;
        private PhoneCamClient _phoneCamClient;
        private CancellationTokenSource _pipelineCancellationTokenSource;
        private PipelineFeederPipeline _pipelineFeeder;
        private Dispatcher _uiDispatcher;
        private ImageSource _mainImageSource;
        private MultipleBuffering _pipelineFeederOutput;
        private ListBuffering<byte[]> _convertToRawJpegOutput;
        private ConvertToRawJpegThread _convertToRawJpegThreads;

        public ImageSource MainImageSource
        {
            get => _mainImageSource;
            set => SetProperty(ref _mainImageSource, value);
        }

        public int Fps
        {
            get => _fps;
            set => SetProperty(ref _fps, value);
        }

        public bool IsStreaming
        {
            get => _isStreaming;
            set
            {
                SetProperty(ref _isStreaming, value);
                CommandLaunchStreaming.RaiseCanExecuteChanged();
                CommandStopStreaming.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand CommandLaunchStreaming { get; set; }
        public RelayCommand CommandStopStreaming { get; set; }

        public DisplayStreamViewModel(Dispatcher uiDispatcher)
        {
            CommandLaunchStreaming = new RelayCommand(LaunchStreaming, CanLaunchStreaming);
            CommandStopStreaming = new RelayCommand(StopStreaming, CanStopStreaming);
            _phoneCamClient = new("192.168.1.37");
            _uiDispatcher = uiDispatcher;
            _pipelineFeederOutput = new(320, 240, 320 * 4, 10, EBufferPixelsFormat.Bgra32Bits);
            _convertToRawJpegOutput = new(10);
        }

        public void LaunchStreaming()
        {
            IsStreaming = true;
            _pipelineFeeder = new(_phoneCamClient, _pipelineFeederOutput);
            _pipelineCancellationTokenSource = new();
            _convertToRawJpegThreads = new(_pipelineFeederOutput, _convertToRawJpegOutput);
            _pipelineFeeder.StartFeeding(_pipelineCancellationTokenSource);
            _convertToRawJpegThreads.LaunchNewWorker(_pipelineCancellationTokenSource);
            new Thread(RefreshMainPicture).Start();
        }

        public bool CanLaunchStreaming()
        {
            return !IsStreaming;
        }

        public void StopStreaming()
        {
            IsStreaming = false;
            _pipelineCancellationTokenSource.Cancel();
        }

        public bool CanStopStreaming()
        {
            return IsStreaming;
        }

        public void RefreshMainPicture()
        {
            Stopwatch watch = new();
            int framesNbrInASecond = 0;
            while (!_pipelineCancellationTokenSource.IsCancellationRequested)
            {
                watch.Start();
                byte[] frame = _convertToRawJpegOutput.GetRawFrame();
                    
                MemoryStream simpleStream = new(frame);
                _uiDispatcher.Invoke(UpdateMainPicture, simpleStream);

                watch.Stop();
                framesNbrInASecond++;
                if (watch.ElapsedMilliseconds > 1000)
                {
                    watch.Reset();
                    _uiDispatcher.Invoke(UpdateFps, framesNbrInASecond);
                    framesNbrInASecond = 0;
                }
            }
        }

        private void UpdateMainPicture(MemoryStream memoryStream)
        {
            MainImageSource = Utils.Convert(memoryStream); // The bitmap now own the stream, so you must not close the memoryStream
        }

        private void UpdateFps(int fps)
        {
            Fps = fps;
        }

        public void Dispose()
        {
            _pipelineFeeder.Dispose();
            _pipelineFeederOutput.Dispose();
        }
    }
}
