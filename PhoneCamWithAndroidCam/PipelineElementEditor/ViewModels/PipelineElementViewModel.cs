using Helper.MVVM;
using ImageProcessingUtils.FrameProcessor;
using PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels.CannyEdge;
using PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels.Incrementor;
using PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels.Lagger;
using PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels.MapThreshold;
using PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels.Scanner;
using ProcessingPipelines.ImageProcessingPipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels
{
    public class PipelineElementViewModel : BindableClass
    {
        public static PipelineElementViewModel GetInstance(PipelineElement pipelineElement)
        {
            if (pipelineElement.FrameProcessor is CannyEdgeDetection cannyEdgeDetection)
            {
                return new CannyEdgeDetectionViewModel(pipelineElement.Name, cannyEdgeDetection);
            }
            if(pipelineElement.FrameProcessor is ColorMapIncrementor incrementor)
            {
                return new ColorMapIncrementorViewModel(pipelineElement.Name, incrementor);
            }
            if(pipelineElement.FrameProcessor is ColorMapThreshold threshold)
            {
                return new ColorMapThresholdViewModel(pipelineElement.Name, threshold);
            }
            if(pipelineElement.FrameProcessor is ScannerProcessor scanner)
            {
                return new ScannerViewModel(pipelineElement.Name, scanner);
            }
            if(pipelineElement.FrameProcessor is FrameLagger lagger)
            {
                return new FrameLaggerViewModel(pipelineElement.Name, lagger);
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
