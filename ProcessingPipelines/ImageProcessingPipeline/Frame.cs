namespace ImageProcessingUtils.Pipeline
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
