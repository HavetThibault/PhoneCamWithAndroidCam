using ProcessingPipelines.PipelineUtils;


namespace ProcessingPipelines.ImageProcessingPipeline;

public class PipelineElement
{
    public delegate void Process(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer, 
        CancellationTokenSource globalCancellationToken, CancellationTokenSource specificCancellationToken, ProcessPerformances processPerformances);

    private Process _process;

    public ProducerConsumerBuffers InputMultipleBuffering { get; set; }
    public ProducerConsumerBuffers OutputMultipleBuffering { get; set; }
    public ProcessPerformances ProcessPerformances { get; set; }
    public string Name { get; set; }

    public PipelineElement(string name, Process process, ProducerConsumerBuffers outputMultipleBuffering)
        : this(name, process, null, outputMultipleBuffering) { }

    public PipelineElement(string name, Process process, ProducerConsumerBuffers inputMultipleBuffering, ProducerConsumerBuffers outputMultipleBuffering)
    {
        OutputMultipleBuffering = outputMultipleBuffering;
        InputMultipleBuffering = inputMultipleBuffering;
        _process = process;
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
            _process.Invoke(InputMultipleBuffering, OutputMultipleBuffering, globalCancellationToken, specificCancellationToken, ProcessPerformances);
        }
    }

    public void Dispose()
    {
        InputMultipleBuffering?.Dispose();
        OutputMultipleBuffering?.Dispose();
    }
}
