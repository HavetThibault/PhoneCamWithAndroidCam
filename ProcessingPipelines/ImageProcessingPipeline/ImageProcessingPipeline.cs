using PhoneCamWithAndroidCam.Threads;


namespace ImageProcessingUtils.Pipeline;

public class ImageProcessingPipeline
{
    private List<PipelineElement> _pipelineElements { get; set; }
    private CancellationTokenSource _cancellationTokenSource;

    public ImageProcessingPipeline()
    {
        _pipelineElements = new();
    }

    public MultipleBuffering GetInputBuffer() => _pipelineElements.First().InputMultipleBuffering;

    public MultipleBuffering GetOutputBuffer() => _pipelineElements.Last().OutputMultipleBuffering;

    public void AddPipelineElement(PipelineElement pipelineElement)
    {
        pipelineElement.InputMultipleBuffering = _pipelineElements.Last().OutputMultipleBuffering;
        _pipelineElements.Add(pipelineElement);
    }

    public void StartPipeline()
    {
        _cancellationTokenSource = new();
        foreach (PipelineElement pipelineElement in _pipelineElements)
        {
            pipelineElement.LaunchNewWorker(_cancellationTokenSource);
        }
    }

    public List<(string, long)> GetPipelineElementStats()
    {
        var pipelineStats = new List<(string, long)>();
        foreach (PipelineElement pipelineElement in _pipelineElements)
            pipelineStats.Add(new(pipelineElement.Name, pipelineElement.LastProcessMsTime));

        return pipelineStats;
    }

    public void StopPipeline()
    {
        _cancellationTokenSource.Cancel();
    }
}
