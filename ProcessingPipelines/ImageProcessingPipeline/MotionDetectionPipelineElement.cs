using ImageProcessingUtils.SpecificFrameProcessor;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    internal class MotionDetectionPipelineElement : PipelineElement
    {
        public MotionDetectionPipelineElement(Dispatcher uiDispatcher, string name, ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer) : 
            base(uiDispatcher, name, inputBuffer, outputBuffer)
        { 
        }

        public MotionDetectionPipelineElement(MotionDetectionPipelineElement element, ProducerConsumerBuffers inputMultipleBuffering, ProducerConsumerBuffers outputMultipleBuffering) 
            : base(element, inputMultipleBuffering, outputMultipleBuffering)
        { }

        public override PipelineElement Clone(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer)
        {
            return new MotionDetectionPipelineElement(this, inputBuffer, outputBuffer);
        }

        public override void Process(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer,
            CancellationTokenSource globalCancellationToken, CancellationTokenSource specificCancellationToken)
        {
            byte[] destBuffer = new byte[inputBuffer.Stride * inputBuffer.Height];
            byte[] lastFrameBuffer = new byte[inputBuffer.Stride * inputBuffer.Height];
            MotionDetection motionDetectionFilter = new(inputBuffer.Width, inputBuffer.Height);
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

                motionDetectionFilter.ApplyMotionDetectionFilter(frame.Data, destBuffer, lastFrameBuffer);
                Buffer.BlockCopy(frame.Data, 0, lastFrameBuffer, 0, destBuffer.Length);

                Monitor.Exit(frame);

                processTimeWatch.Stop();
                inputBuffer.FinishReading();

                waitingWriteTimeWatch.Start();
                outputBuffer.WaitWriteBuffer(destBuffer, frame.Bitmap);
                waitingWriteTimeWatch.Stop();

                if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                {
                    _uiDispatcher.Invoke(new Action(() =>
                    {
                        ProcessPerformances.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                        ProcessPerformances.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                        ProcessPerformances.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                    }));
                    waitingReadTimeWatch.Reset();
                    processTimeWatch.Reset();
                    waitingWriteTimeWatch.Reset();
                }
            }
        }
    }
}
