using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;
using System;
using System.Linq;

namespace NWaves.Operations.Tsm
{
    /// <summary>
    /// Phase vocoder with identity phase locking [Puckette].
    /// </summary>
    public class PhaseLockingVocoder : IFilter
    {
        /// <summary>
        /// Hop size at analysis stage (STFT decomposition)
        /// </summary>
        private readonly int _hopAnalysis;

        /// <summary>
        /// Hop size at synthesis stage (STFT merging)
        /// </summary>
        private readonly int _hopSynthesis;

        /// <summary>
        /// Size of FFT for analysis and synthesis
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Should phase vocoder use phase locking algorithm [Puckette]
        /// </summary>
        private readonly bool _phaseLocking;

        /// <summary>
        /// Stretch ratio
        /// </summary>
        private readonly double _stretch;

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
        /// Linearly spaced frequencies
        /// </summary>
        private readonly double[] _omega;

        /// <summary>
        /// Array of phases computed at previous step
        /// </summary>
        private double[] _prevPhase;

        /// <summary>
        /// Array of new synthesized phases
        /// </summary>
        private double[] _phaseTotal;

        /// <summary>
        /// Array of spectrum magnitudes (at current step)
        /// </summary>
        private double[] _mag;

        /// <summary>
        /// Array of spectrum phases (at current step)
        /// </summary>
        private double[] _phase;

        /// <summary>
        /// Array of phase deltas
        /// </summary>
        private double[] _delta;

        /// <summary>
        /// Array of peak positions (indices)
        /// </summary>
        private int[] _peaks;

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
        /// <param name="stretch"></param>
        /// <param name="hopAnalysis"></param>
        /// <param name="fftSize"></param>
        /// <param name="phaseLocking"></param>
        public PhaseLockingVocoder(double stretch, int hopAnalysis, int fftSize = 0, bool phaseLocking = true)
        {
            _stretch = stretch;
            _hopAnalysis = hopAnalysis;
            _hopSynthesis = (int)(hopAnalysis * stretch);
            _fftSize = (fftSize > 0) ? fftSize : 8 * Math.Max(_hopAnalysis, _hopSynthesis);
            _phaseLocking = phaseLocking;

            _fft = new Fft(_fftSize);
            _window = Window.OfType(WindowTypes.Hann, _fftSize);

            _gain = 2 / (_fftSize * _window.Select(w => w * w).Sum() / _hopSynthesis);

            _omega = Enumerable.Range(0, _fftSize / 2 + 1)
                               .Select(f => 2 * Math.PI * f / _fftSize)
                               .ToArray();

            _mag = new double[_fftSize / 2 + 1];
            _phase = new double[_fftSize / 2 + 1];

            _prevPhase = new double[_fftSize / 2 + 1];
            _phaseTotal = new double[_fftSize / 2 + 1];

            _delta = new double[_fftSize / 2 + 1];
            _peaks = new int[_fftSize / 4];

            _re = new float[_fftSize];
            _im = new float[_fftSize];
            _zeroblock = new float[_fftSize];
        }

        /// <summary>
        /// Phase locking procedure
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringMethod method = FilteringMethod.Auto)
        {
            var input = signal.Samples;
            var output = new float[(int)(input.Length * _stretch) + _fftSize];

            var peakCount = 0;

            var posSynthesis = 0;
            for (var posAnalysis = 0; posAnalysis + _fftSize < input.Length; posAnalysis += _hopAnalysis)
            {
                input.FastCopyTo(_re, _fftSize, posAnalysis);
                _zeroblock.FastCopyTo(_im, _fftSize);

                _re.ApplyWindow(_window);

                _fft.Direct(_re, _im);

                for (var j = 0; j < _mag.Length; j++)
                {
                    _mag[j] = Math.Sqrt(_re[j] * _re[j] + _im[j] * _im[j]);
                    _phase[j] = Math.Atan2(_im[j], _re[j]);
                }

                // spectral peaks in magnitude spectrum

                peakCount = 0;

                for (var j = 2; j < _mag.Length - 3; j++)
                {
                    if (_mag[j] <= _mag[j - 1] || _mag[j] <= _mag[j - 2] ||
                        _mag[j] <= _mag[j + 1] || _mag[j] <= _mag[j + 2])
                    {
                        continue;   // if not a peak
                    }

                    _peaks[peakCount++] = j;
                }

                _peaks[peakCount++] = _mag.Length - 1;

                // assign phases at peaks to all neighboring frequency bins

                var leftPos = 0;

                for (var j = 0; j < peakCount - 1; j++)
                {
                    var peakPos = _peaks[j];
                    var peakPhase = _phase[peakPos];

                    _delta[peakPos] = peakPhase - _prevPhase[peakPos];

                    var deltaUnwrapped = _delta[peakPos] - _hopAnalysis * _omega[peakPos];
                    var deltaWrapped = MathUtils.Mod(deltaUnwrapped + Math.PI, 2 * Math.PI) - Math.PI;

                    var freq = _omega[peakPos] + deltaWrapped / _hopAnalysis;

                    _phaseTotal[peakPos] = _phaseTotal[peakPos] + _hopSynthesis * freq;

                    var rightPos = (_peaks[j] + _peaks[j + 1]) / 2;

                    for (var k = leftPos; k < rightPos; k++)
                    {
                        _phaseTotal[k] = _phaseTotal[peakPos] + _phase[k] - _phase[peakPos];

                        _prevPhase[k] = _phase[k];

                        _re[k] = (float)(_mag[k] * Math.Cos(_phaseTotal[k]));
                        _im[k] = (float)(_mag[k] * Math.Sin(_phaseTotal[k]));
                    }

                    leftPos = rightPos;
                }

                for (var j = _fftSize / 2 + 1; j < _fftSize; j++)
                {
                    _re[j] = _im[j] = 0.0f;
                }

                _fft.Inverse(_re, _im);

                for (var j = 0; j < _re.Length; j++)
                {
                    output[posSynthesis + j] += _re[j] * _window[j];
                }

                for (var j = 0; j < _hopSynthesis; j++)
                {
                    output[posSynthesis + j] *= _gain;
                }

                posSynthesis += _hopSynthesis;
            }

            for (; posSynthesis < output.Length; posSynthesis++)
            {
                output[posSynthesis] *= _gain;
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        public void Reset()
        {
            for (var i = 0; i < _phaseTotal.Length; i++)
            {
                _phaseTotal[i] = 0;
                _prevPhase[i] = 0;
            }
        }
    }
}
