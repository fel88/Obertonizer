namespace Obertonizer
{
    public class Koef
    {
        public double Phase;
        public double Period;
        public double Amplit;
        public double Freq;
        public double MelFreq;
        public double Re;
        public double Img;
        public static Random Rand = new Random((int)DateTime.Now.Ticks);
        public static Koef Generate()
        {
            Koef ret = new Koef();

            ret.Amplit = Rand.NextDouble() * 20;
            ret.Period = (Rand.NextDouble() * 15);
            ret.Phase = Rand.NextDouble() * Math.PI;
            return ret;
        }

        public double Calc(double v)
        {
            return Amplit * Math.Cos(v * Period + Phase);
        }
    }
}