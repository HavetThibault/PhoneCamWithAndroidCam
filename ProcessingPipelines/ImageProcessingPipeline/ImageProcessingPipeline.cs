using Helper.Collection;
using ImageProcessingUtils;
using ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines;
using ProcessingPipelines.PipelineUtils;

namespace ProcessingPipelines.ImageProcessingPipeline;

public class ImageProcessingPipeline
{
    public static Dictionary<string, string> GetAllPipelineNames()
    {
        var dictionary = new Dictionary<string, string>
        {
            { nameof(CannyPipeline), "Canny edge detection" },
            { nameof(ChangingColorPipeline), "Gray color map change" },
            { nameof(MedianPipeline), "Median convolution" },
            { nameof(MotionDetectionPipeline), "Motion detection" },
            { nameof(ColorMapIncrementorPipeline), "Color map change" },
            { nameof(ColorMapSaturator), "Color map saturator" },
            { nameof(ColorMapThreshold), "Color map threshold" },
        };
        return dictionary;
    }

    public static IEnumerable<string> GetSortedPipelineNames()
    {
        return GetAllPipelineNames().Values.OrderBy(x => x);
    }

    public static ImageProcessingPipeline GetPipelineFromName(string name, ProducerConsumerBuffers inputBuffer)
    {
        var className = GetAllPipelineNames().GetKeyMatchingValue(name);
        switch (className)
        {
            case nameof(CannyPipeline):
                return CannyPipeline.GetInstance(inputBuffer);
            case nameof(ChangingColorPipeline):
                return ChangingColorPipeline.GetInstance(inputBuffer);
            case nameof(MedianPipeline):
                return MedianPipeline.GetInstance(inputBuffer);
            case nameof(MotionDetectionPipeline):
                return MotionDetectionPipeline.GetInstance(inputBuffer);
            case nameof(ColorMapIncrementorPipeline):
                return ColorMapIncrementorPipeline.GetInstance(inputBuffer);
            case nameof(ColorMapSaturator):
                return ColorMapSaturatorPipeline.GetInstance(inputBuffer);
            case nameof(ColorMapThreshold):
                return ColorMapThresholdPipeline.GetInstance(inputBuffer);
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
