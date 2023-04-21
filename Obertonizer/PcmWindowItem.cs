using System.Numerics;

namespace Obertonizer
{
    public class PcmWindowItem
    {
        public float[] Data;

        public static float[] HammingKoef;
        public static float[] BlackmanKoef;

        public Koef[] Koefs;

        public double[] TriangleFilter(MelTriangleFilter[] filters, int sampleRate)
        {
            double[] ret = new double[filters.Length];
            for (int i = 0; i < KoefsComplexs.Length; i++)
            {
                for (int j = 0; j < filters.Length; j++)
                {
                    double freq = i * sampleRate / KoefsComplexs.Length;

                    if (filters[j].Start <= freq && filters[j].Finish >= freq)
                    {
                        double measure = (freq - filters[j].Start) / (filters[j].Peak - filters[j].Start);
                        if (freq > filters[j].Peak)
                        {
                            measure = 2.0 - measure;
                        }

                        //double measure = 1.0;
                        ret[j] += measure * KoefsComplexs[i].Magnitude;
                    }
                }
            }
            return ret;
        }

        public static void InitHamming(int N) 
        {
            HammingKoef = new float[N];
            for (int i = 0; i < N; i++)
            {
                HammingKoef[i] = (float)(0.53836 - 0.46164 * Math.Cos((Math.PI * 2 * i) / (N - 1)));
            }
        }
        public static float[] BlackmanWindow(int N)
        {
            BlackmanKoef = new float[N];
            float a = 0.16f;
            float a0 = (1.0f - a) / 2;
            float a1 = (1.0f) / 2;
            float a2 = (a) / 2;

            for (int i = 0; i < N; i++)
            {
                BlackmanKoef[i] = (float)(a0 - a1 * Math.Cos((Math.PI * 2 * i) / (N - 1)) + a2 * Math.Cos((Math.PI * 4 * i) / (N - 1)));
            }
            return BlackmanKoef.ToArray();
        }

        public static float[] HammingWindow(int N)
        {
            HammingKoef = new float[N];
            for (int i = 0; i < N; i++)
            {
                HammingKoef[i] = (float)(0.53836 - 0.46164 * Math.Cos((Math.PI * 2 * i) / (N - 1)));
            }
            return HammingKoef.ToArray();
        }

        public void Hamming()
        {
            if (HammingKoef == null)
            {
                InitHamming(1024); //128ms wing 44,1kHz sample\rate
            }

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = Data[i] * HammingKoef[i];
            }
        }

        public Complex[] KoefsComplexs;
        //discreet fourier transfrom
        public void DFT(int sampleRate = 44100)
        {

            Complex[] kk = new Complex[Data.Length];
            for (int i = 0; i < Data.Length; i++)
            {
                kk[i] = Data[i];
            }

            List<Complex> rr = new List<Complex>(FFT.fft(kk));

            KoefsComplexs = rr.GetRange(0, rr.Count / 2).ToArray();
        }

        public double[] FilterEnergies;
        public void MellTransform(int sampleRate = 44100)
        {


        }

        private double[] preMFCC;

        public double[] MFCC
        {
            get { return preMFCC.ToList().GetRange(0, 13).ToArray(); }
        }

        public void DCTTransform()
        {
            preMFCC = new double[FilterEnergies.Length];
            for (int i = 0; i < FilterEnergies.Length; i++)
            {
                for (int j = 0; j < FilterEnergies.Length; j++)
                {
                    preMFCC[i] += Math.Log(Math.Pow(FilterEnergies[j], 2)) * Math.Cos(i * (j - 0.5) * Math.PI / FilterEnergies.Length);
                }
            }
        }
    }
}