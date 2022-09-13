using AndroidCamClient;

namespace ProcessingPipelines.PipelineFeeder
{
    public class PipelineFeederPipeline
    {
        private PhoneCamClient _phoneCamClient;

        public PipelineFeederPipeline(PhoneCamClient phoneCamClient)
        {
            _phoneCamClient = phoneCamClient;
        }
    }
}
