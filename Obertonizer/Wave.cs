namespace Obertonizer
{
    public class Wave
    {

        public static Random Rand = new Random(DateTime.Now.Millisecond);

        public Wave()
        {
            Pen = new Pen(Color.FromArgb(Rand.Next(255), Rand.Next(255), Rand.Next(255)));
        }

        public double[] Counts;

        public double MinPower
        {
            get
            {
                return Counts.Min(x => Math.Pow(x, 2));
            }

        }

        public double MaxPower
        {
            get
            {
                return Counts.Max(x => Math.Pow(x, 2));
            }

        }

        public double DynamicDiap
        {
            get
            {
                return Math.Log10(MaxPower / MinPower);
            }
        }

        public Pen Pen { get; set; }


    }
}
