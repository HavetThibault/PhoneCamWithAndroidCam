using Helper.MVVM;
using PhoneCamWithAndroidCam.Views;
using ProcessingPipelines.ImageProcessingPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Common.Controls;

namespace PhoneCamWithAndroidCam.ViewModels.PipelineEditor
{
    internal class AddPipelineButtonViewModel : BindableClass
    {
        public delegate void AddPipelineElementDelegate(int place, string pipelineElementName);

        private int _place;
        private AddPipelineElementDelegate _addPipelineElement;

        public RelayCommand AddPipelineElementCommand { get; }

        public AddPipelineButtonViewModel(int place, AddPipelineElementDelegate addPipelineElementCommand)
        {
            _place = place;
            _addPipelineElement = addPipelineElementCommand;
            AddPipelineElementCommand = new RelayCommand(AddPipelineElement);
        }

        private void AddPipelineElement(object param)
        {
            var pipelineChooserViewModel = new PipelineChooserViewModel(
                PipelineInstantiator.GetAllPipelineNames());
            var pipelineChooserView = new PipelineChooserView(pipelineChooserViewModel);
            var dialogWindow = new DialogWindow(pipelineChooserView, pipelineChooserViewModel);
            dialogWindow.ShowDialog();

            string selectedPipeline = pipelineChooserViewModel.SelectedPipeline;
            if (!pipelineChooserViewModel.DialogResult || selectedPipeline is null)
                return;

            _addPipelineElement(_place, selectedPipeline);
        }
    }
}
