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
        private readonly Fft64 _fft;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length of Hilbert Transform</param>
        public HilbertTransform(int length = 1024)
        {
            Length = length;
            _fft = new Fft64(length);
        }

        /// <summary>
        /// Compute complex analytic signal
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <returns>Complex analytic signal</returns>
        public ComplexDiscreteSignal AnalyticSignal(double[] samples)
        {
            var analyticSignal = new ComplexDiscreteSignal(1, samples);

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
                re[i] = 0.0;
                im[i] = 0.0;
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
            var output = new float[signal.Length];
            Direct(signal.Samples, output);
            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Direct Hilbert Transform (in-place)
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="output">Hilbert Transform array</param>
        public void Direct(double[] samples, double[] output)
        {
            var analyticSignal = AnalyticSignal(samples).Imag;
            analyticSignal.FastCopyTo(output, analyticSignal.Length);
        }

        /// <summary>
        /// Direct Hilbert Transform (in-place)
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="output">Hilbert Transform array</param>
        public void Direct(float[] samples, float[] output)
        {
            var analyticSignal = AnalyticSignal(samples.ToDoubles()).Imag;
            
            for (var i = 0; i < output.Length; i++)
            {
                output[i] = (float)analyticSignal[i];
            }
        }
    }
}
