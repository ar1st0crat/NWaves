using System;

namespace NWaves.Utils
{
    /// <summary>
    /// Represents fractional delay line.
    /// </summary>
    public partial class FractionalDelayLine
    {
        /// <summary>
        /// Gets or sets interpolation mode.
        /// </summary>
        public InterpolationMode InterpolationMode { get; set; }

        /// <summary>
        /// Gets the size of delay line (number of samples).
        /// </summary>
        public int Size => _delayLineSize;
        private int _delayLineSize;

        /// <summary>
        /// Delay line.
        /// </summary>
        private float[] _delayLine;

        /// <summary>
        /// Current write position.
        /// </summary>
        private int _n;

        /// <summary>
        /// Previously interpolated sample (used with InterpolationMode.Thiran).
        /// </summary>
        private float _prevInterpolated;

        /// <summary>
        /// Constructs <see cref="FractionalDelayLine"/> and reserves given <paramref name="size"/> for its samples.
        /// </summary>
        /// <param name="size">Delay line size (number of samples)</param>
        /// <param name="interpolationMode">Interpolation mode</param>
        public FractionalDelayLine(int size, InterpolationMode interpolationMode = InterpolationMode.Linear)
        {
            _delayLineSize = Math.Max(4, size);
            _delayLine = new float[_delayLineSize];
            _n = 0;

            InterpolationMode = interpolationMode;
        }

        /// <summary>
        /// Constructs <see cref="FractionalDelayLine"/> and reserves the size 
        /// corresponding to <paramref name="maxDelay"/> seconds.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="maxDelay">Max delay (in seconds)</param>
        /// <param name="interpolationMode">Interpolation mode</param>
        public FractionalDelayLine(int samplingRate,
                                   double maxDelay,
                                   InterpolationMode interpolationMode = InterpolationMode.Linear)
            : this((int)(samplingRate * maxDelay) + 1, interpolationMode)
        {
        }

        /// <summary>
        /// Writes (puts) <paramref name="sample"/> to the delay line.
        /// </summary>
        public void Write(float sample)
        {
            _delayLine[_n] = sample;

            if (++_n >= _delayLineSize)
            {
                _n = 0;
            }
        }

        /// <summary>
        /// Reads (gets) sample from the delay line corresponding to given time <paramref name="delay"/> (in seconds).
        /// </summary>
        public float Read(double delay)
        {
            var precisePosition = (float)(_n - delay + _delayLineSize) % _delayLineSize;

            var intPosition = (int)precisePosition;

            var fraction = precisePosition - intPosition;

            switch (InterpolationMode)
            {
                case InterpolationMode.Nearest:
                    {
                        return _delayLine[intPosition % _delayLineSize];
                    }

                case InterpolationMode.Cubic:
                    {
                        var f2 = fraction * fraction;
                        var f3 = f2 * fraction;

                        var sample1 = _delayLine[(intPosition - 1 + _delayLineSize) % _delayLineSize];
                        var sample2 = _delayLine[intPosition];
                        var sample3 = _delayLine[(intPosition + 1) % _delayLineSize];
                        var sample4 = _delayLine[(intPosition + 2) % _delayLineSize];

                        var a0 = -0.5f * sample1 + 1.5f * sample2 - 1.5f * sample3 + 0.5f * sample4;
                        var a1 = sample1 - 2.5f * sample2 + 2.0f * sample3 - 0.5f * sample4;
                        var a2 = -0.5f * sample1 + 0.5f * sample3;
                        var a3 = sample2;

                        return a0 * f3 + a1 * f2 + a2 * fraction + a3;
                    }
                
                case InterpolationMode.Thiran:
                    {
                        var sample1 = _delayLine[intPosition];
                        var sample2 = _delayLine[(intPosition + 1) % _delayLineSize];

                        // from DAFX book:
                        // var alpha = 1 - fraction;

                        // however, according to paper
                        // "A Lossless, Click-free, Pitchbend-able Delay Line Loop Interpolation Scheme"
                        // by Scott A. Van Duyne et al.:

                        if (fraction < 0.618)       // keep fraction in range [0.618, 1.618] (golden ratio)
                        {
                            fraction++;
                        }

                        var alpha = (1 - fraction) / (1 + fraction);

                        var interpolated = sample2 + alpha * (sample1 - _prevInterpolated);

                        _prevInterpolated = interpolated;

                        // the processing scheme above is rather simple,
                        // so there may be audible artifacts in the output signal, anyway

                        return interpolated;
                    }

                case InterpolationMode.Linear:
                default:
                    {
                        var sample1 = _delayLine[intPosition];
                        var sample2 = _delayLine[(intPosition + 1) % _delayLineSize];

                        return sample1 + fraction * (sample2 - sample1);
                    }
            }
        }

        /// <summary>
        /// Resets delay line.
        /// </summary>
        public void Reset()
        {
            Array.Clear(_delayLine, 0, _delayLineSize);
            _n = 0;
            _prevInterpolated = 0;
        }

        /// <summary>
        /// Resizes delay line to ensure new <paramref name="size"/>. 
        /// If <paramref name="size"/> does not exceed current size of the delay line then nothing happens.
        /// </summary>
        public void Ensure(int size)
        {
            if (size <= _delayLineSize)
            {
                return;
            }

            Array.Resize(ref _delayLine, size);

            _delayLineSize = size;
        }

        /// <summary>
        /// Resizes delay line to ensure new size corresponding to <paramref name="maxDelay"/> seconds. 
        /// If the new size does not exceed current size of the delay line then nothing happens.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="maxDelay">Max delay (in seconds)</param>
        public void Ensure(int samplingRate, double maxDelay)
        {
            Ensure((int)(samplingRate * maxDelay) + 1);
        }
    }
}
