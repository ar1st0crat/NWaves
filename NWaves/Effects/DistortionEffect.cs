using NWaves.Effects.Base;
using NWaves.Utils;
using System;
using System.Diagnostics; 

namespace NWaves.Effects
{
    /// <summary>
    /// Represents Distortion audio effect.
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
        /// Constructs <see cref="DistortionEffect"/>.
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
        /// Processes one sample.
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
        /// Processes a buffer of (possibly) interleaved samples for a single channel. 
        /// </summary>
        /// <param name="sampleBuffer">audio sample buffer</param>
        /// <param name="Channel">Channel #: 1 to MAX_CHANNELS</param>
        /// <param name="nChannels"># of interleaved Channels in buffer: 1 to MAX_CHANNELS</param>
        /// <param name="frameCount"># of frames (sample groups) in buffer: 1 to MAX_FRAME_COUNT </param>
        public bool ProcessSampleBuffer(in IntPtr sampleBuffer, in int Channel, in int nChannels, in int frameCount)
        {
            float delayed, output, sample;
            bool result;

            result = false;
            float t_Dry = Dry;
            float t_Wet = Wet;

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
                        sample = *p *  _inputGain;
  
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
                        *p = output * t_Wet + sample * t_Dry;

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

        }

        /// <summary>
        /// Processes a buffer of (possibly) interleaved samples for a single channel. 
        /// </summary>
        /// <param name="sampleBuffer">audio sample buffer</param>
        /// <param name="Channel">Channel #: 1 to MAX_CHANNELS</param>
        /// <param name="nChannels"># of interleaved Channels in buffer: 1 to MAX_CHANNELS</param>
        /// <param name="frameCount"># of frames (sample groups) in buffer: 1 to MAX_FRAME_COUNT </param>
        public bool ProcessSampleBuffer(in float[] sampleBuffer, in int Channel, in int nChannels, in int frameCount)
        {
            bool result = false;

            try
            {
                unsafe
                {
                    fixed (float* p = sampleBuffer)
                    {
                        IntPtr ptrSampleBuffer = (IntPtr)p;
                        result = ProcessSampleBuffer(ptrSampleBuffer, Channel, nChannels, frameCount);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached) { Debugger.Break(); }
            }

            return result;

        } 

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
        }
    }
}
