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
            set
            {
                Pipeline.Name = value;
            }
        }

        public PipelineEditorViewModel(ProducerConsumerBuffers inputBuffer) : base("Create pipeline")
        {
            Items = new()
            {
                new AddPipelineButtonViewModel(0, AddPipelineElement)
            };
            Pipeline = new ImageProcessingPipeline(inputBuffer);
        }

        public PipelineEditorViewModel(ImageProcessingPipeline pipeline) : base($"Edit '{pipeline.Name}'")
        {
            Pipeline = pipeline;
            Items = new()
            {
                new AddPipelineButtonViewModel(0, AddPipelineElement)
            };
            int i = 0;
            foreach(var element in pipeline.PipelineElements)
                AddPipelineElement(i++, element.Name);
        }

        private void AddPipelineElement(int index, string elementName)
        {
            int offset = index * 4 + 1; 
            Items.Insert(offset++, new VerticalLine());
            Items.Insert(offset++, new PipelineElementViewModel(elementName));
            Items.Insert(offset++, new VerticalLine());
            Items.Insert(offset, new AddPipelineButtonViewModel(1, AddPipelineElement));

            Pipeline.Insert(index, elementName);
        }
    }
}
