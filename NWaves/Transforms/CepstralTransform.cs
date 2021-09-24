using NWaves.Transforms.Base;
using NWaves.Utils;
using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// <para>Class representing Cepstral Transform (CT):</para>
    /// <list type="number">
    ///     <item>Direct Complex CT (complex cepstrum)</item>
    ///     <item>Inverse Complex CT</item>
    ///     <item>Real cepstrum</item>
    ///     <item>Power cepstrum</item>
    ///     <item>Phase cepstrum</item>
    /// </list>
    /// <para>1,2) and 3) are identical to MATLAB functions cceps, icceps and rceps, respectively.</para>
    /// <para><see cref="CepstralTransform"/> operates on real-valued data.</para>
    /// </summary>
    public class CepstralTransform : ITransform
    {
        /// <summary>
        /// Gets cepstrum size.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// FFT transformer.
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Logarithm base (E or 10).
        /// </summary>
        private readonly double _logBase;

        /// <summary>
        /// Internal array for real parts of spectrum.
        /// </summary>
        private readonly float[] _re;

        /// <summary>
        /// Internal array for imaginary parts of spectrum.
        /// </summary>
        private readonly float[] _im;

        /// <summary>
        /// Internal array for storing the unwrapped phase.
        /// </summary>
        private readonly double[] _unwrapped;

        /// <summary>
        /// Construct cepstral transformer. 
        /// If <paramref name="cepstrumSize"/> exceeds <paramref name="fftSize"/>, 
        /// FFT size will be recalculated as the nearest power of 2 to cepstrum size.
        /// </summary>
        /// <param name="cepstrumSize">Cepstrum size</param>
        /// <param name="fftSize">FFT size</param>
        /// <param name="logBase">Logarithm base</param>
        public CepstralTransform(int cepstrumSize, int fftSize = 0, double logBase = Math.E)
        {
            Size = cepstrumSize;

            if (cepstrumSize > fftSize)
            {
                fftSize = MathUtils.NextPowerOfTwo(cepstrumSize);
            }

            _fft = new Fft(fftSize);

            _logBase = logBase;

            _re = new float[fftSize];
            _im = new float[fftSize];
            _unwrapped = new double[fftSize];
        }

        /// <summary>
        /// Evaluate complex cepstrum as:
        /// <code>
        ///    Real{IFFT(log(abs(FFT(x)) + unwrapped_phase))}
        /// </code>
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="cepstrum">Complex cepstrum</param>
        /// <param name="normalize">Normalize cepstrum by FFT size</param>
        /// <returns>Circular delay (number of samples) added to <paramref name="input"/></returns>
        public double ComplexCepstrum(float[] input, float[] cepstrum, bool normalize = true)
        {
            Array.Clear(_re, 0, _re.Length);
            Array.Clear(_im, 0, _im.Length);

            input.FastCopyTo(_re, input.Length);

            // complex fft

            _fft.Direct(_re, _im);

            // complex logarithm of magnitude spectrum

            // the most difficult part is phase unwrapping which is slightly different from MathUtils.Unwrap

            var offset = 0.0;
            _unwrapped[0] = 0.0;

            var prevPhase = Math.Atan2(_im[0], _re[0]);

            for (var n = 1; n < _unwrapped.Length; n++)
            {
                var phase = Math.Atan2(_im[n], _re[n]);

                var delta = phase - prevPhase;

                if (delta > Math.PI)
                {
                    offset -= 2 * Math.PI;
                }
                else if (delta < -Math.PI)
                {
                    offset += 2 * Math.PI;
                }

                _unwrapped[n] = phase + offset;
                prevPhase = phase;
            }

            var mid = _re.Length / 2;
            var delay = Math.Round(_unwrapped[mid] / Math.PI);

            for (var i = 0; i < _re.Length; i++)
            {
                _unwrapped[i] -= Math.PI * delay * i / mid;

                var mag = Math.Sqrt(_re[i] * _re[i] + _im[i] * _im[i]);

                _re[i] = (float)Math.Log(mag + float.Epsilon, _logBase);
                _im[i] = (float)_unwrapped[i];
            }

            // complex ifft

            _fft.Inverse(_re, _im);

            // take truncated part

            _re.FastCopyTo(cepstrum, Size);

            if (normalize)
            {
                for (var i = 0; i < cepstrum.Length; i++)
                {
                    cepstrum[i] /= _fft.Size;
                }
            }

            return delay;
        }

        /// <summary>
        /// Evaluate inverse complex cepstrum of <paramref name="input"/> (removing <paramref name="delay"/> samples).
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="cepstrum">Inverse complex cepstrum</param>
        /// <param name="normalize">Normalize result by FFT size</param>
        /// <param name="delay">Delay (usually, returned by function <see cref="ComplexCepstrum(float[], float[], bool)"/>)</param>
        public void InverseComplexCepstrum(float[] input, float[] cepstrum, bool normalize = true, double delay = 0)
        {
            Array.Clear(_re, 0, _re.Length);
            Array.Clear(_im, 0, _im.Length);

            input.FastCopyTo(_re, input.Length);

            // complex fft

            _fft.Direct(_re, _im);

            // complex exp() of spectrum

            var mid = _re.Length / 2;

            for (var i = 0; i < _re.Length; i++)
            {
                var mag = _re[i];
                var phase = _im[i] + Math.PI * delay * i / mid;

                _re[i] = (float)(Math.Pow(_logBase, mag) * Math.Cos(phase));
                _im[i] = (float)(Math.Pow(_logBase, mag) * Math.Sin(phase));
            }

            // complex ifft

            _fft.Inverse(_re, _im);

            // take truncated part

            _re.FastCopyTo(cepstrum, cepstrum.Length);

            if (normalize)
            {
                for (var i = 0; i < cepstrum.Length; i++)
                {
                    cepstrum[i] /= _fft.Size;
                }
            }
        }

        /// <summary>
        /// Evaluate real cepstrum as:
        /// <code>
        ///    real{IFFT(log(abs(FFT(x))))}
        /// </code>
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="cepstrum">Real cesptrum</param>
        /// <param name="normalize">Normalize cepstrum by FFT size</param>
        public void RealCepstrum(float[] input, float[] cepstrum, bool normalize = true)
        {
            Array.Clear(_re, 0, _re.Length);
            Array.Clear(_im, 0, _im.Length);

            input.FastCopyTo(_re, input.Length);

            // complex fft

            _fft.Direct(_re, _im);

            // logarithm of magnitude spectrum

            for (var i = 0; i < _re.Length; i++)
            {
                var mag = Math.Sqrt(_re[i] * _re[i] + _im[i] * _im[i]);

                _re[i] = (float)Math.Log(mag + float.Epsilon, _logBase);
                _im[i] = 0.0f;
            }

            // complex ifft

            _fft.Inverse(_re, _im);

            // take truncated part

            _re.FastCopyTo(cepstrum, Size);

            if (normalize)
            {
                for (var i = 0; i < cepstrum.Length; i++)
                {
                    cepstrum[i] /= _fft.Size;
                }
            }
        }

        /// <summary>
        /// Evaluate power cepstrum as: 
        /// <code>
        ///    power_cepstrum = 4 * real_cepstrum ^ 2
        /// </code>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cepstrum"></param>
        /// <param name="normalize"></param>
        public void PowerCepstrum(float[] input, float[] cepstrum, bool normalize = true)
        {
            RealCepstrum(input, cepstrum, normalize);

            for (var i = 0; i < cepstrum.Length; i++)
            {
                var pc = 4 * cepstrum[i] * cepstrum[i];

                cepstrum[i] = pc;
            }
        }

        /// <summary>
        /// Evaluate phase cepstrum as: 
        /// <code>
        ///     phase_cepstrum = (complex_cepstrum - reversed_complex_cepstrum) ^ 2
        /// </code>
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="cepstrum">Phase cepstrum</param>
        /// <param name="normalize">Normalize cepstrum by FFT size</param>
        public void PhaseCepstrum(float[] input, float[] cepstrum, bool normalize = true)
        {
            ComplexCepstrum(input, cepstrum, normalize);

            // use this free memory block for storing reversed cepstrum
            cepstrum.FastCopyTo(_re, cepstrum.Length);

            for (var i = 0; i < cepstrum.Length; i++)
            {
                var pc = cepstrum[i] - _re[cepstrum.Length - 1 - i];

                cepstrum[i] = pc * pc;
            }
        }

        /// <summary>
        /// Do cepstral transform. 
        /// It simply calls <see cref="ComplexCepstrum(float[], float[], bool)"/> ignoring the delay parameter.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Direct(float[] input, float[] output) => ComplexCepstrum(input, output, false);

        /// <summary>
        /// Do normalized cepstral transform. 
        /// It simply calls <see cref="ComplexCepstrum(float[], float[], bool)"/> ignoring the delay parameter.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void DirectNorm(float[] input, float[] output) => ComplexCepstrum(input, output);

        /// <summary>
        /// Do inverse cepstral transform. 
        /// It simply calls <see cref="InverseComplexCepstrum(float[], float[], bool, double)"/> ignoring the delay parameter.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Inverse(float[] input, float[] output) => InverseComplexCepstrum(input, output, false);

        /// <summary>
        /// Do normalized inverse cepstral transform. 
        /// It simply calls <see cref="InverseComplexCepstrum(float[], float[], bool, double)"/> ignoring the delay parameter.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void InverseNorm(float[] input, float[] output) => InverseComplexCepstrum(input, output);
    }
}
