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

    public ImageProcessingPipeline(ImageProcessingPipeline pipeline)
    {

    }

    public void Add(PipelineElement pipelineElement)
    {
        if (PipelineElements.Count > 0)
            pipelineElement.InputMultipleBuffering = PipelineElements.Last().OutputMultipleBuffering;
        else
            pipelineElement.InputMultipleBuffering = InputBuffer;
        PipelineElements.Add(pipelineElement);
    }

    private string GetNextAvailableElementName(string name)
    {
        if (PipelineElements.Exists(elem => elem.Name.Equals(name)))
        {
            var newName = name + " 1";

            for (int i = 2; PipelineElements.Exists(elem => elem.Name.Equals(newName)); i++)
                newName = name + $" {i}";
            return newName;
        }
        return name;
    }

    public PipelineElement InstantiateAndInsert(int index, string elementType)
    {
        string name = GetNextAvailableElementName(elementType);
        PipelineElement previousElement;
        PipelineElement nextElement;
        PipelineElement newElement;
        if (index == 0)
        {
            if (InputBuffer is null)
                newElement = PipelineElementBuilder.Build(elementType, null, name);
            else
            {
                newElement = PipelineElementBuilder.Build(
                    elementType,
                    (ProducerConsumerBuffers)InputBuffer.Clone(),
                    name);
                newElement.InputMultipleBuffering = InputBuffer;
            }
                
            PipelineElements.Insert(0, newElement);
        }
        else if(index == PipelineElements.Count)
        {
            previousElement = PipelineElements.Last();
            newElement = PipelineElementBuilder.Build(
                elementType, 
                (ProducerConsumerBuffers)previousElement.OutputMultipleBuffering.Clone(),
                name);

            newElement.InputMultipleBuffering = previousElement.OutputMultipleBuffering;
            PipelineElements.Add(newElement);
        }
        else
        {
            previousElement = PipelineElements.ElementAt(index - 1);
            previousElement.OutputMultipleBuffering = 
                (ProducerConsumerBuffers)previousElement.OutputMultipleBuffering.Clone();
            nextElement = PipelineElements.ElementAt(index);
            newElement = PipelineElementBuilder.Build(
                elementType,
                nextElement.InputMultipleBuffering,
                name);
            newElement.InputMultipleBuffering = previousElement.OutputMultipleBuffering;

            PipelineElements.Insert(index, newElement);
        }
        return newElement;
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

    public void ChangePipelineElementsPlace(int previousIndex, int index)
    {
        var pipelineElement = PipelineElements[previousIndex];
        PipelineElements.RemoveAt(previousIndex);
        if(index > previousIndex)
            PipelineElements.Insert(index - 1, pipelineElement);
        else
            PipelineElements.Insert(index, pipelineElement);
    }

    public void RemoveAt(int index)
    {
        if(index != PipelineElements.Count - 1)
            PipelineElements[index + 1].InputMultipleBuffering = PipelineElements[index].InputMultipleBuffering;
        PipelineElements[index].OutputMultipleBuffering.Dispose();
        PipelineElements.RemoveAt(index);
    }
}
