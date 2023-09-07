using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.ViewModels.PipelineEditor
{
    public class PipelineElementViewModel
    {
        public string Name { get; }

        public PipelineElementViewModel(string name)
        {
            Name = name;
        }
    }
}
