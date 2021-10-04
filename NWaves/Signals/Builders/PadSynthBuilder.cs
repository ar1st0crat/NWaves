using NWaves.Signals.Builders.Base;
using NWaves.Transforms;
using System;
using System.Linq;

namespace NWaves.Signals.Builders
{
    // PADSynth algorithm by Nasca Octavian Paul:
    //
    // (https://zynaddsubfx.sourceforge.io/doc/PADsynth/PADsynth.htm)
    // 

    /// <summary>
    /// Represents builder of signals that uses PadSynth algorithm.
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"frequency", "freq", "f" (default: 440.0 Hz)</item>
    ///     <item>"fftsize", "size" (default: 2048)</item>
    ///     <item>"bandwidth", "bw" (default: 40)</item>
    ///     <item>"bwscale", "scale" (default: 1.25)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class PadSynthBuilder : WaveTableBuilder
    {
        private readonly Random _rand = new Random();

        /// <summary>
        /// Frequency of the note.
        /// </summary>
        protected float _frequency = 440;/*Hz*/

        /// <summary>
        /// Amplitudes of harmonics.
        /// </summary>
        protected float[] _amplitudes;

        /// <summary>
        /// Bandwidth of the first harmonic.
        /// </summary>
        protected float _bw = 40;

        /// <summary>
        /// how much the bandwidth of the harmonic increase according to it's frequency
        /// </summary>
        protected float _bwScale = 1.25f;

        /// <summary>
        /// Internal FFT transformer.
        /// </summary>
        protected RealFft _fft;
        
        /// <summary>
        /// FFT size.
        /// </summary>
        protected int _fftSize = 2048;
        
        /// <summary>
        /// Internal buffer for real parts of spectrum.
        /// </summary>
        protected float[] _re;

        /// <summary>
        /// Internal buffer for imaginary parts of spectrum. 
        /// </summary>
        protected float[] _im;

        /// <summary>
        /// Constructs <see cref="PadSynthBuilder"/>.
        /// </summary>
        public PadSynthBuilder() : base(null)
        {
            ParameterSetters.Add("frequency, freq, f", param => SetFrequency((float)param));
            ParameterSetters.Add("fftsize, size", param => SetFftSize((int)param));
            ParameterSetters.Add("bandwidth, bw", param => SetBandwidth((float)param));
            ParameterSetters.Add("scale, bwscale", param => SetScale((float)param));
        }

        /// <summary>
        /// Sets frequency of the note.
        /// </summary>
        /// <param name="frequency">Frequency</param>
        protected void SetFrequency(float frequency)
        {
            _frequency = frequency;
            GenerateWavetable();
        }

        /// <summary>
        /// Sets FFT size. Must be power of 2.
        /// </summary>
        /// <param name="fftSize">FFT size</param>
        protected void SetFftSize(int fftSize)
        {
            _fftSize = fftSize;

            if (_fft is null || _fft.Size != _fftSize)
            {
                _fft = new RealFft(_fftSize);

                _re = new float[_fftSize];
                _im = new float[_fftSize];

                _samples = new float[_fftSize];
            }

            GenerateWavetable();
        }

        /// <summary>
        /// Sets bandwidth.
        /// </summary>
        /// <param name="bw">Bandwidth</param>
        protected void SetBandwidth(float bw)
        {
            _bw = bw;
            GenerateWavetable();
        }

        /// <summary>
        /// Sets the bandwidth scale parameter.
        /// </summary>
        /// <param name="bwScale">Bandwidth scale</param>
        protected void SetScale(float bwScale)
        {
            _bwScale = bwScale;
            GenerateWavetable();
        }

        /// <summary>
        /// Sets amplitudes of harmonics.
        /// </summary>
        /// <param name="amplitudes">Array of amplitudes</param>
        public PadSynthBuilder SetAmplitudes(float[] amplitudes)
        {
            _amplitudes = amplitudes;
            GenerateWavetable();

            return this;
        }

        /// <summary>
        /// Generates wave table using PadSynth algorithm.
        /// </summary>
        protected void GenerateWavetable()
        {
            if (_fft is null || _amplitudes is null || _frequency <= 0)
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

        /// <summary>
        /// Helper method used in PadSynth algorithm.
        /// </summary>
        /// <param name="f">Frequency of the note</param>
        /// <param name="bw">Bandwidth</param>
        protected static double Profile(double f, double bw)
        {
            var x = f / bw;
            return Math.Exp(-x * x) / bw;
        }

        /// <summary>
        /// Sets the sampling rate of the signal to build.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
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
}
