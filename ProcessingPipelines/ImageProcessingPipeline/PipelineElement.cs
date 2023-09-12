using ProcessingPipelines.PipelineUtils;


namespace ProcessingPipelines.ImageProcessingPipeline;

public abstract class PipelineElement
{
    public ProducerConsumerBuffers InputBuffers { get; set; }
    public ProducerConsumerBuffers OutputBuffers { get; set; }
    public ProcessPerformances ProcessPerformances { get; set; }
    public string Name { get; set; }

    public PipelineElement(string name, ProducerConsumerBuffers inputMultipleBuffering, ProducerConsumerBuffers outputMultipleBuffering)
    {
        OutputBuffers = outputMultipleBuffering;
        InputBuffers = inputMultipleBuffering;
        Name = name;
        ProcessPerformances = new(name);
    }

    public PipelineElement(PipelineElement element, ProducerConsumerBuffers inputMultipleBuffering, ProducerConsumerBuffers outputMultipleBuffering)
    {
        InputBuffers = inputMultipleBuffering;
        OutputBuffers = outputMultipleBuffering;
        Name = element.Name;
        ProcessPerformances = element.ProcessPerformances.Clone();
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
            Process(InputBuffers, OutputBuffers, globalCancellationToken, specificCancellationToken, ProcessPerformances);
        }
    }

    public void Dispose()
    {
        OutputBuffers?.Dispose();
    }

    public abstract void Process(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer,
        CancellationTokenSource globalCancellationToken, CancellationTokenSource specificCancellationToken, ProcessPerformances processPerformances);

    public abstract PipelineElement Clone(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer);
}
