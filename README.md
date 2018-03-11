# NWaves

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

![logo](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/logo_draft.bmp)

NWaves is a .NET library for 1d signal processing focused specifically on audio processing.

## Main features 

Already available:

- [x] major DSP transforms (FFT, DCT, STFT, Hilbert, cepstral)
- [x] basic LTI digital filters (FIR, IIR, comb, moving average, pre/de-emphasis, DC removal)
- [x] BiQuad filters (low-pass, high-pass, band-pass, notch, all-pass, peaking, shelving)
- [x] 1-pole filters (low-pass, high-pass)
- [x] basic operations (convolution, cross-correlation, rectification, envelope detection, resampling, time stretching)
- [x] block convolution (overlap-add, overlap-save)
- [x] modulation (AM, ring, FM, PM)
- [x] basic filter design & analysis (zeros and poles, window method, HP from/to LP, combining filters)
- [x] non-linear filters (median filter, overdrive and distortion effects)
- [x] windowing functions (Hamming, Blackman, Hann, cepstral liftering)
- [x] psychoacoustic filter banks (Mel, Bark, Critical Bands, ERB) and perceptual weighting (A, B, C)
- [x] feature extraction (MFCC, PNCC and SPNCC, LPC, LPCC, modulation spectra) and CSV serialization
- [x] feature post-processing (CMN, deltas)
- [x] spectral features (centroid, spread, flatness, bandwidth, rolloff, contrast, crest)
- [x] sound synthesis and signal builders (sinusoid, white/pink/red/grey noise, triangle, sawtooth, square, periodic pulse)
- [x] time-domain characteristics (rms, energy, zero-crossing rate, entropy)
- [x] pitch tracking
- [x] sound effects (delay, echo, tremolo, wahwah, phaser, distortion, pitch shift)
- [x] simple audio playback and recording (Windows only)

Planned:

- [ ] more transforms (CQT, DWT, Mellin, Haar, Hadamard)
- [ ] more operations (spectral subtraction, adaptive filtering)
- [ ] more feature extraction (MIR descriptors and lots of others)
- [ ] more sound synthesis (ADSR, etc.)
- [ ] more sound effects (Reverb, Vibrato, Chorus, Flanger, etc.)


## Philosophy of NWaves

NWaves was initially intended for research, visualizing and teaching basics of DSP and sound programming. All algorithms are coded in C# as simple as possible and designed mostly for offline processing. It doesn't mean, though, that the library could be used only in toy projects; yes, it's not written in C++ or Asm, but it's not that *very* slow for many purposes either.

In the beginning... there were interfaces and factories here and there, and NWaves was modern-OOD-fashioned library. Now NWaves is more like a bunch of DSP models and methods gathered in separate classes, so that one wouldn't get lost in object-oriented labyrinths.

## Quickstart

### Working with 1d signals using DiscreteSignal class

```C#

// Create signal {0.75, 0.75, 0.75, 0.75, 0.75} sampled at 8 kHz:
var constants = new DiscreteSignal(8000, 5, 0.75);


// Create signal {0.0, 1.0, 2.0, ..., 99.0} sampled at 22050 Hz
var linear = new DiscreteSignal(22050, Enumerable.Range(0, 100));


// Create signal {1.0, 0.0} sampled at 44,1 kHz
var bits = new DiscreteSignal(44100, new double [] { 1, 0 });


// Create one more signal from samples repeated 3 times
var samples = new double[] { 0.5, 0.2, -0.3, 1.2, 1.6, -1.8, 0.3, -0.2 };

var signal = new DiscreteSignal(16000, samples).Repeat(3);


// DiscreteSignal samples are mutable by design:

signal[2] = 1.27;
signal[3] += 0.5;


// slices (as in Python: "signal[6:18]")

var middle = signal[6, 18];

// specific slices:

var starting = signal.First(10);	// Python analog is 'signal[:10]'
var ending = signal.Last(10);		// Python analog is 'signal[-10:]'


// We can get the entire array of samples anytime
// (keeping in mind that it's mutable, i.e. it's not(!) IReadOnlyList)

var samples = signal.Samples;


// repeat signal 100 times {1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, ...}

var bitStream = bits * 100;
// or
var bitStream = bits.Repeat(100);


// concatenate signals

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


// make a deep copy of a signal
var copy = signal.Copy();

// equivalent to:
var copy = new DiscreteSignal(signal.SamplingRate, signal.Samples, allocateNew: true);

```


### Signal builders

```C#

DiscreteSignal sinusoid = 
	new SinusoidBuilder()
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
	new SinusoidBuilder()
		.SetParameter("amp", 3.0)
		.SetParameter("freq", 1200.0/*Hz*/)
		.SetParameter("phase", Math.PI/3)
		.OfLength(1000)
		.SampledAt(44100)
		.SuperimposedWith(noise)
		.Build();

```


### Loading signals from wave files:

```C#

using (var stream = new FileStream("sample.wav", FileMode.Open))
{
	var waveFile = new WaveFile(stream);

	// address signals with Channels enum (Left, Right, Interleave):

	var signalLeft = waveFile[Channels.Left];
	var signalRight = waveFile[Channels.Right];
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

double[] real = signal.First(1024).Samples;
double[] imag = new double [1024];

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
var result = ht.Direct(signal);

// HilbertTransform class also provides method
// for computing complex analytic signal.
// Thus, previous line is equivalent to:

var result = ht.AnalyticSignal(signal).Imag;


// in previous five cases the result of each transform was
// a newly created object of DiscreteSignal class.

// If the sequence of blocks must be processed then 
// it's better to work with reusable arrays in memory:

var spectrum = new double[1024];
var cepstrum = new double[20];

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

var olaFiltered = Operation.OverlapAdd(signal, kernel, 4096);
var olsFiltered = Operation.OverlapSave(signal, kernel, 4096);


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

var filter = new IirFilter(new [] {1.0, 0.5, 0.2}, new [] {1.0, -0.8, 0.3});

var impulseResponse = filter.ImpulseResponse();
var magnitudeResponse = filter.FrequencyResponse().Magnitude;
var phaseResponse = filter.FrequencyResponse().Phase;

var zeros = filter.Zeros;
var poles = filter.Poles;


// some filter design:

var firFilter = DesignFilter.Fir(43, magnitudeResponse);

var lowpassFilter = DesignFilter.FirLp(43, 0.12);
var highpassFilter = DesignFilter.LpToHp(lowpassFilter);


// sequence of filters:

var cascade = filter * firFilter * notchFilter;
var filtered = cascade.ApplyTo(signal);

// equivalent to:

var filtered = filter.ApplyTo(signal);
filtered = firFilter.ApplyTo(filtered);
filtered = notchFilter.ApplyTo(filtered);


var pitchShift = new PitchShiftEffect(1.2);
var wahwah = new WahWahEffect(lfoFrequency: 2/*Hz*/);

var processed = wahwah.ApplyTo(pitchShift.ApplyTo(signal));

```


### Feature extractors

```C#

var lpcExtractor = new LpcExtractor(16, windowSize: 0.032, hopSize: 0.015);
var lpcVectors = lpcExtractor.ComputeFrom(signal);

var mfccExtractor = new MfccExtractor(13, melFilterbanks: 24, preEmphasis: 0.95);
var mfccVectors = mfccExtractor.ComputeFrom(signal).Take(15);

var pnccExtractor = new PnccExtractor(13);
var pnccVectors = pnccExtractor.ComputeFrom(signal.First(10000));
FeaturePostProcessing.NormalizeMean(pnccVectors);

using (var csvFile = new FileStream("mfccs.csv", FileMode.Create))
{
	var serializer = new CsvFeatureSerializer(mfccVectors);
	await serializer.SerializeAsync(csvFile);
}

```


### Playing and recording (Windows only)

MciAudioPlayer and MciAudioRecorder work only with Windows, since they use winmm.dll and MCI commands

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

Playing audio from buffers in memory is implied by design but it's not implemented in MciAudioPlayer so far. Still there's some workaround: in the calling code the signal can be saved to a temporary wave file, and then player can play this file.

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


### Demos

![filters](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/Filters.png)

![pitch](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/pitch.png)

![winforms](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/WinForms.png)

![lpc](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/lpc.png)

![mfcc](https://github.com/ar1st0crat/NWaves/blob/master/screenshots/mfcc.png)
