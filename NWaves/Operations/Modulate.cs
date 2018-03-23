using System;
using System.Linq;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Operations
{
    /// <summary>
    ///  Class providing modulation methods:
    /// 
    ///     - ring
    ///     - amplitude
    ///     - frequency
    ///     - phase
    /// 
    /// </summary>
    public static class Modulate
    {
        /// <summary>
        /// Ring modulation (RM)
        /// </summary>
        /// <param name="carrier">Carrier signal</param>
        /// <param name="modulator">Modulator signal</param>
        /// <returns>RM signal</returns>
        public static DiscreteSignal Ring(DiscreteSignal carrier,
                                          DiscreteSignal modulator)
        {
            if (carrier.SamplingRate != modulator.SamplingRate)
            {
                throw new ArgumentException("Sampling rates must be the same!");
            }

            return new DiscreteSignal(carrier.SamplingRate,
                                      carrier.Samples.Zip(modulator.Samples, (c, m) => c * m));
        }

        /// <summary>
        /// Amplitude modulation (AM)
        /// </summary>
        /// <param name="carrier">Carrier signal</param>
        /// <param name="modulatorFrequency">Modulator frequency</param>
        /// <param name="modulationIndex">Modulation index (depth)</param>
        /// <returns>AM signal</returns>
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
        /// Frequency modulation (FM)
        /// </summary>
        /// <param name="baseband">Baseband signal</param>
        /// <param name="carrierAmplitude">Carrier amplitude</param>
        /// <param name="carrierFrequency">Carrier frequency</param>
        /// <param name="deviation">Frequency deviation</param>
        /// <returns>RM signal</returns>
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
        /// Sinusoidal frequency modulation (FM)
        /// </summary>
        /// <param name="carrierFrequency">Carrier signal frequency</param>
        /// <param name="carrierAmplitude">Carrier signal amplitude</param>
        /// <param name="modulatorFrequency">Modulator frequency</param>
        /// <param name="modulationIndex">Modulation index (depth)</param>
        /// <param name="length">Length of FM signal</param>
        /// <param name="samplingRate">Sampling rate</param>
        /// <returns>Sinusoidal FM signal</returns>
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
        /// Linear frequency modulation (FM)
        /// </summary>
        /// <param name="carrierFrequency">Carrier signal frequency</param>
        /// <param name="carrierAmplitude">Carrier signal amplitude</param>
        /// <param name="modulationIndex">Modulation index (depth)</param>
        /// <param name="length">Length of FM signal</param>
        /// <param name="samplingRate">Sampling rate</param>
        /// <returns>Sinusoidal FM signal</returns>
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
        /// Phase modulation (PM)
        /// </summary>
        /// <param name="baseband">Baseband signal</param>
        /// <param name="carrierAmplitude">Carrier amplitude</param>
        /// <param name="carrierFrequency">Carrier frequency</param>
        /// <param name="deviation">Frequency deviation</param>
        /// <returns>RM signal</returns>
        public static DiscreteSignal Phase(DiscreteSignal baseband,
                                           float carrierAmplitude,
                                           float carrierFrequency,
                                           float deviation = 0.5f)
        {
            var fs = baseband.SamplingRate;
            var ca = carrierAmplitude;          // just short aliases //
            var cf = carrierFrequency;

            var output = Enumerable.Range(0, baseband.Length)
                                   .Select(i => ca * Math.Cos(2 * Math.PI * cf / fs * i + deviation * baseband[i]));

            return new DiscreteSignal(fs, output.ToFloats());
        }
    }
}
