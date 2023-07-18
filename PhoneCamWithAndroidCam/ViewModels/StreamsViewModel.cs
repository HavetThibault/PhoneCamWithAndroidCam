using Helper.MVVM;
using ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines;
using ProcessingPipelines.ImageProcessingPipeline;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class StreamsViewModel : BindableClass
    {
        private DuplicateBuffersThread _duplicateBuffersThread;

        private List<StreamViewModel> _streamViews;

        public List<StreamViewModel> StreamViews
        {
            get => _streamViews;
            set => SetProperty(ref _streamViews, value);
        }

        public RelayCommand AddPipelineCommand { get; set; }

        public StreamsViewModel(Dispatcher uiDispatcher, MultipleBuffering pipelineInput)
        {
            _duplicateBuffersThread = new(pipelineInput);
            MultipleBuffering outputBuffer1 = _duplicateBuffersThread.AddNewOutputBuffer();
            MultipleBuffering outputBuffer2 = _duplicateBuffersThread.AddNewOutputBuffer();
            //MultipleBuffering outputBuffer3 = _duplicateBuffersThread.AddNewOutputBuffer();
            //MultipleBuffering outputBuffer4 = _duplicateBuffersThread.AddNewOutputBuffer();
            ImageProcessingPipeline cannyImageProcessingPipeline = CannyImageProcessingPipeline.CreateCannyImageProcessingPipeline(outputBuffer1);
            ImageProcessingPipeline copyProcessingPipeline = CopyProcessingPipeline.CreateCopyProcessingPipeline(outputBuffer2);
            //ImageProcessingPipeline changingColorPipeline = ChangingColorImageProcessingPipeline.CreateChangingColorImageProcessingPipeline(outputBuffer3);
            //ImageProcessingPipeline motionDetectionPipeline = MotionDetectionProcessingPipeline.CreateCannyImageProcessingPipeline(outputBuffer4);
            _streamViews = new() { new(uiDispatcher, copyProcessingPipeline), new(uiDispatcher, cannyImageProcessingPipeline) };

            AddPipelineCommand = new(AddPipeline);
        }

        private void AddPipeline(object parameter)
        {

        }

        internal void LaunchStreaming(CancellationTokenSource cancellationToken)
        {
            _duplicateBuffersThread.Start(cancellationToken);
            foreach(var streamView in _streamViews)
                streamView.LaunchStreaming(cancellationToken);
        }

        public void StopStreaming()
        {
            foreach (var streamView in _streamViews)
                streamView.StopStreaming();
        }

        internal void Dispose()
        {
            foreach (var streamView in _streamViews)
            {
                streamView.Dispose();
            }
        }
    }
}
