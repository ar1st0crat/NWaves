using System;

namespace NWaves.Utils
{
    /// <summary>
    /// Fractional delay line
    /// </summary>
    public partial class FractionalDelayLine
    {
        /// <summary>
        /// Interpolation mode
        /// </summary>
        public InterpolationMode InterpolationMode { get; set; }

        /// <summary>
        /// Delay line
        /// </summary>
        private float[] _delayLine;

        /// <summary>
        /// Delay line size
        /// </summary>
        private int _delayLineSize;
        public int Size => _delayLineSize;

        /// <summary>
        /// Current write position
        /// </summary>
        private int _n;

        /// <summary>
        /// Used in InterpolationMode.Thiran
        /// </summary>
        private float _prevInterpolated;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxDelayInSamples">Max delay in samples</param>
        /// <param name="interpolationMode">Interpolation mode</param>
        public FractionalDelayLine(int maxDelayInSamples, InterpolationMode interpolationMode = InterpolationMode.Linear)
        {
            _delayLineSize = Math.Max(4, maxDelayInSamples);
            _delayLine = new float[_delayLineSize];
            _n = 0;

            InterpolationMode = interpolationMode;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="maxDelay">Max delay in seconds</param>
        /// <param name="interpolationMode">Interpolation mode</param>
        public FractionalDelayLine(int samplingRate,
                                   double maxDelay,
                                   InterpolationMode interpolationMode = InterpolationMode.Linear)
            : this((int)(samplingRate * maxDelay) + 1, interpolationMode)
        {
        }

        /// <summary>
        /// Write (put) sample in the delay line
        /// </summary>
        /// <param name="sample"></param>
        public void Write(float sample)
        {
            _delayLine[_n] = sample;

            if (++_n >= _delayLineSize)
            {
                _n = 0;
            }
        }

        /// <summary>
        /// Read (get) sample from the delay line corresponding to given time delay (in seconds)
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public float Read(double delay)
        {
            var precisePosition = (float)(_n - 1 - delay + _delayLineSize) % _delayLineSize;

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
        /// Reset delay line
        /// </summary>
        public void Reset()
        {
            Array.Clear(_delayLine, 0, _delayLineSize);
            _n = 0;
            _prevInterpolated = 0;
        }

        /// <summary>
        /// Resize delay line to ensure new size
        /// </summary>
        public void Ensure(int maxDelayInSamples)
        {
            if (maxDelayInSamples <= _delayLineSize)
            {
                return;
            }

            Array.Resize(ref _delayLine, maxDelayInSamples);

            _delayLineSize = maxDelayInSamples;
        }

        /// <summary>
        /// Resize delay line to ensure new size
        /// </summary>
        public void Ensure(int samplingRate, double maxDelay)
        {
            Ensure((int)(samplingRate * maxDelay) + 1);
        }
    }
}
