using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Class responsible for OLA/OLS block convolution.
    /// It can be used as a filter (online filter as well).
    /// </summary>
    public class BlockConvolver : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Filter kernel
        /// </summary>
        protected float[] _kernel;

        /// <summary>
        /// FFT size (also the size of one analyzed chunk)
        /// </summary>
        protected int _fftSize;

        /// <summary>
        /// FFT transformer
        /// </summary>
        protected Fft _fft;

        /// <summary>
        /// internal buffers
        /// </summary>
        private float[] _kernelSpectrumRe;
        private float[] _kernelSpectrumIm;
        private float[] _blockRe;
        private float[] _blockIm;
        private float[] _convRe;
        private float[] _convIm;
        private float[] _zeroblock;
        private float[] _lastSaved;

        /// <summary>
        /// Hop size
        /// </summary>
        public int HopSize => _fftSize - _kernel.Length + 1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="fftSize"></param>
        public BlockConvolver(IEnumerable<float> kernel, int fftSize)
        {
            _fftSize = MathUtils.NextPowerOfTwo(fftSize);

            if (kernel.Count() > _fftSize)
            {
                throw new ArgumentException("Kernel length must not exceed the size of FFT!");
            }

            _fft = new Fft(_fftSize);

            _kernel = kernel.ToArray();
            _kernelSpectrumRe = _kernel.PadZeros(_fftSize);
            _kernelSpectrumIm = new float[_fftSize];
            _convRe = new float[_fftSize];
            _convIm = new float[_fftSize];
            _blockRe = new float[_fftSize];
            _blockIm = new float[_fftSize];
            _buffer = new float[_fftSize];
            _zeroblock = new float[_fftSize];

            _fft.Direct(_kernelSpectrumRe, _kernelSpectrumIm);

            Reset();
        }

        /// <summary>
        /// Construct BlockConvolver from a specific FIR filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="fftSize"></param>
        /// <returns></returns>
        public static BlockConvolver FromFilter(FirFilter filter, int fftSize)
        {
            fftSize = MathUtils.NextPowerOfTwo(fftSize);
            return new BlockConvolver(filter.ImpulseResponse().ToFloats(), fftSize);
        }

        /// <summary>
        /// OLA/OLS online filtering
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="count">Ignored</param>
        /// <param name="inputPos"></param>
        /// <param name="outputPos"></param>
        /// <param name="method"></param>
        public void Process(float[] input,
                            float[] output,
                            int count = 0,
                            int inputPos = 0,
                            int outputPos = 0,
                            FilteringMethod method = FilteringMethod.OverlapSave)
        {
            var M = _kernel.Length;

            int n = inputPos, m = outputPos;

            _zeroblock.FastCopyTo(_blockRe, _fftSize);
            _zeroblock.FastCopyTo(_blockIm, _fftSize);

            /**
             *  ===================================== OVERLAP-ADD ========================================
             */ 
            if (method == FilteringMethod.OverlapAdd)
            {
                int k = Math.Min(HopSize, input.Length - n);

                input.FastCopyTo(_blockRe, k, n);

                _fft.Direct(_blockRe, _blockIm);
                for (var j = 0; j < _fftSize; j++)
                {
                    _convRe[j] = (_blockRe[j] * _kernelSpectrumRe[j] - _blockIm[j] * _kernelSpectrumIm[j]) / _fftSize;
                    _convIm[j] = (_blockRe[j] * _kernelSpectrumIm[j] + _blockIm[j] * _kernelSpectrumRe[j]) / _fftSize;
                }
                _fft.Inverse(_convRe, _convIm);

                for (var j = 0; j < M - 1; j++)
                {
                    _convRe[j] += _lastSaved[j];
                }

                _convRe.FastCopyTo(_lastSaved, M - 1, HopSize);
                _convRe.FastCopyTo(output, k, 0, m);
            }
            /**
             *  ===================================== OVERLAP-SAVE ========================================
             */
            else
            {
                int k = Math.Min(HopSize, input.Length - n);
                input.FastCopyTo(_blockRe, k, n, M - 1);

                _lastSaved.FastCopyTo(_blockRe, M - 1);

                _blockRe.FastCopyTo(_lastSaved, M - 1, k);

                _fft.Direct(_blockRe, _blockIm);
                for (var j = 0; j < _fftSize; j++)
                {
                    _convRe[j] = (_blockRe[j] * _kernelSpectrumRe[j] - _blockIm[j] * _kernelSpectrumIm[j]) / _fftSize;
                    _convIm[j] = (_blockRe[j] * _kernelSpectrumIm[j] + _blockIm[j] * _kernelSpectrumRe[j]) / _fftSize;
                }
                _fft.Inverse(_convRe, _convIm);

                _convRe.FastCopyTo(output, k, M - 1, m);
            }
        }

        /// <summary>
        /// Offline OLA/OLS filtering (essential the same as Operation.BlockConvolve() method)
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.OverlapSave)
        {
            if (signal.Length < _fftSize)
            {
                return signal.Copy();
            }

            var filtered = new float[signal.Length + _kernel.Length - 1];

            var i = 0;
            for (; i < signal.Length; i += HopSize)
            {
                Process(signal.Samples, filtered, _fftSize, i, i, method);
            }

            // FOR TESTS; soon will be removed:

            //var r = new Random();
            //var currentBlockSize = r.Next(HopSize / 5, HopSize * 6);

            //var offset = 0;
            //var output = new float[7 * HopSize];       // just some array for outputs

            //var i = 0;
            //while (i + currentBlockSize < signal.Length)
            //{
            //    var input = signal[i, i + currentBlockSize].Samples;       // emulate the coming of a new input block

            //    // ============================= main part here: ====================================================

            //    int readyCount = ProcessChunks(input, output, method);  // process everything that's available

            //    if (readyCount > 0)                                         // if new output is ready
            //    {
            //        output.FastCopyTo(filtered, readyCount, 0, offset);     // do what we need with the output block
            //        offset += readyCount;                                   // track the offset
            //    }

            //    // ===================================================================================================

            //    i += currentBlockSize;
            //    currentBlockSize = r.Next(HopSize / 5, HopSize * 6);
            //}

            //var lastSamples = signal.Last(signal.Length - i).Samples;
            //int lastCount = blockConvolver.ProcessChunks(lastSamples, output, method, last: true);
            //output.FastCopyTo(filtered, lastCount, 0, offset);

            return new DiscreteSignal(signal.SamplingRate, filtered);
        }

        /// <summary>
        /// Reset filter internals
        /// </summary>
        public void Reset()
        {
            _bufferOffset = 0;
            _lastSaved = null;
            _lastSaved = new float[_kernel.Length - 1];
        }


        /// <summary>
        /// Internal buffer for a delay line
        /// </summary>
        private float[] _buffer;

        /// <summary>
        /// Offset in the delay line
        /// </summary>
        private int _bufferOffset;

        /// <summary>
        /// Process new chunks of arbitrary length
        /// </summary>
        /// <param name="input">Input array of samples</param>
        /// <param name="output">Output array of samples</param>
        /// <param name="method">Filtering method</param>
        /// <param name="last">True if chunks are last in a sequence</param>
        /// <returns>Number of already available filtered samples</returns>
        public virtual int ProcessChunks(float[] input,
                                         float[] output,
                                         FilteringMethod method = FilteringMethod.OverlapSave,
                                         bool last = false)
        {
            var length = input.Length;

            // append new data...

            // and if we still don't have _fftSize samples 

            if (_bufferOffset + length < _fftSize)  
            {
                input.FastCopyTo(_buffer, length, 0, _bufferOffset);     // then just add to buffer
                _bufferOffset += length;

                if (last)   // but if it's the last chunk, then process it anyway
                {
                    Process(_buffer, output, method: method);
                    return _bufferOffset - _kernel.Length + 1;
                }

                return 0;
            }

            // if new chunk is completely ready, process it!

            var inputOffset = _fftSize - _bufferOffset;
            var outputOffset = HopSize;

            input.FastCopyTo(_buffer, inputOffset, 0, _bufferOffset);
            Process(_buffer, output, method: method);

            // save last M - 1 samples

            var M = _kernel.Length - 1;

            _bufferOffset = M;
            _buffer.FastCopyTo(_buffer, _bufferOffset, HopSize);

            // process other chunks (if there are any) till the end of input data

            var i = inputOffset;
            for (; i + HopSize <= length; i += HopSize)
            {
                // if we have all previous M-1 samples right in data array, then don't copy anything
                if (i >= M)
                {
                    Process(input, output, _fftSize, i - M, outputOffset, method);
                }
                // otherwise, copy samples, process them and copy last M-1 samples to buffer
                else
                {
                    input.FastCopyTo(_buffer, HopSize, i, _bufferOffset);
                    Process(_buffer, output, _fftSize, 0, outputOffset, method);
                    _buffer.FastCopyTo(_buffer, _bufferOffset, HopSize);
                }
                outputOffset += HopSize;
            }

            var lastPos = i - HopSize;
            if (lastPos >= M)
            {
                input.FastCopyTo(_buffer, _bufferOffset, i - M);
            }

            // last part

            if (i < length)
            {
                input.FastCopyTo(_buffer, length - i, i, _bufferOffset);
                _bufferOffset += length - i;
            }

            // if we specified that this data is the last in a sequence
            // then we don't need to wait until all _fftSize samples are ready
            if (last)
            {
                Process(_buffer, output, length - i, 0, outputOffset, method);
                outputOffset += _bufferOffset - _kernel.Length + 1;
            }

            return outputOffset;
        }
    }
}
