# NWaves

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![NuGet](https://img.shields.io/nuget/dt/NWaves.svg?style=flat)

![logo](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/logo_draft.bmp)

NWaves is a .NET library for 1D signal processing focused specifically on audio processing.

## Main features 

Already available:

- [x] major DSP transforms (FFT, DCT, STFT, Hilbert, Hartley, cepstral, Goertzel)
- [x] signal builders (sine/cosine, white/pink/red/Perlin noise, awgn, triangle, sawtooth, square, pulse, ramp, sinc)
- [x] basic LTI digital filters (FIR, IIR, comb, moving average, pre/de-emphasis, DC removal, RASTA)
- [x] BiQuad filters (low-pass, high-pass, band-pass, notch, all-pass, peaking, shelving)
- [x] 1-pole filters (low-pass, high-pass)
- [x] basic operations (convolution, cross-correlation, rectification, amplification)
- [x] block convolution (overlap-add / overlap-save offline and online)
- [x] FIR/IIR filtering (offline and online)
- [x] basic filter design & analysis (group delay, zeros/poles, window-sinc, BP, BR, HP from/to LP, combining filters)
- [x] non-linear filters (median filter, overdrive and distortion effects)
- [x] windowing functions (Hamming, Blackman, Hann, Gaussian, Kaiser, KBD, triangular, Lanczos, flat-top, Bartlett-Hann)
- [x] psychoacoustic filter banks (Mel, Bark, Critical Bands, ERB, octaves) and perceptual weighting (A, B, C)
- [x] customizable feature extraction (time-domain, spectral, MFCC, PNCC/SPNCC, LPC, LPCC, AMS) and CSV serialization
- [x] feature post-processing (mean and variance normalization, adding deltas)
- [x] spectral features (centroid, spread, flatness, entropy, rolloff, contrast, crest, decrease, noiseness, MPEG7)
- [x] harmonic features (harmonic centroid and spread, inharmonicity, tristimulus, odd-to-even ratio)
- [x] perceptual features (loudness, sharpness)
- [x] time-domain characteristics (rms, energy, zero-crossing rate, entropy)
- [x] pitch tracking (autocorrelation, YIN, ZCR + Schmitt trigger, HSS/HPS, cepstrum)
- [x] time scale modification (phase vocoder, PV with identity phase locking, WSOLA, PaulStretch)
- [x] simple resampling, interpolation, decimation
- [x] bandlimited resampling
- [x] noise reduction (spectral subtraction, sciPy-style Wiener filtering)
- [x] envelope following
- [x] sound effects (delay, echo, tremolo, wahwah, phaser, vibrato, flanger, pitch shift, sound morphing, robotize, whisperize)
- [x] adaptive filtering (LMS, NLMS, LMF, SignLMS, RLS)
- [x] simple modulation/demodulation (AM, ring, FM, PM)
- [x] simple audio playback and recording

Planned:

- [ ] sound synthesis (wavetable, ADSR, etc.)
- [ ] more sound effects (reverb, chorus, etc.)
- [ ] more transforms (CQT, DWT, Mellin, Haar, Hadamard)


## Philosophy of NWaves

NWaves was initially intended for research, visualizing and teaching basics of DSP and sound programming. All algorithms are coded in C# as simple as possible and were first designed mostly for offline processing (now some online methods are also available). It doesn't mean, though, that the library could be used only in toy projects; yes, it's not written in C/C++ or Asm, but it's not that *very* slow for many purposes either.


[Read wiki documentation](https://github.com/ar1st0crat/NWaves/wiki)


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

DiscreteSignal left, right;

// load

using (var stream = new FileStream("sample.wav", FileMode.Open))
{
    var waveFile = new WaveFile(stream);
    left = waveFile[Channels.Left];
    right = waveFile[Channels.Right];
}

// save

using (var stream = new FileStream("saved_mono.wav", FileMode.Create))
{
	var waveFile = new WaveFile(left);
	waveFile.SaveTo(stream);
}

using (var stream = new FileStream("saved_stereo.wav", FileMode.Create))
{
    var waveFile = new WaveFile(new [] { left, right });
    waveFile.SaveTo(stream);
}

```


### Transforms:

```C#
// For each transform there's a corresponding transformer object.
// Each transformer object has Direct() and Inverse() methods.

// Complex FFT transformer:

var fft = new Fft(1024);

float[] real = signal.First(1024).Samples;
float[] imag = new float [1024];

// in-place FFT
fft.Direct(real, imag);

// ...do something with real and imaginary parts of the spectrum...

// in-place IFFT
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


// Short-Time Fourier Transform:

var stft = new Stft(1024, 256, WindowTypes.Hamming);
var timefreq = stft.Direct(signal);
var reconstructed = stft.Inverse(timefreq);

var spectrogram = stft.Spectrogram(signal);


// Cepstral transformer:

var ct = new CepstralTransform(24, fftSize: 512);
var cepstrum = ct.Direct(signal);
```


### Operations:

```C#
// convolution

var conv = Operation.Convolve(signal, kernel);
var xcorr = Operation.CrossCorrelate(signal1, signal2);

// block convolution

var filtered = Operation.BlockConvolve(signal, kernel, 4096, FilteringMethod.OverlapAdd);

// resampling

var resampled = Operation.Resample(signal, 22050);
var interpolated = Operation.Interpolate(signal, 3);
var decimated = Operation.Decimate(signal, 2);
var updown = Operation.ResampleUpDown(signal, 3, 2);

// time scale modification

var stretch = Operation.TimeStretch(signal, 0.7, TsmAlgorithm.PhaseVocoderPhaseLocking);
var cool = Operation.TimeStretch(signal, 16, TsmAlgorithm.PaulStretch);

// envelope following

var envelope = Operation.Envelope(signal);

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

var filter = new IirFilter(new [] { 1, 0.5, 0.2 }, new [] { 1, -0.8, 0.3 });

var impulseResponse = filter.ImpulseResponse();
var magnitudeResponse = filter.FrequencyResponse().Magnitude;
var phaseResponse = filter.FrequencyResponse().Phase;

var transferFunction = lowPassFilter.Tf;

var b = transferFunction.Numerator;
var a = transferFunction.Denominator;
var zeros = transferFunction.Zeros;
var poles = transferFunction.Poles;

var gd = transferFunction.GroupDelay();
var pd = transferFunction.PhaseDelay();


// some filter design:

var lpFilter = DesignFilter.FirLp(345, 0.15f);

// HP filter can be obtained from LP with the same cutoff frequency:
var hpFilter = DesignFilter.LpToHp(lpFilter);

// and vice versa:
var lowpass  = DesignFilter.HpToLp(hpFilter);

// design BP filter
var bpFilter = DesignFilter.FirBp(123, 0.05f, 0.15f);

// design BR filter
var brFilter = DesignFilter.FirBr(201, 0.08f, 0.23f, WindowTypes.Kaiser);

var kernel = lowpass.Tf.Numerator;


// sequence of filters:

var cascade = filter * firFilter * notchFilter;
var filtered = cascade.ApplyTo(signal);

// equivalent to:

var filtered = filter.ApplyTo(signal);
filtered = firFilter.ApplyTo(filtered);
filtered = notchFilter.ApplyTo(filtered);


// parallel combination of filters:

var parallel = filter1 + filter2;
filtered = parallel.ApplyTo(signal);


// audio effects:

var pitchShift = new PitchShiftEffect(signal.SamplingRate, 1.2);
var wahwah = new WahwahEffect(signal.SamplingRate,, lfoFrequency: 2/*Hz*/);

var processed = wahwah.ApplyTo(pitchShift.ApplyTo(signal));

```


### Online processing

Online processing is supported by all classes that implement the ```IOnlineFilter``` interface.
Currently, all filters, block convolvers (```OlaBlockConvolver```, ```OlsBlockConvolver```) and audio effects contain the ```Process(sample)``` and ```Process(bufferIn, bufferOut)``` methods responsible for online processing.

Simply prepare necessary buffers or just use them if they come from another part of your system:

```C#

float[] output;

...

void NewChunkAvailable(float[] chunk)
{
    filter.Process(chunk, output);
}

```

Sample after sample:

```C#

var outputSample = filter.Process(sample);

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

![onlinedemo](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/onlinedemo.gif)


### Feature extractors

Highly customizable feature extractors are available for offline and online processing (MFCC family, LPC, pitch and lot of others).

```C#

var sr = signal.SamplingRate;

var lpcExtractor = new LpcExtractor(sr, 16, 0.032/*sec*/, 0.015/*sec*/);
var lpcVectors = lpcExtractor.ComputeFrom(signal);


var mfccExtractor = new MfccExtractor(sr, 13, filterbankSize: 24, preEmphasis: 0.95);
var mfccVectors = mfccExtractor.ParallelComputeFrom(signal);


var tdExtractor = new TimeDomainFeaturesExtractor(sr, "all", frameDuration, hopDuration);
var spectralExtractor = new SpectralFeaturesExtractor(sr, "centroid, flatness, c1+c2+c3", 0.032, 0.015);

var vectors = FeaturePostProcessing.Join(
				tdExtractor.ParallelComputeFrom(signal), 
				spectralExtractor.ParallelComputeFrom(signal));

// each vector will contain 1) all time-domain features (energy, rms, entropy, zcr)
//                          2) specified spectral features


var pnccExtractor = new PnccExtractor(sr, 13);
var pnccVectors = pnccExtractor.ComputeFrom(signal, /*from*/1000, /*to*/10000 /*sample*/);
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

// Many extractors allow setting pre-emphasis coefficient.

// This is equivalent to applying pre-emphasis filter:

var mfccExtractor = new MfccExtractor(sr, 13, filterbankSize: 24);
var pre = new PreEmphasisFilter(0.95);

// option 1:
// ApplyTo() will create new signal (allocate new memory)
var mfccVectors = mfccExtractor.ParallelComputeFrom(pre.ApplyTo(signal));

// option 2:
// process array or DiscreteSignal samples in-place:

for (var i = 0; i < signal.Length; i++)
{
    signal[i] = pre.Process(signal[i]);
}

mfccVectors = mfccExtractor.ParallelComputeFrom(signal);
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

### Demos

![filters](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/Filters.png)

![pitch](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/pitch.png)

![winforms](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/WinForms.png)

![lpc](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/lpc.png)

![mfcc](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/mfcc.png)

![spectral](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/spectral.png)

![effects](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/effects.png)

![adaptive](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/adaptive.png)
