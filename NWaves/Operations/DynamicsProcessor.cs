using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;
using System;

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
