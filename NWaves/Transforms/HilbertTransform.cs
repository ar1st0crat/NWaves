using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Transforms
{
    /// <summary>
    /// Hilbert Transform
    /// </summary>
    public class HilbertTransform
    {
        /// <summary>
        /// Length of Hilbert Transform
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Fft transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length of Hilbert Transform</param>
        public HilbertTransform(int length = 1024)
        {
            Length = length;
            _fft = new Fft(length);
        }

        /// <summary>
        /// Compute complex analytic signal
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <returns>Complex analytic signal</returns>
        public ComplexDiscreteSignal AnalyticSignal(float[] samples)
        {
            var analyticSignal = new ComplexDiscreteSignal(1, samples, allocateNew: true);

            var re = analyticSignal.Real;
            var im = analyticSignal.Imag;

            _fft.Direct(re, im);

            for (var i = 1; i < re.Length / 2; i++)
            {
                re[i] *= 2;
                im[i] *= 2;
            }

            for (var i = re.Length / 2 + 1; i < re.Length; i++)
            {
                re[i] = 0.0f;
                im[i] = 0.0f;
            }

            _fft.Inverse(re, im);

            return analyticSignal;
        }

        /// <summary>
        /// Direct Hilbert Transform
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <returns>Hilbert Transform</returns>
        public DiscreteSignal Direct(DiscreteSignal signal)
        {
            var analyticSignal = AnalyticSignal(signal.Samples).Imag;
            return new DiscreteSignal(signal.SamplingRate, analyticSignal);
        }

        /// <summary>
        /// Direct Hilbert Transform (in-place)
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="output">Hilbert Transform array</param>
        public void Direct(float[] samples, float[] output)
        {
            var analyticSignal = AnalyticSignal(samples).Imag;
            FastCopy.ToExistingArray(analyticSignal, output, analyticSignal.Length);
        }
    }
}
