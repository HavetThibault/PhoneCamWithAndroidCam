using ImageProcessingUtils;
using ImageProcessingUtils.FrameProcessor;
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
        public const string COPY = "Copy";
        public const string CANNY_EDGE_DETECTION = "Canny edge detection";
        public const string GRAY_COLOR_MAP_INCREMENTOR = "Gray color map incrementor";
        public const string MEDIAN_FILTER = "Median filter";
        public const string MOTION_DETECTION = "Motion detection";
        public const string COLOR_MAP_INCREMENTOR = "Color map incrementor";
        public const string COLOR_MAP_SATURATOR = "Color map saturator";
        public const string COLOR_MAP_THRESHOLD = "Color map threshold";

        public static IEnumerable<string> GetAllPipelineNames()
        {
            var pipelineNames = new List<string>
            {
                COPY,
                CANNY_EDGE_DETECTION,
                GRAY_COLOR_MAP_INCREMENTOR,
                MEDIAN_FILTER,
                MOTION_DETECTION,
                COLOR_MAP_INCREMENTOR,
                COLOR_MAP_SATURATOR,
                COLOR_MAP_THRESHOLD,
            };
            return pipelineNames.OrderBy(x => x);
        }

        public static ImageProcessingPipeline GetInstance(string pipelineName, ProducerConsumerBuffers inputBuffer)
        {
            switch(pipelineName)
            {
                case COPY:
                    return GetCopyPipeline(inputBuffer);
                case CANNY_EDGE_DETECTION:
                    return GetCannyEdgeDetectionInstance(inputBuffer);
                case GRAY_COLOR_MAP_INCREMENTOR:
                    return GetGrayColorMapIncrementorPipeline(inputBuffer);
                case MEDIAN_FILTER:
                    return GetMedianFilterPipeline(inputBuffer);
                case MOTION_DETECTION:
                    return GetMotionDetectionPipeline(inputBuffer);
                case COLOR_MAP_INCREMENTOR:
                    return GetColorMapIncrementorPipeline(inputBuffer);
                case COLOR_MAP_SATURATOR:
                    return GetColorMapSaturator(inputBuffer);
                case COLOR_MAP_THRESHOLD:
                    return GetColorMapThreshold(inputBuffer);
                default:
                    throw new ArgumentException("No pipeline with this name.");
            }
        }

        private static ImageProcessingPipeline GetCopyPipeline(ProducerConsumerBuffers inputBuffer)
        {
            var pipeline = new ImageProcessingPipeline(inputBuffer);
            var copyFrameProcessor = new CopyFrameProcessor();
            pipeline.Add(new FrameProcessorPipelineElement(COPY, copyFrameProcessor, (ProducerConsumerBuffers)inputBuffer.Clone()));
            return pipeline;
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
            pipeline.Add(new FrameProcessorPipelineElement(GRAY_COLOR_MAP_INCREMENTOR, grayColorMapFrameProcessor, (ProducerConsumerBuffers)inputBuffer.Clone()));
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

        private static ImageProcessingPipeline GetColorMapIncrementorPipeline(ProducerConsumerBuffers inputBuffer)
        {
            var pipeline = new ImageProcessingPipeline(inputBuffer);
            var colorMapFrameProcessor = new ColorMapIncrementor(inputBuffer.Width, inputBuffer.Height, inputBuffer.Stride);
            pipeline.Add(new FrameProcessorPipelineElement(COLOR_MAP_INCREMENTOR, colorMapFrameProcessor, (ProducerConsumerBuffers)inputBuffer.Clone()));
            return pipeline;
        }

        private static ImageProcessingPipeline GetColorMapSaturator(ProducerConsumerBuffers inputBuffer)
        {
            var pipeline = new ImageProcessingPipeline(inputBuffer);
            var colorMapFrameProcessor = new ColorMapSaturator(inputBuffer.Width, inputBuffer.Height, inputBuffer.Stride);
            pipeline.Add(new FrameProcessorPipelineElement(COLOR_MAP_SATURATOR, colorMapFrameProcessor, (ProducerConsumerBuffers)inputBuffer.Clone()));
            return pipeline;
        }

        private static ImageProcessingPipeline GetColorMapThreshold(ProducerConsumerBuffers inputBuffer)
        {
            var pipeline = new ImageProcessingPipeline(inputBuffer);
            var colorMapFrameProcessor = new ColorMapThreshold(inputBuffer.Width, inputBuffer.Height, inputBuffer.Stride, 5);
            pipeline.Add(new FrameProcessorPipelineElement(COLOR_MAP_THRESHOLD, colorMapFrameProcessor, (ProducerConsumerBuffers)inputBuffer.Clone()));
            return pipeline;
        }
    }
}
