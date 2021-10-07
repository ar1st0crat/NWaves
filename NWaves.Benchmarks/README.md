
Benchmark results on a cheaper hardware:

### IIR filters

Different implementations of IIR filters are compared:

- ```IirFilter``` (ver.0.9.2 of NWaves)
- ```IirFilter``` (ver.0.9.4 of NWaves)
- ```IirFilter``` (as of ver.0.9.5 of NWaves)
- ```ZiFilter```

```ZiFilter``` is noticeably faster than ```IirFilter``` in case when the number of zeros is equal to number of poles (e.g. BiQuad and Butterworth filters).

In case of custom IIR filter (3 zeros and 1 pole) ```ZiFilter``` filtering is slower.

``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
Intel Pentium CPU G3460 3.50GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=5.0.100
  [Host]     : .NET 5.0.0 (5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET 5.0.0 (5.0.20.51904), X64 RyuJIT


```
|                       Method |      Mean |    Error |   StdDev |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|----------------------------- |----------:|---------:|---------:|---------:|---------:|---------:|----------:|
|       FilterVersion092BiQuad |  57.20 ms | 0.133 ms | 0.124 ms | 222.2222 | 222.2222 | 222.2222 |     19 MB |
|       FilterVersion094BiQuad |  63.14 ms | 0.147 ms | 0.138 ms | 250.0000 | 250.0000 | 250.0000 |     19 MB |
|       FilterVersion095BiQuad |  55.41 ms | 0.106 ms | 0.094 ms | 200.0000 | 200.0000 | 200.0000 |     19 MB |
|               FilterZiBiQuad |  41.44 ms | 0.130 ms | 0.122 ms | 307.6923 | 307.6923 | 307.6923 |     19 MB |
| FilterVersion092Butterworth6 | 103.57 ms | 0.377 ms | 0.353 ms | 200.0000 | 200.0000 | 200.0000 |     19 MB |
| FilterVersion094Butterworth6 | 108.64 ms | 0.309 ms | 0.274 ms | 200.0000 | 200.0000 | 200.0000 |     19 MB |
| FilterVersion095Butterworth6 |  96.03 ms | 0.186 ms | 0.165 ms | 333.3333 | 333.3333 | 333.3333 |     19 MB |
|         FilterZiButterworth6 |  74.91 ms | 0.216 ms | 0.202 ms | 285.7143 | 285.7143 | 285.7143 |     19 MB |
|       FilterVersion092Custom |  56.77 ms | 0.175 ms | 0.164 ms | 222.2222 | 222.2222 | 222.2222 |     19 MB |
|       FilterVersion094Custom |  62.86 ms | 0.136 ms | 0.121 ms | 250.0000 | 250.0000 | 250.0000 |     19 MB |
|       FilterVersion095Custom |  54.71 ms | 0.101 ms | 0.090 ms | 200.0000 | 200.0000 | 200.0000 |     19 MB |
|               FilterZiCustom |  58.90 ms | 0.146 ms | 0.122 ms | 222.2222 | 222.2222 | 222.2222 |     19 MB |


### FIR Filters

Different implementations of FIR filters are compared:

- ```FirFilter``` (ver.0.9.2 - 0.9.4 of NWaves)
- ```FirFilter``` (as of ver.0.9.5 of NWaves)
- ```ZiFilter```

This benchmark demonstrates that the newer version of ```FirFilter``` is faster. ```ZiFilter``` is expectedly slower than ```FirFilter```.

``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
Intel Pentium CPU G3460 3.50GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=5.0.100
  [Host]     : .NET 5.0.0 (5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET 5.0.0 (5.0.20.51904), X64 RyuJIT


```
|                   Method |      Mean |     Error |    StdDev |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|------------------------- |----------:|----------:|----------:|---------:|---------:|---------:|----------:|
|  FilterVersion092Kernel5 |  9.782 ms | 0.1544 ms | 0.1444 ms | 234.3750 | 234.3750 | 234.3750 |      4 MB |
|  FilterVersion095Kernel5 |  9.697 ms | 0.1397 ms | 0.1307 ms | 187.5000 | 187.5000 | 187.5000 |      4 MB |
|          ZiFilterKernel5 | 11.069 ms | 0.2179 ms | 0.2140 ms | 281.2500 | 281.2500 | 281.2500 |      4 MB |
| FilterVersion092Kernel35 | 51.575 ms | 0.0910 ms | 0.0807 ms | 300.0000 | 300.0000 | 300.0000 |      4 MB |
| FilterVersion095Kernel35 | 36.210 ms | 0.0834 ms | 0.0780 ms | 285.7143 | 285.7143 | 285.7143 |      4 MB |
|         ZiFilterKernel35 | 68.211 ms | 0.0804 ms | 0.0752 ms | 250.0000 | 250.0000 | 250.0000 |      4 MB |


### FFT

Different FFTs are compared:

- RealFft (single precision) : ```float[]``` and ```Span<float>```
- RealFft64 (double precision) : ```double[]``` and ```Span<double>```
- (Complex)Fft (single precision) : ```float[]``` and ```Span<float>```

``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
Intel Pentium CPU G3460 3.50GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=5.0.100
  [Host]     : .NET 5.0.0 (5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET 5.0.0 (5.0.20.51904), X64 RyuJIT


```
|          Method |     Mean |    Error |   StdDev |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |---------:|---------:|---------:|--------:|------:|------:|----------:|
|        FftArray | 14.25 ms | 0.023 ms | 0.021 ms | 15.6250 |     - |     - |  24,648 B |
|         FftSpan | 15.62 ms | 0.024 ms | 0.023 ms |       - |     - |     - |         - |
|      Fft64Array | 14.53 ms | 0.030 ms | 0.028 ms | 15.6250 |     - |     - |  49,224 B |
|       Fft64Span | 15.66 ms | 0.055 ms | 0.051 ms |       - |     - |     - |         - |
| ComplexFftArray | 17.41 ms | 0.081 ms | 0.075 ms |       - |     - |     - |  32,864 B |
|  ComplexFftSpan | 16.67 ms | 0.033 ms | 0.031 ms |       - |     - |     - |         - |


### FIR filters vs. BlockConvolvers

This benchmark demonstrates that OLS and OLA block covolvers are expectedly much faster than FIR filters for longer filter kernels.

``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
Intel Pentium CPU G3460 3.50GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=5.0.100
  [Host]     : .NET 5.0.0 (5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET 5.0.0 (5.0.20.51904), X64 RyuJIT


```
|               Method |      Mean |     Error |    StdDev |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|--------------------- |----------:|----------:|----------:|---------:|---------:|---------:|----------:|
|    FirFilterKernel21 |  2.837 ms | 0.0093 ms | 0.0087 ms | 121.0938 | 121.0938 | 121.0938 |    391 KB |
|   OverlapAddKernel21 |  3.466 ms | 0.0052 ms | 0.0049 ms | 121.0938 | 121.0938 | 121.0938 |    391 KB |
|  OverlapSaveKernel21 |  3.452 ms | 0.0065 ms | 0.0061 ms | 121.0938 | 121.0938 | 121.0938 |    391 KB |
|   FirFilterKernel101 | 10.854 ms | 0.0208 ms | 0.0194 ms | 109.3750 | 109.3750 | 109.3750 |    391 KB |
|  OverlapAddKernel101 |  4.076 ms | 0.0069 ms | 0.0065 ms | 117.1875 | 117.1875 | 117.1875 |    391 KB |
| OverlapSaveKernel101 |  4.035 ms | 0.0087 ms | 0.0081 ms | 117.1875 | 117.1875 | 117.1875 |    391 KB |
|   FirFilterKernel315 | 30.350 ms | 0.0446 ms | 0.0417 ms |  93.7500 |  93.7500 |  93.7500 |    392 KB |
|  OverlapAddKernel315 |  4.374 ms | 0.0082 ms | 0.0076 ms | 117.1875 | 117.1875 | 117.1875 |    392 KB |
| OverlapSaveKernel315 |  4.329 ms | 0.0069 ms | 0.0062 ms | 117.1875 | 117.1875 | 117.1875 |    392 KB |


### Moving average filters

Recursive implementation of moving average filter (```MovingAverageRecursiveFilter```) is expectedly much faster than FIR subclass (```MovingAverageFilter```).


``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
Intel Pentium CPU G3460 3.50GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=5.0.100
  [Host]     : .NET 5.0.0 (5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET 5.0.0 (5.0.20.51904), X64 RyuJIT


```
|                       Method |     Mean |     Error |    StdDev |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|----------------------------- |---------:|----------:|----------:|---------:|---------:|---------:|----------:|
|          MovingAverageFilter | 9.167 ms | 0.1758 ms | 0.1726 ms | 218.7500 | 218.7500 | 218.7500 |      4 MB |
| MovingAverageFilterRecursive | 3.856 ms | 0.0759 ms | 0.0779 ms | 218.7500 | 218.7500 | 218.7500 |      4 MB |


### Median filters

```MedianFilter``` is significantly faster than ```MedianFilter2```.

``` ini

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
Intel Pentium CPU G3460 3.50GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=5.0.100
  [Host]     : .NET 5.0.0 (5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET 5.0.0 (5.0.20.51904), X64 RyuJIT


```
|        Method |      Mean |     Error |    StdDev |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|-------------- |----------:|----------:|----------:|---------:|---------:|---------:|----------:|
|  MedianFilter |  9.327 ms | 0.0201 ms | 0.0188 ms | 109.3750 | 109.3750 | 109.3750 |    391 KB |
| MedianFilter2 | 14.030 ms | 0.0198 ms | 0.0185 ms | 109.3750 | 109.3750 | 109.3750 |    391 KB |
