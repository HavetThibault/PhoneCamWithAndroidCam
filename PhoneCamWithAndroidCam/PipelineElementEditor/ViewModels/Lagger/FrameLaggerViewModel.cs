using ImageProcessingUtils.FrameProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels.Lagger
{
    public class FrameLaggerViewModel : PipelineElementViewModel
    {
        public int MIN_LAGGED_FRAMES_NBR { get; } = 2;
        public int MAX_LAGGED_FRAMES_NBR { get; } = 100;

        private FrameLagger _frameLagger;

        public int LaggedFramesNbr
        {
            get => _frameLagger.LaggedFramesNbr;
            set
            {
                _frameLagger.LaggedFramesNbr = value;
                NotifyPropertyChanged(nameof(LaggedFramesNbr));
            }
        }

        public FrameLaggerViewModel(string name, FrameLagger frameLagger)
            : base(name)
        {
            _frameLagger = frameLagger;
        }
    }
}
