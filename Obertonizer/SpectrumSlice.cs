namespace Obertonizer
{
    public partial class SpectrumSlice
    {
        
        public float[] Orig;//fft orig
        public double[] Spectrum;
        public short[] SpectrumUshorts;

        public int FreqIndex = 0;
        public double Maximum { get; set; }
        public double Minimum { get; set; }

        public static double[] Interpolate(SpectrumSlice slice1, SpectrumSlice slice2, double filterDb, double position, double max)
        {
            var g1 = slice1.GetDbGraph(filterDb, max);
            var g2 = slice2.GetDbGraph(filterDb, max);

            List<double> res = new List<double>();
            for (int j = 0; j < g1.Count(); j++)
            {
                var rMax = g2[j];
                var rMin = g1[j];

                var aver = rMin + (int)((rMax - rMin) * position);
                res.Add(aver);
            }


            return res.ToArray();
        }


        public const double DbKoef = 20.0;

        public static double GetdB(double val, double orig)
        {
            return DbKoef * Math.Log10(val / orig);
        }


        public double[] GetLinearGraph(double filterDb, double max)
        {

            //var max = Spectrum.Max(x => x.Magnitude);
            var min2 = Spectrum.Min(x => x);


            var treshold = max * 0.0001;
            treshold = max * Math.Pow(10, (filterDb / DbKoef));
            //    treshold = min2;

            List<double> dbs = new List<double>();
            for (int i = 0; i < Spectrum.Count(); i++)
            {
                var val = Spectrum[i];

                dbs.Add(val / max);
            }

            return dbs.ToArray();
        }

        
        public double[] GetDbGraph(double filterDb, double max)
        {

            //var max = Spectrum.Max(x => x.Magnitude);
            var min2 = Spectrum.Min(x => x);


            var treshold = max * 0.0001;
            treshold = max * Math.Pow(10, (filterDb / DbKoef));
            //    treshold = min2;

            List<double> dbs = new List<double>();
            for (int i = 0; i < Spectrum.Count(); i++)
            {
                var val = Spectrum[i];
                if (val < treshold)
                {
                    val = treshold;
                }

                dbs.Add(GetdB(val, max));
            }

            return dbs.ToArray();
        }

        public void Quantificate()
        {

        }

        
        public double GetEnergy(bool flat = false)
        {
            double ret = 0;
            for (int i = 0; i < Spectrum.Count(); i++)
            {
                //var d = Spectrum[i].Real * Spectrum[i].Real + Spectrum[i].Imaginary * Spectrum[i].Imaginary;
                var d = Spectrum[i];
                if (!flat)
                {
                    ret += Math.Pow(d, 2);
                }
                else
                {
                    ret += d;
                }
            }
            if (flat)
            {
                return ret;
            }
            return Math.Log(ret);
        }

    }
}