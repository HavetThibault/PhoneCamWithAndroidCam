using ImageProcessingUtils;
using PhoneCamWithAndroidCam.Threads;
using System;
using System.Drawing;


namespace ImageProcessingUtils.Pipeline;

public abstract class PipelineElement
{
    public delegate void Process(MultipleBuffering inputMultipleBuffering, MultipleBuffering outputMultipleBuffering);

    public MultipleBuffering InputMultipleBuffering {get; set;}
    public MultipleBuffering OutputMultipleBuffering {get; set;}
    private Process _process;

    public PipelineElement(Process process, MultipleBuffering outputMultipleBuffering)
    {
        OutputMultipleBuffering = outputMultipleBuffering;
        InputMultipleBuffering = null;
        _process = process;
    }

    public PipelineElement(Process process, MultipleBuffering inputMultipleBuffering, MultipleBuffering outputMultipleBuffering)
    {
        OutputMultipleBuffering = outputMultipleBuffering;
        InputMultipleBuffering = inputMultipleBuffering;
        _process = process;
    }

    public 
}
