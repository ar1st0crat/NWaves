using System.Collections.Generic;
using System.IO;
using System.Linq;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Class representing Finite Impulse Response filters
    /// </summary>
    public class FirFilter : LtiFilter
    {
        /// <summary>
        /// Filter's kernel.
        /// 
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations)
        /// </summary>
        public float[] Kernel { get; set; }

        /// <summary>
        /// If Kernel.Length exceeds this value, 
        /// the filtering code will always call Overlap-Add routine.
        /// </summary>
        public const int FilterSizeForOptimizedProcessing = 64;

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        protected FirFilter()
        {
        }

        /// <summary>
        /// Constructor accepting the kernel of a filter
        /// </summary>
        /// <param name="kernel"></param>
        public FirFilter(IEnumerable<float> kernel)
        {
            Kernel = kernel.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal, 
                                               FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            if (Kernel.Length >= FilterSizeForOptimizedProcessing && filteringOptions == FilteringOptions.Auto)
            {
                filteringOptions = FilteringOptions.OverlapAdd;
            }

            switch (filteringOptions)
            {
                case FilteringOptions.Custom:
                {
                    return ApplyFilterCircularBuffer(signal);
                }
                case FilteringOptions.OverlapAdd:
                {
                    var fftSize = MathUtils.NextPowerOfTwo(4 * Kernel.Length);
                    return Operation.OverlapAdd(signal, new DiscreteSignal(signal.SamplingRate, Kernel), fftSize);
                }
                case FilteringOptions.OverlapSave:
                {
                    var fftSize = MathUtils.NextPowerOfTwo(4 * Kernel.Length);
                    return Operation.OverlapSave(signal, new DiscreteSignal(signal.SamplingRate, Kernel), fftSize);
                }
                default:
                {
                    return ApplyFilterDirectly(signal);
                }
            }
        }

        /// <summary>
        /// The most straightforward implementation of the difference equation:
        /// code the difference equation as it is
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyFilterDirectly(DiscreteSignal signal)
        {
            var input = signal.Samples;
            var kernel = Kernel;

            var samples = new float[input.Length];

            for (var n = 0; n < input.Length; n++)
            {
                for (var k = 0; k < kernel.Length; k++)
                {
                    if (n >= k) samples[n] += kernel[k] * input[n - k];
                }
            }

            return new DiscreteSignal(signal.SamplingRate, samples);
        }

        /// <summary>
        /// More efficient implementation of filtering in time domain:
        /// use circular buffers for recursive and non-recursive delay lines.
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>        
        public DiscreteSignal ApplyFilterCircularBuffer(DiscreteSignal signal)
        {
            var input = signal.Samples;
            var kernel = Kernel;

            var samples = new float[input.Length];

            // buffer for delay lines:
            var wb = new float[kernel.Length];
            
            var wbpos = wb.Length - 1;
            
            for (var n = 0; n < input.Length; n++)
            {
                wb[wbpos] = input[n];

                var pos = 0;
                for (var k = wbpos; k < kernel.Length; k++)
                {
                    samples[n] += kernel[pos++] * wb[k];
                }
                for (var k = 0; k < wbpos; k++)
                {
                    samples[n] += kernel[pos++] * wb[k];
                }

                wbpos--;
                if (wbpos < 0) wbpos = wb.Length - 1;
            }

            return new DiscreteSignal(signal.SamplingRate, samples);
        }

        /// <summary>
        /// Frequency response of an FIR filter is the FT of its impulse response
        /// </summary>
        public override ComplexDiscreteSignal FrequencyResponse(int length = 512)
        {
            var real = FastCopy.PadZeros(Kernel, length);
            var imag = new float[length];

            var fft = new Fft(length);
            fft.Direct(real, imag);

            return new ComplexDiscreteSignal(1, real.Take(length / 2),
                                                imag.Take(length / 2));
        }

        /// <summary>
        /// Impulse response of an FIR filter is its kernel
        /// </summary>
        public override DiscreteSignal ImpulseResponse(int length = 512)
        {
            return new DiscreteSignal(1, Kernel);
        } 

        /// <summary>
        /// Zeros of the transfer function
        /// </summary>
        public override ComplexDiscreteSignal Zeros
        {
            get { return TfToZp(Kernel); }
            set { Kernel = ZpToTf(value); }
        }

        /// <summary>
        /// Poles of the transfer function (FIR filter does not have poles)
        /// </summary>
        public override ComplexDiscreteSignal Poles
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IirFilter AsIir()
        {
            return new IirFilter(Kernel, new []{ 1.0f });
        }

        /// <summary>
        /// Load kernel from csv file
        /// </summary>
        /// <param name="stream"></param>
        public static FirFilter FromCsv(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                var kernel = content.Split(';').Select(float.Parse).ToArray();
                return new FirFilter(kernel);
            }
        }

        /// <summary>
        /// Serialize kernel to csv file
        /// </summary>
        /// <param name="stream"></param>
        public void Serialize(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                var content = string.Join(";", Kernel.Select(k => k.ToString()));
                writer.WriteLine(content);
            }
        }

        /// <summary>
        /// Sequential combination of two FIR filters
        /// </summary>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <returns></returns>
        public static FirFilter operator *(FirFilter filter1, FirFilter filter2)
        {
            var kernel1 = new DiscreteSignal(1, filter1.Kernel);
            var kernel2 = new DiscreteSignal(1, filter2.Kernel);
            var kernel = Operation.Convolve(kernel1, kernel2);

            return new FirFilter(kernel.Samples);
        }

        /// <summary>
        /// Sequential combination of a FIR and an IIR filters
        /// </summary>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <returns></returns>
        public static IirFilter operator *(FirFilter filter1, IirFilter filter2)
        {
            return filter1.AsIir() * filter2;
        }
    }
}
