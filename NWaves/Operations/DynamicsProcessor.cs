using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;
using System;
using System.Diagnostics; //                                                                                      2022-04-20: J.P.B.

namespace NWaves.Operations
{
    /// <summary>
    /// Represents dynamics processor: limiter or compressor or expander or noise gate.
    /// </summary>
    public class DynamicsProcessor : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Dynamics processor mode.
        /// </summary>
        private readonly DynamicsMode _mode;

        /// <summary>
        /// Envelope follower.
        /// </summary>
        private readonly EnvelopeFollower _envelopeFollower;

        /// <summary>
        /// Sampling rate.
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Min threshold for dB amplitude.
        /// </summary>
        private readonly float _minAmplitudeDb;

        /// <summary>
        /// Attack/Release time coefficient.
        /// </summary>
        private readonly float T = 1 / (float)Math.Log(9); // = approx. 2.2

        /// <summary>
        /// Gets or sets compression/expansion threshold.
        /// </summary>
        public float Threshold { get; set; }

        /// <summary>
        /// Gets or sets compression/expansion ratio.
        /// </summary>
        public float Ratio { get; set; }
        
        /// <summary>
        /// Gets or sets makeup gain.
        /// </summary>
        public float MakeupGain { get; set; }

        /// <summary>
        /// Gets or sets attack time.
        /// </summary>
        public float Attack
        {
            get
            {
                switch(_mode)
                {
                    case DynamicsMode.Limiter:
                    case DynamicsMode.Compressor:
                    default:
                        return _envelopeFollower.AttackTime / T;
                    case DynamicsMode.Expander:
                    case DynamicsMode.NoiseGate:
                        return _envelopeFollower.ReleaseTime / T;
                };
            }
            set
            {
                switch (_mode)
                {
                    case DynamicsMode.Limiter:
                    case DynamicsMode.Compressor:
                    default:
                        _envelopeFollower.AttackTime = value * T;
                        break;
                    case DynamicsMode.Expander:
                    case DynamicsMode.NoiseGate:
                        _envelopeFollower.ReleaseTime = value * T;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets release time.
        /// </summary>
        public float Release
        {
            get
            {
                switch (_mode)
                {
                    case DynamicsMode.Limiter:
                    case DynamicsMode.Compressor:
                    default:
                        return _envelopeFollower.ReleaseTime / T;
                    case DynamicsMode.Expander:
                    case DynamicsMode.NoiseGate:
                        return _envelopeFollower.AttackTime / T;
                };
            }
            set
            {
                switch (_mode)
                {
                    case DynamicsMode.Limiter:
                    case DynamicsMode.Compressor:
                    default:
                        _envelopeFollower.ReleaseTime = value * T;
                        break;
                    case DynamicsMode.Expander:
                    case DynamicsMode.NoiseGate:
                        _envelopeFollower.AttackTime = value * T;
                        break;
                }
            }
        }

        /// <summary>
        /// Constructs <see cref="DynamicsProcessor"/> in given <paramref name="mode"/>.
        /// </summary>
        /// <param name="mode">Type (mode) of dynamics processor</param>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="threshold">Compression/expansion threshold</param>
        /// <param name="ratio">Compression/expansion ratio</param>
        /// <param name="makeupGain">Makeup gain</param>
        /// <param name="attack">Attack time (in seconds)</param>
        /// <param name="release">Release time (in seconds)</param>
        /// <param name="minAmplitudeDb">Min threshold for dB amplitude</param>
        public DynamicsProcessor(DynamicsMode mode,
                                 int samplingRate,
                                 float threshold,
                                 float ratio,
                                 float makeupGain = 0,
                                 float attack = 0.01f,
                                 float release = 0.1f,
                                 float minAmplitudeDb = -120/*dB*/)
        {
            _mode = mode;
            _fs = samplingRate;
            _envelopeFollower = new EnvelopeFollower(_fs);
            _minAmplitudeDb = minAmplitudeDb;
            
            Threshold = threshold;
            Ratio = ratio;
            MakeupGain = makeupGain;
            Attack = attack;
            Release = release;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public float Process(float sample)
        {
            var abs = Math.Abs(sample);

            var xg = abs > 1e-6f ? (float)Scale.ToDecibel(abs) : _minAmplitudeDb;
            var yg = 0f;

            switch (_mode)
            {
                case DynamicsMode.Limiter:
                case DynamicsMode.Compressor:
                    {
                        yg = xg < Threshold ? xg : Threshold + (xg - Threshold) / Ratio;
                        break;
                    }

                case DynamicsMode.Expander:
                case DynamicsMode.NoiseGate:
                    {
                        yg = xg > Threshold ? xg : Threshold + (xg - Threshold) * Ratio;
                        break;
                    }
            }

            var envelope = _envelopeFollower.Process(yg - xg);

            var gain = (float)Scale.FromDecibel(MakeupGain - envelope);

            return sample * gain;
        }

        private float[] _ygMINUSxg = null;//                                                                      2022-05-18: Start    J.P.B.
        private float[] _envelope = null;
        /// <summary>
        /// Processes a buffer of (possibly) interleaved samples for a single channel.                           
        /// </summary>
        /// <param name="sampleBuffer">audio sample buffer</param>
        /// <param name="Channel">Channel #: 1 to MAX_CHANNELS</param>
        /// <param name="nChannels"># of interleaved Channels in buffer: 1 to MAX_CHANNELS</param>
        /// <param name="frameCount"># of frames (sample groups) in buffer: 1 to MAX_FRAME_COUNT </param>
        public bool ProcessSampleBuffer(in IntPtr sampleBuffer, in int Channel, in int nChannels, in int frameCount)
        {
            bool result;
            float t_Threshold = Threshold;
            float t_Ratio = Ratio;
            float t_MakeupGain = MakeupGain;
            float abs, xg, yg, gain;

            result = false;

            if ((sampleBuffer == IntPtr.Zero)
                || (frameCount <= 0)
                || (Channel < 1) || (Channel > nChannels)
                || (nChannels < 1) || (nChannels > NWaves.Effects.Base.AudioEffect.MAX_CHANNELS))
            {
                goto Finish;
            } //                                         we have a parameter error. Don't change the audio samples.

            if (_ygMINUSxg == null || _ygMINUSxg.Length != frameCount)
            {
                _ygMINUSxg = new float[frameCount];
                _envelope = new float[frameCount];
            }

            try
            { // parms are OK. process the buffer

                unsafe
                {
                    float* p = (float*)sampleBuffer.ToPointer(); //           start with leftmost channel's first sample
                    if (Channel != 1) p = p + (Channel - 1); //               reposition to correct channel's first sample
                    for (int i = 0; i < (int)frameCount; i++) //              process each frame (sample group) in the buffer
                    {
                        abs = Math.Abs(*p);
                        xg = abs > 1e-6f ? (float)Scale.ToDecibel(abs) : _minAmplitudeDb;
                        yg = 0f;

                        switch (_mode)
                        {
                            case DynamicsMode.Limiter:
                            case DynamicsMode.Compressor:
                                {
                                    yg = xg < t_Threshold ? xg : t_Threshold + (xg - t_Threshold) / t_Ratio;
                                    break;
                                }

                            case DynamicsMode.Expander:
                            case DynamicsMode.NoiseGate:
                                {
                                    yg = xg > t_Threshold ? xg : t_Threshold + (xg - t_Threshold) * t_Ratio;
                                    break;
                                }
                        }

                        _ygMINUSxg[i] = yg - xg;
                        p += nChannels; //                                    move to the next frame (sample group) in the buffer           
                    }

                    _envelopeFollower.ProcessArray(in _ygMINUSxg, ref _envelope);

                    p = (float*)sampleBuffer.ToPointer(); //                  start with leftmost  channel's first sample
                    if (Channel != 1) p = p + (Channel - 1); //               reposition to correct channel's first sample
                    for (int i = 0; i < (int)frameCount; i++) //              process each frame (sample group) in the buffer
                    {
                        gain = (float)Scale.FromDecibel(t_MakeupGain - _envelope[i]);
                        *p = *p * gain;

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

        } //                                                                                                      2022-05-18: End

        /// <summary>
        /// Resets dynamics processor.
        /// </summary>
        public void Reset()
        {
            _envelopeFollower.Reset();
        }

        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new signal (dynamics).
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);
    }
}
