using NWaves.Effects.Base;
using NWaves.Utils;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Class representing Distortion audio effect.
    /// </summary>
    public class DistortionEffect : AudioEffect
    {
        /// <summary>
        /// Gets or sets distortion mode (soft/hard clipping, exponential, full/half-wave rectify).
        /// </summary>
        public DistortionMode Mode { get; set; }

        /// <summary>
        /// Gets or sets input gain (in dB).
        /// </summary>
        public float InputGain
        {
            get => (float)Scale.ToDecibel(_inputGain);
            set => _inputGain = (float)Scale.FromDecibel(value);
        }
        private float _inputGain;

        /// <summary>
        /// Gets or sets output gain (in dB).
        /// </summary>
        public float OutputGain
        {
            get => (float)Scale.ToDecibel(_outputGain);
            set => _outputGain = (float)Scale.FromDecibel(value);
        }
        private float _outputGain;

        /// <summary>
        /// Construct <see cref="DistortionEffect"/>.
        /// </summary>
        /// <param name="mode">Distortion mode</param>
        /// <param name="inputGain">Input gain (in dB)</param>
        /// <param name="outputGain">Output gain (in dB)</param>
        public DistortionEffect(DistortionMode mode, float inputGain = 12/*dB*/, float outputGain = -12/*dB*/)
        {
            Mode = mode;
            InputGain = inputGain;
            OutputGain = outputGain;
        }

        /// <summary>
        /// Process one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            sample *= _inputGain;

            float output;

            switch (Mode)
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

        /// <summary>
        /// Reset effect.
        /// </summary>
        public override void Reset()
        {
        }
    }
}
