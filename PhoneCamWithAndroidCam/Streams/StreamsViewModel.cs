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
using System.Collections.ObjectModel;
using PropertyTools.Wpf;
using System.Windows;
using Wpf.Common.Controls;
using PhoneCamWithAndroidCam.PipelineEditor;
using PhoneCamWithAndroidCam.Serialization;
using PhoneCamWithAndroidCam.TemplateManagement;
using Wpf.Common.Controls.Dialog;
using Wpf.Common.Controls.ElementChooser;
using PhoneCamWithAndroidCam.Main;
using PhoneCamWithAndroidCam.PipelineElementEditor;

namespace PhoneCamWithAndroidCam.Streams
{
    public class StreamsViewModel : BindableClass
    {
        private DuplicateBuffersThread _duplicateBuffersThread;
        private Dispatcher _uiDispatcher;
        private StreamsAndStatsViewModels _parent;
        private CancellationTokenSource _lastGlobalCancellationToken;
        private string _filePath;
        private List<PipelineStructure> _pipelineTemplates;

        public ObservableCollection<StreamViewModel> StreamViews { get; set; }

        public RelayCommand ManageTemplateCommand { get; set; }
        public RelayCommand AddPipelineCommand { get; set; }
        public RelayCommand MoveUpCommand { get; set; }
        public RelayCommand MoveDownCommand { get; set; }
        public RelayCommand EditCommand { get; set; }
        public RelayCommand EditElementParamCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand ImportFromTemplateCommand { get; set; }

        public StreamsViewModel(StreamsAndStatsViewModels parent, Dispatcher uiDispatcher,
            ProducerConsumerBuffers pipelineInput, string filePath)
        {
            _filePath = filePath;
            _parent = parent;
            _uiDispatcher = uiDispatcher;
            _duplicateBuffersThread = new(pipelineInput);

            StreamViews = new();
            TryLoadStreamViewsAndTemplates();

            ManageTemplateCommand = new(ManageTemplate);
            AddPipelineCommand = new(AddPipelineUsingEditorControl);
            MoveUpCommand = new(MoveUp);
            MoveDownCommand = new(MoveDown);
            EditCommand = new(Edit);
            DeleteCommand = new(DeleteAndDispose);
            ImportFromTemplateCommand = new(ImportFromTemplate);
            EditElementParamCommand = new(EditElementParam);
        }

        private void EditElementParam(object sender)
        {
            var streamViewModel = (StreamViewModel)sender;
            var editElementParamViewModel = new PipelineElementEditorViewModel(streamViewModel.Pipeline.PipelineElements);
            var editElementParamControl = new PipelineElementEditorControl(editElementParamViewModel);
            new DialogWindow(editElementParamControl, editElementParamViewModel, 440, 500).ShowDialog();
        }

        private void ImportFromTemplate(object uselessParam)
        {
            var selectionElements = new List<SelectionElement>();
            foreach (var template in _pipelineTemplates)
                selectionElements.Add(new SelectionElement(template.PipelineName, template));
            var selectTemplateViewModel = new ElementChooserViewModel(selectionElements);
            var selectTemplateControl = new ElementChooserControl(selectTemplateViewModel);
            new DialogWindow(selectTemplateControl, selectTemplateViewModel, 380, 250).ShowDialog();

            if (selectTemplateViewModel.DialogResult)
                AddPipeline((PipelineStructure)selectTemplateViewModel.SelectedElement.Value);
        }

        private void ManageTemplate(object uselessParameter)
        {
            var manageTemplateViewModel = new ManageTemplateViewModel(_pipelineTemplates, GetActivePipelinesStructures());
            var manageTemplateControl = new ManageTemplateControl(manageTemplateViewModel);
            new DialogWindow(manageTemplateControl, manageTemplateViewModel, 380, 250).ShowDialog();

            if (manageTemplateViewModel.DialogResult)
                _pipelineTemplates = manageTemplateViewModel.GetPipelineTemplates();
        }

        private List<PipelineStructure> GetActivePipelinesStructures()
        {
            var activePipelines = new List<PipelineStructure>();
            foreach (var streamView in StreamViews)
                activePipelines.Add(new PipelineStructure(streamView.Pipeline));
            return activePipelines;
        }

        private void MoveUp(object sender)
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

        private void AddPipelineUsingEditorControl(object uselessParameter)
        {
            var inputBuffer = _duplicateBuffersThread.AddNewOutputBuffer();

            var pipelineEditorViewModel = new PipelineEditorViewModel(GetNextDefaultPipelineName(), inputBuffer, _uiDispatcher);
            var pipelineEditorControl = new PipelineEditorControl(pipelineEditorViewModel);
            new DialogWindow(pipelineEditorControl, pipelineEditorViewModel, 380, 250, 500, 700).ShowDialog();

            if (pipelineEditorViewModel.DialogResult is false)
            {
                _duplicateBuffersThread.DeleteOutputBuffer(inputBuffer);
                return;
            }
            AddPipelineFromPipelineEditorViewModel(pipelineEditorViewModel);
        }

        private string GetNextDefaultPipelineName()
        {
            var pipelineNames = GetActivePipelineNames();
            string defaultName = "Pipeline";
            if (pipelineNames.Contains(defaultName))
            {
                string nextPipelineName = $"{defaultName} 1";
                for (int i = 2; pipelineNames.Contains(nextPipelineName); i++)
                {
                    nextPipelineName = $"{defaultName} {i}";
                }
                return nextPipelineName;
            }
            return defaultName;
        }

        private List<string> GetActivePipelineNames()
        {
            var activePipelineNames = new List<string>();
            foreach (var streamView in StreamViews)
                activePipelineNames.Add(streamView.PipelineName);
            return activePipelineNames;
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
            foreach (var streamView in StreamViews)
                streamView.PlayStreaming(cancellationToken);
        }

        internal void Dispose()
        {
            new StreamsInfo(GetActivePipelinesStructures(), _pipelineTemplates).Serialize(_filePath);
            foreach (var streamView in StreamViews)
                streamView.Dispose();
        }

        private void TryLoadStreamViewsAndTemplates()
        {
            var streamsInfo = StreamsInfo.TryLoad(_filePath);
            if (streamsInfo is StreamsInfo)
            {
                foreach (var pipeline in streamsInfo.ActivePipelines)
                    AddPipeline(pipeline);
                _pipelineTemplates = streamsInfo.PipelineTemplates;
            }
            else
                _pipelineTemplates = new();
        }
    }
}
