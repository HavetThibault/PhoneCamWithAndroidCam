using ImageProcessingUtils;
using ImageProcessingUtils.FrameProcessor;
using ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public static class PipelineInstantiator
    {
        public const string CANNY_EDGE_DETECTION = "Canny edge detection";
        public const string GRAY_COLOR_MAP_CHANGE = "Gray color map change";
        public const string MEDIAN_FILTER = "Median filter";
        public const string MOTION_DETECTION = "Motion detection";
        public const string COLOR_MAP_CHANGE = "Color map change";
        public const string COLOR_MAP_SATURATOR = "Color map saturator";
        public const string COLOR_MAP_THRESHOLD = "Color map threshold";

        public static IEnumerable<string> GetAllPipelineNames()
        {
            var pipelineNames = new List<string>
            {
                CANNY_EDGE_DETECTION,
                GRAY_COLOR_MAP_CHANGE,
                MEDIAN_FILTER,
                MOTION_DETECTION,
                COLOR_MAP_CHANGE,
                COLOR_MAP_SATURATOR,
                COLOR_MAP_THRESHOLD,
            };
            return pipelineNames.OrderBy(x => x);
        }

        public static ImageProcessingPipeline GetInstance(string pipelineName, ProducerConsumerBuffers inputBuffer)
        {
            switch(pipelineName)
            {
                case CANNY_EDGE_DETECTION:
                    return GetCannyEdgeDetectionInstance(inputBuffer);
                case GRAY_COLOR_MAP_CHANGE:
                    return GetGrayColorMapIncrementorPipeline(inputBuffer);
                case MEDIAN_FILTER:
                    return GetMedianFilterPipeline(inputBuffer);
                case MOTION_DETECTION:
                    return GetMotionDetectionPipeline(inputBuffer);
                default:
                    throw new ArgumentException("No pipeline with this name.");
            }
        }

        private static ImageProcessingPipeline GetCannyEdgeDetectionInstance(ProducerConsumerBuffers inputBuffer)
        {
            var pipeline = new ImageProcessingPipeline(inputBuffer);
            var cannyFrameProcessor = new CannyEdgeDetection(inputBuffer.Width, inputBuffer.Height);
            pipeline.Add(new FrameProcessorPipelineElement(CANNY_EDGE_DETECTION, cannyFrameProcessor, (ProducerConsumerBuffers)inputBuffer.Clone()));
            return pipeline;
        }

        private static ImageProcessingPipeline GetGrayColorMapIncrementorPipeline(ProducerConsumerBuffers inputBuffer)
        {
            var pipeline = new ImageProcessingPipeline(inputBuffer);
            var grayColorMapFrameProcessor = new GrayColorMapIncrementor(inputBuffer.Width, inputBuffer.Height, inputBuffer.Stride);
            pipeline.Add(new FrameProcessorPipelineElement(GRAY_COLOR_MAP_CHANGE, grayColorMapFrameProcessor, (ProducerConsumerBuffers)inputBuffer.Clone()));
            return pipeline;
        }

        private static ImageProcessingPipeline GetMedianFilterPipeline(ProducerConsumerBuffers inputBuffer)
        {
            var pipeline = new ImageProcessingPipeline(inputBuffer);
            var grayColorMapFrameProcessor = new GrayColorMapIncrementor(inputBuffer.Width, inputBuffer.Height, inputBuffer.Stride);
            pipeline.Add(new FrameProcessorPipelineElement(MEDIAN_FILTER, grayColorMapFrameProcessor, (ProducerConsumerBuffers)inputBuffer.Clone()));
            return pipeline;
        }

        private static ImageProcessingPipeline GetMotionDetectionPipeline(ProducerConsumerBuffers inputBuffer)
        {
            var pipeline = new ImageProcessingPipeline(inputBuffer);
            pipeline.Add(new MotionDetectionPipelineElement(MOTION_DETECTION, (ProducerConsumerBuffers)inputBuffer.Clone()));
            return pipeline;
        }
    }
}
