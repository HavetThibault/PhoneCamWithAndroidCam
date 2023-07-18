using Helper.Collection;
using ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines;
using ProcessingPipelines.PipelineUtils;

namespace ProcessingPipelines.ImageProcessingPipeline;

public class ImageProcessingPipeline
{
    public static Dictionary<string, string> GetAllPipelineNames()
    {
        var dictionary = new Dictionary<string, string>
        {
            { nameof(CannyImageProcessingPipeline), "Canny" },
            { nameof(ChangingColorImageProcessingPipeline), "Gray changing color" },
            { nameof(MedianImageProcessingPipeline), "Median" },
            { nameof(MotionDetectionProcessingPipeline), "Motion detection" },
        };
        return dictionary;
    }

    public static ImageProcessingPipeline GetPipelineFromName(string name, ProducerConsumerBuffers inputBuffer)
    {
        var className = GetAllPipelineNames().GetKeyMatchingValue(name);
        switch (className)
        {
            case nameof(CannyImageProcessingPipeline):
                return CannyImageProcessingPipeline.GetInstance(inputBuffer);
            case nameof(ChangingColorImageProcessingPipeline):
                return ChangingColorImageProcessingPipeline.GetInstance(inputBuffer);
            case nameof(MedianImageProcessingPipeline):
                return MedianImageProcessingPipeline.GetInstance(inputBuffer);
            case nameof(MotionDetectionProcessingPipeline):
                return MotionDetectionProcessingPipeline.GetInstance(inputBuffer);
            default:
                throw new ArgumentException("No pipeline with this name.");
        }
    }

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
