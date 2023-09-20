using ImageProcessingUtils.FrameProcessor;
using PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels;
using ProcessingPipelines.ImageProcessingPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels.MapThreshold
{
    internal class ColorMapThresholdViewModel : PipelineElementViewModel
    {
        public int MIN_INTERVAL_NUMBER { get; } = 2;
        public int MAX_INTERVAL_NUMBER { get; } = 128;

        private ColorMapThreshold _colorMapThreshold;

        public int IntervalNbr
        {
            get => _colorMapThreshold.IntervalNbr;
            set
            {
                _colorMapThreshold.IntervalNbr = value;
                NotifyPropertyChanged(nameof(IntervalNbr));
            }
        }

        public ColorMapThresholdViewModel(string name, ColorMapThreshold colorMapThreshold) 
            : base(name)
        {
            _colorMapThreshold = colorMapThreshold;
        }
    }
}
