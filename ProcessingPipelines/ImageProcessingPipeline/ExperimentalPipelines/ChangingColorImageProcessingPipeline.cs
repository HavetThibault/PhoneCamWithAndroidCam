using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;
using System.Diagnostics;

namespace ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines
{
    public class ChangingColorImageProcessingPipeline
    {
        static public ImageProcessingPipeline CreateChangingColorImageProcessingPipeline(MultipleBuffering inputBuffer)
        {
            ImageProcessingPipeline imageProcessingPipeline = new(inputBuffer);
            imageProcessingPipeline.AddPipelineElement(new PipelineElement("ChangingColor", Process, (MultipleBuffering)inputBuffer.Clone()));
            return imageProcessingPipeline;
        }

        static void Process(MultipleBuffering inputBuffer, MultipleBuffering outputBuffer, CancellationTokenSource cancellationTokenSource, ProcessPerformances processPerf)
        {
            byte[] destBuffer = new byte[inputBuffer.Height * inputBuffer.Stride];
            byte[] tempGrayBuffer = new byte[inputBuffer.Width * inputBuffer.Height];
            Stopwatch waitingReadTimeWatch = new();
            Stopwatch waitingWriteTimeWatch = new();
            Stopwatch processTimeWatch = new();
            ColorBuffer colorBuffer = new();
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                waitingReadTimeWatch.Start();
                BitmapFrame frame = inputBuffer.WaitNextReaderBuffer();
                waitingReadTimeWatch.Stop();

                processTimeWatch.Start();
                lock (frame)
                {
                    SIMDHelper.BgraToGrayAndChangeColorAndToBgra(frame.Data, inputBuffer.Width, inputBuffer.Height, inputBuffer.Stride, colorBuffer.ColorsBuffer, destBuffer, inputBuffer.Stride, tempGrayBuffer);
                    colorBuffer.NextColorBuffer();
                }
                processTimeWatch.Stop();
                inputBuffer.FinishReading();

                waitingWriteTimeWatch.Start();
                outputBuffer.WaitWriteBuffer(destBuffer, frame.Bitmap);
                waitingWriteTimeWatch.Stop();

                if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                {
                    lock (processPerf)
                    {
                        processPerf.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                        processPerf.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                        processPerf.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                    }
                    waitingReadTimeWatch.Reset();
                    processTimeWatch.Reset();
                    waitingWriteTimeWatch.Reset();
                }
            }
        }
    }
}
