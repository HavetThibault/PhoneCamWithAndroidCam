using AndroidCamClient;
using ImageProcessingUtils;
using ImageProcessingUtils.Pipeline;
using PhoneCamWithAndroidCam.Threads;
using ProcessingPipelines.PipelineFeeder;
using System;
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
        }

        public void LaunchStreaming()
        {
            IsStreaming = true;
            MultipleBuffering outputMultipleBuffering = new(320, 240, 320 * 4, 10, EBufferPixelsFormat.Bgra32Bits);
            _pipelineFeeder = new(_phoneCamClient, outputMultipleBuffering);
            _pipelineCancellationTokenSource = new();
            _pipelineFeeder.StartFeeding(_pipelineCancellationTokenSource);
            new Thread(RefreshMainPicture).Start(outputMultipleBuffering);
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

        public void RefreshMainPicture(object? inputMultipleBufferingObj)
        {
            if(inputMultipleBufferingObj is MultipleBuffering inputMultipleBuffering)
            {
                while(!_pipelineCancellationTokenSource.IsCancellationRequested)
                {
                    Bitmap bmp;
                    MemoryStream cannyStream;
                    BitmapFrame bitmapFrame = inputMultipleBuffering.WaitNextReaderBuffer();
                    lock (bitmapFrame)
                    {
                        bmp = bitmapFrame.Bitmap;
                        BitmapHelper.FromBgraBufferToBitmap(bitmapFrame.Bitmap, bitmapFrame.Data, 320, 240);
                        cannyStream = new();
                        bitmapFrame.Bitmap.Save(cannyStream, ImageFormat.Jpeg);
                    }
                    bmp.Dispose();
                    inputMultipleBuffering.FinishReading();

                    _uiDispatcher.Invoke(UpdateMainPicture, cannyStream);
                }
            }
        }

        private void UpdateMainPicture(MemoryStream memoryStream)
        {
            BitmapImage bmpImage = Utils.Convert(memoryStream); // The bitmap now own the stream, so you must not close the memoryStream
            MainImageSource = bmpImage;
        }

        public void Dispose()
        {
            
        }
    }
}
