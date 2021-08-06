using NWaves.Filters.BiQuad;
using NWaves.Utils;
using System;

namespace NWaves.Effects.Stereo
{
    /// <summary>
    /// Stereo pan effect based on ITD-ILD (Interaural Time Difference - Interaural Level Difference)
    /// </summary>
    public class ItdIldPanEffect : StereoEffect
    {
        /// <summary>
        /// Speed of sound
        /// </summary>
        const float SpeedOfSound = 340;

        /// <summary>
        /// Constant pi/2
        /// </summary>
        const double Pi2 = Math.PI / 2;

        /// <summary>
        /// Head radius
        /// </summary>
        private readonly float _headRadius;
        public float HeadRadius => _headRadius;

        /// <summary>
        /// Sampling rate
        /// </summary>
        private readonly int _samplingRate;

        /// <summary>
        /// Head factor
        /// </summary>
        private readonly float _headFactor;

        /// <summary>
        /// ITD delay lines
        /// </summary>
        private readonly FractionalDelayLine _itdDelayLeft, _itdDelayRight;

        /// <summary>
        /// ILD filters
        /// </summary>
        private readonly BiQuadFilter _ildFilterLeft, _ildFilterRight;

        /// <summary>
        /// Time delays
        /// </summary>
        private double _delayLeft, _delayRight;

        /// <summary>
        /// Pan
        /// </summary>
        private float _pan;
        public float Pan 
        {
            get => _pan;
            set
            {
                _pan = value;

                // update ITD parameters

                var phi = _pan * Pi2;

                _delayLeft  = Itd(phi + Pi2);
                _delayRight = Itd(phi - Pi2);

                // update ILD parameters

                var alphaL = 1 + (float) Math.Cos(phi + Pi2);
                var alphaR = 1 + (float) Math.Cos(phi - Pi2);

                _ildFilterLeft.Change(_headFactor + alphaL, _headFactor - alphaL, 0, _headFactor + 1, _headFactor - 1, 0);
                _ildFilterRight.Change(_headFactor + alphaR, _headFactor - alphaR, 0, _headFactor + 1, _headFactor - 1, 0);
            }
        }

        /// <summary>
        /// Interaural Time Difference
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        double Itd(double angle)
        {
            if (Math.Abs(angle) < Pi2)
            {
                return _headFactor * (1 - Math.Cos(angle));
            }
            else
            {
                return _headFactor * (Math.Abs(angle) + 1 - Pi2);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="pan"></param>
        /// <param name="interpolationMode"></param>
        /// <param name="maxDelaySeconds"></param>
        /// <param name="headRadius"></param>
        public ItdIldPanEffect(int samplingRate,
                               float pan,
                               InterpolationMode interpolationMode = InterpolationMode.Linear,
                               double maxDelaySeconds = 0.001,
                               float headRadius = 8.5e-2f)
        {
            _samplingRate = samplingRate;
            _headRadius = headRadius;
            _headFactor = _headRadius / SpeedOfSound;

            _itdDelayLeft = new FractionalDelayLine(samplingRate, maxDelaySeconds, interpolationMode);
            _itdDelayRight = new FractionalDelayLine(samplingRate, maxDelaySeconds, interpolationMode);

            _ildFilterLeft = new BiQuadFilter(1, 0, 0, 0, 0, 0);
            _ildFilterRight = new BiQuadFilter(1, 0, 0, 0, 0, 0);

            Pan = pan;
        }

        /// <summary>
        /// Process current sample in each channel
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public override void Process(ref float left, ref float right)
        {
            var leftIn = left;
            var rightIn = right;

            // ITD (Interaural Time Difference)

            _itdDelayLeft.Write(left);
            _itdDelayRight.Write(right);
            
            left = _itdDelayLeft.Read(_delayLeft * _samplingRate);
            right = _itdDelayRight.Read(_delayRight * _samplingRate);

            // ILD (Interaural Level Difference)

            left = _ildFilterLeft.Process(left);
            right = _ildFilterRight.Process(right);

            // Wet/dry mixing (if necessary)

            left = leftIn * Dry + left * Wet;
            right = rightIn * Dry + right * Wet;
        }

        /// <summary>
        /// Reset effect
        /// </summary>
        public override void Reset()
        {
            _itdDelayLeft.Reset();
            _itdDelayRight.Reset();
            _ildFilterLeft.Reset();
            _ildFilterRight.Reset();
        }
    }
}
