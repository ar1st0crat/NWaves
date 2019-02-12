using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Operations
{
    /// <summary>
    /// Class that implements Spectral subtraction algorithm according to
    /// 
    /// [1979] M. Berouti, R. Schwartz, J. Makhoul
    /// "Enhancement of Speech Corrupted by Acoustic Noise".
    /// 
    /// </summary>
    public class SpectralSubtractor : IFilter
    {
        /// <summary>
        /// Hop size
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Size of FFT for analysis and synthesis
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Internal FFT transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Window coefficients
        /// </summary>
        private readonly float[] _window;

        /// <summary>
        /// Window coefficients squared
        /// </summary>
        private readonly float[] _windowSquared;

        /// <summary>
        /// Accumulated noise
        /// </summary>
        private readonly float[] _noiseAcc;

        /// <summary>
        /// Noise estimate
        /// </summary>
        private readonly float[] _noiseEstimate;

        /// <summary>
        /// Internal buffer for real parts of analyzed block
        /// </summary>
        private float[] _re;

        /// <summary>
        /// Internal buffer for imaginary parts of analyzed block
        /// </summary>
        private float[] _im;

        /// <summary>
        /// Internal array of zeros for a quick memset
        /// </summary>
        private readonly float[] _zeroblock;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="noise"></param>
        /// <param name="fftSize"></param>
        /// <param name="hopSize"></param>
        public SpectralSubtractor(DiscreteSignal noise, int fftSize = 1024, int hopSize = 410)
        {
            _fftSize = fftSize;
            _hopSize = hopSize;

            _fft = new Fft(_fftSize);
            _window = Window.OfType(WindowTypes.Hann, _fftSize);
            _windowSquared = _window.Select(w => w * w).ToArray();

            _noiseAcc = new float[_fftSize / 2 + 1];
            _noiseEstimate = new float[_fftSize / 2 + 1];

            _re = new float[_fftSize];
            _im = new float[_fftSize];
            _zeroblock = new float[_fftSize];

            EstimateNoise(noise);
        }

        /// <summary>
        /// Estimate noise power spectrum
        /// </summary>
        /// <param name="noise"></param>
        private void EstimateNoise(DiscreteSignal noise)
        {
            var numFrames = 0;
            
            for (var pos = 0; pos + _fftSize < noise.Length; pos += _hopSize, numFrames++)
            {
                noise.Samples.FastCopyTo(_re, _fftSize, pos);
                _zeroblock.FastCopyTo(_im, _fftSize);

                _fft.Direct(_re, _im);

                for (var j = 0; j <= _fftSize / 2; j++)
                {
                    _noiseAcc[j] += _re[j] * _re[j] + _im[j] * _im[j];
                }
            }

            // (including smoothing)

            for (var j = 1; j < _fftSize / 2; j++)
            {
                _noiseEstimate[j] = (_noiseAcc[j - 1] + _noiseAcc[j] + _noiseAcc[j + 1]) / (3 * numFrames);
            }

            _noiseEstimate[0] /= numFrames;
            _noiseEstimate[_fftSize / 2] /= numFrames;
        }

        /// <summary>
        /// Spectral subtraction
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringMethod method = FilteringMethod.Auto)
        {
            var input = signal.Samples;
            var output = new float[input.Length];

            const float beta = 0.009f;
            const float alphaMin = 2f;
            const float alphaMax = 5f;
            const float snrMin = -5f;
            const float snrMax = 20f;

            const float k = (alphaMin - alphaMax) / (snrMax - snrMin);
            const float b = alphaMax - k * snrMin;

            var windowSum = new float[output.Length];

            for (var pos = 0; pos + _fftSize < input.Length; pos += _hopSize)
            {
                input.FastCopyTo(_re, _fftSize, pos);
                _zeroblock.FastCopyTo(_im, _fftSize);

                _re.ApplyWindow(_window);

                _fft.Direct(_re, _im);

                for (var j = 0; j <= _fftSize / 2; j++)
                {
                    var power = _re[j] * _re[j] + _im[j] * _im[j];
                    var phase = Math.Atan2(_im[j], _re[j]);

                    var noisePower = _noiseEstimate[j];

                    var snr = 10 * Math.Log10(power / noisePower);
                    var alpha = Math.Max(Math.Min(k * snr + b, alphaMax), alphaMin);

                    var diff = power - alpha * noisePower;

                    var mag = Math.Sqrt(Math.Max(diff, beta * noisePower));

                    _re[j] = (float)(mag * Math.Cos(phase));
                    _im[j] = (float)(mag * Math.Sin(phase));
                }

                for (var j = _fftSize / 2 + 1; j < _fftSize; j++)
                {
                    _re[j] = _im[j] = 0.0f;
                }

                _fft.Inverse(_re, _im);

                for (var j = 0; j < _re.Length; j++)
                {
                    output[pos + j] += _re[j] * _window[j];
                    windowSum[pos + j] += _windowSquared[j];
                }
            }

            for (var j = 0; j < output.Length; j++)
            {
                if (windowSum[j] < 1e-3) continue;
                output[j] /= (windowSum[j] * _fftSize / 2);
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
