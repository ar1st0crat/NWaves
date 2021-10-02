using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Filters
{
    /// <summary>
    /// Represents median filter.
    /// </summary>
    public class MedianFilter : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Gets the size of median filter.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Delay line (circular buffer).
        /// </summary>
        private readonly float[] _delayLine;

        /// <summary>
        /// Buffer filled with sorted values from delay line.
        /// </summary>
        private readonly List<float> _sortedSamples;

        /// <summary>
        /// Index of the current sample.
        /// </summary>
        private int _n;

        /// <summary>
        /// Constructs <see cref="MedianFilter"/> of given <paramref name="size"/>.
        /// </summary>
        /// <param name="size">Size of the filter</param>
        public MedianFilter(int size = 9)
        {
            Guard.AgainstEvenNumber(size, "The size of the filter");

            Size = size;

            _sortedSamples = Enumerable.Repeat(0f, Size).ToList();
            _delayLine = new float[Size];
        }

        /// <summary>
        /// Applies filter to entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
        {
            var input = signal.Samples;
            var output = new float[input.Length];

            int i = 0, j = 0;

            // to mimic scipy.signal.medfilt() feed Size/2 zeros first

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
            if (_n == Size)
            {
                _n = 0;
            }

            var sampleToRemove = _delayLine[_n];
            _delayLine[_n++] = sample;
                       
            var removeIndex = _sortedSamples.BinarySearch(sampleToRemove);
            _sortedSamples.RemoveAt(removeIndex);

            // insertion like in insertion sort

            int i = _sortedSamples.Count - 1; 

            while (i >= 0 && sample < _sortedSamples[i])
            {
                i--;
            }

            _sortedSamples.Insert(i + 1, sample);

            return _sortedSamples[Size / 2];
        }

        /// <summary>
        /// Resets filter.
        /// </summary>
        public void Reset()
        {
            _n = 0;

            for (var i = 0; i < _sortedSamples.Count; _sortedSamples[i++] = 0) ;

            Array.Clear(_delayLine, 0, Size);
        }
    }
}
