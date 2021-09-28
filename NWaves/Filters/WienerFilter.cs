using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters
{
    /// <summary>
    /// Represents Wiener filter. 
    /// Implementation is identical to scipy.signal.wiener().
    /// </summary>
    public class WienerFilter : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Size of the Wiener filter.
        /// </summary>
        private readonly int _size;

        /// <summary>
        /// Estimated noise power.
        /// </summary>
        private readonly double _noise;

        /// <summary>
        /// Constructs <see cref="WienerFilter"/> of given <paramref name="size"/>.
        /// </summary>
        /// <param name="size">Size of the Wiener filter</param>
        /// <param name="noise">Estimated noise power</param>
        public WienerFilter(int size = 3, double noise = 0.0)
        {
            Guard.AgainstEvenNumber(size, "The size of the filter");

            _size = size;
            _noise = noise;

            _buf = new float[_size];

            // to mimic scipy.signal.wiener()
            // feed Size / 2 zeros first
            _n = _size / 2;
        }

        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
        {
            var output = new float[signal.Length];

            int i = 0, j = 0;

            for (i = 0; i < _size / 2; i++)    // then feed first samples
            {
                Process(signal[i]);
            }

            for (; j < signal.Length - _size / 2; j++, i++)   // and begin populating output signal
            {
                output[j] = Process(signal[i]);
            }

            for (i = 0; i < _size / 2; i++, j++)     // don't forget last samples
            {
                output[j] = Process(0);
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public float Process(float sample)
        {
            if (_n == _buf.Length)      // some kind of a circular buffer
            {
                _n = 0;
            }

            _buf[_n] = sample;

            var mu = 0.0f;
            for (var i = 0; i < _size; i++)
            {
                mu += _buf[i];
            }
            mu /= _size;

            var sigma = 0.0f;
            for (var i = 0; i < _size; i++)
            {
                sigma += _buf[i] * _buf[i];
            }
            sigma /= _size;
            sigma -= mu * mu;

            var prevSample = _n > 0 ? _buf[_n - 1] : _buf[_size - 1];
            _n++;

            return sigma < _noise ? mu : (float)(mu + (prevSample - mu) * (1 - _noise / sigma));
        }

        /// <summary>
        /// Resets filter.
        /// </summary>
        public void Reset()
        {
            _n = _size / 2;

            for (var i = 0; i < _buf.Length; i++)
            {
                _buf[i] = 0;
            }
        }

        private int _n;

        private float[] _buf;
    }
}
