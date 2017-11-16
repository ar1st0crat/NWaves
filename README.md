# NWaves

NWaves is a .NET library for 1d signal processing focused specifically on audio processing.

## Main features 

Already available:

- [x] major DSP transforms (FFT, DCT, STFT)
- [x] basic LTI digital filters (FIR, IIR, moving average (non-recursive and recursive), pre-emphasis)
- [x] basic operations and filtering (convolution/deconvolution, cross-correlation, overlap-add, overlap-save)
- [x] simple filter design & analysis (zeros and poles, window method)
- [x] BiQuad filters (low-pass, high-pass, band-pass, notch, all-pass, peaking, shelving)
- [x] 1-pole filters (low-pass, high-pass)
- [x] median filter
- [x] windowing functions (Hamming, Blackman, Hann, cepstral liftering)
- [x] psychoacoustic filter banks (Mel, Bark, Critical Bands, ERB)
- [x] feature extraction (MFCC, PNCC and SPNCC, LPC, LPCC, modulation spectra) and post-processing (CMN, deltas)
- [x] sound synthesis and signal builders (sinusoid, white/pink/red noise, triangle, sawtooth, square, periodic pulse)
- [x] simple audio playback and recording (Windows only)

Planned:

- [ ] more transforms (CQT, DWT, Mellin, Hilbert, Haar, Hadamard)
- [ ] more operations (resampling, spectral subtraction, adaptive filtering)
- [ ] more feature extraction (MPEG7 descriptors and lots of others)
- [ ] more sound synthesis (ADSR, etc.)
- [ ] sound effects (WahWah, Reverb, Vibrato, Chorus, Flanger, PitchShift, etc.)


## Philosophy of NWaves

NWaves was initially intended for research, visualizations and teaching basics of DSP and sound programming. All algorithms are coded in C# as simple as possible and designed mostly for offline processing. Perhaps, in the future I'll work on optimized versions and add them to the project separately.

In the beginning... there were interfaces and factories here and there, and NWaves was modern-OOD-fashioned library. Now NWaves is more like a bunch of DSP models and methods gathered in static classes, so that one wouldn't get lost in object-oriented labyrinths. Although you may suddenly find a little bit of fluent syntax (e.g., SignalBuilders) and some Strategy patterns (e.g. FeatureExtractor) in the project.

## Quickstart

### Working with 1d signals using DiscreteSignal class

```C#

var constants = new DiscreteSignal(8000, 10, 0.75);
// {0.75, 0.75, 0.75, 0.75, 0.75, 0.75, 0.75, 0.75, 0.75, 0.75} sampled at 8 kHz

var linear = new DiscreteSignal(22050, Enumerable.Range(0, 100));
// {0.0, 1.0, 2.0, ..., 99.0} sampled at 22050 Hz

var bits = new DiscreteSignal(44100, new double [] {1, 0});
// {1.0, 0.0}


// repeat signal 100 times {1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, ...}

var bitStream = bits * 100;
// or
var bitStream = signal.Repeat(100);


// Samples in DiscreteSignals are mutable by design:

var samples = new double[] {0.5, 0.2, -0.3, 1.2, 1.6, -1.8, 0.3, -0.2};

var signal = new DiscreteSignal(16000, samples).Repeat(3);

signal[2] = 1.27;
signal[3] += 0.5;

var sample = signal[10];

// slices (as in Python: "signal[500:1000]")

var middle = signal[500, 1000];


// specific slices:

var initialSignal = signal.First(100);
var endingSignal = signal.Last(100);



// You can get all samples as an entire array of doubles
// (be careful with mutability! It's not IReadOnlyList)

var samples = signal.Samples;


// concatenate

var concat = signal1 + signal2;
// or
var concat = signal1.Concatenate(signal2);


// add signals element-wise 
// (sizes don't need to fit; broadcasting takes place)

var combination = signal1.Superimpose(signal2);


// delay
var delayed = signal1.Delay(1000);
// or
var delayed = signal1 + 1000;


// make a copy
var copy = signal.Copy();

// equivalent to:
var copy = new Signal(signal.SamplingRate, signal.Samples)

```


### Signal builders

```C#

DiscreteSignal sinusoid = 
	new SinusoidBuilder()
		.SetParameter("amplitude", 1.2)
		.SetParameter("frequency", 0.25)
		.OfLength(1000)
		.SampledAt(44100)
		.Build();

DiscreteSignal noise = 
	new WhiteNoiseBuilder()
		.SetParameter("min", -0.5)
		.SetParameter("max", 0.5)
		.OfLength(800)
		.SampledAt(44100)
		.DelayedBy(200)
		.Build();

DiscreteSignal noisy = 
	new SinusoidBuilder()
		.SetParameter("amp", 3.0)
		.SetParameter("freq", 0.12)
		.SetParameter("phase", Math.PI/3)
		.OfLength(1000)
		.SampledAt(44100)
		.SuperimposedWith(noise)
		.Build();

```


### Loading/saving signals from/in wave files:

```C#

// load

using (var stream = new FileStream("sample.wav", FileMode.Open))
{
	IAudioContainer waveFile = new WaveFile(stream);

	// address with enum (Left, Right, Interleave):

	var signalLeft = waveFile[Channels.Left];
	var signalRight = waveFile[Channels.Right];
	var signalInterleaved = waveFile[Channels.Interleave];

	// or simply like this:

	var signal = waveFile.Signals[0];
	var signalRight = waveFile.Signals[1];
}


// save

using (var stream = new FileStream(@"saved.wav", FileMode.Create))
{
	var waveFile = new WaveFile(signal);
	waveFile.SaveTo(stream);
}

```


### Playing and recording (Windows only)

MciAudioPlayer and MciAudioRecorder work only with Windows, since they use winmm.dll and MCI commands

```C#

IAudioPlayer player = new MciAudioPlayer();
player.Volume = 0.4f;

// play entire file
await player.PlayAsync("temp.wav");

// play from 16000th sample to 32000th sample
await player.PlayAsync("temp.wav", 16000, 32000);


// in some event handler
player.Pause();

// in some event handler
player.Resume();

// in some event handler
player.Stop();


// recording

IAudioRecorder = new MciAudioRecorder();

// in some event handler
recorder.StartRecording(16000);

// in some event handler
recorder.StopRecording("temp.wav");

```

Playing audio from buffers in memory is implied by design but it's not implemented in MciAudioPlayer (seems that it's simply impossible to do...). Still there's some workaround: in the calling code the signal can be saved to a temporary wave file, and then player can play this file.

```C#

// this won't work:

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

### Transforms:

```C#

var spectrogram = Transform.Stft(signal, 512, 256, WindowTypes.Hamming);

var spectrum = Transform.MagnitudeSpectrum(signal[1000, 1512].Samples);
var spectrum = Transform.PowerSpectrum(signal.Samples, fftSize: 512, normalize: false);
var spectrum = Transform.LogPowerSpectrum(samples, 1024);

var cepstrum = Transform.Cepstrum(signal.Samples, 20);

```


### Operations:

```C#

var filteredSignal = Operation.Convolve(signal, kernel);

var correlated = Operation.CrossCorrelate(signal1, signal2);

var deconvolved = Operation.Deconvolve(filteredSignal, kernel);

// TODO:

var resampled = Operation.Resample(signal, 22050);
var decimated = Operation.Decimate(signal, 3);

```


### Filters and effects (that are filters as well):

```C#

var filter = new MovingAverageFilter(7);
var filteredSignal = filter.ApplyTo(signal);

// TODO:

var filtered = signal.ApplyFilter(filter.CombineWith(new Reverb(params))
                                        .CombineWith(new FirFilter(coeffs)));

var distortion = new DistortionEffect();
var echo = new EchoEffect(delay: 20);
var reverb = new ReverbEffect(1.9f);

var filtered = signal.ApplyFilter(distortion + echo + reverb);

var freqz = filter.Freqz();

```


### Feature extractors

```C#

var lpcExtractor = new LpcFeatureExtractor(16, signal.SamplingRate, windowSize: 0.032, overlapSize: 0.015);
var lpcVectors = lpcExtractor.ComputeFrom(signal);

var mfccExtractor = new MfccFeatureExtractor(13, signal.SamplingRate, melFilterbanks: 24, preEmphasis: 0.95);
var mfccVectors = mfccExtractor.ComputeFrom(signal).Take(15);
FeaturePostProcessing.NormalizeMean(mfccVectors);

```

### Demos

![filters](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/Filters.png)

![winforms](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/WinForms.png)

![lpc](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/lpc.png)

![mfcc](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/mfcc.png)
