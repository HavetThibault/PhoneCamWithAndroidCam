using ProcessingPipelines.PipelineUtils;


namespace ProcessingPipelines.ImageProcessingPipeline;

public class PipelineElement
{
    public delegate void Process(MultipleBuffering inputBuffer, MultipleBuffering outputBuffer, 
        CancellationTokenSource cancellationTokenSource, ProcessPerformances processPerformances);

    private Process _process;

    public MultipleBuffering InputMultipleBuffering { get; set; }
    public MultipleBuffering OutputMultipleBuffering { get; set; }
    public ProcessPerformances ProcessPerformances { get; set; }
    public string Name { get; set; }

    public PipelineElement(string name, Process process, MultipleBuffering outputMultipleBuffering)
        : this(name, process, null, outputMultipleBuffering) { }

    public PipelineElement(string name, Process process, MultipleBuffering inputMultipleBuffering, MultipleBuffering outputMultipleBuffering)
    {
        OutputMultipleBuffering = outputMultipleBuffering;
        InputMultipleBuffering = inputMultipleBuffering;
        _process = process;
        Name = name;
        ProcessPerformances = new(name);
    }

    public void LaunchNewWorker(CancellationTokenSource cancellationTokenSource)
    {
        new Thread(RunProcess).Start(cancellationTokenSource);
    }

    private void RunProcess(object? cancellationTokenSourceObject)
    {
        if (cancellationTokenSourceObject is CancellationTokenSource cancellationTokenSource)
        {
            _process.Invoke(InputMultipleBuffering, OutputMultipleBuffering, cancellationTokenSource, ProcessPerformances);
        }
    }
}
