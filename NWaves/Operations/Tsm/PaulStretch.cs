using System;

namespace NWaves.Operations.Tsm
{
    /// <summary>
    /// Represents TSM processor based on Paul stretch algorithm.
    /// </summary>
    class PaulStretch : PhaseVocoder
    {
        /// <summary>
        /// Randomizer for phases.
        /// </summary>
        private readonly Random _rand = new Random();

        /// <summary>
        /// Constructs <see cref="PaulStretch"/>.
        /// </summary>
        /// <param name="stretch">Stretch ratio</param>
        /// <param name="hopAnalysis">Hop length at analysis stage</param>
        /// <param name="fftSize">FFT size</param>
        public PaulStretch(double stretch, int hopAnalysis, int fftSize = 0) : base(stretch, hopAnalysis, fftSize)
        {
        }

        /// <summary>
        /// Processes spectrum at each STFT step: simply randomizes phases.
        /// </summary>
        protected override void ProcessSpectrum()
        {
            for (var j = 1; j <= _fftSize / 2; j++)
            {
                var mag = Math.Sqrt(_re[j] * _re[j] + _im[j] * _im[j]);
                var phase = 2 * Math.PI * _rand.NextDouble();

                _re[j] = (float)(mag * Math.Cos(phase));
                _im[j] = (float)(mag * Math.Sin(phase));
            }
        }

        /// <summary>
        /// Resets TSM processor.
        /// </summary>
        public override void Reset()
        {
        }
    }
}
