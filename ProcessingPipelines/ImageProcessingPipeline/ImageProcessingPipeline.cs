using Helper.Collection;
using ImageProcessingUtils;
using ImageProcessingUtils.FrameProcessor;
using ProcessingPipelines.PipelineUtils;
using System.Windows.Threading;

namespace ProcessingPipelines.ImageProcessingPipeline;

public class ImageProcessingPipeline
{
    private CancellationTokenSource _specificCancellationToken;
    private ProducerConsumerBuffers _inputBuffer;
    private Dispatcher _uiDispatcher;

    public ProducerConsumerBuffers InputBuffer
    {
        get => _inputBuffer;
        set
        {
            _inputBuffer = value;
            var firstElement = PipelineElements.First() ?? 
                throw new AggregateException("You should first assign pipeline elements to the pipeline before setting the InputBuffer.");
            firstElement.InputBuffers = value;
        }
    }

    public bool IsStreaming { get; set; } = false;

    public string Name { get; set; }

    public List<PipelineElement> PipelineElements { get; set; }

    public ProducerConsumerBuffers OutputBuffers => PipelineElements.Last().OutputBuffers;

    public ImageProcessingPipeline(string name, ProducerConsumerBuffers inputBuffer, Dispatcher uiDispatcher)
    {
        PipelineElements = new();
        _inputBuffer = inputBuffer;
        _specificCancellationToken = new();
        _uiDispatcher = uiDispatcher;
        Name = name;
    }

    public ImageProcessingPipeline(ImageProcessingPipeline pipeline, Dispatcher uiDispatcher) 
        : this(pipeline.InputBuffer, pipeline, uiDispatcher)
    { }

    public ImageProcessingPipeline(ProducerConsumerBuffers inputBuffer, ImageProcessingPipeline pipeline, Dispatcher uiDispatcher) 
        : this(pipeline.Name, inputBuffer, uiDispatcher)
    {
        var firstElem = pipeline.PipelineElements.First();
        var clonedElement = firstElem.Clone(InputBuffer, firstElem.OutputBuffers.Clone());
        PipelineElements.Add(clonedElement);
        var previousElementOutput = clonedElement.OutputBuffers;
        for(int i = 1; i < pipeline.PipelineElements.Count; i++)
        {
            var element = pipeline.PipelineElements[i];
            clonedElement = element.Clone(previousElementOutput, element.OutputBuffers.Clone());
            PipelineElements.Add(clonedElement);
            previousElementOutput = clonedElement.OutputBuffers;
        }
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

    public void InstantiateAndAdd(FrameProcessor frameProcessor)
    {
        InstantiateAndInsert(PipelineElements.Count, frameProcessor);
    }

    public PipelineElement InstantiateAndInsert(int index, FrameProcessor frameProcessor)
    {
        string name = GetNextAvailableElementName(frameProcessor.ElementTypeName);
        PipelineElement previousElement;
        PipelineElement nextElement;
        PipelineElement newElement;
        if (index == 0)
        {
            newElement = new(_uiDispatcher, name, frameProcessor, InputBuffer, InputBuffer.Clone());
            PipelineElements.Insert(0, newElement);
        }
        else if (index == PipelineElements.Count)
        {
            previousElement = PipelineElements.Last();
            newElement = new(_uiDispatcher, name, frameProcessor, previousElement.OutputBuffers, previousElement.OutputBuffers.Clone());
            PipelineElements.Add(newElement);
        }
        else
        {
            previousElement = PipelineElements.ElementAt(index - 1);
            previousElement.OutputBuffers = previousElement.OutputBuffers.Clone();
            nextElement = PipelineElements.ElementAt(index);
            newElement = new(_uiDispatcher, name, frameProcessor, previousElement.OutputBuffers, nextElement.InputBuffers);
            PipelineElements.Insert(index, newElement);
        }
        return newElement;
    }

    public PipelineElement InstantiateAndInsert(int index, string elementType)
    {
        string name = GetNextAvailableElementName(elementType);
        PipelineElement previousElement;
        PipelineElement nextElement;
        PipelineElement newElement;
        if (index == 0)
        {
            newElement = PipelineElementFactory.GetInstance(
                elementType,
                InputBuffer.Clone(),
                name,
                _uiDispatcher);
            newElement.InputBuffers = InputBuffer;
                
            PipelineElements.Insert(0, newElement);
        }
        else if(index == PipelineElements.Count)
        {
            previousElement = PipelineElements.Last();
            newElement = PipelineElementFactory.GetInstance(
                elementType, 
                previousElement.OutputBuffers.Clone(),
                name,
                _uiDispatcher);

            newElement.InputBuffers = previousElement.OutputBuffers;
            PipelineElements.Add(newElement);
        }
        else
        {
            previousElement = PipelineElements.ElementAt(index - 1);
            previousElement.OutputBuffers = previousElement.OutputBuffers.Clone();
            nextElement = PipelineElements.ElementAt(index);
            newElement = PipelineElementFactory.GetInstance(
                elementType,
                nextElement.InputBuffers,
                name,
                _uiDispatcher);
            newElement.InputBuffers = previousElement.OutputBuffers;

            PipelineElements.Insert(index, newElement);
        }
        return newElement;
    }

    public void Start(CancellationTokenSource globalCancellationToken)
    {
        IsStreaming = true;
        _specificCancellationToken.TryReset();
        foreach (PipelineElement pipelineElement in PipelineElements)
            pipelineElement.LaunchNewWorker(globalCancellationToken, _specificCancellationToken);
    }

    private void Stop()
    {
        _specificCancellationToken.Cancel();
        IsStreaming = false;
    }

    public void Dispose()
    {
        if (IsStreaming)
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
            PipelineElements[index + 1].InputBuffers = PipelineElements[index].InputBuffers;
        PipelineElements[index].OutputBuffers.Dispose();
        PipelineElements.RemoveAt(index);
    }
}
