namespace ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines
{
    public class ColorBuffer
    {
        public byte[] ColorsBuffer { get; set; }

        public ColorBuffer()
        {
            ColorsBuffer = new byte[256];
            for (int i = 0; i < ColorsBuffer.Length; i++)
                ColorsBuffer[i] = (byte)i;
        }

        public void NextColorBuffer()
        {
            for (int i = 0; i < ColorsBuffer.Length; i++)
            {
                int color = ColorsBuffer[i] - 1;
                if (color < 0)
                    color = 255;
                ColorsBuffer[i] = (byte)color;
            }
        }
    }
}
