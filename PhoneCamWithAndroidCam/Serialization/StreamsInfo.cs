using Helper.Serializer;
using PhoneCamWithAndroidCam.ViewModels;
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

        public string FilePath { get; set; }
        public List<PipelineStructure> ActivePipelines { get; set; }
        public List<PipelineStructure> TemplatePipelines { get; set; }

        public StreamsInfo() { }

        public StreamsInfo(string filePath, StreamsViewModel viewModel)
        {
            ActivePipelines = new();
            foreach(var streamView in viewModel.StreamViews)
                ActivePipelines.Add(new PipelineStructure(streamView.Pipeline));
            TemplatePipelines = new();
            FilePath = filePath;
        }

        public void Serialize()
        {
            XmlSerializerHelper.Serialize(FilePath, this);
        }
    }
}
