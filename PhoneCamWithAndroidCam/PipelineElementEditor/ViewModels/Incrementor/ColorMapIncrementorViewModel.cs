using Helper.MVVM;
using ImageProcessingUtils.FrameProcessor;
using PhoneCamWithAndroidCam.PipelineElementEditor.ViewModel;
using ProcessingPipelines.ImageProcessingPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.PipelineElementEditor.ViewModel.Incrementor
{
    public class ColorMapIncrementorViewModel : PipelineElementViewModel
    {
        private ColorMapIncrementor _incrementor;

        public int MAX_FRAMES_NBR_BEFORE_INCREMENT { get; } = 60;
        public int MIN_FRAMES_NBR_BEFORE_INCREMENT { get; } = 1;

        public int MAX_INCREMENT { get; } = 127;
        public int MIN_INCREMENT { get; } = 1;

        public int FramesNbrBeforeIncrement
        {
            get => _incrementor.FramesNbrBeforeIncrement;
            set
            {
                _incrementor.FramesNbrBeforeIncrement = value;
                NotifyPropertyChanged(nameof(FramesNbrBeforeIncrement));
            }
        }

        public int Increment
        {
            get => _incrementor.Increment;
            set
            {
                _incrementor.Increment = value;
                NotifyPropertyChanged(nameof(Increment));
            }
        }

        public ColorMapIncrementorViewModel(string elementName, ColorMapIncrementor incrementor) : base(elementName)
        {
            _incrementor = incrementor;
        }
    }
}
