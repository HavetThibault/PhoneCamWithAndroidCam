using PhoneCamWithAndroidCam.Threads;
using System.Diagnostics;


namespace ProcessingPipelines.ImageProcessingPipeline;

public class PipelineElement
{
    private IPipelineProcess _process;

    public MultipleBuffering InputMultipleBuffering { get; set; }
    public MultipleBuffering OutputMultipleBuffering { get; set; }
    public string Name { get; set; }

    public PipelineElement(string name, IPipelineProcess process, MultipleBuffering outputMultipleBuffering)
        : this(name, process, null, outputMultipleBuffering) { }

    public PipelineElement(string name, IPipelineProcess process, MultipleBuffering inputMultipleBuffering, MultipleBuffering outputMultipleBuffering)
    {
        OutputMultipleBuffering = outputMultipleBuffering;
        InputMultipleBuffering = inputMultipleBuffering;
        _process = process;
        Name = name;
    }

    public void LaunchNewWorker(CancellationTokenSource cancellationTokenSource)
    {
        new Thread(RunProcess).Start(cancellationTokenSource);
    }

    private void RunProcess(object? cancellationTokenSourceObject)
    {
        if (cancellationTokenSourceObject is CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                _process.Process(InputMultipleBuffering, OutputMultipleBuffering);
            }
        }
    }
}
