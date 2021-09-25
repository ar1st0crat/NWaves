using NWaves.Signals.Builders.Base;
using System;
using System.Collections.Generic;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// <para>ADSR envelope builder.</para>
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"attack", "a" (default: 0.2)</item>
    ///     <item>"decay", "d" (default: 0.2)</item>
    ///     <item>"sustain", "s" (default: 0.2)</item>
    ///     <item>"release", "r" (default: 0.2)</item>
    ///     <item>"attackAmp", "amp" (default: 1.5)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class AdsrBuilder : SignalBuilder
    {
        public enum AdsrState
        {
            Attack,
            Decay,
            Sustain,
            Release
        }

        /// <summary>
        /// Gets ADSR state (attack, decay, sustain, release).
        /// </summary>
        public AdsrState State
        {
            get => _state;
            private set
            {
                _state = value;
                UpdateCoefficients();
            }
        }
        private AdsrState _state;

        private double _attack;
        private double _decay;
        private double _sustain;
        private double _release;

        private float _attackAmp = 1.5f;
        private double _attackSlope = 0.2;
        private double _decaySlope = 0.2;
        private double _sustainSlope = 0.2;
        private double _releaseSlope = 0.2;

        /// <summary>
        /// One-pole filter coefficients
        /// </summary>
        private float _a, _b;

        /// <summary>
        /// Constructs <see cref="AdsrBuilder"/> from ADSR parameters (in the form of number of samples).
        /// </summary>
        /// <param name="attack">Number of samples for attack stage</param>
        /// <param name="decay">Number of samples for decay stage</param>
        /// <param name="sustain">Number of samples for sustain stage</param>
        /// <param name="release">Number of samples for release stage</param>
        public AdsrBuilder(int attack, int decay, int sustain, int release)
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                {"attack, a",      param => { _attackSlope = param; UpdateCoefficients(); } },
                {"decay, d",       param => { _decaySlope = param; UpdateCoefficients(); } },
                {"sustain, s",     param => { _sustainSlope = param; UpdateCoefficients(); } },
                {"release, r",     param => { _releaseSlope = param; UpdateCoefficients(); } },
                {"amp, attackAmp", param =>   _attackAmp = (float)param },
            };

            _attack = attack;
            _decay = _attack + decay;
            _sustain = _decay + sustain;
            _release = _sustain + release;

            Reset();
        }

        /// <summary>
        /// Constructs <see cref="AdsrBuilder"/> from ADSR parameters (in the form of duration in seconds).
        /// </summary>
        /// <param name="attack">Duration of attack stage (seconds)</param>
        /// <param name="decay">Duration of decay stage (seconds)</param>
        /// <param name="sustain">Duration of sustain stage (seconds)</param>
        /// <param name="release">Duration of release stage (seconds)</param>
        public AdsrBuilder(double attack, double decay, double sustain, double release)
        {
            // these parameters will be cast to sample count when the sampling rate is specified

            _attack = attack;
            _decay = _attack + decay;
            _sustain = _decay + sustain;
            _release = _sustain + release;

            ParameterSetters = new Dictionary<string, Action<double>>
            {
                {"attack, a",      param => { _attackSlope = param; UpdateCoefficients(); } },
                {"decay, d",       param => { _decaySlope = param; UpdateCoefficients(); } },
                {"sustain, s",     param => { _sustainSlope = param; UpdateCoefficients(); } },
                {"release, r",     param => { _releaseSlope = param; UpdateCoefficients(); } },
                {"amp, attackAmp", param =>   _attackAmp = (float)param },
            };

            Reset();
        }

        /// <summary>
        /// Generate new sample.
        /// </summary>
        public override float NextSample()
        {
            float cur;

            if (_n > _sustain)
            {
                if (_state != AdsrState.Release)
                {
                    State = AdsrState.Release;
                }
                cur = 0;
            }
            else if (_n > _decay)
            {
                if (_state != AdsrState.Sustain)
                {
                    State = AdsrState.Sustain;
                }
                cur = 1;
            }
            else if (_n > _attack)
            {
                if (_state != AdsrState.Decay)
                {
                    State = AdsrState.Decay;
                }
                cur = 1;
            }
            else
            {
                cur = _attackAmp;
            }

            _prev = _b * cur - _a * _prev;

            if (++_n == _release)
            {
                _n = 0;
            }

            return _prev;
        }

        /// <summary>
        /// Reset sample generator.
        /// </summary>
        public override void Reset()
        {
            _n = 0;
            _prev = 0;

            State = AdsrState.Attack;
        }

        /// <summary>
        /// Set the sampling rate of the signal to build.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        public override SignalBuilder SampledAt(int samplingRate)
        {
            _attack *= samplingRate;
            _decay *= samplingRate;
            _sustain *= samplingRate;
            _release *= samplingRate;

            UpdateCoefficients();

            return base.SampledAt(samplingRate);
        }

        private void UpdateCoefficients()
        {
            switch (_state)
            {
                case AdsrState.Release:
                    _a = (float)-Math.Exp(-1 / ((_release - _sustain) * _releaseSlope));
                    break;
                case AdsrState.Sustain:
                    _a = (float)-Math.Exp(-1 / ((_sustain - _decay) * _sustainSlope));
                    break;
                case AdsrState.Decay:
                    _a = (float)-Math.Exp(-1 / ((_decay - _attack) * _decaySlope));
                    break;
                default:
                    _a = (float)-Math.Exp(-1 / (_attack * _attackSlope));
                    break;
            }
            _b = 1 + _a;
        }

        private int _n;
        private float _prev;
    }
}
