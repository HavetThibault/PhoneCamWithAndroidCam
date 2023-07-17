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
            _imageProcessingPipeline.Add(new PipelineElement("MedianFiltering", Process, (MultipleBuffering)inputBuffer.Clone()));
        }

        void Process(MultipleBuffering inputBuffer, MultipleBuffering outputBuffer, CancellationTokenSource cancellationTokenSource, ProcessPerformances processPerf)
        {
            byte[] destBuffer = new byte[inputBuffer.Stride * inputBuffer.Height];
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                BitmapFrame frame = inputBuffer.WaitNextReaderBuffer();
               
                SIMDHelper.MedianFilter(frame.Data, inputBuffer.Width, inputBuffer.Height, inputBuffer.Stride, 4, destBuffer);

                Monitor.Exit(frame);

                inputBuffer.FinishReading();

                outputBuffer.WaitWriteBuffer(destBuffer, frame.Bitmap);
            }
        }

        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            _imageProcessingPipeline.Start(cancellationTokenSource);
        }
    }
}
