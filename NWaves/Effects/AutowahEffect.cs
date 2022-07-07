using NWaves.Effects.Base;
using NWaves.Operations;
using System;
using System.Diagnostics; 

namespace NWaves.Effects
{
    /// <summary>
    /// Represents AutoWah audio effect (envelope follower + Wah-Wah effect).
    /// </summary>
    public class AutowahEffect : AudioEffect
    {
        /// <summary>
        /// Gets or sets Q factor (a.k.a. Quality Factor, resonance).
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// Gets or sets minimal LFO frequency (in Hz).
        /// </summary>
        public float MinFrequency { get; set; }

        /// <summary>
        /// Gets or sets maximal LFO frequency (in Hz).
        /// </summary>
        public float MaxFrequency { get; set; }

        /// <summary>
        /// Gets or sets attack time (in seconds).
        /// </summary>
        public float AttackTime
        {
            get => _envelopeFollower.AttackTime;
            set => _envelopeFollower.AttackTime = value;
        }

        /// <summary>
        /// Gets or sets release time (in seconds).
        /// </summary>
        public float ReleaseTime
        {
            get => _envelopeFollower.ReleaseTime;
            set => _envelopeFollower.ReleaseTime = value;
        }

        /// <summary>
        /// Sampling rate.
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Internal envelope follower.
        /// </summary>
        private readonly EnvelopeFollower _envelopeFollower;

        /// <summary>
        /// Constructs <see cref="AutowahEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="minFrequency">Minimal LFO frequency (in Hz)</param>
        /// <param name="maxFrequency">Maximal LFO frequency (in Hz)</param>
        /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
        /// <param name="attackTime">Attack time (in seconds)</param>
        /// <param name="releaseTime">Release time (in seconds)</param>
        public AutowahEffect(int samplingRate,
                             float minFrequency = 30,
                             float maxFrequency = 2000,
                             float q = 0.5f,
                             float attackTime = 0.01f,
                             float releaseTime = 0.05f)
        {
            _fs = samplingRate;

            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
            Q = q;

            _envelopeFollower = new EnvelopeFollower(samplingRate, attackTime, releaseTime);
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var env = _envelopeFollower.Process(sample) * Math.Sqrt(Q);

            var frequencyRange = Math.PI * (MaxFrequency - MinFrequency) / _fs;
            var minFreq = Math.PI * MinFrequency / _fs;

            var centerFrequency = env * frequencyRange + minFreq;

            var f = (float)(2 * Math.Sin(centerFrequency));

            _yh = sample - _yl - Q * _yb;
            _yb += f * _yh;
            _yl += f * _yb;

            return Wet * _yb + Dry * sample;
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
            double env, frequencyRange, minFreq, centerFrequency;
            float f;
            bool result;
            float t_Dry = Dry;
            float t_Wet = Wet;
            float t_MaxFrequency = MaxFrequency;
            float t_MinFrequency = MinFrequency;
            float t_Q = Q;
            float t_Sqrt_Q = (float)Math.Sqrt(t_Q);

            result = false;

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
                        env = _envelopeFollower.Process(*p) * t_Sqrt_Q;
                        frequencyRange = Math.PI * (t_MaxFrequency - t_MinFrequency) / _fs;
                        minFreq = Math.PI * t_MinFrequency / _fs;
                        centerFrequency = env * frequencyRange + minFreq;
                        f = (float)(2 * Math.Sin(centerFrequency));
                        _yh = *p - _yl - t_Q * _yb;
                        _yb += f * _yh;
                        _yl += f * _yb;
                        *p = t_Wet * _yb + t_Dry * *p;

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
            _yh = _yl = _yb = 0;
            _envelopeFollower.Reset();
        }

        private float _yh, _yb, _yl;
    }
}
