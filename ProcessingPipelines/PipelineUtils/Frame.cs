namespace ProcessingPipelines.PipelineUtils
{
    public class Frame
    {
        public byte[] Data { get; set; }

        public Frame(byte[] data)
        {
            Data = data;
        }
    }
}
