using NWaves.Effects.Base;
using NWaves.Signals.Builders;
using NWaves.Signals.Builders.Base;
using NWaves.Utils;
using System; //                                                                                                  2022-04-20: J.P.B.
using System.Diagnostics; //                                                                                      2022-04-20: J.P.B.

namespace NWaves.Effects
{
    /// <summary>
    /// Represents Vibrato audio effect.
    /// </summary>
    public class VibratoEffect : AudioEffect
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
        /// Gets or sets width (in seconds).
        /// </summary>
        public float Width
        {
            get => _width;
            set
            {
                _delayLine.Ensure(_fs, value);
                _width = value;
            }
        }
        private float _width;

        /// <summary>
        /// Gets or sets LFO frequency (in Hz).
        /// </summary>
        public float LfoFrequency
        {
            get => _lfoFrequency;
            set
            {
                _lfoFrequency = value;
                _lfo.SetParameter("freq", value);
            }
        }
        private float _lfoFrequency = 1;

        /// <summary>
        /// Gets or sets LFO signal generator.
        /// </summary>
        public SignalBuilder Lfo
        {
            get => _lfo;
            set
            {
                _lfo = value;
                _lfo.SetParameter("min", 0.0).SetParameter("max", 1.0);
            }
        }
        private SignalBuilder _lfo;

        /// <summary>
        /// Gets or sets interpolation mode.
        /// </summary>
        public InterpolationMode InterpolationMode
        {
            get => _delayLine.InterpolationMode;
            set => _delayLine.InterpolationMode = value;
        }

        /// <summary>
        /// Constructs <see cref="VibratoEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfoFrequency">LFO frequency (in Hz)</param>
        /// <param name="width">Width (in seconds)</param>
        /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
        /// <param name="reserveWidth">Max width (in seconds) for reserving the size of delay line</param>
        public VibratoEffect(int samplingRate,
                             float lfoFrequency = 1/*Hz*/,
                             float width = 0.003f/*sec*/,
                             InterpolationMode interpolationMode = InterpolationMode.Linear,
                             float reserveWidth = 0/*sec*/)

            : this(samplingRate, new SineBuilder().SampledAt(samplingRate), width, interpolationMode, reserveWidth)
        {
            LfoFrequency = lfoFrequency;
        }

        /// <summary>
        /// Constructs <see cref="VibratoEffect"/> from <paramref name="lfo"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfo">LFO signal generator</param>
        /// <param name="width">Width (in seconds)</param>
        /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
        /// <param name="reserveWidth">Max width (in seconds) for reserving the size of delay line</param>
        public VibratoEffect(int samplingRate,
                             SignalBuilder lfo,
                             float width = 0.003f/*sec*/,
                             InterpolationMode interpolationMode = InterpolationMode.Linear,
                             float reserveWidth = 0/*sec*/)
        {
            _fs = samplingRate;
            _width = width;

            Lfo = lfo;

            if (reserveWidth < width)
            {
                _delayLine = new FractionalDelayLine(samplingRate, width, interpolationMode);
            }
            else
            {
                _delayLine = new FractionalDelayLine(samplingRate, reserveWidth, interpolationMode);
            }
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var delay = _lfo.NextSample() * _width * _fs;

            var delayedSample = _delayLine.Read(delay);

            _delayLine.Write(sample);

            return Dry * sample + Wet * delayedSample;
        }

        /// <summary>
        /// Processes a buffer of (possibly) interleaved samples for a single channel.                            2022-05-03: Start    J.P.B.
        /// </summary>
        /// <param name="sampleBuffer">audio sample buffer</param>
        /// <param name="Channel">Channel #: 1 to MAX_CHANNELS</param>
        /// <param name="nChannels"># of interleaved Channels in buffer: 1 to MAX_CHANNELS</param>
        /// <param name="frameCount"># of frames (sample groups) in buffer: 1 to MAX_FRAME_COUNT </param>
        public bool ProcessSampleBuffer(in IntPtr sampleBuffer, in int Channel, in int nChannels, in int frameCount)
        {
            float delay, delayedSample;
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
                        delay = _lfo.NextSample() * _width * _fs;
                        delayedSample = _delayLine.Read(delay);
                        _delayLine.Write(*p);
                        *p = t_Dry * *p + t_Wet * delayedSample;
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

        } //                                                                                                      2022-05-03: End

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
            _delayLine.Reset();
            _lfo.Reset();
        }
    }
}
