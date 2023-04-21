namespace Obertonizer
{
    public class PcmWavePreEmphasisPipilineItem : IPipelineItem
    {
        public PcmWavePreEmphasisPipilineItem()
        {
            Enabled = true;
        }
        public bool Enabled { get; set; }

        public object Process(object input)
        {

            var data = (float[])input;
            if (data.Length > 0)
            {
                List<float> ret = new List<float>();
                ret.Add(0);
                for (int i = 1; i < data.Length; i++)
                {
                    ret.Add(data[i] - (float)0.95 * data[i - 1]);
                }

                return ret.ToArray();
            }
            return input;

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