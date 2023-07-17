using ProcessingPipelines.PipelineUtils;
using Helper.MVVM;

namespace PhoneCamWithAndroidCam.Models;

public class ProcessPerformancesModel : BindableClass
{
    private long _waitingReadTimeMs;
    private long _processTimeMs;
    private long _waitingWriteTimeMs;
    private string _processName;

    public long WaitingReadTimeMs
    {
        get => _waitingReadTimeMs;
        set
        {
            SetProperty(ref _waitingReadTimeMs, value);
            NotifyPropertyChanged(nameof(WaitingReadTimeForHundred));
        }
    }

    public double WaitingReadTimeForHundred => (double)WaitingReadTimeMs / IterationTotalTime * 100; 

    public long ProcessTimeMs
    {
        get => _processTimeMs;
        set
        {
            SetProperty(ref _processTimeMs, value);
            NotifyPropertyChanged(nameof(ProcessTimeForHundred));
        }
    }

    public double ProcessTimeForHundred => (double)ProcessTimeMs / IterationTotalTime * 100;

    public long WaitingWriteTimeMs
    {
        get => _waitingWriteTimeMs;
        set
        {
            SetProperty(ref _waitingWriteTimeMs, value);
            NotifyPropertyChanged(nameof(WaitingWriteTimeForHundred));
        }
    }

    public double WaitingWriteTimeForHundred => (double)WaitingWriteTimeMs / IterationTotalTime * 100;

    public long IterationTotalTime => WaitingReadTimeMs + ProcessTimeMs + WaitingWriteTimeMs;

    public string ProcessName
    {
        get => _processName;
        set => SetProperty(ref _processName, value);
    }

    public ProcessPerformancesModel(string processName)
    {
        ProcessName = processName;
    }

    public ProcessPerformancesModel(ProcessPerformances perfs)
    {
        WaitingReadTimeMs = perfs.WaitingReadTimeMs;
        ProcessTimeMs = perfs.ProcessTimeMs;
        WaitingWriteTimeMs = perfs.WaitingWriteTimeMs;
        ProcessName = perfs.ProcessName;
    }

    public override string ToString()
    {
        return $"{nameof(WaitingReadTimeMs)}:{WaitingReadTimeMs} - {nameof(ProcessTimeMs)}:{ProcessTimeMs} - {nameof(WaitingWriteTimeMs)}:{WaitingWriteTimeMs}";
    }

    public void Copy(ProcessPerformances processPerf)
    {
        WaitingReadTimeMs = processPerf.WaitingReadTimeMs;
        ProcessTimeMs = processPerf.ProcessTimeMs;
        WaitingWriteTimeMs = processPerf.WaitingWriteTimeMs;
        ProcessName = processPerf.ProcessName;
    }
}
