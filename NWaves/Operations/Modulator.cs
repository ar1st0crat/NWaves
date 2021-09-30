using System;
using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Operations
{
    /// <summary>
    /// Provides various modulation methods:
    /// <list type="bullet">
    ///     <item>ring</item>
    ///     <item>amplitude</item>
    ///     <item>frequency</item>
    ///     <item>phase</item>
    /// </list>
    /// </summary>
    public class Modulator
    {
        /// <summary>
        /// Does ring modulation (RM) and returns RM signal.
        /// </summary>
        /// <param name="carrier">Carrier signal</param>
        /// <param name="modulator">Modulator signal</param>
        public static DiscreteSignal Ring(DiscreteSignal carrier, DiscreteSignal modulator)
        {
            if (carrier.SamplingRate != modulator.SamplingRate)
            {
                throw new ArgumentException("Sampling rates must be the same!");
            }

            return new DiscreteSignal(carrier.SamplingRate,
                                      carrier.Samples.Zip(modulator.Samples, (c, m) => c * m));
        }

        /// <summary>
        /// Does amplitude modulation (AM) and returns AM signal.
        /// </summary>
        /// <param name="carrier">Carrier signal</param>
        /// <param name="modulatorFrequency">Modulator frequency</param>
        /// <param name="modulationIndex">Modulation index (depth)</param>
        public static DiscreteSignal Amplitude(DiscreteSignal carrier, 
                                               float modulatorFrequency = 20/*Hz*/,
                                               float modulationIndex = 0.5f)
        {
            var fs = carrier.SamplingRate;
            var mf = modulatorFrequency;          // just short aliases //
            var mi = modulationIndex;

            var output = Enumerable.Range(0, carrier.Length)
                                   .Select(i => carrier[i] * (1 + mi * Math.Cos(2 * Math.PI * mf / fs * i)));

            return new DiscreteSignal(fs, output.ToFloats());
        }

        /// <summary>
        /// Does frequency modulation (FM) and returns FM signal.
        /// </summary>
        /// <param name="baseband">Baseband signal</param>
        /// <param name="carrierAmplitude">Carrier amplitude</param>
        /// <param name="carrierFrequency">Carrier frequency</param>
        /// <param name="deviation">Frequency deviation</param>
        public static DiscreteSignal Frequency(DiscreteSignal baseband,
                                              float carrierAmplitude,
                                              float carrierFrequency,
                                              float deviation = 0.1f/*Hz*/)
        {
            var fs = baseband.SamplingRate;
            var ca = carrierAmplitude;          // just short aliases //
            var cf = carrierFrequency;

            var integral = 0.0;
            var output = Enumerable.Range(0, baseband.Length)
                                   .Select(i => ca * Math.Cos(2 * Math.PI * cf / fs * i +
                                                 2 * Math.PI * deviation * (integral += baseband[i])));

            return new DiscreteSignal(fs, output.ToFloats());
        }

        /// <summary>
        /// Does sinusoidal frequency modulation (FM) and returns sinusoidal FM signal.
        /// </summary>
        /// <param name="carrierFrequency">Carrier signal frequency</param>
        /// <param name="carrierAmplitude">Carrier signal amplitude</param>
        /// <param name="modulatorFrequency">Modulator frequency</param>
        /// <param name="modulationIndex">Modulation index (depth)</param>
        /// <param name="length">Length of FM signal</param>
        /// <param name="samplingRate">Sampling rate</param>
        public static DiscreteSignal FrequencySinusoidal(
                                        float carrierFrequency,
                                        float carrierAmplitude,
                                        float modulatorFrequency,
                                        float modulationIndex,
                                        int length,
                                        int samplingRate = 1)
        {
            var fs = samplingRate;
            var ca = carrierAmplitude;
            var cf = carrierFrequency;          // just short aliases //
            var mf = modulatorFrequency;
            var mi = modulationIndex;

            var output = Enumerable.Range(0, length)
                                   .Select(i => ca * Math.Cos(2 * Math.PI * cf / fs * i + 
                                                mi * Math.Sin(2 * Math.PI * mf / fs * i)));

            return new DiscreteSignal(samplingRate, output.ToFloats());
        }

        /// <summary>
        /// Does linear frequency modulation (FM) and returns FM signal.
        /// </summary>
        /// <param name="carrierFrequency">Carrier signal frequency</param>
        /// <param name="carrierAmplitude">Carrier signal amplitude</param>
        /// <param name="modulationIndex">Modulation index (depth)</param>
        /// <param name="length">Length of FM signal</param>
        /// <param name="samplingRate">Sampling rate</param>
        public static DiscreteSignal FrequencyLinear(
                                        float carrierFrequency,
                                        float carrierAmplitude,
                                        float modulationIndex,
                                        int length,
                                        int samplingRate = 1)
        {
            var fs = samplingRate;
            var ca = carrierAmplitude;          // just short aliases //
            var cf = carrierFrequency;
            var mi = modulationIndex;

            var output = Enumerable.Range(0, length)
                                   .Select(i => ca * Math.Cos(2 * Math.PI * (cf / fs + mi * i) * i / fs));

            return new DiscreteSignal(fs, output.ToFloats());
        }

        /// <summary>
        /// Does phase modulation (PM) and returns PM signal.
        /// </summary>
        /// <param name="baseband">Baseband signal</param>
        /// <param name="carrierAmplitude">Carrier amplitude</param>
        /// <param name="carrierFrequency">Carrier frequency</param>
        /// <param name="deviation">Frequency deviation</param>
        public static DiscreteSignal Phase(DiscreteSignal baseband,
                                           float carrierAmplitude,
                                           float carrierFrequency,
                                           float deviation = 0.8f)
        {
            var fs = baseband.SamplingRate;
            var ca = carrierAmplitude;          // just short aliases //
            var cf = carrierFrequency;

            var output = Enumerable.Range(0, baseband.Length)
                                   .Select(i => ca * Math.Cos(2 * Math.PI * cf / fs * i + deviation * baseband[i]));

            return new DiscreteSignal(fs, output.ToFloats());
        }

        /// <summary>
        /// Does simple amplitude demodulation of <paramref name="signal"/> based on Hilbert transform.
        /// </summary>
        public static DiscreteSignal DemodulateAmplitude(DiscreteSignal signal)
        {
            var ht = new HilbertTransform(signal.Length);
            var mag = ht.AnalyticSignal(signal.Samples).Magnitude;

            return new DiscreteSignal(signal.SamplingRate, mag.ToFloats()) - 1.0f;
        }

        /// <summary>
        /// Does simple frequency demodulation pf <paramref name="signal"/> based on Hilbert transform.
        /// </summary>
        public static DiscreteSignal DemodulateFrequency(DiscreteSignal signal)
        {
            var diff = new float[signal.Length];

            MathUtils.Diff(signal.Samples, diff);

            var ht = new HilbertTransform(signal.Length);
            var mag = ht.AnalyticSignal(diff).Magnitude;

            return new DiscreteSignal(signal.SamplingRate, mag.ToFloats()) - 1.0f;
        }
    }
}
