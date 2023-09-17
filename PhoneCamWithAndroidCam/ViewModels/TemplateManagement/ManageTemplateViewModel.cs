using Helper.MVVM;
using PhoneCamWithAndroidCam.Serialization;
using PhoneCamWithAndroidCam.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Common.Controls;
using Wpf.Common.Controls.Dialog;
using Wpf.Common.Controls.ElementChooser;

namespace PhoneCamWithAndroidCam.ViewModels.TemplateManagement
{
    public class ManageTemplateViewModel : DialogWindowViewModel
    {
        private List<PipelineStructure> _activePipelines;

        public ObservableCollection<PipelineTemplateViewModel> PipelineTemplatesViewModels { get; set; }
        public RelayCommand AddTemplateCommand { get; set; }

        public ManageTemplateViewModel(List<PipelineStructure> pipelineTemplates, List<PipelineStructure> activePipelines) : base("Manage template")
        {
            _activePipelines = activePipelines;
            PipelineTemplatesViewModels = new();
            foreach(var template in pipelineTemplates)
            {
                PipelineTemplatesViewModels.Add(new(template.PipelineName, template, RemoveTemplate));
            }
            AddTemplateCommand = new(AddTemplate);
        }

        private void AddTemplate(object uselessParam)
        {
            var activePipelineNames = new List<SelectionElement>();
            foreach (var pipeline in _activePipelines)
                activePipelineNames.Add(new (pipeline.PipelineName, pipeline));
            var pipelineChooserViewModel = new ElementChooserViewModel(activePipelineNames);
            var pipelineUserControl = new ElementChooserControl(pipelineChooserViewModel);
            new DialogWindow(pipelineUserControl, pipelineChooserViewModel, 430, 250).ShowDialog();

            if(pipelineChooserViewModel.DialogResult)
            {
                var selectedPipeline = pipelineChooserViewModel.SelectedElement;
                if (DoesPipelineTemplateExists(selectedPipeline.Name))
                    MessageBox.Show("A pipeline template with this name already exists.");
                else
                {
                    var newPipelineTemplate = (PipelineStructure)selectedPipeline.Value;
                    PipelineTemplatesViewModels.Add(new(newPipelineTemplate.PipelineName, newPipelineTemplate, RemoveTemplate));
                }
            }
        }

        private bool DoesPipelineTemplateExists(string name)
        {
            foreach(var templateViewModel in PipelineTemplatesViewModels)
                if (templateViewModel.PipelineName == name)
                    return true;
            return false;
        }

        private void RemoveTemplate(PipelineTemplateViewModel templateViewModel)
        {
            PipelineTemplatesViewModels.Remove(templateViewModel);
        }

        public List<PipelineStructure> GetPipelineTemplates()
        {
            var pipelineTemplates = new List<PipelineStructure>();
            foreach(var viewModel in PipelineTemplatesViewModels)
                pipelineTemplates.Add(viewModel.Pipeline);
            return pipelineTemplates;
        }

        protected override bool CanOk(object parameter)
        {
            return true;
        }

        protected override bool CanCancel(object parameter)
        {
            return true;
        }
    }
}
