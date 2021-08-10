using BenchmarkDotNet.Attributes;
using NWaves.Signals.Builders;
using NWaves.Transforms;
using NWaves.Utils;
using System;

namespace NWaves.Benchmarks
{
    [MemoryDiagnoser]
    public class ArrayVsSpan
    {
        private const int FrameSize = 2048;
        private const int HopSize = 100;

        private const int N = 50000;

        private readonly RealFft _fft;
        private readonly float[] _samples;
        
        public ArrayVsSpan()
        {
            _fft = new RealFft(FrameSize);
            _samples = new WhiteNoiseBuilder().OfLength(N).Build().Samples;
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
    }
}
