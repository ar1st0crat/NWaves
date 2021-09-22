using System;
using NWaves.Signals;
using NWaves.Transforms.Base;
using NWaves.Utils;

namespace NWaves.Transforms
{
    /// <summary>
    /// <para>Class representing Cepstral Transform (CT):</para>
    /// <list type="number">
    ///     <item>Direct Complex CT</item>
    ///     <item>Inverse Complex CT</item>
    ///     <item>Real cepstrum</item>
    ///     <item>Power cepstrum</item>
    ///     <item>Phase cepstrum</item>
    /// </list>
    /// <para>1,2) and 3) are identical to MATLAB cceps/icceps and rceps, respectively.</para>
    /// </summary>
    public class CepstralTransform : IComplexTransform
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
        /// Intermediate buffer storing real parts of spectrum.
        /// </summary>
        private readonly float[] _realSpectrum;

        /// <summary>
        /// Intermediate buffer storing imaginary parts of spectrum.
        /// </summary>
        private readonly float[] _imagSpectrum;

        /// <summary>
        /// Intermediate buffer storing unwrapped phase.
        /// </summary>
        private readonly double[] _unwrapped;

        /// <summary>
        /// Construct cepstral transformer.
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

            _realSpectrum = new float[fftSize];
            _imagSpectrum = new float[fftSize];
            _unwrapped = new double[fftSize];
        }

        /// <summary>
        /// Do direct complex cepstral transform:
        /// <code>
        ///    Real{IFFT(log(abs(FFT(x)) + unwrapped_phase))}
        /// </code>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cepstrum"></param>
        /// <returns></returns>
        public double Direct(float[] input, float[] cepstrum)
        {
            Array.Clear(_realSpectrum, 0, _realSpectrum.Length);
            Array.Clear(_imagSpectrum, 0, _imagSpectrum.Length);

            input.FastCopyTo(_realSpectrum, input.Length);

            // complex fft

            _fft.Direct(_realSpectrum, _imagSpectrum);

            // complex logarithm of magnitude spectrum

            // the most difficult part is phase unwrapping which is slightly different from MathUtils.Unwrap
            
            var offset = 0.0;
            _unwrapped[0] = 0.0;

            var prevPhase = Math.Atan2(_imagSpectrum[0], _realSpectrum[0]);

            for (var n = 1; n < _unwrapped.Length; n++)
            {
                var phase = Math.Atan2(_imagSpectrum[n], _realSpectrum[n]);

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

            var mid = _realSpectrum.Length / 2;
            var delay = Math.Round(_unwrapped[mid] / Math.PI);

            for (var i = 0; i < _realSpectrum.Length; i++)
            {
                _unwrapped[i] -= Math.PI * delay * i / mid;

                var mag = Math.Sqrt(_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]);

                _realSpectrum[i] = (float)Math.Log(mag + float.Epsilon, _logBase);
                _imagSpectrum[i] = (float)_unwrapped[i];
            }

            // complex ifft

            _fft.Inverse(_realSpectrum, _imagSpectrum);

            // take truncated part

            _realSpectrum.FastCopyTo(cepstrum, Size);

            // normalize

            for (var i = 0; i < cepstrum.Length; i++)
            {
                cepstrum[i] /= _fft.Size;
            }

            return delay;
        }

        /// <summary>
        /// Direct complex cepstral transform
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public DiscreteSignal Direct(DiscreteSignal signal)
        {
            var cepstrum = new float[Size];
            Direct(signal.Samples, cepstrum);
            return new DiscreteSignal(signal.SamplingRate, cepstrum);
        }

        /// <summary>
        /// Inverse complex cepstral transform
        /// </summary>
        /// <param name="cepstrum"></param>
        /// <param name="output"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public void Inverse(float[] cepstrum, float[] output, double delay = 0)
        {
            Array.Clear(_realSpectrum, 0, _realSpectrum.Length);
            Array.Clear(_imagSpectrum, 0, _imagSpectrum.Length);

            cepstrum.FastCopyTo(_realSpectrum, cepstrum.Length);

            // complex fft

            _fft.Direct(_realSpectrum, _imagSpectrum);

            // complex exp() of spectrum

            var mid = _realSpectrum.Length / 2;

            for (var i = 0; i < _realSpectrum.Length; i++)
            {
                var mag = _realSpectrum[i];
                var phase = _imagSpectrum[i] + Math.PI * delay * i / mid;

                _realSpectrum[i] = (float)(Math.Pow(_logBase, mag) * Math.Cos(phase));
                _imagSpectrum[i] = (float)(Math.Pow(_logBase, mag) * Math.Sin(phase));
            }

            // complex ifft

            _fft.Inverse(_realSpectrum, _imagSpectrum);

            // take truncated part

            _realSpectrum.FastCopyTo(output, output.Length);

            // normalize

            for (var i = 0; i < output.Length; i++)
            {
                output[i] /= _fft.Size;
            }
        }

        /// <summary>
        /// Inverse complex cepstral transform
        /// </summary>
        /// <param name="cepstrum"></param>
        /// <returns></returns>
        public DiscreteSignal Inverse(DiscreteSignal cepstrum)
        {
            var output = new float[_realSpectrum.Length];
            Inverse(cepstrum.Samples, output);
            return new DiscreteSignal(cepstrum.SamplingRate, output);
        }

        /// <summary>
        /// Real cepstrum:
        /// 
        /// real{IFFT(log(abs(FFT(x))))}
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cepstrum"></param>
        public void RealCepstrum(float[] input, float[] cepstrum)
        {
            Array.Clear(_realSpectrum, 0, _realSpectrum.Length);
            Array.Clear(_imagSpectrum, 0, _imagSpectrum.Length);

            input.FastCopyTo(_realSpectrum, input.Length);

            // complex fft

            _fft.Direct(_realSpectrum, _imagSpectrum);

            // logarithm of magnitude spectrum

            for (var i = 0; i < _realSpectrum.Length; i++)
            {
                var mag = Math.Sqrt(_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]);

                _realSpectrum[i] = (float)Math.Log(mag + float.Epsilon, _logBase);
                _imagSpectrum[i] = 0.0f;
            }

            // complex ifft

            _fft.Inverse(_realSpectrum, _imagSpectrum);

            // take truncated part

            _realSpectrum.FastCopyTo(cepstrum, Size);

            // normalize

            for (var i = 0; i < cepstrum.Length; i++)
            {
                cepstrum[i] /= _fft.Size;
            }
        }

        /// <summary>
        /// Wiki:
        /// power_cepstrum = 4 * real_cepstrum ^ 2
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cepstrum"></param>
        public void PowerCepstrum(float[] input, float[] cepstrum)
        {
            RealCepstrum(input, cepstrum);

            for (var i = 0; i < cepstrum.Length; i++)
            {
                var pc = 4 * cepstrum[i] * cepstrum[i];

                cepstrum[i] = pc;
            }
        }

        /// <summary>
        /// Wiki:
        /// phase_cepstrum = (complex_cepstrum - reversed_complex_cepstrum) ^ 2
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cepstrum"></param>
        public void PhaseCepstrum(float[] input, float[] cepstrum)
        {
            Direct(input, cepstrum);

            // use this free memory block for storing reversed cepstrum
            cepstrum.FastCopyTo(_realSpectrum, cepstrum.Length);

            for (var i = 0; i < cepstrum.Length; i++)
            {
                var pc = cepstrum[i] - _realSpectrum[cepstrum.Length - 1 - i];

                cepstrum[i] = pc * pc;
            }
        }

        public void Direct(float[] inRe, float[] inIm, float[] outRe, float[] outIm)
        {
            throw new NotImplementedException();
        }

        public void DirectNorm(float[] inRe, float[] inIm, float[] outRe, float[] outIm)
        {
            throw new NotImplementedException();
        }

        public void Inverse(float[] inRe, float[] inIm, float[] outRe, float[] outIm)
        {
            throw new NotImplementedException();
        }

        public void InverseNorm(float[] inRe, float[] inIm, float[] outRe, float[] outIm)
        {
            throw new NotImplementedException();
        }
    }
}
