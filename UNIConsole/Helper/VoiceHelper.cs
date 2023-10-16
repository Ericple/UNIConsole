using System.IO;

namespace UNIConsole.Helper
{
    internal class WaveFormat
    {
        public int SamplePerSecond;
        public short BitsPerSample;
        public short Channels;
        public short BlockAlign;
        public int AverageBytesPerSecond;
    }
    internal class VoiceHelper
    {
        private WaveFormat waveFormat;
        private int SampleCount;
        private string FileName = string.Empty;
        private FileStream WaveFile;
        private BinaryWriter WaveWriter;
        public VoiceHelper(string audioFileName)
        {
            FileName = audioFileName;
            waveFormat = CreateWaveFormat();
        }
        private WaveFormat CreateWaveFormat()
        {
            WaveFormat format = new WaveFormat();
            format.SamplePerSecond = 32000;
            format.BitsPerSample = 16;
            format.Channels = 1;
            format.BlockAlign = (short)(format.Channels * (format.BitsPerSample / 8));
            format.AverageBytesPerSecond = format.BlockAlign * format.SamplePerSecond;
            return format;
        }
        public void RecStop()
        {
            WaveWriter.Seek(4, SeekOrigin.Begin);
            WaveWriter.Write(SampleCount + 36);
            WaveWriter.Seek(40, SeekOrigin.Begin);
            WaveWriter.Write(SampleCount);
            WaveWriter.Close();
            WaveFile.Close();
            WaveWriter = null;
            WaveFile = null;
        }
        public void CreateSoundFile()
        {
            WaveFile = new FileStream(FileName, FileMode.Create);
            WaveWriter = new BinaryWriter(WaveFile);
            var ChunkRiff = new[] { 'R', 'I', 'F', 'F' };
            var ChunkType = new[] { 'W', 'A', 'V', 'E' };
            var ChunkFmt = new[] { 'f', 'm', 't', ' ' };
            var ChunkData = new[] { 'd', 'a', 't', 'a' };
            short shPad = 1;
            int FormatChunkLength = 0x10;
            int Length = 0;
            short shBytesPerSample = 0;
            if(8==waveFormat.BitsPerSample && 1 == waveFormat.Channels)
            {
                shBytesPerSample = 1;
            }else if((8==waveFormat.BitsPerSample && 2== waveFormat.Channels) || (16 == waveFormat.BitsPerSample && 1==waveFormat.Channels))
            {
                shBytesPerSample = 2;
            }else if(16==waveFormat.BitsPerSample && 2 == waveFormat.Channels)
            {
                shBytesPerSample = 4;
            }
            WaveWriter.Write(ChunkRiff);
            WaveWriter.Write(Length);
            WaveWriter.Write(ChunkType);

            WaveWriter.Write(ChunkFmt);
            WaveWriter.Write(FormatChunkLength);
            WaveWriter.Write(shPad);
            WaveWriter.Write(waveFormat.Channels);
            WaveWriter.Write(waveFormat.SamplePerSecond);
            WaveWriter.Write(waveFormat.AverageBytesPerSecond);
            WaveWriter.Write(shBytesPerSample);
            WaveWriter.Write(waveFormat.BitsPerSample);

            WaveWriter.Write(ChunkData);
            WaveWriter.Write(0);
        }
        public void writeData(byte[] data)
        {
            WaveWriter.Write(data,0,data.Length);
            SampleCount += data.Length;
        }
    }
}
