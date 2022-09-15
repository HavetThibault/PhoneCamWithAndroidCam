using PhoneCamWithAndroidCam.Models;
using ProcessingPipelines.PipelineUtils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class ProcessPerformancesViewModel
    {
        private Dispatcher _uiDispatcher;

        public ObservableCollection<ProcessPerformancesModel> ProcessPerformances { get; set; }

        public ProcessPerformancesViewModel(Dispatcher uiDispatcher)
        {
            ProcessPerformances = new();
            _uiDispatcher = uiDispatcher;
        }

        public void UpdatePerformances(List<ProcessPerformances> processPerformances)
        {
            foreach (ProcessPerformances processPerformance in processPerformances)
            {
                if (ProcessPerformances.Where(p => p.ProcessName.Equals(processPerformance.ProcessName)).FirstOrDefault() is ProcessPerformancesModel matchingProcessPerf)
                {
                    _uiDispatcher.BeginInvoke(UpdatePerformance, matchingProcessPerf, processPerformance);
                }
                else
                {
                    ProcessPerformances perf;
                    lock (processPerformance)
                        perf = processPerformance.Clone();

                    _uiDispatcher.BeginInvoke(AddPerformanceStat, perf);
                }
            }
        }

        private void AddPerformanceStat(ProcessPerformances perf)
        {
            ProcessPerformances.Add(new(perf));
        }

        private void UpdatePerformance(ProcessPerformancesModel perfsModel, ProcessPerformances perf)
        {
            lock (perf)
                perfsModel.Copy(perf);
        }
    }
}
