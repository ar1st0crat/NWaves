using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NWaves.Audio.Interfaces;
using NWaves.Signals;

namespace NWaves.Audio
{
    /// <summary>
    /// WAV file container.
    /// Supports only 8bits/sec and 16bits/sec formats.
    /// </summary>
    public class WaveFile : IAudioContainer
    {
        /// <summary>
        /// Signals from all channels.
        /// Usually:
        /// 
        ///     Signals.Count = 1 (mono)
        /// or
        ///     Signals.Count = 2 (stereo)
        /// 
        /// </summary>
        public List<DiscreteSignal> Signals { get; }

        /// <summary>
        /// Wav header struct
        /// </summary>
        public WaveFormat WaveFmt { get; }

        /// <summary>
        /// This constructor loads signals from a wave file.
        /// 
        /// Since NWaves is Portable library, there's no universal FileStream class.
        /// So it's supposed that the client code will take care 
        /// for extracting the stream from a wave file.
        /// 
        /// </summary>
        /// <param name="waveStream"></param>
        /// <returns></returns>
        /// <exception>Possible null exception</exception>
        public WaveFile(Stream waveStream)
        {
            using (var reader = new BinaryReader(waveStream))
            {
                if (reader.ReadInt32() != 0x46464952)     // "RIFF"
                {
                    throw new FormatException("NOT RIFF!");
                }

                // ignore file size
                reader.ReadInt32();

                if (reader.ReadInt32() != 0x45564157)     // "WAVE"
                {
                    throw new FormatException("NOT WAVE!");
                }

                // try to find "fmt " header in the file:

                var fmtPosition = reader.BaseStream.Position;
                while (fmtPosition != reader.BaseStream.Length - 1)
                {
                    reader.BaseStream.Position = fmtPosition;
                    var fmtId = reader.ReadInt32();
                    if (fmtId == 0x20746D66)
                    {
                        break;
                    }
                    fmtPosition++;
                }

                if (fmtPosition == reader.BaseStream.Length - 1)
                {
                    throw new FormatException("NOT fmt !");
                }

                var fmtSize = reader.ReadInt32();

                WaveFormat waveFmt;
                waveFmt.AudioFormat = reader.ReadInt16();
                waveFmt.ChannelCount = reader.ReadInt16();
                waveFmt.SamplingRate = reader.ReadInt32();
                waveFmt.ByteRate = reader.ReadInt32();
                waveFmt.Align = reader.ReadInt16();
                waveFmt.BitsPerSample = reader.ReadInt16();

                WaveFmt = waveFmt;

                if (fmtSize == 18)
                {
                    var fmtExtraSize = reader.ReadInt16();
                    reader.ReadBytes(fmtExtraSize);
                }

                if (reader.ReadInt32() != 0x61746164)      // "data"
                {
                    throw new FormatException("NOT data!");
                }

                var length = reader.ReadInt32();

                length /= waveFmt.ChannelCount;
                length /= (waveFmt.BitsPerSample / 8);

                Signals = new List<DiscreteSignal>();

                for (var i = 0; i < waveFmt.ChannelCount; i++)
                {
                    Signals.Add(new DiscreteSignal(waveFmt.SamplingRate, length));
                }

                if (waveFmt.BitsPerSample == 8)
                {
                    for (var i = 0; i < length; i++)
                    {
                        for (var j = 0; j < waveFmt.ChannelCount; j++)
                        {
                            Signals[j][i] = (reader.ReadByte() - 128) / 128.0;
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < length; i++)
                    {
                        for (var j = 0; j < waveFmt.ChannelCount; j++)
                        {
                            Signals[j][i] = reader.ReadInt16() / (double)short.MaxValue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This constructor loads signal into container.
        /// </summary>
        /// <param name="signal">Signal to be loaded into container</param>
        public WaveFile(DiscreteSignal signal)
        {
            WaveFormat waveFmt;
            waveFmt.AudioFormat = 1;        // PCM
            waveFmt.ChannelCount = 1;       // mono
            waveFmt.BitsPerSample = 16;     // 16 bits/sample

            waveFmt.Align = (short)(waveFmt.ChannelCount * waveFmt.BitsPerSample / 8);
            waveFmt.SamplingRate = signal.SamplingRate;
            waveFmt.ByteRate = waveFmt.SamplingRate * waveFmt.ChannelCount * waveFmt.BitsPerSample / 8;

            WaveFmt = waveFmt;

            Signals = new List<DiscreteSignal> { signal };
        }

        /// <summary>
        /// Method saves the contents of a wave file to stream.
        /// </summary>
        /// <param name="waveStream">Output stream for saving</param>
        public void SaveTo(Stream waveStream)
        {
            using (var writer = new BinaryWriter(waveStream))
            {
                var length = Signals[0].Samples.Length;

                writer.Write(0x46464952);     // "RIFF"
                
                var dataSize = length * WaveFmt.ChannelCount * WaveFmt.BitsPerSample / 8;

                var fileSize = 36 + dataSize;
                writer.Write(fileSize);

                writer.Write(0x45564157);     // "WAVE"
                writer.Write(0x20746D66);     // "fmt "
                writer.Write(16);             // fmtSize = 16 for PCM

                writer.Write(WaveFmt.AudioFormat);
                writer.Write(WaveFmt.ChannelCount);
                writer.Write(WaveFmt.SamplingRate);
                writer.Write(WaveFmt.ByteRate);
                writer.Write(WaveFmt.Align);
                writer.Write(WaveFmt.BitsPerSample);
                
                writer.Write(0x61746164);      // "data"
                writer.Write(dataSize);

                var maxValue = Math.Abs(Signals.SelectMany(s => s.Samples.Select(x => x)).Max());
                maxValue = Math.Max(1.0, maxValue);

                if (WaveFmt.BitsPerSample == 8)
                {
                    for (var i = 0; i < length; i++)
                    {
                        for (var j = 0; j < WaveFmt.ChannelCount; j++)
                        {
                            writer.Write((sbyte)((Signals[j][i] * 128 + 128) / maxValue));
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < length; i++)
                    {
                        for (var j = 0; j < WaveFmt.ChannelCount; j++)
                        {
                            writer.Write((short)((Signals[j][i] * short.MaxValue) / maxValue));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fancy indexer:
        /// 
        ///     waveFile[Channels.Left] -> waveFile.Signals[0]
        ///     waveFile[Channels.Right] -> waveFile.Signals[1]
        ///     waveFile[Channels.Interleave] -> returns interleaved signal
        /// 
        /// </summary>
        /// <param name="channel">Channel enum</param>
        /// <returns>Signal from the channel or interleaved signal</returns>
        public DiscreteSignal this[Channels channel]
        {
            get
            {
                if (channel != Channels.Interleave)
                {
                    return Signals[(int)channel];
                }

                // in case of interleaving first check if our signal is mono

                if (WaveFmt.ChannelCount == 1)
                {
                    return Signals[0];
                }

                // if it ain't mono, we start ACTUALLY interleaving:

                var length = Signals[0].Samples.Length;

                var samples = new double[WaveFmt.ChannelCount * length];

                var idx = 0;
                for (var i = 0; i < length; i++)
                {
                    for (var j = 0; j < WaveFmt.ChannelCount; j++)
                    {
                        samples[idx++] = Signals[j][i];
                    }
                }

                return new DiscreteSignal(samples, WaveFmt.SamplingRate);
            }
        }
    }
}
