using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public interface IPipelineProcess
    {
        public void Process(MultipleBuffering inputBuffer, MultipleBuffering outputBuffer);
    }
}
