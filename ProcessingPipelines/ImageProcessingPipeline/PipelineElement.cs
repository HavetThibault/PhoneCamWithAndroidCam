using ImageProcessingUtils.FrameProcessor;
using ProcessingPipelines.PipelineUtils;
using System.Diagnostics;
using System.Windows.Threading;

namespace ProcessingPipelines.ImageProcessingPipeline;

public class PipelineElement
{
    protected Dispatcher _uiDispatcher;

    public ProducerConsumerBuffers InputBuffers { get; set; }
    public ProducerConsumerBuffers OutputBuffers { get; set; }
    public ProcessPerformancesModel ProcessPerformances { get; set; }
    public FrameProcessor FrameProcessor { get; private set; }
    public string Name { get; set; }
    public string ElementTypeName { get; }

    public PipelineElement(Dispatcher uiDispatcher, string name, FrameProcessor frameProcessor, ProducerConsumerBuffers inputMultipleBuffering, ProducerConsumerBuffers outputMultipleBuffering)
    {
        FrameProcessor = frameProcessor;
        ElementTypeName = frameProcessor.ElementTypeName;
        OutputBuffers = outputMultipleBuffering;
        InputBuffers = inputMultipleBuffering;
        Name = name;
        ProcessPerformances = new(name);
        _uiDispatcher = uiDispatcher;
    }

    public PipelineElement(PipelineElement element, ProducerConsumerBuffers inputMultipleBuffering, ProducerConsumerBuffers outputMultipleBuffering)
    {
        FrameProcessor = element.FrameProcessor.Clone();
        ElementTypeName = element.ElementTypeName;
        InputBuffers = inputMultipleBuffering;
        OutputBuffers = outputMultipleBuffering;
        Name = element.Name;
        ProcessPerformances = new(element.ProcessPerformances.ProcessName);
        _uiDispatcher = element._uiDispatcher;
    }

    public void LaunchNewWorker(CancellationTokenSource globalCancellationToken, CancellationTokenSource specificCancellationToken)
    {
        var processThread = new Thread(RunProcess)
        {
            Name = Name
        };
        processThread.Start((globalCancellationToken, specificCancellationToken));
    }

    private void RunProcess(object? cancellationTokens)
    {
        if (cancellationTokens is (CancellationTokenSource globalCancellationToken, CancellationTokenSource specificCancellationToken))
        {
            Process(InputBuffers, OutputBuffers, globalCancellationToken, specificCancellationToken);
        }
    }

    public void Dispose()
    {
        OutputBuffers?.Dispose();
    }

    public void Process(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer,
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

    public PipelineElement Clone(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer)
    {
        return new PipelineElement(this, inputBuffer, outputBuffer);
    }
}
