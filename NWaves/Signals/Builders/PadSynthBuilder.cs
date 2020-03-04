using NWaves.Transforms;
using System;
using System.Linq;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class providing implementation of the PADSynth algorithm by Nasca Octavian Paul
    /// (https://zynaddsubfx.sourceforge.io/doc/PADsynth/PADsynth.htm)
    /// </summary>
    public class PadSynthBuilder : WaveTableBuilder
    {
        private readonly Random _rand = new Random();

        protected float _frequency;
        protected float[] _amplitudes;

        protected float _bw = 40;
        protected float _bwScale = 1.25f;

        protected RealFft _fft;
        protected int _fftSize;
        protected float[] _re;
        protected float[] _im;

        public PadSynthBuilder() : base(null)
        {
            ParameterSetters.Add("frequency, freq, f", param => SetFrequency((float)param));
            ParameterSetters.Add("fftsize, size", param => SetFftSize((int)param));
            ParameterSetters.Add("bandwidth, bw", param => SetBandwidth((float)param));
            ParameterSetters.Add("scale, bwscale", param => SetScale((float)param));
        }

        protected void SetFrequency(float frequency)
        {
            _frequency = frequency;
            GenerateWavetable();
        }

        protected void SetFftSize(int fftSize)
        {
            _fftSize = fftSize;

            if (_fft == null || _fft.Size != _fftSize)
            {
                _fft = new RealFft(_fftSize);

                _re = new float[_fftSize];
                _im = new float[_fftSize];

                _samples = new float[_fftSize];
            }

            GenerateWavetable();
        }

        protected void SetBandwidth(float bw)
        {
            _bw = bw;
            GenerateWavetable();
        }

        protected void SetScale(float bwScale)
        {
            _bwScale = bwScale;
            GenerateWavetable();
        }

        internal void SetAmplitudeArray(float[] amplitudes)
        {
            _amplitudes = amplitudes;
            GenerateWavetable();
        }

        protected void GenerateWavetable()
        {
            if (_fft == null || _amplitudes == null || _frequency == 0)
            {
                return;
            }

            Array.Clear(_re, 0, _re.Length);
            Array.Clear(_im, 0, _im.Length);

            var fftHalfSize = _fftSize / 2;

            // synthesize spectrum:

            for (var i = 1; i <= _amplitudes.Length; i++)
            {
                if (_amplitudes[i - 1] == 0) continue;

                var bwHz = (Math.Pow(2, _bw / 1200) - 1.0) * _frequency * Math.Pow(i, _bwScale);
                var fi = _frequency * i / SamplingRate;
                var bwi = bwHz / (2.0 * SamplingRate);

                var s = (int)(fi * fftHalfSize);
                
                if (s >= fftHalfSize) continue;

                var h = 1.0;
                var j = s;
                while (h > 1e-10)
                {
                    h = Profile(1.0 * j / fftHalfSize - fi, bwi);
                    _re[j--] += (float)h * _amplitudes[i - 1];
                }
                h = 1.0;
                j = s + 1;
                while (h > 1e-10)
                {
                    h = Profile(1.0 * j / fftHalfSize - fi, bwi);
                    _re[j++] += (float)h * _amplitudes[i - 1];
                }
            }

            // generate samples from synthesized spectrum:

            for (var i = 0; i < _re.Length; i++)
            {
                var mag = _re[i];
                var phase = _rand.NextDouble() * 2 * Math.PI;

                _re[i] = (float)(mag * Math.Cos(phase));
                _im[i] = (float)(mag * Math.Sin(phase));
            }

            _fft.Inverse(_re, _im, _samples);

            var norm = 1 / _samples.Max();

            for (var i = 0; i < _samples.Length; _samples[i++] *= norm) ;
        }

        protected double Profile(double f, double bw)
        {
            var x = f / bw;
            return Math.Exp(-x * x) / bw;
        }

        public override SignalBuilder SampledAt(int samplingRate)
        {
            if (samplingRate <= 0)
            {
                throw new ArgumentException("Sampling rate must be positive!");
            }

            SamplingRate = samplingRate;
            GenerateWavetable();
            return this;
        }
    }

    public static class PadSynthBuilderExtensions
    {
        public static SignalBuilder SetAmplitudes(this SignalBuilder builder, float[] amplitudes)
        {
            PadSynthBuilder padSynth = builder as PadSynthBuilder;

            if (padSynth == null)
            {
                return builder;
            }

            padSynth.SetAmplitudeArray(amplitudes);
            
            return padSynth;
        }
    }
}
