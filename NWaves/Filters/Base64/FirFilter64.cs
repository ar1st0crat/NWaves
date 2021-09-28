using NWaves.Filters.Base;
using NWaves.Operations.Convolution;
using NWaves.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Filters.Base64
{
    /// <summary>
    /// Represents Finite Impulse Response (FIR) filter.
    /// </summary>
    public class FirFilter64 : LtiFilter64
    {
        /// <summary>
        /// Gets copy of the filter kernel (impulse response).
        /// </summary>
        public double[] Kernel => _b.Take(_kernelSize).ToArray();

        /// <summary>
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations).
        /// </summary>
        protected readonly double[] _b;

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
            get => _tf ?? new TransferFunction(_b.Take(_kernelSize).ToArray());
            protected set => _tf = value;
        }

        /// <summary>
        /// Gets or sets the minimum kernel length for switching to OverlapSave algorithm in auto mode.
        /// </summary>
        public int KernelSizeForBlockConvolution { get; set; } = 64;

        /// <summary>
        /// Internal buffer for delay line.
        /// </summary>
        protected double[] _delayLine;

        /// <summary>
        /// Current offset in delay line.
        /// </summary>
        protected int _delayLineOffset;

        /// <summary>
        /// Constructs <see cref="FirFilter64"/> from <paramref name="kernel"/>.
        /// </summary>
        /// <param name="kernel">FIR filter kernel</param>
        public FirFilter64(IEnumerable<double> kernel)
        {
            _kernelSize = kernel.Count();

            _b = new double[_kernelSize * 2];

            for (var i = 0; i < _kernelSize; i++)
            {
                _b[i] = _b[_kernelSize + i] = kernel.ElementAt(i);
            }

            _delayLine = new double[_kernelSize];
            _delayLineOffset = _kernelSize - 1;
        }

        /// <summary>
        /// <para>Constructs <see cref="FirFilter64"/> from transfer function <paramref name="tf"/>.</para>
        /// <para>
        /// Coefficients (used for filtering) will be cast to floats anyway, 
        /// but filter will store the reference to TransferFunction object for FDA.
        /// </para>
        /// </summary>
        /// <param name="tf">Transfer function</param>
        public FirFilter64(TransferFunction tf) : this(tf.Numerator)
        {
            Tf = tf;
        }

        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public override double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto)
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
                    var blockConvolver = OlaBlockConvolver64.FromFilter(this, fftSize);
                    return blockConvolver.ApplyTo(signal);
                }
                case FilteringMethod.OverlapSave:
                {
                    var fftSize = MathUtils.NextPowerOfTwo(4 * _kernelSize);
                    var blockConvolver = OlsBlockConvolver64.FromFilter(this, fftSize);
                    return blockConvolver.ApplyTo(signal);
                }
                default:
                {
                    return ProcessAllSamples(signal);
                }
            }
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override double Process(double sample)
        {
            _delayLine[_delayLineOffset] = sample;

            var output = 0.0;

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
        public double[] ProcessAllSamples(double[] samples)
        {
            // The Process() code is inlined here in the loop for better performance
            // (especially for smaller kernels).

            var filtered = new double[samples.Length + _kernelSize - 1];

            var k = 0;
            while (k < samples.Length)
            {
                _delayLine[_delayLineOffset] = samples[k];

                var output = 0.0;

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
        /// Changes filter kernel online.
        /// </summary>
        /// <param name="kernel">New kernel</param>
        public void ChangeKernel(double[] kernel)
        {
            if (kernel.Length == _kernelSize)
            {
                for (var i = 0; i < _kernelSize; i++)
                {
                    _b[i] = _b[_kernelSize + i] = kernel[i];
                }
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
    }
}
