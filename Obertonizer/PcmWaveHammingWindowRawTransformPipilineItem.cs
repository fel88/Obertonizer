namespace Obertonizer
{
    public class PcmWaveHammingWindowRawTransformPipilineItem : IPipelineItem
    {
        public bool Enabled { get; set; }

        public object Process(object input)
        {
            var data = (float[])input;

            float[] ret = new float[data.Length];
            var w = PcmWindowItem.HammingWindow(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                ret[i] = w[i] * data[i];
            }

            return ret.ToArray();

        }

        public Type InputType
        {
            get { return typeof(float[]); }
        }

        public Type OutputType
        {
            get { return typeof(float[]); }
        }
    }
}