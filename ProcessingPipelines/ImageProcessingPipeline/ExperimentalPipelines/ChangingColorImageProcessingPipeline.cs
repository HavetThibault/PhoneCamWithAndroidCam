using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;

namespace ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines
{
    public class ChangingColorImageProcessingPipeline
    {
        static public ImageProcessingPipeline CreateChangingColorImageProcessingPipeline(MultipleBuffering inputBuffer)
        {
            ImageProcessingPipeline imageProcessingPipeline = new(inputBuffer);
            imageProcessingPipeline.AddPipelineElement(new PipelineElement("ChangingColor", Process, new MultipleBuffering(320, 240, 320 * 4, 10, EBufferPixelsFormat.Bgra32Bits)));
            return imageProcessingPipeline;
        }

        static void Process(MultipleBuffering inputBuffer, MultipleBuffering outputBuffer, CancellationTokenSource cancellationTokenSource)
        {
            byte[] destBuffer = new byte[320 * 240 * 4];
            byte[] tempGrayBuffer = new byte[320 * 240];
            ColorBuffer colorBuffer = new();
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                BitmapFrame frame = inputBuffer.WaitNextReaderBuffer();
                lock (frame)
                {
                    SIMDHelper.BgraToGrayAndChangeColorAndToBgra(frame.Data, 320, 240, 320 * 4, colorBuffer.ColorsBuffer, destBuffer, 320 * 4, tempGrayBuffer);
                    colorBuffer.NextColorBuffer();
                }
                inputBuffer.FinishReading();

                outputBuffer.WaitWriteBuffer(destBuffer, frame.Bitmap);
            }
        }
    }
}
