using BenchmarkDotNet.Running;
using NWaves.FeatureExtractors;
using NWaves.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;

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

            //var summary = BenchmarkRunner.Run<OfflineFilterForLoopVsLinq>();

            //var summary = BenchmarkRunner.Run<FirFiltersVsBlockConvolvers>();

            //var summary = BenchmarkRunner.Run<MovingAverageFilters>();

            new TestOutputConsistency().Run();


            
            
            
            
            // test Mellin
            // test Hartley
            // test FFT / real FFT

            //var ex = new ChromaExtractor(new FeatureExtractors.Options.ChromaOptions
            //{
            //    SamplingRate = 8000,
            //    FrameSize = 512,
            //    HopSize  = 256
            //});

            //var r = new Random();
            //var data = Enumerable.Range(0, 512).Select(x => (float)r.NextDouble() * 2 - 1).ToArray();

            //var fft = new Fft(512);

            //var inRe = Enumerable.Range(0, 512).Select(x => (float)r.NextDouble() * 2 - 1).ToArray();
            //var inIm = Enumerable.Range(0, 512).Select(x => (float)r.NextDouble() * 2 - 1).ToArray();
            //var outRe = new float[512];
            //var outIm = new float[512];
            //var outRe2 = new float[512];
            //var outIm2 = new float[512];

            //fft.Direct(inRe, inIm, outRe, outIm);
            //fft.InverseNorm(outRe, outIm, outRe2, outIm2);

            //var diffRe = inRe.Zip(outRe2, (v1, v2) => Math.Abs(v1 - v2)).Max();
            //var diffIm = inIm.Zip(outIm2, (v1, v2) => Math.Abs(v1 - v2)).Max();

            //Console.WriteLine($"{diffRe} {diffIm}");
        }
    }
}
