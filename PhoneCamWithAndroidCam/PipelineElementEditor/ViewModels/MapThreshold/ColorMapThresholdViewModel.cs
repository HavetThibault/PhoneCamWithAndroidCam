using ImageProcessingUtils.FrameProcessor;
using ProcessingPipelines.ImageProcessingPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels.MapThreshold
{
    internal class ColorMapThresholdViewModel : PipelineElementEditorViewModel
    {
        private ColorMapThreshold _colorMapThreshold;

        public ColorMapThresholdViewModel(IEnumerable<PipelineElement> pipelineElements, ColorMapThreshold colorMapThreshold) 
            : base(pipelineElements)
        {
            _colorMapThreshold = colorMapThreshold;
        }
    }
}
