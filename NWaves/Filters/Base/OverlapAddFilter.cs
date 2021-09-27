using System;
using System.Linq;
using NWaves.Effects.Base;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Filters.Base
{
    // OverlapAddFilter is the base class for all filters working by the STFT overlap-add scheme.
    //
    // Subclasses must implement ProcessSpectrum() method that corresponds to the second stage.
    // 
    // Also, it inherits from WetDryMixer implementing IMixable interface,
    // since audio effects with wet-dry mixing functionality can be built based on this class.
    //

    /// <summary>
    /// Abstract class for all filters working by the STFT overlap-add scheme:
    /// <list type="number">
    ///     <item>short-time frame analysis</item>
    ///     <item>short-time frame processing</item>
    ///     <item>short-time frame synthesis (overlap-add)</item>
    /// </list>
    /// </summary>
    public abstract class OverlapAddFilter : WetDryMixer, IFilter, IOnlineFilter
    {
        /// <summary>
        /// STFT hop length.
        /// </summary>
        protected readonly int _hopSize;

        /// <summary>
        /// Size of FFT for analysis and synthesis.
        /// </summary>
        protected readonly int _fftSize;

        /// <summary>
        /// ISTFT normalization gain.
        /// </summary>
        protected float _gain;

        /// <summary>
        /// Size of frame overlap.
        /// </summary>
        protected readonly int _overlapSize;

        /// <summary>
        /// Internal FFT transformer.
        /// </summary>
        protected readonly RealFft _fft;

        /// <summary>
        /// Window coefficients.
        /// </summary>
        protected readonly float[] _window;

        /// <summary>
        /// Delay line.
        /// </summary>
        private readonly float[] _dl;

        /// <summary>
        /// Offset in the input delay line.
        /// </summary>
        private int _inOffset;

        /// <summary>
        /// Offset in the output buffer.
        /// </summary>
        private int _outOffset;

        // Internal buffers

        private readonly float[] _re;
        private readonly float[] _im;
        private readonly float[] _filteredRe;
        private readonly float[] _filteredIm;
        private readonly float[] _lastSaved;

        /// <summary>
        /// Construct <see cref="OverlapAddFilter"/>.
        /// </summary>
        /// <param name="hopSize">Hop size (hop length, number of samples)</param>
        /// <param name="fftSize">FFT size</param>
        public OverlapAddFilter(int hopSize, int fftSize = 0)
        {
            _hopSize = hopSize;
            _fftSize = (fftSize > 0) ? fftSize : 8 * hopSize;
            _overlapSize = _fftSize - _hopSize;

            Guard.AgainstInvalidRange(_hopSize, _fftSize, "Hop size", "FFT size");

            _fft = new RealFft(_fftSize);

            _window = Window.OfType(WindowType.Hann, _fftSize);

            _gain = 1 / (_fftSize * _window.Select(w => w * w).Sum() / _hopSize);

            _dl = new float[_fftSize];
            _re = new float[_fftSize];
            _im = new float[_fftSize];
            _filteredRe = new float[_fftSize];
            _filteredIm = new float[_fftSize];
            _lastSaved = new float[_overlapSize];

            _inOffset = _overlapSize;
        }

        /// <summary>
        /// Process one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public virtual float Process(float sample)
        {
            _dl[_inOffset++] = sample;

            if (_inOffset == _fftSize)
            {
                ProcessFrame();
            }

            return _filteredRe[_outOffset++];
        }

        /// <summary>
        /// Process one frame (FFT block).
        /// </summary>
        protected virtual void ProcessFrame()
        {
            // analysis =========================================================

            _dl.FastCopyTo(_re, _fftSize);
            _re.ApplyWindow(_window);
            _fft.Direct(_re, _re, _im);

            // processing =======================================================

            ProcessSpectrum(_re, _im, _filteredRe, _filteredIm);

            // synthesis ========================================================

            _fft.Inverse(_filteredRe, _filteredIm, _filteredRe);
            _filteredRe.ApplyWindow(_window);

            for (var j = 0; j < _overlapSize; j++)
            {
                _filteredRe[j] += _lastSaved[j];
            }

            _filteredRe.FastCopyTo(_lastSaved, _overlapSize, _hopSize);

            for (var i = 0; i < _filteredRe.Length; i++)  // Wet/Dry mix
            {
                _filteredRe[i] *= Wet * _gain;
                _filteredRe[i] += _dl[i] * Dry;
            }

            _dl.FastCopyTo(_dl, _overlapSize, _hopSize);

            _inOffset = _overlapSize;
            _outOffset = 0;
        }

        /// <summary>
        /// Process one spectrum at each Overlap-Add STFT step.
        /// </summary>
        /// <param name="re">Real parts of input spectrum</param>
        /// <param name="im">Imaginary parts of input spectrum</param>
        /// <param name="filteredRe">Real parts of output spectrum</param>
        /// <param name="filteredIm">Imaginary parts of output spectrum</param>
        protected abstract void ProcessSpectrum(float[] re,
                                                float[] im,
                                                float[] filteredRe,
                                                float[] filteredIm);

        /// <summary>
        /// Reset filter internals.
        /// </summary>
        public virtual void Reset()
        {
            _inOffset = _overlapSize;
            _outOffset = 0;

            Array.Clear(_dl, 0, _dl.Length);
            Array.Clear(_re, 0, _re.Length);
            Array.Clear(_im, 0, _im.Length);
            Array.Clear(_filteredRe, 0, _filteredRe.Length);
            Array.Clear(_filteredIm, 0, _filteredIm.Length);
            Array.Clear(_lastSaved, 0, _lastSaved.Length);
        }

        /// <summary>
        /// Process entire signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);
    }
}
