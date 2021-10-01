using System;
using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Operations.Convolution
{
    /// <summary>
    /// Represents fast (FFT) convolver.
    /// </summary>
    public class Convolver
    {
        /// <summary>
        /// FFT size.
        /// </summary>
        private int _fftSize;

        /// <summary>
        /// Internal FFT transformer.
        /// </summary>
        private RealFft _fft;

        // internal reusable buffers

        private float[] _real1;
        private float[] _imag1;
        private float[] _real2;
        private float[] _imag2;

        /// <summary>
        /// Constructs <see cref="Convolver"/>. 
        /// Allocates necessary memory according to <paramref name="fftSize"/>. 
        /// If <paramref name="fftSize"/> is not set then the memory will be allocated 
        /// during the first call of Convolve() method based on input signals.
        /// </summary>
        public Convolver(int fftSize = 0)
        {
            if (fftSize > 0)
            {
                PrepareMemory(fftSize);
            }
        }

        /// <summary>
        /// Prepares all necessary arrays for calculations.
        /// </summary>
        private void PrepareMemory(int fftSize)
        {
            _fftSize = fftSize;
            _fft = new RealFft(_fftSize);

            _real1 = new float[_fftSize];
            _imag1 = new float[_fftSize];
            _real2 = new float[_fftSize];
            _imag2 = new float[_fftSize];
        }

        /// <summary>
        /// Does fast convolution of <paramref name="signal"/> with <paramref name="kernel"/> via FFT. 
        /// Returns signal of length: signal.Length + kernel.Length - 1.
        /// </summary>
        public DiscreteSignal Convolve(DiscreteSignal signal, DiscreteSignal kernel)
        {
            var length = signal.Length + kernel.Length - 1;

            if (_fft is null)
            {
                PrepareMemory(MathUtils.NextPowerOfTwo(length));
            }

            var output = new float[_fftSize];

            Convolve(signal.Samples, kernel.Samples, output);

            return new DiscreteSignal(signal.SamplingRate, output).First(length);
        }

        /// <summary>
        /// Does fast convolution of <paramref name="input"/> with <paramref name="kernel"/> via FFT (maximally in-place). 
        /// The result is stored in <paramref name="output"/> array. 
        /// This version is best suited for block processing when memory needs to be reused. 
        /// Input arrays must have size equal to the size of FFT. 
        /// FFT size MUST be set explicitly and properly in constructor!
        /// </summary>
        public void Convolve(float[] input, float[] kernel, float[] output)
        {
            Array.Clear(_real1, 0, _fftSize);
            Array.Clear(_real2, 0, _fftSize);

            input.FastCopyTo(_real1, input.Length);
            kernel.FastCopyTo(_real2, kernel.Length);

            // 1) do FFT of both signals

            _fft.Direct(_real1, _real1, _imag1);
            _fft.Direct(_real2, _real2, _imag2);

            // 2) do complex multiplication of spectra and normalize

            for (var i = 0; i <= _fftSize / 2; i++)
            {
                var re = _real1[i] * _real2[i] - _imag1[i] * _imag2[i];
                var im = _real1[i] * _imag2[i] + _imag1[i] * _real2[i];
                _real1[i] = re / _fftSize;
                _imag1[i] = im / _fftSize;
            }

            // 3) do inverse FFT of resulting spectrum

            _fft.Inverse(_real1, _imag1, output);
        }

        /// <summary>
        /// Does fast cross-correlation between <paramref name="signal1"/> and <paramref name="signal2"/> via FFT.
        /// </summary>
        public DiscreteSignal CrossCorrelate(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            var reversedKernel = new DiscreteSignal(signal2.SamplingRate, signal2.Samples.Reverse());

            return Convolve(signal1, reversedKernel);
        }

        /// <summary>
        /// Does fast cross-correlation between <paramref name="input1"/> and <paramref name="input2"/> via FFT (maximally in-place). 
        /// The result is stored in <paramref name="output"/> array. 
        /// This version is best suited for block processing when memory needs to be reused. 
        /// Input arrays must have size equal to the size of FFT. 
        /// FFT size MUST be set explicitly and properly in constructor!
        /// </summary>
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
