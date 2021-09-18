using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class representing Fast Hilbert Transform.
    /// </summary>
    public class HilbertTransform
    {
        /// <summary>
        /// Gets size of Hilbert transform.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// FFT transformer, single precision.
        /// </summary>
        private readonly Fft _fft32;

        /// <summary>
        /// FFT transformer, double precision.
        /// </summary>
        private readonly Fft64 _fft64;

        /// <summary>
        /// Construct Hilbert transformer. Transform <paramref name="size"/> must be a power of 2.
        /// </summary>
        /// <param name="size">Size of Hilbert Transform</param>
        /// <param name="doublePrecision">
        /// If true, Hilbert transformer will work only with 64-bit data; 
        /// if false, Hilbert transformer will work only with 32-bit data
        /// </param>
        public HilbertTransform(int size = 1024, bool doublePrecision = true)
        {
            Size = size;

            if (doublePrecision)
            {
                _fft64 = new Fft64(size);
            }
            else
            {
                _fft32 = new Fft(size);
            }
        }

        /// <summary>
        /// Compute complex analytic signal (double precision).
        /// </summary>
        /// <param name="samples">Input array of samples</param>
        /// <param name="norm">Normalize by FFT size</param>
        public ComplexDiscreteSignal AnalyticSignal(double[] samples, bool norm = true)
        {
            var analyticSignal = new ComplexDiscreteSignal(1, samples);

            var re = analyticSignal.Real;
            var im = analyticSignal.Imag;

            _fft64.Direct(re, im);

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

            _fft64.Inverse(re, im);

            if (norm)
            {
                analyticSignal.Attenuate(re.Length);
            }

            return analyticSignal;
        }

        /// <summary>
        /// Compute complex analytic signal (single precision).
        /// </summary>
        /// <param name="samples">Input array of samples</param>
        /// <param name="norm">Normalize by FFT size</param>
        public (float[], float[]) AnalyticSignal(float[] samples, bool norm = true)
        {
            var sre = new DiscreteSignal(1, samples, allocateNew: true);
            var sim = new DiscreteSignal(1, samples.Length);

            var re = sre.Samples;
            var im = sim.Samples;

            _fft32.Direct(re, im);

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

            _fft32.Inverse(re, im);

            if (norm)
            {
                sre.Attenuate(re.Length);
                sim.Attenuate(im.Length);
            }

            return (re, im);
        }

        /// <summary>
        /// Do Hilbert transform of <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Input signal</param>
        public DiscreteSignal Direct(DiscreteSignal signal)
        {
            var output = new float[signal.Length];
            Direct(signal.Samples, output);
            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Do Hilbert Transform of array of <paramref name="samples"/> and store the results in array <paramref name="output"/>.
        /// </summary>
        /// <param name="samples">Input array of samples</param>
        /// <param name="output">Output array with Hilbert transform results</param>
        public void Direct(double[] samples, double[] output)
        {
            var analyticSignal = AnalyticSignal(samples).Imag;
            analyticSignal.FastCopyTo(output, analyticSignal.Length);
        }

        /// <summary>
        /// Do Hilbert Transform of array of <paramref name="samples"/> and store the results in array <paramref name="output"/>.
        /// </summary>
        /// <param name="samples">Input array of samples</param>
        /// <param name="output">Output array with Hilbert transform results</param>
        public void Direct(float[] samples, float[] output)
        {
            var analyticSignal = AnalyticSignal(samples).Item2;
            analyticSignal.FastCopyTo(output, analyticSignal.Length);
        }
    }
}
