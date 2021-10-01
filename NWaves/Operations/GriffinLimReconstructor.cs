using NWaves.Transforms;
using NWaves.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Operations
{
    /// <summary>
    /// Reconstructs signal from a power (or magnitude) spectrogram using Griffin-Lim iterative algorithm.
    /// </summary>
    public class GriffinLimReconstructor
    {
        /// <summary>
        /// STFT transformer.
        /// </summary>
        private readonly Stft _stft;

        /// <summary>
        /// Magnitude part of the spectrogram.
        /// </summary>
        private readonly List<float[]> _magnitudes;

        /// <summary>
        /// Get or sets magnitude gain factor.
        /// </summary>
        public float Gain { get; set; } = 16;

        /// <summary>
        /// Constructs <see cref="GriffinLimReconstructor"/> for iterative signal reconstruction from <paramref name="spectrogram"/>.
        /// </summary>
        /// <param name="spectrogram">Spectrogram (list of spectra)</param>
        /// <param name="windowSize">Window size fro STFT</param>
        /// <param name="hopSize">Hop size for STFT</param>
        /// <param name="window">Window for STFT</param>
        /// <param name="power">Power (2 - Power spectra, otherwise - Magnitude spectra)</param>
        public GriffinLimReconstructor(List<float[]> spectrogram, int windowSize = 1024, int hopSize = 256, WindowType window = WindowType.Hann, int power = 2)
            : this(spectrogram, new Stft(windowSize, hopSize, window), power)
        {
        }

        /// <summary>
        /// Constructs <see cref="GriffinLimReconstructor"/> for iterative signal reconstruction from <paramref name="spectrogram"/>.
        /// </summary>
        /// <param name="spectrogram">Spectrogram (list of spectra)</param>
        /// <param name="stft">STFT transformer</param>
        /// <param name="power">Power (2 - Power spectra, otherwise - Magnitude spectra)</param>
        public GriffinLimReconstructor(List<float[]> spectrogram, Stft stft, int power = 2)
        {
            _stft = stft;

            _magnitudes = spectrogram;

            if (power == 2)
            {
                for (var i = 0; i < _magnitudes.Count; i++)
                {
                    for (var j = 0; j < _magnitudes[i].Length; j++)
                    {
                        _magnitudes[i][j] = (float)Math.Sqrt(_magnitudes[i][j]);
                    }
                }
            }

            for (var i = 0; i < _magnitudes.Count; i++)
            {
                for (var j = 0; j < _magnitudes[i].Length; j++)
                {
                    _magnitudes[i][j] *= Gain;
                }
            }
        }

        /// <summary>
        /// Does one iteration of reconstruction and returns reconstructed signal at current step.
        /// </summary>
        /// <param name="signal">Signal reconstructed at previous iteration</param>
        public float[] Iterate(float[] signal = null)
        {
            var magPhase = new MagnitudePhaseList() { Magnitudes = _magnitudes };

            if (signal is null)
            {
                var spectrumSize = _magnitudes[0].Length;

                var r = new Random();
                var randomPhases = new List<float[]>();

                for (var i = 0; i < _magnitudes.Count; i++)
                {
                    randomPhases.Add(Enumerable.Range(0, spectrumSize)
                                               .Select(s => (float)(2 * Math.PI * r.NextDouble()))
                                               .ToArray());
                }

                magPhase.Phases = randomPhases;
            }
            else
            {
                magPhase.Phases = _stft.MagnitudePhaseSpectrogram(signal).Phases;
            }

            return _stft.ReconstructMagnitudePhase(magPhase);
        }

        /// <summary>
        /// Reconstructs signal from spectrogram iteratively.
        /// </summary>
        /// <param name="iterations">Number of iterations in Griffin-Lim algorithm</param>
        public float[] Reconstruct(int iterations = 20)
        {
            var reconstructed = Iterate();

            for (var i = 0; i < iterations - 1; i++)
            {
                reconstructed = Iterate(reconstructed);
            }

            return reconstructed;
        }
    }
}
