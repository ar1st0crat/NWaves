using NWaves.Effects.Base;
using System;
using System.Diagnostics; 

namespace NWaves.Effects
{
    /// <summary>
    /// Represents Bitcrusher (distortion) audio effect.
    /// </summary>
    public class BitCrusherEffect : AudioEffect
    {
        /// <summary>
        /// Step is calculated from bit depth.
        /// </summary>
        private float _step;

        /// <summary>
        /// Gets or sets the bit depth (number of bits).
        /// </summary>
        public int BitDepth 
        {
            get => _bitDepth;
            set
            {
                _bitDepth = value;
                _step = 2 * (float)Math.Pow(0.5, _bitDepth);
            }
        }
        private int _bitDepth;

        /// <summary>
        /// Constructs <see cref="BitCrusherEffect"/> with given <paramref name="bitDepth"/>.
        /// </summary>
        /// <param name="bitDepth">Bit depth (number of bits)</param>
        public BitCrusherEffect(int bitDepth)
        {
            BitDepth = bitDepth;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var output = (float)(_step * Math.Floor(sample / _step + 0.5));

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
            float output;
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
                        output = (float)(_step * Math.Floor(*p / _step + 0.5));
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
