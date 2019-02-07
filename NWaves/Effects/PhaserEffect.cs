using NWaves.Filters.BiQuad;
using NWaves.Signals.Builders;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for phaser effect
    /// </summary>
    public class PhaserEffect : AudioEffect
    {
        /// <summary>
        /// Q
        /// </summary>
        public float Q { get; }

        /// <summary>
        /// LFO frequency
        /// </summary>
        public float LfoFrequency { get; }

        /// <summary>
        /// Min LFO frequency
        /// </summary>
        public float MinFrequency { get; }

        /// <summary>
        /// Max LFO frequency
        /// </summary>
        public float MaxFrequency { get; }

        /// <summary>
        /// Sampling rate
        /// </summary>
        private int _fs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="lfoFrequency"></param>
        /// <param name="minFrequency"></param>
        /// <param name="maxFrequency"></param>
        /// <param name="q"></param>
        public PhaserEffect(int samplingRate, float lfoFrequency = 1.0f, float minFrequency = 300, float maxFrequency = 3000, float q = 0.5f)
        {
            _fs = samplingRate;
            LfoFrequency = lfoFrequency;
            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
            Q = q;

            _lfo = new TriangleWaveBuilder()
                                .SetParameter("lo", MinFrequency)
                                .SetParameter("hi", MaxFrequency)
                                .SetParameter("freq", LfoFrequency)
                                .SampledAt(samplingRate);
        }

        /// <summary>
        /// Method implements simple phaser effect
        /// </summary>
        /// <param name="sample">Input sample</param>
        /// <returns>Output sample</returns>
        public override float Process(float sample)
        {
            NotchFilter.MakeTf(_lfo.NextSample() / _fs, Q, _b, _a);

            var output = (float)((_b[0] * sample + _b[1] * _in1 + _b[2] * _in2 - _a[1] * _out1 - _a[2] * _out2) / _a[0]);

            _in2 = _in1;
            _in1 = sample;
            _out2 = _out1;
            _out1 = output;

            return output * Wet + sample * Dry;
        }

        public override void Reset()
        {
            _in1 = _in2 = _out1 = _out2 = 0;
        }

        private SignalBuilder _lfo;

        private double[] _b = new double[3];
        private double[] _a = new double[3];

        /// <summary>
        /// Delay line
        /// </summary>
        private float _in1;
        private float _in2;
        private float _out1;
        private float _out2;
    }
}
