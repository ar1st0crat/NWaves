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
        /// Head radius
        /// </summary>
        const float HeadRadius = 8.5e-2f;

        /// <summary>
        /// Speed of sound
        /// </summary>
        const float SpeedOfSound = 340;

        /// <summary>
        /// Constant pi/2
        /// </summary>
        const double Pi2 = Math.PI / 2;

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
        public ItdIldPanEffect(int samplingRate, float pan)
        {
            _samplingRate = samplingRate;
            _headFactor = HeadRadius / SpeedOfSound;

            _itdDelayLeft = new FractionalDelayLine(samplingRate, 0.001f);
            _itdDelayRight = new FractionalDelayLine(samplingRate, 0.001f);

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
            // ITD (Interaural Time Difference)

            _itdDelayLeft.Write(left);
            _itdDelayRight.Write(right);
            
            left = _itdDelayLeft.Read(_delayLeft * _samplingRate);
            right = _itdDelayRight.Read(_delayRight * _samplingRate);

            // ILD (Interaural Level Difference)

            left = _ildFilterLeft.Process(left);
            right = _ildFilterRight.Process(right);
        }

        /// <summary>
        /// Reset effect
        /// </summary>
        public override void Reset()
        {
        }
    }
}
