using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Operations.Convolution
{
    /// <summary>
    /// Class responsible for real-valued convolution
    /// </summary>
    public class Convolver
    {
        /// <summary>
        /// FFT size
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// FFT transformer
        /// </summary>
        private readonly Fft _fft;

        // internal reusable buffers

        private float[] _real1;
        private float[] _imag1;
        private float[] _real2;
        private float[] _imag2;
        private float[] _zeroblock;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fftSize">FFT size</param>
        public Convolver(int fftSize = 0)
        {
            if (fftSize <= 0)
            {
                return;
            }

            _fftSize = fftSize;
            _fft = new Fft(_fftSize);

            // prepare blocks in memory:

            _real1 = new float[_fftSize];
            _imag1 = new float[_fftSize];
            _real2 = new float[_fftSize];
            _imag2 = new float[_fftSize];
            _zeroblock = new float[_fftSize];
        }

        /// <summary>
        /// Convolution
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public DiscreteSignal Convolve(DiscreteSignal signal, DiscreteSignal kernel)
        {
            var length = signal.Length + kernel.Length - 1;

            var fft = _fft;
            var fftSize = _fftSize;

            if (_fft == null)
            {
                fftSize = MathUtils.NextPowerOfTwo(length);
                fft = new Fft(fftSize);

                // prepare blocks in memory:

                _real1 = new float[fftSize];
                _imag1 = new float[fftSize];
                _real2 = new float[fftSize];
                _imag2 = new float[fftSize];
                _zeroblock = new float[fftSize];
            }

            _zeroblock.FastCopyTo(_real1, fftSize);
            _zeroblock.FastCopyTo(_real2, fftSize);
            _zeroblock.FastCopyTo(_imag1, fftSize);
            _zeroblock.FastCopyTo(_imag2, fftSize);

            signal.Samples.FastCopyTo(_real1, signal.Length);
            kernel.Samples.FastCopyTo(_real2, kernel.Length);

            // 1) do FFT of both signals

            fft.Direct(_real1, _imag1);
            fft.Direct(_real2, _imag2);

            // 2) do complex multiplication of spectra and normalize

            for (var i = 0; i < fftSize; i++)
            {
                var re = _real1[i] * _real2[i] - _imag1[i] * _imag2[i];
                var im = _real1[i] * _imag2[i] + _imag1[i] * _real2[i];
                _real1[i] = re / fftSize;
                _imag1[i] = im / fftSize;
            }

            // 3) do inverse FFT of resulting spectrum

            fft.Inverse(_real1, _imag1);

            // 4) return resulting meaningful part of the signal (truncate size to N + M - 1)

            return new DiscreteSignal(signal.SamplingRate, _real1).First(length);
        }

        /// <summary>
        /// Fast convolution via FFT for arrays of samples (maximally in-place).
        /// This version is best suited for block processing when memory needs to be reused.
        /// Input arrays must have size equal to the size of FFT.
        /// FFT size MUST be set properly in constructor!
        /// </summary>
        /// <param name="input">Real parts of the 1st signal (zero-padded)</param>
        /// <param name="kernel">Real parts of the 2nd signal (zero-padded)</param>
        /// <param name="output">Real parts of resulting convolution (zero-padded)</param>
        public void Convolve(float[] input, float[] kernel, float[] output)
        {
            _zeroblock.FastCopyTo(_real1, _fftSize);
            _zeroblock.FastCopyTo(_real2, _fftSize);
            _zeroblock.FastCopyTo(_imag1, _fftSize);
            _zeroblock.FastCopyTo(_imag2, _fftSize);

            input.FastCopyTo(_real1, input.Length);
            kernel.FastCopyTo(_real2, kernel.Length);

            // 1) do FFT of both signals

            _fft.Direct(_real1, _imag1);
            _fft.Direct(_real2, _imag2);

            // 2) do complex multiplication of spectra and normalize

            for (var i = 0; i < _fftSize; i++)
            {
                var re = _real1[i] * _real2[i] - _imag1[i] * _imag2[i];
                var im = _real1[i] * _imag2[i] + _imag1[i] * _real2[i];
                output[i] = re / _fftSize;
                _imag1[i] = im / _fftSize;
            }

            // 3) do inverse FFT of resulting spectrum

            _fft.Inverse(output, _imag1);
        }

        /// <summary>
        /// Fast cross-correlation via FFT
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public DiscreteSignal CrossCorrelate(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            var reversedKernel = new DiscreteSignal(signal2.SamplingRate, signal2.Samples.Reverse());

            return Convolve(signal1, reversedKernel);
        }

        /// <summary>
        /// Fast cross-correlation via FFT for arrays of samples (maximally in-place).
        /// This version is best suited for block processing when memory needs to be reused.
        /// Input arrays must have size equal to the size of FFT.
        /// FFT size MUST be set properly in constructor!
        /// </summary>
        /// <param name="input1">Real parts of the 1st signal (zero-padded)</param>
        /// <param name="input2">Real parts of the 2nd signal (zero-padded)</param>
        /// <param name="output">Real parts of resulting cross-correlation (zero-padded if center == 0)</param>
        /// <param name="center">Position of central sample for the case of 2*CENTER-1 cross-correlation 
        /// (if it is set then resulting array has length of CENTER)</param>
        public void CrossCorrelate(float[] input1, float[] input2, float[] output)
        {
            // reverse second signal

            var kernelLength = input2.Length - 1;

            for (var i = 0; i < kernelLength / 2; i++)
            {
                var tmp = input2[i];
                input2[i] = input2[kernelLength - i];
                input2[kernelLength - i] = tmp;
            }

            Convolve(input1, input2, output);
        }
    }
}
