using System;
using NWaves.Effects.Base;
using NWaves.Filters.Base;
using NWaves.Utils;
using System.Diagnostics; //                                                                                      2022-04-27: J.P.B.

namespace NWaves.Effects
{
    // DAFX book [Udo Zoelzer], p.123-124.

    /// <summary>
    /// Represents Tube Distortion audio effect.
    /// </summary>
    public class TubeDistortionEffect : AudioEffect
    {
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
        /// Gets or sets Q factor (Work point). 
        /// Controls the linearity of the transfer function for low input levels. 
        /// More negative - more linear.
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// Gets or sets distortion's character. 
        /// Higher number - harder distortion.
        /// </summary>
        public float Dist { get; set; }

        /// <summary>
        /// Gets filter coefficient (close to 1.0) defining placement of poles 
        /// in the HP filter that removes DC component.
        /// </summary>
        public float Rh { get; }

        /// <summary>
        /// Gets filter coefficient (in range [0, 1]) defining placement of pole 
        /// in the LP filter used to simulate capacitances in tube amplifier.
        /// </summary>
        public float Rl { get; }

        /// <summary>
        /// Internal filter for output signal 
        /// that combines HP and LP filters mentioned above.
        /// </summary>
        private readonly LtiFilter _outputFilter;

        /// <summary>
        /// Constructs <see cref="TubeDistortionEffect"/>.
        /// </summary>
        /// <param name="inputGain">Input gain (in dB)</param>
        /// <param name="outputGain">Output gain (in dB)</param>
        /// <param name="q">Q factor (controls the linearity of the transfer function for low input levels. More negative means more linear)</param>
        /// <param name="dist">Distortion's character (higher number means harder distortion)</param>
        /// <param name="rh">Filter coefficient (close to 1.0) defining placement of poles in the HP filter that removes DC component</param>
        /// <param name="rl">Filter coefficient (in range [0, 1]) defining placement of pole in the LP filter used to simulate capacitances in tube amplifier</param>
        public TubeDistortionEffect(float inputGain = 20/*dB*/,
                                    float outputGain = -12/*dB*/,
                                    float q = -0.2f,
                                    float dist = 5,
                                    float rh = 0.995f,
                                    float rl = 0.5f)
        {
            InputGain = inputGain;
            OutputGain = outputGain;

            Q = q;
            Dist = dist;
            Rh = rh;
            Rl = rl;

            var filter1 = new IirFilter(new[] { 1.0, -2, 1 }, new[] { 1.0, -2 * Rh, Rh * Rh });
            var filter2 = new IirFilter(new[] { 1.0 - Rl },   new[] { 1.0, -Rl });

            _outputFilter = filter1 * filter2;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            float output;

            var q = sample * _inputGain;

            if (Math.Abs(Q) < 1e-10)
            {
                output = Math.Abs(q - Q) < 1e-10 ? 1.0f / Dist : (float)(q / (1 - Math.Exp(-Dist * q)));
            }
            else
            {
                output = Math.Abs(q - Q) < 1e-10 ?
                           (float)(1.0 / Dist + Q / (1 - Math.Exp(Dist * Q))) :
                           (float)((q - Q) / (1 - Math.Exp(-Dist * (q - Q))) + Q / (1 - Math.Exp(Dist * Q)));
            }

            output = _outputFilter.Process(output) * _outputGain;
            
            return output * Wet + sample * Dry;
        }

        /// <summary>
        /// Processes a buffer of (possibly) interleaved samples for a single channel.                            2022-04-27: Start    J.P.B.
        /// </summary>
        /// <param name="sampleBuffer">audio sample buffer</param>
        /// <param name="Channel">Channel #: 1 to MAX_CHANNELS</param>
        /// <param name="nChannels"># of interleaved Channels in buffer: 1 to MAX_CHANNELS</param>
        /// <param name="frameCount"># of frames (sample groups) in buffer: 1 to MAX_FRAME_COUNT </param>
        public bool ProcessSampleBuffer(in IntPtr sampleBuffer, in int Channel, in int nChannels, in int frameCount)
        {
            float delayed, output, q;
            bool result;

            result = false;
            float t_Dry = Dry;
            float t_Wet = Wet;
            float t_Q = Q;
            float t_Dist = Dist;

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
                        q = *p * _inputGain;

                        if (Math.Abs(t_Q) < 1e-10)
                        {
                            output = Math.Abs(q - t_Q) < 1e-10 ? 1.0f / t_Dist : (float)(q / (1 - Math.Exp(-t_Dist * q)));
                        }
                        else
                        {
                            output = Math.Abs(q - t_Q) < 1e-10 ?
                                       (float)(1.0 / t_Dist + t_Q / (1 - Math.Exp(t_Dist * t_Q))) :
                                       (float)((q - t_Q) / (1 - Math.Exp(-t_Dist * (q - t_Q))) + t_Q / (1 - Math.Exp(t_Dist * t_Q)));
                        }

                        output = _outputFilter.Process(output) * _outputGain;
                        *p = output * t_Wet + *p * t_Dry;
                        
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

        } //                                                                                                      2022-04-27: End

        /// <summary>
        /// Processes a buffer of (possibly) interleaved samples for a single channel.                            2022-07-06: Start    J.P.B.
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

        } //                                                                                                      2022-07-06: End    J.P.B.

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
            _outputFilter.Reset();
        }
    }
}
