using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;
using System;
using System.Linq;

namespace NWaves.Operations
{
    /// <summary>
    /// Dynamics processor: limiter / compressor / expander / noise gate
    /// </summary>
    public class DynamicsProcessor : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Dynamics processor mode
        /// </summary>
        private readonly DynamicsMode _mode;

        /// <summary>
        /// Envelope follower
        /// </summary>
        private readonly EnvelopeFollower _envelopeFollower;

        /// <summary>
        /// Sampling rate
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Min threshold for dB amplitude
        /// </summary>
        private readonly float _minAmplitudeDb;

        /// <summary>
        /// Attack/Release time coefficient
        /// </summary>
        private readonly float T = 1 / (float)Math.Log(9); // = approx. 2.2

        /// <summary>
        /// Compression threshold
        /// </summary>
        public float Threshold { get; set; }

        /// <summary>
        /// Compression ratio
        /// </summary>
        public float Ratio { get; set; }
        
        /// <summary>
        /// Makeup gain
        /// </summary>
        public float MakeupGain { get; set; }

        /// <summary>
        /// Attack time
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
        /// Release time
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
        /// Constructor
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="samplingRate"></param>
        /// <param name="threshold"></param>
        /// <param name="ratio"></param>
        /// <param name="makeupGain"></param>
        /// <param name="attack"></param>
        /// <param name="release"></param>
        /// <param name="minAmplitudeDb"></param>
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

        public void Reset()
        {
            _envelopeFollower.Reset();
        }

        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
        {
            return new DiscreteSignal(signal.SamplingRate, signal.Samples.Select(s => Process(s)));
        }
    }
}
