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
using Wpf.Common.Controls.Dialog;
using Wpf.Common.Controls.ElementChooser;

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
            var selectionElements = new List<SelectionElement>();
            foreach(var pipelineNames in PipelineElementFactory.GetAllPipelineElementNames())
                selectionElements.Add(new (pipelineNames, null));
            var elementChooserViewModel = new ElementChooserViewModel(selectionElements);
            var elementChooserView = new ElementChooserControl(elementChooserViewModel);
            var dialogWindow = new DialogWindow(elementChooserView, elementChooserViewModel);
            dialogWindow.ShowDialog();

            var selectedPipeline = elementChooserViewModel.SelectedElement;
            if (!elementChooserViewModel.DialogResult || elementChooserViewModel.SelectedElement is null)
                return;

            _addPipelineElement(Place, selectedPipeline.Name);
        }
    }
}
