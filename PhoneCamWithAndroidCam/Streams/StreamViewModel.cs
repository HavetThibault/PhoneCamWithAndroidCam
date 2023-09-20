using AndroidCamClient;
using ProcessingPipelines.ImageProcessingPipeline;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using Helper.MVVM;
using System.Windows;
using System.Windows.Media.Animation;
using PhoneCamWithAndroidCam.ProcessPerformances;

namespace PhoneCamWithAndroidCam.Streams
{
    public class StreamViewModel : BindableClass
    {
        private ImageSource _mainImageSource = null;
        private Dispatcher _uiDispatcher;
        private ConvertToRawJpegThread _convertToRawJpegThreads;
        private ProducerConsumerBuffers<byte[]> _convertToRawJpegOutput;
        private CancellationTokenSource _globalCancellationToken;
        private CancellationTokenSource _stopRefreshMainPictureThread;
        private Thread _refreshMainPictureThread;
        private int _fps;
        private bool _isStreaming;
        private volatile bool _isDisposed = false;

        private ImageProcessingPipeline _pipeline;

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

        public ImageProcessingPipeline Pipeline
        {
            get => _pipeline;
            set
            {
                _pipeline = value;
                _convertToRawJpegThreads.InputBuffers = _pipeline.OutputBuffers;
                ProcessPerformancesViewModel.RefreshProcessPerformances(
                    _pipeline,
                    _convertToRawJpegThreads.ProcessPerformances);
                NotifyPropertyChanged(nameof(PipelineName));
            }
        }

        public string PipelineName
        {
            get => Pipeline.Name;
            set
            {
                Pipeline.Name = value;
                NotifyPropertyChanged(nameof(PipelineName));
            }
        }

        public ProcessPerformancesViewModel ProcessPerformancesViewModel { get; set; }

        public StreamViewModel(Dispatcher uiDispatcher, ImageProcessingPipeline imageProcessingPipeline)
        {
            _uiDispatcher = uiDispatcher;
            _isStreaming = false;
            _convertToRawJpegOutput = new(10);
            _convertToRawJpegThreads = new(_uiDispatcher, imageProcessingPipeline.OutputBuffers, _convertToRawJpegOutput);
            ProcessPerformancesViewModel = new(imageProcessingPipeline);
            Pipeline = imageProcessingPipeline;
        }

        public void PlayStreaming(CancellationTokenSource globalCancellationToken)
        {
            _globalCancellationToken = globalCancellationToken;
            _isStreaming = true;

            if (_convertToRawJpegOutput.IsDisposed)
            {
                _convertToRawJpegOutput = new(10);
                _convertToRawJpegThreads.OutputBuffers = _convertToRawJpegOutput;
            }

            Pipeline.Start(globalCancellationToken);
            _convertToRawJpegThreads.LaunchNewWorker(globalCancellationToken);
            _refreshMainPictureThread = new Thread(RefreshMainPicture)
            {
                Name = nameof(RefreshMainPicture)
            };
            _refreshMainPictureThread.Start(globalCancellationToken);
        }

        private void RefreshMainPicture(object? cancellationTokenObj)
        {
            if (cancellationTokenObj is CancellationTokenSource cancellationToken)
            {
                Stopwatch watch = new();
                int framesNbrInASecond = 0;
                _stopRefreshMainPictureThread = new();
                while (!cancellationToken.IsCancellationRequested && !_isDisposed && !_stopRefreshMainPictureThread.IsCancellationRequested)
                {
                    watch.Start();
                    byte[]? frame = _convertToRawJpegOutput.GetRawFrame();

                    if (frame == null)
                        return;

                    var frameMemoryStream = new MemoryStream(frame);
                    var updateMainPictureOperation = _uiDispatcher.BeginInvoke(UpdateMainPicture, frameMemoryStream);

                    while (!updateMainPictureOperation.Task.IsCompleted)
                    {
                        if (_stopRefreshMainPictureThread.IsCancellationRequested)
                            break;
                        Thread.Sleep(20);
                    }

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
        }

        public void StopRefreshMainPictureThread()
        {
            _convertToRawJpegOutput.Dispose();
            _stopRefreshMainPictureThread?.Cancel();
            _refreshMainPictureThread?.Join();
        }

        private void UpdateMainPicture(MemoryStream memoryStream)
        {
            MainImageSource = BitmapExtension.Convert(memoryStream);
        }

        public void Dispose()
        {
            _isDisposed = true;
            Pipeline?.Dispose();
            _convertToRawJpegOutput?.Dispose();
        }
    }
}
