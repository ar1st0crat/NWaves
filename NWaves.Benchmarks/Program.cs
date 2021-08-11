using BenchmarkDotNet.Running;

namespace NWaves.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<FftArrayVsSpan>();

            //var summary = BenchmarkRunner.Run<MedianFilters>();

            //var summary = BenchmarkRunner.Run<FirFiltersVsBlockConvolvers>();
        }
    }
}
