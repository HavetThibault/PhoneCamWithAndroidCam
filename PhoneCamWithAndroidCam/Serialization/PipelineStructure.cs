﻿using ImageProcessingUtils.FrameProcessor;
using ProcessingPipelines.ImageProcessingPipeline;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace PhoneCamWithAndroidCam.Serialization
{
    public class PipelineStructure
    {
        public string PipelineName { get; set; }
        public List<FrameProcessor> PipelineElementFrameProcessor { get; set; }

        private PipelineStructure() { }

        public PipelineStructure(ImageProcessingPipeline pipeline) 
        {
            PipelineElementFrameProcessor = new();
            foreach(var element in pipeline.PipelineElements)
                PipelineElementFrameProcessor.Add(element.FrameProcessor);
            PipelineName = pipeline.Name;
        }

        public ImageProcessingPipeline InstantiatePipeline(ProducerConsumerBuffers inputBuffer, Dispatcher uiDispatcher)
        {
            var pipeline = new ImageProcessingPipeline(PipelineName, inputBuffer, uiDispatcher);
            foreach (var frameProcessor in PipelineElementFrameProcessor)
                pipeline.InstantiateAndAdd(frameProcessor);
            return pipeline;
        }
    }
}
