using ProcessingPipelines.PipelineUtils;

namespace ProcessingPipelines.ImageProcessingPipeline;

public class ImageProcessingPipeline
{
    private List<PipelineElement> _pipelineElements;

    public MultipleBuffering InputBuffer { get; set; }
    public MultipleBuffering OutputBuffer => _pipelineElements.Last().OutputMultipleBuffering;

    public List<ProcessPerformances> ElementsProcessPerformances { get; set; }

    public ImageProcessingPipeline(MultipleBuffering inputBuffer)
    {
        _pipelineElements = new();
        InputBuffer = inputBuffer;
        ElementsProcessPerformances = new();
    }

    public void Add(PipelineElement pipelineElement)
    {
        if (_pipelineElements.Count > 0)
        {
            pipelineElement.InputMultipleBuffering = _pipelineElements.Last().OutputMultipleBuffering;
        }
        else
        {
            pipelineElement.InputMultipleBuffering = InputBuffer;
        }
        _pipelineElements.Add(pipelineElement);
    }

    public void Start(CancellationTokenSource cancellationTokenSource)
    {
        ElementsProcessPerformances.Clear();
        foreach (PipelineElement pipelineElement in _pipelineElements)
        {
            ElementsProcessPerformances.Add(pipelineElement.ProcessPerformances);
            pipelineElement.LaunchNewWorker(cancellationTokenSource);
        }
    }
}
