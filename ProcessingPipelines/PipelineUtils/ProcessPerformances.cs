using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingPipelines.PipelineUtils;

public class ProcessPerformances
{
    public long WaitingReadTimeMs { get; set; }
    public long ProcessTimeMs { get; set; }
    public long WaitingWriteTimeMs { get; set; }
    public string ProcessName { get; set; }

    public ProcessPerformances(string processName)
    {
        ProcessName = processName;
    }

    public override string ToString()
    {
        return $"{nameof(WaitingReadTimeMs)}:{WaitingReadTimeMs} - {nameof(ProcessTimeMs)}:{ProcessTimeMs} - {nameof(WaitingWriteTimeMs)}:{WaitingWriteTimeMs}";
    }

    public ProcessPerformances Clone()
    {
        return new ProcessPerformances(ProcessName)
        {
            WaitingReadTimeMs = WaitingReadTimeMs,
            ProcessTimeMs = ProcessTimeMs,
            WaitingWriteTimeMs = WaitingWriteTimeMs
        };
    }
}
