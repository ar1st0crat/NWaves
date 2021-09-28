using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters
{
    /// <summary>
    /// Provides alternate implementation of median filter. 
    /// It is slightly faster than <see cref="MedianFilter"/> only for small filter sizes (not exceeding 5, approx.). 
    /// In other cases <see cref="MedianFilter"/> should be preferred.
    /// </summary>
    public class MedianFilter2 : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Gets the size of median filter.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Constructs <see cref="MedianFilter2"/> of given <paramref name="size"/>.
        /// </summary>
        /// <param name="size">Size of the filter</param>
        public MedianFilter2(int size = 9)
        {
            Guard.AgainstEvenNumber(size, "The size of the filter");

            Size = size;

            _buf = new float[Size];
            _tmp = new float[Size];

            // to mimic scipy.signal.medfilt() 
            // feed Size / 2 zeros first
            _n = Size / 2;
        }

        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
        {
            var input = signal.Samples;
            var output = new float[input.Length];

            int i = 0, j = 0;

            for (i = 0; i < Size / 2; i++)    // feed first samples
            {
                Process(input[i]);
            }

            for (; j < input.Length - Size / 2; j++, i++)   // and begin populating output signal
            {
                output[j] = Process(input[i]);
            }

            for (i = 0; i < Size / 2; i++, j++)     // don't forget last samples
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

            _buf[_n++] = sample;

            _buf.FastCopyTo(_tmp, _buf.Length);

            return MathUtils.FindNth(_tmp, Size / 2, 0, Size - 1);
        }

        /// <summary>
        /// Resets filter.
        /// </summary>
        public void Reset()
        {
            _n = Size / 2;

            for (var i = 0; i < _buf.Length; i++)
            {
                _buf[i] = 0;
            }
        }

        private int _n;

        private readonly float[] _buf;
        private readonly float[] _tmp;
    }
}
