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
            return XmlSerializerHelper.Deserialize<StreamsInfo>(filePath);
        }

        public List<PipelineStructure> ActivePipelines { get; set; }
        public List<PipelineStructure> PipelineTemplates { get; set; }

        public StreamsInfo() { }

        public StreamsInfo(IEnumerable<PipelineStructure> activePipelines, IEnumerable<PipelineStructure> pipelineTemplates)
        {
            ActivePipelines = new(activePipelines);
            PipelineTemplates = new(pipelineTemplates);
        }

        public void Serialize(string filePath)
        {
            XmlSerializerHelper.Serialize(filePath, this);
        }
    }
}
