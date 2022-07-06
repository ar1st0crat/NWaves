using NWaves.Effects.Base;
using NWaves.Signals.Builders;
using NWaves.Signals.Builders.Base;
using System; //                                                                                                  2022-04-20: J.P.B.
using System.Diagnostics; //                                                                                      2022-04-20: J.P.B.

namespace NWaves.Effects
{
    /// <summary>
    /// Represents Tremolo audio effect.
    /// </summary>
    public class TremoloEffect : AudioEffect
    {
        /// <summary>
        /// Gets or sets depth.
        /// </summary>
        public float Depth { get; set; }

        /// <summary>
        /// Gets or sets tremolo frequency (modulation frequency) (in Hz).
        /// </summary>
        public float Frequency
        {
            get => _frequency;
            set
            {
                _frequency = value;
                Lfo.SetParameter("freq", value);
            }
        }
        private float _frequency;

        /// <summary>
        /// Gets or sets tremolo index (modulation index).
        /// </summary>
        public float Index
        {
            get => _index;
            set
            {
                _index = value;
                Lfo.SetParameter("min", 0).SetParameter("max", value * 2);
            }
        }
        private float _index;

        /// <summary>
        /// Gets or sets LFO signal generator.
        /// </summary>
        public SignalBuilder Lfo { get; set; }

        /// <summary>
        /// Constructs <see cref="TremoloEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="depth">Depth</param>
        /// <param name="frequency">Tremolo frequency (modulation frequency) (in Hz)</param>
        /// <param name="tremoloIndex">Tremolo index (modulation index)</param>
        public TremoloEffect(int samplingRate, float depth = 0.5f, float frequency = 10/*Hz*/, float tremoloIndex = 0.5f)
        {
            Lfo = new CosineBuilder().SampledAt(samplingRate);

            Depth = depth;
            Frequency = frequency;
            Index = tremoloIndex;
        }

        /// <summary>
        /// Constructs <see cref="TremoloEffect"/> from <paramref name="lfo"/>.
        /// </summary>
        /// <param name="lfo">LFO signal generator</param>
        /// <param name="depth">Depth</param>
        public TremoloEffect(SignalBuilder lfo, float depth = 0.5f)
        {
            Lfo = lfo;
            Depth = depth;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var output = sample * (1 - Depth + Depth * Lfo.NextSample());

            return output * Wet + sample * Dry;
        }

        /// <summary>
        /// Processes a buffer of (possibly) interleaved samples for a single channel.                            2022-04-28: Start    J.P.B.
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
            float t_Depth = Depth;

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
                        output = *p * (1 - t_Depth + t_Depth * Lfo.NextSample());
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

        } //                                                                                                      2022-04-28: End

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
            Lfo.Reset();
        }
    }
}
