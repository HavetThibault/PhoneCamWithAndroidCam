using ImageProcessingUtils.FrameProcessor;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup.Localizer;
using System.Windows.Threading;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public static class PipelineElementFactory
    {
        public const string COPY = CopyFrameProcessor.ELEMENT_TYPE_NAME;
        public const string CANNY_EDGE_DETECTION = CannyEdgeDetection.ELEMENT_TYPE_NAME;
        public const string GRAY_COLOR_MAP_INCREMENTOR = GrayColorMapIncrementor.ELEMENT_TYPE_NAME;
        public const string MEDIAN_FILTER = MedianFilter.ELEMENT_TYPE_NAME;
        public const string MOTION_DETECTION = MotionDetection.ELEMENT_TYPE_NAME;
        public const string COLOR_MAP_INCREMENTOR = ColorMapIncrementor.ELEMENT_TYPE_NAME;
        public const string COLOR_MAP_SATURATOR = ColorMapSaturator.ELEMENT_TYPE_NAME;
        public const string COLOR_MAP_THRESHOLD = ColorMapThreshold.ELEMENT_TYPE_NAME;
        public const string COLOR_MAP_INVERTER = ColorMapInverter.ELEMENT_TYPE_NAME;
        public const string SCANNER = ScannerProcessor.ELEMENT_TYPE_NAME;

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
                COLOR_MAP_INVERTER,
                SCANNER,
            };
            return pipelineNames.OrderBy(x => x);
        }

        public static PipelineElement GetInstance(string elementType, ProducerConsumerBuffers outputBuffer, string name, Dispatcher uiDispatcher)
        {
            FrameProcessor frameProcessor;
            switch (elementType)
            {
                case COPY:
                    frameProcessor = new CopyFrameProcessor();
                    break;

                case CANNY_EDGE_DETECTION:
                    frameProcessor = new CannyEdgeDetection(outputBuffer.Width, outputBuffer.Height);
                    break;

                case GRAY_COLOR_MAP_INCREMENTOR:
                    frameProcessor = new GrayColorMapIncrementor(outputBuffer.Width, outputBuffer.Height, outputBuffer.Stride);
                    break;

                case MEDIAN_FILTER:
                    frameProcessor = new MedianFilter(outputBuffer.Width, outputBuffer.Height, outputBuffer.Stride);
                    break;

                case COLOR_MAP_INCREMENTOR:
                    frameProcessor = new ColorMapIncrementor(outputBuffer.Width, outputBuffer.Height, outputBuffer.Stride);
                    break;

                case COLOR_MAP_SATURATOR:
                    frameProcessor = new ColorMapSaturator(outputBuffer.Width, outputBuffer.Height, outputBuffer.Stride);
                    break;

                case COLOR_MAP_THRESHOLD:
                    frameProcessor = new ColorMapThreshold(outputBuffer.Width, outputBuffer.Height, outputBuffer.Stride, 5);
                    break;

                case COLOR_MAP_INVERTER:
                    frameProcessor = new ColorMapInverter(outputBuffer.Width, outputBuffer.Height, outputBuffer.Stride);
                    break;

                case SCANNER:
                    frameProcessor = new ScannerProcessor(outputBuffer.Width, outputBuffer.Height, 3, 5000);
                    break;

                case MOTION_DETECTION:
                    frameProcessor = new MotionDetection(outputBuffer.Width, outputBuffer.Height);
                    break;

                default:
                    throw new ArgumentException("No pipeline element with this name.");
            }
            return new PipelineElement(
                uiDispatcher,
                name,
                frameProcessor,
                null,
                outputBuffer);
        }
    }
}
