using ImageProcessingUtils.FrameProcessor;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    internal class FrameProcessorPipelineElement : PipelineElement
    {
        private FrameProcessor _frameProcessor;

        public FrameProcessorPipelineElement(string name, FrameProcessor frameProcessor, ProducerConsumerBuffers outputMultipleBuffering) : base(name, this, outputMultipleBuffering)
        {
            _frameProcessor = frameProcessor;
        }

        public override void Process(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer, 
            CancellationTokenSource globalCancellationToken, CancellationTokenSource specificCancellationToken, 
            ProcessPerformances processPerformances)
        {
            byte[] destBuffer = new byte[inputBuffer.Stride * inputBuffer.Height];
            Stopwatch waitingReadTimeWatch = new();
            Stopwatch waitingWriteTimeWatch = new();
            Stopwatch processTimeWatch = new();
            while (!globalCancellationToken.IsCancellationRequested && !specificCancellationToken.IsCancellationRequested)
            {
                waitingReadTimeWatch.Start();
                BitmapFrame? frame = inputBuffer.WaitNextReaderBuffer();
                waitingReadTimeWatch.Stop();

                if (frame is null)
                    return;

                processTimeWatch.Start();

                _frameProcessor.ProcessFrame(frame.Data, destBuffer);

                Monitor.Exit(frame);

                processTimeWatch.Stop();
                inputBuffer.FinishReading();

                waitingWriteTimeWatch.Start();
                outputBuffer.WaitWriteBuffer(destBuffer, frame.Bitmap);
                waitingWriteTimeWatch.Stop();

                if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                {
                    lock (processPerformances)
                    {
                        processPerformances.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                        processPerformances.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                        processPerformances.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                    }
                    waitingReadTimeWatch.Reset();
                    processTimeWatch.Reset();
                    waitingWriteTimeWatch.Reset();
                }
            }
        }
    }
}
