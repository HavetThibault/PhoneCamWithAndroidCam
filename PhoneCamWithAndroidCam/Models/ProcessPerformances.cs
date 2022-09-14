using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfUtils;

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
        set => SetProperty(ref _waitingReadTimeMs, value);
    }

    public long ProcessTimeMs
    {
        get => _processTimeMs;
        set => SetProperty(ref _processTimeMs, value);
    }

    public long WaitingWriteTimeMs
    {
        get => _waitingWriteTimeMs;
        set => SetProperty(ref _waitingWriteTimeMs, value);
    }
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
