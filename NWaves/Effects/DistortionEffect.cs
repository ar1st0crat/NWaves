using NWaves.Effects.Base;
using NWaves.Utils;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for distortion effect
    /// </summary>
    public class DistortionEffect : AudioEffect
    {
        /// <summary>
        /// Distortion mode
        /// </summary>
        private readonly DistortionMode _mode;

        /// <summary>
        /// Input gain
        /// </summary>
        private float _inputGain;
        public float InputGain
        {
            get => (float)Scale.ToDecibel(_inputGain);
            set => _inputGain = (float)Scale.FromDecibel(value);
        }

        /// <summary>
        /// Output gain
        /// </summary>
        private float _outputGain;
        public float OutputGain
        {
            get => (float)Scale.ToDecibel(_outputGain);
            set => _outputGain = (float)Scale.FromDecibel(value);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="inputGain"></param>
        /// <param name="outputGain"></param>
        public DistortionEffect(DistortionMode mode, float inputGain = 12/*dB*/, float outputGain = -12/*dB*/)
        {
            _mode = mode;
            InputGain = inputGain;
            OutputGain = outputGain;
        }

        /// <summary>
        /// Method implements simple distortion effect
        /// </summary>
        /// <param name="sample">Input sample</param>
        /// <returns>Output sample</returns>
        public override float Process(float sample)
        {
            sample *= _inputGain;

            float output;

            switch (_mode)
            {
                case DistortionMode.HardClipping:

                    if (sample > 0.5f)
                    {
                        output = 0.5f;
                    }
                    else if (sample < -0.5f)
                    {
                        output = -0.5f;
                    }
                    else
                    {
                        output = sample;
                    }
                    break;

                case DistortionMode.Exponential:

                    // DAFX book[Udo Zoelzer], p.124 - 125.

                    if (sample > 0)
                    {
                        output = (float)(1 - Math.Exp(-sample));
                    }
                    else
                    {
                        output = (float)(-1 + Math.Exp(sample));
                    }
                    break;

                case DistortionMode.FullWaveRectify:
                    
                    output = Math.Abs(sample);
                    break;

                case DistortionMode.HalfWaveRectify:

                    if (sample < 0)
                    {
                        output = 0;
                    }
                    else
                    {
                        output = Math.Abs(sample);
                    }
                    break;

                case DistortionMode.SoftClipping:
                default:
                    
                    // DAFX book[Udo Zoelzer], p.118.

                    const float lowerThreshold = 1 / 3f;
                    const float upperThreshold = 2 / 3f;

                    if (sample > upperThreshold)
                    {
                        output = 1;
                    }
                    else if (sample > lowerThreshold)
                    {
                        output = 1 - (2 - 3 * sample) * (2 - 3 * sample) / 3;
                    }
                    else if (sample < -upperThreshold)
                    {
                        output = -1;
                    }
                    else if (sample < -lowerThreshold)
                    { 
                        output = -1 + (2 + 3 * sample) * (2 + 3 * sample) / 3;
                    }
                    else
                    {
                        output = 2 * sample;
                    }

                    output *= 0.5f;

                    break;
            }

            output *= _outputGain;

            return output * Wet + sample * Dry;
        }

        public override void Reset()
        {
        }
    }
}
