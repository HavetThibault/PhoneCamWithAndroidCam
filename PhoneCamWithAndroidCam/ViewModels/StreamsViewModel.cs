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
using PhoneCamWithAndroidCam.Views;
using System.Collections.ObjectModel;
using PropertyTools.Wpf;
using System.Windows;
using Wpf.Common.Controls;

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class StreamsViewModel : BindableClass
    {
        private DuplicateBuffersThread _duplicateBuffersThread;
        private Dispatcher _uiDispatcher;
        private DisplayStreamViewModel _parent;

        public ObservableCollection<StreamViewModel> StreamViews { get; set; }

        public RelayCommand AddPipelineCommand { get; set; }
        public RelayCommand MoveUpCommand { get; set; }
        public RelayCommand MoveDownCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }

        public StreamsViewModel(DisplayStreamViewModel parent, Dispatcher uiDispatcher, ProducerConsumerBuffers pipelineInput)
        {
            _parent = parent;
            _uiDispatcher = uiDispatcher;
            _duplicateBuffersThread = new(pipelineInput);
            ProducerConsumerBuffers outputBuffer = _duplicateBuffersThread.AddNewOutputBuffer();
            ImageProcessingPipeline cannyImageProcessingPipeline = CannyPipeline.GetInstance(outputBuffer);
            StreamViews = new() { new(uiDispatcher, cannyImageProcessingPipeline) };

            AddPipelineCommand = new(AddPipeline);
            MoveUpCommand = new(MoveUp);
            MoveDownCommand = new(MoveDown);
            DeleteCommand = new(Delete);
        }

        private void MoveUp(object sender)
        {
            int senderViewModelIndex = -1;
            for(int i = 0; i < StreamViews.Count; i++)
            {
                if (sender == StreamViews[i])
                {
                    senderViewModelIndex = i;
                    break;
                }
            }

            if (senderViewModelIndex == 0)
                return;

            StreamViews.Move(senderViewModelIndex, senderViewModelIndex - 1);
        }

        private void MoveDown(object sender)
        {
            int senderViewModelIndex = -1;
            for (int i = 0; i < StreamViews.Count; i++)
            {
                if (sender == StreamViews[i])
                {
                    senderViewModelIndex = i;
                    break;
                }
            }

            if (senderViewModelIndex == StreamViews.Count - 1)
                return;

            StreamViews.Move(senderViewModelIndex, senderViewModelIndex + 1);
        }

        private void Delete(object sender)
        {
            var deletedViewModel = sender as StreamViewModel;
            StreamViews.Remove(deletedViewModel);
            deletedViewModel.Dispose();
        }

        private void AddPipeline(object parameter)
        {
            var pipelineChooserViewModel = new PipelineChooserViewModel(ImageProcessingPipeline.GetAllPipelineNames().Values);
            var pipelineChooserView = new PipelineChooserView(pipelineChooserViewModel);
            var dialogWindow = new DialogWindow(pipelineChooserView, pipelineChooserViewModel);
            dialogWindow.ShowDialog();

            string selectedPipeline = pipelineChooserViewModel.SelectedPipeline;
            if (!pipelineChooserViewModel.DialogResult || selectedPipeline is null)
                return;

            var outputBuffer = _duplicateBuffersThread.AddNewOutputBuffer();
            var newPipeline = ImageProcessingPipeline.GetPipelineFromName(selectedPipeline, outputBuffer);
            var streamViewModel = new StreamViewModel(_uiDispatcher, newPipeline);
            StreamViews.Add(streamViewModel);

            if (_parent.IsStreaming)
                streamViewModel.PlayStreaming(_parent.PipelineCancellationToken);
        }

        internal void PlayStreaming(CancellationTokenSource cancellationToken)
        {
            _duplicateBuffersThread.Start(cancellationToken);
            foreach(var streamView in StreamViews)
                streamView.PlayStreaming(cancellationToken);
        }

        public void StopStreaming()
        {
            foreach (var streamView in StreamViews)
                streamView.PauseStreaming();
        }

        internal void Dispose()
        {
            foreach (var streamView in StreamViews)
                streamView.Dispose();
        }
    }
}
