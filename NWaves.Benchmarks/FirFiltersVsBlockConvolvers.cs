using BenchmarkDotNet.Attributes;
using NWaves.Filters.Base;
using NWaves.Filters.Fda;
using NWaves.Operations.Convolution;
using NWaves.Signals;
using NWaves.Signals.Builders;

namespace NWaves.Benchmarks
{
    [MemoryDiagnoser]
    public class FirFiltersVsBlockConvolvers
    {
        private const int N = 100000;

        private readonly DiscreteSignal _signal;

        private readonly FirFilter _filter21;
        private readonly OlaBlockConvolver _ola21;
        private readonly OlsBlockConvolver _ols21;
        private readonly FirFilter _filter101;
        private readonly OlaBlockConvolver _ola101;
        private readonly OlsBlockConvolver _ols101;
        private readonly FirFilter _filter315;
        private readonly OlaBlockConvolver _ola315;
        private readonly OlsBlockConvolver _ols315;

        public FirFiltersVsBlockConvolvers()
        {
            _signal = new WhiteNoiseBuilder().OfLength(N).Build();

            var kernel21 = DesignFilter.FirWinLp(21, 0.1);

            _filter21 = new FirFilter(kernel21);
            _ola21 = new OlaBlockConvolver(kernel21, 128);
            _ols21 = new OlsBlockConvolver(kernel21, 128);

            var kernel101 = DesignFilter.FirWinLp(101, 0.1);

            _filter101 = new FirFilter(kernel101);
            _filter101.KernelSizeForBlockConvolution = 2048;
            _ola101 = new OlaBlockConvolver(kernel101, 512);
            _ols101 = new OlsBlockConvolver(kernel101, 512);

            var kernel315 = DesignFilter.FirWinLp(315, 0.1);

            _filter315 = new FirFilter(kernel315);
            _filter315.KernelSizeForBlockConvolution = 2048;
            _ola315 = new OlaBlockConvolver(kernel315, 2048);
            _ols315 = new OlsBlockConvolver(kernel315, 2048);
        }

        [Benchmark]
        public void FirFilterKernel21()
        {
            var output = _filter21.ApplyTo(_signal);
        }

        [Benchmark]
        public void OverlapAddKernel21()
        {
            var output = _ola21.ApplyTo(_signal);
        }

        [Benchmark]
        public void OverlapSaveKernel21()
        {
            var output = _ols21.ApplyTo(_signal);
        }

        [Benchmark]
        public void FirFilterKernel101()
        {
            var output = _filter101.ApplyTo(_signal);
        }

        [Benchmark]
        public void OverlapAddKernel101()
        {
            var output = _ola101.ApplyTo(_signal);
        }

        [Benchmark]
        public void OverlapSaveKernel101()
        {
            var output = _ols101.ApplyTo(_signal);
        }

        [Benchmark]
        public void FirFilterKernel315()
        {
            var output = _filter315.ApplyTo(_signal);
        }

        [Benchmark]
        public void OverlapAddKernel315()
        {
            var output = _ola315.ApplyTo(_signal);
        }

        [Benchmark]
        public void OverlapSaveKernel315()
        {
            var output = _ols315.ApplyTo(_signal);
        }
    }
}
