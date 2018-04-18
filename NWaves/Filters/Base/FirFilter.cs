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
        protected double[] _kernel;
        protected double[] Kernel
        {
            get
            {
                return _kernel;
            }
            set
            {
                _kernel = value;
                _kernel32 = _kernel.ToFloats();
                Tf = new TransferFunction(_kernel, new [] { 1.0 });
            }
        }
        
        /// <summary>
        /// Float versions of filter coefficients for computations by default
        /// </summary>
        protected float[] _kernel32;

        /// <summary>
        /// If Kernel.Length exceeds this value, 
        /// the filtering code will always call Overlap-Add routine.
        /// </summary>
        public const int FilterSizeForOptimizedProcessing = 64;

        /// <summary>
        /// Internal buffer for delay line
        /// </summary>
        protected float[] _delayLine;

        /// <summary>
        /// Current offset in delay line
        /// </summary>
        protected int _delayLineOffset;

        /// <summary>
        /// Constructor accepting the kernel of a filter
        /// </summary>
        /// <param name="kernel"></param>
        public FirFilter(IEnumerable<double> kernel)
        {
            Kernel = kernel.ToArray();
            ResetInternals();
        }

        /// <summary>
        /// Apply filter to entire signal
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal, 
                                               FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            if (_kernel.Length >= FilterSizeForOptimizedProcessing && filteringOptions == FilteringOptions.Auto)
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
                case FilteringOptions.OverlapSave:
                {
                    var fftSize = MathUtils.NextPowerOfTwo(4 * Kernel.Length);
                    var ir = new DiscreteSignal(signal.SamplingRate, _kernel32);
                    return filteringOptions == FilteringOptions.OverlapAdd ?
                                Operation.BlockConvolve(signal, ir, fftSize, BlockConvolution.OverlapAdd) :
                                Operation.BlockConvolve(signal, ir, fftSize, BlockConvolution.OverlapSave);
                }
                default:
                {
                    return ApplyFilterDirectly(signal);
                }
            }
        }

        /// <summary>
        /// Online filtering (buffer-by-buffer)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public override float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            if (filteringOptions == FilteringOptions.OverlapAdd ||
                filteringOptions == FilteringOptions.OverlapSave)
            {
                return null;
            }

            var output = new float[input.Length];
            
            for (var n = 0; n < input.Length; n++)
            {
                _delayLine[_delayLineOffset] = input[n];

                var pos = 0;
                for (var k = _delayLineOffset; k < _kernel32.Length; k++)
                {
                    output[n] += _kernel32[pos++] * _delayLine[k];
                }
                for (var k = 0; k < _delayLineOffset; k++)
                {
                    output[n] += _kernel32[pos++] * _delayLine[k];
                }

                if (--_delayLineOffset < 0)
                {
                    _delayLineOffset = _delayLine.Length - 1;
                }
            }

            return output;
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

            var output = new float[input.Length];

            for (var n = 0; n < input.Length; n++)
            {
                for (var k = 0; k < _kernel32.Length; k++)
                {
                    if (n >= k) output[n] += _kernel32[k] * input[n - k];
                }
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Reset internal buffer
        /// </summary>
        private void ResetInternals()
        {
            if (_delayLine == null)
            {
                _delayLine = new float[_kernel32.Length];
            }
            else
            {
                for (var i = 0; i < _delayLine.Length; i++)
                {
                    _delayLine[i] = 0;
                }
            }
            _delayLineOffset = _delayLine.Length - 1;
        }

        /// <summary>
        /// Reset filter
        /// </summary>
        public override void Reset()
        {
            ResetInternals();
        }

        /// <summary>
        /// Frequency response of an FIR filter is the FT of its impulse response
        /// </summary>
        public override ComplexDiscreteSignal FrequencyResponse(int length = 512)
        {
            var real = Kernel.PadZeros(length);
            var imag = new double[length];

            var fft = new Fft64(length);
            fft.Direct(real, imag);

            return new ComplexDiscreteSignal(1, real.Take(length / 2 + 1),
                                                imag.Take(length / 2 + 1));
        }

        /// <summary>
        /// Impulse response of an FIR filter is its kernel
        /// </summary>
        public override double[] ImpulseResponse(int length = 512)
        {
            return Kernel.ToArray();    // copy
        } 

        /// <summary>
        /// Convert to IIR filter
        /// </summary>
        /// <returns></returns>
        public IirFilter AsIir()
        {
            return new IirFilter(Kernel, new []{ 1.0 });
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
                var kernel = content.Split(';').Select(double.Parse);
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
        /// Sequential combination of two FIR filters (also an FIR filter)
        /// </summary>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <returns></returns>
        public static FirFilter operator *(FirFilter filter1, FirFilter filter2)
        {
            var tf = filter1.Tf * filter2.Tf;
            return new FirFilter(tf.Numerator);
        }

        /// <summary>
        /// Sequential combination of an FIR and IIR filter
        /// </summary>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <returns></returns>
        public static IirFilter operator *(FirFilter filter1, IirFilter filter2)
        {
            var tf = filter1.Tf * filter2.Tf;
            return new IirFilter(tf.Numerator, tf.Denominator);
        }

        /// <summary>
        /// Parallel combination of two FIR filters
        /// </summary>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <returns></returns>
        public static FirFilter operator +(FirFilter filter1, FirFilter filter2)
        {
            var tf = filter1.Tf + filter2.Tf;
            return new FirFilter(tf.Numerator);
        }

        /// <summary>
        /// Parallel combination of an FIR and IIR filter
        /// </summary>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <returns></returns>
        public static IirFilter operator +(FirFilter filter1, IirFilter filter2)
        {
            var tf = filter1.Tf + filter2.Tf;
            return new IirFilter(tf.Numerator, tf.Denominator);
        }
    }
}
