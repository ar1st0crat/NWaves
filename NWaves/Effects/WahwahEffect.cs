using System;
using NWaves.Effects.Base;
using NWaves.Signals.Builders;
using NWaves.Signals.Builders.Base;
using System; //                                                                                                  2022-06-26: J.P.B.
using System.Diagnostics; //                                                                                      2022-06-26: J.P.B.

namespace NWaves.Effects
{
    /// <summary>
    /// Represents Wah-Wah audio effect.
    /// </summary>
    public class WahwahEffect : AudioEffect
    {
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
        /// Gets or sets Q factor (a.k.a. Quality Factor, resonance).
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// Gets or sets LFO signal generator.
        /// </summary>
        public SignalBuilder Lfo { get; set; }

        /// <summary>
        /// Sampling rate.
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Constructs <see cref="WahwahEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfoFrequency">LFO frequency (in Hz)</param>
        /// <param name="minFrequency">Minimal LFO frequency (in Hz)</param>
        /// <param name="maxFrequency">Maximal LFO frequency (in Hz)</param>
        /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
        public WahwahEffect(int samplingRate,
                            float lfoFrequency = 1.0f,
                            float minFrequency = 300,
                            float maxFrequency = 1500,
                            float q = 0.5f)
        {
            _fs = samplingRate;

            Lfo = new TriangleWaveBuilder().SampledAt(samplingRate);

            LfoFrequency = lfoFrequency;
            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
            Q = q;
        }

        /// <summary>
        /// Constructs <see cref="WahwahEffect"/> from <paramref name="lfo"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfo">LFO signal generator</param>
        /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
        public WahwahEffect(int samplingRate, SignalBuilder lfo, float q = 0.5f)
        {
            _fs = samplingRate;
            Q = q;
            Lfo = lfo;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var fs2pi = 2 * Math.PI / _fs;

            var f = (float)(2 * Math.Sin(Lfo.NextSample() * fs2pi));

            _yh = sample - _yl - Q * _yb;
            _yb += f * _yh;
            _yl += f * _yb;

            return _yb * Wet + sample * Dry;
        }

        /// <summary>
        /// Processes a buffer of (possibly) interleaved samples for a single channel.                            2022-06-26: Start    J.P.B.
        /// </summary>
        /// <param name="sampleBuffer">audio sample buffer</param>
        /// <param name="Channel">Channel #: 1 to MAX_CHANNELS</param>
        /// <param name="nChannels"># of interleaved Channels in buffer: 1 to MAX_CHANNELS</param>
        /// <param name="frameCount"># of frames (sample groups) in buffer: 1 to MAX_FRAME_COUNT </param>
        public bool ProcessSampleBuffer(in IntPtr sampleBuffer, in int Channel, in int nChannels, in int frameCount)
        {
            float delayed, output;
            bool result;

            result = false;
            float t_Dry = Dry;
            float t_Wet = Wet;
            double fs2pi;
            float f;

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
                        fs2pi = 2.0 * Math.PI / _fs;
                        f = (float)(2 * Math.Sin(Lfo.NextSample() * fs2pi));
                        _yh = *p - _yl - Q * _yb;
                        _yb += f * _yh;
                        _yl += f * _yb;
                        *p = _yb * t_Wet + *p * t_Dry;
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

        } //                                                                                                      2022-06-26: End

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
            _yh = _yb = _yl = 0;
            Lfo.Reset();
        }

        private float _yh, _yb, _yl;
    }
}
