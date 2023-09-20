using Helper.Collection;
using ImageProcessingUtils;
using ImageProcessingUtils.FrameProcessor;
using ProcessingPipelines.PipelineUtils;
using System.Windows.Threading;
using System.Xml.Linq;

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
        var (inputBuffer, outputBuffer) = GetInputOutputBuffersAndWireForInsert(index);
        var newElement = InstantiatePipelineElement(inputBuffer, outputBuffer, frameProcessor);
        PipelineElements.Insert(index, newElement);
        return newElement;
    }

    public PipelineElement InstantiateAndInsert(int index, string elementType)
    {
        var (inputBuffer, outputBuffer) = GetInputOutputBuffersAndWireForInsert(index);
        var newElement = InstantiatePipelineElement(inputBuffer, outputBuffer, elementType);
        PipelineElements.Insert(index, newElement);
        return newElement;
    }

    private PipelineElement InstantiatePipelineElement(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer, FrameProcessor frameProcessor)
    {
        string name = GetNextAvailableElementName(frameProcessor.ElementTypeName);
        return new(_uiDispatcher, name, frameProcessor, inputBuffer, outputBuffer);
    }

    private PipelineElement InstantiatePipelineElement(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer, string elementTypeName)
    {
        string name = GetNextAvailableElementName(elementTypeName);
        return PipelineElementFactory.GetInstance(elementTypeName, inputBuffer, outputBuffer, name, _uiDispatcher);
    }

    private (ProducerConsumerBuffers, ProducerConsumerBuffers) GetInputOutputBuffersAndWireForInsert(int index)
    {
        (ProducerConsumerBuffers, ProducerConsumerBuffers) inputOutputBuffers;

        if(index == 0) {
            inputOutputBuffers.Item1 = InputBuffer;
            if(PipelineElements.Count == 0)
                inputOutputBuffers.Item2 = InputBuffer.Clone();
            else {
                inputOutputBuffers.Item2 = PipelineElements[0].InputBuffers.Clone();
                PipelineElements[0].InputBuffers = inputOutputBuffers.Item2;
            }
        }
        else if(index == PipelineElements.Count) {
            inputOutputBuffers.Item1 = PipelineElements[index - 1].OutputBuffers;
            inputOutputBuffers.Item2 = inputOutputBuffers.Item1.Clone();
        }
        else
        {
            var nextElement = PipelineElements.ElementAt(index);
            inputOutputBuffers.Item1 = nextElement.InputBuffers;
            nextElement.InputBuffers = nextElement.InputBuffers.Clone();
            inputOutputBuffers.Item2 = nextElement.InputBuffers;
        }
        return inputOutputBuffers;
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
