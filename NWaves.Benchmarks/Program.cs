using BenchmarkDotNet.Running;

namespace NWaves.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ArrayVsSpan>();

            //var summary = BenchmarkRunner.Run<MedianFilters>();
        }
    }
}
