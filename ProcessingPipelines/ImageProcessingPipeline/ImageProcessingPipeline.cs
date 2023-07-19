using Helper.Collection;
using ImageProcessingUtils;
using ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines;
using ProcessingPipelines.PipelineUtils;

namespace ProcessingPipelines.ImageProcessingPipeline;

public class ImageProcessingPipeline
{
    private List<PipelineElement> _pipelineElements;
    private CancellationTokenSource _specificCancellationToken;
    private bool _isStreaming = false;

    public ProducerConsumerBuffers InputBuffer { get; set; }
    public ProducerConsumerBuffers OutputBuffer => _pipelineElements.Last().OutputMultipleBuffering;

    public List<ProcessPerformances> ElementsProcessPerformances { get; set; }

    public ImageProcessingPipeline(ProducerConsumerBuffers inputBuffer)
    {
        _pipelineElements = new();
        InputBuffer = inputBuffer;
        ElementsProcessPerformances = new();
        _specificCancellationToken = new();
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

    public void Start(CancellationTokenSource globalCancellationToken)
    {
        _isStreaming = true;
        ElementsProcessPerformances.Clear();
        _specificCancellationToken.TryReset();
        foreach (PipelineElement pipelineElement in _pipelineElements)
        {
            ElementsProcessPerformances.Add(pipelineElement.ProcessPerformances);
            pipelineElement.LaunchNewWorker(globalCancellationToken, _specificCancellationToken);
        }
    }

    private void Stop()
    {
        _specificCancellationToken.Cancel();
        _isStreaming = false;
    }

    public void Dispose()
    {
        if (_isStreaming)
            Stop();
        _specificCancellationToken.Dispose();
        InputBuffer?.Dispose();
        foreach (var pipelineElement in _pipelineElements)
            pipelineElement.Dispose();
    }
}
