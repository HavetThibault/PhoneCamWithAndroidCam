using Helper.Collection;
using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;

namespace ProcessingPipelines.ImageProcessingPipeline;

public class ImageProcessingPipeline
{
    
    private CancellationTokenSource _specificCancellationToken;
    private bool _isStreaming = false;
    private ProducerConsumerBuffers _inputBuffer;

    public ProducerConsumerBuffers InputBuffer
    {
        get => _inputBuffer;
        set
        {
            _inputBuffer = value;
            var firstElement = PipelineElements.First() ?? 
                throw new AggregateException("You should first assign pipeline elements to the pipeline before setting the InputBuffer.");
            firstElement.InputMultipleBuffering = value;
        }
    }

    public string Name { get; set; } = "Default name";

    public List<PipelineElement> PipelineElements { get; set; }

    public ProducerConsumerBuffers OutputBuffer => PipelineElements.Last().OutputMultipleBuffering;

    public List<ProcessPerformances> ElementsProcessPerformances { get; set; }

    public ImageProcessingPipeline(ProducerConsumerBuffers inputBuffer)
    {
        PipelineElements = new();
        _inputBuffer = inputBuffer;
        ElementsProcessPerformances = new();
        _specificCancellationToken = new();
    }

    public void Add(PipelineElement pipelineElement)
    {
        if (PipelineElements.Count > 0)
            pipelineElement.InputMultipleBuffering = PipelineElements.Last().OutputMultipleBuffering;
        else
            pipelineElement.InputMultipleBuffering = InputBuffer;
        PipelineElements.Add(pipelineElement);
    }

    public void Insert(int index, string elementName)
    {
        PipelineElement previousElement;
        PipelineElement nextElement;
        PipelineElement newElement;
        if (index == 0)
        {
            if (InputBuffer is null)
                newElement = PipelineElementBuilder.Build(elementName, null);
            else
            {
                newElement = PipelineElementBuilder.Build(
                    elementName,
                    (ProducerConsumerBuffers)InputBuffer.Clone());
                newElement.InputMultipleBuffering = InputBuffer;
            }
                
            PipelineElements.Insert(0, newElement);
        }
        else if(index == PipelineElements.Count)
        {
            previousElement = PipelineElements.Last();
            newElement = PipelineElementBuilder.Build(
                elementName, 
                (ProducerConsumerBuffers)previousElement.OutputMultipleBuffering.Clone());
            newElement.InputMultipleBuffering = previousElement.OutputMultipleBuffering;
            PipelineElements.Add(newElement);
        }
        else
        {
            previousElement = PipelineElements.ElementAt(index - 1);
            previousElement.OutputMultipleBuffering = 
                (ProducerConsumerBuffers)previousElement.OutputMultipleBuffering.Clone();
            nextElement = PipelineElements.ElementAt(index);
            newElement = PipelineElementBuilder.Build(elementName, nextElement.InputMultipleBuffering);
            newElement.InputMultipleBuffering = previousElement.OutputMultipleBuffering;
            PipelineElements.Insert(index, newElement);
        }
    }

    public void Start(CancellationTokenSource globalCancellationToken)
    {
        _isStreaming = true;
        ElementsProcessPerformances.Clear();
        _specificCancellationToken.TryReset();
        foreach (PipelineElement pipelineElement in PipelineElements)
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
        foreach (var pipelineElement in PipelineElements)
            pipelineElement.Dispose();
    }
}
