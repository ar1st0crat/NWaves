using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Filters.Polyphase
{
    /// <summary>
    /// Represents the system of polyphase filters.
    /// </summary>
    public class PolyphaseSystem : IFilter, IOnlineFilter
    {
        /// <summary>
        /// <para>Gets polyphase filters with transfer function E(z^k).</para>
        /// <code>
        /// Example: <br/>
        /// <br/>
        /// h = [1, 2, 3, 4, 3, 2, 1],  k = 3 <br/>
        /// <br/>
        /// e0 = [1, 0, 0, 4, 0, 0, 1] <br/>
        /// e1 = [0, 2, 0, 0, 3, 0, 0] <br/>
        /// e2 = [0, 0, 3, 0, 0, 2, 0] <br/>
        /// </code>
        /// </summary>
        public FirFilter[] Filters { get; private set; }

        /// <summary>
        /// <para>Gets polyphase filters with transfer function E(z) used for multi-rate processing.</para>
        /// <code>
        /// h = [1, 2, 3, 4, 3, 2, 1],  k = 3 <br/>
        /// <br/>
        /// e0 = [1, 4, 1] <br/>
        /// e1 = [2, 3, 0] <br/>
        /// e2 = [3, 2, 0] <br/>
        /// </code>
        /// </summary>
        public FirFilter[] MultirateFilters { get; private set; }

        /// <summary>
        /// Constructs <see cref="PolyphaseSystem"/> with <paramref name="n"/> filters from filter <paramref name="kernel"/>.
        /// </summary>
        /// <param name="kernel">Filter kernel</param>
        /// <param name="n">Number of polyphase filters</param>
        /// <param name="type">Polyphase system type (1 or 2)</param>
        public PolyphaseSystem(double[] kernel, int n, int type = 1)
        {
            Filters = new FirFilter[n];
            MultirateFilters = new FirFilter[n];

            var len = (kernel.Length + 1) / n;

            for (var i = 0; i < Filters.Length; i++)
            {
                var filterKernel = new double[kernel.Length];
                var mrFilterKernel = new double[len];

                for (var j = 0; j < len; j++)
                {
                    var kernelPos = i + n * j;

                    if (kernelPos < kernel.Length)
                    {
                        filterKernel[kernelPos] = kernel[kernelPos];
                        mrFilterKernel[j] = kernel[kernelPos];
                    }
                }

                Filters[i] = new FirFilter(filterKernel);
                MultirateFilters[i] = new FirFilter(mrFilterKernel);
            }

            // type-II -> reverse

            if (type == 2)
            {
                for (var i = 0; i < Filters.Length / 2; i++)
                {
                    var tmp = Filters[i];
                    Filters[i] = Filters[n - 1 - i];
                    Filters[n - 1 - i] = tmp;

                    tmp = MultirateFilters[i];
                    MultirateFilters[i] = MultirateFilters[n - 1 - i];
                    MultirateFilters[n - 1 - i] = tmp;
                }
            }
        }

        /// <summary>
        /// Does polyphase decimation (for type-I systems).
        /// </summary>
        /// <param name="signal">Input signal</param>
        public DiscreteSignal Decimate(DiscreteSignal signal)
        {
            var resampledRate = signal.SamplingRate / MultirateFilters.Length;
            var resampledLength = signal.Length / MultirateFilters.Length;
            var resampled = new DiscreteSignal(resampledRate, resampledLength);

            var acc = 0f;

            // process first K samples separately

            for (var i = MultirateFilters.Length - 1; i >= 1 ; i--)
            {
                acc += MultirateFilters[i].Process(0);
            }
            acc += MultirateFilters[0].Process(signal[0]);
            resampled[0] = acc;

            // rest of the samples are processed very simply by each filter

            var si = 1;
            for (var i = 1; i < resampled.Length; i++)
            {
                acc = 0f;

                for (var j = MultirateFilters.Length - 1; j >= 0; j--)
                {
                    acc += MultirateFilters[j].Process(signal[si++]);
                }

                resampled[i] = acc;
            }

            return resampled;
        }

        /// <summary>
        /// Does polyphase interpolation (for type-II systems).
        /// </summary>
        /// <param name="signal">Input signal</param>
        public DiscreteSignal Interpolate(DiscreteSignal signal)
        {
            var k = MultirateFilters.Length;
            var resampledRate = signal.SamplingRate * k;
            var resampledLength = signal.Length * k;
            var resampled = new DiscreteSignal(resampledRate, resampledLength);

            var ri = 0;
            for (var i = 0; i < signal.Length; i++)
            {
                for (var j = MultirateFilters.Length - 1; j >= 0; j--)
                {
                    resampled[ri++] = k * MultirateFilters[j].Process(signal[i]);
                }
            }

            return resampled;
        }


        #region FIR Filtering (for educational purposes)

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public float Process(float sample)
        {
            // Inefficient, but helps understanding how polyphase filters work

            var output = 0f;

            foreach (var filter in Filters)
            {
                output += filter.Process(sample);
            }

            return output;
        }

        /// <summary>
        /// Resets polyphase filters.
        /// </summary>
        public void Reset()
        {
            foreach (var filter in Filters)
            {
                filter.Reset();
            }

            foreach (var filter in MultirateFilters)
            {
                filter.Reset();
            }
        }

        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);

        #endregion
    }
}
