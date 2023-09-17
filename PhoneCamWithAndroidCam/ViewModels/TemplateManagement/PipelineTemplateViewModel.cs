using Helper.MVVM;
using PhoneCamWithAndroidCam.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.ViewModels.TemplateManagement
{
    public class PipelineTemplateViewModel : BindableClass
    {
        private Action<PipelineTemplateViewModel> _deleteAction;

        public string PipelineName { get; set; }
        public PipelineStructure Pipeline { get; set; }

        public RelayCommand DeleteThisPipelineCommand { get; set; }

        public PipelineTemplateViewModel(string pipelineName, PipelineStructure pipeline, Action<PipelineTemplateViewModel> deleteAction) 
        {
            _deleteAction = deleteAction;
            PipelineName = pipelineName;
            Pipeline = pipeline;
            DeleteThisPipelineCommand = new(DeleteThisPipeline);
        }

        private void DeleteThisPipeline(object uselessParam)
        {
            _deleteAction.Invoke(this);
        }
    }
}
