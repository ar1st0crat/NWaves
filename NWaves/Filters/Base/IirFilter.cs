using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Class representing Infinite Impulse Response filters
    /// </summary>
    public class IirFilter : LtiFilter
    {
        /// <summary>
        /// Denominator part coefficients in filter's transfer function 
        /// (recursive part in difference equations)
        /// </summary>
        protected double[] _a;
        public double[] A
        {
            get
            {
                return _a;
            }
            protected set
            {
                _a = value;
                _a32 = _a.ToFloats();
            }
        }

        /// <summary>
        /// Float versions of denominator coefficients for computations by default
        /// </summary>
        protected float[] _a32;

        /// <summary>
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations)
        /// </summary>
        protected double[] _b;
        public double[] B
        {
            get
            {
                return _b;
            }
            protected set
            {
                _b = value;
                _b32 = _b.ToFloats();
            }
        }

        /// <summary>
        /// Float versions of numerator coefficients for computations by default
        /// </summary>
        protected float[] _b32;

        /// <summary>
        /// If _a.Length + _b.Length exceeds this value, 
        /// the filtering code will use a circular buffer.
        /// </summary>
        public const int FilterSizeForOptimizedProcessing = 64;

        /// <summary>
        /// Default length of truncated impulse response
        /// </summary>
        public const int DefaultImpulseResponseLength = 512;

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        protected IirFilter()
        {
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        /// <param name="b">TF numerator coefficients</param>
        /// <param name="a">TF denominator coefficients</param>
        public IirFilter(IEnumerable<double> b, IEnumerable<double> a)
        {
            B = b.ToArray();
            A = a.ToArray();
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
            switch (filteringOptions)
            {
                case FilteringOptions.Auto:
                case FilteringOptions.Custom:
                {
                    return _a.Length + _b.Length <= FilterSizeForOptimizedProcessing ?
                        ApplyFilterDirectly(signal) : 
                        ApplyFilterCircularBuffer(signal);
                }
                case FilteringOptions.OverlapAdd:
                {
                    var fftSize = MathUtils.NextPowerOfTwo(4 * DefaultImpulseResponseLength);
                    var ir = new DiscreteSignal(signal.SamplingRate, ImpulseResponse().ToFloats());
                    return Operation.OverlapAdd(signal, ir, fftSize);
                }
                case FilteringOptions.OverlapSave:
                {
                    var fftSize = MathUtils.NextPowerOfTwo(4 * DefaultImpulseResponseLength);
                    var ir = new DiscreteSignal(signal.SamplingRate, ImpulseResponse().ToFloats());
                    return Operation.OverlapSave(signal, ir, fftSize);
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
            var a = _a32;
            var b = _b32;

            var output = new float[input.Length];

            for (var n = 0; n < input.Length; n++)
            {
                for (var k = 0; k < b.Length; k++)
                {
                    if (n >= k) output[n] += b[k] * input[n - k];
                }
                for (var m = 1; m < a.Length; m++)
                {
                    if (n >= m) output[n] -= a[m] * output[n - m];
                }
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Quite inefficient implementation of filtering in time domain:
        /// use linear buffers for recursive and non-recursive delay lines.
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>        
        public DiscreteSignal ApplyFilterLinearBuffer(DiscreteSignal signal)
        {
            var input = signal.Samples;
            var a = _a32;
            var b = _b32;

            var output = new float[input.Length];

            // buffers for delay lines:
            var wb = new float[b.Length];
            var wa = new float[a.Length];

            for (var i = 0; i < input.Length; i++)
            {
                wb[0] = input[i];

                for (var k = 0; k < b.Length; k++)
                {
                    output[i] += b[k] * wb[k];
                }

                for (var m = 1; m < a.Length; m++)
                {
                    output[i] -= a[m] * wa[m - 1];
                }

                // update delay line

                for (var k = b.Length - 1; k > 0; k--)
                {
                    wb[k] = wb[k - 1];
                }
                for (var m = a.Length - 1; m > 0; m--)
                {
                    wa[m] = wa[m - 1];
                }

                wa[0] = output[i];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
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
            var a = _a32;
            var b = _b32;

            var output = new float[input.Length];

            // buffers for delay lines:
            var wb = new float[b.Length];
            var wa = new float[a.Length];

            var wbpos = wb.Length - 1;
            var wapos = wa.Length - 1;
            
            for (var n = 0; n < input.Length; n++)
            {
                wb[wbpos] = input[n];

                var pos = 0;
                for (var k = wbpos; k < b.Length; k++)
                {
                    output[n] += b[pos++] * wb[k];
                }
                for (var k = 0; k < wbpos; k++)
                {
                    output[n] += b[pos++] * wb[k];
                }

                pos = 1;
                for (var m = wapos + 1; m < a.Length; m++)
                {
                    output[n] -= a[pos++] * wa[m];
                }
                for (var m = 0; m < wapos; m++)
                {
                    output[n] -= a[pos++] * wa[m];
                }

                wa[wapos] = output[n];

                wbpos--;
                if (wbpos < 0) wbpos = wb.Length - 1;

                wapos--;
                if (wapos < 0) wapos = wa.Length - 1;
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Divide all filter coefficients by A[0]
        /// </summary>
        public void Normalize()
        {
            var first = A[0];

            if (Math.Abs(first) < 1e-10)
            {
                throw new ArgumentException("The first A coefficient can not be zero!");
            }

            if (Math.Abs(first - 1.0) < 1e-10)
            {
                return;
            }

            for (var i = 0; i < A.Length; i++)
            {
                A[i] /= first;
                _a32[i] = (float) A[i];
            }

            for (var i = 0; i < B.Length; i++)
            {
                B[i] /= first;
                _b32[i] = (float) B[i];
            }
        }

        /// <summary>
        /// Returns the real-valued impulse response of a filter.
        /// 
        /// Method calculates the Impulse Response of a filter
        /// by feeding the unit impulse into it.
        /// </summary>
        /// <param name="length">
        /// The length of an impulse reponse.
        /// It's the length of truncated infinite impulse reponse.
        /// </param>
        public override double[] ImpulseResponse(int length = 512)
        {
            var impulse = new double[length];
            impulse[0] = 1.0;
            return ApplyTo(impulse);
        }

        /// <summary>
        /// Zeros of the transfer function
        /// </summary>
        public override ComplexDiscreteSignal Zeros
        {
            get { return TransferFunction.TfToZp(B); }
            set { B = TransferFunction.ZpToTf(value); }
        }

        /// <summary>
        /// Poles of the transfer function
        /// </summary>
        public override ComplexDiscreteSignal Poles
        {
            get { return TransferFunction.TfToZp(A); }
            set { A = TransferFunction.ZpToTf(value); }
        }

        /// <summary>
        /// Sequential combination of two IIR filters
        /// </summary>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <returns></returns>
        public static IirFilter operator *(IirFilter filter1, IirFilter filter2)
        {
            var num = Operation.Convolve(filter1.B, filter2.B);
            var den = Operation.Convolve(filter1.A, filter2.A);

            return new IirFilter(num, den);
        }

        /// <summary>
        /// Sequential combination of an IIR and a FIR filters
        /// </summary>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <returns></returns>
        public static IirFilter operator *(IirFilter filter1, FirFilter filter2)
        {
            return filter1 * filter2.AsIir();
        }

        #region double precision

        public double[] ApplyTo(double[] input)
        {
            var output = new double[input.Length];

            for (var n = 0; n < input.Length; n++)
            {
                for (var k = 0; k < _b.Length; k++)
                {
                    if (n >= k) output[n] += _b[k] * input[n - k];
                }
                for (var m = 1; m < _a.Length; m++)
                {
                    if (n >= m) output[n] -= _a[m] * output[n - m];
                }
            }

            return output;
        }

        #endregion
    }
}
