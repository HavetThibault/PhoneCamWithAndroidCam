using Helper.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Wpf.Common.Controls;

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class PipelineChooserViewModel : DialogWindowViewModel
    {
        public ObservableCollection<string> PipelinesName { get; set; }
        public string SelectedPipeline { get; set; }

        public PipelineChooserViewModel(IEnumerable<string> pipelinesName) : base("Choose the pipeline")
        {
            PipelinesName = new(pipelinesName);
            SelectedPipeline = null;
        }

        protected override bool CanOk(object parameter)
        {
            return true;
        }

        protected override bool CanCancel(object parameter)
        {
            return true;
        }
    }
}
