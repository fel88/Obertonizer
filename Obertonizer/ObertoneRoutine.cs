using NAudio.Wave;

namespace Obertonizer
{
    public class ObertoneRoutine
    {

        public static int[] CalcSegmentsUsengBaseToneQuantiz(SpectrumBatch sp, int steps)
        {
            var hilofreq = ToneQuantizer(sp, steps);

            List<int> ntrvls = new List<int>();
            for (int i = 1; i < hilofreq.Count(); i++)
            {
                if (hilofreq[i - 1] != hilofreq[i])
                {
                    ntrvls.Add(i);
                }
            }


            return ntrvls.ToArray();
        }


        public static int[] ToneQuantizer(SpectrumBatch sp, int steps)
        {
            var maxfreq = 20e3;
            var fx = maxfreq / steps;

            var gg = sp.Slices.Select(x => x.GetEnergy()).ToArray();


            var ftdx = sp.SampleRate / (decimal)sp.SpectrumLen;

            var art =
                sp.Slices.Select(
                    x =>
                        x.Spectrum.Take(x.Spectrum.Length / 2).Select((y, i) => new KeyValuePair<int, double>(i, y))
                            .OrderByDescending(y => y.Value)
                            .First()
                            .Key * ftdx).ToArray();

            var hilofreq = art.Select(x =>
            {
                if (x > 1500)
                {
                    return 1;
                }
                return 0;
            }).ToArray();
            return hilofreq.ToArray();
        }





        public static List<Color> BaseColors = new List<Color>()
        {
            Color.Black,
            Color.DarkBlue,
            Color.Green,
            Color.Yellow,
            Color.Red,
            Color.Red,
        };
        public static List<Color> BwColors = new List<Color>()
        {
            Color.Black,
            Color.White,
        };


        public static Color Interpolate(Color color1, Color color2, double position)
        {
            int rMax = color2.R;
            int rMin = color1.R;
            int gMax = color2.G;
            int gMin = color1.G;
            int bMax = color2.B;
            int bMin = color1.B;

            var colorList = new List<Color>();
            int size = 100;
            //for (int i = 0; i < size; i++)
            int i = (int)Math.Round(position * 100.0f);
            //{
            var rAverage = rMin + (int)((rMax - rMin) * i / size);
            var gAverage = gMin + (int)((gMax - gMin) * i / size);
            var bAverage = bMin + (int)((bMax - bMin) * i / size);

            //colorList.Add(Color.FromArgb(rAverage, gAverage, bAverage));
            //}
            return Color.FromArgb(rAverage, gAverage, bAverage);
        }


        public static Color GetColor(double position, bool bw = false)//black white
        {
            if (double.IsNaN(position)) return BaseColors[0];
            if (bw)
            {
                return Interpolate(BwColors[0], BwColors[1], position);
            }
            var rp = position * 400.0f;
            int cols = (int)(rp / 100.0f);
            float ost = ((int)(rp % 100.0f)) / 100.0f;
            return Interpolate(BaseColors[cols], BaseColors[cols + 1], ost);
        }




        public static ScaleTypeEnum ScaleType = ScaleTypeEnum.Logarithmic;


        public static bool LocalNormalizeSpectrum = false;


        public static double TopFreq = 20e3;


        public static double GetMaxByDiap(SpectrumBatch sp, int down, int top)
        {
            var fdx = sp.SampleRate / (double)sp.SpectrumLen;
            int n1 = (int)(down / fdx);
            int n2 = (int)(top / fdx);

            var mx = sp.Slices.Max(x => x.Spectrum.Skip(n1).Take(n2 - n1).Max());
            return mx;
        }


        public static List<double[]> LastCalculation = new List<double[]>();

        public static Action<SpectrumSlice> PreNormalizingAction;
        public static bool IsBlackWhiteColorspace { get; set; }



        public static ScaleTypeEnum AmplitudeScale = ScaleTypeEnum.Logarithmic;

        public static Bitmap Get(SpectrumBatch batch, int width, int height, int downFreq, int topFreq, bool adaptative = false)
        {
            var fdx = ((double)batch.SampleRate / batch.SpectrumLen);


            var ntop = (int)(TopFreq / fdx);
            var ndown = (int)(downFreq / fdx);

            int N = (int)(TopFreq / fdx);
            List<double[]> ss = new List<double[]>();
            int windowSize = 50;
            double upperKoef = 1;
            List<int> past = new List<int>();
            for (int i = 0; i < width; i++)
            {

                var pos = (i / (double)width);
                var slice = pos * batch.Slices.Count;

                if (!past.Contains((int)slice))
                {
                    past.Add((int)slice);
                    if (PreNormalizingAction != null)
                    {
                        PreNormalizingAction(batch.Slices[(int)slice]);
                    }
                }
                if (adaptative)
                {

                    var start = Math.Max(0, slice - windowSize);
                    var take = Math.Max(slice - start, 1);
                    batch.Max = batch.Slices.Skip((int)start).Take((int)take).Max(x => x.Maximum) * upperKoef;
                }
                ss.Add(batch.GetInterpolatedSlice(pos, FilterDb).Take(N).ToArray());
            }

            LastCalculation = ss.ToList();



            Bitmap bmp = new Bitmap(ss.Count(), height + 1);

            var max = ss.Max(x => x.Max());
            max = Math.Min(max, 0);
            var min = ss.Min(x => x.Min());

            var diap = max - min;
            var fdiap = ntop - ndown;
            fdiap = ss[0].Count();
            var kx = height / (double)(fdiap);//ss[0].Count();
            //kx = height / (double)(ss[0].Count());

            //            RTPeekExtractor.MusicHalls

            double[,] dd = new double[ss.Count, fdiap];

            for (int i = 0; i < ss.Count(); i++)
            {
                if (LocalNormalizeSpectrum)
                {
                    max = ss[i].Max();
                    min = ss[i].Min();

                    diap = max - min;
                }



                for (int j = 0; j < ss[i].Count(); j++)
                {

                    var tt = ((ss[i][j] - min) / diap);

                    if (ScaleType == ScaleTypeEnum.Logarithmic)
                    {
                        var maxf = (ss[i].Count()) * ((double)batch.SampleRate / batch.SpectrumLen);
                        var p1 = (j) / (double)(ss[0].Count());
                        var t1 = (108 * p1) - 42.0;
                        var f2 = (int)(440.0 * Math.Pow(2, t1 / 12.0));
                        var p2 = f2 / (double)maxf;
                        var t2 = (int)(p2 * ss[i].Count());
                        tt = ((ss[i][t2] - min) / diap);


                    }

                    if (j >= ndown && j < ntop)
                    {
                        dd[i, j - ndown] = tt;

                    }


                }
            }

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    double val = 0;
                    double position = j / (double)height;
                    //  if (kx > 1.0)
                    {

                        var pp = (int)(fdiap * position);
                        var pp2 = pp + 1;

                        double diapp = 1.0 / fdiap;
                        position %= diapp;
                        position /= diapp;

                        var rMax = dd[i, pp];

                        if (pp2 > dd.GetLength(1) - 1)
                        {
                            pp2 = pp;
                        }
                        var rMin = dd[i, pp2];

                        var aver = rMin + (int)((rMax - rMin) * position);
                        val = aver;


                    }
                    //   else
                    {
                        //val = dd[i, (int)(j / kx)];
                    }
                    val = Math.Min(val, 1.0);
                    bmp.SetPixel(i, height - j, GetColor(val));
                }
            }


            return bmp;
        }



        //public static double FilterDb = -190;
        public static double FilterDb = -60;
        public static bool NormalziedSpectrum { get; set; }
    }
}
