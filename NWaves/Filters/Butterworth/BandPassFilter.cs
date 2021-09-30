using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Butterworth
{
    /// <summary>
    /// Represents bandpass Butterworth filter.
    /// </summary>
    public class BandPassFilter : ZiFilter
    {
        /// <summary>
        /// Gets low cutoff frequency.
        /// </summary>
        public double FrequencyLow { get; private set; }

        /// <summary>
        /// Gets high cutoff frequency.
        /// </summary>
        public double FrequencyHigh { get; private set; }

        /// <summary>
        /// Gets filter order.
        /// </summary>
        public int Order => (_a.Length - 1) / 2;

        /// <summary>
        /// Constructs <see cref="BandPassFilter"/> of given <paramref name="order"/> 
        /// with given cutoff frequencies <paramref name="frequencyLow"/> and <paramref name="frequencyHigh"/>.
        /// </summary>
        /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
        /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
        /// <param name="order">Filter order</param>
        public BandPassFilter(double frequencyLow, double frequencyHigh, int order) : base(MakeTf(frequencyLow, frequencyHigh, order))
        {
            FrequencyLow = frequencyLow;
            FrequencyHigh = frequencyHigh;
        }

        /// <summary>
        /// Generates transfer function.
        /// </summary>
        /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
        /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
        /// <param name="order">Filter order</param>
        private static TransferFunction MakeTf(double frequencyLow, double frequencyHigh, int order)
        {
            return DesignFilter.IirBpTf(frequencyLow, frequencyHigh, PrototypeButterworth.Poles(order));
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
        /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
        public void Change(double frequencyLow, double frequencyHigh)
        {
            FrequencyLow = frequencyLow;
            FrequencyHigh = frequencyHigh;

            Change(MakeTf(frequencyLow, frequencyHigh, (_a.Length - 1) / 2));
        }
    }
}
