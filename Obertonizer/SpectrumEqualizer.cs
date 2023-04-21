namespace Obertonizer
{
    public class SpectrumEqualizer : IPipelineItem
    {
        public SpectrumEqualizer()
        {
            Enabled = true;
        }
        public bool Enabled { get; set; }

        public int SampleRate { get; set; }

        public int HiFreq { get; set; }
        public int LowFreq { get; set; }
        public double Intensity { get; set; }

        public object Process(object input)
        {

            var data = (double[])input;
            double fdx = SampleRate / (double)data.Length;

            double[] ret = new double[data.Length];
            int n1 = (int)(LowFreq / fdx);
            int n2 = (int)(HiFreq / fdx);



            for (int i = 0; i < data.Length; i++)
            {
                ret[i] = data[i];
                if (i >= n1 && i <= n2)
                {
                    double pos = (i - n1) / (double)(n2 - n1);
                    pos = 1.0;
                    ret[i] = data[i] + data[i] * (pos * Intensity);
                }
            }

            return ret.ToArray();

        }

        public Type InputType
        {
            get { return typeof(double[]); }
        }

        public Type OutputType
        {
            get { return typeof(double[]); }
        }
    }
}
