using NWaves.Effects.Base;
using NWaves.Filters.BiQuad;
using NWaves.Signals.Builders;
using NWaves.Signals.Builders.Base;
using System; 
using System.Diagnostics; 

namespace NWaves.Effects
{
    /// <summary>
    /// Represents Phaser audio effect.
    /// </summary>
    public class PhaserEffect : AudioEffect
    {
        /// <summary>
        /// Gets or sets Q factor (a.k.a. Quality Factor, resonance).
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// Gets or sets LFO frequency (in Hz).
        /// </summary>
        public float LfoFrequency
        {
            get => _lfoFrequency;
            set
            {
                _lfoFrequency = value;
                Lfo.SetParameter("freq", value);
            }
        }
        private float _lfoFrequency;

        /// <summary>
        /// Gets or sets minimal LFO frequency (in Hz).
        /// </summary>
        public float MinFrequency
        {
            get => _minFrequency;
            set
            {
                _minFrequency = value;
                Lfo.SetParameter("min", value);
            }
        }
        private float _minFrequency;

        /// <summary>
        /// Gets or sets maximal LFO frequency (in Hz).
        /// </summary>
        public float MaxFrequency
        {
            get => _maxFrequency;
            set
            {
                _maxFrequency = value;
                Lfo.SetParameter("max", value);
            }
        }
        private float _maxFrequency;

        /// <summary>
        /// Get or sets LFO signal generator.
        /// </summary>
        public SignalBuilder Lfo { get; set; }

        /// <summary>
        /// Sampling rate.
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Notch filter with varying center frequency.
        /// </summary>
        private readonly NotchFilter _filter;

        /// <summary>
        /// Constructs <see cref="PhaserEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfoFrequency">LFO frequency (in Hz)</param>
        /// <param name="minFrequency">Minimal LFO frequency (in Hz)</param>
        /// <param name="maxFrequency">Maximal LFO frequency (in Hz)</param>
        /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
        public PhaserEffect(int samplingRate,
                            float lfoFrequency = 1.0f,
                            float minFrequency = 300,
                            float maxFrequency = 3000,
                            float q = 0.5f)
        {
            _fs = samplingRate;
            
            Lfo = new TriangleWaveBuilder().SampledAt(samplingRate);
            
            LfoFrequency = lfoFrequency;
            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
            Q = q;

            _filter = new NotchFilter(Lfo.NextSample() / _fs, Q);
        }

        /// <summary>
        /// Constructs <see cref="PhaserEffect"/> from <paramref name="lfo"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfo">LFO signal generator</param>
        /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
        public PhaserEffect(int samplingRate, SignalBuilder lfo, float q = 0.5f)
        {
            _fs = samplingRate;
            Q = q;
            Lfo = lfo;

            _filter = new NotchFilter(Lfo.NextSample() / _fs, Q);
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var output = _filter.Process(sample);

            _filter.Change(Lfo.NextSample() / _fs, Q);     // vary notch filter coefficients

            return output * Wet + sample * Dry;
        }


        /// <summary>
        /// Processes a buffer of (possibly) interleaved samples for a single channel. 
        /// </summary>
        /// <param name="sampleBuffer">audio sample buffer</param>
        /// <param name="Channel">Channel #: 1 to MAX_CHANNELS</param>
        /// <param name="nChannels"># of interleaved Channels in buffer: 1 to MAX_CHANNELS</param>
        /// <param name="frameCount"># of frames (sample groups) in buffer: 1 to MAX_FRAME_COUNT </param>
        public bool ProcessSampleBuffer(in IntPtr sampleBuffer, in int Channel, in int nChannels, in int frameCount)
        {
            float output;
            bool result;

            result = false;
            float t_Dry = Dry;
            float t_Wet = Wet;
            float t_Q = Q;

            if ((sampleBuffer == IntPtr.Zero)
                || (frameCount <= 0)
                || (Channel < 1) || (Channel > nChannels)
                || (nChannels < 1) || (nChannels > MAX_CHANNELS))
            {
                goto Finish;
            } //                                         we have a parameter error. Don't change the audio samples.

            try
            { // parms are OK. process the buffer

                unsafe
                {
                    float* p = (float*)sampleBuffer.ToPointer(); //           start with leftmost  channel's first sample
                    if (Channel != 1) p = p + (Channel - 1); //               reposition to correct channel's first sample
                    for (int i = 0; i < (int)frameCount; i++) //              process each frame (sample group) in the buffer
                    {
                        output = _filter.Process(*p);
                        _filter.Change(Lfo.NextSample() / _fs, t_Q);     // vary notch filter coefficients
                        *p = output * t_Wet + *p * t_Dry;

                        p += nChannels; //                                    move to the next frame (sample group) in the buffer           
                    }
                }

                result = true;

            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached) { Debugger.Break(); }
            }

        Finish:
            return result;

        } 

        /// <summary>
        /// Processes a buffer of (possibly) interleaved samples for a single channel.     
        /// </summary>
        /// <param name="sampleBuffer">audio sample buffer</param>
        /// <param name="Channel">Channel #: 1 to MAX_CHANNELS</param>
        /// <param name="nChannels"># of interleaved Channels in buffer: 1 to MAX_CHANNELS</param>
        /// <param name="frameCount"># of frames (sample groups) in buffer: 1 to MAX_FRAME_COUNT </param>
        public bool ProcessSampleBuffer(in float[] sampleBuffer, in int Channel, in int nChannels, in int frameCount)
        {
            bool result = false;

            try
            {
                unsafe
                {
                    fixed (float* p = sampleBuffer)
                    {
                        IntPtr ptrSampleBuffer = (IntPtr)p;
                        result = ProcessSampleBuffer(ptrSampleBuffer, Channel, nChannels, frameCount);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached) { Debugger.Break(); }
            }

            return result;

        } 

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
            _filter.Reset();
            Lfo.Reset();
        }
    }
}
