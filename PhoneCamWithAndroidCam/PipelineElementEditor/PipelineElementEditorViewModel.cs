using Helper.MVVM;
using PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels;
using ProcessingPipelines.ImageProcessingPipeline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Common.Controls.Dialog;

namespace PhoneCamWithAndroidCam.PipelineElementEditor
{
    public class PipelineElementEditorViewModel : DialogWindowViewModel
    {
        public ObservableCollection<PipelineElementViewModel> PipelineElementsViewModel { get; set; }

        public PipelineElementEditorViewModel(IEnumerable<PipelineElement> pipelineElements) : base("Edit pipeline elements params")
        {
            PipelineElementsViewModel = new();
            foreach(var pipelineElement in pipelineElements)
                PipelineElementsViewModel.Add(PipelineElementViewModel.GetInstance(pipelineElement));
        }

        protected override bool CanOk(object parameter)
        {
            return true;
        }

        protected override bool CanCancel(object parameter)
        {
            return false;
        }
    }
}
