using BenchmarkDotNet.Attributes;
using NWaves.Signals.Builders;
using NWaves.Transforms;
using NWaves.Utils;
using System;

namespace NWaves.Benchmarks
{
    [MemoryDiagnoser]
    public class FftArrayVsSpan
    {
        private const int FrameSize = 2048;
        private const int HopSize = 100;

        private const int N = 50000;

        private readonly RealFft _fft;
        private readonly RealFft64 _fft64;
        private readonly Fft _complexFft;
        private readonly float[] _samples;
        private readonly double[] _samples64;

        public FftArrayVsSpan()
        {
            _fft = new RealFft(FrameSize);
            _fft64 = new RealFft64(FrameSize);
            _complexFft = new Fft(FrameSize);
            _samples = new WhiteNoiseBuilder().OfLength(N).Build().Samples;
            _samples64 = _samples.ToDoubles();
        }

        [Benchmark]
        public void FftArray()
        {
            var input = new float[FrameSize];

            var re = new float[FrameSize];
            var im = new float[FrameSize];

            for (var i = 0; i < N - FrameSize; i += HopSize)
            {
                _samples.FastCopyTo(input, FrameSize, i);

                _fft.Direct(input, re, im);
            }
        }

        [Benchmark]
        public void FftSpan()
        {
            Span<float> re = stackalloc float[FrameSize];
            Span<float> im = stackalloc float[FrameSize];

            for (var i = 0; i < N - FrameSize; i += HopSize)
            {
                var input = _samples.AsSpan(i, FrameSize);
                // var input = new ReadOnlySpan<float>(_samples, i, FrameSize);

                _fft.Direct(input, re, im);
            }
        }

        [Benchmark]
        public void Fft64Array()
        {
            var input = new double[FrameSize];

            var re = new double[FrameSize];
            var im = new double[FrameSize];

            for (var i = 0; i < N - FrameSize; i += HopSize)
            {
                _samples64.FastCopyTo(input, FrameSize, i);

                _fft64.Direct(input, re, im);
            }
        }

        [Benchmark]
        public void Fft64Span()
        {
            Span<double> re = stackalloc double[FrameSize];
            Span<double> im = stackalloc double[FrameSize];

            for (var i = 0; i < N - FrameSize; i += HopSize)
            {
                var input = _samples64.AsSpan(i, FrameSize);
                // var input = new ReadOnlySpan<float>(_samples, i, FrameSize);

                _fft64.Direct(input, re, im);
            }
        }

        [Benchmark]
        public void ComplexFftArray()
        {
            var reInput = new float[FrameSize];
            var imInput = new float[FrameSize];

            var re = new float[FrameSize];
            var im = new float[FrameSize];

            for (var i = 0; i < N - FrameSize; i += HopSize)
            {
                _samples.FastCopyTo(reInput, FrameSize, i);

                _complexFft.Direct(reInput, imInput, re, im);
            }
        }

        [Benchmark]
        public void ComplexFftSpan()
        {
            Span<float> re = stackalloc float[FrameSize];
            Span<float> im = stackalloc float[FrameSize];
            ReadOnlySpan<float> imInput = stackalloc float[FrameSize];

            for (var i = 0; i < N - FrameSize; i += HopSize)
            {
                var reInput = _samples.AsSpan(i, FrameSize);
                // var input = new ReadOnlySpan<float>(_samples, i, FrameSize);

                _complexFft.Direct(reInput, imInput, re, im);
            }
        }
    }
}
