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
using PhoneCamWithAndroidCam.Serialization;
using PhoneCamWithAndroidCam.ViewModels.TemplateManagement;
using Wpf.Common.Controls.Dialog;

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class StreamsViewModel : BindableClass
    {
        private DuplicateBuffersThread _duplicateBuffersThread;
        private Dispatcher _uiDispatcher;
        private DisplayStreamViewModel _parent;
        private CancellationTokenSource _lastGlobalCancellationToken;
        private string _filePath;
        private List<PipelineStructure> _pipelineTemplates;

        public ObservableCollection<StreamViewModel> StreamViews { get; set; }

        public RelayCommand ManageTemplateCommand { get; set; }
        public RelayCommand AddPipelineCommand { get; set; }
        public RelayCommand MoveUpCommand { get; set; }
        public RelayCommand MoveDownCommand { get; set; }
        public RelayCommand EditCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }

        public StreamsViewModel(DisplayStreamViewModel parent, Dispatcher uiDispatcher, 
            ProducerConsumerBuffers pipelineInput, string filePath)
        {
            _filePath = filePath;
            _parent = parent;
            _uiDispatcher = uiDispatcher;
            _duplicateBuffersThread = new(pipelineInput);
            TryLoadStreamViews();

            ManageTemplateCommand = new(ManageTemplate);
            AddPipelineCommand = new(AddPipeline);
            MoveUpCommand = new(MoveUp);
            MoveDownCommand = new(MoveDown);
            EditCommand = new(Edit);
            DeleteCommand = new(DeleteAndDispose);
        }

        private void ManageTemplate(object uselessParameter)
        {
            var manageTemplateViewModel = new ManageTemplateViewModel(GetActivePipelines(), _pipelineTemplates);
            var manageTemplateWindow = new ManageTemplateWindow(manageTemplateViewModel);
            manageTemplateWindow.ShowDialog();

            if(manageTemplateViewModel.DialogResult)
            {
                _pipelineTemplates = manageTemplateViewModel.PipelineTemplates;
            }
        }

        private List<PipelineStructure> GetActivePipelines()
        {
            var activePipelines = new List<PipelineStructure>();
            foreach (var streamView in StreamViews)
                activePipelines.Add(new PipelineStructure(streamView.Pipeline));
            return activePipelines;
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
            bool wasStreaming = editedViewModel.Pipeline.IsStreaming;
            var pipelineEditorViewModel = new PipelineEditorViewModel(
                editedViewModel.Pipeline, 
                inputBuffer,
                _uiDispatcher);
            var pipelineEditorControl = new PipelineEditorControl(pipelineEditorViewModel);
            new DialogWindow(pipelineEditorControl, pipelineEditorViewModel).ShowDialog();

            if (pipelineEditorViewModel.DialogResult is false)
            {
                _duplicateBuffersThread.DeleteOutputBuffer(inputBuffer);
                return;
            }

            var newPipeline = new ImageProcessingPipeline(
                pipelineEditorViewModel.Pipeline,
                _uiDispatcher);
            _duplicateBuffersThread.DeleteOutputBuffer(editedViewModel.Pipeline.InputBuffer);
            editedViewModel.Pipeline.Dispose();
            editedViewModel.StopRefreshMainPictureThread();
            editedViewModel.Pipeline = newPipeline;
            if (wasStreaming)
                editedViewModel.PlayStreaming(_lastGlobalCancellationToken);
        }

        private void DeleteAndDispose(object sender)
        {
            var deletedViewModel = (StreamViewModel)sender;
            StreamViews.Remove(deletedViewModel);
            _duplicateBuffersThread.DeleteOutputBuffer(deletedViewModel.Pipeline.InputBuffer);
            deletedViewModel.Dispose();
        }

        private void AddPipeline(PipelineStructure pipeline)
        {
            var inputBuffer = _duplicateBuffersThread.AddNewOutputBuffer();
            StreamViews.Add(new StreamViewModel(_uiDispatcher, pipeline.InstantiatePipeline(inputBuffer, _uiDispatcher)));
        }

        private void AddPipeline(object uselessParameter)
        {
            var inputBuffer = _duplicateBuffersThread.AddNewOutputBuffer();

            var pipelineEditorViewModel = new PipelineEditorViewModel(inputBuffer, _uiDispatcher);
            var pipelineEditorControl = new PipelineEditorControl(pipelineEditorViewModel);
            new DialogWindow(pipelineEditorControl, pipelineEditorViewModel).ShowDialog();

            if (pipelineEditorViewModel.DialogResult is false)
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
            new StreamsInfo(GetActivePipelines(), _pipelineTemplates).Serialize(_filePath);
            foreach (var streamView in StreamViews)
                streamView.Dispose();
        }

        private void TryLoadStreamViews()
        {
            var streamsInfo = StreamsInfo.TryLoad(_filePath);
            StreamViews = new();
            if (streamsInfo is StreamsInfo)
            {
                foreach (var pipeline in streamsInfo.ActivePipelines)
                    AddPipeline(pipeline);
                _pipelineTemplates = streamsInfo.PipelineTemplates;
            }
        }
    }
}
