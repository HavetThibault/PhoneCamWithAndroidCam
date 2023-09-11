using ProcessingPipelines.PipelineUtils;


namespace ProcessingPipelines.ImageProcessingPipeline;

public abstract class PipelineElement
{
    public ProducerConsumerBuffers InputMultipleBuffering { get; set; }
    public ProducerConsumerBuffers OutputMultipleBuffering { get; set; }
    public ProcessPerformances ProcessPerformances { get; set; }
    public string Name { get; set; }

    public PipelineElement(string name, ProducerConsumerBuffers outputMultipleBuffering)
        : this(name, null, outputMultipleBuffering) { }

    public PipelineElement(string name, ProducerConsumerBuffers inputMultipleBuffering, ProducerConsumerBuffers outputMultipleBuffering)
    {
        OutputMultipleBuffering = outputMultipleBuffering;
        InputMultipleBuffering = inputMultipleBuffering;
        Name = name;
        ProcessPerformances = new(name);
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
            Process(InputMultipleBuffering, OutputMultipleBuffering, globalCancellationToken, specificCancellationToken, ProcessPerformances);
        }
    }

    public void Dispose()
    {
        OutputMultipleBuffering?.Dispose();
    }

    public abstract void Process(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer,
        CancellationTokenSource globalCancellationToken, CancellationTokenSource specificCancellationToken, ProcessPerformances processPerformances);

}
