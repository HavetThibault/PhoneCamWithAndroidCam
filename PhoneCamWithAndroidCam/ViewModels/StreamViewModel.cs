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

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class StreamViewModel : BindableClass
    {
        private ImageSource _mainImageSource = null;
        private Dispatcher _uiDispatcher;
        private ConvertToRawJpegThread _convertToRawJpegThreads;
        private ProducerConsumerBuffers<byte[]> _convertToRawJpegOutput;
        private CancellationTokenSource _globalCancellationToken;
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
                ProcessPerformancesViewmodel.RefreshProcessPerformances(
                    value, 
                    _convertToRawJpegThreads.ProcessPerformances);
            }
        }

        public string PipelineName => Pipeline.Name;

        public ProcessPerformancesViewModel ProcessPerformancesViewmodel { get; set; }

        public StreamViewModel(Dispatcher uiDispatcher, ImageProcessingPipeline imageProcessingPipeline)
        {
            _uiDispatcher = uiDispatcher;
            _isStreaming = false;
            _convertToRawJpegOutput = new(10);
            Pipeline = imageProcessingPipeline;
            ProcessPerformancesViewmodel = new(imageProcessingPipeline);
            _convertToRawJpegThreads = new(_uiDispatcher, Pipeline.OutputBuffer, _convertToRawJpegOutput);
        }

        public void PlayStreaming(CancellationTokenSource globalCancellationToken)
        {
            _globalCancellationToken = globalCancellationToken;
            _isStreaming = true;
            
            Pipeline.Start(globalCancellationToken);
            _convertToRawJpegThreads.LaunchNewWorker(globalCancellationToken);
            var refreshMainPicturethread = new Thread(RefreshMainPicture)
            {
                Name = nameof(RefreshMainPicture)
            };
            refreshMainPicturethread.Start(globalCancellationToken);
        }

        private void RefreshMainPicture(object? cancellationTokenSourceObj)
        {
            if(cancellationTokenSourceObj is CancellationTokenSource cancellationTokenSource)
            {
                Stopwatch watch = new();
                int framesNbrInASecond = 0;
                while (!cancellationTokenSource.IsCancellationRequested && !_isDisposed)
                {
                    watch.Start();
                    byte[]? frame = _convertToRawJpegOutput.GetRawFrame();

                    if (frame == null)
                        return;

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
