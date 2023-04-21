namespace Obertonizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //ObertoneRoutine.LocalNormalizeSpectrum = true;            
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadFileCommand(ofd.FileName);
            }
        }
        public void LoadFileCommand(string fileName)
        {
            file = fileName;
            Text = "File: " + fileName;
            fullupdate();
            update1();
        }
        public void update1()
        {
            double len = (float)sp.Param.Wave.Count / sp.Param.SampleRate;
            var img = ObertoneRoutine.Get(sp, (int)(len * 1000), pictureBox1.Height, DownFreq, TopFreq,
                false);

            curOrigSpectrum = img;
            updateTimeMarkerLine();

        }

        private int TopFreq = (int)16e3;
        private int DownFreq = (int)0;
        double drawkx = 5;
        public double TimeMarker = 0;
        public bool DrawCuts = false;

        public void updateTimeMarkerLine()
        {
            if (curOrigSpectrum != null)
            {
                var ff = (Bitmap)curOrigSpectrum.Clone();
                var gr = Graphics.FromImage(ff);
                gr.DrawLine(Pens.GreenYellow, (int)(TimeMarker * ff.Width), 0, (int)(TimeMarker * ff.Width), ff.Height - 1);

                if (DrawCuts)
                {
                    var cuts = ObertoneRoutine.CalcSegmentsUsengBaseToneQuantiz(sp, 8);

                    foreach (var cut in cuts)
                    {
                        var cc = cut / (decimal)sp.Slices.Count;
                        gr.DrawLine(Pens.Gold, (int)(cc * ff.Width), 0, (int)(cc * ff.Width), ff.Height - 1);

                    }
                }


                var w = (int)(SpectrumVerticalPosition * pictureBox1.Height);

                gr.DrawLine(new Pen(Color.DarkOrange, 3), 0, w, pictureBox1.Width, w);
                gr.FillRectangle(Brushes.Blue, 5, w - 22, 100, 20);

                var yy = 1.0 - (SpectrumVerticalPosition);


                var freq = (yy * TopFreq);


                if (ObertoneRoutine.ScaleType == ScaleTypeEnum.Logarithmic)
                {
                    var t1 = (108 * yy) - 42.0;
                    var f2 = (int)(440.0 * Math.Pow(2, t1 / 12.0));
                    freq = f2;
                }

                gr.DrawString(freq.ToString("f") + " Hz", new Font("Arial", 12), Brushes.White, 7, w - 22 + 1 + 3);
                //gr.DrawString();

                if (Map != null)
                {

                    DrawOverBitmap(Map, gr, ff.Width, ff.Height);
                }
                pictureBox1.Image = ff;

            }


        }

        public void DrawOverBitmap(int[,] map, Graphics gr, int width, int height)
        {

            double kx = width / (double)map.GetLength(0);
            double ky = height / (double)map.GetLength(1);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int nx = (int)(i / kx);
                    int ny = (int)(j / ky);

                    if (map[nx, ny] > 0)
                    {

                        gr.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), i, height - j, 1, 1);
                    }
                    else
                    {
                        //gr.FillRectangle(Brushes.Blue, i, j, 1, 1);

                    }
                }
            }


        }
        public double FilePosition = 0;
        public double SpectrumVerticalPosition = 0;
        private int[,] Map;
        private Bitmap curOrigSpectrum;
        private SpectrumBatch sp;

        private int splen = 2048;
        private double tmresol = 5;
        public SpectrumBatch GenerateBatch(string _file)
        {
            //SpectrumBatch.UseParallel = true;
            var sp2 = new SpectrumBatch(_file, new SpectrumSlicerParam()
            {
                OverlapInterval = tmresol,
                SpectrumLen = splen,
                Overlap = OverlapTypeEnum.TimeInterval,
                FullSize = false,
                FullCalc = false,
                UseParallel = false
                ,
                Preprocess = new IPipelineItem[] { new PcmWavePreEmphasisPipilineItem() {
                    Enabled = true },
                    new PcmWaveBlackmanWindowRawTransformPipilineItem() { Enabled = false }, }
             ,
                Postprocess = new IPipelineItem[]
                {
                    FreqFilter, EqulizerFilter,

                    new SpectrumEqualizer() {
                        Enabled = false,
                        LowFreq = 2800, HiFreq = 12000,
                        Intensity = 10, SampleRate = 44100 }


                }
            });




            return sp2;
        }
        public SpectrumEqualizer EqulizerFilter = new SpectrumEqualizer()
        {
            Enabled = false,
            LowFreq = 1800,
            HiFreq = 12000,
            Intensity = 10,
            SampleRate = 44100
        };

        public SpectrumFreqFilter FreqFilter = new SpectrumFreqFilter()
        {
            Enabled = false,
            HiFreq = 8000,
            LowFreq = 800,
            SampleRate = 44100
        };
        private string file = "";

        public void fullupdate()
        {

            sp = GenerateBatch(file);


        }
    }
}
