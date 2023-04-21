namespace Obertonizer
{
    public class SpectrumSlicerParam
    {
        public SpectrumSlicerParam()
        {

        }


        public IPipelineItem[] Preprocess;
        public IPipelineItem[] Postprocess;

        public SpectrumSlicerParam(SpectrumSlicerParam p)
        {
            Wave = p.Wave;
            Overlapkoef = p.Overlapkoef;

            SpectrumLen = p.SpectrumLen;
            Overlap = p.Overlap;
            SampleRate = p.SampleRate;
            FullSize = p.FullSize;
            OverlapInterval = p.OverlapInterval;

            UseParallel = p.UseParallel;
            FullCalc = p.FullCalc;
            Preprocess = p.Preprocess;
            Postprocess = p.Postprocess;
        }

        public bool FullCalc = false;
        public bool UseParallel = false;

        public PcmWave Wave;
        
        public double Overlapkoef;
        
        public double OverlapInterval;
        public OverlapTypeEnum Overlap;

        public bool FullSize = false;


        public int SampleRate = 44100;
        public int SpectrumLen = 2048;
    }
}
