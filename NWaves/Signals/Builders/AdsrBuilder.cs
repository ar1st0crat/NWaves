using System;
using System.Collections.Generic;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// ASDR envelope builder
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

        private double _attack;
        private double _decay;
        private double _sustain;
        private double _release;

        private AdsrState _state;
        public AdsrState State
        {
            get { return _state; }
            private set
            {
                _state = value;
                UpdateCoefficients();
            }
        }

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
        /// Constructor for ADSR parameters in terms of sample count
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="decay"></param>
        /// <param name="sustain"></param>
        /// <param name="release"></param>
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
        /// Constructor for ADSR parameters in terms of time duration (in sec)
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="decay"></param>
        /// <param name="sustain"></param>
        /// <param name="release"></param>
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

        public override void Reset()
        {
            _n = 0;
            _prev = 0;

            State = AdsrState.Attack;
        }

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
