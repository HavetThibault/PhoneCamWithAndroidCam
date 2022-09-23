namespace ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines
{
    public class ColorBuffer
    {
        public byte[] ColorsBuffer { get; set; }
        public bool[] UpOrDownBuffer { get; set; }

        public ColorBuffer()
        {
            ColorsBuffer = new byte[256];
            for (int i = 0; i < ColorsBuffer.Length; i++)
                ColorsBuffer[i] = (byte)i;
            UpOrDownBuffer = new bool[256];
        }

        public void NextColorBuffer()
        {
            int color;
            for (int i = 0; i < ColorsBuffer.Length; i++)
            {
                if (UpOrDownBuffer[i])
                {
                    color = ColorsBuffer[i] + 1;
                    if(color > 255)
                        UpOrDownBuffer[i] = false;
                } 
                else
                {
                    color = ColorsBuffer[i] - 1;
                    if (color < 0)
                        UpOrDownBuffer[i] = true;
                }
                
                ColorsBuffer[i] = (byte)color;
            }
        }
    }
}
