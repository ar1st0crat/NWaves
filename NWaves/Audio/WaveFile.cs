using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NWaves.Audio.Interfaces;
using NWaves.Signals;

namespace NWaves.Audio
{
    /// <summary>
    /// WAV file container
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
        /// Supported bit depths
        /// </summary>
        public short[] SupportedBitDepths = { 8, 16, 24, 32 };

        /// <summary>
        /// This constructor loads signals from a wave file.
        /// 
        /// Since NWaves is Portable library, there's no universal FileStream class.
        /// So it's supposed that the client code will take care 
        /// for extracting the stream from a wave file.
        /// 
        /// </summary>
        /// <param name="waveStream">Input stream</param>
        /// <param name="normalized">Normalization flag</param>
        /// <returns></returns>
        /// <exception>Possible null exception</exception>
        public WaveFile(Stream waveStream, bool normalized = true)
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

                // there may be some wavefile meta info here,
                // so try to find "data" header in the file:

                var dataPosition = reader.BaseStream.Position;
                while (dataPosition != reader.BaseStream.Length - 1)
                {
                    reader.BaseStream.Position = dataPosition;
                    var dataId = reader.ReadInt32();
                    if (dataId == 0x61746164)
                    {
                        break;
                    }
                    dataPosition++;
                }

                if (dataPosition == reader.BaseStream.Length - 1)
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

                switch (waveFmt.BitsPerSample)
                {
                    case 8:
                    {
                        for (var i = 0; i < length; i++)
                        {
                            for (var j = 0; j < waveFmt.ChannelCount; j++)
                            {
                                Signals[j][i] = reader.ReadByte() - 128;
                                if (normalized) Signals[j][i] /= 128;
                            }
                        }
                        break;
                    }

                    case 16:
                    {
                        for (var i = 0; i < length; i++)
                        {
                            for (var j = 0; j < waveFmt.ChannelCount; j++)
                            {
                                Signals[j][i] = reader.ReadInt16();
                                if (normalized) Signals[j][i] /= 32768;
                            }
                        }
                        break;
                    }

                    case 32:
                    {
                        for (var i = 0; i < length; i++)
                        {
                            for (var j = 0; j < waveFmt.ChannelCount; j++)
                            {
                                Signals[j][i] = reader.ReadInt32();
                                if (normalized) Signals[j][i] /= 2147483648;
                            }
                        }
                        break;
                    }

                    case 24:
                    {
                        for (var i = 0; i < length; i++)
                        {
                            for (var j = 0; j < waveFmt.ChannelCount; j++)
                            {
                                var b1 = reader.ReadByte();
                                var b2 = reader.ReadByte();
                                var b3 = reader.ReadByte();

                                Signals[j][i] = (b1 << 8 | b2 << 16 | b3 << 24);
                                if (normalized) Signals[j][i] /= 2147483648;
                            }
                        }
                        break;
                    }

                    default:
                        throw new ArgumentException(
                            "Wrong bit depth! Supported values are: " + string.Join(", ", SupportedBitDepths));
                }
            }
        }

        /// <summary>
        /// This constructor loads signals into container.
        /// </summary>
        /// <param name="signals">Signals to be loaded into container</param>
        /// <param name="bitsPerSample">Bit depth</param>
        public WaveFile(IList<DiscreteSignal> signals, short bitsPerSample = 16)
        {
            if (signals == null || !signals.Any())
            {
                throw new ArgumentException("At least one signal must be provided");
            }

            var samplingRate = signals[0].SamplingRate;
            if (signals.Any(s => s.SamplingRate != samplingRate))
            {
                throw new ArgumentException("Signals must be sampled at the same sampling rate");
            }

            var length = signals[0].Length;
            if (signals.Any(s => s.Length != length))
            {
                throw new ArgumentException("Signals must have the same length");
            }
            
            if (!SupportedBitDepths.Contains(bitsPerSample))
            {
                throw new ArgumentException(
                            "Wrong bit depth! Supported values are: " + string.Join(", ", SupportedBitDepths));
            }
            
            WaveFormat waveFmt;
            waveFmt.AudioFormat = 1;                        // PCM
            waveFmt.ChannelCount = (short)signals.Count;    // mono
            waveFmt.BitsPerSample = bitsPerSample;          // 8, 16, 24 or 32

            waveFmt.Align = (short)(waveFmt.ChannelCount * waveFmt.BitsPerSample / 8);
            waveFmt.SamplingRate = samplingRate;
            waveFmt.ByteRate = waveFmt.SamplingRate * waveFmt.ChannelCount * waveFmt.BitsPerSample / 8;

            WaveFmt = waveFmt;

            Signals = signals.ToList();
        }

        /// <summary>
        /// This constructor loads one signal into container.
        /// </summary>
        /// <param name="signal">Signal to be loaded into container</param>
        /// <param name="bitsPerSample">Bit depth</param>
        public WaveFile(DiscreteSignal signal, short bitsPerSample = 16) : this(new [] { signal }, bitsPerSample)
        {
        }

        /// <summary>
        /// Method saves the contents of a wave file to stream.
        /// </summary>
        /// <param name="waveStream">Output stream for saving</param>
        /// <param name="normalized">Normalization flag</param>
        public void SaveTo(Stream waveStream, bool normalized = true)
        {
            using (var writer = new BinaryWriter(waveStream))
            {
                var length = Signals[0].Length;

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

                switch (WaveFmt.BitsPerSample)
                {
                    case 8:
                    {
                        for (var i = 0; i < length; i++)
                        {
                            for (var j = 0; j < WaveFmt.ChannelCount; j++)
                            {
                                var sample = normalized ? Signals[j][i] * 128 + 128 : Signals[j][i];
                                writer.Write((sbyte) sample);
                            }
                        }
                        break;
                    }

                    case 16:
                    {
                        for (var i = 0; i < length; i++)
                        {
                            for (var j = 0; j < WaveFmt.ChannelCount; j++)
                            {
                                var sample = normalized ? Signals[j][i] * 32768 : Signals[j][i];
                                writer.Write((short) sample);
                            }
                        }
                        break;
                    }

                    case 32:
                    {
                        for (var i = 0; i < length; i++)
                        {
                            for (var j = 0; j < WaveFmt.ChannelCount; j++)
                            {
                                var sample = normalized ? Signals[j][i] * 2147483648 : Signals[j][i];
                                writer.Write((int) sample);
                            }
                        }
                        break;
                    }

                    case 24:
                    {
                        for (var i = 0; i < length; i++)
                        {
                            for (var j = 0; j < WaveFmt.ChannelCount; j++)
                            {
                                var sample = normalized ? Signals[j][i] * 2147483648 : Signals[j][i];
                                var s = (int) sample;

                                var b = (byte)(s >> 8);  writer.Write(b);
                                    b = (byte)(s >> 16); writer.Write(b);
                                    b = (byte)(s >> 24); writer.Write(b);
                            }
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Fancy indexer:
        /// 
        ///     waveFile[Channels.Left] -> waveFile.Signals[0]
        ///     waveFile[Channels.Right] -> waveFile.Signals[1]
        ///     waveFile[Channels.Average] -> returns channel-averaged (new) signal
        ///     waveFile[Channels.Interleave] -> returns interleaved (new) signal
        /// 
        /// </summary>
        /// <param name="channel">Channel enum</param>
        /// <returns>Signal from the channel or interleaved signal</returns>
        public DiscreteSignal this[Channels channel]
        {
            get
            {
                if (channel != Channels.Interleave && channel != Channels.Average)
                {
                    return Signals[(int)channel];
                }

                // in case of averaging or interleaving first check if our signal is mono

                if (WaveFmt.ChannelCount == 1)
                {
                    return Signals[0];
                }

                var length = Signals[0].Length;

                // 1) AVERAGING

                if (channel == Channels.Average)
                {
                    var avgSamples = new float [length];

                    for (var i = 0; i < avgSamples.Length; i++)
                    {
                        for (var j = 0; j < Signals.Count; j++)
                        {
                            avgSamples[i] += Signals[j][i];
                        }
                        avgSamples[i] /= Signals.Count;
                    }

                    return new DiscreteSignal(WaveFmt.SamplingRate, avgSamples);
                }

                // 2) if it ain't mono, we start ACTUALLY interleaving:
                
                var samples = new float[WaveFmt.ChannelCount * length];

                var idx = 0;
                for (var i = 0; i < length; i++)
                {
                    for (var j = 0; j < WaveFmt.ChannelCount; j++)
                    {
                        samples[idx++] = Signals[j][i];
                    }
                }

                return new DiscreteSignal(WaveFmt.SamplingRate, samples);
            }
        }
    }
}
