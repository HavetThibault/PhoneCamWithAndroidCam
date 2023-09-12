using ProcessingPipelines.PipelineUtils;
using System.Windows.Threading;

namespace ProcessingPipelines.ImageProcessingPipeline;

public abstract class PipelineElement
{
    protected Dispatcher _uiDispatcher;

    public ProducerConsumerBuffers InputBuffers { get; set; }
    public ProducerConsumerBuffers OutputBuffers { get; set; }
    public ProcessPerformancesModel ProcessPerformances { get; set; }
    public string Name { get; set; }

    public PipelineElement(Dispatcher uiDispatcher, string name, ProducerConsumerBuffers inputMultipleBuffering, ProducerConsumerBuffers outputMultipleBuffering)
    {
        OutputBuffers = outputMultipleBuffering;
        InputBuffers = inputMultipleBuffering;
        Name = name;
        ProcessPerformances = new(name);
        _uiDispatcher = uiDispatcher;
    }

    public PipelineElement(PipelineElement element, ProducerConsumerBuffers inputMultipleBuffering, ProducerConsumerBuffers outputMultipleBuffering)
    {
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

    public abstract void Process(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer,
        CancellationTokenSource globalCancellationToken, CancellationTokenSource specificCancellationToken);

    public abstract PipelineElement Clone(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer);
}
