using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Operations.Convolution;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Represents Finite Impulse Response (FIR) filter.
    /// </summary>
    public class FirFilter : LtiFilter
    {
        /// <summary>
        /// Gets copy of the filter kernel (impulse response).
        /// </summary>
        public float[] Kernel => _b.Take(_kernelSize).ToArray();

        /// <summary>
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations).
        /// </summary>
        protected readonly float[] _b;

        // 
        // Since the number of coefficients can be really big,
        // we store only float versions and they are used for filtering.
        // 
        // Note.
        // This array is created from duplicated filter kernel:
        // 
        //   kernel                _b
        // [1 2 3 4 5] -> [1 2 3 4 5 1 2 3 4 5]
        // 
        // Such memory layout leads to significant speed-up of online filtering.
        //

        /// <summary>
        /// Kernel length.
        /// </summary>
        protected int _kernelSize;

        /// <summary>
        /// Transfer function.
        /// </summary>
        protected TransferFunction _tf;

        /// <summary>
        /// Gets transfer function.
        /// </summary>
        public override TransferFunction Tf
        {
            // created lazily or set specifically if needed
            get => _tf ?? new TransferFunction(_b.Take(_kernelSize).ToDoubles());
            protected set => _tf = value;
        }

        /// <summary>
        /// Gets or sets the minimum kernel length for switching to OverlapSave algorithm in auto mode.
        /// </summary>
        public int KernelSizeForBlockConvolution { get; set; } = 64;

        /// <summary>
        /// Internal buffer for delay line.
        /// </summary>
        protected float[] _delayLine;

        /// <summary>
        /// Current offset in delay line.
        /// </summary>
        protected int _delayLineOffset;

        /// <summary>
        /// Constructs <see cref="FirFilter"/> from <paramref name="kernel"/>.
        /// </summary>
        /// <param name="kernel">FIR filter kernel</param>
        public FirFilter(IEnumerable<float> kernel)
        {
            _kernelSize = kernel.Count();

            _b = new float[_kernelSize * 2];

            for (var i = 0; i < _kernelSize; i++)
            {
                _b[i] = _b[_kernelSize + i] = kernel.ElementAt(i);
            }

            _delayLine = new float[_kernelSize];
            _delayLineOffset = _kernelSize - 1;
        }

        /// <summary>
        /// <para>Constructs <see cref="FirFilter"/> from 64-bit <paramref name="kernel"/>.</para>
        /// <para>
        /// NOTE. 
        /// It will simply cast values to floats. 
        /// If you need to preserve precision for filter design and analysis, use constructor <see cref="FirFilter(TransferFunction)"/>.
        /// </para>
        /// </summary>
        /// <param name="kernel"></param>
        public FirFilter(IEnumerable<double> kernel) : this(kernel.ToFloats())
        {
        }

        /// <summary>
        /// <para>Constructs <see cref="FirFilter"/> from transfer function <paramref name="tf"/>.</para>
        /// <para>
        /// Coefficients (used for filtering) will be cast to floats anyway, 
        /// but filter will store the reference to TransferFunction object for FDA.
        /// </para>
        /// </summary>
        /// <param name="tf">Transfer function</param>
        public FirFilter(TransferFunction tf) : this(tf.Numerator.ToFloats())
        {
            Tf = tf;
        }

        /// <summary>
        /// Applies filter to entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
        {
            if (_kernelSize >= KernelSizeForBlockConvolution && method == FilteringMethod.Auto)
            {
                method = FilteringMethod.OverlapSave;
            }

            switch (method)
            {
                case FilteringMethod.OverlapAdd:
                {
                    var fftSize = MathUtils.NextPowerOfTwo(4 * _kernelSize);
                    var blockConvolver = OlaBlockConvolver.FromFilter(this, fftSize);
                    return blockConvolver.ApplyTo(signal);
                }
                case FilteringMethod.OverlapSave:
                {
                    var fftSize = MathUtils.NextPowerOfTwo(4 * _kernelSize);
                    var blockConvolver = OlsBlockConvolver.FromFilter(this, fftSize);
                    return blockConvolver.ApplyTo(signal);
                }
                case FilteringMethod.DifferenceEquation:
                {
                    return ApplyFilterDirectly(signal);
                }
                default:
                {
                    return new DiscreteSignal(signal.SamplingRate, ProcessAllSamples(signal.Samples));
                }
            }
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            _delayLine[_delayLineOffset] = sample;

            var output = 0f;

            for (int i = 0, j = _kernelSize - _delayLineOffset; i < _kernelSize; i++, j++)
            {
                output += _delayLine[i] * _b[j];
            }

            if (--_delayLineOffset < 0)
            {
                _delayLineOffset = _kernelSize - 1;
            }

            return output;
        }

        /// <summary>
        /// Processes all <paramref name="samples"/> in loop.
        /// </summary>
        /// <param name="samples">Samples</param>
        public float[] ProcessAllSamples(float[] samples)
        {
            // The Process() code is inlined here in the loop for better performance
            // (especially for smaller kernels).

            var filtered = new float[samples.Length + _kernelSize - 1];

            var k = 0;
            while (k < samples.Length)
            {
                _delayLine[_delayLineOffset] = samples[k];

                var output = 0f;

                for (int i = 0, j = _kernelSize - _delayLineOffset; i < _kernelSize; i++, j++)
                {
                    output += _delayLine[i] * _b[j];
                }

                if (--_delayLineOffset < 0)
                {
                    _delayLineOffset = _kernelSize - 1;
                }

                filtered[k++] = output;
            }

            while (k < filtered.Length)
            {
                filtered[k++] = Process(0);
            }

            return filtered;
        }

        /// <summary>
        /// The most straightforward implementation of the difference equation: 
        /// code the difference equation as it is (it's slower than ProcessAllSamples).
        /// </summary>
        /// <param name="signal">Input signal</param>
        protected DiscreteSignal ApplyFilterDirectly(DiscreteSignal signal)
        {
            var input = signal.Samples;

            var output = new float[input.Length + _kernelSize - 1];

            for (var n = 0; n < output.Length; n++)
            {
                for (var k = 0; k < _kernelSize; k++)
                {
                    if (n >= k && n < input.Length + k)
                    {
                        output[n] += _b[k] * input[n - k];
                    }
                }
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Changes filter kernel online.
        /// </summary>
        /// <param name="kernel">New kernel</param>
        public void ChangeKernel(float[] kernel)
        {
            if (kernel.Length != _kernelSize) return;
            
            for (var i = 0; i < _kernelSize; i++)
            {
                _b[i] = _b[_kernelSize + i] = kernel[i];
            }
        }

        /// <summary>
        /// Resets filter.
        /// </summary>
        public override void Reset()
        {
            _delayLineOffset = _kernelSize - 1;
            Array.Clear(_delayLine, 0, _kernelSize);
        }

        /// <summary>
        /// Creates <see cref="FirFilter"/> from sequential connection of two FIR filters <paramref name="filter1"/> and <paramref name="filter2"/>.
        /// </summary>
        /// <param name="filter1">First FIR filter</param>
        /// <param name="filter2">Second FIR filter</param>
        public static FirFilter operator *(FirFilter filter1, FirFilter filter2)
        {
            var tf = filter1.Tf * filter2.Tf;

            return new FirFilter(tf.Numerator);
        }

        /// <summary>
        /// Creates <see cref="IirFilter"/> from sequential connection of FIR <paramref name="filter1"/> and IIR <paramref name="filter2"/>.
        /// </summary>
        /// <param name="filter1">FIR filter</param>
        /// <param name="filter2">IIR filter</param>
        public static IirFilter operator *(FirFilter filter1, IirFilter filter2)
        {
            var tf = filter1.Tf * filter2.Tf;

            return new IirFilter(tf.Numerator, tf.Denominator);
        }

        /// <summary>
        /// Creates <see cref="FirFilter"/> from parallel connection of two FIR filters <paramref name="filter1"/> and <paramref name="filter2"/>.
        /// </summary>
        /// <param name="filter1">First FIR filter</param>
        /// <param name="filter2">Second FIR filter</param>
        public static FirFilter operator +(FirFilter filter1, FirFilter filter2)
        {
            var tf = filter1.Tf + filter2.Tf;

            return new FirFilter(tf.Numerator);
        }

        /// <summary>
        /// Creates <see cref="IirFilter"/> from parallel connection of FIR <paramref name="filter1"/> and IIR <paramref name="filter2"/>.
        /// </summary>
        /// <param name="filter1">FIR filter</param>
        /// <param name="filter2">IIR filter</param>
        public static IirFilter operator +(FirFilter filter1, IirFilter filter2)
        {
            var tf = filter1.Tf + filter2.Tf;

            return new IirFilter(tf.Numerator, tf.Denominator);
        }
    }
}
