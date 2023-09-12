using Helper.MVVM;
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
using PhoneCamWithAndroidCam.ViewModels.PipelineEditor;

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class StreamsViewModel : BindableClass
    {
        private DuplicateBuffersThread _duplicateBuffersThread;
        private Dispatcher _uiDispatcher;
        private DisplayStreamViewModel _parent;
        private CancellationTokenSource _lastGlobalCancellationToken;

        public ObservableCollection<StreamViewModel> StreamViews { get; set; }

        public RelayCommand AddPipelineCommand { get; set; }
        public RelayCommand MoveUpCommand { get; set; }
        public RelayCommand MoveDownCommand { get; set; }
        public RelayCommand EditCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }

        public StreamsViewModel(DisplayStreamViewModel parent, Dispatcher uiDispatcher, ProducerConsumerBuffers pipelineInput)
        {
            _parent = parent;
            _uiDispatcher = uiDispatcher;
            _duplicateBuffersThread = new(pipelineInput);
            StreamViews = new() { };

            AddPipelineCommand = new(AddPipeline);
            MoveUpCommand = new(MoveUp);
            MoveDownCommand = new(MoveDown);
            EditCommand = new(Edit);
            DeleteCommand = new(DeleteAndDispose);
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

        private void Edit(object sender)
        {
            var inputBuffer = _duplicateBuffersThread.AddNewOutputBuffer();
            var editedViewModel = (StreamViewModel)sender;
            var pipelineEditorViewModel = new PipelineEditorViewModel(editedViewModel.Pipeline, inputBuffer);
            var pipelineEditorControl = new PipelineEditorControl(pipelineEditorViewModel);
            var dialogWindowViewModel = new DialogWindowViewModel("Create new pipeline");
            new DialogWindow(pipelineEditorControl, dialogWindowViewModel).ShowDialog();

            if (dialogWindowViewModel.DialogResult is false)
                return;

            var newPipeline = new ImageProcessingPipeline(
                editedViewModel.Pipeline.InputBuffer,
                pipelineEditorViewModel.Pipeline);
            editedViewModel.Pipeline.Dispose();
            editedViewModel.Pipeline = newPipeline;
            if (!_lastGlobalCancellationToken.IsCancellationRequested)
                editedViewModel.PlayStreaming(_lastGlobalCancellationToken);
        }

        private void DeleteAndDispose(object sender)
        {
            var deletedViewModel = (StreamViewModel)sender;
            StreamViews.Remove(deletedViewModel);
            _duplicateBuffersThread.DeleteOutputBuffer(deletedViewModel.Pipeline.InputBuffer);
            deletedViewModel.Dispose();
        }

        private void AddPipeline(object parameter)
        {
            var inputBuffer = _duplicateBuffersThread.AddNewOutputBuffer();

            var pipelineEditorViewModel = new PipelineEditorViewModel(inputBuffer);
            var pipelineEditorControl = new PipelineEditorControl(pipelineEditorViewModel);
            var dialogWindowViewModel = new DialogWindowViewModel("Create new pipeline");
            new DialogWindow(pipelineEditorControl, dialogWindowViewModel).ShowDialog();

            if (dialogWindowViewModel.DialogResult is false)
            {
                _duplicateBuffersThread.DeleteOutputBuffer(inputBuffer);
                return;
            }
            AddPipelineFromPipelineEditorViewModel(pipelineEditorViewModel);
        }

        private void AddPipelineFromPipelineEditorViewModel(PipelineEditorViewModel pipelineEditorViewModel)
        {
            var newPipeline = pipelineEditorViewModel.Pipeline;
            var streamViewModel = new StreamViewModel(_uiDispatcher, newPipeline);
            StreamViews.Add(streamViewModel);

            if (_parent.IsStreaming)
                streamViewModel.PlayStreaming(_parent.PipelineCancellationToken);
        }

        internal void PlayStreaming(CancellationTokenSource cancellationToken)
        {
            _lastGlobalCancellationToken = cancellationToken;
            _duplicateBuffersThread.Start(cancellationToken);
            foreach(var streamView in StreamViews)
                streamView.PlayStreaming(cancellationToken);
        }

        internal void Dispose()
        {
            foreach (var streamView in StreamViews)
                streamView.Dispose();
        }
    }
}
