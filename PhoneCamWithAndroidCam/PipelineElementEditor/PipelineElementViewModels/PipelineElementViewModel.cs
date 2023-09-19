using Helper.MVVM;
using ImageProcessingUtils.FrameProcessor;
using ProcessingPipelines.ImageProcessingPipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.PipelineElementEditor.PipelineElementViewModels
{
    public class PipelineElementViewModel : BindableClass
    {
        public static PipelineElementViewModel GetInstance(FrameProcessorPipelineElement pipelineElement)
        {
            if (pipelineElement.FrameProcessor is CannyEdgeDetection cannyEdgeDetection)
            {
                return new CannyEdgeDetectionViewModel(pipelineElement.Name, cannyEdgeDetection);
            }
            return new PipelineElementViewModel(pipelineElement.Name);
        }

        public string ElementName { get; init; }

        public PipelineElementViewModel(string elementName) 
        { 
            ElementName = elementName;
        }
    }
}
