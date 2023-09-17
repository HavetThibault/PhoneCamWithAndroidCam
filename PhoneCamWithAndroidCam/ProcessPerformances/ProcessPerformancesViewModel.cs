using Helper.MVVM;
using ProcessingPipelines.ImageProcessingPipeline;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Theraot.Collections;

namespace PhoneCamWithAndroidCam.ProcessPerformances
{
    public class ProcessPerformancesViewModel : BindableClass
    {
        public ObservableCollection<ProcessPerformancesModel> ProcessPerformances { get; set; }

        public ProcessPerformancesViewModel()
        {
            ProcessPerformances = new();
        }

        public ProcessPerformancesViewModel(ImageProcessingPipeline pipeline)
        {
            ProcessPerformances = new();
            foreach (var element in pipeline.PipelineElements)
                ProcessPerformances.Add(element.ProcessPerformances);
        }

        public ProcessPerformancesViewModel(IEnumerable<ProcessPerformancesModel> processPerformances)
        {
            ProcessPerformances = new(processPerformances);
        }

        public void RefreshProcessPerformances(ImageProcessingPipeline pipeline, ProcessPerformancesModel rawJpegConversionPerf)
        {
            ProcessPerformances.Clear();
            foreach (var element in pipeline.PipelineElements)
                ProcessPerformances.Add(element.ProcessPerformances);
            ProcessPerformances.Add(rawJpegConversionPerf);
        }
    }
}
