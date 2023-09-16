using Helper.MVVM;
using PhoneCamWithAndroidCam.Serialization;
using PhoneCamWithAndroidCam.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Common.Controls;
using Wpf.Common.Controls.Dialog;
using Wpf.Common.Controls.ElementChooser;

namespace PhoneCamWithAndroidCam.ViewModels.TemplateManagement
{
    public class ManageTemplateViewModel : DialogWindowViewModel
    {
        private List<PipelineStructure> _activePipelines;

        public List<PipelineStructure> PipelineTemplates { get; set; }
        public RelayCommand AddTemplateCommand { get; set; }

        public ManageTemplateViewModel(List<PipelineStructure> pipelineTemplates, List<PipelineStructure> activePipelines) : base("Manage template")
        {
            _activePipelines = activePipelines;
            PipelineTemplates = pipelineTemplates;
            AddTemplateCommand = new(AddTemplate);
        }

        private void AddTemplate(object uselessParam)
        {
            var activePipelineNames = new List<SelectionElement>();
            foreach (var pipeline in _activePipelines)
                activePipelineNames.Add(new (pipeline.PipelineName, pipeline));
            var pipelineChooserViewModel = new ElementChooserViewModel(activePipelineNames);
            var pipelineUserControl = new ElementChooserControl(pipelineChooserViewModel);
            new DialogWindow(pipelineUserControl, pipelineChooserViewModel).ShowDialog();

            if(pipelineChooserViewModel.DialogResult)
            {
                PipelineTemplates.Add((PipelineStructure)pipelineChooserViewModel.SelectedElement.Value);
            }
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
