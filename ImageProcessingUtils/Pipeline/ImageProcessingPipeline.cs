using ImageProcessingUtils;
using PhoneCamWithAndroidCam.Threads;
using System;
using System.Drawing;


namespace ImageProcessingUtils.Pipeline;

public class ImageProcessingPipeline
{
    private List<PipelineElement> _pipelineElements { get; set; }

    public ImageProcessingPipeline()
    {
        _pipelineElements = new();
    }

    public MultipleBuffering GetInputBuffer() => _pipelineElements.First().InputMultipleBuffering;

    public MultipleBuffering GetOutputBuffer() => _pipelineElements.Last().OutputMultipleBuffering;
}
