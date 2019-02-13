# NWaves

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

![logo](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/logo_draft.bmp)

NWaves is a .NET library for 1d signal processing focused specifically on audio processing.

## Main features 

Already available:

- [x] major DSP transforms (FFT, DCT, STFT, Hilbert, cepstral, Goertzel)
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
- [x] time scale modification (phase vocoder, PV with identity phase locking, WSOLA)
- [x] simple resampling, interpolation, decimation
- [x] bandlimited resampling
- [x] noise reduction (spectral subtraction, sciPy-style Wiener filtering)
- [x] envelope following
- [x] sound effects (delay, echo, tremolo, wahwah, phaser, vibrato, flanger, pitch shift, sound morphing, robotize, whisperize)
- [x] simple modulation/demodulation (AM, ring, FM, PM)
- [x] simple audio playback and recording

Planned:

- [ ] sound synthesis (wavetable, ADSR, etc.)
- [ ] more sound effects (reverb, chorus, etc.)
- [ ] more transforms (CQT, DWT, Mellin, Hartley, Haar, Hadamard)
- [ ] more operations (adaptive filtering, Gabor filter)


## Philosophy of NWaves

NWaves was initially intended for research, visualizing and teaching basics of DSP and sound programming. All algorithms are coded in C# as simple as possible and were first designed mostly for offline processing (now some online methods are also available). It doesn't mean, though, that the library could be used only in toy projects; yes, it's not written in C/C++ or Asm, but it's not that *very* slow for many purposes either.


## Quickstart

### Working with 1d signals using DiscreteSignal class

```C#

// Create signal { 0.75, 0.75, 0.75, 0.75, 0.75 } sampled at 8 kHz:

var constants = new DiscreteSignal(8000, 5, 0.75f);


// Create signal { 0.0, 1.0, 2.0, ..., 99.0 } sampled at 22050 Hz

var linear = new DiscreteSignal(22050, Enumerable.Range(0, 100));


// Create signal { 1.0, 0.0 } sampled at 800 Hz

var bits = new DiscreteSignal(800, new float [] { 1, 0 });


// Create one more signal from samples repeated 3 times

var samples = new [] { 0.5f, 0.2f, -0.3f, 1.2f, 1.6f, -1.8f, 0.3f, -0.2f };
var signal = new DiscreteSignal(16000, samples).Repeat(3);


// DiscreteSignal samples are mutable by design:

signal[2] = 1.27f;
signal[3] += 0.5f;


// slices (as in Python: "signal[6:18]")

var middle = signal[6, 18];

// specific slices:

var starting = signal.First(10);	// Python analog is 'signal[:10]'
var ending = signal.Last(10);		// Python analog is 'signal[-10:]'


// We can get the entire array of samples anytime
// (keeping in mind that it's mutable, i.e. it's not(!) IReadOnlyList)

var samples = signal.Samples;


// repeat signal 100 times {1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, ...}

var bitStream = bits.Repeat(100);


// delay by 1000 samples and -500 samples (ahead)

var delayed = signal1.Delay(1000);
var front = signal1.Delay(-500);


// concatenate signals

var concat = signal1.Concatenate(signal2);


// add signals element-wise 
// (sizes don't need to fit; broadcasting takes place)

var combination = signal1 + signal2;
// or
var combination = signal1.Superimpose(signal2);


// amplify / attenuate

bits.Amplify(10);		// in-place
bits.Attenuate(10);		// in-place

var bitStream = bits * 10;		// new signal


// add constant 0.5 to each sample

var offset = signal1 + 0.5f;


// make a deep copy of a signal

var copy = signal.Copy();

// equivalent to:

var copy = new DiscreteSignal(signal.SamplingRate, signal.Samples, allocateNew: true);

```

The ```DiscreteSignal``` class is a wrapper around array of floats, since for most purposes 32bit precision is sufficient and leads to better performance in terms of speed and memory usage. However, alternative versions of functions dealing with double arrays are also available. For instance, filter design and analysis is done with double precision and filtering is carried out with single precision by default. See more in tutorial (*coming soon*).


### Signal builders

```C#

DiscreteSignal sinusoid = 
	new SineBuilder()
		.SetParameter("amplitude", 1.2)
		.SetParameter("frequency", 500.0/*Hz*/)
		.OfLength(1000)
		.SampledAt(44100/*Hz*/)
		.Build();

DiscreteSignal noise = 
	new PinkNoiseBuilder()
		.SetParameter("min", -1.5)
		.SetParameter("max", 1.5)
		.OfLength(800)
		.SampledAt(44100)
		.DelayedBy(200)
		.Build();

DiscreteSignal noisy = 
	new SineBuilder()
		.SetParameter("amp", 3.0)
		.SetParameter("freq", 1200.0/*Hz*/)
		.SetParameter("phase", Math.PI/3)
		.OfLength(1000)
		.SampledAt(44100)
		.SuperimposedWith(noise)
		.Build();

```

Signal builders can also act as real-time generators of samples:

```C#

DiscreteSignal lfo = 
    new TriangleWaveBuilder()
        .SetParameter("low", 100)
        .SetParameter("high", 1500)
        .SetParameter("frequency", 2.0/*Hz*/)
        .SampledAt(16000/*Hz*/);

while (...)
{
    var sample = lfo.NextSample();
    //...
}

```


### Loading signals from wave files:

```C#

using (var stream = new FileStream("sample.wav", FileMode.Open))
{
	var waveFile = new WaveFile(stream);

	// address signals with Channels enum (Left, Right, Average, Interleave):

	var signalLeft = waveFile[Channels.Left];
	var signalRight = waveFile[Channels.Right];
	var signalAverage = waveFile[Channels.Average];
	var signalInterleaved = waveFile[Channels.Interleave];
	
	// or simply like this:

	signalLeft = waveFile.Signals[0];
	signalRight = waveFile.Signals[1];
}

```


### Saving signals to wave files:

```C#

using (var stream = new FileStream("saved.wav", FileMode.Create))
{
	var waveFile = new WaveFile(signal);
	waveFile.SaveTo(stream);
}

```


### Transforms:

```C#

// For each transform there's a corresponding transformer object.
// Each transformer object has Direct() and Inverse() methods.


// Complex FFT transformer:

var fft = new Fft(1024);


// 1) Handling complex arrays directly:

float[] real = signal.First(1024).Samples;
float[] imag = new float [1024];

// in-place FFT
fft.Direct(real, imag);

// ...do something with real and imaginary parts of the spectrum...

// in-place IFFT
fft.Inverse(real, imag);


// 2) Often we don't need to deal with complex arrays
//    and we don't want to transform samples in-place;
//    instead we need some real-valued post-processed results of complex fft:

var magnitudeSpectrum = 
    fft.MagnitudeSpectrum(signal[1000, 2024]);

var powerSpectrum = 
    fft.PowerSpectrum(signal.First(1024), normalize: false);

var logPowerSpectrum = 
    fft.PowerSpectrum(signal.Last(1024))
       .Samples
       .Select(s => Scale.ToDecibel(s))
       .ToArray();



// Cepstral transformer:

var ct = new CepstralTransform(20, fftSize: 512);
var cepstrum = ct.Direct(signal);


// Hilbert transformer

var ht = new HilbertTransform(1024);
var result = ht.Direct(doubleSamples);

// HilbertTransform class also provides method
// for computing complex analytic signal.
// Thus, previous line is equivalent to:

var result = ht.AnalyticSignal(doubleSamples).Imag;

// by default HilbertTransformer works with double precision;
// this code is for floats:
// var ht = new HilbertTransform(1024, doublePrecision: false);
// var result = ht.AnalyticSignal(floatSamples).Item2;


// in previous five cases the result of each transform was
// a newly created object of DiscreteSignal class.

// If the sequence of blocks must be processed then 
// it's better to work with reusable arrays in memory
// (all intermediate results will also be stored in reusable arrays):

var spectrum = new float[1024];
var cepstrum = new float[20];

fft.PowerSpectrum(signal[1000, 2024].Samples, spectrum);
// do something with spectrum

fft.PowerSpectrum(signal[2024, 3048].Samples, spectrum);
// do something with spectrum

fft.PowerSpectrum(signal[3048, 4072].Samples, spectrum);
// do something with spectrum

ct.Direct(signal[5000, 5512].Samples, cepstrum)
// do something with cepstrum

//...


// Short-Time Fourier Transform:

var stft = new Stft(1024, 512, WindowTypes.Hamming);
var timefreq = stft.Direct(signal);
var reconstructed = stft.Inverse(timefreq);

var spectrogram = stft.Spectrogram(4096, 1024);

```


### Operations:

```C#

// the following four operations are based on FFT convolution:

var filteredSignal = Operation.Convolve(signal, kernel);
var correlated = Operation.CrossCorrelate(signal1, signal2);

// block convolution (each block contains 4096 samples)

var olaFiltered = Operation.BlockConvolve(signal, kernel, 4096, FilteringMethod.OverlapAdd);
var olsFiltered = Operation.BlockConvolve(signal, kernel, 4096, FilteringMethod.OverlapSave);

// resampling:

var resampled = Operation.Resample(signal, 16000);
var decimated = Operation.Decimate(signal, 3);
var interpolated = Operation.Interpolate(signal, 4);

```


### Filters and effects:

```C#

var maFilter = new MovingAverageFilter(7);
var smoothedSignal = maFilter.ApplyTo(signal);

var frequency = 800.0/*Hz*/;
var notchFilter = new BiQuad.NotchFilter(frequency / signal.SamplingRate);
var notchedSignal = notchFilter.ApplyTo(signal);


// filter analysis:

var filter = new IirFilter(new [] {1, 0.5, 0.2}, new [] {1, -0.8, 0.3});

var impulseResponse = filter.ImpulseResponse();
var magnitudeResponse = filter.FrequencyResponse().Magnitude;
var phaseResponse = filter.FrequencyResponse().Phase;

var zeros = filter.Tf.Zeros;
var poles = filter.Tf.Poles;


// some filter design:

var firFilter = DesignFilter.Fir(43, magnitudeResponse);

var lowpassFilter = DesignFilter.FirLp(43, 0.12);
var highpassFilter = DesignFilter.LpToHp(lowpassFilter);

var kernel = lowpassFilter.Tf.Numerator;


// transfer function:

var transferFunction = lowPassFilter.Tf;

var b = transferFunction.Numerator;
var a = transferFunction.Denominator;
var zeros = transferFunction.Zeros;
var poles = transferFunction.Poles;

var gd = transferFunction.GroupDelay();
var pd = transferFunction.PhaseDelay();


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

var pitchShift = new PitchShiftEffect(1.2);
var wahwah = new WahWahEffect(lfoFrequency: 2/*Hz*/);

var processed = wahwah.ApplyTo(pitchShift.ApplyTo(signal));

```


### Online processing

Online processing is supported by all classes that implement the ```IOnlineFilter``` interface.
Currently, all filters, block convolvers (```OlaBlockConvolver```, ```OlsBlockConvolver```) and audio effects contain the ```Process(sample)``` and ```Process(buffer)``` methods responsible for online processing.

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

For another demo let's emulate the frame-by-frame online processing in a loop:

```C#

var frameSize = 128;

// big input array (we'll process it frame-by-frame in a loop)
var input = signal.Samples;

// big resulting array that will be filled more and more after processing each frame
var output = new float[input.Length];

for (int i = 0; i + frameSize < input.Length; i += frameSize)
{
	filter.Process(input, output, frameSize, inputPos: i, outputPos: i);
}

```

Block convolvers:

```C#

// Overlap-Add / Overlap-Save

FirFilter filter = new FirFilter(kernel);

var blockConvolver = OlaBlockConvolver.FromFilter(filter, 16384);

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

In case of block convolvers note that the output will always be "late" by ```FftSize-KernelSize+1``` samples.
The property ```(Ola|Ols)BlockConvoler.HopSize``` returns this value. So you might want to process first ```HopSize``` samples without storing the result anywhere (the samples will just get into delay line). For example, this is how offline method ```ApplyTo``` is implemented for block convolvers:

```C#

var firstCount = Math.Min(HopSize, signal.Length);

int i = 0, j = 0;

for (; i < firstCount; i++)    // first HopSize samples are just placed in the delay line
{
    Process(signal[i]);
}

var filtered = new float[signal.Length + _kernel.Length - 1];

for (; i < signal.Length; i++, j++)    // process
{
    filtered[j] = Process(signal[i]);
}

var lastCount = firstCount + _kernel.Length - 1;

for (i = 0; i < lastCount; i++, j++)    // get last 'late' samples
{
    filtered[j] = Process(0.0f);
}

```

See also OnlineDemoForm code.

![onlinedemo](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/onlinedemo.gif)


### Feature extractors

Highly customizable feature extractors are available for offline and online processing.

```C#

var sr = signal.SamplingRate;

var lpcExtractor = new LpcExtractor(sr, 16, frameSize: 0.032/*sec*/, hopSize: 0.015/*sec*/);
var lpcVectors = lpcExtractor.ComputeFrom(signal).Take(15);


var mfccExtractor = new MfccExtractor(sr, 13, filterbankSize: 24, preEmphasis: 0.95);
var mfccVectors = mfccExtractor.ParallelComputeFrom(signal);

/* equivalent to:

var mfccExtractor = new MfccExtractor(sr, 13, filterbankSize: 24);
var preEmphasis = new PreEmphasisFilter(0.95);
var mfccVectors = mfccExtractor.ParallelComputeFrom(preEmphasis.ApplyTo(signal));

*/

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

using (var csvFile = new FileStream("mfccs.csv", FileMode.Create))
{
	var serializer = new CsvFeatureSerializer(mfccVectors);
	await serializer.SerializeAsync(csvFile);
}

```


### Playing and recording (Windows only)

```MciAudioPlayer``` and ```MciAudioRecorder``` work only with Windows, since they use winmm.dll and MCI commands

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

IAudioRecorder = new MciAudioRecorder();

// ...in some event handler
recorder.StartRecording(16000);

// ...in some event handler
recorder.StopRecording("temp.wav");

```

Playing audio from buffers in memory is implied by design but it's not implemented in ```MciAudioPlayer```. If you want to use NWaves library alone, there's a possible workaround: in the calling code the signal can be saved to a temporary wave file, and then player can play this file.

```C#

// this won't work, unfortunately:

// await player.PlayAsync(signal);
// await player.PlayAsync(signal, 16000, 32000);


// looks not so cool, but at least it works:

// create temporary file
var filename = string.format("{0}.wav", Guid.NewGuid());
using (var stream = new FileStream(filename, FileMode.Create))
{
	var waveFile = new WaveFile(signal);
	waveFile.SaveTo(stream);
}

await player.PlayAsync(filename);

// cleanup temporary file
File.Delete(filename);

```

I have also included a very simple wrapper around ```System.Media.SoundPlayer``` - the ```MemoryStreamPlayer``` class. This class implements the same ```IAudioPlayer``` interface as ```MciAudioPlayer``` and can be used at Windows-client side. You can find it in DemoForms project.

```C#

// use MemoryStreamPlayer class:

var player = new MemoryStreamPlayer();
await player.PlayAsync(signal);

```

### Demos

![filters](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/Filters.png)

![pitch](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/pitch.png)

![winforms](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/WinForms.png)

![lpc](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/lpc.png)

![mfcc](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/mfcc.png)

![spectral](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/spectral.png)

![effects](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/effects.png)