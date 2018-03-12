using NWaves.Operations;
using NWaves.Signals;

namespace NWaves.Features
{
    /// <summary>
    /// Harmonic features
    /// </summary>
    public static class Harmonic
    {
        /// <summary>
        /// Pitch
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="samplingRate"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static double Pitch(double[] samples, int samplingRate, double low = 80, double high = 400)
        {
            var block = new DiscreteSignal(samplingRate, samples);
            var fftSize = samples.Length;

            var autoCorrelation = Operation.CrossCorrelate(block, block).Last(fftSize);

            var pitch1 = (int)(1.0 * samplingRate / high);    // 2,5 ms = 400Hz
            var pitch2 = (int)(1.0 * samplingRate / low);     // 12,5 ms = 80Hz

            var max = autoCorrelation[pitch1];
            var peakIndex = pitch1;
            for (var k = pitch1 + 1; k <= pitch2; k++)
            {
                if (autoCorrelation[k] > max)
                {
                    max = autoCorrelation[k];
                    peakIndex = k;
                }
            }

            var freqResolution = (double)samplingRate / fftSize;

            return peakIndex * freqResolution;
        }
    }
}
