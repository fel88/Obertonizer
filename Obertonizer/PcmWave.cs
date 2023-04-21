using NAudio.Wave;

namespace Obertonizer
{
    public class PcmWave
    {
        public Int16[] Peeks;
        //public sbyte[] Peeks1;

        public decimal Count
        {
            get
            {
                /*if (Peeks1 != null)
                {
                    return Peeks1.Length;
                }*/
                if (Peeks != null)
                {
                    return Peeks.Length;
                }
                return 0;
            }
        }

        public static PcmWave FromArray(byte[] data, int bytePerPeek)
        {
            List<Int16> l = new List<short>();



            for (int i = 0; i < data.Length - 1; i += bytePerPeek)
            {
                Int16 curd = 0;

                for (int j = 0; j < bytePerPeek; j++)
                {
                    curd ^= (short)(data[i + j] << (j * 8));
                }
                //l.Add((Int16)(bb[i] ^ (bb[i + 1] << 8)));
                l.Add(curd);
            }
            return new PcmWave() { Peeks = l.ToArray() };



        }


        public static PcmWave FromFile(string path, int bytePerPeek)
        {
            //TimeProfiler tp = new TimeProfiler();
            var ff = new FileInfo(path).Extension.ToLower();
            byte[] bb = new byte[] { };
            switch (ff)
            {
                case ".raw":
                    bb = File.ReadAllBytes(path);
                    break;
                case ".wav":
                    {
                        WaveFileReader pcm = new WaveFileReader(path);
                        long samplesDesired = pcm.SampleCount;
                        bytePerPeek = pcm.WaveFormat.BitsPerSample / 8;
                        int channels = pcm.WaveFormat.Channels;

                        //if (channels > 1) throw new ArgumentException("more than 1 channel");
                        byte[] buffer = new byte[samplesDesired * channels * bytePerPeek];
                        short[] left = new short[samplesDesired];
                        short[] right = new short[samplesDesired];

                        if ((samplesDesired * bytePerPeek * channels) != pcm.Length)
                        {
                            throw new ArgumentException("length != calc length");
                        }

                        int bytesRead = pcm.Read(buffer, 0, (int)(samplesDesired * bytePerPeek * channels));

                        bb = buffer;
                        int index = 0;
                        for (int sample = 0; sample < bytesRead / 4; sample++)
                        {

                            left[sample] = BitConverter.ToInt16(buffer, index);
                            index += channels;
                            //right[sample] = BitConverter.ToInt16(buffer, index);
                            //index += 2;
                        }

                    }
                    break;
                case ".mp3":
                    {
                        Mp3FileReader pcm = new Mp3FileReader(path);


                        bytePerPeek = pcm.WaveFormat.BitsPerSample / 8;
                        int channels = pcm.WaveFormat.Channels;
                        var sp = pcm.ToSampleProvider();
                        byte[] data = new byte[pcm.Length];
                        //sp.ToMono().ToWaveProvider16().Read(data, 0, data.Length);
                        var samplesDesired = pcm.Length / bytePerPeek;



                        byte[] buffer = new byte[samplesDesired * channels * bytePerPeek];
                        short[] left = new short[samplesDesired];
                        short[] right = new short[samplesDesired];

                        if ((samplesDesired * bytePerPeek * channels) != pcm.Length)
                        {
                            //throw new ArgumentException("length != calc length");
                        }

                        int bytesRead = pcm.Read(buffer, 0, (int)(samplesDesired * bytePerPeek * channels));

                        bb = buffer;
                        int index = 0;
                        for (int sample = 0; sample < bytesRead / 4; sample++)
                        {

                            left[sample] = BitConverter.ToInt16(buffer, index);
                            index += channels;
                            //right[sample] = BitConverter.ToInt16(buffer, index);
                            //index += 2;
                        }

                    }
                    break;

            }
            List<Int16> l = new List<short>();



            for (int i = 0; i < bb.Length - 1; i += bytePerPeek)
            {
                Int16 curd = 0;

                for (int j = 0; j < bytePerPeek; j++)
                {
                    curd ^= (short)(bb[i + j] << (j * 8));
                }
                //l.Add((Int16)(bb[i] ^ (bb[i + 1] << 8)));
                l.Add(curd);
            }
            //tp.Tick("end");
            // var intr = tp.GetTotalInterval();
            return new PcmWave() { Peeks = l.ToArray() };



        }

        public Wave Subwave(int begin, int len)
        {
            List<double> dd = new List<double>();
            for (int i = 0; i < Count; i++)
            {
                /*if (Peeks1 != null)
                {
                    dd.Add(Peeks1[i]);
                }*/
                if (Peeks != null)
                {
                    dd.Add(Peeks[i]);
                }
            }

            Wave ret = new Wave() { Counts = dd.Skip(begin).Take(len).ToArray() };

            return ret;
        }
    }
}
