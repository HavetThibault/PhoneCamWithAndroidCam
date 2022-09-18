using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;

namespace ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines
{
    public class MedianImageProcessingPipeline
    {
        private ImageProcessingPipeline _imageProcessingPipeline;

        public MultipleBuffering OutputBuffer => _imageProcessingPipeline.OutputBuffer;

        public MedianImageProcessingPipeline(MultipleBuffering inputBuffer)
        {
            _imageProcessingPipeline = new(inputBuffer);
            _imageProcessingPipeline.AddPipelineElement(new PipelineElement("MedianFiltering", Process, (MultipleBuffering)inputBuffer.Clone()));
        }

        void Process(MultipleBuffering inputBuffer, MultipleBuffering outputBuffer, CancellationTokenSource cancellationTokenSource, ProcessPerformances processPerf)
        {
            byte[] destBuffer = new byte[inputBuffer.Stride * inputBuffer.Height];
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                BitmapFrame frame = inputBuffer.WaitNextReaderBuffer();
                lock (frame)
                {
                    SIMDHelper.MedianFilter(frame.Data, inputBuffer.Width, inputBuffer.Height, inputBuffer.Stride, 4, destBuffer);
                }
                inputBuffer.FinishReading();

                outputBuffer.WaitWriteBuffer(destBuffer, frame.Bitmap);
            }
        }

        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            _imageProcessingPipeline.StartPipeline(cancellationTokenSource);
        }
    }
}
