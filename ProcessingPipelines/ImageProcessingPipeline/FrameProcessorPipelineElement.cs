using ImageProcessingUtils.FrameProcessor;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public class FrameProcessorPipelineElement : PipelineElement
    {
        public FrameProcessor FrameProcessor { get; private set; }

        public FrameProcessorPipelineElement(Dispatcher uiDispatcher, string name, FrameProcessor frameProcessor, ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer) 
            : base(uiDispatcher, name, frameProcessor.ElementTypeName, inputBuffer, outputBuffer)
        {
            FrameProcessor = frameProcessor;
        }

        public FrameProcessorPipelineElement(FrameProcessorPipelineElement element, ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer) 
            : base(element, inputBuffer, outputBuffer)
        {
            FrameProcessor = element.FrameProcessor.Clone();
        }

        public override PipelineElement Clone(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer)
        {
            return new FrameProcessorPipelineElement(this, inputBuffer, outputBuffer);
        }

        public override void Process(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer, 
            CancellationTokenSource globalCancellationToken, CancellationTokenSource specificCancellationToken)
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

                FrameProcessor.ProcessFrame(frame.Data, destBuffer);

                Monitor.Exit(frame);

                processTimeWatch.Stop();
                inputBuffer.FinishReading();

                waitingWriteTimeWatch.Start();
                outputBuffer.WaitWriteBuffer(destBuffer, frame.Bitmap);
                waitingWriteTimeWatch.Stop();

                if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                {
                    try
                    {
                        _uiDispatcher.Invoke(new Action(() =>
                        {
                            ProcessPerformances.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                            ProcessPerformances.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                            ProcessPerformances.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                        }));
                    }
                    catch { break; }
                    waitingReadTimeWatch.Reset();
                    processTimeWatch.Reset();
                    waitingWriteTimeWatch.Reset();
                }
            }
        }
    }
}
