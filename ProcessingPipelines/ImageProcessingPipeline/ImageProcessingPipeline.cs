using PhoneCamWithAndroidCam.Threads;


namespace ProcessingPipelines.ImageProcessingPipeline;

public class ImageProcessingPipeline
{
    private List<PipelineElement> _pipelineElements;

    public ImageProcessingPipeline(MultipleBuffering inputBuffer)
    {
        _pipelineElements = new();
        InputBuffer = inputBuffer;
    }

    public MultipleBuffering InputBuffer { get; set; }

    public MultipleBuffering GetOutputBuffer() => _pipelineElements.Last().OutputMultipleBuffering;

    public void AddPipelineElement(PipelineElement pipelineElement)
    {
        if(_pipelineElements.Count > 0)
        {
            pipelineElement.InputMultipleBuffering = _pipelineElements.Last().OutputMultipleBuffering;
        }
        else
        {
            pipelineElement.InputMultipleBuffering = InputBuffer;
        }
        _pipelineElements.Add(pipelineElement);
    }

    public void StartPipeline(CancellationTokenSource cancellationTokenSource)
    {
        foreach (PipelineElement pipelineElement in _pipelineElements)
        {
            pipelineElement.LaunchNewWorker(cancellationTokenSource);
        }
    }
}
