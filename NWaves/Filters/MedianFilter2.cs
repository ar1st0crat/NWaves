using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters
{
    /// <summary>
    /// Nonlinear median filter
    /// </summary>
    public class MedianFilter2 : IFilter, IOnlineFilter
    {
        /// <summary>
        /// The size of median filter
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size"></param>
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
        /// Method implements median filtering algorithm
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringMethod method = FilteringMethod.Auto)
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
        /// Online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
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
        /// Reset filter
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

        private float[] _buf;
        private float[] _tmp;
    }
}
