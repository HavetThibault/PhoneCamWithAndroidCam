using ProcessingPipelines.PipelineUtils;
using Helper.MVVM;

namespace ProcessingPipelines.PipelineUtils;

public class ProcessPerformancesModel : BindableClass
{
    private long _waitingReadTimeMs = 0;
    private long _processTimeMs = 0;
    private long _waitingWriteTimeMs = 0;
    private int _fps = 0;
    private string _processName;

    public int Fps
    {
        get => _fps;
        set => SetProperty(ref _fps, value);
    }

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
        _processName = processName;
    }

    public override string ToString()
    {
        return $"{nameof(WaitingReadTimeMs)}:{WaitingReadTimeMs} - {nameof(ProcessTimeMs)}:{ProcessTimeMs} - {nameof(WaitingWriteTimeMs)}:{WaitingWriteTimeMs}";
    }
}
