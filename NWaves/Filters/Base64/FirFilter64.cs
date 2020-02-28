using NWaves.Filters.Base;
using NWaves.Operations.Convolution;
using NWaves.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Filters.Base64
{
    /// <summary>
    /// Class representing Finite Impulse Response filters
    /// </summary>
    public class FirFilter64 : IFilter64, IOnlineFilter64
    {
        /// <summary>
        /// Filter kernel (impulse response)
        /// </summary>
        public double[] Kernel => _b.Take(_kernelSize).ToArray();

        /// <summary>
        /// 
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations).
        /// 
        /// Note.
        /// This array is created from duplicated filter kernel:
        /// 
        ///   kernel                _b
        /// [1 2 3 4 5] -> [1 2 3 4 5 1 2 3 4 5]
        /// 
        /// Such memory layout leads to significant speed-up of online filtering.
        /// 
        /// </summary>
        protected readonly double[] _b;

        /// <summary>
        /// Kernel length
        /// </summary>
        protected int _kernelSize;

        /// <summary>
        /// Transfer function (created lazily or set specifically if needed)
        /// </summary>
        protected TransferFunction _tf;
        public TransferFunction Tf
        {
            get => _tf ?? new TransferFunction(_b.Take(_kernelSize).ToArray());
            protected set => _tf = value;
        }

        /// <summary>
        /// If _kernelSize exceeds this value, 
        /// the filtering code will always call Overlap-Save routine.
        /// </summary>
        public int KernelSizeForBlockConvolution { get; set; } = 64;

        /// <summary>
        /// Internal buffer for delay line
        /// </summary>
        protected double[] _delayLine;

        /// <summary>
        /// Current offset in delay line
        /// </summary>
        protected int _delayLineOffset;

        /// <summary>
        /// Constructor accepting the 64-bit kernel of a filter
        /// </summary>
        /// <param name="kernel"></param>
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
        /// Constructor accepting the transfer function.
        /// 
        /// Coefficients (used for filtering) will be cast to doubles anyway,
        /// but filter will store the reference to TransferFunction object for FDA.
        /// 
        /// </summary>
        /// <param name="kernel"></param>
        public FirFilter64(TransferFunction tf) : this(tf.Numerator)
        {
            Tf = tf;
        }

        /// <summary>
        /// Apply filter to entire signal (offline)
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto)
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
        /// FIR online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public double Process(double sample)
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
        /// Process all signal samples in loop.
        /// The Process() code is inlined in the loop for better performance
        /// (especially for smaller kernels).
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        public double[] ProcessAllSamples(double[] samples)
        {
            var filtered = new double[samples.Length + _kernelSize - 1];

            var k = 0;
            foreach (var sample in samples)
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

                filtered[k++] = output;
            }

            while (k < filtered.Length)
            {
                filtered[k++] = Process(0);
            }

            return filtered;
        }

        /// <summary>
        /// Change filter kernel online
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
        /// Reset filter
        /// </summary>
        public void Reset()
        {
            _delayLineOffset = _kernelSize - 1;
            Array.Clear(_delayLine, 0, _kernelSize);
        }
    }
}
