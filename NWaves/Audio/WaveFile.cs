using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NWaves.Audio.Interfaces;
using NWaves.Signals;

namespace NWaves.Audio
{
    /// <summary>
    /// <para>Represents PCM WAV container.</para>
    /// <para>
    /// <see cref="WaveFile"/> is essentially a constructor of signals in memory based on data 
    /// from the WAV stream, and its lifetime is not synchronized with the stream whatsoever. 
    /// <see cref = "WaveFile" /> is not intended to be a wrapper around the stream, or to acquire any resource
    /// (it doesn't affect the underlying stream). The synonym name of this class could be also WaveContainer.
    /// </para>
    /// </summary>
    public class WaveFile : IAudioContainer
    {
        /// <summary>
        /// Gets the list of discrete signals in container.
        /// </summary>
        public List<DiscreteSignal> Signals { get; protected set; }

        /// <summary>
        /// Gets WAV header (WAVE format).
        /// </summary>
        public WaveFormat WaveFmt { get; protected set; }

        /// <summary>
        /// Supported bit depths.
        /// </summary>
        public short[] SupportedBitDepths = { 8, 16, 24, 32 };

        /// <summary>
        /// Constructs WAV container by loading signals from <paramref name="waveStream"/>.
        /// </summary>
        /// <param name="waveStream">Input stream</param>
        /// <param name="normalized">Normalize samples</param>
        public WaveFile(Stream waveStream, bool normalized = true)
        {
            ReadWaveStream(waveStream, normalized);
        }

        /// <summary>
        /// Constructs WAV container by loading signals from a byte array (i.e. byte content of WAV file).
        /// </summary>
        /// <param name="waveBytes">Input array of bytes</param>
        /// <param name="normalized">Normalize samples</param>
        public WaveFile(byte[] waveBytes, bool normalized = true)
        {
            using (var stream = new MemoryStream(waveBytes))
            {
                ReadWaveStream(stream, normalized);
            }
        }

        /// <summary>
        /// Constructs WAV container by loading signals from part of a byte array (i.e. byte content of WAV file).
        /// </summary>
        /// <param name="waveBytes">Input array of bytes</param>
        /// <param name="index">Start position in byte array</param>
        /// <param name="normalized">Normalize samples</param>
        public WaveFile(byte[] waveBytes, int index, bool normalized = true) 
        {
            using (var stream = new MemoryStream(waveBytes, index, waveBytes.Length - index))
            {
                ReadWaveStream(stream, normalized);
            }
        }

        /// <summary>
        /// Reads PCM WAV binary data and fills <see cref="Signals"/> and <see cref="WaveFmt"/> structure.
        /// </summary>
        /// <param name="waveStream">Input stream of PCM WAV binary data</param>
        /// <param name="normalized">Normalize samples</param>
        protected void ReadWaveStream(Stream waveStream, bool normalized = true)
        {
            using (var reader = new BinaryReader(waveStream, Encoding.ASCII, true))
            {
                if (reader.ReadInt32() != 0x46464952)     // "RIFF"
                {
                    throw new FormatException("WAV data error: NOT RIFF!");
                }

                // ignore file size
                reader.ReadInt32();

                if (reader.ReadInt32() != 0x45564157)     // "WAVE"
                {
                    throw new FormatException("WAV data error: NOT WAVE!");
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
                    throw new FormatException("WAV data error: NOT fmt !");
                }

                var fmtSize = reader.ReadInt32();

                WaveFormat waveFmt;
                waveFmt.AudioFormat = reader.ReadInt16();
                waveFmt.ChannelCount = reader.ReadInt16();
                waveFmt.SamplingRate = reader.ReadInt32();
                waveFmt.ByteRate = reader.ReadInt32();
                waveFmt.Align = reader.ReadInt16();
                waveFmt.BitsPerSample = reader.ReadInt16();

                // Header size might not include sizeof extension block.
                if (fmtSize == 18 || fmtSize == 40)
                {
                    var fmtExtraSize = reader.ReadInt16();
                    // Any non-16bit WAV file should include a format extension chunk describing how the data should be interpreted.
                    var fmtUsedBitsPerSample = reader.ReadInt16(); // Number of bits-per-sample actually used of container-specified bits-per-sample
                    var fmtChannelSpeakerMap = reader.ReadInt32(); // Bitmask/flags indicating which channels are included in the file.
                    var fmtSubFormatCode = reader.ReadInt16(); // Similar to container-level format code (1 = PCM, 3 = IEEE, etc.).
                    var fmtSubFormatRemainder = reader.ReadBytes(14); // Remainder of SubFormat GUID.  Usually just "\x00\x00\x00\x00\x10\x00\x80\x00\x00\xAA\x00\x38\x9B\x71".
                    if (waveFmt.AudioFormat == 0xFFFE)
                    {
                        waveFmt.AudioFormat = fmtSubFormatCode;
                    }
                    if (fmtExtraSize > 22)  // Read any leftovers
                    {
                        reader.ReadBytes(fmtExtraSize - 22);
                    }
                }
                
                WaveFmt = waveFmt;

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
                    throw new FormatException("WAV data error: NOT data!");
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
                            if (waveFmt.AudioFormat == 1)
                            {
                                for (var i = 0; i < length; i++)
                                {
                                    for (var j = 0; j < waveFmt.ChannelCount; j++)
                                    {
                                        Signals[j][i] = reader.ReadInt32();
                                        if (normalized) Signals[j][i] /= 2147483648;
                                    }
                                }
                            }
                            else if (waveFmt.AudioFormat == 3)/*IeeeFloat*/
                            {
                                for (var i = 0; i < length; i++)
                                {
                                    for (var j = 0; j < waveFmt.ChannelCount; j++)
                                    {
                                        Signals[j][i] = reader.ReadSingle();
                                    }
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
        /// Constructs WAV container by loading into it collection of <paramref name="signals"/> with given <paramref name="bitsPerSample"/>.
        /// </summary>
        /// <param name="signals">Signals to be loaded into container</param>
        /// <param name="bitsPerSample">Bit depth</param>
        public WaveFile(IList<DiscreteSignal> signals, short bitsPerSample = 16)
        {
            if (signals is null || !signals.Any())
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
            waveFmt.ChannelCount = (short)signals.Count;    // number of channels
            waveFmt.BitsPerSample = bitsPerSample;          // 8, 16, 24 or 32

            waveFmt.Align = (short)(waveFmt.ChannelCount * waveFmt.BitsPerSample / 8);
            waveFmt.SamplingRate = samplingRate;
            waveFmt.ByteRate = waveFmt.SamplingRate * waveFmt.ChannelCount * waveFmt.BitsPerSample / 8;

            WaveFmt = waveFmt;

            Signals = signals.ToList();
        }

        /// <summary>
        /// Constructs WAV container by loading into it one <paramref name="signal"/> with given <paramref name="bitsPerSample"/>.
        /// </summary>
        /// <param name="signal">Signal to be loaded into container</param>
        /// <param name="bitsPerSample">Bit depth</param>
        public WaveFile(DiscreteSignal signal, short bitsPerSample = 16) : this(new [] { signal }, bitsPerSample)
        {
        }

        /// <summary>
        /// Returns the contents of PCM WAV container as array of bytes.
        /// </summary>
        /// <param name="normalized">True if samples are normalized</param>
        public byte[] GetBytes(bool normalized = true)
        {
            using (var stream = new MemoryStream())
            {
                SaveTo(stream, normalized);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Saves the contents of PCM WAV container to <paramref name="waveStream"/>.
        /// </summary>
        /// <param name="waveStream">Output stream</param>
        /// <param name="normalized">True if samples are normalized</param>
        public void SaveTo(Stream waveStream, bool normalized = true)
        {
            using (var writer = new BinaryWriter(waveStream, Encoding.ASCII, true))
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
        /// <para>Gets the signal from container using indexing scheme based on channel type. Examples</para>
        /// <code>
        ///     waveFile[Channels.Left]  -> waveFile.Signals[0]
        ///     <br/>
        ///     waveFile[Channels.Right] -> waveFile.Signals[1] (if it exists)
        ///     <br/>
        ///     waveFile[(Channels)2]    -> waveFile.Signals[2] (if it exists)
        ///     <br/>
        ///     waveFile[Channels.Average] -> returns channel-averaged (new) signal
        ///     <br/>
        ///     waveFile[Channels.Interleave] -> returns interleaved (new) signal
        /// </code>
        /// </summary>
        /// <param name="channel">Channel (left, right, interleave, sum, average, or ordinary index)</param>
        public DiscreteSignal this[Channels channel]
        {
            get
            {
                if (channel != Channels.Interleave && channel != Channels.Sum && channel != Channels.Average)
                {
                    return Signals[(int)channel];
                }

                // in case of averaging or interleaving first check if our signal is mono

                if (WaveFmt.ChannelCount == 1)
                {
                    return Signals[0];
                }

                var length = Signals[0].Length;

                // 1) SUMMING

                if (channel == Channels.Sum)
                {
                    var sumSamples = new float[length];

                    for (var i = 0; i < sumSamples.Length; i++)
                    {
                        for (var j = 0; j < Signals.Count; j++)
                        {
                            sumSamples[i] += Signals[j][i];
                        }
                    }

                    return new DiscreteSignal(WaveFmt.SamplingRate, sumSamples);
                }

                // 2) AVERAGING

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

                // 3) if it ain't mono, we start ACTUALLY interleaving:

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
