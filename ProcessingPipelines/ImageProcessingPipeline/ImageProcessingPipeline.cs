using Helper.Collection;
using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;
using System.Windows.Threading;

namespace ProcessingPipelines.ImageProcessingPipeline;

public class ImageProcessingPipeline
{
    
    private CancellationTokenSource _specificCancellationToken;
    private bool _isStreaming = false;
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

    public string Name { get; set; } = "Default name";

    public List<PipelineElement> PipelineElements { get; set; }

    public ProducerConsumerBuffers OutputBuffer => PipelineElements.Last().OutputBuffers;

    public List<ProcessPerformancesModel> ElementsProcessPerformances { get; set; }

    public ImageProcessingPipeline(ProducerConsumerBuffers inputBuffer)
    {
        PipelineElements = new();
        _inputBuffer = inputBuffer;
        ElementsProcessPerformances = new();
        _specificCancellationToken = new();
    }

    public ImageProcessingPipeline(ProducerConsumerBuffers inputBuffer, ImageProcessingPipeline pipeline) : this(inputBuffer)
    {
        int i = 0;
        ProducerConsumerBuffers previousElementOutput = null;
        foreach (var element in pipeline.PipelineElements)
        {
            PipelineElement copiedElement;
            if(i == 0)
                PipelineElements.Add(copiedElement = element.Clone(InputBuffer, (ProducerConsumerBuffers)element.OutputBuffers.Clone()));
            else
                PipelineElements.Add(copiedElement = element.Clone(previousElementOutput, (ProducerConsumerBuffers)element.OutputBuffers.Clone()));
            previousElementOutput = copiedElement.OutputBuffers;
            i = 1;
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
                (ProducerConsumerBuffers)InputBuffer.Clone(),
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
                (ProducerConsumerBuffers)previousElement.OutputBuffers.Clone(),
                name,
                _uiDispatcher);

            newElement.InputBuffers = previousElement.OutputBuffers;
            PipelineElements.Add(newElement);
        }
        else
        {
            previousElement = PipelineElements.ElementAt(index - 1);
            previousElement.OutputBuffers = 
                (ProducerConsumerBuffers)previousElement.OutputBuffers.Clone();
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
            PipelineElements[index + 1].InputBuffers = PipelineElements[index].InputBuffers;
        PipelineElements[index].OutputBuffers.Dispose();
        PipelineElements.RemoveAt(index);
    }
}
