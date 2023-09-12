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
    internal class FrameProcessorPipelineElement : PipelineElement
    {
        private IFrameProcessor _frameProcessor;
        private ProcessPerformancesModel _processPerformances;
        private Dispatcher _uiDispatcher;

        public FrameProcessorPipelineElement(Dispatcher uiDispatcher, string name, IFrameProcessor frameProcessor, ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer) 
            : base(uiDispatcher, name, inputBuffer, outputBuffer)
        {
            _frameProcessor = frameProcessor;
            _processPerformances = new(name);
            _uiDispatcher = uiDispatcher;
        }

        public FrameProcessorPipelineElement(FrameProcessorPipelineElement element, ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer) 
            : base(element, inputBuffer, outputBuffer)
        {
            _frameProcessor = element._frameProcessor.Clone();
            _processPerformances = new(element.Name);
            _uiDispatcher = element._uiDispatcher;
        }

        public override PipelineElement Clone(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer)
        {
            return new FrameProcessorPipelineElement(this, inputBuffer, outputBuffer);
        }

        public async override void Process(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer, 
            CancellationTokenSource globalCancellationToken, CancellationTokenSource specificCancellationToken)
        {
            byte[] destBuffer = new byte[inputBuffer.Stride * inputBuffer.Height];
            Stopwatch waitingReadTimeWatch = new();
            Stopwatch waitingWriteTimeWatch = new();
            Stopwatch processTimeWatch = new();
            int _lastSecondProcessedFrames = 0;
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
                _lastSecondProcessedFrames++;

                if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                {
                    await _uiDispatcher.BeginInvoke(new Action(() =>
                    {
                        _processPerformances.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                        _processPerformances.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                        _processPerformances.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                        _processPerformances.Fps = _lastSecondProcessedFrames;
                    }));
                    _lastSecondProcessedFrames = 0;
                    waitingReadTimeWatch.Reset();
                    processTimeWatch.Reset();
                    waitingWriteTimeWatch.Reset();
                }
            }
        }
    }
}
