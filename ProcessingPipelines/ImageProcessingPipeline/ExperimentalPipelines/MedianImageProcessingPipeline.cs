using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;

namespace ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines
{
    public static class MedianImageProcessingPipeline
    {
        public static ImageProcessingPipeline GetInstance(ProducerConsumerBuffers inputBuffer)
        {
            var imageProcessingPipeline = new ImageProcessingPipeline(inputBuffer);
            imageProcessingPipeline.Add(new PipelineElement("Median filter", Process, (ProducerConsumerBuffers)inputBuffer.Clone()));
            return imageProcessingPipeline;
        }

        static void Process(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer, CancellationTokenSource globalCancellationToken, CancellationTokenSource specifiCancellationToken, ProcessPerformances processPerf)
        {
            byte[] destBuffer = new byte[inputBuffer.Stride * inputBuffer.Height];
            while (!globalCancellationToken.IsCancellationRequested && !specifiCancellationToken.IsCancellationRequested)
            {
                BitmapFrame? frame = inputBuffer.WaitNextReaderBuffer();

                if (frame is null)
                    return;
               
                SIMDHelper.MedianFilter(frame.Data, inputBuffer.Width, inputBuffer.Height, inputBuffer.Stride, 4, destBuffer);

                Monitor.Exit(frame);

                inputBuffer.FinishReading();

                outputBuffer.WaitWriteBuffer(destBuffer, frame.Bitmap);
            }
        }
    }
}
