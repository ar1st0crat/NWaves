using NWaves.Filters;
using NWaves.Signals;
using NWaves.Transforms;
using System;
using System.Collections.Generic;

namespace NWaves.Operations
{
    /// <summary>
    /// HPS based on median filtering.
    /// 
    /// D.Fitzgerald. Harmonic/percussive separation using median filtering.
    /// 13th International Conference on Digital Audio Effects (DAFX10), Graz, Austria, 2010.
    /// </summary>
    public class HarmonicPercussiveSeparator
    {
        /// <summary>
        /// Internal STFT transformer
        /// </summary>
        private readonly Stft _stft;

        /// <summary>
        /// Masking function
        /// </summary>
        private readonly Func<float, float, float> _mask;

        /// <summary>
        /// Median filter for time axis
        /// </summary>
        private readonly MedianFilter2 _medianHarmonic;

        /// <summary>
        /// Median filter for frequency axis
        /// </summary>
        private readonly MedianFilter2 _medianPercussive;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fftSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="harmonicWinSize"></param>
        /// <param name="percussiveWinSize"></param>
        /// <param name="masking"></param>
        public HarmonicPercussiveSeparator(int fftSize = 2048,
                                           int hopSize = 512,
                                           int harmonicWinSize = 17,
                                           int percussiveWinSize = 17,
                                           HpsMasking masking = HpsMasking.WienerOrder2)
        {
            _stft = new Stft(fftSize, hopSize);

            _medianHarmonic = new MedianFilter2(harmonicWinSize);
            _medianPercussive = new MedianFilter2(percussiveWinSize);

            switch (masking)
            {
                case HpsMasking.Binary:
                    _mask = BinaryMask;
                    break;
                case HpsMasking.WienerOrder1:
                    _mask = WienerMask1;
                    break;
                default:
                    _mask = WienerMask2;
                    break;
            }
        }

        /// <summary>
        /// Evaluate harmonic and percussive mag-phase spectrograms from given signal.
        /// Both spectrogram objects share the same phase array.
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public (MagnitudePhaseList, MagnitudePhaseList) EvaluateSpectrograms(DiscreteSignal signal)
        {
            // spectrogram memory will be reused for harmonic magnitudes

            var harmonicSpectrogram = _stft.MagnitudePhaseSpectrogram(signal);
            var harmonicMagnitudes = harmonicSpectrogram.Magnitudes;

            // median filtering along frequency axis:

            var percussiveMagnitudes = new List<float[]>(harmonicMagnitudes.Count);

            for (var i = 0; i < harmonicMagnitudes.Count; i++)
            {
                var mag = new DiscreteSignal(1, harmonicMagnitudes[i]);
                percussiveMagnitudes.Add(_medianPercussive.ApplyTo(mag).Samples);
                _medianPercussive.Reset();
            }

            // median filtering along time axis:

            for (var j = 0; j <= _stft.Size / 2; j++)
            {
                int i = 0, k = 0;

                for (; k < _medianHarmonic.Size / 2; k++)    // feed first Size/2 samples
                {
                    _medianHarmonic.Process(harmonicMagnitudes[k][j]);
                }

                for (; i < harmonicMagnitudes.Count - _medianHarmonic.Size / 2; i++, k++)
                {
                    var h = _medianHarmonic.Process(harmonicMagnitudes[k][j]);

                    harmonicMagnitudes[i][j] *= _mask(h, percussiveMagnitudes[k][j]);
                    percussiveMagnitudes[i][j] *= _mask(percussiveMagnitudes[k][j], h);
                }

                for (k = 0; k < _medianHarmonic.Size / 2; i++, k++)     // don't forget last samples
                {
                    var h = _medianHarmonic.Process(0);

                    harmonicMagnitudes[i][j] *= _mask(h, percussiveMagnitudes[i][j]);
                    percussiveMagnitudes[i][j] *= _mask(percussiveMagnitudes[i][j], h);
                }

                _medianHarmonic.Reset();
            }

            var percussiveSpectrogram = new MagnitudePhaseList
            {
                Magnitudes = percussiveMagnitudes,
                Phases = harmonicSpectrogram.Phases
            };

            return (harmonicSpectrogram, percussiveSpectrogram);
        }

        /// <summary>
        /// Evaluate harmonic and percussive signals from given signal
        /// </summary>
        /// <param name="signal"></param>
        /// <returns>Harmonic signal and percussive signal</returns>
        public (DiscreteSignal, DiscreteSignal) EvaluateSignals(DiscreteSignal signal)
        {
            var (harmonicSpectrogram, percussiveSpectrogram) = EvaluateSpectrograms(signal);

            // reconstruct harmonic part:

            var harmonic = new DiscreteSignal(signal.SamplingRate, _stft.ReconstructMagnitudePhase(harmonicSpectrogram));

            // reconstruct percussive part:

            var percussive = new DiscreteSignal(signal.SamplingRate, _stft.ReconstructMagnitudePhase(percussiveSpectrogram));

            return (harmonic, percussive);
        }

        private float BinaryMask(float h, float p) => h > p ? 1 : 0;

        private float WienerMask1(float h, float p) => h + p > 1e-10 ? h / (h + p) : 0;

        private float WienerMask2(float h, float p) => h + p > 1e-10 ? h * h / (h * h + p * p) : 0;
    }

    /// <summary>
    /// Masking mode for HPS algorithm
    /// </summary>
    public enum HpsMasking
    {
        Binary,
        WienerOrder1,
        WienerOrder2
    }
}
