using Helper.MVVM;
using PhoneCamWithAndroidCam.Views;
using ProcessingPipelines.ImageProcessingPipeline;
using PropertyTools.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Common.Controls;

namespace PhoneCamWithAndroidCam.ViewModels.PipelineEditor
{
    internal class AddPipelineButtonViewModel : BindableClass
    {
        public delegate void AddPipelineElementDelegate(int place, string pipelineElementName);
        
        private AddPipelineElementDelegate _addPipelineElement;

        public int Place => Container.IndexOf(this) / 4;
        public RelayCommand AddPipelineElementCommand { get; }
        public Collection<object> Container { get; set; }

        public AddPipelineButtonViewModel(AddPipelineElementDelegate addPipelineElementCommand, Collection<object> container)
        {
            Container = container;
            _addPipelineElement = addPipelineElementCommand;
            AddPipelineElementCommand = new RelayCommand(AddPipelineElement);
        }

        private void AddPipelineElement(object param)
        {
            var pipelineChooserViewModel = new PipelineChooserViewModel(
                PipelineElementFactory.GetAllPipelineElementNames());
            var pipelineChooserView = new PipelineChooserView(pipelineChooserViewModel);
            var dialogWindow = new DialogWindow(pipelineChooserView, pipelineChooserViewModel);
            dialogWindow.ShowDialog();

            string selectedPipeline = pipelineChooserViewModel.SelectedPipeline;
            if (!pipelineChooserViewModel.DialogResult || selectedPipeline is null)
                return;

            _addPipelineElement(Place, selectedPipeline);
        }
    }
}
