using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public class CannyImageProcessingPipeline
    {
        private ImageProcessingPipeline _imageProcessingPipeline;
        private CancellationTokenSource _cancellationTokenSource;

        public MultipleBuffering OutputBuffer => _imageProcessingPipeline.GetOutputBuffer();

        public CannyImageProcessingPipeline(MultipleBuffering inputBuffer)
        {
            _imageProcessingPipeline = new(inputBuffer);
            _imageProcessingPipeline.AddPipelineElement(new PipelineElement("MedianFiltering", Process, new MultipleBuffering(320, 240, 320 * 4, 10, EBufferPixelsFormat.Bgra32Bits)));
        }

        void Process(MultipleBuffering inputBuffer, MultipleBuffering outputBuffer)
        {
            byte[] destBuffer = new byte[320 * 240 * 4];
            CannyEdgeDetection cannyEdgeDetection = new(320, 240);
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                BitmapFrame frame = inputBuffer.WaitNextReaderBuffer();
                lock (frame)
                {
                    cannyEdgeDetection.ApplyCannyFilter(frame.Data, destBuffer);
                }
                inputBuffer.FinishReading();

                outputBuffer.WaitWriteBuffer(destBuffer, frame.Bitmap);
            }
        }

        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _imageProcessingPipeline.StartPipeline(cancellationTokenSource);
        }
    }
}
