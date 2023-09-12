using AndroidCamClient;
using PhoneCamWithAndroidCam.Models;
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
        private ImageSource _mainImageSource;
        private Dispatcher _uiDispatcher;
        private ConvertToRawJpegThread _convertToRawJpegThreads;
        private ProducerConsumerBuffers<byte[]> _convertToRawJpegOutput;
        private Timer _refreshPipelinePerfs;
        private CancellationTokenSource _globalCancellationToken;
        private int _fps;
        private bool _isStreaming;
        private volatile bool _isDisposed = false;
        private string _pipelineName;

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

        public ImageProcessingPipeline Pipeline { get; set; }

        public string PipelineName => Pipeline.Name;

        public ProcessPerformancesViewModel ProcessPerformancesViewModel { get; set; }

        public StreamViewModel(Dispatcher uiDispatcher, ImageProcessingPipeline imageProcessingPipeline)
        {
            _uiDispatcher = uiDispatcher;
            _convertToRawJpegOutput = new(10);
            Pipeline = imageProcessingPipeline;
            ProcessPerformancesViewModel = new(uiDispatcher);
        }

        public void PlayStreaming(CancellationTokenSource globalCancellationToken)
        {
            _globalCancellationToken = globalCancellationToken;
            IsStreaming = true;
            _convertToRawJpegThreads = new(Pipeline.OutputBuffer, _convertToRawJpegOutput);
            Pipeline.Start(globalCancellationToken);
            _convertToRawJpegThreads.LaunchNewWorker(globalCancellationToken);
            var refreshMainPicturethread = new Thread(RefreshMainPicture)
            {
                Name = nameof(RefreshMainPicture)
            };
            refreshMainPicturethread.Start(globalCancellationToken);
            _refreshPipelinePerfs = new(RefreshImageProcessingPipelinePerfs, null, 400, 1000);
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

        private void RefreshImageProcessingPipelinePerfs(object? arg)
        {
            if(!_globalCancellationToken.IsCancellationRequested)
            {
                List<ProcessPerformances> perfs = new(Pipeline.ElementsProcessPerformances)
                {
                    _convertToRawJpegThreads.ProcessPerformances
                };
                ProcessPerformancesViewModel.UpdatePerformances(perfs);
            }
            else
                _refreshPipelinePerfs.Dispose();
        }

        private void UpdateMainPicture(MemoryStream memoryStream)
        {
            MainImageSource = BitmapExtension.Convert(memoryStream); 
        }
 
        public void Dispose()
        {
            _isDisposed = true;
            _refreshPipelinePerfs?.Dispose();
            Pipeline?.Dispose();
            _convertToRawJpegOutput?.Dispose();
        }
    }
}
