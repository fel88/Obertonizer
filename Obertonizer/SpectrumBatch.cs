using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;

namespace Obertonizer
{
    public class SpectrumBatch
    {
        #region Constructors

        public SpectrumBatch()
        {


        }

        public void Init(IPipelineItem[] preprocess, IPipelineItem[] postprocess)
        {
            PreprocessPipeline.Clear();
            foreach (var pipelineItem in preprocess)
            {
                PreprocessPipeline.AddItem(pipelineItem);
            }

            PostprocessPipeline.Clear();
            foreach (var pipelineItem in postprocess)
            {
                PostprocessPipeline.AddItem(pipelineItem);
            }
        }


        public double Max;
        public double Min;

        public double[] GetSingleSlice(SpectrumSlice slice, double filterdb, ScaleTypeEnum scale, bool localMaximum = false)
        {


            var max = Max;

            if (localMaximum)
            {
                max = slice.Maximum;
            }
            if (scale == ScaleTypeEnum.Logarithmic)
            {
                return slice.GetDbGraph(filterdb, max);
            }

            return slice.GetLinearGraph(filterdb, max);

        }

        public double[] GetInterpolatedSlice(double position, double filterdb, bool localMaximum = false)
        {
            var pp = (int)(Slices.Count * position);
            var pp2 = pp + 1;

            var max = Max;

            if (pp < 0)
            {
                if (localMaximum)
                {
                    max = Slices[0].Maximum;
                }
                return Slices[0].GetDbGraph(filterdb, max);
            }

            if (pp > (Slices.Count - 1) || pp2 > (Slices.Count - 1))
            {
                if (localMaximum)
                {
                    max = Slices.Last().Maximum;
                }
                return Slices.Last().GetDbGraph(filterdb, max);
            }

            double diap = 1.0 / Slices.Count;
            position %= diap;
            position /= diap;

            if (localMaximum)
            {
                max = Math.Max(Slices[pp].Maximum, Slices[pp2].Maximum);
            }

            return SpectrumSlice.Interpolate(Slices[pp], Slices[pp2], filterdb, position, max);
        }


        public object Tag;
        public SpectrumSlice Full;
        //public PcmWave Wave;


        public SpectrumSlicerParam Param;

        public SpectrumBatch(SpectrumSlicerParam param)
        {
            Init(param.Preprocess, param.Postprocess);
            Param = param;




            var wav = param.Wave;

            List<Int16> lst = new List<short>();

            lst.AddRange(Enumerable.Range(0, (int)(param.SpectrumLen * 0.7f)).Select(x => (Int16)0).ToArray());
            lst.AddRange(wav.Peeks);
            lst.AddRange(Enumerable.Range(0, (int)(param.SpectrumLen * 0.7f)).Select(x => (Int16)0).ToArray());

            param.Wave.Peeks = lst.ToArray();

            var max = wav.Peeks.Max();
            if (param.UseParallel)
            {
                //GetSpectrumsSlicesParallel(/*wav, param.FullSize ? (int)(wav.Count - 1) : param.SpectrumSize, param.Overlap*/);
            }
            else
            {
                GetSpectrumsSlices(/*wav, param.FullSize ? (int)(wav.Count - 1) : param.SpectrumSize, param.Overlap*/);
            }

            if (param.FullCalc)
            {

                //Full = FullLenSpectrum(wav, (int)(wav.Count - 1));
                //Full = FullLenSpectrum(wav, param.SpectrumLen);     
            }
        }


        public SpectrumBatch(string path, SpectrumSlicerParam param)
            : this(new SpectrumSlicerParam(param) { Wave = PcmWave.FromFile(path, 2) })
        {


        }

        public int SampleRate
        {
            get
            {
                return Param.SampleRate;
            }
        }

        public int SpectrumLen
        {
            get
            {
                return Param.SpectrumLen;
            }
        }


        #endregion

        public void NormalizePower(double logLevel)
        {

            double mul = 0.0;




            while (Math.Abs(mul - 1.0) > 0.05)
            {
                var sumt = Math.Sqrt(Slices.Sum(x => x.Spectrum.Sum()));
                mul = logLevel / sumt;


                foreach (var slice in Slices)
                {

                    for (int i = 0; i < slice.Spectrum.Count(); i++)
                    {
                        slice.Spectrum[i] *= mul;
                    }
                }
            }

        }

        public void Save(string path)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(new FileStream(path, FileMode.Create), this);
        }

        public void CutSilience(double logEnergyTreshold = 47.5)
        {

            List<float> curve = new List<float>();

            int sti = 0;

            var maxen = Slices.Max(x => x.GetEnergy());
            var avgen = Slices.Average(x => x.GetEnergy());
            var minen = Slices.Min(x => x.GetEnergy());

            logEnergyTreshold = ((avgen - minen) / 3) + minen;

            for (int i = 1; i < Slices.Count; i++)
            {
                var ene = Slices[i].GetEnergy();

                if (ene > logEnergyTreshold)
                //if (ene > avgen)
                {
                    sti = i - 1;
                    break;
                }
            }

            int eni = 0;
            for (int i = Slices.Count - 2; i >= 0; i--)
            {
                var ene = Slices[i].GetEnergy();
                if (ene > logEnergyTreshold)
                //if (ene > avgen)
                {
                    eni = i + 1;
                    break;
                }
            }

            while (Slices.Count > eni)
            {
                Slices.RemoveAt(eni);
            }
            Slices.RemoveRange(0, sti);


        }

        public static SpectrumBatch Restore(string path)
        {
            BinaryFormatter bf = new BinaryFormatter();
            return (SpectrumBatch)bf.Deserialize(new FileStream(path, FileMode.Open));
        }

        #region Fields
        public List<SpectrumSlice> Slices = new List<SpectrumSlice>();
        public List<float> _currentWave = new List<float>();
        #endregion


        public void AddSpectrumsSlices(float[] wave, int sliceNum, double overlapKoef = 1.0)
        {
            int begin = 0;
            while (begin <= (wave.Length - sliceNum))
            {


                _currentWave = wave.Skip(begin).Take(sliceNum).ToList();
                begin += (int)(sliceNum * Param.Overlapkoef);



                _currentWave = WindowedPreProcessing(_currentWave.ToArray()).ToList();

                var fftdata = _currentWave.Select(x => new Complex(x, 0)).ToList();
                while (Math.Log(fftdata.Count(), 2) % 1.0f > 0)
                {
                    fftdata.Add(new Complex(0, 0));
                }

                var ret = FFT.fft(fftdata.ToArray());

                Slices.Add(new SpectrumSlice() { Spectrum = ret.Select(x => x.Magnitude).ToArray() });
            }
        }

        //  public double OverlapKoef = 1.0 / 3;






        public void GetSpectrumsSlices()
        {
            int sliceNum = Param.FullSize ? (int)(Param.Wave.Count - 1) : Param.SpectrumLen;

            int begin = 0;
            var full = Param.Wave.Subwave(begin, (int)Param.Wave.Count).Counts.Select(x => (float)x).ToList();
            while (begin < (Param.Wave.Count - sliceNum))
            {
                //TimeProfiler rp = new TimeProfiler();

                //  rp.Tick("start");
                //if (Slices.Count > 2500) break;
                _currentWave = full.GetRange(begin, sliceNum);

                //_currentWave = wave.Subwave(begin, sliceNum).Counts.Select(x => (float)x).ToList();
                //begin += sliceNum;
                switch (Param.Overlap)
                {
                    case OverlapTypeEnum.Koef:
                        begin += (int)(sliceNum * Param.Overlapkoef);
                        break;
                    case OverlapTypeEnum.None:
                        begin += sliceNum;
                        break;
                    case OverlapTypeEnum.TimeInterval:
                        begin += Convert.ToInt32(Math.Round(((double)Param.SampleRate / 1000.0) * Param.OverlapInterval));
                        break;
                }
                //  rp.Tick("r1");

                //begin += overlap ? (int)(sliceNum * OverlapKoef) : sliceNum;
                _currentWave = WindowedPreProcessing(_currentWave.ToArray()).ToList();

                //  rp.Tick("r2");

                var fftdata = _currentWave.Select(x => new AForge.Math.Complex(x, 0)).ToList();
                //var fftdata2 = _currentWave.Select(x => new Complex(x, 0)).ToList();
                while (Math.Log(fftdata.Count(), 2) % 1.0f > 0)
                {
                    fftdata.Add(new AForge.Math.Complex(0, 0));
                }

                //var ret = FFT.fft(fftdata.ToArray());
                var arr = fftdata.ToArray();
                AForge.Math.FourierTransform.FFT(arr, AForge.Math.FourierTransform.Direction.Forward);
                var mgntds = arr.Select(x => x.Magnitude).ToArray();




                mgntds = (double[])PostprocessPipeline.Process(mgntds.ToArray());

                Slices.Add(new SpectrumSlice() { Orig = mgntds.Select(x => (float)x).ToArray(), Spectrum = mgntds, FreqIndex = begin });
                //  rp.Tick("r5");

                var mx = Slices.Last().Spectrum.Max();
                var mn = Slices.Last().Spectrum.Min();
                Slices.Last().SpectrumUshorts = Slices.Last().GetDbGraph(-190, mx).Select(x => (short)(x * 10.0)).ToArray();
                //Slices.Last().Spectrum = null;
                Slices.Last().Maximum = mx;
                Slices.Last().Minimum = mn;
                //rp.Tick("r6");



                // rp.Tick("end");
            }

            Max = Slices.Max(x => x.Maximum);
            Min = Math.Max(1, Slices.Min(x => x.Minimum));
        }
        public Pipeline PreprocessPipeline = new Pipeline();
        public Pipeline PostprocessPipeline = new Pipeline();

        public float[] WindowedPreProcessing(float[] wave)
        {
            var tt = ((float[])PreprocessPipeline.Process(_currentWave.ToArray()));
            return tt.ToArray();
        }



        public void LogScale()
        {
            throw new NotImplementedException();
        }

        public double SnRRatio
        {
            get
            {
                return SpectrumSlice.GetdB(Min, Max);
            }
        }

    }
}