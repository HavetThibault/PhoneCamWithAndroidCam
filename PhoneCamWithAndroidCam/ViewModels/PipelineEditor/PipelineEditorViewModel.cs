using Helper.MVVM;
using ProcessingPipelines.ImageProcessingPipeline;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Common.Controls;

namespace PhoneCamWithAndroidCam.ViewModels.PipelineEditor
{
    public class PipelineEditorViewModel : DialogWindowViewModel
    {
        public ImageProcessingPipeline Pipeline { get; }

        public ObservableCollection<object> Items { get; }

        public string Name
        {
            get => Pipeline.Name;
            set => Pipeline.Name = value;
        }

        public PipelineEditorViewModel(ProducerConsumerBuffers inputBuffer) : base("Create pipeline")
        {
            Items = new();
            Items.Add(new AddPipelineButtonViewModel(AddPipelineElement, Items));
            Pipeline = new ImageProcessingPipeline(inputBuffer);
        }

        public PipelineEditorViewModel(ImageProcessingPipeline pipeline) : base($"Edit '{pipeline.Name}'")
        {
            Pipeline = pipeline;
            Items = new();
            Items.Add(new AddPipelineButtonViewModel(AddPipelineElement, Items));
            int i = 0;
            foreach(var element in pipeline.PipelineElements)
                AddPipelineElement(i++, element.Name);
        }

        private void AddPipelineElement(int index, string elementType)
        {
            var newElement = Pipeline.InstantiateAndInsert(index, elementType);

            int offset = index * 4 + 1;
            Items.Insert(offset++, new VerticalLine());
            Items.Insert(offset++, new PipelineElementViewModel(newElement.Name, DeleteElement));
            Items.Insert(offset++, new VerticalLine());
            Items.Insert(offset, new AddPipelineButtonViewModel(AddPipelineElement, Items));
        }

        internal int GetPipelineElementIndex(PipelineElementViewModel pipelineElement)
        {
            return (Items.IndexOf(pipelineElement) - 2) / 4;
        }

        internal void ChangePipelineElementsPlace(int previousIndex, int index)
        {
            Pipeline.ChangePipelineElementsPlace(previousIndex, index);
            int inListPreviousIndex = previousIndex * 4 + 2;
            int inListNewIndex = index * 4 + 2;
            (Items[inListNewIndex], Items[inListPreviousIndex]) = (Items[inListPreviousIndex], Items[inListNewIndex]);
        }

        private void DeleteElement(object element)
        {
            var pipelineElement = (PipelineElementViewModel)element;
            int index = GetPipelineElementIndex(pipelineElement);
            Pipeline.RemoveAt(index);
            int inListIndex = Items.IndexOf(pipelineElement);
            for (int i = 0; i < 4; i++)
                Items.RemoveAt(inListIndex - 1);
        }
    }
}
