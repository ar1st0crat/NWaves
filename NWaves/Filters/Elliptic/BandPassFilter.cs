using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Elliptic
{
    /// <summary>
    /// Represents bandpass elliptic filter.
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
        /// Gets passband ripple (in dB).
        /// </summary>
        public double RipplePassband { get; private set; }

        /// <summary>
        /// Gets stopband ripple (in dB).
        /// </summary>
        public double RippleStopband { get; private set; }

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
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        public BandPassFilter(double frequencyLow, double frequencyHigh, int order, double ripplePass = 1, double rippleStop = 20)
            : base(MakeTf(frequencyLow, frequencyHigh, order, ripplePass, rippleStop))
        {
            FrequencyLow = frequencyLow;
            FrequencyHigh = frequencyHigh;
            RipplePassband = ripplePass;
            RippleStopband = rippleStop;
        }

        /// <summary>
        /// Generates transfer function.
        /// </summary>
        /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
        /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
        /// <param name="order">Filter order</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        private static TransferFunction MakeTf(double frequencyLow, double frequencyHigh, int order, double ripplePass = 1, double rippleStop = 20)
        {
            return DesignFilter.IirBpTf(frequencyLow,
                                        frequencyHigh,
                                        PrototypeElliptic.Poles(order, ripplePass, rippleStop),
                                        PrototypeElliptic.Zeros(order, ripplePass, rippleStop));
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="frequencyLow">Normalized low cutoff frequency in range [0..0.5]</param>
        /// <param name="frequencyHigh">Normalized high cutoff frequency in range [0..0.5]</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        public void Change(double frequencyLow, double frequencyHigh, double ripplePass = 1, double rippleStop = 20)
        {
            FrequencyLow = frequencyLow;
            FrequencyHigh = frequencyHigh;
            RipplePassband = ripplePass;
            RippleStopband = rippleStop;

            Change(MakeTf(frequencyLow, frequencyHigh, (_a.Length - 1) / 2, ripplePass, rippleStop));
        }
    }
}
