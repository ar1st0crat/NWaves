using NWaves.Effects.Base;
using NWaves.Utils;
using System; 
using System.Diagnostics; 

namespace NWaves.Effects
{
    /// <summary>
    /// Represents Delay audio effect.
    /// </summary>
    public class DelayEffect : AudioEffect
    {
        /// <summary>
        /// Internal fractional delay line.
        /// </summary>
        private readonly FractionalDelayLine _delayLine;

        /// <summary>
        /// Sampling rate.
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Gets or sets delay (in seconds).
        /// </summary>
        public float Delay
        {
            get => _delay / _fs;
            set
            {
                _delayLine.Ensure(_fs, value);
                _delay = _fs * value;
            }
        }
        private float _delay;

        /// <summary>
        /// Gets or sets feedback parameter.
        /// </summary>
        public float Feedback { get; set; }

        /// <summary>
        /// Constructs <see cref="DelayEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="delay">Delay (in seconds)</param>
        /// <param name="feedback">Feedback</param>
        /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
        /// <param name="reserveDelay">Max delay for reserving the size of delay line</param>
        public DelayEffect(int samplingRate,
                           float delay,
                           float feedback = 0.5f,
                           InterpolationMode interpolationMode = InterpolationMode.Nearest,
                           float reserveDelay = 0f)
        {
            _fs = samplingRate;

            if (reserveDelay < delay)
            {
                _delayLine = new FractionalDelayLine(samplingRate, delay, interpolationMode);
            }
            else
            {
                _delayLine = new FractionalDelayLine(samplingRate, reserveDelay, interpolationMode);
            }

            Delay = delay;
            Feedback = feedback;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var delayed = _delayLine.Read(_delay);

            var output = sample + delayed * Feedback;

            _delayLine.Write(sample);

            return sample * Dry + output * Wet;
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
            float delayed, output;
            bool result;

            result = false;
            float t_Dry = Dry;
            float t_Wet = Wet;
            float t_Feedback = Feedback;

            if ((sampleBuffer == IntPtr.Zero)
                || (frameCount <= 0)
                || (Channel < 1) || (Channel > nChannels)
                || (nChannels < 1) || (nChannels > MAX_CHANNELS))
            {
                goto Finish; //                                         we have a parameter error. Don't change the audio samples.
            }

            try
            { // parms are OK. process the buffer

                unsafe
                {
                    float* p = (float*)sampleBuffer.ToPointer(); //           start with leftmost  channel's first sample
                    if (Channel != 1) p = p + (Channel - 1); //               reposition to correct channel's first sample
                    for (int i = 0; i < (int)frameCount; i++) //              process each frame (sample group) in the buffer
                    {

                        delayed = _delayLine.Read(_delay);
                        _delayLine.Write(*p);
                        output = *p + delayed * t_Feedback;  //               apply delay effect to the current sample in the sampleBuffer
                        *p = *p * t_Dry + output * t_Wet; //                       "            "             "             "           "
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
            _delayLine.Reset();
        }
    }
}
