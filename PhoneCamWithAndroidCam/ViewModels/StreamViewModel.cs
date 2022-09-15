using AndroidCamClient;
using ProcessingPipelines.ImageProcessingPipeline;
using ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using WpfUtils;

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class StreamViewModel : BindableClass
    {
        private ImageSource _mainImageSource;
        private Dispatcher _uiDispatcher;
        private ConvertToRawJpegThread _convertToRawJpegThreads;
        private ListBuffering<byte[]> _convertToRawJpegOutput;
        private ImageProcessingPipeline _imageProcessingPipeline;
        private int _fps;
        private bool _isStreaming;

        public int Fps
        {
            get => _fps;
            set => SetProperty(ref _fps, value);
        }

        public ImageSource MainImageSource
        {
            get => _mainImageSource;
            set => SetProperty(ref _mainImageSource, value);
        }

        public bool IsStreaming
        {
            get => _isStreaming;
            set => SetProperty(ref _isStreaming, value);
        }

        public StreamViewModel(Dispatcher uiDispatcher, ImageProcessingPipeline imageProcessingPipeline)
        {
            _uiDispatcher = uiDispatcher;
            _convertToRawJpegOutput = new(10);
            _imageProcessingPipeline = imageProcessingPipeline;
        }

        public void LaunchStreaming(CancellationTokenSource cancellationTokenSource)
        {
            IsStreaming = true;
            _convertToRawJpegThreads = new(_imageProcessingPipeline.GetOutputBuffer(), _convertToRawJpegOutput);
            _imageProcessingPipeline.StartPipeline(cancellationTokenSource);
            _convertToRawJpegThreads.LaunchNewWorker(cancellationTokenSource);
            new Thread(RefreshMainPicture).Start(cancellationTokenSource);
        }

        private void RefreshMainPicture(object? cancellationTokenSourceObj)
        {
            if(cancellationTokenSourceObj is CancellationTokenSource cancellationTokenSource)
            {
                Stopwatch watch = new();
                int framesNbrInASecond = 0;
                try
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
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
                            Fps = framesNbrInASecond;
                            framesNbrInASecond = 0;
                        }
                    }
                }
                catch { } // Catch if GetRawFrame throw exception
            }
        }

        private void UpdateMainPicture(MemoryStream memoryStream)
        {
            MainImageSource = Utils.Convert(memoryStream); 
        }

        public void Dispose()
        {
            _imageProcessingPipeline.InputBuffer?.Dispose();
            _imageProcessingPipeline.GetOutputBuffer()?.Dispose();
            _convertToRawJpegOutput?.Dispose();
        }
    }
}
