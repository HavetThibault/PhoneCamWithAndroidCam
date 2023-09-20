using Helper.Serializer;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace PhoneCamWithAndroidCam.Serialization
{
    public class StreamsInfo
    {
        public static StreamsInfo? TryLoad(string filePath)
        {
            var streamsInfo = XmlSerializerHelper.Deserialize<StreamsInfo>(filePath);
            streamsInfo?.InitFrameProcessorsAfterDeserialization();
            return streamsInfo;
        }

        public List<PipelineStructure> ActivePipelines { get; set; }
        public List<PipelineStructure> PipelineTemplates { get; set; }

        private StreamsInfo() { }

        public StreamsInfo(IEnumerable<PipelineStructure> activePipelines, IEnumerable<PipelineStructure> pipelineTemplates)
        {
            ActivePipelines = new(activePipelines);
            PipelineTemplates = new(pipelineTemplates);
        }

        private void InitFrameProcessorsAfterDeserialization()
        {
            foreach(var pipeline in ActivePipelines)
                foreach(var frameProcessor in pipeline.PipelineElementFrameProcessor)
                    frameProcessor.InitAfterDeserialization();

            foreach (var pipeline in PipelineTemplates)
                foreach (var frameProcessor in pipeline.PipelineElementFrameProcessor)
                    frameProcessor.InitAfterDeserialization();
        }

        public void Serialize(string filePath)
        {
            XmlSerializerHelper.Serialize(filePath, this);
        }
    }
}
