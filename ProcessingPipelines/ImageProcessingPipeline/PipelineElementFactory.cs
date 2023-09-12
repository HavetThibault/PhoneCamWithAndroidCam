using ImageProcessingUtils.FrameProcessor;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public static class PipelineElementFactory
    {
        public const string COPY = "Copy";
        public const string CANNY_EDGE_DETECTION = "Canny edge detection";
        public const string GRAY_COLOR_MAP_INCREMENTOR = "Gray color map incrementor";
        public const string MEDIAN_FILTER = "Median filter";
        public const string MOTION_DETECTION = "Motion detection";
        public const string COLOR_MAP_INCREMENTOR = "Color map incrementor";
        public const string COLOR_MAP_SATURATOR = "Color map saturator";
        public const string COLOR_MAP_THRESHOLD = "Color map threshold";

        public static IEnumerable<string> GetAllPipelineElementNames()
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

        public static PipelineElement GetInstance(string elementType, ProducerConsumerBuffers outputBuffer, string name, Dispatcher uiDispatcher)
        {
            switch (elementType)
            {
                case COPY:
                    var copyFrameProcessor = new CopyFrameProcessor();
                    return new FrameProcessorPipelineElement(
                        uiDispatcher,
                        name, 
                        copyFrameProcessor,
                        null,
                        outputBuffer);

                case CANNY_EDGE_DETECTION:
                    var cannyFrameProcessor = new CannyEdgeDetection(outputBuffer.Width, outputBuffer.Height);
                    return new FrameProcessorPipelineElement(
                        uiDispatcher,
                        name, 
                        cannyFrameProcessor,
                        null,
                        outputBuffer);

                case GRAY_COLOR_MAP_INCREMENTOR:
                    GrayColorMapIncrementor grayColorMapFrameProcessor = new (outputBuffer.Width, outputBuffer.Height, outputBuffer.Stride);
                    return new FrameProcessorPipelineElement(
                        uiDispatcher,
                        name,
                        grayColorMapFrameProcessor,
                        null, 
                        outputBuffer);

                case MEDIAN_FILTER:
                    grayColorMapFrameProcessor = new (outputBuffer.Width, outputBuffer.Height, outputBuffer.Stride);
                    return new FrameProcessorPipelineElement(
                        uiDispatcher,
                        name, 
                        grayColorMapFrameProcessor,
                        null, 
                        outputBuffer);

                case MOTION_DETECTION:
                    return new MotionDetectionPipelineElement(
                        uiDispatcher,
                        name,
                        null,
                        outputBuffer);

                case COLOR_MAP_INCREMENTOR:
                    var colorMapIncrementor = new ColorMapIncrementor(outputBuffer.Width, outputBuffer.Height, outputBuffer.Stride);
                    return new FrameProcessorPipelineElement(
                        uiDispatcher,
                        name,
                        colorMapIncrementor,
                        null, 
                        outputBuffer);

                case COLOR_MAP_SATURATOR:
                    var colorMapSaturator = new ColorMapSaturator(outputBuffer.Width, outputBuffer.Height, outputBuffer.Stride);
                    return new FrameProcessorPipelineElement(
                        uiDispatcher,
                        name, 
                        colorMapSaturator,
                        null, 
                        outputBuffer);

                case COLOR_MAP_THRESHOLD:
                    var colorMapFrameProcessor = new ColorMapThreshold(outputBuffer.Width, outputBuffer.Height, outputBuffer.Stride, 5);
                    return new FrameProcessorPipelineElement(
                        uiDispatcher,
                        name, 
                        colorMapFrameProcessor,
                        null, 
                        outputBuffer);

                default:
                    throw new ArgumentException("No pipeline element with this name.");
            }
        }
    }
}
