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
    public class SpectralSubtractor : IFilter, IOnlineFilter
    {
        // Algorithm parameters

        public float Beta { get; set; } = 0.009f;
        public float AlphaMin { get; set; } = 2f;
        public float AlphaMax { get; set; } = 5f;
        public float SnrMin { get; set; } = -5f;
        public float SnrMax { get; set; } = 20f;

        /// <summary>
        /// Hop size
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Size of FFT for analysis and synthesis
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Size of frame overlap
        /// </summary>
        private readonly int _overlapSize;

        /// <summary>
        /// Internal FFT transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Window coefficients
        /// </summary>
        private readonly float[] _window;
        
        /// <summary>
        /// ISTFT normalization gain
        /// </summary>
        private readonly float _gain;

        /// <summary>
        /// Accumulated noise
        /// </summary>
        private readonly float[] _noiseAcc;

        /// <summary>
        /// Noise estimate
        /// </summary>
        private readonly float[] _noiseEstimate;

        /// <summary>
        /// Delay line
        /// </summary>
        private float[] _dl;

        /// <summary>
        /// Offset in the input delay line
        /// </summary>
        private int _inOffset;

        /// <summary>
        /// Offset in the output buffer
        /// </summary>
        private int _outOffset;

        /// <summary>
        /// Internal buffers
        /// </summary>
        private float[] _re;
        private float[] _im;
        private float[] _filteredRe;
        private float[] _filteredIm;
        private float[] _zeroblock;
        private float[] _lastSaved;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="noise"></param>
        /// <param name="fftSize"></param>
        /// <param name="hopSize"></param>
        public SpectralSubtractor(DiscreteSignal noise, int fftSize = 1024, int hopSize = 128)
        {
            Guard.AgainstInvalidRange(hopSize, fftSize, "Hop size", "FFT size");

            _fftSize = fftSize;
            _hopSize = hopSize;
            _overlapSize = _fftSize - _hopSize;

            _fft = new Fft(_fftSize);
            _window = Window.OfType(WindowTypes.Hann, _fftSize);

            _gain = 2 / (_fftSize * _window.Select(w => w * w).Sum() / _hopSize);

            _noiseAcc = new float[_fftSize / 2 + 1];
            _noiseEstimate = new float[_fftSize / 2 + 1];

            _dl = new float[_fftSize];
            _re = new float[_fftSize];
            _im = new float[_fftSize];
            _filteredRe = new float[_fftSize];
            _filteredIm = new float[_fftSize];
            _zeroblock = new float[_fftSize];
            _lastSaved = new float[_overlapSize];

            EstimateNoise(noise);

            Reset();
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
        /// Online processing (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public float Process(float sample)
        {
            _dl[_inOffset++] = sample;

            if (_inOffset == _fftSize)
            {
                ProcessFrame();
            }

            return _filteredRe[_outOffset++] * _gain;
        }

        /// <summary>
        /// Process one frame (block)
        /// </summary>
        public void ProcessFrame()
        {
            float k = (AlphaMin - AlphaMax) / (SnrMax - SnrMin);
            float b = AlphaMax - k * SnrMin;

            _zeroblock.FastCopyTo(_im, _fftSize);
            _dl.FastCopyTo(_re, _fftSize);

            _re.ApplyWindow(_window);

            _fft.Direct(_re, _im);

            for (var j = 0; j <= _fftSize / 2; j++)
            {
                var power = _re[j] * _re[j] + _im[j] * _im[j];
                var phase = Math.Atan2(_im[j], _re[j]);

                var noisePower = _noiseEstimate[j];

                var snr = 10 * Math.Log10(power / noisePower);
                var alpha = Math.Max(Math.Min(k * snr + b, AlphaMax), AlphaMin);

                var diff = power - alpha * noisePower;

                var mag = Math.Sqrt(Math.Max(diff, Beta * noisePower));

                _filteredRe[j] = (float)(mag * Math.Cos(phase));
                _filteredIm[j] = (float)(mag * Math.Sin(phase));
            }

            for (var j = _fftSize / 2 + 1; j < _fftSize; j++)
            {
                _filteredRe[j] = _filteredIm[j] = 0.0f;
            }

            _fft.Inverse(_filteredRe, _filteredIm);

            _filteredRe.ApplyWindow(_window);

            for (var j = 0; j < _overlapSize; j++)
            {
                _filteredRe[j] += _lastSaved[j];
            }

            _filteredRe.FastCopyTo(_lastSaved, _overlapSize, _hopSize);
            _dl.FastCopyTo(_dl, _overlapSize, _hopSize);

            _inOffset = _overlapSize;
            _outOffset = 0;
        }

        /// <summary>
        /// Reset filter internals
        /// </summary>
        public void Reset()
        {
            _inOffset = _overlapSize;
            _outOffset = 0;

            _zeroblock.FastCopyTo(_dl, _dl.Length);
            _zeroblock.FastCopyTo(_re, _re.Length);
            _zeroblock.FastCopyTo(_im, _im.Length);
            _zeroblock.FastCopyTo(_filteredRe, _filteredRe.Length);
            _zeroblock.FastCopyTo(_filteredIm, _filteredIm.Length);
            _zeroblock.FastCopyTo(_lastSaved, _lastSaved.Length);
        }

        /// <summary>
        /// Offline processing
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringMethod method = FilteringMethod.Auto)
        {
            return new DiscreteSignal(signal.SamplingRate, signal.Samples.Select(s => Process(s)));
        }
    }
}
