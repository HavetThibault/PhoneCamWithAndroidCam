using AndroidCamClient;
using ProcessingPipelines.ImageProcessingPipeline;
using ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using WpfUtils;

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class DisplayStreamViewModel : BindableClass, IDisposable
    {
        private int _fps = 0;
        private bool _isStreaming = false;
        private PhoneCamClient _phoneCamClient;
        private CancellationTokenSource _pipelineCancellationTokenSource;
        private PipelineFeederPipeline _pipelineFeeder;
        private DuplicateBuffersThread _duplicateBuffersThread;
       
        private MultipleBuffering _pipelineFeederOutput;

        private Timer _refreshProcessTimer;

        private List<StreamViewModel> _streamViews;

        public ProcessPerformancesViewModel ProcessPerformancesViewModel { get; set; }

        public List<StreamViewModel> StreamViews
        {
            get => _streamViews;
            set => SetProperty(ref _streamViews, value);
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

        public DisplayStreamViewModel(Dispatcher uiDispatcher, ProcessPerformancesViewModel processPerformancesViewModel)
        {
            CommandLaunchStreaming = new RelayCommand(LaunchStreaming, CanLaunchStreaming);
            CommandStopStreaming = new RelayCommand(StopStreaming, CanStopStreaming);
            _phoneCamClient = new("192.168.0.33");
            _pipelineFeederOutput = new(320, 240, 320 * 4, 10, EBufferPixelsFormat.Bgra32Bits);
            ProcessPerformancesViewModel = processPerformancesViewModel;

            _duplicateBuffersThread = new(_pipelineFeederOutput);
            MultipleBuffering outputBuffer1 = _duplicateBuffersThread.AddNewOutputBuffer();
            MultipleBuffering outputBuffer2 = _duplicateBuffersThread.AddNewOutputBuffer();
            ImageProcessingPipeline cannyImageProcessingPipeline = CannyImageProcessingPipeline.CreateCannyImageProcessingPipeline(outputBuffer1);
            ImageProcessingPipeline changingColorProcessingPipeline = ChangingColorImageProcessingPipeline.CreateChangingColorImageProcessingPipeline(outputBuffer2);
            _streamViews = new() { new(uiDispatcher, cannyImageProcessingPipeline), new StreamViewModel(uiDispatcher, changingColorProcessingPipeline) };
        }

        public void LaunchStreaming()
        {
            IsStreaming = true;
            _pipelineFeeder = new PipelineFeederPipeline(_phoneCamClient, _pipelineFeederOutput);
            _pipelineCancellationTokenSource = new();
            _pipelineFeeder.StartFeeding(_pipelineCancellationTokenSource);
            _duplicateBuffersThread.Start(_pipelineCancellationTokenSource);
            _refreshProcessTimer = new Timer(RefreshProcessTime, null, 400, 1000);
            foreach(var streamView in _streamViews)
            {
                streamView.LaunchStreaming(_pipelineCancellationTokenSource);
            }
        }

        public bool CanLaunchStreaming()
        {
            return !IsStreaming;
        }

        public void StopStreaming()
        {
            IsStreaming = false;
            _pipelineCancellationTokenSource.Cancel();
            _refreshProcessTimer.Dispose();
        }

        public bool CanStopStreaming()
        {
            return IsStreaming;
        }

        public void RefreshProcessTime(object? arg)
        {
            List<ProcessPerformances> perfsList = new()
            {
                _pipelineFeeder.ProcessRawJpegPerf,
                _pipelineFeeder.ProcessRawJpegStreamPerf,
                _pipelineFeeder.ProcessBitmapsPerf
            };

            ProcessPerformancesViewModel.UpdatePerformances(perfsList);
        }

        public void Dispose()
        {
            _pipelineFeeder?.Dispose();
            _pipelineFeederOutput?.Dispose();
            foreach (var streamView in _streamViews)
            {
                streamView.Dispose();
            }
        }
    }
}
