using ImageProcessingUtils.FrameProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels.Scanner
{
    public class ScannerViewModel : PipelineElementViewModel
    {
        private ScannerProcessor _scanner;

        public int MAX_SCAN_STEP { get; } = 30;
        public int MIN_SCAN_STEP { get; } = 1;

        public int MAX_SCAN_INTERVAL_MS { get; } = 30000;
        public int MIN_SCAN_INTERVAL_MS { get; } = 1000;

        public int ScanStep
        {
            get => _scanner.ScanStep;
            set {
                _scanner.ScanStep = value;
                NotifyPropertyChanged(nameof(ScanStep));
            }
        }

        public int ScanIntervalMs
        {
            get => _scanner.ScanIntervalMs;
            set {
                _scanner.ScanIntervalMs = value;
                NotifyPropertyChanged(nameof(ScanIntervalMs));
            }
        }

        public ScannerViewModel(string elementName, ScannerProcessor scanner) : base(elementName)
        {
            _scanner = scanner;
        }
    }
}
