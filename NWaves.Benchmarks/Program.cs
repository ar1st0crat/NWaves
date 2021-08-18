using BenchmarkDotNet.Running;

namespace NWaves.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<IirFiltersVersion2Vs4Vs5VsZi>();
            //var summary = BenchmarkRunner.Run<FirFiltersVersion2Vs5VsZi>();

            //var summary = BenchmarkRunner.Run<FftArrayVsSpan>();

            //var summary = BenchmarkRunner.Run<MedianFilters>();

            //var summary = BenchmarkRunner.Run<FirFiltersVsBlockConvolvers>();

            new TestOutputConsistency().Run();
        }
    }
}
