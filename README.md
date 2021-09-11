# NWaves

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Version](https://img.shields.io/nuget/v/NWaves.svg?style=flat)
![NuGet](https://img.shields.io/nuget/dt/NWaves.svg?style=flat)
[![Gitter](https://badges.gitter.im/NWaves/community.svg)](https://gitter.im/NWaves/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

![logo](https://github.com/ar1st0crat/NWaves/blob/master/assets/logo/logo_draft.bmp)

NWaves is a .NET library for 1D signal processing focused on audio processing.

## Releases

NWaves is [available on NuGet](https://www.nuget.org/packages/NWaves/):

```PM> Install-Package NWaves```

[Read wiki documentation](https://github.com/ar1st0crat/NWaves/wiki)

New version **0.9.5** is out! Faster, smarter, more features. [Read about changes here](https://github.com/ar1st0crat/NWaves/wiki/Known-bugs-and-changelog)

[Notes for non-experts in DSP](https://github.com/ar1st0crat/NWaves/wiki/Notes-for-non~experts-in-DSP)

[NWaves for MATLAB/sciPy users](https://github.com/ar1st0crat/NWaves/wiki/NWaves-for-MATLAB-and-sciPy-users)

[Watch survey video](https://www.youtube.com/watch?v=GyRixqQ613A) | [Samples](https://github.com/ar1st0crat/NWaves.Samples)  |  [Benchmarks](https://github.com/ar1st0crat/NWaves/tree/master/NWaves.Benchmarks) | [Playground](https://ar1st0crat.github.io/NWaves.Playground/) ([code](https://github.com/ar1st0crat/NWaves.Playground))


## Main features

- [x] major DSP transforms (FFT, DCT, MDCT, STFT, FWT, Hilbert, Hartley, Mellin, cepstral, Goertzel)
- [x] signal builders (sine, white/pink/red/Perlin noise, awgn, triangle, sawtooth, square, pulse, ramp, ADSR, wavetable)
- [x] basic LTI digital filters (moving average, comb, Savitzky-Golay, pre/de-emphasis, DC removal, RASTA)
- [x] FIR/IIR filtering (offline and online), zero-phase filtering
- [x] BiQuad filters (low-pass, high-pass, band-pass, notch, all-pass, peaking, shelving)
- [x] 1-pole filters (low-pass, high-pass)
- [x] IIR filters (Bessel, Butterworth, Chebyshev I & II, Elliptic, Thiran)
- [x] basic operations (convolution, cross-correlation, rectification, amplification)
- [x] block convolution (overlap-add / overlap-save offline and online)
- [x] basic filter design & analysis (group delay, zeros/poles, BP, BR, HP from/to LP, SOS, combining filters)
- [x] state space representation of LTI filters
- [x] FIR filter design: frequency sampling, window-sinc, equiripple (Remez / Parks-McClellan)
- [x] IIR filter design: IirNotch / IirPeak / IirCombNotch / IirCombPeak
- [x] non-linear filters (median filter, distortion effects, bit crusher)
- [x] windowing functions (Hamming, Blackman, Hann, Gaussian, Kaiser, KBD, triangular, Lanczos, flat-top, Bartlett)
- [x] periodograms (Welch / Lomb-Scargle)
- [x] psychoacoustic filter banks (Mel, Bark, Critical Bands, ERB, octaves) and VTLN warping
- [x] customizable feature extraction (time-domain, spectral, MFCC, PNCC/SPNCC, LPC, LPCC, PLP, AMS)
- [x] preconfigured MFCC extractors: HTK (MFCC-FB24), Slaney (MFCC-FB40)
- [x] LPC conversions: LPC<->cepstrum, LPC<->LSF
- [x] feature post-processing (mean and variance normalization, adding deltas) and CSV serialization
- [x] spectral features (centroid, spread, flatness, entropy, rolloff, contrast, crest, decrease, noiseness, MPEG7)
- [x] harmonic features (harmonic centroid and spread, inharmonicity, tristimulus, odd-to-even ratio)
- [x] time-domain characteristics (rms, energy, zero-crossing rate, entropy)
- [x] pitch tracking (autocorrelation, YIN, ZCR + Schmitt trigger, HSS/HPS, cepstrum)
- [x] chromagram (chroma feature extractor)
- [x] time scale modification (phase vocoder, PV with identity phase locking, WSOLA, PaulStretch)
- [x] simple resampling, interpolation, decimation
- [x] bandlimited resampling
- [x] wavelets: haar, db, symlet, coiflet
- [x] polyphase filters
- [x] noise reduction (spectral subtraction, sciPy-style Wiener filtering)
- [x] sound effects (echo, tremolo, wahwah, phaser, chorus, vibrato, flanger, pitch shift, morphing, robotize, whisperize)
- [x] 3D/Stereo audio (stereo panning, stereo and ping-pong delay, ITD-ILD, binaural panning)
- [x] envelope following
- [x] dynamics processing (limiter / compressor / expander / noise gate)
- [x] harmonic/percussive separation
- [x] Griffin-Lim algorithm
- [x] Karplus-Strong synthesis
- [x] PADSynth synthesis
- [x] adaptive filtering (LMS, NLMS, LMF, SignLMS, RLS)
- [x] simple modulation/demodulation (AM, ring, FM, PM)
- [x] simple audio playback and recording


## Philosophy of NWaves

NWaves was initially intended for research, visualizing and teaching basics of DSP and sound programming. All algorithms are coded in C# as simple as possible and were first designed mostly for offline processing (now many online methods are also available). It doesn't mean, though, that the library could be used only in toy projects; yes, it's not written in C/C++ or Asm, but it's not that *very* slow for many purposes either.


## Quickstart

### Working with 1D signals

```C#
// Create signal from samples repeated 100 times

float[] samples = new [] { 0.5f, 0.2f, -0.3f, 1.2f, 1.6f, -1.8f, 0.3f, -0.2f };

var s = new DiscreteSignal(8000, samples).Repeat(100);

var length = s.Length;
var duration = s.Duration;

var echoSignal = s + s.Delay(50);

var marginSignal = s.First(64).Concatenate(s.Last(64));

var repeatMiddle = s[400, 500].Repeat(10);

var mean = s.Samples.Average();
var sigma = s.Samples.Average(x => (x - mean) * (x - mean));

var normSignal = s - mean;
normSignal.Attenuate(sigma);

```

### Signal builders

```C#

DiscreteSignal sinusoid = 
    new SineBuilder()
        .SetParameter("frequency", 500.0/*Hz*/)
        .SetParameter("phase", Math.PI / 6)
        .OfLength(1000)
        .SampledAt(44100/*Hz*/)
        .Build();

DiscreteSignal noise = 
    new RedNoiseBuilder()
        .SetParameter("min", -2.5)
        .SetParameter("max", 2.5)
        .OfLength(800)
        .SampledAt(44100)
        .DelayedBy(200)
        .Build();

DiscreteSignal noisy = 
    new SineBuilder()
        .SetParameter("min", -10.0)
        .SetParameter("max", 10.0)
        .SetParameter("freq", 1200.0/*Hz*/)
        .OfLength(1000)
        .SampledAt(44100)
        .SuperimposedWith(noise)
        .Build();

```

Signal builders can also act as real-time generators of samples:

```C#

SignalBuilder lfo = 
    new TriangleWaveBuilder()
            .SetParameter("min", 100)
            .SetParameter("max", 1500)
            .SetParameter("frequency", 2.0/*Hz*/)
            .SampledAt(16000/*Hz*/);

//while (...)
{
    var sample = lfo.NextSample();
    //...
}

```


### Signals and wave files:

```C#

WaveFile waveFile;

// load

using (var stream = new FileStream("sample.wav", FileMode.Open))
{
    waveFile = new WaveFile(stream);
}

DiscreteSignal left = waveFile[Channels.Left];
DiscreteSignal right = waveFile[Channels.Right];


// save

var waveFileOut = new WaveFile(left);

using (var stream = new FileStream("saved_mono.wav", FileMode.Create))
{
    waveFileOut.SaveTo(stream);
}

var waveFileStereo = new WaveFile(new [] { left, right });

using (var stream = new FileStream("saved_stereo.wav", FileMode.Create))
{
    waveFileStereo.SaveTo(stream);
}

```


### Transforms

For each transform there's a corresponding transformer object.
Each transformer object has ```Direct()``` and ```Inverse()``` methods.

#### FFT

```C#

// Complex FFT transformer:

var fft = new Fft(1024);

// Real FFT transformer (faster):

var rfft = new RealFft(1024);


float[] real = signal.First(1024).Samples;
float[] imag = new float [1024];

// in-place complex FFT
fft.Direct(real, imag);

// ...do something with real and imaginary parts of the spectrum...

// in-place complex IFFT
fft.Inverse(real, imag);

// post-processed FFT:

var magnitudeSpectrum = 
    fft.MagnitudeSpectrum(signal[1000, 2024]);

var powerSpectrum = 
    fft.PowerSpectrum(signal.First(1024), normalize: false);

var logPowerSpectrum = 
    fft.PowerSpectrum(signal.Last(1024))
       .Samples
       .Select(s => Scale.ToDecibel(s))
       .ToArray();


// real FFT transforms real-valued signal to complex-valued spectrum:

rfft.Direct(real, real, imag);   // real -> (real, imag)
rfft.Inverse(real, imag, real);  // (real, imag) -> real

var magnitudeSpectrum = 
    rfft.MagnitudeSpectrum(signal[1000, 2024]);

var powerSpectrum = 
    rfft.PowerSpectrum(signal.First(1024), normalize: false);

// ...

```

Lot of methods in NWaves have overloaded versions with output buffers as parameters. So reuse memory whenever possible:

```C#

float[] spectrum = new float[1024];

for (var i = start; i < end; i += step)
{
    rfft.MagnitudeSpectrum(signal[i, i + 1024], spectrum);
    // ...
    // do something with spectrum
}

```


#### STFT

```C#

// Short-Time Fourier Transform:

var stft = new Stft(1024, 256, WindowTypes.Hamming);
var timefreq = stft.Direct(signal);
var reconstructed = stft.Inverse(timefreq);

var spectrogram = stft.Spectrogram(signal);

```

#### Cepstral transform

```C#

// Cepstral transformer:

var ct = new CepstralTransform(24, fftSize: 512);

// complex cepstrum
var cepstrum = ct.Direct(signal);
// or
ct.Direct(input, output);

// real cepstrum
ct.RealCepstrum(input, output);

```

#### Wavelets

```C#

var fwt = new Fwt(192, new Wavelet("db5"));

// or
//var fwt = new Fwt(192, new Wavelet(WaveletFamily.Daubechies, 5));

var output = new float[192];
var reconstructed = new float[192];

fwt.Direct(input, output);
fwt.Inverse(output, reconstructed);

```


### Operations:

```C#
// convolution

var conv = Operation.Convolve(signal, kernel);
var xcorr = Operation.CrossCorrelate(signal1, signal2);

// block convolution

var filtered = Operation.BlockConvolve(signal, kernel, 4096, FilteringMethod.OverlapAdd);

// periodogram evaluation

var periodogram = Operation.Welch(signal, 2048, 1024);
var pgram = Operation.LombScargle(x, y, freqs);

// resampling

var resampled = Operation.Resample(signal, 22050);
var interpolated = Operation.Interpolate(signal, 3);
var decimated = Operation.Decimate(signal, 2);
var updown = Operation.ResampleUpDown(signal, 3, 2);

// time scale modification

var stretch = Operation.TimeStretch(signal, 0.7, TsmAlgorithm.Wsola);
var cool = Operation.TimeStretch(signal, 16, TsmAlgorithm.PaulStretch);

// envelope following

var envelope = Operation.Envelope(signal);

// peak / rms normalization

var peakNorm = Operation.NormalizePeak(signal, -3/*dB*/);
var rmsNorm = Operation.NormalizeRms(signal, -3/*dB*/);
var rmsChanged = Operation.ChangeRms(signal, -6/*dB*/);

// rectification

var halfRect = Operation.HalfRectify(signal);
var fullRect = Operation.FullRectify(signal);

// spectral subtraction

var clean = Operation.SpectralSubtract(signal, noise);
```


### Filters and effects:

```C#

var maFilter = new MovingAverageFilter(7);
var smoothedSignal = maFilter.ApplyTo(signal);

var frequency = 800.0/*Hz*/;
var notchFilter = new BiQuad.NotchFilter(frequency / signal.SamplingRate);
var notchedSignal = notchFilter.ApplyTo(signal);


// filter analysis:

var transferFunction = new TransferFunction(new [] { 1, 0.5, 0.2 }, new [] { 1, -0.8, 0.3 });

var filter = new IirFilter(transferFunction);

// we can also write this:

// var filter = new IirFilter(new [] { 1, 0.5, 0.2 }, new [] { 1, -0.8, 0.3 });
// var transferFunction = filter.Tf;
// ...

// if we simply want to apply filter and don't care much about FDA precision:
// read more in tutorial


var impulseResponse = transferFunction.ImpulseResponse();
var magnitudeResponse = transferFunction.FrequencyResponse().Magnitude;
var phaseResponse = transferFunction.FrequencyResponse().Phase;

var b = transferFunction.Numerator;
var a = transferFunction.Denominator;
var zeros = transferFunction.Zeros;
var poles = transferFunction.Poles;

var gd = transferFunction.GroupDelay();
var pd = transferFunction.PhaseDelay();


// some examples of FIR filter design:

var kernel = DesignFilter.FirWinLp(345, 0.15);
var lpFilter = new FirFilter(kernel);

// HP filter can be obtained from LP with the same cutoff frequency:
var hpFilter = DesignFilter.FirLpToHp(lpFilter);

// design BP filter
var bpFilter = DesignFilter.FirWinBp(123, 0.05, 0.15);

// design equiripple HP filter
var bpFilter = DesignFilter.FirEquirippleHp(123, 0.34, 0.355, 0.05, 0.95);


// sequence of filters:

var cascade = filter * firFilter * notchFilter;
var filtered = cascade.ApplyTo(signal);

// filtering is conceptually equivalent to:

var filtered = filter.ApplyTo(signal);
filtered = firFilter.ApplyTo(filtered);
filtered = notchFilter.ApplyTo(filtered);

// same but with double precision:
var cascadeTf = filter.Tf * firFilter.Tf * notchFilter.Tf;
var cascadeFilter = new IirFilter(cascadeTf);
var filtered = cascadeFilter.ApplyTo(signal);

// parallel combination of filters:

var parallel = filter1 + filter2;
filtered = parallel.ApplyTo(signal);

// same but with double precision:
var parallelTf = filter1.Tf + filter2.Tf;
var parallelFilter = new IirFilter(parallelTf);
var filtered = parallelFilter.ApplyTo(signal);

// audio effects:

var flanger = new FlangerEffect(signal.SamplingRate);
var wahwah = new WahwahEffect(signal.SamplingRate, lfoFrequency: 2/*Hz*/);

var processed = wahwah.ApplyTo(flanger.ApplyTo(signal));
// this will create intermediate copy of the signal


// FilterChain is memory-efficient:

var filters = new FilterChain();
filters.Add(flanger);
filters.Add(wahwah);

processed = filters.ApplyTo(signal);


// Second-Order Sections:

var tf = new Butterworth.BandPassFilter(0.1, 0.16, 7).Tf;

// get array of SOS from TF:
TransferFunction[] sos = DesignFilter.TfToSos(tf);

var sosFilter = new FilterChain(sos);

var y = sosFilter.ApplyTo(x);

// or process samples online:
//    ... outSample = sosFilter.Process(sample);

```


### Online processing

Online processing is supported by all classes that implement the ```IOnlineFilter``` interface.
Currently, all LTI filters, ```FilterChain``` class, block convolvers (```OlaBlockConvolver```, ```OlsBlockConvolver```) and audio effects contain the ```Process(sample)``` and ```Process(bufferIn, bufferOut)``` methods responsible for online processing.

Simply process data sample after sample:

```C#

var outputSample = filter.Process(sample);

```

Or prepare necessary buffers (or just use them if they come from another part of your system):

```C#

float[] output;

...

void NewChunkAvailable(float[] chunk)
{
    filter.Process(chunk, output);
}


// if input chunk shouldn't necessarily be preserved, it can be overwritten:

void NewChunkAvailable(float[] chunk)
{
    filter.Process(chunk, chunk);
}

```


Block convolvers:

```C#

// Overlap-Add / Overlap-Save

FirFilter filter = new FirFilter(kernel);

var blockConvolver = OlaBlockConvolver.FromFilter(filter, 4096);

// processing loop:
// while new input sample is available
{
    var outputSample = blockConvolver.Process(sample);
}

// or:
// while new input buffer is available
{
    blockConvolver.Process(input, output);
}

```

See also OnlineDemoForm code.

![onlinedemo](https://github.com/ar1st0crat/NWaves/blob/master/assets/screenshots/onlinedemo.gif)


### Feature extractors

Highly customizable feature extractors are available for offline and online processing (MFCC family, LPC, pitch and lot of others).

```C#

var mfccOptions = new MfccOptions
{
    SamplingRate = signal.SamplingRate,
    FeatureCount = 13,
    FrameDuration = 0.032/*sec*/,
    HopDuration = 0.015/*sec*/,
    FilterBankSize = 26,
    PreEmphasis = 0.97,
    //...unspecified parameters will have default values 
};

var mfccExtractor = new MfccExtractor(mfccOptions);
var mfccVectors = mfccExtractor.ComputeFrom(signal);


// serialize current config to JSON file:

using (var config = new FileStream("file.json", FileMode.Create))
{
    config.SaveOptions(mfccOptions);
}


var lpcOptions = new LpcOptions
{
    SamplingRate = signal.SamplingRate,
    LpcOrder = 15
};

var lpcExtractor = new LpcExtractor(lpcOptions);
var lpcVectors = lpcExtractor.ParallelComputeFrom(signal);



var opts = new MultiFeatureOptions
{
    SamplingRate = signal.SamplingRate,
    FeatureList = "centroid, flatness, c1+c2+c3"
};

var spectralExtractor = new SpectralFeaturesExtractor(opts);

opts.FeatureList = "all";
var tdExtractor = new TimeDomainFeaturesExtractor(opts);

var vectors = FeaturePostProcessing.Join(
                  tdExtractor.ParallelComputeFrom(signal), 
                  spectralExtractor.ParallelComputeFrom(signal));

// each vector will contain 1) all time-domain features (energy, rms, entropy, zcr)
//                          2) specified spectral features


// open config from JSON file:

PnccOptions options;
using (var config = new FileStream("file.json", FileMode.Open))
{
    options = config.LoadOptions<PnccOptions>();
}

var pnccExtractor = new PnccExtractor(pnccOptions);
var pnccVectors = pnccExtractor.ComputeFrom(signal, /*from*/1000, /*to*/60000 /*sample*/);
FeaturePostProcessing.NormalizeMean(pnccVectors);


// serialization

using (var csvFile = new FileStream("mfccs.csv", FileMode.Create))
{
    var serializer = new CsvFeatureSerializer(mfccVectors);
    await serializer.SerializeAsync(csvFile);
}

```

Pre-processing

```C#

// There are 3 options to perform pre-emphasis filtering:

// 1) Set pre-emphasis coefficient in constructor of a feature extractor
// 2) Apply filter before processing and process filtered signal
// 3) Filter signal in-place and process it

// (...read more in docs...)

// option 1:

var opts = new MfccOptions
{
    SamplingRate = signal.SamplingRate,
    FeatureCount = 13,
    PreEmphasis = 0.95
};
var mfccExtractor = new MfccExtractor(opts);
var mfccVectors = mfccExtractor.ComputeFrom(signal);

// option 2:
// ApplyTo() will create new signal (allocate new memory)

opts.PreEmphasis = 0;
mfccExtractor = new MfccExtractor(opts);
var pre = new PreEmphasisFilter(0.95);
var filtered = pre.ApplyTo(signal);
mfccVectors = mfccExtractor.ComputeFrom(filtered);

// option 3:
// process array or DiscreteSignal samples in-place:

for (var i = 0; i < signal.Length; i++)
{
    signal[i] = pre.Process(signal[i]);
}
// or simply:
// pre.Process(signal.Samples, signal.Samples);

mfccVectors = mfccExtractor.ComputeFrom(signal);

```

### Playing and recording

```MciAudioPlayer``` and ```MciAudioRecorder``` work only at Windows-side, since they use winmm.dll and MCI commands.

```C#

IAudioPlayer player = new MciAudioPlayer();

// play entire file
await player.PlayAsync("temp.wav");

// play file from 16000th sample to 32000th sample
await player.PlayAsync("temp.wav", 16000, 32000);


// ...in some event handler
player.Pause();

// ...in some event handler
player.Resume();

// ...in some event handler
player.Stop();


// recording

IAudioRecorder recorder = new MciAudioRecorder();

// ...in some event handler
recorder.StartRecording(16000);

// ...in some event handler
recorder.StopRecording("temp.wav");

```

### Samples

![filters](https://github.com/ar1st0crat/NWaves/blob/master/assets/screenshots/Filters.png)

![pitch](https://github.com/ar1st0crat/NWaves/blob/master/assets/screenshots/pitch.png)

![lpc](https://github.com/ar1st0crat/NWaves/blob/master/assets/screenshots/lpc.png)

![mfcc](https://github.com/ar1st0crat/NWaves/blob/master/assets/screenshots/mfcc.png)

![spectral](https://github.com/ar1st0crat/NWaves/blob/master/assets/screenshots/spectral.png)

![effects](https://github.com/ar1st0crat/NWaves/blob/master/assets/screenshots/effects.png)

![wavelets](https://github.com/ar1st0crat/NWaves/blob/master/assets/screenshots/wavelets.png)

![adaptive](https://github.com/ar1st0crat/NWaves/blob/master/assets/screenshots/adaptive.png)
